// The source file: REPORT_TOTALS, ID: 372816784, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ReportTotals: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ReportTotals()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ReportTotals(ReportTotals that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ReportTotals Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ReportTotals that)
  {
    base.Assign(that);
    number120 = that.number120;
    currency152 = that.currency152;
    currency132 = that.currency132;
    number90 = that.number90;
  }

  /// <summary>
  /// The value of the NUMBER_12_0 attribute.
  /// </summary>
  [JsonPropertyName("number120")]
  [DefaultValue(0L)]
  [Member(Index = 1, Type = MemberType.Number, Length = 12)]
  public long Number120
  {
    get => number120;
    set => number120 = value;
  }

  /// <summary>
  /// The value of the CURRENCY_15_2 attribute.
  /// </summary>
  [JsonPropertyName("currency152")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 15, Precision = 2)]
  public decimal Currency152
  {
    get => currency152;
    set => currency152 = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the CURRENCY_13_2 attribute.
  /// </summary>
  [JsonPropertyName("currency132")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 13, Precision = 2)]
  public decimal Currency132
  {
    get => currency132;
    set => currency132 = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NUMBER_9_0 attribute.
  /// </summary>
  [JsonPropertyName("number90")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 9)]
  public int Number90
  {
    get => number90;
    set => number90 = value;
  }

  private long number120;
  private decimal currency152;
  private decimal currency132;
  private int number90;
}
