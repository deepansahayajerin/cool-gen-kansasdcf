// The source file: INTERSTATE_SUPPORT_ORDER, ID: 371436572, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Information about a support order that was received or sent on an interstate
/// case and transmitted through CSENet.
/// </summary>
[Serializable]
public partial class InterstateSupportOrder: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateSupportOrder()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateSupportOrder(InterstateSupportOrder that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateSupportOrder Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstateSupportOrder that)
  {
    base.Assign(that);
    systemGeneratedSequenceNum = that.systemGeneratedSequenceNum;
    fipsState = that.fipsState;
    fipsCounty = that.fipsCounty;
    fipsLocation = that.fipsLocation;
    number = that.number;
    orderFilingDate = that.orderFilingDate;
    type1 = that.type1;
    debtType = that.debtType;
    paymentFreq = that.paymentFreq;
    amountOrdered = that.amountOrdered;
    effectiveDate = that.effectiveDate;
    endDate = that.endDate;
    cancelDate = that.cancelDate;
    arrearsFreq = that.arrearsFreq;
    arrearsFreqAmount = that.arrearsFreqAmount;
    arrearsTotalAmount = that.arrearsTotalAmount;
    arrearsAfdcFromDate = that.arrearsAfdcFromDate;
    arrearsAfdcThruDate = that.arrearsAfdcThruDate;
    arrearsAfdcAmount = that.arrearsAfdcAmount;
    arrearsNonAfdcFromDate = that.arrearsNonAfdcFromDate;
    arrearsNonAfdcThruDate = that.arrearsNonAfdcThruDate;
    arrearsNonAfdcAmount = that.arrearsNonAfdcAmount;
    fosterCareFromDate = that.fosterCareFromDate;
    fosterCareThruDate = that.fosterCareThruDate;
    fosterCareAmount = that.fosterCareAmount;
    medicalFromDate = that.medicalFromDate;
    medicalThruDate = that.medicalThruDate;
    medicalAmount = that.medicalAmount;
    medicalOrderedInd = that.medicalOrderedInd;
    tribunalCaseNumber = that.tribunalCaseNumber;
    dateOfLastPayment = that.dateOfLastPayment;
    controllingOrderFlag = that.controllingOrderFlag;
    newOrderFlag = that.newOrderFlag;
    docketNumber = that.docketNumber;
    legalActionId = that.legalActionId;
    ccaTransactionDt = that.ccaTransactionDt;
    ccaTranSerNum = that.ccaTranSerNum;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_SEQUENCE_NUM attribute.
  /// </summary>
  [JsonPropertyName("systemGeneratedSequenceNum")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 1)]
  public int SystemGeneratedSequenceNum
  {
    get => systemGeneratedSequenceNum;
    set => systemGeneratedSequenceNum = value;
  }

  /// <summary>Length of the FIPS_STATE attribute.</summary>
  public const int FipsState_MaxLength = 2;

  /// <summary>
  /// The value of the FIPS_STATE attribute.
  /// FIPS state where Order established
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = FipsState_MaxLength)]
  public string FipsState
  {
    get => fipsState ?? "";
    set => fipsState = TrimEnd(Substring(value, 1, FipsState_MaxLength));
  }

  /// <summary>
  /// The json value of the FipsState attribute.</summary>
  [JsonPropertyName("fipsState")]
  [Computed]
  public string FipsState_Json
  {
    get => NullIf(FipsState, "");
    set => FipsState = value;
  }

  /// <summary>Length of the FIPS_COUNTY attribute.</summary>
  public const int FipsCounty_MaxLength = 3;

  /// <summary>
  /// The value of the FIPS_COUNTY attribute.
  /// FIPS county where order established
  /// </summary>
  [JsonPropertyName("fipsCounty")]
  [Member(Index = 3, Type = MemberType.Char, Length = FipsCounty_MaxLength, Optional
    = true)]
  public string FipsCounty
  {
    get => fipsCounty;
    set => fipsCounty = value != null
      ? TrimEnd(Substring(value, 1, FipsCounty_MaxLength)) : null;
  }

  /// <summary>Length of the FIPS_LOCATION attribute.</summary>
  public const int FipsLocation_MaxLength = 2;

  /// <summary>
  /// The value of the FIPS_LOCATION attribute.
  /// Office FIPS where Order established
  /// </summary>
  [JsonPropertyName("fipsLocation")]
  [Member(Index = 4, Type = MemberType.Char, Length = FipsLocation_MaxLength, Optional
    = true)]
  public string FipsLocation
  {
    get => fipsLocation;
    set => fipsLocation = value != null
      ? TrimEnd(Substring(value, 1, FipsLocation_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int Number_MaxLength = 17;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// Local identification assigned to this Order
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = Number_MaxLength)]
  public string Number
  {
    get => number ?? "";
    set => number = TrimEnd(Substring(value, 1, Number_MaxLength));
  }

  /// <summary>
  /// The json value of the Number attribute.</summary>
  [JsonPropertyName("number")]
  [Computed]
  public string Number_Json
  {
    get => NullIf(Number, "");
    set => Number = value;
  }

  /// <summary>
  /// The value of the ORDER_FILING_DATE attribute.
  /// Date Order established
  /// </summary>
  [JsonPropertyName("orderFilingDate")]
  [Member(Index = 6, Type = MemberType.Date)]
  public DateTime? OrderFilingDate
  {
    get => orderFilingDate;
    set => orderFilingDate = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Order type:
  /// A - Administrative
  /// J - Judicial
  /// P - Paternity
  /// </summary>
  [JsonPropertyName("type1")]
  [Member(Index = 7, Type = MemberType.Char, Length = Type1_MaxLength, Optional
    = true)]
  public string Type1
  {
    get => type1;
    set => type1 = value != null
      ? TrimEnd(Substring(value, 1, Type1_MaxLength)) : null;
  }

  /// <summary>Length of the DEBT_TYPE attribute.</summary>
  public const int DebtType_MaxLength = 7;

  /// <summary>
  /// The value of the DEBT_TYPE attribute.
  /// Type of debt:
  /// CS - Child support
  /// SS - Spousal support
  /// MS - Medical support
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = DebtType_MaxLength)]
  public string DebtType
  {
    get => debtType ?? "";
    set => debtType = TrimEnd(Substring(value, 1, DebtType_MaxLength));
  }

  /// <summary>
  /// The json value of the DebtType attribute.</summary>
  [JsonPropertyName("debtType")]
  [Computed]
  public string DebtType_Json
  {
    get => NullIf(DebtType, "");
    set => DebtType = value;
  }

  /// <summary>Length of the PAYMENT_FREQ attribute.</summary>
  public const int PaymentFreq_MaxLength = 2;

  /// <summary>
  /// The value of the PAYMENT_FREQ attribute.
  /// Frequency of payments:
  /// W - Weekly
  /// B - Biweekly
  /// S - Semi-monthly
  /// M - Monthly
  /// </summary>
  [JsonPropertyName("paymentFreq")]
  [Member(Index = 9, Type = MemberType.Char, Length = PaymentFreq_MaxLength, Optional
    = true)]
  public string PaymentFreq
  {
    get => paymentFreq;
    set => paymentFreq = value != null
      ? TrimEnd(Substring(value, 1, PaymentFreq_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the AMOUNT_ORDERED attribute.
  /// Amount of each payment
  /// </summary>
  [JsonPropertyName("amountOrdered")]
  [Member(Index = 10, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? AmountOrdered
  {
    get => amountOrdered;
    set => amountOrdered = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// Date obigations start to accrue
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 11, Type = MemberType.Date, Optional = true)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// Order this Order ends
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>
  /// The value of the CANCEL_DATE attribute.
  /// Date Order cancelled
  /// </summary>
  [JsonPropertyName("cancelDate")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? CancelDate
  {
    get => cancelDate;
    set => cancelDate = value;
  }

  /// <summary>Length of the ARREARS_FREQ attribute.</summary>
  public const int ArrearsFreq_MaxLength = 1;

  /// <summary>
  /// The value of the ARREARS_FREQ attribute.
  /// Frequency at which arrears payments due:
  /// W - Weekly
  /// B - Biweekly
  /// S - Semi-monthly
  /// M - Monthly
  /// </summary>
  [JsonPropertyName("arrearsFreq")]
  [Member(Index = 14, Type = MemberType.Char, Length = ArrearsFreq_MaxLength, Optional
    = true)]
  public string ArrearsFreq
  {
    get => arrearsFreq;
    set => arrearsFreq = value != null
      ? TrimEnd(Substring(value, 1, ArrearsFreq_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ARREARS_FREQ_AMOUNT attribute.
  /// Amount of arrearage payment
  /// </summary>
  [JsonPropertyName("arrearsFreqAmount")]
  [Member(Index = 15, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? ArrearsFreqAmount
  {
    get => arrearsFreqAmount;
    set => arrearsFreqAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the ARREARS_TOTAL_AMOUNT attribute.
  /// Total amount of Order arrears
  /// </summary>
  [JsonPropertyName("arrearsTotalAmount")]
  [Member(Index = 16, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? ArrearsTotalAmount
  {
    get => arrearsTotalAmount;
    set => arrearsTotalAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the ARREARS_AFDC_FROM_DATE attribute.
  /// Start date for AFDC arrearage
  /// </summary>
  [JsonPropertyName("arrearsAfdcFromDate")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
  public DateTime? ArrearsAfdcFromDate
  {
    get => arrearsAfdcFromDate;
    set => arrearsAfdcFromDate = value;
  }

  /// <summary>
  /// The value of the ARREARS_AFDC_THRU_DATE attribute.
  /// Start date of AFDC arrearage
  /// </summary>
  [JsonPropertyName("arrearsAfdcThruDate")]
  [Member(Index = 18, Type = MemberType.Date, Optional = true)]
  public DateTime? ArrearsAfdcThruDate
  {
    get => arrearsAfdcThruDate;
    set => arrearsAfdcThruDate = value;
  }

  /// <summary>
  /// The value of the ARREARS_AFDC_AMOUNT attribute.
  /// End date of AFDC arrearage
  /// </summary>
  [JsonPropertyName("arrearsAfdcAmount")]
  [Member(Index = 19, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? ArrearsAfdcAmount
  {
    get => arrearsAfdcAmount;
    set => arrearsAfdcAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the ARREARS_NON_AFDC_FROM_DATE attribute.
  /// Start date of NAFDC arrearage
  /// </summary>
  [JsonPropertyName("arrearsNonAfdcFromDate")]
  [Member(Index = 20, Type = MemberType.Date, Optional = true)]
  public DateTime? ArrearsNonAfdcFromDate
  {
    get => arrearsNonAfdcFromDate;
    set => arrearsNonAfdcFromDate = value;
  }

  /// <summary>
  /// The value of the ARREARS_NON_AFDC_THRU_DATE attribute.
  /// End date for NAFDC arrearage
  /// </summary>
  [JsonPropertyName("arrearsNonAfdcThruDate")]
  [Member(Index = 21, Type = MemberType.Date, Optional = true)]
  public DateTime? ArrearsNonAfdcThruDate
  {
    get => arrearsNonAfdcThruDate;
    set => arrearsNonAfdcThruDate = value;
  }

  /// <summary>
  /// The value of the ARREARS_NON_AFDC_AMOUNT attribute.
  /// Total amount of NAFDC arrearage
  /// </summary>
  [JsonPropertyName("arrearsNonAfdcAmount")]
  [Member(Index = 22, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? ArrearsNonAfdcAmount
  {
    get => arrearsNonAfdcAmount;
    set => arrearsNonAfdcAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the FOSTER_CARE_FROM_DATE attribute.
  /// Start date for Foster Care arrearage
  /// </summary>
  [JsonPropertyName("fosterCareFromDate")]
  [Member(Index = 23, Type = MemberType.Date, Optional = true)]
  public DateTime? FosterCareFromDate
  {
    get => fosterCareFromDate;
    set => fosterCareFromDate = value;
  }

  /// <summary>
  /// The value of the FOSTER_CARE_THRU_DATE attribute.
  /// Date Foster Care ended
  /// </summary>
  [JsonPropertyName("fosterCareThruDate")]
  [Member(Index = 24, Type = MemberType.Date, Optional = true)]
  public DateTime? FosterCareThruDate
  {
    get => fosterCareThruDate;
    set => fosterCareThruDate = value;
  }

  /// <summary>
  /// The value of the FOSTER_CARE_AMOUNT attribute.
  /// Amount of FC arrearage
  /// </summary>
  [JsonPropertyName("fosterCareAmount")]
  [Member(Index = 25, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? FosterCareAmount
  {
    get => fosterCareAmount;
    set => fosterCareAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the MEDICAL_FROM_DATE attribute.
  /// Start date of medical arrearage
  /// </summary>
  [JsonPropertyName("medicalFromDate")]
  [Member(Index = 26, Type = MemberType.Date, Optional = true)]
  public DateTime? MedicalFromDate
  {
    get => medicalFromDate;
    set => medicalFromDate = value;
  }

  /// <summary>
  /// The value of the MEDICAL_THRU_DATE attribute.
  /// End date for medical arrearage
  /// </summary>
  [JsonPropertyName("medicalThruDate")]
  [Member(Index = 27, Type = MemberType.Date, Optional = true)]
  public DateTime? MedicalThruDate
  {
    get => medicalThruDate;
    set => medicalThruDate = value;
  }

  /// <summary>
  /// The value of the MEDICAL_AMOUNT attribute.
  /// Total amount of medical arrearage
  /// </summary>
  [JsonPropertyName("medicalAmount")]
  [Member(Index = 28, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? MedicalAmount
  {
    get => medicalAmount;
    set => medicalAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the MEDICAL_ORDERED_IND attribute.</summary>
  public const int MedicalOrderedInd_MaxLength = 1;

  /// <summary>
  /// The value of the MEDICAL_ORDERED_IND attribute.
  /// Indicates whether Medical Coverage has been ordered.  Options &quot;Y
  /// &quot; for yes or &quot;N&quot; for no.
  /// </summary>
  [JsonPropertyName("medicalOrderedInd")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = MedicalOrderedInd_MaxLength, Optional = true)]
  public string MedicalOrderedInd
  {
    get => medicalOrderedInd;
    set => medicalOrderedInd = value != null
      ? TrimEnd(Substring(value, 1, MedicalOrderedInd_MaxLength)) : null;
  }

  /// <summary>Length of the TRIBUNAL_CASE_NUMBER attribute.</summary>
  public const int TribunalCaseNumber_MaxLength = 17;

  /// <summary>
  /// The value of the TRIBUNAL_CASE_NUMBER attribute.
  /// Identifying number assigned by the tribunal.
  /// </summary>
  [JsonPropertyName("tribunalCaseNumber")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = TribunalCaseNumber_MaxLength, Optional = true)]
  public string TribunalCaseNumber
  {
    get => tribunalCaseNumber;
    set => tribunalCaseNumber = value != null
      ? TrimEnd(Substring(value, 1, TribunalCaseNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DATE_OF_LAST_PAYMENT attribute.
  /// This is the last date on which a payment was received from the AP.
  /// </summary>
  [JsonPropertyName("dateOfLastPayment")]
  [Member(Index = 31, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfLastPayment
  {
    get => dateOfLastPayment;
    set => dateOfLastPayment = value;
  }

  /// <summary>Length of the CONTROLLING_ORDER_FLAG attribute.</summary>
  public const int ControllingOrderFlag_MaxLength = 1;

  /// <summary>
  /// The value of the CONTROLLING_ORDER_FLAG attribute.
  /// An indication of whether or not this is the controlling order.
  /// </summary>
  [JsonPropertyName("controllingOrderFlag")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = ControllingOrderFlag_MaxLength, Optional = true)]
  public string ControllingOrderFlag
  {
    get => controllingOrderFlag;
    set => controllingOrderFlag = value != null
      ? TrimEnd(Substring(value, 1, ControllingOrderFlag_MaxLength)) : null;
  }

  /// <summary>Length of the NEW_ORDER_FLAG attribute.</summary>
  public const int NewOrderFlag_MaxLength = 1;

  /// <summary>
  /// The value of the NEW_ORDER_FLAG attribute.
  /// An indication of whether or not this is a new and controlling order.
  /// </summary>
  [JsonPropertyName("newOrderFlag")]
  [Member(Index = 33, Type = MemberType.Char, Length = NewOrderFlag_MaxLength, Optional
    = true)]
  public string NewOrderFlag
  {
    get => newOrderFlag;
    set => newOrderFlag = value != null
      ? TrimEnd(Substring(value, 1, NewOrderFlag_MaxLength)) : null;
  }

  /// <summary>Length of the DOCKET_NUMBER attribute.</summary>
  public const int DocketNumber_MaxLength = 17;

  /// <summary>
  /// The value of the DOCKET_NUMBER attribute.
  /// Identifying number assigned by the Court.
  /// </summary>
  [JsonPropertyName("docketNumber")]
  [Member(Index = 34, Type = MemberType.Char, Length = DocketNumber_MaxLength, Optional
    = true)]
  public string DocketNumber
  {
    get => docketNumber;
    set => docketNumber = value != null
      ? TrimEnd(Substring(value, 1, DocketNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LEGAL_ACTION_ID attribute.
  /// Uniquely identifies a Legal Action
  /// </summary>
  [JsonPropertyName("legalActionId")]
  [Member(Index = 35, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LegalActionId
  {
    get => legalActionId;
    set => legalActionId = value;
  }

  /// <summary>
  /// The value of the TRANSACTION_DATE attribute.
  /// This is the date on which CSENet transmitted the Referral.
  /// </summary>
  [JsonPropertyName("ccaTransactionDt")]
  [Member(Index = 36, Type = MemberType.Date)]
  public DateTime? CcaTransactionDt
  {
    get => ccaTransactionDt;
    set => ccaTransactionDt = value;
  }

  /// <summary>
  /// The value of the TRANS_SERIAL_NUMBER attribute.
  /// This is a unique number assigned to each CSENet transaction.  It has no 
  /// place in the KESSEP system but is required to provide a key used to
  /// process CSENet Referrals.
  /// </summary>
  [JsonPropertyName("ccaTranSerNum")]
  [DefaultValue(0L)]
  [Member(Index = 37, Type = MemberType.Number, Length = 12)]
  public long CcaTranSerNum
  {
    get => ccaTranSerNum;
    set => ccaTranSerNum = value;
  }

  private int systemGeneratedSequenceNum;
  private string fipsState;
  private string fipsCounty;
  private string fipsLocation;
  private string number;
  private DateTime? orderFilingDate;
  private string type1;
  private string debtType;
  private string paymentFreq;
  private decimal? amountOrdered;
  private DateTime? effectiveDate;
  private DateTime? endDate;
  private DateTime? cancelDate;
  private string arrearsFreq;
  private decimal? arrearsFreqAmount;
  private decimal? arrearsTotalAmount;
  private DateTime? arrearsAfdcFromDate;
  private DateTime? arrearsAfdcThruDate;
  private decimal? arrearsAfdcAmount;
  private DateTime? arrearsNonAfdcFromDate;
  private DateTime? arrearsNonAfdcThruDate;
  private decimal? arrearsNonAfdcAmount;
  private DateTime? fosterCareFromDate;
  private DateTime? fosterCareThruDate;
  private decimal? fosterCareAmount;
  private DateTime? medicalFromDate;
  private DateTime? medicalThruDate;
  private decimal? medicalAmount;
  private string medicalOrderedInd;
  private string tribunalCaseNumber;
  private DateTime? dateOfLastPayment;
  private string controllingOrderFlag;
  private string newOrderFlag;
  private string docketNumber;
  private int? legalActionId;
  private DateTime? ccaTransactionDt;
  private long ccaTranSerNum;
}
