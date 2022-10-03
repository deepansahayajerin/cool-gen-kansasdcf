// The source file: EMPLOYER_REGISTERED_AGENT, ID: 371434188, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Registered agent assigned to an employer.
/// </summary>
[Serializable]
public partial class EmployerRegisteredAgent: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EmployerRegisteredAgent()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EmployerRegisteredAgent(EmployerRegisteredAgent that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EmployerRegisteredAgent Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EmployerRegisteredAgent that)
  {
    base.Assign(that);
    note = that.note;
    identifier = that.identifier;
    effectiveDate = that.effectiveDate;
    endDate = that.endDate;
    createdTimestamp = that.createdTimestamp;
    updatedTimestamp = that.updatedTimestamp;
    createdBy = that.createdBy;
    updatedBy = that.updatedBy;
    empId = that.empId;
    raaId = that.raaId;
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 80;

  /// <summary>
  /// The value of the NOTE attribute.
  /// Free format text regarding a registered agent.
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 1, Type = MemberType.Char, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => note;
    set => note = value != null
      ? TrimEnd(Substring(value, 1, Note_MaxLength)) : null;
  }

  /// <summary>Length of the IDENTIFIER attribute.</summary>
  public const int Identifier_MaxLength = 9;

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Unique identifier for an employer registered agent.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Identifier_MaxLength)]
  public string Identifier
  {
    get => identifier ?? "";
    set => identifier = TrimEnd(Substring(value, 1, Identifier_MaxLength));
  }

  /// <summary>
  /// The json value of the Identifier attribute.</summary>
  [JsonPropertyName("identifier")]
  [Computed]
  public string Identifier_Json
  {
    get => NullIf(Identifier, "");
    set => Identifier = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date the registered agent started working for an employer.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date the employer terminated it' contract with the registered agent.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the UPDATED_TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("updatedTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? UpdatedTimestamp
  {
    get => updatedTimestamp;
    set => updatedTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
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

  /// <summary>Length of the UPDATED_BY attribute.</summary>
  public const int UpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the UPDATED_BY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = UpdatedBy_MaxLength)]
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

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("empId")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 9)]
  public int EmpId
  {
    get => empId;
    set => empId = value;
  }

  /// <summary>Length of the IDENTIFIER attribute.</summary>
  public const int RaaId_MaxLength = 9;

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier for this agent.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = RaaId_MaxLength)]
  public string RaaId
  {
    get => raaId ?? "";
    set => raaId = TrimEnd(Substring(value, 1, RaaId_MaxLength));
  }

  /// <summary>
  /// The json value of the RaaId attribute.</summary>
  [JsonPropertyName("raaId")]
  [Computed]
  public string RaaId_Json
  {
    get => NullIf(RaaId, "");
    set => RaaId = value;
  }

  private string note;
  private string identifier;
  private DateTime? effectiveDate;
  private DateTime? endDate;
  private DateTime? createdTimestamp;
  private DateTime? updatedTimestamp;
  private string createdBy;
  private string updatedBy;
  private int empId;
  private string raaId;
}
