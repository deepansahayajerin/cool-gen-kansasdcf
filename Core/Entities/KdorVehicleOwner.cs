// The source file: KDOR_VEHICLE_OWNER, ID: 1625319139, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Ownership information about a vehicle provided by Kansas Department of 
/// Revenue (KDOR).
/// </summary>
[Serializable]
public partial class KdorVehicleOwner: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public KdorVehicleOwner()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public KdorVehicleOwner(KdorVehicleOwner that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new KdorVehicleOwner Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(KdorVehicleOwner that)
  {
    base.Assign(that);
    identifier = that.identifier;
    organizationName = that.organizationName;
    firstName = that.firstName;
    middleName = that.middleName;
    lastName = that.lastName;
    suffix = that.suffix;
    addressLine1 = that.addressLine1;
    addressLine2 = that.addressLine2;
    city = that.city;
    state = that.state;
    zipCode = that.zipCode;
    vestmentType = that.vestmentType;
    homePhone = that.homePhone;
    businessPhone = that.businessPhone;
    createdTstamp = that.createdTstamp;
    createdBy = that.createdBy;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    fkCktKdorVehfkCktCsePers = that.fkCktKdorVehfkCktCsePers;
    fkCktKdorVehidentifier = that.fkCktKdorVehidentifier;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Sequential number used to identify unique occurrences for the same 
  /// KDOR_VEHICLE.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the ORGANIZATION_NAME attribute.</summary>
  public const int OrganizationName_MaxLength = 66;

  /// <summary>
  /// The value of the ORGANIZATION_NAME attribute.
  /// Name of an organization who has ownership in the vehicle.
  /// </summary>
  [JsonPropertyName("organizationName")]
  [Member(Index = 2, Type = MemberType.Varchar, Length
    = OrganizationName_MaxLength, Optional = true)]
  public string OrganizationName
  {
    get => organizationName;
    set => organizationName = value != null
      ? Substring(value, 1, OrganizationName_MaxLength) : null;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 80;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// First name of an individual who has ownership in the vehicle
  /// </summary>
  [JsonPropertyName("firstName")]
  [Member(Index = 3, Type = MemberType.Varchar, Length = FirstName_MaxLength, Optional
    = true)]
  public string FirstName
  {
    get => firstName;
    set => firstName = value != null
      ? Substring(value, 1, FirstName_MaxLength) : null;
  }

  /// <summary>Length of the MIDDLE_NAME attribute.</summary>
  public const int MiddleName_MaxLength = 80;

  /// <summary>
  /// The value of the MIDDLE_NAME attribute.
  /// Middle name of an individual who has ownership in the vehicle.
  /// </summary>
  [JsonPropertyName("middleName")]
  [Member(Index = 4, Type = MemberType.Varchar, Length = MiddleName_MaxLength, Optional
    = true)]
  public string MiddleName
  {
    get => middleName;
    set => middleName = value != null
      ? Substring(value, 1, MiddleName_MaxLength) : null;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 80;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// Last name of an individual who has ownership in the vehicle.
  /// </summary>
  [JsonPropertyName("lastName")]
  [Member(Index = 5, Type = MemberType.Varchar, Length = LastName_MaxLength, Optional
    = true)]
  public string LastName
  {
    get => lastName;
    set => lastName = value != null
      ? Substring(value, 1, LastName_MaxLength) : null;
  }

  /// <summary>Length of the SUFFIX attribute.</summary>
  public const int Suffix_MaxLength = 8;

  /// <summary>
  /// The value of the SUFFIX attribute.
  /// Suffix of an individual who has ownership in the vehicle.
  /// </summary>
  [JsonPropertyName("suffix")]
  [Member(Index = 6, Type = MemberType.Char, Length = Suffix_MaxLength, Optional
    = true)]
  public string Suffix
  {
    get => suffix;
    set => suffix = value != null
      ? TrimEnd(Substring(value, 1, Suffix_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_LINE_1 attribute.</summary>
  public const int AddressLine1_MaxLength = 50;

  /// <summary>
  /// The value of the ADDRESS_LINE_1 attribute.
  /// First street address of the owner.
  /// </summary>
  [JsonPropertyName("addressLine1")]
  [Member(Index = 7, Type = MemberType.Varchar, Length
    = AddressLine1_MaxLength, Optional = true)]
  public string AddressLine1
  {
    get => addressLine1;
    set => addressLine1 = value != null
      ? Substring(value, 1, AddressLine1_MaxLength) : null;
  }

  /// <summary>Length of the ADDRESS_LINE_2 attribute.</summary>
  public const int AddressLine2_MaxLength = 50;

  /// <summary>
  /// The value of the ADDRESS_LINE_2 attribute.
  /// Second street address of the owner.
  /// </summary>
  [JsonPropertyName("addressLine2")]
  [Member(Index = 8, Type = MemberType.Varchar, Length
    = AddressLine2_MaxLength, Optional = true)]
  public string AddressLine2
  {
    get => addressLine2;
    set => addressLine2 = value != null
      ? Substring(value, 1, AddressLine2_MaxLength) : null;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 20;

  /// <summary>
  /// The value of the CITY attribute.
  /// City of the owner.
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 9, Type = MemberType.Varchar, Length = City_MaxLength, Optional
    = true)]
  public string City
  {
    get => city;
    set => city = value != null ? Substring(value, 1, City_MaxLength) : null;
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 4;

  /// <summary>
  /// The value of the STATE attribute.
  /// State of the owner.
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

  /// <summary>Length of the ZIP_CODE attribute.</summary>
  public const int ZipCode_MaxLength = 9;

  /// <summary>
  /// The value of the ZIP_CODE attribute.
  /// Zip code of the owner.
  /// </summary>
  [JsonPropertyName("zipCode")]
  [Member(Index = 11, Type = MemberType.Char, Length = ZipCode_MaxLength, Optional
    = true)]
  public string ZipCode
  {
    get => zipCode;
    set => zipCode = value != null
      ? TrimEnd(Substring(value, 1, ZipCode_MaxLength)) : null;
  }

  /// <summary>Length of the VESTMENT_TYPE attribute.</summary>
  public const int VestmentType_MaxLength = 30;

  /// <summary>
  /// The value of the VESTMENT_TYPE attribute.
  /// Vestment type of the vehicle ownership.  (ex. Owner, Primary Operator, 
  /// Security Interest, Lessee, Transfer on Death Designee)
  /// </summary>
  [JsonPropertyName("vestmentType")]
  [Member(Index = 12, Type = MemberType.Varchar, Length
    = VestmentType_MaxLength, Optional = true)]
  public string VestmentType
  {
    get => vestmentType;
    set => vestmentType = value != null
      ? Substring(value, 1, VestmentType_MaxLength) : null;
  }

  /// <summary>Length of the HOME_PHONE attribute.</summary>
  public const int HomePhone_MaxLength = 25;

  /// <summary>
  /// The value of the HOME_PHONE attribute.
  /// Home phone number.
  /// </summary>
  [JsonPropertyName("homePhone")]
  [Member(Index = 13, Type = MemberType.Varchar, Length = HomePhone_MaxLength, Optional
    = true)]
  public string HomePhone
  {
    get => homePhone;
    set => homePhone = value != null
      ? Substring(value, 1, HomePhone_MaxLength) : null;
  }

  /// <summary>Length of the BUSINESS_PHONE attribute.</summary>
  public const int BusinessPhone_MaxLength = 25;

  /// <summary>
  /// The value of the BUSINESS_PHONE attribute.
  /// Business phone number.
  /// </summary>
  [JsonPropertyName("businessPhone")]
  [Member(Index = 14, Type = MemberType.Varchar, Length
    = BusinessPhone_MaxLength, Optional = true)]
  public string BusinessPhone
  {
    get => businessPhone;
    set => businessPhone = value != null
      ? Substring(value, 1, BusinessPhone_MaxLength) : null;
  }

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 15, Type = MemberType.Timestamp)]
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

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person that last updated the occurrance of the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 17, Type = MemberType.Char, Length
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
  [Member(Index = 18, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int FkCktKdorVehfkCktCsePers_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = FkCktKdorVehfkCktCsePers_MaxLength)]
  public string FkCktKdorVehfkCktCsePers
  {
    get => fkCktKdorVehfkCktCsePers ?? "";
    set => fkCktKdorVehfkCktCsePers =
      TrimEnd(Substring(value, 1, FkCktKdorVehfkCktCsePers_MaxLength));
  }

  /// <summary>
  /// The json value of the FkCktKdorVehfkCktCsePers attribute.</summary>
  [JsonPropertyName("fkCktKdorVehfkCktCsePers")]
  [Computed]
  public string FkCktKdorVehfkCktCsePers_Json
  {
    get => NullIf(FkCktKdorVehfkCktCsePers, "");
    set => FkCktKdorVehfkCktCsePers = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Sequential number used to identify unique occurrences for the same 
  /// cse_person.
  /// </summary>
  [JsonPropertyName("fkCktKdorVehidentifier")]
  [DefaultValue(0)]
  [Member(Index = 20, Type = MemberType.Number, Length = 3)]
  public int FkCktKdorVehidentifier
  {
    get => fkCktKdorVehidentifier;
    set => fkCktKdorVehidentifier = value;
  }

  private int identifier;
  private string organizationName;
  private string firstName;
  private string middleName;
  private string lastName;
  private string suffix;
  private string addressLine1;
  private string addressLine2;
  private string city;
  private string state;
  private string zipCode;
  private string vestmentType;
  private string homePhone;
  private string businessPhone;
  private DateTime? createdTstamp;
  private string createdBy;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private string fkCktKdorVehfkCktCsePers;
  private int fkCktKdorVehidentifier;
}
