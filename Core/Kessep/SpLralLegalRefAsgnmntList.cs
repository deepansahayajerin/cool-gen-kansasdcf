// Program: SP_LRAL_LEGAL_REF_ASGNMNT_LIST, ID: 372246099, model: 746.
// Short name: SWELRALP
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
/// A program: SP_LRAL_LEGAL_REF_ASGNMNT_LIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpLralLegalRefAsgnmntList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_LRAL_LEGAL_REF_ASGNMNT_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpLralLegalRefAsgnmntList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpLralLegalRefAsgnmntList.
  /// </summary>
  public SpLralLegalRefAsgnmntList(IContext context, Import import,
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
    // Date    Developer         Request #  Description
    // ------------------------------------------------
    // 3-11-98 J Rookard, MTW    IDCR 413   Initial development
    // 9-04-00 P Phinney         H00100522  Fix Scrolling
    // Increased field for Number of Records.
    // 10-31-03 GVandy           PR190941   Performance Fixes
    // 12-03-10 GVandy	  	  CQ109      Add search field for override indicator.
    // 10-2-2013 JHarden CQ38042 Hide field on screen because it didn't work 
    // correctly.  Making it less confuseing for user.
    // ------------------------------------------------
    // *****************************************************************
    // resp:  Service Plan
    // This procedure step lists open case assignments for the targeted
    // office service provider.  It is explicitly scrolled and performs
    // no updates or deletes.  Jack Rookard, MTW 2-16-98
    // *********************************************
    // Crook 23 Feb 99 ***
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // *****************************************************************
    // Housekeeping
    // *********************************************
    // Crook 04 Mar 99 ***
    local.Current.Date = Now().Date;

    // *****************************************************************
    // Move Imports to Exports
    // *********************************************
    // Crook 04 Mar 99 ***
    MoveOffice(import.Office, export.Office);
    export.AsgnCount.Count = import.AsgnCount.Count;
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.SearchPromptSvpo.SelectChar = import.SearchPromptSvpo.SelectChar;
    MoveOfficeServiceProvider(import.OfficeServiceProvider,
      export.OfficeServiceProvider);
    export.SearchAp.Text10 = import.SearchAp.Text10;
    export.SearchApFi.Text1 = import.SearchApFi.Text1;
    export.SearchLegalReferral.Assign(import.SearchLegalReferral);
    export.SearchCodeValue.Cdvalue = import.SearchCodeValue.Cdvalue;
    export.SearchPromptCdvl.SelectChar = import.SearchPromptCdvl.SelectChar;
    export.SearchLegalReferralAssignment.OverrideInd =
      import.SearchLegalReferralAssignment.OverrideInd;

    // 9-04-00 P Phinney         H00100522  Fix Scrolling
    local.TextWorkArea.Text10 = "";

    // 9-04-00 P Phinney         H00100522  Fix Scrolling
    if (!IsEmpty(import.Starting.Number))
    {
      local.TextWorkArea.Text10 = import.Starting.Number;
      UseEabPadLeftWithZeros();
    }

    export.Starting.Number = local.TextWorkArea.Text10;
    export.Starting.Number = import.Starting.Number;
    export.MaxPages.Count = import.MaxPages.Count;

    // *****************************************************************
    // Move Import Hiddens to Export Hiddens
    // *********************************************
    // Crook 04 Mar 99 ***
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenSearchAp.Text10 = import.HiddenSearchAp.Text10;
    export.HiddenSearchApFi.Text1 = import.HiddenSearchApFi.Text1;
    export.HiddenSearchLegalReferral.Assign(import.HiddenSearchLegalReferral);
    export.HiddenSearchCodeValue.Cdvalue = import.HiddenSearchCodeValue.Cdvalue;
    export.HiddenSearchLegalReferralAssignment.OverrideInd =
      import.HiddenSearchLegalReferralAssignment.OverrideInd;
    export.HiddenCode.CodeName = import.HiddenCode.CodeName;
    export.HiddenCalcDone.Flag = import.HiddenCalcDone.Flag;

    // 9-04-00 P Phinney         H00100522  Fix Scrolling
    export.HiddenStarting.Number = import.HiddenStarting.Number;
    export.ShowOnlyArrsCases.Flag = import.ShowOnlyArrsCases.Flag;

    if (AsChar(export.HiddenCalcDone.Flag) != 'Y')
    {
      export.HiddenCalcDone.Flag = "N";
    }

    if (IsEmpty(export.ShowOnlyArrsCases.Flag))
    {
      export.ShowOnlyArrsCases.Flag = "N";
    }

    export.Group.Index = -1;

    if (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "RETSVPO") || Equal(global.Command, "RETCDVL"))
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
        export.Group.Update.DetailLegalReferral.Assign(
          import.Group.Item.DetailLegalReferral);
        export.Group.Update.DetailLegalReferralAssignment.Assign(
          import.Group.Item.DetailLegalReferralAssignment);
        export.Group.Update.DetailCase.Number =
          import.Group.Item.DetailCase.Number;
        MoveCsePersonsWorkSet(import.Group.Item.ArDetail,
          export.Group.Update.ArDetail);
        MoveCsePersonsWorkSet(import.Group.Item.ApDetail,
          export.Group.Update.ApDetail);
        export.Group.Update.DetailPrgm.Text3 =
          import.Group.Item.DetailPrgm.Text3;

        if (!IsEmpty(export.Group.Item.DetailCase.Number))
        {
          local.TextWorkArea.Text10 = export.Group.Item.DetailCase.Number;
          UseEabPadLeftWithZeros();
          export.Group.Update.DetailCase.Number = local.TextWorkArea.Text10;
        }
      }

      import.Group.CheckIndex();

      // Move page key imports to page key exports
      export.CurrentPage.Count = import.CurrentPage.Count;

      for(import.PageKeys.Index = 0; import.PageKeys.Index < Import
        .PageKeysGroup.Capacity; ++import.PageKeys.Index)
      {
        if (!import.PageKeys.CheckSize())
        {
          break;
        }

        export.PageKeys.Index = import.PageKeys.Index;
        export.PageKeys.CheckSize();

        export.PageKeys.Update.PageKeyCase.Number =
          import.PageKeys.Item.PageKeyCase.Number;
        export.PageKeys.Update.PageKeyLegalReferral.Identifier =
          import.PageKeys.Item.PageKeyLegalReferral.Identifier;
      }

      import.PageKeys.CheckIndex();
      export.Scroll.ScrollingMessage = import.Scroll.ScrollingMessage;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

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
          
        export.HiddenCalcDone.Flag = "N";
      }

      export.SearchPromptSvpo.SelectChar = "";
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
        export.SearchCodeValue.Cdvalue = import.HiddenCodeValue.Cdvalue;
        export.HiddenCodeValue.Cdvalue = import.HiddenCodeValue.Cdvalue;
        export.HiddenSearchCodeValue.Cdvalue = import.HiddenCodeValue.Cdvalue;
      }

      var field = GetField(export.SearchPromptCdvl, "selectChar");

      field.Protected = false;
      field.Focused = true;

      export.SearchPromptCdvl.SelectChar = "";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCOMP") || Equal
      (global.Command, "RETLACS") || Equal(global.Command, "RTFRMLNK"))
    {
      // User is returning on a link from either COMP, LACS, or LGRQ. Views have
      // already been populated.  Escape.
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
          case '*':
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

    // NEXT field should only work with ENTER
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      if (!Equal(global.Command, "ENTER"))
      {
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      }

      // User is going out of this screen to another screen.
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        return;
      }
    }
    else if (Equal(global.Command, "ENTER"))
    {
      ExitState = "ACO_NE0000_INVALID_COMMAND";

      return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PRINT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // 9-04-00 P Phinney         H00100522  Fix Scrolling
      if (!IsEmpty(export.Starting.Number))
      {
        export.HiddenStarting.Number = "";
      }
    }

    // Group view select processing.
    if (Equal(global.Command, "COMP") || Equal(global.Command, "LGRQ") || Equal
      (global.Command, "LACS"))
    {
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
          case '*':
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

    // *****************************************************************
    // List - Check for Prompts
    // *********************************************
    // Crook 11 Mar 99 ***
    if (Equal(global.Command, "LIST"))
    {
      // Prompt Check
      if (AsChar(export.SearchPromptCdvl.SelectChar) == 'S' && AsChar
        (export.SearchPromptSvpo.SelectChar) == 'S')
      {
        var field3 = GetField(export.SearchPromptCdvl, "selectChar");

        field3.Error = true;

        var field4 = GetField(export.SearchPromptSvpo, "selectChar");

        field4.Error = true;

        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        return;
      }

      switch(AsChar(export.SearchPromptCdvl.SelectChar))
      {
        case 'S':
          export.HiddenCode.CodeName = "CASE LEVEL PROGRAM";
          ExitState = "ECO_LNK_TO_CDVL";

          return;
        case ' ':
          break;
        case '+':
          break;
        default:
          var field = GetField(export.SearchPromptCdvl, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          return;
      }

      switch(AsChar(export.SearchPromptSvpo.SelectChar))
      {
        case 'S':
          ExitState = "ECO_LNK_TO_SVPO";

          return;
        case ' ':
          break;
        case '+':
          break;
        default:
          var field = GetField(export.SearchPromptSvpo, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          return;
      }

      // At this point we know that no acceptable prompt was entered,
      //   but for some unknown reason the PF4 List key was pushed
      var field1 = GetField(export.SearchPromptCdvl, "selectChar");

      field1.Error = true;

      var field2 = GetField(export.SearchPromptSvpo, "selectChar");

      field2.Error = true;

      ExitState = "ACO_NE0000_NO_SELECTION_MADE";

      return;
    }

    // *****************************************************************
    // Check Filters for Change
    // *********************************************
    // Crook 04 Mar 99 ***
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PRINT"))
    {
      // *****************************************************************
      // Check Status Filter
      // *********************************************
      // Crook 07 Apr 99 ***
      if (IsEmpty(import.SearchLegalReferral.Status) || AsChar
        (import.SearchLegalReferral.Status) == 'O' || AsChar
        (import.SearchLegalReferral.Status) == 'S')
      {
        // Plez continue, the above are all acceptable values
      }
      else
      {
        var field = GetField(export.SearchLegalReferral, "status");

        field.Error = true;

        ExitState = "SP0000_ONLY_O_OR_S_ALLOWED";

        return;
      }

      // *****************************************************************
      // Move Filters to Hiddens
      // *********************************************
      // Crook 07 Apr 99 ***
      // 9-04-00 P Phinney         H00100522  Fix Scrolling
      if (!Equal(import.SearchCodeValue.Cdvalue,
        export.HiddenSearchCodeValue.Cdvalue) || !
        Equal(import.SearchLegalReferral.ReferralDate,
        export.HiddenSearchLegalReferral.ReferralDate) || !
        Equal(import.SearchLegalReferral.ReferralReason1,
        export.HiddenSearchLegalReferral.ReferralReason1) || AsChar
        (import.SearchLegalReferral.Status) != AsChar
        (export.HiddenSearchLegalReferral.Status) || !
        Equal(import.SearchAp.Text10, export.HiddenSearchAp.Text10) || !
        Equal(import.Starting.Number, export.HiddenStarting.Number) || AsChar
        (import.SearchLegalReferralAssignment.OverrideInd) != AsChar
        (export.HiddenSearchLegalReferralAssignment.OverrideInd))
      {
        export.HiddenCalcDone.Flag = "N";
      }

      export.HiddenSearchAp.Text10 = import.SearchAp.Text10;
      export.HiddenSearchApFi.Text1 = import.SearchApFi.Text1;
      export.HiddenSearchLegalReferral.Assign(import.SearchLegalReferral);
      export.HiddenSearchCodeValue.Cdvalue = import.SearchCodeValue.Cdvalue;
      export.HiddenSearchLegalReferralAssignment.OverrideInd =
        import.SearchLegalReferralAssignment.OverrideInd;
    }
    else
    {
      local.FiltersHaveChanged.Flag = "N";

      if (!Equal(import.SearchAp.Text10, import.HiddenSearchAp.Text10))
      {
        var field = GetField(export.SearchAp, "text10");

        field.Error = true;

        local.FiltersHaveChanged.Flag = "Y";
      }

      if (AsChar(import.SearchApFi.Text1) != AsChar
        (import.HiddenSearchApFi.Text1))
      {
        var field = GetField(export.SearchApFi, "text1");

        field.Error = true;

        local.FiltersHaveChanged.Flag = "Y";
      }

      if (!Equal(import.SearchLegalReferral.ReferralDate,
        import.HiddenSearchLegalReferral.ReferralDate))
      {
        var field = GetField(export.SearchLegalReferral, "referralDate");

        field.Error = true;

        local.FiltersHaveChanged.Flag = "Y";
      }

      if (AsChar(import.SearchLegalReferral.Status) != AsChar
        (import.HiddenSearchLegalReferral.Status))
      {
        var field = GetField(export.SearchLegalReferral, "status");

        field.Error = true;

        local.FiltersHaveChanged.Flag = "Y";
      }

      if (!Equal(import.SearchLegalReferral.ReferralReason1,
        import.HiddenSearchLegalReferral.ReferralReason1))
      {
        var field = GetField(export.SearchLegalReferral, "referralReason1");

        field.Error = true;

        local.FiltersHaveChanged.Flag = "Y";
      }

      if (!Equal(import.SearchCodeValue.Cdvalue,
        import.HiddenSearchCodeValue.Cdvalue))
      {
        var field = GetField(export.SearchCodeValue, "cdvalue");

        field.Error = true;

        local.FiltersHaveChanged.Flag = "Y";
      }

      // 9-04-00 P Phinney         H00100522  Fix Scrolling
      if (!Equal(import.Starting.Number, import.HiddenStarting.Number))
      {
        var field = GetField(export.Starting, "number");

        field.Error = true;

        local.FiltersHaveChanged.Flag = "Y";
      }

      if (AsChar(export.SearchLegalReferralAssignment.OverrideInd) != AsChar
        (export.HiddenSearchLegalReferralAssignment.OverrideInd))
      {
        var field =
          GetField(export.SearchLegalReferralAssignment, "overrideInd");

        field.Error = true;

        local.FiltersHaveChanged.Flag = "Y";
      }

      if (AsChar(local.FiltersHaveChanged.Flag) == 'Y')
      {
        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";

        return;
      }
    }

    // Main case of command.
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        // Prompt Check done above
        break;
      case "DISPLAY":
        // Display processing at bottom of Procedure step.
        break;
      case "PRINT":
        // Print processing at bottom of Procedure step.
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
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
          // 9-04-00 P Phinney         H00100522  Fix Scrolling
          export.Group.Index = Export.GroupGroup.Capacity - 1;
          export.Group.CheckSize();

          if (!IsEmpty(export.Group.Item.DetailCase.Number))
          {
            export.Starting.Number = export.Group.Item.DetailCase.Number;

            goto Test1;
          }

          ExitState = "OE0000_LIST_IS_FULL";

          return;
        }

Test1:

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
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "COMP":
        ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

        return;
      case "LACS":
        ExitState = "ECO_LNK_TO_LACS";

        return;
      case "LGRQ":
        ExitState = "ECO_LNK_TO_LEGAL_REQUEST";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // 9-04-00 P Phinney         H00100522  Fix Scrolling
    if (!Equal(export.Starting.Number, export.HiddenStarting.Number))
    {
      local.ClearGroupView.Flag = "Y";
      export.HiddenCalcDone.Flag = "N";

      // 9-04-00 P Phinney         H00100522  Fix Scrolling
      if (!IsEmpty(export.Starting.Number))
      {
        local.TextWorkArea.Text10 = import.Starting.Number;
        UseEabPadLeftWithZeros();
        export.Starting.Number = local.TextWorkArea.Text10;
      }
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

        export.Group.Update.DetailLegalReferral.Identifier = 0;
        export.Group.Update.DetailLegalReferral.ReferralDate =
          local.Initialized.Date;
        export.Group.Update.DetailLegalReferral.ReferralReason1 = "";
        export.Group.Update.DetailLegalReferral.ReferralReason2 = "";
        export.Group.Update.DetailLegalReferral.ReferralReason3 = "";
        export.Group.Update.DetailLegalReferral.ReferralReason4 = "";
        export.Group.Update.DetailLegalReferral.Status = "";
        export.Group.Update.DetailCommon.SelectChar = "";
        export.Group.Update.DetailCase.Number = "";
        export.Group.Update.ArDetail.FormattedName = "";
        export.Group.Update.ApDetail.FormattedName = "";
        export.Group.Update.DetailPrgm.Text3 = "";
        export.Group.Update.DetailLegalReferralAssignment.OverrideInd = "";
      }

      export.Group.CheckIndex();
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PRINT"))
    {
      if (!IsEmpty(export.SearchApFi.Text1) && IsEmpty(export.SearchAp.Text10))
      {
        var field = GetField(export.SearchApFi, "text1");

        field.Color = "red";
        field.Protected = false;
        field.Focused = true;

        ExitState = "ACO_NE0000_INVALID_ACTION";

        return;
      }

      if (IsEmpty(export.OfficeServiceProvider.RoleCode) && Equal
        (export.OfficeServiceProvider.EffectiveDate, local.Initialized.Date) &&
        export.ServiceProvider.SystemGeneratedId == 0 && export
        .Office.SystemGeneratedId == 0)
      {
        // This will occur on a dialog flow from the menu to this procedure, or,
        // when a next tran to this procedure is performed and the user then
        // presses PF2 Display, or, when the user has performed a Clear and then
        // presses PF2 Display.  Determine the logged on user and derive the
        // first occurrence of Office Service Provider for this user, ordered by
        // Office.
        if (ReadServiceProvider2())
        {
          export.ServiceProvider.Assign(entities.ServiceProvider);
          local.Ar.FirstName = entities.ServiceProvider.FirstName;
          local.Ar.LastName = entities.ServiceProvider.LastName;
          local.Ar.MiddleInitial = entities.ServiceProvider.MiddleInitial;
          UseSiFormatCsePersonName();
          export.ServiceProvider.LastName = local.Ar.FormattedName;
          local.OspFound.Flag = "N";

          if (ReadOfficeOfficeServiceProvider())
          {
            MoveOfficeServiceProvider(entities.OfficeServiceProvider,
              export.OfficeServiceProvider);
            MoveOffice(entities.Office, export.Office);
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

        if (ReadOffice())
        {
          MoveOffice(entities.Office, export.Office);
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

        if (ReadServiceProvider1())
        {
          export.ServiceProvider.Assign(entities.ServiceProvider);
          local.Ar.FirstName = entities.ServiceProvider.FirstName;
          local.Ar.LastName = entities.ServiceProvider.LastName;
          local.Ar.MiddleInitial = entities.ServiceProvider.MiddleInitial;
          UseSiFormatCsePersonName();
          export.ServiceProvider.LastName = local.Ar.FormattedName;
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

        if (ReadOfficeServiceProvider())
        {
          MoveOfficeServiceProvider(entities.OfficeServiceProvider,
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

      if (!IsEmpty(export.SearchCodeValue.Cdvalue))
      {
        export.Code.CodeName = "CASE LEVEL PROGRAM";
        UseCabValidateCodeValue();

        switch(local.CdvlReturnCode.Count)
        {
          case 0:
            break;
          case 1:
            var field1 = GetField(export.SearchCodeValue, "cdvalue");

            field1.Error = true;

            return;

            ExitState = "CODE_NF";

            break;
          case 2:
            ExitState = "CODE_VALUE_NF";

            var field2 = GetField(export.SearchCodeValue, "cdvalue");

            field2.Error = true;

            return;
          default:
            break;
        }
      }

      // 12-03-10 SWSRGAV  CQ109  Validate the search override indicator.
      switch(AsChar(import.SearchLegalReferralAssignment.OverrideInd))
      {
        case 'Y':
          break;
        case 'N':
          break;
        case ' ':
          break;
        default:
          var field =
            GetField(export.SearchLegalReferralAssignment, "overrideInd");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_Y_OR_N";

          return;
      }
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (AsChar(export.HiddenCalcDone.Flag) == 'N')
      {
        local.AsgnCount.Count = 0;

        // *****************************************************************
        // Count Display Items
        // *********************************************
        // Crook 13 Apr 99 ***
        if (!IsEmpty(export.SearchAp.Text10))
        {
          local.ApSearchLength.Count = Length(TrimEnd(export.SearchAp.Text10));
        }

        // 9-04-00 P Phinney         H00100522  Fix Scrolling
        if (!Equal(export.Starting.Number, export.HiddenStarting.Number))
        {
          // 9-04-00 P Phinney         H00100522  Fix Scrolling
          export.PageKeys.Index = 0;
          export.PageKeys.CheckSize();

          export.PageKeys.Update.PageKeyCase.Number = export.Starting.Number;
          export.PageKeys.Update.PageKeyLegalReferral.Identifier = 0;
        }

        export.Group.Index = -1;

        // The below statement added solely to improve readability of the below 
        // READ EACH
        local.SearchLglRefReasonCode.Text4 =
          export.SearchLegalReferral.ReferralReason1;

        // *****************************************************************
        // Count  Display Items
        // *********************************************
        // Crook 13 Apr 99 ***
        foreach(var item in ReadLegalReferralLegalReferralAssignmentCase1())
        {
          // : If Show Arrears Only Cases is True, then skip all Zero Balance 
          // Obligors.
          if (AsChar(export.ShowOnlyArrsCases.Flag) == 'Y')
          {
            // 10-31-2003 GVandy PR190941 Performance Fixes.  Correct table 
            // space scan of debt detail.  The original
            // summarize is commented out below.
            foreach(var item1 in ReadObligor())
            {
              if (ReadDebtDetail())
              {
                goto Test2;
              }
            }

            continue;
          }

Test2:

          if (!IsEmpty(export.SearchAp.Text10))
          {
            // This IF was added for performance reasons by Carl Galka on 10-19-
            // 1999. We do not need to find the AP at this point, if there is
            // not search criteria based on NAME
            // ************************************************
            // Search and match the APs name.
            // ************************************************
            local.ApCount.Count = 0;
            local.ApMatchMade.Flag = "N";

            foreach(var item1 in ReadCaseRoleCsePerson2())
            {
              ++local.ApCount.Count;

              if (IsEmpty(export.SearchAp.Text10) && local.ApCount.Count > 1)
              {
                break;
              }

              if (!IsEmpty(export.SearchAp.Text10) && local.ApCount.Count > 1
                && AsChar(local.ApMatchMade.Flag) == 'Y')
              {
                break;
              }

              local.Ap.Number = entities.KeyOnly.Number;
              UseSiReadCsePerson1();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                // 9-04-00 P Phinney         H00100522  Fix Scrolling
                local.Ap.LastName = "";
                ExitState = "ACO_NN0000_ALL_OK";
              }

              // Is Last Name equal to the search key Last Name
              if (!IsEmpty(export.SearchAp.Text10))
              {
                // 9-04-00 P Phinney         H00100522  Fix Scrolling
                if (IsEmpty(local.Ap.LastName))
                {
                  local.Ap.FirstName = "";
                  local.Ap.FormattedName = "";

                  continue;
                }

                local.Ap.LastName =
                  Substring(local.Ap.LastName, 1, local.ApSearchLength.Count);

                if (Equal(local.Ap.LastName, export.SearchAp.Text10))
                {
                  local.ApMatchMade.Flag = "Y";

                  // Qualify 1st Initial
                  if (!IsEmpty(export.SearchApFi.Text1))
                  {
                    local.ApMatchMade.Flag = "N";
                    local.Ap.FirstName = Substring(local.Ap.FirstName, 1, 1);

                    if (!Lt(local.Ap.FirstName, export.SearchApFi.Text1))
                    {
                      local.ApMatchMade.Flag = "Y";

                      continue;
                    }
                    else
                    {
                      local.Ap.FirstName = "";
                      local.Ap.FormattedName = "";
                      local.Ap.LastName = "";

                      continue;
                    }
                  }
                }
                else
                {
                  local.Ap.FirstName = "";
                  local.Ap.FormattedName = "";
                  local.Ap.LastName = "";

                  continue;
                }
              }
            }

            if (!IsEmpty(export.SearchAp.Text10) && AsChar
              (local.ApMatchMade.Flag) == 'N')
            {
              continue;
            }
          }

          if (!IsEmpty(export.SearchCodeValue.Cdvalue))
          {
            // The SET , the USE, and the IF EXITSTATE was moved by Carl Galka 
            // on 10/19/1999 for performance reasons. We do not need to call the
            // read_case_program_code if there is no search criteria for
            // program code.
            // ************************************************
            // Determine the Case Level Program.
            // ************************************************
            local.Program.Code = "";
            UseSiReadCaseProgramType();

            if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }

            if (Equal(local.Program.Code, export.SearchCodeValue.Cdvalue))
            {
            }
            else
            {
              continue;
            }
          }

          ++local.AsgnCount.Count;
        }

        export.AsgnCount.Count = local.AsgnCount.Count;
        export.HiddenCalcDone.Flag = "Y";
      }

      // *****************************************************************
      // Housekeeping for Display Population
      // *********************************************
      // Crook 13 Apr 99 ***
      // handle paging for first time into prad
      if (export.CurrentPage.Count == 0)
      {
        export.CurrentPage.Count = 1;

        export.PageKeys.Index = 0;
        export.PageKeys.CheckSize();
      }

      if (!IsEmpty(export.SearchAp.Text10))
      {
        local.ApSearchLength.Count = Length(TrimEnd(export.SearchAp.Text10));
      }

      // 9-04-00 P Phinney         H00100522  Fix Scrolling
      if (!Equal(export.Starting.Number, export.HiddenStarting.Number))
      {
        // 9-04-00 P Phinney         H00100522  Fix Scrolling
        export.PageKeys.Index = 0;
        export.PageKeys.CheckSize();

        export.PageKeys.Update.PageKeyCase.Number = export.Starting.Number;
        export.PageKeys.Update.PageKeyLegalReferral.Identifier = 0;
      }

      export.Group.Index = -1;

      // The below statement added solely to improve readability of the below 
      // READ EACH
      local.SearchLglRefReasonCode.Text4 =
        export.SearchLegalReferral.ReferralReason1;

      // *****************************************************************
      // Populate Display
      // *********************************************
      // Crook 13 Apr 99 ***
      foreach(var item in ReadLegalReferralLegalReferralAssignmentCase2())
      {
        // : If Show Arrears Only Cases is True, then skip all Zero Balance 
        // Obligors.
        if (AsChar(export.ShowOnlyArrsCases.Flag) == 'Y')
        {
          // 10-31-2003 GVandy PR190941 Performance Fixes.  Correct table space 
          // scan of debt detail.  The original
          // summarize is commented out below.
          foreach(var item1 in ReadObligor())
          {
            if (ReadDebtDetail())
            {
              goto Test3;
            }
          }

          continue;
        }

Test3:

        // ************************************************
        // Prepare the AP data.
        // ************************************************
        local.ApCount.Count = 0;
        local.ApMatchMade.Flag = "N";

        // The following read was changed to include key_only cse_person and the
        // seperate read of CSE _person was elimnated by Carl Galka on 10-19-
        // 1999 for performance reasons.
        foreach(var item1 in ReadCaseRoleCsePerson2())
        {
          ++local.ApCount.Count;

          if (IsEmpty(export.SearchAp.Text10) && local.ApCount.Count > 1)
          {
            break;
          }

          if (!IsEmpty(export.SearchAp.Text10) && local.ApCount.Count > 1 && AsChar
            (local.ApMatchMade.Flag) == 'Y')
          {
            break;
          }

          local.Ap.Number = entities.KeyOnly.Number;
          UseSiReadCsePerson1();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            // 9-04-00 P Phinney         H00100522  Fix Scrolling
            local.Ap.FormattedName = "NoNAME FOUND";
            ExitState = "ACO_NN0000_ALL_OK";
          }

          // Is Last Name equal to the search key Last Name
          if (!IsEmpty(export.SearchAp.Text10))
          {
            local.Ap.LastName =
              Substring(local.Ap.LastName, 1, local.ApSearchLength.Count);

            if (Equal(local.Ap.LastName, export.SearchAp.Text10))
            {
              local.ApMatchMade.Flag = "Y";

              // Qualify 1st Initial
              if (!IsEmpty(export.SearchApFi.Text1))
              {
                local.ApMatchMade.Flag = "N";
                local.Ap.FirstName = Substring(local.Ap.FirstName, 1, 1);

                if (!Lt(local.Ap.FirstName, export.SearchApFi.Text1))
                {
                  local.ApMatchMade.Flag = "Y";

                  continue;
                }
                else
                {
                  local.Ap.FirstName = "";
                  local.Ap.FormattedName = "";
                  local.Ap.LastName = "";

                  continue;
                }
              }
            }
            else
            {
              local.Ap.FirstName = "";
              local.Ap.FormattedName = "";
              local.Ap.LastName = "";

              continue;
            }
          }
        }

        if (!IsEmpty(export.SearchAp.Text10) && AsChar
          (local.ApMatchMade.Flag) == 'N')
        {
          continue;
        }

        if (local.ApCount.Count == 0)
        {
          local.Ap.FormattedName = "NO AP FOUND";
        }

        if (local.ApCount.Count > 1)
        {
          local.Ap.FormattedName =
            Substring(local.Ap.FormattedName,
            CsePersonsWorkSet.FormattedName_MaxLength, 1, 14) + "*";
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

        if (!IsEmpty(export.SearchCodeValue.Cdvalue))
        {
          if (Equal(local.Program.Code, export.SearchCodeValue.Cdvalue))
          {
          }
          else
          {
            continue;
          }
        }

        // The following read was changed to include key_only cse_person and the
        // seperate read of CSE _person was elimnated by Carl Galka on 10-19-
        // 1999 for performance reasons.
        // ************************************************
        // Prepare the AR data.
        // ************************************************
        if (ReadCaseRoleCsePerson1())
        {
          local.Ar.Number = entities.KeyOnly.Number;
          UseSiReadCsePerson2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            // 9-04-00 P Phinney         H00100522  Fix Scrolling
            local.Ar.FormattedName = "NoNAME FOUND";
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }
        else
        {
          // 9-04-00 P Phinney         H00100522  Fix Scrolling
          local.Ar.FormattedName = "NO AR FOUND";
          ExitState = "ACO_NN0000_ALL_OK";
        }

        if (export.Group.Index + 1 >= Export.GroupGroup.Capacity)
        {
          break;
        }

        ++export.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.DetailCase.Number = entities.Case1.Number;
        export.Group.Update.DetailLegalReferral.Assign(entities.LegalReferral);
        export.Group.Update.DetailLegalReferralAssignment.Assign(
          entities.LegalReferralAssignment);
        export.Group.Update.DetailPrgm.Text3 = local.Program.Code;
        export.Group.Update.ArDetail.FormattedName = local.Ar.FormattedName;
        export.Group.Update.ApDetail.FormattedName = local.Ap.FormattedName;

        // 9-04-00 P Phinney         H00100522  Fix Scrolling
        if (Equal(local.Ap.FormattedName, "NoNAME FOUND") || Equal
          (local.Ap.FormattedName, "NO AP FOUND"))
        {
          var field = GetField(export.Group.Item.ApDetail, "formattedName");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }

        if (Equal(local.Ar.FormattedName, "NoNAME FOUND") || Equal
          (local.Ar.FormattedName, "NO AR FOUND"))
        {
          var field = GetField(export.Group.Item.ArDetail, "formattedName");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }
      }

      // 9-04-00 P Phinney         H00100522  Fix Scrolling
      export.HiddenStarting.Number = export.Starting.Number;

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
        export.PageKeys.Update.PageKeyLegalReferral.Identifier =
          export.Group.Item.DetailLegalReferral.Identifier;
      }

      if (export.CurrentPage.Count == 1 && IsEmpty(entities.Case1.Number))
      {
        // Only time this can happen is when no data is found.
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        export.Scroll.ScrollingMessage = "More";

        // 9-04-00 P Phinney         H00100522  Fix Scrolling
        export.MaxPages.Count = 1;
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        // 9-04-00 P Phinney         H00100522  Fix Scrolling
        if (export.AsgnCount.Count > 0)
        {
          export.MaxPages.Count = export.AsgnCount.Count / 14;
        }
        else
        {
          export.MaxPages.Count = 1;
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        if ((long)export.MaxPages.Count * 14 < export.AsgnCount.Count)
        {
          ++export.MaxPages.Count;
        }

        if (export.CurrentPage.Count == 1 && !
          Equal(export.PageKeys.Item.PageKeyCase.Number, local.Null1.Number))
        {
          export.Scroll.ScrollingMessage = "More  +";
        }

        if (export.CurrentPage.Count == 1 && Equal
          (export.PageKeys.Item.PageKeyCase.Number, local.Null1.Number))
        {
          export.Scroll.ScrollingMessage = "More";
        }

        if (export.CurrentPage.Count > 1 && !
          Equal(export.PageKeys.Item.PageKeyCase.Number, local.Null1.Number))
        {
          export.Scroll.ScrollingMessage = "More  -+";
        }

        if (export.CurrentPage.Count > 1 && Equal
          (export.PageKeys.Item.PageKeyCase.Number, local.Null1.Number))
        {
          export.Scroll.ScrollingMessage = "More  -";
        }

        if (export.PageKeys.Index + 1 == Export.PageKeysGroup.Capacity)
        {
          export.Scroll.ScrollingMessage = "More  -";
        }
      }
    }
    else if (Equal(global.Command, "PRINT"))
    {
      // : Format and Set the TranCode and Parm Information for the JOB as 
      // required by the Print Function.
      export.DlgflwJob.TranId = global.TranCode;
      local.SvcPrdrIdTxt.Text5 =
        NumberToString(export.ServiceProvider.SystemGeneratedId, 11, 5);
      local.OfficeIdTxt.Text5 =
        NumberToString(export.Office.SystemGeneratedId, 11, 5);
      local.TextMm.Text2 =
        NumberToString(Month(export.SearchLegalReferral.ReferralDate), 14, 2);
      local.TextDd.Text2 =
        NumberToString(Day(export.SearchLegalReferral.ReferralDate), 14, 2);
      local.TextYyyy.Text4 =
        NumberToString(Year(export.SearchLegalReferral.ReferralDate), 12, 4);
      local.RefDateTxt.Text10 = local.TextYyyy.Text4 + "-" + local
        .TextMm.Text2 + "-" + local.TextDd.Text2;
      local.TextMm.Text2 =
        NumberToString(Month(export.OfficeServiceProvider.EffectiveDate), 14, 2);
        
      local.TextDd.Text2 =
        NumberToString(Day(export.OfficeServiceProvider.EffectiveDate), 14, 2);
      local.TextYyyy.Text4 =
        NumberToString(Year(export.OfficeServiceProvider.EffectiveDate), 12, 4);
        
      local.OffSvcPrdrDateTxt.Text10 = local.TextYyyy.Text4 + "-" + local
        .TextMm.Text2 + "-" + local.TextDd.Text2;
      export.DlgflwJobRun.ParmInfo = local.SvcPrdrIdTxt.Text5 + " " + export
        .SearchAp.Text10 + " " + export.SearchApFi.Text1 + " " + export
        .Starting.Number + " " + local.RefDateTxt.Text10 + " " + (
          export.SearchLegalReferral.Status ?? "") + " " + export
        .SearchLegalReferral.ReferralReason1 + Substring
        (export.SearchCodeValue.Cdvalue, CodeValue.Cdvalue_MaxLength, 1, 3) + " " +
        export.ShowOnlyArrsCases.Flag + " " + local.OfficeIdTxt.Text5 + " " + local
        .OffSvcPrdrDateTxt.Text10 + " " + export
        .OfficeServiceProvider.RoleCode;
      export.DlgflwJobRun.ParmInfo =
        Substring(export.DlgflwJobRun.ParmInfo, 1, 72) + " " + import
        .SearchLegalReferralAssignment.OverrideInd;
      ExitState = "CO_LINK_TO_POPT";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
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
    useImport.CodeValue.Cdvalue = export.SearchCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.CdvlReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
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

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.Ar);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.Ar.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Ap.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Ap.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Ar.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Ar.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.KeyOnly.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.KeyOnly.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.KeyOnly.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson2()
  {
    entities.KeyOnly.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.KeyOnly.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.KeyOnly.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligor.Populated);
    entities.ExistingDebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "retiredDt", local.Initialized.Date.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.ExistingObligor.Type1);
        db.SetString(command, "cspNumber", entities.ExistingObligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 6);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 7);
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);
      });
  }

  private IEnumerable<bool> ReadLegalReferralLegalReferralAssignmentCase1()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Case1.Populated = false;
    entities.LegalReferral.Populated = false;
    entities.LegalReferralAssignment.Populated = false;

    return ReadEach("ReadLegalReferralLegalReferralAssignmentCase1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "overrideInd",
          import.SearchLegalReferralAssignment.OverrideInd);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "referralDate",
          export.SearchLegalReferral.ReferralDate.GetValueOrDefault());
        db.SetDate(command, "date", local.Initialized.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "status", export.SearchLegalReferral.Status ?? "");
        db.SetString(command, "text4", local.SearchLglRefReasonCode.Text4);
        db.SetString(
          command, "casNumber", export.PageKeys.Item.PageKeyCase.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 3);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 4);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 7);
        entities.LegalReferralAssignment.OverrideInd = db.GetString(reader, 8);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 12);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 13);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 14);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 15);
        entities.Case1.Populated = true;
        entities.LegalReferral.Populated = true;
        entities.LegalReferralAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralLegalReferralAssignmentCase2()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Case1.Populated = false;
    entities.LegalReferral.Populated = false;
    entities.LegalReferralAssignment.Populated = false;

    return ReadEach("ReadLegalReferralLegalReferralAssignmentCase2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "overrideInd",
          import.SearchLegalReferralAssignment.OverrideInd);
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "referralDate",
          export.SearchLegalReferral.ReferralDate.GetValueOrDefault());
        db.SetDate(command, "date", local.Initialized.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "status", export.SearchLegalReferral.Status ?? "");
        db.SetString(command, "text4", local.SearchLglRefReasonCode.Text4);
        db.SetString(
          command, "casNumber", export.PageKeys.Item.PageKeyCase.Number);
        db.SetInt32(
          command, "identifier",
          export.PageKeys.Item.PageKeyLegalReferral.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 3);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 4);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 7);
        entities.LegalReferralAssignment.OverrideInd = db.GetString(reader, 8);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 12);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 13);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 14);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 15);
        entities.Case1.Populated = true;
        entities.LegalReferral.Populated = true;
        entities.LegalReferralAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligor()
  {
    entities.ExistingObligor.Populated = false;

    return ReadEach("ReadObligor",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingObligor.CspNumber = db.GetString(reader, 0);
        entities.ExistingObligor.Type1 = db.GetString(reader, 1);
        entities.ExistingObligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.ExistingObligor.Type1);

        return true;
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeOfficeServiceProvider()
  {
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 3);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 4);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetDate(
          command, "effectiveDate",
          export.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", export.OfficeServiceProvider.RoleCode);
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", export.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
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
      /// A value of PageKeyLegalReferral.
      /// </summary>
      [JsonPropertyName("pageKeyLegalReferral")]
      public LegalReferral PageKeyLegalReferral
      {
        get => pageKeyLegalReferral ??= new();
        set => pageKeyLegalReferral = value;
      }

      /// <summary>
      /// A value of PageKeyCase.
      /// </summary>
      [JsonPropertyName("pageKeyCase")]
      public Case1 PageKeyCase
      {
        get => pageKeyCase ??= new();
        set => pageKeyCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 875;

      private LegalReferral pageKeyLegalReferral;
      private Case1 pageKeyCase;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetailLegalReferralAssignment.
      /// </summary>
      [JsonPropertyName("detailLegalReferralAssignment")]
      public LegalReferralAssignment DetailLegalReferralAssignment
      {
        get => detailLegalReferralAssignment ??= new();
        set => detailLegalReferralAssignment = value;
      }

      /// <summary>
      /// A value of DetailLegalReferral.
      /// </summary>
      [JsonPropertyName("detailLegalReferral")]
      public LegalReferral DetailLegalReferral
      {
        get => detailLegalReferral ??= new();
        set => detailLegalReferral = value;
      }

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
      /// A value of DetailPrgm.
      /// </summary>
      [JsonPropertyName("detailPrgm")]
      public WorkArea DetailPrgm
      {
        get => detailPrgm ??= new();
        set => detailPrgm = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private LegalReferralAssignment detailLegalReferralAssignment;
      private LegalReferral detailLegalReferral;
      private Common detailCommon;
      private CsePersonsWorkSet apDetail;
      private CsePersonsWorkSet arDetail;
      private Case1 detailCase;
      private WorkArea detailPrgm;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// A value of HiddenCalcDone.
    /// </summary>
    [JsonPropertyName("hiddenCalcDone")]
    public Common HiddenCalcDone
    {
      get => hiddenCalcDone ??= new();
      set => hiddenCalcDone = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of HiddenOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenOfficeServiceProvider")]
    public OfficeServiceProvider HiddenOfficeServiceProvider
    {
      get => hiddenOfficeServiceProvider ??= new();
      set => hiddenOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenSearchAp.
    /// </summary>
    [JsonPropertyName("hiddenSearchAp")]
    public TextWorkArea HiddenSearchAp
    {
      get => hiddenSearchAp ??= new();
      set => hiddenSearchAp = value;
    }

    /// <summary>
    /// A value of HiddenSearchApFi.
    /// </summary>
    [JsonPropertyName("hiddenSearchApFi")]
    public TextWorkArea HiddenSearchApFi
    {
      get => hiddenSearchApFi ??= new();
      set => hiddenSearchApFi = value;
    }

    /// <summary>
    /// A value of HiddenSearchCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenSearchCodeValue")]
    public CodeValue HiddenSearchCodeValue
    {
      get => hiddenSearchCodeValue ??= new();
      set => hiddenSearchCodeValue = value;
    }

    /// <summary>
    /// A value of HiddenSearchLegalReferral.
    /// </summary>
    [JsonPropertyName("hiddenSearchLegalReferral")]
    public LegalReferral HiddenSearchLegalReferral
    {
      get => hiddenSearchLegalReferral ??= new();
      set => hiddenSearchLegalReferral = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of SearchPromptSvpo.
    /// </summary>
    [JsonPropertyName("searchPromptSvpo")]
    public Common SearchPromptSvpo
    {
      get => searchPromptSvpo ??= new();
      set => searchPromptSvpo = value;
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
    /// A value of SearchAp.
    /// </summary>
    [JsonPropertyName("searchAp")]
    public TextWorkArea SearchAp
    {
      get => searchAp ??= new();
      set => searchAp = value;
    }

    /// <summary>
    /// A value of SearchApFi.
    /// </summary>
    [JsonPropertyName("searchApFi")]
    public TextWorkArea SearchApFi
    {
      get => searchApFi ??= new();
      set => searchApFi = value;
    }

    /// <summary>
    /// A value of SearchCodeValue.
    /// </summary>
    [JsonPropertyName("searchCodeValue")]
    public CodeValue SearchCodeValue
    {
      get => searchCodeValue ??= new();
      set => searchCodeValue = value;
    }

    /// <summary>
    /// A value of SearchLegalReferral.
    /// </summary>
    [JsonPropertyName("searchLegalReferral")]
    public LegalReferral SearchLegalReferral
    {
      get => searchLegalReferral ??= new();
      set => searchLegalReferral = value;
    }

    /// <summary>
    /// A value of SearchPromptCdvl.
    /// </summary>
    [JsonPropertyName("searchPromptCdvl")]
    public Common SearchPromptCdvl
    {
      get => searchPromptCdvl ??= new();
      set => searchPromptCdvl = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Case1 Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of MaxPages.
    /// </summary>
    [JsonPropertyName("maxPages")]
    public Common MaxPages
    {
      get => maxPages ??= new();
      set => maxPages = value;
    }

    /// <summary>
    /// A value of ShowOnlyArrsCases.
    /// </summary>
    [JsonPropertyName("showOnlyArrsCases")]
    public Common ShowOnlyArrsCases
    {
      get => showOnlyArrsCases ??= new();
      set => showOnlyArrsCases = value;
    }

    /// <summary>
    /// A value of SearchLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("searchLegalReferralAssignment")]
    public LegalReferralAssignment SearchLegalReferralAssignment
    {
      get => searchLegalReferralAssignment ??= new();
      set => searchLegalReferralAssignment = value;
    }

    /// <summary>
    /// A value of HiddenSearchLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("hiddenSearchLegalReferralAssignment")]
    public LegalReferralAssignment HiddenSearchLegalReferralAssignment
    {
      get => hiddenSearchLegalReferralAssignment ??= new();
      set => hiddenSearchLegalReferralAssignment = value;
    }

    private Common asgnCount;
    private Code code;
    private Common currentPage;
    private Common hiddenCalcDone;
    private Code hiddenCode;
    private CodeValue hiddenCodeValue;
    private NextTranInfo hiddenNextTranInfo;
    private Office hiddenOffice;
    private OfficeServiceProvider hiddenOfficeServiceProvider;
    private TextWorkArea hiddenSearchAp;
    private TextWorkArea hiddenSearchApFi;
    private CodeValue hiddenSearchCodeValue;
    private LegalReferral hiddenSearchLegalReferral;
    private ServiceProvider hiddenServiceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private Common searchPromptSvpo;
    private Standard scroll;
    private TextWorkArea searchAp;
    private TextWorkArea searchApFi;
    private CodeValue searchCodeValue;
    private LegalReferral searchLegalReferral;
    private Common searchPromptCdvl;
    private ServiceProvider serviceProvider;
    private Standard standard;
    private Array<PageKeysGroup> pageKeys;
    private Array<GroupGroup> group;
    private Case1 starting;
    private Case1 hiddenStarting;
    private Common maxPages;
    private Common showOnlyArrsCases;
    private LegalReferralAssignment searchLegalReferralAssignment;
    private LegalReferralAssignment hiddenSearchLegalReferralAssignment;
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
      /// A value of PageKeyLegalReferral.
      /// </summary>
      [JsonPropertyName("pageKeyLegalReferral")]
      public LegalReferral PageKeyLegalReferral
      {
        get => pageKeyLegalReferral ??= new();
        set => pageKeyLegalReferral = value;
      }

      /// <summary>
      /// A value of PageKeyCase.
      /// </summary>
      [JsonPropertyName("pageKeyCase")]
      public Case1 PageKeyCase
      {
        get => pageKeyCase ??= new();
        set => pageKeyCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 875;

      private LegalReferral pageKeyLegalReferral;
      private Case1 pageKeyCase;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetailLegalReferralAssignment.
      /// </summary>
      [JsonPropertyName("detailLegalReferralAssignment")]
      public LegalReferralAssignment DetailLegalReferralAssignment
      {
        get => detailLegalReferralAssignment ??= new();
        set => detailLegalReferralAssignment = value;
      }

      /// <summary>
      /// A value of DetailLegalReferral.
      /// </summary>
      [JsonPropertyName("detailLegalReferral")]
      public LegalReferral DetailLegalReferral
      {
        get => detailLegalReferral ??= new();
        set => detailLegalReferral = value;
      }

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
      /// A value of DetailPrgm.
      /// </summary>
      [JsonPropertyName("detailPrgm")]
      public WorkArea DetailPrgm
      {
        get => detailPrgm ??= new();
        set => detailPrgm = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private LegalReferralAssignment detailLegalReferralAssignment;
      private LegalReferral detailLegalReferral;
      private Common detailCommon;
      private CsePersonsWorkSet apDetail;
      private CsePersonsWorkSet arDetail;
      private Case1 detailCase;
      private WorkArea detailPrgm;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// A value of HiddenCalcDone.
    /// </summary>
    [JsonPropertyName("hiddenCalcDone")]
    public Common HiddenCalcDone
    {
      get => hiddenCalcDone ??= new();
      set => hiddenCalcDone = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HiddenSearchAp.
    /// </summary>
    [JsonPropertyName("hiddenSearchAp")]
    public TextWorkArea HiddenSearchAp
    {
      get => hiddenSearchAp ??= new();
      set => hiddenSearchAp = value;
    }

    /// <summary>
    /// A value of HiddenSearchApFi.
    /// </summary>
    [JsonPropertyName("hiddenSearchApFi")]
    public TextWorkArea HiddenSearchApFi
    {
      get => hiddenSearchApFi ??= new();
      set => hiddenSearchApFi = value;
    }

    /// <summary>
    /// A value of HiddenSearchCodeValue.
    /// </summary>
    [JsonPropertyName("hiddenSearchCodeValue")]
    public CodeValue HiddenSearchCodeValue
    {
      get => hiddenSearchCodeValue ??= new();
      set => hiddenSearchCodeValue = value;
    }

    /// <summary>
    /// A value of HiddenSearchLegalReferral.
    /// </summary>
    [JsonPropertyName("hiddenSearchLegalReferral")]
    public LegalReferral HiddenSearchLegalReferral
    {
      get => hiddenSearchLegalReferral ??= new();
      set => hiddenSearchLegalReferral = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of SearchPromptSvpo.
    /// </summary>
    [JsonPropertyName("searchPromptSvpo")]
    public Common SearchPromptSvpo
    {
      get => searchPromptSvpo ??= new();
      set => searchPromptSvpo = value;
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
    /// A value of SearchAp.
    /// </summary>
    [JsonPropertyName("searchAp")]
    public TextWorkArea SearchAp
    {
      get => searchAp ??= new();
      set => searchAp = value;
    }

    /// <summary>
    /// A value of SearchApFi.
    /// </summary>
    [JsonPropertyName("searchApFi")]
    public TextWorkArea SearchApFi
    {
      get => searchApFi ??= new();
      set => searchApFi = value;
    }

    /// <summary>
    /// A value of SearchCodeValue.
    /// </summary>
    [JsonPropertyName("searchCodeValue")]
    public CodeValue SearchCodeValue
    {
      get => searchCodeValue ??= new();
      set => searchCodeValue = value;
    }

    /// <summary>
    /// A value of SearchLegalReferral.
    /// </summary>
    [JsonPropertyName("searchLegalReferral")]
    public LegalReferral SearchLegalReferral
    {
      get => searchLegalReferral ??= new();
      set => searchLegalReferral = value;
    }

    /// <summary>
    /// A value of SearchPromptCdvl.
    /// </summary>
    [JsonPropertyName("searchPromptCdvl")]
    public Common SearchPromptCdvl
    {
      get => searchPromptCdvl ??= new();
      set => searchPromptCdvl = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Case1 Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of MaxPages.
    /// </summary>
    [JsonPropertyName("maxPages")]
    public Common MaxPages
    {
      get => maxPages ??= new();
      set => maxPages = value;
    }

    /// <summary>
    /// A value of ShowOnlyArrsCases.
    /// </summary>
    [JsonPropertyName("showOnlyArrsCases")]
    public Common ShowOnlyArrsCases
    {
      get => showOnlyArrsCases ??= new();
      set => showOnlyArrsCases = value;
    }

    /// <summary>
    /// A value of DlgflwJob.
    /// </summary>
    [JsonPropertyName("dlgflwJob")]
    public Job DlgflwJob
    {
      get => dlgflwJob ??= new();
      set => dlgflwJob = value;
    }

    /// <summary>
    /// A value of DlgflwJobRun.
    /// </summary>
    [JsonPropertyName("dlgflwJobRun")]
    public JobRun DlgflwJobRun
    {
      get => dlgflwJobRun ??= new();
      set => dlgflwJobRun = value;
    }

    /// <summary>
    /// A value of SearchLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("searchLegalReferralAssignment")]
    public LegalReferralAssignment SearchLegalReferralAssignment
    {
      get => searchLegalReferralAssignment ??= new();
      set => searchLegalReferralAssignment = value;
    }

    /// <summary>
    /// A value of HiddenSearchLegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("hiddenSearchLegalReferralAssignment")]
    public LegalReferralAssignment HiddenSearchLegalReferralAssignment
    {
      get => hiddenSearchLegalReferralAssignment ??= new();
      set => hiddenSearchLegalReferralAssignment = value;
    }

    private Common asgnCount;
    private Code code;
    private Common currentPage;
    private Common hiddenCalcDone;
    private Code hiddenCode;
    private CodeValue hiddenCodeValue;
    private NextTranInfo hiddenNextTranInfo;
    private TextWorkArea hiddenSearchAp;
    private TextWorkArea hiddenSearchApFi;
    private CodeValue hiddenSearchCodeValue;
    private LegalReferral hiddenSearchLegalReferral;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private Common searchPromptSvpo;
    private Standard scroll;
    private TextWorkArea searchAp;
    private TextWorkArea searchApFi;
    private CodeValue searchCodeValue;
    private LegalReferral searchLegalReferral;
    private Common searchPromptCdvl;
    private Case1 selected;
    private ServiceProvider serviceProvider;
    private Standard standard;
    private Array<PageKeysGroup> pageKeys;
    private Array<GroupGroup> group;
    private Case1 starting;
    private Case1 hiddenStarting;
    private Common maxPages;
    private Common showOnlyArrsCases;
    private Job dlgflwJob;
    private JobRun dlgflwJobRun;
    private LegalReferralAssignment searchLegalReferralAssignment;
    private LegalReferralAssignment hiddenSearchLegalReferralAssignment;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FirstDayOfMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfMonth")]
    public DateWorkArea FirstDayOfMonth
    {
      get => firstDayOfMonth ??= new();
      set => firstDayOfMonth = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of AsgnCount.
    /// </summary>
    [JsonPropertyName("asgnCount")]
    public Common AsgnCount
    {
      get => asgnCount ??= new();
      set => asgnCount = value;
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
    /// A value of ClearGroupView.
    /// </summary>
    [JsonPropertyName("clearGroupView")]
    public Common ClearGroupView
    {
      get => clearGroupView ??= new();
      set => clearGroupView = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of FiltersHaveChanged.
    /// </summary>
    [JsonPropertyName("filtersHaveChanged")]
    public Common FiltersHaveChanged
    {
      get => filtersHaveChanged ??= new();
      set => filtersHaveChanged = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of OspFound.
    /// </summary>
    [JsonPropertyName("ospFound")]
    public Common OspFound
    {
      get => ospFound ??= new();
      set => ospFound = value;
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
    /// A value of SearchLglRefReasonCode.
    /// </summary>
    [JsonPropertyName("searchLglRefReasonCode")]
    public TextWorkArea SearchLglRefReasonCode
    {
      get => searchLglRefReasonCode ??= new();
      set => searchLglRefReasonCode = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of SvcPrdrIdTxt.
    /// </summary>
    [JsonPropertyName("svcPrdrIdTxt")]
    public WorkArea SvcPrdrIdTxt
    {
      get => svcPrdrIdTxt ??= new();
      set => svcPrdrIdTxt = value;
    }

    /// <summary>
    /// A value of RefDateTxt.
    /// </summary>
    [JsonPropertyName("refDateTxt")]
    public TextWorkArea RefDateTxt
    {
      get => refDateTxt ??= new();
      set => refDateTxt = value;
    }

    /// <summary>
    /// A value of TextMm.
    /// </summary>
    [JsonPropertyName("textMm")]
    public TextWorkArea TextMm
    {
      get => textMm ??= new();
      set => textMm = value;
    }

    /// <summary>
    /// A value of TextDd.
    /// </summary>
    [JsonPropertyName("textDd")]
    public TextWorkArea TextDd
    {
      get => textDd ??= new();
      set => textDd = value;
    }

    /// <summary>
    /// A value of TextYyyy.
    /// </summary>
    [JsonPropertyName("textYyyy")]
    public TextWorkArea TextYyyy
    {
      get => textYyyy ??= new();
      set => textYyyy = value;
    }

    /// <summary>
    /// A value of ArrearsBalance.
    /// </summary>
    [JsonPropertyName("arrearsBalance")]
    public Common ArrearsBalance
    {
      get => arrearsBalance ??= new();
      set => arrearsBalance = value;
    }

    /// <summary>
    /// A value of OfficeIdTxt.
    /// </summary>
    [JsonPropertyName("officeIdTxt")]
    public WorkArea OfficeIdTxt
    {
      get => officeIdTxt ??= new();
      set => officeIdTxt = value;
    }

    /// <summary>
    /// A value of OffSvcPrdrDateTxt.
    /// </summary>
    [JsonPropertyName("offSvcPrdrDateTxt")]
    public TextWorkArea OffSvcPrdrDateTxt
    {
      get => offSvcPrdrDateTxt ??= new();
      set => offSvcPrdrDateTxt = value;
    }

    /// <summary>
    /// A value of ZdelZzapLocalCrookIsTesting.
    /// </summary>
    [JsonPropertyName("zdelZzapLocalCrookIsTesting")]
    public Security2 ZdelZzapLocalCrookIsTesting
    {
      get => zdelZzapLocalCrookIsTesting ??= new();
      set => zdelZzapLocalCrookIsTesting = value;
    }

    private DateWorkArea firstDayOfMonth;
    private Common apCount;
    private CsePersonsWorkSet ap;
    private Common apMatchMade;
    private Common apSearchLength;
    private CsePersonsWorkSet ar;
    private Common arSearchLength;
    private Common asgnCount;
    private Common cdvlReturnCode;
    private Common clearGroupView;
    private Common count;
    private DateWorkArea current;
    private Common filtersHaveChanged;
    private DateWorkArea initialized;
    private DateWorkArea max;
    private Case1 null1;
    private Common ospFound;
    private Program program;
    private TextWorkArea searchLglRefReasonCode;
    private TextWorkArea textWorkArea;
    private WorkArea svcPrdrIdTxt;
    private TextWorkArea refDateTxt;
    private TextWorkArea textMm;
    private TextWorkArea textDd;
    private TextWorkArea textYyyy;
    private Common arrearsBalance;
    private WorkArea officeIdTxt;
    private TextWorkArea offSvcPrdrDateTxt;
    private Security2 zdelZzapLocalCrookIsTesting;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of LegalReferralCaseRole.
    /// </summary>
    [JsonPropertyName("legalReferralCaseRole")]
    public LegalReferralCaseRole LegalReferralCaseRole
    {
      get => legalReferralCaseRole ??= new();
      set => legalReferralCaseRole = value;
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
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePersonAccount ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    private CsePerson keyOnly;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private LegalReferral legalReferral;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferralCaseRole legalReferralCaseRole;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private CsePersonAccount existingObligor;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
  }
#endregion
}
