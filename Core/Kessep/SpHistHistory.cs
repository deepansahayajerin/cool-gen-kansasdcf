// Program: SP_HIST_HISTORY, ID: 371927203, model: 746.
// Short name: SWEHISTP
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
/// A program: SP_HIST_HISTORY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpHistHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_HIST_HISTORY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpHistHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpHistHistory.
  /// </summary>
  public SpHistHistory(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------
    //   Date	  Developer   Request   Description
    // --------  ---------   -------   -----------
    // 10/24/96  Rick Delgado          Initial Development
    // 01/16/97  Govindaraj	        Minor fixes
    // 02/07/97  Rod Grey		Fix moving of value to export sel_infra for NATE
    // 03/06/97  Siraj Konkader        Added logic for flow to DMON.
    // 04/03/97  Siraj Konkader        Fixes and made explicit scrolling.
    // 01/09/98  Jack Rookard          Change page key from Infrastructure Sys 
    // Gen Id to Infrastructure
    // 				created timestamp.  Change sort on display from Sys Gen Id to
    // 				Created Timestamp.  Change various other references to Infra
    // 				Sys Gen Id to Created Timestamp
    // 10/18/99  SWSRCHF     H00076959 Display an asterisk(*) under the "N" 
    // column,
    //                                 
    // whenever there is a Narrative
    // 10/22/99  SWSRCHF     H00078203 The TYPE filter not working for all
    //                                 
    // selections on the picklist
    // 10/25/99  SWSRCHF     H00076070 The Court Case # filter is not working
    // 06/08/00  SWSRCHF      000170   Replaced the check for Narrative with a 
    // check
    //                                 
    // for Narrative Detail
    // 09/19/00  SWSRCHF     H00103567 Added check for Family Violence Indicator
    // (FV). Only
    //                                 
    // the case worker or his
    // supervisor can view case,
    //                                 
    // when FV is set
    // 05/13/02  SWSRMCA      PR140965 Display details from HIST to ISTM.
    // 07/25/02  SWSRVXM      WR020332 When PF15 (Detail) pressed on HIST by 
    // selecting a CSENET record,
    // 				the system must  flow to ICAS instead of ISTM screen with
    // 				next_tran info.
    // 09/21/10  GVandy        CQ22068 Emergency Fix to correct inefficient 
    // index path chosen after
    // 				DB2 V9 upgrade.  Adding "or 0 = 1" condition to person
    // 				number qualification to alter index from CKI03777 back to
    // 				the original CKI02777.  (This is for READ EACH by Case Number)
    // 04/12/13  GVandy        CQ32829 Display infrastructure records in "E" (
    // Error) status if the Event
    // 				Log to Diary = "Y".
    //                                                                                                                              
    // 07/20/18  JHarden      CQ61838 Not all narratives are showing when using
    // the 'Y' filter on CSLN
    // ---------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    import.PageKeys.Index = -1;

    if (import.PageKeys.Count != 0)
    {
      do
      {
        ++import.PageKeys.Index;
        import.PageKeys.CheckSize();

        export.PageKeys.Index = import.PageKeys.Index;
        export.PageKeys.CheckSize();

        export.PageKeys.Update.GexportPageKey.CreatedTimestamp =
          import.PageKeys.Item.GimportPageKey.CreatedTimestamp;
      }
      while(export.PageKeys.Index + 1 != import.PageKeys.Count);
    }

    export.CurrentPage.Count = import.CurrentPage.Count;
    MoveCode(import.HiddenCode, export.HiddenCode);
    export.HiddenCodeValue.Assign(import.HiddenCodeValue);
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    MoveDateWorkArea(import.HiddenImportHeaderStartDate,
      export.HiddenExportHeaderStartDate);
    MoveFips(import.HiddenImportHeaderFips, export.HiddenExportHeaderFips);
    export.HiddenExportHeaderInfrastructure.Assign(
      import.HiddenImportHeaderInfrastructure);
    export.HiddenExportHeaderLegalAction.CourtCaseNumber =
      import.HiddenImportHeaderLegalAction.CourtCaseNumber;
    export.HiddenExportHeaderFipsTribAddress.Country =
      import.HiddenImportHeaderFipsTribAddress.Country;
    MoveFips(import.HeaderFips, export.HeaderFips);
    export.HeaderInfrastructure.Assign(import.HeaderInfrastructure);
    export.HeaderLegalAction.CourtCaseNumber =
      import.HeaderLegalAction.CourtCaseNumber;
    export.HeaderFipsTribAddress.Country = import.HeaderFipsTribAddress.Country;
    export.Event1.Assign(import.Event1);
    export.EventDetail.Assign(import.EventDetail);
    MoveDateWorkArea(import.HeaderStartDate, export.HeaderStartDate);

    if (Equal(export.HeaderStartDate.Timestamp, local.NullTimestamp.Timestamp))
    {
      local.Current.Date = Now().Date.AddMonths(-1);
      local.TextWorkArea.Text8 =
        NumberToString(DateToInt(local.Current.Date), 8);
      local.TextWorkArea.Text10 =
        Substring(local.TextWorkArea.Text8, TextWorkArea.Text8_MaxLength, 1, 4) +
        "-" + Substring
        (local.TextWorkArea.Text8, TextWorkArea.Text8_MaxLength, 5, 2) + "-" + Substring
        (local.TextWorkArea.Text8, TextWorkArea.Text8_MaxLength, 7, 2);
      export.HeaderStartDate.Timestamp = Timestamp(local.TextWorkArea.Text10);
      local.TextWorkArea.Text8 = "";
      local.TextWorkArea.Text10 = "";
      export.PageKeys.Count = 0;

      export.PageKeys.Index = 0;
      export.PageKeys.CheckSize();

      export.PageKeys.Update.GexportPageKey.CreatedTimestamp =
        export.HeaderStartDate.Timestamp;

      if (export.CurrentPage.Count > 0)
      {
        export.CurrentPage.Count = 0;
        local.ClearGroupView.Flag = "Y";
      }
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Scroll.ScrollingMessage = import.Scroll.ScrollingMessage;

    // **** Validate All Commands
    switch(TrimEnd(global.Command))
    {
      case "ADD":
        break;
      case "DISPLAY":
        // **** Check to see if any filters changed ****
        // The if was commented out By Carl Galka to fix problem report 78227 on
        // 11/9/1999. If the user presses enter, we ALWAYS use the entered
        // criteria.
        export.PageKeys.Index = 0;
        export.PageKeys.CheckSize();

        export.PageKeys.Update.GexportPageKey.CreatedTimestamp =
          export.HeaderStartDate.Timestamp;
        export.PageKeys.Count = 0;
        export.PageKeys.Index = -1;
        export.CurrentPage.Count = 0;
        local.ClearGroupView.Flag = "Y";

        break;
      case "DETAIL":
        break;
      case "DMON":
        break;
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          // ---------------------------------------------
          // Set up local next_tran_info for saving the current values for the 
          // next screen
          // ---------------------------------------------
          local.NextTranInfo.CaseNumber =
            export.HeaderInfrastructure.CaseNumber ?? "";
          local.NextTranInfo.CsePersonNumber =
            export.HeaderInfrastructure.CsePersonNumber ?? "";
          local.NextTranInfo.CourtCaseNumber =
            export.HeaderLegalAction.CourtCaseNumber ?? "";
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
      case "EVLS":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LINK":
        // ---------------------------------------------
        // This command is set when the user links in from ALRT or AMEN
        // ---------------------------------------------
        // -----****** Set up page keys to start from the infrastructure record 
        // received from ALRT or AMEN
        export.PageKeys.Count = 0;

        export.PageKeys.Index = 0;
        export.PageKeys.CheckSize();

        export.CurrentPage.Count = 0;
        local.ClearGroupView.Flag = "Y";
        export.PageKeys.Update.GexportPageKey.CreatedTimestamp =
          export.HeaderInfrastructure.CreatedTimestamp;
        export.HeaderStartDate.Timestamp =
          export.HeaderInfrastructure.CreatedTimestamp;

        if (ReadInfrastructure4())
        {
          MoveInfrastructure5(entities.ExistingInfrastructure,
            export.HeaderInfrastructure);
          export.PageKeys.Update.GexportPageKey.CreatedTimestamp =
            entities.ExistingInfrastructure.CreatedTimestamp;
          export.HeaderStartDate.Timestamp =
            entities.ExistingInfrastructure.CreatedTimestamp;
        }

        global.Command = "DISPLAY";

        break;
      case "LIST":
        if (AsChar(import.HeaderPromptType.PromptField) == 'S')
        {
          export.HiddenCode.CodeName = "EVENT TYPE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }
        else
        {
          var field1 = GetField(export.HeaderPromptType, "promptField");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
        }

        break;
      case "NATE":
        break;
      case "CSLN":
        break;
      case "NEXT":
        // **** if filter has changed then issue a message that  ****
        // **** a display has to be done before a Prev or next    ****
        // **** can be performed
        // 
        // ****
        export.PageKeys.Index = export.CurrentPage.Count - 1;
        export.PageKeys.CheckSize();

        if (export.PageKeys.Index == -1)
        {
          // -----*****  First time
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

        // -----*****  Increment page number and group view
        ++export.CurrentPage.Count;

        export.PageKeys.Index = export.CurrentPage.Count - 1;
        export.PageKeys.CheckSize();

        local.ClearGroupView.Flag = "Y";
        global.Command = "DISPLAY";

        break;
      case "PREV":
        if (export.CurrentPage.Count <= 1)
        {
          if (import.PageKeys.IsEmpty)
          {
            // -----*****  First time
            ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

            break;
          }

          ExitState = "ACO_NI0000_TOP_OF_LIST";

          break;
        }

        // -----*****  Decrement page number and group view
        --export.CurrentPage.Count;

        export.PageKeys.Index = export.CurrentPage.Count - 1;
        export.PageKeys.CheckSize();

        global.Command = "DISPLAY";

        break;
      case "RETCDVL":
        // *** Report problem H00078203
        // *** 10/25/99 SWSRCHF
        // *** start
        local.ReturnFromCodeValue.Flag = "Y";
        export.CurrentPage.Count = 0;

        // *** end
        // *** 10/25/99 SWSRCHF
        // *** Report problem H00078203
        var field = GetField(export.HeaderPromptType, "promptField");

        field.Protected = false;
        field.Focused = true;

        if (!IsEmpty(export.HiddenCodeValue.Cdvalue))
        {
          export.HeaderInfrastructure.EventType =
            export.HiddenCodeValue.Cdvalue;
          global.Command = "DISPLAY";
        }
        else
        {
          global.Command = "RDISPLAY";
        }

        break;
      case "RETCSLN":
        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (!import.Group.CheckSize())
          {
            break;
          }

          export.Group.Index = import.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.MonitoredDocument.ClosureReasonCode =
            import.Group.Item.MonitoredDocument.ClosureReasonCode;
          export.Group.Update.SpTextWorkArea.Text80 =
            import.Group.Item.SpTextWorkArea.Text80;
          MoveCsePersonsWorkSet(import.Group.Item.CsePersonsWorkSet,
            export.Group.Update.CsePersonsWorkSet);
          export.Group.Update.Narrative.Text1 =
            import.Group.Item.Narrative.Text1;
          export.Group.Update.LegalAction.CourtCaseNumber =
            import.Group.Item.LegalAction.CourtCaseNumber;
          MoveInfrastructure4(import.Group.Item.Infrastructure,
            export.Group.Update.Infrastructure);

          if (export.Group.Item.Infrastructure.SystemGeneratedIdentifier != 0)
          {
            var field1 =
              GetField(export.Group.Item.Infrastructure, "caseNumber");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.Infrastructure, "csePersonNumber");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;
          export.Group.Update.Event1.Assign(import.Group.Item.Event1);
          export.Group.Update.CaseUnitFunctionAssignmt.Function =
            import.Group.Item.CaseUnitFunctionAssignmt.Function;

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            // *** Work Request 000170
            // *** 06/08/00 SWSRCHF
            // *** start
            // *** Read for Narrative Detail existence
            if (ReadNarrativeDetail())
            {
              export.Group.Update.Narrative.Text1 = "*";
            }

            // *** end
            // *** 06/08/00 SWSRCHF
            // *** Work Request 000170
          }
        }

        import.Group.CheckIndex();

        break;
      case "RETEVLS":
        if (IsEmpty(import.Event1.Type1))
        {
        }
        else
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

            export.Group.Update.MonitoredDocument.ClosureReasonCode =
              import.Group.Item.MonitoredDocument.ClosureReasonCode;
            export.Group.Update.SpTextWorkArea.Text80 =
              import.Group.Item.SpTextWorkArea.Text80;
            MoveCsePersonsWorkSet(import.Group.Item.CsePersonsWorkSet,
              export.Group.Update.CsePersonsWorkSet);
            export.Group.Update.Narrative.Text1 =
              import.Group.Item.Narrative.Text1;
            export.Group.Update.LegalAction.CourtCaseNumber =
              import.Group.Item.LegalAction.CourtCaseNumber;
            MoveInfrastructure4(import.Group.Item.Infrastructure,
              export.Group.Update.Infrastructure);
            export.Group.Update.Common.SelectChar =
              import.Group.Item.Common.SelectChar;
            export.Group.Update.Event1.Assign(import.Group.Item.Event1);
            export.Group.Update.CaseUnitFunctionAssignmt.Function =
              import.Group.Item.CaseUnitFunctionAssignmt.Function;

            if (export.Group.Item.Infrastructure.SystemGeneratedIdentifier != 0)
            {
              var field1 =
                GetField(export.Group.Item.Infrastructure, "caseNumber");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.Group.Item.Infrastructure, "csePersonNumber");

              field3.Color = "cyan";
              field3.Protected = true;
            }

            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Protected = false;
              field1.Focused = true;

              export.Group.Update.Event1.Assign(import.Event1);
              export.Group.Update.Infrastructure.EventType =
                import.Event1.Type1;

              if (ReadEventDetail1())
              {
                export.Group.Update.Infrastructure.EventDetailName =
                  entities.ExistingEventDetail.DetailName;
                export.Group.Update.Infrastructure.Function =
                  entities.ExistingEventDetail.Function;
              }
              else
              {
                ExitState = "SP0000_EVENT_DETAIL_NF";
              }

              return;
            }
          }

          import.Group.CheckIndex();
        }

        break;
      case "RETNARR":
        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (!import.Group.CheckSize())
          {
            break;
          }

          export.Group.Index = import.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.MonitoredDocument.ClosureReasonCode =
            import.Group.Item.MonitoredDocument.ClosureReasonCode;
          export.Group.Update.SpTextWorkArea.Text80 =
            import.Group.Item.SpTextWorkArea.Text80;
          MoveCsePersonsWorkSet(import.Group.Item.CsePersonsWorkSet,
            export.Group.Update.CsePersonsWorkSet);
          export.Group.Update.Narrative.Text1 =
            import.Group.Item.Narrative.Text1;
          export.Group.Update.LegalAction.CourtCaseNumber =
            import.Group.Item.LegalAction.CourtCaseNumber;
          MoveInfrastructure4(import.Group.Item.Infrastructure,
            export.Group.Update.Infrastructure);

          if (export.Group.Item.Infrastructure.SystemGeneratedIdentifier != 0)
          {
            var field1 =
              GetField(export.Group.Item.Infrastructure, "caseNumber");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.Infrastructure, "csePersonNumber");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;
          export.Group.Update.Event1.Assign(import.Group.Item.Event1);
          export.Group.Update.CaseUnitFunctionAssignmt.Function =
            import.Group.Item.CaseUnitFunctionAssignmt.Function;

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            // *** Work Request 000170
            // *** 06/08/00 SWSRCHF
            // *** start
            // *** Read for Narrative Detail existence
            if (ReadNarrativeDetail())
            {
              export.Group.Update.Narrative.Text1 = "*";
            }

            // *** end
            // *** 06/08/00 SWSRCHF
            // *** Work Request 000170
          }
        }

        import.Group.CheckIndex();

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "RTFRMLNK":
        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (!import.Group.CheckSize())
          {
            break;
          }

          export.Group.Index = import.Group.Index;
          export.Group.CheckSize();

          export.Group.Update.MonitoredDocument.ClosureReasonCode =
            import.Group.Item.MonitoredDocument.ClosureReasonCode;
          export.Group.Update.SpTextWorkArea.Text80 =
            import.Group.Item.SpTextWorkArea.Text80;
          MoveCsePersonsWorkSet(import.Group.Item.CsePersonsWorkSet,
            export.Group.Update.CsePersonsWorkSet);
          export.Group.Update.Narrative.Text1 =
            import.Group.Item.Narrative.Text1;
          export.Group.Update.LegalAction.CourtCaseNumber =
            import.Group.Item.LegalAction.CourtCaseNumber;
          MoveInfrastructure4(import.Group.Item.Infrastructure,
            export.Group.Update.Infrastructure);

          if (export.Group.Item.Infrastructure.SystemGeneratedIdentifier != 0)
          {
            var field1 =
              GetField(export.Group.Item.Infrastructure, "caseNumber");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.Infrastructure, "csePersonNumber");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;
          export.Group.Update.Event1.Assign(import.Group.Item.Event1);
          export.Group.Update.CaseUnitFunctionAssignmt.Function =
            import.Group.Item.CaseUnitFunctionAssignmt.Function;

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            var field1 = GetField(export.Group.Item.Common, "selectChar");

            field1.Protected = false;
            field1.Focused = true;
          }
        }

        import.Group.CheckIndex();

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "XXFMMENU":
        global.Command = "DISPLAY";

        break;
      case "XXNEXTXX":
        // ---------------------------------------------
        // User entered this screen from another screen
        // ---------------------------------------------
        UseScCabNextTranGet();
        local.FindHistCount1.Count =
          Find(String(
            local.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd("HIST"));

        if (local.FindHistCount1.Count == 0)
        {
          export.HeaderInfrastructure.CaseNumber =
            local.NextTranInfo.CaseNumber ?? "";
          export.HeaderInfrastructure.CsePersonNumber =
            local.NextTranInfo.CsePersonNumber ?? "";
        }
        else
        {
          export.HeaderInfrastructure.CaseNumber =
            Substring(local.NextTranInfo.MiscText1, 5, 10);
          export.HeaderInfrastructure.CsePersonNumber =
            Substring(local.NextTranInfo.MiscText1, 15, 10);
          export.HeaderLegalAction.CourtCaseNumber =
            Substring(local.NextTranInfo.MiscText1, 25, 17);
          export.HeaderFips.StateAbbreviation =
            Substring(local.NextTranInfo.MiscText1, 42, 2);
          export.HeaderFips.CountyAbbreviation =
            Substring(local.NextTranInfo.MiscText1, 44, 2);
          export.HeaderFipsTribAddress.Country =
            Substring(local.NextTranInfo.MiscText1, 46, 2);
          export.HeaderInfrastructure.Function =
            Substring(local.NextTranInfo.MiscText1, 48, 3);
          local.FindHistCount2.Count =
            Find(String(
              local.NextTranInfo.MiscText2, NextTranInfo.MiscText2_MaxLength),
            TrimEnd("HIST"));

          if (local.FindHistCount2.Count != 0)
          {
            export.HeaderStartDate.Date =
              IntToDate((int)StringToNumber(Substring(
                local.NextTranInfo.MiscText2, 50, 5, 8)));
            export.HeaderInfrastructure.SystemGeneratedIdentifier =
              (int)StringToNumber(Substring(
                local.NextTranInfo.MiscText2, 50, 19, 9));
            export.HeaderInfrastructure.EventType =
              Substring(local.NextTranInfo.MiscText2, 28, 12);
            export.HeaderStartDate.Timestamp =
              Timestamp(Substring(local.NextTranInfo.MiscText2, 50, 5, 4) + "-"
              + Substring(local.NextTranInfo.MiscText2, 50, 9, 2) + "-" + Substring
              (local.NextTranInfo.MiscText2, 50, 11, 2));
            local.Reposition.SystemGeneratedIdentifier =
              (int)StringToNumber(Substring(
                local.NextTranInfo.MiscText2, 50, 40, 9));

            if (ReadInfrastructure2())
            {
              // -----****** Set up page keys to start from the infrastructure 
              // record received from  NEXT
              export.CurrentPage.Count = 1;

              export.PageKeys.Index = 0;
              export.PageKeys.CheckSize();

              export.PageKeys.Update.GexportPageKey.CreatedTimestamp =
                entities.ExistingInfrastructure.CreatedTimestamp;
            }
          }
        }

        if (local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault() > 0
          && local.FindHistCount1.Count == 0)
        {
          if (ReadLegalAction3())
          {
            MoveLegalAction(entities.ExistingLegalAction,
              export.HeaderLegalAction);

            if (ReadTribunal())
            {
              if (ReadFips())
              {
                MoveFips(entities.ExistingFips, export.HeaderFips);
              }
              else if (ReadFipsTribAddress())
              {
                export.HeaderFipsTribAddress.Country =
                  entities.ExistingFipsTribAddress.Country;
              }
            }
          }
        }

        if (local.NextTranInfo.InfrastructureId.GetValueOrDefault() > 0 && local
          .FindHistCount1.Count == 0)
        {
          // -----****** Set up page keys to start from the infrastructure 
          // record received from  NEXT
          export.CurrentPage.Count = 1;

          export.PageKeys.Index = 0;
          export.PageKeys.CheckSize();

          if (ReadInfrastructure1())
          {
            export.PageKeys.Update.GexportPageKey.CreatedTimestamp =
              entities.ExistingInfrastructure.CreatedTimestamp;
          }
          else
          {
            // Use Infrastructure initialized value.
          }
        }

        global.Command = "DISPLAY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    export.Group.Index = -1;

    if (Equal(global.Command, "DISPLAY"))
    {
      // --- The group view is going to be populated. So don't move group 
      // imports to group exports.
    }
    else if (!import.Group.IsEmpty)
    {
      import.Group.Index = 0;

      for(var limit = import.Group.Count; import.Group.Index < limit; ++
        import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        export.Group.Index = import.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.MonitoredDocument.ClosureReasonCode =
          import.Group.Item.MonitoredDocument.ClosureReasonCode;
        export.Group.Update.SpTextWorkArea.Text80 =
          import.Group.Item.SpTextWorkArea.Text80;
        MoveCsePersonsWorkSet(import.Group.Item.CsePersonsWorkSet,
          export.Group.Update.CsePersonsWorkSet);
        export.Group.Update.Narrative.Text1 = import.Group.Item.Narrative.Text1;
        export.Group.Update.LegalAction.CourtCaseNumber =
          import.Group.Item.LegalAction.CourtCaseNumber;
        MoveInfrastructure4(import.Group.Item.Infrastructure,
          export.Group.Update.Infrastructure);

        if (export.Group.Item.Infrastructure.SystemGeneratedIdentifier != 0)
        {
          var field1 = GetField(export.Group.Item.Infrastructure, "caseNumber");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Group.Item.Infrastructure, "csePersonNumber");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        export.Group.Update.Event1.Assign(import.Group.Item.Event1);
        export.Group.Update.CaseUnitFunctionAssignmt.Function =
          import.Group.Item.CaseUnitFunctionAssignmt.Function;

        if (!IsEmpty(export.Group.Item.Infrastructure.CaseNumber))
        {
          local.TextWorkArea.Text10 =
            export.Group.Item.Infrastructure.CaseNumber ?? Spaces(10);
          UseEabPadLeftWithZeros();
          export.Group.Update.Infrastructure.CaseNumber =
            local.TextWorkArea.Text10;
        }

        if (!IsEmpty(export.Group.Item.Infrastructure.CsePersonNumber))
        {
          local.TextWorkArea.Text10 =
            export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);
          UseEabPadLeftWithZeros();
          export.Group.Update.Infrastructure.CsePersonNumber =
            local.TextWorkArea.Text10;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == '*')
        {
          export.Group.Update.Common.SelectChar = "";
        }
      }

      import.Group.CheckIndex();

      // ------******  Move Scroll indicator
      export.Scroll.ScrollingMessage = import.Scroll.ScrollingMessage;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY"))
    {
      // *** Problem report H00103567
      // *** 09/19/00 SWSRCHF
      // *** start
      if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
      {
        local.WorkCase.Number = export.HeaderInfrastructure.CaseNumber ?? Spaces
          (10);
      }
      else if (!IsEmpty(export.HeaderInfrastructure.CsePersonNumber))
      {
        local.WorkCsePerson.Number =
          export.HeaderInfrastructure.CsePersonNumber ?? Spaces(10);
      }

      // *** end
      // *** 09/19/00 SWSRCHF
      // *** Problem report H00103567
      // *** Problem report H00103567
      // *** 09/19/00 SWSRCHF
      // *** view matched CSE_Person and Case
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -----****** Select processing
    if (Equal(global.Command, "ADD") || Equal(global.Command, "NATE") || Equal
      (global.Command, "DETAIL") || Equal(global.Command, "DMON") || Equal
      (global.Command, "EVLS") || Equal(global.Command, "CSLN"))
    {
      local.Count.Count = 0;

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (export.Group.Item.Infrastructure.SystemGeneratedIdentifier != 0)
        {
          var field1 = GetField(export.Group.Item.Infrastructure, "caseNumber");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Group.Item.Infrastructure, "csePersonNumber");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          local.Pointer.Count = export.Group.Index + 1;
          ++local.Count.Count;
          MoveInfrastructure2(export.Group.Item.Infrastructure,
            export.SelectedInfrastructure);
        }
      }

      export.Group.CheckIndex();

      switch(local.Count.Count)
      {
        case 0:
          if (Equal(global.Command, "CSLN"))
          {
            goto Test1;
          }

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        case 1:
          break;
        default:
          export.SelectedInfrastructure.SystemGeneratedIdentifier = 0;
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
      }
    }

