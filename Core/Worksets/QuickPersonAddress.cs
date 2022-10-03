// The source file: QUICK_PERSON_ADDRESS, ID: 374543766, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class QuickPersonAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickPersonAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickPersonAddress(QuickPersonAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickPersonAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickPersonAddress that)
  {
    base.Assign(that);
    addressType = that.addressType;
    locationType = that.locationType;
    date = that.date;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    state = that.state;
    zip = that.zip;
    zip4 = that.zip4;
    street3 = that.street3;
    street4 = that.street4;
    country = that.country;
    province = that.province;
    postalCode = that.postalCode;
  }

  /// <summary>Length of the ADDRESS_TYPE attribute.</summary>
  public const int AddressType_MaxLength = 1;

  /// <summary>
  /// The value of the ADDRESS_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = AddressType_MaxLength)]
  public string AddressType
  {
    get => addressType ?? "";
    set => addressType = TrimEnd(Substring(value, 1, AddressType_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressType attribute.</summary>
  [JsonPropertyName("addressType")]
  [Computed]
  public string AddressType_Json
  {
    get => NullIf(AddressType, "");
    set => AddressType = value;
  }

  /// <summary>Length of the LOCATION_TYPE attribute.</summary>
  public const int LocationType_MaxLength = 1;

  /// <summary>
  /// The value of the LOCATION_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = LocationType_MaxLength)]
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

  /// <summary>Length of the DATE attribute.</summary>
  public const int Date_MaxLength = 8;

  /// <summary>
  /// The value of the DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Date_MaxLength)]
  public string Date
  {
    get => date ?? "";
    set => date = TrimEnd(Substring(value, 1, Date_MaxLength));
  }

  /// <summary>
  /// The json value of the Date attribute.</summary>
  [JsonPropertyName("date")]
  [Computed]
  public string Date_Json
  {
    get => NullIf(Date, "");
    set => Date = value;
  }

  /// <summary>Length of the STREET1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Street1_MaxLength)]
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

  /// <summary>Length of the STREET2 attribute.</summary>
  public const int Street2_MaxLength = 25;

  /// <summary>
  /// The value of the STREET2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = Street2_MaxLength)]
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

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 15;

  /// <summary>
  /// The value of the CITY attribute.
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

  /// <summary>Length of the ZIP attribute.</summary>
  public const int Zip_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = Zip_MaxLength)]
  public string Zip
  {
    get => zip ?? "";
    set => zip = TrimEnd(Substring(value, 1, Zip_MaxLength));
  }

  /// <summary>
  /// The json value of the Zip attribute.</summary>
  [JsonPropertyName("zip")]
  [Computed]
  public string Zip_Json
  {
    get => NullIf(Zip, "");
    set => Zip = value;
  }

  /// <summary>Length of the ZIP4 attribute.</summary>
  public const int Zip4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = Zip4_MaxLength)]
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

  /// <summary>Length of the STREET3 attribute.</summary>
  public const int Street3_MaxLength = 25;

  /// <summary>
  /// The value of the STREET3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = Street3_MaxLength)]
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

  /// <summary>Length of the STREET4 attribute.</summary>
  public const int Street4_MaxLength = 25;

  /// <summary>
  /// The value of the STREET4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Street4_MaxLength)]
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

  /// <summary>Length of the COUNTRY attribute.</summary>
  public const int Country_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTRY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = Country_MaxLength)]
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

  /// <summary>Length of the PROVINCE attribute.</summary>
  public const int Province_MaxLength = 5;

  /// <summary>
  /// The value of the PROVINCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = Province_MaxLength)]
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

  /// <summary>Length of the POSTAL_CODE attribute.</summary>
  public const int PostalCode_MaxLength = 10;

  /// <summary>
  /// The value of the POSTAL_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = PostalCode_MaxLength)]
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

  private string addressType;
  private string locationType;
  private string date;
  private string street1;
  private string street2;
  private string city;
  private string state;
  private string zip;
  private string zip4;
  private string street3;
  private string street4;
  private string country;
  private string province;
  private string postalCode;
}
