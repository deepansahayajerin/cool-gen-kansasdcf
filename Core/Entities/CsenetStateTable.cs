// The source file: CSENET_STATE_TABLE, ID: 372291376, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  CSNET
/// 
/// Table which holds version and transaction information on each electronic
/// state CSENet system.
/// </summary>
[Serializable]
public partial class CsenetStateTable: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsenetStateTable()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsenetStateTable(CsenetStateTable that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsenetStateTable Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CsenetStateTable that)
  {
    base.Assign(that);
    stateCode = that.stateCode;
    csenetReadyInd = that.csenetReadyInd;
    recStateInd = that.recStateInd;
    quickLocate = that.quickLocate;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
  }

  /// <summary>Length of the STATE_CODE attribute.</summary>
  public const int StateCode_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_CODE attribute.
  /// This field contains the state code of the participation state.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = StateCode_MaxLength)]
  public string StateCode
  {
    get => stateCode ?? "";
    set => stateCode = TrimEnd(Substring(value, 1, StateCode_MaxLength));
  }

  /// <summary>
  /// The json value of the StateCode attribute.</summary>
  [JsonPropertyName("stateCode")]
  [Computed]
  public string StateCode_Json
  {
    get => NullIf(StateCode, "");
    set => StateCode = value;
  }

  /// <summary>Length of the CSENET_READY_IND attribute.</summary>
  public const int CsenetReadyInd_MaxLength = 1;

  /// <summary>
  /// The value of the CSENET_READY_IND attribute.
  /// This is an indicator field (yes/no) as to whether the state is CSENet 
  /// ready.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CsenetReadyInd_MaxLength)]
  public string CsenetReadyInd
  {
    get => csenetReadyInd ?? "";
    set => csenetReadyInd =
      TrimEnd(Substring(value, 1, CsenetReadyInd_MaxLength));
  }

  /// <summary>
  /// The json value of the CsenetReadyInd attribute.</summary>
  [JsonPropertyName("csenetReadyInd")]
  [Computed]
  public string CsenetReadyInd_Json
  {
    get => NullIf(CsenetReadyInd, "");
    set => CsenetReadyInd = value;
  }

  /// <summary>Length of the REC_STATE_IND attribute.</summary>
  public const int RecStateInd_MaxLength = 1;

  /// <summary>
  /// The value of the REC_STATE_IND attribute.
  /// This field is a yes/no field that indicates whether we are sending 
  /// transactions to this state.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = RecStateInd_MaxLength)]
  public string RecStateInd
  {
    get => recStateInd ?? "";
    set => recStateInd = TrimEnd(Substring(value, 1, RecStateInd_MaxLength));
  }

  /// <summary>
  /// The json value of the RecStateInd attribute.</summary>
  [JsonPropertyName("recStateInd")]
  [Computed]
  public string RecStateInd_Json
  {
    get => NullIf(RecStateInd, "");
    set => RecStateInd = value;
  }

  /// <summary>Length of the QUICK_LOCATE attribute.</summary>
  public const int QuickLocate_MaxLength = 1;

  /// <summary>
  /// The value of the QUICK_LOCATE attribute.
  /// This is an indicator field (yes/no) as to whether the state accepts quick 
  /// locate (L01) transactions.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = QuickLocate_MaxLength)]
  public string QuickLocate
  {
    get => quickLocate ?? "";
    set => quickLocate = TrimEnd(Substring(value, 1, QuickLocate_MaxLength));
  }

  /// <summary>
  /// The json value of the QuickLocate attribute.</summary>
  [JsonPropertyName("quickLocate")]
  [Computed]
  public string QuickLocate_Json
  {
    get => NullIf(QuickLocate, "");
    set => QuickLocate = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// This field contains the user id of the last user to update this record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// This field contains the timestamp of the last update of this record.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// This field contains the user id of the person that created this record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => createdBy ?? "";
    set => createdBy = TrimEnd(Substring(value, 1, CreatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the CreatedBy attribute.</summary>
  [JsonPropertyName("createdBy")]
  [Computed]
  public string CreatedBy_Json
  {
    get => NullIf(CreatedBy, "");
    set => CreatedBy = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// This field contains the timestamp value of when this record was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  private string stateCode;
  private string csenetReadyInd;
  private string recStateInd;
  private string quickLocate;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string createdBy;
  private DateTime? createdTimestamp;
}
