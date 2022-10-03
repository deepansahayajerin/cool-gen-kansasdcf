// The source file: FDSO_CERTIFICATION_TOTAL_RECORD, ID: 372668459, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// FDSO certification total record
/// Used as trailer record containing total counts and amounts.
/// </summary>
[Serializable]
public partial class FdsoCertificationTotalRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FdsoCertificationTotalRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FdsoCertificationTotalRecord(FdsoCertificationTotalRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FdsoCertificationTotalRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FdsoCertificationTotalRecord that)
  {
    base.Assign(that);
    submittingState = that.submittingState;
    control = that.control;
    tanfCount = that.tanfCount;
    nontanfCount = that.nontanfCount;
    tanfAmount = that.tanfAmount;
    nontanfAmount = that.nontanfAmount;
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
  /// The value of the TANF_COUNT attribute.
  /// </summary>
  [JsonPropertyName("tanfCount")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 9)]
  public int TanfCount
  {
    get => tanfCount;
    set => tanfCount = value;
  }

  /// <summary>
  /// The value of the NONTANF_COUNT attribute.
  /// </summary>
  [JsonPropertyName("nontanfCount")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 9)]
  public int NontanfCount
  {
    get => nontanfCount;
    set => nontanfCount = value;
  }

  /// <summary>
  /// The value of the TANF_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("tanfAmount")]
  [DefaultValue(0L)]
  [Member(Index = 5, Type = MemberType.Number, Length = 11)]
  public long TanfAmount
  {
    get => tanfAmount;
    set => tanfAmount = value;
  }

  /// <summary>
  /// The value of the NONTANF_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("nontanfAmount")]
  [DefaultValue(0L)]
  [Member(Index = 6, Type = MemberType.Number, Length = 11)]
  public long NontanfAmount
  {
    get => nontanfAmount;
    set => nontanfAmount = value;
  }

  private string submittingState;
  private string control;
  private int tanfCount;
  private int nontanfCount;
  private long tanfAmount;
  private long nontanfAmount;
}
