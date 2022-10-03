// The source file: PERSON_PRIVATE_ATTORNEY, ID: 371439409, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This entity type contains the details of legal representation of a CSE 
/// person by a particular attorney.
/// </summary>
[Serializable]
public partial class PersonPrivateAttorney: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PersonPrivateAttorney()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PersonPrivateAttorney(PersonPrivateAttorney that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PersonPrivateAttorney Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PersonPrivateAttorney that)
  {
    base.Assign(that);
    faxNumberAreaCode = that.faxNumberAreaCode;
    phoneAreaCode = that.phoneAreaCode;
    faxExt = that.faxExt;
    phoneExt = that.phoneExt;
    identifier = that.identifier;
    dateRetained = that.dateRetained;
    dateDismissed = that.dateDismissed;
    lastName = that.lastName;
    firstName = that.firstName;
    middleInitial = that.middleInitial;
    firmName = that.firmName;
    phone = that.phone;
    faxNumber = that.faxNumber;
    courtCaseNumber = that.courtCaseNumber;
    fipsStateAbbreviation = that.fipsStateAbbreviation;
    fipsCountyAbbreviation = that.fipsCountyAbbreviation;
    tribCountry = that.tribCountry;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    emailAddress = that.emailAddress;
    barNumber = that.barNumber;
    consentIndicator = that.consentIndicator;
    note = that.note;
    casNumber = that.casNumber;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the FAX_NUMBER_AREA_CODE attribute.
  /// The 3-digit area code for the fax number of the private attorney.
  /// </summary>
  [JsonPropertyName("faxNumberAreaCode")]
  [Member(Index = 1, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? FaxNumberAreaCode
  {
    get => faxNumberAreaCode;
    set => faxNumberAreaCode = value;
  }

  /// <summary>
  /// The value of the PHONE_AREA_CODE attribute.
  /// The 3-digit area code for the phone number of the private attorney.
  /// </summary>
  [JsonPropertyName("phoneAreaCode")]
  [Member(Index = 2, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? PhoneAreaCode
  {
    get => phoneAreaCode;
    set => phoneAreaCode = value;
  }

  /// <summary>Length of the FAX_EXT attribute.</summary>
  public const int FaxExt_MaxLength = 5;

  /// <summary>
  /// The value of the FAX_EXT attribute.
  /// The 5 digit extension for the fax number of the private attorney.
  /// </summary>
  [JsonPropertyName("faxExt")]
  [Member(Index = 3, Type = MemberType.Char, Length = FaxExt_MaxLength, Optional
    = true)]
  public string FaxExt
  {
    get => faxExt;
    set => faxExt = value != null
      ? TrimEnd(Substring(value, 1, FaxExt_MaxLength)) : null;
  }

  /// <summary>Length of the PHONE_EXT attribute.</summary>
  public const int PhoneExt_MaxLength = 5;

  /// <summary>
  /// The value of the PHONE_EXT attribute.
  /// The 5 digit extension number for the private attorney's phone.
  /// </summary>
  [JsonPropertyName("phoneExt")]
  [Member(Index = 4, Type = MemberType.Char, Length = PhoneExt_MaxLength, Optional
    = true)]
  public string PhoneExt
  {
    get => phoneExt;
    set => phoneExt = value != null
      ? TrimEnd(Substring(value, 1, PhoneExt_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Unique number that descripts a particular attorney's detailed information.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 2)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the DATE_RETAINED attribute.
  /// The date an attorney becomes legally responsible for this legal case.
  /// </summary>
  [JsonPropertyName("dateRetained")]
  [Member(Index = 6, Type = MemberType.Date)]
  public DateTime? DateRetained
  {
    get => dateRetained;
    set => dateRetained = value;
  }

  /// <summary>
  /// The value of the DATE_DISMISSED attribute.
  /// The date an attorney was released from this legal case.
  /// </summary>
  [JsonPropertyName("dateDismissed")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? DateDismissed
  {
    get => dateDismissed;
    set => dateDismissed = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// The surname of the legal counsel hired by the CSE Person.
  /// </summary>
  [JsonPropertyName("lastName")]
  [Member(Index = 8, Type = MemberType.Char, Length = LastName_MaxLength, Optional
    = true)]
  public string LastName
  {
    get => lastName;
    set => lastName = value != null
      ? TrimEnd(Substring(value, 1, LastName_MaxLength)) : null;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// The given name of the legal counsel hired by the CSE Person.
  /// </summary>
  [JsonPropertyName("firstName")]
  [Member(Index = 9, Type = MemberType.Char, Length = FirstName_MaxLength, Optional
    = true)]
  public string FirstName
  {
    get => firstName;
    set => firstName = value != null
      ? TrimEnd(Substring(value, 1, FirstName_MaxLength)) : null;
  }

  /// <summary>Length of the MIDDLE_INITIAL attribute.</summary>
  public const int MiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INITIAL attribute.
  /// The initial of the legal counsel hired by the CSE Person.
  /// </summary>
  [JsonPropertyName("middleInitial")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = MiddleInitial_MaxLength, Optional = true)]
  public string MiddleInitial
  {
    get => middleInitial;
    set => middleInitial = value != null
      ? TrimEnd(Substring(value, 1, MiddleInitial_MaxLength)) : null;
  }

  /// <summary>Length of the FIRM_NAME attribute.</summary>
  public const int FirmName_MaxLength = 30;

  /// <summary>
  /// The value of the FIRM_NAME attribute.
  /// The company name which employs the legal counsel of the CSE Person.
  /// </summary>
  [JsonPropertyName("firmName")]
  [Member(Index = 11, Type = MemberType.Char, Length = FirmName_MaxLength, Optional
    = true)]
  public string FirmName
  {
    get => firmName;
    set => firmName = value != null
      ? TrimEnd(Substring(value, 1, FirmName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHONE attribute.
  /// The phone number of the private attorney as 7 digit phone number.
  /// </summary>
  [JsonPropertyName("phone")]
  [Member(Index = 12, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? Phone
  {
    get => phone;
    set => phone = value;
  }

  /// <summary>
  /// The value of the FAX_NUMBER attribute.
  /// The fax number of the private attorney as 7 digit phone number.
  /// </summary>
  [JsonPropertyName("faxNumber")]
  [Member(Index = 13, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? FaxNumber
  {
    get => faxNumber;
    set => faxNumber = value;
  }

  /// <summary>Length of the COURT_CASE_NUMBER attribute.</summary>
  public const int CourtCaseNumber_MaxLength = 17;

  /// <summary>
  /// The value of the COURT_CASE_NUMBER attribute.
  /// Identifying number assigned by the tribunal.
  /// </summary>
  [JsonPropertyName("courtCaseNumber")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = CourtCaseNumber_MaxLength, Optional = true)]
  public string CourtCaseNumber
  {
    get => courtCaseNumber;
    set => courtCaseNumber = value != null
      ? TrimEnd(Substring(value, 1, CourtCaseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the FIPS_STATE_ABBREVIATION attribute.</summary>
  public const int FipsStateAbbreviation_MaxLength = 2;

  /// <summary>
  /// The value of the FIPS_STATE_ABBREVIATION attribute.
  /// A 2 character standard abbreviation as defined by the US Postal Service.
  /// </summary>
  [JsonPropertyName("fipsStateAbbreviation")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = FipsStateAbbreviation_MaxLength, Optional = true)]
  public string FipsStateAbbreviation
  {
    get => fipsStateAbbreviation;
    set => fipsStateAbbreviation = value != null
      ? TrimEnd(Substring(value, 1, FipsStateAbbreviation_MaxLength)) : null;
  }

  /// <summary>Length of the FIPS_COUNTY_ABBREVIATION attribute.</summary>
  public const int FipsCountyAbbreviation_MaxLength = 2;

  /// <summary>
  /// The value of the FIPS_COUNTY_ABBREVIATION attribute.
  /// This specifies the 2-character alphabetic code for the county.
  /// </summary>
  [JsonPropertyName("fipsCountyAbbreviation")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = FipsCountyAbbreviation_MaxLength, Optional = true)]
  public string FipsCountyAbbreviation
  {
    get => fipsCountyAbbreviation;
    set => fipsCountyAbbreviation = value != null
      ? TrimEnd(Substring(value, 1, FipsCountyAbbreviation_MaxLength)) : null;
  }

  /// <summary>Length of the TRIB_COUNTRY attribute.</summary>
  public const int TribCountry_MaxLength = 2;

  /// <summary>
  /// The value of the TRIB_COUNTRY attribute.
  /// Code indicating the country in which the address is located.
  /// </summary>
  [JsonPropertyName("tribCountry")]
  [Member(Index = 17, Type = MemberType.Char, Length = TribCountry_MaxLength, Optional
    = true)]
  public string TribCountry
  {
    get => tribCountry;
    set => tribCountry = value != null
      ? TrimEnd(Substring(value, 1, TribCountry_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
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
  /// Timestamp of creation of the occurrence.
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
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 21, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the EMAIL_ADDRESS attribute.</summary>
  public const int EmailAddress_MaxLength = 65;

  /// <summary>
  /// The value of the EMAIL_ADDRESS attribute.
  /// </summary>
  [JsonPropertyName("emailAddress")]
  [Member(Index = 22, Type = MemberType.Varchar, Length
    = EmailAddress_MaxLength, Optional = true)]
  public string EmailAddress
  {
    get => emailAddress;
    set => emailAddress = value != null
      ? Substring(value, 1, EmailAddress_MaxLength) : null;
  }

  /// <summary>Length of the BAR_NUMBER attribute.</summary>
  public const int BarNumber_MaxLength = 10;

  /// <summary>
  /// The value of the BAR_NUMBER attribute.
  /// The Bar number of the Attorney
  /// </summary>
  [JsonPropertyName("barNumber")]
  [Member(Index = 23, Type = MemberType.Char, Length = BarNumber_MaxLength, Optional
    = true)]
  public string BarNumber
  {
    get => barNumber;
    set => barNumber = value != null
      ? TrimEnd(Substring(value, 1, BarNumber_MaxLength)) : null;
  }

  /// <summary>Length of the CONSENT_INDICATOR attribute.</summary>
  public const int ConsentIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the CONSENT_INDICATOR attribute.
  /// A Yes or No (Y/N) indicator to denote if consent is given to speak to the 
  /// private attorney.
  /// </summary>
  [JsonPropertyName("consentIndicator")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = ConsentIndicator_MaxLength, Optional = true)]
  public string ConsentIndicator
  {
    get => consentIndicator;
    set => consentIndicator = value != null
      ? TrimEnd(Substring(value, 1, ConsentIndicator_MaxLength)) : null;
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 78;

  /// <summary>
  /// The value of the NOTE attribute.
  /// Freeform area for any additional information.
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 25, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => note;
    set => note = value != null ? Substring(value, 1, Note_MaxLength) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonPropertyName("casNumber")]
  [Member(Index = 26, Type = MemberType.Char, Length = CasNumber_MaxLength, Optional
    = true)]
  public string CasNumber
  {
    get => casNumber;
    set => casNumber = value != null
      ? TrimEnd(Substring(value, 1, CasNumber_MaxLength)) : null;
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
  [Member(Index = 27, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  private int? faxNumberAreaCode;
  private int? phoneAreaCode;
  private string faxExt;
  private string phoneExt;
  private int identifier;
  private DateTime? dateRetained;
  private DateTime? dateDismissed;
  private string lastName;
  private string firstName;
  private string middleInitial;
  private string firmName;
  private int? phone;
  private int? faxNumber;
  private string courtCaseNumber;
  private string fipsStateAbbreviation;
  private string fipsCountyAbbreviation;
  private string tribCountry;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string emailAddress;
  private string barNumber;
  private string consentIndicator;
  private string note;
  private string casNumber;
  private string cspNumber;
}
