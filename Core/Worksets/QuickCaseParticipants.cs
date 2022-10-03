// The source file: QUICK_CASE_PARTICIPANTS, ID: 374543703, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class QuickCaseParticipants: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickCaseParticipants()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickCaseParticipants(QuickCaseParticipants that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickCaseParticipants Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickCaseParticipants that)
  {
    base.Assign(that);
    requestingStateCaseId = that.requestingStateCaseId;
    caseStatus = that.caseStatus;
  }

  /// <summary>Length of the REQUESTING_STATE_CASE_ID attribute.</summary>
  public const int RequestingStateCaseId_MaxLength = 15;

  /// <summary>
  /// The value of the REQUESTING_STATE_CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = RequestingStateCaseId_MaxLength)]
  public string RequestingStateCaseId
  {
    get => requestingStateCaseId ?? "";
    set => requestingStateCaseId =
      TrimEnd(Substring(value, 1, RequestingStateCaseId_MaxLength));
  }

  /// <summary>
  /// The json value of the RequestingStateCaseId attribute.</summary>
  [JsonPropertyName("requestingStateCaseId")]
  [Computed]
  public string RequestingStateCaseId_Json
  {
    get => NullIf(RequestingStateCaseId, "");
    set => RequestingStateCaseId = value;
  }

  /// <summary>Length of the CASE_STATUS attribute.</summary>
  public const int CaseStatus_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_STATUS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CaseStatus_MaxLength)]
  public string CaseStatus
  {
    get => caseStatus ?? "";
    set => caseStatus = TrimEnd(Substring(value, 1, CaseStatus_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseStatus attribute.</summary>
  [JsonPropertyName("caseStatus")]
  [Computed]
  public string CaseStatus_Json
  {
    get => NullIf(CaseStatus, "");
    set => CaseStatus = value;
  }

  private string requestingStateCaseId;
  private string caseStatus;
}
