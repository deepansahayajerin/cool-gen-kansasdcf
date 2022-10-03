// The source file: HEARING_ADDRESS, ID: 371435252, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// This entity contians the address information of the hearings.
/// </summary>
[Serializable]
public partial class HearingAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public HearingAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public HearingAddress(HearingAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new HearingAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(HearingAddress that)
  {
    base.Assign(that);
    location = that.location;
    type1 = that.type1;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    stateProvince = that.stateProvince;
    county = that.county;
    zipCode = that.zipCode;
    zip4 = that.zip4;
    zip3 = that.zip3;
    postalCode = that.postalCode;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    hrgGeneratedId = that.hrgGeneratedId;
  }

  /// <summary>Length of the LOCATION attribute.</summary>
  public const int Location_MaxLength = 15;

  /// <summary>
  /// The value of the LOCATION attribute.
  /// The specific location within the address where a particular hearing takes 
  /// place.
  /// Example: Room 301
  /// </summary>
  [JsonPropertyName("location")]
  [Member(Index = 1, Type = MemberType.Char, Length = Location_MaxLength, Optional
    = true)]
  public string Location
  {
    get => location;
    set => location = value != null
      ? TrimEnd(Substring(value, 1, Location_MaxLength)) : null;
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
  [Member(Index = 2, Type = MemberType.Char, Length = Type1_MaxLength)]
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
  /// The street address of the hearing.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Street1_MaxLength)]
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
  /// The street address of the hearing.
  /// </summary>
  [JsonPropertyName("street2")]
  [Member(Index = 4, Type = MemberType.Char, Length = Street2_MaxLength, Optional
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
  /// The city where the particular hearing takes place.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = City_MaxLength)]
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

  /// <summary>Length of the STATE_PROVINCE attribute.</summary>
  public const int StateProvince_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_PROVINCE attribute.
  /// The two digit state where the particular hearing takes place.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = StateProvince_MaxLength)]
  public string StateProvince
  {
    get => stateProvince ?? "";
    set => stateProvince =
      TrimEnd(Substring(value, 1, StateProvince_MaxLength));
  }

  /// <summary>
  /// The json value of the StateProvince attribute.</summary>
  [JsonPropertyName("stateProvince")]
  [Computed]
  public string StateProvince_Json
  {
    get => NullIf(StateProvince, "");
    set => StateProvince = value;
  }

  /// <summary>Length of the COUNTY attribute.</summary>
  public const int County_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTY attribute.
  /// This attribute specifies the county where the administrative hearing takes
  /// place. The valid values are maintained in the CODE_VALUE entity for CODE
  /// name COUNTY.
  /// </summary>
  [JsonPropertyName("county")]
  [Member(Index = 7, Type = MemberType.Char, Length = County_MaxLength, Optional
    = true)]
  public string County
  {
    get => county;
    set => county = value != null
      ? TrimEnd(Substring(value, 1, County_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE attribute.</summary>
  public const int ZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE attribute.
  /// The basic 5 positions of the Mailing Address.
  /// </summary>
  [JsonPropertyName("zipCode")]
  [Member(Index = 8, Type = MemberType.Char, Length = ZipCode_MaxLength, Optional
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
  /// Additional information about the ZipCode.
  /// </summary>
  [JsonPropertyName("zip4")]
  [Member(Index = 9, Type = MemberType.Char, Length = Zip4_MaxLength, Optional
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
  /// Additional information about the ZipCode and
  /// the Zip + 4.
  /// </summary>
  [JsonPropertyName("zip3")]
  [Member(Index = 10, Type = MemberType.Char, Length = Zip3_MaxLength, Optional
    = true)]
  public string Zip3
  {
    get => zip3;
    set => zip3 = value != null
      ? TrimEnd(Substring(value, 1, Zip3_MaxLength)) : null;
  }

  /// <summary>Length of the POSTAL_CODE attribute.</summary>
  public const int PostalCode_MaxLength = 10;

  /// <summary>
  /// The value of the POSTAL_CODE attribute.
  /// This incorporates worldwide zipcode formats.
  /// </summary>
  [JsonPropertyName("postalCode")]
  [Member(Index = 11, Type = MemberType.Char, Length = PostalCode_MaxLength, Optional
    = true)]
  public string PostalCode
  {
    get => postalCode;
    set => postalCode = value != null
      ? TrimEnd(Substring(value, 1, PostalCode_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrence of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 13, Type = MemberType.Timestamp)]
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
  [Member(Index = 14, Type = MemberType.Char, Length
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
  [Member(Index = 15, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// This is a system generated identifier to uniquely identify a hearing.
  /// </summary>
  [JsonPropertyName("hrgGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 16, Type = MemberType.Number, Length = 9)]
  public int HrgGeneratedId
  {
    get => hrgGeneratedId;
    set => hrgGeneratedId = value;
  }

  private string location;
  private string type1;
  private string street1;
  private string street2;
  private string city;
  private string stateProvince;
  private string county;
  private string zipCode;
  private string zip4;
  private string zip3;
  private string postalCode;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private int hrgGeneratedId;
}
