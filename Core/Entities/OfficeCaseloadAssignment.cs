// The source file: OFFICE_CASELOAD_ASSIGNMENT, ID: 371438516, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// This entity is used to assign a worker to case.
/// Based on:
/// County
/// Program
/// Function
/// alpha
/// </summary>
[Serializable]
public partial class OfficeCaseloadAssignment: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OfficeCaseloadAssignment()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OfficeCaseloadAssignment(OfficeCaseloadAssignment that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OfficeCaseloadAssignment Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(OfficeCaseloadAssignment that)
  {
    base.Assign(that);
    assignmentIndicator = that.assignmentIndicator;
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    endingAlpha = that.endingAlpha;
    endingFirstInitial = that.endingFirstInitial;
    beginingAlpha = that.beginingAlpha;
    beginningFirstIntial = that.beginningFirstIntial;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    priority = that.priority;
    function = that.function;
    assignmentType = that.assignmentType;
    cogTypeCode = that.cogTypeCode;
    cogCode = that.cogCode;
    offGeneratedId = that.offGeneratedId;
    ospRoleCode = that.ospRoleCode;
    ospEffectiveDate = that.ospEffectiveDate;
    offDGeneratedId = that.offDGeneratedId;
    spdGeneratedId = that.spdGeneratedId;
    prgGeneratedId = that.prgGeneratedId;
    trbId = that.trbId;
  }

  /// <summary>Length of the ASSIGNMENT_INDICATOR attribute.</summary>
  public const int AssignmentIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the ASSIGNMENT_INDICATOR attribute.
  /// Used to indicate how the assignment is to be used in terms of assigning or
  /// reassigning a Service Provider to Cases.  This will be used to determine
  /// if the Assignment is to be used in a Reshuffle Status or in an Active
  /// Status.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = AssignmentIndicator_MaxLength)]
  public string AssignmentIndicator
  {
    get => assignmentIndicator ?? "";
    set => assignmentIndicator =
      TrimEnd(Substring(value, 1, AssignmentIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the AssignmentIndicator attribute.</summary>
  [JsonPropertyName("assignmentIndicator")]
  [Computed]
  public string AssignmentIndicator_Json
  {
    get => NullIf(AssignmentIndicator, "");
    set => AssignmentIndicator = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the ENDING_ALPHA attribute.</summary>
  public const int EndingAlpha_MaxLength = 11;

  /// <summary>
  /// The value of the ENDING_ALPHA attribute.
  /// Specifies the end of the alpha range.  (inclusive of this value)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = EndingAlpha_MaxLength)]
  public string EndingAlpha
  {
    get => endingAlpha ?? "";
    set => endingAlpha = TrimEnd(Substring(value, 1, EndingAlpha_MaxLength));
  }

  /// <summary>
  /// The json value of the EndingAlpha attribute.</summary>
  [JsonPropertyName("endingAlpha")]
  [Computed]
  public string EndingAlpha_Json
  {
    get => NullIf(EndingAlpha, "");
    set => EndingAlpha = value;
  }

  /// <summary>Length of the ENDING_FIRST_INITIAL attribute.</summary>
  public const int EndingFirstInitial_MaxLength = 1;

  /// <summary>
  /// The value of the ENDING_FIRST_INITIAL attribute.
  /// Ending of assignment.  First initial of first name used when breaking up 
  /// assignments within a last name.
  /// Example: Smith J
  /// </summary>
  [JsonPropertyName("endingFirstInitial")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = EndingFirstInitial_MaxLength, Optional = true)]
  public string EndingFirstInitial
  {
    get => endingFirstInitial;
    set => endingFirstInitial = value != null
      ? TrimEnd(Substring(value, 1, EndingFirstInitial_MaxLength)) : null;
  }

  /// <summary>Length of the BEGINING_ALPHA attribute.</summary>
  public const int BeginingAlpha_MaxLength = 11;

  /// <summary>
  /// The value of the BEGINING_ALPHA attribute.
  /// Specifies and includes the beginning of the alpha range.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = BeginingAlpha_MaxLength)]
  public string BeginingAlpha
  {
    get => beginingAlpha ?? "";
    set => beginingAlpha =
      TrimEnd(Substring(value, 1, BeginingAlpha_MaxLength));
  }

  /// <summary>
  /// The json value of the BeginingAlpha attribute.</summary>
  [JsonPropertyName("beginingAlpha")]
  [Computed]
  public string BeginingAlpha_Json
  {
    get => NullIf(BeginingAlpha, "");
    set => BeginingAlpha = value;
  }

  /// <summary>Length of the BEGINNING_FIRST_INTIAL attribute.</summary>
  public const int BeginningFirstIntial_MaxLength = 1;

  /// <summary>
  /// The value of the BEGINNING_FIRST_INTIAL attribute.
  /// Beginning of assignment. First initial of first name used when breaking up
  /// assignment within a last name.
  /// Example: Smith J
  /// </summary>
  [JsonPropertyName("beginningFirstIntial")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = BeginningFirstIntial_MaxLength, Optional = true)]
  public string BeginningFirstIntial
  {
    get => beginningFirstIntial;
    set => beginningFirstIntial = value != null
      ? TrimEnd(Substring(value, 1, BeginningFirstIntial_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 7, Type = MemberType.Date)]
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
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
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
  [Member(Index = 9, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 10, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 12, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the PRIORITY attribute.
  /// This is the ranking of the assignment table.  The order of the table 
  /// indicates the order that the program will look for a bucket that matches.
  /// </summary>
  [JsonPropertyName("priority")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 1)]
  public int Priority
  {
    get => priority;
    set => priority = value;
  }

  /// <summary>Length of the FUNCTION attribute.</summary>
  public const int Function_MaxLength = 3;

  /// <summary>
  /// The value of the FUNCTION attribute.
  /// Specifies the CSE CASE function to be matched on doing auto-assiging of 
  /// CSE CASE.
  /// </summary>
  [JsonPropertyName("function")]
  [Member(Index = 14, Type = MemberType.Char, Length = Function_MaxLength, Optional
    = true)]
  public string Function
  {
    get => function;
    set => function = value != null
      ? TrimEnd(Substring(value, 1, Function_MaxLength)) : null;
  }

  /// <summary>Length of the ASSIGNMENT_TYPE attribute.</summary>
  public const int AssignmentType_MaxLength = 2;

  /// <summary>
  /// The value of the ASSIGNMENT_TYPE attribute.
  /// Specifies the type of the occurrence, CA(case) or RE(PA Referral)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = AssignmentType_MaxLength)]
    
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
  [Member(Index = 16, Type = MemberType.Char, Length = CogTypeCode_MaxLength, Optional
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
  [Member(Index = 17, Type = MemberType.Char, Length = CogCode_MaxLength, Optional
    = true)]
  public string CogCode
  {
    get => cogCode;
    set => cogCode = value != null
      ? TrimEnd(Substring(value, 1, CogCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offGeneratedId")]
  [Member(Index = 18, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffGeneratedId
  {
    get => offGeneratedId;
    set => offGeneratedId = value;
  }

  /// <summary>Length of the ROLE_CODE attribute.</summary>
  public const int OspRoleCode_MaxLength = 2;

  /// <summary>
  /// The value of the ROLE_CODE attribute.
  /// This is the job title or role the person is fulfilling at or for a 
  /// particular location.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// </summary>
  [JsonPropertyName("ospRoleCode")]
  [Member(Index = 19, Type = MemberType.Char, Length = OspRoleCode_MaxLength, Optional
    = true)]
  public string OspRoleCode
  {
    get => ospRoleCode;
    set => ospRoleCode = value != null
      ? TrimEnd(Substring(value, 1, OspRoleCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("ospEffectiveDate")]
  [Member(Index = 20, Type = MemberType.Date, Optional = true)]
  public DateTime? OspEffectiveDate
  {
    get => ospEffectiveDate;
    set => ospEffectiveDate = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offDGeneratedId")]
  [Member(Index = 21, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffDGeneratedId
  {
    get => offDGeneratedId;
    set => offDGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdGeneratedId")]
  [Member(Index = 22, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? SpdGeneratedId
  {
    get => spdGeneratedId;
    set => spdGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// identifies the program
  /// </summary>
  [JsonPropertyName("prgGeneratedId")]
  [Member(Index = 23, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? PrgGeneratedId
  {
    get => prgGeneratedId;
    set => prgGeneratedId = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute uniquely identifies a tribunal record. It is automatically 
  /// generated by the system starting from 1.
  /// </summary>
  [JsonPropertyName("trbId")]
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TrbId
  {
    get => trbId;
    set => trbId = value;
  }

  private string assignmentIndicator;
  private int systemGeneratedIdentifier;
  private string endingAlpha;
  private string endingFirstInitial;
  private string beginingAlpha;
  private string beginningFirstIntial;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private string createdBy;
  private DateTime? createdTimestamp;
  private int priority;
  private string function;
  private string assignmentType;
  private string cogTypeCode;
  private string cogCode;
  private int? offGeneratedId;
  private string ospRoleCode;
  private DateTime? ospEffectiveDate;
  private int? offDGeneratedId;
  private int? spdGeneratedId;
  private int? prgGeneratedId;
  private int? trbId;
}
