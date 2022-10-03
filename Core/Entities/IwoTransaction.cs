// The source file: IWO_TRANSACTION, ID: 1902467093, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// An income withholding transaction for an NCP that is to either be printed or
/// submitted electronically to an employer.
/// </summary>
[Serializable]
public partial class IwoTransaction: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public IwoTransaction()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public IwoTransaction(IwoTransaction that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new IwoTransaction Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(IwoTransaction that)
  {
    base.Assign(that);
    identifier = that.identifier;
    transactionNumber = that.transactionNumber;
    currentStatus = that.currentStatus;
    statusDate = that.statusDate;
    note = that.note;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    lgaIdentifier = that.lgaIdentifier;
    cspINumber = that.cspINumber;
    isrIdentifier = that.isrIdentifier;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Sequential number used to identify unique occurrences for the same 
  /// cse_person and legal_action.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the TRANSACTION_NUMBER attribute.</summary>
  public const int TransactionNumber_MaxLength = 12;

  /// <summary>
  /// The value of the TRANSACTION_NUMBER attribute.
  /// Unique system assigned value used to track income withholding information 
  /// to be sent to an employer.
  /// </summary>
  [JsonPropertyName("transactionNumber")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = TransactionNumber_MaxLength, Optional = true)]
  public string TransactionNumber
  {
    get => transactionNumber;
    set => transactionNumber = value != null
      ? TrimEnd(Substring(value, 1, TransactionNumber_MaxLength)) : null;
  }

  /// <summary>Length of the CURRENT_STATUS attribute.</summary>
  public const int CurrentStatus_MaxLength = 1;

  /// <summary>
  /// The value of the CURRENT_STATUS attribute.
  /// The current status of the income withholding information.  This is a 
  /// consolidated status that includes all attempts to transmit the information
  /// to an employer.
  /// </summary>
  [JsonPropertyName("currentStatus")]
  [Member(Index = 3, Type = MemberType.Char, Length = CurrentStatus_MaxLength, Optional
    = true)]
  public string CurrentStatus
  {
    get => currentStatus;
    set => currentStatus = value != null
      ? TrimEnd(Substring(value, 1, CurrentStatus_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the STATUS_DATE attribute.
  /// The date the current_status became effective.
  /// </summary>
  [JsonPropertyName("statusDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? StatusDate
  {
    get => statusDate;
    set => statusDate = value;
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 80;

  /// <summary>
  /// The value of the NOTE attribute.
  /// A free form text entry in which to store a user entered comment.
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 5, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => note;
    set => note = value != null ? Substring(value, 1, Note_MaxLength) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 8, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 9, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 9)]
  public int LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspINumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspINumber")]
  [Member(Index = 11, Type = MemberType.Char, Length = CspINumber_MaxLength, Optional
    = true)]
  public string CspINumber
  {
    get => cspINumber;
    set => cspINumber = value != null
      ? TrimEnd(Substring(value, 1, CspINumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated identifier of no business meaning.
  /// </summary>
  [JsonPropertyName("isrIdentifier")]
  [Member(Index = 12, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? IsrIdentifier
  {
    get => isrIdentifier;
    set => isrIdentifier = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  private int identifier;
  private string transactionNumber;
  private string currentStatus;
  private DateTime? statusDate;
  private string note;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int lgaIdentifier;
  private string cspINumber;
  private DateTime? isrIdentifier;
  private string cspNumber;
}
