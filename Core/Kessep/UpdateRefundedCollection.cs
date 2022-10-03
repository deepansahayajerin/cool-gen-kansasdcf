// Program: UPDATE_REFUNDED_COLLECTION, ID: 371773783, model: 746.
// Short name: SWE01503
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: UPDATE_REFUNDED_COLLECTION.
/// </summary>
[Serializable]
public partial class UpdateRefundedCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_REFUNDED_COLLECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateRefundedCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateRefundedCollection.
  /// </summary>
  public UpdateRefundedCollection(IContext context, Import import, Export export)
    :
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
    // 12/13/95  Holly Kennedy-MTW		Source
    // 04/29/97  JeHoward - DIR		Current date fix
    // 05/12/99  Sunya Sharp			Add logic to update the cash receipt detail 
    // history
    // 					before updating the cash receipt detail.
    // 10/22/99  Sunya Sharp			Read is not fully defined.  Unable to complete
    // 					H00077672 without fixing.
    // 02/13/18  GVandy	CQ45790		Refunds for cash receipt details which 
    // originated
    // 					at the KPC will be sent back to the KPC through
    // 					the Notice of Collections job.  Set new attributes
    // 					on the receipt refund to faciliate the process.
    // ----------------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    export.CashReceiptDetailAddress.Assign(import.CashReceiptDetailAddress);
    export.ReceiptRefund.Assign(import.ReceiptRefund);

    if (!ReadReceiptRefund())
    {
      ExitState = "FN0000_RCPT_REFUND_NF";

      return;
    }

    // *** Add missing relationship to read.  Sunya Sharp 10/22/1999 ***
    if (!ReadPaymentRequestPaymentStatusHistoryPaymentStatus())
    {
      ExitState = "FN0011_PYMNT_REQUEST_NF";

      return;
    }

    if (!IsEmpty(entities.CurrentPaymentRequest.Number) || entities
      .CurrentPaymentRequest.PrintDate != null || !
      Equal(entities.CurrentPaymentStatus.Code, "REQ"))
    {
      ExitState = "FN0000_CANT_UPD_OR_DEL_REFUND";

      return;
    }

    if (!ReadCashReceiptDetail())
    {
      ExitState = "FN0053_CASH_RCPT_DTL_NF_RB";

      return;
    }

    local.ReceiptRefund.Amount = entities.CashReceiptDetail.ReceivedAmount - (
      entities.CashReceiptDetail.RefundedAmount.GetValueOrDefault() + import
      .Difference.Amount);

    if (local.ReceiptRefund.Amount < 0)
    {
      ExitState = "FN0000_RFND_AMT_EXCEED_AVAILABLE";

      return;
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

    if (local.Undistributed.CollectionAmount == entities
      .CashReceiptDetail.CollectionAmount - entities
      .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - (
        entities.CashReceiptDetail.RefundedAmount.GetValueOrDefault() + import
      .Difference.Amount))
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
      export.CashReceiptDetail.RefundedAmount =
        entities.CashReceiptDetail.RefundedAmount;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0054_CASH_RCPT_DTL_NU";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0056_CASH_RCPT_DTL_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
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

    try
    {
      UpdateReceiptRefund();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_ROLLBACK_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_ROLLBACK_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    MoveReceiptRefund(entities.ReceiptRefund, export.ReceiptRefund);

    if (AsChar(import.AddressChanged.Flag) == 'Y')
    {
      if (!ReadCashReceiptDetailAddress())
      {
        ExitState = "FN0000_ROLLBACK_RB";

        return;
      }

      try
      {
        CreateCashReceiptDetailAddress();
        export.CashReceiptDetailAddress.Assign(entities.New1);
        TransferReceiptRefund();
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

    if (!ReadPaymentRequest())
    {
      ExitState = "FILE_READ_ERROR_WITH_RB";

      return;
    }

    try
    {
      UpdatePaymentRequest();

      // ok
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FILE_WRITE_ERROR_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_PAYMENT_REQUEST_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveReceiptRefund(ReceiptRefund source,
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

  private void UseFnAbRecordCrDetailHistory()
  {
    var useImport = new FnAbRecordCrDetailHistory.Import();
    var useExport = new FnAbRecordCrDetailHistory.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
    useImport.CollectionType.SequentialIdentifier =
      import.CollectionType.SequentialIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(FnAbRecordCrDetailHistory.Execute, useImport, useExport);
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

    entities.New1.Populated = false;
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

    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.Street1 = street1;
    entities.New1.Street2 = street2;
    entities.New1.City = city;
    entities.New1.State = state;
    entities.New1.ZipCode5 = zipCode5;
    entities.New1.ZipCode4 = zipCode4;
    entities.New1.ZipCode3 = zipCode3;
    entities.New1.Populated = true;
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          entities.ReceiptRefund.CrdIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "crvIdentifier",
          entities.ReceiptRefund.CrvIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          entities.ReceiptRefund.CstIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "crtIdentifier",
          entities.ReceiptRefund.CrtIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
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
        entities.CashReceiptDetail.Reference = db.GetNullableString(reader, 39);
        entities.CashReceiptDetail.Notes = db.GetNullableString(reader, 40);
        entities.CashReceiptDetail.Populated = true;
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

  private bool ReadCashReceiptDetailAddress()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "crdetailAddressI",
          entities.ReceiptRefund.CdaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.CashReceiptDetailAddress.Populated = true;
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 1);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 2);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.PaymentRequest.Populated = true;
      });
  }

  private bool ReadPaymentRequestPaymentStatusHistoryPaymentStatus()
  {
    entities.CurrentPaymentStatus.Populated = false;
    entities.CurrentPaymentStatusHistory.Populated = false;
    entities.CurrentPaymentRequest.Populated = false;

    return Read("ReadPaymentRequestPaymentStatusHistoryPaymentStatus",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrentPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CurrentPaymentStatusHistory.PrqGeneratedId =
          db.GetInt32(reader, 0);
        entities.CurrentPaymentRequest.Number = db.GetNullableString(reader, 1);
        entities.CurrentPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 2);
        entities.CurrentPaymentRequest.Type1 = db.GetString(reader, 3);
        entities.CurrentPaymentRequest.RctRTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.CurrentPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.CurrentPaymentStatusHistory.PstGeneratedId =
          db.GetInt32(reader, 6);
        entities.CurrentPaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.CurrentPaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.CurrentPaymentStatusHistory.EffectiveDate =
          db.GetDate(reader, 8);
        entities.CurrentPaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.CurrentPaymentStatus.Code = db.GetString(reader, 10);
        entities.CurrentPaymentStatus.Populated = true;
        entities.CurrentPaymentStatusHistory.Populated = true;
        entities.CurrentPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.CurrentPaymentRequest.Type1);
          
      });
  }

  private bool ReadReceiptRefund()
  {
    entities.ReceiptRefund.Populated = false;

    return Read("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ReceiptRefund.PayeeName = db.GetNullableString(reader, 3);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.ReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 8);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 9);
        entities.ReceiptRefund.CdaIdentifier =
          db.GetNullableDateTime(reader, 10);
        entities.ReceiptRefund.ReasonText = db.GetNullableString(reader, 11);
        entities.ReceiptRefund.LastUpdatedBy = db.GetNullableString(reader, 12);
        entities.ReceiptRefund.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 13);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 14);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 15);
        entities.ReceiptRefund.KpcNoticeReqInd =
          db.GetNullableString(reader, 16);
        entities.ReceiptRefund.KpcNoticeProcessedDate =
          db.GetNullableDate(reader, 17);
        entities.ReceiptRefund.Populated = true;
      });
  }

  private void TransferReceiptRefund()
  {
    var cdaIdentifier = entities.New1.SystemGeneratedIdentifier;

    entities.ReceiptRefund.Populated = false;
    Update("TransferReceiptRefund",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "cdaIdentifier", cdaIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ReceiptRefund.CdaIdentifier = cdaIdentifier;
    entities.ReceiptRefund.Populated = true;
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var refundedAmount =
      entities.CashReceiptDetail.RefundedAmount.GetValueOrDefault() +
      import.Difference.Amount;
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

  private void UpdatePaymentRequest()
  {
    var amount = import.ReceiptRefund.Amount;

    entities.PaymentRequest.Populated = false;
    Update("UpdatePaymentRequest",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetInt32(
          command, "paymentRequestId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      });

    entities.PaymentRequest.Amount = amount;
    entities.PaymentRequest.Populated = true;
  }

  private void UpdateReceiptRefund()
  {
    var reasonCode = import.ReceiptRefund.ReasonCode;
    var taxid = import.ReceiptRefund.Taxid ?? "";
    var payeeName = import.ReceiptRefund.PayeeName ?? "";
    var amount = import.ReceiptRefund.Amount;
    var offsetTaxYear = import.ReceiptRefund.OffsetTaxYear.GetValueOrDefault();
    var requestDate = import.ReceiptRefund.RequestDate;
    var reasonText = import.ReceiptRefund.ReasonText ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var kpcNoticeReqInd = local.ReceiptRefund.KpcNoticeReqInd ?? "";
    var kpcNoticeProcessedDate = local.ReceiptRefund.KpcNoticeProcessedDate;

    entities.ReceiptRefund.Populated = false;
    Update("UpdateReceiptRefund",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetNullableString(command, "taxid", taxid);
        db.SetNullableString(command, "payeeName", payeeName);
        db.SetDecimal(command, "amount", amount);
        db.SetNullableInt32(command, "offsetTaxYear", offsetTaxYear);
        db.SetDate(command, "requestDate", requestDate);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "kpcNoticeReqInd", kpcNoticeReqInd);
        db.SetNullableDate(command, "kpcNoticeProcDt", kpcNoticeProcessedDate);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ReceiptRefund.ReasonCode = reasonCode;
    entities.ReceiptRefund.Taxid = taxid;
    entities.ReceiptRefund.PayeeName = payeeName;
    entities.ReceiptRefund.Amount = amount;
    entities.ReceiptRefund.OffsetTaxYear = offsetTaxYear;
    entities.ReceiptRefund.RequestDate = requestDate;
    entities.ReceiptRefund.ReasonText = reasonText;
    entities.ReceiptRefund.LastUpdatedBy = lastUpdatedBy;
    entities.ReceiptRefund.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ReceiptRefund.KpcNoticeReqInd = kpcNoticeReqInd;
    entities.ReceiptRefund.KpcNoticeProcessedDate = kpcNoticeProcessedDate;
    entities.ReceiptRefund.Populated = true;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of Difference.
    /// </summary>
    [JsonPropertyName("difference")]
    public ReceiptRefund Difference
    {
      get => difference ??= new();
      set => difference = value;
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
    /// A value of AddressChanged.
    /// </summary>
    [JsonPropertyName("addressChanged")]
    public Common AddressChanged
    {
      get => addressChanged ??= new();
      set => addressChanged = value;
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

    private CollectionType collectionType;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptSourceType cashReceiptSourceType;
    private ReceiptRefund difference;
    private CashReceiptDetailAddress sendTo;
    private Common addressChanged;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private ReceiptRefund receiptRefund;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    private CashReceiptDetail cashReceiptDetail;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    private CashReceiptDetail undistributed;
    private DateWorkArea current;
    private ReceiptRefund receiptRefund;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CurrentPaymentStatus.
    /// </summary>
    [JsonPropertyName("currentPaymentStatus")]
    public PaymentStatus CurrentPaymentStatus
    {
      get => currentPaymentStatus ??= new();
      set => currentPaymentStatus = value;
    }

    /// <summary>
    /// A value of CurrentPaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("currentPaymentStatusHistory")]
    public PaymentStatusHistory CurrentPaymentStatusHistory
    {
      get => currentPaymentStatusHistory ??= new();
      set => currentPaymentStatusHistory = value;
    }

    /// <summary>
    /// A value of CurrentPaymentRequest.
    /// </summary>
    [JsonPropertyName("currentPaymentRequest")]
    public PaymentRequest CurrentPaymentRequest
    {
      get => currentPaymentRequest ??= new();
      set => currentPaymentRequest = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceiptDetailAddress New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    private PaymentStatus currentPaymentStatus;
    private PaymentStatusHistory currentPaymentStatusHistory;
    private PaymentRequest currentPaymentRequest;
    private PaymentRequest paymentRequest;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailAddress new1;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private ReceiptRefund receiptRefund;
  }
#endregion
}
