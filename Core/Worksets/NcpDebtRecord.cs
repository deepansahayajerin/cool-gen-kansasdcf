// The source file: NCP_DEBT_RECORD, ID: 374396749, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class NcpDebtRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NcpDebtRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NcpDebtRecord(NcpDebtRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NcpDebtRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NcpDebtRecord that)
  {
    base.Assign(that);
    recordType = that.recordType;
    courtDebtId = that.courtDebtId;
    debtType = that.debtType;
    feeClass = that.feeClass;
    overrideFeePercent = that.overrideFeePercent;
    debtFeeExemption = that.debtFeeExemption;
    intersateId = that.intersateId;
    kessepMultiplePayerIndicator = that.kessepMultiplePayerIndicator;
    countyMultiplePayorIndicator = that.countyMultiplePayorIndicator;
    kpcDebtId = that.kpcDebtId;
    filler = that.filler;
  }

  /// <summary>Length of the RECORD_TYPE attribute.</summary>
  public const int RecordType_MaxLength = 1;

  /// <summary>
  /// The value of the RECORD_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordType_MaxLength)]
  public string RecordType
  {
    get => recordType ?? "";
    set => recordType = TrimEnd(Substring(value, 1, RecordType_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordType attribute.</summary>
  [JsonPropertyName("recordType")]
  [Computed]
  public string RecordType_Json
  {
    get => NullIf(RecordType, "");
    set => RecordType = value;
  }

  /// <summary>Length of the COURT_DEBT_ID attribute.</summary>
  public const int CourtDebtId_MaxLength = 20;

  /// <summary>
  /// The value of the COURT_DEBT_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CourtDebtId_MaxLength)]
  public string CourtDebtId
  {
    get => courtDebtId ?? "";
    set => courtDebtId = TrimEnd(Substring(value, 1, CourtDebtId_MaxLength));
  }

  /// <summary>
  /// The json value of the CourtDebtId attribute.</summary>
  [JsonPropertyName("courtDebtId")]
  [Computed]
  public string CourtDebtId_Json
  {
    get => NullIf(CourtDebtId, "");
    set => CourtDebtId = value;
  }

  /// <summary>Length of the DEBT_TYPE attribute.</summary>
  public const int DebtType_MaxLength = 4;

  /// <summary>
  /// The value of the DEBT_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = DebtType_MaxLength)]
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

  /// <summary>Length of the FEE_CLASS attribute.</summary>
  public const int FeeClass_MaxLength = 1;

  /// <summary>
  /// The value of the FEE_CLASS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = FeeClass_MaxLength)]
  public string FeeClass
  {
    get => feeClass ?? "";
    set => feeClass = TrimEnd(Substring(value, 1, FeeClass_MaxLength));
  }

  /// <summary>
  /// The json value of the FeeClass attribute.</summary>
  [JsonPropertyName("feeClass")]
  [Computed]
  public string FeeClass_Json
  {
    get => NullIf(FeeClass, "");
    set => FeeClass = value;
  }

  /// <summary>Length of the OVERRIDE_FEE_PERCENT attribute.</summary>
  public const int OverrideFeePercent_MaxLength = 5;

  /// <summary>
  /// The value of the OVERRIDE_FEE_PERCENT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = OverrideFeePercent_MaxLength)]
  public string OverrideFeePercent
  {
    get => overrideFeePercent ?? "";
    set => overrideFeePercent =
      TrimEnd(Substring(value, 1, OverrideFeePercent_MaxLength));
  }

  /// <summary>
  /// The json value of the OverrideFeePercent attribute.</summary>
  [JsonPropertyName("overrideFeePercent")]
  [Computed]
  public string OverrideFeePercent_Json
  {
    get => NullIf(OverrideFeePercent, "");
    set => OverrideFeePercent = value;
  }

  /// <summary>Length of the DEBT_FEE_EXEMPTION attribute.</summary>
  public const int DebtFeeExemption_MaxLength = 1;

  /// <summary>
  /// The value of the DEBT_FEE_EXEMPTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = DebtFeeExemption_MaxLength)
    ]
  public string DebtFeeExemption
  {
    get => debtFeeExemption ?? "";
    set => debtFeeExemption =
      TrimEnd(Substring(value, 1, DebtFeeExemption_MaxLength));
  }

  /// <summary>
  /// The json value of the DebtFeeExemption attribute.</summary>
  [JsonPropertyName("debtFeeExemption")]
  [Computed]
  public string DebtFeeExemption_Json
  {
    get => NullIf(DebtFeeExemption, "");
    set => DebtFeeExemption = value;
  }

  /// <summary>Length of the INTERSATE_ID attribute.</summary>
  public const int IntersateId_MaxLength = 20;

  /// <summary>
  /// The value of the INTERSATE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = IntersateId_MaxLength)]
  public string IntersateId
  {
    get => intersateId ?? "";
    set => intersateId = TrimEnd(Substring(value, 1, IntersateId_MaxLength));
  }

  /// <summary>
  /// The json value of the IntersateId attribute.</summary>
  [JsonPropertyName("intersateId")]
  [Computed]
  public string IntersateId_Json
  {
    get => NullIf(IntersateId, "");
    set => IntersateId = value;
  }

  /// <summary>Length of the KESSEP_MULTIPLE_PAYER_INDICATOR attribute.
  /// </summary>
  public const int KessepMultiplePayerIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the KESSEP_MULTIPLE_PAYER_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = KessepMultiplePayerIndicator_MaxLength)]
  public string KessepMultiplePayerIndicator
  {
    get => kessepMultiplePayerIndicator ?? "";
    set => kessepMultiplePayerIndicator =
      TrimEnd(Substring(value, 1, KessepMultiplePayerIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the KessepMultiplePayerIndicator attribute.</summary>
  [JsonPropertyName("kessepMultiplePayerIndicator")]
  [Computed]
  public string KessepMultiplePayerIndicator_Json
  {
    get => NullIf(KessepMultiplePayerIndicator, "");
    set => KessepMultiplePayerIndicator = value;
  }

  /// <summary>Length of the COUNTY_MULTIPLE_PAYOR_INDICATOR attribute.
  /// </summary>
  public const int CountyMultiplePayorIndicator_MaxLength = 4;

  /// <summary>
  /// The value of the COUNTY_MULTIPLE_PAYOR_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = CountyMultiplePayorIndicator_MaxLength)]
  public string CountyMultiplePayorIndicator
  {
    get => countyMultiplePayorIndicator ?? "";
    set => countyMultiplePayorIndicator =
      TrimEnd(Substring(value, 1, CountyMultiplePayorIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the CountyMultiplePayorIndicator attribute.</summary>
  [JsonPropertyName("countyMultiplePayorIndicator")]
  [Computed]
  public string CountyMultiplePayorIndicator_Json
  {
    get => NullIf(CountyMultiplePayorIndicator, "");
    set => CountyMultiplePayorIndicator = value;
  }

  /// <summary>Length of the KPC_DEBT_ID attribute.</summary>
  public const int KpcDebtId_MaxLength = 8;

  /// <summary>
  /// The value of the KPC_DEBT_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = KpcDebtId_MaxLength)]
  public string KpcDebtId
  {
    get => kpcDebtId ?? "";
    set => kpcDebtId = TrimEnd(Substring(value, 1, KpcDebtId_MaxLength));
  }

  /// <summary>
  /// The json value of the KpcDebtId attribute.</summary>
  [JsonPropertyName("kpcDebtId")]
  [Computed]
  public string KpcDebtId_Json
  {
    get => NullIf(KpcDebtId, "");
    set => KpcDebtId = value;
  }

  /// <summary>Length of the FILLER attribute.</summary>
  public const int Filler_MaxLength = 134;

  /// <summary>
  /// The value of the FILLER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Filler_MaxLength)]
  public string Filler
  {
    get => filler ?? "";
    set => filler = TrimEnd(Substring(value, 1, Filler_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler attribute.</summary>
  [JsonPropertyName("filler")]
  [Computed]
  public string Filler_Json
  {
    get => NullIf(Filler, "");
    set => Filler = value;
  }

  private string recordType;
  private string courtDebtId;
  private string debtType;
  private string feeClass;
  private string overrideFeePercent;
  private string debtFeeExemption;
  private string intersateId;
  private string kessepMultiplePayerIndicator;
  private string countyMultiplePayorIndicator;
  private string kpcDebtId;
  private string filler;
}
