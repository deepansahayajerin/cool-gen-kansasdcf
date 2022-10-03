// The source file: RECAPTURE_RULE, ID: 371439922, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This entity contains the information nececssary to recapture money being 
/// paid to an Obligee when that Obligee owes money to the state.  Recapture
/// agreements are worked out between the state and the Obligee.  Not all
/// disbursements can be automatically recaptured.  The agreement is based on an
/// amount or percent with a possible upper limit.  This agreement is worked
/// out for each obligation and may apply to specific kinds of disbursements.
/// All of the information necessary to determine if a disbursement can be
/// recaptured is stored in this entity and thru its relationship to the
/// Obligation and Disbursement Type.
/// </summary>
[Serializable]
public partial class RecaptureRule: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public RecaptureRule()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public RecaptureRule(RecaptureRule that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new RecaptureRule Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(RecaptureRule that)
  {
    base.Assign(that);
    negotiatedDate = that.negotiatedDate;
    nonAdcArrearsMaxAmount = that.nonAdcArrearsMaxAmount;
    nonAdcArrearsAmount = that.nonAdcArrearsAmount;
    nonAdcArrearsPercentage = that.nonAdcArrearsPercentage;
    nonAdcCurrentMaxAmount = that.nonAdcCurrentMaxAmount;
    nonAdcCurrentAmount = that.nonAdcCurrentAmount;
    nonAdcCurrentPercentage = that.nonAdcCurrentPercentage;
    type1 = that.type1;
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    passthruPercentage = that.passthruPercentage;
    passthruAmount = that.passthruAmount;
    passthruMaxAmount = that.passthruMaxAmount;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    repaymentLetterPrintDate = that.repaymentLetterPrintDate;
    obligorRuleFiller = that.obligorRuleFiller;
    defaultRuleFiller = that.defaultRuleFiller;
    cpaDType = that.cpaDType;
    cspDNumber = that.cspDNumber;
    dtyGeneratedId = that.dtyGeneratedId;
  }

  /// <summary>
  /// The value of the NEGOTIATED_DATE attribute.
  /// The date upon which there was contact with the obligee who is also an 
  /// obligor for a recovery obligation.  This contact is usually but not always
  /// by phone and may be followed up by a written agreement.  The contact/
  /// negotiation may or may not result in a change to the recapturing for that
  /// obligor.  Either way this date would be used to document that contact was
  /// made to discuss recapturing.
  /// </summary>
  [JsonPropertyName("negotiatedDate")]
  [Member(Index = 1, Type = MemberType.Date, Optional = true)]
  public DateTime? NegotiatedDate
  {
    get => negotiatedDate;
    set => negotiatedDate = value;
  }

  /// <summary>
  /// The value of the NON_ADC_ARREARS_MAX_AMOUNT attribute.
  /// The maximum amount of Non-ADC arrears disbursements that we can retain in 
  /// any given month in order to pay off recovery debts.
  /// </summary>
  [JsonPropertyName("nonAdcArrearsMaxAmount")]
  [Member(Index = 2, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? NonAdcArrearsMaxAmount
  {
    get => nonAdcArrearsMaxAmount;
    set => nonAdcArrearsMaxAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the NON_ADC_ARREARS_AMOUNT attribute.
  /// The amount of Non-ADC arrears disbursements that we can retain per 
  /// disbursement in order to pay off recovery debts.
  /// </summary>
  [JsonPropertyName("nonAdcArrearsAmount")]
  [Member(Index = 3, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? NonAdcArrearsAmount
  {
    get => nonAdcArrearsAmount;
    set => nonAdcArrearsAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the NON_ADC_ARREARS_PERCENTAGE attribute.
  /// The percentage of Non-ADC arrears disbursements that we can retain per 
  /// disbursement in order to pay off recovery debts.
  /// </summary>
  [JsonPropertyName("nonAdcArrearsPercentage")]
  [Member(Index = 4, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? NonAdcArrearsPercentage
  {
    get => nonAdcArrearsPercentage;
    set => nonAdcArrearsPercentage = value;
  }

  /// <summary>
  /// The value of the NON_ADC_CURRENT_MAX_AMOUNT attribute.
  /// The maximum amount of Non-ADC current disbursements that we can retain in 
  /// any given month in order to pay off recovery debts.
  /// </summary>
  [JsonPropertyName("nonAdcCurrentMaxAmount")]
  [Member(Index = 5, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? NonAdcCurrentMaxAmount
  {
    get => nonAdcCurrentMaxAmount;
    set => nonAdcCurrentMaxAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the NON_ADC_CURRENT_AMOUNT attribute.
  /// The amount of Non-ADC current disbursements that we can retain per 
  /// disbursement in order to pay off recovery debts.
  /// </summary>
  [JsonPropertyName("nonAdcCurrentAmount")]
  [Member(Index = 6, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? NonAdcCurrentAmount
  {
    get => nonAdcCurrentAmount;
    set => nonAdcCurrentAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the NON_ADC_CURRENT_PERCENTAGE attribute.
  /// The percentage of Non ADC Current disbursements that we can retain per 
  /// disbursement in order to pay off recovery debts.
  /// </summary>
  [JsonPropertyName("nonAdcCurrentPercentage")]
  [Member(Index = 7, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? NonAdcCurrentPercentage
  {
    get => nonAdcCurrentPercentage;
    set => nonAdcCurrentPercentage = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This is an indicator that is used to divide the Recapture Rule entity into
  /// the following two subtypes:
  /// D - Default recapture rules that are based upon Agency Policy.
  /// O - Obligor recapture rules that are based upon negotiations with the 
  /// obligor.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("O")]
  [Value("D")]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>
  /// The value of the PASSTHRU_PERCENTAGE attribute.
  /// The percentage of passthru disbursements that we can retain per 
  /// disbursement in order to pay off the recovery debt.
  /// </summary>
  [JsonPropertyName("passthruPercentage")]
  [Member(Index = 10, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? PassthruPercentage
  {
    get => passthruPercentage;
    set => passthruPercentage = value;
  }

  /// <summary>
  /// The value of the PASSTHRU_AMOUNT attribute.
  /// The amount of passthru disbursements that we can retaine per disbursement 
  /// to pay off a recovery debt.
  /// </summary>
  [JsonPropertyName("passthruAmount")]
  [Member(Index = 11, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? PassthruAmount
  {
    get => passthruAmount;
    set => passthruAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the PASSTHRU_MAX_AMOUNT attribute.
  /// The maximum amount of passthru disbursements that we can retaine in any 
  /// given month in order to pay off recovery debts.
  /// </summary>
  [JsonPropertyName("passthruMaxAmount")]
  [Member(Index = 12, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? PassthruMaxAmount
  {
    get => passthruMaxAmount;
    set => passthruMaxAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence is activated by the system.  An 
  /// effective date allows the entity to be entered into the system with a
  /// future date.  The occurrence of the entity will &quot;take effect&quot; on
  /// the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 13, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The date upon which this occurrence of the entity can no longer be used.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => createdBy ?? "";
    set => createdBy = TrimEnd(Substring(value, 1, CreatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the CreatedBy attribute.</summary>
  [JsonPropertyName("createdBy")]
  [Computed]
  public string CreatedBy_Json
  {
    get => NullIf(CreatedBy, "");
    set => CreatedBy = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 16, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 18, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>
  /// The value of the REPAYMENT_LETTER_PRINT_DATE attribute.
  /// Defines the date the Repayment Letter was printed.
  /// </summary>
  [JsonPropertyName("repaymentLetterPrintDate")]
  [Member(Index = 19, Type = MemberType.Date, Optional = true)]
  public DateTime? RepaymentLetterPrintDate
  {
    get => repaymentLetterPrintDate;
    set => repaymentLetterPrintDate = value;
  }

  /// <summary>Length of the OBLIGOR_RULE_FILLER attribute.</summary>
  public const int ObligorRuleFiller_MaxLength = 1;

  /// <summary>
  /// The value of the OBLIGOR_RULE_FILLER attribute.
  /// This attribute is needed because IEF requires each subtype to have at 
  /// least one unique attribute.
  /// </summary>
  [JsonPropertyName("obligorRuleFiller")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = ObligorRuleFiller_MaxLength, Optional = true)]
  public string ObligorRuleFiller
  {
    get => obligorRuleFiller;
    set => obligorRuleFiller = value != null
      ? TrimEnd(Substring(value, 1, ObligorRuleFiller_MaxLength)) : null;
  }

  /// <summary>Length of the DEFAULT_RULE_FILLER attribute.</summary>
  public const int DefaultRuleFiller_MaxLength = 1;

  /// <summary>
  /// The value of the DEFAULT_RULE_FILLER attribute.
  /// This attribute is needed because IEF requires each subtype to have at 
  /// least one unique attribute.
  /// </summary>
  [JsonPropertyName("defaultRuleFiller")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = DefaultRuleFiller_MaxLength, Optional = true)]
  public string DefaultRuleFiller
  {
    get => defaultRuleFiller;
    set => defaultRuleFiller = value != null
      ? TrimEnd(Substring(value, 1, DefaultRuleFiller_MaxLength)) : null;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaDType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonPropertyName("cpaDType")]
  [Member(Index = 22, Type = MemberType.Char, Length = CpaDType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaDType
  {
    get => cpaDType;
    set => cpaDType = value != null
      ? TrimEnd(Substring(value, 1, CpaDType_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspDNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspDNumber")]
  [Member(Index = 23, Type = MemberType.Char, Length = CspDNumber_MaxLength, Optional
    = true)]
  public string CspDNumber
  {
    get => cspDNumber;
    set => cspDNumber = value != null
      ? TrimEnd(Substring(value, 1, CspDNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("dtyGeneratedId")]
  [Member(Index = 24, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? DtyGeneratedId
  {
    get => dtyGeneratedId;
    set => dtyGeneratedId = value;
  }

  private DateTime? negotiatedDate;
  private decimal? nonAdcArrearsMaxAmount;
  private decimal? nonAdcArrearsAmount;
  private int? nonAdcArrearsPercentage;
  private decimal? nonAdcCurrentMaxAmount;
  private decimal? nonAdcCurrentAmount;
  private int? nonAdcCurrentPercentage;
  private string type1;
  private int systemGeneratedIdentifier;
  private int? passthruPercentage;
  private decimal? passthruAmount;
  private decimal? passthruMaxAmount;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private DateTime? repaymentLetterPrintDate;
  private string obligorRuleFiller;
  private string defaultRuleFiller;
  private string cpaDType;
  private string cspDNumber;
  private int? dtyGeneratedId;
}
