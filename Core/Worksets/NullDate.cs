// The source file: NULL_DATE, ID: 371740921, model: 746.
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class NullDate: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NullDate()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NullDate(NullDate that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NullDate Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NullDate that)
  {
    base.Assign(that);
    date = that.date;
  }

  /// <summary>
  /// The value of the DATE attribute.
  /// </summary>
  [JsonPropertyName("date")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? Date
  {
    get => date;
    set => date = value;
  }

  private DateTime? date;
}
