// The source file: CASH_RECEIPT_AUDIT, ID: 371431442, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Will keep track of who changed the entity and when it was changed.
/// </summary>
[Serializable]
public partial class CashReceiptAudit: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CashReceiptAudit()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CashReceiptAudit(CashReceiptAudit that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CashReceiptAudit Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CashReceiptAudit that)
  {
    base.Assign(that);
    receiptAmount = that.receiptAmount;
    lastUpdatedTmst = that.lastUpdatedTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    priorTransactionAmount = that.priorTransactionAmount;
    priorAdjustmentAmount = that.priorAdjustmentAmount;
    crtIdentifier = that.crtIdentifier;
    cstIdentifier = that.cstIdentifier;
    crvIdentifier = that.crvIdentifier;
  }

  /// <summary>
  /// The value of the RECEIPT_AMOUNT attribute.
  /// This is the amount actually being receipted.  It is either the amount of 
  /// the check, money order, or currency.  For non-cash could be the amount of
  /// the direct payments from the court.
  /// This is the amount that gets deposited for cash-types.
  /// </summary>
  [JsonPropertyName("receiptAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 1, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal ReceiptAmount
  {
    get => receiptAmount;
    set => receiptAmount = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the PRIOR_TRANSACTION_AMOUNT attribute.
  /// The total amount of transaction on the cash receipt prior to the change 
  /// that was made at the time that this cash receipt audit row was created.
  /// On the court interface this is the total of the type '2' transaction
  /// records.
  /// </summary>
  [JsonPropertyName("priorTransactionAmount")]
  [Member(Index = 4, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? PriorTransactionAmount
  {
    get => priorTransactionAmount;
    set => priorTransactionAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the PRIOR_ADJUSTMENT_AMOUNT attribute.
  /// The total amount of all adjustments on the cash receipt prior to the 
  /// creation of this row of cash receipt audit.   On the court interface this
  /// is the total of the type '5' adjustment records.
  /// 		
  /// </summary>
  [JsonPropertyName("priorAdjustmentAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal PriorAdjustmentAmount
  {
    get => priorAdjustmentAmount;
    set => priorAdjustmentAmount = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 3)]
  public int CrtIdentifier
  {
    get => crtIdentifier;
    set => crtIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cstIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int CstIdentifier
  {
    get => cstIdentifier;
    set => cstIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number that identifies each negotiable instrument in 
  /// any form of money received by CSE.  Or any information of money received
  /// by CSE.
  /// Examples:
  /// Cash, checks, court transmittals.
  /// </summary>
  [JsonPropertyName("crvIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 9)]
  public int CrvIdentifier
  {
    get => crvIdentifier;
    set => crvIdentifier = value;
  }

  private decimal receiptAmount;
  private DateTime? lastUpdatedTmst;
  private string lastUpdatedBy;
  private decimal? priorTransactionAmount;
  private decimal priorAdjustmentAmount;
  private int crtIdentifier;
  private int cstIdentifier;
  private int crvIdentifier;
}
