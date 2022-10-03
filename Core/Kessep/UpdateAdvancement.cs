// Program: UPDATE_ADVANCEMENT, ID: 371726644, model: 746.
// Short name: SWE01464
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: UPDATE_ADVANCEMENT.
/// </summary>
[Serializable]
public partial class UpdateAdvancement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_ADVANCEMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateAdvancement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateAdvancement.
  /// </summary>
  public UpdateAdvancement(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ·¼¼¼¼¼¼¼¼            Modification Log
    // xxxxxxxxxxxxxx		??/??/????	Initial Development
    // Siraj Konkader		09/11/1997
    // Always move imports to exports at the very begining when there is a 
    // potential escape.
    // ALWAYS make sure that ur import and export views contain the same 
    // attributes that r used by the PRAD, more so if those are being displayed
    // on the screen.
    // Sunya Sharp		01/27/1999	Add logic to support the last updated by and last
    // updated timestamp attributes.
    // Sunya Sharp		08/23/1999	Make changes per PR#120.  When updating the 
    // payment request need to ensure that it is the warrant.
    // ·©©©©©©©©
    ExitState = "ACO_NN0000_ALL_OK";
    export.PaymentRequest.Assign(import.PaymentRequest);
    export.ReceiptRefund.Assign(import.ReceiptRefund);

    if (ReadReceiptRefund())
    {
      // Receipt refund update is not allowed for advancement linked to warrant.
      if (Lt(Date(entities.ReceiptRefund.CreatedTimestamp), Now().Date))
      {
        ExitState = "FN0000_ADVANCEMENT_HAS_WARRANT";

        return;
      }

      // *** Make change per PR#120.  Sunya Sharp 08/23/1999 ***
      if (ReadPaymentRequest())
      {
        MovePaymentRequest(entities.PaymentRequest, export.PaymentRequest);
      }
      else
      {
        ExitState = "FN0000_PYMT_REQUEST_NF";

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

      try
      {
        UpdateReceiptRefund();
        MoveReceiptRefund(entities.ReceiptRefund, export.ReceiptRefund);
        UseUpdateCashRcptDetlAddress();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          try
          {
            UpdatePaymentRequest();

            // OK
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_ROLLBACK_RB";

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
        }
        else
        {
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_RCPT_REFUND_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_RCPT_REFUND_PV";

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

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.Number = source.Number;
    target.PrintDate = source.PrintDate;
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
    target.LastUpdatedBy = source.LastUpdatedBy;
  }

  private void UseUpdateCashRcptDetlAddress()
  {
    var useImport = new UpdateCashRcptDetlAddress.Import();
    var useExport = new UpdateCashRcptDetlAddress.Export();

    useImport.CashReceiptDetailAddress.Assign(import.CashReceiptDetailAddress);

    Call(UpdateCashRcptDetlAddress.Execute, useImport, useExport);
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
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 3);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 4);
        entities.PaymentRequest.Type1 = db.GetString(reader, 5);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 6);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 7);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
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
        entities.ReceiptRefund.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.ReceiptRefund.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 13);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 14);
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
    var offsetTaxYear = import.ReceiptRefund.OffsetTaxYear.GetValueOrDefault();
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
        db.SetNullableInt32(command, "offsetTaxYear", offsetTaxYear);
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
    entities.ReceiptRefund.OffsetTaxYear = offsetTaxYear;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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

    private PaymentRequest paymentRequest;
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

    private PaymentRequest paymentRequest;
    private ReceiptRefund receiptRefund;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Inactive.
    /// </summary>
    [JsonPropertyName("inactive")]
    public FundTransactionStatus Inactive
    {
      get => inactive ??= new();
      set => inactive = value;
    }

    private FundTransactionStatus inactive;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
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
    /// A value of PcaFundExplosionRule.
    /// </summary>
    [JsonPropertyName("pcaFundExplosionRule")]
    public PcaFundExplosionRule PcaFundExplosionRule
    {
      get => pcaFundExplosionRule ??= new();
      set => pcaFundExplosionRule = value;
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
    /// A value of ProgramCostAccount.
    /// </summary>
    [JsonPropertyName("programCostAccount")]
    public ProgramCostAccount ProgramCostAccount
    {
      get => programCostAccount ??= new();
      set => programCostAccount = value;
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

    private ReceiptRefund receiptRefund;
    private PaymentRequest paymentRequest;
    private FundTransaction fundTransaction;
    private FundTransactionType fundTransactionType;
    private PcaFundExplosionRule pcaFundExplosionRule;
    private Fund fund;
    private ProgramCostAccount programCostAccount;
    private CashReceiptDetail cashReceiptDetail;
  }
#endregion
}
