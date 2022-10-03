// The source file: QUICK_ORDER_ESTAB_INFO, ID: 374543759, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class QuickOrderEstabInfo: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickOrderEstabInfo()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickOrderEstabInfo(QuickOrderEstabInfo that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickOrderEstabInfo Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickOrderEstabInfo that)
  {
    base.Assign(that);
    oeInd = that.oeInd;
    oeDate = that.oeDate;
  }

  /// <summary>Length of the OE_IND attribute.</summary>
  public const int OeInd_MaxLength = 1;

  /// <summary>
  /// The value of the OE_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = OeInd_MaxLength)]
  public string OeInd
  {
    get => oeInd ?? "";
    set => oeInd = TrimEnd(Substring(value, 1, OeInd_MaxLength));
  }

  /// <summary>
  /// The json value of the OeInd attribute.</summary>
  [JsonPropertyName("oeInd")]
  [Computed]
  public string OeInd_Json
  {
    get => NullIf(OeInd, "");
    set => OeInd = value;
  }

  /// <summary>Length of the OE_DATE attribute.</summary>
  public const int OeDate_MaxLength = 8;

  /// <summary>
  /// The value of the OE_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = OeDate_MaxLength)]
  public string OeDate
  {
    get => oeDate ?? "";
    set => oeDate = TrimEnd(Substring(value, 1, OeDate_MaxLength));
  }

  /// <summary>
  /// The json value of the OeDate attribute.</summary>
  [JsonPropertyName("oeDate")]
  [Computed]
  public string OeDate_Json
  {
    get => NullIf(OeDate, "");
    set => OeDate = value;
  }

  private string oeInd;
  private string oeDate;
}
