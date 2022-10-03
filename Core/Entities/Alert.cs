// The source file: ALERT, ID: 371430542, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// An Alert is a system generated message triggered by an event and distributed
/// for display to one or more service providers via Alert Distribution Rules.
/// An Alert is used to inform the service provider of pertinant activity of 
/// which they may have no knowledge.  An Alert may be triggered by online,
/// batch, or date monitoring routines.
/// Alert definition as per Issue #26:
/// A means to notify a service provider that some event has occurred.  This 
/// event could be the result of some action taken in the application, or the
/// arrival of a specific date.
/// NOTE:
/// CSE users may create customized Office Service Provider Alerts.  These 
/// custom alerts are NOT related to infrastructure, NOT handled by the event
/// processor, and have NO matching Alert template.
/// </summary>
[Serializable]
public partial class Alert: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Alert()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Alert(Alert that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Alert Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Alert that)
  {
    base.Assign(that);
    controlNumber = that.controlNumber;
    name = that.name;
    message = that.message;
    description = that.description;
    externalIndicator = that.externalIndicator;
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
  /// The Alert name should use commonly accepted CSE abbreviations to express 
  /// the purpose of the alert.
  /// The alert prefix indicates whether the alert was triggered by date monitor
  /// processing, a batch routine, or online activity.
  /// Prefix example:
  /// DM =	date monitored activity triggered 	alert
  /// B  =	batch routine triggered the alert
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

  /// <summary>Length of the MESSAGE attribute.</summary>
  public const int Message_MaxLength = 55;

  /// <summary>
  /// The value of the MESSAGE attribute.
  /// Message is the compacted version of the alert description for display from
  /// a list maintenance screen.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Message_MaxLength)]
  public string Message
  {
    get => message ?? "";
    set => message = TrimEnd(Substring(value, 1, Message_MaxLength));
  }

  /// <summary>
  /// The json value of the Message attribute.</summary>
  [JsonPropertyName("message")]
  [Computed]
  public string Message_Json
  {
    get => NullIf(Message, "");
    set => Message = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// Description of the full text message for a n alert which is viewed from 
  /// the Alert Detail Procedure.
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

  /// <summary>Length of the EXTERNAL_INDICATOR attribute.</summary>
  public const int ExternalIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the EXTERNAL_INDICATOR attribute.
  /// The External Indicator identifies those alerts generated for AE/IM or 
  /// KSCARES.  These organizations are external to CSE.
  /// Values are:
  /// Y  =	Yes, externally created alert
  /// N  =	No, system created alert
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = ExternalIndicator_MaxLength)]
  public string ExternalIndicator
  {
    get => externalIndicator ?? "";
    set => externalIndicator =
      TrimEnd(Substring(value, 1, ExternalIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the ExternalIndicator attribute.</summary>
  [JsonPropertyName("externalIndicator")]
  [Computed]
  public string ExternalIndicator_Json
  {
    get => NullIf(ExternalIndicator, "");
    set => ExternalIndicator = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 7, Type = MemberType.Timestamp)]
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
  [Member(Index = 8, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
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
  [Member(Index = 9, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  private int controlNumber;
  private string name;
  private string message;
  private string description;
  private string externalIndicator;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
}
