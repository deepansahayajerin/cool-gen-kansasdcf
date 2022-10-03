// The source file: ADMIN_APPEAL_APPELLANT_ADDRESS, ID: 371430172, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// This entity contains the address information for non-CSE persons that file a
/// Request for a Fair Hearing.
/// </summary>
[Serializable]
public partial class AdminAppealAppellantAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AdminAppealAppellantAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AdminAppealAppellantAddress(AdminAppealAppellantAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AdminAppealAppellantAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AdminAppealAppellantAddress that)
  {
    base.Assign(that);
    type1 = that.type1;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    stateProvince = that.stateProvince;
    country = that.country;
    postalCode = that.postalCode;
    zipCode = that.zipCode;
    zip4 = that.zip4;
    zip3 = that.zip3;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    aapIdentifier = that.aapIdentifier;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of address.
  /// Examples include mailing address, business address, etc.
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
  /// The street address of the appellant.  This person is a non-CSE person.
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
  /// The street address of the appellant.  This person is a non-CSE person.
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
  /// The city where the non-CSE appellant lives.
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
  /// The state/province where the non-CSE appellant lives.
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

  /// <summary>Length of the COUNTRY attribute.</summary>
  public const int Country_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTRY attribute.
  /// The country where the non-CSE appellant lives.
  /// </summary>
  [JsonPropertyName("country")]
  [Member(Index = 6, Type = MemberType.Char, Length = Country_MaxLength, Optional
    = true)]
  public string Country
  {
    get => country;
    set => country = value != null
      ? TrimEnd(Substring(value, 1, Country_MaxLength)) : null;
  }

  /// <summary>Length of the POSTAL_CODE attribute.</summary>
  public const int PostalCode_MaxLength = 10;

  /// <summary>
  /// The value of the POSTAL_CODE attribute.
  /// This attribute specifies the mailing code used for the Administrative 
  /// Appeal Address outside United States.
  /// </summary>
  [JsonPropertyName("postalCode")]
  [Member(Index = 7, Type = MemberType.Char, Length = PostalCode_MaxLength, Optional
    = true)]
  public string PostalCode
  {
    get => postalCode;
    set => postalCode = value != null
      ? TrimEnd(Substring(value, 1, PostalCode_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE attribute.</summary>
  public const int ZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE attribute.
  /// The basic 5 positions of the Mailing Address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ZipCode_MaxLength)]
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
  /// Additional information about the Zip Code.
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
  /// Additional information about the Zip Code
  /// and the Zip plus 4.
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

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 12, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person that last updated the occurrance of the entity.
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
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 14, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify an Administrative 
  /// Appeal.
  /// </summary>
  [JsonPropertyName("aapIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 9)]
  public int AapIdentifier
  {
    get => aapIdentifier;
    set => aapIdentifier = value;
  }

  private string type1;
  private string street1;
  private string street2;
  private string city;
  private string stateProvince;
  private string country;
  private string postalCode;
  private string zipCode;
  private string zip4;
  private string zip3;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private int aapIdentifier;
}
