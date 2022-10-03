// The source file: DOCUMENT_FIELD, ID: 371429745, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: KESSEP
/// 
/// The occurence of a FIELD on a DOCUMENT (associate entity)
/// </summary>
[Serializable]
public partial class DocumentField: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DocumentField()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DocumentField(DocumentField that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DocumentField Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DocumentField that)
  {
    base.Assign(that);
    position = that.position;
    requiredSwitch = that.requiredSwitch;
    screenPrompt = that.screenPrompt;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    docEffectiveDte = that.docEffectiveDte;
    docName = that.docName;
    fldName = that.fldName;
  }

  /// <summary>
  /// The value of the POSITION attribute.
  /// RESP: KESSEP
  /// 
  /// Denotes the order the FIELD_VALUES will be displayed on DDOC.  This
  /// value does not have to be unique in the document.  This allows grouping
  /// similar fields.
  /// </summary>
  [JsonPropertyName("position")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Position
  {
    get => position;
    set => position = value;
  }

  /// <summary>Length of the REQUIRED_SWITCH attribute.</summary>
  public const int RequiredSwitch_MaxLength = 1;

  /// <summary>
  /// The value of the REQUIRED_SWITCH attribute.
  /// RESP: KESSEP
  /// 
  /// Denotes that the field is required on the document, or that the document
  /// can be printed even if the FIELD_VALUE does not exist.  Values: Y --
  /// field is required on the document                                     N
  /// -- filed is not required for the document
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = RequiredSwitch_MaxLength)]
  public string RequiredSwitch
  {
    get => requiredSwitch ?? "";
    set => requiredSwitch =
      TrimEnd(Substring(value, 1, RequiredSwitch_MaxLength));
  }

  /// <summary>
  /// The json value of the RequiredSwitch attribute.</summary>
  [JsonPropertyName("requiredSwitch")]
  [Computed]
  public string RequiredSwitch_Json
  {
    get => NullIf(RequiredSwitch, "");
    set => RequiredSwitch = value;
  }

  /// <summary>Length of the SCREEN_PROMPT attribute.</summary>
  public const int ScreenPrompt_MaxLength = 20;

  /// <summary>
  /// The value of the SCREEN_PROMPT attribute.
  /// RESP: KESSEP
  /// 
  /// Used on DDOC to provide the user an idea of what a FIELD_VALUE is to
  /// contain during the actual print process.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = ScreenPrompt_MaxLength)]
  public string ScreenPrompt
  {
    get => screenPrompt ?? "";
    set => screenPrompt = TrimEnd(Substring(value, 1, ScreenPrompt_MaxLength));
  }

  /// <summary>
  /// The json value of the ScreenPrompt attribute.</summary>
  [JsonPropertyName("screenPrompt")]
  [Computed]
  public string ScreenPrompt_Json
  {
    get => NullIf(ScreenPrompt, "");
    set => ScreenPrompt = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
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
  /// Timestamp of creation of the occurrence.
  /// 	
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
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 7, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
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
  [Member(Index = 8, Type = MemberType.Date)]
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
  [Member(Index = 9, Type = MemberType.Char, Length = DocName_MaxLength)]
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
  [Member(Index = 10, Type = MemberType.Char, Length = FldName_MaxLength)]
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

  private int position;
  private string requiredSwitch;
  private string screenPrompt;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private DateTime? docEffectiveDte;
  private string docName;
  private string fldName;
}
