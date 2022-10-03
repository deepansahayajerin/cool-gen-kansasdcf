// The source file: EXTERNAL_FPLS_RESPONSE, ID: 372367167, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// Resp: OBLGEST	
/// This workset represents the EXTERNAL format for responses to be received 
/// from the Federal Person Locator Service (FPLS).
/// Please note that the external format calls for CASE ID in positions 187-201(
/// 15 positions), we actually sent the following information in this place --
/// AP CSE PERSON number    for 10
/// FPLS_REQUEST identifier for 5 (The Identifier is actually located in the 
/// last three positions)
/// </summary>
[Serializable]
public partial class ExternalFplsResponse: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ExternalFplsResponse()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ExternalFplsResponse(ExternalFplsResponse that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ExternalFplsResponse Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ExternalFplsResponse that)
  {
    base.Assign(that);
    dodDateOfDeathOrSeparation = that.dodDateOfDeathOrSeparation;
    dodEligibilityCode = that.dodEligibilityCode;
    stateAbbreviation = that.stateAbbreviation;
    stationNumber = that.stationNumber;
    agencyCode = that.agencyCode;
    nameSentInd = that.nameSentInd;
    apFirstName = that.apFirstName;
    apMiddleName = that.apMiddleName;
    apFirstLastName = that.apFirstLastName;
    apSecondLastName = that.apSecondLastName;
    apThirdLastName = that.apThirdLastName;
    apNameReturned = that.apNameReturned;
    ssnSubmitted = that.ssnSubmitted;
    apCsePersonNumber = that.apCsePersonNumber;
    fplsRequestIdentifier = that.fplsRequestIdentifier;
    usersField = that.usersField;
    localCode = that.localCode;
    typeOfCase = that.typeOfCase;
    addrDateFormatInd = that.addrDateFormatInd;
    dateOfAddress = that.dateOfAddress;
    responseCode = that.responseCode;
    addressFormatInd = that.addressFormatInd;
    returnedAddress = that.returnedAddress;
    dodStatus = that.dodStatus;
    dodServiceCode = that.dodServiceCode;
    dodPayGradeCode = that.dodPayGradeCode;
    dodAnnualSalary = that.dodAnnualSalary;
    dodDateOfBirth = that.dodDateOfBirth;
    submittingOffice = that.submittingOffice;
    sesaRespondingState = that.sesaRespondingState;
    sesaWageClaimInd = that.sesaWageClaimInd;
    sesaWageAmount = that.sesaWageAmount;
    irsNameControl = that.irsNameControl;
    cpSsn = that.cpSsn;
    irsTaxYear = that.irsTaxYear;
    nprcEmpdOrSepd = that.nprcEmpdOrSepd;
    ssaFederalOrMilitary = that.ssaFederalOrMilitary;
    ssaCorpDivision = that.ssaCorpDivision;
    mbrBenefitAmount = that.mbrBenefitAmount;
    mbrDateOfDeath = that.mbrDateOfDeath;
    vaBenefitCode = that.vaBenefitCode;
    vaDateOfDeath = that.vaDateOfDeath;
    vaAmtOfAwardEffectiveDate = that.vaAmtOfAwardEffectiveDate;
    vaAmountOfAward = that.vaAmountOfAward;
    vaSuspenseCode = that.vaSuspenseCode;
    vaIncarcerationCode = that.vaIncarcerationCode;
    vaRetirementPayCode = that.vaRetirementPayCode;
    addressIndType = that.addressIndType;
    healthInsBenefitIndicator = that.healthInsBenefitIndicator;
    employmentStatus = that.employmentStatus;
    employmentInd = that.employmentInd;
    dateOfHire = that.dateOfHire;
    reportingFedAgency = that.reportingFedAgency;
    fein = that.fein;
    correctedAdditionMultipleSsn = that.correctedAdditionMultipleSsn;
    ssnMatchInd = that.ssnMatchInd;
    reportingQuarter = that.reportingQuarter;
    ndnhResponse = that.ndnhResponse;
    nsaDateOfDeath = that.nsaDateOfDeath;
  }

  /// <summary>
  /// The value of the DOD_DATE_OF_DEATH_OR_SEPARATION attribute.
  /// </summary>
  [JsonPropertyName("dodDateOfDeathOrSeparation")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? DodDateOfDeathOrSeparation
  {
    get => dodDateOfDeathOrSeparation;
    set => dodDateOfDeathOrSeparation = value;
  }

  /// <summary>Length of the DOD_ELIGIBILITY_CODE attribute.</summary>
  public const int DodEligibilityCode_MaxLength = 1;

  /// <summary>
  /// The value of the DOD_ELIGIBILITY_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = DodEligibilityCode_MaxLength)]
  public string DodEligibilityCode
  {
    get => dodEligibilityCode ?? "";
    set => dodEligibilityCode =
      TrimEnd(Substring(value, 1, DodEligibilityCode_MaxLength));
  }

  /// <summary>
  /// The json value of the DodEligibilityCode attribute.</summary>
  [JsonPropertyName("dodEligibilityCode")]
  [Computed]
  public string DodEligibilityCode_Json
  {
    get => NullIf(DodEligibilityCode, "");
    set => DodEligibilityCode = value;
  }

  /// <summary>Length of the STATE_ABBREVIATION attribute.</summary>
  public const int StateAbbreviation_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_ABBREVIATION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = StateAbbreviation_MaxLength)]
  public string StateAbbreviation
  {
    get => stateAbbreviation ?? "";
    set => stateAbbreviation =
      TrimEnd(Substring(value, 1, StateAbbreviation_MaxLength));
  }

  /// <summary>
  /// The json value of the StateAbbreviation attribute.</summary>
  [JsonPropertyName("stateAbbreviation")]
  [Computed]
  public string StateAbbreviation_Json
  {
    get => NullIf(StateAbbreviation, "");
    set => StateAbbreviation = value;
  }

  /// <summary>Length of the STATION_NUMBER attribute.</summary>
  public const int StationNumber_MaxLength = 2;

  /// <summary>
  /// The value of the STATION_NUMBER attribute.
  /// Station No of the sending unit. Assigned by FPLS. Required for known and 
  /// unknown cases.
  /// Refer to FPLS I-O specification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = StationNumber_MaxLength)]
  public string StationNumber
  {
    get => stationNumber ?? "";
    set => stationNumber =
      TrimEnd(Substring(value, 1, StationNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the StationNumber attribute.</summary>
  [JsonPropertyName("stationNumber")]
  [Computed]
  public string StationNumber_Json
  {
    get => NullIf(StationNumber, "");
    set => StationNumber = value;
  }

  /// <summary>Length of the AGENCY_CODE attribute.</summary>
  public const int AgencyCode_MaxLength = 3;

  /// <summary>
  /// The value of the AGENCY_CODE attribute.
  /// Contains 3-char agency code providing the data.
  /// Valid agency codes are maintained in CODE_VALUE entity for CODE_NAME 
  /// FPLS_AGENCY_CODE.
  /// A01 - Response for Lnown request from DOD
  /// B## - Resp for known case from SESA state
  ///       where ## is the numeric FIPS state
  /// C01 - Resp for known case from IRS
  /// C02 - Resp for Unknown case from IRS for
  ///       SSN Id
  /// D01 - Resp for known case from NPRC
  /// E01 - Resp for known case from SSA for
  ///       employer and benefit information
  /// E02 - Resp for unknown case from SSA for
  ///       SSN Identification
  /// F01 - Resp for known case from VA
  /// G01 - Resp for known case from SSS
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = AgencyCode_MaxLength)]
  public string AgencyCode
  {
    get => agencyCode ?? "";
    set => agencyCode = TrimEnd(Substring(value, 1, AgencyCode_MaxLength));
  }

  /// <summary>
  /// The json value of the AgencyCode attribute.</summary>
  [JsonPropertyName("agencyCode")]
  [Computed]
  public string AgencyCode_Json
  {
    get => NullIf(AgencyCode, "");
    set => AgencyCode = value;
  }

  /// <summary>Length of the NAME_SENT_IND attribute.</summary>
  public const int NameSentInd_MaxLength = 1;

  /// <summary>
  /// The value of the NAME_SENT_IND attribute.
  /// Identifies the name  sent with the request to a particular agency. It 
  /// could be one of the following:
  /// 1 - AP last name was sent (AP FIRST LAST
  ///     NAME)
  /// 2 - Alias-1 was sent (AP SECOND LAST NAME)
  /// 3 - Alias-2 was sent (AP THIRD LAST NAME)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = NameSentInd_MaxLength)]
  public string NameSentInd
  {
    get => nameSentInd ?? "";
    set => nameSentInd = TrimEnd(Substring(value, 1, NameSentInd_MaxLength));
  }

  /// <summary>
  /// The json value of the NameSentInd attribute.</summary>
  [JsonPropertyName("nameSentInd")]
  [Computed]
  public string NameSentInd_Json
  {
    get => NullIf(NameSentInd, "");
    set => NameSentInd = value;
  }

  /// <summary>Length of the AP_FIRST_NAME attribute.</summary>
  public const int ApFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the AP_FIRST_NAME attribute.
  /// First Name of the Absent Parent. No special characters allowed.
  /// Required for known and unknown cases.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = ApFirstName_MaxLength)]
  public string ApFirstName
  {
    get => apFirstName ?? "";
    set => apFirstName = TrimEnd(Substring(value, 1, ApFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApFirstName attribute.</summary>
  [JsonPropertyName("apFirstName")]
  [Computed]
  public string ApFirstName_Json
  {
    get => NullIf(ApFirstName, "");
    set => ApFirstName = value;
  }

  /// <summary>Length of the AP_MIDDLE_NAME attribute.</summary>
  public const int ApMiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the AP_MIDDLE_NAME attribute.
  /// Middle Name of the Absent Parent.
  /// Optional field. No special characters allowed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ApMiddleName_MaxLength)]
  public string ApMiddleName
  {
    get => apMiddleName ?? "";
    set => apMiddleName = TrimEnd(Substring(value, 1, ApMiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApMiddleName attribute.</summary>
  [JsonPropertyName("apMiddleName")]
  [Computed]
  public string ApMiddleName_Json
  {
    get => NullIf(ApMiddleName, "");
    set => ApMiddleName = value;
  }

  /// <summary>Length of the AP_FIRST_LAST_NAME attribute.</summary>
  public const int ApFirstLastName_MaxLength = 20;

  /// <summary>
  /// The value of the AP_FIRST_LAST_NAME attribute.
  /// Last name of the absent parent.
  /// No special characters except hyphen. Required for known and  unknown 
  /// cases.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = ApFirstLastName_MaxLength)]
    
  public string ApFirstLastName
  {
    get => apFirstLastName ?? "";
    set => apFirstLastName =
      TrimEnd(Substring(value, 1, ApFirstLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApFirstLastName attribute.</summary>
  [JsonPropertyName("apFirstLastName")]
  [Computed]
  public string ApFirstLastName_Json
  {
    get => NullIf(ApFirstLastName, "");
    set => ApFirstLastName = value;
  }

  /// <summary>Length of the AP_SECOND_LAST_NAME attribute.</summary>
  public const int ApSecondLastName_MaxLength = 20;

  /// <summary>
  /// The value of the AP_SECOND_LAST_NAME attribute.
  /// Alias - 1 of the absent parent
  /// No special characters allowed except hyphen. Optional field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = ApSecondLastName_MaxLength)]
  public string ApSecondLastName
  {
    get => apSecondLastName ?? "";
    set => apSecondLastName =
      TrimEnd(Substring(value, 1, ApSecondLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApSecondLastName attribute.</summary>
  [JsonPropertyName("apSecondLastName")]
  [Computed]
  public string ApSecondLastName_Json
  {
    get => NullIf(ApSecondLastName, "");
    set => ApSecondLastName = value;
  }

  /// <summary>Length of the AP_THIRD_LAST_NAME attribute.</summary>
  public const int ApThirdLastName_MaxLength = 20;

  /// <summary>
  /// The value of the AP_THIRD_LAST_NAME attribute.
  /// Alias - 2 of the absent parent.
  /// No special character allowed except hyphen. Optional field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = ApThirdLastName_MaxLength)
    ]
  public string ApThirdLastName
  {
    get => apThirdLastName ?? "";
    set => apThirdLastName =
      TrimEnd(Substring(value, 1, ApThirdLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApThirdLastName attribute.</summary>
  [JsonPropertyName("apThirdLastName")]
  [Computed]
  public string ApThirdLastName_Json
  {
    get => NullIf(ApThirdLastName, "");
    set => ApThirdLastName = value;
  }

  /// <summary>Length of the AP_NAME_RETURNED attribute.</summary>
  public const int ApNameReturned_MaxLength = 62;

  /// <summary>
  /// The value of the AP_NAME_RETURNED attribute.
  /// AP's name as returned by the agencies. This name may contain employer's 
  /// name. (Not all agencies return one).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = ApNameReturned_MaxLength)]
    
  public string ApNameReturned
  {
    get => apNameReturned ?? "";
    set => apNameReturned =
      TrimEnd(Substring(value, 1, ApNameReturned_MaxLength));
  }

  /// <summary>
  /// The json value of the ApNameReturned attribute.</summary>
  [JsonPropertyName("apNameReturned")]
  [Computed]
  public string ApNameReturned_Json
  {
    get => NullIf(ApNameReturned, "");
    set => ApNameReturned = value;
  }

  /// <summary>Length of the SSN_SUBMITTED attribute.</summary>
  public const int SsnSubmitted_MaxLength = 9;

  /// <summary>
  /// The value of the SSN_SUBMITTED attribute.
  /// This attribute specifies the Social Security Number as located by FPLS.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = SsnSubmitted_MaxLength)]
  public string SsnSubmitted
  {
    get => ssnSubmitted ?? "";
    set => ssnSubmitted = TrimEnd(Substring(value, 1, SsnSubmitted_MaxLength));
  }

  /// <summary>
  /// The json value of the SsnSubmitted attribute.</summary>
  [JsonPropertyName("ssnSubmitted")]
  [Computed]
  public string SsnSubmitted_Json
  {
    get => NullIf(SsnSubmitted, "");
    set => SsnSubmitted = value;
  }

  /// <summary>Length of the AP_CSE_PERSON_NUMBER attribute.</summary>
  public const int ApCsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the AP_CSE_PERSON_NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = ApCsePersonNumber_MaxLength)]
  public string ApCsePersonNumber
  {
    get => apCsePersonNumber ?? "";
    set => apCsePersonNumber =
      TrimEnd(Substring(value, 1, ApCsePersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the ApCsePersonNumber attribute.</summary>
  [JsonPropertyName("apCsePersonNumber")]
  [Computed]
  public string ApCsePersonNumber_Json
  {
    get => NullIf(ApCsePersonNumber, "");
    set => ApCsePersonNumber = value;
  }

  /// <summary>
  /// The value of the FPLS_REQUEST_IDENTIFIER attribute.
  /// The attribute, which together with the relationship with CSE_PERSON, 
  /// identifies uniquely an FPLS transaction.
  /// </summary>
  [JsonPropertyName("fplsRequestIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 5)]
  public int FplsRequestIdentifier
  {
    get => fplsRequestIdentifier;
    set => fplsRequestIdentifier = value;
  }

  /// <summary>Length of the USERS_FIELD attribute.</summary>
  public const int UsersField_MaxLength = 7;

  /// <summary>
  /// The value of the USERS_FIELD attribute.
  /// User's field. Currently Kansas moves 2-digit region code + 1 char Team 
  /// code to this attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = UsersField_MaxLength)]
  public string UsersField
  {
    get => usersField ?? "";
    set => usersField = TrimEnd(Substring(value, 1, UsersField_MaxLength));
  }

  /// <summary>
  /// The json value of the UsersField attribute.</summary>
  [JsonPropertyName("usersField")]
  [Computed]
  public string UsersField_Json
  {
    get => NullIf(UsersField, "");
    set => UsersField = value;
  }

  /// <summary>Length of the LOCAL_CODE attribute.</summary>
  public const int LocalCode_MaxLength = 3;

  /// <summary>
  /// The value of the LOCAL_CODE attribute.
  /// Expected to contain 3-digit county code. It is optional field. Must be 
  /// blank or a valid numeric value.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = LocalCode_MaxLength)]
  public string LocalCode
  {
    get => localCode ?? "";
    set => localCode = TrimEnd(Substring(value, 1, LocalCode_MaxLength));
  }

  /// <summary>
  /// The json value of the LocalCode attribute.</summary>
  [JsonPropertyName("localCode")]
  [Computed]
  public string LocalCode_Json
  {
    get => NullIf(LocalCode, "");
    set => LocalCode = value;
  }

  /// <summary>Length of the TYPE_OF_CASE attribute.</summary>
  public const int TypeOfCase_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE_OF_CASE attribute.
  /// Described type of case.
  /// Required for known and unknown cases.
  /// A - AFDC/Foster Care
  /// N - Non AFDC full service
  /// L - Non AFDC Locate only
  /// P - Parental kidnapping
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = TypeOfCase_MaxLength)]
  public string TypeOfCase
  {
    get => typeOfCase ?? "";
    set => typeOfCase = TrimEnd(Substring(value, 1, TypeOfCase_MaxLength));
  }

  /// <summary>
  /// The json value of the TypeOfCase attribute.</summary>
  [JsonPropertyName("typeOfCase")]
  [Computed]
  public string TypeOfCase_Json
  {
    get => NullIf(TypeOfCase, "");
    set => TypeOfCase = value;
  }

  /// <summary>Length of the ADDR_DATE_FORMAT_IND attribute.</summary>
  public const int AddrDateFormatInd_MaxLength = 1;

  /// <summary>
  /// The value of the ADDR_DATE_FORMAT_IND attribute.
  /// Indicates the format in which the address is returned in RETURNED_ADDRESS 
  /// field.
  /// C - City, State and ZIP break down.
  /// F - Free format (lines separated by \) and with an isolated ZIP code when 
  /// possible.
  /// Blank - No address
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = AddrDateFormatInd_MaxLength)]
  public string AddrDateFormatInd
  {
    get => addrDateFormatInd ?? "";
    set => addrDateFormatInd =
      TrimEnd(Substring(value, 1, AddrDateFormatInd_MaxLength));
  }

  /// <summary>
  /// The json value of the AddrDateFormatInd attribute.</summary>
  [JsonPropertyName("addrDateFormatInd")]
  [Computed]
  public string AddrDateFormatInd_Json
  {
    get => NullIf(AddrDateFormatInd, "");
    set => AddrDateFormatInd = value;
  }

  /// <summary>
  /// The value of the DATE_OF_ADDRESS attribute.
  /// </summary>
  [JsonPropertyName("dateOfAddress")]
  [Member(Index = 20, Type = MemberType.Date)]
  public DateTime? DateOfAddress
  {
    get => dateOfAddress;
    set => dateOfAddress = value;
  }

  /// <summary>Length of the RESPONSE_CODE attribute.</summary>
  public const int ResponseCode_MaxLength = 2;

  /// <summary>
  /// The value of the RESPONSE_CODE attribute.
  /// The code returned by the individual agencies to further clarify the 
  /// response.
  /// The valid values and descriptions are maintained in CODE_VALUE entity for 
  /// CODE_NAME FPLS_RESPONSE_CODE
  /// e.g.
  /// Blank	- Address returned to state
  /// 01	- Unable to send case to agency
  /// 02	- Beneficiary is deceased
  /// ...
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length = ResponseCode_MaxLength)]
  public string ResponseCode
  {
    get => responseCode ?? "";
    set => responseCode = TrimEnd(Substring(value, 1, ResponseCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ResponseCode attribute.</summary>
  [JsonPropertyName("responseCode")]
  [Computed]
  public string ResponseCode_Json
  {
    get => NullIf(ResponseCode, "");
    set => ResponseCode = value;
  }

  /// <summary>Length of the ADDRESS_FORMAT_IND attribute.</summary>
  public const int AddressFormatInd_MaxLength = 1;

  /// <summary>
  /// The value of the ADDRESS_FORMAT_IND attribute.
  /// The date of the address provided by the agency
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = AddressFormatInd_MaxLength)]
  public string AddressFormatInd
  {
    get => addressFormatInd ?? "";
    set => addressFormatInd =
      TrimEnd(Substring(value, 1, AddressFormatInd_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressFormatInd attribute.</summary>
  [JsonPropertyName("addressFormatInd")]
  [Computed]
  public string AddressFormatInd_Json
  {
    get => NullIf(AddressFormatInd, "");
    set => AddressFormatInd = value;
  }

  /// <summary>Length of the RETURNED_ADDRESS attribute.</summary>
  public const int ReturnedAddress_MaxLength = 234;

  /// <summary>
  /// The value of the RETURNED_ADDRESS attribute.
  /// Address as returned by the agency in one of the formats specified by the 
  /// address format indicator.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length = ReturnedAddress_MaxLength)
    ]
  public string ReturnedAddress
  {
    get => returnedAddress ?? "";
    set => returnedAddress =
      TrimEnd(Substring(value, 1, ReturnedAddress_MaxLength));
  }

  /// <summary>
  /// The json value of the ReturnedAddress attribute.</summary>
  [JsonPropertyName("returnedAddress")]
  [Computed]
  public string ReturnedAddress_Json
  {
    get => NullIf(ReturnedAddress, "");
    set => ReturnedAddress = value;
  }

  /// <summary>Length of the DOD_STATUS attribute.</summary>
  public const int DodStatus_MaxLength = 1;

  /// <summary>
  /// The value of the DOD_STATUS attribute.
  /// Returned by Department of Defense (D.O.D)
  /// The valid values and descriptions are maintained in CODE_VALUE entity for 
  /// CODE_NAME FPLS_DOD_STATUS.
  /// e.g.
  /// A - Active duty
  /// B - Recalled to Active duty
  /// C - Civilian
  /// D - 100% disabled
  /// E - New enlistee
  /// F - Former member
  /// H - medal of Honor
  /// J - Academy student
  /// L - Lighthouse service
  /// M - Red Cross
  /// N - Active NATL. Guard
  /// R - Retired
  /// S - Service Secretary
  /// T - Foreign Military
  /// U - Foreign National
  /// V - Reserve on Active duty
  /// Z - Unknown
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length = DodStatus_MaxLength)]
  public string DodStatus
  {
    get => dodStatus ?? "";
    set => dodStatus = TrimEnd(Substring(value, 1, DodStatus_MaxLength));
  }

  /// <summary>
  /// The json value of the DodStatus attribute.</summary>
  [JsonPropertyName("dodStatus")]
  [Computed]
  public string DodStatus_Json
  {
    get => NullIf(DodStatus, "");
    set => DodStatus = value;
  }

  /// <summary>Length of the DOD_SERVICE_CODE attribute.</summary>
  public const int DodServiceCode_MaxLength = 4;

  /// <summary>
  /// The value of the DOD_SERVICE_CODE attribute.
  /// Returned by Department of Defense (D.O.D)
  /// Valid values and descriptions are maintained in CODE_VALUE entity for 
  /// CODE_NAME FPLS_DOD_SERVICE_CODE.
  /// e.g.
  /// A - Army
  /// N - Navy
  /// F - Air Force
  /// M - Marine Corps
  /// P - Coast guard
  /// E - Public Health Service
  /// I - National Oceanic and Atmospheric Admin
  /// X - Other
  /// Z - Unknown
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = DodServiceCode_MaxLength)]
    
  public string DodServiceCode
  {
    get => dodServiceCode ?? "";
    set => dodServiceCode =
      TrimEnd(Substring(value, 1, DodServiceCode_MaxLength));
  }

  /// <summary>
  /// The json value of the DodServiceCode attribute.</summary>
  [JsonPropertyName("dodServiceCode")]
  [Computed]
  public string DodServiceCode_Json
  {
    get => NullIf(DodServiceCode, "");
    set => DodServiceCode = value;
  }

  /// <summary>Length of the DOD_PAY_GRADE_CODE attribute.</summary>
  public const int DodPayGradeCode_MaxLength = 4;

  /// <summary>
  /// The value of the DOD_PAY_GRADE_CODE attribute.
  /// Returned by Department of Defense (D.O.D)
  /// Describes Pay grade.
  /// Valid values are maintained in CODE_VALUE entity for CODE_NAME 
  /// FPLS_DOD_PAY_GRADE_CODE
  /// e.g.
  /// 0	- Recent enlistee (pay grade unknown)
  /// 1-9	- Recent enlistee (pay grade E1 - E9)
  /// 10	- warrant officer (pay grade unknown)
  /// 11-14	- Warrant officer (pay grade W1 - W4)
  /// 19	- Academy
  /// 20	- Officer (pay grade unknown)
  /// 21-31	- Officer pay grade 01-11
  /// 40	- Civil servant pay gr unknown
  /// 41-58	- Civil servant pay gr GS-1 to GS-18
  /// 90-99	- Unknown
  /// 95	- Not applicable
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length = DodPayGradeCode_MaxLength)
    ]
  public string DodPayGradeCode
  {
    get => dodPayGradeCode ?? "";
    set => dodPayGradeCode =
      TrimEnd(Substring(value, 1, DodPayGradeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the DodPayGradeCode attribute.</summary>
  [JsonPropertyName("dodPayGradeCode")]
  [Computed]
  public string DodPayGradeCode_Json
  {
    get => NullIf(DodPayGradeCode, "");
    set => DodPayGradeCode = value;
  }

  /// <summary>
  /// The value of the DOD_ANNUAL_SALARY attribute.
  /// </summary>
  [JsonPropertyName("dodAnnualSalary")]
  [DefaultValue(0)]
  [Member(Index = 27, Type = MemberType.Number, Length = 6)]
  public int DodAnnualSalary
  {
    get => dodAnnualSalary;
    set => dodAnnualSalary = value;
  }

  /// <summary>
  /// The value of the DOD_DATE_OF_BIRTH attribute.
  /// </summary>
  [JsonPropertyName("dodDateOfBirth")]
  [Member(Index = 28, Type = MemberType.Date)]
  public DateTime? DodDateOfBirth
  {
    get => dodDateOfBirth;
    set => dodDateOfBirth = value;
  }

  /// <summary>Length of the SUBMITTING_OFFICE attribute.</summary>
  public const int SubmittingOffice_MaxLength = 4;

  /// <summary>
  /// The value of the SUBMITTING_OFFICE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = SubmittingOffice_MaxLength)]
  public string SubmittingOffice
  {
    get => submittingOffice ?? "";
    set => submittingOffice =
      TrimEnd(Substring(value, 1, SubmittingOffice_MaxLength));
  }

  /// <summary>
  /// The json value of the SubmittingOffice attribute.</summary>
  [JsonPropertyName("submittingOffice")]
  [Computed]
  public string SubmittingOffice_Json
  {
    get => NullIf(SubmittingOffice, "");
    set => SubmittingOffice = value;
  }

  /// <summary>Length of the SESA_RESPONDING_STATE attribute.</summary>
  public const int SesaRespondingState_MaxLength = 2;

  /// <summary>
  /// The value of the SESA_RESPONDING_STATE attribute.
  /// Numeric FIPS code for the state for the responding SESA. This may or may 
  /// not be the same as the requesting FIPS code in the AGENCY_CODE field.
  /// The valid values and descriptions are maintained in CODE_VALUE entity for 
  /// CODE_NAME FPLS_STATE_FIPS_CODE
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = SesaRespondingState_MaxLength)]
  public string SesaRespondingState
  {
    get => sesaRespondingState ?? "";
    set => sesaRespondingState =
      TrimEnd(Substring(value, 1, SesaRespondingState_MaxLength));
  }

  /// <summary>
  /// The json value of the SesaRespondingState attribute.</summary>
  [JsonPropertyName("sesaRespondingState")]
  [Computed]
  public string SesaRespondingState_Json
  {
    get => NullIf(SesaRespondingState, "");
    set => SesaRespondingState = value;
  }

  /// <summary>Length of the SESA_WAGE_CLAIM_IND attribute.</summary>
  public const int SesaWageClaimInd_MaxLength = 1;

  /// <summary>
  /// The value of the SESA_WAGE_CLAIM_IND attribute.
  /// Returned by SESA
  /// Specifies whether Wage/Claimant information is returned.
  /// 1 - SESA Wage information is returned. (See SESA WAGE AMOUNT field)
  /// 2 - SESA Claimant information or Unemployment Insurance benefit
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = SesaWageClaimInd_MaxLength)]
  public string SesaWageClaimInd
  {
    get => sesaWageClaimInd ?? "";
    set => sesaWageClaimInd =
      TrimEnd(Substring(value, 1, SesaWageClaimInd_MaxLength));
  }

  /// <summary>
  /// The json value of the SesaWageClaimInd attribute.</summary>
  [JsonPropertyName("sesaWageClaimInd")]
  [Computed]
  public string SesaWageClaimInd_Json
  {
    get => NullIf(SesaWageClaimInd, "");
    set => SesaWageClaimInd = value;
  }

  /// <summary>
  /// The value of the SESA_WAGE_AMOUNT attribute.
  /// Returned by SESA
  /// Amount is always positive and will be in dollars when available; otherwise
  /// it will contain zeros.
  /// </summary>
  [JsonPropertyName("sesaWageAmount")]
  [DefaultValue(0)]
  [Member(Index = 32, Type = MemberType.Number, Length = 6)]
  public int SesaWageAmount
  {
    get => sesaWageAmount;
    set => sesaWageAmount = value;
  }

  /// <summary>Length of the IRS_NAME_CONTROL attribute.</summary>
  public const int IrsNameControl_MaxLength = 6;

  /// <summary>
  /// The value of the IRS_NAME_CONTROL attribute.
  /// Returned by IRS
  /// Name control as returned by IRS.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length = IrsNameControl_MaxLength)]
    
  public string IrsNameControl
  {
    get => irsNameControl ?? "";
    set => irsNameControl =
      TrimEnd(Substring(value, 1, IrsNameControl_MaxLength));
  }

  /// <summary>
  /// The json value of the IrsNameControl attribute.</summary>
  [JsonPropertyName("irsNameControl")]
  [Computed]
  public string IrsNameControl_Json
  {
    get => NullIf(IrsNameControl, "");
    set => IrsNameControl = value;
  }

  /// <summary>Length of the CP_SSN attribute.</summary>
  public const int CpSsn_MaxLength = 9;

  /// <summary>
  /// The value of the CP_SSN attribute.
  /// Custodial Parent's SSN.
  /// First 3-digits must be one within the following ranges: 001-649 or 700-
  /// 728. This field is required for unknown case to go to IRS for an SSN
  /// search.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length = CpSsn_MaxLength)]
  public string CpSsn
  {
    get => cpSsn ?? "";
    set => cpSsn = TrimEnd(Substring(value, 1, CpSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the CpSsn attribute.</summary>
  [JsonPropertyName("cpSsn")]
  [Computed]
  public string CpSsn_Json
  {
    get => NullIf(CpSsn, "");
    set => CpSsn = value;
  }

  /// <summary>Length of the IRS_TAX_YEAR attribute.</summary>
  public const int IrsTaxYear_MaxLength = 4;

  /// <summary>
  /// The value of the IRS_TAX_YEAR attribute.
  /// Returned by IRS
  /// The FPLS flat file contains year in YY format only.
  /// Tax year of filed tax return
  /// Tax year can be zeros.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length = IrsTaxYear_MaxLength)]
  public string IrsTaxYear
  {
    get => irsTaxYear ?? "";
    set => irsTaxYear = TrimEnd(Substring(value, 1, IrsTaxYear_MaxLength));
  }

  /// <summary>
  /// The json value of the IrsTaxYear attribute.</summary>
  [JsonPropertyName("irsTaxYear")]
  [Computed]
  public string IrsTaxYear_Json
  {
    get => NullIf(IrsTaxYear, "");
    set => IrsTaxYear = value;
  }

  /// <summary>Length of the NPRC_EMPD_OR_SEPD attribute.</summary>
  public const int NprcEmpdOrSepd_MaxLength = 1;

  /// <summary>
  /// The value of the NPRC_EMPD_OR_SEPD attribute.
  /// Returned by National Personnel Records Center (NPRC)
  /// E - Employed
  /// S - Separated
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length = NprcEmpdOrSepd_MaxLength)]
    
  public string NprcEmpdOrSepd
  {
    get => nprcEmpdOrSepd ?? "";
    set => nprcEmpdOrSepd =
      TrimEnd(Substring(value, 1, NprcEmpdOrSepd_MaxLength));
  }

  /// <summary>
  /// The json value of the NprcEmpdOrSepd attribute.</summary>
  [JsonPropertyName("nprcEmpdOrSepd")]
  [Computed]
  public string NprcEmpdOrSepd_Json
  {
    get => NullIf(NprcEmpdOrSepd, "");
    set => NprcEmpdOrSepd = value;
  }

  /// <summary>Length of the SSA_FEDERAL_OR_MILITARY attribute.</summary>
  public const int SsaFederalOrMilitary_MaxLength = 1;

  /// <summary>
  /// The value of the SSA_FEDERAL_OR_MILITARY attribute.
  /// Returned by Social Security Administration (SSA)
  /// F - Federal; Case is automatically sent to NPRC if not already requested 
  /// by State.
  /// M - Military; case is automatically sent to D.O.D. if not already 
  /// requested by state. Observe RESPONSE RETURN CODE 36 and 38.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = SsaFederalOrMilitary_MaxLength)]
  public string SsaFederalOrMilitary
  {
    get => ssaFederalOrMilitary ?? "";
    set => ssaFederalOrMilitary =
      TrimEnd(Substring(value, 1, SsaFederalOrMilitary_MaxLength));
  }

  /// <summary>
  /// The json value of the SsaFederalOrMilitary attribute.</summary>
  [JsonPropertyName("ssaFederalOrMilitary")]
  [Computed]
  public string SsaFederalOrMilitary_Json
  {
    get => NullIf(SsaFederalOrMilitary, "");
    set => SsaFederalOrMilitary = value;
  }

  /// <summary>Length of the SSA_CORP_DIVISION attribute.</summary>
  public const int SsaCorpDivision_MaxLength = 4;

  /// <summary>
  /// The value of the SSA_CORP_DIVISION attribute.
  /// Returned Bby SSA	
  /// Employee's Corporate Division (SSA)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length = SsaCorpDivision_MaxLength)
    ]
  public string SsaCorpDivision
  {
    get => ssaCorpDivision ?? "";
    set => ssaCorpDivision =
      TrimEnd(Substring(value, 1, SsaCorpDivision_MaxLength));
  }

  /// <summary>
  /// The json value of the SsaCorpDivision attribute.</summary>
  [JsonPropertyName("ssaCorpDivision")]
  [Computed]
  public string SsaCorpDivision_Json
  {
    get => NullIf(SsaCorpDivision, "");
    set => SsaCorpDivision = value;
  }

  /// <summary>
  /// The value of the MBR_BENEFIT_AMOUNT attribute.
  /// Returned by Member Beneficiary Record (MBR)
  /// Amount is always positive and will be in dollars when available. Otherwise
  /// zeros.
  /// </summary>
  [JsonPropertyName("mbrBenefitAmount")]
  [DefaultValue(0)]
  [Member(Index = 39, Type = MemberType.Number, Length = 6)]
  public int MbrBenefitAmount
  {
    get => mbrBenefitAmount;
    set => mbrBenefitAmount = value;
  }

  /// <summary>
  /// The value of the MBR_DATE_OF_DEATH attribute.
  /// Returned by Master Beneficiary Record (MBR)
  /// Specifies Date of Death.
  /// FPLS flat file contains date in MMDDYY format.
  /// </summary>
  [JsonPropertyName("mbrDateOfDeath")]
  [Member(Index = 40, Type = MemberType.Date)]
  public DateTime? MbrDateOfDeath
  {
    get => mbrDateOfDeath;
    set => mbrDateOfDeath = value;
  }

  /// <summary>Length of the VA_BENEFIT_CODE attribute.</summary>
  public const int VaBenefitCode_MaxLength = 1;

  /// <summary>
  /// The value of the VA_BENEFIT_CODE attribute.
  /// Returned by Veterans Administration (VA)
  /// Specifies type of benefit.
  /// 1 - Compensation and Pension
  /// 2 - Education
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length = VaBenefitCode_MaxLength)]
  public string VaBenefitCode
  {
    get => vaBenefitCode ?? "";
    set => vaBenefitCode =
      TrimEnd(Substring(value, 1, VaBenefitCode_MaxLength));
  }

  /// <summary>
  /// The json value of the VaBenefitCode attribute.</summary>
  [JsonPropertyName("vaBenefitCode")]
  [Computed]
  public string VaBenefitCode_Json
  {
    get => NullIf(VaBenefitCode, "");
    set => VaBenefitCode = value;
  }

  /// <summary>
  /// The value of the VA_DATE_OF_DEATH attribute.
  /// Returned by Veterans Administration (VA)
  /// Specifies Date of Death
  /// FPLS flat file contains date in MMDDYY format
  /// </summary>
  [JsonPropertyName("vaDateOfDeath")]
  [Member(Index = 42, Type = MemberType.Date)]
  public DateTime? VaDateOfDeath
  {
    get => vaDateOfDeath;
    set => vaDateOfDeath = value;
  }

  /// <summary>
  /// The value of the VA_AMT_OF_AWARD_EFFECTIVE_DATE attribute.
  /// Returned by Veterans Administration
  /// Specifies Effective Date of Amount of Award. Refer to field 
  /// VA_AMOUNT_OF_AWARD
  /// FPLS flat file contains date in MMDDYY format.
  /// </summary>
  [JsonPropertyName("vaAmtOfAwardEffectiveDate")]
  [Member(Index = 43, Type = MemberType.Date)]
  public DateTime? VaAmtOfAwardEffectiveDate
  {
    get => vaAmtOfAwardEffectiveDate;
    set => vaAmtOfAwardEffectiveDate = value;
  }

  /// <summary>
  /// The value of the VA_AMOUNT_OF_AWARD attribute.
  /// Returned by Veterans Administration (VA)
  /// Amount is always positive and will be in dollars when available. Default 
  /// is 0.
  /// </summary>
  [JsonPropertyName("vaAmountOfAward")]
  [DefaultValue(0)]
  [Member(Index = 44, Type = MemberType.Number, Length = 6)]
  public int VaAmountOfAward
  {
    get => vaAmountOfAward;
    set => vaAmountOfAward = value;
  }

  /// <summary>Length of the VA_SUSPENSE_CODE attribute.</summary>
  public const int VaSuspenseCode_MaxLength = 1;

  /// <summary>
  /// The value of the VA_SUSPENSE_CODE attribute.
  /// Returned by Veterans Adinistration (VA)
  /// 0 - Receiving Payments
  /// 1 - Payments have been temporarily stopped
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length = VaSuspenseCode_MaxLength)]
    
  public string VaSuspenseCode
  {
    get => vaSuspenseCode ?? "";
    set => vaSuspenseCode =
      TrimEnd(Substring(value, 1, VaSuspenseCode_MaxLength));
  }

  /// <summary>
  /// The json value of the VaSuspenseCode attribute.</summary>
  [JsonPropertyName("vaSuspenseCode")]
  [Computed]
  public string VaSuspenseCode_Json
  {
    get => NullIf(VaSuspenseCode, "");
    set => VaSuspenseCode = value;
  }

  /// <summary>Length of the VA_INCARCERATION_CODE attribute.</summary>
  public const int VaIncarcerationCode_MaxLength = 1;

  /// <summary>
  /// The value of the VA_INCARCERATION_CODE attribute.
  /// Returned by Veterans Administration (VA)
  /// Specifies Incarceration.
  /// 0 - Released from Jail
  /// 1 - Currently in Jail
  /// Valid values and descriptions are maintained in CODE_VALUE entity for 
  /// CODE_NAME FPLS_VA_INCARERATION_CODE
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = VaIncarcerationCode_MaxLength)]
  public string VaIncarcerationCode
  {
    get => vaIncarcerationCode ?? "";
    set => vaIncarcerationCode =
      TrimEnd(Substring(value, 1, VaIncarcerationCode_MaxLength));
  }

  /// <summary>
  /// The json value of the VaIncarcerationCode attribute.</summary>
  [JsonPropertyName("vaIncarcerationCode")]
  [Computed]
  public string VaIncarcerationCode_Json
  {
    get => NullIf(VaIncarcerationCode, "");
    set => VaIncarcerationCode = value;
  }

  /// <summary>Length of the VA_RETIREMENT_PAY_CODE attribute.</summary>
  public const int VaRetirementPayCode_MaxLength = 1;

  /// <summary>
  /// The value of the VA_RETIREMENT_PAY_CODE attribute.
  /// Returned by Veterans Administration (VA)
  /// Specifies whether or not eligible to receive retirement pay.
  /// 0 - Not eligible to receive retirement pay
  /// 1 - Eligible or is receiving retirement pay
  /// Valid values and descriptions are maintained in 
  /// FPLS_VA_RETIREMENT_PAY_CODE
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length
    = VaRetirementPayCode_MaxLength)]
  public string VaRetirementPayCode
  {
    get => vaRetirementPayCode ?? "";
    set => vaRetirementPayCode =
      TrimEnd(Substring(value, 1, VaRetirementPayCode_MaxLength));
  }

  /// <summary>
  /// The json value of the VaRetirementPayCode attribute.</summary>
  [JsonPropertyName("vaRetirementPayCode")]
  [Computed]
  public string VaRetirementPayCode_Json
  {
    get => NullIf(VaRetirementPayCode, "");
    set => VaRetirementPayCode = value;
  }

  /// <summary>Length of the ADDRESS_IND_TYPE attribute.</summary>
  public const int AddressIndType_MaxLength = 1;

  /// <summary>
  /// The value of the ADDRESS_IND_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 48, Type = MemberType.Char, Length = AddressIndType_MaxLength)]
    
  public string AddressIndType
  {
    get => addressIndType ?? "";
    set => addressIndType =
      TrimEnd(Substring(value, 1, AddressIndType_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressIndType attribute.</summary>
  [JsonPropertyName("addressIndType")]
  [Computed]
  public string AddressIndType_Json
  {
    get => NullIf(AddressIndType, "");
    set => AddressIndType = value;
  }

  /// <summary>Length of the HEALTH_INS_BENEFIT_INDICATOR attribute.</summary>
  public const int HealthInsBenefitIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the HEALTH_INS_BENEFIT_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 49, Type = MemberType.Char, Length
    = HealthInsBenefitIndicator_MaxLength)]
  public string HealthInsBenefitIndicator
  {
    get => healthInsBenefitIndicator ?? "";
    set => healthInsBenefitIndicator =
      TrimEnd(Substring(value, 1, HealthInsBenefitIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the HealthInsBenefitIndicator attribute.</summary>
  [JsonPropertyName("healthInsBenefitIndicator")]
  [Computed]
  public string HealthInsBenefitIndicator_Json
  {
    get => NullIf(HealthInsBenefitIndicator, "");
    set => HealthInsBenefitIndicator = value;
  }

  /// <summary>Length of the EMPLOYMENT_STATUS attribute.</summary>
  public const int EmploymentStatus_MaxLength = 1;

  /// <summary>
  /// The value of the EMPLOYMENT_STATUS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = EmploymentStatus_MaxLength)]
  public string EmploymentStatus
  {
    get => employmentStatus ?? "";
    set => employmentStatus =
      TrimEnd(Substring(value, 1, EmploymentStatus_MaxLength));
  }

  /// <summary>
  /// The json value of the EmploymentStatus attribute.</summary>
  [JsonPropertyName("employmentStatus")]
  [Computed]
  public string EmploymentStatus_Json
  {
    get => NullIf(EmploymentStatus, "");
    set => EmploymentStatus = value;
  }

  /// <summary>Length of the EMPLOYMENT_IND attribute.</summary>
  public const int EmploymentInd_MaxLength = 1;

  /// <summary>
  /// The value of the EMPLOYMENT_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 51, Type = MemberType.Char, Length = EmploymentInd_MaxLength)]
  public string EmploymentInd
  {
    get => employmentInd ?? "";
    set => employmentInd =
      TrimEnd(Substring(value, 1, EmploymentInd_MaxLength));
  }

  /// <summary>
  /// The json value of the EmploymentInd attribute.</summary>
  [JsonPropertyName("employmentInd")]
  [Computed]
  public string EmploymentInd_Json
  {
    get => NullIf(EmploymentInd, "");
    set => EmploymentInd = value;
  }

  /// <summary>
  /// The value of the DATE_OF_HIRE attribute.
  /// </summary>
  [JsonPropertyName("dateOfHire")]
  [Member(Index = 52, Type = MemberType.Date)]
  public DateTime? DateOfHire
  {
    get => dateOfHire;
    set => dateOfHire = value;
  }

  /// <summary>Length of the REPORTING_FED_AGENCY attribute.</summary>
  public const int ReportingFedAgency_MaxLength = 9;

  /// <summary>
  /// The value of the REPORTING_FED_AGENCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 53, Type = MemberType.Char, Length
    = ReportingFedAgency_MaxLength)]
  public string ReportingFedAgency
  {
    get => reportingFedAgency ?? "";
    set => reportingFedAgency =
      TrimEnd(Substring(value, 1, ReportingFedAgency_MaxLength));
  }

  /// <summary>
  /// The json value of the ReportingFedAgency attribute.</summary>
  [JsonPropertyName("reportingFedAgency")]
  [Computed]
  public string ReportingFedAgency_Json
  {
    get => NullIf(ReportingFedAgency, "");
    set => ReportingFedAgency = value;
  }

  /// <summary>Length of the FEIN attribute.</summary>
  public const int Fein_MaxLength = 9;

  /// <summary>
  /// The value of the FEIN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 54, Type = MemberType.Char, Length = Fein_MaxLength)]
  public string Fein
  {
    get => fein ?? "";
    set => fein = TrimEnd(Substring(value, 1, Fein_MaxLength));
  }

  /// <summary>
  /// The json value of the Fein attribute.</summary>
  [JsonPropertyName("fein")]
  [Computed]
  public string Fein_Json
  {
    get => NullIf(Fein, "");
    set => Fein = value;
  }

  /// <summary>Length of the CORRECTED_ADDITION_MULTIPLE_SSN attribute.
  /// </summary>
  public const int CorrectedAdditionMultipleSsn_MaxLength = 9;

  /// <summary>
  /// The value of the CORRECTED_ADDITION_MULTIPLE_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 55, Type = MemberType.Char, Length
    = CorrectedAdditionMultipleSsn_MaxLength)]
  public string CorrectedAdditionMultipleSsn
  {
    get => correctedAdditionMultipleSsn ?? "";
    set => correctedAdditionMultipleSsn =
      TrimEnd(Substring(value, 1, CorrectedAdditionMultipleSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the CorrectedAdditionMultipleSsn attribute.</summary>
  [JsonPropertyName("correctedAdditionMultipleSsn")]
  [Computed]
  public string CorrectedAdditionMultipleSsn_Json
  {
    get => NullIf(CorrectedAdditionMultipleSsn, "");
    set => CorrectedAdditionMultipleSsn = value;
  }

  /// <summary>Length of the SSN_MATCH_IND attribute.</summary>
  public const int SsnMatchInd_MaxLength = 1;

  /// <summary>
  /// The value of the SSN_MATCH_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 56, Type = MemberType.Char, Length = SsnMatchInd_MaxLength)]
  public string SsnMatchInd
  {
    get => ssnMatchInd ?? "";
    set => ssnMatchInd = TrimEnd(Substring(value, 1, SsnMatchInd_MaxLength));
  }

  /// <summary>
  /// The json value of the SsnMatchInd attribute.</summary>
  [JsonPropertyName("ssnMatchInd")]
  [Computed]
  public string SsnMatchInd_Json
  {
    get => NullIf(SsnMatchInd, "");
    set => SsnMatchInd = value;
  }

  /// <summary>Length of the REPORTING_QUARTER attribute.</summary>
  public const int ReportingQuarter_MaxLength = 5;

  /// <summary>
  /// The value of the REPORTING_QUARTER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 57, Type = MemberType.Char, Length
    = ReportingQuarter_MaxLength)]
  public string ReportingQuarter
  {
    get => reportingQuarter ?? "";
    set => reportingQuarter =
      TrimEnd(Substring(value, 1, ReportingQuarter_MaxLength));
  }

  /// <summary>
  /// The json value of the ReportingQuarter attribute.</summary>
  [JsonPropertyName("reportingQuarter")]
  [Computed]
  public string ReportingQuarter_Json
  {
    get => NullIf(ReportingQuarter, "");
    set => ReportingQuarter = value;
  }

  /// <summary>Length of the NDNH_RESPONSE attribute.</summary>
  public const int NdnhResponse_MaxLength = 1;

  /// <summary>
  /// The value of the NDNH_RESPONSE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 58, Type = MemberType.Char, Length = NdnhResponse_MaxLength)]
  public string NdnhResponse
  {
    get => ndnhResponse ?? "";
    set => ndnhResponse = TrimEnd(Substring(value, 1, NdnhResponse_MaxLength));
  }

  /// <summary>
  /// The json value of the NdnhResponse attribute.</summary>
  [JsonPropertyName("ndnhResponse")]
  [Computed]
  public string NdnhResponse_Json
  {
    get => NullIf(NdnhResponse, "");
    set => NdnhResponse = value;
  }

  /// <summary>
  /// The value of the NSA_DATE_OF_DEATH attribute.
  /// </summary>
  [JsonPropertyName("nsaDateOfDeath")]
  [Member(Index = 59, Type = MemberType.Date)]
  public DateTime? NsaDateOfDeath
  {
    get => nsaDateOfDeath;
    set => nsaDateOfDeath = value;
  }

  private DateTime? dodDateOfDeathOrSeparation;
  private string dodEligibilityCode;
  private string stateAbbreviation;
  private string stationNumber;
  private string agencyCode;
  private string nameSentInd;
  private string apFirstName;
  private string apMiddleName;
  private string apFirstLastName;
  private string apSecondLastName;
  private string apThirdLastName;
  private string apNameReturned;
  private string ssnSubmitted;
  private string apCsePersonNumber;
  private int fplsRequestIdentifier;
  private string usersField;
  private string localCode;
  private string typeOfCase;
  private string addrDateFormatInd;
  private DateTime? dateOfAddress;
  private string responseCode;
  private string addressFormatInd;
  private string returnedAddress;
  private string dodStatus;
  private string dodServiceCode;
  private string dodPayGradeCode;
  private int dodAnnualSalary;
  private DateTime? dodDateOfBirth;
  private string submittingOffice;
  private string sesaRespondingState;
  private string sesaWageClaimInd;
  private int sesaWageAmount;
  private string irsNameControl;
  private string cpSsn;
  private string irsTaxYear;
  private string nprcEmpdOrSepd;
  private string ssaFederalOrMilitary;
  private string ssaCorpDivision;
  private int mbrBenefitAmount;
  private DateTime? mbrDateOfDeath;
  private string vaBenefitCode;
  private DateTime? vaDateOfDeath;
  private DateTime? vaAmtOfAwardEffectiveDate;
  private int vaAmountOfAward;
  private string vaSuspenseCode;
  private string vaIncarcerationCode;
  private string vaRetirementPayCode;
  private string addressIndType;
  private string healthInsBenefitIndicator;
  private string employmentStatus;
  private string employmentInd;
  private DateTime? dateOfHire;
  private string reportingFedAgency;
  private string fein;
  private string correctedAdditionMultipleSsn;
  private string ssnMatchInd;
  private string reportingQuarter;
  private string ndnhResponse;
  private DateTime? nsaDateOfDeath;
}
