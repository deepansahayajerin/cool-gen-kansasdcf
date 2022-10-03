// The source file: LIST_SCREEN_WORK_AREA, ID: 371742786, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: FINANCE
/// Contain common attributes used when working with list screens.
/// </summary>
[Serializable]
public partial class ListScreenWorkArea: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ListScreenWorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ListScreenWorkArea(ListScreenWorkArea that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ListScreenWorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ListScreenWorkArea that)
  {
    base.Assign(that);
    textLine10 = that.textLine10;
    textLine76 = that.textLine76;
  }

  /// <summary>Length of the TEXT_LINE_10 attribute.</summary>
  public const int TextLine10_MaxLength = 10;

  /// <summary>
  /// The value of the TEXT_LINE_10 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = TextLine10_MaxLength)]
  public string TextLine10
  {
    get => textLine10 ?? "";
    set => textLine10 = TrimEnd(Substring(value, 1, TextLine10_MaxLength));
  }

  /// <summary>
  /// The json value of the TextLine10 attribute.</summary>
  [JsonPropertyName("textLine10")]
  [Computed]
  public string TextLine10_Json
  {
    get => NullIf(TextLine10, "");
    set => TextLine10 = value;
  }

  /// <summary>Length of the TEXT_LINE_76 attribute.</summary>
  public const int TextLine76_MaxLength = 76;

  /// <summary>
  /// The value of the TEXT_LINE_76 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = TextLine76_MaxLength)]
  public string TextLine76
  {
    get => textLine76 ?? "";
    set => textLine76 = TrimEnd(Substring(value, 1, TextLine76_MaxLength));
  }

  /// <summary>
  /// The json value of the TextLine76 attribute.</summary>
  [JsonPropertyName("textLine76")]
  [Computed]
  public string TextLine76_Json
  {
    get => NullIf(TextLine76, "");
    set => TextLine76 = value;
  }

  private string textLine10;
  private string textLine76;
}
