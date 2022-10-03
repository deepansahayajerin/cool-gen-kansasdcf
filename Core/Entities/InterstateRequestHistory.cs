// The source file: INTERSTATE_REQUEST_HISTORY, ID: 371436528, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// This contains the history of all interstate csenet transactions for both 
/// inbound or outbound request.
/// </summary>
[Serializable]
public partial class InterstateRequestHistory: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateRequestHistory()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateRequestHistory(InterstateRequestHistory that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateRequestHistory Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstateRequestHistory that)
  {
    base.Assign(that);
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    transactionDirectionInd = that.transactionDirectionInd;
    transactionSerialNum = that.transactionSerialNum;
    actionCode = that.actionCode;
    functionalTypeCode = that.functionalTypeCode;
    transactionDate = that.transactionDate;
    actionReasonCode = that.actionReasonCode;
    actionResolutionDate = that.actionResolutionDate;
    attachmentIndicator = that.attachmentIndicator;
    note = that.note;
    intGeneratedId = that.intGeneratedId;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 1, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that last updated the occurrance of the entity.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 2, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>Length of the TRANSACTION_DIRECTION_IND attribute.</summary>
  public const int TransactionDirectionInd_MaxLength = 1;

  /// <summary>
  /// The value of the TRANSACTION_DIRECTION_IND attribute.
  /// This describes whether the transaction is an Inbound or Outbound 
  /// transaction.
  /// Values : I
  ///          O.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = TransactionDirectionInd_MaxLength)]
  public string TransactionDirectionInd
  {
    get => transactionDirectionInd ?? "";
    set => transactionDirectionInd =
      TrimEnd(Substring(value, 1, TransactionDirectionInd_MaxLength));
  }

  /// <summary>
  /// The json value of the TransactionDirectionInd attribute.</summary>
  [JsonPropertyName("transactionDirectionInd")]
  [Computed]
  public string TransactionDirectionInd_Json
  {
    get => NullIf(TransactionDirectionInd, "");
    set => TransactionDirectionInd = value;
  }

  /// <summary>
  /// The value of the TRANSACTION_SERIAL_NUM attribute.
  /// This is a unique number assigned to each CSENet transaction.  It has no 
  /// place in the KESSEP system but is required to provide a key used to
  /// process CSENet Referrals.
  /// </summary>
  [JsonPropertyName("transactionSerialNum")]
  [DefaultValue(0L)]
  [Member(Index = 4, Type = MemberType.Number, Length = 12)]
  public long TransactionSerialNum
  {
    get => transactionSerialNum;
    set => transactionSerialNum = value;
  }

  /// <summary>Length of the ACTION_CODE attribute.</summary>
  public const int ActionCode_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION_CODE attribute.
  /// Describes the action of this CSENet referral.
  /// Values:
  /// R = Request
  /// A = Acknowledgement of receipt
  /// P = Provision of Data
  /// M = Reminder
  /// U = Update of previously transmitted request
  /// C = Cancel
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = ActionCode_MaxLength)]
  public string ActionCode
  {
    get => actionCode ?? "";
    set => actionCode = TrimEnd(Substring(value, 1, ActionCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ActionCode attribute.</summary>
  [JsonPropertyName("actionCode")]
  [Computed]
  public string ActionCode_Json
  {
    get => NullIf(ActionCode, "");
    set => ActionCode = value;
  }

  /// <summary>Length of the FUNCTIONAL_TYPE_CODE attribute.</summary>
  public const int FunctionalTypeCode_MaxLength = 3;

  /// <summary>
  /// The value of the FUNCTIONAL_TYPE_CODE attribute.
  /// Describes the CSE activity requested by the CSENet referral.
  /// Values:
  /// LO1 = Quick Locate
  /// LO2 = Full Locate
  /// PAT = Paternity Establishment
  /// EST = Order Establishment
  /// ENF = Enforcement
  /// COL = Collection
  /// MSC = Miscellaneous
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = FunctionalTypeCode_MaxLength)]
  public string FunctionalTypeCode
  {
    get => functionalTypeCode ?? "";
    set => functionalTypeCode =
      TrimEnd(Substring(value, 1, FunctionalTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the FunctionalTypeCode attribute.</summary>
  [JsonPropertyName("functionalTypeCode")]
  [Computed]
  public string FunctionalTypeCode_Json
  {
    get => NullIf(FunctionalTypeCode, "");
    set => FunctionalTypeCode = value;
  }

  /// <summary>
  /// The value of the TRANSACTION_DATE attribute.
  /// This is the date on which CSENet transmitted the Referral.
  /// </summary>
  [JsonPropertyName("transactionDate")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? TransactionDate
  {
    get => transactionDate;
    set => transactionDate = value;
  }

  /// <summary>Length of the ACTION_REASON_CODE attribute.</summary>
  public const int ActionReasonCode_MaxLength = 5;

  /// <summary>
  /// The value of the ACTION_REASON_CODE attribute.
  /// CSENet field that indicates the reason code associated with this 
  /// transaction.  Sample values are as follows:
  /// GSADD Add a participant
  /// GSDEL Delete a participant
  /// GSCAS Change local case ID
  /// GSFIP Change local FIPS code
  /// </summary>
  [JsonPropertyName("actionReasonCode")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = ActionReasonCode_MaxLength, Optional = true)]
  public string ActionReasonCode
  {
    get => actionReasonCode;
    set => actionReasonCode = value != null
      ? TrimEnd(Substring(value, 1, ActionReasonCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ACTION_RESOLUTION_DATE attribute.
  /// The date that the 'Action' event occurred.
  /// </summary>
  [JsonPropertyName("actionResolutionDate")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? ActionResolutionDate
  {
    get => actionResolutionDate;
    set => actionResolutionDate = value;
  }

  /// <summary>Length of the ATTACHMENT_INDICATOR attribute.</summary>
  public const int AttachmentIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the ATTACHMENT_INDICATOR attribute.
  /// Indicates whether attachment accompany this CSENET transaction.
  /// </summary>
  [JsonPropertyName("attachmentIndicator")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = AttachmentIndicator_MaxLength, Optional = true)]
  public string AttachmentIndicator
  {
    get => attachmentIndicator;
    set => attachmentIndicator = value != null
      ? TrimEnd(Substring(value, 1, AttachmentIndicator_MaxLength)) : null;
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 400;

  /// <summary>
  /// The value of the NOTE attribute.
  /// Note text accompanying CSENET transaction
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 11, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => note;
    set => note = value != null ? Substring(value, 1, Note_MaxLength) : null;
  }

  /// <summary>
  /// The value of the INT_H_GENERATED_ID attribute.
  /// Attribute to uniquely identify an interstate referral associated within a 
  /// case.
  /// This will be a system-generated number.
  /// </summary>
  [JsonPropertyName("intGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 8)]
  public int IntGeneratedId
  {
    get => intGeneratedId;
    set => intGeneratedId = value;
  }

  private DateTime? createdTimestamp;
  private string createdBy;
  private string transactionDirectionInd;
  private long transactionSerialNum;
  private string actionCode;
  private string functionalTypeCode;
  private DateTime? transactionDate;
  private string actionReasonCode;
  private DateTime? actionResolutionDate;
  private string attachmentIndicator;
  private string note;
  private int intGeneratedId;
}
