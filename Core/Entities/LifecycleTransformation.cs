﻿// The source file: LIFECYCLE_TRANSFORMATION, ID: 371437079, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Lifecycle Transformation dates the occurance of a lifecycle transformation 
/// for the current case unit state and the event(detail) causing the
/// transformation.
/// Through the date stamp and relationships, lifecycle transformation maps the 
/// current state and event detail to the resulting lifecycle state generated by
/// the transformation.
/// </summary>
[Serializable]
public partial class LifecycleTransformation: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LifecycleTransformation()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LifecycleTransformation(LifecycleTransformation that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LifecycleTransformation Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LifecycleTransformation that)
  {
    base.Assign(that);
    description = that.description;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    evdIdPri = that.evdIdPri;
    eveCtrlNoPri = that.eveCtrlNoPri;
    evdLctIdSec = that.evdLctIdSec;
    eveNoSec = that.eveNoSec;
    lcsIdPri = that.lcsIdPri;
    lcsLctIdSec = that.lcsLctIdSec;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 75;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Description_MaxLength)]
  public string Description
  {
    get => description ?? "";
    set => description = TrimEnd(Substring(value, 1, Description_MaxLength));
  }

  /// <summary>
  /// The json value of the Description attribute.</summary>
  [JsonPropertyName("description")]
  [Computed]
  public string Description_Json
  {
    get => NullIf(Description, "");
    set => Description = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 3, Type = MemberType.Timestamp)]
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
  [Member(Index = 4, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
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
  [Member(Index = 5, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("evdIdPri")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 3)]
  public int EvdIdPri
  {
    get => evdIdPri;
    set => evdIdPri = value;
  }

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("eveCtrlNoPri")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 5)]
  public int EveCtrlNoPri
  {
    get => eveCtrlNoPri;
    set => eveCtrlNoPri = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("evdLctIdSec")]
  [Member(Index = 8, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? EvdLctIdSec
  {
    get => evdLctIdSec;
    set => evdLctIdSec = value;
  }

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("eveNoSec")]
  [Member(Index = 9, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? EveNoSec
  {
    get => eveNoSec;
    set => eveNoSec = value;
  }

  /// <summary>Length of the IDENTIFIER attribute.</summary>
  public const int LcsIdPri_MaxLength = 5;

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
  [Member(Index = 10, Type = MemberType.Char, Length = LcsIdPri_MaxLength)]
  public string LcsIdPri
  {
    get => lcsIdPri ?? "";
    set => lcsIdPri = TrimEnd(Substring(value, 1, LcsIdPri_MaxLength));
  }

  /// <summary>
  /// The json value of the LcsIdPri attribute.</summary>
  [JsonPropertyName("lcsIdPri")]
  [Computed]
  public string LcsIdPri_Json
  {
    get => NullIf(LcsIdPri, "");
    set => LcsIdPri = value;
  }

  /// <summary>Length of the IDENTIFIER attribute.</summary>
  public const int LcsLctIdSec_MaxLength = 5;

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
  [Member(Index = 11, Type = MemberType.Char, Length = LcsLctIdSec_MaxLength)]
  public string LcsLctIdSec
  {
    get => lcsLctIdSec ?? "";
    set => lcsLctIdSec = TrimEnd(Substring(value, 1, LcsLctIdSec_MaxLength));
  }

  /// <summary>
  /// The json value of the LcsLctIdSec attribute.</summary>
  [JsonPropertyName("lcsLctIdSec")]
  [Computed]
  public string LcsLctIdSec_Json
  {
    get => NullIf(LcsLctIdSec, "");
    set => LcsLctIdSec = value;
  }

  private string description;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int evdIdPri;
  private int eveCtrlNoPri;
  private int? evdLctIdSec;
  private int? eveNoSec;
  private string lcsIdPri;
  private string lcsLctIdSec;
}