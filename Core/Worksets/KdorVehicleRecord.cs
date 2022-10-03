// The source file: KDOR_VEHICLE_RECORD, ID: 1625328950, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class KdorVehicleRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public KdorVehicleRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public KdorVehicleRecord(KdorVehicleRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new KdorVehicleRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(KdorVehicleRecord that)
  {
    base.Assign(that);
    lastName = that.lastName;
    firstName = that.firstName;
    ssn = that.ssn;
    dateOfBirth = that.dateOfBirth;
    personNumber = that.personNumber;
    driversLicenseNumber = that.driversLicenseNumber;
    vin = that.vin;
    make = that.make;
    model = that.model;
    year = that.year;
    plateNumber = that.plateNumber;
    owner1OrganizationName = that.owner1OrganizationName;
    owner1FirstName = that.owner1FirstName;
    owner1MiddleName = that.owner1MiddleName;
    owner1LastName = that.owner1LastName;
    owner1Suffix = that.owner1Suffix;
    owner1MailingAddressLine1 = that.owner1MailingAddressLine1;
    owner1MailingAddresssLine2 = that.owner1MailingAddresssLine2;
    owner1MailingCity = that.owner1MailingCity;
    owner1MailingState = that.owner1MailingState;
    owner1MailingZipCode = that.owner1MailingZipCode;
    owner1VestmentType = that.owner1VestmentType;
    owner1HomeNumber = that.owner1HomeNumber;
    owner1BusinessNumber = that.owner1BusinessNumber;
    owner2OrganizationName = that.owner2OrganizationName;
    owner2FirstName = that.owner2FirstName;
    owner2MiddleName = that.owner2MiddleName;
    owner2LastName = that.owner2LastName;
    owner2Suffix = that.owner2Suffix;
    owner2MailingAddressLine1 = that.owner2MailingAddressLine1;
    owner2MailingAddresssLine2 = that.owner2MailingAddresssLine2;
    owner2MailingCity = that.owner2MailingCity;
    owner2MailingState = that.owner2MailingState;
    owner2MailingZipCode = that.owner2MailingZipCode;
    owner2VestmentType = that.owner2VestmentType;
    owner2HomeNumber = that.owner2HomeNumber;
    owner2BusinessNumber = that.owner2BusinessNumber;
    owner3OrganizationName = that.owner3OrganizationName;
    owner3FirstName = that.owner3FirstName;
    owner3MiddleName = that.owner3MiddleName;
    owner3LastName = that.owner3LastName;
    owner3Suffix = that.owner3Suffix;
    owner3MailingAddressLine1 = that.owner3MailingAddressLine1;
    owner3MailingAddresssLine2 = that.owner3MailingAddresssLine2;
    owner3MailingCity = that.owner3MailingCity;
    owner3MailingState = that.owner3MailingState;
    owner3MailingZipCode = that.owner3MailingZipCode;
    owner3VestmentType = that.owner3VestmentType;
    owner3HomeNumber = that.owner3HomeNumber;
    owner3BusinessNumber = that.owner3BusinessNumber;
    owner4OrganizationName = that.owner4OrganizationName;
    owner4FirstName = that.owner4FirstName;
    owner4MiddleName = that.owner4MiddleName;
    owner4LastName = that.owner4LastName;
    owner4Suffix = that.owner4Suffix;
    owner4MailingAddressLine1 = that.owner4MailingAddressLine1;
    owner4MailingAddresssLine2 = that.owner4MailingAddresssLine2;
    owner4MailingCity = that.owner4MailingCity;
    owner4MailingState = that.owner4MailingState;
    owner4MailingZipCode = that.owner4MailingZipCode;
    owner4VestmentType = that.owner4VestmentType;
    owner4HomeNumber = that.owner4HomeNumber;
    owner4BusinessNumber = that.owner4BusinessNumber;
    owner5OrganizationName = that.owner5OrganizationName;
    owner5FirstName = that.owner5FirstName;
    owner5MiddleName = that.owner5MiddleName;
    owner5LastName = that.owner5LastName;
    owner5Suffix = that.owner5Suffix;
    owner5MailingAddressLine1 = that.owner5MailingAddressLine1;
    owner5MailingAddresssLine2 = that.owner5MailingAddresssLine2;
    owner5MailingCity = that.owner5MailingCity;
    owner5MailingState = that.owner5MailingState;
    owner5MailingZipCode = that.owner5MailingZipCode;
    owner5VestmentType = that.owner5VestmentType;
    owner5HomeNumber = that.owner5HomeNumber;
    owner5BusinessNumber = that.owner5BusinessNumber;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = LastName_MaxLength)]
  public string LastName
  {
    get => lastName ?? "";
    set => lastName = TrimEnd(Substring(value, 1, LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the LastName attribute.</summary>
  [JsonPropertyName("lastName")]
  [Computed]
  public string LastName_Json
  {
    get => NullIf(LastName, "");
    set => LastName = value;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = FirstName_MaxLength)]
  public string FirstName
  {
    get => firstName ?? "";
    set => firstName = TrimEnd(Substring(value, 1, FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the FirstName attribute.</summary>
  [JsonPropertyName("firstName")]
  [Computed]
  public string FirstName_Json
  {
    get => NullIf(FirstName, "");
    set => FirstName = value;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Ssn_MaxLength)]
  public string Ssn
  {
    get => ssn ?? "";
    set => ssn = TrimEnd(Substring(value, 1, Ssn_MaxLength));
  }

  /// <summary>
  /// The json value of the Ssn attribute.</summary>
  [JsonPropertyName("ssn")]
  [Computed]
  public string Ssn_Json
  {
    get => NullIf(Ssn, "");
    set => Ssn = value;
  }

  /// <summary>Length of the DATE_OF_BIRTH attribute.</summary>
  public const int DateOfBirth_MaxLength = 10;

  /// <summary>
  /// The value of the DATE_OF_BIRTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = DateOfBirth_MaxLength)]
  public string DateOfBirth
  {
    get => dateOfBirth ?? "";
    set => dateOfBirth = TrimEnd(Substring(value, 1, DateOfBirth_MaxLength));
  }

  /// <summary>
  /// The json value of the DateOfBirth attribute.</summary>
  [JsonPropertyName("dateOfBirth")]
  [Computed]
  public string DateOfBirth_Json
  {
    get => NullIf(DateOfBirth, "");
    set => DateOfBirth = value;
  }

  /// <summary>Length of the PERSON_NUMBER attribute.</summary>
  public const int PersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the PERSON_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = PersonNumber_MaxLength)]
  public string PersonNumber
  {
    get => personNumber ?? "";
    set => personNumber = TrimEnd(Substring(value, 1, PersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the PersonNumber attribute.</summary>
  [JsonPropertyName("personNumber")]
  [Computed]
  public string PersonNumber_Json
  {
    get => NullIf(PersonNumber, "");
    set => PersonNumber = value;
  }

  /// <summary>Length of the DRIVERS_LICENSE_NUMBER attribute.</summary>
  public const int DriversLicenseNumber_MaxLength = 9;

  /// <summary>
  /// The value of the DRIVERS_LICENSE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = DriversLicenseNumber_MaxLength)]
  public string DriversLicenseNumber
  {
    get => driversLicenseNumber ?? "";
    set => driversLicenseNumber =
      TrimEnd(Substring(value, 1, DriversLicenseNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the DriversLicenseNumber attribute.</summary>
  [JsonPropertyName("driversLicenseNumber")]
  [Computed]
  public string DriversLicenseNumber_Json
  {
    get => NullIf(DriversLicenseNumber, "");
    set => DriversLicenseNumber = value;
  }

  /// <summary>Length of the VIN attribute.</summary>
  public const int Vin_MaxLength = 30;

  /// <summary>
  /// The value of the VIN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Vin_MaxLength)]
  public string Vin
  {
    get => vin ?? "";
    set => vin = TrimEnd(Substring(value, 1, Vin_MaxLength));
  }

  /// <summary>
  /// The json value of the Vin attribute.</summary>
  [JsonPropertyName("vin")]
  [Computed]
  public string Vin_Json
  {
    get => NullIf(Vin, "");
    set => Vin = value;
  }

  /// <summary>Length of the MAKE attribute.</summary>
  public const int Make_MaxLength = 30;

  /// <summary>
  /// The value of the MAKE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = Make_MaxLength)]
  public string Make
  {
    get => make ?? "";
    set => make = TrimEnd(Substring(value, 1, Make_MaxLength));
  }

  /// <summary>
  /// The json value of the Make attribute.</summary>
  [JsonPropertyName("make")]
  [Computed]
  public string Make_Json
  {
    get => NullIf(Make, "");
    set => Make = value;
  }

  /// <summary>Length of the MODEL attribute.</summary>
  public const int Model_MaxLength = 30;

  /// <summary>
  /// The value of the MODEL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = Model_MaxLength)]
  public string Model
  {
    get => model ?? "";
    set => model = TrimEnd(Substring(value, 1, Model_MaxLength));
  }

  /// <summary>
  /// The json value of the Model attribute.</summary>
  [JsonPropertyName("model")]
  [Computed]
  public string Model_Json
  {
    get => NullIf(Model, "");
    set => Model = value;
  }

  /// <summary>Length of the YEAR attribute.</summary>
  public const int Year_MaxLength = 4;

  /// <summary>
  /// The value of the YEAR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = Year_MaxLength)]
  public string Year
  {
    get => year ?? "";
    set => year = TrimEnd(Substring(value, 1, Year_MaxLength));
  }

  /// <summary>
  /// The json value of the Year attribute.</summary>
  [JsonPropertyName("year")]
  [Computed]
  public string Year_Json
  {
    get => NullIf(Year, "");
    set => Year = value;
  }

  /// <summary>Length of the PLATE_NUMBER attribute.</summary>
  public const int PlateNumber_MaxLength = 9;

  /// <summary>
  /// The value of the PLATE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = PlateNumber_MaxLength)]
  public string PlateNumber
  {
    get => plateNumber ?? "";
    set => plateNumber = TrimEnd(Substring(value, 1, PlateNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the PlateNumber attribute.</summary>
  [JsonPropertyName("plateNumber")]
  [Computed]
  public string PlateNumber_Json
  {
    get => NullIf(PlateNumber, "");
    set => PlateNumber = value;
  }

  /// <summary>Length of the OWNER1_ORGANIZATION_NAME attribute.</summary>
  public const int Owner1OrganizationName_MaxLength = 66;

  /// <summary>
  /// The value of the OWNER1_ORGANIZATION_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = Owner1OrganizationName_MaxLength)]
  public string Owner1OrganizationName
  {
    get => owner1OrganizationName ?? "";
    set => owner1OrganizationName =
      TrimEnd(Substring(value, 1, Owner1OrganizationName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1OrganizationName attribute.</summary>
  [JsonPropertyName("owner1OrganizationName")]
  [Computed]
  public string Owner1OrganizationName_Json
  {
    get => NullIf(Owner1OrganizationName, "");
    set => Owner1OrganizationName = value;
  }

  /// <summary>Length of the OWNER1_FIRST_NAME attribute.</summary>
  public const int Owner1FirstName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER1_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = Owner1FirstName_MaxLength)
    ]
  public string Owner1FirstName
  {
    get => owner1FirstName ?? "";
    set => owner1FirstName =
      TrimEnd(Substring(value, 1, Owner1FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1FirstName attribute.</summary>
  [JsonPropertyName("owner1FirstName")]
  [Computed]
  public string Owner1FirstName_Json
  {
    get => NullIf(Owner1FirstName, "");
    set => Owner1FirstName = value;
  }

  /// <summary>Length of the OWNER1_MIDDLE_NAME attribute.</summary>
  public const int Owner1MiddleName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER1_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = Owner1MiddleName_MaxLength)]
  public string Owner1MiddleName
  {
    get => owner1MiddleName ?? "";
    set => owner1MiddleName =
      TrimEnd(Substring(value, 1, Owner1MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1MiddleName attribute.</summary>
  [JsonPropertyName("owner1MiddleName")]
  [Computed]
  public string Owner1MiddleName_Json
  {
    get => NullIf(Owner1MiddleName, "");
    set => Owner1MiddleName = value;
  }

  /// <summary>Length of the OWNER1_LAST_NAME attribute.</summary>
  public const int Owner1LastName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER1_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = Owner1LastName_MaxLength)]
    
  public string Owner1LastName
  {
    get => owner1LastName ?? "";
    set => owner1LastName =
      TrimEnd(Substring(value, 1, Owner1LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1LastName attribute.</summary>
  [JsonPropertyName("owner1LastName")]
  [Computed]
  public string Owner1LastName_Json
  {
    get => NullIf(Owner1LastName, "");
    set => Owner1LastName = value;
  }

  /// <summary>Length of the OWNER1_SUFFIX attribute.</summary>
  public const int Owner1Suffix_MaxLength = 8;

  /// <summary>
  /// The value of the OWNER1_SUFFIX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = Owner1Suffix_MaxLength)]
  public string Owner1Suffix
  {
    get => owner1Suffix ?? "";
    set => owner1Suffix = TrimEnd(Substring(value, 1, Owner1Suffix_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1Suffix attribute.</summary>
  [JsonPropertyName("owner1Suffix")]
  [Computed]
  public string Owner1Suffix_Json
  {
    get => NullIf(Owner1Suffix, "");
    set => Owner1Suffix = value;
  }

  /// <summary>Length of the OWNER1_MAILING_ADDRESS_LINE_1 attribute.</summary>
  public const int Owner1MailingAddressLine1_MaxLength = 50;

  /// <summary>
  /// The value of the OWNER1_MAILING_ADDRESS_LINE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = Owner1MailingAddressLine1_MaxLength)]
  public string Owner1MailingAddressLine1
  {
    get => owner1MailingAddressLine1 ?? "";
    set => owner1MailingAddressLine1 =
      TrimEnd(Substring(value, 1, Owner1MailingAddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1MailingAddressLine1 attribute.</summary>
  [JsonPropertyName("owner1MailingAddressLine1")]
  [Computed]
  public string Owner1MailingAddressLine1_Json
  {
    get => NullIf(Owner1MailingAddressLine1, "");
    set => Owner1MailingAddressLine1 = value;
  }

  /// <summary>Length of the OWNER1_MAILING_ADDRESSS_LINE_2 attribute.</summary>
  public const int Owner1MailingAddresssLine2_MaxLength = 50;

  /// <summary>
  /// The value of the OWNER1_MAILING_ADDRESSS_LINE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = Owner1MailingAddresssLine2_MaxLength)]
  public string Owner1MailingAddresssLine2
  {
    get => owner1MailingAddresssLine2 ?? "";
    set => owner1MailingAddresssLine2 =
      TrimEnd(Substring(value, 1, Owner1MailingAddresssLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1MailingAddresssLine2 attribute.</summary>
  [JsonPropertyName("owner1MailingAddresssLine2")]
  [Computed]
  public string Owner1MailingAddresssLine2_Json
  {
    get => NullIf(Owner1MailingAddresssLine2, "");
    set => Owner1MailingAddresssLine2 = value;
  }

  /// <summary>Length of the OWNER1_MAILING_CITY attribute.</summary>
  public const int Owner1MailingCity_MaxLength = 20;

  /// <summary>
  /// The value of the OWNER1_MAILING_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = Owner1MailingCity_MaxLength)]
  public string Owner1MailingCity
  {
    get => owner1MailingCity ?? "";
    set => owner1MailingCity =
      TrimEnd(Substring(value, 1, Owner1MailingCity_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1MailingCity attribute.</summary>
  [JsonPropertyName("owner1MailingCity")]
  [Computed]
  public string Owner1MailingCity_Json
  {
    get => NullIf(Owner1MailingCity, "");
    set => Owner1MailingCity = value;
  }

  /// <summary>Length of the OWNER1_MAILING_STATE attribute.</summary>
  public const int Owner1MailingState_MaxLength = 4;

  /// <summary>
  /// The value of the OWNER1_MAILING_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = Owner1MailingState_MaxLength)]
  public string Owner1MailingState
  {
    get => owner1MailingState ?? "";
    set => owner1MailingState =
      TrimEnd(Substring(value, 1, Owner1MailingState_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1MailingState attribute.</summary>
  [JsonPropertyName("owner1MailingState")]
  [Computed]
  public string Owner1MailingState_Json
  {
    get => NullIf(Owner1MailingState, "");
    set => Owner1MailingState = value;
  }

  /// <summary>Length of the OWNER1_MAILING_ZIP_CODE attribute.</summary>
  public const int Owner1MailingZipCode_MaxLength = 9;

  /// <summary>
  /// The value of the OWNER1_MAILING_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = Owner1MailingZipCode_MaxLength)]
  public string Owner1MailingZipCode
  {
    get => owner1MailingZipCode ?? "";
    set => owner1MailingZipCode =
      TrimEnd(Substring(value, 1, Owner1MailingZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1MailingZipCode attribute.</summary>
  [JsonPropertyName("owner1MailingZipCode")]
  [Computed]
  public string Owner1MailingZipCode_Json
  {
    get => NullIf(Owner1MailingZipCode, "");
    set => Owner1MailingZipCode = value;
  }

  /// <summary>Length of the OWNER1_VESTMENT_TYPE attribute.</summary>
  public const int Owner1VestmentType_MaxLength = 30;

  /// <summary>
  /// The value of the OWNER1_VESTMENT_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = Owner1VestmentType_MaxLength)]
  public string Owner1VestmentType
  {
    get => owner1VestmentType ?? "";
    set => owner1VestmentType =
      TrimEnd(Substring(value, 1, Owner1VestmentType_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1VestmentType attribute.</summary>
  [JsonPropertyName("owner1VestmentType")]
  [Computed]
  public string Owner1VestmentType_Json
  {
    get => NullIf(Owner1VestmentType, "");
    set => Owner1VestmentType = value;
  }

  /// <summary>Length of the OWNER1_HOME_NUMBER attribute.</summary>
  public const int Owner1HomeNumber_MaxLength = 25;

  /// <summary>
  /// The value of the OWNER1_HOME_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = Owner1HomeNumber_MaxLength)]
  public string Owner1HomeNumber
  {
    get => owner1HomeNumber ?? "";
    set => owner1HomeNumber =
      TrimEnd(Substring(value, 1, Owner1HomeNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1HomeNumber attribute.</summary>
  [JsonPropertyName("owner1HomeNumber")]
  [Computed]
  public string Owner1HomeNumber_Json
  {
    get => NullIf(Owner1HomeNumber, "");
    set => Owner1HomeNumber = value;
  }

  /// <summary>Length of the OWNER1_BUSINESS_NUMBER attribute.</summary>
  public const int Owner1BusinessNumber_MaxLength = 25;

  /// <summary>
  /// The value of the OWNER1_BUSINESS_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = Owner1BusinessNumber_MaxLength)]
  public string Owner1BusinessNumber
  {
    get => owner1BusinessNumber ?? "";
    set => owner1BusinessNumber =
      TrimEnd(Substring(value, 1, Owner1BusinessNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner1BusinessNumber attribute.</summary>
  [JsonPropertyName("owner1BusinessNumber")]
  [Computed]
  public string Owner1BusinessNumber_Json
  {
    get => NullIf(Owner1BusinessNumber, "");
    set => Owner1BusinessNumber = value;
  }

  /// <summary>Length of the OWNER2_ORGANIZATION_NAME attribute.</summary>
  public const int Owner2OrganizationName_MaxLength = 66;

  /// <summary>
  /// The value of the OWNER2_ORGANIZATION_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = Owner2OrganizationName_MaxLength)]
  public string Owner2OrganizationName
  {
    get => owner2OrganizationName ?? "";
    set => owner2OrganizationName =
      TrimEnd(Substring(value, 1, Owner2OrganizationName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2OrganizationName attribute.</summary>
  [JsonPropertyName("owner2OrganizationName")]
  [Computed]
  public string Owner2OrganizationName_Json
  {
    get => NullIf(Owner2OrganizationName, "");
    set => Owner2OrganizationName = value;
  }

  /// <summary>Length of the OWNER2_FIRST_NAME attribute.</summary>
  public const int Owner2FirstName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER2_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length = Owner2FirstName_MaxLength)
    ]
  public string Owner2FirstName
  {
    get => owner2FirstName ?? "";
    set => owner2FirstName =
      TrimEnd(Substring(value, 1, Owner2FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2FirstName attribute.</summary>
  [JsonPropertyName("owner2FirstName")]
  [Computed]
  public string Owner2FirstName_Json
  {
    get => NullIf(Owner2FirstName, "");
    set => Owner2FirstName = value;
  }

  /// <summary>Length of the OWNER2_MIDDLE_NAME attribute.</summary>
  public const int Owner2MiddleName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER2_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = Owner2MiddleName_MaxLength)]
  public string Owner2MiddleName
  {
    get => owner2MiddleName ?? "";
    set => owner2MiddleName =
      TrimEnd(Substring(value, 1, Owner2MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2MiddleName attribute.</summary>
  [JsonPropertyName("owner2MiddleName")]
  [Computed]
  public string Owner2MiddleName_Json
  {
    get => NullIf(Owner2MiddleName, "");
    set => Owner2MiddleName = value;
  }

  /// <summary>Length of the OWNER2_LAST_NAME attribute.</summary>
  public const int Owner2LastName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER2_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length = Owner2LastName_MaxLength)]
    
  public string Owner2LastName
  {
    get => owner2LastName ?? "";
    set => owner2LastName =
      TrimEnd(Substring(value, 1, Owner2LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2LastName attribute.</summary>
  [JsonPropertyName("owner2LastName")]
  [Computed]
  public string Owner2LastName_Json
  {
    get => NullIf(Owner2LastName, "");
    set => Owner2LastName = value;
  }

  /// <summary>Length of the OWNER2_SUFFIX attribute.</summary>
  public const int Owner2Suffix_MaxLength = 8;

  /// <summary>
  /// The value of the OWNER2_SUFFIX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length = Owner2Suffix_MaxLength)]
  public string Owner2Suffix
  {
    get => owner2Suffix ?? "";
    set => owner2Suffix = TrimEnd(Substring(value, 1, Owner2Suffix_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2Suffix attribute.</summary>
  [JsonPropertyName("owner2Suffix")]
  [Computed]
  public string Owner2Suffix_Json
  {
    get => NullIf(Owner2Suffix, "");
    set => Owner2Suffix = value;
  }

  /// <summary>Length of the OWNER2_MAILING_ADDRESS_LINE_1 attribute.</summary>
  public const int Owner2MailingAddressLine1_MaxLength = 50;

  /// <summary>
  /// The value of the OWNER2_MAILING_ADDRESS_LINE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = Owner2MailingAddressLine1_MaxLength)]
  public string Owner2MailingAddressLine1
  {
    get => owner2MailingAddressLine1 ?? "";
    set => owner2MailingAddressLine1 =
      TrimEnd(Substring(value, 1, Owner2MailingAddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2MailingAddressLine1 attribute.</summary>
  [JsonPropertyName("owner2MailingAddressLine1")]
  [Computed]
  public string Owner2MailingAddressLine1_Json
  {
    get => NullIf(Owner2MailingAddressLine1, "");
    set => Owner2MailingAddressLine1 = value;
  }

  /// <summary>Length of the OWNER2_MAILING_ADDRESSS_LINE_2 attribute.</summary>
  public const int Owner2MailingAddresssLine2_MaxLength = 50;

  /// <summary>
  /// The value of the OWNER2_MAILING_ADDRESSS_LINE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = Owner2MailingAddresssLine2_MaxLength)]
  public string Owner2MailingAddresssLine2
  {
    get => owner2MailingAddresssLine2 ?? "";
    set => owner2MailingAddresssLine2 =
      TrimEnd(Substring(value, 1, Owner2MailingAddresssLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2MailingAddresssLine2 attribute.</summary>
  [JsonPropertyName("owner2MailingAddresssLine2")]
  [Computed]
  public string Owner2MailingAddresssLine2_Json
  {
    get => NullIf(Owner2MailingAddresssLine2, "");
    set => Owner2MailingAddresssLine2 = value;
  }

  /// <summary>Length of the OWNER2_MAILING_CITY attribute.</summary>
  public const int Owner2MailingCity_MaxLength = 20;

  /// <summary>
  /// The value of the OWNER2_MAILING_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = Owner2MailingCity_MaxLength)]
  public string Owner2MailingCity
  {
    get => owner2MailingCity ?? "";
    set => owner2MailingCity =
      TrimEnd(Substring(value, 1, Owner2MailingCity_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2MailingCity attribute.</summary>
  [JsonPropertyName("owner2MailingCity")]
  [Computed]
  public string Owner2MailingCity_Json
  {
    get => NullIf(Owner2MailingCity, "");
    set => Owner2MailingCity = value;
  }

  /// <summary>Length of the OWNER2_MAILING_STATE attribute.</summary>
  public const int Owner2MailingState_MaxLength = 4;

  /// <summary>
  /// The value of the OWNER2_MAILING_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = Owner2MailingState_MaxLength)]
  public string Owner2MailingState
  {
    get => owner2MailingState ?? "";
    set => owner2MailingState =
      TrimEnd(Substring(value, 1, Owner2MailingState_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2MailingState attribute.</summary>
  [JsonPropertyName("owner2MailingState")]
  [Computed]
  public string Owner2MailingState_Json
  {
    get => NullIf(Owner2MailingState, "");
    set => Owner2MailingState = value;
  }

  /// <summary>Length of the OWNER2_MAILING_ZIP_CODE attribute.</summary>
  public const int Owner2MailingZipCode_MaxLength = 9;

  /// <summary>
  /// The value of the OWNER2_MAILING_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = Owner2MailingZipCode_MaxLength)]
  public string Owner2MailingZipCode
  {
    get => owner2MailingZipCode ?? "";
    set => owner2MailingZipCode =
      TrimEnd(Substring(value, 1, Owner2MailingZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2MailingZipCode attribute.</summary>
  [JsonPropertyName("owner2MailingZipCode")]
  [Computed]
  public string Owner2MailingZipCode_Json
  {
    get => NullIf(Owner2MailingZipCode, "");
    set => Owner2MailingZipCode = value;
  }

  /// <summary>Length of the OWNER2_VESTMENT_TYPE attribute.</summary>
  public const int Owner2VestmentType_MaxLength = 30;

  /// <summary>
  /// The value of the OWNER2_VESTMENT_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = Owner2VestmentType_MaxLength)]
  public string Owner2VestmentType
  {
    get => owner2VestmentType ?? "";
    set => owner2VestmentType =
      TrimEnd(Substring(value, 1, Owner2VestmentType_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2VestmentType attribute.</summary>
  [JsonPropertyName("owner2VestmentType")]
  [Computed]
  public string Owner2VestmentType_Json
  {
    get => NullIf(Owner2VestmentType, "");
    set => Owner2VestmentType = value;
  }

  /// <summary>Length of the OWNER2_HOME_NUMBER attribute.</summary>
  public const int Owner2HomeNumber_MaxLength = 25;

  /// <summary>
  /// The value of the OWNER2_HOME_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = Owner2HomeNumber_MaxLength)]
  public string Owner2HomeNumber
  {
    get => owner2HomeNumber ?? "";
    set => owner2HomeNumber =
      TrimEnd(Substring(value, 1, Owner2HomeNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2HomeNumber attribute.</summary>
  [JsonPropertyName("owner2HomeNumber")]
  [Computed]
  public string Owner2HomeNumber_Json
  {
    get => NullIf(Owner2HomeNumber, "");
    set => Owner2HomeNumber = value;
  }

  /// <summary>Length of the OWNER2_BUSINESS_NUMBER attribute.</summary>
  public const int Owner2BusinessNumber_MaxLength = 25;

  /// <summary>
  /// The value of the OWNER2_BUSINESS_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = Owner2BusinessNumber_MaxLength)]
  public string Owner2BusinessNumber
  {
    get => owner2BusinessNumber ?? "";
    set => owner2BusinessNumber =
      TrimEnd(Substring(value, 1, Owner2BusinessNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner2BusinessNumber attribute.</summary>
  [JsonPropertyName("owner2BusinessNumber")]
  [Computed]
  public string Owner2BusinessNumber_Json
  {
    get => NullIf(Owner2BusinessNumber, "");
    set => Owner2BusinessNumber = value;
  }

  /// <summary>Length of the OWNER3_ORGANIZATION_NAME attribute.</summary>
  public const int Owner3OrganizationName_MaxLength = 66;

  /// <summary>
  /// The value of the OWNER3_ORGANIZATION_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = Owner3OrganizationName_MaxLength)]
  public string Owner3OrganizationName
  {
    get => owner3OrganizationName ?? "";
    set => owner3OrganizationName =
      TrimEnd(Substring(value, 1, Owner3OrganizationName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3OrganizationName attribute.</summary>
  [JsonPropertyName("owner3OrganizationName")]
  [Computed]
  public string Owner3OrganizationName_Json
  {
    get => NullIf(Owner3OrganizationName, "");
    set => Owner3OrganizationName = value;
  }

  /// <summary>Length of the OWNER3_FIRST_NAME attribute.</summary>
  public const int Owner3FirstName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER3_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 39, Type = MemberType.Char, Length = Owner3FirstName_MaxLength)
    ]
  public string Owner3FirstName
  {
    get => owner3FirstName ?? "";
    set => owner3FirstName =
      TrimEnd(Substring(value, 1, Owner3FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3FirstName attribute.</summary>
  [JsonPropertyName("owner3FirstName")]
  [Computed]
  public string Owner3FirstName_Json
  {
    get => NullIf(Owner3FirstName, "");
    set => Owner3FirstName = value;
  }

  /// <summary>Length of the OWNER3_MIDDLE_NAME attribute.</summary>
  public const int Owner3MiddleName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER3_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = Owner3MiddleName_MaxLength)]
  public string Owner3MiddleName
  {
    get => owner3MiddleName ?? "";
    set => owner3MiddleName =
      TrimEnd(Substring(value, 1, Owner3MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3MiddleName attribute.</summary>
  [JsonPropertyName("owner3MiddleName")]
  [Computed]
  public string Owner3MiddleName_Json
  {
    get => NullIf(Owner3MiddleName, "");
    set => Owner3MiddleName = value;
  }

  /// <summary>Length of the OWNER3_LAST_NAME attribute.</summary>
  public const int Owner3LastName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER3_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length = Owner3LastName_MaxLength)]
    
  public string Owner3LastName
  {
    get => owner3LastName ?? "";
    set => owner3LastName =
      TrimEnd(Substring(value, 1, Owner3LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3LastName attribute.</summary>
  [JsonPropertyName("owner3LastName")]
  [Computed]
  public string Owner3LastName_Json
  {
    get => NullIf(Owner3LastName, "");
    set => Owner3LastName = value;
  }

  /// <summary>Length of the OWNER3_SUFFIX attribute.</summary>
  public const int Owner3Suffix_MaxLength = 8;

  /// <summary>
  /// The value of the OWNER3_SUFFIX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length = Owner3Suffix_MaxLength)]
  public string Owner3Suffix
  {
    get => owner3Suffix ?? "";
    set => owner3Suffix = TrimEnd(Substring(value, 1, Owner3Suffix_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3Suffix attribute.</summary>
  [JsonPropertyName("owner3Suffix")]
  [Computed]
  public string Owner3Suffix_Json
  {
    get => NullIf(Owner3Suffix, "");
    set => Owner3Suffix = value;
  }

  /// <summary>Length of the OWNER3_MAILING_ADDRESS_LINE_1 attribute.</summary>
  public const int Owner3MailingAddressLine1_MaxLength = 50;

  /// <summary>
  /// The value of the OWNER3_MAILING_ADDRESS_LINE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = Owner3MailingAddressLine1_MaxLength)]
  public string Owner3MailingAddressLine1
  {
    get => owner3MailingAddressLine1 ?? "";
    set => owner3MailingAddressLine1 =
      TrimEnd(Substring(value, 1, Owner3MailingAddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3MailingAddressLine1 attribute.</summary>
  [JsonPropertyName("owner3MailingAddressLine1")]
  [Computed]
  public string Owner3MailingAddressLine1_Json
  {
    get => NullIf(Owner3MailingAddressLine1, "");
    set => Owner3MailingAddressLine1 = value;
  }

  /// <summary>Length of the OWNER3_MAILING_ADDRESSS_LINE_2 attribute.</summary>
  public const int Owner3MailingAddresssLine2_MaxLength = 50;

  /// <summary>
  /// The value of the OWNER3_MAILING_ADDRESSS_LINE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = Owner3MailingAddresssLine2_MaxLength)]
  public string Owner3MailingAddresssLine2
  {
    get => owner3MailingAddresssLine2 ?? "";
    set => owner3MailingAddresssLine2 =
      TrimEnd(Substring(value, 1, Owner3MailingAddresssLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3MailingAddresssLine2 attribute.</summary>
  [JsonPropertyName("owner3MailingAddresssLine2")]
  [Computed]
  public string Owner3MailingAddresssLine2_Json
  {
    get => NullIf(Owner3MailingAddresssLine2, "");
    set => Owner3MailingAddresssLine2 = value;
  }

  /// <summary>Length of the OWNER3_MAILING_CITY attribute.</summary>
  public const int Owner3MailingCity_MaxLength = 20;

  /// <summary>
  /// The value of the OWNER3_MAILING_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = Owner3MailingCity_MaxLength)]
  public string Owner3MailingCity
  {
    get => owner3MailingCity ?? "";
    set => owner3MailingCity =
      TrimEnd(Substring(value, 1, Owner3MailingCity_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3MailingCity attribute.</summary>
  [JsonPropertyName("owner3MailingCity")]
  [Computed]
  public string Owner3MailingCity_Json
  {
    get => NullIf(Owner3MailingCity, "");
    set => Owner3MailingCity = value;
  }

  /// <summary>Length of the OWNER3_MAILING_STATE attribute.</summary>
  public const int Owner3MailingState_MaxLength = 4;

  /// <summary>
  /// The value of the OWNER3_MAILING_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = Owner3MailingState_MaxLength)]
  public string Owner3MailingState
  {
    get => owner3MailingState ?? "";
    set => owner3MailingState =
      TrimEnd(Substring(value, 1, Owner3MailingState_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3MailingState attribute.</summary>
  [JsonPropertyName("owner3MailingState")]
  [Computed]
  public string Owner3MailingState_Json
  {
    get => NullIf(Owner3MailingState, "");
    set => Owner3MailingState = value;
  }

  /// <summary>Length of the OWNER3_MAILING_ZIP_CODE attribute.</summary>
  public const int Owner3MailingZipCode_MaxLength = 9;

  /// <summary>
  /// The value of the OWNER3_MAILING_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length
    = Owner3MailingZipCode_MaxLength)]
  public string Owner3MailingZipCode
  {
    get => owner3MailingZipCode ?? "";
    set => owner3MailingZipCode =
      TrimEnd(Substring(value, 1, Owner3MailingZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3MailingZipCode attribute.</summary>
  [JsonPropertyName("owner3MailingZipCode")]
  [Computed]
  public string Owner3MailingZipCode_Json
  {
    get => NullIf(Owner3MailingZipCode, "");
    set => Owner3MailingZipCode = value;
  }

  /// <summary>Length of the OWNER3_VESTMENT_TYPE attribute.</summary>
  public const int Owner3VestmentType_MaxLength = 30;

  /// <summary>
  /// The value of the OWNER3_VESTMENT_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 48, Type = MemberType.Char, Length
    = Owner3VestmentType_MaxLength)]
  public string Owner3VestmentType
  {
    get => owner3VestmentType ?? "";
    set => owner3VestmentType =
      TrimEnd(Substring(value, 1, Owner3VestmentType_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3VestmentType attribute.</summary>
  [JsonPropertyName("owner3VestmentType")]
  [Computed]
  public string Owner3VestmentType_Json
  {
    get => NullIf(Owner3VestmentType, "");
    set => Owner3VestmentType = value;
  }

  /// <summary>Length of the OWNER3_HOME_NUMBER attribute.</summary>
  public const int Owner3HomeNumber_MaxLength = 25;

  /// <summary>
  /// The value of the OWNER3_HOME_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 49, Type = MemberType.Char, Length
    = Owner3HomeNumber_MaxLength)]
  public string Owner3HomeNumber
  {
    get => owner3HomeNumber ?? "";
    set => owner3HomeNumber =
      TrimEnd(Substring(value, 1, Owner3HomeNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3HomeNumber attribute.</summary>
  [JsonPropertyName("owner3HomeNumber")]
  [Computed]
  public string Owner3HomeNumber_Json
  {
    get => NullIf(Owner3HomeNumber, "");
    set => Owner3HomeNumber = value;
  }

  /// <summary>Length of the OWNER3_BUSINESS_NUMBER attribute.</summary>
  public const int Owner3BusinessNumber_MaxLength = 25;

  /// <summary>
  /// The value of the OWNER3_BUSINESS_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = Owner3BusinessNumber_MaxLength)]
  public string Owner3BusinessNumber
  {
    get => owner3BusinessNumber ?? "";
    set => owner3BusinessNumber =
      TrimEnd(Substring(value, 1, Owner3BusinessNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner3BusinessNumber attribute.</summary>
  [JsonPropertyName("owner3BusinessNumber")]
  [Computed]
  public string Owner3BusinessNumber_Json
  {
    get => NullIf(Owner3BusinessNumber, "");
    set => Owner3BusinessNumber = value;
  }

  /// <summary>Length of the OWNER4_ORGANIZATION_NAME attribute.</summary>
  public const int Owner4OrganizationName_MaxLength = 66;

  /// <summary>
  /// The value of the OWNER4_ORGANIZATION_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = Owner4OrganizationName_MaxLength)]
  public string Owner4OrganizationName
  {
    get => owner4OrganizationName ?? "";
    set => owner4OrganizationName =
      TrimEnd(Substring(value, 1, Owner4OrganizationName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4OrganizationName attribute.</summary>
  [JsonPropertyName("owner4OrganizationName")]
  [Computed]
  public string Owner4OrganizationName_Json
  {
    get => NullIf(Owner4OrganizationName, "");
    set => Owner4OrganizationName = value;
  }

  /// <summary>Length of the OWNER4_FIRST_NAME attribute.</summary>
  public const int Owner4FirstName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER4_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 52, Type = MemberType.Char, Length = Owner4FirstName_MaxLength)
    ]
  public string Owner4FirstName
  {
    get => owner4FirstName ?? "";
    set => owner4FirstName =
      TrimEnd(Substring(value, 1, Owner4FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4FirstName attribute.</summary>
  [JsonPropertyName("owner4FirstName")]
  [Computed]
  public string Owner4FirstName_Json
  {
    get => NullIf(Owner4FirstName, "");
    set => Owner4FirstName = value;
  }

  /// <summary>Length of the OWNER4_MIDDLE_NAME attribute.</summary>
  public const int Owner4MiddleName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER4_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 53, Type = MemberType.Char, Length
    = Owner4MiddleName_MaxLength)]
  public string Owner4MiddleName
  {
    get => owner4MiddleName ?? "";
    set => owner4MiddleName =
      TrimEnd(Substring(value, 1, Owner4MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4MiddleName attribute.</summary>
  [JsonPropertyName("owner4MiddleName")]
  [Computed]
  public string Owner4MiddleName_Json
  {
    get => NullIf(Owner4MiddleName, "");
    set => Owner4MiddleName = value;
  }

  /// <summary>Length of the OWNER4_LAST_NAME attribute.</summary>
  public const int Owner4LastName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER4_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 54, Type = MemberType.Char, Length = Owner4LastName_MaxLength)]
    
  public string Owner4LastName
  {
    get => owner4LastName ?? "";
    set => owner4LastName =
      TrimEnd(Substring(value, 1, Owner4LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4LastName attribute.</summary>
  [JsonPropertyName("owner4LastName")]
  [Computed]
  public string Owner4LastName_Json
  {
    get => NullIf(Owner4LastName, "");
    set => Owner4LastName = value;
  }

  /// <summary>Length of the OWNER4_SUFFIX attribute.</summary>
  public const int Owner4Suffix_MaxLength = 8;

  /// <summary>
  /// The value of the OWNER4_SUFFIX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 55, Type = MemberType.Char, Length = Owner4Suffix_MaxLength)]
  public string Owner4Suffix
  {
    get => owner4Suffix ?? "";
    set => owner4Suffix = TrimEnd(Substring(value, 1, Owner4Suffix_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4Suffix attribute.</summary>
  [JsonPropertyName("owner4Suffix")]
  [Computed]
  public string Owner4Suffix_Json
  {
    get => NullIf(Owner4Suffix, "");
    set => Owner4Suffix = value;
  }

  /// <summary>Length of the OWNER4_MAILING_ADDRESS_LINE_1 attribute.</summary>
  public const int Owner4MailingAddressLine1_MaxLength = 50;

  /// <summary>
  /// The value of the OWNER4_MAILING_ADDRESS_LINE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 56, Type = MemberType.Char, Length
    = Owner4MailingAddressLine1_MaxLength)]
  public string Owner4MailingAddressLine1
  {
    get => owner4MailingAddressLine1 ?? "";
    set => owner4MailingAddressLine1 =
      TrimEnd(Substring(value, 1, Owner4MailingAddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4MailingAddressLine1 attribute.</summary>
  [JsonPropertyName("owner4MailingAddressLine1")]
  [Computed]
  public string Owner4MailingAddressLine1_Json
  {
    get => NullIf(Owner4MailingAddressLine1, "");
    set => Owner4MailingAddressLine1 = value;
  }

  /// <summary>Length of the OWNER4_MAILING_ADDRESSS_LINE_2 attribute.</summary>
  public const int Owner4MailingAddresssLine2_MaxLength = 50;

  /// <summary>
  /// The value of the OWNER4_MAILING_ADDRESSS_LINE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 57, Type = MemberType.Char, Length
    = Owner4MailingAddresssLine2_MaxLength)]
  public string Owner4MailingAddresssLine2
  {
    get => owner4MailingAddresssLine2 ?? "";
    set => owner4MailingAddresssLine2 =
      TrimEnd(Substring(value, 1, Owner4MailingAddresssLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4MailingAddresssLine2 attribute.</summary>
  [JsonPropertyName("owner4MailingAddresssLine2")]
  [Computed]
  public string Owner4MailingAddresssLine2_Json
  {
    get => NullIf(Owner4MailingAddresssLine2, "");
    set => Owner4MailingAddresssLine2 = value;
  }

  /// <summary>Length of the OWNER4_MAILING_CITY attribute.</summary>
  public const int Owner4MailingCity_MaxLength = 20;

  /// <summary>
  /// The value of the OWNER4_MAILING_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 58, Type = MemberType.Char, Length
    = Owner4MailingCity_MaxLength)]
  public string Owner4MailingCity
  {
    get => owner4MailingCity ?? "";
    set => owner4MailingCity =
      TrimEnd(Substring(value, 1, Owner4MailingCity_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4MailingCity attribute.</summary>
  [JsonPropertyName("owner4MailingCity")]
  [Computed]
  public string Owner4MailingCity_Json
  {
    get => NullIf(Owner4MailingCity, "");
    set => Owner4MailingCity = value;
  }

  /// <summary>Length of the OWNER4_MAILING_STATE attribute.</summary>
  public const int Owner4MailingState_MaxLength = 4;

  /// <summary>
  /// The value of the OWNER4_MAILING_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 59, Type = MemberType.Char, Length
    = Owner4MailingState_MaxLength)]
  public string Owner4MailingState
  {
    get => owner4MailingState ?? "";
    set => owner4MailingState =
      TrimEnd(Substring(value, 1, Owner4MailingState_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4MailingState attribute.</summary>
  [JsonPropertyName("owner4MailingState")]
  [Computed]
  public string Owner4MailingState_Json
  {
    get => NullIf(Owner4MailingState, "");
    set => Owner4MailingState = value;
  }

  /// <summary>Length of the OWNER4_MAILING_ZIP_CODE attribute.</summary>
  public const int Owner4MailingZipCode_MaxLength = 9;

  /// <summary>
  /// The value of the OWNER4_MAILING_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 60, Type = MemberType.Char, Length
    = Owner4MailingZipCode_MaxLength)]
  public string Owner4MailingZipCode
  {
    get => owner4MailingZipCode ?? "";
    set => owner4MailingZipCode =
      TrimEnd(Substring(value, 1, Owner4MailingZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4MailingZipCode attribute.</summary>
  [JsonPropertyName("owner4MailingZipCode")]
  [Computed]
  public string Owner4MailingZipCode_Json
  {
    get => NullIf(Owner4MailingZipCode, "");
    set => Owner4MailingZipCode = value;
  }

  /// <summary>Length of the OWNER4_VESTMENT_TYPE attribute.</summary>
  public const int Owner4VestmentType_MaxLength = 30;

  /// <summary>
  /// The value of the OWNER4_VESTMENT_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 61, Type = MemberType.Char, Length
    = Owner4VestmentType_MaxLength)]
  public string Owner4VestmentType
  {
    get => owner4VestmentType ?? "";
    set => owner4VestmentType =
      TrimEnd(Substring(value, 1, Owner4VestmentType_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4VestmentType attribute.</summary>
  [JsonPropertyName("owner4VestmentType")]
  [Computed]
  public string Owner4VestmentType_Json
  {
    get => NullIf(Owner4VestmentType, "");
    set => Owner4VestmentType = value;
  }

  /// <summary>Length of the OWNER4_HOME_NUMBER attribute.</summary>
  public const int Owner4HomeNumber_MaxLength = 25;

  /// <summary>
  /// The value of the OWNER4_HOME_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 62, Type = MemberType.Char, Length
    = Owner4HomeNumber_MaxLength)]
  public string Owner4HomeNumber
  {
    get => owner4HomeNumber ?? "";
    set => owner4HomeNumber =
      TrimEnd(Substring(value, 1, Owner4HomeNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4HomeNumber attribute.</summary>
  [JsonPropertyName("owner4HomeNumber")]
  [Computed]
  public string Owner4HomeNumber_Json
  {
    get => NullIf(Owner4HomeNumber, "");
    set => Owner4HomeNumber = value;
  }

  /// <summary>Length of the OWNER4_BUSINESS_NUMBER attribute.</summary>
  public const int Owner4BusinessNumber_MaxLength = 25;

  /// <summary>
  /// The value of the OWNER4_BUSINESS_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 63, Type = MemberType.Char, Length
    = Owner4BusinessNumber_MaxLength)]
  public string Owner4BusinessNumber
  {
    get => owner4BusinessNumber ?? "";
    set => owner4BusinessNumber =
      TrimEnd(Substring(value, 1, Owner4BusinessNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner4BusinessNumber attribute.</summary>
  [JsonPropertyName("owner4BusinessNumber")]
  [Computed]
  public string Owner4BusinessNumber_Json
  {
    get => NullIf(Owner4BusinessNumber, "");
    set => Owner4BusinessNumber = value;
  }

  /// <summary>Length of the OWNER5_ORGANIZATION_NAME attribute.</summary>
  public const int Owner5OrganizationName_MaxLength = 66;

  /// <summary>
  /// The value of the OWNER5_ORGANIZATION_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 64, Type = MemberType.Char, Length
    = Owner5OrganizationName_MaxLength)]
  public string Owner5OrganizationName
  {
    get => owner5OrganizationName ?? "";
    set => owner5OrganizationName =
      TrimEnd(Substring(value, 1, Owner5OrganizationName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5OrganizationName attribute.</summary>
  [JsonPropertyName("owner5OrganizationName")]
  [Computed]
  public string Owner5OrganizationName_Json
  {
    get => NullIf(Owner5OrganizationName, "");
    set => Owner5OrganizationName = value;
  }

  /// <summary>Length of the OWNER5_FIRST_NAME attribute.</summary>
  public const int Owner5FirstName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER5_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 65, Type = MemberType.Char, Length = Owner5FirstName_MaxLength)
    ]
  public string Owner5FirstName
  {
    get => owner5FirstName ?? "";
    set => owner5FirstName =
      TrimEnd(Substring(value, 1, Owner5FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5FirstName attribute.</summary>
  [JsonPropertyName("owner5FirstName")]
  [Computed]
  public string Owner5FirstName_Json
  {
    get => NullIf(Owner5FirstName, "");
    set => Owner5FirstName = value;
  }

  /// <summary>Length of the OWNER5_MIDDLE_NAME attribute.</summary>
  public const int Owner5MiddleName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER5_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 66, Type = MemberType.Char, Length
    = Owner5MiddleName_MaxLength)]
  public string Owner5MiddleName
  {
    get => owner5MiddleName ?? "";
    set => owner5MiddleName =
      TrimEnd(Substring(value, 1, Owner5MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5MiddleName attribute.</summary>
  [JsonPropertyName("owner5MiddleName")]
  [Computed]
  public string Owner5MiddleName_Json
  {
    get => NullIf(Owner5MiddleName, "");
    set => Owner5MiddleName = value;
  }

  /// <summary>Length of the OWNER5_LAST_NAME attribute.</summary>
  public const int Owner5LastName_MaxLength = 80;

  /// <summary>
  /// The value of the OWNER5_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 67, Type = MemberType.Char, Length = Owner5LastName_MaxLength)]
    
  public string Owner5LastName
  {
    get => owner5LastName ?? "";
    set => owner5LastName =
      TrimEnd(Substring(value, 1, Owner5LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5LastName attribute.</summary>
  [JsonPropertyName("owner5LastName")]
  [Computed]
  public string Owner5LastName_Json
  {
    get => NullIf(Owner5LastName, "");
    set => Owner5LastName = value;
  }

  /// <summary>Length of the OWNER5_SUFFIX attribute.</summary>
  public const int Owner5Suffix_MaxLength = 8;

  /// <summary>
  /// The value of the OWNER5_SUFFIX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 68, Type = MemberType.Char, Length = Owner5Suffix_MaxLength)]
  public string Owner5Suffix
  {
    get => owner5Suffix ?? "";
    set => owner5Suffix = TrimEnd(Substring(value, 1, Owner5Suffix_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5Suffix attribute.</summary>
  [JsonPropertyName("owner5Suffix")]
  [Computed]
  public string Owner5Suffix_Json
  {
    get => NullIf(Owner5Suffix, "");
    set => Owner5Suffix = value;
  }

  /// <summary>Length of the OWNER5_MAILING_ADDRESS_LINE_1 attribute.</summary>
  public const int Owner5MailingAddressLine1_MaxLength = 50;

  /// <summary>
  /// The value of the OWNER5_MAILING_ADDRESS_LINE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 69, Type = MemberType.Char, Length
    = Owner5MailingAddressLine1_MaxLength)]
  public string Owner5MailingAddressLine1
  {
    get => owner5MailingAddressLine1 ?? "";
    set => owner5MailingAddressLine1 =
      TrimEnd(Substring(value, 1, Owner5MailingAddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5MailingAddressLine1 attribute.</summary>
  [JsonPropertyName("owner5MailingAddressLine1")]
  [Computed]
  public string Owner5MailingAddressLine1_Json
  {
    get => NullIf(Owner5MailingAddressLine1, "");
    set => Owner5MailingAddressLine1 = value;
  }

  /// <summary>Length of the OWNER5_MAILING_ADDRESSS_LINE_2 attribute.</summary>
  public const int Owner5MailingAddresssLine2_MaxLength = 50;

  /// <summary>
  /// The value of the OWNER5_MAILING_ADDRESSS_LINE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 70, Type = MemberType.Char, Length
    = Owner5MailingAddresssLine2_MaxLength)]
  public string Owner5MailingAddresssLine2
  {
    get => owner5MailingAddresssLine2 ?? "";
    set => owner5MailingAddresssLine2 =
      TrimEnd(Substring(value, 1, Owner5MailingAddresssLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5MailingAddresssLine2 attribute.</summary>
  [JsonPropertyName("owner5MailingAddresssLine2")]
  [Computed]
  public string Owner5MailingAddresssLine2_Json
  {
    get => NullIf(Owner5MailingAddresssLine2, "");
    set => Owner5MailingAddresssLine2 = value;
  }

  /// <summary>Length of the OWNER5_MAILING_CITY attribute.</summary>
  public const int Owner5MailingCity_MaxLength = 20;

  /// <summary>
  /// The value of the OWNER5_MAILING_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 71, Type = MemberType.Char, Length
    = Owner5MailingCity_MaxLength)]
  public string Owner5MailingCity
  {
    get => owner5MailingCity ?? "";
    set => owner5MailingCity =
      TrimEnd(Substring(value, 1, Owner5MailingCity_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5MailingCity attribute.</summary>
  [JsonPropertyName("owner5MailingCity")]
  [Computed]
  public string Owner5MailingCity_Json
  {
    get => NullIf(Owner5MailingCity, "");
    set => Owner5MailingCity = value;
  }

  /// <summary>Length of the OWNER5_MAILING_STATE attribute.</summary>
  public const int Owner5MailingState_MaxLength = 4;

  /// <summary>
  /// The value of the OWNER5_MAILING_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 72, Type = MemberType.Char, Length
    = Owner5MailingState_MaxLength)]
  public string Owner5MailingState
  {
    get => owner5MailingState ?? "";
    set => owner5MailingState =
      TrimEnd(Substring(value, 1, Owner5MailingState_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5MailingState attribute.</summary>
  [JsonPropertyName("owner5MailingState")]
  [Computed]
  public string Owner5MailingState_Json
  {
    get => NullIf(Owner5MailingState, "");
    set => Owner5MailingState = value;
  }

  /// <summary>Length of the OWNER5_MAILING_ZIP_CODE attribute.</summary>
  public const int Owner5MailingZipCode_MaxLength = 9;

  /// <summary>
  /// The value of the OWNER5_MAILING_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 73, Type = MemberType.Char, Length
    = Owner5MailingZipCode_MaxLength)]
  public string Owner5MailingZipCode
  {
    get => owner5MailingZipCode ?? "";
    set => owner5MailingZipCode =
      TrimEnd(Substring(value, 1, Owner5MailingZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5MailingZipCode attribute.</summary>
  [JsonPropertyName("owner5MailingZipCode")]
  [Computed]
  public string Owner5MailingZipCode_Json
  {
    get => NullIf(Owner5MailingZipCode, "");
    set => Owner5MailingZipCode = value;
  }

  /// <summary>Length of the OWNER5_VESTMENT_TYPE attribute.</summary>
  public const int Owner5VestmentType_MaxLength = 30;

  /// <summary>
  /// The value of the OWNER5_VESTMENT_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 74, Type = MemberType.Char, Length
    = Owner5VestmentType_MaxLength)]
  public string Owner5VestmentType
  {
    get => owner5VestmentType ?? "";
    set => owner5VestmentType =
      TrimEnd(Substring(value, 1, Owner5VestmentType_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5VestmentType attribute.</summary>
  [JsonPropertyName("owner5VestmentType")]
  [Computed]
  public string Owner5VestmentType_Json
  {
    get => NullIf(Owner5VestmentType, "");
    set => Owner5VestmentType = value;
  }

  /// <summary>Length of the OWNER5_HOME_NUMBER attribute.</summary>
  public const int Owner5HomeNumber_MaxLength = 25;

  /// <summary>
  /// The value of the OWNER5_HOME_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 75, Type = MemberType.Char, Length
    = Owner5HomeNumber_MaxLength)]
  public string Owner5HomeNumber
  {
    get => owner5HomeNumber ?? "";
    set => owner5HomeNumber =
      TrimEnd(Substring(value, 1, Owner5HomeNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5HomeNumber attribute.</summary>
  [JsonPropertyName("owner5HomeNumber")]
  [Computed]
  public string Owner5HomeNumber_Json
  {
    get => NullIf(Owner5HomeNumber, "");
    set => Owner5HomeNumber = value;
  }

  /// <summary>Length of the OWNER5_BUSINESS_NUMBER attribute.</summary>
  public const int Owner5BusinessNumber_MaxLength = 25;

  /// <summary>
  /// The value of the OWNER5_BUSINESS_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 76, Type = MemberType.Char, Length
    = Owner5BusinessNumber_MaxLength)]
  public string Owner5BusinessNumber
  {
    get => owner5BusinessNumber ?? "";
    set => owner5BusinessNumber =
      TrimEnd(Substring(value, 1, Owner5BusinessNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the Owner5BusinessNumber attribute.</summary>
  [JsonPropertyName("owner5BusinessNumber")]
  [Computed]
  public string Owner5BusinessNumber_Json
  {
    get => NullIf(Owner5BusinessNumber, "");
    set => Owner5BusinessNumber = value;
  }

  private string lastName;
  private string firstName;
  private string ssn;
  private string dateOfBirth;
  private string personNumber;
  private string driversLicenseNumber;
  private string vin;
  private string make;
  private string model;
  private string year;
  private string plateNumber;
  private string owner1OrganizationName;
  private string owner1FirstName;
  private string owner1MiddleName;
  private string owner1LastName;
  private string owner1Suffix;
  private string owner1MailingAddressLine1;
  private string owner1MailingAddresssLine2;
  private string owner1MailingCity;
  private string owner1MailingState;
  private string owner1MailingZipCode;
  private string owner1VestmentType;
  private string owner1HomeNumber;
  private string owner1BusinessNumber;
  private string owner2OrganizationName;
  private string owner2FirstName;
  private string owner2MiddleName;
  private string owner2LastName;
  private string owner2Suffix;
  private string owner2MailingAddressLine1;
  private string owner2MailingAddresssLine2;
  private string owner2MailingCity;
  private string owner2MailingState;
  private string owner2MailingZipCode;
  private string owner2VestmentType;
  private string owner2HomeNumber;
  private string owner2BusinessNumber;
  private string owner3OrganizationName;
  private string owner3FirstName;
  private string owner3MiddleName;
  private string owner3LastName;
  private string owner3Suffix;
  private string owner3MailingAddressLine1;
  private string owner3MailingAddresssLine2;
  private string owner3MailingCity;
  private string owner3MailingState;
  private string owner3MailingZipCode;
  private string owner3VestmentType;
  private string owner3HomeNumber;
  private string owner3BusinessNumber;
  private string owner4OrganizationName;
  private string owner4FirstName;
  private string owner4MiddleName;
  private string owner4LastName;
  private string owner4Suffix;
  private string owner4MailingAddressLine1;
  private string owner4MailingAddresssLine2;
  private string owner4MailingCity;
  private string owner4MailingState;
  private string owner4MailingZipCode;
  private string owner4VestmentType;
  private string owner4HomeNumber;
  private string owner4BusinessNumber;
  private string owner5OrganizationName;
  private string owner5FirstName;
  private string owner5MiddleName;
  private string owner5LastName;
  private string owner5Suffix;
  private string owner5MailingAddressLine1;
  private string owner5MailingAddresssLine2;
  private string owner5MailingCity;
  private string owner5MailingState;
  private string owner5MailingZipCode;
  private string owner5VestmentType;
  private string owner5HomeNumber;
  private string owner5BusinessNumber;
}
