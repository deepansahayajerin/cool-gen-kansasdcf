// Program: SP_CASL_CASE_ASSIGNMENT_LIST, ID: 372325230, model: 746.
// Short name: SWECASLP
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
/// A program: SP_CASL_CASE_ASSIGNMENT_LIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCaslCaseAssignmentList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CASL_CASE_ASSIGNMENT_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCaslCaseAssignmentList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCaslCaseAssignmentList.
  /// </summary>
  public SpCaslCaseAssignmentList(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Date    Developer         Request #  Description
    // ---------------------------------------------------------------------------------------
    // 02-16-98  J Rookard, MTW  IDCR 413   Initial development
    // 12-12-98  Anita Massey               modifications per screen assess
    // 10-19-99  David Lowry	  PRH77894.  Added a keys only view of cse_person.
    // 10-25-00  SWSRCHF         I00106509  Performance fixes (see modifications
    // below):
    //     A) Changed check for Status NOT = 'C' (closed) to Status = 'O' (open
    // ).
    //        Set attribute value to "O" and used in 'READ EACH' and 'SUMMARIZE'
    // statements
    //     B) Replaced the 'READ EACH Case_Unit' with a 'SUMMARIZE Case_Unit' 
    // statement.
    //     C) Made the 'Case number >= SPACES' filter check, the first statement
    // in the
    //        'READ EACH Case, Case_Assignment' loop.
    //        Previously this statement was executed after obtaining the AR 
    // data.
    //     D) Set attribute value to "AP" and used it in the 'READ EACH 
    // Case_Role, CSE_Person'
    //        statement.
    //     E) Set attribute value to "AR" and used it in the 'READ Case_Role, 
    // CSE_Person'
    //        statement.
    //     F) Commented out call to the CAB used to format Service_Provider 
    // name.
    //        Added code to format Service_Provider name.
    //     G) Changed the 'READ', 'READ EACH' and 'SUMMARIZE' property to 
    // Uncommitted/Browse
    //     H) Commented out call to EAB_PAD_LEFT_WITH_ZEROS, not required for 
    // this group field
    //     I) Set attribute value to the USER_ID and used it in a 'READ' 
    // statement
    //     J) Added local view for 'ADABAS' unavailable, set it's value to 'Y' 
    // and passed
    //        it to CAB SI_READ_CSE_PERSON
    //     K) Removed previously commented out code for easier reading.
    // 11-20-00  SWSRCHF         I00108011  Total Assignment field showing 
    // incorrect value
    // 12-17-00  SWSRGAD         W/O:000232 Added the ability to request the 
    // Case Assignment
    //                                      
    // Report on-line.
    // 01-08-01  SWSRCHF         I00110672  Performance problem
    // 01-24-01  SWSRGMB      WO# 232  Added logic to flow to POPT with job run 
    // parms, for print function.
    // 12-03-10  GVandy  	CQ109 segment B  Add search field for override 
    // indicator.
    // 08-23-13  GVandy  	CQ38147 Replace column reason code with tribunal.
    // ---------------------------------------------------------------------------------------
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

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
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    local.Current.Date = Now().Date;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    MoveOffice(import.Office, export.Office);
    export.ServiceProvider.Assign(import.ServiceProvider);
    MoveOfficeServiceProvider(import.OfficeServiceProvider,
      export.OfficeServiceProvider);
    export.HiddenCalcDone.Flag = import.HiddenCalcDone.Flag;
    export.AsgnCount.Count = import.AsgnCount.Count;
    export.PromptSvpo.SelectChar = import.PromptSvpo.SelectChar;
    export.PromptCdvl.SelectChar = import.PromptCdvl.SelectChar;
    export.PromptLtrb.SelectChar = import.PromptLtrb.SelectChar;
    export.SearchProgram.Cdvalue = import.SearchProgram.Cdvalue;
    export.SearchTribunal.Identifier = import.SearchTribunal.Identifier;
    export.SearchFunction.FuncText1 = import.SearchFunction.FuncText1;
    export.SearchAp.Text20 = import.SearchAp.Text20;
    export.SearchApFi.Text1 = import.SearchApFi.Text1;
    export.SearchAr.Text20 = import.SearchAr.Text20;
    export.SearchArFi.Text1 = import.SearchArFi.Text1;
    export.SearchCaseAssignment.OverrideInd =
      import.SearchCaseAssignment.OverrideInd;
    export.HiddenCode.CodeName = import.HiddenCode.CodeName;
    export.Starting.Number = import.Starting.Number;
    export.Group.Index = -1;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (AsChar(export.HiddenCalcDone.Flag) != 'Y')
    {
      export.HiddenCalcDone.Flag = "N";
    }

    export.HiddenSearchCaseAssignment.OverrideInd =
      import.HiddenSearchCaseAssignment.OverrideInd;
    export.HiddenSearchAp.Text20 = import.HiddenSearchAp.Text20;
    export.HiddenSearchApFi.Text1 = import.HiddenSearchApFi.Text1;
    export.HiddenSearchAr.Text20 = import.HiddenSearchAr.Text20;
    export.HiddenSearchArFi.Text1 = import.HiddenSearchArFi.Text1;
    export.HiddenSearchFunction.FuncText1 =
      import.HiddenSearchFunction.FuncText1;
    export.HiddenSearchProgram.Cdvalue = import.HiddenSearchProgram.Cdvalue;
    export.HiddenSearchTribunal.Identifier =
      import.HiddenSearchTribunal.Identifier;
    export.HiddenStarting.Number = import.HiddenStarting.Number;

    if (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "RETSVPO") || Equal(global.Command, "RETCDVL") || Equal
      (global.Command, "RETLTRB"))
    {
      // The group view will be populated so don't move group view imports to 
      // exports.
    }
    else if (!import.Group.IsEmpty)
    {
      for(import.Group.Index = 0; import.Group.Index < Import
        .GroupGroup.Capacity; ++import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        export.Group.Index = import.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.DetailCommon.SelectChar =
          import.Group.Item.DetailCommon.SelectChar;
        export.Group.Update.DetailCase.Number =
          import.Group.Item.DetailCase.Number;
        export.Group.Update.DetailCaseAssignment.Assign(
          import.Group.Item.DetailCaseAssignment);
        export.Group.Update.DetailTribunal.Identifier =
          import.Group.Item.DetailTribunal.Identifier;
        MoveCsePersonsWorkSet2(import.Group.Item.ArDetail,
          export.Group.Update.ArDetail);
        MoveCsePersonsWorkSet2(import.Group.Item.ApDetail,
          export.Group.Update.ApDetail);
        export.Group.Update.DetailCau.Count = import.Group.Item.DetailCau.Count;
        export.Group.Update.DetailFunc.Text1 =
          import.Group.Item.DetailFunc.Text1;
        export.Group.Update.DetailPrgm.Text3 =
          import.Group.Item.DetailPrgm.Text3;

        // *** Problem report I00106509............H)
        // *** 10/25/00 SWSRCHF
        // *** start
        // *** end
        // *** 10/25/00 SWSRCHF
        // *** Problem report I00106509
      }

      import.Group.CheckIndex();

      for(import.PageKeys.Index = 0; import.PageKeys.Index < Import
        .PageKeysGroup.Capacity; ++import.PageKeys.Index)
      {
        if (!import.PageKeys.CheckSize())
        {
          break;
        }

        // Move page key imports to page key exports
        export.PageKeys.Index = import.PageKeys.Index;
        export.PageKeys.CheckSize();

        export.PageKeys.Update.PageKeyCase.Number =
          import.PageKeys.Item.PageKey.Number;
        export.PageKeys.Update.PageKeyTribunal.Identifier =
          import.PageKeys.Item.PageKeys1.Identifier;
      }

      import.PageKeys.CheckIndex();
      export.Scroll.ScrollingMessage = import.Scroll.ScrollingMessage;
      export.CurrentPage.Count = import.CurrentPage.Count;
    }

    if (Equal(global.Command, "RETSVPO"))
    {
      // Returning on a link dialog flow from SVPO.  Determine if an OSP was 
      // selected on SVPO by evaluating the import hidden osp role code.  If it
      // is greater than spaces an OSP was selected on SVPO and returned to this
      // screen.
      if (!IsEmpty(import.HiddenOfficeServiceProvider.RoleCode))
      {
        export.Office.SystemGeneratedId = import.HiddenOffice.SystemGeneratedId;
        MoveOfficeServiceProvider(import.HiddenOfficeServiceProvider,
          export.OfficeServiceProvider);
        MoveServiceProvider(import.HiddenServiceProvider, export.ServiceProvider);
          
      }

      export.PromptSvpo.SelectChar = "";

      var field = GetField(export.PromptSvpo, "selectChar");

      field.Protected = false;
      field.Focused = true;

      // *** Problem report I00108011
      // *** 11/20/00 SWSRCHF
      export.HiddenCalcDone.Flag = "N";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      // Returning on a link dialog flow from CDVL.  Determine if a Program Code
      // was selected on CDVL by evaluating the import hidden code value
      // cdvalue.  If it is greater than spaces a Program Code was selected on
      // CDVL and returned
      // to this screen.
      if (!IsEmpty(import.HiddenCodeValue.Cdvalue))
      {
        export.SearchProgram.Cdvalue = import.HiddenCodeValue.Cdvalue;
        export.HiddenCodeValue.Cdvalue = import.HiddenCodeValue.Cdvalue;
      }

      export.PromptCdvl.SelectChar = "";

      var field = GetField(export.PromptCdvl, "selectChar");

      field.Protected = false;
      field.Focused = true;

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETLTRB"))
    {
      // Returning on a link dialog flow from LTRB.
      if (import.FromLtrb.Identifier > 0)
      {
        export.SearchTribunal.Identifier = import.FromLtrb.Identifier;
      }

      export.PromptLtrb.SelectChar = "";

      var field = GetField(export.PromptLtrb, "selectChar");

      field.Protected = false;
      field.Focused = true;

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETCASU"))
    {
      // User is returning on a link from COMP or CASU. Views have already been 
      // populated.  Escape.
      for(export.Group.Index = 0; export.Group.Index < Export
        .GroupGroup.Capacity; ++export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Group.Item.DetailCommon.SelectChar))
        {
          case 'S':
            export.Group.Update.DetailCommon.SelectChar = "*";

            break;
          case ' ':
            break;
          default:
            export.Group.Update.DetailCommon.SelectChar = "";

            break;
        }
      }

      export.Group.CheckIndex();

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // User entered this screen from another screen
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "ENTER"))
    {
      if (!IsEmpty(import.Standard.NextTransaction))
      {
        // *** User is going out of this screen to another screen.
        UseScCabNextTranPut();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;
        }

        return;
      }

      ExitState = "ACO_NE0000_INVALID_COMMAND";
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PRINT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "COMP") || Equal(global.Command, "CASU"))
    {
      // Group view select processing.
      local.Count.Count = 0;

      for(export.Group.Index = 0; export.Group.Index < Export
        .GroupGroup.Capacity; ++export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Group.Item.DetailCommon.SelectChar))
        {
          case 'S':
            ++local.Count.Count;
            export.Selected.Number = export.Group.Item.DetailCase.Number;

            break;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            var field = GetField(export.Group.Item.DetailCommon, "selectChar");

            field.Color = "red";
            field.Protected = false;
            field.Focused = true;

            return;
        }
      }

      export.Group.CheckIndex();

      if (local.Count.Count > 1)
      {
        export.Selected.Number = "";
        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        return;
      }

      if (local.Count.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }
    }

    // -- Check filters for change.
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PRINT") || Equal
      (global.Command, "LIST"))
    {
    }
    else
    {
      if (AsChar(import.SearchCaseAssignment.OverrideInd) != AsChar
        (import.HiddenSearchCaseAssignment.OverrideInd))
      {
        var field = GetField(export.SearchCaseAssignment, "overrideInd");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (AsChar(import.SearchFunction.FuncText1) != AsChar
        (import.HiddenSearchFunction.FuncText1))
      {
        var field = GetField(export.SearchFunction, "funcText1");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!Equal(import.SearchProgram.Cdvalue,
        import.HiddenSearchProgram.Cdvalue))
      {
        var field = GetField(export.SearchProgram, "cdvalue");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (export.SearchTribunal.Identifier != export
        .HiddenSearchTribunal.Identifier)
      {
        var field = GetField(export.SearchTribunal, "identifier");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!Equal(import.Starting.Number, import.HiddenStarting.Number))
      {
        var field = GetField(export.Starting, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (AsChar(import.SearchArFi.Text1) != AsChar
        (import.HiddenSearchArFi.Text1))
      {
        var field = GetField(export.SearchArFi, "text1");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!Equal(import.SearchAr.Text20, import.HiddenSearchAr.Text20))
      {
        var field = GetField(export.SearchAr, "text20");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (AsChar(import.SearchApFi.Text1) != AsChar
        (import.HiddenSearchApFi.Text1))
      {
        var field = GetField(export.SearchApFi, "text1");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!Equal(import.SearchAp.Text20, import.HiddenSearchAp.Text20))
      {
        var field = GetField(export.SearchAp, "text20");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // Main case of command.
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        switch(AsChar(export.PromptCdvl.SelectChar))
        {
          case 'S':
            ++local.Prompt.Count;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptCdvl, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptLtrb.SelectChar))
        {
          case 'S':
            ++local.Prompt.Count;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptLtrb, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptSvpo.SelectChar))
        {
          case 'S':
            ++local.Prompt.Count;

            break;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptSvpo, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(local.Prompt.Count)
        {
          case 0:
            var field1 = GetField(export.PromptSvpo, "selectChar");

            field1.Error = true;

            var field2 = GetField(export.PromptLtrb, "selectChar");

            field2.Error = true;

            var field3 = GetField(export.PromptCdvl, "selectChar");

            field3.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            return;
          case 1:
            break;
          default:
            if (AsChar(export.PromptCdvl.SelectChar) == 'S')
            {
              var field = GetField(export.PromptCdvl, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.PromptLtrb.SelectChar) == 'S')
            {
              var field = GetField(export.PromptLtrb, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.PromptSvpo.SelectChar) == 'S')
            {
              var field = GetField(export.PromptSvpo, "selectChar");

              field.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
        }

        if (AsChar(export.PromptCdvl.SelectChar) == 'S')
        {
          export.HiddenCode.CodeName = "CASE LEVEL PROGRAM";
          ExitState = "ECO_LNK_TO_CDVL";

          return;
        }
        else
        {
        }

        if (AsChar(export.PromptLtrb.SelectChar) == 'S')
        {
          export.ToLtrb.StateAbbreviation = "KS";
          ExitState = "ECO_LNK_TO_LTRB";

          return;
        }
        else
        {
        }

        if (AsChar(export.PromptSvpo.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_SVPO";

          return;
        }
        else
        {
        }

        break;
      case "DISPLAY":
        // Display processing at bottom of Procedure step.
        export.HiddenCalcDone.Flag = "N";

        break;
      case "PREV":
        if (export.CurrentPage.Count <= 1)
        {
          ExitState = "ACO_NI0000_TOP_OF_LIST";

          return;
        }

        // -----*****  Decrement page number and group view
        --export.CurrentPage.Count;

        export.PageKeys.Index = export.CurrentPage.Count - 1;
        export.PageKeys.CheckSize();

        global.Command = "DISPLAY";

        break;
      case "NEXT":
        if (export.CurrentPage.Count == Export.PageKeysGroup.Capacity)
        {
          ExitState = "OE0000_LIST_IS_FULL";

          return;
        }

        if (export.CurrentPage.Count == 0)
        {
          // -----*****  First time
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        // -----*****  Increment page number and group view
        ++export.CurrentPage.Count;

        export.PageKeys.Index = export.CurrentPage.Count - 1;
        export.PageKeys.CheckSize();

        // -----*****  Test if next page of data exists
        if (Equal(export.PageKeys.Item.PageKeyCase.Number, local.Null1.Number))
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

          // -----*****  Reset page key
          --export.CurrentPage.Count;

          export.PageKeys.Index = export.CurrentPage.Count - 1;
          export.PageKeys.CheckSize();

          return;
        }

        local.ClearGroupView.Flag = "Y";
        global.Command = "DISPLAY";

        break;
      case "CASU":
        ExitState = "ECO_LNK_TO_CASU";

        return;
      case "COMP":
        ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (AsChar(local.ClearGroupView.Flag) == 'Y')
    {
      // ********** this flag is set in NEXT command processing to
      // * clear the group view in case the next page is not full
      // **********
      for(export.Group.Index = 0; export.Group.Index < Export
        .GroupGroup.Capacity; ++export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        export.Group.Update.DetailCommon.SelectChar = "";
        export.Group.Update.DetailCase.Number = "";
        export.Group.Update.DetailCaseAssignment.CreatedBy = "";
        export.Group.Update.DetailCaseAssignment.EffectiveDate =
          local.Initialized.Date;
        export.Group.Update.DetailCaseAssignment.OverrideInd = "";
        export.Group.Update.DetailCaseAssignment.ReasonCode = "";
        export.Group.Update.ArDetail.FormattedName = "";
        export.Group.Update.ApDetail.FormattedName = "";
        export.Group.Update.DetailCau.Count = 0;
        export.Group.Update.DetailFunc.Text1 = "";
        export.Group.Update.DetailPrgm.Text3 = "";
        export.Group.Update.DetailTribunal.Identifier = 0;
      }

      export.Group.CheckIndex();
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PRINT"))
    {
      if (!IsEmpty(export.SearchArFi.Text1) && IsEmpty(export.SearchAr.Text20))
      {
        var field = GetField(export.SearchArFi, "text1");

        field.Color = "red";
        field.Protected = false;
        field.Focused = true;

        ExitState = "ACO_NE0000_INVALID_ACTION";
      }

      if (!IsEmpty(export.SearchApFi.Text1) && IsEmpty(export.SearchAp.Text20))
      {
        var field = GetField(export.SearchApFi, "text1");

        field.Color = "red";
        field.Protected = false;
        field.Focused = true;

        ExitState = "ACO_NE0000_INVALID_ACTION";
      }

      if (!IsEmpty(export.SearchAp.Text20) && !IsEmpty(export.SearchAr.Text20))
      {
        var field1 = GetField(export.SearchAr, "text20");

        field1.Error = true;

        var field2 = GetField(export.SearchAp, "text20");

        field2.Error = true;

        ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";
      }

      if (IsEmpty(export.OfficeServiceProvider.RoleCode) && Equal
        (export.OfficeServiceProvider.EffectiveDate, local.Initialized.Date) &&
        export.ServiceProvider.SystemGeneratedId == 0 && export
        .Office.SystemGeneratedId == 0)
      {
        // *** Problem report I00106509............I)
        // *** 10/25/00 SWSRCHF
        local.WorkServiceProvider.UserId = global.UserId;

        // This will occur on a dialog flow from the menu to this procedure, or,
        // when a next tran to this procedure is performed and the user then
        // presses PF2 Display, or, when the user has performed a Clear and then
        // presses PF2 Display.  Determine the logged on user and derive the
        // first occurrence of Office Service Provider for this user, ordered by
        // Office.
        if (ReadServiceProvider1())
        {
          export.ServiceProvider.Assign(entities.ExistingServiceProvider);

          // *** Problem report I00106509............F)
          // *** 10/25/00 SWSRCHF
          // *** start
          export.ServiceProvider.LastName =
            TrimEnd(entities.ExistingServiceProvider.LastName) + ", " + TrimEnd
            (entities.ExistingServiceProvider.FirstName) + " " + entities
            .ExistingServiceProvider.MiddleInitial;

          // *** end
          // *** 10/25/00 SWSRCHF
          // *** Problem report I00106509
          local.OspFound.Flag = "N";

          // *** Problem report I00106509............G)
          if (ReadOfficeOfficeServiceProvider())
          {
            MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
              export.OfficeServiceProvider);
            MoveOffice(entities.ExistingOffice, export.Office);
            local.OspFound.Flag = "Y";
          }

          if (AsChar(local.OspFound.Flag) == 'N')
          {
            ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

            var field1 =
              GetField(export.OfficeServiceProvider, "effectiveDate");

            field1.Error = true;

            var field2 = GetField(export.OfficeServiceProvider, "roleCode");

            field2.Error = true;

            return;
          }
        }
        else
        {
          ExitState = "SERVICE_PROVIDER_NF";

          var field = GetField(export.ServiceProvider, "systemGeneratedId");

          field.Error = true;

          return;
        }
      }
      else
      {
        if (export.Office.SystemGeneratedId == 0)
        {
          var field = GetField(export.Office, "systemGeneratedId");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        // *** Problem report I00106509............G)
        if (ReadOffice())
        {
          MoveOffice(entities.ExistingOffice, export.Office);
        }
        else
        {
          ExitState = "OFFICE_NF";

          var field = GetField(export.Office, "systemGeneratedId");

          field.Error = true;

          export.Office.Name = "";

          return;
        }

        if (export.ServiceProvider.SystemGeneratedId == 0)
        {
          var field = GetField(export.ServiceProvider, "systemGeneratedId");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        // *** Problem report I00106509............G)
        if (ReadServiceProvider2())
        {
          export.ServiceProvider.Assign(entities.ExistingServiceProvider);

          // *** Problem report I00106509............F)
          // *** 10/25/00 SWSRCHF
          // *** start
          export.ServiceProvider.LastName =
            TrimEnd(entities.ExistingServiceProvider.LastName) + ", " + TrimEnd
            (entities.ExistingServiceProvider.FirstName) + " " + entities
            .ExistingServiceProvider.MiddleInitial;

          // *** end
          // *** 10/25/00 SWSRCHF
          // *** Problem report I00106509
        }
        else
        {
          ExitState = "SERVICE_PROVIDER_NF";

          var field = GetField(export.ServiceProvider, "systemGeneratedId");

          field.Error = true;

          export.ServiceProvider.LastName = "";

          return;
        }

        if (Equal(export.OfficeServiceProvider.EffectiveDate,
          local.Initialized.Date))
        {
          var field = GetField(export.OfficeServiceProvider, "effectiveDate");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        if (IsEmpty(export.OfficeServiceProvider.RoleCode))
        {
          var field = GetField(export.OfficeServiceProvider, "roleCode");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        // *** Problem report I00106509............G)
        if (ReadOfficeServiceProvider())
        {
          MoveOfficeServiceProvider(entities.ExistingOfficeServiceProvider,
            export.OfficeServiceProvider);
        }
        else
        {
          ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

          var field1 = GetField(export.OfficeServiceProvider, "effectiveDate");

          field1.Error = true;

          var field2 = GetField(export.OfficeServiceProvider, "roleCode");

          field2.Error = true;

          return;
        }
      }

      if (!IsEmpty(export.SearchProgram.Cdvalue))
      {
        export.Code.CodeName = "CASE LEVEL PROGRAM";
        UseCabValidateCodeValue();

        switch(local.CdvlReturnCode.Count)
        {
          case 0:
            break;
          case 1:
            var field1 = GetField(export.SearchProgram, "cdvalue");

            field1.Error = true;

            return;

            ExitState = "CODE_NF";

            break;
          case 2:
            ExitState = "CODE_VALUE_NF";

            var field2 = GetField(export.SearchProgram, "cdvalue");

            field2.Error = true;

            return;
          default:
            break;
        }
      }

      if (export.SearchTribunal.Identifier > 0)
      {
        if (!ReadTribunal1())
        {
          ExitState = "TRIBUNAL_NF";

          var field = GetField(export.SearchTribunal, "identifier");

          field.Error = true;

          return;
        }
      }

      switch(AsChar(export.SearchFunction.FuncText1))
      {
        case 'L':
          break;
        case 'P':
          break;
        case 'O':
          break;
        case 'E':
          break;
        case ' ':
          break;
        default:
          ExitState = "CODE_VALUE_NF";

          var field = GetField(export.SearchFunction, "funcText1");

          field.Error = true;

          return;
      }

      // -- 12/03/2010  GVandy CQ109 segment B  Override search indicator must 
      // be either Y, N, or spaces.
      switch(AsChar(export.SearchCaseAssignment.OverrideInd))
      {
        case 'Y':
          break;
        case 'N':
          break;
        case ' ':
          break;
        default:
          ExitState = "ACO_NI0000_ENTER_Y_OR_N";

          var field = GetField(export.SearchCaseAssignment, "overrideInd");

          field.Error = true;

          return;
      }
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (AsChar(export.HiddenCalcDone.Flag) == 'N')
      {
        // *** October 18, 1999  David Lowry
        // PR77894.  Added this summarize statement to replace a read each which
        // has been disabled.
        export.AsgnCount.Count = 0;

        // *** Problem report I00106509............A)
        // *** 10/25/00 SWSRCHF
        local.WorkCase.Status = "O";

        // *** Problem report I00106509............G)
        if (IsEmpty(export.SearchAp.Text20) && IsEmpty
          (export.SearchAr.Text20) && IsEmpty(export.Starting.Number) && IsEmpty
          (export.SearchProgram.Cdvalue) && export
          .SearchTribunal.Identifier == 0 && IsEmpty
          (export.SearchFunction.FuncText1))
        {
          ReadCaseCaseAssignment1();
        }
        else if (export.SearchTribunal.Identifier > 0)
        {
          foreach(var item in ReadCase())
          {
            foreach(var item1 in ReadTribunal2())
            {
              ++export.AsgnCount.Count;
            }
          }
        }
        else
        {
          ReadCaseCaseAssignment2();
        }

        export.HiddenCalcDone.Flag = "Y";
      }

      // handle paging for first time into prad
      if (export.CurrentPage.Count == 0)
      {
        export.CurrentPage.Count = 1;

        export.PageKeys.Index = 0;
        export.PageKeys.CheckSize();
      }

      // 56434
      if (!IsEmpty(export.Starting.Number))
      {
        local.StartingCase.Text10 = export.Starting.Number;
        UseEabPadLeftWithZeros();
        export.Starting.Number = local.StartingCase.Text10;
      }

      if (!IsEmpty(export.SearchAr.Text20))
      {
        export.SearchAr.Text20 = TrimEnd(export.SearchAr.Text20);
      }

      if (!IsEmpty(export.SearchAp.Text20))
      {
        export.SearchAp.Text20 = TrimEnd(export.SearchAp.Text20);
      }

      export.Group.Index = -1;

      // *** Problem report I00106509............A)
      // *** 10/25/00 SWSRCHF
      local.WorkCase.Status = "O";

      // *** Problem report I00106509............G)
      // -- 12/03/2010 GVandy CQ109 segment B  Add assignment override search 
      // field.
      foreach(var item in ReadCaseCaseAssignment3())
      {
        // *** Problem report I00106509
        // *** 10/25/00 SWSRCHF
        // *** start
        // ************************************************
        // Determine if the Case number is greater than or
        // equal to the user entered starting Case number.
        // ************************************************
        // *** Problem report I00106509............C)
        if (!IsEmpty(export.Starting.Number) && Lt
          (entities.ExistingCase.Number, export.Starting.Number))
        {
          continue;
        }

        ++export.Group.Index;
        export.Group.CheckSize();

        // *** Problem report I00106509............D)
        local.WorkCaseRole.Type1 = "AP";

        // *** end
        // *** 10/25/00 SWSRCHF
        // *** Problem report I00106509
        local.ApCount.Count = 0;
        local.ApMatchMade.Flag = "N";

        // ************************************************
        // Prepare the AP data.
        // ************************************************
        if (export.Group.Index + 1 < Export.GroupGroup.Capacity)
        {
          // *** Problem report I00106509............G)
          foreach(var item1 in ReadCaseRoleCsePerson2())
          {
            ++local.ApCount.Count;

            if (IsEmpty(export.SearchAp.Text20) && local.ApCount.Count > 1)
            {
              break;
            }

            if (!IsEmpty(export.SearchAp.Text20) && local.ApCount.Count > 1 && AsChar
              (local.ApMatchMade.Flag) == 'Y')
            {
              break;
            }

            local.ApCsePersonsWorkSet.Number = entities.KeyOnly.Number;

            // *** Problem report I00110672
            // *** 01/08/01 swsrchf
            // *** start
            if (ReadCsePerson1())
            {
              switch(AsChar(entities.ExistingCsePerson.Type1))
              {
                case 'C':
                  // ***
                  // *** Client
                  // ***
                  // *** Retrieve the AP name from ADABAS
                  UseEabReadAdabasClientForCasl1();

                  if (IsEmpty(local.Returned.Type1))
                  {
                    local.ApCsePersonsWorkSet.FormattedName =
                      TrimEnd(local.ApCsePersonsWorkSet.LastName) + ", " + TrimEnd
                      (local.ApCsePersonsWorkSet.FirstName) + " " + local
                      .ApCsePersonsWorkSet.MiddleInitial;
                  }
                  else
                  {
                    local.ApCsePersonsWorkSet.FormattedName = "NAME NOT FOUND";
                    ExitState = "ACO_NN0000_ALL_OK";
                  }

                  break;
                case 'O':
                  // ***
                  // *** Organization
                  // ***
                  local.ApCsePersonsWorkSet.FormattedName =
                    entities.ExistingCsePerson.OrganizationName ?? Spaces(33);

                  break;
                default:
                  break;
              }
            }
            else
            {
              var field = GetField(export.Group.Item.ApDetail, "formattedName");

              field.Error = true;

              ExitState = "CSE_PERSON_NF";

              return;
            }

            // *** end
            // *** 01/08/01 swsrchf
            // *** Problem report I00110672
            // *** October 18, 1999   David Lowry
            // Added a view of keys only cse person which has been read as part 
            // of the case role read.  The original read has been commented out.
            if (!IsEmpty(export.SearchAp.Text20))
            {
              local.ApCsePersonsWorkSet.LastName =
                TrimEnd(local.ApCsePersonsWorkSet.LastName);

              if (Equal(local.ApCsePersonsWorkSet.LastName,
                export.SearchAp.Text20))
              {
                local.ApMatchMade.Flag = "Y";
              }
              else
              {
                continue;
              }

              if (!IsEmpty(export.SearchApFi.Text1))
              {
                local.ApMatchMade.Flag = "N";
                local.ApCsePersonsWorkSet.FirstName =
                  Substring(local.ApCsePersonsWorkSet.FirstName, 1, 1);

                if (!Lt(local.ApCsePersonsWorkSet.FirstName,
                  export.SearchApFi.Text1))
                {
                  local.ApMatchMade.Flag = "Y";
                }
                else
                {
                  --export.Group.Index;
                  export.Group.CheckSize();

                  goto ReadEach1;
                }
              }
            }
          }

          if (!IsEmpty(export.SearchAp.Text20) && AsChar
            (local.ApMatchMade.Flag) == 'N')
          {
            --export.Group.Index;
            export.Group.CheckSize();

            continue;
          }

          if (local.ApCount.Count == 0)
          {
            local.ApCsePersonsWorkSet.FormattedName = "AP NOT ASSIGNED";
          }
          else if (local.ApCount.Count > 1)
          {
            local.ApCsePersonsWorkSet.FormattedName =
              Substring(local.ApCsePersonsWorkSet.FormattedName,
              CsePersonsWorkSet.FormattedName_MaxLength, 1, 14) + "*";
          }

          // *** Problem report I00106509............E)
          // *** 10/25/00 SWSRCHF
          local.WorkCaseRole.Type1 = "AR";

          // ************************************************
          // Prepare the AR data.
          // ************************************************
          // *** Problem report I00106509............G)
          if (ReadCaseRoleCsePerson1())
          {
            local.ArCsePersonsWorkSet.Number = entities.KeyOnly.Number;

            // *** Problem report I00110672
            // *** 01/08/01 swsrchf
            // *** start
            if (ReadCsePerson2())
            {
              switch(AsChar(entities.ExistingCsePerson.Type1))
              {
                case 'C':
                  // ***
                  // *** Client
                  // ***
                  // *** Retrieve the AR name from ADABAS
                  UseEabReadAdabasClientForCasl2();

                  if (IsEmpty(local.Returned.Type1))
                  {
                    local.ArCsePersonsWorkSet.FormattedName =
                      TrimEnd(local.ArCsePersonsWorkSet.LastName) + ", " + TrimEnd
                      (local.ArCsePersonsWorkSet.FirstName) + " " + local
                      .ArCsePersonsWorkSet.MiddleInitial;
                  }
                  else
                  {
                    local.ArCsePersonsWorkSet.FormattedName = "NAME NOT FOUND";
                    ExitState = "ACO_NN0000_ALL_OK";
                  }

                  break;
                case 'O':
                  // ***
                  // *** Organization
                  // ***
                  local.ArCsePersonsWorkSet.FormattedName =
                    entities.ExistingCsePerson.OrganizationName ?? Spaces(33);

                  break;
                default:
                  break;
              }
            }
            else
            {
              var field = GetField(export.Group.Item.ArDetail, "formattedName");

              field.Error = true;

              ExitState = "CSE_PERSON_NF";

              return;
            }

            // *** end
            // *** 01/08/01 swsrchf
            // *** Problem report I00110672
            // *** October 26, 1999  David Lowry
            // Added this if statement if the person is an organization.  We 
            // need to compare the organization name to the entry on the screen.
            if (AsChar(local.WorkCsePerson.Type1) == 'O')
            {
              local.ArCsePersonsWorkSet.LastName =
                Substring(local.ArCsePersonsWorkSet.FormattedName, 1, 17);
            }

            if (!IsEmpty(export.SearchAr.Text20))
            {
              local.ArCsePersonsWorkSet.LastName =
                TrimEnd(local.ArCsePersonsWorkSet.LastName);

              if (!Equal(local.ArCsePersonsWorkSet.LastName,
                export.SearchAr.Text20))
              {
                --export.Group.Index;
                export.Group.CheckSize();

                continue;
              }
            }

            if (!IsEmpty(export.SearchArFi.Text1))
            {
              local.ArCsePersonsWorkSet.FirstName =
                Substring(local.ArCsePersonsWorkSet.FirstName, 1, 1);

              if (Lt(local.ArCsePersonsWorkSet.FirstName,
                export.SearchArFi.Text1))
              {
                --export.Group.Index;
                export.Group.CheckSize();

                continue;
              }
            }
          }
          else
          {
            ExitState = "AR_DB_ERROR_NF";

            var field = GetField(export.Group.Item.ArDetail, "formattedName");

            field.Error = true;

            return;
          }

          // ************************************************
          // Determine the Case Level Function.
          // ************************************************
          local.CaseFuncWorkSet.FuncText1 = "";
          UseSiCabReturnCaseFunction();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsEmpty(local.CaseFuncWorkSet.FuncText1))
            {
              local.CaseFuncWorkSet.FuncText1 = "L";
            }
          }
          else
          {
            var field = GetField(export.Group.Item.DetailCase, "number");

            field.Error = true;

            return;
          }

          if (!IsEmpty(export.SearchFunction.FuncText1))
          {
            if (AsChar(local.CaseFuncWorkSet.FuncText1) != AsChar
              (export.SearchFunction.FuncText1))
            {
              --export.Group.Index;
              export.Group.CheckSize();

              continue;
            }
          }

          // ************************************************
          // Determine the Case Level Program.
          // ************************************************
          local.Program.Code = "";
          UseSiReadCaseProgramType();

          if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }

          if (!IsEmpty(export.SearchProgram.Cdvalue))
          {
            if (!Equal(local.Program.Code, export.SearchProgram.Cdvalue))
            {
              --export.Group.Index;
              export.Group.CheckSize();

              continue;
            }
          }

          // 56434
          // -- Determine if the case has an association to the search tribunal.
          if (export.SearchTribunal.Identifier > 0)
          {
            if (!ReadLegalAction())
            {
              --export.Group.Index;
              export.Group.CheckSize();

              continue;
            }
          }

          // ************************************************
          // Prepare the Case Unit count data.
          // ************************************************
          local.CauCount.Count = 0;

          // *** Problem report I00106509............B)
          // *** 10/25/00 SWSRCHF
          // *** start
          ReadCaseUnit();

          // *** end
          // *** 10/25/00 SWSRCHF
          // *** Problem report I00106509
        }

        // *** Problem report I00106509............C)
        // *** 10/25/00 SWSRCHF
        // *** start
        // *** end
        // *** 10/25/00 SWSRCHF
        // *** Problem report I00106509
        export.Group.Update.DetailCase.Number = entities.ExistingCase.Number;

        if (export.SearchTribunal.Identifier > 0)
        {
          export.Group.Update.DetailTribunal.Identifier =
            export.SearchTribunal.Identifier;

          if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
          {
            break;
          }

          MoveCaseAssignment(entities.ExistingCaseAssignment,
            export.Group.Update.DetailCaseAssignment);
          export.Group.Update.DetailFunc.Text1 =
            local.CaseFuncWorkSet.FuncText1;
          export.Group.Update.DetailPrgm.Text3 = local.Program.Code;
          export.Group.Update.ArDetail.FormattedName =
            local.ArCsePersonsWorkSet.FormattedName;
          export.Group.Update.ApDetail.FormattedName =
            local.ApCsePersonsWorkSet.FormattedName;
          export.Group.Update.DetailCau.Count = local.CauCount.Count;
        }
        else
        {
          local.TribunalFound.Flag = "";

          // -- The following read each is set to distinct occurrences.
          foreach(var item1 in ReadTribunal3())
          {
            if (IsEmpty(local.TribunalFound.Flag))
            {
              local.TribunalFound.Flag = "Y";
            }
            else
            {
              ++export.Group.Index;
              export.Group.CheckSize();
            }

            export.Group.Update.DetailCase.Number =
              entities.ExistingCase.Number;
            export.Group.Update.DetailTribunal.Identifier =
              entities.Tribunal.Identifier;

            if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
            {
              goto ReadEach2;
            }

            MoveCaseAssignment(entities.ExistingCaseAssignment,
              export.Group.Update.DetailCaseAssignment);
            export.Group.Update.DetailFunc.Text1 =
              local.CaseFuncWorkSet.FuncText1;
            export.Group.Update.DetailPrgm.Text3 = local.Program.Code;
            export.Group.Update.ArDetail.FormattedName =
              local.ArCsePersonsWorkSet.FormattedName;
            export.Group.Update.ApDetail.FormattedName =
              local.ApCsePersonsWorkSet.FormattedName;
            export.Group.Update.DetailCau.Count = local.CauCount.Count;
          }

          if (AsChar(local.TribunalFound.Flag) != 'Y')
          {
            MoveCaseAssignment(entities.ExistingCaseAssignment,
              export.Group.Update.DetailCaseAssignment);
            export.Group.Update.DetailFunc.Text1 =
              local.CaseFuncWorkSet.FuncText1;
            export.Group.Update.DetailPrgm.Text3 = local.Program.Code;
            export.Group.Update.ArDetail.FormattedName =
              local.ArCsePersonsWorkSet.FormattedName;
            export.Group.Update.ApDetail.FormattedName =
              local.ApCsePersonsWorkSet.FormattedName;
            export.Group.Update.DetailCau.Count = local.CauCount.Count;
          }

          if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
          {
            break;
          }
        }

ReadEach1:
        ;
      }

ReadEach2:

      // Load values for next page.
      // If group view was not maxed, the next
      // page key value would be set to 0, which
      // is trapped in PF8 processing.
      if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
      {
        export.Group.Index = Export.GroupGroup.Capacity - 1;
        export.Group.CheckSize();

        ++export.PageKeys.Index;
        export.PageKeys.CheckSize();

        export.PageKeys.Update.PageKeyCase.Number =
          export.Group.Item.DetailCase.Number;
        export.PageKeys.Update.PageKeyTribunal.Identifier =
          export.Group.Item.DetailTribunal.Identifier;
      }

      if (export.CurrentPage.Count == 1 && IsEmpty
        (entities.ExistingCase.Number))
      {
        // Only time this can happen is when no data is found.
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        export.Scroll.ScrollingMessage = "MORE";
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        if (export.CurrentPage.Count == 1 && !
          Equal(export.PageKeys.Item.PageKeyCase.Number, local.Null1.Number))
        {
          export.Scroll.ScrollingMessage = "MORE  +";
        }

        if (export.CurrentPage.Count == 1 && Equal
          (export.PageKeys.Item.PageKeyCase.Number, local.Null1.Number))
        {
          export.Scroll.ScrollingMessage = "MORE";
        }

        if (export.CurrentPage.Count > 1 && !
          Equal(export.PageKeys.Item.PageKeyCase.Number, local.Null1.Number))
        {
          export.Scroll.ScrollingMessage = "MORE  -+";
        }

        if (export.CurrentPage.Count > 1 && Equal
          (export.PageKeys.Item.PageKeyCase.Number, local.Null1.Number))
        {
          export.Scroll.ScrollingMessage = "MORE  -";
        }

        if (export.PageKeys.Index + 1 == Export.PageKeysGroup.Capacity)
        {
          export.Scroll.ScrollingMessage = "MORE  -";
        }
      }

      export.HiddenSearchAp.Text20 = export.SearchAp.Text20;
      export.HiddenSearchApFi.Text1 = export.SearchApFi.Text1;
      export.HiddenSearchAr.Text20 = export.SearchAr.Text20;
      export.HiddenSearchArFi.Text1 = export.SearchArFi.Text1;
      export.HiddenStarting.Number = export.Starting.Number;
      export.HiddenSearchTribunal.Identifier = export.SearchTribunal.Identifier;
      export.HiddenSearchProgram.Cdvalue = export.SearchProgram.Cdvalue;
      export.HiddenSearchFunction.FuncText1 = export.SearchFunction.FuncText1;
      export.HiddenSearchCaseAssignment.OverrideInd =
        export.SearchCaseAssignment.OverrideInd;
    }
    else if (Equal(global.Command, "PRINT"))
    {
      // January 24, 2001, M Brown, WO# 232.
      // : Filters have been validated above.
      //    Now prepare the parm data based on the filters.
      if (export.Group.IsEmpty)
      {
        ExitState = "FN0000_MUST_PERFORM_DISPLAY_1ST";

        return;
      }

      // **** SUBSTR values into JOB_RUN PARAMTER_INFORMATION
      local.TextOspEffDate.Text15 =
        NumberToString(DateToInt(export.OfficeServiceProvider.EffectiveDate), 15);
        
      local.TextOspEffDate.Text10 =
        Substring(local.TextOspEffDate.Text15, WorkArea.Text15_MaxLength, 8, 4) +
        "-" + Substring
        (local.TextOspEffDate.Text15, WorkArea.Text15_MaxLength, 12, 2) + "-"
        + Substring
        (local.TextOspEffDate.Text15, WorkArea.Text15_MaxLength, 14, 2);
      export.PassToPoptJob.TranId = global.TranCode;
      local.ApWorkArea.Text17 = export.SearchAp.Text20;
      local.ArWorkArea.Text17 = export.SearchAr.Text20;
      local.TextServiceProviderId.Text5 =
        NumberToString(entities.ExistingServiceProvider.SystemGeneratedId, 11, 5);
        
      export.PassToPoptJobRun.ParmInfo = local.TextServiceProviderId.Text5 + " " +
        NumberToString(export.Office.SystemGeneratedId, 12, 4) + " " + export
        .OfficeServiceProvider.RoleCode + " " + local.TextOspEffDate.Text10 + " " +
        local.ApWorkArea.Text17 + " " + export.SearchApFi.Text1 + " " + local
        .ArWorkArea.Text17 + " " + export.SearchArFi.Text1 + " " + export
        .Starting.Number + " " + TrimEnd("") + Substring
        (export.SearchProgram.Cdvalue, CodeValue.Cdvalue_MaxLength, 1, 3) + " " +
        export.SearchFunction.FuncText1;

      // -- 12/03/2010 GVandy CQ109 segment B  Pass export search assignment 
      // override indicator to POPT.
      export.PassToPoptJobRun.ParmInfo =
        Substring(export.PassToPoptJobRun.ParmInfo, 1, 81) + " " + export
        .SearchCaseAssignment.OverrideInd;
      export.PassToPoptJobRun.ParmInfo =
        Substring(export.PassToPoptJobRun.ParmInfo, 1, 83) + " " + NumberToString
        (export.SearchTribunal.Identifier, 12, 4);
      ExitState = "CO_LINK_TO_POPT";
    }
  }

  private static void MoveCaseAssignment(CaseAssignment source,
    CaseAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
    target.EffectiveDate = source.EffectiveDate;
    target.CreatedBy = source.CreatedBy;
  }

  private static void MoveCaseFuncWorkSet(CaseFuncWorkSet source,
    CaseFuncWorkSet target)
  {
    target.FuncText1 = source.FuncText1;
    target.FuncText3 = source.FuncText3;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = export.Code.CodeName;
    useImport.CodeValue.Cdvalue = export.SearchProgram.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.CdvlReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.StartingCase.Text10;
    useExport.TextWorkArea.Text10 = local.StartingCase.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.StartingCase.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabReadAdabasClientForCasl1()
  {
    var useImport = new EabReadAdabasClientForCasl.Import();
    var useExport = new EabReadAdabasClientForCasl.Export();

    useImport.CsePersonsWorkSet.Number = local.ApCsePersonsWorkSet.Number;
    useImport.DateWorkArea.Date = local.Current.Date;
    useExport.AbendData.Assign(local.Returned);
    MoveCsePersonsWorkSet1(local.ApCsePersonsWorkSet,
      useExport.CsePersonsWorkSet);

    Call(EabReadAdabasClientForCasl.Execute, useImport, useExport);

    local.Returned.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet,
      local.ApCsePersonsWorkSet);
  }

  private void UseEabReadAdabasClientForCasl2()
  {
    var useImport = new EabReadAdabasClientForCasl.Import();
    var useExport = new EabReadAdabasClientForCasl.Export();

    useImport.CsePersonsWorkSet.Number = local.ArCsePersonsWorkSet.Number;
    useImport.DateWorkArea.Date = local.Current.Date;
    useExport.AbendData.Assign(local.Returned);
    MoveCsePersonsWorkSet1(local.ArCsePersonsWorkSet,
      useExport.CsePersonsWorkSet);

    Call(EabReadAdabasClientForCasl.Execute, useImport, useExport);

    local.Returned.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet,
      local.ArCsePersonsWorkSet);
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

    useImport.NextTranInfo.Assign(import.HiddenNextTranInfo);
    useImport.Standard.NextTransaction = import.Standard.NextTransaction;

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

  private void UseSiCabReturnCaseFunction()
  {
    var useImport = new SiCabReturnCaseFunction.Import();
    var useExport = new SiCabReturnCaseFunction.Export();

    useImport.Case1.Number = entities.ExistingCase.Number;

    Call(SiCabReturnCaseFunction.Execute, useImport, useExport);

    MoveCaseFuncWorkSet(useExport.CaseFuncWorkSet, local.CaseFuncWorkSet);
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = entities.ExistingCase.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private IEnumerable<bool> ReadCase()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingCase.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableString(command, "status", local.WorkCase.Status ?? "");
        db.SetDate(
          command, "ospDate",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "overrideInd", export.SearchCaseAssignment.OverrideInd);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.Populated = true;

        return true;
      });
  }

  private bool ReadCaseCaseAssignment1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);

    return Read("ReadCaseCaseAssignment1",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(command, "status", local.WorkCase.Status ?? "");
        db.SetString(
          command, "overrideInd", export.SearchCaseAssignment.OverrideInd);
      },
      (db, reader) =>
      {
        export.AsgnCount.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCaseCaseAssignment2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);

    return Read("ReadCaseCaseAssignment2",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(command, "status", local.WorkCase.Status ?? "");
      },
      (db, reader) =>
      {
        export.AsgnCount.Count = db.GetInt32(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCaseCaseAssignment3()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);
    entities.ExistingCase.Populated = false;
    entities.ExistingCaseAssignment.Populated = false;

    return ReadEach("ReadCaseCaseAssignment3",
      (db, command) =>
      {
        db.SetString(command, "numb", export.PageKeys.Item.PageKeyCase.Number);
        db.SetDate(
          command, "ospDate",
          entities.ExistingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode", entities.ExistingOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.ExistingOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.ExistingOfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(command, "status", local.WorkCase.Status ?? "");
        db.SetString(
          command, "overrideInd", export.SearchCaseAssignment.OverrideInd);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCaseAssignment.CasNo = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCaseAssignment.ReasonCode = db.GetString(reader, 2);
        entities.ExistingCaseAssignment.OverrideInd = db.GetString(reader, 3);
        entities.ExistingCaseAssignment.EffectiveDate = db.GetDate(reader, 4);
        entities.ExistingCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCaseAssignment.CreatedBy = db.GetString(reader, 6);
        entities.ExistingCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ExistingCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.ExistingCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.ExistingCaseAssignment.SpdId = db.GetInt32(reader, 10);
        entities.ExistingCaseAssignment.OffId = db.GetInt32(reader, 11);
        entities.ExistingCaseAssignment.OspCode = db.GetString(reader, 12);
        entities.ExistingCaseAssignment.OspDate = db.GetDate(reader, 13);
        entities.ExistingCase.Populated = true;
        entities.ExistingCaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.KeyOnly.Populated = false;
    entities.ExistingCaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(command, "type", local.WorkCaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.KeyOnly.Number = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.KeyOnly.Populated = true;
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson2()
  {
    entities.KeyOnly.Populated = false;
    entities.ExistingCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(command, "type", local.WorkCaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.KeyOnly.Number = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.KeyOnly.Populated = true;
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);

        return true;
      });
  }

  private bool ReadCaseUnit()
  {
    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.ExistingCase.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        local.CauCount.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.ApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableInt32(command, "trbId", export.SearchTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.Name = db.GetString(reader, 1);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 2);
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadOfficeOfficeServiceProvider()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadOfficeOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOffice.Name = db.GetString(reader, 1);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 2);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 4);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingServiceProvider.SystemGeneratedId);
        db.SetInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
        db.SetDate(
          command, "effectiveDate",
          export.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", export.OfficeServiceProvider.RoleCode);
          
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetString(command, "userId", local.WorkServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", export.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadTribunal1()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", export.SearchTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.Tribunal.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTribunal2()
  {
    entities.Tribunal.Populated = false;

    return ReadEach("ReadTribunal2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", export.SearchTribunal.Identifier);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.Tribunal.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTribunal3()
  {
    entities.Tribunal.Populated = false;

    return ReadEach("ReadTribunal3",
      (db, command) =>
      {
        db.SetString(command, "number1", entities.ExistingCase.Number);
        db.
          SetString(command, "number2", export.PageKeys.Item.PageKeyCase.Number);
          
        db.SetInt32(
          command, "identifier",
          export.PageKeys.Item.PageKeyTribunal.Identifier);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.Tribunal.Populated = true;

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
    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of PageKey.
      /// </summary>
      [JsonPropertyName("pageKey")]
      public Case1 PageKey
      {
        get => pageKey ??= new();
        set => pageKey = value;
      }

      /// <summary>
      /// A value of PageKeys1.
      /// </summary>
      [JsonPropertyName("pageKeys1")]
      public Tribunal PageKeys1
      {
        get => pageKeys1 ??= new();
        set => pageKeys1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Case1 pageKey;
      private Tribunal pageKeys1;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>
      /// A value of ApDetail.
      /// </summary>
      [JsonPropertyName("apDetail")]
      public CsePersonsWorkSet ApDetail
      {
        get => apDetail ??= new();
        set => apDetail = value;
      }

      /// <summary>
      /// A value of ArDetail.
      /// </summary>
      [JsonPropertyName("arDetail")]
      public CsePersonsWorkSet ArDetail
      {
        get => arDetail ??= new();
        set => arDetail = value;
      }

      /// <summary>
      /// A value of DetailFunc.
      /// </summary>
      [JsonPropertyName("detailFunc")]
      public TextWorkArea DetailFunc
      {
        get => detailFunc ??= new();
        set => detailFunc = value;
      }

      /// <summary>
      /// A value of DetailPrgm.
      /// </summary>
      [JsonPropertyName("detailPrgm")]
      public WorkArea DetailPrgm
      {
        get => detailPrgm ??= new();
        set => detailPrgm = value;
      }

      /// <summary>
      /// A value of DetailCau.
      /// </summary>
      [JsonPropertyName("detailCau")]
      public Common DetailCau
      {
        get => detailCau ??= new();
        set => detailCau = value;
      }

      /// <summary>
      /// A value of DetailCaseAssignment.
      /// </summary>
      [JsonPropertyName("detailCaseAssignment")]
      public CaseAssignment DetailCaseAssignment
      {
        get => detailCaseAssignment ??= new();
        set => detailCaseAssignment = value;
      }

      /// <summary>
      /// A value of DetailTribunal.
      /// </summary>
      [JsonPropertyName("detailTribunal")]
      public Tribunal DetailTribunal
      {
        get => detailTribunal ??= new();
        set => detailTribunal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private Common detailCommon;
      private Case1 detailCase;
      private CsePersonsWorkSet apDetail;
      private CsePersonsWorkSet arDetail;
      private TextWorkArea detailFunc;
      private WorkArea detailPrgm;
      private Common detailCau;
      private CaseAssignment detailCaseAssignment;
      private Tribunal detailTribunal;
    }

    /// <summary>
    /// A value of SearchApFi.
    /// </summary>
    [JsonPropertyName("searchApFi")]
    public WorkArea SearchApFi
    {
      get => searchApFi ??= new();
      set => searchApFi = value;
    }

    /// <summary>
    /// A value of SearchAp.
    /// </summary>
    [JsonPropertyName("searchAp")]
    public WorkArea SearchAp
    {
      get => searchAp ??= new();
      set => searchAp = value;
    }

    /// <summary>
    /// A value of SearchArFi.
    /// </summary>
    [JsonPropertyName("searchArFi")]
    public WorkArea SearchArFi
    {
      get => searchArFi ??= new();
      set => searchArFi = value;
    }

    /// <summary>
    /// A value of SearchAr.
    /// </summary>
    [JsonPropertyName("searchAr")]
    public WorkArea SearchAr
    {
      get => searchAr ??= new();
      set => searchAr = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Case1 Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of HiddenOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenOfficeServiceProvider")]
    public OfficeServiceProvider HiddenOfficeServiceProvider
    {
      get => hiddenOfficeServiceProvider ??= new();
      set => hiddenOfficeServiceProvider = value;
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
    /// A value of HiddenOffice.
    /// </summary>
    [JsonPropertyName("hiddenOffice")]
    public Office HiddenOffice
    {
      get => hiddenOffice ??= new();
      set => hiddenOffice = value;
    }

    /// <summary>
    /// A value of SearchFunction.
    /// </summary>
    [JsonPropertyName("searchFunction")]
    public CaseFuncWorkSet SearchFunction
    {
      get => searchFunction ??= new();
      set => searchFunction = value;
    }

    /// <summary>
    /// A value of HiddenCode.
    /// </summary>
    [JsonPropertyName("hiddenCode")]
    public Code HiddenCode
    {
      get => hiddenCode ??= new();
      set => hiddenCode = value;
    }

    /// <summary>
    /// A value of HiddenCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenCodeValue")]
    public CodeValue HiddenCodeValue
    {
      get => hiddenCodeValue ??= new();
      set => hiddenCodeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of SearchProgram.
    /// </summary>
    [JsonPropertyName("searchProgram")]
    public CodeValue SearchProgram
    {
      get => searchProgram ??= new();
      set => searchProgram = value;
    }

    /// <summary>
    /// A value of PromptCdvl.
    /// </summary>
    [JsonPropertyName("promptCdvl")]
    public Common PromptCdvl
    {
      get => promptCdvl ??= new();
      set => promptCdvl = value;
    }

    /// <summary>
    /// A value of PromptSvpo.
    /// </summary>
    [JsonPropertyName("promptSvpo")]
    public Common PromptSvpo
    {
      get => promptSvpo ??= new();
      set => promptSvpo = value;
    }

    /// <summary>
    /// A value of HiddenCalcDone.
    /// </summary>
    [JsonPropertyName("hiddenCalcDone")]
    public Common HiddenCalcDone
    {
      get => hiddenCalcDone ??= new();
      set => hiddenCalcDone = value;
    }

    /// <summary>
    /// A value of AsgnCount.
    /// </summary>
    [JsonPropertyName("asgnCount")]
    public Common AsgnCount
    {
      get => asgnCount ??= new();
      set => asgnCount = value;
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
    /// A value of CurrentPage.
    /// </summary>
    [JsonPropertyName("currentPage")]
    public Common CurrentPage
    {
      get => currentPage ??= new();
      set => currentPage = value;
    }

    /// <summary>
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
    }

    /// <summary>
    /// A value of Scroll.
    /// </summary>
    [JsonPropertyName("scroll")]
    public Standard Scroll
    {
      get => scroll ??= new();
      set => scroll = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of SearchCaseAssignment.
    /// </summary>
    [JsonPropertyName("searchCaseAssignment")]
    public CaseAssignment SearchCaseAssignment
    {
      get => searchCaseAssignment ??= new();
      set => searchCaseAssignment = value;
    }

    /// <summary>
    /// A value of HiddenSearchAp.
    /// </summary>
    [JsonPropertyName("hiddenSearchAp")]
    public WorkArea HiddenSearchAp
    {
      get => hiddenSearchAp ??= new();
      set => hiddenSearchAp = value;
    }

    /// <summary>
    /// A value of HiddenSearchArFi.
    /// </summary>
    [JsonPropertyName("hiddenSearchArFi")]
    public WorkArea HiddenSearchArFi
    {
      get => hiddenSearchArFi ??= new();
      set => hiddenSearchArFi = value;
    }

    /// <summary>
    /// A value of HiddenSearchAr.
    /// </summary>
    [JsonPropertyName("hiddenSearchAr")]
    public WorkArea HiddenSearchAr
    {
      get => hiddenSearchAr ??= new();
      set => hiddenSearchAr = value;
    }

    /// <summary>
    /// A value of HiddenSearchApFi.
    /// </summary>
    [JsonPropertyName("hiddenSearchApFi")]
    public WorkArea HiddenSearchApFi
    {
      get => hiddenSearchApFi ??= new();
      set => hiddenSearchApFi = value;
    }

    /// <summary>
    /// A value of HiddenStarting.
    /// </summary>
    [JsonPropertyName("hiddenStarting")]
    public Case1 HiddenStarting
    {
      get => hiddenStarting ??= new();
      set => hiddenStarting = value;
    }

    /// <summary>
    /// A value of HiddenSearchProgram.
    /// </summary>
    [JsonPropertyName("hiddenSearchProgram")]
    public CodeValue HiddenSearchProgram
    {
      get => hiddenSearchProgram ??= new();
      set => hiddenSearchProgram = value;
    }

    /// <summary>
    /// A value of HiddenSearchFunction.
    /// </summary>
    [JsonPropertyName("hiddenSearchFunction")]
    public CaseFuncWorkSet HiddenSearchFunction
    {
      get => hiddenSearchFunction ??= new();
      set => hiddenSearchFunction = value;
    }

    /// <summary>
    /// A value of HiddenSearchCaseAssignment.
    /// </summary>
    [JsonPropertyName("hiddenSearchCaseAssignment")]
    public CaseAssignment HiddenSearchCaseAssignment
    {
      get => hiddenSearchCaseAssignment ??= new();
      set => hiddenSearchCaseAssignment = value;
    }

    /// <summary>
    /// A value of PromptLtrb.
    /// </summary>
    [JsonPropertyName("promptLtrb")]
    public Common PromptLtrb
    {
      get => promptLtrb ??= new();
      set => promptLtrb = value;
    }

    /// <summary>
    /// A value of HiddenSearchTribunal.
    /// </summary>
    [JsonPropertyName("hiddenSearchTribunal")]
    public Tribunal HiddenSearchTribunal
    {
      get => hiddenSearchTribunal ??= new();
      set => hiddenSearchTribunal = value;
    }

    /// <summary>
    /// A value of SearchTribunal.
    /// </summary>
    [JsonPropertyName("searchTribunal")]
    public Tribunal SearchTribunal
    {
      get => searchTribunal ??= new();
      set => searchTribunal = value;
    }

    /// <summary>
    /// A value of FromLtrb.
    /// </summary>
    [JsonPropertyName("fromLtrb")]
    public Tribunal FromLtrb
    {
      get => fromLtrb ??= new();
      set => fromLtrb = value;
    }

    private WorkArea searchApFi;
    private WorkArea searchAp;
    private WorkArea searchArFi;
    private WorkArea searchAr;
    private Case1 starting;
    private OfficeServiceProvider hiddenOfficeServiceProvider;
    private ServiceProvider hiddenServiceProvider;
    private Office hiddenOffice;
    private CaseFuncWorkSet searchFunction;
    private Code hiddenCode;
    private CodeValue hiddenCodeValue;
    private Code code;
    private CodeValue searchProgram;
    private Common promptCdvl;
    private Common promptSvpo;
    private Common hiddenCalcDone;
    private Common asgnCount;
    private NextTranInfo hiddenNextTranInfo;
    private Common currentPage;
    private Array<PageKeysGroup> pageKeys;
    private Standard scroll;
    private Array<GroupGroup> group;
    private Standard standard;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private CaseAssignment searchCaseAssignment;
    private WorkArea hiddenSearchAp;
    private WorkArea hiddenSearchArFi;
    private WorkArea hiddenSearchAr;
    private WorkArea hiddenSearchApFi;
    private Case1 hiddenStarting;
    private CodeValue hiddenSearchProgram;
    private CaseFuncWorkSet hiddenSearchFunction;
    private CaseAssignment hiddenSearchCaseAssignment;
    private Common promptLtrb;
    private Tribunal hiddenSearchTribunal;
    private Tribunal searchTribunal;
    private Tribunal fromLtrb;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of PageKeyCase.
      /// </summary>
      [JsonPropertyName("pageKeyCase")]
      public Case1 PageKeyCase
      {
        get => pageKeyCase ??= new();
        set => pageKeyCase = value;
      }

      /// <summary>
      /// A value of PageKeyTribunal.
      /// </summary>
      [JsonPropertyName("pageKeyTribunal")]
      public Tribunal PageKeyTribunal
      {
        get => pageKeyTribunal ??= new();
        set => pageKeyTribunal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Case1 pageKeyCase;
      private Tribunal pageKeyTribunal;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of ApDetail.
      /// </summary>
      [JsonPropertyName("apDetail")]
      public CsePersonsWorkSet ApDetail
      {
        get => apDetail ??= new();
        set => apDetail = value;
      }

      /// <summary>
      /// A value of ArDetail.
      /// </summary>
      [JsonPropertyName("arDetail")]
      public CsePersonsWorkSet ArDetail
      {
        get => arDetail ??= new();
        set => arDetail = value;
      }

      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>
      /// A value of DetailFunc.
      /// </summary>
      [JsonPropertyName("detailFunc")]
      public TextWorkArea DetailFunc
      {
        get => detailFunc ??= new();
        set => detailFunc = value;
      }

      /// <summary>
      /// A value of DetailPrgm.
      /// </summary>
      [JsonPropertyName("detailPrgm")]
      public WorkArea DetailPrgm
      {
        get => detailPrgm ??= new();
        set => detailPrgm = value;
      }

      /// <summary>
      /// A value of DetailCau.
      /// </summary>
      [JsonPropertyName("detailCau")]
      public Common DetailCau
      {
        get => detailCau ??= new();
        set => detailCau = value;
      }

      /// <summary>
      /// A value of DetailCaseAssignment.
      /// </summary>
      [JsonPropertyName("detailCaseAssignment")]
      public CaseAssignment DetailCaseAssignment
      {
        get => detailCaseAssignment ??= new();
        set => detailCaseAssignment = value;
      }

      /// <summary>
      /// A value of DetailTribunal.
      /// </summary>
      [JsonPropertyName("detailTribunal")]
      public Tribunal DetailTribunal
      {
        get => detailTribunal ??= new();
        set => detailTribunal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private Common detailCommon;
      private CsePersonsWorkSet apDetail;
      private CsePersonsWorkSet arDetail;
      private Case1 detailCase;
      private TextWorkArea detailFunc;
      private WorkArea detailPrgm;
      private Common detailCau;
      private CaseAssignment detailCaseAssignment;
      private Tribunal detailTribunal;
    }

    /// <summary>
    /// A value of PromptLtrb.
    /// </summary>
    [JsonPropertyName("promptLtrb")]
    public Common PromptLtrb
    {
      get => promptLtrb ??= new();
      set => promptLtrb = value;
    }

    /// <summary>
    /// A value of HiddenSearchTribunal.
    /// </summary>
    [JsonPropertyName("hiddenSearchTribunal")]
    public Tribunal HiddenSearchTribunal
    {
      get => hiddenSearchTribunal ??= new();
      set => hiddenSearchTribunal = value;
    }

    /// <summary>
    /// A value of SearchTribunal.
    /// </summary>
    [JsonPropertyName("searchTribunal")]
    public Tribunal SearchTribunal
    {
      get => searchTribunal ??= new();
      set => searchTribunal = value;
    }

    /// <summary>
    /// A value of SearchApFi.
    /// </summary>
    [JsonPropertyName("searchApFi")]
    public WorkArea SearchApFi
    {
      get => searchApFi ??= new();
      set => searchApFi = value;
    }

    /// <summary>
    /// A value of SearchAp.
    /// </summary>
    [JsonPropertyName("searchAp")]
    public WorkArea SearchAp
    {
      get => searchAp ??= new();
      set => searchAp = value;
    }

    /// <summary>
    /// A value of SearchArFi.
    /// </summary>
    [JsonPropertyName("searchArFi")]
    public WorkArea SearchArFi
    {
      get => searchArFi ??= new();
      set => searchArFi = value;
    }

    /// <summary>
    /// A value of SearchAr.
    /// </summary>
    [JsonPropertyName("searchAr")]
    public WorkArea SearchAr
    {
      get => searchAr ??= new();
      set => searchAr = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Case1 Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of SearchFunction.
    /// </summary>
    [JsonPropertyName("searchFunction")]
    public CaseFuncWorkSet SearchFunction
    {
      get => searchFunction ??= new();
      set => searchFunction = value;
    }

    /// <summary>
    /// A value of HiddenCode.
    /// </summary>
    [JsonPropertyName("hiddenCode")]
    public Code HiddenCode
    {
      get => hiddenCode ??= new();
      set => hiddenCode = value;
    }

    /// <summary>
    /// A value of HiddenCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenCodeValue")]
    public CodeValue HiddenCodeValue
    {
      get => hiddenCodeValue ??= new();
      set => hiddenCodeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of SearchProgram.
    /// </summary>
    [JsonPropertyName("searchProgram")]
    public CodeValue SearchProgram
    {
      get => searchProgram ??= new();
      set => searchProgram = value;
    }

    /// <summary>
    /// A value of PromptCdvl.
    /// </summary>
    [JsonPropertyName("promptCdvl")]
    public Common PromptCdvl
    {
      get => promptCdvl ??= new();
      set => promptCdvl = value;
    }

    /// <summary>
    /// A value of PromptSvpo.
    /// </summary>
    [JsonPropertyName("promptSvpo")]
    public Common PromptSvpo
    {
      get => promptSvpo ??= new();
      set => promptSvpo = value;
    }

    /// <summary>
    /// A value of HiddenCalcDone.
    /// </summary>
    [JsonPropertyName("hiddenCalcDone")]
    public Common HiddenCalcDone
    {
      get => hiddenCalcDone ??= new();
      set => hiddenCalcDone = value;
    }

    /// <summary>
    /// A value of AsgnCount.
    /// </summary>
    [JsonPropertyName("asgnCount")]
    public Common AsgnCount
    {
      get => asgnCount ??= new();
      set => asgnCount = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Case1 Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of CurrentPage.
    /// </summary>
    [JsonPropertyName("currentPage")]
    public Common CurrentPage
    {
      get => currentPage ??= new();
      set => currentPage = value;
    }

    /// <summary>
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
    }

    /// <summary>
    /// A value of Scroll.
    /// </summary>
    [JsonPropertyName("scroll")]
    public Standard Scroll
    {
      get => scroll ??= new();
      set => scroll = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of PassToPoptJob.
    /// </summary>
    [JsonPropertyName("passToPoptJob")]
    public Job PassToPoptJob
    {
      get => passToPoptJob ??= new();
      set => passToPoptJob = value;
    }

    /// <summary>
    /// A value of PassToPoptJobRun.
    /// </summary>
    [JsonPropertyName("passToPoptJobRun")]
    public JobRun PassToPoptJobRun
    {
      get => passToPoptJobRun ??= new();
      set => passToPoptJobRun = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchCaseAssignment.
    /// </summary>
    [JsonPropertyName("searchCaseAssignment")]
    public CaseAssignment SearchCaseAssignment
    {
      get => searchCaseAssignment ??= new();
      set => searchCaseAssignment = value;
    }

    /// <summary>
    /// A value of HiddenSearchAp.
    /// </summary>
    [JsonPropertyName("hiddenSearchAp")]
    public WorkArea HiddenSearchAp
    {
      get => hiddenSearchAp ??= new();
      set => hiddenSearchAp = value;
    }

    /// <summary>
    /// A value of HiddenSearchArFi.
    /// </summary>
    [JsonPropertyName("hiddenSearchArFi")]
    public WorkArea HiddenSearchArFi
    {
      get => hiddenSearchArFi ??= new();
      set => hiddenSearchArFi = value;
    }

    /// <summary>
    /// A value of HiddenSearchAr.
    /// </summary>
    [JsonPropertyName("hiddenSearchAr")]
    public WorkArea HiddenSearchAr
    {
      get => hiddenSearchAr ??= new();
      set => hiddenSearchAr = value;
    }

    /// <summary>
    /// A value of HiddenSearchApFi.
    /// </summary>
    [JsonPropertyName("hiddenSearchApFi")]
    public WorkArea HiddenSearchApFi
    {
      get => hiddenSearchApFi ??= new();
      set => hiddenSearchApFi = value;
    }

    /// <summary>
    /// A value of HiddenStarting.
    /// </summary>
    [JsonPropertyName("hiddenStarting")]
    public Case1 HiddenStarting
    {
      get => hiddenStarting ??= new();
      set => hiddenStarting = value;
    }

    /// <summary>
    /// A value of HiddenSearchProgram.
    /// </summary>
    [JsonPropertyName("hiddenSearchProgram")]
    public CodeValue HiddenSearchProgram
    {
      get => hiddenSearchProgram ??= new();
      set => hiddenSearchProgram = value;
    }

    /// <summary>
    /// A value of HiddenSearchFunction.
    /// </summary>
    [JsonPropertyName("hiddenSearchFunction")]
    public CaseFuncWorkSet HiddenSearchFunction
    {
      get => hiddenSearchFunction ??= new();
      set => hiddenSearchFunction = value;
    }

    /// <summary>
    /// A value of HiddenSearchCaseAssignment.
    /// </summary>
    [JsonPropertyName("hiddenSearchCaseAssignment")]
    public CaseAssignment HiddenSearchCaseAssignment
    {
      get => hiddenSearchCaseAssignment ??= new();
      set => hiddenSearchCaseAssignment = value;
    }

    /// <summary>
    /// A value of ToLtrb.
    /// </summary>
    [JsonPropertyName("toLtrb")]
    public Fips ToLtrb
    {
      get => toLtrb ??= new();
      set => toLtrb = value;
    }

    private Common promptLtrb;
    private Tribunal hiddenSearchTribunal;
    private Tribunal searchTribunal;
    private WorkArea searchApFi;
    private WorkArea searchAp;
    private WorkArea searchArFi;
    private WorkArea searchAr;
    private Case1 starting;
    private CaseFuncWorkSet searchFunction;
    private Code hiddenCode;
    private CodeValue hiddenCodeValue;
    private Code code;
    private CodeValue searchProgram;
    private Common promptCdvl;
    private Common promptSvpo;
    private Common hiddenCalcDone;
    private Common asgnCount;
    private NextTranInfo hiddenNextTranInfo;
    private Case1 selected;
    private Common currentPage;
    private Array<PageKeysGroup> pageKeys;
    private Standard scroll;
    private Array<GroupGroup> group;
    private Standard standard;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private Job passToPoptJob;
    private JobRun passToPoptJobRun;
    private DateWorkArea searchFrom;
    private CaseAssignment searchCaseAssignment;
    private WorkArea hiddenSearchAp;
    private WorkArea hiddenSearchArFi;
    private WorkArea hiddenSearchAr;
    private WorkArea hiddenSearchApFi;
    private Case1 hiddenStarting;
    private CodeValue hiddenSearchProgram;
    private CaseFuncWorkSet hiddenSearchFunction;
    private CaseAssignment hiddenSearchCaseAssignment;
    private Fips toLtrb;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TribunalFound.
    /// </summary>
    [JsonPropertyName("tribunalFound")]
    public Common TribunalFound
    {
      get => tribunalFound ??= new();
      set => tribunalFound = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of System.
    /// </summary>
    [JsonPropertyName("system")]
    public Common System
    {
      get => system ??= new();
      set => system = value;
    }

    /// <summary>
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public AbendData Returned
    {
      get => returned ??= new();
      set => returned = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of WorkCase.
    /// </summary>
    [JsonPropertyName("workCase")]
    public Case1 WorkCase
    {
      get => workCase ??= new();
      set => workCase = value;
    }

    /// <summary>
    /// A value of WorkServiceProvider.
    /// </summary>
    [JsonPropertyName("workServiceProvider")]
    public ServiceProvider WorkServiceProvider
    {
      get => workServiceProvider ??= new();
      set => workServiceProvider = value;
    }

    /// <summary>
    /// A value of WorkCaseRole.
    /// </summary>
    [JsonPropertyName("workCaseRole")]
    public CaseRole WorkCaseRole
    {
      get => workCaseRole ??= new();
      set => workCaseRole = value;
    }

    /// <summary>
    /// A value of WorkCsePerson.
    /// </summary>
    [JsonPropertyName("workCsePerson")]
    public CsePerson WorkCsePerson
    {
      get => workCsePerson ??= new();
      set => workCsePerson = value;
    }

    /// <summary>
    /// A value of ApMatchMade.
    /// </summary>
    [JsonPropertyName("apMatchMade")]
    public Common ApMatchMade
    {
      get => apMatchMade ??= new();
      set => apMatchMade = value;
    }

    /// <summary>
    /// A value of ApSearchLength.
    /// </summary>
    [JsonPropertyName("apSearchLength")]
    public Common ApSearchLength
    {
      get => apSearchLength ??= new();
      set => apSearchLength = value;
    }

    /// <summary>
    /// A value of ArSearchLength.
    /// </summary>
    [JsonPropertyName("arSearchLength")]
    public Common ArSearchLength
    {
      get => arSearchLength ??= new();
      set => arSearchLength = value;
    }

    /// <summary>
    /// A value of StartingCase.
    /// </summary>
    [JsonPropertyName("startingCase")]
    public TextWorkArea StartingCase
    {
      get => startingCase ??= new();
      set => startingCase = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CdvlReturnCode.
    /// </summary>
    [JsonPropertyName("cdvlReturnCode")]
    public Common CdvlReturnCode
    {
      get => cdvlReturnCode ??= new();
      set => cdvlReturnCode = value;
    }

    /// <summary>
    /// A value of AsgnCount.
    /// </summary>
    [JsonPropertyName("asgnCount")]
    public Common AsgnCount
    {
      get => asgnCount ??= new();
      set => asgnCount = value;
    }

    /// <summary>
    /// A value of ApCount.
    /// </summary>
    [JsonPropertyName("apCount")]
    public Common ApCount
    {
      get => apCount ??= new();
      set => apCount = value;
    }

    /// <summary>
    /// A value of ClearGroupView.
    /// </summary>
    [JsonPropertyName("clearGroupView")]
    public Common ClearGroupView
    {
      get => clearGroupView ??= new();
      set => clearGroupView = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Case1 Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of CauCount.
    /// </summary>
    [JsonPropertyName("cauCount")]
    public Common CauCount
    {
      get => cauCount ??= new();
      set => cauCount = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of OspFound.
    /// </summary>
    [JsonPropertyName("ospFound")]
    public Common OspFound
    {
      get => ospFound ??= new();
      set => ospFound = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of PadLeftWithZeros.
    /// </summary>
    [JsonPropertyName("padLeftWithZeros")]
    public TextWorkArea PadLeftWithZeros
    {
      get => padLeftWithZeros ??= new();
      set => padLeftWithZeros = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of ArWorkArea.
    /// </summary>
    [JsonPropertyName("arWorkArea")]
    public WorkArea ArWorkArea
    {
      get => arWorkArea ??= new();
      set => arWorkArea = value;
    }

    /// <summary>
    /// A value of ApWorkArea.
    /// </summary>
    [JsonPropertyName("apWorkArea")]
    public WorkArea ApWorkArea
    {
      get => apWorkArea ??= new();
      set => apWorkArea = value;
    }

    /// <summary>
    /// A value of TextServiceProviderId.
    /// </summary>
    [JsonPropertyName("textServiceProviderId")]
    public WorkArea TextServiceProviderId
    {
      get => textServiceProviderId ??= new();
      set => textServiceProviderId = value;
    }

    /// <summary>
    /// A value of TextOspEffDate.
    /// </summary>
    [JsonPropertyName("textOspEffDate")]
    public WorkArea TextOspEffDate
    {
      get => textOspEffDate ??= new();
      set => textOspEffDate = value;
    }

    private Common tribunalFound;
    private Common prompt;
    private Common system;
    private AbendData returned;
    private Common errOnAdabasUnavailable;
    private Case1 workCase;
    private ServiceProvider workServiceProvider;
    private CaseRole workCaseRole;
    private CsePerson workCsePerson;
    private Common apMatchMade;
    private Common apSearchLength;
    private Common arSearchLength;
    private TextWorkArea startingCase;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private Common cdvlReturnCode;
    private Common asgnCount;
    private Common apCount;
    private Common clearGroupView;
    private Case1 null1;
    private Common cauCount;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private Program program;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Common ospFound;
    private Common count;
    private DateWorkArea initialized;
    private TextWorkArea padLeftWithZeros;
    private DateWorkArea max;
    private DateWorkArea current;
    private WorkArea arWorkArea;
    private WorkArea apWorkArea;
    private WorkArea textServiceProviderId;
    private WorkArea textOspEffDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public CsePerson KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of ExistingCaseUnit.
    /// </summary>
    [JsonPropertyName("existingCaseUnit")]
    public CaseUnit ExistingCaseUnit
    {
      get => existingCaseUnit ??= new();
      set => existingCaseUnit = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
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
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private Tribunal tribunal;
    private CsePerson keyOnly;
    private CaseUnit existingCaseUnit;
    private CsePerson existingCsePerson;
    private CaseRole existingCaseRole;
    private Case1 existingCase;
    private CaseAssignment existingCaseAssignment;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private ServiceProvider existingServiceProvider;
    private Office existingOffice;
  }
#endregion
}
