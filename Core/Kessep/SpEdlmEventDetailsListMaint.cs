// Program: SP_EDLM_EVENT_DETAILS_LIST_MAINT, ID: 371778213, model: 746.
// Short name: SWEEDLMP
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
/// A program: SP_EDLM_EVENT_DETAILS_LIST_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpEdlmEventDetailsListMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EDLM_EVENT_DETAILS_LIST_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEdlmEventDetailsListMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEdlmEventDetailsListMaint.
  /// </summary>
  public SpEdlmEventDetailsListMaint(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------------------------
    // Date	  Developer	     Request #	                Description
    // 10/24/96  Rick Delgado
    // 
    // Initial Development
    // 11/20/96  Alan Samuels
    // 
    // Completed Development
    // 10/06/97  Siraj Konkader    IDCR 390                    Added fld 
    // EXCEPTION
    //                                                         
    // ROUTINE
    // 09/04/98  Anita Massey
    // 
    // Changes to literals and code
    //                                                         
    // per the assessment and change
    //                                                         
    // form.
    // 06/04/99  Anita Massey
    // 
    // Reviewed the reads and
    //                                                         
    // changed to select only.
    // 03/28/08 Joyce Harden       CQ420                       Allow system to 
    // add a Detail with
    //                                                         
    // any Effective Date.  Add search
    //                                                         
    // by reason code.  Make MORE - +
    //                                                         work explicit.
    // 
    // ------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------
    // If command=clear, clear out repeating group.
    // ---------------------------------------------
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }
    else if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    export.Hidden.Assign(import.Hidden);
    MoveEvent1(import.Event2, export.Event2);
    export.HiddenCheck.ControlNumber = import.HiddenCheck.ControlNumber;
    export.Event1.PromptField = import.Event1.PromptField;

    // -----------------------------------------------------
    // 
    // if user wants to signoff, let them do so no matter what.  No
    // validations.  Per user SME Sana Beall
    // 
    // ------------------------------------------------------
    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    export.Plus.Text1 = import.Plus.Text1;
    export.Minus.Text1 = import.Minus.Text1;
    export.PageNumber.PageNumber = import.PageNumber.PageNumber;
    export.Flag.Flag = import.Flag.Flag;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveEventDetail(import.Starting, export.Starting);

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
      global.Command = "DISPLAY";

      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
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

    if (!Equal(global.Command, "RETLINK"))
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    // Move group views if command != display.
    // ---------------------------------------------
    if (!Equal(global.Command, "DISPLAY"))
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

        export.Group.Update.EventDetail.Assign(import.Group.Item.EventDetail);
        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        export.Group.Update.Hidden.Assign(import.Group.Item.Hidden);
      }

      import.Group.CheckIndex();

      for(import.PageKey.Index = 0; import.PageKey.Index < import
        .PageKey.Count; ++import.PageKey.Index)
      {
        if (!import.PageKey.CheckSize())
        {
          break;
        }

        export.PageKey.Index = import.PageKey.Index;
        export.PageKey.CheckSize();

        export.PageKey.Update.PageStart.SystemGeneratedIdentifier =
          import.PageKey.Item.PageStart.SystemGeneratedIdentifier;
        export.PageKey.Update.PageStart.ReasonCode =
          import.PageKey.Item.PageStart.ReasonCode;

        if (export.Starting.SystemGeneratedIdentifier > 0)
        {
          export.Flag.Flag = "D";
        }
        else if (!IsEmpty(export.Starting.ReasonCode))
        {
          export.Flag.Flag = "R";
        }
      }

      import.PageKey.CheckIndex();
    }
    else
    {
      for(import.PageKey.Index = 0; import.PageKey.Index < import
        .PageKey.Count; ++import.PageKey.Index)
      {
        if (!import.PageKey.CheckSize())
        {
          break;
        }

        export.PageKey.Index = import.PageKey.Index;
        export.PageKey.CheckSize();

        export.PageKey.Update.PageStart.SystemGeneratedIdentifier = 0;
        export.PageKey.Update.PageStart.ReasonCode = "";
      }

      import.PageKey.CheckIndex();
      export.PageKey.Index = -1;
      export.PageKey.Count = 0;
      export.PageNumber.PageNumber = 1;

      ++export.PageKey.Index;
      export.PageKey.CheckSize();

      export.PageKey.Update.PageStart.SystemGeneratedIdentifier =
        import.Starting.SystemGeneratedIdentifier;
      export.PageKey.Update.PageStart.ReasonCode = import.Starting.ReasonCode;

      if (export.Starting.SystemGeneratedIdentifier > 0)
      {
        export.Flag.Flag = "D";
      }
      else if (!IsEmpty(export.Starting.ReasonCode))
      {
        export.Flag.Flag = "R";
      }
    }

    // ------------cq420--------added 'S'----
    if (import.Event2.ControlNumber <= 0 && AsChar
      (import.Event1.PromptField) != 'S')
    {
      var field = GetField(export.Event2, "controlNumber");

      field.Error = true;

      ExitState = "SP0000_EVENT_NUMBER_NOT_ENTERED";

      return;
    }

    // ---------------------------------------------
    // Prompt is only valid on PF4 List.
    // ---------------------------------------------
    if (Equal(global.Command, "LIST") || Equal(global.Command, "RETLINK") || Equal
      (global.Command, "EXIT"))
    {
    }
    else if (AsChar(export.Event1.PromptField) == 'S')
    {
      export.Event1.PromptField = "";
    }

    // ---------------------------------------------
    // Must display before maintenance.
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      if (import.Event2.ControlNumber == 0)
      {
        ExitState = "ACO_NE0000_DISPLAY_FIRST";

        return;
      }

      local.Common.Count = 0;

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        if (AsChar(import.Group.Item.Common.SelectChar) == 'S')
        {
          ++local.Common.Count;
        }
      }

      import.Group.CheckIndex();

      if (local.Common.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "NEXT":
        if (AsChar(import.Flag.Flag) == 'R')
        {
          ++export.PageNumber.PageNumber;

          for(export.PageKey.Index = 0; export.PageKey.Index < export
            .PageKey.Count; ++export.PageKey.Index)
          {
            if (!export.PageKey.CheckSize())
            {
              break;
            }

            if (export.PageNumber.PageNumber == export.PageKey.Index + 1)
            {
              local.Read.ReasonCode = export.PageKey.Item.PageStart.ReasonCode;
              global.Command = "DISPLAY";

              goto Test1;
            }
          }

          export.PageKey.CheckIndex();

          var field = GetField(export.Starting, "reasonCode");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;

          ExitState = "CO0000_INVALID_PAGING_REQUEST";
        }
        else
        {
          ++export.PageNumber.PageNumber;

          for(export.PageKey.Index = 0; export.PageKey.Index < export
            .PageKey.Count; ++export.PageKey.Index)
          {
            if (!export.PageKey.CheckSize())
            {
              break;
            }

            if (export.PageNumber.PageNumber == export.PageKey.Index + 1)
            {
              local.Read.SystemGeneratedIdentifier =
                export.PageKey.Item.PageStart.SystemGeneratedIdentifier;
              global.Command = "DISPLAY";

              goto Test1;
            }
          }

          export.PageKey.CheckIndex();

          var field = GetField(export.Starting, "systemGeneratedIdentifier");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;

          ExitState = "CO0000_INVALID_PAGING_REQUEST";
        }

        break;
      case "PREV":
        // -----CQ420---------Flag----R stands for Reason code
        if (AsChar(import.Flag.Flag) == 'R')
        {
          if (export.PageNumber.PageNumber <= 1)
          {
            var field = GetField(export.Starting, "reasonCode");

            field.Color = "green";
            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;

            ExitState = "CO0000_INVALID_PAGING_REQUEST";

            break;
          }

          --export.PageNumber.PageNumber;

          for(export.PageKey.Index = 0; export.PageKey.Index < export
            .PageKey.Count; ++export.PageKey.Index)
          {
            if (!export.PageKey.CheckSize())
            {
              break;
            }

            if (export.PageNumber.PageNumber == export.PageKey.Index + 1)
            {
              local.Read.ReasonCode = export.PageKey.Item.PageStart.ReasonCode;
              global.Command = "DISPLAY";

              goto Test1;
            }
          }

          export.PageKey.CheckIndex();
        }
        else
        {
          if (export.PageNumber.PageNumber <= 1)
          {
            var field = GetField(export.Starting, "systemGeneratedIdentifier");

            field.Color = "green";
            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;

            ExitState = "CO0000_INVALID_PAGING_REQUEST";

            break;
          }

          --export.PageNumber.PageNumber;

          for(export.PageKey.Index = 0; export.PageKey.Index < export
            .PageKey.Count; ++export.PageKey.Index)
          {
            if (!export.PageKey.CheckSize())
            {
              break;
            }

            if (export.PageNumber.PageNumber == export.PageKey.Index + 1)
            {
              local.Read.SystemGeneratedIdentifier =
                export.PageKey.Item.PageStart.SystemGeneratedIdentifier;
              global.Command = "DISPLAY";

              goto Test1;
            }
          }

          export.PageKey.CheckIndex();
        }

        break;
      case "DISPLAY":
        local.Read.SystemGeneratedIdentifier =
          import.Starting.SystemGeneratedIdentifier;
        local.Read.ReasonCode = import.Starting.ReasonCode;

        // Display logic is located at end of PrAD.
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "LIST":
        local.Common.Count = 0;

        if (!IsEmpty(import.Event1.PromptField))
        {
          if (AsChar(import.Event1.PromptField) == 'S')
          {
            ++local.Common.Count;
            ExitState = "ECO_LNK_TO_EVLS";
          }
          else
          {
            var field = GetField(export.Event1, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
          }
        }

        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }
        else if (local.Common.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }
        else
        {
        }

        break;
      case "RETLINK":
        if (AsChar(import.Event1.PromptField) == 'S')
        {
          export.Event1.PromptField = "";

          if (import.FromLink.ControlNumber > 0)
          {
            export.Event2.ControlNumber = import.FromLink.ControlNumber;
          }

          var field = GetField(export.Event2, "controlNumber");

          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;

          global.Command = "DISPLAY";
        }

        break;
      case "ADD":
        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // An add can only occur on a blank row.
              // ---------------------------------------------
              if (export.Group.Item.Hidden.SystemGeneratedIdentifier > 0)
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

                goto Test1;
              }

              if (!IsEmpty(export.Group.Item.Hidden.ReasonCode))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

                goto Test1;
              }

              local.PassToEventDetail.Assign(export.Group.Item.EventDetail);

              // ---------------------------------------------
              // Description is required.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.EventDetail.Description))
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "description");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }

              // ---------------------------------------------
              // Function is required.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.EventDetail.Function))
              {
                var field = GetField(export.Group.Item.EventDetail, "function");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (!Equal(export.Group.Item.EventDetail.Function, "LOC") &&
                !Equal(export.Group.Item.EventDetail.Function, "OBG") && !
                Equal(export.Group.Item.EventDetail.Function, "PAT") && !
                Equal(export.Group.Item.EventDetail.Function, "ENF"))
              {
                ExitState = "CO0000_INVALID_CSE_FUNCTION";

                var field = GetField(export.Group.Item.EventDetail, "function");

                field.Error = true;
              }
              else
              {
              }

              // ---------------------------------------------
              // Discontinue date must be > effective date.
              // Default value = 12/31/2099.
              // ---------------------------------------------
              if (export.Group.Item.EventDetail.DiscontinueDate != null)
              {
                if (export.Group.Item.EventDetail.EffectiveDate != null)
                {
                  if (Lt(export.Group.Item.EventDetail.DiscontinueDate,
                    export.Group.Item.EventDetail.EffectiveDate))
                  {
                    var field =
                      GetField(export.Group.Item.EventDetail, "discontinueDate");
                      

                    field.Error = true;

                    ExitState = "SP0000_INVALID_DISC_DATE";
                  }
                }
                else if (Lt(export.Group.Item.EventDetail.DiscontinueDate,
                  Now().Date))
                {
                  var field =
                    GetField(export.Group.Item.EventDetail, "discontinueDate");

                  field.Error = true;

                  ExitState = "SP0000_INVALID_DISC_DATE";
                }
              }
              else
              {
                local.PassToEventDetail.DiscontinueDate =
                  UseCabSetMaximumDiscontinueDate();
              }

              // ---------------------------------------------
              // Log to diary ind is required. Must be Y or N.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.EventDetail.LogToDiaryInd))
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "logToDiaryInd");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (AsChar(export.Group.Item.EventDetail.LogToDiaryInd) == 'Y'
                || AsChar(export.Group.Item.EventDetail.LogToDiaryInd) == 'N')
              {
              }
              else
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "logToDiaryInd");

                field.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }

              // ---------------------------------------------
              // Lifecycle impact code is required.
              // Must be Y or N.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.EventDetail.LifecycleImpactCode))
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "lifecycleImpactCode");
                  

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (AsChar(export.Group.Item.EventDetail.LifecycleImpactCode) ==
                'Y' || AsChar
                (export.Group.Item.EventDetail.LifecycleImpactCode) == 'N')
              {
              }
              else
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "lifecycleImpactCode");
                  

                field.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }

              // ---------------------------------------------
              // If entered, next event detail id must exist.
              // ---------------------------------------------
              if (!IsEmpty(export.Group.Item.EventDetail.NextEventDetailId) || export
                .Group.Item.EventDetail.NextEventId.GetValueOrDefault() > 0)
              {
                local.EventDetail.SystemGeneratedIdentifier =
                  (int)StringToNumber(export.Group.Item.EventDetail.
                    NextEventDetailId);

                if (!ReadEventDetail3())
                {
                  var field =
                    GetField(export.Group.Item.EventDetail, "nextEventDetailId");
                    

                  field.Error = true;

                  ExitState = "SP0000_EVENT_DETAIL_NF";
                }
              }

              // ---------------------------------------------
              // Reason code is required and must be valid.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.EventDetail.ReasonCode))
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "reasonCode");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }

              // ---------------------------------------------
              // If entered, CSENET in out code must be I or O.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.EventDetail.CsenetInOutCode))
              {
              }
              else if (AsChar(export.Group.Item.EventDetail.CsenetInOutCode) ==
                'I' || AsChar
                (export.Group.Item.EventDetail.CsenetInOutCode) == 'O')
              {
              }
              else
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "csenetInOutCode");

                field.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }

              // ---------------------------------------------
              // If entered, state code must be KS or OS.
              // Default is KS.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.EventDetail.InitiatingStateCode))
              {
                local.PassToEventDetail.InitiatingStateCode = "KS";
              }
              else if (Equal(export.Group.Item.EventDetail.InitiatingStateCode,
                "KS") || Equal
                (export.Group.Item.EventDetail.InitiatingStateCode, "OS"))
              {
              }
              else
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "initiatingStateCode");
                  

                field.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }

              // ---------------------------------------------
              // Detail name is required and must be unique.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.EventDetail.DetailName))
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "detailName");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (ReadEventDetail1())
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "detailName");

                field.Error = true;

                ExitState = "SP0000_DETAIL_NAME_NU";
              }

              // ---------------------------------------------
              // ID number is required and must be unique.
              // ---------------------------------------------
              if (export.Group.Item.EventDetail.SystemGeneratedIdentifier == 0)
              {
                var field =
                  GetField(export.Group.Item.EventDetail,
                  "systemGeneratedIdentifier");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (ReadEventDetail2())
              {
                var field =
                  GetField(export.Group.Item.EventDetail,
                  "systemGeneratedIdentifier");

                field.Error = true;

                ExitState = "SP0000_EVENT_DETAIL_AE";
              }
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              goto Test1;
            }

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_ADD_SUCCESSFUL"))
            {
            }
            else
            {
              goto Test1;
            }

            // ---------------------------------------------
            // Data has passed validation. Create activity
            // start stop.
            // ---------------------------------------------
            UseSpCabCreateEventDetail();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_ADD_SUCCESSFUL"))
            {
              ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
              export.Group.Update.Common.SelectChar = "";
              export.Group.Update.EventDetail.Assign(local.ReturnFrom);
              export.Group.Update.Hidden.Assign(local.ReturnFrom);
            }
            else
            {
              goto Test1;
            }
          }
          else
          {
            continue;
          }
        }

        export.Group.CheckIndex();

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

          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // An update can only occur on a populated row.
              // ---------------------------------------------
              if (export.Group.Item.Hidden.SystemGeneratedIdentifier == 0)
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

                goto Test1;
              }

              if (IsEmpty(export.Group.Item.Hidden.ReasonCode))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

                goto Test1;
              }

              // ---------------------------------------------
              // Some updateable data must have changed.
              // ---------------------------------------------
              if (Equal(export.Group.Item.EventDetail.DetailName,
                export.Group.Item.Hidden.DetailName) && export
                .Group.Item.EventDetail.NextEventId.GetValueOrDefault() == export
                .Group.Item.Hidden.NextEventId.GetValueOrDefault() && Equal
                (export.Group.Item.EventDetail.NextEventDetailId,
                export.Group.Item.Hidden.NextEventDetailId) && Equal
                (export.Group.Item.EventDetail.ProcedureName,
                export.Group.Item.Hidden.ProcedureName) && AsChar
                (export.Group.Item.EventDetail.LogToDiaryInd) == AsChar
                (export.Group.Item.Hidden.LogToDiaryInd) && export
                .Group.Item.EventDetail.DateMonitorDays.GetValueOrDefault() == export
                .Group.Item.Hidden.DateMonitorDays.GetValueOrDefault() && Equal
                (export.Group.Item.EventDetail.EffectiveDate,
                export.Group.Item.Hidden.EffectiveDate) && Equal
                (export.Group.Item.EventDetail.DiscontinueDate,
                export.Group.Item.Hidden.DiscontinueDate) && Equal
                (export.Group.Item.EventDetail.Description,
                export.Group.Item.Hidden.Description) && Equal
                (export.Group.Item.EventDetail.Function,
                export.Group.Item.Hidden.Function) && AsChar
                (export.Group.Item.EventDetail.LifecycleImpactCode) == AsChar
                (export.Group.Item.Hidden.LifecycleImpactCode) && Equal
                (export.Group.Item.EventDetail.ExceptionRoutine,
                export.Group.Item.Hidden.ExceptionRoutine))
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_DATA_NOT_CHANGED";

                goto Test1;
              }

              local.PassToEventDetail.Assign(export.Group.Item.EventDetail);

              // ---------------------------------------------
              // Description is required.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.EventDetail.Description))
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "description");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }

              // ---------------------------------------------
              // Function is required.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.EventDetail.Function))
              {
                var field = GetField(export.Group.Item.EventDetail, "function");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (!Equal(export.Group.Item.EventDetail.Function, "LOC") &&
                !Equal(export.Group.Item.EventDetail.Function, "OBG") && !
                Equal(export.Group.Item.EventDetail.Function, "PAT") && !
                Equal(export.Group.Item.EventDetail.Function, "ENF"))
              {
                ExitState = "CO0000_INVALID_CSE_FUNCTION";

                var field = GetField(export.Group.Item.EventDetail, "function");

                field.Error = true;
              }

              // ---------------------------------------------
              // Discontinue date must be > effective date.
              // Default value = 12/31/2999.
              // ---------------------------------------------
              if (Equal(export.Group.Item.EventDetail.DiscontinueDate,
                export.Group.Item.EventDetail.DiscontinueDate) && Equal
                (export.Group.Item.EventDetail.EffectiveDate,
                export.Group.Item.Hidden.EffectiveDate))
              {
                if (Equal(export.Group.Item.EventDetail.DiscontinueDate,
                  local.LnullDate.Date))
                {
                  local.PassToEventDetail.DiscontinueDate =
                    UseCabSetMaximumDiscontinueDate();
                }
              }
              else if (Lt(local.LnullDate.Date,
                export.Group.Item.EventDetail.DiscontinueDate))
              {
                if (Lt(local.LnullDate.Date,
                  export.Group.Item.EventDetail.EffectiveDate))
                {
                  if (Lt(export.Group.Item.EventDetail.DiscontinueDate,
                    export.Group.Item.EventDetail.EffectiveDate))
                  {
                    var field =
                      GetField(export.Group.Item.EventDetail, "discontinueDate");
                      

                    field.Error = true;

                    ExitState = "SP0000_INVALID_DISC_DATE";
                  }
                }
                else if (Lt(export.Group.Item.EventDetail.DiscontinueDate,
                  Now().Date))
                {
                  var field =
                    GetField(export.Group.Item.EventDetail, "discontinueDate");

                  field.Error = true;

                  ExitState = "SP0000_INVALID_DISC_DATE";
                }
              }
              else
              {
                local.PassToEventDetail.DiscontinueDate =
                  UseCabSetMaximumDiscontinueDate();
              }

              // ---------------------------------------------
              // Log to diary ind is required. Must be Y or N.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.EventDetail.LogToDiaryInd))
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "logToDiaryInd");

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (AsChar(export.Group.Item.EventDetail.LogToDiaryInd) == 'Y'
                || AsChar(export.Group.Item.EventDetail.LogToDiaryInd) == 'N')
              {
              }
              else
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "logToDiaryInd");

                field.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }

              // ---------------------------------------------
              // Lifecycle impact code is required.
              // Must be Y or N.
              // ---------------------------------------------
              if (IsEmpty(export.Group.Item.EventDetail.LifecycleImpactCode))
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "lifecycleImpactCode");
                  

                field.Error = true;

                ExitState = "SP0000_REQUIRED_FIELD_MISSING";
              }
              else if (AsChar(export.Group.Item.EventDetail.LifecycleImpactCode) ==
                'Y' || AsChar
                (export.Group.Item.EventDetail.LifecycleImpactCode) == 'N')
              {
              }
              else
              {
                var field =
                  GetField(export.Group.Item.EventDetail, "lifecycleImpactCode");
                  

                field.Error = true;

                ExitState = "SP0000_INVALID_VALUE_ENTERED";
              }

              if (Equal(export.Group.Item.EventDetail.NextEventDetailId,
                export.Group.Item.Hidden.NextEventDetailId) && export
                .Group.Item.EventDetail.NextEventId.GetValueOrDefault() == export
                .Group.Item.Hidden.NextEventId.GetValueOrDefault())
              {
              }
              else
              {
                // ---------------------------------------------
                // If entered, next event detail id must exist.
                // ---------------------------------------------
                // --------------------??????????? reason_code ---------------
                if (!IsEmpty(export.Group.Item.EventDetail.NextEventDetailId) ||
                  export
                  .Group.Item.EventDetail.NextEventId.GetValueOrDefault() > 0)
                {
                  local.EventDetail.SystemGeneratedIdentifier =
                    (int)StringToNumber(export.Group.Item.EventDetail.
                      NextEventDetailId);

                  if (!ReadEventDetail3())
                  {
                    var field =
                      GetField(export.Group.Item.EventDetail,
                      "nextEventDetailId");

                    field.Error = true;

                    ExitState = "SP0000_EVENT_DETAIL_NF";
                  }
                }
              }

              // ---------------------------------------------
              // Reason code is not updateable.
              // ---------------------------------------------
              if (!Equal(export.Group.Item.EventDetail.ReasonCode,
                export.Group.Item.Hidden.ReasonCode))
              {
                export.Group.Update.EventDetail.ReasonCode =
                  export.Group.Item.Hidden.ReasonCode;

                var field =
                  GetField(export.Group.Item.EventDetail, "reasonCode");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              // ---------------------------------------------
              // CSENET in out code cannot be updated.
              // ---------------------------------------------
              if (AsChar(export.Group.Item.EventDetail.CsenetInOutCode) != AsChar
                (export.Group.Item.Hidden.CsenetInOutCode))
              {
                export.Group.Update.EventDetail.CsenetInOutCode =
                  export.Group.Item.Hidden.CsenetInOutCode;

                var field =
                  GetField(export.Group.Item.EventDetail, "csenetInOutCode");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              // ---------------------------------------------
              // State code cannot be changed.
              // ---------------------------------------------
              if (!Equal(export.Group.Item.EventDetail.InitiatingStateCode,
                export.Group.Item.Hidden.InitiatingStateCode))
              {
                export.Group.Update.EventDetail.InitiatingStateCode =
                  export.Group.Item.Hidden.InitiatingStateCode;

                var field =
                  GetField(export.Group.Item.EventDetail, "initiatingStateCode");
                  

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }

              // ---------------------------------------------
              // Detail name is required and must be unique.
              // ---------------------------------------------
              if (!Equal(export.Group.Item.EventDetail.DetailName,
                export.Group.Item.EventDetail.DetailName))
              {
                if (IsEmpty(export.Group.Item.EventDetail.DetailName))
                {
                  var field =
                    GetField(export.Group.Item.EventDetail, "detailName");

                  field.Error = true;

                  ExitState = "SP0000_REQUIRED_FIELD_MISSING";
                }
                else if (ReadEventDetail1())
                {
                  var field =
                    GetField(export.Group.Item.EventDetail, "detailName");

                  field.Error = true;

                  ExitState = "SP0000_DETAIL_NAME_NU";
                }
              }

              // ---------------------------------------------
              // ID number is not updateable.
              // ---------------------------------------------
              if (export.Group.Item.EventDetail.SystemGeneratedIdentifier == export
                .Group.Item.Hidden.SystemGeneratedIdentifier)
              {
              }
              else
              {
                export.Group.Update.EventDetail.SystemGeneratedIdentifier =
                  export.Group.Item.Hidden.SystemGeneratedIdentifier;

                var field =
                  GetField(export.Group.Item.EventDetail,
                  "systemGeneratedIdentifier");

                field.Error = true;

                ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
              }
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              goto Test1;
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
            goto Test1;
          }

          // ---------------------------------------------
          // Data has passed validation. Update Activity
          // Start Stop.
          // ---------------------------------------------
          UseSpCabUpdateEventDetail();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_UPDATE_SUCCESSFUL"))
          {
            ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.EventDetail.Assign(local.ReturnFrom);
            export.Group.Update.Hidden.Assign(local.ReturnFrom);
          }
          else
          {
            goto Test1;
          }
        }

        export.Group.CheckIndex();

        break;
      case "DELETE":
        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              // ---------------------------------------------
              // A delete can only occur on a populated row.
              // ---------------------------------------------
              if (export.Group.Item.Hidden.SystemGeneratedIdentifier == 0)
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_DELETE_ON_EMPTY_ROW";

                goto Test1;
              }

              // ---------------------------------------------
              // An Event Detail cannot be deleted if any
              // dependent relationships exist.
              // ---------------------------------------------
              if (ReadActivityStartStop())
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_RELATED_DETAILS_EXIST";

                goto Test1;
              }

              if (ReadAlertDistributionRule())
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_RELATED_DETAILS_EXIST";

                goto Test1;
              }

              if (ReadDocument())
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_RELATED_DETAILS_EXIST";

                goto Test1;
              }

              if (ReadLifecycleTransformation1())
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_RELATED_DETAILS_EXIST";

                goto Test1;
              }

              if (ReadLifecycleTransformation2())
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "SP0000_RELATED_DETAILS_EXIST";

                goto Test1;
              }

              local.PassToEventDetail.Assign(export.Group.Item.EventDetail);
              UseSpCabDeleteEventDetail();

              if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                ("ACO_NI0000_DELETE_SUCCESSFUL"))
              {
                ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
                export.Group.Update.EventDetail.Assign(local.Initialize);
                export.Group.Update.Hidden.Assign(local.Initialize);
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                goto Test1;
              }
            }
            else
            {
              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              goto Test1;
            }
          }
        }

        export.Group.CheckIndex();

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
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              ++local.Common.Count;
              export.ToTranEvent.ControlNumber = import.Event2.ControlNumber;
              export.ToTranEventDetail.SystemGeneratedIdentifier =
                export.Group.Item.EventDetail.SystemGeneratedIdentifier;
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              goto Test1;
            }
          }
        }

        export.Group.CheckIndex();

        if (local.Common.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test1:

    // - - - - - - - - - - - - - - - - - - - - - -
    //   Maintain protected fields
    // - - - - - - - - - - - - - - - - - - - - - -
    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      if (!IsEmpty(export.Group.Item.EventDetail.CsenetInOutCode))
      {
        var field = GetField(export.Group.Item.EventDetail, "csenetInOutCode");

        field.Color = "cyan";
        field.Protected = true;
      }

      if (!IsEmpty(export.Group.Item.EventDetail.InitiatingStateCode))
      {
        var field =
          GetField(export.Group.Item.EventDetail, "initiatingStateCode");

        field.Color = "cyan";
        field.Protected = true;
      }

      var field1 =
        GetField(export.Group.Item.EventDetail, "systemGeneratedIdentifier");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Group.Item.EventDetail, "reasonCode");

      field2.Color = "cyan";
      field2.Protected = true;
    }

    export.Group.CheckIndex();

    // ---------------------------------------------
    // Perform display logic if command=display.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      if (ReadEvent())
      {
        MoveEvent1(entities.Existing, export.Event2);
        export.HiddenCheck.ControlNumber = entities.Existing.ControlNumber;
        export.Group.Index = -1;
        export.Group.Count = 0;

        // -------------------------CQ420
        // -----------------------------------
        if (AsChar(export.Flag.Flag) == 'R')
        {
          foreach(var item in ReadEventDetail4())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            export.Group.Update.EventDetail.Assign(entities.EventDetail);
            export.Group.Update.Hidden.Assign(entities.EventDetail);
            local.PassToDateWorkArea.Date =
              export.Group.Item.EventDetail.DiscontinueDate;
            export.Group.Update.EventDetail.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate();
            export.Group.Update.Hidden.DiscontinueDate =
              export.Group.Item.EventDetail.DiscontinueDate;

            if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
            {
              ++export.PageKey.Index;
              export.PageKey.CheckSize();

              export.PageKey.Update.PageStart.ReasonCode =
                export.Group.Item.EventDetail.ReasonCode;

              goto Test2;
            }
          }
        }
        else
        {
          foreach(var item in ReadEventDetail5())
          {
            ++export.Group.Index;
            export.Group.CheckSize();

            export.Group.Update.EventDetail.Assign(entities.EventDetail);
            export.Group.Update.Hidden.Assign(entities.EventDetail);
            local.PassToDateWorkArea.Date =
              export.Group.Item.EventDetail.DiscontinueDate;
            export.Group.Update.EventDetail.DiscontinueDate =
              UseCabSetMaximumDiscontinueDate();
            export.Group.Update.Hidden.DiscontinueDate =
              export.Group.Item.EventDetail.DiscontinueDate;

            if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
            {
              ++export.PageKey.Index;
              export.PageKey.CheckSize();

              export.PageKey.Update.PageStart.SystemGeneratedIdentifier =
                export.Group.Item.EventDetail.SystemGeneratedIdentifier;

              goto Test2;
            }
          }
        }

