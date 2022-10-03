// Program: FN_OACC_MTN_ACCRUING_OBLIGATION, ID: 372084041, model: 746.
// Short name: SWEOACCP
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
/// A program: FN_OACC_MTN_ACCRUING_OBLIGATION.
/// </para>
/// <para>
/// This procedure adds, updates, and displays Accruing Obligations, and related
/// details. Updates and deletes are only allowed on Obligations with no
/// activity.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOaccMtnAccruingObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OACC_MTN_ACCRUING_OBLIGATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOaccMtnAccruingObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOaccMtnAccruingObligation.
  /// </summary>
  public FnOaccMtnAccruingObligation(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************************************
    // DATE		PROGRAMMER		REF #	DESCRIPTION
    // -------------------------------------------------------------------
    // 10/24/96  Holly Kennedy-MTW	Changed logic to allow
    // 				the discontinue date
    // 				to be changed to a blank
    // 				value.
    // 				Changed logic to disallow
    // 				time frames of two obligations
    // 				to overlap.  The program was
    // 				escaping and never hitting the
    // 				logic.
    // 				Added Data level security.
    // 				Changed logic to pass the
    // 				Legal Action Detail back from
    // 				the existence and the
    // 				Read CABs as the value was
    // 				getting lost.
    // 				Fixed scrolling problem.
    // 				The More -+ was being displayed
    // 				when no other screens of data
    // 				existed.  This required changing
    // 				the screen scrolling attribute
    // 				to disallow entry.
    // 01/13/97  HOOKS		              raise events
    // 01/23/97  HOOKS		              ADD LOGIC FOR HIST/MONA
    // 
    // AUTOMATIC NEXTTRAN
    // ***--- Sumanta Mahapatra - MTW - 04/30/97
    //  Made the following changes :-
    //    *- Changed the DB2 dates to IEF dates..
    //    *- Added logic for Alternate address ..
    //    *- Added logic for interstate case# and state
    //    *- Added flow to DPAY
    //  Sumant - 05/13/97
    //    *- Added flow to ASIN and related code
    // 12/29/97	Venkatesh Kamraj	Changed to set the situation # to 0 instead of 
    // calling get_next_situation_no because of changes to infrastucture
    // 3/23/98		Siraj Konkader			ZDEL cleanup
    // **********************************************************************
    // *** 9/8/98  B Adams      restored some views; fixed IF at bottom  ***
    // *** 10/21/98  B Adams    Removed links to ASIN and DPAY;
    // 			not applicable to this function.
    // ------------------------------
    // 12/15/98 - B Adams  -  Now, interstate data is not going to be   entered 
    // through the legal screens.  If any exist they have to be entered via OACC
    // - this one.
    // 6/23/99 - b adams  -  Add new flow from LDET.  Views are the same as when
    // coming over with the FROMLDET command (PF-18), but the Command is TYPE
    // (PF-19).  This is allowed only for Accruing debts and only if an
    // Obligation_Payment_Schedule End_Date is populated.  We will allow another
    // time frame to be entered, with no overlap.
    // In this situation, either the new obligation being created must be 
    // Interstate, or at least an existing obligation related to this
    // Legal_Action_Detail must be Interstate.
    // ------------------------------
    // 10/05/99, mbrown, pr#75622: Allow the user to enter a discontinue date 
    // equal to the
    // accrual start date.
    // (Change is marked with 'mbrown' and the problem report number.)
    // =================================================
    // 10/15/99, pr#77669, m. brown - Added a limit check for start date.
    // 12/3/99 - b adams - PR# 80027: Accrual suspension flag
    //   moved from Obligation level to supported person level
    //   (ob-tran);
    // 12/7/99 - b adams  -  PR#80027: If Accrual Start Date = the
    //   supported person Disc Date, prevent accruals for that
    //   person by creating an Accrual_Suspension record.
    //       *** Remember Pearl Harbor ***
    // 12/17/99 - b adams  -  PR# 83294: Changing a newly created
    //   obligation from being interstate to NOT being interstate was
    //   not working properly; the FN_Update_Interstate_Rqst_
    //   Oblign was not receiving the system-generated-id of the old
    //   I_R_Obligation record.  Added that attribute to appropriate
    //   views.
    // 1/20/00 - b adams  -  PR# 83301: Add of history obligation
    //   automatically suspends accrual, but indicator on screen
    //   was not being set to Y.
    // 3/1/00 - K. Price - PR #85900: Add country code.  For ADD
    // function, allow the use of either State code or Country code
    // 04/20/2000                Vithal Madhira                PR #85900
    // Fixed the code  for Interstate fields.
    // 07/10/2000                Vithal Madhira                PR# 82441, 98898
    // Fixed the Discontinue date edits for History Obligation.
    // October, 2000, mbrown, pr# 106234: Next Tran fix - added new key fields 
    // to imp and exp next tran, and mapped them to screen.
    // ==================================================================
    // 12/15/2000            Vithal Madhira                  WR# 000253
    // 1. Display Supported Person's DOB on the screen.
    // 2. An automatic flow to PEPR(press PF22)  when adding OACC.
    // ==================================================================
    // =================================================================================
    // 03/14/2001               Vithal Madhira                PR# 115279
    // When Obligation Payment Schedules for Accruing Obligations are end dated(
    // Discontinue Date field on OACC), the Accrual Instructions don't always
    // get end dated (Disc Date field for child on OACC). This problem is being
    // corrected by SRRUNBA2 every night, but it needs to be permanently fixed,
    // and SRRUNBA2 needs to be retired.
    // ===================================================================================
    // =================================================================================
    // 03/14/2002               Maureen Brown               WO# 10504
    // If a discontinue date is changed such that new debts with past due dates 
    // will be created, and AF, NF, NC or FC collections exist, we need to ask
    // the user if they want to protect state retained collections.  Note that
    // this is done on a COURT ORDER level.
    // ===================================================================================
    // =================================================================================
    // 06/22/2006               GVandy              WR# 230751
    // Add capability to select tribal interstate request.
    // ===================================================================================
    // =========================================================================================================================
    // 06/23/2008     Arun Mathias  CQ#5065
    // Do not allow to add accruing obligation if the Accrual start date is not 
    // within the appropriate supported person program.
    // =========================================================================================================================
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    local.SupPersMarkedForUpdate.Flag = "N";

    // : Set hardcoded values
    // 10/15/99, pr#?????, m. brown - Set start date limit to Jan 1, 1960
    local.HardcodeStartDateLimit.Date = new DateTime(1960, 1, 1);
    UseFnHardcodedDebtDistribution();
    UseFnHardcodeLegal();

    // : Move all IMPORTs to EXPORTs.
    export.CaseId.Number = import.CaseId.Number;
    MoveCsePersonsWorkSet4(import.Alternate, export.Alternate);
    export.PayeePrompt.Text1 = import.PayeePrompt.Text1;
    export.AltAddrPrompt.Text1 = import.AltAddrPrompt.Text1;
    MoveCodeValue(import.Country, export.Country);
    export.CountryPrompt.SelectChar = import.CountryPrompt.SelectChar;
    export.InterstateRequest.Assign(import.InterstateRequest);
    export.HiddenInterstateRequest.Assign(import.HiddenInterstateRequest);

    if (IsEmpty(export.InterstateRequest.Country) && IsEmpty
      (export.InterstateRequest.TribalAgency) && !
      IsEmpty(export.Country.Cdvalue))
    {
      export.InterstateRequest.Country =
        Substring(export.Country.Cdvalue, 1, 2);
    }

    export.ObligorCsePerson.Number = import.ObligorCsePerson.Number;
    MoveCsePersonsWorkSet4(import.ObligorCsePersonsWorkSet,
      export.ObligorCsePersonsWorkSet);
    export.ConcurrentObligorCsePerson.Number =
      import.ConcurrentObligorCsePerson.Number;
    MoveLegalAction2(import.LegalAction, export.LegalAction);
    export.LegalActionDetail.Assign(import.LegalActionDetail);
    MoveCsePersonsWorkSet3(import.ConcurrentObligorCsePersonsWorkSet,
      export.ConcurrentObligorCsePersonsWorkSet);
    export.ObligationType.Assign(import.ObligationType);
    export.ObligationPaymentSchedule.Assign(import.ObligationPaymentSchedule);
    export.FrequencyWorkSet.Assign(import.FrequencyWorkSet);
    export.DiscontinueDate.Date = import.DiscontinueDate.Date;
    export.HiddenDiscontinueDate.Date = import.HiddenDiscontinueDate.Date;
    MoveObligation1(import.ObligorObligation, export.Obligation);
    export.Concurrent.SystemGeneratedIdentifier =
      import.ConcurrentObligorObligation.SystemGeneratedIdentifier;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.AccrualAmount.TotalCurrency = import.AccrualAmount.TotalCurrency;
    export.ManualDistributionInd.Flag = import.ManualDistributionInd.Flag;
    export.ObligationActive.Flag = import.ObligationActiveInd.Flag;
    export.ObligorPrompt.Flag = import.ObligorPrompt.Flag;
    export.AccuralSuspendedInd.Flag = import.AccrualSuspendedInd.Flag;
    export.InterestSuspendedInd.Flag = import.InterestSuspendedInd.Flag;
    export.AccrualInstructions.LastAccrualDt =
      import.AccrualInstructions.LastAccrualDt;
    export.BeforeLink.Command = import.BeforeLink.Command;
    MoveCodeValue(import.Country, export.Country);
    export.InterstateDebtExists.Flag = import.InterstateDebtExists.Flag;
    export.ObCollProtAct.Flag = import.ObCollProtAct.Flag;

    // **** hidden imports to hidden exports ****
    export.HiddenAccrualInstructions.DiscontinueDt =
      import.HiddenAccrualInstructions.DiscontinueDt;
    export.HiddenAlternate.Number = import.HiddenAlternate.Number;
    export.HiddenConcurrent.Number = import.HiddenConcurrent.Number;
    export.HiddenCommon.Command = import.HiddenCommon.Command;
    export.HiddenIntstInfo.Assign(import.HiddenIntstInfo);
    export.HiddenInterstateRequest.Assign(import.HiddenInterstateRequest);
    export.HiddenLegalAction.Assign(import.HiddenLegalAction);
    export.HiddenLegalActionDetail.Assign(import.HiddenLegalActionDetail);
    export.HiddenObligationPaymentSchedule.StartDt =
      import.HiddenObligationPaymentSchedule.StartDt;
    export.HiddenObligationType.Assign(import.HiddenObligationType);
    export.HiddenObligorCsePerson.Number = import.HiddenObligorCsePerson.Number;
    MoveObligationPaymentSchedule2(import.HiddenPrevious, export.HiddenPrevious);
      
    export.HiddenProtectHistory.Flag = import.HiddenProtectHistory.Flag;
    export.HiddenFlowToPeprCase.Number = import.HiddenFlowToPeprCase.Number;
    export.HiddenFlowToPeprCsePersonsWorkSet.Number =
      import.HiddenFlowToPeprCsePersonsWorkSet.Number;

    if (Equal(global.Command, "ADD"))
    {
      export.ObligationCreatedDate.Date = local.Current.Date;
    }
    else
    {
      export.ObligationCreatedDate.Date = import.ObligationCreatedDate.Date;
    }

    if (Lt(local.Blank.Date, export.DiscontinueDate.Date))
    {
      export.ObligationPaymentSchedule.EndDt = export.DiscontinueDate.Date;
    }

    // =================================================
    // 6/17/99 - b adams  -  If obligation discontinue date is 0, set
    //   it to MAX date, since that's what it really means.
    // =================================================
    if (Equal(export.DiscontinueDate.Date, local.Blank.Date))
    {
      export.DiscontinueDate.Date = local.Max.Date;
    }

    if (IsEmpty(import.HistoryIndicator.Flag))
    {
      export.HistoryIndicator.Flag = "N";
    }
    else
    {
      MoveCommon2(import.HistoryIndicator, export.HistoryIndicator);
    }

    if (IsEmpty(export.ObCollProtAct.Flag))
    {
      export.ObCollProtAct.Flag = "N";
    }

    // M Brown, April 2002, Retro processing.
    // These fields are used in logic to have the user enter Y or N when an 
    // obligation discontinue date is updated or when a new obligation is added,
    // and AF collections exist on the court order.
    export.ProtectQuestionLiteral.Text80 = "";
    export.CollProtAnswer.SelectChar = "";

    var field = GetField(export.CollProtAnswer, "selectChar");

    field.Intensity = Intensity.Dark;
    field.Protected = true;

    if (Equal(global.Command, "CLEAR"))
    {
      export.ObligationPaymentSchedule.StartDt = local.Blank.Date;
      export.DiscontinueDate.Date = local.Blank.Date;
      export.Obligation.Description = "";

      return;
    }

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      MoveCommon1(import.Group.Item.Sel, export.Group.Update.Sel);
      MoveObligationTransaction1(import.Group.Item.ObligationTransaction,
        export.Group.Update.ObligationTransaction);
      export.Group.Update.ProgramScreenAttributes.ProgramTypeInd =
        import.Group.Item.ProgramScreenAttributes.ProgramTypeInd;
      export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
      export.Group.Update.SupportedCsePerson.Number =
        import.Group.Item.SupportedCsePerson.Number;
      MoveCsePersonsWorkSet1(import.Group.Item.SupportedCsePersonsWorkSet,
        export.Group.Update.SupportedCsePersonsWorkSet);
      export.Group.Update.ServiceProvider.UserId =
        import.Group.Item.ServiceProvider.UserId;
      MoveAccrualInstructions(import.Group.Item.AccrualInstructions,
        export.Group.Update.AccrualInstructions);
      MoveAccrualInstructions(import.Group.Item.Hidden,
        export.Group.Update.Hidden);
      export.Group.Update.AccrualSuspended.Flag =
        import.Group.Item.AccrualSuspended.Flag;
      export.Group.Update.HiddenConcurrent.SystemGeneratedIdentifier =
        import.Group.Item.HiddenConcurrent.SystemGeneratedIdentifier;
      MoveCommon1(import.Group.Item.Sel, export.Group.Update.Sel);
      export.Group.Update.ProratePercentage.Percentage =
        import.Group.Item.ProratePercentage.Percentage;
      export.Group.Update.SuspendAccrual.
        Assign(import.Group.Item.SuspendAccrual);

      switch(AsChar(import.Group.Item.Sel.SelectChar))
      {
        case 'S':
          ++local.NoOfSupportedSelected.Count;

          break;
        case '+':
          break;
        case ' ':
          break;
        default:
          break;
      }

      export.Group.Next();
    }

    // 10/15/99, pr#77669, m. brown - Added a limit check for start date.
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (Lt(export.ObligationPaymentSchedule.StartDt,
        local.HardcodeStartDateLimit.Date) && !
        Equal(export.ObligationPaymentSchedule.StartDt, local.Blank.Date))
      {
        var field1 = GetField(export.ObligationPaymentSchedule, "startDt");

        field1.Error = true;

        ++local.ErrorCount.Count;
        ExitState = "FN0000_START_DATE_PRIOR_TO_LIMIT";
        global.Command = "BYPASS";
      }
    }

    if (AsChar(export.HistoryIndicator.Flag) == 'Y' && Equal
      (global.Command, "ADD"))
    {
      // -------------------------------------------------------------------------------
      // To bypass the SET statement this IF statement is added.
      // -------------------------------------------------------------------------------
    }
    else
    {
      export.HiddenObligationPaymentSchedule.StartDt =
        import.ObligationPaymentSchedule.StartDt;
    }

    // *** If the procedure execution is against a Return from the FIPL screen, 
    // move the selected FIPS codes to the export view attributes.
    // R.B.Mohapatra      01/28/1997
    // ------------------------------
    export.AlternateAddrPrompt.Flag = "+";

    if (Equal(global.Command, "RETFIPL"))
    {
      return;
    }

    if (Equal(global.Command, "RTFRMLNK"))
    {
      global.Command = "DISPLAY";
    }
    else if (Equal(global.Command, "RETNAME"))
    {
      // ***--- if coming back from name list, redisplay screen
      var field1 = GetField(export.Alternate, "number");

      field1.Protected = false;
      field1.Focused = true;

      if (!IsEmpty(import.Flow.Number))
      {
        MoveCsePersonsWorkSet3(import.Flow, export.Alternate);
      }

      // =================================================
      // 06/07/2000  Vithal Madhira  Per SME(Marilyn, the following fields will 
      // be protected/unprotected if the 'History Obligation' is N and
      // obligation was already added on the OACC screen. Check the  CASE ADD
      // for similar code.
      // =================================================
      if (AsChar(export.HistoryIndicator.Flag) == 'N' && !
        Lt(local.Current.Date, export.ObligationCreatedDate.Date) && Lt
        (local.Blank.Date, export.ObligationCreatedDate.Date))
      {
        var field2 = GetField(export.HistoryIndicator, "flag");

        field2.Color = "green";
        field2.Protected = false;

        var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.DiscontinueDate, "date");

        field4.Color = "green";
        field4.Protected = false;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          var field5 =
            GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

          field5.Color = "green";
          field5.Protected = false;
        }
      }

      if (AsChar(export.HistoryIndicator.Flag) == 'Y' && AsChar
        (export.Obligation.HistoryInd) == 'Y')
      {
        var field2 = GetField(export.HistoryIndicator, "flag");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

        field3.Color = "cyan";
        field3.Protected = true;

        if (Lt(local.Blank.Date, export.DiscontinueDate.Date) && export
          .Obligation.SystemGeneratedIdentifier > 0)
        {
          var field4 = GetField(export.DiscontinueDate, "date");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          var field4 =
            GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

          field4.Color = "cyan";
          field4.Protected = true;
        }
      }

      if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
      {
        var field2 = GetField(export.Obligation, "otherStateAbbr");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.InterstateRequest, "country");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.InterstateRequest, "tribalAgency");

        field4.Color = "cyan";
        field4.Protected = true;
      }

      if (!IsEmpty(export.HiddenInterstateRequest.Country))
      {
        var field2 = GetField(export.Obligation, "otherStateAbbr");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.InterstateRequest, "country");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.InterstateRequest, "tribalAgency");

        field4.Color = "cyan";
        field4.Protected = true;
      }

      if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
      {
        var field2 = GetField(export.Obligation, "otherStateAbbr");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.InterstateRequest, "country");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.InterstateRequest, "tribalAgency");

        field4.Color = "cyan";
        field4.Protected = true;
      }

      if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
      {
        var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.Obligation, "orderTypeCode");

        field3.Color = "cyan";
        field3.Protected = true;
      }

      if (Equal(export.DiscontinueDate.Date, local.Max.Date))
      {
        export.DiscontinueDate.Date = local.Blank.Date;
      }

      if (AsChar(export.Obligation.OrderTypeCode) == 'K' && export
        .Obligation.SystemGeneratedIdentifier > 0)
      {
        var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.InterstateRequest, "country");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.InterstateRequest, "tribalAgency");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Obligation, "otherStateAbbr");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.Obligation, "orderTypeCode");

        field6.Color = "cyan";
        field6.Protected = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.ObligorCsePerson.Number =
        export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);

      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
        local.FromHistMonaNxttran.Flag = "Y";
        global.Command = "DISPLAY";

        goto Test1;
      }
      else
      {
        local.FromHistMonaNxttran.Flag = "N";
      }

      ExitState = "FN0000_NO_NEXT_TO_OACC";

      return;
    }

Test1:

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      // October, 2000, mbrown, pr# 106234: Next Tran fix.
      // : If obligor has changed, update the next tran info with the new keys.
      if (Equal(export.ObligorCsePerson.Number,
        export.HiddenObligorCsePerson.Number) && !
        Equal(export.ObligorCsePerson.Number,
        export.HiddenNextTranInfo.CsePersonNumber))
      {
        export.HiddenNextTranInfo.LegalActionIdentifier =
          export.LegalAction.Identifier;
        export.HiddenNextTranInfo.StandardCrtOrdNumber =
          export.LegalAction.StandardNumber ?? "";
        export.HiddenNextTranInfo.CourtCaseNumber =
          export.LegalAction.CourtCaseNumber ?? "";
        export.HiddenNextTranInfo.CsePersonNumberObligor =
          export.ObligorCsePerson.Number;
        export.HiddenNextTranInfo.CsePersonNumber =
          export.ObligorCsePerson.Number;
        export.HiddenNextTranInfo.ObligationId =
          export.Obligation.SystemGeneratedIdentifier;
        export.HiddenNextTranInfo.MiscNum1 = export.LegalActionDetail.Number;
        export.HiddenNextTranInfo.MiscNum2 =
          export.ObligationType.SystemGeneratedIdentifier;
        export.HiddenNextTranInfo.CsePersonNumberObligee = "";
        export.HiddenNextTranInfo.CaseNumber = "";
        export.HiddenNextTranInfo.CourtOrderNumber = "";
        export.HiddenNextTranInfo.InfrastructureId = 0;
      }

      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field1 = GetField(export.Standard, "nextTransaction");

        field1.Error = true;
      }

      global.Command = "BYPASS";
    }

    if (Equal(global.Command, "FROMOCTO") || Equal
      (global.Command, "FROMOPAY") || Equal(global.Command, "FROMDEBT"))
    {
      if (Equal(import.ObligorCsePersonsWorkSet.Char2, "LK"))
      {
        MoveLegalAction1(import.HiddenLegalAction, export.LegalAction);
        export.ObligationType.Assign(import.HiddenObligationType);
        export.Obligation.SystemGeneratedIdentifier =
          import.HiddenObligorObligation.SystemGeneratedIdentifier;
        export.ObligorCsePersonsWorkSet.Char2 = "";
      }

      // <<< RBM  02/23/98  THE DLG. FLOW SENDS THE CMD "DISPLAY" WHEN FLOWS 
      // FROM SWEFDEBT >>>
      local.BeforeLink.Command = "FROMOPAY-ETC";

      // ***  9/8/98 - B Adams    added this so processing at bottom could 
      // happen  [command changed]  ***
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "FROMLDET"))
    {
      export.LegalActionDetail.Assign(import.HiddenLegalActionDetail);

      if (export.LegalActionDetail.Number == 0)
      {
        ExitState = "LEGAL_ACTION_DETAIL_NF";

        return;
      }

      export.HiddenProtectHistory.Flag = "Y";
      global.Command = "DISPLAY";
    }

    // --------------------------------------------------------------------------------------
    // It is decided by SME that 'History Obligation' indicator must be 
    // protected  if user flows to OACC from  LEGAL. We must prevent the user
    // from creating History records if user flows to OACC from LEGAL.
    //                                                             
    // Vithal ( 05-12-2000)
    // ---------------------------------------------------------------------------------------
    if (AsChar(export.HiddenProtectHistory.Flag) == 'Y')
    {
      var field1 = GetField(export.HistoryIndicator, "flag");

      field1.Color = "cyan";
      field1.Protected = true;
    }

    // =================================================
    // 6/23/99 - B Adams  -  User wants to add another obligation to
    //   an existing Legal_Action_Detail; the existing obligation(s)
    //   must have an Obligation_Payment_Schedule End_Date
    // =================================================
    if (Equal(global.Command, "TYPE"))
    {
      export.LegalActionDetail.Assign(import.HiddenLegalActionDetail);
      export.BeforeLink.Command = "TYPE";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
    {
      if (AsChar(import.HiddenNoAction.Flag) == 'Y')
      {
        ExitState = "FN0000_CONVRSN_OBLIG_UPDATE_LEGL";
        global.Command = "BYPASS";

        goto Test2;
      }

      // *****
      // Added Data level security 10/24/96
      // *****
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "BYPASS";
      }
    }

Test2:

    if (Equal(global.Command, "ADD") && AsChar
      (export.HistoryIndicator.Flag) == 'Y')
    {
      if (Equal(export.ObligationPaymentSchedule.StartDt, local.Blank.Date) || !
        Lt(export.ObligationPaymentSchedule.StartDt, local.Current.Date))
      {
        var field1 = GetField(export.ObligationPaymentSchedule, "startDt");

        field1.Error = true;

        ExitState = "FN0000_ST_AND_END_DATE_ZERO";
        global.Command = "BYPASS";

        goto Test3;
      }

      if (Equal(export.DiscontinueDate.Date, local.Blank.Date) || !
        Lt(export.DiscontinueDate.Date, local.Current.Date))
      {
        var field1 = GetField(export.DiscontinueDate, "date");

        field1.Error = true;

        ExitState = "FN0000_ST_AND_END_DATE_ZERO";
        global.Command = "BYPASS";

        goto Test3;
      }

      if (!Lt(export.ObligationPaymentSchedule.StartDt,
        export.HiddenObligationPaymentSchedule.StartDt))
      {
        var field1 = GetField(export.ObligationPaymentSchedule, "startDt");

        field1.Error = true;

        ExitState = "FN0000_HIST_OBLG_START_DT_ERROR";
        global.Command = "BYPASS";

        goto Test3;
      }

      if (!Lt(export.DiscontinueDate.Date,
        export.HiddenObligationPaymentSchedule.StartDt))
      {
        var field1 = GetField(export.ObligationPaymentSchedule, "startDt");

        field1.Error = true;

        ExitState = "FN0000_HIST_OBLG_DISC_DT_ERROR";
        global.Command = "BYPASS";

        goto Test3;
      }

      // *** Start and End -dates are non-zero and prior to Current date.....So 
      // continue with the processing
      export.AccrualInstructions.LastAccrualDt = local.Blank.Date;
      export.AccuralSuspendedInd.Flag = "Y";
      export.InterestSuspendedInd.Flag = "Y";
    }

Test3:

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      if (AsChar(local.FromHistMonaNxttran.Flag) == 'Y')
      {
        goto Test4;
      }

      if (export.LegalAction.Identifier == 0)
      {
        ExitState = "LEGAL_ACTION_NF";

        var field1 = GetField(export.LegalAction, "standardNumber");

        field1.Error = true;

        global.Command = "BYPASS";
      }
    }

Test4:

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE"))
    {
      if (IsEmpty(export.ObligorCsePerson.Number))
      {
        ExitState = "FN0000_CSE_PERSON_UNKNOWN";

        var field1 = GetField(export.ObligorCsePerson, "number");

        field1.Error = true;

        ++local.ErrorCount.Count;
      }

      if (export.ObligationType.SystemGeneratedIdentifier == 0 || IsEmpty
        (export.ObligationType.Code))
      {
        ExitState = "FN0007_OBLIGATION_TYPE_UNKNOWN";

        var field1 = GetField(export.ObligationType, "code");

        field1.Error = true;

        ++local.ErrorCount.Count;
      }

      if (IsEmpty(export.ObligationPaymentSchedule.FrequencyCode))
      {
        ExitState = "FN0000_FREQUENCY_CODE_UNKNOWN";

        var field1 =
          GetField(export.ObligationPaymentSchedule, "frequencyCode");

        field1.Error = true;

        ++local.ErrorCount.Count;
      }

      if (local.ErrorCount.Count == 0)
      {
      }
      else
      {
        if (local.ErrorCount.Count == 1)
        {
        }
        else
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        global.Command = "BYPASS";

        goto Test5;
      }

      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
      {
        if (!IsEmpty(export.Alternate.Number))
        {
          local.TextWorkArea.Text10 = export.Alternate.Number;
          UseEabPadLeftWithZeros();
          export.Alternate.Number = local.TextWorkArea.Text10;
          UseFnCabCheckAltAddr();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            global.Command = "BYPASS";
          }
        }
      }
      else
      {
        // =================================================
        // 10/21/98 - b adams  -  Cannot delete an obligation after the
        //   creation date has passed, because accruals have been run
        //   that night.
        // =================================================
        if (Lt(export.ObligationCreatedDate.Date, local.Current.Date))
        {
          ExitState = "FN0000_CANT_DEL_AFTER_CREAT_DATE";
          global.Command = "BYPASS";
        }
      }
    }

