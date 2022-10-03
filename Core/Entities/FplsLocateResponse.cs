// The source file: FPLS_LOCATE_RESPONSE, ID: 371434564, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This entity contains the responses received from FPLS system interface.
/// </summary>
[Serializable]
public partial class FplsLocateResponse: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FplsLocateResponse()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FplsLocateResponse(FplsLocateResponse that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FplsLocateResponse Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FplsLocateResponse that)
  {
    base.Assign(that);
    identifier = that.identifier;
    dateReceived = that.dateReceived;
    usageStatus = that.usageStatus;
    dateUsed = that.dateUsed;
    agencyCode = that.agencyCode;
    nameSentInd = that.nameSentInd;
    apNameReturned = that.apNameReturned;
    addrDateFormatInd = that.addrDateFormatInd;
    dateOfAddress = that.dateOfAddress;
    responseCode = that.responseCode;
    addressFormatInd = that.addressFormatInd;
    returnedAddress = that.returnedAddress;
    dodEligibilityCode = that.dodEligibilityCode;
    dodDateOfDeathOrSeparation = that.dodDateOfDeathOrSeparation;
    dodStatus = that.dodStatus;
    dodServiceCode = that.dodServiceCode;
    dodPayGradeCode = that.dodPayGradeCode;
    sesaRespondingState = that.sesaRespondingState;
    sesaWageClaimInd = that.sesaWageClaimInd;
    sesaWageAmount = that.sesaWageAmount;
    irsNameControl = that.irsNameControl;
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
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    ssnReturned = that.ssnReturned;
    dodAnnualSalary = that.dodAnnualSalary;
    dodDateOfBirth = that.dodDateOfBirth;
    submittingOffice = that.submittingOffice;
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
    flqIdentifier = that.flqIdentifier;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The attribute, which together with the relation to FPLS_REQUEST, uniquely 
  /// identifies one occurrence of the FPLS response.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the DATE_RECEIVED attribute.
  /// Date on which the response was received by KESSEP.
  /// </summary>
  [JsonPropertyName("dateReceived")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? DateReceived
  {
    get => dateReceived;
    set => dateReceived = value;
  }

  /// <summary>Length of the USAGE_STATUS attribute.</summary>
  public const int UsageStatus_MaxLength = 1;

  /// <summary>
  /// The value of the USAGE_STATUS attribute.
  /// Usage status of the response received from FPLS.
  /// Valid values are maintained in CODE_VALUE entity for CODE_NAME 
  /// INTERFACE_RESP_USAGE_STATUS
  /// space - not yet examined
  /// U     - Response has been used
  /// D     - Response has been discarded
  /// </summary>
  [JsonPropertyName("usageStatus")]
  [Member(Index = 3, Type = MemberType.Char, Length = UsageStatus_MaxLength, Optional
    = true)]
  public string UsageStatus
  {
    get => usageStatus;
    set => usageStatus = value != null
      ? TrimEnd(Substring(value, 1, UsageStatus_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DATE_USED attribute.
  /// Date on which the information from the response was used by the worker.
  /// </summary>
  [JsonPropertyName("dateUsed")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? DateUsed
  {
    get => dateUsed;
    set => dateUsed = value;
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
  [JsonPropertyName("agencyCode")]
  [Member(Index = 5, Type = MemberType.Char, Length = AgencyCode_MaxLength, Optional
    = true)]
  public string AgencyCode
  {
    get => agencyCode;
    set => agencyCode = value != null
      ? TrimEnd(Substring(value, 1, AgencyCode_MaxLength)) : null;
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
  [JsonPropertyName("nameSentInd")]
  [Member(Index = 6, Type = MemberType.Char, Length = NameSentInd_MaxLength, Optional
    = true)]
  public string NameSentInd
  {
    get => nameSentInd;
    set => nameSentInd = value != null
      ? TrimEnd(Substring(value, 1, NameSentInd_MaxLength)) : null;
  }

  /// <summary>Length of the AP_NAME_RETURNED attribute.</summary>
  public const int ApNameReturned_MaxLength = 50;

  /// <summary>
  /// The value of the AP_NAME_RETURNED attribute.
  /// AP's name as returned by the agencies. This name may contain employer's 
  /// name. (Not all agencies return one).
  /// </summary>
  [JsonPropertyName("apNameReturned")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = ApNameReturned_MaxLength, Optional = true)]
  public string ApNameReturned
  {
    get => apNameReturned;
    set => apNameReturned = value != null
      ? TrimEnd(Substring(value, 1, ApNameReturned_MaxLength)) : null;
  }

  /// <summary>Length of the ADDR_DATE_FORMAT_IND attribute.</summary>
  public const int AddrDateFormatInd_MaxLength = 1;

  /// <summary>
  /// The value of the ADDR_DATE_FORMAT_IND attribute.
  /// Indicates the format the date of address is returned in DATE_OF_ADDRESS 
  /// field.
  /// 0 - 0000 format (not available)
  /// 1 - MMYY format
  /// 2 - QRYY format (QR=calendar quarter)
  /// 3 - 00YY format
  /// </summary>
  [JsonPropertyName("addrDateFormatInd")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = AddrDateFormatInd_MaxLength, Optional = true)]
  public string AddrDateFormatInd
  {
    get => addrDateFormatInd;
    set => addrDateFormatInd = value != null
      ? TrimEnd(Substring(value, 1, AddrDateFormatInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DATE_OF_ADDRESS attribute.
  /// The date of the address provided by the agency
  /// </summary>
  [JsonPropertyName("dateOfAddress")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfAddress
  {
    get => dateOfAddress;
    set => dateOfAddress = value;
  }

  /// <summary>Length of the RESPONSE_CODE attribute.</summary>
  public const int ResponseCode_MaxLength = 3;

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
  [JsonPropertyName("responseCode")]
  [Member(Index = 10, Type = MemberType.Char, Length = ResponseCode_MaxLength, Optional
    = true)]
  public string ResponseCode
  {
    get => responseCode;
    set => responseCode = value != null
      ? TrimEnd(Substring(value, 1, ResponseCode_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_FORMAT_IND attribute.</summary>
  public const int AddressFormatInd_MaxLength = 1;

  /// <summary>
  /// The value of the ADDRESS_FORMAT_IND attribute.
  /// Indicates the format in which the address is returned in RETURNED_ADDRESS 
  /// field.
  /// C - City, State and ZIP break down.
  /// F - Free format (lines separated by \) and with an isolated ZIP code when 
  /// possible.
  /// Blank - No address
  /// </summary>
  [JsonPropertyName("addressFormatInd")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = AddressFormatInd_MaxLength, Optional = true)]
  public string AddressFormatInd
  {
    get => addressFormatInd;
    set => addressFormatInd = value != null
      ? TrimEnd(Substring(value, 1, AddressFormatInd_MaxLength)) : null;
  }

  /// <summary>Length of the RETURNED_ADDRESS attribute.</summary>
  public const int ReturnedAddress_MaxLength = 234;

  /// <summary>
  /// The value of the RETURNED_ADDRESS attribute.
  /// Address as returned by the agency in one of the formats specified by the 
  /// address format indicator.
  /// </summary>
  [JsonPropertyName("returnedAddress")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = ReturnedAddress_MaxLength, Optional = true)]
  public string ReturnedAddress
  {
    get => returnedAddress;
    set => returnedAddress = value != null
      ? TrimEnd(Substring(value, 1, ReturnedAddress_MaxLength)) : null;
  }

  /// <summary>Length of the DOD_ELIGIBILITY_CODE attribute.</summary>
  public const int DodEligibilityCode_MaxLength = 1;

  /// <summary>
  /// The value of the DOD_ELIGIBILITY_CODE attribute.
  /// Returned by Department of Defense (D.O.D)
  /// 0 - Eligible for medical benefits
  /// 1 - Ineligible for medical benefits
  /// 2 - Deceased
  /// 3 - Reservist (Not on Active Duty)
  /// </summary>
  [JsonPropertyName("dodEligibilityCode")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = DodEligibilityCode_MaxLength, Optional = true)]
  public string DodEligibilityCode
  {
    get => dodEligibilityCode;
    set => dodEligibilityCode = value != null
      ? TrimEnd(Substring(value, 1, DodEligibilityCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DOD_DATE_OF_DEATH_OR_SEPARATION attribute.
  /// Returned by Department of Defense (D.O.D)
  /// Date of death or Date of separation from military.
  /// The input flat file from FPLS contains data in MMDDYY form.
  /// If DOD_ELIGIBILITY_CODE = 2, this attribute gives Date of Death; otherwise
  /// it gives Date of Separation.
  /// </summary>
  [JsonPropertyName("dodDateOfDeathOrSeparation")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? DodDateOfDeathOrSeparation
  {
    get => dodDateOfDeathOrSeparation;
    set => dodDateOfDeathOrSeparation = value;
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
  [JsonPropertyName("dodStatus")]
  [Member(Index = 15, Type = MemberType.Char, Length = DodStatus_MaxLength, Optional
    = true)]
  public string DodStatus
  {
    get => dodStatus;
    set => dodStatus = value != null
      ? TrimEnd(Substring(value, 1, DodStatus_MaxLength)) : null;
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
  [JsonPropertyName("dodServiceCode")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = DodServiceCode_MaxLength, Optional = true)]
  public string DodServiceCode
  {
    get => dodServiceCode;
    set => dodServiceCode = value != null
      ? TrimEnd(Substring(value, 1, DodServiceCode_MaxLength)) : null;
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
  [JsonPropertyName("dodPayGradeCode")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = DodPayGradeCode_MaxLength, Optional = true)]
  public string DodPayGradeCode
  {
    get => dodPayGradeCode;
    set => dodPayGradeCode = value != null
      ? TrimEnd(Substring(value, 1, DodPayGradeCode_MaxLength)) : null;
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
  [JsonPropertyName("sesaRespondingState")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = SesaRespondingState_MaxLength, Optional = true)]
  public string SesaRespondingState
  {
    get => sesaRespondingState;
    set => sesaRespondingState = value != null
      ? TrimEnd(Substring(value, 1, SesaRespondingState_MaxLength)) : null;
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
  [JsonPropertyName("sesaWageClaimInd")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = SesaWageClaimInd_MaxLength, Optional = true)]
  public string SesaWageClaimInd
  {
    get => sesaWageClaimInd;
    set => sesaWageClaimInd = value != null
      ? TrimEnd(Substring(value, 1, SesaWageClaimInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SESA_WAGE_AMOUNT attribute.
  /// Returned by SESA
  /// Amount is always positive and will be in dollars when available; otherwise
  /// it will contain zeros.
  /// </summary>
  [JsonPropertyName("sesaWageAmount")]
  [Member(Index = 20, Type = MemberType.Number, Length = 6, Optional = true)]
  public int? SesaWageAmount
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
  [JsonPropertyName("irsNameControl")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = IrsNameControl_MaxLength, Optional = true)]
  public string IrsNameControl
  {
    get => irsNameControl;
    set => irsNameControl = value != null
      ? TrimEnd(Substring(value, 1, IrsNameControl_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IRS_TAX_YEAR attribute.
  /// Returned by IRS
  /// The FPLS flat file contains year in YY format only.
  /// Tax year of filed tax return
  /// Tax year can be zeros.
  /// </summary>
  [JsonPropertyName("irsTaxYear")]
  [Member(Index = 22, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? IrsTaxYear
  {
    get => irsTaxYear;
    set => irsTaxYear = value;
  }

  /// <summary>Length of the NPRC_EMPD_OR_SEPD attribute.</summary>
  public const int NprcEmpdOrSepd_MaxLength = 1;

  /// <summary>
  /// The value of the NPRC_EMPD_OR_SEPD attribute.
  /// Returned by National Personnel Records Center (NPRC)
  /// E - Employed
  /// S - Separated
  /// </summary>
  [JsonPropertyName("nprcEmpdOrSepd")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = NprcEmpdOrSepd_MaxLength, Optional = true)]
  public string NprcEmpdOrSepd
  {
    get => nprcEmpdOrSepd;
    set => nprcEmpdOrSepd = value != null
      ? TrimEnd(Substring(value, 1, NprcEmpdOrSepd_MaxLength)) : null;
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
  [JsonPropertyName("ssaFederalOrMilitary")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = SsaFederalOrMilitary_MaxLength, Optional = true)]
  public string SsaFederalOrMilitary
  {
    get => ssaFederalOrMilitary;
    set => ssaFederalOrMilitary = value != null
      ? TrimEnd(Substring(value, 1, SsaFederalOrMilitary_MaxLength)) : null;
  }

  /// <summary>Length of the SSA_CORP_DIVISION attribute.</summary>
  public const int SsaCorpDivision_MaxLength = 4;

  /// <summary>
  /// The value of the SSA_CORP_DIVISION attribute.
  /// Returned Bby SSA	
  /// Employee's Corporate Division (SSA)
  /// </summary>
  [JsonPropertyName("ssaCorpDivision")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = SsaCorpDivision_MaxLength, Optional = true)]
  public string SsaCorpDivision
  {
    get => ssaCorpDivision;
    set => ssaCorpDivision = value != null
      ? TrimEnd(Substring(value, 1, SsaCorpDivision_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MBR_BENEFIT_AMOUNT attribute.
  /// Returned by Member Beneficiary Record (MBR)
  /// Amount is always positive and will be in dollars when available. Otherwise
  /// zeros.
  /// </summary>
  [JsonPropertyName("mbrBenefitAmount")]
  [Member(Index = 26, Type = MemberType.Number, Length = 6, Optional = true)]
  public int? MbrBenefitAmount
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
  [Member(Index = 27, Type = MemberType.Date, Optional = true)]
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
  [JsonPropertyName("vaBenefitCode")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = VaBenefitCode_MaxLength, Optional = true)]
  public string VaBenefitCode
  {
    get => vaBenefitCode;
    set => vaBenefitCode = value != null
      ? TrimEnd(Substring(value, 1, VaBenefitCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the VA_DATE_OF_DEATH attribute.
  /// Returned by Veterans Administration (VA)
  /// Specifies Date of Death
  /// FPLS flat file contains date in MMDDYY format
  /// </summary>
  [JsonPropertyName("vaDateOfDeath")]
  [Member(Index = 29, Type = MemberType.Date, Optional = true)]
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
  [Member(Index = 30, Type = MemberType.Date, Optional = true)]
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
  [Member(Index = 31, Type = MemberType.Number, Length = 6, Optional = true)]
  public int? VaAmountOfAward
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
  [JsonPropertyName("vaSuspenseCode")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = VaSuspenseCode_MaxLength, Optional = true)]
  public string VaSuspenseCode
  {
    get => vaSuspenseCode;
    set => vaSuspenseCode = value != null
      ? TrimEnd(Substring(value, 1, VaSuspenseCode_MaxLength)) : null;
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
  [JsonPropertyName("vaIncarcerationCode")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = VaIncarcerationCode_MaxLength, Optional = true)]
  public string VaIncarcerationCode
  {
    get => vaIncarcerationCode;
    set => vaIncarcerationCode = value != null
      ? TrimEnd(Substring(value, 1, VaIncarcerationCode_MaxLength)) : null;
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
  [JsonPropertyName("vaRetirementPayCode")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = VaRetirementPayCode_MaxLength, Optional = true)]
  public string VaRetirementPayCode
  {
    get => vaRetirementPayCode;
    set => vaRetirementPayCode = value != null
      ? TrimEnd(Substring(value, 1, VaRetirementPayCode_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 35, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 36, Type = MemberType.Timestamp, Optional = true)]
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
  [Member(Index = 37, Type = MemberType.Char, Length
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
  [Member(Index = 38, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the SSN_RETURNED attribute.</summary>
  public const int SsnReturned_MaxLength = 9;

  /// <summary>
  /// The value of the SSN_RETURNED attribute.
  /// This attribute specifies the Social Security Number as located by FPLS.
  /// </summary>
  [JsonPropertyName("ssnReturned")]
  [Member(Index = 39, Type = MemberType.Char, Length = SsnReturned_MaxLength, Optional
    = true)]
  public string SsnReturned
  {
    get => ssnReturned;
    set => ssnReturned = value != null
      ? TrimEnd(Substring(value, 1, SsnReturned_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DOD_ANNUAL_SALARY attribute.
  /// Department of Defense annual salary in whole dollars.
  /// </summary>
  [JsonPropertyName("dodAnnualSalary")]
  [Member(Index = 40, Type = MemberType.Number, Length = 6, Optional = true)]
  public int? DodAnnualSalary
  {
    get => dodAnnualSalary;
    set => dodAnnualSalary = value;
  }

  /// <summary>
  /// The value of the DOD_DATE_OF_BIRTH attribute.
  /// Department of Defense date of birth.
  /// </summary>
  [JsonPropertyName("dodDateOfBirth")]
  [Member(Index = 41, Type = MemberType.Date, Optional = true)]
  public DateTime? DodDateOfBirth
  {
    get => dodDateOfBirth;
    set => dodDateOfBirth = value;
  }

  /// <summary>Length of the SUBMITTING_OFFICE attribute.</summary>
  public const int SubmittingOffice_MaxLength = 4;

  /// <summary>
  /// The value of the SUBMITTING_OFFICE attribute.
  /// Personnel Office Identifier (POI) from the Office of Personnel Management 
  /// (OPM).  This code is used with DOD status codes 1, 7 and B.
  /// </summary>
  [JsonPropertyName("submittingOffice")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = SubmittingOffice_MaxLength, Optional = true)]
  public string SubmittingOffice
  {
    get => submittingOffice;
    set => submittingOffice = value != null
      ? TrimEnd(Substring(value, 1, SubmittingOffice_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_IND_TYPE attribute.</summary>
  public const int AddressIndType_MaxLength = 1;

  /// <summary>
  /// The value of the ADDRESS_IND_TYPE attribute.
  /// This field will contain one of the following codes to indicate the 
  /// type of address provided in returned address field:
  /// 1-Employer Address
  /// 
  /// 2-Employee Address
  /// 
  /// Space-No address Returned.
  /// </summary>
  [JsonPropertyName("addressIndType")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = AddressIndType_MaxLength, Optional = true)]
  public string AddressIndType
  {
    get => addressIndType;
    set => addressIndType = value != null
      ? TrimEnd(Substring(value, 1, AddressIndType_MaxLength)) : null;
  }

  /// <summary>Length of the HEALTH_INS_BENEFIT_INDICATOR attribute.</summary>
  public const int HealthInsBenefitIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the HEALTH_INS_BENEFIT_INDICATOR attribute.
  /// This field will contain one of the following codes to indicate the 
  /// employee's type of health insurance coverage.                   F-Family
  /// 
  /// I-Individual
  /// 
  /// Space-Not Available.
  /// </summary>
  [JsonPropertyName("healthInsBenefitIndicator")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = HealthInsBenefitIndicator_MaxLength, Optional = true)]
  public string HealthInsBenefitIndicator
  {
    get => healthInsBenefitIndicator;
    set => healthInsBenefitIndicator = value != null
      ? TrimEnd(Substring(value, 1, HealthInsBenefitIndicator_MaxLength)) : null
      ;
  }

  /// <summary>Length of the EMPLOYMENT_STATUS attribute.</summary>
  public const int EmploymentStatus_MaxLength = 1;

  /// <summary>
  /// The value of the EMPLOYMENT_STATUS attribute.
  /// This field will contain one of the 
  /// following codes to indicate the employee's
  /// employment status:
  /// 
  /// A-Active,L-On Annual Leave, M-On military
  /// Leave, I-Inactive, R-Retired, V-Vacation,
  /// Space-Not Available.
  /// </summary>
  [JsonPropertyName("employmentStatus")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = EmploymentStatus_MaxLength, Optional = true)]
  public string EmploymentStatus
  {
    get => employmentStatus;
    set => employmentStatus = value != null
      ? TrimEnd(Substring(value, 1, EmploymentStatus_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYMENT_IND attribute.</summary>
  public const int EmploymentInd_MaxLength = 1;

  /// <summary>
  /// The value of the EMPLOYMENT_IND attribute.
  /// This field will contain one of the 
  /// following codes to indicate the employee's
  /// type of employment:
  /// 
  /// P-permanent, T-Temporary, C-Consultant, I-
  /// Independent Contractor, Space-Not
  /// Available.
  /// </summary>
  [JsonPropertyName("employmentInd")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = EmploymentInd_MaxLength, Optional = true)]
  public string EmploymentInd
  {
    get => employmentInd;
    set => employmentInd = value != null
      ? TrimEnd(Substring(value, 1, EmploymentInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DATE_OF_HIRE attribute.
  /// This field will contain the employee's date of hire in the year 2000-
  /// compliant format of CCYYMMDD. If not available this field will be all
  /// spaces.
  /// </summary>
  [JsonPropertyName("dateOfHire")]
  [Member(Index = 47, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfHire
  {
    get => dateOfHire;
    set => dateOfHire = value;
  }

  /// <summary>Length of the REPORTING_FED_AGENCY attribute.</summary>
  public const int ReportingFedAgency_MaxLength = 9;

  /// <summary>
  /// The value of the REPORTING_FED_AGENCY attribute.
  /// This field will contain the code of the federal agency returning 
  /// information. This field will be all spaces if the name is not available.
  /// </summary>
  [JsonPropertyName("reportingFedAgency")]
  [Member(Index = 48, Type = MemberType.Char, Length
    = ReportingFedAgency_MaxLength, Optional = true)]
  public string ReportingFedAgency
  {
    get => reportingFedAgency;
    set => reportingFedAgency = value != null
      ? TrimEnd(Substring(value, 1, ReportingFedAgency_MaxLength)) : null;
  }

  /// <summary>Length of the FEIN attribute.</summary>
  public const int Fein_MaxLength = 9;

  /// <summary>
  /// The value of the FEIN attribute.
  /// This field will contain the Federal Employer Identification Number for the
  /// federal agency. This field will be all spaces if not available.
  /// </summary>
  [JsonPropertyName("fein")]
  [Member(Index = 49, Type = MemberType.Char, Length = Fein_MaxLength, Optional
    = true)]
  public string Fein
  {
    get => fein;
    set => fein = value != null
      ? TrimEnd(Substring(value, 1, Fein_MaxLength)) : null;
  }

  /// <summary>Length of the CORRECTED_ADDITION_MULTIPLE_SSN attribute.
  /// </summary>
  public const int CorrectedAdditionMultipleSsn_MaxLength = 9;

  /// <summary>
  /// The value of the CORRECTED_ADDITION_MULTIPLE_SSN attribute.
  /// This field will contain one of the following to clarify which SSN was used
  /// in the NDNH search:                                         If the SSN
  /// Match Indicator is 'C', this field will contain the corrected SSN.
  /// 
  /// If the SSN Match Indicator is 'M' or'X', this field will contain the
  /// Additional/Multiple SSN used in the match. (The SSN in this field will be
  /// different from the SSN in SSN field.)                     If the SSN Match
  /// Indicator is a 'V', this field will be spaces. (The SSN Used in the match
  /// will be int the SSN field.)
  /// </summary>
  [JsonPropertyName("correctedAdditionMultipleSsn")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = CorrectedAdditionMultipleSsn_MaxLength, Optional = true)]
  public string CorrectedAdditionMultipleSsn
  {
    get => correctedAdditionMultipleSsn;
    set => correctedAdditionMultipleSsn = value != null
      ? TrimEnd(Substring(value, 1, CorrectedAdditionMultipleSsn_MaxLength)) : null
      ;
  }

  /// <summary>Length of the SSN_MATCH_IND attribute.</summary>
  public const int SsnMatchInd_MaxLength = 1;

  /// <summary>
  /// The value of the SSN_MATCH_IND attribute.
  /// This field will contain one of the following codes to indicate if the 
  /// SSN contained in the record is the State-Submitted SSN, or a corrected
  /// or multiple SSN:                                                   C-
  /// Corrected SSN
  /// 
  /// M-Additional/Multiple SSN
  /// V-State-
  /// Submitted verified SSN
  /// X-Multiple SSN
  /// from a Corrected SSN                                       If this field
  /// is 'C','M' or 'X', the SSN used in the match will be in the Corrected/
  /// Additional/Multiple SSN fields.
  /// </summary>
  [JsonPropertyName("ssnMatchInd")]
  [Member(Index = 51, Type = MemberType.Char, Length = SsnMatchInd_MaxLength, Optional
    = true)]
  public string SsnMatchInd
  {
    get => ssnMatchInd;
    set => ssnMatchInd = value != null
      ? TrimEnd(Substring(value, 1, SsnMatchInd_MaxLength)) : null;
  }

  /// <summary>Length of the REPORTING_QUARTER attribute.</summary>
  public const int ReportingQuarter_MaxLength = 5;

  /// <summary>
  /// The value of the REPORTING_QUARTER attribute.
  /// This will contain the time period for the UI being sent in this record.
  /// 
  /// The format is CCYYQ. CC-Century,YY-Year.                              Q-
  /// Reporting quarter:
  /// 1-
  /// January1 thru March 31
  /// 2-April 1 thru
  /// June 30                                                                 3
  /// -July 1 thru September 30
  /// 4-October 1 thru
  /// December 31                                                   This field
  /// will be spaces if the information is not available.
  /// </summary>
  [JsonPropertyName("reportingQuarter")]
  [Member(Index = 52, Type = MemberType.Char, Length
    = ReportingQuarter_MaxLength, Optional = true)]
  public string ReportingQuarter
  {
    get => reportingQuarter;
    set => reportingQuarter = value != null
      ? TrimEnd(Substring(value, 1, ReportingQuarter_MaxLength)) : null;
  }

  /// <summary>Length of the NDNH_RESPONSE attribute.</summary>
  public const int NdnhResponse_MaxLength = 1;

  /// <summary>
  /// The value of the NDNH_RESPONSE attribute.
  /// This will contain the value:
  /// 
  /// Y-Response is received from NDNH
  /// N-Response is
  /// not received from NDNH.
  /// </summary>
  [JsonPropertyName("ndnhResponse")]
  [Member(Index = 53, Type = MemberType.Char, Length = NdnhResponse_MaxLength, Optional
    = true)]
  public string NdnhResponse
  {
    get => ndnhResponse;
    set => ndnhResponse = value != null
      ? TrimEnd(Substring(value, 1, NdnhResponse_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The attribute, which together with the relationship with CSE_PERSON, 
  /// identifies uniquely an FPLS transaction.
  /// </summary>
  [JsonPropertyName("flqIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 54, Type = MemberType.Number, Length = 3)]
  public int FlqIdentifier
  {
    get => flqIdentifier;
    set => flqIdentifier = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 55, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  private int identifier;
  private DateTime? dateReceived;
  private string usageStatus;
  private DateTime? dateUsed;
  private string agencyCode;
  private string nameSentInd;
  private string apNameReturned;
  private string addrDateFormatInd;
  private DateTime? dateOfAddress;
  private string responseCode;
  private string addressFormatInd;
  private string returnedAddress;
  private string dodEligibilityCode;
  private DateTime? dodDateOfDeathOrSeparation;
  private string dodStatus;
  private string dodServiceCode;
  private string dodPayGradeCode;
  private string sesaRespondingState;
  private string sesaWageClaimInd;
  private int? sesaWageAmount;
  private string irsNameControl;
  private int? irsTaxYear;
  private string nprcEmpdOrSepd;
  private string ssaFederalOrMilitary;
  private string ssaCorpDivision;
  private int? mbrBenefitAmount;
  private DateTime? mbrDateOfDeath;
  private string vaBenefitCode;
  private DateTime? vaDateOfDeath;
  private DateTime? vaAmtOfAwardEffectiveDate;
  private int? vaAmountOfAward;
  private string vaSuspenseCode;
  private string vaIncarcerationCode;
  private string vaRetirementPayCode;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string ssnReturned;
  private int? dodAnnualSalary;
  private DateTime? dodDateOfBirth;
  private string submittingOffice;
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
  private int flqIdentifier;
  private string cspNumber;
}
