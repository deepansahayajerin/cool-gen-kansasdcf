// The source file: GLOBAL_REASSIGNMENT, ID: 371434945, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN					Each occurrence contains the specifications for the batch 
/// processing based reassignment of assignable business objects from one Office
/// Service Provider to another Office Service Provider.
/// </summary>
[Serializable]
public partial class GlobalReassignment: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public GlobalReassignment()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public GlobalReassignment(GlobalReassignment that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new GlobalReassignment Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(GlobalReassignment that)
  {
    base.Assign(that);
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    processDate = that.processDate;
    statusFlag = that.statusFlag;
    overrideFlag = that.overrideFlag;
    businessObjectCode = that.businessObjectCode;
    assignmentReasonCode = that.assignmentReasonCode;
    boCount = that.boCount;
    monCount = that.monCount;
    ospEffectiveDat = that.ospEffectiveDat;
    ospRoleCod = that.ospRoleCod;
    offGeneratedId1 = that.offGeneratedId1;
    spdGeneratedId1 = that.spdGeneratedId1;
    ospEffectiveDate = that.ospEffectiveDate;
    ospRoleCode = that.ospRoleCode;
    offGeneratedId = that.offGeneratedId;
    spdGeneratedId = that.spdGeneratedId;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The timestamp for when this occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 1, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user Id responsible for the creation of this occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The User Id of the last user to update this occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 3, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// The timestamp for when this occurrence was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the PROCESS_DATE attribute.
  /// The date that this occurrence should be processed by batch processing.
  /// </summary>
  [JsonPropertyName("processDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? ProcessDate
  {
    get => processDate;
    set => processDate = value;
  }

  /// <summary>Length of the STATUS_FLAG attribute.</summary>
  public const int StatusFlag_MaxLength = 1;

  /// <summary>
  /// The value of the STATUS_FLAG attribute.
  /// An attribute that identifies to relevant processing what the status of 
  /// this occurrence is.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = StatusFlag_MaxLength)]
  public string StatusFlag
  {
    get => statusFlag ?? "";
    set => statusFlag = TrimEnd(Substring(value, 1, StatusFlag_MaxLength));
  }

  /// <summary>
  /// The json value of the StatusFlag attribute.</summary>
  [JsonPropertyName("statusFlag")]
  [Computed]
  public string StatusFlag_Json
  {
    get => NullIf(StatusFlag, "");
    set => StatusFlag = value;
  }

  /// <summary>Length of the OVERRIDE_FLAG attribute.</summary>
  public const int OverrideFlag_MaxLength = 1;

  /// <summary>
  /// The value of the OVERRIDE_FLAG attribute.
  /// A Yes/No indicator that instructs relevant processing to either ignore or 
  /// obey business object assignment occurrence override indicators.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = OverrideFlag_MaxLength)]
  public string OverrideFlag
  {
    get => overrideFlag ?? "";
    set => overrideFlag = TrimEnd(Substring(value, 1, OverrideFlag_MaxLength));
  }

  /// <summary>
  /// The json value of the OverrideFlag attribute.</summary>
  [JsonPropertyName("overrideFlag")]
  [Computed]
  public string OverrideFlag_Json
  {
    get => NullIf(OverrideFlag, "");
    set => OverrideFlag = value;
  }

  /// <summary>Length of the BUSINESS_OBJECT_CODE attribute.</summary>
  public const int BusinessObjectCode_MaxLength = 3;

  /// <summary>
  /// The value of the BUSINESS_OBJECT_CODE attribute.
  /// A code that identifies the type of assignable business object to be 
  /// reassigned during global reassignment batch processing.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = BusinessObjectCode_MaxLength)]
  public string BusinessObjectCode
  {
    get => businessObjectCode ?? "";
    set => businessObjectCode =
      TrimEnd(Substring(value, 1, BusinessObjectCode_MaxLength));
  }

  /// <summary>
  /// The json value of the BusinessObjectCode attribute.</summary>
  [JsonPropertyName("businessObjectCode")]
  [Computed]
  public string BusinessObjectCode_Json
  {
    get => NullIf(BusinessObjectCode, "");
    set => BusinessObjectCode = value;
  }

  /// <summary>Length of the ASSIGNMENT_REASON_CODE attribute.</summary>
  public const int AssignmentReasonCode_MaxLength = 3;

  /// <summary>
  /// The value of the ASSIGNMENT_REASON_CODE attribute.
  /// A code that identifies the type of existing assignments that qualify for 
  /// reassignment for the assignable business object to be reassigned during
  /// global reassignment batch processing.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = AssignmentReasonCode_MaxLength)]
  public string AssignmentReasonCode
  {
    get => assignmentReasonCode ?? "";
    set => assignmentReasonCode =
      TrimEnd(Substring(value, 1, AssignmentReasonCode_MaxLength));
  }

  /// <summary>
  /// The json value of the AssignmentReasonCode attribute.</summary>
  [JsonPropertyName("assignmentReasonCode")]
  [Computed]
  public string AssignmentReasonCode_Json
  {
    get => NullIf(AssignmentReasonCode, "");
    set => AssignmentReasonCode = value;
  }

  /// <summary>
  /// The value of the BO_COUNT attribute.
  /// The number of business objects reassigned by this occurrence through batch
  /// processing.
  /// </summary>
  [JsonPropertyName("boCount")]
  [Member(Index = 10, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? BoCount
  {
    get => boCount;
    set => boCount = value;
  }

  /// <summary>
  /// The value of the MON_COUNT attribute.
  /// The number of monitored activities, reassigned by this occurrence through 
  /// batch processing.
  /// </summary>
  [JsonPropertyName("monCount")]
  [Member(Index = 11, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? MonCount
  {
    get => monCount;
    set => monCount = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("ospEffectiveDat")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? OspEffectiveDat
  {
    get => ospEffectiveDat;
    set => ospEffectiveDat = value;
  }

  /// <summary>Length of the ROLE_CODE attribute.</summary>
  public const int OspRoleCod_MaxLength = 2;

  /// <summary>
  /// The value of the ROLE_CODE attribute.
  /// This is the job title or role the person is fulfilling at or for a 
  /// particular location.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// </summary>
  [JsonPropertyName("ospRoleCod")]
  [Member(Index = 13, Type = MemberType.Char, Length = OspRoleCod_MaxLength, Optional
    = true)]
  public string OspRoleCod
  {
    get => ospRoleCod;
    set => ospRoleCod = value != null
      ? TrimEnd(Substring(value, 1, OspRoleCod_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offGeneratedId1")]
  [Member(Index = 14, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffGeneratedId1
  {
    get => offGeneratedId1;
    set => offGeneratedId1 = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdGeneratedId1")]
  [Member(Index = 15, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? SpdGeneratedId1
  {
    get => spdGeneratedId1;
    set => spdGeneratedId1 = value;
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
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
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
  [JsonPropertyName("ospRoleCode")]
  [Member(Index = 17, Type = MemberType.Char, Length = OspRoleCode_MaxLength, Optional
    = true)]
  public string OspRoleCode
  {
    get => ospRoleCode;
    set => ospRoleCode = value != null
      ? TrimEnd(Substring(value, 1, OspRoleCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offGeneratedId")]
  [Member(Index = 18, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffGeneratedId
  {
    get => offGeneratedId;
    set => offGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdGeneratedId")]
  [Member(Index = 19, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? SpdGeneratedId
  {
    get => spdGeneratedId;
    set => spdGeneratedId = value;
  }

  private DateTime? createdTimestamp;
  private string createdBy;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private DateTime? processDate;
  private string statusFlag;
  private string overrideFlag;
  private string businessObjectCode;
  private string assignmentReasonCode;
  private int? boCount;
  private int? monCount;
  private DateTime? ospEffectiveDat;
  private string ospRoleCod;
  private int? offGeneratedId1;
  private int? spdGeneratedId1;
  private DateTime? ospEffectiveDate;
  private string ospRoleCode;
  private int? offGeneratedId;
  private int? spdGeneratedId;
}
