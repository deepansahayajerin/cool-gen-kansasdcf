// Program: SP_CREN_CASE_REVIEW_ENFORCEMENT, ID: 372648434, model: 746.
// Short name: SWECRENP
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
/// A program: SP_CREN_CASE_REVIEW_ENFORCEMENT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCrenCaseReviewEnforcement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREN_CASE_REVIEW_ENFORCEMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCrenCaseReviewEnforcement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCrenCaseReviewEnforcement.
  /// </summary>
  public SpCrenCaseReviewEnforcement(IContext context, Import import,
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
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    // Date		Developer	req #		Description
    // 03/14/96	Alan Hackler           		Retro fits
    // 01/03/97	R. Marchman			Add new security/next tran.
    // 01/10/97	R. Welborn			Conversion from Plan Task Note to Infrastructure/
    // Narrative.
    // 04/30/97	R. Grey				Change Current Date
    // 05/  /97	R. Grey				IDCR #327 Dialog flow PAYR and CASU
    // 07/04/97	R. Grey				Add LE RFRL Rsn Codes 2 thru 4
    // 07/22/97	R. Grey				Add review closed case
    // 08/10/97	R. Grey				Debug financial and return from CREO.
    // 04/04/99	N.Engoor			Added new fields on the screen. Listing all court 
    // case numbers for that AP.
    // 						Added code for calculating summary totals. Removed unwanted READ 
    // stmnts
    // 						Changed the way in which the notes were being displayed.
    // 12/28/99	N.Engoor               		Removed the use of a redundant CAB.
    // 02/03/00	Vithal Madhira	PR# 86247	Modified code to implement the case 
    // review for each AP on a multiple AP case.
    // 08/08/00	SWSRCHF		WR# 000170	Replace the read for Narrative by a read for
    // Narrative Detail
    // 09/21/00	SWDPARM  	WR#         	Add fields to CREN to show mdfn request 
    // and mdfn denial dates
    // 03/09/01	Madhu Kumar	PR# 115223   	Ages of children who are end-dated was
    // also shown.
    // 02/25/02	M Ramirez	PR139864	Added mod order ind, which user is to fill 
    // out before proceeding to the
    // 						completion of the review.  A Y means the user is planning on making
    // a
    // 						change to the order based on the review.  An N means there will
    // 						will no changes based on the review.
    // 03/03/10	J Huss		CQ# 365		Updated READ EACH prior to call to 
    // fn_calc_amts_due_for_obligation
    // 						to correctly choose only obligations that were owed by the current 
    // AP.
    // 						Change occured in two places.  Temporary fix.
    // 03/03/10	J Huss		CQ# 9346	Reads for FDSO, SDSO, and CRED certifications 
    // were returning incorrect
    // 						results due to the reads not accounting for all criteria.  Re-wrote
    // reads
    // 						for all three indicators.  Temporary fix.
    // 03/30/10	J Huss		CQ# 365/9346	Overhauled screen.
    // 08/13/10	J Huss		CQ# 16256	Corrected AP number passed when flowing from 
    // CRES.
    // 04/01/2011	T Pierce	CQ# 23212	Removed references to obsolete Narrative 
    // entity type.
    // ---------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Min.Date = new DateTime(1, 1, 1);
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_XFR_TO_MENU";

      return;
    }

    export.LastIncomeIncrease.Date = import.LastIncomeIncrease.Date;
    export.ApKnownToOtherCases.Flag = import.ApKnownToOtherCases.Flag;
    export.ArTotalMonthly.TotalCurrency = import.ArTotalMonthly.TotalCurrency;
    export.ApTotalMonthly.TotalCurrency = import.ApTotalMonthly.TotalCurrency;
    export.MostRecent.ReferralDate = import.MostRecent.ReferralDate;
    export.GoodCause.Code = import.GoodCause.Code;
    export.Bankruptcy.BankruptcyFilingDate =
      import.Bankruptcy.BankruptcyFilingDate;
    export.NoOfChildren.Count = import.NoOfChildren.Count;
    export.Hidden.Assign(import.Hidden);
    export.HiddenFlowToWork.Flag = import.HiddenFlowToWork.Flag;
    export.CaseClosedIndicator.Flag = import.CaseClosedIndicator.Flag;
    export.CountNoOfApOnCase.Count = import.CountNoOfApOnCase.Count;
    export.CountAllCauReads.Count = import.CountAllCauReads.Count;
    export.PreviousCommand.Command = import.PreviousCommand.Command;
    export.HiddenPassInfra.SystemGeneratedIdentifier =
      import.HiddenPass1.SystemGeneratedIdentifier;
    export.HiddenPassedReviewType.ActionEntry =
      import.HiddenPassedReviewType.ActionEntry;
    export.EnfReview.Text = import.EnfReview.Text;
    export.Client.UnemploymentInd = import.Client.UnemploymentInd;

    for(import.HiddenPass.Index = 0; import.HiddenPass.Index < import
      .HiddenPass.Count; ++import.HiddenPass.Index)
    {
      if (!import.HiddenPass.CheckSize())
      {
        break;
      }

      export.HidnPassNarrative.Index = import.HiddenPass.Index;
      export.HidnPassNarrative.CheckSize();

      export.HidnPassNarrative.Update.GexportH.Text =
        import.HiddenPass.Item.GimportH.Text;
      export.HidnPassNarrative.Update.GexportHiddenPassedFlag.Flag =
        import.HiddenPass.Item.GimportHiddenPassedFlag.Flag;
    }

    import.HiddenPass.CheckIndex();
    export.HiddenFlowToWork.Flag = import.HiddenFlowToWork.Flag;
    export.Case1.Number = import.Case1.Number;
    export.CaseInReview.Number = import.Case1.Number;
    export.MoreApsMsg.Text30 = import.MoreApsMsg.Text30;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.MultiAp.Flag = import.MultiAp.Flag;
    export.ApSelected.Flag = import.ApSelected.Flag;
    MoveCsePersonsWorkSet2(import.SelectedAp, export.SelectedAp);
    MoveCsePersonsWorkSet2(import.Ap, export.PassSelectedAp);
    export.SelectedApToDebt.Number = import.Ap.Number;

    if (!Equal(global.Command, "DISPLAY"))
    {
      if (Equal(global.Command, "RETLINK"))
      {
        export.TotalAdcAp.TotalCurrency = 0;
        export.TotalNonAdcAp.TotalCurrency = 0;
        export.CountNoOfApOnCase.Count = 0;
        export.CountAllCauReads.Count = 0;
        export.MoreApsMsg.Text30 = "";
        export.PreviousCommand.Command = "RETLINK";
        global.Command = "DISPLAY";

        goto Test1;
      }

      export.ApDenialLtr.Date = import.ArDenialLtr.Date;
      export.ApDenialLtr.Date = import.ApDenialLtr.Date;
      export.ModificationRequest.Date = import.ModificationRequest.Date;
      export.LastReviewDate.Date = import.LastReviewDate.Date;
      export.Ar.Assign(import.Ar);
      MoveCsePersonsWorkSet1(import.Ap, export.Ap);
      export.CreditReportingAp.Flag = import.CreditReportingAp.Flag;
      export.FdsoAp.Flag = import.FdsoAp.Flag;
      export.SdsoAp.Flag = import.SdsoAp.Flag;
      export.SelectAp.SelectChar = import.SelectAp.SelectChar;
      export.TotalAdcAp.TotalCurrency = import.TotalAdcAp.TotalCurrency;
      export.TotalNonAdcAp.TotalCurrency = import.TotalNonAdcAp.TotalCurrency;
      export.EnfReview.Text = import.EnfReview.Text;

      for(import.Ages.Index = 0; import.Ages.Index < import.Ages.Count; ++
        import.Ages.Index)
      {
        if (!import.Ages.CheckSize())
        {
          break;
        }

        export.Ages.Index = import.Ages.Index;
        export.Ages.CheckSize();

        export.Ages.Update.Age.Text4 = import.Ages.Item.Age.Text4;
      }

      import.Ages.CheckIndex();

      for(import.HiddenGroupCourtCase.Index = 0; import
        .HiddenGroupCourtCase.Index < import.HiddenGroupCourtCase.Count; ++
        import.HiddenGroupCourtCase.Index)
      {
        if (!import.HiddenGroupCourtCase.CheckSize())
        {
          break;
        }

        export.HiddenGroupCourtCase.Index = import.HiddenGroupCourtCase.Index;
        export.HiddenGroupCourtCase.CheckSize();

        export.HiddenGroupCourtCase.Update.HiddenCommon.SelectChar =
          import.HiddenGroupCourtCase.Item.HiddenCommon.SelectChar;
        MoveLegalAction(import.HiddenGroupCourtCase.Item.HiddenLegalAction,
          export.HiddenGroupCourtCase.Update.HiddenLegalAction);
        export.HiddenGroupCourtCase.Update.HiddenGrpTotalPayoff.TotalCurrency =
          import.HiddenGroupCourtCase.Item.HiddenGrpTotalPayoff.TotalCurrency;
        export.HiddenGroupCourtCase.Update.HiddenGrpMwoDate.Date =
          import.HiddenGroupCourtCase.Item.HiddenGrpMwoDate.Date;
        export.HiddenGroupCourtCase.Update.HiddenGrpIwoDate.Date =
          import.HiddenGroupCourtCase.Item.HiddenGrpIwoDate.Date;
        export.HiddenGroupCourtCase.Update.HiddenGrpLastPayment.Date =
          import.HiddenGroupCourtCase.Item.HiddenGrpLastPayment.Date;
        export.HiddenGroupCourtCase.Update.HiddenGrpMonthlyDue.TotalCurrency =
          import.HiddenGroupCourtCase.Item.HiddenGrpMonthlyDue.TotalCurrency;
      }

      import.HiddenGroupCourtCase.CheckIndex();

      // If the command is next or prev then the group will be redisplayed.  No 
      // need to do it now.
      if (!Equal(global.Command, "NEXT") && !Equal(global.Command, "PREV"))
      {
        for(import.CourtCase.Index = 0; import.CourtCase.Index < import
          .CourtCase.Count; ++import.CourtCase.Index)
        {
          if (!import.CourtCase.CheckSize())
          {
            break;
          }

          export.CourtCase.Index = import.CourtCase.Index;
          export.CourtCase.CheckSize();

          export.CourtCase.Update.Common.SelectChar =
            import.CourtCase.Item.Common.SelectChar;
          MoveLegalAction(import.CourtCase.Item.LegalAction,
            export.CourtCase.Update.LegalAction);
          export.CourtCase.Update.TotalPayoff.TotalCurrency =
            import.CourtCase.Item.TotalPayoff.TotalCurrency;
          export.CourtCase.Update.MwoDate.Date =
            import.CourtCase.Item.MwoDate.Date;
          export.CourtCase.Update.IwoDate.Date =
            import.CourtCase.Item.IwoDate.Date;
          export.CourtCase.Update.LastPayment.Date =
            import.CourtCase.Item.LastPayment.Date;
          export.CourtCase.Update.MonthlyDue.TotalCurrency =
            import.CourtCase.Item.MonthlyDue.TotalCurrency;
        }

        import.CourtCase.CheckIndex();
      }

      export.HiddenCrtCaseSubscript.Count = import.HiddenCrtCaseSubscript.Count;
      export.ScrollMsg.ScrollingMessage = import.ScrollMsg.ScrollingMessage;

      // mjr
      // -------------------------------------------
      // 02/25/2002
      // PR139864 - Added mod order ind
      // If the user does a DISPLAY the indicator should be reset,
      // so only set it if the Command <> DISPLAY
      // ---------------------------------------------------------
      export.ModOrderInd.Flag = import.ModOrderInd.Flag;
    }

