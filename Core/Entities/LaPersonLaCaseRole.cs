// The source file: LA_PERSON_LA_CASE_ROLE, ID: 371436710, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// This entity resolves the many-to-many relationship between LEGAL ACTION CASE
/// ROLE. It relates the legal action person to a cse case role via Legal
/// Action Case Role.  This entity identifies the legal action case roles
/// created by LROL and LOPS separately.
/// </summary>
[Serializable]
public partial class LaPersonLaCaseRole: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LaPersonLaCaseRole()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LaPersonLaCaseRole(LaPersonLaCaseRole that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LaPersonLaCaseRole Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LaPersonLaCaseRole that)
  {
    base.Assign(that);
    identifier = that.identifier;
    lgaId = that.lgaId;
    casNum = that.casNum;
    croId = that.croId;
    cspNum = that.cspNum;
    croType = that.croType;
    lapId = that.lapId;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute along with the relationships with Legal Action Person and 
  /// Legal Action Case Role will uniquely identify one occurrence of this
  /// entity type. This is a system generated identifier starting from 1 for
  /// each Legal Action Person occurrence.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaId")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int LgaId
  {
    get => lgaId;
    set => lgaId = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNum_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CasNum_MaxLength)]
  public string CasNum
  {
    get => casNum ?? "";
    set => casNum = TrimEnd(Substring(value, 1, CasNum_MaxLength));
  }

  /// <summary>
  /// The json value of the CasNum attribute.</summary>
  [JsonPropertyName("casNum")]
  [Computed]
  public string CasNum_Json
  {
    get => NullIf(CasNum, "");
    set => CasNum = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier.  System generated using a 3-digit random generator
  /// </summary>
  [JsonPropertyName("croId")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 3)]
  public int CroId
  {
    get => croId;
    set => croId = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNum_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CspNum_MaxLength)]
  public string CspNum
  {
    get => cspNum ?? "";
    set => cspNum = TrimEnd(Substring(value, 1, CspNum_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNum attribute.</summary>
  [JsonPropertyName("cspNum")]
  [Computed]
  public string CspNum_Json
  {
    get => NullIf(CspNum, "");
    set => CspNum = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CroType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of role played by a CSE Person in a given case.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CroType_MaxLength)]
  [Value("MO")]
  [Value("FA")]
  [Value("CH")]
  [Value("AR")]
  [Value("AP")]
  public string CroType
  {
    get => croType ?? "";
    set => croType = TrimEnd(Substring(value, 1, CroType_MaxLength));
  }

  /// <summary>
  /// The json value of the CroType attribute.</summary>
  [JsonPropertyName("croType")]
  [Computed]
  public string CroType_Json
  {
    get => NullIf(CroType, "");
    set => CroType = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("lapId")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 9)]
  public int LapId
  {
    get => lapId;
    set => lapId = value;
  }

  private int identifier;
  private int lgaId;
  private string casNum;
  private int croId;
  private string cspNum;
  private string croType;
  private int lapId;
}
