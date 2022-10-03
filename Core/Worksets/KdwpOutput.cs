// The source file: KDWP_OUTPUT, ID: 371325869, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class KdwpOutput: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public KdwpOutput()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public KdwpOutput(KdwpOutput that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new KdwpOutput Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(KdwpOutput that)
  {
    base.Assign(that);
    csePersonNumber = that.csePersonNumber;
    lastName = that.lastName;
    firstName = that.firstName;
    middleName = that.middleName;
    ssn = that.ssn;
    dob = that.dob;
    aliasInd = that.aliasInd;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CsePersonNumber_MaxLength)]
    
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
  public const int LastName_MaxLength = 30;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = LastName_MaxLength)]
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
  public const int FirstName_MaxLength = 16;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = FirstName_MaxLength)]
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

  /// <summary>Length of the MIDDLE_NAME attribute.</summary>
  public const int MiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = MiddleName_MaxLength)]
  public string MiddleName
  {
    get => middleName ?? "";
    set => middleName = TrimEnd(Substring(value, 1, MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the MiddleName attribute.</summary>
  [JsonPropertyName("middleName")]
  [Computed]
  public string MiddleName_Json
  {
    get => NullIf(MiddleName, "");
    set => MiddleName = value;
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

  /// <summary>Length of the ALIAS_IND attribute.</summary>
  public const int AliasInd_MaxLength = 1;

  /// <summary>
  /// The value of the ALIAS_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = AliasInd_MaxLength)]
  public string AliasInd
  {
    get => aliasInd ?? "";
    set => aliasInd = TrimEnd(Substring(value, 1, AliasInd_MaxLength));
  }

  /// <summary>
  /// The json value of the AliasInd attribute.</summary>
  [JsonPropertyName("aliasInd")]
  [Computed]
  public string AliasInd_Json
  {
    get => NullIf(AliasInd, "");
    set => AliasInd = value;
  }

  private string csePersonNumber;
  private string lastName;
  private string firstName;
  private string middleName;
  private string ssn;
  private string dob;
  private string aliasInd;
}
