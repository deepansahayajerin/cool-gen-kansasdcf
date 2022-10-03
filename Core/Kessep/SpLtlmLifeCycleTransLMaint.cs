// Program: SP_LTLM_LIFE_CYCLE_TRANS_L_MAINT, ID: 371779395, model: 746.
// Short name: SWELTLMP
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
/// A program: SP_LTLM_LIFE_CYCLE_TRANS_L_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpLtlmLifeCycleTransLMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_LTLM_LIFE_CYCLE_TRANS_L_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpLtlmLifeCycleTransLMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpLtlmLifeCycleTransLMaint.
  /// </summary>
  public SpLtlmLifeCycleTransLMaint(IContext context, Import import,
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
    // 11/12/96 Alan Samuels                Initial Development
    // 09/22/98  Anita Massey         Fixes per screen assessment
    //                                         
    // and correction form
    // 06/07/99  Anita Massey         Set read property to select
    //                                         
    // only.
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // ---------------------------------------------
    // If command=clear, clear out group view.
    // ---------------------------------------------
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

    export.Event1.ControlNumber = import.Event1.ControlNumber;
    export.HiddenEvent.ControlNumber = import.HiddenEvent.ControlNumber;
    MoveEventDetail(import.EventDetail2, export.EventDetail2);
    export.HiddenEventDetail.SystemGeneratedIdentifier =
      import.HiddenEventDetail.SystemGeneratedIdentifier;
    export.EventDetail1.SelectChar = import.EventDetail1.SelectChar;
    export.Start.Identifier = import.Start.Identifier;

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
      export.HiddenNextTranInfo.Assign(local.NextTranInfo);

      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      local.NextTranInfo.Assign(import.HiddenNextTranInfo);

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

    if (Equal(global.Command, "RETLINK"))
    {
    }
    else
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    // Move group views if command <> display.
    // ---------------------------------------------
    if (!Equal(global.Command, "DISPLAY"))
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

        export.Group.Update.Event1.ControlNumber =
          import.Group.Item.Event1.ControlNumber;
        MoveEventDetail(import.Group.Item.EventDetail1,
          export.Group.Update.EventDetail2);
        export.Group.Update.LifecycleTransformation.Description =
          import.Group.Item.LifecycleTransformation.Description;
        export.Group.Update.Current.Identifier =
          import.Group.Item.Current.Identifier;
        export.Group.Update.EventDetail1.SelectChar =
          import.Group.Item.EventDetail2.SelectChar;
        export.Group.Update.Transformed.Identifier =
          import.Group.Item.Transformed.Identifier;
        export.Group.Update.LcState.SelectChar =
          import.Group.Item.LcState.SelectChar;
        export.Group.Update.ResultingLcState.SelectChar =
          import.Group.Item.ResultingLcState.SelectChar;
        export.Group.Update.HiddenEvent.ControlNumber =
          import.Group.Item.HiddenEvent.ControlNumber;
        MoveEventDetail(import.Group.Item.HiddenEventDetail,
          export.Group.Update.HiddenEventDetail);
        export.Group.Update.HiddenLifecycleTransformation.Description =
          import.Group.Item.HiddenLifecycleTransformation.Description;
        export.Group.Update.HiddenExportGrpCurrent.Identifier =
          import.Group.Item.HiddenImportGrpCurrent.Identifier;
        export.Group.Update.HiddenExportGrpTransformed.Identifier =
          import.Group.Item.HiddenImportGrpTransformed.Identifier;
        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        export.Group.Next();
      }
    }

    // ---------------------------------------------
    // Prompt is only valid on PF4 List.
    // ---------------------------------------------
    if (Equal(global.Command, "LIST") || Equal(global.Command, "RETLINK") || Equal
      (global.Command, "EXIT"))
    {
    }
    else
    {
      if (!IsEmpty(import.EventDetail1.SelectChar))
      {
        var field = GetField(export.EventDetail1, "selectChar");

        field.Error = true;

        ExitState = "SP0000_PROMPT_NOT_ALLOWED";
      }

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!IsEmpty(export.Group.Item.LcState.SelectChar))
        {
          var field = GetField(export.Group.Item.LcState, "selectChar");

          field.Error = true;

          ExitState = "SP0000_PROMPT_NOT_ALLOWED";
        }

        if (!IsEmpty(export.Group.Item.EventDetail1.SelectChar))
        {
          var field = GetField(export.Group.Item.EventDetail1, "selectChar");

          field.Error = true;

          ExitState = "SP0000_PROMPT_NOT_ALLOWED";
        }

        if (!IsEmpty(export.Group.Item.ResultingLcState.SelectChar))
        {
          var field =
            GetField(export.Group.Item.ResultingLcState, "selectChar");

          field.Error = true;

          ExitState = "SP0000_PROMPT_NOT_ALLOWED";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Display first before any maintenance.
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      if (import.Event1.ControlNumber == 0)
      {
        ExitState = "ACO_NE0000_DISPLAY_FIRST";

        var field1 = GetField(export.EventDetail2, "systemGeneratedIdentifier");

        field1.Error = true;

        var field2 = GetField(export.Event1, "controlNumber");

        field2.Error = true;

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
        // Display logic is located at bottom of PrAD.
        break;
      case "LIST":
        local.Common.Count = 0;

        if (!IsEmpty(import.EventDetail1.SelectChar))
        {
          if (AsChar(import.EventDetail1.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_EDLM";
            ++local.Common.Count;
          }
          else
          {
            var field = GetField(export.EventDetail1, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
          }
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.LcState.SelectChar))
          {
            if (AsChar(export.Group.Item.LcState.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_LSLM";
              ++local.Common.Count;
            }
            else
            {
              var field = GetField(export.Group.Item.LcState, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              return;
            }
          }

          if (!IsEmpty(export.Group.Item.EventDetail1.SelectChar))
          {
            if (AsChar(export.Group.Item.EventDetail1.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_EDLM";
              ++local.Common.Count;
            }
            else
            {
              var field =
                GetField(export.Group.Item.EventDetail1, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              return;
            }
          }

          if (!IsEmpty(export.Group.Item.ResultingLcState.SelectChar))
          {
            if (AsChar(export.Group.Item.ResultingLcState.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_LSLM";
              ++local.Common.Count;
            }
            else
            {
              var field =
                GetField(export.Group.Item.ResultingLcState, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              return;
            }
          }
        }

        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }
        else if (local.Common.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }

        break;
      case "RETLINK":
        if (AsChar(import.EventDetail1.SelectChar) == 'S')
        {
          export.EventDetail1.SelectChar = "";

          if (import.FromLinkEvent.ControlNumber > 0)
          {
            export.Event1.ControlNumber = import.FromLinkEvent.ControlNumber;
            export.EventDetail2.SystemGeneratedIdentifier =
              import.FromLinkEventDetail.SystemGeneratedIdentifier;
          }

          var field = GetField(export.Event1, "controlNumber");

          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;

          global.Command = "DISPLAY";
        }
        else
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.LcState.SelectChar) == 'S')
            {
              export.Group.Update.LcState.SelectChar = "";

              if (!IsEmpty(import.FromLinkLifecycleState.Identifier))
              {
                export.Group.Update.Current.Identifier =
                  import.FromLinkLifecycleState.Identifier;
              }

              var field = GetField(export.Group.Item.Current, "identifier");

              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;

              return;
            }
            else if (AsChar(export.Group.Item.EventDetail1.SelectChar) == 'S')
            {
              export.Group.Update.EventDetail1.SelectChar = "";

              if (import.FromLinkEvent.ControlNumber > 0)
              {
                export.Group.Update.Event1.ControlNumber =
                  import.FromLinkEvent.ControlNumber;
                export.Group.Update.EventDetail2.SystemGeneratedIdentifier =
                  import.FromLinkEventDetail.SystemGeneratedIdentifier;
              }

              var field = GetField(export.Group.Item.Event1, "controlNumber");

              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;

              return;
            }
            else if (AsChar(export.Group.Item.ResultingLcState.SelectChar) == 'S'
              )
            {
              export.Group.Update.ResultingLcState.SelectChar = "";

              if (!IsEmpty(import.FromLinkLifecycleState.Identifier))
              {
                export.Group.Update.Transformed.Identifier =
                  import.FromLinkLifecycleState.Identifier;
              }

              var field = GetField(export.Group.Item.Transformed, "identifier");

              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;

              return;
            }
          }
        }

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
              if (!IsEmpty(export.Group.Item.HiddenExportGrpCurrent.Identifier))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

                return;
              }

              // ---------------------------------------------
              // Perform data validation. All fields are
              // required.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.LifecycleTransformation.Description))
                
              {
                var field =
                  GetField(export.Group.Item.LifecycleTransformation,
                  "description");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }

              if (IsEmpty(export.Group.Item.Transformed.Identifier))
              {
                var field =
                  GetField(export.Group.Item.Transformed, "identifier");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (!ReadLifecycleState2())
              {
                var field =
                  GetField(export.Group.Item.Transformed, "identifier");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
              }

              if (export.Group.Item.EventDetail2.SystemGeneratedIdentifier == 0)
              {
                var field1 =
                  GetField(export.Group.Item.EventDetail2,
                  "systemGeneratedIdentifier");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.Event1, "controlNumber");

                field2.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
              }
              else if (!ReadEventDetail2())
              {
                var field1 =
                  GetField(export.Group.Item.EventDetail2,
                  "systemGeneratedIdentifier");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.Event1, "controlNumber");

                field2.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
              }

              if (IsEmpty(export.Group.Item.Current.Identifier))
              {
                var field = GetField(export.Group.Item.Current, "identifier");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (!ReadLifecycleState1())
              {
                var field = GetField(export.Group.Item.Current, "identifier");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
              }

              // ---------------------------------------------
              // The combination of the caused by event detail
              // current lifecycle state and resulting Life Cycle
              // State must be unique.
              // ---------------------------------------------
              if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                ("ACO_NI0000_ADD_SUCCESSFUL"))
              {
                if (ReadLifecycleTransformation())
                {
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;

                  ExitState = "SP0000_COMBINATION_NOT_UNIQUE";
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
            ("ACO_NI0000_ADD_SUCCESSFUL"))
          {
          }
          else
          {
            return;
          }

          // ---------------------------------------------
          // Data has passed validation. Create
          // Lifecycle Transformation.
          // ---------------------------------------------
          local.PassTo.Description =
            export.Group.Item.LifecycleTransformation.Description;
          local.PassToCausedByEvent.ControlNumber = import.Event1.ControlNumber;
          local.PassToCausedByEventDetail.SystemGeneratedIdentifier =
            import.EventDetail2.SystemGeneratedIdentifier;
          local.PassToCurrent.Identifier = export.Group.Item.Current.Identifier;
          local.PassToResulting.Identifier =
            export.Group.Item.Transformed.Identifier;
          local.PassToTargetedEvent.ControlNumber =
            export.Group.Item.Event1.ControlNumber;
          local.PassToTargetedEventDetail.SystemGeneratedIdentifier =
            export.Group.Item.EventDetail2.SystemGeneratedIdentifier;
          UseSpCabCreateLcTransformation();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_ADD_SUCCESSFUL"))
          {
            ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.HiddenEvent.ControlNumber =
              import.Group.Item.Event1.ControlNumber;
            MoveEventDetail(import.Group.Item.EventDetail1,
              export.Group.Update.HiddenEventDetail);
            export.Group.Update.HiddenLifecycleTransformation.Description =
              import.Group.Item.LifecycleTransformation.Description;
            export.Group.Update.HiddenExportGrpCurrent.Identifier =
              import.Group.Item.Current.Identifier;
            export.Group.Update.HiddenExportGrpTransformed.Identifier =
              import.Group.Item.Transformed.Identifier;
            export.Group.Update.EventDetail2.DetailName =
              entities.TargetedEventDetail.DetailName;
            export.Group.Update.HiddenEventDetail.DetailName =
              entities.TargetedEventDetail.DetailName;
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
              if (IsEmpty(export.Group.Item.HiddenExportGrpCurrent.Identifier))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

                return;
              }

              // ---------------------------------------------
              // Perform data validation for update request.
              // Only transformation description can be
              // changed.
              // ---------------------------------------------
              if (Equal(export.Group.Item.LifecycleTransformation.Description,
                import.Group.Item.HiddenLifecycleTransformation.Description))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_DATA_NOT_CHANGED";

                return;
              }

              if (Equal(export.Group.Item.Transformed.Identifier,
                export.Group.Item.HiddenExportGrpTransformed.Identifier))
              {
              }
              else
              {
                export.Group.Update.Transformed.Identifier =
                  export.Group.Item.HiddenExportGrpTransformed.Identifier;

                var field =
                  GetField(export.Group.Item.Transformed, "identifier");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              if (export.Group.Item.EventDetail2.SystemGeneratedIdentifier == export
                .Group.Item.HiddenEventDetail.SystemGeneratedIdentifier)
              {
              }
              else
              {
                export.Group.Update.EventDetail2.SystemGeneratedIdentifier =
                  export.Group.Item.HiddenEventDetail.SystemGeneratedIdentifier;
                  

                var field =
                  GetField(export.Group.Item.EventDetail2,
                  "systemGeneratedIdentifier");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              if (export.Group.Item.Event1.ControlNumber == export
                .Group.Item.HiddenEvent.ControlNumber)
              {
              }
              else
              {
                export.Group.Update.Event1.ControlNumber =
                  export.Group.Item.HiddenEvent.ControlNumber;

                var field = GetField(export.Group.Item.Event1, "controlNumber");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              if (Equal(export.Group.Item.Current.Identifier,
                export.Group.Item.HiddenExportGrpCurrent.Identifier))
              {
              }
              else
              {
                export.Group.Update.Current.Identifier =
                  export.Group.Item.HiddenExportGrpCurrent.Identifier;

                var field = GetField(export.Group.Item.Current, "identifier");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
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
          // Lifecycle Transformation.
          // ---------------------------------------------
          local.PassTo.Description =
            export.Group.Item.LifecycleTransformation.Description;
          local.PassToCausedByEvent.ControlNumber = import.Event1.ControlNumber;
          local.PassToCausedByEventDetail.SystemGeneratedIdentifier =
            import.EventDetail2.SystemGeneratedIdentifier;
          local.PassToCurrent.Identifier = export.Group.Item.Current.Identifier;
          local.PassToResulting.Identifier =
            export.Group.Item.Transformed.Identifier;
          local.PassToTargetedEvent.ControlNumber =
            export.Group.Item.Event1.ControlNumber;
          local.PassToTargetedEventDetail.SystemGeneratedIdentifier =
            export.Group.Item.EventDetail2.SystemGeneratedIdentifier;
          UseSpCabUpdateLcTransformation();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_UPDATE_SUCCESSFUL"))
          {
            ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.HiddenLifecycleTransformation.Description =
              export.Group.Item.LifecycleTransformation.Description;
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
              if (IsEmpty(export.Group.Item.HiddenExportGrpCurrent.Identifier))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_DELETE_ON_EMPTY_ROW";

                return;
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

          local.PassTo.Description =
            export.Group.Item.LifecycleTransformation.Description;
          local.PassToCausedByEvent.ControlNumber = import.Event1.ControlNumber;
          local.PassToCausedByEventDetail.SystemGeneratedIdentifier =
            import.EventDetail2.SystemGeneratedIdentifier;
          local.PassToCurrent.Identifier = export.Group.Item.Current.Identifier;
          local.PassToResulting.Identifier =
            export.Group.Item.Transformed.Identifier;
          local.PassToTargetedEvent.ControlNumber =
            export.Group.Item.Event1.ControlNumber;
          local.PassToTargetedEventDetail.SystemGeneratedIdentifier =
            export.Group.Item.EventDetail2.SystemGeneratedIdentifier;
          UseSpCabDeleteLcTransformation();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_DELETE_SUCCESSFUL"))
          {
            ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.Event1.ControlNumber =
              local.InitializeEvent.ControlNumber;
            export.Group.Update.HiddenEvent.ControlNumber =
              local.InitializeEvent.ControlNumber;
            MoveEventDetail(local.InitializeEventDetail,
              export.Group.Update.EventDetail2);
            MoveEventDetail(local.InitializeEventDetail,
              export.Group.Update.HiddenEventDetail);
            export.Group.Update.Current.Identifier =
              local.InitializeLifecycleState.Identifier;
            export.Group.Update.HiddenExportGrpCurrent.Identifier =
              local.InitializeLifecycleState.Identifier;
            export.Group.Update.Transformed.Identifier =
              local.InitializeLifecycleState.Identifier;
            export.Group.Update.HiddenExportGrpTransformed.Identifier =
              local.InitializeLifecycleState.Identifier;
            export.Group.Update.LifecycleTransformation.Description =
              local.InitializeLifecycleTransformation.Description;
            export.Group.Update.HiddenLifecycleTransformation.Description =
              local.InitializeLifecycleTransformation.Description;
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

    if (Equal(global.Command, "DISPLAY"))
    {
      if (ReadEventDetail1())
      {
        export.HiddenEvent.ControlNumber = import.Event1.ControlNumber;
        MoveEventDetail(entities.EventDetail, export.EventDetail2);
        export.HiddenEventDetail.SystemGeneratedIdentifier =
          entities.EventDetail.SystemGeneratedIdentifier;
      }
      else
      {
        if (export.Event1.ControlNumber > 0)
        {
          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          export.EventDetail1.SelectChar = "S";

          var field1 =
            GetField(export.EventDetail2, "systemGeneratedIdentifier");

          field1.Error = true;

          var field2 = GetField(export.Event1, "controlNumber");

          field2.Error = true;
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          export.EventDetail1.SelectChar = "S";

          var field = GetField(export.Event1, "controlNumber");

          field.Protected = false;
          field.Focused = true;
        }

        return;
      }

      export.Group.Index = 0;
      export.Group.Clear();

      foreach(var item in ReadLifecycleTransformationLifecycleStateLifecycleState())
        
      {
        export.Group.Update.LifecycleTransformation.Description =
          entities.LifecycleTransformation.Description;
        export.Group.Update.HiddenLifecycleTransformation.Description =
          entities.LifecycleTransformation.Description;
        export.Group.Update.Current.Identifier = entities.Current.Identifier;
        export.Group.Update.HiddenExportGrpCurrent.Identifier =
          entities.Current.Identifier;
        export.Group.Update.Transformed.Identifier =
          entities.Transformed.Identifier;
        export.Group.Update.HiddenExportGrpTransformed.Identifier =
          entities.Transformed.Identifier;
        MoveEventDetail(entities.TargetedEventDetail,
          export.Group.Update.EventDetail2);
        MoveEventDetail(entities.TargetedEventDetail,
          export.Group.Update.HiddenEventDetail);
        export.Group.Update.Event1.ControlNumber =
          entities.TargetedEvent.ControlNumber;
        export.Group.Update.HiddenEvent.ControlNumber =
          entities.TargetedEvent.ControlNumber;
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
    }
  }

  private static void MoveEventDetail(EventDetail source, EventDetail target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DetailName = source.DetailName;
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

  private void UseSpCabCreateLcTransformation()
  {
    var useImport = new SpCabCreateLcTransformation.Import();
    var useExport = new SpCabCreateLcTransformation.Export();

    useImport.Current.Identifier = local.PassToCurrent.Identifier;
    useImport.Resulting.Identifier = local.PassToResulting.Identifier;
    useImport.CausedByEventDetail.SystemGeneratedIdentifier =
      local.PassToCausedByEventDetail.SystemGeneratedIdentifier;
    useImport.CausedByEvent.ControlNumber =
      local.PassToCausedByEvent.ControlNumber;
    useImport.TargetedEvent.ControlNumber =
      local.PassToTargetedEvent.ControlNumber;
    useImport.TargetedEventDetail.SystemGeneratedIdentifier =
      local.PassToTargetedEventDetail.SystemGeneratedIdentifier;
    useImport.LifecycleTransformation.Description = local.PassTo.Description;

    Call(SpCabCreateLcTransformation.Execute, useImport, useExport);
  }

  private void UseSpCabDeleteLcTransformation()
  {
    var useImport = new SpCabDeleteLcTransformation.Import();
    var useExport = new SpCabDeleteLcTransformation.Export();

    useImport.Current.Identifier = local.PassToCurrent.Identifier;
    useImport.Resulting.Identifier = local.PassToResulting.Identifier;
    useImport.CausedByEventDetail.SystemGeneratedIdentifier =
      local.PassToCausedByEventDetail.SystemGeneratedIdentifier;
    useImport.CausedByEvent.ControlNumber =
      local.PassToCausedByEvent.ControlNumber;
    useImport.TargetedEvent.ControlNumber =
      local.PassToTargetedEvent.ControlNumber;
    useImport.TargetedEventDetail.SystemGeneratedIdentifier =
      local.PassToTargetedEventDetail.SystemGeneratedIdentifier;
    useImport.LifecycleTransformation.Description = local.PassTo.Description;

    Call(SpCabDeleteLcTransformation.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateLcTransformation()
  {
    var useImport = new SpCabUpdateLcTransformation.Import();
    var useExport = new SpCabUpdateLcTransformation.Export();

    useImport.Current.Identifier = local.PassToCurrent.Identifier;
    useImport.Resulting.Identifier = local.PassToResulting.Identifier;
    useImport.CausedByEventDetail.SystemGeneratedIdentifier =
      local.PassToCausedByEventDetail.SystemGeneratedIdentifier;
    useImport.CausedByEvent.ControlNumber =
      local.PassToCausedByEvent.ControlNumber;
    useImport.TargetedEvent.ControlNumber =
      local.PassToTargetedEvent.ControlNumber;
    useImport.TargetedEventDetail.SystemGeneratedIdentifier =
      local.PassToTargetedEventDetail.SystemGeneratedIdentifier;
    useImport.LifecycleTransformation.Description = local.PassTo.Description;

    Call(SpCabUpdateLcTransformation.Execute, useImport, useExport);
  }

  private bool ReadEventDetail1()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.EventDetail2.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", export.Event1.ControlNumber);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.DetailName = db.GetString(reader, 1);
        entities.EventDetail.EveNo = db.GetInt32(reader, 2);
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadEventDetail2()
  {
    entities.TargetedEventDetail.Populated = false;

    return Read("ReadEventDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.Group.Item.EventDetail2.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", export.Group.Item.Event1.ControlNumber);
      },
      (db, reader) =>
      {
        entities.TargetedEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.TargetedEventDetail.DetailName = db.GetString(reader, 1);
        entities.TargetedEventDetail.EveNo = db.GetInt32(reader, 2);
        entities.TargetedEventDetail.Populated = true;
      });
  }

  private bool ReadLifecycleState1()
  {
    entities.Current.Populated = false;

    return Read("ReadLifecycleState1",
      (db, command) =>
      {
        db.
          SetString(command, "identifier", export.Group.Item.Current.Identifier);
          
      },
      (db, reader) =>
      {
        entities.Current.Identifier = db.GetString(reader, 0);
        entities.Current.Populated = true;
      });
  }

  private bool ReadLifecycleState2()
  {
    entities.Transformed.Populated = false;

    return Read("ReadLifecycleState2",
      (db, command) =>
      {
        db.SetString(
          command, "identifier", export.Group.Item.Transformed.Identifier);
      },
      (db, reader) =>
      {
        entities.Transformed.Identifier = db.GetString(reader, 0);
        entities.Transformed.Populated = true;
      });
  }

  private bool ReadLifecycleTransformation()
  {
    entities.LifecycleTransformation.Populated = false;

    return Read("ReadLifecycleTransformation",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdIdPri", import.EventDetail2.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveCtrlNoPri", import.Event1.ControlNumber);
        db.SetString(command, "lcsLctIdSec", entities.Transformed.Identifier);
        db.SetString(command, "lcsIdPri", entities.Current.Identifier);
      },
      (db, reader) =>
      {
        entities.LifecycleTransformation.Description = db.GetString(reader, 0);
        entities.LifecycleTransformation.LcsIdPri = db.GetString(reader, 1);
        entities.LifecycleTransformation.EveCtrlNoPri = db.GetInt32(reader, 2);
        entities.LifecycleTransformation.EvdIdPri = db.GetInt32(reader, 3);
        entities.LifecycleTransformation.LcsLctIdSec = db.GetString(reader, 4);
        entities.LifecycleTransformation.EveNoSec =
          db.GetNullableInt32(reader, 5);
        entities.LifecycleTransformation.EvdLctIdSec =
          db.GetNullableInt32(reader, 6);
        entities.LifecycleTransformation.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadLifecycleTransformationLifecycleStateLifecycleState()
  {
    System.Diagnostics.Debug.Assert(entities.EventDetail.Populated);

    return ReadEach("ReadLifecycleTransformationLifecycleStateLifecycleState",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdIdPri", entities.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveCtrlNoPri", entities.EventDetail.EveNo);
        db.SetString(command, "lcsIdPri", export.Start.Identifier);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.LifecycleTransformation.Description = db.GetString(reader, 0);
        entities.LifecycleTransformation.LcsIdPri = db.GetString(reader, 1);
        entities.Current.Identifier = db.GetString(reader, 1);
        entities.LifecycleTransformation.EveCtrlNoPri = db.GetInt32(reader, 2);
        entities.LifecycleTransformation.EvdIdPri = db.GetInt32(reader, 3);
        entities.LifecycleTransformation.LcsLctIdSec = db.GetString(reader, 4);
        entities.Transformed.Identifier = db.GetString(reader, 4);
        entities.LifecycleTransformation.EveNoSec =
          db.GetNullableInt32(reader, 5);
        entities.TargetedEventDetail.EveNo = db.GetInt32(reader, 5);
        entities.TargetedEvent.ControlNumber = db.GetInt32(reader, 5);
        entities.TargetedEvent.ControlNumber = db.GetInt32(reader, 5);
        entities.LifecycleTransformation.EvdLctIdSec =
          db.GetNullableInt32(reader, 6);
        entities.TargetedEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.TargetedEventDetail.DetailName = db.GetString(reader, 7);
        entities.TargetedEventDetail.Populated = true;
        entities.TargetedEvent.Populated = true;
        entities.Transformed.Populated = true;
        entities.Current.Populated = true;
        entities.LifecycleTransformation.Populated = true;

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
      /// A value of HiddenLifecycleTransformation.
      /// </summary>
      [JsonPropertyName("hiddenLifecycleTransformation")]
      public LifecycleTransformation HiddenLifecycleTransformation
      {
        get => hiddenLifecycleTransformation ??= new();
        set => hiddenLifecycleTransformation = value;
      }

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
      /// A value of HiddenImportGrpTransformed.
      /// </summary>
      [JsonPropertyName("hiddenImportGrpTransformed")]
      public LifecycleState HiddenImportGrpTransformed
      {
        get => hiddenImportGrpTransformed ??= new();
        set => hiddenImportGrpTransformed = value;
      }

      /// <summary>
      /// A value of HiddenImportGrpCurrent.
      /// </summary>
      [JsonPropertyName("hiddenImportGrpCurrent")]
      public LifecycleState HiddenImportGrpCurrent
      {
        get => hiddenImportGrpCurrent ??= new();
        set => hiddenImportGrpCurrent = value;
      }

      /// <summary>
      /// A value of HiddenEventDetail.
      /// </summary>
      [JsonPropertyName("hiddenEventDetail")]
      public EventDetail HiddenEventDetail
      {
        get => hiddenEventDetail ??= new();
        set => hiddenEventDetail = value;
      }

      /// <summary>
      /// A value of EventDetail1.
      /// </summary>
      [JsonPropertyName("eventDetail1")]
      public EventDetail EventDetail1
      {
        get => eventDetail1 ??= new();
        set => eventDetail1 = value;
      }

      /// <summary>
      /// A value of HiddenEvent.
      /// </summary>
      [JsonPropertyName("hiddenEvent")]
      public Event1 HiddenEvent
      {
        get => hiddenEvent ??= new();
        set => hiddenEvent = value;
      }

      /// <summary>
      /// A value of Event1.
      /// </summary>
      [JsonPropertyName("event1")]
      public Event1 Event1
      {
        get => event1 ??= new();
        set => event1 = value;
      }

      /// <summary>
      /// A value of Transformed.
      /// </summary>
      [JsonPropertyName("transformed")]
      public LifecycleState Transformed
      {
        get => transformed ??= new();
        set => transformed = value;
      }

      /// <summary>
      /// A value of Current.
      /// </summary>
      [JsonPropertyName("current")]
      public LifecycleState Current
      {
        get => current ??= new();
        set => current = value;
      }

      /// <summary>
      /// A value of ResultingLcState.
      /// </summary>
      [JsonPropertyName("resultingLcState")]
      public Common ResultingLcState
      {
        get => resultingLcState ??= new();
        set => resultingLcState = value;
      }

      /// <summary>
      /// A value of EventDetail2.
      /// </summary>
      [JsonPropertyName("eventDetail2")]
      public Common EventDetail2
      {
        get => eventDetail2 ??= new();
        set => eventDetail2 = value;
      }

      /// <summary>
      /// A value of LcState.
      /// </summary>
      [JsonPropertyName("lcState")]
      public Common LcState
      {
        get => lcState ??= new();
        set => lcState = value;
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
      public const int Capacity = 60;

      private LifecycleTransformation hiddenLifecycleTransformation;
      private LifecycleTransformation lifecycleTransformation;
      private LifecycleState hiddenImportGrpTransformed;
      private LifecycleState hiddenImportGrpCurrent;
      private EventDetail hiddenEventDetail;
      private EventDetail eventDetail1;
      private Event1 hiddenEvent;
      private Event1 event1;
      private LifecycleState transformed;
      private LifecycleState current;
      private Common resultingLcState;
      private Common eventDetail2;
      private Common lcState;
      private Common common;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public LifecycleState Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of FromLinkLifecycleState.
    /// </summary>
    [JsonPropertyName("fromLinkLifecycleState")]
    public LifecycleState FromLinkLifecycleState
    {
      get => fromLinkLifecycleState ??= new();
      set => fromLinkLifecycleState = value;
    }

    /// <summary>
    /// A value of FromLinkEventDetail.
    /// </summary>
    [JsonPropertyName("fromLinkEventDetail")]
    public EventDetail FromLinkEventDetail
    {
      get => fromLinkEventDetail ??= new();
      set => fromLinkEventDetail = value;
    }

    /// <summary>
    /// A value of FromLinkEvent.
    /// </summary>
    [JsonPropertyName("fromLinkEvent")]
    public Event1 FromLinkEvent
    {
      get => fromLinkEvent ??= new();
      set => fromLinkEvent = value;
    }

    /// <summary>
    /// A value of EventDetail1.
    /// </summary>
    [JsonPropertyName("eventDetail1")]
    public Common EventDetail1
    {
      get => eventDetail1 ??= new();
      set => eventDetail1 = value;
    }

    /// <summary>
    /// A value of HiddenEventDetail.
    /// </summary>
    [JsonPropertyName("hiddenEventDetail")]
    public EventDetail HiddenEventDetail
    {
      get => hiddenEventDetail ??= new();
      set => hiddenEventDetail = value;
    }

    /// <summary>
    /// A value of EventDetail2.
    /// </summary>
    [JsonPropertyName("eventDetail2")]
    public EventDetail EventDetail2
    {
      get => eventDetail2 ??= new();
      set => eventDetail2 = value;
    }

    /// <summary>
    /// A value of HiddenEvent.
    /// </summary>
    [JsonPropertyName("hiddenEvent")]
    public Event1 HiddenEvent
    {
      get => hiddenEvent ??= new();
      set => hiddenEvent = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private LifecycleState start;
    private LifecycleState fromLinkLifecycleState;
    private EventDetail fromLinkEventDetail;
    private Event1 fromLinkEvent;
    private Common eventDetail1;
    private EventDetail hiddenEventDetail;
    private EventDetail eventDetail2;
    private Event1 hiddenEvent;
    private Event1 event1;
    private Standard standard;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
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
      /// A value of HiddenLifecycleTransformation.
      /// </summary>
      [JsonPropertyName("hiddenLifecycleTransformation")]
      public LifecycleTransformation HiddenLifecycleTransformation
      {
        get => hiddenLifecycleTransformation ??= new();
        set => hiddenLifecycleTransformation = value;
      }

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
      /// A value of ResultingLcState.
      /// </summary>
      [JsonPropertyName("resultingLcState")]
      public Common ResultingLcState
      {
        get => resultingLcState ??= new();
        set => resultingLcState = value;
      }

      /// <summary>
      /// A value of EventDetail1.
      /// </summary>
      [JsonPropertyName("eventDetail1")]
      public Common EventDetail1
      {
        get => eventDetail1 ??= new();
        set => eventDetail1 = value;
      }

      /// <summary>
      /// A value of LcState.
      /// </summary>
      [JsonPropertyName("lcState")]
      public Common LcState
      {
        get => lcState ??= new();
        set => lcState = value;
      }

      /// <summary>
      /// A value of HiddenExportGrpTransformed.
      /// </summary>
      [JsonPropertyName("hiddenExportGrpTransformed")]
      public LifecycleState HiddenExportGrpTransformed
      {
        get => hiddenExportGrpTransformed ??= new();
        set => hiddenExportGrpTransformed = value;
      }

      /// <summary>
      /// A value of HiddenExportGrpCurrent.
      /// </summary>
      [JsonPropertyName("hiddenExportGrpCurrent")]
      public LifecycleState HiddenExportGrpCurrent
      {
        get => hiddenExportGrpCurrent ??= new();
        set => hiddenExportGrpCurrent = value;
      }

      /// <summary>
      /// A value of HiddenEventDetail.
      /// </summary>
      [JsonPropertyName("hiddenEventDetail")]
      public EventDetail HiddenEventDetail
      {
        get => hiddenEventDetail ??= new();
        set => hiddenEventDetail = value;
      }

      /// <summary>
      /// A value of EventDetail2.
      /// </summary>
      [JsonPropertyName("eventDetail2")]
      public EventDetail EventDetail2
      {
        get => eventDetail2 ??= new();
        set => eventDetail2 = value;
      }

      /// <summary>
      /// A value of HiddenEvent.
      /// </summary>
      [JsonPropertyName("hiddenEvent")]
      public Event1 HiddenEvent
      {
        get => hiddenEvent ??= new();
        set => hiddenEvent = value;
      }

      /// <summary>
      /// A value of Event1.
      /// </summary>
      [JsonPropertyName("event1")]
      public Event1 Event1
      {
        get => event1 ??= new();
        set => event1 = value;
      }

      /// <summary>
      /// A value of Transformed.
      /// </summary>
      [JsonPropertyName("transformed")]
      public LifecycleState Transformed
      {
        get => transformed ??= new();
        set => transformed = value;
      }

      /// <summary>
      /// A value of Current.
      /// </summary>
      [JsonPropertyName("current")]
      public LifecycleState Current
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 60;

      private LifecycleTransformation hiddenLifecycleTransformation;
      private LifecycleTransformation lifecycleTransformation;
      private Common resultingLcState;
      private Common eventDetail1;
      private Common lcState;
      private LifecycleState hiddenExportGrpTransformed;
      private LifecycleState hiddenExportGrpCurrent;
      private EventDetail hiddenEventDetail;
      private EventDetail eventDetail2;
      private Event1 hiddenEvent;
      private Event1 event1;
      private LifecycleState transformed;
      private LifecycleState current;
      private Common common;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public LifecycleState Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of EventDetail1.
    /// </summary>
    [JsonPropertyName("eventDetail1")]
    public Common EventDetail1
    {
      get => eventDetail1 ??= new();
      set => eventDetail1 = value;
    }

    /// <summary>
    /// A value of HiddenEventDetail.
    /// </summary>
    [JsonPropertyName("hiddenEventDetail")]
    public EventDetail HiddenEventDetail
    {
      get => hiddenEventDetail ??= new();
      set => hiddenEventDetail = value;
    }

    /// <summary>
    /// A value of EventDetail2.
    /// </summary>
    [JsonPropertyName("eventDetail2")]
    public EventDetail EventDetail2
    {
      get => eventDetail2 ??= new();
      set => eventDetail2 = value;
    }

    /// <summary>
    /// A value of HiddenEvent.
    /// </summary>
    [JsonPropertyName("hiddenEvent")]
    public Event1 HiddenEvent
    {
      get => hiddenEvent ??= new();
      set => hiddenEvent = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private LifecycleState start;
    private Common eventDetail1;
    private EventDetail hiddenEventDetail;
    private EventDetail eventDetail2;
    private Event1 hiddenEvent;
    private Event1 event1;
    private Standard standard;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InitializeLifecycleTransformation.
    /// </summary>
    [JsonPropertyName("initializeLifecycleTransformation")]
    public LifecycleTransformation InitializeLifecycleTransformation
    {
      get => initializeLifecycleTransformation ??= new();
      set => initializeLifecycleTransformation = value;
    }

    /// <summary>
    /// A value of InitializeEventDetail.
    /// </summary>
    [JsonPropertyName("initializeEventDetail")]
    public EventDetail InitializeEventDetail
    {
      get => initializeEventDetail ??= new();
      set => initializeEventDetail = value;
    }

    /// <summary>
    /// A value of InitializeEvent.
    /// </summary>
    [JsonPropertyName("initializeEvent")]
    public Event1 InitializeEvent
    {
      get => initializeEvent ??= new();
      set => initializeEvent = value;
    }

    /// <summary>
    /// A value of InitializeLifecycleState.
    /// </summary>
    [JsonPropertyName("initializeLifecycleState")]
    public LifecycleState InitializeLifecycleState
    {
      get => initializeLifecycleState ??= new();
      set => initializeLifecycleState = value;
    }

    /// <summary>
    /// A value of PassToCurrent.
    /// </summary>
    [JsonPropertyName("passToCurrent")]
    public LifecycleState PassToCurrent
    {
      get => passToCurrent ??= new();
      set => passToCurrent = value;
    }

    /// <summary>
    /// A value of PassToResulting.
    /// </summary>
    [JsonPropertyName("passToResulting")]
    public LifecycleState PassToResulting
    {
      get => passToResulting ??= new();
      set => passToResulting = value;
    }

    /// <summary>
    /// A value of PassToCausedByEventDetail.
    /// </summary>
    [JsonPropertyName("passToCausedByEventDetail")]
    public EventDetail PassToCausedByEventDetail
    {
      get => passToCausedByEventDetail ??= new();
      set => passToCausedByEventDetail = value;
    }

    /// <summary>
    /// A value of PassToCausedByEvent.
    /// </summary>
    [JsonPropertyName("passToCausedByEvent")]
    public Event1 PassToCausedByEvent
    {
      get => passToCausedByEvent ??= new();
      set => passToCausedByEvent = value;
    }

    /// <summary>
    /// A value of PassToTargetedEvent.
    /// </summary>
    [JsonPropertyName("passToTargetedEvent")]
    public Event1 PassToTargetedEvent
    {
      get => passToTargetedEvent ??= new();
      set => passToTargetedEvent = value;
    }

    /// <summary>
    /// A value of PassToTargetedEventDetail.
    /// </summary>
    [JsonPropertyName("passToTargetedEventDetail")]
    public EventDetail PassToTargetedEventDetail
    {
      get => passToTargetedEventDetail ??= new();
      set => passToTargetedEventDetail = value;
    }

    /// <summary>
    /// A value of PassTo.
    /// </summary>
    [JsonPropertyName("passTo")]
    public LifecycleTransformation PassTo
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

    private LifecycleTransformation initializeLifecycleTransformation;
    private EventDetail initializeEventDetail;
    private Event1 initializeEvent;
    private LifecycleState initializeLifecycleState;
    private LifecycleState passToCurrent;
    private LifecycleState passToResulting;
    private EventDetail passToCausedByEventDetail;
    private Event1 passToCausedByEvent;
    private Event1 passToTargetedEvent;
    private EventDetail passToTargetedEventDetail;
    private LifecycleTransformation passTo;
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
    /// A value of TargetedEventDetail.
    /// </summary>
    [JsonPropertyName("targetedEventDetail")]
    public EventDetail TargetedEventDetail
    {
      get => targetedEventDetail ??= new();
      set => targetedEventDetail = value;
    }

    /// <summary>
    /// A value of TargetedEvent.
    /// </summary>
    [JsonPropertyName("targetedEvent")]
    public Event1 TargetedEvent
    {
      get => targetedEvent ??= new();
      set => targetedEvent = value;
    }

    /// <summary>
    /// A value of Transformed.
    /// </summary>
    [JsonPropertyName("transformed")]
    public LifecycleState Transformed
    {
      get => transformed ??= new();
      set => transformed = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public LifecycleState Current
    {
      get => current ??= new();
      set => current = value;
    }

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
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    private EventDetail targetedEventDetail;
    private Event1 targetedEvent;
    private LifecycleState transformed;
    private LifecycleState current;
    private LifecycleTransformation lifecycleTransformation;
    private EventDetail eventDetail;
    private Event1 event1;
  }
#endregion
}
