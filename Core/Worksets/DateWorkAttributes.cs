// The source file: DATE_WORK_ATTRIBUTES, ID: 372126711, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class DateWorkAttributes: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DateWorkAttributes()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DateWorkAttributes(DateWorkAttributes that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DateWorkAttributes Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DateWorkAttributes that)
  {
    base.Assign(that);
    textDate20Char = that.textDate20Char;
    textDate10Char = that.textDate10Char;
    textDay = that.textDay;
    textMonth = that.textMonth;
    textYear = that.textYear;
    textMonthYear = that.textMonthYear;
    textYearMonth = that.textYearMonth;
    numericalDay = that.numericalDay;
    numericalMonth = that.numericalMonth;
    numericalYear = that.numericalYear;
  }

  /// <summary>Length of the TEXT_DATE_20_CHAR attribute.</summary>
  public const int TextDate20Char_MaxLength = 20;

  /// <summary>
  /// The value of the TEXT_DATE_20_CHAR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = TextDate20Char_MaxLength)]
  public string TextDate20Char
  {
    get => textDate20Char ?? "";
    set => textDate20Char =
      TrimEnd(Substring(value, 1, TextDate20Char_MaxLength));
  }

  /// <summary>
  /// The json value of the TextDate20Char attribute.</summary>
  [JsonPropertyName("textDate20Char")]
  [Computed]
  public string TextDate20Char_Json
  {
    get => NullIf(TextDate20Char, "");
    set => TextDate20Char = value;
  }

  /// <summary>Length of the TEXT_DATE_10_CHAR attribute.</summary>
  public const int TextDate10Char_MaxLength = 10;

  /// <summary>
  /// The value of the TEXT_DATE_10_CHAR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = TextDate10Char_MaxLength)]
  public string TextDate10Char
  {
    get => textDate10Char ?? "";
    set => textDate10Char =
      TrimEnd(Substring(value, 1, TextDate10Char_MaxLength));
  }

  /// <summary>
  /// The json value of the TextDate10Char attribute.</summary>
  [JsonPropertyName("textDate10Char")]
  [Computed]
  public string TextDate10Char_Json
  {
    get => NullIf(TextDate10Char, "");
    set => TextDate10Char = value;
  }

  /// <summary>Length of the TEXT_DAY attribute.</summary>
  public const int TextDay_MaxLength = 2;

  /// <summary>
  /// The value of the TEXT_DAY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = TextDay_MaxLength)]
  public string TextDay
  {
    get => textDay ?? "";
    set => textDay = TrimEnd(Substring(value, 1, TextDay_MaxLength));
  }

  /// <summary>
  /// The json value of the TextDay attribute.</summary>
  [JsonPropertyName("textDay")]
  [Computed]
  public string TextDay_Json
  {
    get => NullIf(TextDay, "");
    set => TextDay = value;
  }

  /// <summary>Length of the TEXT_MONTH attribute.</summary>
  public const int TextMonth_MaxLength = 2;

  /// <summary>
  /// The value of the TEXT_MONTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = TextMonth_MaxLength)]
  public string TextMonth
  {
    get => textMonth ?? "";
    set => textMonth = TrimEnd(Substring(value, 1, TextMonth_MaxLength));
  }

  /// <summary>
  /// The json value of the TextMonth attribute.</summary>
  [JsonPropertyName("textMonth")]
  [Computed]
  public string TextMonth_Json
  {
    get => NullIf(TextMonth, "");
    set => TextMonth = value;
  }

  /// <summary>Length of the TEXT_YEAR attribute.</summary>
  public const int TextYear_MaxLength = 4;

  /// <summary>
  /// The value of the TEXT_YEAR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = TextYear_MaxLength)]
  public string TextYear
  {
    get => textYear ?? "";
    set => textYear = TrimEnd(Substring(value, 1, TextYear_MaxLength));
  }

  /// <summary>
  /// The json value of the TextYear attribute.</summary>
  [JsonPropertyName("textYear")]
  [Computed]
  public string TextYear_Json
  {
    get => NullIf(TextYear, "");
    set => TextYear = value;
  }

  /// <summary>Length of the TEXT_MONTH_YEAR attribute.</summary>
  public const int TextMonthYear_MaxLength = 6;

  /// <summary>
  /// The value of the TEXT_MONTH_YEAR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = TextMonthYear_MaxLength)]
  public string TextMonthYear
  {
    get => textMonthYear ?? "";
    set => textMonthYear =
      TrimEnd(Substring(value, 1, TextMonthYear_MaxLength));
  }

  /// <summary>
  /// The json value of the TextMonthYear attribute.</summary>
  [JsonPropertyName("textMonthYear")]
  [Computed]
  public string TextMonthYear_Json
  {
    get => NullIf(TextMonthYear, "");
    set => TextMonthYear = value;
  }

  /// <summary>Length of the TEXT_YEAR_MONTH attribute.</summary>
  public const int TextYearMonth_MaxLength = 6;

  /// <summary>
  /// The value of the TEXT_YEAR_MONTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = TextYearMonth_MaxLength)]
  public string TextYearMonth
  {
    get => textYearMonth ?? "";
    set => textYearMonth =
      TrimEnd(Substring(value, 1, TextYearMonth_MaxLength));
  }

  /// <summary>
  /// The json value of the TextYearMonth attribute.</summary>
  [JsonPropertyName("textYearMonth")]
  [Computed]
  public string TextYearMonth_Json
  {
    get => NullIf(TextYearMonth, "");
    set => TextYearMonth = value;
  }

  /// <summary>
  /// The value of the NUMERICAL_DAY attribute.
  /// </summary>
  [JsonPropertyName("numericalDay")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 2)]
  public int NumericalDay
  {
    get => numericalDay;
    set => numericalDay = value;
  }

  /// <summary>
  /// The value of the NUMERICAL_MONTH attribute.
  /// </summary>
  [JsonPropertyName("numericalMonth")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 2)]
  [Value(1, 12)]
  public int NumericalMonth
  {
    get => numericalMonth;
    set => numericalMonth = value;
  }

  /// <summary>
  /// The value of the NUMERICAL_YEAR attribute.
  /// </summary>
  [JsonPropertyName("numericalYear")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 4)]
  public int NumericalYear
  {
    get => numericalYear;
    set => numericalYear = value;
  }

  private string textDate20Char;
  private string textDate10Char;
  private string textDay;
  private string textMonth;
  private string textYear;
  private string textMonthYear;
  private string textYearMonth;
  private int numericalDay;
  private int numericalMonth;
  private int numericalYear;
}
