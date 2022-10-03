// The source file: PARTICIPANT_RECORD, ID: 374396770, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ParticipantRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ParticipantRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ParticipantRecord(ParticipantRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ParticipantRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ParticipantRecord that)
  {
    base.Assign(that);
    recordType = that.recordType;
    role = that.role;
    type1 = that.type1;
    ssn = that.ssn;
    lastName = that.lastName;
    firstName = that.firstName;
    middleInitial = that.middleInitial;
    suffix = that.suffix;
    gender = that.gender;
    dateOfBirth = that.dateOfBirth;
    srsPersonNumber = that.srsPersonNumber;
    familyViolenceIndicator = that.familyViolenceIndicator;
    pin = that.pin;
    source = that.source;
    filler = that.filler;
  }

  /// <summary>Length of the RECORD_TYPE attribute.</summary>
  public const int RecordType_MaxLength = 1;

  /// <summary>
  /// The value of the RECORD_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordType_MaxLength)]
  public string RecordType
  {
    get => recordType ?? "";
    set => recordType = TrimEnd(Substring(value, 1, RecordType_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordType attribute.</summary>
  [JsonPropertyName("recordType")]
  [Computed]
  public string RecordType_Json
  {
    get => NullIf(RecordType, "");
    set => RecordType = value;
  }

  /// <summary>Length of the ROLE attribute.</summary>
  public const int Role_MaxLength = 4;

  /// <summary>
  /// The value of the ROLE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Role_MaxLength)]
  public string Role
  {
    get => role ?? "";
    set => role = TrimEnd(Substring(value, 1, Role_MaxLength));
  }

  /// <summary>
  /// The json value of the Role attribute.</summary>
  [JsonPropertyName("role")]
  [Computed]
  public string Role_Json
  {
    get => NullIf(Role, "");
    set => Role = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 4;

  /// <summary>
  /// The value of the TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Type1_MaxLength)]
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

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Ssn_MaxLength)]
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

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 20;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastName_MaxLength)]
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
  public const int FirstName_MaxLength = 20;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = FirstName_MaxLength)]
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

  /// <summary>Length of the MIDDLE_INITIAL attribute.</summary>
  public const int MiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INITIAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = MiddleInitial_MaxLength)]
  public string MiddleInitial
  {
    get => middleInitial ?? "";
    set => middleInitial =
      TrimEnd(Substring(value, 1, MiddleInitial_MaxLength));
  }

  /// <summary>
  /// The json value of the MiddleInitial attribute.</summary>
  [JsonPropertyName("middleInitial")]
  [Computed]
  public string MiddleInitial_Json
  {
    get => NullIf(MiddleInitial, "");
    set => MiddleInitial = value;
  }

  /// <summary>Length of the SUFFIX attribute.</summary>
  public const int Suffix_MaxLength = 4;

  /// <summary>
  /// The value of the SUFFIX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = Suffix_MaxLength)]
  public string Suffix
  {
    get => suffix ?? "";
    set => suffix = TrimEnd(Substring(value, 1, Suffix_MaxLength));
  }

  /// <summary>
  /// The json value of the Suffix attribute.</summary>
  [JsonPropertyName("suffix")]
  [Computed]
  public string Suffix_Json
  {
    get => NullIf(Suffix, "");
    set => Suffix = value;
  }

  /// <summary>Length of the GENDER attribute.</summary>
  public const int Gender_MaxLength = 1;

  /// <summary>
  /// The value of the GENDER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = Gender_MaxLength)]
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

  /// <summary>Length of the DATE_OF_BIRTH attribute.</summary>
  public const int DateOfBirth_MaxLength = 8;

  /// <summary>
  /// The value of the DATE_OF_BIRTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = DateOfBirth_MaxLength)]
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

  /// <summary>Length of the SRS_PERSON_NUMBER attribute.</summary>
  public const int SrsPersonNumber_MaxLength = 15;

  /// <summary>
  /// The value of the SRS_PERSON_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = SrsPersonNumber_MaxLength)
    ]
  public string SrsPersonNumber
  {
    get => srsPersonNumber ?? "";
    set => srsPersonNumber =
      TrimEnd(Substring(value, 1, SrsPersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the SrsPersonNumber attribute.</summary>
  [JsonPropertyName("srsPersonNumber")]
  [Computed]
  public string SrsPersonNumber_Json
  {
    get => NullIf(SrsPersonNumber, "");
    set => SrsPersonNumber = value;
  }

  /// <summary>Length of the FAMILY_VIOLENCE_INDICATOR attribute.</summary>
  public const int FamilyViolenceIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the FAMILY_VIOLENCE_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = FamilyViolenceIndicator_MaxLength)]
  public string FamilyViolenceIndicator
  {
    get => familyViolenceIndicator ?? "";
    set => familyViolenceIndicator =
      TrimEnd(Substring(value, 1, FamilyViolenceIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the FamilyViolenceIndicator attribute.</summary>
  [JsonPropertyName("familyViolenceIndicator")]
  [Computed]
  public string FamilyViolenceIndicator_Json
  {
    get => NullIf(FamilyViolenceIndicator, "");
    set => FamilyViolenceIndicator = value;
  }

  /// <summary>Length of the PIN attribute.</summary>
  public const int Pin_MaxLength = 8;

  /// <summary>
  /// The value of the PIN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = Pin_MaxLength)]
  public string Pin
  {
    get => pin ?? "";
    set => pin = TrimEnd(Substring(value, 1, Pin_MaxLength));
  }

  /// <summary>
  /// The json value of the Pin attribute.</summary>
  [JsonPropertyName("pin")]
  [Computed]
  public string Pin_Json
  {
    get => NullIf(Pin, "");
    set => Pin = value;
  }

  /// <summary>Length of the SOURCE attribute.</summary>
  public const int Source_MaxLength = 4;

  /// <summary>
  /// The value of the SOURCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = Source_MaxLength)]
  public string Source
  {
    get => source ?? "";
    set => source = TrimEnd(Substring(value, 1, Source_MaxLength));
  }

  /// <summary>
  /// The json value of the Source attribute.</summary>
  [JsonPropertyName("source")]
  [Computed]
  public string Source_Json
  {
    get => NullIf(Source, "");
    set => Source = value;
  }

  /// <summary>Length of the FILLER attribute.</summary>
  public const int Filler_MaxLength = 96;

  /// <summary>
  /// The value of the FILLER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = Filler_MaxLength)]
  public string Filler
  {
    get => filler ?? "";
    set => filler = TrimEnd(Substring(value, 1, Filler_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler attribute.</summary>
  [JsonPropertyName("filler")]
  [Computed]
  public string Filler_Json
  {
    get => NullIf(Filler, "");
    set => Filler = value;
  }

  private string recordType;
  private string role;
  private string type1;
  private string ssn;
  private string lastName;
  private string firstName;
  private string middleInitial;
  private string suffix;
  private string gender;
  private string dateOfBirth;
  private string srsPersonNumber;
  private string familyViolenceIndicator;
  private string pin;
  private string source;
  private string filler;
}
