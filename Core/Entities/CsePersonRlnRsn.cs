// The source file: CSE_PERSON_RLN_RSN, ID: 371433302, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// The CSE Person RLN RSN entity describes the reason for relating two CSE 
/// Persons two each other.
/// Example:
/// CSE Person 1  'Is the Designated Payee for' CSE Person 2
/// </summary>
[Serializable]
public partial class CsePersonRlnRsn: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsePersonRlnRsn()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsePersonRlnRsn(CsePersonRlnRsn that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsePersonRlnRsn Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int SystemGeneratedIdentifier
  {
    get => Get<int?>("systemGeneratedIdentifier") ?? 0;
    set => Set("systemGeneratedIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int Code_MaxLength = 10;

  /// <summary>
  /// The value of the CODE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Code_MaxLength)]
  public string Code
  {
    get => Get<string>("code") ?? "";
    set => Set(
      "code", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Code_MaxLength)));
  }

  /// <summary>
  /// The json value of the Code attribute.</summary>
  [JsonPropertyName("code")]
  [Computed]
  public string Code_Json
  {
    get => NullIf(Code, "");
    set => Code = value;
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
  [Member(Index = 3, Type = MemberType.Char, Length = Name_MaxLength)]
  public string Name
  {
    get => Get<string>("name") ?? "";
    set => Set(
      "name", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Name_MaxLength)));
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
  /// An explanation of the business function.  The description should be 
  /// specific enough to allow a person to distinguish/understand the business
  /// function.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 4, Type = MemberType.Varchar, Length
    = Description_MaxLength, Optional = true)]
  public string Description
  {
    get => Get<string>("description");
    set => Set("description", Substring(value, 1, Description_MaxLength));
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => Get<DateTime?>("effectiveDate");
    set => Set("effectiveDate", value);
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The date upon which this occurrence of the entity can no longer be used.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => Get<DateTime?>("discontinueDate");
    set => Set("discontinueDate", value);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => Get<string>("createdBy") ?? "";
    set => Set(
      "createdBy", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CreatedBy_MaxLength)));
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
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The User ID or Program ID responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 9, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 10, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => Get<DateTime?>("lastUpdatedTmst");
    set => Set("lastUpdatedTmst", value);
  }
}
