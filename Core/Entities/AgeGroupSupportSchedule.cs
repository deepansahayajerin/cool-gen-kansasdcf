// The source file: AGE_GROUP_SUPPORT_SCHEDULE, ID: 371430525, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// Child support schedule that projects child support per child for a given age
/// range according to the combined gross monthly income of both parents.  This
/// does not include higher income levels.
/// Age Groups are 0-6, 7-15, &amp; 16-18 years. You can determine the age of a 
/// child by using the age on the child's nearest birthdate.
/// </summary>
[Serializable]
public partial class AgeGroupSupportSchedule: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AgeGroupSupportSchedule()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AgeGroupSupportSchedule(AgeGroupSupportSchedule that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AgeGroupSupportSchedule Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AgeGroupSupportSchedule that)
  {
    base.Assign(that);
    maximumAgeInARange = that.maximumAgeInARange;
    ageGroupFactor = that.ageGroupFactor;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    cssIdentifier = that.cssIdentifier;
    cssGuidelineYr = that.cssGuidelineYr;
  }

  /// <summary>
  /// The value of the MAXIMUM_AGE_IN_A_RANGE attribute.
  /// 2-digit number indicating the MAXIMUM age of a child within different age 
  /// groups.
  /// </summary>
  [JsonPropertyName("maximumAgeInARange")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 2)]
  public int MaximumAgeInARange
  {
    get => maximumAgeInARange;
    set => maximumAgeInARange = value;
  }

  /// <summary>
  /// The value of the AGE_GROUP_FACTOR attribute.
  /// A FACTOR FROM THE CHILD SUPPORT SCHEDULE THAT IS TO BE USED WHEN INCOME 
  /// LEVEL EXCEEDS THE HIGHEST AMT ON THE TABLE FOR AGES 7-15 and 0-6.
  /// </summary>
  [JsonPropertyName("ageGroupFactor")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 8, Precision = 6)]
  public decimal AgeGroupFactor
  {
    get => ageGroupFactor;
    set => ageGroupFactor = Truncate(value, 6);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 4, Type = MemberType.Timestamp)]
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
  [Member(Index = 5, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Sequence Number to determine a unique record.
  /// </summary>
  [JsonPropertyName("cssIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 2)]
  public int CssIdentifier
  {
    get => cssIdentifier;
    set => cssIdentifier = value;
  }

  /// <summary>
  /// The value of the CS_GUIDELINE_YEAR attribute.
  /// The year the child support guidelines values are set.  This routinely 
  /// changes approximately every four years. This attribute is stored as a four
  /// character year like 2008, 2012, 2016.   Each time the guidelines change (
  /// every four years or so) the numbers will be entered onto the table along
  /// with the new values.  The existing values for prior years will remain.
  /// </summary>
  [JsonPropertyName("cssGuidelineYr")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 4)]
  public int CssGuidelineYr
  {
    get => cssGuidelineYr;
    set => cssGuidelineYr = value;
  }

  private int maximumAgeInARange;
  private decimal ageGroupFactor;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int cssIdentifier;
  private int cssGuidelineYr;
}
