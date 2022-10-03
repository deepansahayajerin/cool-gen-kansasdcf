// The source file: FIELD_VALUE, ID: 371422033, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Contains the data values and the optional clauses for a specific document 
/// awaiting printing.
/// </summary>
[Serializable]
public partial class FieldValue: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FieldValue()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FieldValue(FieldValue that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FieldValue Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FieldValue that)
  {
    base.Assign(that);
    value = that.value;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatdTstamp = that.lastUpdatdTstamp;
    infIdentifier = that.infIdentifier;
    docEffectiveDte = that.docEffectiveDte;
    docName = that.docName;
    fldName = that.fldName;
  }

  /// <summary>Length of the VALUE attribute.</summary>
  public const int Value_MaxLength = 245;

  /// <summary>
  /// The value of the VALUE attribute.
  /// Contains the concatenation of data, delimited by #, required by a document
  /// instance.
  /// </summary>
  [JsonPropertyName("value")]
  [Member(Index = 1, Type = MemberType.Varchar, Length = Value_MaxLength, Optional
    = true)]
  public string Value
  {
    get => value;
    set => this.value = value != null ? Substring(value, 1, Value_MaxLength) : null
      ;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
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
  /// Timestamp of creation of the occurrence.
  /// 	
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
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  /// The value of the LAST_UPDATD_TSTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatdTstamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatdTstamp
  {
    get => lastUpdatdTstamp;
    set => lastUpdatdTstamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("infIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 9)]
  public int InfIdentifier
  {
    get => infIdentifier;
    set => infIdentifier = value;
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
  [Member(Index = 7, Type = MemberType.Date)]
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = DocName_MaxLength)]
  public string DocName
  {
    get => docName ?? "";
    set => docName = TrimEnd(Substring(value, 1, DocName_MaxLength));
  }

  /// <summary>
  /// The json value of the DocName attribute.</summary>
  [JsonPropertyName("docName")]
  [Computed]
  public string DocName_Json
  {
    get => NullIf(DocName, "");
    set => DocName = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int FldName_MaxLength = 10;

  /// <summary>
  /// The value of the NAME attribute.
  /// RESP: KESSEP
  /// 
  /// Identifier for Field.  Non-descriptive name wich is matched to
  /// fields in the WordPerfect (R) template.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = FldName_MaxLength)]
  public string FldName
  {
    get => fldName ?? "";
    set => fldName = TrimEnd(Substring(value, 1, FldName_MaxLength));
  }

  /// <summary>
  /// The json value of the FldName attribute.</summary>
  [JsonPropertyName("fldName")]
  [Computed]
  public string FldName_Json
  {
    get => NullIf(FldName, "");
    set => FldName = value;
  }

  private string value;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatdTstamp;
  private int infIdentifier;
  private DateTime? docEffectiveDte;
  private string docName;
  private string fldName;
}
