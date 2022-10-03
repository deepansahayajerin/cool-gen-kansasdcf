// The source file: LOCATE_REQUEST, ID: 374417970, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// 
/// Records locate information of requests for, and receipt of,
/// information for CSE persons meeting automated locate qualification.
/// The generated request and response are sent to and received by state
/// licensing agencies and state agencies (which have jurisdiction over
/// real and personal property).
/// </summary>
[Serializable]
public partial class LocateRequest: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LocateRequest()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LocateRequest(LocateRequest that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LocateRequest Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LocateRequest that)
  {
    base.Assign(that);
    socialSecurityNumber = that.socialSecurityNumber;
    dateOfBirth = that.dateOfBirth;
    csePersonNumber = that.csePersonNumber;
    requestDate = that.requestDate;
    responseDate = that.responseDate;
    licenseIssuedDate = that.licenseIssuedDate;
    licenseExpirationDate = that.licenseExpirationDate;
    licenseSuspendedDate = that.licenseSuspendedDate;
    licenseNumber = that.licenseNumber;
    agencyNumber = that.agencyNumber;
    sequenceNumber = that.sequenceNumber;
    licenseSourceName = that.licenseSourceName;
    street1 = that.street1;
    addressType = that.addressType;
    street2 = that.street2;
    street3 = that.street3;
    street4 = that.street4;
    city = that.city;
    state = that.state;
    zipCode5 = that.zipCode5;
    zipCode4 = that.zipCode4;
    zipCode3 = that.zipCode3;
    province = that.province;
    postalCode = that.postalCode;
    country = that.country;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    licenseSuspensionInd = that.licenseSuspensionInd;
  }

  /// <summary>Length of the SOCIAL_SECURITY_NUMBER attribute.</summary>
  public const int SocialSecurityNumber_MaxLength = 9;

  /// <summary>
  /// The value of the SOCIAL_SECURITY_NUMBER attribute.
  /// The social security number of the CSE member for whom the locate request 
  /// is generated.
  /// </summary>
  [JsonPropertyName("socialSecurityNumber")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = SocialSecurityNumber_MaxLength, Optional = true)]
  public string SocialSecurityNumber
  {
    get => socialSecurityNumber;
    set => socialSecurityNumber = value != null
      ? TrimEnd(Substring(value, 1, SocialSecurityNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DATE_OF_BIRTH attribute.
  /// The date of birth fro the CSE member for whom the locate request is 
  /// generated.
  /// </summary>
  [JsonPropertyName("dateOfBirth")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfBirth
  {
    get => dateOfBirth;
    set => dateOfBirth = value;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// A SRS system generated number which will be used by the users to identify 
  /// a person. This will have a business meaning, and is unique.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CsePersonNumber_MaxLength)]
    
  public string CsePersonNumber
  {
    get => csePersonNumber ?? "";
    set => csePersonNumber =
      TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CsePersonNumber attribute.</summary>
  [JsonPropertyName("csePersonNumber")]
  [Computed]
  public string CsePersonNumber_Json
  {
    get => NullIf(CsePersonNumber, "");
    set => CsePersonNumber = value;
  }

  /// <summary>
  /// The value of the REQUEST_DATE attribute.
  /// The date which the request was created.
  /// </summary>
  [JsonPropertyName("requestDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? RequestDate
  {
    get => requestDate;
    set => requestDate = value;
  }

  /// <summary>
  /// The value of the RESPONSE_DATE attribute.
  /// The date which the request was received from the participating state 
  /// agency.
  /// </summary>
  [JsonPropertyName("responseDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? ResponseDate
  {
    get => responseDate;
    set => responseDate = value;
  }

  /// <summary>
  /// The value of the LICENSE_ISSUED_DATE attribute.
  /// The date which the license, as specified by the participating state 
  /// agency, was issued.
  /// </summary>
  [JsonPropertyName("licenseIssuedDate")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? LicenseIssuedDate
  {
    get => licenseIssuedDate;
    set => licenseIssuedDate = value;
  }

  /// <summary>
  /// The value of the LICENSE_EXPIRATION_DATE attribute.
  /// The date which the license, as specified by the participating state 
  /// agencey,expired.
  /// </summary>
  [JsonPropertyName("licenseExpirationDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? LicenseExpirationDate
  {
    get => licenseExpirationDate;
    set => licenseExpirationDate = value;
  }

  /// <summary>
  /// The value of the LICENSE_SUSPENDED_DATE attribute.
  /// The date which the license, as specified by the participating state 
  /// agencey, was suspended.
  /// </summary>
  [JsonPropertyName("licenseSuspendedDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? LicenseSuspendedDate
  {
    get => licenseSuspendedDate;
    set => licenseSuspendedDate = value;
  }

  /// <summary>Length of the LICENSE_NUMBER attribute.</summary>
  public const int LicenseNumber_MaxLength = 25;

  /// <summary>
  /// The value of the LICENSE_NUMBER attribute.
  /// The number assigned to a license, as specified by the participating state 
  /// agency.
  /// </summary>
  [JsonPropertyName("licenseNumber")]
  [Member(Index = 9, Type = MemberType.Char, Length = LicenseNumber_MaxLength, Optional
    = true)]
  public string LicenseNumber
  {
    get => licenseNumber;
    set => licenseNumber = value != null
      ? TrimEnd(Substring(value, 1, LicenseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the AGENCY_NUMBER attribute.</summary>
  public const int AgencyNumber_MaxLength = 5;

  /// <summary>
  /// The value of the AGENCY_NUMBER attribute.
  /// A number which represents a particular participating state agency, as 
  /// specified by SRS.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = AgencyNumber_MaxLength)]
  public string AgencyNumber
  {
    get => agencyNumber ?? "";
    set => agencyNumber = TrimEnd(Substring(value, 1, AgencyNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the AgencyNumber attribute.</summary>
  [JsonPropertyName("agencyNumber")]
  [Computed]
  public string AgencyNumber_Json
  {
    get => NullIf(AgencyNumber, "");
    set => AgencyNumber = value;
  }

  /// <summary>
  /// The value of the SEQUENCE_NUMBER attribute.
  /// A number which represents a particular source whichin a participating 
  /// state agency.
  /// </summary>
  [JsonPropertyName("sequenceNumber")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 2)]
  public int SequenceNumber
  {
    get => sequenceNumber;
    set => sequenceNumber = value;
  }

  /// <summary>Length of the LICENSE_SOURCE_NAME attribute.</summary>
  public const int LicenseSourceName_MaxLength = 25;

  /// <summary>
  /// The value of the LICENSE_SOURCE_NAME attribute.
  /// The name of the licensing source within a perticular state agency.
  /// </summary>
  [JsonPropertyName("licenseSourceName")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = LicenseSourceName_MaxLength, Optional = true)]
  public string LicenseSourceName
  {
    get => licenseSourceName;
    set => licenseSourceName = value != null
      ? TrimEnd(Substring(value, 1, LicenseSourceName_MaxLength)) : null;
  }

  /// <summary>Length of the STREET_1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_1 attribute.
  /// The first line of the street address for the CSE member for whom the 
  /// locate request was received.
  /// </summary>
  [JsonPropertyName("street1")]
  [Member(Index = 13, Type = MemberType.Char, Length = Street1_MaxLength, Optional
    = true)]
  public string Street1
  {
    get => street1;
    set => street1 = value != null
      ? TrimEnd(Substring(value, 1, Street1_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_TYPE attribute.</summary>
  public const int AddressType_MaxLength = 1;

  /// <summary>
  /// The value of the ADDRESS_TYPE attribute.
  /// The type of address received from a participating state agency for a CSE 
  /// member (&quot;M&quot; for mailing, &quot;R&quot; for residential)
  /// </summary>
  [JsonPropertyName("addressType")]
  [Member(Index = 14, Type = MemberType.Char, Length = AddressType_MaxLength, Optional
    = true)]
  public string AddressType
  {
    get => addressType;
    set => addressType = value != null
      ? TrimEnd(Substring(value, 1, AddressType_MaxLength)) : null;
  }

  /// <summary>Length of the STREET_2 attribute.</summary>
  public const int Street2_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_2 attribute.
  /// The second line of the street address for the CSE member for whom the 
  /// locate request was received.
  /// </summary>
  [JsonPropertyName("street2")]
  [Member(Index = 15, Type = MemberType.Char, Length = Street2_MaxLength, Optional
    = true)]
  public string Street2
  {
    get => street2;
    set => street2 = value != null
      ? TrimEnd(Substring(value, 1, Street2_MaxLength)) : null;
  }

  /// <summary>Length of the STREET_3 attribute.</summary>
  public const int Street3_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_3 attribute.
  /// The third line of the street address for the CSE member for whom the 
  /// locate request was received.
  /// </summary>
  [JsonPropertyName("street3")]
  [Member(Index = 16, Type = MemberType.Char, Length = Street3_MaxLength, Optional
    = true)]
  public string Street3
  {
    get => street3;
    set => street3 = value != null
      ? TrimEnd(Substring(value, 1, Street3_MaxLength)) : null;
  }

  /// <summary>Length of the STREET_4 attribute.</summary>
  public const int Street4_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_4 attribute.
  /// The fourth line of the street address for the CSE member for whom the 
  /// locate request was received.
  /// </summary>
  [JsonPropertyName("street4")]
  [Member(Index = 17, Type = MemberType.Char, Length = Street4_MaxLength, Optional
    = true)]
  public string Street4
  {
    get => street4;
    set => street4 = value != null
      ? TrimEnd(Substring(value, 1, Street4_MaxLength)) : null;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 15;

  /// <summary>
  /// The value of the CITY attribute.
  /// Community where the address is located.
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 18, Type = MemberType.Char, Length = City_MaxLength, Optional
    = true)]
  public string City
  {
    get => city;
    set => city = value != null
      ? TrimEnd(Substring(value, 1, City_MaxLength)) : null;
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// Region in which the address is located.
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 19, Type = MemberType.Char, Length = State_MaxLength, Optional
    = true)]
  public string State
  {
    get => state;
    set => state = value != null
      ? TrimEnd(Substring(value, 1, State_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE_5 attribute.</summary>
  public const int ZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE_5 attribute.
  /// The five character ZIP of an address.
  /// </summary>
  [JsonPropertyName("zipCode5")]
  [Member(Index = 20, Type = MemberType.Char, Length = ZipCode5_MaxLength, Optional
    = true)]
  public string ZipCode5
  {
    get => zipCode5;
    set => zipCode5 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode5_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE_4 attribute.</summary>
  public const int ZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP_CODE_4 attribute.
  /// The second part of the zip code consisting of four digits.
  /// </summary>
  [JsonPropertyName("zipCode4")]
  [Member(Index = 21, Type = MemberType.Char, Length = ZipCode4_MaxLength, Optional
    = true)]
  public string ZipCode4
  {
    get => zipCode4;
    set => zipCode4 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode4_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE_3 attribute.</summary>
  public const int ZipCode3_MaxLength = 3;

  /// <summary>
  /// The value of the ZIP_CODE_3 attribute.
  /// The third and last part of the zip code consisting of three digits.
  /// </summary>
  [JsonPropertyName("zipCode3")]
  [Member(Index = 22, Type = MemberType.Char, Length = ZipCode3_MaxLength, Optional
    = true)]
  public string ZipCode3
  {
    get => zipCode3;
    set => zipCode3 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode3_MaxLength)) : null;
  }

  /// <summary>Length of the PROVINCE attribute.</summary>
  public const int Province_MaxLength = 5;

  /// <summary>
  /// The value of the PROVINCE attribute.
  /// Foreign region in which an address is located; if the value is spaces then
  /// it is a domestic address.
  /// </summary>
  [JsonPropertyName("province")]
  [Member(Index = 23, Type = MemberType.Char, Length = Province_MaxLength, Optional
    = true)]
  public string Province
  {
    get => province;
    set => province = value != null
      ? TrimEnd(Substring(value, 1, Province_MaxLength)) : null;
  }

  /// <summary>Length of the POSTAL_CODE attribute.</summary>
  public const int PostalCode_MaxLength = 10;

  /// <summary>
  /// The value of the POSTAL_CODE attribute.
  /// The ten character ZIP of an foreign address which incorporates worldwide 
  /// zip code formats.
  /// </summary>
  [JsonPropertyName("postalCode")]
  [Member(Index = 24, Type = MemberType.Char, Length = PostalCode_MaxLength, Optional
    = true)]
  public string PostalCode
  {
    get => postalCode;
    set => postalCode = value != null
      ? TrimEnd(Substring(value, 1, PostalCode_MaxLength)) : null;
  }

  /// <summary>Length of the COUNTRY attribute.</summary>
  public const int Country_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTRY attribute.
  /// Code indicating the country in which the address is located.
  /// </summary>
  [JsonPropertyName("country")]
  [Member(Index = 25, Type = MemberType.Char, Length = Country_MaxLength, Optional
    = true)]
  public string Country
  {
    get => country;
    set => country = value != null
      ? TrimEnd(Substring(value, 1, Country_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation for the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 26, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER identification which created the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Last timestamp of alteration for the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 28, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// USER identification which last updated the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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

  /// <summary>Length of the LICENSE_SUSPENSION_IND attribute.</summary>
  public const int LicenseSuspensionInd_MaxLength = 1;

  /// <summary>
  /// The value of the LICENSE_SUSPENSION_IND attribute.
  /// This attribute defines the suspension status of a CSE Person's 
  /// professional license.
  /// N=Not suspended
  /// 
  /// Y=Suspended
  /// 
  /// W=Waiting for locate verification.
  /// </summary>
  [JsonPropertyName("licenseSuspensionInd")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = LicenseSuspensionInd_MaxLength, Optional = true)]
  public string LicenseSuspensionInd
  {
    get => licenseSuspensionInd;
    set => licenseSuspensionInd = value != null
      ? TrimEnd(Substring(value, 1, LicenseSuspensionInd_MaxLength)) : null;
  }

  private string socialSecurityNumber;
  private DateTime? dateOfBirth;
  private string csePersonNumber;
  private DateTime? requestDate;
  private DateTime? responseDate;
  private DateTime? licenseIssuedDate;
  private DateTime? licenseExpirationDate;
  private DateTime? licenseSuspendedDate;
  private string licenseNumber;
  private string agencyNumber;
  private int sequenceNumber;
  private string licenseSourceName;
  private string street1;
  private string addressType;
  private string street2;
  private string street3;
  private string street4;
  private string city;
  private string state;
  private string zipCode5;
  private string zipCode4;
  private string zipCode3;
  private string province;
  private string postalCode;
  private string country;
  private DateTime? createdTimestamp;
  private string createdBy;
  private DateTime? lastUpdatedTimestamp;
  private string lastUpdatedBy;
  private string licenseSuspensionInd;
}
