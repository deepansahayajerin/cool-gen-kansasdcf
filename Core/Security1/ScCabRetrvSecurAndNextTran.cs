// Program: SC_CAB_RETRV_SECUR_AND_NEXT_TRAN, ID: 371453008, model: 746.
// Short name: SWE01078
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// <para>
/// A program: SC_CAB_RETRV_SECUR_AND_NEXT_TRAN.
/// </para>
/// <para>
/// RESP: SECUR
/// </para>
/// </summary>
[Serializable]
public partial class ScCabRetrvSecurAndNextTran: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CAB_RETRV_SECUR_AND_NEXT_TRAN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCabRetrvSecurAndNextTran(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCabRetrvSecurAndNextTran.
  /// </summary>
  public ScCabRetrvSecurAndNextTran(IContext context, Import import,
    Export export):
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
    export.Hidden.LinkIndicator = import.Hidden.LinkIndicator;

    // *** THIS "IF" TEST IS ONLY HERE SO THAT I HAVE A WAY TO ESCAPE FROM THE 
    // EXECUTED CODE AND STILL BE ABLE TO EXECUTE ADDITIONAL CODE AT THE END OF
    // THE CAB. THERE IS NO OTHER REASON FOR THIS
    if (local.Common.Count == 0)
    {
      if (!ReadTransaction())
      {
        export.Hidden.Command = "NOTRAN";
        ExitState = "SC0002_SCREEN_ID_NF";

        goto Test;
      }

      if (AsChar(entities.ExistingTransaction.MenuInd) == 'Y')
      {
        // ALL MENUES ARE BY DEFAULT AUTHORIZED TO ALL USERS.
        local.Common.Count = 1;
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
          export.Hidden.Command = "XXNOKEY";
          ExitState = "SC0025_KEY_NF_RB";

          goto Test;
        }

        if (Equal(entities.ExistingProfile.Desc, "TEST"))
        {
          // we are in the test environment. get all of the allowable commands 
          // for the desired transaction.
          export.HiddenSecurity.Index = 0;
          export.HiddenSecurity.Clear();

          foreach(var item in ReadTransactionCommandCommand())
          {
            export.HiddenSecurity.Update.HiddenSecurityCommand.Value =
              entities.ExistingCommand.Value;
            export.HiddenSecurity.Update.HiddenSecurityProfileAuthorization.
              ActiveInd = "Y";
            local.Common.Count = 1;
            export.HiddenSecurity.Next();
          }
        }
        else if (Equal(entities.ExistingProfile.Desc, "PROD"))
        {
          // we are in the production environment. get all of the users 
          // security.
          local.Common.Count = 0;

          if (!ReadProfile1())
          {
            export.Hidden.Command = "NOTAUTH";
            ExitState = "SC0001_USER_NOT_AUTH_TRAN";

            goto Test;
          }

          export.HiddenSecurity.Index = 0;
          export.HiddenSecurity.Clear();

          foreach(var item in ReadProfileAuthorizationCommand())
          {
            ++local.Common.Count;
            export.HiddenSecurity.Update.HiddenSecurityCommand.Value =
              entities.ExistingCommand.Value;
            export.HiddenSecurity.Update.HiddenSecurityProfileAuthorization.
              ActiveInd = entities.ExistingProfileAuthorization.ActiveInd;
            export.HiddenSecurity.Next();
          }
        }
      }

      if (local.Common.Count == 0)
      {
        export.Hidden.Command = "NOTAUTH";
        ExitState = "SC0004_USER_NOT_AUTH_MENU_SEL";
      }
    }

