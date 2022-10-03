// The source file: OBLIGATION_ADMIN_ACTION_ASSGN, ID: 371438021, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// This documents the assignment of an obligatin administrative action to an 
/// office service provider.
/// DATA MODEL ALERT!!!!
/// *	The relationship between OBLIGATION ADMIN ACTION ASSGN and OBLIGATION 
/// ADMIN ACTION is not drawn.
/// each OBLIGATION ADMIN ACTION sometimes is assigned to one or more OBLIGATION
/// ADMIN ACTION ASSGN
/// each OBLIGATION ADMIN ACTION ASSGNalways assigns one OBLIGATION ADMIN ACTION
/// *	The relationship between OFFICE SERVICE PROVIDER and OBLIGATION ADMIN 
/// ACTION ASSGN is not drawn.
/// each OFFICE SERVICE PROVIDER sometimes is assigned to one or more OBLIGATION
/// ADMIN ACTION ASSGN
/// each OBLIGATION ADMIN ACTION ASSGN always assigns one OFFICE SERVICE 
/// PROVIDER
/// *	Both relationships should be added to the identifier.
/// </summary>
[Serializable]
public partial class ObligationAdminActionAssgn: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ObligationAdminActionAssgn()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ObligationAdminActionAssgn(ObligationAdminActionAssgn that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ObligationAdminActionAssgn Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ObligationAdminActionAssgn that)
  {
    base.Assign(that);
    reasonCode = that.reasonCode;
    overrideInd = that.overrideInd;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    oaaDate = that.oaaDate;
    cpaType = that.cpaType;
    cspNo = that.cspNo;
    obgId = that.obgId;
    aatType = that.aatType;
    otyId = that.otyId;
    ospDate = that.ospDate;
    ospCode = that.ospCode;
    offId = that.offId;
    spdId = that.spdId;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 3;

  /// <summary>
  /// The value of the REASON_CODE attribute.
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

  /// <summary>Length of the OVERRIDE_IND attribute.</summary>
  public const int OverrideInd_MaxLength = 1;

  /// <summary>
  /// The value of the OVERRIDE_IND attribute.
  /// This switch allows an office service provider to flag assignments as not 
  /// available for reassignment.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = OverrideInd_MaxLength)]
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
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The date upon which this occurrence of the entity can no longer be used.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 7, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 8, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the TAKEN_DATE attribute.
  /// The date that an enforcement action is taken against an Obligor for a 
  /// particular Obligation.
  /// </summary>
  [JsonPropertyName("oaaDate")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? OaaDate
  {
    get => oaaDate;
    set => oaaDate = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = CpaType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => cpaType ?? "";
    set => cpaType = TrimEnd(Substring(value, 1, CpaType_MaxLength));
  }

  /// <summary>
  /// The json value of the CpaType attribute.</summary>
  [JsonPropertyName("cpaType")]
  [Computed]
  public string CpaType_Json
  {
    get => NullIf(CpaType, "");
    set => CpaType = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNo_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CspNo_MaxLength)]
  public string CspNo
  {
    get => cspNo ?? "";
    set => cspNo = TrimEnd(Substring(value, 1, CspNo_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNo attribute.</summary>
  [JsonPropertyName("cspNo")]
  [Computed]
  public string CspNo_Json
  {
    get => NullIf(CspNo, "");
    set => CspNo = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgId")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 3)]
  public int ObgId
  {
    get => obgId;
    set => obgId = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int AatType_MaxLength = 4;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This identifies the type of enforcement that can be taken.
  /// Examples are:
  ///      SDSO
  ///      FDSO
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = AatType_MaxLength)]
  public string AatType
  {
    get => aatType ?? "";
    set => aatType = TrimEnd(Substring(value, 1, AatType_MaxLength));
  }

  /// <summary>
  /// The json value of the AatType attribute.</summary>
  [JsonPropertyName("aatType")]
  [Computed]
  public string AatType_Json
  {
    get => NullIf(AatType, "");
    set => AatType = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyId")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 3)]
  public int OtyId
  {
    get => otyId;
    set => otyId = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("ospDate")]
  [Member(Index = 15, Type = MemberType.Date)]
  public DateTime? OspDate
  {
    get => ospDate;
    set => ospDate = value;
  }

  /// <summary>Length of the ROLE_CODE attribute.</summary>
  public const int OspCode_MaxLength = 2;

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
  [Member(Index = 16, Type = MemberType.Char, Length = OspCode_MaxLength)]
  public string OspCode
  {
    get => ospCode ?? "";
    set => ospCode = TrimEnd(Substring(value, 1, OspCode_MaxLength));
  }

  /// <summary>
  /// The json value of the OspCode attribute.</summary>
  [JsonPropertyName("ospCode")]
  [Computed]
  public string OspCode_Json
  {
    get => NullIf(OspCode, "");
    set => OspCode = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offId")]
  [DefaultValue(0)]
  [Member(Index = 17, Type = MemberType.Number, Length = 4)]
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
  [Member(Index = 18, Type = MemberType.Number, Length = 5)]
  public int SpdId
  {
    get => spdId;
    set => spdId = value;
  }

  private string reasonCode;
  private string overrideInd;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private DateTime? oaaDate;
  private string cpaType;
  private string cspNo;
  private int obgId;
  private string aatType;
  private int otyId;
  private DateTime? ospDate;
  private string ospCode;
  private int offId;
  private int spdId;
}
