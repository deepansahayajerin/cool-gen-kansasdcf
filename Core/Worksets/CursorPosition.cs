// The source file: CURSOR_POSITION, ID: 371457778, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// Contains the attributes used by the external action block 
/// EAB_GET_CURSOR_POSITION.
/// </summary>
[Serializable]
public partial class CursorPosition: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CursorPosition()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CursorPosition(CursorPosition that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CursorPosition Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CursorPosition that)
  {
    base.Assign(that);
    row = that.row;
    column = that.column;
  }

  /// <summary>
  /// The value of the ROW attribute.
  /// </summary>
  [JsonPropertyName("row")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 2)]
  public int Row
  {
    get => row;
    set => row = value;
  }

  /// <summary>
  /// The value of the COLUMN attribute.
  /// </summary>
  [JsonPropertyName("column")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int Column
  {
    get => column;
    set => column = value;
  }

  private int row;
  private int column;
}
