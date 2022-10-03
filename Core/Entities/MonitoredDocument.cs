// The source file: MONITORED_DOCUMENT, ID: 371437378, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN	
/// Monitored Document contains a record for those documents which have required
/// response days to monitor.
/// These are the records which will be displayed to the service provider.
/// DATA MODEL ALERT!!!!!
/// *	The relationship between MONITORED_DOCUMENT and OFFICE_SERVICE_PROVIDER is
/// not drawn.
/// Each MONITORED_DOCUMENT always is identified by one OFFICE_SERVICE_PROVIDER
/// Each OFFICE_SERVICE_PROVIDER sometimes identifies one or more 
/// MONITORED_DOCUMENTs.
/// </summary>
[Serializable]
public partial class MonitoredDocument: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public MonitoredDocument()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public MonitoredDocument(MonitoredDocument that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new MonitoredDocument Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(MonitoredDocument that)
  {
    base.Assign(that);
    requiredResponseDate = that.requiredResponseDate;
    actualResponseDate = that.actualResponseDate;
    closureDate = that.closureDate;
    closureReasonCode = that.closureReasonCode;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    offId = that.offId;
    ospDate = that.ospDate;
    ospCode = that.ospCode;
    spdId = that.spdId;
    infId = that.infId;
  }

  /// <summary>
  /// The value of the REQUIRED_RESPONSE_DATE attribute.
  /// Required Response Dae is the date derived from required response days and 
  /// send date.
  /// This date indicates the date by which the document addressee must respond 
  /// to CSE to remain in cooperation or compliance with CSE request.
  /// </summary>
  [JsonPropertyName("requiredResponseDate")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? RequiredResponseDate
  {
    get => requiredResponseDate;
    set => requiredResponseDate = value;
  }

  /// <summary>
  /// The value of the ACTUAL_RESPONSE_DATE attribute.
  /// The date of completion for the monitored document.
  /// </summary>
  [JsonPropertyName("actualResponseDate")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? ActualResponseDate
  {
    get => actualResponseDate;
    set => actualResponseDate = value;
  }

  /// <summary>
  /// The value of the CLOSURE_DATE attribute.
  /// The date of completion of the monitored activity.
  /// </summary>
  [JsonPropertyName("closureDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? ClosureDate
  {
    get => closureDate;
    set => closureDate = value;
  }

  /// <summary>Length of the CLOSURE_REASON_CODE attribute.</summary>
  public const int ClosureReasonCode_MaxLength = 1;

  /// <summary>
  /// The value of the CLOSURE_REASON_CODE attribute.
  /// The End Reason Code indicates the activity closure reason.
  /// </summary>
  [JsonPropertyName("closureReasonCode")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = ClosureReasonCode_MaxLength, Optional = true)]
  public string ClosureReasonCode
  {
    get => closureReasonCode;
    set => closureReasonCode = value != null
      ? TrimEnd(Substring(value, 1, ClosureReasonCode_MaxLength)) : null;
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

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offId")]
  [Member(Index = 9, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffId
  {
    get => offId;
    set => offId = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("ospDate")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? OspDate
  {
    get => ospDate;
    set => ospDate = value;
  }

  /// <summary>Length of the ROLE_CODE attribute.</summary>
  public const int OspCode_MaxLength = 2;

  /// <summary>
  /// The value of the ROLE_CODE attribute.
  /// This is the job title or role the person is fulfilling at or for a 
  /// particular location.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// </summary>
  [JsonPropertyName("ospCode")]
  [Member(Index = 11, Type = MemberType.Char, Length = OspCode_MaxLength, Optional
    = true)]
  public string OspCode
  {
    get => ospCode;
    set => ospCode = value != null
      ? TrimEnd(Substring(value, 1, OspCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdId")]
  [Member(Index = 12, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? SpdId
  {
    get => spdId;
    set => spdId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("infId")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 9)]
  public int InfId
  {
    get => infId;
    set => infId = value;
  }

  private DateTime? requiredResponseDate;
  private DateTime? actualResponseDate;
  private DateTime? closureDate;
  private string closureReasonCode;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int? offId;
  private DateTime? ospDate;
  private string ospCode;
  private int? spdId;
  private int infId;
}
