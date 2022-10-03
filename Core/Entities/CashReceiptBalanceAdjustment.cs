// The source file: CASH_RECEIPT_BALANCE_ADJUSTMENT, ID: 371431455, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Contains the information pertaining to the reason for tying two cash 
/// receipts together.
/// </summary>
[Serializable]
public partial class CashReceiptBalanceAdjustment: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CashReceiptBalanceAdjustment()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CashReceiptBalanceAdjustment(CashReceiptBalanceAdjustment that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CashReceiptBalanceAdjustment Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CashReceiptBalanceAdjustment that)
  {
    base.Assign(that);
    adjustmentAmount = that.adjustmentAmount;
    description = that.description;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    crtIdentifier = that.crtIdentifier;
    cstIdentifier = that.cstIdentifier;
    crvIdentifier = that.crvIdentifier;
    crtIIdentifier = that.crtIIdentifier;
    cstIIdentifier = that.cstIIdentifier;
    crvIIdentifier = that.crvIIdentifier;
    crrIdentifier = that.crrIdentifier;
  }

  /// <summary>
  /// The value of the ADJUSTMENT_AMOUNT attribute.
  /// The dollar amount from one days cash receipt that adjusts another days 
  /// cash receipt in order to help make that days cash receipt amount balance
  /// with the court interface amount.  Sometimes it make take more than two
  /// days of adjustments to account for the difference between a court
  /// interface and the check that the court sends.
  /// </summary>
  [JsonPropertyName("adjustmentAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 1, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal AdjustmentAmount
  {
    get => adjustmentAmount;
    set => adjustmentAmount = Truncate(value, 2);
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// Represent the description of the reason for relating two cash receipts 
  /// together.
  /// Example: The cash receipt for Oct 5th is over by $200 to cover the 
  /// shortage of the cash receipt for Oct 1st.
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
  /// The userid or program id that created the occurrance of the entity.
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
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 3)]
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
  [Member(Index = 6, Type = MemberType.Number, Length = 3)]
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
  [Member(Index = 7, Type = MemberType.Number, Length = 9)]
  public int CrvIdentifier
  {
    get => crvIdentifier;
    set => crvIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtIIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 3)]
  public int CrtIIdentifier
  {
    get => crtIIdentifier;
    set => crtIIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cstIIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 3)]
  public int CstIIdentifier
  {
    get => cstIIdentifier;
    set => cstIIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number that identifies each negotiable instrument in 
  /// any form of money received by CSE.  Or any information of money received
  /// by CSE.
  /// Examples:
  /// Cash, checks, court transmittals.
  /// </summary>
  [JsonPropertyName("crvIIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 9)]
  public int CrvIIdentifier
  {
    get => crvIIdentifier;
    set => crvIIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crrIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int CrrIdentifier
  {
    get => crrIdentifier;
    set => crrIdentifier = value;
  }

  private decimal adjustmentAmount;
  private string description;
  private string createdBy;
  private DateTime? createdTimestamp;
  private int crtIdentifier;
  private int cstIdentifier;
  private int crvIdentifier;
  private int crtIIdentifier;
  private int cstIIdentifier;
  private int crvIIdentifier;
  private int crrIdentifier;
}
