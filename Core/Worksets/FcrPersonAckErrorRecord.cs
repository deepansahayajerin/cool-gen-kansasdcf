// The source file: FCR_PERSON_ACK_ERROR_RECORD, ID: 373550739, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This record is returned to the state or territory at least once for each 
/// Input Person/Locate Request received by the FCR.
/// This record provides the submitter with the information necessary to 
/// synchronize the FCR data with the information on the State's or territory's
/// system.
/// </summary>
[Serializable]
public partial class FcrPersonAckErrorRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrPersonAckErrorRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrPersonAckErrorRecord(FcrPersonAckErrorRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrPersonAckErrorRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrPersonAckErrorRecord that)
  {
    base.Assign(that);
    recordIdentifier = that.recordIdentifier;
    actionTypeCode = that.actionTypeCode;
    caseId = that.caseId;
    userField = that.userField;
    fipsCountyCode = that.fipsCountyCode;
    locateRequestType = that.locateRequestType;
    bundleFplsLocateResults = that.bundleFplsLocateResults;
    participantType = that.participantType;
    familyViolence = that.familyViolence;
    memberId = that.memberId;
    sexCode = that.sexCode;
    dateOfBirth = that.dateOfBirth;
    ssn = that.ssn;
    previousSsn = that.previousSsn;
    firstName = that.firstName;
    middleName = that.middleName;
    lastName = that.lastName;
    cityOfBirth = that.cityOfBirth;
    stateOrCountryOfBirth = that.stateOrCountryOfBirth;
    fathersFirstName = that.fathersFirstName;
    fathersMiddleInitial = that.fathersMiddleInitial;
    fathersLastName = that.fathersLastName;
    mothersFirstName = that.mothersFirstName;
    mothersMiddleInitial = that.mothersMiddleInitial;
    mothersMaidenName = that.mothersMaidenName;
    irsUSsn = that.irsUSsn;
    additionalSsn1 = that.additionalSsn1;
    additionalSsn2 = that.additionalSsn2;
    additionalFirstName1 = that.additionalFirstName1;
    additionalMiddleName1 = that.additionalMiddleName1;
    additionalLastName1 = that.additionalLastName1;
    additionalFirstName2 = that.additionalFirstName2;
    additionalMiddleName2 = that.additionalMiddleName2;
    additionalLastName2 = that.additionalLastName2;
    additionalFirstName3 = that.additionalFirstName3;
    additionalMiddleName3 = that.additionalMiddleName3;
    additionalLastName3 = that.additionalLastName3;
    additionalFirstName4 = that.additionalFirstName4;
    additionalMiddleName4 = that.additionalMiddleName4;
    additionalLastName4 = that.additionalLastName4;
    newMemberId = that.newMemberId;
    irs1099 = that.irs1099;
    locateSource1 = that.locateSource1;
    locateSource2 = that.locateSource2;
    locateSource3 = that.locateSource3;
    locateSource4 = that.locateSource4;
    locateSource5 = that.locateSource5;
    locateSource6 = that.locateSource6;
    locateSource7 = that.locateSource7;
    locateSource8 = that.locateSource8;
    ssnValidityCode = that.ssnValidityCode;
    providedOrCorrectedSsn = that.providedOrCorrectedSsn;
    multipleSsn1 = that.multipleSsn1;
    multipleSsn2 = that.multipleSsn2;
    multipleSsn3 = that.multipleSsn3;
    ssaDateOfBirthIndicator = that.ssaDateOfBirthIndicator;
    batchNumber = that.batchNumber;
    dateOfDeath = that.dateOfDeath;
    ssaZipCodeOfLastResidence = that.ssaZipCodeOfLastResidence;
    ssaZipCodeOfLumpSumPayment = that.ssaZipCodeOfLumpSumPayment;
    fcrPrimarySsn = that.fcrPrimarySsn;
    fcrPrimaryFirstName = that.fcrPrimaryFirstName;
    fcrPrimaryMiddleName = that.fcrPrimaryMiddleName;
    fcrPrimaryLastName = that.fcrPrimaryLastName;
    acknowledgementCode = that.acknowledgementCode;
    errorCode1 = that.errorCode1;
    errorMessage1 = that.errorMessage1;
    errorCode2 = that.errorCode2;
    errorMessage2 = that.errorMessage2;
    errorCode3 = that.errorCode3;
    errorMessage3 = that.errorMessage3;
    errorCode4 = that.errorCode4;
    errorMessage4 = that.errorMessage4;
    errorCode5 = that.errorCode5;
    errorMessage5 = that.errorMessage5;
    additionalSsn1ValidityCode = that.additionalSsn1ValidityCode;
    additionalSsn2ValidityCode = that.additionalSsn2ValidityCode;
  }

  /// <summary>Length of the RECORD_IDENTIFIER attribute.</summary>
  public const int RecordIdentifier_MaxLength = 2;

  /// <summary>
  /// The value of the RECORD_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordIdentifier_MaxLength)
    ]
  public string RecordIdentifier
  {
    get => recordIdentifier ?? "";
    set => recordIdentifier =
      TrimEnd(Substring(value, 1, RecordIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordIdentifier attribute.</summary>
  [JsonPropertyName("recordIdentifier")]
  [Computed]
  public string RecordIdentifier_Json
  {
    get => NullIf(RecordIdentifier, "");
    set => RecordIdentifier = value;
  }

  /// <summary>Length of the ACTION_TYPE_CODE attribute.</summary>
  public const int ActionTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION_TYPE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ActionTypeCode_MaxLength)]
  public string ActionTypeCode
  {
    get => actionTypeCode ?? "";
    set => actionTypeCode =
      TrimEnd(Substring(value, 1, ActionTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ActionTypeCode attribute.</summary>
  [JsonPropertyName("actionTypeCode")]
  [Computed]
  public string ActionTypeCode_Json
  {
    get => NullIf(ActionTypeCode, "");
    set => ActionTypeCode = value;
  }

  /// <summary>Length of the CASE_ID attribute.</summary>
  public const int CaseId_MaxLength = 15;

  /// <summary>
  /// The value of the CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CaseId_MaxLength)]
  public string CaseId
  {
    get => caseId ?? "";
    set => caseId = TrimEnd(Substring(value, 1, CaseId_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseId attribute.</summary>
  [JsonPropertyName("caseId")]
  [Computed]
  public string CaseId_Json
  {
    get => NullIf(CaseId, "");
    set => CaseId = value;
  }

  /// <summary>Length of the USER_FIELD attribute.</summary>
  public const int UserField_MaxLength = 15;

  /// <summary>
  /// The value of the USER_FIELD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = UserField_MaxLength)]
  public string UserField
  {
    get => userField ?? "";
    set => userField = TrimEnd(Substring(value, 1, UserField_MaxLength));
  }

  /// <summary>
  /// The json value of the UserField attribute.</summary>
  [JsonPropertyName("userField")]
  [Computed]
  public string UserField_Json
  {
    get => NullIf(UserField, "");
    set => UserField = value;
  }

  /// <summary>Length of the FIPS_COUNTY_CODE attribute.</summary>
  public const int FipsCountyCode_MaxLength = 3;

  /// <summary>
  /// The value of the FIPS_COUNTY_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = FipsCountyCode_MaxLength)]
  public string FipsCountyCode
  {
    get => fipsCountyCode ?? "";
    set => fipsCountyCode =
      TrimEnd(Substring(value, 1, FipsCountyCode_MaxLength));
  }

  /// <summary>
  /// The json value of the FipsCountyCode attribute.</summary>
  [JsonPropertyName("fipsCountyCode")]
  [Computed]
  public string FipsCountyCode_Json
  {
    get => NullIf(FipsCountyCode, "");
    set => FipsCountyCode = value;
  }

  /// <summary>Length of the LOCATE_REQUEST_TYPE attribute.</summary>
  public const int LocateRequestType_MaxLength = 2;

  /// <summary>
  /// The value of the LOCATE_REQUEST_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = LocateRequestType_MaxLength)]
  public string LocateRequestType
  {
    get => locateRequestType ?? "";
    set => locateRequestType =
      TrimEnd(Substring(value, 1, LocateRequestType_MaxLength));
  }

  /// <summary>
  /// The json value of the LocateRequestType attribute.</summary>
  [JsonPropertyName("locateRequestType")]
  [Computed]
  public string LocateRequestType_Json
  {
    get => NullIf(LocateRequestType, "");
    set => LocateRequestType = value;
  }

  /// <summary>Length of the BUNDLE_FPLS_LOCATE_RESULTS attribute.</summary>
  public const int BundleFplsLocateResults_MaxLength = 1;

  /// <summary>
  /// The value of the BUNDLE_FPLS_LOCATE_RESULTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = BundleFplsLocateResults_MaxLength)]
  public string BundleFplsLocateResults
  {
    get => bundleFplsLocateResults ?? "";
    set => bundleFplsLocateResults =
      TrimEnd(Substring(value, 1, BundleFplsLocateResults_MaxLength));
  }

  /// <summary>
  /// The json value of the BundleFplsLocateResults attribute.</summary>
  [JsonPropertyName("bundleFplsLocateResults")]
  [Computed]
  public string BundleFplsLocateResults_Json
  {
    get => NullIf(BundleFplsLocateResults, "");
    set => BundleFplsLocateResults = value;
  }

  /// <summary>Length of the PARTICIPANT_TYPE attribute.</summary>
  public const int ParticipantType_MaxLength = 2;

  /// <summary>
  /// The value of the PARTICIPANT_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ParticipantType_MaxLength)]
    
  public string ParticipantType
  {
    get => participantType ?? "";
    set => participantType =
      TrimEnd(Substring(value, 1, ParticipantType_MaxLength));
  }

  /// <summary>
  /// The json value of the ParticipantType attribute.</summary>
  [JsonPropertyName("participantType")]
  [Computed]
  public string ParticipantType_Json
  {
    get => NullIf(ParticipantType, "");
    set => ParticipantType = value;
  }

  /// <summary>Length of the FAMILY_VIOLENCE attribute.</summary>
  public const int FamilyViolence_MaxLength = 2;

  /// <summary>
  /// The value of the FAMILY_VIOLENCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = FamilyViolence_MaxLength)]
  public string FamilyViolence
  {
    get => familyViolence ?? "";
    set => familyViolence =
      TrimEnd(Substring(value, 1, FamilyViolence_MaxLength));
  }

  /// <summary>
  /// The json value of the FamilyViolence attribute.</summary>
  [JsonPropertyName("familyViolence")]
  [Computed]
  public string FamilyViolence_Json
  {
    get => NullIf(FamilyViolence, "");
    set => FamilyViolence = value;
  }

  /// <summary>Length of the MEMBER_ID attribute.</summary>
  public const int MemberId_MaxLength = 15;

  /// <summary>
  /// The value of the MEMBER_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = MemberId_MaxLength)]
  public string MemberId
  {
    get => memberId ?? "";
    set => memberId = TrimEnd(Substring(value, 1, MemberId_MaxLength));
  }

  /// <summary>
  /// The json value of the MemberId attribute.</summary>
  [JsonPropertyName("memberId")]
  [Computed]
  public string MemberId_Json
  {
    get => NullIf(MemberId, "");
    set => MemberId = value;
  }

  /// <summary>Length of the SEX_CODE attribute.</summary>
  public const int SexCode_MaxLength = 1;

  /// <summary>
  /// The value of the SEX_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = SexCode_MaxLength)]
  public string SexCode
  {
    get => sexCode ?? "";
    set => sexCode = TrimEnd(Substring(value, 1, SexCode_MaxLength));
  }

  /// <summary>
  /// The json value of the SexCode attribute.</summary>
  [JsonPropertyName("sexCode")]
  [Computed]
  public string SexCode_Json
  {
    get => NullIf(SexCode, "");
    set => SexCode = value;
  }

  /// <summary>
  /// The value of the DATE_OF_BIRTH attribute.
  /// </summary>
  [JsonPropertyName("dateOfBirth")]
  [Member(Index = 12, Type = MemberType.Date)]
  public DateTime? DateOfBirth
  {
    get => dateOfBirth;
    set => dateOfBirth = value;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = Ssn_MaxLength)]
  public string Ssn
  {
    get => ssn ?? "";
    set => ssn = TrimEnd(Substring(value, 1, Ssn_MaxLength));
  }

  /// <summary>
  /// The json value of the Ssn attribute.</summary>
  [JsonPropertyName("ssn")]
  [Computed]
  public string Ssn_Json
  {
    get => NullIf(Ssn, "");
    set => Ssn = value;
  }

  /// <summary>Length of the PREVIOUS_SSN attribute.</summary>
  public const int PreviousSsn_MaxLength = 9;

  /// <summary>
  /// The value of the PREVIOUS_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = PreviousSsn_MaxLength)]
  public string PreviousSsn
  {
    get => previousSsn ?? "";
    set => previousSsn = TrimEnd(Substring(value, 1, PreviousSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the PreviousSsn attribute.</summary>
  [JsonPropertyName("previousSsn")]
  [Computed]
  public string PreviousSsn_Json
  {
    get => NullIf(PreviousSsn, "");
    set => PreviousSsn = value;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 16;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = FirstName_MaxLength)]
  public string FirstName
  {
    get => firstName ?? "";
    set => firstName = TrimEnd(Substring(value, 1, FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the FirstName attribute.</summary>
  [JsonPropertyName("firstName")]
  [Computed]
  public string FirstName_Json
  {
    get => NullIf(FirstName, "");
    set => FirstName = value;
  }

  /// <summary>Length of the MIDDLE_NAME attribute.</summary>
  public const int MiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = MiddleName_MaxLength)]
  public string MiddleName
  {
    get => middleName ?? "";
    set => middleName = TrimEnd(Substring(value, 1, MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the MiddleName attribute.</summary>
  [JsonPropertyName("middleName")]
  [Computed]
  public string MiddleName_Json
  {
    get => NullIf(MiddleName, "");
    set => MiddleName = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 30;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = LastName_MaxLength)]
  public string LastName
  {
    get => lastName ?? "";
    set => lastName = TrimEnd(Substring(value, 1, LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the LastName attribute.</summary>
  [JsonPropertyName("lastName")]
  [Computed]
  public string LastName_Json
  {
    get => NullIf(LastName, "");
    set => LastName = value;
  }

  /// <summary>Length of the CITY_OF_BIRTH attribute.</summary>
  public const int CityOfBirth_MaxLength = 16;

  /// <summary>
  /// The value of the CITY_OF_BIRTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = CityOfBirth_MaxLength)]
  public string CityOfBirth
  {
    get => cityOfBirth ?? "";
    set => cityOfBirth = TrimEnd(Substring(value, 1, CityOfBirth_MaxLength));
  }

  /// <summary>
  /// The json value of the CityOfBirth attribute.</summary>
  [JsonPropertyName("cityOfBirth")]
  [Computed]
  public string CityOfBirth_Json
  {
    get => NullIf(CityOfBirth, "");
    set => CityOfBirth = value;
  }

  /// <summary>Length of the STATE_OR_COUNTRY_OF_BIRTH attribute.</summary>
  public const int StateOrCountryOfBirth_MaxLength = 4;

  /// <summary>
  /// The value of the STATE_OR_COUNTRY_OF_BIRTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = StateOrCountryOfBirth_MaxLength)]
  public string StateOrCountryOfBirth
  {
    get => stateOrCountryOfBirth ?? "";
    set => stateOrCountryOfBirth =
      TrimEnd(Substring(value, 1, StateOrCountryOfBirth_MaxLength));
  }

  /// <summary>
  /// The json value of the StateOrCountryOfBirth attribute.</summary>
  [JsonPropertyName("stateOrCountryOfBirth")]
  [Computed]
  public string StateOrCountryOfBirth_Json
  {
    get => NullIf(StateOrCountryOfBirth, "");
    set => StateOrCountryOfBirth = value;
  }

  /// <summary>Length of the FATHERS_FIRST_NAME attribute.</summary>
  public const int FathersFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the FATHERS_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = FathersFirstName_MaxLength)]
  public string FathersFirstName
  {
    get => fathersFirstName ?? "";
    set => fathersFirstName =
      TrimEnd(Substring(value, 1, FathersFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the FathersFirstName attribute.</summary>
  [JsonPropertyName("fathersFirstName")]
  [Computed]
  public string FathersFirstName_Json
  {
    get => NullIf(FathersFirstName, "");
    set => FathersFirstName = value;
  }

  /// <summary>Length of the FATHERS_MIDDLE_INITIAL attribute.</summary>
  public const int FathersMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the FATHERS_MIDDLE_INITIAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = FathersMiddleInitial_MaxLength)]
  public string FathersMiddleInitial
  {
    get => fathersMiddleInitial ?? "";
    set => fathersMiddleInitial =
      TrimEnd(Substring(value, 1, FathersMiddleInitial_MaxLength));
  }

  /// <summary>
  /// The json value of the FathersMiddleInitial attribute.</summary>
  [JsonPropertyName("fathersMiddleInitial")]
  [Computed]
  public string FathersMiddleInitial_Json
  {
    get => NullIf(FathersMiddleInitial, "");
    set => FathersMiddleInitial = value;
  }

  /// <summary>Length of the FATHERS_LAST_NAME attribute.</summary>
  public const int FathersLastName_MaxLength = 16;

  /// <summary>
  /// The value of the FATHERS_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length = FathersLastName_MaxLength)
    ]
  public string FathersLastName
  {
    get => fathersLastName ?? "";
    set => fathersLastName =
      TrimEnd(Substring(value, 1, FathersLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the FathersLastName attribute.</summary>
  [JsonPropertyName("fathersLastName")]
  [Computed]
  public string FathersLastName_Json
  {
    get => NullIf(FathersLastName, "");
    set => FathersLastName = value;
  }

  /// <summary>Length of the MOTHERS_FIRST_NAME attribute.</summary>
  public const int MothersFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the MOTHERS_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = MothersFirstName_MaxLength)]
  public string MothersFirstName
  {
    get => mothersFirstName ?? "";
    set => mothersFirstName =
      TrimEnd(Substring(value, 1, MothersFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the MothersFirstName attribute.</summary>
  [JsonPropertyName("mothersFirstName")]
  [Computed]
  public string MothersFirstName_Json
  {
    get => NullIf(MothersFirstName, "");
    set => MothersFirstName = value;
  }

  /// <summary>Length of the MOTHERS_MIDDLE_INITIAL attribute.</summary>
  public const int MothersMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MOTHERS_MIDDLE_INITIAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = MothersMiddleInitial_MaxLength)]
  public string MothersMiddleInitial
  {
    get => mothersMiddleInitial ?? "";
    set => mothersMiddleInitial =
      TrimEnd(Substring(value, 1, MothersMiddleInitial_MaxLength));
  }

  /// <summary>
  /// The json value of the MothersMiddleInitial attribute.</summary>
  [JsonPropertyName("mothersMiddleInitial")]
  [Computed]
  public string MothersMiddleInitial_Json
  {
    get => NullIf(MothersMiddleInitial, "");
    set => MothersMiddleInitial = value;
  }

  /// <summary>Length of the MOTHERS_MAIDEN_NAME attribute.</summary>
  public const int MothersMaidenName_MaxLength = 16;

  /// <summary>
  /// The value of the MOTHERS_MAIDEN_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = MothersMaidenName_MaxLength)]
  public string MothersMaidenName
  {
    get => mothersMaidenName ?? "";
    set => mothersMaidenName =
      TrimEnd(Substring(value, 1, MothersMaidenName_MaxLength));
  }

  /// <summary>
  /// The json value of the MothersMaidenName attribute.</summary>
  [JsonPropertyName("mothersMaidenName")]
  [Computed]
  public string MothersMaidenName_Json
  {
    get => NullIf(MothersMaidenName, "");
    set => MothersMaidenName = value;
  }

  /// <summary>Length of the IRS_U_SSN attribute.</summary>
  public const int IrsUSsn_MaxLength = 9;

  /// <summary>
  /// The value of the IRS_U_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length = IrsUSsn_MaxLength)]
  public string IrsUSsn
  {
    get => irsUSsn ?? "";
    set => irsUSsn = TrimEnd(Substring(value, 1, IrsUSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the IrsUSsn attribute.</summary>
  [JsonPropertyName("irsUSsn")]
  [Computed]
  public string IrsUSsn_Json
  {
    get => NullIf(IrsUSsn, "");
    set => IrsUSsn = value;
  }

  /// <summary>Length of the ADDITIONAL_SSN_1 attribute.</summary>
  public const int AdditionalSsn1_MaxLength = 9;

  /// <summary>
  /// The value of the ADDITIONAL_SSN_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length = AdditionalSsn1_MaxLength)]
    
  public string AdditionalSsn1
  {
    get => additionalSsn1 ?? "";
    set => additionalSsn1 =
      TrimEnd(Substring(value, 1, AdditionalSsn1_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalSsn1 attribute.</summary>
  [JsonPropertyName("additionalSsn1")]
  [Computed]
  public string AdditionalSsn1_Json
  {
    get => NullIf(AdditionalSsn1, "");
    set => AdditionalSsn1 = value;
  }

  /// <summary>Length of the ADDITIONAL_SSN_2 attribute.</summary>
  public const int AdditionalSsn2_MaxLength = 9;

  /// <summary>
  /// The value of the ADDITIONAL_SSN_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length = AdditionalSsn2_MaxLength)]
    
  public string AdditionalSsn2
  {
    get => additionalSsn2 ?? "";
    set => additionalSsn2 =
      TrimEnd(Substring(value, 1, AdditionalSsn2_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalSsn2 attribute.</summary>
  [JsonPropertyName("additionalSsn2")]
  [Computed]
  public string AdditionalSsn2_Json
  {
    get => NullIf(AdditionalSsn2, "");
    set => AdditionalSsn2 = value;
  }

  /// <summary>Length of the ADDITIONAL_FIRST_NAME_1 attribute.</summary>
  public const int AdditionalFirstName1_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_FIRST_NAME_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = AdditionalFirstName1_MaxLength)]
  public string AdditionalFirstName1
  {
    get => additionalFirstName1 ?? "";
    set => additionalFirstName1 =
      TrimEnd(Substring(value, 1, AdditionalFirstName1_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalFirstName1 attribute.</summary>
  [JsonPropertyName("additionalFirstName1")]
  [Computed]
  public string AdditionalFirstName1_Json
  {
    get => NullIf(AdditionalFirstName1, "");
    set => AdditionalFirstName1 = value;
  }

  /// <summary>Length of the ADDITIONAL_MIDDLE_NAME_1 attribute.</summary>
  public const int AdditionalMiddleName1_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_MIDDLE_NAME_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = AdditionalMiddleName1_MaxLength)]
  public string AdditionalMiddleName1
  {
    get => additionalMiddleName1 ?? "";
    set => additionalMiddleName1 =
      TrimEnd(Substring(value, 1, AdditionalMiddleName1_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalMiddleName1 attribute.</summary>
  [JsonPropertyName("additionalMiddleName1")]
  [Computed]
  public string AdditionalMiddleName1_Json
  {
    get => NullIf(AdditionalMiddleName1, "");
    set => AdditionalMiddleName1 = value;
  }

  /// <summary>Length of the ADDITIONAL_LAST_NAME_1 attribute.</summary>
  public const int AdditionalLastName1_MaxLength = 30;

  /// <summary>
  /// The value of the ADDITIONAL_LAST_NAME_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = AdditionalLastName1_MaxLength)]
  public string AdditionalLastName1
  {
    get => additionalLastName1 ?? "";
    set => additionalLastName1 =
      TrimEnd(Substring(value, 1, AdditionalLastName1_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalLastName1 attribute.</summary>
  [JsonPropertyName("additionalLastName1")]
  [Computed]
  public string AdditionalLastName1_Json
  {
    get => NullIf(AdditionalLastName1, "");
    set => AdditionalLastName1 = value;
  }

  /// <summary>Length of the ADDITIONAL_FIRST_NAME_2 attribute.</summary>
  public const int AdditionalFirstName2_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_FIRST_NAME_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = AdditionalFirstName2_MaxLength)]
  public string AdditionalFirstName2
  {
    get => additionalFirstName2 ?? "";
    set => additionalFirstName2 =
      TrimEnd(Substring(value, 1, AdditionalFirstName2_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalFirstName2 attribute.</summary>
  [JsonPropertyName("additionalFirstName2")]
  [Computed]
  public string AdditionalFirstName2_Json
  {
    get => NullIf(AdditionalFirstName2, "");
    set => AdditionalFirstName2 = value;
  }

  /// <summary>Length of the ADDITIONAL_MIDDLE_NAME_2 attribute.</summary>
  public const int AdditionalMiddleName2_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_MIDDLE_NAME_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = AdditionalMiddleName2_MaxLength)]
  public string AdditionalMiddleName2
  {
    get => additionalMiddleName2 ?? "";
    set => additionalMiddleName2 =
      TrimEnd(Substring(value, 1, AdditionalMiddleName2_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalMiddleName2 attribute.</summary>
  [JsonPropertyName("additionalMiddleName2")]
  [Computed]
  public string AdditionalMiddleName2_Json
  {
    get => NullIf(AdditionalMiddleName2, "");
    set => AdditionalMiddleName2 = value;
  }

  /// <summary>Length of the ADDITIONAL_LAST_NAME_2 attribute.</summary>
  public const int AdditionalLastName2_MaxLength = 30;

  /// <summary>
  /// The value of the ADDITIONAL_LAST_NAME_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = AdditionalLastName2_MaxLength)]
  public string AdditionalLastName2
  {
    get => additionalLastName2 ?? "";
    set => additionalLastName2 =
      TrimEnd(Substring(value, 1, AdditionalLastName2_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalLastName2 attribute.</summary>
  [JsonPropertyName("additionalLastName2")]
  [Computed]
  public string AdditionalLastName2_Json
  {
    get => NullIf(AdditionalLastName2, "");
    set => AdditionalLastName2 = value;
  }

  /// <summary>Length of the ADDITIONAL_FIRST_NAME_3 attribute.</summary>
  public const int AdditionalFirstName3_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_FIRST_NAME_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = AdditionalFirstName3_MaxLength)]
  public string AdditionalFirstName3
  {
    get => additionalFirstName3 ?? "";
    set => additionalFirstName3 =
      TrimEnd(Substring(value, 1, AdditionalFirstName3_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalFirstName3 attribute.</summary>
  [JsonPropertyName("additionalFirstName3")]
  [Computed]
  public string AdditionalFirstName3_Json
  {
    get => NullIf(AdditionalFirstName3, "");
    set => AdditionalFirstName3 = value;
  }

  /// <summary>Length of the ADDITIONAL_MIDDLE_NAME_3 attribute.</summary>
  public const int AdditionalMiddleName3_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_MIDDLE_NAME_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = AdditionalMiddleName3_MaxLength)]
  public string AdditionalMiddleName3
  {
    get => additionalMiddleName3 ?? "";
    set => additionalMiddleName3 =
      TrimEnd(Substring(value, 1, AdditionalMiddleName3_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalMiddleName3 attribute.</summary>
  [JsonPropertyName("additionalMiddleName3")]
  [Computed]
  public string AdditionalMiddleName3_Json
  {
    get => NullIf(AdditionalMiddleName3, "");
    set => AdditionalMiddleName3 = value;
  }

  /// <summary>Length of the ADDITIONAL_LAST_NAME_3 attribute.</summary>
  public const int AdditionalLastName3_MaxLength = 30;

  /// <summary>
  /// The value of the ADDITIONAL_LAST_NAME_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = AdditionalLastName3_MaxLength)]
  public string AdditionalLastName3
  {
    get => additionalLastName3 ?? "";
    set => additionalLastName3 =
      TrimEnd(Substring(value, 1, AdditionalLastName3_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalLastName3 attribute.</summary>
  [JsonPropertyName("additionalLastName3")]
  [Computed]
  public string AdditionalLastName3_Json
  {
    get => NullIf(AdditionalLastName3, "");
    set => AdditionalLastName3 = value;
  }

  /// <summary>Length of the ADDITIONAL_FIRST_NAME_4 attribute.</summary>
  public const int AdditionalFirstName4_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_FIRST_NAME_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = AdditionalFirstName4_MaxLength)]
  public string AdditionalFirstName4
  {
    get => additionalFirstName4 ?? "";
    set => additionalFirstName4 =
      TrimEnd(Substring(value, 1, AdditionalFirstName4_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalFirstName4 attribute.</summary>
  [JsonPropertyName("additionalFirstName4")]
  [Computed]
  public string AdditionalFirstName4_Json
  {
    get => NullIf(AdditionalFirstName4, "");
    set => AdditionalFirstName4 = value;
  }

  /// <summary>Length of the ADDITIONAL_MIDDLE_NAME_4 attribute.</summary>
  public const int AdditionalMiddleName4_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_MIDDLE_NAME_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = AdditionalMiddleName4_MaxLength)]
  public string AdditionalMiddleName4
  {
    get => additionalMiddleName4 ?? "";
    set => additionalMiddleName4 =
      TrimEnd(Substring(value, 1, AdditionalMiddleName4_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalMiddleName4 attribute.</summary>
  [JsonPropertyName("additionalMiddleName4")]
  [Computed]
  public string AdditionalMiddleName4_Json
  {
    get => NullIf(AdditionalMiddleName4, "");
    set => AdditionalMiddleName4 = value;
  }

  /// <summary>Length of the ADDITIONAL_LAST_NAME_4 attribute.</summary>
  public const int AdditionalLastName4_MaxLength = 30;

  /// <summary>
  /// The value of the ADDITIONAL_LAST_NAME_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = AdditionalLastName4_MaxLength)]
  public string AdditionalLastName4
  {
    get => additionalLastName4 ?? "";
    set => additionalLastName4 =
      TrimEnd(Substring(value, 1, AdditionalLastName4_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalLastName4 attribute.</summary>
  [JsonPropertyName("additionalLastName4")]
  [Computed]
  public string AdditionalLastName4_Json
  {
    get => NullIf(AdditionalLastName4, "");
    set => AdditionalLastName4 = value;
  }

  /// <summary>Length of the NEW_MEMBER_ID attribute.</summary>
  public const int NewMemberId_MaxLength = 15;

  /// <summary>
  /// The value of the NEW_MEMBER_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length = NewMemberId_MaxLength)]
  public string NewMemberId
  {
    get => newMemberId ?? "";
    set => newMemberId = TrimEnd(Substring(value, 1, NewMemberId_MaxLength));
  }

  /// <summary>
  /// The json value of the NewMemberId attribute.</summary>
  [JsonPropertyName("newMemberId")]
  [Computed]
  public string NewMemberId_Json
  {
    get => NullIf(NewMemberId, "");
    set => NewMemberId = value;
  }

  /// <summary>Length of the IRS_1099 attribute.</summary>
  public const int Irs1099_MaxLength = 1;

  /// <summary>
  /// The value of the IRS_1099 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length = Irs1099_MaxLength)]
  public string Irs1099
  {
    get => irs1099 ?? "";
    set => irs1099 = TrimEnd(Substring(value, 1, Irs1099_MaxLength));
  }

  /// <summary>
  /// The json value of the Irs1099 attribute.</summary>
  [JsonPropertyName("irs1099")]
  [Computed]
  public string Irs1099_Json
  {
    get => NullIf(Irs1099, "");
    set => Irs1099 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_1 attribute.</summary>
  public const int LocateSource1_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length = LocateSource1_MaxLength)]
  public string LocateSource1
  {
    get => locateSource1 ?? "";
    set => locateSource1 =
      TrimEnd(Substring(value, 1, LocateSource1_MaxLength));
  }

  /// <summary>
  /// The json value of the LocateSource1 attribute.</summary>
  [JsonPropertyName("locateSource1")]
  [Computed]
  public string LocateSource1_Json
  {
    get => NullIf(LocateSource1, "");
    set => LocateSource1 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_2 attribute.</summary>
  public const int LocateSource2_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 44, Type = MemberType.Char, Length = LocateSource2_MaxLength)]
  public string LocateSource2
  {
    get => locateSource2 ?? "";
    set => locateSource2 =
      TrimEnd(Substring(value, 1, LocateSource2_MaxLength));
  }

  /// <summary>
  /// The json value of the LocateSource2 attribute.</summary>
  [JsonPropertyName("locateSource2")]
  [Computed]
  public string LocateSource2_Json
  {
    get => NullIf(LocateSource2, "");
    set => LocateSource2 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_3 attribute.</summary>
  public const int LocateSource3_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length = LocateSource3_MaxLength)]
  public string LocateSource3
  {
    get => locateSource3 ?? "";
    set => locateSource3 =
      TrimEnd(Substring(value, 1, LocateSource3_MaxLength));
  }

  /// <summary>
  /// The json value of the LocateSource3 attribute.</summary>
  [JsonPropertyName("locateSource3")]
  [Computed]
  public string LocateSource3_Json
  {
    get => NullIf(LocateSource3, "");
    set => LocateSource3 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_4 attribute.</summary>
  public const int LocateSource4_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length = LocateSource4_MaxLength)]
  public string LocateSource4
  {
    get => locateSource4 ?? "";
    set => locateSource4 =
      TrimEnd(Substring(value, 1, LocateSource4_MaxLength));
  }

  /// <summary>
  /// The json value of the LocateSource4 attribute.</summary>
  [JsonPropertyName("locateSource4")]
  [Computed]
  public string LocateSource4_Json
  {
    get => NullIf(LocateSource4, "");
    set => LocateSource4 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_5 attribute.</summary>
  public const int LocateSource5_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_5 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length = LocateSource5_MaxLength)]
  public string LocateSource5
  {
    get => locateSource5 ?? "";
    set => locateSource5 =
      TrimEnd(Substring(value, 1, LocateSource5_MaxLength));
  }

  /// <summary>
  /// The json value of the LocateSource5 attribute.</summary>
  [JsonPropertyName("locateSource5")]
  [Computed]
  public string LocateSource5_Json
  {
    get => NullIf(LocateSource5, "");
    set => LocateSource5 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_6 attribute.</summary>
  public const int LocateSource6_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_6 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 48, Type = MemberType.Char, Length = LocateSource6_MaxLength)]
  public string LocateSource6
  {
    get => locateSource6 ?? "";
    set => locateSource6 =
      TrimEnd(Substring(value, 1, LocateSource6_MaxLength));
  }

  /// <summary>
  /// The json value of the LocateSource6 attribute.</summary>
  [JsonPropertyName("locateSource6")]
  [Computed]
  public string LocateSource6_Json
  {
    get => NullIf(LocateSource6, "");
    set => LocateSource6 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_7 attribute.</summary>
  public const int LocateSource7_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_7 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 49, Type = MemberType.Char, Length = LocateSource7_MaxLength)]
  public string LocateSource7
  {
    get => locateSource7 ?? "";
    set => locateSource7 =
      TrimEnd(Substring(value, 1, LocateSource7_MaxLength));
  }

  /// <summary>
  /// The json value of the LocateSource7 attribute.</summary>
  [JsonPropertyName("locateSource7")]
  [Computed]
  public string LocateSource7_Json
  {
    get => NullIf(LocateSource7, "");
    set => LocateSource7 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_8 attribute.</summary>
  public const int LocateSource8_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_8 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 50, Type = MemberType.Char, Length = LocateSource8_MaxLength)]
  public string LocateSource8
  {
    get => locateSource8 ?? "";
    set => locateSource8 =
      TrimEnd(Substring(value, 1, LocateSource8_MaxLength));
  }

  /// <summary>
  /// The json value of the LocateSource8 attribute.</summary>
  [JsonPropertyName("locateSource8")]
  [Computed]
  public string LocateSource8_Json
  {
    get => NullIf(LocateSource8, "");
    set => LocateSource8 = value;
  }

  /// <summary>Length of the SSN_VALIDITY_CODE attribute.</summary>
  public const int SsnValidityCode_MaxLength = 1;

  /// <summary>
  /// The value of the SSN_VALIDITY_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 51, Type = MemberType.Char, Length = SsnValidityCode_MaxLength)
    ]
  public string SsnValidityCode
  {
    get => ssnValidityCode ?? "";
    set => ssnValidityCode =
      TrimEnd(Substring(value, 1, SsnValidityCode_MaxLength));
  }

  /// <summary>
  /// The json value of the SsnValidityCode attribute.</summary>
  [JsonPropertyName("ssnValidityCode")]
  [Computed]
  public string SsnValidityCode_Json
  {
    get => NullIf(SsnValidityCode, "");
    set => SsnValidityCode = value;
  }

  /// <summary>Length of the PROVIDED_OR_CORRECTED_SSN attribute.</summary>
  public const int ProvidedOrCorrectedSsn_MaxLength = 9;

  /// <summary>
  /// The value of the PROVIDED_OR_CORRECTED_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 52, Type = MemberType.Char, Length
    = ProvidedOrCorrectedSsn_MaxLength)]
  public string ProvidedOrCorrectedSsn
  {
    get => providedOrCorrectedSsn ?? "";
    set => providedOrCorrectedSsn =
      TrimEnd(Substring(value, 1, ProvidedOrCorrectedSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the ProvidedOrCorrectedSsn attribute.</summary>
  [JsonPropertyName("providedOrCorrectedSsn")]
  [Computed]
  public string ProvidedOrCorrectedSsn_Json
  {
    get => NullIf(ProvidedOrCorrectedSsn, "");
    set => ProvidedOrCorrectedSsn = value;
  }

  /// <summary>Length of the MULTIPLE_SSN_1 attribute.</summary>
  public const int MultipleSsn1_MaxLength = 9;

  /// <summary>
  /// The value of the MULTIPLE_SSN_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 53, Type = MemberType.Char, Length = MultipleSsn1_MaxLength)]
  public string MultipleSsn1
  {
    get => multipleSsn1 ?? "";
    set => multipleSsn1 = TrimEnd(Substring(value, 1, MultipleSsn1_MaxLength));
  }

  /// <summary>
  /// The json value of the MultipleSsn1 attribute.</summary>
  [JsonPropertyName("multipleSsn1")]
  [Computed]
  public string MultipleSsn1_Json
  {
    get => NullIf(MultipleSsn1, "");
    set => MultipleSsn1 = value;
  }

  /// <summary>Length of the MULTIPLE_SSN_2 attribute.</summary>
  public const int MultipleSsn2_MaxLength = 9;

  /// <summary>
  /// The value of the MULTIPLE_SSN_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 54, Type = MemberType.Char, Length = MultipleSsn2_MaxLength)]
  public string MultipleSsn2
  {
    get => multipleSsn2 ?? "";
    set => multipleSsn2 = TrimEnd(Substring(value, 1, MultipleSsn2_MaxLength));
  }

  /// <summary>
  /// The json value of the MultipleSsn2 attribute.</summary>
  [JsonPropertyName("multipleSsn2")]
  [Computed]
  public string MultipleSsn2_Json
  {
    get => NullIf(MultipleSsn2, "");
    set => MultipleSsn2 = value;
  }

  /// <summary>Length of the MULTIPLE_SSN_3 attribute.</summary>
  public const int MultipleSsn3_MaxLength = 9;

  /// <summary>
  /// The value of the MULTIPLE_SSN_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 55, Type = MemberType.Char, Length = MultipleSsn3_MaxLength)]
  public string MultipleSsn3
  {
    get => multipleSsn3 ?? "";
    set => multipleSsn3 = TrimEnd(Substring(value, 1, MultipleSsn3_MaxLength));
  }

  /// <summary>
  /// The json value of the MultipleSsn3 attribute.</summary>
  [JsonPropertyName("multipleSsn3")]
  [Computed]
  public string MultipleSsn3_Json
  {
    get => NullIf(MultipleSsn3, "");
    set => MultipleSsn3 = value;
  }

  /// <summary>Length of the SSA_DATE_OF_BIRTH_INDICATOR attribute.</summary>
  public const int SsaDateOfBirthIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the SSA_DATE_OF_BIRTH_INDICATOR attribute.
  /// Y = DOB returned is diiferent than the one submitted on the input record
  /// N = DOB on the input record matched the DOB in SSA records
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 56, Type = MemberType.Char, Length
    = SsaDateOfBirthIndicator_MaxLength)]
  public string SsaDateOfBirthIndicator
  {
    get => ssaDateOfBirthIndicator ?? "";
    set => ssaDateOfBirthIndicator =
      TrimEnd(Substring(value, 1, SsaDateOfBirthIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the SsaDateOfBirthIndicator attribute.</summary>
  [JsonPropertyName("ssaDateOfBirthIndicator")]
  [Computed]
  public string SsaDateOfBirthIndicator_Json
  {
    get => NullIf(SsaDateOfBirthIndicator, "");
    set => SsaDateOfBirthIndicator = value;
  }

  /// <summary>Length of the BATCH_NUMBER attribute.</summary>
  public const int BatchNumber_MaxLength = 6;

  /// <summary>
  /// The value of the BATCH_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 57, Type = MemberType.Char, Length = BatchNumber_MaxLength)]
  public string BatchNumber
  {
    get => batchNumber ?? "";
    set => batchNumber = TrimEnd(Substring(value, 1, BatchNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the BatchNumber attribute.</summary>
  [JsonPropertyName("batchNumber")]
  [Computed]
  public string BatchNumber_Json
  {
    get => NullIf(BatchNumber, "");
    set => BatchNumber = value;
  }

  /// <summary>
  /// The value of the DATE_OF_DEATH attribute.
  /// </summary>
  [JsonPropertyName("dateOfDeath")]
  [Member(Index = 58, Type = MemberType.Date)]
  public DateTime? DateOfDeath
  {
    get => dateOfDeath;
    set => dateOfDeath = value;
  }

  /// <summary>Length of the SSA_ZIP_CODE_OF_LAST_RESIDENCE attribute.</summary>
  public const int SsaZipCodeOfLastResidence_MaxLength = 5;

  /// <summary>
  /// The value of the SSA_ZIP_CODE_OF_LAST_RESIDENCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 59, Type = MemberType.Char, Length
    = SsaZipCodeOfLastResidence_MaxLength)]
  public string SsaZipCodeOfLastResidence
  {
    get => ssaZipCodeOfLastResidence ?? "";
    set => ssaZipCodeOfLastResidence =
      TrimEnd(Substring(value, 1, SsaZipCodeOfLastResidence_MaxLength));
  }

  /// <summary>
  /// The json value of the SsaZipCodeOfLastResidence attribute.</summary>
  [JsonPropertyName("ssaZipCodeOfLastResidence")]
  [Computed]
  public string SsaZipCodeOfLastResidence_Json
  {
    get => NullIf(SsaZipCodeOfLastResidence, "");
    set => SsaZipCodeOfLastResidence = value;
  }

  /// <summary>Length of the SSA_ZIP_CODE_OF_LUMP_SUM_PAYMENT attribute.
  /// </summary>
  public const int SsaZipCodeOfLumpSumPayment_MaxLength = 5;

  /// <summary>
  /// The value of the SSA_ZIP_CODE_OF_LUMP_SUM_PAYMENT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 60, Type = MemberType.Char, Length
    = SsaZipCodeOfLumpSumPayment_MaxLength)]
  public string SsaZipCodeOfLumpSumPayment
  {
    get => ssaZipCodeOfLumpSumPayment ?? "";
    set => ssaZipCodeOfLumpSumPayment =
      TrimEnd(Substring(value, 1, SsaZipCodeOfLumpSumPayment_MaxLength));
  }

  /// <summary>
  /// The json value of the SsaZipCodeOfLumpSumPayment attribute.</summary>
  [JsonPropertyName("ssaZipCodeOfLumpSumPayment")]
  [Computed]
  public string SsaZipCodeOfLumpSumPayment_Json
  {
    get => NullIf(SsaZipCodeOfLumpSumPayment, "");
    set => SsaZipCodeOfLumpSumPayment = value;
  }

  /// <summary>Length of the FCR_PRIMARY_SSN attribute.</summary>
  public const int FcrPrimarySsn_MaxLength = 9;

  /// <summary>
  /// The value of the FCR_PRIMARY_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 61, Type = MemberType.Char, Length = FcrPrimarySsn_MaxLength)]
  public string FcrPrimarySsn
  {
    get => fcrPrimarySsn ?? "";
    set => fcrPrimarySsn =
      TrimEnd(Substring(value, 1, FcrPrimarySsn_MaxLength));
  }

  /// <summary>
  /// The json value of the FcrPrimarySsn attribute.</summary>
  [JsonPropertyName("fcrPrimarySsn")]
  [Computed]
  public string FcrPrimarySsn_Json
  {
    get => NullIf(FcrPrimarySsn, "");
    set => FcrPrimarySsn = value;
  }

  /// <summary>Length of the FCR_PRIMARY_FIRST_NAME attribute.</summary>
  public const int FcrPrimaryFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the FCR_PRIMARY_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 62, Type = MemberType.Char, Length
    = FcrPrimaryFirstName_MaxLength)]
  public string FcrPrimaryFirstName
  {
    get => fcrPrimaryFirstName ?? "";
    set => fcrPrimaryFirstName =
      TrimEnd(Substring(value, 1, FcrPrimaryFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the FcrPrimaryFirstName attribute.</summary>
  [JsonPropertyName("fcrPrimaryFirstName")]
  [Computed]
  public string FcrPrimaryFirstName_Json
  {
    get => NullIf(FcrPrimaryFirstName, "");
    set => FcrPrimaryFirstName = value;
  }

  /// <summary>Length of the FCR_PRIMARY_MIDDLE_NAME attribute.</summary>
  public const int FcrPrimaryMiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the FCR_PRIMARY_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 63, Type = MemberType.Char, Length
    = FcrPrimaryMiddleName_MaxLength)]
  public string FcrPrimaryMiddleName
  {
    get => fcrPrimaryMiddleName ?? "";
    set => fcrPrimaryMiddleName =
      TrimEnd(Substring(value, 1, FcrPrimaryMiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the FcrPrimaryMiddleName attribute.</summary>
  [JsonPropertyName("fcrPrimaryMiddleName")]
  [Computed]
  public string FcrPrimaryMiddleName_Json
  {
    get => NullIf(FcrPrimaryMiddleName, "");
    set => FcrPrimaryMiddleName = value;
  }

  /// <summary>Length of the FCR_PRIMARY_LAST_NAME attribute.</summary>
  public const int FcrPrimaryLastName_MaxLength = 30;

  /// <summary>
  /// The value of the FCR_PRIMARY_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 64, Type = MemberType.Char, Length
    = FcrPrimaryLastName_MaxLength)]
  public string FcrPrimaryLastName
  {
    get => fcrPrimaryLastName ?? "";
    set => fcrPrimaryLastName =
      TrimEnd(Substring(value, 1, FcrPrimaryLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the FcrPrimaryLastName attribute.</summary>
  [JsonPropertyName("fcrPrimaryLastName")]
  [Computed]
  public string FcrPrimaryLastName_Json
  {
    get => NullIf(FcrPrimaryLastName, "");
    set => FcrPrimaryLastName = value;
  }

  /// <summary>Length of the ACKNOWLEDGEMENT_CODE attribute.</summary>
  public const int AcknowledgementCode_MaxLength = 5;

  /// <summary>
  /// The value of the ACKNOWLEDGEMENT_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 65, Type = MemberType.Char, Length
    = AcknowledgementCode_MaxLength)]
  public string AcknowledgementCode
  {
    get => acknowledgementCode ?? "";
    set => acknowledgementCode =
      TrimEnd(Substring(value, 1, AcknowledgementCode_MaxLength));
  }

  /// <summary>
  /// The json value of the AcknowledgementCode attribute.</summary>
  [JsonPropertyName("acknowledgementCode")]
  [Computed]
  public string AcknowledgementCode_Json
  {
    get => NullIf(AcknowledgementCode, "");
    set => AcknowledgementCode = value;
  }

  /// <summary>Length of the ERROR_CODE_1 attribute.</summary>
  public const int ErrorCode1_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 66, Type = MemberType.Char, Length = ErrorCode1_MaxLength)]
  public string ErrorCode1
  {
    get => errorCode1 ?? "";
    set => errorCode1 = TrimEnd(Substring(value, 1, ErrorCode1_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorCode1 attribute.</summary>
  [JsonPropertyName("errorCode1")]
  [Computed]
  public string ErrorCode1_Json
  {
    get => NullIf(ErrorCode1, "");
    set => ErrorCode1 = value;
  }

  /// <summary>Length of the ERROR_MESSAGE_1 attribute.</summary>
  public const int ErrorMessage1_MaxLength = 50;

  /// <summary>
  /// The value of the ERROR_MESSAGE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 67, Type = MemberType.Char, Length = ErrorMessage1_MaxLength)]
  public string ErrorMessage1
  {
    get => errorMessage1 ?? "";
    set => errorMessage1 =
      TrimEnd(Substring(value, 1, ErrorMessage1_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorMessage1 attribute.</summary>
  [JsonPropertyName("errorMessage1")]
  [Computed]
  public string ErrorMessage1_Json
  {
    get => NullIf(ErrorMessage1, "");
    set => ErrorMessage1 = value;
  }

  /// <summary>Length of the ERROR_CODE_2 attribute.</summary>
  public const int ErrorCode2_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 68, Type = MemberType.Char, Length = ErrorCode2_MaxLength)]
  public string ErrorCode2
  {
    get => errorCode2 ?? "";
    set => errorCode2 = TrimEnd(Substring(value, 1, ErrorCode2_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorCode2 attribute.</summary>
  [JsonPropertyName("errorCode2")]
  [Computed]
  public string ErrorCode2_Json
  {
    get => NullIf(ErrorCode2, "");
    set => ErrorCode2 = value;
  }

  /// <summary>Length of the ERROR_MESSAGE_2 attribute.</summary>
  public const int ErrorMessage2_MaxLength = 50;

  /// <summary>
  /// The value of the ERROR_MESSAGE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 69, Type = MemberType.Char, Length = ErrorMessage2_MaxLength)]
  public string ErrorMessage2
  {
    get => errorMessage2 ?? "";
    set => errorMessage2 =
      TrimEnd(Substring(value, 1, ErrorMessage2_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorMessage2 attribute.</summary>
  [JsonPropertyName("errorMessage2")]
  [Computed]
  public string ErrorMessage2_Json
  {
    get => NullIf(ErrorMessage2, "");
    set => ErrorMessage2 = value;
  }

  /// <summary>Length of the ERROR_CODE_3 attribute.</summary>
  public const int ErrorCode3_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 70, Type = MemberType.Char, Length = ErrorCode3_MaxLength)]
  public string ErrorCode3
  {
    get => errorCode3 ?? "";
    set => errorCode3 = TrimEnd(Substring(value, 1, ErrorCode3_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorCode3 attribute.</summary>
  [JsonPropertyName("errorCode3")]
  [Computed]
  public string ErrorCode3_Json
  {
    get => NullIf(ErrorCode3, "");
    set => ErrorCode3 = value;
  }

  /// <summary>Length of the ERROR_MESSAGE_3 attribute.</summary>
  public const int ErrorMessage3_MaxLength = 50;

  /// <summary>
  /// The value of the ERROR_MESSAGE_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 71, Type = MemberType.Char, Length = ErrorMessage3_MaxLength)]
  public string ErrorMessage3
  {
    get => errorMessage3 ?? "";
    set => errorMessage3 =
      TrimEnd(Substring(value, 1, ErrorMessage3_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorMessage3 attribute.</summary>
  [JsonPropertyName("errorMessage3")]
  [Computed]
  public string ErrorMessage3_Json
  {
    get => NullIf(ErrorMessage3, "");
    set => ErrorMessage3 = value;
  }

  /// <summary>Length of the ERROR_CODE_4 attribute.</summary>
  public const int ErrorCode4_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 72, Type = MemberType.Char, Length = ErrorCode4_MaxLength)]
  public string ErrorCode4
  {
    get => errorCode4 ?? "";
    set => errorCode4 = TrimEnd(Substring(value, 1, ErrorCode4_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorCode4 attribute.</summary>
  [JsonPropertyName("errorCode4")]
  [Computed]
  public string ErrorCode4_Json
  {
    get => NullIf(ErrorCode4, "");
    set => ErrorCode4 = value;
  }

  /// <summary>Length of the ERROR_MESSAGE_4 attribute.</summary>
  public const int ErrorMessage4_MaxLength = 50;

  /// <summary>
  /// The value of the ERROR_MESSAGE_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 73, Type = MemberType.Char, Length = ErrorMessage4_MaxLength)]
  public string ErrorMessage4
  {
    get => errorMessage4 ?? "";
    set => errorMessage4 =
      TrimEnd(Substring(value, 1, ErrorMessage4_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorMessage4 attribute.</summary>
  [JsonPropertyName("errorMessage4")]
  [Computed]
  public string ErrorMessage4_Json
  {
    get => NullIf(ErrorMessage4, "");
    set => ErrorMessage4 = value;
  }

  /// <summary>Length of the ERROR_CODE_5 attribute.</summary>
  public const int ErrorCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_5 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 74, Type = MemberType.Char, Length = ErrorCode5_MaxLength)]
  public string ErrorCode5
  {
    get => errorCode5 ?? "";
    set => errorCode5 = TrimEnd(Substring(value, 1, ErrorCode5_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorCode5 attribute.</summary>
  [JsonPropertyName("errorCode5")]
  [Computed]
  public string ErrorCode5_Json
  {
    get => NullIf(ErrorCode5, "");
    set => ErrorCode5 = value;
  }

  /// <summary>Length of the ERROR_MESSAGE_5 attribute.</summary>
  public const int ErrorMessage5_MaxLength = 50;

  /// <summary>
  /// The value of the ERROR_MESSAGE_5 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 75, Type = MemberType.Char, Length = ErrorMessage5_MaxLength)]
  public string ErrorMessage5
  {
    get => errorMessage5 ?? "";
    set => errorMessage5 =
      TrimEnd(Substring(value, 1, ErrorMessage5_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorMessage5 attribute.</summary>
  [JsonPropertyName("errorMessage5")]
  [Computed]
  public string ErrorMessage5_Json
  {
    get => NullIf(ErrorMessage5, "");
    set => ErrorMessage5 = value;
  }

  /// <summary>Length of the ADDITIONAL_SSN1_VALIDITY_CODE attribute.</summary>
  public const int AdditionalSsn1ValidityCode_MaxLength = 1;

  /// <summary>
  /// The value of the ADDITIONAL_SSN1_VALIDITY_CODE attribute.
  /// This field contains a code to indicate the validity of the additional SSN 
  /// that was submitted:
  /// V - The additional SSN! and name combination submitted was verified by the
  /// SSA SSN verification rountines.
  /// U - The additional SSN! and name combination submitted was not verified by
  /// the SSA SSN verification rountines.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 76, Type = MemberType.Char, Length
    = AdditionalSsn1ValidityCode_MaxLength)]
  public string AdditionalSsn1ValidityCode
  {
    get => additionalSsn1ValidityCode ?? "";
    set => additionalSsn1ValidityCode =
      TrimEnd(Substring(value, 1, AdditionalSsn1ValidityCode_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalSsn1ValidityCode attribute.</summary>
  [JsonPropertyName("additionalSsn1ValidityCode")]
  [Computed]
  public string AdditionalSsn1ValidityCode_Json
  {
    get => NullIf(AdditionalSsn1ValidityCode, "");
    set => AdditionalSsn1ValidityCode = value;
  }

  /// <summary>Length of the ADDITIONAL_SSN2_VALIDITY_CODE attribute.</summary>
  public const int AdditionalSsn2ValidityCode_MaxLength = 1;

  /// <summary>
  /// The value of the ADDITIONAL_SSN2_VALIDITY_CODE attribute.
  /// This field contains a code to indicate the validity of the additional SSN 
  /// that was submitted:
  /// V - The additional SSN! and name combination submitted was verified by the
  /// SSA SSN verification rountines.
  /// U - The additional SSN! and name combination submitted was not verified by
  /// the SSA SSN verification rountines.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 77, Type = MemberType.Char, Length
    = AdditionalSsn2ValidityCode_MaxLength)]
  public string AdditionalSsn2ValidityCode
  {
    get => additionalSsn2ValidityCode ?? "";
    set => additionalSsn2ValidityCode =
      TrimEnd(Substring(value, 1, AdditionalSsn2ValidityCode_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalSsn2ValidityCode attribute.</summary>
  [JsonPropertyName("additionalSsn2ValidityCode")]
  [Computed]
  public string AdditionalSsn2ValidityCode_Json
  {
    get => NullIf(AdditionalSsn2ValidityCode, "");
    set => AdditionalSsn2ValidityCode = value;
  }

  private string recordIdentifier;
  private string actionTypeCode;
  private string caseId;
  private string userField;
  private string fipsCountyCode;
  private string locateRequestType;
  private string bundleFplsLocateResults;
  private string participantType;
  private string familyViolence;
  private string memberId;
  private string sexCode;
  private DateTime? dateOfBirth;
  private string ssn;
  private string previousSsn;
  private string firstName;
  private string middleName;
  private string lastName;
  private string cityOfBirth;
  private string stateOrCountryOfBirth;
  private string fathersFirstName;
  private string fathersMiddleInitial;
  private string fathersLastName;
  private string mothersFirstName;
  private string mothersMiddleInitial;
  private string mothersMaidenName;
  private string irsUSsn;
  private string additionalSsn1;
  private string additionalSsn2;
  private string additionalFirstName1;
  private string additionalMiddleName1;
  private string additionalLastName1;
  private string additionalFirstName2;
  private string additionalMiddleName2;
  private string additionalLastName2;
  private string additionalFirstName3;
  private string additionalMiddleName3;
  private string additionalLastName3;
  private string additionalFirstName4;
  private string additionalMiddleName4;
  private string additionalLastName4;
  private string newMemberId;
  private string irs1099;
  private string locateSource1;
  private string locateSource2;
  private string locateSource3;
  private string locateSource4;
  private string locateSource5;
  private string locateSource6;
  private string locateSource7;
  private string locateSource8;
  private string ssnValidityCode;
  private string providedOrCorrectedSsn;
  private string multipleSsn1;
  private string multipleSsn2;
  private string multipleSsn3;
  private string ssaDateOfBirthIndicator;
  private string batchNumber;
  private DateTime? dateOfDeath;
  private string ssaZipCodeOfLastResidence;
  private string ssaZipCodeOfLumpSumPayment;
  private string fcrPrimarySsn;
  private string fcrPrimaryFirstName;
  private string fcrPrimaryMiddleName;
  private string fcrPrimaryLastName;
  private string acknowledgementCode;
  private string errorCode1;
  private string errorMessage1;
  private string errorCode2;
  private string errorMessage2;
  private string errorCode3;
  private string errorMessage3;
  private string errorCode4;
  private string errorMessage4;
  private string errorCode5;
  private string errorMessage5;
  private string additionalSsn1ValidityCode;
  private string additionalSsn2ValidityCode;
}
