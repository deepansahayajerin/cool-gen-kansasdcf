// The source file: KDOR_DL_MATCH_RECORD, ID: 1625328929, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class KdorDlMatchRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public KdorDlMatchRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public KdorDlMatchRecord(KdorDlMatchRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new KdorDlMatchRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(KdorDlMatchRecord that)
  {
    base.Assign(that);
    lastName = that.lastName;
    firstName = that.firstName;
    ssn = that.ssn;
    dateOfBirth = that.dateOfBirth;
    personNumber = that.personNumber;
    driversLicenseNumber = that.driversLicenseNumber;
    dlClass = that.dlClass;
    motocycleClass = that.motocycleClass;
    cdlClass = that.cdlClass;
    expirationDate = that.expirationDate;
    gender = that.gender;
    addressLine1 = that.addressLine1;
    addressLine2 = that.addressLine2;
    city = that.city;
    state = that.state;
    zipCode = that.zipCode;
    heightFt = that.heightFt;
    heightIn = that.heightIn;
    weight = that.weight;
    eyeColor = that.eyeColor;
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

  /// <summary>Length of the DL_CLASS attribute.</summary>
  public const int DlClass_MaxLength = 1;

  /// <summary>
  /// The value of the DL_CLASS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = DlClass_MaxLength)]
  public string DlClass
  {
    get => dlClass ?? "";
    set => dlClass = TrimEnd(Substring(value, 1, DlClass_MaxLength));
  }

  /// <summary>
  /// The json value of the DlClass attribute.</summary>
  [JsonPropertyName("dlClass")]
  [Computed]
  public string DlClass_Json
  {
    get => NullIf(DlClass, "");
    set => DlClass = value;
  }

  /// <summary>Length of the MOTOCYCLE_CLASS attribute.</summary>
  public const int MotocycleClass_MaxLength = 1;

  /// <summary>
  /// The value of the MOTOCYCLE_CLASS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = MotocycleClass_MaxLength)]
  public string MotocycleClass
  {
    get => motocycleClass ?? "";
    set => motocycleClass =
      TrimEnd(Substring(value, 1, MotocycleClass_MaxLength));
  }

  /// <summary>
  /// The json value of the MotocycleClass attribute.</summary>
  [JsonPropertyName("motocycleClass")]
  [Computed]
  public string MotocycleClass_Json
  {
    get => NullIf(MotocycleClass, "");
    set => MotocycleClass = value;
  }

  /// <summary>Length of the CDL_CLASS attribute.</summary>
  public const int CdlClass_MaxLength = 1;

  /// <summary>
  /// The value of the CDL_CLASS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CdlClass_MaxLength)]
  public string CdlClass
  {
    get => cdlClass ?? "";
    set => cdlClass = TrimEnd(Substring(value, 1, CdlClass_MaxLength));
  }

  /// <summary>
  /// The json value of the CdlClass attribute.</summary>
  [JsonPropertyName("cdlClass")]
  [Computed]
  public string CdlClass_Json
  {
    get => NullIf(CdlClass, "");
    set => CdlClass = value;
  }

  /// <summary>Length of the EXPIRATION_DATE attribute.</summary>
  public const int ExpirationDate_MaxLength = 10;

  /// <summary>
  /// The value of the EXPIRATION_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = ExpirationDate_MaxLength)]
    
  public string ExpirationDate
  {
    get => expirationDate ?? "";
    set => expirationDate =
      TrimEnd(Substring(value, 1, ExpirationDate_MaxLength));
  }

  /// <summary>
  /// The json value of the ExpirationDate attribute.</summary>
  [JsonPropertyName("expirationDate")]
  [Computed]
  public string ExpirationDate_Json
  {
    get => NullIf(ExpirationDate, "");
    set => ExpirationDate = value;
  }

  /// <summary>Length of the GENDER attribute.</summary>
  public const int Gender_MaxLength = 1;

  /// <summary>
  /// The value of the GENDER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Gender_MaxLength)]
  public string Gender
  {
    get => gender ?? "";
    set => gender = TrimEnd(Substring(value, 1, Gender_MaxLength));
  }

  /// <summary>
  /// The json value of the Gender attribute.</summary>
  [JsonPropertyName("gender")]
  [Computed]
  public string Gender_Json
  {
    get => NullIf(Gender, "");
    set => Gender = value;
  }

  /// <summary>Length of the ADDRESS_LINE_1 attribute.</summary>
  public const int AddressLine1_MaxLength = 20;

  /// <summary>
  /// The value of the ADDRESS_LINE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = AddressLine1_MaxLength)]
  public string AddressLine1
  {
    get => addressLine1 ?? "";
    set => addressLine1 = TrimEnd(Substring(value, 1, AddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressLine1 attribute.</summary>
  [JsonPropertyName("addressLine1")]
  [Computed]
  public string AddressLine1_Json
  {
    get => NullIf(AddressLine1, "");
    set => AddressLine1 = value;
  }

  /// <summary>Length of the ADDRESS_LINE_2 attribute.</summary>
  public const int AddressLine2_MaxLength = 20;

  /// <summary>
  /// The value of the ADDRESS_LINE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = AddressLine2_MaxLength)]
  public string AddressLine2
  {
    get => addressLine2 ?? "";
    set => addressLine2 = TrimEnd(Substring(value, 1, AddressLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressLine2 attribute.</summary>
  [JsonPropertyName("addressLine2")]
  [Computed]
  public string AddressLine2_Json
  {
    get => NullIf(AddressLine2, "");
    set => AddressLine2 = value;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 15;

  /// <summary>
  /// The value of the CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = City_MaxLength)]
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
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = State_MaxLength)]
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

  /// <summary>Length of the ZIP_CODE attribute.</summary>
  public const int ZipCode_MaxLength = 9;

  /// <summary>
  /// The value of the ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = ZipCode_MaxLength)]
  public string ZipCode
  {
    get => zipCode ?? "";
    set => zipCode = TrimEnd(Substring(value, 1, ZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ZipCode attribute.</summary>
  [JsonPropertyName("zipCode")]
  [Computed]
  public string ZipCode_Json
  {
    get => NullIf(ZipCode, "");
    set => ZipCode = value;
  }

  /// <summary>Length of the HEIGHT_FT attribute.</summary>
  public const int HeightFt_MaxLength = 1;

  /// <summary>
  /// The value of the HEIGHT_FT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = HeightFt_MaxLength)]
  public string HeightFt
  {
    get => heightFt ?? "";
    set => heightFt = TrimEnd(Substring(value, 1, HeightFt_MaxLength));
  }

  /// <summary>
  /// The json value of the HeightFt attribute.</summary>
  [JsonPropertyName("heightFt")]
  [Computed]
  public string HeightFt_Json
  {
    get => NullIf(HeightFt, "");
    set => HeightFt = value;
  }

  /// <summary>Length of the HEIGHT_IN attribute.</summary>
  public const int HeightIn_MaxLength = 2;

  /// <summary>
  /// The value of the HEIGHT_IN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = HeightIn_MaxLength)]
  public string HeightIn
  {
    get => heightIn ?? "";
    set => heightIn = TrimEnd(Substring(value, 1, HeightIn_MaxLength));
  }

  /// <summary>
  /// The json value of the HeightIn attribute.</summary>
  [JsonPropertyName("heightIn")]
  [Computed]
  public string HeightIn_Json
  {
    get => NullIf(HeightIn, "");
    set => HeightIn = value;
  }

  /// <summary>Length of the WEIGHT attribute.</summary>
  public const int Weight_MaxLength = 3;

  /// <summary>
  /// The value of the WEIGHT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = Weight_MaxLength)]
  public string Weight
  {
    get => weight ?? "";
    set => weight = TrimEnd(Substring(value, 1, Weight_MaxLength));
  }

  /// <summary>
  /// The json value of the Weight attribute.</summary>
  [JsonPropertyName("weight")]
  [Computed]
  public string Weight_Json
  {
    get => NullIf(Weight, "");
    set => Weight = value;
  }

  /// <summary>Length of the EYE_COLOR attribute.</summary>
  public const int EyeColor_MaxLength = 3;

  /// <summary>
  /// The value of the EYE_COLOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = EyeColor_MaxLength)]
  public string EyeColor
  {
    get => eyeColor ?? "";
    set => eyeColor = TrimEnd(Substring(value, 1, EyeColor_MaxLength));
  }

  /// <summary>
  /// The json value of the EyeColor attribute.</summary>
  [JsonPropertyName("eyeColor")]
  [Computed]
  public string EyeColor_Json
  {
    get => NullIf(EyeColor, "");
    set => EyeColor = value;
  }

  private string lastName;
  private string firstName;
  private string ssn;
  private string dateOfBirth;
  private string personNumber;
  private string driversLicenseNumber;
  private string dlClass;
  private string motocycleClass;
  private string cdlClass;
  private string expirationDate;
  private string gender;
  private string addressLine1;
  private string addressLine2;
  private string city;
  private string state;
  private string zipCode;
  private string heightFt;
  private string heightIn;
  private string weight;
  private string eyeColor;
}
