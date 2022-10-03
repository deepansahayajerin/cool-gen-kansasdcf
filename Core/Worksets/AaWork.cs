// The source file: AA_WORK, ID: 371863782, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class AaWork: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AaWork()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AaWork(AaWork that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AaWork Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the TEXT_LENGTH_31 attribute.</summary>
  public const int TextLength31_MaxLength = 31;

  /// <summary>
  /// The value of the TEXT_LENGTH_31 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Varchar, Length = TextLength31_MaxLength)]
    
  public string TextLength31
  {
    get => Get<string>("textLength31") ?? "";
    set => Set(
      "textLength31", IsEmpty(value) ? null : Substring
      (value, 1, TextLength31_MaxLength));
  }

  /// <summary>
  /// The json value of the TextLength31 attribute.</summary>
  [JsonPropertyName("textLength31")]
  [Computed]
  public string TextLength31_Json
  {
    get => NullIf(TextLength31, "");
    set => TextLength31 = value;
  }

  /// <summary>Length of the TEXT_LENGTH_16 attribute.</summary>
  public const int TextLength16_MaxLength = 16;

  /// <summary>
  /// The value of the TEXT_LENGTH_16 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Varchar, Length = TextLength16_MaxLength)]
    
  public string TextLength16
  {
    get => Get<string>("textLength16") ?? "";
    set => Set(
      "textLength16", IsEmpty(value) ? null : Substring
      (value, 1, TextLength16_MaxLength));
  }

  /// <summary>
  /// The json value of the TextLength16 attribute.</summary>
  [JsonPropertyName("textLength16")]
  [Computed]
  public string TextLength16_Json
  {
    get => NullIf(TextLength16, "");
    set => TextLength16 = value;
  }

  /// <summary>Length of the TEXT_LENGTH_01 attribute.</summary>
  public const int TextLength01_MaxLength = 1;

  /// <summary>
  /// The value of the TEXT_LENGTH_01 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = TextLength01_MaxLength)]
  public string TextLength01
  {
    get => Get<string>("textLength01") ?? "";
    set => Set(
      "textLength01", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TextLength01_MaxLength)));
  }

  /// <summary>
  /// The json value of the TextLength01 attribute.</summary>
  [JsonPropertyName("textLength01")]
  [Computed]
  public string TextLength01_Json
  {
    get => NullIf(TextLength01, "");
    set => TextLength01 = value;
  }

  /// <summary>Length of the TEXT_LENGTH_80 attribute.</summary>
  public const int TextLength80_MaxLength = 80;

  /// <summary>
  /// The value of the TEXT_LENGTH_80 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = TextLength80_MaxLength)]
  public string TextLength80
  {
    get => Get<string>("textLength80") ?? "";
    set => Set(
      "textLength80", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TextLength80_MaxLength)));
  }

  /// <summary>
  /// The json value of the TextLength80 attribute.</summary>
  [JsonPropertyName("textLength80")]
  [Computed]
  public string TextLength80_Json
  {
    get => NullIf(TextLength80, "");
    set => TextLength80 = value;
  }

  /// <summary>Length of the TEXT_LENGTH_20 attribute.</summary>
  public const int TextLength20_MaxLength = 20;

  /// <summary>
  /// The value of the TEXT_LENGTH_20 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = TextLength20_MaxLength)]
  public string TextLength20
  {
    get => Get<string>("textLength20") ?? "";
    set => Set(
      "textLength20", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TextLength20_MaxLength)));
  }

  /// <summary>
  /// The json value of the TextLength20 attribute.</summary>
  [JsonPropertyName("textLength20")]
  [Computed]
  public string TextLength20_Json
  {
    get => NullIf(TextLength20, "");
    set => TextLength20 = value;
  }

  /// <summary>Length of the TEXT_LENGTH_10 attribute.</summary>
  public const int TextLength10_MaxLength = 10;

  /// <summary>
  /// The value of the TEXT_LENGTH_10 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = TextLength10_MaxLength)]
  public string TextLength10
  {
    get => Get<string>("textLength10") ?? "";
    set => Set(
      "textLength10", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TextLength10_MaxLength)));
  }

  /// <summary>
  /// The json value of the TextLength10 attribute.</summary>
  [JsonPropertyName("textLength10")]
  [Computed]
  public string TextLength10_Json
  {
    get => NullIf(TextLength10, "");
    set => TextLength10 = value;
  }

  /// <summary>Length of the TEXT_LENGTH_12 attribute.</summary>
  public const int TextLength12_MaxLength = 12;

  /// <summary>
  /// The value of the TEXT_LENGTH_12 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = TextLength12_MaxLength)]
  public string TextLength12
  {
    get => Get<string>("textLength12") ?? "";
    set => Set(
      "textLength12", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TextLength12_MaxLength)));
  }

  /// <summary>
  /// The json value of the TextLength12 attribute.</summary>
  [JsonPropertyName("textLength12")]
  [Computed]
  public string TextLength12_Json
  {
    get => NullIf(TextLength12, "");
    set => TextLength12 = value;
  }

  /// <summary>Length of the RADIO_BUTTON attribute.</summary>
  public const int RadioButton_MaxLength = 1;

  /// <summary>
  /// The value of the RADIO_BUTTON attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = RadioButton_MaxLength)]
  [Value("")]
  [Value("C")]
  [Value("A")]
  [Value("Y")]
  [Value("U")]
  [Value("X")]
  [Value("D")]
  [ImplicitValue("")]
  public string RadioButton
  {
    get => Get<string>("radioButton") ?? "";
    set => Set(
      "radioButton", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, RadioButton_MaxLength)));
  }

  /// <summary>
  /// The json value of the RadioButton attribute.</summary>
  [JsonPropertyName("radioButton")]
  [Computed]
  public string RadioButton_Json
  {
    get => NullIf(RadioButton, "");
    set => RadioButton = value;
  }

  /// <summary>Length of the TEXT_VALUE attribute.</summary>
  public const int TextValue_MaxLength = 8;

  /// <summary>
  /// The value of the TEXT_VALUE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = TextValue_MaxLength)]
  public string TextValue
  {
    get => Get<string>("textValue") ?? "";
    set => Set(
      "textValue", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TextValue_MaxLength)));
  }

  /// <summary>
  /// The json value of the TextValue attribute.</summary>
  [JsonPropertyName("textValue")]
  [Computed]
  public string TextValue_Json
  {
    get => NullIf(TextValue, "");
    set => TextValue = value;
  }

  /// <summary>
  /// The value of the LENGTH attribute.
  /// </summary>
  [JsonPropertyName("length")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 3)]
  public int Length
  {
    get => Get<int?>("length") ?? 0;
    set => Set("length", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the EXITSTATE_PREFIX attribute.</summary>
  public const int ExitstatePrefix_MaxLength = 8;

  /// <summary>
  /// The value of the EXITSTATE_PREFIX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = ExitstatePrefix_MaxLength)
    ]
  public string ExitstatePrefix
  {
    get => Get<string>("exitstatePrefix") ?? "";
    set => Set(
      "exitstatePrefix", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ExitstatePrefix_MaxLength)));
  }

  /// <summary>
  /// The json value of the ExitstatePrefix attribute.</summary>
  [JsonPropertyName("exitstatePrefix")]
  [Computed]
  public string ExitstatePrefix_Json
  {
    get => NullIf(ExitstatePrefix, "");
    set => ExitstatePrefix = value;
  }

  /// <summary>Length of the TEXT_LENGTH_3866 attribute.</summary>
  public const int TextLength3866_MaxLength = 3866;

  /// <summary>
  /// The value of the TEXT_LENGTH_3866 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Varchar, Length
    = TextLength3866_MaxLength)]
  public string TextLength3866
  {
    get => Get<string>("textLength3866") ?? "";
    set => Set(
      "textLength3866", IsEmpty(value) ? null : Substring
      (value, 1, TextLength3866_MaxLength));
  }

  /// <summary>
  /// The json value of the TextLength3866 attribute.</summary>
  [JsonPropertyName("textLength3866")]
  [Computed]
  public string TextLength3866_Json
  {
    get => NullIf(TextLength3866, "");
    set => TextLength3866 = value;
  }

  /// <summary>Length of the TEXT_LENGTH_8 attribute.</summary>
  public const int TextLength8_MaxLength = 8;

  /// <summary>
  /// The value of the TEXT_LENGTH_8 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = TextLength8_MaxLength)]
  public string TextLength8
  {
    get => Get<string>("textLength8") ?? "";
    set => Set(
      "textLength8", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TextLength8_MaxLength)));
  }

  /// <summary>
  /// The json value of the TextLength8 attribute.</summary>
  [JsonPropertyName("textLength8")]
  [Computed]
  public string TextLength8_Json
  {
    get => NullIf(TextLength8, "");
    set => TextLength8 = value;
  }

  /// <summary>
  /// The value of the TEXT_LINE_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("textLineNumber")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 8)]
  public int TextLineNumber
  {
    get => Get<int?>("textLineNumber") ?? 0;
    set => Set("textLineNumber", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the TEXT_TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("textTimestamp")]
  [Member(Index = 15, Type = MemberType.Time)]
  public TimeSpan TextTimestamp
  {
    get => Get<TimeSpan?>("textTimestamp") ?? default;
    set => Set("textTimestamp", value == default ? null : value as TimeSpan?);
  }

  /// <summary>Length of the TEXT_TITLE attribute.</summary>
  public const int TextTitle_MaxLength = 32;

  /// <summary>
  /// The value of the TEXT_TITLE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = TextTitle_MaxLength)]
  public string TextTitle
  {
    get => Get<string>("textTitle") ?? "";
    set => Set(
      "textTitle", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TextTitle_MaxLength)));
  }

  /// <summary>
  /// The json value of the TextTitle attribute.</summary>
  [JsonPropertyName("textTitle")]
  [Computed]
  public string TextTitle_Json
  {
    get => NullIf(TextTitle, "");
    set => TextTitle = value;
  }
}
