// The source file: DATE_MMDDYY_ALPHA, ID: 372367164, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class DateMmddyyAlpha: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DateMmddyyAlpha()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DateMmddyyAlpha(DateMmddyyAlpha that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DateMmddyyAlpha Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DateMmddyyAlpha that)
  {
    base.Assign(that);
    alphaMm = that.alphaMm;
    alphaDd = that.alphaDd;
    alphaYy = that.alphaYy;
  }

  /// <summary>Length of the ALPHA_MM attribute.</summary>
  public const int AlphaMm_MaxLength = 2;

  /// <summary>
  /// The value of the ALPHA_MM attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = AlphaMm_MaxLength)]
  public string AlphaMm
  {
    get => alphaMm ?? "";
    set => alphaMm = TrimEnd(Substring(value, 1, AlphaMm_MaxLength));
  }

  /// <summary>
  /// The json value of the AlphaMm attribute.</summary>
  [JsonPropertyName("alphaMm")]
  [Computed]
  public string AlphaMm_Json
  {
    get => NullIf(AlphaMm, "");
    set => AlphaMm = value;
  }

  /// <summary>Length of the ALPHA_DD attribute.</summary>
  public const int AlphaDd_MaxLength = 2;

  /// <summary>
  /// The value of the ALPHA_DD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = AlphaDd_MaxLength)]
  public string AlphaDd
  {
    get => alphaDd ?? "";
    set => alphaDd = TrimEnd(Substring(value, 1, AlphaDd_MaxLength));
  }

  /// <summary>
  /// The json value of the AlphaDd attribute.</summary>
  [JsonPropertyName("alphaDd")]
  [Computed]
  public string AlphaDd_Json
  {
    get => NullIf(AlphaDd, "");
    set => AlphaDd = value;
  }

  /// <summary>Length of the ALPHA_YY attribute.</summary>
  public const int AlphaYy_MaxLength = 2;

  /// <summary>
  /// The value of the ALPHA_YY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = AlphaYy_MaxLength)]
  public string AlphaYy
  {
    get => alphaYy ?? "";
    set => alphaYy = TrimEnd(Substring(value, 1, AlphaYy_MaxLength));
  }

  /// <summary>
  /// The json value of the AlphaYy attribute.</summary>
  [JsonPropertyName("alphaYy")]
  [Computed]
  public string AlphaYy_Json
  {
    get => NullIf(AlphaYy, "");
    set => AlphaYy = value;
  }

  private string alphaMm;
  private string alphaDd;
  private string alphaYy;
}