Test5:

    if (AsChar(export.HistoryIndicator.Flag) == 'Y' && export
      .Obligation.SystemGeneratedIdentifier == 0 && Equal
      (global.Command, "UPDATE"))
    {
      ExitState = "FN0000_ADD_HISTORY_OBLIGATION";
      global.Command = "BYPASS";
    }

    // **** Following validations only for add ****
    if (Equal(global.Command, "ADD"))
    {
      // **** day of the month or day of the week is required with the 
      // appropriate freq ****
      if (Equal(export.ObligationPaymentSchedule.FrequencyCode,
        local.HardcodeOpsCMonthly.FrequencyCode) || Equal
        (export.ObligationPaymentSchedule.FrequencyCode,
        local.HardcodeOpsCSemiMonthly.FrequencyCode) || Equal
        (export.ObligationPaymentSchedule.FrequencyCode,
        local.HardcodeOpsCBiMonthly.FrequencyCode))
      {
        if (export.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault() ==
          0)
        {
          var field1 =
            GetField(export.ObligationPaymentSchedule, "dayOfMonth1");

          field1.Error = true;

          ExitState = "FN0000_DAY_OF_MONTH_REQUIRED";
        }
      }

      if (Equal(export.ObligationPaymentSchedule.FrequencyCode,
        local.HardcodeOpsCSemiMonthly.FrequencyCode))
      {
        if (export.ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault() ==
          0)
        {
          var field1 =
            GetField(export.ObligationPaymentSchedule, "dayOfMonth2");

          field1.Error = true;

          ExitState = "FN0000_DAY_OF_MONTH_REQUIRED";
        }
      }

      if (Equal(export.ObligationPaymentSchedule.FrequencyCode,
        local.HardcodeOpsCWeekly.FrequencyCode) || Equal
        (export.ObligationPaymentSchedule.FrequencyCode,
        local.HardcodeOpsCBiWeekly.FrequencyCode))
      {
        if (export.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault() == 0)
        {
          var field1 = GetField(export.ObligationPaymentSchedule, "dayOfWeek");

          field1.Error = true;

          ExitState = "FN0000_DAY_OF_THE_WEEK_REQUIRED";
        }
      }

      // : Start Date can be past, present or future, but is required
      if (Equal(export.ObligationPaymentSchedule.StartDt, local.Blank.Date))
      {
        var field1 = GetField(export.ObligationPaymentSchedule, "startDt");

        field1.Error = true;

        ExitState = "AS_OF_DATE_REQUIRED";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "BYPASS";
      }
    }

    // ** Validation common to ADD and UPDATE functions **
    // <<<----- RBM   01/15/1998   Validation for Interstate Obligations -----
    // >>>
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD"))
    {
      if (IsEmpty(export.Obligation.OrderTypeCode))
      {
        export.Obligation.OrderTypeCode = "K";
      }

      // =================================================
      // 3/1/00  K. Price - process country or state code for ADD
      // and UPDATE
      // =================================================
      switch(AsChar(export.Obligation.OrderTypeCode))
      {
        case 'K':
          if (!IsEmpty(export.InterstateRequest.OtherStateCaseId))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Error = true;

            ExitState = "INVALID_VALUE";
          }

          if (!IsEmpty(export.Obligation.OtherStateAbbr))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Error = true;

            ExitState = "INVALID_VALUE";
          }

          if (!IsEmpty(export.InterstateRequest.Country))
          {
            var field2 = GetField(export.InterstateRequest, "country");

            field2.Error = true;

            ExitState = "INVALID_VALUE";
          }

          if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            var field2 = GetField(export.InterstateRequest, "tribalAgency");

            field2.Error = true;

            ExitState = "INVALID_VALUE";
          }

          break;
        case 'I':
          if (IsEmpty(export.InterstateRequest.OtherStateCaseId))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Error = true;

            ExitState = "FN0000_MANDATORY_FIELDS";
          }

          if (IsEmpty(export.Obligation.OtherStateAbbr) && IsEmpty
            (export.InterstateRequest.Country) && IsEmpty
            (export.InterstateRequest.TribalAgency))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Error = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Error = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Error = true;

            ExitState = "FN0000_IVD_AGENCY_REQUIRED";
          }

          local.IvdAgency.Count = 0;

          if (!IsEmpty(export.Obligation.OtherStateAbbr))
          {
            ++local.IvdAgency.Count;
          }

          if (!IsEmpty(export.InterstateRequest.Country))
          {
            ++local.IvdAgency.Count;
          }

          if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            ++local.IvdAgency.Count;
          }

          if (local.IvdAgency.Count > 1)
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Error = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Error = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Error = true;

            ExitState = "FN0000_IVD_AGENCY_REQUIRED";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test6;
          }

          UseFnRetrieveInterstateRequest1();

          if (IsExitState("FN0000_FIPS_FOR_THE_STATE_NF"))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Error = true;
          }
          else if (IsExitState("INTERSTATE_REQUEST_NF"))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Error = true;
          }
          else if (IsExitState("LE0000_INVALID_COUNTRY_CODE"))
          {
            var field2 = GetField(export.InterstateRequest, "country");

            field2.Error = true;
          }
          else if (IsExitState("ACO_NE0000_INVALID_STATE_CODE"))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Error = true;
          }
          else if (IsExitState("FN0000_INVALID_COUNTRY_INTERSTAT"))
          {
            var field2 = GetField(export.InterstateRequest, "country");

            field2.Error = true;
          }
          else if (IsExitState("FN0000_INVALID_STATE_INTERSTATE"))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Error = true;
          }
          else if (IsExitState("FN0000_INTERSTATE_AP_MISMATCH"))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Error = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Error = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Error = true;

            var field5 = GetField(export.Obligation, "otherStateAbbr");

            field5.Error = true;
          }
          else if (IsExitState("FN0000_INVALID_TRIBAL_AGENCY"))
          {
            var field2 = GetField(export.InterstateRequest, "tribalAgency");

            field2.Error = true;
          }
          else if (IsExitState("FN0000_INVALID_TRIBAL_INTERSTAT"))
          {
            var field2 = GetField(export.InterstateRequest, "tribalAgency");

            field2.Error = true;
          }
          else
          {
          }

          break;
        case ' ':
          export.Obligation.OrderTypeCode = "K";
          export.InterstateRequest.Country = "";
          export.InterstateRequest.OtherStateCaseId = "";
          export.Obligation.OtherStateAbbr = "";

          break;
        default:
          var field1 = GetField(export.Obligation, "orderTypeCode");

          field1.Error = true;

          ExitState = "FN0000_INVALID_INTERSTATE_IND";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "BYPASS";

        goto Test6;
      }

      if (Equal(global.Command, "UPDATE"))
      {
        switch(AsChar(export.Obligation.OrderTypeCode))
        {
          case 'K':
            if (AsChar(export.Obligation.OrderTypeCode) == AsChar
              (export.HiddenIntstInfo.OrderTypeCode) && Equal
              (export.Obligation.OtherStateAbbr,
              export.HiddenIntstInfo.OtherStateAbbr) && Equal
              (export.InterstateRequest.OtherStateCaseId,
              export.HiddenInterstateRequest.OtherStateCaseId) && Equal
              (export.InterstateRequest.Country,
              export.HiddenInterstateRequest.Country) && Equal
              (export.InterstateRequest.TribalAgency,
              export.HiddenInterstateRequest.TribalAgency))
            {
              // <<< The Interstate info has not changed >>>
              // <<< Initialize the local--- view which  >>>
              // <<< is passed to the UPDATE Cab.        >>>
              local.UpdateInterstateInfo.Flag = "N";
            }
            else if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'I' && Lt
              (export.ObligationCreatedDate.Date, local.Current.Date))
            {
              // Trying to change an Interstate Obligation to Non-interstate 
              // Obligation
              // after accruals have been run.
              var field2 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field2.Error = true;

              var field3 = GetField(export.Obligation, "otherStateAbbr");

              field3.Error = true;

              var field4 = GetField(export.Obligation, "orderTypeCode");

              field4.Error = true;

              ExitState = "FN0000_CHANGE_TO_NON_INTST_NA";
              global.Command = "BYPASS";

              goto Test6;
            }
            else
            {
              // <<< The Interstate info has     changed >>>
              // <<< Initialize the local--- view which  >>>
              // <<< is passed to the UPDATE Cab.        >>>
              local.UpdateInterstateInfo.Flag = "Y";
            }

            break;
          case 'I':
            if (AsChar(export.Obligation.OrderTypeCode) == AsChar
              (export.HiddenIntstInfo.OrderTypeCode) && Equal
              (export.InterstateRequest.OtherStateCaseId,
              export.HiddenInterstateRequest.OtherStateCaseId) && Equal
              (export.Obligation.OtherStateAbbr,
              export.HiddenIntstInfo.OtherStateAbbr) && Equal
              (export.InterstateRequest.Country,
              export.HiddenInterstateRequest.Country) && Equal
              (export.InterstateRequest.TribalAgency,
              export.HiddenInterstateRequest.TribalAgency))
            {
              // <<< The Interstate country info has not changed >>>
              // <<< Initialize the local--- view which  >>>
              // <<< is passed to the UPDATE Cab.        >>>
              local.UpdateInterstateInfo.Flag = "N";
            }
            else if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'K' && Lt
              (export.ObligationCreatedDate.Date, local.Current.Date))
            {
              // Trying to change an Inter-Country Obligation
              //  to Non-interstate Obligation after accruals
              //  have been run.
              var field2 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field2.Error = true;

              var field3 = GetField(export.InterstateRequest, "country");

              field3.Error = true;

              var field4 = GetField(export.Obligation, "orderTypeCode");

              field4.Error = true;

              ExitState = "FN0000_CHANGE_TO_NON_INTST_NA";
              global.Command = "BYPASS";

              goto Test6;
            }
            else
            {
              // <<< The Interstate info has     changed >>>
              // <<< Initialize the local--- view which  >>>
              // <<< is passed to the UPDATE Cab.        >>>
              local.UpdateInterstateInfo.Flag = "Y";
            }

            break;
          default:
            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Error = true;

            local.UpdateInterstateInfo.Flag = "N";
            ExitState = "FN0000_CHANGE_TO_NON_INTST_NA";
            global.Command = "BYPASS";

            goto Test6;
        }
      }

      // <<<----- End of Validation for Interstate Obligation ----->>>
      // **** Discontinue date if present, must be greater than start date and 
      // last accrual date ****
      if (Equal(export.DiscontinueDate.Date, local.Max.Date))
      {
        local.ObligationPaymentSchedule.EndDt = local.Max.Date;
        export.ObligationPaymentSchedule.EndDt = local.Max.Date;
      }
      else
      {
        local.ObligationPaymentSchedule.EndDt =
          export.ObligationPaymentSchedule.EndDt;
      }

      // 10/05/99, mbrown, pr#75622: Changed the following 'if' statement from '
      // less than or equal to' to 'less than'.
      if (Lt(local.ObligationPaymentSchedule.EndDt,
        export.ObligationPaymentSchedule.StartDt))
      {
        var field1 = GetField(export.DiscontinueDate, "date");

        field1.Error = true;

        ExitState = "DISC_DATE_CANNOT_LT_EFF_DATE";
        global.Command = "BYPASS";

        goto Test6;
      }

      if (Equal(global.Command, "UPDATE"))
      {
        if (Lt(local.ObligationPaymentSchedule.EndDt,
          export.AccrualInstructions.LastAccrualDt))
        {
          var field1 = GetField(export.DiscontinueDate, "date");

          field1.Error = true;

          ExitState = "DICONT_DATE_LT_LAST_ACCR_DATE";
          global.Command = "BYPASS";

          goto Test6;
        }
      }

      // : EDIT LIST PORTION OF SCREEN for ADD and UPDATE commands.
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        // : Validate action character.
        switch(AsChar(export.Group.Item.Sel.SelectChar))
        {
          case ' ':
            if (Equal(global.Command, "ADD") && !
              IsEmpty(export.Group.Item.SupportedCsePerson.Number))
            {
              export.Group.Update.Sel.SelectChar = "S";
            }

            // ---------------------------------------------------------------------------
            // PR#  82441    Vithal Madhira             0620/2000
            //    Obligation and supported person 'Discontinue Date' edits will 
            // be fixed by this code.
            // --------------------------------------------------------------------------
            if (Equal(global.Command, "UPDATE") && !
              IsEmpty(export.Group.Item.SupportedCsePerson.Number))
            {
              export.Group.Update.Sel.SelectChar = "U";
              export.Group.Update.Sel.Flag = "U";
            }

            break;
          case 'S':
            if (!IsEmpty(export.Group.Item.SupportedCsePerson.Number))
            {
              // *** Valid selection ****
            }
            else
            {
              var field2 = GetField(export.Group.Item.Sel, "selectChar");

              field2.Error = true;

              global.Command = "BYPASS";

              goto Test6;
            }

            if (Equal(global.Command, "UPDATE"))
            {
              ++local.SelCount.Count;
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field1 = GetField(export.Group.Item.Sel, "selectChar");

            field1.Error = true;

            global.Command = "BYPASS";

            goto Test6;
        }

        if (!IsEmpty(export.Group.Item.SupportedCsePerson.Number))
        {
          if (AsChar(export.Group.Item.Sel.SelectChar) == 'S')
          {
            // *** CQ#5065 Changes Begin Here ***
            if (Equal(global.Command, "ADD"))
            {
              local.InterstatePgmFound.Flag = "N";

              if (ReadProgram2())
              {
                local.InterstatePgmFound.Flag = "Y";
              }

              local.NonInterstatePgmFound.Flag = "N";

              if (ReadProgram1())
              {
                local.NonInterstatePgmFound.Flag = "Y";
              }

              if (AsChar(export.Obligation.OrderTypeCode) == 'I')
              {
                if (AsChar(local.InterstatePgmFound.Flag) == 'N')
                {
                  if (AsChar(local.NonInterstatePgmFound.Flag) == 'N')
                  {
                    ExitState = "FN0000_INTST_PGM_NOT_FND_ON_PEPR";
                  }
                  else if (AsChar(local.NonInterstatePgmFound.Flag) == 'Y')
                  {
                    ExitState = "FN0000_NON_INTST_PGM_FND_ON_PEPR";
                  }

                  global.Command = "BYPASS";

                  goto Test6;
                }
              }
              else
              {
                if (AsChar(local.NonInterstatePgmFound.Flag) == 'N')
                {
                  if (AsChar(local.InterstatePgmFound.Flag) == 'N')
                  {
                    ExitState = "FN0000_NON_INTST_PGM_NT_FND_PEPR";
                  }
                  else if (AsChar(local.InterstatePgmFound.Flag) == 'Y')
                  {
                    ExitState = "FN0000_INTST_PGM_FND_ON_PEPR";
                  }

                  global.Command = "BYPASS";

                  goto Test6;
                }
              }
            }

            // *** CQ#5065 Changes End   Here ***
            // <<< If the attempt is to ADD a History obligation, Set the 
            // discontinue date for all supported persons to the Discontinue
            // date of the Obligation. >>>
            if (AsChar(export.HistoryIndicator.Flag) == 'Y')
            {
              if (Equal(global.Command, "ADD"))
              {
                if (Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
                  local.Blank.Date))
                {
                  // ---------------------------------------------------------------------------
                  // SET the  Discontinue date for Supported person to 
                  // Obligation Discontinue Date'.. Continue processing
                  // ...........
                  // ----------------------------------------------------------------------
                  export.Group.Update.AccrualInstructions.DiscontinueDt =
                    export.DiscontinueDate.Date;
                }

                if (!Lt(export.DiscontinueDate.Date,
                  export.Group.Item.AccrualInstructions.DiscontinueDt) && Lt
                  (local.Blank.Date,
                  export.Group.Item.AccrualInstructions.DiscontinueDt))
                {
                  // ---------------------------------------------------------------------------
                  // Use the same date as Discontinue date for Supported person.
                  // Continue processing...........
                  // ----------------------------------------------------------------------
                }

                if (Lt(export.Group.Item.AccrualInstructions.DiscontinueDt,
                  export.ObligationPaymentSchedule.StartDt) || Lt
                  (export.DiscontinueDate.Date,
                  export.Group.Item.AccrualInstructions.DiscontinueDt))
                {
                  var field1 =
                    GetField(export.Group.Item.AccrualInstructions,
                    "discontinueDt");

                  field1.Error = true;

                  var field2 = GetField(export.DiscontinueDate, "date");

                  field2.Error = true;

                  var field3 =
                    GetField(export.ObligationPaymentSchedule, "startDt");

                  field3.Error = true;

                  ExitState = "FN0000_SUPP_PERSON_DISC_DT_ERROR";
                  global.Command = "BYPASS";

                  goto Test6;
                }
              }

              if (Equal(global.Command, "UPDATE"))
              {
                export.Group.Update.AccrualInstructions.DiscontinueDt =
                  export.DiscontinueDate.Date;
              }
            }
            else if (!Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
              local.Blank.Date))
            {
              if (Lt(export.Group.Item.AccrualInstructions.DiscontinueDt,
                export.ObligationPaymentSchedule.StartDt))
              {
                // : Discontinue Date must be greater than the Start Date.
                ExitState = "DISC_DATE_CANNOT_LT_EFF_DATE";

                var field1 =
                  GetField(export.Group.Item.AccrualInstructions,
                  "discontinueDt");

                field1.Error = true;

                global.Command = "BYPASS";

                goto Test6;
              }

              if (Equal(global.Command, "UPDATE"))
              {
                if (Lt(export.Group.Item.AccrualInstructions.DiscontinueDt,
                  export.Group.Item.AccrualInstructions.LastAccrualDt))
                {
                  var field1 =
                    GetField(export.Group.Item.AccrualInstructions,
                    "discontinueDt");

                  field1.Error = true;

                  ExitState = "DICONT_DATE_LT_LAST_ACCR_DATE";
                  global.Command = "BYPASS";

                  goto Test6;
                }
              }

              if (!Equal(export.DiscontinueDate.Date, local.Blank.Date))
              {
                if (Lt(export.DiscontinueDate.Date,
                  export.Group.Item.AccrualInstructions.DiscontinueDt))
                {
                  var field1 =
                    GetField(export.Group.Item.AccrualInstructions,
                    "discontinueDt");

                  field1.Error = true;

                  ExitState = "FN0000_ACCRUAL_INST_OBLIG";
                  global.Command = "BYPASS";

                  goto Test6;
                }
              }
            }
            else
            {
              // **** If pymnt schedule discontinue dt is entered and accrual 
              // instr disc date
              //     is blank for individual supported persons populate the 
              // latter from the pymnt
              //     sch discont dt ****
              if (!Equal(export.DiscontinueDate.Date, local.Blank.Date))
              {
                export.Group.Update.AccrualInstructions.DiscontinueDt =
                  export.DiscontinueDate.Date;
              }
            }

            // ***---  end of IF select char = 'S'
          }
          else
          {
            // *------------------------------------------------------------------------------------
            // 04/11/1997   RBM  ATTENTION....ATTENTION....ATTENTION
            //  To whosoever it may concern....PLEASE DO NOT DELETE THE 
            // FOLLOWING SET STATEMENTS
            //  as they enable the CAB FN_UPDATE_ACCRUING_OBLIGATION to UPDATE 
            // all the Supported
            //  Persons if the OBLIGATION is DISCONTINUED.
            // *------------------------------------------------------------------------------------
            // =================================================
            // 1/26/99 - b adams  -  If the obligation discontinue date was
            //   changed from some date to a blank date, then the supported
            //   persons' discontinued dates must also be changed to blank.
            // 6/6/99 - b adams  -  If the obligation discontinue date is not
            //   the same as the supported person's discontinue date, then
            //   do NOT change the supported person's date.
            // =================================================
            if (Equal(global.Command, "UPDATE"))
            {
              if (AsChar(export.Group.Item.Sel.Flag) == 'U')
              {
                // ***---  if the new debt discontinue date < the supported 
                // person discontinue date
                // ***---  then reset the supported person's discontinue date to
                // be the new debt date.
                // ***---
                // ***---  if the old debt discontinue date = the supported 
                // person discontinue date
                // ***---  then reset the supported person's discontinue date to
                // be the new debt date.
                // : Aug 13,1999, mfb - The commented IF below is the 'before' 
                // version.
                // It has been changed to remove 'and export_grp 
                // accrual_instructions discontinue_dt
                // is not equal to local_blank date_work_area date'.
                // This was done because the accrual date was not being updated 
                // when blank,
                // and it should have been.
                if (!Equal(export.DiscontinueDate.Date,
                  export.HiddenDiscontinueDate.Date) && (
                    Equal(export.HiddenDiscontinueDate.Date,
                  export.Group.Item.AccrualInstructions.DiscontinueDt) || Equal
                  (export.Group.Item.AccrualInstructions.DiscontinueDt,
                  local.Blank.Date) || Lt
                  (export.DiscontinueDate.Date,
                  export.Group.Item.AccrualInstructions.DiscontinueDt)))
                {
                  export.Group.Update.Sel.SelectChar = "S";
                  export.Group.Update.AccrualInstructions.DiscontinueDt =
                    export.DiscontinueDate.Date;
                  local.SupPersMarkedForUpdate.Flag = "Y";
                }
                else if (!Equal(export.Group.Item.AccrualInstructions.
                  DiscontinueDt, export.Group.Item.Hidden.DiscontinueDt) && Equal
                  (export.Group.Item.AccrualInstructions.DiscontinueDt,
                  export.DiscontinueDate.Date))
                {
                  export.Group.Update.Sel.SelectChar = "S";
                  export.Group.Update.AccrualInstructions.DiscontinueDt =
                    export.DiscontinueDate.Date;
                  local.SupPersMarkedForUpdate.Flag = "Y";
                }

                if (Equal(export.DiscontinueDate.Date,
                  export.HiddenDiscontinueDate.Date) && !
                  Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
                  export.Group.Item.Hidden.DiscontinueDt))
                {
                  if (AsChar(export.Group.Item.Sel.SelectChar) != 'S')
                  {
                    ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

                    var field1 =
                      GetField(export.Group.Item.AccrualInstructions,
                      "discontinueDt");

                    field1.Error = true;

                    var field2 = GetField(export.Group.Item.Sel, "selectChar");

                    field2.Error = true;
                  }
                }
              }
            }
          }
        }

        // =================================================
        // 12/7/99 - bud adams  -  PR# 80027: If Accrual Start Date =
        //   supported person Discontinue Date then suspend accrual.
        //   Remember Pearl Harbor.
        //   (Check to see if this has already been done and if accruals
        //   have already been run - if created date is in the past.)
        // =================================================
        if (Equal(export.ObligationPaymentSchedule.StartDt,
          export.Group.Item.AccrualInstructions.DiscontinueDt) && !
          Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
          export.HiddenAccrualInstructions.DiscontinueDt) && Equal
          (export.ObligationCreatedDate.Date, local.Current.Date) && AsChar
          (export.Group.Item.AccrualSuspended.Flag) != 'Y')
        {
          export.Group.Update.SuspendAccrual.Flag = "Y";
          local.SuspendAccrual.Flag = "Y";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "BYPASS";
      }
    }

Test6:

    if (AsChar(export.HistoryIndicator.Flag) == 'Y' && export
      .Obligation.SystemGeneratedIdentifier == 0 && !
      Equal(global.Command, "ADD"))
    {
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test7;
      }

      if (Equal(global.Command, "EXIT") || Equal(global.Command, "RETURN") || Equal
        (global.Command, "SIGNOFF") || Equal(global.Command, "LIST"))
      {
        goto Test7;
      }

      // -----------------------------------------------------------------------------
      // User wants to bulid the History Obligation and supposed to press PF5 ( 
      // ADD). This is the valid action now. Else user must exit the screen by
      // pressing PF9.
      // ---------------------------------------------------------------------------------
      ExitState = "ACO_NE0000_ONLY_ADD_IS_VALID";

      var field1 = GetField(export.HistoryIndicator, "flag");

      field1.Color = "cyan";
      field1.Protected = true;

      global.Command = "BYPASS";
    }

