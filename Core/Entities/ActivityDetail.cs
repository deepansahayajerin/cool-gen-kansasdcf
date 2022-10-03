// The source file: ACTIVITY_DETAIL, ID: 371430067, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Activity Detail contains the business rule timeframe templates for 
/// monitoring of CSE activities within specific timeframes.
/// The timeframe source is identified by values for federal or 'other' non 
/// compliance and nearing non compliance days.  All timeframes are stated in
/// the unit of measurement 'DAYS'.
/// </summary>
[Serializable]
public partial class ActivityDetail: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ActivityDetail()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ActivityDetail(ActivityDetail that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ActivityDetail Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ActivityDetail that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    businessObjectCode = that.businessObjectCode;
    caseUnitFunction = that.caseUnitFunction;
    fedNonComplianceDays = that.fedNonComplianceDays;
    fedNearNonComplDays = that.fedNearNonComplDays;
    otherNonComplianceDays = that.otherNonComplianceDays;
    otherNearNonComplDays = that.otherNearNonComplDays;
    regulationSourceId = that.regulationSourceId;
    regulationSourceDescription = that.regulationSourceDescription;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    actNo = that.actNo;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
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
  /// Case Unit Function must be accompanied by the case unit identifier to 
  /// correctly identify the office service provider assigned.
  /// Case Unit Function is populated with a code representing the value of the 
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

  /// <summary>
  /// The value of the FED_NON_COMPLIANCE_DAYS attribute.
  /// Federal Non Compliance indicates the timeframe duration for the monitored 
  /// activity when federal guidelines are used.  It is always expressed in
  /// days.
  /// This attribute is used to determine the date for compliance with the 
  /// federal mandated regulation.
  /// Federal Non Compliance is measured from the start date for the event which
  /// triggers the activity.
  /// </summary>
  [JsonPropertyName("fedNonComplianceDays")]
  [Member(Index = 4, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? FedNonComplianceDays
  {
    get => fedNonComplianceDays;
    set => fedNonComplianceDays = value;
  }

  /// <summary>
  /// The value of the FED_NEAR_NON_COMPL_DAYS attribute.
  /// Federal Near Non Compliance is used as a filtering device when federal 
  /// guidelines are used to monitor the activity.  It is always expressed in
  /// days.
  /// This attribute is used to determine the date from which point the office 
  /// service provider may wish to review the status of the monitored activity
  /// to ensure compliance.
  /// Federal Near Non Compliance is measured from the start date of the event 
  /// which triggers the activity.
  /// </summary>
  [JsonPropertyName("fedNearNonComplDays")]
  [Member(Index = 5, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? FedNearNonComplDays
  {
    get => fedNearNonComplDays;
    set => fedNearNonComplDays = value;
  }

  /// <summary>
  /// The value of the OTHER_NON_COMPLIANCE_DAYS attribute.
  /// Other Non Compliance indicates the timeframe duration for the monitored 
  /// activity when no federal guidelines exist, or when used by CSE as a
  /// supplement to federal guidelines.  It is always expressed in days.
  /// This attribute is used to determine the date for compliance with the 
  /// regulation.  An 'other' regulation may be a mandated Kansas legislative
  /// requirement or simply CSE policy.
  /// Other Non Compliance is measured from the start date of the event which 
  /// triggers the activity.
  /// </summary>
  [JsonPropertyName("otherNonComplianceDays")]
  [Member(Index = 6, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OtherNonComplianceDays
  {
    get => otherNonComplianceDays;
    set => otherNonComplianceDays = value;
  }

  /// <summary>
  /// The value of the OTHER_NEAR_NON_COMPL_DAYS attribute.
  /// Other Near Non Compliance is used as a filtering device when guidelines 
  /// other than federal regulations are used to monitor the activity.  It is
  /// always expressed in days.
  /// This attribute is used to determine a date from which point the office 
  /// service provider may wish to review the status of the monitored activity
  /// to ensure compliance.
  /// Other Near Non Compliance is measured from the start date of the event 
  /// which triggers the activity.
  /// </summary>
  [JsonPropertyName("otherNearNonComplDays")]
  [Member(Index = 7, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OtherNearNonComplDays
  {
    get => otherNearNonComplDays;
    set => otherNearNonComplDays = value;
  }

  /// <summary>Length of the REGULATION_SOURCE_ID attribute.</summary>
  public const int RegulationSourceId_MaxLength = 1;

  /// <summary>
  /// The value of the REGULATION_SOURCE_ID attribute.
  /// The Regulation Source Identifier specifies whether the non-compliance/near
  /// non-compliance timeframe is based upon a federal or 'other' source.
  /// An 'other' regulation source may be the state or SRS.
  /// Values are:
  /// F=Federal
  /// O=Other
  /// </summary>
  [JsonPropertyName("regulationSourceId")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = RegulationSourceId_MaxLength, Optional = true)]
  public string RegulationSourceId
  {
    get => regulationSourceId;
    set => regulationSourceId = value != null
      ? TrimEnd(Substring(value, 1, RegulationSourceId_MaxLength)) : null;
  }

  /// <summary>Length of the REGULATION_SOURCE_DESCRIPTION attribute.</summary>
  public const int RegulationSourceDescription_MaxLength = 72;

  /// <summary>
  /// The value of the REGULATION_SOURCE_DESCRIPTION attribute.
  /// This attribute is populated with a string of text identifiers pointing to 
  /// the federal or other regulation source identifier.
  /// More than one regulation identifier may be strung together.
  /// </summary>
  [JsonPropertyName("regulationSourceDescription")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = RegulationSourceDescription_MaxLength, Optional = true)]
  public string RegulationSourceDescription
  {
    get => regulationSourceDescription;
    set => regulationSourceDescription = value != null
      ? TrimEnd(Substring(value, 1, RegulationSourceDescription_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 10, Type = MemberType.Date)]
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
  [Member(Index = 11, Type = MemberType.Date)]
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
  [Member(Index = 12, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 13, Type = MemberType.Timestamp)]
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
  [Member(Index = 14, Type = MemberType.Char, Length
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
  [Member(Index = 15, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
  /// A predefined numeric identifier for performance optimization.
  /// </summary>
  [JsonPropertyName("actNo")]
  [DefaultValue(0)]
  [Member(Index = 16, Type = MemberType.Number, Length = 5)]
  public int ActNo
  {
    get => actNo;
    set => actNo = value;
  }

  private int systemGeneratedIdentifier;
  private string businessObjectCode;
  private string caseUnitFunction;
  private int? fedNonComplianceDays;
  private int? fedNearNonComplDays;
  private int? otherNonComplianceDays;
  private int? otherNearNonComplDays;
  private string regulationSourceId;
  private string regulationSourceDescription;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int actNo;
}
