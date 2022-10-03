// The source file: OBLIGATION_TRANSACTION_RLN, ID: 371438282, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This defines the relationship between financial transactions.
/// Examples: Accrual creates debt, adjustment modifies debt, etc.
/// </summary>
[Serializable]
public partial class ObligationTransactionRln: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ObligationTransactionRln()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ObligationTransactionRln(ObligationTransactionRln that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ObligationTransactionRln Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ObligationTransactionRln that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    description = that.description;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    otyTypePrimary = that.otyTypePrimary;
    otrPType = that.otrPType;
    otrPGeneratedId = that.otrPGeneratedId;
    cpaPType = that.cpaPType;
    cspPNumber = that.cspPNumber;
    obgPGeneratedId = that.obgPGeneratedId;
    otyTypeSecondary = that.otyTypeSecondary;
    otrType = that.otrType;
    otrGeneratedId = that.otrGeneratedId;
    cpaType = that.cpaType;
    cspNumber = that.cspNumber;
    obgGeneratedId = that.obgGeneratedId;
    onrGeneratedId = that.onrGeneratedId;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number that distinguishes one occurrence
  /// of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// Represent the description of the reason for relating two transactions 
  /// together.
  /// Example: Collection &quot;satisifies&quot; debt - where &quot;satisifies
  /// &quot; respresents the reason.  This may have a description of &quot;
  /// received from AP's mother&quot;.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 2, Type = MemberType.Varchar, Length
    = Description_MaxLength, Optional = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? Substring(value, 1, Description_MaxLength) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TMST attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 6, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyTypePrimary")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int OtyTypePrimary
  {
    get => otyTypePrimary;
    set => otyTypePrimary = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int OtrPType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the obligation transaction type.
  /// Permitted Values:
  /// 	- DE = DEBT
  /// 	- DA = DEBT ADJUSTMENT
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = OtrPType_MaxLength)]
  [Value("DE")]
  [Value("Z2")]
  [Value("DA")]
  [ImplicitValue("DE")]
  public string OtrPType
  {
    get => otrPType ?? "";
    set => otrPType = TrimEnd(Substring(value, 1, OtrPType_MaxLength));
  }

  /// <summary>
  /// The json value of the OtrPType attribute.</summary>
  [JsonPropertyName("otrPType")]
  [Computed]
  public string OtrPType_Json
  {
    get => NullIf(OtrPType, "");
    set => OtrPType = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("otrPGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 9)]
  public int OtrPGeneratedId
  {
    get => otrPGeneratedId;
    set => otrPGeneratedId = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaPType_MaxLength = 1;

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
  [Member(Index = 10, Type = MemberType.Char, Length = CpaPType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaPType
  {
    get => cpaPType ?? "";
    set => cpaPType = TrimEnd(Substring(value, 1, CpaPType_MaxLength));
  }

  /// <summary>
  /// The json value of the CpaPType attribute.</summary>
  [JsonPropertyName("cpaPType")]
  [Computed]
  public string CpaPType_Json
  {
    get => NullIf(CpaPType, "");
    set => CpaPType = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspPNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CspPNumber_MaxLength)]
  public string CspPNumber
  {
    get => cspPNumber ?? "";
    set => cspPNumber = TrimEnd(Substring(value, 1, CspPNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspPNumber attribute.</summary>
  [JsonPropertyName("cspPNumber")]
  [Computed]
  public string CspPNumber_Json
  {
    get => NullIf(CspPNumber, "");
    set => CspPNumber = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgPGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 3)]
  public int ObgPGeneratedId
  {
    get => obgPGeneratedId;
    set => obgPGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyTypeSecondary")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 3)]
  public int OtyTypeSecondary
  {
    get => otyTypeSecondary;
    set => otyTypeSecondary = value;
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
  [Member(Index = 14, Type = MemberType.Char, Length = OtrType_MaxLength)]
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
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("otrGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 9)]
  public int OtrGeneratedId
  {
    get => otrGeneratedId;
    set => otrGeneratedId = value;
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
  [JsonPropertyName("obgGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 18, Type = MemberType.Number, Length = 3)]
  public int ObgGeneratedId
  {
    get => obgGeneratedId;
    set => obgGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("onrGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 19, Type = MemberType.Number, Length = 3)]
  public int OnrGeneratedId
  {
    get => onrGeneratedId;
    set => onrGeneratedId = value;
  }

  private int systemGeneratedIdentifier;
  private string description;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private int otyTypePrimary;
  private string otrPType;
  private int otrPGeneratedId;
  private string cpaPType;
  private string cspPNumber;
  private int obgPGeneratedId;
  private int otyTypeSecondary;
  private string otrType;
  private int otrGeneratedId;
  private string cpaType;
  private string cspNumber;
  private int obgGeneratedId;
  private int onrGeneratedId;
}
