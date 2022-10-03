// Program: SC_CAB_NEXT_TRAN_PUT, ID: 371425724, model: 746.
// Short name: SWE01077
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SC_CAB_NEXT_TRAN_PUT.
/// </para>
/// <para>
/// RESP: SECURITY
/// Put all information required for population of the next transaction into 
/// entity NEXT_TRAN_INFO.
/// Set NEXTTRAN to translation of screen-id to TRANCODE.  Add command of &quot;
/// xxnextxx&quot; to NEXTTRAN.
/// </para>
/// </summary>
[Serializable]
public partial class ScCabNextTranPut: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CAB_NEXT_TRAN_PUT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCabNextTranPut(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCabNextTranPut.
  /// </summary>
  public ScCabNextTranPut(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // mjr
    // -----------------------------------------------------------------
    // Date	Developer	Description
    // --------------------------------------------------------------------
    // 				Initial development
    // 03/03/1999	M Ramirez	Added import_print_process view
    // --------------------------------------------------------------------
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
      if (IsEmpty(import.PrintProcess.Flag))
      {
        // mjr
        // -------------------------------------------
        // 03/03/1999
        // If this is populated, it means we are in the print process:
        // ignore this error
        // --------------------------------------------------------
        ExitState = "SC0052_NEXT_TRAN_PROHIBITED";

        return;
      }
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
        ExitState = "SC0005_USER_NF";

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

    if (IsEmpty(import.PrintProcess.Command))
    {
      global.NextTran = entities.ExistingTransaction.Trancode + " " + "XXNEXTXX"
        ;
    }
    else
    {
      // mjr
      // -------------------------------------------
      // 03/03/1999
      // If this is populated, it means we are in the print process:
      // override the COMMAND xxnextxx
      // Note:  there is no checking that the COMMAND is a valid command
      // --------------------------------------------------------
      global.NextTran = entities.ExistingTransaction.Trancode + " " + import
        .PrintProcess.Command;
    }
  }

  private void CreateNextTranInfo()
  {
    var lastTran = Substring(global.TranCode, 1, 4);
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
        db.SetNullableString(command, "lastTran", lastTran);
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

    entities.ExistingNextTranInfo.LastTran = lastTran;
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
        entities.ExistingTransaction.NextTranAuthorization =
          db.GetString(reader, 2);
        entities.ExistingTransaction.Populated = true;
      });
  }

  private void UpdateNextTranInfo()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingNextTranInfo.Populated);

    var lastTran = Substring(global.TranCode, 1, 4);
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
        db.SetNullableString(command, "lastTran", lastTran);
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

    entities.ExistingNextTranInfo.LastTran = lastTran;
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

    /// <summary>
    /// A value of PrintProcess.
    /// </summary>
    [JsonPropertyName("printProcess")]
    public Common PrintProcess
    {
      get => printProcess ??= new();
      set => printProcess = value;
    }

    private Standard standard;
    private NextTranInfo nextTranInfo;
    private Common printProcess;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
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

    private NextTranInfo existingNextTranInfo;
    private ServiceProvider existingServiceProvider;
    private Transaction existingTransaction;
  }
#endregion
}
