// The source file: SCROLLING_ATTRIBUTES, ID: 371822109, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: OBLGESTB
/// </summary>
[Serializable]
public partial class ScrollingAttributes: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ScrollingAttributes()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ScrollingAttributes(ScrollingAttributes that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ScrollingAttributes Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ScrollingAttributes that)
  {
    base.Assign(that);
    selection = that.selection;
    selectionCount = that.selectionCount;
    pageNumber = that.pageNumber;
    lineCommand = that.lineCommand;
    plusFlag = that.plusFlag;
    minusFlag = that.minusFlag;
    commandCount = that.commandCount;
    commandType = that.commandType;
    commandTypeCount = that.commandTypeCount;
    readCount = that.readCount;
    searchInputValue = that.searchInputValue;
    searchBeginValue = that.searchBeginValue;
    searchEndValue = that.searchEndValue;
    searchPartialValue = that.searchPartialValue;
    keyInputText = that.keyInputText;
    moreIndicator = that.moreIndicator;
    searchInputText = that.searchInputText;
  }

  /// <summary>Length of the SELECTION attribute.</summary>
  public const int Selection_MaxLength = 1;

  /// <summary>
  /// The value of the SELECTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Selection_MaxLength)]
  public string Selection
  {
    get => selection ?? "";
    set => selection = TrimEnd(Substring(value, 1, Selection_MaxLength));
  }

  /// <summary>
  /// The json value of the Selection attribute.</summary>
  [JsonPropertyName("selection")]
  [Computed]
  public string Selection_Json
  {
    get => NullIf(Selection, "");
    set => Selection = value;
  }

  /// <summary>
  /// The value of the SELECTION_COUNT attribute.
  /// </summary>
  [JsonPropertyName("selectionCount")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int SelectionCount
  {
    get => selectionCount;
    set => selectionCount = value;
  }

  /// <summary>
  /// The value of the PAGE_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("pageNumber")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 2)]
  public int PageNumber
  {
    get => pageNumber;
    set => pageNumber = value;
  }

  /// <summary>Length of the LINE_COMMAND attribute.</summary>
  public const int LineCommand_MaxLength = 1;

  /// <summary>
  /// The value of the LINE_COMMAND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = LineCommand_MaxLength)]
  public string LineCommand
  {
    get => lineCommand ?? "";
    set => lineCommand = TrimEnd(Substring(value, 1, LineCommand_MaxLength));
  }

  /// <summary>
  /// The json value of the LineCommand attribute.</summary>
  [JsonPropertyName("lineCommand")]
  [Computed]
  public string LineCommand_Json
  {
    get => NullIf(LineCommand, "");
    set => LineCommand = value;
  }

  /// <summary>Length of the PLUS_FLAG attribute.</summary>
  public const int PlusFlag_MaxLength = 1;

  /// <summary>
  /// The value of the PLUS_FLAG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = PlusFlag_MaxLength)]
  public string PlusFlag
  {
    get => plusFlag ?? "";
    set => plusFlag = TrimEnd(Substring(value, 1, PlusFlag_MaxLength));
  }

  /// <summary>
  /// The json value of the PlusFlag attribute.</summary>
  [JsonPropertyName("plusFlag")]
  [Computed]
  public string PlusFlag_Json
  {
    get => NullIf(PlusFlag, "");
    set => PlusFlag = value;
  }

  /// <summary>Length of the MINUS_FLAG attribute.</summary>
  public const int MinusFlag_MaxLength = 1;

  /// <summary>
  /// The value of the MINUS_FLAG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = MinusFlag_MaxLength)]
  public string MinusFlag
  {
    get => minusFlag ?? "";
    set => minusFlag = TrimEnd(Substring(value, 1, MinusFlag_MaxLength));
  }

  /// <summary>
  /// The json value of the MinusFlag attribute.</summary>
  [JsonPropertyName("minusFlag")]
  [Computed]
  public string MinusFlag_Json
  {
    get => NullIf(MinusFlag, "");
    set => MinusFlag = value;
  }

  /// <summary>
  /// The value of the COMMAND_COUNT attribute.
  /// </summary>
  [JsonPropertyName("commandCount")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int CommandCount
  {
    get => commandCount;
    set => commandCount = value;
  }

  /// <summary>Length of the COMMAND_TYPE attribute.</summary>
  public const int CommandType_MaxLength = 1;

  /// <summary>
  /// The value of the COMMAND_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CommandType_MaxLength)]
  public string CommandType
  {
    get => commandType ?? "";
    set => commandType = TrimEnd(Substring(value, 1, CommandType_MaxLength));
  }

  /// <summary>
  /// The json value of the CommandType attribute.</summary>
  [JsonPropertyName("commandType")]
  [Computed]
  public string CommandType_Json
  {
    get => NullIf(CommandType, "");
    set => CommandType = value;
  }

  /// <summary>
  /// The value of the COMMAND_TYPE_COUNT attribute.
  /// </summary>
  [JsonPropertyName("commandTypeCount")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 3)]
  public int CommandTypeCount
  {
    get => commandTypeCount;
    set => commandTypeCount = value;
  }

  /// <summary>
  /// The value of the READ_COUNT attribute.
  /// </summary>
  [JsonPropertyName("readCount")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 3)]
  public int ReadCount
  {
    get => readCount;
    set => readCount = value;
  }

  /// <summary>Length of the SEARCH_INPUT_VALUE attribute.</summary>
  public const int SearchInputValue_MaxLength = 254;

  /// <summary>
  /// The value of the SEARCH_INPUT_VALUE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = SearchInputValue_MaxLength)]
  public string SearchInputValue
  {
    get => searchInputValue ?? "";
    set => searchInputValue =
      TrimEnd(Substring(value, 1, SearchInputValue_MaxLength));
  }

  /// <summary>
  /// The json value of the SearchInputValue attribute.</summary>
  [JsonPropertyName("searchInputValue")]
  [Computed]
  public string SearchInputValue_Json
  {
    get => NullIf(SearchInputValue, "");
    set => SearchInputValue = value;
  }

  /// <summary>Length of the SEARCH_BEGIN_VALUE attribute.</summary>
  public const int SearchBeginValue_MaxLength = 254;

  /// <summary>
  /// The value of the SEARCH_BEGIN_VALUE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = SearchBeginValue_MaxLength)]
  public string SearchBeginValue
  {
    get => searchBeginValue ?? "";
    set => searchBeginValue =
      TrimEnd(Substring(value, 1, SearchBeginValue_MaxLength));
  }

  /// <summary>
  /// The json value of the SearchBeginValue attribute.</summary>
  [JsonPropertyName("searchBeginValue")]
  [Computed]
  public string SearchBeginValue_Json
  {
    get => NullIf(SearchBeginValue, "");
    set => SearchBeginValue = value;
  }

  /// <summary>Length of the SEARCH_END_VALUE attribute.</summary>
  public const int SearchEndValue_MaxLength = 254;

  /// <summary>
  /// The value of the SEARCH_END_VALUE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = SearchEndValue_MaxLength)]
    
  public string SearchEndValue
  {
    get => searchEndValue ?? "";
    set => searchEndValue =
      TrimEnd(Substring(value, 1, SearchEndValue_MaxLength));
  }

  /// <summary>
  /// The json value of the SearchEndValue attribute.</summary>
  [JsonPropertyName("searchEndValue")]
  [Computed]
  public string SearchEndValue_Json
  {
    get => NullIf(SearchEndValue, "");
    set => SearchEndValue = value;
  }

  /// <summary>Length of the SEARCH_PARTIAL_VALUE attribute.</summary>
  public const int SearchPartialValue_MaxLength = 254;

  /// <summary>
  /// The value of the SEARCH_PARTIAL_VALUE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = SearchPartialValue_MaxLength)]
  public string SearchPartialValue
  {
    get => searchPartialValue ?? "";
    set => searchPartialValue =
      TrimEnd(Substring(value, 1, SearchPartialValue_MaxLength));
  }

  /// <summary>
  /// The json value of the SearchPartialValue attribute.</summary>
  [JsonPropertyName("searchPartialValue")]
  [Computed]
  public string SearchPartialValue_Json
  {
    get => NullIf(SearchPartialValue, "");
    set => SearchPartialValue = value;
  }

  /// <summary>Length of the KEY_INPUT_TEXT attribute.</summary>
  public const int KeyInputText_MaxLength = 16;

  /// <summary>
  /// The value of the KEY_INPUT_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = KeyInputText_MaxLength)]
  public string KeyInputText
  {
    get => keyInputText ?? "";
    set => keyInputText = TrimEnd(Substring(value, 1, KeyInputText_MaxLength));
  }

  /// <summary>
  /// The json value of the KeyInputText attribute.</summary>
  [JsonPropertyName("keyInputText")]
  [Computed]
  public string KeyInputText_Json
  {
    get => NullIf(KeyInputText, "");
    set => KeyInputText = value;
  }

  /// <summary>Length of the MORE_INDICATOR attribute.</summary>
  public const int MoreIndicator_MaxLength = 4;

  /// <summary>
  /// The value of the MORE_INDICATOR attribute.
  /// This attribute is used to contain the literal MORE.
  /// It is used in conjunction with the PLUS_FLAG and
  /// MINUS_FLAG.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = MoreIndicator_MaxLength)]
  public string MoreIndicator
  {
    get => moreIndicator ?? "";
    set => moreIndicator =
      TrimEnd(Substring(value, 1, MoreIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the MoreIndicator attribute.</summary>
  [JsonPropertyName("moreIndicator")]
  [Computed]
  public string MoreIndicator_Json
  {
    get => NullIf(MoreIndicator, "");
    set => MoreIndicator = value;
  }

  /// <summary>Length of the SEARCH_INPUT_TEXT attribute.</summary>
  public const int SearchInputText_MaxLength = 16;

  /// <summary>
  /// The value of the SEARCH_INPUT_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = SearchInputText_MaxLength)
    ]
  public string SearchInputText
  {
    get => searchInputText ?? "";
    set => searchInputText =
      TrimEnd(Substring(value, 1, SearchInputText_MaxLength));
  }

  /// <summary>
  /// The json value of the SearchInputText attribute.</summary>
  [JsonPropertyName("searchInputText")]
  [Computed]
  public string SearchInputText_Json
  {
    get => NullIf(SearchInputText, "");
    set => SearchInputText = value;
  }

  private string selection;
  private int selectionCount;
  private int pageNumber;
  private string lineCommand;
  private string plusFlag;
  private string minusFlag;
  private int commandCount;
  private string commandType;
  private int commandTypeCount;
  private int readCount;
  private string searchInputValue;
  private string searchBeginValue;
  private string searchEndValue;
  private string searchPartialValue;
  private string keyInputText;
  private string moreIndicator;
  private string searchInputText;
}