Test1:

    // If the review type is read only, protect the Review Note field.
    if (Equal(export.HiddenPassedReviewType.ActionEntry, "R"))
    {
      var field = GetField(export.EnfReview, "text");

      field.Color = "cyan";
      field.Highlighting = Highlighting.Normal;
      field.Protected = true;
      field.Focused = false;
    }
    else
    {
      var field = GetField(export.EnfReview, "text");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
      field.Focused = true;
    }

    // Check to see if the user has selected multiple entries or has entered 
    // invalid selection characters
    if (!export.CourtCase.IsEmpty)
    {
      for(export.CourtCase.Index = 0; export.CourtCase.Index < export
        .CourtCase.Count; ++export.CourtCase.Index)
      {
        if (!export.CourtCase.CheckSize())
        {
          break;
        }

        if (AsChar(export.CourtCase.Item.Common.SelectChar) == 'S')
        {
          ++local.SelCount.Count;
        }
        else if (AsChar(export.CourtCase.Item.Common.SelectChar) != 'S' && !
          IsEmpty(export.CourtCase.Item.Common.SelectChar))
        {
          ++local.ErrorSel.Count;
        }
      }

      export.CourtCase.CheckIndex();
    }

    // The user has entered invalid selection characters.  Highlight the invalid
    // selections.
    if (local.ErrorSel.Count > 0)
    {
      for(export.CourtCase.Index = 0; export.CourtCase.Index < export
        .CourtCase.Count; ++export.CourtCase.Index)
      {
        if (!export.CourtCase.CheckSize())
        {
          break;
        }

        if (AsChar(export.CourtCase.Item.Common.SelectChar) != 'S' && !
          IsEmpty(export.CourtCase.Item.Common.SelectChar))
        {
          var field = GetField(export.CourtCase.Item.Common, "selectChar");

          field.Error = true;
        }
      }

      export.CourtCase.CheckIndex();
      ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
    }

    // The user has selected multiple fields.  Highlight the invalid selections.
    if (local.SelCount.Count > 1)
    {
      for(export.CourtCase.Index = 0; export.CourtCase.Index < export
        .CourtCase.Count; ++export.CourtCase.Index)
      {
        if (!export.CourtCase.CheckSize())
        {
          break;
        }

        if (AsChar(export.CourtCase.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.CourtCase.Item.Common, "selectChar");

          field.Error = true;
        }
      }

      export.CourtCase.CheckIndex();
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      if (Equal(global.Command, "INVALID"))
      {
        export.Standard.NextTransaction = "";
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
      }

      if (!Equal(global.Command, "ENTER"))
      {
        export.Standard.NextTransaction = "";

        goto Test2;
      }

      export.Hidden.CaseNumber = export.Case1.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }
    else
    {
    }

