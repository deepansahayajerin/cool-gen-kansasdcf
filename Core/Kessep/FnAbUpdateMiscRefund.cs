// Program: FN_AB_UPDATE_MISC_REFUND, ID: 372312308, model: 746.
// Short name: SWE00249
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_AB_UPDATE_MISC_REFUND.
/// </summary>
[Serializable]
public partial class FnAbUpdateMiscRefund: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_UPDATE_MISC_REFUND program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbUpdateMiscRefund(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbUpdateMiscRefund.
  /// </summary>
  public FnAbUpdateMiscRefund(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------
    // Date	  Developer Name	Request #	Description
    // 01/29/95  Holly Kennedy-MTW			Source
    // 04/28/97  Ty Hill-MTW                           Change Current_date
    // ------------------------------------------------------------------------
    // 	
    local.Current.Date = Now().Date;

    if (ReadReceiptRefund())
    {
      // ************************************************
      // *If a Warrant has already been cut for this    *
      // *refund, then do not allow an update.          *
      // ************************************************
      if (ReadPaymentRequestPaymentStatusHistoryPaymentStatus())
      {
        if (entities.ExistingPaymentRequest.PrintDate != null || !
          IsEmpty(entities.ExistingPaymentRequest.Number) && !
          Equal(entities.ExistingPaymentStatus.Code, "REQ"))
        {
          ExitState = "FN0000_CANT_UPD_OR_DEL_REFUND";

          return;
        }
      }
      else
      {
        // **** Continue ****
      }

      // ************************************************
      // *Update the Receipt Refund                     *
      // ************************************************
      try
      {
        UpdateReceiptRefund();

        // ************************************************
        // *Update the Payment Request                    *
        // ************************************************
        if (ReadPaymentRequest())
        {
          try
          {
            UpdatePaymentRequest();

            // ok
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_ROLLBACK_RB";

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
    }
    else
    {
      ExitState = "FN0000_RCPT_REFUND_NF";
    }
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
    entities.ExistingPaymentStatus.Populated = false;
    entities.ExistingPaymentStatusHistory.Populated = false;
    entities.ExistingPaymentRequest.Populated = false;

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
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentStatusHistory.PrqGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentRequest.Number =
          db.GetNullableString(reader, 1);
        entities.ExistingPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingPaymentRequest.Type1 = db.GetString(reader, 3);
        entities.ExistingPaymentRequest.RctRTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.ExistingPaymentStatusHistory.PstGeneratedId =
          db.GetInt32(reader, 6);
        entities.ExistingPaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingPaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingPaymentStatusHistory.EffectiveDate =
          db.GetDate(reader, 8);
        entities.ExistingPaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingPaymentStatus.Code = db.GetString(reader, 10);
        entities.ExistingPaymentStatus.Populated = true;
        entities.ExistingPaymentStatusHistory.Populated = true;
        entities.ExistingPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1",
          entities.ExistingPaymentRequest.Type1);
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
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 5);
        entities.ReceiptRefund.ReasonText = db.GetNullableString(reader, 6);
        entities.ReceiptRefund.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.ReceiptRefund.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.ReceiptRefund.Populated = true;
      });
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
    var requestDate = import.ReceiptRefund.RequestDate;
    var reasonText = import.ReceiptRefund.ReasonText ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ReceiptRefund.Populated = false;
    Update("UpdateReceiptRefund",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetNullableString(command, "taxid", taxid);
        db.SetNullableString(command, "payeeName", payeeName);
        db.SetDecimal(command, "amount", amount);
        db.SetDate(command, "requestDate", requestDate);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ReceiptRefund.ReasonCode = reasonCode;
    entities.ReceiptRefund.Taxid = taxid;
    entities.ReceiptRefund.PayeeName = payeeName;
    entities.ReceiptRefund.Amount = amount;
    entities.ReceiptRefund.RequestDate = requestDate;
    entities.ReceiptRefund.ReasonText = reasonText;
    entities.ReceiptRefund.LastUpdatedBy = lastUpdatedBy;
    entities.ReceiptRefund.LastUpdatedTimestamp = lastUpdatedTimestamp;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public ReceiptRefund Current
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public CashReceiptDetailAddress Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private ReceiptRefund current;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CashReceiptSourceType cashReceiptSourceType;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptDetailAddress existing;
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
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingPaymentStatus.
    /// </summary>
    [JsonPropertyName("existingPaymentStatus")]
    public PaymentStatus ExistingPaymentStatus
    {
      get => existingPaymentStatus ??= new();
      set => existingPaymentStatus = value;
    }

    /// <summary>
    /// A value of ExistingPaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("existingPaymentStatusHistory")]
    public PaymentStatusHistory ExistingPaymentStatusHistory
    {
      get => existingPaymentStatusHistory ??= new();
      set => existingPaymentStatusHistory = value;
    }

    /// <summary>
    /// A value of ExistingPaymentRequest.
    /// </summary>
    [JsonPropertyName("existingPaymentRequest")]
    public PaymentRequest ExistingPaymentRequest
    {
      get => existingPaymentRequest ??= new();
      set => existingPaymentRequest = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of ExistingCashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailAddress")]
    public CashReceiptDetailAddress ExistingCashReceiptDetailAddress
    {
      get => existingCashReceiptDetailAddress ??= new();
      set => existingCashReceiptDetailAddress = value;
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
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    private PaymentStatus existingPaymentStatus;
    private PaymentStatusHistory existingPaymentStatusHistory;
    private PaymentRequest existingPaymentRequest;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private PaymentRequest paymentRequest;
    private CashReceiptDetailAddress existingCashReceiptDetailAddress;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
  }
#endregion
}
