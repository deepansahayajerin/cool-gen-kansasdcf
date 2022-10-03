// The source file: ZDEL_RECORDED_DOCUMENT, ID: 374600629, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Recorded Document identifies occurances of generated documents.
/// These records include those documents which are not monitored for a response
/// date.
/// These records allow for multiple generation of the same document to 
/// different addresses.
/// Note that the procedure may determine whether or not to record the document 
/// based on concatenated values for both the document generation date and the
/// other pertinant attributes.
/// For Example:
/// A Journal Entry will not be recorded unless both a Document Generation Date 
/// and a court filing date exist.  This is to allow for draft copies of the
/// document to be generated but not recorded.
/// DATA MODEL ALERT!!!!!
/// *	Relationships must be added to the following entity types:
/// INCARCERATION
/// CSE PERSON ADDRESS
/// INCOME SOURCE
/// INCOME SOURCE CONTACT
/// BANKRUPTCY
/// PERSON PROGRAM
/// CSE PERSON RESOURCE
/// CHILD SUPPORT WORKSHEET
/// GENETIC TEST
/// PERSON GENETIC TEST
/// PERSONAL HEALTH INSURANCE
/// OBLIGATION TRANSACTION
/// SERVICE
/// *	The relationship between RECORDED_DOCUMENT and OUTGOING_DOCUMENT is not 
/// drawn.
/// each RECORDED_DOCUMENT always is the result of one OUTGOING_DOCUMENT
/// each OUTGOING_DOCUMENT sometimes results in one RECORDED_DOCUMENT
/// </summary>
[Serializable]
public partial class ZdelRecordedDocument: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ZdelRecordedDocument()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ZdelRecordedDocument(ZdelRecordedDocument that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ZdelRecordedDocument Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ZdelRecordedDocument that)
  {
    base.Assign(that);
    denormNumeric12 = that.denormNumeric12;
    denormText12 = that.denormText12;
    denormDate = that.denormDate;
    denormTimestamp = that.denormTimestamp;
    printDate = that.printDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    infId = that.infId;
  }

  /// <summary>
  /// The value of the DENORM_NUMERIC_12 attribute.
  /// Denorm Numeric 12 is one of a set of four attributes used to support a 
  /// denormalized identifier for the business object identified through the
  /// business object code.
  /// This set of attributes includes:
  /// DENORM_NUMBERIC_12
  /// DENORM_TEXT_12
  /// DENORM_DATE
  /// DENORM_TIMESTAMP
  /// </summary>
  [JsonPropertyName("denormNumeric12")]
  [Member(Index = 1, Type = MemberType.Number, Length = 12, Optional = true)]
  public long? DenormNumeric12
  {
    get => denormNumeric12;
    set => denormNumeric12 = value;
  }

  /// <summary>Length of the DENORM_TEXT_12 attribute.</summary>
  public const int DenormText12_MaxLength = 12;

  /// <summary>
  /// The value of the DENORM_TEXT_12 attribute.
  /// Denorm Text 12 is one of a set of four attributes used to support a 
  /// denormalized identifier for the business object identified through the
  /// business object code.
  /// This set of attributes includes:
  /// DENORM_NUMBERIC_12
  /// DENORM_TEXT_12
  /// DENORM_DATE
  /// DENORM_TIMESTAMP
  /// </summary>
  [JsonPropertyName("denormText12")]
  [Member(Index = 2, Type = MemberType.Char, Length = DenormText12_MaxLength, Optional
    = true)]
  public string DenormText12
  {
    get => denormText12;
    set => denormText12 = value != null
      ? TrimEnd(Substring(value, 1, DenormText12_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DENORM_DATE attribute.
  /// Denorm Date 12 is one of a set of four attributes used to support a 
  /// denormalized identifier for the business object identified through the
  /// business object code.
  /// This set of attributes includes:
  /// DENORM_NUMBERIC_12
  /// DENORM_TEXT_12
  /// DENORM_DATE
  /// DENORM_TIMESTAMP
  /// </summary>
  [JsonPropertyName("denormDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? DenormDate
  {
    get => denormDate;
    set => denormDate = value;
  }

  /// <summary>
  /// The value of the DENORM_TIMESTAMP attribute.
  /// Denorm Timestamp 12 is one of a set of four attributes used to support a 
  /// denormalized identifier for the business object identified through the
  /// business object code.
  /// This set of attributes includes:
  /// DENORM_NUMBERIC_12
  /// DENORM_TEXT_12
  /// DENORM_DATE
  /// DENORM_TIMESTAMP
  /// </summary>
  [JsonPropertyName("denormTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? DenormTimestamp
  {
    get => denormTimestamp;
    set => denormTimestamp = value;
  }

  /// <summary>
  /// The value of the PRINT_DATE attribute.
  /// Print Date is the date this specific occurance of the document is printed.
  /// </summary>
  [JsonPropertyName("printDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? PrintDate
  {
    get => printDate;
    set => printDate = value;
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
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("infId")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 9)]
  public int InfId
  {
    get => infId;
    set => infId = value;
  }

  private long? denormNumeric12;
  private string denormText12;
  private DateTime? denormDate;
  private DateTime? denormTimestamp;
  private DateTime? printDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int infId;
}
