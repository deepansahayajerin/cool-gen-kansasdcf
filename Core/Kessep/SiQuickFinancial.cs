// Program: SI_QUICK_FINANCIAL, ID: 374541621, model: 746.
// Short name: SWE03120
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_QUICK_FINANCIAL.
/// </para>
/// <para>
/// Financial
/// </para>
/// </summary>
[Serializable]
public partial class SiQuickFinancial: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUICK_FINANCIAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuickFinancial(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuickFinancial.
  /// </summary>
  public SiQuickFinancial(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 10/01/2009	J Huss		CQ# 211		Initial development
    // ----------------------------------------------------------------------------
    // ***************************************************************************************************
    // Action block:  The action block SI_QUICK_FINANCIAL (SWE03120) is used to 
    // retrieve
    //   the Financial data for the QUICK system.
    // PStep:  The SI_QUICK_FINANCIAL_RETR_MAIN procedure step (SWEQKFNP) calls 
    // the (SWE03120) action block.
    // COM Proxy:  The CA Gen COM Proxy DLL (SweqkfnsAX) invokes the SWEQKFNP 
    // procedure step.
    // Delegate:  The delegate module Quick.Retriever.Financial.dll executes the
    // COM Proxy
    //   using query parameters.
    // ***************************************************************************************************
    local.Current.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);
    local.Min.Date = new DateTime(1, 1, 1);

    // Convert text type requested filter start date to date type using yyyy-mm-
    // dd input format
    local.Year.Text4 = Substring(import.QuickInQuery.StDate, 1, 4);
    local.Month.Text2 = Substring(import.QuickInQuery.StDate, 5, 2);
    local.Day.Text2 = Substring(import.QuickInQuery.StDate, 7, 2);
    local.RequestedFilterStart.Date = StringToDate(local.Year.Text4 + "-" + local
      .Month.Text2 + "-" + local.Day.Text2);

    // Convert text type requested filter end date to date type using yyyy-mm-dd
    // input format
    local.Year.Text4 = Substring(import.QuickInQuery.EndDate, 1, 4);
    local.Month.Text2 = Substring(import.QuickInQuery.EndDate, 5, 2);
    local.Day.Text2 = Substring(import.QuickInQuery.EndDate, 7, 2);
    local.RequestedFilterEnd.Date = StringToDate(local.Year.Text4 + "-" + local
      .Month.Text2 + "-" + local.Day.Text2);
    UseCabFirstAndLastDateOfMonth();

    // Return a maximum of 3 years worth of data for payment and warrant 
    // details.
    local.FilterStartDate.Date = AddYears(local.Current.Date, -3);
    local.FilterEndDate.Date = local.Current.Date;

    // If the start date provided falls between 3 years ago and today, use it.
    if (!Lt(local.RequestedFilterStart.Date, local.FilterStartDate.Date) && !
      Lt(local.Current.Date, local.RequestedFilterStart.Date))
    {
      local.FilterStartDate.Date = local.RequestedFilterStart.Date;
    }

    // If the end date provided falls between the start date and today, use it.
    if (!Lt(local.Current.Date, local.RequestedFilterEnd.Date) && !
      Lt(local.RequestedFilterEnd.Date, local.FilterStartDate.Date))
    {
      local.FilterEndDate.Date = local.RequestedFilterEnd.Date;
    }

    // Get header information
    UseSiQuickGetCpHeader();

    if (IsExitState("CASE_NF"))
    {
      export.QuickErrorMessages.ErrorCode = "406";
      export.QuickErrorMessages.ErrorMessage = "Test Case Not Found";

      return;
    }

    export.QuickCpHeader.Assign(local.QuickCpHeader);

    // **********************************************************************
    // If family violence is found for any case participant on the case
    // then no data will be returned.  This includes all active and
    // inactive case participants.  This is true for open or closed cases.
    // **********************************************************************
    if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
    {
      export.QuickErrorMessages.ErrorCode = "407";
      export.QuickErrorMessages.ErrorMessage =
        "Disclosure prohibited on the case requested.";

      return;
    }

    local.ValidProgramCode.Text2 = "AF";
    local.ValidProgramState.Text2 = "PA";
    local.TotalAfPaOwed.NumericalDollarValue = 0;
    local.TotalAfPaDeductions.NumericalDollarValue = 0;
    export.QuickFinanceSummary.LastPaymentAmount = 0;
    export.QuickFinanceSummary.LastPaymentDate = "00010101";
    export.QuickFinanceSummary.MonthlyArrearsAmount = 0;
    export.QuickFinanceSummary.MonthlySupportAmount = 0;
    export.QuickFinanceSummary.OtherMonthlyAmount = 0;
    export.QuickFinanceSummary.TotalArrearsOwed = 0;
    export.QuickFinanceSummary.TotalAssignedArrears = 0;
    export.QuickFinanceSummary.TotalInterestOwed = 0;
    export.QuickFinanceSummary.TotalJudgmentAmount = 0;
    export.QuickFinanceSummary.TotalMonthlyAmount = 0;
    export.QuickFinanceSummary.TotalNcpFeesOwed = 0;
    export.QuickFinanceSummary.TotalOwedAmount = 0;
    local.CurrentObligation.NumericalDollarValue = 0;

    // Look for any debts owed by the NCP.  Ignore secondary obligations.
    foreach(var item in ReadObligationObligationTransactionObligationType())
    {
      local.ObligationType.Classification = "A";
      UseFnGetObligationStatus();

      // Don't process this Obligation if it's (D)eactivated
      if (AsChar(local.ScreenObligationStatus.ObligationStatus) == 'D')
      {
        continue;
      }

      // Check to see if this obligation transaction is in support of some 
      // person
      if (ReadCsePersonAccount2())
      {
        // Obligation transaction is in support of a person.  Check to see if 
        // this person is a member of this case
        if (ReadCsePerson())
        {
          // Supported person is a member of this case.  Include this obligation
          // transaction in the totals.
          local.Supported.Number = entities.SupportedCsePerson.Number;
        }
        else
        {
          // Supported person is not a member of this case.  We're not 
          // interested in this record.
          continue;
        }
      }
      else
      {
        // Obligation transaction is not in support of anybody specific.  
        // Include it in the totals.
      }

      // Fees don't have supported persons, so they need to be checked for 
      // validity by using
      // the court order's association to the CSE case.
      if (Equal(entities.ObligationType.Code, "FEE"))
      {
        // Check to see if the fee is court ordered
        if (ReadLegalAction())
        {
          // The fee is court ordered, check to see if it's associated to the 
          // requested case
          if (ReadCase())
          {
            // This is a court ordered fee that's associated to the requested 
            // case.  Continue processing
          }
          else
          {
            // This is a court ordered fee that's associated to another case.  
            // We're not interested in this record.
            continue;
          }
        }
        else
        {
          // This is an arbitrary fee that's not associated with any court 
          // order.  Include it in the totals
        }
      }

      local.AfPa.Flag = "N";

      // Check to see if this debt detail is for AF-PA.  If so, set flag to 
      // update assigned arrears amounts.
      foreach(var item1 in ReadDebtDetail3())
      {
        local.ObligationType.Classification = "A";
        UseFnDeterminePgmForDebtDetail();

        if (Equal(local.DprProgram.ProgramState, local.ValidProgramState.Text2) &&
          Equal(local.Program.Code, local.ValidProgramCode.Text2))
        {
          local.AfPa.Flag = "Y";
        }
      }

      if (Equal(entities.ObligationType.Code, "CS") || Equal
        (entities.ObligationType.Code, "SP") || Equal
        (entities.ObligationType.Code, "MS") || Equal
        (entities.ObligationType.Code, "MC") || Equal
        (entities.ObligationType.Code, "MJ") || Equal
        (entities.ObligationType.Code, "SAJ") || Equal
        (entities.ObligationType.Code, "AJ") || Equal
        (entities.ObligationType.Code, "CRCH") || Equal
        (entities.ObligationType.Code, "718B"))
      {
        // ****************************
        // Total Arrears Owed
        // ****************************
        foreach(var item1 in ReadDebtDetail1())
        {
          // If the due date is less than the first of this month it's arrears.
          if (Lt(entities.DebtDetail.DueDt, local.FirstOfMonth.Date))
          {
            export.QuickFinanceSummary.TotalArrearsOwed += entities.DebtDetail.
              BalanceDueAmt;
          }
        }
      }

      if (Equal(entities.ObligationType.Code, "CS") || Equal
        (entities.ObligationType.Code, "SP") || Equal
        (entities.ObligationType.Code, "MS"))
      {
        // ****************************
        // Monthly Support Amount
        // ****************************
        // Monthly support amount is the monthly amount due for accruing 
        // obligations
        // Check to see if accrual is still active
        if (ReadObligationPaymentSchedule1())
        {
          // Accrual is still active, include this debt in the total
          foreach(var item1 in ReadAccrualInstructions())
          {
            local.TempCommon.TotalCurrency =
              entities.ObligationTransaction.Amount;

            // Convert amount to a monthly amount
            UseFnCalculateMonthlyAmountDue();
            export.QuickFinanceSummary.MonthlySupportAmount += local.TempCommon.
              TotalCurrency;
          }
        }
        else
        {
          // Accrual is no longer active.  Do not include this debt
          goto Test;
        }

        // Total the amount from the current obligation that is not yet paid for
        // this month
        foreach(var item1 in ReadDebtDetail2())
        {
          local.CurrentObligation.NumericalDollarValue += entities.DebtDetail.
            BalanceDueAmt;

          // Check to see if this debt detail is for AF-PA.  If so, add to 
          // assigned arrears deductions.
          if (AsChar(local.AfPa.Flag) == 'Y')
          {
            local.TotalAfPaDeductions.NumericalDollarValue += entities.
              DebtDetail.BalanceDueAmt;
          }
        }
      }
      else if (Equal(entities.ObligationType.Code, "MJ") || Equal
        (entities.ObligationType.Code, "AJ") || Equal
        (entities.ObligationType.Code, "CRCH") || Equal
        (entities.ObligationType.Code, "SAJ") || Equal
        (entities.ObligationType.Code, "718B"))
      {
        // ****************************
        // Total Judgment Amount
        // ****************************
        // Total the amounts from the non-accruing judgments
        foreach(var item1 in ReadDebtDetail3())
        {
          export.QuickFinanceSummary.TotalJudgmentAmount += entities.DebtDetail.
            BalanceDueAmt;
        }

        // ****************************
        // Other Arrears Amount
        // ****************************
        // Only process payment schedules for each obligation once
        if (local.PreviousObligation.SystemGeneratedIdentifier == entities
          .Obligation.SystemGeneratedIdentifier)
        {
          goto Test;
        }

        local.PreviousObligation.SystemGeneratedIdentifier =
          entities.Obligation.SystemGeneratedIdentifier;

        // If payment arrangements have been made for arrears obligations,
        // those payment arrangements make up the Monthly Arrears Amount
        foreach(var item1 in ReadObligationPaymentSchedule2())
        {
          local.TempCommon.TotalCurrency =
            entities.ObligationPaymentSchedule.Amount.GetValueOrDefault();

          // Convert amount to a monthly amount
          UseFnCalculateMonthlyAmountDue();
          export.QuickFinanceSummary.MonthlyArrearsAmount += local.TempCommon.
            TotalCurrency;
        }
      }
      else if (Equal(entities.ObligationType.Code, "IJ") || Equal
        (entities.ObligationType.Code, "FEE"))
      {
        foreach(var item1 in ReadDebtDetail1())
        {
          if (Equal(entities.ObligationType.Code, "IJ"))
          {
            // ****************************
            // Total Interest Owed
            // ****************************
            export.QuickFinanceSummary.TotalInterestOwed += entities.DebtDetail.
              BalanceDueAmt;
          }
          else
          {
            // ****************************
            // Total NCP Fees Owed
            // ****************************
            export.QuickFinanceSummary.TotalNcpFeesOwed += entities.DebtDetail.
              BalanceDueAmt;
          }

          // The related IJ or Fee debt detail is for AF-PA, add balance due 
          // amount to assigned arrears deductions.
          if (AsChar(local.AfPa.Flag) == 'Y')
          {
            local.TotalAfPaDeductions.NumericalDollarValue += entities.
              DebtDetail.BalanceDueAmt;
          }
        }

        // ****************************
        // Other Monthly Amount
        // ****************************
        // Only process payment schedules for each obligation once
        if (local.PreviousObligation.SystemGeneratedIdentifier == entities
          .Obligation.SystemGeneratedIdentifier)
        {
          goto Test;
        }

        local.PreviousObligation.SystemGeneratedIdentifier =
          entities.Obligation.SystemGeneratedIdentifier;

        // If payment arrangements have been made for other obligations,
        // those payment arrangements make up the Monthly Other Amount
        foreach(var item1 in ReadObligationPaymentSchedule2())
        {
          local.TempCommon.TotalCurrency =
            entities.ObligationPaymentSchedule.Amount.GetValueOrDefault();

          // Convert amount to a monthly amount
          UseFnCalculateMonthlyAmountDue();
          export.QuickFinanceSummary.OtherMonthlyAmount += local.TempCommon.
            TotalCurrency;
        }
      }

Test:

      // ****************************
      // Total Owed Amount
      // ****************************
      // Total Owed Amount is the sum of all debts and interest
      foreach(var item1 in ReadDebtDetail1())
      {
        export.QuickFinanceSummary.TotalOwedAmount =
          export.QuickFinanceSummary.TotalOwedAmount + entities
          .DebtDetail.BalanceDueAmt + entities
          .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

        // Check to see if this debt detail is for AF-PA.  If so, add to total 
        // assigned arrears.
        if (AsChar(local.AfPa.Flag) == 'Y')
        {
          local.TotalAfPaOwed.NumericalDollarValue =
            local.TotalAfPaOwed.NumericalDollarValue + entities
            .DebtDetail.BalanceDueAmt + entities
            .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
        }
      }
    }

    // ****************************
    // Total Monthly Amount
    // ****************************
    // Total Monthly Amount is Monthly Support + Monthly Arrears + Other Monthly
    // Amount
    export.QuickFinanceSummary.TotalMonthlyAmount =
      export.QuickFinanceSummary.MonthlySupportAmount + export
      .QuickFinanceSummary.MonthlyArrearsAmount + export
      .QuickFinanceSummary.OtherMonthlyAmount;

    // ****************************
    // Total Assigned Arrears
    // ****************************
    // Total Assigned Arrears is Total Assigned Owed - Total Assigned Deductions
    // Total Assigned Owed is Total Owed for AF-PA
    // Total Assigned Deductions is Total Obligations, Fees, and Interest for AF
    // -PA
    export.QuickFinanceSummary.TotalAssignedArrears =
      local.TotalAfPaOwed.NumericalDollarValue - local
      .TotalAfPaDeductions.NumericalDollarValue;

    // ****************************
    // NCP Payment Details
    // ****************************
    // Look for payments made by the NCP that fall within the date range filter.
    // If the payment is associated with this CSE case or not associated with 
    // any CSE case, include it.
    export.Payments.Index = -1;

    foreach(var item in ReadCashReceiptDetail())
    {
      local.PaymentAmount.NumericalDollarValue = 0;

      foreach(var item1 in ReadCollection())
      {
        // If the collection has a court order number, check to see if it is 
        // related to this case.
        if (!IsEmpty(entities.Collection1.CourtOrderAppliedTo))
        {
          if (ReadObligationObligationTypeObligationTransaction())
          {
            if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'N')
            {
              // If the obligation does not require a supported person, check to
              // see if the obligation is court ordered
              if (ReadLegalAction())
              {
                // The obligation is court ordered, check to see if it's 
                // associated to the requested case
                if (ReadCase())
                {
                  // This is a court ordered obligation that's associated to the
                  // requested case.
                }
                else
                {
                  // This is a court ordered obligation that's associated to 
                  // another case.  We're not interested in this record.
                  continue;
                }
              }
              else
              {
                // This is an arbitrary obligation that's not associated with 
                // any court order.
              }
            }
            else
            {
              // If the obligation does required a supported person, then the 
              // supported person must be from this case.
              if (ReadCsePersonAccount1())
              {
                // Supported person is a participant of this case.
              }
              else
              {
                // Supported person is not a participant of this case.  We're 
                // not interested in this record.
                continue;
              }
            }
          }
          else
          {
            // Obligation is not owed by the current NCP.
            continue;
          }
        }

        local.CollectionType.Code = "";

        if (ReadCollectionType())
        {
          local.CollectionType.Code = entities.CollectionType.Code;
        }

        local.PaymentSourceCode.Text3 = "";

        if (Equal(local.CollectionType.Code, "F"))
        {
          // Kansas:	Federal Offset (F)
          // OCSE:	IRS Tax Intercept (FTO)
          local.PaymentSourceCode.Text3 = "FTO";
        }
        else if (Equal(local.CollectionType.Code, "U"))
        {
          // Kansas:	Unemployment Offset (U)
          // OCSE:	Unemployment Benefit (UBI)
          local.PaymentSourceCode.Text3 = "UBI";
        }
        else if (Equal(local.CollectionType.Code, "S") || Equal
          (local.CollectionType.Code, "K"))
        {
          // Kansas:	State Offset (S), KPERS (K)
          // OCSE:	State Tax Intercept (STO)
          local.PaymentSourceCode.Text3 = "STO";
        }
        else if (Equal(local.CollectionType.Code, "C") || Equal
          (local.CollectionType.Code, "A") || Equal
          (local.CollectionType.Code, "V"))
        {
          // Kansas:	Regular Collection (C), Collection Agency (A), Voluntary (V
          // )
          // OCSE:	Received Directly from NCP (NDP)
          local.PaymentSourceCode.Text3 = "NDP";
        }
        else if (Equal(local.CollectionType.Code, "I"))
        {
          // Kansas:	Income Withholding (I)
          // OCSE:	Income Withholding (NIW)
          local.PaymentSourceCode.Text3 = "NIW";
        }
        else if (Equal(local.CollectionType.Code, "T") || Equal
          (local.CollectionType.Code, "Y") || Equal
          (local.CollectionType.Code, "Z"))
        {
          // Kansas:	Treasury Offset (T, Y, Z)
          // OCSE:	Administrative Offset (FAO)
          local.PaymentSourceCode.Text3 = "FAO";
        }
        else if (Equal(local.CollectionType.Code, "P"))
        {
          // Kansas:	Genetic Test Fee (P)
          // OCSE:	Other (OTH)
          local.PaymentSourceCode.Text3 = "OTH";
        }
        else
        {
          goto ReadEach;
        }

        local.PaymentAmount.NumericalDollarValue += entities.Collection1.Amount;
      }

      // If no money from this cash receipt detail was applied to obligations 
      // for this case,
      // move to the next cash receipt detail
      if (local.PaymentAmount.NumericalDollarValue == 0)
      {
        continue;
      }

      // If the cash receipt detail falls in our requested date range, include 
      // it
      if (!Lt(entities.CashReceiptDetail.CollectionDate,
        local.FilterStartDate.Date) && !
        Lt(local.FilterEndDate.Date, entities.CashReceiptDetail.CollectionDate))
      {
        ++export.Payments.Index;
        export.Payments.CheckSize();

        if (export.Payments.Index >= Export.PaymentsGroup.Capacity)
        {
          break;
        }

        export.Payments.Update.QuickFinancePayment.Date =
          entities.CashReceiptDetail.CollectionDate;
        export.Payments.Update.QuickFinancePayment.Amount =
          local.PaymentAmount.NumericalDollarValue;
        export.Payments.Update.QuickFinancePayment.SourceCode =
          local.PaymentSourceCode.Text3;
      }

      // ****************************
      // Last Payment Date & Amount
      // ****************************
      // If the collection date is newer than the current last payment date, 
      // reset the summation.
      if (Lt(IntToDate((int)StringToNumber(
        export.QuickFinanceSummary.LastPaymentDate)),
        entities.CashReceiptDetail.CollectionDate))
      {
        export.QuickFinanceSummary.LastPaymentDate =
          NumberToString(DateToInt(entities.CashReceiptDetail.CollectionDate), 8);
          
        export.QuickFinanceSummary.LastPaymentAmount =
          local.PaymentAmount.NumericalDollarValue;
      }
      else if (Equal(entities.CashReceiptDetail.CollectionDate,
        IntToDate((int)StringToNumber(export.QuickFinanceSummary.LastPaymentDate))))
        
      {
        // Collection date is the same as a previous collection, continue the 
        // summation.
        export.QuickFinanceSummary.LastPaymentAmount += local.PaymentAmount.
          NumericalDollarValue;
      }
      else
      {
        // Date must be older than what we have already recorded.  We're not 
        // interested in the record
      }

ReadEach:
      ;
    }

    // ****************************
    // Disbursement Details
    // ****************************
    // Look for warrants in support of the CP or of children on the case.
    // If the warrant was sent to a designated payee rather than the CP, include
    // that name instead.
    // The warrant must be in a "paid" status and have an original print date.
    export.Disbursements.Index = -1;

    foreach(var item in ReadPaymentRequestPaymentStatus2())
    {
      // A reissue warrant will normally have a paid warrant related to it
      if (Equal(entities.PaymentStatus.Code, "REIS") || Equal
        (entities.PaymentStatus.Code, "PAID"))
      {
        local.PaymentRequest.Assign(entities.PaymentRequest);
        local.PaymentStatus.Code = entities.PaymentStatus.Code;

        // If a warrant is in a reissue status, find the related warrant(s) not 
        // in a reissue status.
        local.Bailout.Count = 0;
        local.TempPaymentRequest.SystemGeneratedIdentifier =
          entities.PaymentRequest.SystemGeneratedIdentifier;

        while(Equal(local.PaymentStatus.Code, "REIS"))
        {
          if (ReadPaymentRequestPaymentStatus1())
          {
            MovePaymentRequest(entities.ReissuePaymentRequest,
              local.PaymentRequest);
            local.PaymentStatus.Code = entities.ReissuePaymentStatus.Code;
          }
          else
          {
            // If a reissued warrant doesn't have a previous warrant associated 
            // with it, something went wrong.
            break;
          }

          local.TempPaymentRequest.SystemGeneratedIdentifier =
            entities.ReissuePaymentRequest.SystemGeneratedIdentifier;
          ++local.Bailout.Count;

          // If we've looped more than 50 times, something must have gone wrong.
          // Bail out.
          if (local.Bailout.Count > 50)
          {
            break;
          }
        }

        // If we didn't find a warrant in paid status, or what we did find isn't
        // a warrant, ignore the record
        if (!Equal(local.PaymentStatus.Code, "PAID") || !
          Equal(local.PaymentRequest.Type1, "WAR"))
        {
          continue;
        }
      }
      else
      {
        continue;
      }

      // The warrant must have a warrant number and print date to be valid
      if (IsEmpty(local.PaymentRequest.Number) || !
        Lt(local.Min.Date, local.PaymentRequest.PrintDate))
      {
        continue;
      }

      // If the warrant doesn't fall in our date range filter, move to the next 
      // record
      if (Lt(local.PaymentRequest.PrintDate, local.FilterStartDate.Date) || Lt
        (local.FilterEndDate.Date, local.PaymentRequest.PrintDate))
      {
        continue;
      }

      ++export.Disbursements.Index;
      export.Disbursements.CheckSize();

      if (export.Disbursements.Index >= Export.DisbursementsGroup.Capacity)
      {
        return;
      }

      export.Disbursements.Update.QuickFinanceDisbursement.Date =
        local.PaymentRequest.PrintDate;
      export.Disbursements.Update.QuickFinanceDisbursement.Amount =
        local.PaymentRequest.Amount;
      export.Disbursements.Update.QuickFinanceDisbursement.InstrumentNumber =
        local.PaymentRequest.Number ?? Spaces(15);

      // If the warrant was sent to a designated payee rather than the obligee, 
      // list that person instead.
      if (!IsEmpty(local.PaymentRequest.DesignatedPayeeCsePersonNo))
      {
        local.CsePersonsWorkSet.Number =
          local.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces(10);
      }
      else
      {
        local.CsePersonsWorkSet.Number =
          local.PaymentRequest.CsePersonNumber ?? Spaces(10);
      }

      local.CsePersonsWorkSet.FirstName = "";
      local.CsePersonsWorkSet.MiddleInitial = "";
      local.CsePersonsWorkSet.LastName = "";

      if (ReadCsePersonDetail())
      {
        local.CsePersonsWorkSet.FirstName = entities.CsePersonDetail.FirstName;
        local.CsePersonsWorkSet.MiddleInitial =
          entities.CsePersonDetail.MiddleInitial ?? Spaces(1);
        local.CsePersonsWorkSet.LastName = entities.CsePersonDetail.LastName;

        // Return name in <Last Name>, <First Name> <Middle Initial> format.
        export.Disbursements.Update.QuickFinanceDisbursement.RecipientName =
          TrimEnd(local.CsePersonsWorkSet.LastName) + ", " + TrimEnd
          (local.CsePersonsWorkSet.FirstName) + " " + TrimEnd
          (local.CsePersonsWorkSet.MiddleInitial);
      }
      else
      {
        // Person may not be loaded into CSE Person Detail table yet
      }
    }
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.CsePersonNumber = source.CsePersonNumber;
    target.DesignatedPayeeCsePersonNo = source.DesignatedPayeeCsePersonNo;
    target.Number = source.Number;
    target.PrintDate = source.PrintDate;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.FirstOfMonth.Date = useExport.First.Date;
    local.LastOfMonth.Date = useExport.Last.Date;
  }

  private void UseFnCalculateMonthlyAmountDue()
  {
    var useImport = new FnCalculateMonthlyAmountDue.Import();
    var useExport = new FnCalculateMonthlyAmountDue.Export();

    useImport.ObligationPaymentSchedule.Assign(
      entities.ObligationPaymentSchedule);
    useImport.Period.Date = local.Current.Date;
    useImport.PeriodAmountDue.TotalCurrency = local.TempCommon.TotalCurrency;

    Call(FnCalculateMonthlyAmountDue.Execute, useImport, useExport);

    local.TempCommon.TotalCurrency = useExport.MonthlyDue.TotalCurrency;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.Collection.Date = local.Current.Date;
    useImport.HardcodedAccruing.Classification =
      local.ObligationType.Classification;
    useImport.SupportedPerson.Number = local.Supported.Number;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
    local.Program.Code = useExport.Program.Code;
  }

  private void UseFnGetObligationStatus()
  {
    var useImport = new FnGetObligationStatus.Import();
    var useExport = new FnGetObligationStatus.Export();

    useImport.CsePerson.Number = entities.ApCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.CsePersonAccount.Type1 = entities.Obligor.Type1;
    useImport.Current.Date = local.Current.Date;
    useImport.HcOtCAccruing.Classification =
      local.ObligationType.Classification;

    Call(FnGetObligationStatus.Execute, useImport, useExport);

    local.ScreenObligationStatus.ObligationStatus =
      useExport.ScreenObligationStatus.ObligationStatus;
  }

  private void UseSiQuickGetCpHeader()
  {
    var useImport = new SiQuickGetCpHeader.Import();
    var useExport = new SiQuickGetCpHeader.Export();

    useImport.QuickInQuery.CaseId = import.QuickInQuery.CaseId;

    Call(SiQuickGetCpHeader.Execute, useImport, useExport);

    local.QuickCpHeader.Assign(useExport.QuickCpHeader);
    export.Case1.Number = useExport.Case1.Number;
  }

  private IEnumerable<bool> ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.AccrualInstructions.Populated = false;

    return ReadEach("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetNullableDate(
          command, "discontinueDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);

        return true;
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Case1.Number);
        db.SetString(command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr", local.QuickCpHeader.NcpPersonNumber);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 5);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection1.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.Collection1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection1.CollectionDt = db.GetDate(reader, 1);
        entities.Collection1.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection1.ConcurrentInd = db.GetString(reader, 3);
        entities.Collection1.CrtType = db.GetInt32(reader, 4);
        entities.Collection1.CstId = db.GetInt32(reader, 5);
        entities.Collection1.CrvId = db.GetInt32(reader, 6);
        entities.Collection1.CrdId = db.GetInt32(reader, 7);
        entities.Collection1.ObgId = db.GetInt32(reader, 8);
        entities.Collection1.CspNumber = db.GetString(reader, 9);
        entities.Collection1.CpaType = db.GetString(reader, 10);
        entities.Collection1.OtrId = db.GetInt32(reader, 11);
        entities.Collection1.OtrType = db.GetString(reader, 12);
        entities.Collection1.OtyId = db.GetInt32(reader, 13);
        entities.Collection1.CollectionAdjProcessDate = db.GetDate(reader, 14);
        entities.Collection1.Amount = db.GetDecimal(reader, 15);
        entities.Collection1.CourtOrderAppliedTo =
          db.GetNullableString(reader, 16);
        entities.Collection1.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection1.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection1.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection1.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection1.OtrType);

        return true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.
      Assert(entities.SupportedCsePersonAccount.Populated);
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.SupportedCsePersonAccount.CspNumber);
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.SupportedCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount1",
      (db, command) =>
      {
        db.SetString(
          command, "type", entities.ObligationTransaction.CpaSupType ?? "");
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspSupNumber ?? ""
          );
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.SupportedCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.SupportedCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.SupportedCsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.SupportedCsePersonAccount.Type1);
      });
  }

  private bool ReadCsePersonAccount2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.SupportedCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount2",
      (db, command) =>
      {
        db.SetString(
          command, "type", entities.ObligationTransaction.CpaSupType ?? "");
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspSupNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.SupportedCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.SupportedCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.SupportedCsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.SupportedCsePersonAccount.Type1);
      });
  }

  private bool ReadCsePersonDetail()
  {
    entities.CsePersonDetail.Populated = false;

    return Read("ReadCsePersonDetail",
      (db, command) =>
      {
        db.SetString(command, "personNumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonDetail.PersonNumber = db.GetString(reader, 0);
        entities.CsePersonDetail.FirstName = db.GetString(reader, 1);
        entities.CsePersonDetail.LastName = db.GetString(reader, 2);
        entities.CsePersonDetail.MiddleInitial =
          db.GetNullableString(reader, 3);
        entities.CsePersonDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail1",
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
        db.SetNullableDate(
          command, "retiredDt", local.Min.Date.GetValueOrDefault());
        db.
          SetDate(command, "dueDt", local.LastOfMonth.Date.GetValueOrDefault());
          
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 12);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail2",
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
        db.
          SetDate(command, "date1", local.FirstOfMonth.Date.GetValueOrDefault());
          
        db.
          SetDate(command, "date2", local.LastOfMonth.Date.GetValueOrDefault());
          
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 12);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetail3()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail3",
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 12);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationTransactionObligationType()
  {
    entities.ApCsePerson.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Obligor.Populated = false;

    return ReadEach("ReadObligationObligationTransactionObligationType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.QuickCpHeader.NcpPersonNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligor.CspNumber = db.GetString(reader, 1);
        entities.Obligor.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 6);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 8);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 9);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 10);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 14);
        entities.ApCsePerson.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Obligor.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);

        return true;
      });
  }

  private bool ReadObligationObligationTypeObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Collection1.Populated);
    entities.Obligation.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadObligationObligationTypeObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection1.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection1.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection1.OtrId);
        db.SetString(command, "cpaType", entities.Collection1.CpaType);
        db.SetString(command, "cspNumber1", entities.Collection1.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection1.ObgId);
        db.
          SetString(command, "cspNumber2", local.QuickCpHeader.NcpPersonNumber);
          
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 6);
        entities.ObligationType.Code = db.GetString(reader, 7);
        entities.ObligationType.Classification = db.GetString(reader, 8);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 9);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 11);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 12);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.Obligation.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private bool ReadObligationPaymentSchedule1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
        db.SetDate(command, "startDt", local.Current.Date.GetValueOrDefault());
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
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 8);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 9);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 10);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 11);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);
      });
  }

  private IEnumerable<bool> ReadObligationPaymentSchedule2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return ReadEach("ReadObligationPaymentSchedule2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
        db.SetDate(command, "startDt", local.Current.Date.GetValueOrDefault());
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
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 8);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 9);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 10);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 11);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);

        return true;
      });
  }

  private bool ReadPaymentRequestPaymentStatus1()
  {
    entities.ReissuePaymentRequest.Populated = false;
    entities.ReissuePaymentStatus.Populated = false;

    return Read("ReadPaymentRequestPaymentStatus1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqRGeneratedId",
          local.TempPaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ReissuePaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReissuePaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 1);
        entities.ReissuePaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 2);
        entities.ReissuePaymentRequest.Number = db.GetNullableString(reader, 3);
        entities.ReissuePaymentRequest.PrintDate =
          db.GetNullableDate(reader, 4);
        entities.ReissuePaymentRequest.Type1 = db.GetString(reader, 5);
        entities.ReissuePaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.ReissuePaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.ReissuePaymentStatus.Code = db.GetString(reader, 8);
        entities.ReissuePaymentRequest.Populated = true;
        entities.ReissuePaymentStatus.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.ReissuePaymentRequest.Type1);
          
      });
  }

  private IEnumerable<bool> ReadPaymentRequestPaymentStatus2()
  {
    entities.PaymentRequest.Populated = false;
    entities.PaymentStatus.Populated = false;

    return ReadEach("ReadPaymentRequestPaymentStatus2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber1", local.QuickCpHeader.CpPersonNumber);
        db.
          SetString(command, "cspNumber2", local.QuickCpHeader.NcpPersonNumber);
          
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 1);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 2);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 3);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 4);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 5);
        entities.PaymentRequest.Type1 = db.GetString(reader, 6);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 7);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.PaymentStatus.Code = db.GetString(reader, 9);
        entities.PaymentRequest.Populated = true;
        entities.PaymentStatus.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

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
    /// <summary>
    /// A value of QuickInQuery.
    /// </summary>
    [JsonPropertyName("quickInQuery")]
    public QuickInQuery QuickInQuery
    {
      get => quickInQuery ??= new();
      set => quickInQuery = value;
    }

    private QuickInQuery quickInQuery;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A DisbursementsGroup group.</summary>
    [Serializable]
    public class DisbursementsGroup
    {
      /// <summary>
      /// A value of QuickFinanceDisbursement.
      /// </summary>
      [JsonPropertyName("quickFinanceDisbursement")]
      public QuickFinanceDisbursement QuickFinanceDisbursement
      {
        get => quickFinanceDisbursement ??= new();
        set => quickFinanceDisbursement = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private QuickFinanceDisbursement quickFinanceDisbursement;
    }

    /// <summary>A PaymentsGroup group.</summary>
    [Serializable]
    public class PaymentsGroup
    {
      /// <summary>
      /// A value of QuickFinancePayment.
      /// </summary>
      [JsonPropertyName("quickFinancePayment")]
      public QuickFinancePayment QuickFinancePayment
      {
        get => quickFinancePayment ??= new();
        set => quickFinancePayment = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private QuickFinancePayment quickFinancePayment;
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
    /// A value of QuickCpHeader.
    /// </summary>
    [JsonPropertyName("quickCpHeader")]
    public QuickCpHeader QuickCpHeader
    {
      get => quickCpHeader ??= new();
      set => quickCpHeader = value;
    }

    /// <summary>
    /// A value of QuickErrorMessages.
    /// </summary>
    [JsonPropertyName("quickErrorMessages")]
    public QuickErrorMessages QuickErrorMessages
    {
      get => quickErrorMessages ??= new();
      set => quickErrorMessages = value;
    }

    /// <summary>
    /// A value of QuickFinanceSummary.
    /// </summary>
    [JsonPropertyName("quickFinanceSummary")]
    public QuickFinanceSummary QuickFinanceSummary
    {
      get => quickFinanceSummary ??= new();
      set => quickFinanceSummary = value;
    }

    /// <summary>
    /// Gets a value of Disbursements.
    /// </summary>
    [JsonIgnore]
    public Array<DisbursementsGroup> Disbursements => disbursements ??= new(
      DisbursementsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Disbursements for json serialization.
    /// </summary>
    [JsonPropertyName("disbursements")]
    [Computed]
    public IList<DisbursementsGroup> Disbursements_Json
    {
      get => disbursements;
      set => Disbursements.Assign(value);
    }

    /// <summary>
    /// Gets a value of Payments.
    /// </summary>
    [JsonIgnore]
    public Array<PaymentsGroup> Payments => payments ??= new(
      PaymentsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Payments for json serialization.
    /// </summary>
    [JsonPropertyName("payments")]
    [Computed]
    public IList<PaymentsGroup> Payments_Json
    {
      get => payments;
      set => Payments.Assign(value);
    }

    private Case1 case1;
    private QuickCpHeader quickCpHeader;
    private QuickErrorMessages quickErrorMessages;
    private QuickFinanceSummary quickFinanceSummary;
    private Array<DisbursementsGroup> disbursements;
    private Array<PaymentsGroup> payments;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AfPa.
    /// </summary>
    [JsonPropertyName("afPa")]
    public Common AfPa
    {
      get => afPa ??= new();
      set => afPa = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Bailout.
    /// </summary>
    [JsonPropertyName("bailout")]
    public Common Bailout
    {
      get => bailout ??= new();
      set => bailout = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CurrentObligation.
    /// </summary>
    [JsonPropertyName("currentObligation")]
    public FinanceWorkAttributes CurrentObligation
    {
      get => currentObligation ??= new();
      set => currentObligation = value;
    }

    /// <summary>
    /// A value of Day.
    /// </summary>
    [JsonPropertyName("day")]
    public TextWorkArea Day
    {
      get => day ??= new();
      set => day = value;
    }

    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    /// <summary>
    /// A value of FilterEndDate.
    /// </summary>
    [JsonPropertyName("filterEndDate")]
    public DateWorkArea FilterEndDate
    {
      get => filterEndDate ??= new();
      set => filterEndDate = value;
    }

    /// <summary>
    /// A value of FilterStartDate.
    /// </summary>
    [JsonPropertyName("filterStartDate")]
    public DateWorkArea FilterStartDate
    {
      get => filterStartDate ??= new();
      set => filterStartDate = value;
    }

    /// <summary>
    /// A value of FirstOfMonth.
    /// </summary>
    [JsonPropertyName("firstOfMonth")]
    public DateWorkArea FirstOfMonth
    {
      get => firstOfMonth ??= new();
      set => firstOfMonth = value;
    }

    /// <summary>
    /// A value of LastOfMonth.
    /// </summary>
    [JsonPropertyName("lastOfMonth")]
    public DateWorkArea LastOfMonth
    {
      get => lastOfMonth ??= new();
      set => lastOfMonth = value;
    }

    /// <summary>
    /// A value of MdArrears.
    /// </summary>
    [JsonPropertyName("mdArrears")]
    public FinanceWorkAttributes MdArrears
    {
      get => mdArrears ??= new();
      set => mdArrears = value;
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
    /// A value of Min.
    /// </summary>
    [JsonPropertyName("min")]
    public DateWorkArea Min
    {
      get => min ??= new();
      set => min = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public TextWorkArea Month
    {
      get => month ??= new();
      set => month = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of PaymentAmount.
    /// </summary>
    [JsonPropertyName("paymentAmount")]
    public FinanceWorkAttributes PaymentAmount
    {
      get => paymentAmount ??= new();
      set => paymentAmount = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of PaymentSourceCode.
    /// </summary>
    [JsonPropertyName("paymentSourceCode")]
    public WorkArea PaymentSourceCode
    {
      get => paymentSourceCode ??= new();
      set => paymentSourceCode = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of PreviousCollection.
    /// </summary>
    [JsonPropertyName("previousCollection")]
    public Collection PreviousCollection
    {
      get => previousCollection ??= new();
      set => previousCollection = value;
    }

    /// <summary>
    /// A value of PreviousObligation.
    /// </summary>
    [JsonPropertyName("previousObligation")]
    public Obligation PreviousObligation
    {
      get => previousObligation ??= new();
      set => previousObligation = value;
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
    /// A value of QuickCpHeader.
    /// </summary>
    [JsonPropertyName("quickCpHeader")]
    public QuickCpHeader QuickCpHeader
    {
      get => quickCpHeader ??= new();
      set => quickCpHeader = value;
    }

    /// <summary>
    /// A value of RequestedFilterEnd.
    /// </summary>
    [JsonPropertyName("requestedFilterEnd")]
    public DateWorkArea RequestedFilterEnd
    {
      get => requestedFilterEnd ??= new();
      set => requestedFilterEnd = value;
    }

    /// <summary>
    /// A value of RequestedFilterStart.
    /// </summary>
    [JsonPropertyName("requestedFilterStart")]
    public DateWorkArea RequestedFilterStart
    {
      get => requestedFilterStart ??= new();
      set => requestedFilterStart = value;
    }

    /// <summary>
    /// A value of ScreenObligationStatus.
    /// </summary>
    [JsonPropertyName("screenObligationStatus")]
    public ScreenObligationStatus ScreenObligationStatus
    {
      get => screenObligationStatus ??= new();
      set => screenObligationStatus = value;
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
    /// A value of ScreenOwedAmountsDtl.
    /// </summary>
    [JsonPropertyName("screenOwedAmountsDtl")]
    public ScreenOwedAmountsDtl ScreenOwedAmountsDtl
    {
      get => screenOwedAmountsDtl ??= new();
      set => screenOwedAmountsDtl = value;
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
    /// A value of TempCommon.
    /// </summary>
    [JsonPropertyName("tempCommon")]
    public Common TempCommon
    {
      get => tempCommon ??= new();
      set => tempCommon = value;
    }

    /// <summary>
    /// A value of TempPaymentRequest.
    /// </summary>
    [JsonPropertyName("tempPaymentRequest")]
    public PaymentRequest TempPaymentRequest
    {
      get => tempPaymentRequest ??= new();
      set => tempPaymentRequest = value;
    }

    /// <summary>
    /// A value of TotalAfPaDeductions.
    /// </summary>
    [JsonPropertyName("totalAfPaDeductions")]
    public FinanceWorkAttributes TotalAfPaDeductions
    {
      get => totalAfPaDeductions ??= new();
      set => totalAfPaDeductions = value;
    }

    /// <summary>
    /// A value of TotalAfPaOwed.
    /// </summary>
    [JsonPropertyName("totalAfPaOwed")]
    public FinanceWorkAttributes TotalAfPaOwed
    {
      get => totalAfPaOwed ??= new();
      set => totalAfPaOwed = value;
    }

    /// <summary>
    /// A value of ValidProgramCode.
    /// </summary>
    [JsonPropertyName("validProgramCode")]
    public TextWorkArea ValidProgramCode
    {
      get => validProgramCode ??= new();
      set => validProgramCode = value;
    }

    /// <summary>
    /// A value of ValidProgramState.
    /// </summary>
    [JsonPropertyName("validProgramState")]
    public TextWorkArea ValidProgramState
    {
      get => validProgramState ??= new();
      set => validProgramState = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public TextWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    private Common afPa;
    private CsePerson ap;
    private Common bailout;
    private CollectionType collectionType;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea current;
    private FinanceWorkAttributes currentObligation;
    private TextWorkArea day;
    private DprProgram dprProgram;
    private DateWorkArea filterEndDate;
    private DateWorkArea filterStartDate;
    private DateWorkArea firstOfMonth;
    private DateWorkArea lastOfMonth;
    private FinanceWorkAttributes mdArrears;
    private DateWorkArea max;
    private DateWorkArea min;
    private TextWorkArea month;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationType obligationType;
    private FinanceWorkAttributes paymentAmount;
    private PaymentRequest paymentRequest;
    private WorkArea paymentSourceCode;
    private PaymentStatus paymentStatus;
    private Collection previousCollection;
    private Obligation previousObligation;
    private Program program;
    private QuickCpHeader quickCpHeader;
    private DateWorkArea requestedFilterEnd;
    private DateWorkArea requestedFilterStart;
    private ScreenObligationStatus screenObligationStatus;
    private ScreenOwedAmounts screenOwedAmounts;
    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
    private CsePerson supported;
    private Common tempCommon;
    private PaymentRequest tempPaymentRequest;
    private FinanceWorkAttributes totalAfPaDeductions;
    private FinanceWorkAttributes totalAfPaOwed;
    private TextWorkArea validProgramCode;
    private TextWorkArea validProgramState;
    private TextWorkArea year;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("apLegalActionCaseRole")]
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ApLegalActionPerson.
    /// </summary>
    [JsonPropertyName("apLegalActionPerson")]
    public LegalActionPerson ApLegalActionPerson
    {
      get => apLegalActionPerson ??= new();
      set => apLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ChildLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("childLegalActionCaseRole")]
    public LegalActionCaseRole ChildLegalActionCaseRole
    {
      get => childLegalActionCaseRole ??= new();
      set => childLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ChildLegalActionPerson.
    /// </summary>
    [JsonPropertyName("childLegalActionPerson")]
    public LegalActionPerson ChildLegalActionPerson
    {
      get => childLegalActionPerson ??= new();
      set => childLegalActionPerson = value;
    }

    /// <summary>
    /// A value of Collection1.
    /// </summary>
    [JsonPropertyName("collection1")]
    public Collection Collection1
    {
      get => collection1 ??= new();
      set => collection1 = value;
    }

    /// <summary>
    /// A value of Collection2.
    /// </summary>
    [JsonPropertyName("collection2")]
    public DisbursementTransaction Collection2
    {
      get => collection2 ??= new();
      set => collection2 = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of CsePersonDetail.
    /// </summary>
    [JsonPropertyName("csePersonDetail")]
    public CsePersonDetail CsePersonDetail
    {
      get => csePersonDetail ??= new();
      set => csePersonDetail = value;
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
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of ReissuePaymentRequest.
    /// </summary>
    [JsonPropertyName("reissuePaymentRequest")]
    public PaymentRequest ReissuePaymentRequest
    {
      get => reissuePaymentRequest ??= new();
      set => reissuePaymentRequest = value;
    }

    /// <summary>
    /// A value of ReissuePaymentStatus.
    /// </summary>
    [JsonPropertyName("reissuePaymentStatus")]
    public PaymentStatus ReissuePaymentStatus
    {
      get => reissuePaymentStatus ??= new();
      set => reissuePaymentStatus = value;
    }

    /// <summary>
    /// A value of ReissuePaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("reissuePaymentStatusHistory")]
    public PaymentStatusHistory ReissuePaymentStatusHistory
    {
      get => reissuePaymentStatusHistory ??= new();
      set => reissuePaymentStatusHistory = value;
    }

    /// <summary>
    /// A value of SupportedCaseRole.
    /// </summary>
    [JsonPropertyName("supportedCaseRole")]
    public CaseRole SupportedCaseRole
    {
      get => supportedCaseRole ??= new();
      set => supportedCaseRole = value;
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
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
    }

    private AccrualInstructions accrualInstructions;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private LegalActionCaseRole apLegalActionCaseRole;
    private LegalActionPerson apLegalActionPerson;
    private CaseRole arCaseRole;
    private CsePerson arCsePerson;
    private Case1 case1;
    private CashReceiptDetail cashReceiptDetail;
    private LegalActionCaseRole childLegalActionCaseRole;
    private LegalActionPerson childLegalActionPerson;
    private Collection collection1;
    private DisbursementTransaction collection2;
    private CollectionType collectionType;
    private CsePerson csePerson;
    private CsePersonDetail csePersonDetail;
    private DebtDetail debtDetail;
    private DisbursementTransaction disbursement;
    private DisbursementTransactionRln disbursementTransactionRln;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private CsePersonAccount obligee;
    private CsePersonAccount obligor;
    private PaymentRequest paymentRequest;
    private PaymentStatus paymentStatus;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentRequest reissuePaymentRequest;
    private PaymentStatus reissuePaymentStatus;
    private PaymentStatusHistory reissuePaymentStatusHistory;
    private CaseRole supportedCaseRole;
    private CsePerson supportedCsePerson;
    private CsePersonAccount supportedCsePersonAccount;
  }
#endregion
}
