// The source file: HEALTH_INSURANCE_COMPANY_ADDRESS, ID: 371435062, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// Address of the health insurance company.
/// (Volume of records arbitrarily entered. needs to be corrected).
/// </summary>
[Serializable]
public partial class HealthInsuranceCompanyAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public HealthInsuranceCompanyAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public HealthInsuranceCompanyAddress(HealthInsuranceCompanyAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new HealthInsuranceCompanyAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(HealthInsuranceCompanyAddress that)
  {
    base.Assign(that);
    effectiveDate = that.effectiveDate;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    state = that.state;
    province = that.province;
    postalCode = that.postalCode;
    zipCode5 = that.zipCode5;
    zipCode4 = that.zipCode4;
    zip3 = that.zip3;
    country = that.country;
    addressType = that.addressType;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    hicIdentifier = that.hicIdentifier;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date it was verified that the health insurance company receives mail 
  /// at this address.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>Length of the STREET_1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_1 attribute.
  /// The first line of a postal address of the health insurance company.
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
  /// The second line of a postal address of the health insurance company
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
  /// The community where the  address of the health insurance company is 
  /// located.
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
  /// The politically autonomous or semi-autonomous region in which the health 
  /// insurance company address is located.
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
  /// The administrative district in which the health insurance company address 
  /// is located.
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
  /// Identifying the area in which the health insurance company foreign address
  /// is located.
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
  /// The 5-digit addressing standard US postal code that idenrifies the region 
  /// in which the health insurance company address is located.
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
  /// digit zip code to further identify the region in which the health
  /// insurance company address is located.
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

  /// <summary>Length of the ZIP3 attribute.</summary>
  public const int Zip3_MaxLength = 3;

  /// <summary>
  /// The value of the ZIP3 attribute.
  /// The 3-digit US postal code used in conjunction with 5-digit and 4-digit 
  /// zip codes to further identify the region in which the health insurance
  /// company address is located.
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

  /// <summary>Length of the COUNTRY attribute.</summary>
  public const int Country_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTRY attribute.
  /// Identifies the part of the world in which the health insurance company 
  /// address is located.
  /// </summary>
  [JsonPropertyName("country")]
  [Member(Index = 11, Type = MemberType.Char, Length = Country_MaxLength, Optional
    = true)]
  public string Country
  {
    get => country;
    set => country = value != null
      ? TrimEnd(Substring(value, 1, Country_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_TYPE attribute.</summary>
  public const int AddressType_MaxLength = 1;

  /// <summary>
  /// The value of the ADDRESS_TYPE attribute.
  /// This attribute denotes whether an address is a mailing address or an 
  /// address of residence.
  /// The field attributes should be matched with Income Maintenance attribute 
  /// definition.
  /// The permitted values are maintained in CODE_VALUE entity for CODE_NAME 
  /// ADDRESS_TYPE.
  /// </summary>
  [JsonPropertyName("addressType")]
  [Member(Index = 12, Type = MemberType.Char, Length = AddressType_MaxLength, Optional
    = true)]
  public string AddressType
  {
    get => addressType;
    set => addressType = value != null
      ? TrimEnd(Substring(value, 1, AddressType_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 14, Type = MemberType.Timestamp)]
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
  [Member(Index = 15, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 16, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Artificial attribute to uniquely identify the entity occurrence.
  /// </summary>
  [JsonPropertyName("hicIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 17, Type = MemberType.Number, Length = 7)]
  public int HicIdentifier
  {
    get => hicIdentifier;
    set => hicIdentifier = value;
  }

  private DateTime? effectiveDate;
  private string street1;
  private string street2;
  private string city;
  private string state;
  private string province;
  private string postalCode;
  private string zipCode5;
  private string zipCode4;
  private string zip3;
  private string country;
  private string addressType;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int hicIdentifier;
}
