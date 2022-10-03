// The source file: CASH_RECEIPT_EVENT, ID: 371431934, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Scope: This represents the instance of receipt of cash or non-cash received 
/// by CSE.
/// Qualification: Any representation of negotiable instruments received by CSE 
/// in any form.
/// Purpose: To provide a audit trail for all cash and non-cash recevied by CSE.
/// Examples:  Currency, Check, Money Order, Credit, Cash Transmittal, Court 
/// Interface, Debt Set-Off, Electronic Funds Transfer, etc.
/// Integrity Conditions: Cash Receipt is a dependent entity of Cash Receipt 
/// Event.
/// </summary>
[Serializable]
public partial class CashReceiptEvent: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CashReceiptEvent()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CashReceiptEvent(CashReceiptEvent that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CashReceiptEvent Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CashReceiptEvent that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    receivedDate = that.receivedDate;
    sourceCreationDate = that.sourceCreationDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    totalNonCashTransactionCount = that.totalNonCashTransactionCount;
    totalAdjustmentCount = that.totalAdjustmentCount;
    totalCashFeeAmt = that.totalCashFeeAmt;
    totalNonCashFeeAmt = that.totalNonCashFeeAmt;
    anticipatedCheckAmt = that.anticipatedCheckAmt;
    totalCashAmt = that.totalCashAmt;
    totalCashTransactionCount = that.totalCashTransactionCount;
    totalNonCashAmt = that.totalNonCashAmt;
    cstIdentifier = that.cstIdentifier;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number that identifies each negotiable instrument in 
  /// any form of money received by CSE.  Or any information of money received
  /// by CSE.
  /// Examples:
  /// Cash, checks, court transmittals.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>
  /// The value of the RECEIVED_DATE attribute.
  /// The date the cash was receipted/received?
  /// </summary>
  [JsonPropertyName("receivedDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? ReceivedDate
  {
    get => receivedDate;
    set => receivedDate = value;
  }

  /// <summary>
  /// The value of the SOURCE_CREATION_DATE attribute.
  /// The date the interface was created by the sender.
  /// </summary>
  [JsonPropertyName("sourceCreationDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? SourceCreationDate
  {
    get => sourceCreationDate;
    set => sourceCreationDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the 
  /// CASH_RECEIPT_EVENT.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp that the CASH_RECEIPT_EVENT was entered into the system.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last updat of 
  /// CASH_RECEIPT_EVENT.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 6, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The date of the most recent change to a CASH_RECEIPT_EVENT.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 7, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>
  /// The value of the TOTAL_NON_CASH_TRANSACTION_COUNT attribute.
  /// The total number of non-cash transactions included in the transmission 
  /// control total record.
  /// Only used on court transmissions.
  /// </summary>
  [JsonPropertyName("totalNonCashTransactionCount")]
  [Member(Index = 8, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? TotalNonCashTransactionCount
  {
    get => totalNonCashTransactionCount;
    set => totalNonCashTransactionCount = value;
  }

  /// <summary>
  /// The value of the TOTAL_ADJUSTMENT_COUNT attribute.
  /// The total number of adjustment transactions included on the transmission 
  /// control total record.
  /// Only used on court transmissions.
  /// </summary>
  [JsonPropertyName("totalAdjustmentCount")]
  [Member(Index = 9, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? TotalAdjustmentCount
  {
    get => totalAdjustmentCount;
    set => totalAdjustmentCount = value;
  }

  /// <summary>
  /// The value of the TOTAL_CASH_FEE_AMT attribute.
  /// The total dollar amount of the SRS cash fees included on the transmission 
  /// control total record.
  /// Only used on court transmissions.
  /// </summary>
  [JsonPropertyName("totalCashFeeAmt")]
  [Member(Index = 10, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotalCashFeeAmt
  {
    get => totalCashFeeAmt;
    set => totalCashFeeAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the TOTAL_NON_CASH_FEE_AMT attribute.
  /// The total dollar of SRS non-cash fees included on the transmission control
  /// total record.
  /// Only used for court transmissions.
  /// </summary>
  [JsonPropertyName("totalNonCashFeeAmt")]
  [Member(Index = 11, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotalNonCashFeeAmt
  {
    get => totalNonCashFeeAmt;
    set => totalNonCashFeeAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the ANTICIPATED_CHECK_AMT attribute.
  /// The amount of the check SRS anticipates to receive to match this court 
  /// transmission.  This amount is calculated by the court.
  /// </summary>
  [JsonPropertyName("anticipatedCheckAmt")]
  [Member(Index = 12, Type = MemberType.Number, Length = 11, Precision = 2, Optional
    = true)]
  public decimal? AnticipatedCheckAmt
  {
    get => anticipatedCheckAmt;
    set => anticipatedCheckAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the TOTAL_CASH_AMT attribute.
  /// THE TOTAL DOLLAR OF CASH COLLECTIONS INCLUDED ON THE TRANSMISSION CONTROL 
  /// TOTAL RECORD.
  /// ONLY USED ON COURT TRANSMISSIONS.
  /// </summary>
  [JsonPropertyName("totalCashAmt")]
  [Member(Index = 13, Type = MemberType.Number, Length = 11, Precision = 2, Optional
    = true)]
  public decimal? TotalCashAmt
  {
    get => totalCashAmt;
    set => totalCashAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the TOTAL_CASH_TRANSACTION_COUNT attribute.
  /// The total number of cash collections included on the transmision control 
  /// total record.
  /// Only used on court transmissions.
  /// </summary>
  [JsonPropertyName("totalCashTransactionCount")]
  [Member(Index = 14, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? TotalCashTransactionCount
  {
    get => totalCashTransactionCount;
    set => totalCashTransactionCount = value;
  }

  /// <summary>
  /// The value of the TOTAL_NON_CASH_AMT attribute.
  /// The total dollars of non-cash collections included on the transmission 
  /// control total record.
  /// Only used on court transmissions.
  /// </summary>
  [JsonPropertyName("totalNonCashAmt")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotalNonCashAmt
  {
    get => totalNonCashAmt;
    set => totalNonCashAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cstIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 16, Type = MemberType.Number, Length = 3)]
  public int CstIdentifier
  {
    get => cstIdentifier;
    set => cstIdentifier = value;
  }

  private int systemGeneratedIdentifier;
  private DateTime? receivedDate;
  private DateTime? sourceCreationDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private int? totalNonCashTransactionCount;
  private int? totalAdjustmentCount;
  private decimal? totalCashFeeAmt;
  private decimal? totalNonCashFeeAmt;
  private decimal? anticipatedCheckAmt;
  private decimal? totalCashAmt;
  private int? totalCashTransactionCount;
  private decimal? totalNonCashAmt;
  private int cstIdentifier;
}
