// The source file: COUNTY_SERVICE, ID: 371432720, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// The CSE programs and or functions  provided by an OFFICE.  This is used for 
/// case assignment to determine the office that provides a particular service
/// for a given county.
/// </summary>
[Serializable]
public partial class CountyService: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CountyService()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CountyService(CountyService that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CountyService Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CountyService that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    type1 = that.type1;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatdTstamp = that.lastUpdatdTstamp;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    function = that.function;
    offGeneratedId = that.offGeneratedId;
    cogTypeCode = that.cogTypeCode;
    cogCode = that.cogCode;
    prgGeneratedId = that.prgGeneratedId;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Describes the status:
  /// Note: Program type or Function type
  /// each occurance serves only 1 type.
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

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 3, Type = MemberType.Date)]
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
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
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
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatdTstamp")]
  [Member(Index = 6, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatdTstamp
  {
    get => lastUpdatdTstamp;
    set => lastUpdatdTstamp = value;
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// the timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the FUNCTION attribute.</summary>
  public const int Function_MaxLength = 3;

  /// <summary>
  /// The value of the FUNCTION attribute.
  /// Identifies the federally defined function available for specific work by 
  /// Office Service Providers at the county described by this occurrence.
  /// </summary>
  [JsonPropertyName("function")]
  [Member(Index = 9, Type = MemberType.Char, Length = Function_MaxLength, Optional
    = true)]
  public string Function
  {
    get => function;
    set => function = value != null
      ? TrimEnd(Substring(value, 1, Function_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offGeneratedId")]
  [Member(Index = 10, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffGeneratedId
  {
    get => offGeneratedId;
    set => offGeneratedId = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CogTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// REGION
  /// DIVISION
  /// COUNTY
  /// </summary>
  [JsonPropertyName("cogTypeCode")]
  [Member(Index = 11, Type = MemberType.Char, Length = CogTypeCode_MaxLength, Optional
    = true)]
  public string CogTypeCode
  {
    get => cogTypeCode;
    set => cogTypeCode = value != null
      ? TrimEnd(Substring(value, 1, CogTypeCode_MaxLength)) : null;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int CogCode_MaxLength = 2;

  /// <summary>
  /// The value of the CODE attribute.
  /// Identifies the CSE organization.  It distinguishes one occurrence of the 
  /// entity type from another.
  /// </summary>
  [JsonPropertyName("cogCode")]
  [Member(Index = 12, Type = MemberType.Char, Length = CogCode_MaxLength, Optional
    = true)]
  public string CogCode
  {
    get => cogCode;
    set => cogCode = value != null
      ? TrimEnd(Substring(value, 1, CogCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// identifies the program
  /// </summary>
  [JsonPropertyName("prgGeneratedId")]
  [Member(Index = 13, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? PrgGeneratedId
  {
    get => prgGeneratedId;
    set => prgGeneratedId = value;
  }

  private int systemGeneratedIdentifier;
  private string type1;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string lastUpdatedBy;
  private DateTime? lastUpdatdTstamp;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string function;
  private int? offGeneratedId;
  private string cogTypeCode;
  private string cogCode;
  private int? prgGeneratedId;
}
