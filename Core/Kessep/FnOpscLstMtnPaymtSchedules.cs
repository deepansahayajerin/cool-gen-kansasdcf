// Program: FN_OPSC_LST_MTN_PAYMT_SCHEDULES, ID: 372041394, model: 746.
// Short name: SWEOPSCP
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
/// A program: FN_OPSC_LST_MTN_PAYMT_SCHEDULES.
/// </para>
/// <para>
/// RESP: FINANCE
/// This procedure will be used to list and maintain payment schedules for a non
/// -accruing debt.  Active and inactive payment schedules can be listed.
/// Active payment schedules, payment schedules which have an effective date
/// prior to, or equal to, the current date, can only be discontinued.  A
/// payment schedule which has not yet become effective can be modified or
/// deleted.  Inactive payment schedules, where the discontinue date is prior
/// to, or equal to, the current day, can not be changed.  A new payment
/// schedule can also be added on this screen.  Only one payment schedule can be
/// active for any period of time.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOpscLstMtnPaymtSchedules: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OPSC_LST_MTN_PAYMT_SCHEDULES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOpscLstMtnPaymtSchedules(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOpscLstMtnPaymtSchedules.
  /// </summary>
  public FnOpscLstMtnPaymtSchedules(IContext context, Import import,
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
    // ------------------------------------------------------------------
    // Date	   Programmer		Reason #	Description
    // 01/22/96   Holly Kennedy-MTW			Modify initial code.
    // 01/29/96   Maureen Brown-MTW                    Added code for concurrent
    // obligors.
    // 05/28/96   Siraj Konkader                       Print functions
    // 12/11/96   R. Marchman			Add new security and next tran
    // 2/20/97    S. Konkader                  Removed print funtions
    // 04/01/1997 Shyamal Datta                Fixes.
    // 05/18/1997 Holly Kennedy-MTW            Fixed update logic so that
    //                                         
    // payment schedule is not
    //                                         
    // validated against itself.
    //                                         
    // Disallow update if Payment
    //                                         
    // schedule is the result of
    //                                         
    // legal action.
    // 07/15/97   Paul R. Egger - MTW          Add / Delete logic allowing start
    // date to be equal to current date.  Changed read legal_action_detail read
    // to properly qualify the read.  Changed the screen to underline the
    // Interest owed field.  Validated that the frequency code entered exists on
    // the code value table via the CAB_GET_CODE_VALUE_DESCRIPTION instead of
    // the FN_SET_FREQUENCY_TEXT_FIELD.
    // 09/08/97	Siraj Konkader		Fixed code in PROCESS command. Added ESCAPE 
    // after every error. Checked for exit states (valid - add succ etc.) before
    // continuing. This would allow multiple actions to be carried out at the
    // same time.
    // Deleted edit code (in delete portion) for active PS because that is being
    // handled in Delete CAB.
    // Also found that in ADD CAB, there is logic that prevents addition of a 
    // new PS if another is already active AND there is also logic (following
    // the above) that ends the previous PS when adding a new one. Obviously the
    // latter portion is never executed.
    // ------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------
    // 10/27/98   G Sharp              Phase 2  Changes: 1. Add Day of Month 1 
    // and 2 and Day of Week to the screen.
    //                                                   
    // 2. Clean up zdelete exit state's.
    //                                                   
    // 3. Added obligation_type to matching views for
    // fn_update_obligation_pymnt_sch and
    // fn_delete_obligation_pymnt_sch.
    //                                                   
    // 4. Added concurrent obligation_type to: dialog
    // flow from ONAC and concurrent reads.
    //                                                    
    // 5. On read for concurrent obligation, change
    // concurrent cse_person_account to (only)
    // cse_person_account and concurrent cse_person to (
    // only) ces_person. Also deleted entity action views
    // for concurrent cse_person_account and concurrent
    // cse_person.
    //                                                    
    // 6. Added to screen Worker ID. This could be last
    // updated by or if no last update by then created by.
    //                                                    
    // 7. Added to screen Interstate Ind.
    //                                                    
    // 8. Added CAB fn_cab_set_accrual_or_due_amount. This
    // CAB is used to calculate Obligation Amount. The
    // screen will be changed to use this obligation
    // amount.
    //                                                    
    // 9. Changed the order of edit on ADD.
    // 5/13/99 - bud adams  -  Changed 8 CURRENT_DATE references;  changed Read 
    // properties to Select Only;
    // 9/10/99 - kalpesh doshi - PR#H72822 Remove 'Current Owed' field on the 
    // screen.(instead of deleting the views the field has been marked hidden
    // just incase...)
    // 12/10/99 - Bud Adams  -  Removed 5 more CURRENT_DATE functions that have 
    // been added since May; fixed 2 Read actions of Obligation_Payment_Schedule
    // that were not fully qualified - they were missing CSE_Person Number,
    // etc.
    // ---------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------
    // 5/22/07 - MWF - PR#294742 Modified to create a new infrastructure record 
    // each time regardless of
    // the print_succesful_indicator of local outgoing_document.
    // ---------------------------------------------------------------------------------------
    // #################################################
    // ##
    // ##   When the current date is required, refer to the work
    // ##   attribute  local_current_date and NOT the IEF supplied
    // ##   function.  That function results in a huge amount of
    // ##   overhead.
    // ##
    // #################################################
    // *****
    // Set Initial Exit State.
    // *****
    ExitState = "ACO_NN0000_ALL_OK";

    // *****
    // Move Imports to Exports.
    // *****
    local.UnsuccessfulOpCount.Count = 0;

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    export.CsePerson.Number = import.CsePerson.Number;
    export.HiddenConcurrentCsePerson.Number =
      import.HiddenConcurrentCsePerson.Number;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    MoveLegalAction(import.LegalAction, export.LegalAction);
    MoveObligation(import.Obligation, export.Obligation);
    export.HiddenConcurrentObligation.SystemGeneratedIdentifier =
      import.HiddenConcurrentObligation.SystemGeneratedIdentifier;
    export.ObligationType.Assign(import.ObligationType);
    export.HiddenConcurrentObligationType.SystemGeneratedIdentifier =
      import.HiddenConcurrentObligationType.SystemGeneratedIdentifier;
    export.DebtDetail.DueDt = import.DebtDetail.DueDt;
    MoveLegalActionDetail(import.LegalActionDetail, export.LegalActionDetail);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // Move Imports to Exports for Current, Arrears, Interest, Total Owed & 
    // Amounts Error line.
    export.ScreenOwedAmounts.Assign(import.ScreenOwedAmounts);
    export.TotalAmountDue.TotalCurrency = import.TotalAmountDue.TotalCurrency;
    export.AccrualOrDue.Date = import.AccrualOrDue.Date;

    // *** Move Imports to Exports for Add Payment Schedule Line. ***
    MoveFrequencyWorkSet(import.AddLineFrequencyWorkSet,
      export.AddLineFrequencyWorkSet);
    export.SearchFrequency.SelectChar = import.SearchFrequency.SelectChar;
    export.AddLineFreqPeriod.Text13 = import.AddLineFreqPeriod.Text13;
    MoveObligationPaymentSchedule4(import.AddLineObligationPaymentSchedule,
      export.AddLineObligationPaymentSchedule);

    // *****
    // Move Imports to Exports for Obligation Payment Schedule Listing.
    // *****
    export.PaymentSchedule.Index = 0;
    export.PaymentSchedule.Clear();

    for(import.PaymentSchedule.Index = 0; import.PaymentSchedule.Index < import
      .PaymentSchedule.Count; ++import.PaymentSchedule.Index)
    {
      if (export.PaymentSchedule.IsFull)
      {
        break;
      }

      export.PaymentSchedule.Update.DetailAction.ActionEntry =
        import.PaymentSchedule.Item.DetailAction.ActionEntry;
      MoveFrequencyWorkSet(import.PaymentSchedule.Item.DetailFrequencyWorkSet,
        export.PaymentSchedule.Update.DetailFrequencyWorkSet);
      export.PaymentSchedule.Update.DetailObligationPaymentSchedule.Assign(
        import.PaymentSchedule.Item.DetailObligationPaymentSchedule);
      export.PaymentSchedule.Update.DtlPrevHidden.Assign(
        import.PaymentSchedule.Item.DtlPrevHidden);
      export.PaymentSchedule.Update.DetailCreateDate.Date =
        import.PaymentSchedule.Item.DetailCreateDate.Date;
      export.PaymentSchedule.Next();
    }

    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(global.Command, "CLEAR"))
    {
      export.PaymentSchedule.Index = 0;
      export.PaymentSchedule.Clear();

      for(import.PaymentSchedule.Index = 0; import.PaymentSchedule.Index < import
        .PaymentSchedule.Count; ++import.PaymentSchedule.Index)
      {
        if (export.PaymentSchedule.IsFull)
        {
          break;
        }

        export.PaymentSchedule.Update.DetailAction.ActionEntry = "";
        export.PaymentSchedule.Update.DetailFrequencyWorkSet.FrequencyCode = "";
        export.PaymentSchedule.Update.DetailFrequencyWorkSet.
          FrequencyDescription = "";
        export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
          DayOfMonth1 = 0;
        export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
          DayOfMonth2 = 0;
        export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
          DayOfWeek = 0;
        export.PaymentSchedule.Update.DetailObligationPaymentSchedule.Amount =
          0;
        export.PaymentSchedule.Update.DetailObligationPaymentSchedule.EndDt =
          local.Current.Date;
        export.PaymentSchedule.Update.DetailObligationPaymentSchedule.StartDt =
          local.Current.Date;
        export.PaymentSchedule.Update.DetailCreateDate.Date =
          local.Current.Date;
        export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
          LastUpdateBy = "";
        export.PaymentSchedule.Next();
      }

      export.AddLineObligationPaymentSchedule.Amount = 0;
      export.AddLineFrequencyWorkSet.FrequencyCode = "";
      export.AddLineFrequencyWorkSet.FrequencyDescription = "";
      export.AddLineObligationPaymentSchedule.FrequencyCode = "";
      export.AddLineFreqPeriod.Text13 = "";
      export.AddLineObligationPaymentSchedule.DayOfMonth1 = 0;
      export.AddLineObligationPaymentSchedule.DayOfMonth2 = 0;
      export.AddLineObligationPaymentSchedule.DayOfWeek = 0;
      export.AddLineObligationPaymentSchedule.EndDt =
        local.InitializedAddLineObligationPaymentSchedule.EndDt;
      export.AddLineObligationPaymentSchedule.StartDt =
        local.InitializedAddLineObligationPaymentSchedule.StartDt;

      return;
    }

    if (Equal(global.Command, "RETURN"))
    {
      ExitState = "ACO_NE0000_RETURN";

      return;
    }

    // *****
    // Get valid hardcode values for local entity view attributes.
    // *****
    UseFnHardcodedDebtDistribution();

    // : If command is FIRSTIME, we came here from an Obligation Maintenance 
    // screen.
    //   Save last command to bypass error condition for no Payment Schedules 
    // found.
    if (Equal(global.Command, "FIRSTIME"))
    {
      local.Previous.Command = global.Command;
      export.AddLineObligationPaymentSchedule.Amount = 0;
      global.Command = "DISPLAY";
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
      local.NextTranInfo.CsePersonNumberAp = export.Obligor.Number;
      local.NextTranInfo.StandardCrtOrdNumber =
        export.LegalAction.StandardNumber ?? "";
      local.NextTranInfo.LegalActionIdentifier = export.LegalAction.Identifier;
      local.NextTranInfo.ObligationId =
        export.Obligation.SystemGeneratedIdentifier;
      local.NextTranInfo.CsePersonNumber = export.CsePerson.Number;
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
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();
      export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      export.LegalAction.Identifier =
        export.Hidden.LegalActionIdentifier.GetValueOrDefault();
      export.Obligation.SystemGeneratedIdentifier =
        export.Hidden.ObligationId.GetValueOrDefault();
      export.ObligationType.SystemGeneratedIdentifier =
        export.Hidden.ObligationId.GetValueOrDefault();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // **** begin group C ****
    // to validate action level security
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // * continue *
    }
    else
    {
      return;
    }

    // **** end   group C ****
    // *****
    // Main Case of Command.
    // *****
    if (!IsEmpty(export.AddLineFrequencyWorkSet.FrequencyCode) || Lt
      (local.InitializedDate.Date,
      export.AddLineObligationPaymentSchedule.StartDt) || export
      .AddLineObligationPaymentSchedule.Amount.GetValueOrDefault() != 0 || Lt
      (local.InitializedDate.Date, export.AddLineObligationPaymentSchedule.EndDt)
      || export
      .AddLineObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault() != 0
      || export
      .AddLineObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault() != 0
      || export
      .AddLineObligationPaymentSchedule.DayOfWeek.GetValueOrDefault() != 0)
    {
      if (IsEmpty(import.AddLineFrequencyWorkSet.FrequencyCode))
      {
        var field = GetField(export.AddLineFrequencyWorkSet, "frequencyCode");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (Equal(export.AddLineObligationPaymentSchedule.StartDt,
        local.InitializedDate.Date))
      {
        var field =
          GetField(export.AddLineObligationPaymentSchedule, "startDt");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (import.AddLineObligationPaymentSchedule.Amount.GetValueOrDefault() ==
        0)
      {
        var field = GetField(export.AddLineObligationPaymentSchedule, "amount");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (IsExitState("ACO_NE0000_REQUIRED_DATA_MISSING"))
      {
        return;
      }
    }

    local.MultipleActions.Count = 0;

    for(import.PaymentSchedule.Index = 0; import.PaymentSchedule.Index < import
      .PaymentSchedule.Count; ++import.PaymentSchedule.Index)
    {
      if (!IsEmpty(import.PaymentSchedule.Item.DetailAction.ActionEntry) && !
        Equal(import.PaymentSchedule.Item.DetailAction.ActionEntry, "*"))
      {
        ++local.MultipleActions.Count;
      }
    }

    if (local.MultipleActions.Count > 0)
    {
      if (!IsEmpty(export.AddLineFrequencyWorkSet.FrequencyCode))
      {
        ExitState = "FN0000_MULT_ACTNS_NOT_ALLOWED";

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        local.CsePersonsWorkSet.Number = export.CsePerson.Number;
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        if (import.ObligationType.SystemGeneratedIdentifier > 0)
        {
          if (ReadObligationObligationType())
          {
            MoveObligation(entities.Obligation, export.Obligation);
            export.ObligationType.Assign(entities.ObligationType);

            // *****
            // If obligation type is Voluntary, Accruing, & Health Insurance, it
            // is invalid
            // for debt detail payment schedule.
            // *****
            if (AsChar(entities.ObligationType.Classification) == AsChar
              (local.HardcodedVoluntary.Classification) || AsChar
              (entities.ObligationType.Classification) == AsChar
              (local.HardcodedHealthIns.Classification) || AsChar
              (entities.ObligationType.Classification) == AsChar
              (local.HardcodedAccruing.Classification))
            {
              ExitState = "FN0000_OBLIG_TYPE_PY_SCH_INVALID";
            }
          }
          else
          {
            ExitState = "FN0000_OBLIGATION_NF";
          }
        }
        else
        {
          ExitState = "LE0000_OBLIG_MUST_BE_SELECTED";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        // : Read Legal Action Detail to see if it has the same payment schedule
        // start date.
        //   If it does, this condition is valid.
        //   Read any one of the Legal Action Details for current Obligation.
        if (ReadLegalActionDetail1())
        {
          MoveLegalActionDetail(entities.LegalActionDetail,
            export.LegalActionDetail);
        }
        else
        {
          // ** OK **
        }

        if (!IsEmpty(import.HiddenConcurrentCsePerson.Number))
        {
          // =================================================
          // 5/13/99 - bud adams  -  The exception condition was missing
          //   and there was a note saying that it would cause an abort.
          //   We do not want an abort.
          // =================================================
          if (!ReadObligation())
          {
            ExitState = "FN0000_CONCURRENT_OBLIGATION_NF";

            return;
          }
        }

        local.IncludeCrdInd.Flag = "Y";

        // *****
        // This Common Action Block calculates Current, Arrears, Interest & 
        // Total Owed for an Obligation.  The Payment Schedule List/Maintain
        // Screen displays totals.  It also displays a message when monthly
        // obligor summary info is not available.
        // 5/13/99 - b adams  -  new CAB to replace old one.
        // *****
        UseFnComputeSummaryTotals();

        if (IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
        {
          // * continue *
        }
        else
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Error = true;
        }

        // ..................
        // Check EXIT STATEs
        // ..................
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // *** Use this CAB to calculate Obligation Amount, that will be 
        // displayed by the screen. Added by G Sharp 11/24/98. ***
        UseFnCabSetAccrualOrDueAmount();

        // =================================================
        // This Read may return more than one row, one for each
        // supported person.  But it doesn't matter, because all we
        // REALLY want is to get a Debt_Detail record and use that
        // Due_Dt as an edit check.  All of these have identical data
        // and all those due dates will be the same.
        // This is only applicable to Non-Accruing Obligations.
        // =================================================
        if (!ReadObligationTransaction())
        {
          ExitState = "FN0000_OBLIG_TRANS_NF";

          return;
        }

        if (ReadDebtDetail())
        {
          export.DebtDetail.DueDt = entities.DebtDetail.DueDt;
        }
        else
        {
          ExitState = "FN0211_DEBT_DETAIL_NF";

          return;
        }

        export.PaymentSchedule.Index = 0;
        export.PaymentSchedule.Clear();

        foreach(var item in ReadObligationPaymentSchedule4())
        {
          MoveObligationPaymentSchedule2(entities.ObligationPaymentSchedule,
            export.PaymentSchedule.Update.DetailObligationPaymentSchedule);
          MoveObligationPaymentSchedule5(entities.ObligationPaymentSchedule,
            export.PaymentSchedule.Update.DtlPrevHidden);
          export.PaymentSchedule.Update.DetailAction.ActionEntry = "";
          export.PaymentSchedule.Update.DetailCreateDate.Date =
            Date(entities.ObligationPaymentSchedule.CreatedTmst);

          if (Equal(entities.ObligationPaymentSchedule.EndDt, local.Maximum.Date))
            
          {
            export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
              EndDt = local.InitializedDate.Date;
          }

          // *** G Sharp 10-27-98. Added to screen Worked ID. This could be last
          // updated by, or if no last update by then created by. ***
          if (IsEmpty(entities.ObligationPaymentSchedule.LastUpdateBy))
          {
            export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
              LastUpdateBy = entities.ObligationPaymentSchedule.CreatedBy;
          }

          local.CodeCode.CodeName = "FREQUENCY";
          local.CodeCodeValue.Cdvalue =
            entities.ObligationPaymentSchedule.FrequencyCode;
          export.PaymentSchedule.Update.DetailFrequencyWorkSet.FrequencyCode =
            entities.ObligationPaymentSchedule.FrequencyCode;
          UseCabGetCodeValueDescription();

          switch(local.CodeReturnCode.Count)
          {
            case 0:
              if (Lt(local.Current.Date, local.CodeCodeValue.ExpirationDate))
              {
                export.PaymentSchedule.Update.DetailFrequencyWorkSet.
                  FrequencyDescription = local.CodeCodeValue.Description;
              }
              else
              {
                var field =
                  GetField(export.PaymentSchedule.Item.DetailFrequencyWorkSet,
                  "frequencyCode");

                field.Error = true;

                ExitState = "FN0000_CODE_VALUE_EXPIRED";
              }

              break;
            case 4:
              ExitState = "FN0000_FREQUENCY_CODE_UNKNOWN";

              var field1 =
                GetField(export.PaymentSchedule.Item.DetailFrequencyWorkSet,
                "frequencyCode");

              field1.Error = true;

              break;
            default:
              var field2 =
                GetField(export.PaymentSchedule.Item.DetailFrequencyWorkSet,
                "frequencyCode");

              field2.Error = true;

              ExitState = "FN0000_FREQUENCY_CODE_UNKNOWN";

              break;
          }

          export.PaymentSchedule.Next();
        }

        if (export.PaymentSchedule.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

          return;
        }

        if (export.PaymentSchedule.IsEmpty)
        {
          if (Equal(local.Previous.Command, "FIRSTIME"))
          {
          }
          else
          {
            ExitState = "FN0000_OBLIG_PYMNT_SCH_NF";

            return;
          }
        }

        // *****
        // Initialized Add Line Fields on Display.
        // *****
        if (Equal(local.Previous.Command, "FIRSTIME"))
        {
          // : If prev command was firstime, Add Line values have been imported 
          // from
          //   another screen. Do not initialize Add Line.
        }
        else
        {
          export.AddLineFrequencyWorkSet.FrequencyCode =
            local.InitializedAddLineFrequencyWorkSet.FrequencyCode;
          export.AddLineFrequencyWorkSet.FrequencyDescription =
            local.InitializedAddLineFrequencyWorkSet.FrequencyDescription;
          export.AddLineFreqPeriod.Text13 =
            local.InitializedAddLineWorkArea.Text13;
          export.AddLineObligationPaymentSchedule.DayOfMonth1 =
            local.InitializedAddLineObligationPaymentSchedule.DayOfMonth1.
              GetValueOrDefault();
          export.AddLineObligationPaymentSchedule.DayOfMonth2 =
            local.InitializedAddLineObligationPaymentSchedule.DayOfMonth2.
              GetValueOrDefault();
          export.AddLineObligationPaymentSchedule.DayOfWeek =
            local.InitializedAddLineObligationPaymentSchedule.DayOfWeek.
              GetValueOrDefault();
          export.AddLineObligationPaymentSchedule.StartDt =
            local.InitializedAddLineObligationPaymentSchedule.StartDt;
          export.AddLineObligationPaymentSchedule.Amount =
            local.InitializedAddLineObligationPaymentSchedule.Amount.
              GetValueOrDefault();
          export.AddLineObligationPaymentSchedule.EndDt =
            local.InitializedAddLineObligationPaymentSchedule.EndDt;
        }

        break;
      case "PROCESS":
        // *****
        // This case allows multiple processing for adding, deleting, & 
        // modifying obligation payment schedules.  However, only one payment
        // schedule can be added at one time.
        // *****
        for(export.PaymentSchedule.Index = 0; export.PaymentSchedule.Index < export
          .PaymentSchedule.Count; ++export.PaymentSchedule.Index)
        {
          switch(TrimEnd(export.PaymentSchedule.Item.DetailAction.ActionEntry))
          {
            case "*":
              break;
            case "":
              break;
            case "D":
              // *****
              // Don't allow add, update, or delete if the Payment Schedule is 
              // the result
              // of a legal action
              // *****
              if (ReadLegalActionDetail2())
              {
                if (!IsEmpty(entities.LegalActionDetail.FreqPeriodCode))
                {
                  var field1 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "amount");

                  field1.Error = true;

                  var field2 =
                    GetField(export.PaymentSchedule.Item.DetailFrequencyWorkSet,
                    "frequencyCode");

                  field2.Error = true;

                  var field3 =
                    GetField(export.PaymentSchedule.Item.DetailFrequencyWorkSet,
                    "frequencyDescription");

                  field3.Error = true;

                  var field4 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "dayOfMonth1");

                  field4.Error = true;

                  var field5 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "dayOfMonth2");

                  field5.Error = true;

                  var field6 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "dayOfWeek");

                  field6.Error = true;

                  var field7 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "startDt");

                  field7.Error = true;

                  var field8 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "endDt");

                  field8.Error = true;

                  var field9 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "lastUpdateBy");

                  field9.Error = true;

                  ExitState = "FN0000_OPS_EST_BY_LDET";

                  return;
                }
              }

              break;
            case "C":
              // *****
              // Don't allow add, update, or delete if the Payment Schedule is 
              // the result
              // of a legal action
              // *****
              if (ReadLegalActionDetail2())
              {
                if (!IsEmpty(entities.LegalActionDetail.FreqPeriodCode))
                {
                  ExitState = "FN0000_OPS_EST_BY_LDET";

                  var field1 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "amount");

                  field1.Error = true;

                  var field2 =
                    GetField(export.PaymentSchedule.Item.DetailFrequencyWorkSet,
                    "frequencyCode");

                  field2.Error = true;

                  var field3 =
                    GetField(export.PaymentSchedule.Item.DetailFrequencyWorkSet,
                    "frequencyDescription");

                  field3.Error = true;

                  var field4 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "dayOfMonth1");

                  field4.Error = true;

                  var field5 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "dayOfMonth2");

                  field5.Error = true;

                  var field6 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "dayOfWeek");

                  field6.Error = true;

                  var field7 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "startDt");

                  field7.Error = true;

                  var field8 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "endDt");

                  field8.Error = true;

                  return;
                }
              }

              break;
            default:
              break;
          }

          switch(TrimEnd(export.PaymentSchedule.Item.DetailAction.ActionEntry))
          {
            case "*":
              // Reset the Action field to spaces for processed obligation 
              // payment schedules.
              export.PaymentSchedule.Update.DetailAction.ActionEntry = "";

              continue;
            case "":
              continue;
            case "D":
              ++local.ActionCounter.Count;

              // *****
              // Delete Obligation Payment Schedule.
              // *****
              // *****
              // Don't allow delete if the start date is less than the current 
              // date
              // (the schedule is active).
              // *****
              UseFnDeleteObligationPymntSch1();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                // :Aug 7, 1999, mfb - If it's a recovery obligation, handle
                //  RCAPRPAY document processing.
                if (AsChar(export.ObligationType.Classification) == AsChar
                  (local.HardcodeRecovery.Classification))
                {
                  // : If the payment schedule was created today, cancel the 
                  // RCAPRPAY document.
                  if (Equal(export.PaymentSchedule.Item.DetailCreateDate.Date,
                    local.Current.Date))
                  {
                    if (ReadObligorRule())
                    {
                      // : IF a document has been generated for this payment 
                      // schedule, cancel it.
                      local.Document.Name = "RCAPRPAY";
                      local.Infrastructure.ReferenceDate = local.Current.Date;
                      local.SpDocKey.KeyRecaptureRule =
                        entities.ObligorRule.SystemGeneratedIdentifier;
                      UseSpDocFindOutgoingDocument();

                      if (local.Infrastructure.SystemGeneratedIdentifier > 0)
                      {
                        // mjr---> A document already exists.  Check 
                        // outgoing_doc print_succesfful_ind to determine
                        // action.
                        switch(AsChar(local.OutgoingDocument.
                          PrintSucessfulIndicator))
                        {
                          case 'Y':
                            // mjr---> Printed successfully
                            local.Infrastructure.SystemGeneratedIdentifier = 0;

                            break;
                          case 'N':
                            // mjr---> NOT Printed successfully
                            UseSpDocCancelOutgoingDoc();

                            break;
                          case 'G':
                            // mjr---> Awaiting generation
                            UseSpDocCancelOutgoingDoc();

                            break;
                          case 'B':
                            // mjr---> Awaiting Natural batch print
                            UseSpDocCancelOutgoingDoc();

                            break;
                          case 'C':
                            // mjr---> Printing canceled
                            break;
                          default:
                            break;
                        }
                      }
                    }
                    else
                    {
                      ExitState = "FN0000_RECAPTURE_RULE_NF_RB";
                    }
                  }
                }
              }
              else if (IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
              {
              }
              else if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
              {
              }
              else if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
              }
              else
              {
                var field1 =
                  GetField(export.PaymentSchedule.Item.DetailAction,
                  "actionEntry");

                field1.Error = true;

                var field2 =
                  GetField(export.PaymentSchedule.Item.DetailFrequencyWorkSet,
                  "frequencyDescription");

                field2.Error = true;

                var field3 =
                  GetField(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule, "dayOfMonth1");

                field3.Error = true;

                var field4 =
                  GetField(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule, "dayOfMonth2");

                field4.Error = true;

                var field5 =
                  GetField(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule, "dayOfWeek");

                field5.Error = true;

                var field6 =
                  GetField(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule, "startDt");

                field6.Error = true;

                var field7 =
                  GetField(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule, "amount");

                field7.Error = true;

                var field8 =
                  GetField(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule, "endDt");

                field8.Error = true;

                return;
              }

              // : Perform the same processing for Concurrent Obligor, if there 
              // is one.
              if (!IsEmpty(import.HiddenConcurrentCsePerson.Number))
              {
                UseFnDeleteObligationPymntSch2();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                }
                else if (IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
                {
                }
                else if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
                {
                }
                else if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
                {
                }
                else
                {
                  var field1 =
                    GetField(export.PaymentSchedule.Item.DetailAction,
                    "actionEntry");

                  field1.Error = true;

                  var field2 =
                    GetField(export.PaymentSchedule.Item.DetailFrequencyWorkSet,
                    "frequencyDescription");

                  field2.Error = true;

                  var field3 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "dayOfMonth1");

                  field3.Error = true;

                  var field4 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "dayOfMonth2");

                  field4.Error = true;

                  var field5 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "dayOfWeek");

                  field5.Error = true;

                  var field6 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "startDt");

                  field6.Error = true;

                  var field7 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "amount");

                  field7.Error = true;

                  var field8 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "endDt");

                  field8.Error = true;

                  return;
                }
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.PaymentSchedule.Update.DetailAction.ActionEntry = "*";

                if (Equal(export.PaymentSchedule.Item.
                  DetailObligationPaymentSchedule.EndDt, local.Maximum.Date))
                {
                  export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                    EndDt = local.InitializedDate.Date;
                }

                // *** G Sharp 10-27-98. Added to screen Worked ID. Set last 
                // updated by to user_id. ***
                export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                  LastUpdateBy = global.UserId;
                ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
              }

              // *** CHANGE start. Note added by G Sharp. ***
              break;
            case "C":
              ++local.ActionCounter.Count;

              if (Equal(export.PaymentSchedule.Item.
                DetailObligationPaymentSchedule.EndDt,
                local.InitializedDate.Date))
              {
                export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                  EndDt = local.Maximum.Date;
              }

              // *****
              // If no data has changed inform user that update is not done
              // *****
              if (export.PaymentSchedule.Item.DtlPrevHidden.Amount.
                GetValueOrDefault() == export
                .PaymentSchedule.Item.DetailObligationPaymentSchedule.Amount.
                  GetValueOrDefault() && Equal
                (export.PaymentSchedule.Item.DtlPrevHidden.EndDt,
                export.PaymentSchedule.Item.DetailObligationPaymentSchedule.
                  EndDt) && Equal
                (export.PaymentSchedule.Item.DtlPrevHidden.StartDt,
                export.PaymentSchedule.Item.DetailObligationPaymentSchedule.
                  StartDt))
              {
                if (Equal(export.PaymentSchedule.Item.
                  DetailObligationPaymentSchedule.EndDt, local.Maximum.Date))
                {
                  export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                    EndDt = local.InitializedDate.Date;
                }

                ExitState = "FN0000_NO_CHANGE_TO_UPDATE";

                return;
              }

              // *****
              // Disallow change to the start date.
              // *****
              if (!Equal(export.PaymentSchedule.Item.
                DetailObligationPaymentSchedule.StartDt,
                export.PaymentSchedule.Item.DtlPrevHidden.StartDt))
              {
                ExitState = "CANNOT_CHANGE_EFFECTIVE_DATE";
                export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                  StartDt = export.PaymentSchedule.Item.DtlPrevHidden.StartDt;

                var field1 =
                  GetField(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule, "startDt");

                field1.Error = true;

                if (Equal(export.PaymentSchedule.Item.
                  DetailObligationPaymentSchedule.EndDt, local.Maximum.Date))
                {
                  export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                    EndDt = local.InitializedDate.Date;
                }

                return;
              }

              if (!Equal(export.PaymentSchedule.Item.
                DetailObligationPaymentSchedule.EndDt,
                export.PaymentSchedule.Item.DtlPrevHidden.EndDt))
              {
                // *****
                // Do not allow end date to be earlier than the Start date.
                // *****
                // : Aug 8, 1999, mfb - changed the 'LESS THAN' to 'LESS OR 
                // EQUAL TO' in the following statement.
                if (!Lt(export.PaymentSchedule.Item.
                  DetailObligationPaymentSchedule.StartDt,
                  export.PaymentSchedule.Item.DetailObligationPaymentSchedule.
                    EndDt))
                {
                  export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                    EndDt = export.PaymentSchedule.Item.DtlPrevHidden.EndDt;

                  var field1 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "endDt");

                  field1.Error = true;

                  ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

                  if (Equal(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule.EndDt, local.Maximum.Date))
                  {
                    export.PaymentSchedule.Update.
                      DetailObligationPaymentSchedule.EndDt =
                        local.InitializedDate.Date;
                  }

                  return;
                }

                // *****
                // Do not allow the discontinue date to be less than the current
                // date.
                // *****
                if (Lt(export.PaymentSchedule.Item.
                  DetailObligationPaymentSchedule.EndDt, local.Current.Date) &&
                  !
                  Equal(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule.EndDt,
                  local.InitializedDate.Date))
                {
                  export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                    EndDt = export.PaymentSchedule.Item.DtlPrevHidden.EndDt;

                  var field1 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "endDt");

                  field1.Error = true;

                  ExitState = "FN0000_DISC_DATE_LESS_CURRENT_DT";

                  if (Equal(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule.EndDt, local.Maximum.Date))
                  {
                    export.PaymentSchedule.Update.
                      DetailObligationPaymentSchedule.EndDt =
                        local.InitializedDate.Date;
                  }

                  return;
                }
              }

              if (!Lt(export.PaymentSchedule.Item.DtlPrevHidden.StartDt,
                local.Current.Date))
              {
              }
              else
              {
                // *****
                // Schedule is active. So only allow discontinued date to be 
                // changed.  The disc date can only be changed to current,
                // future or blanks.
                // *****
                if (export.PaymentSchedule.Item.DtlPrevHidden.Amount.
                  GetValueOrDefault() != export
                  .PaymentSchedule.Item.DetailObligationPaymentSchedule.Amount.
                    GetValueOrDefault())
                {
                  export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                    Amount =
                      export.PaymentSchedule.Item.DtlPrevHidden.Amount.
                      GetValueOrDefault();

                  var field1 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "amount");

                  field1.Error = true;

                  ExitState = "CANNOT_CHANGE_AMOUNT";

                  if (Equal(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule.EndDt, local.Maximum.Date))
                  {
                    export.PaymentSchedule.Update.
                      DetailObligationPaymentSchedule.EndDt =
                        local.InitializedDate.Date;
                  }

                  return;
                }

                // *****
                // If the discontinue date has been changed on screen, 
                // discontinue obligation payment schedule processing will
                // occur.
                // *****
                if (!Equal(export.PaymentSchedule.Item.DtlPrevHidden.EndDt,
                  export.PaymentSchedule.Item.DetailObligationPaymentSchedule.
                    EndDt))
                {
                  if (Lt(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule.EndDt,
                    local.Current.Date) && !
                    Equal(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule.EndDt,
                    local.InitializedDate.Date))
                  {
                    export.PaymentSchedule.Update.
                      DetailObligationPaymentSchedule.EndDt =
                        export.PaymentSchedule.Item.DtlPrevHidden.EndDt;

                    var field1 =
                      GetField(export.PaymentSchedule.Item.
                        DetailObligationPaymentSchedule, "endDt");

                    field1.Error = true;

                    ExitState = "FN0000_DISC_DATE_LESS_CURRENT_DT";

                    if (Equal(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule.EndDt,
                      local.Maximum.Date))
                    {
                      export.PaymentSchedule.Update.
                        DetailObligationPaymentSchedule.EndDt =
                          local.InitializedDate.Date;
                    }

                    return;
                  }

                  local.PaymentSchDiscontinue.Date =
                    export.PaymentSchedule.Item.DetailObligationPaymentSchedule.
                      EndDt;

                  // =================================================
                  // 12/10/99 - b adams  -  PR# 81828: Read was not properly
                  //   qualified; it was missing the relationship to cse_person.
                  //   Also missing was the rel to ob_type and the "R" type.
                  // =================================================
                  if (ReadObligationPaymentSchedule2())
                  {
                    // *****
                    // Okay, proceed with discontinue payment schedule 
                    // processing.
                    // *****
                  }
                  else
                  {
                    ExitState = "FN0000_OBLIG_PYMNT_SCH_NF";

                    if (Equal(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule.EndDt,
                      local.Maximum.Date))
                    {
                      export.PaymentSchedule.Update.
                        DetailObligationPaymentSchedule.EndDt =
                          local.InitializedDate.Date;
                    }

                    return;
                  }

                  // ***
                  // R.B.M    08-22-97
                  // If Input End-date < previous End-date, then no need to 
                  // check for overlapping dates
                  // ***
                  if (!Lt(export.PaymentSchedule.Item.DtlPrevHidden.EndDt,
                    export.PaymentSchedule.Item.DetailObligationPaymentSchedule.
                      EndDt))
                  {
                    // *** Bypass Overlapping date check and proceed to do the 
                    // change ***
                  }
                  else
                  {
                    // *****
                    // R.B.M      08-22-97  Changed the Read- to find the 
                    // Overlapping schedules.
                    // Check for overlapping dates if end date change
                    // *****
                    // =================================================
                    // 12/10/99 - b adams  -  PR# 81828: Read was not properly
                    //   qualified; it was missing the relationship to 
                    // cse_person.
                    //   Also missing was the rel to ob_type and the "R" type.
                    // =================================================
                    if (ReadObligationPaymentSchedule1())
                    {
                      export.PaymentSchedule.Update.
                        DetailObligationPaymentSchedule.EndDt =
                          export.PaymentSchedule.Item.DtlPrevHidden.EndDt;

                      var field1 =
                        GetField(export.PaymentSchedule.Item.
                          DetailObligationPaymentSchedule, "endDt");

                      field1.Error = true;

                      ExitState = "OVERLAPPING_DATE_RANGE";

                      return;
                    }
                    else
                    {
                      // *****
                      // No conflicts
                      // *****
                    }
                  }

                  // *****
                  // Discontinue Obligation Payment Schedule.
                  // *****
                  UseFnDiscObligationPymntSch();

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.PaymentSchedule.Update.DetailAction.ActionEntry =
                      "*";

                    // *****
                    // Display Obligation Payment Schedule End Date as blank on 
                    // screen when = to Maximum Date.
                    // *****
                    if (Equal(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule.EndDt,
                      local.Maximum.Date))
                    {
                      export.PaymentSchedule.Update.
                        DetailObligationPaymentSchedule.EndDt =
                          local.InitializedDate.Date;
                    }

                    // *** G Sharp 10-27-98. Added to screen Worked ID. Set last
                    // updated by to user_id. ***
                    export.PaymentSchedule.Update.
                      DetailObligationPaymentSchedule.LastUpdateBy =
                        global.UserId;
                    MoveObligationPaymentSchedule5(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule,
                      export.PaymentSchedule.Update.DtlPrevHidden);
                    ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

                    continue;
                  }
                  else if (IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
                  {
                  }
                  else if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
                  {
                  }
                  else if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
                  {
                  }
                  else
                  {
                    return;
                  }
                }
              }

              // ***
              //    The Original Payment_Sch Start-date >= Current Date ; So 
              // Future Payment_Schedule.
              //    Already Validated that :
              //       - Start-Date can not be changed
              //       - End-date can not be less than the Start_Date
              //       - End-date can not be less than Current Date
              // ***
              if (!Equal(export.PaymentSchedule.Item.
                DetailObligationPaymentSchedule.EndDt,
                export.PaymentSchedule.Item.DtlPrevHidden.EndDt))
              {
                if (Lt(export.PaymentSchedule.Item.
                  DetailObligationPaymentSchedule.EndDt, local.Current.Date))
                {
                  export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                    EndDt = export.PaymentSchedule.Item.DtlPrevHidden.EndDt;

                  var field1 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "endDt");

                  field1.Error = true;

                  ExitState = "DISC_DATE_NOT_GREATER_CURRENT";

                  if (Equal(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule.EndDt, local.Maximum.Date))
                  {
                    export.PaymentSchedule.Update.
                      DetailObligationPaymentSchedule.EndDt =
                        local.InitializedDate.Date;
                  }

                  return;
                }

                if (Lt(export.PaymentSchedule.Item.
                  DetailObligationPaymentSchedule.EndDt,
                  export.PaymentSchedule.Item.DetailObligationPaymentSchedule.
                    StartDt))
                {
                  export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                    EndDt = export.PaymentSchedule.Item.DtlPrevHidden.EndDt;

                  var field1 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "startDt");

                  field1.Error = true;

                  var field2 =
                    GetField(export.PaymentSchedule.Item.
                      DetailObligationPaymentSchedule, "endDt");

                  field2.Error = true;

                  ExitState = "FN0000_INVALID_DISCONTINUE_DATE";

                  if (Equal(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule.EndDt, local.Maximum.Date))
                  {
                    export.PaymentSchedule.Update.
                      DetailObligationPaymentSchedule.EndDt =
                        local.InitializedDate.Date;
                  }

                  return;
                }
              }

              if (export.PaymentSchedule.Item.DetailObligationPaymentSchedule.
                Amount.GetValueOrDefault() == 0)
              {
                export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                  Amount =
                    export.PaymentSchedule.Item.DtlPrevHidden.Amount.
                    GetValueOrDefault();

                var field1 =
                  GetField(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule, "amount");

                field1.Error = true;

                ExitState = "FN0000_AMT_CANNOT_BE_NEGATIVE";

                if (Equal(export.PaymentSchedule.Item.
                  DetailObligationPaymentSchedule.EndDt, local.Maximum.Date))
                {
                  export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                    EndDt = local.InitializedDate.Date;
                }

                return;
              }

              if (export.PaymentSchedule.Item.DetailObligationPaymentSchedule.
                Amount.GetValueOrDefault() > export
                .ScreenOwedAmounts.TotalAmountOwed)
              {
                var field1 =
                  GetField(export.ScreenOwedAmounts, "totalAmountOwed");

                field1.Error = true;

                var field2 =
                  GetField(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule, "amount");

                field2.Error = true;

                ExitState = "FN0000_PRD_AMT_GRTR_THN_AMT_OWED";

                if (Equal(export.PaymentSchedule.Item.
                  DetailObligationPaymentSchedule.EndDt, local.Maximum.Date))
                {
                  export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                    EndDt = local.InitializedDate.Date;
                }

                return;
              }

              // *****
              // Update Obligation Payment Schedule.
              // *****
              UseFnUpdateObligationPymntSch1();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                // : Perform the same processing for Concurrent Obligor, if 
                // there is one.
                if (!IsEmpty(export.HiddenConcurrentCsePerson.Number))
                {
                  UseFnUpdateObligationPymntSch2();
                }

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.PaymentSchedule.Update.DetailAction.ActionEntry = "*";

                  if (Equal(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule.EndDt, local.Maximum.Date))
                  {
                    export.PaymentSchedule.Update.
                      DetailObligationPaymentSchedule.EndDt =
                        local.InitializedDate.Date;
                  }

                  // *** G Sharp 10-27-98. Added to screen Worked ID. Set last 
                  // updated by to user_id. ***
                  export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                    LastUpdateBy = global.UserId;
                  MoveObligationPaymentSchedule5(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule,
                    export.PaymentSchedule.Update.DtlPrevHidden);
                  ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

                  // : Aug 5, 1999, mfb - If this is a recovery obligation, set 
                  // a flag
                  //   to indicate that the RCAPRPAY document should be 
                  // generated.
                  //  Also save the action so that the exitstate can be reset.
                  if (AsChar(export.ObligationType.Classification) == AsChar
                    (local.HardcodeRecovery.Classification))
                  {
                    local.Rcaprpay.Flag = "Y";
                    local.SaveAction.Command = "UPDATE";
                  }
                }
                else
                {
                  return;
                }
              }
              else if (IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
              {
              }
              else if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
                // : Aug 5, 1999, mfb - If this is a recovery obligation, set a 
                // flag
                //   to indicate that the RCAPRPAY document should be generated.
                //  Also save the action so that the exitstate can be reset.
                if (AsChar(export.ObligationType.Classification) == AsChar
                  (local.HardcodeRecovery.Classification))
                {
                  local.Rcaprpay.Flag = "Y";
                  local.SaveAction.Command = "UPDATE";
                }
              }
              else if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
              {
              }
              else
              {
                var field1 =
                  GetField(export.PaymentSchedule.Item.DetailAction,
                  "actionEntry");

                field1.Error = true;

                var field2 =
                  GetField(export.PaymentSchedule.Item.DetailFrequencyWorkSet,
                  "frequencyDescription");

                field2.Error = true;

                var field3 =
                  GetField(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule, "startDt");

                field3.Error = true;

                var field4 =
                  GetField(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule, "amount");

                field4.Error = true;

                var field5 =
                  GetField(export.PaymentSchedule.Item.
                    DetailObligationPaymentSchedule, "endDt");

                field5.Error = true;

                if (Equal(export.PaymentSchedule.Item.
                  DetailObligationPaymentSchedule.EndDt, local.Maximum.Date))
                {
                  export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                    EndDt = local.InitializedDate.Date;
                }

                return;
              }

              break;
            default:
              var field =
                GetField(export.PaymentSchedule.Item.DetailAction, "actionEntry");
                

              field.Error = true;

              ExitState = "INVALID_ACTION_C_OR_D";

              return;
          }
        }

        // *** Start of ADD logic. Note added by G Sharp. ***
        // *****
        // The following edit checks data entry in the add obligation payment 
        // schedule fields.  The input fields include effective & discontinue
        // dates, and amount.  If a user populates any of these fields, the
        // logic defaults to add processing edits.  However, if the user does
        // not enter anything in the fields, payment schedule addition logic is
        // avoided.
        // *****
        if (IsEmpty(export.AddLineFrequencyWorkSet.FrequencyCode))
        {
        }
        else
        {
          // *****
          // Don't allow add, update, or delete if the Payment Schedule is the 
          // result of a legal action
          // *****
          if (!import.PaymentSchedule.IsEmpty)
          {
            for(export.PaymentSchedule.Index = 0; export
              .PaymentSchedule.Index < export.PaymentSchedule.Count; ++
              export.PaymentSchedule.Index)
            {
              if (ReadLegalActionDetail2())
              {
                if (!IsEmpty(entities.LegalActionDetail.FreqPeriodCode))
                {
                  var field1 =
                    GetField(export.AddLineObligationPaymentSchedule, "amount");
                    

                  field1.Error = true;

                  var field2 =
                    GetField(export.AddLineFrequencyWorkSet, "frequencyCode");

                  field2.Error = true;

                  var field3 =
                    GetField(export.AddLineFrequencyWorkSet,
                    "frequencyDescription");

                  field3.Error = true;

                  var field4 =
                    GetField(export.AddLineObligationPaymentSchedule,
                    "dayOfMonth1");

                  field4.Error = true;

                  var field5 =
                    GetField(export.AddLineObligationPaymentSchedule,
                    "dayOfMonth2");

                  field5.Error = true;

                  var field6 =
                    GetField(export.AddLineObligationPaymentSchedule,
                    "dayOfWeek");

                  field6.Error = true;

                  var field7 =
                    GetField(export.AddLineObligationPaymentSchedule, "startDt");
                    

                  field7.Error = true;

                  var field8 =
                    GetField(export.AddLineObligationPaymentSchedule, "endDt");

                  field8.Error = true;

                  ExitState = "FN0000_OPS_EST_BY_LDET";

                  return;
                }
              }
            }
          }

          // *****
          // PAYMENT SCHEDULE ADDITION REQUESTED.  DO ADD PROCESSING.
          // *****
          ++local.ActionCounter.Count;

          // *** Changes the order of the edits.
          //       1. edit Start Date, because it is used in the edit of 
          // Frequency.
          //       2. edit Frequency. 3. Periodic Amount Due. 4. Discount Date.
          // Change by G Sharp ***
          // ................................
          // Start Date edit checks
          // ...............................
          // ***---  changed test from LESS THAN OR EQUAL --- 7/26/99 badams
          if (Lt(export.AddLineObligationPaymentSchedule.StartDt,
            local.Current.Date))
          {
            if (Equal(export.LegalActionDetail.EffectiveDate,
              export.AddLineObligationPaymentSchedule.StartDt))
            {
              // : This condition is valid if the same Effective Date is found 
              // in legal.
            }
            else
            {
              var field =
                GetField(export.AddLineObligationPaymentSchedule, "startDt");

              field.Error = true;

              ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

              return;
            }
          }

          // %%%
          if (Lt(export.AddLineObligationPaymentSchedule.StartDt,
            export.DebtDetail.DueDt))
          {
            var field =
              GetField(export.AddLineObligationPaymentSchedule, "startDt");

            field.Error = true;

            ExitState = "EFFECTIVE_DT_LESS_THAN_DUE_DT";

            return;
          }

          // ........................
          // Frequency Code edit checks
          // ..........................
          export.AddLineObligationPaymentSchedule.FrequencyCode =
            export.AddLineFrequencyWorkSet.FrequencyCode;
          local.CodeCodeValue.Cdvalue =
            export.AddLineFrequencyWorkSet.FrequencyCode;
          local.CodeCode.CodeName = "FREQUENCY";
          UseCabGetCodeValueDescription();

          switch(local.CodeReturnCode.Count)
          {
            case 0:
              if (Lt(local.Current.Date, local.CodeCodeValue.ExpirationDate))
              {
                export.AddLineFrequencyWorkSet.FrequencyDescription =
                  local.CodeCodeValue.Description;
              }
              else
              {
                var field =
                  GetField(export.AddLineFrequencyWorkSet, "frequencyCode");

                field.Error = true;

                ExitState = "FN0000_CODE_VALUE_EXPIRED";

                return;
              }

              break;
            case 4:
              ExitState = "FN0000_FREQUENCY_CODE_UNKNOWN";

              var field1 =
                GetField(export.AddLineFrequencyWorkSet, "frequencyCode");

              field1.Error = true;

              return;
            default:
              ExitState = "FN0000_FREQUENCY_CODE_UNKNOWN";

              var field2 =
                GetField(export.AddLineFrequencyWorkSet, "frequencyCode");

              field2.Error = true;

              return;
          }

          // ****
          // G Sharp - 10/27/98   -  Day of the month 2 is required with for 
          // frequency code of semi-monthly.
          //                 DOM1 DOM2 DOW
          // Monthly          X
          // Semi-monthly     X    X
          // Bi-weekly        X
          // Weekly                     X
          // Bi-weekly                  X
          // ****
          if (Equal(export.AddLineFrequencyWorkSet.FrequencyCode,
            local.HardcodeOpsCSemiMonthly.FrequencyCode))
          {
            if (export.AddLineObligationPaymentSchedule.DayOfMonth2.
              GetValueOrDefault() == 0)
            {
              ExitState = "FN0000_DAY_OF_MONTH_REQUIRED";

              var field =
                GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth2");
                

              field.Error = true;

              return;
            }

            if (export.AddLineObligationPaymentSchedule.DayOfMonth1.
              GetValueOrDefault() < export
              .AddLineObligationPaymentSchedule.DayOfMonth2.
                GetValueOrDefault())
            {
              // ****  OK  ****
            }
            else
            {
              ExitState = "FN0000_DOM1_SHOULD_BE_LESS_2";

              var field1 =
                GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth1");
                

              field1.Error = true;

              var field2 =
                GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth2");
                

              field2.Error = true;

              return;
            }
          }

          if (Equal(export.AddLineFrequencyWorkSet.FrequencyCode,
            local.HardcodeOpsCWeekly.FrequencyCode) || Equal
            (export.AddLineFrequencyWorkSet.FrequencyCode,
            local.HardcodeOpsCBiWeekly.FrequencyCode))
          {
            if (export.AddLineObligationPaymentSchedule.DayOfMonth1.
              GetValueOrDefault() == 0)
            {
              // ****  OK  ****
            }
            else
            {
              ExitState = "FN0000_INVALID_DAY_O_MO_FOR_FREQ";

              var field =
                GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth1");
                

              field.Error = true;

              return;
            }
          }

          if (Equal(export.AddLineFrequencyWorkSet.FrequencyCode,
            local.HardcodeOpsCMonthly.FrequencyCode) || Equal
            (export.AddLineFrequencyWorkSet.FrequencyCode,
            local.HardcodeOpsCSemiMonthly.FrequencyCode) || Equal
            (export.AddLineFrequencyWorkSet.FrequencyCode,
            local.HardcodeOpsCBiMonthly.FrequencyCode))
          {
            if (export.AddLineObligationPaymentSchedule.DayOfWeek.
              GetValueOrDefault() == 0)
            {
              // ****  OK  ****
            }
            else
            {
              ExitState = "FN0000_INVALID_DAY_O_WK_FOR_FREQ";

              var field =
                GetField(export.AddLineObligationPaymentSchedule, "dayOfWeek");

              field.Error = true;

              return;
            }
          }

          // ****
          // G Sharp - 10/27/98   - More editing for Frequency code: Prepare to 
          // import data into fn_validate_frequency_info.
          // ****
          local.ObligationPaymentSchedule.Assign(
            export.AddLineObligationPaymentSchedule);
          local.ObligationPaymentSchedule.FrequencyCode =
            export.AddLineFrequencyWorkSet.FrequencyCode;
          local.Day1.Count =
            export.AddLineObligationPaymentSchedule.DayOfMonth1.
              GetValueOrDefault();
          local.Day2.Count =
            export.AddLineObligationPaymentSchedule.DayOfMonth2.
              GetValueOrDefault();

          // ****
          // G Sharp - 10/27/98   - fn_validate_frequency_info does NOT have 
          // import for DOW, so it uses import DOM1.
          // ****
          if (Equal(export.AddLineFrequencyWorkSet.FrequencyCode,
            local.HardcodeOpsCWeekly.FrequencyCode) || Equal
            (export.AddLineFrequencyWorkSet.FrequencyCode,
            local.HardcodeOpsCBiWeekly.FrequencyCode))
          {
            local.Day1.Count =
              export.AddLineObligationPaymentSchedule.DayOfWeek.
                GetValueOrDefault();
          }

          UseFnValidateFrequencyInfo();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.AddLineObligationPaymentSchedule.PeriodInd =
              local.ObligationPaymentSchedule.PeriodInd ?? "";

            if (export.AddLineObligationPaymentSchedule.DayOfMonth1.
              GetValueOrDefault() == 0)
            {
              if (Equal(export.AddLineFrequencyWorkSet.FrequencyCode,
                local.HardcodeOpsCMonthly.FrequencyCode) || Equal
                (export.AddLineFrequencyWorkSet.FrequencyCode,
                local.HardcodeOpsCSemiMonthly.FrequencyCode) || Equal
                (export.AddLineFrequencyWorkSet.FrequencyCode,
                local.HardcodeOpsCBiMonthly.FrequencyCode))
              {
                export.AddLineObligationPaymentSchedule.DayOfMonth1 =
                  local.ObligationPaymentSchedule.DayOfMonth1.
                    GetValueOrDefault();
              }
            }

            if (export.AddLineObligationPaymentSchedule.DayOfWeek.
              GetValueOrDefault() == 0)
            {
              if (Equal(export.AddLineFrequencyWorkSet.FrequencyCode,
                local.HardcodeOpsCWeekly.FrequencyCode) || Equal
                (export.AddLineFrequencyWorkSet.FrequencyCode,
                local.HardcodeOpsCBiWeekly.FrequencyCode))
              {
                export.AddLineObligationPaymentSchedule.DayOfWeek =
                  local.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault();
                  
              }
            }
          }
          else if (IsExitState("INVALID_DAY_OF_WEEK"))
          {
            var field =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfWeek");

            field.Error = true;

            return;
          }
          else if (IsExitState("INVALID_DAY_2_MUST_BE_ZERO"))
          {
            var field1 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth1");

            field1.Error = true;

            var field2 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth2");

            field2.Error = true;

            return;
          }
          else if (IsExitState("ACO_NE0000_REQUIRED_DATA_MISSING"))
          {
            var field =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth2");

            field.Error = true;

            return;
          }
          else if (IsExitState("INVALID_DAY_OF_MONTH"))
          {
            if (export.AddLineObligationPaymentSchedule.DayOfMonth1.
              GetValueOrDefault() > 31)
            {
              var field =
                GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth1");
                

              field.Error = true;
            }

            if (export.AddLineObligationPaymentSchedule.DayOfMonth2.
              GetValueOrDefault() > 31)
            {
              var field =
                GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth2");
                

              field.Error = true;
            }

            return;
          }
          else if (IsExitState("DAY1_AND_DAY2_CANNOT_BE_THE_SAME"))
          {
            var field1 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth1");

            field1.Error = true;

            var field2 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth2");

            field2.Error = true;

            return;
          }
          else if (IsExitState("FN0000_INVALID_FREQ_CODE"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            var field1 =
              GetField(export.AddLineFrequencyWorkSet, "frequencyCode");

            field1.Error = true;

            var field2 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth1");

            field2.Error = true;

            var field3 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth2");

            field3.Error = true;

            var field4 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfWeek");

            field4.Error = true;

            return;
          }

          // ..........................
          // Amount edit checks
          // ........................
          if (export.AddLineObligationPaymentSchedule.Amount.
            GetValueOrDefault() > export.ScreenOwedAmounts.TotalAmountOwed)
          {
            var field1 = GetField(export.ScreenOwedAmounts, "totalAmountOwed");

            field1.Error = true;

            var field2 =
              GetField(export.AddLineObligationPaymentSchedule, "amount");

            field2.Error = true;

            ExitState = "FN0000_PRD_AMT_GRTR_THN_AMT_OWED";

            return;
          }

          // ...................................
          // End Date edit Checks
          // ...................................
          // *** Change edit rules for End Date, End Date must be greater than 
          // Start Date. Also changed error message. Changed by G Sharp ***
          if (Equal(export.AddLineObligationPaymentSchedule.EndDt,
            local.InitializedDate.Date))
          {
            export.AddLineObligationPaymentSchedule.EndDt = local.Maximum.Date;
          }

          if (Lt(export.AddLineObligationPaymentSchedule.StartDt,
            export.AddLineObligationPaymentSchedule.EndDt))
          {
            // *** OK ***
          }
          else
          {
            var field =
              GetField(export.AddLineObligationPaymentSchedule, "endDt");

            field.Error = true;

            ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

            return;
          }

          // *****
          // Add Obligation Payment Schedule.
          // *****
          UseFnEstObligationPymntSch();

          if (IsExitState("FN0000_OBLIG_PYMNT_SCH_ACTIVE"))
          {
            var field1 =
              GetField(export.AddLineFrequencyWorkSet, "frequencyCode");

            field1.Error = true;

            var field2 =
              GetField(export.AddLineFrequencyWorkSet, "frequencyDescription");

            field2.Error = true;

            var field3 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth1");

            field3.Error = true;

            var field4 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth2");

            field4.Error = true;

            var field5 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfWeek");

            field5.Error = true;

            var field6 =
              GetField(export.AddLineObligationPaymentSchedule, "startDt");

            field6.Error = true;

            var field7 =
              GetField(export.AddLineObligationPaymentSchedule, "amount");

            field7.Error = true;

            var field8 =
              GetField(export.AddLineObligationPaymentSchedule, "endDt");

            field8.Error = true;
          }
          else if (IsExitState("FN0000_OBLIG_PYMNT_SCH_AE"))
          {
            var field1 =
              GetField(export.AddLineFrequencyWorkSet, "frequencyCode");

            field1.Error = true;

            var field2 =
              GetField(export.AddLineFrequencyWorkSet, "frequencyDescription");

            field2.Error = true;

            var field3 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth1");

            field3.Error = true;

            var field4 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfMonth2");

            field4.Error = true;

            var field5 =
              GetField(export.AddLineObligationPaymentSchedule, "dayOfWeek");

            field5.Error = true;

            var field6 =
              GetField(export.AddLineObligationPaymentSchedule, "startDt");

            field6.Error = true;

            var field7 =
              GetField(export.AddLineObligationPaymentSchedule, "amount");

            field7.Error = true;

            var field8 =
              GetField(export.AddLineObligationPaymentSchedule, "endDt");

            field8.Error = true;
          }
          else if (IsExitState("FN0211_DEBT_DETAIL_NF"))
          {
          }
          else if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // *****
            // Okay, continue processing.
            // *****
            // : Aug 5, 1999, mfb - If this is a recapture, set a flag to 
            // generate
            //   the RCAPRPAY document at the end of the prad.
            //  Also save the action so that the exitstate can be reset.
            if (AsChar(export.ObligationType.Classification) == AsChar
              (local.HardcodeRecovery.Classification))
            {
              local.Rcaprpay.Flag = "Y";
              local.SaveAction.Command = "ADD";
            }

            export.AddLineFrequencyWorkSet.FrequencyCode =
              local.InitializedAddLineFrequencyWorkSet.FrequencyCode;
            export.AddLineFrequencyWorkSet.FrequencyDescription =
              local.InitializedAddLineFrequencyWorkSet.FrequencyDescription;
            export.AddLineFreqPeriod.Text13 =
              local.InitializedAddLineWorkArea.Text13;
            export.AddLineObligationPaymentSchedule.DayOfMonth1 =
              local.InitializedAddLineObligationPaymentSchedule.DayOfMonth1.
                GetValueOrDefault();
            export.AddLineObligationPaymentSchedule.DayOfMonth2 =
              local.InitializedAddLineObligationPaymentSchedule.DayOfMonth2.
                GetValueOrDefault();
            export.AddLineObligationPaymentSchedule.DayOfWeek =
              local.InitializedAddLineObligationPaymentSchedule.DayOfWeek.
                GetValueOrDefault();
            export.AddLineObligationPaymentSchedule.StartDt =
              local.InitializedAddLineObligationPaymentSchedule.StartDt;
            export.AddLineObligationPaymentSchedule.Amount =
              local.InitializedAddLineObligationPaymentSchedule.Amount.
                GetValueOrDefault();
            export.AddLineObligationPaymentSchedule.EndDt =
              local.InitializedAddLineObligationPaymentSchedule.EndDt;

            export.PaymentSchedule.Index = 0;
            export.PaymentSchedule.Clear();

            foreach(var item in ReadObligationPaymentSchedule3())
            {
              MoveObligationPaymentSchedule2(entities.ObligationPaymentSchedule,
                export.PaymentSchedule.Update.DetailObligationPaymentSchedule);
              MoveObligationPaymentSchedule5(entities.ObligationPaymentSchedule,
                export.PaymentSchedule.Update.DtlPrevHidden);
              export.PaymentSchedule.Update.DetailAction.ActionEntry = "";
              export.PaymentSchedule.Update.DetailCreateDate.Date =
                Date(entities.ObligationPaymentSchedule.CreatedTmst);

              if (Equal(entities.ObligationPaymentSchedule.EndDt,
                local.Maximum.Date))
              {
                export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                  EndDt = local.InitializedDate.Date;
              }

              // *** G Sharp 10-27-98. Added to screen Worked ID. This could be 
              // last updated by, or if no last update by then created by. ***
              if (IsEmpty(entities.ObligationPaymentSchedule.LastUpdateBy))
              {
                export.PaymentSchedule.Update.DetailObligationPaymentSchedule.
                  LastUpdateBy = entities.ObligationPaymentSchedule.CreatedBy;
              }

              local.CodeCode.CodeName = "FREQUENCY";
              local.CodeCodeValue.Cdvalue =
                entities.ObligationPaymentSchedule.FrequencyCode;
              export.PaymentSchedule.Update.DetailFrequencyWorkSet.
                FrequencyCode =
                  entities.ObligationPaymentSchedule.FrequencyCode;
              UseCabGetCodeValueDescription();

              switch(local.CodeReturnCode.Count)
              {
                case 0:
                  if (Lt(local.Current.Date, local.CodeCodeValue.ExpirationDate))
                    
                  {
                    export.PaymentSchedule.Update.DetailFrequencyWorkSet.
                      FrequencyDescription = local.CodeCodeValue.Description;
                  }
                  else
                  {
                    var field =
                      GetField(export.PaymentSchedule.Item.
                        DetailFrequencyWorkSet, "frequencyCode");

                    field.Error = true;

                    ExitState = "FN0000_CODE_VALUE_EXPIRED";
                  }

                  break;
                case 4:
                  ExitState = "FN0000_FREQUENCY_CODE_UNKNOWN";

                  var field1 =
                    GetField(export.PaymentSchedule.Item.DetailFrequencyWorkSet,
                    "frequencyCode");

                  field1.Error = true;

                  break;
                default:
                  var field2 =
                    GetField(export.PaymentSchedule.Item.DetailFrequencyWorkSet,
                    "frequencyCode");

                  field2.Error = true;

                  ExitState = "FN0000_FREQUENCY_CODE_UNKNOWN";

                  break;
              }

              export.PaymentSchedule.Next();
            }

            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
          else if (IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
          {
          }
          else if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
          {
          }
          else if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
          {
          }
          else
          {
            return;
          }
        }

        // ....................................
        // No ADD or CHANGE or DELETE was requested
        // ........................................
        if (local.ActionCounter.Count == 0)
        {
          ExitState = "FN0000_ACTION_NOT_INDICATED";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // : Aug 5, 1999, mfb - If the RCAPRPAY flag has been set, generate
    //   the RCAPRPAY document.
    if (AsChar(local.Rcaprpay.Flag) == 'Y')
    {
      if (!ReadObligorRule())
      {
        ExitState = "FN0000_OBLIGOR_RULE_NF";

        return;
      }

      // *************** Set RCAPRPAY document trigger ******************
      // mjr
      // ------------------------------------------------
      // 02/18/1999
      // Use this action block (find_outgoing_doc) if you need to check
      // for another document for some reason.
      // ------------------------------------------------------------
      local.Document.Name = "RCAPRPAY";
      local.Infrastructure.ReferenceDate = local.Current.Date;
      local.SpDocKey.KeyRecaptureRule =
        entities.ObligorRule.SystemGeneratedIdentifier;
      UseSpDocFindOutgoingDocument();

      if (local.Infrastructure.SystemGeneratedIdentifier > 0)
      {
        // mjr---> A document already exists.  Check outgoing_doc 
        // print_succesfful_ind to determine action.
        switch(AsChar(local.OutgoingDocument.PrintSucessfulIndicator))
        {
          case 'Y':
            // mjr---> Printed successfully
            // PR 294742 05/22/07 MWF Disabled following SET statement.
            break;
          case 'N':
            // mjr---> NOT Printed successfully
            break;
          case 'G':
            // mjr---> Awaiting generation
            break;
          case 'B':
            // mjr---> Awaiting Natural batch print
            UseSpDocCancelOutgoingDoc();

            // PR 294742 05/22/07 MWF Disabled following SET statement.
            break;
          case 'C':
            // mjr---> Printing canceled
            break;
          default:
            break;
        }

        // PR 294742 05/22/07 MWF Added following SET statement.
        local.Infrastructure.SystemGeneratedIdentifier = 0;
      }

      // mjr
      // ------------------------------------------------
      // 02/18/1999
      // Use this action block (create_document_infrastruct) to create a 
      // document trigger
      // ------------------------------------------------------------
      UseSpCreateDocumentInfrastruct();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        switch(TrimEnd(local.SaveAction.Command))
        {
          case "ADD":
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

            break;
          case "DELETE":
            ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

            break;
          case "UPDATE":
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

            break;
          default:
            break;
        }
      }
      else if (IsExitState("SP0000_EXPIRED_DOCUMENT_INACTIVE"))
      {
        // : This is a rollback exitstate - we don't want to rollback,
        //   so reset the exitstate.
        ExitState = "FN0000_RCAPRPAY_DOC_GEN_ERR";
      }
      else
      {
      }

      // *************** End RCAPRPAY document trigger ******************
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveFrequencyWorkSet(FrequencyWorkSet source,
    FrequencyWorkSet target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.FrequencyDescription = source.FrequencyDescription;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalActionDetail(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.Number = source.Number;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.AsOfDtNadArrBal = source.AsOfDtNadArrBal;
    target.AsOfDtNadIntBal = source.AsOfDtNadIntBal;
    target.AsOfDtAdcArrBal = source.AsOfDtAdcArrBal;
    target.AsOfDtAdcIntBal = source.AsOfDtAdcIntBal;
    target.AsOfDtRecBal = source.AsOfDtRecBal;
    target.AsOdDtRecIntBal = source.AsOdDtRecIntBal;
    target.AsOfDtFeeBal = source.AsOfDtFeeBal;
    target.AsOfDtFeeIntBal = source.AsOfDtFeeIntBal;
    target.AsOfDtTotBalCurrArr = source.AsOfDtTotBalCurrArr;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligationPaymentSchedule1(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.PeriodInd = source.PeriodInd;
  }

  private static void MoveObligationPaymentSchedule2(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.Amount = source.Amount;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdateBy = source.LastUpdateBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
  }

  private static void MoveObligationPaymentSchedule3(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.StartDt = source.StartDt;
  }

  private static void MoveObligationPaymentSchedule4(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.PeriodInd = source.PeriodInd;
    target.Amount = source.Amount;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdateBy = source.LastUpdateBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
  }

  private static void MoveObligationPaymentSchedule5(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.Amount = source.Amount;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeCodeValue.Cdvalue;
    MoveCode(local.CodeCode, useImport.Code);

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.CodeCodeValue.Assign(useExport.CodeValue);
    local.CodeReturnCode.Count = useExport.ReturnCode.Count;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCabSetAccrualOrDueAmount()
  {
    var useImport = new FnCabSetAccrualOrDueAmount.Import();
    var useExport = new FnCabSetAccrualOrDueAmount.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveObligationType(export.ObligationType, useImport.ObligationType);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;

    Call(FnCabSetAccrualOrDueAmount.Execute, useImport, useExport);

    export.TotalAmountDue.TotalCurrency = useExport.Common.TotalCurrency;
    export.AccrualOrDue.Date = useExport.StartDte.Date;
  }

  private void UseFnComputeSummaryTotals()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.OmitCrdInd.Flag = local.IncludeCrdInd.Flag;
    useImport.Obligor.Number = export.CsePerson.Number;
    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.Filter.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnDeleteObligationPymntSch1()
  {
    var useImport = new FnDeleteObligationPymntSch.Import();
    var useExport = new FnDeleteObligationPymntSch.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationPaymentSchedule.StartDt =
      export.PaymentSchedule.Item.DetailObligationPaymentSchedule.StartDt;

    Call(FnDeleteObligationPymntSch.Execute, useImport, useExport);
  }

  private void UseFnDeleteObligationPymntSch2()
  {
    var useImport = new FnDeleteObligationPymntSch.Import();
    var useExport = new FnDeleteObligationPymntSch.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      export.HiddenConcurrentObligationType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.HiddenConcurrentCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.HiddenConcurrentObligation.SystemGeneratedIdentifier;
    useImport.ObligationPaymentSchedule.StartDt =
      export.PaymentSchedule.Item.DetailObligationPaymentSchedule.StartDt;

    Call(FnDeleteObligationPymntSch.Execute, useImport, useExport);
  }

  private void UseFnDiscObligationPymntSch()
  {
    var useImport = new FnDiscObligationPymntSch.Import();
    var useExport = new FnDiscObligationPymntSch.Export();

    useImport.PymntSchedule.Date = local.PaymentSchDiscontinue.Date;
    useImport.ObligationPaymentSchedule.Assign(
      entities.ObligationPaymentSchedule);

    Call(FnDiscObligationPymntSch.Execute, useImport, useExport);

    MoveObligationPaymentSchedule2(useImport.ObligationPaymentSchedule,
      entities.ObligationPaymentSchedule);
  }

  private void UseFnEstObligationPymntSch()
  {
    var useImport = new FnEstObligationPymntSch.Import();
    var useExport = new FnEstObligationPymntSch.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.CsePersonAccount.Type1 = export.CsePersonAccount.Type1;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ConcurrentObligationType.SystemGeneratedIdentifier =
      export.HiddenConcurrentObligationType.SystemGeneratedIdentifier;
    useImport.ConcurrentCsePerson.Number =
      export.HiddenConcurrentCsePerson.Number;
    useImport.ConcurrentObligation.SystemGeneratedIdentifier =
      export.HiddenConcurrentObligation.SystemGeneratedIdentifier;
    useImport.ObligationPaymentSchedule.Assign(
      export.AddLineObligationPaymentSchedule);

    Call(FnEstObligationPymntSch.Execute, useImport, useExport);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeOpsCMonthly.FrequencyCode =
      useExport.OpsCMonthly.FrequencyCode;
    local.HardcodeOpsCBiWeekly.FrequencyCode =
      useExport.OpsCBiWeekly.FrequencyCode;
    local.HardcodeOpsCWeekly.FrequencyCode = useExport.OpsCWeekly.FrequencyCode;
    local.HardcodeOpsCSemiMonthly.FrequencyCode =
      useExport.OpsCSemiMonthly.FrequencyCode;
    local.HardcodeOpsCBiMonthly.FrequencyCode =
      useExport.OpsCBiMonthly.FrequencyCode;
    local.HardcodedAccruing.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.HardcodedVoluntary.Classification =
      useExport.OtCVoluntaryClassification.Classification;
    local.HardcodedDebt.Type1 = useExport.OtrnDtDebtDetail.Type1;
    local.HardcodedObligor.Type1 = useExport.CpaObligor.Type1;
    local.HardcodeRecovery.Classification =
      useExport.OtCRecoverClassification.Classification;
  }

  private void UseFnUpdateObligationPymntSch1()
  {
    var useImport = new FnUpdateObligationPymntSch.Import();
    var useExport = new FnUpdateObligationPymntSch.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationPaymentSchedule.Assign(
      export.PaymentSchedule.Item.DetailObligationPaymentSchedule);

    Call(FnUpdateObligationPymntSch.Execute, useImport, useExport);
  }

  private void UseFnUpdateObligationPymntSch2()
  {
    var useImport = new FnUpdateObligationPymntSch.Import();
    var useExport = new FnUpdateObligationPymntSch.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      export.HiddenConcurrentObligationType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.HiddenConcurrentCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.HiddenConcurrentObligation.SystemGeneratedIdentifier;
    useImport.ObligationPaymentSchedule.Assign(
      export.PaymentSchedule.Item.DetailObligationPaymentSchedule);

    Call(FnUpdateObligationPymntSch.Execute, useImport, useExport);
  }

  private void UseFnValidateFrequencyInfo()
  {
    var useImport = new FnValidateFrequencyInfo.Import();
    var useExport = new FnValidateFrequencyInfo.Export();

    useImport.Day2.Count = local.Day2.Count;
    useImport.Day1.Count = local.Day1.Count;
    MoveObligationPaymentSchedule3(local.ObligationPaymentSchedule,
      useImport.ObligationPaymentSchedule);

    Call(FnValidateFrequencyInfo.Execute, useImport, useExport);

    local.PaymentFreq.Text13 = useExport.WorkArea.Text13;
    local.ErrorDay2.Count = useExport.ErrorDay2.Count;
    local.ErrorDay1.Count = useExport.ErrorDay1.Count;
    MoveObligationPaymentSchedule1(useExport.ObligationPaymentSchedule,
      local.ObligationPaymentSchedule);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
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

    useImport.CsePerson.Number = import.CsePerson.Number;

    MoveLegalAction(import.LegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Name = local.Document.Name;
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.SpDocKey.KeyRecaptureRule = local.SpDocKey.KeyRecaptureRule;

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
  }

  private void UseSpDocCancelOutgoingDoc()
  {
    var useImport = new SpDocCancelOutgoingDoc.Import();
    var useExport = new SpDocCancelOutgoingDoc.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;

    Call(SpDocCancelOutgoingDoc.Execute, useImport, useExport);
  }

  private void UseSpDocFindOutgoingDocument()
  {
    var useImport = new SpDocFindOutgoingDocument.Import();
    var useExport = new SpDocFindOutgoingDocument.Export();

    useImport.Document.Name = local.Document.Name;
    useImport.SpDocKey.KeyRecaptureRule = local.SpDocKey.KeyRecaptureRule;

    Call(SpDocFindOutgoingDocument.Execute, useImport, useExport);

    local.Infrastructure.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
    local.OutgoingDocument.PrintSucessfulIndicator =
      useExport.OutgoingDocument.PrintSucessfulIndicator;
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadLegalActionDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 3);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 4);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionDetail2()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionDetail.JudgementAmount =
          db.GetNullableDecimal(reader, 3);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 4);
        entities.LegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadObligation()
  {
    entities.Concurrent.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId",
          import.HiddenConcurrentObligation.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", local.HardcodedObligor.Type1);
        db.SetString(
          command, "cspNumber", export.HiddenConcurrentCsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.HiddenConcurrentObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Concurrent.CpaType = db.GetString(reader, 0);
        entities.Concurrent.CspNumber = db.GetString(reader, 1);
        entities.Concurrent.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Concurrent.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Concurrent.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Concurrent.CpaType);
      });
  }

  private bool ReadObligationObligationType()
  {
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadObligationObligationType",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", local.HardcodedObligor.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "debtTypId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.AsOfDtNadArrBal = db.GetNullableDecimal(reader, 4);
        entities.Obligation.AsOfDtNadIntBal = db.GetNullableDecimal(reader, 5);
        entities.Obligation.AsOfDtAdcArrBal = db.GetNullableDecimal(reader, 6);
        entities.Obligation.AsOfDtAdcIntBal = db.GetNullableDecimal(reader, 7);
        entities.Obligation.AsOfDtRecBal = db.GetNullableDecimal(reader, 8);
        entities.Obligation.AsOdDtRecIntBal = db.GetNullableDecimal(reader, 9);
        entities.Obligation.AsOfDtFeeBal = db.GetNullableDecimal(reader, 10);
        entities.Obligation.AsOfDtFeeIntBal = db.GetNullableDecimal(reader, 11);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 12);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 13);
        entities.ObligationType.Code = db.GetString(reader, 14);
        entities.ObligationType.Classification = db.GetString(reader, 15);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadObligationPaymentSchedule1()
  {
    entities.Overlap.Populated = false;

    return Read("ReadObligationPaymentSchedule1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          export.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", export.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "obgCpaType", local.HardcodedObligor.Type1);
        db.SetString(command, "obgCspNumber", export.CsePerson.Number);
        db.SetDate(
          command, "startDt1",
          export.PaymentSchedule.Item.DtlPrevHidden.EndDt.GetValueOrDefault());
        db.SetDate(
          command, "startDt2",
          export.PaymentSchedule.Item.DetailObligationPaymentSchedule.EndDt.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Overlap.OtyType = db.GetInt32(reader, 0);
        entities.Overlap.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.Overlap.ObgCspNumber = db.GetString(reader, 2);
        entities.Overlap.ObgCpaType = db.GetString(reader, 3);
        entities.Overlap.StartDt = db.GetDate(reader, 4);
        entities.Overlap.Amount = db.GetNullableDecimal(reader, 5);
        entities.Overlap.EndDt = db.GetNullableDate(reader, 6);
        entities.Overlap.CreatedBy = db.GetString(reader, 7);
        entities.Overlap.CreatedTmst = db.GetDateTime(reader, 8);
        entities.Overlap.LastUpdateBy = db.GetNullableString(reader, 9);
        entities.Overlap.LastUpdateTmst = db.GetNullableDateTime(reader, 10);
        entities.Overlap.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.Overlap.ObgCpaType);
      });
  }

  private bool ReadObligationPaymentSchedule2()
  {
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule2",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          export.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", export.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "obgCpaType", local.HardcodedObligor.Type1);
        db.SetString(command, "obgCspNumber", export.CsePerson.Number);
        db.SetDate(
          command, "startDt",
          export.PaymentSchedule.Item.DtlPrevHidden.StartDt.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.CreatedBy = db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.CreatedTmst =
          db.GetDateTime(reader, 8);
        entities.ObligationPaymentSchedule.LastUpdateBy =
          db.GetNullableString(reader, 9);
        entities.ObligationPaymentSchedule.LastUpdateTmst =
          db.GetNullableDateTime(reader, 10);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 11);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 12);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 13);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 14);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
      });
  }

  private IEnumerable<bool> ReadObligationPaymentSchedule3()
  {
    return ReadEach("ReadObligationPaymentSchedule3",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          export.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", export.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "obgCpaType", local.HardcodedObligor.Type1);
        db.SetString(command, "obgCspNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.PaymentSchedule.IsFull)
        {
          return false;
        }

        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.CreatedBy = db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.CreatedTmst =
          db.GetDateTime(reader, 8);
        entities.ObligationPaymentSchedule.LastUpdateBy =
          db.GetNullableString(reader, 9);
        entities.ObligationPaymentSchedule.LastUpdateTmst =
          db.GetNullableDateTime(reader, 10);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 11);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 12);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 13);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 14);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationPaymentSchedule4()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    return ReadEach("ReadObligationPaymentSchedule4",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        if (export.PaymentSchedule.IsFull)
        {
          return false;
        }

        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.CreatedBy = db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.CreatedTmst =
          db.GetDateTime(reader, 8);
        entities.ObligationPaymentSchedule.LastUpdateBy =
          db.GetNullableString(reader, 9);
        entities.ObligationPaymentSchedule.LastUpdateTmst =
          db.GetNullableDateTime(reader, 10);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 11);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 12);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 13);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 14);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);

        return true;
      });
  }

  private bool ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 7);
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 8);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private bool ReadObligorRule()
  {
    entities.ObligorRule.Populated = false;

    return Read("ReadObligorRule",
      (db, command) =>
      {
        db.SetNullableString(command, "cspDNumber", export.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligorRule.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ObligorRule.CpaDType = db.GetNullableString(reader, 1);
        entities.ObligorRule.CspDNumber = db.GetNullableString(reader, 2);
        entities.ObligorRule.EffectiveDate = db.GetDate(reader, 3);
        entities.ObligorRule.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.ObligorRule.Populated = true;
        CheckValid<RecaptureRule>("CpaDType", entities.ObligorRule.CpaDType);
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
    /// <summary>A PaymentScheduleGroup group.</summary>
    [Serializable]
    public class PaymentScheduleGroup
    {
      /// <summary>
      /// A value of DetailAction.
      /// </summary>
      [JsonPropertyName("detailAction")]
      public Common DetailAction
      {
        get => detailAction ??= new();
        set => detailAction = value;
      }

      /// <summary>
      /// A value of DetailCreateDate.
      /// </summary>
      [JsonPropertyName("detailCreateDate")]
      public DateWorkArea DetailCreateDate
      {
        get => detailCreateDate ??= new();
        set => detailCreateDate = value;
      }

      /// <summary>
      /// A value of DetailFrequencyWorkSet.
      /// </summary>
      [JsonPropertyName("detailFrequencyWorkSet")]
      public FrequencyWorkSet DetailFrequencyWorkSet
      {
        get => detailFrequencyWorkSet ??= new();
        set => detailFrequencyWorkSet = value;
      }

      /// <summary>
      /// A value of DetailObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("detailObligationPaymentSchedule")]
      public ObligationPaymentSchedule DetailObligationPaymentSchedule
      {
        get => detailObligationPaymentSchedule ??= new();
        set => detailObligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of DtlPrevHidden.
      /// </summary>
      [JsonPropertyName("dtlPrevHidden")]
      public ObligationPaymentSchedule DtlPrevHidden
      {
        get => dtlPrevHidden ??= new();
        set => dtlPrevHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailAction;
      private DateWorkArea detailCreateDate;
      private FrequencyWorkSet detailFrequencyWorkSet;
      private ObligationPaymentSchedule detailObligationPaymentSchedule;
      private ObligationPaymentSchedule dtlPrevHidden;
    }

    /// <summary>
    /// A value of AddLineFrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("addLineFrequencyWorkSet")]
    public FrequencyWorkSet AddLineFrequencyWorkSet
    {
      get => addLineFrequencyWorkSet ??= new();
      set => addLineFrequencyWorkSet = value;
    }

    /// <summary>
    /// A value of AddLineObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("addLineObligationPaymentSchedule")]
    public ObligationPaymentSchedule AddLineObligationPaymentSchedule
    {
      get => addLineObligationPaymentSchedule ??= new();
      set => addLineObligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of SearchFrequency.
    /// </summary>
    [JsonPropertyName("searchFrequency")]
    public Common SearchFrequency
    {
      get => searchFrequency ??= new();
      set => searchFrequency = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of HiddenConcurrentObligation.
    /// </summary>
    [JsonPropertyName("hiddenConcurrentObligation")]
    public Obligation HiddenConcurrentObligation
    {
      get => hiddenConcurrentObligation ??= new();
      set => hiddenConcurrentObligation = value;
    }

    /// <summary>
    /// A value of HiddenConcurrentObligationType.
    /// </summary>
    [JsonPropertyName("hiddenConcurrentObligationType")]
    public ObligationType HiddenConcurrentObligationType
    {
      get => hiddenConcurrentObligationType ??= new();
      set => hiddenConcurrentObligationType = value;
    }

    /// <summary>
    /// A value of HiddenConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenConcurrentCsePerson")]
    public CsePerson HiddenConcurrentCsePerson
    {
      get => hiddenConcurrentCsePerson ??= new();
      set => hiddenConcurrentCsePerson = value;
    }

    /// <summary>
    /// Gets a value of PaymentSchedule.
    /// </summary>
    [JsonIgnore]
    public Array<PaymentScheduleGroup> PaymentSchedule =>
      paymentSchedule ??= new(PaymentScheduleGroup.Capacity);

    /// <summary>
    /// Gets a value of PaymentSchedule for json serialization.
    /// </summary>
    [JsonPropertyName("paymentSchedule")]
    [Computed]
    public IList<PaymentScheduleGroup> PaymentSchedule_Json
    {
      get => paymentSchedule;
      set => PaymentSchedule.Assign(value);
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

    /// <summary>
    /// A value of AddLineFreqPeriod.
    /// </summary>
    [JsonPropertyName("addLineFreqPeriod")]
    public WorkArea AddLineFreqPeriod
    {
      get => addLineFreqPeriod ??= new();
      set => addLineFreqPeriod = value;
    }

    /// <summary>
    /// A value of TotalAmountDue.
    /// </summary>
    [JsonPropertyName("totalAmountDue")]
    public Common TotalAmountDue
    {
      get => totalAmountDue ??= new();
      set => totalAmountDue = value;
    }

    /// <summary>
    /// A value of AccrualOrDue.
    /// </summary>
    [JsonPropertyName("accrualOrDue")]
    public DateWorkArea AccrualOrDue
    {
      get => accrualOrDue ??= new();
      set => accrualOrDue = value;
    }

    private FrequencyWorkSet addLineFrequencyWorkSet;
    private ObligationPaymentSchedule addLineObligationPaymentSchedule;
    private Common searchFrequency;
    private ScreenOwedAmounts screenOwedAmounts;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private DebtDetail debtDetail;
    private LegalActionDetail legalActionDetail;
    private Obligation hiddenConcurrentObligation;
    private ObligationType hiddenConcurrentObligationType;
    private CsePerson hiddenConcurrentCsePerson;
    private Array<PaymentScheduleGroup> paymentSchedule;
    private NextTranInfo hidden;
    private Standard standard;
    private WorkArea addLineFreqPeriod;
    private Common totalAmountDue;
    private DateWorkArea accrualOrDue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PaymentScheduleGroup group.</summary>
    [Serializable]
    public class PaymentScheduleGroup
    {
      /// <summary>
      /// A value of DetailAction.
      /// </summary>
      [JsonPropertyName("detailAction")]
      public Common DetailAction
      {
        get => detailAction ??= new();
        set => detailAction = value;
      }

      /// <summary>
      /// A value of DetailCreateDate.
      /// </summary>
      [JsonPropertyName("detailCreateDate")]
      public DateWorkArea DetailCreateDate
      {
        get => detailCreateDate ??= new();
        set => detailCreateDate = value;
      }

      /// <summary>
      /// A value of DetailFrequencyWorkSet.
      /// </summary>
      [JsonPropertyName("detailFrequencyWorkSet")]
      public FrequencyWorkSet DetailFrequencyWorkSet
      {
        get => detailFrequencyWorkSet ??= new();
        set => detailFrequencyWorkSet = value;
      }

      /// <summary>
      /// A value of DetailObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("detailObligationPaymentSchedule")]
      public ObligationPaymentSchedule DetailObligationPaymentSchedule
      {
        get => detailObligationPaymentSchedule ??= new();
        set => detailObligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of DtlPrevHidden.
      /// </summary>
      [JsonPropertyName("dtlPrevHidden")]
      public ObligationPaymentSchedule DtlPrevHidden
      {
        get => dtlPrevHidden ??= new();
        set => dtlPrevHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailAction;
      private DateWorkArea detailCreateDate;
      private FrequencyWorkSet detailFrequencyWorkSet;
      private ObligationPaymentSchedule detailObligationPaymentSchedule;
      private ObligationPaymentSchedule dtlPrevHidden;
    }

    /// <summary>
    /// A value of AddLineFrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("addLineFrequencyWorkSet")]
    public FrequencyWorkSet AddLineFrequencyWorkSet
    {
      get => addLineFrequencyWorkSet ??= new();
      set => addLineFrequencyWorkSet = value;
    }

    /// <summary>
    /// A value of AddLineObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("addLineObligationPaymentSchedule")]
    public ObligationPaymentSchedule AddLineObligationPaymentSchedule
    {
      get => addLineObligationPaymentSchedule ??= new();
      set => addLineObligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of SearchFrequency.
    /// </summary>
    [JsonPropertyName("searchFrequency")]
    public Common SearchFrequency
    {
      get => searchFrequency ??= new();
      set => searchFrequency = value;
    }

    /// <summary>
    /// A value of TotalOwed.
    /// </summary>
    [JsonPropertyName("totalOwed")]
    public Common TotalOwed
    {
      get => totalOwed ??= new();
      set => totalOwed = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of HiddenConcurrentObligation.
    /// </summary>
    [JsonPropertyName("hiddenConcurrentObligation")]
    public Obligation HiddenConcurrentObligation
    {
      get => hiddenConcurrentObligation ??= new();
      set => hiddenConcurrentObligation = value;
    }

    /// <summary>
    /// A value of HiddenConcurrentObligationType.
    /// </summary>
    [JsonPropertyName("hiddenConcurrentObligationType")]
    public ObligationType HiddenConcurrentObligationType
    {
      get => hiddenConcurrentObligationType ??= new();
      set => hiddenConcurrentObligationType = value;
    }

    /// <summary>
    /// A value of HiddenConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenConcurrentCsePerson")]
    public CsePerson HiddenConcurrentCsePerson
    {
      get => hiddenConcurrentCsePerson ??= new();
      set => hiddenConcurrentCsePerson = value;
    }

    /// <summary>
    /// Gets a value of PaymentSchedule.
    /// </summary>
    [JsonIgnore]
    public Array<PaymentScheduleGroup> PaymentSchedule =>
      paymentSchedule ??= new(PaymentScheduleGroup.Capacity);

    /// <summary>
    /// Gets a value of PaymentSchedule for json serialization.
    /// </summary>
    [JsonPropertyName("paymentSchedule")]
    [Computed]
    public IList<PaymentScheduleGroup> PaymentSchedule_Json
    {
      get => paymentSchedule;
      set => PaymentSchedule.Assign(value);
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

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of AddLineFreqPeriod.
    /// </summary>
    [JsonPropertyName("addLineFreqPeriod")]
    public WorkArea AddLineFreqPeriod
    {
      get => addLineFreqPeriod ??= new();
      set => addLineFreqPeriod = value;
    }

    /// <summary>
    /// A value of TotalAmountDue.
    /// </summary>
    [JsonPropertyName("totalAmountDue")]
    public Common TotalAmountDue
    {
      get => totalAmountDue ??= new();
      set => totalAmountDue = value;
    }

    /// <summary>
    /// A value of AccrualOrDue.
    /// </summary>
    [JsonPropertyName("accrualOrDue")]
    public DateWorkArea AccrualOrDue
    {
      get => accrualOrDue ??= new();
      set => accrualOrDue = value;
    }

    private FrequencyWorkSet addLineFrequencyWorkSet;
    private ObligationPaymentSchedule addLineObligationPaymentSchedule;
    private Common searchFrequency;
    private Common totalOwed;
    private ScreenOwedAmounts screenOwedAmounts;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private ObligationType obligationType;
    private LegalActionDetail legalActionDetail;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private DebtDetail debtDetail;
    private Obligation hiddenConcurrentObligation;
    private ObligationType hiddenConcurrentObligationType;
    private CsePerson hiddenConcurrentCsePerson;
    private Array<PaymentScheduleGroup> paymentSchedule;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePerson obligor;
    private WorkArea addLineFreqPeriod;
    private Common totalAmountDue;
    private DateWorkArea accrualOrDue;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of IncludeCrdInd.
    /// </summary>
    [JsonPropertyName("includeCrdInd")]
    public Common IncludeCrdInd
    {
      get => includeCrdInd ??= new();
      set => includeCrdInd = value;
    }

    /// <summary>
    /// A value of Day2.
    /// </summary>
    [JsonPropertyName("day2")]
    public Common Day2
    {
      get => day2 ??= new();
      set => day2 = value;
    }

    /// <summary>
    /// A value of Day1.
    /// </summary>
    [JsonPropertyName("day1")]
    public Common Day1
    {
      get => day1 ??= new();
      set => day1 = value;
    }

    /// <summary>
    /// A value of PaymentFreq.
    /// </summary>
    [JsonPropertyName("paymentFreq")]
    public WorkArea PaymentFreq
    {
      get => paymentFreq ??= new();
      set => paymentFreq = value;
    }

    /// <summary>
    /// A value of ErrorDay2.
    /// </summary>
    [JsonPropertyName("errorDay2")]
    public Common ErrorDay2
    {
      get => errorDay2 ??= new();
      set => errorDay2 = value;
    }

    /// <summary>
    /// A value of ErrorDay1.
    /// </summary>
    [JsonPropertyName("errorDay1")]
    public Common ErrorDay1
    {
      get => errorDay1 ??= new();
      set => errorDay1 = value;
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
    /// A value of UnsuccessfulOpCount.
    /// </summary>
    [JsonPropertyName("unsuccessfulOpCount")]
    public Common UnsuccessfulOpCount
    {
      get => unsuccessfulOpCount ??= new();
      set => unsuccessfulOpCount = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of InitializedAddLineFrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("initializedAddLineFrequencyWorkSet")]
    public FrequencyWorkSet InitializedAddLineFrequencyWorkSet
    {
      get => initializedAddLineFrequencyWorkSet ??= new();
      set => initializedAddLineFrequencyWorkSet = value;
    }

    /// <summary>
    /// A value of InitializedAddLineCommon.
    /// </summary>
    [JsonPropertyName("initializedAddLineCommon")]
    public Common InitializedAddLineCommon
    {
      get => initializedAddLineCommon ??= new();
      set => initializedAddLineCommon = value;
    }

    /// <summary>
    /// A value of InitializedAddLineWorkArea.
    /// </summary>
    [JsonPropertyName("initializedAddLineWorkArea")]
    public WorkArea InitializedAddLineWorkArea
    {
      get => initializedAddLineWorkArea ??= new();
      set => initializedAddLineWorkArea = value;
    }

    /// <summary>
    /// A value of InitializedAddLineObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("initializedAddLineObligationPaymentSchedule")]
    public ObligationPaymentSchedule InitializedAddLineObligationPaymentSchedule
    {
      get => initializedAddLineObligationPaymentSchedule ??= new();
      set => initializedAddLineObligationPaymentSchedule = value;
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
    /// A value of PaymentSchDiscontinue.
    /// </summary>
    [JsonPropertyName("paymentSchDiscontinue")]
    public DateWorkArea PaymentSchDiscontinue
    {
      get => paymentSchDiscontinue ??= new();
      set => paymentSchDiscontinue = value;
    }

    /// <summary>
    /// A value of HardcodedAccruing.
    /// </summary>
    [JsonPropertyName("hardcodedAccruing")]
    public ObligationType HardcodedAccruing
    {
      get => hardcodedAccruing ??= new();
      set => hardcodedAccruing = value;
    }

    /// <summary>
    /// A value of HardcodedHealthIns.
    /// </summary>
    [JsonPropertyName("hardcodedHealthIns")]
    public ObligationType HardcodedHealthIns
    {
      get => hardcodedHealthIns ??= new();
      set => hardcodedHealthIns = value;
    }

    /// <summary>
    /// A value of HardcodedVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodedVoluntary")]
    public ObligationType HardcodedVoluntary
    {
      get => hardcodedVoluntary ??= new();
      set => hardcodedVoluntary = value;
    }

    /// <summary>
    /// A value of HardcodeRecovery.
    /// </summary>
    [JsonPropertyName("hardcodeRecovery")]
    public ObligationType HardcodeRecovery
    {
      get => hardcodeRecovery ??= new();
      set => hardcodeRecovery = value;
    }

    /// <summary>
    /// A value of HardcodedDebt.
    /// </summary>
    [JsonPropertyName("hardcodedDebt")]
    public ObligationTransaction HardcodedDebt
    {
      get => hardcodedDebt ??= new();
      set => hardcodedDebt = value;
    }

    /// <summary>
    /// A value of HardcodedObligor.
    /// </summary>
    [JsonPropertyName("hardcodedObligor")]
    public CsePersonAccount HardcodedObligor
    {
      get => hardcodedObligor ??= new();
      set => hardcodedObligor = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of InitializedDate.
    /// </summary>
    [JsonPropertyName("initializedDate")]
    public DateWorkArea InitializedDate
    {
      get => initializedDate ??= new();
      set => initializedDate = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public Document Print
    {
      get => print ??= new();
      set => print = value;
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
    /// A value of MultipleActions.
    /// </summary>
    [JsonPropertyName("multipleActions")]
    public Common MultipleActions
    {
      get => multipleActions ??= new();
      set => multipleActions = value;
    }

    /// <summary>
    /// A value of CodeCodeValue.
    /// </summary>
    [JsonPropertyName("codeCodeValue")]
    public CodeValue CodeCodeValue
    {
      get => codeCodeValue ??= new();
      set => codeCodeValue = value;
    }

    /// <summary>
    /// A value of CodeCode.
    /// </summary>
    [JsonPropertyName("codeCode")]
    public Code CodeCode
    {
      get => codeCode ??= new();
      set => codeCode = value;
    }

    /// <summary>
    /// A value of CodeReturnCode.
    /// </summary>
    [JsonPropertyName("codeReturnCode")]
    public Common CodeReturnCode
    {
      get => codeReturnCode ??= new();
      set => codeReturnCode = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
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
    /// A value of Rcaprpay.
    /// </summary>
    [JsonPropertyName("rcaprpay")]
    public Common Rcaprpay
    {
      get => rcaprpay ??= new();
      set => rcaprpay = value;
    }

    /// <summary>
    /// A value of SaveAction.
    /// </summary>
    [JsonPropertyName("saveAction")]
    public Common SaveAction
    {
      get => saveAction ??= new();
      set => saveAction = value;
    }

    private Common includeCrdInd;
    private Common day2;
    private Common day1;
    private WorkArea paymentFreq;
    private Common errorDay2;
    private Common errorDay1;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationPaymentSchedule hardcodeOpsCMonthly;
    private ObligationPaymentSchedule hardcodeOpsCBiWeekly;
    private ObligationPaymentSchedule hardcodeOpsCWeekly;
    private ObligationPaymentSchedule hardcodeOpsCSemiMonthly;
    private ObligationPaymentSchedule hardcodeOpsCBiMonthly;
    private Common unsuccessfulOpCount;
    private Common previous;
    private FrequencyWorkSet initializedAddLineFrequencyWorkSet;
    private Common initializedAddLineCommon;
    private WorkArea initializedAddLineWorkArea;
    private ObligationPaymentSchedule initializedAddLineObligationPaymentSchedule;
      
    private Common actionCounter;
    private DateWorkArea paymentSchDiscontinue;
    private ObligationType hardcodedAccruing;
    private ObligationType hardcodedHealthIns;
    private ObligationType hardcodedVoluntary;
    private ObligationType hardcodeRecovery;
    private ObligationTransaction hardcodedDebt;
    private CsePersonAccount hardcodedObligor;
    private DateWorkArea maximum;
    private DateWorkArea initializedDate;
    private DateWorkArea current;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Document print;
    private NextTranInfo nextTranInfo;
    private Common multipleActions;
    private CodeValue codeCodeValue;
    private Code codeCode;
    private Common codeReturnCode;
    private Document document;
    private Infrastructure infrastructure;
    private SpDocKey spDocKey;
    private OutgoingDocument outgoingDocument;
    private Common rcaprpay;
    private Common saveAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Overlap.
    /// </summary>
    [JsonPropertyName("overlap")]
    public ObligationPaymentSchedule Overlap
    {
      get => overlap ??= new();
      set => overlap = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligorRule.
    /// </summary>
    [JsonPropertyName("obligorRule")]
    public RecaptureRule ObligorRule
    {
      get => obligorRule ??= new();
      set => obligorRule = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    private ObligationPaymentSchedule overlap;
    private CsePersonAccount csePersonAccount;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePerson csePerson;
    private Obligation obligation;
    private RecaptureRule obligorRule;
    private DebtDetail debtDetail;
    private Obligation concurrent;
    private LegalActionPerson legalActionPerson;
  }
#endregion
}
