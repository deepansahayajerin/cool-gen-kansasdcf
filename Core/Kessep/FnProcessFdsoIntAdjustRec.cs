// Program: FN_PROCESS_FDSO_INT_ADJUST_REC, ID: 372529957, model: 746.
// Short name: SWE01627
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PROCESS_FDSO_INT_ADJUST_REC.
/// </summary>
[Serializable]
public partial class FnProcessFdsoIntAdjustRec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_FDSO_INT_ADJUST_REC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessFdsoIntAdjustRec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessFdsoIntAdjustRec.
  /// </summary>
  public FnProcessFdsoIntAdjustRec(IContext context, Import import,
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
    // ***************************************************
    // 04/28/97	A.Kinney	Changed Current_Date
    // ***************************************************
    // 05/12/99	SWSRPDP      	Re-did ALL Logic for Adjustments
    // Changed to SELECT by SSN and Process_Year
    // the SMALLEST Collection Cash_Receipt_Detail
    // that has a Collection_Amount
    // Large enough to cover the Current Adjustment
    // and that has NOT been Previously Adjusted (Either Fully OR Partially).
    // Changed the logic for Partial_Adjustments
    // to Roll Back amounts for Adjustments in THIS order:
    // 1)   UNDistributed Amounts
    // 2)   Distributed Amounts
    // 3)   Refunded Amounts
    // ***************************************************
    // 05/27/99	SWSRPDP      	H00078288  Added Obligor NAME to Error Message.
    // ***************************************************
    // 11/29/00	SWSRPDP      	I00107466  Added Processing for Multiple 
    // Adjustments on
    // the same collection (CRD)
    // ***************************************************
    // 01/08/02        SWSRPDP         WR010504-A - Retro Processing
    // Create an EVENT if "P"rotected Collection is Adjusted
    // ***************************************************
    // 06/11/10  R.Mathews   Set discontinue date for FDSO adjustments to 
    // process date
    // instead of collection date
    // ***************************************************
    local.Current.Date = import.Save.Date;
    local.CaseNumber.CaseNumber = "";
    MoveProgramProcessingInfo(import.ProgramProcessingInfo, local.Pass);

    // *****
    // Hardcoded area.
    // *****
    export.AdjRefCnt.Count = import.AdjRefCnt.Count;
    export.AdjRefCurrCnt.Count = import.AdjRefCurrCnt.Count;
    export.AdjRefPrevCnt.Count = import.AdjRefPrevCnt.Count;
    export.AdjRefAmt.TotalCurrency = import.AdjRefAmt.TotalCurrency;
    export.AdjRefAmtCurr.TotalCurrency = import.AdjRefAmtCurr.TotalCurrency;
    export.AdjRefAmtPrev.TotalCurrency = import.AdjRefAmtPrev.TotalCurrency;
    export.AdjAdjCnt.Count = import.AdjAdjCnt.Count;
    export.AdjAdjPrevCnt.Count = import.AdjAdjPrevCnt.Count;
    export.AdjAdjCurrCnt.Count = import.AdjAdjCurrCnt.Count;
    export.AdjAdjAmt.TotalCurrency = import.AdjAdjAmt.TotalCurrency;
    export.AdjAdjAmtCurr.TotalCurrency = import.AdjAdjAmtCurr.TotalCurrency;
    export.AdjAdjAmtPrev.TotalCurrency = import.AdjAdjAmtPrev.TotalCurrency;
    export.Swefb612AdjustCount.Count = import.Swefb612AdjustCount.Count;
    export.Swefb612AdjustmentAmt.TotalCurrency =
      import.Swefb612AdjustmentAmt.TotalCurrency;
    export.Swefb612NetAmount.TotalCurrency =
      import.Swefb612NetAmount.TotalCurrency;
    UseFnHardcodedCashReceipting();

    // *****
    // WRONG - This was Interfunded (10)  -- NEEDS to be EFT(6)
    // *****
    local.ViewHardcodedViews.HardcodedEft.SystemGeneratedIdentifier = 6;

    // *****
    // WR010504-B - Retro Processing - EVENT if "P"rotected Collection is 
    // Adjusted
    // *****
    local.HardcodedProtected.DistributionMethod = "P";

    // *****
    // Validate the Cash Receipt Detail Relationship Reason.
    // *****
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.AdjustingCashReceipt.SequentialNumber =
      export.ImportCashReceipt.SequentialNumber;

    if (!ReadCashReceipt())
    {
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    if (ReadCashReceiptDetailRlnRsn())
    {
      local.CashReceiptDetailRlnRsn.Assign(entities.CashReceiptDetailRlnRsn);
    }
    else
    {
      ExitState = "FN0059_CASH_RCPT_DTL_RLN_RSN_NF";

      return;
    }

    // *** Read cash receipt source type = FDSO ***
    if (!ReadCashReceiptSourceType())
    {
      ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

      return;
    }

    // *****
    // Validate the amount of the adjustment against the Cash Receipt Details 
    // available to adjust.
    // *****
    local.TotAmtOfCollForAdj.TotalCurrency = 0;
    export.NoMatchOnYear.Flag = "N";
    local.TotalDetailAdjustments.TotalCurrency = 0;

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // 01/21/1999  SWSRPDP  Added Relationship
    // AND DESIRED original cash_receipt_detail is_part_of
    // DESIRED original cash_receipt
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // Sort to start with SMALLEST Collection Amount
    // Get Records ONLY for SSN SPECIFIED on Adjustment
    // Get ONLY  Records CREATED by "SWEFB612"
    // Do NOT Get ADJUSTMENT TYPE (Adjustment_Ind = Y) Records
    // Do NOT Get Records CURRENTLY in ADJUSTED Status
    // bypass BLANK ADJUSTMENT YEAR
    foreach(var item in ReadCashReceiptDetailCashReceipt2())
    {
      // * * * * * * * * * * * * * *
      // 11/29/00 SWSRPDP I00107466  Change Processing to Allow Multiple 
      // Adjustments
      local.AlreadyAdjustedSwitch.Flag = "N";

      // Does this Cash_Receipt_Detail have a VALID YEAR
      if (Lt(entities.OriginalToBeAdjustedCashReceiptDetail.OffsetTaxYear, 1900))
        
      {
        // This FLAG is used for ERROR REPORTING when a Cash_Receipt_Detail 
        // EXIST -- But NOT for the Correct Year
        export.NoMatchOnYear.Flag = "Y";

        continue;
      }

      // Is This Cash_Receipt_Detail for the CORRECT YEAR
      if (!Equal(entities.OriginalToBeAdjustedCashReceiptDetail.OffsetTaxYear,
        import.AdditionalInformation.Adjustment.OffsetTaxYear.
          GetValueOrDefault()))
      {
        // This FLAG is used for ERROR REPORTING when a Cash_Receipt_Detail 
        // EXIST -- But NOT for the Correct Year
        export.NoMatchOnYear.Flag = "Y";

        continue;
      }

      export.NoMatchOnYear.Flag = "N";

      // Has this Cash_Receipt_Detail been ADJUSTED
      // * * * * * * * * * * * * * *
      // 11/29/00 SWSRPDP I00107466  Change Processing for Allow Multiple 
      // Adjustments
      local.TotAmtOfCollForAdj.TotalCurrency = 0;
      local.TotalAdjustmentForCol.TotalCurrency = 0;

      foreach(var item1 in ReadCashReceiptDetailCashReceiptDetailBalanceAdj())
      {
        // * * * * * * * * * * * * * *
        // 11/29/00 SWSRPDP I00107466  Change Processing for Allow Multiple 
        // Adjustments
        local.AlreadyAdjustedSwitch.Flag = "Y";

        // * * * * * * * * * * * * * *
        // 11/29/00 SWSRPDP I00107466  Accumulate ALL Balance Adjustments for 
        // this Collection
        local.TotalAdjustmentForCol.TotalCurrency += entities.
          AdjustedByCashReceiptDetail.CollectionAmount;
      }

      // 06/09/99 - Added to BYPASS Partially Adjusted CRDs.
      // IF a Receipt_Refund Exist
      // WITHOUT a Payment_Request
      // This Cash_Receip_Detail has Been Partially Adjusted
      // OTHERWISE Continue Processing
      foreach(var item1 in ReadReceiptRefund())
      {
        foreach(var item2 in ReadPaymentRequest())
        {
          // 06/09/99 - If a PAYMENT_REQUEST DOES
          // Exist --  NOT a  PARTIAL_ADJUSTMENT
          // Read NEXT Adjusting Receipt_Refund
          goto ReadEach;
        }

        // * * * * * * * * * * * * * *
        // 11/29/00 SWSRPDP I00107466  Change Processing for Allow Multiple 
        // Adjustments
        local.AlreadyAdjustedSwitch.Flag = "Y";
        local.TotalAdjustmentForCol.TotalCurrency += entities.
          AdjustingReceiptRefund.Amount;

ReadEach:
        ;
      }

      // * * * * * * * * * * * * * *
      // 11/29/00 SWSRPDP I00107466  Change Processing for Allow Multiple 
      // Adjustments
      // 06/09/99 - Determine the TOTAL availiable for adjustment
      local.TotAmtOfCollForAdj.TotalCurrency =
        entities.OriginalToBeAdjustedCashReceiptDetail.CollectionAmount - local
        .TotalAdjustmentForCol.TotalCurrency;

      // 03/13/99 -- SWSRPDP  ---- If there is NOT enough Collection AMT on this
      // CR_Detail to cover the adjustment, Read the NEXT CR_Detail
      if (local.TotAmtOfCollForAdj.TotalCurrency >= import
        .Collection.DetailAdjustmentAmt.TotalCurrency)
      {
        // If there is enough Collection_Amount
        // 
        // to cover the Adjustment_Amount
        // SAVE the keys and Continue
        local.ProcessCashReceipt.SequentialNumber =
          entities.OriginalToBeAdjustedCashReceipt.SequentialNumber;
        local.ProcessCashReceiptDetail.SequentialIdentifier =
          entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier;

        break;
      }
      else
      {
        // If there is NOT enough Collection_Amount
        // 
        // to cover the Adjustment_Amount
        // Get the NEXT availiable Cash_Receipt_Detail
        continue;
      }
    }

    if (entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier == 0
      || local.TotAmtOfCollForAdj.TotalCurrency == 0 && AsChar
      (export.NoMatchOnYear.Flag) == 'Y')
    {
      // NOT FOUND
      // OR NOT FOUND for SPECIFIC DATE
      ExitState = "FN0000_COLLECTION_NOT_AVAILABLE";

      return;
    }
    else
    {
      if (local.TotAmtOfCollForAdj.TotalCurrency <= 0)
      {
        // Found - BUT ALL were Rejected and NOT Processed
        ExitState = "FN0000_COLLECTION_NOT_AVAILABLE";

        return;
      }

      if (local.TotAmtOfCollForAdj.TotalCurrency < import
        .Collection.DetailAdjustmentAmt.TotalCurrency)
      {
        // ADJUSTMENT_AMOUNT is greater than TOTAL_COLLECTION_AMOUNT
        // THIS SHOULD NEVER HAPPEN
        // ************************
        export.AmtOfCollectionsCalc.TotalCurrency =
          local.TotAmtOfCollForAdj.TotalCurrency;
        ExitState = "FN0000_ADJUSTMENT_AMOUNT_2_HIGH";

        return;
      }

      // This Cash Receipt Detail will be processed
    }

    // ADJUSTMENT_AMOUNT is for EXACTLY the COLLECTED_AMOUNT
    if (local.TotAmtOfCollForAdj.TotalCurrency == import
      .Collection.DetailAdjustmentAmt.TotalCurrency && local
      .TotAmtOfCollForAdj.TotalCurrency == entities
      .OriginalToBeAdjustedCashReceiptDetail.CollectionAmount)
    {
      // * * *   Total Adjustments
      // *****
      // Read Each of the Cash Receipt Details being adjusted.
      // *****
      local.TotAmtOfCollForAdj.TotalCurrency = 0;

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // *
      // 01/21/1999  SWSRPDP  Added Relationship
      // AND DESIRED original cash_receipt_detail is_part_of
      // DESIRED original cash_receipt
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // *
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // *
      foreach(var item in ReadCashReceiptDetailCashReceipt1())
      {
        // * * * * * * * * * * * * * * * * * * * * * * * * * *
        // *****
        // WR010504-B - Retro Processing - Create EVENT if "P"rotected 
        // Collection is Adjusted
        // *****
        // Read collection and Create ALERT "IF" Collection is Protected
        local.ProtColFound.Flag = "N";

        if (ReadCollection())
        {
          local.ProtColFound.Flag = "Y";
        }

        ++export.ImportNumberOfReads.Count;
        local.Original.Assign(entities.OriginalToBeAdjustedCashReceipt);

        // *****
        // Reread the Original Key Cash Receipt Detail so the entity view may be
        // view matched to persistent views in subsequent CABs.  Although this
        // may seem redundant, it will save on the total number of reads in the
        // long run.
        // *****
        if (ReadCashReceiptDetail3())
        {
          ++export.ImportNumberOfReads.Count;

          if (ReadCashReceiptTypeCashReceiptEvent())
          {
            ++export.ImportNumberOfReads.Count;
          }
          else
          {
            ExitState = "FN0113_CASH_RCPT_TYPE_NF";

            return;
          }
        }
        else
        {
          ExitState = "FN0124_ORIGINAL_CASH_RCPT_ADJ_NF";

          return;
        }

        // *****
        // Determine if the original cash receipt detail is associated to a 
        // collection type.  If it is, associate the adjustment CRD to the same
        // collection type.
        // *****
        UseFnReadCollectionTypeViaCrd();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else if (IsExitState("FN0000_COLLECTION_TYPE_NF"))
        {
          // *****  Continue processing.  Since the original is not associated 
          // to a Collection Type, the adjustment CRD will also not be
          // associated to one.
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          return;
        }

        // *****
        // Create the Adjustment Cash Receipt Detail.
        // *****
        MoveCashReceiptDetail2(entities.OriginalToBeAdjustedCashReceiptDetail,
          local.AdjustingCashReceiptDetail);
        local.AdjustingCashReceiptDetail.AdjustmentInd = "Y";
        ReadCashReceiptDetail2();
        local.AdjustingCashReceiptDetail.SequentialIdentifier =
          entities.MaxId.SequentialIdentifier + 1;
        ++export.ImportNextCheckId.SequentialIdentifier;
        UseRecordCollection();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        // *****
        // Create the Cash Receipt Detail Balance Adjustment.
        // *****
        if (!ReadCashReceiptDetail1())
        {
          ExitState = "FN0000_ADJUSTMENT_CR_DETAIL_NF";

          return;
        }

        local.PassToBalAdj.Name = global.UserId;
        UseFnCreateCashRcptDtlBalAdj();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        // *****
        // Apply Adjustment to the Cash Receipts.
        // *****
        // **** Create/Update cash receipt balance adjustment ****
        if (!ReadCashReceiptRlnRsn())
        {
          ExitState = "FN0093_CASH_RCPT_RLN_RSN_NF";

          return;
        }

        if (ReadCashReceiptBalanceAdjustment())
        {
          UpdateCashReceiptBalanceAdjustment();
        }
        else
        {
          // *** Create one ***
          CreateCashReceiptBalanceAdjustment();
        }

        // *****
        // If the Distributed amount on the original Cash Receipt Detail is 
        // greater than Zero, mark all of the collections for this CRD for
        // adjustment.
        // *****
        if (Lt(0,
          entities.OriginalToBeAdjustedCashReceiptDetail.DistributedAmount))
        {
          if (ReadCollectionAdjustmentReason())
          {
            local.CollectionAdjustmentReason.Assign(
              entities.CollectionAdjustmentReason);
            local.AdjustedCollection.AdjustedInd = "Y";
            local.AdjustedCollection.CollectionAdjustmentDt = import.Save.Date;
            local.AdjustedCollection.CollectionAdjustmentReasonTxt =
              entities.CollectionAdjustmentReason.Name;

            // * * *
            // Changed from collection_adjustment_reason name
            local.AdjustedCollection.CollectionAdjustmentReasonTxt =
              local.CashReceiptDetailRlnRsn.Name;
          }
          else
          {
            ExitState = "FN0000_COLL_ADJUST_REASON_NF";

            return;
          }

          local.DistributionProgramProcessingInfo.ProcessDate =
            import.ProgramProcessingInfo.ProcessDate;
          local.DistributionProgramProcessingInfo.Name = global.UserId;
          local.DistributionCollection.CollectionAdjustmentDt =
            entities.OriginalToBeAdjustedCashReceiptDetail.CollectionDate;
          local.DistributionCollection.CollectionAdjustmentReasonTxt =
            entities.CollectionAdjustmentReason.Name;

          // * * *
          // Changed from collection_adjustment_reason name
          local.DistributionCollection.CollectionAdjustmentReasonTxt =
            local.CashReceiptDetailRlnRsn.Name;
          local.DistributionObligor.PgmChgEffectiveDate =
            entities.OriginalToBeAdjustedCashReceiptDetail.CollectionDate;

          // 06/09/99 -- Changed Passed Cash_Receipt_Detail_Status
          // -- From "RECORDED" to "ADJUSTED"
          UseFnCabReverseOneCshRcptDtl2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }
        }
        else
        {
          // NO Distributed Amount
          // 06/10/99 ADD A/B to Record the CR_DETAIL_History
          UseFnAbRecordCrDetailHistory1();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          // UPDATE the Status History of the ORIGINAL cash_receipt_detail
          local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
            local.HardcodedViews.HardcodedAdjusted.SystemGeneratedIdentifier;
          UseFnChangeCashRcptDtlStatHis2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }
        }

        // 03/13/99  Set Fully_Applied_Indicator to 'Y'
        //         and Set the REFUNDED Amount to ZERO
        // *
        // This is for "ALL" Full Adjustments
        // *
        try
        {
          UpdateCashReceiptDetail1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // * * * * * * * * * * * * * * * * * * * * * * * * * *
        // *****
        // WR010504-B - Retro Processing - Create EVENT if "P"rotected 
        // Collection is Adjusted
        // *****
        // Read collection and Create ALERT "IF" Collection is Protected
        if (AsChar(local.ProtColFound.Flag) == 'Y')
        {
          // * A PROTECTED Collection has been Found
          ExitState = "ACO_NN0000_ALL_OK";
          UseFnRaiseProtCrdAdjustEvent();

          // IF AN EVENT was NOT created (event_created = N)
          // for this Protected Cash Receipt Detail  (Protected Found = Y)
          // * Reset the Exit State so the ERROR will appear on the ERROR Report
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // * EVENT Created
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else if (IsExitState("CASE_NF"))
          {
            // * NO Match on CASE - NO EVENT Created
            ExitState = "CASE_NF";
          }
          else if (IsExitState("CSE_PERSON_NF"))
          {
            // * NO Person or Court Order Information - NO EVENT Created
            ExitState = "CSE_PERSON_NF_RB";
          }
          else
          {
            // * Error Creating EVENT - NO Flags set
            ExitState = "SP0000_EVENT_NF";
          }
        }
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * *
      // Accumulate TOTALS for Reporting
      ++export.AdjAdjCnt.Count;
      export.AdjAdjAmt.TotalCurrency += import.Collection.DetailAdjustmentAmt.
        TotalCurrency;

      if (import.CashReceiptEvent.SystemGeneratedIdentifier == entities
        .OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier)
      {
        ++export.AdjAdjCurrCnt.Count;
        export.AdjAdjAmtCurr.TotalCurrency += import.Collection.
          DetailAdjustmentAmt.TotalCurrency;
      }
      else
      {
        ++export.AdjAdjPrevCnt.Count;
        export.AdjAdjAmtPrev.TotalCurrency += import.Collection.
          DetailAdjustmentAmt.TotalCurrency;
      }
    }
    else
    {
      // * * *    Partial Adjustments
      // ADJUSTMENT_AMOUNT is LESS than TOTAL_COLLECTION_AMOUNT
      // *****
      // Read Each of the Cash Receipt Details being adjusted.
      // *****
      // This COULD have been done once for both Full and Partial Adjustments
      // But was done in Each case to keep ALL logic together
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // *
      // 01/21/1999  SWSRPDP  Added Relationship
      // AND DESIRED original cash_receipt_detail is_part_of
      // DESIRED original cash_receipt
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // *
      foreach(var item in ReadCashReceiptDetailCashReceipt1())
      {
        ++export.ImportNumberOfReads.Count;
        local.Original.Assign(entities.OriginalToBeAdjustedCashReceipt);

        // * * * * * * * * * * * * * * * * * * * * * * * * * *
        // *****
        // WR010504-B - Retro Processing - Create EVENT if "P"rotected 
        // Collection is Adjusted
        // *****
        // Read collection and Create ALERT "IF" Collection is Protected
        local.ProtColFound.Flag = "N";

        if (ReadCollection())
        {
          local.ProtColFound.Flag = "Y";
        }

        // *****
        // Reread the Original Key Cash Receipt Detail so the entity view may be
        // view matched to persistent views in subsequent CABs.  Although this
        // may seem redundant, it will save on the total number of reads in the
        // long run.
        // *****
        if (ReadCashReceiptDetail3())
        {
          ++export.ImportNumberOfReads.Count;

          if (ReadCashReceiptTypeCashReceiptEvent())
          {
            ++export.ImportNumberOfReads.Count;
          }
          else
          {
            ExitState = "FN0113_CASH_RCPT_TYPE_NF";

            return;
          }
        }
        else
        {
          ExitState = "FN0124_ORIGINAL_CASH_RCPT_ADJ_NF";

          return;
        }

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // Create the Receipt_Refund for this Cash_Receipt_Detail
        // ****  NO ****  PAYMENT_REQUEST will be Created
        // for this Receipt_Refund
        // ALWAYS Create the RECEIPT_REFUND
        // for the TOTAL ADJUSTMENT AMOUNT
        if (!ReadCsePerson2())
        {
          // test pdp 12/08/00
          if (!ReadCsePerson1())
          {
            ExitState = "CSE_PERSON_NF";

            return;
          }
        }

        if (!ReadCashReceiptDetailAddress())
        {
          ExitState = "CSE_PERSON_ADDRESS_NF_RB";

          return;
        }

        // 04/06/99  SWSRPDP    Changed MISC to FED ADJ
        local.Adjustment.ReasonCode = "FED ADJ";
        local.Adjustment.Taxid =
          entities.OriginalToBeAdjustedCashReceiptDetail.
            ObligorSocialSecurityNumber;
        local.Adjustment.PayeeName =
          TrimEnd(entities.OriginalToBeAdjustedCashReceiptDetail.ObligorLastName)
          + ",  " + TrimEnd
          (entities.OriginalToBeAdjustedCashReceiptDetail.ObligorFirstName);
        local.Adjustment.PayeeName = TrimEnd(local.Adjustment.PayeeName) + " " +
          TrimEnd
          (entities.OriginalToBeAdjustedCashReceiptDetail.ObligorMiddleName);
        local.Adjustment.OffsetTaxYear =
          entities.OriginalToBeAdjustedCashReceiptDetail.OffsetTaxYear;
        local.Adjustment.RequestDate = import.Save.Date;
        local.Adjustment.ReasonText =
          "NO WARRANT ISSUED AS ALREADY REFUNDED BY FEDERAL GOVERNMENT.";
        local.Adjustment.Amount =
          import.Collection.DetailAdjustmentAmt.TotalCurrency;

        try
        {
          CreateReceiptRefund();

          if (ReadCollectionType())
          {
            AssociateReceiptRefund();
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_RCPT_REFUND_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_RCPT_REFUND_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // 05/10/99 -- New Partial Adjustment Logic -- HERE
        // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        // The ADJUSTMENTS are to be APPLIED in this order
        // 1) Undistributed
        // Amount
        // 
        // 2) Distributed Amount
        // 
        // 3) Refunded Amount
        // All three sets of logic are VERY similar and could have
        // been done in ONE set of logic - however for ease of
        // understnding and MAINTENCE/future changes which
        // might affect ONLY ONE of the conditions - the logic
        // was divided as stated above.
        // 1)  Does UNDISTRIBUTED Amount Cover Adjustment
        // UNDISTRIBUTED is whats left after
        // 
        // subtracting Distributed and Refunded
        // 
        // from the Collection Amount - Regardless of STATUS.
        local.Difference.TotalCurrency =
          entities.OriginalToBeAdjustedCashReceiptDetail.CollectionAmount - entities
          .OriginalToBeAdjustedCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() - entities
          .OriginalToBeAdjustedCashReceiptDetail.RefundedAmount.
            GetValueOrDefault();

        if (local.Difference.TotalCurrency >= import
          .Collection.DetailAdjustmentAmt.TotalCurrency)
        {
          // UPDATE the Status History of the ORIGINAL cash_receipt_detail
          if (local.Difference.TotalCurrency == import
            .Collection.DetailAdjustmentAmt.TotalCurrency)
          {
            // ALL Undistributed Amount Required
            // UPDATE the Status of the Cash_Receipt_Detail to 'REFUNDED'
            local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
              local.HardcodedViews.HardcodedRefunded.SystemGeneratedIdentifier;
            local.Update.CollectionAmtFullyAppliedInd = "Y";
          }
          else
          {
            // Some Undistributed Amount left - Will be Re-Distributed
            // UPDATE the Status of the Cash_Receipt_Detail to 'RELEASED'
            // Set the fully applied flag to space
            // so Remaining Undistributed will be Distributed
            local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
              local.HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier;
            local.Update.CollectionAmtFullyAppliedInd = "";
          }

          // NO Distributed Amount
          // 06/10/99 ADD A/B to Record the CR_DETAIL_History
          UseFnAbRecordCrDetailHistory2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          UseFnChangeCashRcptDtlStatHis1();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          // 03/13/99  --- SWSRPDP
          local.Calculating.RefundedAmount =
            entities.OriginalToBeAdjustedCashReceiptDetail.RefundedAmount.
              GetValueOrDefault() + import
            .Collection.DetailAdjustmentAmt.TotalCurrency;

          // 06/12/99  --- IF Calculated REFUNDED amount
          // is GREATER than the Collection Amount
          // SET the REFUNDED AMOUNT
          // to the Collection Amount
          // This SHOULD NEVER occur at this point
          if (local.Calculating.RefundedAmount.GetValueOrDefault() > entities
            .OriginalToBeAdjustedCashReceiptDetail.CollectionAmount)
          {
            // Refunded Amount should NOT be greater than the Amount Collected
            local.Calculating.RefundedAmount =
              entities.OriginalToBeAdjustedCashReceiptDetail.CollectionAmount;
          }

          try
          {
            UpdateCashReceiptDetail2();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                break;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          // 03/13/99  --- SWSRPDP
          export.AmtOfCollectionsCalc.TotalCurrency =
            local.TotAmtOfCollForAdj.TotalCurrency;
          ExitState = "FN0000_COLL_ADJUSTMENT_ADDED";

          break;
        }

        // 2) Does UNDISTRIBUTED + DISTRIBUTED Amount Cover Adjustment
        local.Difference.TotalCurrency =
          entities.OriginalToBeAdjustedCashReceiptDetail.CollectionAmount - entities
          .OriginalToBeAdjustedCashReceiptDetail.RefundedAmount.
            GetValueOrDefault();

        if (local.Difference.TotalCurrency >= import
          .Collection.DetailAdjustmentAmt.TotalCurrency)
        {
          // UPDATE the Status History of the ORIGINAL cash_receipt_detail
          if (local.Difference.TotalCurrency == import
            .Collection.DetailAdjustmentAmt.TotalCurrency)
          {
            // NO Undistributed Amount left
            // UPDATE the Status of the Cash_Receipt_Detail to 'REFUNDED'
            local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
              local.HardcodedViews.HardcodedRefunded.SystemGeneratedIdentifier;
            local.Update.CollectionAmtFullyAppliedInd = "Y";
          }
          else
          {
            // Some Undistributed Amount left - Will be Re-Distributed
            // UPDATE the Status of the Cash_Receipt_Detail to 'RELEASED'
            // Set the fully applied flag to space
            // so Remaining Undistributed will be Distributed
            local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
              local.HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier;
            local.Update.CollectionAmtFullyAppliedInd = "";
          }

          // *****
          // If the Distributed amount on the original Cash Receipt Detail is 
          // greater than Zero, mark all of the collections for this CRD for
          // adjustment.
          // *****
          if (Lt(0,
            entities.OriginalToBeAdjustedCashReceiptDetail.DistributedAmount))
          {
            if (ReadCollectionAdjustmentReason())
            {
              local.CollectionAdjustmentReason.Assign(
                entities.CollectionAdjustmentReason);
              local.AdjustedCollection.AdjustedInd = "Y";
              local.AdjustedCollection.CollectionAdjustmentDt =
                import.Save.Date;
              local.AdjustedCollection.CollectionAdjustmentReasonTxt =
                entities.CollectionAdjustmentReason.Name;
            }
            else
            {
              ExitState = "FN0000_COLL_ADJUST_REASON_NF";

              return;
            }

            local.DistributionProgramProcessingInfo.ProcessDate =
              import.ProgramProcessingInfo.ProcessDate;
            local.DistributionProgramProcessingInfo.Name = global.UserId;
            local.DistributionCollection.CollectionAdjustmentDt =
              entities.OriginalToBeAdjustedCashReceiptDetail.CollectionDate;
            local.DistributionCollection.CollectionAdjustmentReasonTxt =
              entities.CollectionAdjustmentReason.Name;
            local.DistributionObligor.PgmChgEffectiveDate =
              entities.OriginalToBeAdjustedCashReceiptDetail.CollectionDate;
            UseFnCabReverseOneCshRcptDtl1();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }
          }
          else
          {
            // NO Distributed Amount
            // 06/10/99 ADD A/B to Record the CR_DETAIL_History
            UseFnAbRecordCrDetailHistory1();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }

            // UPDATE the Status History of the ORIGINAL cash_receipt_detail
            UseFnChangeCashRcptDtlStatHis1();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }
          }

          // 03/13/99  --- SWSRPDP
          local.Calculating.RefundedAmount =
            entities.OriginalToBeAdjustedCashReceiptDetail.RefundedAmount.
              GetValueOrDefault() + import
            .Collection.DetailAdjustmentAmt.TotalCurrency;

          // 06/12/99  --- IF Calculated REFUNDED amount
          // is GREATER than the Collection Amount
          // SET the REFUNDED AMOUNT
          // to the Collection Amount
          // This SHOULD NEVER occur at this point
          if (local.Calculating.RefundedAmount.GetValueOrDefault() > entities
            .OriginalToBeAdjustedCashReceiptDetail.CollectionAmount)
          {
            // Refunded Amount should NOT be greater than the Amount Collected
            local.Calculating.RefundedAmount =
              entities.OriginalToBeAdjustedCashReceiptDetail.CollectionAmount;
          }

          try
          {
            UpdateCashReceiptDetail2();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                break;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          // 03/13/99  --- SWSRPDP
          export.AmtOfCollectionsCalc.TotalCurrency =
            local.TotAmtOfCollForAdj.TotalCurrency;
          ExitState = "FN0000_COLL_ADJUSTMENT_ADDED";

          break;
        }

        // 3) REFUNDED Amount is Needed to Cover Adjustment
        local.Difference.TotalCurrency =
          entities.OriginalToBeAdjustedCashReceiptDetail.CollectionAmount - entities
          .OriginalToBeAdjustedCashReceiptDetail.RefundedAmount.
            GetValueOrDefault();

        if (local.Difference.TotalCurrency < import
          .Collection.DetailAdjustmentAmt.TotalCurrency)
        {
          // UPDATE the Status History of the ORIGINAL cash_receipt_detail
          // UPDATE the Status of the Cash_Receipt_Detail to 'REFUNDED'
          local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
            local.HardcodedViews.HardcodedRefunded.SystemGeneratedIdentifier;
          local.Update.CollectionAmtFullyAppliedInd = "Y";

          // *****
          // If the Distributed amount on the original Cash Receipt Detail is 
          // greater than Zero, mark all of the collections for this CRD for
          // adjustment.
          // *****
          if (Lt(0,
            entities.OriginalToBeAdjustedCashReceiptDetail.DistributedAmount))
          {
            if (ReadCollectionAdjustmentReason())
            {
              local.CollectionAdjustmentReason.Assign(
                entities.CollectionAdjustmentReason);
              local.AdjustedCollection.AdjustedInd = "Y";
              local.AdjustedCollection.CollectionAdjustmentDt =
                import.Save.Date;
              local.AdjustedCollection.CollectionAdjustmentReasonTxt =
                entities.CollectionAdjustmentReason.Name;
            }
            else
            {
              ExitState = "FN0000_COLL_ADJUST_REASON_NF";

              return;
            }

            local.DistributionProgramProcessingInfo.ProcessDate =
              import.ProgramProcessingInfo.ProcessDate;
            local.DistributionProgramProcessingInfo.Name = global.UserId;
            local.DistributionCollection.CollectionAdjustmentDt =
              entities.OriginalToBeAdjustedCashReceiptDetail.CollectionDate;
            local.DistributionCollection.CollectionAdjustmentReasonTxt =
              entities.CollectionAdjustmentReason.Name;
            local.DistributionObligor.PgmChgEffectiveDate =
              entities.OriginalToBeAdjustedCashReceiptDetail.CollectionDate;
            UseFnCabReverseOneCshRcptDtl1();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }
          }
          else
          {
            // NO Distributed Amount
            // 06/10/99 ADD A/B to Record the CR_DETAIL_History
            UseFnAbRecordCrDetailHistory1();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }

            UseFnChangeCashRcptDtlStatHis1();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }
          }

          // 06/12/99  --- IF Calculated REFUNDED amount
          // SET the REFUNDED AMOUNT to COLLECTION_AMOUNT
          // Refunded Amount should NOT be greater than the Amount Collected
          local.Calculating.RefundedAmount =
            entities.OriginalToBeAdjustedCashReceiptDetail.CollectionAmount;

          try
          {
            UpdateCashReceiptDetail2();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                break;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          // 03/13/99  --- SWSRPDP
          export.AmtOfCollectionsCalc.TotalCurrency =
            local.TotAmtOfCollForAdj.TotalCurrency;
          ExitState = "FN0000_COLL_ADJUSTMENT_ADDED";

          break;
        }

        // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        // 05/10/99 -- New Partial Adjustment Logic -- HERE
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * * *
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * *
      // Accumulate TOTALS for Reporting
      ++export.AdjRefCnt.Count;
      export.AdjRefAmt.TotalCurrency += import.Collection.DetailAdjustmentAmt.
        TotalCurrency;

      if (import.CashReceiptEvent.SystemGeneratedIdentifier == entities
        .OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier)
      {
        ++export.AdjRefCurrCnt.Count;
        export.AdjRefAmtCurr.TotalCurrency += import.Collection.
          DetailAdjustmentAmt.TotalCurrency;
      }
      else
      {
        ++export.AdjRefPrevCnt.Count;
        export.AdjRefAmtPrev.TotalCurrency += import.Collection.
          DetailAdjustmentAmt.TotalCurrency;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * *
      // *****
      // WR010504-B - Retro Processing - Create EVENT if "P"rotected Collection 
      // is Adjusted
      // *****
      // Read collection and Create ALERT "IF" Collection is Protected
      if (AsChar(local.ProtColFound.Flag) == 'Y')
      {
        // * A PROTECTED Collection has been Found
        ExitState = "ACO_NN0000_ALL_OK";
        UseFnRaiseProtCrdAdjustEvent();

        // IF AN EVENT was NOT created (event_created = N)
        // for this Protected Cash Receipt Detail  (Protected Found = Y)
        // * Reset the Exit State so the ERROR will appear on the ERROR Report
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // * EVENT Created
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (IsExitState("CASE_NF"))
        {
          // * NO Match on CASE - NO EVENT Created
          ExitState = "CASE_NF";
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          // * NO Person or Court Order Information - NO EVENT Created
          ExitState = "CSE_PERSON_NF_RB";
        }
        else
        {
          // * Error Creating EVENT - NO Flags set
          ExitState = "SP0000_EVENT_NF";
        }
      }
    }

    // *****
    // Accumulate the totals on the Adjustment Cash Receipt Increase).  None of 
    // the transaction counts need to be incremented since the Courts do not
    // include adjustment amounts in the Adjusting CR transaction totals.
    // *****
    ++export.Swefb612AdjustCount.Count;
    export.Swefb612AdjustmentAmt.TotalCurrency += import.Collection.
      DetailAdjustmentAmt.TotalCurrency;
    export.Swefb612NetAmount.TotalCurrency -= import.Collection.
      DetailAdjustmentAmt.TotalCurrency;
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmtFullyAppliedInd = source.CollectionAmtFullyAppliedInd;
    target.InterfaceTransId = source.InterfaceTransId;
    target.AdjustmentInd = source.AdjustmentInd;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.InterfaceTransId = source.InterfaceTransId;
    target.AdjustmentInd = source.AdjustmentInd;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CaseNumber = source.CaseNumber;
    target.OffsetTaxid = source.OffsetTaxid;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
    target.ObligorPhoneNumber = source.ObligorPhoneNumber;
    target.InjuredSpouseInd = source.InjuredSpouseInd;
  }

  private static void MoveCashReceiptDetail3(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
  }

  private static void MoveCashReceiptDetail4(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.RefundedAmount = source.RefundedAmount;
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CrtIdentifier = source.CrtIdentifier;
    target.CstIdentifier = source.CstIdentifier;
    target.CrvIdentifier = source.CrvIdentifier;
  }

  private static void MoveCollection1(Collection source, Collection target)
  {
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CollectionAdjustmentReasonTxt = source.CollectionAdjustmentReasonTxt;
  }

  private static void MoveCollection2(Collection source, Collection target)
  {
    target.CourtOrderAppliedTo = source.CourtOrderAppliedTo;
    target.ArNumber = source.ArNumber;
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnAbRecordCrDetailHistory1()
  {
    var useImport = new FnAbRecordCrDetailHistory.Import();
    var useExport = new FnAbRecordCrDetailHistory.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      entities.OriginalToBeAdjustedCashReceipt.SequentialNumber;
    useImport.CollectionType.SequentialIdentifier =
      local.CollectionType.SequentialIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedEft.SystemGeneratedIdentifier;

    Call(FnAbRecordCrDetailHistory.Execute, useImport, useExport);
  }

  private void UseFnAbRecordCrDetailHistory2()
  {
    var useImport = new FnAbRecordCrDetailHistory.Import();
    var useExport = new FnAbRecordCrDetailHistory.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.OriginalKeyCashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber = local.Original.SequentialNumber;
    useImport.CollectionType.SequentialIdentifier =
      local.CollectionType.SequentialIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedEft.SystemGeneratedIdentifier;

    Call(FnAbRecordCrDetailHistory.Execute, useImport, useExport);
  }

  private void UseFnCabReverseOneCshRcptDtl1()
  {
    var useImport = new FnCabReverseOneCshRcptDtl.Import();
    var useExport = new FnCabReverseOneCshRcptDtl.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    MoveCashReceiptDetail1(entities.OriginalToBeAdjustedCashReceiptDetail,
      useImport.CashReceiptDetail);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      entities.OriginalToBeAdjustedCashReceipt.SequentialNumber;
    MoveProgramProcessingInfo(local.DistributionProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.CollectionAdjustmentReason.SystemGeneratedIdentifier =
      local.CollectionAdjustmentReason.SystemGeneratedIdentifier;
    MoveCollection1(local.AdjustedCollection, useImport.Collection);
    useImport.Max.Date = local.Max.Date;
    useImport.RecAdjStatus.SystemGeneratedIdentifier =
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.Obligor.PgmChgEffectiveDate =
      local.DistributionObligor.PgmChgEffectiveDate;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedEft.SystemGeneratedIdentifier;

    Call(FnCabReverseOneCshRcptDtl.Execute, useImport, useExport);
  }

  private void UseFnCabReverseOneCshRcptDtl2()
  {
    var useImport = new FnCabReverseOneCshRcptDtl.Import();
    var useExport = new FnCabReverseOneCshRcptDtl.Export();

    MoveCashReceiptDetail1(entities.OriginalToBeAdjustedCashReceiptDetail,
      useImport.CashReceiptDetail);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier;
    MoveProgramProcessingInfo(local.DistributionProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.CollectionAdjustmentReason.SystemGeneratedIdentifier =
      local.CollectionAdjustmentReason.SystemGeneratedIdentifier;
    MoveCollection1(local.AdjustedCollection, useImport.Collection);
    useImport.Max.Date = local.Max.Date;
    useImport.Obligor.PgmChgEffectiveDate =
      local.DistributionObligor.PgmChgEffectiveDate;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedEft.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      entities.OriginalToBeAdjustedCashReceipt.SequentialNumber;
    useImport.RecAdjStatus.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedAdjusted.SystemGeneratedIdentifier;

    Call(FnCabReverseOneCshRcptDtl.Execute, useImport, useExport);
  }

  private void UseFnChangeCashRcptDtlStatHis1()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.Persistent.Assign(entities.OriginalKeyCashReceiptDetail);
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedEft.SystemGeneratedIdentifier;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);

    MoveCashReceiptDetail(useImport.Persistent,
      entities.OriginalKeyCashReceiptDetail);
  }

  private void UseFnChangeCashRcptDtlStatHis2()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.Persistent.Assign(entities.OriginalKeyCashReceiptDetail);
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedEft.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedAdjusted.SystemGeneratedIdentifier;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);

    MoveCashReceiptDetail(useImport.Persistent,
      entities.OriginalKeyCashReceiptDetail);
  }

  private void UseFnCreateCashRcptDtlBalAdj()
  {
    var useImport = new FnCreateCashRcptDtlBalAdj.Import();
    var useExport = new FnCreateCashRcptDtlBalAdj.Export();

    useImport.CashReceiptDetailRlnRsn.SequentialIdentifier =
      local.CashReceiptDetailRlnRsn.SequentialIdentifier;
    useImport.AdjustedCashReceipt.SequentialNumber =
      entities.OriginalToBeAdjustedCashReceipt.SequentialNumber;
    useImport.PersistentAdjusting.Assign(entities.AdjustingCashReceiptDetail);
    useImport.AdjustingCashReceipt.SequentialNumber =
      local.AdjustingCashReceipt.SequentialNumber;
    MoveCashReceiptDetail4(entities.OriginalToBeAdjustedCashReceiptDetail,
      useImport.AdjustedCashReceiptDetail);
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnCreateCashRcptDtlBalAdj.Execute, useImport, useExport);

    entities.AdjustingCashReceiptDetail.Assign(useImport.PersistentAdjusting);
    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedTaxAdj.SequentialIdentifier =
      useExport.CrdTaxAdj.SequentialIdentifier;
    local.ViewHardcodedViews.HardcodedOver.CashBalanceReason =
      useExport.CrCashBalanceReason.CrOver.CashBalanceReason;
    local.ViewHardcodedViews.HardcodedUnder.CashBalanceReason =
      useExport.CrCashBalanceReason.CrUnder.CashBalanceReason;
    local.ViewHardcodedViews.HardcodedGeneralError.SystemGeneratedIdentifier =
      useExport.CrRlnRsnSystemId.CrrrCourtInterface.SystemGeneratedIdentifier;
    local.ViewHardcodedViews.HardcodedRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedRefunded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
  }

  private void UseFnRaiseProtCrdAdjustEvent()
  {
    var useImport = new FnRaiseProtCrdAdjustEvent.Import();
    var useExport = new FnRaiseProtCrdAdjustEvent.Export();

    MoveCollection2(entities.Protected1, useImport.Collection);
    MoveCashReceiptDetail3(entities.OriginalToBeAdjustedCashReceiptDetail,
      useImport.CashReceiptDetail);
    useImport.CashReceipt.SequentialNumber =
      entities.OriginalToBeAdjustedCashReceipt.SequentialNumber;

    Call(FnRaiseProtCrdAdjustEvent.Execute, useImport, useExport);
  }

  private void UseFnReadCollectionTypeViaCrd()
  {
    var useImport = new FnReadCollectionTypeViaCrd.Import();
    var useExport = new FnReadCollectionTypeViaCrd.Export();

    useImport.Persistent.Assign(entities.OriginalKeyCashReceiptDetail);
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      entities.OriginalToBeAdjustedCashReceipt.SequentialNumber;

    Call(FnReadCollectionTypeViaCrd.Execute, useImport, useExport);

    entities.OriginalKeyCashReceiptDetail.Assign(useImport.Persistent);
    MoveCollectionType(useExport.CollectionType, local.CollectionType);
  }

  private void UseRecordCollection()
  {
    var useImport = new RecordCollection.Import();
    var useExport = new RecordCollection.Export();

    useImport.PersistentCashReceipt.Assign(entities.Adjustment);
    MoveCollectionType(local.CollectionType, useImport.CollectionType);
    useImport.CashReceipt.SequentialNumber =
      local.AdjustingCashReceipt.SequentialNumber;
    MoveCashReceiptDetail2(local.AdjustingCashReceiptDetail,
      useImport.CashReceiptDetail);
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedRecorded.SystemGeneratedIdentifier;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(RecordCollection.Execute, useImport, useExport);

    entities.Adjustment.SequentialNumber =
      useImport.PersistentCashReceipt.SequentialNumber;
    MoveCashReceiptDetail2(useExport.CashReceiptDetail,
      local.AdjustingCashReceiptDetail);
    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void AssociateReceiptRefund()
  {
    var cltIdentifier = entities.CollectionType.SequentialIdentifier;

    entities.AdjustingReceiptRefund.Populated = false;
    Update("AssociateReceiptRefund",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cltIdentifier", cltIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.AdjustingReceiptRefund.CreatedTimestamp.GetValueOrDefault());
          
      });

    entities.AdjustingReceiptRefund.CltIdentifier = cltIdentifier;
    entities.AdjustingReceiptRefund.Populated = true;
  }

  private void CreateCashReceiptBalanceAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.Adjustment.Populated);
    System.Diagnostics.Debug.Assert(
      entities.OriginalToBeAdjustedCashReceipt.Populated);

    var crtIdentifier = entities.OriginalToBeAdjustedCashReceipt.CrtIdentifier;
    var cstIdentifier = entities.OriginalToBeAdjustedCashReceipt.CstIdentifier;
    var crvIdentifier = entities.OriginalToBeAdjustedCashReceipt.CrvIdentifier;
    var crtIIdentifier = entities.Adjustment.CrtIdentifier;
    var cstIIdentifier = entities.Adjustment.CstIdentifier;
    var crvIIdentifier = entities.Adjustment.CrvIdentifier;
    var crrIdentifier = entities.CashReceiptRlnRsn.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var adjustmentAmount = local.AdjustingCashReceiptDetail.CollectionAmount;
    var createdBy = global.UserId;

    entities.CashReceiptBalanceAdjustment.Populated = false;
    Update("CreateCashReceiptBalanceAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "crtIIdentifier", crtIIdentifier);
        db.SetInt32(command, "cstIIdentifier", cstIIdentifier);
        db.SetInt32(command, "crvIIdentifier", crvIIdentifier);
        db.SetInt32(command, "crrIdentifier", crrIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetDecimal(command, "adjustmentAmount", adjustmentAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "description", "");
      });

    entities.CashReceiptBalanceAdjustment.CrtIdentifier = crtIdentifier;
    entities.CashReceiptBalanceAdjustment.CstIdentifier = cstIdentifier;
    entities.CashReceiptBalanceAdjustment.CrvIdentifier = crvIdentifier;
    entities.CashReceiptBalanceAdjustment.CrtIIdentifier = crtIIdentifier;
    entities.CashReceiptBalanceAdjustment.CstIIdentifier = cstIIdentifier;
    entities.CashReceiptBalanceAdjustment.CrvIIdentifier = crvIIdentifier;
    entities.CashReceiptBalanceAdjustment.CrrIdentifier = crrIdentifier;
    entities.CashReceiptBalanceAdjustment.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptBalanceAdjustment.AdjustmentAmount = adjustmentAmount;
    entities.CashReceiptBalanceAdjustment.CreatedBy = createdBy;
    entities.CashReceiptBalanceAdjustment.Description = "";
    entities.CashReceiptBalanceAdjustment.Populated = true;
  }

  private void CreateReceiptRefund()
  {
    System.Diagnostics.Debug.Assert(
      entities.OriginalToBeAdjustedCashReceiptDetail.Populated);

    var createdTimestamp = Now();
    var reasonCode = local.Adjustment.ReasonCode;
    var taxid = local.Adjustment.Taxid ?? "";
    var payeeName = local.Adjustment.PayeeName ?? "";
    var amount = local.Adjustment.Amount;
    var offsetTaxYear = local.Adjustment.OffsetTaxYear.GetValueOrDefault();
    var requestDate = local.Adjustment.RequestDate;
    var createdBy = global.UserId;
    var cspNumber = entities.OriginalCsePerson.Number;
    var cstAIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    var crvIdentifier =
      entities.OriginalToBeAdjustedCashReceiptDetail.CrvIdentifier;
    var crdIdentifier =
      entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier;
    var cdaIdentifier =
      entities.OriginalCashReceiptDetailAddress.SystemGeneratedIdentifier;
    var reasonText = local.Adjustment.ReasonText ?? "";
    var crtIdentifier =
      entities.OriginalToBeAdjustedCashReceiptDetail.CrtIdentifier;
    var cstIdentifier =
      entities.OriginalToBeAdjustedCashReceiptDetail.CstIdentifier;

    entities.AdjustingReceiptRefund.Populated = false;
    Update("CreateReceiptRefund",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetNullableString(command, "taxid", taxid);
        db.SetNullableString(command, "payeeName", payeeName);
        db.SetDecimal(command, "amount", amount);
        db.SetNullableInt32(command, "offsetTaxYear", offsetTaxYear);
        db.SetDate(command, "requestDate", requestDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableInt32(command, "cstAIdentifier", cstAIdentifier);
        db.SetNullableInt32(command, "crvIdentifier", crvIdentifier);
        db.SetNullableInt32(command, "crdIdentifier", crdIdentifier);
        db.SetNullableDateTime(command, "cdaIdentifier", cdaIdentifier);
        db.SetString(command, "offsetClosed", "");
        db.SetNullableDate(command, "dateTransmitted", default(DateTime));
        db.SetNullableString(command, "taxIdSuffix", "");
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableInt32(command, "crtIdentifier", crtIdentifier);
        db.SetNullableInt32(command, "cstIdentifier", cstIdentifier);
      });

    entities.AdjustingReceiptRefund.CreatedTimestamp = createdTimestamp;
    entities.AdjustingReceiptRefund.ReasonCode = reasonCode;
    entities.AdjustingReceiptRefund.Taxid = taxid;
    entities.AdjustingReceiptRefund.PayeeName = payeeName;
    entities.AdjustingReceiptRefund.Amount = amount;
    entities.AdjustingReceiptRefund.OffsetTaxYear = offsetTaxYear;
    entities.AdjustingReceiptRefund.RequestDate = requestDate;
    entities.AdjustingReceiptRefund.CreatedBy = createdBy;
    entities.AdjustingReceiptRefund.CspNumber = cspNumber;
    entities.AdjustingReceiptRefund.CstAIdentifier = cstAIdentifier;
    entities.AdjustingReceiptRefund.CrvIdentifier = crvIdentifier;
    entities.AdjustingReceiptRefund.CrdIdentifier = crdIdentifier;
    entities.AdjustingReceiptRefund.CdaIdentifier = cdaIdentifier;
    entities.AdjustingReceiptRefund.TaxIdSuffix = "";
    entities.AdjustingReceiptRefund.ReasonText = reasonText;
    entities.AdjustingReceiptRefund.CltIdentifier = null;
    entities.AdjustingReceiptRefund.LastUpdatedBy = "";
    entities.AdjustingReceiptRefund.LastUpdatedTimestamp = null;
    entities.AdjustingReceiptRefund.CrtIdentifier = crtIdentifier;
    entities.AdjustingReceiptRefund.CstIdentifier = cstIdentifier;
    entities.AdjustingReceiptRefund.Populated = true;
  }

  private bool ReadCashReceipt()
  {
    entities.Adjustment.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId",
          local.AdjustingCashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.Adjustment.CrvIdentifier = db.GetInt32(reader, 0);
        entities.Adjustment.CstIdentifier = db.GetInt32(reader, 1);
        entities.Adjustment.CrtIdentifier = db.GetInt32(reader, 2);
        entities.Adjustment.SequentialNumber = db.GetInt32(reader, 3);
        entities.Adjustment.Populated = true;
      });
  }

  private bool ReadCashReceiptBalanceAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.Adjustment.Populated);
    System.Diagnostics.Debug.Assert(
      entities.OriginalToBeAdjustedCashReceipt.Populated);
    entities.CashReceiptBalanceAdjustment.Populated = false;

    return Read("ReadCashReceiptBalanceAdjustment",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier",
          entities.OriginalToBeAdjustedCashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.OriginalToBeAdjustedCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.OriginalToBeAdjustedCashReceipt.CrvIdentifier);
        db.
          SetInt32(command, "crtIIdentifier", entities.Adjustment.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIIdentifier", entities.Adjustment.CstIdentifier);
          
        db.
          SetInt32(command, "crvIIdentifier", entities.Adjustment.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptBalanceAdjustment.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptBalanceAdjustment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptBalanceAdjustment.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptBalanceAdjustment.CrtIIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptBalanceAdjustment.CstIIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptBalanceAdjustment.CrvIIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptBalanceAdjustment.CrrIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptBalanceAdjustment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.CashReceiptBalanceAdjustment.AdjustmentAmount =
          db.GetDecimal(reader, 8);
        entities.CashReceiptBalanceAdjustment.CreatedBy =
          db.GetString(reader, 9);
        entities.CashReceiptBalanceAdjustment.Description =
          db.GetNullableString(reader, 10);
        entities.CashReceiptBalanceAdjustment.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.Adjustment.Populated);
    entities.AdjustingCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          local.AdjustingCashReceiptDetail.SequentialIdentifier);
        db.
          SetInt32(command, "crtIdentifier", entities.Adjustment.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.Adjustment.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.Adjustment.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.AdjustingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.AdjustingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.AdjustingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.AdjustingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.AdjustingCashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.Adjustment.Populated);
    entities.MaxId.Populated = false;

    return Read("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.Adjustment.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.Adjustment.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.Adjustment.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.MaxId.CrvIdentifier = db.GetInt32(reader, 0);
        entities.MaxId.CstIdentifier = db.GetInt32(reader, 1);
        entities.MaxId.CrtIdentifier = db.GetInt32(reader, 2);
        entities.MaxId.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.MaxId.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail3()
  {
    System.Diagnostics.Debug.Assert(
      entities.OriginalToBeAdjustedCashReceipt.Populated);
    entities.OriginalKeyCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail3",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.OriginalToBeAdjustedCashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.OriginalToBeAdjustedCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.OriginalToBeAdjustedCashReceipt.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.OriginalKeyCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalKeyCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.OriginalKeyCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalKeyCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.OriginalKeyCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.OriginalKeyCashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailAddress()
  {
    System.Diagnostics.Debug.Assert(
      entities.OriginalToBeAdjustedCashReceiptDetail.Populated);
    entities.OriginalCashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.OriginalCashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.OriginalCashReceiptDetailAddress.CrtIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.OriginalCashReceiptDetailAddress.CstIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.OriginalCashReceiptDetailAddress.CrvIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.OriginalCashReceiptDetailAddress.CrdIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.OriginalCashReceiptDetailAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceipt1()
  {
    entities.OriginalToBeAdjustedCashReceiptDetail.Populated = false;
    entities.OriginalToBeAdjustedCashReceipt.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceipt1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offsetTaxYear",
          import.AdditionalInformation.Adjustment.OffsetTaxYear.
            GetValueOrDefault());
        db.SetNullableString(
          command, "oblgorSsn",
          import.Collection.Detail.ObligorSocialSecurityNumber ?? "");
        db.SetString(command, "createdBy", global.UserId);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cashReceiptId", local.ProcessCashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crdId",
          local.ProcessCashReceiptDetail.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.OriginalToBeAdjustedCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalToBeAdjustedCashReceipt.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalToBeAdjustedCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.OriginalToBeAdjustedCashReceipt.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.OriginalToBeAdjustedCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalToBeAdjustedCashReceipt.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.OriginalToBeAdjustedCashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.OriginalToBeAdjustedCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.OriginalToBeAdjustedCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.OriginalToBeAdjustedCashReceiptDetail.CaseNumber =
          db.GetNullableString(reader, 7);
        entities.OriginalToBeAdjustedCashReceiptDetail.OffsetTaxid =
          db.GetNullableInt32(reader, 8);
        entities.OriginalToBeAdjustedCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 9);
        entities.OriginalToBeAdjustedCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 10);
        entities.OriginalToBeAdjustedCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 11);
        entities.OriginalToBeAdjustedCashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 12);
        entities.OriginalToBeAdjustedCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 13);
        entities.OriginalToBeAdjustedCashReceiptDetail.
          ObligorSocialSecurityNumber = db.GetNullableString(reader, 14);
        entities.OriginalToBeAdjustedCashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 15);
        entities.OriginalToBeAdjustedCashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 16);
        entities.OriginalToBeAdjustedCashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 17);
        entities.OriginalToBeAdjustedCashReceiptDetail.ObligorPhoneNumber =
          db.GetNullableString(reader, 18);
        entities.OriginalToBeAdjustedCashReceiptDetail.CreatedBy =
          db.GetString(reader, 19);
        entities.OriginalToBeAdjustedCashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.OriginalToBeAdjustedCashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 21);
        entities.OriginalToBeAdjustedCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 22);
        entities.OriginalToBeAdjustedCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 23);
        entities.OriginalToBeAdjustedCashReceiptDetail.
          CollectionAmtFullyAppliedInd = db.GetNullableString(reader, 24);
        entities.OriginalToBeAdjustedCashReceiptDetail.InjuredSpouseInd =
          db.GetNullableString(reader, 25);
        entities.OriginalToBeAdjustedCashReceipt.ReceiptAmount =
          db.GetDecimal(reader, 26);
        entities.OriginalToBeAdjustedCashReceipt.SequentialNumber =
          db.GetInt32(reader, 27);
        entities.OriginalToBeAdjustedCashReceipt.ReceiptDate =
          db.GetDate(reader, 28);
        entities.OriginalToBeAdjustedCashReceipt.CheckType =
          db.GetNullableString(reader, 29);
        entities.OriginalToBeAdjustedCashReceipt.CheckNumber =
          db.GetNullableString(reader, 30);
        entities.OriginalToBeAdjustedCashReceipt.CheckDate =
          db.GetNullableDate(reader, 31);
        entities.OriginalToBeAdjustedCashReceipt.ReceivedDate =
          db.GetDate(reader, 32);
        entities.OriginalToBeAdjustedCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 33);
        entities.OriginalToBeAdjustedCashReceipt.ReferenceNumber =
          db.GetNullableString(reader, 34);
        entities.OriginalToBeAdjustedCashReceipt.PayorOrganization =
          db.GetNullableString(reader, 35);
        entities.OriginalToBeAdjustedCashReceipt.PayorFirstName =
          db.GetNullableString(reader, 36);
        entities.OriginalToBeAdjustedCashReceipt.PayorMiddleName =
          db.GetNullableString(reader, 37);
        entities.OriginalToBeAdjustedCashReceipt.PayorLastName =
          db.GetNullableString(reader, 38);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedToName =
          db.GetNullableString(reader, 39);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedStreet1 =
          db.GetNullableString(reader, 40);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedStreet2 =
          db.GetNullableString(reader, 41);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedCity =
          db.GetNullableString(reader, 42);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedState =
          db.GetNullableString(reader, 43);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedZip5 =
          db.GetNullableString(reader, 44);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedZip4 =
          db.GetNullableString(reader, 45);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedZip3 =
          db.GetNullableString(reader, 46);
        entities.OriginalToBeAdjustedCashReceipt.BalancedTimestamp =
          db.GetNullableDateTime(reader, 47);
        entities.OriginalToBeAdjustedCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 48);
        entities.OriginalToBeAdjustedCashReceipt.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 49);
        entities.OriginalToBeAdjustedCashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 50);
        entities.OriginalToBeAdjustedCashReceipt.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 51);
        entities.OriginalToBeAdjustedCashReceipt.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 52);
        entities.OriginalToBeAdjustedCashReceipt.CreatedBy =
          db.GetString(reader, 53);
        entities.OriginalToBeAdjustedCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 54);
        entities.OriginalToBeAdjustedCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 55);
        entities.OriginalToBeAdjustedCashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 56);
        entities.OriginalToBeAdjustedCashReceipt.CashDue =
          db.GetNullableDecimal(reader, 57);
        entities.OriginalToBeAdjustedCashReceipt.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 58);
        entities.OriginalToBeAdjustedCashReceipt.LastUpdatedBy =
          db.GetNullableString(reader, 59);
        entities.OriginalToBeAdjustedCashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 60);
        entities.OriginalToBeAdjustedCashReceipt.Note =
          db.GetNullableString(reader, 61);
        entities.OriginalToBeAdjustedCashReceiptDetail.Populated = true;
        entities.OriginalToBeAdjustedCashReceipt.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.OriginalToBeAdjustedCashReceiptDetail.
            CollectionAmtFullyAppliedInd);
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.OriginalToBeAdjustedCashReceipt.CashBalanceReason);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceipt2()
  {
    entities.OriginalToBeAdjustedCashReceiptDetail.Populated = false;
    entities.OriginalToBeAdjustedCashReceipt.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceipt2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorSsn",
          import.Collection.Detail.ObligorSocialSecurityNumber ?? "");
        db.SetString(command, "createdBy", global.UserId);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "cdsIdentifier",
          local.HardcodedViews.HardcodedAdjusted.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OriginalToBeAdjustedCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalToBeAdjustedCashReceipt.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalToBeAdjustedCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.OriginalToBeAdjustedCashReceipt.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.OriginalToBeAdjustedCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalToBeAdjustedCashReceipt.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.OriginalToBeAdjustedCashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.OriginalToBeAdjustedCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.OriginalToBeAdjustedCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.OriginalToBeAdjustedCashReceiptDetail.CaseNumber =
          db.GetNullableString(reader, 7);
        entities.OriginalToBeAdjustedCashReceiptDetail.OffsetTaxid =
          db.GetNullableInt32(reader, 8);
        entities.OriginalToBeAdjustedCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 9);
        entities.OriginalToBeAdjustedCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 10);
        entities.OriginalToBeAdjustedCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 11);
        entities.OriginalToBeAdjustedCashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 12);
        entities.OriginalToBeAdjustedCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 13);
        entities.OriginalToBeAdjustedCashReceiptDetail.
          ObligorSocialSecurityNumber = db.GetNullableString(reader, 14);
        entities.OriginalToBeAdjustedCashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 15);
        entities.OriginalToBeAdjustedCashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 16);
        entities.OriginalToBeAdjustedCashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 17);
        entities.OriginalToBeAdjustedCashReceiptDetail.ObligorPhoneNumber =
          db.GetNullableString(reader, 18);
        entities.OriginalToBeAdjustedCashReceiptDetail.CreatedBy =
          db.GetString(reader, 19);
        entities.OriginalToBeAdjustedCashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.OriginalToBeAdjustedCashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 21);
        entities.OriginalToBeAdjustedCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 22);
        entities.OriginalToBeAdjustedCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 23);
        entities.OriginalToBeAdjustedCashReceiptDetail.
          CollectionAmtFullyAppliedInd = db.GetNullableString(reader, 24);
        entities.OriginalToBeAdjustedCashReceiptDetail.InjuredSpouseInd =
          db.GetNullableString(reader, 25);
        entities.OriginalToBeAdjustedCashReceipt.ReceiptAmount =
          db.GetDecimal(reader, 26);
        entities.OriginalToBeAdjustedCashReceipt.SequentialNumber =
          db.GetInt32(reader, 27);
        entities.OriginalToBeAdjustedCashReceipt.ReceiptDate =
          db.GetDate(reader, 28);
        entities.OriginalToBeAdjustedCashReceipt.CheckType =
          db.GetNullableString(reader, 29);
        entities.OriginalToBeAdjustedCashReceipt.CheckNumber =
          db.GetNullableString(reader, 30);
        entities.OriginalToBeAdjustedCashReceipt.CheckDate =
          db.GetNullableDate(reader, 31);
        entities.OriginalToBeAdjustedCashReceipt.ReceivedDate =
          db.GetDate(reader, 32);
        entities.OriginalToBeAdjustedCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 33);
        entities.OriginalToBeAdjustedCashReceipt.ReferenceNumber =
          db.GetNullableString(reader, 34);
        entities.OriginalToBeAdjustedCashReceipt.PayorOrganization =
          db.GetNullableString(reader, 35);
        entities.OriginalToBeAdjustedCashReceipt.PayorFirstName =
          db.GetNullableString(reader, 36);
        entities.OriginalToBeAdjustedCashReceipt.PayorMiddleName =
          db.GetNullableString(reader, 37);
        entities.OriginalToBeAdjustedCashReceipt.PayorLastName =
          db.GetNullableString(reader, 38);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedToName =
          db.GetNullableString(reader, 39);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedStreet1 =
          db.GetNullableString(reader, 40);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedStreet2 =
          db.GetNullableString(reader, 41);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedCity =
          db.GetNullableString(reader, 42);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedState =
          db.GetNullableString(reader, 43);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedZip5 =
          db.GetNullableString(reader, 44);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedZip4 =
          db.GetNullableString(reader, 45);
        entities.OriginalToBeAdjustedCashReceipt.ForwardedZip3 =
          db.GetNullableString(reader, 46);
        entities.OriginalToBeAdjustedCashReceipt.BalancedTimestamp =
          db.GetNullableDateTime(reader, 47);
        entities.OriginalToBeAdjustedCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 48);
        entities.OriginalToBeAdjustedCashReceipt.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 49);
        entities.OriginalToBeAdjustedCashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 50);
        entities.OriginalToBeAdjustedCashReceipt.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 51);
        entities.OriginalToBeAdjustedCashReceipt.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 52);
        entities.OriginalToBeAdjustedCashReceipt.CreatedBy =
          db.GetString(reader, 53);
        entities.OriginalToBeAdjustedCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 54);
        entities.OriginalToBeAdjustedCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 55);
        entities.OriginalToBeAdjustedCashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 56);
        entities.OriginalToBeAdjustedCashReceipt.CashDue =
          db.GetNullableDecimal(reader, 57);
        entities.OriginalToBeAdjustedCashReceipt.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 58);
        entities.OriginalToBeAdjustedCashReceipt.LastUpdatedBy =
          db.GetNullableString(reader, 59);
        entities.OriginalToBeAdjustedCashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 60);
        entities.OriginalToBeAdjustedCashReceipt.Note =
          db.GetNullableString(reader, 61);
        entities.OriginalToBeAdjustedCashReceiptDetail.Populated = true;
        entities.OriginalToBeAdjustedCashReceipt.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.OriginalToBeAdjustedCashReceiptDetail.
            CollectionAmtFullyAppliedInd);
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.OriginalToBeAdjustedCashReceipt.CashBalanceReason);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceiptDetailBalanceAdj()
  {
    System.Diagnostics.Debug.Assert(
      entities.OriginalToBeAdjustedCashReceiptDetail.Populated);
    entities.AdjustedByCashReceiptDetail.Populated = false;
    entities.AdjustedByCashReceiptDetailBalanceAdj.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptDetailBalanceAdj",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.AdjustedByCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.AdjustedByCashReceiptDetailBalanceAdj.CrvSIdentifier =
          db.GetInt32(reader, 0);
        entities.AdjustedByCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.AdjustedByCashReceiptDetailBalanceAdj.CstSIdentifier =
          db.GetInt32(reader, 1);
        entities.AdjustedByCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.AdjustedByCashReceiptDetailBalanceAdj.CrtSIdentifier =
          db.GetInt32(reader, 2);
        entities.AdjustedByCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.AdjustedByCashReceiptDetailBalanceAdj.CrdSIdentifier =
          db.GetInt32(reader, 3);
        entities.AdjustedByCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 4);
        entities.AdjustedByCashReceiptDetailBalanceAdj.CrdIdentifier =
          db.GetInt32(reader, 5);
        entities.AdjustedByCashReceiptDetailBalanceAdj.CrvIdentifier =
          db.GetInt32(reader, 6);
        entities.AdjustedByCashReceiptDetailBalanceAdj.CstIdentifier =
          db.GetInt32(reader, 7);
        entities.AdjustedByCashReceiptDetailBalanceAdj.CrtIdentifier =
          db.GetInt32(reader, 8);
        entities.AdjustedByCashReceiptDetailBalanceAdj.CrnIdentifier =
          db.GetInt32(reader, 9);
        entities.AdjustedByCashReceiptDetailBalanceAdj.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.AdjustedByCashReceiptDetailBalanceAdj.Description =
          db.GetNullableString(reader, 11);
        entities.AdjustedByCashReceiptDetail.Populated = true;
        entities.AdjustedByCashReceiptDetailBalanceAdj.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailRlnRsn()
  {
    entities.CashReceiptDetailRlnRsn.Populated = false;

    return Read("ReadCashReceiptDetailRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdtlRlnRsnId", local.HardcodedTaxAdj.SequentialIdentifier);
          
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailRlnRsn.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailRlnRsn.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailRlnRsn.Name = db.GetString(reader, 2);
        entities.CashReceiptDetailRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptDetailRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptDetailRlnRsn.Populated = true;
      });
  }

  private bool ReadCashReceiptRlnRsn()
  {
    entities.CashReceiptRlnRsn.Populated = false;

    return Read("ReadCashReceiptRlnRsn",
      null,
      (db, reader) =>
      {
        entities.CashReceiptRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptRlnRsn.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      null,
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptTypeCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(
      entities.OriginalKeyCashReceiptDetail.Populated);
    entities.OriginalKeyCashReceiptType.Populated = false;
    entities.OriginalKeyCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptTypeCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          entities.OriginalKeyCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.OriginalKeyCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "crtypeId",
          entities.OriginalKeyCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.OriginalKeyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalKeyCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalKeyCashReceiptType.Populated = true;
        entities.OriginalKeyCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(
      entities.OriginalToBeAdjustedCashReceiptDetail.Populated);
    entities.Protected1.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvId",
          entities.OriginalToBeAdjustedCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstId",
          entities.OriginalToBeAdjustedCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtType",
          entities.OriginalToBeAdjustedCashReceiptDetail.CrtIdentifier);
        db.SetString(
          command, "distMtd", local.HardcodedProtected.DistributionMethod);
      },
      (db, reader) =>
      {
        entities.Protected1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Protected1.AppliedToCode = db.GetString(reader, 1);
        entities.Protected1.CollectionDt = db.GetDate(reader, 2);
        entities.Protected1.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Protected1.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Protected1.ConcurrentInd = db.GetString(reader, 5);
        entities.Protected1.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Protected1.CrtType = db.GetInt32(reader, 7);
        entities.Protected1.CstId = db.GetInt32(reader, 8);
        entities.Protected1.CrvId = db.GetInt32(reader, 9);
        entities.Protected1.CrdId = db.GetInt32(reader, 10);
        entities.Protected1.ObgId = db.GetInt32(reader, 11);
        entities.Protected1.CspNumber = db.GetString(reader, 12);
        entities.Protected1.CpaType = db.GetString(reader, 13);
        entities.Protected1.OtrId = db.GetInt32(reader, 14);
        entities.Protected1.OtrType = db.GetString(reader, 15);
        entities.Protected1.OtyId = db.GetInt32(reader, 16);
        entities.Protected1.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.Protected1.CollectionAdjProcessDate = db.GetDate(reader, 18);
        entities.Protected1.CreatedBy = db.GetString(reader, 19);
        entities.Protected1.CreatedTmst = db.GetDateTime(reader, 20);
        entities.Protected1.LastUpdatedBy = db.GetNullableString(reader, 21);
        entities.Protected1.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 22);
        entities.Protected1.Amount = db.GetDecimal(reader, 23);
        entities.Protected1.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 24);
        entities.Protected1.DistributionMethod = db.GetString(reader, 25);
        entities.Protected1.ProgramAppliedTo = db.GetString(reader, 26);
        entities.Protected1.AppliedToOrderTypeCode = db.GetString(reader, 27);
        entities.Protected1.CourtNoticeReqInd =
          db.GetNullableString(reader, 28);
        entities.Protected1.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 29);
        entities.Protected1.AeNotifiedDate = db.GetNullableDate(reader, 30);
        entities.Protected1.ManualDistributionReasonText =
          db.GetNullableString(reader, 31);
        entities.Protected1.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 32);
        entities.Protected1.CourtOrderAppliedTo =
          db.GetNullableString(reader, 33);
        entities.Protected1.ArNumber = db.GetNullableString(reader, 34);
        entities.Protected1.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Protected1.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Protected1.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Protected1.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Protected1.CpaType);
        CheckValid<Collection>("OtrType", entities.Protected1.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Protected1.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.Protected1.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Protected1.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Protected1.AppliedToOrderTypeCode);
      });
  }

  private bool ReadCollectionAdjustmentReason()
  {
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      null,
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Name = db.GetString(reader, 2);
        entities.CollectionAdjustmentReason.Description =
          db.GetNullableString(reader, 3);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(
      entities.OriginalKeyCashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.OriginalKeyCashReceiptDetail.CltIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.PrintName = db.GetNullableString(reader, 1);
        entities.CollectionType.Code = db.GetString(reader, 2);
        entities.CollectionType.Name = db.GetString(reader, 3);
        entities.CollectionType.CashNonCashInd = db.GetString(reader, 4);
        entities.CollectionType.DisbursementInd = db.GetString(reader, 5);
        entities.CollectionType.EffectiveDate = db.GetDate(reader, 6);
        entities.CollectionType.DiscontinueDate = db.GetNullableDate(reader, 7);
        entities.CollectionType.CreatedBy = db.GetString(reader, 8);
        entities.CollectionType.CreatedTmst = db.GetDateTime(reader, 9);
        entities.CollectionType.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.CollectionType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.CollectionType.Description = db.GetNullableString(reader, 12);
        entities.CollectionType.Populated = true;
        CheckValid<CollectionType>("CashNonCashInd",
          entities.CollectionType.CashNonCashInd);
        CheckValid<CollectionType>("DisbursementInd",
          entities.CollectionType.DisbursementInd);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.OriginalCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "numb", import.Collection.Detail.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OriginalCsePerson.Number = db.GetString(reader, 0);
        entities.OriginalCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.OriginalCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb",
          entities.OriginalToBeAdjustedCashReceiptDetail.
            ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OriginalCsePerson.Number = db.GetString(reader, 0);
        entities.OriginalCsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.AdjustingReceiptRefund.CreatedTimestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.RecoveryFiller = db.GetString(reader, 9);
        entities.PaymentRequest.RecaptureFiller = db.GetString(reader, 10);
        entities.PaymentRequest.AchFormatCode =
          db.GetNullableString(reader, 11);
        entities.PaymentRequest.InterfundVoucherFiller =
          db.GetString(reader, 12);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 13);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 14);
        entities.PaymentRequest.Type1 = db.GetString(reader, 15);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 16);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 17);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 18);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefund()
  {
    System.Diagnostics.Debug.Assert(
      entities.OriginalToBeAdjustedCashReceiptDetail.Populated);
    entities.AdjustingReceiptRefund.Populated = false;

    return ReadEach("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.AdjustingReceiptRefund.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.AdjustingReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.AdjustingReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.AdjustingReceiptRefund.PayeeName =
          db.GetNullableString(reader, 3);
        entities.AdjustingReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.AdjustingReceiptRefund.OffsetTaxYear =
          db.GetNullableInt32(reader, 5);
        entities.AdjustingReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.AdjustingReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.AdjustingReceiptRefund.CspNumber =
          db.GetNullableString(reader, 8);
        entities.AdjustingReceiptRefund.CstAIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.AdjustingReceiptRefund.CrvIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.AdjustingReceiptRefund.CrdIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.AdjustingReceiptRefund.CdaIdentifier =
          db.GetNullableDateTime(reader, 12);
        entities.AdjustingReceiptRefund.TaxIdSuffix =
          db.GetNullableString(reader, 13);
        entities.AdjustingReceiptRefund.ReasonText =
          db.GetNullableString(reader, 14);
        entities.AdjustingReceiptRefund.CltIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.AdjustingReceiptRefund.LastUpdatedBy =
          db.GetNullableString(reader, 16);
        entities.AdjustingReceiptRefund.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 17);
        entities.AdjustingReceiptRefund.CrtIdentifier =
          db.GetNullableInt32(reader, 18);
        entities.AdjustingReceiptRefund.CstIdentifier =
          db.GetNullableInt32(reader, 19);
        entities.AdjustingReceiptRefund.Populated = true;

        return true;
      });
  }

  private void UpdateCashReceiptBalanceAdjustment()
  {
    System.Diagnostics.Debug.Assert(
      entities.CashReceiptBalanceAdjustment.Populated);

    var adjustmentAmount =
      entities.CashReceiptBalanceAdjustment.AdjustmentAmount +
      local.AdjustingCashReceiptDetail.CollectionAmount;

    entities.CashReceiptBalanceAdjustment.Populated = false;
    Update("UpdateCashReceiptBalanceAdjustment",
      (db, command) =>
      {
        db.SetDecimal(command, "adjustmentAmount", adjustmentAmount);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptBalanceAdjustment.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptBalanceAdjustment.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptBalanceAdjustment.CrvIdentifier);
        db.SetInt32(
          command, "crtIIdentifier",
          entities.CashReceiptBalanceAdjustment.CrtIIdentifier);
        db.SetInt32(
          command, "cstIIdentifier",
          entities.CashReceiptBalanceAdjustment.CstIIdentifier);
        db.SetInt32(
          command, "crvIIdentifier",
          entities.CashReceiptBalanceAdjustment.CrvIIdentifier);
        db.SetInt32(
          command, "crrIdentifier",
          entities.CashReceiptBalanceAdjustment.CrrIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CashReceiptBalanceAdjustment.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.CashReceiptBalanceAdjustment.AdjustmentAmount = adjustmentAmount;
    entities.CashReceiptBalanceAdjustment.Populated = true;
  }

  private void UpdateCashReceiptDetail1()
  {
    System.Diagnostics.Debug.Assert(
      entities.OriginalToBeAdjustedCashReceiptDetail.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var refundedAmount = 0M;
    var collectionAmtFullyAppliedInd = "Y";

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.OriginalToBeAdjustedCashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail1",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "refundedAmt", refundedAmount);
        db.SetNullableString(
          command, "collamtApplInd", collectionAmtFullyAppliedInd);
        db.SetInt32(
          command, "crvIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId",
          entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier);
      });

    entities.OriginalToBeAdjustedCashReceiptDetail.LastUpdatedBy =
      lastUpdatedBy;
    entities.OriginalToBeAdjustedCashReceiptDetail.LastUpdatedTmst =
      lastUpdatedTmst;
    entities.OriginalToBeAdjustedCashReceiptDetail.RefundedAmount =
      refundedAmount;
    entities.OriginalToBeAdjustedCashReceiptDetail.
      CollectionAmtFullyAppliedInd = collectionAmtFullyAppliedInd;
    entities.OriginalToBeAdjustedCashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(
      entities.OriginalToBeAdjustedCashReceiptDetail.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var refundedAmount = local.Calculating.RefundedAmount.GetValueOrDefault();
    var collectionAmtFullyAppliedInd =
      local.Update.CollectionAmtFullyAppliedInd ?? "";

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.OriginalToBeAdjustedCashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail2",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "refundedAmt", refundedAmount);
        db.SetNullableString(
          command, "collamtApplInd", collectionAmtFullyAppliedInd);
        db.SetInt32(
          command, "crvIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.OriginalToBeAdjustedCashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId",
          entities.OriginalToBeAdjustedCashReceiptDetail.SequentialIdentifier);
      });

    entities.OriginalToBeAdjustedCashReceiptDetail.LastUpdatedBy =
      lastUpdatedBy;
    entities.OriginalToBeAdjustedCashReceiptDetail.LastUpdatedTmst =
      lastUpdatedTmst;
    entities.OriginalToBeAdjustedCashReceiptDetail.RefundedAmount =
      refundedAmount;
    entities.OriginalToBeAdjustedCashReceiptDetail.
      CollectionAmtFullyAppliedInd = collectionAmtFullyAppliedInd;
    entities.OriginalToBeAdjustedCashReceiptDetail.Populated = true;
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
    /// <summary>A CollectionGroup group.</summary>
    [Serializable]
    public class CollectionGroup
    {
      /// <summary>
      /// A value of DetalLocalCode.
      /// </summary>
      [JsonPropertyName("detalLocalCode")]
      public TextWorkArea DetalLocalCode
      {
        get => detalLocalCode ??= new();
        set => detalLocalCode = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CashReceiptDetail Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailAdjustmentAmt.
      /// </summary>
      [JsonPropertyName("detailAdjustmentAmt")]
      public Common DetailAdjustmentAmt
      {
        get => detailAdjustmentAmt ??= new();
        set => detailAdjustmentAmt = value;
      }

      private TextWorkArea detalLocalCode;
      private CashReceiptDetail detail;
      private Common detailAdjustmentAmt;
    }

    /// <summary>A AdditionalInformationGroup group.</summary>
    [Serializable]
    public class AdditionalInformationGroup
    {
      /// <summary>
      /// A value of Adjustment.
      /// </summary>
      [JsonPropertyName("adjustment")]
      public CashReceiptDetail Adjustment
      {
        get => adjustment ??= new();
        set => adjustment = value;
      }

      private CashReceiptDetail adjustment;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public DateWorkArea Save
    {
      get => save ??= new();
      set => save = value;
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
    /// A value of AdjRefAmt.
    /// </summary>
    [JsonPropertyName("adjRefAmt")]
    public Common AdjRefAmt
    {
      get => adjRefAmt ??= new();
      set => adjRefAmt = value;
    }

    /// <summary>
    /// A value of AdjRefAmtPrev.
    /// </summary>
    [JsonPropertyName("adjRefAmtPrev")]
    public Common AdjRefAmtPrev
    {
      get => adjRefAmtPrev ??= new();
      set => adjRefAmtPrev = value;
    }

    /// <summary>
    /// A value of AdjRefAmtCurr.
    /// </summary>
    [JsonPropertyName("adjRefAmtCurr")]
    public Common AdjRefAmtCurr
    {
      get => adjRefAmtCurr ??= new();
      set => adjRefAmtCurr = value;
    }

    /// <summary>
    /// A value of AdjAdjAmt.
    /// </summary>
    [JsonPropertyName("adjAdjAmt")]
    public Common AdjAdjAmt
    {
      get => adjAdjAmt ??= new();
      set => adjAdjAmt = value;
    }

    /// <summary>
    /// A value of AdjAdjAmtPrev.
    /// </summary>
    [JsonPropertyName("adjAdjAmtPrev")]
    public Common AdjAdjAmtPrev
    {
      get => adjAdjAmtPrev ??= new();
      set => adjAdjAmtPrev = value;
    }

    /// <summary>
    /// A value of AdjAdjAmtCurr.
    /// </summary>
    [JsonPropertyName("adjAdjAmtCurr")]
    public Common AdjAdjAmtCurr
    {
      get => adjAdjAmtCurr ??= new();
      set => adjAdjAmtCurr = value;
    }

    /// <summary>
    /// A value of AdjRefCurrCnt.
    /// </summary>
    [JsonPropertyName("adjRefCurrCnt")]
    public Common AdjRefCurrCnt
    {
      get => adjRefCurrCnt ??= new();
      set => adjRefCurrCnt = value;
    }

    /// <summary>
    /// A value of AdjRefPrevCnt.
    /// </summary>
    [JsonPropertyName("adjRefPrevCnt")]
    public Common AdjRefPrevCnt
    {
      get => adjRefPrevCnt ??= new();
      set => adjRefPrevCnt = value;
    }

    /// <summary>
    /// A value of AdjAdjCurrCnt.
    /// </summary>
    [JsonPropertyName("adjAdjCurrCnt")]
    public Common AdjAdjCurrCnt
    {
      get => adjAdjCurrCnt ??= new();
      set => adjAdjCurrCnt = value;
    }

    /// <summary>
    /// A value of AdjAdjPrevCnt.
    /// </summary>
    [JsonPropertyName("adjAdjPrevCnt")]
    public Common AdjAdjPrevCnt
    {
      get => adjAdjPrevCnt ??= new();
      set => adjAdjPrevCnt = value;
    }

    /// <summary>
    /// A value of AdjAdjCnt.
    /// </summary>
    [JsonPropertyName("adjAdjCnt")]
    public Common AdjAdjCnt
    {
      get => adjAdjCnt ??= new();
      set => adjAdjCnt = value;
    }

    /// <summary>
    /// A value of AdjRefCnt.
    /// </summary>
    [JsonPropertyName("adjRefCnt")]
    public Common AdjRefCnt
    {
      get => adjRefCnt ??= new();
      set => adjRefCnt = value;
    }

    /// <summary>
    /// A value of Swefb612NetAmount.
    /// </summary>
    [JsonPropertyName("swefb612NetAmount")]
    public Common Swefb612NetAmount
    {
      get => swefb612NetAmount ??= new();
      set => swefb612NetAmount = value;
    }

    /// <summary>
    /// A value of Swefb612AdjustmentAmt.
    /// </summary>
    [JsonPropertyName("swefb612AdjustmentAmt")]
    public Common Swefb612AdjustmentAmt
    {
      get => swefb612AdjustmentAmt ??= new();
      set => swefb612AdjustmentAmt = value;
    }

    /// <summary>
    /// A value of Swefb612AdjustCount.
    /// </summary>
    [JsonPropertyName("swefb612AdjustCount")]
    public Common Swefb612AdjustCount
    {
      get => swefb612AdjustCount ??= new();
      set => swefb612AdjustCount = value;
    }

    /// <summary>
    /// Gets a value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public CollectionGroup Collection
    {
      get => collection ?? (collection = new());
      set => collection = value;
    }

    /// <summary>
    /// Gets a value of AdditionalInformation.
    /// </summary>
    [JsonPropertyName("additionalInformation")]
    public AdditionalInformationGroup AdditionalInformation
    {
      get => additionalInformation ?? (additionalInformation = new());
      set => additionalInformation = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    private DateWorkArea save;
    private ProgramProcessingInfo programProcessingInfo;
    private Common adjRefAmt;
    private Common adjRefAmtPrev;
    private Common adjRefAmtCurr;
    private Common adjAdjAmt;
    private Common adjAdjAmtPrev;
    private Common adjAdjAmtCurr;
    private Common adjRefCurrCnt;
    private Common adjRefPrevCnt;
    private Common adjAdjCurrCnt;
    private Common adjAdjPrevCnt;
    private Common adjAdjCnt;
    private Common adjRefCnt;
    private Common swefb612NetAmount;
    private Common swefb612AdjustmentAmt;
    private Common swefb612AdjustCount;
    private CollectionGroup collection;
    private AdditionalInformationGroup additionalInformation;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AmtOfCollectionsCalc.
    /// </summary>
    [JsonPropertyName("amtOfCollectionsCalc")]
    public Common AmtOfCollectionsCalc
    {
      get => amtOfCollectionsCalc ??= new();
      set => amtOfCollectionsCalc = value;
    }

    /// <summary>
    /// A value of AdjRefAmt.
    /// </summary>
    [JsonPropertyName("adjRefAmt")]
    public Common AdjRefAmt
    {
      get => adjRefAmt ??= new();
      set => adjRefAmt = value;
    }

    /// <summary>
    /// A value of AdjRefAmtPrev.
    /// </summary>
    [JsonPropertyName("adjRefAmtPrev")]
    public Common AdjRefAmtPrev
    {
      get => adjRefAmtPrev ??= new();
      set => adjRefAmtPrev = value;
    }

    /// <summary>
    /// A value of AdjRefAmtCurr.
    /// </summary>
    [JsonPropertyName("adjRefAmtCurr")]
    public Common AdjRefAmtCurr
    {
      get => adjRefAmtCurr ??= new();
      set => adjRefAmtCurr = value;
    }

    /// <summary>
    /// A value of AdjAdjAmt.
    /// </summary>
    [JsonPropertyName("adjAdjAmt")]
    public Common AdjAdjAmt
    {
      get => adjAdjAmt ??= new();
      set => adjAdjAmt = value;
    }

    /// <summary>
    /// A value of AdjAdjAmtPrev.
    /// </summary>
    [JsonPropertyName("adjAdjAmtPrev")]
    public Common AdjAdjAmtPrev
    {
      get => adjAdjAmtPrev ??= new();
      set => adjAdjAmtPrev = value;
    }

    /// <summary>
    /// A value of AdjAdjAmtCurr.
    /// </summary>
    [JsonPropertyName("adjAdjAmtCurr")]
    public Common AdjAdjAmtCurr
    {
      get => adjAdjAmtCurr ??= new();
      set => adjAdjAmtCurr = value;
    }

    /// <summary>
    /// A value of AdjRefPrevCnt.
    /// </summary>
    [JsonPropertyName("adjRefPrevCnt")]
    public Common AdjRefPrevCnt
    {
      get => adjRefPrevCnt ??= new();
      set => adjRefPrevCnt = value;
    }

    /// <summary>
    /// A value of AdjRefCurrCnt.
    /// </summary>
    [JsonPropertyName("adjRefCurrCnt")]
    public Common AdjRefCurrCnt
    {
      get => adjRefCurrCnt ??= new();
      set => adjRefCurrCnt = value;
    }

    /// <summary>
    /// A value of AdjAdjPrevCnt.
    /// </summary>
    [JsonPropertyName("adjAdjPrevCnt")]
    public Common AdjAdjPrevCnt
    {
      get => adjAdjPrevCnt ??= new();
      set => adjAdjPrevCnt = value;
    }

    /// <summary>
    /// A value of AdjAdjCurrCnt.
    /// </summary>
    [JsonPropertyName("adjAdjCurrCnt")]
    public Common AdjAdjCurrCnt
    {
      get => adjAdjCurrCnt ??= new();
      set => adjAdjCurrCnt = value;
    }

    /// <summary>
    /// A value of AdjAdjCnt.
    /// </summary>
    [JsonPropertyName("adjAdjCnt")]
    public Common AdjAdjCnt
    {
      get => adjAdjCnt ??= new();
      set => adjAdjCnt = value;
    }

    /// <summary>
    /// A value of AdjRefCnt.
    /// </summary>
    [JsonPropertyName("adjRefCnt")]
    public Common AdjRefCnt
    {
      get => adjRefCnt ??= new();
      set => adjRefCnt = value;
    }

    /// <summary>
    /// A value of NoMatchOnYear.
    /// </summary>
    [JsonPropertyName("noMatchOnYear")]
    public Common NoMatchOnYear
    {
      get => noMatchOnYear ??= new();
      set => noMatchOnYear = value;
    }

    /// <summary>
    /// A value of Swefb612AdjustCount.
    /// </summary>
    [JsonPropertyName("swefb612AdjustCount")]
    public Common Swefb612AdjustCount
    {
      get => swefb612AdjustCount ??= new();
      set => swefb612AdjustCount = value;
    }

    /// <summary>
    /// A value of Swefb612AdjustmentAmt.
    /// </summary>
    [JsonPropertyName("swefb612AdjustmentAmt")]
    public Common Swefb612AdjustmentAmt
    {
      get => swefb612AdjustmentAmt ??= new();
      set => swefb612AdjustmentAmt = value;
    }

    /// <summary>
    /// A value of Swefb612NetAmount.
    /// </summary>
    [JsonPropertyName("swefb612NetAmount")]
    public Common Swefb612NetAmount
    {
      get => swefb612NetAmount ??= new();
      set => swefb612NetAmount = value;
    }

    /// <summary>
    /// A value of ImportAdjustment.
    /// </summary>
    [JsonPropertyName("importAdjustment")]
    public Common ImportAdjustment
    {
      get => importAdjustment ??= new();
      set => importAdjustment = value;
    }

    /// <summary>
    /// A value of ImportCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("importCashReceiptDetail")]
    public CashReceiptDetail ImportCashReceiptDetail
    {
      get => importCashReceiptDetail ??= new();
      set => importCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ImportNet.
    /// </summary>
    [JsonPropertyName("importNet")]
    public Common ImportNet
    {
      get => importNet ??= new();
      set => importNet = value;
    }

    /// <summary>
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    /// <summary>
    /// A value of ImportCashReceipt.
    /// </summary>
    [JsonPropertyName("importCashReceipt")]
    public CashReceipt ImportCashReceipt
    {
      get => importCashReceipt ??= new();
      set => importCashReceipt = value;
    }

    /// <summary>
    /// A value of ImportNextCheckId.
    /// </summary>
    [JsonPropertyName("importNextCheckId")]
    public CashReceiptDetail ImportNextCheckId
    {
      get => importNextCheckId ??= new();
      set => importNextCheckId = value;
    }

    /// <summary>
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
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

    private Common amtOfCollectionsCalc;
    private Common adjRefAmt;
    private Common adjRefAmtPrev;
    private Common adjRefAmtCurr;
    private Common adjAdjAmt;
    private Common adjAdjAmtPrev;
    private Common adjAdjAmtCurr;
    private Common adjRefPrevCnt;
    private Common adjRefCurrCnt;
    private Common adjAdjPrevCnt;
    private Common adjAdjCurrCnt;
    private Common adjAdjCnt;
    private Common adjRefCnt;
    private Common noMatchOnYear;
    private Common swefb612AdjustCount;
    private Common swefb612AdjustmentAmt;
    private Common swefb612NetAmount;
    private Common importAdjustment;
    private CashReceiptDetail importCashReceiptDetail;
    private Common importNet;
    private Common importNumberOfReads;
    private CashReceipt importCashReceipt;
    private CashReceiptDetail importNextCheckId;
    private Common importNumberOfUpdates;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ViewHardcodedViewsGroup group.</summary>
    [Serializable]
    public class ViewHardcodedViewsGroup
    {
      /// <summary>
      /// A value of HardcodedOver.
      /// </summary>
      [JsonPropertyName("hardcodedOver")]
      public CashReceipt HardcodedOver
      {
        get => hardcodedOver ??= new();
        set => hardcodedOver = value;
      }

      /// <summary>
      /// A value of HardcodedUnder.
      /// </summary>
      [JsonPropertyName("hardcodedUnder")]
      public CashReceipt HardcodedUnder
      {
        get => hardcodedUnder ??= new();
        set => hardcodedUnder = value;
      }

      /// <summary>
      /// A value of HardcodedEft.
      /// </summary>
      [JsonPropertyName("hardcodedEft")]
      public CashReceiptType HardcodedEft
      {
        get => hardcodedEft ??= new();
        set => hardcodedEft = value;
      }

      /// <summary>
      /// A value of HardcodedGeneralError.
      /// </summary>
      [JsonPropertyName("hardcodedGeneralError")]
      public CashReceiptRlnRsn HardcodedGeneralError
      {
        get => hardcodedGeneralError ??= new();
        set => hardcodedGeneralError = value;
      }

      /// <summary>
      /// A value of HardcodedRecorded.
      /// </summary>
      [JsonPropertyName("hardcodedRecorded")]
      public CashReceiptDetailStatus HardcodedRecorded
      {
        get => hardcodedRecorded ??= new();
        set => hardcodedRecorded = value;
      }

      private CashReceipt hardcodedOver;
      private CashReceipt hardcodedUnder;
      private CashReceiptType hardcodedEft;
      private CashReceiptRlnRsn hardcodedGeneralError;
      private CashReceiptDetailStatus hardcodedRecorded;
    }

    /// <summary>A HardcodedViewsGroup group.</summary>
    [Serializable]
    public class HardcodedViewsGroup
    {
      /// <summary>
      /// A value of HardcodedPending.
      /// </summary>
      [JsonPropertyName("hardcodedPending")]
      public CashReceiptDetailStatus HardcodedPending
      {
        get => hardcodedPending ??= new();
        set => hardcodedPending = value;
      }

      /// <summary>
      /// A value of HardcodedReleased.
      /// </summary>
      [JsonPropertyName("hardcodedReleased")]
      public CashReceiptDetailStatus HardcodedReleased
      {
        get => hardcodedReleased ??= new();
        set => hardcodedReleased = value;
      }

      /// <summary>
      /// A value of ZdelHardcodedEft1.
      /// </summary>
      [JsonPropertyName("zdelHardcodedEft1")]
      public CashReceiptType ZdelHardcodedEft1
      {
        get => zdelHardcodedEft1 ??= new();
        set => zdelHardcodedEft1 = value;
      }

      /// <summary>
      /// A value of ZdelHardcodedRecorded1.
      /// </summary>
      [JsonPropertyName("zdelHardcodedRecorded1")]
      public CashReceiptDetailStatus ZdelHardcodedRecorded1
      {
        get => zdelHardcodedRecorded1 ??= new();
        set => zdelHardcodedRecorded1 = value;
      }

      /// <summary>
      /// A value of HardcodedSuspended.
      /// </summary>
      [JsonPropertyName("hardcodedSuspended")]
      public CashReceiptDetailStatus HardcodedSuspended
      {
        get => hardcodedSuspended ??= new();
        set => hardcodedSuspended = value;
      }

      /// <summary>
      /// A value of HardcodedRefunded.
      /// </summary>
      [JsonPropertyName("hardcodedRefunded")]
      public CashReceiptDetailStatus HardcodedRefunded
      {
        get => hardcodedRefunded ??= new();
        set => hardcodedRefunded = value;
      }

      /// <summary>
      /// A value of HardcodedAdjusted.
      /// </summary>
      [JsonPropertyName("hardcodedAdjusted")]
      public CashReceiptDetailStatus HardcodedAdjusted
      {
        get => hardcodedAdjusted ??= new();
        set => hardcodedAdjusted = value;
      }

      private CashReceiptDetailStatus hardcodedPending;
      private CashReceiptDetailStatus hardcodedReleased;
      private CashReceiptType zdelHardcodedEft1;
      private CashReceiptDetailStatus zdelHardcodedRecorded1;
      private CashReceiptDetailStatus hardcodedSuspended;
      private CashReceiptDetailStatus hardcodedRefunded;
      private CashReceiptDetailStatus hardcodedAdjusted;
    }

    /// <summary>
    /// A value of ProtColFound.
    /// </summary>
    [JsonPropertyName("protColFound")]
    public Common ProtColFound
    {
      get => protColFound ??= new();
      set => protColFound = value;
    }

    /// <summary>
    /// A value of ZdelLocalProtectedFound.
    /// </summary>
    [JsonPropertyName("zdelLocalProtectedFound")]
    public Common ZdelLocalProtectedFound
    {
      get => zdelLocalProtectedFound ??= new();
      set => zdelLocalProtectedFound = value;
    }

    /// <summary>
    /// A value of ZdelLocalPersonNf.
    /// </summary>
    [JsonPropertyName("zdelLocalPersonNf")]
    public Common ZdelLocalPersonNf
    {
      get => zdelLocalPersonNf ??= new();
      set => zdelLocalPersonNf = value;
    }

    /// <summary>
    /// A value of ZdelLocalEventCreated.
    /// </summary>
    [JsonPropertyName("zdelLocalEventCreated")]
    public Common ZdelLocalEventCreated
    {
      get => zdelLocalEventCreated ??= new();
      set => zdelLocalEventCreated = value;
    }

    /// <summary>
    /// A value of ZdelLocalCaseNotFound.
    /// </summary>
    [JsonPropertyName("zdelLocalCaseNotFound")]
    public Common ZdelLocalCaseNotFound
    {
      get => zdelLocalCaseNotFound ??= new();
      set => zdelLocalCaseNotFound = value;
    }

    /// <summary>
    /// A value of HardcodedProtected.
    /// </summary>
    [JsonPropertyName("hardcodedProtected")]
    public Collection HardcodedProtected
    {
      get => hardcodedProtected ??= new();
      set => hardcodedProtected = value;
    }

    /// <summary>
    /// A value of AlreadyAdjustedSwitch.
    /// </summary>
    [JsonPropertyName("alreadyAdjustedSwitch")]
    public Common AlreadyAdjustedSwitch
    {
      get => alreadyAdjustedSwitch ??= new();
      set => alreadyAdjustedSwitch = value;
    }

    /// <summary>
    /// A value of PassToBalAdj.
    /// </summary>
    [JsonPropertyName("passToBalAdj")]
    public ProgramProcessingInfo PassToBalAdj
    {
      get => passToBalAdj ??= new();
      set => passToBalAdj = value;
    }

    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public CashReceiptDetail Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public ProgramProcessingInfo Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of TotalAdjustmentForCol.
    /// </summary>
    [JsonPropertyName("totalAdjustmentForCol")]
    public Common TotalAdjustmentForCol
    {
      get => totalAdjustmentForCol ??= new();
      set => totalAdjustmentForCol = value;
    }

    /// <summary>
    /// A value of Calculating.
    /// </summary>
    [JsonPropertyName("calculating")]
    public CashReceiptDetail Calculating
    {
      get => calculating ??= new();
      set => calculating = value;
    }

    /// <summary>
    /// A value of DistributionCollection.
    /// </summary>
    [JsonPropertyName("distributionCollection")]
    public Collection DistributionCollection
    {
      get => distributionCollection ??= new();
      set => distributionCollection = value;
    }

    /// <summary>
    /// A value of DistributionProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("distributionProgramProcessingInfo")]
    public ProgramProcessingInfo DistributionProgramProcessingInfo
    {
      get => distributionProgramProcessingInfo ??= new();
      set => distributionProgramProcessingInfo = value;
    }

    /// <summary>
    /// A value of ProcessCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("processCashReceiptDetail")]
    public CashReceiptDetail ProcessCashReceiptDetail
    {
      get => processCashReceiptDetail ??= new();
      set => processCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ProcessCashReceipt.
    /// </summary>
    [JsonPropertyName("processCashReceipt")]
    public CashReceipt ProcessCashReceipt
    {
      get => processCashReceipt ??= new();
      set => processCashReceipt = value;
    }

    /// <summary>
    /// A value of AdjustedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("adjustedCashReceiptDetail")]
    public CashReceiptDetail AdjustedCashReceiptDetail
    {
      get => adjustedCashReceiptDetail ??= new();
      set => adjustedCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of TotalDetailAdjustments.
    /// </summary>
    [JsonPropertyName("totalDetailAdjustments")]
    public Common TotalDetailAdjustments
    {
      get => totalDetailAdjustments ??= new();
      set => totalDetailAdjustments = value;
    }

    /// <summary>
    /// A value of CaseNumber.
    /// </summary>
    [JsonPropertyName("caseNumber")]
    public CashReceiptDetail CaseNumber
    {
      get => caseNumber ??= new();
      set => caseNumber = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public CashReceiptDetail Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public ReceiptRefund Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
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
    /// A value of AdjustedCollection.
    /// </summary>
    [JsonPropertyName("adjustedCollection")]
    public Collection AdjustedCollection
    {
      get => adjustedCollection ??= new();
      set => adjustedCollection = value;
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
    /// A value of Difference.
    /// </summary>
    [JsonPropertyName("difference")]
    public Common Difference
    {
      get => difference ??= new();
      set => difference = value;
    }

    /// <summary>
    /// A value of HardcodedTaxAdj.
    /// </summary>
    [JsonPropertyName("hardcodedTaxAdj")]
    public CashReceiptDetailRlnRsn HardcodedTaxAdj
    {
      get => hardcodedTaxAdj ??= new();
      set => hardcodedTaxAdj = value;
    }

    /// <summary>
    /// A value of TotAmtOfCollForAdj.
    /// </summary>
    [JsonPropertyName("totAmtOfCollForAdj")]
    public Common TotAmtOfCollForAdj
    {
      get => totAmtOfCollForAdj ??= new();
      set => totAmtOfCollForAdj = value;
    }

    /// <summary>
    /// Gets a value of ViewHardcodedViews.
    /// </summary>
    [JsonPropertyName("viewHardcodedViews")]
    public ViewHardcodedViewsGroup ViewHardcodedViews
    {
      get => viewHardcodedViews ?? (viewHardcodedViews = new());
      set => viewHardcodedViews = value;
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
    /// A value of CashReceiptDetailRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailRlnRsn")]
    public CashReceiptDetailRlnRsn CashReceiptDetailRlnRsn
    {
      get => cashReceiptDetailRlnRsn ??= new();
      set => cashReceiptDetailRlnRsn = value;
    }

    /// <summary>
    /// A value of Original.
    /// </summary>
    [JsonPropertyName("original")]
    public CashReceipt Original
    {
      get => original ??= new();
      set => original = value;
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
    /// A value of TotalNonCashFees.
    /// </summary>
    [JsonPropertyName("totalNonCashFees")]
    public Common TotalNonCashFees
    {
      get => totalNonCashFees ??= new();
      set => totalNonCashFees = value;
    }

    /// <summary>
    /// A value of TotalCashFees.
    /// </summary>
    [JsonPropertyName("totalCashFees")]
    public Common TotalCashFees
    {
      get => totalCashFees ??= new();
      set => totalCashFees = value;
    }

    /// <summary>
    /// A value of AdjustingCashReceipt.
    /// </summary>
    [JsonPropertyName("adjustingCashReceipt")]
    public CashReceipt AdjustingCashReceipt
    {
      get => adjustingCashReceipt ??= new();
      set => adjustingCashReceipt = value;
    }

    /// <summary>
    /// A value of AdjustingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("adjustingCashReceiptDetail")]
    public CashReceiptDetail AdjustingCashReceiptDetail
    {
      get => adjustingCashReceiptDetail ??= new();
      set => adjustingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of IncreaseOrDecrease.
    /// </summary>
    [JsonPropertyName("increaseOrDecrease")]
    public Common IncreaseOrDecrease
    {
      get => increaseOrDecrease ??= new();
      set => increaseOrDecrease = value;
    }

    /// <summary>
    /// Gets a value of HardcodedViews.
    /// </summary>
    [JsonPropertyName("hardcodedViews")]
    public HardcodedViewsGroup HardcodedViews
    {
      get => hardcodedViews ?? (hardcodedViews = new());
      set => hardcodedViews = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of DistributionObligor.
    /// </summary>
    [JsonPropertyName("distributionObligor")]
    public CsePersonAccount DistributionObligor
    {
      get => distributionObligor ??= new();
      set => distributionObligor = value;
    }

    private Common protColFound;
    private Common zdelLocalProtectedFound;
    private Common zdelLocalPersonNf;
    private Common zdelLocalEventCreated;
    private Common zdelLocalCaseNotFound;
    private Collection hardcodedProtected;
    private Common alreadyAdjustedSwitch;
    private ProgramProcessingInfo passToBalAdj;
    private CashReceiptDetail update;
    private ProgramProcessingInfo pass;
    private Common totalAdjustmentForCol;
    private CashReceiptDetail calculating;
    private Collection distributionCollection;
    private ProgramProcessingInfo distributionProgramProcessingInfo;
    private CashReceiptDetail processCashReceiptDetail;
    private CashReceipt processCashReceipt;
    private CashReceiptDetail adjustedCashReceiptDetail;
    private Common totalDetailAdjustments;
    private CashReceiptDetail caseNumber;
    private CashReceiptDetail case1;
    private ReceiptRefund adjustment;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private Collection adjustedCollection;
    private DateWorkArea current;
    private Common difference;
    private CashReceiptDetailRlnRsn hardcodedTaxAdj;
    private Common totAmtOfCollForAdj;
    private ViewHardcodedViewsGroup viewHardcodedViews;
    private DateWorkArea max;
    private CashReceiptDetailRlnRsn cashReceiptDetailRlnRsn;
    private CashReceipt original;
    private CollectionType collectionType;
    private Common totalNonCashFees;
    private Common totalCashFees;
    private CashReceipt adjustingCashReceipt;
    private CashReceiptDetail adjustingCashReceiptDetail;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private Common increaseOrDecrease;
    private HardcodedViewsGroup hardcodedViews;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CsePersonAccount distributionObligor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of AdjustedByCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("adjustedByCashReceiptDetail")]
    public CashReceiptDetail AdjustedByCashReceiptDetail
    {
      get => adjustedByCashReceiptDetail ??= new();
      set => adjustedByCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of AdjustedByCashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("adjustedByCashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj AdjustedByCashReceiptDetailBalanceAdj
    {
      get => adjustedByCashReceiptDetailBalanceAdj ??= new();
      set => adjustedByCashReceiptDetailBalanceAdj = value;
    }

    /// <summary>
    /// A value of AdjustingReceiptRefund.
    /// </summary>
    [JsonPropertyName("adjustingReceiptRefund")]
    public ReceiptRefund AdjustingReceiptRefund
    {
      get => adjustingReceiptRefund ??= new();
      set => adjustingReceiptRefund = value;
    }

    /// <summary>
    /// A value of OriginalCsePerson.
    /// </summary>
    [JsonPropertyName("originalCsePerson")]
    public CsePerson OriginalCsePerson
    {
      get => originalCsePerson ??= new();
      set => originalCsePerson = value;
    }

    /// <summary>
    /// A value of OriginalCashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("originalCashReceiptDetailAddress")]
    public CashReceiptDetailAddress OriginalCashReceiptDetailAddress
    {
      get => originalCashReceiptDetailAddress ??= new();
      set => originalCashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of MaxId.
    /// </summary>
    [JsonPropertyName("maxId")]
    public CashReceiptDetail MaxId
    {
      get => maxId ??= new();
      set => maxId = value;
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
    /// A value of Protected1.
    /// </summary>
    [JsonPropertyName("protected1")]
    public Collection Protected1
    {
      get => protected1 ??= new();
      set => protected1 = value;
    }

    /// <summary>
    /// A value of AdjustingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("adjustingCashReceiptDetail")]
    public CashReceiptDetail AdjustingCashReceiptDetail
    {
      get => adjustingCashReceiptDetail ??= new();
      set => adjustingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public CashReceipt Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
    }

    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
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
    /// A value of CashReceiptDetailRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailRlnRsn")]
    public CashReceiptDetailRlnRsn CashReceiptDetailRlnRsn
    {
      get => cashReceiptDetailRlnRsn ??= new();
      set => cashReceiptDetailRlnRsn = value;
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
    /// A value of OriginalToBeAdjustedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("originalToBeAdjustedCashReceiptDetail")]
    public CashReceiptDetail OriginalToBeAdjustedCashReceiptDetail
    {
      get => originalToBeAdjustedCashReceiptDetail ??= new();
      set => originalToBeAdjustedCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of OriginalKeyCashReceiptType.
    /// </summary>
    [JsonPropertyName("originalKeyCashReceiptType")]
    public CashReceiptType OriginalKeyCashReceiptType
    {
      get => originalKeyCashReceiptType ??= new();
      set => originalKeyCashReceiptType = value;
    }

    /// <summary>
    /// A value of OriginalKeyCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("originalKeyCashReceiptEvent")]
    public CashReceiptEvent OriginalKeyCashReceiptEvent
    {
      get => originalKeyCashReceiptEvent ??= new();
      set => originalKeyCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of OriginalToBeAdjustedCashReceipt.
    /// </summary>
    [JsonPropertyName("originalToBeAdjustedCashReceipt")]
    public CashReceipt OriginalToBeAdjustedCashReceipt
    {
      get => originalToBeAdjustedCashReceipt ??= new();
      set => originalToBeAdjustedCashReceipt = value;
    }

    /// <summary>
    /// A value of OriginalKeyCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("originalKeyCashReceiptDetail")]
    public CashReceiptDetail OriginalKeyCashReceiptDetail
    {
      get => originalKeyCashReceiptDetail ??= new();
      set => originalKeyCashReceiptDetail = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of ZdelOriginalCheck.
    /// </summary>
    [JsonPropertyName("zdelOriginalCheck")]
    public CashReceipt ZdelOriginalCheck
    {
      get => zdelOriginalCheck ??= new();
      set => zdelOriginalCheck = value;
    }

    /// <summary>
    /// A value of ZdelOriginalKey.
    /// </summary>
    [JsonPropertyName("zdelOriginalKey")]
    public CashReceiptSourceType ZdelOriginalKey
    {
      get => zdelOriginalKey ??= new();
      set => zdelOriginalKey = value;
    }

    /// <summary>
    /// A value of ZdelOriginalCheckKey.
    /// </summary>
    [JsonPropertyName("zdelOriginalCheckKey")]
    public CashReceiptType ZdelOriginalCheckKey
    {
      get => zdelOriginalCheckKey ??= new();
      set => zdelOriginalCheckKey = value;
    }

    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetail adjustedByCashReceiptDetail;
    private CashReceiptDetailBalanceAdj adjustedByCashReceiptDetailBalanceAdj;
    private ReceiptRefund adjustingReceiptRefund;
    private CsePerson originalCsePerson;
    private CashReceiptDetailAddress originalCashReceiptDetailAddress;
    private CashReceiptDetail maxId;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private Collection protected1;
    private CashReceiptDetail adjustingCashReceiptDetail;
    private CashReceipt adjustment;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptDetailRlnRsn cashReceiptDetailRlnRsn;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetail originalToBeAdjustedCashReceiptDetail;
    private CashReceiptType originalKeyCashReceiptType;
    private CashReceiptEvent originalKeyCashReceiptEvent;
    private CashReceipt originalToBeAdjustedCashReceipt;
    private CashReceiptDetail originalKeyCashReceiptDetail;
    private CollectionType collectionType;
    private PaymentRequest paymentRequest;
    private CashReceipt zdelOriginalCheck;
    private CashReceiptSourceType zdelOriginalKey;
    private CashReceiptType zdelOriginalCheckKey;
  }
#endregion
}
