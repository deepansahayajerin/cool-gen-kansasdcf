// The source file: INTERFACE_ALERT, ID: 371435658, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPL
/// This entity is used to process incoming and outgoing alerts.  This table is 
/// populated by a Natural program copying from and to ADABAS.
/// </summary>
[Serializable]
public partial class InterfaceAlert: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterfaceAlert()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterfaceAlert(InterfaceAlert that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterfaceAlert Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterfaceAlert that)
  {
    base.Assign(that);
    identifier = that.identifier;
    csePersonNumber = that.csePersonNumber;
    processStatus = that.processStatus;
    alertCode = that.alertCode;
    alertName = that.alertName;
    noteText = that.noteText;
    sendingSystem = that.sendingSystem;
    lastUpdatedTmstamp = that.lastUpdatedTmstamp;
    receivingSystem = that.receivingSystem;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
  }

  /// <summary>Length of the IDENTIFIER attribute.</summary>
  public const int Identifier_MaxLength = 16;

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Identifies the occurrence in the interface table.
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

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// The CSE person number for which the alert is created.  If more than one 
  /// CSE person number is required, then that number and name can be passed in
  /// the text field.  No processing will occur on the text field.
  /// </summary>
  [JsonPropertyName("csePersonNumber")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = CsePersonNumber_MaxLength, Optional = true)]
  public string CsePersonNumber
  {
    get => csePersonNumber;
    set => csePersonNumber = value != null
      ? TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength)) : null;
  }

  /// <summary>Length of the PROCESS_STATUS attribute.</summary>
  public const int ProcessStatus_MaxLength = 1;

  /// <summary>
  /// The value of the PROCESS_STATUS attribute.
  /// A status to indicate the stage of the processing.
  /// </summary>
  [JsonPropertyName("processStatus")]
  [Member(Index = 3, Type = MemberType.Char, Length = ProcessStatus_MaxLength, Optional
    = true)]
  public string ProcessStatus
  {
    get => processStatus;
    set => processStatus = value != null
      ? TrimEnd(Substring(value, 1, ProcessStatus_MaxLength)) : null;
  }

  /// <summary>Length of the ALERT_CODE attribute.</summary>
  public const int AlertCode_MaxLength = 2;

  /// <summary>
  /// The value of the ALERT_CODE attribute.
  /// The code associated to the alert.
  /// </summary>
  [JsonPropertyName("alertCode")]
  [Member(Index = 4, Type = MemberType.Char, Length = AlertCode_MaxLength, Optional
    = true)]
  public string AlertCode
  {
    get => alertCode;
    set => alertCode = value != null
      ? TrimEnd(Substring(value, 1, AlertCode_MaxLength)) : null;
  }

  /// <summary>Length of the ALERT_NAME attribute.</summary>
  public const int AlertName_MaxLength = 40;

  /// <summary>
  /// The value of the ALERT_NAME attribute.
  /// The text name of the alert.
  /// </summary>
  [JsonPropertyName("alertName")]
  [Member(Index = 5, Type = MemberType.Char, Length = AlertName_MaxLength, Optional
    = true)]
  public string AlertName
  {
    get => alertName;
    set => alertName = value != null
      ? TrimEnd(Substring(value, 1, AlertName_MaxLength)) : null;
  }

  /// <summary>Length of the NOTE_TEXT attribute.</summary>
  public const int NoteText_MaxLength = 245;

  /// <summary>
  /// The value of the NOTE_TEXT attribute.
  /// Contains additional information for the alert.
  /// 	Example:  Old/New name on a name change.
  /// </summary>
  [JsonPropertyName("noteText")]
  [Member(Index = 6, Type = MemberType.Char, Length = NoteText_MaxLength, Optional
    = true)]
  public string NoteText
  {
    get => noteText;
    set => noteText = value != null
      ? TrimEnd(Substring(value, 1, NoteText_MaxLength)) : null;
  }

  /// <summary>Length of the SENDING_SYSTEM attribute.</summary>
  public const int SendingSystem_MaxLength = 3;

  /// <summary>
  /// The value of the SENDING_SYSTEM attribute.
  /// The source system for the alert.
  /// 	Example: AE
  /// 		 CSE
  /// 		 KSC
  /// </summary>
  [JsonPropertyName("sendingSystem")]
  [Member(Index = 7, Type = MemberType.Char, Length = SendingSystem_MaxLength, Optional
    = true)]
  public string SendingSystem
  {
    get => sendingSystem;
    set => sendingSystem = value != null
      ? TrimEnd(Substring(value, 1, SendingSystem_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMSTAMP attribute.
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmstamp")]
  [Member(Index = 8, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmstamp
  {
    get => lastUpdatedTmstamp;
    set => lastUpdatedTmstamp = value;
  }

  /// <summary>Length of the RECEIVING_SYSTEM attribute.</summary>
  public const int ReceivingSystem_MaxLength = 3;

  /// <summary>
  /// The value of the RECEIVING_SYSTEM attribute.
  /// The destination system name.
  /// 	Example: AE
  /// 	 	 CSE
  /// 		 KSC
  /// </summary>
  [JsonPropertyName("receivingSystem")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = ReceivingSystem_MaxLength, Optional = true)]
  public string ReceivingSystem
  {
    get => receivingSystem;
    set => receivingSystem = value != null
      ? TrimEnd(Substring(value, 1, ReceivingSystem_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 10, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
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

  private string identifier;
  private string csePersonNumber;
  private string processStatus;
  private string alertCode;
  private string alertName;
  private string noteText;
  private string sendingSystem;
  private DateTime? lastUpdatedTmstamp;
  private string receivingSystem;
  private DateTime? createdTimestamp;
  private string createdBy;
}
