// The source file: IEF_SUPPLIED, ID: 371420387, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class Common: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Common()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Common(Common that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Common Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = State_MaxLength)]
  public string State
  {
    get => Get<string>("state") ?? "";
    set => Set(
      "state", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, State_MaxLength)));
  }

  /// <summary>
  /// The json value of the State attribute.</summary>
  [JsonPropertyName("state")]
  [Computed]
  public string State_Json
  {
    get => NullIf(State, "");
    set => State = value;
  }

  /// <summary>Length of the PERSON_NAME attribute.</summary>
  public const int PersonName_MaxLength = 40;

  /// <summary>
  /// The value of the PERSON_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = PersonName_MaxLength)]
  public string PersonName
  {
    get => Get<string>("personName") ?? "";
    set => Set(
      "personName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, PersonName_MaxLength)));
  }

  /// <summary>
  /// The json value of the PersonName attribute.</summary>
  [JsonPropertyName("personName")]
  [Computed]
  public string PersonName_Json
  {
    get => NullIf(PersonName, "");
    set => PersonName = value;
  }

  /// <summary>
  /// The value of the COUNT attribute.
  /// </summary>
  [JsonPropertyName("count")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 9)]
  public int Count
  {
    get => Get<int?>("count") ?? 0;
    set => Set("count", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the TOTAL_REAL attribute.
  /// </summary>
  [JsonPropertyName("totalReal")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 4, Type = MemberType.Number, Length = 15, Precision = 4)]
  public decimal TotalReal
  {
    get => Get<decimal?>("totalReal") ?? 0M;
    set =>
      Set("totalReal", value == 0M ? null : Truncate(value, 4) as decimal?);
  }

  /// <summary>
  /// The value of the TOTAL_CURRENCY attribute.
  /// </summary>
  [JsonPropertyName("totalCurrency")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 15, Precision = 2)]
  public decimal TotalCurrency
  {
    get => Get<decimal?>("totalCurrency") ?? 0M;
    set => Set(
      "totalCurrency", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the TOTAL_INTEGER attribute.
  /// </summary>
  [JsonPropertyName("totalInteger")]
  [DefaultValue(0L)]
  [Member(Index = 6, Type = MemberType.Number, Length = 15)]
  public long TotalInteger
  {
    get => Get<long?>("totalInteger") ?? 0L;
    set => Set("totalInteger", value == 0L ? null : value as long?);
  }

  /// <summary>
  /// The value of the PERCENTAGE attribute.
  /// </summary>
  [JsonPropertyName("percentage")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int Percentage
  {
    get => Get<int?>("percentage") ?? 0;
    set => Set("percentage", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the AVERAGE_REAL attribute.
  /// </summary>
  [JsonPropertyName("averageReal")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 8, Type = MemberType.Number, Length = 15, Precision = 4)]
  public decimal AverageReal
  {
    get => Get<decimal?>("averageReal") ?? 0M;
    set => Set(
      "averageReal", value == 0M ? null : Truncate(value, 4) as decimal?);
  }

  /// <summary>
  /// The value of the AVERAGE_CURRENCY attribute.
  /// </summary>
  [JsonPropertyName("averageCurrency")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 9, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal AverageCurrency
  {
    get => Get<decimal?>("averageCurrency") ?? 0M;
    set => Set(
      "averageCurrency", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the AVERAGE_INTEGER attribute.
  /// </summary>
  [JsonPropertyName("averageInteger")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 9)]
  public int AverageInteger
  {
    get => Get<int?>("averageInteger") ?? 0;
    set => Set("averageInteger", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the FLAG attribute.</summary>
  public const int Flag_MaxLength = 1;

  /// <summary>
  /// The value of the FLAG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Flag_MaxLength)]
  [Value("Y")]
  [Value("N")]
  [Value("?")]
  [Value("+")]
  [Value("S")]
  [Value("")]
  [Value("*")]
  public string Flag
  {
    get => Get<string>("flag") ?? "";
    set => Set(
      "flag", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Flag_MaxLength)));
  }

  /// <summary>
  /// The json value of the Flag attribute.</summary>
  [JsonPropertyName("flag")]
  [Computed]
  public string Flag_Json
  {
    get => NullIf(Flag, "");
    set => Flag = value;
  }

  /// <summary>
  /// The value of the SUBSCRIPT attribute.
  /// </summary>
  [JsonPropertyName("subscript")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 9)]
  public int Subscript
  {
    get => Get<int?>("subscript") ?? 0;
    set => Set("subscript", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the COMMAND attribute.</summary>
  public const int Command_MaxLength = 80;

  /// <summary>
  /// The value of the COMMAND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = Command_MaxLength)]
  public string Command
  {
    get => Get<string>("command") ?? "";
    set => Set(
      "command", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Command_MaxLength)));
  }

  /// <summary>
  /// The json value of the Command attribute.</summary>
  [JsonPropertyName("command")]
  [Computed]
  public string Command_Json
  {
    get => NullIf(Command, "");
    set => Command = value;
  }

  /// <summary>Length of the ACTION_ENTRY attribute.</summary>
  public const int ActionEntry_MaxLength = 2;

  /// <summary>
  /// The value of the ACTION_ENTRY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = ActionEntry_MaxLength)]
  public string ActionEntry
  {
    get => Get<string>("actionEntry") ?? "";
    set => Set(
      "actionEntry", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ActionEntry_MaxLength)));
  }

  /// <summary>
  /// The json value of the ActionEntry attribute.</summary>
  [JsonPropertyName("actionEntry")]
  [Computed]
  public string ActionEntry_Json
  {
    get => NullIf(ActionEntry, "");
    set => ActionEntry = value;
  }

  /// <summary>Length of the SELECT_CHAR attribute.</summary>
  public const int SelectChar_MaxLength = 1;

  /// <summary>
  /// The value of the SELECT_CHAR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = SelectChar_MaxLength)]
  public string SelectChar
  {
    get => Get<string>("selectChar") ?? "";
    set => Set(
      "selectChar", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, SelectChar_MaxLength)));
  }

  /// <summary>
  /// The json value of the SelectChar attribute.</summary>
  [JsonPropertyName("selectChar")]
  [Computed]
  public string SelectChar_Json
  {
    get => NullIf(SelectChar, "");
    set => SelectChar = value;
  }
}
