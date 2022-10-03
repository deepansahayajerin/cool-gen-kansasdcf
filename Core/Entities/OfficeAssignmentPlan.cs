// The source file: OFFICE_ASSIGNMENT_PLAN, ID: 371438466, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// This entity allows an OFFICE to set up how it assigns service providers to 
/// cases.
/// By:
/// County
/// Program
/// Function(task)
/// Alpha
/// </summary>
[Serializable]
public partial class OfficeAssignmentPlan: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OfficeAssignmentPlan()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OfficeAssignmentPlan(OfficeAssignmentPlan that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OfficeAssignmentPlan Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(OfficeAssignmentPlan that)
  {
    base.Assign(that);
    assignmentType = that.assignmentType;
    countyAssignmentInd = that.countyAssignmentInd;
    alphaAssignmentInd = that.alphaAssignmentInd;
    functionAssignmentInd = that.functionAssignmentInd;
    programAssignmentInd = that.programAssignmentInd;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdTstamp = that.createdTstamp;
    createdBy = that.createdBy;
    lastUpdatdTstamp = that.lastUpdatdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    tribunalInd = that.tribunalInd;
    offGeneratedId = that.offGeneratedId;
  }

  /// <summary>Length of the ASSIGNMENT_TYPE attribute.</summary>
  public const int AssignmentType_MaxLength = 2;

  /// <summary>
  /// The value of the ASSIGNMENT_TYPE attribute.
  /// This attribute is used to allow multiple types of assignments to be 
  /// processed from the same table.
  /// 'CT' = Court Trustee - Attorney
  /// 'CC' = Case Coordinator - Collection Officer
  /// Used as an aid for the user to determine how this task relates to other 
  /// tasks.
  /// Rules are based on the type.
  /// Use the Set mnemonics table or generic code table
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = AssignmentType_MaxLength)]
  public string AssignmentType
  {
    get => assignmentType ?? "";
    set => assignmentType =
      TrimEnd(Substring(value, 1, AssignmentType_MaxLength));
  }

  /// <summary>
  /// The json value of the AssignmentType attribute.</summary>
  [JsonPropertyName("assignmentType")]
  [Computed]
  public string AssignmentType_Json
  {
    get => NullIf(AssignmentType, "");
    set => AssignmentType = value;
  }

  /// <summary>Length of the COUNTY_ASSIGNMENT_IND attribute.</summary>
  public const int CountyAssignmentInd_MaxLength = 1;

  /// <summary>
  /// The value of the COUNTY_ASSIGNMENT_IND attribute.
  /// this is for offices that serve more than 1 county and assigns cases based 
  /// on these counties.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = CountyAssignmentInd_MaxLength)]
  public string CountyAssignmentInd
  {
    get => countyAssignmentInd ?? "";
    set => countyAssignmentInd =
      TrimEnd(Substring(value, 1, CountyAssignmentInd_MaxLength));
  }

  /// <summary>
  /// The json value of the CountyAssignmentInd attribute.</summary>
  [JsonPropertyName("countyAssignmentInd")]
  [Computed]
  public string CountyAssignmentInd_Json
  {
    get => NullIf(CountyAssignmentInd, "");
    set => CountyAssignmentInd = value;
  }

  /// <summary>Length of the ALPHA_ASSIGNMENT_IND attribute.</summary>
  public const int AlphaAssignmentInd_MaxLength = 1;

  /// <summary>
  /// The value of the ALPHA_ASSIGNMENT_IND attribute.
  /// if this switch is on this office assigns cases based on alpha .
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = AlphaAssignmentInd_MaxLength)]
  public string AlphaAssignmentInd
  {
    get => alphaAssignmentInd ?? "";
    set => alphaAssignmentInd =
      TrimEnd(Substring(value, 1, AlphaAssignmentInd_MaxLength));
  }

  /// <summary>
  /// The json value of the AlphaAssignmentInd attribute.</summary>
  [JsonPropertyName("alphaAssignmentInd")]
  [Computed]
  public string AlphaAssignmentInd_Json
  {
    get => NullIf(AlphaAssignmentInd, "");
    set => AlphaAssignmentInd = value;
  }

  /// <summary>Length of the FUNCTION_ASSIGNMENT_IND attribute.</summary>
  public const int FunctionAssignmentInd_MaxLength = 1;

  /// <summary>
  /// The value of the FUNCTION_ASSIGNMENT_IND attribute.
  /// if this switch is on this office sets up a specialty for each collection 
  /// officer of function and assigns cases based on the specialty.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = FunctionAssignmentInd_MaxLength)]
  public string FunctionAssignmentInd
  {
    get => functionAssignmentInd ?? "";
    set => functionAssignmentInd =
      TrimEnd(Substring(value, 1, FunctionAssignmentInd_MaxLength));
  }

  /// <summary>
  /// The json value of the FunctionAssignmentInd attribute.</summary>
  [JsonPropertyName("functionAssignmentInd")]
  [Computed]
  public string FunctionAssignmentInd_Json
  {
    get => NullIf(FunctionAssignmentInd, "");
    set => FunctionAssignmentInd = value;
  }

  /// <summary>Length of the PROGRAM_ASSIGNMENT_IND attribute.</summary>
  public const int ProgramAssignmentInd_MaxLength = 1;

  /// <summary>
  /// The value of the PROGRAM_ASSIGNMENT_IND attribute.
  /// if switch is on this office assigns cases by programs
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = ProgramAssignmentInd_MaxLength)]
  public string ProgramAssignmentInd
  {
    get => programAssignmentInd ?? "";
    set => programAssignmentInd =
      TrimEnd(Substring(value, 1, ProgramAssignmentInd_MaxLength));
  }

  /// <summary>
  /// The json value of the ProgramAssignmentInd attribute.</summary>
  [JsonPropertyName("programAssignmentInd")]
  [Computed]
  public string ProgramAssignmentInd_Json
  {
    get => NullIf(ProgramAssignmentInd, "");
    set => ProgramAssignmentInd = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 6, Type = MemberType.Date)]
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
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 10, Type = MemberType.Timestamp, Optional = true)]
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
  [Member(Index = 11, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>Length of the TRIBUNAL_IND attribute.</summary>
  public const int TribunalInd_MaxLength = 1;

  /// <summary>
  /// The value of the TRIBUNAL_IND attribute.
  /// Flag to indicate if the office makes case assignments based on legal 
  /// action tribunal.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = TribunalInd_MaxLength)]
  public string TribunalInd
  {
    get => tribunalInd ?? "";
    set => tribunalInd = TrimEnd(Substring(value, 1, TribunalInd_MaxLength));
  }

  /// <summary>
  /// The json value of the TribunalInd attribute.</summary>
  [JsonPropertyName("tribunalInd")]
  [Computed]
  public string TribunalInd_Json
  {
    get => NullIf(TribunalInd, "");
    set => TribunalInd = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 4)]
  public int OffGeneratedId
  {
    get => offGeneratedId;
    set => offGeneratedId = value;
  }

  private string assignmentType;
  private string countyAssignmentInd;
  private string alphaAssignmentInd;
  private string functionAssignmentInd;
  private string programAssignmentInd;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private DateTime? createdTstamp;
  private string createdBy;
  private DateTime? lastUpdatdTstamp;
  private string lastUpdatedBy;
  private string tribunalInd;
  private int offGeneratedId;
}
