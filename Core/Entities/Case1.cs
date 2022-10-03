// The source file: CASE, ID: 371421260, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// 	    CSE CASE STRUCTURE
/// A Case is a group of established and/or alleged relationships of CSE 
/// Persons.
/// A Case revolves around a child or children, of the same parents, living in 
/// the same household.
/// A Case requires an AR (since it requires a valid application or mandated 
/// need for services).
/// A child can actively participate on only one case. A case may contain one or
/// more programs.
/// A case requires the absence of a parent at some point in time, and a valid 
/// referral or application for CSE services (with the exception of Expedited
/// Paternity services in which the absence of a parent is based upon the
/// absence of Paternity Establishment, rather than upon a physical absence from
/// the home).
/// The concept of an application remains for the initial stage of a case; a 
/// valid application &quot;is&quot; a case. In the same manner, a valid
/// referral is also a case.
/// Validation of the case (determination if a duplicate case exists) occurs 
/// before a case number is assigned and is handled by Service Initiation.  If a
/// duplicate case exists, the applicaiton is accepted and the existing case is
/// updated.
/// CASE SCENARIOS:
/// If children do not live in the same household, each different household 
/// would be a different case (e.g., parents split the children, caretaker
/// relative).
/// If children reside in the same household but have different parents, they 
/// are different cases (e.g., a mother and two children with two different
/// fathers would be two different cases).
/// If there is more thatn one alleged AP (for a specific child), all alleged 
/// APs (for that child) are participants on the same case.
/// A separate case will be established for each child in Foster Care or in a 
/// state institution.  Both the mother and father would be APs on a Foster Care
/// or Institutional case.
/// </summary>
[Serializable]
public partial class Case1: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Case1()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Case1(Case1 that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Case1 Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int Number_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Number_MaxLength)]
  public string Number
  {
    get => Get<string>("number") ?? "";
    set => Set(
      "number", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Number_MaxLength)));
  }

  /// <summary>
  /// The json value of the Number attribute.</summary>
  [JsonPropertyName("number")]
  [Computed]
  public string Number_Json
  {
    get => NullIf(Number, "");
    set => Number = value;
  }

  /// <summary>
  /// The value of the IC_TRANS_SERIAL_NUMBER attribute.
  /// This is a unique number assigned to each CSENet transaction.  It has no 
  /// place in the KESSEP system but is required to provide a key used to
  /// process CSENet Referrals.
  /// </summary>
  [JsonPropertyName("icTransSerialNumber")]
  [DefaultValue(0L)]
  [Member(Index = 2, Type = MemberType.Number, Length = 12)]
  public long IcTransSerialNumber
  {
    get => Get<long?>("icTransSerialNumber") ?? 0L;
    set => Set("icTransSerialNumber", value == 0L ? null : value as long?);
  }

  /// <summary>
  /// The value of the IC_TRANSACTION_DATE attribute.
  /// This is the date on which CSENet transmitted the Referral.
  /// </summary>
  [JsonPropertyName("icTransactionDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? IcTransactionDate
  {
    get => Get<DateTime?>("icTransactionDate");
    set => Set("icTransactionDate", value);
  }

  /// <summary>
  /// The value of the PA_REF_CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("paRefCreatedTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? PaRefCreatedTimestamp
  {
    get => Get<DateTime?>("paRefCreatedTimestamp");
    set => Set("paRefCreatedTimestamp", value);
  }

  /// <summary>Length of the PA_REFERRAL_NUMBER attribute.</summary>
  public const int PaReferralNumber_MaxLength = 10;

  /// <summary>
  /// The value of the PA_REFERRAL_NUMBER attribute.
  /// Referral Number (Unique identifier)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = PaReferralNumber_MaxLength)
    ]
  public string PaReferralNumber
  {
    get => Get<string>("paReferralNumber") ?? "";
    set => Set(
      "paReferralNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, PaReferralNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the PaReferralNumber attribute.</summary>
  [JsonPropertyName("paReferralNumber")]
  [Computed]
  public string PaReferralNumber_Json
  {
    get => NullIf(PaReferralNumber, "");
    set => PaReferralNumber = value;
  }

  /// <summary>Length of the PA_REFERRAL_TYPE attribute.</summary>
  public const int PaReferralType_MaxLength = 6;

  /// <summary>
  /// The value of the PA_REFERRAL_TYPE attribute.
  /// This will indicate whether this Referral is:
  /// 'New'
  /// 'Reopen'
  /// 'Change'
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = PaReferralType_MaxLength)]
  public string PaReferralType
  {
    get => Get<string>("paReferralType") ?? "";
    set => Set(
      "paReferralType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, PaReferralType_MaxLength)));
  }

  /// <summary>
  /// The json value of the PaReferralType attribute.</summary>
  [JsonPropertyName("paReferralType")]
  [Computed]
  public string PaReferralType_Json
  {
    get => NullIf(PaReferralType, "");
    set => PaReferralType = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the EXPEDITED_PATERNITY_IND attribute.</summary>
  public const int ExpeditedPaternityInd_MaxLength = 1;

  /// <summary>
  /// The value of the EXPEDITED_PATERNITY_IND attribute.
  /// Indicates that this case was opened by CSE in order to establish paternity
  /// as requested by an Economic Assistance worker.
  /// </summary>
  [JsonPropertyName("expeditedPaternityInd")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = ExpeditedPaternityInd_MaxLength, Optional = true)]
  public string ExpeditedPaternityInd
  {
    get => Get<string>("expeditedPaternityInd");
    set => Set(
      "expeditedPaternityInd", TrimEnd(Substring(value, 1,
      ExpeditedPaternityInd_MaxLength)));
  }

  /// <summary>
  /// The value of the OFFICE_IDENTIFIER attribute.
  /// This identifies the office within the MANAGEMENT_AREA specified.
  /// SECURITY!
  /// </summary>
  [JsonPropertyName("officeIdentifier")]
  [Member(Index = 10, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OfficeIdentifier
  {
    get => Get<int?>("officeIdentifier");
    set => Set("officeIdentifier", value);
  }

  /// <summary>Length of the MANAGEMENT_AREA attribute.</summary>
  public const int ManagementArea_MaxLength = 3;

  /// <summary>
  /// The value of the MANAGEMENT_AREA attribute.
  /// This is the level below MANAGEMENT_REGION. (This represents the 'team' in 
  /// the old system.)
  /// </summary>
  [JsonPropertyName("managementArea")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = ManagementArea_MaxLength, Optional = true)]
  public string ManagementArea
  {
    get => Get<string>("managementArea");
    set => Set(
      "managementArea", TrimEnd(Substring(value, 1, ManagementArea_MaxLength)));
      
  }

  /// <summary>Length of the MANAGEMENT_REGION attribute.</summary>
  public const int ManagementRegion_MaxLength = 3;

  /// <summary>
  /// The value of the MANAGEMENT_REGION attribute.
  /// The highest CSE organizational level.  Usage on this attribute is for 
  /// SECURITY
  /// </summary>
  [JsonPropertyName("managementRegion")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = ManagementRegion_MaxLength, Optional = true)]
  public string ManagementRegion
  {
    get => Get<string>("managementRegion");
    set => Set(
      "managementRegion",
      TrimEnd(Substring(value, 1, ManagementRegion_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 13, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => Get<DateTime?>("lastUpdatedTimestamp");
    set => Set("lastUpdatedTimestamp", value);
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
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>Length of the FULL_SERVICE_WITHOUT_MED_IND attribute.</summary>
  public const int FullServiceWithoutMedInd_MaxLength = 1;

  /// <summary>
  /// The value of the FULL_SERVICE_WITHOUT_MED_IND attribute.
  /// Indicates that the Non-ADC AR has requested that CSE provide full service 
  /// without medical on their behalf.
  /// </summary>
  [JsonPropertyName("fullServiceWithoutMedInd")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = FullServiceWithoutMedInd_MaxLength, Optional = true)]
  public string FullServiceWithoutMedInd
  {
    get => Get<string>("fullServiceWithoutMedInd");
    set => Set(
      "fullServiceWithoutMedInd", TrimEnd(Substring(value, 1,
      FullServiceWithoutMedInd_MaxLength)));
  }

  /// <summary>Length of the FULL_SERVICE_WITH_MED_IND attribute.</summary>
  public const int FullServiceWithMedInd_MaxLength = 1;

  /// <summary>
  /// The value of the FULL_SERVICE_WITH_MED_IND attribute.
  /// Indicates that the Non-ADC AR has requested that CSE provide full service 
  /// with medical on their behalf.
  /// </summary>
  [JsonPropertyName("fullServiceWithMedInd")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = FullServiceWithMedInd_MaxLength, Optional = true)]
  public string FullServiceWithMedInd
  {
    get => Get<string>("fullServiceWithMedInd");
    set => Set(
      "fullServiceWithMedInd", TrimEnd(Substring(value, 1,
      FullServiceWithMedInd_MaxLength)));
  }

  /// <summary>Length of the LOCATE_IND attribute.</summary>
  public const int LocateInd_MaxLength = 1;

  /// <summary>
  /// The value of the LOCATE_IND attribute.
  /// Indicates that the Non-ADC AR has requested that CSE provide locate only 
  /// services on their behalf. For Interstate cases, this indicates that this
  /// is an Incoming Full Locate Only case.
  /// </summary>
  [JsonPropertyName("locateInd")]
  [Member(Index = 17, Type = MemberType.Char, Length = LocateInd_MaxLength, Optional
    = true)]
  public string LocateInd
  {
    get => Get<string>("locateInd");
    set => Set("locateInd", TrimEnd(Substring(value, 1, LocateInd_MaxLength)));
  }

  /// <summary>Length of the CLOSURE_REASON attribute.</summary>
  public const int ClosureReason_MaxLength = 2;

  /// <summary>
  /// The value of the CLOSURE_REASON attribute.
  /// Indicates the reason the IV-D case was closed.
  /// 	AD - AP Deceased
  /// 	LC - Lack of Contact
  /// 	NL - No Locate
  /// 	NP - Paternity Cannot be Established
  /// 	CC - Collection Complete
  /// 	NA - No Authorization
  /// 	NC - No Contact
  /// 	PR - Participant Request
  /// 	OT - Other
  /// </summary>
  [JsonPropertyName("closureReason")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = ClosureReason_MaxLength, Optional = true)]
  public string ClosureReason
  {
    get => Get<string>("closureReason");
    set => Set(
      "closureReason", TrimEnd(Substring(value, 1, ClosureReason_MaxLength)));
  }

  /// <summary>
  /// The value of the INFORMATION_REQUEST_NUMBER attribute.
  /// A system generated number used soley to identify information requests.  
  /// This is also the application number and will be printed on the
  /// applciation.
  /// </summary>
  [JsonPropertyName("informationRequestNumber")]
  [Member(Index = 19, Type = MemberType.Number, Length = 10, Optional = true)]
  public long? InformationRequestNumber
  {
    get => Get<long?>("informationRequestNumber");
    set => Set("informationRequestNumber", value);
  }

  /// <summary>Length of the STATION_ID attribute.</summary>
  public const int StationId_MaxLength = 7;

  /// <summary>
  /// The value of the STATION_ID attribute.
  /// </summary>
  [JsonPropertyName("stationId")]
  [Member(Index = 20, Type = MemberType.Char, Length = StationId_MaxLength, Optional
    = true)]
  public string StationId
  {
    get => Get<string>("stationId");
    set => Set("stationId", TrimEnd(Substring(value, 1, StationId_MaxLength)));
  }

  /// <summary>Length of the APPLICANT_LAST_NAME attribute.</summary>
  public const int ApplicantLastName_MaxLength = 17;

  /// <summary>
  /// The value of the APPLICANT_LAST_NAME attribute.
  /// The last name of the applicant
  /// </summary>
  [JsonPropertyName("applicantLastName")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = ApplicantLastName_MaxLength, Optional = true)]
  public string ApplicantLastName
  {
    get => Get<string>("applicantLastName");
    set => Set(
      "applicantLastName", TrimEnd(Substring(value, 1,
      ApplicantLastName_MaxLength)));
  }

  /// <summary>Length of the APPLICANT_FIRST_NAME attribute.</summary>
  public const int ApplicantFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the APPLICANT_FIRST_NAME attribute.
  /// The first name of the applicant
  /// </summary>
  [JsonPropertyName("applicantFirstName")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = ApplicantFirstName_MaxLength, Optional = true)]
  public string ApplicantFirstName
  {
    get => Get<string>("applicantFirstName");
    set => Set(
      "applicantFirstName", TrimEnd(Substring(value, 1,
      ApplicantFirstName_MaxLength)));
  }

  /// <summary>Length of the APPLICANT_MIDDLE_INITIAL attribute.</summary>
  public const int ApplicantMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the APPLICANT_MIDDLE_INITIAL attribute.
  /// The middle initial of an applicant
  /// </summary>
  [JsonPropertyName("applicantMiddleInitial")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = ApplicantMiddleInitial_MaxLength, Optional = true)]
  public string ApplicantMiddleInitial
  {
    get => Get<string>("applicantMiddleInitial");
    set => Set(
      "applicantMiddleInitial", TrimEnd(Substring(value, 1,
      ApplicantMiddleInitial_MaxLength)));
  }

  /// <summary>
  /// The value of the APPLICATION_SENT_DATE attribute.
  /// The date the application was sent
  /// </summary>
  [JsonPropertyName("applicationSentDate")]
  [Member(Index = 24, Type = MemberType.Date, Optional = true)]
  public DateTime? ApplicationSentDate
  {
    get => Get<DateTime?>("applicationSentDate");
    set => Set("applicationSentDate", value);
  }

  /// <summary>
  /// The value of the APPLICATION_REQUEST_DATE attribute.
  /// The date the application is requested
  /// </summary>
  [JsonPropertyName("applicationRequestDate")]
  [Member(Index = 25, Type = MemberType.Date, Optional = true)]
  public DateTime? ApplicationRequestDate
  {
    get => Get<DateTime?>("applicationRequestDate");
    set => Set("applicationRequestDate", value);
  }

  /// <summary>
  /// The value of the APPLICATION_RETURN_DATE attribute.
  /// The date the application was returned
  /// </summary>
  [JsonPropertyName("applicationReturnDate")]
  [Member(Index = 26, Type = MemberType.Date, Optional = true)]
  public DateTime? ApplicationReturnDate
  {
    get => Get<DateTime?>("applicationReturnDate");
    set => Set("applicationReturnDate", value);
  }

  /// <summary>
  /// The value of the DENIED_REQUEST_DATE attribute.
  /// </summary>
  [JsonPropertyName("deniedRequestDate")]
  [Member(Index = 27, Type = MemberType.Date, Optional = true)]
  public DateTime? DeniedRequestDate
  {
    get => Get<DateTime?>("deniedRequestDate");
    set => Set("deniedRequestDate", value);
  }

  /// <summary>Length of the DENIED_REQUEST_CODE attribute.</summary>
  public const int DeniedRequestCode_MaxLength = 2;

  /// <summary>
  /// The value of the DENIED_REQUEST_CODE attribute.
  /// </summary>
  [JsonPropertyName("deniedRequestCode")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = DeniedRequestCode_MaxLength, Optional = true)]
  public string DeniedRequestCode
  {
    get => Get<string>("deniedRequestCode");
    set => Set(
      "deniedRequestCode", TrimEnd(Substring(value, 1,
      DeniedRequestCode_MaxLength)));
  }

  /// <summary>Length of the DENIED_REQUEST_REASON attribute.</summary>
  public const int DeniedRequestReason_MaxLength = 78;

  /// <summary>
  /// The value of the DENIED_REQUEST_REASON attribute.
  /// </summary>
  [JsonPropertyName("deniedRequestReason")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = DeniedRequestReason_MaxLength, Optional = true)]
  public string DeniedRequestReason
  {
    get => Get<string>("deniedRequestReason");
    set => Set(
      "deniedRequestReason", TrimEnd(Substring(value, 1,
      DeniedRequestReason_MaxLength)));
  }

  /// <summary>Length of the STATUS attribute.</summary>
  public const int Status_MaxLength = 1;

  /// <summary>
  /// The value of the STATUS attribute.
  /// The current status of the case
  /// e.g. Opened, Closed
  /// </summary>
  [JsonPropertyName("status")]
  [Member(Index = 30, Type = MemberType.Char, Length = Status_MaxLength, Optional
    = true)]
  public string Status
  {
    get => Get<string>("status");
    set => Set("status", TrimEnd(Substring(value, 1, Status_MaxLength)));
  }

  /// <summary>Length of the KS_FIPS_CODE attribute.</summary>
  public const int KsFipsCode_MaxLength = 5;

  /// <summary>
  /// The value of the KS_FIPS_CODE attribute.
  /// </summary>
  [JsonPropertyName("ksFipsCode")]
  [Member(Index = 31, Type = MemberType.Char, Length = KsFipsCode_MaxLength, Optional
    = true)]
  public string KsFipsCode
  {
    get => Get<string>("ksFipsCode");
    set =>
      Set("ksFipsCode", TrimEnd(Substring(value, 1, KsFipsCode_MaxLength)));
  }

  /// <summary>
  /// The value of the VALID_APPLICATION_RECEIVED_DATE attribute.
  /// This is the date that CSE received a complete and accurate application.
  /// </summary>
  [JsonPropertyName("validApplicationReceivedDate")]
  [Member(Index = 32, Type = MemberType.Date, Optional = true)]
  public DateTime? ValidApplicationReceivedDate
  {
    get => Get<DateTime?>("validApplicationReceivedDate");
    set => Set("validApplicationReceivedDate", value);
  }

  /// <summary>
  /// The value of the STATUS_DATE attribute.
  /// The date the current status was set
  /// </summary>
  [JsonPropertyName("statusDate")]
  [Member(Index = 33, Type = MemberType.Date, Optional = true)]
  public DateTime? StatusDate
  {
    get => Get<DateTime?>("statusDate");
    set => Set("statusDate", value);
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 80;

  /// <summary>
  /// The value of the NOTE attribute.
  /// Freeform area for any additional information.
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 34, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => Get<string>("note");
    set => Set("note", Substring(value, 1, Note_MaxLength));
  }

  /// <summary>Length of the POTENTIAL attribute.</summary>
  public const int Potential_MaxLength = 6;

  /// <summary>
  /// The value of the POTENTIAL attribute.
  /// Potential for recovery for each case.  Three Values are possible:
  /// Low - minimal potential for recovery, minimal effort expended.
  /// Medium - Reasonable chance of recovery.  Most effort is expended here.
  /// High - Excellent potential for recovery.  High effort, but there are few 
  /// of these.
  /// </summary>
  [JsonPropertyName("potential")]
  [Member(Index = 35, Type = MemberType.Char, Length = Potential_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("HIGH")]
  [Value("MEDIUM")]
  [Value("LOW")]
  public string Potential
  {
    get => Get<string>("potential");
    set => Set("potential", TrimEnd(Substring(value, 1, Potential_MaxLength)));
  }

  /// <summary>
  /// The value of the CSE_OPEN_DATE attribute.
  /// The date the case was established
  /// FR(III-2)
  /// </summary>
  [JsonPropertyName("cseOpenDate")]
  [Member(Index = 36, Type = MemberType.Date, Optional = true)]
  public DateTime? CseOpenDate
  {
    get => Get<DateTime?>("cseOpenDate");
    set => Set("cseOpenDate", value);
  }

  /// <summary>Length of the PA_MEDICAL_SERVICE attribute.</summary>
  public const int PaMedicalService_MaxLength = 2;

  /// <summary>
  /// The value of the PA_MEDICAL_SERVICE attribute.
  /// A code indicating whether or not the AR wants medical support services 
  /// only or a child support services in addition to medical support services.
  /// MO - Medical Only
  /// MC - Mdical and Child Support
  /// </summary>
  [JsonPropertyName("paMedicalService")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = PaMedicalService_MaxLength, Optional = true)]
  public string PaMedicalService
  {
    get => Get<string>("paMedicalService");
    set => Set(
      "paMedicalService",
      TrimEnd(Substring(value, 1, PaMedicalService_MaxLength)));
  }

  /// <summary>
  /// The value of the CLOSURE_LETTER_DATE attribute.
  /// Date the case was closed.
  /// </summary>
  [JsonPropertyName("closureLetterDate")]
  [Member(Index = 38, Type = MemberType.Date, Optional = true)]
  public DateTime? ClosureLetterDate
  {
    get => Get<DateTime?>("closureLetterDate");
    set => Set("closureLetterDate", value);
  }

  /// <summary>Length of the INTERSTATE_CASE_ID attribute.</summary>
  public const int InterstateCaseId_MaxLength = 10;

  /// <summary>
  /// The value of the INTERSTATE_CASE_ID attribute.
  /// Interstate Case that this CSE Case orginated from.
  /// </summary>
  [JsonPropertyName("interstateCaseId")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = InterstateCaseId_MaxLength, Optional = true)]
  public string InterstateCaseId
  {
    get => Get<string>("interstateCaseId");
    set => Set(
      "interstateCaseId",
      TrimEnd(Substring(value, 1, InterstateCaseId_MaxLength)));
  }

  /// <summary>
  /// The value of the ADC_OPEN_DATE attribute.
  /// Most recent date the IV_A (ADC) case opened.
  /// </summary>
  [JsonPropertyName("adcOpenDate")]
  [Member(Index = 40, Type = MemberType.Date, Optional = true)]
  public DateTime? AdcOpenDate
  {
    get => Get<DateTime?>("adcOpenDate");
    set => Set("adcOpenDate", value);
  }

  /// <summary>
  /// The value of the ADC_CLOSE_DATE attribute.
  /// Most recent date the IV_A (ADC) case closed.
  /// </summary>
  [JsonPropertyName("adcCloseDate")]
  [Member(Index = 41, Type = MemberType.Date, Optional = true)]
  public DateTime? AdcCloseDate
  {
    get => Get<DateTime?>("adcCloseDate");
    set => Set("adcCloseDate", value);
  }

  /// <summary>Length of the DUPLICATE_CASE_INDICATOR attribute.</summary>
  public const int DuplicateCaseIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the DUPLICATE_CASE_INDICATOR attribute.
  /// This flag indicates that the CSE Case is a Kansas Case with incoming 
  /// interstate involvement.
  /// </summary>
  [JsonPropertyName("duplicateCaseIndicator")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = DuplicateCaseIndicator_MaxLength, Optional = true)]
  public string DuplicateCaseIndicator
  {
    get => Get<string>("duplicateCaseIndicator");
    set => Set(
      "duplicateCaseIndicator", TrimEnd(Substring(value, 1,
      DuplicateCaseIndicator_MaxLength)));
  }

  /// <summary>Length of the LAST_CASE_EVENT attribute.</summary>
  public const int LastCaseEvent_MaxLength = 1;

  /// <summary>
  /// The value of the LAST_CASE_EVENT attribute.
  /// This attribute is populated with the most recent status event for all 
  /// debt_details for this Case. The values are blank for none, A for
  /// Activation, or D for Deactivation.
  /// </summary>
  [JsonPropertyName("lastCaseEvent")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = LastCaseEvent_MaxLength, Optional = true)]
  public string LastCaseEvent
  {
    get => Get<string>("lastCaseEvent");
    set => Set(
      "lastCaseEvent", TrimEnd(Substring(value, 1, LastCaseEvent_MaxLength)));
  }

  /// <summary>Length of the NO_JURISDICTION_CD attribute.</summary>
  public const int NoJurisdictionCd_MaxLength = 2;

  /// <summary>
  /// The value of the NO_JURISDICTION_CD attribute.
  /// A code that indicates why CSE has no jurisdiction for the case.  Values 
  /// wil include: International, Tribal, or Other
  /// </summary>
  [JsonPropertyName("noJurisdictionCd")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = NoJurisdictionCd_MaxLength, Optional = true)]
  public string NoJurisdictionCd
  {
    get => Get<string>("noJurisdictionCd");
    set => Set(
      "noJurisdictionCd",
      TrimEnd(Substring(value, 1, NoJurisdictionCd_MaxLength)));
  }

  /// <summary>Length of the ENROLLMENT_TYPE attribute.</summary>
  public const int EnrollmentType_MaxLength = 1;

  /// <summary>
  /// The value of the ENROLLMENT_TYPE attribute.
  /// The type of enrollment the original information request was created for.
  /// </summary>
  [JsonPropertyName("enrollmentType")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = EnrollmentType_MaxLength, Optional = true)]
  public string EnrollmentType
  {
    get => Get<string>("enrollmentType");
    set => Set(
      "enrollmentType", TrimEnd(Substring(value, 1, EnrollmentType_MaxLength)));
      
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offGeneratedId")]
  [Member(Index = 46, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffGeneratedId
  {
    get => Get<int?>("offGeneratedId");
    set => Set("offGeneratedId", value);
  }
}
