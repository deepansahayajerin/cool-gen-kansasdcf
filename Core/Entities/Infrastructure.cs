// The source file: INFRASTRUCTURE, ID: 371427616, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Infrastructure records are the mechanism by which service plan processes 
/// events raised from the application.  An Infrastructure record contains the
/// details required for generation of office service provider alerts, monitored
/// activities, monitored and recorded documents, and case diary entries.
/// Infrastructure records associate an event with the appropriate business 
/// objects and office service provider assignments.
/// Infrastructure represents the lifecycle of the event transaction.
/// The status of an Infrastructure occurance may be:
/// 	Queued
/// 	Processed
/// 	Retained (in diary entry)
/// DATA MODEL ALERT!!!!
/// *	Relationships between INFRASTRUCTURE and CASE, ADMINISTRATIVE ACT 
/// CERTIFICATION, ADMINISTRATIVE APPEAL, OBLIGATION ADMINISTRATIVE ACTION,
/// OBLIGATION, LEGAL ACTION, LEGAL REQUEST, PA REFERRAL, RECORDED DOCUMENT,
/// INTERSTATE CASE, and INFORMATION REQUEST are not drawn.
/// each INFRASTRUCTURE sometimes is identified by one (substitute entity type 
/// name here)
/// each (substitute entity type name here) sometimes identifies one or more 
/// INFRASTRUCTURE
/// </summary>
[Serializable]
public partial class Infrastructure: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Infrastructure()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Infrastructure(Infrastructure that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Infrastructure Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

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
  [Member(Index = 1, Type = MemberType.Char, Length = Function_MaxLength, Optional
    = true)]
  public string Function
  {
    get => Get<string>("function");
    set => Set("function", TrimEnd(Substring(value, 1, Function_MaxLength)));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => Get<int?>("systemGeneratedIdentifier") ?? 0;
    set => Set("systemGeneratedIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the SITUATION_NUMBER attribute.
  /// The Situation Identifier is used by the event processor for optimization 
  /// of Service Provider Alert distribution.
  /// When an event is raised in an application, one or more infrastructure 
  /// records are created and queued for event processign.  Multiple
  /// infrastructure records created by a single event are given the same
  /// situation identifier to enable the event processor to handle those records
  /// as a set.
  /// </summary>
  [JsonPropertyName("situationNumber")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 9)]
  public int SituationNumber
  {
    get => Get<int?>("situationNumber") ?? 0;
    set => Set("situationNumber", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the PROCESS_STATUS attribute.</summary>
  public const int ProcessStatus_MaxLength = 1;

  /// <summary>
  /// The value of the PROCESS_STATUS attribute.
  /// Process Status identifies the lifecycle state of the Infrastructure 
  /// transaction.
  /// A queued status indicates that the application has created an 
  /// infrastructure record(s) which is ready for processing.
  /// A processed status indicates that processing is complete for this 
  /// infrastructure record.  It may be purged from the data base.
  /// A retained status indicates that processing is complete for the 
  /// infrastructure record.  The record is retained for case history.
  /// Values are:
  /// 	Q = Queued
  /// 	P = Processed
  /// 	L = Logged to diary
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = ProcessStatus_MaxLength)]
  public string ProcessStatus
  {
    get => Get<string>("processStatus") ?? "";
    set => Set(
      "processStatus", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ProcessStatus_MaxLength)));
  }

  /// <summary>
  /// The json value of the ProcessStatus attribute.</summary>
  [JsonPropertyName("processStatus")]
  [Computed]
  public string ProcessStatus_Json
  {
    get => NullIf(ProcessStatus, "");
    set => ProcessStatus = value;
  }

  /// <summary>
  /// The value of the EVENT_ID attribute.
  /// </summary>
  [JsonPropertyName("eventId")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 5)]
  public int EventId
  {
    get => Get<int?>("eventId") ?? 0;
    set => Set("eventId", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the EVENT TYPE attribute.</summary>
  public const int EventType_MaxLength = 12;

  /// <summary>
  /// The value of the EVENT TYPE attribute.
  /// Type classifies an Event into one of the following categories.  Event Type
  /// codes are documented in Code Value Table.
  /// Event Types include:
  /// ADMINACT	Administrative Action
  /// ADMINAPL	Administrative Appeal
  /// EXNOTF		External Notification
  /// APDTL		AP Details
  /// COMPLIANCE	Compliance
  /// CASEUNIT	Case Unit Status
  /// CSENET		CSENet Transaction
  /// DOC		Document Generated
  /// DM		Date Monitor
  /// GENTST 		Genetic Test
  /// LEACT		Legal Action
  /// LEEVT		Legal Event
  /// LERFRL		Legal Referral
  /// LERESP		Legal Response
  /// MODFN		Support Modification
  /// OBLG		Obligation Established
  /// SERV		Process of Service
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = EventType_MaxLength)]
  public string EventType
  {
    get => Get<string>("eventType") ?? "";
    set => Set(
      "eventType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, EventType_MaxLength)));
  }

  /// <summary>
  /// The json value of the EventType attribute.</summary>
  [JsonPropertyName("eventType")]
  [Computed]
  public string EventType_Json
  {
    get => NullIf(EventType, "");
    set => EventType = value;
  }

  /// <summary>Length of the EVENT_DETAIL_NAME attribute.</summary>
  public const int EventDetailName_MaxLength = 40;

  /// <summary>
  /// The value of the EVENT_DETAIL_NAME attribute.
  /// The Event Name of the initiating transaction which resulted in creation of
  /// the Infrastructure occurance.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = EventDetailName_MaxLength)]
    
  public string EventDetailName
  {
    get => Get<string>("eventDetailName") ?? "";
    set => Set(
      "eventDetailName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, EventDetailName_MaxLength)));
  }

  /// <summary>
  /// The json value of the EventDetailName attribute.</summary>
  [JsonPropertyName("eventDetailName")]
  [Computed]
  public string EventDetailName_Json
  {
    get => NullIf(EventDetailName, "");
    set => EventDetailName = value;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 15;

  /// <summary>
  /// The value of the REASON_CODE attribute.
  /// The Reason Code identifies the detailed reason for a transaction.
  /// If the event is E_DOCUMENT_GENERATED, the reason code will carry the value
  /// of the document name.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ReasonCode_MaxLength)]
  public string ReasonCode
  {
    get => Get<string>("reasonCode") ?? "";
    set => Set(
      "reasonCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ReasonCode_MaxLength)));
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

  /// <summary>Length of the BUSINESS_OBJECT_CD attribute.</summary>
  public const int BusinessObjectCd_MaxLength = 3;

  /// <summary>
  /// The value of the BUSINESS_OBJECT_CD attribute.
  /// The Business Object represents the primary entity type to which the 
  /// business object belongs.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = BusinessObjectCd_MaxLength)
    ]
  public string BusinessObjectCd
  {
    get => Get<string>("businessObjectCd") ?? "";
    set => Set(
      "businessObjectCd", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, BusinessObjectCd_MaxLength)));
  }

  /// <summary>
  /// The json value of the BusinessObjectCd attribute.</summary>
  [JsonPropertyName("businessObjectCd")]
  [Computed]
  public string BusinessObjectCd_Json
  {
    get => NullIf(BusinessObjectCd, "");
    set => BusinessObjectCd = value;
  }

  /// <summary>
  /// The value of the DENORM_NUMERIC_12 attribute.
  /// Denorm Numeric 12 is one of a set of four attributes used to support a 
  /// denormalized identifier for the business object identified through the
  /// business object code.
  /// This set of attributes includes:
  /// DENORM_NUMBERIC_12
  /// DENORM_TEXT_12
  /// DENORM_DATE
  /// DENORM_TIMESTAMP
  /// </summary>
  [JsonPropertyName("denormNumeric12")]
  [Member(Index = 10, Type = MemberType.Number, Length = 12, Optional = true)]
  public long? DenormNumeric12
  {
    get => Get<long?>("denormNumeric12");
    set => Set("denormNumeric12", value);
  }

  /// <summary>Length of the DENORM TEXT 12 attribute.</summary>
  public const int DenormText12_MaxLength = 12;

  /// <summary>
  /// The value of the DENORM TEXT 12 attribute.
  /// Denorm Text 12 is one of a set of four attributes used to support a 
  /// denormalized identifier for the business object identified through the
  /// business object code.
  /// This set of attributes includes:
  /// DENORM_NUMBERIC_12
  /// DENORM_TEXT_12
  /// DENORM_DATE
  /// DENORM_TIMESTAMP
  /// </summary>
  [JsonPropertyName("denormText12")]
  [Member(Index = 11, Type = MemberType.Char, Length = DenormText12_MaxLength, Optional
    = true)]
  public string DenormText12
  {
    get => Get<string>("denormText12");
    set => Set(
      "denormText12", TrimEnd(Substring(value, 1, DenormText12_MaxLength)));
  }

  /// <summary>
  /// The value of the DENORM DATE attribute.
  /// Denorm Date 12 is one of a set of four attributes used to support a 
  /// denormalized identifier for the business object identified through the
  /// business object code.
  /// This set of attributes includes:
  /// DENORM_NUMBERIC_12
  /// DENORM_TEXT_12
  /// DENORM_DATE
  /// DENORM_TIMESTAMP
  /// </summary>
  [JsonPropertyName("denormDate")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? DenormDate
  {
    get => Get<DateTime?>("denormDate");
    set => Set("denormDate", value);
  }

  /// <summary>
  /// The value of the DENORM TIMESTAMP attribute.
  /// Denorm Timestamp 12 is one of a set of four attributes used to support a 
  /// denormalized identifier for the business object identified through the
  /// business object code.
  /// This set of attributes includes:
  /// DENORM_NUMBERIC_12
  /// DENORM_TEXT_12
  /// DENORM_DATE
  /// DENORM_TIMESTAMP
  /// </summary>
  [JsonPropertyName("denormTimestamp")]
  [Member(Index = 13, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? DenormTimestamp
  {
    get => Get<DateTime?>("denormTimestamp");
    set => Set("denormTimestamp", value);
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
  [Member(Index = 14, Type = MemberType.Char, Length
    = InitiatingStateCode_MaxLength)]
  public string InitiatingStateCode
  {
    get => Get<string>("initiatingStateCode") ?? "";
    set => Set(
      "initiatingStateCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, InitiatingStateCode_MaxLength)));
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
  [Member(Index = 15, Type = MemberType.Char, Length = CsenetInOutCode_MaxLength)
    ]
  public string CsenetInOutCode
  {
    get => Get<string>("csenetInOutCode") ?? "";
    set => Set(
      "csenetInOutCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CsenetInOutCode_MaxLength)));
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

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// The Case Identifier associated with this infrastructure record.
  /// </summary>
  [JsonPropertyName("caseNumber")]
  [Member(Index = 16, Type = MemberType.Char, Length = CaseNumber_MaxLength, Optional
    = true)]
  public string CaseNumber
  {
    get => Get<string>("caseNumber");
    set =>
      Set("caseNumber", TrimEnd(Substring(value, 1, CaseNumber_MaxLength)));
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// The CSE Person Identifier for the person associated with this 
  /// infrastructure record.
  /// </summary>
  [JsonPropertyName("csePersonNumber")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = CsePersonNumber_MaxLength, Optional = true)]
  public string CsePersonNumber
  {
    get => Get<string>("csePersonNumber");
    set => Set(
      "csePersonNumber",
      TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the CASE_UNIT_NUMBER attribute.
  /// The Case Unit Identifier associated with this infrastructure record.
  /// </summary>
  [JsonPropertyName("caseUnitNumber")]
  [Member(Index = 18, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CaseUnitNumber
  {
    get => Get<int?>("caseUnitNumber");
    set => Set("caseUnitNumber", value);
  }

  /// <summary>Length of the USER_ID attribute.</summary>
  public const int UserId_MaxLength = 8;

  /// <summary>
  /// The value of the USER_ID attribute.
  /// User ID identifies the user logged on at the time of the data 
  /// transformation event.
  /// The User ID may not be associated to the service provider assigned to any 
  /// of the business objects involved in the transaction.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = UserId_MaxLength)]
  public string UserId
  {
    get => Get<string>("userId") ?? "";
    set => Set(
      "userId", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, UserId_MaxLength)));
  }

  /// <summary>
  /// The json value of the UserId attribute.</summary>
  [JsonPropertyName("userId")]
  [Computed]
  public string UserId_Json
  {
    get => NullIf(UserId, "");
    set => UserId = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => Get<string>("createdBy") ?? "";
    set => Set(
      "createdBy", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CreatedBy_MaxLength)));
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
  [Member(Index = 21, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 23, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => Get<DateTime?>("lastUpdatedTimestamp");
    set => Set("lastUpdatedTimestamp", value);
  }

  /// <summary>
  /// The value of the REFERENCE_DATE attribute.
  /// THIS DATE IS USED BY EVENT PROCESSOR TO DETERMINE THE APPROPRIATE DATE TO 
  /// BE SET ON SPAWNED ALERTS AND ACTIVTIVES
  /// </summary>
  [JsonPropertyName("referenceDate")]
  [Member(Index = 24, Type = MemberType.Date, Optional = true)]
  public DateTime? ReferenceDate
  {
    get => Get<DateTime?>("referenceDate");
    set => Set("referenceDate", value);
  }

  /// <summary>Length of the DETAIL attribute.</summary>
  public const int Detail_MaxLength = 75;

  /// <summary>
  /// The value of the DETAIL attribute.
  /// Holds additional information to identify the record because sometimes a 
  /// case number, cse person number, case unit number and court order
  /// number (these are the attributes on the screen) are not enough.
  /// </summary>
  [JsonPropertyName("detail")]
  [Member(Index = 25, Type = MemberType.Varchar, Length = Detail_MaxLength, Optional
    = true)]
  public string Detail
  {
    get => Get<string>("detail");
    set => Set("detail", Substring(value, 1, Detail_MaxLength));
  }

  /// <summary>Length of the CASE_UNIT_STATE attribute.</summary>
  public const int CaseUnitState_MaxLength = 5;

  /// <summary>
  /// The value of the CASE_UNIT_STATE attribute.
  /// This attribute represents the Life Cycle state properties of a CASE UNIT 
  /// immediately prior to the creation of an INFRASTRUCTURE entity. It is
  /// expected that this attribute will be used by quality assurance to
  /// determine the Life Cycle state of MONITORED ACTIVITY.
  /// Position 1 = function  values (LPOE)
  /// Position 2 = service type  values (FLE)
  /// Position 3 = is located   values (YN)
  /// Position 4 = is a AP     values (YNU)
  /// Position 5 = is obligated   values (YNU)
  /// REASON Required for QA in order to determine the functions that existed 
  /// prior to generation of monitored activity. Is critical to QA reporting.
  /// </summary>
  [JsonPropertyName("caseUnitState")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = CaseUnitState_MaxLength, Optional = true)]
  public string CaseUnitState
  {
    get => Get<string>("caseUnitState");
    set => Set(
      "caseUnitState", TrimEnd(Substring(value, 1, CaseUnitState_MaxLength)));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgId")]
  [Member(Index = 27, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ObgId
  {
    get => Get<int?>("obgId");
    set => Set("obgId", value);
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNo_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNo")]
  [Member(Index = 28, Type = MemberType.Char, Length = CspNo_MaxLength, Optional
    = true)]
  public string CspNo
  {
    get => Get<string>("cspNo");
    set => Set("cspNo", TrimEnd(Substring(value, 1, CspNo_MaxLength)));
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
  [JsonPropertyName("cpaType")]
  [Member(Index = 29, Type = MemberType.Char, Length = CpaType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => Get<string>("cpaType");
    set => Set("cpaType", TrimEnd(Substring(value, 1, CpaType_MaxLength)));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyId")]
  [Member(Index = 30, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OtyId
  {
    get => Get<int?>("otyId");
    set => Set("otyId", value);
  }

  /// <summary>
  /// The value of the TAKEN_DATE attribute.
  /// The date that an enforcement action is taken against an Obligor for a 
  /// particular Obligation.
  /// </summary>
  [JsonPropertyName("oaaDate")]
  [Member(Index = 31, Type = MemberType.Date, Optional = true)]
  public DateTime? OaaDate
  {
    get => Get<DateTime?>("oaaDate");
    set => Set("oaaDate", value);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaOaaType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonPropertyName("cpaOaaType")]
  [Member(Index = 32, Type = MemberType.Char, Length = CpaOaaType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaOaaType
  {
    get => Get<string>("cpaOaaType");
    set =>
      Set("cpaOaaType", TrimEnd(Substring(value, 1, CpaOaaType_MaxLength)));
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspOaaNo_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspOaaNo")]
  [Member(Index = 33, Type = MemberType.Char, Length = CspOaaNo_MaxLength, Optional
    = true)]
  public string CspOaaNo
  {
    get => Get<string>("cspOaaNo");
    set => Set("cspOaaNo", TrimEnd(Substring(value, 1, CspOaaNo_MaxLength)));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgOaaId")]
  [Member(Index = 34, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ObgOaaId
  {
    get => Get<int?>("obgOaaId");
    set => Set("obgOaaId", value);
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
  [JsonPropertyName("aatType")]
  [Member(Index = 35, Type = MemberType.Char, Length = AatType_MaxLength, Optional
    = true)]
  public string AatType
  {
    get => Get<string>("aatType");
    set => Set("aatType", TrimEnd(Substring(value, 1, AatType_MaxLength)));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyOaaId")]
  [Member(Index = 36, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OtyOaaId
  {
    get => Get<int?>("otyOaaId");
    set => Set("otyOaaId", value);
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("pafTstamp")]
  [Member(Index = 37, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? PafTstamp
  {
    get => Get<DateTime?>("pafTstamp");
    set => Set("pafTstamp", value);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int PafType_MaxLength = 6;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This will indicate whether this Referral is:
  /// 'New'
  /// 'Reopen'
  /// 'Change'
  /// </summary>
  [JsonPropertyName("pafType")]
  [Member(Index = 38, Type = MemberType.Char, Length = PafType_MaxLength, Optional
    = true)]
  public string PafType
  {
    get => Get<string>("pafType");
    set => Set("pafType", TrimEnd(Substring(value, 1, PafType_MaxLength)));
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int PafNo_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// Referral Number (Unique identifier)
  /// </summary>
  [JsonPropertyName("pafNo")]
  [Member(Index = 39, Type = MemberType.Char, Length = PafNo_MaxLength, Optional
    = true)]
  public string PafNo
  {
    get => Get<string>("pafNo");
    set => Set("pafNo", TrimEnd(Substring(value, 1, PafNo_MaxLength)));
  }
}
