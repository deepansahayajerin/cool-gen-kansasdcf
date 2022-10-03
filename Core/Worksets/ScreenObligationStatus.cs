// The source file: SCREEN_OBLIGATION_STATUS, ID: 371740943, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This workset will be used by the Get Obligation Status AB and will display a
/// one character status of &quot;A&quot; (active) or &quot;D&quot; (deactived
/// ).
/// </summary>
[Serializable]
public partial class ScreenObligationStatus: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ScreenObligationStatus()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ScreenObligationStatus(ScreenObligationStatus that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ScreenObligationStatus Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ScreenObligationStatus that)
  {
    base.Assign(that);
    obligationStatusTxt = that.obligationStatusTxt;
    obligationStatus = that.obligationStatus;
  }

  /// <summary>Length of the OBLIGATION_STATUS_TXT attribute.</summary>
  public const int ObligationStatusTxt_MaxLength = 10;

  /// <summary>
  /// The value of the OBLIGATION_STATUS_TXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = ObligationStatusTxt_MaxLength)]
  public string ObligationStatusTxt
  {
    get => obligationStatusTxt ?? "";
    set => obligationStatusTxt =
      TrimEnd(Substring(value, 1, ObligationStatusTxt_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligationStatusTxt attribute.</summary>
  [JsonPropertyName("obligationStatusTxt")]
  [Computed]
  public string ObligationStatusTxt_Json
  {
    get => NullIf(ObligationStatusTxt, "");
    set => ObligationStatusTxt = value;
  }

  /// <summary>Length of the OBLIGATION_STATUS attribute.</summary>
  public const int ObligationStatus_MaxLength = 1;

  /// <summary>
  /// The value of the OBLIGATION_STATUS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ObligationStatus_MaxLength)
    ]
  public string ObligationStatus
  {
    get => obligationStatus ?? "";
    set => obligationStatus =
      TrimEnd(Substring(value, 1, ObligationStatus_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligationStatus attribute.</summary>
  [JsonPropertyName("obligationStatus")]
  [Computed]
  public string ObligationStatus_Json
  {
    get => NullIf(ObligationStatus, "");
    set => ObligationStatus = value;
  }

  private string obligationStatusTxt;
  private string obligationStatus;
}
