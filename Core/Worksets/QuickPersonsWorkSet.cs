// The source file: QUICK_PERSONS_WORK_SET, ID: 374543786, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class QuickPersonsWorkSet: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickPersonsWorkSet()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickPersonsWorkSet(QuickPersonsWorkSet that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickPersonsWorkSet Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickPersonsWorkSet that)
  {
    base.Assign(that);
    organizationName = that.organizationName;
    roleType = that.roleType;
    firstName = that.firstName;
    middleInitial = that.middleInitial;
    lastName = that.lastName;
    dob = that.dob;
    ssn = that.ssn;
    fvi = that.fvi;
  }

  /// <summary>Length of the ORGANIZATION_NAME attribute.</summary>
  public const int OrganizationName_MaxLength = 33;

  /// <summary>
  /// The value of the ORGANIZATION_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = OrganizationName_MaxLength)
    ]
  public string OrganizationName
  {
    get => organizationName ?? "";
    set => organizationName =
      TrimEnd(Substring(value, 1, OrganizationName_MaxLength));
  }

  /// <summary>
  /// The json value of the OrganizationName attribute.</summary>
  [JsonPropertyName("organizationName")]
  [Computed]
  public string OrganizationName_Json
  {
    get => NullIf(OrganizationName, "");
    set => OrganizationName = value;
  }

  /// <summary>Length of the ROLE_TYPE attribute.</summary>
  public const int RoleType_MaxLength = 5;

  /// <summary>
  /// The value of the ROLE_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = RoleType_MaxLength)]
  public string RoleType
  {
    get => roleType ?? "";
    set => roleType = TrimEnd(Substring(value, 1, RoleType_MaxLength));
  }

  /// <summary>
  /// The json value of the RoleType attribute.</summary>
  [JsonPropertyName("roleType")]
  [Computed]
  public string RoleType_Json
  {
    get => NullIf(RoleType, "");
    set => RoleType = value;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 12;

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

  /// <summary>Length of the MIDDLE_INITIAL attribute.</summary>
  public const int MiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INITIAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = MiddleInitial_MaxLength)]
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

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

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

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Ssn_MaxLength)]
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

  /// <summary>Length of the FVI attribute.</summary>
  public const int Fvi_MaxLength = 3;

  /// <summary>
  /// The value of the FVI attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = Fvi_MaxLength)]
  public string Fvi
  {
    get => fvi ?? "";
    set => fvi = TrimEnd(Substring(value, 1, Fvi_MaxLength));
  }

  /// <summary>
  /// The json value of the Fvi attribute.</summary>
  [JsonPropertyName("fvi")]
  [Computed]
  public string Fvi_Json
  {
    get => NullIf(Fvi, "");
    set => Fvi = value;
  }

  private string organizationName;
  private string roleType;
  private string firstName;
  private string middleInitial;
  private string lastName;
  private string dob;
  private string ssn;
  private string fvi;
}
