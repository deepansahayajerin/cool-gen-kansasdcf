// Program: CANCEL_ADVANCEMENT, ID: 371725118, model: 746.
// Short name: SWE00110
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CANCEL_ADVANCEMENT.
/// </summary>
[Serializable]
public partial class CancelAdvancement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CANCEL_ADVANCEMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CancelAdvancement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CancelAdvancement.
  /// </summary>
  public CancelAdvancement(IContext context, Import import, Export export):
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
    // * * * * * * * * * * * * * * * * * * * * *
    // 05/13/97 M. D. Wheaton    Change CURRENT DATE
    // * * * * * * * * * * * * * * * * * * * * *
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadReceiptRefund())
    {
      // Receipt refund cancellation is not permitted for advancement attached 
      // to warrant.
      // *****
      // Only allow deletion the same day to ensure that processing has not 
      // occurred
      // *****
      if (Lt(Date(entities.ReceiptRefund.CreatedTimestamp), local.Current.Date))
      {
        ExitState = "FN0000_ADVANCEMENT_HAS_WARRANT";

        return;
      }

      if (ReadCashReceiptDetail())
      {
        ExitState = "FN0000_ADVANCEMENT_HAS_RCPT_DTL";

        return;
      }
      else
      {
        // OK, did not want to find one.
      }

      if (ReadPaymentRequest())
      {
        if (ReadPaymentStatusHistory())
        {
          DeletePaymentStatusHistory();
        }
        else
        {
          ExitState = "ZD_FN0000_PYMNT_STAT_NF_RB_1";

          return;
        }

        DeleteReceiptRefund();
        DeletePaymentRequest();
      }
      else
      {
        ExitState = "FN0000_PYMT_REQUEST_NF";
      }
    }
    else
    {
      ExitState = "FN0000_RCPT_REFUND_NF";
    }
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
        entities.CashReceiptDetail.Populated = true;
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

  private bool ReadPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.Populated = true;
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
        entities.ReceiptRefund.ReasonText = db.GetNullableString(reader, 10);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 11);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 12);
        entities.ReceiptRefund.Populated = true;
      });
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    private ReceiptRefund receiptRefund;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    private PaymentRequest paymentRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
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
    /// A value of Inactive.
    /// </summary>
    [JsonPropertyName("inactive")]
    public FundTransactionStatus Inactive
    {
      get => inactive ??= new();
      set => inactive = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      inactive = null;
    }

    private DateWorkArea current;
    private FundTransactionStatus inactive;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Fund.
    /// </summary>
    [JsonPropertyName("fund")]
    public Fund Fund
    {
      get => fund ??= new();
      set => fund = value;
    }

    /// <summary>
    /// A value of PcaFundExplosionRule.
    /// </summary>
    [JsonPropertyName("pcaFundExplosionRule")]
    public PcaFundExplosionRule PcaFundExplosionRule
    {
      get => pcaFundExplosionRule ??= new();
      set => pcaFundExplosionRule = value;
    }

    /// <summary>
    /// A value of FundTransactionType.
    /// </summary>
    [JsonPropertyName("fundTransactionType")]
    public FundTransactionType FundTransactionType
    {
      get => fundTransactionType ??= new();
      set => fundTransactionType = value;
    }

    /// <summary>
    /// A value of ProgramCostAccount.
    /// </summary>
    [JsonPropertyName("programCostAccount")]
    public ProgramCostAccount ProgramCostAccount
    {
      get => programCostAccount ??= new();
      set => programCostAccount = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
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

    private PaymentStatusHistory paymentStatusHistory;
    private CashReceiptDetail cashReceiptDetail;
    private Fund fund;
    private PcaFundExplosionRule pcaFundExplosionRule;
    private FundTransactionType fundTransactionType;
    private ProgramCostAccount programCostAccount;
    private FundTransaction fundTransaction;
    private ReceiptRefund receiptRefund;
    private PaymentRequest paymentRequest;
  }
#endregion
}
