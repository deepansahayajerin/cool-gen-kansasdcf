// The source file: TYPE_STATUS_AUDIT, ID: 371440424, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This designer added entity type holds before images of type and status 
/// entities for all such entity types.
/// </summary>
[Serializable]
public partial class TypeStatusAudit: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public TypeStatusAudit()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public TypeStatusAudit(TypeStatusAudit that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new TypeStatusAudit Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(TypeStatusAudit that)
  {
    base.Assign(that);
    stringOfOthers = that.stringOfOthers;
    auditedBy = that.auditedBy;
    auditTimestamp = that.auditTimestamp;
    tableName = that.tableName;
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    code = that.code;
    name = that.name;
    description = that.description;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
  }

  /// <summary>Length of the STRING_OF_OTHERS attribute.</summary>
  public const int StringOfOthers_MaxLength = 20;

  /// <summary>
  /// The value of the STRING_OF_OTHERS attribute.
  /// This attribute will hold the string of other attributes that may be 
  /// specific to a particular type or status entity type.
  /// It was designed to be this way since those situations were very rare.
  /// </summary>
  [JsonPropertyName("stringOfOthers")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = StringOfOthers_MaxLength, Optional = true)]
  public string StringOfOthers
  {
    get => stringOfOthers;
    set => stringOfOthers = value != null
      ? TrimEnd(Substring(value, 1, StringOfOthers_MaxLength)) : null;
  }

  /// <summary>Length of the AUDITED_BY attribute.</summary>
  public const int AuditedBy_MaxLength = 8;

  /// <summary>
  /// The value of the AUDITED_BY attribute.
  /// The user id or other system id of the person or process logging the audit.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = AuditedBy_MaxLength)]
  public string AuditedBy
  {
    get => auditedBy ?? "";
    set => auditedBy = TrimEnd(Substring(value, 1, AuditedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the AuditedBy attribute.</summary>
  [JsonPropertyName("auditedBy")]
  [Computed]
  public string AuditedBy_Json
  {
    get => NullIf(AuditedBy, "");
    set => AuditedBy = value;
  }

  /// <summary>
  /// The value of the AUDIT_TIMESTAMP attribute.
  /// The timestamp of when the audit was logged.
  /// </summary>
  [JsonPropertyName("auditTimestamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? AuditTimestamp
  {
    get => auditTimestamp;
    set => auditTimestamp = value;
  }

  /// <summary>Length of the TABLE_NAME attribute.</summary>
  public const int TableName_MaxLength = 32;

  /// <summary>
  /// The value of the TABLE_NAME attribute.
  /// The name of the entity type being audited.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = TableName_MaxLength)]
  public string TableName
  {
    get => tableName ?? "";
    set => tableName = TrimEnd(Substring(value, 1, TableName_MaxLength));
  }

  /// <summary>
  /// The json value of the TableName attribute.</summary>
  [JsonPropertyName("tableName")]
  [Computed]
  public string TableName_Json
  {
    get => NullIf(TableName, "");
    set => TableName = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 3)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int Code_MaxLength = 10;

  /// <summary>
  /// The value of the CODE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// </summary>
  [JsonPropertyName("code")]
  [Member(Index = 6, Type = MemberType.Char, Length = Code_MaxLength, Optional
    = true)]
  public string Code
  {
    get => code;
    set => code = value != null
      ? TrimEnd(Substring(value, 1, Code_MaxLength)) : null;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 40;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the code.  This name will be more descriptive than the code 
  /// but will be much less shorter than the description.  The reason for
  /// needing the name is that the code is not descriptive enough and the
  /// description is too long for the list screens.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Name_MaxLength)]
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

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// An explanation of the entity type.  The description should be specific 
  /// enough to allow a person to distinguish/understand the entity type.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 8, Type = MemberType.Varchar, Length
    = Description_MaxLength, Optional = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? Substring(value, 1, Description_MaxLength) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence is activated by the system.  An 
  /// effective date allows the entity to be entered into the system with a
  /// future date.  The occurrence of the entity will &quot;take effect&quot; on
  /// the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The date upon which this occurrence of the entity can no longer be used.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 11, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 12, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 14, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  private string stringOfOthers;
  private string auditedBy;
  private DateTime? auditTimestamp;
  private string tableName;
  private int systemGeneratedIdentifier;
  private string code;
  private string name;
  private string description;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
}
