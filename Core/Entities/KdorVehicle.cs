// The source file: KDOR_VEHICLE, ID: 1625319116, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Information about a vehicle match from Kansas Department of Revenue (KDOR).
/// </summary>
[Serializable]
public partial class KdorVehicle: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public KdorVehicle()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public KdorVehicle(KdorVehicle that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new KdorVehicle Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(KdorVehicle that)
  {
    base.Assign(that);
    identifier = that.identifier;
    lastName = that.lastName;
    firstName = that.firstName;
    ssn = that.ssn;
    dateOfBirth = that.dateOfBirth;
    licenseNumber = that.licenseNumber;
    vinNumber = that.vinNumber;
    make = that.make;
    model = that.model;
    year = that.year;
    plateNumber = that.plateNumber;
    createdTstamp = that.createdTstamp;
    createdBy = that.createdBy;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    fkCktCsePersnumb = that.fkCktCsePersnumb;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Sequential number used to identify unique occurrences for the same 
  /// cse_person.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// Last name of the person who was matched to the vehicle
  /// </summary>
  [JsonPropertyName("lastName")]
  [Member(Index = 2, Type = MemberType.Char, Length = LastName_MaxLength, Optional
    = true)]
  public string LastName
  {
    get => lastName;
    set => lastName = value != null
      ? TrimEnd(Substring(value, 1, LastName_MaxLength)) : null;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// First name of the person who was matched to the vehicle.
  /// </summary>
  [JsonPropertyName("firstName")]
  [Member(Index = 3, Type = MemberType.Char, Length = FirstName_MaxLength, Optional
    = true)]
  public string FirstName
  {
    get => firstName;
    set => firstName = value != null
      ? TrimEnd(Substring(value, 1, FirstName_MaxLength)) : null;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// Social Security Number of the person who was matched to the vehicle.
  /// </summary>
  [JsonPropertyName("ssn")]
  [Member(Index = 4, Type = MemberType.Char, Length = Ssn_MaxLength, Optional
    = true)]
  public string Ssn
  {
    get => ssn;
    set => ssn = value != null ? TrimEnd(Substring(value, 1, Ssn_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the DATE_OF_BIRTH attribute.
  /// Date of birth of the person who was matched to the vehicle.
  /// </summary>
  [JsonPropertyName("dateOfBirth")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfBirth
  {
    get => dateOfBirth;
    set => dateOfBirth = value;
  }

  /// <summary>Length of the LICENSE_NUMBER attribute.</summary>
  public const int LicenseNumber_MaxLength = 9;

  /// <summary>
  /// The value of the LICENSE_NUMBER attribute.
  /// Drivers license number of the person who was matched to the vehicle.
  /// </summary>
  [JsonPropertyName("licenseNumber")]
  [Member(Index = 6, Type = MemberType.Char, Length = LicenseNumber_MaxLength, Optional
    = true)]
  public string LicenseNumber
  {
    get => licenseNumber;
    set => licenseNumber = value != null
      ? TrimEnd(Substring(value, 1, LicenseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the VIN_NUMBER attribute.</summary>
  public const int VinNumber_MaxLength = 30;

  /// <summary>
  /// The value of the VIN_NUMBER attribute.
  /// Vehicle identification number.
  /// </summary>
  [JsonPropertyName("vinNumber")]
  [Member(Index = 7, Type = MemberType.Char, Length = VinNumber_MaxLength, Optional
    = true)]
  public string VinNumber
  {
    get => vinNumber;
    set => vinNumber = value != null
      ? TrimEnd(Substring(value, 1, VinNumber_MaxLength)) : null;
  }

  /// <summary>Length of the MAKE attribute.</summary>
  public const int Make_MaxLength = 30;

  /// <summary>
  /// The value of the MAKE attribute.
  /// Vehicle manufacturer.
  /// </summary>
  [JsonPropertyName("make")]
  [Member(Index = 8, Type = MemberType.Char, Length = Make_MaxLength, Optional
    = true)]
  public string Make
  {
    get => make;
    set => make = value != null
      ? TrimEnd(Substring(value, 1, Make_MaxLength)) : null;
  }

  /// <summary>Length of the MODEL attribute.</summary>
  public const int Model_MaxLength = 30;

  /// <summary>
  /// The value of the MODEL attribute.
  /// Model name of the vehicle.
  /// </summary>
  [JsonPropertyName("model")]
  [Member(Index = 9, Type = MemberType.Char, Length = Model_MaxLength, Optional
    = true)]
  public string Model
  {
    get => model;
    set => model = value != null
      ? TrimEnd(Substring(value, 1, Model_MaxLength)) : null;
  }

  /// <summary>Length of the YEAR attribute.</summary>
  public const int Year_MaxLength = 4;

  /// <summary>
  /// The value of the YEAR attribute.
  /// Year the vehicle was manufactured.
  /// </summary>
  [JsonPropertyName("year")]
  [Member(Index = 10, Type = MemberType.Char, Length = Year_MaxLength, Optional
    = true)]
  public string Year
  {
    get => year;
    set => year = value != null
      ? TrimEnd(Substring(value, 1, Year_MaxLength)) : null;
  }

  /// <summary>Length of the PLATE_NUMBER attribute.</summary>
  public const int PlateNumber_MaxLength = 9;

  /// <summary>
  /// The value of the PLATE_NUMBER attribute.
  /// License plate number assigned to the vehicle.
  /// </summary>
  [JsonPropertyName("plateNumber")]
  [Member(Index = 11, Type = MemberType.Char, Length = PlateNumber_MaxLength, Optional
    = true)]
  public string PlateNumber
  {
    get => plateNumber;
    set => plateNumber = value != null
      ? TrimEnd(Substring(value, 1, PlateNumber_MaxLength)) : null;
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

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
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

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person that last updated the occurrance of the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 14, Type = MemberType.Char, Length
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
  [Member(Index = 15, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int FkCktCsePersnumb_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = FkCktCsePersnumb_MaxLength)]
  public string FkCktCsePersnumb
  {
    get => fkCktCsePersnumb ?? "";
    set => fkCktCsePersnumb =
      TrimEnd(Substring(value, 1, FkCktCsePersnumb_MaxLength));
  }

  /// <summary>
  /// The json value of the FkCktCsePersnumb attribute.</summary>
  [JsonPropertyName("fkCktCsePersnumb")]
  [Computed]
  public string FkCktCsePersnumb_Json
  {
    get => NullIf(FkCktCsePersnumb, "");
    set => FkCktCsePersnumb = value;
  }

  private int identifier;
  private string lastName;
  private string firstName;
  private string ssn;
  private DateTime? dateOfBirth;
  private string licenseNumber;
  private string vinNumber;
  private string make;
  private string model;
  private string year;
  private string plateNumber;
  private DateTime? createdTstamp;
  private string createdBy;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private string fkCktCsePersnumb;
}
