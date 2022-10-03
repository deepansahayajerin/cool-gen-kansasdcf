// The source file: ACTIVITY_START_STOP, ID: 371430123, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN	
/// One Event may initiate generation of one or more monitored activities.  The 
/// same Event may also end (stop) one or more activities.
/// Activity Start Stop entity allows for the maintenance of these associations 
/// between Event and Activity.
/// </summary>
[Serializable]
public partial class ActivityStartStop: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ActivityStartStop()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ActivityStartStop(ActivityStartStop that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ActivityStartStop Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ActivityStartStop that)
  {
    base.Assign(that);
    actionCode = that.actionCode;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    acdId = that.acdId;
    actNo = that.actNo;
    evdId = that.evdId;
    eveNo = that.eveNo;
  }

  /// <summary>Length of the ACTION_CODE attribute.</summary>
  public const int ActionCode_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION_CODE attribute.
  /// Action Code is used by the application to determine if the event is a 
  /// starting or ending event for monitored activities.
  /// A specific event may function as both start and end event for different 
  /// activities.  A value of 'start' will associate to a specific activity.  A
  /// value of 'end' will associate to a specific but entirely different
  /// activity.
  /// Permitted values are:
  /// S = activity monitor starting event
  /// E = activity monitor ending event
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ActionCode_MaxLength)]
  public string ActionCode
  {
    get => actionCode ?? "";
    set => actionCode = TrimEnd(Substring(value, 1, ActionCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ActionCode attribute.</summary>
  [JsonPropertyName("actionCode")]
  [Computed]
  public string ActionCode_Json
  {
    get => NullIf(ActionCode, "");
    set => ActionCode = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 2, Type = MemberType.Date)]
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
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
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

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("acdId")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 3)]
  public int AcdId
  {
    get => acdId;
    set => acdId = value;
  }

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
  /// A predefined numeric identifier for performance optimization.
  /// </summary>
  [JsonPropertyName("actNo")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 5)]
  public int ActNo
  {
    get => actNo;
    set => actNo = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("evdId")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 3)]
  public int EvdId
  {
    get => evdId;
    set => evdId = value;
  }

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("eveNo")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 5)]
  public int EveNo
  {
    get => eveNo;
    set => eveNo = value;
  }

  private string actionCode;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private DateTime? lastUpdatedTimestamp;
  private string lastUpdatedBy;
  private DateTime? createdTimestamp;
  private string createdBy;
  private int acdId;
  private int actNo;
  private int evdId;
  private int eveNo;
}
