// The source file: TEXT_WORK_AREA, ID: 371424130, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: SRVPLAN
/// </summary>
[Serializable]
public partial class TextWorkArea: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public TextWorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public TextWorkArea(TextWorkArea that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new TextWorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(TextWorkArea that)
  {
    base.Assign(that);
    text1 = that.text1;
    text2 = that.text2;
    text4 = that.text4;
    text8 = that.text8;
    text10 = that.text10;
    text12 = that.text12;
    text30 = that.text30;
  }

  /// <summary>Length of the TEXT_1 attribute.</summary>
  public const int Text1_MaxLength = 1;

  /// <summary>
  /// The value of the TEXT_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Text1_MaxLength)]
  public string Text1
  {
    get => text1 ?? "";
    set => text1 = TrimEnd(Substring(value, 1, Text1_MaxLength));
  }

  /// <summary>
  /// The json value of the Text1 attribute.</summary>
  [JsonPropertyName("text1")]
  [Computed]
  public string Text1_Json
  {
    get => NullIf(Text1, "");
    set => Text1 = value;
  }

  /// <summary>Length of the TEXT_2 attribute.</summary>
  public const int Text2_MaxLength = 2;

  /// <summary>
  /// The value of the TEXT_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Text2_MaxLength)]
  public string Text2
  {
    get => text2 ?? "";
    set => text2 = TrimEnd(Substring(value, 1, Text2_MaxLength));
  }

  /// <summary>
  /// The json value of the Text2 attribute.</summary>
  [JsonPropertyName("text2")]
  [Computed]
  public string Text2_Json
  {
    get => NullIf(Text2, "");
    set => Text2 = value;
  }

  /// <summary>Length of the TEXT_4 attribute.</summary>
  public const int Text4_MaxLength = 4;

  /// <summary>
  /// The value of the TEXT_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Text4_MaxLength)]
  public string Text4
  {
    get => text4 ?? "";
    set => text4 = TrimEnd(Substring(value, 1, Text4_MaxLength));
  }

  /// <summary>
  /// The json value of the Text4 attribute.</summary>
  [JsonPropertyName("text4")]
  [Computed]
  public string Text4_Json
  {
    get => NullIf(Text4, "");
    set => Text4 = value;
  }

  /// <summary>Length of the TEXT_8 attribute.</summary>
  public const int Text8_MaxLength = 8;

  /// <summary>
  /// The value of the TEXT_8 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Text8_MaxLength)]
  public string Text8
  {
    get => text8 ?? "";
    set => text8 = TrimEnd(Substring(value, 1, Text8_MaxLength));
  }

  /// <summary>
  /// The json value of the Text8 attribute.</summary>
  [JsonPropertyName("text8")]
  [Computed]
  public string Text8_Json
  {
    get => NullIf(Text8, "");
    set => Text8 = value;
  }

  /// <summary>Length of the TEXT_10 attribute.</summary>
  public const int Text10_MaxLength = 10;

  /// <summary>
  /// The value of the TEXT_10 attribute.
  /// RESP: SRVPLAN
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = Text10_MaxLength)]
  public string Text10
  {
    get => text10 ?? "";
    set => text10 = TrimEnd(Substring(value, 1, Text10_MaxLength));
  }

  /// <summary>
  /// The json value of the Text10 attribute.</summary>
  [JsonPropertyName("text10")]
  [Computed]
  public string Text10_Json
  {
    get => NullIf(Text10, "");
    set => Text10 = value;
  }

  /// <summary>Length of the TEXT_12 attribute.</summary>
  public const int Text12_MaxLength = 12;

  /// <summary>
  /// The value of the TEXT_12 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Text12_MaxLength)]
  public string Text12
  {
    get => text12 ?? "";
    set => text12 = TrimEnd(Substring(value, 1, Text12_MaxLength));
  }

  /// <summary>
  /// The json value of the Text12 attribute.</summary>
  [JsonPropertyName("text12")]
  [Computed]
  public string Text12_Json
  {
    get => NullIf(Text12, "");
    set => Text12 = value;
  }

  /// <summary>Length of the TEXT_30 attribute.</summary>
  public const int Text30_MaxLength = 30;

  /// <summary>
  /// The value of the TEXT_30 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Text30_MaxLength)]
  public string Text30
  {
    get => text30 ?? "";
    set => text30 = TrimEnd(Substring(value, 1, Text30_MaxLength));
  }

  /// <summary>
  /// The json value of the Text30 attribute.</summary>
  [JsonPropertyName("text30")]
  [Computed]
  public string Text30_Json
  {
    get => NullIf(Text30, "");
    set => Text30 = value;
  }

  private string text1;
  private string text2;
  private string text4;
  private string text8;
  private string text10;
  private string text12;
  private string text30;
}
