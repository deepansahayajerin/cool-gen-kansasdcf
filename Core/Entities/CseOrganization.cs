// The source file: CSE_ORGANIZATION, ID: 371432782, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// A non overlapping geographical partitioning of the CSE part of the State of 
/// Kansas based on:
/// Division: a CSE Divisional Collection manager's Regions of responsibility.
/// Division 1( Hays, Salinina, and Wichita.)
/// Divsion 2 ( KC,NE, SE, and Topeka)
/// REGION: A CSE Chiefs counties of responsibility.
/// The 7 current CSE regions (NE,SE,Wichita, Salina, Hayes, KC, Topeka)
/// AREA: For use by income maintenance. These are not required for CSE 
/// assignment purposes. Their are currently 12 area offices. These are not the
/// same as CSE offices, regions or divisions.
/// This entity does not include courts and offices.
/// </summary>
[Serializable]
public partial class CseOrganization: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CseOrganization()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CseOrganization(CseOrganization that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CseOrganization Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CseOrganization that)
  {
    base.Assign(that);
    taxSuffix = that.taxSuffix;
    code = that.code;
    type1 = that.type1;
    name = that.name;
    taxId = that.taxId;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    lastUpdatdTstamp = that.lastUpdatdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
  }

  /// <summary>
  /// The value of the TAX_SUFFIX attribute.
  /// A suffix to Tax_ID for a county.
  /// </summary>
  [JsonPropertyName("taxSuffix")]
  [Member(Index = 1, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? TaxSuffix
  {
    get => taxSuffix;
    set => taxSuffix = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int Code_MaxLength = 2;

  /// <summary>
  /// The value of the CODE attribute.
  /// Identifies the CSE organization.  It distinguishes one occurrence of the 
  /// entity type from another.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Code_MaxLength)]
  public string Code
  {
    get => code ?? "";
    set => code = TrimEnd(Substring(value, 1, Code_MaxLength));
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

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// REGION
  /// DIVISION
  /// COUNTY
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Type1_MaxLength)]
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

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 20;

  /// <summary>
  /// The value of the NAME attribute.
  /// the name of the organizational level.
  /// ex: Name of county, division, region
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Name_MaxLength)]
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

  /// <summary>Length of the TAX_ID attribute.</summary>
  public const int TaxId_MaxLength = 9;

  /// <summary>
  /// The value of the TAX_ID attribute.
  /// This is for a tax id when needed.  The state has a tax id.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = TaxId_MaxLength)]
  public string TaxId
  {
    get => taxId ?? "";
    set => taxId = TrimEnd(Substring(value, 1, TaxId_MaxLength));
  }

  /// <summary>
  /// The json value of the TaxId attribute.</summary>
  [JsonPropertyName("taxId")]
  [Computed]
  public string TaxId_Json
  {
    get => NullIf(TaxId, "");
    set => TaxId = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the LAST_UPDATD_TSTAMP attribute.
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatdTstamp")]
  [Member(Index = 8, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatdTstamp
  {
    get => lastUpdatdTstamp;
    set => lastUpdatdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 9, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  private int? taxSuffix;
  private string code;
  private string type1;
  private string name;
  private string taxId;
  private DateTime? createdTimestamp;
  private string createdBy;
  private DateTime? lastUpdatdTstamp;
  private string lastUpdatedBy;
}
