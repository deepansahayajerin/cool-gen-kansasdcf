// The source file: STANDARD_CHAR, ID: 371800281, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class StandardChar: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public StandardChar()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public StandardChar(StandardChar that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new StandardChar Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(StandardChar that)
  {
    base.Assign(that);
    command = that.command;
  }

  /// <summary>Length of the COMMAND attribute.</summary>
  public const int Command_MaxLength = 8;

  /// <summary>
  /// The value of the COMMAND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Command_MaxLength)]
  public string Command
  {
    get => command ?? "";
    set => command = TrimEnd(Substring(value, 1, Command_MaxLength));
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

  private string command;
}
