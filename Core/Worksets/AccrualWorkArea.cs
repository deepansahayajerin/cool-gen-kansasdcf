// The source file: ACCRUAL_WORK_AREA, ID: 371972107, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: FINANCE
/// This is a work area used during the accrual process.
/// </summary>
[Serializable]
public partial class AccrualWorkArea: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AccrualWorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AccrualWorkArea(AccrualWorkArea that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AccrualWorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AccrualWorkArea that)
  {
    base.Assign(that);
    currentMonth = that.currentMonth;
    currentYear = that.currentYear;
    processFromDate = that.processFromDate;
    processThruDate = that.processThruDate;
    processThruMonth = that.processThruMonth;
    processThruYear = that.processThruYear;
    currentProcessDate = that.currentProcessDate;
    currentProcessMonth = that.currentProcessMonth;
    currentProcessYear = that.currentProcessYear;
    accrualDiscDate = that.accrualDiscDate;
    accrualDiscMonth = that.accrualDiscMonth;
    accrualDiscYear = that.accrualDiscYear;
    dayOfWeek = that.dayOfWeek;
    dayOfWeekText = that.dayOfWeekText;
    dayOfYear = that.dayOfYear;
    startingSunday = that.startingSunday;
  }

  /// <summary>
  /// The value of the CURRENT MONTH attribute.
  /// </summary>
  [JsonPropertyName("currentMonth")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 2)]
  public int CurrentMonth
  {
    get => currentMonth;
    set => currentMonth = value;
  }

  /// <summary>
  /// The value of the CURRENT YEAR attribute.
  /// </summary>
  [JsonPropertyName("currentYear")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 4)]
  public int CurrentYear
  {
    get => currentYear;
    set => currentYear = value;
  }

  /// <summary>
  /// The value of the PROCESS_FROM_DATE attribute.
  /// </summary>
  [JsonPropertyName("processFromDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? ProcessFromDate
  {
    get => processFromDate;
    set => processFromDate = value;
  }

  /// <summary>
  /// The value of the PROCESS_THRU_DATE attribute.
  /// </summary>
  [JsonPropertyName("processThruDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? ProcessThruDate
  {
    get => processThruDate;
    set => processThruDate = value;
  }

  /// <summary>
  /// The value of the PROCESS_THRU_MONTH attribute.
  /// </summary>
  [JsonPropertyName("processThruMonth")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 2)]
  public int ProcessThruMonth
  {
    get => processThruMonth;
    set => processThruMonth = value;
  }

  /// <summary>
  /// The value of the PROCESS_THRU_YEAR attribute.
  /// </summary>
  [JsonPropertyName("processThruYear")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 4)]
  public int ProcessThruYear
  {
    get => processThruYear;
    set => processThruYear = value;
  }

  /// <summary>
  /// The value of the CURRENT_PROCESS_DATE attribute.
  /// </summary>
  [JsonPropertyName("currentProcessDate")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? CurrentProcessDate
  {
    get => currentProcessDate;
    set => currentProcessDate = value;
  }

  /// <summary>
  /// The value of the CURRENT_PROCESS_MONTH attribute.
  /// </summary>
  [JsonPropertyName("currentProcessMonth")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 2)]
  public int CurrentProcessMonth
  {
    get => currentProcessMonth;
    set => currentProcessMonth = value;
  }

  /// <summary>
  /// The value of the CURRENT_PROCESS_YEAR attribute.
  /// </summary>
  [JsonPropertyName("currentProcessYear")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 4)]
  public int CurrentProcessYear
  {
    get => currentProcessYear;
    set => currentProcessYear = value;
  }

  /// <summary>
  /// The value of the ACCRUAL DISC DATE attribute.
  /// </summary>
  [JsonPropertyName("accrualDiscDate")]
  [Member(Index = 10, Type = MemberType.Date)]
  public DateTime? AccrualDiscDate
  {
    get => accrualDiscDate;
    set => accrualDiscDate = value;
  }

  /// <summary>
  /// The value of the ACCRUAL DISC MONTH attribute.
  /// </summary>
  [JsonPropertyName("accrualDiscMonth")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 2)]
  public int AccrualDiscMonth
  {
    get => accrualDiscMonth;
    set => accrualDiscMonth = value;
  }

  /// <summary>
  /// The value of the ACCRUAL DISC YEAR attribute.
  /// </summary>
  [JsonPropertyName("accrualDiscYear")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 4)]
  public int AccrualDiscYear
  {
    get => accrualDiscYear;
    set => accrualDiscYear = value;
  }

  /// <summary>
  /// The value of the DAY_OF_WEEK attribute.
  /// </summary>
  [JsonPropertyName("dayOfWeek")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 1)]
  public int DayOfWeek
  {
    get => dayOfWeek;
    set => dayOfWeek = value;
  }

  /// <summary>Length of the DAY_OF_WEEK_TEXT attribute.</summary>
  public const int DayOfWeekText_MaxLength = 20;

  /// <summary>
  /// The value of the DAY_OF_WEEK_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = DayOfWeekText_MaxLength)]
  public string DayOfWeekText
  {
    get => dayOfWeekText ?? "";
    set => dayOfWeekText =
      TrimEnd(Substring(value, 1, DayOfWeekText_MaxLength));
  }

  /// <summary>
  /// The json value of the DayOfWeekText attribute.</summary>
  [JsonPropertyName("dayOfWeekText")]
  [Computed]
  public string DayOfWeekText_Json
  {
    get => NullIf(DayOfWeekText, "");
    set => DayOfWeekText = value;
  }

  /// <summary>
  /// The value of the DAY_OF_YEAR attribute.
  /// </summary>
  [JsonPropertyName("dayOfYear")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 3)]
  public int DayOfYear
  {
    get => dayOfYear;
    set => dayOfYear = value;
  }

  /// <summary>
  /// The value of the STARTING_SUNDAY attribute.
  /// </summary>
  [JsonPropertyName("startingSunday")]
  [DefaultValue(0)]
  [Member(Index = 16, Type = MemberType.Number, Length = 3)]
  public int StartingSunday
  {
    get => startingSunday;
    set => startingSunday = value;
  }

  private int currentMonth;
  private int currentYear;
  private DateTime? processFromDate;
  private DateTime? processThruDate;
  private int processThruMonth;
  private int processThruYear;
  private DateTime? currentProcessDate;
  private int currentProcessMonth;
  private int currentProcessYear;
  private DateTime? accrualDiscDate;
  private int accrualDiscMonth;
  private int accrualDiscYear;
  private int dayOfWeek;
  private string dayOfWeekText;
  private int dayOfYear;
  private int startingSunday;
}
