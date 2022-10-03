// The source file: FCR_MASTER_MEMBER_RECORD, ID: 374577034, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class FcrMasterMemberRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrMasterMemberRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrMasterMemberRecord(FcrMasterMemberRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrMasterMemberRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrMasterMemberRecord that)
  {
    base.Assign(that);
    caseId = that.caseId;
    memberId = that.memberId;
    actionTypeCode = that.actionTypeCode;
    locateRequestType = that.locateRequestType;
    recordIdentifier = that.recordIdentifier;
    participantType = that.participantType;
    sexCode = that.sexCode;
    dateOfBirth = that.dateOfBirth;
    ssn = that.ssn;
    firstName = that.firstName;
    middleName = that.middleName;
    lastName = that.lastName;
    fipsCountyCode = that.fipsCountyCode;
    familyViolence = that.familyViolence;
    previousSsn = that.previousSsn;
    cityOfBirth = that.cityOfBirth;
    stateOrCountryOfBirth = that.stateOrCountryOfBirth;
    fathersFirstName = that.fathersFirstName;
    fathersMiddleInitial = that.fathersMiddleInitial;
    fathersLastName = that.fathersLastName;
    mothersFirstName = that.mothersFirstName;
    mothersMiddleInitial = that.mothersMiddleInitial;
    mothersMaidenNm = that.mothersMaidenNm;
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
    errorCode2 = that.errorCode2;
    errorCode3 = that.errorCode3;
    errorCode4 = that.errorCode4;
    errorCode5 = that.errorCode5;
    additionalSsn1ValidityCode = that.additionalSsn1ValidityCode;
    additionalSsn2ValidityCode = that.additionalSsn2ValidityCode;
    bundleFplsLocateResults = that.bundleFplsLocateResults;
    ssaCityOfLastResidence = that.ssaCityOfLastResidence;
    ssaStateOfLastResidence = that.ssaStateOfLastResidence;
    ssaCityOfLumpSumPayment = that.ssaCityOfLumpSumPayment;
    ssaStateOfLumpSumPayment = that.ssaStateOfLumpSumPayment;
    lastSentDtToFcr = that.lastSentDtToFcr;
    lastReceivedDtFromFcr = that.lastReceivedDtFromFcr;
    memberUserField = that.memberUserField;
  }

  /// <summary>Length of the CASE_ID attribute.</summary>
  public const int CaseId_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_ID attribute.
  /// This field must contain a unique identifier assigned to the case by the KS
  /// CSE Application. It must not be all spaces, all zeroes, contain an
  /// asterisk or backslash and the first position must not be a space.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CaseId_MaxLength)]
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

  /// <summary>Length of the MEMBER_ID attribute.</summary>
  public const int MemberId_MaxLength = 10;

  /// <summary>
  /// The value of the MEMBER_ID attribute.
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = MemberId_MaxLength)]
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

  /// <summary>Length of the ACTION_TYPE_CODE attribute.</summary>
  public const int ActionTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION_TYPE_CODE attribute.
  /// This field contains the information that was submitted by CSE Application.
  /// This fields will havThis field contains the information that was
  /// submitted by CSE Application.  This fields will have any of the below
  /// mentioned values:
  /// 'A' dd       - Add new records to FCR database
  /// 'C'hanage - Change the information to the existing record with FCR.
  /// 'D'elete     - Remove the existing Case/Person record from FCR databasee 
  /// any of the below mentioned values:
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = ActionTypeCode_MaxLength)]
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

  /// <summary>Length of the LOCATE_REQUEST_TYPE attribute.</summary>
  public const int LocateRequestType_MaxLength = 2;

  /// <summary>
  /// The value of the LOCATE_REQUEST_TYPE attribute.
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// This field can be used to initiate a Locate Request when the person is 
  /// being added to the FCR. The field must contain the following code or
  /// spaces:
  /// CS	- Request for IV-D purposes
  /// The Locate Request Type must be consistent with the person's Case Type. 
  /// Refer to Chart 6-14,
  /// &quot;Types of Locate Requests&quot;, for an explanation of the 
  /// information available based on the Locate Request Type Code.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
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

  /// <summary>Length of the RECORD_IDENTIFIER attribute.</summary>
  public const int RecordIdentifier_MaxLength = 2;

  /// <summary>
  /// The value of the RECORD_IDENTIFIER attribute.
  /// This field contains the characters 'FS'.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = RecordIdentifier_MaxLength)
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

  /// <summary>Length of the PARTICIPANT_TYPE attribute.</summary>
  public const int ParticipantType_MaxLength = 2;

  /// <summary>
  /// The value of the PARTICIPANT_TYPE attribute.
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// This field must contain one of the following codes to define the person's 
  /// Participant Type in the case:
  /// CH	Child
  /// CP	Custodial Party
  /// NP	Non-custodial Parent
  /// PF	Putative Father (allowed for IV-D cases only)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = ParticipantType_MaxLength)]
    
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

  /// <summary>Length of the SEX_CODE attribute.</summary>
  public const int SexCode_MaxLength = 1;

  /// <summary>
  /// The value of the SEX_CODE attribute.
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = SexCode_MaxLength)]
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
  /// If the Date of Birth submitted is different from the Date of Birth on SSA'
  /// s records, this field contains SSA's recorded Date of Birth for the
  /// person.
  /// Otherwise, this field contains the information that was submitted by the 
  /// Kansas CSE Appplication.
  /// If a Date of Birth was not submitted and one could not be found in SSA's 
  /// records, this field contains spaces.
  /// If Warning Code PW010 is returned, this field contains the Date of Birth 
  /// stored on the FCR Database for the person.
  /// </summary>
  [JsonPropertyName("dateOfBirth")]
  [Member(Index = 8, Type = MemberType.Date)]
  public DateTime? DateOfBirth
  {
    get => dateOfBirth;
    set => dateOfBirth = value;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = Ssn_MaxLength)]
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

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 16;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = FirstName_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = MiddleName_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = LastName_MaxLength)]
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

  /// <summary>Length of the FIPS_COUNTY_CODE attribute.</summary>
  public const int FipsCountyCode_MaxLength = 3;

  /// <summary>
  /// The value of the FIPS_COUNTY_CODE attribute.
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = FipsCountyCode_MaxLength)]
    
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

  /// <summary>Length of the FAMILY_VIOLENCE attribute.</summary>
  public const int FamilyViolence_MaxLength = 2;

  /// <summary>
  /// The value of the FAMILY_VIOLENCE attribute.
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = FamilyViolence_MaxLength)]
    
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

  /// <summary>Length of the PREVIOUS_SSN attribute.</summary>
  public const int PreviousSsn_MaxLength = 9;

  /// <summary>
  /// The value of the PREVIOUS_SSN attribute.
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = PreviousSsn_MaxLength)]
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

  /// <summary>Length of the CITY_OF_BIRTH attribute.</summary>
  public const int CityOfBirth_MaxLength = 16;

  /// <summary>
  /// The value of the CITY_OF_BIRTH attribute.
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = CityOfBirth_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = FathersLastName_MaxLength)
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length
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

  /// <summary>Length of the MOTHERS_MAIDEN_NM attribute.</summary>
  public const int MothersMaidenNm_MaxLength = 16;

  /// <summary>
  /// The value of the MOTHERS_MAIDEN_NM attribute.
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length = MothersMaidenNm_MaxLength)
    ]
  public string MothersMaidenNm
  {
    get => mothersMaidenNm ?? "";
    set => mothersMaidenNm =
      TrimEnd(Substring(value, 1, MothersMaidenNm_MaxLength));
  }

  /// <summary>
  /// The json value of the MothersMaidenNm attribute.</summary>
  [JsonPropertyName("mothersMaidenNm")]
  [Computed]
  public string MothersMaidenNm_Json
  {
    get => NullIf(MothersMaidenNm, "");
    set => MothersMaidenNm = value;
  }

  /// <summary>Length of the IRS_U_SSN attribute.</summary>
  public const int IrsUSsn_MaxLength = 9;

  /// <summary>
  /// The value of the IRS_U_SSN attribute.
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length = IrsUSsn_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = AdditionalSsn1_MaxLength)]
    
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length = AdditionalSsn2_MaxLength)]
    
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 39, Type = MemberType.Char, Length = NewMemberId_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length = Irs1099_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length = LocateSource1_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length = LocateSource2_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length = LocateSource3_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 44, Type = MemberType.Char, Length = LocateSource4_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length = LocateSource5_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length = LocateSource6_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length = LocateSource7_MaxLength)]
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
  /// This field contains the information that was submitted by Kansas CSE 
  /// Application on the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 48, Type = MemberType.Char, Length = LocateSource8_MaxLength)]
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
  /// This field contains a value to indicate the validity of the SSN that was 
  /// submitted:
  /// C	- The SSN that was submitted for this person was corrected.
  /// E	- The SSN and Name combination that was submitted for this person could 
  /// not be verified or corrected but the additional person data that was
  /// provided identified an SSN for this person.
  /// N	- The SSN and Name combination that was submitted for this person was 
  /// not verified by SSA's SSN Verification Routines, but the Name Matching
  /// Routine has identified it as a probable name match.
  /// P	- The SSN was not submitted, but the additional person data that was 
  /// submitted identified an SSN for this person without manual intervention
  /// and it is provided; or the SSN that was provided did not verify but an SSN
  /// was identified using SSA's alpha search.
  /// R	- The person data that was submitted identified multiple possible SSNs 
  /// for the person, and the SSN that was provided was selected using the
  /// Requires Manual Review process.
  /// S	- The IRS-U SSN that was submitted allowed the SSN to be identified by 
  /// using the IRS information.
  /// V	- The SSN and Name combination that was submitted was verified by SSA's 
  /// SSN verification routines.
  /// Space - The SSN that was provided could not be verified, or there was no 
  /// SSN provided and one could not be identified using the information
  /// submitted. See the Error Code 1-5 fields for a more specific explanation
  /// of the condition.
  /// If the Acknowledgement Code equals 'AAAAA' and this field equals a space, 
  /// the person has been accepted by the FCR System as an unverified person and
  /// is not available for FCR Query or proactive matching.  If a State
  /// identifies a new SSN (an IRS-U SSN, Additional Name information, or ESKARI
  /// information that can be used to verify the SSN) the State may submit the
  /// new information as a Change Transaction. The State may also elect to
  /// delete the person and add them back to the FCR Database with the new
  /// information.  If the Acknowledgement Code equals 'REJCT', the FCR System
  /// has rejected the person
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 49, Type = MemberType.Char, Length = SsnValidityCode_MaxLength)
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
  /// If present, this field is the identified or corrected SSN for the person 
  /// who was found during the SSN verification routines. The Provided/Corrected
  /// SSN is used to store the person record on the FCR Database.
  /// If the SSN Validity Code equals 'C', 'E', 'P', 'R' or 'S', this field 
  /// contains a valid SSN.
  /// If the SSN Validity Code does not equal 'C', 'E', 'P', 'R' or 'S', this 
  /// field contains spaces.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 50, Type = MemberType.Char, Length
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
  /// If the SSA SSN verification routines identified multiple valid SSNs for 
  /// the person, this field contains the third additional SSN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 51, Type = MemberType.Char, Length = MultipleSsn1_MaxLength)]
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
  /// If the SSA SSN verification routines identified multiple valid SSNs for 
  /// the person, this field contains the third additional SSN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 52, Type = MemberType.Char, Length = MultipleSsn2_MaxLength)]
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
  /// If the SSA SSN verification routines identified multiple valid SSNs for 
  /// the person, this field contains the third additional SSN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 53, Type = MemberType.Char, Length = MultipleSsn3_MaxLength)]
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
  /// N = DOB on the input record matched the DOB in SSA records.
  ///  If the Date of Birth that was submitted on the input record is within one
  /// year (plus or minus) of the Date of Birth that is stored on the FCR
  /// Database, this field contains an 'N'.
  /// If the Date of Birth that is returned on this record is not within one 
  /// year (plus or minus) of the Date of Birth that is stored on the FCR
  /// Database, or if it was not sent by the State but is identified by SSA,
  /// this field contains a 'Y'.
  /// If Warning Code PW010 is returned, this field contains a 'Y'.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 54, Type = MemberType.Char, Length
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
  /// This field contains the Kansas CSE Application assigned number of the 
  /// batch that contained the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 55, Type = MemberType.Char, Length = BatchNumber_MaxLength)]
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
  /// This field contains the SSA-recorded Date of Death in CCYYMMDD format, for
  /// the person.
  /// If no DOD is available, this field contains spaces.
  /// Note: If the SSA Death Master File contained '00' in the day, the FCR 
  /// System returns '01' in the day portion of the Date of Death.
  /// </summary>
  [JsonPropertyName("dateOfDeath")]
  [Member(Index = 56, Type = MemberType.Date)]
  public DateTime? DateOfDeath
  {
    get => dateOfDeath;
    set => dateOfDeath = value;
  }

  /// <summary>Length of the SSA_ZIP_CODE_OF_LAST_RESIDENCE attribute.</summary>
  public const int SsaZipCodeOfLastResidence_MaxLength = 5;

  /// <summary>
  /// The value of the SSA_ZIP_CODE_OF_LAST_RESIDENCE attribute.
  /// This field contains the valid Zip Code of where the lump sum death benefit
  /// payment was sent based on SSA's death records. Invalid or incomplete Zip
  /// Codes on the SSA Death Record are not returned.
  /// If a Zip Code is not available, this field contains spaces.
  /// If FINALIST did not validate the Zip Code that was provided by the Death 
  /// Master File, this field contains spaces.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 57, Type = MemberType.Char, Length
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
  /// This field contains the valid Zip Code of where the lump sum death benefit
  /// payment was sent based on SSA's death records. Invalid or incomplete Zip
  /// Codes on the SSA Death Record are not returned.
  /// If a Zip Code is not available, this field contains spaces.
  /// If FINALIST did not validate the Zip Code that was provided by the Death 
  /// Master File, this field contains spaces.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 58, Type = MemberType.Char, Length
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
  /// This field contains the SSN that is stored on the FCR Database as the 
  /// person's primary SSN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 59, Type = MemberType.Char, Length = FcrPrimarySsn_MaxLength)]
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
  /// This field contains the first name of the person who is stored on the FCR 
  /// Database that verified with the Primary SSN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 60, Type = MemberType.Char, Length
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
  /// This is the middle name of the person who is stored on the FCR Database 
  /// that verified with the Primary SSN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 61, Type = MemberType.Char, Length
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
  /// This is the last name of the person who is stored on the FCR Database that
  /// verified with the Primary SSN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 62, Type = MemberType.Char, Length
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
  /// This field contains a code that indicates if the record was accepted, 
  /// rejected, or is pending.
  /// If the record was accepted, this field contains the code 'AAAAA'.
  /// If the record is pending SSN identification on the person record, this 
  /// field contains the code 'HOLDS'.
  /// If the record was rejected, this field contains the code 'REJCT'.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 63, Type = MemberType.Char, Length
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
  /// If the record was accepted, but a fifth non-critical error was detected, 
  /// this field contains an alphanumeric warning code beginning with 'LW', 'PW'
  /// or 'TW'.
  /// If the record was rejected for multiple errors, this field contains an 
  /// alphanumeric error code beginning with 'LE', 'PE' or 'TE'.
  /// For a complete explanation of these codes.  Refer to FCR Appendix J, 
  /// &quot;Error Messages&quot;.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 64, Type = MemberType.Char, Length = ErrorCode1_MaxLength)]
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

  /// <summary>Length of the ERROR_CODE_2 attribute.</summary>
  public const int ErrorCode2_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_2 attribute.
  /// If the record was accepted, but a fifth non-critical error was detected, 
  /// this field contains an alphanumeric warning code beginning with 'LW', 'PW'
  /// or 'TW'.
  /// If the record was rejected for multiple errors, this field contains an 
  /// alphanumeric error code beginning with 'LE', 'PE' or 'TE'.
  /// For a complete explanation of these codes.  Refer to FCR Appendix J, 
  /// &quot;Error Messages&quot;.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 65, Type = MemberType.Char, Length = ErrorCode2_MaxLength)]
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

  /// <summary>Length of the ERROR_CODE_3 attribute.</summary>
  public const int ErrorCode3_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_3 attribute.
  /// If the record was accepted, but a fifth non-critical error was detected, 
  /// this field contains an alphanumeric warning code beginning with 'LW', 'PW'
  /// or 'TW'.
  /// If the record was rejected for multiple errors, this field contains an 
  /// alphanumeric error code beginning with 'LE', 'PE' or 'TE'.
  /// For a complete explanation of these codes.  Refer to FCR Appendix J, 
  /// &quot;Error Messages&quot;.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 66, Type = MemberType.Char, Length = ErrorCode3_MaxLength)]
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

  /// <summary>Length of the ERROR_CODE_4 attribute.</summary>
  public const int ErrorCode4_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_4 attribute.
  /// If the record was accepted, but a fifth non-critical error was detected, 
  /// this field contains an alphanumeric warning code beginning with 'LW', 'PW'
  /// or 'TW'.
  /// If the record was rejected for multiple errors, this field contains an 
  /// alphanumeric error code beginning with 'LE', 'PE' or 'TE'.
  /// For a complete explanation of these codes.  Refer to FCR Appendix J, 
  /// &quot;Error Messages&quot;.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 67, Type = MemberType.Char, Length = ErrorCode4_MaxLength)]
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

  /// <summary>Length of the ERROR_CODE_5 attribute.</summary>
  public const int ErrorCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_5 attribute.
  /// If the record was accepted, but a fifth non-critical error was detected, 
  /// this field contains an alphanumeric warning code beginning with 'LW', 'PW'
  /// or 'TW'.
  /// If the record was rejected for multiple errors, this field contains an 
  /// alphanumeric error code beginning with 'LE', 'PE' or 'TE'.
  /// For a complete explanation of these codes.  Refer to FCR Appendix J, 
  /// &quot;Error Messages&quot;.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 68, Type = MemberType.Char, Length = ErrorCode5_MaxLength)]
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
  [Member(Index = 69, Type = MemberType.Char, Length
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
  [Member(Index = 70, Type = MemberType.Char, Length
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

  /// <summary>Length of the BUNDLE_FPLS_LOCATE_RESULTS attribute.</summary>
  public const int BundleFplsLocateResults_MaxLength = 1;

  /// <summary>
  /// The value of the BUNDLE_FPLS_LOCATE_RESULTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 71, Type = MemberType.Char, Length
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

  /// <summary>Length of the SSA_CITY_OF_LAST_RESIDENCE attribute.</summary>
  public const int SsaCityOfLastResidence_MaxLength = 15;

  /// <summary>
  /// The value of the SSA_CITY_OF_LAST_RESIDENCE attribute.
  /// If FINALIST validated the SSA Zip Code of Last Residence that was provided
  /// by the Death Master File, this field contains the city that is associated
  /// with the Zip Code in the FINALIST database.   If a valid Zip Code is not
  /// available, this field contains spaces.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 72, Type = MemberType.Char, Length
    = SsaCityOfLastResidence_MaxLength)]
  public string SsaCityOfLastResidence
  {
    get => ssaCityOfLastResidence ?? "";
    set => ssaCityOfLastResidence =
      TrimEnd(Substring(value, 1, SsaCityOfLastResidence_MaxLength));
  }

  /// <summary>
  /// The json value of the SsaCityOfLastResidence attribute.</summary>
  [JsonPropertyName("ssaCityOfLastResidence")]
  [Computed]
  public string SsaCityOfLastResidence_Json
  {
    get => NullIf(SsaCityOfLastResidence, "");
    set => SsaCityOfLastResidence = value;
  }

  /// <summary>Length of the SSA_STATE_OF_LAST_RESIDENCE attribute.</summary>
  public const int SsaStateOfLastResidence_MaxLength = 2;

  /// <summary>
  /// The value of the SSA_STATE_OF_LAST_RESIDENCE attribute.
  /// If FINALIST validated the SSA Zip Code of Last Residence that was provided
  /// by the Death Master File, this field contains the state that is
  /// associated with the Zip Code in the FINALIST database.   If a valid Zip
  /// Code is not available, this field contains spaces.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 73, Type = MemberType.Char, Length
    = SsaStateOfLastResidence_MaxLength)]
  public string SsaStateOfLastResidence
  {
    get => ssaStateOfLastResidence ?? "";
    set => ssaStateOfLastResidence =
      TrimEnd(Substring(value, 1, SsaStateOfLastResidence_MaxLength));
  }

  /// <summary>
  /// The json value of the SsaStateOfLastResidence attribute.</summary>
  [JsonPropertyName("ssaStateOfLastResidence")]
  [Computed]
  public string SsaStateOfLastResidence_Json
  {
    get => NullIf(SsaStateOfLastResidence, "");
    set => SsaStateOfLastResidence = value;
  }

  /// <summary>Length of the SSA_CITY_OF_LUMP_SUM_PAYMENT attribute.</summary>
  public const int SsaCityOfLumpSumPayment_MaxLength = 15;

  /// <summary>
  /// The value of the SSA_CITY_OF_LUMP_SUM_PAYMENT attribute.
  /// If FINALIST validated the SSA Zip Code of Lump Sum Payment that was 
  /// provided by the Death Master File, this field contains the city that is
  /// associated with the Zip Code in the FINALIST database.   If a valid Zip
  /// Code is not available, this field contains spaces.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 74, Type = MemberType.Char, Length
    = SsaCityOfLumpSumPayment_MaxLength)]
  public string SsaCityOfLumpSumPayment
  {
    get => ssaCityOfLumpSumPayment ?? "";
    set => ssaCityOfLumpSumPayment =
      TrimEnd(Substring(value, 1, SsaCityOfLumpSumPayment_MaxLength));
  }

  /// <summary>
  /// The json value of the SsaCityOfLumpSumPayment attribute.</summary>
  [JsonPropertyName("ssaCityOfLumpSumPayment")]
  [Computed]
  public string SsaCityOfLumpSumPayment_Json
  {
    get => NullIf(SsaCityOfLumpSumPayment, "");
    set => SsaCityOfLumpSumPayment = value;
  }

  /// <summary>Length of the SSA_STATE_OF_LUMP_SUM_PAYMENT attribute.</summary>
  public const int SsaStateOfLumpSumPayment_MaxLength = 2;

  /// <summary>
  /// The value of the SSA_STATE_OF_LUMP_SUM_PAYMENT attribute.
  /// If FINALIST validated the SSA Zip Code of Lump Sum Payment that was 
  /// provided by the Death Master File, this field contains the state that is
  /// associated with the Zip Code in the FINALIST database.   If a valid Zip
  /// Code is not available, this field contains spaces.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 75, Type = MemberType.Char, Length
    = SsaStateOfLumpSumPayment_MaxLength)]
  public string SsaStateOfLumpSumPayment
  {
    get => ssaStateOfLumpSumPayment ?? "";
    set => ssaStateOfLumpSumPayment =
      TrimEnd(Substring(value, 1, SsaStateOfLumpSumPayment_MaxLength));
  }

  /// <summary>
  /// The json value of the SsaStateOfLumpSumPayment attribute.</summary>
  [JsonPropertyName("ssaStateOfLumpSumPayment")]
  [Computed]
  public string SsaStateOfLumpSumPayment_Json
  {
    get => NullIf(SsaStateOfLumpSumPayment, "");
    set => SsaStateOfLumpSumPayment = value;
  }

  /// <summary>
  /// The value of the LAST_SENT_DT_TO_FCR attribute.
  /// </summary>
  [JsonPropertyName("lastSentDtToFcr")]
  [Member(Index = 76, Type = MemberType.Date)]
  public DateTime? LastSentDtToFcr
  {
    get => lastSentDtToFcr;
    set => lastSentDtToFcr = value;
  }

  /// <summary>
  /// The value of the LAST_RECEIVED_DT_FROM_FCR attribute.
  /// </summary>
  [JsonPropertyName("lastReceivedDtFromFcr")]
  [Member(Index = 77, Type = MemberType.Date)]
  public DateTime? LastReceivedDtFromFcr
  {
    get => lastReceivedDtFromFcr;
    set => lastReceivedDtFromFcr = value;
  }

  /// <summary>Length of the MEMBER_USER_FIELD attribute.</summary>
  public const int MemberUserField_MaxLength = 15;

  /// <summary>
  /// The value of the MEMBER_USER_FIELD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 78, Type = MemberType.Char, Length = MemberUserField_MaxLength)
    ]
  public string MemberUserField
  {
    get => memberUserField ?? "";
    set => memberUserField =
      TrimEnd(Substring(value, 1, MemberUserField_MaxLength));
  }

  /// <summary>
  /// The json value of the MemberUserField attribute.</summary>
  [JsonPropertyName("memberUserField")]
  [Computed]
  public string MemberUserField_Json
  {
    get => NullIf(MemberUserField, "");
    set => MemberUserField = value;
  }

  private string caseId;
  private string memberId;
  private string actionTypeCode;
  private string locateRequestType;
  private string recordIdentifier;
  private string participantType;
  private string sexCode;
  private DateTime? dateOfBirth;
  private string ssn;
  private string firstName;
  private string middleName;
  private string lastName;
  private string fipsCountyCode;
  private string familyViolence;
  private string previousSsn;
  private string cityOfBirth;
  private string stateOrCountryOfBirth;
  private string fathersFirstName;
  private string fathersMiddleInitial;
  private string fathersLastName;
  private string mothersFirstName;
  private string mothersMiddleInitial;
  private string mothersMaidenNm;
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
  private string errorCode2;
  private string errorCode3;
  private string errorCode4;
  private string errorCode5;
  private string additionalSsn1ValidityCode;
  private string additionalSsn2ValidityCode;
  private string bundleFplsLocateResults;
  private string ssaCityOfLastResidence;
  private string ssaStateOfLastResidence;
  private string ssaCityOfLumpSumPayment;
  private string ssaStateOfLumpSumPayment;
  private DateTime? lastSentDtToFcr;
  private DateTime? lastReceivedDtFromFcr;
  private string memberUserField;
}
