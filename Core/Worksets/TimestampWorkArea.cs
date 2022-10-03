// The source file: TIMESTAMP_WORK_AREA, ID: 371726897, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: FINANCE
/// This work set is used to aid in converting date and time entry to time 
/// stamps.
/// </summary>
[Serializable]
public partial class TimestampWorkArea: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public TimestampWorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public TimestampWorkArea(TimestampWorkArea that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new TimestampWorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(TimestampWorkArea that)
  {
    base.Assign(that);
    timestampText = that.timestampText;
    yearText = that.yearText;
    monthText = that.monthText;
    dayText = that.dayText;
    hourText = that.hourText;
    minText = that.minText;
  }

  /// <summary>Length of the TIMESTAMP_TEXT attribute.</summary>
  public const int TimestampText_MaxLength = 20;

  /// <summary>
  /// The value of the TIMESTAMP_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = TimestampText_MaxLength)]
  public string TimestampText
  {
    get => timestampText ?? "";
    set => timestampText =
      TrimEnd(Substring(value, 1, TimestampText_MaxLength));
  }

  /// <summary>
  /// The json value of the TimestampText attribute.</summary>
  [JsonPropertyName("timestampText")]
  [Computed]
  public string TimestampText_Json
  {
    get => NullIf(TimestampText, "");
    set => TimestampText = value;
  }

  /// <summary>Length of the YEAR_TEXT attribute.</summary>
  public const int YearText_MaxLength = 4;

  /// <summary>
  /// The value of the YEAR_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = YearText_MaxLength)]
  public string YearText
  {
    get => yearText ?? "";
    set => yearText = TrimEnd(Substring(value, 1, YearText_MaxLength));
  }

  /// <summary>
  /// The json value of the YearText attribute.</summary>
  [JsonPropertyName("yearText")]
  [Computed]
  public string YearText_Json
  {
    get => NullIf(YearText, "");
    set => YearText = value;
  }

  /// <summary>Length of the MONTH_TEXT attribute.</summary>
  public const int MonthText_MaxLength = 2;

  /// <summary>
  /// The value of the MONTH_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = MonthText_MaxLength)]
  public string MonthText
  {
    get => monthText ?? "";
    set => monthText = TrimEnd(Substring(value, 1, MonthText_MaxLength));
  }

  /// <summary>
  /// The json value of the MonthText attribute.</summary>
  [JsonPropertyName("monthText")]
  [Computed]
  public string MonthText_Json
  {
    get => NullIf(MonthText, "");
    set => MonthText = value;
  }

  /// <summary>Length of the DAY_TEXT attribute.</summary>
  public const int DayText_MaxLength = 2;

  /// <summary>
  /// The value of the DAY_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = DayText_MaxLength)]
  public string DayText
  {
    get => dayText ?? "";
    set => dayText = TrimEnd(Substring(value, 1, DayText_MaxLength));
  }

  /// <summary>
  /// The json value of the DayText attribute.</summary>
  [JsonPropertyName("dayText")]
  [Computed]
  public string DayText_Json
  {
    get => NullIf(DayText, "");
    set => DayText = value;
  }

  /// <summary>Length of the HOUR_TEXT attribute.</summary>
  public const int HourText_MaxLength = 2;

  /// <summary>
  /// The value of the HOUR_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = HourText_MaxLength)]
  public string HourText
  {
    get => hourText ?? "";
    set => hourText = TrimEnd(Substring(value, 1, HourText_MaxLength));
  }

  /// <summary>
  /// The json value of the HourText attribute.</summary>
  [JsonPropertyName("hourText")]
  [Computed]
  public string HourText_Json
  {
    get => NullIf(HourText, "");
    set => HourText = value;
  }

  /// <summary>Length of the MIN_TEXT attribute.</summary>
  public const int MinText_MaxLength = 2;

  /// <summary>
  /// The value of the MIN_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = MinText_MaxLength)]
  public string MinText
  {
    get => minText ?? "";
    set => minText = TrimEnd(Substring(value, 1, MinText_MaxLength));
  }

  /// <summary>
  /// The json value of the MinText attribute.</summary>
  [JsonPropertyName("minText")]
  [Computed]
  public string MinText_Json
  {
    get => NullIf(MinText, "");
    set => MinText = value;
  }

  private string timestampText;
  private string yearText;
  private string monthText;
  private string dayText;
  private string hourText;
  private string minText;
}
