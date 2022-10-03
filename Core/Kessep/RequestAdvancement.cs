// Program: REQUEST_ADVANCEMENT, ID: 371726399, model: 746.
// Short name: SWE01067
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: REQUEST_ADVANCEMENT.
/// </summary>
[Serializable]
public partial class RequestAdvancement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the REQUEST_ADVANCEMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new RequestAdvancement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of RequestAdvancement.
  /// </summary>
  public RequestAdvancement(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************
    // Modification Log
    // xxxxxxxxxxxxxx		??/??/????	Initial Development
    // Siraj Konkader		09/11/1997
    // Always move imports to exports at the very begining when there is a 
    // potential escape.
    // Sunya Sharp		01/25/1999
    // Add logic for collection type.
    // ****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.CashReceiptDetailAddress.Assign(import.CashReceiptDetailAddress);
    export.ReceiptRefund.Assign(import.ReceiptRefund);
    export.PaymentRequest.Assign(import.PaymentRequest);
    MoveCollectionType(import.CollectionType, export.CollectionType);

    if (ReadCashReceiptSourceType())
    {
      MoveCashReceiptSourceType(entities.CashReceiptSourceType,
        export.CashReceiptSourceType);

      if (ReadCollectionType())
      {
        MoveCollectionType(entities.CollectionType, export.CollectionType);
      }
      else
      {
        ExitState = "FN0000_COLLECTION_TYPE_NF";

        return;
      }

      if (!ReadCsePerson())
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }

      try
      {
        CreateReceiptRefund();
        export.ReceiptRefund.Assign(entities.ReceiptRefund);

        try
        {
          CreateCashReceiptDetailAddress();
          export.CashReceiptDetailAddress.Assign(
            entities.CashReceiptDetailAddress);
          ExitState = "ACO_NN0000_ALL_OK";
        }
        catch(Exception e1)
        {
          switch(GetErrorCode(e1))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0038_CASH_RCPT_DTL_ADDR_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0041_CASH_RCPT_DTL_ADDR_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        try
        {
          CreatePaymentRequest();
          export.PaymentRequest.Assign(entities.PaymentRequest);
        }
        catch(Exception e1)
        {
          switch(GetErrorCode(e1))
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

        // --- Create pmt request status history ----
        if (ReadPaymentStatus())
        {
          try
          {
            CreatePaymentStatusHistory();
            export.PaymentStatus.Code = entities.PaymentStatus.Code;
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "ZD_FN0000_PYMNT_STAT_HIST_AE_RB2";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PYMNT_STAT_HIST_PV_RB";

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
          ExitState = "ZD_FN0000_PYMNT_STAT_NF_RB_3";
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_RCPT_REFUND_AE";

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
      ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";
    }
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private DateTime? UseFnAssignCashRcptAddrId()
  {
    var useImport = new FnAssignCashRcptAddrId.Import();
    var useExport = new FnAssignCashRcptAddrId.Export();

    Call(FnAssignCashRcptAddrId.Execute, useImport, useExport);

    return useExport.CashReceiptDetailAddress.SystemGeneratedIdentifier;
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
    var systemGeneratedIdentifier = UseFnAssignCashRcptAddrId();
    var street1 = import.CashReceiptDetailAddress.Street1;
    var street2 = import.CashReceiptDetailAddress.Street2 ?? "";
    var city = import.CashReceiptDetailAddress.City;
    var state = import.CashReceiptDetailAddress.State;
    var zipCode5 = import.CashReceiptDetailAddress.ZipCode5;
    var zipCode4 = import.CashReceiptDetailAddress.ZipCode4 ?? "";
    var zipCode3 = import.CashReceiptDetailAddress.ZipCode3 ?? "";

    entities.CashReceiptDetailAddress.Populated = false;
    entities.ReceiptRefund.Populated = false;
    Update("CreateCashReceiptDetailAddress#1",
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

    Update("CreateCashReceiptDetailAddress#3",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "cdaIdentifier", systemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
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
    entities.ReceiptRefund.CdaIdentifier = systemGeneratedIdentifier;
    entities.CashReceiptDetailAddress.Populated = true;
    entities.ReceiptRefund.Populated = true;
  }

  private void CreatePaymentRequest()
  {
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var processDate = import.PaymentRequest.ProcessDate;
    var amount = import.PaymentRequest.Amount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var designatedPayeeCsePersonNo =
      import.PaymentRequest.DesignatedPayeeCsePersonNo ?? "";
    var csePersonNumber = import.PaymentRequest.CsePersonNumber ?? "";
    var imprestFundCode = import.PaymentRequest.ImprestFundCode ?? "";
    var classification = import.PaymentRequest.Classification;
    var type1 = import.PaymentRequest.Type1;
    var rctRTstamp = entities.ReceiptRefund.CreatedTimestamp;

    CheckValid<PaymentRequest>("Type1", type1);
    entities.PaymentRequest.Populated = false;
    Update("CreatePaymentRequest",
      (db, command) =>
      {
        db.SetInt32(command, "paymentRequestId", systemGeneratedIdentifier);
        db.SetDate(command, "processDate", processDate);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.
          SetNullableString(command, "dpCsePerNum", designatedPayeeCsePersonNo);
          
        db.SetNullableString(command, "csePersonNumber", csePersonNumber);
        db.SetNullableString(command, "imprestFundCode", imprestFundCode);
        db.SetString(command, "classification", classification);
        db.SetString(command, "recoveryFiller", "");
        db.SetNullableString(command, "achFormatCode", "");
        db.SetNullableString(command, "number", "");
        db.SetNullableDate(command, "printDate", default(DateTime));
        db.SetString(command, "type", type1);
        db.SetNullableDateTime(command, "rctRTstamp", rctRTstamp);
      });

    entities.PaymentRequest.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentRequest.ProcessDate = processDate;
    entities.PaymentRequest.Amount = amount;
    entities.PaymentRequest.CreatedBy = createdBy;
    entities.PaymentRequest.CreatedTimestamp = createdTimestamp;
    entities.PaymentRequest.DesignatedPayeeCsePersonNo =
      designatedPayeeCsePersonNo;
    entities.PaymentRequest.CsePersonNumber = csePersonNumber;
    entities.PaymentRequest.ImprestFundCode = imprestFundCode;
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
    var systemGeneratedIdentifier = 1;
    var effectiveDate = Now().Date;
    var discontinueDate = new DateTime(2099, 12, 31);
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = "Offset Advancement creation";

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

  private void CreateReceiptRefund()
  {
    var createdTimestamp = Now();
    var reasonCode = import.ReceiptRefund.ReasonCode;
    var taxid = import.ReceiptRefund.Taxid ?? "";
    var payeeName = import.ReceiptRefund.PayeeName ?? "";
    var amount = import.ReceiptRefund.Amount;
    var offsetTaxYear = import.ReceiptRefund.OffsetTaxYear.GetValueOrDefault();
    var requestDate = import.ReceiptRefund.RequestDate;
    var createdBy = global.UserId;
    var cspNumber = entities.CsePerson.Number;
    var cstAIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    var reasonText = import.ReceiptRefund.ReasonText ?? "";
    var cltIdentifier = entities.CollectionType.SequentialIdentifier;

    entities.ReceiptRefund.Populated = false;
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
        db.SetString(command, "offsetClosed", "");
        db.SetNullableDate(command, "dateTransmitted", default(DateTime));
        db.SetNullableString(command, "taxIdSuffix", "");
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableInt32(command, "cltIdentifier", cltIdentifier);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
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
    entities.ReceiptRefund.CstAIdentifier = cstAIdentifier;
    entities.ReceiptRefund.CdaIdentifier = null;
    entities.ReceiptRefund.ReasonText = reasonText;
    entities.ReceiptRefund.CltIdentifier = cltIdentifier;
    entities.ReceiptRefund.Populated = true;
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType.Code);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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

    private CollectionType collectionType;
    private PaymentRequest paymentRequest;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CsePerson csePerson;
    private CashReceiptSourceType cashReceiptSourceType;
    private FundTransaction fundTransaction;
    private FundTransactionType fundTransactionType;
    private Fund fund;
    private ProgramCostAccount programCostAccount;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
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
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    private CollectionType collectionType;
    private PaymentStatus paymentStatus;
    private PaymentRequest paymentRequest;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CsePerson csePerson;
    private ReceiptRefund receiptRefund;
    private CashReceiptSourceType cashReceiptSourceType;
    private FundTransaction fundTransaction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    /// <summary>
    /// A value of ClearingFundFund.
    /// </summary>
    [JsonPropertyName("clearingFundFund")]
    public Fund ClearingFundFund
    {
      get => clearingFundFund ??= new();
      set => clearingFundFund = value;
    }

    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public FundTransactionStatus Active
    {
      get => active ??= new();
      set => active = value;
    }

    /// <summary>
    /// A value of ClearingFundProgramCostAccount.
    /// </summary>
    [JsonPropertyName("clearingFundProgramCostAccount")]
    public ProgramCostAccount ClearingFundProgramCostAccount
    {
      get => clearingFundProgramCostAccount ??= new();
      set => clearingFundProgramCostAccount = value;
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

    private FundTransaction fundTransaction;
    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private Fund clearingFundFund;
    private FundTransactionStatus active;
    private ProgramCostAccount clearingFundProgramCostAccount;
    private FundTransactionType refund;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
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
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
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

    private CollectionType collectionType;
    private PaymentStatus paymentStatus;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentRequest paymentRequest;
    private FundTransaction fundTransaction;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CsePerson csePerson;
    private ReceiptRefund receiptRefund;
    private CashReceiptSourceType cashReceiptSourceType;
    private FundTransactionType fundTransactionType;
    private PcaFundExplosionRule pcaFundExplosionRule;
    private Fund fund;
    private ProgramCostAccount programCostAccount;
  }
#endregion
}
