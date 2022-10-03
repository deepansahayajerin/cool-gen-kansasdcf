// The source file: ACTIVITY_DISTRIBUTION_RULE, ID: 371430098, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Contains the business rules for distribution of monitored activities for 
/// office service provider assignment.
/// </summary>
[Serializable]
public partial class ActivityDistributionRule: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ActivityDistributionRule()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ActivityDistributionRule(ActivityDistributionRule that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ActivityDistributionRule Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ActivityDistributionRule that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    businessObjectCode = that.businessObjectCode;
    caseUnitFunction = that.caseUnitFunction;
    reasonCode = that.reasonCode;
    responsibilityCode = that.responsibilityCode;
    caseRoleCode = that.caseRoleCode;
    csePersonAcctCode = that.csePersonAcctCode;
    csenetRoleCode = that.csenetRoleCode;
    laCaseRoleCode = that.laCaseRoleCode;
    laPersonCode = that.laPersonCode;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    actControlNo = that.actControlNo;
    acdId = that.acdId;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 5)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the BUSINESS_OBJECT_CODE attribute.</summary>
  public const int BusinessObjectCode_MaxLength = 3;

  /// <summary>
  /// The value of the BUSINESS_OBJECT_CODE attribute.
  /// The Business Object represents the primary entity for which the Alert has 
  /// meaning.
  /// Valid Business Objects include:
  /// Case
  /// Case_Unit
  /// Information_Request
  /// Referral
  /// Legal_Referral
  /// Legal_Action
  /// Obligation
  /// Obligation_Administrative_Action
  /// Administrative_Appeal
  /// Administrative_Act_Certification
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
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

  /// <summary>Length of the CASE_UNIT_FUNCTION attribute.</summary>
  public const int CaseUnitFunction_MaxLength = 3;

  /// <summary>
  /// The value of the CASE_UNIT_FUNCTION attribute.
  /// When an office service provider has been assigned to work a specific 
  /// function for a case, that assignment is made for a function at the case
  /// unit level.
  /// Case unit function must be accompanied by the case unit identifier to 
  /// correctly identify the office service provider assigned.
  /// Case unit function is populated with a code representing the value of the 
  /// function property of a case unit's lifecycle state.
  /// Valid Functions are:
  /// LOC	=	Case Unit in Locate
  /// PAT	=	Case Unit in Paternity
  /// OE	=	Case Unit in Obligation 		Establishment
  /// ENF	=	Case Unit in Enforcment
  /// </summary>
  [JsonPropertyName("caseUnitFunction")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = CaseUnitFunction_MaxLength, Optional = true)]
  public string CaseUnitFunction
  {
    get => caseUnitFunction;
    set => caseUnitFunction = value != null
      ? TrimEnd(Substring(value, 1, CaseUnitFunction_MaxLength)) : null;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 3;

  /// <summary>
  /// The value of the REASON_CODE attribute.
  /// Reason Code specifies the reason for the assignment.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = ReasonCode_MaxLength)]
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

  /// <summary>Length of the RESPONSIBILITY_CODE attribute.</summary>
  public const int ResponsibilityCode_MaxLength = 3;

  /// <summary>
  /// The value of the RESPONSIBILITY_CODE attribute.
  /// Responsibility Code indicates whether the monitored activity assignment 
  /// will be with responsibility for compliance or informational only.
  /// Values are 0 through 9
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = ResponsibilityCode_MaxLength)]
  public string ResponsibilityCode
  {
    get => responsibilityCode ?? "";
    set => responsibilityCode =
      TrimEnd(Substring(value, 1, ResponsibilityCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ResponsibilityCode attribute.</summary>
  [JsonPropertyName("responsibilityCode")]
  [Computed]
  public string ResponsibilityCode_Json
  {
    get => NullIf(ResponsibilityCode, "");
    set => ResponsibilityCode = value;
  }

  /// <summary>Length of the CASE_ROLE_CODE attribute.</summary>
  public const int CaseRoleCode_MaxLength = 2;

  /// <summary>
  /// The value of the CASE_ROLE_CODE attribute.
  /// The Case Role Code refers to the entiy, Case_Role.
  /// This Code identifies the active Case Role held by the CSE Person 
  /// identified as the Business Object of the Infrastructure reccord which was
  /// created in response to an event.  If the CSE Person is not actually the
  /// Business Object, then this code identifies the CSE Person most central to
  /// the Business Object.
  /// The business object for the Infrastructure record and the Event are the 
  /// same.
  /// Values are:
  /// 	AR = Applicant Recipient
  /// 	AP = Absent Parent
  /// 	CH = Child
  /// </summary>
  [JsonPropertyName("caseRoleCode")]
  [Member(Index = 6, Type = MemberType.Char, Length = CaseRoleCode_MaxLength, Optional
    = true)]
  public string CaseRoleCode
  {
    get => caseRoleCode;
    set => caseRoleCode = value != null
      ? TrimEnd(Substring(value, 1, CaseRoleCode_MaxLength)) : null;
  }

  /// <summary>Length of the CSE_PERSON_ACCT_CODE attribute.</summary>
  public const int CsePersonAcctCode_MaxLength = 2;

  /// <summary>
  /// The value of the CSE_PERSON_ACCT_CODE attribute.
  /// The CSE Person Account Code refers to the entity, CSE_Person_Account.
  /// This code identifies the active financial role held by the CSE Person 
  /// central to the Business Object of the Infrastructure record which was
  /// created in response to an Event.
  /// The business object for the Infrastructure record and the Event are the 
  /// same.
  /// Values are:
  /// 	E = Obligee
  /// 	R = Obligor
  /// 	S = Supported Person
  /// </summary>
  [JsonPropertyName("csePersonAcctCode")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = CsePersonAcctCode_MaxLength, Optional = true)]
  public string CsePersonAcctCode
  {
    get => csePersonAcctCode;
    set => csePersonAcctCode = value != null
      ? TrimEnd(Substring(value, 1, CsePersonAcctCode_MaxLength)) : null;
  }

  /// <summary>Length of the CSENET_ROLE_CODE attribute.</summary>
  public const int CsenetRoleCode_MaxLength = 2;

  /// <summary>
  /// The value of the CSENET_ROLE_CODE attribute.
  /// The CSENet Role Code refers to the entity, CSENET_ROLE.
  /// This code identifies a CSE Person's CSENet transaction role in the 
  /// associated CSENet transaction event.
  /// This code identifies the active financial role held by the CSE Person 
  /// Identified for/as the business object of the Infrastructure record which
  /// was created in response to an Event.
  /// The business object for the Infrastructure record and the Event are the 
  /// same.
  /// </summary>
  [JsonPropertyName("csenetRoleCode")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = CsenetRoleCode_MaxLength, Optional = true)]
  public string CsenetRoleCode
  {
    get => csenetRoleCode;
    set => csenetRoleCode = value != null
      ? TrimEnd(Substring(value, 1, CsenetRoleCode_MaxLength)) : null;
  }

  /// <summary>Length of the LA_CASE_ROLE_CODE attribute.</summary>
  public const int LaCaseRoleCode_MaxLength = 2;

  /// <summary>
  /// The value of the LA_CASE_ROLE_CODE attribute.
  /// The LA Case Role Code refers to the entity LEGAL_ACTION_CASE_ROLE.
  /// Legal Action Case Role exists to associate a CSE Person's legal role to 
  /// their case role.
  /// This code identifies the active associated Case Role held by the CSE 
  /// Person central to the business object of the Infrastructure record which
  /// was created in response to an event.
  /// The business object for the Infrastructure record and the Event are the 
  /// same.
  /// Values are:
  /// 	AR = Applicant Recipient
  /// 	AP = Absent Parent
  /// 	CH = Child
  /// </summary>
  [JsonPropertyName("laCaseRoleCode")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = LaCaseRoleCode_MaxLength, Optional = true)]
  public string LaCaseRoleCode
  {
    get => laCaseRoleCode;
    set => laCaseRoleCode = value != null
      ? TrimEnd(Substring(value, 1, LaCaseRoleCode_MaxLength)) : null;
  }

  /// <summary>Length of the LA_PERSON_CODE attribute.</summary>
  public const int LaPersonCode_MaxLength = 2;

  /// <summary>
  /// The value of the LA_PERSON_CODE attribute.
  /// LA Person Code relates to the entity LEGAL ACTION PERSON.
  /// This code identifies the active Legal Action role held by the CSE Person 
  /// central to the business object of the Infrastructure record which was
  /// created in response to an Event.
  /// The business object for the Infrastructure record and the Event are the 
  /// same.
  /// Values are:
  /// 	P = Petitioner
  /// 	R = Respondent
  /// 	C = Child
  /// </summary>
  [JsonPropertyName("laPersonCode")]
  [Member(Index = 10, Type = MemberType.Char, Length = LaPersonCode_MaxLength, Optional
    = true)]
  public string LaPersonCode
  {
    get => laPersonCode;
    set => laPersonCode = value != null
      ? TrimEnd(Substring(value, 1, LaPersonCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 11, Type = MemberType.Date)]
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
  [Member(Index = 12, Type = MemberType.Date)]
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
  [Member(Index = 13, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 14, Type = MemberType.Timestamp)]
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
  [Member(Index = 15, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
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
  [Member(Index = 16, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
  /// A predefined numeric identifier for performance optimization.
  /// </summary>
  [JsonPropertyName("actControlNo")]
  [DefaultValue(0)]
  [Member(Index = 17, Type = MemberType.Number, Length = 5)]
  public int ActControlNo
  {
    get => actControlNo;
    set => actControlNo = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("acdId")]
  [DefaultValue(0)]
  [Member(Index = 18, Type = MemberType.Number, Length = 3)]
  public int AcdId
  {
    get => acdId;
    set => acdId = value;
  }

  private int systemGeneratedIdentifier;
  private string businessObjectCode;
  private string caseUnitFunction;
  private string reasonCode;
  private string responsibilityCode;
  private string caseRoleCode;
  private string csePersonAcctCode;
  private string csenetRoleCode;
  private string laCaseRoleCode;
  private string laPersonCode;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int actControlNo;
  private int acdId;
}
