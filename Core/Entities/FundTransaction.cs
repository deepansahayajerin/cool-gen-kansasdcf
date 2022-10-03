// The source file: FUND_TRANSACTION, ID: 371434709, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// A recording of the details of a transaction involving a fund.  All relevent 
/// information about a fund transaction is kept.  This includes things like
/// what kind of transaction it is, , who performed the transaction and when it
/// was performed, and the affect on the fund.
/// Example:
///   A deposit of a cash receipt into the clearing fund.
/// </summary>
[Serializable]
public partial class FundTransaction: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FundTransaction()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FundTransaction(FundTransaction that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FundTransaction Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FundTransaction that)
  {
    base.Assign(that);
    depositNumber = that.depositNumber;
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    amount = that.amount;
    businessDate = that.businessDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    fttIdentifier = that.fttIdentifier;
    pcaCode = that.pcaCode;
    pcaEffectiveDate = that.pcaEffectiveDate;
    funIdentifier = that.funIdentifier;
  }

  /// <summary>
  /// The value of the DEPOSIT_NUMBER attribute.
  /// A sequential deposit number that the users will use when referencing a 
  /// specific deposit transaction.
  /// </summary>
  [JsonPropertyName("depositNumber")]
  [Member(Index = 1, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? DepositNumber
  {
    get => depositNumber;
    set => depositNumber = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique number that specifies each occurrence of the entity type.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>
  /// The value of the AMOUNT attribute.
  /// The value of the transaction in U.S. dollars.
  /// </summary>
  [JsonPropertyName("amount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal Amount
  {
    get => amount;
    set => amount = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the BUSINESS_DATE attribute.
  /// The date the transaction actually occurred in the business.This date could
  /// be different from the date that the transaction was entered into the
  /// system.  A transaction could occur but not be entered into the system for
  /// a couple of days.  In this case the APPLIED_DATE would be a couple of days
  /// before the CREATED_DATE.
  /// </summary>
  [JsonPropertyName("businessDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? BusinessDate
  {
    get => businessDate;
    set => businessDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for creating the occurrence of the 
  /// entity type.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp of when the occurrence of the entity type was created.  This
  /// timestamp is used for the identifier.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id for the last update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 7, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the entity occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 8, Type = MemberType.Timestamp, Optional = true)]
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
  [JsonPropertyName("fttIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 3)]
  public int FttIdentifier
  {
    get => fttIdentifier;
    set => fttIdentifier = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int PcaCode_MaxLength = 5;

  /// <summary>
  /// The value of the CODE attribute.
  /// A short representation  for the purpose of quick identification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = PcaCode_MaxLength)]
  public string PcaCode
  {
    get => pcaCode ?? "";
    set => pcaCode = TrimEnd(Substring(value, 1, PcaCode_MaxLength));
  }

  /// <summary>
  /// The json value of the PcaCode attribute.</summary>
  [JsonPropertyName("pcaCode")]
  [Computed]
  public string PcaCode_Json
  {
    get => NullIf(PcaCode, "");
    set => PcaCode = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence is activated by the system.  An 
  /// effective date allows the entity to be entered into the system with a
  /// future date.  The occurrence of the entity will &quot;take effect&quot; on
  /// the effective date.
  /// </summary>
  [JsonPropertyName("pcaEffectiveDate")]
  [Member(Index = 11, Type = MemberType.Date)]
  public DateTime? PcaEffectiveDate
  {
    get => pcaEffectiveDate;
    set => pcaEffectiveDate = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("funIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 3)]
  public int FunIdentifier
  {
    get => funIdentifier;
    set => funIdentifier = value;
  }

  private int? depositNumber;
  private int systemGeneratedIdentifier;
  private decimal amount;
  private DateTime? businessDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private int fttIdentifier;
  private string pcaCode;
  private DateTime? pcaEffectiveDate;
  private int funIdentifier;
}
