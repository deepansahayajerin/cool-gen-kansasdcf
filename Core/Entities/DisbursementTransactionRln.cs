// The source file: DISBURSEMENT_TRANSACTION_RLN, ID: 371433766, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This entity contains the information needed to capture the information about
/// the relationship between two transactions.  The business reason for the
/// relationship is recorded thru the relationship to
/// Disbursement_Transaction_RLN_RSN.
/// For Example:
/// A Disbursement Fee of type Cost Recovery is related to a Collection 
/// transaction.
/// </summary>
[Serializable]
public partial class DisbursementTransactionRln: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DisbursementTransactionRln()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DisbursementTransactionRln(DisbursementTransactionRln that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DisbursementTransactionRln Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DisbursementTransactionRln that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    description = that.description;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    dnrGeneratedId = that.dnrGeneratedId;
    dtrGeneratedId = that.dtrGeneratedId;
    cpaType = that.cpaType;
    cspNumber = that.cspNumber;
    dtrPGeneratedId = that.dtrPGeneratedId;
    cpaPType = that.cpaPType;
    cspPNumber = that.cspPNumber;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number that distinguishes one occurrence
  /// of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
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
  /// Example: Disbursement &quot;disburses&quot; collection - where &quot;
  /// disbursement&quot; respresents the reason.  This may have a description of
  /// &quot;the collection was paid out thru a disbursement&quot;.
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
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
  /// Timestamp of the most recent update to the entity occurence.
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
  [JsonPropertyName("dnrGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int DnrGeneratedId
  {
    get => dnrGeneratedId;
    set => dnrGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated timestamp that distinguishes one occurrence of 
  /// the entity type from another.
  /// </summary>
  [JsonPropertyName("dtrGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 9)]
  public int DtrGeneratedId
  {
    get => dtrGeneratedId;
    set => dtrGeneratedId = value;
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
  [Member(Index = 9, Type = MemberType.Char, Length = CpaType_MaxLength)]
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

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated timestamp that distinguishes one occurrence of 
  /// the entity type from another.
  /// </summary>
  [JsonPropertyName("dtrPGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 9)]
  public int DtrPGeneratedId
  {
    get => dtrPGeneratedId;
    set => dtrPGeneratedId = value;
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
  [Member(Index = 12, Type = MemberType.Char, Length = CpaPType_MaxLength)]
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
  [Member(Index = 13, Type = MemberType.Char, Length = CspPNumber_MaxLength)]
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

  private int systemGeneratedIdentifier;
  private string description;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private int dnrGeneratedId;
  private int dtrGeneratedId;
  private string cpaType;
  private string cspNumber;
  private int dtrPGeneratedId;
  private string cpaPType;
  private string cspPNumber;
}
