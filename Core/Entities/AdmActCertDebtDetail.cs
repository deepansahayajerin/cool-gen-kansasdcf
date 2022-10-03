// The source file: ADM_ACT_CERT_DEBT_DETAIL, ID: 371430149, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLEN
/// This enity captures the debts certified for an administrative action like 
/// FDSO, SDSO, etc. This resolved the many-to-many relationship between
/// DEBT_DETAIL and ADMINISTRATIVE ACT CERTIFICATION entities.
/// </summary>
[Serializable]
public partial class AdmActCertDebtDetail: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AdmActCertDebtDetail()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AdmActCertDebtDetail(AdmActCertDebtDetail that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AdmActCertDebtDetail Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AdmActCertDebtDetail that)
  {
    base.Assign(that);
    amtCertifiedBalance = that.amtCertifiedBalance;
    amtCertifiedInterest = that.amtCertifiedInterest;
    referInd = that.referInd;
    updatedWorkerId = that.updatedWorkerId;
    updatedDate = that.updatedDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    aacTakenDate = that.aacTakenDate;
    aacType = that.aacType;
    aacTanfCode = that.aacTanfCode;
    cspNumberDebt = that.cspNumberDebt;
    cpaTypeDebt = that.cpaTypeDebt;
    otrType = that.otrType;
    otyId = that.otyId;
    otrId = that.otrId;
    cpaType = that.cpaType;
    cspNumber = that.cspNumber;
    obgId = that.obgId;
  }

  /// <summary>
  /// The value of the AMT_CERTIFIED_BALANCE attribute.
  /// This attribute specifies the balance amount cerified for the 
  /// administrative action.
  /// </summary>
  [JsonPropertyName("amtCertifiedBalance")]
  [Member(Index = 1, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AmtCertifiedBalance
  {
    get => amtCertifiedBalance;
    set => amtCertifiedBalance = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the AMT_CERTIFIED_INTEREST attribute.
  /// This attribute specifies the interest amount certified for the 
  /// administrative action.
  /// </summary>
  [JsonPropertyName("amtCertifiedInterest")]
  [Member(Index = 2, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? AmtCertifiedInterest
  {
    get => amtCertifiedInterest;
    set => amtCertifiedInterest = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the REFER_IND attribute.</summary>
  public const int ReferInd_MaxLength = 1;

  /// <summary>
  /// The value of the REFER_IND attribute.
  /// This attribute specifies whether or not the debt must be certified. By 
  /// default it will be created as &quot;Y&quot;.  However, the CSE Worker can
  /// change it to &quot;N&quot;.  This facility is available only for
  /// Collection Agency Referral now.
  /// 	Y   the debt must be certified for the administrative action.	
  /// 	N   The debt must not be certified for the administrative action.
  /// </summary>
  [JsonPropertyName("referInd")]
  [Member(Index = 3, Type = MemberType.Char, Length = ReferInd_MaxLength, Optional
    = true)]
  public string ReferInd
  {
    get => referInd;
    set => referInd = value != null
      ? TrimEnd(Substring(value, 1, ReferInd_MaxLength)) : null;
  }

  /// <summary>Length of the UPDATED_WORKER_ID attribute.</summary>
  public const int UpdatedWorkerId_MaxLength = 8;

  /// <summary>
  /// The value of the UPDATED_WORKER_ID attribute.
  /// This attribute specifies the logon userid of the worker who updated 
  /// REFER_IND to &quot;N&quot;.
  /// </summary>
  [JsonPropertyName("updatedWorkerId")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = UpdatedWorkerId_MaxLength, Optional = true)]
  public string UpdatedWorkerId
  {
    get => updatedWorkerId;
    set => updatedWorkerId = value != null
      ? TrimEnd(Substring(value, 1, UpdatedWorkerId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the UPDATED_DATE attribute.
  /// This attribute specifies the date the CSE Worker updated REFER_IND to 
  /// &quot;N&quot;.
  /// </summary>
  [JsonPropertyName("updatedDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? UpdatedDate
  {
    get => updatedDate;
    set => updatedDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The userid of the person who added this entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The date/time as to when this entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the TAKEN_DATE attribute.
  /// This is the date the enforcement action is taken for a particular 
  /// Obligation.
  /// </summary>
  [JsonPropertyName("aacTakenDate")]
  [Member(Index = 8, Type = MemberType.Date)]
  public DateTime? AacTakenDate
  {
    get => aacTakenDate;
    set => aacTakenDate = value;
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
  [Member(Index = 10, Type = MemberType.Char, Length = AacTanfCode_MaxLength)]
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumberDebt_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CspNumberDebt_MaxLength)]
  public string CspNumberDebt
  {
    get => cspNumberDebt ?? "";
    set => cspNumberDebt =
      TrimEnd(Substring(value, 1, CspNumberDebt_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumberDebt attribute.</summary>
  [JsonPropertyName("cspNumberDebt")]
  [Computed]
  public string CspNumberDebt_Json
  {
    get => NullIf(CspNumberDebt, "");
    set => CspNumberDebt = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaTypeDebt_MaxLength = 1;

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
  [Member(Index = 12, Type = MemberType.Char, Length = CpaTypeDebt_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaTypeDebt
  {
    get => cpaTypeDebt ?? "";
    set => cpaTypeDebt = TrimEnd(Substring(value, 1, CpaTypeDebt_MaxLength));
  }

  /// <summary>
  /// The json value of the CpaTypeDebt attribute.</summary>
  [JsonPropertyName("cpaTypeDebt")]
  [Computed]
  public string CpaTypeDebt_Json
  {
    get => NullIf(CpaTypeDebt, "");
    set => CpaTypeDebt = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int OtrType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the obligation transaction type.
  /// Permitted Values:
  /// 	- DE = DEBT
  /// 	- DA = DEBT ADJUSTMENT
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = OtrType_MaxLength)]
  [Value("DE")]
  [Value("Z2")]
  [Value("DA")]
  [ImplicitValue("DE")]
  public string OtrType
  {
    get => otrType ?? "";
    set => otrType = TrimEnd(Substring(value, 1, OtrType_MaxLength));
  }

  /// <summary>
  /// The json value of the OtrType attribute.</summary>
  [JsonPropertyName("otrType")]
  [Computed]
  public string OtrType_Json
  {
    get => NullIf(OtrType, "");
    set => OtrType = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyId")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 3)]
  public int OtyId
  {
    get => otyId;
    set => otyId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("otrId")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 9)]
  public int OtrId
  {
    get => otrId;
    set => otrId = value;
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
  [Member(Index = 16, Type = MemberType.Char, Length = CpaType_MaxLength)]
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgId")]
  [DefaultValue(0)]
  [Member(Index = 18, Type = MemberType.Number, Length = 3)]
  public int ObgId
  {
    get => obgId;
    set => obgId = value;
  }

  private decimal? amtCertifiedBalance;
  private decimal? amtCertifiedInterest;
  private string referInd;
  private string updatedWorkerId;
  private DateTime? updatedDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private DateTime? aacTakenDate;
  private string aacType;
  private string aacTanfCode;
  private string cspNumberDebt;
  private string cpaTypeDebt;
  private string otrType;
  private int otyId;
  private int otrId;
  private string cpaType;
  private string cspNumber;
  private int obgId;
}
