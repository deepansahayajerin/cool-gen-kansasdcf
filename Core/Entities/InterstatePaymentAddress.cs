// The source file: INTERSTATE_PAYMENT_ADDRESS, ID: 371436473, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB	
/// The address where interstate payments should be sent.
/// </summary>
[Serializable]
public partial class InterstatePaymentAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstatePaymentAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstatePaymentAddress(InterstatePaymentAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstatePaymentAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstatePaymentAddress that)
  {
    base.Assign(that);
    locationType = that.locationType;
    payableToName = that.payableToName;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    type1 = that.type1;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    zip5 = that.zip5;
    addressStartDate = that.addressStartDate;
    addressEndDate = that.addressEndDate;
    fipsCounty = that.fipsCounty;
    fipsState = that.fipsState;
    fipsLocation = that.fipsLocation;
    routingNumberAba = that.routingNumberAba;
    accountNumberDfi = that.accountNumberDfi;
    accountType = that.accountType;
    street3 = that.street3;
    street4 = that.street4;
    province = that.province;
    postalCode = that.postalCode;
    country = that.country;
    state = that.state;
    zipCode = that.zipCode;
    zip4 = that.zip4;
    zip3 = that.zip3;
    county = that.county;
    intGeneratedId = that.intGeneratedId;
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

  /// <summary>Length of the PAYABLE_TO_NAME attribute.</summary>
  public const int PayableToName_MaxLength = 33;

  /// <summary>
  /// The value of the PAYABLE_TO_NAME attribute.
  /// This attribute specifies the payee name. This attribute will be maintained
  /// manually in KESSEP. It does not come through the CSENet transaction
  /// record
  /// </summary>
  [JsonPropertyName("payableToName")]
  [Member(Index = 2, Type = MemberType.Char, Length = PayableToName_MaxLength, Optional
    = true)]
  public string PayableToName
  {
    get => payableToName;
    set => payableToName = value != null
      ? TrimEnd(Substring(value, 1, PayableToName_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 4, Type = MemberType.Timestamp)]
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
  [Member(Index = 5, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This attribute indicates what kind of address is in this entity.
  /// CT - Contact Worker (PA or Interstate)
  /// PY - Payment Address
  /// EF- Electronic fund transmission
  /// </summary>
  [JsonPropertyName("type1")]
  [Member(Index = 7, Type = MemberType.Char, Length = Type1_MaxLength, Optional
    = true)]
  public string Type1
  {
    get => type1;
    set => type1 = value != null
      ? TrimEnd(Substring(value, 1, Type1_MaxLength)) : null;
  }

  /// <summary>Length of the STREET1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET1 attribute.
  /// Street 1 (primary street and number)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = Street1_MaxLength)]
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
  /// Street 2.  Could be Apt., Suite, Building, etc.
  /// </summary>
  [JsonPropertyName("street2")]
  [Member(Index = 9, Type = MemberType.Char, Length = Street2_MaxLength, Optional
    = true)]
  public string Street2
  {
    get => street2;
    set => street2 = value != null
      ? TrimEnd(Substring(value, 1, Street2_MaxLength)) : null;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 18;

  /// <summary>
  /// The value of the CITY attribute.
  /// City of work or residence
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = City_MaxLength)]
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

  /// <summary>Length of the ZIP_5 attribute.</summary>
  public const int Zip5_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_5 attribute.
  /// ZIP (five-digit)
  /// </summary>
  [JsonPropertyName("zip5")]
  [Member(Index = 11, Type = MemberType.Char, Length = Zip5_MaxLength, Optional
    = true)]
  public string Zip5
  {
    get => zip5;
    set => zip5 = value != null
      ? TrimEnd(Substring(value, 1, Zip5_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ADDRESS_START_DATE attribute.
  /// The date this address became effective.
  /// </summary>
  [JsonPropertyName("addressStartDate")]
  [Member(Index = 12, Type = MemberType.Date)]
  public DateTime? AddressStartDate
  {
    get => addressStartDate;
    set => addressStartDate = value;
  }

  /// <summary>
  /// The value of the ADDRESS_END_DATE attribute.
  /// The date this address is no longer effective.
  /// </summary>
  [JsonPropertyName("addressEndDate")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? AddressEndDate
  {
    get => addressEndDate;
    set => addressEndDate = value;
  }

  /// <summary>Length of the FIPS_COUNTY attribute.</summary>
  public const int FipsCounty_MaxLength = 3;

  /// <summary>
  /// The value of the FIPS_COUNTY attribute.
  /// FIPS county used for the payment mailing address.
  /// </summary>
  [JsonPropertyName("fipsCounty")]
  [Member(Index = 14, Type = MemberType.Char, Length = FipsCounty_MaxLength, Optional
    = true)]
  public string FipsCounty
  {
    get => fipsCounty;
    set => fipsCounty = value != null
      ? TrimEnd(Substring(value, 1, FipsCounty_MaxLength)) : null;
  }

  /// <summary>Length of the FIPS_STATE attribute.</summary>
  public const int FipsState_MaxLength = 2;

  /// <summary>
  /// The value of the FIPS_STATE attribute.
  /// FIPS state used for the payment mailing address.
  /// </summary>
  [JsonPropertyName("fipsState")]
  [Member(Index = 15, Type = MemberType.Char, Length = FipsState_MaxLength, Optional
    = true)]
  public string FipsState
  {
    get => fipsState;
    set => fipsState = value != null
      ? TrimEnd(Substring(value, 1, FipsState_MaxLength)) : null;
  }

  /// <summary>Length of the FIPS_LOCATION attribute.</summary>
  public const int FipsLocation_MaxLength = 2;

  /// <summary>
  /// The value of the FIPS_LOCATION attribute.
  /// FIPS office used for the payment mailing address.
  /// </summary>
  [JsonPropertyName("fipsLocation")]
  [Member(Index = 16, Type = MemberType.Char, Length = FipsLocation_MaxLength, Optional
    = true)]
  public string FipsLocation
  {
    get => fipsLocation;
    set => fipsLocation = value != null
      ? TrimEnd(Substring(value, 1, FipsLocation_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ROUTING_NUMBER_ABA attribute.
  /// A number used by the banking industry to facilitate the routing of 
  /// electronic fund transmission information.
  /// </summary>
  [JsonPropertyName("routingNumberAba")]
  [Member(Index = 17, Type = MemberType.Number, Length = 10, Optional = true)]
  public long? RoutingNumberAba
  {
    get => routingNumberAba;
    set => routingNumberAba = value;
  }

  /// <summary>Length of the ACCOUNT_NUMBER_DFI attribute.</summary>
  public const int AccountNumberDfi_MaxLength = 17;

  /// <summary>
  /// The value of the ACCOUNT_NUMBER_DFI attribute.
  /// The account number of the account that will be used to receive electronic 
  /// fund transmission for the given obligee.
  /// </summary>
  [JsonPropertyName("accountNumberDfi")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = AccountNumberDfi_MaxLength, Optional = true)]
  public string AccountNumberDfi
  {
    get => accountNumberDfi;
    set => accountNumberDfi = value != null
      ? TrimEnd(Substring(value, 1, AccountNumberDfi_MaxLength)) : null;
  }

  /// <summary>Length of the ACCOUNT_TYPE attribute.</summary>
  public const int AccountType_MaxLength = 1;

  /// <summary>
  /// The value of the ACCOUNT_TYPE attribute.
  /// Tells about the kind of payment method that the person chose to pay during
  /// a particular period in time. Permissible values are:
  /// 
  /// C=Checking account
  /// 
  /// S=Savings account
  /// </summary>
  [JsonPropertyName("accountType")]
  [Member(Index = 19, Type = MemberType.Char, Length = AccountType_MaxLength, Optional
    = true)]
  public string AccountType
  {
    get => accountType;
    set => accountType = value != null
      ? TrimEnd(Substring(value, 1, AccountType_MaxLength)) : null;
  }

  /// <summary>Length of the STREET 3 attribute.</summary>
  public const int Street3_MaxLength = 25;

  /// <summary>
  /// The value of the STREET 3 attribute.
  /// First line of a postal address
  /// </summary>
  [JsonPropertyName("street3")]
  [Member(Index = 20, Type = MemberType.Char, Length = Street3_MaxLength, Optional
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
  [Member(Index = 21, Type = MemberType.Char, Length = Street4_MaxLength, Optional
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
  [Member(Index = 22, Type = MemberType.Char, Length = Province_MaxLength, Optional
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
  [Member(Index = 23, Type = MemberType.Char, Length = PostalCode_MaxLength, Optional
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
  [Member(Index = 24, Type = MemberType.Char, Length = Country_MaxLength, Optional
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
  /// Region in which the address is located
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 25, Type = MemberType.Char, Length = State_MaxLength, Optional
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
  /// This is the basic five-character ZIP which is part of an address.
  /// </summary>
  [JsonPropertyName("zipCode")]
  [Member(Index = 26, Type = MemberType.Char, Length = ZipCode_MaxLength, Optional
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
  /// Basic mailing code. Part of an address
  /// </summary>
  [JsonPropertyName("zip4")]
  [Member(Index = 27, Type = MemberType.Char, Length = Zip4_MaxLength, Optional
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
  /// Basic Mailing code; part of an address; This is not an enterable field - 
  /// there is a CAB to calculate this.
  /// </summary>
  [JsonPropertyName("zip3")]
  [Member(Index = 28, Type = MemberType.Char, Length = Zip3_MaxLength, Optional
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
  /// The county in which this address is found.
  /// </summary>
  [JsonPropertyName("county")]
  [Member(Index = 29, Type = MemberType.Char, Length = County_MaxLength, Optional
    = true)]
  public string County
  {
    get => county;
    set => county = value != null
      ? TrimEnd(Substring(value, 1, County_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the INT_H_GENERATED_ID attribute.
  /// Attribute to uniquely identify an interstate referral associated within a 
  /// case.
  /// This will be a system-generated number.
  /// </summary>
  [JsonPropertyName("intGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 30, Type = MemberType.Number, Length = 8)]
  public int IntGeneratedId
  {
    get => intGeneratedId;
    set => intGeneratedId = value;
  }

  private string locationType;
  private string payableToName;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string type1;
  private string street1;
  private string street2;
  private string city;
  private string zip5;
  private DateTime? addressStartDate;
  private DateTime? addressEndDate;
  private string fipsCounty;
  private string fipsState;
  private string fipsLocation;
  private long? routingNumberAba;
  private string accountNumberDfi;
  private string accountType;
  private string street3;
  private string street4;
  private string province;
  private string postalCode;
  private string country;
  private string state;
  private string zipCode;
  private string zip4;
  private string zip3;
  private string county;
  private int intGeneratedId;
}
