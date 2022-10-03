// The source file: DATE_VALIDATION, ID: 371972114, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// Contains work attributes to hold various information relating to valid 
/// dates.
/// </summary>
[Serializable]
public partial class DateValidation: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DateValidation()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DateValidation(DateValidation that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DateValidation Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DateValidation that)
  {
    base.Assign(that);
    month = that.month;
    day = that.day;
    year = that.year;
    maximumNumberOfDaysInMonth = that.maximumNumberOfDaysInMonth;
  }

  /// <summary>
  /// The value of the MONTH attribute.
  /// </summary>
  [JsonPropertyName("month")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 2)]
  public int Month
  {
    get => month;
    set => month = value;
  }

  /// <summary>
  /// The value of the DAY attribute.
  /// A numeric value that represents the day of a month.
  /// </summary>
  [JsonPropertyName("day")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int Day
  {
    get => day;
    set => day = value;
  }

  /// <summary>
  /// The value of the YEAR attribute.
  /// A numeric value that represents a year.
  /// </summary>
  [JsonPropertyName("year")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 4)]
  public int Year
  {
    get => year;
    set => year = value;
  }

  /// <summary>
  /// The value of the MAXIMUM_NUMBER_OF_DAYS_IN_MONTH attribute.
  /// Represents the maximum number of days for a specific month.
  /// </summary>
  [JsonPropertyName("maximumNumberOfDaysInMonth")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 2)]
  public int MaximumNumberOfDaysInMonth
  {
    get => maximumNumberOfDaysInMonth;
    set => maximumNumberOfDaysInMonth = value;
  }

  private int month;
  private int day;
  private int year;
  private int maximumNumberOfDaysInMonth;
}