Test7:

    // ** Mainline **
    switch(TrimEnd(global.Command))
    {
      case "REIP":
        ExitState = "ECO_LNK_TO_REC_IND_PYMNT_HIST";

        break;
      case "RERE":
        ExitState = "ECO_LNK_TO_REC_RCRNG_PYMNT_HIST";

        break;
      case "DISPLAY":
        local.Common.Command = "DISPLAY";
        local.ExitState.Flag = "";
        local.CheckObligationExistence.Flag = "N";

        // =================================================
        // 4/15/99 - B Adams  -  added test for obligation_type in IF.
        //   Hope you filed your income taxes!
        // =================================================
        if (IsEmpty(export.ObligorCsePerson.Number) || export
          .ObligationType.SystemGeneratedIdentifier == 0)
        {
          if (AsChar(local.FromHistMonaNxttran.Flag) == 'Y')
          {
            UseFnGetOblFromHistMonaNxtran();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }
          }

          UseFnCheckExistenceOfObligation1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }
        else
        {
          local.CheckObligationExistence.Flag = "Y";
        }

        // ***---  if this is flowing from LDET with Command of TYPE the Flag 
        // should be = "Y"
        if (AsChar(local.CheckObligationExistence.Flag) == 'Y')
        {
          if (IsEmpty(export.ObligorCsePerson.Number))
          {
            export.ObligorCsePerson.Number = local.CsePerson.Number;
          }

          if (export.ObligationType.SystemGeneratedIdentifier == 0)
          {
            export.ObligationType.Assign(local.ObligationType);
          }

          if (export.Obligation.SystemGeneratedIdentifier == 0)
          {
            export.Obligation.SystemGeneratedIdentifier =
              local.Obligation.SystemGeneratedIdentifier;
          }

          UseFnReadAccruingObligation();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          ExitState = "OBLIGATION_SUCCESSFULLY_DISPLAY";
          local.ExitState.Flag = "N";
          export.HiddenObligationPaymentSchedule.StartDt =
            export.ObligationPaymentSchedule.StartDt;
          MoveObligationPaymentSchedule2(export.ObligationPaymentSchedule,
            export.HiddenPrevious);

          // =================================================
          // 6/25/99 - B Adams  -  If this is a new interstate timeframe,
          //   then the start date of the new timeframe cannot be earlier
          //   than the end date of the previous one.
          // =================================================
          if (Equal(export.BeforeLink.Command, "TYPE"))
          {
            export.ObligationPaymentSchedule.StartDt =
              AddDays(export.ObligationPaymentSchedule.EndDt, 1);
            export.ObligationPaymentSchedule.EndDt = local.Blank.Date;
            export.DiscontinueDate.Date = local.Blank.Date;
            export.Obligation.SystemGeneratedIdentifier = 0;
            export.Obligation.LastUpdateTmst = local.Blank.Timestamp;
            export.Obligation.LastUpdatedBy = "";
            export.Obligation.CreatedTmst = local.Blank.Timestamp;
            export.Obligation.CreatedBy = "";
            ExitState = "FN0000_OB_NF_FOR_LA_PF5_TO_ADD";
            local.ExitState.Flag = "Y";
          }

          if (Equal(export.HiddenObligorCsePerson.Number,
            export.ObligorCsePerson.Number))
          {
            // =================================================
            // 11/24/98 - Bud Adams  -  If 'export_hidden' = 'export' then it
            //   may be that the user has changed the History Indicator.  If
            //   so, we don't want to write over it with the Obligation value.
            // =================================================
          }
          else
          {
            export.HistoryIndicator.Flag = export.Obligation.HistoryInd ?? Spaces
              (1);
          }

          if (IsEmpty(export.HistoryIndicator.Flag))
          {
            export.HistoryIndicator.Flag = "N";
          }

          // =================================================
          // 11/24/98 - B Adams  -  If this is an historical debt, then the Y
          //   remains protected.  If it is not, then the user may create a
          //   new historical obligation with the same demographics and
          //   the same legal-action-detail, but the Accrual Start and End
          //   Date ranges must have no overlap with existing ones.
          // =================================================
          if (AsChar(export.Obligation.HistoryInd) == 'Y')
          {
            var field2 = GetField(export.HistoryIndicator, "flag");

            field2.Color = "cyan";
            field2.Protected = true;

            // :08/10/99, mfb - Protect the discontinue date if this is a 
            // historical debt.
            var field3 = GetField(export.DiscontinueDate, "date");

            field3.Color = "cyan";
            field3.Protected = true;
          }
        }
        else
        {
          UseFnReadLegalDetailForOblig();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "FN0000_OB_NF_FOR_LA_PF5_TO_ADD";
          }
          else
          {
            break;
          }
        }

        if (ReadObligCollProtectionHist())
        {
          export.ObCollProtAct.Flag = "Y";
        }
        else
        {
          export.ObCollProtAct.Flag = "N";
        }

        switch(AsChar(export.Obligation.OrderTypeCode))
        {
          case 'K':
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.Obligation, "otherStateAbbr");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.Obligation, "orderTypeCode");

            field6.Color = "cyan";
            field6.Protected = true;

            break;
          case 'I':
            UseFnRetrieveInterstateRequest1();

            if (IsExitState("FN0000_FIPS_FOR_THE_STATE_NF"))
            {
              var field8 = GetField(export.Obligation, "otherStateAbbr");

              field8.Error = true;
            }
            else if (IsExitState("INTERSTATE_REQUEST_NF"))
            {
              var field8 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field8.Error = true;
            }
            else if (IsExitState("LE0000_INVALID_COUNTRY_CODE"))
            {
              var field8 = GetField(export.InterstateRequest, "country");

              field8.Error = true;
            }
            else if (IsExitState("ACO_NE0000_INVALID_STATE_CODE"))
            {
              var field8 = GetField(export.Obligation, "otherStateAbbr");

              field8.Error = true;
            }
            else if (IsExitState("FN0000_INVALID_COUNTRY_INTERSTAT"))
            {
              var field8 = GetField(export.InterstateRequest, "country");

              field8.Error = true;
            }
            else if (IsExitState("FN0000_INVALID_STATE_INTERSTATE"))
            {
              var field8 = GetField(export.Obligation, "otherStateAbbr");

              field8.Error = true;
            }
            else if (IsExitState("FN0000_INVALID_TRIBAL_AGENCY"))
            {
              var field8 = GetField(export.InterstateRequest, "tribalAgency");

              field8.Error = true;
            }
            else if (IsExitState("FN0000_INVALID_TRIBAL_INTERSTAT"))
            {
              var field8 = GetField(export.InterstateRequest, "tribalAgency");

              field8.Error = true;
            }
            else if (IsExitState("FN0000_INTERSTATE_AP_MISMATCH"))
            {
              // =================================================
              // 7/1/99 - bud adams  -  An Interstate_Request can only be
              //   tied to one KESSEP Case.  But, a Joint & Several Obligation
              //   consists of two cases, and if that case is not the one this
              //   obligor is tied to, then it's the one the concurrent obligor
              //   is tied to.
              // =================================================
              if (AsChar(export.Obligation.PrimarySecondaryCode) == AsChar
                (local.HardcodeObligJointSevlConcur.PrimarySecondaryCode))
              {
                ExitState = "ACO_NN0000_ALL_OK";
                UseFnRetrieveInterstateRequest2();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  if (AsChar(local.ExitState.Flag) == 'N')
                  {
                    ExitState = "OBLIGATION_SUCCESSFULLY_DISPLAY";
                  }
                  else if (AsChar(local.ExitState.Flag) == 'Y')
                  {
                    ExitState = "FN0000_OB_NF_FOR_LA_PF5_TO_ADD";
                  }
                }
                else if (IsExitState("FN0000_FIPS_FOR_THE_STATE_NF"))
                {
                  var field8 = GetField(export.Obligation, "otherStateAbbr");

                  field8.Error = true;

                  var field9 =
                    GetField(export.InterstateRequest, "otherStateCaseId");

                  field9.Error = true;
                }
                else if (IsExitState("INTERSTATE_REQUEST_NF"))
                {
                  var field8 =
                    GetField(export.InterstateRequest, "otherStateCaseId");

                  field8.Error = true;
                }
                else if (IsExitState("LE0000_INVALID_COUNTRY_CODE"))
                {
                  var field8 = GetField(export.InterstateRequest, "country");

                  field8.Error = true;
                }
                else if (IsExitState("ACO_NE0000_INVALID_STATE_CODE"))
                {
                  var field8 = GetField(export.Obligation, "otherStateAbbr");

                  field8.Error = true;
                }
                else if (IsExitState("FN0000_INVALID_COUNTRY_INTERSTAT"))
                {
                  var field8 = GetField(export.InterstateRequest, "country");

                  field8.Error = true;
                }
                else if (IsExitState("FN0000_INVALID_STATE_INTERSTATE"))
                {
                  var field8 = GetField(export.Obligation, "otherStateAbbr");

                  field8.Error = true;
                }
                else if (IsExitState("FN0000_INTERSTATE_AP_MISMATCH"))
                {
                  var field8 =
                    GetField(export.InterstateRequest, "otherStateCaseId");

                  field8.Error = true;

                  var field9 = GetField(export.InterstateRequest, "country");

                  field9.Error = true;

                  var field10 =
                    GetField(export.InterstateRequest, "tribalAgency");

                  field10.Error = true;

                  var field11 = GetField(export.Obligation, "otherStateAbbr");

                  field11.Error = true;
                }
                else if (IsExitState("FN0000_INVALID_TRIBAL_AGENCY"))
                {
                  var field8 =
                    GetField(export.InterstateRequest, "tribalAgency");

                  field8.Error = true;
                }
                else if (IsExitState("FN0000_INVALID_TRIBAL_INTERSTAT"))
                {
                  var field8 =
                    GetField(export.InterstateRequest, "tribalAgency");

                  field8.Error = true;
                }
                else
                {
                }
              }
              else
              {
                var field8 = GetField(export.Obligation, "otherStateAbbr");

                field8.Error = true;

                var field9 = GetField(export.InterstateRequest, "country");

                field9.Error = true;

                var field10 =
                  GetField(export.InterstateRequest, "tribalAgency");

                field10.Error = true;

                var field11 =
                  GetField(export.InterstateRequest, "otherStateCaseId");

                field11.Error = true;
              }
            }
            else
            {
            }

            break;
          case ' ':
            export.Obligation.OrderTypeCode = "K";
            export.InterstateRequest.Country = "";
            export.InterstateRequest.TribalAgency = "";
            export.InterstateRequest.OtherStateCaseId = "";
            export.Obligation.OtherStateAbbr = "";

            break;
          default:
            var field7 = GetField(export.Obligation, "orderTypeCode");

            field7.Error = true;

            break;
        }

        if (IsExitState("OBLIGATION_SUCCESSFULLY_DISPLAY"))
        {
          export.HiddenCommon.Command = "DISPLAY";
        }

        export.HiddenAlternate.Number = export.Alternate.Number;
        export.ObligationCreatedDate.Date = Date(export.Obligation.CreatedTmst);

        // =================================================
        // 10/23/98 - B Adams  -  Other state case ID cannot be SPACES
        //   if it is an Interstate Case, so make it unprotected.
        // =================================================
        // =================================================
        // 11/20/98 - B Adams  -  The accrual start date should always
        //   be unprotected until the day after the Obligation is created.
        //   Then, Debt_Details would have already been created by
        //   the accrual process.
        // ( The above STATEMENT  is not true anymore from 06/07/2000 per 
        // Marilyn Gaperich. She wants always to protect the 'Accrual Start
        // Date' after adding the Obligation. The code will be modified. The
        // business rules will be updated by SME accordingly.
        //                                              
        // ------> Vithal Madhira(06/07/2000).
        //   BUT, if the History-Indicator has been set to Y by the user,
        //   then the Start-Date should be left unprotected.  The only
        //   thing the user is allowed to do to is Add.  No Updating is
        //   allowed.
        //   Just in case the user had changed to Y and does a Display
        //   again, reset the history-indicator command.  This is used to
        //   communicate to the next pass that the user intends to Add
        //   historical time frames - and that Update is not valid.
        // =================================================
        export.HistoryIndicator.Command = "";

        if (Equal(export.ObligationCreatedDate.Date, local.Blank.Date))
        {
        }
        else if (Equal(export.ObligationCreatedDate.Date, local.Current.Date) ||
          AsChar(export.HistoryIndicator.Flag) == 'Y')
        {
          // : Aug 6, 1999, mfb - added "AND export obligation history_ind
          //  is not = 'Y' ", to unprotect only when the user has changed the
          //  history indicator for an obligation that is not a history 
          // obligation.
          if (AsChar(export.HistoryIndicator.Flag) == 'Y' && AsChar
            (export.Obligation.HistoryInd) != 'Y')
          {
            export.HistoryIndicator.Command = global.Command;
          }
        }

        // =================================================
        // 06/07/2000  Vithal Madhira  Per SME(Marilyn, the following fields 
        // will be protected/unprotected if the 'History Obligation' is N and
        // obligation was already added on the OACC screen. Check the  CASE ADD
        // for similar code.
        // =================================================
        if (AsChar(export.HistoryIndicator.Flag) == 'N' && !
          Lt(local.Current.Date, export.ObligationCreatedDate.Date) && Lt
          (local.Blank.Date, export.ObligationCreatedDate.Date))
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "green";
          field2.Protected = false;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DiscontinueDate, "date");

          field4.Color = "green";
          field4.Protected = false;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field5 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field5.Color = "green";
            field5.Protected = false;
          }
        }

        if (Equal(export.ObligationCreatedDate.Date, local.Current.Date) || Equal
          (export.ObligationCreatedDate.Date, local.Blank.Date) || Equal
          (export.BeforeLink.Command, "TYPE"))
        {
        }
        else
        {
          var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "country");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "tribalAgency");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.Obligation, "otherStateAbbr");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.Obligation, "orderTypeCode");

          field6.Color = "cyan";
          field6.Protected = true;
        }

        // =================================================
        // PR# 235: 8/25/99 - bud adams -  when creating a new debt,
        //   always keep this unprotected.
        // =================================================
        if (IsExitState("FN0000_OB_NF_FOR_LA_PF5_TO_ADD") || IsExitState
          ("OBLIGATION_SUCCESSFULLY_DISPLAY"))
        {
          // : Set hidden key fields
          export.HiddenCommon.Command = "DISPLAY";
          export.HiddenLegalAction.Assign(export.LegalAction);
          export.HiddenObligationType.Assign(export.ObligationType);
          MoveObligation4(export.Obligation, export.HiddenIntstInfo);
          export.HiddenInterstateRequest.Assign(export.InterstateRequest);

          export.HiddenDiscontinueDate.Date = export.DiscontinueDate.Date;

          if (AsChar(export.HistoryIndicator.Flag) == 'Y' && AsChar
            (export.Obligation.HistoryInd) == 'Y')
          {
            var field2 = GetField(export.HistoryIndicator, "flag");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

            field3.Color = "cyan";
            field3.Protected = true;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              var field4 =
                GetField(export.Group.Item.AccrualInstructions, "discontinueDt");
                

              field4.Color = "cyan";
              field4.Protected = true;
            }
          }

          if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Obligation, "orderTypeCode");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          // -------------------------------------------------------------------------------
          // If the user wants to build History Obligation  from an existing 
          // Obligation, the 'History Obliagtion' will be changed from 'N' to '
          // Y' and will press PF2. This will unprotect the 'Accrual Start Date'
          // field.
          // Now SME wants the following fields to be blanked out also.
          // 1. Created By, 2. Created Date, 3. Updated By, 4. Last Updated, 5. 
          // Ob Id, 6. Discontinue Date,
          // and for all supported persons: 7. 'Acrl Susp', 8. Disc Date.
          // Read the Obligation and check  if  'History_ind' is changed. If so,
          // clear all the fields.
          // -------------------------------------------------------------------------------
          if (ReadObligation1())
          {
            if (AsChar(export.HistoryIndicator.Flag) == 'Y' && (
              AsChar(entities.Obligation.HistoryInd) == 'N' || IsEmpty
              (entities.Obligation.HistoryInd)))
            {
              export.Obligation.CreatedBy = "";
              export.Obligation.CreatedTmst = local.Blank.Timestamp;
              export.Obligation.LastUpdatedBy = "";
              export.Obligation.LastUpdateTmst = local.Blank.Timestamp;
              export.Obligation.SystemGeneratedIdentifier = 0;
              export.DiscontinueDate.Date = local.Blank.Date;

              var field2 = GetField(export.HistoryIndicator, "flag");

              field2.Color = "cyan";
              field2.Protected = true;

              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                export.Group.Update.AccrualInstructions.DiscontinueDt =
                  local.Blank.Date;
                export.Group.Update.AccrualSuspended.Flag = "";
              }
            }
          }
        }

        // ***---  end of CASE DISPLAY
        break;
      case "LIST":
        local.NoOfPromptsSelected.Count = 0;

        if (AsChar(export.AltAddrPrompt.Text1) != 'S')
        {
          UseEabCursorPosition();

          switch(local.CursorPosition.Row)
          {
            case 5:
              export.AltAddrPrompt.Text1 = "S";

              break;
            case 7:
              export.CountryPrompt.SelectChar = "S";

              break;
            default:
              break;
          }
        }

        switch(AsChar(export.AltAddrPrompt.Text1))
        {
          case 'S':
            ++local.NoOfPromptsSelected.Count;

            break;
          case '+':
            break;
          case ' ':
            break;
          default:
            var field2 = GetField(export.AltAddrPrompt, "text1");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (local.NoOfPromptsSelected.Count > 1)
        {
          if (AsChar(export.AltAddrPrompt.Text1) == 'S')
          {
            var field2 = GetField(export.AltAddrPrompt, "text1");

            field2.Error = true;
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          break;
        }
        else if (local.NoOfPromptsSelected.Count == 0)
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          var field2 = GetField(export.AltAddrPrompt, "text1");

          field2.Color = "green";
          field2.Protected = false;
          field2.Focused = true;

          break;
        }

        if (AsChar(export.AltAddrPrompt.Text1) == 'S')
        {
          if (export.Obligation.SystemGeneratedIdentifier == 0 || IsEmpty
            (export.ObligationType.Code))
          {
            ExitState = "FN0000_OBLIG_REQUIRED_TO_FLOW";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }

          export.AltAddrPrompt.Text1 = "+";

          break;
        }

        var field1 = GetField(export.AltAddrPrompt, "text1");

        field1.Error = true;

        local.NoOfPromptsSelected.Count = 0;

        break;
      case "ADD":
        // ***         CASE ADD           ******
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        local.Common.Command = "ADD";
        export.HistoryIndicator.Command = global.Command;

        if (!Equal(export.HiddenCommon.Command, "DISPLAY"))
        {
          ExitState = "FN0000_DISPLAY_BEFORE_UPD_DEL";

          break;
        }

        if (Equal(export.ObligorCsePerson.Number,
          export.ConcurrentObligorCsePerson.Number))
        {
          ExitState = "FN0000_OBLIGOR_TWICE_ON_LOPS";

          break;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          local.TestDup.Count = 0;

          for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
            import.Group.Index)
          {
            if (Equal(export.Group.Item.SupportedCsePerson.Number,
              import.Group.Item.SupportedCsePerson.Number))
            {
              ++local.TestDup.Count;
            }
          }

          if (local.TestDup.Count > 1)
          {
            ExitState = "FN0000_SUPPORTED_TWICE_ON_LOPS";

            goto Test8;
          }
        }

        UseFnCheckExistenceOfObligation2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (AsChar(local.CheckObligationExistence.Flag) == 'Y')
        {
          ExitState = "FN0000_OBLIGATION_AE";

          var field2 = GetField(export.ObligationPaymentSchedule, "startDt");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.Country, "description");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.InterstateRequest, "country");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 = GetField(export.InterstateRequest, "tribalAgency");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 = GetField(export.Country, "description");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 = GetField(export.Obligation, "otherStateAbbr");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.Obligation, "orderTypeCode");

          field9.Color = "cyan";
          field9.Protected = true;

          // -------------------------------------------------------------------------------------
          // The following code is added to fix the PR# 115279 ( when the 
          // obligation is updated with discontinue date, sometimes the
          // supported person was not discontinued. The batch SRRUNBA2 is used
          // to fix this problem.)
          // The problem is,  if there is an error while discontinuing the 
          // obligation ( like pressing a wrong key, PF5 instead of PF6, etc.)
          // along with entering a wrong discontinued date,  the sup_person
          // discontinued date will be set to that wrong date,  and the error
          // message will be displayed. If the user then enter the right
          // discontinued date and hit the right key (PF6) the obligation will
          // be updated but the sup. person will still have the wrong date and (
          // accrual_instruction) will not be updated.
          // So the fix is to reset the sup. person disc.date back.
          //                                                      
          // Vithal Madhira(03/14/2001)
          // ---------------------------------------------------------------------------------------
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!Equal(export.Group.Item.Hidden.DiscontinueDt,
              export.Group.Item.AccrualInstructions.DiscontinueDt))
            {
              export.Group.Update.AccrualInstructions.DiscontinueDt =
                export.Group.Item.Hidden.DiscontinueDt;
            }
          }

          if (AsChar(export.HistoryIndicator.Flag) == 'Y' && export
            .Obligation.SystemGeneratedIdentifier == 0)
          {
            var field10 = GetField(export.HistoryIndicator, "flag");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 = GetField(export.ObligationPaymentSchedule, "startDt");

            field11.Color = "green";
            field11.Protected = false;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              export.Group.Update.AccrualInstructions.DiscontinueDt =
                local.Blank.Date;
            }
          }

          break;
        }

        if (Equal(export.BeforeLink.Command, "TYPE"))
        {
          if (IsEmpty(export.InterstateRequest.OtherStateCaseId) && AsChar
            (export.InterstateDebtExists.Flag) == 'N')
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Error = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Error = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Error = true;

            var field5 = GetField(export.Obligation, "otherStateAbbr");

            field5.Error = true;

            var field6 = GetField(export.Obligation, "orderTypeCode");

            field6.Error = true;

            ExitState = "FN0000_INTERSTATE_DATA_MISSING";

            break;
          }

          if (Lt(export.HiddenPrevious.EndDt,
            export.ObligationPaymentSchedule.StartDt))
          {
          }
          else
          {
            export.ObligationPaymentSchedule.StartDt =
              AddDays(export.HiddenPrevious.EndDt, 1);

            var field2 = GetField(export.ObligationPaymentSchedule, "startDt");

            field2.Error = true;

            ExitState = "FN0000_TIMEFRAME_OVERLAP_ERROR";

            break;
          }
        }

        // #####################
        // # RETRO PROCESSING  #
        // #####################
        // : WR 010504 - A Doty
        // To support Collection Protection, a warning message is to be 
        // displayed when the worker changes the discontinue date for the
        // obligation or any of the supported persons.
        if (IsEmpty(import.CollProtAnswer.SelectChar))
        {
          // : Only go further if the start date is in the past.
          if (Lt(export.ObligationPaymentSchedule.StartDt, local.Current.Date))
          {
            if (ReadCollection2())
            {
              var field2 = GetField(export.CollProtAnswer, "selectChar");

              field2.Color = "green";
              field2.Highlighting = Highlighting.Underscore;
              field2.Protected = false;
              field2.Focused = true;

              export.ProtectQuestionLiteral.Text80 =
                "State retained collections exist: protect prior payments?";
              ExitState = "FN0000_Y_OR_N_AND_PF5_TO_ADD_OB";

              break;
            }
            else
            {
              // : OK
            }
          }
        }
        else if (AsChar(import.CollProtAnswer.SelectChar) == 'N')
        {
          // : OK, continue.
        }
        else if (AsChar(import.CollProtAnswer.SelectChar) == 'Y')
        {
          // : Protect collections.
          local.ObligCollProtectionHist.CvrdCollStrtDt =
            export.ObligationPaymentSchedule.StartDt;

          // : The collection protection end date is set to current date, or the
          // earliest discontinue date, if it is prior to the current date.
          local.ObligCollProtectionHist.CvrdCollEndDt = local.Current.Date;

          if (Lt(export.DiscontinueDate.Date,
            local.ObligCollProtectionHist.CvrdCollEndDt) && !
            Equal(export.DiscontinueDate.Date, local.Blank.Date))
          {
            local.ObligCollProtectionHist.CvrdCollEndDt =
              export.DiscontinueDate.Date;
          }

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (Lt(export.Group.Item.AccrualInstructions.DiscontinueDt,
              local.ObligCollProtectionHist.CvrdCollEndDt) && !
              Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
              local.Blank.Date))
            {
              local.ObligCollProtectionHist.CvrdCollEndDt =
                export.Group.Item.AccrualInstructions.DiscontinueDt;
            }
          }

          foreach(var item in ReadObligation2())
          {
            UseFnProtectCollectionsForOblig();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test8;
            }
          }
        }
        else
        {
          // : Something other than 'Y' or 'N' was entered in the answer.
          var field2 = GetField(export.CollProtAnswer, "selectChar");

          field2.Color = "green";
          field2.Highlighting = Highlighting.Underscore;
          field2.Protected = false;
          field2.Focused = true;

          export.ProtectQuestionLiteral.Text80 =
            "State retained collections exist: protect prior payments?";
          ExitState = "FN0000_Y_OR_N_AND_PF5_TO_ADD_OB";

          break;
        }

        if (IsEmpty(export.Obligation.OrderTypeCode))
        {
          export.Obligation.OrderTypeCode = "K";
        }

        UseFnEstAccruingObligation();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        export.HiddenObligationPaymentSchedule.StartDt =
          export.ObligationPaymentSchedule.StartDt;
        local.Infrastructure.SituationNumber = 0;
        local.Infrastructure.ReferenceDate = local.Current.Date;

        // =================================================
        // 2/26/1999 - bud adams  -  Replaced CRUD in the 'est' action
        //   block above with the following logic / CAB.
        // =================================================
        if (!IsEmpty(export.InterstateRequest.OtherStateCaseId))
        {
          UseFnCreateInterstateRqstOblign1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          if (!IsEmpty(import.ConcurrentObligorCsePerson.Number))
          {
            UseFnCreateInterstateRqstOblign2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }
          }
        }

        UseFnEstablishAccrObligDetail();

        // =================================================
        // 12/7/99 - bud adams  -  PR# 80027: If Accrual Start Date =
        //   supported person Discontinue Date then suspend accrual.
        //   Remember Pearl Harbor.
        //   (Check to see if this has already been done and if accruals
        //   have already been run - if created date is in the past.)
        // =================================================
        if (AsChar(local.SuspendAccrual.Flag) == 'Y' && AsChar
          (export.HistoryIndicator.Flag) == 'N')
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.SuspendAccrual.Flag) == 'Y')
            {
              local.AccrualSuspension.ResumeDt = local.Max.Date;
              local.AccrualSuspension.SuspendDt =
                export.ObligationPaymentSchedule.StartDt;
              local.AccrualSuspension.ReductionPercentage = 100;
              local.AccrualSuspension.ReasonTxt =
                "Obligation created with Accrual Start Date equal to the supported person's Discontinue Date.";
                
              local.AccrualSuspension.CreatedTmst = local.Current.Timestamp;

              // ##########################
              // ##  add accrual suspension
              // ##########################
              UseFnAddAccrualSuspension();
              export.Group.Update.SuspendAccrual.Flag = "";
              export.Group.Update.AccrualSuspended.Flag = "Y";
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // : Set hidden key fields
          export.Obligation.CreatedTmst = local.Current.Timestamp;
          export.Obligation.CreatedBy = global.UserId;
          export.HiddenLegalAction.Assign(export.LegalAction);
          export.HiddenObligationType.Assign(export.ObligationType);
          MoveObligation4(export.Obligation, export.HiddenIntstInfo);
          export.HiddenInterstateRequest.Assign(export.InterstateRequest);
          export.HiddenDiscontinueDate.Date = export.DiscontinueDate.Date;
          export.AccrualAmount.TotalCurrency = 0;
          export.HiddenCommon.Command = "ADD";
          export.HistoryIndicator.Command = "";
          export.BeforeLink.Command = "";

          // : Aug. 6, 1999 mfb.
          if (AsChar(export.HistoryIndicator.Flag) == 'Y')
          {
            var field2 = GetField(export.HistoryIndicator, "flag");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

            field3.Color = "cyan";
            field3.Protected = true;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              export.Group.Update.AccrualInstructions.LastAccrualDt =
                local.Blank.Date;
              MoveAccrualInstructions(export.Group.Item.AccrualInstructions,
                export.Group.Update.Hidden);

              var field4 =
                GetField(export.Group.Item.AccrualInstructions, "discontinueDt");
                

              field4.Color = "cyan";
              field4.Protected = true;
            }
          }
          else
          {
            // -----------------------------------------------------------------
            // If the  history_indicator is N,  process the following 
            // statements. COPY  this code to  CASE DISPLAY also.
            //                                        
            // Vithal Madhira (06/07/2000)
            // -------------------------------------------------------------
            var field2 = GetField(export.HistoryIndicator, "flag");

            field2.Color = "green";
            field2.Protected = false;

            var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DiscontinueDate, "date");

            field4.Color = "green";
            field4.Protected = false;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              MoveAccrualInstructions(export.Group.Item.AccrualInstructions,
                export.Group.Update.Hidden);

              var field5 =
                GetField(export.Group.Item.AccrualInstructions, "discontinueDt");
                

              field5.Color = "green";
              field5.Protected = false;
            }
          }

          ExitState = "ACO_NI0000_OACC_CREATED_OK";

          // ----------------------------------------------------------------
          // This code is to populate Accrual Amounts.
          // ----------------------------------------------------------------
          export.AccrualAmount.TotalCurrency = 0;

          foreach(var item in ReadAccrualInstructionsObligationTransaction())
          {
            if (Lt(local.Current.Date,
              entities.AccrualInstructions.DiscontinueDt) || AsChar
              (export.Obligation.HistoryInd) == 'Y')
            {
              export.AccrualAmount.TotalCurrency += entities.
                ObligationTransaction.Amount;
            }
          }

          if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Obligation, "orderTypeCode");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'K')
          {
            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Obligation, "otherStateAbbr");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "country");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.InterstateRequest, "tribalAgency");

            field6.Color = "cyan";
            field6.Protected = true;
          }
        }
        else
        {
        }

        // ***---  end of CASE ADD
        break;
      case "UPDATE":
        // ***               CASE UPDATE         *********
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // : WR 010504 - A Doty
        // To support Collection Protection, a warning message is to be 
        // displayed when the worker is changes the discontinue date for the
        // obligation or any of the supported persons.
        if (IsEmpty(import.CollProtAnswer.SelectChar))
        {
          local.ChngdDiscDateFnd.Flag = "N";

          // : Set the date changed flag if the new discontinue date is greater 
          // than the old one, and the old discontinue date is in the past,
          // which will cause new debts to be created with a due date in the
          // past.
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (Equal(export.Group.Item.Hidden.DiscontinueDt, local.Blank.Date))
            {
              // : Nothing needs to be done if the discontinue date is max date.
              continue;
            }

            if (!Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
              export.Group.Item.Hidden.DiscontinueDt) && Lt
              (export.Group.Item.Hidden.DiscontinueDt, local.Current.Date) && (
                Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
              local.Blank.Date) || Lt
              (export.Group.Item.Hidden.DiscontinueDt,
              export.Group.Item.AccrualInstructions.DiscontinueDt)))
            {
              local.ChngdDiscDateFnd.Flag = "Y";

              break;
            }
          }

          if (Equal(export.HiddenAccrualInstructions.DiscontinueDt,
            local.Blank.Date))
          {
            // : Nothing needs to be done if the discontinue date is max date.
          }
          else if (!Equal(export.DiscontinueDate.Date,
            export.HiddenDiscontinueDate.Date) && Lt
            (export.HiddenAccrualInstructions.DiscontinueDt, local.Current.Date) &&
            (Equal(export.DiscontinueDate.Date, local.Blank.Date) || Lt
            (export.HiddenDiscontinueDate.Date, export.DiscontinueDate.Date)))
          {
            local.ChngdDiscDateFnd.Flag = "Y";
          }

          if (AsChar(local.ChngdDiscDateFnd.Flag) == 'Y')
          {
            // : If the discontinue date was updated such that new debts will be
            // created,
            //   in the past, check for AF, FC, NF or NC payments on the court 
            // order.
            // : Find the lowest discontinue date, and use that as the low date 
            // in the date range
            //   to search for collections that should be protected.
            local.CollProt.Date = export.HiddenDiscontinueDate.Date;

            if (Equal(local.CollProt.Date, local.Blank.Date))
            {
              local.CollProt.Date = local.Max.Date;
            }

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (Lt(export.Group.Item.Hidden.DiscontinueDt, local.CollProt.Date)
                && !
                Equal(export.Group.Item.Hidden.DiscontinueDt, local.Blank.Date))
              {
                local.CollProt.Date = export.Group.Item.Hidden.DiscontinueDt;
              }
            }

            if (ReadCollection1())
            {
              var field2 = GetField(export.CollProtAnswer, "selectChar");

              field2.Color = "green";
              field2.Highlighting = Highlighting.Underscore;
              field2.Protected = false;
              field2.Focused = true;

              export.ProtectQuestionLiteral.Text80 =
                "State retained collections exist: protect prior payments?";
              ExitState = "FN0000_PRESS_PF6_TO_CONFIRM_UPD";

              break;
            }
            else
            {
              // : OK
            }
          }
        }
        else if (AsChar(import.CollProtAnswer.SelectChar) == 'N')
        {
          // : OK, continue.
        }
        else if (AsChar(import.CollProtAnswer.SelectChar) == 'Y')
        {
          // : Protect collections.
          // : Find the lowest discontinue date, and use that as the low date in
          // the date range
          //   to search for collections that should be protected.
          local.ObligCollProtectionHist.CvrdCollStrtDt =
            export.HiddenAccrualInstructions.DiscontinueDt;

          if (Equal(local.ObligCollProtectionHist.CvrdCollStrtDt,
            local.Blank.Date))
          {
            local.ObligCollProtectionHist.CvrdCollStrtDt = local.Max.Date;
          }

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (Lt(export.Group.Item.Hidden.DiscontinueDt,
              local.ObligCollProtectionHist.CvrdCollStrtDt) && !
              Equal(export.Group.Item.Hidden.DiscontinueDt, local.Blank.Date))
            {
              local.ObligCollProtectionHist.CvrdCollStrtDt =
                export.Group.Item.Hidden.DiscontinueDt;
            }
          }

          local.ObligCollProtectionHist.CvrdCollEndDt = local.Current.Date;

          foreach(var item in ReadObligation2())
          {
            UseFnProtectCollectionsForOblig();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test8;
            }
          }
        }
        else
        {
          // : Something other than 'Y' or 'N' was entered in the answer.
          var field2 = GetField(export.CollProtAnswer, "selectChar");

          field2.Color = "green";
          field2.Highlighting = Highlighting.Underscore;
          field2.Protected = false;
          field2.Focused = true;

          export.ProtectQuestionLiteral.Text80 =
            "State retained collections exist: protect prior payments?";
          ExitState = "FN0000_PRESS_PF6_TO_CONFIRM_UPD";

          break;
        }

        if (AsChar(export.HistoryIndicator.Flag) == 'Y' && export
          .Obligation.SystemGeneratedIdentifier > 0 && Equal
          (export.Obligation.Description, export.HiddenIntstInfo.Description) &&
          Equal(export.Alternate.Number, export.HiddenAlternate.Number))
        {
          // ------------------------------------------------------------------
          // You can update only the 'NOTE' and 'Alt Bill Loc' fields.
          // ----------------------------------------------------------------------
          ExitState = "FN0000_ADD_ONLY_WITH_HISTORY_IND";

          if (AsChar(export.HistoryIndicator.Flag) == 'Y')
          {
            var field2 = GetField(export.HistoryIndicator, "flag");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

            field3.Color = "cyan";
            field3.Protected = true;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              var field4 =
                GetField(export.Group.Item.AccrualInstructions, "discontinueDt");
                

              field4.Color = "cyan";
              field4.Protected = true;
            }
          }

          if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Obligation, "orderTypeCode");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'K')
          {
            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Obligation, "otherStateAbbr");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "country");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.InterstateRequest, "tribalAgency");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          break;
        }

        if (Equal(export.BeforeLink.Command, "TYPE"))
        {
          ExitState = "FN0000_ADD_ONLY_INTRST_TIMEFRAME";

          break;
        }

        local.Common.Command = "UPDATE";
        UseFnCheckExistenceOfObligation3();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (AsChar(local.CheckObligationExistence.Flag) == 'N')
        {
          ExitState = "OBLIGATION_NF";

          break;
        }

        if (Equal(export.DiscontinueDate.Date, local.Blank.Date))
        {
          export.DiscontinueDate.Date = local.Max.Date;
        }

        if (Equal(export.BeforeLink.Command, "TYPE") || Lt
          (local.Blank.Date, export.HiddenPrevious.EndDt))
        {
          if (IsEmpty(export.InterstateRequest.OtherStateCaseId) && AsChar
            (export.InterstateDebtExists.Flag) == 'N')
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Error = true;

            var field3 = GetField(export.Obligation, "otherStateAbbr");

            field3.Error = true;

            var field4 = GetField(export.Obligation, "orderTypeCode");

            field4.Error = true;

            ExitState = "FN0000_INTERSTATE_DATA_MISSING";

            break;
          }

          // ***---  if Start Date has changed AND this is a multi-timeframe
          // ***---  it must be AFTER the previous End Date
          if (Lt(export.HiddenPrevious.EndDt,
            export.ObligationPaymentSchedule.StartDt) || Equal
            (export.HiddenObligationPaymentSchedule.StartDt,
            export.ObligationPaymentSchedule.StartDt))
          {
          }
          else
          {
            export.ObligationPaymentSchedule.StartDt =
              AddDays(export.HiddenPrevious.EndDt, 1);

            var field2 = GetField(export.ObligationPaymentSchedule, "startDt");

            field2.Error = true;

            ExitState = "FN0000_TIMEFRAME_OVERLAP_ERROR";

            break;
          }
        }

        // =================================================
        // PR# 75247: 9/28/99 - bud adams  -  If the user had entered Y
        //   in the History Indicator and pressed PF-2 Display, the
        //   Accrual Start Dt would be unprotected.  Then, if the user
        //   changed the Y to an N, and then changed the Accrual_Start
        //   _Dt and did a PF-6 Update - the accrual start date was
        //   updated.  This will never give that a chance to happen.
        // =================================================
        if (AsChar(export.HistoryIndicator.Flag) == 'N' && !
          Equal(export.ObligationCreatedDate.Date, local.Current.Date))
        {
          export.ObligationPaymentSchedule.StartDt =
            export.HiddenPrevious.StartDt;
        }

        if (IsEmpty(export.Obligation.OrderTypeCode))
        {
          export.Obligation.OrderTypeCode = "K";
        }

        // =================================================
        // 12/16/98 - B Adams  -  User must be able to change alternate
        //   billing address to be <SPACES>.
        // =================================================
        if (Equal(export.Alternate.Number, export.HiddenAlternate.Number))
        {
        }
        else
        {
          export.Alternate.Flag = "C";
        }

        UseFnUpdateAccruingObligation();

        // =================================================
        // 12/7/99 - bud adams  -  PR# 80027: If Accrual Start Date =
        //   supported person Discontinue Date then suspend accrual.
        //   Added this to existing IF / FOR EACH construct.
        //   Remember Pearl Harbor.
        //   (Check to see if this has already been done and if accruals
        //   have already been run - if created date is in the past.)
        // =================================================
        if (AsChar(local.SuspendAccrual.Flag) == 'Y' && AsChar
          (export.HistoryIndicator.Flag) == 'N' || AsChar
          (local.SupPersMarkedForUpdate.Flag) == 'Y')
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.SuspendAccrual.Flag) == 'Y')
            {
              // ##########################
              // ##  add accrual suspension
              // ##########################
              local.AccrualSuspension.ResumeDt = local.Max.Date;
              local.AccrualSuspension.SuspendDt =
                export.ObligationPaymentSchedule.StartDt;
              local.AccrualSuspension.ReductionPercentage = 100;
              local.AccrualSuspension.ReasonTxt =
                "Obligation created with Accrual Start Date equal to the supported person's Discontinue Date.";
                
              local.AccrualSuspension.CreatedTmst = local.Current.Timestamp;
              UseFnAddAccrualSuspension();
              export.Group.Update.SuspendAccrual.Flag = "";
              export.Group.Update.AccrualSuspended.Flag = "Y";
            }

            if (AsChar(local.SupPersMarkedForUpdate.Flag) == 'Y')
            {
              export.Group.Update.Sel.SelectChar = "";
            }
          }
        }

        if (Equal(export.Obligation.OtherStateAbbr, "KS"))
        {
          export.Obligation.OtherStateAbbr = "";
        }

        if (IsEmpty(export.Alternate.Number))
        {
          export.Alternate.FormattedName = "";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Obligation.LastUpdateTmst = local.Current.Timestamp;
          export.Obligation.LastUpdatedBy = global.UserId;
          export.HiddenCommon.Command = "UPDATE";
          export.HiddenAlternate.Number = export.Alternate.Number;
          MoveObligation4(export.Obligation, export.HiddenIntstInfo);
          export.HiddenDiscontinueDate.Date = export.DiscontinueDate.Date;
          export.HiddenInterstateRequest.Assign(export.InterstateRequest);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

          // =================================================
          // 06/07/2000  Vithal Madhira  Per SME(Marilyn), the following fields 
          // will be protected/unprotected if the 'History Obligation' is N and
          // obligation was already added on the OACC screen. Check the  CASE
          // ADD for similar code.
          // =================================================
          if (AsChar(export.HistoryIndicator.Flag) == 'N' && !
            Lt(local.Current.Date, export.ObligationCreatedDate.Date) && Lt
            (local.Blank.Date, export.ObligationCreatedDate.Date))
          {
            var field2 = GetField(export.HistoryIndicator, "flag");

            field2.Color = "green";
            field2.Protected = false;

            var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DiscontinueDate, "date");

            field4.Color = "green";
            field4.Protected = false;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              var field5 =
                GetField(export.Group.Item.AccrualInstructions, "discontinueDt");
                

              field5.Color = "green";
              field5.Protected = false;
            }
          }

          if (AsChar(export.HistoryIndicator.Flag) == 'Y')
          {
            var field2 = GetField(export.HistoryIndicator, "flag");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

            field3.Color = "cyan";
            field3.Protected = true;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              var field4 =
                GetField(export.Group.Item.AccrualInstructions, "discontinueDt");
                

              field4.Color = "cyan";
              field4.Protected = true;
            }
          }

          if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Obligation, "orderTypeCode");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'K')
          {
            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Obligation, "otherStateAbbr");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "country");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.InterstateRequest, "tribalAgency");

            field6.Color = "cyan";
            field6.Protected = true;
          }
        }
        else if (AsChar(local.AltBillLocError.SelectChar) == 'Y')
        {
          var field2 = GetField(export.Alternate, "number");

          field2.Error = true;
        }

        // ***---  end of CASE UPDATE
        break;
      case "DELETE":
        if (export.Obligation.SystemGeneratedIdentifier == 0)
        {
          ExitState = "OBLIGATION_NF";

          break;
        }

        if (Equal(export.BeforeLink.Command, "TYPE"))
        {
          ExitState = "FN0000_ADD_ONLY_INTRST_TIMEFRAME";

          break;
        }

        UseFnDeleteObligation();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // <<< USE Eab_Rollback_CICS >>>
          UseEabRollbackCics();

          break;
        }

        export.HiddenLegalAction.StandardNumber = "";
        export.ObligationPaymentSchedule.StartDt = local.Blank.Date;
        export.DiscontinueDate.Date = local.Blank.Date;
        export.Obligation.Description = "";
        export.AccrualAmount.TotalCurrency = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          export.Group.Update.AccrualInstructions.DiscontinueDt =
            local.Blank.Date;
          export.Group.Update.Sel.SelectChar = "";
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        // ***---  end of CASE DELETE
        break;
      case "MDIS":
        ExitState = "ECO_LNK_TO_MTN_MANUAL_DIST_INST";

        break;
      case "COLP":
        ExitState = "ECO_LNK_TO_COLP";

        break;
      case "CSPM":
        ExitState = "ECO_LNK_LST_MTN_OB_S_C_SUPP";

        break;
      case "INMS":
        ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

        // =================================================
        // 06/07/2000  Vithal Madhira  Per SME(Marilyn, the following fields 
        // will be protected/unprotected if the 'History Obligation' is N and
        // obligation was already added on the OACC screen. Check the  CASE ADD
        // for similar code.
        // =================================================
        if (AsChar(export.HistoryIndicator.Flag) == 'N' && !
          Lt(local.Current.Date, export.ObligationCreatedDate.Date) && Lt
          (local.Blank.Date, export.ObligationCreatedDate.Date))
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "green";
          field2.Protected = false;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DiscontinueDate, "date");

          field4.Color = "green";
          field4.Protected = false;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field5 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field5.Color = "green";
            field5.Protected = false;
          }
        }

        if (AsChar(export.HistoryIndicator.Flag) == 'Y' && AsChar
          (export.Obligation.HistoryInd) == 'Y')
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field4 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }

        if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
        {
          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "country");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "tribalAgency");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.Country))
        {
          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "country");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "tribalAgency");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
        {
          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "country");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "tribalAgency");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
        {
          var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.Obligation, "orderTypeCode");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'K')
        {
          if (export.Obligation.SystemGeneratedIdentifier > 0)
          {
            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Obligation, "otherStateAbbr");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "country");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.InterstateRequest, "tribalAgency");

            field6.Color = "cyan";
            field6.Protected = true;
          }
        }

        break;
      case "OPAY":
        // =================================================
        // 12/2/98 - B Adams  -  Dialog flow return from link to OPAY has
        //   'legal_action' imported if user returns via PF9.  But, because
        //   this value is not sent to OPAY, and because OPAY has no
        //   way of knowing what specific Legal_Action to ship back, it
        //   really cannot provide this data - and so it doesn't.  Thing is
        //   those incoming SPACES replace whatever legal-action data
        //   may have existed.
        //   The intent here is to give us a signal that when COMMAND
        //   is 'FROMOPAY' that we know if it's returning from a Link,
        //   or is flowing via PF15 from OPAY.  If it's a link return, then
        //   we want to recall the Legal_Action data from the hidden
        //   view.
        //   If the time ever comes that the dialog manager for OPAY can
        //   be scoped into this subset, simply remove Legal_Action
        //   from the return link and get rid of all this other stuff.
        // =================================================
        export.HiddenLegalAction.Assign(export.LegalAction);
        export.HiddenObligationType.Assign(export.ObligationType);
        export.HiddenObligorObligation.SystemGeneratedIdentifier =
          export.Obligation.SystemGeneratedIdentifier;
        export.ObligorCsePersonsWorkSet.Char2 = "LK";
        ExitState = "ECO_LNK_LST_OBLIG_BY_AP_PAYOR";

        break;
      case "OCTO":
        ExitState = "ECO_LNK_TO_LST_OBLIG_BY_CRT_ORDR";

        break;
      case "ASUS":
        local.SelectCounter.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          switch(AsChar(export.Group.Item.Sel.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              MoveObligationTransaction1(export.Group.Item.
                ObligationTransaction, export.PassObligationTransaction);
              export.PassSupportedPerson.Number =
                export.Group.Item.SupportedCsePerson.Number;
              ++local.SelectCounter.Count;

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              break;
          }
        }

        if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE"))
        {
        }
        else
        {
          switch(local.SelectCounter.Count)
          {
            case 0:
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                var field2 = GetField(export.Group.Item.Sel, "selectChar");

                field2.Error = true;
              }

              ExitState = "ACO_NE0000_SELECTION_REQUIRED";

              break;
            case 1:
              ExitState = "ECO_LNK_TO_MTN_ACCR_OBLIG_SUSP";

              break;
            default:
              ExitState = "OE0000_MORE_THAN_ONE_RECORD_SELD";

              break;
          }
        }

        // =================================================
        // 06/07/2000  Vithal Madhira  Per SME(Marilyn, the following fields 
        // will be protected/unprotected if the 'History Obligation' is N and
        // obligation was already added on the OACC screen. Check the  CASE ADD
        // for similar code.
        // =================================================
        if (AsChar(export.HistoryIndicator.Flag) == 'N' && !
          Lt(local.Current.Date, export.ObligationCreatedDate.Date) && Lt
          (local.Blank.Date, export.ObligationCreatedDate.Date))
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "green";
          field2.Protected = false;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DiscontinueDate, "date");

          field4.Color = "green";
          field4.Protected = false;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field5 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field5.Color = "green";
            field5.Protected = false;
          }
        }

        if (AsChar(export.HistoryIndicator.Flag) == 'Y' && AsChar
          (export.Obligation.HistoryInd) == 'Y')
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field4 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }

        if (export.Obligation.SystemGeneratedIdentifier > 0)
        {
          if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Obligation, "orderTypeCode");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'K')
          {
            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Obligation, "otherStateAbbr");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "country");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.InterstateRequest, "tribalAgency");

            field6.Color = "cyan";
            field6.Protected = true;
          }
        }

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        // =================================================
        // 06/07/2000  Vithal Madhira  Per SME(Marilyn, the following fields 
        // will be protected/unprotected if the 'History Obligation' is N and
        // obligation was already added on the OACC screen. Check the  CASE ADD
        // for similar code.
        // =================================================
        if (AsChar(export.HistoryIndicator.Flag) == 'N' && !
          Lt(local.Current.Date, export.ObligationCreatedDate.Date) && Lt
          (local.Blank.Date, export.ObligationCreatedDate.Date))
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "green";
          field2.Protected = false;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DiscontinueDate, "date");

          field4.Color = "green";
          field4.Protected = false;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field5 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field5.Color = "green";
            field5.Protected = false;
          }
        }

        if (AsChar(export.HistoryIndicator.Flag) == 'Y' && AsChar
          (export.Obligation.HistoryInd) == 'Y')
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field4 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }

        if (export.Obligation.SystemGeneratedIdentifier > 0)
        {
          if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Obligation, "orderTypeCode");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'K')
          {
            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Obligation, "otherStateAbbr");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "country");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.InterstateRequest, "tribalAgency");

            field6.Color = "cyan";
            field6.Protected = true;
          }
        }

        break;
      case "PREV":
        ExitState = "CO0000_SCROLLED_BEYOND_1ST_PG";

        // =================================================
        // 06/07/2000  Vithal Madhira  Per SME(Marilyn, the following fields 
        // will be protected/unprotected if the 'History Obligation' is N and
        // obligation was already added on the OACC screen. Check the  CASE ADD
        // for similar code.
        // =================================================
        if (AsChar(export.HistoryIndicator.Flag) == 'N' && !
          Lt(local.Current.Date, export.ObligationCreatedDate.Date) && Lt
          (local.Blank.Date, export.ObligationCreatedDate.Date))
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "green";
          field2.Protected = false;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DiscontinueDate, "date");

          field4.Color = "green";
          field4.Protected = false;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field5 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field5.Color = "green";
            field5.Protected = false;
          }
        }

        if (AsChar(export.HistoryIndicator.Flag) == 'Y' && AsChar
          (export.Obligation.HistoryInd) == 'Y')
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field4 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }

        if (export.Obligation.SystemGeneratedIdentifier > 0)
        {
          if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Obligation, "orderTypeCode");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'K')
          {
            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Obligation, "otherStateAbbr");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "country");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.InterstateRequest, "tribalAgency");

            field6.Color = "cyan";
            field6.Protected = true;
          }
        }

        break;
      case "RETURN":
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " + "XXNEXTXX"
            ;
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "BYPASS":
        // =================================================
        // 06/07/2000  Vithal Madhira  Per SME(Marilyn, the following fields 
        // will be protected/unprotected if the 'History Obligation' is N and
        // obligation was already added on the OACC screen. Check the  CASE ADD
        // for similar code.
        // =================================================
        if (AsChar(export.HistoryIndicator.Flag) == 'N' && !
          Lt(local.Current.Date, export.ObligationCreatedDate.Date) && Lt
          (local.Blank.Date, export.ObligationCreatedDate.Date))
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "green";
          field2.Protected = false;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DiscontinueDate, "date");

          field4.Color = "green";
          field4.Protected = false;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field5 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field5.Color = "green";
            field5.Protected = false;
          }
        }

        if (AsChar(export.HistoryIndicator.Flag) == 'Y' && AsChar
          (export.Obligation.HistoryInd) == 'Y')
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field4 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }

        if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
        {
          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "country");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "tribalAgency");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.Country))
        {
          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "country");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "tribalAgency");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
        {
          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "country");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "tribalAgency");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
        {
          var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.Obligation, "orderTypeCode");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (AsChar(export.HistoryIndicator.Flag) == 'Y')
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'K')
        {
          if (export.Obligation.SystemGeneratedIdentifier > 0 || AsChar
            (export.HistoryIndicator.Flag) == 'Y')
          {
            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Obligation, "otherStateAbbr");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "country");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.InterstateRequest, "tribalAgency");

            field6.Color = "cyan";
            field6.Protected = true;
          }
        }

        break;
      case "PEPR":
        // -----------------------------------------------------------------------------
        // Uncomment this code when the flow to PEPR is created.
        //                                             
        // ---- Vithal (12/18/2000)
        // -----------------------------------------------------------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // --------------------------------------------------------------------------
        // WR# 000253 :  This new flow is added as part of the business rule.
        //                                                     
        // Vithal (12/15/2000)
        // --------------------------------------------------------------------------
        local.CountSelected.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          // -----------------------------------------------------------------------
          // Count the no. of supported persons selected. If more than one 
          // selected, display an error message.
          // ---------------------------------------------------------------------
          if (AsChar(export.Group.Item.Sel.SelectChar) == 'S')
          {
            ++local.CountSelected.Count;
          }
          else if (!IsEmpty(export.Group.Item.Sel.SelectChar))
          {
            var field2 = GetField(export.Group.Item.Sel, "selectChar");

            field2.Error = true;

            local.FlowToPepr.Flag = "N";
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        switch(local.CountSelected.Count)
        {
          case 0:
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.CountSelected.Count = 0;

              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                // ---------------------------------------------------------------------------------
                // Per WR# 253, user can flow to PEPR without selecting 
                // supported person, if only one supported person exists.
                // -------------------------------------------------------------------------------
                if (!IsEmpty(export.Group.Item.SupportedCsePerson.Number))
                {
                  ++local.CountSelected.Count;
                  export.HiddenFlowToPeprCsePersonsWorkSet.Number =
                    export.Group.Item.SupportedCsePerson.Number;
                }
              }

              if (local.CountSelected.Count > 1)
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  var field2 = GetField(export.Group.Item.Sel, "selectChar");

                  field2.Error = true;

                  ExitState = "ACO_NE0000_NO_SELECTION_MADE";
                }

                export.HiddenFlowToPeprCsePersonsWorkSet.Number = "";
              }
              else
              {
                local.FlowToPepr.Flag = "Y";
              }
            }

            break;
          case 1:
            local.FlowToPepr.Flag = "Y";

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (AsChar(export.Group.Item.Sel.SelectChar) == 'S')
              {
                export.Group.Update.Sel.SelectChar = "";
                export.HiddenFlowToPeprCsePersonsWorkSet.Number =
                  export.Group.Item.SupportedCsePerson.Number;
              }
            }

            break;
          default:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (AsChar(export.Group.Item.Sel.SelectChar) == 'S')
              {
                var field2 = GetField(export.Group.Item.Sel, "selectChar");

                field2.Error = true;
              }
            }

            local.FlowToPepr.Flag = "N";
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        if (AsChar(local.FlowToPepr.Flag) == 'Y')
        {
          // ---------------------------------------------------------------------------------
          // Case# needs to be passed to PEPR. It is mandatory. Find out the 
          // Case# to which the selected supported person associated and pass
          // the value to PEPR.
          // -------------------------------------------------------------------------------
          if (ReadCase())
          {
            export.HiddenFlowToPeprCase.Number = entities.Case1.Number;
            ExitState = "ECO_LNK_TO_PEPR";

            return;
          }
          else
          {
            ExitState = "CASE_NF";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(export.HistoryIndicator.Flag) == 'N' && !
            Lt(local.Current.Date, export.ObligationCreatedDate.Date) && Lt
            (local.Blank.Date, export.ObligationCreatedDate.Date))
          {
            var field2 = GetField(export.HistoryIndicator, "flag");

            field2.Color = "green";
            field2.Protected = false;

            var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.DiscontinueDate, "date");

            field4.Color = "green";
            field4.Protected = false;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              var field5 =
                GetField(export.Group.Item.AccrualInstructions, "discontinueDt");
                

              field5.Color = "green";
              field5.Protected = false;
            }
          }

          if (AsChar(export.HistoryIndicator.Flag) == 'Y' && AsChar
            (export.Obligation.HistoryInd) == 'Y')
          {
            var field2 = GetField(export.HistoryIndicator, "flag");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

            field3.Color = "cyan";
            field3.Protected = true;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              var field4 =
                GetField(export.Group.Item.AccrualInstructions, "discontinueDt");
                

              field4.Color = "cyan";
              field4.Protected = true;
            }
          }

          if (export.Obligation.SystemGeneratedIdentifier > 0)
          {
            if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
            {
              var field2 = GetField(export.Obligation, "otherStateAbbr");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.InterstateRequest, "country");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.InterstateRequest, "tribalAgency");

              field4.Color = "cyan";
              field4.Protected = true;
            }

            if (!IsEmpty(export.HiddenInterstateRequest.Country))
            {
              var field2 = GetField(export.Obligation, "otherStateAbbr");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.InterstateRequest, "country");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.InterstateRequest, "tribalAgency");

              field4.Color = "cyan";
              field4.Protected = true;
            }

            if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
            {
              var field2 = GetField(export.Obligation, "otherStateAbbr");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.InterstateRequest, "country");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.InterstateRequest, "tribalAgency");

              field4.Color = "cyan";
              field4.Protected = true;
            }

            if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
            {
              var field2 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.Obligation, "orderTypeCode");

              field3.Color = "cyan";
              field3.Protected = true;
            }

            if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'K')
            {
              var field2 = GetField(export.Obligation, "orderTypeCode");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.Obligation, "otherStateAbbr");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 = GetField(export.InterstateRequest, "country");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 = GetField(export.InterstateRequest, "tribalAgency");

              field6.Color = "cyan";
              field6.Protected = true;
            }
          }
        }

        break;
      default:
        // =================================================
        // 06/07/2000  Vithal Madhira  Per SME(Marilyn, the following fields 
        // will be protected/unprotected if the 'History Obligation' is N and
        // obligation was already added on the OACC screen. Check the  CASE ADD
        // for similar code.
        // =================================================
        if (AsChar(export.HistoryIndicator.Flag) == 'N' && !
          Lt(local.Current.Date, export.ObligationCreatedDate.Date) && Lt
          (local.Blank.Date, export.ObligationCreatedDate.Date))
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "green";
          field2.Protected = false;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DiscontinueDate, "date");

          field4.Color = "green";
          field4.Protected = false;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field5 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field5.Color = "green";
            field5.Protected = false;
          }
        }

        if (AsChar(export.HistoryIndicator.Flag) == 'Y' && AsChar
          (export.Obligation.HistoryInd) == 'Y')
        {
          var field2 = GetField(export.HistoryIndicator, "flag");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

          field3.Color = "cyan";
          field3.Protected = true;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field4 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }

        if (export.Obligation.SystemGeneratedIdentifier > 0)
        {
          if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Obligation, "orderTypeCode");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'K')
          {
            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Obligation, "otherStateAbbr");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "country");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.InterstateRequest, "tribalAgency");

            field6.Color = "cyan";
            field6.Protected = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test8:

    // ==============================================
    // 04/15/2000 - K. Price
    // SI_READ_CSE_PERSON calls ADABAS to get the first,
    // middle initial, and last name for a person.  When debugging
    // in trace mode, it will abend when calling this action block.
    // All calls to SI_READ_CSE_PERSON have been moved here
    // to facilitate disabling the calls quickly, and safely.
    // ===============================================
    if (!IsEmpty(export.ObligorCsePerson.Number) && IsEmpty
      (export.ObligorCsePersonsWorkSet.FormattedName))
    {
      export.HiddenObligorCsePerson.Number = "";
    }

    if (!IsEmpty(export.ObligorCsePerson.Number) && !
      Equal(export.ObligorCsePerson.Number, export.HiddenObligorCsePerson.Number))
      
    {
      export.ObligorCsePersonsWorkSet.Number = export.ObligorCsePerson.Number;
      UseSiReadCsePerson2();

      if (IsEmpty(export.ObligorCsePersonsWorkSet.FormattedName))
      {
        export.ObligorCsePersonsWorkSet.FormattedName =
          TrimEnd(export.ObligorCsePersonsWorkSet.LastName) + ", " + TrimEnd
          (export.ObligorCsePersonsWorkSet.FirstName) + " " + export
          .ObligorCsePersonsWorkSet.MiddleInitial;
      }

      if (!IsEmpty(local.Eab.Type1))
      {
        var field1 = GetField(export.ObligorCsePerson, "number");

        field1.Error = true;

        ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
      }
    }

    export.HiddenObligorCsePerson.Number = export.ObligorCsePerson.Number;

    if (!IsEmpty(export.ConcurrentObligorCsePerson.Number) && IsEmpty
      (export.ConcurrentObligorCsePersonsWorkSet.FormattedName))
    {
      export.HiddenConcurrent.Number = "";
    }

    if (!IsEmpty(export.ConcurrentObligorCsePerson.Number) && !
      Equal(export.ConcurrentObligorCsePerson.Number,
      export.HiddenConcurrent.Number))
    {
      export.ConcurrentObligorCsePersonsWorkSet.Number =
        export.ConcurrentObligorCsePerson.Number;
      UseSiReadCsePerson3();

      if (IsEmpty(export.ConcurrentObligorCsePersonsWorkSet.FormattedName))
      {
        export.ConcurrentObligorCsePersonsWorkSet.FormattedName =
          TrimEnd(export.ConcurrentObligorCsePersonsWorkSet.LastName) + ", " + TrimEnd
          (export.ConcurrentObligorCsePersonsWorkSet.FirstName) + " " + export
          .ConcurrentObligorCsePersonsWorkSet.MiddleInitial;
      }

      if (!IsEmpty(local.Eab.Type1))
      {
        var field1 = GetField(export.ConcurrentObligorCsePerson, "number");

        field1.Error = true;

        ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
      }
    }

    export.HiddenConcurrent.Number = export.ConcurrentObligorCsePerson.Number;

    if (!IsEmpty(export.Alternate.Number) && IsEmpty
      (export.Alternate.FormattedName))
    {
      export.HiddenAltAddress.Number = "";
    }

    if (!IsEmpty(export.Alternate.Number) && !
      Equal(export.Alternate.Number, export.HiddenAltAddress.Number))
    {
      UseSiReadCsePerson1();

      if (IsEmpty(export.Alternate.FormattedName))
      {
        export.Alternate.FormattedName = TrimEnd(export.Alternate.LastName) + ", " +
          TrimEnd(export.Alternate.FirstName) + " " + export
          .Alternate.MiddleInitial;
      }

      if (!IsEmpty(local.Eab.Type1))
      {
        var field1 = GetField(export.Alternate, "number");

        field1.Error = true;

        ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
      }
    }

    export.HiddenAltAddress.Number = export.Alternate.Number;

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE"))
    {
      if (IsExitState("ACO_NI0000_OACC_CREATED_OK") || IsExitState
        ("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
        ("ACO_NI0000_SUCCESSFUL_DELETE"))
      {
        // DESIRED OPERATION SUCCESSFULLY COMPLETED
      }
      else
      {
        UseEabRollbackCics();
      }
    }

    // **** The following statements will be executed after all processing ****
    if (Equal(export.DiscontinueDate.Date, local.Max.Date))
    {
      export.DiscontinueDate.Date = local.Blank.Date;
    }

    if (!export.Group.IsEmpty)
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!IsEmpty(export.Group.Item.Sel.SelectChar))
        {
          if (AsChar(local.FlowToPepr.Flag) == 'N')
          {
          }
          else
          {
            export.Group.Update.Sel.SelectChar = "";
          }
        }

        if (Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
          local.Max.Date))
        {
          export.Group.Update.AccrualInstructions.DiscontinueDt =
            local.Blank.Date;
        }

        // =================================================
        // 1/26/99 - B Adams  -  If there is no End_Date for the supported
        //   person in Legal_Action_Person, then the Accrual_Instruction
        //   Discontinue_Date on the screen for that peron must be
        //   unprotected.
        // =================================================
        if (AsChar(export.Group.Item.Sel.Flag) == 'U')
        {
          if (AsChar(export.HistoryIndicator.Flag) == 'Y' && export
            .Obligation.SystemGeneratedIdentifier > 0)
          {
            var field1 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field1.Color = "cyan";
            field1.Highlighting = Highlighting.Normal;
            field1.Protected = true;
          }
          else
          {
            var field1 =
              GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

            field1.Color = "green";
            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = false;
          }
        }

        // =================================================
        // 5/5/99 - bud adams  -  If the supported person is non-case-
        //   related, it is not maintainable.  It's display only.
        // 6/1/99 - bud adams  -  Also, discontinue date does not apply
        //   and should be blank and protected.
        // =================================================
        if (Equal(export.Group.Item.ProgramScreenAttributes.ProgramTypeInd, "Z"))
          
        {
          export.Group.Update.AccrualInstructions.DiscontinueDt =
            local.Blank.Date;
          export.Group.Update.Sel.SelectChar = "";

          var field1 = GetField(export.Group.Item.Sel, "selectChar");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

          field2.Color = "cyan";
          field2.Highlighting = Highlighting.Normal;
          field2.Protected = true;
        }

        if (Equal(export.BeforeLink.Command, "TYPE") && Equal
          (global.Command, "DISPLAY"))
        {
          export.Group.Update.AccrualInstructions.DiscontinueDt =
            local.Blank.Date;

          var field1 =
            GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

          field1.Color = "green";
          field1.Highlighting = Highlighting.Underscore;
          field1.Protected = false;
        }

        // =================================================
        // 1/20/00 - b adams  -  PR# 83301: When a history obligation
        //   is Added all accruals are automatically suspended, but the
        //   indicator was not being displayed.
        // =================================================
        if (IsExitState("ACO_NI0000_OACC_CREATED_OK") && AsChar
          (export.HistoryIndicator.Flag) == 'Y')
        {
          export.Group.Update.AccrualSuspended.Flag = "Y";
        }

        if (!IsEmpty(export.Group.Item.SupportedCsePerson.Number) && !
          Equal(export.Group.Item.SupportedCsePerson.Number,
          export.Group.Item.SupportedCsePersonsWorkSet.Number))
        {
          local.Supported.Number = export.Group.Item.SupportedCsePerson.Number;
          UseSiReadCsePerson4();
          MoveCsePersonsWorkSet1(local.Supported,
            export.Group.Update.SupportedCsePersonsWorkSet);

          if (IsEmpty(export.Group.Item.SupportedCsePersonsWorkSet.FormattedName))
            
          {
            export.Group.Update.SupportedCsePersonsWorkSet.FormattedName =
              TrimEnd(local.Supported.FirstName) + " " + TrimEnd
              (local.Supported.MiddleInitial) + " " + local.Supported.LastName;
          }

          if (!IsEmpty(local.Eab.Type1))
          {
            var field1 =
              GetField(export.Group.Item.SupportedCsePerson, "number");

            field1.Error = true;

            ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
          }
        }
      }
    }

    if (Equal(global.Command, "LIST") && !IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(export.HistoryIndicator.Flag) == 'N' && !
        Lt(local.Current.Date, export.ObligationCreatedDate.Date) && Lt
        (local.Blank.Date, export.ObligationCreatedDate.Date))
      {
        var field1 = GetField(export.HistoryIndicator, "flag");

        field1.Color = "green";
        field1.Protected = false;

        var field2 = GetField(export.ObligationPaymentSchedule, "startDt");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.DiscontinueDate, "date");

        field3.Color = "green";
        field3.Protected = false;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          var field4 =
            GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

          field4.Color = "green";
          field4.Protected = false;
        }
      }

      if (AsChar(export.HistoryIndicator.Flag) == 'Y' && AsChar
        (export.Obligation.HistoryInd) == 'Y')
      {
        var field1 = GetField(export.HistoryIndicator, "flag");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.ObligationPaymentSchedule, "startDt");

        field2.Color = "cyan";
        field2.Protected = true;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          var field3 =
            GetField(export.Group.Item.AccrualInstructions, "discontinueDt");

          field3.Color = "cyan";
          field3.Protected = true;
        }
      }

      if (!IsEmpty(export.HiddenIntstInfo.OtherStateAbbr))
      {
        var field1 = GetField(export.Obligation, "otherStateAbbr");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.InterstateRequest, "country");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.InterstateRequest, "tribalAgency");

        field3.Color = "cyan";
        field3.Protected = true;
      }

      if (!IsEmpty(export.HiddenInterstateRequest.Country))
      {
        var field1 = GetField(export.Obligation, "otherStateAbbr");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.InterstateRequest, "country");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.InterstateRequest, "tribalAgency");

        field3.Color = "cyan";
        field3.Protected = true;
      }

      if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
      {
        var field1 = GetField(export.Obligation, "otherStateAbbr");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.InterstateRequest, "country");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.InterstateRequest, "tribalAgency");

        field3.Color = "cyan";
        field3.Protected = true;
      }

      if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
      {
        var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Obligation, "orderTypeCode");

        field2.Color = "cyan";
        field2.Protected = true;
      }

      if (AsChar(export.HistoryIndicator.Flag) == 'Y')
      {
        var field1 = GetField(export.HistoryIndicator, "flag");

        field1.Color = "cyan";
        field1.Protected = true;
      }

      if (AsChar(export.HiddenIntstInfo.OrderTypeCode) == 'K')
      {
        if (export.Obligation.SystemGeneratedIdentifier > 0 || AsChar
          (export.HistoryIndicator.Flag) == 'Y')
        {
          var field1 = GetField(export.Obligation, "orderTypeCode");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.Obligation, "otherStateAbbr");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "country");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.InterstateRequest, "tribalAgency");

          field5.Color = "cyan";
          field5.Protected = true;
        }
      }
    }

    // : August 10, 99, mfb - If this is a history obligation, and
    //   history command does not indicate an add in progress,
    //   protect the discontinue date.
    if (AsChar(export.HistoryIndicator.Flag) == 'Y' && IsEmpty
      (export.HistoryIndicator.Command))
    {
      var field1 = GetField(export.DiscontinueDate, "date");

      field1.Color = "cyan";
      field1.Protected = true;
    }

    // =================================================
    // 2/17/1999 - Bud Adams  -  If alternate billing address was
    //   designated via the LACT screen, then it must be applied to
    //   all debts for that Legal_Action and must be protected.
    // =================================================
    if (Equal(export.Alternate.Char2, "LE"))
    {
      export.AltAddrPrompt.Text1 = "";

      var field1 = GetField(export.Alternate, "number");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.AltAddrPrompt, "text1");

      field2.Color = "cyan";
      field2.Protected = true;
    }

    // =================================================
    // 2/24/1999 - bud adams  -  Conversion accrual arrears-only
    //   debts will not have frequency period code and DOM/DOW
    //   valued in Legal_Action_Detail (LDET) and people won't be
    //   assigned (LOPS).  No processing of this obligation will be
    //   permitted until those things are fixed.
    // =================================================
    if (IsExitState("FN0000_CONVRSN_OBLIG_UPDATE_LEGL") || IsExitState
      ("FN0000_CONVRSN_OBLIG_UPDATE_LOPS"))
    {
      export.HiddenNoAction.Flag = "Y";

      var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Obligation, "otherStateAbbr");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.InterstateRequest, "country");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.InterstateRequest, "tribalAgency");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.Obligation, "orderTypeCode");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.Alternate, "number");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.AltAddrPrompt, "text1");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.HistoryIndicator, "flag");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 = GetField(export.Obligation, "description");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 = GetField(export.DiscontinueDate, "date");

      field10.Color = "cyan";
      field10.Protected = true;
    }

    // *** CQ#5065 Changes Begin Here ***
    if (IsExitState("FN0000_INTST_PGM_FND_ON_PEPR") || IsExitState
      ("FN0000_INTST_PGM_NOT_FND_ON_PEPR") || IsExitState
      ("FN0000_NON_INTST_PGM_FND_ON_PEPR") || IsExitState
      ("FN0000_NON_INTST_PGM_NT_FND_PEPR"))
    {
      if (IsExitState("FN0000_INTST_PGM_NOT_FND_ON_PEPR") || IsExitState
        ("FN0000_NON_INTST_PGM_FND_ON_PEPR"))
      {
        var field4 = GetField(export.InterstateRequest, "otherStateCaseId");

        field4.Error = true;

        var field5 = GetField(export.Obligation, "otherStateAbbr");

        field5.Error = true;

        var field6 = GetField(export.InterstateRequest, "country");

        field6.Error = true;

        var field7 = GetField(export.InterstateRequest, "tribalAgency");

        field7.Error = true;
      }

      var field1 = GetField(export.Obligation, "orderTypeCode");

      field1.Error = true;

      // *** The Accrual Start date is unprotected so that the user can key the 
      // date again ***
      var field2 = GetField(export.ObligationPaymentSchedule, "startDt");

      field2.Color = "green";
      field2.Protected = false;

      var field3 = GetField(export.ObligationPaymentSchedule, "startDt");

      field3.Error = true;
    }

    // *** CQ#5065 Changes End  Here ***
  }

  private static void MoveAccrualInstructions(AccrualInstructions source,
    AccrualInstructions target)
  {
    target.DiscontinueDt = source.DiscontinueDt;
    target.LastAccrualDt = source.LastAccrualDt;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCommon1(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCommon2(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Dob = source.Dob;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
    target.Char2 = source.Char2;
  }

  private static void MoveCsePersonsWorkSet5(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Flag = source.Flag;
  }

  private static void MoveCursorPosition(CursorPosition source,
    CursorPosition target)
  {
    target.Row = source.Row;
    target.Column = source.Column;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveExport1ToGroup(FnEstablishAccrObligDetail.Import.
    ExportGroup source, Export.GroupGroup target)
  {
    target.Zdel.Code = source.ExportProgram.Code;
    MoveCommon1(source.ExportGrpSel, target.Sel);
    target.SupportedCsePerson.Number =
      source.ExportGrpSupportedCsePerson.Number;
    target.AccrualSuspended.Flag = source.ExportGrpSupportdPrmpt.Flag;
    MoveCsePersonsWorkSet3(source.ExportGrpSupportedCsePersonsWorkSet,
      target.SupportedCsePersonsWorkSet);
    target.Case1.Number = source.ExportCase.Number;
    target.ProratePercentage.Percentage =
      source.ExportGrpProratePercnt.Percentage;
    MoveObligationTransaction1(source.ExportObligationTransaction,
      target.ObligationTransaction);
    target.ProgramScreenAttributes.ProgramTypeInd =
      source.ExportHidden.ProgramTypeInd;
    target.ServiceProvider.UserId = source.ExportServiceProvider.UserId;
    target.AccrualInstructions.DiscontinueDt =
      source.ExportAccrualInstructions.DiscontinueDt;
    target.HiddenConcurrent.SystemGeneratedIdentifier =
      source.ExportGrpConcurrent.SystemGeneratedIdentifier;
    target.Previous.Amount = source.ExportGrpPrev.Amount;
    target.SuspendAccrual.Assign(source.ExportGrpSuspendAcrual);
    target.Hidden.DiscontinueDt = source.ExportGrpHid.DiscontinueDt;
  }

  private static void MoveGroup1(Export.GroupGroup source,
    FnUpdateAccruingObligation.Import.GroupGroup target)
  {
    target.Program.Code = source.Zdel.Code;
    MoveCommon1(source.Sel, target.Sel);
    target.SupportedCsePerson.Number = source.SupportedCsePerson.Number;
    target.Prompt.Flag = source.AccrualSuspended.Flag;
    MoveCsePersonsWorkSet3(source.SupportedCsePersonsWorkSet,
      target.SupportedCsePersonsWorkSet);
    target.Case1.Number = source.Case1.Number;
    target.ProratePercentage.Percentage = source.ProratePercentage.Percentage;
    MoveObligationTransaction1(source.ObligationTransaction,
      target.ObligationTransaction);
    target.ProgramScreenAttributes.ProgramTypeInd =
      source.ProgramScreenAttributes.ProgramTypeInd;
    target.ServiceProvider.UserId = source.ServiceProvider.UserId;
    target.AccrualInstructions.DiscontinueDt =
      source.AccrualInstructions.DiscontinueDt;
    target.HiddenConcurrent.SystemGeneratedIdentifier =
      source.HiddenConcurrent.SystemGeneratedIdentifier;
    target.Previous.Amount = source.Previous.Amount;
    target.SuspendAccrual.Assign(source.SuspendAccrual);
    target.Hidden.DiscontinueDt = source.Hidden.DiscontinueDt;
  }

  private static void MoveGroup3(FnReadLegalDetailForOblig.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.Zdel.Code = source.Zdel.Code;
    MoveCommon1(source.Sel, target.Sel);
    target.SupportedCsePerson.Number = source.SupportedCsePerson.Number;
    target.AccrualSuspended.Flag = source.SupportedPrompt.Flag;
    MoveCsePersonsWorkSet3(source.SupportedCsePersonsWorkSet,
      target.SupportedCsePersonsWorkSet);
    target.Case1.Number = source.Case1.Number;
    target.ProratePercentage.Percentage = source.ProratePercentage.Percentage;
    MoveObligationTransaction1(source.ObligationTransaction,
      target.ObligationTransaction);
    target.ProgramScreenAttributes.ProgramTypeInd =
      source.ProgramScreenAttributes.ProgramTypeInd;
    target.ServiceProvider.UserId = source.ServiceProvider.UserId;
    MoveAccrualInstructions(source.AccrualInstructions,
      target.AccrualInstructions);
    target.HiddenConcurrent.SystemGeneratedIdentifier =
      source.HiddenConcurrent.SystemGeneratedIdentifier;
    target.Previous.Amount = source.Previous.Amount;
    target.SuspendAccrual.Assign(source.ZdelExportGrpDesigPayee);
    MoveAccrualInstructions(source.Hidden, target.Hidden);
  }

  private static void MoveGroup4(FnUpdateAccruingObligation.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.Zdel.Code = source.Program.Code;
    MoveCommon1(source.Sel, target.Sel);
    target.SupportedCsePerson.Number = source.SupportedCsePerson.Number;
    target.AccrualSuspended.Flag = source.Prompt.Flag;
    MoveCsePersonsWorkSet3(source.SupportedCsePersonsWorkSet,
      target.SupportedCsePersonsWorkSet);
    target.Case1.Number = source.Case1.Number;
    target.ProratePercentage.Percentage = source.ProratePercentage.Percentage;
    MoveObligationTransaction1(source.ObligationTransaction,
      target.ObligationTransaction);
    target.ProgramScreenAttributes.ProgramTypeInd =
      source.ProgramScreenAttributes.ProgramTypeInd;
    target.ServiceProvider.UserId = source.ServiceProvider.UserId;
    target.AccrualInstructions.DiscontinueDt =
      source.AccrualInstructions.DiscontinueDt;
    target.HiddenConcurrent.SystemGeneratedIdentifier =
      source.HiddenConcurrent.SystemGeneratedIdentifier;
    target.Previous.Amount = source.Previous.Amount;
    target.SuspendAccrual.Assign(source.SuspendAccrual);
    target.Hidden.DiscontinueDt = source.Hidden.DiscontinueDt;
  }

  private static void MoveGroup2(FnReadAccruingObligation.Export.
    Group2Group source, Export.GroupGroup target)
  {
    target.Zdel.Code = source.ZdelExportGrp2.Code;
    MoveCommon1(source.Grp2Common, target.Sel);
    target.SupportedCsePerson.Number = source.Supported2CsePerson.Number;
    target.AccrualSuspended.Flag = source.Grp2AccrualSuspended.Flag;
    MoveCsePersonsWorkSet3(source.Supported2CsePersonsWorkSet,
      target.SupportedCsePersonsWorkSet);
    target.Case1.Number = source.Grp2Case.Number;
    target.ProratePercentage.Percentage = source.ProratePercentage2.Percentage;
    MoveObligationTransaction1(source.Grp2ObligationTransaction,
      target.ObligationTransaction);
    target.ProgramScreenAttributes.ProgramTypeInd =
      source.Grp2ProgramScreenAttributes.ProgramTypeInd;
    target.ServiceProvider.UserId = source.Grp2ServiceProvider.UserId;
    MoveAccrualInstructions(source.Grp2AccrualInstructions,
      target.AccrualInstructions);
    target.HiddenConcurrent.SystemGeneratedIdentifier =
      source.ConcurrentObligor2.SystemGeneratedIdentifier;
    target.Previous.Amount = source.Previous2.Amount;
    target.SuspendAccrual.Assign(source.Zdel);
    MoveAccrualInstructions(source.Grp2Hidden, target.Hidden);
  }

  private static void MoveGroupToExport1(Export.GroupGroup source,
    FnEstablishAccrObligDetail.Import.ExportGroup target)
  {
    target.ExportProgram.Code = source.Zdel.Code;
    MoveCommon1(source.Sel, target.ExportGrpSel);
    target.ExportGrpSupportedCsePerson.Number =
      source.SupportedCsePerson.Number;
    target.ExportGrpSupportdPrmpt.Flag = source.AccrualSuspended.Flag;
    MoveCsePersonsWorkSet3(source.SupportedCsePersonsWorkSet,
      target.ExportGrpSupportedCsePersonsWorkSet);
    target.ExportCase.Number = source.Case1.Number;
    target.ExportGrpProratePercnt.Percentage =
      source.ProratePercentage.Percentage;
    MoveObligationTransaction1(source.ObligationTransaction,
      target.ExportObligationTransaction);
    target.ExportHidden.ProgramTypeInd =
      source.ProgramScreenAttributes.ProgramTypeInd;
    target.ExportServiceProvider.UserId = source.ServiceProvider.UserId;
    target.ExportAccrualInstructions.DiscontinueDt =
      source.AccrualInstructions.DiscontinueDt;
    target.ExportGrpConcurrent.SystemGeneratedIdentifier =
      source.HiddenConcurrent.SystemGeneratedIdentifier;
    target.ExportGrpPrev.Amount = source.Previous.Amount;
    target.ExportGrpSuspendAcrual.Assign(source.SuspendAccrual);
    target.ExportGrpHid.DiscontinueDt = source.Hidden.DiscontinueDt;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveInterstateRequest(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
    target.PaymentLocation = source.PaymentLocation;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.Type1 = source.Type1;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction4(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction5(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction6(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalActionDetail(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.FreqPeriodCode = source.FreqPeriodCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.PeriodInd = source.PeriodInd;
    target.Number = source.Number;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LastTran = source.LastTran;
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
  }

  private static void MoveObligation1(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation2(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation3(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation4(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation5(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation6(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation7(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
  }

  private static void MoveObligationPaymentSchedule1(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.Amount = source.Amount;
    target.StartDt = source.StartDt;
  }

  private static void MoveObligationPaymentSchedule2(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
  }

  private static void MoveObligationTransaction1(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction2(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabCursorPosition()
  {
    var useImport = new EabCursorPosition.Import();
    var useExport = new EabCursorPosition.Export();

    MoveCursorPosition(local.CursorPosition, useExport.CursorPosition);

    Call(EabCursorPosition.Execute, useImport, useExport);

    MoveCursorPosition(useExport.CursorPosition, local.CursorPosition);
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnAddAccrualSuspension()
  {
    var useImport = new FnAddAccrualSuspension.Import();
    var useExport = new FnAddAccrualSuspension.Export();

    useImport.AccrualSuspension.Assign(local.AccrualSuspension);
    useImport.SuspendFromDayOne.Flag = local.SuspendAccrual.Flag;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      export.Group.Item.ObligationTransaction.SystemGeneratedIdentifier;

    Call(FnAddAccrualSuspension.Execute, useImport, useExport);
  }

  private void UseFnCabCheckAltAddr()
  {
    var useImport = new FnCabCheckAltAddr.Import();
    var useExport = new FnCabCheckAltAddr.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.Alternate.Number = export.Alternate.Number;

    Call(FnCabCheckAltAddr.Execute, useImport, useExport);
  }

  private void UseFnCheckExistenceOfObligation1()
  {
    var useImport = new FnCheckExistenceOfObligation.Import();
    var useExport = new FnCheckExistenceOfObligation.Export();

    MoveObligationTransaction2(local.HardcodeOtrnDtAccrualInstruc,
      useImport.HcOtrnDtAccrualInsruc);
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.HcLapObligorAcctType.AccountType =
      local.HardcodeLapObligor.AccountType;
    useImport.HcLapSupportedAcctTyp.AccountType =
      local.HardcodeSupported.AccountType;
    useImport.Maximum.Date = local.Max.Date;
    useImport.Current.Date = local.Current.Date;
    useImport.StartDate.StartDt = export.ObligationPaymentSchedule.StartDt;
    useImport.Common.Command = local.Common.Command;
    useImport.EndDate.Date = export.DiscontinueDate.Date;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    MoveObligationType(export.ObligationType, useImport.ObligationType);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.BeforeLink.Command = export.BeforeLink.Command;
    useImport.Concurrent.Number = export.ConcurrentObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;

    Call(FnCheckExistenceOfObligation.Execute, useImport, useExport);

    local.CheckObligationExistence.Flag = useExport.Common.Flag;
    export.InterstateDebtExists.Flag = useExport.ExistingInterstateDebt.Flag;
    export.HiddenPrevious.EndDt = useExport.Previous.EndDt;
    export.ObligorCsePerson.Number = useExport.Obligor.Number;
    export.ObligationType.Assign(useExport.ObligationType);
    export.Obligation.SystemGeneratedIdentifier =
      useExport.Obligation.SystemGeneratedIdentifier;
  }

  private void UseFnCheckExistenceOfObligation2()
  {
    var useImport = new FnCheckExistenceOfObligation.Import();
    var useExport = new FnCheckExistenceOfObligation.Export();

    MoveObligationTransaction2(local.HardcodeOtrnDtAccrualInstruc,
      useImport.HcOtrnDtAccrualInsruc);
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.HcLapObligorAcctType.AccountType =
      local.HardcodeLapObligor.AccountType;
    useImport.HcLapSupportedAcctTyp.AccountType =
      local.HardcodeSupported.AccountType;
    useImport.Maximum.Date = local.Max.Date;
    useImport.Current.Date = local.Current.Date;
    useImport.StartDate.StartDt = export.ObligationPaymentSchedule.StartDt;
    useImport.Common.Command = local.Common.Command;
    useImport.EndDate.Date = export.DiscontinueDate.Date;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    MoveObligationType(export.ObligationType, useImport.ObligationType);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.BeforeLink.Command = export.BeforeLink.Command;
    useImport.Concurrent.Number = export.ConcurrentObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;

    Call(FnCheckExistenceOfObligation.Execute, useImport, useExport);

    local.CheckObligationExistence.Flag = useExport.Common.Flag;
  }

  private void UseFnCheckExistenceOfObligation3()
  {
    var useImport = new FnCheckExistenceOfObligation.Import();
    var useExport = new FnCheckExistenceOfObligation.Export();

    MoveObligationTransaction2(local.HardcodeOtrnDtAccrualInstruc,
      useImport.HcOtrnDtAccrualInsruc);
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.HcLapObligorAcctType.AccountType =
      local.HardcodeLapObligor.AccountType;
    useImport.HcLapSupportedAcctTyp.AccountType =
      local.HardcodeSupported.AccountType;
    useImport.Maximum.Date = local.Max.Date;
    useImport.Current.Date = local.Current.Date;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.StartDate.StartDt = export.ObligationPaymentSchedule.StartDt;
    useImport.Common.Command = local.Common.Command;
    useImport.EndDate.Date = export.DiscontinueDate.Date;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    MoveObligationType(export.ObligationType, useImport.ObligationType);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.BeforeLink.Command = export.BeforeLink.Command;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;

    Call(FnCheckExistenceOfObligation.Execute, useImport, useExport);

    export.ObligationType.Assign(useExport.ObligationType);
    local.CheckObligationExistence.Flag = useExport.Common.Flag;
    export.InterstateDebtExists.Flag = useExport.ExistingInterstateDebt.Flag;
    export.HiddenPrevious.EndDt = useExport.Previous.EndDt;
  }

  private void UseFnCreateInterstateRqstOblign1()
  {
    var useImport = new FnCreateInterstateRqstOblign.Import();
    var useExport = new FnCreateInterstateRqstOblign.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.Max.Date = local.Max.Date;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;

    Call(FnCreateInterstateRqstOblign.Execute, useImport, useExport);
  }

  private void UseFnCreateInterstateRqstOblign2()
  {
    var useImport = new FnCreateInterstateRqstOblign.Import();
    var useExport = new FnCreateInterstateRqstOblign.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.Max.Date = local.Max.Date;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Concurrent.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.ConcurrentObligorCsePerson.Number;
    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;

    Call(FnCreateInterstateRqstOblign.Execute, useImport, useExport);
  }

  private void UseFnDeleteObligation()
  {
    var useImport = new FnDeleteObligation.Import();
    var useExport = new FnDeleteObligation.Export();

    useImport.HcOrrJointAndSeveral.SequentialGeneratedIdentifier =
      local.HardcodeOrrJointAndSeveral.SequentialGeneratedIdentifier;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    useImport.ObligationType.Assign(export.ObligationType);
    MoveObligation7(export.Obligation, useImport.Obligation);

    Call(FnDeleteObligation.Execute, useImport, useExport);
  }

  private void UseFnEstAccruingObligation()
  {
    var useImport = new FnEstAccruingObligation.Import();
    var useExport = new FnEstAccruingObligation.Export();

    useImport.HcObligJointSevConcur.PrimarySecondaryCode =
      local.HardcodeObligJointSevlConcur.PrimarySecondaryCode;
    useImport.HcOrrJointSeveral.SequentialGeneratedIdentifier =
      local.HardcodeOrrJointAndSeveral.SequentialGeneratedIdentifier;
    useImport.Max.Date = local.Max.Date;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtCAccruingClassifi.Classification =
      local.HardcodeAccruing.Classification;
    useImport.HcOtCFeesClassificati.Classification =
      local.HardcodeOtCFeesClassificatio.Classification;
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodeOtCRecoverClassifica.Classification;
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.HistoryIndicator.Flag = export.HistoryIndicator.Flag;
    useImport.AltAdd.Number = export.Alternate.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    MoveObligationType(export.ObligationType, useImport.ObligationType);
    MoveObligation3(export.Obligation, useImport.Obligation);
    useImport.ObligationPaymentSchedule.
      Assign(export.ObligationPaymentSchedule);
    useImport.Discontinue.Date = export.DiscontinueDate.Date;
    useImport.Case1.Number = export.CaseId.Number;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    useImport.ConcurrentObligor.Number =
      export.ConcurrentObligorCsePerson.Number;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;

    Call(FnEstAccruingObligation.Execute, useImport, useExport);

    MoveObligation2(useExport.Obligation, export.Obligation);
    export.Concurrent.SystemGeneratedIdentifier =
      useExport.Concurrent.SystemGeneratedIdentifier;
  }

  private void UseFnEstablishAccrObligDetail()
  {
    var useImport = new FnEstablishAccrObligDetail.Import();
    var useExport = new FnEstablishAccrObligDetail.Export();

    useImport.HardcodeObligorLap.AccountType =
      local.HardcodeLapObligor.AccountType;
    useImport.OtrrConcurrentObligatio.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.Max.Date = local.Max.Date;
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;
    useImport.HcOt718BUraJudgement.SystemGeneratedIdentifier =
      local.HardcodeOt718BUraJudgement.SystemGeneratedIdentifier;
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodeOtCRecoverClassifica.Classification;
    useImport.HcOtCFeesClassificati.Classification =
      local.HardcodeOtCFeesClassificatio.Classification;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcCpaSupportedPerson.Type1 =
      local.HardcodeCpaSupportedPerson.Type1;
    MoveObligationTransaction2(local.HardcodeOtrnDtVoluntary,
      useImport.HcOtrnDtVoluntary);
    useImport.HcDdshActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    MoveObligationTransaction2(local.HardcodeOtrnDtAccrualInstruc,
      useImport.HcOtrnDtAccrualInstru);
    useImport.HistoryIndicator.Flag = export.HistoryIndicator.Flag;
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    useImport.ConcurrentCsePerson.Number =
      export.ConcurrentObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.Assign(export.ObligationType);
    useImport.ConcurrentObligation.SystemGeneratedIdentifier =
      export.Concurrent.SystemGeneratedIdentifier;
    useImport.LegalAction.Assign(export.LegalAction);
    useImport.ObligationPaymentSchedule.
      Assign(export.ObligationPaymentSchedule);
    export.Group.CopyTo(useImport.Export1, MoveGroupToExport1);
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;

    Call(FnEstablishAccrObligDetail.Execute, useImport, useExport);

    useImport.Export1.CopyTo(export.Group, MoveExport1ToGroup);
  }

  private void UseFnGetOblFromHistMonaNxtran()
  {
    var useImport = new FnGetOblFromHistMonaNxtran.Import();
    var useExport = new FnGetOblFromHistMonaNxtran.Export();

    useImport.NextTranInfo.InfrastructureId =
      export.HiddenNextTranInfo.InfrastructureId;

    Call(FnGetOblFromHistMonaNxtran.Execute, useImport, useExport);

    MoveObligationType(useExport.ObligationType, export.ObligationType);
    MoveLegalAction3(useExport.LegalAction, export.LegalAction);
    export.Obligation.SystemGeneratedIdentifier =
      useExport.Obligation.SystemGeneratedIdentifier;
    export.LegalActionDetail.Number = useExport.LegalActionDetail.Number;
  }

  private void UseFnHardcodeLegal()
  {
    var useImport = new FnHardcodeLegal.Import();
    var useExport = new FnHardcodeLegal.Export();

    Call(FnHardcodeLegal.Execute, useImport, useExport);

    local.HardcodeSupported.AccountType = useExport.Supported.AccountType;
    local.HardcodeLapObligor.AccountType = useExport.Obligor.AccountType;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObligJointSevlConcur.PrimarySecondaryCode =
      useExport.ObligJointSeveralConcurrent.PrimarySecondaryCode;
    local.HardcodeOpsCMonthly.FrequencyCode =
      useExport.OpsCMonthly.FrequencyCode;
    local.HardcodeOpsCBiWeekly.FrequencyCode =
      useExport.OpsCBiWeekly.FrequencyCode;
    local.HardcodeOpsCWeekly.FrequencyCode = useExport.OpsCWeekly.FrequencyCode;
    local.HardcodeOpsCSemiMonthly.FrequencyCode =
      useExport.OpsCSemiMonthly.FrequencyCode;
    local.HardcodeOpsCBiMonthly.FrequencyCode =
      useExport.OpsCBiMonthly.FrequencyCode;
    local.HardcodeDdshActiveStatus.Code = useExport.DdshActiveStatus.Code;
    MoveObligationTransaction2(useExport.OtrnDtVoluntary,
      local.HardcodeOtrnDtVoluntary);
    local.HardcodeCpaSupportedPerson.Type1 = useExport.CpaSupportedPerson.Type1;
    local.HardcodeOtCFeesClassificatio.Classification =
      useExport.OtCFeesClassification.Classification;
    local.HardcodeOtCRecoverClassifica.Classification =
      useExport.OtCRecoverClassification.Classification;
    MoveObligationTransaction2(useExport.OtrnDtDebtDetail,
      local.HardcodeOtrnDtDebtDetail);
    local.HardcodeOt718BUraJudgement.SystemGeneratedIdentifier =
      useExport.Ot718BUraJudgement.SystemGeneratedIdentifier;
    local.HardcodeOtCVoluntaryClassifi.Classification =
      useExport.OtCVoluntaryClassification.Classification;
    MoveObligationTransaction2(useExport.OtrnDtAccrualInstructions,
      local.HardcodeOtrnDtAccrualInstruc);
    local.HardcodeOtrnTDebt.Type1 = useExport.OtrnTDebt.Type1;
    local.HardcodePgmNonAdcFosterCare.ProgramTypeInd =
      useExport.PgmNonAdcFosterCare.ProgramTypeInd;
    local.HardcodePgmAdc.ProgramTypeInd = useExport.PgmAdc.ProgramTypeInd;
    local.HardcodePgmAdcFosterCare.ProgramTypeInd =
      useExport.PgmAdcFosterCare.ProgramTypeInd;
    local.HardcodeOrrJointAndSeveral.SequentialGeneratedIdentifier =
      useExport.OrrJointSeveral.SequentialGeneratedIdentifier;
    local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier =
      useExport.OtrrConcurrentObligation.SystemGeneratedIdentifier;
    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
    local.HardcodeAccruing.Classification =
      useExport.OtCAccruingClassification.Classification;
  }

  private void UseFnProtectCollectionsForOblig()
  {
    var useImport = new FnProtectCollectionsForOblig.Import();
    var useExport = new FnProtectCollectionsForOblig.Export();

    useImport.Persistent.Assign(entities.OtherView);
    useImport.CreateObCollProtHist.Flag = local.Common.Flag;
    useImport.ObligCollProtectionHist.Assign(local.ObligCollProtectionHist);

    Call(FnProtectCollectionsForOblig.Execute, useImport, useExport);

    entities.OtherView.Assign(useImport.Persistent);
    local.CollsFndToProtect.Flag = useExport.CollsFndToProtect.Flag;
    local.ObCollProtHistCreated.Flag = useExport.ObCollProtHistCreated.Flag;
  }

  private void UseFnReadAccruingObligation()
  {
    var useImport = new FnReadAccruingObligation.Import();
    var useExport = new FnReadAccruingObligation.Export();

    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOrrJointSeveralOblg.SequentialGeneratedIdentifier =
      local.HardcodeOrrJointAndSeveral.SequentialGeneratedIdentifier;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.Max.Date = local.Max.Date;
    useImport.Current.Date = local.Current.Date;
    MoveLegalAction5(export.LegalAction, useImport.LegalAction);
    useImport.ObligorCsePerson.Number = export.ObligorCsePerson.Number;
    useImport.ObligorObligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.LegalActionDetail.Assign(export.LegalActionDetail);

    Call(FnReadAccruingObligation.Execute, useImport, useExport);

    useExport.Group2.CopyTo(export.Group, MoveGroup2);
    MoveInterstateRequest(useExport.InterstateRequest, export.InterstateRequest);
      
    MoveCsePersonsWorkSet4(useExport.Alternate, export.Alternate);
    MoveCsePersonsWorkSet2(useExport.Obligor, export.ObligorCsePersonsWorkSet);
    export.ConcurrentObligorCsePerson.Number =
      useExport.ConcurrentObligorCsePerson.Number;
    MoveCsePersonsWorkSet3(useExport.ConcurrentObligorCsePersonsWorkSet,
      export.ConcurrentObligorCsePersonsWorkSet);
    MoveObligation1(useExport.Obligation, export.Obligation);
    export.Concurrent.SystemGeneratedIdentifier =
      useExport.Concurrent.SystemGeneratedIdentifier;
    export.ObligationType.Assign(useExport.ObligationType);
    export.ObligationPaymentSchedule.
      Assign(useExport.ObligationPaymentSchedule);
    export.FrequencyWorkSet.Assign(useExport.FrequencyWorkSet);
    export.DiscontinueDate.Date = useExport.DiscontinueDate.Date;
    MoveLegalAction6(useExport.LegalAction, export.LegalAction);
    export.AccrualAmount.TotalCurrency = useExport.AccrualAmount.TotalCurrency;
    export.AccuralSuspendedInd.Flag = useExport.AccrualSuspendedInd.Flag;
    export.InterestSuspendedInd.Flag = useExport.InterestSuspendedInd.Flag;
    export.ManualDistributionInd.Flag = useExport.ManualDistributionInd.Flag;
    export.ObligationActive.Flag = useExport.ObligationActiveInd.Flag;
    export.AccrualInstructions.LastAccrualDt =
      useExport.LastAccrual.LastAccrualDt;
    MoveLegalActionDetail(useExport.LegalActionDetail, export.LegalActionDetail);
      
  }

  private void UseFnReadLegalDetailForOblig()
  {
    var useImport = new FnReadLegalDetailForOblig.Import();
    var useExport = new FnReadLegalDetailForOblig.Export();

    useImport.HcLapSupported.AccountType = local.HardcodeSupported.AccountType;
    useImport.HcLapObligor.AccountType = local.HardcodeLapObligor.AccountType;
    useImport.HcOpsCWeeklyFreqCode.FrequencyCode =
      local.HardcodeOpsCWeekly.FrequencyCode;
    useImport.HcPgmNonAdcFosterCar.ProgramTypeInd =
      local.HardcodePgmNonAdcFosterCare.ProgramTypeInd;
    useImport.HdPgmAdc.ProgramTypeInd = local.HardcodePgmAdc.ProgramTypeInd;
    useImport.HcPgmAdcFosterCare.ProgramTypeInd =
      local.HardcodePgmAdcFosterCare.ProgramTypeInd;
    useImport.Current.Date = local.Current.Date;
    useImport.Maximum.Date = local.Max.Date;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;

    Call(FnReadLegalDetailForOblig.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup3);
    MoveObligation6(useExport.InterstateInd, export.Obligation);
    export.DiscontinueDate.Date = useExport.Discontinue.Date;
    MoveLegalAction4(useExport.LegalAction, export.LegalAction);
    export.FrequencyWorkSet.Assign(useExport.FrequencyWorkSet);
    export.ObligationType.Assign(useExport.ObligationType);
    export.ConcurrentObligorCsePerson.Number =
      useExport.ConcurrentObligorCsePerson.Number;
    export.ObligorCsePerson.Number = useExport.ObligorCsePerson.Number;
    export.ManualDistributionInd.Flag = useExport.ManualDistb.Flag;
    export.AccuralSuspendedInd.Flag = useExport.SuspendAccrual.Flag;
    export.InterestSuspendedInd.Flag = useExport.SuspendInt.Flag;
    MoveCsePersonsWorkSet3(useExport.ConcurrentObligorCsePersonsWorkSet,
      export.ConcurrentObligorCsePersonsWorkSet);
    MoveCsePersonsWorkSet3(useExport.ObligorCsePersonsWorkSet,
      export.ObligorCsePersonsWorkSet);
    MoveObligationPaymentSchedule1(useExport.ObligationPaymentSchedule,
      export.ObligationPaymentSchedule);
    MoveCsePersonsWorkSet4(useExport.Alternate, export.Alternate);
  }

  private void UseFnRetrieveInterstateRequest1()
  {
    var useImport = new FnRetrieveInterstateRequest.Import();
    var useExport = new FnRetrieveInterstateRequest.Export();

    MoveObligation5(export.Obligation, useImport.Obligor);
    useImport.InterstateRequest.Assign(export.InterstateRequest);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.Current.Date = local.Current.Date;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;

    Call(FnRetrieveInterstateRequest.Execute, useImport, useExport);

    export.InterstateRequest.Assign(useExport.InterstateRequest);
    export.Country.Description = useExport.Country.Description;
  }

  private void UseFnRetrieveInterstateRequest2()
  {
    var useImport = new FnRetrieveInterstateRequest.Import();
    var useExport = new FnRetrieveInterstateRequest.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.InterstateRequest.Assign(export.InterstateRequest);
    useImport.CsePerson.Number = export.ConcurrentObligorCsePerson.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    MoveObligation5(export.Obligation, useImport.Obligor);
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;

    Call(FnRetrieveInterstateRequest.Execute, useImport, useExport);

    export.Country.Description = useExport.Country.Description;
    export.InterstateRequest.Assign(useExport.InterstateRequest);
  }

  private void UseFnUpdateAccruingObligation()
  {
    var useImport = new FnUpdateAccruingObligation.Import();
    var useExport = new FnUpdateAccruingObligation.Export();

    useImport.HcOtDebtType.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.HcCpaSupported.Type1 = local.HardcodeCpaSupportedPerson.Type1;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.Maximum.Date = local.Max.Date;
    useImport.UpdateInterstateInfo.Flag = local.UpdateInterstateInfo.Flag;
    MoveCsePersonsWorkSet5(export.Alternate, useImport.AltAdd);
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.ConcurrentCsePerson.Number =
      export.ConcurrentObligorCsePerson.Number;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    MoveObligation4(export.Obligation, useImport.Obligation);
    useImport.ConcurrentObligation.SystemGeneratedIdentifier =
      export.Concurrent.SystemGeneratedIdentifier;
    useImport.ObligationPaymentSchedule.
      Assign(export.ObligationPaymentSchedule);
    MoveDateWorkArea(local.Current, useImport.CurrentDate);
    useImport.Discontinue.Date = export.DiscontinueDate.Date;
    export.Group.CopyTo(useImport.Group, MoveGroup1);
    useImport.Case1.Number = entities.Case1.Number;
    useImport.New1.Assign(export.InterstateRequest);
    MoveLegalAction5(export.LegalAction, useImport.LegalAction);
    useImport.Old.IntHGeneratedId =
      export.HiddenInterstateRequest.IntHGeneratedId;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;

    Call(FnUpdateAccruingObligation.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup4);
    local.AltBillLocError.SelectChar = useExport.AltBillLocError.SelectChar;
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

    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);
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

    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.CsePersonsWorkSet.Number = export.ObligorCsePersonsWorkSet.Number;
    useImport.LegalAction.Assign(export.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Alternate.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Eab.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet, export.Alternate);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ObligorCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Eab.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet,
      export.ObligorCsePersonsWorkSet);
  }

  private void UseSiReadCsePerson3()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      export.ConcurrentObligorCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Eab.Assign(useExport.AbendData);
    export.ConcurrentObligorCsePersonsWorkSet.
      Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson4()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Supported.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Supported.Assign(useExport.CsePersonsWorkSet);
    local.Eab.Assign(useExport.AbendData);
  }

  private IEnumerable<bool> ReadAccrualInstructionsObligationTransaction()
  {
    entities.AccrualInstructions.Populated = false;
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadAccrualInstructionsObligationTransaction",
      (db, command) =>
      {
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
        db.SetInt32(
          command, "obgGeneratedId",
          export.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", export.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 7);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 8);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 9);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 10);
        entities.AccrualInstructions.Populated = true;
        entities.ObligationTransaction.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", export.ObligorCsePerson.Number);
        db.SetString(
          command, "cspNumber2",
          export.HiddenFlowToPeprCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCollection1()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetDate(command, "date1", local.CollProt.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 16);
        entities.Collection.DistributionMethod = db.GetString(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
      });
  }

  private bool ReadCollection2()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetDate(
          command, "startDt",
          export.ObligationPaymentSchedule.StartDt.GetValueOrDefault());
        db.SetDate(command, "date", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 16);
        entities.Collection.DistributionMethod = db.GetString(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
      });
  }

  private bool ReadObligCollProtectionHist()
  {
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgIdentifier",
          export.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyIdentifier",
          export.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
        db.SetDate(
          command, "deactivationDate", local.Blank.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 1);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 2);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 3);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 4);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 5);
        entities.ObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ObligCollProtectionHist.CpaType);
      });
  }

  private bool ReadObligation1()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
        db.SetString(command, "cpaType", local.HardcodeObligor.Type1);
        db.SetInt32(
          command, "dtyGeneratedId",
          export.ObligationType.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "ladNumber", export.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", export.LegalAction.Identifier);
        db.
          SetInt32(command, "obId", export.Obligation.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 4);
        entities.Obligation.Description = db.GetNullableString(reader, 5);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 6);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 7);
        entities.Obligation.CreatedBy = db.GetString(reader, 8);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 9);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 11);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 12);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 13);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 14);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private IEnumerable<bool> ReadObligation2()
  {
    entities.OtherView.Populated = false;

    return ReadEach("ReadObligation2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OtherView.CpaType = db.GetString(reader, 0);
        entities.OtherView.CspNumber = db.GetString(reader, 1);
        entities.OtherView.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.OtherView.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.OtherView.LgaId = db.GetNullableInt32(reader, 4);
        entities.OtherView.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.OtherView.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.OtherView.LastUpdateTmst = db.GetNullableDateTime(reader, 7);
        entities.OtherView.LastObligationEvent =
          db.GetNullableString(reader, 8);
        entities.OtherView.Populated = true;
        CheckValid<Obligation>("CpaType", entities.OtherView.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.OtherView.PrimarySecondaryCode);

        return true;
      });
  }

  private bool ReadProgram1()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", export.Group.Item.SupportedCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          export.ObligationPaymentSchedule.StartDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;
      });
  }

  private bool ReadProgram2()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", export.Group.Item.SupportedCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          export.ObligationPaymentSchedule.StartDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
      /// A value of Zdel.
      /// </summary>
      [JsonPropertyName("zdel")]
      public Program Zdel
      {
        get => zdel ??= new();
        set => zdel = value;
      }

      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of SupportedCsePerson.
      /// </summary>
      [JsonPropertyName("supportedCsePerson")]
      public CsePerson SupportedCsePerson
      {
        get => supportedCsePerson ??= new();
        set => supportedCsePerson = value;
      }

      /// <summary>
      /// A value of AccrualSuspended.
      /// </summary>
      [JsonPropertyName("accrualSuspended")]
      public Common AccrualSuspended
      {
        get => accrualSuspended ??= new();
        set => accrualSuspended = value;
      }

      /// <summary>
      /// A value of SupportedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("supportedCsePersonsWorkSet")]
      public CsePersonsWorkSet SupportedCsePersonsWorkSet
      {
        get => supportedCsePersonsWorkSet ??= new();
        set => supportedCsePersonsWorkSet = value;
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
      /// A value of ProratePercentage.
      /// </summary>
      [JsonPropertyName("proratePercentage")]
      public Common ProratePercentage
      {
        get => proratePercentage ??= new();
        set => proratePercentage = value;
      }

      /// <summary>
      /// A value of ObligationTransaction.
      /// </summary>
      [JsonPropertyName("obligationTransaction")]
      public ObligationTransaction ObligationTransaction
      {
        get => obligationTransaction ??= new();
        set => obligationTransaction = value;
      }

      /// <summary>
      /// A value of ProgramScreenAttributes.
      /// </summary>
      [JsonPropertyName("programScreenAttributes")]
      public ProgramScreenAttributes ProgramScreenAttributes
      {
        get => programScreenAttributes ??= new();
        set => programScreenAttributes = value;
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
      /// A value of AccrualInstructions.
      /// </summary>
      [JsonPropertyName("accrualInstructions")]
      public AccrualInstructions AccrualInstructions
      {
        get => accrualInstructions ??= new();
        set => accrualInstructions = value;
      }

      /// <summary>
      /// A value of HiddenConcurrent.
      /// </summary>
      [JsonPropertyName("hiddenConcurrent")]
      public ObligationTransaction HiddenConcurrent
      {
        get => hiddenConcurrent ??= new();
        set => hiddenConcurrent = value;
      }

      /// <summary>
      /// A value of Previous.
      /// </summary>
      [JsonPropertyName("previous")]
      public ObligationTransaction Previous
      {
        get => previous ??= new();
        set => previous = value;
      }

      /// <summary>
      /// A value of SuspendAccrual.
      /// </summary>
      [JsonPropertyName("suspendAccrual")]
      public CsePersonsWorkSet SuspendAccrual
      {
        get => suspendAccrual ??= new();
        set => suspendAccrual = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public AccrualInstructions Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program zdel;
      private Common sel;
      private CsePerson supportedCsePerson;
      private Common accrualSuspended;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private Common proratePercentage;
      private ObligationTransaction obligationTransaction;
      private ProgramScreenAttributes programScreenAttributes;
      private ServiceProvider serviceProvider;
      private AccrualInstructions accrualInstructions;
      private ObligationTransaction hiddenConcurrent;
      private ObligationTransaction previous;
      private CsePersonsWorkSet suspendAccrual;
      private AccrualInstructions hidden;
    }

    /// <summary>
    /// A value of HiddenConcurrent.
    /// </summary>
    [JsonPropertyName("hiddenConcurrent")]
    public CsePerson HiddenConcurrent
    {
      get => hiddenConcurrent ??= new();
      set => hiddenConcurrent = value;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of InterstateDebtExists.
    /// </summary>
    [JsonPropertyName("interstateDebtExists")]
    public Common InterstateDebtExists
    {
      get => interstateDebtExists ??= new();
      set => interstateDebtExists = value;
    }

    /// <summary>
    /// A value of HiddenAlternate.
    /// </summary>
    [JsonPropertyName("hiddenAlternate")]
    public CsePersonsWorkSet HiddenAlternate
    {
      get => hiddenAlternate ??= new();
      set => hiddenAlternate = value;
    }

    /// <summary>
    /// A value of HiddenObligorObligation.
    /// </summary>
    [JsonPropertyName("hiddenObligorObligation")]
    public Obligation HiddenObligorObligation
    {
      get => hiddenObligorObligation ??= new();
      set => hiddenObligorObligation = value;
    }

    /// <summary>
    /// A value of ObligationCreatedDate.
    /// </summary>
    [JsonPropertyName("obligationCreatedDate")]
    public DateWorkArea ObligationCreatedDate
    {
      get => obligationCreatedDate ??= new();
      set => obligationCreatedDate = value;
    }

    /// <summary>
    /// A value of HiddenIntstInfo.
    /// </summary>
    [JsonPropertyName("hiddenIntstInfo")]
    public Obligation HiddenIntstInfo
    {
      get => hiddenIntstInfo ??= new();
      set => hiddenIntstInfo = value;
    }

    /// <summary>
    /// A value of HiddenInterstateRequest.
    /// </summary>
    [JsonPropertyName("hiddenInterstateRequest")]
    public InterstateRequest HiddenInterstateRequest
    {
      get => hiddenInterstateRequest ??= new();
      set => hiddenInterstateRequest = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of BeforeLink.
    /// </summary>
    [JsonPropertyName("beforeLink")]
    public Common BeforeLink
    {
      get => beforeLink ??= new();
      set => beforeLink = value;
    }

    /// <summary>
    /// A value of HiddenDiscontinueDate.
    /// </summary>
    [JsonPropertyName("hiddenDiscontinueDate")]
    public DateWorkArea HiddenDiscontinueDate
    {
      get => hiddenDiscontinueDate ??= new();
      set => hiddenDiscontinueDate = value;
    }

    /// <summary>
    /// A value of HiddenObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("hiddenObligationPaymentSchedule")]
    public ObligationPaymentSchedule HiddenObligationPaymentSchedule
    {
      get => hiddenObligationPaymentSchedule ??= new();
      set => hiddenObligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of HistoryIndicator.
    /// </summary>
    [JsonPropertyName("historyIndicator")]
    public Common HistoryIndicator
    {
      get => historyIndicator ??= new();
      set => historyIndicator = value;
    }

    /// <summary>
    /// A value of AssignName.
    /// </summary>
    [JsonPropertyName("assignName")]
    public CsePersonsWorkSet AssignName
    {
      get => assignName ??= new();
      set => assignName = value;
    }

    /// <summary>
    /// A value of AssignPrompt.
    /// </summary>
    [JsonPropertyName("assignPrompt")]
    public TextWorkArea AssignPrompt
    {
      get => assignPrompt ??= new();
      set => assignPrompt = value;
    }

    /// <summary>
    /// A value of Assign1.
    /// </summary>
    [JsonPropertyName("assign1")]
    public ServiceProvider Assign1
    {
      get => assign1 ??= new();
      set => assign1 = value;
    }

    /// <summary>
    /// A value of ZdelImportDesignatedPayee.
    /// </summary>
    [JsonPropertyName("zdelImportDesignatedPayee")]
    public CsePersonsWorkSet ZdelImportDesignatedPayee
    {
      get => zdelImportDesignatedPayee ??= new();
      set => zdelImportDesignatedPayee = value;
    }

    /// <summary>
    /// A value of CaseId.
    /// </summary>
    [JsonPropertyName("caseId")]
    public Case1 CaseId
    {
      get => caseId ??= new();
      set => caseId = value;
    }

    /// <summary>
    /// A value of PayeePrompt.
    /// </summary>
    [JsonPropertyName("payeePrompt")]
    public TextWorkArea PayeePrompt
    {
      get => payeePrompt ??= new();
      set => payeePrompt = value;
    }

    /// <summary>
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public CsePersonsWorkSet Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    /// <summary>
    /// A value of AltAddrPrompt.
    /// </summary>
    [JsonPropertyName("altAddrPrompt")]
    public TextWorkArea AltAddrPrompt
    {
      get => altAddrPrompt ??= new();
      set => altAddrPrompt = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePersonsWorkSet Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorPrompt.
    /// </summary>
    [JsonPropertyName("obligorPrompt")]
    public Common ObligorPrompt
    {
      get => obligorPrompt ??= new();
      set => obligorPrompt = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ConcurrentObligorCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentObligorCsePerson")]
    public CsePerson ConcurrentObligorCsePerson
    {
      get => concurrentObligorCsePerson ??= new();
      set => concurrentObligorCsePerson = value;
    }

    /// <summary>
    /// A value of ConcurrentObligorPrompt.
    /// </summary>
    [JsonPropertyName("concurrentObligorPrompt")]
    public Common ConcurrentObligorPrompt
    {
      get => concurrentObligorPrompt ??= new();
      set => concurrentObligorPrompt = value;
    }

    /// <summary>
    /// A value of ConcurrentObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("concurrentObligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ConcurrentObligorCsePersonsWorkSet
    {
      get => concurrentObligorCsePersonsWorkSet ??= new();
      set => concurrentObligorCsePersonsWorkSet = value;
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
    /// A value of LegalActionPrompt.
    /// </summary>
    [JsonPropertyName("legalActionPrompt")]
    public Common LegalActionPrompt
    {
      get => legalActionPrompt ??= new();
      set => legalActionPrompt = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligationTypePrompt.
    /// </summary>
    [JsonPropertyName("obligationTypePrompt")]
    public Common ObligationTypePrompt
    {
      get => obligationTypePrompt ??= new();
      set => obligationTypePrompt = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
    }

    /// <summary>
    /// A value of FrequencyPrompt.
    /// </summary>
    [JsonPropertyName("frequencyPrompt")]
    public Common FrequencyPrompt
    {
      get => frequencyPrompt ??= new();
      set => frequencyPrompt = value;
    }

    /// <summary>
    /// A value of DiscontinueDate.
    /// </summary>
    [JsonPropertyName("discontinueDate")]
    public DateWorkArea DiscontinueDate
    {
      get => discontinueDate ??= new();
      set => discontinueDate = value;
    }

    /// <summary>
    /// A value of AccrualSuspendedInd.
    /// </summary>
    [JsonPropertyName("accrualSuspendedInd")]
    public Common AccrualSuspendedInd
    {
      get => accrualSuspendedInd ??= new();
      set => accrualSuspendedInd = value;
    }

    /// <summary>
    /// A value of ManualDistributionInd.
    /// </summary>
    [JsonPropertyName("manualDistributionInd")]
    public Common ManualDistributionInd
    {
      get => manualDistributionInd ??= new();
      set => manualDistributionInd = value;
    }

    /// <summary>
    /// A value of InterestSuspendedInd.
    /// </summary>
    [JsonPropertyName("interestSuspendedInd")]
    public Common InterestSuspendedInd
    {
      get => interestSuspendedInd ??= new();
      set => interestSuspendedInd = value;
    }

    /// <summary>
    /// A value of AccrualAmount.
    /// </summary>
    [JsonPropertyName("accrualAmount")]
    public Common AccrualAmount
    {
      get => accrualAmount ??= new();
      set => accrualAmount = value;
    }

    /// <summary>
    /// A value of ObligorObligation.
    /// </summary>
    [JsonPropertyName("obligorObligation")]
    public Obligation ObligorObligation
    {
      get => obligorObligation ??= new();
      set => obligorObligation = value;
    }

    /// <summary>
    /// A value of ConcurrentObligorObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligorObligation")]
    public Obligation ConcurrentObligorObligation
    {
      get => concurrentObligorObligation ??= new();
      set => concurrentObligorObligation = value;
    }

    /// <summary>
    /// A value of ObligationActiveInd.
    /// </summary>
    [JsonPropertyName("obligationActiveInd")]
    public Common ObligationActiveInd
    {
      get => obligationActiveInd ??= new();
      set => obligationActiveInd = value;
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
    /// A value of HiddenObligorCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenObligorCsePerson")]
    public CsePerson HiddenObligorCsePerson
    {
      get => hiddenObligorCsePerson ??= new();
      set => hiddenObligorCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenObligationType.
    /// </summary>
    [JsonPropertyName("hiddenObligationType")]
    public ObligationType HiddenObligationType
    {
      get => hiddenObligationType ??= new();
      set => hiddenObligationType = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

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
    /// A value of HiddenPrevious.
    /// </summary>
    [JsonPropertyName("hiddenPrevious")]
    public ObligationPaymentSchedule HiddenPrevious
    {
      get => hiddenPrevious ??= new();
      set => hiddenPrevious = value;
    }

    /// <summary>
    /// A value of HiddenLegalActionDetail.
    /// </summary>
    [JsonPropertyName("hiddenLegalActionDetail")]
    public LegalActionDetail HiddenLegalActionDetail
    {
      get => hiddenLegalActionDetail ??= new();
      set => hiddenLegalActionDetail = value;
    }

    /// <summary>
    /// A value of HiddenNoAction.
    /// </summary>
    [JsonPropertyName("hiddenNoAction")]
    public Common HiddenNoAction
    {
      get => hiddenNoAction ??= new();
      set => hiddenNoAction = value;
    }

    /// <summary>
    /// A value of HiddenAccrualInstructions.
    /// </summary>
    [JsonPropertyName("hiddenAccrualInstructions")]
    public AccrualInstructions HiddenAccrualInstructions
    {
      get => hiddenAccrualInstructions ??= new();
      set => hiddenAccrualInstructions = value;
    }

    /// <summary>
    /// A value of CountryPrompt.
    /// </summary>
    [JsonPropertyName("countryPrompt")]
    public Common CountryPrompt
    {
      get => countryPrompt ??= new();
      set => countryPrompt = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of HiddenProtectHistory.
    /// </summary>
    [JsonPropertyName("hiddenProtectHistory")]
    public Common HiddenProtectHistory
    {
      get => hiddenProtectHistory ??= new();
      set => hiddenProtectHistory = value;
    }

    /// <summary>
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
    }

    /// <summary>
    /// A value of HiddenFlowToPeprCase.
    /// </summary>
    [JsonPropertyName("hiddenFlowToPeprCase")]
    public Case1 HiddenFlowToPeprCase
    {
      get => hiddenFlowToPeprCase ??= new();
      set => hiddenFlowToPeprCase = value;
    }

    /// <summary>
    /// A value of HiddenFlowToPeprCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenFlowToPeprCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenFlowToPeprCsePersonsWorkSet
    {
      get => hiddenFlowToPeprCsePersonsWorkSet ??= new();
      set => hiddenFlowToPeprCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ObCollProtAct.
    /// </summary>
    [JsonPropertyName("obCollProtAct")]
    public Common ObCollProtAct
    {
      get => obCollProtAct ??= new();
      set => obCollProtAct = value;
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
    /// A value of ProtectQuestionLiteral.
    /// </summary>
    [JsonPropertyName("protectQuestionLiteral")]
    public SpTextWorkArea ProtectQuestionLiteral
    {
      get => protectQuestionLiteral ??= new();
      set => protectQuestionLiteral = value;
    }

    /// <summary>
    /// A value of CollProtAnswer.
    /// </summary>
    [JsonPropertyName("collProtAnswer")]
    public Common CollProtAnswer
    {
      get => collProtAnswer ??= new();
      set => collProtAnswer = value;
    }

    private CsePerson hiddenConcurrent;
    private CodeValue country;
    private Common interstateDebtExists;
    private CsePersonsWorkSet hiddenAlternate;
    private Obligation hiddenObligorObligation;
    private DateWorkArea obligationCreatedDate;
    private Obligation hiddenIntstInfo;
    private InterstateRequest hiddenInterstateRequest;
    private InterstateRequest interstateRequest;
    private Common beforeLink;
    private DateWorkArea hiddenDiscontinueDate;
    private ObligationPaymentSchedule hiddenObligationPaymentSchedule;
    private Common historyIndicator;
    private CsePersonsWorkSet assignName;
    private TextWorkArea assignPrompt;
    private ServiceProvider assign1;
    private CsePersonsWorkSet zdelImportDesignatedPayee;
    private Case1 caseId;
    private TextWorkArea payeePrompt;
    private CsePersonsWorkSet flow;
    private TextWorkArea altAddrPrompt;
    private CsePersonsWorkSet alternate;
    private AccrualInstructions accrualInstructions;
    private Standard standard;
    private CsePerson obligorCsePerson;
    private Common obligorPrompt;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private CsePerson concurrentObligorCsePerson;
    private Common concurrentObligorPrompt;
    private CsePersonsWorkSet concurrentObligorCsePersonsWorkSet;
    private LegalAction legalAction;
    private Common legalActionPrompt;
    private ObligationType obligationType;
    private Common obligationTypePrompt;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private FrequencyWorkSet frequencyWorkSet;
    private Common frequencyPrompt;
    private DateWorkArea discontinueDate;
    private Common accrualSuspendedInd;
    private Common manualDistributionInd;
    private Common interestSuspendedInd;
    private Common accrualAmount;
    private Obligation obligorObligation;
    private Obligation concurrentObligorObligation;
    private Common obligationActiveInd;
    private Array<GroupGroup> group;
    private CsePerson hiddenObligorCsePerson;
    private ObligationType hiddenObligationType;
    private LegalAction hiddenLegalAction;
    private NextTranInfo hiddenNextTranInfo;
    private Common hiddenCommon;
    private ObligationPaymentSchedule hiddenPrevious;
    private LegalActionDetail hiddenLegalActionDetail;
    private Common hiddenNoAction;
    private AccrualInstructions hiddenAccrualInstructions;
    private Common countryPrompt;
    private LegalActionDetail legalActionDetail;
    private Common hiddenProtectHistory;
    private CsePersonAccount hcCpaObligor;
    private Case1 hiddenFlowToPeprCase;
    private CsePersonsWorkSet hiddenFlowToPeprCsePersonsWorkSet;
    private Common obCollProtAct;
    private Common common;
    private SpTextWorkArea protectQuestionLiteral;
    private Common collProtAnswer;
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
      /// A value of Zdel.
      /// </summary>
      [JsonPropertyName("zdel")]
      public Program Zdel
      {
        get => zdel ??= new();
        set => zdel = value;
      }

      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of SupportedCsePerson.
      /// </summary>
      [JsonPropertyName("supportedCsePerson")]
      public CsePerson SupportedCsePerson
      {
        get => supportedCsePerson ??= new();
        set => supportedCsePerson = value;
      }

      /// <summary>
      /// A value of AccrualSuspended.
      /// </summary>
      [JsonPropertyName("accrualSuspended")]
      public Common AccrualSuspended
      {
        get => accrualSuspended ??= new();
        set => accrualSuspended = value;
      }

      /// <summary>
      /// A value of SupportedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("supportedCsePersonsWorkSet")]
      public CsePersonsWorkSet SupportedCsePersonsWorkSet
      {
        get => supportedCsePersonsWorkSet ??= new();
        set => supportedCsePersonsWorkSet = value;
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
      /// A value of ProratePercentage.
      /// </summary>
      [JsonPropertyName("proratePercentage")]
      public Common ProratePercentage
      {
        get => proratePercentage ??= new();
        set => proratePercentage = value;
      }

      /// <summary>
      /// A value of ObligationTransaction.
      /// </summary>
      [JsonPropertyName("obligationTransaction")]
      public ObligationTransaction ObligationTransaction
      {
        get => obligationTransaction ??= new();
        set => obligationTransaction = value;
      }

      /// <summary>
      /// A value of ProgramScreenAttributes.
      /// </summary>
      [JsonPropertyName("programScreenAttributes")]
      public ProgramScreenAttributes ProgramScreenAttributes
      {
        get => programScreenAttributes ??= new();
        set => programScreenAttributes = value;
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
      /// A value of AccrualInstructions.
      /// </summary>
      [JsonPropertyName("accrualInstructions")]
      public AccrualInstructions AccrualInstructions
      {
        get => accrualInstructions ??= new();
        set => accrualInstructions = value;
      }

      /// <summary>
      /// A value of HiddenConcurrent.
      /// </summary>
      [JsonPropertyName("hiddenConcurrent")]
      public ObligationTransaction HiddenConcurrent
      {
        get => hiddenConcurrent ??= new();
        set => hiddenConcurrent = value;
      }

      /// <summary>
      /// A value of Previous.
      /// </summary>
      [JsonPropertyName("previous")]
      public ObligationTransaction Previous
      {
        get => previous ??= new();
        set => previous = value;
      }

      /// <summary>
      /// A value of SuspendAccrual.
      /// </summary>
      [JsonPropertyName("suspendAccrual")]
      public CsePersonsWorkSet SuspendAccrual
      {
        get => suspendAccrual ??= new();
        set => suspendAccrual = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public AccrualInstructions Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program zdel;
      private Common sel;
      private CsePerson supportedCsePerson;
      private Common accrualSuspended;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private Common proratePercentage;
      private ObligationTransaction obligationTransaction;
      private ProgramScreenAttributes programScreenAttributes;
      private ServiceProvider serviceProvider;
      private AccrualInstructions accrualInstructions;
      private ObligationTransaction hiddenConcurrent;
      private ObligationTransaction previous;
      private CsePersonsWorkSet suspendAccrual;
      private AccrualInstructions hidden;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of InterstateDebtExists.
    /// </summary>
    [JsonPropertyName("interstateDebtExists")]
    public Common InterstateDebtExists
    {
      get => interstateDebtExists ??= new();
      set => interstateDebtExists = value;
    }

    /// <summary>
    /// A value of HiddenAlternate.
    /// </summary>
    [JsonPropertyName("hiddenAlternate")]
    public CsePersonsWorkSet HiddenAlternate
    {
      get => hiddenAlternate ??= new();
      set => hiddenAlternate = value;
    }

    /// <summary>
    /// A value of HiddenObligorObligation.
    /// </summary>
    [JsonPropertyName("hiddenObligorObligation")]
    public Obligation HiddenObligorObligation
    {
      get => hiddenObligorObligation ??= new();
      set => hiddenObligorObligation = value;
    }

    /// <summary>
    /// A value of ObligationCreatedDate.
    /// </summary>
    [JsonPropertyName("obligationCreatedDate")]
    public DateWorkArea ObligationCreatedDate
    {
      get => obligationCreatedDate ??= new();
      set => obligationCreatedDate = value;
    }

    /// <summary>
    /// A value of HiddenIntstInfo.
    /// </summary>
    [JsonPropertyName("hiddenIntstInfo")]
    public Obligation HiddenIntstInfo
    {
      get => hiddenIntstInfo ??= new();
      set => hiddenIntstInfo = value;
    }

    /// <summary>
    /// A value of HiddenInterstateRequest.
    /// </summary>
    [JsonPropertyName("hiddenInterstateRequest")]
    public InterstateRequest HiddenInterstateRequest
    {
      get => hiddenInterstateRequest ??= new();
      set => hiddenInterstateRequest = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of HiddenDiscontinueDate.
    /// </summary>
    [JsonPropertyName("hiddenDiscontinueDate")]
    public DateWorkArea HiddenDiscontinueDate
    {
      get => hiddenDiscontinueDate ??= new();
      set => hiddenDiscontinueDate = value;
    }

    /// <summary>
    /// A value of HiddenObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("hiddenObligationPaymentSchedule")]
    public ObligationPaymentSchedule HiddenObligationPaymentSchedule
    {
      get => hiddenObligationPaymentSchedule ??= new();
      set => hiddenObligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of HistoryIndicator.
    /// </summary>
    [JsonPropertyName("historyIndicator")]
    public Common HistoryIndicator
    {
      get => historyIndicator ??= new();
      set => historyIndicator = value;
    }

    /// <summary>
    /// A value of AssignName.
    /// </summary>
    [JsonPropertyName("assignName")]
    public CsePersonsWorkSet AssignName
    {
      get => assignName ??= new();
      set => assignName = value;
    }

    /// <summary>
    /// A value of AssignPrompt.
    /// </summary>
    [JsonPropertyName("assignPrompt")]
    public TextWorkArea AssignPrompt
    {
      get => assignPrompt ??= new();
      set => assignPrompt = value;
    }

    /// <summary>
    /// A value of Assign1.
    /// </summary>
    [JsonPropertyName("assign1")]
    public ServiceProvider Assign1
    {
      get => assign1 ??= new();
      set => assign1 = value;
    }

    /// <summary>
    /// A value of FlowSpTextWorkArea.
    /// </summary>
    [JsonPropertyName("flowSpTextWorkArea")]
    public SpTextWorkArea FlowSpTextWorkArea
    {
      get => flowSpTextWorkArea ??= new();
      set => flowSpTextWorkArea = value;
    }

    /// <summary>
    /// A value of FlowCsePersonAccount.
    /// </summary>
    [JsonPropertyName("flowCsePersonAccount")]
    public CsePersonAccount FlowCsePersonAccount
    {
      get => flowCsePersonAccount ??= new();
      set => flowCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ZdelExportDesignatedPayee.
    /// </summary>
    [JsonPropertyName("zdelExportDesignatedPayee")]
    public CsePersonsWorkSet ZdelExportDesignatedPayee
    {
      get => zdelExportDesignatedPayee ??= new();
      set => zdelExportDesignatedPayee = value;
    }

    /// <summary>
    /// A value of CaseId.
    /// </summary>
    [JsonPropertyName("caseId")]
    public Case1 CaseId
    {
      get => caseId ??= new();
      set => caseId = value;
    }

    /// <summary>
    /// A value of PayeePrompt.
    /// </summary>
    [JsonPropertyName("payeePrompt")]
    public TextWorkArea PayeePrompt
    {
      get => payeePrompt ??= new();
      set => payeePrompt = value;
    }

    /// <summary>
    /// A value of AltAddrPrompt.
    /// </summary>
    [JsonPropertyName("altAddrPrompt")]
    public TextWorkArea AltAddrPrompt
    {
      get => altAddrPrompt ??= new();
      set => altAddrPrompt = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePersonsWorkSet Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
    }

    /// <summary>
    /// A value of AlternateAddrPrompt.
    /// </summary>
    [JsonPropertyName("alternateAddrPrompt")]
    public Common AlternateAddrPrompt
    {
      get => alternateAddrPrompt ??= new();
      set => alternateAddrPrompt = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of HiddenLegalActionDetail.
    /// </summary>
    [JsonPropertyName("hiddenLegalActionDetail")]
    public LegalActionDetail HiddenLegalActionDetail
    {
      get => hiddenLegalActionDetail ??= new();
      set => hiddenLegalActionDetail = value;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorPrompt.
    /// </summary>
    [JsonPropertyName("obligorPrompt")]
    public Common ObligorPrompt
    {
      get => obligorPrompt ??= new();
      set => obligorPrompt = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ConcurrentObligorCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentObligorCsePerson")]
    public CsePerson ConcurrentObligorCsePerson
    {
      get => concurrentObligorCsePerson ??= new();
      set => concurrentObligorCsePerson = value;
    }

    /// <summary>
    /// A value of ConcurrentObligorPrompt.
    /// </summary>
    [JsonPropertyName("concurrentObligorPrompt")]
    public Common ConcurrentObligorPrompt
    {
      get => concurrentObligorPrompt ??= new();
      set => concurrentObligorPrompt = value;
    }

    /// <summary>
    /// A value of ConcurrentObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("concurrentObligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ConcurrentObligorCsePersonsWorkSet
    {
      get => concurrentObligorCsePersonsWorkSet ??= new();
      set => concurrentObligorCsePersonsWorkSet = value;
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
    /// A value of LegalActionPrompt.
    /// </summary>
    [JsonPropertyName("legalActionPrompt")]
    public Common LegalActionPrompt
    {
      get => legalActionPrompt ??= new();
      set => legalActionPrompt = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligationTypePrompt.
    /// </summary>
    [JsonPropertyName("obligationTypePrompt")]
    public Common ObligationTypePrompt
    {
      get => obligationTypePrompt ??= new();
      set => obligationTypePrompt = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
    }

    /// <summary>
    /// A value of FrequencyPrompt.
    /// </summary>
    [JsonPropertyName("frequencyPrompt")]
    public Common FrequencyPrompt
    {
      get => frequencyPrompt ??= new();
      set => frequencyPrompt = value;
    }

    /// <summary>
    /// A value of DiscontinueDate.
    /// </summary>
    [JsonPropertyName("discontinueDate")]
    public DateWorkArea DiscontinueDate
    {
      get => discontinueDate ??= new();
      set => discontinueDate = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Concurrent.
    /// </summary>
    [JsonPropertyName("concurrent")]
    public Obligation Concurrent
    {
      get => concurrent ??= new();
      set => concurrent = value;
    }

    /// <summary>
    /// A value of AccuralSuspendedInd.
    /// </summary>
    [JsonPropertyName("accuralSuspendedInd")]
    public Common AccuralSuspendedInd
    {
      get => accuralSuspendedInd ??= new();
      set => accuralSuspendedInd = value;
    }

    /// <summary>
    /// A value of ManualDistributionInd.
    /// </summary>
    [JsonPropertyName("manualDistributionInd")]
    public Common ManualDistributionInd
    {
      get => manualDistributionInd ??= new();
      set => manualDistributionInd = value;
    }

    /// <summary>
    /// A value of InterestSuspendedInd.
    /// </summary>
    [JsonPropertyName("interestSuspendedInd")]
    public Common InterestSuspendedInd
    {
      get => interestSuspendedInd ??= new();
      set => interestSuspendedInd = value;
    }

    /// <summary>
    /// A value of ObligationActive.
    /// </summary>
    [JsonPropertyName("obligationActive")]
    public Common ObligationActive
    {
      get => obligationActive ??= new();
      set => obligationActive = value;
    }

    /// <summary>
    /// A value of AccrualAmount.
    /// </summary>
    [JsonPropertyName("accrualAmount")]
    public Common AccrualAmount
    {
      get => accrualAmount ??= new();
      set => accrualAmount = value;
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
    /// A value of HiddenObligorCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenObligorCsePerson")]
    public CsePerson HiddenObligorCsePerson
    {
      get => hiddenObligorCsePerson ??= new();
      set => hiddenObligorCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenObligationType.
    /// </summary>
    [JsonPropertyName("hiddenObligationType")]
    public ObligationType HiddenObligationType
    {
      get => hiddenObligationType ??= new();
      set => hiddenObligationType = value;
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
    /// A value of HiddenCommon.
    /// </summary>
    [JsonPropertyName("hiddenCommon")]
    public Common HiddenCommon
    {
      get => hiddenCommon ??= new();
      set => hiddenCommon = value;
    }

    /// <summary>
    /// A value of PassObligation.
    /// </summary>
    [JsonPropertyName("passObligation")]
    public Obligation PassObligation
    {
      get => passObligation ??= new();
      set => passObligation = value;
    }

    /// <summary>
    /// A value of PassObligationType.
    /// </summary>
    [JsonPropertyName("passObligationType")]
    public ObligationType PassObligationType
    {
      get => passObligationType ??= new();
      set => passObligationType = value;
    }

    /// <summary>
    /// A value of PassObligationTransaction.
    /// </summary>
    [JsonPropertyName("passObligationTransaction")]
    public ObligationTransaction PassObligationTransaction
    {
      get => passObligationTransaction ??= new();
      set => passObligationTransaction = value;
    }

    /// <summary>
    /// A value of DlgflwAr.
    /// </summary>
    [JsonPropertyName("dlgflwAr")]
    public CsePersonsWorkSet DlgflwAr
    {
      get => dlgflwAr ??= new();
      set => dlgflwAr = value;
    }

    /// <summary>
    /// A value of PassSupported.
    /// </summary>
    [JsonPropertyName("passSupported")]
    public CsePersonsWorkSet PassSupported
    {
      get => passSupported ??= new();
      set => passSupported = value;
    }

    /// <summary>
    /// A value of PassSupportedPerson.
    /// </summary>
    [JsonPropertyName("passSupportedPerson")]
    public CsePerson PassSupportedPerson
    {
      get => passSupportedPerson ??= new();
      set => passSupportedPerson = value;
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
    /// A value of HiddenPrevious.
    /// </summary>
    [JsonPropertyName("hiddenPrevious")]
    public ObligationPaymentSchedule HiddenPrevious
    {
      get => hiddenPrevious ??= new();
      set => hiddenPrevious = value;
    }

    /// <summary>
    /// A value of BeforeLink.
    /// </summary>
    [JsonPropertyName("beforeLink")]
    public Common BeforeLink
    {
      get => beforeLink ??= new();
      set => beforeLink = value;
    }

    /// <summary>
    /// A value of HiddenNoAction.
    /// </summary>
    [JsonPropertyName("hiddenNoAction")]
    public Common HiddenNoAction
    {
      get => hiddenNoAction ??= new();
      set => hiddenNoAction = value;
    }

    /// <summary>
    /// A value of HiddenAccrualInstructions.
    /// </summary>
    [JsonPropertyName("hiddenAccrualInstructions")]
    public AccrualInstructions HiddenAccrualInstructions
    {
      get => hiddenAccrualInstructions ??= new();
      set => hiddenAccrualInstructions = value;
    }

    /// <summary>
    /// A value of CountryPrompt.
    /// </summary>
    [JsonPropertyName("countryPrompt")]
    public Common CountryPrompt
    {
      get => countryPrompt ??= new();
      set => countryPrompt = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of HiddenConcurrent.
    /// </summary>
    [JsonPropertyName("hiddenConcurrent")]
    public CsePerson HiddenConcurrent
    {
      get => hiddenConcurrent ??= new();
      set => hiddenConcurrent = value;
    }

    /// <summary>
    /// A value of HiddenAltAddress.
    /// </summary>
    [JsonPropertyName("hiddenAltAddress")]
    public CsePersonsWorkSet HiddenAltAddress
    {
      get => hiddenAltAddress ??= new();
      set => hiddenAltAddress = value;
    }

    /// <summary>
    /// A value of HiddenProtectHistory.
    /// </summary>
    [JsonPropertyName("hiddenProtectHistory")]
    public Common HiddenProtectHistory
    {
      get => hiddenProtectHistory ??= new();
      set => hiddenProtectHistory = value;
    }

    /// <summary>
    /// A value of HiddenFlowToPeprCase.
    /// </summary>
    [JsonPropertyName("hiddenFlowToPeprCase")]
    public Case1 HiddenFlowToPeprCase
    {
      get => hiddenFlowToPeprCase ??= new();
      set => hiddenFlowToPeprCase = value;
    }

    /// <summary>
    /// A value of HiddenFlowToPeprCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenFlowToPeprCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenFlowToPeprCsePersonsWorkSet
    {
      get => hiddenFlowToPeprCsePersonsWorkSet ??= new();
      set => hiddenFlowToPeprCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ObCollProtAct.
    /// </summary>
    [JsonPropertyName("obCollProtAct")]
    public Common ObCollProtAct
    {
      get => obCollProtAct ??= new();
      set => obCollProtAct = value;
    }

    /// <summary>
    /// A value of ProtectQuestionLiteral.
    /// </summary>
    [JsonPropertyName("protectQuestionLiteral")]
    public SpTextWorkArea ProtectQuestionLiteral
    {
      get => protectQuestionLiteral ??= new();
      set => protectQuestionLiteral = value;
    }

    /// <summary>
    /// A value of CollProtAnswer.
    /// </summary>
    [JsonPropertyName("collProtAnswer")]
    public Common CollProtAnswer
    {
      get => collProtAnswer ??= new();
      set => collProtAnswer = value;
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
    /// A value of ZdelExportFreqDay1.
    /// </summary>
    [JsonPropertyName("zdelExportFreqDay1")]
    public Common ZdelExportFreqDay1
    {
      get => zdelExportFreqDay1 ??= new();
      set => zdelExportFreqDay1 = value;
    }

    /// <summary>
    /// A value of ZdelExportFreqDay2.
    /// </summary>
    [JsonPropertyName("zdelExportFreqDay2")]
    public Common ZdelExportFreqDay2
    {
      get => zdelExportFreqDay2 ??= new();
      set => zdelExportFreqDay2 = value;
    }

    private CodeValue country;
    private Common interstateDebtExists;
    private CsePersonsWorkSet hiddenAlternate;
    private Obligation hiddenObligorObligation;
    private DateWorkArea obligationCreatedDate;
    private Obligation hiddenIntstInfo;
    private InterstateRequest hiddenInterstateRequest;
    private InterstateRequest interstateRequest;
    private DateWorkArea hiddenDiscontinueDate;
    private ObligationPaymentSchedule hiddenObligationPaymentSchedule;
    private Common historyIndicator;
    private CsePersonsWorkSet assignName;
    private TextWorkArea assignPrompt;
    private ServiceProvider assign1;
    private SpTextWorkArea flowSpTextWorkArea;
    private CsePersonAccount flowCsePersonAccount;
    private CsePersonsWorkSet zdelExportDesignatedPayee;
    private Case1 caseId;
    private TextWorkArea payeePrompt;
    private TextWorkArea altAddrPrompt;
    private CsePersonsWorkSet alternate;
    private Common alternateAddrPrompt;
    private AccrualInstructions accrualInstructions;
    private LegalActionDetail hiddenLegalActionDetail;
    private Standard standard;
    private CsePerson obligorCsePerson;
    private Common obligorPrompt;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private CsePerson concurrentObligorCsePerson;
    private Common concurrentObligorPrompt;
    private CsePersonsWorkSet concurrentObligorCsePersonsWorkSet;
    private LegalAction legalAction;
    private Common legalActionPrompt;
    private ObligationType obligationType;
    private Common obligationTypePrompt;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private FrequencyWorkSet frequencyWorkSet;
    private Common frequencyPrompt;
    private DateWorkArea discontinueDate;
    private Obligation obligation;
    private Obligation concurrent;
    private Common accuralSuspendedInd;
    private Common manualDistributionInd;
    private Common interestSuspendedInd;
    private Common obligationActive;
    private Common accrualAmount;
    private Array<GroupGroup> group;
    private CsePerson hiddenObligorCsePerson;
    private ObligationType hiddenObligationType;
    private LegalAction hiddenLegalAction;
    private Common hiddenCommon;
    private Obligation passObligation;
    private ObligationType passObligationType;
    private ObligationTransaction passObligationTransaction;
    private CsePersonsWorkSet dlgflwAr;
    private CsePersonsWorkSet passSupported;
    private CsePerson passSupportedPerson;
    private NextTranInfo hiddenNextTranInfo;
    private ObligationPaymentSchedule hiddenPrevious;
    private Common beforeLink;
    private Common hiddenNoAction;
    private AccrualInstructions hiddenAccrualInstructions;
    private Common countryPrompt;
    private Code code;
    private LegalActionDetail legalActionDetail;
    private CsePerson hiddenConcurrent;
    private CsePersonsWorkSet hiddenAltAddress;
    private Common hiddenProtectHistory;
    private Case1 hiddenFlowToPeprCase;
    private CsePersonsWorkSet hiddenFlowToPeprCsePersonsWorkSet;
    private Common obCollProtAct;
    private SpTextWorkArea protectQuestionLiteral;
    private Common collProtAnswer;
    private Common common;
    private Common zdelExportFreqDay1;
    private Common zdelExportFreqDay2;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of IvdAgency.
    /// </summary>
    [JsonPropertyName("ivdAgency")]
    public Common IvdAgency
    {
      get => ivdAgency ??= new();
      set => ivdAgency = value;
    }

    /// <summary>
    /// A value of CursorPosition.
    /// </summary>
    [JsonPropertyName("cursorPosition")]
    public CursorPosition CursorPosition
    {
      get => cursorPosition ??= new();
      set => cursorPosition = value;
    }

    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    /// <summary>
    /// A value of SuspendAccrual.
    /// </summary>
    [JsonPropertyName("suspendAccrual")]
    public Common SuspendAccrual
    {
      get => suspendAccrual ??= new();
      set => suspendAccrual = value;
    }

    /// <summary>
    /// A value of TestDup.
    /// </summary>
    [JsonPropertyName("testDup")]
    public Common TestDup
    {
      get => testDup ??= new();
      set => testDup = value;
    }

    /// <summary>
    /// A value of HardcodeStartDateLimit.
    /// </summary>
    [JsonPropertyName("hardcodeStartDateLimit")]
    public DateWorkArea HardcodeStartDateLimit
    {
      get => hardcodeStartDateLimit ??= new();
      set => hardcodeStartDateLimit = value;
    }

    /// <summary>
    /// A value of HardcodeObligJointSevlConcur.
    /// </summary>
    [JsonPropertyName("hardcodeObligJointSevlConcur")]
    public Obligation HardcodeObligJointSevlConcur
    {
      get => hardcodeObligJointSevlConcur ??= new();
      set => hardcodeObligJointSevlConcur = value;
    }

    /// <summary>
    /// A value of HardcodeLapObligor.
    /// </summary>
    [JsonPropertyName("hardcodeLapObligor")]
    public LegalActionPerson HardcodeLapObligor
    {
      get => hardcodeLapObligor ??= new();
      set => hardcodeLapObligor = value;
    }

    /// <summary>
    /// A value of HardcodeOpsCMonthly.
    /// </summary>
    [JsonPropertyName("hardcodeOpsCMonthly")]
    public ObligationPaymentSchedule HardcodeOpsCMonthly
    {
      get => hardcodeOpsCMonthly ??= new();
      set => hardcodeOpsCMonthly = value;
    }

    /// <summary>
    /// A value of HardcodeOpsCBiWeekly.
    /// </summary>
    [JsonPropertyName("hardcodeOpsCBiWeekly")]
    public ObligationPaymentSchedule HardcodeOpsCBiWeekly
    {
      get => hardcodeOpsCBiWeekly ??= new();
      set => hardcodeOpsCBiWeekly = value;
    }

    /// <summary>
    /// A value of HardcodeOpsCWeekly.
    /// </summary>
    [JsonPropertyName("hardcodeOpsCWeekly")]
    public ObligationPaymentSchedule HardcodeOpsCWeekly
    {
      get => hardcodeOpsCWeekly ??= new();
      set => hardcodeOpsCWeekly = value;
    }

    /// <summary>
    /// A value of HardcodeOpsCSemiMonthly.
    /// </summary>
    [JsonPropertyName("hardcodeOpsCSemiMonthly")]
    public ObligationPaymentSchedule HardcodeOpsCSemiMonthly
    {
      get => hardcodeOpsCSemiMonthly ??= new();
      set => hardcodeOpsCSemiMonthly = value;
    }

    /// <summary>
    /// A value of HardcodeOpsCBiMonthly.
    /// </summary>
    [JsonPropertyName("hardcodeOpsCBiMonthly")]
    public ObligationPaymentSchedule HardcodeOpsCBiMonthly
    {
      get => hardcodeOpsCBiMonthly ??= new();
      set => hardcodeOpsCBiMonthly = value;
    }

    /// <summary>
    /// A value of HardcodeDdshActiveStatus.
    /// </summary>
    [JsonPropertyName("hardcodeDdshActiveStatus")]
    public DebtDetailStatusHistory HardcodeDdshActiveStatus
    {
      get => hardcodeDdshActiveStatus ??= new();
      set => hardcodeDdshActiveStatus = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnDtVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnDtVoluntary")]
    public ObligationTransaction HardcodeOtrnDtVoluntary
    {
      get => hardcodeOtrnDtVoluntary ??= new();
      set => hardcodeOtrnDtVoluntary = value;
    }

    /// <summary>
    /// A value of HardcodeCpaSupportedPerson.
    /// </summary>
    [JsonPropertyName("hardcodeCpaSupportedPerson")]
    public CsePersonAccount HardcodeCpaSupportedPerson
    {
      get => hardcodeCpaSupportedPerson ??= new();
      set => hardcodeCpaSupportedPerson = value;
    }

    /// <summary>
    /// A value of HardcodeOtCFeesClassificatio.
    /// </summary>
    [JsonPropertyName("hardcodeOtCFeesClassificatio")]
    public ObligationType HardcodeOtCFeesClassificatio
    {
      get => hardcodeOtCFeesClassificatio ??= new();
      set => hardcodeOtCFeesClassificatio = value;
    }

    /// <summary>
    /// A value of HardcodeOtCRecoverClassifica.
    /// </summary>
    [JsonPropertyName("hardcodeOtCRecoverClassifica")]
    public ObligationType HardcodeOtCRecoverClassifica
    {
      get => hardcodeOtCRecoverClassifica ??= new();
      set => hardcodeOtCRecoverClassifica = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnDtDebtDetail.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnDtDebtDetail")]
    public ObligationTransaction HardcodeOtrnDtDebtDetail
    {
      get => hardcodeOtrnDtDebtDetail ??= new();
      set => hardcodeOtrnDtDebtDetail = value;
    }

    /// <summary>
    /// A value of HardcodeOt718BUraJudgement.
    /// </summary>
    [JsonPropertyName("hardcodeOt718BUraJudgement")]
    public ObligationType HardcodeOt718BUraJudgement
    {
      get => hardcodeOt718BUraJudgement ??= new();
      set => hardcodeOt718BUraJudgement = value;
    }

    /// <summary>
    /// A value of HardcodeOtCVoluntaryClassifi.
    /// </summary>
    [JsonPropertyName("hardcodeOtCVoluntaryClassifi")]
    public ObligationType HardcodeOtCVoluntaryClassifi
    {
      get => hardcodeOtCVoluntaryClassifi ??= new();
      set => hardcodeOtCVoluntaryClassifi = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnDtAccrualInstruc.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnDtAccrualInstruc")]
    public ObligationTransaction HardcodeOtrnDtAccrualInstruc
    {
      get => hardcodeOtrnDtAccrualInstruc ??= new();
      set => hardcodeOtrnDtAccrualInstruc = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnTDebt")]
    public ObligationTransaction HardcodeOtrnTDebt
    {
      get => hardcodeOtrnTDebt ??= new();
      set => hardcodeOtrnTDebt = value;
    }

    /// <summary>
    /// A value of HardcodePgmNonAdcFosterCare.
    /// </summary>
    [JsonPropertyName("hardcodePgmNonAdcFosterCare")]
    public ProgramScreenAttributes HardcodePgmNonAdcFosterCare
    {
      get => hardcodePgmNonAdcFosterCare ??= new();
      set => hardcodePgmNonAdcFosterCare = value;
    }

    /// <summary>
    /// A value of HardcodePgmAdc.
    /// </summary>
    [JsonPropertyName("hardcodePgmAdc")]
    public ProgramScreenAttributes HardcodePgmAdc
    {
      get => hardcodePgmAdc ??= new();
      set => hardcodePgmAdc = value;
    }

    /// <summary>
    /// A value of HardcodePgmAdcFosterCare.
    /// </summary>
    [JsonPropertyName("hardcodePgmAdcFosterCare")]
    public ProgramScreenAttributes HardcodePgmAdcFosterCare
    {
      get => hardcodePgmAdcFosterCare ??= new();
      set => hardcodePgmAdcFosterCare = value;
    }

    /// <summary>
    /// A value of HardcodeOrrJointAndSeveral.
    /// </summary>
    [JsonPropertyName("hardcodeOrrJointAndSeveral")]
    public ObligationRlnRsn HardcodeOrrJointAndSeveral
    {
      get => hardcodeOrrJointAndSeveral ??= new();
      set => hardcodeOrrJointAndSeveral = value;
    }

    /// <summary>
    /// A value of HardcodeOtrrConcurrentObliga.
    /// </summary>
    [JsonPropertyName("hardcodeOtrrConcurrentObliga")]
    public ObligationTransactionRlnRsn HardcodeOtrrConcurrentObliga
    {
      get => hardcodeOtrrConcurrentObliga ??= new();
      set => hardcodeOtrrConcurrentObliga = value;
    }

    /// <summary>
    /// A value of UpdateInterstateInfo.
    /// </summary>
    [JsonPropertyName("updateInterstateInfo")]
    public Common UpdateInterstateInfo
    {
      get => updateInterstateInfo ??= new();
      set => updateInterstateInfo = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public InterstateRequest New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of SupPersMarkedForUpdate.
    /// </summary>
    [JsonPropertyName("supPersMarkedForUpdate")]
    public Common SupPersMarkedForUpdate
    {
      get => supPersMarkedForUpdate ??= new();
      set => supPersMarkedForUpdate = value;
    }

    /// <summary>
    /// A value of AlternateAddrSet.
    /// </summary>
    [JsonPropertyName("alternateAddrSet")]
    public Common AlternateAddrSet
    {
      get => alternateAddrSet ??= new();
      set => alternateAddrSet = value;
    }

    /// <summary>
    /// A value of FromHistMonaNxttran.
    /// </summary>
    [JsonPropertyName("fromHistMonaNxttran")]
    public Common FromHistMonaNxttran
    {
      get => fromHistMonaNxttran ??= new();
      set => fromHistMonaNxttran = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    /// <summary>
    /// A value of AltBillLocError.
    /// </summary>
    [JsonPropertyName("altBillLocError")]
    public Common AltBillLocError
    {
      get => altBillLocError ??= new();
      set => altBillLocError = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of SelectCounter.
    /// </summary>
    [JsonPropertyName("selectCounter")]
    public Common SelectCounter
    {
      get => selectCounter ??= new();
      set => selectCounter = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of HardcodeObligor.
    /// </summary>
    [JsonPropertyName("hardcodeObligor")]
    public CsePersonAccount HardcodeObligor
    {
      get => hardcodeObligor ??= new();
      set => hardcodeObligor = value;
    }

    /// <summary>
    /// A value of ErrorCount.
    /// </summary>
    [JsonPropertyName("errorCount")]
    public Common ErrorCount
    {
      get => errorCount ??= new();
      set => errorCount = value;
    }

    /// <summary>
    /// A value of HardcodeAccruing.
    /// </summary>
    [JsonPropertyName("hardcodeAccruing")]
    public ObligationType HardcodeAccruing
    {
      get => hardcodeAccruing ??= new();
      set => hardcodeAccruing = value;
    }

    /// <summary>
    /// A value of BeforeLink.
    /// </summary>
    [JsonPropertyName("beforeLink")]
    public Common BeforeLink
    {
      get => beforeLink ??= new();
      set => beforeLink = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of CheckObligationExistence.
    /// </summary>
    [JsonPropertyName("checkObligationExistence")]
    public Common CheckObligationExistence
    {
      get => checkObligationExistence ??= new();
      set => checkObligationExistence = value;
    }

    /// <summary>
    /// A value of HardcodeSupported.
    /// </summary>
    [JsonPropertyName("hardcodeSupported")]
    public LegalActionPerson HardcodeSupported
    {
      get => hardcodeSupported ??= new();
      set => hardcodeSupported = value;
    }

    /// <summary>
    /// A value of NoOfPromptsSelected.
    /// </summary>
    [JsonPropertyName("noOfPromptsSelected")]
    public Common NoOfPromptsSelected
    {
      get => noOfPromptsSelected ??= new();
      set => noOfPromptsSelected = value;
    }

    /// <summary>
    /// A value of NoOfSupportedSelected.
    /// </summary>
    [JsonPropertyName("noOfSupportedSelected")]
    public Common NoOfSupportedSelected
    {
      get => noOfSupportedSelected ??= new();
      set => noOfSupportedSelected = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonsWorkSet Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public AbendData Eab
    {
      get => eab ??= new();
      set => eab = value;
    }

    /// <summary>
    /// A value of ProtectHistory.
    /// </summary>
    [JsonPropertyName("protectHistory")]
    public Common ProtectHistory
    {
      get => protectHistory ??= new();
      set => protectHistory = value;
    }

    /// <summary>
    /// A value of Amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public Common Amount
    {
      get => amount ??= new();
      set => amount = value;
    }

    /// <summary>
    /// A value of ExitState.
    /// </summary>
    [JsonPropertyName("exitState")]
    public Common ExitState
    {
      get => exitState ??= new();
      set => exitState = value;
    }

    /// <summary>
    /// A value of CountSelected.
    /// </summary>
    [JsonPropertyName("countSelected")]
    public Common CountSelected
    {
      get => countSelected ??= new();
      set => countSelected = value;
    }

    /// <summary>
    /// A value of SelectedSupported.
    /// </summary>
    [JsonPropertyName("selectedSupported")]
    public CsePersonsWorkSet SelectedSupported
    {
      get => selectedSupported ??= new();
      set => selectedSupported = value;
    }

    /// <summary>
    /// A value of FlowToPepr.
    /// </summary>
    [JsonPropertyName("flowToPepr")]
    public Common FlowToPepr
    {
      get => flowToPepr ??= new();
      set => flowToPepr = value;
    }

    /// <summary>
    /// A value of ChngdDiscDateFnd.
    /// </summary>
    [JsonPropertyName("chngdDiscDateFnd")]
    public Common ChngdDiscDateFnd
    {
      get => chngdDiscDateFnd ??= new();
      set => chngdDiscDateFnd = value;
    }

    /// <summary>
    /// A value of CollProt.
    /// </summary>
    [JsonPropertyName("collProt")]
    public DateWorkArea CollProt
    {
      get => collProt ??= new();
      set => collProt = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    /// <summary>
    /// A value of CollsFndToProtect.
    /// </summary>
    [JsonPropertyName("collsFndToProtect")]
    public Common CollsFndToProtect
    {
      get => collsFndToProtect ??= new();
      set => collsFndToProtect = value;
    }

    /// <summary>
    /// A value of ObCollProtHistCreated.
    /// </summary>
    [JsonPropertyName("obCollProtHistCreated")]
    public Common ObCollProtHistCreated
    {
      get => obCollProtHistCreated ??= new();
      set => obCollProtHistCreated = value;
    }

    /// <summary>
    /// A value of NonInterstatePgmFound.
    /// </summary>
    [JsonPropertyName("nonInterstatePgmFound")]
    public Common NonInterstatePgmFound
    {
      get => nonInterstatePgmFound ??= new();
      set => nonInterstatePgmFound = value;
    }

    /// <summary>
    /// A value of InterstatePgmFound.
    /// </summary>
    [JsonPropertyName("interstatePgmFound")]
    public Common InterstatePgmFound
    {
      get => interstatePgmFound ??= new();
      set => interstatePgmFound = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      ivdAgency = null;
      cursorPosition = null;
      accrualSuspension = null;
      suspendAccrual = null;
      testDup = null;
      hardcodeStartDateLimit = null;
      hardcodeObligJointSevlConcur = null;
      hardcodeLapObligor = null;
      hardcodeOpsCMonthly = null;
      hardcodeOpsCBiWeekly = null;
      hardcodeOpsCWeekly = null;
      hardcodeOpsCSemiMonthly = null;
      hardcodeOpsCBiMonthly = null;
      updateInterstateInfo = null;
      new1 = null;
      textWorkArea = null;
      supPersMarkedForUpdate = null;
      alternateAddrSet = null;
      fromHistMonaNxttran = null;
      infrastructure = null;
      common = null;
      selCount = null;
      altBillLocError = null;
      obligationType = null;
      obligation = null;
      csePerson = null;
      selectCounter = null;
      csePersonsWorkSet = null;
      obligationPaymentSchedule = null;
      errorCount = null;
      beforeLink = null;
      blank = null;
      checkObligationExistence = null;
      noOfPromptsSelected = null;
      noOfSupportedSelected = null;
      supported = null;
      eab = null;
      protectHistory = null;
      amount = null;
      exitState = null;
      countSelected = null;
      selectedSupported = null;
      flowToPepr = null;
      chngdDiscDateFnd = null;
      collProt = null;
      obligCollProtectionHist = null;
      collsFndToProtect = null;
      obCollProtHistCreated = null;
      nonInterstatePgmFound = null;
      interstatePgmFound = null;
    }

    private Common ivdAgency;
    private CursorPosition cursorPosition;
    private AccrualSuspension accrualSuspension;
    private Common suspendAccrual;
    private Common testDup;
    private DateWorkArea hardcodeStartDateLimit;
    private Obligation hardcodeObligJointSevlConcur;
    private LegalActionPerson hardcodeLapObligor;
    private ObligationPaymentSchedule hardcodeOpsCMonthly;
    private ObligationPaymentSchedule hardcodeOpsCBiWeekly;
    private ObligationPaymentSchedule hardcodeOpsCWeekly;
    private ObligationPaymentSchedule hardcodeOpsCSemiMonthly;
    private ObligationPaymentSchedule hardcodeOpsCBiMonthly;
    private DebtDetailStatusHistory hardcodeDdshActiveStatus;
    private ObligationTransaction hardcodeOtrnDtVoluntary;
    private CsePersonAccount hardcodeCpaSupportedPerson;
    private ObligationType hardcodeOtCFeesClassificatio;
    private ObligationType hardcodeOtCRecoverClassifica;
    private ObligationTransaction hardcodeOtrnDtDebtDetail;
    private ObligationType hardcodeOt718BUraJudgement;
    private ObligationType hardcodeOtCVoluntaryClassifi;
    private ObligationTransaction hardcodeOtrnDtAccrualInstruc;
    private ObligationTransaction hardcodeOtrnTDebt;
    private ProgramScreenAttributes hardcodePgmNonAdcFosterCare;
    private ProgramScreenAttributes hardcodePgmAdc;
    private ProgramScreenAttributes hardcodePgmAdcFosterCare;
    private ObligationRlnRsn hardcodeOrrJointAndSeveral;
    private ObligationTransactionRlnRsn hardcodeOtrrConcurrentObliga;
    private Common updateInterstateInfo;
    private InterstateRequest new1;
    private DateWorkArea current;
    private TextWorkArea textWorkArea;
    private Common supPersMarkedForUpdate;
    private Common alternateAddrSet;
    private Common fromHistMonaNxttran;
    private Infrastructure infrastructure;
    private Common common;
    private Common selCount;
    private Common altBillLocError;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePerson csePerson;
    private Common selectCounter;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePersonAccount hardcodeObligor;
    private Common errorCount;
    private ObligationType hardcodeAccruing;
    private Common beforeLink;
    private DateWorkArea max;
    private DateWorkArea blank;
    private Common checkObligationExistence;
    private LegalActionPerson hardcodeSupported;
    private Common noOfPromptsSelected;
    private Common noOfSupportedSelected;
    private CsePersonsWorkSet supported;
    private AbendData eab;
    private Common protectHistory;
    private Common amount;
    private Common exitState;
    private Common countSelected;
    private CsePersonsWorkSet selectedSupported;
    private Common flowToPepr;
    private Common chngdDiscDateFnd;
    private DateWorkArea collProt;
    private ObligCollProtectionHist obligCollProtectionHist;
    private Common collsFndToProtect;
    private Common obCollProtHistCreated;
    private Common nonInterstatePgmFound;
    private Common interstatePgmFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of RoleTypeAr.
    /// </summary>
    [JsonPropertyName("roleTypeAr")]
    public CaseRole RoleTypeAr
    {
      get => roleTypeAr ??= new();
      set => roleTypeAr = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of SupportedSpOrCh.
    /// </summary>
    [JsonPropertyName("supportedSpOrCh")]
    public CsePerson SupportedSpOrCh
    {
      get => supportedSpOrCh ??= new();
      set => supportedSpOrCh = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of SupportedPersonCaseUnit.
    /// </summary>
    [JsonPropertyName("supportedPersonCaseUnit")]
    public CaseUnit SupportedPersonCaseUnit
    {
      get => supportedPersonCaseUnit ??= new();
      set => supportedPersonCaseUnit = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of SupportedPersonCaseRole.
    /// </summary>
    [JsonPropertyName("supportedPersonCaseRole")]
    public CaseRole SupportedPersonCaseRole
    {
      get => supportedPersonCaseRole ??= new();
      set => supportedPersonCaseRole = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of OtherView.
    /// </summary>
    [JsonPropertyName("otherView")]
    public Obligation OtherView
    {
      get => otherView ??= new();
      set => otherView = value;
    }

    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    private CaseRole roleTypeAr;
    private CsePerson ar;
    private Fips fips;
    private InterstateRequest interstateRequest;
    private CaseRole caseRole;
    private Case1 case1;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private CsePerson supportedSpOrCh;
    private Obligation obligation;
    private CsePersonAccount obligorCsePersonAccount;
    private CsePerson obligorCsePerson;
    private ObligationType obligationType;
    private AccrualInstructions accrualInstructions;
    private ObligationTransaction obligationTransaction;
    private CaseUnit supportedPersonCaseUnit;
    private CsePerson supported;
    private CaseRole supportedPersonCaseRole;
    private CaseRole ap;
    private ObligCollProtectionHist obligCollProtectionHist;
    private Collection collection;
    private ObligationTransaction debt;
    private Obligation otherView;
    private AccrualSuspension accrualSuspension;
    private Program program;
    private PersonProgram personProgram;
  }
#endregion
}
