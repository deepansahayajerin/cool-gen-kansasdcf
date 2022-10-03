// Program: SP_LSLM_LIFE_CYCLE_STATE_L_MAINT, ID: 371779136, model: 746.
// Short name: SWELSLMP
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
/// A program: SP_LSLM_LIFE_CYCLE_STATE_L_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpLslmLifeCycleStateLMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_LSLM_LIFE_CYCLE_STATE_L_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpLslmLifeCycleStateLMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpLslmLifeCycleStateLMaint.
  /// </summary>
  public SpLslmLifeCycleStateLMaint(IContext context, Import import,
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
    // Date	 Developer 	Request #    Description
    // 11/10/96 Alan Samuels                Initial Development
    // 09/20/98  Anita Massey            Fixes per screen
    //                                     
    // assessment form
    // 06/07/99  Anita Massey          review read property and
    //                                        
    // changed to select only
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }
    else if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }
    else if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.Hidden.Assign(local.NextTranInfo);

      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      local.NextTranInfo.Assign(import.Hidden);

      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
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

    export.Starting.Identifier = import.Starting.Identifier;
    export.HiddenExportStarting.Identifier =
      import.HiddenImportStarting.Identifier;
    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
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

        export.Group.Update.LifecycleState.Description =
          import.Group.Item.LifecycleState.Description;
        MoveLifecycleState(import.Group.Item.Hidden, export.Group.Update.Hidden);
          
        export.Group.Update.Ap.OneChar = import.Group.Item.Ap.OneChar;
        export.Group.Update.Cuf.OneChar = import.Group.Item.Cuf.OneChar;
        export.Group.Update.Loc.OneChar = import.Group.Item.Loc.OneChar;
        export.Group.Update.Obl.OneChar = import.Group.Item.Obl.OneChar;
        export.Group.Update.SvcType.OneChar = import.Group.Item.SvcType.OneChar;
        export.Group.Update.HiddenExportGrpAp.OneChar =
          import.Group.Item.HiddenImportGrpAp.OneChar;
        export.Group.Update.HiddenExportGrpCuf.OneChar =
          import.Group.Item.HiddenImportGrpCuf.OneChar;
        export.Group.Update.HiddenExportGrpLoc.OneChar =
          import.Group.Item.HiddenImportGrpLoc.OneChar;
        export.Group.Update.HiddenExportGrpObl.OneChar =
          import.Group.Item.HiddenImportGrpObl.OneChar;
        export.Group.Update.HiddenExportGrpSvcType.OneChar =
          import.Group.Item.HiddenImportGrpSvcType.OneChar;
        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        export.Group.Next();
      }
    }

    // ---------------------------------------------
    // Display first before any maintenance.
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      if (Equal(import.Starting.Identifier,
        import.HiddenImportStarting.Identifier))
      {
      }
      else
      {
        ExitState = "ACO_NE0000_DISPLAY_FIRST";

        var field = GetField(export.Starting, "identifier");

        field.Error = true;

        return;
      }

      local.Common.Count = 0;

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (AsChar(import.Group.Item.Common.SelectChar) == 'S')
        {
          ++local.Common.Count;
        }
      }

      if (local.Common.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadLifecycleState())
        {
          MoveLifecycleState(entities.LifecycleState, export.Group.Update.Hidden);
            
          export.Group.Update.LifecycleState.Description =
            entities.LifecycleState.Description;
          export.Group.Update.Cuf.OneChar =
            Substring(entities.LifecycleState.Identifier, 1, 1);
          export.Group.Update.SvcType.OneChar =
            Substring(entities.LifecycleState.Identifier, 2, 1);
          export.Group.Update.Loc.OneChar =
            Substring(entities.LifecycleState.Identifier, 3, 1);
          export.Group.Update.Ap.OneChar =
            Substring(entities.LifecycleState.Identifier, 4, 1);
          export.Group.Update.Obl.OneChar =
            Substring(entities.LifecycleState.Identifier, 5, 1);
          export.Group.Update.HiddenExportGrpCuf.OneChar =
            export.Group.Item.Cuf.OneChar;
          export.Group.Update.HiddenExportGrpSvcType.OneChar =
            export.Group.Item.SvcType.OneChar;
          export.Group.Update.HiddenExportGrpLoc.OneChar =
            export.Group.Item.Loc.OneChar;
          export.Group.Update.HiddenExportGrpAp.OneChar =
            export.Group.Item.Ap.OneChar;
          export.Group.Update.HiddenExportGrpObl.OneChar =
            export.Group.Item.Obl.OneChar;
          export.Group.Next();
        }

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        export.HiddenExportStarting.Identifier = import.Starting.Identifier;

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "ADD":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // An add must be on a previously blank row.
              // ---------------------------------------------
              if (!IsEmpty(export.Group.Item.HiddenExportGrpCuf.OneChar))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

                return;
              }

              // ---------------------------------------------
              // Perform data validation. All ID code fields
              // are required.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.Obl.OneChar))
              {
                var field = GetField(export.Group.Item.Obl, "oneChar");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (AsChar(export.Group.Item.Obl.OneChar) == 'Y' || AsChar
                (export.Group.Item.Obl.OneChar) == 'N' || AsChar
                (export.Group.Item.Obl.OneChar) == 'U')
              {
              }
              else
              {
                var field = GetField(export.Group.Item.Obl, "oneChar");

                field.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }

              if (IsEmpty(export.Group.Item.Ap.OneChar))
              {
                var field = GetField(export.Group.Item.Ap, "oneChar");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (AsChar(export.Group.Item.Ap.OneChar) == 'Y' || AsChar
                (export.Group.Item.Ap.OneChar) == 'N' || AsChar
                (export.Group.Item.Ap.OneChar) == 'U')
              {
              }
              else
              {
                var field = GetField(export.Group.Item.Ap, "oneChar");

                field.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }

              if (IsEmpty(export.Group.Item.Loc.OneChar))
              {
                var field = GetField(export.Group.Item.Loc, "oneChar");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (AsChar(export.Group.Item.Loc.OneChar) == 'Y' || AsChar
                (export.Group.Item.Loc.OneChar) == 'N')
              {
              }
              else
              {
                var field = GetField(export.Group.Item.Loc, "oneChar");

                field.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }

              if (IsEmpty(export.Group.Item.SvcType.OneChar))
              {
                var field = GetField(export.Group.Item.SvcType, "oneChar");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (AsChar(export.Group.Item.SvcType.OneChar) == 'F' || AsChar
                (export.Group.Item.SvcType.OneChar) == 'E' || AsChar
                (export.Group.Item.SvcType.OneChar) == 'L')
              {
              }
              else
              {
                var field = GetField(export.Group.Item.SvcType, "oneChar");

                field.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }

              if (IsEmpty(export.Group.Item.Cuf.OneChar))
              {
                var field = GetField(export.Group.Item.Cuf, "oneChar");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (AsChar(export.Group.Item.Cuf.OneChar) == 'L' || AsChar
                (export.Group.Item.Cuf.OneChar) == 'P' || AsChar
                (export.Group.Item.Cuf.OneChar) == 'O' || AsChar
                (export.Group.Item.Cuf.OneChar) == 'E')
              {
              }
              else
              {
                var field = GetField(export.Group.Item.Cuf, "oneChar");

                field.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }

              if (IsEmpty(export.Group.Item.LifecycleState.Description))
              {
                var field =
                  GetField(export.Group.Item.LifecycleState, "description");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
            }
          }
          else
          {
            continue;
          }

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_ADD_SUCCESSFUL"))
          {
          }
          else
          {
            return;
          }

          // ---------------------------------------------
          // Data has passed validation. Create
          // Lifecycle State.
          // ---------------------------------------------
          local.PassTo.Description =
            export.Group.Item.LifecycleState.Description ?? "";
          local.PassTo.Identifier = export.Group.Item.Cuf.OneChar + export
            .Group.Item.SvcType.OneChar + export.Group.Item.Loc.OneChar + export
            .Group.Item.Ap.OneChar + export.Group.Item.Obl.OneChar;
          UseSpCabCreateLifeCycleState();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_ADD_SUCCESSFUL"))
          {
            ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.HiddenExportGrpObl.OneChar =
              export.Group.Item.Obl.OneChar;
            export.Group.Update.HiddenExportGrpAp.OneChar =
              export.Group.Item.Ap.OneChar;
            export.Group.Update.HiddenExportGrpLoc.OneChar =
              export.Group.Item.Loc.OneChar;
            export.Group.Update.HiddenExportGrpSvcType.OneChar =
              export.Group.Item.SvcType.OneChar;
            export.Group.Update.HiddenExportGrpCuf.OneChar =
              export.Group.Item.Cuf.OneChar;
            global.Command = "DISPLAY";
          }
          else
          {
            return;
          }
        }

        break;
      case "UPDATE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // An update must be performed on a populated
              // row.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.HiddenExportGrpCuf.OneChar))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

                return;
              }

              // ---------------------------------------------
              // Perform data validation for update request.
              // ---------------------------------------------
              // ---------------------------------------------
              // No identifying codes can be changed.
              // ---------------------------------------------
              if (AsChar(export.Group.Item.Obl.OneChar) == AsChar
                (export.Group.Item.HiddenExportGrpObl.OneChar))
              {
              }
              else
              {
                export.Group.Update.Obl.OneChar =
                  export.Group.Item.HiddenExportGrpObl.OneChar;

                var field = GetField(export.Group.Item.Obl, "oneChar");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              if (AsChar(export.Group.Item.Ap.OneChar) == AsChar
                (export.Group.Item.HiddenExportGrpAp.OneChar))
              {
              }
              else
              {
                export.Group.Update.Ap.OneChar =
                  export.Group.Item.HiddenExportGrpAp.OneChar;

                var field = GetField(export.Group.Item.Ap, "oneChar");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              if (AsChar(export.Group.Item.Loc.OneChar) == AsChar
                (export.Group.Item.HiddenExportGrpLoc.OneChar))
              {
              }
              else
              {
                export.Group.Update.Loc.OneChar =
                  export.Group.Item.HiddenExportGrpLoc.OneChar;

                var field = GetField(export.Group.Item.Loc, "oneChar");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              if (AsChar(export.Group.Item.SvcType.OneChar) == AsChar
                (export.Group.Item.HiddenExportGrpSvcType.OneChar))
              {
              }
              else
              {
                export.Group.Update.SvcType.OneChar =
                  export.Group.Item.HiddenExportGrpSvcType.OneChar;

                var field = GetField(export.Group.Item.SvcType, "oneChar");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              if (AsChar(export.Group.Item.Cuf.OneChar) == AsChar
                (export.Group.Item.HiddenExportGrpCuf.OneChar))
              {
              }
              else
              {
                export.Group.Update.Cuf.OneChar =
                  export.Group.Item.HiddenExportGrpCuf.OneChar;

                var field = GetField(export.Group.Item.Cuf, "oneChar");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              // ---------------------------------------------
              // Description is the only updateable field.
              // ---------------------------------------------
              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (Equal(export.Group.Item.LifecycleState.Description,
                  export.Group.Item.Hidden.Description))
                {
                  var field1 = GetField(export.Group.Item.Common, "selectChar");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Group.Item.LifecycleState, "description");

                  field2.Error = true;

                  ExitState = "SP0000_DATA_NOT_CHANGED";
                }
                else if (IsEmpty(export.Group.Item.LifecycleState.Description))
                {
                  var field =
                    GetField(export.Group.Item.LifecycleState, "description");

                  field.Error = true;

                  ExitState = "SP0000_REQUIRED_FIELD_MISSING";
                }
              }
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
            }
          }
          else
          {
            continue;
          }

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_UPDATE_SUCCESSFUL"))
          {
          }
          else
          {
            return;
          }

          // ---------------------------------------------
          // Data has passed validation. Update
          // Lifecycle State.
          // ---------------------------------------------
          local.PassTo.Description =
            export.Group.Item.LifecycleState.Description ?? "";
          local.PassTo.Identifier = export.Group.Item.Hidden.Identifier;
          UseSpCabUpdateLifecycleState();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_UPDATE_SUCCESSFUL"))
          {
            ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.Hidden.Description =
              export.Group.Item.LifecycleState.Description ?? "";
          }
          else
          {
            return;
          }
        }

        break;
      case "DELETE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // A delete must be performed on a populated
              // row.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.HiddenExportGrpCuf.OneChar))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_DELETE_ON_EMPTY_ROW";

                return;
              }

              // ---------------------------------------------
              // A delete may not be performed if any related
              // transformation records exist.
              // ---------------------------------------------
              if (ReadLifecycleTransformation())
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_RELATED_DETAILS_EXIST";

                return;
              }
              else
              {
                local.PassTo.Identifier = export.Group.Item.Hidden.Identifier;
                UseSpCabDeleteLifecycleState();
              }
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
            }
          }
          else
          {
            continue;
          }

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_DELETE_SUCCESSFUL"))
          {
            ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.Obl.OneChar = "";
            export.Group.Update.Ap.OneChar = "";
            export.Group.Update.Loc.OneChar = "";
            export.Group.Update.SvcType.OneChar = "";
            export.Group.Update.Cuf.OneChar = "";
            export.Group.Update.HiddenExportGrpObl.OneChar = "";
            export.Group.Update.HiddenExportGrpAp.OneChar = "";
            export.Group.Update.HiddenExportGrpLoc.OneChar = "";
            export.Group.Update.HiddenExportGrpSvcType.OneChar = "";
            export.Group.Update.HiddenExportGrpCuf.OneChar = "";
            export.Group.Update.Hidden.Identifier = "";
            export.Group.Update.Hidden.Description = "";
            export.Group.Update.LifecycleState.Description = "";
          }
          else
          {
            return;
          }
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETURN":
        // ---------------------------------------------
        // Return back on a link to the calling
        // procedure.  A selection is not required, but
        // if made, only one selection is allowed.
        // ---------------------------------------------
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              ++local.Common.Count;
              export.ToTran.Identifier = export.Group.Item.Hidden.Identifier;
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              return;
            }
          }
        }

        if (local.Common.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      case "PREV":
        ExitState = "CO0000_INVALID_PAGING_REQUEST";

        break;
      case "NEXT":
        ExitState = "CO0000_INVALID_PAGING_REQUEST";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveLifecycleState(LifecycleState source,
    LifecycleState target)
  {
    target.Identifier = source.Identifier;
    target.Description = source.Description;
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

  private void UseSpCabCreateLifeCycleState()
  {
    var useImport = new SpCabCreateLifeCycleState.Import();
    var useExport = new SpCabCreateLifeCycleState.Export();

    MoveLifecycleState(local.PassTo, useImport.LifecycleState);

    Call(SpCabCreateLifeCycleState.Execute, useImport, useExport);
  }

  private void UseSpCabDeleteLifecycleState()
  {
    var useImport = new SpCabDeleteLifecycleState.Import();
    var useExport = new SpCabDeleteLifecycleState.Export();

    useImport.LifecycleState.Identifier = local.PassTo.Identifier;

    Call(SpCabDeleteLifecycleState.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateLifecycleState()
  {
    var useImport = new SpCabUpdateLifecycleState.Import();
    var useExport = new SpCabUpdateLifecycleState.Export();

    MoveLifecycleState(local.PassTo, useImport.LifecycleState);

    Call(SpCabUpdateLifecycleState.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadLifecycleState()
  {
    return ReadEach("ReadLifecycleState",
      (db, command) =>
      {
        db.SetString(command, "identifier", import.Starting.Identifier);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.LifecycleState.Identifier = db.GetString(reader, 0);
        entities.LifecycleState.Description = db.GetNullableString(reader, 1);
        entities.LifecycleState.Populated = true;

        return true;
      });
  }

  private bool ReadLifecycleTransformation()
  {
    entities.LifecycleTransformation.Populated = false;

    return Read("ReadLifecycleTransformation",
      (db, command) =>
      {
        db.
          SetString(command, "identifier", export.Group.Item.Hidden.Identifier);
          
      },
      (db, reader) =>
      {
        entities.LifecycleTransformation.Description = db.GetString(reader, 0);
        entities.LifecycleTransformation.LcsIdPri = db.GetString(reader, 1);
        entities.LifecycleTransformation.EveCtrlNoPri = db.GetInt32(reader, 2);
        entities.LifecycleTransformation.EvdIdPri = db.GetInt32(reader, 3);
        entities.LifecycleTransformation.LcsLctIdSec = db.GetString(reader, 4);
        entities.LifecycleTransformation.Populated = true;
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
      /// A value of HiddenImportGrpObl.
      /// </summary>
      [JsonPropertyName("hiddenImportGrpObl")]
      public Standard HiddenImportGrpObl
      {
        get => hiddenImportGrpObl ??= new();
        set => hiddenImportGrpObl = value;
      }

      /// <summary>
      /// A value of HiddenImportGrpAp.
      /// </summary>
      [JsonPropertyName("hiddenImportGrpAp")]
      public Standard HiddenImportGrpAp
      {
        get => hiddenImportGrpAp ??= new();
        set => hiddenImportGrpAp = value;
      }

      /// <summary>
      /// A value of HiddenImportGrpLoc.
      /// </summary>
      [JsonPropertyName("hiddenImportGrpLoc")]
      public Standard HiddenImportGrpLoc
      {
        get => hiddenImportGrpLoc ??= new();
        set => hiddenImportGrpLoc = value;
      }

      /// <summary>
      /// A value of HiddenImportGrpSvcType.
      /// </summary>
      [JsonPropertyName("hiddenImportGrpSvcType")]
      public Standard HiddenImportGrpSvcType
      {
        get => hiddenImportGrpSvcType ??= new();
        set => hiddenImportGrpSvcType = value;
      }

      /// <summary>
      /// A value of HiddenImportGrpCuf.
      /// </summary>
      [JsonPropertyName("hiddenImportGrpCuf")]
      public Standard HiddenImportGrpCuf
      {
        get => hiddenImportGrpCuf ??= new();
        set => hiddenImportGrpCuf = value;
      }

      /// <summary>
      /// A value of Obl.
      /// </summary>
      [JsonPropertyName("obl")]
      public Standard Obl
      {
        get => obl ??= new();
        set => obl = value;
      }

      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public Standard Ap
      {
        get => ap ??= new();
        set => ap = value;
      }

      /// <summary>
      /// A value of Loc.
      /// </summary>
      [JsonPropertyName("loc")]
      public Standard Loc
      {
        get => loc ??= new();
        set => loc = value;
      }

      /// <summary>
      /// A value of SvcType.
      /// </summary>
      [JsonPropertyName("svcType")]
      public Standard SvcType
      {
        get => svcType ??= new();
        set => svcType = value;
      }

      /// <summary>
      /// A value of Cuf.
      /// </summary>
      [JsonPropertyName("cuf")]
      public Standard Cuf
      {
        get => cuf ??= new();
        set => cuf = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public LifecycleState Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of LifecycleState.
      /// </summary>
      [JsonPropertyName("lifecycleState")]
      public LifecycleState LifecycleState
      {
        get => lifecycleState ??= new();
        set => lifecycleState = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Standard hiddenImportGrpObl;
      private Standard hiddenImportGrpAp;
      private Standard hiddenImportGrpLoc;
      private Standard hiddenImportGrpSvcType;
      private Standard hiddenImportGrpCuf;
      private Standard obl;
      private Standard ap;
      private Standard loc;
      private Standard svcType;
      private Standard cuf;
      private LifecycleState hidden;
      private LifecycleState lifecycleState;
      private Common common;
    }

    /// <summary>
    /// A value of HiddenImportStarting.
    /// </summary>
    [JsonPropertyName("hiddenImportStarting")]
    public LifecycleState HiddenImportStarting
    {
      get => hiddenImportStarting ??= new();
      set => hiddenImportStarting = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public LifecycleState Starting
    {
      get => starting ??= new();
      set => starting = value;
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

    private LifecycleState hiddenImportStarting;
    private LifecycleState starting;
    private Standard standard;
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
      /// A value of HiddenExportGrpObl.
      /// </summary>
      [JsonPropertyName("hiddenExportGrpObl")]
      public Standard HiddenExportGrpObl
      {
        get => hiddenExportGrpObl ??= new();
        set => hiddenExportGrpObl = value;
      }

      /// <summary>
      /// A value of HiddenExportGrpAp.
      /// </summary>
      [JsonPropertyName("hiddenExportGrpAp")]
      public Standard HiddenExportGrpAp
      {
        get => hiddenExportGrpAp ??= new();
        set => hiddenExportGrpAp = value;
      }

      /// <summary>
      /// A value of HiddenExportGrpLoc.
      /// </summary>
      [JsonPropertyName("hiddenExportGrpLoc")]
      public Standard HiddenExportGrpLoc
      {
        get => hiddenExportGrpLoc ??= new();
        set => hiddenExportGrpLoc = value;
      }

      /// <summary>
      /// A value of HiddenExportGrpSvcType.
      /// </summary>
      [JsonPropertyName("hiddenExportGrpSvcType")]
      public Standard HiddenExportGrpSvcType
      {
        get => hiddenExportGrpSvcType ??= new();
        set => hiddenExportGrpSvcType = value;
      }

      /// <summary>
      /// A value of HiddenExportGrpCuf.
      /// </summary>
      [JsonPropertyName("hiddenExportGrpCuf")]
      public Standard HiddenExportGrpCuf
      {
        get => hiddenExportGrpCuf ??= new();
        set => hiddenExportGrpCuf = value;
      }

      /// <summary>
      /// A value of Obl.
      /// </summary>
      [JsonPropertyName("obl")]
      public Standard Obl
      {
        get => obl ??= new();
        set => obl = value;
      }

      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public Standard Ap
      {
        get => ap ??= new();
        set => ap = value;
      }

      /// <summary>
      /// A value of Loc.
      /// </summary>
      [JsonPropertyName("loc")]
      public Standard Loc
      {
        get => loc ??= new();
        set => loc = value;
      }

      /// <summary>
      /// A value of SvcType.
      /// </summary>
      [JsonPropertyName("svcType")]
      public Standard SvcType
      {
        get => svcType ??= new();
        set => svcType = value;
      }

      /// <summary>
      /// A value of Cuf.
      /// </summary>
      [JsonPropertyName("cuf")]
      public Standard Cuf
      {
        get => cuf ??= new();
        set => cuf = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public LifecycleState Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of LifecycleState.
      /// </summary>
      [JsonPropertyName("lifecycleState")]
      public LifecycleState LifecycleState
      {
        get => lifecycleState ??= new();
        set => lifecycleState = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Standard hiddenExportGrpObl;
      private Standard hiddenExportGrpAp;
      private Standard hiddenExportGrpLoc;
      private Standard hiddenExportGrpSvcType;
      private Standard hiddenExportGrpCuf;
      private Standard obl;
      private Standard ap;
      private Standard loc;
      private Standard svcType;
      private Standard cuf;
      private LifecycleState hidden;
      private LifecycleState lifecycleState;
      private Common common;
    }

    /// <summary>
    /// A value of HiddenExportStarting.
    /// </summary>
    [JsonPropertyName("hiddenExportStarting")]
    public LifecycleState HiddenExportStarting
    {
      get => hiddenExportStarting ??= new();
      set => hiddenExportStarting = value;
    }

    /// <summary>
    /// A value of ToTran.
    /// </summary>
    [JsonPropertyName("toTran")]
    public LifecycleState ToTran
    {
      get => toTran ??= new();
      set => toTran = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public LifecycleState Starting
    {
      get => starting ??= new();
      set => starting = value;
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

    private LifecycleState hiddenExportStarting;
    private LifecycleState toTran;
    private LifecycleState starting;
    private Standard standard;
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
    /// A value of PassTo.
    /// </summary>
    [JsonPropertyName("passTo")]
    public LifecycleState PassTo
    {
      get => passTo ??= new();
      set => passTo = value;
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

    private LifecycleState passTo;
    private Common common;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LifecycleTransformation.
    /// </summary>
    [JsonPropertyName("lifecycleTransformation")]
    public LifecycleTransformation LifecycleTransformation
    {
      get => lifecycleTransformation ??= new();
      set => lifecycleTransformation = value;
    }

    /// <summary>
    /// A value of LifecycleState.
    /// </summary>
    [JsonPropertyName("lifecycleState")]
    public LifecycleState LifecycleState
    {
      get => lifecycleState ??= new();
      set => lifecycleState = value;
    }

    private LifecycleTransformation lifecycleTransformation;
    private LifecycleState lifecycleState;
  }
#endregion
}
