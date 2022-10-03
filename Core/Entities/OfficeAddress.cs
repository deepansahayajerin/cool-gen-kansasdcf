// The source file: OFFICE_ADDRESS, ID: 371422534, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// The physical location of a CSE Office.
/// </summary>
[Serializable]
public partial class OfficeAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OfficeAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OfficeAddress(OfficeAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OfficeAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(OfficeAddress that)
  {
    base.Assign(that);
    type1 = that.type1;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    stateProvince = that.stateProvince;
    postalCode = that.postalCode;
    zip = that.zip;
    zip4 = that.zip4;
    zip3 = that.zip3;
    country = that.country;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatdTstamp = that.lastUpdatdTstamp;
    offGeneratedId = that.offGeneratedId;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Can be a :
  /// Home Physical
  /// Home Mailing
  /// Office Physical
  /// Office Mailing
  /// Used as an aid for the user to determine how this address is used.
  /// Rules are based on the type.
  /// Use the Set mnemonics table or generic code table
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Type1_MaxLength)]
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
  /// line 1 of street address.
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
  /// line 2 of street address
  /// </summary>
  [JsonPropertyName("street2")]
  [Member(Index = 3, Type = MemberType.Char, Length = Street2_MaxLength, Optional
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
  /// the name of the city
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = City_MaxLength)]
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
  /// state  or province abbreviation
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = StateProvince_MaxLength)]
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

  /// <summary>Length of the POSTAL_CODE attribute.</summary>
  public const int PostalCode_MaxLength = 10;

  /// <summary>
  /// The value of the POSTAL_CODE attribute.
  /// This incorporates worldwide zipcode formats.
  /// </summary>
  [JsonPropertyName("postalCode")]
  [Member(Index = 6, Type = MemberType.Char, Length = PostalCode_MaxLength, Optional
    = true)]
  public string PostalCode
  {
    get => postalCode;
    set => postalCode = value != null
      ? TrimEnd(Substring(value, 1, PostalCode_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP attribute.</summary>
  public const int Zip_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP attribute.
  /// This is the basic five-character ZIP which is part of a mailing address.
  /// </summary>
  [JsonPropertyName("zip")]
  [Member(Index = 7, Type = MemberType.Char, Length = Zip_MaxLength, Optional
    = true)]
  public string Zip
  {
    get => zip;
    set => zip = value != null ? TrimEnd(Substring(value, 1, Zip_MaxLength)) : null
      ;
  }

  /// <summary>Length of the ZIP4 attribute.</summary>
  public const int Zip4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP4 attribute.
  /// </summary>
  [JsonPropertyName("zip4")]
  [Member(Index = 8, Type = MemberType.Char, Length = Zip4_MaxLength, Optional
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
  /// The  three-digit ZIP add on to four-digit and five-digit ZIP
  /// </summary>
  [JsonPropertyName("zip3")]
  [Member(Index = 9, Type = MemberType.Char, Length = Zip3_MaxLength, Optional
    = true)]
  public string Zip3
  {
    get => zip3;
    set => zip3 = value != null
      ? TrimEnd(Substring(value, 1, Zip3_MaxLength)) : null;
  }

  /// <summary>Length of the COUNTRY attribute.</summary>
  public const int Country_MaxLength = 10;

  /// <summary>
  /// The value of the COUNTRY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = Country_MaxLength)]
  [ImplicitValue("USA")]
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

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 12, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATD_TSTAMP attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatdTstamp")]
  [Member(Index = 14, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatdTstamp
  {
    get => lastUpdatdTstamp;
    set => lastUpdatdTstamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 4)]
  public int OffGeneratedId
  {
    get => offGeneratedId;
    set => offGeneratedId = value;
  }

  private string type1;
  private string street1;
  private string street2;
  private string city;
  private string stateProvince;
  private string postalCode;
  private string zip;
  private string zip4;
  private string zip3;
  private string country;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatdTstamp;
  private int offGeneratedId;
}
