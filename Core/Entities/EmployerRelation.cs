// The source file: EMPLOYER_RELATION, ID: 371434201, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT	
/// Defines a relationship from one employer to another employer.
/// Example: An income may be reported as paid by Pepsico but it is discovered 
/// that KFC is the actual employer. Pepsico is the HQ of KFC.
/// </summary>
[Serializable]
public partial class EmployerRelation: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EmployerRelation()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EmployerRelation(EmployerRelation that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EmployerRelation Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EmployerRelation that)
  {
    base.Assign(that);
    note = that.note;
    identifier = that.identifier;
    verifiedDate = that.verifiedDate;
    verifiedBy = that.verifiedBy;
    effectiveDate = that.effectiveDate;
    endDate = that.endDate;
    createdTimestamp = that.createdTimestamp;
    updatedTimestamp = that.updatedTimestamp;
    createdBy = that.createdBy;
    updatedBy = that.updatedBy;
    type1 = that.type1;
    empHqId = that.empHqId;
    empLocId = that.empLocId;
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 80;

  /// <summary>
  /// The value of the NOTE attribute.
  /// Free format text regarding an employer relationship.
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

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Unique identifier for this entity.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the VERIFIED_DATE attribute.
  /// Date the relationship between two employers was verified.
  /// </summary>
  [JsonPropertyName("verifiedDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? VerifiedDate
  {
    get => verifiedDate;
    set => verifiedDate = value;
  }

  /// <summary>Length of the VERIFIED_BY attribute.</summary>
  public const int VerifiedBy_MaxLength = 8;

  /// <summary>
  /// The value of the VERIFIED_BY attribute.
  /// The person who verified the relationship between two employers.
  /// </summary>
  [JsonPropertyName("verifiedBy")]
  [Member(Index = 4, Type = MemberType.Char, Length = VerifiedBy_MaxLength, Optional
    = true)]
  public string VerifiedBy
  {
    get => verifiedBy;
    set => verifiedBy = value != null
      ? TrimEnd(Substring(value, 1, VerifiedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date a relationship between two employers was effective.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date the relationship between two empolyers was terminated.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the UPDATED_TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("updatedTimestamp")]
  [Member(Index = 8, Type = MemberType.Timestamp)]
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

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 8;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Describes the type of relationship between employers.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Type1_MaxLength)]
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
  /// The value of the IDENTIFIER attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("empHqId")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 9)]
  public int EmpHqId
  {
    get => empHqId;
    set => empHqId = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("empLocId")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 9)]
  public int EmpLocId
  {
    get => empLocId;
    set => empLocId = value;
  }

  private string note;
  private int identifier;
  private DateTime? verifiedDate;
  private string verifiedBy;
  private DateTime? effectiveDate;
  private DateTime? endDate;
  private DateTime? createdTimestamp;
  private DateTime? updatedTimestamp;
  private string createdBy;
  private string updatedBy;
  private string type1;
  private int empHqId;
  private int empLocId;
}
