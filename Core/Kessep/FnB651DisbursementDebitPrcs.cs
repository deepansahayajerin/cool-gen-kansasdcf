// Program: FN_B651_DISBURSEMENT_DEBIT_PRCS, ID: 372544546, model: 746.
// Short name: SWEF651B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B651_DISBURSEMENT_DEBIT_PRCS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB651DisbursementDebitPrcs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B651_DISBURSEMENT_DEBIT_PRCS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB651DisbursementDebitPrcs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB651DisbursementDebitPrcs.
  /// </summary>
  public FnB651DisbursementDebitPrcs(IContext context, Import import,
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
    // <<< R.B.Mohapatra   MTW
    // It was decided that the Recaptures will be addressed in the SWEFB641 
    // Batch procedure. So the execution of the Recapture logic was bypassed in
    // this procedure. But as a safety measure, we will be retaining the codes
    // for some more time in this procedure and use this local variable to
    // bypass the Recapture processing Logic. >>>
    // ****************************************************************
    // Put in code to have the program abend less. The code was originally 
    // written to abend for almost anything. Have replaced that strategy so that
    // only the severest errors and an accumalation of errors trigger an Abend.
    // This is per managements request after the code was built, then changed
    // by me, then revisited much later and done in a rush. Its not pretty but
    // it works.  revisit this rk 4/26
    // 99-11-01  PR 78493  Fangman  Added code to check for person_program 
    // med_type of 'EM' or 'WT' when determining if Cost Recovery Fee should not
    // be taken.
    // 99-11-02  PR 77907  Fangman  Added code to pass the Cash_Non_Cash 
    // indicator to FN_Create_Disbursement.
    // 00-02-09  PR 86861  Fangman  - Replace code for determining if CR Fees 
    // should be taken & some restructuring of other code.
    // 00-03-31  PR 86768  Fangman -  Pass collection date to collection 
    // suppression AB.  Added check to ensure that only one FDSO letter goes out
    // instead of multiple when the collection is backed off and reapplied or
    // when there are multiple FDSO collections for same per/ref in same night.
    // Changed suppression logic to ensure that a FDSO collection suppression is
    // not overriden by a person or automatic suppression.
    // 04-07-00  WK 000206  Fangman - Added code to handle URA suppression.
    // 08-28-00  WK 000206  Fangman - Changed code to set the effective date of 
    // a X suppression rule to the process date instead of the current date.
    // 09-07-00  WK 000206  Fangman - Changed code to set the X suppression rule
    // ID based on the last ID for that AR instead of the last one in the
    // table.
    // 09-12-00  103323  Fangman  Changed code to do less I/O within AB to 
    // create disbursements.  Will now abend after 400 errors.  This was put in
    // with the changes to fix the disb suppr with past discontinue dates.
    // 09-19-00  PRWORA SEG ID A4 - Added suppression release counts.
    // ****************************************************************
    // ****************************************************************
    // 09-25-00  PR 98039  Fangman  - Change code to avoid duplicate payments.
    // 12/07/00  PR 108996  Fangman - Changed code to include CR Fee amt in 
    // duplicate total fields.  Added code to print out info to help resolve the
    // problem with suppressing non-duplicate payments.
    // 12/08/00  PR 108996  Fangman - Changed IF stmt prior to AB to apply dup 
    // disb to Pot Recoveries.  Note:  All work fields for duplicate
    // disbursement logic include the cost recovery fees because they are
    // calculated using the disb cr NOT the disb db.
    // 12/26/00  PR 108996  Fangman - Changed print stmts for Dup Pmt Area & 
    // commented out a stmt to increment the Loc_Tot_Disb_For_Ref_Nbr.
    // 01/03/01  PR 108996  Fangman - Uncommented the code that suppresses/
    // recaptures duplicate payments.  Commented out the print statements for
    // the duplicate payment code.
    // 04/12/01  PR 118495  Fangman - Dup Pmt modification.
    // 12/07/01  PR 118495  Fangman - Dup Pmt modification.  Added code to keep 
    // separate total for suppr xura amt.
    // 01/28/02  04/04/01  WR 000235  Fangman - PSUM redesign.  Started in 04/04
    // /01 then put on hold for other projects.  Also adding a change to handle
    // an error exist state when returning from fn_release_suppressed_disb.
    // 05/08/02  PR 146227  Added last 3 fields to initialization view of 
    // monthly obligee summary.
    // ****************************************************************
    // ****************************************************************
    // 05/29/02  PR 144630  Changed code in one of the action blocks to use the 
    // suppression reason of the original disbursement for the backed out
    // disbursement for X URA disbursements being suppressed.  Along with this
    // change I am adding some more info in the abend print area so that we can
    // better research any abends that occur.
    // ****************************************************************
    // -----------------------------------------------------------------
    // 10/02/02  Fangman  WR 020120
    //      Added code to pass new field needed so that action blocks could call
    // new archive action block that returns a unique disbursement transaction
    // ID by checking both the prod & archive tables to ensure that the ID is
    // unique in both tables.
    // -----------------------------------------------------------------
    // -----------------------------------------------------------------
    // 02/16/04  Fangman  PR 200634
    //      Added code to fix problem with ending processing on certain errors 
    // instead of reporting them and continuing processing with other
    // transactions.
    // -----------------------------------------------------------------
    // 08/25/2009   J Harden   CQ 7814  Close Adabas  (added si_close_adabas)
    // ______________________________________________________________
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 10/22/2010  Raj S              CQ9426      Modified to exempt Cost 
    // Recovery Fee for *
    // *
    // 
    // Incoming interstate request with foreign *
    // *
    // 
    // address.                                 *
    // *
    // 
    // *
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.ErrorLoop.Flag = "Y";
    local.AbendCheckLoop.Flag = "Y";
    local.HardcodedMs.SystemGeneratedIdentifier = 3;
    local.HardcodedPerson.Type1 = "P";
    local.HardcodedCollectonType.Type1 = "C";
    local.HardcodedAutomatic.Type1 = "A";
    local.HardcodedGift.SystemGeneratedIdentifier = 152;
    local.EabFileHandling.Action = "WRITE";

    if (AsChar(local.AbendCheckLoop.Flag) == 'Y')
    {
      UseFnB651Housekeeping();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.ToInitializeObligee.LastDisbDt =
          local.ProgramProcessingInfo.ProcessDate;
        local.WorkingDisbursement.DisbursementDate =
          local.ProgramProcessingInfo.ProcessDate;
      }
      else
      {
        local.AbendOccurred.Flag = "Y";

        goto Test2;
      }

      foreach(var item in ReadDisbursementTransactionCsePersonAccountCsePerson())
        
      {
        if (!IsEmpty(local.TestFirstObligee.Number) || !
          IsEmpty(local.TestLastObligee.Number))
        {
          if (Lt(entities.ObligeeCsePerson.Number, local.TestFirstObligee.Number))
            
          {
            continue;
          }

          if (Lt(local.TestLastObligee.Number, entities.ObligeeCsePerson.Number))
            
          {
            break;
          }
        }

        if (AsChar(local.TestDisplayInd.Flag) == 'Y')
        {
          UseFnB651PrintDisbursement();
        }

        // **** PR 200634 - Fix prob in 651 w/ ending processing on certain 
        // errors ****
        local.DatabaseUpdated.Flag = "N";
        local.Rollback.Flag = "N";

        if (AsChar(local.ErrorLoop.Flag) == 'Y')
        {
          ++local.CountsAndAmounts.NbrOfDisbRead.Count;
          local.CountsAndAmounts.AmtOfDisbRead.TotalCurrency += entities.
            DisbursementTransaction.Amount;

          if (Equal(entities.Collection.ProgramAppliedTo, "NC"))
          {
            ExitState = "FN0000_NC_PROG_CODE_FOUND";

            goto Test1;
          }

          local.WorkingDisbursement.Amount =
            entities.DisbursementTransaction.Amount;
          local.WorkingDisbursement.ProcessDate =
            local.InitializedDateWorkArea.Date;
          local.WorkingDisbursement.CashNonCashInd =
            entities.CashReceiptType.CategoryIndicator;
          local.AmtRemainingToDisburse.Amount =
            entities.DisbursementTransaction.Amount;

          if (Equal(entities.Collection.ProgramAppliedTo, "NA") || Equal
            (entities.Collection.ProgramAppliedTo, "NAI"))
          {
            local.AdcProgram.Flag = "N";
          }
          else
          {
            local.AdcProgram.Flag = "Y";
          }

          if (!Equal(entities.DisbursementTransaction.ReferenceNumber,
            local.Hold.ReferenceNumber))
          {
            if (!IsEmpty(local.HoldObligee.Number))
            {
              if (local.PrevTotArRefNbrAmt.TotalCurrency > 0)
              {
                if (local.CurrTotArRefNbrAmt.TotalCurrency > 0)
                {
                  // This collection has already been counted so do not count 
                  // again.
                }
                else if (local.PrevTotArRefNbrAmt.TotalCurrency + local
                  .CurrTotArRefNbrAmt.TotalCurrency == 0)
                {
                  // This collection has been counted previously and now it has 
                  // been backed off so subtract one from the collection count
                  // for the AR/YR MM/Ref #.
                  local.ToUpdateMonthlyObligeeSummary.NumberOfCollections = -1;
                }
                else
                {
                  // This collection has been counted previously and now it has 
                  // been partially backed off but some of it still applies to
                  // this AR so leave the collection count for the AR/YR MM/Ref
                  // # alone.
                }
              }
              else if (local.CurrTotArRefNbrAmt.TotalCurrency > 0)
              {
                // This collection has not been counted previously so add one to
                // the collection count for the AR/YR MM/Ref #.
                local.ToUpdateMonthlyObligeeSummary.NumberOfCollections = 1;
              }
              else
              {
                // This collection has not been counted previously and should 
                // not be counted this time so leave the collection count for
                // the AR/YR MM/Ref # alone.
              }
            }

            local.CurrTotArRefNbrAmt.TotalCurrency = 0;

            // ============  F.  Update the OBLIGEE MONTHLY TOTALS for the 
            // previous AR ============
            if (AsChar(local.HoldObligee.Type1) == 'C')
            {
              if (!Equal(entities.PreviousObligeeCsePerson.Number,
                local.HoldObligee.Number))
              {
                if (ReadCsePersonAccountCsePerson())
                {
                  // Continue
                }
                else
                {
                  ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";

                  goto Test1;
                }
              }

              UseFnUpdateObligeeMonthlyTotals();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto Test1;
              }
            }

            local.ToUpdateMonthlyObligeeSummary.Assign(
              local.ToInitializeMonthlyObligeeSummary);
            local.ToUpdateMonthlyObligeeSummary.Month =
              Month(entities.DisbursementTransaction.CollectionDate);
            local.ToUpdateMonthlyObligeeSummary.Year =
              Year(entities.DisbursementTransaction.CollectionDate);
            MoveDisbursementTransaction1(entities.DisbursementTransaction,
              local.Hold);

            if (AsChar(entities.CashReceiptType.CategoryIndicator) == 'C')
            {
              UseFnB651DupPmtCheck();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto Test1;
              }

              if (AsChar(local.CheckForDupPmt.Flag) == 'Y')
              {
                ++local.CountsAndAmounts.NbrOfDupChecks.Count;
              }
            }
            else
            {
              local.CheckForDupPmt.Flag = "N";
            }
          }

          if (!Equal(entities.ObligeeCsePerson.Number, local.HoldObligee.Number))
            
          {
            // Initialize the ura event table
            local.ApEvent.Count = 0;

            if (!IsEmpty(local.HoldObligee.Number))
            {
              // ===========  G.  Update the OBLIGEE TOTALS  ============
              local.ToUpdateObligee.LastDisbDt =
                local.ProgramProcessingInfo.ProcessDate;

              if (AsChar(entities.ObligeeCsePerson.Type1) == 'C')
              {
                UseFnUpdateObligeeTotals();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  goto Test1;
                }
              }
            }

            MoveCsePerson(entities.ObligeeCsePerson, local.HoldObligee);
            local.Hold.ReferenceNumber =
              entities.DisbursementTransaction.ReferenceNumber;
            local.ToUpdateObligee.Assign(local.ToInitializeObligee);
          }

          // ============  A.  Determine DISBURSEMENT TYPE type by checking the 
          // oblig type, the coll applied_to_code and program type.
          // ============
          UseFnDetermineDisbType();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test1;
          }

          // ============  B.  Determine COST RECOVERY FEES  ============
          // ***************************************************************************************
          // * START  CQ9426: Exempt Cost Recovery Fee for Interstate Request 
          // with Forign address  *
          // ***************************************************************************************
          ReadInterstateRequest();

          if (entities.InterstateRequest.Populated && !
            IsEmpty(entities.InterstateRequest.Country))
          {
            // ***************************************************************************************
            // * The Interstate Request linked to the current disbursement 
            // transaction is for foriegn*
            // * address, need to exempt from Cost 
            // Recovery Fee
            // 
            // *
            // ***************************************************************************************
            // ***************************************************************************************
            // * END   CQ9426: Exempt Cost Recovery Fee for Interstate Request 
            // with Forign address  *
            // ***************************************************************************************
          }
          else if (AsChar(entities.CashReceiptType.CategoryIndicator) == 'C'
            && AsChar(local.AdcProgram.Flag) == 'N' && AsChar
            (entities.Collection.AppliedToCode) != 'G')
          {
            // Cost recovery fees are taken on Cash collections that are NA and 
            // are not gifts.
            if (AsChar(entities.Collection.AdjustedInd) == 'Y')
            {
              local.TakeOutCrFee.Flag = "Y";
            }
            else
            {
              UseFnB651DetIfCrFeeNeeded();
            }

            if (AsChar(local.TakeOutCrFee.Flag) == 'Y')
            {
              UseFnAssessNonAdcCostRecvyFee();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.CrFeeTaken.TotalCurrency =
                  entities.DisbursementTransaction.Amount - local
                  .AmtRemainingToDisburse.Amount;

                if (local.CrFeeTaken.TotalCurrency != 0)
                {
                  local.ToUpdateMonthlyObligeeSummary.FeeAmount =
                    local.ToUpdateMonthlyObligeeSummary.FeeAmount.
                      GetValueOrDefault() + local.CrFeeTaken.TotalCurrency;
                  ++local.ProcessCountToCommit.Count;
                  ++local.CountsAndAmounts.NbrOfDisbCreated.Count;
                  local.CountsAndAmounts.AmtOfDisbCreated.
                    TotalCurrency += local.CrFeeTaken.TotalCurrency;
                }
              }
              else
              {
                goto Test1;
              }
            }
          }

          // ============  C.  Determine SUPPRESSION  ============
          local.ForCreate.SuppressionReason = "";
          local.HighestSuppressionDate.Date =
            local.InitializedDateWorkArea.Date;

          if (AsChar(entities.CashReceiptType.CategoryIndicator) == 'C' && (
            Equal(entities.Collection.ProgramAppliedTo, "NA") || Equal
            (entities.Collection.ProgramAppliedTo, "NAI") || Equal
            (entities.Collection.ProgramAppliedTo, "AFI") || Equal
            (entities.Collection.ProgramAppliedTo, "FCI")))
          {
            // ****  Dup Suppr -  1. Only NA, NAI, AFI or FCI disbursements can 
            // be suppressed.    2. Only suppress cash collections.  3.  If
            // suppressing for duplicate payments then there is no reason to
            // check other suppressions rules.
            if (AsChar(local.TestDisplayInd.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail = "    Disbursable money";
              UseCabControlReport();
            }

            if (AsChar(local.CheckForDupPmt.Flag) == 'Y' && entities
              .DisbursementTransaction.Amount > 0)
            {
              if (local.AmtRemainingToDisburse.Amount <= local
                .CollFundsAvailForDisb.TotalCurrency)
              {
                // ****  No need to check for duplicate logic.
                local.CollFundsAvailForDisb.TotalCurrency -= local.
                  AmtRemainingToDisburse.Amount;

                if (AsChar(local.TestDisplayInd.Flag) == 'Y')
                {
                  local.EabReportSend.RptDetail =
                    "    Disbursement is LE collection funds available for disbursement";
                    
                  UseCabControlReport();
                }
              }
              else
              {
                if (local.CollFundsAvailForDisb.TotalCurrency > 0)
                {
                  // **** Split this disbursement into 2 so that part of it can 
                  // be released & part of it can be applied to previous
                  // potential recoveries or be suppressed.
                  // New AB to split out part of the disbursement & apply the 
                  // suppression logic & then create the disbursement.
                  UseFnB651SplitDupDisbToNet();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    goto Test1;
                  }

                  if (AsChar(local.TestDisplayInd.Flag) == 'Y')
                  {
                    local.EabReportSend.RptDetail =
                      "    Disbursement is split " + NumberToString
                      ((long)(local.CollFundsAvailForDisb.TotalCurrency * 100),
                      15);
                    UseCabControlReport();
                  }

                  ++local.CountsAndAmounts.NbrOfDisbCreated.Count;
                  local.CountsAndAmounts.AmtOfDisbCreated.
                    TotalCurrency += local.CollFundsAvailForDisb.TotalCurrency;
                  local.CollFundsAvailForDisb.TotalCurrency = 0;
                }

                UseFnB651ApplyDupToPotRcvObl();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  goto Test1;
                }

                // Any part of the disb not applied to potential recoveries will
                // be suppressed with a 'D' rule.
                local.ForCreate.SuppressionReason = "D";
                local.HighestSuppressionDate.Date = local.Max.Date;

                if (AsChar(local.TestDisplayInd.Flag) == 'Y' && local
                  .AmtRemainingToDisburse.Amount > 0)
                {
                  local.EabReportSend.RptDetail =
                    "    After looking for recoveries some left to suppr for 'D' " +
                    NumberToString
                    ((long)(local.AmtRemainingToDisburse.Amount * 100), 15);
                  UseCabControlReport();
                }
              }
            }
            else if (AsChar(local.CheckForDupPmt.Flag) == 'Y' && entities
              .DisbursementTransaction.Amount < 0)
            {
              local.CollFundsAvailForDisb.TotalCurrency -= local.
                AmtRemainingToDisburse.Amount;
            }

            if (local.AmtRemainingToDisburse.Amount != 0 && Equal
              (local.HighestSuppressionDate.Date,
              local.InitializedDateWorkArea.Date))
            {
              // If suppression was already turned on for Duplicate Payments 
              // then skip other suppression rules.
              UseFnB651ApplySuppressionRules();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto Test1;
              }
            }
          }

          local.ToUpdateMonthlyObligeeSummary.CollectionsAmount =
            local.ToUpdateMonthlyObligeeSummary.CollectionsAmount.
              GetValueOrDefault() + entities.DisbursementTransaction.Amount;

          if (AsChar(entities.CashReceiptType.CategoryIndicator) == 'C')
          {
            if (AsChar(local.AdcProgram.Flag) == 'N')
            {
              local.WorkingDisbursement.ProcessDate =
                local.InitializedDateWorkArea.Date;

              if (Lt(local.InitializedDateWorkArea.Date,
                local.HighestSuppressionDate.Date))
              {
                local.DisbursementStatus.SystemGeneratedIdentifier =
                  entities.Per3Suppressed.SystemGeneratedIdentifier;
              }
              else
              {
                local.DisbursementStatus.SystemGeneratedIdentifier =
                  entities.Per1Released.SystemGeneratedIdentifier;
                local.ToUpdateObligee.LastDisbAmt =
                  local.ToUpdateObligee.LastDisbAmt.GetValueOrDefault() + local
                  .WorkingDisbursement.Amount;
              }

              if (Equal(entities.Collection.ProgramAppliedTo, "NA"))
              {
                // ************************* NA *************************
                // The tot_non_adc_undisb_amt field really contains the total 
                // non-adc disbursed amount.
                local.ToUpdateObligee.TotNonAdcUndisbAmt =
                  local.ToUpdateObligee.TotNonAdcUndisbAmt.GetValueOrDefault() +
                  local.WorkingDisbursement.Amount;
              }

              if (entities.ObligationType.SystemGeneratedIdentifier == local
                .HardcodedMs.SystemGeneratedIdentifier)
              {
                local.ToUpdateObligee.TotMedDisbAmt =
                  local.ToUpdateObligee.TotMedDisbAmt.GetValueOrDefault() + local
                  .WorkingDisbursement.Amount;
              }
            }
            else
            {
              // ************************* ADC *************************
              local.ToUpdateObligee.TotAdcUndisbAmt =
                local.WorkingDisbursement.Amount;

              if (Equal(entities.Collection.ProgramAppliedTo, "AFI") || Equal
                (entities.Collection.ProgramAppliedTo, "FCI"))
              {
                // ************************* ADC  Interstate 
                // *************************
                if (Equal(local.HighestSuppressionDate.Date,
                  local.InitializedDateWorkArea.Date))
                {
                  local.DisbursementStatus.SystemGeneratedIdentifier =
                    entities.Per1Released.SystemGeneratedIdentifier;
                }
                else
                {
                  local.DisbursementStatus.SystemGeneratedIdentifier =
                    entities.Per3Suppressed.SystemGeneratedIdentifier;
                }

                local.WorkingDisbursement.ProcessDate =
                  local.InitializedDateWorkArea.Date;
              }
              else
              {
                // ************************* ADC  Kansas 
                // *************************
                local.DisbursementStatus.SystemGeneratedIdentifier =
                  entities.Per2Processed.SystemGeneratedIdentifier;
                local.WorkingDisbursement.ProcessDate =
                  local.ProgramProcessingInfo.ProcessDate;
              }
            }
          }
          else
          {
            // ************************* NON-CASH *************************
            local.DisbursementStatus.SystemGeneratedIdentifier =
              entities.Per2Processed.SystemGeneratedIdentifier;
            local.WorkingDisbursement.ProcessDate =
              local.ProgramProcessingInfo.ProcessDate;

            if (AsChar(local.AdcProgram.Flag) == 'Y')
            {
              local.ToUpdateObligee.TotAdcUndisbAmt =
                local.ToUpdateObligee.TotAdcUndisbAmt.GetValueOrDefault() + local
                .WorkingDisbursement.Amount;
            }
          }

          if (local.AmtRemainingToDisburse.Amount != 0)
          {
            // ============  D.  Create the DEBIT DISBURSEMENT  ============
            local.WorkingDisbursement.Amount =
              local.AmtRemainingToDisburse.Amount;
            UseFnCreateDisbursementNew();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ++local.ProcessCountToCommit.Count;
              ++local.CountsAndAmounts.NbrOfDisbCreated.Count;
              local.CountsAndAmounts.AmtOfDisbCreated.TotalCurrency += local.
                WorkingDisbursement.Amount;

              if (AsChar(entities.DisbursementTransaction.ExcessUraInd) == 'Y'
                && AsChar(local.CheckForDupPmt.Flag) == 'Y' && local
                .AmtRemainingToDisburse.Amount < 0 && Lt
                (local.ProgramProcessingInfo.ProcessDate,
                local.HighestSuppressionDate.Date))
              {
                // If this is an XURA and it is negative and suppressed then 
                // suptract it back out from the funds avail for disb because
                // the postive coming thru later may not be XURA (and therefore
                // would not be suppressed for the same period of time) so we
                // would not want it to release.
                local.CollFundsAvailForDisb.TotalCurrency += local.
                  AmtRemainingToDisburse.Amount;

                if (AsChar(local.TestDisplayInd.Flag) == 'Y')
                {
                  local.EabReportSend.RptDetail =
                    "  Reducing Funds Avail to Disb for suppr neg XURA amt of " +
                    NumberToString
                    ((long)(local.AmtRemainingToDisburse.Amount * 100), 7, 9);
                  UseCabControlReport();
                }
              }

              if (Lt(local.InitializedDateWorkArea.Date,
                local.HighestSuppressionDate.Date))
              {
                local.ToUpdateMonthlyObligeeSummary.DisbursementsSuppressed =
                  local.ToUpdateMonthlyObligeeSummary.DisbursementsSuppressed.
                    GetValueOrDefault() + local.WorkingDisbursement.Amount;
              }
              else if (AsChar(entities.DisbursementTransaction.ExcessUraInd) ==
                'Y')
              {
                local.ToUpdateMonthlyObligeeSummary.TotExcessUraAmt =
                  local.ToUpdateMonthlyObligeeSummary.TotExcessUraAmt.
                    GetValueOrDefault() + local.WorkingDisbursement.Amount;
              }
              else if (ReadDisbursementType())
              {
                if (Equal(entities.DisbursementType.ProgramCode, "AF"))
                {
                  local.ToUpdateMonthlyObligeeSummary.AdcReimbursedAmount =
                    local.ToUpdateMonthlyObligeeSummary.AdcReimbursedAmount.
                      GetValueOrDefault() + local.WorkingDisbursement.Amount;
                }
                else
                {
                  local.ToUpdateMonthlyObligeeSummary.CollectionsDisbursedToAr =
                    local.ToUpdateMonthlyObligeeSummary.
                      CollectionsDisbursedToAr.GetValueOrDefault() + local
                    .WorkingDisbursement.Amount;
                }
              }
              else
              {
                ExitState = "FN0000_DISB_TYPE_NF";

                goto Test1;
              }
            }
            else
            {
              goto Test1;
            }
          }

          if (AsChar(local.TestDisplayInd.Flag) == 'Y' && AsChar
            (local.CheckForDupPmt.Flag) == 'Y')
          {
            if (local.CollFundsAvailForDisb.TotalCurrency < 0)
            {
              local.Sign.Text1 = "-";
            }
            else
            {
              local.Sign.Text1 = "";
            }

            local.NumericFundsAvailForDisb.Number112 =
              local.CollFundsAvailForDisb.TotalCurrency;
            UseCabFormat112AmtFieldTo8();
            local.EabReportSend.RptDetail =
              "    Coll funds avail for disbursement " + local.Sign.Text1 + local
              .TextFundsAvailForDisb.Text9;
            UseCabControlReport();
          }

          // ============  E.  Update the CREDIT DISBURSEMENT  ============
          try
          {
            UpdateDisbursementTransaction();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_DISB_TRANS_NU_RB";

                goto Test1;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_DISB_TRANSACTION_PV";

                goto Test1;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          local.CurrTotArRefNbrAmt.TotalCurrency += entities.
            DisbursementTransaction.Amount;

          if (local.ProcessCountToCommit.Count >= local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            local.ProcessCountToCommit.Count = 0;

            if (AsChar(local.TestRunInd.Flag) != 'Y')
            {
              UseExtToDoACommit();

              if (local.PassArea.NumericReturnCode != 0)
              {
                ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

                goto Test1;
              }
            }
          }

          ExitState = "ACO_NN0000_ALL_OK";
        }

