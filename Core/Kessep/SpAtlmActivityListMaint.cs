// Program: SP_ATLM_ACTIVITY_LIST_MAINT, ID: 371745500, model: 746.
// Short name: SWEATLMP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_ATLM_ACTIVITY_LIST_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpAtlmActivityListMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_ATLM_ACTIVITY_LIST_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpAtlmActivityListMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpAtlmActivityListMaint.
  /// </summary>
  public SpAtlmActivityListMaint(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----- Start Of Maintenance History -------------------------
    // Date	 Developer 	Request #    Description
    // 11/04/96 Alan Samuels                Initial Development
    // 11/10/98 SWSRKEH                     Phase II Changes
    // ----- End Of Maintenance History ---------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Hidden.Assign(import.Hidden);
    export.HiddenExportManualList.Flag = import.HiddenImportManualList.Flag;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    export.Starting.Name = import.Starting.Name;

    switch(TrimEnd(global.Command))
    {
      case "XXNEXTXX":
        // ---------------------------------------------
        // User entered this screen from another screen
        // ---------------------------------------------
        UseScCabNextTranGet();

        // ---------------------------------------------
        // Populate export views from local next_tran_info view read from the 
        // data base
        // Set command to initial command required or ESCAPE
        // ---------------------------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;

          return;
        }

        export.Hidden.Assign(local.NextTranInfo);

        return;
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          local.NextTranInfo.Assign(import.Hidden);
          UseScCabNextTranPut();

          // ---------------------------------------------
          // Set up local next_tran_info for saving the current values for the 
          // next screen
          // ---------------------------------------------
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.Standard, "nextTransaction");

            field.Error = true;

            break;
          }

          return;
        }

        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

        break;
      case "DISPLAY":
        break;
      case "EXIT":
        // ---------------------------------------------
        // If list is a 'manual activity list", this
        // command is not allowed.
        // ---------------------------------------------
        if (!IsEmpty(import.HiddenImportManualList.Flag))
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

            export.Group.Update.Activity.Assign(import.Group.Item.Activity);

            var field1 = GetField(export.Group.Item.Activity, "controlNumber");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.Activity, "typeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            export.Group.Update.Common.SelectChar =
              import.Group.Item.Common.SelectChar;
            export.Group.Update.Hidden.Assign(import.Group.Item.Hidden);

            if (!IsEmpty(export.Group.Item.Common.SelectChar))
            {
              if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;
              }
            }

            export.Group.Next();
          }

          ExitState = "SP0000_MANUAL_LIST_ONLY";

          return;
        }

        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "ADD":
        break;
      case "UPDATE":
        break;
      case "DELETE":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "ADLM":
        // ---------------------------------------------
        // If list is a 'manual activity list", this
        // command is not allowed.
        // ---------------------------------------------
        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.Activity.Assign(import.Group.Item.Activity);

          var field1 = GetField(export.Group.Item.Activity, "controlNumber");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Group.Item.Activity, "typeCode");

          field2.Color = "cyan";
          field2.Protected = true;

          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;
          export.Group.Update.Hidden.Assign(import.Group.Item.Hidden);

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            ++local.Select.Count;
            export.ToTran.ControlNumber =
              export.Group.Item.Activity.ControlNumber;

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;
          }

          export.Group.Next();
        }

        if (!IsEmpty(import.HiddenImportManualList.Flag))
        {
          ExitState = "SP0000_MANUAL_LIST_ONLY";

          return;
        }

        switch(local.Select.Count)
        {
          case 0:
            ExitState = "SP0000_REQUEST_REQUIRES_SEL";

            break;
          case 1:
            ExitState = "ECO_XFR_TO_ADLM";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        return;
      case "RETURN":
        // ---------------------------------------------
        // Return back on a link to the calling
        // procedure.  A selection is not required, but
        // if made, only one selection is allowed.
        // ---------------------------------------------
        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.Activity.Assign(import.Group.Item.Activity);

          var field1 = GetField(export.Group.Item.Activity, "controlNumber");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Group.Item.Activity, "typeCode");

          field2.Color = "cyan";
          field2.Protected = true;

          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;
          export.Group.Update.Hidden.Assign(import.Group.Item.Hidden);

          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ++local.Select.Count;
              export.ToTran.Assign(export.Group.Item.Activity);
            }
          }

          export.Group.Next();
        }

        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_RETURN";

            break;
          case 1:
            ExitState = "ACO_NE0000_RETURN";

            break;
          default:
            ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTION";

            break;
        }

        return;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY1";

        break;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // ---------------------------------------------
    // Move group views if command <> display.
    // ---------------------------------------------
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

        export.Group.Update.Activity.Assign(import.Group.Item.Activity);

        var field1 = GetField(export.Group.Item.Activity, "controlNumber");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Group.Item.Activity, "typeCode");

        field2.Color = "cyan";
        field2.Protected = true;

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          ++local.Select.Count;

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;
        }

        export.Group.Update.Hidden.Assign(import.Group.Item.Hidden);
        export.Group.Next();
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE"))
    {
      switch(local.Select.Count)
      {
        case 0:
          ExitState = "SP0000_REQUEST_REQUIRES_SEL";

          break;
        case 1:
          break;
        default:
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

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

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // -----------------------------------------
        // If this is a "manual activity list", only
        // display manual types.
        // -----------------------------------------
        if (!IsEmpty(import.HiddenImportManualList.Flag))
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadActivity2())
          {
            export.Group.Update.Activity.Assign(entities.Activity);
            export.Group.Update.Hidden.Assign(entities.Activity);
            export.Group.Next();
          }
        }
        else
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadActivity3())
          {
            export.Group.Update.Activity.Assign(entities.Activity);

            var field1 = GetField(export.Group.Item.Activity, "controlNumber");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.Activity, "typeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            export.Group.Update.Hidden.Assign(entities.Activity);
            export.Group.Next();
          }
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "ADD":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            local.Errors.Count = 0;

            // -----------------------------------------
            // An ADD must be on a blank row.
            // -----------------------------------------
            if (export.Group.Item.Hidden.ControlNumber != 0)
            {
              ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

              return;
            }

            // -----------------------------------------
            // Perform data validation for add request.
            // -----------------------------------------
            // -----------------------------------------
            // If list is a 'manual activity list', ADD is not allowed
            // -----------------------------------------
            if (!IsEmpty(import.HiddenImportManualList.Flag))
            {
              ExitState = "SP0000_MANUAL_LIST_ONLY";

              return;
            }

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;

            // -----------------------------------------
            // Description, type and name are required.
            // -----------------------------------------
            if (IsEmpty(export.Group.Item.Activity.Description))
            {
              var field1 = GetField(export.Group.Item.Activity, "description");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }

            // -----------------------------------------
            // Type code must be 'AUT or MAN.
            // -----------------------------------------
            switch(TrimEnd(export.Group.Item.Activity.TypeCode))
            {
              case "":
                var field1 = GetField(export.Group.Item.Activity, "typeCode");

                field1.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";

                break;
              case "AUT":
                break;
              case "MAN":
                break;
              default:
                var field2 = GetField(export.Group.Item.Activity, "typeCode");

                field2.Error = true;

                ExitState = "SP0000_INVALID_TYPE_CODE";

                break;
            }

            if (IsEmpty(export.Group.Item.Activity.Name))
            {
              var field1 = GetField(export.Group.Item.Activity, "name");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }
            else
            {
              // -----------------------------------------
              // Name nust be unique.
              // -----------------------------------------
              if (ReadActivity1())
              {
                var field1 = GetField(export.Group.Item.Activity, "name");

                field1.Error = true;

                ExitState = "SP0000_NAME_ALREADY_EXISTS";
              }
              else
              {
                // -----------------------------------------
                // Continue
                // -----------------------------------------
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK") && !
              IsExitState("ACO_NI0000_ADD_SUCCESSFUL"))
            {
              return;
            }

            // -----------------------------------------
            // Data passed validation. Create Activity.
            // -----------------------------------------
            local.PassTo.Assign(export.Group.Item.Activity);
            UseSpCabCreateActivity();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_ADD_SUCCESSFUL"))
            {
              ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
              export.Group.Update.Common.SelectChar = "";
            }
            else
            {
              return;
            }

            export.Group.Update.Activity.Assign(local.ReturnFrom);
            export.Group.Update.Hidden.Assign(local.ReturnFrom);

            return;
          }
        }

        break;
      case "UPDATE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            // -----------------------------------------
            // AN update must be performed on a populated row.
            // -----------------------------------------
            if (export.Group.Item.Activity.ControlNumber == 0)
            {
              ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

              return;
            }

            // -----------------------------------------
            // Perform data validation for add request.
            // -----------------------------------------
            // -----------------------------------------
            // If list is a 'manual activity list', UPDATE is not allowed
            // -----------------------------------------
            if (!IsEmpty(import.HiddenImportManualList.Flag))
            {
              ExitState = "SP0000_MANUAL_LIST_ONLY";

              return;
            }

            // -----------------------------------------
            // Name and/or description must have changed
            // -----------------------------------------
            if (Equal(export.Group.Item.Activity.Description,
              export.Group.Item.Hidden.Description) && Equal
              (export.Group.Item.Activity.Name, export.Group.Item.Hidden.Name))
            {
              ExitState = "SP0000_DATA_NOT_CHANGED";

              return;
            }

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;

            // -----------------------------------------
            // Description is required
            // -----------------------------------------
            if (IsEmpty(export.Group.Item.Activity.Description))
            {
              var field1 = GetField(export.Group.Item.Activity, "description");

              field1.Error = true;

              ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            }

            // -----------------------------------------
            // Type code cannot be changed.
            // -----------------------------------------
            if (!Equal(export.Group.Item.Activity.TypeCode,
              export.Group.Item.Hidden.TypeCode))
            {
              var field1 = GetField(export.Group.Item.Activity, "typeCode");

              field1.Error = true;

              ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            }

            // -----------------------------------------
            // If name is changed, it must be unique.
            // Name is required.
            // -----------------------------------------
            if (!Equal(export.Group.Item.Activity.Name,
              export.Group.Item.Hidden.Name))
            {
              if (IsEmpty(export.Group.Item.Activity.Name))
              {
                var field1 = GetField(export.Group.Item.Activity, "name");

                field1.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else
              {
                // -----------------------------------------
                // Name nust be unique.
                // -----------------------------------------
                if (ReadActivity1())
                {
                  var field1 = GetField(export.Group.Item.Activity, "name");

                  field1.Error = true;

                  ExitState = "SP0000_NAME_ALREADY_EXISTS";
                }
                else
                {
                  // -----------------------------------------
                  // Continue
                  // -----------------------------------------
                }
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // -----------------------------------------
            // Data passed validation. Update Activity.
            // -----------------------------------------
            local.PassTo.Assign(export.Group.Item.Activity);
            UseSpCabUpdateActivity();

            if (!IsExitState("ACO_NN0000_ALL_OK") && !
              IsExitState("ACO_NI0000_UPDATE_SUCCESSFUL"))
            {
              return;
            }

            ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.Activity.Assign(local.ReturnFrom);
            export.Group.Update.Hidden.Assign(local.ReturnFrom);

            return;
          }
        }

        break;
      case "DELETE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            // -----------------------------------------
            // A delete must be performed on a populated row.
            // -----------------------------------------
            if (export.Group.Item.Activity.ControlNumber == 0)
            {
              ExitState = "SP0000_DELETE_ON_EMPTY_ROW";

              return;
            }

            // -----------------------------------------
            // Perform data validation for add request.
            // -----------------------------------------
            // -----------------------------------------
            // If list is a 'manual activity list', ADD is not allowed
            // -----------------------------------------
            if (!IsEmpty(import.HiddenImportManualList.Flag))
            {
              ExitState = "SP0000_MANUAL_LIST_ONLY";

              return;
            }

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;

            local.PassTo.Assign(export.Group.Item.Activity);
            UseSpCabDeleteActivity();

            if (!IsExitState("ACO_NN0000_ALL_OK") && !
              IsExitState("ACO_NI0000_DELETE_SUCCESSFUL"))
            {
              return;
            }

            ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.Activity.Assign(local.ReturnFrom);
            export.Group.Update.Hidden.Assign(local.ReturnFrom);

            return;
          }
        }

        break;
      default:
        break;
    }
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

  private void UseSpCabCreateActivity()
  {
    var useImport = new SpCabCreateActivity.Import();
    var useExport = new SpCabCreateActivity.Export();

    useImport.Activity.Assign(local.PassTo);

    Call(SpCabCreateActivity.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.Activity);
  }

  private void UseSpCabDeleteActivity()
  {
    var useImport = new SpCabDeleteActivity.Import();
    var useExport = new SpCabDeleteActivity.Export();

    useImport.Activity.ControlNumber = local.PassTo.ControlNumber;

    Call(SpCabDeleteActivity.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateActivity()
  {
    var useImport = new SpCabUpdateActivity.Import();
    var useExport = new SpCabUpdateActivity.Export();

    useImport.Activity.Assign(local.PassTo);

    Call(SpCabUpdateActivity.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.Activity);
  }

  private bool ReadActivity1()
  {
    entities.Activity.Populated = false;

    return Read("ReadActivity1",
      (db, command) =>
      {
        db.SetString(command, "name", export.Group.Item.Activity.Name);
      },
      (db, reader) =>
      {
        entities.Activity.ControlNumber = db.GetInt32(reader, 0);
        entities.Activity.Name = db.GetString(reader, 1);
        entities.Activity.TypeCode = db.GetString(reader, 2);
        entities.Activity.Description = db.GetNullableString(reader, 3);
        entities.Activity.Populated = true;
      });
  }

  private IEnumerable<bool> ReadActivity2()
  {
    return ReadEach("ReadActivity2",
      (db, command) =>
      {
        db.SetString(command, "name", import.Starting.Name);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Activity.ControlNumber = db.GetInt32(reader, 0);
        entities.Activity.Name = db.GetString(reader, 1);
        entities.Activity.TypeCode = db.GetString(reader, 2);
        entities.Activity.Description = db.GetNullableString(reader, 3);
        entities.Activity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadActivity3()
  {
    return ReadEach("ReadActivity3",
      (db, command) =>
      {
        db.SetString(command, "name", import.Starting.Name);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.Activity.ControlNumber = db.GetInt32(reader, 0);
        entities.Activity.Name = db.GetString(reader, 1);
        entities.Activity.TypeCode = db.GetString(reader, 2);
        entities.Activity.Description = db.GetNullableString(reader, 3);
        entities.Activity.Populated = true;

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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Activity.
      /// </summary>
      [JsonPropertyName("activity")]
      public Activity Activity
      {
        get => activity ??= new();
        set => activity = value;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public Activity Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 47;

      private Activity activity;
      private Common common;
      private Activity hidden;
    }

    /// <summary>
    /// A value of HiddenImportManualList.
    /// </summary>
    [JsonPropertyName("hiddenImportManualList")]
    public Common HiddenImportManualList
    {
      get => hiddenImportManualList ??= new();
      set => hiddenImportManualList = value;
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
    public Activity Starting
    {
      get => starting ??= new();
      set => starting = value;
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

    private Common hiddenImportManualList;
    private Standard standard;
    private Activity starting;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
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
      /// A value of Activity.
      /// </summary>
      [JsonPropertyName("activity")]
      public Activity Activity
      {
        get => activity ??= new();
        set => activity = value;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public Activity Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 47;

      private Activity activity;
      private Common common;
      private Activity hidden;
    }

    /// <summary>
    /// A value of ToTran.
    /// </summary>
    [JsonPropertyName("toTran")]
    public Activity ToTran
    {
      get => toTran ??= new();
      set => toTran = value;
    }

    /// <summary>
    /// A value of HiddenExportManualList.
    /// </summary>
    [JsonPropertyName("hiddenExportManualList")]
    public Common HiddenExportManualList
    {
      get => hiddenExportManualList ??= new();
      set => hiddenExportManualList = value;
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
    public Activity Starting
    {
      get => starting ??= new();
      set => starting = value;
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

    private Activity toTran;
    private Common hiddenExportManualList;
    private Standard standard;
    private Activity starting;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Errors.
    /// </summary>
    [JsonPropertyName("errors")]
    public Common Errors
    {
      get => errors ??= new();
      set => errors = value;
    }

    /// <summary>
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

    /// <summary>
    /// A value of ReturnFrom.
    /// </summary>
    [JsonPropertyName("returnFrom")]
    public Activity ReturnFrom
    {
      get => returnFrom ??= new();
      set => returnFrom = value;
    }

    /// <summary>
    /// A value of PassTo.
    /// </summary>
    [JsonPropertyName("passTo")]
    public Activity PassTo
    {
      get => passTo ??= new();
      set => passTo = value;
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

    private Common errors;
    private Common select;
    private Activity returnFrom;
    private Activity passTo;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private Activity activity;
  }
#endregion
}
