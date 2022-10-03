// The source file: KDOR_DRIVERS_LICENSE, ID: 1625319074, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Information about a drivers license match from Kansas Department of Revenue
/// (KDOR).
/// </summary>
[Serializable]
public partial class KdorDriversLicense: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public KdorDriversLicense()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public KdorDriversLicense(KdorDriversLicense that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new KdorDriversLicense Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(KdorDriversLicense that)
  {
    base.Assign(that);
    type1 = that.type1;
    lastName = that.lastName;
    firstName = that.firstName;
    ssn = that.ssn;
    dateOfBirth = that.dateOfBirth;
    licenseNumber = that.licenseNumber;
    createdTstamp = that.createdTstamp;
    createdBy = that.createdBy;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    status = that.status;
    errorReason = that.errorReason;
    dlClassInd = that.dlClassInd;
    motorcycleClassInd = that.motorcycleClassInd;
    cdlClassInd = that.cdlClassInd;
    expirationDt = that.expirationDt;
    genderCode = that.genderCode;
    addressLine1 = that.addressLine1;
    addressLine2 = that.addressLine2;
    city = that.city;
    state = that.state;
    zipCode = that.zipCode;
    heightFeet = that.heightFeet;
    heightInches = that.heightInches;
    weight = that.weight;
    eyeColor = that.eyeColor;
    fkCktCsePersnumb = that.fkCktCsePersnumb;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines whether the KDOR matching process produced a valid drivers 
  /// license match or an error.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("E")]
  [Value("M")]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// Defines whether the KDOR matching process produced a valid drivers 
  /// license match or an error.
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
  /// First name on the drivers license
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
  /// Social Security Number associated with the drivers license.
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
  /// Date of birth on the drivers license.
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
  /// Drivers license number.
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

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
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
  [Member(Index = 8, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The user id or batch program that last updated the record.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 9, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// Timestamp of last update of the record.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 10, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>Length of the STATUS attribute.</summary>
  public const int Status_MaxLength = 3;

  /// <summary>
  /// The value of the STATUS attribute.
  /// Status of the drivers license as provided by KDOR
  /// </summary>
  [JsonPropertyName("status")]
  [Member(Index = 11, Type = MemberType.Char, Length = Status_MaxLength, Optional
    = true)]
  public string Status
  {
    get => status;
    set => status = value != null
      ? TrimEnd(Substring(value, 1, Status_MaxLength)) : null;
  }

  /// <summary>Length of the ERROR_REASON attribute.</summary>
  public const int ErrorReason_MaxLength = 20;

  /// <summary>
  /// The value of the ERROR_REASON attribute.
  /// Description of the drivers license match error encountered by KDOR
  /// </summary>
  [JsonPropertyName("errorReason")]
  [Member(Index = 12, Type = MemberType.Char, Length = ErrorReason_MaxLength, Optional
    = true)]
  public string ErrorReason
  {
    get => errorReason;
    set => errorReason = value != null
      ? TrimEnd(Substring(value, 1, ErrorReason_MaxLength)) : null;
  }

  /// <summary>Length of the DL_CLASS_IND attribute.</summary>
  public const int DlClassInd_MaxLength = 1;

  /// <summary>
  /// The value of the DL_CLASS_IND attribute.
  /// Yes/No indicator whether the drivers license is valid for automobiles.
  /// </summary>
  [JsonPropertyName("dlClassInd")]
  [Member(Index = 13, Type = MemberType.Char, Length = DlClassInd_MaxLength, Optional
    = true)]
  public string DlClassInd
  {
    get => dlClassInd;
    set => dlClassInd = value != null
      ? TrimEnd(Substring(value, 1, DlClassInd_MaxLength)) : null;
  }

  /// <summary>Length of the MOTORCYCLE_CLASS_IND attribute.</summary>
  public const int MotorcycleClassInd_MaxLength = 1;

  /// <summary>
  /// The value of the MOTORCYCLE_CLASS_IND attribute.
  /// Yes/No indicator whether the drivers license is valid for motorcycles
  /// </summary>
  [JsonPropertyName("motorcycleClassInd")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = MotorcycleClassInd_MaxLength, Optional = true)]
  public string MotorcycleClassInd
  {
    get => motorcycleClassInd;
    set => motorcycleClassInd = value != null
      ? TrimEnd(Substring(value, 1, MotorcycleClassInd_MaxLength)) : null;
  }

  /// <summary>Length of the CDL_CLASS_IND attribute.</summary>
  public const int CdlClassInd_MaxLength = 1;

  /// <summary>
  /// The value of the CDL_CLASS_IND attribute.
  /// Yes/No indicator whether the drivers license is valid for commercial 
  /// vehicles.
  /// </summary>
  [JsonPropertyName("cdlClassInd")]
  [Member(Index = 15, Type = MemberType.Char, Length = CdlClassInd_MaxLength, Optional
    = true)]
  public string CdlClassInd
  {
    get => cdlClassInd;
    set => cdlClassInd = value != null
      ? TrimEnd(Substring(value, 1, CdlClassInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EXPIRATION_DT attribute.
  /// Date the drivers license expires.
  /// </summary>
  [JsonPropertyName("expirationDt")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? ExpirationDt
  {
    get => expirationDt;
    set => expirationDt = value;
  }

  /// <summary>Length of the GENDER_CODE attribute.</summary>
  public const int GenderCode_MaxLength = 1;

  /// <summary>
  /// The value of the GENDER_CODE attribute.
  /// Code indicating the person gender as it appears on the drivers license
  /// </summary>
  [JsonPropertyName("genderCode")]
  [Member(Index = 17, Type = MemberType.Char, Length = GenderCode_MaxLength, Optional
    = true)]
  public string GenderCode
  {
    get => genderCode;
    set => genderCode = value != null
      ? TrimEnd(Substring(value, 1, GenderCode_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_LINE_1 attribute.</summary>
  public const int AddressLine1_MaxLength = 20;

  /// <summary>
  /// The value of the ADDRESS_LINE_1 attribute.
  /// First street address line as it appears on the drivers license.
  /// </summary>
  [JsonPropertyName("addressLine1")]
  [Member(Index = 18, Type = MemberType.Char, Length = AddressLine1_MaxLength, Optional
    = true)]
  public string AddressLine1
  {
    get => addressLine1;
    set => addressLine1 = value != null
      ? TrimEnd(Substring(value, 1, AddressLine1_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_LINE_2 attribute.</summary>
  public const int AddressLine2_MaxLength = 20;

  /// <summary>
  /// The value of the ADDRESS_LINE_2 attribute.
  /// Second street address line as it appears on the drivers license
  /// </summary>
  [JsonPropertyName("addressLine2")]
  [Member(Index = 19, Type = MemberType.Char, Length = AddressLine2_MaxLength, Optional
    = true)]
  public string AddressLine2
  {
    get => addressLine2;
    set => addressLine2 = value != null
      ? TrimEnd(Substring(value, 1, AddressLine2_MaxLength)) : null;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 15;

  /// <summary>
  /// The value of the CITY attribute.
  /// City as it appears on the drivers license.
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 20, Type = MemberType.Char, Length = City_MaxLength, Optional
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
  /// State as it appears on the drivers license.
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 21, Type = MemberType.Char, Length = State_MaxLength, Optional
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
  /// Zip code as it appears on the drivers license.
  /// </summary>
  [JsonPropertyName("zipCode")]
  [Member(Index = 22, Type = MemberType.Char, Length = ZipCode_MaxLength, Optional
    = true)]
  public string ZipCode
  {
    get => zipCode;
    set => zipCode = value != null
      ? TrimEnd(Substring(value, 1, ZipCode_MaxLength)) : null;
  }

  /// <summary>Length of the HEIGHT_FEET attribute.</summary>
  public const int HeightFeet_MaxLength = 1;

  /// <summary>
  /// The value of the HEIGHT_FEET attribute.
  /// Height, in feet, as it appears on the drivers license.
  /// </summary>
  [JsonPropertyName("heightFeet")]
  [Member(Index = 23, Type = MemberType.Char, Length = HeightFeet_MaxLength, Optional
    = true)]
  public string HeightFeet
  {
    get => heightFeet;
    set => heightFeet = value != null
      ? TrimEnd(Substring(value, 1, HeightFeet_MaxLength)) : null;
  }

  /// <summary>Length of the HEIGHT_INCHES attribute.</summary>
  public const int HeightInches_MaxLength = 2;

  /// <summary>
  /// The value of the HEIGHT_INCHES attribute.
  /// Height, in inches, as it appears on the drivers license.
  /// </summary>
  [JsonPropertyName("heightInches")]
  [Member(Index = 24, Type = MemberType.Char, Length = HeightInches_MaxLength, Optional
    = true)]
  public string HeightInches
  {
    get => heightInches;
    set => heightInches = value != null
      ? TrimEnd(Substring(value, 1, HeightInches_MaxLength)) : null;
  }

  /// <summary>Length of the WEIGHT attribute.</summary>
  public const int Weight_MaxLength = 3;

  /// <summary>
  /// The value of the WEIGHT attribute.
  /// Weight as it appears on the drivers license.
  /// </summary>
  [JsonPropertyName("weight")]
  [Member(Index = 25, Type = MemberType.Char, Length = Weight_MaxLength, Optional
    = true)]
  public string Weight
  {
    get => weight;
    set => weight = value != null
      ? TrimEnd(Substring(value, 1, Weight_MaxLength)) : null;
  }

  /// <summary>Length of the EYE_COLOR attribute.</summary>
  public const int EyeColor_MaxLength = 3;

  /// <summary>
  /// The value of the EYE_COLOR attribute.
  /// Eye color as it appears on the drivers license.
  /// </summary>
  [JsonPropertyName("eyeColor")]
  [Member(Index = 26, Type = MemberType.Char, Length = EyeColor_MaxLength, Optional
    = true)]
  public string EyeColor
  {
    get => eyeColor;
    set => eyeColor = value != null
      ? TrimEnd(Substring(value, 1, EyeColor_MaxLength)) : null;
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
  [Member(Index = 27, Type = MemberType.Char, Length
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

  private string type1;
  private string lastName;
  private string firstName;
  private string ssn;
  private DateTime? dateOfBirth;
  private string licenseNumber;
  private DateTime? createdTstamp;
  private string createdBy;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private string status;
  private string errorReason;
  private string dlClassInd;
  private string motorcycleClassInd;
  private string cdlClassInd;
  private DateTime? expirationDt;
  private string genderCode;
  private string addressLine1;
  private string addressLine2;
  private string city;
  private string state;
  private string zipCode;
  private string heightFeet;
  private string heightInches;
  private string weight;
  private string eyeColor;
  private string fkCktCsePersnumb;
}
