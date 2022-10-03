// The source file: EVENT_DETAIL, ID: 371434252, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Event Detail specifies those properties of Events which have been 
/// parametized by Event Type classification and those properties dependent upon
/// an event's interstate statuses.
/// DATA MODEL ALERT!!!!!
/// *	The relationship between EVENT_DETAIL and DOCUMENT is not drawn.
/// Each EVENT_DETAIL sometimes (33%) defines one DOCUMENT.
/// Each DOCUMENT sometimes (99%) is defined by one EVENT_DETAIL.
/// </summary>
[Serializable]
public partial class EventDetail: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EventDetail()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EventDetail(EventDetail that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EventDetail Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EventDetail that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    detailName = that.detailName;
    description = that.description;
    initiatingStateCode = that.initiatingStateCode;
    csenetInOutCode = that.csenetInOutCode;
    reasonCode = that.reasonCode;
    procedureName = that.procedureName;
    lifecycleImpactCode = that.lifecycleImpactCode;
    logToDiaryInd = that.logToDiaryInd;
    dateMonitorDays = that.dateMonitorDays;
    nextEventId = that.nextEventId;
    nextEventDetailId = that.nextEventDetailId;
    nextInitiatingState = that.nextInitiatingState;
    nextCsenetInOut = that.nextCsenetInOut;
    nextReason = that.nextReason;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedDtstamp = that.lastUpdatedDtstamp;
    function = that.function;
    exceptionRoutine = that.exceptionRoutine;
    eveNo = that.eveNo;
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

  /// <summary>Length of the DETAIL_NAME attribute.</summary>
  public const int DetailName_MaxLength = 55;

  /// <summary>
  /// The value of the DETAIL_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = DetailName_MaxLength)]
  public string DetailName
  {
    get => detailName ?? "";
    set => detailName = TrimEnd(Substring(value, 1, DetailName_MaxLength));
  }

  /// <summary>
  /// The json value of the DetailName attribute.</summary>
  [JsonPropertyName("detailName")]
  [Computed]
  public string DetailName_Json
  {
    get => NullIf(DetailName, "");
    set => DetailName = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// A full text business definition of the event detail.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 3, Type = MemberType.Char, Length = Description_MaxLength, Optional
    = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? TrimEnd(Substring(value, 1, Description_MaxLength)) : null;
  }

  /// <summary>Length of the INITIATING_STATE_CODE attribute.</summary>
  public const int InitiatingStateCode_MaxLength = 2;

  /// <summary>
  /// The value of the INITIATING_STATE_CODE attribute.
  /// Initiating State Code identifies the state ownership of a case.
  /// For an interstate transaction, the initiating state is the state which 
  /// initiated the original request for services.
  /// When used in relationship to non CSENet transactions, the value will 
  /// default to 'KS' (Kansas) unless Activities or Alerts specific to an
  /// interstate case are involved.  In those circumstances, 2 event details
  /// exist.  One  for a KS case, the other for the OS (other state) case.
  /// Valid Values:
  /// KS = Kansas case
  /// OS = other state's case
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = InitiatingStateCode_MaxLength)]
  public string InitiatingStateCode
  {
    get => initiatingStateCode ?? "";
    set => initiatingStateCode =
      TrimEnd(Substring(value, 1, InitiatingStateCode_MaxLength));
  }

  /// <summary>
  /// The json value of the InitiatingStateCode attribute.</summary>
  [JsonPropertyName("initiatingStateCode")]
  [Computed]
  public string InitiatingStateCode_Json
  {
    get => NullIf(InitiatingStateCode, "");
    set => InitiatingStateCode = value;
  }

  /// <summary>Length of the CSENET_IN_OUT_CODE attribute.</summary>
  public const int CsenetInOutCode_MaxLength = 1;

  /// <summary>
  /// The value of the CSENET_IN_OUT_CODE attribute.
  /// The CSENet In Out Code is used by the application to determine if an 
  /// interstate Event is for an incoming or outgoing transaction.
  /// The initiating state for an interstate event is determined by the 
  /// initiating state code value.
  /// Valid values are:
  /// I = incoming transaction
  /// O = outgoing transaction
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CsenetInOutCode_MaxLength)]
    
  public string CsenetInOutCode
  {
    get => csenetInOutCode ?? "";
    set => csenetInOutCode =
      TrimEnd(Substring(value, 1, CsenetInOutCode_MaxLength));
  }

  /// <summary>
  /// The json value of the CsenetInOutCode attribute.</summary>
  [JsonPropertyName("csenetInOutCode")]
  [Computed]
  public string CsenetInOutCode_Json
  {
    get => NullIf(CsenetInOutCode, "");
    set => CsenetInOutCode = value;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 15;

  /// <summary>
  /// The value of the REASON_CODE attribute.
  /// The Reason Code identifies the reason for a business transaction.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = ReasonCode_MaxLength)]
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

  /// <summary>Length of the PROCEDURE_NAME attribute.</summary>
  public const int ProcedureName_MaxLength = 8;

  /// <summary>
  /// The value of the PROCEDURE_NAME attribute.
  /// The name of the online or batch procedure from which the Event is raised.
  /// This is for the purpose of documentation.
  /// </summary>
  [JsonPropertyName("procedureName")]
  [Member(Index = 7, Type = MemberType.Char, Length = ProcedureName_MaxLength, Optional
    = true)]
  public string ProcedureName
  {
    get => procedureName;
    set => procedureName = value != null
      ? TrimEnd(Substring(value, 1, ProcedureName_MaxLength)) : null;
  }

  /// <summary>Length of the LIFECYCLE_IMPACT_CODE attribute.</summary>
  public const int LifecycleImpactCode_MaxLength = 1;

  /// <summary>
  /// The value of the LIFECYCLE_IMPACT_CODE attribute.
  /// Lifecycle Impact Code identifies those Events with a potential to impact 
  /// the case unit lifecycle state.
  /// An incoming CSENet transaction will not directly affect lifecycle, but an 
  /// update based upon the transaction reason may.
  /// Valid Lifecycle Impact Codes are:
  /// Y = 	Definite change to case unit state
  /// N = 	No change to case unit lifecycle
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = LifecycleImpactCode_MaxLength)]
  public string LifecycleImpactCode
  {
    get => lifecycleImpactCode ?? "";
    set => lifecycleImpactCode =
      TrimEnd(Substring(value, 1, LifecycleImpactCode_MaxLength));
  }

  /// <summary>
  /// The json value of the LifecycleImpactCode attribute.</summary>
  [JsonPropertyName("lifecycleImpactCode")]
  [Computed]
  public string LifecycleImpactCode_Json
  {
    get => NullIf(LifecycleImpactCode, "");
    set => LifecycleImpactCode = value;
  }

  /// <summary>Length of the LOG_TO_DIARY_IND attribute.</summary>
  public const int LogToDiaryInd_MaxLength = 1;

  /// <summary>
  /// The value of the LOG_TO_DIARY_IND attribute.
  /// The Log to Diary Indicator identifies those events for which a Case Diary 
  /// entry will be created in Infrastructure.
  /// Values are:
  /// Y = Set Infrastructure occurance to retain as diary entry.
  /// N = Set infrastructure occurance to 'Process' and eventual deletion.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = LogToDiaryInd_MaxLength)]
  public string LogToDiaryInd
  {
    get => logToDiaryInd ?? "";
    set => logToDiaryInd =
      TrimEnd(Substring(value, 1, LogToDiaryInd_MaxLength));
  }

  /// <summary>
  /// The json value of the LogToDiaryInd attribute.</summary>
  [JsonPropertyName("logToDiaryInd")]
  [Computed]
  public string LogToDiaryInd_Json
  {
    get => NullIf(LogToDiaryInd, "");
    set => LogToDiaryInd = value;
  }

  /// <summary>
  /// The value of the DATE_MONITOR_DAYS attribute.
  /// Date Monitor Days is a parameter used by the batch date monitor routine.
  /// Date Monitor Days is a value added to the date parameter passed along with
  /// an Event to the batch routine.  The derived date is then compared to
  /// Current Date
  /// Date Monitor Days is always expressed as a number of days.
  /// </summary>
  [JsonPropertyName("dateMonitorDays")]
  [Member(Index = 10, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? DateMonitorDays
  {
    get => dateMonitorDays;
    set => dateMonitorDays = value;
  }

  /// <summary>
  /// The value of the NEXT_EVENT_ID attribute.
  /// Next event id is populated when a potential 'next' activity may be 
  /// automated for a given event detail.
  /// As event and event details are predefined, the combinatin of attributes 
  /// Next Event ID and Next Event Detail ID will map an event to a unique '
  /// next' event and detail.
  /// Currently this exists for documentation of outgoing CSENet transactions 
  /// which may be generated for an interstate case based on the occurance of
  /// normal, 'service' events.
  /// This indicator could also be used to support automatic notification of 
  /// appropriate parties in many situation.
  /// </summary>
  [JsonPropertyName("nextEventId")]
  [Member(Index = 11, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? NextEventId
  {
    get => nextEventId;
    set => nextEventId = value;
  }

  /// <summary>Length of the NEXT_EVENT_DETAIL_ID attribute.</summary>
  public const int NextEventDetailId_MaxLength = 3;

  /// <summary>
  /// The value of the NEXT_EVENT_DETAIL_ID attribute.
  /// </summary>
  [JsonPropertyName("nextEventDetailId")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = NextEventDetailId_MaxLength, Optional = true)]
  public string NextEventDetailId
  {
    get => nextEventDetailId;
    set => nextEventDetailId = value != null
      ? TrimEnd(Substring(value, 1, NextEventDetailId_MaxLength)) : null;
  }

  /// <summary>Length of the NEXT_INITIATING_STATE attribute.</summary>
  public const int NextInitiatingState_MaxLength = 2;

  /// <summary>
  /// The value of the NEXT_INITIATING_STATE attribute.
  /// Next initiating state is populated when a potential 'next' activity may be
  /// automated for a given event detail.
  /// Currently this exists for documentation of outgoing CSENet transactions 
  /// which may be generaed for an interstate case based on the occurance of
  /// normal, 'service' events.
  /// This indicator could also be used to support automatic notification of 
  /// appropriate parties in many situations.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = NextInitiatingState_MaxLength)]
  public string NextInitiatingState
  {
    get => nextInitiatingState ?? "";
    set => nextInitiatingState =
      TrimEnd(Substring(value, 1, NextInitiatingState_MaxLength));
  }

  /// <summary>
  /// The json value of the NextInitiatingState attribute.</summary>
  [JsonPropertyName("nextInitiatingState")]
  [Computed]
  public string NextInitiatingState_Json
  {
    get => NullIf(NextInitiatingState, "");
    set => NextInitiatingState = value;
  }

  /// <summary>Length of the NEXT_CSENET_IN_OUT attribute.</summary>
  public const int NextCsenetInOut_MaxLength = 1;

  /// <summary>
  /// The value of the NEXT_CSENET_IN_OUT attribute.
  /// Next CSENet in out is populated when a potential 'next' activity may be 
  /// automated for a given event detail.
  /// Currently this exists for documentation of outgoing CSENet transactions 
  /// which may be generated for an interstate case based on the occurance of
  /// normal, 'service' events.
  /// This indicator could also be used to support automatic notification of 
  /// appropriate parties in many situation.
  /// </summary>
  [JsonPropertyName("nextCsenetInOut")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = NextCsenetInOut_MaxLength, Optional = true)]
  public string NextCsenetInOut
  {
    get => nextCsenetInOut;
    set => nextCsenetInOut = value != null
      ? TrimEnd(Substring(value, 1, NextCsenetInOut_MaxLength)) : null;
  }

  /// <summary>Length of the NEXT_REASON attribute.</summary>
  public const int NextReason_MaxLength = 15;

  /// <summary>
  /// The value of the NEXT_REASON attribute.
  /// Next reason is populated when a potential 'next' activity may be automated
  /// for a given event detail.
  /// Currently this exists for documentation of outgoing CSENet transactions 
  /// which may be generated for an interstate case based on the occurance of
  /// normal, 'service' events.
  /// This indicator could also be used to support automatic notificaton of 
  /// appropriate parties in many situations.
  /// </summary>
  [JsonPropertyName("nextReason")]
  [Member(Index = 15, Type = MemberType.Char, Length = NextReason_MaxLength, Optional
    = true)]
  public string NextReason
  {
    get => nextReason;
    set => nextReason = value != null
      ? TrimEnd(Substring(value, 1, NextReason_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 16, Type = MemberType.Date)]
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
  [Member(Index = 17, Type = MemberType.Date)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// * Draft *
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// * Draft *
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 19, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// * Draft *
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_DTSTAMP attribute.
  /// * Draft *
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatedDtstamp")]
  [Member(Index = 21, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedDtstamp
  {
    get => lastUpdatedDtstamp;
    set => lastUpdatedDtstamp = value;
  }

  /// <summary>Length of the FUNCTION attribute.</summary>
  public const int Function_MaxLength = 3;

  /// <summary>
  /// The value of the FUNCTION attribute.
  /// This value represents one of the four federally defined child support 
  /// lifecycle functions.
  /// Valid values:
  ///              	LOC - Locate
  /// 		PAT - Paternity
  /// 		OBE - Obligation Establishment
  /// 		ENF - Enforcement
  /// </summary>
  [JsonPropertyName("function")]
  [Member(Index = 22, Type = MemberType.Char, Length = Function_MaxLength, Optional
    = true)]
  public string Function
  {
    get => function;
    set => function = value != null
      ? TrimEnd(Substring(value, 1, Function_MaxLength)) : null;
  }

  /// <summary>Length of the EXCEPTION_ROUTINE attribute.</summary>
  public const int ExceptionRoutine_MaxLength = 8;

  /// <summary>
  /// The value of the EXCEPTION_ROUTINE attribute.
  /// </summary>
  [JsonPropertyName("exceptionRoutine")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = ExceptionRoutine_MaxLength, Optional = true)]
  public string ExceptionRoutine
  {
    get => exceptionRoutine;
    set => exceptionRoutine = value != null
      ? TrimEnd(Substring(value, 1, ExceptionRoutine_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("eveNo")]
  [DefaultValue(0)]
  [Member(Index = 24, Type = MemberType.Number, Length = 5)]
  public int EveNo
  {
    get => eveNo;
    set => eveNo = value;
  }

  private int systemGeneratedIdentifier;
  private string detailName;
  private string description;
  private string initiatingStateCode;
  private string csenetInOutCode;
  private string reasonCode;
  private string procedureName;
  private string lifecycleImpactCode;
  private string logToDiaryInd;
  private int? dateMonitorDays;
  private int? nextEventId;
  private string nextEventDetailId;
  private string nextInitiatingState;
  private string nextCsenetInOut;
  private string nextReason;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedDtstamp;
  private string function;
  private string exceptionRoutine;
  private int eveNo;
}
