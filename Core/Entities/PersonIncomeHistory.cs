// The source file: PERSON_INCOME_HISTORY, ID: 371439288, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// 	
/// Earned and unearned income received by a CSE Person.  For example: military 
/// pay, retirement benefits, social security benefits, royalties, wages,
/// interest on accounts or investments.
/// FED REQ: B-1.a.3
/// </summary>
[Serializable]
public partial class PersonIncomeHistory: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PersonIncomeHistory()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PersonIncomeHistory(PersonIncomeHistory that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PersonIncomeHistory Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PersonIncomeHistory that)
  {
    base.Assign(that);
    identifier = that.identifier;
    incomeEffDt = that.incomeEffDt;
    incomeAmt = that.incomeAmt;
    freq = that.freq;
    workerId = that.workerId;
    verifiedDt = that.verifiedDt;
    checkEarned = that.checkEarned;
    checkEarnedFrequency = that.checkEarnedFrequency;
    checkUnearned = that.checkUnearned;
    checkUnearnedFrequency = that.checkUnearnedFrequency;
    checkPayDate = that.checkPayDate;
    checkDeferredCompensation = that.checkDeferredCompensation;
    checkLastUpdateDate = that.checkLastUpdateDate;
    paymentType = that.paymentType;
    checkMonthlyAmount = that.checkMonthlyAmount;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    militaryBaqAllotment = that.militaryBaqAllotment;
    cspNumber = that.cspNumber;
    cspINumber = that.cspINumber;
    isrIdentifier = that.isrIdentifier;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// </summary>
  [JsonPropertyName("identifier")]
  [Member(Index = 1, Type = MemberType.Timestamp)]
  public DateTime? Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the INCOME_EFF_DT attribute.
  /// Date that the income amount became effective.
  /// This will differ from the start date when
  /// e.g. a pay raise has been applied
  ///      an allowance has been changed
  /// </summary>
  [JsonPropertyName("incomeEffDt")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? IncomeEffDt
  {
    get => incomeEffDt;
    set => incomeEffDt = value;
  }

  /// <summary>
  /// The value of the INCOME_AMT attribute.
  /// The amount of income currently being received.
  /// </summary>
  [JsonPropertyName("incomeAmt")]
  [Member(Index = 3, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? IncomeAmt
  {
    get => incomeAmt;
    set => incomeAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the FREQ attribute.</summary>
  public const int Freq_MaxLength = 1;

  /// <summary>
  /// The value of the FREQ attribute.
  /// The frequency in which the income is received.
  /// e.g. Weekly
  ///      Monthly
  ///      Yearly
  /// Permitted values are maintained in CODE_VALUE entity for the CODE_NAME 
  /// FREQUENCY_TYPE
  /// </summary>
  [JsonPropertyName("freq")]
  [Member(Index = 4, Type = MemberType.Char, Length = Freq_MaxLength, Optional
    = true)]
  public string Freq
  {
    get => freq;
    set => freq = value != null
      ? TrimEnd(Substring(value, 1, Freq_MaxLength)) : null;
  }

  /// <summary>Length of the WORKER_ID attribute.</summary>
  public const int WorkerId_MaxLength = 8;

  /// <summary>
  /// The value of the WORKER_ID attribute.
  /// The ID of the person who last changed the income details.
  /// </summary>
  [JsonPropertyName("workerId")]
  [Member(Index = 5, Type = MemberType.Char, Length = WorkerId_MaxLength, Optional
    = true)]
  public string WorkerId
  {
    get => workerId;
    set => workerId = value != null
      ? TrimEnd(Substring(value, 1, WorkerId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the VERIFIED_DT attribute.
  /// The date that the income was verified.
  /// </summary>
  [JsonPropertyName("verifiedDt")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? VerifiedDt
  {
    get => verifiedDt;
    set => verifiedDt = value;
  }

  /// <summary>
  /// The value of the CHECK_EARNED attribute.
  /// The income/wages of a CSE person that is reported to the state for 
  /// unemployed compensation and social security benefits.
  /// </summary>
  [JsonPropertyName("checkEarned")]
  [Member(Index = 7, Type = MemberType.Number, Length = 10, Precision = 2, Optional
    = true)]
  [ImplicitValue("0")]
  public decimal? CheckEarned
  {
    get => checkEarned;
    set => checkEarned = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the CHECK_EARNED_FREQUENCY attribute.</summary>
  public const int CheckEarnedFrequency_MaxLength = 7;

  /// <summary>
  /// The value of the CHECK_EARNED_FREQUENCY attribute.
  /// The periodic increment that further identifies the time frame for 
  /// distribution of the paycheck to the CSE person in reguard to earned
  /// income.
  /// The permitted values are maintained in CODE_VALUE entity for CODE_NAME = 
  /// FREQUENCY_CODE
  /// </summary>
  [JsonPropertyName("checkEarnedFrequency")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = CheckEarnedFrequency_MaxLength, Optional = true)]
  [Value(null)]
  [Value("MONTHLY")]
  [Value("DAILY")]
  [Value("ANNUAL")]
  [Value("HOURLY")]
  [Value("WEEKLY")]
  [ImplicitValue("MONTHLY")]
  public string CheckEarnedFrequency
  {
    get => checkEarnedFrequency;
    set => checkEarnedFrequency = value != null
      ? TrimEnd(Substring(value, 1, CheckEarnedFrequency_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CHECK_UNEARNED attribute.
  /// Income from passive sources, such as stocks, bonds, investments, etc.
  /// </summary>
  [JsonPropertyName("checkUnearned")]
  [Member(Index = 9, Type = MemberType.Number, Length = 10, Precision = 2, Optional
    = true)]
  [ImplicitValue("0")]
  public decimal? CheckUnearned
  {
    get => checkUnearned;
    set => checkUnearned = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the CHECK_UNEARNED_FREQUENCY attribute.</summary>
  public const int CheckUnearnedFrequency_MaxLength = 7;

  /// <summary>
  /// The value of the CHECK_UNEARNED_FREQUENCY attribute.
  /// The periodic increment that further identifies the time frame for 
  /// distribtuion of the paycheck to the CSE person in reguard to unearned
  /// income.
  /// The permitted values are maintained in CODE_VALUE entity for CODE_NAME 
  /// FREQUENCY_CODE.
  /// </summary>
  [JsonPropertyName("checkUnearnedFrequency")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = CheckUnearnedFrequency_MaxLength, Optional = true)]
  [Value(null)]
  [Value("MONTHLY")]
  [Value("WEEKLY")]
  [Value("ANNUAL")]
  [Value("HOURLY")]
  [Value("DAILY")]
  [ImplicitValue("MONTHLY")]
  public string CheckUnearnedFrequency
  {
    get => checkUnearnedFrequency;
    set => checkUnearnedFrequency = value != null
      ? TrimEnd(Substring(value, 1, CheckUnearnedFrequency_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CHECK_PAY_DATE attribute.
  /// The actual date that a CSE person is paid by a resource.
  /// </summary>
  [JsonPropertyName("checkPayDate")]
  [Member(Index = 11, Type = MemberType.Date, Optional = true)]
  public DateTime? CheckPayDate
  {
    get => checkPayDate;
    set => checkPayDate = value;
  }

  /// <summary>
  /// The value of the CHECK_DEFERRED_COMPENSATION attribute.
  /// An amount deducted from gross earned income on which payment of taxes is 
  /// defered, usually until retirement.
  /// </summary>
  [JsonPropertyName("checkDeferredCompensation")]
  [Member(Index = 12, Type = MemberType.Number, Length = 10, Precision = 2, Optional
    = true)]
  [ImplicitValue("0")]
  public decimal? CheckDeferredCompensation
  {
    get => checkDeferredCompensation;
    set => checkDeferredCompensation = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the CHECK_LAST_UPDATE_DATE attribute.
  /// The latest date when information on the CSE Person's income was supplied.
  /// </summary>
  [JsonPropertyName("checkLastUpdateDate")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? CheckLastUpdateDate
  {
    get => checkLastUpdateDate;
    set => checkLastUpdateDate = value;
  }

  /// <summary>Length of the PAYMENT_TYPE attribute.</summary>
  public const int PaymentType_MaxLength = 2;

  /// <summary>
  /// The value of the PAYMENT_TYPE attribute.
  /// The sorce of compensation.  For example:  what?  once determined, adjust 
  /// attribute length.
  /// </summary>
  [JsonPropertyName("paymentType")]
  [Member(Index = 14, Type = MemberType.Char, Length = PaymentType_MaxLength, Optional
    = true)]
  public string PaymentType
  {
    get => paymentType;
    set => paymentType = value != null
      ? TrimEnd(Substring(value, 1, PaymentType_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CHECK_MONTHLY_AMOUNT attribute.
  /// The dollar amount of compensation to the CSE Person.
  /// </summary>
  [JsonPropertyName("checkMonthlyAmount")]
  [Member(Index = 15, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? CheckMonthlyAmount
  {
    get => checkMonthlyAmount;
    set => checkMonthlyAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 17, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 19, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the MILITARY_BAQ_ALLOTMENT attribute.
  /// BONUS MONTHLY INCOME FOR MILITARY PERSONNEL
  /// </summary>
  [JsonPropertyName("militaryBaqAllotment")]
  [Member(Index = 20, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? MilitaryBaqAllotment
  {
    get => militaryBaqAllotment;
    set => militaryBaqAllotment = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspINumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length = CspINumber_MaxLength)]
  public string CspINumber
  {
    get => cspINumber ?? "";
    set => cspINumber = TrimEnd(Substring(value, 1, CspINumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspINumber attribute.</summary>
  [JsonPropertyName("cspINumber")]
  [Computed]
  public string CspINumber_Json
  {
    get => NullIf(CspINumber, "");
    set => CspINumber = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated identifier of no business meaning.
  /// </summary>
  [JsonPropertyName("isrIdentifier")]
  [Member(Index = 23, Type = MemberType.Timestamp)]
  public DateTime? IsrIdentifier
  {
    get => isrIdentifier;
    set => isrIdentifier = value;
  }

  private DateTime? identifier;
  private DateTime? incomeEffDt;
  private decimal? incomeAmt;
  private string freq;
  private string workerId;
  private DateTime? verifiedDt;
  private decimal? checkEarned;
  private string checkEarnedFrequency;
  private decimal? checkUnearned;
  private string checkUnearnedFrequency;
  private DateTime? checkPayDate;
  private decimal? checkDeferredCompensation;
  private DateTime? checkLastUpdateDate;
  private string paymentType;
  private decimal? checkMonthlyAmount;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private decimal? militaryBaqAllotment;
  private string cspNumber;
  private string cspINumber;
  private DateTime? isrIdentifier;
}
