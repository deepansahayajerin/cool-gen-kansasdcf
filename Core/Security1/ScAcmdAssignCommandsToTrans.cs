// Program: SC_ACMD_ASSIGN_COMMANDS_TO_TRANS, ID: 371743091, model: 746.
// Short name: SWEACMDP
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
/// A program: SC_ACMD_ASSIGN_COMMANDS_TO_TRANS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class ScAcmdAssignCommandsToTrans: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_ACMD_ASSIGN_COMMANDS_TO_TRANS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScAcmdAssignCommandsToTrans(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScAcmdAssignCommandsToTrans.
  /// </summary>
  public ScAcmdAssignCommandsToTrans(IContext context, Import import,
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
    //        M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 12/10/95  Alan Hackler		Initial Development
    // 09/18/96  G. Lofton		Corrected overflow problem
    // 12/12/96  R. Marchman           Add new security/next tran
    // 07/23/97  R. Marchman		Increased the size of the import group view.
    // 03/02/02  Vithal Madhira        PR# 139574.  The security CAB '
    // SC_CAB_TEST_SECURITY'
    // 				is not getting executed. Fixed the IF loop.
    // 01/13/20  GVandy		CQ67034.  Increase group view size and add starting
    // 				command value.
    // -----------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Transaction.Assign(import.Transaction);
    MoveTransaction1(import.HiddenPrev, export.HiddenPrev);
    export.PromptTransaction.PromptField = import.PromptTransaction.PromptField;
    export.Starting.Value = import.Starting.Value;

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
        MoveCommand(import.Group.Item.Command, export.Group.Update.Command);
        export.Group.Update.Active.SelectChar =
          import.Group.Item.Active.SelectChar;
        export.Group.Update.HiddenPrevActive.SelectChar =
          import.Group.Item.HiddenPrevActive.SelectChar;
        export.Group.Update.HiddenAction.SelectChar = "";

        if (AsChar(export.Transaction.MenuInd) == 'Y')
        {
          // no need to allow user to set up commands on menues, so dont allow 
          // the user to try, protect up the select field.
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Color = "cyan";
          field.Protected = true;
        }

        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case 'S':
            ++local.Common.Count;

            break;
          case ' ':
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

    if (Equal(global.Command, "RETFMTRN"))
    {
      export.PromptTransaction.PromptField = "";

      if (IsEmpty(import.HiddenSelected.Trancode))
      {
        // nothing was returned from the transaction list
      }
      else
      {
        MoveTransaction2(import.HiddenSelected, export.Transaction);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "UPDATE"))
    {
      if (Equal(import.Transaction.ScreenId, import.HiddenPrev.ScreenId) && Equal
        (import.Transaction.Trancode, import.HiddenPrev.Trancode))
      {
      }
      else
      {
        var field1 = GetField(export.Transaction, "trancode");

        field1.Error = true;

        var field2 = GetField(export.Transaction, "screenId");

        field2.Error = true;

        ExitState = "SC0019_KEY_CHANGED_REDSPLAY";

        return;
      }

      if (AsChar(export.Transaction.MenuInd) == 'Y')
      {
        ExitState = "SC0043_MENUES_DONT_REQUIRE_CMDS";

        return;
      }

      if (local.Common.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          switch(AsChar(export.Group.Item.Active.SelectChar))
          {
            case 'Y':
              break;
            case 'N':
              break;
            default:
              var field = GetField(export.Group.Item.Active, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE0_INVALID_SELECTION_YN";

              break;
          }
        }
      }

      export.Group.CheckIndex();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          if (AsChar(export.Group.Item.Active.SelectChar) == AsChar
            (export.Group.Item.HiddenPrevActive.SelectChar))
          {
            // NO CHANGE, NO UPDATE NEEDED
            export.Group.Update.HiddenAction.SelectChar = "";

            continue;
          }

          if (AsChar(export.Group.Item.Active.SelectChar) == 'Y' && AsChar
            (export.Group.Item.HiddenPrevActive.SelectChar) == 'N')
          {
            // WILL BE ADDED TO THE TRANSACTION, MUST BE AN ADD
            export.Group.Update.HiddenAction.SelectChar = "A";

            continue;
          }

          if (AsChar(export.Group.Item.Active.SelectChar) == 'N' && AsChar
            (export.Group.Item.HiddenPrevActive.SelectChar) == 'Y')
          {
            // MUST WANT TO DELETE THE COMMAND FROM THE TRANSACTION. MUST BE A 
            // DELETE
            export.Group.Update.HiddenAction.SelectChar = "D";

            // VERIFY THAT THE TRANSACTION AND COMMAND COMBINATION ARE NOT 
            // CURRENTLY AUTHORIZED TO SOME GROUP OF USERS.
            if (ReadProfileAuthorization())
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "SC0017_CANNOT_DELETE_IN_USE";
            }
          }
        }
      }

      export.Group.CheckIndex();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
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
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
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
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
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
        export.PromptTransaction.PromptField = "";

        if (ReadTransaction())
        {
          export.Transaction.Assign(entities.ExistingTransaction);
          MoveTransaction1(entities.ExistingTransaction, export.HiddenPrev);
        }
        else
        {
          export.Transaction.Trancode = "";
          export.Transaction.Desc = "";
          export.Transaction.MenuInd = "";
          export.Transaction.NextTranAuthorization = "";

          var field = GetField(export.Transaction, "screenId");

          field.Error = true;

          ExitState = "SC0002_SCREEN_ID_NF";

          return;
        }

        if (AsChar(export.Transaction.MenuInd) == 'Y')
        {
          ExitState = "SC0043_MENUES_DONT_REQUIRE_CMDS";

          return;
        }

        export.Group.Index = -1;
        export.Group.Count = 0;

        foreach(var item in ReadCommand())
        {
          if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
          {
            ExitState = "SC0045_GROUP_VIEW_LIMIT_EXCEEDED";

            break;
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          MoveCommand(entities.ExistingCommand, export.Group.Update.Command);
          export.Group.Update.Active.SelectChar = "N";

          if (ReadTransactionCommand())
          {
            export.Group.Update.Active.SelectChar = "Y";
          }

          if (AsChar(entities.ExistingTransaction.MenuInd) == 'Y')
          {
            // no need to allow user to set up commands on menues, so dont allow
            // the user to try, protect up the select field.
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = true;
          }

          export.Group.Update.HiddenPrevActive.SelectChar =
            export.Group.Item.Active.SelectChar;
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        break;
      case "LIST":
        switch(AsChar(import.PromptTransaction.PromptField))
        {
          case 'S':
            ExitState = "ECO_LNK_TO_TRANSACTIONS";

            break;
          case ' ':
            var field1 = GetField(export.PromptTransaction, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          default:
            var field2 = GetField(export.PromptTransaction, "promptField");

            field2.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

            break;
        }

        break;
      case "UPDATE":
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
              switch(AsChar(export.Group.Item.HiddenAction.SelectChar))
              {
                case 'A':
                  UseCreateTransactionCommands();

                  break;
                case 'D':
                  // MUST BE A "D" FOR DELETES
                  UseDeleteTransactionCommands();

                  break;
                case ' ':
                  break;
                default:
                  break;
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";
                export.Group.Update.HiddenPrevActive.SelectChar =
                  export.Group.Item.Active.SelectChar;
              }
              else if (IsExitState("SC0002_SCREEN_ID_NF"))
              {
                var field1 = GetField(export.Transaction, "screenId");

                field1.Error = true;

                var field2 = GetField(export.Transaction, "trancode");

                field2.Error = true;
              }
              else if (IsExitState("SC0012_COMMAND_NF"))
              {
                var field = GetField(export.Group.Item.Command, "value");

                field.Error = true;
              }
              else if (IsExitState("SC0018_TRANS_COMMAND_NF"))
              {
                var field1 = GetField(export.Transaction, "screenId");

                field1.Error = true;

                var field2 = GetField(export.Transaction, "trancode");

                field2.Error = true;

                var field3 = GetField(export.Group.Item.Command, "value");

                field3.Error = true;
              }
              else if (IsExitState("SC0018_TRANS_COMMAND_AE"))
              {
                var field1 = GetField(export.Transaction, "screenId");

                field1.Error = true;

                var field2 = GetField(export.Transaction, "trancode");

                field2.Error = true;

                var field3 = GetField(export.Group.Item.Command, "value");

                field3.Error = true;
              }
              else
              {
                var field1 = GetField(export.Transaction, "screenId");

                field1.Error = true;

                var field2 = GetField(export.Transaction, "trancode");

                field2.Error = true;

                var field3 = GetField(export.Group.Item.Command, "value");

                field3.Error = true;
              }
            }
          }
        }

        export.Group.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
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

  private static void MoveCommand(Command source, Command target)
  {
    target.Value = source.Value;
    target.Desc = source.Desc;
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

  private static void MoveTransaction1(Transaction source, Transaction target)
  {
    target.ScreenId = source.ScreenId;
    target.Trancode = source.Trancode;
  }

  private static void MoveTransaction2(Transaction source, Transaction target)
  {
    target.ScreenId = source.ScreenId;
    target.Trancode = source.Trancode;
    target.Desc = source.Desc;
    target.MenuInd = source.MenuInd;
  }

  private void UseCreateTransactionCommands()
  {
    var useImport = new CreateTransactionCommands.Import();
    var useExport = new CreateTransactionCommands.Export();

    MoveTransaction1(export.Transaction, useImport.Transaction);
    useImport.Command.Value = export.Group.Item.Command.Value;

    Call(CreateTransactionCommands.Execute, useImport, useExport);
  }

  private void UseDeleteTransactionCommands()
  {
    var useImport = new DeleteTransactionCommands.Import();
    var useExport = new DeleteTransactionCommands.Export();

    MoveTransaction1(export.Transaction, useImport.Transaction);
    useImport.Command.Value = export.Group.Item.Command.Value;

    Call(DeleteTransactionCommands.Execute, useImport, useExport);
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
        db.SetString(command, "cmdValue", export.Starting.Value);
      },
      (db, reader) =>
      {
        entities.ExistingCommand.Value = db.GetString(reader, 0);
        entities.ExistingCommand.Desc = db.GetNullableString(reader, 1);
        entities.ExistingCommand.Populated = true;

        return true;
      });
  }

  private bool ReadProfileAuthorization()
  {
    entities.ExistingProfileAuthorization.Populated = false;

    return Read("ReadProfileAuthorization",
      (db, command) =>
      {
        db.SetString(command, "fkTrnScreenid", export.Transaction.ScreenId);
        db.SetString(command, "fkTrnTrancode", export.Transaction.Trancode);
        db.SetString(command, "fkCmdValue", export.Group.Item.Command.Value);
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

  private bool ReadTransaction()
  {
    entities.ExistingTransaction.Populated = false;

    return Read("ReadTransaction",
      (db, command) =>
      {
        db.SetString(command, "screenId", export.Transaction.ScreenId);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.NextTranAuthorization =
          db.GetString(reader, 4);
        entities.ExistingTransaction.Populated = true;
      });
  }

  private bool ReadTransactionCommand()
  {
    entities.ExistingTransactionCommand.Populated = false;

    return Read("ReadTransactionCommand",
      (db, command) =>
      {
        db.SetString(command, "fkCmdValue", entities.ExistingCommand.Value);
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
        entities.ExistingTransactionCommand.FkTrnTrancode =
          db.GetString(reader, 2);
        entities.ExistingTransactionCommand.FkCmdValue =
          db.GetString(reader, 3);
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of Command.
      /// </summary>
      [JsonPropertyName("command")]
      public Command Command
      {
        get => command ??= new();
        set => command = value;
      }

      /// <summary>
      /// A value of Active.
      /// </summary>
      [JsonPropertyName("active")]
      public Common Active
      {
        get => active ??= new();
        set => active = value;
      }

      /// <summary>
      /// A value of HiddenPrevActive.
      /// </summary>
      [JsonPropertyName("hiddenPrevActive")]
      public Common HiddenPrevActive
      {
        get => hiddenPrevActive ??= new();
        set => hiddenPrevActive = value;
      }

      /// <summary>
      /// A value of HiddenAction.
      /// </summary>
      [JsonPropertyName("hiddenAction")]
      public Common HiddenAction
      {
        get => hiddenAction ??= new();
        set => hiddenAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 300;

      private Common common;
      private Command command;
      private Common active;
      private Common hiddenPrevActive;
      private Common hiddenAction;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Transaction HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public Transaction HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    /// <summary>
    /// A value of PromptTransaction.
    /// </summary>
    [JsonPropertyName("promptTransaction")]
    public Standard PromptTransaction
    {
      get => promptTransaction ??= new();
      set => promptTransaction = value;
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

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Command Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    private Transaction hiddenPrev;
    private Array<GroupGroup> group;
    private Transaction transaction;
    private Transaction hiddenSelected;
    private Standard promptTransaction;
    private NextTranInfo hidden;
    private Standard standard;
    private Command starting;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of Command.
      /// </summary>
      [JsonPropertyName("command")]
      public Command Command
      {
        get => command ??= new();
        set => command = value;
      }

      /// <summary>
      /// A value of Active.
      /// </summary>
      [JsonPropertyName("active")]
      public Common Active
      {
        get => active ??= new();
        set => active = value;
      }

      /// <summary>
      /// A value of HiddenPrevActive.
      /// </summary>
      [JsonPropertyName("hiddenPrevActive")]
      public Common HiddenPrevActive
      {
        get => hiddenPrevActive ??= new();
        set => hiddenPrevActive = value;
      }

      /// <summary>
      /// A value of HiddenAction.
      /// </summary>
      [JsonPropertyName("hiddenAction")]
      public Common HiddenAction
      {
        get => hiddenAction ??= new();
        set => hiddenAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 300;

      private Common common;
      private Command command;
      private Common active;
      private Common hiddenPrevActive;
      private Common hiddenAction;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Transaction HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
    }

    /// <summary>
    /// A value of PromptTransaction.
    /// </summary>
    [JsonPropertyName("promptTransaction")]
    public Standard PromptTransaction
    {
      get => promptTransaction ??= new();
      set => promptTransaction = value;
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

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Command Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    private Transaction hiddenPrev;
    private Array<GroupGroup> group;
    private Transaction transaction;
    private Standard promptTransaction;
    private NextTranInfo hidden;
    private Standard standard;
    private Command starting;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Transaction Prev
    {
      get => prev ??= new();
      set => prev = value;
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

    private Transaction prev;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private ProfileAuthorization existingProfileAuthorization;
    private TransactionCommand existingTransactionCommand;
    private Command existingCommand;
    private Transaction existingTransaction;
  }
#endregion
}
