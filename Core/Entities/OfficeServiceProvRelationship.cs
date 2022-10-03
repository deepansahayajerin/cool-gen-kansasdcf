// The source file: OFFICE_SERVICE_PROV_RELATIONSHIP, ID: 371438569, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// This identifies the organization of CSE service providers as they relate to 
/// one another.
/// For example:
/// A section supervisor supervises Collection Officers
/// Collection Officers may have an Administrative Clerk
/// Chief Has Supervisors that report to him.
/// Business need:  Inform supervisors if delinquent ticklers and out of 
/// compliance  completion dates of cases.
/// Do we need dates to show history?  Per Karen Buchele on 03/27/95 we do not.
/// </summary>
[Serializable]
public partial class OfficeServiceProvRelationship: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OfficeServiceProvRelationship()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OfficeServiceProvRelationship(OfficeServiceProvRelationship that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OfficeServiceProvRelationship Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(OfficeServiceProvRelationship that)
  {
    base.Assign(that);
    reasonCode = that.reasonCode;
    createdBy = that.createdBy;
    createdDtstamp = that.createdDtstamp;
    ospRoleCode = that.ospRoleCode;
    ospEffectiveDate = that.ospEffectiveDate;
    offGeneratedId = that.offGeneratedId;
    spdGeneratedId = that.spdGeneratedId;
    ospRRoleCode = that.ospRRoleCode;
    ospREffectiveDt = that.ospREffectiveDt;
    offRGeneratedId = that.offRGeneratedId;
    spdRGeneratedId = that.spdRGeneratedId;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 2;

  /// <summary>
  /// The value of the REASON_CODE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ReasonCode_MaxLength)]
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

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
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

  /// <summary>
  /// The value of the CREATED_DTSTAMP attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdDtstamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedDtstamp
  {
    get => createdDtstamp;
    set => createdDtstamp = value;
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
  [Member(Index = 4, Type = MemberType.Char, Length = OspRoleCode_MaxLength)]
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
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("ospEffectiveDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? OspEffectiveDate
  {
    get => ospEffectiveDate;
    set => ospEffectiveDate = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 4)]
  public int OffGeneratedId
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
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 5)]
  public int SpdGeneratedId
  {
    get => spdGeneratedId;
    set => spdGeneratedId = value;
  }

  /// <summary>Length of the ROLE_CODE attribute.</summary>
  public const int OspRRoleCode_MaxLength = 2;

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
  [Member(Index = 8, Type = MemberType.Char, Length = OspRRoleCode_MaxLength)]
  public string OspRRoleCode
  {
    get => ospRRoleCode ?? "";
    set => ospRRoleCode = TrimEnd(Substring(value, 1, OspRRoleCode_MaxLength));
  }

  /// <summary>
  /// The json value of the OspRRoleCode attribute.</summary>
  [JsonPropertyName("ospRRoleCode")]
  [Computed]
  public string OspRRoleCode_Json
  {
    get => NullIf(OspRRoleCode, "");
    set => OspRRoleCode = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("ospREffectiveDt")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? OspREffectiveDt
  {
    get => ospREffectiveDt;
    set => ospREffectiveDt = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offRGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 4)]
  public int OffRGeneratedId
  {
    get => offRGeneratedId;
    set => offRGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdRGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 5)]
  public int SpdRGeneratedId
  {
    get => spdRGeneratedId;
    set => spdRGeneratedId = value;
  }

  private string reasonCode;
  private string createdBy;
  private DateTime? createdDtstamp;
  private string ospRoleCode;
  private DateTime? ospEffectiveDate;
  private int offGeneratedId;
  private int spdGeneratedId;
  private string ospRRoleCode;
  private DateTime? ospREffectiveDt;
  private int offRGeneratedId;
  private int spdRGeneratedId;
}
