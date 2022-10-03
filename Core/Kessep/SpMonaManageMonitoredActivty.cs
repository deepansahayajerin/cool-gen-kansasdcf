// Program: SP_MONA_MANAGE_MONITORED_ACTIVTY, ID: 371929774, model: 746.
// Short name: SWEMONAP
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
/// A program: SP_MONA_MANAGE_MONITORED_ACTIVTY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpMonaManageMonitoredActivty: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_MONA_MANAGE_MONITORED_ACTIVTY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpMonaManageMonitoredActivty(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpMonaManageMonitoredActivty.
  /// </summary>
  public SpMonaManageMonitoredActivty(IContext context, Import import,
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
    // ***************************************************************************************
    // Date      Developer         Request #  	Description
    // --------  ----------------  ---------  	------------------------
    // 10/24/96  Rick Delgado			Initial Development
    // 01/20/97  Siraj Konkader		Completion
    // 04/15/97  Siraj Konkader		Allow update if logged on user owns monitored
    // 					activity or is at a supervisory level.
    // 05/30/97  JF. Caillouet			Change to Explicit Scrolling
    // 06/12/97  JF. Caillouet			Fix Field Highlighting
    // 06/00/97  R Grey			Test, Debug, Fix display and filter problems
    // 07/08/97  R Grey			Implement MONA Problem Memo design chngs
    // 12/30/98  SWSRKEH			Phase II changes
    // 10/06/99  SWSRCHF   	    H00076695	Change filter FDNC to work properly
    // 					Sort on FED_NON_COMPLIANCE_DATE
    // 02/22/00  SWSRCHF	    H00088582	MONA currently displays under all OSP's 
    // it
    // 					has ever been assigned to.
    // 10/02/00  SWSRCHF	    H00104550	MONA not displaying manually closed 
    // activities the day
    // 					that they are closed. The Moniored Activity is closed,
    // 					but the Monitored Activity Assignment is still open
    // 			    H00104139A	MONA displays under both the OLD and NEW
    // 					Service Provider
    // 11/25/02  SWSRKXD	    PR#148011	Fix Screen Help.
    // 09/23/11  GVandy	    CQ30438	Performance changes to DISPLAY Read Each's.
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // **** House Keeping - roll import ot export ****
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    local.NextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenActivity.Assign(import.HiddenActivity);
    MoveMonitoredActivity3(import.HiddenMonitoredActivity,
      export.HiddenMonitoredActivity);
    export.HiddenCode.CodeName = import.HiddenCode.CodeName;
    export.HiddenCodeValue.Cdvalue = import.HiddenCodeValue.Cdvalue;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenMonitoredActivityAssignment.ReasonCode =
      import.HiddenMonitoredActivityAssignment.ReasonCode;
    export.HideHdrServiceProvider.UserId = import.HideHdrServiceProvider.UserId;
    export.HideHdrFltrStrtRange.Date = import.HideHdrFltrStrtRange.Date;
    export.HideHdrFltrDteTyp.Text4 = import.HideHdrFltrDteTyp.Text4;
    export.HideHdrShowAll.OneChar = import.HideHdrShowAll.OneChar;
    MoveServiceProvider(import.HeaderServiceProvider,
      export.HeaderServiceProvider);
    export.HeaderPromptSvpo.PromptField = import.HeaderPromptSvpo.PromptField;
    export.HeaderOffice.Assign(import.HeaderOffice);
    MoveOfficeServiceProvider(import.HeaderOfficeServiceProvider,
      export.HeaderOfficeServiceProvider);
    MoveDateWorkArea(import.HeaderFilterStartRange,
      export.HeaderFilterStartRange);
    export.HeaderFilterDateType.Text4 = import.HeaderFilterDateType.Text4;
    export.HeaderPrompDateFilter.PromptField =
      import.HeaderPromptDateFilter.PromptField;
    export.HeaderShowAll.OneChar = import.HeaderShowAll.OneChar;

    if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
    {
    }
    else
    {
      export.HeaderShowAll.OneChar = "N";
    }

    export.UseNate.NextTransaction = "MONA";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.HeaderMonitoredActivityAssignment.ReasonCode =
      import.HeaderMonitoredActivityAssignment.ReasonCode;
    export.HeaderPromptMonActAsn.PromptField =
      import.HeaderPromptMonActAsn.PromptField;
    MoveInfrastructure5(import.HeaderInfrastructure, export.HeaderInfrastructure);
      
    MoveLegalAction(import.HeaderLegalAction, export.HeaderLegalAction);
    MoveFips(import.HeaderFips, export.HeaderFips);
    export.HeaderFipsTribAddress.Country = import.HeaderFipsTribAddress.Country;
    export.HideHdrMonitoredActivityAssignment.ReasonCode =
      import.HideHdrMonitoredActivityAssignment.ReasonCode;
    MoveInfrastructure4(import.HideHdrInfrastructure,
      export.HideHdrInfrastructure);
    export.HideHdrLegalAction.CourtCaseNumber =
      import.HideHdrLegalAction.CourtCaseNumber;
    MoveFips(import.HideHdrFips, export.HideHdrFips);
    export.HideHdrFipsTribAddress.Country =
      import.HideHdrFipsTribAddress.Country;
    local.Current.Date = Now().Date;
    local.High.Date = new DateTime(2099, 12, 31);

    // ****  Anything Changed ? ****
    if (!IsEmpty(export.HideHdrServiceProvider.UserId))
    {
      // **** If header filters changes - force a display on all screen commands
      // ****
      if (!Equal(export.HeaderServiceProvider.UserId,
        export.HideHdrServiceProvider.UserId))
      {
        local.Change.Flag = "Y";
      }
      else if (!Equal(export.HeaderFilterStartRange.Date,
        export.HideHdrFltrStrtRange.Date))
      {
        local.Change.Flag = "Y";
      }
      else if (!Equal(export.HeaderFilterDateType.Text4,
        export.HideHdrFltrDteTyp.Text4))
      {
        local.Change.Flag = "Y";
      }
      else if (AsChar(export.HeaderShowAll.OneChar) != AsChar
        (export.HideHdrShowAll.OneChar))
      {
        local.Change.Flag = "Y";
      }
      else if (!Equal(export.HeaderMonitoredActivityAssignment.ReasonCode,
        export.HideHdrMonitoredActivityAssignment.ReasonCode))
      {
        local.Change.Flag = "Y";
      }
      else if (!Equal(export.HeaderInfrastructure.CaseNumber,
        export.HideHdrInfrastructure.CaseNumber))
      {
        local.Change.Flag = "Y";
      }
      else if (!Equal(export.HeaderInfrastructure.CsePersonNumber,
        export.HideHdrInfrastructure.CsePersonNumber))
      {
        local.Change.Flag = "Y";
      }
      else if (!Equal(export.HeaderLegalAction.CourtCaseNumber,
        export.HideHdrLegalAction.CourtCaseNumber))
      {
        local.Change.Flag = "Y";
      }
      else if (!Equal(export.HeaderFips.StateAbbreviation,
        export.HideHdrFips.StateAbbreviation))
      {
        local.Change.Flag = "Y";
      }
      else if (!Equal(export.HeaderFips.CountyAbbreviation,
        export.HideHdrFips.CountyAbbreviation))
      {
        local.Change.Flag = "Y";
      }
      else if (!Equal(export.HeaderFipsTribAddress.Country,
        export.HideHdrFipsTribAddress.Country))
      {
        local.Change.Flag = "Y";
      }
    }

    // *** Move Page Keys and Scroll Message ***
    if (!import.Group.IsEmpty)
    {
      export.CurrentPage.Count = import.CurrentPage.Count;

      for(import.PageKeys.Index = 0; import.PageKeys.Index < import
        .PageKeys.Count; ++import.PageKeys.Index)
      {
        if (!import.PageKeys.CheckSize())
        {
          break;
        }

        export.PageKeys.Index = import.PageKeys.Index;
        export.PageKeys.CheckSize();

        export.PageKeys.Update.GexportPageKeyMonitoredActivity.
          SystemGeneratedIdentifier =
            import.PageKeys.Item.GimportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier;

        // *** 10/14/99 SWSRCHF
        // *** H00076695
        export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
          import.PageKeys.Item.GimportPageKeyDateWorkArea.Date;

        // *** Problem report H00104550
        // *** 10/02/00 SWSRCHF
        export.PageKeys.Update.GexportPageKeyMonitoredActivityAssignment.
          CreatedTimestamp =
            import.PageKeys.Item.GimportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp;
      }

      import.PageKeys.CheckIndex();
      export.Scroll.ScrollingMessage = import.Scroll.ScrollingMessage;
    }

    // ****  Call security if user tries to add, update, display. The procedure 
    // handles checking the authority of the user on delete command so there is
    // no need to handle it here. K Cole         ****
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "UPDATE"))
    {
      local.Case1.Number = export.HeaderInfrastructure.CaseNumber ?? Spaces(10);
      local.CsePerson.Number = export.HeaderInfrastructure.CsePersonNumber ?? Spaces
        (10);
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ****  Validate All Commands ****
    switch(TrimEnd(global.Command))
    {
      case "ADD":
        if (AsChar(local.Change.Flag) == 'Y')
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
        }

        break;
      case "ASIN":
        if (AsChar(local.Change.Flag) == 'Y')
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
        }

        break;
      case "ATLM":
        if (AsChar(local.Change.Flag) == 'Y')
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
        }

        break;
      case "DETAIL":
        if (AsChar(local.Change.Flag) == 'Y')
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
        }

        break;
      case "DISPLAY":
        export.CurrentPage.Count = 0;
        export.PageKeys.Count = 0;

        break;
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          local.NextTranInfo.Assign(import.HiddenNextTranInfo);
          local.NextTranInfo.CaseNumber =
            import.HeaderInfrastructure.CaseNumber ?? "";
          local.NextTranInfo.CsePersonNumber =
            import.HeaderInfrastructure.CsePersonNumber ?? "";
          local.NextTranInfo.CourtCaseNumber =
            import.HeaderLegalAction.CourtCaseNumber ?? "";
          local.NextTranInfo.LastTran = "MONA";

          // ---------------------------------------------
          // Set up local next_tran_info for saving the current values for the 
          // next screen
          // ---------------------------------------------
          UseScCabNextTranPut1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field1 = GetField(export.Standard, "nextTransaction");

            field1.Error = true;

            break;
          }

          return;
        }

        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        // List command is processed first, before any edits too.
        if (Equal(global.Command, "LIST"))
        {
          if (AsChar(export.HeaderPromptSvpo.PromptField) == 'S')
          {
            ExitState = "ECO_LNK_TO_LIST_SERVICE_PROVIDER";

            break;
          }

          if (AsChar(export.HeaderPrompDateFilter.PromptField) == 'S')
          {
            export.HiddenCode.CodeName = "MONITORED ACTIVITY DATE FILTER";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          }

          if (AsChar(export.HeaderPromptMonActAsn.PromptField) == 'S')
          {
            export.HiddenCode.CodeName = "MONA ASSIGNMENT REASON CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
        }

        break;
      case "NATE":
        if (AsChar(local.Change.Flag) == 'Y')
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
        }

        break;
      case "NEXT":
        if (AsChar(local.Change.Flag) == 'Y')
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

        export.PageKeys.Index = export.CurrentPage.Count - 1;
        export.PageKeys.CheckSize();

        if (export.PageKeys.Index == -1)
        {
          // ** First Time Thru **
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

        if (export.PageKeys.Index + 1 == export.PageKeys.Count)
        {
          ExitState = "SP0000_END_OF_SCROLL_LIST";

          break;
        }

        if (export.PageKeys.Index + 1 == Export.PageKeysGroup.Capacity)
        {
          ExitState = "SP0000_END_OF_SCROLL_LIST";

          break;
        }

        if (export.CurrentPage.Count == Export.PageKeysGroup.Capacity)
        {
          ExitState = "OE0000_LIST_IS_FULL";

          return;
        }

        // *** Increment Page Number and Group view ***
        ++export.CurrentPage.Count;

        export.PageKeys.Index = export.CurrentPage.Count - 1;
        export.PageKeys.CheckSize();

        export.Group.Count = 0;
        global.Command = "DISPLAY";

        break;
      case "PREV":
        if (AsChar(local.Change.Flag) == 'Y')
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

        if (export.CurrentPage.Count <= 1)
        {
          if (export.PageKeys.IsEmpty)
          {
            // ** First Time Thru **
            ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

            break;
          }

          ExitState = "ACO_NI0000_TOP_OF_LIST";

          break;
        }

        // *** Decrement Page Number and Key ***
        export.CurrentPage.Count = (int)((long)export.CurrentPage.Count - 1);

        export.PageKeys.Index = export.CurrentPage.Count - 1;
        export.PageKeys.CheckSize();

        export.Group.Index = -1;
        global.Command = "DISPLAY";

        break;
      case "RETASIN":
        break;
      case "RETATLM":
        break;
      case "RETCDVL":
        if (AsChar(export.HeaderPrompDateFilter.PromptField) == 'S')
        {
          if (!IsEmpty(export.HiddenCodeValue.Cdvalue))
          {
            export.HeaderFilterDateType.Text4 = export.HiddenCodeValue.Cdvalue;
            global.Command = "DISPLAY";
          }
          else
          {
            global.Command = "RDISPLAY";
          }

          export.HeaderPrompDateFilter.PromptField = "";

          var field1 = GetField(export.HeaderPrompDateFilter, "promptField");

          field1.Protected = false;
          field1.Focused = true;

          break;
        }

        if (AsChar(import.HeaderPromptMonActAsn.PromptField) == 'S')
        {
          if (!IsEmpty(export.HiddenCodeValue.Cdvalue))
          {
            export.HeaderMonitoredActivityAssignment.ReasonCode =
              export.HiddenCodeValue.Cdvalue;
            global.Command = "DISPLAY";
          }
          else
          {
            global.Command = "RDISPLAY";
          }

          export.HeaderPromptMonActAsn.PromptField = "";

          var field1 = GetField(export.HeaderPromptMonActAsn, "promptField");

          field1.Protected = false;
          field1.Focused = true;
        }

        break;
      case "RETNATE":
        break;
      case "RETSVPO":
        export.HeaderPromptSvpo.PromptField = "";

        var field = GetField(export.HeaderPromptSvpo, "promptField");

        field.Protected = false;
        field.Focused = true;

        if (Equal(export.HeaderServiceProvider.UserId,
          export.HideHdrServiceProvider.UserId) || IsEmpty
          (export.HeaderServiceProvider.UserId))
        {
          export.HeaderServiceProvider.UserId =
            export.HideHdrServiceProvider.UserId;
          global.Command = "RDISPLAY";
        }
        else
        {
          global.Command = "DISPLAY";
        }

        break;
      case "RETURN":
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT"))
        {
          global.NextTran = "SRPT " + "XXNEXTXX";

          return;
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "UPDATE":
        if (AsChar(local.Change.Flag) == 'Y')
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
        }

        break;
      case "XXFMMENU":
        break;
      case "XXNEXTXX":
        // ---------------------------------------------
        // User entered this screen from another screen
        // ---------------------------------------------
        UseScCabNextTranGet();
        local.NextTran.SystemGeneratedIdentifier =
          export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault();
        export.HeaderInfrastructure.CaseNumber =
          export.HiddenNextTranInfo.CaseNumber ?? "";
        export.HeaderInfrastructure.CsePersonNumber =
          export.HiddenNextTranInfo.CsePersonNumber ?? "";
        UseScNextStoreExtractOsp2();

        // ** Set Page_Keys to start Monitor Activities received from NEXT **
        export.CurrentPage.Count = 1;

        export.PageKeys.Index = 0;
        export.PageKeys.CheckSize();

        export.PageKeys.Update.GexportPageKeyMonitoredActivity.
          SystemGeneratedIdentifier =
            local.NextTranInfo.InfrastructureId.GetValueOrDefault();
        global.Command = "DISPLAY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    // *** Move Import Grp to Export Group ***
    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else if (!import.Group.IsEmpty)
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

        export.Group.Update.PromptReason.PromptField =
          import.Group.Item.PromptReason.PromptField;
        export.Group.Update.MonitoredActivity.Assign(
          import.Group.Item.MonitoredActivity);
        export.Group.Update.SpTextWorkArea.Text80 =
          import.Group.Item.SpTextWorkArea.Text80;
        MoveCsePersonsWorkSet(import.Group.Item.CsePersonsWorkSet,
          export.Group.Update.CsePersonsWorkSet);
        MoveInfrastructure2(import.Group.Item.Infrastructure,
          export.Group.Update.Infrastructure);
        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        MoveLegalAction(import.Group.Item.LegalAction,
          export.Group.Update.LegalAction);
        export.Group.Update.HighliteOldAsgnmt.Flag =
          import.Group.Item.HighliteOldAsgnmt.Flag;

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          local.Pointer.Count = export.Group.Index + 1;
        }

        if (AsChar(export.Group.Item.HighliteOldAsgnmt.Flag) == 'Y')
        {
          // ************************************************
          // Previously assigned Monitored Activities which have had the 
          // assignments discontinued are protected and displayed in Yellow.
          // ************************************************
          var field1 =
            GetField(export.Group.Item.MonitoredActivity, "typeCode");

          field1.Color = "yellow";
          field1.Protected = true;

          var field2 =
            GetField(export.Group.Item.MonitoredActivity, "fedNonComplianceDate");
            

          field2.Color = "yellow";
          field2.Protected = true;

          var field3 =
            GetField(export.Group.Item.MonitoredActivity, "startDate");

          field3.Color = "yellow";
          field3.Protected = true;

          var field4 =
            GetField(export.Group.Item.MonitoredActivity, "closureDate");

          field4.Color = "yellow";
          field4.Protected = true;

          var field5 =
            GetField(export.Group.Item.MonitoredActivity,
            "otherNonComplianceDate");

          field5.Color = "yellow";
          field5.Protected = true;

          var field6 =
            GetField(export.Group.Item.MonitoredActivity, "closureReasonCode");

          field6.Color = "yellow";
          field6.Protected = true;

          var field7 = GetField(export.Group.Item.PromptReason, "promptField");

          field7.Color = "yellow";
          field7.Protected = true;

          var field8 =
            GetField(export.Group.Item.MonitoredActivity, "createdBy");

          field8.Color = "yellow";
          field8.Protected = true;

          var field9 = GetField(export.Group.Item.Infrastructure, "caseNumber");

          field9.Color = "yellow";
          field9.Protected = true;

          var field10 =
            GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

          field10.Color = "yellow";
          field10.Protected = true;

          var field11 =
            GetField(export.Group.Item.Infrastructure, "csePersonNumber");

          field11.Color = "yellow";
          field11.Protected = true;

          var field12 = GetField(export.Group.Item.MonitoredActivity, "name");

          field12.Color = "yellow";
          field12.Protected = true;

          var field13 =
            GetField(export.Group.Item.CsePersonsWorkSet, "formattedName");

          field13.Color = "yellow";
          field13.Protected = true;

          var field14 = GetField(export.Group.Item.Infrastructure, "detail");

          field14.Color = "yellow";
          field14.Protected = true;

          var field15 =
            GetField(export.Group.Item.LegalAction, "courtCaseNumber");

          field15.Color = "yellow";
          field15.Protected = true;
        }
        else
        {
          // ***********************************************
          // Check FDNC and ONC and set colors for 'Open' Monitored Activities
          // If Near non compliance date is reached, set the corresponding
          // Non compliance date to yellow.
          // If Non compliance date is reached, set it to red.
          // ***********************************************
          if (!IsEmpty(export.Group.Item.MonitoredActivity.ClosureReasonCode) &&
            (
              Lt(export.Group.Item.MonitoredActivity.ClosureDate,
            local.Current.Date) && !
            Equal(export.Group.Item.MonitoredActivity.ClosureDate,
            local.Null1.Date) || Equal
            (export.Group.Item.MonitoredActivity.ClosureDate, local.High.Date)))
          {
            var field1 =
              GetField(export.Group.Item.MonitoredActivity, "closureReasonCode");
              

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.PromptReason, "promptField");

            field2.Color = "cyan";
            field2.Protected = true;
          }
          else
          {
            if (Equal(export.HeaderMonitoredActivityAssignment.ReasonCode, "INF"))
              
            {
              var field1 =
                GetField(export.Group.Item.MonitoredActivity,
                "closureReasonCode");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.PromptReason, "promptField");

              field2.Color = "cyan";
              field2.Highlighting = Highlighting.Underscore;
              field2.Protected = true;
            }

            if (!Lt(local.Current.Date,
              export.Group.Item.MonitoredActivity.FedNearNonComplDate) && !
              Equal(export.Group.Item.MonitoredActivity.FedNearNonComplDate,
              local.Null1.Date))
            {
              var field =
                GetField(export.Group.Item.MonitoredActivity,
                "fedNonComplianceDate");

              field.Color = "yellow";
              field.Protected = true;
            }

            if (!Lt(local.Current.Date,
              export.Group.Item.MonitoredActivity.OtherNearNonComplDate) && !
              Equal(export.Group.Item.MonitoredActivity.OtherNearNonComplDate,
              local.Null1.Date))
            {
              var field =
                GetField(export.Group.Item.MonitoredActivity,
                "otherNonComplianceDate");

              field.Color = "yellow";
              field.Protected = true;
            }

            if (!Lt(local.Current.Date,
              export.Group.Item.MonitoredActivity.FedNonComplianceDate) && !
              Equal(export.Group.Item.MonitoredActivity.FedNonComplianceDate,
              local.Null1.Date))
            {
              var field =
                GetField(export.Group.Item.MonitoredActivity,
                "fedNonComplianceDate");

              field.Color = "red";
              field.Protected = true;
            }

            if (!Lt(local.Current.Date,
              export.Group.Item.MonitoredActivity.OtherNonComplianceDate) && !
              Equal(export.Group.Item.MonitoredActivity.OtherNonComplianceDate,
              local.Null1.Date))
            {
              var field =
                GetField(export.Group.Item.MonitoredActivity,
                "otherNonComplianceDate");

              field.Color = "red";
              field.Protected = true;
            }
          }
        }
      }

      import.Group.CheckIndex();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***********************************************
    // Security for MONA is accomplished at the user id and screen transaction 
    // id the statement:
    // USE sc_cab_test_security is not required here.
    // ***********************************************
    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // For commands that need a row selected do those edits here.
    if (Equal(global.Command, "ADD") || Equal(global.Command, "ASIN") || Equal
      (global.Command, "ATLM") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DETAIL") || Equal(global.Command, "NATE") || Equal
      (global.Command, "LIST"))
    {
      if (AsChar(local.Change.Flag) == 'Y')
      {
        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        return;
      }

      if (export.Group.IsEmpty)
      {
        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        return;
      }

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
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Error = true;

            local.Count.Count += 10;
            MoveInfrastructure3(export.Group.Item.Infrastructure,
              export.SelectedInfrastructure);
            local.Pointer.Count = export.Group.Index + 1;

            break;
          case ' ':
            break;
          default:
            local.Count.Count += 10;

            var field2 = GetField(export.Group.Item.Common, "selectChar");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.Group.Item.PromptReason.PromptField))
        {
          case 'S':
            var field1 =
              GetField(export.Group.Item.PromptReason, "promptField");

            field1.Error = true;

            ++local.Count.Count;
            local.Pointer.Count = export.Group.Index + 1;

            break;
          case ' ':
            break;
          default:
            ++local.Count.Count;

            var field2 =
              GetField(export.Group.Item.PromptReason, "promptField");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }
      }

      export.Group.CheckIndex();

      switch(local.Count.Count)
      {
        case 0:
          // **** No Selection or prompt entered ****
          // **** Please make a selection with this function. ****
          ExitState = "ACO_NE0000_SEL_REQD_W_FUNCTION";

          return;
        case 1:
          // **** A prompt entered without an selection ****
          if (Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
          }

          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          return;
        case 10:
          // **** One selection made  ****
          if (Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            return;
          }

          export.Group.Index = local.Pointer.Count - 1;
          export.Group.CheckSize();

          var field1 = GetField(export.Group.Item.Common, "selectChar");

          field1.Protected = false;

          break;
        case 11:
          // **** One selection & One prompt made - good for LIST only  ****
          if (!Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

            return;
          }

          // **** Have to be on same record  ****
          export.Group.Index = local.Pointer.Count - 1;
          export.Group.CheckSize();

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S' && AsChar
            (export.Group.Item.PromptReason.PromptField) == 'S')
          {
            var field3 = GetField(export.Group.Item.Common, "selectChar");

            field3.Protected = false;

            var field4 =
              GetField(export.Group.Item.PromptReason, "promptField");

            field4.Protected = false;
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            return;
          }

          var field2 = GetField(export.Group.Item.Common, "selectChar");

          field2.Protected = false;

          break;
        default:
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "LIST":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        export.HiddenCode.CodeName = "MONITORED ACTIVITY CLOSE REASONS";
        ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

        break;
      case "RETCDVL":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.PromptReason.PromptField) == 'S')
          {
            export.Group.Update.PromptReason.PromptField = "";

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(export.HiddenCodeValue.Cdvalue))
            {
              export.Group.Update.MonitoredActivity.ClosureReasonCode =
                export.HiddenCodeValue.Cdvalue;
            }

            goto Test1;
          }
        }

        export.Group.CheckIndex();

        break;
      case "RETATLM":
        // Make sure an activity was selected by testing the return view from 
        // ATLM.
        if (!IsEmpty(export.HiddenActivity.TypeCode))
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              var field3 = GetField(export.Group.Item.Common, "selectChar");

              field3.Protected = false;
              field3.Focused = true;

              export.Group.Update.MonitoredActivity.TypeCode =
                export.HiddenActivity.TypeCode;

              if (!Equal(export.HiddenActivity.TypeCode, "MAN"))
              {
                var field =
                  GetField(export.Group.Item.MonitoredActivity, "typeCode");

                field.Error = true;

                ExitState = "SP0000_ONLY_MANUAL_ACTIVITY";

                return;
              }
              else
              {
                // In command = ADD, I can check if this field is populated to 
                // ensure that a correct activity has been selected for add.
                export.Group.Update.MonitoredActivity.ActivityControlNumber =
                  export.HiddenActivity.ControlNumber;
              }

              export.Group.Update.MonitoredActivity.CreatedBy = global.UserId;
              export.Group.Update.MonitoredActivity.Name =
                export.HiddenActivity.Name;

              var field4 =
                GetField(export.Group.Item.MonitoredActivity, "startDate");

              field4.Color = "green";
              field4.Highlighting = Highlighting.Underscore;
              field4.Protected = false;
              field4.Focused = true;

              var field5 =
                GetField(export.Group.Item.Infrastructure, "caseNumber");

              field5.Color = "green";
              field5.Highlighting = Highlighting.Underscore;
              field5.Protected = false;

              var field6 =
                GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

              field6.Color = "green";
              field6.Highlighting = Highlighting.Underscore;
              field6.Protected = false;

              var field7 =
                GetField(export.Group.Item.Infrastructure, "csePersonNumber");

              field7.Color = "green";
              field7.Highlighting = Highlighting.Underscore;
              field7.Protected = false;

              return;
            }
          }

          export.Group.CheckIndex();
        }
        else
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              export.Group.Update.Common.SelectChar = "";

              break;
            }
          }

          export.Group.CheckIndex();
        }

        break;
      case "DETAIL":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        if (export.Group.Item.MonitoredActivity.SystemGeneratedIdentifier == 0)
        {
          ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          break;
        }

        // Make sure there is a valid TRANCODE and it is not MONA.
        if (!IsEmpty(export.Group.Item.Infrastructure.UserId) && !
          Equal(export.Group.Item.Infrastructure.UserId, "MONA") && Length
          (TrimEnd(export.Group.Item.Infrastructure.UserId)) == 4)
        {
          local.NextTranInfo.InfrastructureId =
            export.Group.Item.Infrastructure.SystemGeneratedIdentifier;
          local.NextTranInfo.CaseNumber =
            export.Group.Item.Infrastructure.CaseNumber ?? "";
          local.NextTranInfo.CsePersonNumber =
            export.Group.Item.Infrastructure.CsePersonNumber ?? "";
          local.NextTranInfo.LegalActionIdentifier =
            (int?)export.Group.Item.Infrastructure.DenormNumeric12.
              GetValueOrDefault();
          local.NextTranInfo.MiscText1 =
            export.Group.Item.Infrastructure.DenormText12 ?? "";
          export.Standard.NextTransaction =
            export.Group.Item.Infrastructure.UserId;
          UseScNextStoreExtractOsp1();
          UseScCabNextTranPut2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "SP0000_INVALID_DETAIL_LINK";

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;
          }
        }
        else
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "SP0000_INVALID_DETAIL_LINK";
        }

        break;
      case "UPDATE":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        if (export.Group.Item.MonitoredActivity.SystemGeneratedIdentifier == 0)
        {
          // Make sure user is updating a non blank occurence.
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "SP0000_UPDATE_ON_EMPTY_ROW";

          return;
        }

        // Make sure update is valid.
        if (AsChar(export.Group.Item.HighliteOldAsgnmt.Flag) == 'Y')
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "INVALID_UPDATE";

          return;
        }

        if (ReadMonitoredActivity2())
        {
          if (!IsEmpty(entities.MonitoredActivity.ClosureReasonCode))
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "INVALID_UPDATE";

            return;
          }
        }
        else
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "SP0000_MONITORED_ACTIVITY_NF";

          return;
        }

        // ************************************************
        // If the Monitored Activity does not belong to the current user,
        // check if he/she has authority to delete.
        // ************************************************
        if (ReadServiceProvider1())
        {
          local.LoggedOnUser.UserId = global.UserId;

          if (!Equal(entities.ServiceProvider.UserId, local.LoggedOnUser.UserId) &&
            !
            Equal(entities.MonitoredActivity.CreatedBy,
            local.LoggedOnUser.UserId))
          {
            UseCoCabIsPersonSupervisor();

            if (AsChar(local.IsSupervisor.Flag) == 'N')
            {
              foreach(var item in ReadServiceProviderOfficeServiceProviderOffice2())
                
              {
                if (ReadServiceProviderOfficeServiceProviderOffice1())
                {
                  goto Read;
                }
              }

              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "CO0000_MUST_HAVE_SUPERVSRY_ROLE";

              return;
            }
          }
        }
        else
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_NF";

          return;
        }

