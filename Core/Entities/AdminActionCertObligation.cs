// The source file: ADMIN_ACTION_CERT_OBLIGATION, ID: 371430164, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC	
/// This entity type is used to resolve a many-to-many relationship between 
/// OBLIGATION and ADMINISTRATIVE_ACTION_CERTIFICATION.
/// </summary>
[Serializable]
public partial class AdminActionCertObligation: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AdminActionCertObligation()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AdminActionCertObligation(AdminActionCertObligation that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AdminActionCertObligation Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AdminActionCertObligation that)
  {
    base.Assign(that);
    timestamp = that.timestamp;
    otyType = that.otyType;
    obgGeneratedId = that.obgGeneratedId;
    cspCNumber = that.cspCNumber;
    cpaCType = that.cpaCType;
    aacTanfCode = that.aacTanfCode;
    aacTakenDate = that.aacTakenDate;
    cpaType = that.cpaType;
    aacType = that.aacType;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the TIMESTAMP attribute.
  /// This is the TimeStamp that the Entity Type is created.
  /// </summary>
  [JsonPropertyName("timestamp")]
  [Member(Index = 1, Type = MemberType.Timestamp)]
  public DateTime? Timestamp
  {
    get => timestamp;
    set => timestamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyType")]
  [Member(Index = 2, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OtyType
  {
    get => otyType;
    set => otyType = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgGeneratedId")]
  [Member(Index = 3, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ObgGeneratedId
  {
    get => obgGeneratedId;
    set => obgGeneratedId = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspCNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspCNumber")]
  [Member(Index = 4, Type = MemberType.Char, Length = CspCNumber_MaxLength, Optional
    = true)]
  public string CspCNumber
  {
    get => cspCNumber;
    set => cspCNumber = value != null
      ? TrimEnd(Substring(value, 1, CspCNumber_MaxLength)) : null;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaCType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonPropertyName("cpaCType")]
  [Member(Index = 5, Type = MemberType.Char, Length = CpaCType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaCType
  {
    get => cpaCType;
    set => cpaCType = value != null
      ? TrimEnd(Substring(value, 1, CpaCType_MaxLength)) : null;
  }

  /// <summary>Length of the TANF_CODE attribute.</summary>
  public const int AacTanfCode_MaxLength = 1;

  /// <summary>
  /// The value of the TANF_CODE attribute.
  /// Code used to identify TANF or non-TANF.                                 T 
  /// - TANF
  /// 
  /// N - Non-TANF
  /// 
  /// Space - Not Seperated by TANF (Default)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = AacTanfCode_MaxLength)]
  public string AacTanfCode
  {
    get => aacTanfCode ?? "";
    set => aacTanfCode = TrimEnd(Substring(value, 1, AacTanfCode_MaxLength));
  }

  /// <summary>
  /// The json value of the AacTanfCode attribute.</summary>
  [JsonPropertyName("aacTanfCode")]
  [Computed]
  public string AacTanfCode_Json
  {
    get => NullIf(AacTanfCode, "");
    set => AacTanfCode = value;
  }

  /// <summary>
  /// The value of the TAKEN_DATE attribute.
  /// This is the date the enforcement action is taken for a particular 
  /// Obligation.
  /// </summary>
  [JsonPropertyName("aacTakenDate")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? AacTakenDate
  {
    get => aacTakenDate;
    set => aacTakenDate = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CpaType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => cpaType ?? "";
    set => cpaType = TrimEnd(Substring(value, 1, CpaType_MaxLength));
  }

  /// <summary>
  /// The json value of the CpaType attribute.</summary>
  [JsonPropertyName("cpaType")]
  [Computed]
  public string CpaType_Json
  {
    get => NullIf(CpaType, "");
    set => CpaType = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int AacType_MaxLength = 4;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of certified enforcement action taken.
  /// They can be: FDSO
  ///              SDSO
  ///              CRED
  ///              RECA
  ///              IRS
  ///              KSMW
  /// 
  /// KDWP
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = AacType_MaxLength)]
  [Value("KDWP")]
  [Value("KDMV")]
  [Value("FDSO")]
  [Value("IRSC")]
  [Value("COAG")]
  [Value("CRED")]
  [Value("SDSO")]
  [Value("KSMW")]
  public string AacType
  {
    get => aacType ?? "";
    set => aacType = TrimEnd(Substring(value, 1, AacType_MaxLength));
  }

  /// <summary>
  /// The json value of the AacType attribute.</summary>
  [JsonPropertyName("aacType")]
  [Computed]
  public string AacType_Json
  {
    get => NullIf(AacType, "");
    set => AacType = value;
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
  [Member(Index = 10, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  private DateTime? timestamp;
  private int? otyType;
  private int? obgGeneratedId;
  private string cspCNumber;
  private string cpaCType;
  private string aacTanfCode;
  private DateTime? aacTakenDate;
  private string cpaType;
  private string aacType;
  private string cspNumber;
}
