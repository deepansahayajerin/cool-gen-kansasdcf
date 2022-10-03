// The source file: FREQUENCY_WORK_SET, ID: 372002448, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This work set is for display of frequency attributes.
/// </summary>
[Serializable]
public partial class FrequencyWorkSet: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FrequencyWorkSet()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FrequencyWorkSet(FrequencyWorkSet that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FrequencyWorkSet Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FrequencyWorkSet that)
  {
    base.Assign(that);
    frequencyCode = that.frequencyCode;
    frequencyDescription = that.frequencyDescription;
    day1 = that.day1;
    day2 = that.day2;
    dow = that.dow;
  }

  /// <summary>Length of the FREQUENCY_CODE attribute.</summary>
  public const int FrequencyCode_MaxLength = 2;

  /// <summary>
  /// The value of the FREQUENCY_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = FrequencyCode_MaxLength)]
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

  /// <summary>Length of the FREQUENCY_DESCRIPTION attribute.</summary>
  public const int FrequencyDescription_MaxLength = 13;

  /// <summary>
  /// The value of the FREQUENCY_DESCRIPTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = FrequencyDescription_MaxLength)]
  public string FrequencyDescription
  {
    get => frequencyDescription ?? "";
    set => frequencyDescription =
      TrimEnd(Substring(value, 1, FrequencyDescription_MaxLength));
  }

  /// <summary>
  /// The json value of the FrequencyDescription attribute.</summary>
  [JsonPropertyName("frequencyDescription")]
  [Computed]
  public string FrequencyDescription_Json
  {
    get => NullIf(FrequencyDescription, "");
    set => FrequencyDescription = value;
  }

  /// <summary>
  /// The value of the DAY1 attribute.
  /// </summary>
  [JsonPropertyName("day1")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 2)]
  public int Day1
  {
    get => day1;
    set => day1 = value;
  }

  /// <summary>
  /// The value of the DAY2 attribute.
  /// </summary>
  [JsonPropertyName("day2")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 2)]
  public int Day2
  {
    get => day2;
    set => day2 = value;
  }

  /// <summary>
  /// The value of the DOW attribute.
  /// day of week
  /// </summary>
  [JsonPropertyName("dow")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 1)]
  public int Dow
  {
    get => dow;
    set => dow = value;
  }

  private string frequencyCode;
  private string frequencyDescription;
  private int day1;
  private int day2;
  private int dow;
}
