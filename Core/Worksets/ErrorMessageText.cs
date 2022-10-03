// The source file: ERROR_MESSAGE_TEXT, ID: 371740917, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// Different length of error message
/// </summary>
[Serializable]
public partial class ErrorMessageText: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ErrorMessageText()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ErrorMessageText(ErrorMessageText that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ErrorMessageText Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ErrorMessageText that)
  {
    base.Assign(that);
    msg80 = that.msg80;
    msg40 = that.msg40;
    msg20 = that.msg20;
  }

  /// <summary>Length of the MSG_80 attribute.</summary>
  public const int Msg80_MaxLength = 80;

  /// <summary>
  /// The value of the MSG_80 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Msg80_MaxLength)]
  public string Msg80
  {
    get => msg80 ?? "";
    set => msg80 = TrimEnd(Substring(value, 1, Msg80_MaxLength));
  }

  /// <summary>
  /// The json value of the Msg80 attribute.</summary>
  [JsonPropertyName("msg80")]
  [Computed]
  public string Msg80_Json
  {
    get => NullIf(Msg80, "");
    set => Msg80 = value;
  }

  /// <summary>Length of the MSG_40 attribute.</summary>
  public const int Msg40_MaxLength = 40;

  /// <summary>
  /// The value of the MSG_40 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Msg40_MaxLength)]
  public string Msg40
  {
    get => msg40 ?? "";
    set => msg40 = TrimEnd(Substring(value, 1, Msg40_MaxLength));
  }

  /// <summary>
  /// The json value of the Msg40 attribute.</summary>
  [JsonPropertyName("msg40")]
  [Computed]
  public string Msg40_Json
  {
    get => NullIf(Msg40, "");
    set => Msg40 = value;
  }

  /// <summary>Length of the MSG_20 attribute.</summary>
  public const int Msg20_MaxLength = 20;

  /// <summary>
  /// The value of the MSG_20 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Msg20_MaxLength)]
  public string Msg20
  {
    get => msg20 ?? "";
    set => msg20 = TrimEnd(Substring(value, 1, Msg20_MaxLength));
  }

  /// <summary>
  /// The json value of the Msg20 attribute.</summary>
  [JsonPropertyName("msg20")]
  [Computed]
  public string Msg20_Json
  {
    get => NullIf(Msg20, "");
    set => Msg20 = value;
  }

  private string msg80;
  private string msg40;
  private string msg20;
}