Read:

        if (!IsEmpty(export.Group.Item.MonitoredActivity.ClosureReasonCode))
        {
          export.HiddenCodeValue.Cdvalue =
            export.Group.Item.MonitoredActivity.ClosureReasonCode ?? Spaces
            (10);
          export.HiddenCode.CodeName = "MONITORED ACTIVITY CLOSE REASONS";
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            var field =
              GetField(export.Group.Item.MonitoredActivity, "closureReasonCode");
              

            field.Error = true;
          }
        }
        else
        {
          export.Group.Update.MonitoredActivity.ClosureReasonCode = "MAN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Unprotect this field as it would have been protected.
          var field3 =
            GetField(export.Group.Item.MonitoredActivity, "closureReasonCode");

          field3.Color = "";
          field3.Highlighting = Highlighting.Normal;
          field3.Protected = false;
          field3.Focused = false;

          var field4 =
            GetField(export.Group.Item.MonitoredActivity, "closureReasonCode");

          field4.Error = true;

          var field5 = GetField(export.Group.Item.Common, "selectChar");

          field5.Error = true;

          return;
        }

        export.Group.Update.MonitoredActivity.ClosureDate = local.Current.Date;
        UseSpCabUpdateMonitoredActivity();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Group.Update.Common.SelectChar = "";

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Protected = false;
          field.Focused = true;

          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
          // Unprotect this field as it would have been protected.
          var field =
            GetField(export.Group.Item.MonitoredActivity, "closureReasonCode");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
        }

        break;
      case "ASIN":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        if (export.Group.Item.MonitoredActivity.SystemGeneratedIdentifier == 0)
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";
        }
        else
        {
          export.HiddenMonitoredActivity.SystemGeneratedIdentifier =
            export.Group.Item.MonitoredActivity.SystemGeneratedIdentifier;
          export.HiddenMonitoredActivity.TypeCode =
            export.Group.Item.MonitoredActivity.TypeCode ?? "";
          export.HiddenMonitoredActivity.Name =
            export.Group.Item.MonitoredActivity.Name;
          export.HiddenMonitoredActivity.CreatedBy =
            export.Group.Item.MonitoredActivity.CreatedBy;
          export.HiddenObject.Text20 = "MONITORED ACTIVITY";
          ExitState = "ECO_LNK_TO_ASIN";
        }

        break;
      case "RETASIN":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        export.Group.Update.Common.SelectChar = "";

        var field1 = GetField(export.Group.Item.Common, "selectChar");

        field1.Protected = false;
        field1.Focused = true;

        break;
      case "ATLM":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        // Make sure user selected blank occurrence for ATLM
        if (export.Group.Item.MonitoredActivity.SystemGeneratedIdentifier != 0)
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "CO0000_SELECT_BLANK_DETAIL_4_FN";

          break;
        }

        export.AtlmManualList.Flag = "Y";
        ExitState = "ECO_LNK_TO_ATLM";

        break;
      case "NATE":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        if (export.Group.Item.MonitoredActivity.SystemGeneratedIdentifier == 0)
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";

          break;
        }

        MoveCsePersonsWorkSet(export.Group.Item.CsePersonsWorkSet,
          export.SelectedCsePersonsWorkSet);
        MoveLegalAction(export.Group.Item.LegalAction,
          export.SelectedLegalAction);
        export.SelectedDateWorkArea.Timestamp =
          export.Group.Item.Infrastructure.CreatedTimestamp;
        export.SelectedDateWorkArea.Date =
          Date(export.Group.Item.Infrastructure.CreatedTimestamp);
        ExitState = "ECO_LNK_TO_NATE";

        break;
      case "RETNATE":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        export.Group.Update.Common.SelectChar = "";

        var field2 = GetField(export.Group.Item.Common, "selectChar");

        field2.Protected = false;
        field2.Focused = true;

        break;
      case "ADD":
        // Should have currency on OSP. Read is done (add & display) before CASE
        // command.
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        if (export.Group.Item.MonitoredActivity.ActivityControlNumber == 0 || export
          .Group.Item.MonitoredActivity.ActivityControlNumber != export
          .HiddenActivity.ControlNumber)
        {
          // Make sure user has selected an activity from ATLM.
          ExitState = "SP0000_SELECT_FROM_ATLM";

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          return;
        }

        if (Equal(export.Group.Item.MonitoredActivity.StartDate,
          local.Null1.Date))
        {
          export.Group.Update.MonitoredActivity.StartDate = local.Current.Date;
        }

        // Set null date to max date.
        if (Equal(export.Group.Item.MonitoredActivity.ClosureDate,
          local.Null1.Date))
        {
          local.DateWorkArea.Date =
            export.Group.Item.MonitoredActivity.ClosureDate;
          export.Group.Update.MonitoredActivity.ClosureDate =
            UseCabSetMaximumDiscontinueDate2();
        }

        if (IsEmpty(export.Group.Item.Infrastructure.CaseNumber))
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          var field3 = GetField(export.Group.Item.Common, "selectChar");

          field3.Error = true;

          var field4 = GetField(export.Group.Item.Infrastructure, "caseNumber");

          field4.Error = true;

          return;
        }

        if (!IsEmpty(export.Group.Item.Infrastructure.CaseNumber))
        {
          local.Case1.Number = export.Group.Item.Infrastructure.CaseNumber ?? Spaces
            (10);
          UseCabZeroFillNumber1();
          export.Group.Update.Infrastructure.CaseNumber = local.Case1.Number;
        }

        if (!ReadCase())
        {
          ExitState = "CASE_NF";

          var field3 = GetField(export.Group.Item.Common, "selectChar");

          field3.Error = true;

          var field4 = GetField(export.Group.Item.Infrastructure, "caseNumber");

          field4.Error = true;

          return;
        }

        if (!IsEmpty(export.Group.Item.Infrastructure.CsePersonNumber))
        {
          local.CsePerson.Number =
            export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);
          UseCabZeroFillNumber2();
          export.Group.Update.Infrastructure.CsePersonNumber =
            local.CsePerson.Number;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (ReadActivityActivityDetail())
        {
          if (ReadActivityStartStop())
          {
            if (ReadEventDetail())
            {
              if (!ReadEvent())
              {
                ExitState = "SP0000_EVENT_NF";

                return;
              }
            }
            else
            {
              ExitState = "SP0000_EVENT_DETAIL_NF";

              return;
            }
          }
          else
          {
            ExitState = "SP0000_ACTIVITY_START_STOP_NF";

            return;
          }

          export.HiddenActivityDetail.Assign(entities.ActivityDetail);
          export.HiddenActivity.Assign(entities.Activity);

          // Populate infrastructure.........
          export.Group.Update.Infrastructure.EventId =
            entities.Event1.ControlNumber;
          export.Group.Update.Infrastructure.BusinessObjectCd =
            entities.Event1.BusinessObjectCode;
          export.Group.Update.Infrastructure.EventType = entities.Event1.Type1;
          export.Group.Update.Infrastructure.EventDetailName =
            entities.EventDetail.DetailName;
          export.Group.Update.Infrastructure.Function =
            entities.EventDetail.Function;
          export.Group.Update.Infrastructure.InitiatingStateCode =
            entities.EventDetail.InitiatingStateCode;
          export.Group.Update.Infrastructure.ReasonCode =
            entities.EventDetail.ReasonCode;
          export.Group.Update.Infrastructure.CsenetInOutCode =
            entities.EventDetail.CsenetInOutCode;
          export.Group.Update.Infrastructure.UserId = "MONA";
          export.Group.Update.Infrastructure.SituationNumber = 0;

          if (AsChar(entities.EventDetail.LogToDiaryInd) == 'Y')
          {
            export.Group.Update.Infrastructure.ProcessStatus = "H";
          }
          else
          {
            export.Group.Update.Infrastructure.ProcessStatus = "P";
          }

          if (export.HiddenActivityDetail.OtherNonComplianceDays.
            GetValueOrDefault() != 0)
          {
            export.Group.Update.MonitoredActivity.OtherNonComplianceDate =
              AddDays(local.Current.Date,
              export.HiddenActivityDetail.OtherNonComplianceDays.
                GetValueOrDefault());
          }

          if (export.HiddenActivityDetail.OtherNearNonComplDays.
            GetValueOrDefault() != 0)
          {
            export.Group.Update.MonitoredActivity.OtherNearNonComplDate =
              AddDays(local.Current.Date,
              export.HiddenActivityDetail.OtherNearNonComplDays.
                GetValueOrDefault());
          }

          if (export.HiddenActivityDetail.FedNonComplianceDays.
            GetValueOrDefault() != 0)
          {
            export.Group.Update.MonitoredActivity.FedNonComplianceDate =
              AddDays(local.Current.Date,
              export.HiddenActivityDetail.FedNonComplianceDays.
                GetValueOrDefault());
          }

          if (export.HiddenActivityDetail.FedNearNonComplDays.
            GetValueOrDefault() != 0)
          {
            export.Group.Update.MonitoredActivity.FedNearNonComplDate =
              AddDays(local.Current.Date,
              export.HiddenActivityDetail.FedNearNonComplDays.
                GetValueOrDefault());
          }
        }
        else
        {
          ExitState = "SP0000_ACTIVITY_NF";

          return;
        }

        // To create this monitored activity,
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.SelectedInfrastructure.SystemGeneratedIdentifier =
          export.Group.Item.Infrastructure.SystemGeneratedIdentifier;
        UseCreateMonitoredActivity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.HiddenMonitoredActivityAssignment.DiscontinueDate =
          UseCabSetMaximumDiscontinueDate1();
        export.HiddenMonitoredActivityAssignment.EffectiveDate =
          local.Current.Date;
        export.HiddenMonitoredActivityAssignment.OverrideInd = "N";
        export.HiddenMonitoredActivityAssignment.ReasonCode = "RSP";
        UseSpCabCreateMonActAssignment();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        // Reset close date.
        if (!Equal(export.Group.Item.MonitoredActivity.ClosureDate,
          local.Null1.Date))
        {
          local.DateWorkArea.Date =
            export.Group.Item.MonitoredActivity.ClosureDate;
          export.Group.Update.MonitoredActivity.ClosureDate =
            UseCabSetMaximumDiscontinueDate2();
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_CREATE_OK";

          var field3 =
            GetField(export.Group.Item.Infrastructure, "csePersonNumber");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.Group.Item.Infrastructure, "caseNumber");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.Group.Item.MonitoredActivity, "startDate");

          field6.Color = "cyan";
          field6.Protected = true;

          export.Group.Update.Common.SelectChar = "";

          if (!IsEmpty(export.Group.Item.Infrastructure.CsePersonNumber))
          {
            export.Group.Update.CsePersonsWorkSet.Number =
              export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);
            UseSiReadCsePerson();
          }
        }

        break;
      default:
        break;
    }

