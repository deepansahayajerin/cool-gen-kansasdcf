// The source file: CSE_PERSON_ADDRESS, ID: 371421880, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// A domestic or foreign residential or maining location where mail is received
/// for any CSE related person.
/// </summary>
[Serializable]
public partial class CsePersonAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsePersonAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsePersonAddress(CsePersonAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsePersonAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CsePersonAddress that)
  {
    base.Assign(that);
    locationType = that.locationType;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    zdelStartDate = that.zdelStartDate;
    sendDate = that.sendDate;
    source = that.source;
    identifier = that.identifier;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    type1 = that.type1;
    workerId = that.workerId;
    verifiedDate = that.verifiedDate;
    endDate = that.endDate;
    endCode = that.endCode;
    zdelVerifiedCode = that.zdelVerifiedCode;
    county = that.county;
    state = that.state;
    zipCode = that.zipCode;
    zip4 = that.zip4;
    zip3 = that.zip3;
    street3 = that.street3;
    street4 = that.street4;
    province = that.province;
    postalCode = that.postalCode;
    country = that.country;
    cspNumber = that.cspNumber;
    oraCreatedBy = that.oraCreatedBy;
    oraTstamp = that.oraTstamp;
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
  [Member(Index = 4, Type = MemberType.Timestamp, Optional = true)]
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
  [JsonPropertyName("createdBy")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ZDEL_START_DATE attribute.
  /// </summary>
  [JsonPropertyName("zdelStartDate")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? ZdelStartDate
  {
    get => zdelStartDate;
    set => zdelStartDate = value;
  }

  /// <summary>
  /// The value of the SEND_DATE attribute.
  /// The date the postmaster letter was or will be sent.
  /// </summary>
  [JsonPropertyName("sendDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? SendDate
  {
    get => sendDate;
    set => sendDate = value;
  }

  /// <summary>Length of the SOURCE attribute.</summary>
  public const int Source_MaxLength = 4;

  /// <summary>
  /// The value of the SOURCE attribute.
  /// The source that supplied the address.
  /// </summary>
  [JsonPropertyName("source")]
  [Member(Index = 8, Type = MemberType.Char, Length = Source_MaxLength, Optional
    = true)]
  public string Source
  {
    get => source;
    set => source = value != null
      ? TrimEnd(Substring(value, 1, Source_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// an identifier for an address.  This will be used by the system to ensure 
  /// uniqueness, but will not be displayed or used by the users.  User
  /// inquiries will need to be by relationship to the entity that owns the
  /// address, which will produce a list if more than one address is known for a
  /// given entity.
  /// </summary>
  [JsonPropertyName("identifier")]
  [Member(Index = 9, Type = MemberType.Timestamp)]
  public DateTime? Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the STREET_1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_1 attribute.
  /// First line of a postal address
  /// </summary>
  [JsonPropertyName("street1")]
  [Member(Index = 10, Type = MemberType.Char, Length = Street1_MaxLength, Optional
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
  /// Second line of a postal address
  /// </summary>
  [JsonPropertyName("street2")]
  [Member(Index = 11, Type = MemberType.Char, Length = Street2_MaxLength, Optional
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
  /// Community where the address is located
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 12, Type = MemberType.Char, Length = City_MaxLength, Optional
    = true)]
  public string City
  {
    get => city;
    set => city = value != null
      ? TrimEnd(Substring(value, 1, City_MaxLength)) : null;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This attribute denotes whether an address is a mailing address or an 
  /// address of residence.
  /// The field attributes should be matched with Income Maintenance attribute 
  /// definition.
  /// </summary>
  [JsonPropertyName("type1")]
  [Member(Index = 13, Type = MemberType.Char, Length = Type1_MaxLength, Optional
    = true)]
  public string Type1
  {
    get => type1;
    set => type1 = value != null
      ? TrimEnd(Substring(value, 1, Type1_MaxLength)) : null;
  }

  /// <summary>Length of the WORKER_ID attribute.</summary>
  public const int WorkerId_MaxLength = 8;

  /// <summary>
  /// The value of the WORKER_ID attribute.
  /// This code indicates the case worker who verified the address.  It has been
  /// requested that this code also reflect the case worker's unit.
  /// </summary>
  [JsonPropertyName("workerId")]
  [Member(Index = 14, Type = MemberType.Char, Length = WorkerId_MaxLength, Optional
    = true)]
  public string WorkerId
  {
    get => workerId;
    set => workerId = value != null
      ? TrimEnd(Substring(value, 1, WorkerId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the VERIFIED_DATE attribute.
  /// This attribute gives the date on which it was verified that the CSE Person
  /// receives mail or resides at the given address.
  /// </summary>
  [JsonPropertyName("verifiedDate")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? VerifiedDate
  {
    get => verifiedDate;
    set => verifiedDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date the address is no longer correct.                        EX: The 
  /// date a person moves to another address, the date a business ceases to
  /// exist.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>Length of the END_CODE attribute.</summary>
  public const int EndCode_MaxLength = 2;

  /// <summary>
  /// The value of the END_CODE attribute.
  /// This code indicates the reason that the address was end-dated.  For 
  /// example:  moved, never lived there, invalid address, etc.
  /// </summary>
  [JsonPropertyName("endCode")]
  [Member(Index = 17, Type = MemberType.Char, Length = EndCode_MaxLength, Optional
    = true)]
  public string EndCode
  {
    get => endCode;
    set => endCode = value != null
      ? TrimEnd(Substring(value, 1, EndCode_MaxLength)) : null;
  }

  /// <summary>Length of the ZDEL_VERIFIED_CODE attribute.</summary>
  public const int ZdelVerifiedCode_MaxLength = 2;

  /// <summary>
  /// The value of the ZDEL_VERIFIED_CODE attribute.
  /// </summary>
  [JsonPropertyName("zdelVerifiedCode")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = ZdelVerifiedCode_MaxLength, Optional = true)]
  public string ZdelVerifiedCode
  {
    get => zdelVerifiedCode;
    set => zdelVerifiedCode = value != null
      ? TrimEnd(Substring(value, 1, ZdelVerifiedCode_MaxLength)) : null;
  }

  /// <summary>Length of the COUNTY attribute.</summary>
  public const int County_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTY attribute.
  /// The county in which this address is found.
  /// </summary>
  [JsonPropertyName("county")]
  [Member(Index = 19, Type = MemberType.Char, Length = County_MaxLength, Optional
    = true)]
  public string County
  {
    get => county;
    set => county = value != null
      ? TrimEnd(Substring(value, 1, County_MaxLength)) : null;
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// Region in which the address is located
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 20, Type = MemberType.Char, Length = State_MaxLength, Optional
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
  [Member(Index = 21, Type = MemberType.Char, Length = ZipCode_MaxLength, Optional
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
  [Member(Index = 22, Type = MemberType.Char, Length = Zip4_MaxLength, Optional
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
  [Member(Index = 23, Type = MemberType.Char, Length = Zip3_MaxLength, Optional
    = true)]
  public string Zip3
  {
    get => zip3;
    set => zip3 = value != null
      ? TrimEnd(Substring(value, 1, Zip3_MaxLength)) : null;
  }

  /// <summary>Length of the STREET 3 attribute.</summary>
  public const int Street3_MaxLength = 25;

  /// <summary>
  /// The value of the STREET 3 attribute.
  /// First line of a postal address
  /// </summary>
  [JsonPropertyName("street3")]
  [Member(Index = 24, Type = MemberType.Char, Length = Street3_MaxLength, Optional
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
  [Member(Index = 25, Type = MemberType.Char, Length = Street4_MaxLength, Optional
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
  [Member(Index = 26, Type = MemberType.Char, Length = Province_MaxLength, Optional
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
  [Member(Index = 27, Type = MemberType.Char, Length = PostalCode_MaxLength, Optional
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
  [Member(Index = 28, Type = MemberType.Char, Length = Country_MaxLength, Optional
    = true)]
  public string Country
  {
    get => country;
    set => country = value != null
      ? TrimEnd(Substring(value, 1, Country_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int OraCreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID of the logged on user that is responsible for the creation of 
  /// this occurrence of the entity.
  /// </summary>
  [JsonPropertyName("oraCreatedBy")]
  [Member(Index = 30, Type = MemberType.Char, Length = OraCreatedBy_MaxLength, Optional
    = true)]
  public string OraCreatedBy
  {
    get => oraCreatedBy;
    set => oraCreatedBy = value != null
      ? TrimEnd(Substring(value, 1, OraCreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// A timestamp for the creation of the occurrence.
  /// </summary>
  [JsonPropertyName("oraTstamp")]
  [Member(Index = 31, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? OraTstamp
  {
    get => oraTstamp;
    set => oraTstamp = value;
  }

  private string locationType;
  private DateTime? lastUpdatedTimestamp;
  private string lastUpdatedBy;
  private DateTime? createdTimestamp;
  private string createdBy;
  private DateTime? zdelStartDate;
  private DateTime? sendDate;
  private string source;
  private DateTime? identifier;
  private string street1;
  private string street2;
  private string city;
  private string type1;
  private string workerId;
  private DateTime? verifiedDate;
  private DateTime? endDate;
  private string endCode;
  private string zdelVerifiedCode;
  private string county;
  private string state;
  private string zipCode;
  private string zip4;
  private string zip3;
  private string street3;
  private string street4;
  private string province;
  private string postalCode;
  private string country;
  private string cspNumber;
  private string oraCreatedBy;
  private DateTime? oraTstamp;
}
