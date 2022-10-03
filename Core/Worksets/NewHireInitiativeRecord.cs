// The source file: NEW_HIRE_INITIATIVE_RECORD, ID: 1902520763, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class NewHireInitiativeRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NewHireInitiativeRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NewHireInitiativeRecord(NewHireInitiativeRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NewHireInitiativeRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NewHireInitiativeRecord that)
  {
    base.Assign(that);
    groupId = that.groupId;
    fein = that.fein;
    kansasId = that.kansasId;
    employerName = that.employerName;
    addressLine1 = that.addressLine1;
    addressLine2 = that.addressLine2;
    city = that.city;
    state = that.state;
    zipCode = that.zipCode;
    zipExtension = that.zipExtension;
    interventionType = that.interventionType;
    personNumber = that.personNumber;
    hireDate = that.hireDate;
    receivedDate = that.receivedDate;
    numberOfDays = that.numberOfDays;
    outputRecord = that.outputRecord;
  }

  /// <summary>Length of the GROUP_ID attribute.</summary>
  public const int GroupId_MaxLength = 9;

  /// <summary>
  /// The value of the GROUP_ID attribute.
  /// Number used to identify the initiative grouping.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = GroupId_MaxLength)]
  public string GroupId
  {
    get => groupId ?? "";
    set => groupId = TrimEnd(Substring(value, 1, GroupId_MaxLength));
  }

  /// <summary>
  /// The json value of the GroupId attribute.</summary>
  [JsonPropertyName("groupId")]
  [Computed]
  public string GroupId_Json
  {
    get => NullIf(GroupId, "");
    set => GroupId = value;
  }

  /// <summary>Length of the FEIN attribute.</summary>
  public const int Fein_MaxLength = 9;

  /// <summary>
  /// The value of the FEIN attribute.
  /// The Federal Employer Identification Number being tracked.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Fein_MaxLength)]
  public string Fein
  {
    get => fein ?? "";
    set => fein = TrimEnd(Substring(value, 1, Fein_MaxLength));
  }

  /// <summary>
  /// The json value of the Fein attribute.</summary>
  [JsonPropertyName("fein")]
  [Computed]
  public string Fein_Json
  {
    get => NullIf(Fein, "");
    set => Fein = value;
  }

  /// <summary>Length of the KANSAS_ID attribute.</summary>
  public const int KansasId_MaxLength = 6;

  /// <summary>
  /// The value of the KANSAS_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = KansasId_MaxLength)]
  public string KansasId
  {
    get => kansasId ?? "";
    set => kansasId = TrimEnd(Substring(value, 1, KansasId_MaxLength));
  }

  /// <summary>
  /// The json value of the KansasId attribute.</summary>
  [JsonPropertyName("kansasId")]
  [Computed]
  public string KansasId_Json
  {
    get => NullIf(KansasId, "");
    set => KansasId = value;
  }

  /// <summary>Length of the EMPLOYER_NAME attribute.</summary>
  public const int EmployerName_MaxLength = 45;

  /// <summary>
  /// The value of the EMPLOYER_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = EmployerName_MaxLength)]
  public string EmployerName
  {
    get => employerName ?? "";
    set => employerName = TrimEnd(Substring(value, 1, EmployerName_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerName attribute.</summary>
  [JsonPropertyName("employerName")]
  [Computed]
  public string EmployerName_Json
  {
    get => NullIf(EmployerName, "");
    set => EmployerName = value;
  }

  /// <summary>Length of the ADDRESS_LINE_1 attribute.</summary>
  public const int AddressLine1_MaxLength = 40;

  /// <summary>
  /// The value of the ADDRESS_LINE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = AddressLine1_MaxLength)]
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
  public const int AddressLine2_MaxLength = 40;

  /// <summary>
  /// The value of the ADDRESS_LINE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = AddressLine2_MaxLength)]
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
  public const int City_MaxLength = 25;

  /// <summary>
  /// The value of the CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = City_MaxLength)]
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
  [Member(Index = 8, Type = MemberType.Char, Length = State_MaxLength)]
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
  public const int ZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = ZipCode_MaxLength)]
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

  /// <summary>Length of the ZIP_EXTENSION attribute.</summary>
  public const int ZipExtension_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP_EXTENSION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = ZipExtension_MaxLength)]
  public string ZipExtension
  {
    get => zipExtension ?? "";
    set => zipExtension = TrimEnd(Substring(value, 1, ZipExtension_MaxLength));
  }

  /// <summary>
  /// The json value of the ZipExtension attribute.</summary>
  [JsonPropertyName("zipExtension")]
  [Computed]
  public string ZipExtension_Json
  {
    get => NullIf(ZipExtension, "");
    set => ZipExtension = value;
  }

  /// <summary>Length of the INTERVENTION_TYPE attribute.</summary>
  public const int InterventionType_MaxLength = 15;

  /// <summary>
  /// The value of the INTERVENTION_TYPE attribute.
  /// The type of intervention that was provided to the employer.  (Letter, 
  /// Postcard, Pamphlet, Phone Call, Control Group)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = InterventionType_MaxLength)]
  public string InterventionType
  {
    get => interventionType ?? "";
    set => interventionType =
      TrimEnd(Substring(value, 1, InterventionType_MaxLength));
  }

  /// <summary>
  /// The json value of the InterventionType attribute.</summary>
  [JsonPropertyName("interventionType")]
  [Computed]
  public string InterventionType_Json
  {
    get => NullIf(InterventionType, "");
    set => InterventionType = value;
  }

  /// <summary>Length of the PERSON_NUMBER attribute.</summary>
  public const int PersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the PERSON_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = PersonNumber_MaxLength)]
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

  /// <summary>Length of the HIRE_DATE attribute.</summary>
  public const int HireDate_MaxLength = 10;

  /// <summary>
  /// The value of the HIRE_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = HireDate_MaxLength)]
  public string HireDate
  {
    get => hireDate ?? "";
    set => hireDate = TrimEnd(Substring(value, 1, HireDate_MaxLength));
  }

  /// <summary>
  /// The json value of the HireDate attribute.</summary>
  [JsonPropertyName("hireDate")]
  [Computed]
  public string HireDate_Json
  {
    get => NullIf(HireDate, "");
    set => HireDate = value;
  }

  /// <summary>Length of the RECEIVED_DATE attribute.</summary>
  public const int ReceivedDate_MaxLength = 10;

  /// <summary>
  /// The value of the RECEIVED_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = ReceivedDate_MaxLength)]
  public string ReceivedDate
  {
    get => receivedDate ?? "";
    set => receivedDate = TrimEnd(Substring(value, 1, ReceivedDate_MaxLength));
  }

  /// <summary>
  /// The json value of the ReceivedDate attribute.</summary>
  [JsonPropertyName("receivedDate")]
  [Computed]
  public string ReceivedDate_Json
  {
    get => NullIf(ReceivedDate, "");
    set => ReceivedDate = value;
  }

  /// <summary>Length of the NUMBER_OF_DAYS attribute.</summary>
  public const int NumberOfDays_MaxLength = 9;

  /// <summary>
  /// The value of the NUMBER_OF_DAYS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = NumberOfDays_MaxLength)]
  public string NumberOfDays
  {
    get => numberOfDays ?? "";
    set => numberOfDays = TrimEnd(Substring(value, 1, NumberOfDays_MaxLength));
  }

  /// <summary>
  /// The json value of the NumberOfDays attribute.</summary>
  [JsonPropertyName("numberOfDays")]
  [Computed]
  public string NumberOfDays_Json
  {
    get => NullIf(NumberOfDays, "");
    set => NumberOfDays = value;
  }

  /// <summary>Length of the OUTPUT_RECORD attribute.</summary>
  public const int OutputRecord_MaxLength = 254;

  /// <summary>
  /// The value of the OUTPUT_RECORD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = OutputRecord_MaxLength)]
  public string OutputRecord
  {
    get => outputRecord ?? "";
    set => outputRecord = TrimEnd(Substring(value, 1, OutputRecord_MaxLength));
  }

  /// <summary>
  /// The json value of the OutputRecord attribute.</summary>
  [JsonPropertyName("outputRecord")]
  [Computed]
  public string OutputRecord_Json
  {
    get => NullIf(OutputRecord, "");
    set => OutputRecord = value;
  }

  private string groupId;
  private string fein;
  private string kansasId;
  private string employerName;
  private string addressLine1;
  private string addressLine2;
  private string city;
  private string state;
  private string zipCode;
  private string zipExtension;
  private string interventionType;
  private string personNumber;
  private string hireDate;
  private string receivedDate;
  private string numberOfDays;
  private string outputRecord;
}
