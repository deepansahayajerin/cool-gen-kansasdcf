// Program: REFUND_UNDISTRIBUTED_COLLECTION, ID: 371773359, model: 746.
// Short name: SWE01057
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: REFUND_UNDISTRIBUTED_COLLECTION.
/// </summary>
[Serializable]
public partial class RefundUndistributedCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the REFUND_UNDISTRIBUTED_COLLECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new RefundUndistributedCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of RefundUndistributedCollection.
  /// </summary>
  public RefundUndistributedCollection(IContext context, Import import,
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
    // Every initial development and change to that
    // development needs to be documented.
    // ---------------------------------------------
    // ----------------------------------------------------------------------------------------------
    // Date	   Developer	Request #	Description
    // ---------  ----------   ------------    
    // ------------------------------------------------------
    // 01/29/96  Holly Kennedy-MTW		Updated existing Code
    // 04/29/97  SHERAZ MALIK			CHANGE CURRENT_DATE
    // 02/26/99  Sunya Sharp			Add logic to support the collection fully
    // 					applied indicator.
    // 05/04/99  Sunya Sharp			More hardcoded cab to the top of the action 
    // block.
    // 					Local fields are not getting set correctly.
    // 					If the organization number cannot be found for
    // 					the source code need to make error and rollback.
    // 02/13/18  GVandy	CQ45790		Refunds for cash receipt details which 
    // originated
    // 					at the KPC will be sent back to the KPC through
    // 					the Notice of Collections job.  Set new attributes
    // 					on the receipt refund to faciliate the process.
    // ----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    MoveReceiptRefund1(import.ReceiptRefund, export.ReceiptRefund);
    export.CashReceiptDetailAddress.Assign(import.CashReceiptDetailAddress);
    export.SendTo.Assign(import.SendTo);
    export.CsePerson.Number = import.RefundToCsePerson.Number;
    export.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    local.Max.Date = UseCabSetMaximumDiscontinueDate2();
    UseFnHardcodedCashReceipting();

    if (!ReadCashReceiptDetailCashReceiptSourceTypeCashReceipt())
    {
      ExitState = "FN0052_CASH_RCPT_DTL_NF";

      return;
    }

    MoveCashReceiptDetail(entities.CashReceiptDetail, export.CashReceiptDetail);

    if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
    {
      if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .Adjusted.SystemGeneratedIdentifier || entities
        .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .Suspended.SystemGeneratedIdentifier)
      {
        if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
          .Suspended.SystemGeneratedIdentifier && Equal
          (import.FromDistributionProcess.Command, "DISTIB"))
        {
          goto Read;
        }

        ExitState = "COLLECTION_STATUS_INVALID_4_REQ";

        return;
      }
    }
    else
    {
      ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";

      return;
    }

