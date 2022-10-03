// The source file: WORK_AREA, ID: 371424136, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// Fields for concatenating and manipulating data.
/// </summary>
[Serializable]
public partial class WorkArea: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public WorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public WorkArea(WorkArea that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new WorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the TEXT_200 attribute.</summary>
  public const int Text200_MaxLength = 200;

  /// <summary>
  /// The value of the TEXT_200 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Text200_MaxLength)]
  public string Text200
  {
    get => Get<string>("text200") ?? "";
    set => Set(
      "text200", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text200_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text200 attribute.</summary>
  [JsonPropertyName("text200")]
  [Computed]
  public string Text200_Json
  {
    get => NullIf(Text200, "");
    set => Text200 = value;
  }

  /// <summary>Length of the TEXT_166 attribute.</summary>
  public const int Text166_MaxLength = 166;

  /// <summary>
  /// The value of the TEXT_166 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Text166_MaxLength)]
  public string Text166
  {
    get => Get<string>("text166") ?? "";
    set => Set(
      "text166", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text166_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text166 attribute.</summary>
  [JsonPropertyName("text166")]
  [Computed]
  public string Text166_Json
  {
    get => NullIf(Text166, "");
    set => Text166 = value;
  }

  /// <summary>Length of the TEXT_80 attribute.</summary>
  public const int Text80_MaxLength = 80;

  /// <summary>
  /// The value of the TEXT_80 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Text80_MaxLength)]
  public string Text80
  {
    get => Get<string>("text80") ?? "";
    set => Set(
      "text80", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text80_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text80 attribute.</summary>
  [JsonPropertyName("text80")]
  [Computed]
  public string Text80_Json
  {
    get => NullIf(Text80, "");
    set => Text80 = value;
  }

  /// <summary>Length of the TEXT_1 attribute.</summary>
  public const int Text1_MaxLength = 1;

  /// <summary>
  /// The value of the TEXT_1 attribute.
  /// Work field used to hold text.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Text1_MaxLength)]
  public string Text1
  {
    get => Get<string>("text1") ?? "";
    set => Set(
      "text1", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text1_MaxLength)));
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
  [Member(Index = 5, Type = MemberType.Char, Length = Text2_MaxLength)]
  public string Text2
  {
    get => Get<string>("text2") ?? "";
    set => Set(
      "text2", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text2_MaxLength)));
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

  /// <summary>Length of the TEXT_3 attribute.</summary>
  public const int Text3_MaxLength = 3;

  /// <summary>
  /// The value of the TEXT_3 attribute.
  /// Work field used to hold text.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Text3_MaxLength)]
  public string Text3
  {
    get => Get<string>("text3") ?? "";
    set => Set(
      "text3", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text3_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text3 attribute.</summary>
  [JsonPropertyName("text3")]
  [Computed]
  public string Text3_Json
  {
    get => NullIf(Text3, "");
    set => Text3 = value;
  }

  /// <summary>Length of the TEXT_4 attribute.</summary>
  public const int Text4_MaxLength = 4;

  /// <summary>
  /// The value of the TEXT_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Text4_MaxLength)]
  public string Text4
  {
    get => Get<string>("text4") ?? "";
    set => Set(
      "text4", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text4_MaxLength)));
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

  /// <summary>Length of the TEXT_5 attribute.</summary>
  public const int Text5_MaxLength = 5;

  /// <summary>
  /// The value of the TEXT_5 attribute.
  /// Work field used to hold text.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = Text5_MaxLength)]
  public string Text5
  {
    get => Get<string>("text5") ?? "";
    set => Set(
      "text5", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text5_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text5 attribute.</summary>
  [JsonPropertyName("text5")]
  [Computed]
  public string Text5_Json
  {
    get => NullIf(Text5, "");
    set => Text5 = value;
  }

  /// <summary>Length of the TEXT_6 attribute.</summary>
  public const int Text6_MaxLength = 6;

  /// <summary>
  /// The value of the TEXT_6 attribute.
  /// Work field used to hold text.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = Text6_MaxLength)]
  public string Text6
  {
    get => Get<string>("text6") ?? "";
    set => Set(
      "text6", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text6_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text6 attribute.</summary>
  [JsonPropertyName("text6")]
  [Computed]
  public string Text6_Json
  {
    get => NullIf(Text6, "");
    set => Text6 = value;
  }

  /// <summary>Length of the TEXT_7 attribute.</summary>
  public const int Text7_MaxLength = 7;

  /// <summary>
  /// The value of the TEXT_7 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = Text7_MaxLength)]
  public string Text7
  {
    get => Get<string>("text7") ?? "";
    set => Set(
      "text7", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text7_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text7 attribute.</summary>
  [JsonPropertyName("text7")]
  [Computed]
  public string Text7_Json
  {
    get => NullIf(Text7, "");
    set => Text7 = value;
  }

  /// <summary>Length of the TEXT_8 attribute.</summary>
  public const int Text8_MaxLength = 8;

  /// <summary>
  /// The value of the TEXT_8 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Text8_MaxLength)]
  public string Text8
  {
    get => Get<string>("text8") ?? "";
    set => Set(
      "text8", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text8_MaxLength)));
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

  /// <summary>Length of the TEXT_9 attribute.</summary>
  public const int Text9_MaxLength = 9;

  /// <summary>
  /// The value of the TEXT_9 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = Text9_MaxLength)]
  public string Text9
  {
    get => Get<string>("text9") ?? "";
    set => Set(
      "text9", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text9_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text9 attribute.</summary>
  [JsonPropertyName("text9")]
  [Computed]
  public string Text9_Json
  {
    get => NullIf(Text9, "");
    set => Text9 = value;
  }

  /// <summary>Length of the TEXT_10 attribute.</summary>
  public const int Text10_MaxLength = 10;

  /// <summary>
  /// The value of the TEXT_10 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = Text10_MaxLength)]
  public string Text10
  {
    get => Get<string>("text10") ?? "";
    set => Set(
      "text10", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text10_MaxLength)));
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
  /// Work field used to hold text.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = Text11_MaxLength)]
  public string Text11
  {
    get => Get<string>("text11") ?? "";
    set => Set(
      "text11", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text11_MaxLength)));
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

  /// <summary>Length of the TEXT_12 attribute.</summary>
  public const int Text12_MaxLength = 12;

  /// <summary>
  /// The value of the TEXT_12 attribute.
  /// Work field used to hold text.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = Text12_MaxLength)]
  public string Text12
  {
    get => Get<string>("text12") ?? "";
    set => Set(
      "text12", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text12_MaxLength)));
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

  /// <summary>Length of the TEXT_13 attribute.</summary>
  public const int Text13_MaxLength = 13;

  /// <summary>
  /// The value of the TEXT_13 attribute.
  /// Work field used to hold text.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = Text13_MaxLength)]
  public string Text13
  {
    get => Get<string>("text13") ?? "";
    set => Set(
      "text13", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text13_MaxLength)));
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

  /// <summary>Length of the TEXT_14 attribute.</summary>
  public const int Text14_MaxLength = 14;

  /// <summary>
  /// The value of the TEXT_14 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = Text14_MaxLength)]
  public string Text14
  {
    get => Get<string>("text14") ?? "";
    set => Set(
      "text14", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text14_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text14 attribute.</summary>
  [JsonPropertyName("text14")]
  [Computed]
  public string Text14_Json
  {
    get => NullIf(Text14, "");
    set => Text14 = value;
  }

  /// <summary>Length of the TEXT_15 attribute.</summary>
  public const int Text15_MaxLength = 15;

  /// <summary>
  /// The value of the TEXT_15 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = Text15_MaxLength)]
  public string Text15
  {
    get => Get<string>("text15") ?? "";
    set => Set(
      "text15", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text15_MaxLength)));
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

  /// <summary>Length of the TEXT_16 attribute.</summary>
  public const int Text16_MaxLength = 16;

  /// <summary>
  /// The value of the TEXT_16 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = Text16_MaxLength)]
  public string Text16
  {
    get => Get<string>("text16") ?? "";
    set => Set(
      "text16", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text16_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text16 attribute.</summary>
  [JsonPropertyName("text16")]
  [Computed]
  public string Text16_Json
  {
    get => NullIf(Text16, "");
    set => Text16 = value;
  }

  /// <summary>Length of the TEXT_17 attribute.</summary>
  public const int Text17_MaxLength = 17;

  /// <summary>
  /// The value of the TEXT_17 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = Text17_MaxLength)]
  public string Text17
  {
    get => Get<string>("text17") ?? "";
    set => Set(
      "text17", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text17_MaxLength)));
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

  /// <summary>Length of the TEXT_18 attribute.</summary>
  public const int Text18_MaxLength = 18;

  /// <summary>
  /// The value of the TEXT_18 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length = Text18_MaxLength)]
  public string Text18
  {
    get => Get<string>("text18") ?? "";
    set => Set(
      "text18", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text18_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text18 attribute.</summary>
  [JsonPropertyName("text18")]
  [Computed]
  public string Text18_Json
  {
    get => NullIf(Text18, "");
    set => Text18 = value;
  }

  /// <summary>Length of the TEXT_20 attribute.</summary>
  public const int Text20_MaxLength = 20;

  /// <summary>
  /// The value of the TEXT_20 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length = Text20_MaxLength)]
  public string Text20
  {
    get => Get<string>("text20") ?? "";
    set => Set(
      "text20", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text20_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text20 attribute.</summary>
  [JsonPropertyName("text20")]
  [Computed]
  public string Text20_Json
  {
    get => NullIf(Text20, "");
    set => Text20 = value;
  }

  /// <summary>Length of the TEXT_21 attribute.</summary>
  public const int Text21_MaxLength = 21;

  /// <summary>
  /// The value of the TEXT_21 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length = Text21_MaxLength)]
  public string Text21
  {
    get => Get<string>("text21") ?? "";
    set => Set(
      "text21", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text21_MaxLength)));
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
  [Member(Index = 24, Type = MemberType.Char, Length = Text22_MaxLength)]
  public string Text22
  {
    get => Get<string>("text22") ?? "";
    set => Set(
      "text22", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text22_MaxLength)));
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

  /// <summary>Length of the TEXT_23 attribute.</summary>
  public const int Text23_MaxLength = 23;

  /// <summary>
  /// The value of the TEXT_23 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = Text23_MaxLength)]
  public string Text23
  {
    get => Get<string>("text23") ?? "";
    set => Set(
      "text23", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text23_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text23 attribute.</summary>
  [JsonPropertyName("text23")]
  [Computed]
  public string Text23_Json
  {
    get => NullIf(Text23, "");
    set => Text23 = value;
  }

  /// <summary>Length of the TEXT_24 attribute.</summary>
  public const int Text24_MaxLength = 24;

  /// <summary>
  /// The value of the TEXT_24 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length = Text24_MaxLength)]
  public string Text24
  {
    get => Get<string>("text24") ?? "";
    set => Set(
      "text24", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text24_MaxLength)));
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

  /// <summary>Length of the TEXT_25 attribute.</summary>
  public const int Text25_MaxLength = 25;

  /// <summary>
  /// The value of the TEXT_25 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length = Text25_MaxLength)]
  public string Text25
  {
    get => Get<string>("text25") ?? "";
    set => Set(
      "text25", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text25_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text25 attribute.</summary>
  [JsonPropertyName("text25")]
  [Computed]
  public string Text25_Json
  {
    get => NullIf(Text25, "");
    set => Text25 = value;
  }

  /// <summary>Length of the TEXT_32 attribute.</summary>
  public const int Text32_MaxLength = 32;

  /// <summary>
  /// The value of the TEXT_32 attribute.
  /// Work field used to hold text.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length = Text32_MaxLength)]
  public string Text32
  {
    get => Get<string>("text32") ?? "";
    set => Set(
      "text32", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text32_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text32 attribute.</summary>
  [JsonPropertyName("text32")]
  [Computed]
  public string Text32_Json
  {
    get => NullIf(Text32, "");
    set => Text32 = value;
  }

  /// <summary>Length of the TEXT_33 attribute.</summary>
  public const int Text33_MaxLength = 33;

  /// <summary>
  /// The value of the TEXT_33 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length = Text33_MaxLength)]
  public string Text33
  {
    get => Get<string>("text33") ?? "";
    set => Set(
      "text33", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text33_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text33 attribute.</summary>
  [JsonPropertyName("text33")]
  [Computed]
  public string Text33_Json
  {
    get => NullIf(Text33, "");
    set => Text33 = value;
  }

  /// <summary>Length of the TEXT_35 attribute.</summary>
  public const int Text35_MaxLength = 35;

  /// <summary>
  /// The value of the TEXT_35 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length = Text35_MaxLength)]
  public string Text35
  {
    get => Get<string>("text35") ?? "";
    set => Set(
      "text35", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text35_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text35 attribute.</summary>
  [JsonPropertyName("text35")]
  [Computed]
  public string Text35_Json
  {
    get => NullIf(Text35, "");
    set => Text35 = value;
  }

  /// <summary>Length of the TEXT_37 attribute.</summary>
  public const int Text37_MaxLength = 37;

  /// <summary>
  /// The value of the TEXT_37 attribute.
  /// Work field used to hold text.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length = Text37_MaxLength)]
  public string Text37
  {
    get => Get<string>("text37") ?? "";
    set => Set(
      "text37", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text37_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text37 attribute.</summary>
  [JsonPropertyName("text37")]
  [Computed]
  public string Text37_Json
  {
    get => NullIf(Text37, "");
    set => Text37 = value;
  }

  /// <summary>Length of the TEXT_40 attribute.</summary>
  public const int Text40_MaxLength = 40;

  /// <summary>
  /// The value of the TEXT_40 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length = Text40_MaxLength)]
  public string Text40
  {
    get => Get<string>("text40") ?? "";
    set => Set(
      "text40", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text40_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text40 attribute.</summary>
  [JsonPropertyName("text40")]
  [Computed]
  public string Text40_Json
  {
    get => NullIf(Text40, "");
    set => Text40 = value;
  }

  /// <summary>Length of the TEXT_50 attribute.</summary>
  public const int Text50_MaxLength = 50;

  /// <summary>
  /// The value of the TEXT_50 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length = Text50_MaxLength)]
  public string Text50
  {
    get => Get<string>("text50") ?? "";
    set => Set(
      "text50", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text50_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text50 attribute.</summary>
  [JsonPropertyName("text50")]
  [Computed]
  public string Text50_Json
  {
    get => NullIf(Text50, "");
    set => Text50 = value;
  }

  /// <summary>Length of the TEXT_60 attribute.</summary>
  public const int Text60_MaxLength = 60;

  /// <summary>
  /// The value of the TEXT_60 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length = Text60_MaxLength)]
  public string Text60
  {
    get => Get<string>("text60") ?? "";
    set => Set(
      "text60", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Text60_MaxLength)));
  }

  /// <summary>
  /// The json value of the Text60 attribute.</summary>
  [JsonPropertyName("text60")]
  [Computed]
  public string Text60_Json
  {
    get => NullIf(Text60, "");
    set => Text60 = value;
  }
}