Test2:

        // --------cq 420---------------
        if (export.PageNumber.PageNumber <= 1)
        {
          export.Minus.Text1 = "";
        }
        else
        {
          export.Minus.Text1 = "-";
        }

        if (export.PageNumber.PageNumber < export.PageKey.Count)
        {
          export.Plus.Text1 = "+";
        }
        else
        {
          export.Plus.Text1 = "";
        }

        // --------cq 420---------------
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          var field1 =
            GetField(export.Group.Item.EventDetail, "systemGeneratedIdentifier");
            

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Group.Item.EventDetail, "reasonCode");

          field2.Color = "cyan";
          field2.Protected = true;

          if (!IsEmpty(export.Group.Item.EventDetail.CsenetInOutCode))
          {
            var field =
              GetField(export.Group.Item.EventDetail, "csenetInOutCode");

            field.Color = "cyan";
            field.Protected = true;
          }

          if (!IsEmpty(export.Group.Item.EventDetail.InitiatingStateCode))
          {
            var field =
              GetField(export.Group.Item.EventDetail, "initiatingStateCode");

            field.Color = "cyan";
            field.Protected = true;
          }
        }

        export.Group.CheckIndex();

        if (export.Group.IsEmpty)
        {
          // --------cq 420-------jlh--------
          export.Minus.Text1 = "-";
          export.Plus.Text1 = "";
          ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
      }
      else if (export.Event2.ControlNumber > 0)
      {
        ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

        var field = GetField(export.Event2, "controlNumber");

        field.Error = true;

        // --------cq 420-------jlh--------
        export.Minus.Text1 = "-";
        export.Plus.Text1 = "";
      }
    }
  }

  private static void MoveEvent1(Event1 source, Event1 target)
  {
    target.ControlNumber = source.ControlNumber;
    target.Name = source.Name;
  }

  private static void MoveEventDetail(EventDetail source, EventDetail target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReasonCode = source.ReasonCode;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.PassToDateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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

  private void UseSpCabCreateEventDetail()
  {
    var useImport = new SpCabCreateEventDetail.Import();
    var useExport = new SpCabCreateEventDetail.Export();

    useImport.Event1.ControlNumber = import.Event2.ControlNumber;
    useImport.EventDetail.Assign(local.PassToEventDetail);

    Call(SpCabCreateEventDetail.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.EventDetail);
  }

  private void UseSpCabDeleteEventDetail()
  {
    var useImport = new SpCabDeleteEventDetail.Import();
    var useExport = new SpCabDeleteEventDetail.Export();

    useImport.Event1.ControlNumber = import.Event2.ControlNumber;
    useImport.EventDetail.SystemGeneratedIdentifier =
      local.PassToEventDetail.SystemGeneratedIdentifier;

    Call(SpCabDeleteEventDetail.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateEventDetail()
  {
    var useImport = new SpCabUpdateEventDetail.Import();
    var useExport = new SpCabUpdateEventDetail.Export();

    useImport.Event1.ControlNumber = import.Event2.ControlNumber;
    useImport.EventDetail.Assign(local.PassToEventDetail);

    Call(SpCabUpdateEventDetail.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.EventDetail);
  }

  private bool ReadActivityStartStop()
  {
    entities.ActivityStartStop.Populated = false;

    return Read("ReadActivityStartStop",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdId",
          export.Group.Item.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", import.Event2.ControlNumber);
      },
      (db, reader) =>
      {
        entities.ActivityStartStop.ActionCode = db.GetString(reader, 0);
        entities.ActivityStartStop.ActNo = db.GetInt32(reader, 1);
        entities.ActivityStartStop.AcdId = db.GetInt32(reader, 2);
        entities.ActivityStartStop.EveNo = db.GetInt32(reader, 3);
        entities.ActivityStartStop.EvdId = db.GetInt32(reader, 4);
        entities.ActivityStartStop.Populated = true;
      });
  }

  private bool ReadAlertDistributionRule()
  {
    entities.AlertDistributionRule.Populated = false;

    return Read("ReadAlertDistributionRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdId",
          export.Group.Item.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", import.Event2.ControlNumber);
      },
      (db, reader) =>
      {
        entities.AlertDistributionRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AlertDistributionRule.ReasonCode =
          db.GetNullableString(reader, 1);
        entities.AlertDistributionRule.EveNo = db.GetInt32(reader, 2);
        entities.AlertDistributionRule.EvdId = db.GetInt32(reader, 3);
        entities.AlertDistributionRule.Populated = true;
      });
  }

  private bool ReadDocument()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "evdId",
          export.Group.Item.EventDetail.SystemGeneratedIdentifier);
        db.SetNullableInt32(command, "eveNo", import.Event2.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.EveNo = db.GetNullableInt32(reader, 1);
        entities.Document.EvdId = db.GetNullableInt32(reader, 2);
        entities.Document.EffectiveDate = db.GetDate(reader, 3);
        entities.Document.Populated = true;
      });
  }

  private bool ReadEvent()
  {
    entities.Existing.Populated = false;

    return Read("ReadEvent",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", export.Event2.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Existing.ControlNumber = db.GetInt32(reader, 0);
        entities.Existing.Name = db.GetString(reader, 1);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadEventDetail1()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail1",
      (db, command) =>
      {
        db.SetString(
          command, "detailName", export.Group.Item.EventDetail.DetailName);
        db.SetInt32(command, "eveNo", import.Event2.ControlNumber);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.DetailName = db.GetString(reader, 1);
        entities.EventDetail.Description = db.GetNullableString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.CsenetInOutCode = db.GetString(reader, 4);
        entities.EventDetail.ReasonCode = db.GetString(reader, 5);
        entities.EventDetail.ProcedureName = db.GetNullableString(reader, 6);
        entities.EventDetail.LifecycleImpactCode = db.GetString(reader, 7);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 8);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 9);
        entities.EventDetail.NextEventId = db.GetNullableInt32(reader, 10);
        entities.EventDetail.NextEventDetailId =
          db.GetNullableString(reader, 11);
        entities.EventDetail.EffectiveDate = db.GetDate(reader, 12);
        entities.EventDetail.DiscontinueDate = db.GetDate(reader, 13);
        entities.EventDetail.EveNo = db.GetInt32(reader, 14);
        entities.EventDetail.Function = db.GetNullableString(reader, 15);
        entities.EventDetail.ExceptionRoutine =
          db.GetNullableString(reader, 16);
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadEventDetail2()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.Group.Item.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", import.Event2.ControlNumber);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.DetailName = db.GetString(reader, 1);
        entities.EventDetail.Description = db.GetNullableString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.CsenetInOutCode = db.GetString(reader, 4);
        entities.EventDetail.ReasonCode = db.GetString(reader, 5);
        entities.EventDetail.ProcedureName = db.GetNullableString(reader, 6);
        entities.EventDetail.LifecycleImpactCode = db.GetString(reader, 7);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 8);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 9);
        entities.EventDetail.NextEventId = db.GetNullableInt32(reader, 10);
        entities.EventDetail.NextEventDetailId =
          db.GetNullableString(reader, 11);
        entities.EventDetail.EffectiveDate = db.GetDate(reader, 12);
        entities.EventDetail.DiscontinueDate = db.GetDate(reader, 13);
        entities.EventDetail.EveNo = db.GetInt32(reader, 14);
        entities.EventDetail.Function = db.GetNullableString(reader, 15);
        entities.EventDetail.ExceptionRoutine =
          db.GetNullableString(reader, 16);
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadEventDetail3()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail3",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          local.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "eveNo",
          export.Group.Item.EventDetail.NextEventId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.DetailName = db.GetString(reader, 1);
        entities.EventDetail.Description = db.GetNullableString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.CsenetInOutCode = db.GetString(reader, 4);
        entities.EventDetail.ReasonCode = db.GetString(reader, 5);
        entities.EventDetail.ProcedureName = db.GetNullableString(reader, 6);
        entities.EventDetail.LifecycleImpactCode = db.GetString(reader, 7);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 8);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 9);
        entities.EventDetail.NextEventId = db.GetNullableInt32(reader, 10);
        entities.EventDetail.NextEventDetailId =
          db.GetNullableString(reader, 11);
        entities.EventDetail.EffectiveDate = db.GetDate(reader, 12);
        entities.EventDetail.DiscontinueDate = db.GetDate(reader, 13);
        entities.EventDetail.EveNo = db.GetInt32(reader, 14);
        entities.EventDetail.Function = db.GetNullableString(reader, 15);
        entities.EventDetail.ExceptionRoutine =
          db.GetNullableString(reader, 16);
        entities.EventDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadEventDetail4()
  {
    entities.EventDetail.Populated = false;

    return ReadEach("ReadEventDetail4",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.Existing.ControlNumber);
        db.SetString(command, "reasonCode", local.Read.ReasonCode);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.DetailName = db.GetString(reader, 1);
        entities.EventDetail.Description = db.GetNullableString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.CsenetInOutCode = db.GetString(reader, 4);
        entities.EventDetail.ReasonCode = db.GetString(reader, 5);
        entities.EventDetail.ProcedureName = db.GetNullableString(reader, 6);
        entities.EventDetail.LifecycleImpactCode = db.GetString(reader, 7);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 8);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 9);
        entities.EventDetail.NextEventId = db.GetNullableInt32(reader, 10);
        entities.EventDetail.NextEventDetailId =
          db.GetNullableString(reader, 11);
        entities.EventDetail.EffectiveDate = db.GetDate(reader, 12);
        entities.EventDetail.DiscontinueDate = db.GetDate(reader, 13);
        entities.EventDetail.EveNo = db.GetInt32(reader, 14);
        entities.EventDetail.Function = db.GetNullableString(reader, 15);
        entities.EventDetail.ExceptionRoutine =
          db.GetNullableString(reader, 16);
        entities.EventDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadEventDetail5()
  {
    entities.EventDetail.Populated = false;

    return ReadEach("ReadEventDetail5",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.Existing.ControlNumber);
        db.SetInt32(
          command, "systemGeneratedI", local.Read.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.DetailName = db.GetString(reader, 1);
        entities.EventDetail.Description = db.GetNullableString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.CsenetInOutCode = db.GetString(reader, 4);
        entities.EventDetail.ReasonCode = db.GetString(reader, 5);
        entities.EventDetail.ProcedureName = db.GetNullableString(reader, 6);
        entities.EventDetail.LifecycleImpactCode = db.GetString(reader, 7);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 8);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 9);
        entities.EventDetail.NextEventId = db.GetNullableInt32(reader, 10);
        entities.EventDetail.NextEventDetailId =
          db.GetNullableString(reader, 11);
        entities.EventDetail.EffectiveDate = db.GetDate(reader, 12);
        entities.EventDetail.DiscontinueDate = db.GetDate(reader, 13);
        entities.EventDetail.EveNo = db.GetInt32(reader, 14);
        entities.EventDetail.Function = db.GetNullableString(reader, 15);
        entities.EventDetail.ExceptionRoutine =
          db.GetNullableString(reader, 16);
        entities.EventDetail.Populated = true;

        return true;
      });
  }

  private bool ReadLifecycleTransformation1()
  {
    entities.LifecycleTransformation.Populated = false;

    return Read("ReadLifecycleTransformation1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "evdLctIdSec",
          export.Group.Item.EventDetail.SystemGeneratedIdentifier);
        db.SetNullableInt32(command, "eveNoSec", import.Event2.ControlNumber);
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

  private bool ReadLifecycleTransformation2()
  {
    entities.LifecycleTransformation.Populated = false;

    return Read("ReadLifecycleTransformation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdIdPri",
          export.Group.Item.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveCtrlNoPri", import.Event2.ControlNumber);
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public EventDetail Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
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
      /// A value of EventDetail.
      /// </summary>
      [JsonPropertyName("eventDetail")]
      public EventDetail EventDetail
      {
        get => eventDetail ??= new();
        set => eventDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private EventDetail hidden;
      private Common common;
      private EventDetail eventDetail;
    }

    /// <summary>A PageKeyGroup group.</summary>
    [Serializable]
    public class PageKeyGroup
    {
      /// <summary>
      /// A value of PageStart.
      /// </summary>
      [JsonPropertyName("pageStart")]
      public EventDetail PageStart
      {
        get => pageStart ??= new();
        set => pageStart = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 250;

      private EventDetail pageStart;
    }

    /// <summary>
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
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
    /// Gets a value of PageKey.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeyGroup> PageKey => pageKey ??= new(
      PageKeyGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKey for json serialization.
    /// </summary>
    [JsonPropertyName("pageKey")]
    [Computed]
    public IList<PageKeyGroup> PageKey_Json
    {
      get => pageKey;
      set => PageKey.Assign(value);
    }

    /// <summary>
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Standard PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public TextWorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public TextWorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public EventDetail Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of FromLink.
    /// </summary>
    [JsonPropertyName("fromLink")]
    public Event1 FromLink
    {
      get => fromLink ??= new();
      set => fromLink = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Standard Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of HiddenCheck.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    public Event1 HiddenCheck
    {
      get => hiddenCheck ??= new();
      set => hiddenCheck = value;
    }

    /// <summary>
    /// A value of Event2.
    /// </summary>
    [JsonPropertyName("event2")]
    public Event1 Event2
    {
      get => event2 ??= new();
      set => event2 = value;
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

    private Common flag;
    private Array<GroupGroup> group;
    private Array<PageKeyGroup> pageKey;
    private Standard pageNumber;
    private TextWorkArea plus;
    private TextWorkArea minus;
    private EventDetail starting;
    private Event1 fromLink;
    private Standard event1;
    private Event1 hiddenCheck;
    private Event1 event2;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public EventDetail Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
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
      /// A value of EventDetail.
      /// </summary>
      [JsonPropertyName("eventDetail")]
      public EventDetail EventDetail
      {
        get => eventDetail ??= new();
        set => eventDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private EventDetail hidden;
      private Common common;
      private EventDetail eventDetail;
    }

    /// <summary>A PageKeyGroup group.</summary>
    [Serializable]
    public class PageKeyGroup
    {
      /// <summary>
      /// A value of PageStart.
      /// </summary>
      [JsonPropertyName("pageStart")]
      public EventDetail PageStart
      {
        get => pageStart ??= new();
        set => pageStart = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 250;

      private EventDetail pageStart;
    }

    /// <summary>
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
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
    /// Gets a value of PageKey.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeyGroup> PageKey => pageKey ??= new(
      PageKeyGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKey for json serialization.
    /// </summary>
    [JsonPropertyName("pageKey")]
    [Computed]
    public IList<PageKeyGroup> PageKey_Json
    {
      get => pageKey;
      set => PageKey.Assign(value);
    }

    /// <summary>
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Standard PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public TextWorkArea Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public TextWorkArea Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public EventDetail Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of ToTranEventDetail.
    /// </summary>
    [JsonPropertyName("toTranEventDetail")]
    public EventDetail ToTranEventDetail
    {
      get => toTranEventDetail ??= new();
      set => toTranEventDetail = value;
    }

    /// <summary>
    /// A value of ToTranEvent.
    /// </summary>
    [JsonPropertyName("toTranEvent")]
    public Event1 ToTranEvent
    {
      get => toTranEvent ??= new();
      set => toTranEvent = value;
    }

    /// <summary>
    /// A value of ToTranCode.
    /// </summary>
    [JsonPropertyName("toTranCode")]
    public Code ToTranCode
    {
      get => toTranCode ??= new();
      set => toTranCode = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Standard Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of HiddenCheck.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    public Event1 HiddenCheck
    {
      get => hiddenCheck ??= new();
      set => hiddenCheck = value;
    }

    /// <summary>
    /// A value of Event2.
    /// </summary>
    [JsonPropertyName("event2")]
    public Event1 Event2
    {
      get => event2 ??= new();
      set => event2 = value;
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

    private Common flag;
    private Array<GroupGroup> group;
    private Array<PageKeyGroup> pageKey;
    private Standard pageNumber;
    private TextWorkArea plus;
    private TextWorkArea minus;
    private EventDetail starting;
    private EventDetail toTranEventDetail;
    private Event1 toTranEvent;
    private Code toTranCode;
    private Standard event1;
    private Event1 hiddenCheck;
    private Event1 event2;
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
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public EventDetail Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of LnullDate.
    /// </summary>
    [JsonPropertyName("lnullDate")]
    public DateWorkArea LnullDate
    {
      get => lnullDate ??= new();
      set => lnullDate = value;
    }

    /// <summary>
    /// A value of PassToDateWorkArea.
    /// </summary>
    [JsonPropertyName("passToDateWorkArea")]
    public DateWorkArea PassToDateWorkArea
    {
      get => passToDateWorkArea ??= new();
      set => passToDateWorkArea = value;
    }

    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public EventDetail Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
    }

    /// <summary>
    /// A value of ReturnFromValidation.
    /// </summary>
    [JsonPropertyName("returnFromValidation")]
    public Common ReturnFromValidation
    {
      get => returnFromValidation ??= new();
      set => returnFromValidation = value;
    }

    /// <summary>
    /// A value of PassToValidateCodeValue.
    /// </summary>
    [JsonPropertyName("passToValidateCodeValue")]
    public CodeValue PassToValidateCodeValue
    {
      get => passToValidateCodeValue ??= new();
      set => passToValidateCodeValue = value;
    }

    /// <summary>
    /// A value of PassToValidateCode.
    /// </summary>
    [JsonPropertyName("passToValidateCode")]
    public Code PassToValidateCode
    {
      get => passToValidateCode ??= new();
      set => passToValidateCode = value;
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
    /// A value of ReturnFrom.
    /// </summary>
    [JsonPropertyName("returnFrom")]
    public EventDetail ReturnFrom
    {
      get => returnFrom ??= new();
      set => returnFrom = value;
    }

    /// <summary>
    /// A value of PassToEventDetail.
    /// </summary>
    [JsonPropertyName("passToEventDetail")]
    public EventDetail PassToEventDetail
    {
      get => passToEventDetail ??= new();
      set => passToEventDetail = value;
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

    private EventDetail read;
    private DateWorkArea lnullDate;
    private DateWorkArea passToDateWorkArea;
    private EventDetail initialize;
    private Common returnFromValidation;
    private CodeValue passToValidateCodeValue;
    private Code passToValidateCode;
    private EventDetail eventDetail;
    private EventDetail returnFrom;
    private EventDetail passToEventDetail;
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
    /// A value of ActivityStartStop.
    /// </summary>
    [JsonPropertyName("activityStartStop")]
    public ActivityStartStop ActivityStartStop
    {
      get => activityStartStop ??= new();
      set => activityStartStop = value;
    }

    /// <summary>
    /// A value of AlertDistributionRule.
    /// </summary>
    [JsonPropertyName("alertDistributionRule")]
    public AlertDistributionRule AlertDistributionRule
    {
      get => alertDistributionRule ??= new();
      set => alertDistributionRule = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Event1 Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private LifecycleTransformation lifecycleTransformation;
    private ActivityStartStop activityStartStop;
    private AlertDistributionRule alertDistributionRule;
    private Document document;
    private EventDetail eventDetail;
    private Event1 existing;
  }
#endregion
}
