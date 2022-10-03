// The source file: EVENT, ID: 371434217, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// An Event is the predefined infrastructure expression of either a change in 
/// the data, or of a specific status of data in the system.
/// An Event drives the occurance of Alerts, Activities, and case unit Lifecycle
/// Transformation.  Event does not drive the occurance of Documents but may
/// result in recording the document.
/// An Event is used by the application in automated management of CSE services 
/// and monitoring of compliance timeframes and activities.
/// For Example:
/// 1.	Change in data.
/// Creation of or update to the AP prison release date will result in raising 
/// an event which expresses the action of changing the AP prison release date.
/// 2.	Status of data.
/// A batch routine is run to evaluate AP compliance with court ordered 
/// obligations.  If the batch routine identifies an occurance of non
/// compliance, an event is raised expressing the specific details of non
/// compliance.
/// </summary>
[Serializable]
public partial class Event1: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Event1()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Event1(Event1 that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Event1 Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Event1 that)
  {
    base.Assign(that);
    controlNumber = that.controlNumber;
    name = that.name;
    type1 = that.type1;
    description = that.description;
    businessObjectCode = that.businessObjectCode;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
  }

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
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
  /// A descriptive name for the event.
  /// The convention for the event name is to use commonly accepted CSE 
  /// abbreviations and to structure the name so as to facilitate sorting
  /// related events together.
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

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 15;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Type classifies an Event into one of the following categories.  Event Type
  /// codes are documented in Code Value Table.
  /// Event Types include:
  /// ADMINACT	Administrative Action
  /// ADMINAPL	Administrative Appeal
  /// EXNOTF		External Notification
  /// APDTL		AP Details
  /// COMPLIANCE	Compliance
  /// CASEUNIT	Case Unit Status
  /// CSENET		CSENet Transaction
  /// DOC		Document Generated
  /// DM		Date Monitor
  /// GENTST 		Genetic Test
  /// LEACT		Legal Action
  /// LEEVT		Legal Event
  /// LERFRL		Legal Referral
  /// LERESP		Legal Response
  /// MODFN		Support Modification
  /// OBLG		Obligation Established
  /// SERV		Process of Service
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Type1_MaxLength)]
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

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// A full text business definition of the event.
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

  /// <summary>Length of the BUSINESS_OBJECT_CODE attribute.</summary>
  public const int BusinessObjectCode_MaxLength = 3;

  /// <summary>
  /// The value of the BUSINESS_OBJECT_CODE attribute.
  /// The Business Object represents the primary entity for which the event (
  /// activity) has meaning.
  /// The business object for an event will always include Case.
  /// Valid Business Objects include:
  /// Case
  /// Case_Unit
  /// CSE_Person
  /// Information_Request
  /// Referral
  /// Legal_Referral
  /// Legal_Action
  /// Obligation
  /// Obligation_Administrative_Action
  /// Administrative_Appeal
  /// Administrative_Act_Certification
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = BusinessObjectCode_MaxLength)]
  public string BusinessObjectCode
  {
    get => businessObjectCode ?? "";
    set => businessObjectCode =
      TrimEnd(Substring(value, 1, BusinessObjectCode_MaxLength));
  }

  /// <summary>
  /// The json value of the BusinessObjectCode attribute.</summary>
  [JsonPropertyName("businessObjectCode")]
  [Computed]
  public string BusinessObjectCode_Json
  {
    get => NullIf(BusinessObjectCode, "");
    set => BusinessObjectCode = value;
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
  private string type1;
  private string description;
  private string businessObjectCode;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
}
