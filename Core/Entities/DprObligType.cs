// The source file: DPR_OBLIG_TYPE, ID: 371434027, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINAN
/// This entity resolves the many-many relationship begtween DISTRIBUTION POLICY
/// RULE and OBLIGATION TYPE. It specifies the application Obligation Types for
/// a Distribution Policy Rule.
/// </summary>
[Serializable]
public partial class DprObligType: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DprObligType()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DprObligType(DprObligType that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DprObligType Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DprObligType that)
  {
    base.Assign(that);
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    dprGeneratedId = that.dprGeneratedId;
    dbpGeneratedId = that.dbpGeneratedId;
    otyGeneratedId = that.otyGeneratedId;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("dprGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 3)]
  public int DprGeneratedId
  {
    get => dprGeneratedId;
    set => dprGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("dbpGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 3)]
  public int DbpGeneratedId
  {
    get => dbpGeneratedId;
    set => dbpGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 3)]
  public int OtyGeneratedId
  {
    get => otyGeneratedId;
    set => otyGeneratedId = value;
  }

  private string createdBy;
  private DateTime? createdTimestamp;
  private int dprGeneratedId;
  private int dbpGeneratedId;
  private int otyGeneratedId;
}
