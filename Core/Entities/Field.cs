// The source file: FIELD, ID: 371429761, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: KESSEP
/// 
/// Code name of a field to be used on documents.  The NAME attribute
/// matches the field name found in the WordPerfect (R)  templates.  Each
/// field will only be found on a document once.  The exception occurs in
/// User Input fields, wich can occur repeatedly per document.
/// </summary>
[Serializable]
public partial class Field: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Field()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Field(Field that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Field Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Field that)
  {
    base.Assign(that);
    name = that.name;
    dependancy = that.dependancy;
    subroutineName = that.subroutineName;
    screenName = that.screenName;
    description = that.description;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 10;

  /// <summary>
  /// The value of the NAME attribute.
  /// RESP: KESSEP
  /// 
  /// Identifier for Field.  Non-descriptive name wich is matched to
  /// fields in the WordPerfect (R) template.
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

  /// <summary>Length of the DEPENDANCY attribute.</summary>
  public const int Dependancy_MaxLength = 8;

  /// <summary>
  /// The value of the DEPENDANCY attribute.
  /// RESP: KESSEP
  /// 
  /// Coupled with Subroutine_name, helps data retrieval CAB determine which
  /// Key Fields are required to retrieve a Field_value.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Dependancy_MaxLength)]
  public string Dependancy
  {
    get => dependancy ?? "";
    set => dependancy = TrimEnd(Substring(value, 1, Dependancy_MaxLength));
  }

  /// <summary>
  /// The json value of the Dependancy attribute.</summary>
  [JsonPropertyName("dependancy")]
  [Computed]
  public string Dependancy_Json
  {
    get => NullIf(Dependancy, "");
    set => Dependancy = value;
  }

  /// <summary>Length of the SUBROUTINE_NAME attribute.</summary>
  public const int SubroutineName_MaxLength = 8;

  /// <summary>
  /// The value of the SUBROUTINE_NAME attribute.
  /// RESP: KESSEP
  /// 
  /// Coupled with Dependancy, helps data retrieval CAB determine wich Key
  /// Fields are required to retrieve a Field_Value.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = SubroutineName_MaxLength)]
  public string SubroutineName
  {
    get => subroutineName ?? "";
    set => subroutineName =
      TrimEnd(Substring(value, 1, SubroutineName_MaxLength));
  }

  /// <summary>
  /// The json value of the SubroutineName attribute.</summary>
  [JsonPropertyName("subroutineName")]
  [Computed]
  public string SubroutineName_Json
  {
    get => NullIf(SubroutineName, "");
    set => SubroutineName = value;
  }

  /// <summary>Length of the SCREEN_NAME attribute.</summary>
  public const int ScreenName_MaxLength = 4;

  /// <summary>
  /// The value of the SCREEN_NAME attribute.
  /// RESP: KESSEP
  /// 
  /// The name of the KESSEP screen where the user can enter the required
  /// data for the field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = ScreenName_MaxLength)]
  public string ScreenName
  {
    get => screenName ?? "";
    set => screenName = TrimEnd(Substring(value, 1, ScreenName_MaxLength));
  }

  /// <summary>
  /// The json value of the ScreenName attribute.</summary>
  [JsonPropertyName("screenName")]
  [Computed]
  public string ScreenName_Json
  {
    get => NullIf(ScreenName, "");
    set => ScreenName = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 38;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// RESP: KESSEP
  /// 
  /// Description of Field.  Used to help table maintenance person form CSE
  /// staff.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = Description_MaxLength)]
  public string Description
  {
    get => description ?? "";
    set => description = TrimEnd(Substring(value, 1, Description_MaxLength));
  }

  /// <summary>
  /// The json value of the Description attribute.</summary>
  [JsonPropertyName("description")]
  [Computed]
  public string Description_Json
  {
    get => NullIf(Description, "");
    set => Description = value;
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

  private string name;
  private string dependancy;
  private string subroutineName;
  private string screenName;
  private string description;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
}
