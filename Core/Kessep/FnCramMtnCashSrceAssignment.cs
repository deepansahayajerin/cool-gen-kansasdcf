// Program: FN_CRAM_MTN_CASH_SRCE_ASSIGNMENT, ID: 372315632, model: 746.
// Short name: SWECRAMP
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
/// <para>
/// A program: FN_CRAM_MTN_CASH_SRCE_ASSIGNMENT.
/// </para>
/// <para>
/// RESP: CASHMGMNT
/// This procedure will allow for the assignment, unassignment, and display of 
/// assignments of cash receipt source types that have been assigned to a
/// service provider.  This procedure provides three options:  list assignments,
/// list sources, and process assignments.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCramMtnCashSrceAssignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRAM_MTN_CASH_SRCE_ASSIGNMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCramMtnCashSrceAssignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCramMtnCashSrceAssignment.
  /// </summary>
  public FnCramMtnCashSrceAssignment(IContext context, Import import,
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
    // ------------------------------------------------------------------------
    // Date 	  	Developer Name     Description
    // 02/07/96	Holly Kennedy-MTW  Retrofits
    // 12/16/96	R. Marchman	   Add new security/next tran
    // 02/07/97	R. Welborn         Numerous Fixes
    // 04/28/97	JF. Caillouet	   Change Current Date
    // 06/14/97	T.O.Redmond	   Allow Multiple Assignments by Source
    // 
    // 2/8/99-3/11/99  S. Newman          Increase view size; Make Assign and
    // UnAssign two separate processes with their own PF Key; correct date
    // format; add edits to prevent duplicate assignments to the same source;
    // change the order of the PFKeys, prevent assignment to invalid service
    // providers; added prompt to SRVO, added 'Clear Successful' message;
    // changed to read source filter for List Sources; changed default of person
    // signed on for  List Unassigned and List Sources, allowed display of
    // multiple assignments for the same source for List Sources; modified logic
    // for List UnAssigned; added prompt to CRSL; revised date edits; added
    // edits if Assign or UnAssign function is chosen without selecting a detail
    // line;  added edits to not allow Clear, PF7 or PF8 if 's' is on detail
    // line; added edits to not allow '0101001' date; added prompt to CRUC;
    // rewrote read  statement for UnAssign function to read the correct field.
    // ------------------------------------------------------------------------
    // 08/27/99          SWSRPDP   PR-123  Add code to allow multiple
    // Error's without highlighting
    // preceding blank selections
    local.Current.Date = Now().Date;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // Set Initial Exit State.
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // Move all Imports to Exports.
    MoveServiceProvider(import.Search, export.Search);
    export.ShowHistory.Flag = import.ShowHistory.Flag;
    export.Input.Code = import.CashReceiptSourceType.Code;
    export.PromptSourceCode.SelectChar = import.PromptSourceCode.SelectChar;
    export.HiddenCashReceiptSourceType.Code =
      import.HiddenCashReceiptSourceType.Code;
    MoveServiceProvider(import.HiddenServiceProvider,
      export.HiddenServiceProvider);
    export.PromptServiceProvider.SelectChar =
      import.PromptServiceProvider.SelectChar;
    export.Office.SystemGeneratedId = import.Office.SystemGeneratedId;

    // Set the maximum date in local view to date determined in common action 
    // block "set_maximum_discontinued_date".  This date will be compared to
    // research assignment discontinued date.
    // Move Import Group View to Export Group View.
    export.Assgn.Index = -1;

    for(import.Assgn.Index = 0; import.Assgn.Index < import.Assgn.Count; ++
      import.Assgn.Index)
    {
      ++export.Assgn.Index;
      export.Assgn.CheckSize();

      export.Assgn.Update.GexportAction.ActionEntry =
        import.Assgn.Item.Action.ActionEntry;
      export.Assgn.Update.GcashReceiptSourceType.Assign(
        import.Assgn.Item.CashReceiptSourceType);
      MoveReceiptResearchAssignment(import.Assgn.Item.ReceiptResearchAssignment,
        export.Assgn.Update.GreceiptResearchAssignment);
      export.Assgn.Update.Detail.UserId = import.Assgn.Item.Detail.UserId;
    }

    // ************************************************
    // *Next Tran/Security logic.                     *
    // ************************************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // No views to populate
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
      // *****No views to populate*****
      UseScCabNextTranGet();
      global.Command = "LIST_SOU";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // ************************************************
    // *Validate action level security.               *
    // ************************************************
    if (Equal(global.Command, "ASSIGN") || Equal
      (global.Command, "UNASSIGN") || Equal(global.Command, "LIST") || Equal
      (global.Command, "RETCRSL") || Equal(global.Command, "RETSVPO") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "ENTER") || Equal
      (global.Command, "CRUC") || Equal(global.Command, "RETCRUC"))
    {
    }
    else
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

    // 08/27/99          SWSRPDP   PR-123
    local.InvalidSelectCode.Flag = "N";

    for(export.Assgn.Index = 0; export.Assgn.Index < export.Assgn.Count; ++
      export.Assgn.Index)
    {
      if (!export.Assgn.CheckSize())
      {
        break;
      }

      if (Equal(export.Assgn.Item.GexportAction.ActionEntry, "S"))
      {
        // ***** Valid Option.  Continue Processing. *****
      }
      else if (IsEmpty(export.Assgn.Item.GexportAction.ActionEntry))
      {
        // ***** Valid Option.  Continue Processing. *****
      }
      else if (Equal(export.Assgn.Item.GexportAction.ActionEntry, "*"))
      {
        // ***** Valid Option.  Continue Processing. *****
      }
      else
      {
        var field = GetField(export.Assgn.Item.GexportAction, "actionEntry");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        // 08/27/99          SWSRPDP   PR-123
        local.InvalidSelectCode.Flag = "Y";
      }
    }

    export.Assgn.CheckIndex();

    // 08/27/99          SWSRPDP   PR-123
    if (AsChar(local.InvalidSelectCode.Flag) == 'Y')
    {
      ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

      return;
    }

    if (Equal(global.Command, "RETCRSL"))
    {
      if (IsEmpty(import.CashReceiptSourceType.Code))
      {
        export.Input.Code = export.HiddenCashReceiptSourceType.Code;
      }
      else
      {
        export.Input.Code = import.CashReceiptSourceType.Code;
      }

      if (!IsEmpty(export.Search.UserId))
      {
        export.Search.UserId = "";
      }

      global.Command = "LIST_ASG";
    }

    if (Equal(global.Command, "RETSVPO"))
    {
      if (IsEmpty(import.Search.UserId))
      {
        MoveServiceProvider(export.HiddenServiceProvider, export.Search);
      }
      else
      {
        MoveServiceProvider(import.Search, export.Search);
      }

      if (!IsEmpty(export.Input.Code))
      {
        export.Input.Code = "";
      }

      global.Command = "LIST_ASG";
    }

    if (Equal(global.Command, "RETCRUC"))
    {
      if (!IsEmpty(export.HiddenServiceProvider.UserId))
      {
        if (!IsEmpty(export.HiddenCashReceiptSourceType.Code))
        {
          export.Input.Code = "";
        }

        MoveServiceProvider(export.HiddenServiceProvider, export.Search);
      }
      else
      {
        export.Input.Code = export.HiddenCashReceiptSourceType.Code;
      }

      global.Command = "LIST_ASG";
    }

    // MAIN CASE OF COMMAND
    switch(TrimEnd(global.Command))
    {
      case "CLEAR":
        break;
      case "CRUC":
        MoveServiceProvider(export.Search, export.HiddenServiceProvider);
        export.HiddenCashReceiptSourceType.Code = export.Input.Code;
        ExitState = "ECO_LNK_TO_LST_CRUC_UNDSTRBTD_CL";

        break;
      case "LIST":
        switch(AsChar(export.PromptSourceCode.SelectChar))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptSourceCode, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptServiceProvider.SelectChar))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptServiceProvider, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(local.PromptCount.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            return;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            var field1 = GetField(export.PromptServiceProvider, "selectChar");

            field1.Error = true;

            var field2 = GetField(export.PromptSourceCode, "selectChar");

            field2.Error = true;

            return;
        }

        if (AsChar(export.PromptServiceProvider.SelectChar) == 'S')
        {
          // ****Link to List Service Provider Screen.****
          export.PromptServiceProvider.SelectChar = "+";
          export.Office.SystemGeneratedId = 30;
          MoveServiceProvider(export.Search, export.HiddenServiceProvider);
          ExitState = "ECO_LNK_TO_SVPO";

          return;
        }
        else
        {
        }

        if (AsChar(export.PromptSourceCode.SelectChar) == 'S')
        {
          // ****Link to List Cash Receipt Source Code Screen.****
          export.PromptSourceCode.SelectChar = "+";
          export.HiddenCashReceiptSourceType.Code = export.Input.Code;
          ExitState = "ECO_LNK_TO_CASH_RECEIPT_SRC_TYPE";
        }
        else
        {
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LIST_ASG":
        if (!IsEmpty(export.Input.Code) && !IsEmpty(export.Search.UserId))
        {
          var field1 = GetField(export.Input, "code");

          field1.Error = true;

          var field2 = GetField(export.Search, "userId");

          field2.Error = true;

          ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";

          return;
        }

        if (IsEmpty(export.Search.UserId) && IsEmpty(export.Input.Code))
        {
          export.Search.UserId = global.UserId;
        }

        // *****Clear out export group view******
        export.Assgn.Count = 0;

        if (!IsEmpty(export.Search.UserId))
        {
          // ************************************************
          // *Search using the Filter USERID.               *
          // ************************************************
          if (ReadServiceProvider())
          {
            if (AsChar(export.ShowHistory.Flag) == 'Y')
            {
              // ************************************************
              // *Show History of Assignments for 1 Service     *
              // *Provider.
              // 
              // *
              // ************************************************
              export.Assgn.Index = -1;

              foreach(var item in ReadCashReceiptSourceTypeReceiptResearchAssignment2())
                
              {
                if (export.Assgn.IsFull)
                {
                  ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

                  return;
                }

                ++export.Assgn.Index;
                export.Assgn.CheckSize();

                export.Assgn.Update.GexportAction.ActionEntry = "";
                export.Assgn.Update.GcashReceiptSourceType.Assign(
                  entities.ExistingCashReceiptSourceType);
                MoveReceiptResearchAssignment(entities.
                  ExistingReceiptResearchAssignment,
                  export.Assgn.Update.GreceiptResearchAssignment);
                MoveServiceProvider(entities.ExistingServiceProvider,
                  export.Assgn.Update.Detail);
                local.MaxDate.Date =
                  export.Assgn.Item.GreceiptResearchAssignment.DiscontinueDate;
                export.Assgn.Update.GreceiptResearchAssignment.DiscontinueDate =
                  UseCabSetMaximumDiscontinueDate();
              }
            }
            else if (AsChar(export.ShowHistory.Flag) != 'Y')
            {
              // ************************************************
              // *Show Assignments for input Service Provider   *
              // *but do not show history                       *
              // ************************************************
              export.Assgn.Index = -1;

              foreach(var item in ReadCashReceiptSourceTypeReceiptResearchAssignment1())
                
              {
                if (export.Assgn.IsFull)
                {
                  ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

                  return;
                }

                ++export.Assgn.Index;
                export.Assgn.CheckSize();

                export.Assgn.Update.GexportAction.ActionEntry = "";
                export.Assgn.Update.GcashReceiptSourceType.Assign(
                  entities.ExistingCashReceiptSourceType);
                MoveReceiptResearchAssignment(entities.
                  ExistingReceiptResearchAssignment,
                  export.Assgn.Update.GreceiptResearchAssignment);
                MoveServiceProvider(entities.ExistingServiceProvider,
                  export.Assgn.Update.Detail);
                local.MaxDate.Date =
                  export.Assgn.Item.GreceiptResearchAssignment.DiscontinueDate;
                export.Assgn.Update.GreceiptResearchAssignment.DiscontinueDate =
                  UseCabSetMaximumDiscontinueDate();
              }
            }
          }
          else
          {
            var field = GetField(export.Search, "userId");

            field.Error = true;

            ExitState = "SERVICE_PROVIDER_NF";

            return;
          }

          if (export.Assgn.IsEmpty)
          {
            ExitState = "FN0000_NO_SRC_ASSIGNED";
          }
        }
        else if (!IsEmpty(export.Input.Code))
        {
          // ************************************************
          // *User has requested list for a specific source *
          // ************************************************
          // *****Clear out export group view******
          export.Assgn.Count = 0;
          export.Assgn.Index = -1;

          if (ReadCashReceiptSourceType1())
          {
            if (AsChar(export.ShowHistory.Flag) == 'Y')
            {
              foreach(var item in ReadServiceProviderReceiptResearchAssignment3())
                
              {
                if (export.Assgn.IsFull)
                {
                  ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

                  return;
                }

                ++export.Assgn.Index;
                export.Assgn.CheckSize();

                export.Assgn.Update.GexportAction.ActionEntry = "";
                export.Assgn.Update.GcashReceiptSourceType.Assign(
                  entities.ExistingCashReceiptSourceType);
                MoveReceiptResearchAssignment(entities.
                  ExistingReceiptResearchAssignment,
                  export.Assgn.Update.GreceiptResearchAssignment);
                MoveServiceProvider(entities.ExistingServiceProvider,
                  export.Assgn.Update.Detail);
                local.MaxDate.Date =
                  export.Assgn.Item.GreceiptResearchAssignment.DiscontinueDate;
                export.Assgn.Update.GreceiptResearchAssignment.DiscontinueDate =
                  UseCabSetMaximumDiscontinueDate();
              }
            }
            else if (AsChar(export.ShowHistory.Flag) != 'Y')
            {
              export.Assgn.Index = -1;

              foreach(var item in ReadServiceProviderReceiptResearchAssignment2())
                
              {
                if (export.Assgn.IsFull)
                {
                  ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

                  return;
                }

                ++export.Assgn.Index;
                export.Assgn.CheckSize();

                export.Assgn.Update.GexportAction.ActionEntry = "";
                export.Assgn.Update.GcashReceiptSourceType.Assign(
                  entities.ExistingCashReceiptSourceType);
                MoveReceiptResearchAssignment(entities.
                  ExistingReceiptResearchAssignment,
                  export.Assgn.Update.GreceiptResearchAssignment);
                MoveServiceProvider(entities.ExistingServiceProvider,
                  export.Assgn.Update.Detail);
                local.MaxDate.Date =
                  export.Assgn.Item.GreceiptResearchAssignment.DiscontinueDate;
                export.Assgn.Update.GreceiptResearchAssignment.DiscontinueDate =
                  UseCabSetMaximumDiscontinueDate();
              }
            }
          }
          else
          {
            var field = GetField(export.Input, "code");

            field.Error = true;

            ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

            return;
          }

          if (export.Assgn.IsEmpty)
          {
            ExitState = "FN0000_RCPT_RESEARCH_ASSGNMNT_NF";
          }
        }
        else
        {
          var field1 = GetField(export.Input, "code");

          field1.Error = true;

          var field2 = GetField(export.Search, "userId");

          field2.Error = true;

          ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";

          return;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "LIST_SOU":
        export.Input.Code = "";

        // *****Clear out export group view******
        export.Assgn.Count = 0;
        export.Assgn.Index = -1;

        foreach(var item in ReadCashReceiptSourceType3())
        {
          foreach(var item1 in ReadServiceProviderReceiptResearchAssignment1())
          {
            if (export.Assgn.IsFull)
            {
              ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

              return;
            }

            ++export.Assgn.Index;
            export.Assgn.CheckSize();

            local.Common.Flag = "Y";
            export.Assgn.Update.GcashReceiptSourceType.Assign(
              entities.ExistingCashReceiptSourceType);
            export.Assgn.Update.GexportAction.ActionEntry = "";
            MoveServiceProvider(entities.ExistingServiceProvider,
              export.Assgn.Update.Detail);
            MoveReceiptResearchAssignment(entities.
              ExistingReceiptResearchAssignment,
              export.Assgn.Update.GreceiptResearchAssignment);

            if (Equal(export.Assgn.Item.GreceiptResearchAssignment.
              DiscontinueDate, new DateTime(2099, 12, 31)))
            {
              export.Assgn.Update.GreceiptResearchAssignment.DiscontinueDate =
                local.InitializedReceiptResearchAssignment.DiscontinueDate;
            }
          }

          if (export.Assgn.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

            return;
          }

          if (AsChar(local.Common.Flag) != 'Y')
          {
            ++export.Assgn.Index;
            export.Assgn.CheckSize();

            local.Common.Flag = "Y";
            export.Assgn.Update.GcashReceiptSourceType.Assign(
              entities.ExistingCashReceiptSourceType);
            export.Assgn.Update.GexportAction.ActionEntry = "";
          }
          else
          {
          }

          local.Common.Flag = "";
        }

        if (export.Assgn.IsEmpty)
        {
          ExitState = "FN0000_RCPT_RESEARCH_ASSGNMNT_NF";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "L_UNASG":
        // ************************************************
        // *List Source Codes that are not assigned.      *
        // ************************************************
        // *****Clear out export group view******
        export.Assgn.Count = 0;

        if (!IsEmpty(export.Search.UserId))
        {
          export.Search.UserId = "";
        }

        export.Input.Code = "";
        export.Assgn.Index = -1;

        foreach(var item in ReadCashReceiptSourceType2())
        {
          if (export.Assgn.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

            return;
          }

          if (ReadReceiptResearchAssignmentServiceProvider())
          {
            // *****
            // Do not want assigned Sources
            // *****
            continue;
          }
          else
          {
            ++export.Assgn.Index;
            export.Assgn.CheckSize();

            export.Assgn.Update.Detail.UserId = "";
            export.Assgn.Update.GreceiptResearchAssignment.DiscontinueDate =
              local.InitializedDateWorkArea.Date;
            export.Assgn.Update.GreceiptResearchAssignment.EffectiveDate =
              local.InitializedDateWorkArea.Date;
            export.Assgn.Update.GcashReceiptSourceType.Assign(
              entities.ExistingCashReceiptSourceType);
            export.Assgn.Update.GexportAction.ActionEntry = "";
          }

          export.Assgn.Update.GcashReceiptSourceType.Assign(
            entities.ExistingCashReceiptSourceType);
        }

        if (export.Assgn.IsEmpty)
        {
          ExitState = "FN0000_RCPT_RESEARCH_ASSGNMNT_NF";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "ASSIGN":
        export.Assgn.Index = -1;
        local.Assigned.Flag = "N";

        // 08/27/99          SWSRPDP   PR-123
        local.InvalidSelectCode.Flag = "N";
        export.Assgn.Index = 0;

        for(var limit = export.Assgn.Count; export.Assgn.Index < limit; ++
          export.Assgn.Index)
        {
          if (!export.Assgn.CheckSize())
          {
            break;
          }

          if (Equal(export.Assgn.Item.GexportAction.ActionEntry, "S"))
          {
            if (ReadServiceProvider())
            {
              MoveServiceProvider(entities.ExistingServiceProvider,
                export.Search);
            }
            else
            {
              var field = GetField(export.Search, "userId");

              field.Error = true;

              ExitState = "SERVICE_PROVIDER_NF";

              return;
            }

            ++local.ActionCounter.Count;

            // ************************************************
            // *Display Research Assignment Effective Date as *
            // *current date when not supplied.               *
            // ************************************************
            if (Equal(export.Assgn.Item.GreceiptResearchAssignment.
              EffectiveDate, local.InitializedDateWorkArea.Date))
            {
              export.Assgn.Update.GreceiptResearchAssignment.EffectiveDate =
                local.Current.Date;
            }

            // ************************************************
            // *Check for Receipt Research Assignment time    *
            // *conflict exists for chosen Cash Receipt Source*
            // *Code per selected Service Provider.           *
            // ************************************************
            if (ReadReceiptResearchAssignment())
            {
              if (!Lt(export.Assgn.Item.GreceiptResearchAssignment.
                EffectiveDate,
                entities.ExistingReceiptResearchAssignment.EffectiveDate))
              {
                // ************************************************
                // *A current assignment exists for this Source   *
                // *Code, for this Service Provider.              *
                // ************************************************
                var field1 =
                  GetField(export.Assgn.Item.GexportAction, "actionEntry");

                field1.Error = true;

                var field2 =
                  GetField(export.Assgn.Item.GcashReceiptSourceType, "code");

                field2.Error = true;

                ExitState = "RESEARCH_ASSGN_EXISTS_FOR_CODE";

                return;
              }
              else
              {
                // ************************************************
                // *A current open assignment exists for this     *
                // *Source Code, for this Service Provider.       *
                // ************************************************
                if (Lt(export.Assgn.Item.GreceiptResearchAssignment.
                  EffectiveDate,
                  entities.ExistingReceiptResearchAssignment.EffectiveDate))
                {
                  var field1 =
                    GetField(export.Assgn.Item.GexportAction, "actionEntry");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Assgn.Item.GcashReceiptSourceType, "code");

                  field2.Error = true;

                  ExitState = "CURRENT_OPEN_RESEARCH_ASSIGNMENT";

                  return;
                }
              }
            }
            else
            {
              // ************************************************
              // *No current assignment exists for this Service *
              // *Provider, Therefore it is ok to add a new     *
              // *assignment of this service provider to this   *
              // *Source Type.
              // 
              // *
              // ************************************************
              ExitState = "OKAY_TO_ASSIGN";
              UseAssignReceiptResearch();

              if (IsExitState("EFFECTIVE_DATE_PRIOR_TO_CURRENT"))
              {
                var field =
                  GetField(export.Assgn.Item.GreceiptResearchAssignment,
                  "effectiveDate");

                field.Error = true;

                return;
              }

              if (IsExitState("EXPIRE_DATE_PRIOR_TO_EFFECTIVE"))
              {
                var field =
                  GetField(export.Assgn.Item.GreceiptResearchAssignment,
                  "discontinueDate");

                field.Error = true;

                return;
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Assgn.Update.GexportAction.ActionEntry = "*";
                local.Assigned.Flag = "Y";
                export.Assgn.Update.Detail.UserId = export.Search.UserId;
                ExitState = "FN0000_SUCCESSFULLY_ASSIGNED";
              }
              else
              {
                // 08/27/99          SWSRPDP   PR-123
                var field =
                  GetField(export.Assgn.Item.GexportAction, "actionEntry");

                field.Error = true;

                return;
              }
            }
          }
          else if (Equal(export.Assgn.Item.GexportAction.ActionEntry, "*"))
          {
            // *****Valid for newly assigned assignments.*****
          }
          else if (IsEmpty(export.Assgn.Item.GexportAction.ActionEntry))
          {
            // *****Valid because an Assignment has been created*****
          }
          else
          {
            var field =
              GetField(export.Assgn.Item.GexportAction, "actionEntry");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            // 08/27/99          SWSRPDP   PR-123
            local.InvalidSelectCode.Flag = "Y";
          }
        }

        export.Assgn.CheckIndex();

        // 08/27/99          SWSRPDP   PR-123
        if (AsChar(local.InvalidSelectCode.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          return;
        }

        if (AsChar(local.Assigned.Flag) == 'Y')
        {
          ExitState = "FN0000_SUCCESSFULLY_ASSIGNED";

          return;
        }

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        break;
      case "UNASSIGN":
        export.Assgn.Index = -1;
        local.Unassigned.Flag = "N";

        // 08/27/99          SWSRPDP   PR-123
        local.InvalidSelectCode.Flag = "N";
        export.Assgn.Index = 0;

        for(var limit = export.Assgn.Count; export.Assgn.Index < limit; ++
          export.Assgn.Index)
        {
          if (!export.Assgn.CheckSize())
          {
            break;
          }

          if (Equal(export.Assgn.Item.GexportAction.ActionEntry, "S"))
          {
            if (Equal(export.Assgn.Item.GreceiptResearchAssignment.
              EffectiveDate, local.InitializedDateWorkArea.Date))
            {
              var field =
                GetField(export.Assgn.Item.GreceiptResearchAssignment,
                "effectiveDate");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              return;
            }

            // ************************************************
            // *Use current date for the Research Assignment  *
            // *Discontinued Date if none is supplied.        *
            // ************************************************
            if (Equal(export.Assgn.Item.GreceiptResearchAssignment.
              DiscontinueDate, local.InitializedDateWorkArea.Date))
            {
              export.Assgn.Update.GreceiptResearchAssignment.DiscontinueDate =
                local.Current.Date;
            }

            ++local.ActionCounter.Count;
            UseUnassignReceiptResearch();

            if (IsExitState("EXPIRATION_DATE_PRIOR_TO_CURRENT"))
            {
              var field =
                GetField(export.Assgn.Item.GreceiptResearchAssignment,
                "discontinueDate");

              field.Error = true;

              return;
            }

            if (IsExitState("FN0000_RCPT_RESEARCH_ASSGNMNT_NU"))
            {
              var field1 =
                GetField(export.Assgn.Item.GexportAction, "actionEntry");

              field1.Error = true;

              var field2 =
                GetField(export.Assgn.Item.GcashReceiptSourceType, "code");

              field2.Error = true;

              return;
            }

            if (IsExitState("FN0000_RCPT_RESEARCH_ASSGNMNT_NF"))
            {
              var field1 =
                GetField(export.Assgn.Item.GexportAction, "actionEntry");

              field1.Error = true;

              var field2 =
                GetField(export.Assgn.Item.GcashReceiptSourceType, "code");

              field2.Error = true;

              return;
            }

            if (IsExitState("FN0000_RCPT_RESEARCH_ASSGNMNT_PV"))
            {
              var field1 =
                GetField(export.Assgn.Item.GexportAction, "actionEntry");

              field1.Error = true;

              var field2 =
                GetField(export.Assgn.Item.GcashReceiptSourceType, "code");

              field2.Error = true;

              return;
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Assgn.Update.GexportAction.ActionEntry = "*";
              local.Unassigned.Flag = "Y";
              ExitState = "FN0000_UNASSIGNMENT_SUCCESSFUL";
            }
            else
            {
              // 08/27/99          SWSRPDP   PR-123
              var field =
                GetField(export.Assgn.Item.GexportAction, "actionEntry");

              field.Error = true;

              return;
            }
          }
          else if (Equal(export.Assgn.Item.GexportAction.ActionEntry, "*"))
          {
            // *****Valid for newly unassigned assignments.*****
          }
          else if (IsEmpty(export.Assgn.Item.GexportAction.ActionEntry))
          {
            // *****Valid because an Unassignment has been created.*****
          }
          else
          {
            var field =
              GetField(export.Assgn.Item.GexportAction, "actionEntry");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            // 08/27/99          SWSRPDP   PR-123
            local.InvalidSelectCode.Flag = "Y";
          }
        }

        export.Assgn.CheckIndex();

        // 08/27/99          SWSRPDP   PR-123
        if (AsChar(local.InvalidSelectCode.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          return;
        }

        if (AsChar(local.Unassigned.Flag) == 'Y')
        {
          ExitState = "FN0000_UNASSIGNMENT_SUCCESSFUL";

          return;
        }

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        if (Equal(global.Command, "ENTER"))
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_PF_KEY";
        }

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

  private static void MoveReceiptResearchAssignment(
    ReceiptResearchAssignment source, ReceiptResearchAssignment target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private void UseAssignReceiptResearch()
  {
    var useImport = new AssignReceiptResearch.Import();
    var useExport = new AssignReceiptResearch.Export();

    MoveServiceProvider(export.Search, useImport.ServiceProvider);
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.Assgn.Item.GcashReceiptSourceType.SystemGeneratedIdentifier;
    MoveReceiptResearchAssignment(export.Assgn.Item.GreceiptResearchAssignment,
      useImport.ReceiptResearchAssignment);

    Call(AssignReceiptResearch.Execute, useImport, useExport);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.MaxDate.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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

  private void UseUnassignReceiptResearch()
  {
    var useImport = new UnassignReceiptResearch.Import();
    var useExport = new UnassignReceiptResearch.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.Assgn.Item.GcashReceiptSourceType.SystemGeneratedIdentifier;
    MoveReceiptResearchAssignment(export.Assgn.Item.GreceiptResearchAssignment,
      useImport.ReceiptResearchAssignment);
    MoveServiceProvider(export.Assgn.Item.Detail, useImport.ServiceProvider);

    Call(UnassignReceiptResearch.Execute, useImport, useExport);
  }

  private bool ReadCashReceiptSourceType1()
  {
    entities.ExistingCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType1",
      (db, command) =>
      {
        db.SetString(command, "code", export.Input.Code);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.ExistingCashReceiptSourceType.Name = db.GetString(reader, 3);
        entities.ExistingCashReceiptSourceType.EffectiveDate =
          db.GetDate(reader, 4);
        entities.ExistingCashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.ExistingCashReceiptSourceType.InterfaceIndicator);
      });
  }

  private IEnumerable<bool> ReadCashReceiptSourceType2()
  {
    entities.ExistingCashReceiptSourceType.Populated = false;

    return ReadEach("ReadCashReceiptSourceType2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "code", import.CashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.ExistingCashReceiptSourceType.Name = db.GetString(reader, 3);
        entities.ExistingCashReceiptSourceType.EffectiveDate =
          db.GetDate(reader, 4);
        entities.ExistingCashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.ExistingCashReceiptSourceType.InterfaceIndicator);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptSourceType3()
  {
    entities.ExistingCashReceiptSourceType.Populated = false;

    return ReadEach("ReadCashReceiptSourceType3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "code", import.CashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.ExistingCashReceiptSourceType.Name = db.GetString(reader, 3);
        entities.ExistingCashReceiptSourceType.EffectiveDate =
          db.GetDate(reader, 4);
        entities.ExistingCashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.ExistingCashReceiptSourceType.InterfaceIndicator);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptSourceTypeReceiptResearchAssignment1()
  {
    entities.ExistingCashReceiptSourceType.Populated = false;
    entities.ExistingReceiptResearchAssignment.Populated = false;

    return ReadEach("ReadCashReceiptSourceTypeReceiptResearchAssignment1",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdIdentifier",
          entities.ExistingServiceProvider.SystemGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingReceiptResearchAssignment.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.ExistingCashReceiptSourceType.Name = db.GetString(reader, 3);
        entities.ExistingCashReceiptSourceType.EffectiveDate =
          db.GetDate(reader, 4);
        entities.ExistingCashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 6);
        entities.ExistingReceiptResearchAssignment.SpdIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingReceiptResearchAssignment.EffectiveDate =
          db.GetDate(reader, 8);
        entities.ExistingReceiptResearchAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingReceiptResearchAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.ExistingReceiptResearchAssignment.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingCashReceiptSourceType.Populated = true;
        entities.ExistingReceiptResearchAssignment.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.ExistingCashReceiptSourceType.InterfaceIndicator);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptSourceTypeReceiptResearchAssignment2()
  {
    entities.ExistingCashReceiptSourceType.Populated = false;
    entities.ExistingReceiptResearchAssignment.Populated = false;

    return ReadEach("ReadCashReceiptSourceTypeReceiptResearchAssignment2",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdIdentifier",
          entities.ExistingServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingReceiptResearchAssignment.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.ExistingCashReceiptSourceType.Name = db.GetString(reader, 3);
        entities.ExistingCashReceiptSourceType.EffectiveDate =
          db.GetDate(reader, 4);
        entities.ExistingCashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 6);
        entities.ExistingReceiptResearchAssignment.SpdIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingReceiptResearchAssignment.EffectiveDate =
          db.GetDate(reader, 8);
        entities.ExistingReceiptResearchAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingReceiptResearchAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.ExistingReceiptResearchAssignment.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingCashReceiptSourceType.Populated = true;
        entities.ExistingReceiptResearchAssignment.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.ExistingCashReceiptSourceType.InterfaceIndicator);

        return true;
      });
  }

  private bool ReadReceiptResearchAssignment()
  {
    entities.ExistingReceiptResearchAssignment.Populated = false;

    return Read("ReadReceiptResearchAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdIdentifier",
          entities.ExistingServiceProvider.SystemGeneratedId);
        db.SetString(
          command, "code", export.Assgn.Item.GcashReceiptSourceType.Code);
        db.SetNullableDate(
          command, "discontinueDate",
          export.Assgn.Item.GreceiptResearchAssignment.EffectiveDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingReceiptResearchAssignment.SpdIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingReceiptResearchAssignment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingReceiptResearchAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingReceiptResearchAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingReceiptResearchAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ExistingReceiptResearchAssignment.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingReceiptResearchAssignment.Populated = true;
      });
  }

  private bool ReadReceiptResearchAssignmentServiceProvider()
  {
    entities.ExistingReceiptResearchAssignment.Populated = false;
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadReceiptResearchAssignmentServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingReceiptResearchAssignment.SpdIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingReceiptResearchAssignment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingReceiptResearchAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingReceiptResearchAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingReceiptResearchAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ExistingReceiptResearchAssignment.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 6);
        entities.ExistingReceiptResearchAssignment.Populated = true;
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", export.Search.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderReceiptResearchAssignment1()
  {
    entities.ExistingReceiptResearchAssignment.Populated = false;
    entities.ExistingServiceProvider.Populated = false;

    return ReadEach("ReadServiceProviderReceiptResearchAssignment1",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingReceiptResearchAssignment.SpdIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingReceiptResearchAssignment.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingReceiptResearchAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingReceiptResearchAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingReceiptResearchAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ExistingReceiptResearchAssignment.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.ExistingReceiptResearchAssignment.Populated = true;
        entities.ExistingServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderReceiptResearchAssignment2()
  {
    entities.ExistingReceiptResearchAssignment.Populated = false;
    entities.ExistingServiceProvider.Populated = false;

    return ReadEach("ReadServiceProviderReceiptResearchAssignment2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingReceiptResearchAssignment.SpdIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingReceiptResearchAssignment.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingReceiptResearchAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingReceiptResearchAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingReceiptResearchAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ExistingReceiptResearchAssignment.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.ExistingReceiptResearchAssignment.Populated = true;
        entities.ExistingServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderReceiptResearchAssignment3()
  {
    entities.ExistingReceiptResearchAssignment.Populated = false;
    entities.ExistingServiceProvider.Populated = false;

    return ReadEach("ReadServiceProviderReceiptResearchAssignment3",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingReceiptResearchAssignment.SpdIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingReceiptResearchAssignment.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingReceiptResearchAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingReceiptResearchAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingReceiptResearchAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ExistingReceiptResearchAssignment.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.ExistingReceiptResearchAssignment.Populated = true;
        entities.ExistingServiceProvider.Populated = true;

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
    /// <summary>A AssgnGroup group.</summary>
    [Serializable]
    public class AssgnGroup
    {
      /// <summary>
      /// A value of Action.
      /// </summary>
      [JsonPropertyName("action")]
      public Common Action
      {
        get => action ??= new();
        set => action = value;
      }

      /// <summary>
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of ReceiptResearchAssignment.
      /// </summary>
      [JsonPropertyName("receiptResearchAssignment")]
      public ReceiptResearchAssignment ReceiptResearchAssignment
      {
        get => receiptResearchAssignment ??= new();
        set => receiptResearchAssignment = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ServiceProvider Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 196;

      private Common action;
      private CashReceiptSourceType cashReceiptSourceType;
      private ReceiptResearchAssignment receiptResearchAssignment;
      private ServiceProvider detail;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of HiddenServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenServiceProvider")]
    public ServiceProvider HiddenServiceProvider
    {
      get => hiddenServiceProvider ??= new();
      set => hiddenServiceProvider = value;
    }

    /// <summary>
    /// A value of PromptServiceProvider.
    /// </summary>
    [JsonPropertyName("promptServiceProvider")]
    public Common PromptServiceProvider
    {
      get => promptServiceProvider ??= new();
      set => promptServiceProvider = value;
    }

    /// <summary>
    /// A value of PromptSourceCode.
    /// </summary>
    [JsonPropertyName("promptSourceCode")]
    public Common PromptSourceCode
    {
      get => promptSourceCode ??= new();
      set => promptSourceCode = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptSourceType")]
    public CashReceiptSourceType HiddenCashReceiptSourceType
    {
      get => hiddenCashReceiptSourceType ??= new();
      set => hiddenCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// Gets a value of Assgn.
    /// </summary>
    [JsonIgnore]
    public Array<AssgnGroup> Assgn => assgn ??= new(AssgnGroup.Capacity);

    /// <summary>
    /// Gets a value of Assgn for json serialization.
    /// </summary>
    [JsonPropertyName("assgn")]
    [Computed]
    public IList<AssgnGroup> Assgn_Json
    {
      get => assgn;
      set => Assgn.Assign(value);
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ServiceProvider Search
    {
      get => search ??= new();
      set => search = value;
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

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Office office;
    private ServiceProvider hiddenServiceProvider;
    private Common promptServiceProvider;
    private Common promptSourceCode;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private CashReceiptSourceType cashReceiptSourceType;
    private Array<AssgnGroup> assgn;
    private Common showHistory;
    private ServiceProvider search;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A SsGroup group.</summary>
    [Serializable]
    public class SsGroup
    {
      /// <summary>
      /// A value of SsCommon.
      /// </summary>
      [JsonPropertyName("ssCommon")]
      public Common SsCommon
      {
        get => ssCommon ??= new();
        set => ssCommon = value;
      }

      /// <summary>
      /// A value of SsCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("ssCashReceiptSourceType")]
      public CashReceiptSourceType SsCashReceiptSourceType
      {
        get => ssCashReceiptSourceType ??= new();
        set => ssCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of SsReceiptResearchAssignment.
      /// </summary>
      [JsonPropertyName("ssReceiptResearchAssignment")]
      public ReceiptResearchAssignment SsReceiptResearchAssignment
      {
        get => ssReceiptResearchAssignment ??= new();
        set => ssReceiptResearchAssignment = value;
      }

      /// <summary>
      /// A value of SsServiceProvider.
      /// </summary>
      [JsonPropertyName("ssServiceProvider")]
      public ServiceProvider SsServiceProvider
      {
        get => ssServiceProvider ??= new();
        set => ssServiceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private Common ssCommon;
      private CashReceiptSourceType ssCashReceiptSourceType;
      private ReceiptResearchAssignment ssReceiptResearchAssignment;
      private ServiceProvider ssServiceProvider;
    }

    /// <summary>A AssgnGroup group.</summary>
    [Serializable]
    public class AssgnGroup
    {
      /// <summary>
      /// A value of GexportAction.
      /// </summary>
      [JsonPropertyName("gexportAction")]
      public Common GexportAction
      {
        get => gexportAction ??= new();
        set => gexportAction = value;
      }

      /// <summary>
      /// A value of GcashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("gcashReceiptSourceType")]
      public CashReceiptSourceType GcashReceiptSourceType
      {
        get => gcashReceiptSourceType ??= new();
        set => gcashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of GreceiptResearchAssignment.
      /// </summary>
      [JsonPropertyName("greceiptResearchAssignment")]
      public ReceiptResearchAssignment GreceiptResearchAssignment
      {
        get => greceiptResearchAssignment ??= new();
        set => greceiptResearchAssignment = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ServiceProvider Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 196;

      private Common gexportAction;
      private CashReceiptSourceType gcashReceiptSourceType;
      private ReceiptResearchAssignment greceiptResearchAssignment;
      private ServiceProvider detail;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// Gets a value of Ss.
    /// </summary>
    [JsonIgnore]
    public Array<SsGroup> Ss => ss ??= new(SsGroup.Capacity);

    /// <summary>
    /// Gets a value of Ss for json serialization.
    /// </summary>
    [JsonPropertyName("ss")]
    [Computed]
    public IList<SsGroup> Ss_Json
    {
      get => ss;
      set => Ss.Assign(value);
    }

    /// <summary>
    /// A value of HiddenServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenServiceProvider")]
    public ServiceProvider HiddenServiceProvider
    {
      get => hiddenServiceProvider ??= new();
      set => hiddenServiceProvider = value;
    }

    /// <summary>
    /// A value of PromptServiceProvider.
    /// </summary>
    [JsonPropertyName("promptServiceProvider")]
    public Common PromptServiceProvider
    {
      get => promptServiceProvider ??= new();
      set => promptServiceProvider = value;
    }

    /// <summary>
    /// A value of PromptSourceCode.
    /// </summary>
    [JsonPropertyName("promptSourceCode")]
    public Common PromptSourceCode
    {
      get => promptSourceCode ??= new();
      set => promptSourceCode = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptSourceType")]
    public CashReceiptSourceType HiddenCashReceiptSourceType
    {
      get => hiddenCashReceiptSourceType ??= new();
      set => hiddenCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of Input.
    /// </summary>
    [JsonPropertyName("input")]
    public CashReceiptSourceType Input
    {
      get => input ??= new();
      set => input = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ServiceProvider Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// Gets a value of Assgn.
    /// </summary>
    [JsonIgnore]
    public Array<AssgnGroup> Assgn => assgn ??= new(AssgnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Assgn for json serialization.
    /// </summary>
    [JsonPropertyName("assgn")]
    [Computed]
    public IList<AssgnGroup> Assgn_Json
    {
      get => assgn;
      set => Assgn.Assign(value);
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

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Office office;
    private Array<SsGroup> ss;
    private ServiceProvider hiddenServiceProvider;
    private Common promptServiceProvider;
    private Common promptSourceCode;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private CashReceiptSourceType input;
    private ServiceProvider search;
    private Common showHistory;
    private Array<AssgnGroup> assgn;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ClearInd.
    /// </summary>
    [JsonPropertyName("clearInd")]
    public Common ClearInd
    {
      get => clearInd ??= new();
      set => clearInd = value;
    }

    /// <summary>
    /// A value of Unassigned.
    /// </summary>
    [JsonPropertyName("unassigned")]
    public Common Unassigned
    {
      get => unassigned ??= new();
      set => unassigned = value;
    }

    /// <summary>
    /// A value of Assigned.
    /// </summary>
    [JsonPropertyName("assigned")]
    public Common Assigned
    {
      get => assigned ??= new();
      set => assigned = value;
    }

    /// <summary>
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
    }

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
    /// A value of Holding.
    /// </summary>
    [JsonPropertyName("holding")]
    public ReceiptResearchAssignment Holding
    {
      get => holding ??= new();
      set => holding = value;
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
    /// A value of InitializedReceiptResearchAssignment.
    /// </summary>
    [JsonPropertyName("initializedReceiptResearchAssignment")]
    public ReceiptResearchAssignment InitializedReceiptResearchAssignment
    {
      get => initializedReceiptResearchAssignment ??= new();
      set => initializedReceiptResearchAssignment = value;
    }

    /// <summary>
    /// A value of InitializedCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("initializedCashReceiptSourceType")]
    public CashReceiptSourceType InitializedCashReceiptSourceType
    {
      get => initializedCashReceiptSourceType ??= new();
      set => initializedCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ActionCounter.
    /// </summary>
    [JsonPropertyName("actionCounter")]
    public Common ActionCounter
    {
      get => actionCounter ??= new();
      set => actionCounter = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of InitializedDateWorkArea.
    /// </summary>
    [JsonPropertyName("initializedDateWorkArea")]
    public DateWorkArea InitializedDateWorkArea
    {
      get => initializedDateWorkArea ??= new();
      set => initializedDateWorkArea = value;
    }

    /// <summary>
    /// A value of InvalidSelectCode.
    /// </summary>
    [JsonPropertyName("invalidSelectCode")]
    public Common InvalidSelectCode
    {
      get => invalidSelectCode ??= new();
      set => invalidSelectCode = value;
    }

    private Common clearInd;
    private Common unassigned;
    private Common assigned;
    private Common promptCount;
    private DateWorkArea current;
    private ReceiptResearchAssignment holding;
    private Common common;
    private ReceiptResearchAssignment initializedReceiptResearchAssignment;
    private CashReceiptSourceType initializedCashReceiptSourceType;
    private Common actionCounter;
    private DateWorkArea maxDate;
    private DateWorkArea initializedDateWorkArea;
    private Common invalidSelectCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptSourceType")]
    public CashReceiptSourceType ExistingCashReceiptSourceType
    {
      get => existingCashReceiptSourceType ??= new();
      set => existingCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ExistingReceiptResearchAssignment.
    /// </summary>
    [JsonPropertyName("existingReceiptResearchAssignment")]
    public ReceiptResearchAssignment ExistingReceiptResearchAssignment
    {
      get => existingReceiptResearchAssignment ??= new();
      set => existingReceiptResearchAssignment = value;
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

    private CashReceiptSourceType existingCashReceiptSourceType;
    private ReceiptResearchAssignment existingReceiptResearchAssignment;
    private ServiceProvider existingServiceProvider;
  }
#endregion
}