Test:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      if (Equal(global.Command, "XXFMMENU"))
      {
        ExitState = "ECO_LNK_RETURN_TO_MENU";
      }
      else if (AsChar(export.Hidden.LinkIndicator) == 'L')
      {
        export.Hidden.Command = "NOLINK";
        export.Hidden.LinkIndicator = "R";
        ExitState = "ACO_NE0000_RETURN";
      }
      else
      {
        export.Hidden.Command = "NONEXT";
        ExitState = "ECO_LNK_RETURN_TO_MENU";
      }

      return;
    }

    // the XXNEXTXX command will be set if the procedure is executed via next 
    // tran from another screen.
    if (Equal(global.Command, "XXNEXTXX"))
    {
      if (ReadNextTranInfo())
      {
        export.NextTranInfo.Assign(entities.ExistingNextTranInfo);

        if (AsChar(entities.ExistingTransaction.MenuInd) == 'Y')
        {
          global.Command = "";
        }
      }
      else
      {
        ExitState = "SC0000_NEXT_TRAN_INFO_NF";
      }
    }
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

  private IEnumerable<bool> ReadProfileAuthorizationCommand()
  {
    return ReadEach("ReadProfileAuthorizationCommand",
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
        if (export.HiddenSecurity.IsFull)
        {
          return false;
        }

        entities.ExistingProfileAuthorization.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingProfileAuthorization.ActiveInd =
          db.GetNullableString(reader, 1);
        entities.ExistingProfileAuthorization.FkProName =
          db.GetString(reader, 2);
        entities.ExistingProfileAuthorization.FkTrnTrancode =
          db.GetString(reader, 3);
        entities.ExistingProfileAuthorization.FkTrnScreenid =
          db.GetString(reader, 4);
        entities.ExistingProfileAuthorization.FkCmdValue =
          db.GetString(reader, 5);
        entities.ExistingCommand.Value = db.GetString(reader, 5);
        entities.ExistingCommand.Populated = true;
        entities.ExistingProfileAuthorization.Populated = true;

        return true;
      });
  }

  private bool ReadTransaction()
  {
    entities.ExistingTransaction.Populated = false;

    return Read("ReadTransaction",
      (db, command) =>
      {
        db.SetString(command, "trancode", global.TranCode);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTransactionCommandCommand()
  {
    return ReadEach("ReadTransactionCommandCommand",
      (db, command) =>
      {
        db.SetString(
          command, "fkTrnTrancode", entities.ExistingTransaction.Trancode);
        db.SetString(
          command, "fkTrnScreenid", entities.ExistingTransaction.ScreenId);
      },
      (db, reader) =>
      {
        if (export.HiddenSecurity.IsFull)
        {
          return false;
        }

        entities.ExistingTransactionCommand.Id = db.GetInt32(reader, 0);
        entities.ExistingTransactionCommand.FkTrnScreenid =
          db.GetString(reader, 1);
        entities.ExistingTransactionCommand.FkTrnTrancode =
          db.GetString(reader, 2);
        entities.ExistingTransactionCommand.FkCmdValue =
          db.GetString(reader, 3);
        entities.ExistingCommand.Value = db.GetString(reader, 3);
        entities.ExistingCommand.Populated = true;
        entities.ExistingTransactionCommand.Populated = true;

        return true;
      });
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public Security2 Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private Security2 hidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public Security2 Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private Array<HiddenSecurityGroup> hiddenSecurity;
    private NextTranInfo nextTranInfo;
    private Security2 hidden;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of Auth.
    /// </summary>
    [JsonPropertyName("auth")]
    public Common Auth
    {
      get => auth ??= new();
      set => auth = value;
    }

    private DateWorkArea current;
    private Common common;
    private NextTranInfo nextTranInfo;
    private Common auth;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCommand.
    /// </summary>
    [JsonPropertyName("existingCommand")]
    public Command ExistingCommand
    {
      get => existingCommand ??= new();
      set => existingCommand = value;
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
    /// A value of ExistingTransactionCommand.
    /// </summary>
    [JsonPropertyName("existingTransactionCommand")]
    public TransactionCommand ExistingTransactionCommand
    {
      get => existingTransactionCommand ??= new();
      set => existingTransactionCommand = value;
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
    /// A value of ExistingServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("existingServiceProviderProfile")]
    public ServiceProviderProfile ExistingServiceProviderProfile
    {
      get => existingServiceProviderProfile ??= new();
      set => existingServiceProviderProfile = value;
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

    private Command existingCommand;
    private ProfileAuthorization existingProfileAuthorization;
    private TransactionCommand existingTransactionCommand;
    private Transaction existingTransaction;
    private Profile existingProfile;
    private ServiceProviderProfile existingServiceProviderProfile;
    private ServiceProvider existingServiceProvider;
    private NextTranInfo existingNextTranInfo;
  }
#endregion
}
