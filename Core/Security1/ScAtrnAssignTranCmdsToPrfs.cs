// Program: SC_ATRN_ASSIGN_TRAN_CMDS_TO_PRFS, ID: 371452488, model: 746.
// Short name: SWEATRNP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Kessep;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// <para>
/// A program: SC_ATRN_ASSIGN_TRAN_CMDS_TO_PRFS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class ScAtrnAssignTranCmdsToPrfs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_ATRN_ASSIGN_TRAN_CMDS_TO_PRFS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScAtrnAssignTranCmdsToPrfs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScAtrnAssignTranCmdsToPrfs.
  /// </summary>
  public ScAtrnAssignTranCmdsToPrfs(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date	      Developer 		Description
    // 01/06/96      M. Mallari            Initial development
    // 08/26/96      G. Lofton             Corrected overflow
    //                                     
    // problem.
    // 12/12/96      R. Marchman           Add new security/next tran
    // ------------------------------------------------------------
    // --------------------------------------------------------------------------------
    // 03/02/2002  Vithal Madhira         PR# 139574.
    // The security CAB 'SC_CAB_TEST_SECURITY' is not getting executed. Fixed 
    // the IF loop.
    // ---------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    MoveProfile(import.Profile, export.Profile);
    export.HiddenPrev.Name = import.HiddenPrevProfile.Name;
    export.PromptProfile.PromptField = import.PromptProfile.PromptField;
    export.PreviousCommandDelete.Flag = import.PreviousCommandDelete.Flag;
    export.Starting.ScreenId = import.Starting.ScreenId;

    for(import.HiddenSelected.Index = 0; import.HiddenSelected.Index < import
      .HiddenSelected.Count; ++import.HiddenSelected.Index)
    {
      if (!import.HiddenSelected.CheckSize())
      {
        break;
      }

      export.Selected.Index = import.HiddenSelected.Index;
      export.Selected.CheckSize();

      export.Selected.Update.HiddenSelCommon.SelectChar =
        import.HiddenSelected.Item.HiddenSelCommon.SelectChar;
      MoveTransaction(import.HiddenSelected.Item.HiddenSelTransaction,
        export.Selected.Update.HiddenSelTransaction);
    }

    import.HiddenSelected.CheckIndex();

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        export.Group.Index = import.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        export.Group.Update.Transaction.Assign(import.Group.Item.Transaction);
        export.Group.Update.TransactionCommand.Id =
          import.Group.Item.TransactionCommand.Id;
        export.Group.Update.ProfileAuthorization.CreatedTimestamp =
          import.Group.Item.ProfileAuthorization.CreatedTimestamp;
        export.Group.Update.AuthorizedCommand.Count =
          import.Group.Item.AuthorizedCommands.Count;

        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case 'S':
            ++local.Common.Count;

            break;
          case ' ':
            break;
          case '*':
            break;
          default:
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

            break;
        }
      }

      import.Group.CheckIndex();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (AsChar(import.PreviousCommandDelete.Flag) == 'Y')
    {
      if (!Equal(global.Command, "DELETE"))
      {
        export.PreviousCommandDelete.Flag = "";
      }
    }

    if (Equal(global.Command, "PTSL") || Equal(global.Command, "PCSL") || Equal
      (global.Command, "DELETE"))
    {
      if (Equal(import.Profile.Name, import.HiddenPrevProfile.Name))
      {
      }
      else
      {
        ExitState = "SC0019_KEY_CHANGED_REDSPLAY";

        return;
      }
    }

    if (Equal(global.Command, "PCSL") || Equal(global.Command, "DELETE"))
    {
      if (local.Common.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }
    }

    if (Equal(global.Command, "RETFMPRO"))
    {
      // this is when you come back from selecting a profile
      export.PromptProfile.PromptField = "";

      if (IsEmpty(import.HiddenSelected1.Name))
      {
        // nothing was returned from the profile list
      }
      else
      {
        export.PromptProfile.PromptField = "";
        MoveProfile(import.HiddenSelected1, export.Profile);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETFMCMD") || Equal(global.Command, "RETFMTRN"))
    {
      for(export.Selected.Index = 0; export.Selected.Index < export
        .Selected.Count; ++export.Selected.Index)
      {
        if (!export.Selected.CheckSize())
        {
          break;
        }

        if (AsChar(export.Selected.Item.HiddenSelCommon.SelectChar) == 'S')
        {
          export.Selected.Update.HiddenSelCommon.SelectChar = "";
          MoveTransaction(export.Selected.Item.HiddenSelTransaction,
            export.HiddenSelected1);
          ExitState = "ECO_LNK_TO_COMMANDS";

          return;
        }
      }

      export.Selected.CheckIndex();
      global.Command = "DISPLAY";
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    // --------------------------------------------------------------------------------
    // 03/02/2002  Vithal Madhira         PR# 139574.
    // The above security CAB 'SC_CAB_TEST_SECURITY' is not getting executed. 
    // Fixed the IF loop.
    // ---------------------------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();
    }
    else
    {
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** end   group C ****
    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        export.PromptProfile.PromptField = "";

        if (ReadProfile())
        {
          MoveProfile(entities.ExistingProfile, export.Profile);
        }
        else
        {
          export.Profile.Desc = "";

          var field = GetField(export.Profile, "name");

          field.Error = true;

          ExitState = "SC0015_PROFILE_NF";

          return;
        }

        export.Group.Index = -1;

        foreach(var item in ReadTransaction())
        {
          if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
          {
            ExitState = "SC0044_VIEW_EXCEEDED_USE_STRT_KY";

            break;
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.Transaction.Assign(entities.ExistingTransaction);
          export.Group.Update.AuthorizedCommand.Count = 0;

          foreach(var item1 in ReadCommand())
          {
            export.Group.Update.AuthorizedCommand.Count =
              export.Group.Item.AuthorizedCommand.Count + 1;
          }

          if (ReadTransactionCommandProfileAuthorization())
          {
            export.Group.Update.TransactionCommand.Id =
              entities.ExistingTransactionCommand.Id;
            export.Group.Update.ProfileAuthorization.CreatedTimestamp =
              entities.ExistingProfileAuthorization.CreatedTimestamp;
          }
          else
          {
            ExitState = "SC0018_TRANS_COMMAND_NF";
          }
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        export.HiddenPrev.Name = entities.ExistingProfile.Name;
        export.PreviousCommandDelete.Flag = "";

        break;
      case "LIST":
        switch(AsChar(import.PromptProfile.PromptField))
        {
          case 'S':
            ExitState = "ECO_LNK_TO_PROFILES";

            break;
          case ' ':
            var field = GetField(export.PromptProfile, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          default:
            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

            break;
        }

        break;
      case "PCSL":
        export.Selected.Index = -1;

        // LOAD UP THE EXPORT HIDDEN SELECTED GROUP WITH THE TRANCODE AND SCREEN
        // ID SELECTED.
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            ++export.Selected.Index;
            export.Selected.CheckSize();

            export.Selected.Update.HiddenSelCommon.SelectChar =
              export.Group.Item.Common.SelectChar;
            MoveTransaction(export.Group.Item.Transaction,
              export.Selected.Update.HiddenSelTransaction);
            export.Group.Update.Common.SelectChar = "";
          }
        }

        export.Group.CheckIndex();

        for(export.Selected.Index = 0; export.Selected.Index < export
          .Selected.Count; ++export.Selected.Index)
        {
          if (!export.Selected.CheckSize())
          {
            break;
          }

          if (AsChar(export.Selected.Item.HiddenSelCommon.SelectChar) == 'S')
          {
            export.Selected.Update.HiddenSelCommon.SelectChar = "";
            MoveTransaction(export.Selected.Item.HiddenSelTransaction,
              export.HiddenSelected1);

            break;
          }
        }

        export.Selected.CheckIndex();
        ExitState = "ECO_LNK_TO_COMMANDS";

        break;
      case "PTSL":
        if (IsEmpty(export.Profile.Name))
        {
          var field = GetField(export.Profile, "name");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        ExitState = "ECO_LNK_TO_TRANSACTIONS";

        break;
      case "DELETE":
        // ****   This is the command set up by the DELETE confirmation key.  It
        // will go through all the transactions for the TRANSACTION_COMMAND and
        // delete.
        if (AsChar(export.PreviousCommandDelete.Flag) == 'Y')
        {
        }
        else
        {
          export.PreviousCommandDelete.Flag = "Y";
          ExitState = "SC0033_DELETE_CONFIRMATION";

          return;
        }

        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseDeleteProfileAuthorization();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;
              }
            }
          }
        }

        export.Group.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_DEL_2";
        }

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveProfile(Profile source, Profile target)
  {
    target.Name = source.Name;
    target.Desc = source.Desc;
  }

  private static void MoveTransaction(Transaction source, Transaction target)
  {
    target.ScreenId = source.ScreenId;
    target.Trancode = source.Trancode;
  }

  private void UseDeleteProfileAuthorization()
  {
    var useImport = new DeleteProfileAuthorization.Import();
    var useExport = new DeleteProfileAuthorization.Export();

    useImport.Profile.Name = export.Profile.Name;
    MoveTransaction(export.Group.Item.Transaction, useImport.Transaction);

    Call(DeleteProfileAuthorization.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCommand()
  {
    entities.ExistingCommand.Populated = false;

    return ReadEach("ReadCommand",
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
        entities.ExistingCommand.Value = db.GetString(reader, 0);
        entities.ExistingCommand.Populated = true;

        return true;
      });
  }

  private bool ReadProfile()
  {
    entities.ExistingProfile.Populated = false;

    return Read("ReadProfile",
      (db, command) =>
      {
        db.SetString(command, "name", export.Profile.Name);
      },
      (db, reader) =>
      {
        entities.ExistingProfile.Name = db.GetString(reader, 0);
        entities.ExistingProfile.Desc = db.GetNullableString(reader, 1);
        entities.ExistingProfile.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTransaction()
  {
    entities.ExistingTransaction.Populated = false;

    return ReadEach("ReadTransaction",
      (db, command) =>
      {
        db.SetString(command, "fkProName", entities.ExistingProfile.Name);
        db.SetString(command, "screenId", export.Starting.ScreenId);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private bool ReadTransactionCommandProfileAuthorization()
  {
    entities.ExistingProfileAuthorization.Populated = false;
    entities.ExistingTransactionCommand.Populated = false;

    return Read("ReadTransactionCommandProfileAuthorization",
      (db, command) =>
      {
        db.SetString(
          command, "fkTrnTrancode", entities.ExistingTransaction.Trancode);
        db.SetString(
          command, "fkTrnScreenid", entities.ExistingTransaction.ScreenId);
      },
      (db, reader) =>
      {
        entities.ExistingTransactionCommand.Id = db.GetInt32(reader, 0);
        entities.ExistingTransactionCommand.FkTrnScreenid =
          db.GetString(reader, 1);
        entities.ExistingProfileAuthorization.FkTrnScreenid =
          db.GetString(reader, 1);
        entities.ExistingTransactionCommand.FkTrnTrancode =
          db.GetString(reader, 2);
        entities.ExistingProfileAuthorization.FkTrnTrancode =
          db.GetString(reader, 2);
        entities.ExistingTransactionCommand.FkCmdValue =
          db.GetString(reader, 3);
        entities.ExistingProfileAuthorization.FkCmdValue =
          db.GetString(reader, 3);
        entities.ExistingProfileAuthorization.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingProfileAuthorization.FkProName =
          db.GetString(reader, 5);
        entities.ExistingProfileAuthorization.Populated = true;
        entities.ExistingTransactionCommand.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of TransactionCommand.
      /// </summary>
      [JsonPropertyName("transactionCommand")]
      public TransactionCommand TransactionCommand
      {
        get => transactionCommand ??= new();
        set => transactionCommand = value;
      }

      /// <summary>
      /// A value of ProfileAuthorization.
      /// </summary>
      [JsonPropertyName("profileAuthorization")]
      public ProfileAuthorization ProfileAuthorization
      {
        get => profileAuthorization ??= new();
        set => profileAuthorization = value;
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
      /// A value of Transaction.
      /// </summary>
      [JsonPropertyName("transaction")]
      public Transaction Transaction
      {
        get => transaction ??= new();
        set => transaction = value;
      }

      /// <summary>
      /// A value of AuthorizedCommands.
      /// </summary>
      [JsonPropertyName("authorizedCommands")]
      public Common AuthorizedCommands
      {
        get => authorizedCommands ??= new();
        set => authorizedCommands = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 230;

      private TransactionCommand transactionCommand;
      private ProfileAuthorization profileAuthorization;
      private Common common;
      private Transaction transaction;
      private Common authorizedCommands;
    }

    /// <summary>A HiddenSelectedGroup group.</summary>
    [Serializable]
    public class HiddenSelectedGroup
    {
      /// <summary>
      /// A value of HiddenSelCommon.
      /// </summary>
      [JsonPropertyName("hiddenSelCommon")]
      public Common HiddenSelCommon
      {
        get => hiddenSelCommon ??= new();
        set => hiddenSelCommon = value;
      }

      /// <summary>
      /// A value of HiddenSelTransaction.
      /// </summary>
      [JsonPropertyName("hiddenSelTransaction")]
      public Transaction HiddenSelTransaction
      {
        get => hiddenSelTransaction ??= new();
        set => hiddenSelTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common hiddenSelCommon;
      private Transaction hiddenSelTransaction;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Transaction Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of HiddenPrevTransaction.
    /// </summary>
    [JsonPropertyName("hiddenPrevTransaction")]
    public Transaction HiddenPrevTransaction
    {
      get => hiddenPrevTransaction ??= new();
      set => hiddenPrevTransaction = value;
    }

    /// <summary>
    /// A value of PreviousCommandDelete.
    /// </summary>
    [JsonPropertyName("previousCommandDelete")]
    public Common PreviousCommandDelete
    {
      get => previousCommandDelete ??= new();
      set => previousCommandDelete = value;
    }

    /// <summary>
    /// A value of HiddenPrevProfile.
    /// </summary>
    [JsonPropertyName("hiddenPrevProfile")]
    public Profile HiddenPrevProfile
    {
      get => hiddenPrevProfile ??= new();
      set => hiddenPrevProfile = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// A value of PromptProfile.
    /// </summary>
    [JsonPropertyName("promptProfile")]
    public Standard PromptProfile
    {
      get => promptProfile ??= new();
      set => promptProfile = value;
    }

    /// <summary>
    /// A value of HiddenSelected1.
    /// </summary>
    [JsonPropertyName("hiddenSelected1")]
    public Profile HiddenSelected1
    {
      get => hiddenSelected1 ??= new();
      set => hiddenSelected1 = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenSelected.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSelectedGroup> HiddenSelected => hiddenSelected ??= new(
      HiddenSelectedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenSelected for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    [Computed]
    public IList<HiddenSelectedGroup> HiddenSelected_Json
    {
      get => hiddenSelected;
      set => HiddenSelected.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Transaction starting;
    private Transaction hiddenPrevTransaction;
    private Common previousCommandDelete;
    private Profile hiddenPrevProfile;
    private Profile profile;
    private Standard promptProfile;
    private Profile hiddenSelected1;
    private Array<GroupGroup> group;
    private Array<HiddenSelectedGroup> hiddenSelected;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of TransactionCommand.
      /// </summary>
      [JsonPropertyName("transactionCommand")]
      public TransactionCommand TransactionCommand
      {
        get => transactionCommand ??= new();
        set => transactionCommand = value;
      }

      /// <summary>
      /// A value of ProfileAuthorization.
      /// </summary>
      [JsonPropertyName("profileAuthorization")]
      public ProfileAuthorization ProfileAuthorization
      {
        get => profileAuthorization ??= new();
        set => profileAuthorization = value;
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
      /// A value of Transaction.
      /// </summary>
      [JsonPropertyName("transaction")]
      public Transaction Transaction
      {
        get => transaction ??= new();
        set => transaction = value;
      }

      /// <summary>
      /// A value of AuthorizedCommand.
      /// </summary>
      [JsonPropertyName("authorizedCommand")]
      public Common AuthorizedCommand
      {
        get => authorizedCommand ??= new();
        set => authorizedCommand = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 230;

      private TransactionCommand transactionCommand;
      private ProfileAuthorization profileAuthorization;
      private Common common;
      private Transaction transaction;
      private Common authorizedCommand;
    }

    /// <summary>A SelectedGroup group.</summary>
    [Serializable]
    public class SelectedGroup
    {
      /// <summary>
      /// A value of HiddenSelCommon.
      /// </summary>
      [JsonPropertyName("hiddenSelCommon")]
      public Common HiddenSelCommon
      {
        get => hiddenSelCommon ??= new();
        set => hiddenSelCommon = value;
      }

      /// <summary>
      /// A value of HiddenSelTransaction.
      /// </summary>
      [JsonPropertyName("hiddenSelTransaction")]
      public Transaction HiddenSelTransaction
      {
        get => hiddenSelTransaction ??= new();
        set => hiddenSelTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common hiddenSelCommon;
      private Transaction hiddenSelTransaction;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Transaction Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of HiddenSelected1.
    /// </summary>
    [JsonPropertyName("hiddenSelected1")]
    public Transaction HiddenSelected1
    {
      get => hiddenSelected1 ??= new();
      set => hiddenSelected1 = value;
    }

    /// <summary>
    /// A value of PreviousCommandDelete.
    /// </summary>
    [JsonPropertyName("previousCommandDelete")]
    public Common PreviousCommandDelete
    {
      get => previousCommandDelete ??= new();
      set => previousCommandDelete = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Profile HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// A value of PromptProfile.
    /// </summary>
    [JsonPropertyName("promptProfile")]
    public Standard PromptProfile
    {
      get => promptProfile ??= new();
      set => promptProfile = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// Gets a value of Selected.
    /// </summary>
    [JsonIgnore]
    public Array<SelectedGroup> Selected => selected ??= new(
      SelectedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Selected for json serialization.
    /// </summary>
    [JsonPropertyName("selected")]
    [Computed]
    public IList<SelectedGroup> Selected_Json
    {
      get => selected;
      set => Selected.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Transaction starting;
    private Transaction hiddenSelected1;
    private Common previousCommandDelete;
    private Profile hiddenPrev;
    private Profile profile;
    private Standard promptProfile;
    private Array<GroupGroup> group;
    private Array<SelectedGroup> selected;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Common Command
    {
      get => command ??= new();
      set => command = value;
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

    private Common command;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingCommand.
    /// </summary>
    [JsonPropertyName("existingCommand")]
    public Command ExistingCommand
    {
      get => existingCommand ??= new();
      set => existingCommand = value;
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

    private Profile existingProfile;
    private ProfileAuthorization existingProfileAuthorization;
    private TransactionCommand existingTransactionCommand;
    private Command existingCommand;
    private Transaction existingTransaction;
  }
#endregion
}
