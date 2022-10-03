// The source file: QUICK_PATERNITY_INFO, ID: 374543763, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class QuickPaternityInfo: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickPaternityInfo()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickPaternityInfo(QuickPaternityInfo that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickPaternityInfo Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickPaternityInfo that)
  {
    base.Assign(that);
    patInd = that.patInd;
    patDate = that.patDate;
  }

  /// <summary>Length of the PAT_IND attribute.</summary>
  public const int PatInd_MaxLength = 1;

  /// <summary>
  /// The value of the PAT_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = PatInd_MaxLength)]
  public string PatInd
  {
    get => patInd ?? "";
    set => patInd = TrimEnd(Substring(value, 1, PatInd_MaxLength));
  }

  /// <summary>
  /// The json value of the PatInd attribute.</summary>
  [JsonPropertyName("patInd")]
  [Computed]
  public string PatInd_Json
  {
    get => NullIf(PatInd, "");
    set => PatInd = value;
  }

  /// <summary>Length of the PAT_DATE attribute.</summary>
  public const int PatDate_MaxLength = 8;

  /// <summary>
  /// The value of the PAT_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = PatDate_MaxLength)]
  public string PatDate
  {
    get => patDate ?? "";
    set => patDate = TrimEnd(Substring(value, 1, PatDate_MaxLength));
  }

  /// <summary>
  /// The json value of the PatDate attribute.</summary>
  [JsonPropertyName("patDate")]
  [Computed]
  public string PatDate_Json
  {
    get => NullIf(PatDate, "");
    set => PatDate = value;
  }

  private string patInd;
  private string patDate;
}