Test1:

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("ADABAS_UNAVAILABLE_RB") || IsExitState
            ("ADABAS_READ_UNSUCCESSFUL") || IsExitState
            ("ACO_NE0000_CICS_UNAVAILABLE") || IsExitState
            ("ADABAS_INVALID_RETURN_CODE"))
          {
            // These errors should not cause an abend.
            local.DatabaseUpdated.Flag = "N";
          }

          if (AsChar(local.DatabaseUpdated.Flag) == 'Y')
          {
            goto Test2;
          }
          else
          {
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = "AR " + entities
              .ObligeeCsePerson.Number + "  Disb ID " + NumberToString
              (entities.DisbursementTransaction.SystemGeneratedIdentifier, 7, 9) +
              "  ERROR:  " + local.ExitStateMessage.Message;
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            UseFnB651PrintDisbursement();
            ++local.CountsAndAmounts.NbrOfErrors.Count;
            local.CountsAndAmounts.AmtOfErrors.TotalCurrency += entities.
              DisbursementTransaction.Amount;

            if (local.CountsAndAmounts.NbrOfErrors.Count > local
              .ErrorsBeforeAbend.LastUsedNumber)
            {
              local.AbendOccurred.Flag = "Y";
              local.EabReportSend.RptDetail =
                "Maximum errors occurred in SWEFB651 - must abend.";
              UseCabErrorReport1();

              goto Test2;
            }
          }
        }

        ExitState = "ACO_NN0000_ALL_OK";
      }
    }

