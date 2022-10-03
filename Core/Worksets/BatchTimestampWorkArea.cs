// The source file: BATCH_TIMESTAMP_WORK_AREA, ID: 371735300, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This work set is used to convert a timestamp
/// into its components.
/// </summary>
[Serializable]
public partial class BatchTimestampWorkArea: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public BatchTimestampWorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public BatchTimestampWorkArea(BatchTimestampWorkArea that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new BatchTimestampWorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(BatchTimestampWorkArea that)
  {
    base.Assign(that);
    iefTimestamp = that.iefTimestamp;
    textTimestamp = that.textTimestamp;
    textDate = that.textDate;
    textDateYyyy = that.textDateYyyy;
    textDateMm = that.textDateMm;
    testDateDd = that.testDateDd;
    textTime = that.textTime;
    testTimeHh = that.testTimeHh;
    textTimeMm = that.textTimeMm;
    textTimeSs = that.textTimeSs;
    textMillisecond = that.textMillisecond;
    numDate = that.numDate;
    numTime = that.numTime;
    numMillisecond = that.numMillisecond;
  }

  /// <summary>
  /// The value of the IEF_TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("iefTimestamp")]
  [Member(Index = 1, Type = MemberType.Timestamp)]
  public DateTime? IefTimestamp
  {
    get => iefTimestamp;
    set => iefTimestamp = value;
  }

  /// <summary>Length of the TEXT_TIMESTAMP attribute.</summary>
  public const int TextTimestamp_MaxLength = 26;

  /// <summary>
  /// The value of the TEXT_TIMESTAMP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = TextTimestamp_MaxLength)]
  public string TextTimestamp
  {
    get => textTimestamp ?? "";
    set => textTimestamp =
      TrimEnd(Substring(value, 1, TextTimestamp_MaxLength));
  }

  /// <summary>
  /// The json value of the TextTimestamp attribute.</summary>
  [JsonPropertyName("textTimestamp")]
  [Computed]
  public string TextTimestamp_Json
  {
    get => NullIf(TextTimestamp, "");
    set => TextTimestamp = value;
  }

  /// <summary>Length of the TEXT_DATE attribute.</summary>
  public const int TextDate_MaxLength = 8;

  /// <summary>
  /// The value of the TEXT_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = TextDate_MaxLength)]
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

  /// <summary>Length of the TEXT_DATE_YYYY attribute.</summary>
  public const int TextDateYyyy_MaxLength = 4;

  /// <summary>
  /// The value of the TEXT_DATE_YYYY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = TextDateYyyy_MaxLength)]
  public string TextDateYyyy
  {
    get => textDateYyyy ?? "";
    set => textDateYyyy = TrimEnd(Substring(value, 1, TextDateYyyy_MaxLength));
  }

  /// <summary>
  /// The json value of the TextDateYyyy attribute.</summary>
  [JsonPropertyName("textDateYyyy")]
  [Computed]
  public string TextDateYyyy_Json
  {
    get => NullIf(TextDateYyyy, "");
    set => TextDateYyyy = value;
  }

  /// <summary>Length of the TEXT_DATE_MM attribute.</summary>
  public const int TextDateMm_MaxLength = 2;

  /// <summary>
  /// The value of the TEXT_DATE_MM attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = TextDateMm_MaxLength)]
  public string TextDateMm
  {
    get => textDateMm ?? "";
    set => textDateMm = TrimEnd(Substring(value, 1, TextDateMm_MaxLength));
  }

  /// <summary>
  /// The json value of the TextDateMm attribute.</summary>
  [JsonPropertyName("textDateMm")]
  [Computed]
  public string TextDateMm_Json
  {
    get => NullIf(TextDateMm, "");
    set => TextDateMm = value;
  }

  /// <summary>Length of the TEST_DATE_DD attribute.</summary>
  public const int TestDateDd_MaxLength = 2;

  /// <summary>
  /// The value of the TEST_DATE_DD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = TestDateDd_MaxLength)]
  public string TestDateDd
  {
    get => testDateDd ?? "";
    set => testDateDd = TrimEnd(Substring(value, 1, TestDateDd_MaxLength));
  }

  /// <summary>
  /// The json value of the TestDateDd attribute.</summary>
  [JsonPropertyName("testDateDd")]
  [Computed]
  public string TestDateDd_Json
  {
    get => NullIf(TestDateDd, "");
    set => TestDateDd = value;
  }

  /// <summary>Length of the TEXT_TIME attribute.</summary>
  public const int TextTime_MaxLength = 6;

  /// <summary>
  /// The value of the TEXT_TIME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = TextTime_MaxLength)]
  public string TextTime
  {
    get => textTime ?? "";
    set => textTime = TrimEnd(Substring(value, 1, TextTime_MaxLength));
  }

  /// <summary>
  /// The json value of the TextTime attribute.</summary>
  [JsonPropertyName("textTime")]
  [Computed]
  public string TextTime_Json
  {
    get => NullIf(TextTime, "");
    set => TextTime = value;
  }

  /// <summary>Length of the TEST_TIME_HH attribute.</summary>
  public const int TestTimeHh_MaxLength = 2;

  /// <summary>
  /// The value of the TEST_TIME_HH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = TestTimeHh_MaxLength)]
  public string TestTimeHh
  {
    get => testTimeHh ?? "";
    set => testTimeHh = TrimEnd(Substring(value, 1, TestTimeHh_MaxLength));
  }

  /// <summary>
  /// The json value of the TestTimeHh attribute.</summary>
  [JsonPropertyName("testTimeHh")]
  [Computed]
  public string TestTimeHh_Json
  {
    get => NullIf(TestTimeHh, "");
    set => TestTimeHh = value;
  }

  /// <summary>Length of the TEXT_TIME_MM attribute.</summary>
  public const int TextTimeMm_MaxLength = 2;

  /// <summary>
  /// The value of the TEXT_TIME_MM attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = TextTimeMm_MaxLength)]
  public string TextTimeMm
  {
    get => textTimeMm ?? "";
    set => textTimeMm = TrimEnd(Substring(value, 1, TextTimeMm_MaxLength));
  }

  /// <summary>
  /// The json value of the TextTimeMm attribute.</summary>
  [JsonPropertyName("textTimeMm")]
  [Computed]
  public string TextTimeMm_Json
  {
    get => NullIf(TextTimeMm, "");
    set => TextTimeMm = value;
  }

  /// <summary>Length of the TEXT_TIME_SS attribute.</summary>
  public const int TextTimeSs_MaxLength = 2;

  /// <summary>
  /// The value of the TEXT_TIME_SS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = TextTimeSs_MaxLength)]
  public string TextTimeSs
  {
    get => textTimeSs ?? "";
    set => textTimeSs = TrimEnd(Substring(value, 1, TextTimeSs_MaxLength));
  }

  /// <summary>
  /// The json value of the TextTimeSs attribute.</summary>
  [JsonPropertyName("textTimeSs")]
  [Computed]
  public string TextTimeSs_Json
  {
    get => NullIf(TextTimeSs, "");
    set => TextTimeSs = value;
  }

  /// <summary>Length of the TEXT_MILLISECOND attribute.</summary>
  public const int TextMillisecond_MaxLength = 6;

  /// <summary>
  /// The value of the TEXT_MILLISECOND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = TextMillisecond_MaxLength)
    ]
  public string TextMillisecond
  {
    get => textMillisecond ?? "";
    set => textMillisecond =
      TrimEnd(Substring(value, 1, TextMillisecond_MaxLength));
  }

  /// <summary>
  /// The json value of the TextMillisecond attribute.</summary>
  [JsonPropertyName("textMillisecond")]
  [Computed]
  public string TextMillisecond_Json
  {
    get => NullIf(TextMillisecond, "");
    set => TextMillisecond = value;
  }

  /// <summary>
  /// The value of the NUM_DATE attribute.
  /// </summary>
  [JsonPropertyName("numDate")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 8)]
  public int NumDate
  {
    get => numDate;
    set => numDate = value;
  }

  /// <summary>
  /// The value of the NUM_TIME attribute.
  /// </summary>
  [JsonPropertyName("numTime")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 6)]
  public int NumTime
  {
    get => numTime;
    set => numTime = value;
  }

  /// <summary>
  /// The value of the NUM_MILLISECOND attribute.
  /// </summary>
  [JsonPropertyName("numMillisecond")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 6)]
  public int NumMillisecond
  {
    get => numMillisecond;
    set => numMillisecond = value;
  }

  private DateTime? iefTimestamp;
  private string textTimestamp;
  private string textDate;
  private string textDateYyyy;
  private string textDateMm;
  private string testDateDd;
  private string textTime;
  private string testTimeHh;
  private string textTimeMm;
  private string textTimeSs;
  private string textMillisecond;
  private int numDate;
  private int numTime;
  private int numMillisecond;
}
