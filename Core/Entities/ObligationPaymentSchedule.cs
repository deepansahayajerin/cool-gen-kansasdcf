// The source file: OBLIGATION_PAYMENT_SCHEDULE, ID: 371438064, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// The frequency, amount due and periods for the specific year for a 
/// obligation.  Payment schedules may be defined to represent child support
/// obligations, medical insurance premiums, spousal support, recovery debts,
/// etc.
/// Examples:
///   1. Monthly, $100.00, Months: Sept-May.
///   2. Monthly, $250.00, Months: Jan-Dec.
///   3. Weekly, $50.00, Weeks: 1-20 &amp; 40-52.
/// </summary>
[Serializable]
public partial class ObligationPaymentSchedule: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ObligationPaymentSchedule()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ObligationPaymentSchedule(ObligationPaymentSchedule that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ObligationPaymentSchedule Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ObligationPaymentSchedule that)
  {
    base.Assign(that);
    frequencyCode = that.frequencyCode;
    dayOfWeek = that.dayOfWeek;
    dayOfMonth1 = that.dayOfMonth1;
    dayOfMonth2 = that.dayOfMonth2;
    periodInd = that.periodInd;
    amount = that.amount;
    startDt = that.startDt;
    endDt = that.endDt;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdateBy = that.lastUpdateBy;
    lastUpdateTmst = that.lastUpdateTmst;
    repaymentLetterPrintDate = that.repaymentLetterPrintDate;
    otyType = that.otyType;
    obgGeneratedId = that.obgGeneratedId;
    obgCspNumber = that.obgCspNumber;
    obgCpaType = that.obgCpaType;
  }

  /// <summary>Length of the FREQUENCY_CODE attribute.</summary>
  public const int FrequencyCode_MaxLength = 2;

  /// <summary>
  /// The value of the FREQUENCY_CODE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = FrequencyCode_MaxLength)]
  [Value("BM")]
  [Value("M")]
  [Value("W")]
  [Value("BW")]
  [Value("SM")]
  [ImplicitValue("M")]
  public string FrequencyCode
  {
    get => frequencyCode ?? "";
    set => frequencyCode =
      TrimEnd(Substring(value, 1, FrequencyCode_MaxLength));
  }

  /// <summary>
  /// The json value of the FrequencyCode attribute.</summary>
  [JsonPropertyName("frequencyCode")]
  [Computed]
  public string FrequencyCode_Json
  {
    get => NullIf(FrequencyCode, "");
    set => FrequencyCode = value;
  }

  /// <summary>
  /// The value of the DAY_OF_WEEK attribute.
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
  /// Used for Bi-Monthly &amp; Bi-Weekly (One debt created every two months or 
  /// one debt created every two weeks).  It will indicate whether the month or
  /// week is even or odd.
  /// </summary>
  [JsonPropertyName("periodInd")]
  [Member(Index = 5, Type = MemberType.Char, Length = PeriodInd_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("")]
  [Value("O")]
  [Value("E")]
  [ImplicitValue("O")]
  public string PeriodInd
  {
    get => periodInd;
    set => periodInd = value != null
      ? TrimEnd(Substring(value, 1, PeriodInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the AMOUNT attribute.
  /// The dollar amount to be paid periodically as scheduled.
  /// </summary>
  [JsonPropertyName("amount")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? Amount
  {
    get => amount;
    set => amount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the START_DT attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("startDt")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? StartDt
  {
    get => startDt;
    set => startDt = value;
  }

  /// <summary>
  /// The value of the END_DT attribute.
  /// The date upon which this occurrence of the enity can no longer be used.
  /// </summary>
  [JsonPropertyName("endDt")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDt
  {
    get => endDt;
    set => endDt = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User Id or Program Id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TMST attribute.
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 10, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the LAST_UPDATE_BY attribute.</summary>
  public const int LastUpdateBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATE_BY attribute.
  /// The User Id or Program Id responsible for the last change to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdateBy")]
  [Member(Index = 11, Type = MemberType.Char, Length = LastUpdateBy_MaxLength, Optional
    = true)]
  public string LastUpdateBy
  {
    get => lastUpdateBy;
    set => lastUpdateBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdateBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATE_TMST attribute.
  /// The timestamp of the last update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdateTmst")]
  [Member(Index = 12, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdateTmst
  {
    get => lastUpdateTmst;
    set => lastUpdateTmst = value;
  }

  /// <summary>
  /// The value of the REPAYMENT_LETTER_PRINT_DATE attribute.
  /// Date this letter was printed.
  /// </summary>
  [JsonPropertyName("repaymentLetterPrintDate")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? RepaymentLetterPrintDate
  {
    get => repaymentLetterPrintDate;
    set => repaymentLetterPrintDate = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyType")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 3)]
  public int OtyType
  {
    get => otyType;
    set => otyType = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 3)]
  public int ObgGeneratedId
  {
    get => obgGeneratedId;
    set => obgGeneratedId = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int ObgCspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = ObgCspNumber_MaxLength)]
  public string ObgCspNumber
  {
    get => obgCspNumber ?? "";
    set => obgCspNumber = TrimEnd(Substring(value, 1, ObgCspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the ObgCspNumber attribute.</summary>
  [JsonPropertyName("obgCspNumber")]
  [Computed]
  public string ObgCspNumber_Json
  {
    get => NullIf(ObgCspNumber, "");
    set => ObgCspNumber = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int ObgCpaType_MaxLength = 1;

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
  [Member(Index = 17, Type = MemberType.Char, Length = ObgCpaType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string ObgCpaType
  {
    get => obgCpaType ?? "";
    set => obgCpaType = TrimEnd(Substring(value, 1, ObgCpaType_MaxLength));
  }

  /// <summary>
  /// The json value of the ObgCpaType attribute.</summary>
  [JsonPropertyName("obgCpaType")]
  [Computed]
  public string ObgCpaType_Json
  {
    get => NullIf(ObgCpaType, "");
    set => ObgCpaType = value;
  }

  private string frequencyCode;
  private int? dayOfWeek;
  private int? dayOfMonth1;
  private int? dayOfMonth2;
  private string periodInd;
  private decimal? amount;
  private DateTime? startDt;
  private DateTime? endDt;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdateBy;
  private DateTime? lastUpdateTmst;
  private DateTime? repaymentLetterPrintDate;
  private int otyType;
  private int obgGeneratedId;
  private string obgCspNumber;
  private string obgCpaType;
}