Test2:

    if (IsEmpty(local.HoldObligee.Number))
    {
      local.EabReportSend.RptDetail =
        "No disbursements credits were found to process.";
      UseCabErrorReport1();
    }
    else if (AsChar(local.AbendOccurred.Flag) != 'Y')
    {
      if (AsChar(entities.ObligeeCsePerson.Type1) == 'C')
      {
        if (local.PrevTotArRefNbrAmt.TotalCurrency > 0)
        {
          if (local.CurrTotArRefNbrAmt.TotalCurrency > 0)
          {
            // This collection has already been counted so do not count again.
          }
          else if (local.PrevTotArRefNbrAmt.TotalCurrency + local
            .CurrTotArRefNbrAmt.TotalCurrency == 0)
          {
            // This collection has been counted previously and now it has been 
            // backed off so subtract one from the collection count for the AR/
            // YR MM/Ref #.
            local.ToUpdateMonthlyObligeeSummary.NumberOfCollections = -1;
          }
          else
          {
            // This collection has been counted previously and now it has been 
            // partially backed off but some of it still applies to this AR so
            // leave the collection count for the AR/YR MM/Ref # alone.
          }
        }
        else if (local.CurrTotArRefNbrAmt.TotalCurrency > 0)
        {
          // This collection has not been counted previously so add one to the 
          // collection count for the AR/YR MM/Ref #.
          local.ToUpdateMonthlyObligeeSummary.NumberOfCollections = 1;
        }
        else
        {
          // This collection has not been counted previously and should not be 
          // counted this time so leave the collection count for the AR/YR MM/
          // Ref # alone.
        }

        // ============  F.  Update the OBLIGEE MONTHLY TOTALS for the last AR 
        // ============
        if (!Equal(entities.PreviousObligeeCsePerson.Number,
          local.HoldObligee.Number))
        {
          if (ReadCsePersonAccountCsePerson())
          {
            // Continue
          }
          else
          {
            ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";

            goto Test3;
          }
        }

        UseFnUpdateObligeeMonthlyTotals();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test3;
        }

        // ============  G.  Update the OBLIGEE TOTALS for the last AR  
        // ============
        UseFnUpdateObligeeTotals();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.AbendOccurred.Flag = "Y";

          goto Test3;
        }
      }

      if (AsChar(local.TestRunInd.Flag) != 'Y')
      {
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";
          local.AbendOccurred.Flag = "Y";
        }
      }
    }

