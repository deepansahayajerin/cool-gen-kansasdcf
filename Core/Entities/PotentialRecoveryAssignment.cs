// The source file: POTENTIAL_RECOVERY_ASSIGNMENT, ID: 371439570, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE					Used to assign Payment Requests to 
/// OFFICE_SERVICE_PROVIDERS based upon rules established by Financial
/// Assignment Rules.
/// </summary>
[Serializable]
public partial class PotentialRecoveryAssignment: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PotentialRecoveryAssignment()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PotentialRecoveryAssignment(PotentialRecoveryAssignment that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PotentialRecoveryAssignment Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PotentialRecoveryAssignment that)
  {
    base.Assign(that);
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    reasonCode = that.reasonCode;
    overrideInd = that.overrideInd;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    ospEffectiveDate = that.ospEffectiveDate;
    ospRoleCode = that.ospRoleCode;
    offId = that.offId;
    spdId = that.spdId;
    prqIdentifier = that.prqIdentifier;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The Person that created the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The Timestamp of the person that Created the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The Person that updated this occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The Timestamp of the person that Last Updated this occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 3;

  /// <summary>
  /// The value of the REASON_CODE attribute.
  /// A three character code field that conveys the reason for the assignment.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = ReasonCode_MaxLength)]
  public string ReasonCode
  {
    get => reasonCode ?? "";
    set => reasonCode = TrimEnd(Substring(value, 1, ReasonCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ReasonCode attribute.</summary>
  [JsonPropertyName("reasonCode")]
  [Computed]
  public string ReasonCode_Json
  {
    get => NullIf(ReasonCode, "");
    set => ReasonCode = value;
  }

  /// <summary>Length of the OVERRIDE_IND attribute.</summary>
  public const int OverrideInd_MaxLength = 1;

  /// <summary>
  /// The value of the OVERRIDE_IND attribute.
  /// A one character code field that controls whether or not the current 
  /// assignment can be reassigned. If the value is Y the override is on and the
  /// assignment may not be discontinued. If the value is N the override is off
  /// and the assignment may be discontinued and the targeted business object (
  /// Payment Request) may be assigned to another Office Service Provider.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = OverrideInd_MaxLength)]
  public string OverrideInd
  {
    get => overrideInd ?? "";
    set => overrideInd = TrimEnd(Substring(value, 1, OverrideInd_MaxLength));
  }

  /// <summary>
  /// The json value of the OverrideInd attribute.</summary>
  [JsonPropertyName("overrideInd")]
  [Computed]
  public string OverrideInd_Json
  {
    get => NullIf(OverrideInd, "");
    set => OverrideInd = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date the assignment is effective.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The date the assignment is no longer effective. The assignment period 
  /// includes the discontinue date. For example, if the discontinue date is 12/
  /// 31/97, the assignment is in effect until 12:00 midnight on that date. At
  /// 01:00 of 01/01/98 the assignment is no longer in effect. Always set to a
  /// value of 2099/12/31 when a new assignment is created and no disc date is
  /// provided.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 8, Type = MemberType.Date)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("ospEffectiveDate")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? OspEffectiveDate
  {
    get => ospEffectiveDate;
    set => ospEffectiveDate = value;
  }

  /// <summary>Length of the ROLE_CODE attribute.</summary>
  public const int OspRoleCode_MaxLength = 2;

  /// <summary>
  /// The value of the ROLE_CODE attribute.
  /// This is the job title or role the person is fulfilling at or for a 
  /// particular location.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = OspRoleCode_MaxLength)]
  public string OspRoleCode
  {
    get => ospRoleCode ?? "";
    set => ospRoleCode = TrimEnd(Substring(value, 1, OspRoleCode_MaxLength));
  }

  /// <summary>
  /// The json value of the OspRoleCode attribute.</summary>
  [JsonPropertyName("ospRoleCode")]
  [Computed]
  public string OspRoleCode_Json
  {
    get => NullIf(OspRoleCode, "");
    set => OspRoleCode = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offId")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 4)]
  public int OffId
  {
    get => offId;
    set => offId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdId")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 5)]
  public int SpdId
  {
    get => spdId;
    set => spdId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated timestamp that distinguishes one occurrence of 
  /// the entity type from another.
  /// </summary>
  [JsonPropertyName("prqIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 9)]
  public int PrqIdentifier
  {
    get => prqIdentifier;
    set => prqIdentifier = value;
  }

  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private string reasonCode;
  private string overrideInd;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private DateTime? ospEffectiveDate;
  private string ospRoleCode;
  private int offId;
  private int spdId;
  private int prqIdentifier;
}
