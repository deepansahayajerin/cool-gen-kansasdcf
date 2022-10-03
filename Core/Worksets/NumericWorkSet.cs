// The source file: NUMERIC_WORK_SET, ID: 372680736, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class NumericWorkSet: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NumericWorkSet()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NumericWorkSet(NumericWorkSet that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NumericWorkSet Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the NUMBER_1 attribute.
  /// </summary>
  [JsonPropertyName("number1")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 1)]
  public int Number1
  {
    get => Get<int?>("number1") ?? 0;
    set => Set("number1", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the NUMBER_2 attribute.
  /// </summary>
  [JsonPropertyName("number2")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int Number2
  {
    get => Get<int?>("number2") ?? 0;
    set => Set("number2", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the NUMBER_3 attribute.
  /// </summary>
  [JsonPropertyName("number3")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 3)]
  public int Number3
  {
    get => Get<int?>("number3") ?? 0;
    set => Set("number3", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the NUMBER_4 attribute.
  /// </summary>
  [JsonPropertyName("number4")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 4)]
  public int Number4
  {
    get => Get<int?>("number4") ?? 0;
    set => Set("number4", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the NUMBER_5 attribute.
  /// </summary>
  [JsonPropertyName("number5")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 5)]
  public int Number5
  {
    get => Get<int?>("number5") ?? 0;
    set => Set("number5", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the NUMBER_6 attribute.
  /// </summary>
  [JsonPropertyName("number6")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 6)]
  public int Number6
  {
    get => Get<int?>("number6") ?? 0;
    set => Set("number6", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the NUMBER_7 attribute.
  /// </summary>
  [JsonPropertyName("number7")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 7)]
  public int Number7
  {
    get => Get<int?>("number7") ?? 0;
    set => Set("number7", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the NUMBER_7_2 attribute.
  /// </summary>
  [JsonPropertyName("number72")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 8, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal Number72
  {
    get => Get<decimal?>("number72") ?? 0M;
    set => Set("number72", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the NUMBER_8 attribute.
  /// </summary>
  [JsonPropertyName("number8")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 8)]
  public int Number8
  {
    get => Get<int?>("number8") ?? 0;
    set => Set("number8", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the NUMBER_9 attribute.
  /// </summary>
  [JsonPropertyName("number9")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 9)]
  public int Number9
  {
    get => Get<int?>("number9") ?? 0;
    set => Set("number9", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the NUMBER_9_2 attribute.
  /// </summary>
  [JsonPropertyName("number92")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 11, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal Number92
  {
    get => Get<decimal?>("number92") ?? 0M;
    set => Set("number92", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the NUMBER_10 attribute.
  /// </summary>
  [JsonPropertyName("number10")]
  [DefaultValue(0L)]
  [Member(Index = 12, Type = MemberType.Number, Length = 10)]
  public long Number10
  {
    get => Get<long?>("number10") ?? 0L;
    set => Set("number10", value == 0L ? null : value as long?);
  }

  /// <summary>
  /// The value of the NUMBER_11 attribute.
  /// </summary>
  [JsonPropertyName("number11")]
  [DefaultValue(0L)]
  [Member(Index = 13, Type = MemberType.Number, Length = 11)]
  public long Number11
  {
    get => Get<long?>("number11") ?? 0L;
    set => Set("number11", value == 0L ? null : value as long?);
  }

  /// <summary>
  /// The value of the NUMBER_11_2 attribute.
  /// </summary>
  [JsonPropertyName("number112")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 14, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal Number112
  {
    get => Get<decimal?>("number112") ?? 0M;
    set =>
      Set("number112", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the NUMBER_12 attribute.
  /// </summary>
  [JsonPropertyName("number12")]
  [DefaultValue(0L)]
  [Member(Index = 15, Type = MemberType.Number, Length = 12)]
  public long Number12
  {
    get => Get<long?>("number12") ?? 0L;
    set => Set("number12", value == 0L ? null : value as long?);
  }

  /// <summary>
  /// The value of the NUMBER_13 attribute.
  /// </summary>
  [JsonPropertyName("number13")]
  [DefaultValue(0L)]
  [Member(Index = 16, Type = MemberType.Number, Length = 13)]
  public long Number13
  {
    get => Get<long?>("number13") ?? 0L;
    set => Set("number13", value == 0L ? null : value as long?);
  }

  /// <summary>
  /// The value of the NUMBER_14 attribute.
  /// </summary>
  [JsonPropertyName("number14")]
  [DefaultValue(0L)]
  [Member(Index = 17, Type = MemberType.Number, Length = 14)]
  public long Number14
  {
    get => Get<long?>("number14") ?? 0L;
    set => Set("number14", value == 0L ? null : value as long?);
  }

  /// <summary>
  /// The value of the NUMBER_15 attribute.
  /// </summary>
  [JsonPropertyName("number15")]
  [DefaultValue(0L)]
  [Member(Index = 18, Type = MemberType.Number, Length = 15)]
  public long Number15
  {
    get => Get<long?>("number15") ?? 0L;
    set => Set("number15", value == 0L ? null : value as long?);
  }
}
