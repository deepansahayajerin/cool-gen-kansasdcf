// The source file: OUTGOING_DOCUMENT, ID: 371422621, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Each Document 'generated' will have an entry in this entity. This is the 
/// holding place for all documents awaiting printing either at night or on-
/// demand.
/// </summary>
[Serializable]
public partial class OutgoingDocument: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OutgoingDocument()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OutgoingDocument(OutgoingDocument that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OutgoingDocument Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(OutgoingDocument that)
  {
    base.Assign(that);
    printSucessfulIndicator = that.printSucessfulIndicator;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatdTstamp = that.lastUpdatdTstamp;
    fieldValuesArchiveDate = that.fieldValuesArchiveDate;
    fieldValuesArchiveInd = that.fieldValuesArchiveInd;
    docEffectiveDte = that.docEffectiveDte;
    docName = that.docName;
    podPrinterId = that.podPrinterId;
    infId = that.infId;
  }

  /// <summary>Length of the PRINT_SUCESSFUL_INDICATOR attribute.</summary>
  public const int PrintSucessfulIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the PRINT_SUCESSFUL_INDICATOR attribute.
  /// Flag indicating the outcome of a print operation.
  /// 	
  /// 	Y - Yes (successful)
  /// 	N - No  (unsuccessful)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = PrintSucessfulIndicator_MaxLength)]
  public string PrintSucessfulIndicator
  {
    get => printSucessfulIndicator ?? "";
    set => printSucessfulIndicator =
      TrimEnd(Substring(value, 1, PrintSucessfulIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the PrintSucessfulIndicator attribute.</summary>
  [JsonPropertyName("printSucessfulIndicator")]
  [Computed]
  public string PrintSucessfulIndicator_Json
  {
    get => NullIf(PrintSucessfulIndicator, "");
    set => PrintSucessfulIndicator = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// * Draft *
  /// The user id or program id responsible for the creation of the occurrence.
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
  /// * Draft *
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// * Draft *
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 4, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATD_TSTAMP attribute.
  /// * Draft *
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatdTstamp")]
  [Member(Index = 5, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatdTstamp
  {
    get => lastUpdatdTstamp;
    set => lastUpdatdTstamp = value;
  }

  /// <summary>
  /// The value of the FIELD_VALUES_ARCHIVE_DATE attribute.
  /// The date Field values archived for this Outgoing Document.
  /// </summary>
  [JsonPropertyName("fieldValuesArchiveDate")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? FieldValuesArchiveDate
  {
    get => fieldValuesArchiveDate;
    set => fieldValuesArchiveDate = value;
  }

  /// <summary>Length of the FIELD_VALUES_ARCHIVE_IND attribute.</summary>
  public const int FieldValuesArchiveInd_MaxLength = 1;

  /// <summary>
  /// The value of the FIELD_VALUES_ARCHIVE_IND attribute.
  /// </summary>
  [JsonPropertyName("fieldValuesArchiveInd")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = FieldValuesArchiveInd_MaxLength, Optional = true)]
  [Value(null)]
  [Value("N")]
  [Value("Y")]
  [ImplicitValue("N")]
  public string FieldValuesArchiveInd
  {
    get => fieldValuesArchiveInd;
    set => fieldValuesArchiveInd = value != null
      ? TrimEnd(Substring(value, 1, FieldValuesArchiveInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// RESP: KESSEP
  /// 
  /// Effective date of the document.  A document could have many versions, but
  /// only one version is current on a given day.  On ADD, is set to system
  /// current date.
  /// </summary>
  [JsonPropertyName("docEffectiveDte")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? DocEffectiveDte
  {
    get => docEffectiveDte;
    set => docEffectiveDte = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int DocName_MaxLength = 8;

  /// <summary>
  /// The value of the NAME attribute.
  /// Eight char user defined name for this notice. eg CSE-201
  /// </summary>
  [JsonPropertyName("docName")]
  [Member(Index = 9, Type = MemberType.Char, Length = DocName_MaxLength, Optional
    = true)]
  public string DocName
  {
    get => docName;
    set => docName = value != null
      ? TrimEnd(Substring(value, 1, DocName_MaxLength)) : null;
  }

  /// <summary>Length of the PRINTER_ID attribute.</summary>
  public const int PodPrinterId_MaxLength = 8;

  /// <summary>
  /// The value of the PRINTER_ID attribute.
  /// User defined printer id. Unique to the system. Must be the actual printer 
  /// id that a program can use to print to.
  /// </summary>
  [JsonPropertyName("podPrinterId")]
  [Member(Index = 10, Type = MemberType.Char, Length = PodPrinterId_MaxLength, Optional
    = true)]
  public string PodPrinterId
  {
    get => podPrinterId;
    set => podPrinterId = value != null
      ? TrimEnd(Substring(value, 1, PodPrinterId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("infId")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 9)]
  public int InfId
  {
    get => infId;
    set => infId = value;
  }

  private string printSucessfulIndicator;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatdTstamp;
  private DateTime? fieldValuesArchiveDate;
  private string fieldValuesArchiveInd;
  private DateTime? docEffectiveDte;
  private string docName;
  private string podPrinterId;
  private int infId;
}
