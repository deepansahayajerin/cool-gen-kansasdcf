// The source file: STARS_VOUCHER_TOTALS, ID: 372881056, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class StarsVoucherTotals: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public StarsVoucherTotals()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public StarsVoucherTotals(StarsVoucherTotals that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new StarsVoucherTotals Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(StarsVoucherTotals that)
  {
    base.Assign(that);
    excessUra = that.excessUra;
    interstate = that.interstate;
    refunds = that.refunds;
    passthru = that.passthru;
    nonAfdc = that.nonAfdc;
    voucherTotal = that.voucherTotal;
    error = that.error;
    zdelBadCheck = that.zdelBadCheck;
  }

  /// <summary>
  /// The value of the EXCESS_URA attribute.
  /// </summary>
  [JsonPropertyName("excessUra")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 1, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal ExcessUra
  {
    get => excessUra;
    set => excessUra = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the INTERSTATE attribute.
  /// </summary>
  [JsonPropertyName("interstate")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal Interstate
  {
    get => interstate;
    set => interstate = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the REFUNDS attribute.
  /// </summary>
  [JsonPropertyName("refunds")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal Refunds
  {
    get => refunds;
    set => refunds = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the PASSTHRU attribute.
  /// </summary>
  [JsonPropertyName("passthru")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 4, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal Passthru
  {
    get => passthru;
    set => passthru = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NON_AFDC attribute.
  /// </summary>
  [JsonPropertyName("nonAfdc")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal NonAfdc
  {
    get => nonAfdc;
    set => nonAfdc = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the VOUCHER_TOTAL attribute.
  /// </summary>
  [JsonPropertyName("voucherTotal")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal VoucherTotal
  {
    get => voucherTotal;
    set => voucherTotal = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ERROR attribute.
  /// </summary>
  [JsonPropertyName("error")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal Error
  {
    get => error;
    set => error = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ZDEL_BAD_CHECK attribute.
  /// </summary>
  [JsonPropertyName("zdelBadCheck")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal ZdelBadCheck
  {
    get => zdelBadCheck;
    set => zdelBadCheck = Truncate(value, 2);
  }

  private decimal excessUra;
  private decimal interstate;
  private decimal refunds;
  private decimal passthru;
  private decimal nonAfdc;
  private decimal voucherTotal;
  private decimal error;
  private decimal zdelBadCheck;
}
