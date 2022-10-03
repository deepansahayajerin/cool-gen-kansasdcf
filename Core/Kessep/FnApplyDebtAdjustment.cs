// Program: FN_APPLY_DEBT_ADJUSTMENT, ID: 372257418, model: 746.
// Short name: SWE00258
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
/// A program: FN_APPLY_DEBT_ADJUSTMENT.
/// </para>
/// <para>
/// Resp: Finance
/// </para>
/// </summary>
[Serializable]
public partial class FnApplyDebtAdjustment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_APPLY_DEBT_ADJUSTMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnApplyDebtAdjustment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnApplyDebtAdjustment.
  /// </summary>
  public FnApplyDebtAdjustment(IContext context, Import import, Export export):
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
    // =================================================
    // 3/19/1999 - Bud Adams  -  Processing date logic not needed.
    //   Action block is only used on-line by DBAJ and DBWR, and
    //   the date is always going to be current_date.
    // =================================================
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // 090397	govind		Modified to reverse the collections and the associated 
    // processing online immediately instead of doing at the end of the day
    // batch process. (This eliminates the need for the Debt Adjustment Batch
    // procedure).
    // 112497	govind		Added Obligation Type in the import views.
    // 3/1/1999  E. Parker	Modified action block to meet new business 
    // requirements.
    // 3/19/1999 - b adams  -  Read properties set
    // 10/16/99 - b adams  -  PR# 78519 (details below)
    // 11/3/1999 - Snuya Sharp - PR# H00076154 - When a debt adjustment is done 
    // and the collection is reapplied the CRD is not placed in the correct
    // status and the fully applied indicator is not being set if the CRD is
    // fully distributed.
    // 12/2/99 - b adams  -  PR# 81189: See note below.
    // April, 2000 - m. brown - Made major changes to this cab.  Removed 
    // reversal of manual collections.  Removed logic that creates collections.
    // Changed logic that determines how many collections need to be backed
    // off - now all automatic collections for the debt are reversed.  Also
    // added logic for the new 'writeoff' and 'reinstate' functions.  Note that
    // Primary/Secondary logic is done differently for a regular decrease
    // adjustment on a P/S obligation, vs a writeoff type of decrease
    // adjustment.  This cab is called twice for a writeoff, and only once for a
    // regular decrease type of adjustmemt.
    // ANY CHANGES TO EDITS FOR WRITEOFF AND REINSTATE PROCESSING ALSO NEED TO 
    // BE XCOPIED TO CAB FN_DEBT_ADJ_VERIFY_WO_OR_REINST.
    // September, 2000, MBrown, Work Order # 197:
    // Added new value for Collection Distribution.  When we write off a debt, 
    // we set all automatic collections for the debt to 'W', so that if the debt
    // is reinstated, we can set them back to 'A'utomatic.
    // ---------------------------------------------
    // *******************************************************************
    // January, 2002 - M Brown - Work Order Number: 010504 - Retro Processing.
    //  - Do not reverse collections that are protected against retro 
    // processing.
    //  - Provide a message telling the user to unprotect collections if a 
    // negative
    //    adjustment takes the balance below zero and there are protected 
    // collections.
    //  - If reinstating, collections with a 'W'riteoff distribution method need
    // to
    //    be changed to 'P'rotected, if there is an active collection protection
    //    timeframe for the obligation, that the collection falls within.
    // - If doing a positive debt adjustment, and the obligation is paid in 
    // full, and
    //    there is and active collection protection timeframe set up, the 
    // adjustment
    //    will not be allowed.  The collection protection timeframe must be 
    // deactivated first.
    // January, 2002 - M Brown - Work Order Number: 020199 - Closed Case.
    //  - New debt adjustment reason code of  'CLOSECA'.  This is handled 
    // exactly the same
    //    way as writeoff.
    // *******************************************************************
    // M. Brown, PR # 150538, September 2002.
    // Debts may be written off a second time without a reinstate being done 
    // first.
    // Commented out the edit that checks for a writeoff and does not allow a 
    // second writeoff if collections with a distribution method of 'W' are
    // found.
    ExitState = "ACO_NN0000_ALL_OK";
    local.DebtAdjustment.Amount = import.DebtAdjustment.Amount;
    local.CollectionAdjustmentReason.SystemGeneratedIdentifier = 4;
    local.CollectionAdjustmentReason.Code = "RETDEBT";
    local.Collection.CollectionAdjustmentReasonTxt =
      "SYSTEM GENERATED - RETRO DEBT ADJUSTMENT";
    local.HardcodedAccruing.Classification = "A";
    local.HardcodedVoluntary.Classification = "V";

    if (!ReadObligationType())
    {
      ExitState = "OBLIGATION_TYPE_NF";

      return;
    }

    if (!ReadObligationDebtDebtDetail())
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y')
    {
      if (!ReadCsePerson())
      {
        ExitState = "FN0000_SUPPORTED_PERSON_NF";

        return;
      }
    }

    if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeReinstate.SystemGeneratedIdentifier)
    {
      local.DebtAdjustment.Amount = 0;

      foreach(var item in ReadObligationTransactionRlnRsnDebtAdjustment())
      {
        if (entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodeReinstate.SystemGeneratedIdentifier)
        {
          // : Debt is reinstated already.
          ExitState = "FN0000_WRITE_OFF_DEBT_ADJ_NF";

          return;
        }

        // : January, 2002 - M Brown - Work Order Number: 020199 -
        //   Added Closed Case to this IF.
        if (entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodeWriteoffAll.SystemGeneratedIdentifier || entities
          .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodeWriteoffNaOnly.SystemGeneratedIdentifier || entities
          .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodeCloseCase.SystemGeneratedIdentifier || entities
          .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodeWriteoffAf.SystemGeneratedIdentifier)
        {
          // : Set the adjustment amount to the amount of the writeoff 
          // adjustment
          //  (with an increase type to offset the writeoff).
          local.DebtAdjustment.Amount = entities.DebtAdjustment.Amount;

          goto Test1;
        }
      }

      if (local.DebtAdjustment.Amount == 0)
      {
        // : If the adjustment amount was not set, it means there are no debt 
        // adjustments
        //   for this debt, therefore reinstate is invalid.
        ExitState = "FN0000_WRITE_OFF_DEBT_ADJ_NF";

        return;
      }
    }
    else
    {
      // M. Brown, PR # 150538, September 2002.
      // Debts may be written off a second time without a reinstate being done 
      // first.
    }

