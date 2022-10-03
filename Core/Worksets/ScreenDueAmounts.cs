// The source file: SCREEN_DUE_AMOUNTS, ID: 371740924, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: FINANCE
/// This workset will allows a common set of attributes to be displayed to the 
/// screen to represent amounts due.  The amounts will be calculated in an AB.
/// </summary>
[Serializable]
public partial class ScreenDueAmounts: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ScreenDueAmounts()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ScreenDueAmounts(ScreenDueAmounts that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ScreenDueAmounts Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ScreenDueAmounts that)
  {
    base.Assign(that);
    currentAmountDue = that.currentAmountDue;
    periodicAmountDue = that.periodicAmountDue;
    totalAmountDue = that.totalAmountDue;
  }

  /// <summary>
  /// The value of the CURRENT_AMOUNT_DUE attribute.
  /// </summary>
  [JsonPropertyName("currentAmountDue")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 1, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal CurrentAmountDue
  {
    get => currentAmountDue;
    set => currentAmountDue = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the PERIODIC_AMOUNT_DUE attribute.
  /// </summary>
  [JsonPropertyName("periodicAmountDue")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal PeriodicAmountDue
  {
    get => periodicAmountDue;
    set => periodicAmountDue = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_AMOUNT_DUE attribute.
  /// </summary>
  [JsonPropertyName("totalAmountDue")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal TotalAmountDue
  {
    get => totalAmountDue;
    set => totalAmountDue = Truncate(value, 2);
  }

  private decimal currentAmountDue;
  private decimal periodicAmountDue;
  private decimal totalAmountDue;
}
