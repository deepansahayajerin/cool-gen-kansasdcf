// Program: SC_CMDS_MAINTAIN_COMMANDS, ID: 371452758, model: 746.
// Short name: SWECMDSP
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
/// A program: SC_CMDS_MAINTAIN_COMMANDS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class ScCmdsMaintainCommands: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CMDS_MAINTAIN_COMMANDS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCmdsMaintainCommands(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCmdsMaintainCommands.
  /// </summary>
  public ScCmdsMaintainCommands(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer      request #   Description
    // 12/10/95 Alan Hackler                Initial Development
    // 12/12/96 R. Marchman                 Add new security/next tran
    // 03/02/02 Vithal Madhira	PR# 139574   The security CAB '
    // SC_CAB_TEST_SECURITY' is not
    // 				     getting executed. Fixed the IF loop.
    // 01/13/20 GVandy		CQ67034	     Increase group view size and add starting
    // 				     command search value.
    // -----------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Starting.Value = import.Starting.Value;

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        MoveCommand(import.Group.Item.Command, export.Group.Update.Command);

        var field = GetField(export.Group.Item.Command, "value");

        field.Color = "cyan";
        field.Protected = true;

        export.Group.Next();
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "RETURN"))
    {
      export.HiddenSelected.Index = -1;

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case 'S':
            ++export.HiddenSelected.Index;
            export.HiddenSelected.CheckSize();

            export.HiddenSelected.Update.Hidden.Value =
              export.Group.Item.Command.Value;
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

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
        (global.Command, "DELETE"))
      {
        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
      }

      if (Equal(global.Command, "DELETE"))
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            if (ReadTransactionCommand())
            {
              var field1 = GetField(export.Group.Item.Command, "value");

              field1.Error = true;

              var field2 = GetField(export.Group.Item.Common, "selectChar");

              field2.Error = true;

              ExitState = "SC0013_COMMAND_IN_USE_NOT_DELETE";
            }
          }
        }
      }

      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            if (IsEmpty(export.Group.Item.Command.Value))
            {
              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (IsEmpty(export.Group.Item.Command.Desc))
            {
              export.Group.Update.Command.Desc =
                export.Group.Item.Command.Value;
            }
          }
        }
      }
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
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
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
        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadCommand())
        {
          MoveCommand(entities.ExistingCommand, export.Group.Update.Command);

          var field = GetField(export.Group.Item.Command, "value");

          field.Color = "cyan";
          field.Protected = true;

          export.Group.Next();
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        break;
      case "ADD":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseCreateCommand();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                var field = GetField(export.Group.Item.Command, "value");

                field.Error = true;
              }
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseUpdateCommand();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                var field = GetField(export.Group.Item.Command, "value");

                field.Error = true;
              }
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "DELETE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseDeleteCommand();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                var field = GetField(export.Group.Item.Command, "value");

                field.Error = true;
              }
            }
          }
        }

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

  private void UseCreateCommand()
  {
    var useImport = new CreateCommand.Import();
    var useExport = new CreateCommand.Export();

    MoveCommand(export.Group.Item.Command, useImport.Command);

    Call(CreateCommand.Execute, useImport, useExport);
  }

  private void UseDeleteCommand()
  {
    var useImport = new DeleteCommand.Import();
    var useExport = new DeleteCommand.Export();

    useImport.Command.Value = export.Group.Item.Command.Value;

    Call(DeleteCommand.Execute, useImport, useExport);
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

  private void UseUpdateCommand()
  {
    var useImport = new UpdateCommand.Import();
    var useExport = new UpdateCommand.Export();

    MoveCommand(export.Group.Item.Command, useImport.Command);

    Call(UpdateCommand.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCommand()
  {
    return ReadEach("ReadCommand",
      (db, command) =>
      {
        db.SetString(command, "cmdValue", export.Starting.Value);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingCommand.Value = db.GetString(reader, 0);
        entities.ExistingCommand.Desc = db.GetNullableString(reader, 1);
        entities.ExistingCommand.Populated = true;

        return true;
      });
  }

  private bool ReadTransactionCommand()
  {
    entities.ExistingTransactionCommand.Populated = false;

    return Read("ReadTransactionCommand",
      (db, command) =>
      {
        db.SetString(command, "fkCmdValue", export.Group.Item.Command.Value);
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 300;

      private Common common;
      private Command command;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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

    private Array<GroupGroup> group;
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
    /// <summary>A HiddenSelectedGroup group.</summary>
    [Serializable]
    public class HiddenSelectedGroup
    {
      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public Command Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 300;

      private Command hidden;
    }

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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 300;

      private Common common;
      private Command command;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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

    private Array<HiddenSelectedGroup> hiddenSelected;
    private Array<GroupGroup> group;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common common;
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
    /// A value of ExistingTransactionCommand.
    /// </summary>
    [JsonPropertyName("existingTransactionCommand")]
    public TransactionCommand ExistingTransactionCommand
    {
      get => existingTransactionCommand ??= new();
      set => existingTransactionCommand = value;
    }

    private Command existingCommand;
    private TransactionCommand existingTransactionCommand;
  }
#endregion
}
