// The source file: NEW_WORK_SET, ID: 371045720, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class NewWorkSet: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NewWorkSet()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NewWorkSet(NewWorkSet that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NewWorkSet Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NewWorkSet that)
  {
    base.Assign(that);
    text3 = that.text3;
    text76 = that.text76;
    text9 = that.text9;
    fillerText1 = that.fillerText1;
    pageNumber = that.pageNumber;
    startIndex = that.startIndex;
    endIndex = that.endIndex;
  }

  /// <summary>Length of the TEXT_3 attribute.</summary>
  public const int Text3_MaxLength = 3;

  /// <summary>
  /// The value of the TEXT_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Text3_MaxLength)]
  public string Text3
  {
    get => text3 ?? "";
    set => text3 = TrimEnd(Substring(value, 1, Text3_MaxLength));
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

  /// <summary>Length of the TEXT_76 attribute.</summary>
  public const int Text76_MaxLength = 76;

  /// <summary>
  /// The value of the TEXT_76 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Text76_MaxLength)]
  public string Text76
  {
    get => text76 ?? "";
    set => text76 = TrimEnd(Substring(value, 1, Text76_MaxLength));
  }

  /// <summary>
  /// The json value of the Text76 attribute.</summary>
  [JsonPropertyName("text76")]
  [Computed]
  public string Text76_Json
  {
    get => NullIf(Text76, "");
    set => Text76 = value;
  }

  /// <summary>Length of the TEXT_9 attribute.</summary>
  public const int Text9_MaxLength = 9;

  /// <summary>
  /// The value of the TEXT_9 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Text9_MaxLength)]
  public string Text9
  {
    get => text9 ?? "";
    set => text9 = TrimEnd(Substring(value, 1, Text9_MaxLength));
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

  /// <summary>Length of the FILLER_TEXT_1 attribute.</summary>
  public const int FillerText1_MaxLength = 1;

  /// <summary>
  /// The value of the FILLER_TEXT_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = FillerText1_MaxLength)]
  public string FillerText1
  {
    get => fillerText1 ?? "";
    set => fillerText1 = TrimEnd(Substring(value, 1, FillerText1_MaxLength));
  }

  /// <summary>
  /// The json value of the FillerText1 attribute.</summary>
  [JsonPropertyName("fillerText1")]
  [Computed]
  public string FillerText1_Json
  {
    get => NullIf(FillerText1, "");
    set => FillerText1 = value;
  }

  /// <summary>
  /// The value of the PAGE_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("pageNumber")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 2)]
  public int PageNumber
  {
    get => pageNumber;
    set => pageNumber = value;
  }

  /// <summary>
  /// The value of the START_INDEX attribute.
  /// </summary>
  [JsonPropertyName("startIndex")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 2)]
  public int StartIndex
  {
    get => startIndex;
    set => startIndex = value;
  }

  /// <summary>
  /// The value of the END_INDEX attribute.
  /// </summary>
  [JsonPropertyName("endIndex")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 2)]
  public int EndIndex
  {
    get => endIndex;
    set => endIndex = value;
  }

  private string text3;
  private string text76;
  private string text9;
  private string fillerText1;
  private int pageNumber;
  private int startIndex;
  private int endIndex;
}
