// The source file: ALERT_DISTRIBUTION_RULE, ID: 371430568, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Alert Distribution Rule contains the business rules governing the 
/// distribution and optimization of office service provider alerts.
/// </summary>
[Serializable]
public partial class AlertDistributionRule: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AlertDistributionRule()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AlertDistributionRule(AlertDistributionRule that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AlertDistributionRule Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AlertDistributionRule that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    businessObjectCode = that.businessObjectCode;
    caseUnitFunction = that.caseUnitFunction;
    prioritizationCode = that.prioritizationCode;
    optimizationInd = that.optimizationInd;
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
    aleNo = that.aleNo;
    evdId = that.evdId;
    eveNo = that.eveNo;
    ospGeneratedId = that.ospGeneratedId;
    offGeneratedId = that.offGeneratedId;
    ospRoleCode = that.ospRoleCode;
    ospEffectiveDt = that.ospEffectiveDt;
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
  [JsonPropertyName("businessObjectCode")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = BusinessObjectCode_MaxLength, Optional = true)]
  public string BusinessObjectCode
  {
    get => businessObjectCode;
    set => businessObjectCode = value != null
      ? TrimEnd(Substring(value, 1, BusinessObjectCode_MaxLength)) : null;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CaseUnitFunction_MaxLength)
    ]
  public string CaseUnitFunction
  {
    get => caseUnitFunction ?? "";
    set => caseUnitFunction =
      TrimEnd(Substring(value, 1, CaseUnitFunction_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseUnitFunction attribute.</summary>
  [JsonPropertyName("caseUnitFunction")]
  [Computed]
  public string CaseUnitFunction_Json
  {
    get => NullIf(CaseUnitFunction, "");
    set => CaseUnitFunction = value;
  }

  /// <summary>
  /// The value of the PRIORITIZATION_CODE attribute.
  /// The Prioritization Code ranks the distribution rules into a hierarchy for 
  /// processing.
  /// Values are 1 through 9 with '1' having the highest value and '9' having 
  /// the lowest value.
  /// Default to '1' unless otherwise set.
  /// </summary>
  [JsonPropertyName("prioritizationCode")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 1)]
  public int PrioritizationCode
  {
    get => prioritizationCode;
    set => prioritizationCode = value;
  }

  /// <summary>Length of the OPTIMIZATION_IND attribute.</summary>
  public const int OptimizationInd_MaxLength = 1;

  /// <summary>
  /// The value of the OPTIMIZATION_IND attribute.
  /// The Optimization Indicator supplements distribution of Alerts by Business 
  /// Object and assignment.  The indicator identifies those alerts requiring
  /// optimization.
  /// Optimization results in a deletion of a specified 'set' of the total 
  /// Alerts which resulted from multiple Infrastructure records of the same
  /// Situation_Number.
  /// Default is set to 'N', indicating that the alert does not require 
  /// optimization.
  /// Values are:
  /// Y=Optimize alert distribution
  /// N=Do not optimize
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = OptimizationInd_MaxLength)]
    
  public string OptimizationInd
  {
    get => optimizationInd ?? "";
    set => optimizationInd =
      TrimEnd(Substring(value, 1, OptimizationInd_MaxLength));
  }

  /// <summary>
  /// The json value of the OptimizationInd attribute.</summary>
  [JsonPropertyName("optimizationInd")]
  [Computed]
  public string OptimizationInd_Json
  {
    get => NullIf(OptimizationInd, "");
    set => OptimizationInd = value;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 3;

  /// <summary>
  /// The value of the REASON_CODE attribute.
  /// Reason Code specifies the reason for the assignment.
  /// </summary>
  [JsonPropertyName("reasonCode")]
  [Member(Index = 6, Type = MemberType.Char, Length = ReasonCode_MaxLength, Optional
    = true)]
  public string ReasonCode
  {
    get => reasonCode;
    set => reasonCode = value != null
      ? TrimEnd(Substring(value, 1, ReasonCode_MaxLength)) : null;
  }

  /// <summary>Length of the RESPONSIBILITY_CODE attribute.</summary>
  public const int ResponsibilityCode_MaxLength = 3;

  /// <summary>
  /// The value of the RESPONSIBILITY_CODE attribute.
  /// Responsibility Code indictes whether the monitored activity assignment 
  /// will be with responsibility for compliance or informational only.
  /// Values are:
  ///   INF = informational
  ///   RSP = responsible for disposition of the assigned monitored activity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
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
  [Member(Index = 8, Type = MemberType.Char, Length = CaseRoleCode_MaxLength, Optional
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
  [Member(Index = 9, Type = MemberType.Char, Length
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
  [Member(Index = 10, Type = MemberType.Char, Length
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
  [Member(Index = 11, Type = MemberType.Char, Length
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
  [Member(Index = 12, Type = MemberType.Char, Length = LaPersonCode_MaxLength, Optional
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
  [Member(Index = 13, Type = MemberType.Date)]
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
  [Member(Index = 14, Type = MemberType.Date)]
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
  [Member(Index = 15, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 16, Type = MemberType.Timestamp)]
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
  [Member(Index = 17, Type = MemberType.Char, Length
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
  [Member(Index = 18, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
  /// A predefined numeric identifier for performance optimization.
  /// </summary>
  [JsonPropertyName("aleNo")]
  [Member(Index = 19, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? AleNo
  {
    get => aleNo;
    set => aleNo = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("evdId")]
  [DefaultValue(0)]
  [Member(Index = 20, Type = MemberType.Number, Length = 3)]
  public int EvdId
  {
    get => evdId;
    set => evdId = value;
  }

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("eveNo")]
  [DefaultValue(0)]
  [Member(Index = 21, Type = MemberType.Number, Length = 5)]
  public int EveNo
  {
    get => eveNo;
    set => eveNo = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("ospGeneratedId")]
  [Member(Index = 22, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? OspGeneratedId
  {
    get => ospGeneratedId;
    set => ospGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offGeneratedId")]
  [Member(Index = 23, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffGeneratedId
  {
    get => offGeneratedId;
    set => offGeneratedId = value;
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
  [Member(Index = 24, Type = MemberType.Char, Length = OspRoleCode_MaxLength, Optional
    = true)]
  public string OspRoleCode
  {
    get => ospRoleCode;
    set => ospRoleCode = value != null
      ? TrimEnd(Substring(value, 1, OspRoleCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("ospEffectiveDt")]
  [Member(Index = 25, Type = MemberType.Date, Optional = true)]
  public DateTime? OspEffectiveDt
  {
    get => ospEffectiveDt;
    set => ospEffectiveDt = value;
  }

  private int systemGeneratedIdentifier;
  private string businessObjectCode;
  private string caseUnitFunction;
  private int prioritizationCode;
  private string optimizationInd;
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
  private int? aleNo;
  private int evdId;
  private int eveNo;
  private int? ospGeneratedId;
  private int? offGeneratedId;
  private string ospRoleCode;
  private DateTime? ospEffectiveDt;
}
