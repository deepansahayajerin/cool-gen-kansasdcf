// The source file: BATCH_CONVERT_NUM_TO_TEXT, ID: 371822101, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class BatchConvertNumToText: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public BatchConvertNumToText()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public BatchConvertNumToText(BatchConvertNumToText that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new BatchConvertNumToText Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(BatchConvertNumToText that)
  {
    base.Assign(that);
    currency = that.currency;
    number9 = that.number9;
    number15 = that.number15;
    textNumber9 = that.textNumber9;
    textNumber15 = that.textNumber15;
    textNumber16 = that.textNumber16;
  }

  /// <summary>
  /// The value of the CURRENCY attribute.
  /// </summary>
  [JsonPropertyName("currency")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 1, Type = MemberType.Number, Length = 15, Precision = 2)]
  public decimal Currency
  {
    get => currency;
    set => currency = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NUMBER_9 attribute.
  /// </summary>
  [JsonPropertyName("number9")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int Number9
  {
    get => number9;
    set => number9 = value;
  }

  /// <summary>
  /// The value of the NUMBER_15 attribute.
  /// </summary>
  [JsonPropertyName("number15")]
  [DefaultValue(0L)]
  [Member(Index = 3, Type = MemberType.Number, Length = 15)]
  public long Number15
  {
    get => number15;
    set => number15 = value;
  }

  /// <summary>Length of the TEXT_NUMBER_9 attribute.</summary>
  public const int TextNumber9_MaxLength = 9;

  /// <summary>
  /// The value of the TEXT_NUMBER_9 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = TextNumber9_MaxLength)]
  public string TextNumber9
  {
    get => textNumber9 ?? "";
    set => textNumber9 = TrimEnd(Substring(value, 1, TextNumber9_MaxLength));
  }

  /// <summary>
  /// The json value of the TextNumber9 attribute.</summary>
  [JsonPropertyName("textNumber9")]
  [Computed]
  public string TextNumber9_Json
  {
    get => NullIf(TextNumber9, "");
    set => TextNumber9 = value;
  }

  /// <summary>Length of the TEXT_NUMBER_15 attribute.</summary>
  public const int TextNumber15_MaxLength = 15;

  /// <summary>
  /// The value of the TEXT_NUMBER_15 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = TextNumber15_MaxLength)]
  public string TextNumber15
  {
    get => textNumber15 ?? "";
    set => textNumber15 = TrimEnd(Substring(value, 1, TextNumber15_MaxLength));
  }

  /// <summary>
  /// The json value of the TextNumber15 attribute.</summary>
  [JsonPropertyName("textNumber15")]
  [Computed]
  public string TextNumber15_Json
  {
    get => NullIf(TextNumber15, "");
    set => TextNumber15 = value;
  }

  /// <summary>Length of the TEXT_NUMBER_16 attribute.</summary>
  public const int TextNumber16_MaxLength = 16;

  /// <summary>
  /// The value of the TEXT_NUMBER_16 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = TextNumber16_MaxLength)]
  public string TextNumber16
  {
    get => textNumber16 ?? "";
    set => textNumber16 = TrimEnd(Substring(value, 1, TextNumber16_MaxLength));
  }

  /// <summary>
  /// The json value of the TextNumber16 attribute.</summary>
  [JsonPropertyName("textNumber16")]
  [Computed]
  public string TextNumber16_Json
  {
    get => NullIf(TextNumber16, "");
    set => TextNumber16 = value;
  }

  private decimal currency;
  private int number9;
  private long number15;
  private string textNumber9;
  private string textNumber15;
  private string textNumber16;
}
