// The source file: PGM_NAME_TABLE, ID: 371439541, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: BATCH DESIGNER				This Entity Type will contain information about all 
/// KESSEP Batch Program Runs that are active and those that have been used but
/// are no longer active.
/// </summary>
[Serializable]
public partial class PgmNameTable: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PgmNameTable()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PgmNameTable(PgmNameTable that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PgmNameTable Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PgmNameTable that)
  {
    base.Assign(that);
    lastRunDate = that.lastRunDate;
    pgmName = that.pgmName;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    updatedTimestamp = that.updatedTimestamp;
    pgmActive = that.pgmActive;
    pgmParmList = that.pgmParmList;
    pgmDescription = that.pgmDescription;
    pgmType = that.pgmType;
    updatedBy = that.updatedBy;
  }

  /// <summary>
  /// The value of the LAST_RUN_DATE attribute.
  /// This is the last date that this record was processed. If equal to current 
  /// date this record will be skipped.
  /// </summary>
  [JsonPropertyName("lastRunDate")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? LastRunDate
  {
    get => lastRunDate;
    set => lastRunDate = value;
  }

  /// <summary>Length of the PGM_NAME attribute.</summary>
  public const int PgmName_MaxLength = 18;

  /// <summary>
  /// The value of the PGM_NAME attribute.
  /// This is the name of an active or deactive kessep batch program run name. (
  /// example) (srrun033).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = PgmName_MaxLength)]
  public string PgmName
  {
    get => pgmName ?? "";
    set => pgmName = TrimEnd(Substring(value, 1, PgmName_MaxLength));
  }

  /// <summary>
  /// The json value of the PgmName attribute.</summary>
  [JsonPropertyName("pgmName")]
  [Computed]
  public string PgmName_Json
  {
    get => NullIf(PgmName, "");
    set => PgmName = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
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
  /// The value of the UPDATED_TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("updatedTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? UpdatedTimestamp
  {
    get => updatedTimestamp;
    set => updatedTimestamp = value;
  }

  /// <summary>Length of the PGM_ACTIVE attribute.</summary>
  public const int PgmActive_MaxLength = 1;

  /// <summary>
  /// The value of the PGM_ACTIVE attribute.
  /// Is this an active batch program run(Y/N).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = PgmActive_MaxLength)]
  public string PgmActive
  {
    get => pgmActive ?? "";
    set => pgmActive = TrimEnd(Substring(value, 1, PgmActive_MaxLength));
  }

  /// <summary>
  /// The json value of the PgmActive attribute.</summary>
  [JsonPropertyName("pgmActive")]
  [Computed]
  public string PgmActive_Json
  {
    get => NullIf(PgmActive, "");
    set => PgmActive = value;
  }

  /// <summary>Length of the PGM_PARM_LIST attribute.</summary>
  public const int PgmParmList_MaxLength = 100;

  /// <summary>
  /// The value of the PGM_PARM_LIST attribute.
  /// </summary>
  [JsonPropertyName("pgmParmList")]
  [Member(Index = 7, Type = MemberType.Char, Length = PgmParmList_MaxLength, Optional
    = true)]
  public string PgmParmList
  {
    get => pgmParmList;
    set => pgmParmList = value != null
      ? TrimEnd(Substring(value, 1, PgmParmList_MaxLength)) : null;
  }

  /// <summary>Length of the PGM_DESCRIPTION attribute.</summary>
  public const int PgmDescription_MaxLength = 240;

  /// <summary>
  /// The value of the PGM_DESCRIPTION attribute.
  /// Short description of batch program.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = PgmDescription_MaxLength)]
  public string PgmDescription
  {
    get => pgmDescription ?? "";
    set => pgmDescription =
      TrimEnd(Substring(value, 1, PgmDescription_MaxLength));
  }

  /// <summary>
  /// The json value of the PgmDescription attribute.</summary>
  [JsonPropertyName("pgmDescription")]
  [Computed]
  public string PgmDescription_Json
  {
    get => NullIf(PgmDescription, "");
    set => PgmDescription = value;
  }

  /// <summary>Length of the PGM_TYPE attribute.</summary>
  public const int PgmType_MaxLength = 3;

  /// <summary>
  /// The value of the PGM_TYPE attribute.
  /// Defines the type of program run (example) (mon=monthly, wek=weekly, 
  /// day=daily).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = PgmType_MaxLength)]
  public string PgmType
  {
    get => pgmType ?? "";
    set => pgmType = TrimEnd(Substring(value, 1, PgmType_MaxLength));
  }

  /// <summary>
  /// The json value of the PgmType attribute.</summary>
  [JsonPropertyName("pgmType")]
  [Computed]
  public string PgmType_Json
  {
    get => NullIf(PgmType, "");
    set => PgmType = value;
  }

  /// <summary>Length of the UPDATED_BY attribute.</summary>
  public const int UpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the UPDATED_BY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = UpdatedBy_MaxLength)]
  public string UpdatedBy
  {
    get => updatedBy ?? "";
    set => updatedBy = TrimEnd(Substring(value, 1, UpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the UpdatedBy attribute.</summary>
  [JsonPropertyName("updatedBy")]
  [Computed]
  public string UpdatedBy_Json
  {
    get => NullIf(UpdatedBy, "");
    set => UpdatedBy = value;
  }

  private DateTime? lastRunDate;
  private string pgmName;
  private DateTime? createdTimestamp;
  private string createdBy;
  private DateTime? updatedTimestamp;
  private string pgmActive;
  private string pgmParmList;
  private string pgmDescription;
  private string pgmType;
  private string updatedBy;
}
