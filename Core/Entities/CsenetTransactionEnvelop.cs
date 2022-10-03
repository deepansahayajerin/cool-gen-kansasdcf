// The source file: CSENET_TRANSACTION_ENVELOP, ID: 371433420, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Data about the CSENet transaction; used to uniquely identify the transaction
/// within KESSEP, to indicate its processing status, creation date and time.
/// </summary>
[Serializable]
public partial class CsenetTransactionEnvelop: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsenetTransactionEnvelop()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsenetTransactionEnvelop(CsenetTransactionEnvelop that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsenetTransactionEnvelop Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CsenetTransactionEnvelop that)
  {
    base.Assign(that);
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    directionInd = that.directionInd;
    processingStatusCode = that.processingStatusCode;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    errorCode = that.errorCode;
    ccaTransactionDt = that.ccaTransactionDt;
    ccaTransSerNum = that.ccaTransSerNum;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the DIRECTION_IND attribute.</summary>
  public const int DirectionInd_MaxLength = 1;

  /// <summary>
  /// The value of the DIRECTION_IND attribute.
  /// This describes whether the transaction is an Inbound or Outbound 
  /// transaction.
  /// Values : I
  ///          O.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = DirectionInd_MaxLength)]
  public string DirectionInd
  {
    get => directionInd ?? "";
    set => directionInd = TrimEnd(Substring(value, 1, DirectionInd_MaxLength));
  }

  /// <summary>
  /// The json value of the DirectionInd attribute.</summary>
  [JsonPropertyName("directionInd")]
  [Computed]
  public string DirectionInd_Json
  {
    get => NullIf(DirectionInd, "");
    set => DirectionInd = value;
  }

  /// <summary>Length of the PROCESSING_STATUS_CODE attribute.</summary>
  public const int ProcessingStatusCode_MaxLength = 1;

  /// <summary>
  /// The value of the PROCESSING_STATUS_CODE attribute.
  /// This describes the current procesing status.
  /// R - Request
  /// S - Sent
  /// C - Received
  /// P - Processed.	
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = ProcessingStatusCode_MaxLength)]
  public string ProcessingStatusCode
  {
    get => processingStatusCode ?? "";
    set => processingStatusCode =
      TrimEnd(Substring(value, 1, ProcessingStatusCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ProcessingStatusCode attribute.</summary>
  [JsonPropertyName("processingStatusCode")]
  [Computed]
  public string ProcessingStatusCode_Json
  {
    get => NullIf(ProcessingStatusCode, "");
    set => ProcessingStatusCode = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the ERROR_CODE attribute.</summary>
  public const int ErrorCode_MaxLength = 8;

  /// <summary>
  /// The value of the ERROR_CODE attribute.
  /// Describes the type of error that occurred at the host.
  /// </summary>
  [JsonPropertyName("errorCode")]
  [Member(Index = 7, Type = MemberType.Char, Length = ErrorCode_MaxLength, Optional
    = true)]
  public string ErrorCode
  {
    get => errorCode;
    set => errorCode = value != null
      ? TrimEnd(Substring(value, 1, ErrorCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the TRANSACTION_DATE attribute.
  /// This is the date on which CSENet transmitted the Referral.
  /// </summary>
  [JsonPropertyName("ccaTransactionDt")]
  [Member(Index = 8, Type = MemberType.Date)]
  public DateTime? CcaTransactionDt
  {
    get => ccaTransactionDt;
    set => ccaTransactionDt = value;
  }

  /// <summary>
  /// The value of the TRANS_SERIAL_NUMBER attribute.
  /// This is a unique number assigned to each CSENet transaction.  It has no 
  /// place in the KESSEP system but is required to provide a key used to
  /// process CSENet Referrals.
  /// </summary>
  [JsonPropertyName("ccaTransSerNum")]
  [DefaultValue(0L)]
  [Member(Index = 9, Type = MemberType.Number, Length = 12)]
  public long CcaTransSerNum
  {
    get => ccaTransSerNum;
    set => ccaTransSerNum = value;
  }

  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string directionInd;
  private string processingStatusCode;
  private string createdBy;
  private DateTime? createdTstamp;
  private string errorCode;
  private DateTime? ccaTransactionDt;
  private long ccaTransSerNum;
}
