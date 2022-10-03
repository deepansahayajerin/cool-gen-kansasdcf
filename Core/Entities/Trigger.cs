// The source file: TRIGGER, ID: 374351708, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  SRVPLAN
/// 
/// FOR GENERAL USE. Records the fact that an entity has been added, changed,
/// deleted, etc. The triggers will then be processed later by a batch job.
/// The denormalized attributes will hold the key values of the entity. The
/// identifier attribute is used to uniquely identify the record, the record
/// name attribute will identify which process is supposed to process the
/// record, the action attribute will tell that process what action the
/// record needs, and the status attribute will tell that process whether the
/// record has already been processed and to what degree (for multiple step
/// processes).
/// </summary>
[Serializable]
public partial class Trigger: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Trigger()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Trigger(Trigger that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Trigger Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Trigger that)
  {
    base.Assign(that);
    identifier = that.identifier;
    type1 = that.type1;
    action = that.action;
    status = that.status;
    denormNumeric1 = that.denormNumeric1;
    denormNumeric2 = that.denormNumeric2;
    denormNumeric3 = that.denormNumeric3;
    denormText1 = that.denormText1;
    denormText2 = that.denormText2;
    denormText3 = that.denormText3;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    updatedTimestamp = that.updatedTimestamp;
    denormTimestamp = that.denormTimestamp;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Uniquely identifies an accurrence of the trigger entity. Randomly 
  /// assigned.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 8;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Type of trigger. Used later by a batch job to identify the triggers to be 
  /// processed.
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

  /// <summary>Length of the ACTION attribute.</summary>
  public const int Action_MaxLength = 8;

  /// <summary>
  /// The value of the ACTION attribute.
  /// Records the action taken on the entity. Examples include ADD, UPDATE, 
  /// DELETE, SUSPEND, etc.
  /// </summary>
  [JsonPropertyName("action")]
  [Member(Index = 3, Type = MemberType.Char, Length = Action_MaxLength, Optional
    = true)]
  public string Action
  {
    get => action;
    set => action = value != null
      ? TrimEnd(Substring(value, 1, Action_MaxLength)) : null;
  }

  /// <summary>Length of the STATUS attribute.</summary>
  public const int Status_MaxLength = 1;

  /// <summary>
  /// The value of the STATUS attribute.
  /// The processing status of the trigger.
  /// </summary>
  [JsonPropertyName("status")]
  [Member(Index = 4, Type = MemberType.Char, Length = Status_MaxLength, Optional
    = true)]
  public string Status
  {
    get => status;
    set => status = value != null
      ? TrimEnd(Substring(value, 1, Status_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DENORM_NUMERIC_1 attribute.
  /// Denormalized numeric identifier of the entity which this trigger refers.
  /// </summary>
  [JsonPropertyName("denormNumeric1")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DenormNumeric1
  {
    get => denormNumeric1;
    set => denormNumeric1 = value;
  }

  /// <summary>
  /// The value of the DENORM_NUMERIC_2 attribute.
  /// Denormalized numeric identifier of the entity which this trigger refers.
  /// </summary>
  [JsonPropertyName("denormNumeric2")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DenormNumeric2
  {
    get => denormNumeric2;
    set => denormNumeric2 = value;
  }

  /// <summary>
  /// The value of the DENORM_NUMERIC_3 attribute.
  /// Denormalized numeric identifier of the entity which this trigger refers.
  /// </summary>
  [JsonPropertyName("denormNumeric3")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DenormNumeric3
  {
    get => denormNumeric3;
    set => denormNumeric3 = value;
  }

  /// <summary>Length of the DENORM_TEXT_1 attribute.</summary>
  public const int DenormText1_MaxLength = 10;

  /// <summary>
  /// The value of the DENORM_TEXT_1 attribute.
  /// Denormalized text identifier of the entity which this trigger refers.
  /// </summary>
  [JsonPropertyName("denormText1")]
  [Member(Index = 8, Type = MemberType.Char, Length = DenormText1_MaxLength, Optional
    = true)]
  public string DenormText1
  {
    get => denormText1;
    set => denormText1 = value != null
      ? TrimEnd(Substring(value, 1, DenormText1_MaxLength)) : null;
  }

  /// <summary>Length of the DENORM_TEXT_2 attribute.</summary>
  public const int DenormText2_MaxLength = 10;

  /// <summary>
  /// The value of the DENORM_TEXT_2 attribute.
  /// Denormalized text identifier of the entity which this trigger refers.
  /// </summary>
  [JsonPropertyName("denormText2")]
  [Member(Index = 9, Type = MemberType.Char, Length = DenormText2_MaxLength, Optional
    = true)]
  public string DenormText2
  {
    get => denormText2;
    set => denormText2 = value != null
      ? TrimEnd(Substring(value, 1, DenormText2_MaxLength)) : null;
  }

  /// <summary>Length of the DENORM_TEXT_3 attribute.</summary>
  public const int DenormText3_MaxLength = 10;

  /// <summary>
  /// The value of the DENORM_TEXT_3 attribute.
  /// Denormalized text identifier of the entity which this trigger refers.
  /// </summary>
  [JsonPropertyName("denormText3")]
  [Member(Index = 10, Type = MemberType.Char, Length = DenormText3_MaxLength, Optional
    = true)]
  public string DenormText3
  {
    get => denormText3;
    set => denormText3 = value != null
      ? TrimEnd(Substring(value, 1, DenormText3_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USERID of the person or process that created the trigger.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 11, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of the creation of the trigger.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 12, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// USERID of the person or process that last updated the trigger.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the UPDATED_TIMESTAMP attribute.
  /// Timestamp of the last update of the trigger.
  /// </summary>
  [JsonPropertyName("updatedTimestamp")]
  [Member(Index = 14, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? UpdatedTimestamp
  {
    get => updatedTimestamp;
    set => updatedTimestamp = value;
  }

  /// <summary>
  /// The value of the DENORM_TIMESTAMP attribute.
  /// Denormalized timestamp identifier of the entity which this trigger refers.
  /// </summary>
  [JsonPropertyName("denormTimestamp")]
  [Member(Index = 15, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? DenormTimestamp
  {
    get => denormTimestamp;
    set => denormTimestamp = value;
  }

  private int identifier;
  private string type1;
  private string action;
  private string status;
  private int? denormNumeric1;
  private int? denormNumeric2;
  private int? denormNumeric3;
  private string denormText1;
  private string denormText2;
  private string denormText3;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? updatedTimestamp;
  private DateTime? denormTimestamp;
}
