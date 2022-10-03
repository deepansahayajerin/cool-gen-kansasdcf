// The source file: KDMV_FILE, ID: 371325860, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class KdmvFile: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public KdmvFile()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public KdmvFile(KdmvFile that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new KdmvFile Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(KdmvFile that)
  {
    base.Assign(that);
    fileType = that.fileType;
    csePersonNumber = that.csePersonNumber;
    lastName = that.lastName;
    firstName = that.firstName;
    ssn = that.ssn;
    dob = that.dob;
    driverLicenseNumber = that.driverLicenseNumber;
    dmvProblemText = that.dmvProblemText;
    requestStatus = that.requestStatus;
    processStatus = that.processStatus;
    processDate = that.processDate;
  }

  /// <summary>Length of the FILE_TYPE attribute.</summary>
  public const int FileType_MaxLength = 1;

  /// <summary>
  /// The value of the FILE_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = FileType_MaxLength)]
  public string FileType
  {
    get => fileType ?? "";
    set => fileType = TrimEnd(Substring(value, 1, FileType_MaxLength));
  }

  /// <summary>
  /// The json value of the FileType attribute.</summary>
  [JsonPropertyName("fileType")]
  [Computed]
  public string FileType_Json
  {
    get => NullIf(FileType, "");
    set => FileType = value;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CsePersonNumber_MaxLength)]
    
  public string CsePersonNumber
  {
    get => csePersonNumber ?? "";
    set => csePersonNumber =
      TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CsePersonNumber attribute.</summary>
  [JsonPropertyName("csePersonNumber")]
  [Computed]
  public string CsePersonNumber_Json
  {
    get => NullIf(CsePersonNumber, "");
    set => CsePersonNumber = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = LastName_MaxLength)]
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
  [Member(Index = 4, Type = MemberType.Char, Length = FirstName_MaxLength)]
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
  [Member(Index = 5, Type = MemberType.Char, Length = Ssn_MaxLength)]
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

  /// <summary>Length of the DOB attribute.</summary>
  public const int Dob_MaxLength = 8;

  /// <summary>
  /// The value of the DOB attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Dob_MaxLength)]
  public string Dob
  {
    get => dob ?? "";
    set => dob = TrimEnd(Substring(value, 1, Dob_MaxLength));
  }

  /// <summary>
  /// The json value of the Dob attribute.</summary>
  [JsonPropertyName("dob")]
  [Computed]
  public string Dob_Json
  {
    get => NullIf(Dob, "");
    set => Dob = value;
  }

  /// <summary>Length of the DRIVER_LICENSE_NUMBER attribute.</summary>
  public const int DriverLicenseNumber_MaxLength = 9;

  /// <summary>
  /// The value of the DRIVER_LICENSE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = DriverLicenseNumber_MaxLength)]
  public string DriverLicenseNumber
  {
    get => driverLicenseNumber ?? "";
    set => driverLicenseNumber =
      TrimEnd(Substring(value, 1, DriverLicenseNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the DriverLicenseNumber attribute.</summary>
  [JsonPropertyName("driverLicenseNumber")]
  [Computed]
  public string DriverLicenseNumber_Json
  {
    get => NullIf(DriverLicenseNumber, "");
    set => DriverLicenseNumber = value;
  }

  /// <summary>Length of the DMV_PROBLEM_TEXT attribute.</summary>
  public const int DmvProblemText_MaxLength = 37;

  /// <summary>
  /// The value of the DMV_PROBLEM_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = DmvProblemText_MaxLength)]
  public string DmvProblemText
  {
    get => dmvProblemText ?? "";
    set => dmvProblemText =
      TrimEnd(Substring(value, 1, DmvProblemText_MaxLength));
  }

  /// <summary>
  /// The json value of the DmvProblemText attribute.</summary>
  [JsonPropertyName("dmvProblemText")]
  [Computed]
  public string DmvProblemText_Json
  {
    get => NullIf(DmvProblemText, "");
    set => DmvProblemText = value;
  }

  /// <summary>Length of the REQUEST_STATUS attribute.</summary>
  public const int RequestStatus_MaxLength = 3;

  /// <summary>
  /// The value of the REQUEST_STATUS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = RequestStatus_MaxLength)]
  public string RequestStatus
  {
    get => requestStatus ?? "";
    set => requestStatus =
      TrimEnd(Substring(value, 1, RequestStatus_MaxLength));
  }

  /// <summary>
  /// The json value of the RequestStatus attribute.</summary>
  [JsonPropertyName("requestStatus")]
  [Computed]
  public string RequestStatus_Json
  {
    get => NullIf(RequestStatus, "");
    set => RequestStatus = value;
  }

  /// <summary>Length of the PROCESS_STATUS attribute.</summary>
  public const int ProcessStatus_MaxLength = 1;

  /// <summary>
  /// The value of the PROCESS_STATUS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = ProcessStatus_MaxLength)]
  public string ProcessStatus
  {
    get => processStatus ?? "";
    set => processStatus =
      TrimEnd(Substring(value, 1, ProcessStatus_MaxLength));
  }

  /// <summary>
  /// The json value of the ProcessStatus attribute.</summary>
  [JsonPropertyName("processStatus")]
  [Computed]
  public string ProcessStatus_Json
  {
    get => NullIf(ProcessStatus, "");
    set => ProcessStatus = value;
  }

  /// <summary>Length of the PROCESS_DATE attribute.</summary>
  public const int ProcessDate_MaxLength = 8;

  /// <summary>
  /// The value of the PROCESS_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = ProcessDate_MaxLength)]
  public string ProcessDate
  {
    get => processDate ?? "";
    set => processDate = TrimEnd(Substring(value, 1, ProcessDate_MaxLength));
  }

  /// <summary>
  /// The json value of the ProcessDate attribute.</summary>
  [JsonPropertyName("processDate")]
  [Computed]
  public string ProcessDate_Json
  {
    get => NullIf(ProcessDate, "");
    set => ProcessDate = value;
  }

  private string fileType;
  private string csePersonNumber;
  private string lastName;
  private string firstName;
  private string ssn;
  private string dob;
  private string driverLicenseNumber;
  private string dmvProblemText;
  private string requestStatus;
  private string processStatus;
  private string processDate;
}
