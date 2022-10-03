// The source file: SP_PRINT_WORK_SET, ID: 371424065, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: SRVINIT
/// May 17, 95: Contains 2 attrs viz., Field Name and Reqd_Ind.
/// </summary>
[Serializable]
public partial class SpPrintWorkSet: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public SpPrintWorkSet()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public SpPrintWorkSet(SpPrintWorkSet that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new SpPrintWorkSet Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(SpPrintWorkSet that)
  {
    base.Assign(that);
    locationType = that.locationType;
    street1 = that.street1;
    street2 = that.street2;
    street3 = that.street3;
    street4 = that.street4;
    city = that.city;
    state = that.state;
    county = that.county;
    zipCode = that.zipCode;
    zip4 = that.zip4;
    zip3 = that.zip3;
    province = that.province;
    postalCode = that.postalCode;
    country = that.country;
    lastName = that.lastName;
    firstName = that.firstName;
    midInitial = that.midInitial;
    text15 = that.text15;
    phoneAreaCode = that.phoneAreaCode;
    phoneExt = that.phoneExt;
    phone7Digit = that.phone7Digit;
    number2 = that.number2;
    indReqd = that.indReqd;
  }

  /// <summary>Length of the LOCATION_TYPE attribute.</summary>
  public const int LocationType_MaxLength = 1;

  /// <summary>
  /// The value of the LOCATION_TYPE attribute.
  /// An attribute denoting whether the address is a domestic address or a 
  /// foreign address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = LocationType_MaxLength)]
  [Value("D")]
  [Value("F")]
  public string LocationType
  {
    get => locationType ?? "";
    set => locationType = TrimEnd(Substring(value, 1, LocationType_MaxLength));
  }

  /// <summary>
  /// The json value of the LocationType attribute.</summary>
  [JsonPropertyName("locationType")]
  [Computed]
  public string LocationType_Json
  {
    get => NullIf(LocationType, "");
    set => LocationType = value;
  }

  /// <summary>Length of the STREET_1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_1 attribute.
  /// First line of a postal address
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Street1_MaxLength)]
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
  /// Second line of a postal address
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Street2_MaxLength)]
  public string Street2
  {
    get => street2 ?? "";
    set => street2 = TrimEnd(Substring(value, 1, Street2_MaxLength));
  }

  /// <summary>
  /// The json value of the Street2 attribute.</summary>
  [JsonPropertyName("street2")]
  [Computed]
  public string Street2_Json
  {
    get => NullIf(Street2, "");
    set => Street2 = value;
  }

  /// <summary>Length of the STREET 3 attribute.</summary>
  public const int Street3_MaxLength = 25;

  /// <summary>
  /// The value of the STREET 3 attribute.
  /// First line of a postal address
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Street3_MaxLength)]
  public string Street3
  {
    get => street3 ?? "";
    set => street3 = TrimEnd(Substring(value, 1, Street3_MaxLength));
  }

  /// <summary>
  /// The json value of the Street3 attribute.</summary>
  [JsonPropertyName("street3")]
  [Computed]
  public string Street3_Json
  {
    get => NullIf(Street3, "");
    set => Street3 = value;
  }

  /// <summary>Length of the STREET 4 attribute.</summary>
  public const int Street4_MaxLength = 25;

  /// <summary>
  /// The value of the STREET 4 attribute.
  /// Second line of a postal address
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = Street4_MaxLength)]
  public string Street4
  {
    get => street4 ?? "";
    set => street4 = TrimEnd(Substring(value, 1, Street4_MaxLength));
  }

  /// <summary>
  /// The json value of the Street4 attribute.</summary>
  [JsonPropertyName("street4")]
  [Computed]
  public string Street4_Json
  {
    get => NullIf(Street4, "");
    set => Street4 = value;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 15;

  /// <summary>
  /// The value of the CITY attribute.
  /// Community where the address is located
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = City_MaxLength)]
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
  /// Region in which the address is located
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = State_MaxLength)]
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

  /// <summary>Length of the COUNTY attribute.</summary>
  public const int County_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTY attribute.
  /// The county in which this address is found.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = County_MaxLength)]
  public string County
  {
    get => county ?? "";
    set => county = TrimEnd(Substring(value, 1, County_MaxLength));
  }

  /// <summary>
  /// The json value of the County attribute.</summary>
  [JsonPropertyName("county")]
  [Computed]
  public string County_Json
  {
    get => NullIf(County, "");
    set => County = value;
  }

  /// <summary>Length of the ZIP CODE attribute.</summary>
  public const int ZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP CODE attribute.
  /// This is the basic five-character ZIP which is part of an address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = ZipCode_MaxLength)]
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
  /// Basic mailing code. Part of an address
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = Zip4_MaxLength)]
  public string Zip4
  {
    get => zip4 ?? "";
    set => zip4 = TrimEnd(Substring(value, 1, Zip4_MaxLength));
  }

  /// <summary>
  /// The json value of the Zip4 attribute.</summary>
  [JsonPropertyName("zip4")]
  [Computed]
  public string Zip4_Json
  {
    get => NullIf(Zip4, "");
    set => Zip4 = value;
  }

  /// <summary>Length of the ZIP3 attribute.</summary>
  public const int Zip3_MaxLength = 3;

  /// <summary>
  /// The value of the ZIP3 attribute.
  /// Basic Mailing code; part of an address; This is not an enterable field - 
  /// there is a CAB to calculate this.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Zip3_MaxLength)]
  public string Zip3
  {
    get => zip3 ?? "";
    set => zip3 = TrimEnd(Substring(value, 1, Zip3_MaxLength));
  }

  /// <summary>
  /// The json value of the Zip3 attribute.</summary>
  [JsonPropertyName("zip3")]
  [Computed]
  public string Zip3_Json
  {
    get => NullIf(Zip3, "");
    set => Zip3 = value;
  }

  /// <summary>Length of the PROVINCE attribute.</summary>
  public const int Province_MaxLength = 5;

  /// <summary>
  /// The value of the PROVINCE attribute.
  /// Region in which an address is located
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = Province_MaxLength)]
  public string Province
  {
    get => province ?? "";
    set => province = TrimEnd(Substring(value, 1, Province_MaxLength));
  }

  /// <summary>
  /// The json value of the Province attribute.</summary>
  [JsonPropertyName("province")]
  [Computed]
  public string Province_Json
  {
    get => NullIf(Province, "");
    set => Province = value;
  }

  /// <summary>Length of the POSTAL CODE attribute.</summary>
  public const int PostalCode_MaxLength = 10;

  /// <summary>
  /// The value of the POSTAL CODE attribute.
  /// This incorporates worldwide zipcode formats.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = PostalCode_MaxLength)]
  public string PostalCode
  {
    get => postalCode ?? "";
    set => postalCode = TrimEnd(Substring(value, 1, PostalCode_MaxLength));
  }

  /// <summary>
  /// The json value of the PostalCode attribute.</summary>
  [JsonPropertyName("postalCode")]
  [Computed]
  public string PostalCode_Json
  {
    get => NullIf(PostalCode, "");
    set => PostalCode = value;
  }

  /// <summary>Length of the COUNTRY attribute.</summary>
  public const int Country_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTRY attribute.
  /// Code indicating the country in which the address is located
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = Country_MaxLength)]
  public string Country
  {
    get => country ?? "";
    set => country = TrimEnd(Substring(value, 1, Country_MaxLength));
  }

  /// <summary>
  /// The json value of the Country attribute.</summary>
  [JsonPropertyName("country")]
  [Computed]
  public string Country_Json
  {
    get => NullIf(Country, "");
    set => Country = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// The last name of the Service Provider linked to the Office Service 
  /// Provider represented by this occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = LastName_MaxLength)]
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

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// The first name of the Service Provider linked to the Office Service 
  /// Provider represented by this occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = FirstName_MaxLength)]
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

  /// <summary>Length of the MID_INITIAL attribute.</summary>
  public const int MidInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MID_INITIAL attribute.
  /// The middle initial of the Service Provider linked to the Office Service 
  /// Provider represented by this occurrence
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = MidInitial_MaxLength)]
  public string MidInitial
  {
    get => midInitial ?? "";
    set => midInitial = TrimEnd(Substring(value, 1, MidInitial_MaxLength));
  }

  /// <summary>
  /// The json value of the MidInitial attribute.</summary>
  [JsonPropertyName("midInitial")]
  [Computed]
  public string MidInitial_Json
  {
    get => NullIf(MidInitial, "");
    set => MidInitial = value;
  }

  /// <summary>Length of the TEXT_15 attribute.</summary>
  public const int Text15_MaxLength = 15;

  /// <summary>
  /// The value of the TEXT_15 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = Text15_MaxLength)]
  public string Text15
  {
    get => text15 ?? "";
    set => text15 = TrimEnd(Substring(value, 1, Text15_MaxLength));
  }

  /// <summary>
  /// The json value of the Text15 attribute.</summary>
  [JsonPropertyName("text15")]
  [Computed]
  public string Text15_Json
  {
    get => NullIf(Text15, "");
    set => Text15 = value;
  }

  /// <summary>
  /// The value of the PHONE_AREA_CODE attribute.
  /// The 3-digit area code for the work phone number of the contact.
  /// </summary>
  [JsonPropertyName("phoneAreaCode")]
  [DefaultValue(0)]
  [Member(Index = 19, Type = MemberType.Number, Length = 3)]
  public int PhoneAreaCode
  {
    get => phoneAreaCode;
    set => phoneAreaCode = value;
  }

  /// <summary>Length of the PHONE_EXT attribute.</summary>
  public const int PhoneExt_MaxLength = 5;

  /// <summary>
  /// The value of the PHONE_EXT attribute.
  /// The 5 digit extension for the work phone of the contact.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = PhoneExt_MaxLength)]
  public string PhoneExt
  {
    get => phoneExt ?? "";
    set => phoneExt = TrimEnd(Substring(value, 1, PhoneExt_MaxLength));
  }

  /// <summary>
  /// The json value of the PhoneExt attribute.</summary>
  [JsonPropertyName("phoneExt")]
  [Computed]
  public string PhoneExt_Json
  {
    get => NullIf(PhoneExt, "");
    set => PhoneExt = value;
  }

  /// <summary>
  /// The value of the PHONE_7_DIGIT attribute.
  /// The 7-digit Work or other phone number for contact.
  /// </summary>
  [JsonPropertyName("phone7Digit")]
  [DefaultValue(0)]
  [Member(Index = 21, Type = MemberType.Number, Length = 7)]
  public int Phone7Digit
  {
    get => phone7Digit;
    set => phone7Digit = value;
  }

  /// <summary>
  /// The value of the NUMBER_2 attribute.
  /// </summary>
  [JsonPropertyName("number2")]
  [DefaultValue(0)]
  [Member(Index = 22, Type = MemberType.Number, Length = 2)]
  public int Number2
  {
    get => number2;
    set => number2 = value;
  }

  /// <summary>Length of the IND_REQD attribute.</summary>
  public const int IndReqd_MaxLength = 1;

  /// <summary>
  /// The value of the IND_REQD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length = IndReqd_MaxLength)]
  public string IndReqd
  {
    get => indReqd ?? "";
    set => indReqd = TrimEnd(Substring(value, 1, IndReqd_MaxLength));
  }

  /// <summary>
  /// The json value of the IndReqd attribute.</summary>
  [JsonPropertyName("indReqd")]
  [Computed]
  public string IndReqd_Json
  {
    get => NullIf(IndReqd, "");
    set => IndReqd = value;
  }

  private string locationType;
  private string street1;
  private string street2;
  private string street3;
  private string street4;
  private string city;
  private string state;
  private string county;
  private string zipCode;
  private string zip4;
  private string zip3;
  private string province;
  private string postalCode;
  private string country;
  private string lastName;
  private string firstName;
  private string midInitial;
  private string text15;
  private int phoneAreaCode;
  private string phoneExt;
  private int phone7Digit;
  private int number2;
  private string indReqd;
}
