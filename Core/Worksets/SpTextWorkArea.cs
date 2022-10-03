// The source file: SP_TEXT_WORK_AREA, ID: 371735315, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: SRVPLAN
/// </summary>
[Serializable]
public partial class SpTextWorkArea: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public SpTextWorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public SpTextWorkArea(SpTextWorkArea that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new SpTextWorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(SpTextWorkArea that)
  {
    base.Assign(that);
    text80 = that.text80;
    text20 = that.text20;
    text4 = that.text4;
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

  /// <summary>Length of the TEXT_20 attribute.</summary>
  public const int Text20_MaxLength = 20;

  /// <summary>
  /// The value of the TEXT_20 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Text20_MaxLength)]
  public string Text20
  {
    get => text20 ?? "";
    set => text20 = TrimEnd(Substring(value, 1, Text20_MaxLength));
  }

  /// <summary>
  /// The json value of the Text20 attribute.</summary>
  [JsonPropertyName("text20")]
  [Computed]
  public string Text20_Json
  {
    get => NullIf(Text20, "");
    set => Text20 = value;
  }

  /// <summary>Length of the TEXT_4 attribute.</summary>
  public const int Text4_MaxLength = 4;

  /// <summary>
  /// The value of the TEXT_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Text4_MaxLength)]
  public string Text4
  {
    get => text4 ?? "";
    set => text4 = TrimEnd(Substring(value, 1, Text4_MaxLength));
  }

  /// <summary>
  /// The json value of the Text4 attribute.</summary>
  [JsonPropertyName("text4")]
  [Computed]
  public string Text4_Json
  {
    get => NullIf(Text4, "");
    set => Text4 = value;
  }

  private string text80;
  private string text20;
  private string text4;
}
