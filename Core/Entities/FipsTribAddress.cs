// The source file: FIPS_TRIB_ADDRESS, ID: 371434420, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLEN
/// This entity contains the address information fot the FIPS.  It also contains
/// address information for foreign tribunals.  All the US tribunals will have
/// a FIPS code and the FIPS address will be used as the tribunal address.
/// The relationship between TRIBUNAL and FIPS_TRIB_ADDRESS is redundant in the 
/// case of US tribunals.  It is however maintained by the online procedure to
/// minimize the impact on the already developed procedures.
/// </summary>
[Serializable]
public partial class FipsTribAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FipsTribAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FipsTribAddress(FipsTribAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FipsTribAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FipsTribAddress that)
  {
    base.Assign(that);
    identifier = that.identifier;
    faxExtension = that.faxExtension;
    faxAreaCode = that.faxAreaCode;
    phoneExtension = that.phoneExtension;
    areaCode = that.areaCode;
    type1 = that.type1;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    state = that.state;
    zipCode = that.zipCode;
    zip4 = that.zip4;
    zip3 = that.zip3;
    county = that.county;
    street3 = that.street3;
    street4 = that.street4;
    province = that.province;
    postalCode = that.postalCode;
    country = that.country;
    phoneNumber = that.phoneNumber;
    faxNumber = that.faxNumber;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    fipLocation = that.fipLocation;
    fipCounty = that.fipCounty;
    fipState = that.fipState;
    trbId = that.trbId;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute uniquely identifies an occurence of the FIPS TRIB ADDRESS 
  /// entity type.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the FAX_EXTENSION attribute.</summary>
  public const int FaxExtension_MaxLength = 5;

  /// <summary>
  /// The value of the FAX_EXTENSION attribute.
  /// The extension of the fax number for the tribunal.
  /// </summary>
  [JsonPropertyName("faxExtension")]
  [Member(Index = 2, Type = MemberType.Char, Length = FaxExtension_MaxLength, Optional
    = true)]
  public string FaxExtension
  {
    get => faxExtension;
    set => faxExtension = value != null
      ? TrimEnd(Substring(value, 1, FaxExtension_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the FAX_AREA_CODE attribute.
  /// The fax area code of the tribunal or first 3 digits of the phone number.
  /// </summary>
  [JsonPropertyName("faxAreaCode")]
  [Member(Index = 3, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? FaxAreaCode
  {
    get => faxAreaCode;
    set => faxAreaCode = value;
  }

  /// <summary>Length of the PHONE_EXTENSION attribute.</summary>
  public const int PhoneExtension_MaxLength = 5;

  /// <summary>
  /// The value of the PHONE_EXTENSION attribute.
  /// The extension of the tribunal phone number.
  /// </summary>
  [JsonPropertyName("phoneExtension")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = PhoneExtension_MaxLength, Optional = true)]
  public string PhoneExtension
  {
    get => phoneExtension;
    set => phoneExtension = value != null
      ? TrimEnd(Substring(value, 1, PhoneExtension_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the AREA_CODE attribute.
  /// The local area code or first 3 digits of the tribunal telephone number.
  /// </summary>
  [JsonPropertyName("areaCode")]
  [Member(Index = 5, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? AreaCode
  {
    get => areaCode;
    set => areaCode = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This identifies the type of address.
  /// Ex.  Mailing Address, Physical Address, etc.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Type1_MaxLength)]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>Length of the STREET_1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_1 attribute.
  /// The street address of the tribunal.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Street1_MaxLength)]
  public string Street1
  {
    get => street1 ?? "";
    set => street1 = TrimEnd(Substring(value, 1, Street1_MaxLength));
  }

  /// <summary>
  /// The json value of the Street1 attribute.</summary>
  [JsonPropertyName("street1")]
  [Computed]
  public string Street1_Json
  {
    get => NullIf(Street1, "");
    set => Street1 = value;
  }

  /// <summary>Length of the STREET_2 attribute.</summary>
  public const int Street2_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_2 attribute.
  /// The street address of the tribunal.
  /// </summary>
  [JsonPropertyName("street2")]
  [Member(Index = 8, Type = MemberType.Char, Length = Street2_MaxLength, Optional
    = true)]
  public string Street2
  {
    get => street2;
    set => street2 = value != null
      ? TrimEnd(Substring(value, 1, Street2_MaxLength)) : null;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 15;

  /// <summary>
  /// The value of the CITY attribute.
  /// The city of the tribunal.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = City_MaxLength)]
  public string City
  {
    get => city ?? "";
    set => city = TrimEnd(Substring(value, 1, City_MaxLength));
  }

  /// <summary>
  /// The json value of the City attribute.</summary>
  [JsonPropertyName("city")]
  [Computed]
  public string City_Json
  {
    get => NullIf(City, "");
    set => City = value;
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// The state/province of the tribunal.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = State_MaxLength)]
  public string State
  {
    get => state ?? "";
    set => state = TrimEnd(Substring(value, 1, State_MaxLength));
  }

  /// <summary>
  /// The json value of the State attribute.</summary>
  [JsonPropertyName("state")]
  [Computed]
  public string State_Json
  {
    get => NullIf(State, "");
    set => State = value;
  }

  /// <summary>Length of the ZIP_CODE attribute.</summary>
  public const int ZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE attribute.
  /// The residential mailing code of the tribunal.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = ZipCode_MaxLength)]
  public string ZipCode
  {
    get => zipCode ?? "";
    set => zipCode = TrimEnd(Substring(value, 1, ZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ZipCode attribute.</summary>
  [JsonPropertyName("zipCode")]
  [Computed]
  public string ZipCode_Json
  {
    get => NullIf(ZipCode, "");
    set => ZipCode = value;
  }

  /// <summary>Length of the ZIP4 attribute.</summary>
  public const int Zip4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP4 attribute.
  /// The four additional characters that further identify the Zip Code.
  /// </summary>
  [JsonPropertyName("zip4")]
  [Member(Index = 12, Type = MemberType.Char, Length = Zip4_MaxLength, Optional
    = true)]
  public string Zip4
  {
    get => zip4;
    set => zip4 = value != null
      ? TrimEnd(Substring(value, 1, Zip4_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP3 attribute.</summary>
  public const int Zip3_MaxLength = 3;

  /// <summary>
  /// The value of the ZIP3 attribute.
  /// The three additional characters that further identify the Zip Code and 
  /// Zip4.
  /// </summary>
  [JsonPropertyName("zip3")]
  [Member(Index = 13, Type = MemberType.Char, Length = Zip3_MaxLength, Optional
    = true)]
  public string Zip3
  {
    get => zip3;
    set => zip3 = value != null
      ? TrimEnd(Substring(value, 1, Zip3_MaxLength)) : null;
  }

  /// <summary>Length of the COUNTY attribute.</summary>
  public const int County_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTY attribute.
  /// Indicates the County the Tribunal is located.
  /// </summary>
  [JsonPropertyName("county")]
  [Member(Index = 14, Type = MemberType.Char, Length = County_MaxLength, Optional
    = true)]
  public string County
  {
    get => county;
    set => county = value != null
      ? TrimEnd(Substring(value, 1, County_MaxLength)) : null;
  }

  /// <summary>Length of the STREET_3 attribute.</summary>
  public const int Street3_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_3 attribute.
  /// First line of a postal address
  /// </summary>
  [JsonPropertyName("street3")]
  [Member(Index = 15, Type = MemberType.Char, Length = Street3_MaxLength, Optional
    = true)]
  public string Street3
  {
    get => street3;
    set => street3 = value != null
      ? TrimEnd(Substring(value, 1, Street3_MaxLength)) : null;
  }

  /// <summary>Length of the STREET 4 attribute.</summary>
  public const int Street4_MaxLength = 25;

  /// <summary>
  /// The value of the STREET 4 attribute.
  /// Second line of a postal address
  /// </summary>
  [JsonPropertyName("street4")]
  [Member(Index = 16, Type = MemberType.Char, Length = Street4_MaxLength, Optional
    = true)]
  public string Street4
  {
    get => street4;
    set => street4 = value != null
      ? TrimEnd(Substring(value, 1, Street4_MaxLength)) : null;
  }

  /// <summary>Length of the PROVINCE attribute.</summary>
  public const int Province_MaxLength = 5;

  /// <summary>
  /// The value of the PROVINCE attribute.
  /// Region in which an address is located
  /// </summary>
  [JsonPropertyName("province")]
  [Member(Index = 17, Type = MemberType.Char, Length = Province_MaxLength, Optional
    = true)]
  public string Province
  {
    get => province;
    set => province = value != null
      ? TrimEnd(Substring(value, 1, Province_MaxLength)) : null;
  }

  /// <summary>Length of the POSTAL CODE attribute.</summary>
  public const int PostalCode_MaxLength = 10;

  /// <summary>
  /// The value of the POSTAL CODE attribute.
  /// This incorporates worldwide zipcode formats.
  /// </summary>
  [JsonPropertyName("postalCode")]
  [Member(Index = 18, Type = MemberType.Char, Length = PostalCode_MaxLength, Optional
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
  /// Code indicating the country in which the address is located
  /// </summary>
  [JsonPropertyName("country")]
  [Member(Index = 19, Type = MemberType.Char, Length = Country_MaxLength, Optional
    = true)]
  public string Country
  {
    get => country;
    set => country = value != null
      ? TrimEnd(Substring(value, 1, Country_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHONE_NUMBER attribute.
  /// The phone number of the Tribunal.
  /// </summary>
  [JsonPropertyName("phoneNumber")]
  [Member(Index = 20, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? PhoneNumber
  {
    get => phoneNumber;
    set => phoneNumber = value;
  }

  /// <summary>
  /// The value of the FAX_NUMBER attribute.
  /// The fax number of the Tribunal.
  /// </summary>
  [JsonPropertyName("faxNumber")]
  [Member(Index = 21, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? FaxNumber
  {
    get => faxNumber;
    set => faxNumber = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrence of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 23, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person that last updated the occurrence of the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrence of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 25, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>
  /// The value of the LOCATION attribute.
  /// The last two positions of the FIPS code which identify a location within 
  /// the county.
  /// </summary>
  [JsonPropertyName("fipLocation")]
  [Member(Index = 26, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? FipLocation
  {
    get => fipLocation;
    set => fipLocation = value;
  }

  /// <summary>
  /// The value of the COUNTY attribute.
  /// This is 3-5 position of the FIPS code identifying the county.
  /// </summary>
  [JsonPropertyName("fipCounty")]
  [Member(Index = 27, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? FipCounty
  {
    get => fipCounty;
    set => fipCounty = value;
  }

  /// <summary>
  /// The value of the STATE attribute.
  /// The first two characters of the FIPS code which identify the state.
  /// </summary>
  [JsonPropertyName("fipState")]
  [Member(Index = 28, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? FipState
  {
    get => fipState;
    set => fipState = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute uniquely identifies a tribunal record. It is automatically 
  /// generated by the system starting from 1.
  /// </summary>
  [JsonPropertyName("trbId")]
  [Member(Index = 29, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TrbId
  {
    get => trbId;
    set => trbId = value;
  }

  private int identifier;
  private string faxExtension;
  private int? faxAreaCode;
  private string phoneExtension;
  private int? areaCode;
  private string type1;
  private string street1;
  private string street2;
  private string city;
  private string state;
  private string zipCode;
  private string zip4;
  private string zip3;
  private string county;
  private string street3;
  private string street4;
  private string province;
  private string postalCode;
  private string country;
  private int? phoneNumber;
  private int? faxNumber;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private int? fipLocation;
  private int? fipCounty;
  private int? fipState;
  private int? trbId;
}
