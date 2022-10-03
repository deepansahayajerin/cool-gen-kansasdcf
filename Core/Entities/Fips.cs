// The source file: FIPS, ID: 371434352, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// The Federal Information Processing System (FIPS) Code Entity contains the 
/// information which identifies geographical areas in the United States.
/// The first two characters are state.	
/// The next three are county.
/// The last two are location, which could be used to handle multiple 
/// jurisdictions in one
/// county such as multiple courts, CSE offices that receive support payments, 
/// etc.
/// </summary>
[Serializable]
public partial class Fips: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Fips()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Fips(Fips that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Fips Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Fips that)
  {
    base.Assign(that);
    stateAbbreviation = that.stateAbbreviation;
    state = that.state;
    countyAbbreviation = that.countyAbbreviation;
    county = that.county;
    location = that.location;
    stateDescription = that.stateDescription;
    countyDescription = that.countyDescription;
    locationDescription = that.locationDescription;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    offIdentifier = that.offIdentifier;
    cspNumber = that.cspNumber;
  }

  /// <summary>Length of the STATE_ABBREVIATION attribute.</summary>
  public const int StateAbbreviation_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_ABBREVIATION attribute.
  /// A 2 charactor standard abbreviation as defined by the US Postal Service.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = StateAbbreviation_MaxLength)]
  public string StateAbbreviation
  {
    get => stateAbbreviation ?? "";
    set => stateAbbreviation =
      TrimEnd(Substring(value, 1, StateAbbreviation_MaxLength));
  }

  /// <summary>
  /// The json value of the StateAbbreviation attribute.</summary>
  [JsonPropertyName("stateAbbreviation")]
  [Computed]
  public string StateAbbreviation_Json
  {
    get => NullIf(StateAbbreviation, "");
    set => StateAbbreviation = value;
  }

  /// <summary>
  /// The value of the STATE attribute.
  /// The first two characters of the FIPS code which identify the state.
  /// </summary>
  [JsonPropertyName("state")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int State
  {
    get => state;
    set => state = value;
  }

  /// <summary>Length of the COUNTY_ABBREVIATION attribute.</summary>
  public const int CountyAbbreviation_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTY_ABBREVIATION attribute.
  /// This specifies the 2-character alphabetic code for the county.  For 
  /// example:
  /// 	SN - Shawnee County
  /// 	JO - Johnson County
  /// 	SG - Sedgwick County
  /// </summary>
  [JsonPropertyName("countyAbbreviation")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = CountyAbbreviation_MaxLength, Optional = true)]
  public string CountyAbbreviation
  {
    get => countyAbbreviation;
    set => countyAbbreviation = value != null
      ? TrimEnd(Substring(value, 1, CountyAbbreviation_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the COUNTY attribute.
  /// This is 3-5 position of the FIPS code identifying the county.
  /// </summary>
  [JsonPropertyName("county")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 3)]
  public int County
  {
    get => county;
    set => county = value;
  }

  /// <summary>
  /// The value of the LOCATION attribute.
  /// The last two positions of the FIPS code which identify a location within 
  /// the county.
  /// </summary>
  [JsonPropertyName("location")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 2)]
  public int Location
  {
    get => location;
    set => location = value;
  }

  /// <summary>Length of the STATE_DESCRIPTION attribute.</summary>
  public const int StateDescription_MaxLength = 20;

  /// <summary>
  /// The value of the STATE_DESCRIPTION attribute.
  /// This identifies the two byte STATE in the FIPS code.
  /// </summary>
  [JsonPropertyName("stateDescription")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = StateDescription_MaxLength, Optional = true)]
  public string StateDescription
  {
    get => stateDescription;
    set => stateDescription = value != null
      ? TrimEnd(Substring(value, 1, StateDescription_MaxLength)) : null;
  }

  /// <summary>Length of the COUNTY_DESCRIPTION attribute.</summary>
  public const int CountyDescription_MaxLength = 20;

  /// <summary>
  /// The value of the COUNTY_DESCRIPTION attribute.
  /// This identifies the three byte COUNTY in the FIPS code.
  /// </summary>
  [JsonPropertyName("countyDescription")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = CountyDescription_MaxLength, Optional = true)]
  public string CountyDescription
  {
    get => countyDescription;
    set => countyDescription = value != null
      ? TrimEnd(Substring(value, 1, CountyDescription_MaxLength)) : null;
  }

  /// <summary>Length of the LOCATION_DESCRIPTION attribute.</summary>
  public const int LocationDescription_MaxLength = 40;

  /// <summary>
  /// The value of the LOCATION_DESCRIPTION attribute.
  /// This identifies the two byte LOCATION in the FIPS code.
  /// </summary>
  [JsonPropertyName("locationDescription")]
  [Member(Index = 8, Type = MemberType.Varchar, Length
    = LocationDescription_MaxLength, Optional = true)]
  public string LocationDescription
  {
    get => locationDescription;
    set => locationDescription = value != null
      ? Substring(value, 1, LocationDescription_MaxLength) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person or program that created the occurrance of the 
  /// entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 10, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person or program that last updated the occurrance of 
  /// the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 11, Type = MemberType.Char, Length
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
  [Member(Index = 12, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offIdentifier")]
  [Member(Index = 13, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffIdentifier
  {
    get => offIdentifier;
    set => offIdentifier = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumber")]
  [Member(Index = 14, Type = MemberType.Char, Length = CspNumber_MaxLength, Optional
    = true)]
  public string CspNumber
  {
    get => cspNumber;
    set => cspNumber = value != null
      ? TrimEnd(Substring(value, 1, CspNumber_MaxLength)) : null;
  }

  private string stateAbbreviation;
  private int state;
  private string countyAbbreviation;
  private int county;
  private int location;
  private string stateDescription;
  private string countyDescription;
  private string locationDescription;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private int? offIdentifier;
  private string cspNumber;
}