Test1:

    // *** Set for Display ***
    if (Equal(global.Command, "DISPLAY"))
    {
      export.Group.Count = 0;

      if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
      {
        local.Case1.Number = export.HeaderInfrastructure.CaseNumber ?? Spaces
          (10);
        UseCabZeroFillNumber1();
        export.HeaderInfrastructure.CaseNumber = local.Case1.Number;
      }

      if (!IsEmpty(export.HeaderInfrastructure.CsePersonNumber))
      {
        local.CsePerson.Number =
          export.HeaderInfrastructure.CsePersonNumber ?? Spaces(10);
        UseCabZeroFillNumber2();
        export.HeaderInfrastructure.CsePersonNumber = local.CsePerson.Number;
      }

      if (Equal(export.HeaderFilterStartRange.Date, local.Null1.Date))
      {
        export.HeaderFilterStartRange.Date = AddMonths(local.Current.Date, -1);
      }

      if (export.HeaderServiceProvider.SystemGeneratedId == 0)
      {
        // ************************************************
        // LOOK HERE TO FIX RETURN
        // PROBLEM WITH 1ST OFFICE FOUND DISPLAYED
        // WHEN ORIGINAL OFFICE BEING WORKED IS 2ND OFFICE
        // ************************************************
        if (ReadServiceProvider3())
        {
          MoveServiceProvider(entities.ServiceProvider,
            export.HeaderServiceProvider);

          if (ReadOfficeServiceProviderOffice())
          {
            MoveOfficeServiceProvider(entities.OfficeServiceProvider,
              export.HeaderOfficeServiceProvider);
            MoveOffice(entities.Office, export.HeaderOffice);
          }

          if (!entities.OfficeServiceProvider.Populated)
          {
            ExitState = "OFFICE_SERVICE_PROVIDER_NF";

            return;
          }
        }
        else
        {
          ExitState = "SERVICE_PROVIDER_NF";

          return;
        }
      }
      else if (IsEmpty(export.HeaderServiceProvider.UserId))
      {
        // ************************************************
        // This happen on a return from a F15 detail -
        // SC_NEXT_STORE_EXRACT_OSP does not
        // store the USER-ID
        // ************************************************
        if (ReadServiceProvider2())
        {
          MoveServiceProvider(entities.ServiceProvider,
            export.HeaderServiceProvider);

          if (ReadOfficeServiceProviderOffice())
          {
            MoveOfficeServiceProvider(entities.OfficeServiceProvider,
              export.HeaderOfficeServiceProvider);
            MoveOffice(entities.Office, export.HeaderOffice);
          }

          if (!entities.OfficeServiceProvider.Populated)
          {
            ExitState = "OFFICE_SERVICE_PROVIDER_NF";

            return;
          }
        }
        else
        {
          ExitState = "SERVICE_PROVIDER_NF";

          return;
        }
      }

      // ***********************************************
      // Validate Code Values of display filter for:
      // 	Activity Date Type Code
      // 	Assignment Reason
      // ***********************************************
      if (IsEmpty(export.HeaderFilterDateType.Text4))
      {
        export.HeaderFilterDateType.Text4 = "STRT";
      }
      else
      {
        export.HiddenCode.CodeName = "MONITORED ACTIVITY DATE FILTER";
        export.HiddenCodeValue.Cdvalue = export.HeaderFilterDateType.Text4;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

          var field = GetField(export.HeaderFilterDateType, "text4");

          field.Error = true;

          return;
        }
      }

      if (IsEmpty(export.HeaderMonitoredActivityAssignment.ReasonCode))
      {
        export.HeaderMonitoredActivityAssignment.ReasonCode = "RSP";
      }
      else
      {
        export.HiddenCode.CodeName = "MONA ASSIGNMENT REASON CODE";
        export.HiddenCodeValue.Cdvalue =
          export.HeaderMonitoredActivityAssignment.ReasonCode;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

          var field =
            GetField(export.HeaderMonitoredActivityAssignment, "reasonCode");

          field.Error = true;

          return;
        }
      }

      // ***********************************************
      // Validate required FIPS Code Values for Legal Action display filters.
      // ***********************************************
      if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
      {
        // Either (State and County) or Country must be entered
        if (IsEmpty(export.HeaderFips.CountyAbbreviation) && IsEmpty
          (export.HeaderFips.StateAbbreviation) && IsEmpty
          (export.HeaderFipsTribAddress.Country))
        {
          ExitState = "SP0000_ENTER_ST_AND_CO_OR_CY";

          var field1 = GetField(export.HeaderFips, "countyAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.HeaderFips, "stateAbbreviation");

          field2.Error = true;

          var field3 = GetField(export.HeaderFipsTribAddress, "country");

          field3.Error = true;
        }

        if (IsEmpty(export.HeaderFips.CountyAbbreviation) && !
          IsEmpty(export.HeaderFips.StateAbbreviation))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.HeaderFips, "countyAbbreviation");

          field.Error = true;
        }

        if (!IsEmpty(export.HeaderFips.CountyAbbreviation) && IsEmpty
          (export.HeaderFips.StateAbbreviation))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.HeaderFips, "stateAbbreviation");

          field.Error = true;
        }

        if ((!IsEmpty(export.HeaderFips.CountyAbbreviation) || !
          IsEmpty(export.HeaderFips.StateAbbreviation)) && !
          IsEmpty(export.HeaderFipsTribAddress.Country))
        {
          ExitState = "SP0000_ENTER_ST_AND_CO_OR_CY";

          var field1 = GetField(export.HeaderFips, "countyAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.HeaderFips, "stateAbbreviation");

          field2.Error = true;

          var field3 = GetField(export.HeaderFipsTribAddress, "country");

          field3.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
      else
      {
        if (!IsEmpty(export.FilterFips.StateAbbreviation))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.FilterLegalAction, "courtCaseNumber");

          field.Error = true;

          if (IsEmpty(export.FilterFips.CountyAbbreviation))
          {
            var field1 = GetField(export.FilterFips, "countyAbbreviation");

            field1.Error = true;
          }

          return;
        }

        if (!IsEmpty(export.FilterFips.CountyAbbreviation))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field1 = GetField(export.FilterLegalAction, "courtCaseNumber");

          field1.Error = true;

          var field2 = GetField(export.FilterFips, "stateAbbreviation");

          field2.Error = true;

          return;
        }

        if (!IsEmpty(export.FilterFipsTribAddress.Country))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.FilterLegalAction, "courtCaseNumber");

          field.Error = true;

          return;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // *** For first time thru PRAD ***
      if (export.CurrentPage.Count == 0)
      {
        export.CurrentPage.Count = 1;

        export.PageKeys.Index = 0;
        export.PageKeys.CheckSize();
      }

      // ***********************************************
      // Populate Export Group View
      // ***********************************************
      export.Group.Index = -1;
      local.MonitoredActivityAssignment.EffectiveDate = local.Current.Date;
      local.MonitoredActivityAssignment.DiscontinueDate = local.Current.Date;

      switch(TrimEnd(export.HeaderFilterDateType.Text4))
      {
        case "STRT":
          // *** Problem report H00104550/H00104139A
          // *** 10/02/00 SWSRCHF
          // *** added check for Monitered Activity Assignment discontinue date 
          // not equal
          // *** '2099-12-31' and Monitored Activity closure date = '2099-12-31'
          // to the
          // *** READ statement
          // -- 09/23/11 GVandy CQ30438 Created 4 separate Read Each statements 
          // for each filter date type.
          //    These reads are optimized for Case, Person, Court Order Number, 
          // and blank search criteria.
          //    Use care when modifying these reads to insure that the index 
          // path is not altered.
          if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
          {
            // -- 09/23/2011 New access path with case search criteria is...
            // Access table SRCKTM1.CKT_INFRASTRUCTURE
            //        using index SRCKTM1.CKI02777 (1 COL)
            //        Intent share; Prefetch through page list;
            //        access degree is 12  pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_INFRASTRUCTU03
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_MONITORED_ACT
            //        using index SRCKTM1.CKI02721 (1 COL)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_MONITORED_AC02
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
            //        using index SRCKTM1.CKI01710 (5 COLS)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_ASSGN_MNT_AC01
            //        qualified for page range screening
            // Composite table is sorted for ORDER BY clause
            foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity15())
              
            {
              if (!IsEmpty(export.HeaderInfrastructure.CsePersonNumber))
              {
                if (!Equal(entities.Infrastructure.CsePersonNumber,
                  export.HeaderInfrastructure.CsePersonNumber))
                {
                  continue;
                }
              }

              if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
              {
                if (!Equal(entities.Infrastructure.BusinessObjectCd, "LEA"))
                {
                  continue;
                }
                else
                {
                  if (!IsEmpty(export.HeaderFips.StateAbbreviation))
                  {
                    // ***********************************************
                    // User has entered domestic court case number.
                    // ***********************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (ReadLegalAction1())
                    {
                      goto Test2;
                    }
                    else
                    {
                      continue;
                    }
                  }

                  if (!IsEmpty(export.HeaderFipsTribAddress.Country))
                  {
                    // ************************************************
                    // User has entered international court case number.
                    // ************************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (!ReadLegalAction2())
                    {
                      continue;
                    }
                  }
                }
              }

Test2:

              ++export.Group.Index;
              export.Group.CheckSize();

              if (export.Group.Index >= Export.GroupGroup.Capacity)
              {
                if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
                {
                  ++export.PageKeys.Index;
                  export.PageKeys.CheckSize();

                  export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                    SystemGeneratedIdentifier =
                      entities.MonitoredActivity.SystemGeneratedIdentifier;

                  // *** 10/14/99 SWSRCHF
                  // *** H00076695
                  export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                    entities.MonitoredActivity.StartDate;

                  // *** Problem report H00104550
                  // *** 10/02/00 SWSRCHF
                  export.PageKeys.Update.
                    GexportPageKeyMonitoredActivityAssignment.CreatedTimestamp =
                      entities.MonitoredActivityAssignment.CreatedTimestamp;
                }
                else
                {
                  ExitState = "SP0000_LIST_IS_FULL";
                }

                break;
              }

              export.Group.Update.MonitoredActivity.Assign(
                entities.MonitoredActivity);
              export.Group.Update.Infrastructure.
                Assign(entities.Infrastructure);

              if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
              {
                if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
                else
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                }
              }
              else if (!Lt(local.Current.Date,
                entities.MonitoredActivity.ClosureDate))
              {
                export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
              }
            }
          }
          else if (!IsEmpty(export.HeaderInfrastructure.CsePersonNumber))
          {
            // -- 09/23/2011 New access path with Person search criteria is...
            // Access table SRCKTM1.CKT_INFRASTRUCTURE
            //        using index SRCKTM1.CKI03777 (1 COL)
            //        Intent share; Prefetch through page list;
            //        access degree is 12  pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_INFRASTRUCTU03
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_MONITORED_ACT
            //        using index SRCKTM1.CKI02721 (1 COL)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_MONITORED_AC02
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
            //        using index SRCKTM1.CKI01710 (5 COLS)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_ASSGN_MNT_AC01
            //        qualified for page range screening
            // Composite table is sorted for ORDER BY clause
            foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity16())
              
            {
              if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
              {
                if (!Equal(entities.Infrastructure.BusinessObjectCd, "LEA"))
                {
                  continue;
                }
                else
                {
                  if (!IsEmpty(export.HeaderFips.StateAbbreviation))
                  {
                    // ***********************************************
                    // User has entered domestic court case number.
                    // ***********************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (ReadLegalAction1())
                    {
                      goto Test3;
                    }
                    else
                    {
                      continue;
                    }
                  }

                  if (!IsEmpty(export.HeaderFipsTribAddress.Country))
                  {
                    // ************************************************
                    // User has entered international court case number.
                    // ************************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (!ReadLegalAction2())
                    {
                      continue;
                    }
                  }
                }
              }

Test3:

              ++export.Group.Index;
              export.Group.CheckSize();

              if (export.Group.Index >= Export.GroupGroup.Capacity)
              {
                if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
                {
                  ++export.PageKeys.Index;
                  export.PageKeys.CheckSize();

                  export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                    SystemGeneratedIdentifier =
                      entities.MonitoredActivity.SystemGeneratedIdentifier;

                  // *** 10/14/99 SWSRCHF
                  // *** H00076695
                  export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                    entities.MonitoredActivity.StartDate;

                  // *** Problem report H00104550
                  // *** 10/02/00 SWSRCHF
                  export.PageKeys.Update.
                    GexportPageKeyMonitoredActivityAssignment.CreatedTimestamp =
                      entities.MonitoredActivityAssignment.CreatedTimestamp;
                }
                else
                {
                  ExitState = "SP0000_LIST_IS_FULL";
                }

                break;
              }

              export.Group.Update.MonitoredActivity.Assign(
                entities.MonitoredActivity);
              export.Group.Update.Infrastructure.
                Assign(entities.Infrastructure);

              if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
              {
                if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
                else
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                }
              }
              else if (!Lt(local.Current.Date,
                entities.MonitoredActivity.ClosureDate))
              {
                export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
              }
            }
          }
          else if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
          {
            if (!IsEmpty(export.HeaderFips.StateAbbreviation))
            {
              // -- 09/23/2011 New access path with Domestic Court Order Number 
              // search criteria is...
              // Access table SRCKTM1.CKT_FIPS
              //        using index SRCKTM1.CKI04320 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_FIPS04
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_TRIBUNAL
              //        using index SRCKTM1.CKI02330 (3 COLS)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_TRIBUNAL09
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_LEGAL_ACTION
              //        using index SRCKTM1.CKI06328 (2 COLS)
              //        Intent share;
              //        index only
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_LEGAL_ACTION05
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_INFRASTRUCTURE
              //        using index SRCKTM1.CKI09777 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_INFRASTRUCTU03
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_MONITORED_ACT
              //        using index SRCKTM1.CKI02721 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_MONITORED_AC02
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
              //        using index SRCKTM1.CKI01710 (5 COLS)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_ASSGN_MNT_AC01
              //        qualified for page range screening
              // Composite table is sorted for DISTINCT/UNIQUE clause, ORDER BY 
              // clause
              // -- Domestic tribunal.
              foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity13())
                
              {
                ++export.Group.Index;
                export.Group.CheckSize();

                if (export.Group.Index >= Export.GroupGroup.Capacity)
                {
                  if (export.PageKeys.Index + 1 != Export
                    .PageKeysGroup.Capacity)
                  {
                    ++export.PageKeys.Index;
                    export.PageKeys.CheckSize();

                    export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                      SystemGeneratedIdentifier =
                        entities.MonitoredActivity.SystemGeneratedIdentifier;

                    // *** 10/14/99 SWSRCHF
                    // *** H00076695
                    export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                      entities.MonitoredActivity.StartDate;

                    // *** Problem report H00104550
                    // *** 10/02/00 SWSRCHF
                    export.PageKeys.Update.
                      GexportPageKeyMonitoredActivityAssignment.
                        CreatedTimestamp =
                        entities.MonitoredActivityAssignment.CreatedTimestamp;
                  }
                  else
                  {
                    ExitState = "SP0000_LIST_IS_FULL";
                  }

                  break;
                }

                export.Group.Update.MonitoredActivity.Assign(
                  entities.MonitoredActivity);
                export.Group.Update.Infrastructure.Assign(
                  entities.Infrastructure);

                if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
                {
                  if (!Lt(local.Current.Date,
                    entities.MonitoredActivity.ClosureDate))
                  {
                    export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                  }
                  else
                  {
                    export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                  }
                }
                else if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
              }
            }
            else
            {
              // -- 09/23/2011 New access path with Foreign Court Order Number 
              // search criteria is...
              // Access table SRCKTM1.CKT_LEGAL_ACTION
              //        using index SRCKTM1.CKI06328 (2 COLS)
              //        Intent share;
              //        index only
              //        access degree is 0   pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_LEGAL_ACTION05
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_INFRASTRUCTURE
              //        using index SRCKTM1.CKI09777 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_INFRASTRUCTU03
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_MONITORED_ACT
              //        using index SRCKTM1.CKI02721 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_MONITORED_AC02
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
              //        using index SRCKTM1.CKI01710 (5 COLS)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_ASSGN_MNT_AC01
              //        qualified for page range screening
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_FIPS_TRIB_ADDR
              //        using index SRCKTM1.CKI03304 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_FIPS_TRIB_AD04
              // Composite table is sorted for DISTINCT/UNIQUE clause, ORDER BY 
              // clause
              // -- Foreign tribunal.
              foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity14())
                
              {
                ++export.Group.Index;
                export.Group.CheckSize();

                if (export.Group.Index >= Export.GroupGroup.Capacity)
                {
                  if (export.PageKeys.Index + 1 != Export
                    .PageKeysGroup.Capacity)
                  {
                    ++export.PageKeys.Index;
                    export.PageKeys.CheckSize();

                    export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                      SystemGeneratedIdentifier =
                        entities.MonitoredActivity.SystemGeneratedIdentifier;

                    // *** 10/14/99 SWSRCHF
                    // *** H00076695
                    export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                      entities.MonitoredActivity.StartDate;

                    // *** Problem report H00104550
                    // *** 10/02/00 SWSRCHF
                    export.PageKeys.Update.
                      GexportPageKeyMonitoredActivityAssignment.
                        CreatedTimestamp =
                        entities.MonitoredActivityAssignment.CreatedTimestamp;
                  }
                  else
                  {
                    ExitState = "SP0000_LIST_IS_FULL";
                  }

                  break;
                }

                export.Group.Update.MonitoredActivity.Assign(
                  entities.MonitoredActivity);
                export.Group.Update.Infrastructure.Assign(
                  entities.Infrastructure);

                if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
                {
                  if (!Lt(local.Current.Date,
                    entities.MonitoredActivity.ClosureDate))
                  {
                    export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                  }
                  else
                  {
                    export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                  }
                }
                else if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
              }
            }
          }
          else
          {
            // -- 09/23/2011 New access path with no search criteria is...
            // Access table SRCKTM1.CKT_ASSGN_MNT_ACT
            //        using index SRCKTM1.CKI02710 (4 COLS)
            //        Intent share; Sequential prefetch;
            //        index only
            //        access degree is 0   pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_ASSGN_MNT_AC02
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_MONITORED_ACT
            //        using index SRCKTM1.CKI05721 (2 COLS)
            //        Intent share;
            //        index only
            //        access degree is 0   pgroup is  1
            //        join degree is 0     pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_MONITORED_AC01
            // Composite table is sorted for ORDER BY clause
            foreach(var item in ReadMonitoredActivityMonitoredActivityAssignment())
              
            {
              if (!ReadMonitoredActivity1())
              {
                // -- Should never happen
                continue;
              }

              if (!ReadInfrastructure())
              {
                // -- Should never happen
                continue;
              }

              ++export.Group.Index;
              export.Group.CheckSize();

              if (export.Group.Index >= Export.GroupGroup.Capacity)
              {
                if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
                {
                  ++export.PageKeys.Index;
                  export.PageKeys.CheckSize();

                  export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                    SystemGeneratedIdentifier =
                      entities.MonitoredActivity.SystemGeneratedIdentifier;

                  // *** 10/14/99 SWSRCHF
                  // *** H00076695
                  export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                    entities.MonitoredActivity.StartDate;

                  // *** Problem report H00104550
                  // *** 10/02/00 SWSRCHF
                  export.PageKeys.Update.
                    GexportPageKeyMonitoredActivityAssignment.CreatedTimestamp =
                      entities.Cki02710.CreatedTimestamp;
                }
                else
                {
                  ExitState = "SP0000_LIST_IS_FULL";
                }

                break;
              }

              export.Group.Update.MonitoredActivity.Assign(
                entities.MonitoredActivity);
              export.Group.Update.Infrastructure.
                Assign(entities.Infrastructure);

              if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
              {
                if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
                else
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                }
              }
              else if (!Lt(local.Current.Date,
                entities.MonitoredActivity.ClosureDate))
              {
                export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
              }
            }
          }

          // -- 09/23/2011  Original read each commented out below.
          break;
        case "CLOS":
          // *** Problem report H00104550/H00104139A
          // *** 10/02/00 SWSRCHF
          // *** added check for Monitered Activity Assignment discontinue date 
          // not equal
          // *** '2099-12-31' and Monitored Activity closure date = '2099-12-31'
          // to the
          // *** READ statement
          // -- 09/23/11 GVandy CQ30438 Created 4 separate Read Each statements 
          // for each filter date type.
          //    These reads are optimized for Case, Person, Court Order Number, 
          // and blank search criteria.
          //    Use care when modifying these reads to insure that the index 
          // path is not altered.
          if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
          {
            // -- 09/23/2011 New access path with case search criteria is...
            // Access table SRCKTM1.CKT_INFRASTRUCTURE
            //        using index SRCKTM1.CKI02777 (1 COL)
            //        Intent share; Prefetch through page list;
            //        access degree is 12  pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_INFRASTRUCTU03
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_MONITORED_ACT
            //        using index SRCKTM1.CKI02721 (1 COL)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_MONITORED_AC02
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
            //        using index SRCKTM1.CKI01710 (5 COLS)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_ASSGN_MNT_AC01
            //        qualified for page range screening
            // Composite table is sorted for ORDER BY clause
            foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity3())
              
            {
              if (!IsEmpty(export.HeaderInfrastructure.CsePersonNumber))
              {
                if (!Equal(entities.Infrastructure.CsePersonNumber,
                  export.HeaderInfrastructure.CsePersonNumber))
                {
                  continue;
                }
              }

              if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
              {
                if (!Equal(entities.Infrastructure.BusinessObjectCd, "LEA"))
                {
                  continue;
                }
                else
                {
                  if (!IsEmpty(export.HeaderFips.StateAbbreviation))
                  {
                    // ***********************************************
                    // User has entered domestic court case number.
                    // ***********************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (ReadLegalAction1())
                    {
                      goto Test4;
                    }
                    else
                    {
                      continue;
                    }
                  }

                  if (!IsEmpty(export.HeaderFipsTribAddress.Country))
                  {
                    // ************************************************
                    // User has entered international court case number.
                    // ************************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (!ReadLegalAction2())
                    {
                      continue;
                    }
                  }
                }
              }

Test4:

              ++export.Group.Index;
              export.Group.CheckSize();

              if (export.Group.Index >= Export.GroupGroup.Capacity)
              {
                if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
                {
                  ++export.PageKeys.Index;
                  export.PageKeys.CheckSize();

                  export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                    SystemGeneratedIdentifier =
                      entities.MonitoredActivity.SystemGeneratedIdentifier;

                  // *** 10/14/99 SWSRCHF
                  // *** H00076695
                  export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                    entities.MonitoredActivity.ClosureDate;

                  // *** Problem report H00104550
                  // *** 10/02/00 SWSRCHF
                  export.PageKeys.Update.
                    GexportPageKeyMonitoredActivityAssignment.CreatedTimestamp =
                      entities.MonitoredActivityAssignment.CreatedTimestamp;
                }
                else
                {
                  ExitState = "SP0000_LIST_IS_FULL";
                }

                break;
              }

              export.Group.Update.MonitoredActivity.Assign(
                entities.MonitoredActivity);
              export.Group.Update.Infrastructure.
                Assign(entities.Infrastructure);

              // ************************************************
              // Disallow updates to closed Monitored Activities
              // ************************************************
              var field1 =
                GetField(export.Group.Item.MonitoredActivity, "startDate");

              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.MonitoredActivity, "closureDate");

              field2.Protected = true;

              var field3 =
                GetField(export.Group.Item.MonitoredActivity,
                "closureReasonCode");

              field3.Protected = true;

              var field4 =
                GetField(export.Group.Item.PromptReason, "promptField");

              field4.Protected = true;

              var field5 =
                GetField(export.Group.Item.Infrastructure, "caseNumber");

              field5.Protected = true;

              var field6 =
                GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

              field6.Protected = true;

              var field7 =
                GetField(export.Group.Item.Infrastructure, "csePersonNumber");

              field7.Protected = true;

              // *** Problem report H00104550
              // *** 10/02/00 SWSRCHF
              export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
            }
          }
          else if (!IsEmpty(export.HeaderInfrastructure.CsePersonNumber))
          {
            // -- 09/23/2011 New access path with Person search criteria is...
            // Access table SRCKTM1.CKT_INFRASTRUCTURE
            //        using index SRCKTM1.CKI03777 (1 COL)
            //        Intent share; Prefetch through page list;
            //        access degree is 12  pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_INFRASTRUCTU03
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_MONITORED_ACT
            //        using index SRCKTM1.CKI02721 (1 COL)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_MONITORED_AC02
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
            //        using index SRCKTM1.CKI01710 (5 COLS)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_ASSGN_MNT_AC01
            //        qualified for page range screening
            // Composite table is sorted for ORDER BY clause
            foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity4())
              
            {
              if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
              {
                if (!Equal(entities.Infrastructure.BusinessObjectCd, "LEA"))
                {
                  continue;
                }
                else
                {
                  if (!IsEmpty(export.HeaderFips.StateAbbreviation))
                  {
                    // ***********************************************
                    // User has entered domestic court case number.
                    // ***********************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (ReadLegalAction1())
                    {
                      goto Test5;
                    }
                    else
                    {
                      continue;
                    }
                  }

                  if (!IsEmpty(export.HeaderFipsTribAddress.Country))
                  {
                    // ************************************************
                    // User has entered international court case number.
                    // ************************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (!ReadLegalAction2())
                    {
                      continue;
                    }
                  }
                }
              }

Test5:

              ++export.Group.Index;
              export.Group.CheckSize();

              if (export.Group.Index >= Export.GroupGroup.Capacity)
              {
                if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
                {
                  ++export.PageKeys.Index;
                  export.PageKeys.CheckSize();

                  export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                    SystemGeneratedIdentifier =
                      entities.MonitoredActivity.SystemGeneratedIdentifier;

                  // *** 10/14/99 SWSRCHF
                  // *** H00076695
                  export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                    entities.MonitoredActivity.ClosureDate;

                  // *** Problem report H00104550
                  // *** 10/02/00 SWSRCHF
                  export.PageKeys.Update.
                    GexportPageKeyMonitoredActivityAssignment.CreatedTimestamp =
                      entities.MonitoredActivityAssignment.CreatedTimestamp;
                }
                else
                {
                  ExitState = "SP0000_LIST_IS_FULL";
                }

                break;
              }

              export.Group.Update.MonitoredActivity.Assign(
                entities.MonitoredActivity);
              export.Group.Update.Infrastructure.
                Assign(entities.Infrastructure);

              // ************************************************
              // Disallow updates to closed Monitored Activities
              // ************************************************
              var field1 =
                GetField(export.Group.Item.MonitoredActivity, "startDate");

              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.MonitoredActivity, "closureDate");

              field2.Protected = true;

              var field3 =
                GetField(export.Group.Item.MonitoredActivity,
                "closureReasonCode");

              field3.Protected = true;

              var field4 =
                GetField(export.Group.Item.PromptReason, "promptField");

              field4.Protected = true;

              var field5 =
                GetField(export.Group.Item.Infrastructure, "caseNumber");

              field5.Protected = true;

              var field6 =
                GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

              field6.Protected = true;

              var field7 =
                GetField(export.Group.Item.Infrastructure, "csePersonNumber");

              field7.Protected = true;

              // *** Problem report H00104550
              // *** 10/02/00 SWSRCHF
              export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
            }
          }
          else if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
          {
            if (!IsEmpty(export.HeaderFips.StateAbbreviation))
            {
              // -- 09/23/2011 New access path with Domestic Court Order Number 
              // search criteria is...
              // Access table SRCKTM1.CKT_FIPS
              //        using index SRCKTM1.CKI04320 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_FIPS04
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_TRIBUNAL
              //        using index SRCKTM1.CKI02330 (3 COLS)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_TRIBUNAL09
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_LEGAL_ACTION
              //        using index SRCKTM1.CKI06328 (2 COLS)
              //        Intent share;
              //        index only
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_LEGAL_ACTION05
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_INFRASTRUCTURE
              //        using index SRCKTM1.CKI09777 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_INFRASTRUCTU03
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_MONITORED_ACT
              //        using index SRCKTM1.CKI02721 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_MONITORED_AC02
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
              //        using index SRCKTM1.CKI01710 (5 COLS)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_ASSGN_MNT_AC01
              //        qualified for page range screening
              // Composite table is sorted for DISTINCT/UNIQUE clause, ORDER BY 
              // clause
              // -- Domestic tribunal.
              foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity1())
                
              {
                ++export.Group.Index;
                export.Group.CheckSize();

                if (export.Group.Index >= Export.GroupGroup.Capacity)
                {
                  if (export.PageKeys.Index + 1 != Export
                    .PageKeysGroup.Capacity)
                  {
                    ++export.PageKeys.Index;
                    export.PageKeys.CheckSize();

                    export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                      SystemGeneratedIdentifier =
                        entities.MonitoredActivity.SystemGeneratedIdentifier;

                    // *** 10/14/99 SWSRCHF
                    // *** H00076695
                    export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                      entities.MonitoredActivity.ClosureDate;

                    // *** Problem report H00104550
                    // *** 10/02/00 SWSRCHF
                    export.PageKeys.Update.
                      GexportPageKeyMonitoredActivityAssignment.
                        CreatedTimestamp =
                        entities.MonitoredActivityAssignment.CreatedTimestamp;
                  }
                  else
                  {
                    ExitState = "SP0000_LIST_IS_FULL";
                  }

                  break;
                }

                export.Group.Update.MonitoredActivity.Assign(
                  entities.MonitoredActivity);
                export.Group.Update.Infrastructure.Assign(
                  entities.Infrastructure);

                // ************************************************
                // Disallow updates to closed Monitored Activities
                // ************************************************
                var field1 =
                  GetField(export.Group.Item.MonitoredActivity, "startDate");

                field1.Protected = true;

                var field2 =
                  GetField(export.Group.Item.MonitoredActivity, "closureDate");

                field2.Protected = true;

                var field3 =
                  GetField(export.Group.Item.MonitoredActivity,
                  "closureReasonCode");

                field3.Protected = true;

                var field4 =
                  GetField(export.Group.Item.PromptReason, "promptField");

                field4.Protected = true;

                var field5 =
                  GetField(export.Group.Item.Infrastructure, "caseNumber");

                field5.Protected = true;

                var field6 =
                  GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

                field6.Protected = true;

                var field7 =
                  GetField(export.Group.Item.Infrastructure, "csePersonNumber");
                  

                field7.Protected = true;

                // *** Problem report H00104550
                // *** 10/02/00 SWSRCHF
                export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
              }
            }
            else
            {
              // -- 09/23/2011 New access path with Foreign Court Order Number 
              // search criteria is...
              // Access table SRCKTM1.CKT_LEGAL_ACTION
              //        using index SRCKTM1.CKI06328 (2 COLS)
              //        Intent share;
              //        index only
              //        access degree is 0   pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_LEGAL_ACTION05
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_INFRASTRUCTURE
              //        using index SRCKTM1.CKI09777 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_INFRASTRUCTU03
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_MONITORED_ACT
              //        using index SRCKTM1.CKI02721 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_MONITORED_AC02
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
              //        using index SRCKTM1.CKI01710 (5 COLS)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_ASSGN_MNT_AC01
              //        qualified for page range screening
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_FIPS_TRIB_ADDR
              //        using index SRCKTM1.CKI03304 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_FIPS_TRIB_AD04
              // Composite table is sorted for DISTINCT/UNIQUE clause, ORDER BY 
              // clause
              // -- Foreign tribunal.
              foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity2())
                
              {
                ++export.Group.Index;
                export.Group.CheckSize();

                if (export.Group.Index >= Export.GroupGroup.Capacity)
                {
                  if (export.PageKeys.Index + 1 != Export
                    .PageKeysGroup.Capacity)
                  {
                    ++export.PageKeys.Index;
                    export.PageKeys.CheckSize();

                    export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                      SystemGeneratedIdentifier =
                        entities.MonitoredActivity.SystemGeneratedIdentifier;

                    // *** 10/14/99 SWSRCHF
                    // *** H00076695
                    export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                      entities.MonitoredActivity.ClosureDate;

                    // *** Problem report H00104550
                    // *** 10/02/00 SWSRCHF
                    export.PageKeys.Update.
                      GexportPageKeyMonitoredActivityAssignment.
                        CreatedTimestamp =
                        entities.MonitoredActivityAssignment.CreatedTimestamp;
                  }
                  else
                  {
                    ExitState = "SP0000_LIST_IS_FULL";
                  }

                  break;
                }

                export.Group.Update.MonitoredActivity.Assign(
                  entities.MonitoredActivity);
                export.Group.Update.Infrastructure.Assign(
                  entities.Infrastructure);

                // ************************************************
                // Disallow updates to closed Monitored Activities
                // ************************************************
                var field1 =
                  GetField(export.Group.Item.MonitoredActivity, "startDate");

                field1.Protected = true;

                var field2 =
                  GetField(export.Group.Item.MonitoredActivity, "closureDate");

                field2.Protected = true;

                var field3 =
                  GetField(export.Group.Item.MonitoredActivity,
                  "closureReasonCode");

                field3.Protected = true;

                var field4 =
                  GetField(export.Group.Item.PromptReason, "promptField");

                field4.Protected = true;

                var field5 =
                  GetField(export.Group.Item.Infrastructure, "caseNumber");

                field5.Protected = true;

                var field6 =
                  GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

                field6.Protected = true;

                var field7 =
                  GetField(export.Group.Item.Infrastructure, "csePersonNumber");
                  

                field7.Protected = true;

                // *** Problem report H00104550
                // *** 10/02/00 SWSRCHF
                export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
              }
            }
          }
          else
          {
            // -- 09/23/2011 New access path with no search criteria is...
            // Access table SRCKTM1.CKT_ASSGN_MNT_ACT
            //        using index SRCKTM1.CKI02710 (4 COLS)
            //        Intent share; Sequential prefetch;
            //        index only
            //        access degree is 0   pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_ASSGN_MNT_AC02
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_MONITORED_ACT
            //        using index SRCKTM1.CKI05721 (2 COLS)
            //        Intent share;
            //        index only
            //        access degree is 0   pgroup is  1
            //        join degree is 0     pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_MONITORED_AC01
            // Composite table is sorted for ORDER BY clause
            foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity17())
              
            {
              if (!ReadMonitoredActivity1())
              {
                // -- Should never happen
                continue;
              }

              if (!ReadInfrastructure())
              {
                // -- Should never happen
                continue;
              }

              ++export.Group.Index;
              export.Group.CheckSize();

              if (export.Group.Index >= Export.GroupGroup.Capacity)
              {
                if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
                {
                  ++export.PageKeys.Index;
                  export.PageKeys.CheckSize();

                  export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                    SystemGeneratedIdentifier =
                      entities.MonitoredActivity.SystemGeneratedIdentifier;

                  // *** 10/14/99 SWSRCHF
                  // *** H00076695
                  export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                    entities.MonitoredActivity.ClosureDate;

                  // *** Problem report H00104550
                  // *** 10/02/00 SWSRCHF
                  export.PageKeys.Update.
                    GexportPageKeyMonitoredActivityAssignment.CreatedTimestamp =
                      entities.Cki02710.CreatedTimestamp;
                }
                else
                {
                  ExitState = "SP0000_LIST_IS_FULL";
                }

                break;
              }

              export.Group.Update.MonitoredActivity.Assign(
                entities.MonitoredActivity);
              export.Group.Update.Infrastructure.
                Assign(entities.Infrastructure);

              // ************************************************
              // Disallow updates to closed Monitored Activities
              // ************************************************
              var field1 =
                GetField(export.Group.Item.MonitoredActivity, "startDate");

              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.MonitoredActivity, "closureDate");

              field2.Protected = true;

              var field3 =
                GetField(export.Group.Item.MonitoredActivity,
                "closureReasonCode");

              field3.Protected = true;

              var field4 =
                GetField(export.Group.Item.PromptReason, "promptField");

              field4.Protected = true;

              var field5 =
                GetField(export.Group.Item.Infrastructure, "caseNumber");

              field5.Protected = true;

              var field6 =
                GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

              field6.Protected = true;

              var field7 =
                GetField(export.Group.Item.Infrastructure, "csePersonNumber");

              field7.Protected = true;

              // *** Problem report H00104550
              // *** 10/02/00 SWSRCHF
              export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
            }
          }

          // -- 09/23/2011  Original read each commented out below.
          break;
        case "FDNC":
          // *** Problem report H00104550/H00104139A
          // *** 10/02/00 SWSRCHF
          // *** added check for Monitered Activity Assignment discontinue date 
          // not equal
          // *** '2099-12-31' and Monitored Activity closure date = '2099-12-31'
          // to the
          // *** READ statement
          // -- 09/23/11 GVandy CQ30438 Created 4 separate Read Each statements 
          // for each filter date type.
          //    These reads are optimized for Case, Person, Court Order Number, 
          // and blank search criteria.
          //    Use care when modifying these reads to insure that the index 
          // path is not altered.
          if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
          {
            // -- 09/23/2011 New access path with case search criteria is...
            // Access table SRCKTM1.CKT_INFRASTRUCTURE
            //        using index SRCKTM1.CKI02777 (1 COL)
            //        Intent share; Prefetch through page list;
            //        access degree is 12  pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_INFRASTRUCTU03
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_MONITORED_ACT
            //        using index SRCKTM1.CKI02721 (1 COL)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_MONITORED_AC02
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
            //        using index SRCKTM1.CKI01710 (5 COLS)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_ASSGN_MNT_AC01
            //        qualified for page range screening
            // Composite table is sorted for ORDER BY clause
            foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity7())
              
            {
              if (!IsEmpty(export.HeaderInfrastructure.CsePersonNumber))
              {
                if (!Equal(entities.Infrastructure.CsePersonNumber,
                  export.HeaderInfrastructure.CsePersonNumber))
                {
                  continue;
                }
              }

              if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
              {
                if (!Equal(entities.Infrastructure.BusinessObjectCd, "LEA"))
                {
                  continue;
                }
                else
                {
                  if (!IsEmpty(export.HeaderFips.StateAbbreviation))
                  {
                    // ***********************************************
                    // User has entered domestic court case number.
                    // ***********************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (ReadLegalAction1())
                    {
                      goto Test6;
                    }
                    else
                    {
                      continue;
                    }
                  }

                  if (!IsEmpty(export.HeaderFipsTribAddress.Country))
                  {
                    // ************************************************
                    // User has entered international court case number.
                    // ************************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (!ReadLegalAction2())
                    {
                      continue;
                    }
                  }
                }
              }

Test6:

              ++export.Group.Index;
              export.Group.CheckSize();

              if (export.Group.Index >= Export.GroupGroup.Capacity)
              {
                if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
                {
                  ++export.PageKeys.Index;
                  export.PageKeys.CheckSize();

                  export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                    SystemGeneratedIdentifier =
                      entities.MonitoredActivity.SystemGeneratedIdentifier;

                  // *** 10/14/99 SWSRCHF
                  // *** H00076695
                  export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                    entities.MonitoredActivity.FedNonComplianceDate;

                  // *** Problem report H00104550
                  // *** 10/02/00 SWSRCHF
                  export.PageKeys.Update.
                    GexportPageKeyMonitoredActivityAssignment.CreatedTimestamp =
                      entities.MonitoredActivityAssignment.CreatedTimestamp;
                }
                else
                {
                  ExitState = "SP0000_LIST_IS_FULL";
                }

                break;
              }

              export.Group.Update.MonitoredActivity.Assign(
                entities.MonitoredActivity);
              export.Group.Update.Infrastructure.
                Assign(entities.Infrastructure);

              if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
              {
                if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
                else
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                }
              }
              else if (!Lt(local.Current.Date,
                entities.MonitoredActivity.ClosureDate))
              {
                export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
              }
            }
          }
          else if (!IsEmpty(export.HeaderInfrastructure.CsePersonNumber))
          {
            // -- 09/23/2011 New access path with Person search criteria is...
            // Access table SRCKTM1.CKT_INFRASTRUCTURE
            //        using index SRCKTM1.CKI03777 (1 COL)
            //        Intent share; Prefetch through page list;
            //        access degree is 12  pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_INFRASTRUCTU03
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_MONITORED_ACT
            //        using index SRCKTM1.CKI02721 (1 COL)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_MONITORED_AC02
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
            //        using index SRCKTM1.CKI01710 (5 COLS)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_ASSGN_MNT_AC01
            //        qualified for page range screening
            // Composite table is sorted for ORDER BY clause
            foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity8())
              
            {
              if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
              {
                if (!Equal(entities.Infrastructure.BusinessObjectCd, "LEA"))
                {
                  continue;
                }
                else
                {
                  if (!IsEmpty(export.HeaderFips.StateAbbreviation))
                  {
                    // ***********************************************
                    // User has entered domestic court case number.
                    // ***********************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (ReadLegalAction1())
                    {
                      goto Test7;
                    }
                    else
                    {
                      continue;
                    }
                  }

                  if (!IsEmpty(export.HeaderFipsTribAddress.Country))
                  {
                    // ************************************************
                    // User has entered international court case number.
                    // ************************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (!ReadLegalAction2())
                    {
                      continue;
                    }
                  }
                }
              }

Test7:

              ++export.Group.Index;
              export.Group.CheckSize();

              if (export.Group.Index >= Export.GroupGroup.Capacity)
              {
                if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
                {
                  ++export.PageKeys.Index;
                  export.PageKeys.CheckSize();

                  export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                    SystemGeneratedIdentifier =
                      entities.MonitoredActivity.SystemGeneratedIdentifier;

                  // *** 10/14/99 SWSRCHF
                  // *** H00076695
                  export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                    entities.MonitoredActivity.FedNonComplianceDate;

                  // *** Problem report H00104550
                  // *** 10/02/00 SWSRCHF
                  export.PageKeys.Update.
                    GexportPageKeyMonitoredActivityAssignment.CreatedTimestamp =
                      entities.MonitoredActivityAssignment.CreatedTimestamp;
                }
                else
                {
                  ExitState = "SP0000_LIST_IS_FULL";
                }

                break;
              }

              export.Group.Update.MonitoredActivity.Assign(
                entities.MonitoredActivity);
              export.Group.Update.Infrastructure.
                Assign(entities.Infrastructure);

              if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
              {
                if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
                else
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                }
              }
              else if (!Lt(local.Current.Date,
                entities.MonitoredActivity.ClosureDate))
              {
                export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
              }
            }
          }
          else if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
          {
            if (!IsEmpty(export.HeaderFips.StateAbbreviation))
            {
              // -- 09/23/2011 New access path with Domestic Court Order Number 
              // search criteria is...
              // Access table SRCKTM1.CKT_FIPS
              //        using index SRCKTM1.CKI04320 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_FIPS04
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_TRIBUNAL
              //        using index SRCKTM1.CKI02330 (3 COLS)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_TRIBUNAL09
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_LEGAL_ACTION
              //        using index SRCKTM1.CKI06328 (2 COLS)
              //        Intent share;
              //        index only
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_LEGAL_ACTION05
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_INFRASTRUCTURE
              //        using index SRCKTM1.CKI09777 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_INFRASTRUCTU03
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_MONITORED_ACT
              //        using index SRCKTM1.CKI02721 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_MONITORED_AC02
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
              //        using index SRCKTM1.CKI01710 (5 COLS)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_ASSGN_MNT_AC01
              //        qualified for page range screening
              // Composite table is sorted for DISTINCT/UNIQUE clause, ORDER BY 
              // clause
              // -- Domestic tribunal.
              foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity5())
                
              {
                ++export.Group.Index;
                export.Group.CheckSize();

                if (export.Group.Index >= Export.GroupGroup.Capacity)
                {
                  if (export.PageKeys.Index + 1 != Export
                    .PageKeysGroup.Capacity)
                  {
                    ++export.PageKeys.Index;
                    export.PageKeys.CheckSize();

                    export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                      SystemGeneratedIdentifier =
                        entities.MonitoredActivity.SystemGeneratedIdentifier;

                    // *** 10/14/99 SWSRCHF
                    // *** H00076695
                    export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                      entities.MonitoredActivity.FedNonComplianceDate;

                    // *** Problem report H00104550
                    // *** 10/02/00 SWSRCHF
                    export.PageKeys.Update.
                      GexportPageKeyMonitoredActivityAssignment.
                        CreatedTimestamp =
                        entities.MonitoredActivityAssignment.CreatedTimestamp;
                  }
                  else
                  {
                    ExitState = "SP0000_LIST_IS_FULL";
                  }

                  break;
                }

                export.Group.Update.MonitoredActivity.Assign(
                  entities.MonitoredActivity);
                export.Group.Update.Infrastructure.Assign(
                  entities.Infrastructure);

                if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
                {
                  if (!Lt(local.Current.Date,
                    entities.MonitoredActivity.ClosureDate))
                  {
                    export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                  }
                  else
                  {
                    export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                  }
                }
                else if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
              }
            }
            else
            {
              // -- 09/23/2011 New access path with Foreign Court Order Number 
              // search criteria is...
              // Access table SRCKTM1.CKT_LEGAL_ACTION
              //        using index SRCKTM1.CKI06328 (2 COLS)
              //        Intent share;
              //        index only
              //        access degree is 0   pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_LEGAL_ACTION05
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_INFRASTRUCTURE
              //        using index SRCKTM1.CKI09777 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_INFRASTRUCTU03
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_MONITORED_ACT
              //        using index SRCKTM1.CKI02721 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_MONITORED_AC02
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
              //        using index SRCKTM1.CKI01710 (5 COLS)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_ASSGN_MNT_AC01
              //        qualified for page range screening
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_FIPS_TRIB_ADDR
              //        using index SRCKTM1.CKI03304 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_FIPS_TRIB_AD04
              // Composite table is sorted for DISTINCT/UNIQUE clause, ORDER BY 
              // clause
              // -- Foreign tribunal.
              foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity6())
                
              {
                ++export.Group.Index;
                export.Group.CheckSize();

                if (export.Group.Index >= Export.GroupGroup.Capacity)
                {
                  if (export.PageKeys.Index + 1 != Export
                    .PageKeysGroup.Capacity)
                  {
                    ++export.PageKeys.Index;
                    export.PageKeys.CheckSize();

                    export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                      SystemGeneratedIdentifier =
                        entities.MonitoredActivity.SystemGeneratedIdentifier;

                    // *** 10/14/99 SWSRCHF
                    // *** H00076695
                    export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                      entities.MonitoredActivity.FedNonComplianceDate;

                    // *** Problem report H00104550
                    // *** 10/02/00 SWSRCHF
                    export.PageKeys.Update.
                      GexportPageKeyMonitoredActivityAssignment.
                        CreatedTimestamp =
                        entities.MonitoredActivityAssignment.CreatedTimestamp;
                  }
                  else
                  {
                    ExitState = "SP0000_LIST_IS_FULL";
                  }

                  break;
                }

                export.Group.Update.MonitoredActivity.Assign(
                  entities.MonitoredActivity);
                export.Group.Update.Infrastructure.Assign(
                  entities.Infrastructure);

                if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
                {
                  if (!Lt(local.Current.Date,
                    entities.MonitoredActivity.ClosureDate))
                  {
                    export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                  }
                  else
                  {
                    export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                  }
                }
                else if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
              }
            }
          }
          else
          {
            // -- 09/23/2011 New access path with no search criteria is...
            // Access table SRCKTM1.CKT_ASSGN_MNT_ACT
            //        using index SRCKTM1.CKI02710 (4 COLS)
            //        Intent share; Sequential prefetch;
            //        index only
            //        access degree is 0   pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_ASSGN_MNT_AC02
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_MONITORED_ACT
            //        using index SRCKTM1.CKI05721 (2 COLS)
            //        Intent share;
            //        index only
            //        access degree is 0   pgroup is  1
            //        join degree is 0     pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_MONITORED_AC01
            // Composite table is sorted for ORDER BY clause
            foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity18())
              
            {
              if (!ReadMonitoredActivity1())
              {
                // -- Should never happen
                continue;
              }

              if (!ReadInfrastructure())
              {
                // -- Should never happen
                continue;
              }

              ++export.Group.Index;
              export.Group.CheckSize();

              if (export.Group.Index >= Export.GroupGroup.Capacity)
              {
                if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
                {
                  ++export.PageKeys.Index;
                  export.PageKeys.CheckSize();

                  export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                    SystemGeneratedIdentifier =
                      entities.MonitoredActivity.SystemGeneratedIdentifier;

                  // *** 10/14/99 SWSRCHF
                  // *** H00076695
                  export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                    entities.MonitoredActivity.FedNonComplianceDate;

                  // *** Problem report H00104550
                  // *** 10/02/00 SWSRCHF
                  export.PageKeys.Update.
                    GexportPageKeyMonitoredActivityAssignment.CreatedTimestamp =
                      entities.Cki02710.CreatedTimestamp;
                }
                else
                {
                  ExitState = "SP0000_LIST_IS_FULL";
                }

                break;
              }

              export.Group.Update.MonitoredActivity.Assign(
                entities.MonitoredActivity);
              export.Group.Update.Infrastructure.
                Assign(entities.Infrastructure);

              if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
              {
                if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
                else
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                }
              }
              else if (!Lt(local.Current.Date,
                entities.MonitoredActivity.ClosureDate))
              {
                export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
              }
            }
          }

          // -- 09/23/2011  Original read each commented out below.
          break;
        case "ONC":
          // *** Problem report H00104550/H00104139A
          // *** 10/02/00 SWSRCHF
          // *** added check for Monitered Activity Assignment discontinue date 
          // not equal
          // *** '2099-12-31' and Monitored Activity closure date = '2099-12-31'
          // to the
          // *** READ statement
          // -- 09/23/11 GVandy CQ30438 Created 4 separate Read Each statements 
          // for each filter date type.
          //    These reads are optimized for Case, Person, Court Order Number, 
          // and blank search criteria.
          //    Use care when modifying these reads to insure that the index 
          // path is not altered.
          if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
          {
            // -- 09/23/2011 New access path with case search criteria is...
            // Access table SRCKTM1.CKT_INFRASTRUCTURE
            //        using index SRCKTM1.CKI02777 (1 COL)
            //        Intent share; Prefetch through page list;
            //        access degree is 12  pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_INFRASTRUCTU03
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_MONITORED_ACT
            //        using index SRCKTM1.CKI02721 (1 COL)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_MONITORED_AC02
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
            //        using index SRCKTM1.CKI01710 (5 COLS)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_ASSGN_MNT_AC01
            //        qualified for page range screening
            // Composite table is sorted for ORDER BY clause
            foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity11())
              
            {
              if (!IsEmpty(export.HeaderInfrastructure.CsePersonNumber))
              {
                if (!Equal(entities.Infrastructure.CsePersonNumber,
                  export.HeaderInfrastructure.CsePersonNumber))
                {
                  continue;
                }
              }

              if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
              {
                if (!Equal(entities.Infrastructure.BusinessObjectCd, "LEA"))
                {
                  continue;
                }
                else
                {
                  if (!IsEmpty(export.HeaderFips.StateAbbreviation))
                  {
                    // ***********************************************
                    // User has entered domestic court case number.
                    // ***********************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (ReadLegalAction1())
                    {
                      goto Test8;
                    }
                    else
                    {
                      continue;
                    }
                  }

                  if (!IsEmpty(export.HeaderFipsTribAddress.Country))
                  {
                    // ************************************************
                    // User has entered international court case number.
                    // ************************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (!ReadLegalAction2())
                    {
                      continue;
                    }
                  }
                }
              }

Test8:

              ++export.Group.Index;
              export.Group.CheckSize();

              if (export.Group.Index >= Export.GroupGroup.Capacity)
              {
                if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
                {
                  ++export.PageKeys.Index;
                  export.PageKeys.CheckSize();

                  export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                    SystemGeneratedIdentifier =
                      entities.MonitoredActivity.SystemGeneratedIdentifier;

                  // *** 10/14/99 SWSRCHF
                  // *** H00076695
                  export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                    entities.MonitoredActivity.OtherNonComplianceDate;

                  // *** Problem report H00104550
                  // *** 10/02/00 SWSRCHF
                  export.PageKeys.Update.
                    GexportPageKeyMonitoredActivityAssignment.CreatedTimestamp =
                      entities.MonitoredActivityAssignment.CreatedTimestamp;
                }
                else
                {
                  ExitState = "SP0000_LIST_IS_FULL";
                }

                break;
              }

              export.Group.Update.MonitoredActivity.Assign(
                entities.MonitoredActivity);
              export.Group.Update.Infrastructure.
                Assign(entities.Infrastructure);

              if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
              {
                if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
                else
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                }
              }
              else if (!Lt(local.Current.Date,
                entities.MonitoredActivity.ClosureDate))
              {
                export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
              }
            }
          }
          else if (!IsEmpty(export.HeaderInfrastructure.CsePersonNumber))
          {
            // -- 09/23/2011 New access path with Person search criteria is...
            // Access table SRCKTM1.CKT_INFRASTRUCTURE
            //        using index SRCKTM1.CKI03777 (1 COL)
            //        Intent share; Prefetch through page list;
            //        access degree is 12  pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_INFRASTRUCTU03
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_MONITORED_ACT
            //        using index SRCKTM1.CKI02721 (1 COL)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_MONITORED_AC02
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
            //        using index SRCKTM1.CKI01710 (5 COLS)
            //        Intent share;
            //        access degree is 12  pgroup is  1
            //        join degree is 12    pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_ASSGN_MNT_AC01
            //        qualified for page range screening
            // Composite table is sorted for ORDER BY clause
            foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity12())
              
            {
              if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
              {
                if (!Equal(entities.Infrastructure.BusinessObjectCd, "LEA"))
                {
                  continue;
                }
                else
                {
                  if (!IsEmpty(export.HeaderFips.StateAbbreviation))
                  {
                    // ***********************************************
                    // User has entered domestic court case number.
                    // ***********************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (ReadLegalAction1())
                    {
                      goto Test9;
                    }
                    else
                    {
                      continue;
                    }
                  }

                  if (!IsEmpty(export.HeaderFipsTribAddress.Country))
                  {
                    // ************************************************
                    // User has entered international court case number.
                    // ************************************************
                    local.Read.Identifier =
                      (int)entities.Infrastructure.DenormNumeric12.
                        GetValueOrDefault();

                    if (!ReadLegalAction2())
                    {
                      continue;
                    }
                  }
                }
              }

Test9:

              ++export.Group.Index;
              export.Group.CheckSize();

              if (export.Group.Index >= Export.GroupGroup.Capacity)
              {
                if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
                {
                  ++export.PageKeys.Index;
                  export.PageKeys.CheckSize();

                  export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                    SystemGeneratedIdentifier =
                      entities.MonitoredActivity.SystemGeneratedIdentifier;

                  // *** 10/14/99 SWSRCHF
                  // *** H00076695
                  export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                    entities.MonitoredActivity.OtherNonComplianceDate;

                  // *** Problem report H00104550
                  // *** 10/02/00 SWSRCHF
                  export.PageKeys.Update.
                    GexportPageKeyMonitoredActivityAssignment.CreatedTimestamp =
                      entities.MonitoredActivityAssignment.CreatedTimestamp;
                }
                else
                {
                  ExitState = "SP0000_LIST_IS_FULL";
                }

                break;
              }

              export.Group.Update.MonitoredActivity.Assign(
                entities.MonitoredActivity);
              export.Group.Update.Infrastructure.
                Assign(entities.Infrastructure);

              if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
              {
                if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
                else
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                }
              }
              else if (!Lt(local.Current.Date,
                entities.MonitoredActivity.ClosureDate))
              {
                export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
              }
            }
          }
          else if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
          {
            if (!IsEmpty(export.HeaderFips.StateAbbreviation))
            {
              // -- 09/23/2011 New access path with Domestic Court Order Number 
              // search criteria is...
              // Access table SRCKTM1.CKT_FIPS
              //        using index SRCKTM1.CKI04320 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_FIPS04
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_TRIBUNAL
              //        using index SRCKTM1.CKI02330 (3 COLS)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_TRIBUNAL09
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_LEGAL_ACTION
              //        using index SRCKTM1.CKI06328 (2 COLS)
              //        Intent share;
              //        index only
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_LEGAL_ACTION05
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_INFRASTRUCTURE
              //        using index SRCKTM1.CKI09777 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_INFRASTRUCTU03
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_MONITORED_ACT
              //        using index SRCKTM1.CKI02721 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_MONITORED_AC02
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
              //        using index SRCKTM1.CKI01710 (5 COLS)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_ASSGN_MNT_AC01
              //        qualified for page range screening
              // Composite table is sorted for DISTINCT/UNIQUE clause, ORDER BY 
              // clause
              // -- Domestic tribunal.
              foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity9())
                
              {
                ++export.Group.Index;
                export.Group.CheckSize();

                if (export.Group.Index >= Export.GroupGroup.Capacity)
                {
                  if (export.PageKeys.Index + 1 != Export
                    .PageKeysGroup.Capacity)
                  {
                    ++export.PageKeys.Index;
                    export.PageKeys.CheckSize();

                    export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                      SystemGeneratedIdentifier =
                        entities.MonitoredActivity.SystemGeneratedIdentifier;

                    // *** 10/14/99 SWSRCHF
                    // *** H00076695
                    export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                      entities.MonitoredActivity.OtherNonComplianceDate;

                    // *** Problem report H00104550
                    // *** 10/02/00 SWSRCHF
                    export.PageKeys.Update.
                      GexportPageKeyMonitoredActivityAssignment.
                        CreatedTimestamp =
                        entities.MonitoredActivityAssignment.CreatedTimestamp;
                  }
                  else
                  {
                    ExitState = "SP0000_LIST_IS_FULL";
                  }

                  break;
                }

                export.Group.Update.MonitoredActivity.Assign(
                  entities.MonitoredActivity);
                export.Group.Update.Infrastructure.Assign(
                  entities.Infrastructure);

                if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
                {
                  if (!Lt(local.Current.Date,
                    entities.MonitoredActivity.ClosureDate))
                  {
                    export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                  }
                  else
                  {
                    export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                  }
                }
                else if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
              }
            }
            else
            {
              // -- 09/23/2011 New access path with Foreign Court Order Number 
              // search criteria is...
              // Access table SRCKTM1.CKT_LEGAL_ACTION
              //        using index SRCKTM1.CKI06328 (2 COLS)
              //        Intent share;
              //        index only
              //        access degree is 0   pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_LEGAL_ACTION05
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_INFRASTRUCTURE
              //        using index SRCKTM1.CKI09777 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_INFRASTRUCTU03
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_MONITORED_ACT
              //        using index SRCKTM1.CKI02721 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_MONITORED_AC02
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_ASSGN_MNT_ACT
              //        using index SRCKTM1.CKI01710 (5 COLS)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_ASSGN_MNT_AC01
              //        qualified for page range screening
              // Join using nested loop join with
              //        Access table SRCKTM1.CKT_FIPS_TRIB_ADDR
              //        using index SRCKTM1.CKI03304 (1 COL)
              //        Intent share;
              //        access degree is 0   pgroup is  1
              //        join degree is 0     pgroup is  1
              //        parallelism query CP
              //        correlation name is CKT_FIPS_TRIB_AD04
              // Composite table is sorted for DISTINCT/UNIQUE clause, ORDER BY 
              // clause
              // -- Foreign tribunal.
              foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity10())
                
              {
                ++export.Group.Index;
                export.Group.CheckSize();

                if (export.Group.Index >= Export.GroupGroup.Capacity)
                {
                  if (export.PageKeys.Index + 1 != Export
                    .PageKeysGroup.Capacity)
                  {
                    ++export.PageKeys.Index;
                    export.PageKeys.CheckSize();

                    export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                      SystemGeneratedIdentifier =
                        entities.MonitoredActivity.SystemGeneratedIdentifier;

                    // *** 10/14/99 SWSRCHF
                    // *** H00076695
                    export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                      entities.MonitoredActivity.OtherNonComplianceDate;

                    // *** Problem report H00104550
                    // *** 10/02/00 SWSRCHF
                    export.PageKeys.Update.
                      GexportPageKeyMonitoredActivityAssignment.
                        CreatedTimestamp =
                        entities.MonitoredActivityAssignment.CreatedTimestamp;
                  }
                  else
                  {
                    ExitState = "SP0000_LIST_IS_FULL";
                  }

                  break;
                }

                export.Group.Update.MonitoredActivity.Assign(
                  entities.MonitoredActivity);
                export.Group.Update.Infrastructure.Assign(
                  entities.Infrastructure);

                if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
                {
                  if (!Lt(local.Current.Date,
                    entities.MonitoredActivity.ClosureDate))
                  {
                    export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                  }
                  else
                  {
                    export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                  }
                }
                else if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
              }
            }
          }
          else
          {
            // -- 09/23/2011 New access path with no search criteria is...
            // Access table SRCKTM1.CKT_ASSGN_MNT_ACT
            //        using index SRCKTM1.CKI02710 (4 COLS)
            //        Intent share; Sequential prefetch;
            //        index only
            //        access degree is 0   pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_ASSGN_MNT_AC02
            // Join using nested loop join with
            //        Access table SRCKTM1.CKT_MONITORED_ACT
            //        using index SRCKTM1.CKI05721 (2 COLS)
            //        Intent share;
            //        index only
            //        access degree is 0   pgroup is  1
            //        join degree is 0     pgroup is  1
            //        parallelism query CP
            //        correlation name is CKT_MONITORED_AC01
            // Composite table is sorted for ORDER BY clause
            foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity19())
              
            {
              if (!ReadMonitoredActivity1())
              {
                // -- Should never happen
                continue;
              }

              if (!ReadInfrastructure())
              {
                // --  Should never happen
                continue;
              }

              ++export.Group.Index;
              export.Group.CheckSize();

              if (export.Group.Index >= Export.GroupGroup.Capacity)
              {
                if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
                {
                  ++export.PageKeys.Index;
                  export.PageKeys.CheckSize();

                  export.PageKeys.Update.GexportPageKeyMonitoredActivity.
                    SystemGeneratedIdentifier =
                      entities.MonitoredActivity.SystemGeneratedIdentifier;

                  // *** 10/14/99 SWSRCHF
                  // *** H00076695
                  export.PageKeys.Update.GexportPageKeyDateWorkArea.Date =
                    entities.MonitoredActivity.OtherNonComplianceDate;

                  // *** Problem report H00104550
                  // *** 10/02/00 SWSRCHF
                  export.PageKeys.Update.
                    GexportPageKeyMonitoredActivityAssignment.CreatedTimestamp =
                      entities.Cki02710.CreatedTimestamp;
                }
                else
                {
                  ExitState = "SP0000_LIST_IS_FULL";
                }

                break;
              }

              export.Group.Update.MonitoredActivity.Assign(
                entities.MonitoredActivity);
              export.Group.Update.Infrastructure.
                Assign(entities.Infrastructure);

              if (AsChar(export.HeaderShowAll.OneChar) == 'Y')
              {
                if (!Lt(local.Current.Date,
                  entities.MonitoredActivity.ClosureDate))
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
                }
                else
                {
                  export.Group.Update.HighliteOldAsgnmt.Flag = "N";
                }
              }
              else if (!Lt(local.Current.Date,
                entities.MonitoredActivity.ClosureDate))
              {
                export.Group.Update.HighliteOldAsgnmt.Flag = "Y";
              }
            }
          }

          // -- 09/23/2011  Original read each commented out below.
          break;
        default:
          break;
      }

      // ************************************************
      // Now read legal action and protect, set colours.
      // ************************************************
      if (!export.Group.IsEmpty)
      {
        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          export.Group.Update.Common.SelectChar = "";

          if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
          {
          }
          else
          {
            local.Read.Identifier =
              (int)export.Group.Item.Infrastructure.DenormNumeric12.
                GetValueOrDefault();

            if (Equal(export.Group.Item.Infrastructure.BusinessObjectCd, "LEA"))
            {
              // Read legal action to get court case number.
              if (ReadLegalAction3())
              {
                MoveLegalAction(entities.LegalAction,
                  export.Group.Update.LegalAction);
              }
              else
              {
                export.Group.Update.LegalAction.CourtCaseNumber =
                  "**Legal Action NF";
              }
            }
            else
            {
              export.Group.Update.LegalAction.CourtCaseNumber = "";
            }
          }

          if (!Equal(export.Group.Item.MonitoredActivity.ClosureDate,
            local.Null1.Date))
          {
            local.DateWorkArea.Date =
              export.Group.Item.MonitoredActivity.ClosureDate;
            export.Group.Update.MonitoredActivity.ClosureDate =
              UseCabSetMaximumDiscontinueDate2();
          }

          if (!Equal(export.Group.Item.MonitoredActivity.FedNonComplianceDate,
            local.Null1.Date))
          {
            local.DateWorkArea.Date =
              export.Group.Item.MonitoredActivity.FedNonComplianceDate;
            export.Group.Update.MonitoredActivity.FedNonComplianceDate =
              UseCabSetMaximumDiscontinueDate2();
          }

          if (!Equal(export.Group.Item.MonitoredActivity.OtherNonComplianceDate,
            local.Null1.Date))
          {
            local.DateWorkArea.Date =
              export.Group.Item.MonitoredActivity.OtherNonComplianceDate;
            export.Group.Update.MonitoredActivity.OtherNonComplianceDate =
              UseCabSetMaximumDiscontinueDate2();
          }

          if (AsChar(export.Group.Item.HighliteOldAsgnmt.Flag) == 'Y')
          {
            // ************************************************
            // If the Monitored Activity Assignment has been discontinued, set 
            // color to 'Yellow' and Protect all fields.
            // ************************************************
            var field1 =
              GetField(export.Group.Item.MonitoredActivity, "typeCode");

            field1.Color = "yellow";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.MonitoredActivity,
              "fedNonComplianceDate");

            field2.Color = "yellow";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.MonitoredActivity,
              "otherNonComplianceDate");

            field3.Color = "yellow";
            field3.Protected = true;

            var field4 =
              GetField(export.Group.Item.MonitoredActivity, "startDate");

            field4.Color = "yellow";
            field4.Protected = true;

            var field5 =
              GetField(export.Group.Item.MonitoredActivity, "closureDate");

            field5.Color = "yellow";
            field5.Protected = true;

            var field6 =
              GetField(export.Group.Item.MonitoredActivity, "closureReasonCode");
              

            field6.Color = "yellow";
            field6.Protected = true;

            var field7 =
              GetField(export.Group.Item.PromptReason, "promptField");

            field7.Color = "yellow";
            field7.Protected = true;

            var field8 =
              GetField(export.Group.Item.MonitoredActivity, "createdBy");

            field8.Color = "yellow";
            field8.Protected = true;

            var field9 =
              GetField(export.Group.Item.Infrastructure, "caseNumber");

            field9.Color = "yellow";
            field9.Protected = true;

            var field10 =
              GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

            field10.Color = "yellow";
            field10.Protected = true;

            var field11 =
              GetField(export.Group.Item.Infrastructure, "csePersonNumber");

            field11.Color = "yellow";
            field11.Protected = true;

            var field12 = GetField(export.Group.Item.MonitoredActivity, "name");

            field12.Color = "yellow";
            field12.Protected = true;

            var field13 =
              GetField(export.Group.Item.CsePersonsWorkSet, "formattedName");

            field13.Color = "yellow";
            field13.Protected = true;

            var field14 = GetField(export.Group.Item.Infrastructure, "detail");

            field14.Color = "yellow";
            field14.Protected = true;

            var field15 =
              GetField(export.Group.Item.LegalAction, "courtCaseNumber");

            field15.Color = "yellow";
            field15.Protected = true;
          }
          else
          {
            // ************************************************
            // Check FDNC and ONC and set colors
            // If Near non compliance date is reached, set the corresponding Non
            // compliance date to yellow.
            // If Non compliance date is reached, set it to red.
            // ************************************************
            if (IsEmpty(export.Group.Item.MonitoredActivity.ClosureReasonCode))
            {
              if (!Lt(local.Current.Date,
                export.Group.Item.MonitoredActivity.FedNearNonComplDate) && !
                Equal(export.Group.Item.MonitoredActivity.FedNearNonComplDate,
                local.Null1.Date))
              {
                var field =
                  GetField(export.Group.Item.MonitoredActivity,
                  "fedNonComplianceDate");

                field.Color = "yellow";
                field.Protected = true;
              }

              if (!Lt(local.Current.Date,
                export.Group.Item.MonitoredActivity.OtherNearNonComplDate) && !
                Equal(export.Group.Item.MonitoredActivity.OtherNearNonComplDate,
                local.Null1.Date))
              {
                var field =
                  GetField(export.Group.Item.MonitoredActivity,
                  "otherNonComplianceDate");

                field.Color = "yellow";
                field.Protected = true;
              }

              if (!Lt(local.Current.Date,
                export.Group.Item.MonitoredActivity.FedNonComplianceDate) && !
                Equal(export.Group.Item.MonitoredActivity.FedNonComplianceDate,
                local.Null1.Date))
              {
                var field =
                  GetField(export.Group.Item.MonitoredActivity,
                  "fedNonComplianceDate");

                field.Color = "red";
                field.Protected = true;
              }

              if (!Lt(local.Current.Date,
                export.Group.Item.MonitoredActivity.OtherNonComplianceDate) && !
                Equal(export.Group.Item.MonitoredActivity.
                  OtherNonComplianceDate, local.Null1.Date))
              {
                var field =
                  GetField(export.Group.Item.MonitoredActivity,
                  "otherNonComplianceDate");

                field.Color = "red";
                field.Protected = true;
              }
            }
            else
            {
              var field1 =
                GetField(export.Group.Item.MonitoredActivity, "closureDate");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.MonitoredActivity,
                "closureReasonCode");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.Group.Item.PromptReason, "promptField");

              field3.Color = "cyan";
              field3.Protected = true;
            }

            if (Equal(export.HeaderMonitoredActivityAssignment.ReasonCode, "INF"))
              
            {
              // ************************************************
              // Protect all when Assignment Reason Code is 'INF'
              // ************************************************
              var field1 =
                GetField(export.Group.Item.PromptReason, "promptField");

              field1.Color = "cyan";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.Infrastructure, "caseNumber");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.Group.Item.Infrastructure, "csePersonNumber");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 =
                GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 =
                GetField(export.Group.Item.MonitoredActivity, "closureDate");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 =
                GetField(export.Group.Item.MonitoredActivity, "startDate");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 =
                GetField(export.Group.Item.MonitoredActivity,
                "closureReasonCode");

              field7.Color = "cyan";
              field7.Protected = true;
            }
          }

          if (!IsEmpty(export.Group.Item.Infrastructure.CsePersonNumber))
          {
            export.Group.Update.CsePersonsWorkSet.Number =
              export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Group.Update.CsePersonsWorkSet.FormattedName = "**Name NF";
            }
          }
          else
          {
            export.Group.Update.CsePersonsWorkSet.FormattedName = "";
          }
        }

        export.Group.CheckIndex();
      }

      // *******************************************************
      // Load values for next page - and -
      // Page handler test for ''More -+''
      // *******************************************************
      if (export.CurrentPage.Count == 1 && export.Group.IsEmpty)
      {
        export.CurrentPage.Count = 0;

        // -----***** The only time this can happen is when no data is found for
        // search criteria entered.
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        export.Scroll.ScrollingMessage = "More";
      }
      else if (export.CurrentPage.Count == 1 && !export.Group.IsFull)
      {
        export.Scroll.ScrollingMessage = "More";
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
      else if (export.CurrentPage.Count == 1 && export.Group.IsFull)
      {
        export.Scroll.ScrollingMessage = "More";

        if (export.PageKeys.Count > 1)
        {
          export.Scroll.ScrollingMessage = "More +";
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
      else if (export.CurrentPage.Count > 1 && !export.Group.IsFull)
      {
        export.Scroll.ScrollingMessage = "More -";
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
      else if (export.CurrentPage.Count > 1 && export.Group.IsFull)
      {
        if (export.PageKeys.Count > export.CurrentPage.Count)
        {
          export.Scroll.ScrollingMessage = "More -+";
        }
        else
        {
          export.Scroll.ScrollingMessage = "More -";
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      // **** Roll header filters over to hidden header filters ****
      export.HideHdrServiceProvider.UserId =
        export.HeaderServiceProvider.UserId;
      export.HideHdrFltrStrtRange.Date = export.HeaderFilterStartRange.Date;
      export.HideHdrFltrDteTyp.Text4 = export.HeaderFilterDateType.Text4;
      export.HideHdrShowAll.OneChar = export.HeaderShowAll.OneChar;
      export.HideHdrMonitoredActivityAssignment.ReasonCode =
        export.HeaderMonitoredActivityAssignment.ReasonCode;
      MoveInfrastructure4(export.HeaderInfrastructure,
        export.HideHdrInfrastructure);
      export.HideHdrLegalAction.CourtCaseNumber =
        export.HeaderLegalAction.CourtCaseNumber;
      MoveFips(export.HeaderFips, export.HideHdrFips);
      export.HideHdrFipsTribAddress.Country =
        export.HeaderFipsTribAddress.Country;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure3(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ProcessStatus = source.ProcessStatus;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveInfrastructure4(Infrastructure source,
    Infrastructure target)
  {
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
  }

  private static void MoveInfrastructure5(Infrastructure source,
    Infrastructure target)
  {
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveMonitoredActivity1(MonitoredActivity source,
    MonitoredActivity target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Name = source.Name;
    target.ActivityControlNumber = source.ActivityControlNumber;
    target.TypeCode = source.TypeCode;
    target.FedNonComplianceDate = source.FedNonComplianceDate;
    target.FedNearNonComplDate = source.FedNearNonComplDate;
    target.OtherNonComplianceDate = source.OtherNonComplianceDate;
    target.OtherNearNonComplDate = source.OtherNearNonComplDate;
    target.StartDate = source.StartDate;
    target.ClosureDate = source.ClosureDate;
    target.ClosureReasonCode = source.ClosureReasonCode;
    target.CaseUnitClosedInd = source.CaseUnitClosedInd;
    target.CreatedBy = source.CreatedBy;
  }

  private static void MoveMonitoredActivity2(MonitoredActivity source,
    MonitoredActivity target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Name = source.Name;
    target.TypeCode = source.TypeCode;
  }

  private static void MoveMonitoredActivity3(MonitoredActivity source,
    MonitoredActivity target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Name = source.Name;
    target.TypeCode = source.TypeCode;
    target.ClosureReasonCode = source.ClosureReasonCode;
    target.CreatedBy = source.CreatedBy;
  }

  private static void MoveMonitoredActivityAssignment(
    MonitoredActivityAssignment source, MonitoredActivityAssignment target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReasonCode = source.ReasonCode;
    target.EffectiveDate = source.EffectiveDate;
    target.OverrideInd = source.OverrideInd;
    target.DiscontinueDate = source.DiscontinueDate;
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
    target.UserId = source.UserId;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Null1.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = export.HiddenCodeValue.Cdvalue;
    useImport.Code.CodeName = export.HiddenCode.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabZeroFillNumber1()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = local.Case1.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    local.Case1.Number = useImport.Case1.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    local.CsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseCoCabIsPersonSupervisor()
  {
    var useImport = new CoCabIsPersonSupervisor.Import();
    var useExport = new CoCabIsPersonSupervisor.Export();

    useImport.ServiceProvider.UserId = local.LoggedOnUser.UserId;
    useImport.ProcessDtOrCurrentDt.Date = local.Current.Date;

    Call(CoCabIsPersonSupervisor.Execute, useImport, useExport);

    local.IsSupervisor.Flag = useExport.IsSupervisor.Flag;
  }

  private void UseCreateMonitoredActivity()
  {
    var useImport = new CreateMonitoredActivity.Import();
    var useExport = new CreateMonitoredActivity.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.SelectedInfrastructure.SystemGeneratedIdentifier;
    MoveMonitoredActivity1(export.Group.Item.MonitoredActivity,
      useImport.MonitoredActivity);

    Call(CreateMonitoredActivity.Execute, useImport, useExport);

    export.Group.Update.MonitoredActivity.Assign(useExport.MonitoredActivity);
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.NextTranInfo.Assign(local.NextTranInfo);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

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

    useImport.Case1.Number = local.Case1.Number;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScNextStoreExtractOsp1()
  {
    var useImport = new ScNextStoreExtractOsp.Import();
    var useExport = new ScNextStoreExtractOsp.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      export.HeaderServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = export.HeaderOffice.SystemGeneratedId;
    MoveOfficeServiceProvider(export.HeaderOfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(ScNextStoreExtractOsp.Execute, useImport, useExport);

    local.NextTranInfo.MiscText1 = useExport.NextTranInfo.MiscText1;
  }

  private void UseScNextStoreExtractOsp2()
  {
    var useImport = new ScNextStoreExtractOsp.Import();
    var useExport = new ScNextStoreExtractOsp.Export();

    useImport.NextTranInfo.MiscText1 = export.HiddenNextTranInfo.MiscText1;

    Call(ScNextStoreExtractOsp.Execute, useImport, useExport);

    export.HeaderServiceProvider.SystemGeneratedId =
      useExport.ServiceProvider.SystemGeneratedId;
    export.HeaderOffice.SystemGeneratedId = useExport.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(useExport.OfficeServiceProvider,
      export.HeaderOfficeServiceProvider);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      export.Group.Item.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.Group.Update.CsePersonsWorkSet);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure1(export.Group.Item.Infrastructure,
      useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    export.Group.Update.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpCabCreateMonActAssignment()
  {
    var useImport = new SpCabCreateMonActAssignment.Import();
    var useExport = new SpCabCreateMonActAssignment.Export();

    MoveMonitoredActivityAssignment(export.HiddenMonitoredActivityAssignment,
      useImport.MonitoredActivityAssignment);
    useImport.Office.SystemGeneratedId = export.HeaderOffice.SystemGeneratedId;
    MoveOfficeServiceProvider(export.HeaderOfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      export.HeaderServiceProvider.SystemGeneratedId;
    MoveMonitoredActivity2(export.Group.Item.MonitoredActivity,
      useImport.MonitoredActivity);

    Call(SpCabCreateMonActAssignment.Execute, useImport, useExport);

    export.HiddenMonitoredActivityAssignment.Assign(
      useExport.MonitoredActivityAssignment);
  }

  private void UseSpCabUpdateMonitoredActivity()
  {
    var useImport = new SpCabUpdateMonitoredActivity.Import();
    var useExport = new SpCabUpdateMonitoredActivity.Export();

    MoveMonitoredActivity1(export.Group.Item.MonitoredActivity,
      useImport.MonitoredActivity);

    Call(SpCabUpdateMonitoredActivity.Execute, useImport, useExport);
  }

  private bool ReadActivityActivityDetail()
  {
    entities.ActivityDetail.Populated = false;
    entities.Activity.Populated = false;

    return Read("ReadActivityActivityDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "controlNumber", export.HiddenActivity.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Activity.ControlNumber = db.GetInt32(reader, 0);
        entities.ActivityDetail.ActNo = db.GetInt32(reader, 0);
        entities.Activity.Name = db.GetString(reader, 1);
        entities.Activity.TypeCode = db.GetString(reader, 2);
        entities.ActivityDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ActivityDetail.BusinessObjectCode =
          db.GetNullableString(reader, 4);
        entities.ActivityDetail.CaseUnitFunction =
          db.GetNullableString(reader, 5);
        entities.ActivityDetail.FedNonComplianceDays =
          db.GetNullableInt32(reader, 6);
        entities.ActivityDetail.FedNearNonComplDays =
          db.GetNullableInt32(reader, 7);
        entities.ActivityDetail.OtherNonComplianceDays =
          db.GetNullableInt32(reader, 8);
        entities.ActivityDetail.OtherNearNonComplDays =
          db.GetNullableInt32(reader, 9);
        entities.ActivityDetail.Populated = true;
        entities.Activity.Populated = true;
      });
  }

  private bool ReadActivityStartStop()
  {
    System.Diagnostics.Debug.Assert(entities.ActivityDetail.Populated);
    entities.ActivityStartStop.Populated = false;

    return Read("ReadActivityStartStop",
      (db, command) =>
      {
        db.SetInt32(
          command, "acdId", entities.ActivityDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "actNo", entities.ActivityDetail.ActNo);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ActivityStartStop.ActionCode = db.GetString(reader, 0);
        entities.ActivityStartStop.EffectiveDate = db.GetDate(reader, 1);
        entities.ActivityStartStop.DiscontinueDate = db.GetDate(reader, 2);
        entities.ActivityStartStop.ActNo = db.GetInt32(reader, 3);
        entities.ActivityStartStop.AcdId = db.GetInt32(reader, 4);
        entities.ActivityStartStop.EveNo = db.GetInt32(reader, 5);
        entities.ActivityStartStop.EvdId = db.GetInt32(reader, 6);
        entities.ActivityStartStop.Populated = true;
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.Group.Item.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadEvent()
  {
    System.Diagnostics.Debug.Assert(entities.EventDetail.Populated);
    entities.Event1.Populated = false;

    return Read("ReadEvent",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", entities.EventDetail.EveNo);
      },
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.Event1.Type1 = db.GetString(reader, 1);
        entities.Event1.BusinessObjectCode = db.GetString(reader, 2);
        entities.Event1.Populated = true;
      });
  }

  private bool ReadEventDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ActivityStartStop.Populated);
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI", entities.ActivityStartStop.EvdId);
        db.SetInt32(command, "eveNo", entities.ActivityStartStop.EveNo);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.DetailName = db.GetString(reader, 1);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 2);
        entities.EventDetail.CsenetInOutCode = db.GetString(reader, 3);
        entities.EventDetail.ReasonCode = db.GetString(reader, 4);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 5);
        entities.EventDetail.EffectiveDate = db.GetDate(reader, 6);
        entities.EventDetail.DiscontinueDate = db.GetDate(reader, 7);
        entities.EventDetail.EveNo = db.GetInt32(reader, 8);
        entities.EventDetail.Function = db.GetNullableString(reader, 9);
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    System.Diagnostics.Debug.Assert(entities.MonitoredActivity.Populated);
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.MonitoredActivity.InfSysGenId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 24);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.Read.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
        db.SetString(
          command, "stateAbbreviation", export.HeaderFips.StateAbbreviation);
        db.SetNullableString(
          command, "countyAbbr", export.HeaderFips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.Read.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
        db.SetNullableString(
          command, "country", export.HeaderFipsTribAddress.Country ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.Read.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadMonitoredActivity1()
  {
    entities.MonitoredActivity.Populated = false;

    return Read("ReadMonitoredActivity1",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.Cki05721.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.Name = db.GetString(reader, 1);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 2);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 3);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 4);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 6);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 7);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 8);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 9);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 10);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 11);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 12);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 16);
        entities.MonitoredActivity.Populated = true;
      });
  }

  private bool ReadMonitoredActivity2()
  {
    entities.MonitoredActivity.Populated = false;

    return Read("ReadMonitoredActivity2",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.Group.Item.MonitoredActivity.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.Name = db.GetString(reader, 1);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 2);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 3);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 4);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 6);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 7);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 8);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 9);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 10);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 11);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 12);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 16);
        entities.MonitoredActivity.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity1()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
        db.SetString(
          command, "stateAbbreviation", export.HeaderFips.StateAbbreviation);
        db.SetNullableString(
          command, "countyAbbr", export.HeaderFips.CountyAbbreviation ?? "");
        db.SetNullableDate(
          command, "closureDate1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.High.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "closureDate2",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity10()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity10",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
        db.SetNullableString(
          command, "country", export.HeaderFipsTribAddress.Country ?? "");
        db.SetNullableDate(
          command, "otherNcompDte1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "otherNcompDte2", local.High.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "otherNcompDte3",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity11()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity11",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", export.HeaderInfrastructure.CaseNumber ?? "");
        db.SetNullableDate(
          command, "otherNcompDte1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "otherNcompDte2", local.High.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "otherNcompDte3",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity12()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity12",
      (db, command) =>
      {
        db.SetNullableString(
          command, "csePersonNum",
          export.HeaderInfrastructure.CsePersonNumber ?? "");
        db.SetNullableDate(
          command, "otherNcompDte1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "otherNcompDte2", local.High.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "otherNcompDte3",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity13()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity13",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
        db.SetString(
          command, "stateAbbreviation", export.HeaderFips.StateAbbreviation);
        db.SetNullableString(
          command, "countyAbbr", export.HeaderFips.CountyAbbreviation ?? "");
        db.SetDate(
          command, "startDate1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetNullableDate(
          command, "closureDate", local.High.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "startDate2",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity14()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity14",
      (db, command) =>
      {
        db.SetNullableString(
          command, "country", export.HeaderFipsTribAddress.Country ?? "");
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
        db.SetDate(
          command, "startDate1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetNullableDate(
          command, "closureDate", local.High.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "startDate2",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity15()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity15",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", export.HeaderInfrastructure.CaseNumber ?? "");
        db.SetDate(
          command, "startDate1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetNullableDate(
          command, "closureDate", local.High.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "startDate2",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity16()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity16",
      (db, command) =>
      {
        db.SetNullableString(
          command, "csePersonNum",
          export.HeaderInfrastructure.CsePersonNumber ?? "");
        db.SetDate(
          command, "startDate1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetNullableDate(
          command, "closureDate", local.High.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "startDate2",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity17()
  {
    entities.Cki05721.Populated = false;
    entities.Cki02710.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity17",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetNullableDate(
          command, "closureDate1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.High.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "closureDate2",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Cki02710.ReasonCode = db.GetString(reader, 0);
        entities.Cki02710.EffectiveDate = db.GetDate(reader, 1);
        entities.Cki02710.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Cki02710.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Cki02710.SpdId = db.GetInt32(reader, 4);
        entities.Cki02710.OffId = db.GetInt32(reader, 5);
        entities.Cki02710.OspCode = db.GetString(reader, 6);
        entities.Cki02710.OspDate = db.GetDate(reader, 7);
        entities.Cki02710.MacId = db.GetInt32(reader, 8);
        entities.Cki05721.SystemGeneratedIdentifier = db.GetInt32(reader, 8);
        entities.Cki05721.FedNonComplianceDate = db.GetNullableDate(reader, 9);
        entities.Cki05721.OtherNonComplianceDate =
          db.GetNullableDate(reader, 10);
        entities.Cki05721.StartDate = db.GetDate(reader, 11);
        entities.Cki05721.ClosureDate = db.GetNullableDate(reader, 12);
        entities.Cki05721.Populated = true;
        entities.Cki02710.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity18()
  {
    entities.Cki05721.Populated = false;
    entities.Cki02710.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity18",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetNullableDate(
          command, "closureDate", local.High.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fedNcompDte1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fedNcompDte2",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Cki02710.ReasonCode = db.GetString(reader, 0);
        entities.Cki02710.EffectiveDate = db.GetDate(reader, 1);
        entities.Cki02710.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Cki02710.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Cki02710.SpdId = db.GetInt32(reader, 4);
        entities.Cki02710.OffId = db.GetInt32(reader, 5);
        entities.Cki02710.OspCode = db.GetString(reader, 6);
        entities.Cki02710.OspDate = db.GetDate(reader, 7);
        entities.Cki02710.MacId = db.GetInt32(reader, 8);
        entities.Cki05721.SystemGeneratedIdentifier = db.GetInt32(reader, 8);
        entities.Cki05721.FedNonComplianceDate = db.GetNullableDate(reader, 9);
        entities.Cki05721.OtherNonComplianceDate =
          db.GetNullableDate(reader, 10);
        entities.Cki05721.StartDate = db.GetDate(reader, 11);
        entities.Cki05721.ClosureDate = db.GetNullableDate(reader, 12);
        entities.Cki05721.Populated = true;
        entities.Cki02710.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity19()
  {
    entities.Cki05721.Populated = false;
    entities.Cki02710.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity19",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetNullableDate(
          command, "closureDate", local.High.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "otherNcompDte1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "otherNcompDte2",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Cki02710.ReasonCode = db.GetString(reader, 0);
        entities.Cki02710.EffectiveDate = db.GetDate(reader, 1);
        entities.Cki02710.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Cki02710.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Cki02710.SpdId = db.GetInt32(reader, 4);
        entities.Cki02710.OffId = db.GetInt32(reader, 5);
        entities.Cki02710.OspCode = db.GetString(reader, 6);
        entities.Cki02710.OspDate = db.GetDate(reader, 7);
        entities.Cki02710.MacId = db.GetInt32(reader, 8);
        entities.Cki05721.SystemGeneratedIdentifier = db.GetInt32(reader, 8);
        entities.Cki05721.FedNonComplianceDate = db.GetNullableDate(reader, 9);
        entities.Cki05721.OtherNonComplianceDate =
          db.GetNullableDate(reader, 10);
        entities.Cki05721.StartDate = db.GetDate(reader, 11);
        entities.Cki05721.ClosureDate = db.GetNullableDate(reader, 12);
        entities.Cki05721.Populated = true;
        entities.Cki02710.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity2()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
        db.SetNullableString(
          command, "country", export.HeaderFipsTribAddress.Country ?? "");
        db.SetNullableDate(
          command, "closureDate1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.High.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "closureDate2",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity3()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", export.HeaderInfrastructure.CaseNumber ?? "");
        db.SetNullableDate(
          command, "closureDate1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.High.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "closureDate2",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity4()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "csePersonNum",
          export.HeaderInfrastructure.CsePersonNumber ?? "");
        db.SetNullableDate(
          command, "closureDate1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.High.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "closureDate2",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity5()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity5",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
        db.SetString(
          command, "stateAbbreviation", export.HeaderFips.StateAbbreviation);
        db.SetNullableString(
          command, "countyAbbr", export.HeaderFips.CountyAbbreviation ?? "");
        db.SetNullableDate(
          command, "fedNcompDte1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fedNcompDte2", local.High.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fedNcompDte3",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity6()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity6",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
        db.SetNullableString(
          command, "country", export.HeaderFipsTribAddress.Country ?? "");
        db.SetNullableDate(
          command, "fedNcompDte1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fedNcompDte2", local.High.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fedNcompDte3",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity7()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity7",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseNumber", export.HeaderInfrastructure.CaseNumber ?? "");
        db.SetNullableDate(
          command, "fedNcompDte1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fedNcompDte2", local.High.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fedNcompDte3",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity8()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity8",
      (db, command) =>
      {
        db.SetNullableString(
          command, "csePersonNum",
          export.HeaderInfrastructure.CsePersonNumber ?? "");
        db.SetNullableDate(
          command, "fedNcompDte1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fedNcompDte2", local.High.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fedNcompDte3",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity9()
  {
    entities.MonitoredActivity.Populated = false;
    entities.MonitoredActivityAssignment.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity9",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
        db.SetString(
          command, "stateAbbreviation", export.HeaderFips.StateAbbreviation);
        db.SetNullableString(
          command, "countyAbbr", export.HeaderFips.CountyAbbreviation ?? "");
        db.SetNullableDate(
          command, "otherNcompDte1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "otherNcompDte2", local.High.Date.GetValueOrDefault());
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "otherNcompDte3",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 31);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 32);
        entities.Infrastructure.EventId = db.GetInt32(reader, 33);
        entities.Infrastructure.EventType = db.GetString(reader, 34);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 35);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 36);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 37);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 38);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 39);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 40);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 41);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 42);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 43);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 44);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 45);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 46);
        entities.Infrastructure.UserId = db.GetString(reader, 47);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 48);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 49);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 50);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 51);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 52);
        entities.Infrastructure.Function = db.GetNullableString(reader, 53);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 54);
        entities.MonitoredActivity.Populated = true;
        entities.MonitoredActivityAssignment.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityMonitoredActivityAssignment()
  {
    entities.Cki05721.Populated = false;
    entities.Cki02710.Populated = false;

    return ReadEach("ReadMonitoredActivityMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdId", export.HeaderServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "ospDate",
          export.HeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
        db.SetString(
          command, "ospCode", export.HeaderOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "offId", export.HeaderOffice.SystemGeneratedId);
        db.SetString(
          command, "reasonCode",
          export.HeaderMonitoredActivityAssignment.ReasonCode);
        db.SetDate(
          command, "startDate1",
          export.HeaderFilterStartRange.Date.GetValueOrDefault());
        db.SetString(command, "oneChar", export.HeaderShowAll.OneChar);
        db.SetNullableDate(
          command, "closureDate", local.High.Date.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "startDate2",
          export.PageKeys.Item.GexportPageKeyDateWorkArea.Date.
            GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          export.PageKeys.Item.GexportPageKeyMonitoredActivity.
            SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKeyMonitoredActivityAssignment.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Cki05721.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Cki02710.MacId = db.GetInt32(reader, 0);
        entities.Cki05721.FedNonComplianceDate = db.GetNullableDate(reader, 1);
        entities.Cki05721.OtherNonComplianceDate =
          db.GetNullableDate(reader, 2);
        entities.Cki05721.StartDate = db.GetDate(reader, 3);
        entities.Cki05721.ClosureDate = db.GetNullableDate(reader, 4);
        entities.Cki02710.ReasonCode = db.GetString(reader, 5);
        entities.Cki02710.EffectiveDate = db.GetDate(reader, 6);
        entities.Cki02710.DiscontinueDate = db.GetNullableDate(reader, 7);
        entities.Cki02710.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Cki02710.SpdId = db.GetInt32(reader, 9);
        entities.Cki02710.OffId = db.GetInt32(reader, 10);
        entities.Cki02710.OspCode = db.GetString(reader, 11);
        entities.Cki02710.OspDate = db.GetDate(reader, 12);
        entities.Cki05721.Populated = true;
        entities.Cki02710.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProviderOffice()
  {
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.Name = db.GetString(reader, 5);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 6);
        entities.Office.Populated = true;
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
          command, "macId",
          entities.MonitoredActivity.SystemGeneratedIdentifier);
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
        db.SetInt32(
          command, "servicePrvderId",
          export.HeaderServiceProvider.SystemGeneratedId);
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

  private bool ReadServiceProvider3()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider3",
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

  private bool ReadServiceProviderOfficeServiceProviderOffice1()
  {
    entities.UpdatedByServiceProvider.Populated = false;
    entities.UpdatedByOfficeServiceProvider.Populated = false;
    entities.UpdatedByOffice.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice1",
      (db, command) =>
      {
        db.SetString(command, "userId", local.LoggedOnUser.UserId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "officeId", entities.CreatedByOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.UpdatedByServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.UpdatedByOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.UpdatedByServiceProvider.UserId = db.GetString(reader, 1);
        entities.UpdatedByOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 2);
        entities.UpdatedByOffice.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.UpdatedByOfficeServiceProvider.RoleCode =
          db.GetString(reader, 3);
        entities.UpdatedByOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 4);
        entities.UpdatedByOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.UpdatedByOffice.OffOffice = db.GetNullableInt32(reader, 6);
        entities.UpdatedByServiceProvider.Populated = true;
        entities.UpdatedByOfficeServiceProvider.Populated = true;
        entities.UpdatedByOffice.Populated = true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderOfficeServiceProviderOffice2()
  {
    entities.CreatedByServiceProvider.Populated = false;
    entities.CreatedByOfficeServiceProvider.Populated = false;
    entities.CreatedByOffice.Populated = false;

    return ReadEach("ReadServiceProviderOfficeServiceProviderOffice2",
      (db, command) =>
      {
        db.SetString(command, "userId", entities.ServiceProvider.UserId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CreatedByServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.CreatedByOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.CreatedByServiceProvider.UserId = db.GetString(reader, 1);
        entities.CreatedByOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 2);
        entities.CreatedByOffice.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.CreatedByOfficeServiceProvider.RoleCode =
          db.GetString(reader, 3);
        entities.CreatedByOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 4);
        entities.CreatedByOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CreatedByOffice.OffOffice = db.GetNullableInt32(reader, 6);
        entities.CreatedByServiceProvider.Populated = true;
        entities.CreatedByOfficeServiceProvider.Populated = true;
        entities.CreatedByOffice.Populated = true;

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
      /// A value of GimportPageKeyDateWorkArea.
      /// </summary>
      [JsonPropertyName("gimportPageKeyDateWorkArea")]
      public DateWorkArea GimportPageKeyDateWorkArea
      {
        get => gimportPageKeyDateWorkArea ??= new();
        set => gimportPageKeyDateWorkArea = value;
      }

      /// <summary>
      /// A value of GimportPageKeyMonitoredActivity.
      /// </summary>
      [JsonPropertyName("gimportPageKeyMonitoredActivity")]
      public MonitoredActivity GimportPageKeyMonitoredActivity
      {
        get => gimportPageKeyMonitoredActivity ??= new();
        set => gimportPageKeyMonitoredActivity = value;
      }

      /// <summary>
      /// A value of GimportPageKeyMonitoredActivityAssignment.
      /// </summary>
      [JsonPropertyName("gimportPageKeyMonitoredActivityAssignment")]
      public MonitoredActivityAssignment GimportPageKeyMonitoredActivityAssignment
        
      {
        get => gimportPageKeyMonitoredActivityAssignment ??= new();
        set => gimportPageKeyMonitoredActivityAssignment = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private DateWorkArea gimportPageKeyDateWorkArea;
      private MonitoredActivity gimportPageKeyMonitoredActivity;
      private MonitoredActivityAssignment gimportPageKeyMonitoredActivityAssignment;
        
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of MonitoredActivity.
      /// </summary>
      [JsonPropertyName("monitoredActivity")]
      public MonitoredActivity MonitoredActivity
      {
        get => monitoredActivity ??= new();
        set => monitoredActivity = value;
      }

      /// <summary>
      /// A value of Infrastructure.
      /// </summary>
      [JsonPropertyName("infrastructure")]
      public Infrastructure Infrastructure
      {
        get => infrastructure ??= new();
        set => infrastructure = value;
      }

      /// <summary>
      /// A value of PromptReason.
      /// </summary>
      [JsonPropertyName("promptReason")]
      public Standard PromptReason
      {
        get => promptReason ??= new();
        set => promptReason = value;
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
      /// A value of SpTextWorkArea.
      /// </summary>
      [JsonPropertyName("spTextWorkArea")]
      public SpTextWorkArea SpTextWorkArea
      {
        get => spTextWorkArea ??= new();
        set => spTextWorkArea = value;
      }

      /// <summary>
      /// A value of HighliteOldAsgnmt.
      /// </summary>
      [JsonPropertyName("highliteOldAsgnmt")]
      public Common HighliteOldAsgnmt
      {
        get => highliteOldAsgnmt ??= new();
        set => highliteOldAsgnmt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalAction legalAction;
      private Common common;
      private MonitoredActivity monitoredActivity;
      private Infrastructure infrastructure;
      private Standard promptReason;
      private CsePersonsWorkSet csePersonsWorkSet;
      private SpTextWorkArea spTextWorkArea;
      private Common highliteOldAsgnmt;
    }

    /// <summary>
    /// A value of HideHdrServiceProvider.
    /// </summary>
    [JsonPropertyName("hideHdrServiceProvider")]
    public ServiceProvider HideHdrServiceProvider
    {
      get => hideHdrServiceProvider ??= new();
      set => hideHdrServiceProvider = value;
    }

    /// <summary>
    /// A value of HideHdrPrmptSvpo.
    /// </summary>
    [JsonPropertyName("hideHdrPrmptSvpo")]
    public Standard HideHdrPrmptSvpo
    {
      get => hideHdrPrmptSvpo ??= new();
      set => hideHdrPrmptSvpo = value;
    }

    /// <summary>
    /// A value of HideHdrOffice.
    /// </summary>
    [JsonPropertyName("hideHdrOffice")]
    public Office HideHdrOffice
    {
      get => hideHdrOffice ??= new();
      set => hideHdrOffice = value;
    }

    /// <summary>
    /// A value of HideHdrOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("hideHdrOfficeServiceProvider")]
    public OfficeServiceProvider HideHdrOfficeServiceProvider
    {
      get => hideHdrOfficeServiceProvider ??= new();
      set => hideHdrOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HideHdrFltrStrtRange.
    /// </summary>
    [JsonPropertyName("hideHdrFltrStrtRange")]
    public DateWorkArea HideHdrFltrStrtRange
    {
      get => hideHdrFltrStrtRange ??= new();
      set => hideHdrFltrStrtRange = value;
    }

    /// <summary>
    /// A value of HideHdrFltrDteTyp.
    /// </summary>
    [JsonPropertyName("hideHdrFltrDteTyp")]
    public SpTextWorkArea HideHdrFltrDteTyp
    {
      get => hideHdrFltrDteTyp ??= new();
      set => hideHdrFltrDteTyp = value;
    }

    /// <summary>
    /// A value of HideHdrPrmptDteTyp.
    /// </summary>
    [JsonPropertyName("hideHdrPrmptDteTyp")]
    public Standard HideHdrPrmptDteTyp
    {
      get => hideHdrPrmptDteTyp ??= new();
      set => hideHdrPrmptDteTyp = value;
    }

    /// <summary>
    /// A value of HideHdrShowAll.
    /// </summary>
    [JsonPropertyName("hideHdrShowAll")]
    public Standard HideHdrShowAll
    {
      get => hideHdrShowAll ??= new();
      set => hideHdrShowAll = value;
    }

    /// <summary>
    /// A value of HideHdrMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("hideHdrMonitoredActivityAssignment")]
    public MonitoredActivityAssignment HideHdrMonitoredActivityAssignment
    {
      get => hideHdrMonitoredActivityAssignment ??= new();
      set => hideHdrMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of HideHdrPrmptMonActAsgmt.
    /// </summary>
    [JsonPropertyName("hideHdrPrmptMonActAsgmt")]
    public Standard HideHdrPrmptMonActAsgmt
    {
      get => hideHdrPrmptMonActAsgmt ??= new();
      set => hideHdrPrmptMonActAsgmt = value;
    }

    /// <summary>
    /// A value of HideHdrInfrastructure.
    /// </summary>
    [JsonPropertyName("hideHdrInfrastructure")]
    public Infrastructure HideHdrInfrastructure
    {
      get => hideHdrInfrastructure ??= new();
      set => hideHdrInfrastructure = value;
    }

    /// <summary>
    /// A value of HideHdrLegalAction.
    /// </summary>
    [JsonPropertyName("hideHdrLegalAction")]
    public LegalAction HideHdrLegalAction
    {
      get => hideHdrLegalAction ??= new();
      set => hideHdrLegalAction = value;
    }

    /// <summary>
    /// A value of HideHdrFips.
    /// </summary>
    [JsonPropertyName("hideHdrFips")]
    public Fips HideHdrFips
    {
      get => hideHdrFips ??= new();
      set => hideHdrFips = value;
    }

    /// <summary>
    /// A value of HideHdrFipsTribAddress.
    /// </summary>
    [JsonPropertyName("hideHdrFipsTribAddress")]
    public FipsTribAddress HideHdrFipsTribAddress
    {
      get => hideHdrFipsTribAddress ??= new();
      set => hideHdrFipsTribAddress = value;
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
    /// A value of CurrentPage.
    /// </summary>
    [JsonPropertyName("currentPage")]
    public Common CurrentPage
    {
      get => currentPage ??= new();
      set => currentPage = value;
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
    /// A value of HiddenMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("hiddenMonitoredActivityAssignment")]
    public MonitoredActivityAssignment HiddenMonitoredActivityAssignment
    {
      get => hiddenMonitoredActivityAssignment ??= new();
      set => hiddenMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of HeaderShowAll.
    /// </summary>
    [JsonPropertyName("headerShowAll")]
    public Standard HeaderShowAll
    {
      get => headerShowAll ??= new();
      set => headerShowAll = value;
    }

    /// <summary>
    /// A value of HeaderFilterStartRange.
    /// </summary>
    [JsonPropertyName("headerFilterStartRange")]
    public DateWorkArea HeaderFilterStartRange
    {
      get => headerFilterStartRange ??= new();
      set => headerFilterStartRange = value;
    }

    /// <summary>
    /// A value of HiddenActivity.
    /// </summary>
    [JsonPropertyName("hiddenActivity")]
    public Activity HiddenActivity
    {
      get => hiddenActivity ??= new();
      set => hiddenActivity = value;
    }

    /// <summary>
    /// A value of HiddenActivityDetail.
    /// </summary>
    [JsonPropertyName("hiddenActivityDetail")]
    public ActivityDetail HiddenActivityDetail
    {
      get => hiddenActivityDetail ??= new();
      set => hiddenActivityDetail = value;
    }

    /// <summary>
    /// A value of HiddenObject.
    /// </summary>
    [JsonPropertyName("hiddenObject")]
    public SpTextWorkArea HiddenObject
    {
      get => hiddenObject ??= new();
      set => hiddenObject = value;
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
    /// A value of HiddenCode.
    /// </summary>
    [JsonPropertyName("hiddenCode")]
    public Code HiddenCode
    {
      get => hiddenCode ??= new();
      set => hiddenCode = value;
    }

    /// <summary>
    /// A value of HiddenMonitoredActivity.
    /// </summary>
    [JsonPropertyName("hiddenMonitoredActivity")]
    public MonitoredActivity HiddenMonitoredActivity
    {
      get => hiddenMonitoredActivity ??= new();
      set => hiddenMonitoredActivity = value;
    }

    /// <summary>
    /// A value of HeaderOffice.
    /// </summary>
    [JsonPropertyName("headerOffice")]
    public Office HeaderOffice
    {
      get => headerOffice ??= new();
      set => headerOffice = value;
    }

    /// <summary>
    /// A value of HeaderPromptSvpo.
    /// </summary>
    [JsonPropertyName("headerPromptSvpo")]
    public Standard HeaderPromptSvpo
    {
      get => headerPromptSvpo ??= new();
      set => headerPromptSvpo = value;
    }

    /// <summary>
    /// A value of HeaderPromptDateFilter.
    /// </summary>
    [JsonPropertyName("headerPromptDateFilter")]
    public Standard HeaderPromptDateFilter
    {
      get => headerPromptDateFilter ??= new();
      set => headerPromptDateFilter = value;
    }

    /// <summary>
    /// A value of HeaderPromptMonActAsn.
    /// </summary>
    [JsonPropertyName("headerPromptMonActAsn")]
    public Standard HeaderPromptMonActAsn
    {
      get => headerPromptMonActAsn ??= new();
      set => headerPromptMonActAsn = value;
    }

    /// <summary>
    /// A value of HeaderMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("headerMonitoredActivityAssignment")]
    public MonitoredActivityAssignment HeaderMonitoredActivityAssignment
    {
      get => headerMonitoredActivityAssignment ??= new();
      set => headerMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of HeaderFilterDateType.
    /// </summary>
    [JsonPropertyName("headerFilterDateType")]
    public SpTextWorkArea HeaderFilterDateType
    {
      get => headerFilterDateType ??= new();
      set => headerFilterDateType = value;
    }

    /// <summary>
    /// A value of HeaderOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("headerOfficeServiceProvider")]
    public OfficeServiceProvider HeaderOfficeServiceProvider
    {
      get => headerOfficeServiceProvider ??= new();
      set => headerOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("headerServiceProvider")]
    public ServiceProvider HeaderServiceProvider
    {
      get => headerServiceProvider ??= new();
      set => headerServiceProvider = value;
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

    /// <summary>
    /// A value of HeaderInfrastructure.
    /// </summary>
    [JsonPropertyName("headerInfrastructure")]
    public Infrastructure HeaderInfrastructure
    {
      get => headerInfrastructure ??= new();
      set => headerInfrastructure = value;
    }

    /// <summary>
    /// A value of HeaderLegalAction.
    /// </summary>
    [JsonPropertyName("headerLegalAction")]
    public LegalAction HeaderLegalAction
    {
      get => headerLegalAction ??= new();
      set => headerLegalAction = value;
    }

    /// <summary>
    /// A value of HeaderFips.
    /// </summary>
    [JsonPropertyName("headerFips")]
    public Fips HeaderFips
    {
      get => headerFips ??= new();
      set => headerFips = value;
    }

    /// <summary>
    /// A value of HeaderFipsTribAddress.
    /// </summary>
    [JsonPropertyName("headerFipsTribAddress")]
    public FipsTribAddress HeaderFipsTribAddress
    {
      get => headerFipsTribAddress ??= new();
      set => headerFipsTribAddress = value;
    }

    /// <summary>
    /// A value of UseNate.
    /// </summary>
    [JsonPropertyName("useNate")]
    public Standard UseNate
    {
      get => useNate ??= new();
      set => useNate = value;
    }

    private ServiceProvider hideHdrServiceProvider;
    private Standard hideHdrPrmptSvpo;
    private Office hideHdrOffice;
    private OfficeServiceProvider hideHdrOfficeServiceProvider;
    private DateWorkArea hideHdrFltrStrtRange;
    private SpTextWorkArea hideHdrFltrDteTyp;
    private Standard hideHdrPrmptDteTyp;
    private Standard hideHdrShowAll;
    private MonitoredActivityAssignment hideHdrMonitoredActivityAssignment;
    private Standard hideHdrPrmptMonActAsgmt;
    private Infrastructure hideHdrInfrastructure;
    private LegalAction hideHdrLegalAction;
    private Fips hideHdrFips;
    private FipsTribAddress hideHdrFipsTribAddress;
    private Array<PageKeysGroup> pageKeys;
    private Common currentPage;
    private Standard scroll;
    private MonitoredActivityAssignment hiddenMonitoredActivityAssignment;
    private Standard headerShowAll;
    private DateWorkArea headerFilterStartRange;
    private Activity hiddenActivity;
    private ActivityDetail hiddenActivityDetail;
    private SpTextWorkArea hiddenObject;
    private CodeValue hiddenCodeValue;
    private Code hiddenCode;
    private MonitoredActivity hiddenMonitoredActivity;
    private Office headerOffice;
    private Standard headerPromptSvpo;
    private Standard headerPromptDateFilter;
    private Standard headerPromptMonActAsn;
    private MonitoredActivityAssignment headerMonitoredActivityAssignment;
    private SpTextWorkArea headerFilterDateType;
    private OfficeServiceProvider headerOfficeServiceProvider;
    private ServiceProvider headerServiceProvider;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Infrastructure headerInfrastructure;
    private LegalAction headerLegalAction;
    private Fips headerFips;
    private FipsTribAddress headerFipsTribAddress;
    private Standard useNate;
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
      /// A value of GexportPageKeyDateWorkArea.
      /// </summary>
      [JsonPropertyName("gexportPageKeyDateWorkArea")]
      public DateWorkArea GexportPageKeyDateWorkArea
      {
        get => gexportPageKeyDateWorkArea ??= new();
        set => gexportPageKeyDateWorkArea = value;
      }

      /// <summary>
      /// A value of GexportPageKeyMonitoredActivity.
      /// </summary>
      [JsonPropertyName("gexportPageKeyMonitoredActivity")]
      public MonitoredActivity GexportPageKeyMonitoredActivity
      {
        get => gexportPageKeyMonitoredActivity ??= new();
        set => gexportPageKeyMonitoredActivity = value;
      }

      /// <summary>
      /// A value of GexportPageKeyMonitoredActivityAssignment.
      /// </summary>
      [JsonPropertyName("gexportPageKeyMonitoredActivityAssignment")]
      public MonitoredActivityAssignment GexportPageKeyMonitoredActivityAssignment
        
      {
        get => gexportPageKeyMonitoredActivityAssignment ??= new();
        set => gexportPageKeyMonitoredActivityAssignment = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private DateWorkArea gexportPageKeyDateWorkArea;
      private MonitoredActivity gexportPageKeyMonitoredActivity;
      private MonitoredActivityAssignment gexportPageKeyMonitoredActivityAssignment;
        
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of MonitoredActivity.
      /// </summary>
      [JsonPropertyName("monitoredActivity")]
      public MonitoredActivity MonitoredActivity
      {
        get => monitoredActivity ??= new();
        set => monitoredActivity = value;
      }

      /// <summary>
      /// A value of Infrastructure.
      /// </summary>
      [JsonPropertyName("infrastructure")]
      public Infrastructure Infrastructure
      {
        get => infrastructure ??= new();
        set => infrastructure = value;
      }

      /// <summary>
      /// A value of PromptReason.
      /// </summary>
      [JsonPropertyName("promptReason")]
      public Standard PromptReason
      {
        get => promptReason ??= new();
        set => promptReason = value;
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
      /// A value of SpTextWorkArea.
      /// </summary>
      [JsonPropertyName("spTextWorkArea")]
      public SpTextWorkArea SpTextWorkArea
      {
        get => spTextWorkArea ??= new();
        set => spTextWorkArea = value;
      }

      /// <summary>
      /// A value of HighliteOldAsgnmt.
      /// </summary>
      [JsonPropertyName("highliteOldAsgnmt")]
      public Common HighliteOldAsgnmt
      {
        get => highliteOldAsgnmt ??= new();
        set => highliteOldAsgnmt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalAction legalAction;
      private Common common;
      private MonitoredActivity monitoredActivity;
      private Infrastructure infrastructure;
      private Standard promptReason;
      private CsePersonsWorkSet csePersonsWorkSet;
      private SpTextWorkArea spTextWorkArea;
      private Common highliteOldAsgnmt;
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
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelectedDateWorkArea.
    /// </summary>
    [JsonPropertyName("selectedDateWorkArea")]
    public DateWorkArea SelectedDateWorkArea
    {
      get => selectedDateWorkArea ??= new();
      set => selectedDateWorkArea = value;
    }

    /// <summary>
    /// A value of HideHdrServiceProvider.
    /// </summary>
    [JsonPropertyName("hideHdrServiceProvider")]
    public ServiceProvider HideHdrServiceProvider
    {
      get => hideHdrServiceProvider ??= new();
      set => hideHdrServiceProvider = value;
    }

    /// <summary>
    /// A value of HideHdrPrmptSvpo.
    /// </summary>
    [JsonPropertyName("hideHdrPrmptSvpo")]
    public Standard HideHdrPrmptSvpo
    {
      get => hideHdrPrmptSvpo ??= new();
      set => hideHdrPrmptSvpo = value;
    }

    /// <summary>
    /// A value of HideHdrOffice.
    /// </summary>
    [JsonPropertyName("hideHdrOffice")]
    public Office HideHdrOffice
    {
      get => hideHdrOffice ??= new();
      set => hideHdrOffice = value;
    }

    /// <summary>
    /// A value of HideHdrOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("hideHdrOfficeServiceProvider")]
    public OfficeServiceProvider HideHdrOfficeServiceProvider
    {
      get => hideHdrOfficeServiceProvider ??= new();
      set => hideHdrOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HideHdrFltrStrtRange.
    /// </summary>
    [JsonPropertyName("hideHdrFltrStrtRange")]
    public DateWorkArea HideHdrFltrStrtRange
    {
      get => hideHdrFltrStrtRange ??= new();
      set => hideHdrFltrStrtRange = value;
    }

    /// <summary>
    /// A value of HideHdrFltrDteTyp.
    /// </summary>
    [JsonPropertyName("hideHdrFltrDteTyp")]
    public SpTextWorkArea HideHdrFltrDteTyp
    {
      get => hideHdrFltrDteTyp ??= new();
      set => hideHdrFltrDteTyp = value;
    }

    /// <summary>
    /// A value of HideHdrPrmptDteTyp.
    /// </summary>
    [JsonPropertyName("hideHdrPrmptDteTyp")]
    public Standard HideHdrPrmptDteTyp
    {
      get => hideHdrPrmptDteTyp ??= new();
      set => hideHdrPrmptDteTyp = value;
    }

    /// <summary>
    /// A value of HideHdrShowAll.
    /// </summary>
    [JsonPropertyName("hideHdrShowAll")]
    public Standard HideHdrShowAll
    {
      get => hideHdrShowAll ??= new();
      set => hideHdrShowAll = value;
    }

    /// <summary>
    /// A value of HideHdrMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("hideHdrMonitoredActivityAssignment")]
    public MonitoredActivityAssignment HideHdrMonitoredActivityAssignment
    {
      get => hideHdrMonitoredActivityAssignment ??= new();
      set => hideHdrMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of HideHdrPrmptMonActAsgmt.
    /// </summary>
    [JsonPropertyName("hideHdrPrmptMonActAsgmt")]
    public Standard HideHdrPrmptMonActAsgmt
    {
      get => hideHdrPrmptMonActAsgmt ??= new();
      set => hideHdrPrmptMonActAsgmt = value;
    }

    /// <summary>
    /// A value of HideHdrInfrastructure.
    /// </summary>
    [JsonPropertyName("hideHdrInfrastructure")]
    public Infrastructure HideHdrInfrastructure
    {
      get => hideHdrInfrastructure ??= new();
      set => hideHdrInfrastructure = value;
    }

    /// <summary>
    /// A value of HideHdrLegalAction.
    /// </summary>
    [JsonPropertyName("hideHdrLegalAction")]
    public LegalAction HideHdrLegalAction
    {
      get => hideHdrLegalAction ??= new();
      set => hideHdrLegalAction = value;
    }

    /// <summary>
    /// A value of HideHdrFips.
    /// </summary>
    [JsonPropertyName("hideHdrFips")]
    public Fips HideHdrFips
    {
      get => hideHdrFips ??= new();
      set => hideHdrFips = value;
    }

    /// <summary>
    /// A value of HideHdrFipsTribAddress.
    /// </summary>
    [JsonPropertyName("hideHdrFipsTribAddress")]
    public FipsTribAddress HideHdrFipsTribAddress
    {
      get => hideHdrFipsTribAddress ??= new();
      set => hideHdrFipsTribAddress = value;
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
    /// A value of CurrentPage.
    /// </summary>
    [JsonPropertyName("currentPage")]
    public Common CurrentPage
    {
      get => currentPage ??= new();
      set => currentPage = value;
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
    /// A value of HiddenMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("hiddenMonitoredActivityAssignment")]
    public MonitoredActivityAssignment HiddenMonitoredActivityAssignment
    {
      get => hiddenMonitoredActivityAssignment ??= new();
      set => hiddenMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of AtlmManualList.
    /// </summary>
    [JsonPropertyName("atlmManualList")]
    public Common AtlmManualList
    {
      get => atlmManualList ??= new();
      set => atlmManualList = value;
    }

    /// <summary>
    /// A value of HeaderShowAll.
    /// </summary>
    [JsonPropertyName("headerShowAll")]
    public Standard HeaderShowAll
    {
      get => headerShowAll ??= new();
      set => headerShowAll = value;
    }

    /// <summary>
    /// A value of HeaderFilterStartRange.
    /// </summary>
    [JsonPropertyName("headerFilterStartRange")]
    public DateWorkArea HeaderFilterStartRange
    {
      get => headerFilterStartRange ??= new();
      set => headerFilterStartRange = value;
    }

    /// <summary>
    /// A value of HiddenActivity.
    /// </summary>
    [JsonPropertyName("hiddenActivity")]
    public Activity HiddenActivity
    {
      get => hiddenActivity ??= new();
      set => hiddenActivity = value;
    }

    /// <summary>
    /// A value of HiddenActivityDetail.
    /// </summary>
    [JsonPropertyName("hiddenActivityDetail")]
    public ActivityDetail HiddenActivityDetail
    {
      get => hiddenActivityDetail ??= new();
      set => hiddenActivityDetail = value;
    }

    /// <summary>
    /// A value of HiddenObject.
    /// </summary>
    [JsonPropertyName("hiddenObject")]
    public SpTextWorkArea HiddenObject
    {
      get => hiddenObject ??= new();
      set => hiddenObject = value;
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
    /// A value of HiddenCode.
    /// </summary>
    [JsonPropertyName("hiddenCode")]
    public Code HiddenCode
    {
      get => hiddenCode ??= new();
      set => hiddenCode = value;
    }

    /// <summary>
    /// A value of HiddenMonitoredActivity.
    /// </summary>
    [JsonPropertyName("hiddenMonitoredActivity")]
    public MonitoredActivity HiddenMonitoredActivity
    {
      get => hiddenMonitoredActivity ??= new();
      set => hiddenMonitoredActivity = value;
    }

    /// <summary>
    /// A value of HeaderOffice.
    /// </summary>
    [JsonPropertyName("headerOffice")]
    public Office HeaderOffice
    {
      get => headerOffice ??= new();
      set => headerOffice = value;
    }

    /// <summary>
    /// A value of HeaderPromptSvpo.
    /// </summary>
    [JsonPropertyName("headerPromptSvpo")]
    public Standard HeaderPromptSvpo
    {
      get => headerPromptSvpo ??= new();
      set => headerPromptSvpo = value;
    }

    /// <summary>
    /// A value of HeaderPrompDateFilter.
    /// </summary>
    [JsonPropertyName("headerPrompDateFilter")]
    public Standard HeaderPrompDateFilter
    {
      get => headerPrompDateFilter ??= new();
      set => headerPrompDateFilter = value;
    }

    /// <summary>
    /// A value of HeaderPromptMonActAsn.
    /// </summary>
    [JsonPropertyName("headerPromptMonActAsn")]
    public Standard HeaderPromptMonActAsn
    {
      get => headerPromptMonActAsn ??= new();
      set => headerPromptMonActAsn = value;
    }

    /// <summary>
    /// A value of HeaderMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("headerMonitoredActivityAssignment")]
    public MonitoredActivityAssignment HeaderMonitoredActivityAssignment
    {
      get => headerMonitoredActivityAssignment ??= new();
      set => headerMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of HeaderFilterDateType.
    /// </summary>
    [JsonPropertyName("headerFilterDateType")]
    public SpTextWorkArea HeaderFilterDateType
    {
      get => headerFilterDateType ??= new();
      set => headerFilterDateType = value;
    }

    /// <summary>
    /// A value of HeaderOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("headerOfficeServiceProvider")]
    public OfficeServiceProvider HeaderOfficeServiceProvider
    {
      get => headerOfficeServiceProvider ??= new();
      set => headerOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("headerServiceProvider")]
    public ServiceProvider HeaderServiceProvider
    {
      get => headerServiceProvider ??= new();
      set => headerServiceProvider = value;
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

    /// <summary>
    /// A value of HeaderInfrastructure.
    /// </summary>
    [JsonPropertyName("headerInfrastructure")]
    public Infrastructure HeaderInfrastructure
    {
      get => headerInfrastructure ??= new();
      set => headerInfrastructure = value;
    }

    /// <summary>
    /// A value of HeaderLegalAction.
    /// </summary>
    [JsonPropertyName("headerLegalAction")]
    public LegalAction HeaderLegalAction
    {
      get => headerLegalAction ??= new();
      set => headerLegalAction = value;
    }

    /// <summary>
    /// A value of HeaderFips.
    /// </summary>
    [JsonPropertyName("headerFips")]
    public Fips HeaderFips
    {
      get => headerFips ??= new();
      set => headerFips = value;
    }

    /// <summary>
    /// A value of HeaderFipsTribAddress.
    /// </summary>
    [JsonPropertyName("headerFipsTribAddress")]
    public FipsTribAddress HeaderFipsTribAddress
    {
      get => headerFipsTribAddress ??= new();
      set => headerFipsTribAddress = value;
    }

    /// <summary>
    /// A value of SelectedInfrastructure.
    /// </summary>
    [JsonPropertyName("selectedInfrastructure")]
    public Infrastructure SelectedInfrastructure
    {
      get => selectedInfrastructure ??= new();
      set => selectedInfrastructure = value;
    }

    /// <summary>
    /// A value of UseNate.
    /// </summary>
    [JsonPropertyName("useNate")]
    public Standard UseNate
    {
      get => useNate ??= new();
      set => useNate = value;
    }

    /// <summary>
    /// A value of FilterLegalAction.
    /// </summary>
    [JsonPropertyName("filterLegalAction")]
    public LegalAction FilterLegalAction
    {
      get => filterLegalAction ??= new();
      set => filterLegalAction = value;
    }

    /// <summary>
    /// A value of FilterFips.
    /// </summary>
    [JsonPropertyName("filterFips")]
    public Fips FilterFips
    {
      get => filterFips ??= new();
      set => filterFips = value;
    }

    /// <summary>
    /// A value of FilterFipsTribAddress.
    /// </summary>
    [JsonPropertyName("filterFipsTribAddress")]
    public FipsTribAddress FilterFipsTribAddress
    {
      get => filterFipsTribAddress ??= new();
      set => filterFipsTribAddress = value;
    }

    private LegalAction selectedLegalAction;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private DateWorkArea selectedDateWorkArea;
    private ServiceProvider hideHdrServiceProvider;
    private Standard hideHdrPrmptSvpo;
    private Office hideHdrOffice;
    private OfficeServiceProvider hideHdrOfficeServiceProvider;
    private DateWorkArea hideHdrFltrStrtRange;
    private SpTextWorkArea hideHdrFltrDteTyp;
    private Standard hideHdrPrmptDteTyp;
    private Standard hideHdrShowAll;
    private MonitoredActivityAssignment hideHdrMonitoredActivityAssignment;
    private Standard hideHdrPrmptMonActAsgmt;
    private Infrastructure hideHdrInfrastructure;
    private LegalAction hideHdrLegalAction;
    private Fips hideHdrFips;
    private FipsTribAddress hideHdrFipsTribAddress;
    private Array<PageKeysGroup> pageKeys;
    private Common currentPage;
    private Standard scroll;
    private MonitoredActivityAssignment hiddenMonitoredActivityAssignment;
    private Common atlmManualList;
    private Standard headerShowAll;
    private DateWorkArea headerFilterStartRange;
    private Activity hiddenActivity;
    private ActivityDetail hiddenActivityDetail;
    private SpTextWorkArea hiddenObject;
    private CodeValue hiddenCodeValue;
    private Code hiddenCode;
    private MonitoredActivity hiddenMonitoredActivity;
    private Office headerOffice;
    private Standard headerPromptSvpo;
    private Standard headerPrompDateFilter;
    private Standard headerPromptMonActAsn;
    private MonitoredActivityAssignment headerMonitoredActivityAssignment;
    private SpTextWorkArea headerFilterDateType;
    private OfficeServiceProvider headerOfficeServiceProvider;
    private ServiceProvider headerServiceProvider;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Infrastructure headerInfrastructure;
    private LegalAction headerLegalAction;
    private Fips headerFips;
    private FipsTribAddress headerFipsTribAddress;
    private Infrastructure selectedInfrastructure;
    private Standard useNate;
    private LegalAction filterLegalAction;
    private Fips filterFips;
    private FipsTribAddress filterFipsTribAddress;
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
    public LegalAction Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of High.
    /// </summary>
    [JsonPropertyName("high")]
    public DateWorkArea High
    {
      get => high ??= new();
      set => high = value;
    }

    /// <summary>
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of Pointer.
    /// </summary>
    [JsonPropertyName("pointer")]
    public Common Pointer
    {
      get => pointer ??= new();
      set => pointer = value;
    }

    /// <summary>
    /// A value of ClearGrpView.
    /// </summary>
    [JsonPropertyName("clearGrpView")]
    public Common ClearGrpView
    {
      get => clearGrpView ??= new();
      set => clearGrpView = value;
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
    /// A value of NextTran.
    /// </summary>
    [JsonPropertyName("nextTran")]
    public Infrastructure NextTran
    {
      get => nextTran ??= new();
      set => nextTran = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    /// <summary>
    /// A value of Object1.
    /// </summary>
    [JsonPropertyName("object1")]
    public SpTextWorkArea Object1
    {
      get => object1 ??= new();
      set => object1 = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of LoggedOnUser.
    /// </summary>
    [JsonPropertyName("loggedOnUser")]
    public ServiceProvider LoggedOnUser
    {
      get => loggedOnUser ??= new();
      set => loggedOnUser = value;
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
    /// A value of IsSupervisor.
    /// </summary>
    [JsonPropertyName("isSupervisor")]
    public Common IsSupervisor
    {
      get => isSupervisor ??= new();
      set => isSupervisor = value;
    }

    /// <summary>
    /// A value of Change.
    /// </summary>
    [JsonPropertyName("change")]
    public Common Change
    {
      get => change ??= new();
      set => change = value;
    }

    private LegalAction read;
    private DateWorkArea high;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private Common pointer;
    private Common clearGrpView;
    private Common count;
    private Infrastructure nextTran;
    private DateWorkArea null1;
    private DateWorkArea dateWorkArea;
    private MonitoredActivity monitoredActivity;
    private SpTextWorkArea object1;
    private Common validCode;
    private NextTranInfo nextTranInfo;
    private Case1 case1;
    private CsePerson csePerson;
    private ServiceProvider loggedOnUser;
    private DateWorkArea current;
    private Common isSupervisor;
    private Common change;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Cki05721.
    /// </summary>
    [JsonPropertyName("cki05721")]
    public MonitoredActivity Cki05721
    {
      get => cki05721 ??= new();
      set => cki05721 = value;
    }

    /// <summary>
    /// A value of Cki02710.
    /// </summary>
    [JsonPropertyName("cki02710")]
    public MonitoredActivityAssignment Cki02710
    {
      get => cki02710 ??= new();
      set => cki02710 = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    /// <summary>
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of UpdatedByServiceProvider.
    /// </summary>
    [JsonPropertyName("updatedByServiceProvider")]
    public ServiceProvider UpdatedByServiceProvider
    {
      get => updatedByServiceProvider ??= new();
      set => updatedByServiceProvider = value;
    }

    /// <summary>
    /// A value of UpdatedByOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("updatedByOfficeServiceProvider")]
    public OfficeServiceProvider UpdatedByOfficeServiceProvider
    {
      get => updatedByOfficeServiceProvider ??= new();
      set => updatedByOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of UpdatedByOffice.
    /// </summary>
    [JsonPropertyName("updatedByOffice")]
    public Office UpdatedByOffice
    {
      get => updatedByOffice ??= new();
      set => updatedByOffice = value;
    }

    /// <summary>
    /// A value of CreatedByServiceProvider.
    /// </summary>
    [JsonPropertyName("createdByServiceProvider")]
    public ServiceProvider CreatedByServiceProvider
    {
      get => createdByServiceProvider ??= new();
      set => createdByServiceProvider = value;
    }

    /// <summary>
    /// A value of CreatedByOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("createdByOfficeServiceProvider")]
    public OfficeServiceProvider CreatedByOfficeServiceProvider
    {
      get => createdByOfficeServiceProvider ??= new();
      set => createdByOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of CreatedByOffice.
    /// </summary>
    [JsonPropertyName("createdByOffice")]
    public Office CreatedByOffice
    {
      get => createdByOffice ??= new();
      set => createdByOffice = value;
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
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
    }

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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private MonitoredActivity cki05721;
    private MonitoredActivityAssignment cki02710;
    private MonitoredActivity monitoredActivity;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private ServiceProvider updatedByServiceProvider;
    private OfficeServiceProvider updatedByOfficeServiceProvider;
    private Office updatedByOffice;
    private ServiceProvider createdByServiceProvider;
    private OfficeServiceProvider createdByOfficeServiceProvider;
    private Office createdByOffice;
    private Case1 case1;
    private EventDetail eventDetail;
    private Event1 event1;
    private ActivityStartStop activityStartStop;
    private ActivityDetail activityDetail;
    private Activity activity;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Infrastructure infrastructure;
    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
    private LegalAction legalAction;
    private Fips fips;
  }
#endregion
}
