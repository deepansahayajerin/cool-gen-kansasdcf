// Program: SC_CAB_SET_UP_NEXT_TRAN, ID: 371453011, model: 746.
// Short name: SWE01080
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// <para>
/// A program: SC_CAB_SET_UP_NEXT_TRAN.
/// </para>
/// <para>
/// RESP: SECUR
/// </para>
/// </summary>
[Serializable]
public partial class ScCabSetUpNextTran: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CAB_SET_UP_NEXT_TRAN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCabSetUpNextTran(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCabSetUpNextTran.
  /// </summary>
  public ScCabSetUpNextTran(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------
    // MAINTENANCE LOG:
    // 04/29/97	JF. Caillouet	Change Current Date
    // ----------------------------------------------------------------
    local.Current.Date = Now().Date;

    if (!ReadTransaction())
    {
      ExitState = "SC0002_SCREEN_ID_NF";

      return;
    }

    if (Equal(entities.ExistingTransaction.Trancode, global.TranCode))
    {
      ExitState = "SC0029_CANNOT_NEXT_TRAN_TO_SELF";

      return;
    }

    if (AsChar(entities.ExistingTransaction.NextTranAuthorization) == 'N')
    {
      ExitState = "SC0041_NEXT_TRAN_NOT_ALLOWED";

      return;
    }

    if (AsChar(entities.ExistingTransaction.MenuInd) == 'Y')
    {
      // ALL MENUES ARE BY DEFAULT AUTHORIZED TO ALL USERS.
      local.Auth.Flag = "Y";
    }
    else
    {
      // now determine what environment we are in,  the desctription will 
      // contain the answer
      // 	PROD - procuction environment			verify security
      // 	TEST - Test environment
      // 		allow all action to be 			performed. this is so that
      // 		security does not need to be 		setup for each developer.
      // 			
      if (!ReadProfile2())
      {
        ExitState = "SC0025_KEY_NF_RB";

        return;
      }

      if (Equal(entities.ExistingProfile.Desc, "TEST"))
      {
        // we are in the test environment. let the developer go to the next 
        // transaction.
        local.Auth.Flag = "Y";
      }
      else if (Equal(entities.ExistingProfile.Desc, "PROD"))
      {
        // we are in the production environment. get all of the users security.
        local.Auth.Flag = "N";

        if (!ReadProfile1())
        {
          ExitState = "SC0001_USER_NOT_AUTH_TRAN";

          goto Test;
        }

        if (ReadProfileAuthorization())
        {
          local.Auth.Flag = "Y";
        }
      }
    }

Test:

    if (AsChar(local.Auth.Flag) == 'N')
    {
      ExitState = "SC0001_USER_NOT_AUTH_TRAN";

      return;
    }

    if (ReadNextTranInfo())
    {
      try
      {
        UpdateNextTranInfo();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SC0000_NEXT_TRAN_INFO_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      if (!ReadServiceProvider())
      {
        ExitState = "SC0005_USER_NF_RB";

        return;
      }

      try
      {
        CreateNextTranInfo();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SC0000_NEXT_TRAN_INFO_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    global.NextTran = entities.ExistingTransaction.Trancode + " " + "XXNEXTXX";
  }

  private void CreateNextTranInfo()
  {
    var legalActionIdentifier =
      import.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();
    var courtCaseNumber = import.NextTranInfo.CourtCaseNumber ?? "";
    var caseNumber = import.NextTranInfo.CaseNumber ?? "";
    var csePersonNumber = import.NextTranInfo.CsePersonNumber ?? "";
    var csePersonNumberAp = import.NextTranInfo.CsePersonNumberAp ?? "";
    var csePersonNumberObligee = import.NextTranInfo.CsePersonNumberObligee ?? ""
      ;
    var csePersonNumberObligor = import.NextTranInfo.CsePersonNumberObligor ?? ""
      ;
    var courtOrderNumber = import.NextTranInfo.CourtOrderNumber ?? "";
    var obligationId = import.NextTranInfo.ObligationId.GetValueOrDefault();
    var standardCrtOrdNumber = import.NextTranInfo.StandardCrtOrdNumber ?? "";
    var infrastructureId =
      import.NextTranInfo.InfrastructureId.GetValueOrDefault();
    var miscText1 = import.NextTranInfo.MiscText1 ?? "";
    var miscText2 = import.NextTranInfo.MiscText2 ?? "";
    var miscNum1 = import.NextTranInfo.MiscNum1.GetValueOrDefault();
    var miscNum2 = import.NextTranInfo.MiscNum2.GetValueOrDefault();
    var miscNum1V2 = import.NextTranInfo.MiscNum1V2.GetValueOrDefault();
    var miscNum2V2 = import.NextTranInfo.MiscNum2V2.GetValueOrDefault();
    var ospId = entities.ExistingServiceProvider.SystemGeneratedId;

    entities.ExistingNextTranInfo.Populated = false;
    Update("CreateNextTranInfo",
      (db, command) =>
      {
        db.SetNullableString(command, "lastTran", "");
        db.SetNullableInt32(command, "legalActionIdent", legalActionIdentifier);
        db.SetNullableString(command, "courtCaseNumber", courtCaseNumber);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "csePersonNumber", csePersonNumber);
        db.SetNullableString(command, "csePersonNumber0", csePersonNumberAp);
        db.
          SetNullableString(command, "csePersonNumber1", csePersonNumberObligee);
          
        db.
          SetNullableString(command, "csePersonNumber2", csePersonNumberObligor);
          
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableInt32(command, "obligationId", obligationId);
        db.SetNullableString(command, "stdCrtOrdNbr", standardCrtOrdNumber);
        db.SetNullableInt32(command, "planTaskId", infrastructureId);
        db.SetNullableString(command, "miscText1", miscText1);
        db.SetNullableString(command, "miscText2", miscText2);
        db.SetNullableInt64(command, "miscNum1", miscNum1);
        db.SetNullableInt64(command, "miscNum2", miscNum2);
        db.SetNullableDecimal(command, "miscNum1V2", miscNum1V2);
        db.SetNullableDecimal(command, "miscNum2V2", miscNum2V2);
        db.SetInt32(command, "ospId", ospId);
      });

    entities.ExistingNextTranInfo.LastTran = "";
    entities.ExistingNextTranInfo.LegalActionIdentifier = legalActionIdentifier;
    entities.ExistingNextTranInfo.CourtCaseNumber = courtCaseNumber;
    entities.ExistingNextTranInfo.CaseNumber = caseNumber;
    entities.ExistingNextTranInfo.CsePersonNumber = csePersonNumber;
    entities.ExistingNextTranInfo.CsePersonNumberAp = csePersonNumberAp;
    entities.ExistingNextTranInfo.CsePersonNumberObligee =
      csePersonNumberObligee;
    entities.ExistingNextTranInfo.CsePersonNumberObligor =
      csePersonNumberObligor;
    entities.ExistingNextTranInfo.CourtOrderNumber = courtOrderNumber;
    entities.ExistingNextTranInfo.ObligationId = obligationId;
    entities.ExistingNextTranInfo.StandardCrtOrdNumber = standardCrtOrdNumber;
    entities.ExistingNextTranInfo.InfrastructureId = infrastructureId;
    entities.ExistingNextTranInfo.MiscText1 = miscText1;
    entities.ExistingNextTranInfo.MiscText2 = miscText2;
    entities.ExistingNextTranInfo.MiscNum1 = miscNum1;
    entities.ExistingNextTranInfo.MiscNum2 = miscNum2;
    entities.ExistingNextTranInfo.MiscNum1V2 = miscNum1V2;
    entities.ExistingNextTranInfo.MiscNum2V2 = miscNum2V2;
    entities.ExistingNextTranInfo.OspId = ospId;
    entities.ExistingNextTranInfo.Populated = true;
  }

  private bool ReadNextTranInfo()
  {
    entities.ExistingNextTranInfo.Populated = false;

    return Read("ReadNextTranInfo",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingNextTranInfo.LastTran =
          db.GetNullableString(reader, 0);
        entities.ExistingNextTranInfo.LegalActionIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.ExistingNextTranInfo.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingNextTranInfo.CaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingNextTranInfo.CsePersonNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingNextTranInfo.CsePersonNumberAp =
          db.GetNullableString(reader, 5);
        entities.ExistingNextTranInfo.CsePersonNumberObligee =
          db.GetNullableString(reader, 6);
        entities.ExistingNextTranInfo.CsePersonNumberObligor =
          db.GetNullableString(reader, 7);
        entities.ExistingNextTranInfo.CourtOrderNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingNextTranInfo.ObligationId =
          db.GetNullableInt32(reader, 9);
        entities.ExistingNextTranInfo.StandardCrtOrdNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingNextTranInfo.InfrastructureId =
          db.GetNullableInt32(reader, 11);
        entities.ExistingNextTranInfo.MiscText1 =
          db.GetNullableString(reader, 12);
        entities.ExistingNextTranInfo.MiscText2 =
          db.GetNullableString(reader, 13);
        entities.ExistingNextTranInfo.MiscNum1 =
          db.GetNullableInt64(reader, 14);
        entities.ExistingNextTranInfo.MiscNum2 =
          db.GetNullableInt64(reader, 15);
        entities.ExistingNextTranInfo.MiscNum1V2 =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingNextTranInfo.MiscNum2V2 =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingNextTranInfo.OspId = db.GetInt32(reader, 18);
        entities.ExistingNextTranInfo.Populated = true;
      });
  }

  private bool ReadProfile1()
  {
    entities.ExistingProfile.Populated = false;

    return Read("ReadProfile1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingProfile.Name = db.GetString(reader, 0);
        entities.ExistingProfile.Desc = db.GetNullableString(reader, 1);
        entities.ExistingProfile.Populated = true;
      });
  }

  private bool ReadProfile2()
  {
    entities.ExistingProfile.Populated = false;

    return Read("ReadProfile2",
      null,
      (db, reader) =>
      {
        entities.ExistingProfile.Name = db.GetString(reader, 0);
        entities.ExistingProfile.Desc = db.GetNullableString(reader, 1);
        entities.ExistingProfile.Populated = true;
      });
  }

  private bool ReadProfileAuthorization()
  {
    entities.ExistingProfileAuthorization.Populated = false;

    return Read("ReadProfileAuthorization",
      (db, command) =>
      {
        db.SetString(
          command, "fkTrnTrancode", entities.ExistingTransaction.Trancode);
        db.SetString(
          command, "fkTrnScreenid", entities.ExistingTransaction.ScreenId);
        db.SetString(command, "fkProName", entities.ExistingProfile.Name);
      },
      (db, reader) =>
      {
        entities.ExistingProfileAuthorization.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingProfileAuthorization.FkProName =
          db.GetString(reader, 1);
        entities.ExistingProfileAuthorization.FkTrnTrancode =
          db.GetString(reader, 2);
        entities.ExistingProfileAuthorization.FkTrnScreenid =
          db.GetString(reader, 3);
        entities.ExistingProfileAuthorization.FkCmdValue =
          db.GetString(reader, 4);
        entities.ExistingProfileAuthorization.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadTransaction()
  {
    entities.ExistingTransaction.Populated = false;

    return Read("ReadTransaction",
      (db, command) =>
      {
        db.SetString(command, "screenId", import.Standard.NextTransaction);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.NextTranAuthorization =
          db.GetString(reader, 3);
        entities.ExistingTransaction.Populated = true;
      });
  }

  private void UpdateNextTranInfo()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingNextTranInfo.Populated);

    var legalActionIdentifier =
      import.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();
    var courtCaseNumber = import.NextTranInfo.CourtCaseNumber ?? "";
    var caseNumber = import.NextTranInfo.CaseNumber ?? "";
    var csePersonNumber = import.NextTranInfo.CsePersonNumber ?? "";
    var csePersonNumberAp = import.NextTranInfo.CsePersonNumberAp ?? "";
    var csePersonNumberObligee = import.NextTranInfo.CsePersonNumberObligee ?? ""
      ;
    var csePersonNumberObligor = import.NextTranInfo.CsePersonNumberObligor ?? ""
      ;
    var courtOrderNumber = import.NextTranInfo.CourtOrderNumber ?? "";
    var obligationId = import.NextTranInfo.ObligationId.GetValueOrDefault();
    var standardCrtOrdNumber = import.NextTranInfo.StandardCrtOrdNumber ?? "";
    var infrastructureId =
      import.NextTranInfo.InfrastructureId.GetValueOrDefault();
    var miscText1 = import.NextTranInfo.MiscText1 ?? "";
    var miscText2 = import.NextTranInfo.MiscText2 ?? "";
    var miscNum1 = import.NextTranInfo.MiscNum1.GetValueOrDefault();
    var miscNum2 = import.NextTranInfo.MiscNum2.GetValueOrDefault();
    var miscNum1V2 = import.NextTranInfo.MiscNum1V2.GetValueOrDefault();
    var miscNum2V2 = import.NextTranInfo.MiscNum2V2.GetValueOrDefault();

    entities.ExistingNextTranInfo.Populated = false;
    Update("UpdateNextTranInfo",
      (db, command) =>
      {
        db.SetNullableString(command, "lastTran", "");
        db.SetNullableInt32(command, "legalActionIdent", legalActionIdentifier);
        db.SetNullableString(command, "courtCaseNumber", courtCaseNumber);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "csePersonNumber", csePersonNumber);
        db.SetNullableString(command, "csePersonNumber0", csePersonNumberAp);
        db.
          SetNullableString(command, "csePersonNumber1", csePersonNumberObligee);
          
        db.
          SetNullableString(command, "csePersonNumber2", csePersonNumberObligor);
          
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableInt32(command, "obligationId", obligationId);
        db.SetNullableString(command, "stdCrtOrdNbr", standardCrtOrdNumber);
        db.SetNullableInt32(command, "planTaskId", infrastructureId);
        db.SetNullableString(command, "miscText1", miscText1);
        db.SetNullableString(command, "miscText2", miscText2);
        db.SetNullableInt64(command, "miscNum1", miscNum1);
        db.SetNullableInt64(command, "miscNum2", miscNum2);
        db.SetNullableDecimal(command, "miscNum1V2", miscNum1V2);
        db.SetNullableDecimal(command, "miscNum2V2", miscNum2V2);
        db.SetInt32(command, "ospId", entities.ExistingNextTranInfo.OspId);
      });

    entities.ExistingNextTranInfo.LastTran = "";
    entities.ExistingNextTranInfo.LegalActionIdentifier = legalActionIdentifier;
    entities.ExistingNextTranInfo.CourtCaseNumber = courtCaseNumber;
    entities.ExistingNextTranInfo.CaseNumber = caseNumber;
    entities.ExistingNextTranInfo.CsePersonNumber = csePersonNumber;
    entities.ExistingNextTranInfo.CsePersonNumberAp = csePersonNumberAp;
    entities.ExistingNextTranInfo.CsePersonNumberObligee =
      csePersonNumberObligee;
    entities.ExistingNextTranInfo.CsePersonNumberObligor =
      csePersonNumberObligor;
    entities.ExistingNextTranInfo.CourtOrderNumber = courtOrderNumber;
    entities.ExistingNextTranInfo.ObligationId = obligationId;
    entities.ExistingNextTranInfo.StandardCrtOrdNumber = standardCrtOrdNumber;
    entities.ExistingNextTranInfo.InfrastructureId = infrastructureId;
    entities.ExistingNextTranInfo.MiscText1 = miscText1;
    entities.ExistingNextTranInfo.MiscText2 = miscText2;
    entities.ExistingNextTranInfo.MiscNum1 = miscNum1;
    entities.ExistingNextTranInfo.MiscNum2 = miscNum2;
    entities.ExistingNextTranInfo.MiscNum1V2 = miscNum1V2;
    entities.ExistingNextTranInfo.MiscNum2V2 = miscNum2V2;
    entities.ExistingNextTranInfo.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private Standard standard;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Auth.
    /// </summary>
    [JsonPropertyName("auth")]
    public Common Auth
    {
      get => auth ??= new();
      set => auth = value;
    }

    private DateWorkArea current;
    private Common auth;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("existingServiceProviderProfile")]
    public ServiceProviderProfile ExistingServiceProviderProfile
    {
      get => existingServiceProviderProfile ??= new();
      set => existingServiceProviderProfile = value;
    }

    /// <summary>
    /// A value of ExistingProfile.
    /// </summary>
    [JsonPropertyName("existingProfile")]
    public Profile ExistingProfile
    {
      get => existingProfile ??= new();
      set => existingProfile = value;
    }

    /// <summary>
    /// A value of ExistingTransactionCommand.
    /// </summary>
    [JsonPropertyName("existingTransactionCommand")]
    public TransactionCommand ExistingTransactionCommand
    {
      get => existingTransactionCommand ??= new();
      set => existingTransactionCommand = value;
    }

    /// <summary>
    /// A value of ExistingProfileAuthorization.
    /// </summary>
    [JsonPropertyName("existingProfileAuthorization")]
    public ProfileAuthorization ExistingProfileAuthorization
    {
      get => existingProfileAuthorization ??= new();
      set => existingProfileAuthorization = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingNextTranInfo.
    /// </summary>
    [JsonPropertyName("existingNextTranInfo")]
    public NextTranInfo ExistingNextTranInfo
    {
      get => existingNextTranInfo ??= new();
      set => existingNextTranInfo = value;
    }

    /// <summary>
    /// A value of ExistingTransaction.
    /// </summary>
    [JsonPropertyName("existingTransaction")]
    public Transaction ExistingTransaction
    {
      get => existingTransaction ??= new();
      set => existingTransaction = value;
    }

    private ServiceProviderProfile existingServiceProviderProfile;
    private Profile existingProfile;
    private TransactionCommand existingTransactionCommand;
    private ProfileAuthorization existingProfileAuthorization;
    private ServiceProvider existingServiceProvider;
    private NextTranInfo existingNextTranInfo;
    private Transaction existingTransaction;
  }
#endregion
}