Test1:

    // ---------------     WRITE-OFF EDITS    ----------------
    // : January, 2002 - M Brown - Work Order Number: 020199 - Closed Case.
    if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeWriteoffAll.SystemGeneratedIdentifier || import
      .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeWriteoffNaOnly.SystemGeneratedIdentifier || import
      .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeCloseCase.SystemGeneratedIdentifier || import
      .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeWriteoffAf.SystemGeneratedIdentifier)
    {
      // : Check to see if there are any debt adjustments with a reason code of 
      // Writeoff,
      //   for current date. This is not allowed.
      // : January, 2002 - M Brown - Work Order Number: 020199 -
      //   Added Closed Case to this IF.
      if (ReadDebtAdjustment())
      {
        ExitState = "FN0000_WO_FOR_CURRENT_DATE_AE";

        return;
      }
      else
      {
        // :  ok
      }
    }

    // ---------------     END OF WRITE-OFF EDITS    ----------------
    if (AsChar(import.CollProtExists.Flag) == 'Y')
    {
      // : January, 2002 - M Brown - Work Order Number: 010504 - Retro 
      // Processing.
      // : Get total protected collections for the debt.
      ReadCollection3();
    }

    // : April, 2000, M. Brown - adjustment changes.
    if (AsChar(import.DebtAdjustment.DebtAdjustmentType) == 'D')
    {
      // : DECREASE ADJUSTMENT TYPE
      // : Get the total of automatic collections for the debt.
      ReadCollection1();

      // :     WRITEOFF PROCESSING
      // : January, 2002 - M Brown - Work Order Number: 020199 - Added Closed 
      // Case to this IF.
      if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
        .HardcodeWriteoffAll.SystemGeneratedIdentifier || import
        .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
        .HardcodeWriteoffNaOnly.SystemGeneratedIdentifier || import
        .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
        .HardcodeCloseCase.SystemGeneratedIdentifier || import
        .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
        .HardcodeWriteoffAf.SystemGeneratedIdentifier)
      {
        // : Set the adjustment amount to the balance due on the debt detail,
        //   so that the debt detail balance due will be zero after the 
        // adjustment.
        local.DebtAdjustment.Amount = entities.DebtDetail.BalanceDueAmt;

        if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodeWriteoffNaOnly.SystemGeneratedIdentifier)
        {
          // : Get the program,  to determine whether or not this debt should be
          //   written off.
          UseFnDeterminePgmForDebtDetail();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (Equal(local.Program.Code, "NA") || Equal
            (local.Program.Code, "NAI"))
          {
            // : continue
          }
          else
          {
            ExitState = "FN0000_NO_DEBTS_FOUND_FOR_NA_WO";

            return;
          }
        }

        if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodeWriteoffAf.SystemGeneratedIdentifier)
        {
          // : Get the program,  to determine whether or not this debt should be
          //   written off.
          UseFnDeterminePgmForDebtDetail();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (Equal(local.Program.Code, "AF") || Equal
            (local.Program.Code, "FC") || Equal(local.Program.Code, "NC") || Equal
            (local.Program.Code, "NF"))
          {
            // : continue
          }
          else
          {
            ExitState = "FN0000_NO_DEBTS_FOUND_FOR_ST_ARR";

            return;
          }
        }

        if (entities.DebtDetail.BalanceDueAmt == 0 && local
          .AutoCollTotal.TotalCurrency == 0)
        {
          // : No further processing required if the debt detail balance is 
          // already zero.
          ExitState = "FN0000_NO_BALANCE_TO_WRITE_OFF";

          return;
        }

        // : Set all collections related to the debt to manually distributed.
        if (local.AutoCollTotal.TotalCurrency > 0)
        {
          foreach(var item in ReadCollection6())
          {
            // : Set collection to manually distributed for writeoff function.
            // : January, 2002 - M Brown - Work Order Number: 020199 - Closed 
            // Case.
            //   Set the manually distributed reason text differently for closed
            // case.
            if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier ==
              import.HardcodeCloseCase.SystemGeneratedIdentifier)
            {
              local.Collection.ManualDistributionReasonText =
                "Collection changed to manually distributed due to closed case write off of associated debt.";
                
            }
            else
            {
              local.Collection.ManualDistributionReasonText =
                "Collection changed to manually distributed due to write off of associated debt.";
                
            }

            // September, 2000, MBrown, Work Order # 197
            // If there is no debt balance, no writeoff will be done, so set 
            // collections
            // to manually distributed.  Otherwise set to W, which will allow us
            // to reset
            // to A or P if the written off debt is reinstated.
            if (entities.DebtDetail.BalanceDueAmt == 0)
            {
              if (AsChar(entities.Collection.DistributionMethod) == 'P')
              {
                // : Leave protected collections as-is.
              }
              else
              {
                // : Automatic collection on writeoffs with zero balance.  Set 
                // collections to Manual.
                try
                {
                  UpdateCollection2();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "FN0000_COLLECTION_NU_RB";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "FN0000_COLLECTION_PV_RB";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }
            else
            {
              // : A balance is being written off, so all collections will be 
              // set to 'W'.
              try
              {
                UpdateCollection3();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_COLLECTION_NU_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_COLLECTION_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }

          if (entities.DebtDetail.BalanceDueAmt == 0)
          {
            // : No further processing required if the debt detail balance is 
            // already zero.
            ExitState = "FN0000_NOTHING_TO_WRITE_OFF";

            return;
          }
        }

        // :    END  WRITEOFF PROCESSING
      }
      else
      {
        if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodeIncAdj.SystemGeneratedIdentifier)
        {
          // : Get the program,  to determine whether or not this debt should be
          //   written off.
          UseFnDeterminePgmForDebtDetail();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (Equal(local.Program.Code, "AF") || Equal
            (local.Program.Code, "FC") || Equal(local.Program.Code, "NC") || Equal
            (local.Program.Code, "NF"))
          {
            // : continue
          }
          else
          {
            ExitState = "FN0000_NO_DEBTS_FOUND_FOR_ST_ARR";

            return;
          }
        }

        // : REGULAR DECREASE TYPE ADJUSTMENT
        if (local.DebtAdjustment.Amount > entities.DebtDetail.BalanceDueAmt)
        {
          // : The adjustment is greater than the debt detail balance due.
          // : Get total manual collections for the debt.
          ReadCollection2();

          // : The adjustment is greater than the debt detail balance due.   We 
          // need
          //   to reverse automatic collections.
          if (entities.DebtDetail.BalanceDueAmt + local
            .AutoCollTotal.TotalCurrency < local.DebtAdjustment.Amount)
          {
            // January, 2002 - M Brown - Work Order Number: 010504 - Retro 
            // Processing.
            //   When checking to see if the adjustment is greater than the 
            // balance due,
            //   in addition to looking at manual and automatic collection 
            // totals, also
            //   need to look at protected collections, and let the user know if
            // they need
            //   to unprotect protected collections in order to do the 
            // adjustment.
            if (entities.DebtDetail.BalanceDueAmt + local
              .AutoCollTotal.TotalCurrency + local
              .ManualCollTotal.TotalCurrency + local
              .ProtectedCollection.TotalCurrency < local.DebtAdjustment.Amount)
            {
              // : Debt adjustment is too high, even if collections are backed 
              // off.
              ExitState = "FN0000_ADJ_AMT_TOO_HIGH_RB";

              return;
            }
            else
            {
              if (entities.DebtDetail.BalanceDueAmt + local
                .AutoCollTotal.TotalCurrency + local
                .ManualCollTotal.TotalCurrency >= local.DebtAdjustment.Amount)
              {
                // : There are not enough automatic collections to cover the 
                // debt adjustment amount
                //   but there are manual collections that can be backed off.
                ExitState = "FN0000_MUST_BACK_OFF_MANUAL_COLL";

                return;
              }

              if (entities.DebtDetail.BalanceDueAmt + local
                .AutoCollTotal.TotalCurrency + local
                .ProtectedCollection.TotalCurrency >= local
                .DebtAdjustment.Amount)
              {
                // : There are not enough automatic collections to cover the 
                // debt adjustment amount
                //   but there are protected collections that could be 
                // unprotected by CRU.
                ExitState = "FN_MUST_UNPROT_COLL_FOR_DEBT_ADJ";

                return;
              }
              else
              {
                // : January, 2002 - M Brown - Work Order Number: 010504 - Retro
                // Processing.
                // : There are not enough automatic collections to cover the 
                // debt adjustment amount
                //   but there are protected collections that could be 
                // unprotected by CRU,
                //   and manual collections that can be backed off.
                ExitState = "FN_COLL_CHANGE_REQD_FOR_DEBT_ADJ";

                return;
              }
            }
          }

          if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'P' || AsChar
            (entities.Obligation.PrimarySecondaryCode) == 'S')
          {
            // : Read the 'other' obligation for primary/secondary obligation.
            if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'P')
            {
              if (!ReadObligation2())
              {
                ExitState = "FN0000_PRIMARY_SECONDRY_OBLIG_NF";

                return;
              }
            }
            else if (!ReadObligation1())
            {
              ExitState = "FN0000_PRIMARY_SECONDRY_OBLIG_NF";

              return;
            }

            // : We need to read the Cash Receipt Details in order to identify 
            // the Secondary collections to be backed off.  We want to back off
            // only those secondary collections that belong to the correct
            // secondary obligation and come from the same cash receipt detail
            // as the collection for the primary debt. (or vice versa if the
            // original adjustment is being done on a secondary).
            foreach(var item in ReadCollectionCashReceiptDetail())
            {
              if (!ReadCashReceiptEventCashReceiptSourceTypeCashReceiptType())
              {
                ExitState = "CASH_RECEIPT_SOURCE_TYPE_NF";

                return;
              }

              UseFnReverseADistributedColl1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              if (entities.CashReceiptDetail.SequentialIdentifier == local
                .SaveCashReceiptDetail.SequentialIdentifier && entities
                .CashReceiptEvent.SystemGeneratedIdentifier == local
                .SaveCashReceiptEvent.SystemGeneratedIdentifier && entities
                .CashReceiptSourceType.SystemGeneratedIdentifier == local
                .SaveCashReceiptSourceType.SystemGeneratedIdentifier && entities
                .CashReceiptType.SystemGeneratedIdentifier == local
                .SaveCashReceiptType.SystemGeneratedIdentifier)
              {
              }
              else
              {
                local.SaveCashReceiptDetail.SequentialIdentifier =
                  entities.CashReceiptDetail.SequentialIdentifier;
                local.SaveCashReceiptEvent.SystemGeneratedIdentifier =
                  entities.CashReceiptEvent.SystemGeneratedIdentifier;
                local.SaveCashReceiptSourceType.SystemGeneratedIdentifier =
                  entities.CashReceiptSourceType.SystemGeneratedIdentifier;
                local.SaveCashReceiptType.SystemGeneratedIdentifier =
                  entities.CashReceiptType.SystemGeneratedIdentifier;

                // : Now reverse the collections on the 'other' obligation.
                //   Only do this once for each CRD read.
                if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y'
                  )
                {
                  // : Use Supported Person to qualify the read.
                  foreach(var item1 in ReadCollection7())
                  {
                    UseFnReverseADistributedColl2();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }
                  }
                }
                else
                {
                  foreach(var item1 in ReadCollection8())
                  {
                    UseFnReverseADistributedColl2();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }
                  }
                }
              }
            }
          }
          else
          {
            // : Not primary/secondary
            foreach(var item in ReadCollection4())
            {
              UseFnReverseADistributedColl1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }
          }
        }
        else
        {
          // : Nothing needs to be done with collections - the debt adjustment 
          // amount
          //   is less than the debt detail balance due.
        }
      }

      // ***---  end of IF this is a "D"ecrease type of adjusment
    }
    else
    {
      // ------------------------------------------------
      //  Increase type of adjustment.
      // ------------------------------------------------
      // January, 2002 - M Brown - Work Order Number: 010504 - Retro Processing.
      // If the obligation is paid in full, collection protection must be de-
      // activated on COLP first.
      if (AsChar(import.CollProtExists.Flag) == 'Y')
      {
        if (AsChar(entities.ObligationType.Classification) == AsChar
          (local.HardcodedAccruing.Classification))
        {
          if (ReadDebtDetail2())
          {
            // Accruing Obligation not Paid in Full.
          }
          else if (ReadAccrualInstructions())
          {
            if (Lt(import.Current.Date,
              entities.AccrualInstructions2.DiscontinueDt))
            {
              // There are active accrual instructions for the obligation.
            }
            else
            {
              // Accruing Obligation is Paid in Full.
              ExitState = "FN0000_MUST_UNPROTECT_COLLS";

              return;
            }
          }
          else
          {
            ExitState = "FN0000_ACCRUAL_INSTRUCTION_NF_RB";

            return;
          }
        }
        else
        {
          if (AsChar(entities.ObligationType.Classification) == AsChar
            (local.HardcodedVoluntary.Classification))
          {
            goto Test2;
          }

          // : Check to see if non accruing obligation is paid in full.
          if (!Equal(entities.DebtDetail.RetiredDt, local.Null1.Date))
          {
            // : Non-Accruing Obligation is Paid in Full.
            ExitState = "FN0000_MUST_UNPROTECT_COLLS";

            return;
          }
        }
      }

Test2:

      if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
        .HardcodeReinstate.SystemGeneratedIdentifier)
      {
        // September, 2000, MBrown, Work Order # 197: This date will be reset to
        // low values
        //  if any collections are set from 'W' distribution method to 'A'.
        local.DebtAdjustment.DebtAdjustmentProcessDate = import.Current.Date;

        // January, 2002 - M Brown - Work Order Number: 010504 - Retro 
        // Processing.
        // Set the value of the collection distribution method.  If the 
        // collection date
        // is within the date range of a collection protection occurrence, the 
        // value
        // will be 'P'rotected; otherwise the value will be 'A'utomatic.
        foreach(var item in ReadCollection5())
        {
          if (AsChar(import.CollProtExists.Flag) == 'Y')
          {
            if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'S')
            {
              // : Read for collection protection through the primary 
              // obligation.
              //   Collection Protection timeframes are only set up on primary 
              // obligations.
              if (ReadObligCollProtectionHist1())
              {
                local.Collection.DistributionMethod = "P";
              }
              else
              {
                local.Collection.DistributionMethod = "A";
              }
            }
            else if (ReadObligCollProtectionHist2())
            {
              local.Collection.DistributionMethod = "P";
            }
            else
            {
              local.Collection.DistributionMethod = "A";
            }
          }
          else
          {
            // : No collection protection timeframe is set up for the 
            // obligation,
            //   so just set all the collections to 'A'.
            local.Collection.DistributionMethod = "A";
          }

          // September, 2000, MBrown, Work Order # 197
          try
          {
            UpdateCollection1();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_COLLECTION_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_COLLECTION_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }

    // =================================================
    // 4/8/99 - bud adams  -  Retrieve LAP for the debt-adjustment
    //   ob_tran below.
    // =================================================
    if (!ReadLegalActionPerson())
    {
      // ***---  OK; optional relationship
    }

    // : For write-off and reinstate, we set the debt adjustment process date to
    // current date;
    //   anything else sets it to low-date.
    // : January, 2002 - M Brown - Work Order Number: 020199 - Closed Case.
    if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeWriteoffAll.SystemGeneratedIdentifier || import
      .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeWriteoffNaOnly.SystemGeneratedIdentifier || import
      .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeCloseCase.SystemGeneratedIdentifier || import
      .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeWriteoffAf.SystemGeneratedIdentifier)
    {
      local.DebtAdjustment.DebtAdjustmentProcessDate = import.Current.Date;
    }
    else if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeReinstate.SystemGeneratedIdentifier)
    {
      local.DebtAdjustment.DebtAdjustmentProcessDate = import.Current.Date;
    }
    else
    {
      local.DebtAdjustment.DebtAdjustmentProcessDate = local.Null1.Date;
    }

    if (entities.LegalActionPerson.Populated)
    {
      for(local.RetryLoop.Count = 1; local.RetryLoop.Count <= 5; ++
        local.RetryLoop.Count)
      {
        try
        {
          CreateDebtAdjustment1();

          goto Test3;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              if (local.RetryLoop.Count < 5)
              {
                // *** Next ***
              }
              else
              {
                ExitState = "FN0000_OBLIG_TRANS_AE_RB";

                return;
              }

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      for(local.RetryLoop.Count = 1; local.RetryLoop.Count <= 5; ++
        local.RetryLoop.Count)
      {
        try
        {
          CreateDebtAdjustment2();

          goto Test3;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              if (local.RetryLoop.Count < 5)
              {
                // *** Next ***
              }
              else
              {
                ExitState = "FN0000_OBLIG_TRANS_AE_RB";

                return;
              }

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

Test3:

    if (!ReadObligationTransactionRlnRsn())
    {
      ExitState = "FN0000_OBLIG_TRANS_RLN_RSN_NF";

      return;
    }

    for(local.RetryLoop.Count = 1; local.RetryLoop.Count <= 5; ++
      local.RetryLoop.Count)
    {
      try
      {
        CreateObligationTransactionRln();

        break;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            if (local.RetryLoop.Count < 5)
            {
            }
            else
            {
              ExitState = "FN0000_OBLIG_TRANS_RLN_RSN_AE_RB";

              return;
            }

            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (ReadDebtDetail1())
    {
      switch(AsChar(import.DebtAdjustment.DebtAdjustmentType))
      {
        case 'I':
          local.DebtDetail.BalanceDueAmt = entities.DebtDetail.BalanceDueAmt + local
            .DebtAdjustment.Amount;

          break;
        case 'D':
          local.DebtDetail.BalanceDueAmt = entities.DebtDetail.BalanceDueAmt - local
            .DebtAdjustment.Amount;

          if (local.DebtDetail.BalanceDueAmt < 0)
          {
            ExitState = "FN0000_ADJ_AMT_TOO_HIGH_RB";

            return;
          }

          break;
        default:
          break;
      }

      if (local.DebtDetail.BalanceDueAmt > 0)
      {
        local.DebtDetail.RetiredDt = local.Null1.Date;
      }
      else
      {
        local.DebtDetail.RetiredDt = import.Current.Date;
      }

      try
      {
        UpdateDebtDetail();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0214_DEBT_DETAIL_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0218_DEBT_DETAIL_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0000_DEBT_DETAIL_NF_RB";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // : Update Debt Detail Status History.
    local.DebtDetailStatusHistory.EffectiveDt = import.Current.Date;
    local.DebtDetailStatusHistory.ReasonTxt = "DEBT ADJUSTMENT PROCESSING";

    if (entities.DebtDetail.BalanceDueAmt + entities
      .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() > 0)
    {
      local.DebtDetailStatusHistory.Code = import.HardcodeActiveStatus.Code;
    }
    else
    {
      local.DebtDetailStatusHistory.Code =
        import.HardcodeDeactivatedStatus.Code;
    }

    if (ReadDebtDetailStatusHistory())
    {
      try
      {
        UpdateDebtDetailStatusHistory();

        for(local.RetryLoop.Count = 1; local.RetryLoop.Count <= 5; ++
          local.RetryLoop.Count)
        {
          try
          {
            CreateDebtDetailStatusHistory();

            return;
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                if (local.RetryLoop.Count < 5)
                {
                  continue;
                }
                else
                {
                  ExitState = "FN0220_DEBT_DETL_STAT_HIST_AE";

                  return;
                }

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0226_DEBT_DETL_STAT_HIST_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0224_DEBT_DETL_STAT_HIST_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0226_DEBT_DETL_STAT_HIST_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0222_DEBT_DETL_STAT_HIST_NF";
    }
  }

  private static void MoveCollectionAdjustmentReason(
    CollectionAdjustmentReason source, CollectionAdjustmentReason target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
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

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);
    useImport.SupportedPerson.Number = entities.Supported1.Number;
    useImport.Collection.Date = import.Current.Date;
    useImport.HardcodedAccruing.Classification =
      local.HardcodedAccruing.Classification;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void UseFnReverseADistributedColl1()
  {
    var useImport = new FnReverseADistributedColl.Import();
    var useExport = new FnReverseADistributedColl.Export();

    useImport.Persistent.Assign(entities.Collection);
    MoveDateWorkArea(import.Current, useImport.Current);
    MoveCollectionAdjustmentReason(local.CollectionAdjustmentReason,
      useImport.CollectionAdjustmentReason);
    useImport.Updated.CollectionAdjustmentReasonTxt =
      local.Collection.CollectionAdjustmentReasonTxt;

    Call(FnReverseADistributedColl.Execute, useImport, useExport);

    entities.Collection.Assign(useImport.Persistent);
  }

  private void UseFnReverseADistributedColl2()
  {
    var useImport = new FnReverseADistributedColl.Import();
    var useExport = new FnReverseADistributedColl.Export();

    useImport.Persistent.Assign(entities.OtherCollection);
    MoveDateWorkArea(import.Current, useImport.Current);
    MoveCollectionAdjustmentReason(local.CollectionAdjustmentReason,
      useImport.CollectionAdjustmentReason);
    useImport.Updated.CollectionAdjustmentReasonTxt =
      local.Collection.CollectionAdjustmentReasonTxt;

    Call(FnReverseADistributedColl.Execute, useImport, useExport);

    entities.OtherCollection.Assign(useImport.Persistent);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateDebtAdjustment1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = import.HardcodeDebtAdjustment.Type1;
    var amount = local.DebtAdjustment.Amount;
    var debtAdjustmentType = import.DebtAdjustment.DebtAdjustmentType;
    var debtAdjustmentDt = import.Current.Date;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var cpaSupType =
      GetImplicitValue<ObligationTransaction, string>("CpaSupType");
    var otyType = entities.Obligation.DtyGeneratedId;
    var debtAdjustmentProcessDate =
      local.DebtAdjustment.DebtAdjustmentProcessDate;
    var debtAdjCollAdjProcReqInd = "Y";
    var lapId = entities.LegalActionPerson.Identifier;

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtAdjustmentType", debtAdjustmentType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    CheckValid<ObligationTransaction>("DebtAdjCollAdjProcReqInd",
      debtAdjCollAdjProcReqInd);
    entities.DebtAdjustment.Populated = false;
    Update("CreateDebtAdjustment1",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(
          command, "debtAdjInd", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentInd"));
        db.SetString(command, "debtAdjTyp", debtAdjustmentType);
        db.SetDate(command, "debAdjDt", debtAdjustmentDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "debtTyp", "");
        db.SetNullableInt32(command, "volPctAmt", 0);
        db.SetNullableInt32(command, "zdelPrecnvRcptN", 0);
        db.SetNullableInt32(command, "zdelPrecnvrsnIsn", 0);
        db.SetNullableString(command, "cpaSupType", cpaSupType);
        db.SetInt32(command, "otyType", otyType);
        db.SetDate(command, "debtAdjProcDate", debtAdjustmentProcessDate);
        db.SetString(command, "daCaProcReqInd", debtAdjCollAdjProcReqInd);
        db.SetNullableDate(command, "daCaProcDt", default(DateTime));
        db.SetString(command, "rsnCd", "");
        db.SetNullableInt32(command, "lapId", lapId);
        db.SetNullableString(
          command, "reverseClctnsInd", debtAdjCollAdjProcReqInd);
      });

    entities.DebtAdjustment.ObgGeneratedId = obgGeneratedId;
    entities.DebtAdjustment.CspNumber = cspNumber;
    entities.DebtAdjustment.CpaType = cpaType;
    entities.DebtAdjustment.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DebtAdjustment.Type1 = type1;
    entities.DebtAdjustment.Amount = amount;
    entities.DebtAdjustment.DebtAdjustmentType = debtAdjustmentType;
    entities.DebtAdjustment.DebtAdjustmentDt = debtAdjustmentDt;
    entities.DebtAdjustment.CreatedBy = createdBy;
    entities.DebtAdjustment.CreatedTmst = createdTmst;
    entities.DebtAdjustment.CspSupNumber = null;
    entities.DebtAdjustment.CpaSupType = cpaSupType;
    entities.DebtAdjustment.OtyType = otyType;
    entities.DebtAdjustment.DebtAdjustmentProcessDate =
      debtAdjustmentProcessDate;
    entities.DebtAdjustment.DebtAdjCollAdjProcReqInd = debtAdjCollAdjProcReqInd;
    entities.DebtAdjustment.LapId = lapId;
    entities.DebtAdjustment.ReverseCollectionsInd = debtAdjCollAdjProcReqInd;
    entities.DebtAdjustment.Populated = true;
  }

  private void CreateDebtAdjustment2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = import.HardcodeDebtAdjustment.Type1;
    var amount = local.DebtAdjustment.Amount;
    var debtAdjustmentType = import.DebtAdjustment.DebtAdjustmentType;
    var debtAdjustmentDt = import.Current.Date;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var cpaSupType =
      GetImplicitValue<ObligationTransaction, string>("CpaSupType");
    var otyType = entities.Obligation.DtyGeneratedId;
    var debtAdjustmentProcessDate =
      local.DebtAdjustment.DebtAdjustmentProcessDate;
    var debtAdjCollAdjProcReqInd = "Y";

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtAdjustmentType", debtAdjustmentType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    CheckValid<ObligationTransaction>("DebtAdjCollAdjProcReqInd",
      debtAdjCollAdjProcReqInd);
    entities.DebtAdjustment.Populated = false;
    Update("CreateDebtAdjustment2",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(
          command, "debtAdjInd", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentInd"));
        db.SetString(command, "debtAdjTyp", debtAdjustmentType);
        db.SetDate(command, "debAdjDt", debtAdjustmentDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "debtTyp", "");
        db.SetNullableInt32(command, "volPctAmt", 0);
        db.SetNullableInt32(command, "zdelPrecnvRcptN", 0);
        db.SetNullableInt32(command, "zdelPrecnvrsnIsn", 0);
        db.SetNullableString(command, "cpaSupType", cpaSupType);
        db.SetInt32(command, "otyType", otyType);
        db.SetDate(command, "debtAdjProcDate", debtAdjustmentProcessDate);
        db.SetString(command, "daCaProcReqInd", debtAdjCollAdjProcReqInd);
        db.SetNullableDate(command, "daCaProcDt", default(DateTime));
        db.SetString(command, "rsnCd", "");
        db.SetNullableString(
          command, "reverseClctnsInd", debtAdjCollAdjProcReqInd);
      });

    entities.DebtAdjustment.ObgGeneratedId = obgGeneratedId;
    entities.DebtAdjustment.CspNumber = cspNumber;
    entities.DebtAdjustment.CpaType = cpaType;
    entities.DebtAdjustment.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DebtAdjustment.Type1 = type1;
    entities.DebtAdjustment.Amount = amount;
    entities.DebtAdjustment.DebtAdjustmentType = debtAdjustmentType;
    entities.DebtAdjustment.DebtAdjustmentDt = debtAdjustmentDt;
    entities.DebtAdjustment.CreatedBy = createdBy;
    entities.DebtAdjustment.CreatedTmst = createdTmst;
    entities.DebtAdjustment.CspSupNumber = null;
    entities.DebtAdjustment.CpaSupType = cpaSupType;
    entities.DebtAdjustment.OtyType = otyType;
    entities.DebtAdjustment.DebtAdjustmentProcessDate =
      debtAdjustmentProcessDate;
    entities.DebtAdjustment.DebtAdjCollAdjProcReqInd = debtAdjCollAdjProcReqInd;
    entities.DebtAdjustment.LapId = null;
    entities.DebtAdjustment.ReverseCollectionsInd = debtAdjCollAdjProcReqInd;
    entities.DebtAdjustment.Populated = true;
  }

  private void CreateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);

    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDt = local.DebtDetailStatusHistory.EffectiveDt;
    var discontinueDt = import.Max.Date;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var otrType = entities.DebtDetail.OtrType;
    var otrId = entities.DebtDetail.OtrGeneratedId;
    var cpaType = entities.DebtDetail.CpaType;
    var cspNumber = entities.DebtDetail.CspNumber;
    var obgId = entities.DebtDetail.ObgGeneratedId;
    var code = local.DebtDetailStatusHistory.Code;
    var otyType = entities.DebtDetail.OtyType;
    var reasonTxt = local.DebtDetailStatusHistory.ReasonTxt ?? "";

    CheckValid<DebtDetailStatusHistory>("OtrType", otrType);
    CheckValid<DebtDetailStatusHistory>("CpaType", cpaType);
    entities.DebtDetailStatusHistory.Populated = false;
    Update("CreateDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "obTrnStatHstId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrId", otrId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "obTrnStCd", code);
        db.SetInt32(command, "otyType", otyType);
        db.SetNullableString(command, "rsnTxt", reasonTxt);
      });

    entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DebtDetailStatusHistory.EffectiveDt = effectiveDt;
    entities.DebtDetailStatusHistory.DiscontinueDt = discontinueDt;
    entities.DebtDetailStatusHistory.CreatedBy = createdBy;
    entities.DebtDetailStatusHistory.CreatedTmst = createdTmst;
    entities.DebtDetailStatusHistory.OtrType = otrType;
    entities.DebtDetailStatusHistory.OtrId = otrId;
    entities.DebtDetailStatusHistory.CpaType = cpaType;
    entities.DebtDetailStatusHistory.CspNumber = cspNumber;
    entities.DebtDetailStatusHistory.ObgId = obgId;
    entities.DebtDetailStatusHistory.Code = code;
    entities.DebtDetailStatusHistory.OtyType = otyType;
    entities.DebtDetailStatusHistory.ReasonTxt = reasonTxt;
    entities.DebtDetailStatusHistory.Populated = true;
  }

  private void CreateObligationTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    System.Diagnostics.Debug.Assert(entities.DebtAdjustment.Populated);

    var onrGeneratedId =
      entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier;
    var otrType = entities.DebtAdjustment.Type1;
    var otrGeneratedId = entities.DebtAdjustment.SystemGeneratedIdentifier;
    var cpaType = entities.DebtAdjustment.CpaType;
    var cspNumber = entities.DebtAdjustment.CspNumber;
    var obgGeneratedId = entities.DebtAdjustment.ObgGeneratedId;
    var otrPType = entities.Debt.Type1;
    var otrPGeneratedId = entities.Debt.SystemGeneratedIdentifier;
    var cpaPType = entities.Debt.CpaType;
    var cspPNumber = entities.Debt.CspNumber;
    var obgPGeneratedId = entities.Debt.ObgGeneratedId;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var otyTypePrimary = entities.Debt.OtyType;
    var otyTypeSecondary = entities.DebtAdjustment.OtyType;
    var description = import.ObligationTransactionRln.Description ?? "";

    CheckValid<ObligationTransactionRln>("OtrType", otrType);
    CheckValid<ObligationTransactionRln>("CpaType", cpaType);
    CheckValid<ObligationTransactionRln>("OtrPType", otrPType);
    CheckValid<ObligationTransactionRln>("CpaPType", cpaPType);
    entities.ObligationTransactionRln.Populated = false;
    Update("CreateObligationTransactionRln",
      (db, command) =>
      {
        db.SetInt32(command, "onrGeneratedId", onrGeneratedId);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrGeneratedId", otrGeneratedId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "otrPType", otrPType);
        db.SetInt32(command, "otrPGeneratedId", otrPGeneratedId);
        db.SetString(command, "cpaPType", cpaPType);
        db.SetString(command, "cspPNumber", cspPNumber);
        db.SetInt32(command, "obgPGeneratedId", obgPGeneratedId);
        db.SetInt32(command, "obTrnRlnId", systemGeneratedIdentifier);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "otyTypePrimary", otyTypePrimary);
        db.SetInt32(command, "otyTypeSecondary", otyTypeSecondary);
        db.SetNullableString(command, "obTrnRlnDsc", description);
      });

    entities.ObligationTransactionRln.OnrGeneratedId = onrGeneratedId;
    entities.ObligationTransactionRln.OtrType = otrType;
    entities.ObligationTransactionRln.OtrGeneratedId = otrGeneratedId;
    entities.ObligationTransactionRln.CpaType = cpaType;
    entities.ObligationTransactionRln.CspNumber = cspNumber;
    entities.ObligationTransactionRln.ObgGeneratedId = obgGeneratedId;
    entities.ObligationTransactionRln.OtrPType = otrPType;
    entities.ObligationTransactionRln.OtrPGeneratedId = otrPGeneratedId;
    entities.ObligationTransactionRln.CpaPType = cpaPType;
    entities.ObligationTransactionRln.CspPNumber = cspPNumber;
    entities.ObligationTransactionRln.ObgPGeneratedId = obgPGeneratedId;
    entities.ObligationTransactionRln.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ObligationTransactionRln.CreatedBy = createdBy;
    entities.ObligationTransactionRln.CreatedTmst = createdTmst;
    entities.ObligationTransactionRln.OtyTypePrimary = otyTypePrimary;
    entities.ObligationTransactionRln.OtyTypeSecondary = otyTypeSecondary;
    entities.ObligationTransactionRln.Description = description;
    entities.ObligationTransactionRln.Populated = true;
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AccrualInstructions2.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions2.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions2.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions2.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions2.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions2.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions2.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions2.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.AccrualInstructions2.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions2.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions2.CpaType);
      });
  }

  private bool ReadCashReceiptEventCashReceiptSourceTypeCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptSourceTypeCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "creventId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        local.AutoCollTotal.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        local.ManualCollTotal.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadCollection3()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);

    return Read("ReadCollection3",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        local.ProtectedCollection.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCollection4()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection4",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Collection.CrtType = db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Collection.OtyId = db.GetInt32(reader, 16);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 18);
        entities.Collection.CreatedBy = db.GetString(reader, 19);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 20);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 21);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 22);
        entities.Collection.Amount = db.GetDecimal(reader, 23);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 24);
        entities.Collection.DistributionMethod = db.GetString(reader, 25);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 26);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 27);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 28);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 29);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 30);
        entities.Collection.Ocse34ReportingPeriod =
          db.GetNullableDate(reader, 31);
        entities.Collection.ManualDistributionReasonText =
          db.GetNullableString(reader, 32);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 33);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 34);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection5()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection5",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Collection.CrtType = db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Collection.OtyId = db.GetInt32(reader, 16);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 18);
        entities.Collection.CreatedBy = db.GetString(reader, 19);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 20);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 21);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 22);
        entities.Collection.Amount = db.GetDecimal(reader, 23);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 24);
        entities.Collection.DistributionMethod = db.GetString(reader, 25);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 26);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 27);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 28);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 29);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 30);
        entities.Collection.Ocse34ReportingPeriod =
          db.GetNullableDate(reader, 31);
        entities.Collection.ManualDistributionReasonText =
          db.GetNullableString(reader, 32);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 33);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 34);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection6()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection6",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Collection.CrtType = db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Collection.OtyId = db.GetInt32(reader, 16);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 18);
        entities.Collection.CreatedBy = db.GetString(reader, 19);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 20);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 21);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 22);
        entities.Collection.Amount = db.GetDecimal(reader, 23);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 24);
        entities.Collection.DistributionMethod = db.GetString(reader, 25);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 26);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 27);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 28);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 29);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 30);
        entities.Collection.Ocse34ReportingPeriod =
          db.GetNullableDate(reader, 31);
        entities.Collection.ManualDistributionReasonText =
          db.GetNullableString(reader, 32);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 33);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 34);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection7()
  {
    System.Diagnostics.Debug.Assert(entities.OtherObligation.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.OtherCollection.Populated = false;

    return ReadEach("ReadCollection7",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.
          SetInt32(command, "otyType", entities.OtherObligation.DtyGeneratedId);
          
        db.SetInt32(
          command, "obgGeneratedId",
          entities.OtherObligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.OtherObligation.CspNumber);
        db.SetString(command, "cpaType", entities.OtherObligation.CpaType);
        db.
          SetNullableString(command, "cspSupNumber", entities.Supported1.Number);
          
      },
      (db, reader) =>
      {
        entities.OtherCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OtherCollection.AppliedToCode = db.GetString(reader, 1);
        entities.OtherCollection.CollectionDt = db.GetDate(reader, 2);
        entities.OtherCollection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.OtherCollection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.OtherCollection.ConcurrentInd = db.GetString(reader, 5);
        entities.OtherCollection.DisbursementAdjProcessDate =
          db.GetDate(reader, 6);
        entities.OtherCollection.CrtType = db.GetInt32(reader, 7);
        entities.OtherCollection.CstId = db.GetInt32(reader, 8);
        entities.OtherCollection.CrvId = db.GetInt32(reader, 9);
        entities.OtherCollection.CrdId = db.GetInt32(reader, 10);
        entities.OtherCollection.ObgId = db.GetInt32(reader, 11);
        entities.OtherCollection.CspNumber = db.GetString(reader, 12);
        entities.OtherCollection.CpaType = db.GetString(reader, 13);
        entities.OtherCollection.OtrId = db.GetInt32(reader, 14);
        entities.OtherCollection.OtrType = db.GetString(reader, 15);
        entities.OtherCollection.OtyId = db.GetInt32(reader, 16);
        entities.OtherCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 17);
        entities.OtherCollection.CollectionAdjProcessDate =
          db.GetDate(reader, 18);
        entities.OtherCollection.CreatedBy = db.GetString(reader, 19);
        entities.OtherCollection.CreatedTmst = db.GetDateTime(reader, 20);
        entities.OtherCollection.LastUpdatedBy =
          db.GetNullableString(reader, 21);
        entities.OtherCollection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 22);
        entities.OtherCollection.Amount = db.GetDecimal(reader, 23);
        entities.OtherCollection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 24);
        entities.OtherCollection.DistributionMethod = db.GetString(reader, 25);
        entities.OtherCollection.ProgramAppliedTo = db.GetString(reader, 26);
        entities.OtherCollection.AppliedToOrderTypeCode =
          db.GetString(reader, 27);
        entities.OtherCollection.CourtNoticeReqInd =
          db.GetNullableString(reader, 28);
        entities.OtherCollection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 29);
        entities.OtherCollection.AeNotifiedDate =
          db.GetNullableDate(reader, 30);
        entities.OtherCollection.Ocse34ReportingPeriod =
          db.GetNullableDate(reader, 31);
        entities.OtherCollection.ManualDistributionReasonText =
          db.GetNullableString(reader, 32);
        entities.OtherCollection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 33);
        entities.OtherCollection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 34);
        entities.OtherCollection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.OtherCollection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd",
          entities.OtherCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.OtherCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.OtherCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.OtherCollection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.OtherCollection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.OtherCollection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.OtherCollection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.OtherCollection.AppliedToOrderTypeCode);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection8()
  {
    System.Diagnostics.Debug.Assert(entities.OtherObligation.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.OtherCollection.Populated = false;

    return ReadEach("ReadCollection8",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetInt32(command, "otyId", entities.OtherObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", entities.OtherObligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cspNumber", entities.OtherObligation.CspNumber);
        db.SetString(command, "cpaType", entities.OtherObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.OtherCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OtherCollection.AppliedToCode = db.GetString(reader, 1);
        entities.OtherCollection.CollectionDt = db.GetDate(reader, 2);
        entities.OtherCollection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.OtherCollection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.OtherCollection.ConcurrentInd = db.GetString(reader, 5);
        entities.OtherCollection.DisbursementAdjProcessDate =
          db.GetDate(reader, 6);
        entities.OtherCollection.CrtType = db.GetInt32(reader, 7);
        entities.OtherCollection.CstId = db.GetInt32(reader, 8);
        entities.OtherCollection.CrvId = db.GetInt32(reader, 9);
        entities.OtherCollection.CrdId = db.GetInt32(reader, 10);
        entities.OtherCollection.ObgId = db.GetInt32(reader, 11);
        entities.OtherCollection.CspNumber = db.GetString(reader, 12);
        entities.OtherCollection.CpaType = db.GetString(reader, 13);
        entities.OtherCollection.OtrId = db.GetInt32(reader, 14);
        entities.OtherCollection.OtrType = db.GetString(reader, 15);
        entities.OtherCollection.OtyId = db.GetInt32(reader, 16);
        entities.OtherCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 17);
        entities.OtherCollection.CollectionAdjProcessDate =
          db.GetDate(reader, 18);
        entities.OtherCollection.CreatedBy = db.GetString(reader, 19);
        entities.OtherCollection.CreatedTmst = db.GetDateTime(reader, 20);
        entities.OtherCollection.LastUpdatedBy =
          db.GetNullableString(reader, 21);
        entities.OtherCollection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 22);
        entities.OtherCollection.Amount = db.GetDecimal(reader, 23);
        entities.OtherCollection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 24);
        entities.OtherCollection.DistributionMethod = db.GetString(reader, 25);
        entities.OtherCollection.ProgramAppliedTo = db.GetString(reader, 26);
        entities.OtherCollection.AppliedToOrderTypeCode =
          db.GetString(reader, 27);
        entities.OtherCollection.CourtNoticeReqInd =
          db.GetNullableString(reader, 28);
        entities.OtherCollection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 29);
        entities.OtherCollection.AeNotifiedDate =
          db.GetNullableDate(reader, 30);
        entities.OtherCollection.Ocse34ReportingPeriod =
          db.GetNullableDate(reader, 31);
        entities.OtherCollection.ManualDistributionReasonText =
          db.GetNullableString(reader, 32);
        entities.OtherCollection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 33);
        entities.OtherCollection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 34);
        entities.OtherCollection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.OtherCollection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd",
          entities.OtherCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.OtherCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.OtherCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.OtherCollection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.OtherCollection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.OtherCollection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.OtherCollection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.OtherCollection.AppliedToOrderTypeCode);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Collection.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCollectionCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Collection.CrtType = db.GetInt32(reader, 7);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Collection.OtyId = db.GetInt32(reader, 16);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 18);
        entities.Collection.CreatedBy = db.GetString(reader, 19);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 20);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 21);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 22);
        entities.Collection.Amount = db.GetDecimal(reader, 23);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 24);
        entities.Collection.DistributionMethod = db.GetString(reader, 25);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 26);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 27);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 28);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 29);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 30);
        entities.Collection.Ocse34ReportingPeriod =
          db.GetNullableDate(reader, 31);
        entities.Collection.ManualDistributionReasonText =
          db.GetNullableString(reader, 32);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 33);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 34);
        entities.Collection.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Supported1.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supported1.Number = db.GetString(reader, 0);
        entities.Supported1.Type1 = db.GetString(reader, 1);
        entities.Supported1.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Supported1.Type1);
      });
  }

  private bool ReadDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          import.HardcodeWriteoffAll.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          import.HardcodeWriteoffNaOnly.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          import.HardcodeCloseCase.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          import.HardcodeWriteoffAf.SystemGeneratedIdentifier);
        db.
          SetDate(command, "debAdjDt", import.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.DebtAdjustment.CreatedBy = db.GetString(reader, 8);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 9);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 11);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 12);
        entities.DebtAdjustment.DebtAdjustmentProcessDate =
          db.GetDate(reader, 13);
        entities.DebtAdjustment.DebtAdjCollAdjProcReqInd =
          db.GetString(reader, 14);
        entities.DebtAdjustment.LapId = db.GetNullableInt32(reader, 15);
        entities.DebtAdjustment.ReverseCollectionsInd =
          db.GetNullableString(reader, 16);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);
        CheckValid<ObligationTransaction>("DebtAdjCollAdjProcReqInd",
          entities.DebtAdjustment.DebtAdjCollAdjProcReqInd);
      });
  }

  private bool ReadDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
        db.SetInt32(command, "obgGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
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
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.DebtDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 14);
        entities.DebtDetail.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.CheckPif.Populated = false;

    return Read("ReadDebtDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CheckPif.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.CheckPif.CspNumber = db.GetString(reader, 1);
        entities.CheckPif.CpaType = db.GetString(reader, 2);
        entities.CheckPif.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.CheckPif.OtyType = db.GetInt32(reader, 4);
        entities.CheckPif.OtrType = db.GetString(reader, 5);
        entities.CheckPif.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.CheckPif.RetiredDt = db.GetNullableDate(reader, 7);
        entities.CheckPif.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.CheckPif.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.CheckPif.OtrType);
      });
  }

  private bool ReadDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    entities.DebtDetailStatusHistory.Populated = false;

    return Read("ReadDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetInt32(command, "obgId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetInt32(command, "otrId", entities.DebtDetail.OtrGeneratedId);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
        db.SetNullableDate(
          command, "discontinueDt", import.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DebtDetailStatusHistory.EffectiveDt = db.GetDate(reader, 1);
        entities.DebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 2);
        entities.DebtDetailStatusHistory.CreatedBy = db.GetString(reader, 3);
        entities.DebtDetailStatusHistory.CreatedTmst =
          db.GetDateTime(reader, 4);
        entities.DebtDetailStatusHistory.OtrType = db.GetString(reader, 5);
        entities.DebtDetailStatusHistory.OtrId = db.GetInt32(reader, 6);
        entities.DebtDetailStatusHistory.CpaType = db.GetString(reader, 7);
        entities.DebtDetailStatusHistory.CspNumber = db.GetString(reader, 8);
        entities.DebtDetailStatusHistory.ObgId = db.GetInt32(reader, 9);
        entities.DebtDetailStatusHistory.Code = db.GetString(reader, 10);
        entities.DebtDetailStatusHistory.OtyType = db.GetInt32(reader, 11);
        entities.DebtDetailStatusHistory.ReasonTxt =
          db.GetNullableString(reader, 12);
        entities.DebtDetailStatusHistory.Populated = true;
        CheckValid<DebtDetailStatusHistory>("OtrType",
          entities.DebtDetailStatusHistory.OtrType);
        CheckValid<DebtDetailStatusHistory>("CpaType",
          entities.DebtDetailStatusHistory.CpaType);
      });
  }

  private bool ReadLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(
          command, "laPersonId", entities.Debt.LapId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadObligCollProtectionHist1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist1",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "deactivationDate", local.Null1.Date.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollStrtDt",
          entities.Collection.CollectionDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 1);
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 4);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 5);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 6);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 7);
        entities.ObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ObligCollProtectionHist.CpaType);
      });
  }

  private bool ReadObligCollProtectionHist2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          entities.Obligation.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "otyIdentifier", entities.Obligation.DtyGeneratedId);
          
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetDate(
          command, "deactivationDate", local.Null1.Date.GetValueOrDefault());
        db.SetDate(
          command, "cvrdCollStrtDt",
          entities.Collection.CollectionDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 1);
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 4);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 5);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 6);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 7);
        entities.ObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ObligCollProtectionHist.CpaType);
      });
  }

  private bool ReadObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.OtherObligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.OtherObligation.CpaType = db.GetString(reader, 0);
        entities.OtherObligation.CspNumber = db.GetString(reader, 1);
        entities.OtherObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.OtherObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.OtherObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.OtherObligation.CpaType);
      });
  }

  private bool ReadObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.OtherObligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.OtherObligation.CpaType = db.GetString(reader, 0);
        entities.OtherObligation.CspNumber = db.GetString(reader, 1);
        entities.OtherObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.OtherObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.OtherObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.OtherObligation.CpaType);
      });
  }

  private bool ReadObligationDebtDebtDetail()
  {
    entities.Obligation.Populated = false;
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;

    return Read("ReadObligationDebtDebtDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(command, "obTrnId", import.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Debt.CpaType = db.GetString(reader, 0);
        entities.DebtDetail.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.OtyType = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 5);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 6);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 6);
        entities.Debt.Type1 = db.GetString(reader, 7);
        entities.DebtDetail.OtrType = db.GetString(reader, 7);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 8);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 9);
        entities.Debt.LapId = db.GetNullableInt32(reader, 10);
        entities.DebtDetail.DueDt = db.GetDate(reader, 11);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 12);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 13);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 14);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 15);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 17);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 18);
        entities.DebtDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 19);
        entities.DebtDetail.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.Obligation.Populated = true;
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private bool ReadObligationTransactionRlnRsn()
  {
    entities.ObligationTransactionRlnRsn.Populated = false;

    return Read("ReadObligationTransactionRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionRlnRsnDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtAdjustment.Populated = false;
    entities.ObligationTransactionRlnRsn.Populated = false;

    return ReadEach("ReadObligationTransactionRlnRsnDebtAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 2);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 3);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 5);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 7);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 8);
        entities.DebtAdjustment.CreatedBy = db.GetString(reader, 9);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 10);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 11);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 12);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 13);
        entities.DebtAdjustment.DebtAdjustmentProcessDate =
          db.GetDate(reader, 14);
        entities.DebtAdjustment.DebtAdjCollAdjProcReqInd =
          db.GetString(reader, 15);
        entities.DebtAdjustment.LapId = db.GetNullableInt32(reader, 16);
        entities.DebtAdjustment.ReverseCollectionsInd =
          db.GetNullableString(reader, 17);
        entities.DebtAdjustment.Populated = true;
        entities.ObligationTransactionRlnRsn.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);
        CheckValid<ObligationTransaction>("DebtAdjCollAdjProcReqInd",
          entities.DebtAdjustment.DebtAdjCollAdjProcReqInd);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Classification = db.GetString(reader, 1);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private void UpdateCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var distributionMethod = local.Collection.DistributionMethod;
    var manualDistributionReasonText = Spaces(240);

    CheckValid<Collection>("DistributionMethod", distributionMethod);
    entities.Collection.Populated = false;
    Update("UpdateCollection1",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "distMtd", distributionMethod);
        db.SetNullableString(
          command, "mnlDistRsnTxt", manualDistributionReasonText);
        db.SetInt32(
          command, "collId", entities.Collection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.Collection.CrtType);
        db.SetInt32(command, "cstId", entities.Collection.CstId);
        db.SetInt32(command, "crvId", entities.Collection.CrvId);
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "obgId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetInt32(command, "otrId", entities.Collection.OtrId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otyId", entities.Collection.OtyId);
      });

    entities.Collection.LastUpdatedBy = lastUpdatedBy;
    entities.Collection.LastUpdatedTmst = lastUpdatedTmst;
    entities.Collection.DistributionMethod = distributionMethod;
    entities.Collection.ManualDistributionReasonText =
      manualDistributionReasonText;
    entities.Collection.Populated = true;
  }

  private void UpdateCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var distributionMethod = "M";
    var manualDistributionReasonText =
      local.Collection.ManualDistributionReasonText ?? "";

    CheckValid<Collection>("DistributionMethod", distributionMethod);
    entities.Collection.Populated = false;
    Update("UpdateCollection2",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "distMtd", distributionMethod);
        db.SetNullableString(
          command, "mnlDistRsnTxt", manualDistributionReasonText);
        db.SetInt32(
          command, "collId", entities.Collection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.Collection.CrtType);
        db.SetInt32(command, "cstId", entities.Collection.CstId);
        db.SetInt32(command, "crvId", entities.Collection.CrvId);
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "obgId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetInt32(command, "otrId", entities.Collection.OtrId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otyId", entities.Collection.OtyId);
      });

    entities.Collection.LastUpdatedBy = lastUpdatedBy;
    entities.Collection.LastUpdatedTmst = lastUpdatedTmst;
    entities.Collection.DistributionMethod = distributionMethod;
    entities.Collection.ManualDistributionReasonText =
      manualDistributionReasonText;
    entities.Collection.Populated = true;
  }

  private void UpdateCollection3()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var distributionMethod = "W";
    var manualDistributionReasonText =
      local.Collection.ManualDistributionReasonText ?? "";

    CheckValid<Collection>("DistributionMethod", distributionMethod);
    entities.Collection.Populated = false;
    Update("UpdateCollection3",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "distMtd", distributionMethod);
        db.SetNullableString(
          command, "mnlDistRsnTxt", manualDistributionReasonText);
        db.SetInt32(
          command, "collId", entities.Collection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.Collection.CrtType);
        db.SetInt32(command, "cstId", entities.Collection.CstId);
        db.SetInt32(command, "crvId", entities.Collection.CrvId);
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "obgId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetInt32(command, "otrId", entities.Collection.OtrId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otyId", entities.Collection.OtyId);
      });

    entities.Collection.LastUpdatedBy = lastUpdatedBy;
    entities.Collection.LastUpdatedTmst = lastUpdatedTmst;
    entities.Collection.DistributionMethod = distributionMethod;
    entities.Collection.ManualDistributionReasonText =
      manualDistributionReasonText;
    entities.Collection.Populated = true;
  }

  private void UpdateDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);

    var balanceDueAmt = local.DebtDetail.BalanceDueAmt;
    var retiredDt = local.DebtDetail.RetiredDt;
    var lastUpdatedTmst = import.Current.Timestamp;
    var lastUpdatedBy = global.UserId;

    entities.DebtDetail.Populated = false;
    Update("UpdateDebtDetail",
      (db, command) =>
      {
        db.SetDecimal(command, "balDueAmt", balanceDueAmt);
        db.SetNullableDate(command, "retiredDt", retiredDt);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetInt32(
          command, "otrGeneratedId", entities.DebtDetail.OtrGeneratedId);
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
      });

    entities.DebtDetail.BalanceDueAmt = balanceDueAmt;
    entities.DebtDetail.RetiredDt = retiredDt;
    entities.DebtDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.DebtDetail.LastUpdatedBy = lastUpdatedBy;
    entities.DebtDetail.Populated = true;
  }

  private void UpdateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetailStatusHistory.Populated);

    var discontinueDt = import.Current.Date;
    var reasonTxt = local.DebtDetailStatusHistory.ReasonTxt ?? "";

    entities.DebtDetailStatusHistory.Populated = false;
    Update("UpdateDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetNullableString(command, "rsnTxt", reasonTxt);
        db.SetInt32(
          command, "obTrnStatHstId",
          entities.DebtDetailStatusHistory.SystemGeneratedIdentifier);
        db.SetString(
          command, "otrType", entities.DebtDetailStatusHistory.OtrType);
        db.SetInt32(command, "otrId", entities.DebtDetailStatusHistory.OtrId);
        db.SetString(
          command, "cpaType", entities.DebtDetailStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber", entities.DebtDetailStatusHistory.CspNumber);
        db.SetInt32(command, "obgId", entities.DebtDetailStatusHistory.ObgId);
        db.
          SetString(command, "obTrnStCd", entities.DebtDetailStatusHistory.Code);
          
        db.
          SetInt32(command, "otyType", entities.DebtDetailStatusHistory.OtyType);
          
      });

    entities.DebtDetailStatusHistory.DiscontinueDt = discontinueDt;
    entities.DebtDetailStatusHistory.ReasonTxt = reasonTxt;
    entities.DebtDetailStatusHistory.Populated = true;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
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
    /// A value of HardcodeWriteoffAll.
    /// </summary>
    [JsonPropertyName("hardcodeWriteoffAll")]
    public ObligationTransactionRlnRsn HardcodeWriteoffAll
    {
      get => hardcodeWriteoffAll ??= new();
      set => hardcodeWriteoffAll = value;
    }

    /// <summary>
    /// A value of HardcodeWriteoffNaOnly.
    /// </summary>
    [JsonPropertyName("hardcodeWriteoffNaOnly")]
    public ObligationTransactionRlnRsn HardcodeWriteoffNaOnly
    {
      get => hardcodeWriteoffNaOnly ??= new();
      set => hardcodeWriteoffNaOnly = value;
    }

    /// <summary>
    /// A value of HardcodeReinstate.
    /// </summary>
    [JsonPropertyName("hardcodeReinstate")]
    public ObligationTransactionRlnRsn HardcodeReinstate
    {
      get => hardcodeReinstate ??= new();
      set => hardcodeReinstate = value;
    }

    /// <summary>
    /// A value of HardcodeDeactivatedStatus.
    /// </summary>
    [JsonPropertyName("hardcodeDeactivatedStatus")]
    public DebtDetailStatusHistory HardcodeDeactivatedStatus
    {
      get => hardcodeDeactivatedStatus ??= new();
      set => hardcodeDeactivatedStatus = value;
    }

    /// <summary>
    /// A value of HardcodeActiveStatus.
    /// </summary>
    [JsonPropertyName("hardcodeActiveStatus")]
    public DebtDetailStatusHistory HardcodeActiveStatus
    {
      get => hardcodeActiveStatus ??= new();
      set => hardcodeActiveStatus = value;
    }

    /// <summary>
    /// A value of HardcodeDebtAdjustment.
    /// </summary>
    [JsonPropertyName("hardcodeDebtAdjustment")]
    public ObligationTransaction HardcodeDebtAdjustment
    {
      get => hardcodeDebtAdjustment ??= new();
      set => hardcodeDebtAdjustment = value;
    }

    /// <summary>
    /// A value of HardcodeCloseCase.
    /// </summary>
    [JsonPropertyName("hardcodeCloseCase")]
    public ObligationTransactionRlnRsn HardcodeCloseCase
    {
      get => hardcodeCloseCase ??= new();
      set => hardcodeCloseCase = value;
    }

    /// <summary>
    /// A value of HcOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hcOtrnTDebt")]
    public ObligationTransaction HcOtrnTDebt
    {
      get => hcOtrnTDebt ??= new();
      set => hcOtrnTDebt = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CollProtExists.
    /// </summary>
    [JsonPropertyName("collProtExists")]
    public Common CollProtExists
    {
      get => collProtExists ??= new();
      set => collProtExists = value;
    }

    /// <summary>
    /// A value of HardcodeIncAdj.
    /// </summary>
    [JsonPropertyName("hardcodeIncAdj")]
    public ObligationTransactionRlnRsn HardcodeIncAdj
    {
      get => hardcodeIncAdj ??= new();
      set => hardcodeIncAdj = value;
    }

    /// <summary>
    /// A value of HardcodeWriteoffAf.
    /// </summary>
    [JsonPropertyName("hardcodeWriteoffAf")]
    public ObligationTransactionRlnRsn HardcodeWriteoffAf
    {
      get => hardcodeWriteoffAf ??= new();
      set => hardcodeWriteoffAf = value;
    }

    private CsePerson csePerson;
    private Obligation obligation;
    private ObligationType obligationType;
    private ObligationTransaction debt;
    private ObligationTransaction debtAdjustment;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private ObligationTransactionRln obligationTransactionRln;
    private DateWorkArea max;
    private ObligationTransactionRlnRsn hardcodeWriteoffAll;
    private ObligationTransactionRlnRsn hardcodeWriteoffNaOnly;
    private ObligationTransactionRlnRsn hardcodeReinstate;
    private DebtDetailStatusHistory hardcodeDeactivatedStatus;
    private DebtDetailStatusHistory hardcodeActiveStatus;
    private ObligationTransaction hardcodeDebtAdjustment;
    private ObligationTransactionRlnRsn hardcodeCloseCase;
    private ObligationTransaction hcOtrnTDebt;
    private CsePersonAccount hcCpaObligor;
    private DateWorkArea current;
    private Common collProtExists;
    private ObligationTransactionRlnRsn hardcodeIncAdj;
    private ObligationTransactionRlnRsn hardcodeWriteoffAf;
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
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of Concurrent.
    /// </summary>
    [JsonPropertyName("concurrent")]
    public ObligationTransaction Concurrent
    {
      get => concurrent ??= new();
      set => concurrent = value;
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
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
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
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    /// <summary>
    /// A value of SaveCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("saveCashReceiptSourceType")]
    public CashReceiptSourceType SaveCashReceiptSourceType
    {
      get => saveCashReceiptSourceType ??= new();
      set => saveCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of SaveCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("saveCashReceiptEvent")]
    public CashReceiptEvent SaveCashReceiptEvent
    {
      get => saveCashReceiptEvent ??= new();
      set => saveCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of SaveCashReceiptType.
    /// </summary>
    [JsonPropertyName("saveCashReceiptType")]
    public CashReceiptType SaveCashReceiptType
    {
      get => saveCashReceiptType ??= new();
      set => saveCashReceiptType = value;
    }

    /// <summary>
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of RetryLoop.
    /// </summary>
    [JsonPropertyName("retryLoop")]
    public Common RetryLoop
    {
      get => retryLoop ??= new();
      set => retryLoop = value;
    }

    /// <summary>
    /// A value of AutoCollTotal.
    /// </summary>
    [JsonPropertyName("autoCollTotal")]
    public Common AutoCollTotal
    {
      get => autoCollTotal ??= new();
      set => autoCollTotal = value;
    }

    /// <summary>
    /// A value of ManualCollTotal.
    /// </summary>
    [JsonPropertyName("manualCollTotal")]
    public Common ManualCollTotal
    {
      get => manualCollTotal ??= new();
      set => manualCollTotal = value;
    }

    /// <summary>
    /// A value of SaveCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("saveCashReceiptDetail")]
    public CashReceiptDetail SaveCashReceiptDetail
    {
      get => saveCashReceiptDetail ??= new();
      set => saveCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ProtectedCollection.
    /// </summary>
    [JsonPropertyName("protectedCollection")]
    public Common ProtectedCollection
    {
      get => protectedCollection ??= new();
      set => protectedCollection = value;
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
    /// A value of HardcodedVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodedVoluntary")]
    public ObligationType HardcodedVoluntary
    {
      get => hardcodedVoluntary ??= new();
      set => hardcodedVoluntary = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      concurrent = null;
      program = null;
      debtAdjustment = null;
      saveCashReceiptSourceType = null;
      saveCashReceiptEvent = null;
      saveCashReceiptType = null;
      null1 = null;
      retryLoop = null;
      autoCollTotal = null;
      manualCollTotal = null;
      saveCashReceiptDetail = null;
      protectedCollection = null;
      hardcodedAccruing = null;
      hardcodedVoluntary = null;
    }

    private ObligationTransaction concurrent;
    private Program program;
    private ObligationTransaction debtAdjustment;
    private DebtDetail debtDetail;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private CashReceiptSourceType saveCashReceiptSourceType;
    private CashReceiptEvent saveCashReceiptEvent;
    private CashReceiptType saveCashReceiptType;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private Collection collection;
    private DateWorkArea null1;
    private Common retryLoop;
    private Common autoCollTotal;
    private Common manualCollTotal;
    private CashReceiptDetail saveCashReceiptDetail;
    private Common protectedCollection;
    private ObligationType hardcodedAccruing;
    private ObligationType hardcodedVoluntary;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
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
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
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
    /// A value of OtherObligation.
    /// </summary>
    [JsonPropertyName("otherObligation")]
    public Obligation OtherObligation
    {
      get => otherObligation ??= new();
      set => otherObligation = value;
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
    /// A value of AccrualInstructions1.
    /// </summary>
    [JsonPropertyName("accrualInstructions1")]
    public ObligationTransaction AccrualInstructions1
    {
      get => accrualInstructions1 ??= new();
      set => accrualInstructions1 = value;
    }

    /// <summary>
    /// A value of OtherDebt.
    /// </summary>
    [JsonPropertyName("otherDebt")]
    public ObligationTransaction OtherDebt
    {
      get => otherDebt ??= new();
      set => otherDebt = value;
    }

    /// <summary>
    /// A value of AccrualInstructions2.
    /// </summary>
    [JsonPropertyName("accrualInstructions2")]
    public AccrualInstructions AccrualInstructions2
    {
      get => accrualInstructions2 ??= new();
      set => accrualInstructions2 = value;
    }

    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of OtherCollection.
    /// </summary>
    [JsonPropertyName("otherCollection")]
    public Collection OtherCollection
    {
      get => otherCollection ??= new();
      set => otherCollection = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of CheckPif.
    /// </summary>
    [JsonPropertyName("checkPif")]
    public DebtDetail CheckPif
    {
      get => checkPif ??= new();
      set => checkPif = value;
    }

    /// <summary>
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private CsePerson supported1;
    private CsePersonAccount supported2;
    private Obligation obligation;
    private ObligationRln obligationRln;
    private ObligationType obligationType;
    private Obligation otherObligation;
    private ObligationTransaction debt;
    private ObligationTransaction accrualInstructions1;
    private ObligationTransaction otherDebt;
    private AccrualInstructions accrualInstructions2;
    private ObligationTransaction debtAdjustment;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private DebtDetail debtDetail;
    private Collection collection;
    private Collection otherCollection;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private LegalActionPerson legalActionPerson;
    private ObligCollProtectionHist obligCollProtectionHist;
    private DebtDetail checkPif;
    private DebtDetailStatusHistory debtDetailStatusHistory;
  }
#endregion
}
