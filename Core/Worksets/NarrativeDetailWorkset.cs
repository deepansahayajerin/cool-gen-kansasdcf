// The source file: NARRATIVE_DETAIL_WORKSET, ID: 370974521, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class NarrativeDetailWorkset: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NarrativeDetailWorkset()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NarrativeDetailWorkset(NarrativeDetailWorkset that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NarrativeDetailWorkset Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NarrativeDetailWorkset that)
  {
    base.Assign(that);
    text10 = that.text10;
    text11 = that.text11;
    text13 = that.text13;
    text15 = that.text15;
    text17 = that.text17;
    text21 = that.text21;
    text22 = that.text22;
    text24 = that.text24;
    text26 = that.text26;
    text28 = that.text28;
  }

  /// <summary>Length of the TEXT_10 attribute.</summary>
  public const int Text10_MaxLength = 10;

  /// <summary>
  /// The value of the TEXT_10 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Text10_MaxLength)]
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

  /// <summary>Length of the TEXT_11 attribute.</summary>
  public const int Text11_MaxLength = 11;

  /// <summary>
  /// The value of the TEXT_11 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Text11_MaxLength)]
  public string Text11
  {
    get => text11 ?? "";
    set => text11 = TrimEnd(Substring(value, 1, Text11_MaxLength));
  }

  /// <summary>
  /// The json value of the Text11 attribute.</summary>
  [JsonPropertyName("text11")]
  [Computed]
  public string Text11_Json
  {
    get => NullIf(Text11, "");
    set => Text11 = value;
  }

  /// <summary>Length of the TEXT_13 attribute.</summary>
  public const int Text13_MaxLength = 13;

  /// <summary>
  /// The value of the TEXT_13 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Text13_MaxLength)]
  public string Text13
  {
    get => text13 ?? "";
    set => text13 = TrimEnd(Substring(value, 1, Text13_MaxLength));
  }

  /// <summary>
  /// The json value of the Text13 attribute.</summary>
  [JsonPropertyName("text13")]
  [Computed]
  public string Text13_Json
  {
    get => NullIf(Text13, "");
    set => Text13 = value;
  }

  /// <summary>Length of the TEXT_15 attribute.</summary>
  public const int Text15_MaxLength = 15;

  /// <summary>
  /// The value of the TEXT_15 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Text15_MaxLength)]
  public string Text15
  {
    get => text15 ?? "";
    set => text15 = TrimEnd(Substring(value, 1, Text15_MaxLength));
  }

  /// <summary>
  /// The json value of the Text15 attribute.</summary>
  [JsonPropertyName("text15")]
  [Computed]
  public string Text15_Json
  {
    get => NullIf(Text15, "");
    set => Text15 = value;
  }

  /// <summary>Length of the TEXT_17 attribute.</summary>
  public const int Text17_MaxLength = 17;

  /// <summary>
  /// The value of the TEXT_17 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = Text17_MaxLength)]
  public string Text17
  {
    get => text17 ?? "";
    set => text17 = TrimEnd(Substring(value, 1, Text17_MaxLength));
  }

  /// <summary>
  /// The json value of the Text17 attribute.</summary>
  [JsonPropertyName("text17")]
  [Computed]
  public string Text17_Json
  {
    get => NullIf(Text17, "");
    set => Text17 = value;
  }

  /// <summary>Length of the TEXT_21 attribute.</summary>
  public const int Text21_MaxLength = 21;

  /// <summary>
  /// The value of the TEXT_21 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Text21_MaxLength)]
  public string Text21
  {
    get => text21 ?? "";
    set => text21 = TrimEnd(Substring(value, 1, Text21_MaxLength));
  }

  /// <summary>
  /// The json value of the Text21 attribute.</summary>
  [JsonPropertyName("text21")]
  [Computed]
  public string Text21_Json
  {
    get => NullIf(Text21, "");
    set => Text21 = value;
  }

  /// <summary>Length of the TEXT_22 attribute.</summary>
  public const int Text22_MaxLength = 22;

  /// <summary>
  /// The value of the TEXT_22 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Text22_MaxLength)]
  public string Text22
  {
    get => text22 ?? "";
    set => text22 = TrimEnd(Substring(value, 1, Text22_MaxLength));
  }

  /// <summary>
  /// The json value of the Text22 attribute.</summary>
  [JsonPropertyName("text22")]
  [Computed]
  public string Text22_Json
  {
    get => NullIf(Text22, "");
    set => Text22 = value;
  }

  /// <summary>Length of the TEXT_24 attribute.</summary>
  public const int Text24_MaxLength = 24;

  /// <summary>
  /// The value of the TEXT_24 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = Text24_MaxLength)]
  public string Text24
  {
    get => text24 ?? "";
    set => text24 = TrimEnd(Substring(value, 1, Text24_MaxLength));
  }

  /// <summary>
  /// The json value of the Text24 attribute.</summary>
  [JsonPropertyName("text24")]
  [Computed]
  public string Text24_Json
  {
    get => NullIf(Text24, "");
    set => Text24 = value;
  }

  /// <summary>Length of the TEXT_26 attribute.</summary>
  public const int Text26_MaxLength = 26;

  /// <summary>
  /// The value of the TEXT_26 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = Text26_MaxLength)]
  public string Text26
  {
    get => text26 ?? "";
    set => text26 = TrimEnd(Substring(value, 1, Text26_MaxLength));
  }

  /// <summary>
  /// The json value of the Text26 attribute.</summary>
  [JsonPropertyName("text26")]
  [Computed]
  public string Text26_Json
  {
    get => NullIf(Text26, "");
    set => Text26 = value;
  }

  /// <summary>Length of the TEXT_28 attribute.</summary>
  public const int Text28_MaxLength = 28;

  /// <summary>
  /// The value of the TEXT_28 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = Text28_MaxLength)]
  public string Text28
  {
    get => text28 ?? "";
    set => text28 = TrimEnd(Substring(value, 1, Text28_MaxLength));
  }

  /// <summary>
  /// The json value of the Text28 attribute.</summary>
  [JsonPropertyName("text28")]
  [Computed]
  public string Text28_Json
  {
    get => NullIf(Text28, "");
    set => Text28 = value;
  }

  private string text10;
  private string text11;
  private string text13;
  private string text15;
  private string text17;
  private string text21;
  private string text22;
  private string text24;
  private string text26;
  private string text28;
}
