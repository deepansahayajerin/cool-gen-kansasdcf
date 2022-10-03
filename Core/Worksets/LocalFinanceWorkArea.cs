// The source file: LOCAL_FINANCE_WORK_AREA, ID: 372524080, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class LocalFinanceWorkArea: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LocalFinanceWorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LocalFinanceWorkArea(LocalFinanceWorkArea that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LocalFinanceWorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the COMPILATION_DET_FOUND_FLAG attribute.</summary>
  public const int CompilationDetFoundFlag_MaxLength = 1;

  /// <summary>
  /// The value of the COMPILATION_DET_FOUND_FLAG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = CompilationDetFoundFlag_MaxLength)]
  public string CompilationDetFoundFlag
  {
    get => Get<string>("compilationDetFoundFlag") ?? "";
    set => Set(
      "compilationDetFoundFlag", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CompilationDetFoundFlag_MaxLength)));
  }

  /// <summary>
  /// The json value of the CompilationDetFoundFlag attribute.</summary>
  [JsonPropertyName("compilationDetFoundFlag")]
  [Computed]
  public string CompilationDetFoundFlag_Json
  {
    get => NullIf(CompilationDetFoundFlag, "");
    set => CompilationDetFoundFlag = value;
  }

  /// <summary>Length of the REMAIL_ADDR_FOUND_FLAG attribute.</summary>
  public const int RemailAddrFoundFlag_MaxLength = 1;

  /// <summary>
  /// The value of the REMAIL_ADDR_FOUND_FLAG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = RemailAddrFoundFlag_MaxLength)]
  public string RemailAddrFoundFlag
  {
    get => Get<string>("remailAddrFoundFlag") ?? "";
    set => Set(
      "remailAddrFoundFlag", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, RemailAddrFoundFlag_MaxLength)));
  }

  /// <summary>
  /// The json value of the RemailAddrFoundFlag attribute.</summary>
  [JsonPropertyName("remailAddrFoundFlag")]
  [Computed]
  public string RemailAddrFoundFlag_Json
  {
    get => NullIf(RemailAddrFoundFlag, "");
    set => RemailAddrFoundFlag = value;
  }

  /// <summary>Length of the PAYMENT_STATUS_HIST_FOUND_FLAG attribute.</summary>
  public const int PaymentStatusHistFoundFlag_MaxLength = 1;

  /// <summary>
  /// The value of the PAYMENT_STATUS_HIST_FOUND_FLAG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = PaymentStatusHistFoundFlag_MaxLength)]
  public string PaymentStatusHistFoundFlag
  {
    get => Get<string>("paymentStatusHistFoundFlag") ?? "";
    set => Set(
      "paymentStatusHistFoundFlag", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, PaymentStatusHistFoundFlag_MaxLength)));
  }

  /// <summary>
  /// The json value of the PaymentStatusHistFoundFlag attribute.</summary>
  [JsonPropertyName("paymentStatusHistFoundFlag")]
  [Computed]
  public string PaymentStatusHistFoundFlag_Json
  {
    get => NullIf(PaymentStatusHistFoundFlag, "");
    set => PaymentStatusHistFoundFlag = value;
  }

  /// <summary>Length of the IMPRESSED_FUND_CODE attribute.</summary>
  public const int ImpressedFundCode_MaxLength = 10;

  /// <summary>
  /// The value of the IMPRESSED_FUND_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = ImpressedFundCode_MaxLength)]
  public string ImpressedFundCode
  {
    get => Get<string>("impressedFundCode") ?? "";
    set => Set(
      "impressedFundCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ImpressedFundCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the ImpressedFundCode attribute.</summary>
  [JsonPropertyName("impressedFundCode")]
  [Computed]
  public string ImpressedFundCode_Json
  {
    get => NullIf(ImpressedFundCode, "");
    set => ImpressedFundCode = value;
  }

  /// <summary>Length of the CLASSIFICATION_TYPE attribute.</summary>
  public const int ClassificationType_MaxLength = 10;

  /// <summary>
  /// The value of the CLASSIFICATION_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = ClassificationType_MaxLength)]
  public string ClassificationType
  {
    get => Get<string>("classificationType") ?? "";
    set => Set(
      "classificationType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ClassificationType_MaxLength)));
  }

  /// <summary>
  /// The json value of the ClassificationType attribute.</summary>
  [JsonPropertyName("classificationType")]
  [Computed]
  public string ClassificationType_Json
  {
    get => NullIf(ClassificationType, "");
    set => ClassificationType = value;
  }

  /// <summary>
  /// The value of the DISB_DATE attribute.
  /// </summary>
  [JsonPropertyName("disbDate")]
  [Member(Index = 6, Type = MemberType.Date)]
  public DateTime? DisbDate
  {
    get => Get<DateTime?>("disbDate");
    set => Set("disbDate", value);
  }

  /// <summary>Length of the DISB_CODE attribute.</summary>
  public const int DisbCode_MaxLength = 8;

  /// <summary>
  /// The value of the DISB_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = DisbCode_MaxLength)]
  public string DisbCode
  {
    get => Get<string>("disbCode") ?? "";
    set => Set(
      "disbCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, DisbCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the DisbCode attribute.</summary>
  [JsonPropertyName("disbCode")]
  [Computed]
  public string DisbCode_Json
  {
    get => NullIf(DisbCode, "");
    set => DisbCode = value;
  }

  /// <summary>
  /// The value of the DISB_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("disbAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal DisbAmount
  {
    get => Get<decimal?>("disbAmount") ?? 0M;
    set =>
      Set("disbAmount", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>Length of the CASH_RCPT_REF_NUMBER attribute.</summary>
  public const int CashRcptRefNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASH_RCPT_REF_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = CashRcptRefNumber_MaxLength)]
  public string CashRcptRefNumber
  {
    get => Get<string>("cashRcptRefNumber") ?? "";
    set => Set(
      "cashRcptRefNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CashRcptRefNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the CashRcptRefNumber attribute.</summary>
  [JsonPropertyName("cashRcptRefNumber")]
  [Computed]
  public string CashRcptRefNumber_Json
  {
    get => NullIf(CashRcptRefNumber, "");
    set => CashRcptRefNumber = value;
  }

  /// <summary>
  /// The value of the SL_NO attribute.
  /// </summary>
  [JsonPropertyName("slNo")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 2)]
  public int SlNo
  {
    get => Get<int?>("slNo") ?? 0;
    set => Set("slNo", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the PROGRAM_ERR_MSG attribute.</summary>
  public const int ProgramErrMsg_MaxLength = 50;

  /// <summary>
  /// The value of the PROGRAM_ERR_MSG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = ProgramErrMsg_MaxLength)]
  public string ProgramErrMsg
  {
    get => Get<string>("programErrMsg") ?? "";
    set => Set(
      "programErrMsg", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ProgramErrMsg_MaxLength)));
  }

  /// <summary>
  /// The json value of the ProgramErrMsg attribute.</summary>
  [JsonPropertyName("programErrMsg")]
  [Computed]
  public string ProgramErrMsg_Json
  {
    get => NullIf(ProgramErrMsg, "");
    set => ProgramErrMsg = value;
  }

  /// <summary>Length of the REISSUED_FROM_PAYMENT_REQ_NUMBER attribute.
  /// </summary>
  public const int ReissuedFromPaymentReqNumber_MaxLength = 9;

  /// <summary>
  /// The value of the REISSUED_FROM_PAYMENT_REQ_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = ReissuedFromPaymentReqNumber_MaxLength)]
  public string ReissuedFromPaymentReqNumber
  {
    get => Get<string>("reissuedFromPaymentReqNumber") ?? "";
    set => Set(
      "reissuedFromPaymentReqNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ReissuedFromPaymentReqNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the ReissuedFromPaymentReqNumber attribute.</summary>
  [JsonPropertyName("reissuedFromPaymentReqNumber")]
  [Computed]
  public string ReissuedFromPaymentReqNumber_Json
  {
    get => NullIf(ReissuedFromPaymentReqNumber, "");
    set => ReissuedFromPaymentReqNumber = value;
  }

  /// <summary>Length of the REISSUED_TO_PAYMENT_REQ_NUMBER attribute.</summary>
  public const int ReissuedToPaymentReqNumber_MaxLength = 9;

  /// <summary>
  /// The value of the REISSUED_TO_PAYMENT_REQ_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = ReissuedToPaymentReqNumber_MaxLength)]
  public string ReissuedToPaymentReqNumber
  {
    get => Get<string>("reissuedToPaymentReqNumber") ?? "";
    set => Set(
      "reissuedToPaymentReqNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ReissuedToPaymentReqNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the ReissuedToPaymentReqNumber attribute.</summary>
  [JsonPropertyName("reissuedToPaymentReqNumber")]
  [Computed]
  public string ReissuedToPaymentReqNumber_Json
  {
    get => NullIf(ReissuedToPaymentReqNumber, "");
    set => ReissuedToPaymentReqNumber = value;
  }

  /// <summary>Length of the WARRANT_NUMBER_PROMPT attribute.</summary>
  public const int WarrantNumberPrompt_MaxLength = 1;

  /// <summary>
  /// The value of the WARRANT_NUMBER_PROMPT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = WarrantNumberPrompt_MaxLength)]
  public string WarrantNumberPrompt
  {
    get => Get<string>("warrantNumberPrompt") ?? "";
    set => Set(
      "warrantNumberPrompt", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, WarrantNumberPrompt_MaxLength)));
  }

  /// <summary>
  /// The json value of the WarrantNumberPrompt attribute.</summary>
  [JsonPropertyName("warrantNumberPrompt")]
  [Computed]
  public string WarrantNumberPrompt_Json
  {
    get => NullIf(WarrantNumberPrompt, "");
    set => WarrantNumberPrompt = value;
  }
}
