// The source file: MONITORED_ACTIVITY, ID: 371437342, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Identifies activity that needs to be completed for a Business Object to 
/// remain in compliance with federal or state regulation.
/// Monitored Activity is assigned to a service provider to be completed.
/// Monitored Activities are those Events which act as 'ending' or 'closing' 
/// Events for an Activity.
/// An Activity may be closed by any one of the Events in the set of closing 
/// Events for an Activity.
/// </summary>
[Serializable]
public partial class MonitoredActivity: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public MonitoredActivity()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public MonitoredActivity(MonitoredActivity that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new MonitoredActivity Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(MonitoredActivity that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    name = that.name;
    activityControlNumber = that.activityControlNumber;
    typeCode = that.typeCode;
    fedNonComplianceDate = that.fedNonComplianceDate;
    fedNearNonComplDate = that.fedNearNonComplDate;
    otherNonComplianceDate = that.otherNonComplianceDate;
    otherNearNonComplDate = that.otherNearNonComplDate;
    startDate = that.startDate;
    closureDate = that.closureDate;
    closureReasonCode = that.closureReasonCode;
    caseUnitClosedInd = that.caseUnitClosedInd;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    infSysGenId = that.infSysGenId;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 40;

  /// <summary>
  /// The value of the NAME attribute.
  /// Name is the event name for the activity which is being monitored.  The 
  /// system is monitoring for the event to 'complete' or 'close' the activity.
  /// The name suffix indicates the number of days, months, or years for 
  /// determining non compliance.  This will facilitate easy cross reference to
  /// federal regulations.  The actual values for compliance, however, will
  /// always be expressed in days.
  /// Name suffix examples:
  /// 1_YR	=	one year
  /// 30_DY	=	30 days
  /// 6_MO	=	6 months
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Name_MaxLength)]
  public string Name
  {
    get => name ?? "";
    set => name = TrimEnd(Substring(value, 1, Name_MaxLength));
  }

  /// <summary>
  /// The json value of the Name attribute.</summary>
  [JsonPropertyName("name")]
  [Computed]
  public string Name_Json
  {
    get => NullIf(Name, "");
    set => Name = value;
  }

  /// <summary>
  /// The value of the ACTIVITY_CONTROL_NUMBER attribute.
  /// A predefined numeric identifier for performance optimization.
  /// </summary>
  [JsonPropertyName("activityControlNumber")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 5)]
  public int ActivityControlNumber
  {
    get => activityControlNumber;
    set => activityControlNumber = value;
  }

  /// <summary>Length of the TYPE_CODE attribute.</summary>
  public const int TypeCode_MaxLength = 3;

  /// <summary>
  /// The value of the TYPE_CODE attribute.
  /// Type classifies a monitored activity as system generated or user generated
  /// This attribute is always populated by the application.  It is only used
  /// by the user as a filtering agent in the Monitored Activity list screen.
  /// Values are:
  ///   MAN = manually generated (by a service provider)
  ///   AUT = automatically generated (by the event processor)
  /// </summary>
  [JsonPropertyName("typeCode")]
  [Member(Index = 4, Type = MemberType.Char, Length = TypeCode_MaxLength, Optional
    = true)]
  public string TypeCode
  {
    get => typeCode;
    set => typeCode = value != null
      ? TrimEnd(Substring(value, 1, TypeCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the FED_NON_COMPLIANCE_DATE attribute.
  /// Federal Non Compliance Date is the derived date based on the activity 
  /// create timestamp and the federal non compliance days.
  /// This date represents the date by which the monitored activity is to be 
  /// completed.
  /// </summary>
  [JsonPropertyName("fedNonComplianceDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? FedNonComplianceDate
  {
    get => fedNonComplianceDate;
    set => fedNonComplianceDate = value;
  }

  /// <summary>
  /// The value of the FED_NEAR_NON_COMPL_DATE attribute.
  /// Federal Near Non Compliance Date is the derived date based on the activity
  /// create timestamp and the federal near non compliance days.
  /// This date is used to filter activity displays for the service provider and
  /// represents the date by which the service provider may need to review the
  /// status of activites to ensure compliance.
  /// </summary>
  [JsonPropertyName("fedNearNonComplDate")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? FedNearNonComplDate
  {
    get => fedNearNonComplDate;
    set => fedNearNonComplDate = value;
  }

  /// <summary>
  /// The value of the OTHER_NON_COMPLIANCE_DATE attribute.
  /// Other Non Compliance Date is the derived date based on the activity create
  /// timestamp and the state non compliance days.
  /// This date represents the date by which the monitored activity is to be 
  /// completed.
  /// </summary>
  [JsonPropertyName("otherNonComplianceDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? OtherNonComplianceDate
  {
    get => otherNonComplianceDate;
    set => otherNonComplianceDate = value;
  }

  /// <summary>
  /// The value of the OTHER_NEAR_NON_COMPL_DATE attribute.
  /// Other Near Non Compliance Date is the derived date based on the activity 
  /// create timestamp and Other near non compliance days.
  /// This date is used to filter activity displays for the service provider and
  /// represents the date by which the service provider may need to review the
  /// status of activities to ensure compliance.
  /// </summary>
  [JsonPropertyName("otherNearNonComplDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? OtherNearNonComplDate
  {
    get => otherNearNonComplDate;
    set => otherNearNonComplDate = value;
  }

  /// <summary>
  /// The value of the START_DATE attribute.
  /// The date on which the monitored activity was initiated by the starting 
  /// event.
  /// </summary>
  [JsonPropertyName("startDate")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? StartDate
  {
    get => startDate;
    set => startDate = value;
  }

  /// <summary>
  /// The value of the CLOSURE_DATE attribute.
  /// The date of completion of the monitored activity.
  /// </summary>
  [JsonPropertyName("closureDate")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? ClosureDate
  {
    get => closureDate;
    set => closureDate = value;
  }

  /// <summary>Length of the CLOSURE_REASON_CODE attribute.</summary>
  public const int ClosureReasonCode_MaxLength = 3;

  /// <summary>
  /// The value of the CLOSURE_REASON_CODE attribute.
  /// The End Reason Code indicates the activity closure reason.
  /// </summary>
  [JsonPropertyName("closureReasonCode")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = ClosureReasonCode_MaxLength, Optional = true)]
  public string ClosureReasonCode
  {
    get => closureReasonCode;
    set => closureReasonCode = value != null
      ? TrimEnd(Substring(value, 1, ClosureReasonCode_MaxLength)) : null;
  }

  /// <summary>Length of the CASE_UNIT_CLOSED_IND attribute.</summary>
  public const int CaseUnitClosedInd_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_UNIT_CLOSED_IND attribute.
  /// The Case Unit Closed Indicator enables the application to process those 
  /// Monitored Activities that are in an open status when the case or case unit
  /// is closed due to external circumstances.
  /// EXAMPLE:
  /// An AR is non-cooped and the Case is closed as a result.  Any open 
  /// Monitored Activites associated to the closed Case are flagged.
  /// If the case is reopened due to AR cooperation, the system will be capable 
  /// of reopening the Monitored Activity.
  /// Values are:
  /// Y = case unit closed while this monitored activity was open.
  /// N = monitored activity was closed prior to case unit closure.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = CaseUnitClosedInd_MaxLength)]
  public string CaseUnitClosedInd
  {
    get => caseUnitClosedInd ?? "";
    set => caseUnitClosedInd =
      TrimEnd(Substring(value, 1, CaseUnitClosedInd_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseUnitClosedInd attribute.</summary>
  [JsonPropertyName("caseUnitClosedInd")]
  [Computed]
  public string CaseUnitClosedInd_Json
  {
    get => NullIf(CaseUnitClosedInd, "");
    set => CaseUnitClosedInd = value;
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
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("infSysGenId")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? InfSysGenId
  {
    get => infSysGenId;
    set => infSysGenId = value;
  }

  private int systemGeneratedIdentifier;
  private string name;
  private int activityControlNumber;
  private string typeCode;
  private DateTime? fedNonComplianceDate;
  private DateTime? fedNearNonComplDate;
  private DateTime? otherNonComplianceDate;
  private DateTime? otherNearNonComplDate;
  private DateTime? startDate;
  private DateTime? closureDate;
  private string closureReasonCode;
  private string caseUnitClosedInd;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int? infSysGenId;
}
