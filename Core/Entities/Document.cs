// The source file: DOCUMENT, ID: 371422006, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Defines all possible documents in KESSEP. Contains the name and description 
/// of the document.
/// </summary>
[Serializable]
public partial class Document: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Document()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Document(Document that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Document Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Document that)
  {
    base.Assign(that);
    name = that.name;
    type1 = that.type1;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatdTstamp = that.lastUpdatdTstamp;
    businessObject = that.businessObject;
    requiredResponseDays = that.requiredResponseDays;
    effectiveDate = that.effectiveDate;
    expirationDate = that.expirationDate;
    printPreviewSwitch = that.printPreviewSwitch;
    versionNumber = that.versionNumber;
    description = that.description;
    detailedDescription = that.detailedDescription;
    evdId = that.evdId;
    eveNo = that.eveNo;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 8;

  /// <summary>
  /// The value of the NAME attribute.
  /// Eight char user defined name for this notice. eg CSE-201
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Name_MaxLength)]
  public string Name
  {
    get => name ?? "";
    set => name = TrimEnd(Substring(value, 1, Name_MaxLength));
  }

  /// <summary>
  /// The json value of the Name attribute.</summary>
  [JsonPropertyName("name")]
  [Computed]
  public string Name_Json
  {
    get => NullIf(Name, "");
    set => Name = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 4;

  /// <summary>
  /// The value of the TYPE attribute.
  /// RESP: KESSEP
  /// 
  /// User friendly trancode of the screen from which this document can be
  /// printed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Type1_MaxLength)]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
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
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 4, Type = MemberType.Timestamp)]
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
  [Member(Index = 5, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
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
  [Member(Index = 6, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatdTstamp
  {
    get => lastUpdatdTstamp;
    set => lastUpdatdTstamp = value;
  }

  /// <summary>Length of the BUSINESS_OBJECT attribute.</summary>
  public const int BusinessObject_MaxLength = 3;

  /// <summary>
  /// The value of the BUSINESS_OBJECT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = BusinessObject_MaxLength)]
  public string BusinessObject
  {
    get => businessObject ?? "";
    set => businessObject =
      TrimEnd(Substring(value, 1, BusinessObject_MaxLength));
  }

  /// <summary>
  /// The json value of the BusinessObject attribute.</summary>
  [JsonPropertyName("businessObject")]
  [Computed]
  public string BusinessObject_Json
  {
    get => NullIf(BusinessObject, "");
    set => BusinessObject = value;
  }

  /// <summary>
  /// The value of the REQUIRED_RESPONSE_DAYS attribute.
  /// </summary>
  [JsonPropertyName("requiredResponseDays")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 4)]
  public int RequiredResponseDays
  {
    get => requiredResponseDays;
    set => requiredResponseDays = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// RESP: KESSEP
  /// 
  /// Effective date of the document.  A document could have many versions, but
  /// only one version is current on a given day.  On ADD, is set to system
  /// current date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the EXPIRATION_DATE attribute.
  /// RESP: KESSEP
  /// 
  /// Expiration date of the document. A document could have many versions,
  /// but only on version is current on a given day.  On ADD, defaults to
  /// system maximum date.
  /// </summary>
  [JsonPropertyName("expirationDate")]
  [Member(Index = 10, Type = MemberType.Date)]
  public DateTime? ExpirationDate
  {
    get => expirationDate;
    set => expirationDate = value;
  }

  /// <summary>Length of the PRINT_PREVIEW_SWITCH attribute.</summary>
  public const int PrintPreviewSwitch_MaxLength = 1;

  /// <summary>
  /// The value of the PRINT_PREVIEW_SWITCH attribute.
  /// RESP: KESSEP
  /// 
  /// Allows user to decide to show DDOC prior to a system Print for a given
  /// document.  Values: Y -- show DDOC before printing
  /// 
  /// N -- do not show DDOC if possible
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = PrintPreviewSwitch_MaxLength)]
  public string PrintPreviewSwitch
  {
    get => printPreviewSwitch ?? "";
    set => printPreviewSwitch =
      TrimEnd(Substring(value, 1, PrintPreviewSwitch_MaxLength));
  }

  /// <summary>
  /// The json value of the PrintPreviewSwitch attribute.</summary>
  [JsonPropertyName("printPreviewSwitch")]
  [Computed]
  public string PrintPreviewSwitch_Json
  {
    get => NullIf(PrintPreviewSwitch, "");
    set => PrintPreviewSwitch = value;
  }

  /// <summary>Length of the VERSION_NUMBER attribute.</summary>
  public const int VersionNumber_MaxLength = 3;

  /// <summary>
  /// The value of the VERSION_NUMBER attribute.
  /// RESP: KESSEP
  /// 
  /// Allows system to determine which version of the WordPerfect (R) templete
  /// should be used when printing and reprinting this document.  This is used
  /// in the download script, which will look for xxxxxxxx.yyy; where xxxxxxxx
  /// is the document name and yyy is the version number.
  /// 
  /// For example, if DOCUMENT_NAME = 'docA' and VERSION_NUMBER = '003', the
  /// download script will look for a WordPerfect (R) templete named docA.003.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = VersionNumber_MaxLength)]
  public string VersionNumber
  {
    get => versionNumber ?? "";
    set => versionNumber =
      TrimEnd(Substring(value, 1, VersionNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the VersionNumber attribute.</summary>
  [JsonPropertyName("versionNumber")]
  [Computed]
  public string VersionNumber_Json
  {
    get => NullIf(VersionNumber, "");
    set => VersionNumber = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 40;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// Brief Desc about this document.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 13, Type = MemberType.Char, Length = Description_MaxLength, Optional
    = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? TrimEnd(Substring(value, 1, Description_MaxLength)) : null;
  }

  /// <summary>Length of the DETAILED_DESCRIPTION attribute.</summary>
  public const int DetailedDescription_MaxLength = 240;

  /// <summary>
  /// The value of the DETAILED_DESCRIPTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Varchar, Length
    = DetailedDescription_MaxLength)]
  public string DetailedDescription
  {
    get => detailedDescription ?? "";
    set => detailedDescription =
      Substring(value, 1, DetailedDescription_MaxLength);
  }

  /// <summary>
  /// The json value of the DetailedDescription attribute.</summary>
  [JsonPropertyName("detailedDescription")]
  [Computed]
  public string DetailedDescription_Json
  {
    get => NullIf(DetailedDescription, "");
    set => DetailedDescription = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("evdId")]
  [Member(Index = 15, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? EvdId
  {
    get => evdId;
    set => evdId = value;
  }

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("eveNo")]
  [Member(Index = 16, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? EveNo
  {
    get => eveNo;
    set => eveNo = value;
  }

  private string name;
  private string type1;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatdTstamp;
  private string businessObject;
  private int requiredResponseDays;
  private DateTime? effectiveDate;
  private DateTime? expirationDate;
  private string printPreviewSwitch;
  private string versionNumber;
  private string description;
  private string detailedDescription;
  private int? evdId;
  private int? eveNo;
}
