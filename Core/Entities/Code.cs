// The source file: CODE, ID: 371421437, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// An entity type which contains allowed values and validations for allowed 
/// values so that they can be done in a generic manner.
/// </summary>
[Serializable]
public partial class Code: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Code()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Code(Code that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Code Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Code that)
  {
    base.Assign(that);
    id = that.id;
    codeName = that.codeName;
    effectiveDate = that.effectiveDate;
    expirationDate = that.expirationDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    displayTitle = that.displayTitle;
  }

  /// <summary>
  /// The value of the ID attribute.
  /// Attribute to uniquely identify one instance of the CODE entity type.
  /// </summary>
  [JsonPropertyName("id")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 8)]
  public int Id
  {
    get => id;
    set => id = value;
  }

  /// <summary>Length of the CODE_NAME attribute.</summary>
  public const int CodeName_MaxLength = 32;

  /// <summary>
  /// The value of the CODE_NAME attribute.
  /// Code/ Attribute Name.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CodeName_MaxLength)]
  public string CodeName
  {
    get => codeName ?? "";
    set => codeName = TrimEnd(Substring(value, 1, CodeName_MaxLength));
  }

  /// <summary>
  /// The json value of the CodeName attribute.</summary>
  [JsonPropertyName("codeName")]
  [Computed]
  public string CodeName_Json
  {
    get => NullIf(CodeName, "");
    set => CodeName = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// Date the CODE is effective from (including it).
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the EXPIRATION_DATE attribute.
  /// The date upto and including which the CODE is effective.
  /// </summary>
  [JsonPropertyName("expirationDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? ExpirationDate
  {
    get => expirationDate;
    set => expirationDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
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
  [Member(Index = 7, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the DISPLAY_TITLE attribute.</summary>
  public const int DisplayTitle_MaxLength = 40;

  /// <summary>
  /// The value of the DISPLAY_TITLE attribute.
  /// Title used in display (since attribute name cannot be used directly as 
  /// display title).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Varchar, Length = DisplayTitle_MaxLength)]
    
  public string DisplayTitle
  {
    get => displayTitle ?? "";
    set => displayTitle = Substring(value, 1, DisplayTitle_MaxLength);
  }

  /// <summary>
  /// The json value of the DisplayTitle attribute.</summary>
  [JsonPropertyName("displayTitle")]
  [Computed]
  public string DisplayTitle_Json
  {
    get => NullIf(DisplayTitle, "");
    set => DisplayTitle = value;
  }

  private int id;
  private string codeName;
  private DateTime? effectiveDate;
  private DateTime? expirationDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string displayTitle;
}