Test1:

    switch(TrimEnd(global.Command))
    {
      case "DETAIL":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        if (Equal(export.Group.Item.Infrastructure.EventType, "HIST"))
        {
          ExitState = "SP0000_INVALID_DETAIL_LINK";

          return;
        }

        if (ReadInfrastructure3())
        {
          if (Equal(entities.ExistingInfrastructure.EventType, "DOCUMENT"))
          {
            export.SelectedInfrastructure.
              Assign(entities.ExistingInfrastructure);
            ExitState = "ECO_LNK_TO_DDOC";

            return;
          }

          if (!IsEmpty(entities.ExistingInfrastructure.UserId) && Length
            (TrimEnd(entities.ExistingInfrastructure.UserId)) == 4)
          {
            local.NextTranInfo.InfrastructureId =
              entities.ExistingInfrastructure.SystemGeneratedIdentifier;
            local.NextTranInfo.LastTran = "HIST";
            local.NextTranInfo.MiscNum1 =
              entities.ExistingInfrastructure.DenormNumeric12;
            local.NextTranInfo.CsePersonNumber =
              entities.ExistingInfrastructure.CsePersonNumber;
            local.NextTranInfo.CaseNumber =
              entities.ExistingInfrastructure.CaseNumber;

            // Changes by C Galka
            // Use MISC_TEXT_1 and MISC_TEXT_2 to hold the criteria to enable us
            // to reposition the screen to the current value when we return
            // from a DETAILd screen.
            // In MISC_TEXT_1 store a literal HIST to identify it was populated 
            // in the this procedure. Follow that by Case, Person and the Court
            // order data which includes state, county, and country and
            // function.
            local.NextTranInfo.MiscText1 = "HIST" + (
              export.HeaderInfrastructure.CaseNumber ?? "") + (
                export.HeaderInfrastructure.CsePersonNumber ?? "") + (
                export.HeaderLegalAction.CourtCaseNumber ?? "") + export
              .HeaderFips.StateAbbreviation + (
                export.HeaderFips.CountyAbbreviation ?? "") + (
                export.HeaderFipsTribAddress.Country ?? "") + (
                export.HeaderInfrastructure.Function ?? "");

            // In MISC_TEXT_2 store a literal HIST to identify it was populated 
            // in the this procedure. Follow that by Date, first infrastructure
            // ID on page, Type, and selected infrastructure id.
            local.NextTranInfo.MiscText2 = "HIST" + NumberToString
              (DateToInt(Date(export.HeaderStartDate.Timestamp)), 8, 8);

            import.Group.Index = 0;
            import.Group.CheckSize();

            local.NextTranInfo.MiscText2 =
              TrimEnd(local.NextTranInfo.MiscText2) + NumberToString
              (import.Group.Item.Infrastructure.SystemGeneratedIdentifier, 15);
            local.NextTranInfo.MiscText2 =
              TrimEnd(local.NextTranInfo.MiscText2) + export
              .HeaderInfrastructure.EventType + NumberToString
              (entities.ExistingInfrastructure.SystemGeneratedIdentifier, 7, 9);
              

            switch(TrimEnd(entities.ExistingInfrastructure.BusinessObjectCd))
            {
              case "ADA":
                break;
              case "BKR":
                break;
              case "CON":
                break;
              case "CPA":
                break;
              case "CPR":
                break;
              case "CSW":
                break;
              case "CUF":
                break;
              case "FPL":
                break;
              case "FTR":
                break;
              case "GNT":
                break;
              case "HIN":
                break;
              case "ICS":
                break;
              case "INC":
                break;
              case "IRQ":
                break;
              case "ISC":
                break;
              case "LEA":
                local.NextTranInfo.LegalActionIdentifier =
                  (int?)entities.ExistingInfrastructure.DenormNumeric12;

                break;
              case "LRF":
                break;
              case "MIL":
                break;
              case "OAA":
                break;
              case "OBL":
                break;
              case "OBT":
                break;
              case "PAR":
                break;
              case "PGT":
                break;
              case "PHI":
                break;
              case "PIH":
                break;
              case "PPR":
                break;
              case "RCP":
                break;
              case "TRB":
                break;
              default:
                break;
            }

            export.Standard.NextTransaction =
              entities.ExistingInfrastructure.UserId;
            UseScCabNextTranPut2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Standard.NextTransaction = "";
              ExitState = "SP0000_INVALID_DETAIL_LINK";
            }

            return;
          }
          else if (entities.ExistingInfrastructure.EventId == 12 || entities
            .ExistingInfrastructure.EventId == 13 || entities
            .ExistingInfrastructure.EventId == 14 || entities
            .ExistingInfrastructure.EventId == 15 || entities
            .ExistingInfrastructure.EventId == 16 || entities
            .ExistingInfrastructure.EventId == 17)
          {
            // --------------------------------------------------------------------------------------
            // WR# 020332:  When PF15 (Detail) pressed on HIST by selecting a 
            // CSENET record, the system must  flow to ICAS instead of ISTM
            // screen with next_tran info. The following code is added as part
            // of this work request.
            // Instead of passing the Interstate_case 'trans_serial_number' (
            // denorm_text_12 or denorm_numeric_12 on Infrastructure record)  to
            // the 'Next_tran_put'  CAB,  pass the Infrastructure
            // system_generated_ID into Next_tran_put and in  ICAS PSTEP extract
            // the 'Interstate_case'  identifiers ( 'trans_serial_number' and '
            // transaction_date') from infrastructure record  based on the
            // Infrastructure system_ generated_ID.
            // The reason for passing Infrastructure system_generated_Id to '
            // Next_tran_put' CAB is, the next_transaction entity has no
            // attribute of type 'Date' to set the 'denorm_date' (ie,
            // transaction_date) of infrastructure record.
            //                                                             
            // Vithal(07/25/02)
            // ---------------------------------------------------------------------------------------
            export.Standard.NextTransaction = "ICAS";

            // --------------------------------------------------------------------------------------
            // User wants the ability to flow back to this screen from ICAS by 
            // pressing PF9 (Return) on ICAS. So collect the data  necessary to
            // redisplay HIST after coming back from ICAS.
            //                                                             
            // Vithal(08/16/2002)
            // --------------------------------------------------------------------------------------
            local.NextTranInfo.LastTran = "HIST";
            local.NextTranInfo.MiscNum1 =
              entities.ExistingInfrastructure.DenormNumeric12;
            local.NextTranInfo.CsePersonNumber =
              entities.ExistingInfrastructure.CsePersonNumber;
            local.NextTranInfo.CaseNumber =
              entities.ExistingInfrastructure.CaseNumber;

            // Use MISC_TEXT_1 and MISC_TEXT_2 to hold the criteria to enable us
            // to reposition the screen to the current value when we return
            // from a DETAILd screen.
            // In MISC_TEXT_1 store a literal HIST to identify it was populated 
            // in the this procedure. Follow that by Case, Person and the Court
            // order data which includes state, county, and country and
            // function.
            local.NextTranInfo.MiscText1 = "HIST" + (
              export.HeaderInfrastructure.CaseNumber ?? "") + (
                export.HeaderInfrastructure.CsePersonNumber ?? "") + (
                export.HeaderLegalAction.CourtCaseNumber ?? "") + export
              .HeaderFips.StateAbbreviation + (
                export.HeaderFips.CountyAbbreviation ?? "") + (
                export.HeaderFipsTribAddress.Country ?? "") + (
                export.HeaderInfrastructure.Function ?? "");

            // In MISC_TEXT_2 store a literal HIST to identify it was populated 
            // in the this procedure. Follow that by Date, first infrastructure
            // ID on page, Type, and selected infrastructure id.
            local.NextTranInfo.MiscText2 = "HIST" + NumberToString
              (DateToInt(Date(export.HeaderStartDate.Timestamp)), 8, 8);

            import.Group.Index = 0;
            import.Group.CheckSize();

            local.NextTranInfo.MiscText2 =
              TrimEnd(local.NextTranInfo.MiscText2) + NumberToString
              (import.Group.Item.Infrastructure.SystemGeneratedIdentifier, 15);
            local.NextTranInfo.MiscText2 =
              TrimEnd(local.NextTranInfo.MiscText2) + export
              .HeaderInfrastructure.EventType + NumberToString
              (entities.ExistingInfrastructure.SystemGeneratedIdentifier, 7, 9);
              

            // *******************************************************************
            // MCA 5-13-02 PR140965. There was a change made to incomming batch 
            // to set the tran id to denorm text 12 instead of denorm numeric
            // 12, therefore the older transactions will have the numeric
            // populated.  Check text first then numeric.
            // *******************************************************************
            if (!IsEmpty(entities.ExistingInfrastructure.DenormText12) && Verify
              (entities.ExistingInfrastructure.DenormText12, " 1234567890") == 0
              )
            {
              local.NextTranInfo.InfrastructureId =
                entities.ExistingInfrastructure.SystemGeneratedIdentifier;
            }
            else if (!Equal(entities.ExistingInfrastructure.DenormNumeric12, 0))
            {
              local.NextTranInfo.InfrastructureId =
                entities.ExistingInfrastructure.SystemGeneratedIdentifier;
            }
            else
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              if (!IsEmpty(export.Standard.NextTransaction))
              {
                export.Standard.NextTransaction = "";
              }

              ExitState = "SP0000_INVALID_DETAIL_LINK";

              return;
            }

            UseScCabNextTranPut2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (!IsEmpty(export.Standard.NextTransaction))
              {
                export.Standard.NextTransaction = "";
              }

              ExitState = "SP0000_INVALID_DETAIL_LINK";
            }

            return;
          }
          else
          {
            ExitState = "SP0000_INVALID_DETAIL_LINK";

            return;
          }
        }
        else
        {
          if (!IsEmpty(export.Standard.NextTransaction))
          {
            export.Standard.NextTransaction = "";
          }

          ExitState = "SP0000_INVALID_DETAIL_LINK";

          return;
        }

        break;
      case "EVLS":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        if (export.Group.Item.Infrastructure.SystemGeneratedIdentifier != 0)
        {
          // ***** Make sure RECORD IS NOT populated *****
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "SP0000_MUST_SELECT_BLANK_LINE";

          return;
        }

        // **** Selection can only be on a empty row - precursor to an add ****
        export.Event1.Type1 = "EXTERNAL";
        ExitState = "ECO_LNK_TO_EVLS";

        return;
      case "DMON":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        if (export.Group.Item.Infrastructure.SystemGeneratedIdentifier == 0)
        {
          // ***** Make sure RECORD IS an existing record *****
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "SI0000_NO_RECORD_SELECTED";

          return;
        }

        if (!Equal(export.Group.Item.Infrastructure.EventType, "DOCUMENT"))
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "SP0000_NO_MONITORED_DOC_EXISTS";

          return;
        }

        if (ReadMonitoredDocument1())
        {
          ExitState = "ECO_LNK_TO_DMON";
        }
        else
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "SP0000_NO_MONITORED_DOC_EXISTS";
        }

        return;
      case "NATE":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        if (export.Group.Item.Infrastructure.SystemGeneratedIdentifier == 0)
        {
          // ***** Make sure RECORD IS an existing record *****
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "SI0000_NO_RECORD_SELECTED";

          return;
        }

        MoveCsePersonsWorkSet(export.Group.Item.CsePersonsWorkSet,
          export.SelectedCsePersonsWorkSet);
        export.SelectedLegalAction.CourtCaseNumber =
          export.Group.Item.LegalAction.CourtCaseNumber;
        MoveInfrastructure2(export.Group.Item.Infrastructure,
          export.SelectedInfrastructure);
        export.HiddenNextTranInfo.LastTran = "HIST";
        export.Case1.Number = export.HeaderInfrastructure.CaseNumber ?? Spaces
          (10);
        ExitState = "ECO_LNK_TO_NATE";

        return;
      case "CSLN":
        if (IsEmpty(export.SelectedInfrastructure.CaseNumber))
        {
          export.SelectedInfrastructure.CaseNumber =
            export.HeaderInfrastructure.CaseNumber ?? "";
        }

        ExitState = "ECO_XFER_FROM_HIST_TO_CSLN";

        return;
      case "ADD":
        export.Group.Index = local.Pointer.Count - 1;
        export.Group.CheckSize();

        if (export.Group.Item.Infrastructure.SystemGeneratedIdentifier != 0)
        {
          // ***** Make sure RECORD IS NOT populated *****
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "CANNOT_ADD_AN_EXISTING_OCCURRENC";

          return;
        }

        export.Group.Update.Event1.Assign(export.Event1);
        MoveInfrastructure3(export.Group.Item.Infrastructure,
          local.Infrastructure);

        if (IsEmpty(export.Group.Item.Event1.Type1))
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "SP0000_FLOW_TO_EVLS_REQUIRED";

          return;
        }

        if (IsEmpty(export.Group.Item.Infrastructure.CaseNumber))
        {
          var field = GetField(export.Group.Item.Infrastructure, "caseNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else if (!ReadCase())
        {
          var field = GetField(export.Group.Item.Infrastructure, "caseNumber");

          field.Error = true;

          ExitState = "CASE_NF";

          return;
        }

        if (!IsEmpty(export.Group.Item.Infrastructure.CsePersonNumber))
        {
          local.TextWorkArea.Text10 =
            export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);
          UseEabPadLeftWithZeros();
          export.Group.Update.Infrastructure.CsePersonNumber =
            local.TextWorkArea.Text10;
          local.CsePersonsWorkSet.Number =
            export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);
          UseSiReadCsePerson1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field =
              GetField(export.Group.Item.Infrastructure, "csePersonNumber");

            field.Error = true;

            ExitState = "CSE_PERSON_NF";

            return;
          }
        }

        if (IsEmpty(export.Group.Item.Event1.Type1))
        {
          var field = GetField(export.Group.Item.Infrastructure, "eventType");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (ReadEvent())
        {
          local.Infrastructure.EventId = entities.ExistingEvent.ControlNumber;
          local.Infrastructure.EventType = entities.ExistingEvent.Type1;
          local.Infrastructure.BusinessObjectCd =
            entities.ExistingEvent.BusinessObjectCode;

          if (ReadEventDetail2())
          {
            local.Infrastructure.EventDetailName =
              entities.ExistingEventDetail.DetailName;
            local.Infrastructure.InitiatingStateCode =
              entities.ExistingEventDetail.InitiatingStateCode;
            local.Infrastructure.ReasonCode =
              entities.ExistingEventDetail.ReasonCode;
            local.Infrastructure.CsenetInOutCode =
              entities.ExistingEventDetail.CsenetInOutCode;
          }
          else
          {
            ExitState = "SP0000_EVENT_DETAIL_NF";

            return;
          }
        }
        else
        {
          ExitState = "SP0000_EVENT_NF";

          return;
        }

        local.Infrastructure.ProcessStatus = "H";
        local.Infrastructure.UserId = "HIST";
        local.Infrastructure.SituationNumber = 0;
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Protected = false;
          field.Focused = true;

          export.Group.Update.Common.SelectChar = "";
          ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
          global.Command = "DISPLAY";

          return;
        }

        break;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.HeaderInfrastructure.CaseNumber) && IsEmpty
        (export.HeaderInfrastructure.CsePersonNumber) && IsEmpty
        (export.HeaderInfrastructure.EventType) && IsEmpty
        (export.HeaderInfrastructure.Function) && IsEmpty
        (export.HeaderLegalAction.CourtCaseNumber))
      {
        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      local.StartCase.CaseNumber = "";
      local.EndCase.CaseNumber = "9999999999";

      if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
      {
        local.TextWorkArea.Text10 = export.HeaderInfrastructure.CaseNumber ?? Spaces
          (10);
        UseEabPadLeftWithZeros();
        export.HeaderInfrastructure.CaseNumber = local.TextWorkArea.Text10;
        local.StartCase.CaseNumber = export.HeaderInfrastructure.CaseNumber ?? ""
          ;
        local.EndCase.CaseNumber = export.HeaderInfrastructure.CaseNumber ?? "";
      }

      local.StartPerson.CsePersonNumber = "";
      local.EndPerson.CsePersonNumber = "9999999999";

      if (!IsEmpty(export.HeaderInfrastructure.CsePersonNumber))
      {
        local.TextWorkArea.Text10 =
          export.HeaderInfrastructure.CsePersonNumber ?? Spaces(10);
        UseEabPadLeftWithZeros();
        export.HeaderInfrastructure.CsePersonNumber = local.TextWorkArea.Text10;
        local.StartPerson.CsePersonNumber =
          export.HeaderInfrastructure.CsePersonNumber ?? "";
        local.EndPerson.CsePersonNumber =
          export.HeaderInfrastructure.CsePersonNumber ?? "";
      }

      if (AsChar(local.ClearGroupView.Flag) == 'Y')
      {
        // ********** this flag is set in NEXT command processing to
        // * clear the group view in case the next page is not full
        // **********
        export.Group.Count = 0;
      }

      local.Start.CourtCaseNumber = "";
      local.End.CourtCaseNumber = "99999999999999999";
      local.Start.Identifier = 0;
      local.End.Identifier = 999999999;
      local.StartDenormNum.DenormNumeric12 = 0;
      local.EndDenormNum.DenormNumeric12 = 999999999999L;
      local.StartBusobj.BusinessObjectCd = "";
      local.EndBusobj.BusinessObjectCd = "999";

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

        if (!IsEmpty(export.HeaderFips.CountyAbbreviation) && !
          IsEmpty(export.HeaderFips.StateAbbreviation) && IsEmpty
          (export.HeaderFipsTribAddress.Country))
        {
          local.Keep.Flag = "S";
        }
        else if (IsEmpty(export.HeaderFips.CountyAbbreviation) && IsEmpty
          (export.HeaderFips.StateAbbreviation) && !
          IsEmpty(entities.ExistingFipsTribAddress.Country))
        {
          local.Keep.Flag = "C";
        }
        else
        {
          ExitState = "SP0000_ENTER_ST_AND_CO_OR_CY";

          var field1 = GetField(export.HeaderFips, "countyAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.HeaderFips, "stateAbbreviation");

          field2.Error = true;

          var field3 = GetField(export.HeaderFipsTribAddress, "country");

          field3.Error = true;

          return;
        }

        local.Prev.Number = "";
        local.Prev.FormattedName = "";
        local.Count.Count = 0;

        if (AsChar(local.Keep.Flag) == 'S')
        {
          // The finding of the Low and High Legal Action ID for a court case 
          // was changed By Carl Galka on 12/01/1999. It was changed to two READ
          // EACHS that vary depending if State/County or Country are entered
          // in the criteria.
          // Additionally, I found the first 5 specific occurrences to include 
          // in the Infrastructure read each. The 6th and last legal action will
          // be make a range that will need to be narrowed down by subsequent
          // reads of legal action.
          foreach(var item in ReadLegalAction4())
          {
            ++local.Count.Count;
            export.HeaderLegalAction.Identifier =
              entities.ExistingLegalAction.Identifier;

            switch(local.Count.Count)
            {
              case 1:
                local.Position1.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;
                local.Position2.DenormNumeric12 =
                  local.Position1.DenormNumeric12;
                local.Position3.DenormNumeric12 =
                  local.Position2.DenormNumeric12;
                local.Start.Identifier =
                  entities.ExistingLegalAction.Identifier;
                local.StartDenormNum.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;

                break;
              case 2:
                local.Position2.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;
                local.Position3.DenormNumeric12 =
                  local.Position2.DenormNumeric12;
                local.Position4.DenormNumeric12 =
                  local.Position2.DenormNumeric12;
                local.Position5.DenormNumeric12 =
                  local.Position2.DenormNumeric12;
                local.Start.Identifier =
                  entities.ExistingLegalAction.Identifier;
                local.StartDenormNum.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;

                break;
              case 3:
                local.Position3.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;
                local.Position4.DenormNumeric12 =
                  local.Position3.DenormNumeric12;
                local.Position5.DenormNumeric12 =
                  local.Position3.DenormNumeric12;
                local.Start.Identifier =
                  entities.ExistingLegalAction.Identifier;
                local.StartDenormNum.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;

                break;
              case 4:
                local.Position4.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;
                local.Position5.DenormNumeric12 =
                  local.Position4.DenormNumeric12;
                local.Start.Identifier =
                  entities.ExistingLegalAction.Identifier;
                local.StartDenormNum.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;

                break;
              case 5:
                local.Position5.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;
                local.Start.Identifier =
                  entities.ExistingLegalAction.Identifier;
                local.StartDenormNum.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;

                break;
              case 6:
                local.Start.Identifier =
                  entities.ExistingLegalAction.Identifier;
                local.StartDenormNum.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;

                break;
              default:
                break;
            }

            local.EndDenormNum.DenormNumeric12 =
              entities.ExistingLegalAction.Identifier;
            local.End.Identifier = entities.ExistingLegalAction.Identifier;
          }
        }
        else if (AsChar(local.Keep.Flag) == 'C')
        {
          foreach(var item in ReadLegalAction5())
          {
            ++local.Count.Count;
            export.HeaderLegalAction.Identifier =
              entities.ExistingLegalAction.Identifier;

            switch(local.Count.Count)
            {
              case 1:
                local.Position1.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;
                local.Position2.DenormNumeric12 =
                  local.Position1.DenormNumeric12;
                local.Position3.DenormNumeric12 =
                  local.Position2.DenormNumeric12;
                local.Start.Identifier =
                  entities.ExistingLegalAction.Identifier;
                local.StartDenormNum.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;

                break;
              case 2:
                local.Position2.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;
                local.Position3.DenormNumeric12 =
                  local.Position2.DenormNumeric12;
                local.Position4.DenormNumeric12 =
                  local.Position2.DenormNumeric12;
                local.Position5.DenormNumeric12 =
                  local.Position2.DenormNumeric12;
                local.Start.Identifier =
                  entities.ExistingLegalAction.Identifier;
                local.StartDenormNum.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;

                break;
              case 3:
                local.Position3.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;
                local.Position4.DenormNumeric12 =
                  local.Position3.DenormNumeric12;
                local.Position5.DenormNumeric12 =
                  local.Position3.DenormNumeric12;
                local.Start.Identifier =
                  entities.ExistingLegalAction.Identifier;
                local.StartDenormNum.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;

                break;
              case 4:
                local.Position4.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;
                local.Position5.DenormNumeric12 =
                  local.Position4.DenormNumeric12;
                local.Start.Identifier =
                  entities.ExistingLegalAction.Identifier;
                local.StartDenormNum.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;

                break;
              case 5:
                local.Position5.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;
                local.Start.Identifier =
                  entities.ExistingLegalAction.Identifier;
                local.StartDenormNum.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;

                break;
              case 6:
                local.Start.Identifier =
                  entities.ExistingLegalAction.Identifier;
                local.StartDenormNum.DenormNumeric12 =
                  entities.ExistingLegalAction.Identifier;

                break;
              default:
                break;
            }

            local.EndDenormNum.DenormNumeric12 =
              entities.ExistingLegalAction.Identifier;
            local.End.Identifier = entities.ExistingLegalAction.Identifier;
          }
        }

        if (local.Count.Count == 0)
        {
          switch(AsChar(local.Keep.Flag))
          {
            case 'S':
              ExitState = "SP0000_COURT_CASE_ST_CY_COMBO_NF";

              var field1 =
                GetField(export.HeaderLegalAction, "courtCaseNumber");

              field1.Error = true;

              var field2 = GetField(export.HeaderFips, "countyAbbreviation");

              field2.Error = true;

              var field3 = GetField(export.HeaderFips, "stateAbbreviation");

              field3.Error = true;

              return;
            case 'C':
              ExitState = "SP0000_COURT_CASE_ST_CY_COMBO_NF";

              var field4 =
                GetField(export.HeaderLegalAction, "courtCaseNumber");

              field4.Error = true;

              var field5 = GetField(export.HeaderFipsTribAddress, "country");

              field5.Error = true;

              return;
            default:
              break;
          }
        }
        else
        {
          local.Start.CourtCaseNumber =
            export.HeaderLegalAction.CourtCaseNumber ?? "";
          local.End.CourtCaseNumber =
            export.HeaderLegalAction.CourtCaseNumber ?? "";
          local.StartBusobj.BusinessObjectCd = "LEA";
          local.EndBusobj.BusinessObjectCd = "LEA";
        }

        local.Count.Count = 0;
      }

      local.StartType.EventType = "";
      local.EndType.EventType = "999999999999";

      if (!IsEmpty(export.HeaderInfrastructure.EventType))
      {
        // *** Report problem H00078203
        // *** 10/25/99 SWSRCHF
        // *** start
        if (AsChar(local.ReturnFromCodeValue.Flag) == 'Y')
        {
          local.StartType.EventType = export.HeaderInfrastructure.EventType;
          local.EndType.EventType = export.HeaderInfrastructure.EventType;

          goto Test2;
        }

        // *** end
        // *** 10/25/99 SWSRCHF
        // *** Report problem H00078203
        local.Code.CodeName = "EVENT TYPE";
        local.CodeValue.Cdvalue = export.HeaderInfrastructure.EventType;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          ExitState = "SP0000_INVALID_EVENT_TYPE";

          var field = GetField(export.HeaderInfrastructure, "eventType");

          field.Error = true;

          return;
        }

        local.StartType.EventType = export.HeaderInfrastructure.EventType;

        // *** Report problem H00078203
        // *** 10/22/99 SWSRCHF
        local.EndType.EventType = export.HeaderInfrastructure.EventType;
      }

