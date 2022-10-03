// The source file: CSE_PERSONS_WORK_SET, ID: 371423417, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: SRVINIT
/// This contains data about a CSE PERSON held on the ADABAS files.
/// </summary>
[Serializable]
public partial class CsePersonsWorkSet: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsePersonsWorkSet()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsePersonsWorkSet(CsePersonsWorkSet that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsePersonsWorkSet Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CsePersonsWorkSet that)
  {
    base.Assign(that);
    replicationIndicator = that.replicationIndicator;
    uniqueKey = that.uniqueKey;
    number = that.number;
    sex = that.sex;
    dob = that.dob;
    ssn = that.ssn;
    firstName = that.firstName;
    middleInitial = that.middleInitial;
    formattedName = that.formattedName;
    flag = that.flag;
    char10 = that.char10;
    char2 = that.char2;
    lastName = that.lastName;
  }

  /// <summary>Length of the REPLICATION_INDICATOR attribute.</summary>
  public const int ReplicationIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the REPLICATION_INDICATOR attribute.
  /// replication indicator for III ECP to show if person has been indicated as 
  /// P primary   S secondary or blank for not yet set
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = ReplicationIndicator_MaxLength)]
  public string ReplicationIndicator
  {
    get => replicationIndicator ?? "";
    set => replicationIndicator =
      TrimEnd(Substring(value, 1, ReplicationIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the ReplicationIndicator attribute.</summary>
  [JsonPropertyName("replicationIndicator")]
  [Computed]
  public string ReplicationIndicator_Json
  {
    get => NullIf(ReplicationIndicator, "");
    set => ReplicationIndicator = value;
  }

  /// <summary>Length of the UNIQUE_KEY attribute.</summary>
  public const int UniqueKey_MaxLength = 100;

  /// <summary>
  /// The value of the UNIQUE_KEY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = UniqueKey_MaxLength)]
  public string UniqueKey
  {
    get => uniqueKey ?? "";
    set => uniqueKey = TrimEnd(Substring(value, 1, UniqueKey_MaxLength));
  }

  /// <summary>
  /// The json value of the UniqueKey attribute.</summary>
  [JsonPropertyName("uniqueKey")]
  [Computed]
  public string UniqueKey_Json
  {
    get => NullIf(UniqueKey, "");
    set => UniqueKey = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int Number_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This replaces the CSE_PERSON Number.		
  /// This information will be retrieved from the ADABAS files and displayed on 
  /// KESSEP screens using this field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Number_MaxLength)]
  public string Number
  {
    get => number ?? "";
    set => number = TrimEnd(Substring(value, 1, Number_MaxLength));
  }

  /// <summary>
  /// The json value of the Number attribute.</summary>
  [JsonPropertyName("number")]
  [Computed]
  public string Number_Json
  {
    get => NullIf(Number, "");
    set => Number = value;
  }

  /// <summary>Length of the SEX attribute.</summary>
  public const int Sex_MaxLength = 1;

  /// <summary>
  /// The value of the SEX attribute.
  /// This field replaces the CSE_Person SEX.
  /// The information will be retrieved from the ADABAS files and displayed on 
  /// KESSEP screens using this field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Sex_MaxLength)]
  public string Sex
  {
    get => sex ?? "";
    set => sex = TrimEnd(Substring(value, 1, Sex_MaxLength));
  }

  /// <summary>
  /// The json value of the Sex attribute.</summary>
  [JsonPropertyName("sex")]
  [Computed]
  public string Sex_Json
  {
    get => NullIf(Sex, "");
    set => Sex = value;
  }

  /// <summary>
  /// The value of the DOB attribute.
  /// This field replaces the CSE_Person Date_Of_Birth.
  /// The information will be retrieved from the ADABAS files and displayed on 
  /// KESSEP screens using this field.
  /// </summary>
  [JsonPropertyName("dob")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? Dob
  {
    get => dob;
    set => dob = value;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// This field replaces the CSE_Person SSN field.
  /// This information will be retrieved from the ADABAS files and displayed on 
  /// KESSEP screens using this field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Ssn_MaxLength)]
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

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = FirstName_MaxLength)]
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
  [Member(Index = 8, Type = MemberType.Char, Length = MiddleInitial_MaxLength)]
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

  /// <summary>Length of the FORMATTED_NAME attribute.</summary>
  public const int FormattedName_MaxLength = 33;

  /// <summary>
  /// The value of the FORMATTED_NAME attribute.
  /// This is used by the format cse person name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = FormattedName_MaxLength)]
  public string FormattedName
  {
    get => formattedName ?? "";
    set => formattedName =
      TrimEnd(Substring(value, 1, FormattedName_MaxLength));
  }

  /// <summary>
  /// The json value of the FormattedName attribute.</summary>
  [JsonPropertyName("formattedName")]
  [Computed]
  public string FormattedName_Json
  {
    get => NullIf(FormattedName, "");
    set => FormattedName = value;
  }

  /// <summary>Length of the FLAG attribute.</summary>
  public const int Flag_MaxLength = 1;

  /// <summary>
  /// The value of the FLAG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = Flag_MaxLength)]
  public string Flag
  {
    get => flag ?? "";
    set => flag = TrimEnd(Substring(value, 1, Flag_MaxLength));
  }

  /// <summary>
  /// The json value of the Flag attribute.</summary>
  [JsonPropertyName("flag")]
  [Computed]
  public string Flag_Json
  {
    get => NullIf(Flag, "");
    set => Flag = value;
  }

  /// <summary>Length of the CHAR_10 attribute.</summary>
  public const int Char10_MaxLength = 10;

  /// <summary>
  /// The value of the CHAR_10 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Char10_MaxLength)]
  public string Char10
  {
    get => char10 ?? "";
    set => char10 = TrimEnd(Substring(value, 1, Char10_MaxLength));
  }

  /// <summary>
  /// The json value of the Char10 attribute.</summary>
  [JsonPropertyName("char10")]
  [Computed]
  public string Char10_Json
  {
    get => NullIf(Char10, "");
    set => Char10 = value;
  }

  /// <summary>Length of the CHAR_2 attribute.</summary>
  public const int Char2_MaxLength = 2;

  /// <summary>
  /// The value of the CHAR_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = Char2_MaxLength)]
  public string Char2
  {
    get => char2 ?? "";
    set => char2 = TrimEnd(Substring(value, 1, Char2_MaxLength));
  }

  /// <summary>
  /// The json value of the Char2 attribute.</summary>
  [JsonPropertyName("char2")]
  [Computed]
  public string Char2_Json
  {
    get => NullIf(Char2, "");
    set => Char2 = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = LastName_MaxLength)]
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

  private string replicationIndicator;
  private string uniqueKey;
  private string number;
  private string sex;
  private DateTime? dob;
  private string ssn;
  private string firstName;
  private string middleInitial;
  private string formattedName;
  private string flag;
  private string char10;
  private string char2;
  private string lastName;
}
