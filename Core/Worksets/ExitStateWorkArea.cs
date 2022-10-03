// The source file: EXIT_STATE_WORK_AREA, ID: 371790900, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ExitStateWorkArea: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ExitStateWorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ExitStateWorkArea(ExitStateWorkArea that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ExitStateWorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ExitStateWorkArea that)
  {
    base.Assign(that);
    message = that.message;
  }

  /// <summary>Length of the MESSAGE attribute.</summary>
  public const int Message_MaxLength = 80;

  /// <summary>
  /// The value of the MESSAGE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Message_MaxLength)]
  public string Message
  {
    get => message ?? "";
    set => message = TrimEnd(Substring(value, 1, Message_MaxLength));
  }

  /// <summary>
  /// The json value of the Message attribute.</summary>
  [JsonPropertyName("message")]
  [Computed]
  public string Message_Json
  {
    get => NullIf(Message, "");
    set => Message = value;
  }

  private string message;
}