Test2:

      local.StartFunction.Function = "";
      local.EndFunction.Function = "999";

      if (!IsEmpty(export.HeaderInfrastructure.Function))
      {
        local.Code.CodeName = "FUNCTION";
        local.CodeValue.Cdvalue = export.HeaderInfrastructure.Function ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          ExitState = "SP0000_INVALID_FUNCTION";

          var field = GetField(export.HeaderInfrastructure, "function");

          field.Error = true;

          return;
        }

        local.StartFunction.Function = export.HeaderInfrastructure.Function ?? ""
          ;
        local.EndFunction.Function = export.HeaderInfrastructure.Function ?? "";
      }

      // -----*****  Handle paging for first time into PRAD
      if (export.CurrentPage.Count == 0)
      {
        export.Scroll.ScrollingMessage = "More";
        export.CurrentPage.Count = 1;

        export.PageKeys.Index = 0;
        export.PageKeys.CheckSize();
      }

      export.Group.Index = -1;

      if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
      {
        // *****  Read on Case key  ****
        // -- 09/21/2010 GVandy CQ22068 Emergency Fix to correct inefficient 
        // index path chosen after DB2 V9 upgrade.
        //    Adding "or 0 = 1" condition to person number qualification to 
        // alter index from CKI03777 back to the original CKI02777.
        local.ConstantZero.Count = 0;
        local.ConstantOne.Count = 1;

        foreach(var item in ReadInfrastructure5())
        {
          // CQ61838 added if log to diary ind is not equal to 'Y', next
          if (!ReadEventDetail3())
          {
            continue;
          }

          if (AsChar(entities.ExistingEventDetail.LogToDiaryInd) == 'Y')
          {
          }
          else
          {
            continue;
          }

          // *** 10/18/99 SWSRCHF
          // *** Problem report H00076959
          local.LegalAction.CourtCaseNumber = local.Initialised.CourtCaseNumber;

          if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
          {
            if (!Equal(entities.ExistingInfrastructure.BusinessObjectCd, "LEA"))
            {
              continue;
            }

            // *** 10/18/99 SWSRCHF
            // *** Problem report H00076959
            if (!Equal(entities.ExistingInfrastructure.DenormNumeric12, 0))
            {
              local.PerformanceLegalAction.Identifier =
                (int)entities.ExistingInfrastructure.DenormNumeric12.
                  GetValueOrDefault();

              if (ReadLegalAction1())
              {
                // *** Problem report H00076070
                // *** 10/25/99 SWSRCHF
                // *** start
                switch(AsChar(local.Keep.Flag))
                {
                  case 'S':
                    if (!ReadFipsFipsTribAddress1())
                    {
                      continue;
                    }

                    break;
                  case 'C':
                    if (!ReadFipsFipsTribAddress2())
                    {
                      continue;
                    }

                    break;
                  default:
                    break;
                }

                // *** end
                // *** 10/25/99 SWSRCHF
                // *** Problem report H00076070
                local.LegalAction.CourtCaseNumber =
                  entities.ExistingLegalAction.CourtCaseNumber;
              }
              else
              {
                continue;
              }
            }
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          if (local.Reposition.SystemGeneratedIdentifier == entities
            .ExistingInfrastructure.SystemGeneratedIdentifier)
          {
            export.Group.Update.Common.SelectChar = "S";

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }

          if (export.Group.Index >= Export.GroupGroup.Capacity)
          {
            if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
            {
              ++export.PageKeys.Index;
              export.PageKeys.CheckSize();

              export.PageKeys.Update.GexportPageKey.CreatedTimestamp =
                entities.ExistingInfrastructure.CreatedTimestamp;
            }
            else
            {
              ExitState = "SP0000_LIST_IS_FULL";
            }

            break;
          }

          MoveInfrastructure2(entities.ExistingInfrastructure,
            export.Group.Update.Infrastructure);
          export.Group.Update.LegalAction.CourtCaseNumber =
            local.LegalAction.CourtCaseNumber;

          // *** 10/18/99 SWSRCHF
          // *** Problem report H00076959
          // *** start
          export.Group.Update.Narrative.Text1 = "";

          // *** Work Request 000170
          // *** 06/08/00 SWSRCHF
          // *** start
          // *** Read for Narrative Detail existence
          if (ReadNarrativeDetail())
          {
            export.Group.Update.Narrative.Text1 = "*";
          }

          // *** end
          // *** 06/08/00 SWSRCHF
          // *** Work Request 000170
          // *** end
          // *** 10/18/99 SWSRCHF
          // *** Problem report H00076959
          // -----***** Even if Court Case filters were not used, we need to 
          // display Court Case numbers if they exist.
          if (Equal(entities.ExistingInfrastructure.BusinessObjectCd, "LEA"))
          {
            if (!Equal(local.LegalAction.CourtCaseNumber,
              local.Initialised.CourtCaseNumber))
            {
              continue;
            }

            // *** 10/18/99 SWSRCHF
            // *** Problem report H00076959
            if (!Equal(entities.ExistingInfrastructure.DenormNumeric12, 0))
            {
              local.PerformanceLegalAction.Identifier =
                (int)entities.ExistingInfrastructure.DenormNumeric12.
                  GetValueOrDefault();

              if (ReadLegalAction2())
              {
                // *** 10/18/99 SWSRCHF
                // *** Problem report H00076959
                export.Group.Update.LegalAction.CourtCaseNumber =
                  entities.ExistingLegalAction.CourtCaseNumber;
              }
            }
          }

          if (!IsEmpty(export.Group.Item.Infrastructure.CsePersonNumber))
          {
            export.Group.Update.CsePersonsWorkSet.Number =
              export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);

            if (Equal(local.Prev.Number,
              export.Group.Item.Infrastructure.CsePersonNumber))
            {
              export.Group.Update.CsePersonsWorkSet.FormattedName =
                local.Prev.FormattedName;

              goto Test3;
            }

            UseSiReadCsePerson2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
              export.Group.Update.CsePersonsWorkSet.FormattedName = "**Name NF";
            }

            local.Prev.Number =
              export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);
            local.Prev.FormattedName =
              export.Group.Item.CsePersonsWorkSet.FormattedName;
          }
          else
          {
            export.Group.Update.CsePersonsWorkSet.FormattedName = "";
            local.Prev.Number = "";
            local.Prev.FormattedName = "";
          }

