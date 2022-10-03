// The source file: MONTHLY_OBLIGEE_SUMMARY, ID: 371437403, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This entity records the monthly totals for various types of money related to
/// assistance.  In general the totals being kept are for the amount of money
/// granted and the amount of money collected.  The monthly totals kept may
/// relate to any type of CSE_Person or may exist only for one or two types of
/// CSE_Person.
/// </summary>
[Serializable]
public partial class MonthlyObligeeSummary: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public MonthlyObligeeSummary()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public MonthlyObligeeSummary(MonthlyObligeeSummary that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new MonthlyObligeeSummary Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(MonthlyObligeeSummary that)
  {
    base.Assign(that);
    year = that.year;
    month = that.month;
    passthruRecapAmt = that.passthruRecapAmt;
    disbursementsSuppressed = that.disbursementsSuppressed;
    recapturedAmt = that.recapturedAmt;
    naArrearsRecapAmt = that.naArrearsRecapAmt;
    passthruAmount = that.passthruAmount;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    collectionsAmount = that.collectionsAmount;
    collectionsDisbursedToAr = that.collectionsDisbursedToAr;
    feeAmount = that.feeAmount;
    adcReimbursedAmount = that.adcReimbursedAmount;
    zdelType = that.zdelType;
    totExcessUraAmt = that.totExcessUraAmt;
    numberOfCollections = that.numberOfCollections;
    naCurrRecapAmt = that.naCurrRecapAmt;
    cpaSType = that.cpaSType;
    cspSNumber = that.cspSNumber;
  }

  /// <summary>
  /// The value of the YEAR attribute.
  /// This attribute represents the year for which we are keeping track of the 
  /// monetary fields related to a CSE_Person_Account.
  /// </summary>
  [JsonPropertyName("year")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 4)]
  public int Year
  {
    get => year;
    set => year = value;
  }

  /// <summary>
  /// The value of the MONTH attribute.
  /// This attribute represents the month for which we are keeping track of 
  /// monetary amounts for a CSE_Person_Account.
  /// </summary>
  [JsonPropertyName("month")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int Month
  {
    get => month;
    set => month = value;
  }

  /// <summary>
  /// The value of the PASSTHRU_RECAP_AMT attribute.
  /// This is the amount of passthru collections that was recaptured during a 
  /// year and month for an obligee. This amount is tracked by process date and
  /// not by collection date.
  /// </summary>
  [JsonPropertyName("passthruRecapAmt")]
  [Member(Index = 3, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? PassthruRecapAmt
  {
    get => passthruRecapAmt;
    set => passthruRecapAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the DISBURSEMENTS_SUPPRESSED attribute.
  /// This field will contain the total amount of suppressed disbursements for 
  /// an obligee where the collection date associated to the disbursement is for
  /// the specified year and month.
  /// </summary>
  [JsonPropertyName("disbursementsSuppressed")]
  [Member(Index = 4, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? DisbursementsSuppressed
  {
    get => disbursementsSuppressed;
    set => disbursementsSuppressed = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the RECAPTURED_AMT attribute.
  /// The amount of money recaptured from all kinds of disbursements in a given 
  /// month.
  /// </summary>
  [JsonPropertyName("recapturedAmt")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? RecapturedAmt
  {
    get => recapturedAmt;
    set => recapturedAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the NA_ARREARS_RECAP_AMT attribute.
  /// This is the amount of arrears ADC collections recaptured during a year and
  /// month for an Obligee. This amount is tracked by process date and not by
  /// collection date.
  /// </summary>
  [JsonPropertyName("naArrearsRecapAmt")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? NaArrearsRecapAmt
  {
    get => naArrearsRecapAmt;
    set => naArrearsRecapAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the PASSTHRU_AMOUNT attribute.
  /// Amount of money paid to an AR as a passhthru for the particular year and 
  /// month.
  /// </summary>
  [JsonPropertyName("passthruAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal PassthruAmount
  {
    get => passthruAmount;
    set => passthruAmount = Truncate(value, 2);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 9, Type = MemberType.Timestamp)]
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
  [Member(Index = 10, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 11, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>
  /// The value of the COLLECTIONS_AMOUNT attribute.
  /// The total dollar amount for a particular month that has come in for or on 
  /// the behalf of this obligee.
  /// </summary>
  [JsonPropertyName("collectionsAmount")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CollectionsAmount
  {
    get => collectionsAmount;
    set => collectionsAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the COLLECTIONS_DISBURSED_TO_AR attribute.
  /// The total dollar amount for a particular month that was disbursed out to 
  /// the obligee after all fees have been taken out.
  /// </summary>
  [JsonPropertyName("collectionsDisbursedToAr")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CollectionsDisbursedToAr
  {
    get => collectionsDisbursedToAr;
    set => collectionsDisbursedToAr = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the FEE AMOUNT attribute.
  /// The total dollar amount for a particular month that was charged as fees to
  /// the ar/payee.
  /// </summary>
  [JsonPropertyName("feeAmount")]
  [Member(Index = 14, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? FeeAmount
  {
    get => feeAmount;
    set => feeAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the ADC REIMBURSED AMOUNT attribute.
  /// The total dollar amount for a particular month that was disbursed to the 
  /// state for an AR who was on ADC.
  /// </summary>
  [JsonPropertyName("adcReimbursedAmount")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AdcReimbursedAmount
  {
    get => adcReimbursedAmount;
    set => adcReimbursedAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the ZDEL_TYPE attribute.</summary>
  public const int ZdelType_MaxLength = 3;

  /// <summary>
  /// The value of the ZDEL_TYPE attribute.
  /// This attribute identifies the AE Case (IM Household) as AFDC (AF) or 
  /// Foster Care (FC). If the AE Case is Foster Care the child is treated as
  /// the Obligee for receiving grants and passthrus.
  /// </summary>
  [JsonPropertyName("zdelType")]
  [Member(Index = 16, Type = MemberType.Char, Length = ZdelType_MaxLength, Optional
    = true)]
  public string ZdelType
  {
    get => zdelType;
    set => zdelType = value != null
      ? TrimEnd(Substring(value, 1, ZdelType_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the TOT_EXCESS_URA_AMT attribute.
  /// This attribute holds the total amount of excess ura money that has been 
  /// disbursed during the month. This attribute will be used for the PSUM
  /// screen. This attribute will be updated each time an excess URA
  /// disbursement is released. Excess URA disbursements are usually created in
  /// a suppress status with a release date set for 30 days from the process
  /// date.
  /// </summary>
  [JsonPropertyName("totExcessUraAmt")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotExcessUraAmt
  {
    get => totExcessUraAmt;
    set => totExcessUraAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the NUMBER_OF_COLLECTIONS attribute.
  /// This field will contain the number of collections that occurred in the 
  /// year month for the Obligee. A collection is defined as a single cash
  /// receipt detail that was disbursed to the obligees account.
  /// </summary>
  [JsonPropertyName("numberOfCollections")]
  [Member(Index = 18, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? NumberOfCollections
  {
    get => numberOfCollections;
    set => numberOfCollections = value;
  }

  /// <summary>
  /// The value of the NA_CURR_RECAP_AMT attribute.
  /// This is the amount of current ADC collections recaptured during and year 
  /// and month for an Obligee. This amount is tracked by process date and not
  /// by collection date.
  /// </summary>
  [JsonPropertyName("naCurrRecapAmt")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? NaCurrRecapAmt
  {
    get => naCurrRecapAmt;
    set => naCurrRecapAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaSType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = CpaSType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaSType
  {
    get => cpaSType ?? "";
    set => cpaSType = TrimEnd(Substring(value, 1, CpaSType_MaxLength));
  }

  /// <summary>
  /// The json value of the CpaSType attribute.</summary>
  [JsonPropertyName("cpaSType")]
  [Computed]
  public string CpaSType_Json
  {
    get => NullIf(CpaSType, "");
    set => CpaSType = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspSNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length = CspSNumber_MaxLength)]
  public string CspSNumber
  {
    get => cspSNumber ?? "";
    set => cspSNumber = TrimEnd(Substring(value, 1, CspSNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspSNumber attribute.</summary>
  [JsonPropertyName("cspSNumber")]
  [Computed]
  public string CspSNumber_Json
  {
    get => NullIf(CspSNumber, "");
    set => CspSNumber = value;
  }

  private int year;
  private int month;
  private decimal? passthruRecapAmt;
  private decimal? disbursementsSuppressed;
  private decimal? recapturedAmt;
  private decimal? naArrearsRecapAmt;
  private decimal passthruAmount;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private decimal? collectionsAmount;
  private decimal? collectionsDisbursedToAr;
  private decimal? feeAmount;
  private decimal? adcReimbursedAmount;
  private string zdelType;
  private decimal? totExcessUraAmt;
  private int? numberOfCollections;
  private decimal? naCurrRecapAmt;
  private string cpaSType;
  private string cspSNumber;
}
