// The source file: COURT_CAPTION, ID: 371432745, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// Information that is used for the header of each legal action of a court 
/// case.
/// </summary>
[Serializable]
public partial class CourtCaption: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CourtCaption()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CourtCaption(CourtCaption that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CourtCaption Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CourtCaption that)
  {
    base.Assign(that);
    number = that.number;
    line = that.line;
    lgaIdentifier = that.lgaIdentifier;
  }

  /// <summary>
  /// The value of the NUMBER attribute.
  /// Sequential number assigned for each line of the caption.
  /// </summary>
  [JsonPropertyName("number")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 2)]
  public int Number
  {
    get => number;
    set => number = value;
  }

  /// <summary>Length of the LINE attribute.</summary>
  public const int Line_MaxLength = 40;

  /// <summary>
  /// The value of the LINE attribute.
  /// The individual occurrence of the court caption line.
  /// </summary>
  [JsonPropertyName("line")]
  [Member(Index = 2, Type = MemberType.Varchar, Length = Line_MaxLength, Optional
    = true)]
  public string Line
  {
    get => line;
    set => line = value != null ? Substring(value, 1, Line_MaxLength) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 9)]
  public int LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  private int number;
  private string line;
  private int lgaIdentifier;
}
