// The source file: KDOR_DL_ERROR_RECORD, ID: 1625328919, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class KdorDlErrorRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public KdorDlErrorRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public KdorDlErrorRecord(KdorDlErrorRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new KdorDlErrorRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(KdorDlErrorRecord that)
  {
    base.Assign(that);
    lastName = that.lastName;
    firstName = that.firstName;
    ssn = that.ssn;
    dateOfBirth = that.dateOfBirth;
    personNumber = that.personNumber;
    driversLicenseNumber = that.driversLicenseNumber;
    status = that.status;
    errorReason = that.errorReason;
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
  public const int DateOfBirth_MaxLength = 8;

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

  /// <summary>Length of the STATUS attribute.</summary>
  public const int Status_MaxLength = 3;

  /// <summary>
  /// The value of the STATUS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Status_MaxLength)]
  public string Status
  {
    get => status ?? "";
    set => status = TrimEnd(Substring(value, 1, Status_MaxLength));
  }

  /// <summary>
  /// The json value of the Status attribute.</summary>
  [JsonPropertyName("status")]
  [Computed]
  public string Status_Json
  {
    get => NullIf(Status, "");
    set => Status = value;
  }

  /// <summary>Length of the ERROR_REASON attribute.</summary>
  public const int ErrorReason_MaxLength = 20;

  /// <summary>
  /// The value of the ERROR_REASON attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ErrorReason_MaxLength)]
  public string ErrorReason
  {
    get => errorReason ?? "";
    set => errorReason = TrimEnd(Substring(value, 1, ErrorReason_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorReason attribute.</summary>
  [JsonPropertyName("errorReason")]
  [Computed]
  public string ErrorReason_Json
  {
    get => NullIf(ErrorReason, "");
    set => ErrorReason = value;
  }

  private string lastName;
  private string firstName;
  private string ssn;
  private string dateOfBirth;
  private string personNumber;
  private string driversLicenseNumber;
  private string status;
  private string errorReason;
}