Test3:

          export.Group.Update.MonitoredDocument.ClosureReasonCode = "";

          if (Equal(entities.ExistingInfrastructure.EventType, "DOCUMENT"))
          {
            // -----***** Read closure reason code for a monitored document
            if (ReadMonitoredDocument2())
            {
              export.Group.Update.MonitoredDocument.ClosureReasonCode =
                entities.ExistingMonitoredDocument.ClosureReasonCode;
            }
            else
            {
              // -----***** May not be a monitored document
            }
          }
        }
      }
      else if (!IsEmpty(export.HeaderInfrastructure.CsePersonNumber))
      {
        // *****  Read on CSE_Person key  ****
        foreach(var item in ReadInfrastructure6())
        {
          // CQ61838 added if log to diary ind is not equal to 'Y', next
          if (!ReadEventDetail3())
          {
            continue;
          }

          if (AsChar(entities.ExistingEventDetail.LogToDiaryInd) == 'Y')
          {
          }
          else
          {
            continue;
          }

          // *** 10/18/99 SWSRCHF
          // *** Problem report H00076959
          local.LegalAction.CourtCaseNumber = local.Initialised.CourtCaseNumber;

          if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
          {
            if (!Equal(entities.ExistingInfrastructure.BusinessObjectCd, "LEA"))
            {
              continue;
            }

            // *** 10/18/99 SWSRCHF
            // *** Problem report H00076959
            if (!Equal(entities.ExistingInfrastructure.DenormNumeric12, 0))
            {
              local.PerformanceLegalAction.Identifier =
                (int)entities.ExistingInfrastructure.DenormNumeric12.
                  GetValueOrDefault();

              if (ReadLegalAction1())
              {
                // *** Problem report H00076070
                // *** 10/25/99 SWSRCHF
                // *** start
                switch(AsChar(local.Keep.Flag))
                {
                  case 'S':
                    if (!ReadFipsFipsTribAddress1())
                    {
                      continue;
                    }

                    break;
                  case 'C':
                    if (!ReadFipsFipsTribAddress2())
                    {
                      continue;
                    }

                    break;
                  default:
                    break;
                }

                // *** end
                // *** 10/25/99 SWSRCHF
                // *** Problem report H00076070
                local.LegalAction.CourtCaseNumber =
                  entities.ExistingLegalAction.CourtCaseNumber;
              }
              else
              {
                continue;
              }
            }
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          if (local.Reposition.SystemGeneratedIdentifier == entities
            .ExistingInfrastructure.SystemGeneratedIdentifier)
          {
            export.Group.Update.Common.SelectChar = "S";

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }

          if (export.Group.Index >= Export.GroupGroup.Capacity)
          {
            if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
            {
              ++export.PageKeys.Index;
              export.PageKeys.CheckSize();

              export.PageKeys.Update.GexportPageKey.CreatedTimestamp =
                entities.ExistingInfrastructure.CreatedTimestamp;
            }
            else
            {
              ExitState = "SP0000_LIST_IS_FULL";
            }

            break;
          }

          MoveInfrastructure2(entities.ExistingInfrastructure,
            export.Group.Update.Infrastructure);
          export.Group.Update.LegalAction.CourtCaseNumber =
            local.LegalAction.CourtCaseNumber;

          // *** 10/18/99 SWSRCHF
          // *** Problem report H00076959
          // *** start
          export.Group.Update.Narrative.Text1 = "";

          // *** Work Request 000170
          // *** 06/08/00 SWSRCHF
          // *** start
          // *** Read for Narrative Detail existence
          if (ReadNarrativeDetail())
          {
            export.Group.Update.Narrative.Text1 = "*";
          }

          // *** end
          // *** 06/08/00 SWSRCHF
          // *** Work Request 000170
          // *** end
          // *** 10/18/99 SWSRCHF
          // *** Problem report H00076959
          // -----***** Even if Court Case filters were not used, we need to 
          // display Court Case numbers if they exist.
          if (Equal(entities.ExistingInfrastructure.BusinessObjectCd, "LEA"))
          {
            if (!Equal(local.LegalAction.CourtCaseNumber,
              local.Initialised.CourtCaseNumber))
            {
              continue;
            }

            // *** 10/18/99 SWSRCHF
            // *** Problem report H00076959
            if (!Equal(entities.ExistingInfrastructure.DenormNumeric12, 0))
            {
              local.PerformanceLegalAction.Identifier =
                (int)entities.ExistingInfrastructure.DenormNumeric12.
                  GetValueOrDefault();

              if (ReadLegalAction2())
              {
                // *** 10/18/99 SWSRCHF
                // ***Problem report H00076959
                export.Group.Update.LegalAction.CourtCaseNumber =
                  entities.ExistingLegalAction.CourtCaseNumber;
              }
            }
          }

          if (!IsEmpty(export.Group.Item.Infrastructure.CsePersonNumber))
          {
            export.Group.Update.CsePersonsWorkSet.Number =
              export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);

            if (Equal(local.Prev.Number,
              export.Group.Item.Infrastructure.CsePersonNumber))
            {
              export.Group.Update.CsePersonsWorkSet.FormattedName =
                local.Prev.FormattedName;

              goto Test4;
            }

            UseSiReadCsePerson2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
              export.Group.Update.CsePersonsWorkSet.FormattedName = "**Name NF";
            }

            local.Prev.Number =
              export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);
            local.Prev.FormattedName =
              export.Group.Item.CsePersonsWorkSet.FormattedName;
          }
          else
          {
            export.Group.Update.CsePersonsWorkSet.FormattedName = "";
            local.Prev.Number = "";
            local.Prev.FormattedName = "";
          }

