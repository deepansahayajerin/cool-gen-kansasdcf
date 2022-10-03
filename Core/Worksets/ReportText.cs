// The source file: REPORT_TEXT, ID: 372816783, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ReportText: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ReportText()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ReportText(ReportText that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ReportText Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ReportText that)
  {
    base.Assign(that);
    text15 = that.text15;
    text23 = that.text23;
    text33 = that.text33;
    text59 = that.text59;
  }

  /// <summary>Length of the TEXT15 attribute.</summary>
  public const int Text15_MaxLength = 15;

  /// <summary>
  /// The value of the TEXT15 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Text15_MaxLength)]
  public string Text15
  {
    get => text15 ?? "";
    set => text15 = TrimEnd(Substring(value, 1, Text15_MaxLength));
  }

  /// <summary>
  /// The json value of the Text15 attribute.</summary>
  [JsonPropertyName("text15")]
  [Computed]
  public string Text15_Json
  {
    get => NullIf(Text15, "");
    set => Text15 = value;
  }

  /// <summary>Length of the TEXT23 attribute.</summary>
  public const int Text23_MaxLength = 23;

  /// <summary>
  /// The value of the TEXT23 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Text23_MaxLength)]
  public string Text23
  {
    get => text23 ?? "";
    set => text23 = TrimEnd(Substring(value, 1, Text23_MaxLength));
  }

  /// <summary>
  /// The json value of the Text23 attribute.</summary>
  [JsonPropertyName("text23")]
  [Computed]
  public string Text23_Json
  {
    get => NullIf(Text23, "");
    set => Text23 = value;
  }

  /// <summary>Length of the TEXT33 attribute.</summary>
  public const int Text33_MaxLength = 33;

  /// <summary>
  /// The value of the TEXT33 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Text33_MaxLength)]
  public string Text33
  {
    get => text33 ?? "";
    set => text33 = TrimEnd(Substring(value, 1, Text33_MaxLength));
  }

  /// <summary>
  /// The json value of the Text33 attribute.</summary>
  [JsonPropertyName("text33")]
  [Computed]
  public string Text33_Json
  {
    get => NullIf(Text33, "");
    set => Text33 = value;
  }

  /// <summary>Length of the TEXT59 attribute.</summary>
  public const int Text59_MaxLength = 59;

  /// <summary>
  /// The value of the TEXT59 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Text59_MaxLength)]
  public string Text59
  {
    get => text59 ?? "";
    set => text59 = TrimEnd(Substring(value, 1, Text59_MaxLength));
  }

  /// <summary>
  /// The json value of the Text59 attribute.</summary>
  [JsonPropertyName("text59")]
  [Computed]
  public string Text59_Json
  {
    get => NullIf(Text59, "");
    set => Text59 = value;
  }

  private string text15;
  private string text23;
  private string text33;
  private string text59;
}