Read:

    if (import.ReceiptRefund.Amount > export
      .CashReceiptDetail.ReceivedAmount - entities
      .CashReceiptDetail.RefundedAmount.GetValueOrDefault())
    {
      ExitState = "FN0000_RFND_AMT_EXCEED_AVAILABLE";

      return;
    }

    // *** Add logic to get the collection type for updating the cash receipt 
    // detail history entity.  Sunya Sharp 5/6/1999 ***
    ReadCollectionType();
    local.ReceiptRefund.Amount = import.ReceiptRefund.Amount;

    if (AsChar(import.NewAddress.Flag) == 'Y')
    {
      try
      {
        CreateCashReceiptDetailAddress();
        export.SendTo.Assign(entities.CashReceiptDetailAddress);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_ROLLBACK_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_ROLLBACK_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else if (ReadCashReceiptDetailAddress())
    {
      export.CashReceiptDetailAddress.Assign(entities.CashReceiptDetailAddress);

      // ok
    }
    else
    {
      ExitState = "FN0000_ROLLBACK_RB";

      return;
    }

    // 02/13/18  GVandy  CQ45790  Refunds for cash receipt details which 
    // originated at the
    // KPC will be sent back to the KPC through the Notice of Collections job.  
    // Set new
    // attributes on the receipt refund to faciliate the process.
    // -- If the original cash receipt detail was created through the KPC 
    // interface then set
    //    the kpc_notice_req_ind to "Y".
    local.ReceiptRefund.KpcNoticeReqInd = "N";
    local.ReceiptRefund.KpcNoticeProcessedDate = new DateTime(1, 1, 1);

    if (Equal(entities.CashReceiptDetail.CreatedBy, "SWEFB610"))
    {
      if (Equal(import.ReceiptRefund.ReasonCode, "PIF") || Equal
        (import.ReceiptRefund.ReasonCode, "OVERPAY") || Equal
        (import.ReceiptRefund.ReasonCode, "NON SRS"))
      {
        local.ReceiptRefund.KpcNoticeReqInd = "Y";
      }
    }

    if (!IsEmpty(import.RefundToCsePerson.Number))
    {
      if (ReadCsePerson1())
      {
        export.CsePerson.Number = entities.RefundToCsePerson.Number;
        local.RefundToPayment.Number = entities.RefundToCsePerson.Number;
      }
      else
      {
        ExitState = "CSE_PERSON_NF_RB";

        return;
      }

      try
      {
        CreateReceiptRefund1();
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
    }

    if (!IsEmpty(import.RefundToCashReceiptSourceType.Code))
    {
      if (ReadCashReceiptSourceType())
      {
        local.RefundTo.Assign(entities.RefundToCashReceiptSourceType);

        if (ReadCsePerson2())
        {
          local.RefundToPayment.Number = entities.RefundToOrganization.Number;
        }
        else
        {
          ExitState = "FN0000_NO_ADD_REFUND_NO_ORG_NUMB";

          return;
        }
      }
      else
      {
        ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

        return;
      }

      try
      {
        CreateReceiptRefund2();
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
    }

    // *** Add logic to create a cash receipt detail history.  If something 
    // happens and the cash receipt detail is not updated a rollback is
    // performed.  This will rollback the history update as well.  Sunya Sharp 5
    // /6/1999 ***
    UseFnAbRecordCrDetailHistory();

    if (IsExitState("FN0052_CASH_RCPT_DTL_NF"))
    {
      ExitState = "FN0053_CASH_RCPT_DTL_NF_RB";
    }
    else if (IsExitState("FN0049_CASH_RCPT_DTL_HIST_AE"))
    {
      ExitState = "FN0049_CASH_RCPT_DTL_HIST_AE_RB";
    }
    else if (IsExitState("FN0050_CASH_RCPT_DTL_HIST_PV"))
    {
      ExitState = "FN0050_CASH_RCPT_DTL_HIST_PV_RB";
    }
    else
    {
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    MoveReceiptRefund2(entities.ReceiptRefund, export.ReceiptRefund);

    if (local.Undistributed.CollectionAmount == entities
      .CashReceiptDetail.CollectionAmount - entities
      .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - (
        entities.CashReceiptDetail.RefundedAmount.GetValueOrDefault() + entities
      .ReceiptRefund.Amount))
    {
      local.Undistributed.CollectionAmtFullyAppliedInd = "Y";
    }
    else
    {
      local.Undistributed.CollectionAmtFullyAppliedInd = "";
    }

    try
    {
      UpdateCashReceiptDetail();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_ROLLBACK_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_ROLLBACK_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    MoveCashReceiptDetail(entities.CashReceiptDetail, export.CashReceiptDetail);

    if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
    {
      UseFnChangeCashRcptDtlStatHis();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "FN0000_ROLLBACK_RB";

        return;
      }
    }
    else
    {
      ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";

      return;
    }

    // *** Added logic to support refunds being payed to a source code.  Sunya 
    // Sharp 3/1/1999 ***
    try
    {
      CreatePaymentRequest();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_PAYMENT_REQUEST_AE_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_PAYMENT_REQUEST_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (!ReadPaymentStatus())
    {
      ExitState = "FN0000_PYMNT_STAT_HIST_NF_RB";

      return;
    }

    local.Attempts.Count = 0;

    while(local.Attempts.Count < 10)
    {
      try
      {
        CreatePaymentStatusHistory();
        MovePaymentStatus(entities.PaymentStatus, export.PaymentStatus);

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ++local.Attempts.Count;
            ExitState = "FN0000_PYMNT_STAT_HIST_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_PYMNT_STAT_HIST_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.OffsetTaxid = source.OffsetTaxid;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.JointReturnName = source.JointReturnName;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MovePaymentStatus(PaymentStatus source,
    PaymentStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveReceiptRefund1(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Taxid = source.Taxid;
    target.PayeeName = source.PayeeName;
    target.Amount = source.Amount;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.RequestDate = source.RequestDate;
    target.ReasonText = source.ReasonText;
  }

  private static void MoveReceiptRefund2(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Taxid = source.Taxid;
    target.PayeeName = source.PayeeName;
    target.Amount = source.Amount;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.RequestDate = source.RequestDate;
    target.ReasonText = source.ReasonText;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnAbRecordCrDetailHistory()
  {
    var useImport = new FnAbRecordCrDetailHistory.Import();
    var useExport = new FnAbRecordCrDetailHistory.Export();

    useImport.CollectionType.SequentialIdentifier =
      entities.CollectionType.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(FnAbRecordCrDetailHistory.Execute, useImport, useExport);
  }

  private void UseFnChangeCashRcptDtlStatHis()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.Refunded.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.Refunded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
    local.Suspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.Adjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateCashReceiptDetailAddress()
  {
    var systemGeneratedIdentifier = Now();
    var street1 = import.SendTo.Street1;
    var street2 = import.SendTo.Street2 ?? "";
    var city = import.SendTo.City;
    var state = import.SendTo.State;
    var zipCode5 = import.SendTo.ZipCode5;
    var zipCode4 = import.SendTo.ZipCode4 ?? "";
    var zipCode3 = import.SendTo.ZipCode3 ?? "";

    entities.CashReceiptDetailAddress.Populated = false;
    Update("CreateCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetDateTime(command, "crdetailAddressI", systemGeneratedIdentifier);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zipCode3", zipCode3);
      });

    entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CashReceiptDetailAddress.Street1 = street1;
    entities.CashReceiptDetailAddress.Street2 = street2;
    entities.CashReceiptDetailAddress.City = city;
    entities.CashReceiptDetailAddress.State = state;
    entities.CashReceiptDetailAddress.ZipCode5 = zipCode5;
    entities.CashReceiptDetailAddress.ZipCode4 = zipCode4;
    entities.CashReceiptDetailAddress.ZipCode3 = zipCode3;
    entities.CashReceiptDetailAddress.Populated = true;
  }

  private void CreatePaymentRequest()
  {
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var amount = import.ReceiptRefund.Amount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var csePersonNumber = local.RefundToPayment.Number;
    var classification = "REF";
    var type1 = "WAR";
    var rctRTstamp = entities.ReceiptRefund.CreatedTimestamp;

    CheckValid<PaymentRequest>("Type1", type1);
    entities.PaymentRequest.Populated = false;
    Update("CreatePaymentRequest",
      (db, command) =>
      {
        db.SetInt32(command, "paymentRequestId", systemGeneratedIdentifier);
        db.SetDate(command, "processDate", default(DateTime));
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "dpCsePerNum", "");
        db.SetNullableString(command, "csePersonNumber", csePersonNumber);
        db.SetNullableString(command, "imprestFundCode", "");
        db.SetString(command, "classification", classification);
        db.SetNullableString(command, "achFormatCode", "");
        db.SetNullableString(command, "number", "");
        db.SetString(command, "type", type1);
        db.SetNullableDateTime(command, "rctRTstamp", rctRTstamp);
      });

    entities.PaymentRequest.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentRequest.Amount = amount;
    entities.PaymentRequest.CreatedBy = createdBy;
    entities.PaymentRequest.CreatedTimestamp = createdTimestamp;
    entities.PaymentRequest.CsePersonNumber = csePersonNumber;
    entities.PaymentRequest.Classification = classification;
    entities.PaymentRequest.Type1 = type1;
    entities.PaymentRequest.RctRTstamp = rctRTstamp;
    entities.PaymentRequest.PrqRGeneratedId = null;
    entities.PaymentRequest.Populated = true;
  }

  private void CreatePaymentStatusHistory()
  {
    var pstGeneratedId = entities.PaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var effectiveDate = local.Current.Date;
    var discontinueDate = UseCabSetMaximumDiscontinueDate1();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = import.ReceiptRefund.ReasonText ?? "";

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreateReceiptRefund1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var createdTimestamp = Now();
    var reasonCode = import.ReceiptRefund.ReasonCode;
    var taxid = import.ReceiptRefund.Taxid ?? "";
    var payeeName = import.ReceiptRefund.PayeeName ?? "";
    var amount = import.ReceiptRefund.Amount;
    var offsetTaxYear = import.ReceiptRefund.OffsetTaxYear.GetValueOrDefault();
    var requestDate = local.Current.Date;
    var createdBy = global.UserId;
    var cspNumber = entities.RefundToCsePerson.Number;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var cdaIdentifier =
      entities.CashReceiptDetailAddress.SystemGeneratedIdentifier;
    var reasonText = import.ReceiptRefund.ReasonText ?? "";
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var kpcNoticeReqInd = local.ReceiptRefund.KpcNoticeReqInd ?? "";
    var kpcNoticeProcessedDate = local.ReceiptRefund.KpcNoticeProcessedDate;

    entities.ReceiptRefund.Populated = false;
    Update("CreateReceiptRefund1",
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
        db.SetNullableInt32(command, "crvIdentifier", crvIdentifier);
        db.SetNullableInt32(command, "crdIdentifier", crdIdentifier);
        db.SetNullableDateTime(command, "cdaIdentifier", cdaIdentifier);
        db.SetString(command, "offsetClosed", "");
        db.SetNullableDate(command, "dateTransmitted", default(DateTime));
        db.SetNullableString(command, "taxIdSuffix", "");
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableInt32(command, "crtIdentifier", crtIdentifier);
        db.SetNullableInt32(command, "cstIdentifier", cstIdentifier);
        db.SetNullableString(command, "kpcNoticeReqInd", kpcNoticeReqInd);
        db.SetNullableDate(command, "kpcNoticeProcDt", kpcNoticeProcessedDate);
      });

    entities.ReceiptRefund.CreatedTimestamp = createdTimestamp;
    entities.ReceiptRefund.ReasonCode = reasonCode;
    entities.ReceiptRefund.Taxid = taxid;
    entities.ReceiptRefund.PayeeName = payeeName;
    entities.ReceiptRefund.Amount = amount;
    entities.ReceiptRefund.OffsetTaxYear = offsetTaxYear;
    entities.ReceiptRefund.RequestDate = requestDate;
    entities.ReceiptRefund.CreatedBy = createdBy;
    entities.ReceiptRefund.CspNumber = cspNumber;
    entities.ReceiptRefund.CstAIdentifier = null;
    entities.ReceiptRefund.CrvIdentifier = crvIdentifier;
    entities.ReceiptRefund.CrdIdentifier = crdIdentifier;
    entities.ReceiptRefund.CdaIdentifier = cdaIdentifier;
    entities.ReceiptRefund.ReasonText = reasonText;
    entities.ReceiptRefund.CrtIdentifier = crtIdentifier;
    entities.ReceiptRefund.CstIdentifier = cstIdentifier;
    entities.ReceiptRefund.KpcNoticeReqInd = kpcNoticeReqInd;
    entities.ReceiptRefund.KpcNoticeProcessedDate = kpcNoticeProcessedDate;
    entities.ReceiptRefund.Populated = true;
  }

  private void CreateReceiptRefund2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var createdTimestamp = Now();
    var reasonCode = import.ReceiptRefund.ReasonCode;
    var taxid = import.ReceiptRefund.Taxid ?? "";
    var payeeName = import.ReceiptRefund.PayeeName ?? "";
    var amount = import.ReceiptRefund.Amount;
    var offsetTaxYear = import.ReceiptRefund.OffsetTaxYear.GetValueOrDefault();
    var requestDate = local.Current.Date;
    var createdBy = global.UserId;
    var cstAIdentifier =
      entities.RefundToCashReceiptSourceType.SystemGeneratedIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var cdaIdentifier =
      entities.CashReceiptDetailAddress.SystemGeneratedIdentifier;
    var reasonText = import.ReceiptRefund.ReasonText ?? "";
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var kpcNoticeReqInd = local.ReceiptRefund.KpcNoticeReqInd ?? "";
    var kpcNoticeProcessedDate = local.ReceiptRefund.KpcNoticeProcessedDate;

    entities.ReceiptRefund.Populated = false;
    Update("CreateReceiptRefund2",
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
        db.SetNullableInt32(command, "cstAIdentifier", cstAIdentifier);
        db.SetNullableInt32(command, "crvIdentifier", crvIdentifier);
        db.SetNullableInt32(command, "crdIdentifier", crdIdentifier);
        db.SetNullableDateTime(command, "cdaIdentifier", cdaIdentifier);
        db.SetString(command, "offsetClosed", "");
        db.SetNullableDate(command, "dateTransmitted", default(DateTime));
        db.SetNullableString(command, "taxIdSuffix", "");
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableInt32(command, "crtIdentifier", crtIdentifier);
        db.SetNullableInt32(command, "cstIdentifier", cstIdentifier);
        db.SetNullableString(command, "kpcNoticeReqInd", kpcNoticeReqInd);
        db.SetNullableDate(command, "kpcNoticeProcDt", kpcNoticeProcessedDate);
      });

    entities.ReceiptRefund.CreatedTimestamp = createdTimestamp;
    entities.ReceiptRefund.ReasonCode = reasonCode;
    entities.ReceiptRefund.Taxid = taxid;
    entities.ReceiptRefund.PayeeName = payeeName;
    entities.ReceiptRefund.Amount = amount;
    entities.ReceiptRefund.OffsetTaxYear = offsetTaxYear;
    entities.ReceiptRefund.RequestDate = requestDate;
    entities.ReceiptRefund.CreatedBy = createdBy;
    entities.ReceiptRefund.CspNumber = null;
    entities.ReceiptRefund.CstAIdentifier = cstAIdentifier;
    entities.ReceiptRefund.CrvIdentifier = crvIdentifier;
    entities.ReceiptRefund.CrdIdentifier = crdIdentifier;
    entities.ReceiptRefund.CdaIdentifier = cdaIdentifier;
    entities.ReceiptRefund.ReasonText = reasonText;
    entities.ReceiptRefund.CrtIdentifier = crtIdentifier;
    entities.ReceiptRefund.CstIdentifier = cstIdentifier;
    entities.ReceiptRefund.KpcNoticeReqInd = kpcNoticeReqInd;
    entities.ReceiptRefund.KpcNoticeProcessedDate = kpcNoticeProcessedDate;
    entities.ReceiptRefund.Populated = true;
  }

  private bool ReadCashReceiptDetailAddress()
  {
    entities.CashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "crdetailAddressI",
          import.CashReceiptDetailAddress.SystemGeneratedIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.CashReceiptDetailAddress.Street1 = db.GetString(reader, 1);
        entities.CashReceiptDetailAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.CashReceiptDetailAddress.City = db.GetString(reader, 3);
        entities.CashReceiptDetailAddress.State = db.GetString(reader, 4);
        entities.CashReceiptDetailAddress.ZipCode5 = db.GetString(reader, 5);
        entities.CashReceiptDetailAddress.ZipCode4 =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailAddress.ZipCode3 =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetailAddress.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCashReceiptSourceTypeCashReceipt()
  {
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptSourceTypeCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.OffsetTaxid = db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 9);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 10);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 11);
        entities.CashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 13);
        entities.CashReceiptDetail.JointReturnInd =
          db.GetNullableString(reader, 14);
        entities.CashReceiptDetail.JointReturnName =
          db.GetNullableString(reader, 15);
        entities.CashReceiptDetail.DefaultedCollectionDateInd =
          db.GetNullableString(reader, 16);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 17);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 18);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 19);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 21);
        entities.CashReceiptDetail.ObligorPhoneNumber =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetail.PayeeFirstName =
          db.GetNullableString(reader, 23);
        entities.CashReceiptDetail.PayeeMiddleName =
          db.GetNullableString(reader, 24);
        entities.CashReceiptDetail.PayeeLastName =
          db.GetNullableString(reader, 25);
        entities.CashReceiptDetail.Attribute1SupportedPersonFirstName =
          db.GetNullableString(reader, 26);
        entities.CashReceiptDetail.Attribute1SupportedPersonMiddleName =
          db.GetNullableString(reader, 27);
        entities.CashReceiptDetail.Attribute1SupportedPersonLastName =
          db.GetNullableString(reader, 28);
        entities.CashReceiptDetail.Attribute2SupportedPersonFirstName =
          db.GetNullableString(reader, 29);
        entities.CashReceiptDetail.Attribute2SupportedPersonLastName =
          db.GetNullableString(reader, 30);
        entities.CashReceiptDetail.Attribute2SupportedPersonMiddleName =
          db.GetNullableString(reader, 31);
        entities.CashReceiptDetail.CreatedBy = db.GetString(reader, 32);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 33);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 34);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 35);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 36);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 37);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 38);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 39);
        entities.CashReceiptDetail.Reference = db.GetNullableString(reader, 40);
        entities.CashReceiptDetail.Notes = db.GetNullableString(reader, 41);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 42);
        entities.CashReceipt.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 43);
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("JointReturnInd",
          entities.CashReceiptDetail.JointReturnInd);
        CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
          entities.CashReceiptDetail.DefaultedCollectionDateInd);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatHistory.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 7);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 10);
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.RefundToCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.
          SetString(command, "code", import.RefundToCashReceiptSourceType.Code);
          
      },
      (db, reader) =>
      {
        entities.RefundToCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.RefundToCashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.RefundToCashReceiptSourceType.State =
          db.GetNullableInt32(reader, 2);
        entities.RefundToCashReceiptSourceType.County =
          db.GetNullableInt32(reader, 3);
        entities.RefundToCashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 4);
        entities.RefundToCashReceiptSourceType.Populated = true;
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
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.RefundToCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.RefundToCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.RefundToCsePerson.Number = db.GetString(reader, 0);
        entities.RefundToCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.RefundToOrganization.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetInt32(command, "state", local.RefundTo.State.GetValueOrDefault());
        db.
          SetInt32(command, "county", local.RefundTo.County.GetValueOrDefault());
          
        db.SetInt32(
          command, "location", local.RefundTo.Location.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.RefundToOrganization.Number = db.GetString(reader, 0);
        entities.RefundToOrganization.Type1 = db.GetString(reader, 1);
        entities.RefundToOrganization.Populated = true;
        CheckValid<CsePerson>("Type1", entities.RefundToOrganization.Type1);
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      null,
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Populated = true;
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var refundedAmount =
      entities.CashReceiptDetail.RefundedAmount.GetValueOrDefault() +
      entities.ReceiptRefund.Amount;
    var collectionAmtFullyAppliedInd =
      local.Undistributed.CollectionAmtFullyAppliedInd ?? "";

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "refundedAmt", refundedAmount);
        db.SetNullableString(
          command, "collamtApplInd", collectionAmtFullyAppliedInd);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.RefundedAmount = refundedAmount;
    entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
      collectionAmtFullyAppliedInd;
    entities.CashReceiptDetail.Populated = true;
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
    /// A value of RefundToCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("refundToCashReceiptSourceType")]
    public CashReceiptSourceType RefundToCashReceiptSourceType
    {
      get => refundToCashReceiptSourceType ??= new();
      set => refundToCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of FromDistributionProcess.
    /// </summary>
    [JsonPropertyName("fromDistributionProcess")]
    public Common FromDistributionProcess
    {
      get => fromDistributionProcess ??= new();
      set => fromDistributionProcess = value;
    }

    /// <summary>
    /// A value of SendTo.
    /// </summary>
    [JsonPropertyName("sendTo")]
    public CashReceiptDetailAddress SendTo
    {
      get => sendTo ??= new();
      set => sendTo = value;
    }

    /// <summary>
    /// A value of NewAddress.
    /// </summary>
    [JsonPropertyName("newAddress")]
    public Common NewAddress
    {
      get => newAddress ??= new();
      set => newAddress = value;
    }

    /// <summary>
    /// A value of RefundToCsePerson.
    /// </summary>
    [JsonPropertyName("refundToCsePerson")]
    public CsePerson RefundToCsePerson
    {
      get => refundToCsePerson ??= new();
      set => refundToCsePerson = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
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

    private CashReceiptSourceType refundToCashReceiptSourceType;
    private Common fromDistributionProcess;
    private CashReceiptDetailAddress sendTo;
    private Common newAddress;
    private CsePerson refundToCsePerson;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private ReceiptRefund receiptRefund;
    private CashReceiptSourceType cashReceiptSourceType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of SendTo.
    /// </summary>
    [JsonPropertyName("sendTo")]
    public CashReceiptDetailAddress SendTo
    {
      get => sendTo ??= new();
      set => sendTo = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    private PaymentStatus paymentStatus;
    private CashReceiptDetailAddress sendTo;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson csePerson;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private ReceiptRefund receiptRefund;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RefundToPayment.
    /// </summary>
    [JsonPropertyName("refundToPayment")]
    public CsePerson RefundToPayment
    {
      get => refundToPayment ??= new();
      set => refundToPayment = value;
    }

    /// <summary>
    /// A value of RefundTo.
    /// </summary>
    [JsonPropertyName("refundTo")]
    public CashReceiptSourceType RefundTo
    {
      get => refundTo ??= new();
      set => refundTo = value;
    }

    /// <summary>
    /// A value of Undistributed.
    /// </summary>
    [JsonPropertyName("undistributed")]
    public CashReceiptDetail Undistributed
    {
      get => undistributed ??= new();
      set => undistributed = value;
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
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
    }

    /// <summary>
    /// A value of Refunded.
    /// </summary>
    [JsonPropertyName("refunded")]
    public CashReceiptDetailStatus Refunded
    {
      get => refunded ??= new();
      set => refunded = value;
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
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public CashReceiptDetailStatus Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
    }

    /// <summary>
    /// A value of Adjusted.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public CashReceiptDetailStatus Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
    }

    /// <summary>
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of ClearingFund.
    /// </summary>
    [JsonPropertyName("clearingFund")]
    public Fund ClearingFund
    {
      get => clearingFund ??= new();
      set => clearingFund = value;
    }

    /// <summary>
    /// A value of RevenueReversal.
    /// </summary>
    [JsonPropertyName("revenueReversal")]
    public ProgramCostAccount RevenueReversal
    {
      get => revenueReversal ??= new();
      set => revenueReversal = value;
    }

    /// <summary>
    /// A value of Refund.
    /// </summary>
    [JsonPropertyName("refund")]
    public FundTransactionType Refund
    {
      get => refund ??= new();
      set => refund = value;
    }

    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public FundTransactionStatusHistory Active
    {
      get => active ??= new();
      set => active = value;
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
    /// A value of Attempts.
    /// </summary>
    [JsonPropertyName("attempts")]
    public Common Attempts
    {
      get => attempts ??= new();
      set => attempts = value;
    }

    private CsePerson refundToPayment;
    private CashReceiptSourceType refundTo;
    private CashReceiptDetail undistributed;
    private DateWorkArea current;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CashReceiptDetailStatus refunded;
    private DateWorkArea max;
    private CashReceiptDetailStatus suspended;
    private CashReceiptDetailStatus adjusted;
    private ReceiptRefund receiptRefund;
    private Fund clearingFund;
    private ProgramCostAccount revenueReversal;
    private FundTransactionType refund;
    private FundTransaction fundTransaction;
    private FundTransactionStatusHistory active;
    private DateWorkArea maximum;
    private Common attempts;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of RefundToOrganization.
    /// </summary>
    [JsonPropertyName("refundToOrganization")]
    public CsePerson RefundToOrganization
    {
      get => refundToOrganization ??= new();
      set => refundToOrganization = value;
    }

    /// <summary>
    /// A value of RefundToCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("refundToCashReceiptSourceType")]
    public CashReceiptSourceType RefundToCashReceiptSourceType
    {
      get => refundToCashReceiptSourceType ??= new();
      set => refundToCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of AdjustmentCashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("adjustmentCashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj AdjustmentCashReceiptDetailBalanceAdj
    {
      get => adjustmentCashReceiptDetailBalanceAdj ??= new();
      set => adjustmentCashReceiptDetailBalanceAdj = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

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
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    /// <summary>
    /// A value of RefundToCsePerson.
    /// </summary>
    [JsonPropertyName("refundToCsePerson")]
    public CsePerson RefundToCsePerson
    {
      get => refundToCsePerson ??= new();
      set => refundToCsePerson = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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

    private CollectionType collectionType;
    private Fips fips;
    private CsePerson refundToOrganization;
    private CashReceiptSourceType refundToCashReceiptSourceType;
    private CashReceiptDetailBalanceAdj adjustmentCashReceiptDetailBalanceAdj;
    private CashReceiptDetail adjustmentCashReceiptDetail;
    private ObligationType obligationType;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private Collection collection;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private PaymentRequest paymentRequest;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private FundTransaction fundTransaction;
    private CsePerson refundToCsePerson;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private ReceiptRefund receiptRefund;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
  }
#endregion
}