Test4:

          export.Group.Update.MonitoredDocument.ClosureReasonCode = "";

          if (Equal(entities.ExistingInfrastructure.EventType, "DOCUMENT"))
          {
            // -----***** Read closure reason code for a monitored document
            if (ReadMonitoredDocument2())
            {
              export.Group.Update.MonitoredDocument.ClosureReasonCode =
                entities.ExistingMonitoredDocument.ClosureReasonCode;
            }
            else
            {
              // -----***** May not be a monitored document
            }
          }
        }
      }
      else if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
      {
        // *
        // * Set the denorm numeric 12 to the court case number. I
        // * realize that
        // the court case number is 17 characters and
        // * the denorm numeric
        // 12 is only 12. But, this will still help
        // 
        // * the performance by having db2 eliminate most of the
        // *
        // unnecessary rows. LEA rows will have the court case
        // * number or
        // spaces. If there are spaces, we will still need
        // * to look at legal
        // _action. ....... Carl Galka 02/22/2000
        // *
        local.Infrastructure.DenormText12 =
          export.HeaderLegalAction.CourtCaseNumber ?? "";

        // *****  Read on Legal Action  key  ****
        foreach(var item in ReadInfrastructure7())
        {
          // CQ61838 added if log to diary ind is not equal to 'Y', next
          if (!ReadEventDetail3())
          {
            continue;
          }

          if (AsChar(entities.ExistingEventDetail.LogToDiaryInd) == 'Y')
          {
          }
          else
          {
            continue;
          }

          // *** Problem report H00103567
          // *** 09/19/00 SWSRCHF
          // *** start
          if (!IsEmpty(entities.ExistingInfrastructure.CsePersonNumber))
          {
            local.WorkCsePerson.Number =
              entities.ExistingInfrastructure.CsePersonNumber ?? Spaces(10);
          }
          else if (!IsEmpty(entities.ExistingInfrastructure.CaseNumber))
          {
            local.WorkCase.Number =
              entities.ExistingInfrastructure.CaseNumber ?? Spaces(10);
          }

          UseScSecurityValidAuthForFv();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";

            continue;
          }

          // *** end
          // *** 09/19/00 SWSRCHF
          // *** Problem report H00103567
          // *** 10/18/99 SWSRCHF
          // *** Problem report H00076959
          local.LegalAction.CourtCaseNumber = local.Initialised.CourtCaseNumber;

          if (!IsEmpty(export.HeaderLegalAction.CourtCaseNumber))
          {
            // *
            // * Only Infrastructure records with BUSINESS_OBJECT_CD
            // 
            // * of LEA will have a court case.  That test is in the
            // * READ EACH
            // ........ Carl Galka 02/22/2000
            // *
            // *** 10/18/99 SWSRCHF
            // *** Problem report H00076959
            if (!Equal(entities.ExistingInfrastructure.DenormNumeric12, 0))
            {
              local.PerformanceLegalAction.Identifier =
                (int)entities.ExistingInfrastructure.DenormNumeric12.
                  GetValueOrDefault();

              if (ReadLegalAction1())
              {
                // *** Problem report H00076070
                // *** 10/25/99 SWSRCHF
                // *** start
                switch(AsChar(local.Keep.Flag))
                {
                  case 'S':
                    if (!ReadFipsFipsTribAddress1())
                    {
                      continue;
                    }

                    break;
                  case 'C':
                    if (!ReadFipsFipsTribAddress2())
                    {
                      continue;
                    }

                    break;
                  default:
                    break;
                }

                // *** end
                // *** 10/25/99 SWSRCHF
                // *** Problem report H00076070
                local.LegalAction.CourtCaseNumber =
                  entities.ExistingLegalAction.CourtCaseNumber;
              }
              else
              {
                continue;
              }
            }
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          if (local.Reposition.SystemGeneratedIdentifier == entities
            .ExistingInfrastructure.SystemGeneratedIdentifier)
          {
            export.Group.Update.Common.SelectChar = "S";

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }

          if (export.Group.Index >= Export.GroupGroup.Capacity)
          {
            if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
            {
              ++export.PageKeys.Index;
              export.PageKeys.CheckSize();

              export.PageKeys.Update.GexportPageKey.CreatedTimestamp =
                entities.ExistingInfrastructure.CreatedTimestamp;
            }
            else
            {
              ExitState = "SP0000_LIST_IS_FULL";
            }

            break;
          }

          MoveInfrastructure2(entities.ExistingInfrastructure,
            export.Group.Update.Infrastructure);
          export.Group.Update.LegalAction.CourtCaseNumber =
            local.LegalAction.CourtCaseNumber;

          // *** 10/18/99 SWSRCHF
          // *** Problem report H00076959
          // *** start
          export.Group.Update.Narrative.Text1 = "";

          // *** Work Request 000170
          // *** 06/08/00 SWSRCHF
          // *** start
          // *** Read for Narrative Detail existence
          if (ReadNarrativeDetail())
          {
            export.Group.Update.Narrative.Text1 = "*";
          }

          // *** end
          // *** 06/08/00 SWSRCHF
          // *** Work Request 000170
          // *** end
          // *** 10/18/99 SWSRCHF
          // *** Problem report H00076959
          // -----***** Even if Court Case filters were not used, we need to 
          // display Court Case numbers if they exist.
          if (Equal(entities.ExistingInfrastructure.BusinessObjectCd, "LEA"))
          {
            if (!Equal(local.LegalAction.CourtCaseNumber,
              local.Initialised.CourtCaseNumber))
            {
              continue;
            }

            // *** 10/18/99 SWSRCHF
            // *** Problem report H00076959
            if (!Equal(entities.ExistingInfrastructure.DenormNumeric12, 0))
            {
              local.PerformanceLegalAction.Identifier =
                (int)entities.ExistingInfrastructure.DenormNumeric12.
                  GetValueOrDefault();

              if (ReadLegalAction2())
              {
                // *** 10/18/99 SWSRCHF
                // *** Problem report H00076959
                export.Group.Update.LegalAction.CourtCaseNumber =
                  entities.ExistingLegalAction.CourtCaseNumber;
              }
            }
          }

          if (!IsEmpty(export.Group.Item.Infrastructure.CsePersonNumber))
          {
            export.Group.Update.CsePersonsWorkSet.Number =
              export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);

            if (Equal(local.Prev.Number,
              export.Group.Item.Infrastructure.CsePersonNumber))
            {
              export.Group.Update.CsePersonsWorkSet.FormattedName =
                local.Prev.FormattedName;

              goto Test5;
            }

            UseSiReadCsePerson2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
              export.Group.Update.CsePersonsWorkSet.FormattedName = "**Name NF";
            }

            local.Prev.Number =
              export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);
            local.Prev.FormattedName =
              export.Group.Item.CsePersonsWorkSet.FormattedName;
          }
          else
          {
            export.Group.Update.CsePersonsWorkSet.FormattedName = "";
            local.Prev.Number = "";
            local.Prev.FormattedName = "";
          }

Test5:

          export.Group.Update.MonitoredDocument.ClosureReasonCode = "";

          if (Equal(entities.ExistingInfrastructure.EventType, "DOCUMENT"))
          {
            // -----***** Read closure reason code for a monitored document
            if (ReadMonitoredDocument2())
            {
              export.Group.Update.MonitoredDocument.ClosureReasonCode =
                entities.ExistingMonitoredDocument.ClosureReasonCode;
            }
            else
            {
              // -----***** May not be a monitored document
            }
          }
        }
      }
      else
      {
        // *****  Read on TimeStamp key  ****
        foreach(var item in ReadInfrastructure8())
        {
          // CQ61838 added if log to diary ind is not equal to 'Y', next
          if (!ReadEventDetail3())
          {
            continue;
          }

          if (AsChar(entities.ExistingEventDetail.LogToDiaryInd) == 'Y')
          {
          }
          else
          {
            continue;
          }

          // *** Problem report H00103567
          // *** 09/19/00 SWSRCHF
          // *** start
          if (!IsEmpty(entities.ExistingInfrastructure.CsePersonNumber))
          {
            local.WorkCsePerson.Number =
              entities.ExistingInfrastructure.CsePersonNumber ?? Spaces(10);
          }
          else if (!IsEmpty(entities.ExistingInfrastructure.CaseNumber))
          {
            local.WorkCase.Number =
              entities.ExistingInfrastructure.CaseNumber ?? Spaces(10);
          }

          UseScSecurityValidAuthForFv();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";

            continue;
          }

          // *** end
          // *** 09/19/00 SWSRCHF
          // *** Problem report H00103567
          ++export.Group.Index;
          export.Group.CheckSize();

          if (local.Reposition.SystemGeneratedIdentifier == entities
            .ExistingInfrastructure.SystemGeneratedIdentifier)
          {
            export.Group.Update.Common.SelectChar = "S";

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }

          if (export.Group.Index >= Export.GroupGroup.Capacity)
          {
            if (export.PageKeys.Index + 1 != Export.PageKeysGroup.Capacity)
            {
              ++export.PageKeys.Index;
              export.PageKeys.CheckSize();

              export.PageKeys.Update.GexportPageKey.CreatedTimestamp =
                entities.ExistingInfrastructure.CreatedTimestamp;
            }
            else
            {
              ExitState = "SP0000_LIST_IS_FULL";
            }

            break;
          }

          MoveInfrastructure2(entities.ExistingInfrastructure,
            export.Group.Update.Infrastructure);
          export.Group.Update.LegalAction.CourtCaseNumber =
            local.LegalAction.CourtCaseNumber;

          // *** 10/18/99 SWSRCHF
          // *** Problem report H00076959
          // *** start
          export.Group.Update.Narrative.Text1 = "";

          // *** Work Request 000170
          // *** 06/08/00 SWSRCHF
          // *** start
          // *** Read for Narrative Detail existence
          if (ReadNarrativeDetail())
          {
            export.Group.Update.Narrative.Text1 = "*";
          }

          // *** end
          // *** 06/08/00 SWSRCHF
          // *** Work Request 000170
          // *** end
          // *** 10/18/99 SWSRCHF
          // *** Problem report H00076959
          // -----***** Even if Court Case filters were not used, we need to 
          // display Court Case numbers if they exist.
          if (Equal(entities.ExistingInfrastructure.BusinessObjectCd, "LEA"))
          {
            if (!Equal(local.LegalAction.CourtCaseNumber,
              local.Initialised.CourtCaseNumber))
            {
              continue;
            }

            // *** 10/18/99 SWSRCHF
            // *** Problem report H00076959
            if (!Equal(entities.ExistingInfrastructure.DenormNumeric12, 0))
            {
              local.PerformanceLegalAction.Identifier =
                (int)entities.ExistingInfrastructure.DenormNumeric12.
                  GetValueOrDefault();

              if (ReadLegalAction2())
              {
                // *** 10/18/99 SWSRCHF
                // *** Problem report H00076959
                export.Group.Update.LegalAction.CourtCaseNumber =
                  entities.ExistingLegalAction.CourtCaseNumber;
              }
            }
          }

          if (!IsEmpty(export.Group.Item.Infrastructure.CsePersonNumber))
          {
            export.Group.Update.CsePersonsWorkSet.Number =
              export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);

            if (Equal(local.Prev.Number,
              export.Group.Item.Infrastructure.CsePersonNumber))
            {
              export.Group.Update.CsePersonsWorkSet.FormattedName =
                local.Prev.FormattedName;

              goto Test6;
            }

            UseSiReadCsePerson2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
              export.Group.Update.CsePersonsWorkSet.FormattedName = "**Name NF";
            }

            local.Prev.Number =
              export.Group.Item.Infrastructure.CsePersonNumber ?? Spaces(10);
            local.Prev.FormattedName =
              export.Group.Item.CsePersonsWorkSet.FormattedName;
          }
          else
          {
            export.Group.Update.CsePersonsWorkSet.FormattedName = "";
            local.Prev.Number = "";
            local.Prev.FormattedName = "";
          }

