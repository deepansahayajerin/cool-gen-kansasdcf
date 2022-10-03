// The source file: INTERSTATE_AP_LOCATE, ID: 371435929, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT	
/// Information about a AP's current and previous employers and addresses that 
/// was received or sent on an interstate case and transmitted through CSENet.
/// </summary>
[Serializable]
public partial class InterstateApLocate: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateApLocate()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateApLocate(InterstateApLocate that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateApLocate Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstateApLocate that)
  {
    base.Assign(that);
    residentialAddressLine1 = that.residentialAddressLine1;
    residentialAddressLine2 = that.residentialAddressLine2;
    residentialCity = that.residentialCity;
    residentialState = that.residentialState;
    residentialZipCode5 = that.residentialZipCode5;
    residentialZipCode4 = that.residentialZipCode4;
    mailingAddressLine1 = that.mailingAddressLine1;
    mailingAddressLine2 = that.mailingAddressLine2;
    mailingCity = that.mailingCity;
    mailingState = that.mailingState;
    mailingZipCode5 = that.mailingZipCode5;
    mailingZipCode4 = that.mailingZipCode4;
    residentialAddressEffectvDate = that.residentialAddressEffectvDate;
    residentialAddressEndDate = that.residentialAddressEndDate;
    residentialAddressConfirmInd = that.residentialAddressConfirmInd;
    mailingAddressEffectiveDate = that.mailingAddressEffectiveDate;
    mailingAddressEndDate = that.mailingAddressEndDate;
    mailingAddressConfirmedInd = that.mailingAddressConfirmedInd;
    homePhoneNumber = that.homePhoneNumber;
    workPhoneNumber = that.workPhoneNumber;
    driversLicState = that.driversLicState;
    driversLicenseNum = that.driversLicenseNum;
    alias1FirstName = that.alias1FirstName;
    alias1MiddleName = that.alias1MiddleName;
    alias1LastName = that.alias1LastName;
    alias1Suffix = that.alias1Suffix;
    alias2FirstName = that.alias2FirstName;
    alias2MiddleName = that.alias2MiddleName;
    alias2LastName = that.alias2LastName;
    alias2Suffix = that.alias2Suffix;
    alias3FirstName = that.alias3FirstName;
    alias3MiddleName = that.alias3MiddleName;
    alias3LastName = that.alias3LastName;
    alias3Suffix = that.alias3Suffix;
    currentSpouseFirstName = that.currentSpouseFirstName;
    currentSpouseMiddleName = that.currentSpouseMiddleName;
    currentSpouseLastName = that.currentSpouseLastName;
    currentSpouseSuffix = that.currentSpouseSuffix;
    occupation = that.occupation;
    employerEin = that.employerEin;
    employerName = that.employerName;
    employerAddressLine1 = that.employerAddressLine1;
    employerAddressLine2 = that.employerAddressLine2;
    employerCity = that.employerCity;
    employerState = that.employerState;
    employerZipCode5 = that.employerZipCode5;
    employerZipCode4 = that.employerZipCode4;
    employerPhoneNum = that.employerPhoneNum;
    employerEffectiveDate = that.employerEffectiveDate;
    employerEndDate = that.employerEndDate;
    employerConfirmedInd = that.employerConfirmedInd;
    wageQtr = that.wageQtr;
    wageYear = that.wageYear;
    wageAmount = that.wageAmount;
    insuranceCarrierName = that.insuranceCarrierName;
    insurancePolicyNum = that.insurancePolicyNum;
    lastResAddressLine1 = that.lastResAddressLine1;
    lastResAddressLine2 = that.lastResAddressLine2;
    lastResCity = that.lastResCity;
    lastResState = that.lastResState;
    lastResZipCode5 = that.lastResZipCode5;
    lastResZipCode4 = that.lastResZipCode4;
    lastResAddressDate = that.lastResAddressDate;
    lastMailAddressLine1 = that.lastMailAddressLine1;
    lastMailAddressLine2 = that.lastMailAddressLine2;
    lastMailCity = that.lastMailCity;
    lastMailState = that.lastMailState;
    lastMailZipCode5 = that.lastMailZipCode5;
    lastMailZipCode4 = that.lastMailZipCode4;
    lastMailAddressDate = that.lastMailAddressDate;
    lastEmployerName = that.lastEmployerName;
    lastEmployerDate = that.lastEmployerDate;
    lastEmployerAddressLine1 = that.lastEmployerAddressLine1;
    lastEmployerAddressLine2 = that.lastEmployerAddressLine2;
    lastEmployerCity = that.lastEmployerCity;
    lastEmployerState = that.lastEmployerState;
    lastEmployerZipCode5 = that.lastEmployerZipCode5;
    lastEmployerZipCode4 = that.lastEmployerZipCode4;
    professionalLicenses = that.professionalLicenses;
    workAreaCode = that.workAreaCode;
    homeAreaCode = that.homeAreaCode;
    lastEmployerEndDate = that.lastEmployerEndDate;
    employerAreaCode = that.employerAreaCode;
    employer2Name = that.employer2Name;
    employer2Ein = that.employer2Ein;
    employer2PhoneNumber = that.employer2PhoneNumber;
    employer2AreaCode = that.employer2AreaCode;
    employer2AddressLine1 = that.employer2AddressLine1;
    employer2AddressLine2 = that.employer2AddressLine2;
    employer2City = that.employer2City;
    employer2State = that.employer2State;
    employer2ZipCode5 = that.employer2ZipCode5;
    employer2ZipCode4 = that.employer2ZipCode4;
    employer2ConfirmedIndicator = that.employer2ConfirmedIndicator;
    employer2EffectiveDate = that.employer2EffectiveDate;
    employer2EndDate = that.employer2EndDate;
    employer2WageAmount = that.employer2WageAmount;
    employer2WageQuarter = that.employer2WageQuarter;
    employer2WageYear = that.employer2WageYear;
    employer3Name = that.employer3Name;
    employer3Ein = that.employer3Ein;
    employer3PhoneNumber = that.employer3PhoneNumber;
    employer3AreaCode = that.employer3AreaCode;
    employer3AddressLine1 = that.employer3AddressLine1;
    employer3AddressLine2 = that.employer3AddressLine2;
    employer3City = that.employer3City;
    employer3State = that.employer3State;
    employer3ZipCode5 = that.employer3ZipCode5;
    employer3ZipCode4 = that.employer3ZipCode4;
    employer3ConfirmedIndicator = that.employer3ConfirmedIndicator;
    employer3EffectiveDate = that.employer3EffectiveDate;
    employer3EndDate = that.employer3EndDate;
    employer3WageAmount = that.employer3WageAmount;
    employer3WageQuarter = that.employer3WageQuarter;
    employer3WageYear = that.employer3WageYear;
    cncTransSerlNbr = that.cncTransSerlNbr;
    cncTransactionDt = that.cncTransactionDt;
  }

  /// <summary>Length of the RESIDENTIAL_ADDRESS_LINE_1 attribute.</summary>
  public const int ResidentialAddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the RESIDENTIAL_ADDRESS_LINE_1 attribute.
  /// *** DRAFT ***
  /// The first line of the address.
  /// </summary>
  [JsonPropertyName("residentialAddressLine1")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = ResidentialAddressLine1_MaxLength, Optional = true)]
  public string ResidentialAddressLine1
  {
    get => residentialAddressLine1;
    set => residentialAddressLine1 = value != null
      ? TrimEnd(Substring(value, 1, ResidentialAddressLine1_MaxLength)) : null;
  }

  /// <summary>Length of the RESIDENTIAL_ADDRESS_LINE_2 attribute.</summary>
  public const int ResidentialAddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the RESIDENTIAL_ADDRESS_LINE_2 attribute.
  /// *** DRAFT ***
  /// The second line of the address.
  /// </summary>
  [JsonPropertyName("residentialAddressLine2")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = ResidentialAddressLine2_MaxLength, Optional = true)]
  public string ResidentialAddressLine2
  {
    get => residentialAddressLine2;
    set => residentialAddressLine2 = value != null
      ? TrimEnd(Substring(value, 1, ResidentialAddressLine2_MaxLength)) : null;
  }

  /// <summary>Length of the RESIDENTIAL_CITY attribute.</summary>
  public const int ResidentialCity_MaxLength = 15;

  /// <summary>
  /// The value of the RESIDENTIAL_CITY attribute.
  /// *** DRAFT ***
  /// The city part of the address.
  /// </summary>
  [JsonPropertyName("residentialCity")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = ResidentialCity_MaxLength, Optional = true)]
  public string ResidentialCity
  {
    get => residentialCity;
    set => residentialCity = value != null
      ? TrimEnd(Substring(value, 1, ResidentialCity_MaxLength)) : null;
  }

  /// <summary>Length of the RESIDENTIAL_STATE attribute.</summary>
  public const int ResidentialState_MaxLength = 2;

  /// <summary>
  /// The value of the RESIDENTIAL_STATE attribute.
  /// *** DRAFT ***
  /// The state part of the address.
  /// </summary>
  [JsonPropertyName("residentialState")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = ResidentialState_MaxLength, Optional = true)]
  public string ResidentialState
  {
    get => residentialState;
    set => residentialState = value != null
      ? TrimEnd(Substring(value, 1, ResidentialState_MaxLength)) : null;
  }

  /// <summary>Length of the RESIDENTIAL_ZIP_CODE_5 attribute.</summary>
  public const int ResidentialZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the RESIDENTIAL_ZIP_CODE_5 attribute.
  /// *** DRAFT ***
  /// The first five digits of the zip code.
  /// </summary>
  [JsonPropertyName("residentialZipCode5")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = ResidentialZipCode5_MaxLength, Optional = true)]
  public string ResidentialZipCode5
  {
    get => residentialZipCode5;
    set => residentialZipCode5 = value != null
      ? TrimEnd(Substring(value, 1, ResidentialZipCode5_MaxLength)) : null;
  }

  /// <summary>Length of the RESIDENTIAL_ZIP_CODE_4 attribute.</summary>
  public const int ResidentialZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the RESIDENTIAL_ZIP_CODE_4 attribute.
  /// *** DRAFT ***
  /// The second part of the zip code.  It consists of four digits.
  /// </summary>
  [JsonPropertyName("residentialZipCode4")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = ResidentialZipCode4_MaxLength, Optional = true)]
  public string ResidentialZipCode4
  {
    get => residentialZipCode4;
    set => residentialZipCode4 = value != null
      ? TrimEnd(Substring(value, 1, ResidentialZipCode4_MaxLength)) : null;
  }

  /// <summary>Length of the MAILING_ADDRESS_LINE_1 attribute.</summary>
  public const int MailingAddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the MAILING_ADDRESS_LINE_1 attribute.
  /// *** DRAFT ***
  /// The first line of the address.
  /// </summary>
  [JsonPropertyName("mailingAddressLine1")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = MailingAddressLine1_MaxLength, Optional = true)]
  public string MailingAddressLine1
  {
    get => mailingAddressLine1;
    set => mailingAddressLine1 = value != null
      ? TrimEnd(Substring(value, 1, MailingAddressLine1_MaxLength)) : null;
  }

  /// <summary>Length of the MAILING_ADDRESS_LINE_2 attribute.</summary>
  public const int MailingAddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the MAILING_ADDRESS_LINE_2 attribute.
  /// *** DRAFT ***
  /// The second line of the address.
  /// </summary>
  [JsonPropertyName("mailingAddressLine2")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = MailingAddressLine2_MaxLength, Optional = true)]
  public string MailingAddressLine2
  {
    get => mailingAddressLine2;
    set => mailingAddressLine2 = value != null
      ? TrimEnd(Substring(value, 1, MailingAddressLine2_MaxLength)) : null;
  }

  /// <summary>Length of the MAILING_CITY attribute.</summary>
  public const int MailingCity_MaxLength = 15;

  /// <summary>
  /// The value of the MAILING_CITY attribute.
  /// *** DRAFT ***
  /// The city part of the address.
  /// </summary>
  [JsonPropertyName("mailingCity")]
  [Member(Index = 9, Type = MemberType.Char, Length = MailingCity_MaxLength, Optional
    = true)]
  public string MailingCity
  {
    get => mailingCity;
    set => mailingCity = value != null
      ? TrimEnd(Substring(value, 1, MailingCity_MaxLength)) : null;
  }

  /// <summary>Length of the MAILING_STATE attribute.</summary>
  public const int MailingState_MaxLength = 2;

  /// <summary>
  /// The value of the MAILING_STATE attribute.
  /// *** DRAFT ***
  /// The state part of the address.
  /// </summary>
  [JsonPropertyName("mailingState")]
  [Member(Index = 10, Type = MemberType.Char, Length = MailingState_MaxLength, Optional
    = true)]
  public string MailingState
  {
    get => mailingState;
    set => mailingState = value != null
      ? TrimEnd(Substring(value, 1, MailingState_MaxLength)) : null;
  }

  /// <summary>Length of the MAILING_ZIP_CODE_5 attribute.</summary>
  public const int MailingZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the MAILING_ZIP_CODE_5 attribute.
  /// *** DRAFT ***
  /// The first five digits of the zip code.
  /// </summary>
  [JsonPropertyName("mailingZipCode5")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = MailingZipCode5_MaxLength, Optional = true)]
  public string MailingZipCode5
  {
    get => mailingZipCode5;
    set => mailingZipCode5 = value != null
      ? TrimEnd(Substring(value, 1, MailingZipCode5_MaxLength)) : null;
  }

  /// <summary>Length of the MAILING_ZIP_CODE_4 attribute.</summary>
  public const int MailingZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the MAILING_ZIP_CODE_4 attribute.
  /// *** DRAFT ***
  /// The second part of the zip code.  It consists of four digits.
  /// </summary>
  [JsonPropertyName("mailingZipCode4")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = MailingZipCode4_MaxLength, Optional = true)]
  public string MailingZipCode4
  {
    get => mailingZipCode4;
    set => mailingZipCode4 = value != null
      ? TrimEnd(Substring(value, 1, MailingZipCode4_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the RESIDENTIAL_ADDRESS_EFFECTV_DATE attribute.
  /// The Date the address becomes or became effective.
  /// </summary>
  [JsonPropertyName("residentialAddressEffectvDate")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? ResidentialAddressEffectvDate
  {
    get => residentialAddressEffectvDate;
    set => residentialAddressEffectvDate = value;
  }

  /// <summary>
  /// The value of the RESIDENTIAL_ADDRESS_END_DATE attribute.
  /// The date this address is no longer effective.
  /// </summary>
  [JsonPropertyName("residentialAddressEndDate")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? ResidentialAddressEndDate
  {
    get => residentialAddressEndDate;
    set => residentialAddressEndDate = value;
  }

  /// <summary>Length of the RESIDENTIAL_ADDRESS_CONFIRM_IND attribute.
  /// </summary>
  public const int ResidentialAddressConfirmInd_MaxLength = 1;

  /// <summary>
  /// The value of the RESIDENTIAL_ADDRESS_CONFIRM_IND attribute.
  /// Whether or not the address is Verified as correct.
  /// </summary>
  [JsonPropertyName("residentialAddressConfirmInd")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = ResidentialAddressConfirmInd_MaxLength, Optional = true)]
  public string ResidentialAddressConfirmInd
  {
    get => residentialAddressConfirmInd;
    set => residentialAddressConfirmInd = value != null
      ? TrimEnd(Substring(value, 1, ResidentialAddressConfirmInd_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the MAILING_ADDRESS_EFFECTIVE_DATE attribute.
  /// The Date the address becomes or became effective.
  /// </summary>
  [JsonPropertyName("mailingAddressEffectiveDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? MailingAddressEffectiveDate
  {
    get => mailingAddressEffectiveDate;
    set => mailingAddressEffectiveDate = value;
  }

  /// <summary>
  /// The value of the MAILING_ADDRESS_END_DATE attribute.
  /// The date this address is no longer effective.
  /// </summary>
  [JsonPropertyName("mailingAddressEndDate")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
  public DateTime? MailingAddressEndDate
  {
    get => mailingAddressEndDate;
    set => mailingAddressEndDate = value;
  }

  /// <summary>Length of the MAILING_ADDRESS_CONFIRMED_IND attribute.</summary>
  public const int MailingAddressConfirmedInd_MaxLength = 1;

  /// <summary>
  /// The value of the MAILING_ADDRESS_CONFIRMED_IND attribute.
  /// Whether or not the address is Verified as correct.
  /// </summary>
  [JsonPropertyName("mailingAddressConfirmedInd")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = MailingAddressConfirmedInd_MaxLength, Optional = true)]
  public string MailingAddressConfirmedInd
  {
    get => mailingAddressConfirmedInd;
    set => mailingAddressConfirmedInd = value != null
      ? TrimEnd(Substring(value, 1, MailingAddressConfirmedInd_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the HOME_PHONE_NUMBER attribute.
  /// Home phone number
  /// </summary>
  [JsonPropertyName("homePhoneNumber")]
  [Member(Index = 19, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? HomePhoneNumber
  {
    get => homePhoneNumber;
    set => homePhoneNumber = value;
  }

  /// <summary>
  /// The value of the WORK_PHONE_NUMBER attribute.
  /// Work phone number
  /// </summary>
  [JsonPropertyName("workPhoneNumber")]
  [Member(Index = 20, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? WorkPhoneNumber
  {
    get => workPhoneNumber;
    set => workPhoneNumber = value;
  }

  /// <summary>Length of the DRIVERS_LIC_STATE attribute.</summary>
  public const int DriversLicState_MaxLength = 2;

  /// <summary>
  /// The value of the DRIVERS_LIC_STATE attribute.
  /// State issuing drivers' license
  /// </summary>
  [JsonPropertyName("driversLicState")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = DriversLicState_MaxLength, Optional = true)]
  public string DriversLicState
  {
    get => driversLicState;
    set => driversLicState = value != null
      ? TrimEnd(Substring(value, 1, DriversLicState_MaxLength)) : null;
  }

  /// <summary>Length of the DRIVERS_LICENSE_NUM attribute.</summary>
  public const int DriversLicenseNum_MaxLength = 20;

  /// <summary>
  /// The value of the DRIVERS_LICENSE_NUM attribute.
  /// Drivers' license number or Id
  /// </summary>
  [JsonPropertyName("driversLicenseNum")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = DriversLicenseNum_MaxLength, Optional = true)]
  public string DriversLicenseNum
  {
    get => driversLicenseNum;
    set => driversLicenseNum = value != null
      ? TrimEnd(Substring(value, 1, DriversLicenseNum_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_1_FIRST_NAME attribute.</summary>
  public const int Alias1FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the ALIAS_1_FIRST_NAME attribute.
  /// Alias1 first name
  /// </summary>
  [JsonPropertyName("alias1FirstName")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = Alias1FirstName_MaxLength, Optional = true)]
  public string Alias1FirstName
  {
    get => alias1FirstName;
    set => alias1FirstName = value != null
      ? TrimEnd(Substring(value, 1, Alias1FirstName_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_1_MIDDLE_NAME attribute.</summary>
  public const int Alias1MiddleName_MaxLength = 1;

  /// <summary>
  /// The value of the ALIAS_1_MIDDLE_NAME attribute.
  /// Alias1 middle initial
  /// </summary>
  [JsonPropertyName("alias1MiddleName")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = Alias1MiddleName_MaxLength, Optional = true)]
  public string Alias1MiddleName
  {
    get => alias1MiddleName;
    set => alias1MiddleName = value != null
      ? TrimEnd(Substring(value, 1, Alias1MiddleName_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_1_LAST_NAME attribute.</summary>
  public const int Alias1LastName_MaxLength = 17;

  /// <summary>
  /// The value of the ALIAS_1_LAST_NAME attribute.
  /// Alias1 last name
  /// </summary>
  [JsonPropertyName("alias1LastName")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = Alias1LastName_MaxLength, Optional = true)]
  public string Alias1LastName
  {
    get => alias1LastName;
    set => alias1LastName = value != null
      ? TrimEnd(Substring(value, 1, Alias1LastName_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_1_SUFFIX attribute.</summary>
  public const int Alias1Suffix_MaxLength = 3;

  /// <summary>
  /// The value of the ALIAS_1_SUFFIX attribute.
  /// Alias1 name suffix
  /// </summary>
  [JsonPropertyName("alias1Suffix")]
  [Member(Index = 26, Type = MemberType.Char, Length = Alias1Suffix_MaxLength, Optional
    = true)]
  public string Alias1Suffix
  {
    get => alias1Suffix;
    set => alias1Suffix = value != null
      ? TrimEnd(Substring(value, 1, Alias1Suffix_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_2_FIRST_NAME attribute.</summary>
  public const int Alias2FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the ALIAS_2_FIRST_NAME attribute.
  /// Alias2 first name
  /// </summary>
  [JsonPropertyName("alias2FirstName")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = Alias2FirstName_MaxLength, Optional = true)]
  public string Alias2FirstName
  {
    get => alias2FirstName;
    set => alias2FirstName = value != null
      ? TrimEnd(Substring(value, 1, Alias2FirstName_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_2_MIDDLE_NAME attribute.</summary>
  public const int Alias2MiddleName_MaxLength = 1;

  /// <summary>
  /// The value of the ALIAS_2_MIDDLE_NAME attribute.
  /// Alias2 middle initial
  /// </summary>
  [JsonPropertyName("alias2MiddleName")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = Alias2MiddleName_MaxLength, Optional = true)]
  public string Alias2MiddleName
  {
    get => alias2MiddleName;
    set => alias2MiddleName = value != null
      ? TrimEnd(Substring(value, 1, Alias2MiddleName_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_2_LAST_NAME attribute.</summary>
  public const int Alias2LastName_MaxLength = 17;

  /// <summary>
  /// The value of the ALIAS_2_LAST_NAME attribute.
  /// Alias2 last name
  /// </summary>
  [JsonPropertyName("alias2LastName")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = Alias2LastName_MaxLength, Optional = true)]
  public string Alias2LastName
  {
    get => alias2LastName;
    set => alias2LastName = value != null
      ? TrimEnd(Substring(value, 1, Alias2LastName_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_2_SUFFIX attribute.</summary>
  public const int Alias2Suffix_MaxLength = 3;

  /// <summary>
  /// The value of the ALIAS_2_SUFFIX attribute.
  /// Alias2 suffix
  /// </summary>
  [JsonPropertyName("alias2Suffix")]
  [Member(Index = 30, Type = MemberType.Char, Length = Alias2Suffix_MaxLength, Optional
    = true)]
  public string Alias2Suffix
  {
    get => alias2Suffix;
    set => alias2Suffix = value != null
      ? TrimEnd(Substring(value, 1, Alias2Suffix_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_3_FIRST_NAME attribute.</summary>
  public const int Alias3FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the ALIAS_3_FIRST_NAME attribute.
  /// Alias3 first name
  /// </summary>
  [JsonPropertyName("alias3FirstName")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = Alias3FirstName_MaxLength, Optional = true)]
  public string Alias3FirstName
  {
    get => alias3FirstName;
    set => alias3FirstName = value != null
      ? TrimEnd(Substring(value, 1, Alias3FirstName_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_3_MIDDLE_NAME attribute.</summary>
  public const int Alias3MiddleName_MaxLength = 1;

  /// <summary>
  /// The value of the ALIAS_3_MIDDLE_NAME attribute.
  /// Alias3 middle initial
  /// </summary>
  [JsonPropertyName("alias3MiddleName")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = Alias3MiddleName_MaxLength, Optional = true)]
  public string Alias3MiddleName
  {
    get => alias3MiddleName;
    set => alias3MiddleName = value != null
      ? TrimEnd(Substring(value, 1, Alias3MiddleName_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_3_LAST_NAME attribute.</summary>
  public const int Alias3LastName_MaxLength = 17;

  /// <summary>
  /// The value of the ALIAS_3_LAST_NAME attribute.
  /// Alias 3 last name
  /// </summary>
  [JsonPropertyName("alias3LastName")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = Alias3LastName_MaxLength, Optional = true)]
  public string Alias3LastName
  {
    get => alias3LastName;
    set => alias3LastName = value != null
      ? TrimEnd(Substring(value, 1, Alias3LastName_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_3_SUFFIX attribute.</summary>
  public const int Alias3Suffix_MaxLength = 3;

  /// <summary>
  /// The value of the ALIAS_3_SUFFIX attribute.
  /// Alias3 suffix
  /// </summary>
  [JsonPropertyName("alias3Suffix")]
  [Member(Index = 34, Type = MemberType.Char, Length = Alias3Suffix_MaxLength, Optional
    = true)]
  public string Alias3Suffix
  {
    get => alias3Suffix;
    set => alias3Suffix = value != null
      ? TrimEnd(Substring(value, 1, Alias3Suffix_MaxLength)) : null;
  }

  /// <summary>Length of the CURRENT_SPOUSE_FIRST_NAME attribute.</summary>
  public const int CurrentSpouseFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the CURRENT_SPOUSE_FIRST_NAME attribute.
  /// AP spouse first name
  /// </summary>
  [JsonPropertyName("currentSpouseFirstName")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = CurrentSpouseFirstName_MaxLength, Optional = true)]
  public string CurrentSpouseFirstName
  {
    get => currentSpouseFirstName;
    set => currentSpouseFirstName = value != null
      ? TrimEnd(Substring(value, 1, CurrentSpouseFirstName_MaxLength)) : null;
  }

  /// <summary>Length of the CURRENT_SPOUSE_MIDDLE_NAME attribute.</summary>
  public const int CurrentSpouseMiddleName_MaxLength = 1;

  /// <summary>
  /// The value of the CURRENT_SPOUSE_MIDDLE_NAME attribute.
  /// AP spouse middle inital
  /// </summary>
  [JsonPropertyName("currentSpouseMiddleName")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = CurrentSpouseMiddleName_MaxLength, Optional = true)]
  public string CurrentSpouseMiddleName
  {
    get => currentSpouseMiddleName;
    set => currentSpouseMiddleName = value != null
      ? TrimEnd(Substring(value, 1, CurrentSpouseMiddleName_MaxLength)) : null;
  }

  /// <summary>Length of the CURRENT_SPOUSE_LAST_NAME attribute.</summary>
  public const int CurrentSpouseLastName_MaxLength = 17;

  /// <summary>
  /// The value of the CURRENT_SPOUSE_LAST_NAME attribute.
  /// AP spouse last name
  /// </summary>
  [JsonPropertyName("currentSpouseLastName")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = CurrentSpouseLastName_MaxLength, Optional = true)]
  public string CurrentSpouseLastName
  {
    get => currentSpouseLastName;
    set => currentSpouseLastName = value != null
      ? TrimEnd(Substring(value, 1, CurrentSpouseLastName_MaxLength)) : null;
  }

  /// <summary>Length of the CURRENT_SPOUSE_SUFFIX attribute.</summary>
  public const int CurrentSpouseSuffix_MaxLength = 3;

  /// <summary>
  /// The value of the CURRENT_SPOUSE_SUFFIX attribute.
  /// AP spouse name suffix
  /// </summary>
  [JsonPropertyName("currentSpouseSuffix")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = CurrentSpouseSuffix_MaxLength, Optional = true)]
  public string CurrentSpouseSuffix
  {
    get => currentSpouseSuffix;
    set => currentSpouseSuffix = value != null
      ? TrimEnd(Substring(value, 1, CurrentSpouseSuffix_MaxLength)) : null;
  }

  /// <summary>Length of the OCCUPATION attribute.</summary>
  public const int Occupation_MaxLength = 32;

  /// <summary>
  /// The value of the OCCUPATION attribute.
  /// AP occupation
  /// </summary>
  [JsonPropertyName("occupation")]
  [Member(Index = 39, Type = MemberType.Char, Length = Occupation_MaxLength, Optional
    = true)]
  public string Occupation
  {
    get => occupation;
    set => occupation = value != null
      ? TrimEnd(Substring(value, 1, Occupation_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EMPLOYER_EIN attribute.
  /// Employer Identification Number
  /// </summary>
  [JsonPropertyName("employerEin")]
  [Member(Index = 40, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? EmployerEin
  {
    get => employerEin;
    set => employerEin = value;
  }

  /// <summary>Length of the EMPLOYER_NAME attribute.</summary>
  public const int EmployerName_MaxLength = 40;

  /// <summary>
  /// The value of the EMPLOYER_NAME attribute.
  /// Employer name
  /// </summary>
  [JsonPropertyName("employerName")]
  [Member(Index = 41, Type = MemberType.Char, Length = EmployerName_MaxLength, Optional
    = true)]
  public string EmployerName
  {
    get => employerName;
    set => employerName = value != null
      ? TrimEnd(Substring(value, 1, EmployerName_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_ADDRESS_LINE_1 attribute.</summary>
  public const int EmployerAddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the EMPLOYER_ADDRESS_LINE_1 attribute.
  /// *** DRAFT ***
  /// The first line of the address.
  /// </summary>
  [JsonPropertyName("employerAddressLine1")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = EmployerAddressLine1_MaxLength, Optional = true)]
  public string EmployerAddressLine1
  {
    get => employerAddressLine1;
    set => employerAddressLine1 = value != null
      ? TrimEnd(Substring(value, 1, EmployerAddressLine1_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_ADDRESS_LINE_2 attribute.</summary>
  public const int EmployerAddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the EMPLOYER_ADDRESS_LINE_2 attribute.
  /// *** DRAFT ***
  /// The second line of the address.
  /// </summary>
  [JsonPropertyName("employerAddressLine2")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = EmployerAddressLine2_MaxLength, Optional = true)]
  public string EmployerAddressLine2
  {
    get => employerAddressLine2;
    set => employerAddressLine2 = value != null
      ? TrimEnd(Substring(value, 1, EmployerAddressLine2_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_CITY attribute.</summary>
  public const int EmployerCity_MaxLength = 15;

  /// <summary>
  /// The value of the EMPLOYER_CITY attribute.
  /// *** DRAFT ***
  /// The city part of the address.
  /// </summary>
  [JsonPropertyName("employerCity")]
  [Member(Index = 44, Type = MemberType.Char, Length = EmployerCity_MaxLength, Optional
    = true)]
  public string EmployerCity
  {
    get => employerCity;
    set => employerCity = value != null
      ? TrimEnd(Substring(value, 1, EmployerCity_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_STATE attribute.</summary>
  public const int EmployerState_MaxLength = 2;

  /// <summary>
  /// The value of the EMPLOYER_STATE attribute.
  /// *** DRAFT ***
  /// The state part of the address.
  /// </summary>
  [JsonPropertyName("employerState")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = EmployerState_MaxLength, Optional = true)]
  public string EmployerState
  {
    get => employerState;
    set => employerState = value != null
      ? TrimEnd(Substring(value, 1, EmployerState_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_ZIP_CODE_5 attribute.</summary>
  public const int EmployerZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the EMPLOYER_ZIP_CODE_5 attribute.
  /// *** DRAFT ***
  /// The first five digits of the zip code.
  /// </summary>
  [JsonPropertyName("employerZipCode5")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = EmployerZipCode5_MaxLength, Optional = true)]
  public string EmployerZipCode5
  {
    get => employerZipCode5;
    set => employerZipCode5 = value != null
      ? TrimEnd(Substring(value, 1, EmployerZipCode5_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_ZIP_CODE_4 attribute.</summary>
  public const int EmployerZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the EMPLOYER_ZIP_CODE_4 attribute.
  /// *** DRAFT ***
  /// The second part of the zip code.  It consists of four digits.
  /// </summary>
  [JsonPropertyName("employerZipCode4")]
  [Member(Index = 47, Type = MemberType.Char, Length
    = EmployerZipCode4_MaxLength, Optional = true)]
  public string EmployerZipCode4
  {
    get => employerZipCode4;
    set => employerZipCode4 = value != null
      ? TrimEnd(Substring(value, 1, EmployerZipCode4_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EMPLOYER_PHONE_NUM attribute.
  /// Employer phone number
  /// </summary>
  [JsonPropertyName("employerPhoneNum")]
  [Member(Index = 48, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? EmployerPhoneNum
  {
    get => employerPhoneNum;
    set => employerPhoneNum = value;
  }

  /// <summary>
  /// The value of the EMPLOYER_EFFECTIVE_DATE attribute.
  /// Start date at this employer
  /// </summary>
  [JsonPropertyName("employerEffectiveDate")]
  [Member(Index = 49, Type = MemberType.Date, Optional = true)]
  public DateTime? EmployerEffectiveDate
  {
    get => employerEffectiveDate;
    set => employerEffectiveDate = value;
  }

  /// <summary>
  /// The value of the EMPLOYER_END_DATE attribute.
  /// End date at this employer
  /// </summary>
  [JsonPropertyName("employerEndDate")]
  [Member(Index = 50, Type = MemberType.Date, Optional = true)]
  public DateTime? EmployerEndDate
  {
    get => employerEndDate;
    set => employerEndDate = value;
  }

  /// <summary>Length of the EMPLOYER_CONFIRMED_IND attribute.</summary>
  public const int EmployerConfirmedInd_MaxLength = 1;

  /// <summary>
  /// The value of the EMPLOYER_CONFIRMED_IND attribute.
  /// Employment verified indicator
  /// Y-Yes	
  /// N-No
  /// </summary>
  [JsonPropertyName("employerConfirmedInd")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = EmployerConfirmedInd_MaxLength, Optional = true)]
  public string EmployerConfirmedInd
  {
    get => employerConfirmedInd;
    set => employerConfirmedInd = value != null
      ? TrimEnd(Substring(value, 1, EmployerConfirmedInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the WAGE_QTR attribute.
  /// Indicates for which quarter wages were paid
  /// </summary>
  [JsonPropertyName("wageQtr")]
  [Member(Index = 52, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? WageQtr
  {
    get => wageQtr;
    set => wageQtr = value;
  }

  /// <summary>
  /// The value of the WAGE_YEAR attribute.
  /// The year for which the wages were paid
  /// </summary>
  [JsonPropertyName("wageYear")]
  [Member(Index = 53, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? WageYear
  {
    get => wageYear;
    set => wageYear = value;
  }

  /// <summary>
  /// The value of the WAGE_AMOUNT attribute.
  /// Amount of wages paid for the quarter and year indicated
  /// </summary>
  [JsonPropertyName("wageAmount")]
  [Member(Index = 54, Type = MemberType.Number, Length = 12, Precision = 2, Optional
    = true)]
  public decimal? WageAmount
  {
    get => wageAmount;
    set => wageAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the INSURANCE_CARRIER_NAME attribute.</summary>
  public const int InsuranceCarrierName_MaxLength = 36;

  /// <summary>
  /// The value of the INSURANCE_CARRIER_NAME attribute.
  /// Name of insurance company providing coverage
  /// </summary>
  [JsonPropertyName("insuranceCarrierName")]
  [Member(Index = 55, Type = MemberType.Char, Length
    = InsuranceCarrierName_MaxLength, Optional = true)]
  public string InsuranceCarrierName
  {
    get => insuranceCarrierName;
    set => insuranceCarrierName = value != null
      ? TrimEnd(Substring(value, 1, InsuranceCarrierName_MaxLength)) : null;
  }

  /// <summary>Length of the INSURANCE_POLICY_NUM attribute.</summary>
  public const int InsurancePolicyNum_MaxLength = 20;

  /// <summary>
  /// The value of the INSURANCE_POLICY_NUM attribute.
  /// Insurance policy identification
  /// </summary>
  [JsonPropertyName("insurancePolicyNum")]
  [Member(Index = 56, Type = MemberType.Char, Length
    = InsurancePolicyNum_MaxLength, Optional = true)]
  public string InsurancePolicyNum
  {
    get => insurancePolicyNum;
    set => insurancePolicyNum = value != null
      ? TrimEnd(Substring(value, 1, InsurancePolicyNum_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_RES_ADDRESS_LINE_1 attribute.</summary>
  public const int LastResAddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the LAST_RES_ADDRESS_LINE_1 attribute.
  /// *** DRAFT ***
  /// The first line of the address.
  /// </summary>
  [JsonPropertyName("lastResAddressLine1")]
  [Member(Index = 57, Type = MemberType.Char, Length
    = LastResAddressLine1_MaxLength, Optional = true)]
  public string LastResAddressLine1
  {
    get => lastResAddressLine1;
    set => lastResAddressLine1 = value != null
      ? TrimEnd(Substring(value, 1, LastResAddressLine1_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_RES_ADDRESS_LINE_2 attribute.</summary>
  public const int LastResAddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the LAST_RES_ADDRESS_LINE_2 attribute.
  /// *** DRAFT ***
  /// The second line of the address.
  /// </summary>
  [JsonPropertyName("lastResAddressLine2")]
  [Member(Index = 58, Type = MemberType.Char, Length
    = LastResAddressLine2_MaxLength, Optional = true)]
  public string LastResAddressLine2
  {
    get => lastResAddressLine2;
    set => lastResAddressLine2 = value != null
      ? TrimEnd(Substring(value, 1, LastResAddressLine2_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_RES_CITY attribute.</summary>
  public const int LastResCity_MaxLength = 15;

  /// <summary>
  /// The value of the LAST_RES_CITY attribute.
  /// *** DRAFT ***
  /// The city part of the address.
  /// </summary>
  [JsonPropertyName("lastResCity")]
  [Member(Index = 59, Type = MemberType.Char, Length = LastResCity_MaxLength, Optional
    = true)]
  public string LastResCity
  {
    get => lastResCity;
    set => lastResCity = value != null
      ? TrimEnd(Substring(value, 1, LastResCity_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_RES_STATE attribute.</summary>
  public const int LastResState_MaxLength = 2;

  /// <summary>
  /// The value of the LAST_RES_STATE attribute.
  /// *** DRAFT ***
  /// The state part of the address.
  /// </summary>
  [JsonPropertyName("lastResState")]
  [Member(Index = 60, Type = MemberType.Char, Length = LastResState_MaxLength, Optional
    = true)]
  public string LastResState
  {
    get => lastResState;
    set => lastResState = value != null
      ? TrimEnd(Substring(value, 1, LastResState_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_RES_ZIP_CODE_5 attribute.</summary>
  public const int LastResZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the LAST_RES_ZIP_CODE_5 attribute.
  /// *** DRAFT ***
  /// The first five digits of the zip code.
  /// </summary>
  [JsonPropertyName("lastResZipCode5")]
  [Member(Index = 61, Type = MemberType.Char, Length
    = LastResZipCode5_MaxLength, Optional = true)]
  public string LastResZipCode5
  {
    get => lastResZipCode5;
    set => lastResZipCode5 = value != null
      ? TrimEnd(Substring(value, 1, LastResZipCode5_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_RES_ZIP_CODE_4 attribute.</summary>
  public const int LastResZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the LAST_RES_ZIP_CODE_4 attribute.
  /// *** DRAFT ***
  /// The second part of the zip code.  It consists of four digits.
  /// </summary>
  [JsonPropertyName("lastResZipCode4")]
  [Member(Index = 62, Type = MemberType.Char, Length
    = LastResZipCode4_MaxLength, Optional = true)]
  public string LastResZipCode4
  {
    get => lastResZipCode4;
    set => lastResZipCode4 = value != null
      ? TrimEnd(Substring(value, 1, LastResZipCode4_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_RES_ADDRESS_DATE attribute.
  /// The date this address is no longer effective.
  /// </summary>
  [JsonPropertyName("lastResAddressDate")]
  [Member(Index = 63, Type = MemberType.Date, Optional = true)]
  public DateTime? LastResAddressDate
  {
    get => lastResAddressDate;
    set => lastResAddressDate = value;
  }

  /// <summary>Length of the LAST_MAIL_ADDRESS_LINE_1 attribute.</summary>
  public const int LastMailAddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the LAST_MAIL_ADDRESS_LINE_1 attribute.
  /// *** DRAFT ***
  /// The first line of the address.
  /// </summary>
  [JsonPropertyName("lastMailAddressLine1")]
  [Member(Index = 64, Type = MemberType.Char, Length
    = LastMailAddressLine1_MaxLength, Optional = true)]
  public string LastMailAddressLine1
  {
    get => lastMailAddressLine1;
    set => lastMailAddressLine1 = value != null
      ? TrimEnd(Substring(value, 1, LastMailAddressLine1_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_MAIL_ADDRESS_LINE_2 attribute.</summary>
  public const int LastMailAddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the LAST_MAIL_ADDRESS_LINE_2 attribute.
  /// *** DRAFT ***
  /// The second line of the address.
  /// </summary>
  [JsonPropertyName("lastMailAddressLine2")]
  [Member(Index = 65, Type = MemberType.Char, Length
    = LastMailAddressLine2_MaxLength, Optional = true)]
  public string LastMailAddressLine2
  {
    get => lastMailAddressLine2;
    set => lastMailAddressLine2 = value != null
      ? TrimEnd(Substring(value, 1, LastMailAddressLine2_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_MAIL_CITY attribute.</summary>
  public const int LastMailCity_MaxLength = 15;

  /// <summary>
  /// The value of the LAST_MAIL_CITY attribute.
  /// *** DRAFT ***
  /// The city part of the address.
  /// </summary>
  [JsonPropertyName("lastMailCity")]
  [Member(Index = 66, Type = MemberType.Char, Length = LastMailCity_MaxLength, Optional
    = true)]
  public string LastMailCity
  {
    get => lastMailCity;
    set => lastMailCity = value != null
      ? TrimEnd(Substring(value, 1, LastMailCity_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_MAIL_STATE attribute.</summary>
  public const int LastMailState_MaxLength = 2;

  /// <summary>
  /// The value of the LAST_MAIL_STATE attribute.
  /// *** DRAFT ***
  /// The state part of the address.
  /// </summary>
  [JsonPropertyName("lastMailState")]
  [Member(Index = 67, Type = MemberType.Char, Length
    = LastMailState_MaxLength, Optional = true)]
  public string LastMailState
  {
    get => lastMailState;
    set => lastMailState = value != null
      ? TrimEnd(Substring(value, 1, LastMailState_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_MAIL_ZIP_CODE_5 attribute.</summary>
  public const int LastMailZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the LAST_MAIL_ZIP_CODE_5 attribute.
  /// *** DRAFT ***
  /// The first five digits of the zip code.
  /// </summary>
  [JsonPropertyName("lastMailZipCode5")]
  [Member(Index = 68, Type = MemberType.Char, Length
    = LastMailZipCode5_MaxLength, Optional = true)]
  public string LastMailZipCode5
  {
    get => lastMailZipCode5;
    set => lastMailZipCode5 = value != null
      ? TrimEnd(Substring(value, 1, LastMailZipCode5_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_MAIL_ZIP_CODE_4 attribute.</summary>
  public const int LastMailZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the LAST_MAIL_ZIP_CODE_4 attribute.
  /// *** DRAFT ***
  /// The second part of the zip code.  It consists of four digits.
  /// </summary>
  [JsonPropertyName("lastMailZipCode4")]
  [Member(Index = 69, Type = MemberType.Char, Length
    = LastMailZipCode4_MaxLength, Optional = true)]
  public string LastMailZipCode4
  {
    get => lastMailZipCode4;
    set => lastMailZipCode4 = value != null
      ? TrimEnd(Substring(value, 1, LastMailZipCode4_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_MAIL_ADDRESS_DATE attribute.
  /// The date this address is no longer effective.
  /// </summary>
  [JsonPropertyName("lastMailAddressDate")]
  [Member(Index = 70, Type = MemberType.Date, Optional = true)]
  public DateTime? LastMailAddressDate
  {
    get => lastMailAddressDate;
    set => lastMailAddressDate = value;
  }

  /// <summary>Length of the LAST_EMPLOYER_NAME attribute.</summary>
  public const int LastEmployerName_MaxLength = 40;

  /// <summary>
  /// The value of the LAST_EMPLOYER_NAME attribute.
  /// Previous employer name
  /// </summary>
  [JsonPropertyName("lastEmployerName")]
  [Member(Index = 71, Type = MemberType.Char, Length
    = LastEmployerName_MaxLength, Optional = true)]
  public string LastEmployerName
  {
    get => lastEmployerName;
    set => lastEmployerName = value != null
      ? TrimEnd(Substring(value, 1, LastEmployerName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_EMPLOYER_DATE attribute.
  /// Date AP left previous employer
  /// </summary>
  [JsonPropertyName("lastEmployerDate")]
  [Member(Index = 72, Type = MemberType.Date, Optional = true)]
  public DateTime? LastEmployerDate
  {
    get => lastEmployerDate;
    set => lastEmployerDate = value;
  }

  /// <summary>Length of the LAST_EMPLOYER_ADDRESS_LINE_1 attribute.</summary>
  public const int LastEmployerAddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the LAST_EMPLOYER_ADDRESS_LINE_1 attribute.
  /// *** DRAFT ***
  /// The first line of the address.
  /// </summary>
  [JsonPropertyName("lastEmployerAddressLine1")]
  [Member(Index = 73, Type = MemberType.Char, Length
    = LastEmployerAddressLine1_MaxLength, Optional = true)]
  public string LastEmployerAddressLine1
  {
    get => lastEmployerAddressLine1;
    set => lastEmployerAddressLine1 = value != null
      ? TrimEnd(Substring(value, 1, LastEmployerAddressLine1_MaxLength)) : null
      ;
  }

  /// <summary>Length of the LAST_EMPLOYER_ADDRESS_LINE_2 attribute.</summary>
  public const int LastEmployerAddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the LAST_EMPLOYER_ADDRESS_LINE_2 attribute.
  /// *** DRAFT ***
  /// The second line of the address.
  /// </summary>
  [JsonPropertyName("lastEmployerAddressLine2")]
  [Member(Index = 74, Type = MemberType.Char, Length
    = LastEmployerAddressLine2_MaxLength, Optional = true)]
  public string LastEmployerAddressLine2
  {
    get => lastEmployerAddressLine2;
    set => lastEmployerAddressLine2 = value != null
      ? TrimEnd(Substring(value, 1, LastEmployerAddressLine2_MaxLength)) : null
      ;
  }

  /// <summary>Length of the LAST_EMPLOYER_CITY attribute.</summary>
  public const int LastEmployerCity_MaxLength = 15;

  /// <summary>
  /// The value of the LAST_EMPLOYER_CITY attribute.
  /// *** DRAFT ***
  /// The city part of the address.
  /// </summary>
  [JsonPropertyName("lastEmployerCity")]
  [Member(Index = 75, Type = MemberType.Char, Length
    = LastEmployerCity_MaxLength, Optional = true)]
  public string LastEmployerCity
  {
    get => lastEmployerCity;
    set => lastEmployerCity = value != null
      ? TrimEnd(Substring(value, 1, LastEmployerCity_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_EMPLOYER_STATE attribute.</summary>
  public const int LastEmployerState_MaxLength = 2;

  /// <summary>
  /// The value of the LAST_EMPLOYER_STATE attribute.
  /// *** DRAFT ***
  /// The state part of the address.
  /// </summary>
  [JsonPropertyName("lastEmployerState")]
  [Member(Index = 76, Type = MemberType.Char, Length
    = LastEmployerState_MaxLength, Optional = true)]
  public string LastEmployerState
  {
    get => lastEmployerState;
    set => lastEmployerState = value != null
      ? TrimEnd(Substring(value, 1, LastEmployerState_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_EMPLOYER_ZIP_CODE_5 attribute.</summary>
  public const int LastEmployerZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the LAST_EMPLOYER_ZIP_CODE_5 attribute.
  /// *** DRAFT ***
  /// The first five digits of the zip code.
  /// </summary>
  [JsonPropertyName("lastEmployerZipCode5")]
  [Member(Index = 77, Type = MemberType.Char, Length
    = LastEmployerZipCode5_MaxLength, Optional = true)]
  public string LastEmployerZipCode5
  {
    get => lastEmployerZipCode5;
    set => lastEmployerZipCode5 = value != null
      ? TrimEnd(Substring(value, 1, LastEmployerZipCode5_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_EMPLOYER_ZIP_CODE_4 attribute.</summary>
  public const int LastEmployerZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the LAST_EMPLOYER_ZIP_CODE_4 attribute.
  /// *** DRAFT ***
  /// The second part of the zip code.  It consists of four digits.
  /// </summary>
  [JsonPropertyName("lastEmployerZipCode4")]
  [Member(Index = 78, Type = MemberType.Char, Length
    = LastEmployerZipCode4_MaxLength, Optional = true)]
  public string LastEmployerZipCode4
  {
    get => lastEmployerZipCode4;
    set => lastEmployerZipCode4 = value != null
      ? TrimEnd(Substring(value, 1, LastEmployerZipCode4_MaxLength)) : null;
  }

  /// <summary>Length of the PROFESSIONAL_LICENSES attribute.</summary>
  public const int ProfessionalLicenses_MaxLength = 50;

  /// <summary>
  /// The value of the PROFESSIONAL_LICENSES attribute.
  /// Any known professional licenses carried by this person.
  /// </summary>
  [JsonPropertyName("professionalLicenses")]
  [Member(Index = 79, Type = MemberType.Char, Length
    = ProfessionalLicenses_MaxLength, Optional = true)]
  public string ProfessionalLicenses
  {
    get => professionalLicenses;
    set => professionalLicenses = value != null
      ? TrimEnd(Substring(value, 1, ProfessionalLicenses_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the WORK_AREA_CODE attribute.
  /// The area code of the work number for this person.
  /// </summary>
  [JsonPropertyName("workAreaCode")]
  [Member(Index = 80, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? WorkAreaCode
  {
    get => workAreaCode;
    set => workAreaCode = value;
  }

  /// <summary>
  /// The value of the HOME_AREA_CODE attribute.
  /// The area code of the telephone number for this person.
  /// </summary>
  [JsonPropertyName("homeAreaCode")]
  [Member(Index = 81, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? HomeAreaCode
  {
    get => homeAreaCode;
    set => homeAreaCode = value;
  }

  /// <summary>
  /// The value of the LAST_EMPLOYER_END_DATE attribute.
  /// The date the last employer effective date became invalid.
  /// </summary>
  [JsonPropertyName("lastEmployerEndDate")]
  [Member(Index = 82, Type = MemberType.Date, Optional = true)]
  public DateTime? LastEmployerEndDate
  {
    get => lastEmployerEndDate;
    set => lastEmployerEndDate = value;
  }

  /// <summary>
  /// The value of the EMPLOYER_AREA_CODE attribute.
  /// The area code for the phone number of the employer.
  /// </summary>
  [JsonPropertyName("employerAreaCode")]
  [Member(Index = 83, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? EmployerAreaCode
  {
    get => employerAreaCode;
    set => employerAreaCode = value;
  }

  /// <summary>Length of the EMPLOYER2_NAME attribute.</summary>
  public const int Employer2Name_MaxLength = 33;

  /// <summary>
  /// The value of the EMPLOYER2_NAME attribute.
  /// The name of the employer.
  /// </summary>
  [JsonPropertyName("employer2Name")]
  [Member(Index = 84, Type = MemberType.Char, Length
    = Employer2Name_MaxLength, Optional = true)]
  public string Employer2Name
  {
    get => employer2Name;
    set => employer2Name = value != null
      ? TrimEnd(Substring(value, 1, Employer2Name_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EMPLOYER2_EIN attribute.
  /// The employer identification number (EIN).
  /// </summary>
  [JsonPropertyName("employer2Ein")]
  [Member(Index = 85, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? Employer2Ein
  {
    get => employer2Ein;
    set => employer2Ein = value;
  }

  /// <summary>Length of the EMPLOYER2_PHONE_NUMBER attribute.</summary>
  public const int Employer2PhoneNumber_MaxLength = 7;

  /// <summary>
  /// The value of the EMPLOYER2_PHONE_NUMBER attribute.
  /// The phone number of the employer.
  /// </summary>
  [JsonPropertyName("employer2PhoneNumber")]
  [Member(Index = 86, Type = MemberType.Char, Length
    = Employer2PhoneNumber_MaxLength, Optional = true)]
  public string Employer2PhoneNumber
  {
    get => employer2PhoneNumber;
    set => employer2PhoneNumber = value != null
      ? TrimEnd(Substring(value, 1, Employer2PhoneNumber_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER2_AREA_CODE attribute.</summary>
  public const int Employer2AreaCode_MaxLength = 3;

  /// <summary>
  /// The value of the EMPLOYER2_AREA_CODE attribute.
  /// The area code for the phone number of the employer.
  /// </summary>
  [JsonPropertyName("employer2AreaCode")]
  [Member(Index = 87, Type = MemberType.Char, Length
    = Employer2AreaCode_MaxLength, Optional = true)]
  public string Employer2AreaCode
  {
    get => employer2AreaCode;
    set => employer2AreaCode = value != null
      ? TrimEnd(Substring(value, 1, Employer2AreaCode_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER2_ADDRESS_LINE_1 attribute.</summary>
  public const int Employer2AddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the EMPLOYER2_ADDRESS_LINE_1 attribute.
  /// The first line of the postal address.
  /// </summary>
  [JsonPropertyName("employer2AddressLine1")]
  [Member(Index = 88, Type = MemberType.Char, Length
    = Employer2AddressLine1_MaxLength, Optional = true)]
  public string Employer2AddressLine1
  {
    get => employer2AddressLine1;
    set => employer2AddressLine1 = value != null
      ? TrimEnd(Substring(value, 1, Employer2AddressLine1_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER2_ADDRESS_LINE_2 attribute.</summary>
  public const int Employer2AddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the EMPLOYER2_ADDRESS_LINE_2 attribute.
  /// The second line of the postal address.
  /// </summary>
  [JsonPropertyName("employer2AddressLine2")]
  [Member(Index = 89, Type = MemberType.Char, Length
    = Employer2AddressLine2_MaxLength, Optional = true)]
  public string Employer2AddressLine2
  {
    get => employer2AddressLine2;
    set => employer2AddressLine2 = value != null
      ? TrimEnd(Substring(value, 1, Employer2AddressLine2_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER2_CITY attribute.</summary>
  public const int Employer2City_MaxLength = 15;

  /// <summary>
  /// The value of the EMPLOYER2_CITY attribute.
  /// The city that the employer is located in.
  /// </summary>
  [JsonPropertyName("employer2City")]
  [Member(Index = 90, Type = MemberType.Char, Length
    = Employer2City_MaxLength, Optional = true)]
  public string Employer2City
  {
    get => employer2City;
    set => employer2City = value != null
      ? TrimEnd(Substring(value, 1, Employer2City_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER2_STATE attribute.</summary>
  public const int Employer2State_MaxLength = 2;

  /// <summary>
  /// The value of the EMPLOYER2_STATE attribute.
  /// The state that the employer is located in.
  /// </summary>
  [JsonPropertyName("employer2State")]
  [Member(Index = 91, Type = MemberType.Char, Length
    = Employer2State_MaxLength, Optional = true)]
  public string Employer2State
  {
    get => employer2State;
    set => employer2State = value != null
      ? TrimEnd(Substring(value, 1, Employer2State_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EMPLOYER2_ZIP_CODE_5 attribute.
  /// The first part of the zip code. It consists of five digits.
  /// </summary>
  [JsonPropertyName("employer2ZipCode5")]
  [Member(Index = 92, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? Employer2ZipCode5
  {
    get => employer2ZipCode5;
    set => employer2ZipCode5 = value;
  }

  /// <summary>
  /// The value of the EMPLOYER2_ZIP_CODE_4 attribute.
  /// The second part of the zip code. It consists of four digits.
  /// </summary>
  [JsonPropertyName("employer2ZipCode4")]
  [Member(Index = 93, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? Employer2ZipCode4
  {
    get => employer2ZipCode4;
    set => employer2ZipCode4 = value;
  }

  /// <summary>Length of the EMPLOYER2_CONFIRMED_INDICATOR attribute.</summary>
  public const int Employer2ConfirmedIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the EMPLOYER2_CONFIRMED_INDICATOR attribute.
  /// Employment verified indicator. Y=Yes N=No.
  /// </summary>
  [JsonPropertyName("employer2ConfirmedIndicator")]
  [Member(Index = 94, Type = MemberType.Char, Length
    = Employer2ConfirmedIndicator_MaxLength, Optional = true)]
  public string Employer2ConfirmedIndicator
  {
    get => employer2ConfirmedIndicator;
    set => employer2ConfirmedIndicator = value != null
      ? TrimEnd(Substring(value, 1, Employer2ConfirmedIndicator_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the EMPLOYER2_EFFECTIVE_DATE attribute.
  /// Start date at this employer.
  /// </summary>
  [JsonPropertyName("employer2EffectiveDate")]
  [Member(Index = 95, Type = MemberType.Date, Optional = true)]
  public DateTime? Employer2EffectiveDate
  {
    get => employer2EffectiveDate;
    set => employer2EffectiveDate = value;
  }

  /// <summary>
  /// The value of the EMPLOYER2_END_DATE attribute.
  /// End date at this employer.
  /// </summary>
  [JsonPropertyName("employer2EndDate")]
  [Member(Index = 96, Type = MemberType.Date, Optional = true)]
  public DateTime? Employer2EndDate
  {
    get => employer2EndDate;
    set => employer2EndDate = value;
  }

  /// <summary>
  /// The value of the EMPLOYER2_WAGE_AMOUNT attribute.
  /// Amount of wages paid for the quarter and year indicated.
  /// </summary>
  [JsonPropertyName("employer2WageAmount")]
  [Member(Index = 97, Type = MemberType.Number, Length = 12, Optional = true)]
  public long? Employer2WageAmount
  {
    get => employer2WageAmount;
    set => employer2WageAmount = value;
  }

  /// <summary>
  /// The value of the EMPLOYER2_WAGE_QUARTER attribute.
  /// Indicates for which quarter wages were paid.
  /// </summary>
  [JsonPropertyName("employer2WageQuarter")]
  [Member(Index = 98, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? Employer2WageQuarter
  {
    get => employer2WageQuarter;
    set => employer2WageQuarter = value;
  }

  /// <summary>
  /// The value of the EMPLOYER2_WAGE_YEAR attribute.
  /// The year for which wages were paid.
  /// </summary>
  [JsonPropertyName("employer2WageYear")]
  [Member(Index = 99, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? Employer2WageYear
  {
    get => employer2WageYear;
    set => employer2WageYear = value;
  }

  /// <summary>Length of the EMPLOYER3_NAME attribute.</summary>
  public const int Employer3Name_MaxLength = 33;

  /// <summary>
  /// The value of the EMPLOYER3_NAME attribute.
  /// The name of the employer.
  /// </summary>
  [JsonPropertyName("employer3Name")]
  [Member(Index = 100, Type = MemberType.Char, Length
    = Employer3Name_MaxLength, Optional = true)]
  public string Employer3Name
  {
    get => employer3Name;
    set => employer3Name = value != null
      ? TrimEnd(Substring(value, 1, Employer3Name_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EMPLOYER3_EIN attribute.
  /// The employer identification number (EIN).
  /// </summary>
  [JsonPropertyName("employer3Ein")]
  [Member(Index = 101, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? Employer3Ein
  {
    get => employer3Ein;
    set => employer3Ein = value;
  }

  /// <summary>Length of the EMPLOYER3_PHONE_NUMBER attribute.</summary>
  public const int Employer3PhoneNumber_MaxLength = 7;

  /// <summary>
  /// The value of the EMPLOYER3_PHONE_NUMBER attribute.
  /// The phone number of the employer.
  /// </summary>
  [JsonPropertyName("employer3PhoneNumber")]
  [Member(Index = 102, Type = MemberType.Char, Length
    = Employer3PhoneNumber_MaxLength, Optional = true)]
  public string Employer3PhoneNumber
  {
    get => employer3PhoneNumber;
    set => employer3PhoneNumber = value != null
      ? TrimEnd(Substring(value, 1, Employer3PhoneNumber_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER3_AREA_CODE attribute.</summary>
  public const int Employer3AreaCode_MaxLength = 3;

  /// <summary>
  /// The value of the EMPLOYER3_AREA_CODE attribute.
  /// The area code for the phone number of the employer.
  /// </summary>
  [JsonPropertyName("employer3AreaCode")]
  [Member(Index = 103, Type = MemberType.Char, Length
    = Employer3AreaCode_MaxLength, Optional = true)]
  public string Employer3AreaCode
  {
    get => employer3AreaCode;
    set => employer3AreaCode = value != null
      ? TrimEnd(Substring(value, 1, Employer3AreaCode_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER3_ADDRESS_LINE_1 attribute.</summary>
  public const int Employer3AddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the EMPLOYER3_ADDRESS_LINE_1 attribute.
  /// The first line of the postal address.
  /// </summary>
  [JsonPropertyName("employer3AddressLine1")]
  [Member(Index = 104, Type = MemberType.Char, Length
    = Employer3AddressLine1_MaxLength, Optional = true)]
  public string Employer3AddressLine1
  {
    get => employer3AddressLine1;
    set => employer3AddressLine1 = value != null
      ? TrimEnd(Substring(value, 1, Employer3AddressLine1_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER3_ADDRESS_LINE_2 attribute.</summary>
  public const int Employer3AddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the EMPLOYER3_ADDRESS_LINE_2 attribute.
  /// The second line of the postal address.
  /// </summary>
  [JsonPropertyName("employer3AddressLine2")]
  [Member(Index = 105, Type = MemberType.Char, Length
    = Employer3AddressLine2_MaxLength, Optional = true)]
  public string Employer3AddressLine2
  {
    get => employer3AddressLine2;
    set => employer3AddressLine2 = value != null
      ? TrimEnd(Substring(value, 1, Employer3AddressLine2_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER3_CITY attribute.</summary>
  public const int Employer3City_MaxLength = 15;

  /// <summary>
  /// The value of the EMPLOYER3_CITY attribute.
  /// The city that the employer is located in.
  /// </summary>
  [JsonPropertyName("employer3City")]
  [Member(Index = 106, Type = MemberType.Char, Length
    = Employer3City_MaxLength, Optional = true)]
  public string Employer3City
  {
    get => employer3City;
    set => employer3City = value != null
      ? TrimEnd(Substring(value, 1, Employer3City_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER3_STATE attribute.</summary>
  public const int Employer3State_MaxLength = 2;

  /// <summary>
  /// The value of the EMPLOYER3_STATE attribute.
  /// The state that the employer is located in.
  /// </summary>
  [JsonPropertyName("employer3State")]
  [Member(Index = 107, Type = MemberType.Char, Length
    = Employer3State_MaxLength, Optional = true)]
  public string Employer3State
  {
    get => employer3State;
    set => employer3State = value != null
      ? TrimEnd(Substring(value, 1, Employer3State_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EMPLOYER3_ZIP_CODE_5 attribute.
  /// The first part of the zip code. It consists of five digits.
  /// </summary>
  [JsonPropertyName("employer3ZipCode5")]
  [Member(Index = 108, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? Employer3ZipCode5
  {
    get => employer3ZipCode5;
    set => employer3ZipCode5 = value;
  }

  /// <summary>
  /// The value of the EMPLOYER3_ZIP_CODE_4 attribute.
  /// The second part of the zip code. It consists of four digits.
  /// </summary>
  [JsonPropertyName("employer3ZipCode4")]
  [Member(Index = 109, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? Employer3ZipCode4
  {
    get => employer3ZipCode4;
    set => employer3ZipCode4 = value;
  }

  /// <summary>Length of the EMPLOYER3_CONFIRMED_INDICATOR attribute.</summary>
  public const int Employer3ConfirmedIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the EMPLOYER3_CONFIRMED_INDICATOR attribute.
  /// Employment verified indicator. Y=Yes N=No.
  /// </summary>
  [JsonPropertyName("employer3ConfirmedIndicator")]
  [Member(Index = 110, Type = MemberType.Char, Length
    = Employer3ConfirmedIndicator_MaxLength, Optional = true)]
  public string Employer3ConfirmedIndicator
  {
    get => employer3ConfirmedIndicator;
    set => employer3ConfirmedIndicator = value != null
      ? TrimEnd(Substring(value, 1, Employer3ConfirmedIndicator_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the EMPLOYER3_EFFECTIVE_DATE attribute.
  /// Start date at this employer.
  /// </summary>
  [JsonPropertyName("employer3EffectiveDate")]
  [Member(Index = 111, Type = MemberType.Date, Optional = true)]
  public DateTime? Employer3EffectiveDate
  {
    get => employer3EffectiveDate;
    set => employer3EffectiveDate = value;
  }

  /// <summary>
  /// The value of the EMPLOYER3_END_DATE attribute.
  /// End date at this employer.
  /// </summary>
  [JsonPropertyName("employer3EndDate")]
  [Member(Index = 112, Type = MemberType.Date, Optional = true)]
  public DateTime? Employer3EndDate
  {
    get => employer3EndDate;
    set => employer3EndDate = value;
  }

  /// <summary>
  /// The value of the EMPLOYER3_WAGE_AMOUNT attribute.
  /// Amount of wages paid for the quarter and year indicated.
  /// </summary>
  [JsonPropertyName("employer3WageAmount")]
  [Member(Index = 113, Type = MemberType.Number, Length = 12, Optional = true)]
  public long? Employer3WageAmount
  {
    get => employer3WageAmount;
    set => employer3WageAmount = value;
  }

  /// <summary>
  /// The value of the EMPLOYER3_WAGE_QUARTER attribute.
  /// Indicates for which quarter wages were paid.
  /// </summary>
  [JsonPropertyName("employer3WageQuarter")]
  [Member(Index = 114, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? Employer3WageQuarter
  {
    get => employer3WageQuarter;
    set => employer3WageQuarter = value;
  }

  /// <summary>
  /// The value of the EMPLOYER3_WAGE_YEAR attribute.
  /// The year for which quarter wages were paid.
  /// </summary>
  [JsonPropertyName("employer3WageYear")]
  [Member(Index = 115, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? Employer3WageYear
  {
    get => employer3WageYear;
    set => employer3WageYear = value;
  }

  /// <summary>
  /// The value of the TRANS_SERIAL_NUMBER attribute.
  /// This is a unique number assigned to each CSENet transaction.  It has no 
  /// place in the KESSEP system but is required to provide a key used to
  /// process CSENet Referrals.
  /// </summary>
  [JsonPropertyName("cncTransSerlNbr")]
  [DefaultValue(0L)]
  [Member(Index = 116, Type = MemberType.Number, Length = 12)]
  public long CncTransSerlNbr
  {
    get => cncTransSerlNbr;
    set => cncTransSerlNbr = value;
  }

  /// <summary>
  /// The value of the TRANSACTION_DATE attribute.
  /// This is the date on which CSENet transmitted the Referral.
  /// </summary>
  [JsonPropertyName("cncTransactionDt")]
  [Member(Index = 117, Type = MemberType.Date)]
  public DateTime? CncTransactionDt
  {
    get => cncTransactionDt;
    set => cncTransactionDt = value;
  }

  private string residentialAddressLine1;
  private string residentialAddressLine2;
  private string residentialCity;
  private string residentialState;
  private string residentialZipCode5;
  private string residentialZipCode4;
  private string mailingAddressLine1;
  private string mailingAddressLine2;
  private string mailingCity;
  private string mailingState;
  private string mailingZipCode5;
  private string mailingZipCode4;
  private DateTime? residentialAddressEffectvDate;
  private DateTime? residentialAddressEndDate;
  private string residentialAddressConfirmInd;
  private DateTime? mailingAddressEffectiveDate;
  private DateTime? mailingAddressEndDate;
  private string mailingAddressConfirmedInd;
  private int? homePhoneNumber;
  private int? workPhoneNumber;
  private string driversLicState;
  private string driversLicenseNum;
  private string alias1FirstName;
  private string alias1MiddleName;
  private string alias1LastName;
  private string alias1Suffix;
  private string alias2FirstName;
  private string alias2MiddleName;
  private string alias2LastName;
  private string alias2Suffix;
  private string alias3FirstName;
  private string alias3MiddleName;
  private string alias3LastName;
  private string alias3Suffix;
  private string currentSpouseFirstName;
  private string currentSpouseMiddleName;
  private string currentSpouseLastName;
  private string currentSpouseSuffix;
  private string occupation;
  private int? employerEin;
  private string employerName;
  private string employerAddressLine1;
  private string employerAddressLine2;
  private string employerCity;
  private string employerState;
  private string employerZipCode5;
  private string employerZipCode4;
  private int? employerPhoneNum;
  private DateTime? employerEffectiveDate;
  private DateTime? employerEndDate;
  private string employerConfirmedInd;
  private int? wageQtr;
  private int? wageYear;
  private decimal? wageAmount;
  private string insuranceCarrierName;
  private string insurancePolicyNum;
  private string lastResAddressLine1;
  private string lastResAddressLine2;
  private string lastResCity;
  private string lastResState;
  private string lastResZipCode5;
  private string lastResZipCode4;
  private DateTime? lastResAddressDate;
  private string lastMailAddressLine1;
  private string lastMailAddressLine2;
  private string lastMailCity;
  private string lastMailState;
  private string lastMailZipCode5;
  private string lastMailZipCode4;
  private DateTime? lastMailAddressDate;
  private string lastEmployerName;
  private DateTime? lastEmployerDate;
  private string lastEmployerAddressLine1;
  private string lastEmployerAddressLine2;
  private string lastEmployerCity;
  private string lastEmployerState;
  private string lastEmployerZipCode5;
  private string lastEmployerZipCode4;
  private string professionalLicenses;
  private int? workAreaCode;
  private int? homeAreaCode;
  private DateTime? lastEmployerEndDate;
  private int? employerAreaCode;
  private string employer2Name;
  private int? employer2Ein;
  private string employer2PhoneNumber;
  private string employer2AreaCode;
  private string employer2AddressLine1;
  private string employer2AddressLine2;
  private string employer2City;
  private string employer2State;
  private int? employer2ZipCode5;
  private int? employer2ZipCode4;
  private string employer2ConfirmedIndicator;
  private DateTime? employer2EffectiveDate;
  private DateTime? employer2EndDate;
  private long? employer2WageAmount;
  private int? employer2WageQuarter;
  private int? employer2WageYear;
  private string employer3Name;
  private int? employer3Ein;
  private string employer3PhoneNumber;
  private string employer3AreaCode;
  private string employer3AddressLine1;
  private string employer3AddressLine2;
  private string employer3City;
  private string employer3State;
  private int? employer3ZipCode5;
  private int? employer3ZipCode4;
  private string employer3ConfirmedIndicator;
  private DateTime? employer3EffectiveDate;
  private DateTime? employer3EndDate;
  private long? employer3WageAmount;
  private int? employer3WageQuarter;
  private int? employer3WageYear;
  private long cncTransSerlNbr;
  private DateTime? cncTransactionDt;
}
