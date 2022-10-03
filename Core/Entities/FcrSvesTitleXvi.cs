// The source file: FCR_SVES_TITLE_XVI, ID: 945065698, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// This Entity Type stores SVES Title XVI Claims (E06) response information 
/// from FCR.
/// </summary>
[Serializable]
public partial class FcrSvesTitleXvi: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrSvesTitleXvi()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrSvesTitleXvi(FcrSvesTitleXvi that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrSvesTitleXvi Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrSvesTitleXvi that)
  {
    base.Assign(that);
    seqNo = that.seqNo;
    otherName = that.otherName;
    raceCode = that.raceCode;
    dateOfDeathSourceCode = that.dateOfDeathSourceCode;
    payeeStateOfJurisdiction = that.payeeStateOfJurisdiction;
    payeeCountyOfJurisdiction = that.payeeCountyOfJurisdiction;
    payeeDistrictOfficeCode = that.payeeDistrictOfficeCode;
    typeOfPayeeCode = that.typeOfPayeeCode;
    typeOfRecipient = that.typeOfRecipient;
    recordEstablishmentDate = that.recordEstablishmentDate;
    dateOfTitleXviEligibility = that.dateOfTitleXviEligibility;
    titleXviAppealCode = that.titleXviAppealCode;
    dateOfTitleXviAppeal = that.dateOfTitleXviAppeal;
    titleXviLastRedeterminDate = that.titleXviLastRedeterminDate;
    titleXviDenialDate = that.titleXviDenialDate;
    currentPaymentStatusCode = that.currentPaymentStatusCode;
    paymentStatusCode = that.paymentStatusCode;
    paymentStatusDate = that.paymentStatusDate;
    telephoneNumber = that.telephoneNumber;
    thirdPartyInsuranceIndicator = that.thirdPartyInsuranceIndicator;
    directDepositIndicator = that.directDepositIndicator;
    representativePayeeIndicator = that.representativePayeeIndicator;
    custodyCode = that.custodyCode;
    estimatedSelfEmploymentAmount = that.estimatedSelfEmploymentAmount;
    unearnedIncomeNumOfEntries = that.unearnedIncomeNumOfEntries;
    unearnedIncomeTypeCode1 = that.unearnedIncomeTypeCode1;
    unearnedIncomeVerifiCd1 = that.unearnedIncomeVerifiCd1;
    unearnedIncomeStartDate1 = that.unearnedIncomeStartDate1;
    unearnedIncomeStopDate1 = that.unearnedIncomeStopDate1;
    unearnedIncomeTypeCode2 = that.unearnedIncomeTypeCode2;
    unearnedIncomeVerifiCd2 = that.unearnedIncomeVerifiCd2;
    unearnedIncomeStartDate2 = that.unearnedIncomeStartDate2;
    unearnedIncomeStopDate2 = that.unearnedIncomeStopDate2;
    unearnedIncomeTypeCode3 = that.unearnedIncomeTypeCode3;
    unearnedIncomeVerifiCd3 = that.unearnedIncomeVerifiCd3;
    unearnedIncomeStartDate3 = that.unearnedIncomeStartDate3;
    unearnedIncomeStopDate3 = that.unearnedIncomeStopDate3;
    unearnedIncomeTypeCode4 = that.unearnedIncomeTypeCode4;
    unearnedIncomeVerifiCd4 = that.unearnedIncomeVerifiCd4;
    unearnedIncomeStartDate4 = that.unearnedIncomeStartDate4;
    unearnedIncomeStopDate4 = that.unearnedIncomeStopDate4;
    unearnedIncomeTypeCode5 = that.unearnedIncomeTypeCode5;
    unearnedIncomeVerifiCd5 = that.unearnedIncomeVerifiCd5;
    unearnedIncomeStartDate5 = that.unearnedIncomeStartDate5;
    unearnedIncomeStopDate5 = that.unearnedIncomeStopDate5;
    unearnedIncomeTypeCode6 = that.unearnedIncomeTypeCode6;
    unearnedIncomeVerifiCd6 = that.unearnedIncomeVerifiCd6;
    unearnedIncomeStartDate6 = that.unearnedIncomeStartDate6;
    unearnedIncomeStopDate6 = that.unearnedIncomeStopDate6;
    unearnedIncomeTypeCode7 = that.unearnedIncomeTypeCode7;
    unearnedIncomeVerifiCd7 = that.unearnedIncomeVerifiCd7;
    unearnedIncomeStartDate7 = that.unearnedIncomeStartDate7;
    unearnedIncomeStopDate7 = that.unearnedIncomeStopDate7;
    unearnedIncomeTypeCode8 = that.unearnedIncomeTypeCode8;
    unearnedIncomeVerifiCd8 = that.unearnedIncomeVerifiCd8;
    unearnedIncomeStartDate8 = that.unearnedIncomeStartDate8;
    unearnedIncomeStopDate8 = that.unearnedIncomeStopDate8;
    unearnedIncomeTypeCode9 = that.unearnedIncomeTypeCode9;
    unearnedIncomeVerifiCd9 = that.unearnedIncomeVerifiCd9;
    unearnedIncomeStartDate9 = that.unearnedIncomeStartDate9;
    unearnedIncomeStopDate9 = that.unearnedIncomeStopDate9;
    phistNumberOfEntries = that.phistNumberOfEntries;
    phistPaymentDate1 = that.phistPaymentDate1;
    ssiMonthlyAssistanceAmount1 = that.ssiMonthlyAssistanceAmount1;
    phistPaymentPayFlag1 = that.phistPaymentPayFlag1;
    phistPaymentDate2 = that.phistPaymentDate2;
    ssiMonthlyAssistanceAmount2 = that.ssiMonthlyAssistanceAmount2;
    phistPaymentPayFlag2 = that.phistPaymentPayFlag2;
    phistPaymentDate3 = that.phistPaymentDate3;
    ssiMonthlyAssistanceAmount3 = that.ssiMonthlyAssistanceAmount3;
    phistPaymentPayFlag3 = that.phistPaymentPayFlag3;
    phistPaymentDate4 = that.phistPaymentDate4;
    ssiMonthlyAssistanceAmount4 = that.ssiMonthlyAssistanceAmount4;
    phistPaymentPayFlag4 = that.phistPaymentPayFlag4;
    phistPaymentDate5 = that.phistPaymentDate5;
    ssiMonthlyAssistanceAmount5 = that.ssiMonthlyAssistanceAmount5;
    phistPaymentPayFlag5 = that.phistPaymentPayFlag5;
    phistPaymentDate6 = that.phistPaymentDate6;
    ssiMonthlyAssistanceAmount6 = that.ssiMonthlyAssistanceAmount6;
    phistPaymentPayFlag6 = that.phistPaymentPayFlag6;
    phistPaymentDate7 = that.phistPaymentDate7;
    ssiMonthlyAssistanceAmount7 = that.ssiMonthlyAssistanceAmount7;
    phistPaymentPayFlag7 = that.phistPaymentPayFlag7;
    phistPaymentDate8 = that.phistPaymentDate8;
    ssiMonthlyAssistanceAmount8 = that.ssiMonthlyAssistanceAmount8;
    phistPaymentPayFlag8 = that.phistPaymentPayFlag8;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    fcgLSRspAgy = that.fcgLSRspAgy;
    fcgMemberId = that.fcgMemberId;
  }

  /// <summary>
  /// The value of the SEQ_NO attribute.
  /// This field contains a value to identify unique Title XVI record.
  /// </summary>
  [JsonPropertyName("seqNo")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 2)]
  public int SeqNo
  {
    get => seqNo;
    set => seqNo = value;
  }

  /// <summary>Length of the OTHER_NAME attribute.</summary>
  public const int OtherName_MaxLength = 6;

  /// <summary>
  /// The value of the OTHER_NAME attribute.
  /// This field contains any other name that is used by the recipient.
  /// If no name is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("otherName")]
  [Member(Index = 2, Type = MemberType.Char, Length = OtherName_MaxLength, Optional
    = true)]
  public string OtherName
  {
    get => otherName;
    set => otherName = value != null
      ? TrimEnd(Substring(value, 1, OtherName_MaxLength)) : null;
  }

  /// <summary>Length of the RACE_CODE attribute.</summary>
  public const int RaceCode_MaxLength = 1;

  /// <summary>
  /// The value of the RACE_CODE attribute.
  /// This field contains a value to indicate the race of the recipient:
  /// A  Asian
  /// B  Black
  /// H  Hispanic
  /// I  North American Indian
  /// N  Negro
  /// O  Other
  /// U  Not determined
  /// W  White
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("raceCode")]
  [Member(Index = 3, Type = MemberType.Char, Length = RaceCode_MaxLength, Optional
    = true)]
  public string RaceCode
  {
    get => raceCode;
    set => raceCode = value != null
      ? TrimEnd(Substring(value, 1, RaceCode_MaxLength)) : null;
  }

  /// <summary>Length of the DATE_OF_DEATH_SOURCE_CODE attribute.</summary>
  public const int DateOfDeathSourceCode_MaxLength = 1;

  /// <summary>
  /// The value of the DATE_OF_DEATH_SOURCE_CODE attribute.
  /// This field contains one of the following values:
  /// 1  SSA District Office (DO) notification or manual adjustment
  /// 2  Electronic Death Registration Notification
  /// 3  Master Beneficiary Record (MBR) notification
  /// 4  Treasury returned check notification
  /// 5  Returned check from Treasury with no date of death shown (date of 
  /// death field contains the date of transaction)
  /// 6  State notification
  /// If no code is available, this field contains a space.
  /// If the death code is updated by a subsequent transmission, this code may 
  /// change.
  /// </summary>
  [JsonPropertyName("dateOfDeathSourceCode")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = DateOfDeathSourceCode_MaxLength, Optional = true)]
  public string DateOfDeathSourceCode
  {
    get => dateOfDeathSourceCode;
    set => dateOfDeathSourceCode = value != null
      ? TrimEnd(Substring(value, 1, DateOfDeathSourceCode_MaxLength)) : null;
  }

  /// <summary>Length of the PAYEE_STATE_OF_JURISDICTION attribute.</summary>
  public const int PayeeStateOfJurisdiction_MaxLength = 2;

  /// <summary>
  /// The value of the PAYEE_STATE_OF_JURISDICTION attribute.
  /// This field contains a FIPS code for the State that is responsible for any 
  /// mandatory or optional supplemental payment. The code represents the State
  /// Code of residence for the recipient unless another State has jurisdiction.
  /// If no State is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("payeeStateOfJurisdiction")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = PayeeStateOfJurisdiction_MaxLength, Optional = true)]
  public string PayeeStateOfJurisdiction
  {
    get => payeeStateOfJurisdiction;
    set => payeeStateOfJurisdiction = value != null
      ? TrimEnd(Substring(value, 1, PayeeStateOfJurisdiction_MaxLength)) : null
      ;
  }

  /// <summary>Length of the PAYEE_COUNTY_OF_JURISDICTION attribute.</summary>
  public const int PayeeCountyOfJurisdiction_MaxLength = 3;

  /// <summary>
  /// The value of the PAYEE_COUNTY_OF_JURISDICTION attribute.
  /// This field contains a FIPS code for the county that is responsible for any
  /// mandatory or optional supplemental payment. The code represents the
  /// county code of residence for the recipient unless another county has
  /// jurisdiction.
  /// If no county is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("payeeCountyOfJurisdiction")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = PayeeCountyOfJurisdiction_MaxLength, Optional = true)]
  public string PayeeCountyOfJurisdiction
  {
    get => payeeCountyOfJurisdiction;
    set => payeeCountyOfJurisdiction = value != null
      ? TrimEnd(Substring(value, 1, PayeeCountyOfJurisdiction_MaxLength)) : null
      ;
  }

  /// <summary>Length of the PAYEE_DISTRICT_OFFICE_CODE attribute.</summary>
  public const int PayeeDistrictOfficeCode_MaxLength = 3;

  /// <summary>
  /// The value of the PAYEE_DISTRICT_OFFICE_CODE attribute.
  /// This field contains the SSA District Office (DO) code of the office that 
  /// services the claim  referenced in the Response Record.
  /// </summary>
  [JsonPropertyName("payeeDistrictOfficeCode")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = PayeeDistrictOfficeCode_MaxLength, Optional = true)]
  public string PayeeDistrictOfficeCode
  {
    get => payeeDistrictOfficeCode;
    set => payeeDistrictOfficeCode = value != null
      ? TrimEnd(Substring(value, 1, PayeeDistrictOfficeCode_MaxLength)) : null;
  }

  /// <summary>Length of the TYPE_OF_PAYEE_CODE attribute.</summary>
  public const int TypeOfPayeeCode_MaxLength = 3;

  /// <summary>
  /// The value of the TYPE_OF_PAYEE_CODE attribute.
  /// This field contains a value to indicate who receives the Supplemental 
  /// Security Income (SSI) payment:
  /// AGY  Social agency
  /// CHD  Natural, adopted or stepchild (as payee for a parent)
  /// ESP  Essential person is payee
  /// FDM  Federal mental institution
  /// FDO  Federal non-mental institution
  /// FIN  Financial organization
  /// FTH  Natural or adoptive father
  /// GPR  Grandparent
  /// INP  Legally incompetent, but no representative payee has been selected
  /// MTH  Natural or adoptive mother
  /// NPM  Nonprofit mental institution
  /// NPO  Nonprofit non-mental institution
  /// OFF  Public official
  /// OTH  Other
  /// PRM  Proprietary mental institution
  /// PRO  Proprietary non-mental institution
  /// PYE  Recipient previously had payee, but is now receiving direct payments
  /// REL  Other relative (includes in-laws)
  /// RPD  Representative payee is being developed
  /// SEL  Beneficiary is own payee
  /// SFT  Stepfather
  /// SLM  State/local mental institution
  /// SLO  State/local non-mental institution
  /// SMT  Stepmother
  /// SPO  Spouse
  /// Space  Beneficiary is own payee
  /// </summary>
  [JsonPropertyName("typeOfPayeeCode")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = TypeOfPayeeCode_MaxLength, Optional = true)]
  public string TypeOfPayeeCode
  {
    get => typeOfPayeeCode;
    set => typeOfPayeeCode = value != null
      ? TrimEnd(Substring(value, 1, TypeOfPayeeCode_MaxLength)) : null;
  }

  /// <summary>Length of the TYPE_OF_RECIPIENT attribute.</summary>
  public const int TypeOfRecipient_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE_OF_RECIPIENT attribute.
  /// This field indicates the type of recipient or other individual who is 
  /// involved in the record.
  /// If a recipient is initially disabled, this code will not change at age 65.
  /// This field contains one of the following values:
  /// AI  Aged individual
  /// AS  Aged spouse
  /// BC  Blind child
  /// BI  Blind individual
  /// BS  Blind spouse
  /// DC  Disabled child
  /// DI  Disabled individual
  /// DS  Disabled spouse
  /// EP  Essential person
  /// XF  Ineligible father
  /// XM  Ineligible mother
  /// XP  Ineligible person
  /// XS  Ineligible spouse
  /// </summary>
  [JsonPropertyName("typeOfRecipient")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = TypeOfRecipient_MaxLength, Optional = true)]
  public string TypeOfRecipient
  {
    get => typeOfRecipient;
    set => typeOfRecipient = value != null
      ? TrimEnd(Substring(value, 1, TypeOfRecipient_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the RECORD_ESTABLISHMENT_DATE attribute.
  /// This field contains the date that the SSI record was established for the 
  /// recipient. The date is in CCYYMMDD format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("recordEstablishmentDate")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? RecordEstablishmentDate
  {
    get => recordEstablishmentDate;
    set => recordEstablishmentDate = value;
  }

  /// <summary>
  /// The value of the DATE_OF_TITLE_XVI_ELIGIBILITY attribute.
  /// This field contains the application date, the final onset date, or the 
  /// date that the recipient attained the age of 65 years, whichever is later.
  /// This field is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("dateOfTitleXviEligibility")]
  [Member(Index = 11, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfTitleXviEligibility
  {
    get => dateOfTitleXviEligibility;
    set => dateOfTitleXviEligibility = value;
  }

  /// <summary>Length of the TITLE_XVI_APPEAL_CODE attribute.</summary>
  public const int TitleXviAppealCode_MaxLength = 1;

  /// <summary>
  /// The value of the TITLE_XVI_APPEAL_CODE attribute.
  /// This field contains a value to indicate the level of appeal:
  /// A  Appeal council review
  /// C  Court case
  /// D  Decision Review Board Review
  /// F  Federal Regional Office Review
  /// H  Hearing
  /// I  Initial Determination Review
  /// O  Class action
  /// R  Reconsideration
  /// If no code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("titleXviAppealCode")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = TitleXviAppealCode_MaxLength, Optional = true)]
  public string TitleXviAppealCode
  {
    get => titleXviAppealCode;
    set => titleXviAppealCode = value != null
      ? TrimEnd(Substring(value, 1, TitleXviAppealCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DATE_OF_TITLE_XVI_APPEAL attribute.
  /// This field contains the date of the most recent appeal action. The date is
  /// in CCYYMMDD format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("dateOfTitleXviAppeal")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfTitleXviAppeal
  {
    get => dateOfTitleXviAppeal;
    set => dateOfTitleXviAppeal = value;
  }

  /// <summary>
  /// The value of the TITLE_XVI_LAST_REDETERMIN_DATE attribute.
  /// This field contains the date that all of the required actions for the 
  /// redetermination were completed. This field is in CCYYMMDD format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("titleXviLastRedeterminDate")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? TitleXviLastRedeterminDate
  {
    get => titleXviLastRedeterminDate;
    set => titleXviLastRedeterminDate = value;
  }

  /// <summary>
  /// The value of the TITLE_XVI_DENIAL_DATE attribute.
  /// This field contains the date that the applicant was denied SSI benefits 
  /// and/or State supplementation. The date is in CCYYMMDD format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("titleXviDenialDate")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? TitleXviDenialDate
  {
    get => titleXviDenialDate;
    set => titleXviDenialDate = value;
  }

  /// <summary>Length of the CURRENT_PAYMENT_STATUS_CODE attribute.</summary>
  public const int CurrentPaymentStatusCode_MaxLength = 3;

  /// <summary>
  /// The value of the CURRENT_PAYMENT_STATUS_CODE attribute.
  /// This field refers to the most current SSI payment status code. (See 
  /// Payment Status Code below for an explanation of codes.)
  /// </summary>
  [JsonPropertyName("currentPaymentStatusCode")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = CurrentPaymentStatusCode_MaxLength, Optional = true)]
  public string CurrentPaymentStatusCode
  {
    get => currentPaymentStatusCode;
    set => currentPaymentStatusCode = value != null
      ? TrimEnd(Substring(value, 1, CurrentPaymentStatusCode_MaxLength)) : null
      ;
  }

  /// <summary>Length of the PAYMENT_STATUS_CODE attribute.</summary>
  public const int PaymentStatusCode_MaxLength = 3;

  /// <summary>
  /// The value of the PAYMENT_STATUS_CODE attribute.
  /// This is a three-position alphanumeric field, which is composed of two 
  /// elements: The first position reflects the status of the SSI/State
  /// Supplement payment; the second and third positions reflect the reason for
  /// the status. Refer to Appendix E, Data Dictionary, for the list of valid
  /// values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("paymentStatusCode")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = PaymentStatusCode_MaxLength, Optional = true)]
  public string PaymentStatusCode
  {
    get => paymentStatusCode;
    set => paymentStatusCode = value != null
      ? TrimEnd(Substring(value, 1, PaymentStatusCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PAYMENT_STATUS_DATE attribute.
  /// This field contains the effective date of the last change to the Payment 
  /// Status Code. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("paymentStatusDate")]
  [Member(Index = 18, Type = MemberType.Date, Optional = true)]
  public DateTime? PaymentStatusDate
  {
    get => paymentStatusDate;
    set => paymentStatusDate = value;
  }

  /// <summary>Length of the TELEPHONE_NUMBER attribute.</summary>
  public const int TelephoneNumber_MaxLength = 10;

  /// <summary>
  /// The value of the TELEPHONE_NUMBER attribute.
  /// This field contains the recipients telephone number.
  /// If no number is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("telephoneNumber")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = TelephoneNumber_MaxLength, Optional = true)]
  public string TelephoneNumber
  {
    get => telephoneNumber;
    set => telephoneNumber = value != null
      ? TrimEnd(Substring(value, 1, TelephoneNumber_MaxLength)) : null;
  }

  /// <summary>Length of the THIRD_PARTY_INSURANCE_INDICATOR attribute.
  /// </summary>
  public const int ThirdPartyInsuranceIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the THIRD_PARTY_INSURANCE_INDICATOR attribute.
  /// This field indicates if there could be a third party liability for health 
  /// care expenses. This field is not updated after the initial posting.
  /// This field contains one of the following values:
  /// A  Third party liability does exist but applicant refuses to assign 
  /// rights.
  /// F  Disabled/Blind child living overseas, ineligible for Medicaid, and 
  /// living with a parent who is a member of the military.
  /// N  Third party liability does not exist (1634 State only).
  /// Q  Medicaid qualifying trust may exist.
  /// R  Failure to cooperate in providing third party.
  /// Y  Third party liability does exist (1634 State only) and applicant 
  /// agrees to assign rights.
  ///  If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("thirdPartyInsuranceIndicator")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = ThirdPartyInsuranceIndicator_MaxLength, Optional = true)]
  public string ThirdPartyInsuranceIndicator
  {
    get => thirdPartyInsuranceIndicator;
    set => thirdPartyInsuranceIndicator = value != null
      ? TrimEnd(Substring(value, 1, ThirdPartyInsuranceIndicator_MaxLength)) : null
      ;
  }

  /// <summary>Length of the DIRECT_DEPOSIT_INDICATOR attribute.</summary>
  public const int DirectDepositIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the DIRECT_DEPOSIT_INDICATOR attribute.
  /// This field contains one of the following values:
  /// C  Checking
  /// S  Savings
  /// Space  None
  /// </summary>
  [JsonPropertyName("directDepositIndicator")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = DirectDepositIndicator_MaxLength, Optional = true)]
  public string DirectDepositIndicator
  {
    get => directDepositIndicator;
    set => directDepositIndicator = value != null
      ? TrimEnd(Substring(value, 1, DirectDepositIndicator_MaxLength)) : null;
  }

  /// <summary>Length of the REPRESENTATIVE_PAYEE_INDICATOR attribute.</summary>
  public const int RepresentativePayeeIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the REPRESENTATIVE_PAYEE_INDICATOR attribute.
  /// This field contains a value to indicate the presence of a representative 
  /// payee for the recipient:
  /// Y  There is a representative payee.
  /// N  There is not a representative payee.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("representativePayeeIndicator")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = RepresentativePayeeIndicator_MaxLength, Optional = true)]
  public string RepresentativePayeeIndicator
  {
    get => representativePayeeIndicator;
    set => representativePayeeIndicator = value != null
      ? TrimEnd(Substring(value, 1, RepresentativePayeeIndicator_MaxLength)) : null
      ;
  }

  /// <summary>Length of the CUSTODY_CODE attribute.</summary>
  public const int CustodyCode_MaxLength = 3;

  /// <summary>
  /// The value of the CUSTODY_CODE attribute.
  /// This field contains a value to indicate who has physical custody of the 
  /// recipient:
  /// AGY	 Social agency
  /// CHD	 Natural, adopted or stepchild (as payee for a parent)
  /// ESP	 Essential person is payee
  /// FDM	 Federal mental institution
  /// FDO	 Federal non-mental institution
  /// FIN	 Financial organization
  /// FTH	 Natural or adoptive father
  /// GPR	 Grandparent
  /// INP	 Legally incompetent, but no representative payee
  /// MTH	 Natural or adoptive mother
  /// NPM	 Nonprofit mental institution
  /// NPO	 Nonprofit non-mental institution
  /// OFF	 Public official
  /// OTH	 Other
  /// PRM	 Proprietary mental institution
  /// PRO	 Proprietary non-mental institution
  /// PYE	 Payee has custody
  /// REL	 Other relative (includes in-laws)
  /// RPD	 Representative payee is being developed
  /// SEL	 Living by self
  /// SFT	 Stepfather
  /// SLM	 State/local mental institution
  /// SLO	 State/local non-mental institution
  /// SMT	 Stepmother
  /// SPO	 Spouse
  ///  If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("custodyCode")]
  [Member(Index = 23, Type = MemberType.Char, Length = CustodyCode_MaxLength, Optional
    = true)]
  public string CustodyCode
  {
    get => custodyCode;
    set => custodyCode = value != null
      ? TrimEnd(Substring(value, 1, CustodyCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ESTIMATED_SELF_EMPLOYMENT_AMOUNT attribute.
  /// This field indicates the estimated net amount of self-employment income 
  /// for the period shown. The format is $$$$¢¢.
  /// </summary>
  [JsonPropertyName("estimatedSelfEmploymentAmount")]
  [Member(Index = 24, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? EstimatedSelfEmploymentAmount
  {
    get => estimatedSelfEmploymentAmount;
    set => estimatedSelfEmploymentAmount = value != null
      ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_NUM_OF_ENTRIES attribute.
  /// This field contains the number of occurrences of the unearned income 
  /// fields: Unearned Income Type Code, Unearned Income Verification Code,
  /// Unearned Income Start Date, and Unearned   Income Stop Date. This field
  /// contains the value 0 through 9.
  /// </summary>
  [JsonPropertyName("unearnedIncomeNumOfEntries")]
  [Member(Index = 25, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? UnearnedIncomeNumOfEntries
  {
    get => unearnedIncomeNumOfEntries;
    set => unearnedIncomeNumOfEntries = value;
  }

  /// <summary>Length of the UNEARNED_INCOME_TYPE_CODE_1 attribute.</summary>
  public const int UnearnedIncomeTypeCode1_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_TYPE_CODE_1 attribute.
  /// This field contains a code that indicates the type of unearned income the 
  /// recipient is, or was, receiving. Refer to Appendix E, Data Dictionary,
  /// for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeTypeCode1")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = UnearnedIncomeTypeCode1_MaxLength, Optional = true)]
  public string UnearnedIncomeTypeCode1
  {
    get => unearnedIncomeTypeCode1;
    set => unearnedIncomeTypeCode1 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeTypeCode1_MaxLength)) : null;
  }

  /// <summary>Length of the UNEARNED_INCOME_VERIFI_CD_1 attribute.</summary>
  public const int UnearnedIncomeVerifiCd1_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_VERIFI_CD_1 attribute.
  /// This field contains a code that indicates if the unearned income 
  /// allegations of the recipient have been verified. Refer to Appendix E, 
  /// Data Dictionary, for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeVerifiCd1")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = UnearnedIncomeVerifiCd1_MaxLength, Optional = true)]
  public string UnearnedIncomeVerifiCd1
  {
    get => unearnedIncomeVerifiCd1;
    set => unearnedIncomeVerifiCd1 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeVerifiCd1_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_START_DATE_1 attribute.
  /// This field contains the first occurrence of the date that the one-time 
  /// unearned income payment was received, or the date that the unearned income
  /// was started if the payment is made monthly. The date is in CCYYMM01
  /// format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStartDate1")]
  [Member(Index = 28, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStartDate1
  {
    get => unearnedIncomeStartDate1;
    set => unearnedIncomeStartDate1 = value;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_STOP_DATE_1 attribute.
  /// This field contains the termination date of the first occurrence of 
  /// monthly unearned income. The date is in CCYYMM01 format. In situations
  /// where the unearned income amount changes, this field contains the last
  /// date that the previous rate, or a one-time payment, was received.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStopDate1")]
  [Member(Index = 29, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStopDate1
  {
    get => unearnedIncomeStopDate1;
    set => unearnedIncomeStopDate1 = value;
  }

  /// <summary>Length of the UNEARNED_INCOME_TYPE_CODE_2 attribute.</summary>
  public const int UnearnedIncomeTypeCode2_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_TYPE_CODE_2 attribute.
  /// This field contains a code that indicates the type of unearned income the 
  /// recipient is, or was, receiving. Refer to Appendix E, Data Dictionary,
  /// for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeTypeCode2")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = UnearnedIncomeTypeCode2_MaxLength, Optional = true)]
  public string UnearnedIncomeTypeCode2
  {
    get => unearnedIncomeTypeCode2;
    set => unearnedIncomeTypeCode2 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeTypeCode2_MaxLength)) : null;
  }

  /// <summary>Length of the UNEARNED_INCOME_VERIFI_CD_2 attribute.</summary>
  public const int UnearnedIncomeVerifiCd2_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_VERIFI_CD_2 attribute.
  /// This field contains a code that indicates if the unearned income 
  /// allegations of the recipient have been verified. Refer to Appendix E, 
  /// Data Dictionary, for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeVerifiCd2")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = UnearnedIncomeVerifiCd2_MaxLength, Optional = true)]
  public string UnearnedIncomeVerifiCd2
  {
    get => unearnedIncomeVerifiCd2;
    set => unearnedIncomeVerifiCd2 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeVerifiCd2_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_START_DATE_2 attribute.
  /// This field contains the second occurrence of the monthly-unearned income 
  /// payment date. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStartDate2")]
  [Member(Index = 32, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStartDate2
  {
    get => unearnedIncomeStartDate2;
    set => unearnedIncomeStartDate2 = value;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_STOP_DATE_2 attribute.
  /// This field contains the termination date of the second occurrence of 
  /// monthly unearned income. The date is in CCYYMM01 format. In situations
  /// where the unearned income amount changes, this field contains the last
  /// date that the previous rate, or a one-time payment, was received.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStopDate2")]
  [Member(Index = 33, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStopDate2
  {
    get => unearnedIncomeStopDate2;
    set => unearnedIncomeStopDate2 = value;
  }

  /// <summary>Length of the UNEARNED_INCOME_TYPE_CODE_3 attribute.</summary>
  public const int UnearnedIncomeTypeCode3_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_TYPE_CODE_3 attribute.
  /// This field contains a code that indicates the type of unearned income the 
  /// recipient is, or was, receiving. Refer to Appendix E, Data Dictionary,
  /// for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeTypeCode3")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = UnearnedIncomeTypeCode3_MaxLength, Optional = true)]
  public string UnearnedIncomeTypeCode3
  {
    get => unearnedIncomeTypeCode3;
    set => unearnedIncomeTypeCode3 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeTypeCode3_MaxLength)) : null;
  }

  /// <summary>Length of the UNEARNED_INCOME_VERIFI_CD_3 attribute.</summary>
  public const int UnearnedIncomeVerifiCd3_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_VERIFI_CD_3 attribute.
  /// This field contains a code that indicates if the unearned income 
  /// allegations of the recipient have been verified. Refer to Appendix E, 
  /// Data Dictionary, for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeVerifiCd3")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = UnearnedIncomeVerifiCd3_MaxLength, Optional = true)]
  public string UnearnedIncomeVerifiCd3
  {
    get => unearnedIncomeVerifiCd3;
    set => unearnedIncomeVerifiCd3 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeVerifiCd3_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_START_DATE_3 attribute.
  /// This field contains the third occurrence of the monthly-unearned income 
  /// payment date. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStartDate3")]
  [Member(Index = 36, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStartDate3
  {
    get => unearnedIncomeStartDate3;
    set => unearnedIncomeStartDate3 = value;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_STOP_DATE_3 attribute.
  /// This field contains the termination date of the third occurrence of 
  /// monthly unearned income. The date is in CCYYMM01 format. In situations
  /// where the unearned income amount changes, this field contains the last
  /// date that the previous rate, or a one-time payment, was received.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStopDate3")]
  [Member(Index = 37, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStopDate3
  {
    get => unearnedIncomeStopDate3;
    set => unearnedIncomeStopDate3 = value;
  }

  /// <summary>Length of the UNEARNED_INCOME_TYPE_CODE_4 attribute.</summary>
  public const int UnearnedIncomeTypeCode4_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_TYPE_CODE_4 attribute.
  /// This field contains a code that indicates the type of unearned income the 
  /// recipient is, or was, receiving. Refer to Appendix E, Data Dictionary,
  /// for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeTypeCode4")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = UnearnedIncomeTypeCode4_MaxLength, Optional = true)]
  public string UnearnedIncomeTypeCode4
  {
    get => unearnedIncomeTypeCode4;
    set => unearnedIncomeTypeCode4 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeTypeCode4_MaxLength)) : null;
  }

  /// <summary>Length of the UNEARNED_INCOME_VERIFI_CD_4 attribute.</summary>
  public const int UnearnedIncomeVerifiCd4_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_VERIFI_CD_4 attribute.
  /// This field contains a code that indicates if the unearned income 
  /// allegations of the recipient have been verified. Refer to Appendix E, 
  /// Data Dictionary, for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeVerifiCd4")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = UnearnedIncomeVerifiCd4_MaxLength, Optional = true)]
  public string UnearnedIncomeVerifiCd4
  {
    get => unearnedIncomeVerifiCd4;
    set => unearnedIncomeVerifiCd4 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeVerifiCd4_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_START_DATE_4 attribute.
  /// This field contains the fourth occurrence of the monthly-unearned income 
  /// payment date. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStartDate4")]
  [Member(Index = 40, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStartDate4
  {
    get => unearnedIncomeStartDate4;
    set => unearnedIncomeStartDate4 = value;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_STOP_DATE_4 attribute.
  /// This field contains the termination date of the fourth occurrence of 
  /// monthly unearned income. The date is in CCYYMM01 format. In situations
  /// where the unearned income amount changes, this field contains the last
  /// date that the previous rate, or a one-time payment, was received.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStopDate4")]
  [Member(Index = 41, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStopDate4
  {
    get => unearnedIncomeStopDate4;
    set => unearnedIncomeStopDate4 = value;
  }

  /// <summary>Length of the UNEARNED_INCOME_TYPE_CODE_5 attribute.</summary>
  public const int UnearnedIncomeTypeCode5_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_TYPE_CODE_5 attribute.
  /// This field contains a code that indicates the type of unearned income the 
  /// recipient is, or was, receiving. Refer to Appendix E, Data Dictionary,
  /// for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeTypeCode5")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = UnearnedIncomeTypeCode5_MaxLength, Optional = true)]
  public string UnearnedIncomeTypeCode5
  {
    get => unearnedIncomeTypeCode5;
    set => unearnedIncomeTypeCode5 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeTypeCode5_MaxLength)) : null;
  }

  /// <summary>Length of the UNEARNED_INCOME_VERIFI_CD_5 attribute.</summary>
  public const int UnearnedIncomeVerifiCd5_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_VERIFI_CD_5 attribute.
  /// This field contains a code that indicates if the unearned income 
  /// allegations of the recipient have been verified. Refer to Appendix E, 
  /// Data Dictionary, for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeVerifiCd5")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = UnearnedIncomeVerifiCd5_MaxLength, Optional = true)]
  public string UnearnedIncomeVerifiCd5
  {
    get => unearnedIncomeVerifiCd5;
    set => unearnedIncomeVerifiCd5 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeVerifiCd5_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_START_DATE_5 attribute.
  /// This field contains the fifth occurrence of the monthly-unearned income 
  /// payment date. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStartDate5")]
  [Member(Index = 44, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStartDate5
  {
    get => unearnedIncomeStartDate5;
    set => unearnedIncomeStartDate5 = value;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_STOP_DATE_5 attribute.
  /// This field contains the termination date of the fifth occurrence of 
  /// monthly unearned income. The date is in CCYYMM01 format. In situations
  /// where the unearned income amount changes, this field contains the last
  /// date that the previous rate, or a one-time payment, was received.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStopDate5")]
  [Member(Index = 45, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStopDate5
  {
    get => unearnedIncomeStopDate5;
    set => unearnedIncomeStopDate5 = value;
  }

  /// <summary>Length of the UNEARNED_INCOME_TYPE_CODE_6 attribute.</summary>
  public const int UnearnedIncomeTypeCode6_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_TYPE_CODE_6 attribute.
  /// This field contains a code that indicates the type of unearned income the 
  /// recipient is, or was, receiving. Refer to Appendix E, Data Dictionary,
  /// for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeTypeCode6")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = UnearnedIncomeTypeCode6_MaxLength, Optional = true)]
  public string UnearnedIncomeTypeCode6
  {
    get => unearnedIncomeTypeCode6;
    set => unearnedIncomeTypeCode6 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeTypeCode6_MaxLength)) : null;
  }

  /// <summary>Length of the UNEARNED_INCOME_VERIFI_CD_6 attribute.</summary>
  public const int UnearnedIncomeVerifiCd6_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_VERIFI_CD_6 attribute.
  /// This field contains a code that indicates if the unearned income 
  /// allegations of the recipient have been verified. Refer to Appendix E, 
  /// Data Dictionary, for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeVerifiCd6")]
  [Member(Index = 47, Type = MemberType.Char, Length
    = UnearnedIncomeVerifiCd6_MaxLength, Optional = true)]
  public string UnearnedIncomeVerifiCd6
  {
    get => unearnedIncomeVerifiCd6;
    set => unearnedIncomeVerifiCd6 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeVerifiCd6_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_START_DATE_6 attribute.
  /// This field contains the sixth occurrence of the monthly-unearned income 
  /// payment date. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStartDate6")]
  [Member(Index = 48, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStartDate6
  {
    get => unearnedIncomeStartDate6;
    set => unearnedIncomeStartDate6 = value;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_STOP_DATE_6 attribute.
  /// This field contains the termination date of the sixth occurrence of 
  /// monthly unearned income. The date is in CCYYMM01 format. In situations
  /// where the unearned income amount changes, this field contains the last
  /// date that the previous rate, or a one-time payment, was received.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStopDate6")]
  [Member(Index = 49, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStopDate6
  {
    get => unearnedIncomeStopDate6;
    set => unearnedIncomeStopDate6 = value;
  }

  /// <summary>Length of the UNEARNED_INCOME_TYPE_CODE_7 attribute.</summary>
  public const int UnearnedIncomeTypeCode7_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_TYPE_CODE_7 attribute.
  /// This field contains a code that indicates the type of unearned income the 
  /// recipient is, or was, receiving. Refer to Appendix E, Data Dictionary,
  /// for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeTypeCode7")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = UnearnedIncomeTypeCode7_MaxLength, Optional = true)]
  public string UnearnedIncomeTypeCode7
  {
    get => unearnedIncomeTypeCode7;
    set => unearnedIncomeTypeCode7 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeTypeCode7_MaxLength)) : null;
  }

  /// <summary>Length of the UNEARNED_INCOME_VERIFI_CD_7 attribute.</summary>
  public const int UnearnedIncomeVerifiCd7_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_VERIFI_CD_7 attribute.
  /// This field contains a code that indicates if the unearned income 
  /// allegations of the recipient have been verified. Refer to Appendix E, 
  /// Data Dictionary, for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeVerifiCd7")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = UnearnedIncomeVerifiCd7_MaxLength, Optional = true)]
  public string UnearnedIncomeVerifiCd7
  {
    get => unearnedIncomeVerifiCd7;
    set => unearnedIncomeVerifiCd7 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeVerifiCd7_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_START_DATE_7 attribute.
  /// This field contains the seventh occurrence of the monthly-unearned income 
  /// payment date. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStartDate7")]
  [Member(Index = 52, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStartDate7
  {
    get => unearnedIncomeStartDate7;
    set => unearnedIncomeStartDate7 = value;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_STOP_DATE_7 attribute.
  /// This field contains the termination date of the seventh occurrence of 
  /// monthly unearned income. The date is in CCYYMM01 format. In situations
  /// where the unearned income amount changes, this field contains the last
  /// date that the previous rate, or a one-time payment, was received.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStopDate7")]
  [Member(Index = 53, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStopDate7
  {
    get => unearnedIncomeStopDate7;
    set => unearnedIncomeStopDate7 = value;
  }

  /// <summary>Length of the UNEARNED_INCOME_TYPE_CODE_8 attribute.</summary>
  public const int UnearnedIncomeTypeCode8_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_TYPE_CODE_8 attribute.
  /// This field contains a code that indicates the type of unearned income the 
  /// recipient is, or was, receiving. Refer to Appendix E, Data Dictionary,
  /// for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeTypeCode8")]
  [Member(Index = 54, Type = MemberType.Char, Length
    = UnearnedIncomeTypeCode8_MaxLength, Optional = true)]
  public string UnearnedIncomeTypeCode8
  {
    get => unearnedIncomeTypeCode8;
    set => unearnedIncomeTypeCode8 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeTypeCode8_MaxLength)) : null;
  }

  /// <summary>Length of the UNEARNED_INCOME_VERIFI_CD8 attribute.</summary>
  public const int UnearnedIncomeVerifiCd8_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_VERIFI_CD8 attribute.
  /// This field contains a code that indicates if the unearned income 
  /// allegations of the recipient have been verified. Refer to Appendix E, 
  /// Data Dictionary, for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeVerifiCd8")]
  [Member(Index = 55, Type = MemberType.Char, Length
    = UnearnedIncomeVerifiCd8_MaxLength, Optional = true)]
  public string UnearnedIncomeVerifiCd8
  {
    get => unearnedIncomeVerifiCd8;
    set => unearnedIncomeVerifiCd8 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeVerifiCd8_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_START_DATE_8 attribute.
  /// This field contains the eighth occurrence of the monthly-unearned income 
  /// payment date. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStartDate8")]
  [Member(Index = 56, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStartDate8
  {
    get => unearnedIncomeStartDate8;
    set => unearnedIncomeStartDate8 = value;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_STOP_DATE_8 attribute.
  /// This field contains the termination date of the eighth occurrence of 
  /// monthly unearned income. The date is in CCYYMM01 format. In situations
  /// where the unearned income amount changes, this field contains the last
  /// date that the previous rate, or a one-time payment, was received.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStopDate8")]
  [Member(Index = 57, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStopDate8
  {
    get => unearnedIncomeStopDate8;
    set => unearnedIncomeStopDate8 = value;
  }

  /// <summary>Length of the UNEARNED_INCOME_TYPE_CODE_9 attribute.</summary>
  public const int UnearnedIncomeTypeCode9_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_TYPE_CODE_9 attribute.
  /// This field contains a code that indicates the type of unearned income the 
  /// recipient is, or was, receiving. Refer to Appendix E, Data Dictionary,
  /// for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeTypeCode9")]
  [Member(Index = 58, Type = MemberType.Char, Length
    = UnearnedIncomeTypeCode9_MaxLength, Optional = true)]
  public string UnearnedIncomeTypeCode9
  {
    get => unearnedIncomeTypeCode9;
    set => unearnedIncomeTypeCode9 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeTypeCode9_MaxLength)) : null;
  }

  /// <summary>Length of the UNEARNED_INCOME_VERIFI_CD_9 attribute.</summary>
  public const int UnearnedIncomeVerifiCd9_MaxLength = 1;

  /// <summary>
  /// The value of the UNEARNED_INCOME_VERIFI_CD_9 attribute.
  /// This field contains a code that indicates if the unearned income 
  /// allegations of the recipient have been verified. Refer to Appendix E, 
  /// Data Dictionary, for the list of valid values and their descriptions.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeVerifiCd9")]
  [Member(Index = 59, Type = MemberType.Char, Length
    = UnearnedIncomeVerifiCd9_MaxLength, Optional = true)]
  public string UnearnedIncomeVerifiCd9
  {
    get => unearnedIncomeVerifiCd9;
    set => unearnedIncomeVerifiCd9 = value != null
      ? TrimEnd(Substring(value, 1, UnearnedIncomeVerifiCd9_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_START_DATE_9 attribute.
  /// This field contains the ninth occurrence of the monthly-unearned income 
  /// payment date. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStartDate9")]
  [Member(Index = 60, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStartDate9
  {
    get => unearnedIncomeStartDate9;
    set => unearnedIncomeStartDate9 = value;
  }

  /// <summary>
  /// The value of the UNEARNED_INCOME_STOP_DATE_9 attribute.
  /// This field contains the termination date of the ninth occurrence of 
  /// monthly unearned income. The date is in CCYYMM01 format. In situations
  /// where the unearned income amount changes, this field contains the last
  /// date that the previous rate, or a one-time payment, was received.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("unearnedIncomeStopDate9")]
  [Member(Index = 61, Type = MemberType.Date, Optional = true)]
  public DateTime? UnearnedIncomeStopDate9
  {
    get => unearnedIncomeStopDate9;
    set => unearnedIncomeStopDate9 = value;
  }

  /// <summary>
  /// The value of the PHIST_NUMBER_OF_ENTRIES attribute.
  /// This field contains the number of occurrences of the Payment History (
  /// PHIST) Table fields. The fields comprise the Payment History Table: PHIST
  /// Payment Date, SSI Monthly Assistance Amount, and PHIST Payment Flag. Each
  /// occurrence of data indicates a change in the payment amount. This field
  /// contains the values 0 through 8.
  /// </summary>
  [JsonPropertyName("phistNumberOfEntries")]
  [Member(Index = 62, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? PhistNumberOfEntries
  {
    get => phistNumberOfEntries;
    set => phistNumberOfEntries = value;
  }

  /// <summary>
  /// The value of the PHIST_PAYMENT_DATE_1 attribute.
  /// This field contains the first date that payment or recovery was made. The 
  /// date is in CCYYMMDD format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("phistPaymentDate1")]
  [Member(Index = 63, Type = MemberType.Date, Optional = true)]
  public DateTime? PhistPaymentDate1
  {
    get => phistPaymentDate1;
    set => phistPaymentDate1 = value;
  }

  /// <summary>
  /// The value of the SSI_MONTHLY_ASSISTANCE_AMOUNT_1 attribute.
  /// This field contains the SSI Monthly Assistance Amount in signed COBOL 
  /// format S9(5)V99. Negative values can be present.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("ssiMonthlyAssistanceAmount1")]
  [Member(Index = 64, Type = MemberType.Number, Length = 7, Precision = 2, Optional
    = true)]
  public decimal? SsiMonthlyAssistanceAmount1
  {
    get => ssiMonthlyAssistanceAmount1;
    set => ssiMonthlyAssistanceAmount1 = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the PHIST_PAYMENT_PAY_FLAG_1 attribute.</summary>
  public const int PhistPaymentPayFlag1_MaxLength = 1;

  /// <summary>
  /// The value of the PHIST_PAYMENT_PAY_FLAG_1 attribute.
  /// This field contains a code that indicates the type of payment made and if 
  /// it is has been returned. Refer to Appendix E, Data Dictionary, for the
  /// list of valid values and their descriptions.
  /// If no code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("phistPaymentPayFlag1")]
  [Member(Index = 65, Type = MemberType.Char, Length
    = PhistPaymentPayFlag1_MaxLength, Optional = true)]
  public string PhistPaymentPayFlag1
  {
    get => phistPaymentPayFlag1;
    set => phistPaymentPayFlag1 = value != null
      ? TrimEnd(Substring(value, 1, PhistPaymentPayFlag1_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHIST_PAYMENT_DATE_2 attribute.
  /// This field contains the second date that payment or recovery was made. The
  /// date is in CCYYMMDD format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("phistPaymentDate2")]
  [Member(Index = 66, Type = MemberType.Date, Optional = true)]
  public DateTime? PhistPaymentDate2
  {
    get => phistPaymentDate2;
    set => phistPaymentDate2 = value;
  }

  /// <summary>
  /// The value of the SSI_MONTHLY_ASSISTANCE_AMOUNT_2 attribute.
  /// This field contains the SSI Monthly Assistance Amount in signed COBOL 
  /// format S9(5)V99. Negative values can be present.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("ssiMonthlyAssistanceAmount2")]
  [Member(Index = 67, Type = MemberType.Number, Length = 7, Precision = 2, Optional
    = true)]
  public decimal? SsiMonthlyAssistanceAmount2
  {
    get => ssiMonthlyAssistanceAmount2;
    set => ssiMonthlyAssistanceAmount2 = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the PHIST_PAYMENT_PAY_FLAG_2 attribute.</summary>
  public const int PhistPaymentPayFlag2_MaxLength = 1;

  /// <summary>
  /// The value of the PHIST_PAYMENT_PAY_FLAG_2 attribute.
  /// This field contains a code that indicates the type of payment made and if 
  /// it is has been returned. Refer to Appendix E, Data Dictionary, for the
  /// list of valid values and their descriptions.
  /// If no code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("phistPaymentPayFlag2")]
  [Member(Index = 68, Type = MemberType.Char, Length
    = PhistPaymentPayFlag2_MaxLength, Optional = true)]
  public string PhistPaymentPayFlag2
  {
    get => phistPaymentPayFlag2;
    set => phistPaymentPayFlag2 = value != null
      ? TrimEnd(Substring(value, 1, PhistPaymentPayFlag2_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHIST_PAYMENT_DATE_3 attribute.
  /// This field contains the third date that payment or recovery was made. The 
  /// date is in CCYYMMDD format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("phistPaymentDate3")]
  [Member(Index = 69, Type = MemberType.Date, Optional = true)]
  public DateTime? PhistPaymentDate3
  {
    get => phistPaymentDate3;
    set => phistPaymentDate3 = value;
  }

  /// <summary>
  /// The value of the SSI_MONTHLY_ASSISTANCE_AMOUNT_3 attribute.
  /// This field contains the SSI Monthly Assistance Amount in signed COBOL 
  /// format S9(5)V99. Negative values can be present.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("ssiMonthlyAssistanceAmount3")]
  [Member(Index = 70, Type = MemberType.Number, Length = 7, Precision = 2, Optional
    = true)]
  public decimal? SsiMonthlyAssistanceAmount3
  {
    get => ssiMonthlyAssistanceAmount3;
    set => ssiMonthlyAssistanceAmount3 = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the PHIST_PAYMENT_PAY_FLAG_3 attribute.</summary>
  public const int PhistPaymentPayFlag3_MaxLength = 1;

  /// <summary>
  /// The value of the PHIST_PAYMENT_PAY_FLAG_3 attribute.
  /// This field contains a code that indicates the type of payment made and if 
  /// it is has been returned. Refer to Appendix E, Data Dictionary, for the
  /// list of valid values and their descriptions.
  /// If no code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("phistPaymentPayFlag3")]
  [Member(Index = 71, Type = MemberType.Char, Length
    = PhistPaymentPayFlag3_MaxLength, Optional = true)]
  public string PhistPaymentPayFlag3
  {
    get => phistPaymentPayFlag3;
    set => phistPaymentPayFlag3 = value != null
      ? TrimEnd(Substring(value, 1, PhistPaymentPayFlag3_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHIST_PAYMENT_DATE_4 attribute.
  /// This field contains the fourth date that payment or recovery was made. The
  /// date is in CCYYMMDD format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("phistPaymentDate4")]
  [Member(Index = 72, Type = MemberType.Date, Optional = true)]
  public DateTime? PhistPaymentDate4
  {
    get => phistPaymentDate4;
    set => phistPaymentDate4 = value;
  }

  /// <summary>
  /// The value of the SSI_MONTHLY_ASSISTANCE_AMOUNT_4 attribute.
  /// This field contains the SSI Monthly Assistance Amount in signed COBOL 
  /// format S9(5)V99. Negative values can be present.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("ssiMonthlyAssistanceAmount4")]
  [Member(Index = 73, Type = MemberType.Number, Length = 7, Precision = 2, Optional
    = true)]
  public decimal? SsiMonthlyAssistanceAmount4
  {
    get => ssiMonthlyAssistanceAmount4;
    set => ssiMonthlyAssistanceAmount4 = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the PHIST_PAYMENT_PAY_FLAG_4 attribute.</summary>
  public const int PhistPaymentPayFlag4_MaxLength = 1;

  /// <summary>
  /// The value of the PHIST_PAYMENT_PAY_FLAG_4 attribute.
  /// This field contains a code that indicates the type of payment made and if 
  /// it is has been returned. Refer to Appendix E, Data Dictionary, for the
  /// list of valid values and their descriptions.
  /// If no code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("phistPaymentPayFlag4")]
  [Member(Index = 74, Type = MemberType.Char, Length
    = PhistPaymentPayFlag4_MaxLength, Optional = true)]
  public string PhistPaymentPayFlag4
  {
    get => phistPaymentPayFlag4;
    set => phistPaymentPayFlag4 = value != null
      ? TrimEnd(Substring(value, 1, PhistPaymentPayFlag4_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHIST_PAYMENT_DATE_5 attribute.
  /// This field contains the fifth date that payment or recovery was made. The 
  /// date is in CCYYMMDD format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("phistPaymentDate5")]
  [Member(Index = 75, Type = MemberType.Date, Optional = true)]
  public DateTime? PhistPaymentDate5
  {
    get => phistPaymentDate5;
    set => phistPaymentDate5 = value;
  }

  /// <summary>
  /// The value of the SSI_MONTHLY_ASSISTANCE_AMOUNT_5 attribute.
  /// This field contains the SSI Monthly Assistance Amount in signed COBOL 
  /// format S9(5)V99. Negative values can be present.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("ssiMonthlyAssistanceAmount5")]
  [Member(Index = 76, Type = MemberType.Number, Length = 7, Precision = 2, Optional
    = true)]
  public decimal? SsiMonthlyAssistanceAmount5
  {
    get => ssiMonthlyAssistanceAmount5;
    set => ssiMonthlyAssistanceAmount5 = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the PHIST_PAYMENT_PAY_FLAG_5 attribute.</summary>
  public const int PhistPaymentPayFlag5_MaxLength = 1;

  /// <summary>
  /// The value of the PHIST_PAYMENT_PAY_FLAG_5 attribute.
  /// This field contains a code that indicates the type of payment made and if 
  /// it is has been returned. Refer to Appendix E, Data Dictionary, for the
  /// list of valid values and their descriptions.
  /// If no code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("phistPaymentPayFlag5")]
  [Member(Index = 77, Type = MemberType.Char, Length
    = PhistPaymentPayFlag5_MaxLength, Optional = true)]
  public string PhistPaymentPayFlag5
  {
    get => phistPaymentPayFlag5;
    set => phistPaymentPayFlag5 = value != null
      ? TrimEnd(Substring(value, 1, PhistPaymentPayFlag5_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHIST_PAYMENT_DATE_6 attribute.
  /// This field contains the sixth date that payment or recovery was made. The 
  /// date is in CCYYMMDD format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("phistPaymentDate6")]
  [Member(Index = 78, Type = MemberType.Date, Optional = true)]
  public DateTime? PhistPaymentDate6
  {
    get => phistPaymentDate6;
    set => phistPaymentDate6 = value;
  }

  /// <summary>
  /// The value of the SSI_MONTHLY_ASSISTANCE_AMOUNT_6 attribute.
  /// This field contains the SSI Monthly Assistance Amount in signed COBOL 
  /// format S9(5)V99. Negative values can be present.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("ssiMonthlyAssistanceAmount6")]
  [Member(Index = 79, Type = MemberType.Number, Length = 7, Precision = 2, Optional
    = true)]
  public decimal? SsiMonthlyAssistanceAmount6
  {
    get => ssiMonthlyAssistanceAmount6;
    set => ssiMonthlyAssistanceAmount6 = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the PHIST_PAYMENT_PAY_FLAG_6 attribute.</summary>
  public const int PhistPaymentPayFlag6_MaxLength = 1;

  /// <summary>
  /// The value of the PHIST_PAYMENT_PAY_FLAG_6 attribute.
  /// This field contains a code that indicates the type of payment made and if 
  /// it is has been returned. Refer to Appendix E, Data Dictionary, for the
  /// list of valid values and their descriptions.
  /// If no code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("phistPaymentPayFlag6")]
  [Member(Index = 80, Type = MemberType.Char, Length
    = PhistPaymentPayFlag6_MaxLength, Optional = true)]
  public string PhistPaymentPayFlag6
  {
    get => phistPaymentPayFlag6;
    set => phistPaymentPayFlag6 = value != null
      ? TrimEnd(Substring(value, 1, PhistPaymentPayFlag6_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHIST_PAYMENT_DATE_7 attribute.
  /// This field contains the seventh date that payment or recovery was made. 
  /// The date is in CCYYMMDD format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("phistPaymentDate7")]
  [Member(Index = 81, Type = MemberType.Date, Optional = true)]
  public DateTime? PhistPaymentDate7
  {
    get => phistPaymentDate7;
    set => phistPaymentDate7 = value;
  }

  /// <summary>
  /// The value of the SSI_MONTHLY_ASSISTANCE_AMOUNT_7 attribute.
  /// This field contains the SSI Monthly Assistance Amount in signed COBOL 
  /// format S9(5)V99. Negative values can be present.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("ssiMonthlyAssistanceAmount7")]
  [Member(Index = 82, Type = MemberType.Number, Length = 7, Precision = 2, Optional
    = true)]
  public decimal? SsiMonthlyAssistanceAmount7
  {
    get => ssiMonthlyAssistanceAmount7;
    set => ssiMonthlyAssistanceAmount7 = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the PHIST_PAYMENT_PAY_FLAG_7 attribute.</summary>
  public const int PhistPaymentPayFlag7_MaxLength = 1;

  /// <summary>
  /// The value of the PHIST_PAYMENT_PAY_FLAG_7 attribute.
  /// This field contains a code that indicates the type of payment made and if 
  /// it is has been returned. Refer to Appendix E, Data Dictionary, for the
  /// list of valid values and their descriptions.
  /// If no code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("phistPaymentPayFlag7")]
  [Member(Index = 83, Type = MemberType.Char, Length
    = PhistPaymentPayFlag7_MaxLength, Optional = true)]
  public string PhistPaymentPayFlag7
  {
    get => phistPaymentPayFlag7;
    set => phistPaymentPayFlag7 = value != null
      ? TrimEnd(Substring(value, 1, PhistPaymentPayFlag7_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHIST_PAYMENT_DATE_8 attribute.
  /// This field contains the eighth date that payment or recovery was made. The
  /// date is in CCYYMMDD format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces
  /// </summary>
  [JsonPropertyName("phistPaymentDate8")]
  [Member(Index = 84, Type = MemberType.Date, Optional = true)]
  public DateTime? PhistPaymentDate8
  {
    get => phistPaymentDate8;
    set => phistPaymentDate8 = value;
  }

  /// <summary>
  /// The value of the SSI_MONTHLY_ASSISTANCE_AMOUNT_8 attribute.
  /// This field contains the SSI Monthly Assistance Amount in signed COBOL 
  /// format S9(5)V99. Negative values can be present.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("ssiMonthlyAssistanceAmount8")]
  [Member(Index = 85, Type = MemberType.Number, Length = 7, Precision = 2, Optional
    = true)]
  public decimal? SsiMonthlyAssistanceAmount8
  {
    get => ssiMonthlyAssistanceAmount8;
    set => ssiMonthlyAssistanceAmount8 = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the PHIST_PAYMENT_PAY_FLAG_8 attribute.</summary>
  public const int PhistPaymentPayFlag8_MaxLength = 1;

  /// <summary>
  /// The value of the PHIST_PAYMENT_PAY_FLAG_8 attribute.
  /// This field contains a code that indicates the type of payment made and if 
  /// it is has been returned. Refer to Appendix E, Data Dictionary, for the
  /// list of valid values and their descriptions.
  /// If no code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("phistPaymentPayFlag8")]
  [Member(Index = 86, Type = MemberType.Char, Length
    = PhistPaymentPayFlag8_MaxLength, Optional = true)]
  public string PhistPaymentPayFlag8
  {
    get => phistPaymentPayFlag8;
    set => phistPaymentPayFlag8 = value != null
      ? TrimEnd(Substring(value, 1, PhistPaymentPayFlag8_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 87, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 88, Type = MemberType.Timestamp)]
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
  [Member(Index = 89, Type = MemberType.Char, Length
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
  [Member(Index = 90, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_RESPONSE_AGENCY_CO attribute.
  /// </summary>
  public const int FcgLSRspAgy_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_RESPONSE_AGENCY_CO attribute.
  /// This field contains the code that identifies the Locate Source.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 91, Type = MemberType.Char, Length = FcgLSRspAgy_MaxLength)]
  public string FcgLSRspAgy
  {
    get => fcgLSRspAgy ?? "";
    set => fcgLSRspAgy = TrimEnd(Substring(value, 1, FcgLSRspAgy_MaxLength));
  }

  /// <summary>
  /// The json value of the FcgLSRspAgy attribute.</summary>
  [JsonPropertyName("fcgLSRspAgy")]
  [Computed]
  public string FcgLSRspAgy_Json
  {
    get => NullIf(FcgLSRspAgy, "");
    set => FcgLSRspAgy = value;
  }

  /// <summary>Length of the MEMBER_ID attribute.</summary>
  public const int FcgMemberId_MaxLength = 15;

  /// <summary>
  /// The value of the MEMBER_ID attribute.
  /// This field contains the Member ID that was provided by the submitter of 
  /// the Locate Request.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 92, Type = MemberType.Char, Length = FcgMemberId_MaxLength)]
  public string FcgMemberId
  {
    get => fcgMemberId ?? "";
    set => fcgMemberId = TrimEnd(Substring(value, 1, FcgMemberId_MaxLength));
  }

  /// <summary>
  /// The json value of the FcgMemberId attribute.</summary>
  [JsonPropertyName("fcgMemberId")]
  [Computed]
  public string FcgMemberId_Json
  {
    get => NullIf(FcgMemberId, "");
    set => FcgMemberId = value;
  }

  private int seqNo;
  private string otherName;
  private string raceCode;
  private string dateOfDeathSourceCode;
  private string payeeStateOfJurisdiction;
  private string payeeCountyOfJurisdiction;
  private string payeeDistrictOfficeCode;
  private string typeOfPayeeCode;
  private string typeOfRecipient;
  private DateTime? recordEstablishmentDate;
  private DateTime? dateOfTitleXviEligibility;
  private string titleXviAppealCode;
  private DateTime? dateOfTitleXviAppeal;
  private DateTime? titleXviLastRedeterminDate;
  private DateTime? titleXviDenialDate;
  private string currentPaymentStatusCode;
  private string paymentStatusCode;
  private DateTime? paymentStatusDate;
  private string telephoneNumber;
  private string thirdPartyInsuranceIndicator;
  private string directDepositIndicator;
  private string representativePayeeIndicator;
  private string custodyCode;
  private decimal? estimatedSelfEmploymentAmount;
  private int? unearnedIncomeNumOfEntries;
  private string unearnedIncomeTypeCode1;
  private string unearnedIncomeVerifiCd1;
  private DateTime? unearnedIncomeStartDate1;
  private DateTime? unearnedIncomeStopDate1;
  private string unearnedIncomeTypeCode2;
  private string unearnedIncomeVerifiCd2;
  private DateTime? unearnedIncomeStartDate2;
  private DateTime? unearnedIncomeStopDate2;
  private string unearnedIncomeTypeCode3;
  private string unearnedIncomeVerifiCd3;
  private DateTime? unearnedIncomeStartDate3;
  private DateTime? unearnedIncomeStopDate3;
  private string unearnedIncomeTypeCode4;
  private string unearnedIncomeVerifiCd4;
  private DateTime? unearnedIncomeStartDate4;
  private DateTime? unearnedIncomeStopDate4;
  private string unearnedIncomeTypeCode5;
  private string unearnedIncomeVerifiCd5;
  private DateTime? unearnedIncomeStartDate5;
  private DateTime? unearnedIncomeStopDate5;
  private string unearnedIncomeTypeCode6;
  private string unearnedIncomeVerifiCd6;
  private DateTime? unearnedIncomeStartDate6;
  private DateTime? unearnedIncomeStopDate6;
  private string unearnedIncomeTypeCode7;
  private string unearnedIncomeVerifiCd7;
  private DateTime? unearnedIncomeStartDate7;
  private DateTime? unearnedIncomeStopDate7;
  private string unearnedIncomeTypeCode8;
  private string unearnedIncomeVerifiCd8;
  private DateTime? unearnedIncomeStartDate8;
  private DateTime? unearnedIncomeStopDate8;
  private string unearnedIncomeTypeCode9;
  private string unearnedIncomeVerifiCd9;
  private DateTime? unearnedIncomeStartDate9;
  private DateTime? unearnedIncomeStopDate9;
  private int? phistNumberOfEntries;
  private DateTime? phistPaymentDate1;
  private decimal? ssiMonthlyAssistanceAmount1;
  private string phistPaymentPayFlag1;
  private DateTime? phistPaymentDate2;
  private decimal? ssiMonthlyAssistanceAmount2;
  private string phistPaymentPayFlag2;
  private DateTime? phistPaymentDate3;
  private decimal? ssiMonthlyAssistanceAmount3;
  private string phistPaymentPayFlag3;
  private DateTime? phistPaymentDate4;
  private decimal? ssiMonthlyAssistanceAmount4;
  private string phistPaymentPayFlag4;
  private DateTime? phistPaymentDate5;
  private decimal? ssiMonthlyAssistanceAmount5;
  private string phistPaymentPayFlag5;
  private DateTime? phistPaymentDate6;
  private decimal? ssiMonthlyAssistanceAmount6;
  private string phistPaymentPayFlag6;
  private DateTime? phistPaymentDate7;
  private decimal? ssiMonthlyAssistanceAmount7;
  private string phistPaymentPayFlag7;
  private DateTime? phistPaymentDate8;
  private decimal? ssiMonthlyAssistanceAmount8;
  private string phistPaymentPayFlag8;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string fcgLSRspAgy;
  private string fcgMemberId;
}