Test6:

          export.Group.Update.MonitoredDocument.ClosureReasonCode = "";

          if (Equal(entities.ExistingInfrastructure.EventType, "DOCUMENT"))
          {
            // -----***** Read closure reason code for a monitored document
            if (ReadMonitoredDocument2())
            {
              export.Group.Update.MonitoredDocument.ClosureReasonCode =
                entities.ExistingMonitoredDocument.ClosureReasonCode;
            }
            else
            {
              // -----***** May not be a monitored document
            }
          }
        }
      }

      // **** The following disabled code - disabled to try to improve the read-
      // eachs - see above code. -- swsrkeh ****
      if (export.CurrentPage.Count == 1 && export.Group.IsEmpty)
      {
        export.CurrentPage.Count = 0;

        // -----***** The only time this can happen is when no data is found for
        // search criteria entered.
        export.Scroll.ScrollingMessage = "More";
        ExitState = "SP0000_NO_HIST_REC_TO_DISPLAY";
      }
      else if (export.CurrentPage.Count == 1 && !export.Group.IsFull)
      {
        export.Scroll.ScrollingMessage = "More";
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
      else if (export.CurrentPage.Count == 1 && export.Group.IsFull)
      {
        export.Scroll.ScrollingMessage = "More";
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        if (export.PageKeys.Count > 1)
        {
          export.Scroll.ScrollingMessage = "More +";
        }
      }
      else if (export.CurrentPage.Count > 1 && !export.Group.IsFull)
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        export.Scroll.ScrollingMessage = "More -";
      }
      else if (export.CurrentPage.Count > 1 && export.Group.IsFull)
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        if (export.PageKeys.Count > export.CurrentPage.Count)
        {
          export.Scroll.ScrollingMessage = "More -+";
        }
        else
        {
          export.Scroll.ScrollingMessage = "More -";
        }
      }

      MoveDateWorkArea(export.HeaderStartDate,
        export.HiddenExportHeaderStartDate);
      MoveFips(export.HeaderFips, export.HiddenExportHeaderFips);
      export.HiddenExportHeaderInfrastructure.
        Assign(export.HeaderInfrastructure);
      export.HiddenExportHeaderLegalAction.CourtCaseNumber =
        export.HeaderLegalAction.CourtCaseNumber;
      export.HiddenExportHeaderFipsTribAddress.Country =
        export.HeaderFipsTribAddress.Country;
    }

    export.Group.Index = -1;

    do
    {
      ++export.Group.Index;
      export.Group.CheckSize();

      if (export.Group.Item.Infrastructure.SystemGeneratedIdentifier != 0)
      {
        var field1 = GetField(export.Group.Item.Infrastructure, "caseNumber");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.Group.Item.Infrastructure, "caseUnitNumber");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Group.Item.Infrastructure, "csePersonNumber");

        field3.Color = "cyan";
        field3.Protected = true;
      }
    }
    while(export.Group.Index + 1 != export.Group.Count);
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
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
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ProcessStatus = source.ProcessStatus;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
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
    target.DenormNumeric12 = source.DenormNumeric12;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure4(Infrastructure source,
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
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure5(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EventType = source.EventType;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
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

    local.NextTranInfo.Assign(useExport.NextTranInfo);
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

    useImport.Case1.Number = local.WorkCase.Number;
    useImport.CsePerson.Number = local.WorkCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityValidAuthForFv()
  {
    var useImport = new ScSecurityValidAuthForFv.Import();
    var useExport = new ScSecurityValidAuthForFv.Export();

    useImport.Case1.Number = local.WorkCase.Number;
    useImport.CsePerson.Number = local.WorkCsePerson.Number;

    Call(ScSecurityValidAuthForFv.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.Group.Update.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
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

    MoveInfrastructure1(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure2(useExport.Infrastructure,
      export.Group.Update.Infrastructure);
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
    entities.ExistingEvent.Populated = false;

    return Read("ReadEvent",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", export.Event1.ControlNumber);
      },
      (db, reader) =>
      {
        entities.ExistingEvent.ControlNumber = db.GetInt32(reader, 0);
        entities.ExistingEvent.Name = db.GetString(reader, 1);
        entities.ExistingEvent.Type1 = db.GetString(reader, 2);
        entities.ExistingEvent.Description = db.GetNullableString(reader, 3);
        entities.ExistingEvent.BusinessObjectCode = db.GetString(reader, 4);
        entities.ExistingEvent.Populated = true;
      });
  }

  private bool ReadEventDetail1()
  {
    entities.ExistingEventDetail.Populated = false;

    return Read("ReadEventDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", import.Event1.ControlNumber);
      },
      (db, reader) =>
      {
        entities.ExistingEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingEventDetail.DetailName = db.GetString(reader, 1);
        entities.ExistingEventDetail.InitiatingStateCode =
          db.GetString(reader, 2);
        entities.ExistingEventDetail.CsenetInOutCode = db.GetString(reader, 3);
        entities.ExistingEventDetail.ReasonCode = db.GetString(reader, 4);
        entities.ExistingEventDetail.LogToDiaryInd = db.GetString(reader, 5);
        entities.ExistingEventDetail.EveNo = db.GetInt32(reader, 6);
        entities.ExistingEventDetail.Function = db.GetNullableString(reader, 7);
        entities.ExistingEventDetail.Populated = true;
      });
  }

  private bool ReadEventDetail2()
  {
    entities.ExistingEventDetail.Populated = false;

    return Read("ReadEventDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.ExistingEvent.ControlNumber);
      },
      (db, reader) =>
      {
        entities.ExistingEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingEventDetail.DetailName = db.GetString(reader, 1);
        entities.ExistingEventDetail.InitiatingStateCode =
          db.GetString(reader, 2);
        entities.ExistingEventDetail.CsenetInOutCode = db.GetString(reader, 3);
        entities.ExistingEventDetail.ReasonCode = db.GetString(reader, 4);
        entities.ExistingEventDetail.LogToDiaryInd = db.GetString(reader, 5);
        entities.ExistingEventDetail.EveNo = db.GetInt32(reader, 6);
        entities.ExistingEventDetail.Function = db.GetNullableString(reader, 7);
        entities.ExistingEventDetail.Populated = true;
      });
  }

  private bool ReadEventDetail3()
  {
    entities.ExistingEventDetail.Populated = false;

    return Read("ReadEventDetail3",
      (db, command) =>
      {
        db.SetString(
          command, "reasonCode", entities.ExistingInfrastructure.ReasonCode);
        db.SetInt32(command, "eveNo", entities.ExistingInfrastructure.EventId);
      },
      (db, reader) =>
      {
        entities.ExistingEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingEventDetail.DetailName = db.GetString(reader, 1);
        entities.ExistingEventDetail.InitiatingStateCode =
          db.GetString(reader, 2);
        entities.ExistingEventDetail.CsenetInOutCode = db.GetString(reader, 3);
        entities.ExistingEventDetail.ReasonCode = db.GetString(reader, 4);
        entities.ExistingEventDetail.LogToDiaryInd = db.GetString(reader, 5);
        entities.ExistingEventDetail.EveNo = db.GetInt32(reader, 6);
        entities.ExistingEventDetail.Function = db.GetNullableString(reader, 7);
        entities.ExistingEventDetail.Populated = true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.ExistingTribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.ExistingTribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state",
          entities.ExistingTribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 3);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 4);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFipsFipsTribAddress1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingLegalAction.Populated);
    entities.ExistingFips.Populated = false;
    entities.ExistingFipsTribAddress.Populated = false;

    return Read("ReadFipsFipsTribAddress1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalAction.TrbId.GetValueOrDefault());
        db.SetNullableString(
          command, "countyAbbr", export.HeaderFips.CountyAbbreviation ?? "");
        db.SetString(
          command, "stateAbbreviation", export.HeaderFips.StateAbbreviation);
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 2);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 3);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 4);
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 5);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 6);
        entities.ExistingFipsTribAddress.TrbId = db.GetNullableInt32(reader, 7);
        entities.ExistingFips.Populated = true;
        entities.ExistingFipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsFipsTribAddress2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingLegalAction.Populated);
    entities.ExistingFips.Populated = false;
    entities.ExistingFipsTribAddress.Populated = false;

    return Read("ReadFipsFipsTribAddress2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalAction.TrbId.GetValueOrDefault());
        db.SetNullableString(
          command, "country", export.HeaderFipsTribAddress.Country ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 2);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 3);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 4);
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 5);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 6);
        entities.ExistingFipsTribAddress.TrbId = db.GetNullableInt32(reader, 7);
        entities.ExistingFips.Populated = true;
        entities.ExistingFipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.ExistingFipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 1);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 2);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 3);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 4);
        entities.ExistingFipsTribAddress.TrbId = db.GetNullableInt32(reader, 5);
        entities.ExistingFipsTribAddress.Populated = true;
      });
  }

  private bool ReadInfrastructure1()
  {
    entities.ExistingInfrastructure.Populated = false;

    return Read("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          local.NextTranInfo.InfrastructureId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.SituationNumber =
          db.GetInt32(reader, 1);
        entities.ExistingInfrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.ExistingInfrastructure.EventId = db.GetInt32(reader, 3);
        entities.ExistingInfrastructure.EventType = db.GetString(reader, 4);
        entities.ExistingInfrastructure.EventDetailName =
          db.GetString(reader, 5);
        entities.ExistingInfrastructure.ReasonCode = db.GetString(reader, 6);
        entities.ExistingInfrastructure.BusinessObjectCd =
          db.GetString(reader, 7);
        entities.ExistingInfrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.ExistingInfrastructure.DenormText12 =
          db.GetNullableString(reader, 9);
        entities.ExistingInfrastructure.DenormDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingInfrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingInfrastructure.InitiatingStateCode =
          db.GetString(reader, 12);
        entities.ExistingInfrastructure.CsenetInOutCode =
          db.GetString(reader, 13);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 14);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.ExistingInfrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.ExistingInfrastructure.UserId = db.GetString(reader, 17);
        entities.ExistingInfrastructure.CreatedBy = db.GetString(reader, 18);
        entities.ExistingInfrastructure.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.ExistingInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingInfrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingInfrastructure.ReferenceDate =
          db.GetNullableDate(reader, 22);
        entities.ExistingInfrastructure.Function =
          db.GetNullableString(reader, 23);
        entities.ExistingInfrastructure.Detail =
          db.GetNullableString(reader, 24);
        entities.ExistingInfrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure2()
  {
    entities.ExistingInfrastructure.Populated = false;

    return Read("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.HeaderInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.SituationNumber =
          db.GetInt32(reader, 1);
        entities.ExistingInfrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.ExistingInfrastructure.EventId = db.GetInt32(reader, 3);
        entities.ExistingInfrastructure.EventType = db.GetString(reader, 4);
        entities.ExistingInfrastructure.EventDetailName =
          db.GetString(reader, 5);
        entities.ExistingInfrastructure.ReasonCode = db.GetString(reader, 6);
        entities.ExistingInfrastructure.BusinessObjectCd =
          db.GetString(reader, 7);
        entities.ExistingInfrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.ExistingInfrastructure.DenormText12 =
          db.GetNullableString(reader, 9);
        entities.ExistingInfrastructure.DenormDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingInfrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingInfrastructure.InitiatingStateCode =
          db.GetString(reader, 12);
        entities.ExistingInfrastructure.CsenetInOutCode =
          db.GetString(reader, 13);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 14);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.ExistingInfrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.ExistingInfrastructure.UserId = db.GetString(reader, 17);
        entities.ExistingInfrastructure.CreatedBy = db.GetString(reader, 18);
        entities.ExistingInfrastructure.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.ExistingInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingInfrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingInfrastructure.ReferenceDate =
          db.GetNullableDate(reader, 22);
        entities.ExistingInfrastructure.Function =
          db.GetNullableString(reader, 23);
        entities.ExistingInfrastructure.Detail =
          db.GetNullableString(reader, 24);
        entities.ExistingInfrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure3()
  {
    entities.ExistingInfrastructure.Populated = false;

    return Read("ReadInfrastructure3",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.Group.Item.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.SituationNumber =
          db.GetInt32(reader, 1);
        entities.ExistingInfrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.ExistingInfrastructure.EventId = db.GetInt32(reader, 3);
        entities.ExistingInfrastructure.EventType = db.GetString(reader, 4);
        entities.ExistingInfrastructure.EventDetailName =
          db.GetString(reader, 5);
        entities.ExistingInfrastructure.ReasonCode = db.GetString(reader, 6);
        entities.ExistingInfrastructure.BusinessObjectCd =
          db.GetString(reader, 7);
        entities.ExistingInfrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.ExistingInfrastructure.DenormText12 =
          db.GetNullableString(reader, 9);
        entities.ExistingInfrastructure.DenormDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingInfrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingInfrastructure.InitiatingStateCode =
          db.GetString(reader, 12);
        entities.ExistingInfrastructure.CsenetInOutCode =
          db.GetString(reader, 13);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 14);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.ExistingInfrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.ExistingInfrastructure.UserId = db.GetString(reader, 17);
        entities.ExistingInfrastructure.CreatedBy = db.GetString(reader, 18);
        entities.ExistingInfrastructure.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.ExistingInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingInfrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingInfrastructure.ReferenceDate =
          db.GetNullableDate(reader, 22);
        entities.ExistingInfrastructure.Function =
          db.GetNullableString(reader, 23);
        entities.ExistingInfrastructure.Detail =
          db.GetNullableString(reader, 24);
        entities.ExistingInfrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure4()
  {
    entities.ExistingInfrastructure.Populated = false;

    return Read("ReadInfrastructure4",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.HeaderInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.SituationNumber =
          db.GetInt32(reader, 1);
        entities.ExistingInfrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.ExistingInfrastructure.EventId = db.GetInt32(reader, 3);
        entities.ExistingInfrastructure.EventType = db.GetString(reader, 4);
        entities.ExistingInfrastructure.EventDetailName =
          db.GetString(reader, 5);
        entities.ExistingInfrastructure.ReasonCode = db.GetString(reader, 6);
        entities.ExistingInfrastructure.BusinessObjectCd =
          db.GetString(reader, 7);
        entities.ExistingInfrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.ExistingInfrastructure.DenormText12 =
          db.GetNullableString(reader, 9);
        entities.ExistingInfrastructure.DenormDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingInfrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingInfrastructure.InitiatingStateCode =
          db.GetString(reader, 12);
        entities.ExistingInfrastructure.CsenetInOutCode =
          db.GetString(reader, 13);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 14);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.ExistingInfrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.ExistingInfrastructure.UserId = db.GetString(reader, 17);
        entities.ExistingInfrastructure.CreatedBy = db.GetString(reader, 18);
        entities.ExistingInfrastructure.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.ExistingInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingInfrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingInfrastructure.ReferenceDate =
          db.GetNullableDate(reader, 22);
        entities.ExistingInfrastructure.Function =
          db.GetNullableString(reader, 23);
        entities.ExistingInfrastructure.Detail =
          db.GetNullableString(reader, 24);
        entities.ExistingInfrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure5()
  {
    entities.ExistingInfrastructure.Populated = false;

    return ReadEach("ReadInfrastructure5",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKey.CreatedTimestamp.
            GetValueOrDefault());
        db.SetNullableString(
          command, "caseNumber", export.HeaderInfrastructure.CaseNumber ?? "");
        db.SetNullableString(
          command, "csePersonNumber1", local.StartPerson.CsePersonNumber ?? ""
          );
        db.SetNullableString(
          command, "csePersonNumber2", local.EndPerson.CsePersonNumber ?? "");
        db.SetInt32(command, "count1", local.ConstantZero.Count);
        db.SetInt32(command, "count2", local.ConstantOne.Count);
        db.SetString(command, "eventType1", local.StartType.EventType);
        db.SetString(command, "eventType2", local.EndType.EventType);
        db.SetNullableString(
          command, "function1", local.StartFunction.Function ?? "");
        db.SetNullableString(
          command, "function2", local.EndFunction.Function ?? "");
        db.SetString(
          command, "businessObjectCd1", local.StartBusobj.BusinessObjectCd);
        db.SetString(
          command, "businessObjectCd2", local.EndBusobj.BusinessObjectCd);
        db.SetNullableInt64(
          command, "denormNumeric1",
          local.StartDenormNum.DenormNumeric12.GetValueOrDefault());
        db.SetNullableInt64(
          command, "denormNumeric2",
          local.EndDenormNum.DenormNumeric12.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.SituationNumber =
          db.GetInt32(reader, 1);
        entities.ExistingInfrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.ExistingInfrastructure.EventId = db.GetInt32(reader, 3);
        entities.ExistingInfrastructure.EventType = db.GetString(reader, 4);
        entities.ExistingInfrastructure.EventDetailName =
          db.GetString(reader, 5);
        entities.ExistingInfrastructure.ReasonCode = db.GetString(reader, 6);
        entities.ExistingInfrastructure.BusinessObjectCd =
          db.GetString(reader, 7);
        entities.ExistingInfrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.ExistingInfrastructure.DenormText12 =
          db.GetNullableString(reader, 9);
        entities.ExistingInfrastructure.DenormDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingInfrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingInfrastructure.InitiatingStateCode =
          db.GetString(reader, 12);
        entities.ExistingInfrastructure.CsenetInOutCode =
          db.GetString(reader, 13);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 14);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.ExistingInfrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.ExistingInfrastructure.UserId = db.GetString(reader, 17);
        entities.ExistingInfrastructure.CreatedBy = db.GetString(reader, 18);
        entities.ExistingInfrastructure.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.ExistingInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingInfrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingInfrastructure.ReferenceDate =
          db.GetNullableDate(reader, 22);
        entities.ExistingInfrastructure.Function =
          db.GetNullableString(reader, 23);
        entities.ExistingInfrastructure.Detail =
          db.GetNullableString(reader, 24);
        entities.ExistingInfrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure6()
  {
    entities.ExistingInfrastructure.Populated = false;

    return ReadEach("ReadInfrastructure6",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKey.CreatedTimestamp.
            GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNum",
          export.HeaderInfrastructure.CsePersonNumber ?? "");
        db.SetString(command, "eventType1", local.StartType.EventType);
        db.SetString(command, "eventType2", local.EndType.EventType);
        db.SetNullableString(
          command, "function1", local.StartFunction.Function ?? "");
        db.SetNullableString(
          command, "function2", local.EndFunction.Function ?? "");
        db.SetString(
          command, "businessObjectCd1", local.StartBusobj.BusinessObjectCd);
        db.SetString(
          command, "businessObjectCd2", local.EndBusobj.BusinessObjectCd);
        db.SetNullableInt64(
          command, "denormNumeric1",
          local.StartDenormNum.DenormNumeric12.GetValueOrDefault());
        db.SetNullableInt64(
          command, "denormNumeric2",
          local.EndDenormNum.DenormNumeric12.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.SituationNumber =
          db.GetInt32(reader, 1);
        entities.ExistingInfrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.ExistingInfrastructure.EventId = db.GetInt32(reader, 3);
        entities.ExistingInfrastructure.EventType = db.GetString(reader, 4);
        entities.ExistingInfrastructure.EventDetailName =
          db.GetString(reader, 5);
        entities.ExistingInfrastructure.ReasonCode = db.GetString(reader, 6);
        entities.ExistingInfrastructure.BusinessObjectCd =
          db.GetString(reader, 7);
        entities.ExistingInfrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.ExistingInfrastructure.DenormText12 =
          db.GetNullableString(reader, 9);
        entities.ExistingInfrastructure.DenormDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingInfrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingInfrastructure.InitiatingStateCode =
          db.GetString(reader, 12);
        entities.ExistingInfrastructure.CsenetInOutCode =
          db.GetString(reader, 13);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 14);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.ExistingInfrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.ExistingInfrastructure.UserId = db.GetString(reader, 17);
        entities.ExistingInfrastructure.CreatedBy = db.GetString(reader, 18);
        entities.ExistingInfrastructure.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.ExistingInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingInfrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingInfrastructure.ReferenceDate =
          db.GetNullableDate(reader, 22);
        entities.ExistingInfrastructure.Function =
          db.GetNullableString(reader, 23);
        entities.ExistingInfrastructure.Detail =
          db.GetNullableString(reader, 24);
        entities.ExistingInfrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure7()
  {
    entities.ExistingInfrastructure.Populated = false;

    return ReadEach("ReadInfrastructure7",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKey.CreatedTimestamp.
            GetValueOrDefault());
        db.SetString(command, "eventType1", local.StartType.EventType);
        db.SetString(command, "eventType2", local.EndType.EventType);
        db.SetNullableString(
          command, "denormText12", local.Infrastructure.DenormText12 ?? "");
        db.SetNullableString(
          command, "function1", local.StartFunction.Function ?? "");
        db.SetNullableString(
          command, "function2", local.EndFunction.Function ?? "");
        db.SetNullableInt64(
          command, "denormNumeric1",
          local.StartDenormNum.DenormNumeric12.GetValueOrDefault());
        db.SetNullableInt64(
          command, "denormNumeric2",
          local.EndDenormNum.DenormNumeric12.GetValueOrDefault());
        db.SetNullableInt64(
          command, "denormNumeric3",
          local.Position1.DenormNumeric12.GetValueOrDefault());
        db.SetNullableInt64(
          command, "denormNumeric4",
          local.Position2.DenormNumeric12.GetValueOrDefault());
        db.SetNullableInt64(
          command, "denormNumeric5",
          local.Position3.DenormNumeric12.GetValueOrDefault());
        db.SetNullableInt64(
          command, "denormNumeric6",
          local.Position4.DenormNumeric12.GetValueOrDefault());
        db.SetNullableInt64(
          command, "denormNumeric7",
          local.Position5.DenormNumeric12.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.SituationNumber =
          db.GetInt32(reader, 1);
        entities.ExistingInfrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.ExistingInfrastructure.EventId = db.GetInt32(reader, 3);
        entities.ExistingInfrastructure.EventType = db.GetString(reader, 4);
        entities.ExistingInfrastructure.EventDetailName =
          db.GetString(reader, 5);
        entities.ExistingInfrastructure.ReasonCode = db.GetString(reader, 6);
        entities.ExistingInfrastructure.BusinessObjectCd =
          db.GetString(reader, 7);
        entities.ExistingInfrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.ExistingInfrastructure.DenormText12 =
          db.GetNullableString(reader, 9);
        entities.ExistingInfrastructure.DenormDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingInfrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingInfrastructure.InitiatingStateCode =
          db.GetString(reader, 12);
        entities.ExistingInfrastructure.CsenetInOutCode =
          db.GetString(reader, 13);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 14);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.ExistingInfrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.ExistingInfrastructure.UserId = db.GetString(reader, 17);
        entities.ExistingInfrastructure.CreatedBy = db.GetString(reader, 18);
        entities.ExistingInfrastructure.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.ExistingInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingInfrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingInfrastructure.ReferenceDate =
          db.GetNullableDate(reader, 22);
        entities.ExistingInfrastructure.Function =
          db.GetNullableString(reader, 23);
        entities.ExistingInfrastructure.Detail =
          db.GetNullableString(reader, 24);
        entities.ExistingInfrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure8()
  {
    entities.ExistingInfrastructure.Populated = false;

    return ReadEach("ReadInfrastructure8",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.PageKeys.Item.GexportPageKey.CreatedTimestamp.
            GetValueOrDefault());
        db.SetString(command, "eventType1", local.StartType.EventType);
        db.SetString(command, "eventType2", local.EndType.EventType);
        db.SetNullableString(
          command, "function1", local.StartFunction.Function ?? "");
        db.SetNullableString(
          command, "function2", local.EndFunction.Function ?? "");
        db.SetString(
          command, "businessObjectCd1", local.StartBusobj.BusinessObjectCd);
        db.SetString(
          command, "businessObjectCd2", local.EndBusobj.BusinessObjectCd);
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.SituationNumber =
          db.GetInt32(reader, 1);
        entities.ExistingInfrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.ExistingInfrastructure.EventId = db.GetInt32(reader, 3);
        entities.ExistingInfrastructure.EventType = db.GetString(reader, 4);
        entities.ExistingInfrastructure.EventDetailName =
          db.GetString(reader, 5);
        entities.ExistingInfrastructure.ReasonCode = db.GetString(reader, 6);
        entities.ExistingInfrastructure.BusinessObjectCd =
          db.GetString(reader, 7);
        entities.ExistingInfrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.ExistingInfrastructure.DenormText12 =
          db.GetNullableString(reader, 9);
        entities.ExistingInfrastructure.DenormDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingInfrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingInfrastructure.InitiatingStateCode =
          db.GetString(reader, 12);
        entities.ExistingInfrastructure.CsenetInOutCode =
          db.GetString(reader, 13);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 14);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.ExistingInfrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.ExistingInfrastructure.UserId = db.GetString(reader, 17);
        entities.ExistingInfrastructure.CreatedBy = db.GetString(reader, 18);
        entities.ExistingInfrastructure.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.ExistingInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingInfrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.ExistingInfrastructure.ReferenceDate =
          db.GetNullableDate(reader, 22);
        entities.ExistingInfrastructure.Function =
          db.GetNullableString(reader, 23);
        entities.ExistingInfrastructure.Detail =
          db.GetNullableString(reader, 24);
        entities.ExistingInfrastructure.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", local.PerformanceLegalAction.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", local.PerformanceLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction4()
  {
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalAction4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
        db.SetNullableString(
          command, "countyAbbr", export.HeaderFips.CountyAbbreviation ?? "");
        db.SetString(
          command, "stateAbbreviation", export.HeaderFips.StateAbbreviation);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction5()
  {
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalAction5",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.HeaderLegalAction.CourtCaseNumber ?? ""
          );
        db.SetNullableString(
          command, "country", export.HeaderFipsTribAddress.Country ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadMonitoredDocument1()
  {
    entities.ExistingMonitoredDocument.Populated = false;

    return Read("ReadMonitoredDocument1",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId",
          export.SelectedInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingMonitoredDocument.RequiredResponseDate =
          db.GetDate(reader, 0);
        entities.ExistingMonitoredDocument.ClosureReasonCode =
          db.GetNullableString(reader, 1);
        entities.ExistingMonitoredDocument.InfId = db.GetInt32(reader, 2);
        entities.ExistingMonitoredDocument.Populated = true;
      });
  }

  private bool ReadMonitoredDocument2()
  {
    entities.ExistingMonitoredDocument.Populated = false;

    return Read("ReadMonitoredDocument2",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingMonitoredDocument.RequiredResponseDate =
          db.GetDate(reader, 0);
        entities.ExistingMonitoredDocument.ClosureReasonCode =
          db.GetNullableString(reader, 1);
        entities.ExistingMonitoredDocument.InfId = db.GetInt32(reader, 2);
        entities.ExistingMonitoredDocument.Populated = true;
      });
  }

  private bool ReadNarrativeDetail()
  {
    entities.ExistingNarrativeDetail.Populated = false;

    return Read("ReadNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          export.Group.Item.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingNarrativeDetail.InfrastructureId =
          db.GetInt32(reader, 0);
        entities.ExistingNarrativeDetail.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ExistingNarrativeDetail.LineNumber = db.GetInt32(reader, 2);
        entities.ExistingNarrativeDetail.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingLegalAction.Populated);
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 1);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.ExistingTribunal.Populated = true;
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
      /// A value of GimportPageKey.
      /// </summary>
      [JsonPropertyName("gimportPageKey")]
      public Infrastructure GimportPageKey
      {
        get => gimportPageKey ??= new();
        set => gimportPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 250;

      private Infrastructure gimportPageKey;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Narrative.
      /// </summary>
      [JsonPropertyName("narrative")]
      public TextWorkArea Narrative
      {
        get => narrative ??= new();
        set => narrative = value;
      }

      /// <summary>
      /// A value of CaseUnitFunctionAssignmt.
      /// </summary>
      [JsonPropertyName("caseUnitFunctionAssignmt")]
      public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
      {
        get => caseUnitFunctionAssignmt ??= new();
        set => caseUnitFunctionAssignmt = value;
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
      /// A value of Event1.
      /// </summary>
      [JsonPropertyName("event1")]
      public Event1 Event1
      {
        get => event1 ??= new();
        set => event1 = value;
      }

      /// <summary>
      /// A value of MonitoredDocument.
      /// </summary>
      [JsonPropertyName("monitoredDocument")]
      public MonitoredDocument MonitoredDocument
      {
        get => monitoredDocument ??= new();
        set => monitoredDocument = value;
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private TextWorkArea narrative;
      private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
      private Common common;
      private Event1 event1;
      private MonitoredDocument monitoredDocument;
      private SpTextWorkArea spTextWorkArea;
      private LegalAction legalAction;
      private Infrastructure infrastructure;
      private CsePersonsWorkSet csePersonsWorkSet;
    }

    /// <summary>
    /// A value of HiddenImportHeaderFipsTribAddress.
    /// </summary>
    [JsonPropertyName("hiddenImportHeaderFipsTribAddress")]
    public FipsTribAddress HiddenImportHeaderFipsTribAddress
    {
      get => hiddenImportHeaderFipsTribAddress ??= new();
      set => hiddenImportHeaderFipsTribAddress = value;
    }

    /// <summary>
    /// A value of HiddenImportHeaderStartDate.
    /// </summary>
    [JsonPropertyName("hiddenImportHeaderStartDate")]
    public DateWorkArea HiddenImportHeaderStartDate
    {
      get => hiddenImportHeaderStartDate ??= new();
      set => hiddenImportHeaderStartDate = value;
    }

    /// <summary>
    /// A value of HiddenImportHeaderFips.
    /// </summary>
    [JsonPropertyName("hiddenImportHeaderFips")]
    public Fips HiddenImportHeaderFips
    {
      get => hiddenImportHeaderFips ??= new();
      set => hiddenImportHeaderFips = value;
    }

    /// <summary>
    /// A value of HiddenImportHeaderLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenImportHeaderLegalAction")]
    public LegalAction HiddenImportHeaderLegalAction
    {
      get => hiddenImportHeaderLegalAction ??= new();
      set => hiddenImportHeaderLegalAction = value;
    }

    /// <summary>
    /// A value of HiddenImportHeaderInfrastructure.
    /// </summary>
    [JsonPropertyName("hiddenImportHeaderInfrastructure")]
    public Infrastructure HiddenImportHeaderInfrastructure
    {
      get => hiddenImportHeaderInfrastructure ??= new();
      set => hiddenImportHeaderInfrastructure = value;
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
    /// A value of HeaderPromptType.
    /// </summary>
    [JsonPropertyName("headerPromptType")]
    public Standard HeaderPromptType
    {
      get => headerPromptType ??= new();
      set => headerPromptType = value;
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
    /// A value of HeaderStartDate.
    /// </summary>
    [JsonPropertyName("headerStartDate")]
    public DateWorkArea HeaderStartDate
    {
      get => headerStartDate ??= new();
      set => headerStartDate = value;
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
    /// A value of HeaderLegalAction.
    /// </summary>
    [JsonPropertyName("headerLegalAction")]
    public LegalAction HeaderLegalAction
    {
      get => headerLegalAction ??= new();
      set => headerLegalAction = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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

    private FipsTribAddress hiddenImportHeaderFipsTribAddress;
    private DateWorkArea hiddenImportHeaderStartDate;
    private Fips hiddenImportHeaderFips;
    private LegalAction hiddenImportHeaderLegalAction;
    private Infrastructure hiddenImportHeaderInfrastructure;
    private Standard scroll;
    private Array<PageKeysGroup> pageKeys;
    private Common currentPage;
    private Code hiddenCode;
    private CodeValue hiddenCodeValue;
    private EventDetail eventDetail;
    private Event1 event1;
    private Standard headerPromptType;
    private FipsTribAddress headerFipsTribAddress;
    private DateWorkArea headerStartDate;
    private Fips headerFips;
    private LegalAction headerLegalAction;
    private Infrastructure headerInfrastructure;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Array<GroupGroup> group;
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
      /// A value of GexportPageKey.
      /// </summary>
      [JsonPropertyName("gexportPageKey")]
      public Infrastructure GexportPageKey
      {
        get => gexportPageKey ??= new();
        set => gexportPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 250;

      private Infrastructure gexportPageKey;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Narrative.
      /// </summary>
      [JsonPropertyName("narrative")]
      public TextWorkArea Narrative
      {
        get => narrative ??= new();
        set => narrative = value;
      }

      /// <summary>
      /// A value of CaseUnitFunctionAssignmt.
      /// </summary>
      [JsonPropertyName("caseUnitFunctionAssignmt")]
      public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
      {
        get => caseUnitFunctionAssignmt ??= new();
        set => caseUnitFunctionAssignmt = value;
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
      /// A value of Event1.
      /// </summary>
      [JsonPropertyName("event1")]
      public Event1 Event1
      {
        get => event1 ??= new();
        set => event1 = value;
      }

      /// <summary>
      /// A value of MonitoredDocument.
      /// </summary>
      [JsonPropertyName("monitoredDocument")]
      public MonitoredDocument MonitoredDocument
      {
        get => monitoredDocument ??= new();
        set => monitoredDocument = value;
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private TextWorkArea narrative;
      private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
      private Common common;
      private Event1 event1;
      private MonitoredDocument monitoredDocument;
      private SpTextWorkArea spTextWorkArea;
      private LegalAction legalAction;
      private Infrastructure infrastructure;
      private CsePersonsWorkSet csePersonsWorkSet;
    }

    /// <summary>
    /// A value of HiddenExportHeaderFipsTribAddress.
    /// </summary>
    [JsonPropertyName("hiddenExportHeaderFipsTribAddress")]
    public FipsTribAddress HiddenExportHeaderFipsTribAddress
    {
      get => hiddenExportHeaderFipsTribAddress ??= new();
      set => hiddenExportHeaderFipsTribAddress = value;
    }

    /// <summary>
    /// A value of HiddenExportHeaderStartDate.
    /// </summary>
    [JsonPropertyName("hiddenExportHeaderStartDate")]
    public DateWorkArea HiddenExportHeaderStartDate
    {
      get => hiddenExportHeaderStartDate ??= new();
      set => hiddenExportHeaderStartDate = value;
    }

    /// <summary>
    /// A value of HiddenExportHeaderFips.
    /// </summary>
    [JsonPropertyName("hiddenExportHeaderFips")]
    public Fips HiddenExportHeaderFips
    {
      get => hiddenExportHeaderFips ??= new();
      set => hiddenExportHeaderFips = value;
    }

    /// <summary>
    /// A value of HiddenExportHeaderLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenExportHeaderLegalAction")]
    public LegalAction HiddenExportHeaderLegalAction
    {
      get => hiddenExportHeaderLegalAction ??= new();
      set => hiddenExportHeaderLegalAction = value;
    }

    /// <summary>
    /// A value of HiddenExportHeaderInfrastructure.
    /// </summary>
    [JsonPropertyName("hiddenExportHeaderInfrastructure")]
    public Infrastructure HiddenExportHeaderInfrastructure
    {
      get => hiddenExportHeaderInfrastructure ??= new();
      set => hiddenExportHeaderInfrastructure = value;
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
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
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
    /// A value of SelectedInfrastructure.
    /// </summary>
    [JsonPropertyName("selectedInfrastructure")]
    public Infrastructure SelectedInfrastructure
    {
      get => selectedInfrastructure ??= new();
      set => selectedInfrastructure = value;
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
    /// A value of HeaderPromptType.
    /// </summary>
    [JsonPropertyName("headerPromptType")]
    public Standard HeaderPromptType
    {
      get => headerPromptType ??= new();
      set => headerPromptType = value;
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
    /// A value of HeaderStartDate.
    /// </summary>
    [JsonPropertyName("headerStartDate")]
    public DateWorkArea HeaderStartDate
    {
      get => headerStartDate ??= new();
      set => headerStartDate = value;
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
    /// A value of HeaderLegalAction.
    /// </summary>
    [JsonPropertyName("headerLegalAction")]
    public LegalAction HeaderLegalAction
    {
      get => headerLegalAction ??= new();
      set => headerLegalAction = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private FipsTribAddress hiddenExportHeaderFipsTribAddress;
    private DateWorkArea hiddenExportHeaderStartDate;
    private Fips hiddenExportHeaderFips;
    private LegalAction hiddenExportHeaderLegalAction;
    private Infrastructure hiddenExportHeaderInfrastructure;
    private Standard scroll;
    private Array<PageKeysGroup> pageKeys;
    private Common currentPage;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private LegalAction selectedLegalAction;
    private Infrastructure selectedInfrastructure;
    private Code hiddenCode;
    private CodeValue hiddenCodeValue;
    private EventDetail eventDetail;
    private Event1 event1;
    private Standard headerPromptType;
    private FipsTribAddress headerFipsTribAddress;
    private DateWorkArea headerStartDate;
    private Fips headerFips;
    private LegalAction headerLegalAction;
    private Infrastructure headerInfrastructure;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Array<GroupGroup> group;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ConstantOne.
    /// </summary>
    [JsonPropertyName("constantOne")]
    public Common ConstantOne
    {
      get => constantOne ??= new();
      set => constantOne = value;
    }

    /// <summary>
    /// A value of ConstantZero.
    /// </summary>
    [JsonPropertyName("constantZero")]
    public Common ConstantZero
    {
      get => constantZero ??= new();
      set => constantZero = value;
    }

    /// <summary>
    /// A value of Position1.
    /// </summary>
    [JsonPropertyName("position1")]
    public Infrastructure Position1
    {
      get => position1 ??= new();
      set => position1 = value;
    }

    /// <summary>
    /// A value of Position2.
    /// </summary>
    [JsonPropertyName("position2")]
    public Infrastructure Position2
    {
      get => position2 ??= new();
      set => position2 = value;
    }

    /// <summary>
    /// A value of Position3.
    /// </summary>
    [JsonPropertyName("position3")]
    public Infrastructure Position3
    {
      get => position3 ??= new();
      set => position3 = value;
    }

    /// <summary>
    /// A value of Position4.
    /// </summary>
    [JsonPropertyName("position4")]
    public Infrastructure Position4
    {
      get => position4 ??= new();
      set => position4 = value;
    }

    /// <summary>
    /// A value of Position5.
    /// </summary>
    [JsonPropertyName("position5")]
    public Infrastructure Position5
    {
      get => position5 ??= new();
      set => position5 = value;
    }

    /// <summary>
    /// A value of Reposition.
    /// </summary>
    [JsonPropertyName("reposition")]
    public Infrastructure Reposition
    {
      get => reposition ??= new();
      set => reposition = value;
    }

    /// <summary>
    /// A value of EndDenormNum.
    /// </summary>
    [JsonPropertyName("endDenormNum")]
    public Infrastructure EndDenormNum
    {
      get => endDenormNum ??= new();
      set => endDenormNum = value;
    }

    /// <summary>
    /// A value of StartDenormNum.
    /// </summary>
    [JsonPropertyName("startDenormNum")]
    public Infrastructure StartDenormNum
    {
      get => startDenormNum ??= new();
      set => startDenormNum = value;
    }

    /// <summary>
    /// A value of FindHistCount2.
    /// </summary>
    [JsonPropertyName("findHistCount2")]
    public Common FindHistCount2
    {
      get => findHistCount2 ??= new();
      set => findHistCount2 = value;
    }

    /// <summary>
    /// A value of FindHistCount1.
    /// </summary>
    [JsonPropertyName("findHistCount1")]
    public Common FindHistCount1
    {
      get => findHistCount1 ??= new();
      set => findHistCount1 = value;
    }

    /// <summary>
    /// A value of ReturnFromCodeValue.
    /// </summary>
    [JsonPropertyName("returnFromCodeValue")]
    public Common ReturnFromCodeValue
    {
      get => returnFromCodeValue ??= new();
      set => returnFromCodeValue = value;
    }

    /// <summary>
    /// A value of StartBusobj.
    /// </summary>
    [JsonPropertyName("startBusobj")]
    public Infrastructure StartBusobj
    {
      get => startBusobj ??= new();
      set => startBusobj = value;
    }

    /// <summary>
    /// A value of EndBusobj.
    /// </summary>
    [JsonPropertyName("endBusobj")]
    public Infrastructure EndBusobj
    {
      get => endBusobj ??= new();
      set => endBusobj = value;
    }

    /// <summary>
    /// A value of StartFunction.
    /// </summary>
    [JsonPropertyName("startFunction")]
    public Infrastructure StartFunction
    {
      get => startFunction ??= new();
      set => startFunction = value;
    }

    /// <summary>
    /// A value of EndFunction.
    /// </summary>
    [JsonPropertyName("endFunction")]
    public Infrastructure EndFunction
    {
      get => endFunction ??= new();
      set => endFunction = value;
    }

    /// <summary>
    /// A value of StartType.
    /// </summary>
    [JsonPropertyName("startType")]
    public Infrastructure StartType
    {
      get => startType ??= new();
      set => startType = value;
    }

    /// <summary>
    /// A value of EndType.
    /// </summary>
    [JsonPropertyName("endType")]
    public Infrastructure EndType
    {
      get => endType ??= new();
      set => endType = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public LegalAction Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public LegalAction End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of StartPerson.
    /// </summary>
    [JsonPropertyName("startPerson")]
    public Infrastructure StartPerson
    {
      get => startPerson ??= new();
      set => startPerson = value;
    }

    /// <summary>
    /// A value of EndPerson.
    /// </summary>
    [JsonPropertyName("endPerson")]
    public Infrastructure EndPerson
    {
      get => endPerson ??= new();
      set => endPerson = value;
    }

    /// <summary>
    /// A value of EndCase.
    /// </summary>
    [JsonPropertyName("endCase")]
    public Infrastructure EndCase
    {
      get => endCase ??= new();
      set => endCase = value;
    }

    /// <summary>
    /// A value of StartCase.
    /// </summary>
    [JsonPropertyName("startCase")]
    public Infrastructure StartCase
    {
      get => startCase ??= new();
      set => startCase = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePersonsWorkSet Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of PerformanceInfrastructure.
    /// </summary>
    [JsonPropertyName("performanceInfrastructure")]
    public Infrastructure PerformanceInfrastructure
    {
      get => performanceInfrastructure ??= new();
      set => performanceInfrastructure = value;
    }

    /// <summary>
    /// A value of Keep.
    /// </summary>
    [JsonPropertyName("keep")]
    public Common Keep
    {
      get => keep ??= new();
      set => keep = value;
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
    /// A value of PerformanceLegalAction.
    /// </summary>
    [JsonPropertyName("performanceLegalAction")]
    public LegalAction PerformanceLegalAction
    {
      get => performanceLegalAction ??= new();
      set => performanceLegalAction = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Initialised.
    /// </summary>
    [JsonPropertyName("initialised")]
    public LegalAction Initialised
    {
      get => initialised ??= new();
      set => initialised = value;
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
    /// A value of WorkCase.
    /// </summary>
    [JsonPropertyName("workCase")]
    public Case1 WorkCase
    {
      get => workCase ??= new();
      set => workCase = value;
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
    /// A value of NullTimestamp.
    /// </summary>
    [JsonPropertyName("nullTimestamp")]
    public DateWorkArea NullTimestamp
    {
      get => nullTimestamp ??= new();
      set => nullTimestamp = value;
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
    /// A value of SpTextWorkArea.
    /// </summary>
    [JsonPropertyName("spTextWorkArea")]
    public SpTextWorkArea SpTextWorkArea
    {
      get => spTextWorkArea ??= new();
      set => spTextWorkArea = value;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of SelectionMade.
    /// </summary>
    [JsonPropertyName("selectionMade")]
    public Common SelectionMade
    {
      get => selectionMade ??= new();
      set => selectionMade = value;
    }

    private Common constantOne;
    private Common constantZero;
    private Infrastructure position1;
    private Infrastructure position2;
    private Infrastructure position3;
    private Infrastructure position4;
    private Infrastructure position5;
    private Infrastructure reposition;
    private Infrastructure endDenormNum;
    private Infrastructure startDenormNum;
    private Common findHistCount2;
    private Common findHistCount1;
    private Common returnFromCodeValue;
    private Infrastructure startBusobj;
    private Infrastructure endBusobj;
    private Infrastructure startFunction;
    private Infrastructure endFunction;
    private Infrastructure startType;
    private Infrastructure endType;
    private LegalAction start;
    private LegalAction end;
    private Infrastructure startPerson;
    private Infrastructure endPerson;
    private Infrastructure endCase;
    private Infrastructure startCase;
    private Common pointer;
    private CsePersonsWorkSet prev;
    private Infrastructure performanceInfrastructure;
    private Common keep;
    private DateWorkArea current;
    private LegalAction performanceLegalAction;
    private Common clearGroupView;
    private Common count;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Infrastructure infrastructure;
    private LegalAction initialised;
    private LegalAction legalAction;
    private CodeValue codeValue;
    private Code code;
    private Case1 workCase;
    private CsePerson workCsePerson;
    private DateWorkArea nullTimestamp;
    private EventDetail eventDetail;
    private Event1 event1;
    private SpTextWorkArea spTextWorkArea;
    private TextWorkArea textWorkArea;
    private NextTranInfo nextTranInfo;
    private Common validCode;
    private DateWorkArea initialized;
    private Common selectionMade;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingNarrativeDetail.
    /// </summary>
    [JsonPropertyName("existingNarrativeDetail")]
    public NarrativeDetail ExistingNarrativeDetail
    {
      get => existingNarrativeDetail ??= new();
      set => existingNarrativeDetail = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of KeysOnly.
    /// </summary>
    [JsonPropertyName("keysOnly")]
    public Infrastructure KeysOnly
    {
      get => keysOnly ??= new();
      set => keysOnly = value;
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
    /// A value of ExistingEventDetail.
    /// </summary>
    [JsonPropertyName("existingEventDetail")]
    public EventDetail ExistingEventDetail
    {
      get => existingEventDetail ??= new();
      set => existingEventDetail = value;
    }

    /// <summary>
    /// A value of ExistingEvent.
    /// </summary>
    [JsonPropertyName("existingEvent")]
    public Event1 ExistingEvent
    {
      get => existingEvent ??= new();
      set => existingEvent = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingFipsTribAddress.
    /// </summary>
    [JsonPropertyName("existingFipsTribAddress")]
    public FipsTribAddress ExistingFipsTribAddress
    {
      get => existingFipsTribAddress ??= new();
      set => existingFipsTribAddress = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of ExistingMonitoredDocument.
    /// </summary>
    [JsonPropertyName("existingMonitoredDocument")]
    public MonitoredDocument ExistingMonitoredDocument
    {
      get => existingMonitoredDocument ??= new();
      set => existingMonitoredDocument = value;
    }

    /// <summary>
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
    }

    private NarrativeDetail existingNarrativeDetail;
    private OutgoingDocument outgoingDocument;
    private Infrastructure keysOnly;
    private Case1 case1;
    private EventDetail existingEventDetail;
    private Event1 existingEvent;
    private Fips existingFips;
    private LegalAction existingLegalAction;
    private FipsTribAddress existingFipsTribAddress;
    private Tribunal existingTribunal;
    private MonitoredDocument existingMonitoredDocument;
    private Infrastructure existingInfrastructure;
  }
#endregion
}
