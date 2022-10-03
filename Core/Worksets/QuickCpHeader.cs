// The source file: QUICK_CP_HEADER, ID: 374543709, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class QuickCpHeader: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickCpHeader()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickCpHeader(QuickCpHeader that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickCpHeader Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickCpHeader that)
  {
    base.Assign(that);
    cpPersonNumber = that.cpPersonNumber;
    ncpPersonNumber = that.ncpPersonNumber;
    ncpLastName = that.ncpLastName;
    ncpMiddleInitial = that.ncpMiddleInitial;
    ncpFirstName = that.ncpFirstName;
    cpTypeCode = that.cpTypeCode;
    cpOrganizationName = that.cpOrganizationName;
    cpFirstName = that.cpFirstName;
    cpMiddleInitial = that.cpMiddleInitial;
    cpLastName = that.cpLastName;
  }

  /// <summary>Length of the CP_PERSON_NUMBER attribute.</summary>
  public const int CpPersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CP_PERSON_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CpPersonNumber_MaxLength)]
  public string CpPersonNumber
  {
    get => cpPersonNumber ?? "";
    set => cpPersonNumber =
      TrimEnd(Substring(value, 1, CpPersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CpPersonNumber attribute.</summary>
  [JsonPropertyName("cpPersonNumber")]
  [Computed]
  public string CpPersonNumber_Json
  {
    get => NullIf(CpPersonNumber, "");
    set => CpPersonNumber = value;
  }

  /// <summary>Length of the NCP_PERSON_NUMBER attribute.</summary>
  public const int NcpPersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NCP_PERSON_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = NcpPersonNumber_MaxLength)]
    
  public string NcpPersonNumber
  {
    get => ncpPersonNumber ?? "";
    set => ncpPersonNumber =
      TrimEnd(Substring(value, 1, NcpPersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpPersonNumber attribute.</summary>
  [JsonPropertyName("ncpPersonNumber")]
  [Computed]
  public string NcpPersonNumber_Json
  {
    get => NullIf(NcpPersonNumber, "");
    set => NcpPersonNumber = value;
  }

  /// <summary>Length of the NCP_LAST_NAME attribute.</summary>
  public const int NcpLastName_MaxLength = 17;

  /// <summary>
  /// The value of the NCP_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = NcpLastName_MaxLength)]
  public string NcpLastName
  {
    get => ncpLastName ?? "";
    set => ncpLastName = TrimEnd(Substring(value, 1, NcpLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpLastName attribute.</summary>
  [JsonPropertyName("ncpLastName")]
  [Computed]
  public string NcpLastName_Json
  {
    get => NullIf(NcpLastName, "");
    set => NcpLastName = value;
  }

  /// <summary>Length of the NCP_MIDDLE_INITIAL attribute.</summary>
  public const int NcpMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_MIDDLE_INITIAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = NcpMiddleInitial_MaxLength)
    ]
  public string NcpMiddleInitial
  {
    get => ncpMiddleInitial ?? "";
    set => ncpMiddleInitial =
      TrimEnd(Substring(value, 1, NcpMiddleInitial_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpMiddleInitial attribute.</summary>
  [JsonPropertyName("ncpMiddleInitial")]
  [Computed]
  public string NcpMiddleInitial_Json
  {
    get => NullIf(NcpMiddleInitial, "");
    set => NcpMiddleInitial = value;
  }

  /// <summary>Length of the NCP_FIRST_NAME attribute.</summary>
  public const int NcpFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the NCP_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = NcpFirstName_MaxLength)]
  public string NcpFirstName
  {
    get => ncpFirstName ?? "";
    set => ncpFirstName = TrimEnd(Substring(value, 1, NcpFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpFirstName attribute.</summary>
  [JsonPropertyName("ncpFirstName")]
  [Computed]
  public string NcpFirstName_Json
  {
    get => NullIf(NcpFirstName, "");
    set => NcpFirstName = value;
  }

  /// <summary>Length of the CP_TYPE_CODE attribute.</summary>
  public const int CpTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the CP_TYPE_CODE attribute.
  /// C or O -   C for Person and O for Organization
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CpTypeCode_MaxLength)]
  public string CpTypeCode
  {
    get => cpTypeCode ?? "";
    set => cpTypeCode = TrimEnd(Substring(value, 1, CpTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the CpTypeCode attribute.</summary>
  [JsonPropertyName("cpTypeCode")]
  [Computed]
  public string CpTypeCode_Json
  {
    get => NullIf(CpTypeCode, "");
    set => CpTypeCode = value;
  }

  /// <summary>Length of the CP_ORGANIZATION_NAME attribute.</summary>
  public const int CpOrganizationName_MaxLength = 33;

  /// <summary>
  /// The value of the CP_ORGANIZATION_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = CpOrganizationName_MaxLength)]
  public string CpOrganizationName
  {
    get => cpOrganizationName ?? "";
    set => cpOrganizationName =
      TrimEnd(Substring(value, 1, CpOrganizationName_MaxLength));
  }

  /// <summary>
  /// The json value of the CpOrganizationName attribute.</summary>
  [JsonPropertyName("cpOrganizationName")]
  [Computed]
  public string CpOrganizationName_Json
  {
    get => NullIf(CpOrganizationName, "");
    set => CpOrganizationName = value;
  }

  /// <summary>Length of the CP_FIRST_NAME attribute.</summary>
  public const int CpFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the CP_FIRST_NAME attribute.
  /// CSE Person first name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CpFirstName_MaxLength)]
  public string CpFirstName
  {
    get => cpFirstName ?? "";
    set => cpFirstName = TrimEnd(Substring(value, 1, CpFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the CpFirstName attribute.</summary>
  [JsonPropertyName("cpFirstName")]
  [Computed]
  public string CpFirstName_Json
  {
    get => NullIf(CpFirstName, "");
    set => CpFirstName = value;
  }

  /// <summary>Length of the CP_MIDDLE_INITIAL attribute.</summary>
  public const int CpMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the CP_MIDDLE_INITIAL attribute.
  /// CSE Person middle initial
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CpMiddleInitial_MaxLength)]
    
  public string CpMiddleInitial
  {
    get => cpMiddleInitial ?? "";
    set => cpMiddleInitial =
      TrimEnd(Substring(value, 1, CpMiddleInitial_MaxLength));
  }

  /// <summary>
  /// The json value of the CpMiddleInitial attribute.</summary>
  [JsonPropertyName("cpMiddleInitial")]
  [Computed]
  public string CpMiddleInitial_Json
  {
    get => NullIf(CpMiddleInitial, "");
    set => CpMiddleInitial = value;
  }

  /// <summary>Length of the CP_LAST_NAME attribute.</summary>
  public const int CpLastName_MaxLength = 17;

  /// <summary>
  /// The value of the CP_LAST_NAME attribute.
  /// CSE Person last name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = CpLastName_MaxLength)]
  public string CpLastName
  {
    get => cpLastName ?? "";
    set => cpLastName = TrimEnd(Substring(value, 1, CpLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the CpLastName attribute.</summary>
  [JsonPropertyName("cpLastName")]
  [Computed]
  public string CpLastName_Json
  {
    get => NullIf(CpLastName, "");
    set => CpLastName = value;
  }

  private string cpPersonNumber;
  private string ncpPersonNumber;
  private string ncpLastName;
  private string ncpMiddleInitial;
  private string ncpFirstName;
  private string cpTypeCode;
  private string cpOrganizationName;
  private string cpFirstName;
  private string cpMiddleInitial;
  private string cpLastName;
}
