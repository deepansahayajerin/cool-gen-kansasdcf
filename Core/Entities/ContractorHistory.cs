// The source file: CONTRACTOR_HISTORY, ID: 1625341091, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// The history judicial districts and the time periods that were covered by 
/// various venders.
/// </summary>
[Serializable]
public partial class ContractorHistory: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ContractorHistory()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ContractorHistory(ContractorHistory that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ContractorHistory Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ContractorHistory that)
  {
    base.Assign(that);
    identifier = that.identifier;
    createdBy = that.createdBy;
    createdTimestmp = that.createdTimestmp;
    effectiveDate = that.effectiveDate;
    endDate = that.endDate;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdateTimestmp = that.lastUpdateTimestmp;
    fkCktCseOrgatypeCode = that.fkCktCseOrgatypeCode;
    fkCktCseOrgaorganztnId = that.fkCktCseOrgaorganztnId;
    fk0CktCseOrgatypeCode = that.fk0CktCseOrgatypeCode;
    fk0CktCseOrgaorganztnId = that.fk0CktCseOrgaorganztnId;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The system generated number that identifies a contractor and judicial 
  /// district.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// Who created the attorney record.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 2, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTMP attribute.
  /// This is the date and time when this record was created.
  /// </summary>
  [JsonPropertyName("createdTimestmp")]
  [Member(Index = 3, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? CreatedTimestmp
  {
    get => createdTimestmp;
    set => createdTimestmp = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date the record becomes active.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date of the record became inactive
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The last person who updated record.
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
  /// The value of the LAST_UPDATE_TIMESTMP attribute.
  /// This is the date and time when this record was updated.
  /// 	
  /// </summary>
  [JsonPropertyName("lastUpdateTimestmp")]
  [Member(Index = 7, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdateTimestmp
  {
    get => lastUpdateTimestmp;
    set => lastUpdateTimestmp = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int FkCktCseOrgatypeCode_MaxLength = 1;

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
  [Member(Index = 8, Type = MemberType.Char, Length
    = FkCktCseOrgatypeCode_MaxLength)]
  public string FkCktCseOrgatypeCode
  {
    get => fkCktCseOrgatypeCode ?? "";
    set => fkCktCseOrgatypeCode =
      TrimEnd(Substring(value, 1, FkCktCseOrgatypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the FkCktCseOrgatypeCode attribute.</summary>
  [JsonPropertyName("fkCktCseOrgatypeCode")]
  [Computed]
  public string FkCktCseOrgatypeCode_Json
  {
    get => NullIf(FkCktCseOrgatypeCode, "");
    set => FkCktCseOrgatypeCode = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int FkCktCseOrgaorganztnId_MaxLength = 2;

  /// <summary>
  /// The value of the CODE attribute.
  /// Identifies the CSE organization.  It distinguishes one occurrence of the 
  /// entity type from another.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = FkCktCseOrgaorganztnId_MaxLength)]
  public string FkCktCseOrgaorganztnId
  {
    get => fkCktCseOrgaorganztnId ?? "";
    set => fkCktCseOrgaorganztnId =
      TrimEnd(Substring(value, 1, FkCktCseOrgaorganztnId_MaxLength));
  }

  /// <summary>
  /// The json value of the FkCktCseOrgaorganztnId attribute.</summary>
  [JsonPropertyName("fkCktCseOrgaorganztnId")]
  [Computed]
  public string FkCktCseOrgaorganztnId_Json
  {
    get => NullIf(FkCktCseOrgaorganztnId, "");
    set => FkCktCseOrgaorganztnId = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Fk0CktCseOrgatypeCode_MaxLength = 1;

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
  [Member(Index = 10, Type = MemberType.Char, Length
    = Fk0CktCseOrgatypeCode_MaxLength)]
  public string Fk0CktCseOrgatypeCode
  {
    get => fk0CktCseOrgatypeCode ?? "";
    set => fk0CktCseOrgatypeCode =
      TrimEnd(Substring(value, 1, Fk0CktCseOrgatypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the Fk0CktCseOrgatypeCode attribute.</summary>
  [JsonPropertyName("fk0CktCseOrgatypeCode")]
  [Computed]
  public string Fk0CktCseOrgatypeCode_Json
  {
    get => NullIf(Fk0CktCseOrgatypeCode, "");
    set => Fk0CktCseOrgatypeCode = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int Fk0CktCseOrgaorganztnId_MaxLength = 2;

  /// <summary>
  /// The value of the CODE attribute.
  /// Identifies the CSE organization.  It distinguishes one occurrence of the 
  /// entity type from another.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = Fk0CktCseOrgaorganztnId_MaxLength)]
  public string Fk0CktCseOrgaorganztnId
  {
    get => fk0CktCseOrgaorganztnId ?? "";
    set => fk0CktCseOrgaorganztnId =
      TrimEnd(Substring(value, 1, Fk0CktCseOrgaorganztnId_MaxLength));
  }

  /// <summary>
  /// The json value of the Fk0CktCseOrgaorganztnId attribute.</summary>
  [JsonPropertyName("fk0CktCseOrgaorganztnId")]
  [Computed]
  public string Fk0CktCseOrgaorganztnId_Json
  {
    get => NullIf(Fk0CktCseOrgaorganztnId, "");
    set => Fk0CktCseOrgaorganztnId = value;
  }

  private int identifier;
  private string createdBy;
  private DateTime? createdTimestmp;
  private DateTime? effectiveDate;
  private DateTime? endDate;
  private string lastUpdatedBy;
  private DateTime? lastUpdateTimestmp;
  private string fkCktCseOrgatypeCode;
  private string fkCktCseOrgaorganztnId;
  private string fk0CktCseOrgatypeCode;
  private string fk0CktCseOrgaorganztnId;
}
