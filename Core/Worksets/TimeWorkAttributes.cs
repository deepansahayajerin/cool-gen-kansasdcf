// The source file: TIME_WORK_ATTRIBUTES, ID: 372728263, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class TimeWorkAttributes: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public TimeWorkAttributes()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public TimeWorkAttributes(TimeWorkAttributes that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new TimeWorkAttributes Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(TimeWorkAttributes that)
  {
    base.Assign(that);
    textTime15Char = that.textTime15Char;
    numericalHours = that.numericalHours;
    numericalMinutes = that.numericalMinutes;
    numericalSeconds = that.numericalSeconds;
    numericalMicroseconds = that.numericalMicroseconds;
  }

  /// <summary>Length of the TEXT_TIME_15_CHAR attribute.</summary>
  public const int TextTime15Char_MaxLength = 15;

  /// <summary>
  /// The value of the TEXT_TIME_15_CHAR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = TextTime15Char_MaxLength)]
  public string TextTime15Char
  {
    get => textTime15Char ?? "";
    set => textTime15Char =
      TrimEnd(Substring(value, 1, TextTime15Char_MaxLength));
  }

  /// <summary>
  /// The json value of the TextTime15Char attribute.</summary>
  [JsonPropertyName("textTime15Char")]
  [Computed]
  public string TextTime15Char_Json
  {
    get => NullIf(TextTime15Char, "");
    set => TextTime15Char = value;
  }

  /// <summary>
  /// The value of the NUMERICAL_HOURS attribute.
  /// </summary>
  [JsonPropertyName("numericalHours")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int NumericalHours
  {
    get => numericalHours;
    set => numericalHours = value;
  }

  /// <summary>
  /// The value of the NUMERICAL_MINUTES attribute.
  /// </summary>
  [JsonPropertyName("numericalMinutes")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 2)]
  public int NumericalMinutes
  {
    get => numericalMinutes;
    set => numericalMinutes = value;
  }

  /// <summary>
  /// The value of the NUMERICAL_SECONDS attribute.
  /// </summary>
  [JsonPropertyName("numericalSeconds")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 2)]
  public int NumericalSeconds
  {
    get => numericalSeconds;
    set => numericalSeconds = value;
  }

  /// <summary>
  /// The value of the NUMERICAL_MICROSECONDS attribute.
  /// </summary>
  [JsonPropertyName("numericalMicroseconds")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 6)]
  public int NumericalMicroseconds
  {
    get => numericalMicroseconds;
    set => numericalMicroseconds = value;
  }

  private string textTime15Char;
  private int numericalHours;
  private int numericalMinutes;
  private int numericalSeconds;
  private int numericalMicroseconds;
}
