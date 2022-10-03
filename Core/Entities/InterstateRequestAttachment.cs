// The source file: INTERSTATE_REQUEST_ATTACHMENT, ID: 371436513, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Provides a listing of the attachments that were sent or received with the 
/// interstate request. This could be a copy of their enforcement laws, birth
/// records etc.
/// </summary>
[Serializable]
public partial class InterstateRequestAttachment: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateRequestAttachment()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateRequestAttachment(InterstateRequestAttachment that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateRequestAttachment Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstateRequestAttachment that)
  {
    base.Assign(that);
    systemGeneratedSequenceNum = that.systemGeneratedSequenceNum;
    sentDate = that.sentDate;
    requestDate = that.requestDate;
    receivedDate = that.receivedDate;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    incompleteInd = that.incompleteInd;
    dataTypeCode = that.dataTypeCode;
    note = that.note;
    intHGeneratedId = that.intHGeneratedId;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_SEQUENCE_NUM attribute.
  /// </summary>
  [JsonPropertyName("systemGeneratedSequenceNum")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int SystemGeneratedSequenceNum
  {
    get => systemGeneratedSequenceNum;
    set => systemGeneratedSequenceNum = value;
  }

  /// <summary>
  /// The value of the SENT_DATE attribute.
  /// The date the attachments were sent to the other state.
  /// </summary>
  [JsonPropertyName("sentDate")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? SentDate
  {
    get => sentDate;
    set => sentDate = value;
  }

  /// <summary>
  /// The value of the REQUEST_DATE attribute.
  /// The date the attachments were requested from the other state.
  /// </summary>
  [JsonPropertyName("requestDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? RequestDate
  {
    get => requestDate;
    set => requestDate = value;
  }

  /// <summary>
  /// The value of the RECEIVED_DATE attribute.
  /// The date the attachments were received from the other state.
  /// </summary>
  [JsonPropertyName("receivedDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? ReceivedDate
  {
    get => receivedDate;
    set => receivedDate = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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

  /// <summary>Length of the INCOMPLETE_IND attribute.</summary>
  public const int IncompleteInd_MaxLength = 1;

  /// <summary>
  /// The value of the INCOMPLETE_IND attribute.
  /// </summary>
  [JsonPropertyName("incompleteInd")]
  [Member(Index = 9, Type = MemberType.Char, Length = IncompleteInd_MaxLength, Optional
    = true)]
  public string IncompleteInd
  {
    get => incompleteInd;
    set => incompleteInd = value != null
      ? TrimEnd(Substring(value, 1, IncompleteInd_MaxLength)) : null;
  }

  /// <summary>Length of the DATA_TYPE_CODE attribute.</summary>
  public const int DataTypeCode_MaxLength = 4;

  /// <summary>
  /// The value of the DATA_TYPE_CODE attribute.
  /// describes the type of attachment.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = DataTypeCode_MaxLength)]
  public string DataTypeCode
  {
    get => dataTypeCode ?? "";
    set => dataTypeCode = TrimEnd(Substring(value, 1, DataTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the DataTypeCode attribute.</summary>
  [JsonPropertyName("dataTypeCode")]
  [Computed]
  public string DataTypeCode_Json
  {
    get => NullIf(DataTypeCode, "");
    set => DataTypeCode = value;
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 80;

  /// <summary>
  /// The value of the NOTE attribute.
  /// Free-form text for additional information.
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
  [JsonPropertyName("intHGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 8)]
  public int IntHGeneratedId
  {
    get => intHGeneratedId;
    set => intHGeneratedId = value;
  }

  private int systemGeneratedSequenceNum;
  private DateTime? sentDate;
  private DateTime? requestDate;
  private DateTime? receivedDate;
  private DateTime? lastUpdatedTimestamp;
  private string lastUpdatedBy;
  private DateTime? createdTimestamp;
  private string createdBy;
  private string incompleteInd;
  private string dataTypeCode;
  private string note;
  private int intHGeneratedId;
}
