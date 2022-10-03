// The source file: WORK_DECIMAL_NUMBERS, ID: 371413619, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class WorkDecimalNumbers: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public WorkDecimalNumbers()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public WorkDecimalNumbers(WorkDecimalNumbers that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new WorkDecimalNumbers Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(WorkDecimalNumbers that)
  {
    base.Assign(that);
    number31 = that.number31;
  }

  /// <summary>
  /// The value of the NUMBER_3_1 attribute.
  /// </summary>
  [JsonPropertyName("number31")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 1, Type = MemberType.Number, Length = 4, Precision = 1)]
  public decimal Number31
  {
    get => number31;
    set => number31 = Truncate(value, 1);
  }

  private decimal number31;
}
