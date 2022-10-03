// The source file: CASH_RECEIPT_DETAIL_STAT_HISTORY, ID: 371431880, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Provides an audit trail representing each period in time the cash receipt 
/// detail was in a particular state.
/// </summary>
[Serializable]
public partial class CashReceiptDetailStatHistory: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CashReceiptDetailStatHistory()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CashReceiptDetailStatHistory(CashReceiptDetailStatHistory that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CashReceiptDetailStatHistory Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CashReceiptDetailStatHistory that)
  {
    base.Assign(that);
    discontinueDate = that.discontinueDate;
    reasonCodeId = that.reasonCodeId;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    reasonText = that.reasonText;
    crdIdentifier = that.crdIdentifier;
    crvIdentifier = that.crvIdentifier;
    cstIdentifier = that.cstIdentifier;
    crtIdentifier = that.crtIdentifier;
    cdsIdentifier = that.cdsIdentifier;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The date upon which this occurrence of the entity can no longer be used.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 1, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the REASON_CODE_ID attribute.</summary>
  public const int ReasonCodeId_MaxLength = 10;

  /// <summary>
  /// The value of the REASON_CODE_ID attribute.
  /// The reason code identifier from the codes table.  Describes why the item 
  /// was put in suspense or pending status. It is required if the status
  /// history is a suspense  or pended status.  It should have no value if the
  /// status history if not in either of those statuses.
  /// </summary>
  [JsonPropertyName("reasonCodeId")]
  [Member(Index = 2, Type = MemberType.Char, Length = ReasonCodeId_MaxLength, Optional
    = true)]
  public string ReasonCodeId
  {
    get => reasonCodeId;
    set => reasonCodeId = value != null
      ? TrimEnd(Substring(value, 1, ReasonCodeId_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
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
  /// The timestamp the occurrence was created.  This is also the date that the 
  /// status history record becomes effective.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the REASON_TEXT attribute.</summary>
  public const int ReasonText_MaxLength = 240;

  /// <summary>
  /// The value of the REASON_TEXT attribute.
  /// May contain some reason why the entity was changed or deleted.
  /// </summary>
  [JsonPropertyName("reasonText")]
  [Member(Index = 5, Type = MemberType.Varchar, Length = ReasonText_MaxLength, Optional
    = true)]
  public string ReasonText
  {
    get => reasonText;
    set => reasonText = value != null
      ? Substring(value, 1, ReasonText_MaxLength) : null;
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique sequential number within CASH_RECEIPT that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crdIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 4)]
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
  [JsonPropertyName("cstIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 3)]
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
  [Member(Index = 9, Type = MemberType.Number, Length = 3)]
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
  [JsonPropertyName("cdsIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 3)]
  public int CdsIdentifier
  {
    get => cdsIdentifier;
    set => cdsIdentifier = value;
  }

  private DateTime? discontinueDate;
  private string reasonCodeId;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string reasonText;
  private int crdIdentifier;
  private int crvIdentifier;
  private int cstIdentifier;
  private int crtIdentifier;
  private int cdsIdentifier;
}
