// The source file: HEADER_RECORD, ID: 374396735, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class HeaderRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public HeaderRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public HeaderRecord(HeaderRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new HeaderRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(HeaderRecord that)
  {
    base.Assign(that);
    recordType = that.recordType;
    actionCode = that.actionCode;
    transactionDate = that.transactionDate;
    userid = that.userid;
    timestamp = that.timestamp;
    filler = that.filler;
  }

  /// <summary>Length of the RECORD_TYPE attribute.</summary>
  public const int RecordType_MaxLength = 1;

  /// <summary>
  /// The value of the RECORD_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordType_MaxLength)]
  public string RecordType
  {
    get => recordType ?? "";
    set => recordType = TrimEnd(Substring(value, 1, RecordType_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordType attribute.</summary>
  [JsonPropertyName("recordType")]
  [Computed]
  public string RecordType_Json
  {
    get => NullIf(RecordType, "");
    set => RecordType = value;
  }

  /// <summary>Length of the ACTION_CODE attribute.</summary>
  public const int ActionCode_MaxLength = 4;

  /// <summary>
  /// The value of the ACTION_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ActionCode_MaxLength)]
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

  /// <summary>Length of the TRANSACTION_DATE attribute.</summary>
  public const int TransactionDate_MaxLength = 8;

  /// <summary>
  /// The value of the TRANSACTION_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = TransactionDate_MaxLength)]
    
  public string TransactionDate
  {
    get => transactionDate ?? "";
    set => transactionDate =
      TrimEnd(Substring(value, 1, TransactionDate_MaxLength));
  }

  /// <summary>
  /// The json value of the TransactionDate attribute.</summary>
  [JsonPropertyName("transactionDate")]
  [Computed]
  public string TransactionDate_Json
  {
    get => NullIf(TransactionDate, "");
    set => TransactionDate = value;
  }

  /// <summary>Length of the USERID attribute.</summary>
  public const int Userid_MaxLength = 8;

  /// <summary>
  /// The value of the USERID attribute.
  /// The signon of the person or program that last updated the occurrence of 
  /// the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Userid_MaxLength)]
  public string Userid
  {
    get => userid ?? "";
    set => userid = TrimEnd(Substring(value, 1, Userid_MaxLength));
  }

  /// <summary>
  /// The json value of the Userid attribute.</summary>
  [JsonPropertyName("userid")]
  [Computed]
  public string Userid_Json
  {
    get => NullIf(Userid, "");
    set => Userid = value;
  }

  /// <summary>Length of the TIMESTAMP attribute.</summary>
  public const int Timestamp_MaxLength = 20;

  /// <summary>
  /// The value of the TIMESTAMP attribute.
  /// The date and time that the occurrence of the entity was last updated.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = Timestamp_MaxLength)]
  public string Timestamp
  {
    get => timestamp ?? "";
    set => timestamp = TrimEnd(Substring(value, 1, Timestamp_MaxLength));
  }

  /// <summary>
  /// The json value of the Timestamp attribute.</summary>
  [JsonPropertyName("timestamp")]
  [Computed]
  public string Timestamp_Json
  {
    get => NullIf(Timestamp, "");
    set => Timestamp = value;
  }

  /// <summary>Length of the FILLER attribute.</summary>
  public const int Filler_MaxLength = 155;

  /// <summary>
  /// The value of the FILLER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Filler_MaxLength)]
  public string Filler
  {
    get => filler ?? "";
    set => filler = TrimEnd(Substring(value, 1, Filler_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler attribute.</summary>
  [JsonPropertyName("filler")]
  [Computed]
  public string Filler_Json
  {
    get => NullIf(Filler, "");
    set => Filler = value;
  }

  private string recordType;
  private string actionCode;
  private string transactionDate;
  private string userid;
  private string timestamp;
  private string filler;
}
