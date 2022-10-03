// The source file: JAIL_ADDRESSES, ID: 373339562, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  This entity will store the prison/jail addresses in the U.S.
/// </summary>
[Serializable]
public partial class JailAddresses: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public JailAddresses()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public JailAddresses(JailAddresses that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new JailAddresses Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(JailAddresses that)
  {
    base.Assign(that);
    identifier = that.identifier;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    state = that.state;
    province = that.province;
    postalCode = that.postalCode;
    zipCode5 = that.zipCode5;
    zipCode4 = that.zipCode4;
    zipCode3 = that.zipCode3;
    jailName = that.jailName;
    phone = that.phone;
    phoneAreaCode = that.phoneAreaCode;
    phoneExtension = that.phoneExtension;
    country = that.country;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// An identifier for an address. This will be used by the system to ensure 
  /// uniqueness, but will not be displayed or used by the users.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 8)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the STREET_1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_1 attribute.
  /// The first line of the postal address for the Incarceration/Jail.
  /// </summary>
  [JsonPropertyName("street1")]
  [Member(Index = 2, Type = MemberType.Char, Length = Street1_MaxLength, Optional
    = true)]
  public string Street1
  {
    get => street1;
    set => street1 = value != null
      ? TrimEnd(Substring(value, 1, Street1_MaxLength)) : null;
  }

  /// <summary>Length of the STREET_2 attribute.</summary>
  public const int Street2_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_2 attribute.
  /// The second line of the postal address for the incarceration/Jail.
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
  /// The community where the incarceration address is located.
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 4, Type = MemberType.Char, Length = City_MaxLength, Optional
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
  /// The politically autonomous or semi-autonomous region in which the 
  /// incarceration address is located.
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 5, Type = MemberType.Char, Length = State_MaxLength, Optional
    = true)]
  public string State
  {
    get => state;
    set => state = value != null
      ? TrimEnd(Substring(value, 1, State_MaxLength)) : null;
  }

  /// <summary>Length of the PROVINCE attribute.</summary>
  public const int Province_MaxLength = 5;

  /// <summary>
  /// The value of the PROVINCE attribute.
  /// The administrative district in which the incarceration address is located.
  /// </summary>
  [JsonPropertyName("province")]
  [Member(Index = 6, Type = MemberType.Char, Length = Province_MaxLength, Optional
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
  /// The code that identifies the area in which the foreign incarceration 
  /// address is located.
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

  /// <summary>Length of the ZIP_CODE5 attribute.</summary>
  public const int ZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE5 attribute.
  /// The 5-digit addressing standard US postal code that identifies the region 
  /// in which the incarceration address is located.
  /// </summary>
  [JsonPropertyName("zipCode5")]
  [Member(Index = 8, Type = MemberType.Char, Length = ZipCode5_MaxLength, Optional
    = true)]
  public string ZipCode5
  {
    get => zipCode5;
    set => zipCode5 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode5_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE4 attribute.</summary>
  public const int ZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP_CODE4 attribute.
  /// The 4-digit addressing standard US postal code used in conjunction with 5-
  /// digit zip code to further identify the region in which the incarceration
  /// address is located.
  /// </summary>
  [JsonPropertyName("zipCode4")]
  [Member(Index = 9, Type = MemberType.Char, Length = ZipCode4_MaxLength, Optional
    = true)]
  public string ZipCode4
  {
    get => zipCode4;
    set => zipCode4 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode4_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE3 attribute.</summary>
  public const int ZipCode3_MaxLength = 3;

  /// <summary>
  /// The value of the ZIP_CODE3 attribute.
  /// The 3-digit US postal code used in conjunction with 5-digit and 4-digit 
  /// zip codes to further identify the region in which the incarceration
  /// address is located.
  /// </summary>
  [JsonPropertyName("zipCode3")]
  [Member(Index = 10, Type = MemberType.Char, Length = ZipCode3_MaxLength, Optional
    = true)]
  public string ZipCode3
  {
    get => zipCode3;
    set => zipCode3 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode3_MaxLength)) : null;
  }

  /// <summary>Length of the JAIL_NAME attribute.</summary>
  public const int JailName_MaxLength = 33;

  /// <summary>
  /// The value of the JAIL_NAME attribute.
  /// The name of the Prison/Jail.
  /// </summary>
  [JsonPropertyName("jailName")]
  [Member(Index = 11, Type = MemberType.Char, Length = JailName_MaxLength, Optional
    = true)]
  public string JailName
  {
    get => jailName;
    set => jailName = value != null
      ? TrimEnd(Substring(value, 1, JailName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHONE attribute.
  /// The phone number used to contact the parole officer of the CSE Person. The
  /// phone number is specified as 7 digit phone number.
  /// </summary>
  [JsonPropertyName("phone")]
  [Member(Index = 12, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? Phone
  {
    get => phone;
    set => phone = value;
  }

  /// <summary>
  /// The value of the PHONE_AREA_CODE attribute.
  /// The 3-digit area code for the phone number used to contact the parole.
  /// </summary>
  [JsonPropertyName("phoneAreaCode")]
  [Member(Index = 13, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? PhoneAreaCode
  {
    get => phoneAreaCode;
    set => phoneAreaCode = value;
  }

  /// <summary>Length of the PHONE_EXTENSION attribute.</summary>
  public const int PhoneExtension_MaxLength = 5;

  /// <summary>
  /// The value of the PHONE_EXTENSION attribute.
  /// The 5 character extension for the phone number of the prision/jail.
  /// </summary>
  [JsonPropertyName("phoneExtension")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = PhoneExtension_MaxLength, Optional = true)]
  public string PhoneExtension
  {
    get => phoneExtension;
    set => phoneExtension = value != null
      ? TrimEnd(Substring(value, 1, PhoneExtension_MaxLength)) : null;
  }

  /// <summary>Length of the COUNTRY attribute.</summary>
  public const int Country_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTRY attribute.
  /// Identifies the part of the world in which the incarceration address is 
  /// located.
  /// </summary>
  [JsonPropertyName("country")]
  [Member(Index = 15, Type = MemberType.Char, Length = Country_MaxLength, Optional
    = true)]
  public string Country
  {
    get => country;
    set => country = value != null
      ? TrimEnd(Substring(value, 1, Country_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 17, Type = MemberType.Timestamp)]
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
  [Member(Index = 18, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  /// Timestamp of last updatte of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 19, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  private int identifier;
  private string street1;
  private string street2;
  private string city;
  private string state;
  private string province;
  private string postalCode;
  private string zipCode5;
  private string zipCode4;
  private string zipCode3;
  private string jailName;
  private int? phone;
  private int? phoneAreaCode;
  private string phoneExtension;
  private string country;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
}
