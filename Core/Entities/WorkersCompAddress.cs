// The source file: WORKERS_COMP_ADDRESS, ID: 1902561425, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Workers compensation claim address information provided by KDOL.
/// </summary>
[Serializable]
public partial class WorkersCompAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public WorkersCompAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public WorkersCompAddress(WorkersCompAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new WorkersCompAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(WorkersCompAddress that)
  {
    base.Assign(that);
    typeCode = that.typeCode;
    streetAddress = that.streetAddress;
    city = that.city;
    state = that.state;
    zipCode = that.zipCode;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    cspNumber = that.cspNumber;
    wccIdentifier = that.wccIdentifier;
  }

  /// <summary>Length of the TYPE_CODE attribute.</summary>
  public const int TypeCode_MaxLength = 3;

  /// <summary>
  /// The value of the TYPE_CODE attribute.
  /// The type of address associated with the workers comp claim.  (CMT=claimant
  /// address, CAT=claimant attorney address, EMP=employer address,
  /// INS=insurance carrier address, IAT=insurance attorney address)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = TypeCode_MaxLength)]
  public string TypeCode
  {
    get => typeCode ?? "";
    set => typeCode = TrimEnd(Substring(value, 1, TypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the TypeCode attribute.</summary>
  [JsonPropertyName("typeCode")]
  [Computed]
  public string TypeCode_Json
  {
    get => NullIf(TypeCode, "");
    set => TypeCode = value;
  }

  /// <summary>Length of the STREET_ADDRESS attribute.</summary>
  public const int StreetAddress_MaxLength = 55;

  /// <summary>
  /// The value of the STREET_ADDRESS attribute.
  /// The street address portion of the address.
  /// </summary>
  [JsonPropertyName("streetAddress")]
  [Member(Index = 2, Type = MemberType.Varchar, Length
    = StreetAddress_MaxLength, Optional = true)]
  public string StreetAddress
  {
    get => streetAddress;
    set => streetAddress = value != null
      ? Substring(value, 1, StreetAddress_MaxLength) : null;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 50;

  /// <summary>
  /// The value of the CITY attribute.
  /// The city name of the address.
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 3, Type = MemberType.Varchar, Length = City_MaxLength, Optional
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
  /// The state of the address.
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 4, Type = MemberType.Char, Length = State_MaxLength, Optional
    = true)]
  public string State
  {
    get => state;
    set => state = value != null
      ? TrimEnd(Substring(value, 1, State_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE attribute.</summary>
  public const int ZipCode_MaxLength = 10;

  /// <summary>
  /// The value of the ZIP_CODE attribute.
  /// The zip code of the address.
  /// </summary>
  [JsonPropertyName("zipCode")]
  [Member(Index = 5, Type = MemberType.Char, Length = ZipCode_MaxLength, Optional
    = true)]
  public string ZipCode
  {
    get => zipCode;
    set => zipCode = value != null
      ? TrimEnd(Substring(value, 1, ZipCode_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 7, Type = MemberType.Timestamp)]
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
  [Member(Index = 8, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
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
  [Member(Index = 9, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
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
  [Member(Index = 10, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Number used in uniquely identifing the workers comp claim.
  /// </summary>
  [JsonPropertyName("wccIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 9)]
  public int WccIdentifier
  {
    get => wccIdentifier;
    set => wccIdentifier = value;
  }

  private string typeCode;
  private string streetAddress;
  private string city;
  private string state;
  private string zipCode;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string cspNumber;
  private int wccIdentifier;
}
