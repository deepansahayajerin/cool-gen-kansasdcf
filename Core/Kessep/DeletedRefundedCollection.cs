// Program: DELETED_REFUNDED_COLLECTION, ID: 371773237, model: 746.
// Short name: SWE00203
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: DELETED_REFUNDED_COLLECTION.
/// </summary>
[Serializable]
public partial class DeletedRefundedCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETED_REFUNDED_COLLECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeletedRefundedCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeletedRefundedCollection.
  /// </summary>
  public DeletedRefundedCollection(IContext context, Import import,
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
    // ------------------------------------------------------------------------
    // Date	  Developer Name	Request #	Description
    // 12/13/95  Holly Kennedy-MTW			Source
    // 04/29/97  Ty Hill-MTW                           Change Current_date
    // 02/26/99  Sunya Sharp		Add logic to update the collection fully applied 
    // amount indicator.
    // 05/05/99  Sunya Sharp		Add logic to support the new cash receipt detail 
    // status field.  Add logic to put the previous reason code and reason text
    // when reverting to the previous status when deleting a refund.
    // 05/12/99  Sunya Sharp		Add logic to create the cash receipt detail 
    // history when the cash receipt detail is updated.
    // 11/5/99	Sunya Sharp	PR# 00079394 Add logic to check if the address is 
    // associated to another refund before deleting.
    // ------------------------------------------------------------------------
    // 	
    local.Current.Date = Now().Date;
    local.Max.DiscontinueDate = UseCabSetMaximumDiscontinueDate();
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadCashReceiptDetailCashReceipt())
    {
      if (ReadReceiptRefund1())
      {
        if (ReadPaymentRequestPaymentStatusPaymentStatusHistory())
        {
          if (!IsEmpty(entities.CurrentPaymentRequest.Number) || entities
            .CurrentPaymentRequest.PrintDate != null || !
            Equal(entities.CurrentPaymentStatus.Code, "REQ"))
          {
            ExitState = "FN0000_CANT_UPD_OR_DEL_REFUND";

            return;
          }
        }
        else
        {
          ExitState = "FN0011_PYMNT_REQUEST_NF";

          return;
        }

        if (ReadPaymentRequest())
        {
          foreach(var item in ReadPaymentStatusHistory())
          {
            DeletePaymentStatusHistory();
          }
        }
        else
        {
          ExitState = "FILE_READ_ERROR_WITH_RB";

          return;
        }

        // *****
        // Prepare to reset the CRD Status
        // *****
        if (ReadCashReceiptDetail())
        {
          // *****
          // If a Receipt Refund still exists for the CRD do not change the 
          // status.
          // *****
          if (ReadReceiptRefund2())
          {
            goto Read;
          }

          // *****
          // If a Receipt Refund does not exist for the CRD set the status to 
          // the last known status.  If the status is not Distributed or
          // Suspended the refund cannot be deleted.
          // *****
          // *** The above note is not correct.  Changed logic to revert the 
          // cash receipt detail status to the previous status.  It does not
          // have to be suspended or distributed.  I reviewed this with the SME.
          // Sunya Sharp 2/24/1999 ***
          if (ReadCashReceiptDetailStatHistory())
          {
            foreach(var item in ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
              
            {
              if (Equal(entities.Past.CreatedTimestamp,
                entities.CurrentCashReceiptDetailStatHistory.CreatedTimestamp))
              {
                continue;
              }

              try
              {
                UpdateCashReceiptDetailStatHistory();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_ROLLBACK_RB";
                    ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_ROLLBACK_RB";
                    ExitState = "FN0074_CASH_RCPT_DTL_STAT_PV";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              UseFnCreateCashRcptDtlStatHis();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (IsExitState("FN0066_CASH_RCPT_DTL_STAT_HST_PV"))
                {
                  ExitState = "FN0105_CASH_RCPT_STAT_HIST_PV_RB";
                }
                else if (IsExitState("FN0063_CASH_RCPT_DTL_STAT_HST_AE"))
                {
                  ExitState = "FN0101_CASH_RCPT_STAT_HIST_AE_RB";
                }
                else
                {
                  ExitState = "FN0000_ROLLBACK_RB";
                }

                return;
              }

              goto Read;
            }
          }
        }

Read:

        export.PreviousStatus.Code = entities.CashReceiptDetailStatus.Code;

        // *****
        // Determine if the Refund Address is the same as the Cash Receipt 
        // Address.  If it is a different address, delete the Address.
        // *****
        // *** PR#00079394 - Add logic to check to see if the address is 
        // associated to another refund before deleting.  Sunya Sharp 11/05/1999
        // ***
        if (ReadCashReceiptDetailAddress1())
        {
          if (ReadCashReceiptDetailAddress2())
          {
            // ok
          }
          else
          {
            // ok
          }

          if (ReadCashReceiptDetailAddress3())
          {
            // ok
          }
          else
          {
            // ok
          }
        }

        DeleteReceiptRefund();

        if (Equal(entities.CashReceiptDetailAddress.SystemGeneratedIdentifier,
          entities.Crd.SystemGeneratedIdentifier))
        {
          // Do not delete
        }
        else if (Equal(entities.CashReceiptDetailAddress.
          SystemGeneratedIdentifier,
          entities.OtherRefundCashReceiptDetailAddress.
            SystemGeneratedIdentifier))
        {
          // Do not delete
        }
        else if (!Equal(entities.CashReceiptDetailAddress.
          SystemGeneratedIdentifier, local.Null1.SystemGeneratedIdentifier))
        {
          DeleteCashReceiptDetailAddress();
        }

        // *** Add logic to create a cash receipt detail history.  If something 
        // happens and the cash receipt detail is not updated a rollback is
        // performed.  This will rollback the history update as well.  Sunya
        // Sharp 5/6/1999 ***
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
              ExitState = "FN0054_CASH_RCPT_DTL_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_ROLLBACK_RB";
              ExitState = "FN0056_CASH_RCPT_DTL_PV";

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
        ExitState = "FN0000_RCPT_REFUND_NF";
      }
    }
    else
    {
      ExitState = "FN0052_CASH_RCPT_DTL_NF";
    }
  }

  private static void MoveCashReceiptDetailStatHistory(
    CashReceiptDetailStatHistory source, CashReceiptDetailStatHistory target)
  {
    target.ReasonCodeId = source.ReasonCodeId;
    target.ReasonText = source.ReasonText;
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

    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
    useImport.CollectionType.SequentialIdentifier =
      import.CollectionType.SequentialIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;

    Call(FnAbRecordCrDetailHistory.Execute, useImport, useExport);
  }

  private void UseFnCreateCashRcptDtlStatHis()
  {
    var useImport = new FnCreateCashRcptDtlStatHis.Import();
    var useExport = new FnCreateCashRcptDtlStatHis.Export();

    MoveCashReceiptDetailStatHistory(entities.Past,
      useImport.CashReceiptDetailStatHistory);
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      entities.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;

    Call(FnCreateCashRcptDtlStatHis.Execute, useImport, useExport);
  }

  private void DeleteCashReceiptDetailAddress()
  {
    Update("DeleteCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "crdetailAddressI",
          entities.CashReceiptDetailAddress.SystemGeneratedIdentifier.
            GetValueOrDefault());
      });
  }

  private void DeletePaymentStatusHistory()
  {
    Update("DeletePaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.PaymentStatusHistory.SystemGeneratedIdentifier);
      });
  }

  private void DeleteReceiptRefund()
  {
    bool exists;

    Update("DeleteReceiptRefund#1",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    exists = Read("DeleteReceiptRefund#2",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ELEC_FUND_TRAN\".",
        "50001");
    }

    Update("DeleteReceiptRefund#3",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    Update("DeleteReceiptRefund#4",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    exists = Read("DeleteReceiptRefund#5",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_POT_RECOVERY\".",
        "50001");
    }

    Update("DeleteReceiptRefund#6",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    Update("DeleteReceiptRefund#7",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });
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
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private bool ReadCashReceiptDetailAddress1()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress1",
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
        entities.CashReceiptDetailAddress.Street1 = db.GetString(reader, 1);
        entities.CashReceiptDetailAddress.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailAddress2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Crd.Populated = false;

    return Read("ReadCashReceiptDetailAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.Crd.SystemGeneratedIdentifier = db.GetDateTime(reader, 0);
        entities.Crd.CrtIdentifier = db.GetNullableInt32(reader, 1);
        entities.Crd.CstIdentifier = db.GetNullableInt32(reader, 2);
        entities.Crd.CrvIdentifier = db.GetNullableInt32(reader, 3);
        entities.Crd.CrdIdentifier = db.GetNullableInt32(reader, 4);
        entities.Crd.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailAddress3()
  {
    entities.OtherRefundCashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress3",
      (db, command) =>
      {
        db.SetDateTime(
          command, "crdetailAddressI",
          entities.CashReceiptDetailAddress.SystemGeneratedIdentifier.
            GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OtherRefundCashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.OtherRefundCashReceiptDetailAddress.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCashReceipt()
  {
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCashReceipt",
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
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 7);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 8);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private bool ReadCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CurrentCashReceiptDetailStatHistory.Populated = false;

    return Read("ReadCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate",
          local.Max.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrentCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CurrentCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CurrentCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CurrentCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CurrentCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.CurrentCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CurrentCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.CurrentCashReceiptDetailStatHistory.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Past.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;

    return ReadEach("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
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
        entities.Past.CrdIdentifier = db.GetInt32(reader, 0);
        entities.Past.CrvIdentifier = db.GetInt32(reader, 1);
        entities.Past.CstIdentifier = db.GetInt32(reader, 2);
        entities.Past.CrtIdentifier = db.GetInt32(reader, 3);
        entities.Past.CdsIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.Past.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Past.ReasonCodeId = db.GetNullableString(reader, 6);
        entities.Past.DiscontinueDate = db.GetNullableDate(reader, 7);
        entities.Past.ReasonText = db.GetNullableString(reader, 8);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 9);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.Past.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;

        return true;
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
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 1);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PaymentRequest.Populated = true;
      });
  }

  private bool ReadPaymentRequestPaymentStatusPaymentStatusHistory()
  {
    entities.CurrentPaymentStatusHistory.Populated = false;
    entities.CurrentPaymentStatus.Populated = false;
    entities.CurrentPaymentRequest.Populated = false;

    return Read("ReadPaymentRequestPaymentStatusPaymentStatusHistory",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
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
        entities.CurrentPaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.CurrentPaymentStatusHistory.PstGeneratedId =
          db.GetInt32(reader, 6);
        entities.CurrentPaymentStatus.Code = db.GetString(reader, 7);
        entities.CurrentPaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.CurrentPaymentStatusHistory.EffectiveDate =
          db.GetDate(reader, 9);
        entities.CurrentPaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.CurrentPaymentStatusHistory.Populated = true;
        entities.CurrentPaymentStatus.Populated = true;
        entities.CurrentPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.CurrentPaymentRequest.Type1);
          
      });
  }

  private IEnumerable<bool> ReadPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;

    return ReadEach("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.Populated = true;

        return true;
      });
  }

  private bool ReadReceiptRefund1()
  {
    entities.ReceiptRefund.Populated = false;

    return Read("ReadReceiptRefund1",
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
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 12);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 13);
        entities.ReceiptRefund.Populated = true;
      });
  }

  private bool ReadReceiptRefund2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Existing.Populated = false;

    return Read("ReadReceiptRefund2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          import.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.Existing.CrvIdentifier = db.GetNullableInt32(reader, 1);
        entities.Existing.CrdIdentifier = db.GetNullableInt32(reader, 2);
        entities.Existing.CrtIdentifier = db.GetNullableInt32(reader, 3);
        entities.Existing.CstIdentifier = db.GetNullableInt32(reader, 4);
        entities.Existing.Populated = true;
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var refundedAmount =
      entities.CashReceiptDetail.RefundedAmount.GetValueOrDefault() -
      import.ReceiptRefund.Amount;

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd", "");
    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "refundedAmt", refundedAmount);
        db.SetNullableString(command, "collamtApplInd", "");
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
    entities.CashReceiptDetail.CollectionAmtFullyAppliedInd = "";
    entities.CashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.CurrentCashReceiptDetailStatHistory.Populated);

    var discontinueDate = local.Current.Date;

    entities.CurrentCashReceiptDetailStatHistory.Populated = false;
    Update("UpdateCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crdIdentifier",
          entities.CurrentCashReceiptDetailStatHistory.CrdIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.CurrentCashReceiptDetailStatHistory.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CurrentCashReceiptDetailStatHistory.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CurrentCashReceiptDetailStatHistory.CrtIdentifier);
        db.SetInt32(
          command, "cdsIdentifier",
          entities.CurrentCashReceiptDetailStatHistory.CdsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CurrentCashReceiptDetailStatHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.CurrentCashReceiptDetailStatHistory.DiscontinueDate =
      discontinueDate;
    entities.CurrentCashReceiptDetailStatHistory.Populated = true;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CollectionType collectionType;
    private ReceiptRefund receiptRefund;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PreviousStatus.
    /// </summary>
    [JsonPropertyName("previousStatus")]
    public CashReceiptDetailStatus PreviousStatus
    {
      get => previousStatus ??= new();
      set => previousStatus = value;
    }

    private CashReceiptDetailStatus previousStatus;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public CashReceiptDetailStatus Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public CashReceiptDetailAddress Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private DateWorkArea current;
    private CashReceiptDetailStatus max;
    private CashReceiptDetailAddress null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OtherRefundReceiptRefund.
    /// </summary>
    [JsonPropertyName("otherRefundReceiptRefund")]
    public ReceiptRefund OtherRefundReceiptRefund
    {
      get => otherRefundReceiptRefund ??= new();
      set => otherRefundReceiptRefund = value;
    }

    /// <summary>
    /// A value of OtherRefundCashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("otherRefundCashReceiptDetailAddress")]
    public CashReceiptDetailAddress OtherRefundCashReceiptDetailAddress
    {
      get => otherRefundCashReceiptDetailAddress ??= new();
      set => otherRefundCashReceiptDetailAddress = value;
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
    /// A value of CurrentPaymentStatus.
    /// </summary>
    [JsonPropertyName("currentPaymentStatus")]
    public PaymentStatus CurrentPaymentStatus
    {
      get => currentPaymentStatus ??= new();
      set => currentPaymentStatus = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public ReceiptRefund Existing
    {
      get => existing ??= new();
      set => existing = value;
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
    /// A value of Past.
    /// </summary>
    [JsonPropertyName("past")]
    public CashReceiptDetailStatHistory Past
    {
      get => past ??= new();
      set => past = value;
    }

    /// <summary>
    /// A value of CurrentCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("currentCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CurrentCashReceiptDetailStatHistory
    {
      get => currentCashReceiptDetailStatHistory ??= new();
      set => currentCashReceiptDetailStatHistory = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Crd.
    /// </summary>
    [JsonPropertyName("crd")]
    public CashReceiptDetailAddress Crd
    {
      get => crd ??= new();
      set => crd = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CurrentPaymentRequest.
    /// </summary>
    [JsonPropertyName("currentPaymentRequest")]
    public PaymentRequest CurrentPaymentRequest
    {
      get => currentPaymentRequest ??= new();
      set => currentPaymentRequest = value;
    }

    private ReceiptRefund otherRefundReceiptRefund;
    private CashReceiptDetailAddress otherRefundCashReceiptDetailAddress;
    private PaymentStatusHistory currentPaymentStatusHistory;
    private PaymentStatus currentPaymentStatus;
    private ReceiptRefund existing;
    private PaymentStatusHistory paymentStatusHistory;
    private CashReceiptDetailStatHistory past;
    private CashReceiptDetailStatHistory currentCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private PaymentRequest paymentRequest;
    private CashReceiptDetailAddress crd;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private ReceiptRefund receiptRefund;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private PaymentRequest currentPaymentRequest;
  }
#endregion
}
