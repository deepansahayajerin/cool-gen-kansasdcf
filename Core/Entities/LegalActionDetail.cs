// The source file: LEGAL_ACTION_DETAIL, ID: 371436746, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// Scope: The detailed findings on a legal action.
/// Example: Current Support, Arrears, Medical Insurance.
/// </summary>
[Serializable]
public partial class LegalActionDetail: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LegalActionDetail()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LegalActionDetail(LegalActionDetail that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LegalActionDetail Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LegalActionDetail that)
  {
    base.Assign(that);
    freqPeriodCode = that.freqPeriodCode;
    dayOfWeek = that.dayOfWeek;
    dayOfMonth1 = that.dayOfMonth1;
    dayOfMonth2 = that.dayOfMonth2;
    periodInd = that.periodInd;
    number = that.number;
    detailType = that.detailType;
    endDate = that.endDate;
    effectiveDate = that.effectiveDate;
    description = that.description;
    bondAmount = that.bondAmount;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    kpcDate = that.kpcDate;
    nonFinOblgType = that.nonFinOblgType;
    limit = that.limit;
    arrearsAmount = that.arrearsAmount;
    currentAmount = that.currentAmount;
    judgementAmount = that.judgementAmount;
    lgaIdentifier = that.lgaIdentifier;
    otyId = that.otyId;
  }

  /// <summary>Length of the FREQ_PERIOD_CODE attribute.</summary>
  public const int FreqPeriodCode_MaxLength = 2;

  /// <summary>
  /// The value of the FREQ_PERIOD_CODE attribute.
  /// This attribute specifies the frequency of the payment.  E.g. weekly, 
  /// monthly, bimonthly, etc.  The valid values are maintained in COCE_VALUE
  /// table for the code name FREQUENCY.
  /// </summary>
  [JsonPropertyName("freqPeriodCode")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = FreqPeriodCode_MaxLength, Optional = true)]
  public string FreqPeriodCode
  {
    get => freqPeriodCode;
    set => freqPeriodCode = value != null
      ? TrimEnd(Substring(value, 1, FreqPeriodCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DAY_OF_WEEK attribute.
  /// *** Draft *** 1-23-95	
  /// The numerical equivalence of the day of the calendar week for which a 
  /// weekly payment is due, according to the table below.
  /// 1 = Sunday
  /// 2 = Monday
  /// 3 = Tuesday
  /// 4 = Wednesday
  /// 5 = Thursday
  /// 6 = Friday
  /// 7 = Saturday
  /// Example:  A payment due every Friday will be coded a &quot;6&quot;.
  /// </summary>
  [JsonPropertyName("dayOfWeek")]
  [Member(Index = 2, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? DayOfWeek
  {
    get => dayOfWeek;
    set => dayOfWeek = value;
  }

  /// <summary>
  /// The value of the DAY_OF_MONTH_1 attribute.
  /// *** Draft *** 1-23-95
  /// In the case of a monthly payment schedule, the day of the month on which 
  /// the monthly payment is due.
  /// Example:  Ordered to pay monthly on the 15th of each month.  The 
  /// occurrence will be coded &quot;15&quot;
  /// In the case of a semi-monthly payment schedule, the day of month on which 
  /// the first payment of the month is due.
  /// Example:  Ordered to pay semi-monthly on the 10th and the 28th, the 
  /// occurrence will be coded &quot;10&quot;.
  /// </summary>
  [JsonPropertyName("dayOfMonth1")]
  [Member(Index = 3, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? DayOfMonth1
  {
    get => dayOfMonth1;
    set => dayOfMonth1 = value;
  }

  /// <summary>
  /// The value of the DAY_OF_MONTH_2 attribute.
  /// *** Draft *** 1-23-95
  /// Designates the second payment due date for semi-monthly payment schedules.
  /// Example:  Ordered to pay semi-monthly on the 10th and the 28th of every 
  /// month, the occurrence will be coded &quot;28&quot;.
  /// </summary>
  [JsonPropertyName("dayOfMonth2")]
  [Member(Index = 4, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? DayOfMonth2
  {
    get => dayOfMonth2;
    set => dayOfMonth2 = value;
  }

  /// <summary>Length of the PERIOD_IND attribute.</summary>
  public const int PeriodInd_MaxLength = 1;

  /// <summary>
  /// The value of the PERIOD_IND attribute.
  /// *** DRAFT ***
  /// Used for Semi-Monthly (One debt created every two months).  It will 
  /// indicate whether the month is even or odd.
  /// </summary>
  [JsonPropertyName("periodInd")]
  [Member(Index = 5, Type = MemberType.Char, Length = PeriodInd_MaxLength, Optional
    = true)]
  public string PeriodInd
  {
    get => periodInd;
    set => periodInd = value != null
      ? TrimEnd(Substring(value, 1, PeriodInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A number, unique within the legal action, used to identify each detail of 
  /// the Legal Action.  Starts with one and moves sequentially.
  /// </summary>
  [JsonPropertyName("number")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 2)]
  public int Number
  {
    get => number;
    set => number = value;
  }

  /// <summary>Length of the DETAIL_TYPE attribute.</summary>
  public const int DetailType_MaxLength = 1;

  /// <summary>
  /// The value of the DETAIL_TYPE attribute.
  /// This partitioning attribute is used to identity the type of detail, &quot;
  /// F&quot; for financial and &quot;N&quot; for non-financial.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = DetailType_MaxLength)]
  [Value("F")]
  [Value("N")]
  public string DetailType
  {
    get => detailType ?? "";
    set => detailType = TrimEnd(Substring(value, 1, DetailType_MaxLength));
  }

  /// <summary>
  /// The json value of the DetailType attribute.</summary>
  [JsonPropertyName("detailType")]
  [Computed]
  public string DetailType_Json
  {
    get => NullIf(DetailType, "");
    set => DetailType = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The specific date that the support action is to end.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// A specific date that a support obligation is to begin.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// The detailed description of each detail of the Legal Action.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Varchar, Length = Description_MaxLength)]
    
  public string Description
  {
    get => description ?? "";
    set => description = Substring(value, 1, Description_MaxLength);
  }

  /// <summary>
  /// The json value of the Description attribute.</summary>
  [JsonPropertyName("description")]
  [Computed]
  public string Description_Json
  {
    get => NullIf(Description, "");
    set => Description = value;
  }

  /// <summary>
  /// The value of the BOND_AMOUNT attribute.
  /// The specific dollar amount ordered by the court setting the bond.
  /// Surety Bond
  /// Cash Bond
  /// Own Recognizance Bond
  /// NOTE: Scott is researching to find out if more is needed for bonds.
  /// </summary>
  [JsonPropertyName("bondAmount")]
  [Member(Index = 11, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? BondAmount
  {
    get => bondAmount;
    set => bondAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 13, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person that last updated the occurrance of the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 15, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>
  /// The value of the KPC_DATE attribute.
  /// This field will store the date when the legal action associated to this 
  /// legal action detail is picked up by the batch program 'Fn_B691_KPC
  /// Court_Order_Extract' (SWEF691B).
  /// </summary>
  [JsonPropertyName("kpcDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? KpcDate
  {
    get => kpcDate;
    set => kpcDate = value;
  }

  /// <summary>Length of the NON_FIN_OBLG_TYPE attribute.</summary>
  public const int NonFinOblgType_MaxLength = 4;

  /// <summary>
  /// The value of the NON_FIN_OBLG_TYPE attribute.
  /// This attribute specifies the type of non-financial obligation type 
  /// specified in the court order. The valid values are maintained in
  /// CODE_VALUE entity for code name LA DET NON FIN OBLG TYPE.
  /// </summary>
  [JsonPropertyName("nonFinOblgType")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = NonFinOblgType_MaxLength, Optional = true)]
  public string NonFinOblgType
  {
    get => nonFinOblgType;
    set => nonFinOblgType = value != null
      ? TrimEnd(Substring(value, 1, NonFinOblgType_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LIMIT attribute.
  /// A restriction on the percentage amount of an individuals disposable 
  /// earnings that may be withheld or garnished for the support of any person.
  /// </summary>
  [JsonPropertyName("limit")]
  [Member(Index = 18, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? Limit
  {
    get => limit;
    set => limit = value;
  }

  /// <summary>
  /// The value of the ARREARS_AMOUNT attribute.
  /// This indicates the amount that is to be applied to arrears per interval.  
  /// This amount is the total for all the supported persons in the given Legal
  /// Detail.
  /// </summary>
  [JsonPropertyName("arrearsAmount")]
  [Member(Index = 19, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? ArrearsAmount
  {
    get => arrearsAmount;
    set => arrearsAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the CURRENT_AMOUNT attribute.
  /// This indicates the amount that is owed.  This amount is per interval. This
  /// amount is the total for all the supported persons in the given Legal
  /// Detail.
  /// </summary>
  [JsonPropertyName("currentAmount")]
  [Member(Index = 20, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? CurrentAmount
  {
    get => currentAmount;
    set => currentAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the JUDGEMENT_AMOUNT attribute.
  /// The total amount to be paid for a particular detail type. This amount is 
  /// the total for all the supported persons in the given Legal Detail.
  /// </summary>
  [JsonPropertyName("judgementAmount")]
  [Member(Index = 21, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? JudgementAmount
  {
    get => judgementAmount;
    set => judgementAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 22, Type = MemberType.Number, Length = 9)]
  public int LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyId")]
  [Member(Index = 23, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OtyId
  {
    get => otyId;
    set => otyId = value;
  }

  private string freqPeriodCode;
  private int? dayOfWeek;
  private int? dayOfMonth1;
  private int? dayOfMonth2;
  private string periodInd;
  private int number;
  private string detailType;
  private DateTime? endDate;
  private DateTime? effectiveDate;
  private string description;
  private decimal? bondAmount;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private DateTime? kpcDate;
  private string nonFinOblgType;
  private int? limit;
  private decimal? arrearsAmount;
  private decimal? currentAmount;
  private decimal? judgementAmount;
  private int lgaIdentifier;
  private int? otyId;
}
