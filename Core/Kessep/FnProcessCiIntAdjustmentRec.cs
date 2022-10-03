// Program: FN_PROCESS_CI_INT_ADJUSTMENT_REC, ID: 372565699, model: 746.
// Short name: SWE00519
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
/// A program: FN_PROCESS_CI_INT_ADJUSTMENT_REC.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block will process the Adjustment records (Type = 5) from
/// the court interface.
/// It will create a Cash Receipt Adjustment Balance and associate it to the 
/// prior Cash Receipt being adjusted.
/// </para>
/// </summary>
[Serializable]
public partial class FnProcessCiIntAdjustmentRec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_CI_INT_ADJUSTMENT_REC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessCiIntAdjustmentRec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessCiIntAdjustmentRec.
  /// </summary>
  public FnProcessCiIntAdjustmentRec(IContext context, Import import,
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
    // *****************************************************************
    // RICH GALICHON - 2/99
    // Unit test and fixes.
    // *****************************************************************
    // ***************************************************
    // 01/08/02        SWSRPDP         WR010504-A - Retro Processing
    // Create an EVENT if "P"rotected Collection is Adjusted
    // ***************************************************
    // *****
    // WR010504-B - Retro Processing - EVENT if "P"rotected Collection is 
    // Adjusted
    // *****
    local.HardcodedProtected.DistributionMethod = "P";
    export.Swefb610AdjustmentCount.Count =
      import.Swefb610AdjustmentCount.Count + 1;
    UseFnHardcodedCashReceipting();

    // *****************************************************************
    // Validate the Cash Receipt Detail Relationship Reason and
    // retrieve the sequential identifier number.
    // *****************************************************************
    local.Current.Timestamp = Now();
    local.AsOf.Date = import.CashReceiptEvent.SourceCreationDate;
    UseFnReadCrDtlRlnRsnViaCode();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *****************************************************************
    // Read the Original Interface Trans ID to find the existing
    // Cash Receipt and Cash Receipt Detail being adjusted.
    // *****************************************************************
    if (ReadCashReceiptDetailCashReceiptTypeCashReceiptEvent())
    {
      if (AsChar(entities.OriginalCashReceiptDetail.AdjustmentInd) == 'Y')
      {
        ExitState = "FN0000_ORIG_DTL_ADJ_IND_IS_YES";

        return;
      }

      MoveCashReceiptDetail4(entities.OriginalCashReceiptDetail, local.Original);
        

      // *****************************************************************
      // Read the original Cash Receipt so the amounts can be
      // adjusted.
      // *****************************************************************
      if (!ReadCashReceipt())
      {
        ExitState = "FN0125_ORIGINAL_CASH_RCPT_ADJ_NF";

        return;
      }

      // *****************************************************************
      // Re-read the Original Key Cash Receipt Detail so the entity
      // view may be view matched to persistent views in
      // subsequent CABs.  Although this may seem redundant, it
      // will save on the total number of reads in the long run.
      // *****************************************************************
      if (!ReadCashReceiptDetail2())
      {
        ExitState = "FN0124_ORIGINAL_CASH_RCPT_ADJ_NF";

        return;
      }
    }
    else
    {
      // -----------------------------------------------------------------
      // Either the original tran id provided by the court on the
      // adjustment record was not found OR the court order # on
      // the adjustment record does not match the court order #
      // on the original detail record.
      // -----------------------------------------------------------------
      if (ReadCashReceiptDetail1())
      {
        // -----------------------------------------------------------
        // Since the interface tran id was found, the court order #
        // on the original detail record must not match the court
        // order # provided by the court on the adjustment record.
        // -----------------------------------------------------------
        export.Original.CourtOrderNumber =
          entities.OriginalCashReceiptDetail.CourtOrderNumber;
        ExitState = "FN0000_ADJ_COURT_ORDER_MISMATCH";
      }
      else
      {
        // --------------------------------------------------------------
        // The original tran id provided by the court on the adjustment
        // record was not found.
        // --------------------------------------------------------------
        ExitState = "FN0124_ORIGINAL_CASH_RCPT_ADJ_NF";
      }

      return;
    }

    // *****************************************************************
    // Determine if the original cash receipt detail is associated to
    // a collection type.  If it is, associate the adjustment CRD to
    // the same collection type.
    // *****************************************************************
    UseFnReadCollectionTypeViaCrd();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else if (IsExitState("FN0000_COLLECTION_TYPE_NF"))
    {
      // *****************************************************************
      // Continue processing.  Since the original is not associated to
      // a Collection Type, the adjustment CRD will also not be
      // associated to one.
      // *****************************************************************
      ExitState = "ACO_NN0000_ALL_OK";
    }
    else
    {
      return;
    }

    // *********************************************************************
    // Determine if the Original cash receipt is already in adjusted status.
    // *********************************************************************
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
    {
      if (entities.OriginalCashReceiptDetailStatus.SystemGeneratedIdentifier ==
        local
        .ViewHardcodedViews.HardcodedCrdsAdjusted.SystemGeneratedIdentifier)
      {
        ExitState = "FN0000_CR_DTL_ALREADY_ADJ";

        return;
      }
    }
    else
    {
      ExitState = "FN0064_CSH_RCPT_DTL_ST_HST_NF_RB";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *****
    // WR010504-B - Retro Processing - EVENT if "P"rotected Collection is 
    // Adjusted
    // *****
    // Read collection and Create ALERT "IF" Collection is Protected
    // * * * * * * * * * * * * * * * * * * * * * * * * * *
    local.ProtCollFound.Flag = "N";

    if (ReadCollection())
    {
      local.ProtCollFound.Flag = "Y";
    }

    // *****************************************************************
    // If the Distributed amount on the original Cash Receipt Detail
    // is greater than Zero, reverse all the collections.
    // *****************************************************************
    if (Lt(0, entities.OriginalCashReceiptDetail.DistributedAmount))
    {
      if (!ReadCollectionAdjustmentReason())
      {
        ExitState = "FN0000_COLL_ADJUST_REASON_NF";

        return;
      }

      if (ReadCashReceiptDetailRlnRsn())
      {
        local.AdjustmentCollection.CollectionAdjustmentReasonTxt =
          entities.AdjustmentReason.Description;
      }
      else
      {
        ExitState = "FN0059_CASH_RCPT_DTL_RLN_RSN_NF";

        return;
      }

      local.AdjustmentCollection.CollectionAdjustmentDt =
        import.CashReceiptEvent.SourceCreationDate;
      local.Obligor.PgmChgEffectiveDate =
        entities.OriginalCashReceiptDetail.CollectionDate;
      UseFnCabReverseOneCshRcptDtl();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // *****************************************************************
      // Set the collection fully applied indicator to YES.   JLK  04/29/99
      // *****************************************************************
      try
      {
        UpdateCashReceiptDetail1();
        ++export.ImportNumberOfUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_CASH_RCPT_DTL_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_CASH_RCPT_DTL_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      // *****************************************************************
      // If the Original cash receipt has existing refunds, create a
      // detail history record and set the original cash receipt
      // Refunded Amount to zero.    JLK  04/14/99
      // *****************************************************************
      if (Lt(0, entities.OriginalCashReceiptDetail.RefundedAmount))
      {
        // **************************************************************
        // Create cash receipt detail history record with existing
        // detail data prior to updating the original cash receipt.
        // JLK  04/17/99
        // **************************************************************
        UseFnAbRecordCrDetailHistory();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // **************************************************************
        // Update the original cash receipt detail to reset the
        // Refunded Amount back to zero.    JLK  04/14/99
        // **************************************************************
        try
        {
          UpdateCashReceiptDetail2();
          ++export.ImportNumberOfUpdates.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CASH_RCPT_DTL_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CASH_RCPT_DTL_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        // *****************************************************************
        // Set the collection fully applied indicator to YES.   JLK  04/29/99
        // *****************************************************************
        try
        {
          UpdateCashReceiptDetail1();
          ++export.ImportNumberOfUpdates.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CASH_RCPT_DTL_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CASH_RCPT_DTL_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // *****************************************************************
      // Change the status of the Original cash receipt to ADJ.
      // JLK  04/14/99
      // *****************************************************************
      UseFnChangeCashRcptDtlStatHis();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ******************************************
    // SET THE ADJUSTMENT CASH RECEIPT IDENTIFIER
    // ******************************************
    local.AdjustmentCashReceipt.SequentialNumber =
      export.ImportCheck.SequentialNumber;

    // *************************************************
    // SET THE ADJUSTMENT CASH RECEIPT DETAIL IDENTIFIER
    // *************************************************
    ++export.ImportNextCheckId.SequentialIdentifier;
    local.AdjustmentCashReceiptDetail.SequentialIdentifier =
      export.ImportNextCheckId.SequentialIdentifier;

    // *****************************************
    // CREATE THE ADJUSTMENT CASH RECEIPT DETAIL
    // *****************************************
    local.AdjustmentCashReceiptDetail.InterfaceTransId =
      import.AdjustmentRecord.AdjustmentInputNew.InterfaceTransId ?? "";
    local.AdjustmentCashReceiptDetail.CourtOrderNumber =
      import.AdjustmentRecord.AdjustmentInputOriginal.CourtOrderNumber ?? "";
    local.AdjustmentCashReceiptDetail.AdjustmentInd = "Y";
    local.AdjustmentCashReceiptDetail.ReceivedAmount =
      entities.OriginalCashReceiptDetail.ReceivedAmount;
    local.AdjustmentCashReceiptDetail.CollectionAmount =
      entities.OriginalCashReceiptDetail.CollectionAmount;
    export.Swefb610AdjustmentCount.TotalCurrency =
      entities.OriginalCashReceiptDetail.ReceivedAmount;
    UseRecordCollection();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *****************************************************************
    // Create the Cash Receipt Detail Balance Adjustment.
    // *****************************************************************
    UseFnCreateCashRcptDtlBalAdj();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *****************************************************************
    // APPLY ADJUSTMENT TO THE CASH RECEIPTS
    // Determine if a Cash Receipt Adjustment has been created
    // that relates the original and new Cash Receipts.  If it has
    // been created, accumulate the existing Adjustment;
    // otherwise, create a new Cash Receipt Balance Adjustment.
    // *****************************************************************
    UseFnReadCashRcptBalanceAdjust();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // *****************************************************************
      // Update the exiting Cash Receipt Balance Adjustment.
      // *****************************************************************
      local.CashReceiptBalanceAdjustment.AdjustmentAmount += entities.
        OriginalCashReceiptDetail.ReceivedAmount;
      UseFnUpdateCashRcptBalanceAdj();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // *****************************************************************
        // Reset the adjustment amount to apply only the incremental
        // increase to the Cash Receipts.  Any previous balance from
        // an existing adjustment has already been applied.
        // *****************************************************************
        local.CashReceiptBalanceAdjustment.AdjustmentAmount =
          entities.OriginalCashReceiptDetail.ReceivedAmount;
      }
      else
      {
        return;
      }
    }
    else if (IsExitState("FN0031_CASH_RCPT_BAL_ADJ_NF"))
    {
      ExitState = "ACO_NN0000_ALL_OK";

      // *****************************************************************
      // Create the Cash Receipt Balance Adjustment and associate
      // the New CR to the Original CR.
      // *****************************************************************
      local.CashReceiptBalanceAdjustment.AdjustmentAmount =
        entities.OriginalCashReceiptDetail.ReceivedAmount;
      local.CashReceiptBalanceAdjustment.Description =
        Spaces(CashReceiptBalanceAdjustment.Description_MaxLength);
      UseFnCreateCashRcptBalanceAdj();

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
      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *****
    // WR010504-B - Retro Processing - EVENT if "P"rotected Collection is 
    // Adjusted
    // *****
    // Read collection and Create ALERT "IF" Collection is Protected
    if (AsChar(local.ProtCollFound.Flag) == 'Y')
    {
      // * A PROTECTED Collection has been Found
      ExitState = "ACO_NN0000_ALL_OK";
      UseFnRaiseProtCrdAdjustEvent();

      // IF AN EVENT was NOT created
      // for this Protected Cash Receipt Detail
      // * Reset the Exit State so the ERROR will appear on the ERROR Report
      // * These are set to UNIQUE (for this job) Exit States
      // *so the ERROR can be Reported WITHOUT Abending the Job
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
        // ERROR creating EVENT
        ExitState = "SP0000_EVENT_NF";
      }
    }
  }

  private static void MoveCashReceiptBalanceAdjustment1(
    CashReceiptBalanceAdjustment source, CashReceiptBalanceAdjustment target)
  {
    target.AdjustmentAmount = source.AdjustmentAmount;
    target.Description = source.Description;
  }

  private static void MoveCashReceiptBalanceAdjustment2(
    CashReceiptBalanceAdjustment source, CashReceiptBalanceAdjustment target)
  {
    target.AdjustmentAmount = source.AdjustmentAmount;
    target.Description = source.Description;
    target.CreatedTimestamp = source.CreatedTimestamp;
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
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
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

  private static void MoveCashReceiptDetailRlnRsn(
    CashReceiptDetailRlnRsn source, CashReceiptDetailRlnRsn target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
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

  private void UseFnAbRecordCrDetailHistory()
  {
    var useImport = new FnAbRecordCrDetailHistory.Import();
    var useExport = new FnAbRecordCrDetailHistory.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      entities.OriginalCashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.OriginalKeyCashReceiptDetail.SequentialIdentifier;
    useImport.CollectionType.SequentialIdentifier =
      local.CollectionType.SequentialIdentifier;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnAbRecordCrDetailHistory.Execute, useImport, useExport);

    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnCabReverseOneCshRcptDtl()
  {
    var useImport = new FnCabReverseOneCshRcptDtl.Import();
    var useExport = new FnCabReverseOneCshRcptDtl.Export();

    useImport.CashReceipt.SequentialNumber =
      entities.OriginalCashReceipt.SequentialNumber;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptType.SystemGeneratedIdentifier;
    MoveCashReceiptDetail1(entities.OriginalCashReceiptDetail,
      useImport.CashReceiptDetail);
    useImport.CollectionAdjustmentReason.SystemGeneratedIdentifier =
      entities.CollectionAdjustmentReason.SystemGeneratedIdentifier;
    MoveProgramProcessingInfo(import.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    MoveCollection1(local.AdjustmentCollection, useImport.Collection);
    useImport.Obligor.PgmChgEffectiveDate = local.Obligor.PgmChgEffectiveDate;
    useImport.Max.Date = local.Max.Date;
    useImport.RecAdjStatus.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedCrdsAdjusted.SystemGeneratedIdentifier;

    Call(FnCabReverseOneCshRcptDtl.Execute, useImport, useExport);
  }

  private void UseFnChangeCashRcptDtlStatHis()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      entities.OriginalKeyCashReceiptType.SystemGeneratedIdentifier;
    useImport.Persistent.Assign(entities.OriginalKeyCashReceiptDetail);
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedCrdsAdjusted.SystemGeneratedIdentifier;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);

    entities.OriginalKeyCashReceiptDetail.Assign(useImport.Persistent);
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnCreateCashRcptBalanceAdj()
  {
    var useImport = new FnCreateCashRcptBalanceAdj.Import();
    var useExport = new FnCreateCashRcptBalanceAdj.Export();

    useImport.Decrease.SequentialNumber =
      entities.OriginalCashReceipt.SequentialNumber;
    useImport.Increase.SequentialNumber =
      local.AdjustmentCashReceipt.SequentialNumber;
    MoveCashReceiptBalanceAdjustment1(local.CashReceiptBalanceAdjustment,
      useImport.CashReceiptBalanceAdjustment);
    useImport.CashReceiptRlnRsn.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedCourtInterface.
        SystemGeneratedIdentifier;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnCreateCashRcptBalanceAdj.Execute, useImport, useExport);

    local.CashReceiptBalanceAdjustment.Assign(
      useExport.CashReceiptBalanceAdjustment);
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnCreateCashRcptDtlBalAdj()
  {
    var useImport = new FnCreateCashRcptDtlBalAdj.Import();
    var useExport = new FnCreateCashRcptDtlBalAdj.Export();

    useImport.AdjustedCashReceipt.SequentialNumber =
      entities.OriginalCashReceipt.SequentialNumber;
    MoveProgramProcessingInfo(import.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.AdjustingCashReceipt.SequentialNumber =
      local.AdjustmentCashReceipt.SequentialNumber;
    useImport.AdjustingCashReceiptDetail.SequentialIdentifier =
      local.AdjustmentCashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptDetailRlnRsn.SequentialIdentifier =
      local.CashReceiptDetailRlnRsn.SequentialIdentifier;
    MoveCashReceiptDetail4(local.Original, useImport.AdjustedCashReceiptDetail);
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnCreateCashRcptDtlBalAdj.Execute, useImport, useExport);

    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.ViewHardcodedViews.HardcodedCrdsRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.ViewHardcodedViews.HardcodedCrdsAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
    local.ViewHardcodedViews.HardcodedCourtInterface.SystemGeneratedIdentifier =
      useExport.CrRlnRsnSystemId.CrrrCourtInterface.SystemGeneratedIdentifier;
  }

  private void UseFnRaiseProtCrdAdjustEvent()
  {
    var useImport = new FnRaiseProtCrdAdjustEvent.Import();
    var useExport = new FnRaiseProtCrdAdjustEvent.Export();

    MoveCollection2(entities.Protected1, useImport.Collection);
    useImport.CashReceipt.SequentialNumber =
      entities.OriginalCashReceipt.SequentialNumber;
    MoveCashReceiptDetail3(entities.OriginalCashReceiptDetail,
      useImport.CashReceiptDetail);

    Call(FnRaiseProtCrdAdjustEvent.Execute, useImport, useExport);
  }

  private void UseFnReadCashRcptBalanceAdjust()
  {
    var useImport = new FnReadCashRcptBalanceAdjust.Import();
    var useExport = new FnReadCashRcptBalanceAdjust.Export();

    useImport.CashReceiptRlnRsn.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedCourtInterface.
        SystemGeneratedIdentifier;
    useImport.Increase.SequentialNumber =
      local.AdjustmentCashReceipt.SequentialNumber;
    useImport.Decrease.SequentialNumber =
      entities.OriginalCashReceipt.SequentialNumber;

    Call(FnReadCashRcptBalanceAdjust.Execute, useImport, useExport);

    local.CashReceiptBalanceAdjustment.Assign(
      useExport.CashReceiptBalanceAdjustment);
  }

  private void UseFnReadCollectionTypeViaCrd()
  {
    var useImport = new FnReadCollectionTypeViaCrd.Import();
    var useExport = new FnReadCollectionTypeViaCrd.Export();

    useImport.CashReceipt.SequentialNumber =
      entities.OriginalCashReceipt.SequentialNumber;
    useImport.Persistent.Assign(entities.OriginalKeyCashReceiptDetail);
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.OriginalCashReceiptDetail.SequentialIdentifier;

    Call(FnReadCollectionTypeViaCrd.Execute, useImport, useExport);

    entities.OriginalKeyCashReceiptDetail.Assign(useImport.Persistent);
    MoveCollectionType(useExport.CollectionType, local.CollectionType);
  }

  private void UseFnReadCrDtlRlnRsnViaCode()
  {
    var useImport = new FnReadCrDtlRlnRsnViaCode.Import();
    var useExport = new FnReadCrDtlRlnRsnViaCode.Export();

    useImport.CashReceiptDetailRlnRsn.Code =
      import.AdjustmentRecord.Adjustment.Code;
    useImport.AsOf.Date = local.AsOf.Date;

    Call(FnReadCrDtlRlnRsnViaCode.Execute, useImport, useExport);

    MoveCashReceiptDetailRlnRsn(useExport.CashReceiptDetailRlnRsn,
      local.CashReceiptDetailRlnRsn);
  }

  private void UseFnUpdateCashRcptBalanceAdj()
  {
    var useImport = new FnUpdateCashRcptBalanceAdj.Import();
    var useExport = new FnUpdateCashRcptBalanceAdj.Export();

    useImport.Decreasing.SequentialNumber =
      entities.OriginalCashReceipt.SequentialNumber;
    useImport.Increasing.SequentialNumber =
      local.AdjustmentCashReceipt.SequentialNumber;
    MoveCashReceiptBalanceAdjustment2(local.CashReceiptBalanceAdjustment,
      useImport.CashReceiptBalanceAdjustment);
    useImport.CashReceiptRlnRsn.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedCourtInterface.
        SystemGeneratedIdentifier;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnUpdateCashRcptBalanceAdj.Execute, useImport, useExport);

    local.CashReceiptBalanceAdjustment.Assign(
      useExport.CashReceiptBalanceAdjustment);
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseRecordCollection()
  {
    var useImport = new RecordCollection.Import();
    var useExport = new RecordCollection.Export();

    useImport.CashReceipt.SequentialNumber =
      local.AdjustmentCashReceipt.SequentialNumber;
    MoveCashReceiptDetail2(local.AdjustmentCashReceiptDetail,
      useImport.CashReceiptDetail);
    MoveCollectionType(local.CollectionType, useImport.CollectionType);
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.ViewHardcodedViews.HardcodedCrdsRecorded.SystemGeneratedIdentifier;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(RecordCollection.Execute, useImport, useExport);

    MoveCashReceiptDetail2(useExport.CashReceiptDetail,
      local.AdjustmentCashReceiptDetail);
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private bool ReadCashReceipt()
  {
    System.Diagnostics.Debug.
      Assert(entities.OriginalCashReceiptDetail.Populated);
    entities.OriginalCashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier",
          entities.OriginalCashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.OriginalCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.OriginalCashReceiptDetail.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.OriginalCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.OriginalCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.OriginalCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.OriginalCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.OriginalCashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail1()
  {
    entities.OriginalCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "interfaceTranId",
          import.AdjustmentRecord.AdjustmentInputOriginal.InterfaceTransId ?? ""
          );
      },
      (db, reader) =>
      {
        entities.OriginalCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.OriginalCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.OriginalCashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.OriginalCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.OriginalCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.OriginalCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 7);
        entities.OriginalCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.OriginalCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.OriginalCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.OriginalCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.OriginalCashReceiptDetail.CreatedBy = db.GetString(reader, 12);
        entities.OriginalCashReceiptDetail.CreatedTmst =
          db.GetDateTime(reader, 13);
        entities.OriginalCashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.OriginalCashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.OriginalCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 16);
        entities.OriginalCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 17);
        entities.OriginalCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 18);
        entities.OriginalCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.OriginalCashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private bool ReadCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.OriginalCashReceipt.Populated);
    entities.OriginalKeyCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          entities.OriginalCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.OriginalCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.OriginalCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.OriginalCashReceipt.CrvIdentifier);
          
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
        entities.OriginalKeyCashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCashReceiptTypeCashReceiptEvent()
  {
    entities.OriginalKeyCashReceiptEvent.Populated = false;
    entities.OriginalKeyCashReceiptType.Populated = false;
    entities.OriginalCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptTypeCashReceiptEvent",
      (db, command) =>
      {
        db.SetNullableString(
          command, "interfaceTranId",
          import.AdjustmentRecord.AdjustmentInputOriginal.InterfaceTransId ?? ""
          );
        db.SetNullableString(
          command, "courtOrderNumber",
          import.AdjustmentRecord.AdjustmentInputOriginal.CourtOrderNumber ?? ""
          );
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OriginalCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalKeyCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.OriginalKeyCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.OriginalCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalKeyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.OriginalCashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.OriginalCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.OriginalCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.OriginalCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 7);
        entities.OriginalCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.OriginalCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.OriginalCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.OriginalCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.OriginalCashReceiptDetail.CreatedBy = db.GetString(reader, 12);
        entities.OriginalCashReceiptDetail.CreatedTmst =
          db.GetDateTime(reader, 13);
        entities.OriginalCashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.OriginalCashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.OriginalCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 16);
        entities.OriginalCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 17);
        entities.OriginalCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 18);
        entities.OriginalKeyCashReceiptEvent.Populated = true;
        entities.OriginalKeyCashReceiptType.Populated = true;
        entities.OriginalCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.OriginalCashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private bool ReadCashReceiptDetailRlnRsn()
  {
    entities.AdjustmentReason.Populated = false;

    return Read("ReadCashReceiptDetailRlnRsn",
      (db, command) =>
      {
        db.SetString(command, "code", import.AdjustmentRecord.Adjustment.Code);
      },
      (db, reader) =>
      {
        entities.AdjustmentReason.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.AdjustmentReason.Code = db.GetString(reader, 1);
        entities.AdjustmentReason.Description = db.GetNullableString(reader, 2);
        entities.AdjustmentReason.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.
      Assert(entities.OriginalCashReceiptDetail.Populated);
    entities.OriginalCashReceiptDetailStatHistory.Populated = false;
    entities.OriginalCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.OriginalCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.OriginalCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.OriginalCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.OriginalCashReceiptDetail.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OriginalCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.OriginalCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.OriginalCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.OriginalCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.OriginalCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.OriginalCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.OriginalCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OriginalCashReceiptDetailStatHistory.Populated = true;
        entities.OriginalCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.
      Assert(entities.OriginalCashReceiptDetail.Populated);
    entities.Protected1.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          entities.OriginalCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvId", entities.OriginalCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstId", entities.OriginalCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtType", entities.OriginalCashReceiptDetail.CrtIdentifier);
          
        db.SetString(
          command, "distMtd", local.HardcodedProtected.DistributionMethod);
      },
      (db, reader) =>
      {
        entities.Protected1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Protected1.CrtType = db.GetInt32(reader, 1);
        entities.Protected1.CstId = db.GetInt32(reader, 2);
        entities.Protected1.CrvId = db.GetInt32(reader, 3);
        entities.Protected1.CrdId = db.GetInt32(reader, 4);
        entities.Protected1.ObgId = db.GetInt32(reader, 5);
        entities.Protected1.CspNumber = db.GetString(reader, 6);
        entities.Protected1.CpaType = db.GetString(reader, 7);
        entities.Protected1.OtrId = db.GetInt32(reader, 8);
        entities.Protected1.OtrType = db.GetString(reader, 9);
        entities.Protected1.OtyId = db.GetInt32(reader, 10);
        entities.Protected1.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.Protected1.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.Protected1.DistributionMethod = db.GetString(reader, 13);
        entities.Protected1.CourtOrderAppliedTo =
          db.GetNullableString(reader, 14);
        entities.Protected1.ArNumber = db.GetNullableString(reader, 15);
        entities.Protected1.Populated = true;
        CheckValid<Collection>("CpaType", entities.Protected1.CpaType);
        CheckValid<Collection>("OtrType", entities.Protected1.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Protected1.DistributionMethod);
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
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private void UpdateCashReceiptDetail1()
  {
    System.Diagnostics.Debug.
      Assert(entities.OriginalCashReceiptDetail.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.Current.Timestamp;
    var collectionAmtFullyAppliedInd = "Y";

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.OriginalCashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail1",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(
          command, "collamtApplInd", collectionAmtFullyAppliedInd);
        db.SetInt32(
          command, "crvIdentifier",
          entities.OriginalCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.OriginalCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.OriginalCashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId",
          entities.OriginalCashReceiptDetail.SequentialIdentifier);
      });

    entities.OriginalCashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.OriginalCashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.OriginalCashReceiptDetail.CollectionAmtFullyAppliedInd =
      collectionAmtFullyAppliedInd;
    entities.OriginalCashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptDetail2()
  {
    System.Diagnostics.Debug.
      Assert(entities.OriginalCashReceiptDetail.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.Current.Timestamp;
    var refundedAmount = 0M;
    var collectionAmtFullyAppliedInd = "Y";

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.OriginalCashReceiptDetail.Populated = false;
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
          entities.OriginalCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.OriginalCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.OriginalCashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId",
          entities.OriginalCashReceiptDetail.SequentialIdentifier);
      });

    entities.OriginalCashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.OriginalCashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.OriginalCashReceiptDetail.RefundedAmount = refundedAmount;
    entities.OriginalCashReceiptDetail.CollectionAmtFullyAppliedInd =
      collectionAmtFullyAppliedInd;
    entities.OriginalCashReceiptDetail.Populated = true;
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
    /// <summary>A AdjustmentRecordGroup group.</summary>
    [Serializable]
    public class AdjustmentRecordGroup
    {
      /// <summary>
      /// A value of Adjustment.
      /// </summary>
      [JsonPropertyName("adjustment")]
      public CashReceiptDetailRlnRsn Adjustment
      {
        get => adjustment ??= new();
        set => adjustment = value;
      }

      /// <summary>
      /// A value of AdjustmentInputOriginal.
      /// </summary>
      [JsonPropertyName("adjustmentInputOriginal")]
      public CashReceiptDetail AdjustmentInputOriginal
      {
        get => adjustmentInputOriginal ??= new();
        set => adjustmentInputOriginal = value;
      }

      /// <summary>
      /// A value of AdjustmentInputNew.
      /// </summary>
      [JsonPropertyName("adjustmentInputNew")]
      public CashReceiptDetail AdjustmentInputNew
      {
        get => adjustmentInputNew ??= new();
        set => adjustmentInputNew = value;
      }

      private CashReceiptDetailRlnRsn adjustment;
      private CashReceiptDetail adjustmentInputOriginal;
      private CashReceiptDetail adjustmentInputNew;
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
    /// A value of Swefb610AdjustmentCount.
    /// </summary>
    [JsonPropertyName("swefb610AdjustmentCount")]
    public Common Swefb610AdjustmentCount
    {
      get => swefb610AdjustmentCount ??= new();
      set => swefb610AdjustmentCount = value;
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
    /// Gets a value of AdjustmentRecord.
    /// </summary>
    [JsonPropertyName("adjustmentRecord")]
    public AdjustmentRecordGroup AdjustmentRecord
    {
      get => adjustmentRecord ?? (adjustmentRecord = new());
      set => adjustmentRecord = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private Common swefb610AdjustmentCount;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private AdjustmentRecordGroup adjustmentRecord;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Swefb610AdjustmentCount.
    /// </summary>
    [JsonPropertyName("swefb610AdjustmentCount")]
    public Common Swefb610AdjustmentCount
    {
      get => swefb610AdjustmentCount ??= new();
      set => swefb610AdjustmentCount = value;
    }

    /// <summary>
    /// A value of ImportCheck.
    /// </summary>
    [JsonPropertyName("importCheck")]
    public CashReceipt ImportCheck
    {
      get => importCheck ??= new();
      set => importCheck = value;
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
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
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
    /// A value of Original.
    /// </summary>
    [JsonPropertyName("original")]
    public CashReceiptDetail Original
    {
      get => original ??= new();
      set => original = value;
    }

    private Common swefb610AdjustmentCount;
    private CashReceipt importCheck;
    private CashReceiptDetail importNextCheckId;
    private Common importNumberOfReads;
    private Common importNumberOfUpdates;
    private CashReceiptDetail original;
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
      /// A value of HardcodedCrdsRecorded.
      /// </summary>
      [JsonPropertyName("hardcodedCrdsRecorded")]
      public CashReceiptDetailStatus HardcodedCrdsRecorded
      {
        get => hardcodedCrdsRecorded ??= new();
        set => hardcodedCrdsRecorded = value;
      }

      /// <summary>
      /// A value of HardcodedCrdsAdjusted.
      /// </summary>
      [JsonPropertyName("hardcodedCrdsAdjusted")]
      public CashReceiptDetailStatus HardcodedCrdsAdjusted
      {
        get => hardcodedCrdsAdjusted ??= new();
        set => hardcodedCrdsAdjusted = value;
      }

      /// <summary>
      /// A value of HardcodedCourtInterface.
      /// </summary>
      [JsonPropertyName("hardcodedCourtInterface")]
      public CashReceiptRlnRsn HardcodedCourtInterface
      {
        get => hardcodedCourtInterface ??= new();
        set => hardcodedCourtInterface = value;
      }

      private CashReceiptDetailStatus hardcodedCrdsRecorded;
      private CashReceiptDetailStatus hardcodedCrdsAdjusted;
      private CashReceiptRlnRsn hardcodedCourtInterface;
    }

    /// <summary>A ErrorsGroup group.</summary>
    [Serializable]
    public class ErrorsGroup
    {
      /// <summary>
      /// A value of ErrorsDetailProgramError.
      /// </summary>
      [JsonPropertyName("errorsDetailProgramError")]
      public ProgramError ErrorsDetailProgramError
      {
        get => errorsDetailProgramError ??= new();
        set => errorsDetailProgramError = value;
      }

      /// <summary>
      /// A value of ErrorsDetailStandard.
      /// </summary>
      [JsonPropertyName("errorsDetailStandard")]
      public Standard ErrorsDetailStandard
      {
        get => errorsDetailStandard ??= new();
        set => errorsDetailStandard = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private ProgramError errorsDetailProgramError;
      private Standard errorsDetailStandard;
    }

    /// <summary>
    /// A value of ProtCollFound.
    /// </summary>
    [JsonPropertyName("protCollFound")]
    public Common ProtCollFound
    {
      get => protCollFound ??= new();
      set => protCollFound = value;
    }

    /// <summary>
    /// A value of Original.
    /// </summary>
    [JsonPropertyName("original")]
    public CashReceiptDetail Original
    {
      get => original ??= new();
      set => original = value;
    }

    /// <summary>
    /// A value of AdjustmentCashReceipt.
    /// </summary>
    [JsonPropertyName("adjustmentCashReceipt")]
    public CashReceipt AdjustmentCashReceipt
    {
      get => adjustmentCashReceipt ??= new();
      set => adjustmentCashReceipt = value;
    }

    /// <summary>
    /// A value of AdjustmentCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("adjustmentCashReceiptDetail")]
    public CashReceiptDetail AdjustmentCashReceiptDetail
    {
      get => adjustmentCashReceiptDetail ??= new();
      set => adjustmentCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of AdjustmentCollection.
    /// </summary>
    [JsonPropertyName("adjustmentCollection")]
    public Collection AdjustmentCollection
    {
      get => adjustmentCollection ??= new();
      set => adjustmentCollection = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of CashReceiptDetailRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailRlnRsn")]
    public CashReceiptDetailRlnRsn CashReceiptDetailRlnRsn
    {
      get => cashReceiptDetailRlnRsn ??= new();
      set => cashReceiptDetailRlnRsn = value;
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
    /// A value of TotalNonCashFees.
    /// </summary>
    [JsonPropertyName("totalNonCashFees")]
    public Common TotalNonCashFees
    {
      get => totalNonCashFees ??= new();
      set => totalNonCashFees = value;
    }

    /// <summary>
    /// A value of AsOf.
    /// </summary>
    [JsonPropertyName("asOf")]
    public DateWorkArea AsOf
    {
      get => asOf ??= new();
      set => asOf = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// Gets a value of Errors.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorsGroup> Errors => errors ??= new(ErrorsGroup.Capacity);

    /// <summary>
    /// Gets a value of Errors for json serialization.
    /// </summary>
    [JsonPropertyName("errors")]
    [Computed]
    public IList<ErrorsGroup> Errors_Json
    {
      get => errors;
      set => Errors.Assign(value);
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
    /// A value of ZdelLocalEventCreated.
    /// </summary>
    [JsonPropertyName("zdelLocalEventCreated")]
    public Common ZdelLocalEventCreated
    {
      get => zdelLocalEventCreated ??= new();
      set => zdelLocalEventCreated = value;
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
    /// A value of ZdelLocalCaseNotFound.
    /// </summary>
    [JsonPropertyName("zdelLocalCaseNotFound")]
    public Common ZdelLocalCaseNotFound
    {
      get => zdelLocalCaseNotFound ??= new();
      set => zdelLocalCaseNotFound = value;
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

    private Common protCollFound;
    private CashReceiptDetail original;
    private CashReceipt adjustmentCashReceipt;
    private CashReceiptDetail adjustmentCashReceiptDetail;
    private Collection adjustmentCollection;
    private CollectionType collectionType;
    private CsePersonAccount obligor;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceiptDetailRlnRsn cashReceiptDetailRlnRsn;
    private Common totalCashFees;
    private Common totalNonCashFees;
    private DateWorkArea asOf;
    private DateWorkArea current;
    private DateWorkArea max;
    private ViewHardcodedViewsGroup viewHardcodedViews;
    private Array<ErrorsGroup> errors;
    private Collection hardcodedProtected;
    private Common zdelLocalEventCreated;
    private Common zdelLocalPersonNf;
    private Common zdelLocalCaseNotFound;
    private Common zdelLocalProtectedFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OriginalCashReceipt.
    /// </summary>
    [JsonPropertyName("originalCashReceipt")]
    public CashReceipt OriginalCashReceipt
    {
      get => originalCashReceipt ??= new();
      set => originalCashReceipt = value;
    }

    /// <summary>
    /// A value of OriginalKeyCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("originalKeyCashReceiptSourceType")]
    public CashReceiptSourceType OriginalKeyCashReceiptSourceType
    {
      get => originalKeyCashReceiptSourceType ??= new();
      set => originalKeyCashReceiptSourceType = value;
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
    /// A value of OriginalKeyCashReceiptType.
    /// </summary>
    [JsonPropertyName("originalKeyCashReceiptType")]
    public CashReceiptType OriginalKeyCashReceiptType
    {
      get => originalKeyCashReceiptType ??= new();
      set => originalKeyCashReceiptType = value;
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
    /// A value of OriginalCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("originalCashReceiptDetail")]
    public CashReceiptDetail OriginalCashReceiptDetail
    {
      get => originalCashReceiptDetail ??= new();
      set => originalCashReceiptDetail = value;
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
    /// A value of OriginalCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("originalCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory OriginalCashReceiptDetailStatHistory
    {
      get => originalCashReceiptDetailStatHistory ??= new();
      set => originalCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of OriginalCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("originalCashReceiptDetailStatus")]
    public CashReceiptDetailStatus OriginalCashReceiptDetailStatus
    {
      get => originalCashReceiptDetailStatus ??= new();
      set => originalCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of AdjustmentReason.
    /// </summary>
    [JsonPropertyName("adjustmentReason")]
    public CashReceiptDetailRlnRsn AdjustmentReason
    {
      get => adjustmentReason ??= new();
      set => adjustmentReason = value;
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
    /// A value of ZdelOriginalToBeAdjustedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("zdelOriginalToBeAdjustedCashReceiptDetail")]
    public CashReceiptDetail ZdelOriginalToBeAdjustedCashReceiptDetail
    {
      get => zdelOriginalToBeAdjustedCashReceiptDetail ??= new();
      set => zdelOriginalToBeAdjustedCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ZdelOriginalToBeAdjustedCashReceipt.
    /// </summary>
    [JsonPropertyName("zdelOriginalToBeAdjustedCashReceipt")]
    public CashReceipt ZdelOriginalToBeAdjustedCashReceipt
    {
      get => zdelOriginalToBeAdjustedCashReceipt ??= new();
      set => zdelOriginalToBeAdjustedCashReceipt = value;
    }

    private CashReceipt originalCashReceipt;
    private CashReceiptSourceType originalKeyCashReceiptSourceType;
    private CashReceiptEvent originalKeyCashReceiptEvent;
    private CashReceiptType originalKeyCashReceiptType;
    private CashReceiptDetail originalKeyCashReceiptDetail;
    private CashReceiptDetail originalCashReceiptDetail;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CashReceiptDetailStatHistory originalCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus originalCashReceiptDetailStatus;
    private CashReceiptDetailRlnRsn adjustmentReason;
    private Collection protected1;
    private CashReceiptDetail zdelOriginalToBeAdjustedCashReceiptDetail;
    private CashReceipt zdelOriginalToBeAdjustedCashReceipt;
  }
#endregion
}
