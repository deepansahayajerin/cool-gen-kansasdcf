// The source file: WARRANT_REMAIL_ADDRESS, ID: 371440590, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This entity contains an address for an Obligee.  When a warrant is returned 
/// in the mail we often obtain a new address and remail the warrant.  We store
/// the address that we mail it to in this entity instead of replacing the
/// mailing address of that person so that this address can be verified before
/// the original address is updated.
/// </summary>
[Serializable]
public partial class WarrantRemailAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public WarrantRemailAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public WarrantRemailAddress(WarrantRemailAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new WarrantRemailAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(WarrantRemailAddress that)
  {
    base.Assign(that);
    remailDate = that.remailDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    name = that.name;
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    state = that.state;
    zipCode5 = that.zipCode5;
    zipCode4 = that.zipCode4;
    zipCode3 = that.zipCode3;
    prqId = that.prqId;
  }

  /// <summary>
  /// The value of the REMAIL_DATE attribute.
  /// The date upon which the warrant was remailed.
  /// </summary>
  [JsonPropertyName("remailDate")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? RemailDate
  {
    get => remailDate;
    set => remailDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The User ID or Program ID responsible for the last update to the 
  /// occurrence.
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
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 5, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 33;

  /// <summary>
  /// The value of the NAME attribute.
  /// This field contains the name part of the address.  This name may or may 
  /// not be the same as the name of the person that the warrent was made out
  /// to.
  /// </summary>
  [JsonPropertyName("name")]
  [Member(Index = 6, Type = MemberType.Char, Length = Name_MaxLength, Optional
    = true)]
  public string Name
  {
    get => name;
    set => name = value != null
      ? TrimEnd(Substring(value, 1, Name_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number that distinguishes a occurrance of an entity 
  /// from all other occurrances.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the STREET_1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_1 attribute.
  /// The first line of the address.
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

  /// <summary>Length of the STREET_2 attribute.</summary>
  public const int Street2_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_2 attribute.
  /// The second line of the address.
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
  public const int City_MaxLength = 30;

  /// <summary>
  /// The value of the CITY attribute.
  /// The city part of the address.
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

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// The state part of the address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = State_MaxLength)]
  public string State
  {
    get => state ?? "";
    set => state = TrimEnd(Substring(value, 1, State_MaxLength));
  }

  /// <summary>
  /// The json value of the State attribute.</summary>
  [JsonPropertyName("state")]
  [Computed]
  public string State_Json
  {
    get => NullIf(State, "");
    set => State = value;
  }

  /// <summary>Length of the ZIP_CODE_5 attribute.</summary>
  public const int ZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE_5 attribute.
  /// The first five digits of the zip code.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = ZipCode5_MaxLength)]
  public string ZipCode5
  {
    get => zipCode5 ?? "";
    set => zipCode5 = TrimEnd(Substring(value, 1, ZipCode5_MaxLength));
  }

  /// <summary>
  /// The json value of the ZipCode5 attribute.</summary>
  [JsonPropertyName("zipCode5")]
  [Computed]
  public string ZipCode5_Json
  {
    get => NullIf(ZipCode5, "");
    set => ZipCode5 = value;
  }

  /// <summary>Length of the ZIP_CODE_4 attribute.</summary>
  public const int ZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP_CODE_4 attribute.
  /// *** DRAFT ***
  /// The second part of the zip code.  It consists of four digits.
  /// </summary>
  [JsonPropertyName("zipCode4")]
  [Member(Index = 13, Type = MemberType.Char, Length = ZipCode4_MaxLength, Optional
    = true)]
  public string ZipCode4
  {
    get => zipCode4;
    set => zipCode4 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode4_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE_3 attribute.</summary>
  public const int ZipCode3_MaxLength = 3;

  /// <summary>
  /// The value of the ZIP_CODE_3 attribute.
  /// *** DRAFT ***
  /// The third part of zip code.  It consists of a two digit house number and a
  /// one digit check code number.
  /// </summary>
  [JsonPropertyName("zipCode3")]
  [Member(Index = 14, Type = MemberType.Char, Length = ZipCode3_MaxLength, Optional
    = true)]
  public string ZipCode3
  {
    get => zipCode3;
    set => zipCode3 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode3_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated timestamp that distinguishes one occurrence of 
  /// the entity type from another.
  /// </summary>
  [JsonPropertyName("prqId")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 9)]
  public int PrqId
  {
    get => prqId;
    set => prqId = value;
  }

  private DateTime? remailDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private string name;
  private int systemGeneratedIdentifier;
  private string street1;
  private string street2;
  private string city;
  private string state;
  private string zipCode5;
  private string zipCode4;
  private string zipCode3;
  private int prqId;
}