Test2:

    if (Equal(global.Command, "COMP") || Equal(global.Command, "OCTO") || Equal
      (global.Command, "WORK") || Equal(global.Command, "LGRQ") || Equal
      (global.Command, "OBLO") || Equal(global.Command, "INCL") || Equal
      (global.Command, "DEBT") || Equal(global.Command, "APSM"))
    {
    }
    else
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "COMP") || Equal(global.Command, "OCTO") || Equal
      (global.Command, "WORK") || Equal(global.Command, "LGRQ") || Equal
      (global.Command, "OBLO") || Equal(global.Command, "INCL") || Equal
      (global.Command, "DEBT") || Equal(global.Command, "APSM") || Equal
      (global.Command, "RETURN"))
    {
      if (!export.HidnPassNarrative.IsEmpty)
      {
        export.HidnPassNarrative.Index = 4;
        export.HidnPassNarrative.CheckSize();

        if (!Equal(export.EnfReview.Text,
          export.HidnPassNarrative.Item.GexportH.Text))
        {
          export.HidnPassNarrative.Update.GexportH.Text = export.EnfReview.Text;
        }
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "OCTO":
        if (local.ErrorSel.Count == 0 && local.SelCount.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        }

        if (!export.CourtCase.IsEmpty)
        {
          for(export.CourtCase.Index = 0; export.CourtCase.Index < export
            .CourtCase.Count; ++export.CourtCase.Index)
          {
            if (!export.CourtCase.CheckSize())
            {
              break;
            }

            if (AsChar(export.CourtCase.Item.Common.SelectChar) == 'S')
            {
              export.SelectOcto.StandardNumber =
                export.CourtCase.Item.LegalAction.StandardNumber ?? "";
              ExitState = "ECO_LNK_TO_OCTO";

              goto Test3;
            }
          }

          export.CourtCase.CheckIndex();
        }

        break;
      case "COMP":
        ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

        break;
      case "APSM":
        ExitState = "ECO_LNK_TO_APSM";

        break;
      case "DEBT":
        ExitState = "ECO_LNK_TO_LST_DBT_ACT_BY_APPYR";

        break;
      case "INCL":
        ExitState = "ECO_LNK_TO_INCL_INC_SOURCE_LIST";

        break;
      case "WORK":
        // *****************************************************************
        //   An IDCR must be entered for WORK in order to make the flow
        //   to there returnable from WORK.  Pending that IDCR being done,
        //   the exit state below will be set.
        // *****************************************************************
        export.HiddenFlowToWork.Flag = "Y";
        export.FromCren.Flag = "Y";
        ExitState = "ECO_LNK_TO_WORKSHEET";

        break;
      case "LGRQ":
        ExitState = "ECO_LNK_TO_LEGAL_REQUEST";

        break;
      case "OBLO":
        ExitState = "ECO_LNK_OBLO_ADM_ACTS_BY_OBLIGOR";

        break;
      default:
        break;
    }

Test3:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        UseSpCrenDisplayCaseReviewEnf();

        // If the exit state indicates the list is full, it's ok, keep 
        // processing.
        // Any other non-ok exit state indicates an error occurred.
        if (!IsExitState("ACO_NN0000_ALL_OK") && !
          IsExitState("ACO_NI0000_LST_RETURNED_FULL"))
        {
          return;
        }

        // Set the displayed group's subscript to one so it will display 
        // starting with the first item
        export.HiddenCrtCaseSubscript.Count = 1;

        if (Equal(import.HiddenPassedReviewType.ActionEntry, "R"))
        {
          var field = GetField(export.EnfReview, "text");

          field.Color = "cyan";
          field.Protected = true;
        }
        else if (!export.HidnPassNarrative.IsEmpty)
        {
          if (Equal(export.HiddenPassedReviewType.ActionEntry, "P"))
          {
            export.HidnPassNarrative.Index = 4;
            export.HidnPassNarrative.CheckSize();

            export.EnfReview.Text = export.HidnPassNarrative.Item.GexportH.Text;
          }
          else if (AsChar(export.HiddenModWMedRvw.Flag) != 'Y')
          {
            export.HidnPassNarrative.Index = export.HidnPassNarrative.Count - 1;

            for(var increment = export.HidnPassNarrative.Count; increment >= 0
              ? export.HidnPassNarrative.Index < export
              .HidnPassNarrative.Count : export.HidnPassNarrative.Index + 1 >= export
              .HidnPassNarrative.Count; export
              .HidnPassNarrative.Index = export.HidnPassNarrative.Index + 1 + increment
              - 1)
            {
              if (!export.HidnPassNarrative.CheckSize())
              {
                break;
              }

              export.EnfReview.Text =
                export.HidnPassNarrative.Item.GexportH.Text;
            }

            export.HidnPassNarrative.CheckIndex();
          }
          else
          {
            export.HidnPassNarrative.Index = 1;
            export.HidnPassNarrative.CheckSize();

            export.EnfReview.Text = export.HidnPassNarrative.Item.GexportH.Text;
          }
        }

        // Notify the user if multiple APs exist on the case.
        switch(export.CountNoOfApOnCase.Count)
        {
          case 0:
            export.MoreApsMsg.Text30 = "";

            break;
          case 1:
            export.MoreApsMsg.Text30 = "";

            break;
          case 2:
            export.MoreApsMsg.Text30 = "Two AP's exist on case";

            break;
          default:
            export.MoreApsMsg.Text30 = "More than 2 AP's exist on case";

            break;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "ENTER":
        if (Equal(import.HiddenPassedReviewType.ActionEntry, "M") || Equal
          (import.HiddenPassedReviewType.ActionEntry, "P"))
        {
          if (IsEmpty(export.EnfReview.Text))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            var field = GetField(export.EnfReview, "text");

            field.Error = true;

            return;
          }
          else
          {
            export.HidnPassNarrative.Index = 4;
            export.HidnPassNarrative.CheckSize();

            export.HidnPassNarrative.Update.GexportH.Text =
              export.EnfReview.Text;
          }

          global.Command = "ADD";
          export.CommandPassedToInitial.Flag = "A";
        }
        else
        {
          global.Command = "DISPLAY";
          export.CommandPassedToInitial.Flag = "D";
        }

        if (Equal(import.HiddenPassedReviewType.ActionEntry, "M"))
        {
          if (AsChar(import.HiddenFlowToWork.Flag) == 'Y')
          {
            // mjr
            // --------------------------------------------
            // 02/25/2002
            // PR139864 - Added mod order ind
            // If this has not been set yet, make the user enter a value
            // ---------------------------------------------------------
            switch(AsChar(export.ModOrderInd.Flag))
            {
              case 'Y':
                export.Flag.Flag = "Y";
                ExitState = "ECO_XFR_TO_CR_INITIAL";

                break;
              case 'N':
                export.Flag.Flag = "Y";
                ExitState = "ECO_XFR_TO_CR_INITIAL";

                break;
              case ' ':
                var field1 = GetField(export.EnfReview, "text");

                field1.Color = "cyan";
                field1.Protected = true;

                var field2 = GetField(export.ModOrderInd, "flag");

                field2.Error = true;

                ExitState = "SP0000_REVIEW_RESULT_IN_ORD_MOD";

                break;
              default:
                var field3 = GetField(export.EnfReview, "text");

                field3.Color = "cyan";
                field3.Protected = true;

                var field4 = GetField(export.ModOrderInd, "flag");

                field4.Error = true;

                ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                break;
            }
          }
          else
          {
            ExitState = "MUST_FLOW_TO_WORK";
          }
        }
        else
        {
          export.Flag.Flag = "Y";
          ExitState = "ECO_XFR_TO_CR_INITIAL";
        }

        break;
      case "PREV":
        // If the user is already on the first page, provide a message.
        // Otherwise, set the display group's subscript to the previous page.
        if (export.HiddenCrtCaseSubscript.Count == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";
        }
        else
        {
          export.HiddenCrtCaseSubscript.Count -= Export.CourtCaseGroup.Capacity;
        }

        break;
      case "NEXT":
        // If the user is already on the last page, provide a message.
        // Otherwise, set the display group's subscript to the next page.
        if (export.HiddenCrtCaseSubscript.Count + Export
          .CourtCaseGroup.Capacity > export.HiddenGroupCourtCase.Count)
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";
        }
        else
        {
          export.HiddenCrtCaseSubscript.Count += Export.CourtCaseGroup.Capacity;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    if (AsChar(export.CaseClosedIndicator.Flag) == 'Y')
    {
      var field = GetField(export.EnfReview, "text");

      field.Color = "cyan";
      field.Protected = true;
    }

    // Populate scrollable group field with data from the current 'page'.
    for(export.CourtCase.Index = 0; export.CourtCase.Index < Export
      .CourtCaseGroup.Capacity; ++export.CourtCase.Index)
    {
      if (!export.CourtCase.CheckSize())
      {
        break;
      }

      export.HiddenGroupCourtCase.Index =
        export.HiddenCrtCaseSubscript.Count - 1;
      export.HiddenGroupCourtCase.CheckSize();

      export.CourtCase.Update.Common.SelectChar =
        export.HiddenGroupCourtCase.Item.HiddenCommon.SelectChar;
      MoveLegalAction(export.HiddenGroupCourtCase.Item.HiddenLegalAction,
        export.CourtCase.Update.LegalAction);
      export.CourtCase.Update.TotalPayoff.TotalCurrency =
        export.HiddenGroupCourtCase.Item.HiddenGrpTotalPayoff.TotalCurrency;
      export.CourtCase.Update.MwoDate.Date =
        export.HiddenGroupCourtCase.Item.HiddenGrpMwoDate.Date;
      export.CourtCase.Update.IwoDate.Date =
        export.HiddenGroupCourtCase.Item.HiddenGrpIwoDate.Date;
      export.CourtCase.Update.LastPayment.Date =
        export.HiddenGroupCourtCase.Item.HiddenGrpLastPayment.Date;
      export.CourtCase.Update.MonthlyDue.TotalCurrency =
        export.HiddenGroupCourtCase.Item.HiddenGrpMonthlyDue.TotalCurrency;
      ++export.HiddenCrtCaseSubscript.Count;

      if (export.HiddenCrtCaseSubscript.Count > export
        .HiddenGroupCourtCase.Count)
      {
        break;
      }
    }

    export.CourtCase.CheckIndex();

    // Reset subscript to indicate the first item in the currently displayed 
    // group
    export.HiddenCrtCaseSubscript.Count -= export.CourtCase.Count;

    // Protect the select field on group entries that don't have an identifier 
    // associated to them.
    for(export.CourtCase.Index = 0; export.CourtCase.Index < export
      .CourtCase.Count; ++export.CourtCase.Index)
    {
      if (!export.CourtCase.CheckSize())
      {
        break;
      }

      if (export.CourtCase.Item.LegalAction.Identifier <= 0)
      {
        var field = GetField(export.CourtCase.Item.Common, "selectChar");

        field.Intensity = Intensity.Dark;
        field.Protected = true;
      }
    }

    export.CourtCase.CheckIndex();

    // Determine if moving to the previous page is a valid option
    if (export.HiddenCrtCaseSubscript.Count > Export.CourtCaseGroup.Capacity)
    {
      local.Less.Text1 = "-";
    }
    else
    {
      local.Less.Text1 = "";
    }

    // Determine if moving to the next page is a valid option
    if (export.HiddenGroupCourtCase.Count > export
      .HiddenCrtCaseSubscript.Count + Export.CourtCaseGroup.Capacity)
    {
      local.More.Text1 = "+";
    }
    else
    {
      local.More.Text1 = "";
    }

    // Set scroll indicator depending on current position within group of data
    export.ScrollMsg.ScrollingMessage = "More " + local.Less.Text1 + " " + local
      .More.Text1;
  }

  private static void MoveAgesOfChildren(SpCrenDisplayCaseReviewEnf.Export.
    AgesOfChildrenGroup source, Export.AgesGroup target)
  {
    target.Age.Text4 = source.AgeOfChild.Text4;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveFinanceDetailsToHiddenGroupCourtCase(
    SpCrenDisplayCaseReviewEnf.Export.FinanceDetailsGroup source,
    Export.HiddenGroupCourtCaseGroup target)
  {
    target.HiddenCommon.SelectChar = source.Common.SelectChar;
    target.HiddenGrpIwoDate.Date = source.IwoDate.Date;
    target.HiddenGrpLastPayment.Date = source.LastPayment.Date;
    MoveLegalAction(source.LegalAction, target.HiddenLegalAction);
    target.HiddenGrpMonthlyDue.TotalCurrency = source.MonthlyDue.TotalCurrency;
    target.HiddenGrpMwoDate.Date = source.MwoDate.Date;
    target.HiddenGrpTotalPayoff.TotalCurrency = source.Payoff.TotalCurrency;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.MaxDate.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

    useImport.Case1.Number = import.Case1.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpCrenDisplayCaseReviewEnf()
  {
    var useImport = new SpCrenDisplayCaseReviewEnf.Import();
    var useExport = new SpCrenDisplayCaseReviewEnf.Export();

    useImport.SelectedAp.Number = import.Ap.Number;
    useImport.Case1.Number = import.Case1.Number;
    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.HiddenPassInfra.SystemGeneratedIdentifier;

    Call(SpCrenDisplayCaseReviewEnf.Execute, useImport, useExport);

    MoveCsePersonsWorkSet2(useExport.Ap, export.Ap);
    export.ApDenialLtr.Date = useExport.ApLastModificationDen.Date;
    export.ApKnownToOtherCases.Flag = useExport.ApKnownToOtherCases.Flag;
    export.ApTotalMonthly.TotalCurrency =
      useExport.ApMonthlyIncome.TotalCurrency;
    MoveCsePersonsWorkSet2(useExport.Ar, export.Ar);
    export.ArDenialLtr.Date = useExport.ArLastModificationDen.Date;
    export.ArTotalMonthly.TotalCurrency =
      useExport.ArMonthlyIncome.TotalCurrency;
    export.Bankruptcy.BankruptcyFilingDate =
      useExport.Bankruptcy.BankruptcyFilingDate;
    export.Case1.Number = useExport.Case1.Number;
    export.Client.UnemploymentInd = useExport.CsePerson.UnemploymentInd;
    export.CountNoOfApOnCase.Count = useExport.NumberOfAps.Count;
    export.CreditReportingAp.Flag = useExport.CredCertification.Flag;
    export.FdsoAp.Flag = useExport.FdsoCertification.Flag;
    export.GoodCause.Code = useExport.GoodCause.Code;
    useExport.AgesOfChildren.CopyTo(export.Ages, MoveAgesOfChildren);
    useExport.FinanceDetails.CopyTo(
      export.HiddenGroupCourtCase, MoveFinanceDetailsToHiddenGroupCourtCase);
    export.LastIncomeIncrease.Date = useExport.LastIncomeIncrease.Date;
    export.LastReviewDate.Date = useExport.LastReviewDate.Date;
    export.ModificationRequest.Date = useExport.LastModificationRequest.Date;
    export.MostRecent.ReferralDate = useExport.Last.ReferralDate;
    export.NoOfChildren.Count = useExport.NumberOfChildren.Count;
    export.SdsoAp.Flag = useExport.SdsoCertification.Flag;
    export.EnfReview.Text = useExport.ReviewNote.Text;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A HdnPasLegActGroup group.</summary>
    [Serializable]
    public class HdnPasLegActGroup
    {
      /// <summary>
      /// A value of GimportPassH.
      /// </summary>
      [JsonPropertyName("gimportPassH")]
      public LegalAction GimportPassH
      {
        get => gimportPassH ??= new();
        set => gimportPassH = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private LegalAction gimportPassH;
    }

    /// <summary>A HiddenPassGroup group.</summary>
    [Serializable]
    public class HiddenPassGroup
    {
      /// <summary>
      /// A value of GimportHiddenPassedFlag.
      /// </summary>
      [JsonPropertyName("gimportHiddenPassedFlag")]
      public Common GimportHiddenPassedFlag
      {
        get => gimportHiddenPassedFlag ??= new();
        set => gimportHiddenPassedFlag = value;
      }

      /// <summary>
      /// A value of GimportH.
      /// </summary>
      [JsonPropertyName("gimportH")]
      public NarrativeWork GimportH
      {
        get => gimportH ??= new();
        set => gimportH = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common gimportHiddenPassedFlag;
      private NarrativeWork gimportH;
    }

    /// <summary>A CourtCaseGroup group.</summary>
    [Serializable]
    public class CourtCaseGroup
    {
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
      /// A value of IwoDate.
      /// </summary>
      [JsonPropertyName("iwoDate")]
      public DateWorkArea IwoDate
      {
        get => iwoDate ??= new();
        set => iwoDate = value;
      }

      /// <summary>
      /// A value of LastPayment.
      /// </summary>
      [JsonPropertyName("lastPayment")]
      public DateWorkArea LastPayment
      {
        get => lastPayment ??= new();
        set => lastPayment = value;
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
      /// A value of MonthlyDue.
      /// </summary>
      [JsonPropertyName("monthlyDue")]
      public Common MonthlyDue
      {
        get => monthlyDue ??= new();
        set => monthlyDue = value;
      }

      /// <summary>
      /// A value of MwoDate.
      /// </summary>
      [JsonPropertyName("mwoDate")]
      public DateWorkArea MwoDate
      {
        get => mwoDate ??= new();
        set => mwoDate = value;
      }

      /// <summary>
      /// A value of TotalPayoff.
      /// </summary>
      [JsonPropertyName("totalPayoff")]
      public Common TotalPayoff
      {
        get => totalPayoff ??= new();
        set => totalPayoff = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common common;
      private DateWorkArea iwoDate;
      private DateWorkArea lastPayment;
      private LegalAction legalAction;
      private Common monthlyDue;
      private DateWorkArea mwoDate;
      private Common totalPayoff;
    }

    /// <summary>A AgesGroup group.</summary>
    [Serializable]
    public class AgesGroup
    {
      /// <summary>
      /// A value of Age.
      /// </summary>
      [JsonPropertyName("age")]
      public TextWorkArea Age
      {
        get => age ??= new();
        set => age = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private TextWorkArea age;
    }

    /// <summary>A HiddenGroupCourtCaseGroup group.</summary>
    [Serializable]
    public class HiddenGroupCourtCaseGroup
    {
      /// <summary>
      /// A value of HiddenCommon.
      /// </summary>
      [JsonPropertyName("hiddenCommon")]
      public Common HiddenCommon
      {
        get => hiddenCommon ??= new();
        set => hiddenCommon = value;
      }

      /// <summary>
      /// A value of HiddenGrpIwoDate.
      /// </summary>
      [JsonPropertyName("hiddenGrpIwoDate")]
      public DateWorkArea HiddenGrpIwoDate
      {
        get => hiddenGrpIwoDate ??= new();
        set => hiddenGrpIwoDate = value;
      }

      /// <summary>
      /// A value of HiddenGrpLastPayment.
      /// </summary>
      [JsonPropertyName("hiddenGrpLastPayment")]
      public DateWorkArea HiddenGrpLastPayment
      {
        get => hiddenGrpLastPayment ??= new();
        set => hiddenGrpLastPayment = value;
      }

      /// <summary>
      /// A value of HiddenLegalAction.
      /// </summary>
      [JsonPropertyName("hiddenLegalAction")]
      public LegalAction HiddenLegalAction
      {
        get => hiddenLegalAction ??= new();
        set => hiddenLegalAction = value;
      }

      /// <summary>
      /// A value of HiddenGrpMonthlyDue.
      /// </summary>
      [JsonPropertyName("hiddenGrpMonthlyDue")]
      public Common HiddenGrpMonthlyDue
      {
        get => hiddenGrpMonthlyDue ??= new();
        set => hiddenGrpMonthlyDue = value;
      }

      /// <summary>
      /// A value of HiddenGrpMwoDate.
      /// </summary>
      [JsonPropertyName("hiddenGrpMwoDate")]
      public DateWorkArea HiddenGrpMwoDate
      {
        get => hiddenGrpMwoDate ??= new();
        set => hiddenGrpMwoDate = value;
      }

      /// <summary>
      /// A value of HiddenGrpTotalPayoff.
      /// </summary>
      [JsonPropertyName("hiddenGrpTotalPayoff")]
      public Common HiddenGrpTotalPayoff
      {
        get => hiddenGrpTotalPayoff ??= new();
        set => hiddenGrpTotalPayoff = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common hiddenCommon;
      private DateWorkArea hiddenGrpIwoDate;
      private DateWorkArea hiddenGrpLastPayment;
      private LegalAction hiddenLegalAction;
      private Common hiddenGrpMonthlyDue;
      private DateWorkArea hiddenGrpMwoDate;
      private Common hiddenGrpTotalPayoff;
    }

    /// <summary>
    /// Gets a value of HdnPasLegAct.
    /// </summary>
    [JsonIgnore]
    public Array<HdnPasLegActGroup> HdnPasLegAct => hdnPasLegAct ??= new(
      HdnPasLegActGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HdnPasLegAct for json serialization.
    /// </summary>
    [JsonPropertyName("hdnPasLegAct")]
    [Computed]
    public IList<HdnPasLegActGroup> HdnPasLegAct_Json
    {
      get => hdnPasLegAct;
      set => HdnPasLegAct.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenPass.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPassGroup> HiddenPass => hiddenPass ??= new(
      HiddenPassGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPass for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPass")]
    [Computed]
    public IList<HiddenPassGroup> HiddenPass_Json
    {
      get => hiddenPass;
      set => HiddenPass.Assign(value);
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
    /// A value of ApDenialLtr.
    /// </summary>
    [JsonPropertyName("apDenialLtr")]
    public DateWorkArea ApDenialLtr
    {
      get => apDenialLtr ??= new();
      set => apDenialLtr = value;
    }

    /// <summary>
    /// A value of ApKnownToOtherCases.
    /// </summary>
    [JsonPropertyName("apKnownToOtherCases")]
    public Common ApKnownToOtherCases
    {
      get => apKnownToOtherCases ??= new();
      set => apKnownToOtherCases = value;
    }

    /// <summary>
    /// A value of ApSelected.
    /// </summary>
    [JsonPropertyName("apSelected")]
    public Common ApSelected
    {
      get => apSelected ??= new();
      set => apSelected = value;
    }

    /// <summary>
    /// A value of ApTotalMonthly.
    /// </summary>
    [JsonPropertyName("apTotalMonthly")]
    public Common ApTotalMonthly
    {
      get => apTotalMonthly ??= new();
      set => apTotalMonthly = value;
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
    /// A value of ArDenialLtr.
    /// </summary>
    [JsonPropertyName("arDenialLtr")]
    public DateWorkArea ArDenialLtr
    {
      get => arDenialLtr ??= new();
      set => arDenialLtr = value;
    }

    /// <summary>
    /// A value of ArTotalMonthly.
    /// </summary>
    [JsonPropertyName("arTotalMonthly")]
    public Common ArTotalMonthly
    {
      get => arTotalMonthly ??= new();
      set => arTotalMonthly = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
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
    /// A value of CountAllCauReads.
    /// </summary>
    [JsonPropertyName("countAllCauReads")]
    public Common CountAllCauReads
    {
      get => countAllCauReads ??= new();
      set => countAllCauReads = value;
    }

    /// <summary>
    /// A value of CountNoOfApOnCase.
    /// </summary>
    [JsonPropertyName("countNoOfApOnCase")]
    public Common CountNoOfApOnCase
    {
      get => countNoOfApOnCase ??= new();
      set => countNoOfApOnCase = value;
    }

    /// <summary>
    /// A value of CreditReportingAp.
    /// </summary>
    [JsonPropertyName("creditReportingAp")]
    public Common CreditReportingAp
    {
      get => creditReportingAp ??= new();
      set => creditReportingAp = value;
    }

    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    /// <summary>
    /// A value of CaseClosedIndicator.
    /// </summary>
    [JsonPropertyName("caseClosedIndicator")]
    public Common CaseClosedIndicator
    {
      get => caseClosedIndicator ??= new();
      set => caseClosedIndicator = value;
    }

    /// <summary>
    /// A value of EnfReview.
    /// </summary>
    [JsonPropertyName("enfReview")]
    public NarrativeWork EnfReview
    {
      get => enfReview ??= new();
      set => enfReview = value;
    }

    /// <summary>
    /// A value of FdsoAp.
    /// </summary>
    [JsonPropertyName("fdsoAp")]
    public Common FdsoAp
    {
      get => fdsoAp ??= new();
      set => fdsoAp = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// Gets a value of CourtCase.
    /// </summary>
    [JsonIgnore]
    public Array<CourtCaseGroup> CourtCase => courtCase ??= new(
      CourtCaseGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CourtCase for json serialization.
    /// </summary>
    [JsonPropertyName("courtCase")]
    [Computed]
    public IList<CourtCaseGroup> CourtCase_Json
    {
      get => courtCase;
      set => CourtCase.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ages.
    /// </summary>
    [JsonIgnore]
    public Array<AgesGroup> Ages => ages ??= new(AgesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ages for json serialization.
    /// </summary>
    [JsonPropertyName("ages")]
    [Computed]
    public IList<AgesGroup> Ages_Json
    {
      get => ages;
      set => Ages.Assign(value);
    }

    /// <summary>
    /// A value of HiddenCrtCaseSubscript.
    /// </summary>
    [JsonPropertyName("hiddenCrtCaseSubscript")]
    public Common HiddenCrtCaseSubscript
    {
      get => hiddenCrtCaseSubscript ??= new();
      set => hiddenCrtCaseSubscript = value;
    }

    /// <summary>
    /// A value of HiddenFlowToWork.
    /// </summary>
    [JsonPropertyName("hiddenFlowToWork")]
    public Common HiddenFlowToWork
    {
      get => hiddenFlowToWork ??= new();
      set => hiddenFlowToWork = value;
    }

    /// <summary>
    /// Gets a value of HiddenGroupCourtCase.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroupCourtCaseGroup> HiddenGroupCourtCase =>
      hiddenGroupCourtCase ??= new(HiddenGroupCourtCaseGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenGroupCourtCase for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGroupCourtCase")]
    [Computed]
    public IList<HiddenGroupCourtCaseGroup> HiddenGroupCourtCase_Json
    {
      get => hiddenGroupCourtCase;
      set => HiddenGroupCourtCase.Assign(value);
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
    /// A value of HiddenPass1.
    /// </summary>
    [JsonPropertyName("hiddenPass1")]
    public Infrastructure HiddenPass1
    {
      get => hiddenPass1 ??= new();
      set => hiddenPass1 = value;
    }

    /// <summary>
    /// A value of HiddenPassedReviewType.
    /// </summary>
    [JsonPropertyName("hiddenPassedReviewType")]
    public Common HiddenPassedReviewType
    {
      get => hiddenPassedReviewType ??= new();
      set => hiddenPassedReviewType = value;
    }

    /// <summary>
    /// A value of LastIncomeIncrease.
    /// </summary>
    [JsonPropertyName("lastIncomeIncrease")]
    public DateWorkArea LastIncomeIncrease
    {
      get => lastIncomeIncrease ??= new();
      set => lastIncomeIncrease = value;
    }

    /// <summary>
    /// A value of LastReviewDate.
    /// </summary>
    [JsonPropertyName("lastReviewDate")]
    public DateWorkArea LastReviewDate
    {
      get => lastReviewDate ??= new();
      set => lastReviewDate = value;
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
    /// A value of ModOrderInd.
    /// </summary>
    [JsonPropertyName("modOrderInd")]
    public Common ModOrderInd
    {
      get => modOrderInd ??= new();
      set => modOrderInd = value;
    }

    /// <summary>
    /// A value of ModificationRequest.
    /// </summary>
    [JsonPropertyName("modificationRequest")]
    public DateWorkArea ModificationRequest
    {
      get => modificationRequest ??= new();
      set => modificationRequest = value;
    }

    /// <summary>
    /// A value of MoreApsMsg.
    /// </summary>
    [JsonPropertyName("moreApsMsg")]
    public TextWorkArea MoreApsMsg
    {
      get => moreApsMsg ??= new();
      set => moreApsMsg = value;
    }

    /// <summary>
    /// A value of MostRecent.
    /// </summary>
    [JsonPropertyName("mostRecent")]
    public LegalReferral MostRecent
    {
      get => mostRecent ??= new();
      set => mostRecent = value;
    }

    /// <summary>
    /// A value of MultiAp.
    /// </summary>
    [JsonPropertyName("multiAp")]
    public Common MultiAp
    {
      get => multiAp ??= new();
      set => multiAp = value;
    }

    /// <summary>
    /// A value of NoOfChildren.
    /// </summary>
    [JsonPropertyName("noOfChildren")]
    public Common NoOfChildren
    {
      get => noOfChildren ??= new();
      set => noOfChildren = value;
    }

    /// <summary>
    /// A value of PreviousCommand.
    /// </summary>
    [JsonPropertyName("previousCommand")]
    public Common PreviousCommand
    {
      get => previousCommand ??= new();
      set => previousCommand = value;
    }

    /// <summary>
    /// A value of ScrollMsg.
    /// </summary>
    [JsonPropertyName("scrollMsg")]
    public Standard ScrollMsg
    {
      get => scrollMsg ??= new();
      set => scrollMsg = value;
    }

    /// <summary>
    /// A value of SdsoAp.
    /// </summary>
    [JsonPropertyName("sdsoAp")]
    public Common SdsoAp
    {
      get => sdsoAp ??= new();
      set => sdsoAp = value;
    }

    /// <summary>
    /// A value of SelectAp.
    /// </summary>
    [JsonPropertyName("selectAp")]
    public Common SelectAp
    {
      get => selectAp ??= new();
      set => selectAp = value;
    }

    /// <summary>
    /// A value of SelectedAp.
    /// </summary>
    [JsonPropertyName("selectedAp")]
    public CsePersonsWorkSet SelectedAp
    {
      get => selectedAp ??= new();
      set => selectedAp = value;
    }

    /// <summary>
    /// A value of SelectedApObligor.
    /// </summary>
    [JsonPropertyName("selectedApObligor")]
    public CsePerson SelectedApObligor
    {
      get => selectedApObligor ??= new();
      set => selectedApObligor = value;
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
    /// A value of TotalAdcAp.
    /// </summary>
    [JsonPropertyName("totalAdcAp")]
    public Common TotalAdcAp
    {
      get => totalAdcAp ??= new();
      set => totalAdcAp = value;
    }

    /// <summary>
    /// A value of TotalNonAdcAp.
    /// </summary>
    [JsonPropertyName("totalNonAdcAp")]
    public Common TotalNonAdcAp
    {
      get => totalNonAdcAp ??= new();
      set => totalNonAdcAp = value;
    }

    private Array<HdnPasLegActGroup> hdnPasLegAct;
    private Array<HiddenPassGroup> hiddenPass;
    private CsePersonsWorkSet ap;
    private DateWorkArea apDenialLtr;
    private Common apKnownToOtherCases;
    private Common apSelected;
    private Common apTotalMonthly;
    private CsePersonsWorkSet ar;
    private DateWorkArea arDenialLtr;
    private Common arTotalMonthly;
    private Bankruptcy bankruptcy;
    private Case1 case1;
    private Common countAllCauReads;
    private Common countNoOfApOnCase;
    private Common creditReportingAp;
    private CsePerson client;
    private Common caseClosedIndicator;
    private NarrativeWork enfReview;
    private Common fdsoAp;
    private GoodCause goodCause;
    private Array<CourtCaseGroup> courtCase;
    private Array<AgesGroup> ages;
    private Common hiddenCrtCaseSubscript;
    private Common hiddenFlowToWork;
    private Array<HiddenGroupCourtCaseGroup> hiddenGroupCourtCase;
    private NextTranInfo hidden;
    private Infrastructure hiddenPass1;
    private Common hiddenPassedReviewType;
    private DateWorkArea lastIncomeIncrease;
    private DateWorkArea lastReviewDate;
    private LegalAction legalAction;
    private Common modOrderInd;
    private DateWorkArea modificationRequest;
    private TextWorkArea moreApsMsg;
    private LegalReferral mostRecent;
    private Common multiAp;
    private Common noOfChildren;
    private Common previousCommand;
    private Standard scrollMsg;
    private Common sdsoAp;
    private Common selectAp;
    private CsePersonsWorkSet selectedAp;
    private CsePerson selectedApObligor;
    private Standard standard;
    private Common totalAdcAp;
    private Common totalNonAdcAp;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CourtCaseGroup group.</summary>
    [Serializable]
    public class CourtCaseGroup
    {
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
      /// A value of IwoDate.
      /// </summary>
      [JsonPropertyName("iwoDate")]
      public DateWorkArea IwoDate
      {
        get => iwoDate ??= new();
        set => iwoDate = value;
      }

      /// <summary>
      /// A value of LastPayment.
      /// </summary>
      [JsonPropertyName("lastPayment")]
      public DateWorkArea LastPayment
      {
        get => lastPayment ??= new();
        set => lastPayment = value;
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
      /// A value of MonthlyDue.
      /// </summary>
      [JsonPropertyName("monthlyDue")]
      public Common MonthlyDue
      {
        get => monthlyDue ??= new();
        set => monthlyDue = value;
      }

      /// <summary>
      /// A value of MwoDate.
      /// </summary>
      [JsonPropertyName("mwoDate")]
      public DateWorkArea MwoDate
      {
        get => mwoDate ??= new();
        set => mwoDate = value;
      }

      /// <summary>
      /// A value of TotalPayoff.
      /// </summary>
      [JsonPropertyName("totalPayoff")]
      public Common TotalPayoff
      {
        get => totalPayoff ??= new();
        set => totalPayoff = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common common;
      private DateWorkArea iwoDate;
      private DateWorkArea lastPayment;
      private LegalAction legalAction;
      private Common monthlyDue;
      private DateWorkArea mwoDate;
      private Common totalPayoff;
    }

    /// <summary>A AgesGroup group.</summary>
    [Serializable]
    public class AgesGroup
    {
      /// <summary>
      /// A value of Age.
      /// </summary>
      [JsonPropertyName("age")]
      public TextWorkArea Age
      {
        get => age ??= new();
        set => age = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private TextWorkArea age;
    }

    /// <summary>A HiddenGroupCourtCaseGroup group.</summary>
    [Serializable]
    public class HiddenGroupCourtCaseGroup
    {
      /// <summary>
      /// A value of HiddenCommon.
      /// </summary>
      [JsonPropertyName("hiddenCommon")]
      public Common HiddenCommon
      {
        get => hiddenCommon ??= new();
        set => hiddenCommon = value;
      }

      /// <summary>
      /// A value of HiddenGrpIwoDate.
      /// </summary>
      [JsonPropertyName("hiddenGrpIwoDate")]
      public DateWorkArea HiddenGrpIwoDate
      {
        get => hiddenGrpIwoDate ??= new();
        set => hiddenGrpIwoDate = value;
      }

      /// <summary>
      /// A value of HiddenGrpLastPayment.
      /// </summary>
      [JsonPropertyName("hiddenGrpLastPayment")]
      public DateWorkArea HiddenGrpLastPayment
      {
        get => hiddenGrpLastPayment ??= new();
        set => hiddenGrpLastPayment = value;
      }

      /// <summary>
      /// A value of HiddenLegalAction.
      /// </summary>
      [JsonPropertyName("hiddenLegalAction")]
      public LegalAction HiddenLegalAction
      {
        get => hiddenLegalAction ??= new();
        set => hiddenLegalAction = value;
      }

      /// <summary>
      /// A value of HiddenGrpMonthlyDue.
      /// </summary>
      [JsonPropertyName("hiddenGrpMonthlyDue")]
      public Common HiddenGrpMonthlyDue
      {
        get => hiddenGrpMonthlyDue ??= new();
        set => hiddenGrpMonthlyDue = value;
      }

      /// <summary>
      /// A value of HiddenGrpMwoDate.
      /// </summary>
      [JsonPropertyName("hiddenGrpMwoDate")]
      public DateWorkArea HiddenGrpMwoDate
      {
        get => hiddenGrpMwoDate ??= new();
        set => hiddenGrpMwoDate = value;
      }

      /// <summary>
      /// A value of HiddenGrpTotalPayoff.
      /// </summary>
      [JsonPropertyName("hiddenGrpTotalPayoff")]
      public Common HiddenGrpTotalPayoff
      {
        get => hiddenGrpTotalPayoff ??= new();
        set => hiddenGrpTotalPayoff = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common hiddenCommon;
      private DateWorkArea hiddenGrpIwoDate;
      private DateWorkArea hiddenGrpLastPayment;
      private LegalAction hiddenLegalAction;
      private Common hiddenGrpMonthlyDue;
      private DateWorkArea hiddenGrpMwoDate;
      private Common hiddenGrpTotalPayoff;
    }

    /// <summary>A HidnPassNarrativeGroup group.</summary>
    [Serializable]
    public class HidnPassNarrativeGroup
    {
      /// <summary>
      /// A value of GexportHiddenPassedFlag.
      /// </summary>
      [JsonPropertyName("gexportHiddenPassedFlag")]
      public Common GexportHiddenPassedFlag
      {
        get => gexportHiddenPassedFlag ??= new();
        set => gexportHiddenPassedFlag = value;
      }

      /// <summary>
      /// A value of GexportH.
      /// </summary>
      [JsonPropertyName("gexportH")]
      public NarrativeWork GexportH
      {
        get => gexportH ??= new();
        set => gexportH = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common gexportHiddenPassedFlag;
      private NarrativeWork gexportH;
    }

    /// <summary>A HdnPasLegActGroup group.</summary>
    [Serializable]
    public class HdnPasLegActGroup
    {
      /// <summary>
      /// A value of GexportPassH.
      /// </summary>
      [JsonPropertyName("gexportPassH")]
      public LegalAction GexportPassH
      {
        get => gexportPassH ??= new();
        set => gexportPassH = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private LegalAction gexportPassH;
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
    /// A value of ApDenialLtr.
    /// </summary>
    [JsonPropertyName("apDenialLtr")]
    public DateWorkArea ApDenialLtr
    {
      get => apDenialLtr ??= new();
      set => apDenialLtr = value;
    }

    /// <summary>
    /// A value of ApKnownToOtherCases.
    /// </summary>
    [JsonPropertyName("apKnownToOtherCases")]
    public Common ApKnownToOtherCases
    {
      get => apKnownToOtherCases ??= new();
      set => apKnownToOtherCases = value;
    }

    /// <summary>
    /// A value of ApSelected.
    /// </summary>
    [JsonPropertyName("apSelected")]
    public Common ApSelected
    {
      get => apSelected ??= new();
      set => apSelected = value;
    }

    /// <summary>
    /// A value of ApTotalMonthly.
    /// </summary>
    [JsonPropertyName("apTotalMonthly")]
    public Common ApTotalMonthly
    {
      get => apTotalMonthly ??= new();
      set => apTotalMonthly = value;
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
    /// A value of ArDenialLtr.
    /// </summary>
    [JsonPropertyName("arDenialLtr")]
    public DateWorkArea ArDenialLtr
    {
      get => arDenialLtr ??= new();
      set => arDenialLtr = value;
    }

    /// <summary>
    /// A value of ArTotalMonthly.
    /// </summary>
    [JsonPropertyName("arTotalMonthly")]
    public Common ArTotalMonthly
    {
      get => arTotalMonthly ??= new();
      set => arTotalMonthly = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
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
    /// A value of CaseClosedIndicator.
    /// </summary>
    [JsonPropertyName("caseClosedIndicator")]
    public Common CaseClosedIndicator
    {
      get => caseClosedIndicator ??= new();
      set => caseClosedIndicator = value;
    }

    /// <summary>
    /// A value of CaseInReview.
    /// </summary>
    [JsonPropertyName("caseInReview")]
    public Case1 CaseInReview
    {
      get => caseInReview ??= new();
      set => caseInReview = value;
    }

    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    /// <summary>
    /// A value of CommandPassedToInitial.
    /// </summary>
    [JsonPropertyName("commandPassedToInitial")]
    public Common CommandPassedToInitial
    {
      get => commandPassedToInitial ??= new();
      set => commandPassedToInitial = value;
    }

    /// <summary>
    /// A value of CountAllCauReads.
    /// </summary>
    [JsonPropertyName("countAllCauReads")]
    public Common CountAllCauReads
    {
      get => countAllCauReads ??= new();
      set => countAllCauReads = value;
    }

    /// <summary>
    /// A value of CountNoOfApOnCase.
    /// </summary>
    [JsonPropertyName("countNoOfApOnCase")]
    public Common CountNoOfApOnCase
    {
      get => countNoOfApOnCase ??= new();
      set => countNoOfApOnCase = value;
    }

    /// <summary>
    /// A value of CreditReportingAp.
    /// </summary>
    [JsonPropertyName("creditReportingAp")]
    public Common CreditReportingAp
    {
      get => creditReportingAp ??= new();
      set => creditReportingAp = value;
    }

    /// <summary>
    /// A value of EnfReview.
    /// </summary>
    [JsonPropertyName("enfReview")]
    public NarrativeWork EnfReview
    {
      get => enfReview ??= new();
      set => enfReview = value;
    }

    /// <summary>
    /// A value of FdsoAp.
    /// </summary>
    [JsonPropertyName("fdsoAp")]
    public Common FdsoAp
    {
      get => fdsoAp ??= new();
      set => fdsoAp = value;
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
    /// A value of FromCren.
    /// </summary>
    [JsonPropertyName("fromCren")]
    public Common FromCren
    {
      get => fromCren ??= new();
      set => fromCren = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// Gets a value of CourtCase.
    /// </summary>
    [JsonIgnore]
    public Array<CourtCaseGroup> CourtCase => courtCase ??= new(
      CourtCaseGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CourtCase for json serialization.
    /// </summary>
    [JsonPropertyName("courtCase")]
    [Computed]
    public IList<CourtCaseGroup> CourtCase_Json
    {
      get => courtCase;
      set => CourtCase.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ages.
    /// </summary>
    [JsonIgnore]
    public Array<AgesGroup> Ages => ages ??= new(AgesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ages for json serialization.
    /// </summary>
    [JsonPropertyName("ages")]
    [Computed]
    public IList<AgesGroup> Ages_Json
    {
      get => ages;
      set => Ages.Assign(value);
    }

    /// <summary>
    /// A value of HiddenCrtCaseSubscript.
    /// </summary>
    [JsonPropertyName("hiddenCrtCaseSubscript")]
    public Common HiddenCrtCaseSubscript
    {
      get => hiddenCrtCaseSubscript ??= new();
      set => hiddenCrtCaseSubscript = value;
    }

    /// <summary>
    /// A value of HiddenFlowToWork.
    /// </summary>
    [JsonPropertyName("hiddenFlowToWork")]
    public Common HiddenFlowToWork
    {
      get => hiddenFlowToWork ??= new();
      set => hiddenFlowToWork = value;
    }

    /// <summary>
    /// Gets a value of HiddenGroupCourtCase.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroupCourtCaseGroup> HiddenGroupCourtCase =>
      hiddenGroupCourtCase ??= new(HiddenGroupCourtCaseGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenGroupCourtCase for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGroupCourtCase")]
    [Computed]
    public IList<HiddenGroupCourtCaseGroup> HiddenGroupCourtCase_Json
    {
      get => hiddenGroupCourtCase;
      set => HiddenGroupCourtCase.Assign(value);
    }

    /// <summary>
    /// A value of HiddenModWMedRvw.
    /// </summary>
    [JsonPropertyName("hiddenModWMedRvw")]
    public Common HiddenModWMedRvw
    {
      get => hiddenModWMedRvw ??= new();
      set => hiddenModWMedRvw = value;
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
    /// A value of HiddenPassInfra.
    /// </summary>
    [JsonPropertyName("hiddenPassInfra")]
    public Infrastructure HiddenPassInfra
    {
      get => hiddenPassInfra ??= new();
      set => hiddenPassInfra = value;
    }

    /// <summary>
    /// A value of HiddenPassedReviewType.
    /// </summary>
    [JsonPropertyName("hiddenPassedReviewType")]
    public Common HiddenPassedReviewType
    {
      get => hiddenPassedReviewType ??= new();
      set => hiddenPassedReviewType = value;
    }

    /// <summary>
    /// A value of LastIncomeIncrease.
    /// </summary>
    [JsonPropertyName("lastIncomeIncrease")]
    public DateWorkArea LastIncomeIncrease
    {
      get => lastIncomeIncrease ??= new();
      set => lastIncomeIncrease = value;
    }

    /// <summary>
    /// A value of LastReviewDate.
    /// </summary>
    [JsonPropertyName("lastReviewDate")]
    public DateWorkArea LastReviewDate
    {
      get => lastReviewDate ??= new();
      set => lastReviewDate = value;
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
    /// A value of ModOrderInd.
    /// </summary>
    [JsonPropertyName("modOrderInd")]
    public Common ModOrderInd
    {
      get => modOrderInd ??= new();
      set => modOrderInd = value;
    }

    /// <summary>
    /// A value of ModificationRequest.
    /// </summary>
    [JsonPropertyName("modificationRequest")]
    public DateWorkArea ModificationRequest
    {
      get => modificationRequest ??= new();
      set => modificationRequest = value;
    }

    /// <summary>
    /// A value of MoreApsMsg.
    /// </summary>
    [JsonPropertyName("moreApsMsg")]
    public TextWorkArea MoreApsMsg
    {
      get => moreApsMsg ??= new();
      set => moreApsMsg = value;
    }

    /// <summary>
    /// A value of MostRecent.
    /// </summary>
    [JsonPropertyName("mostRecent")]
    public LegalReferral MostRecent
    {
      get => mostRecent ??= new();
      set => mostRecent = value;
    }

    /// <summary>
    /// A value of MultiAp.
    /// </summary>
    [JsonPropertyName("multiAp")]
    public Common MultiAp
    {
      get => multiAp ??= new();
      set => multiAp = value;
    }

    /// <summary>
    /// A value of NoOfChildren.
    /// </summary>
    [JsonPropertyName("noOfChildren")]
    public Common NoOfChildren
    {
      get => noOfChildren ??= new();
      set => noOfChildren = value;
    }

    /// <summary>
    /// A value of PassSelectedAp.
    /// </summary>
    [JsonPropertyName("passSelectedAp")]
    public CsePersonsWorkSet PassSelectedAp
    {
      get => passSelectedAp ??= new();
      set => passSelectedAp = value;
    }

    /// <summary>
    /// A value of PreviousCommand.
    /// </summary>
    [JsonPropertyName("previousCommand")]
    public Common PreviousCommand
    {
      get => previousCommand ??= new();
      set => previousCommand = value;
    }

    /// <summary>
    /// A value of SdsoAp.
    /// </summary>
    [JsonPropertyName("sdsoAp")]
    public Common SdsoAp
    {
      get => sdsoAp ??= new();
      set => sdsoAp = value;
    }

    /// <summary>
    /// A value of SelectOcto.
    /// </summary>
    [JsonPropertyName("selectOcto")]
    public LegalAction SelectOcto
    {
      get => selectOcto ??= new();
      set => selectOcto = value;
    }

    /// <summary>
    /// A value of ScrollMsg.
    /// </summary>
    [JsonPropertyName("scrollMsg")]
    public Standard ScrollMsg
    {
      get => scrollMsg ??= new();
      set => scrollMsg = value;
    }

    /// <summary>
    /// A value of SelectAp.
    /// </summary>
    [JsonPropertyName("selectAp")]
    public Common SelectAp
    {
      get => selectAp ??= new();
      set => selectAp = value;
    }

    /// <summary>
    /// A value of TotalAdcAp.
    /// </summary>
    [JsonPropertyName("totalAdcAp")]
    public Common TotalAdcAp
    {
      get => totalAdcAp ??= new();
      set => totalAdcAp = value;
    }

    /// <summary>
    /// A value of TotalNonAdcAp.
    /// </summary>
    [JsonPropertyName("totalNonAdcAp")]
    public Common TotalNonAdcAp
    {
      get => totalNonAdcAp ??= new();
      set => totalNonAdcAp = value;
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
    /// A value of SelectedApToDebt.
    /// </summary>
    [JsonPropertyName("selectedApToDebt")]
    public CsePerson SelectedApToDebt
    {
      get => selectedApToDebt ??= new();
      set => selectedApToDebt = value;
    }

    /// <summary>
    /// A value of SelectedAp.
    /// </summary>
    [JsonPropertyName("selectedAp")]
    public CsePersonsWorkSet SelectedAp
    {
      get => selectedAp ??= new();
      set => selectedAp = value;
    }

    /// <summary>
    /// Gets a value of HidnPassNarrative.
    /// </summary>
    [JsonIgnore]
    public Array<HidnPassNarrativeGroup> HidnPassNarrative =>
      hidnPassNarrative ??= new(HidnPassNarrativeGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HidnPassNarrative for json serialization.
    /// </summary>
    [JsonPropertyName("hidnPassNarrative")]
    [Computed]
    public IList<HidnPassNarrativeGroup> HidnPassNarrative_Json
    {
      get => hidnPassNarrative;
      set => HidnPassNarrative.Assign(value);
    }

    /// <summary>
    /// Gets a value of HdnPasLegAct.
    /// </summary>
    [JsonIgnore]
    public Array<HdnPasLegActGroup> HdnPasLegAct => hdnPasLegAct ??= new(
      HdnPasLegActGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HdnPasLegAct for json serialization.
    /// </summary>
    [JsonPropertyName("hdnPasLegAct")]
    [Computed]
    public IList<HdnPasLegActGroup> HdnPasLegAct_Json
    {
      get => hdnPasLegAct;
      set => HdnPasLegAct.Assign(value);
    }

    private CsePersonsWorkSet ap;
    private DateWorkArea apDenialLtr;
    private Common apKnownToOtherCases;
    private Common apSelected;
    private Common apTotalMonthly;
    private CsePersonsWorkSet ar;
    private DateWorkArea arDenialLtr;
    private Common arTotalMonthly;
    private Bankruptcy bankruptcy;
    private Case1 case1;
    private Common caseClosedIndicator;
    private Case1 caseInReview;
    private CsePerson client;
    private Common commandPassedToInitial;
    private Common countAllCauReads;
    private Common countNoOfApOnCase;
    private Common creditReportingAp;
    private NarrativeWork enfReview;
    private Common fdsoAp;
    private Common flag;
    private Common fromCren;
    private GoodCause goodCause;
    private Array<CourtCaseGroup> courtCase;
    private Array<AgesGroup> ages;
    private Common hiddenCrtCaseSubscript;
    private Common hiddenFlowToWork;
    private Array<HiddenGroupCourtCaseGroup> hiddenGroupCourtCase;
    private Common hiddenModWMedRvw;
    private NextTranInfo hidden;
    private Infrastructure hiddenPassInfra;
    private Common hiddenPassedReviewType;
    private DateWorkArea lastIncomeIncrease;
    private DateWorkArea lastReviewDate;
    private LegalAction legalAction;
    private Common modOrderInd;
    private DateWorkArea modificationRequest;
    private TextWorkArea moreApsMsg;
    private LegalReferral mostRecent;
    private Common multiAp;
    private Common noOfChildren;
    private CsePersonsWorkSet passSelectedAp;
    private Common previousCommand;
    private Common sdsoAp;
    private LegalAction selectOcto;
    private Standard scrollMsg;
    private Common selectAp;
    private Common totalAdcAp;
    private Common totalNonAdcAp;
    private Standard standard;
    private CsePerson selectedApToDebt;
    private CsePersonsWorkSet selectedAp;
    private Array<HidnPassNarrativeGroup> hidnPassNarrative;
    private Array<HdnPasLegActGroup> hdnPasLegAct;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of ErrorSel.
    /// </summary>
    [JsonPropertyName("errorSel")]
    public Common ErrorSel
    {
      get => errorSel ??= new();
      set => errorSel = value;
    }

    /// <summary>
    /// A value of Less.
    /// </summary>
    [JsonPropertyName("less")]
    public TextWorkArea Less
    {
      get => less ??= new();
      set => less = value;
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
    /// A value of Min.
    /// </summary>
    [JsonPropertyName("min")]
    public DateWorkArea Min
    {
      get => min ??= new();
      set => min = value;
    }

    /// <summary>
    /// A value of More.
    /// </summary>
    [JsonPropertyName("more")]
    public TextWorkArea More
    {
      get => more ??= new();
      set => more = value;
    }

    /// <summary>
    /// A value of SelCount.
    /// </summary>
    [JsonPropertyName("selCount")]
    public Common SelCount
    {
      get => selCount ??= new();
      set => selCount = value;
    }

    private DateWorkArea current;
    private Common errorSel;
    private TextWorkArea less;
    private DateWorkArea maxDate;
    private DateWorkArea min;
    private TextWorkArea more;
    private Common selCount;
  }
#endregion
}
