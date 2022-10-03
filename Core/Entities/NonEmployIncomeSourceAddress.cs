// The source file: NON_EMPLOY_INCOME_SOURCE_ADDRESS, ID: 371437728, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// The address of any income source with a type of 'O'.
/// </summary>
[Serializable]
public partial class NonEmployIncomeSourceAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NonEmployIncomeSourceAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NonEmployIncomeSourceAddress(NonEmployIncomeSourceAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NonEmployIncomeSourceAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NonEmployIncomeSourceAddress that)
  {
    base.Assign(that);
    locationType = that.locationType;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    street3 = that.street3;
    street4 = that.street4;
    province = that.province;
    postalCode = that.postalCode;
    country = that.country;
    state = that.state;
    zipCode = that.zipCode;
    zip4 = that.zip4;
    zip3 = that.zip3;
    csePerson = that.csePerson;
    isrIdentifier = that.isrIdentifier;
  }

  /// <summary>Length of the LOCATION_TYPE attribute.</summary>
  public const int LocationType_MaxLength = 1;

  /// <summary>
  /// The value of the LOCATION_TYPE attribute.
  /// The location of this address.
  /// e.g. domestic (U.S.) or foreign (non-U.S.)
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

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 3, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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

  /// <summary>Length of the STREET1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET1 attribute.
  /// The first line of an income source address
  /// </summary>
  [JsonPropertyName("street1")]
  [Member(Index = 6, Type = MemberType.Char, Length = Street1_MaxLength, Optional
    = true)]
  public string Street1
  {
    get => street1;
    set => street1 = value != null
      ? TrimEnd(Substring(value, 1, Street1_MaxLength)) : null;
  }

  /// <summary>Length of the STREET2 attribute.</summary>
  public const int Street2_MaxLength = 25;

  /// <summary>
  /// The value of the STREET2 attribute.
  /// The second line of an income source address
  /// </summary>
  [JsonPropertyName("street2")]
  [Member(Index = 7, Type = MemberType.Char, Length = Street2_MaxLength, Optional
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
  /// The city that the income source is located in
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 8, Type = MemberType.Char, Length = City_MaxLength, Optional
    = true)]
  public string City
  {
    get => city;
    set => city = value != null
      ? TrimEnd(Substring(value, 1, City_MaxLength)) : null;
  }

  /// <summary>Length of the STREET3 attribute.</summary>
  public const int Street3_MaxLength = 25;

  /// <summary>
  /// The value of the STREET3 attribute.
  /// The first line of an income source address
  /// </summary>
  [JsonPropertyName("street3")]
  [Member(Index = 9, Type = MemberType.Char, Length = Street3_MaxLength, Optional
    = true)]
  public string Street3
  {
    get => street3;
    set => street3 = value != null
      ? TrimEnd(Substring(value, 1, Street3_MaxLength)) : null;
  }

  /// <summary>Length of the STREET4 attribute.</summary>
  public const int Street4_MaxLength = 25;

  /// <summary>
  /// The value of the STREET4 attribute.
  /// The second line of an income source address
  /// </summary>
  [JsonPropertyName("street4")]
  [Member(Index = 10, Type = MemberType.Char, Length = Street4_MaxLength, Optional
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
  /// The province that the income source is located in
  /// </summary>
  [JsonPropertyName("province")]
  [Member(Index = 11, Type = MemberType.Char, Length = Province_MaxLength, Optional
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
  /// The postal code of the income source
  /// </summary>
  [JsonPropertyName("postalCode")]
  [Member(Index = 12, Type = MemberType.Char, Length = PostalCode_MaxLength, Optional
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
  /// The country an income source is located in
  /// </summary>
  [JsonPropertyName("country")]
  [Member(Index = 13, Type = MemberType.Char, Length = Country_MaxLength, Optional
    = true)]
  public string Country
  {
    get => country;
    set => country = value != null
      ? TrimEnd(Substring(value, 1, Country_MaxLength)) : null;
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// The state that the income sourec is located in
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 14, Type = MemberType.Char, Length = State_MaxLength, Optional
    = true)]
  public string State
  {
    get => state;
    set => state = value != null
      ? TrimEnd(Substring(value, 1, State_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP CODE attribute.</summary>
  public const int ZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP CODE attribute.
  /// The zip code assigned to an income source address
  /// </summary>
  [JsonPropertyName("zipCode")]
  [Member(Index = 15, Type = MemberType.Char, Length = ZipCode_MaxLength, Optional
    = true)]
  public string ZipCode
  {
    get => zipCode;
    set => zipCode = value != null
      ? TrimEnd(Substring(value, 1, ZipCode_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP4 attribute.</summary>
  public const int Zip4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP4 attribute.
  /// The zip4 of the income source address
  /// </summary>
  [JsonPropertyName("zip4")]
  [Member(Index = 16, Type = MemberType.Char, Length = Zip4_MaxLength, Optional
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
  /// the zip3 of the income source address. Generated
  /// </summary>
  [JsonPropertyName("zip3")]
  [Member(Index = 17, Type = MemberType.Char, Length = Zip3_MaxLength, Optional
    = true)]
  public string Zip3
  {
    get => zip3;
    set => zip3 = value != null
      ? TrimEnd(Substring(value, 1, Zip3_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CsePerson_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = CsePerson_MaxLength)]
  public string CsePerson
  {
    get => csePerson ?? "";
    set => csePerson = TrimEnd(Substring(value, 1, CsePerson_MaxLength));
  }

  /// <summary>
  /// The json value of the CsePerson attribute.</summary>
  [JsonPropertyName("csePerson")]
  [Computed]
  public string CsePerson_Json
  {
    get => NullIf(CsePerson, "");
    set => CsePerson = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated identifier of no business meaning.
  /// </summary>
  [JsonPropertyName("isrIdentifier")]
  [Member(Index = 19, Type = MemberType.Timestamp)]
  public DateTime? IsrIdentifier
  {
    get => isrIdentifier;
    set => isrIdentifier = value;
  }

  private string locationType;
  private DateTime? lastUpdatedTimestamp;
  private string lastUpdatedBy;
  private DateTime? createdTimestamp;
  private string createdBy;
  private string street1;
  private string street2;
  private string city;
  private string street3;
  private string street4;
  private string province;
  private string postalCode;
  private string country;
  private string state;
  private string zipCode;
  private string zip4;
  private string zip3;
  private string csePerson;
  private DateTime? isrIdentifier;
}
