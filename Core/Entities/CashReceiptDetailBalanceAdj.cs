// The source file: CASH_RECEIPT_DETAIL_BALANCE_ADJ, ID: 371431714, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Contains the information about how one cash receipt detail adjusts another 
/// cash receipt detail.
/// </summary>
[Serializable]
public partial class CashReceiptDetailBalanceAdj: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CashReceiptDetailBalanceAdj()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CashReceiptDetailBalanceAdj(CashReceiptDetailBalanceAdj that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CashReceiptDetailBalanceAdj Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CashReceiptDetailBalanceAdj that)
  {
    base.Assign(that);
    description = that.description;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    crdIdentifier = that.crdIdentifier;
    crvIdentifier = that.crvIdentifier;
    cstIdentifier = that.cstIdentifier;
    crtIdentifier = that.crtIdentifier;
    crdSIdentifier = that.crdSIdentifier;
    crvSIdentifier = that.crvSIdentifier;
    cstSIdentifier = that.cstSIdentifier;
    crtSIdentifier = that.crtSIdentifier;
    crnIdentifier = that.crnIdentifier;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// Represent the description of the reason for relating two cash receipt 
  /// details together.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 1, Type = MemberType.Varchar, Length
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
  /// The userid of the person who created the cash receipt balance adjustment.
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The date and time that the entity was created on the system.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique sequential number within CASH_RECEIPT that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crdIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 4)]
  public int CrdIdentifier
  {
    get => crdIdentifier;
    set => crdIdentifier = value;
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
  [Member(Index = 5, Type = MemberType.Number, Length = 9)]
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
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int CrtIdentifier
  {
    get => crtIdentifier;
    set => crtIdentifier = value;
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique sequential number within CASH_RECEIPT that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crdSIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 4)]
  public int CrdSIdentifier
  {
    get => crdSIdentifier;
    set => crdSIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number that identifies each negotiable instrument in 
  /// any form of money received by CSE.  Or any information of money received
  /// by CSE.
  /// Examples:
  /// Cash, checks, court transmittals.
  /// </summary>
  [JsonPropertyName("crvSIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 9)]
  public int CrvSIdentifier
  {
    get => crvSIdentifier;
    set => crvSIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cstSIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 3)]
  public int CstSIdentifier
  {
    get => cstSIdentifier;
    set => cstSIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtSIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int CrtSIdentifier
  {
    get => crtSIdentifier;
    set => crtSIdentifier = value;
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crnIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 3)]
  public int CrnIdentifier
  {
    get => crnIdentifier;
    set => crnIdentifier = value;
  }

  private string description;
  private string createdBy;
  private DateTime? createdTimestamp;
  private int crdIdentifier;
  private int crvIdentifier;
  private int cstIdentifier;
  private int crtIdentifier;
  private int crdSIdentifier;
  private int crvSIdentifier;
  private int cstSIdentifier;
  private int crtSIdentifier;
  private int crnIdentifier;
}
