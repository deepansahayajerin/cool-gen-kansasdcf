// The source file: LIFECYCLE_STATE, ID: 371437065, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Life Cycle State identifies the unique property value combinations which 
/// compose to identify each state.
/// Each Lifecycle State is composed of a unique combination of values 
/// populating the following five properties:
/// Case Unit Function	(L/P/O/E)
/// Service Type		(E/F/L)
/// Located			(Y/N)
/// Is an AP		(Y/N/U)
/// Obligation		(Y/N/U)
/// </summary>
[Serializable]
public partial class LifecycleState: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LifecycleState()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LifecycleState(LifecycleState that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LifecycleState Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LifecycleState that)
  {
    base.Assign(that);
    identifier = that.identifier;
    description = that.description;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
  }

  /// <summary>Length of the IDENTIFIER attribute.</summary>
  public const int Identifier_MaxLength = 5;

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The identifier for lifecycle state is the unique property value 
  /// combination for the state.
  /// Each state identifier is composed of a unique combination of values 
  /// populating the following five properties:
  /// Case Unit Function	(L/P/O/E)
  /// Service Type		(E/F/L)
  /// Located			(Y/N)
  /// Is an AP		(Y/N/U)
  /// Obligation		(Y/N/U)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Identifier_MaxLength)]
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

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 60;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// Short description of the life cycle state identifier.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 2, Type = MemberType.Char, Length = Description_MaxLength, Optional
    = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? TrimEnd(Substring(value, 1, Description_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 4, Type = MemberType.Timestamp)]
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
  [Member(Index = 5, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
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
  [Member(Index = 6, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  private string identifier;
  private string description;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
}
