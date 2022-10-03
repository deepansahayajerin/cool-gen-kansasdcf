// The source file: FCR_SVES_ADDRESS, ID: 945065523, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// This Entity Type stores the addresses and address scrub indicators as 
/// returned on the SVES response information from FCR.
/// </summary>
[Serializable]
public partial class FcrSvesAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrSvesAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrSvesAddress(FcrSvesAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrSvesAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrSvesAddress that)
  {
    base.Assign(that);
    svesAddressTypeCode = that.svesAddressTypeCode;
    addressLine1 = that.addressLine1;
    addressLine2 = that.addressLine2;
    addressLine3 = that.addressLine3;
    addressLine4 = that.addressLine4;
    city = that.city;
    state = that.state;
    zipCode5 = that.zipCode5;
    zipCode4 = that.zipCode4;
    zip3 = that.zip3;
    addressScrubIndicator1 = that.addressScrubIndicator1;
    addressScrubIndicator2 = that.addressScrubIndicator2;
    addressScrubIndicator3 = that.addressScrubIndicator3;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    fcgMemberId = that.fcgMemberId;
    fcgLSRspAgy = that.fcgLSRspAgy;
  }

  /// <summary>Length of the SVES_ADDRESS_TYPE_CODE attribute.</summary>
  public const int SvesAddressTypeCode_MaxLength = 2;

  /// <summary>
  /// The value of the SVES_ADDRESS_TYPE_CODE attribute.
  /// This field contains a value to indicate the type of the address that was 
  /// received from SVES.
  /// RA     Residential Address
  /// DO    District Office Mailing Address
  /// PY     Payee Mailing Address
  /// CL     Claimant Address
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = SvesAddressTypeCode_MaxLength)]
  public string SvesAddressTypeCode
  {
    get => svesAddressTypeCode ?? "";
    set => svesAddressTypeCode =
      TrimEnd(Substring(value, 1, SvesAddressTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the SvesAddressTypeCode attribute.</summary>
  [JsonPropertyName("svesAddressTypeCode")]
  [Computed]
  public string SvesAddressTypeCode_Json
  {
    get => NullIf(SvesAddressTypeCode, "");
    set => SvesAddressTypeCode = value;
  }

  /// <summary>Length of the ADDRESS_LINE_1 attribute.</summary>
  public const int AddressLine1_MaxLength = 40;

  /// <summary>
  /// The value of the ADDRESS_LINE_1 attribute.
  /// This field contains the first line of the current response Address.
  /// </summary>
  [JsonPropertyName("addressLine1")]
  [Member(Index = 2, Type = MemberType.Varchar, Length
    = AddressLine1_MaxLength, Optional = true)]
  public string AddressLine1
  {
    get => addressLine1;
    set => addressLine1 = value != null
      ? Substring(value, 1, AddressLine1_MaxLength) : null;
  }

  /// <summary>Length of the ADDRESS_LINE_2 attribute.</summary>
  public const int AddressLine2_MaxLength = 40;

  /// <summary>
  /// The value of the ADDRESS_LINE_2 attribute.
  /// This field contains the second line of the current response Address.
  /// </summary>
  [JsonPropertyName("addressLine2")]
  [Member(Index = 3, Type = MemberType.Varchar, Length
    = AddressLine2_MaxLength, Optional = true)]
  public string AddressLine2
  {
    get => addressLine2;
    set => addressLine2 = value != null
      ? Substring(value, 1, AddressLine2_MaxLength) : null;
  }

  /// <summary>Length of the ADDRESS_LINE_3 attribute.</summary>
  public const int AddressLine3_MaxLength = 40;

  /// <summary>
  /// The value of the ADDRESS_LINE_3 attribute.
  /// This field contains the third line of the current response Address.
  /// </summary>
  [JsonPropertyName("addressLine3")]
  [Member(Index = 4, Type = MemberType.Varchar, Length
    = AddressLine3_MaxLength, Optional = true)]
  public string AddressLine3
  {
    get => addressLine3;
    set => addressLine3 = value != null
      ? Substring(value, 1, AddressLine3_MaxLength) : null;
  }

  /// <summary>Length of the ADDRESS_LINE_4 attribute.</summary>
  public const int AddressLine4_MaxLength = 40;

  /// <summary>
  /// The value of the ADDRESS_LINE_4 attribute.
  /// This field contains the fourth line of the current response Address.
  /// </summary>
  [JsonPropertyName("addressLine4")]
  [Member(Index = 5, Type = MemberType.Varchar, Length
    = AddressLine4_MaxLength, Optional = true)]
  public string AddressLine4
  {
    get => addressLine4;
    set => addressLine4 = value != null
      ? Substring(value, 1, AddressLine4_MaxLength) : null;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 28;

  /// <summary>
  /// The value of the CITY attribute.
  /// This field contains the City of the current response Address.
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 6, Type = MemberType.Varchar, Length = City_MaxLength, Optional
    = true)]
  public string City
  {
    get => city;
    set => city = value != null ? Substring(value, 1, City_MaxLength) : null;
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// This field contains the State abbreviation of the current response 
  /// Address.
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 7, Type = MemberType.Char, Length = State_MaxLength, Optional
    = true)]
  public string State
  {
    get => state;
    set => state = value != null
      ? TrimEnd(Substring(value, 1, State_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE5 attribute.</summary>
  public const int ZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE5 attribute.
  /// The 5-digit addressing standard US postal code that identifies the region 
  /// in which the contact address is located.
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
  /// digit zip code to further identify the region in which the contact address
  /// is located.
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
  /// zip codes to further identify the region in which the contact address is
  /// located.
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

  /// <summary>Length of the ADDRESS_SCRUB_INDICATOR_1 attribute.</summary>
  public const int AddressScrubIndicator1_MaxLength = 2;

  /// <summary>
  /// The value of the ADDRESS_SCRUB_INDICATOR_1 attribute.
  /// The first Address Scrub Code represents the general status of the address.
  /// It is always present on the Response Record. This field contains an
  /// address scrub code to indicate the results of the address editing:
  /// BA  Bad address. FINALIST determined it to be an undeliverable address. 
  /// The address is left unchanged.
  /// CH  Changed address. The address was corrected and is considered by 
  /// FINALIST to be deliverable.
  /// EA  Empty address. No address is present on the record. The address was 
  /// not provided by the Locate source.
  /// GA  Good address. FINALIST has determined it to be a deliverable address.
  /// </summary>
  [JsonPropertyName("addressScrubIndicator1")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = AddressScrubIndicator1_MaxLength, Optional = true)]
  public string AddressScrubIndicator1
  {
    get => addressScrubIndicator1;
    set => addressScrubIndicator1 = value != null
      ? TrimEnd(Substring(value, 1, AddressScrubIndicator1_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_SCRUB_INDICATOR_2 attribute.</summary>
  public const int AddressScrubIndicator2_MaxLength = 2;

  /// <summary>
  /// The value of the ADDRESS_SCRUB_INDICATOR_2 attribute.
  /// This field contains a value to further define the results of the address 
  /// editing  information returned in the response. Success or failure of
  /// correction attempts is indicated by the Address Scrub Indicator 1.
  /// If the Address Scrub Indicator 1 is BA, this field contains one of the 
  /// following values:
  /// BR  Bad range. The house number is out of range for that street. This 
  /// type of address error cannot be corrected.
  /// BU   Bad unit number. In a multi-dwelling unit, the unit number has a non
  /// -standard format,  is out of range, or is missing. In PO Box addresses,
  /// the box number does not match the Zip+4 code. Standardization was
  /// attempted.
  /// BX  Missing State code or missing State code and Zip Code. Assigning a 
  /// State or Zip Code was attempted.
  /// MA  Mismatched address. The street name is not found in the city (the 
  /// address may be deliverable because some addresses do not require a street
  /// name).
  /// MX  Mismatched State and Zip Code. Correction of the Zip Code was 
  /// attempted.
  /// NC  Non-determined city name. Correction of the city name was attempted.
  /// NZ  Non-determined Zip Code. Correction of the Zip Code was attempted but
  /// failed.
  /// If the Address Scrub Indicator 1 is CH, this field contains one of these
  /// codes (as defined above):
  /// BU  Bad unit number.
  /// BX  Missing State code or missing State code and Zip Code.
  /// CA  Corrected address.
  /// CC  Corrected city name.
  /// CZ  Corrected Zip Code.
  /// MA  Mismatched address.
  /// 
  /// MX  Mismatched State &amp; Zip Code.
  /// NC 
  /// Non-determined Zip Code
  /// 
  /// If the Address Scrub Ind 1 contains 'EA' or 'GA', this field contains
  /// spaces.
  /// </summary>
  [JsonPropertyName("addressScrubIndicator2")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = AddressScrubIndicator2_MaxLength, Optional = true)]
  public string AddressScrubIndicator2
  {
    get => addressScrubIndicator2;
    set => addressScrubIndicator2 = value != null
      ? TrimEnd(Substring(value, 1, AddressScrubIndicator2_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_SCRUB_INDICATOR_3 attribute.</summary>
  public const int AddressScrubIndicator3_MaxLength = 2;

  /// <summary>
  /// The value of the ADDRESS_SCRUB_INDICATOR_3 attribute.
  /// This field contains a value to further define the results of the scrubbing
  /// for the address information returned in the response:
  /// If the Address Scrub Indicator 1 is BA, this field contains one of these
  /// codes:
  /// BR Bad range. The house number is out of range for that street. This type
  /// of address error cannot be corrected.
  /// BU Bad unit number. In a multi-dwelling unit, the unit number has a non-
  /// standard format, is out of range, or is missing. In PO Box addresses, the
  /// box number does not match the Zip+4 code. Standardization was attempted.
  /// BX Missing State code or missing State code and Zip Code. Assigning a 
  /// State or Zip  Code was attempted
  /// MA Mismatched address. The street name is not found in the city (the 
  /// address may be deliverable because some addresses do not require a street
  /// name).
  /// MX Mismatched State and Zip Code. Correction of the Zip Code was 
  /// attempted.
  /// NC Non-determined city name. Correction of the city name was attempted.
  /// NZ Non-determined Zip Code. Correction of the Zip Code was attempted but 
  /// failed.
  /// Spaces  No additional errors were detected.
  /// If the Address Scrub Indicator 1 is CH, this field contains one of these
  /// codes (as defined above):
  /// BU Bad unit number.
  /// BX Missing State code or missing State code and Zip Code.
  /// CA Corrected address.
  /// CC Corrected city name.
  /// CZ Corrected Zip Code.
  /// MA Mismatched address
  /// MX Mismatched State and Zip Code.
  /// 
  /// NC Non-determined city name.
  /// If the Address Scrub Indicator 1 contains EA or GA, this field 
  /// contains spaces.
  /// </summary>
  [JsonPropertyName("addressScrubIndicator3")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = AddressScrubIndicator3_MaxLength, Optional = true)]
  public string AddressScrubIndicator3
  {
    get => addressScrubIndicator3;
    set => addressScrubIndicator3 = value != null
      ? TrimEnd(Substring(value, 1, AddressScrubIndicator3_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 15, Type = MemberType.Timestamp)]
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
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 17, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the MEMBER_ID attribute.</summary>
  public const int FcgMemberId_MaxLength = 15;

  /// <summary>
  /// The value of the MEMBER_ID attribute.
  /// This field contains the Member ID that was provided by the submitter of 
  /// the Locate Request.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = FcgMemberId_MaxLength)]
  public string FcgMemberId
  {
    get => fcgMemberId ?? "";
    set => fcgMemberId = TrimEnd(Substring(value, 1, FcgMemberId_MaxLength));
  }

  /// <summary>
  /// The json value of the FcgMemberId attribute.</summary>
  [JsonPropertyName("fcgMemberId")]
  [Computed]
  public string FcgMemberId_Json
  {
    get => NullIf(FcgMemberId, "");
    set => FcgMemberId = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_RESPONSE_AGENCY_CO attribute.
  /// </summary>
  public const int FcgLSRspAgy_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_RESPONSE_AGENCY_CO attribute.
  /// This field contains the code that identifies the Locate Source.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = FcgLSRspAgy_MaxLength)]
  public string FcgLSRspAgy
  {
    get => fcgLSRspAgy ?? "";
    set => fcgLSRspAgy = TrimEnd(Substring(value, 1, FcgLSRspAgy_MaxLength));
  }

  /// <summary>
  /// The json value of the FcgLSRspAgy attribute.</summary>
  [JsonPropertyName("fcgLSRspAgy")]
  [Computed]
  public string FcgLSRspAgy_Json
  {
    get => NullIf(FcgLSRspAgy, "");
    set => FcgLSRspAgy = value;
  }

  private string svesAddressTypeCode;
  private string addressLine1;
  private string addressLine2;
  private string addressLine3;
  private string addressLine4;
  private string city;
  private string state;
  private string zipCode5;
  private string zipCode4;
  private string zip3;
  private string addressScrubIndicator1;
  private string addressScrubIndicator2;
  private string addressScrubIndicator3;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string fcgMemberId;
  private string fcgLSRspAgy;
}
