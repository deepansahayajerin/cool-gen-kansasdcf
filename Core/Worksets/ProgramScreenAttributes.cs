// The source file: PROGRAM_SCREEN_ATTRIBUTES, ID: 371738570, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ProgramScreenAttributes: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ProgramScreenAttributes()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ProgramScreenAttributes(ProgramScreenAttributes that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ProgramScreenAttributes Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ProgramScreenAttributes that)
  {
    base.Assign(that);
    programTypeInd = that.programTypeInd;
  }

  /// <summary>Length of the PROGRAM_TYPE_IND attribute.</summary>
  public const int ProgramTypeInd_MaxLength = 5;

  /// <summary>
  /// The value of the PROGRAM_TYPE_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ProgramTypeInd_MaxLength)]
  public string ProgramTypeInd
  {
    get => programTypeInd ?? "";
    set => programTypeInd =
      TrimEnd(Substring(value, 1, ProgramTypeInd_MaxLength));
  }

  /// <summary>
  /// The json value of the ProgramTypeInd attribute.</summary>
  [JsonPropertyName("programTypeInd")]
  [Computed]
  public string ProgramTypeInd_Json
  {
    get => NullIf(ProgramTypeInd, "");
    set => ProgramTypeInd = value;
  }

  private string programTypeInd;
}
