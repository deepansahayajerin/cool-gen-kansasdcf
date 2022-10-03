// The source file: PA_PARTICIPANT_ADDRESS, ID: 371438626, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// These are addresses for Participants in PA Referralsfrom other agencies 
/// except for interstate (CSENET).
/// R - Residential
/// M - Mailing
/// E - Employer
/// </summary>
[Serializable]
public partial class PaParticipantAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PaParticipantAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PaParticipantAddress(PaParticipantAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PaParticipantAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PaParticipantAddress that)
  {
    base.Assign(that);
    identifier = that.identifier;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    type1 = that.type1;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    state = that.state;
    zip = that.zip;
    zip4 = that.zip4;
    zip3 = that.zip3;
    pafTstamp = that.pafTstamp;
    preNumber = that.preNumber;
    pafType = that.pafType;
    prpIdentifier = that.prpIdentifier;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute is used to uniquely identify an occurance of a PA REFERRAL 
  /// ADDRESS.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 2, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 3, Type = MemberType.Timestamp, Optional = true)]
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
  [Member(Index = 4, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
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
  [Member(Index = 5, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This attributes indicates what type of address is stored in this entity.
  /// R - Residential
  /// M - Mailing
  /// </summary>
  [JsonPropertyName("type1")]
  [Member(Index = 6, Type = MemberType.Char, Length = Type1_MaxLength, Optional
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
  /// Primary street or box number
  /// </summary>
  [JsonPropertyName("street1")]
  [Member(Index = 7, Type = MemberType.Char, Length = Street1_MaxLength, Optional
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
  /// Secondary address data
  /// </summary>
  [JsonPropertyName("street2")]
  [Member(Index = 8, Type = MemberType.Char, Length = Street2_MaxLength, Optional
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
  /// City of residence or work
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 9, Type = MemberType.Char, Length = City_MaxLength, Optional
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
  /// State of residence or work
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 10, Type = MemberType.Char, Length = State_MaxLength, Optional
    = true)]
  public string State
  {
    get => state;
    set => state = value != null
      ? TrimEnd(Substring(value, 1, State_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP attribute.</summary>
  public const int Zip_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP attribute.
  /// Five-digit ZIP
  /// </summary>
  [JsonPropertyName("zip")]
  [Member(Index = 11, Type = MemberType.Char, Length = Zip_MaxLength, Optional
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
  /// Four-digit ZIP add-on
  /// </summary>
  [JsonPropertyName("zip4")]
  [Member(Index = 12, Type = MemberType.Char, Length = Zip4_MaxLength, Optional
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
  [Member(Index = 13, Type = MemberType.Char, Length = Zip3_MaxLength, Optional
    = true)]
  public string Zip3
  {
    get => zip3;
    set => zip3 = value != null
      ? TrimEnd(Substring(value, 1, Zip3_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("pafTstamp")]
  [Member(Index = 14, Type = MemberType.Timestamp)]
  public DateTime? PafTstamp
  {
    get => pafTstamp;
    set => pafTstamp = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int PreNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// Referral Number (Unique identifier)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = PreNumber_MaxLength)]
  public string PreNumber
  {
    get => preNumber ?? "";
    set => preNumber = TrimEnd(Substring(value, 1, PreNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the PreNumber attribute.</summary>
  [JsonPropertyName("preNumber")]
  [Computed]
  public string PreNumber_Json
  {
    get => NullIf(PreNumber, "");
    set => PreNumber = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int PafType_MaxLength = 6;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This will indicate whether this Referral is:
  /// 'New'
  /// 'Reopen'
  /// 'Change'
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = PafType_MaxLength)]
  public string PafType
  {
    get => pafType ?? "";
    set => pafType = TrimEnd(Substring(value, 1, PafType_MaxLength));
  }

  /// <summary>
  /// The json value of the PafType attribute.</summary>
  [JsonPropertyName("pafType")]
  [Computed]
  public string PafType_Json
  {
    get => NullIf(PafType, "");
    set => PafType = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute is used to uniquely identify an occurance of a PA REFERRAL 
  /// PARTICIPANT.
  /// </summary>
  [JsonPropertyName("prpIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 17, Type = MemberType.Number, Length = 3)]
  public int PrpIdentifier
  {
    get => prpIdentifier;
    set => prpIdentifier = value;
  }

  private int identifier;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string type1;
  private string street1;
  private string street2;
  private string city;
  private string state;
  private string zip;
  private string zip4;
  private string zip3;
  private DateTime? pafTstamp;
  private string preNumber;
  private string pafType;
  private int prpIdentifier;
}