Test3:

    if (AsChar(local.AbendOccurred.Flag) != 'Y')
    {
      // ============  H.  Update the SUPPRESSION STATUS on all suppressed 
      // disbursements  ==========
      UseFnReleaseSuppressedDisbursemt();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE"))
      {
        return;
      }
      else
      {
        ++local.CountsAndAmounts.NbrOfErrors.Count;
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail =
          "Error in routine that releases suppr disb: " + local
          .ExitStateMessage.Message;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.AbendOccurred.Flag = "Y";
      }
    }

    // *************************************
    // All Abend errors will fall out here.
    // *************************************
    if (AsChar(local.AbendOccurred.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "Abend situation encountered.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "Loc Hold CSE Person " + local
        .HoldObligee.Number + "  Loc Hold Reference Nbr " + (
          local.Hold.ReferenceNumber ?? "") + " Loc Hold Disb Id " + NumberToString
        (local.Hold.SystemGeneratedIdentifier, 7, 9);
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "AR " + entities
        .ObligeeCsePerson.Number + "  Disb ID " + NumberToString
        (entities.DisbursementTransaction.SystemGeneratedIdentifier, 7, 9) + "  Abend Error " +
        local.ExitStateMessage.Message;
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      UseFnB651PrintDisbursement();
      local.EabReportSend.RptDetail = "About to abend SWEFB651.";
      UseCabErrorReport2();
      local.EabReportSend.RptDetail = "";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "End of Abend msgs.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      // ****************************************************************
      // End of the program write out the control totals
      // ***************************************************************
      UseFnB651WriteTotals();

      // ****************
      // Close the files
      // ****************
      UseSiCloseAdabas();
      local.CloseInd.Flag = "Y";
      UseFnB651PrintErrorLine();
      local.EabFileHandling.Action = "CLOSE";
      local.EabReportSend.RptDetail = "";
      UseCabControlReport();
      UseCabErrorReport3();

      if (AsChar(local.TestRunInd.Flag) == 'Y')
      {
        ExitState = "ACO_NN000_ROLLBACK_FOR_BATCH_TST";
      }
      else
      {
        ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
      }
    }
  }

  private static void MoveApEvent1(FnB651ApplySuppressionRules.Export.
    ApEventGroup source, Local.ApEventGroup target)
  {
    target.ApGrpDtl.Number = source.ApGrpDtl.Number;
    target.RegUraGrpDtl.Flag = source.RegUraGrpDtl.Flag;
    target.RegUraAdjGrpDtl.Flag = source.RegUraAdjGrpDtl.Flag;
    target.MedUraGrpDtl.Flag = source.MedUraGrpDtl.Flag;
    target.MedUraAdjGrpDtl.Flag = source.MedUraAdjGrpDtl.Flag;
  }

  private static void MoveApEvent2(FnB651SplitDupDisbToNet.Export.
    ApEventGroup source, Local.ApEventGroup target)
  {
    target.ApGrpDtl.Number = source.ApGrpDtl.Number;
    target.RegUraGrpDtl.Flag = source.RegUraGrpDtl.Flag;
    target.RegUraAdjGrpDtl.Flag = source.RegUraAdjGrpDtl.Flag;
    target.MedUraGrpDtl.Flag = source.MedUraGrpDtl.Flag;
    target.MedUraAdjGrpDtl.Flag = source.MedUraAdjGrpDtl.Flag;
  }

  private static void MoveApEvent3(Local.ApEventGroup source,
    FnB651ApplySuppressionRules.Export.ApEventGroup target)
  {
    target.ApGrpDtl.Number = source.ApGrpDtl.Number;
    target.RegUraGrpDtl.Flag = source.RegUraGrpDtl.Flag;
    target.RegUraAdjGrpDtl.Flag = source.RegUraAdjGrpDtl.Flag;
    target.MedUraGrpDtl.Flag = source.MedUraGrpDtl.Flag;
    target.MedUraAdjGrpDtl.Flag = source.MedUraAdjGrpDtl.Flag;
  }

  private static void MoveApEvent4(Local.ApEventGroup source,
    FnB651SplitDupDisbToNet.Export.ApEventGroup target)
  {
    target.ApGrpDtl.Number = source.ApGrpDtl.Number;
    target.RegUraGrpDtl.Flag = source.RegUraGrpDtl.Flag;
    target.RegUraAdjGrpDtl.Flag = source.RegUraAdjGrpDtl.Flag;
    target.MedUraGrpDtl.Flag = source.MedUraGrpDtl.Flag;
    target.MedUraAdjGrpDtl.Flag = source.MedUraAdjGrpDtl.Flag;
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.AppliedToCode = source.AppliedToCode;
    target.CollectionDt = source.CollectionDt;
    target.AdjustedInd = source.AdjustedInd;
    target.AppliedToFuture = source.AppliedToFuture;
    target.DistPgmStateAppldTo = source.DistPgmStateAppldTo;
  }

  private static void MoveCountsAndAmounts1(FnB651WriteTotals.Export.
    CountsAndAmountsGroup source, Local.CountsAndAmountsGroup target)
  {
    target.NbrOfDisbRead.Count = source.NbrOfDisbRead.Count;
    target.NbrOfDisbCreated.Count = source.NbrOfDisbCreated.Count;
    target.NbrOfErrors.Count = source.NbrOfErrors.Count;
    target.NbrOfDupChecks.Count = source.NbrOfDupChecks.Count;
    target.AmtOfDisbRead.TotalCurrency = source.AmtOfDisbRead.TotalCurrency;
    target.AmtOfDisbCreated.TotalCurrency =
      source.AmtOfDisbCreated.TotalCurrency;
    target.AmtOfErrors.TotalCurrency = source.AmtOfErrors.TotalCurrency;
  }

  private static void MoveCountsAndAmounts2(Local.CountsAndAmountsGroup source,
    FnB651WriteTotals.Export.CountsAndAmountsGroup target)
  {
    target.NbrOfDisbRead.Count = source.NbrOfDisbRead.Count;
    target.NbrOfDisbCreated.Count = source.NbrOfDisbCreated.Count;
    target.NbrOfErrors.Count = source.NbrOfErrors.Count;
    target.NbrOfDupChecks.Count = source.NbrOfDupChecks.Count;
    target.AmtOfDisbRead.TotalCurrency = source.AmtOfDisbRead.TotalCurrency;
    target.AmtOfDisbCreated.TotalCurrency =
      source.AmtOfDisbCreated.TotalCurrency;
    target.AmtOfErrors.TotalCurrency = source.AmtOfErrors.TotalCurrency;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonAccount(CsePersonAccount source,
    CsePersonAccount target)
  {
    target.Type1 = source.Type1;
    target.CspNumber = source.CspNumber;
  }

  private static void MoveDisbursementTransaction1(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
  }

  private static void MoveDisbursementTransaction2(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.CollectionDate = source.CollectionDate;
    target.InterstateInd = source.InterstateInd;
    target.ExcessUraInd = source.ExcessUraInd;
    target.CollectionProcessDate = source.CollectionProcessDate;
  }

  private static void MoveDisbursementTransaction(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.CollectionDate = source.CollectionDate;
    target.InterstateInd = source.InterstateInd;
    target.ExcessUraInd = source.ExcessUraInd;
    target.CollectionProcessDate = source.CollectionProcessDate;
    target.IntInterId = source.IntInterId;
  }

  private static void MoveDisbursementTransaction4(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.CollectionDate = source.CollectionDate;
    target.ExcessUraInd = source.ExcessUraInd;
  }

  private static void MoveMonthlyObligeeSummary(MonthlyObligeeSummary source,
    MonthlyObligeeSummary target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
    target.PassthruRecapAmt = source.PassthruRecapAmt;
    target.DisbursementsSuppressed = source.DisbursementsSuppressed;
    target.RecapturedAmt = source.RecapturedAmt;
    target.NaArrearsRecapAmt = source.NaArrearsRecapAmt;
    target.PassthruAmount = source.PassthruAmount;
    target.CollectionsAmount = source.CollectionsAmount;
    target.CollectionsDisbursedToAr = source.CollectionsDisbursedToAr;
    target.FeeAmount = source.FeeAmount;
    target.AdcReimbursedAmount = source.AdcReimbursedAmount;
    target.TotExcessUraAmt = source.TotExcessUraAmt;
    target.NumberOfCollections = source.NumberOfCollections;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveSupprCounts1(FnReleaseSuppressedDisbursemt.Export.
    SupprCountsGroup source, Local.SupprCountsGroup target)
  {
    target.DtlSupprRead.Count = source.DtlSupprRead.Count;
    target.DtlSupprReleased.Count = source.DtlSupprReleased.Count;
    target.DtlSupprExtended.Count = source.DtlSupprExtended.Count;
    target.DtlSupprReduced.Count = source.DtlSupprReduced.Count;
    target.DtlSupprNoChange.Count = source.DtlSupprNoChange.Count;
    target.DtlPSuppr.Count = source.DtlPSupprCnt.Count;
    target.DtlCSuppr.Count = source.DtlCSupprCnt.Count;
    target.DtlASuppr.Count = source.DtlASupprCnt.Count;
    target.DtlXSuppr.Count = source.DtlXSupprCnt.Count;
    target.DtlDSuppr.Count = source.DtlDSupprCnt.Count;
    target.DtlPSupprAmt.TotalCurrency = source.DtlPSupprAmt.TotalCurrency;
    target.DtlCSupprAmt.TotalCurrency = source.DtlCSupprAmt.TotalCurrency;
    target.DtlASupprAmt.TotalCurrency = source.DtlASupprAmt.TotalCurrency;
    target.DtlXSupprAmt.TotalCurrency = source.DtlXSupprAmt.TotalCurrency;
    target.DtlDSupprAmt.TotalCurrency = source.DtlDSupprAmt.TotalCurrency;
    target.DtlYSuppr.Count = source.DtlYSupprCnt.Count;
    target.DtlZSuppr.Count = source.DtlZSupprCnt.Count;
    target.DtlYSupprAmt.TotalCurrency = source.DtlYSupprAmt.TotalCurrency;
    target.DtlZSupprAmt.TotalCurrency = source.DtlZSupprAmt.TotalCurrency;
  }

  private static void MoveSupprCounts2(Local.SupprCountsGroup source,
    FnB651WriteTotals.Export.SupprCountsGroup target)
  {
    target.DtlSupprRead.Count = source.DtlSupprRead.Count;
    target.DtlSupprReleased.Count = source.DtlSupprReleased.Count;
    target.DtlSupprExtended.Count = source.DtlSupprExtended.Count;
    target.DtlSupprReduced.Count = source.DtlSupprReduced.Count;
    target.DtlSupprNoChange.Count = source.DtlSupprNoChange.Count;
    target.DtlPSuppr.Count = source.DtlPSuppr.Count;
    target.DtlCSuppr.Count = source.DtlCSuppr.Count;
    target.DtlASuppr.Count = source.DtlASuppr.Count;
    target.DtlXSuppr.Count = source.DtlXSuppr.Count;
    target.DtlDSuppr.Count = source.DtlDSuppr.Count;
    target.DtlPSupprAmt.TotalCurrency = source.DtlPSupprAmt.TotalCurrency;
    target.DtlCSupprAmt.TotalCurrency = source.DtlCSupprAmt.TotalCurrency;
    target.DtlASupprAmt.TotalCurrency = source.DtlASupprAmt.TotalCurrency;
    target.DtlXSupprAmt.TotalCurrency = source.DtlXSupprAmt.TotalCurrency;
    target.DtlDSupprAmt.TotalCurrency = source.DtlDSupprAmt.TotalCurrency;
    target.DtlYSuppr.Count = source.DtlYSuppr.Count;
    target.DtlZSuppr.Count = source.DtlZSuppr.Count;
    target.DtlYSupprAmt.TotalCurrency = source.DtlYSupprAmt.TotalCurrency;
    target.DtlZSupprAmt.TotalCurrency = source.DtlZSupprAmt.TotalCurrency;
  }

  private static void MoveSupprCounts3(FnB651WriteTotals.Export.
    SupprCountsGroup source, Local.SupprCountsGroup target)
  {
    target.DtlSupprRead.Count = source.DtlSupprRead.Count;
    target.DtlSupprReleased.Count = source.DtlSupprReleased.Count;
    target.DtlSupprExtended.Count = source.DtlSupprExtended.Count;
    target.DtlSupprReduced.Count = source.DtlSupprReduced.Count;
    target.DtlSupprNoChange.Count = source.DtlSupprNoChange.Count;
    target.DtlPSuppr.Count = source.DtlPSuppr.Count;
    target.DtlCSuppr.Count = source.DtlCSuppr.Count;
    target.DtlASuppr.Count = source.DtlASuppr.Count;
    target.DtlXSuppr.Count = source.DtlXSuppr.Count;
    target.DtlDSuppr.Count = source.DtlDSuppr.Count;
    target.DtlPSupprAmt.TotalCurrency = source.DtlPSupprAmt.TotalCurrency;
    target.DtlCSupprAmt.TotalCurrency = source.DtlCSupprAmt.TotalCurrency;
    target.DtlASupprAmt.TotalCurrency = source.DtlASupprAmt.TotalCurrency;
    target.DtlXSupprAmt.TotalCurrency = source.DtlXSupprAmt.TotalCurrency;
    target.DtlDSupprAmt.TotalCurrency = source.DtlDSupprAmt.TotalCurrency;
    target.DtlYSuppr.Count = source.DtlYSuppr.Count;
    target.DtlZSuppr.Count = source.DtlZSuppr.Count;
    target.DtlYSupprAmt.TotalCurrency = source.DtlYSupprAmt.TotalCurrency;
    target.DtlZSupprAmt.TotalCurrency = source.DtlZSupprAmt.TotalCurrency;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabFormat112AmtFieldTo8()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.NumericFundsAvailForDisb.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.TextFundsAvailForDisb.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateMessage.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateMessage.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnAssessNonAdcCostRecvyFee()
  {
    var useImport = new FnAssessNonAdcCostRecvyFee.Import();
    var useExport = new FnAssessNonAdcCostRecvyFee.Export();

    useImport.PerCollection1.Assign(entities.DisbursementTransaction);
    useImport.PerCollection2.Assign(entities.Collection);
    useImport.PerObligation.Assign(entities.Obligation);
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.PerObligee.Assign(entities.ObligeeCsePersonAccount);
    useImport.Per73CrFee.Assign(entities.Per73CrFee);
    useImport.Per2Processed.Assign(entities.Per2Processed);
    useImport.PerDisbursementTranRlnRsn.Assign(entities.Per1);
    useImport.TestDisplay.Flag = local.TestDisplayInd.Flag;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Max.Date = local.Max.Date;

    Call(FnAssessNonAdcCostRecvyFee.Execute, useImport, useExport);

    MoveDisbursementTransaction2(useImport.PerCollection1,
      entities.DisbursementTransaction);
    MoveCollection(useImport.PerCollection2, entities.Collection);
    entities.Obligation.SystemGeneratedIdentifier =
      useImport.PerObligation.SystemGeneratedIdentifier;
    entities.ObligeeCsePersonAccount.Type1 = useImport.PerObligee.Type1;
    entities.Per73CrFee.SystemGeneratedIdentifier =
      useImport.Per73CrFee.SystemGeneratedIdentifier;
    entities.Per2Processed.SystemGeneratedIdentifier =
      useImport.Per2Processed.SystemGeneratedIdentifier;
    entities.Per1.SystemGeneratedIdentifier =
      useImport.PerDisbursementTranRlnRsn.SystemGeneratedIdentifier;
    local.AmtRemainingToDisburse.Amount =
      useExport.RemainingDisbursement.Amount;
    local.DatabaseUpdated.Flag = useExport.DatabaseUpdated.Flag;
  }

  private void UseFnB651ApplyDupToPotRcvObl()
  {
    var useImport = new FnB651ApplyDupToPotRcvObl.Import();
    var useExport = new FnB651ApplyDupToPotRcvObl.Export();

    useImport.ExpDatabaseUpdated.Flag = local.DatabaseUpdated.Flag;
    useImport.ExpAmtRemainingToDisburs.Amount =
      local.AmtRemainingToDisburse.Amount;
    useImport.PerObligeeCsePerson.Assign(entities.ObligeeCsePerson);
    useImport.PerObligeeCsePersonAccount.
      Assign(entities.ObligeeCsePersonAccount);
    useImport.PerDisbursementTransaction.
      Assign(entities.DisbursementTransaction);
    useImport.CashReceiptType.CategoryIndicator =
      entities.CashReceiptType.CategoryIndicator;
    useImport.DisbursementType.SystemGeneratedIdentifier =
      local.Determined.SystemGeneratedIdentifier;
    useImport.Per1AfCcs.Assign(entities.Per1AfCcs);
    useImport.Per2AfAcs.Assign(entities.Per2AfAcs);
    useImport.Per4NaCcs.Assign(entities.Per4NaCcs);
    useImport.Per5NaAcs.Assign(entities.Per5NaCcs);
    useImport.Per73CrFee.Assign(entities.Per73CrFee);
    useImport.Per2Processed.Assign(entities.Per2Processed);
    useImport.PerDisbursementTranRlnRsn.Assign(entities.Per1);
    useImport.Per23Denied.Assign(entities.Per23Denied);
    useImport.ExpNbrOfDisbCreated.Count =
      local.CountsAndAmounts.NbrOfDisbCreated.Count;
    useImport.ExpAmtOfDisbCreated.TotalCurrency =
      local.CountsAndAmounts.AmtOfDisbCreated.TotalCurrency;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Max.Date = local.Max.Date;
    useImport.TestDisplayInd.Flag = local.TestDisplayInd.Flag;
    useExport.ImpExpToUpdate.Assign(local.ToUpdateMonthlyObligeeSummary);

    Call(FnB651ApplyDupToPotRcvObl.Execute, useImport, useExport);

    local.DatabaseUpdated.Flag = useImport.ExpDatabaseUpdated.Flag;
    local.AmtRemainingToDisburse.Amount =
      useImport.ExpAmtRemainingToDisburs.Amount;
    MoveCsePerson(useImport.PerObligeeCsePerson, entities.ObligeeCsePerson);
    entities.ObligeeCsePersonAccount.Type1 =
      useImport.PerObligeeCsePersonAccount.Type1;
    MoveDisbursementTransaction2(useImport.PerDisbursementTransaction,
      entities.DisbursementTransaction);
    entities.Per1AfCcs.SystemGeneratedIdentifier =
      useImport.Per1AfCcs.SystemGeneratedIdentifier;
    entities.Per2AfAcs.SystemGeneratedIdentifier =
      useImport.Per2AfAcs.SystemGeneratedIdentifier;
    entities.Per4NaCcs.SystemGeneratedIdentifier =
      useImport.Per4NaCcs.SystemGeneratedIdentifier;
    entities.Per5NaCcs.SystemGeneratedIdentifier =
      useImport.Per5NaAcs.SystemGeneratedIdentifier;
    entities.Per73CrFee.SystemGeneratedIdentifier =
      useImport.Per73CrFee.SystemGeneratedIdentifier;
    entities.Per2Processed.SystemGeneratedIdentifier =
      useImport.Per2Processed.SystemGeneratedIdentifier;
    entities.Per1.SystemGeneratedIdentifier =
      useImport.PerDisbursementTranRlnRsn.SystemGeneratedIdentifier;
    entities.Per23Denied.SystemGeneratedIdentifier =
      useImport.Per23Denied.SystemGeneratedIdentifier;
    local.CountsAndAmounts.NbrOfDisbCreated.Count =
      useImport.ExpNbrOfDisbCreated.Count;
    local.CountsAndAmounts.AmtOfDisbCreated.TotalCurrency =
      useImport.ExpAmtOfDisbCreated.TotalCurrency;
    local.ToUpdateMonthlyObligeeSummary.Assign(useExport.ImpExpToUpdate);
  }

  private void UseFnB651ApplySuppressionRules()
  {
    var useImport = new FnB651ApplySuppressionRules.Import();
    var useExport = new FnB651ApplySuppressionRules.Export();

    useImport.ExpDatabaseUpdated.Flag = local.DatabaseUpdated.Flag;
    useImport.PerObligeeCsePerson.Assign(entities.ObligeeCsePerson);
    useImport.PerObligeeCsePersonAccount.
      Assign(entities.ObligeeCsePersonAccount);
    useImport.PerDisbursementTransaction.
      Assign(entities.DisbursementTransaction);
    useImport.PerCollection.Assign(entities.Collection);
    useImport.PerCashReceiptType.Assign(entities.CashReceiptType);
    useImport.PerCollectionType.Assign(entities.CollectionType);
    useImport.PerDebtDetail.Assign(entities.DebtDetail);
    useImport.PerObligor.Assign(entities.ObligorCsePerson);
    useImport.PerObligationType.Assign(entities.ObligationType);
    useImport.PerSupported.Assign(entities.Supported);
    useImport.Per3Suppressed.Assign(entities.Per3Suppressed);
    useImport.UraSuppressionLength.LastUsedNumber =
      local.UraSuppressionLength.LastUsedNumber;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.TestDisplayInd.Flag = local.TestDisplayInd.Flag;
    local.ApEvent.CopyTo(useExport.ApEvent, MoveApEvent3);

    Call(FnB651ApplySuppressionRules.Execute, useImport, useExport);

    local.DatabaseUpdated.Flag = useImport.ExpDatabaseUpdated.Flag;
    MoveCsePerson(useImport.PerObligeeCsePerson, entities.ObligeeCsePerson);
    entities.ObligeeCsePersonAccount.Type1 =
      useImport.PerObligeeCsePersonAccount.Type1;
    MoveDisbursementTransaction2(useImport.PerDisbursementTransaction,
      entities.DisbursementTransaction);
    MoveCollection(useImport.PerCollection, entities.Collection);
    entities.CashReceiptType.Assign(useImport.PerCashReceiptType);
    entities.CollectionType.SequentialIdentifier =
      useImport.PerCollectionType.SequentialIdentifier;
    entities.DebtDetail.DueDt = useImport.PerDebtDetail.DueDt;
    entities.ObligorCsePerson.Number = useImport.PerObligor.Number;
    entities.ObligationType.SystemGeneratedIdentifier =
      useImport.PerObligationType.SystemGeneratedIdentifier;
    entities.Supported.Number = useImport.PerSupported.Number;
    entities.Per3Suppressed.SystemGeneratedIdentifier =
      useImport.Per3Suppressed.SystemGeneratedIdentifier;
    local.HighestSuppressionDate.Date = useExport.HighestSuppressionDate.Date;
    local.ForCreate.SuppressionReason = useExport.ForCreate.SuppressionReason;
    useExport.ApEvent.CopyTo(local.ApEvent, MoveApEvent1);
  }

  private void UseFnB651DetIfCrFeeNeeded()
  {
    var useImport = new FnB651DetIfCrFeeNeeded.Import();
    var useExport = new FnB651DetIfCrFeeNeeded.Export();

    useImport.Ar.Number = entities.ObligeeCsePerson.Number;
    useImport.Collection.CollectionDt = entities.Collection.CollectionDt;
    useImport.TestDisplay.Flag = local.TestDisplayInd.Flag;

    Call(FnB651DetIfCrFeeNeeded.Execute, useImport, useExport);

    local.TakeOutCrFee.Flag = useExport.CrFeeNeededInd.Flag;
  }

  private void UseFnB651DupPmtCheck()
  {
    var useImport = new FnB651DupPmtCheck.Import();
    var useExport = new FnB651DupPmtCheck.Export();

    useImport.PerObligee.Assign(entities.ObligeeCsePersonAccount);
    useImport.PerCashReceiptDetail.Assign(entities.CashReceiptDetail);
    useImport.PerCollection.Assign(entities.Collection);
    useImport.TestDisplayInd.Flag = local.TestDisplayInd.Flag;
    useImport.PerDisbursementTransaction.
      Assign(entities.DisbursementTransaction);
    useImport.PerCollectionType.Assign(entities.CollectionType);

    Call(FnB651DupPmtCheck.Execute, useImport, useExport);

    MoveCsePersonAccount(useImport.PerObligee, entities.ObligeeCsePersonAccount);
      
    entities.CashReceiptDetail.SequentialIdentifier =
      useImport.PerCashReceiptDetail.SequentialIdentifier;
    MoveCollection(useImport.PerCollection, entities.Collection);
    MoveDisbursementTransaction2(useImport.PerDisbursementTransaction,
      entities.DisbursementTransaction);
    entities.CollectionType.SequentialIdentifier =
      useImport.PerCollectionType.SequentialIdentifier;
    local.CheckForDupPmt.Flag = useExport.CheckForDupPmt.Flag;
    local.CollFundsAvailForDisb.TotalCurrency =
      useExport.CollFundsAvailForDisb.TotalCurrency;
    local.PrevTotArRefNbrAmt.TotalCurrency =
      useExport.PrevTotArRefNbrAmt.TotalCurrency;
  }

  private void UseFnB651Housekeeping()
  {
    var useImport = new FnB651Housekeeping.Import();
    var useExport = new FnB651Housekeeping.Export();

    useExport.Per1AfCcs.Assign(entities.Per1AfCcs);
    useExport.Per2AfAcs.Assign(entities.Per2AfAcs);
    useExport.Per4NaCcs.Assign(entities.Per4NaCcs);
    useExport.Per5NaAcs.Assign(entities.Per5NaCcs);
    useExport.Per73CrFee.Assign(entities.Per73CrFee);
    useExport.Per23Denied.Assign(entities.Per23Denied);
    useExport.Per1Released.Assign(entities.Per1Released);
    useExport.Per3Suppressed.Assign(entities.Per3Suppressed);
    useExport.Per2Processed.Assign(entities.Per2Processed);
    useExport.Per1.Assign(entities.Per1);

    Call(FnB651Housekeeping.Execute, useImport, useExport);

    local.Max.Date = useExport.Max.Date;
    entities.Per1AfCcs.SystemGeneratedIdentifier =
      useExport.Per1AfCcs.SystemGeneratedIdentifier;
    entities.Per2AfAcs.SystemGeneratedIdentifier =
      useExport.Per2AfAcs.SystemGeneratedIdentifier;
    entities.Per4NaCcs.SystemGeneratedIdentifier =
      useExport.Per4NaCcs.SystemGeneratedIdentifier;
    entities.Per5NaCcs.SystemGeneratedIdentifier =
      useExport.Per5NaAcs.SystemGeneratedIdentifier;
    entities.Per73CrFee.SystemGeneratedIdentifier =
      useExport.Per73CrFee.SystemGeneratedIdentifier;
    entities.Per23Denied.SystemGeneratedIdentifier =
      useExport.Per23Denied.SystemGeneratedIdentifier;
    local.ErrorsBeforeAbend.LastUsedNumber =
      useExport.ErrorsBeforeAbend.LastUsedNumber;
    entities.Per1Released.SystemGeneratedIdentifier =
      useExport.Per1Released.SystemGeneratedIdentifier;
    entities.Per3Suppressed.SystemGeneratedIdentifier =
      useExport.Per3Suppressed.SystemGeneratedIdentifier;
    entities.Per2Processed.SystemGeneratedIdentifier =
      useExport.Per2Processed.SystemGeneratedIdentifier;
    entities.Per1.SystemGeneratedIdentifier =
      useExport.Per1.SystemGeneratedIdentifier;
    local.TestFirstObligee.Number = useExport.TestFirstObligee.Number;
    local.TestLastObligee.Number = useExport.TestLastObligee.Number;
    local.TestDisplayInd.Flag = useExport.TestDisplayInd.Flag;
    local.TestRunInd.Flag = useExport.TestRunInd.Flag;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.UraSuppressionLength.LastUsedNumber =
      useExport.UraSuppressionLength.LastUsedNumber;
  }

  private void UseFnB651PrintDisbursement()
  {
    var useImport = new FnB651PrintDisbursement.Import();
    var useExport = new FnB651PrintDisbursement.Export();

    useImport.Ar.Number = entities.ObligeeCsePerson.Number;
    useImport.Ap.Number = entities.ObligorCsePerson.Number;
    useImport.Supported.Number = entities.Supported.Number;
    MoveDisbursementTransaction4(entities.DisbursementTransaction,
      useImport.DisbursementTransaction);
    MoveCollection(entities.Collection, useImport.Collection);
    useImport.CollectionType.SequentialIdentifier =
      entities.CollectionType.SequentialIdentifier;
    useImport.CashReceiptType.Assign(entities.CashReceiptType);
    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      entities.ObligationTransaction.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.DebtDetail.DueDt = entities.DebtDetail.DueDt;

    Call(FnB651PrintDisbursement.Execute, useImport, useExport);
  }

  private void UseFnB651PrintErrorLine()
  {
    var useImport = new FnB651PrintErrorLine.Import();
    var useExport = new FnB651PrintErrorLine.Export();

    useImport.CloseInd.Flag = local.CloseInd.Flag;

    Call(FnB651PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnB651SplitDupDisbToNet()
  {
    var useImport = new FnB651SplitDupDisbToNet.Import();
    var useExport = new FnB651SplitDupDisbToNet.Export();

    useImport.PerObligeeCsePerson.Assign(entities.ObligeeCsePerson);
    useImport.PerObligor.Assign(entities.ObligorCsePerson);
    useImport.PerSupported.Assign(entities.Supported);
    useImport.PerDisbursementTransaction.
      Assign(entities.DisbursementTransaction);
    useImport.PerCollection.Assign(entities.Collection);
    useImport.PerCollectionType.Assign(entities.CollectionType);
    useImport.PerCashReceiptType.Assign(entities.CashReceiptType);
    useImport.PerObligationType.Assign(entities.ObligationType);
    useImport.PerDebtDetail.Assign(entities.DebtDetail);
    useImport.PerObligeeCsePersonAccount.
      Assign(entities.ObligeeCsePersonAccount);
    useImport.Per1AfCcs.Assign(entities.Per1AfCcs);
    useImport.Per2AfAcs.Assign(entities.Per2AfAcs);
    useImport.Per4NaCcs.Assign(entities.Per4NaCcs);
    useImport.Per5NaAcs.Assign(entities.Per5NaCcs);
    useImport.Per73CrFee.Assign(entities.Per73CrFee);
    useImport.Per1Released.Assign(entities.Per1Released);
    useImport.Per3Suppressed.Assign(entities.Per3Suppressed);
    useImport.Per1.Assign(entities.Per1);
    useImport.ExpAmtRemainingToDisburs.Amount =
      local.AmtRemainingToDisburse.Amount;
    useImport.AmtOfDupToSplitToNet.TotalCurrency =
      local.CollFundsAvailForDisb.TotalCurrency;
    useImport.UraSuppressionLength.LastUsedNumber =
      local.UraSuppressionLength.LastUsedNumber;
    useImport.ExpDatabaseUpdated.Flag = local.DatabaseUpdated.Flag;
    useImport.TestDisplayInd.Flag = local.TestDisplayInd.Flag;
    useImport.DisbursementType.SystemGeneratedIdentifier =
      local.Determined.SystemGeneratedIdentifier;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Max.Date = local.Max.Date;
    local.ApEvent.CopyTo(useExport.ApEvent, MoveApEvent4);
    useExport.ImpExpToUpdate.Assign(local.ToUpdateMonthlyObligeeSummary);

    Call(FnB651SplitDupDisbToNet.Execute, useImport, useExport);

    MoveCsePerson(useImport.PerObligeeCsePerson, entities.ObligeeCsePerson);
    entities.ObligorCsePerson.Number = useImport.PerObligor.Number;
    entities.Supported.Number = useImport.PerSupported.Number;
    MoveDisbursementTransaction2(useImport.PerDisbursementTransaction,
      entities.DisbursementTransaction);
    MoveCollection(useImport.PerCollection, entities.Collection);
    entities.CollectionType.SequentialIdentifier =
      useImport.PerCollectionType.SequentialIdentifier;
    entities.CashReceiptType.Assign(useImport.PerCashReceiptType);
    entities.ObligationType.SystemGeneratedIdentifier =
      useImport.PerObligationType.SystemGeneratedIdentifier;
    entities.DebtDetail.DueDt = useImport.PerDebtDetail.DueDt;
    entities.ObligeeCsePersonAccount.Type1 =
      useImport.PerObligeeCsePersonAccount.Type1;
    entities.Per1AfCcs.SystemGeneratedIdentifier =
      useImport.Per1AfCcs.SystemGeneratedIdentifier;
    entities.Per2AfAcs.SystemGeneratedIdentifier =
      useImport.Per2AfAcs.SystemGeneratedIdentifier;
    entities.Per4NaCcs.SystemGeneratedIdentifier =
      useImport.Per4NaCcs.SystemGeneratedIdentifier;
    entities.Per5NaCcs.SystemGeneratedIdentifier =
      useImport.Per5NaAcs.SystemGeneratedIdentifier;
    entities.Per73CrFee.SystemGeneratedIdentifier =
      useImport.Per73CrFee.SystemGeneratedIdentifier;
    entities.Per1Released.SystemGeneratedIdentifier =
      useImport.Per1Released.SystemGeneratedIdentifier;
    entities.Per3Suppressed.SystemGeneratedIdentifier =
      useImport.Per3Suppressed.SystemGeneratedIdentifier;
    entities.Per1.SystemGeneratedIdentifier =
      useImport.Per1.SystemGeneratedIdentifier;
    local.AmtRemainingToDisburse.Amount =
      useImport.ExpAmtRemainingToDisburs.Amount;
    local.DatabaseUpdated.Flag = useImport.ExpDatabaseUpdated.Flag;
    useExport.ApEvent.CopyTo(local.ApEvent, MoveApEvent2);
    local.ToUpdateMonthlyObligeeSummary.Assign(useExport.ImpExpToUpdate);
  }

  private void UseFnB651WriteTotals()
  {
    var useImport = new FnB651WriteTotals.Import();
    var useExport = new FnB651WriteTotals.Export();

    MoveCountsAndAmounts2(local.CountsAndAmounts, useExport.CountsAndAmounts);
    MoveSupprCounts2(local.SupprCounts, useExport.SupprCounts);

    Call(FnB651WriteTotals.Execute, useImport, useExport);

    MoveCountsAndAmounts1(useExport.CountsAndAmounts, local.CountsAndAmounts);
    MoveSupprCounts3(useExport.SupprCounts, local.SupprCounts);
  }

  private void UseFnCreateDisbursementNew()
  {
    var useImport = new FnCreateDisbursementNew.Import();
    var useExport = new FnCreateDisbursementNew.Export();

    useImport.PerCredit.Assign(entities.DisbursementTransaction);
    useImport.PerObligee.Assign(entities.ObligeeCsePersonAccount);
    useImport.Per1AfCcs.Assign(entities.Per1AfCcs);
    useImport.Per2AfAcs.Assign(entities.Per2AfAcs);
    useImport.Per4NaCcs.Assign(entities.Per4NaCcs);
    useImport.Per5NaAcs.Assign(entities.Per5NaCcs);
    useImport.Per73CrFee.Assign(entities.Per73CrFee);
    useImport.Per1.Assign(entities.Per1);
    useImport.HighestSuppressionDate.Date = local.HighestSuppressionDate.Date;
    useImport.New1.Assign(local.WorkingDisbursement);
    useImport.DisbursementType.SystemGeneratedIdentifier =
      local.Determined.SystemGeneratedIdentifier;
    useImport.Max.Date = local.Max.Date;
    useImport.Per1Released.Assign(entities.Per1Released);
    useImport.Per2Processed.Assign(entities.Per2Processed);
    useImport.Per3Suppressed.Assign(entities.Per3Suppressed);
    useImport.ForCreate.SuppressionReason = local.ForCreate.SuppressionReason;
    useImport.ExpDatabaseUpdated.Flag = local.DatabaseUpdated.Flag;
    useImport.TestDisplayInd.Flag = local.TestDisplayInd.Flag;
    useImport.DisbursementStatus.SystemGeneratedIdentifier =
      local.DisbursementStatus.SystemGeneratedIdentifier;

    Call(FnCreateDisbursementNew.Execute, useImport, useExport);

    MoveDisbursementTransaction(useImport.PerCredit,
      entities.DisbursementTransaction);
    entities.ObligeeCsePersonAccount.Type1 = useImport.PerObligee.Type1;
    entities.Per1AfCcs.SystemGeneratedIdentifier =
      useImport.Per1AfCcs.SystemGeneratedIdentifier;
    entities.Per2AfAcs.SystemGeneratedIdentifier =
      useImport.Per2AfAcs.SystemGeneratedIdentifier;
    entities.Per4NaCcs.SystemGeneratedIdentifier =
      useImport.Per4NaCcs.SystemGeneratedIdentifier;
    entities.Per5NaCcs.SystemGeneratedIdentifier =
      useImport.Per5NaAcs.SystemGeneratedIdentifier;
    entities.Per73CrFee.SystemGeneratedIdentifier =
      useImport.Per73CrFee.SystemGeneratedIdentifier;
    entities.Per1.SystemGeneratedIdentifier =
      useImport.Per1.SystemGeneratedIdentifier;
    entities.Per1Released.SystemGeneratedIdentifier =
      useImport.Per1Released.SystemGeneratedIdentifier;
    entities.Per2Processed.SystemGeneratedIdentifier =
      useImport.Per2Processed.SystemGeneratedIdentifier;
    entities.Per3Suppressed.SystemGeneratedIdentifier =
      useImport.Per3Suppressed.SystemGeneratedIdentifier;
    local.DatabaseUpdated.Flag = useImport.ExpDatabaseUpdated.Flag;
    local.NewForPrint.SystemGeneratedIdentifier =
      useExport.New1.SystemGeneratedIdentifier;
  }

  private void UseFnDetermineDisbType()
  {
    var useImport = new FnDetermineDisbType.Import();
    var useExport = new FnDetermineDisbType.Export();

    useImport.Per.Assign(entities.Collection);
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;

    Call(FnDetermineDisbType.Execute, useImport, useExport);

    local.Determined.SystemGeneratedIdentifier =
      useExport.DisbursementType.SystemGeneratedIdentifier;
  }

  private void UseFnReleaseSuppressedDisbursemt()
  {
    var useImport = new FnReleaseSuppressedDisbursemt.Import();
    var useExport = new FnReleaseSuppressedDisbursemt.Export();

    useImport.TestDisplayInd.Flag = local.TestDisplayInd.Flag;
    useImport.MaxDate.Date = local.Max.Date;
    useImport.HardcodedPerson.Type1 = local.HardcodedPerson.Type1;
    useImport.DisbSuppressionStatusHistory.Type1 =
      local.HardcodedCollectonType.Type1;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.PerReleased.Assign(entities.Per1Released);
    useImport.PerSuppressed.Assign(entities.Per3Suppressed);
    useImport.TestFirst.Number = local.TestFirstObligee.Number;
    useImport.TestLast.Number = local.TestLastObligee.Number;
    useImport.UraSuppressionLength.LastUsedNumber =
      local.UraSuppressionLength.LastUsedNumber;

    Call(FnReleaseSuppressedDisbursemt.Execute, useImport, useExport);

    entities.Per1Released.SystemGeneratedIdentifier =
      useImport.PerReleased.SystemGeneratedIdentifier;
    entities.Per3Suppressed.SystemGeneratedIdentifier =
      useImport.PerSuppressed.SystemGeneratedIdentifier;
    MoveSupprCounts1(useExport.SupprCounts, local.SupprCounts);
  }

  private void UseFnUpdateObligeeMonthlyTotals()
  {
    var useImport = new FnUpdateObligeeMonthlyTotals.Import();
    var useExport = new FnUpdateObligeeMonthlyTotals.Export();

    MoveMonthlyObligeeSummary(local.ToUpdateMonthlyObligeeSummary,
      useImport.MonthlyObligeeSummary);
    useImport.Per.Assign(entities.PreviousObligeeCsePersonAccount);

    Call(FnUpdateObligeeMonthlyTotals.Execute, useImport, useExport);

    MoveCsePersonAccount(useImport.Per, entities.PreviousObligeeCsePersonAccount);
      
  }

  private void UseFnUpdateObligeeTotals()
  {
    var useImport = new FnUpdateObligeeTotals.Import();
    var useExport = new FnUpdateObligeeTotals.Export();

    useImport.CsePerson.Number = local.HoldObligee.Number;
    useImport.Obligee.Assign(local.ToUpdateObligee);

    Call(FnUpdateObligeeTotals.Execute, useImport, useExport);
  }

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private bool ReadCsePersonAccountCsePerson()
  {
    entities.PreviousObligeeCsePerson.Populated = false;
    entities.PreviousObligeeCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccountCsePerson",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.HoldObligee.Number);
      },
      (db, reader) =>
      {
        entities.PreviousObligeeCsePersonAccount.CspNumber =
          db.GetString(reader, 0);
        entities.PreviousObligeeCsePerson.Number = db.GetString(reader, 0);
        entities.PreviousObligeeCsePersonAccount.Type1 =
          db.GetString(reader, 1);
        entities.PreviousObligeeCsePerson.Populated = true;
        entities.PreviousObligeeCsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.PreviousObligeeCsePersonAccount.Type1);
      });
  }

  private IEnumerable<bool>
    ReadDisbursementTransactionCsePersonAccountCsePerson()
  {
    entities.ObligeeCsePerson.Populated = false;
    entities.ObligorCsePerson.Populated = false;
    entities.Supported.Populated = false;
    entities.DisbursementTransaction.Populated = false;
    entities.Collection.Populated = false;
    entities.CollectionType.Populated = false;
    entities.CashReceiptType.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.ObligationType.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.ObligeeCsePersonAccount.Populated = false;

    return ReadEach("ReadDisbursementTransactionCsePersonAccountCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "processDate",
          local.InitializedDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.ObligeeCsePersonAccount.Type1 = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligeeCsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTransaction.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 9);
        entities.DisbursementTransaction.CollectionProcessDate =
          db.GetDate(reader, 10);
        entities.DisbursementTransaction.OtyId =
          db.GetNullableInt32(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 11);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 11);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 11);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 11);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 11);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 11);
        entities.DisbursementTransaction.OtrTypeDisb =
          db.GetNullableString(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 12);
        entities.DebtDetail.OtrType = db.GetString(reader, 12);
        entities.DisbursementTransaction.OtrId =
          db.GetNullableInt32(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 13);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 13);
        entities.DisbursementTransaction.CpaTypeDisb =
          db.GetNullableString(reader, 14);
        entities.Collection.CpaType = db.GetString(reader, 14);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 14);
        entities.Obligation.CpaType = db.GetString(reader, 14);
        entities.Obligation.CpaType = db.GetString(reader, 14);
        entities.DebtDetail.CpaType = db.GetString(reader, 14);
        entities.DebtDetail.CpaType = db.GetString(reader, 14);
        entities.DisbursementTransaction.CspNumberDisb =
          db.GetNullableString(reader, 15);
        entities.Collection.CspNumber = db.GetString(reader, 15);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 15);
        entities.Obligation.CspNumber = db.GetString(reader, 15);
        entities.Obligation.CspNumber = db.GetString(reader, 15);
        entities.DebtDetail.CspNumber = db.GetString(reader, 15);
        entities.DebtDetail.CspNumber = db.GetString(reader, 15);
        entities.ObligorCsePerson.Number = db.GetString(reader, 15);
        entities.ObligorCsePerson.Number = db.GetString(reader, 15);
        entities.ObligorCsePerson.Number = db.GetString(reader, 15);
        entities.ObligorCsePerson.Number = db.GetString(reader, 15);
        entities.DisbursementTransaction.ObgId =
          db.GetNullableInt32(reader, 16);
        entities.Collection.ObgId = db.GetInt32(reader, 16);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 16);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 16);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 16);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 16);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 16);
        entities.DisbursementTransaction.CrdId =
          db.GetNullableInt32(reader, 17);
        entities.Collection.CrdId = db.GetInt32(reader, 17);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 17);
        entities.DisbursementTransaction.CrvId =
          db.GetNullableInt32(reader, 18);
        entities.Collection.CrvId = db.GetInt32(reader, 18);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 18);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 18);
        entities.DisbursementTransaction.CstId =
          db.GetNullableInt32(reader, 19);
        entities.Collection.CstId = db.GetInt32(reader, 19);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 19);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 19);
        entities.DisbursementTransaction.CrtId =
          db.GetNullableInt32(reader, 20);
        entities.Collection.CrtType = db.GetInt32(reader, 20);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 20);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 20);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 20);
        entities.DisbursementTransaction.ColId =
          db.GetNullableInt32(reader, 21);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 21);
        entities.DisbursementTransaction.InterstateInd =
          db.GetNullableString(reader, 22);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 23);
        entities.DisbursementTransaction.IntInterId =
          db.GetNullableInt32(reader, 24);
        entities.DisbursementTransaction.ExcessUraInd =
          db.GetNullableString(reader, 25);
        entities.ObligeeCsePerson.Number = db.GetString(reader, 26);
        entities.ObligeeCsePerson.Type1 = db.GetString(reader, 27);
        entities.Collection.AppliedToCode = db.GetString(reader, 28);
        entities.Collection.CollectionDt = db.GetDate(reader, 29);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 30);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 31);
        entities.Collection.AppliedToFuture = db.GetString(reader, 32);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 33);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 34);
        entities.Supported.Number = db.GetString(reader, 34);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 35);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 36);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 36);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 37);
        entities.CashReceiptType.Code = db.GetString(reader, 38);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 39);
        entities.DebtDetail.DueDt = db.GetDate(reader, 40);
        entities.ObligeeCsePerson.Populated = true;
        entities.ObligorCsePerson.Populated = true;
        entities.Supported.Populated = true;
        entities.DisbursementTransaction.Populated = true;
        entities.Collection.Populated = true;
        entities.CollectionType.Populated = true;
        entities.CashReceiptType.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.ObligeeCsePersonAccount.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<CsePersonAccount>("Type1",
          entities.ObligeeCsePersonAccount.Type1);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.DisbursementTransaction.OtrTypeDisb);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.DisbursementTransaction.CpaTypeDisb);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<CsePerson>("Type1", entities.ObligeeCsePerson.Type1);
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToFuture",
          entities.Collection.AppliedToFuture);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);

        return true;
      });
  }

  private bool ReadDisbursementType()
  {
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTypeId", local.Determined.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 1);
        entities.DisbursementType.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.DisbursementTransaction.IntInterId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 1);
        entities.InterstateRequest.Populated = true;
      });
  }

  private void UpdateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var processDate = local.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = Now();

    entities.DisbursementTransaction.Populated = false;
    Update("UpdateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableDate(command, "processDate", processDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
      });

    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.LastUpdatedBy = lastUpdatedBy;
    entities.DisbursementTransaction.LastUpdateTmst = lastUpdateTmst;
    entities.DisbursementTransaction.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ApEventGroup group.</summary>
    [Serializable]
    public class ApEventGroup
    {
      /// <summary>
      /// A value of ApGrpDtl.
      /// </summary>
      [JsonPropertyName("apGrpDtl")]
      public CsePerson ApGrpDtl
      {
        get => apGrpDtl ??= new();
        set => apGrpDtl = value;
      }

      /// <summary>
      /// A value of RegUraGrpDtl.
      /// </summary>
      [JsonPropertyName("regUraGrpDtl")]
      public Common RegUraGrpDtl
      {
        get => regUraGrpDtl ??= new();
        set => regUraGrpDtl = value;
      }

      /// <summary>
      /// A value of RegUraAdjGrpDtl.
      /// </summary>
      [JsonPropertyName("regUraAdjGrpDtl")]
      public Common RegUraAdjGrpDtl
      {
        get => regUraAdjGrpDtl ??= new();
        set => regUraAdjGrpDtl = value;
      }

      /// <summary>
      /// A value of MedUraGrpDtl.
      /// </summary>
      [JsonPropertyName("medUraGrpDtl")]
      public Common MedUraGrpDtl
      {
        get => medUraGrpDtl ??= new();
        set => medUraGrpDtl = value;
      }

      /// <summary>
      /// A value of MedUraAdjGrpDtl.
      /// </summary>
      [JsonPropertyName("medUraAdjGrpDtl")]
      public Common MedUraAdjGrpDtl
      {
        get => medUraAdjGrpDtl ??= new();
        set => medUraAdjGrpDtl = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson apGrpDtl;
      private Common regUraGrpDtl;
      private Common regUraAdjGrpDtl;
      private Common medUraGrpDtl;
      private Common medUraAdjGrpDtl;
    }

    /// <summary>A CountsAndAmountsGroup group.</summary>
    [Serializable]
    public class CountsAndAmountsGroup
    {
      /// <summary>
      /// A value of NbrOfDisbRead.
      /// </summary>
      [JsonPropertyName("nbrOfDisbRead")]
      public Common NbrOfDisbRead
      {
        get => nbrOfDisbRead ??= new();
        set => nbrOfDisbRead = value;
      }

      /// <summary>
      /// A value of NbrOfDisbCreated.
      /// </summary>
      [JsonPropertyName("nbrOfDisbCreated")]
      public Common NbrOfDisbCreated
      {
        get => nbrOfDisbCreated ??= new();
        set => nbrOfDisbCreated = value;
      }

      /// <summary>
      /// A value of NbrOfErrors.
      /// </summary>
      [JsonPropertyName("nbrOfErrors")]
      public Common NbrOfErrors
      {
        get => nbrOfErrors ??= new();
        set => nbrOfErrors = value;
      }

      /// <summary>
      /// A value of NbrOfDupChecks.
      /// </summary>
      [JsonPropertyName("nbrOfDupChecks")]
      public Common NbrOfDupChecks
      {
        get => nbrOfDupChecks ??= new();
        set => nbrOfDupChecks = value;
      }

      /// <summary>
      /// A value of AmtOfDisbRead.
      /// </summary>
      [JsonPropertyName("amtOfDisbRead")]
      public Common AmtOfDisbRead
      {
        get => amtOfDisbRead ??= new();
        set => amtOfDisbRead = value;
      }

      /// <summary>
      /// A value of AmtOfDisbCreated.
      /// </summary>
      [JsonPropertyName("amtOfDisbCreated")]
      public Common AmtOfDisbCreated
      {
        get => amtOfDisbCreated ??= new();
        set => amtOfDisbCreated = value;
      }

      /// <summary>
      /// A value of AmtOfErrors.
      /// </summary>
      [JsonPropertyName("amtOfErrors")]
      public Common AmtOfErrors
      {
        get => amtOfErrors ??= new();
        set => amtOfErrors = value;
      }

      private Common nbrOfDisbRead;
      private Common nbrOfDisbCreated;
      private Common nbrOfErrors;
      private Common nbrOfDupChecks;
      private Common amtOfDisbRead;
      private Common amtOfDisbCreated;
      private Common amtOfErrors;
    }

    /// <summary>A SupprCountsGroup group.</summary>
    [Serializable]
    public class SupprCountsGroup
    {
      /// <summary>
      /// A value of DtlSupprRead.
      /// </summary>
      [JsonPropertyName("dtlSupprRead")]
      public Common DtlSupprRead
      {
        get => dtlSupprRead ??= new();
        set => dtlSupprRead = value;
      }

      /// <summary>
      /// A value of DtlSupprReleased.
      /// </summary>
      [JsonPropertyName("dtlSupprReleased")]
      public Common DtlSupprReleased
      {
        get => dtlSupprReleased ??= new();
        set => dtlSupprReleased = value;
      }

      /// <summary>
      /// A value of DtlSupprExtended.
      /// </summary>
      [JsonPropertyName("dtlSupprExtended")]
      public Common DtlSupprExtended
      {
        get => dtlSupprExtended ??= new();
        set => dtlSupprExtended = value;
      }

      /// <summary>
      /// A value of DtlSupprReduced.
      /// </summary>
      [JsonPropertyName("dtlSupprReduced")]
      public Common DtlSupprReduced
      {
        get => dtlSupprReduced ??= new();
        set => dtlSupprReduced = value;
      }

      /// <summary>
      /// A value of DtlSupprNoChange.
      /// </summary>
      [JsonPropertyName("dtlSupprNoChange")]
      public Common DtlSupprNoChange
      {
        get => dtlSupprNoChange ??= new();
        set => dtlSupprNoChange = value;
      }

      /// <summary>
      /// A value of DtlPSuppr.
      /// </summary>
      [JsonPropertyName("dtlPSuppr")]
      public Common DtlPSuppr
      {
        get => dtlPSuppr ??= new();
        set => dtlPSuppr = value;
      }

      /// <summary>
      /// A value of DtlCSuppr.
      /// </summary>
      [JsonPropertyName("dtlCSuppr")]
      public Common DtlCSuppr
      {
        get => dtlCSuppr ??= new();
        set => dtlCSuppr = value;
      }

      /// <summary>
      /// A value of DtlASuppr.
      /// </summary>
      [JsonPropertyName("dtlASuppr")]
      public Common DtlASuppr
      {
        get => dtlASuppr ??= new();
        set => dtlASuppr = value;
      }

      /// <summary>
      /// A value of DtlXSuppr.
      /// </summary>
      [JsonPropertyName("dtlXSuppr")]
      public Common DtlXSuppr
      {
        get => dtlXSuppr ??= new();
        set => dtlXSuppr = value;
      }

      /// <summary>
      /// A value of DtlDSuppr.
      /// </summary>
      [JsonPropertyName("dtlDSuppr")]
      public Common DtlDSuppr
      {
        get => dtlDSuppr ??= new();
        set => dtlDSuppr = value;
      }

      /// <summary>
      /// A value of DtlPSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlPSupprAmt")]
      public Common DtlPSupprAmt
      {
        get => dtlPSupprAmt ??= new();
        set => dtlPSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlCSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlCSupprAmt")]
      public Common DtlCSupprAmt
      {
        get => dtlCSupprAmt ??= new();
        set => dtlCSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlASupprAmt.
      /// </summary>
      [JsonPropertyName("dtlASupprAmt")]
      public Common DtlASupprAmt
      {
        get => dtlASupprAmt ??= new();
        set => dtlASupprAmt = value;
      }

      /// <summary>
      /// A value of DtlXSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlXSupprAmt")]
      public Common DtlXSupprAmt
      {
        get => dtlXSupprAmt ??= new();
        set => dtlXSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlDSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlDSupprAmt")]
      public Common DtlDSupprAmt
      {
        get => dtlDSupprAmt ??= new();
        set => dtlDSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlYSuppr.
      /// </summary>
      [JsonPropertyName("dtlYSuppr")]
      public Common DtlYSuppr
      {
        get => dtlYSuppr ??= new();
        set => dtlYSuppr = value;
      }

      /// <summary>
      /// A value of DtlZSuppr.
      /// </summary>
      [JsonPropertyName("dtlZSuppr")]
      public Common DtlZSuppr
      {
        get => dtlZSuppr ??= new();
        set => dtlZSuppr = value;
      }

      /// <summary>
      /// A value of DtlYSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlYSupprAmt")]
      public Common DtlYSupprAmt
      {
        get => dtlYSupprAmt ??= new();
        set => dtlYSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlZSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlZSupprAmt")]
      public Common DtlZSupprAmt
      {
        get => dtlZSupprAmt ??= new();
        set => dtlZSupprAmt = value;
      }

      private Common dtlSupprRead;
      private Common dtlSupprReleased;
      private Common dtlSupprExtended;
      private Common dtlSupprReduced;
      private Common dtlSupprNoChange;
      private Common dtlPSuppr;
      private Common dtlCSuppr;
      private Common dtlASuppr;
      private Common dtlXSuppr;
      private Common dtlDSuppr;
      private Common dtlPSupprAmt;
      private Common dtlCSupprAmt;
      private Common dtlASupprAmt;
      private Common dtlXSupprAmt;
      private Common dtlDSupprAmt;
      private Common dtlYSuppr;
      private Common dtlZSuppr;
      private Common dtlYSupprAmt;
      private Common dtlZSupprAmt;
    }

    /// <summary>
    /// A value of CloseInd.
    /// </summary>
    [JsonPropertyName("closeInd")]
    public Common CloseInd
    {
      get => closeInd ??= new();
      set => closeInd = value;
    }

    /// <summary>
    /// A value of AmtRemainingToDisburse.
    /// </summary>
    [JsonPropertyName("amtRemainingToDisburse")]
    public DisbursementTransaction AmtRemainingToDisburse
    {
      get => amtRemainingToDisburse ??= new();
      set => amtRemainingToDisburse = value;
    }

    /// <summary>
    /// A value of CollFundsAvailForDisb.
    /// </summary>
    [JsonPropertyName("collFundsAvailForDisb")]
    public Common CollFundsAvailForDisb
    {
      get => collFundsAvailForDisb ??= new();
      set => collFundsAvailForDisb = value;
    }

    /// <summary>
    /// A value of CrFeeTaken.
    /// </summary>
    [JsonPropertyName("crFeeTaken")]
    public Common CrFeeTaken
    {
      get => crFeeTaken ??= new();
      set => crFeeTaken = value;
    }

    /// <summary>
    /// A value of NewForPrint.
    /// </summary>
    [JsonPropertyName("newForPrint")]
    public DisbursementTransaction NewForPrint
    {
      get => newForPrint ??= new();
      set => newForPrint = value;
    }

    /// <summary>
    /// A value of SplitDisbSupprOrReleased.
    /// </summary>
    [JsonPropertyName("splitDisbSupprOrReleased")]
    public DisbursementStatus SplitDisbSupprOrReleased
    {
      get => splitDisbSupprOrReleased ??= new();
      set => splitDisbSupprOrReleased = value;
    }

    /// <summary>
    /// A value of CurrTotArRefNbrAmt.
    /// </summary>
    [JsonPropertyName("currTotArRefNbrAmt")]
    public Common CurrTotArRefNbrAmt
    {
      get => currTotArRefNbrAmt ??= new();
      set => currTotArRefNbrAmt = value;
    }

    /// <summary>
    /// A value of PrevTotArRefNbrAmt.
    /// </summary>
    [JsonPropertyName("prevTotArRefNbrAmt")]
    public Common PrevTotArRefNbrAmt
    {
      get => prevTotArRefNbrAmt ??= new();
      set => prevTotArRefNbrAmt = value;
    }

    /// <summary>
    /// A value of Sign1.
    /// </summary>
    [JsonPropertyName("sign1")]
    public TextWorkArea Sign1
    {
      get => sign1 ??= new();
      set => sign1 = value;
    }

    /// <summary>
    /// A value of Sign2.
    /// </summary>
    [JsonPropertyName("sign2")]
    public TextWorkArea Sign2
    {
      get => sign2 ??= new();
      set => sign2 = value;
    }

    /// <summary>
    /// A value of TestMsg.
    /// </summary>
    [JsonPropertyName("testMsg")]
    public WorkArea TestMsg
    {
      get => testMsg ??= new();
      set => testMsg = value;
    }

    /// <summary>
    /// A value of NumericDisbAmt.
    /// </summary>
    [JsonPropertyName("numericDisbAmt")]
    public NumericWorkSet NumericDisbAmt
    {
      get => numericDisbAmt ??= new();
      set => numericDisbAmt = value;
    }

    /// <summary>
    /// A value of NumericFundsAvailForDisb.
    /// </summary>
    [JsonPropertyName("numericFundsAvailForDisb")]
    public NumericWorkSet NumericFundsAvailForDisb
    {
      get => numericFundsAvailForDisb ??= new();
      set => numericFundsAvailForDisb = value;
    }

    /// <summary>
    /// A value of NumericCollAmt.
    /// </summary>
    [JsonPropertyName("numericCollAmt")]
    public NumericWorkSet NumericCollAmt
    {
      get => numericCollAmt ??= new();
      set => numericCollAmt = value;
    }

    /// <summary>
    /// A value of TextDisbAmt.
    /// </summary>
    [JsonPropertyName("textDisbAmt")]
    public WorkArea TextDisbAmt
    {
      get => textDisbAmt ??= new();
      set => textDisbAmt = value;
    }

    /// <summary>
    /// A value of TextFundsAvailForDisb.
    /// </summary>
    [JsonPropertyName("textFundsAvailForDisb")]
    public WorkArea TextFundsAvailForDisb
    {
      get => textFundsAvailForDisb ??= new();
      set => textFundsAvailForDisb = value;
    }

    /// <summary>
    /// A value of TextCollAmt.
    /// </summary>
    [JsonPropertyName("textCollAmt")]
    public WorkArea TextCollAmt
    {
      get => textCollAmt ??= new();
      set => textCollAmt = value;
    }

    /// <summary>
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public DisbursementStatusHistory ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
    }

    /// <summary>
    /// A value of ForDupPmtCheck.
    /// </summary>
    [JsonPropertyName("forDupPmtCheck")]
    public CashReceiptDetail ForDupPmtCheck
    {
      get => forDupPmtCheck ??= new();
      set => forDupPmtCheck = value;
    }

    /// <summary>
    /// A value of HoldObligee.
    /// </summary>
    [JsonPropertyName("holdObligee")]
    public CsePerson HoldObligee
    {
      get => holdObligee ??= new();
      set => holdObligee = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public DisbursementTransaction Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of CheckForDupPmt.
    /// </summary>
    [JsonPropertyName("checkForDupPmt")]
    public Common CheckForDupPmt
    {
      get => checkForDupPmt ??= new();
      set => checkForDupPmt = value;
    }

    /// <summary>
    /// Gets a value of ApEvent.
    /// </summary>
    [JsonIgnore]
    public Array<ApEventGroup> ApEvent => apEvent ??= new(
      ApEventGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ApEvent for json serialization.
    /// </summary>
    [JsonPropertyName("apEvent")]
    [Computed]
    public IList<ApEventGroup> ApEvent_Json
    {
      get => apEvent;
      set => ApEvent.Assign(value);
    }

    /// <summary>
    /// A value of UraSuppressionLength.
    /// </summary>
    [JsonPropertyName("uraSuppressionLength")]
    public ControlTable UraSuppressionLength
    {
      get => uraSuppressionLength ??= new();
      set => uraSuppressionLength = value;
    }

    /// <summary>
    /// A value of ErrorsBeforeAbend.
    /// </summary>
    [JsonPropertyName("errorsBeforeAbend")]
    public ControlTable ErrorsBeforeAbend
    {
      get => errorsBeforeAbend ??= new();
      set => errorsBeforeAbend = value;
    }

    /// <summary>
    /// A value of DatabaseUpdated.
    /// </summary>
    [JsonPropertyName("databaseUpdated")]
    public Common DatabaseUpdated
    {
      get => databaseUpdated ??= new();
      set => databaseUpdated = value;
    }

    /// <summary>
    /// A value of Rollback.
    /// </summary>
    [JsonPropertyName("rollback")]
    public Common Rollback
    {
      get => rollback ??= new();
      set => rollback = value;
    }

    /// <summary>
    /// A value of HighestSuppressionDate.
    /// </summary>
    [JsonPropertyName("highestSuppressionDate")]
    public DateWorkArea HighestSuppressionDate
    {
      get => highestSuppressionDate ??= new();
      set => highestSuppressionDate = value;
    }

    /// <summary>
    /// A value of NbrSuprDisbForArAndCr.
    /// </summary>
    [JsonPropertyName("nbrSuprDisbForArAndCr")]
    public Common NbrSuprDisbForArAndCr
    {
      get => nbrSuprDisbForArAndCr ??= new();
      set => nbrSuprDisbForArAndCr = value;
    }

    /// <summary>
    /// A value of TestFirstObligee.
    /// </summary>
    [JsonPropertyName("testFirstObligee")]
    public CsePerson TestFirstObligee
    {
      get => testFirstObligee ??= new();
      set => testFirstObligee = value;
    }

    /// <summary>
    /// A value of TestLastObligee.
    /// </summary>
    [JsonPropertyName("testLastObligee")]
    public CsePerson TestLastObligee
    {
      get => testLastObligee ??= new();
      set => testLastObligee = value;
    }

    /// <summary>
    /// A value of TestDisplayInd.
    /// </summary>
    [JsonPropertyName("testDisplayInd")]
    public Common TestDisplayInd
    {
      get => testDisplayInd ??= new();
      set => testDisplayInd = value;
    }

    /// <summary>
    /// A value of TestRunInd.
    /// </summary>
    [JsonPropertyName("testRunInd")]
    public Common TestRunInd
    {
      get => testRunInd ??= new();
      set => testRunInd = value;
    }

    /// <summary>
    /// A value of CountErrorsForAbend.
    /// </summary>
    [JsonPropertyName("countErrorsForAbend")]
    public Common CountErrorsForAbend
    {
      get => countErrorsForAbend ??= new();
      set => countErrorsForAbend = value;
    }

    /// <summary>
    /// A value of ProgramCounter.
    /// </summary>
    [JsonPropertyName("programCounter")]
    public Common ProgramCounter
    {
      get => programCounter ??= new();
      set => programCounter = value;
    }

    /// <summary>
    /// A value of TakeOutCrFee.
    /// </summary>
    [JsonPropertyName("takeOutCrFee")]
    public Common TakeOutCrFee
    {
      get => takeOutCrFee ??= new();
      set => takeOutCrFee = value;
    }

    /// <summary>
    /// A value of HardcodedAutomatic.
    /// </summary>
    [JsonPropertyName("hardcodedAutomatic")]
    public DisbSuppressionStatusHistory HardcodedAutomatic
    {
      get => hardcodedAutomatic ??= new();
      set => hardcodedAutomatic = value;
    }

    /// <summary>
    /// A value of HardcodedGift.
    /// </summary>
    [JsonPropertyName("hardcodedGift")]
    public DisbursementType HardcodedGift
    {
      get => hardcodedGift ??= new();
      set => hardcodedGift = value;
    }

    /// <summary>
    /// A value of ProcessCountToCommit.
    /// </summary>
    [JsonPropertyName("processCountToCommit")]
    public Common ProcessCountToCommit
    {
      get => processCountToCommit ??= new();
      set => processCountToCommit = value;
    }

    /// <summary>
    /// A value of HardcodedMs.
    /// </summary>
    [JsonPropertyName("hardcodedMs")]
    public ObligationType HardcodedMs
    {
      get => hardcodedMs ??= new();
      set => hardcodedMs = value;
    }

    /// <summary>
    /// A value of ToInitializeMonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("toInitializeMonthlyObligeeSummary")]
    public MonthlyObligeeSummary ToInitializeMonthlyObligeeSummary
    {
      get => toInitializeMonthlyObligeeSummary ??= new();
      set => toInitializeMonthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of ToInitializeObligee.
    /// </summary>
    [JsonPropertyName("toInitializeObligee")]
    public CsePersonAccount ToInitializeObligee
    {
      get => toInitializeObligee ??= new();
      set => toInitializeObligee = value;
    }

    /// <summary>
    /// A value of ToUpdateObligee.
    /// </summary>
    [JsonPropertyName("toUpdateObligee")]
    public CsePersonAccount ToUpdateObligee
    {
      get => toUpdateObligee ??= new();
      set => toUpdateObligee = value;
    }

    /// <summary>
    /// A value of ToUpdateMonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("toUpdateMonthlyObligeeSummary")]
    public MonthlyObligeeSummary ToUpdateMonthlyObligeeSummary
    {
      get => toUpdateMonthlyObligeeSummary ??= new();
      set => toUpdateMonthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of WorkingDisbursement.
    /// </summary>
    [JsonPropertyName("workingDisbursement")]
    public DisbursementTransaction WorkingDisbursement
    {
      get => workingDisbursement ??= new();
      set => workingDisbursement = value;
    }

    /// <summary>
    /// A value of Determined.
    /// </summary>
    [JsonPropertyName("determined")]
    public DisbursementType Determined
    {
      get => determined ??= new();
      set => determined = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    /// <summary>
    /// A value of AdcProgram.
    /// </summary>
    [JsonPropertyName("adcProgram")]
    public Common AdcProgram
    {
      get => adcProgram ??= new();
      set => adcProgram = value;
    }

    /// <summary>
    /// A value of Hardcoded.
    /// </summary>
    [JsonPropertyName("hardcoded")]
    public DisbursementTranRlnRsn Hardcoded
    {
      get => hardcoded ??= new();
      set => hardcoded = value;
    }

    /// <summary>
    /// A value of HardcodedPerson.
    /// </summary>
    [JsonPropertyName("hardcodedPerson")]
    public DisbSuppressionStatusHistory HardcodedPerson
    {
      get => hardcodedPerson ??= new();
      set => hardcodedPerson = value;
    }

    /// <summary>
    /// A value of HardcodedCollectonType.
    /// </summary>
    [JsonPropertyName("hardcodedCollectonType")]
    public DisbSuppressionStatusHistory HardcodedCollectonType
    {
      get => hardcodedCollectonType ??= new();
      set => hardcodedCollectonType = value;
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
    /// A value of InitializedDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("initializedDisbursementTransaction")]
    public DisbursementTransaction InitializedDisbursementTransaction
    {
      get => initializedDisbursementTransaction ??= new();
      set => initializedDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of NumberOfUpdates.
    /// </summary>
    [JsonPropertyName("numberOfUpdates")]
    public Common NumberOfUpdates
    {
      get => numberOfUpdates ??= new();
      set => numberOfUpdates = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of ExitStateMessage.
    /// </summary>
    [JsonPropertyName("exitStateMessage")]
    public ExitStateWorkArea ExitStateMessage
    {
      get => exitStateMessage ??= new();
      set => exitStateMessage = value;
    }

    /// <summary>
    /// A value of AbendOccurred.
    /// </summary>
    [JsonPropertyName("abendOccurred")]
    public Common AbendOccurred
    {
      get => abendOccurred ??= new();
      set => abendOccurred = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public CsePerson Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// Gets a value of CountsAndAmounts.
    /// </summary>
    [JsonPropertyName("countsAndAmounts")]
    public CountsAndAmountsGroup CountsAndAmounts
    {
      get => countsAndAmounts ?? (countsAndAmounts = new());
      set => countsAndAmounts = value;
    }

    /// <summary>
    /// Gets a value of SupprCounts.
    /// </summary>
    [JsonPropertyName("supprCounts")]
    public SupprCountsGroup SupprCounts
    {
      get => supprCounts ?? (supprCounts = new());
      set => supprCounts = value;
    }

    /// <summary>
    /// A value of Sign.
    /// </summary>
    [JsonPropertyName("sign")]
    public WorkArea Sign
    {
      get => sign ??= new();
      set => sign = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of AbendCheckLoop.
    /// </summary>
    [JsonPropertyName("abendCheckLoop")]
    public Common AbendCheckLoop
    {
      get => abendCheckLoop ??= new();
      set => abendCheckLoop = value;
    }

    /// <summary>
    /// A value of ErrorLoop.
    /// </summary>
    [JsonPropertyName("errorLoop")]
    public Common ErrorLoop
    {
      get => errorLoop ??= new();
      set => errorLoop = value;
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

    private Common closeInd;
    private DisbursementTransaction amtRemainingToDisburse;
    private Common collFundsAvailForDisb;
    private Common crFeeTaken;
    private DisbursementTransaction newForPrint;
    private DisbursementStatus splitDisbSupprOrReleased;
    private Common currTotArRefNbrAmt;
    private Common prevTotArRefNbrAmt;
    private TextWorkArea sign1;
    private TextWorkArea sign2;
    private WorkArea testMsg;
    private NumericWorkSet numericDisbAmt;
    private NumericWorkSet numericFundsAvailForDisb;
    private NumericWorkSet numericCollAmt;
    private WorkArea textDisbAmt;
    private WorkArea textFundsAvailForDisb;
    private WorkArea textCollAmt;
    private DisbursementStatusHistory forCreate;
    private CashReceiptDetail forDupPmtCheck;
    private CsePerson holdObligee;
    private DisbursementTransaction hold;
    private Common checkForDupPmt;
    private Array<ApEventGroup> apEvent;
    private ControlTable uraSuppressionLength;
    private ControlTable errorsBeforeAbend;
    private Common databaseUpdated;
    private Common rollback;
    private DateWorkArea highestSuppressionDate;
    private Common nbrSuprDisbForArAndCr;
    private CsePerson testFirstObligee;
    private CsePerson testLastObligee;
    private Common testDisplayInd;
    private Common testRunInd;
    private Common countErrorsForAbend;
    private Common programCounter;
    private Common takeOutCrFee;
    private DisbSuppressionStatusHistory hardcodedAutomatic;
    private DisbursementType hardcodedGift;
    private Common processCountToCommit;
    private ObligationType hardcodedMs;
    private MonthlyObligeeSummary toInitializeMonthlyObligeeSummary;
    private CsePersonAccount toInitializeObligee;
    private CsePersonAccount toUpdateObligee;
    private MonthlyObligeeSummary toUpdateMonthlyObligeeSummary;
    private DisbursementTransaction workingDisbursement;
    private DisbursementType determined;
    private DisbursementStatus disbursementStatus;
    private Common adcProgram;
    private DisbursementTranRlnRsn hardcoded;
    private DisbSuppressionStatusHistory hardcodedPerson;
    private DisbSuppressionStatusHistory hardcodedCollectonType;
    private CsePersonAccount hardcodeObligor;
    private DisbursementTransaction initializedDisbursementTransaction;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common numberOfUpdates;
    private External passArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateMessage;
    private Common abendOccurred;
    private CsePerson current;
    private CountsAndAmountsGroup countsAndAmounts;
    private SupprCountsGroup supprCounts;
    private WorkArea sign;
    private DateWorkArea initializedDateWorkArea;
    private DateWorkArea max;
    private Common abendCheckLoop;
    private Common errorLoop;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of PreviousObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("previousObligeeCsePerson")]
    public CsePerson PreviousObligeeCsePerson
    {
      get => previousObligeeCsePerson ??= new();
      set => previousObligeeCsePerson = value;
    }

    /// <summary>
    /// A value of PreviousObligeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("previousObligeeCsePersonAccount")]
    public CsePersonAccount PreviousObligeeCsePersonAccount
    {
      get => previousObligeeCsePersonAccount ??= new();
      set => previousObligeeCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("obligeeCsePerson")]
    public CsePerson ObligeeCsePerson
    {
      get => obligeeCsePerson ??= new();
      set => obligeeCsePerson = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of ObligeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligeeCsePersonAccount")]
    public CsePersonAccount ObligeeCsePersonAccount
    {
      get => obligeeCsePersonAccount ??= new();
      set => obligeeCsePersonAccount = value;
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
    /// A value of SupportedPerson.
    /// </summary>
    [JsonPropertyName("supportedPerson")]
    public CsePersonAccount SupportedPerson
    {
      get => supportedPerson ??= new();
      set => supportedPerson = value;
    }

    /// <summary>
    /// A value of Per1AfCcs.
    /// </summary>
    [JsonPropertyName("per1AfCcs")]
    public DisbursementType Per1AfCcs
    {
      get => per1AfCcs ??= new();
      set => per1AfCcs = value;
    }

    /// <summary>
    /// A value of Per2AfAcs.
    /// </summary>
    [JsonPropertyName("per2AfAcs")]
    public DisbursementType Per2AfAcs
    {
      get => per2AfAcs ??= new();
      set => per2AfAcs = value;
    }

    /// <summary>
    /// A value of Per4NaCcs.
    /// </summary>
    [JsonPropertyName("per4NaCcs")]
    public DisbursementType Per4NaCcs
    {
      get => per4NaCcs ??= new();
      set => per4NaCcs = value;
    }

    /// <summary>
    /// A value of Per5NaCcs.
    /// </summary>
    [JsonPropertyName("per5NaCcs")]
    public DisbursementType Per5NaCcs
    {
      get => per5NaCcs ??= new();
      set => per5NaCcs = value;
    }

    /// <summary>
    /// A value of Per73CrFee.
    /// </summary>
    [JsonPropertyName("per73CrFee")]
    public DisbursementType Per73CrFee
    {
      get => per73CrFee ??= new();
      set => per73CrFee = value;
    }

    /// <summary>
    /// A value of Per1Released.
    /// </summary>
    [JsonPropertyName("per1Released")]
    public DisbursementStatus Per1Released
    {
      get => per1Released ??= new();
      set => per1Released = value;
    }

    /// <summary>
    /// A value of Per2Processed.
    /// </summary>
    [JsonPropertyName("per2Processed")]
    public DisbursementStatus Per2Processed
    {
      get => per2Processed ??= new();
      set => per2Processed = value;
    }

    /// <summary>
    /// A value of Per3Suppressed.
    /// </summary>
    [JsonPropertyName("per3Suppressed")]
    public DisbursementStatus Per3Suppressed
    {
      get => per3Suppressed ??= new();
      set => per3Suppressed = value;
    }

    /// <summary>
    /// A value of Per1.
    /// </summary>
    [JsonPropertyName("per1")]
    public DisbursementTranRlnRsn Per1
    {
      get => per1 ??= new();
      set => per1 = value;
    }

    /// <summary>
    /// A value of Per23Denied.
    /// </summary>
    [JsonPropertyName("per23Denied")]
    public PaymentStatus Per23Denied
    {
      get => per23Denied ??= new();
      set => per23Denied = value;
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

    private DisbursementType disbursementType;
    private CsePerson previousObligeeCsePerson;
    private CsePersonAccount previousObligeeCsePersonAccount;
    private CsePerson obligeeCsePerson;
    private CsePerson obligorCsePerson;
    private CsePerson supported;
    private DisbursementTransaction disbursementTransaction;
    private Collection collection;
    private CollectionType collectionType;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private DebtDetail debtDetail;
    private CsePersonAccount obligeeCsePersonAccount;
    private CsePersonAccount obligorCsePersonAccount;
    private CsePersonAccount supportedPerson;
    private DisbursementType per1AfCcs;
    private DisbursementType per2AfAcs;
    private DisbursementType per4NaCcs;
    private DisbursementType per5NaCcs;
    private DisbursementType per73CrFee;
    private DisbursementStatus per1Released;
    private DisbursementStatus per2Processed;
    private DisbursementStatus per3Suppressed;
    private DisbursementTranRlnRsn per1;
    private PaymentStatus per23Denied;
    private InterstateRequest interstateRequest;
  }
#endregion
}
