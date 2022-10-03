// The source file: FDSO_RETURN_H_TOTAL, ID: 372668496, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class FdsoReturnHTotal: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FdsoReturnHTotal()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FdsoReturnHTotal(FdsoReturnHTotal that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FdsoReturnHTotal Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FdsoReturnHTotal that)
  {
    base.Assign(that);
    submittingState = that.submittingState;
    control = that.control;
    tanfAccepted = that.tanfAccepted;
    tanfRejected = that.tanfRejected;
    nontanfAccepted = that.nontanfAccepted;
    nontanfRejected = that.nontanfRejected;
  }

  /// <summary>Length of the SUBMITTING_STATE attribute.</summary>
  public const int SubmittingState_MaxLength = 2;

  /// <summary>
  /// The value of the SUBMITTING_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = SubmittingState_MaxLength)]
    
  public string SubmittingState
  {
    get => submittingState ?? "";
    set => submittingState =
      TrimEnd(Substring(value, 1, SubmittingState_MaxLength));
  }

  /// <summary>
  /// The json value of the SubmittingState attribute.</summary>
  [JsonPropertyName("submittingState")]
  [Computed]
  public string SubmittingState_Json
  {
    get => NullIf(SubmittingState, "");
    set => SubmittingState = value;
  }

  /// <summary>Length of the CONTROL attribute.</summary>
  public const int Control_MaxLength = 3;

  /// <summary>
  /// The value of the CONTROL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Control_MaxLength)]
  public string Control
  {
    get => control ?? "";
    set => control = TrimEnd(Substring(value, 1, Control_MaxLength));
  }

  /// <summary>
  /// The json value of the Control attribute.</summary>
  [JsonPropertyName("control")]
  [Computed]
  public string Control_Json
  {
    get => NullIf(Control, "");
    set => Control = value;
  }

  /// <summary>
  /// The value of the TANF_ACCEPTED attribute.
  /// </summary>
  [JsonPropertyName("tanfAccepted")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 9)]
  public int TanfAccepted
  {
    get => tanfAccepted;
    set => tanfAccepted = value;
  }

  /// <summary>
  /// The value of the TANF_REJECTED attribute.
  /// </summary>
  [JsonPropertyName("tanfRejected")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 9)]
  public int TanfRejected
  {
    get => tanfRejected;
    set => tanfRejected = value;
  }

  /// <summary>
  /// The value of the NONTANF_ACCEPTED attribute.
  /// </summary>
  [JsonPropertyName("nontanfAccepted")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 9)]
  public int NontanfAccepted
  {
    get => nontanfAccepted;
    set => nontanfAccepted = value;
  }

  /// <summary>
  /// The value of the NONTANF_REJECTED attribute.
  /// </summary>
  [JsonPropertyName("nontanfRejected")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 9)]
  public int NontanfRejected
  {
    get => nontanfRejected;
    set => nontanfRejected = value;
  }

  private string submittingState;
  private string control;
  private int tanfAccepted;
  private int tanfRejected;
  private int nontanfAccepted;
  private int nontanfRejected;
}
