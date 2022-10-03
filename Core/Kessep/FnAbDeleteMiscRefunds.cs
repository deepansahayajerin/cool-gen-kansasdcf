// Program: FN_AB_DELETE_MISC_REFUNDS, ID: 372312307, model: 746.
// Short name: SWE00242
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_AB_DELETE_MISC_REFUNDS.
/// </summary>
[Serializable]
public partial class FnAbDeleteMiscRefunds: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_DELETE_MISC_REFUNDS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbDeleteMiscRefunds(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbDeleteMiscRefunds.
  /// </summary>
  public FnAbDeleteMiscRefunds(IContext context, Import import, Export export):
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
    // 01/29/95  Holly Kennedy-MTW			Source
    // 04/28/97  Ty Hill-MTW                           Change Current_date
    // ------------------------------------------------------------------------
    // 	
    local.Current.Date = Now().Date;

    if (ReadReceiptRefund())
    {
      // *****
      // Disallow deletion if Warrant processing has occurred
      // *****
      if (ReadPaymentRequestPaymentStatusPaymentStatusHistory())
      {
        if (entities.ExistingPaymentRequest.PrintDate != null || !
          IsEmpty(entities.ExistingPaymentRequest.Number) || !
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

      // *****
      // Read and delete associated Address.
      // *****
      if (!ReadCashReceiptDetailAddress())
      {
        ExitState = "FN0000_ROLLBACK_RB";

        return;
      }

      // *****
      // Delete Payment Request Status History records
      // *****
      if (ReadPaymentRequest())
      {
        foreach(var item in ReadPaymentStatusHistory())
        {
          DeletePaymentStatusHistory();
        }

        DeletePaymentRequest();
      }
      else
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        return;
      }

      // *****
      // All is ok, delete the Refund.  The Payment Request will automatically 
      // be deleted because of Delete Rules.
      // *****
      DeleteReceiptRefund();
      DeleteCashReceiptDetailAddress();
    }
    else
    {
      ExitState = "FN0000_RCPT_REFUND_NF";
    }
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

  private void DeletePaymentRequest()
  {
    bool exists;

    Update("DeletePaymentRequest#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      });

    exists = Read("DeletePaymentRequest#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ELEC_FUND_TRAN\".",
        "50001");
    }

    Update("DeletePaymentRequest#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      });

    Update("DeletePaymentRequest#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      });

    exists = Read("DeletePaymentRequest#5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_POT_RECOVERY\".",
        "50001");
    }

    Update("DeletePaymentRequest#6",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
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
        entities.PaymentRequest.Number = db.GetNullableString(reader, 1);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 2);
        entities.PaymentRequest.Type1 = db.GetString(reader, 3);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 4);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentRequestPaymentStatusPaymentStatusHistory()
  {
    entities.ExistingPaymentStatusHistory.Populated = false;
    entities.ExistingPaymentStatus.Populated = false;
    entities.ExistingPaymentRequest.Populated = false;

    return Read("ReadPaymentRequestPaymentStatusPaymentStatusHistory",
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
        entities.ExistingPaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingPaymentStatusHistory.PstGeneratedId =
          db.GetInt32(reader, 6);
        entities.ExistingPaymentStatus.Code = db.GetString(reader, 7);
        entities.ExistingPaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.ExistingPaymentStatusHistory.EffectiveDate =
          db.GetDate(reader, 9);
        entities.ExistingPaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingPaymentStatusHistory.Populated = true;
        entities.ExistingPaymentStatus.Populated = true;
        entities.ExistingPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1",
          entities.ExistingPaymentRequest.Type1);
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
        entities.ReceiptRefund.CdaIdentifier =
          db.GetNullableDateTime(reader, 6);
        entities.ReceiptRefund.ReasonText = db.GetNullableString(reader, 7);
        entities.ReceiptRefund.Populated = true;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private ReceiptRefund receiptRefund;
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
    /// A value of ExistingPaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("existingPaymentStatusHistory")]
    public PaymentStatusHistory ExistingPaymentStatusHistory
    {
      get => existingPaymentStatusHistory ??= new();
      set => existingPaymentStatusHistory = value;
    }

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
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of ExistingPaymentRequest.
    /// </summary>
    [JsonPropertyName("existingPaymentRequest")]
    public PaymentRequest ExistingPaymentRequest
    {
      get => existingPaymentRequest ??= new();
      set => existingPaymentRequest = value;
    }

    private PaymentStatusHistory existingPaymentStatusHistory;
    private PaymentStatus existingPaymentStatus;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
    private PaymentStatusHistory paymentStatusHistory;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private PaymentRequest paymentRequest;
    private ReceiptRefund receiptRefund;
    private PaymentRequest existingPaymentRequest;
  }
#endregion
}
