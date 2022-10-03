// The source file: SSN_WORK_AREA, ID: 371456805, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class SsnWorkArea: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public SsnWorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public SsnWorkArea(SsnWorkArea that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new SsnWorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(SsnWorkArea that)
  {
    base.Assign(that);
    convertOption = that.convertOption;
    ssnText9 = that.ssnText9;
    ssnTextPart1 = that.ssnTextPart1;
    ssnTextPart2 = that.ssnTextPart2;
    ssnTextPart3 = that.ssnTextPart3;
    ssnNum9 = that.ssnNum9;
    ssnNumPart1 = that.ssnNumPart1;
    ssnNumPart2 = that.ssnNumPart2;
    ssnNumPart3 = that.ssnNumPart3;
  }

  /// <summary>Length of the CONVERT_OPTION attribute.</summary>
  public const int ConvertOption_MaxLength = 1;

  /// <summary>
  /// The value of the CONVERT_OPTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ConvertOption_MaxLength)]
  public string ConvertOption
  {
    get => convertOption ?? "";
    set => convertOption =
      TrimEnd(Substring(value, 1, ConvertOption_MaxLength));
  }

  /// <summary>
  /// The json value of the ConvertOption attribute.</summary>
  [JsonPropertyName("convertOption")]
  [Computed]
  public string ConvertOption_Json
  {
    get => NullIf(ConvertOption, "");
    set => ConvertOption = value;
  }

  /// <summary>Length of the SSN_TEXT9 attribute.</summary>
  public const int SsnText9_MaxLength = 9;

  /// <summary>
  /// The value of the SSN_TEXT9 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = SsnText9_MaxLength)]
  public string SsnText9
  {
    get => ssnText9 ?? "";
    set => ssnText9 = TrimEnd(Substring(value, 1, SsnText9_MaxLength));
  }

  /// <summary>
  /// The json value of the SsnText9 attribute.</summary>
  [JsonPropertyName("ssnText9")]
  [Computed]
  public string SsnText9_Json
  {
    get => NullIf(SsnText9, "");
    set => SsnText9 = value;
  }

  /// <summary>Length of the SSN_TEXT_PART1 attribute.</summary>
  public const int SsnTextPart1_MaxLength = 3;

  /// <summary>
  /// The value of the SSN_TEXT_PART1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = SsnTextPart1_MaxLength)]
  public string SsnTextPart1
  {
    get => ssnTextPart1 ?? "";
    set => ssnTextPart1 = TrimEnd(Substring(value, 1, SsnTextPart1_MaxLength));
  }

  /// <summary>
  /// The json value of the SsnTextPart1 attribute.</summary>
  [JsonPropertyName("ssnTextPart1")]
  [Computed]
  public string SsnTextPart1_Json
  {
    get => NullIf(SsnTextPart1, "");
    set => SsnTextPart1 = value;
  }

  /// <summary>Length of the SSN_TEXT_PART2 attribute.</summary>
  public const int SsnTextPart2_MaxLength = 2;

  /// <summary>
  /// The value of the SSN_TEXT_PART2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = SsnTextPart2_MaxLength)]
  public string SsnTextPart2
  {
    get => ssnTextPart2 ?? "";
    set => ssnTextPart2 = TrimEnd(Substring(value, 1, SsnTextPart2_MaxLength));
  }

  /// <summary>
  /// The json value of the SsnTextPart2 attribute.</summary>
  [JsonPropertyName("ssnTextPart2")]
  [Computed]
  public string SsnTextPart2_Json
  {
    get => NullIf(SsnTextPart2, "");
    set => SsnTextPart2 = value;
  }

  /// <summary>Length of the SSN_TEXT_PART3 attribute.</summary>
  public const int SsnTextPart3_MaxLength = 4;

  /// <summary>
  /// The value of the SSN_TEXT_PART3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = SsnTextPart3_MaxLength)]
  public string SsnTextPart3
  {
    get => ssnTextPart3 ?? "";
    set => ssnTextPart3 = TrimEnd(Substring(value, 1, SsnTextPart3_MaxLength));
  }

  /// <summary>
  /// The json value of the SsnTextPart3 attribute.</summary>
  [JsonPropertyName("ssnTextPart3")]
  [Computed]
  public string SsnTextPart3_Json
  {
    get => NullIf(SsnTextPart3, "");
    set => SsnTextPart3 = value;
  }

  /// <summary>
  /// The value of the SSN_NUM9 attribute.
  /// </summary>
  [JsonPropertyName("ssnNum9")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 9)]
  public int SsnNum9
  {
    get => ssnNum9;
    set => ssnNum9 = value;
  }

  /// <summary>
  /// The value of the SSN_NUM_PART1 attribute.
  /// </summary>
  [JsonPropertyName("ssnNumPart1")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int SsnNumPart1
  {
    get => ssnNumPart1;
    set => ssnNumPart1 = value;
  }

  /// <summary>
  /// The value of the SSN_NUM_PART2 attribute.
  /// </summary>
  [JsonPropertyName("ssnNumPart2")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 2)]
  public int SsnNumPart2
  {
    get => ssnNumPart2;
    set => ssnNumPart2 = value;
  }

  /// <summary>
  /// The value of the SSN_NUM_PART3 attribute.
  /// </summary>
  [JsonPropertyName("ssnNumPart3")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 4)]
  public int SsnNumPart3
  {
    get => ssnNumPart3;
    set => ssnNumPart3 = value;
  }

  private string convertOption;
  private string ssnText9;
  private string ssnTextPart1;
  private string ssnTextPart2;
  private string ssnTextPart3;
  private int ssnNum9;
  private int ssnNumPart1;
  private int ssnNumPart2;
  private int ssnNumPart3;
}
