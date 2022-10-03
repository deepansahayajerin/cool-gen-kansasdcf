// The source file: DATE_WORK_AREA, ID: 371423562, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class DateWorkArea: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DateWorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DateWorkArea(DateWorkArea that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DateWorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DateWorkArea that)
  {
    base.Assign(that);
    textDate = that.textDate;
    date = that.date;
    time = that.time;
    timestamp = that.timestamp;
    yearMonth = that.yearMonth;
    day = that.day;
    month = that.month;
    year = that.year;
  }

  /// <summary>Length of the TEXT_DATE attribute.</summary>
  public const int TextDate_MaxLength = 8;

  /// <summary>
  /// The value of the TEXT_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = TextDate_MaxLength)]
  public string TextDate
  {
    get => textDate ?? "";
    set => textDate = TrimEnd(Substring(value, 1, TextDate_MaxLength));
  }

  /// <summary>
  /// The json value of the TextDate attribute.</summary>
  [JsonPropertyName("textDate")]
  [Computed]
  public string TextDate_Json
  {
    get => NullIf(TextDate, "");
    set => TextDate = value;
  }

  /// <summary>
  /// The value of the DATE attribute.
  /// </summary>
  [JsonPropertyName("date")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? Date
  {
    get => date;
    set => date = value;
  }

  /// <summary>
  /// The value of the TIME attribute.
  /// </summary>
  [JsonPropertyName("time")]
  [Member(Index = 3, Type = MemberType.Time)]
  public TimeSpan Time
  {
    get => time;
    set => time = value;
  }

  /// <summary>
  /// The value of the TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("timestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? Timestamp
  {
    get => timestamp;
    set => timestamp = value;
  }

  /// <summary>
  /// The value of the YEAR_MONTH attribute.
  /// This attribute contains a year and month.  It is used for monthly 
  /// processing such as passthrus and recaptures.
  /// </summary>
  [JsonPropertyName("yearMonth")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 6)]
  public int YearMonth
  {
    get => yearMonth;
    set => yearMonth = value;
  }

  /// <summary>
  /// The value of the DAY attribute.
  /// A numeric value that represents the day of a month.
  /// </summary>
  [JsonPropertyName("day")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 2)]
  public int Day
  {
    get => day;
    set => day = value;
  }

  /// <summary>
  /// The value of the MONTH attribute.
  /// </summary>
  [JsonPropertyName("month")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 2)]
  public int Month
  {
    get => month;
    set => month = value;
  }

  /// <summary>
  /// The value of the YEAR attribute.
  /// A numeric value that represents a year.
  /// </summary>
  [JsonPropertyName("year")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 4)]
  public int Year
  {
    get => year;
    set => year = value;
  }

  private string textDate;
  private DateTime? date;
  private TimeSpan time;
  private DateTime? timestamp;
  private int yearMonth;
  private int day;
  private int month;
  private int year;
}
