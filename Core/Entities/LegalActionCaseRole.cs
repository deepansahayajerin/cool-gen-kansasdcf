// The source file: LEGAL_ACTION_CASE_ROLE, ID: 371436738, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// This entity resolves the many-to-many relationship between LEGAL_ACTION and 
/// CASE_ROLE.
/// </summary>
[Serializable]
public partial class LegalActionCaseRole: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LegalActionCaseRole()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LegalActionCaseRole(LegalActionCaseRole that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LegalActionCaseRole Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LegalActionCaseRole that)
  {
    base.Assign(that);
    initialCreationInd = that.initialCreationInd;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    croIdentifier = that.croIdentifier;
    croType = that.croType;
    cspNumber = that.cspNumber;
    casNumber = that.casNumber;
    lgaId = that.lgaId;
  }

  /// <summary>Length of the INITIAL_CREATION_IND attribute.</summary>
  public const int InitialCreationInd_MaxLength = 1;

  /// <summary>
  /// The value of the INITIAL_CREATION_IND attribute.
  /// This attribute identifies whether the Legal Action Case Role occurrence 
  /// was created at the time Legal Action/ Legal Action Detail was created OR
  /// it was created subsequently due to a change in the case case.
  /// It will be Y if it was created initially by LROL/ LOPS online procedures. 
  /// It will be N if it was created by other processes in response to a change
  /// in the cse case.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = InitialCreationInd_MaxLength)]
  public string InitialCreationInd
  {
    get => initialCreationInd ?? "";
    set => initialCreationInd =
      TrimEnd(Substring(value, 1, InitialCreationInd_MaxLength));
  }

  /// <summary>
  /// The json value of the InitialCreationInd attribute.</summary>
  [JsonPropertyName("initialCreationInd")]
  [Computed]
  public string InitialCreationInd_Json
  {
    get => NullIf(InitialCreationInd, "");
    set => InitialCreationInd = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier.  System generated using a 3-digit random generator
  /// </summary>
  [JsonPropertyName("croIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 3)]
  public int CroIdentifier
  {
    get => croIdentifier;
    set => croIdentifier = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CroType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of role played by a CSE Person in a given case.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CroType_MaxLength)]
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CasNumber_MaxLength)]
  public string CasNumber
  {
    get => casNumber ?? "";
    set => casNumber = TrimEnd(Substring(value, 1, CasNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CasNumber attribute.</summary>
  [JsonPropertyName("casNumber")]
  [Computed]
  public string CasNumber_Json
  {
    get => NullIf(CasNumber, "");
    set => CasNumber = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaId")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 9)]
  public int LgaId
  {
    get => lgaId;
    set => lgaId = value;
  }

  private string initialCreationInd;
  private string createdBy;
  private DateTime? createdTstamp;
  private int croIdentifier;
  private string croType;
  private string cspNumber;
  private string casNumber;
  private int lgaId;
}
