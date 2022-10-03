// The source file: ACTIVITY, ID: 371430042, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Activity definition as per Issue #26:
/// Activity contains the business rule template for a monitor of the number of 
/// days to complete a specified activity, having a definite starting event and
/// ending event, as defined by regulatory policies.
/// CSE users may select from specific activity types maintained in the 
/// reference table to generate a user defined monitored activity.
/// Activities which are integrated into automated event processing are 
/// classified as a separate type.
/// </summary>
[Serializable]
public partial class Activity: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Activity()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Activity(Activity that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Activity Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Activity that)
  {
    base.Assign(that);
    controlNumber = that.controlNumber;
    name = that.name;
    typeCode = that.typeCode;
    description = that.description;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
  }

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
  /// A predefined numeric identifier for performance optimization.
  /// </summary>
  [JsonPropertyName("controlNumber")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 5)]
  public int ControlNumber
  {
    get => controlNumber;
    set => controlNumber = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 40;

  /// <summary>
  /// The value of the NAME attribute.
  /// The Name of the activity monitor should be descriptive of the purpose of 
  /// the monitoring activity and timeframe.
  /// The Name suffix indicates the number of days, months, or years for 
  /// determining non compliance.  This will facilitate easy cross reference to
  /// federal regulations.  The actual values for compliance, however, will
  /// always be expressed in days.
  /// Name suffix examples:
  /// 1_YR	=	one year
  /// 30_DY	=	30 days
  /// 6_MO	=	6 months
  /// 	
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Name_MaxLength)]
  public string Name
  {
    get => name ?? "";
    set => name = TrimEnd(Substring(value, 1, Name_MaxLength));
  }

  /// <summary>
  /// The json value of the Name attribute.</summary>
  [JsonPropertyName("name")]
  [Computed]
  public string Name_Json
  {
    get => NullIf(Name, "");
    set => Name = value;
  }

  /// <summary>Length of the TYPE_CODE attribute.</summary>
  public const int TypeCode_MaxLength = 3;

  /// <summary>
  /// The value of the TYPE_CODE attribute.
  /// Type codeis used to designate and classify those activities which may be 
  /// selected for a user generated monitored activity.
  /// Values are:
  ///   MAN = manually generated (by a service provider)
  ///   AUT = automatically generated (by the event processor)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = TypeCode_MaxLength)]
  public string TypeCode
  {
    get => typeCode ?? "";
    set => typeCode = TrimEnd(Substring(value, 1, TypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the TypeCode attribute.</summary>
  [JsonPropertyName("typeCode")]
  [Computed]
  public string TypeCode_Json
  {
    get => NullIf(TypeCode, "");
    set => TypeCode = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// Description of the Activity details the business rule involved.
  /// The start and ending events involved in the Activity as well as the 
  /// timeframe should be noted.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 4, Type = MemberType.Char, Length = Description_MaxLength, Optional
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
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 6, Type = MemberType.Timestamp)]
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
  [Member(Index = 7, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
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
  [Member(Index = 8, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  private int controlNumber;
  private string name;
  private string typeCode;
  private string description;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
}
