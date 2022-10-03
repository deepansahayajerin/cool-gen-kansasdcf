// The source file: CHILD_SUPPORT_SCHEDULE, ID: 371432225, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// A chart that was developed by the Child Support Guidelines Advisory 
/// Committee. The schedules are based upon national data regarding average
/// family expenditures for children, which may vary depending upon three major
/// factors: the parent's combined income, the number of children in the family,
/// and the ages of the children. The schedules are derived from an economic
/// model initially developed by Dr. William Terrall in 1987, updated in 1989 by
/// Dr. Ann Coulson using more current data, and modified downward at lower
/// income levels in 1990 at the court's request, and adjusted for current
/// economic data in 1993.
/// </summary>
[Serializable]
public partial class ChildSupportSchedule: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ChildSupportSchedule()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ChildSupportSchedule(ChildSupportSchedule that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ChildSupportSchedule Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ChildSupportSchedule that)
  {
    base.Assign(that);
    expirationDate = that.expirationDate;
    effectiveDate = that.effectiveDate;
    identifier = that.identifier;
    monthlyIncomePovertyLevelInd = that.monthlyIncomePovertyLevelInd;
    incomeMultiplier = that.incomeMultiplier;
    incomeExponent = that.incomeExponent;
    numberOfChildrenInFamily = that.numberOfChildrenInFamily;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    csGuidelineYear = that.csGuidelineYear;
  }

  /// <summary>
  /// The value of the EXPIRATION_DATE attribute.
  /// expiration date for the table.
  /// </summary>
  [JsonPropertyName("expirationDate")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? ExpirationDate
  {
    get => expirationDate;
    set => expirationDate = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// effective date for the table to be used.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Sequence Number to determine a unique record.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 2)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the MONTHLY_INCOME_POVERTY_LEVEL_IND attribute.
  /// THIS MONTHLY INCOME INDICATES THE START OF POVERTY LEVEL FOR A FAMILY WITH
  /// THE NUMBER OF CHILDREN GIVEN.
  /// </summary>
  [JsonPropertyName("monthlyIncomePovertyLevelInd")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 5)]
  public int MonthlyIncomePovertyLevelInd
  {
    get => monthlyIncomePovertyLevelInd;
    set => monthlyIncomePovertyLevelInd = value;
  }

  /// <summary>
  /// The value of the INCOME_MULTIPLIER attribute.
  /// Multiplying factor used for high income levels for age range 16 thru 18.
  /// </summary>
  [JsonPropertyName("incomeMultiplier")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 8, Precision = 6)]
  public decimal IncomeMultiplier
  {
    get => incomeMultiplier;
    set => incomeMultiplier = Truncate(value, 6);
  }

  /// <summary>
  /// The value of the INCOME_EXPONENT attribute.
  /// AN INCOME EXPONENT FROM THE CHILD SUPPORT SCHEDULE THAT IS TO BE USED WHEN
  /// INCOME LEVEL EXCEEDS THE HIGHEST GROSS MONTHLY AMOUNT ON THE TABLE FOR
  /// AGES 16-18.
  /// </summary>
  [JsonPropertyName("incomeExponent")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 6, Type = MemberType.Number, Length = 8, Precision = 6)]
  public decimal IncomeExponent
  {
    get => incomeExponent;
    set => incomeExponent = Truncate(value, 6);
  }

  /// <summary>
  /// The value of the NUMBER_OF_CHILDREN_IN_FAMILY attribute.
  /// THE NUMBER OF CHILDREN IN THE FAMILY.
  /// </summary>
  [JsonPropertyName("numberOfChildrenInFamily")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 2)]
  public int NumberOfChildrenInFamily
  {
    get => numberOfChildrenInFamily;
    set => numberOfChildrenInFamily = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
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
  /// Timestamp of creation of the occurrence.
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
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 11, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the CS_GUIDELINE_YEAR attribute.
  /// The year the child support guidelines values are set.  This routinely 
  /// changes approximately every four years. This attribute is stored as a four
  /// character year like 2008, 2012, 2016.   Each time the guidelines change (
  /// every four years or so) the numbers will be entered onto the table along
  /// with the new values.  The existing values for prior years will remain.
  /// </summary>
  [JsonPropertyName("csGuidelineYear")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 4)]
  public int CsGuidelineYear
  {
    get => csGuidelineYear;
    set => csGuidelineYear = value;
  }

  private DateTime? expirationDate;
  private DateTime? effectiveDate;
  private int identifier;
  private int monthlyIncomePovertyLevelInd;
  private decimal incomeMultiplier;
  private decimal incomeExponent;
  private int numberOfChildrenInFamily;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int csGuidelineYear;
}
