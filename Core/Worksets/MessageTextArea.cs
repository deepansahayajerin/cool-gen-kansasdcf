// The source file: MESSAGE_TEXT_AREA, ID: 372724446, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class MessageTextArea: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public MessageTextArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public MessageTextArea(MessageTextArea that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new MessageTextArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(MessageTextArea that)
  {
    base.Assign(that);
    text80 = that.text80;
  }

  /// <summary>Length of the TEXT_80 attribute.</summary>
  public const int Text80_MaxLength = 80;

  /// <summary>
  /// The value of the TEXT_80 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Text80_MaxLength)]
  public string Text80
  {
    get => text80 ?? "";
    set => text80 = TrimEnd(Substring(value, 1, Text80_MaxLength));
  }

  /// <summary>
  /// The json value of the Text80 attribute.</summary>
  [JsonPropertyName("text80")]
  [Computed]
  public string Text80_Json
  {
    get => NullIf(Text80, "");
    set => Text80 = value;
  }

  private string text80;
}
