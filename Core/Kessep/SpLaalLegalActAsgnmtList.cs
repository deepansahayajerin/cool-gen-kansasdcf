// Program: SP_LAAL_LEGAL_ACT_ASGNMT_LIST, ID: 945046516, model: 746.
// Short name: SWELAALP
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
/// A program: SP_LAAL_LEGAL_ACT_ASGNMT_LIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpLaalLegalActAsgnmtList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_LAAL_LEGAL_ACT_ASGNMT_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpLaalLegalActAsgnmtList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpLaalLegalActAsgnmtList.
  /// </summary>
  public SpLaalLegalActAsgnmtList(IContext context, Import import, Export export)
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
    // Date      Developer       Request #  Description
    // -----------------------------------------------------------------------------------
    // 12/03/10  GVandy	  CQ109      Initial development.
    // -----------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------
    // Performance concerns dictated the following approach for this screen.
    // 1) There is a maximum number of legal actions allowed for display.  This 
    // is
    //    controlled by a code table entry (initially set to 1,000 assignments).
    // A
    //    message is displayed to the user if this threshold is exceeded.  They 
    // can
    //    either enter additional search criteria to reduce the number of 
    // assignments
    //    or use F18 PRINT to create the full list (hopefully they use the e-
    // mail
    //    delivery or view online instead of sending the list to a printer).  At
    // the time
    //    this screen was created there were many legal service providers with 
    // more
    //    than 10,000 legal action assignments and at least one with more than
    //    100,000 active legal action assignments.
    // 2) The screen displays 11 legal actions at a time using standard import 
    // and
    //    export groups with 11 occurrences.  There is a second set of hidden 
    // import
    //    and export groups which hold 660 legal actions (i.e. 60 screens worth 
    // of
    //    data).  When the user requests to scroll forward or backward we 
    // utilize
    //    these hidden groups to accomodate the scrolling.  If the user attempts
    // to
    //    scroll beyond the 60 pages then we read the next (or previous, as the 
    // case
    //    may be) 60 pages of data.  Finally, there is an import and export 
    // paging
    //    group which contains the keys of each page of data.  In this situation
    // the
    //    keys are the key of the first occurrence on each page of 660 
    // assignments.
    // 3) We don't do an automatic display on entry to the screen nor do we do 
    // an
    //    automatic display upon return from prompting to select search 
    // criteria.  The
    //    user must explicitly press F2 DISPLAY when they have completed entry 
    // of
    //    the search criteria.
    // -----------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_XFR_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "RETSVPO"))
    {
      // -- Returning from a link to SVPO.  Determine if an OSP was selected on 
      // SVPO by evaluating the
      //   osp role code.  If it is greater than spaces an OSP was selected on 
      // SVPO and returned to this screen.
      // -- On a return from SVPO we want to clear all the search criteria and 
      // group data.
      if (!IsEmpty(import.FromSvpoOfficeServiceProvider.RoleCode))
      {
        MoveOffice(import.FromSvpoOffice, export.Office);
        MoveOfficeServiceProvider(import.FromSvpoOfficeServiceProvider,
          export.OfficeServiceProvider);
        MoveServiceProvider(import.FromSvpoServiceProvider,
          export.ServiceProvider);
      }

      export.SearchPromptSvpo.SelectChar = "";

      var field = GetField(export.SearchPromptSvpo, "selectChar");

      field.Protected = false;
      field.Focused = true;

      return;
    }

    local.Current.Date = Now().Date;

    // --  Move Imports to Exports
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.AsgnCount.Count = import.AsgnCount.Count;
    MoveOffice(import.Office, export.Office);
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.SearchPromptSvpo.SelectChar = import.SearchPromptSvpo.SelectChar;
    MoveOfficeServiceProvider(import.OfficeServiceProvider,
      export.OfficeServiceProvider);
    export.SearchLegalAction.Assign(import.SearchLegalAction);
    export.SearchFipsTribAddress.Assign(import.SearchFipsTribAddress);
    export.SearchTribunalPrompt.SelectChar =
      import.SearchTribunalPrompt.SelectChar;
    export.SearchClassPrompt.SelectChar = import.SearchClassPrompt.SelectChar;
    MoveCodeValue(import.SearchActionTaken, export.SearchActionTaken);
    export.SearchActionTaknPrompt.SelectChar =
      import.SearchActionTaknPrompt.SelectChar;

    if (!IsEmpty(import.SearchCase.Number))
    {
      local.TextWorkArea.Text10 = import.SearchCase.Number;
      UseEabPadLeftWithZeros();
      export.SearchCase.Number = local.TextWorkArea.Text10;
    }

    export.SearchLegalActionAssigment.OverrideInd =
      import.SearchLegalActionAssigment.OverrideInd;
    export.HiddenSearch.Identifier = import.HiddenSearch.Identifier;

    if (!Equal(global.Command, "DISPLAY"))
    {
      export.SearchHiddenFipsTribAddress.Assign(
        import.SearchHiddenFipsTribAddress);
      export.SearchHiddenLegalAction.Assign(import.SearchHiddenLegalAction);
      export.SearchHiddenLegalActionAssigment.OverrideInd =
        import.SearchHiddenLegalActionAssigment.OverrideInd;
      export.SearchHiddenActionTakn.Assign(import.SearchHiddenActionTakn);
      export.SearchHiddenCase.Number = import.SearchHiddenCase.Number;
    }

    export.ScreenNumber.Count = import.ScreenNumber.Count;
    export.MaxScreenNumber.Count = import.MaxScreenNumber.Count;
    export.CurrentPage.Count = import.CurrentPage.Count;
    MoveScrollingAttributes(import.ScrollingAttributes,
      export.ScrollingAttributes);

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

      if (AsChar(import.Group.Item.Common.SelectChar) == '*')
      {
        export.Group.Update.Common.SelectChar = "";
      }

      export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
      export.Group.Update.LegalAction.Assign(import.Group.Item.LegalAction);
      export.Group.Update.LegalActionAssigment.OverrideInd =
        import.Group.Item.LegalActionAssigment.OverrideInd;
      export.Group.Update.SubscriptIn660.Subscript =
        import.Group.Item.SubscriptIn660.Subscript;
    }

    import.Group.CheckIndex();

    // Move import paging group to export paging group
    for(import.PageKeys.Index = 0; import.PageKeys.Index < import
      .PageKeys.Count; ++import.PageKeys.Index)
    {
      if (!import.PageKeys.CheckSize())
      {
        break;
      }

      export.PageKeys.Index = import.PageKeys.Index;
      export.PageKeys.CheckSize();

      export.PageKeys.Update.PageKeyCase.Number =
        import.PageKeys.Item.PageKeyCase.Number;
      export.PageKeys.Update.PageKeyLegalAction.Assign(
        import.PageKeys.Item.PageKeyLegalAction);
    }

    import.PageKeys.CheckIndex();

    for(import.Import660Page.Index = 0; import.Import660Page.Index < import
      .Import660Page.Count; ++import.Import660Page.Index)
    {
      if (!import.Import660Page.CheckSize())
      {
        break;
      }

      export.Export660Page.Index = import.Import660Page.Index;
      export.Export660Page.CheckSize();

      export.Export660Page.Update.Export770Case.Number =
        import.Import660Page.Item.Import660Case.Number;
      export.Export660Page.Update.Export770LegalAction.Identifier =
        import.Import660Page.Item.Import660LegalAction.Identifier;
    }

    import.Import660Page.CheckIndex();

    if (Equal(global.Command, "RETPOPT"))
    {
      return;
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      // -- Returning on a link to CDVL.  Determine if a code value was selected
      // on CDVL by evaluating the
      //    the returned code value.  If it is greater than spaces a value was 
      // selected on CDVL and returned to this screen.
      if (AsChar(import.SearchClassPrompt.SelectChar) == 'S')
      {
        // -- We're returning from a prompt on class
        export.SearchLegalAction.Classification =
          Substring(import.FromCdvl.Cdvalue, 1, 1);
        export.SearchClassPrompt.SelectChar = "";

        var field = GetField(export.SearchClassPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }
      else if (AsChar(import.SearchActionTaknPrompt.SelectChar) == 'S')
      {
        // -- We're returning from a prompt on action taken
        MoveCodeValue(import.FromCdvl, export.SearchActionTaken);
        export.SearchLegalAction.ActionTaken = import.FromCdvl.Cdvalue;
        export.SearchActionTaknPrompt.SelectChar = "";

        var field = GetField(export.SearchActionTaknPrompt, "selectChar");

        field.Protected = false;
        field.Focused = true;
      }

      return;
    }

    if (Equal(global.Command, "RETLTRB"))
    {
      // -- Returning from a link to LTRB.  Determine if a tribunal was selected
      // on LTRB by evaluating the
      //   tribunal id.  If it is not zero then a tribunal was selected on LTRB 
      // and returned to this screen.
      if (import.FromLtrbTribunal.Identifier > 0)
      {
        export.HiddenSearch.Identifier = import.FromLtrbTribunal.Identifier;
        export.SearchFipsTribAddress.County =
          import.FromLtrbFips.CountyAbbreviation ?? "";
        export.SearchFipsTribAddress.State =
          import.FromLtrbFips.StateAbbreviation;
        export.SearchFipsTribAddress.Country =
          import.FromLtrbFipsTribAddress.Country ?? "";
      }
      else
      {
        export.HiddenSearch.Identifier = 0;
        export.SearchFipsTribAddress.County = "";
        export.SearchFipsTribAddress.State = "";
        export.SearchFipsTribAddress.Country = "";
      }

      export.SearchTribunalPrompt.SelectChar = "";

      var field = GetField(export.SearchTribunalPrompt, "selectChar");

      field.Protected = false;
      field.Focused = true;

      if (Equal(import.HiddenPreviousAction.Command, "LTRB"))
      {
        // -- We sent the user to LTRB when they pressed display because more 
        // than one tribunal matched the state/county
        //    entered.  They are now returning from LTRB so continue with the 
        // display.
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "RTFRMLNK"))
    {
      // User is returning on a link from either COMP, LACS, or LGRQ. Views have
      // already been populated.  Escape.
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case 'S':
            export.Group.Update.Common.SelectChar = "*";

            break;
          case ' ':
            break;
          default:
            export.Group.Update.Common.SelectChar = "";

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
    }

    if (Equal(global.Command, "XXNEXTXX") || Equal(global.Command, "FROMMENU"))
    {
      // -- Default service provider to current user.
      if (ReadServiceProvider2())
      {
        export.ServiceProvider.Assign(entities.ServiceProvider);
        local.CsePersonsWorkSet.FirstName = entities.ServiceProvider.FirstName;
        local.CsePersonsWorkSet.LastName = entities.ServiceProvider.LastName;
        local.CsePersonsWorkSet.MiddleInitial =
          entities.ServiceProvider.MiddleInitial;
        UseSiFormatCsePersonName();
        export.ServiceProvider.LastName = local.CsePersonsWorkSet.FormattedName;

        if (ReadOfficeOfficeServiceProvider())
        {
          MoveOfficeServiceProvider(entities.OfficeServiceProvider,
            export.OfficeServiceProvider);
          MoveOffice(entities.Office, export.Office);
        }

        if (!entities.OfficeServiceProvider.Populated)
        {
          ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

          var field1 = GetField(export.OfficeServiceProvider, "effectiveDate");

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

      return;
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
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          local.NextTranInfo.LegalActionIdentifier =
            export.Group.Item.LegalAction.Identifier;
          local.NextTranInfo.StandardCrtOrdNumber =
            export.Group.Item.LegalAction.StandardNumber ?? "";
          local.NextTranInfo.CaseNumber = export.Group.Item.Case1.Number;

          if (ReadLegalAction1())
          {
            local.NextTranInfo.CourtCaseNumber =
              entities.LegalAction.CourtCaseNumber;
            local.NextTranInfo.CourtOrderNumber =
              entities.LegalAction.CourtCaseNumber;
          }

          break;
        }
        else
        {
        }
      }

      export.Group.CheckIndex();
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
    }

    // Group view select processing.
    if (Equal(global.Command, "COMP") || Equal(global.Command, "LACT") || Equal
      (global.Command, "ASIN") || Equal(global.Command, "LGRQ") || Equal
      (global.Command, "IWGL"))
    {
      local.Count.Count = 0;

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case 'S':
            ++local.Count.Count;
            export.SelectedCase.Number = export.Group.Item.Case1.Number;
            MoveLegalAction(export.Group.Item.LegalAction,
              export.SelectedLegalAction);

            if (Equal(global.Command, "IWGL"))
            {
              if (AsChar(export.Group.Item.LegalAction.Classification) != 'I'
                && AsChar(export.Group.Item.LegalAction.Classification) != 'G')
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "LE0000_I_OR_G_CLASS_FOR_IWGL";
              }

              export.IwglType.Text1 =
                export.Group.Item.LegalAction.Classification;
            }

            break;
          case ' ':
            break;
          case '*':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Color = "red";
            field.Protected = false;
            field.Focused = true;

            return;
        }
      }

      export.Group.CheckIndex();

      if (local.Count.Count > 1)
      {
        export.SelectedCase.Number = "";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;
          }
        }

        export.Group.CheckIndex();
        ExitState = "MULTIPLE_SELECTION_NOT_ALLOWED";

        return;
      }

      if (local.Count.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
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
      // -- "S" is the only valid prompt character.
      switch(AsChar(export.SearchActionTaknPrompt.SelectChar))
      {
        case 'S':
          ++local.Prompt.Count;

          break;
        case ' ':
          break;
        case '+':
          break;
        default:
          var field = GetField(export.SearchActionTaknPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }

      switch(AsChar(export.SearchClassPrompt.SelectChar))
      {
        case 'S':
          ++local.Prompt.Count;

          break;
        case ' ':
          break;
        case '+':
          break;
        default:
          var field = GetField(export.SearchClassPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }

      switch(AsChar(export.SearchTribunalPrompt.SelectChar))
      {
        case 'S':
          ++local.Prompt.Count;

          break;
        case ' ':
          break;
        case '+':
          break;
        default:
          var field = GetField(export.SearchTribunalPrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }

      switch(AsChar(export.SearchPromptSvpo.SelectChar))
      {
        case 'S':
          ++local.Prompt.Count;

          break;
        case ' ':
          break;
        case '+':
          break;
        default:
          var field = GetField(export.SearchPromptSvpo, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // -- Only one prompt can be selected
      if (local.Prompt.Count > 1)
      {
        if (AsChar(export.SearchActionTaknPrompt.SelectChar) == 'S')
        {
          var field = GetField(export.SearchActionTaknPrompt, "selectChar");

          field.Error = true;
        }

        if (AsChar(export.SearchClassPrompt.SelectChar) == 'S')
        {
          var field = GetField(export.SearchClassPrompt, "selectChar");

          field.Error = true;
        }

        if (AsChar(export.SearchTribunalPrompt.SelectChar) == 'S')
        {
          var field = GetField(export.SearchTribunalPrompt, "selectChar");

          field.Error = true;
        }

        if (AsChar(export.SearchPromptSvpo.SelectChar) == 'S')
        {
          var field = GetField(export.SearchPromptSvpo, "selectChar");

          field.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

        return;
      }

      if (AsChar(export.SearchActionTaknPrompt.SelectChar) == 'S')
      {
        export.ToCdvlCode.CodeName = "ACTION TAKEN";

        if (!IsEmpty(export.SearchLegalAction.Classification))
        {
          export.ToCdvlValidWithCode.CodeName = "LEGAL ACTION CLASSIFICATION";
          export.ToCdvlValidWithCodeValue.Cdvalue =
            export.SearchLegalAction.Classification;
        }

        ExitState = "ECO_LNK_TO_CDVL";

        return;
      }

      if (AsChar(export.SearchClassPrompt.SelectChar) == 'S')
      {
        export.ToCdvlCode.CodeName = "LEGAL ACTION CLASSIFICATION";
        ExitState = "ECO_LNK_TO_CDVL";

        return;
      }

      if (AsChar(export.SearchTribunalPrompt.SelectChar) == 'S')
      {
        export.ToLtrb.StateAbbreviation = import.SearchFipsTribAddress.State;
        ExitState = "ECO_LNK_TO_LTRB";

        return;
      }

      if (AsChar(export.SearchPromptSvpo.SelectChar) == 'S')
      {
        ExitState = "ECO_LNK_TO_SVPO";

        return;
      }

      // At this point we know that no acceptable prompt was entered.
      var field1 = GetField(export.SearchActionTaknPrompt, "selectChar");

      field1.Error = true;

      var field2 = GetField(export.SearchClassPrompt, "selectChar");

      field2.Error = true;

      var field3 = GetField(export.SearchTribunalPrompt, "selectChar");

      field3.Error = true;

      var field4 = GetField(export.SearchPromptSvpo, "selectChar");

      field4.Error = true;

      ExitState = "ACO_NE0000_NO_SELECTION_MADE";

      return;
    }

    if (!Equal(global.Command, "DISPLAY") && !Equal(global.Command, "PRINT"))
    {
      // -- Check Filters for Change
      if (AsChar(import.SearchLegalActionAssigment.OverrideInd) != AsChar
        (export.SearchHiddenLegalActionAssigment.OverrideInd))
      {
        var field = GetField(export.SearchLegalActionAssigment, "overrideInd");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!Equal(export.SearchCase.Number, import.SearchHiddenCase.Number))
      {
        var field = GetField(export.SearchCase, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!Equal(import.SearchLegalAction.FiledDate,
        import.SearchHiddenLegalAction.FiledDate))
      {
        var field = GetField(export.SearchLegalAction, "filedDate");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!Equal(import.SearchActionTaken.Cdvalue,
        import.SearchHiddenActionTakn.Cdvalue))
      {
        var field = GetField(export.SearchActionTaken, "description");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!Equal(import.SearchLegalAction.CreatedTstamp,
        import.SearchHiddenLegalAction.CreatedTstamp))
      {
        var field = GetField(export.SearchLegalAction, "createdTstamp");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (AsChar(import.SearchLegalAction.Classification) != AsChar
        (import.SearchHiddenLegalAction.Classification))
      {
        var field = GetField(export.SearchLegalAction, "classification");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!Equal(import.SearchFipsTribAddress.State,
        import.SearchHiddenFipsTribAddress.State))
      {
        var field = GetField(export.SearchFipsTribAddress, "state");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!Equal(import.SearchFipsTribAddress.County,
        import.SearchHiddenFipsTribAddress.County))
      {
        var field = GetField(export.SearchFipsTribAddress, "county");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!Equal(import.SearchFipsTribAddress.Country,
        import.SearchHiddenFipsTribAddress.Country))
      {
        var field1 = GetField(export.SearchFipsTribAddress, "county");

        field1.Error = true;

        var field2 = GetField(export.SearchFipsTribAddress, "state");

        field2.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";
      }

      if (!Equal(import.SearchLegalAction.StandardNumber,
        import.SearchHiddenLegalAction.StandardNumber))
      {
        var field = GetField(export.SearchLegalAction, "standardNumber");

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
      case "DISPLAY":
        // Display processing at bottom of Procedure step.
        export.PageKeys.Count = 0;
        export.CurrentPage.Count = 1;
        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "";
        export.ScreenNumber.Count = 0;
        export.MaxScreenNumber.Count = 0;

        break;
      case "PRINT":
        // Print processing at bottom of Procedure step.
        break;
      case "PREV":
        if (export.CurrentPage.Count == 0)
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        // --  Test if previous page of data exists
        if (AsChar(export.ScrollingAttributes.MinusFlag) != '-')
        {
          ExitState = "ACO_NI0000_TOP_OF_LIST";

          return;
        }

        --export.ScreenNumber.Count;
        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "+";

        // -- Each record in the display group of 11 occurrences contains the 
        // index
        //    where the same record resides within the group of 660 records.  
        // That is
        //    what the grp_export_subscript_in_660 view represents in the 
        // following
        //    statements.
        //    Using the 1st record in the export group of 11 records, we set the
        // subcript
        //    of the export 660 group to be 11 less than the 1st records 
        // corresponding
        //    660 group subscript.  That is the starting subscript within the 
        // group of 660
        //    for the previous screen of data.  However, if this sets the 
        // subscript to a
        //    negative number then we know we need to read the previous 660 rows
        // of
        //    data.
        export.Group.Index = 0;
        export.Group.CheckSize();

        export.Export660Page.Index =
          export.Group.Item.SubscriptIn660.Subscript - 12;
        export.Export660Page.CheckSize();

        if (export.Export660Page.Index < 0)
        {
          // -- We're moving beyond the bounds of the 60 hidden pages of data.  
          // Decrement the current
          //    page number and fall through to the display logic at the bottom 
          // of the pstep to read the previous group
          //    of 660 records.
          --export.CurrentPage.Count;
          export.ScrollingAttributes.MinusFlag = "-";

          break;
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        // -- Populate the previous screen of data using the hidden group of 660
        // records.
        export.Group.Index = -1;
        export.Group.Count = 0;

        if (export.Export660Page.Index != 0 || export.CurrentPage.Count != 1)
        {
          export.ScrollingAttributes.MinusFlag = "-";
        }

        export.Export660Page.Index = export.Export660Page.Index;

        for(var limit = export.Export660Page.Count; export
          .Export660Page.Index < limit; ++export.Export660Page.Index)
        {
          if (!export.Export660Page.CheckSize())
          {
            break;
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          if (export.Group.Index >= Export.GroupGroup.Capacity)
          {
            // -- There is more data beyond the 11 rows being displayed to the 
            // user.  Set the plus scrolling indicator.
            export.ScrollingAttributes.PlusFlag = "+";

            break;
          }

          export.Group.Update.SubscriptIn660.Subscript =
            export.Export660Page.Index + 1;
          export.Group.Update.Case1.Number =
            export.Export660Page.Item.Export770Case.Number;

          if (ReadLegalAction2())
          {
            export.Group.Update.LegalAction.Assign(entities.LegalAction);
            UseLeGetActionTakenDescription();
            export.Group.Update.LegalAction.ActionTaken =
              local.CodeValue.Description;
          }
          else
          {
            export.Group.Update.LegalAction.Assign(local.NullLegalAction);
            export.Group.Update.LegalActionAssigment.OverrideInd = "";
            ExitState = "LEGAL_ACTION_NF";

            continue;
          }

          if (ReadLegalActionAssigment())
          {
            export.Group.Update.LegalActionAssigment.OverrideInd =
              entities.LegalActionAssigment.OverrideInd;
          }
          else
          {
            ExitState = "LEGAL_ACTION_ASSIGNMENT_NF";
          }
        }

        export.Export660Page.CheckIndex();

        return;
      case "NEXT":
        if (export.CurrentPage.Count == 0)
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        // --  Test if next page of data exists
        if (AsChar(export.ScrollingAttributes.PlusFlag) != '+' || export
          .Group.Count != Export.GroupGroup.Capacity)
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

          return;
        }

        ++export.ScreenNumber.Count;
        export.ScrollingAttributes.MinusFlag = "-";
        export.ScrollingAttributes.PlusFlag = "";

        // -- Each record in the display group of 11 occurrences contains the 
        // index
        //    where the same record resides within the group of 660 records.  
        // That is
        //    what the grp_export_subscript_in_660 view represents in the 
        // following
        //    statements.
        //    Using the last record in the export group of 11 records, we set 
        // the subcript
        //    of the export 660 group to be 1 more than the last records 
        // corresponding
        //    660 group subscript.  That is the starting subscript within the 
        // group of 660
        //    for the next screen of data.  However, if this sets the subscript 
        // to a
        //    number greater than 660 then we know we need to read the next 660 
        // rows
        //    of data.
        export.Group.Index = Export.GroupGroup.Capacity - 1;
        export.Group.CheckSize();

        export.Export660Page.Index = export.Group.Item.SubscriptIn660.Subscript;
        export.Export660Page.CheckSize();

        if (export.Export660Page.Index + 1 >= Export
          .Export660PageGroup.Capacity)
        {
          // -- We're moving beyond the bounds of the 60 hidden pages of data.  
          // Increment the current
          //    page number and fall through to the display logic at the bottom 
          // of the pstep to read the next group
          //    of 660 records.
          ++export.CurrentPage.Count;

          break;
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        // -- Populate the next screen of data using the hidden group of 660 
        // records.
        export.Group.Index = -1;
        export.Group.Count = 0;
        export.Export660Page.Index = export.Export660Page.Index;

        for(var limit = export.Export660Page.Count; export
          .Export660Page.Index < limit; ++export.Export660Page.Index)
        {
          if (!export.Export660Page.CheckSize())
          {
            break;
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          if (export.Group.Index >= Export.GroupGroup.Capacity)
          {
            // -- There is more data beyond the 11 rows being displayed to the 
            // user.  Set the plus scrolling indicator.
            export.ScrollingAttributes.PlusFlag = "+";

            break;
          }

          export.Group.Update.SubscriptIn660.Subscript =
            export.Export660Page.Index + 1;
          export.Group.Update.Case1.Number =
            export.Export660Page.Item.Export770Case.Number;

          if (ReadLegalAction2())
          {
            export.Group.Update.LegalAction.Assign(entities.LegalAction);
            UseLeGetActionTakenDescription();
            export.Group.Update.LegalAction.ActionTaken =
              local.CodeValue.Description;
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";
            export.Group.Update.LegalAction.Assign(local.NullLegalAction);
            export.Group.Update.LegalActionAssigment.OverrideInd = "";

            continue;
          }

          if (ReadLegalActionAssigment())
          {
            export.Group.Update.LegalActionAssigment.OverrideInd =
              entities.LegalActionAssigment.OverrideInd;
          }
          else
          {
            ExitState = "LEGAL_ACTION_ASSIGNMENT_NF";
          }
        }

        export.Export660Page.CheckIndex();

        if (export.PageKeys.Count > export.CurrentPage.Count)
        {
          // -- This sets the plus scrolling indicator in the situation where 
          // we've moved the last rows of data from
          //    the 660 group to the screen display but there are more rows of 
          // data that can be read from the tables.
          export.ScrollingAttributes.PlusFlag = "+";
        }

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "LACT":
        ExitState = "ECO_LNK_TO_LEGAL_ACTION";

        return;
      case "ASIN":
        export.DlgflwAsinHeaderObject.Text20 = "LEGAL ACTION";
        ExitState = "ECO_LNK_TO_ASIN";

        return;
      case "IWGL":
        ExitState = "ECO_LNK_TO_IWGL";

        return;
      case "COMP":
        ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

        return;
      case "LGRQ":
        ExitState = "ECO_LNK_TO_LEGAL_REQUEST";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PRINT") || Equal
      (global.Command, "NEXT") || Equal(global.Command, "PREV"))
    {
      if (IsEmpty(export.OfficeServiceProvider.RoleCode) && Equal
        (export.OfficeServiceProvider.EffectiveDate, local.Initialized.Date) &&
        export.ServiceProvider.SystemGeneratedId == 0 && export
        .Office.SystemGeneratedId == 0)
      {
        // -- This will occur when the user has performed a Clear and then 
        // presses PF2 Display.  Determine
        //     the logged on user and derive the first occurrence of Office 
        // Service Provider for this user, ordered by Office.
        if (ReadServiceProvider2())
        {
          export.ServiceProvider.Assign(entities.ServiceProvider);
          local.CsePersonsWorkSet.FirstName =
            entities.ServiceProvider.FirstName;
          local.CsePersonsWorkSet.LastName = entities.ServiceProvider.LastName;
          local.CsePersonsWorkSet.MiddleInitial =
            entities.ServiceProvider.MiddleInitial;
          UseSiFormatCsePersonName();
          export.ServiceProvider.LastName =
            local.CsePersonsWorkSet.FormattedName;

          if (ReadOfficeOfficeServiceProvider())
          {
            MoveOfficeServiceProvider(entities.OfficeServiceProvider,
              export.OfficeServiceProvider);
            MoveOffice(entities.Office, export.Office);
          }

          if (!entities.OfficeServiceProvider.Populated)
          {
            ExitState = "SP0000_OFFICE_SERVICE_PROVIDR_NF";

            var field1 =
              GetField(export.OfficeServiceProvider, "effectiveDate");

            field1.Error = true;

            var field2 = GetField(export.OfficeServiceProvider, "roleCode");

            field2.Error = true;

            goto Test1;
          }
        }
        else
        {
          ExitState = "SERVICE_PROVIDER_NF";

          var field = GetField(export.ServiceProvider, "systemGeneratedId");

          field.Error = true;

          goto Test1;
        }
      }
      else
      {
        if (export.Office.SystemGeneratedId == 0)
        {
          var field = GetField(export.Office, "systemGeneratedId");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          goto Test1;
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

          goto Test1;
        }

        if (export.ServiceProvider.SystemGeneratedId == 0)
        {
          var field = GetField(export.ServiceProvider, "systemGeneratedId");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          goto Test1;
        }

        if (ReadServiceProvider1())
        {
          export.ServiceProvider.Assign(entities.ServiceProvider);
          local.CsePersonsWorkSet.FirstName =
            entities.ServiceProvider.FirstName;
          local.CsePersonsWorkSet.LastName = entities.ServiceProvider.LastName;
          local.CsePersonsWorkSet.MiddleInitial =
            entities.ServiceProvider.MiddleInitial;
          UseSiFormatCsePersonName();
          export.ServiceProvider.LastName =
            local.CsePersonsWorkSet.FormattedName;
        }
        else
        {
          ExitState = "SERVICE_PROVIDER_NF";

          var field = GetField(export.ServiceProvider, "systemGeneratedId");

          field.Error = true;

          export.ServiceProvider.LastName = "";

          goto Test1;
        }

        if (Equal(export.OfficeServiceProvider.EffectiveDate,
          local.Initialized.Date))
        {
          var field = GetField(export.OfficeServiceProvider, "effectiveDate");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          goto Test1;
        }

        if (IsEmpty(export.OfficeServiceProvider.RoleCode))
        {
          var field = GetField(export.OfficeServiceProvider, "roleCode");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          goto Test1;
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

          goto Test1;
        }
      }

      // -- Validate the tribunal.  Flow to LTRB if not unique (like LACT does).
      if (Equal(import.HiddenPreviousAction.Command, "LTRB"))
      {
        // -- On the last execution, we sent the user to LTRB to select a unique
        // tribunal based on the entered state and county.
        //    They either selected a tribunal or returned without selecting one,
        // which blanks out the tribunal info.  No need to do any validation.
      }
      else
      {
        // -- Validate that state and county abbreviations are valid tribunals.
        if (!IsEmpty(export.SearchFipsTribAddress.State) && !
          IsEmpty(export.SearchFipsTribAddress.County))
        {
          // -- Ignore court trustees (fips location code 12) when determining 
          // how many tribunals match the entered state and county.
          foreach(var item in ReadTribunalFips())
          {
            ++local.TribunalCount.Count;
            local.Tribunal.Identifier = entities.Tribunal.Identifier;
            MoveFips(entities.Fips, local.Fips);

            if (entities.Tribunal.Identifier == import.HiddenSearch.Identifier)
            {
              // -- User was most recently working with this tribunal.  Don't 
              // make them go back to LTRB to pick again since they haven't
              // switched from the tribunal they originally picked.
              local.TribunalCount.Count = 1;

              break;
            }
          }

          switch(local.TribunalCount.Count)
          {
            case 0:
              export.HiddenSearch.Identifier = 0;

              var field1 = GetField(export.SearchFipsTribAddress, "county");

              field1.Error = true;

              var field2 = GetField(export.SearchFipsTribAddress, "state");

              field2.Error = true;

              if (ReadFips())
              {
                ExitState = "LE0000_TRIBUNAL_NF";
              }
              else
              {
                ExitState = "SI0000_TRIB_FIPS_NF_RB";
              }

              goto Test1;
            case 1:
              export.HiddenSearch.Identifier = local.Tribunal.Identifier;
              export.SearchFipsTribAddress.Country = "";

              break;
            default:
              export.HiddenSearch.Identifier = 0;
              export.HiddenPreviousAction.Command = "LTRB";
              MoveFips(local.Fips, export.ToLtrb);
              ExitState = "ECO_LNK_TO_LTRB";

              return;
          }
        }
        else
        {
          if (!IsEmpty(export.SearchFipsTribAddress.State) && !
            IsEmpty(export.SearchFipsTribAddress.Country))
          {
            var field1 = GetField(export.SearchFipsTribAddress, "country");

            field1.Error = true;

            var field2 = GetField(export.SearchFipsTribAddress, "state");

            field2.Error = true;

            ExitState = "EITHER_STATE_OR_CNTRY_CODE";
          }

          if (!IsEmpty(export.SearchFipsTribAddress.State) && IsEmpty
            (export.SearchFipsTribAddress.County))
          {
            var field = GetField(export.SearchFipsTribAddress, "county");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }

          if (IsEmpty(export.SearchFipsTribAddress.State) && !
            IsEmpty(export.SearchFipsTribAddress.County))
          {
            var field = GetField(export.SearchFipsTribAddress, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test1;
          }

          if (IsEmpty(export.SearchFipsTribAddress.State) && IsEmpty
            (export.SearchFipsTribAddress.County) && IsEmpty
            (export.SearchFipsTribAddress.Country))
          {
            export.HiddenSearch.Identifier = 0;
          }
        }
      }

      // -- Validate classification.
      if (!IsEmpty(export.SearchLegalAction.Classification))
      {
        export.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
        local.CodeValue.Cdvalue = export.SearchLegalAction.Classification;
        UseCabValidateCodeValue();

        if (AsChar(local.CdvlReturnCode.Flag) != 'Y')
        {
          var field = GetField(export.SearchLegalAction, "classification");

          field.Error = true;

          ExitState = "LE0000_INVALID_CLASSIFICATION";

          goto Test1;
        }

        // -- Validate that the search classification corresponds to the search 
        // action taken.
        if (!IsEmpty(export.SearchLegalAction.ActionTaken))
        {
          UseLeCabGetClassForActTaken();

          if (AsChar(export.SearchLegalAction.Classification) != AsChar
            (local.LegalAction.Classification))
          {
            var field1 = GetField(export.SearchLegalAction, "classification");

            field1.Error = true;

            var field2 = GetField(export.SearchActionTaken, "description");

            field2.Error = true;

            ExitState = "LE0000_CLASS_AND_ACTION_MISMATCH";

            goto Test1;
          }
        }
      }

      // -- Validate the search override indicator.
      switch(AsChar(export.SearchLegalActionAssigment.OverrideInd))
      {
        case 'Y':
          break;
        case 'N':
          break;
        case ' ':
          break;
        default:
          var field =
            GetField(export.SearchLegalActionAssigment, "overrideInd");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_Y_OR_N";

          break;
      }
    }

Test1:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- A validation error occurred.  Blank out the display group, set the 
      // screen counts and assignment counts to zero.
      export.CurrentPage.Count = 0;
      export.Group.Count = 0;
      export.Export660Page.Count = 0;
      export.PageKeys.Count = 0;
      export.SearchHiddenLegalAction.Assign(local.NullLegalAction);
      export.SearchHiddenCase.Number = local.NullCase.Number;
      export.SearchHiddenFipsTribAddress.Assign(local.NullFipsTribAddress);
      MoveCodeValue(local.NullCodeValue, export.SearchHiddenActionTakn);
      export.SearchHiddenLegalActionAssigment.OverrideInd = "";
      export.ScrollingAttributes.MinusFlag = "";
      export.ScrollingAttributes.PlusFlag = "";
      export.ScreenNumber.Count = 0;
      export.MaxScreenNumber.Count = 0;
      export.AsgnCount.Count = 0;

      return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      // -- Read the code table to determine the maximum number of legal action 
      // assignments for which the
      //    user can display on LAAL.  This was done to enhance performance 
      // since some legal service providers
      //    have in excess of 100,000 active legal action assignments.
      if (ReadCodeValue())
      {
        local.MaxAssignmentsToDisplay.Count =
          (int)StringToNumber(TrimEnd(entities.CodeValue.Cdvalue));
      }
      else
      {
        local.MaxAssignmentsToDisplay.Count = 15000;
      }

      // -- Gather data for display.
      export.Group.Index = -1;
      export.Group.Count = 0;

      export.PageKeys.Index = export.CurrentPage.Count - 1;
      export.PageKeys.CheckSize();

      if (Equal(global.Command, "DISPLAY"))
      {
        export.AsgnCount.Count = 0;
      }

      local.Group.Index = -1;
      local.Group.Count = 0;

      // -- Read eachs are separated depending upon the search criteria entered 
      // to aid performance.
      //    These reads simply gather the raw data.  The data will be sorted 
      // later.
      if (IsEmpty(export.SearchCase.Number) && IsEmpty
        (export.SearchLegalAction.StandardNumber))
      {
        foreach(var item in ReadLegalActionLegalActionAssigment2())
        {
          if (export.HiddenSearch.Identifier != 0)
          {
            if (!ReadTribunal())
            {
              continue;
            }
          }

          if (Equal(global.Command, "DISPLAY"))
          {
            ++export.AsgnCount.Count;

            if (export.AsgnCount.Count > local.MaxAssignmentsToDisplay.Count)
            {
              // -- We exceeded the maximum number of legal action assignments 
              // permitted for display.
              //    Continue to count the number of assignments that meet the 
              // search criteria.  This number will be displayed to the user.
              continue;
            }
          }

          local.CaseFound.Flag = "N";

          foreach(var item1 in ReadCase())
          {
            if (Equal(global.Command, "NEXT") || Equal(global.Command, "PREV"))
            {
              // -- This logic excludes data from previous pages.
              if (export.CurrentPage.Count == 1)
              {
                goto Test2;
              }

              if (Lt(entities.Case1.Number,
                export.PageKeys.Item.PageKeyCase.Number) || Equal
                (entities.Case1.Number, export.PageKeys.Item.PageKeyCase.Number) &&
                Lt
                (entities.LegalAction.StandardNumber,
                export.PageKeys.Item.PageKeyLegalAction.StandardNumber) || Equal
                (entities.Case1.Number, export.PageKeys.Item.PageKeyCase.Number) &&
                Equal
                (entities.LegalAction.StandardNumber,
                export.PageKeys.Item.PageKeyLegalAction.StandardNumber) && Lt
                (export.PageKeys.Item.PageKeyLegalAction.CreatedTstamp,
                entities.LegalAction.CreatedTstamp) || Equal
                (entities.Case1.Number, export.PageKeys.Item.PageKeyCase.Number) &&
                Equal
                (entities.LegalAction.StandardNumber,
                export.PageKeys.Item.PageKeyLegalAction.StandardNumber) && Equal
                (entities.LegalAction.CreatedTstamp,
                export.PageKeys.Item.PageKeyLegalAction.CreatedTstamp) && entities
                .LegalAction.Identifier < export
                .PageKeys.Item.PageKeyLegalAction.Identifier)
              {
                continue;
              }
            }

Test2:

            local.CaseFound.Flag = "Y";

            ++local.Group.Index;
            local.Group.CheckSize();

            local.Group.Update.Case1.Number = entities.Case1.Number;
            local.Group.Update.LegalActionAssigment.OverrideInd =
              entities.LegalActionAssigment.OverrideInd;
            local.Group.Update.LegalAction.Assign(entities.LegalAction);
          }

          if (AsChar(local.CaseFound.Flag) == 'N')
          {
            // -- If there are no cases associated to the legal action we still 
            // need to display the legal action info without a case number.
            if (Equal(global.Command, "NEXT") || Equal(global.Command, "PREV"))
            {
              // -- This logic excludes data from previous pages.
              if (export.CurrentPage.Count == 1)
              {
                goto Test3;
              }

              if (!IsEmpty(export.PageKeys.Item.PageKeyCase.Number) || IsEmpty
                (export.PageKeys.Item.PageKeyCase.Number) && Lt
                (entities.LegalAction.StandardNumber,
                export.PageKeys.Item.PageKeyLegalAction.StandardNumber) || IsEmpty
                (export.PageKeys.Item.PageKeyCase.Number) && Equal
                (entities.LegalAction.StandardNumber,
                export.PageKeys.Item.PageKeyLegalAction.StandardNumber) && Lt
                (export.PageKeys.Item.PageKeyLegalAction.CreatedTstamp,
                entities.LegalAction.CreatedTstamp) || IsEmpty
                (export.PageKeys.Item.PageKeyCase.Number) && Equal
                (entities.LegalAction.StandardNumber,
                export.PageKeys.Item.PageKeyLegalAction.StandardNumber) && Equal
                (entities.LegalAction.CreatedTstamp,
                export.PageKeys.Item.PageKeyLegalAction.CreatedTstamp) && entities
                .LegalAction.Identifier < export
                .PageKeys.Item.PageKeyLegalAction.Identifier)
              {
                continue;
              }
            }

Test3:

            ++local.Group.Index;
            local.Group.CheckSize();

            local.Group.Update.Case1.Number = local.NullCase.Number;
            local.Group.Update.LegalActionAssigment.OverrideInd =
              entities.LegalActionAssigment.OverrideInd;
            local.Group.Update.LegalAction.Assign(entities.LegalAction);
          }
        }
      }
      else if (!IsEmpty(export.SearchCase.Number))
      {
        foreach(var item in ReadCaseLegalActionLegalActionAssigment())
        {
          if (export.HiddenSearch.Identifier != 0)
          {
            if (!ReadTribunal())
            {
              continue;
            }
          }

          if (Equal(global.Command, "DISPLAY"))
          {
            ++export.AsgnCount.Count;

            if (export.AsgnCount.Count > local.MaxAssignmentsToDisplay.Count)
            {
              // -- We exceeded the maximum number of legal action assignments 
              // permitted for display.
              //    Continue to count the number of assignments that meet the 
              // search criteria.  This number will be displayed to the user.
              continue;
            }
          }

          if (Equal(global.Command, "NEXT") || Equal(global.Command, "PREV"))
          {
            // -- This logic excludes data from previous pages.
            if (export.CurrentPage.Count == 1)
            {
              goto Test4;
            }

            if (Lt(entities.Case1.Number,
              export.PageKeys.Item.PageKeyCase.Number) || Equal
              (entities.Case1.Number, export.PageKeys.Item.PageKeyCase.Number) &&
              Lt
              (entities.LegalAction.StandardNumber,
              export.PageKeys.Item.PageKeyLegalAction.StandardNumber) || Equal
              (entities.Case1.Number, export.PageKeys.Item.PageKeyCase.Number) &&
              Equal
              (entities.LegalAction.StandardNumber,
              export.PageKeys.Item.PageKeyLegalAction.StandardNumber) && Lt
              (export.PageKeys.Item.PageKeyLegalAction.CreatedTstamp,
              entities.LegalAction.CreatedTstamp) || Equal
              (entities.Case1.Number, export.PageKeys.Item.PageKeyCase.Number) &&
              Equal
              (entities.LegalAction.StandardNumber,
              export.PageKeys.Item.PageKeyLegalAction.StandardNumber) && Equal
              (entities.LegalAction.CreatedTstamp,
              export.PageKeys.Item.PageKeyLegalAction.CreatedTstamp) && entities
              .LegalAction.Identifier < export
              .PageKeys.Item.PageKeyLegalAction.Identifier)
            {
              continue;
            }
          }

Test4:

          local.CaseFound.Flag = "Y";

          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.Case1.Number = entities.Case1.Number;
          local.Group.Update.LegalActionAssigment.OverrideInd =
            entities.LegalActionAssigment.OverrideInd;
          local.Group.Update.LegalAction.Assign(entities.LegalAction);
        }
      }
      else if (!IsEmpty(export.SearchLegalAction.StandardNumber))
      {
        foreach(var item in ReadLegalActionLegalActionAssigment1())
        {
          if (export.HiddenSearch.Identifier != 0)
          {
            if (!ReadTribunal())
            {
              continue;
            }
          }

          if (Equal(global.Command, "DISPLAY"))
          {
            ++export.AsgnCount.Count;

            if (export.AsgnCount.Count > local.MaxAssignmentsToDisplay.Count)
            {
              // -- We exceeded the maximum number of legal action assignments 
              // permitted for display.
              //    Continue to count the number of assignments that meet the 
              // search criteria.  This number will be displayed to the user.
              continue;
            }
          }

          local.CaseFound.Flag = "N";

          foreach(var item1 in ReadCase())
          {
            if (Equal(global.Command, "NEXT") || Equal(global.Command, "PREV"))
            {
              // -- This logic excludes data from previous pages.
              if (export.CurrentPage.Count == 1)
              {
                goto Test5;
              }

              if (Lt(entities.Case1.Number,
                export.PageKeys.Item.PageKeyCase.Number) || Equal
                (entities.Case1.Number, export.PageKeys.Item.PageKeyCase.Number) &&
                Lt
                (entities.LegalAction.StandardNumber,
                export.PageKeys.Item.PageKeyLegalAction.StandardNumber) || Equal
                (entities.Case1.Number, export.PageKeys.Item.PageKeyCase.Number) &&
                Equal
                (entities.LegalAction.StandardNumber,
                export.PageKeys.Item.PageKeyLegalAction.StandardNumber) && Lt
                (export.PageKeys.Item.PageKeyLegalAction.CreatedTstamp,
                entities.LegalAction.CreatedTstamp) || Equal
                (entities.Case1.Number, export.PageKeys.Item.PageKeyCase.Number) &&
                Equal
                (entities.LegalAction.StandardNumber,
                export.PageKeys.Item.PageKeyLegalAction.StandardNumber) && Equal
                (entities.LegalAction.CreatedTstamp,
                export.PageKeys.Item.PageKeyLegalAction.CreatedTstamp) && entities
                .LegalAction.Identifier < export
                .PageKeys.Item.PageKeyLegalAction.Identifier)
              {
                continue;
              }
            }

Test5:

            local.CaseFound.Flag = "Y";

            ++local.Group.Index;
            local.Group.CheckSize();

            local.Group.Update.Case1.Number = entities.Case1.Number;
            local.Group.Update.LegalActionAssigment.OverrideInd =
              entities.LegalActionAssigment.OverrideInd;
            local.Group.Update.LegalAction.Assign(entities.LegalAction);
          }

          if (AsChar(local.CaseFound.Flag) == 'N')
          {
            // -- If there are no cases associated to the legal action we still 
            // need to display the legal action info without a case number.
            if (Equal(global.Command, "NEXT") || Equal(global.Command, "PREV"))
            {
              // -- This logic excludes data from previous pages.
              if (export.CurrentPage.Count == 1)
              {
                goto Test6;
              }

              if (!IsEmpty(export.PageKeys.Item.PageKeyCase.Number) || IsEmpty
                (export.PageKeys.Item.PageKeyCase.Number) && Lt
                (entities.LegalAction.StandardNumber,
                export.PageKeys.Item.PageKeyLegalAction.StandardNumber) || IsEmpty
                (export.PageKeys.Item.PageKeyCase.Number) && Equal
                (entities.LegalAction.StandardNumber,
                export.PageKeys.Item.PageKeyLegalAction.StandardNumber) && Lt
                (export.PageKeys.Item.PageKeyLegalAction.CreatedTstamp,
                entities.LegalAction.CreatedTstamp) || IsEmpty
                (export.PageKeys.Item.PageKeyCase.Number) && Equal
                (entities.LegalAction.StandardNumber,
                export.PageKeys.Item.PageKeyLegalAction.StandardNumber) && Equal
                (entities.LegalAction.CreatedTstamp,
                export.PageKeys.Item.PageKeyLegalAction.CreatedTstamp) && entities
                .LegalAction.Identifier < export
                .PageKeys.Item.PageKeyLegalAction.Identifier)
              {
                continue;
              }
            }

Test6:

            ++local.Group.Index;
            local.Group.CheckSize();

            local.Group.Update.Case1.Number = local.NullCase.Number;
            local.Group.Update.LegalActionAssigment.OverrideInd =
              entities.LegalActionAssigment.OverrideInd;
            local.Group.Update.LegalAction.Assign(entities.LegalAction);
          }
        }
      }

      if (export.AsgnCount.Count > local.MaxAssignmentsToDisplay.Count)
      {
        // -- The search criteria resulted in more legal action assignments than
        // currently permitted for display.
        export.Group.Count = 0;
        ExitState = "ACO_NE0000_LIMIT_SEARCH_OR_PRINT";

        return;
      }

      if (Equal(global.Command, "DISPLAY"))
      {
        if (export.AsgnCount.Count == 0)
        {
          export.ScreenNumber.Count = 0;
          export.MaxScreenNumber.Count = 0;
        }
        else
        {
          // -- Calculate the max number of screens of data.
          export.ScreenNumber.Count = 1;
          export.MaxScreenNumber.Count = local.Group.Count / Export
            .GroupGroup.Capacity;

          if (export.MaxScreenNumber.Count * Export.GroupGroup.Capacity < local
            .Group.Count)
          {
            ++export.MaxScreenNumber.Count;
          }
        }
      }

      // -- Sort the raw data by Case Number (ascending), Standard Number (
      // ascending), Created Timestamp (descending),
      //    and Legal Action ID (ascending).
      local.I.Count = 1;

      for(var limit = local.Group.Count; local.I.Count <= limit; ++
        local.I.Count)
      {
        if (local.I.Count > Export.Export660PageGroup.Capacity + 1)
        {
          break;
        }

        local.Group.Index = local.I.Count - 1;
        local.Group.CheckSize();

        local.CompareGroupA.CompareACase.Number = local.Group.Item.Case1.Number;
        local.CompareGroupA.CompareALegalAction.Assign(
          local.Group.Item.LegalAction);
        local.CompareGroupA.CompareALegalActionAssigment.OverrideInd =
          local.Group.Item.LegalActionAssigment.OverrideInd;
        local.J.Count = local.I.Count + 1;

        for(var limit1 = local.Group.Count; local.J.Count <= limit1; ++
          local.J.Count)
        {
          local.Group.Index = local.J.Count - 1;
          local.Group.CheckSize();

          if (Lt(local.CompareGroupA.CompareACase.Number,
            local.Group.Item.Case1.Number) || Equal
            (local.Group.Item.Case1.Number,
            local.CompareGroupA.CompareACase.Number) && Lt
            (local.CompareGroupA.CompareALegalAction.StandardNumber,
            local.Group.Item.LegalAction.StandardNumber) || Equal
            (local.Group.Item.Case1.Number,
            local.CompareGroupA.CompareACase.Number) && Equal
            (local.Group.Item.LegalAction.StandardNumber,
            local.CompareGroupA.CompareALegalAction.StandardNumber) && Lt
            (local.Group.Item.LegalAction.CreatedTstamp,
            local.CompareGroupA.CompareALegalAction.CreatedTstamp) || Equal
            (local.Group.Item.Case1.Number,
            local.CompareGroupA.CompareACase.Number) && Equal
            (local.Group.Item.LegalAction.StandardNumber,
            local.CompareGroupA.CompareALegalAction.StandardNumber) && Equal
            (local.Group.Item.LegalAction.CreatedTstamp,
            local.CompareGroupA.CompareALegalAction.CreatedTstamp) && local
            .Group.Item.LegalAction.Identifier > local
            .CompareGroupA.CompareALegalAction.Identifier)
          {
            continue;
          }

          local.CompareGroupB.CompareBCase.Number =
            local.Group.Item.Case1.Number;
          local.CompareGroupB.CompareBLegalAction.Assign(
            local.Group.Item.LegalAction);
          local.CompareGroupB.CompareBLegalActionAssigment.OverrideInd =
            local.Group.Item.LegalActionAssigment.OverrideInd;
          local.Group.Update.Case1.Number =
            local.CompareGroupA.CompareACase.Number;
          local.Group.Update.LegalAction.Assign(
            local.CompareGroupA.CompareALegalAction);
          local.Group.Update.LegalActionAssigment.OverrideInd =
            local.CompareGroupA.CompareALegalActionAssigment.OverrideInd;

          local.Group.Index = local.I.Count - 1;
          local.Group.CheckSize();

          local.Group.Update.Case1.Number =
            local.CompareGroupB.CompareBCase.Number;
          local.Group.Update.LegalAction.Assign(
            local.CompareGroupB.CompareBLegalAction);
          local.Group.Update.LegalActionAssigment.OverrideInd =
            local.CompareGroupB.CompareBLegalActionAssigment.OverrideInd;
          local.CompareGroupA.CompareACase.Number =
            local.Group.Item.Case1.Number;
          local.CompareGroupA.CompareALegalAction.Assign(
            local.Group.Item.LegalAction);
          local.CompareGroupA.CompareALegalActionAssigment.OverrideInd =
            local.Group.Item.LegalActionAssigment.OverrideInd;
        }
      }

      // -- Load the big group with 660 rows of data.  This will be used to 
      // satisfy additional scrolling requests made by the user.
      //    This will allow the user to scroll up to 60 screens without having 
      // to re-read any data.
      //    This approach was taken to aid performance of this screen.
      export.Export660Page.Index = -1;
      export.Export660Page.Count = 0;

      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        if (export.Export660Page.Index + 1 < Export.Export660PageGroup.Capacity)
        {
          ++export.Export660Page.Index;
          export.Export660Page.CheckSize();

          export.Export660Page.Update.Export770Case.Number =
            local.Group.Item.Case1.Number;
          export.Export660Page.Update.Export770LegalAction.Identifier =
            local.Group.Item.LegalAction.Identifier;
        }
        else
        {
          // -- Group is full.  This is the key of the first item on the next 
          // page of 660 items.
          export.PageKeys.Index = export.CurrentPage.Count;
          export.PageKeys.CheckSize();

          export.PageKeys.Update.PageKeyLegalAction.Assign(
            local.Group.Item.LegalAction);
          export.PageKeys.Update.PageKeyCase.Number =
            local.Group.Item.Case1.Number;
          export.ScrollingAttributes.PlusFlag = "+";

          break;
        }
      }

      local.Group.CheckIndex();

      // -- Load the screen display group with 11 rows of data.
      if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT"))
      {
        local.StartingIndex.Subscript = 1;
      }
      else if (Equal(global.Command, "PREV"))
      {
        // -- When scolling backwards we need to display the last 11 rows (from 
        // 650 to 660) in the export group.
        local.StartingIndex.Subscript = Export.Export660PageGroup.Capacity - 10;
      }

      local.Group.Index = local.StartingIndex.Subscript - 1;

      for(var limit = local.Group.Count; local.Group.Index < limit; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        if (export.Group.Index + 1 < Export.GroupGroup.Capacity)
        {
          // -- Add to the display group
          ++export.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.Case1.Number = local.Group.Item.Case1.Number;
          export.Group.Update.LegalAction.Assign(local.Group.Item.LegalAction);
          export.Group.Update.LegalActionAssigment.OverrideInd =
            local.Group.Item.LegalActionAssigment.OverrideInd;
          export.Group.Update.Common.SelectChar = "";
          UseLeGetActionTakenDescription();
          export.Group.Update.LegalAction.ActionTaken =
            local.CodeValue.Description;
          export.Group.Update.SubscriptIn660.Subscript = local.Group.Index + 1;
        }
        else
        {
          // -- Display group is full.  There is more data to display set the 
          // more scrolling indicator.
          export.ScrollingAttributes.PlusFlag = "+";

          break;
        }
      }

      local.Group.CheckIndex();

      if (export.Group.Count == 0)
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
      else
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      export.SearchHiddenFipsTribAddress.Assign(export.SearchFipsTribAddress);
      export.SearchHiddenLegalAction.Assign(export.SearchLegalAction);
      export.SearchHiddenLegalActionAssigment.OverrideInd =
        export.SearchLegalActionAssigment.OverrideInd;
      MoveCodeValue(export.SearchActionTaken, export.SearchHiddenActionTakn);
      export.SearchHiddenCase.Number = export.SearchCase.Number;

      return;
    }

    if (Equal(global.Command, "PRINT"))
    {
      // : Format and Set the TranCode and Parm Information for the JOB as 
      // required by the Print Function.
      // -- Send Office id (4), Service provider id (5), office service provider
      // role (2), office service provider effective date (10),
      //    standard number (12), tribunal id (9), created date (10), class (1),
      // action taken, filed date (10),
      //    case number (10), override indicator (1).
      // 1         2         3         4         5         6         7         8
      // 9         0         1         2
      // 123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
      // 9999 99999 XX YYYY-MM-DD XXXXXXXXXXXX 999999999 YYYY-MM-DD X 
      // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX YYYY-MM-DD 9999999999 X
      // POSITIONS DATA VALUE
      // --------- 
      // -------------------------------------
      //   1 -   4 Office id (4)
      //   6 -  10 Service provider id (5)
      //  12 -  13 office service provider role (2)
      //  15 -  24 office service provider effective date (10)
      //  26 -  37 standard number (12)
      //  39 -  47 tribunal id (9)
      //  49 -  58 created date (10)
      //  60 -  60 class (1)
      //  62 -  91 action taken (30)
      //  93 - 102 Filed date (10)
      // 104 - 113 case number (10)
      // 115 - 115 override indicator (1)
      export.DlgflwJob.TranId = global.TranCode;
      local.OfficeId.Text4 =
        NumberToString(export.Office.SystemGeneratedId, 12, 4);
      local.SvcPrdrIdTxt.Text5 =
        NumberToString(export.ServiceProvider.SystemGeneratedId, 11, 5);
      local.TextMm.Text2 =
        NumberToString(Month(export.OfficeServiceProvider.EffectiveDate), 14, 2);
        
      local.TextDd.Text2 =
        NumberToString(Day(export.OfficeServiceProvider.EffectiveDate), 14, 2);
      local.TextYyyy.Text4 =
        NumberToString(Year(export.OfficeServiceProvider.EffectiveDate), 12, 4);
        
      local.OffSvcPrdrDateTxt.Text10 = local.TextYyyy.Text4 + "-" + local
        .TextMm.Text2 + "-" + local.TextDd.Text2;
      local.StandardNumberTxt.Text12 =
        Substring(export.SearchLegalAction.StandardNumber, 1, 12);
      local.TribunalTxt.Text9 =
        NumberToString(export.HiddenSearch.Identifier, 7, 9);
      local.TextMm.Text2 =
        NumberToString(Month(export.SearchLegalAction.CreatedTstamp), 14, 2);
      local.TextDd.Text2 =
        NumberToString(Day(export.SearchLegalAction.CreatedTstamp), 14, 2);
      local.TextYyyy.Text4 =
        NumberToString(Year(export.SearchLegalAction.CreatedTstamp), 12, 4);
      local.CreatedDateTxt.Text10 = local.TextYyyy.Text4 + "-" + local
        .TextMm.Text2 + "-" + local.TextDd.Text2;
      local.TextMm.Text2 =
        NumberToString(Month(export.SearchLegalAction.FiledDate), 14, 2);
      local.TextDd.Text2 =
        NumberToString(Day(export.SearchLegalAction.FiledDate), 14, 2);
      local.TextYyyy.Text4 =
        NumberToString(Year(export.SearchLegalAction.FiledDate), 12, 4);
      local.FiledDateTxt.Text10 = local.TextYyyy.Text4 + "-" + local
        .TextMm.Text2 + "-" + local.TextDd.Text2;
      export.DlgflwJobRun.ParmInfo = local.OfficeId.Text4 + " " + local
        .SvcPrdrIdTxt.Text5 + " " + export.OfficeServiceProvider.RoleCode + " " +
        local.OffSvcPrdrDateTxt.Text10 + " " + local
        .StandardNumberTxt.Text12 + " " + local.TribunalTxt.Text9 + " " + local
        .CreatedDateTxt.Text10 + " " + Substring
        (export.SearchLegalAction.Classification,
        LegalAction.Classification_MaxLength, 1, 1) + " " + export
        .SearchLegalAction.ActionTaken + " " + local.FiledDateTxt.Text10 + " " +
        export.SearchCase.Number + " " + export
        .SearchLegalActionAssigment.OverrideInd;
      ExitState = "CO_LINK_TO_POPT";
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyDescription = source.CountyDescription;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
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

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
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

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = export.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.CdvlReturnCode.Flag = useExport.ValidCode.Flag;
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

  private void UseLeCabGetClassForActTaken()
  {
    var useImport = new LeCabGetClassForActTaken.Import();
    var useExport = new LeCabGetClassForActTaken.Export();

    useImport.LegalAction.ActionTaken = export.SearchLegalAction.ActionTaken;

    Call(LeCabGetClassForActTaken.Execute, useImport, useExport);

    local.LegalAction.Classification = useExport.LegalAction.Classification;
  }

  private void UseLeGetActionTakenDescription()
  {
    var useImport = new LeGetActionTakenDescription.Import();
    var useExport = new LeGetActionTakenDescription.Export();

    useImport.LegalAction.ActionTaken =
      export.Group.Item.LegalAction.ActionTaken;

    Call(LeGetActionTakenDescription.Execute, useImport, useExport);

    local.CodeValue.Description = useExport.CodeValue.Description;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(local.NextTranInfo, useImport.NextTranInfo);

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

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseLegalActionLegalActionAssigment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LegalActionAssigment.Populated = false;
    entities.LegalAction.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseLegalActionLegalActionAssigment",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.SearchCase.Number);
        db.SetDateTime(
          command, "createdTstamp",
          export.SearchLegalAction.CreatedTstamp.GetValueOrDefault());
        db.SetString(
          command, "classification", export.SearchLegalAction.Classification);
        db.SetString(
          command, "actionTaken", export.SearchLegalAction.ActionTaken);
        db.SetNullableDate(
          command, "filedDt",
          export.SearchLegalAction.FiledDate.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNumber",
          export.SearchLegalAction.StandardNumber ?? "");
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "overrideInd",
          export.SearchLegalActionAssigment.OverrideInd);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 1);
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.LegalAction.Classification = db.GetString(reader, 2);
        entities.LegalAction.ActionTaken = db.GetString(reader, 3);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 9);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 10);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 13);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 14);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 15);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.LegalActionAssigment.Populated = true;
        entities.LegalAction.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", export.SearchFipsTribAddress.State);
        db.SetNullableString(
          command, "countyAbbr", export.SearchFipsTribAddress.County ?? "");
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", export.Group.Item.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          export.Export660Page.Item.Export770LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionAssigment()
  {
    entities.LegalActionAssigment.Populated = false;

    return Read("ReadLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "ospEffectiveDate",
          export.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableString(
          command, "ospRoleCode", export.OfficeServiceProvider.RoleCode);
        db.SetNullableInt32(
          command, "offGeneratedId", export.Office.SystemGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId", export.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 7);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.LegalActionAssigment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionAssigment1()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LegalActionAssigment.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionAssigment1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", export.SearchLegalAction.StandardNumber ?? ""
          );
        db.SetDateTime(
          command, "createdTstamp",
          export.SearchLegalAction.CreatedTstamp.GetValueOrDefault());
        db.SetString(
          command, "classification", export.SearchLegalAction.Classification);
        db.SetString(
          command, "actionTaken", export.SearchLegalAction.ActionTaken);
        db.SetNullableDate(
          command, "filedDt",
          export.SearchLegalAction.FiledDate.GetValueOrDefault());
        db.SetString(
          command, "overrideInd",
          export.SearchLegalActionAssigment.OverrideInd);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 8);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 9);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 12);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 14);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.LegalActionAssigment.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionAssigment2()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.LegalActionAssigment.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionAssigment2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          export.SearchLegalAction.CreatedTstamp.GetValueOrDefault());
        db.SetString(
          command, "classification", export.SearchLegalAction.Classification);
        db.SetString(
          command, "actionTaken", export.SearchLegalAction.ActionTaken);
        db.SetNullableDate(
          command, "filedDt",
          export.SearchLegalAction.FiledDate.GetValueOrDefault());
        db.SetString(
          command, "overrideInd",
          export.SearchLegalActionAssigment.OverrideInd);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 8);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 9);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 12);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 14);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.LegalActionAssigment.Populated = true;
        entities.LegalAction.Populated = true;

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

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier1",
          entities.LegalAction.TrbId.GetValueOrDefault());
        db.SetInt32(command, "identifier2", export.HiddenSearch.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Tribunal.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTribunalFips()
  {
    entities.Fips.Populated = false;
    entities.Tribunal.Populated = false;

    return ReadEach("ReadTribunalFips",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", export.SearchFipsTribAddress.State);
        db.SetNullableString(
          command, "countyAbbr", export.SearchFipsTribAddress.County ?? "");
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Fips.Location = db.GetInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Fips.County = db.GetInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Fips.State = db.GetInt32(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.StateAbbreviation = db.GetString(reader, 5);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 6);
        entities.Fips.Populated = true;
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
    /// <summary>A Import660PageGroup group.</summary>
    [Serializable]
    public class Import660PageGroup
    {
      /// <summary>
      /// A value of Import660Case.
      /// </summary>
      [JsonPropertyName("import660Case")]
      public Case1 Import660Case
      {
        get => import660Case ??= new();
        set => import660Case = value;
      }

      /// <summary>
      /// A value of Import660LegalAction.
      /// </summary>
      [JsonPropertyName("import660LegalAction")]
      public LegalAction Import660LegalAction
      {
        get => import660LegalAction ??= new();
        set => import660LegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 660;

      private Case1 import660Case;
      private LegalAction import660LegalAction;
    }

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
      /// A value of PageKeyLegalAction.
      /// </summary>
      [JsonPropertyName("pageKeyLegalAction")]
      public LegalAction PageKeyLegalAction
      {
        get => pageKeyLegalAction ??= new();
        set => pageKeyLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private Case1 pageKeyCase;
      private LegalAction pageKeyLegalAction;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of SubscriptIn660.
      /// </summary>
      [JsonPropertyName("subscriptIn660")]
      public Common SubscriptIn660
      {
        get => subscriptIn660 ??= new();
        set => subscriptIn660 = value;
      }

      /// <summary>
      /// A value of LegalActionAssigment.
      /// </summary>
      [JsonPropertyName("legalActionAssigment")]
      public LegalActionAssigment LegalActionAssigment
      {
        get => legalActionAssigment ??= new();
        set => legalActionAssigment = value;
      }

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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Common subscriptIn660;
      private LegalActionAssigment legalActionAssigment;
      private LegalAction legalAction;
      private Common common;
      private Case1 case1;
    }

    /// <summary>
    /// A value of MaxScreenNumber.
    /// </summary>
    [JsonPropertyName("maxScreenNumber")]
    public Common MaxScreenNumber
    {
      get => maxScreenNumber ??= new();
      set => maxScreenNumber = value;
    }

    /// <summary>
    /// A value of ScreenNumber.
    /// </summary>
    [JsonPropertyName("screenNumber")]
    public Common ScreenNumber
    {
      get => screenNumber ??= new();
      set => screenNumber = value;
    }

    /// <summary>
    /// Gets a value of Import660Page.
    /// </summary>
    [JsonIgnore]
    public Array<Import660PageGroup> Import660Page => import660Page ??= new(
      Import660PageGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Import660Page for json serialization.
    /// </summary>
    [JsonPropertyName("import660Page")]
    [Computed]
    public IList<Import660PageGroup> Import660Page_Json
    {
      get => import660Page;
      set => Import660Page.Assign(value);
    }

    /// <summary>
    /// A value of HiddenSearch.
    /// </summary>
    [JsonPropertyName("hiddenSearch")]
    public Tribunal HiddenSearch
    {
      get => hiddenSearch ??= new();
      set => hiddenSearch = value;
    }

    /// <summary>
    /// A value of HiddenPreviousAction.
    /// </summary>
    [JsonPropertyName("hiddenPreviousAction")]
    public Common HiddenPreviousAction
    {
      get => hiddenPreviousAction ??= new();
      set => hiddenPreviousAction = value;
    }

    /// <summary>
    /// A value of FromLtrbFipsTribAddress.
    /// </summary>
    [JsonPropertyName("fromLtrbFipsTribAddress")]
    public FipsTribAddress FromLtrbFipsTribAddress
    {
      get => fromLtrbFipsTribAddress ??= new();
      set => fromLtrbFipsTribAddress = value;
    }

    /// <summary>
    /// A value of FromLtrbFips.
    /// </summary>
    [JsonPropertyName("fromLtrbFips")]
    public Fips FromLtrbFips
    {
      get => fromLtrbFips ??= new();
      set => fromLtrbFips = value;
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
    /// A value of SearchPromptSvpo.
    /// </summary>
    [JsonPropertyName("searchPromptSvpo")]
    public Common SearchPromptSvpo
    {
      get => searchPromptSvpo ??= new();
      set => searchPromptSvpo = value;
    }

    /// <summary>
    /// A value of SearchTribunalPrompt.
    /// </summary>
    [JsonPropertyName("searchTribunalPrompt")]
    public Common SearchTribunalPrompt
    {
      get => searchTribunalPrompt ??= new();
      set => searchTribunalPrompt = value;
    }

    /// <summary>
    /// A value of SearchClassPrompt.
    /// </summary>
    [JsonPropertyName("searchClassPrompt")]
    public Common SearchClassPrompt
    {
      get => searchClassPrompt ??= new();
      set => searchClassPrompt = value;
    }

    /// <summary>
    /// A value of SearchActionTaknPrompt.
    /// </summary>
    [JsonPropertyName("searchActionTaknPrompt")]
    public Common SearchActionTaknPrompt
    {
      get => searchActionTaknPrompt ??= new();
      set => searchActionTaknPrompt = value;
    }

    /// <summary>
    /// A value of SearchActionTaken.
    /// </summary>
    [JsonPropertyName("searchActionTaken")]
    public CodeValue SearchActionTaken
    {
      get => searchActionTaken ??= new();
      set => searchActionTaken = value;
    }

    /// <summary>
    /// A value of SearchHiddenActionTakn.
    /// </summary>
    [JsonPropertyName("searchHiddenActionTakn")]
    public CodeValue SearchHiddenActionTakn
    {
      get => searchHiddenActionTakn ??= new();
      set => searchHiddenActionTakn = value;
    }

    /// <summary>
    /// A value of SearchLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("searchLegalActionAssigment")]
    public LegalActionAssigment SearchLegalActionAssigment
    {
      get => searchLegalActionAssigment ??= new();
      set => searchLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of SearchHiddenLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("searchHiddenLegalActionAssigment")]
    public LegalActionAssigment SearchHiddenLegalActionAssigment
    {
      get => searchHiddenLegalActionAssigment ??= new();
      set => searchHiddenLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of SearchFipsTribAddress.
    /// </summary>
    [JsonPropertyName("searchFipsTribAddress")]
    public FipsTribAddress SearchFipsTribAddress
    {
      get => searchFipsTribAddress ??= new();
      set => searchFipsTribAddress = value;
    }

    /// <summary>
    /// A value of SearchHiddenFipsTribAddress.
    /// </summary>
    [JsonPropertyName("searchHiddenFipsTribAddress")]
    public FipsTribAddress SearchHiddenFipsTribAddress
    {
      get => searchHiddenFipsTribAddress ??= new();
      set => searchHiddenFipsTribAddress = value;
    }

    /// <summary>
    /// A value of SearchLegalAction.
    /// </summary>
    [JsonPropertyName("searchLegalAction")]
    public LegalAction SearchLegalAction
    {
      get => searchLegalAction ??= new();
      set => searchLegalAction = value;
    }

    /// <summary>
    /// A value of SearchHiddenLegalAction.
    /// </summary>
    [JsonPropertyName("searchHiddenLegalAction")]
    public LegalAction SearchHiddenLegalAction
    {
      get => searchHiddenLegalAction ??= new();
      set => searchHiddenLegalAction = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of FromLtrbTribunal.
    /// </summary>
    [JsonPropertyName("fromLtrbTribunal")]
    public Tribunal FromLtrbTribunal
    {
      get => fromLtrbTribunal ??= new();
      set => fromLtrbTribunal = value;
    }

    /// <summary>
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    /// <summary>
    /// A value of SearchHiddenCase.
    /// </summary>
    [JsonPropertyName("searchHiddenCase")]
    public Case1 SearchHiddenCase
    {
      get => searchHiddenCase ??= new();
      set => searchHiddenCase = value;
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
    /// A value of FromCdvl.
    /// </summary>
    [JsonPropertyName("fromCdvl")]
    public CodeValue FromCdvl
    {
      get => fromCdvl ??= new();
      set => fromCdvl = value;
    }

    /// <summary>
    /// A value of FromSvpoOffice.
    /// </summary>
    [JsonPropertyName("fromSvpoOffice")]
    public Office FromSvpoOffice
    {
      get => fromSvpoOffice ??= new();
      set => fromSvpoOffice = value;
    }

    /// <summary>
    /// A value of FromSvpoOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("fromSvpoOfficeServiceProvider")]
    public OfficeServiceProvider FromSvpoOfficeServiceProvider
    {
      get => fromSvpoOfficeServiceProvider ??= new();
      set => fromSvpoOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of FromSvpoServiceProvider.
    /// </summary>
    [JsonPropertyName("fromSvpoServiceProvider")]
    public ServiceProvider FromSvpoServiceProvider
    {
      get => fromSvpoServiceProvider ??= new();
      set => fromSvpoServiceProvider = value;
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

    private Common maxScreenNumber;
    private Common screenNumber;
    private Array<Import660PageGroup> import660Page;
    private Tribunal hiddenSearch;
    private Common hiddenPreviousAction;
    private FipsTribAddress fromLtrbFipsTribAddress;
    private Fips fromLtrbFips;
    private Standard standard;
    private Common searchPromptSvpo;
    private Common searchTribunalPrompt;
    private Common searchClassPrompt;
    private Common searchActionTaknPrompt;
    private CodeValue searchActionTaken;
    private CodeValue searchHiddenActionTakn;
    private LegalActionAssigment searchLegalActionAssigment;
    private LegalActionAssigment searchHiddenLegalActionAssigment;
    private FipsTribAddress searchFipsTribAddress;
    private FipsTribAddress searchHiddenFipsTribAddress;
    private LegalAction searchLegalAction;
    private LegalAction searchHiddenLegalAction;
    private ScrollingAttributes scrollingAttributes;
    private Tribunal fromLtrbTribunal;
    private Case1 searchCase;
    private Case1 searchHiddenCase;
    private Common asgnCount;
    private Code code;
    private Common currentPage;
    private CodeValue fromCdvl;
    private Office fromSvpoOffice;
    private OfficeServiceProvider fromSvpoOfficeServiceProvider;
    private ServiceProvider fromSvpoServiceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Array<PageKeysGroup> pageKeys;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A Export660PageGroup group.</summary>
    [Serializable]
    public class Export660PageGroup
    {
      /// <summary>
      /// A value of Export770Case.
      /// </summary>
      [JsonPropertyName("export770Case")]
      public Case1 Export770Case
      {
        get => export770Case ??= new();
        set => export770Case = value;
      }

      /// <summary>
      /// A value of Export770LegalAction.
      /// </summary>
      [JsonPropertyName("export770LegalAction")]
      public LegalAction Export770LegalAction
      {
        get => export770LegalAction ??= new();
        set => export770LegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 660;

      private Case1 export770Case;
      private LegalAction export770LegalAction;
    }

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
      /// A value of PageKeyLegalAction.
      /// </summary>
      [JsonPropertyName("pageKeyLegalAction")]
      public LegalAction PageKeyLegalAction
      {
        get => pageKeyLegalAction ??= new();
        set => pageKeyLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private Case1 pageKeyCase;
      private LegalAction pageKeyLegalAction;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of SubscriptIn660.
      /// </summary>
      [JsonPropertyName("subscriptIn660")]
      public Common SubscriptIn660
      {
        get => subscriptIn660 ??= new();
        set => subscriptIn660 = value;
      }

      /// <summary>
      /// A value of LegalActionAssigment.
      /// </summary>
      [JsonPropertyName("legalActionAssigment")]
      public LegalActionAssigment LegalActionAssigment
      {
        get => legalActionAssigment ??= new();
        set => legalActionAssigment = value;
      }

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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Common subscriptIn660;
      private LegalActionAssigment legalActionAssigment;
      private LegalAction legalAction;
      private Common common;
      private Case1 case1;
    }

    /// <summary>
    /// A value of MaxScreenNumber.
    /// </summary>
    [JsonPropertyName("maxScreenNumber")]
    public Common MaxScreenNumber
    {
      get => maxScreenNumber ??= new();
      set => maxScreenNumber = value;
    }

    /// <summary>
    /// A value of ScreenNumber.
    /// </summary>
    [JsonPropertyName("screenNumber")]
    public Common ScreenNumber
    {
      get => screenNumber ??= new();
      set => screenNumber = value;
    }

    /// <summary>
    /// Gets a value of Export660Page.
    /// </summary>
    [JsonIgnore]
    public Array<Export660PageGroup> Export660Page => export660Page ??= new(
      Export660PageGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export660Page for json serialization.
    /// </summary>
    [JsonPropertyName("export660Page")]
    [Computed]
    public IList<Export660PageGroup> Export660Page_Json
    {
      get => export660Page;
      set => Export660Page.Assign(value);
    }

    /// <summary>
    /// A value of IwglType.
    /// </summary>
    [JsonPropertyName("iwglType")]
    public WorkArea IwglType
    {
      get => iwglType ??= new();
      set => iwglType = value;
    }

    /// <summary>
    /// A value of HiddenPreviousAction.
    /// </summary>
    [JsonPropertyName("hiddenPreviousAction")]
    public Common HiddenPreviousAction
    {
      get => hiddenPreviousAction ??= new();
      set => hiddenPreviousAction = value;
    }

    /// <summary>
    /// A value of ToCdvlValidWithCode.
    /// </summary>
    [JsonPropertyName("toCdvlValidWithCode")]
    public Code ToCdvlValidWithCode
    {
      get => toCdvlValidWithCode ??= new();
      set => toCdvlValidWithCode = value;
    }

    /// <summary>
    /// A value of ToCdvlValidWithCodeValue.
    /// </summary>
    [JsonPropertyName("toCdvlValidWithCodeValue")]
    public CodeValue ToCdvlValidWithCodeValue
    {
      get => toCdvlValidWithCodeValue ??= new();
      set => toCdvlValidWithCodeValue = value;
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

    /// <summary>
    /// A value of SelectedLegalAction.
    /// </summary>
    [JsonPropertyName("selectedLegalAction")]
    public LegalAction SelectedLegalAction
    {
      get => selectedLegalAction ??= new();
      set => selectedLegalAction = value;
    }

    /// <summary>
    /// A value of SearchHiddenActionTakn.
    /// </summary>
    [JsonPropertyName("searchHiddenActionTakn")]
    public CodeValue SearchHiddenActionTakn
    {
      get => searchHiddenActionTakn ??= new();
      set => searchHiddenActionTakn = value;
    }

    /// <summary>
    /// A value of SearchHiddenLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("searchHiddenLegalActionAssigment")]
    public LegalActionAssigment SearchHiddenLegalActionAssigment
    {
      get => searchHiddenLegalActionAssigment ??= new();
      set => searchHiddenLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of SearchHiddenFipsTribAddress.
    /// </summary>
    [JsonPropertyName("searchHiddenFipsTribAddress")]
    public FipsTribAddress SearchHiddenFipsTribAddress
    {
      get => searchHiddenFipsTribAddress ??= new();
      set => searchHiddenFipsTribAddress = value;
    }

    /// <summary>
    /// A value of SearchHiddenLegalAction.
    /// </summary>
    [JsonPropertyName("searchHiddenLegalAction")]
    public LegalAction SearchHiddenLegalAction
    {
      get => searchHiddenLegalAction ??= new();
      set => searchHiddenLegalAction = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of SearchActionTaknPrompt.
    /// </summary>
    [JsonPropertyName("searchActionTaknPrompt")]
    public Common SearchActionTaknPrompt
    {
      get => searchActionTaknPrompt ??= new();
      set => searchActionTaknPrompt = value;
    }

    /// <summary>
    /// A value of SearchClassPrompt.
    /// </summary>
    [JsonPropertyName("searchClassPrompt")]
    public Common SearchClassPrompt
    {
      get => searchClassPrompt ??= new();
      set => searchClassPrompt = value;
    }

    /// <summary>
    /// A value of SearchTribunalPrompt.
    /// </summary>
    [JsonPropertyName("searchTribunalPrompt")]
    public Common SearchTribunalPrompt
    {
      get => searchTribunalPrompt ??= new();
      set => searchTribunalPrompt = value;
    }

    /// <summary>
    /// A value of SearchActionTaken.
    /// </summary>
    [JsonPropertyName("searchActionTaken")]
    public CodeValue SearchActionTaken
    {
      get => searchActionTaken ??= new();
      set => searchActionTaken = value;
    }

    /// <summary>
    /// A value of SearchLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("searchLegalActionAssigment")]
    public LegalActionAssigment SearchLegalActionAssigment
    {
      get => searchLegalActionAssigment ??= new();
      set => searchLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of HiddenSearch.
    /// </summary>
    [JsonPropertyName("hiddenSearch")]
    public Tribunal HiddenSearch
    {
      get => hiddenSearch ??= new();
      set => hiddenSearch = value;
    }

    /// <summary>
    /// A value of SearchFipsTribAddress.
    /// </summary>
    [JsonPropertyName("searchFipsTribAddress")]
    public FipsTribAddress SearchFipsTribAddress
    {
      get => searchFipsTribAddress ??= new();
      set => searchFipsTribAddress = value;
    }

    /// <summary>
    /// A value of SearchLegalAction.
    /// </summary>
    [JsonPropertyName("searchLegalAction")]
    public LegalAction SearchLegalAction
    {
      get => searchLegalAction ??= new();
      set => searchLegalAction = value;
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
    /// A value of ToCdvlCode.
    /// </summary>
    [JsonPropertyName("toCdvlCode")]
    public Code ToCdvlCode
    {
      get => toCdvlCode ??= new();
      set => toCdvlCode = value;
    }

    /// <summary>
    /// A value of ToCdvlCodeValue.
    /// </summary>
    [JsonPropertyName("toCdvlCodeValue")]
    public CodeValue ToCdvlCodeValue
    {
      get => toCdvlCodeValue ??= new();
      set => toCdvlCodeValue = value;
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
    /// A value of SelectedCase.
    /// </summary>
    [JsonPropertyName("selectedCase")]
    public Case1 SelectedCase
    {
      get => selectedCase ??= new();
      set => selectedCase = value;
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
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    /// <summary>
    /// A value of SearchHiddenCase.
    /// </summary>
    [JsonPropertyName("searchHiddenCase")]
    public Case1 SearchHiddenCase
    {
      get => searchHiddenCase ??= new();
      set => searchHiddenCase = value;
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
    /// A value of DlgflwAsinHeaderObject.
    /// </summary>
    [JsonPropertyName("dlgflwAsinHeaderObject")]
    public SpTextWorkArea DlgflwAsinHeaderObject
    {
      get => dlgflwAsinHeaderObject ??= new();
      set => dlgflwAsinHeaderObject = value;
    }

    private Common maxScreenNumber;
    private Common screenNumber;
    private Array<Export660PageGroup> export660Page;
    private WorkArea iwglType;
    private Common hiddenPreviousAction;
    private Code toCdvlValidWithCode;
    private CodeValue toCdvlValidWithCodeValue;
    private Fips toLtrb;
    private LegalAction selectedLegalAction;
    private CodeValue searchHiddenActionTakn;
    private LegalActionAssigment searchHiddenLegalActionAssigment;
    private FipsTribAddress searchHiddenFipsTribAddress;
    private LegalAction searchHiddenLegalAction;
    private ScrollingAttributes scrollingAttributes;
    private Common searchActionTaknPrompt;
    private Common searchClassPrompt;
    private Common searchTribunalPrompt;
    private CodeValue searchActionTaken;
    private LegalActionAssigment searchLegalActionAssigment;
    private Tribunal hiddenSearch;
    private FipsTribAddress searchFipsTribAddress;
    private LegalAction searchLegalAction;
    private Common asgnCount;
    private Code code;
    private Common currentPage;
    private Common hiddenCalcDone;
    private Code toCdvlCode;
    private CodeValue toCdvlCodeValue;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private Common searchPromptSvpo;
    private Case1 selectedCase;
    private ServiceProvider serviceProvider;
    private Standard standard;
    private Array<PageKeysGroup> pageKeys;
    private Array<GroupGroup> group;
    private Case1 searchCase;
    private Case1 searchHiddenCase;
    private Job dlgflwJob;
    private JobRun dlgflwJobRun;
    private SpTextWorkArea dlgflwAsinHeaderObject;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A CompareGroupBGroup group.</summary>
    [Serializable]
    public class CompareGroupBGroup
    {
      /// <summary>
      /// A value of CompareBLegalActionAssigment.
      /// </summary>
      [JsonPropertyName("compareBLegalActionAssigment")]
      public LegalActionAssigment CompareBLegalActionAssigment
      {
        get => compareBLegalActionAssigment ??= new();
        set => compareBLegalActionAssigment = value;
      }

      /// <summary>
      /// A value of CompareBLegalAction.
      /// </summary>
      [JsonPropertyName("compareBLegalAction")]
      public LegalAction CompareBLegalAction
      {
        get => compareBLegalAction ??= new();
        set => compareBLegalAction = value;
      }

      /// <summary>
      /// A value of CompareBCase.
      /// </summary>
      [JsonPropertyName("compareBCase")]
      public Case1 CompareBCase
      {
        get => compareBCase ??= new();
        set => compareBCase = value;
      }

      private LegalActionAssigment compareBLegalActionAssigment;
      private LegalAction compareBLegalAction;
      private Case1 compareBCase;
    }

    /// <summary>A CompareGroupAGroup group.</summary>
    [Serializable]
    public class CompareGroupAGroup
    {
      /// <summary>
      /// A value of CompareALegalActionAssigment.
      /// </summary>
      [JsonPropertyName("compareALegalActionAssigment")]
      public LegalActionAssigment CompareALegalActionAssigment
      {
        get => compareALegalActionAssigment ??= new();
        set => compareALegalActionAssigment = value;
      }

      /// <summary>
      /// A value of CompareALegalAction.
      /// </summary>
      [JsonPropertyName("compareALegalAction")]
      public LegalAction CompareALegalAction
      {
        get => compareALegalAction ??= new();
        set => compareALegalAction = value;
      }

      /// <summary>
      /// A value of CompareACase.
      /// </summary>
      [JsonPropertyName("compareACase")]
      public Case1 CompareACase
      {
        get => compareACase ??= new();
        set => compareACase = value;
      }

      private LegalActionAssigment compareALegalActionAssigment;
      private LegalAction compareALegalAction;
      private Case1 compareACase;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of LegalActionAssigment.
      /// </summary>
      [JsonPropertyName("legalActionAssigment")]
      public LegalActionAssigment LegalActionAssigment
      {
        get => legalActionAssigment ??= new();
        set => legalActionAssigment = value;
      }

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
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20000;

      private LegalActionAssigment legalActionAssigment;
      private LegalAction legalAction;
      private Case1 case1;
    }

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
    /// A value of NullCodeValue.
    /// </summary>
    [JsonPropertyName("nullCodeValue")]
    public CodeValue NullCodeValue
    {
      get => nullCodeValue ??= new();
      set => nullCodeValue = value;
    }

    /// <summary>
    /// A value of NullFipsTribAddress.
    /// </summary>
    [JsonPropertyName("nullFipsTribAddress")]
    public FipsTribAddress NullFipsTribAddress
    {
      get => nullFipsTribAddress ??= new();
      set => nullFipsTribAddress = value;
    }

    /// <summary>
    /// A value of OfficeId.
    /// </summary>
    [JsonPropertyName("officeId")]
    public WorkArea OfficeId
    {
      get => officeId ??= new();
      set => officeId = value;
    }

    /// <summary>
    /// A value of TribunalTxt.
    /// </summary>
    [JsonPropertyName("tribunalTxt")]
    public WorkArea TribunalTxt
    {
      get => tribunalTxt ??= new();
      set => tribunalTxt = value;
    }

    /// <summary>
    /// A value of StandardNumberTxt.
    /// </summary>
    [JsonPropertyName("standardNumberTxt")]
    public TextWorkArea StandardNumberTxt
    {
      get => standardNumberTxt ??= new();
      set => standardNumberTxt = value;
    }

    /// <summary>
    /// A value of CreatedDateTxt.
    /// </summary>
    [JsonPropertyName("createdDateTxt")]
    public TextWorkArea CreatedDateTxt
    {
      get => createdDateTxt ??= new();
      set => createdDateTxt = value;
    }

    /// <summary>
    /// A value of J.
    /// </summary>
    [JsonPropertyName("j")]
    public Common J
    {
      get => j ??= new();
      set => j = value;
    }

    /// <summary>
    /// A value of I.
    /// </summary>
    [JsonPropertyName("i")]
    public Common I
    {
      get => i ??= new();
      set => i = value;
    }

    /// <summary>
    /// A value of StartingIndex.
    /// </summary>
    [JsonPropertyName("startingIndex")]
    public Common StartingIndex
    {
      get => startingIndex ??= new();
      set => startingIndex = value;
    }

    /// <summary>
    /// Gets a value of CompareGroupB.
    /// </summary>
    [JsonPropertyName("compareGroupB")]
    public CompareGroupBGroup CompareGroupB
    {
      get => compareGroupB ?? (compareGroupB = new());
      set => compareGroupB = value;
    }

    /// <summary>
    /// Gets a value of CompareGroupA.
    /// </summary>
    [JsonPropertyName("compareGroupA")]
    public CompareGroupAGroup CompareGroupA
    {
      get => compareGroupA ?? (compareGroupA = new());
      set => compareGroupA = value;
    }

    /// <summary>
    /// A value of SortFinished.
    /// </summary>
    [JsonPropertyName("sortFinished")]
    public Common SortFinished
    {
      get => sortFinished ??= new();
      set => sortFinished = value;
    }

    /// <summary>
    /// A value of MaxAssignmentsToDisplay.
    /// </summary>
    [JsonPropertyName("maxAssignmentsToDisplay")]
    public Common MaxAssignmentsToDisplay
    {
      get => maxAssignmentsToDisplay ??= new();
      set => maxAssignmentsToDisplay = value;
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
    /// A value of CaseFound.
    /// </summary>
    [JsonPropertyName("caseFound")]
    public Common CaseFound
    {
      get => caseFound ??= new();
      set => caseFound = value;
    }

    /// <summary>
    /// A value of PagingKey.
    /// </summary>
    [JsonPropertyName("pagingKey")]
    public LegalAction PagingKey
    {
      get => pagingKey ??= new();
      set => pagingKey = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of TribunalCount.
    /// </summary>
    [JsonPropertyName("tribunalCount")]
    public Common TribunalCount
    {
      get => tribunalCount ??= new();
      set => tribunalCount = value;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of NullLegalAction.
    /// </summary>
    [JsonPropertyName("nullLegalAction")]
    public LegalAction NullLegalAction
    {
      get => nullLegalAction ??= new();
      set => nullLegalAction = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of NullCase.
    /// </summary>
    [JsonPropertyName("nullCase")]
    public Case1 NullCase
    {
      get => nullCase ??= new();
      set => nullCase = value;
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
    /// A value of FiledDateTxt.
    /// </summary>
    [JsonPropertyName("filedDateTxt")]
    public TextWorkArea FiledDateTxt
    {
      get => filedDateTxt ??= new();
      set => filedDateTxt = value;
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
    /// A value of OffSvcPrdrDateTxt.
    /// </summary>
    [JsonPropertyName("offSvcPrdrDateTxt")]
    public TextWorkArea OffSvcPrdrDateTxt
    {
      get => offSvcPrdrDateTxt ??= new();
      set => offSvcPrdrDateTxt = value;
    }

    private LegalAction legalAction;
    private CodeValue nullCodeValue;
    private FipsTribAddress nullFipsTribAddress;
    private WorkArea officeId;
    private WorkArea tribunalTxt;
    private TextWorkArea standardNumberTxt;
    private TextWorkArea createdDateTxt;
    private Common j;
    private Common i;
    private Common startingIndex;
    private CompareGroupBGroup compareGroupB;
    private CompareGroupAGroup compareGroupA;
    private Common sortFinished;
    private Common maxAssignmentsToDisplay;
    private Array<GroupGroup> group;
    private Common caseFound;
    private LegalAction pagingKey;
    private Fips fips;
    private Common tribunalCount;
    private Tribunal tribunal;
    private NextTranInfo nextTranInfo;
    private CodeValue codeValue;
    private Common prompt;
    private LegalAction nullLegalAction;
    private Common apCount;
    private CsePersonsWorkSet ap;
    private Common apMatchMade;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common arSearchLength;
    private Common asgnCount;
    private Common cdvlReturnCode;
    private Common count;
    private DateWorkArea current;
    private DateWorkArea initialized;
    private Case1 nullCase;
    private TextWorkArea textWorkArea;
    private WorkArea svcPrdrIdTxt;
    private TextWorkArea filedDateTxt;
    private TextWorkArea textMm;
    private TextWorkArea textDd;
    private TextWorkArea textYyyy;
    private TextWorkArea offSvcPrdrDateTxt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

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

    private CodeValue codeValue;
    private Code code;
    private LegalActionCaseRole legalActionCaseRole;
    private Fips fips;
    private Tribunal tribunal;
    private LegalActionAssigment legalActionAssigment;
    private LegalAction legalAction;
    private Case1 case1;
    private CaseRole caseRole;
    private LegalReferralCaseRole legalReferralCaseRole;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
  }
#endregion
}
