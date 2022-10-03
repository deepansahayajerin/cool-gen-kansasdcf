// The source file: CRD_CR_COMBO_NO, ID: 371872377, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class CrdCrComboNo: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CrdCrComboNo()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CrdCrComboNo(CrdCrComboNo that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CrdCrComboNo Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CrdCrComboNo that)
  {
    base.Assign(that);
    crdCrCombo = that.crdCrCombo;
  }

  /// <summary>Length of the CRD_CR_COMBO attribute.</summary>
  public const int CrdCrCombo_MaxLength = 14;

  /// <summary>
  /// The value of the CRD_CR_COMBO attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CrdCrCombo_MaxLength)]
  public string CrdCrCombo
  {
    get => crdCrCombo ?? "";
    set => crdCrCombo = TrimEnd(Substring(value, 1, CrdCrCombo_MaxLength));
  }

  /// <summary>
  /// The json value of the CrdCrCombo attribute.</summary>
  [JsonPropertyName("crdCrCombo")]
  [Computed]
  public string CrdCrCombo_Json
  {
    get => NullIf(CrdCrCombo, "");
    set => CrdCrCombo = value;
  }

  private string crdCrCombo;
}
