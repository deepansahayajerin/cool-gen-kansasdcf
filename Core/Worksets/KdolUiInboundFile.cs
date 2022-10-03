// The source file: KDOL_UI_INBOUND_FILE, ID: 945102650, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class KdolUiInboundFile: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public KdolUiInboundFile()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public KdolUiInboundFile(KdolUiInboundFile that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new KdolUiInboundFile Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(KdolUiInboundFile that)
  {
    base.Assign(that);
    uiWithholdingRecord = that.uiWithholdingRecord;
    newClaimantRecord = that.newClaimantRecord;
  }

  /// <summary>Length of the UI_WITHHOLDING_RECORD attribute.</summary>
  public const int UiWithholdingRecord_MaxLength = 150;

  /// <summary>
  /// The value of the UI_WITHHOLDING_RECORD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = UiWithholdingRecord_MaxLength)]
  public string UiWithholdingRecord
  {
    get => uiWithholdingRecord ?? "";
    set => uiWithholdingRecord =
      TrimEnd(Substring(value, 1, UiWithholdingRecord_MaxLength));
  }

  /// <summary>
  /// The json value of the UiWithholdingRecord attribute.</summary>
  [JsonPropertyName("uiWithholdingRecord")]
  [Computed]
  public string UiWithholdingRecord_Json
  {
    get => NullIf(UiWithholdingRecord, "");
    set => UiWithholdingRecord = value;
  }

  /// <summary>Length of the NEW_CLAIMANT_RECORD attribute.</summary>
  public const int NewClaimantRecord_MaxLength = 107;

  /// <summary>
  /// The value of the NEW_CLAIMANT_RECORD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = NewClaimantRecord_MaxLength)]
  public string NewClaimantRecord
  {
    get => newClaimantRecord ?? "";
    set => newClaimantRecord =
      TrimEnd(Substring(value, 1, NewClaimantRecord_MaxLength));
  }

  /// <summary>
  /// The json value of the NewClaimantRecord attribute.</summary>
  [JsonPropertyName("newClaimantRecord")]
  [Computed]
  public string NewClaimantRecord_Json
  {
    get => NullIf(NewClaimantRecord, "");
    set => NewClaimantRecord = value;
  }

  private string uiWithholdingRecord;
  private string newClaimantRecord;
}
