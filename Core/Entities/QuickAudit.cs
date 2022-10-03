// The source file: QUICK_AUDIT, ID: 374536995, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: QUICK
/// 
/// Maintains audit data related to the QUICK (Query Interstate Cases for Kids)
/// system.
/// </summary>
[Serializable]
public partial class QuickAudit: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickAudit()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickAudit(QuickAudit that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickAudit Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickAudit that)
  {
    base.Assign(that);
    systemUserId = that.systemUserId;
    requestTimestamp = that.requestTimestamp;
    requestorId = that.requestorId;
    stateGeneratedId = that.stateGeneratedId;
    startDate = that.startDate;
    endDate = that.endDate;
    dataRequestType = that.dataRequestType;
    providerCaseState = that.providerCaseState;
    providerCaseOtherId = that.providerCaseOtherId;
    requestingCaseState = that.requestingCaseState;
    requestingCaseId = that.requestingCaseId;
    requestingCaseOtherId = that.requestingCaseOtherId;
    systemServerId = that.systemServerId;
    systemResponseCode = that.systemResponseCode;
    dataResponseCode = that.dataResponseCode;
    systemResponseMessage = that.systemResponseMessage;
    dataResponseMessage = that.dataResponseMessage;
  }

  /// <summary>Length of the SYSTEM_USER_ID attribute.</summary>
  public const int SystemUserId_MaxLength = 50;

  /// <summary>
  /// The value of the SYSTEM_USER_ID attribute.
  /// An identifier that associates a user with access or authorization to a 
  /// system.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = SystemUserId_MaxLength)]
  public string SystemUserId
  {
    get => systemUserId ?? "";
    set => systemUserId = TrimEnd(Substring(value, 1, SystemUserId_MaxLength));
  }

  /// <summary>
  /// The json value of the SystemUserId attribute.</summary>
  [JsonPropertyName("systemUserId")]
  [Computed]
  public string SystemUserId_Json
  {
    get => NullIf(SystemUserId, "");
    set => SystemUserId = value;
  }

  /// <summary>
  /// The value of the REQUEST_TIMESTAMP attribute.
  /// The date and time that a request for information was made.
  /// </summary>
  [JsonPropertyName("requestTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? RequestTimestamp
  {
    get => requestTimestamp;
    set => requestTimestamp = value;
  }

  /// <summary>Length of the REQUESTOR_ID attribute.</summary>
  public const int RequestorId_MaxLength = 5;

  /// <summary>
  /// The value of the REQUESTOR_ID attribute.
  /// Unique identifier for a request. First two characters are state number, 
  /// followed by three additional characters (suggest 000).
  /// Example:
  /// STATE     Requestor ID
  /// 
  /// Kansas       20000
  /// 
  /// Colorado     08000
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = RequestorId_MaxLength)]
  public string RequestorId
  {
    get => requestorId ?? "";
    set => requestorId = TrimEnd(Substring(value, 1, RequestorId_MaxLength));
  }

  /// <summary>
  /// The json value of the RequestorId attribute.</summary>
  [JsonPropertyName("requestorId")]
  [Computed]
  public string RequestorId_Json
  {
    get => NullIf(RequestorId, "");
    set => RequestorId = value;
  }

  /// <summary>Length of the STATE_GENERATED_ID attribute.</summary>
  public const int StateGeneratedId_MaxLength = 40;

  /// <summary>
  /// The value of the STATE_GENERATED_ID attribute.
  /// Unique identifier supplied by a state to associate a case query to a 
  /// specific request.  For Kansas, this is a generated number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Varchar, Length
    = StateGeneratedId_MaxLength)]
  public string StateGeneratedId
  {
    get => stateGeneratedId ?? "";
    set => stateGeneratedId = Substring(value, 1, StateGeneratedId_MaxLength);
  }

  /// <summary>
  /// The json value of the StateGeneratedId attribute.</summary>
  [JsonPropertyName("stateGeneratedId")]
  [Computed]
  public string StateGeneratedId_Json
  {
    get => NullIf(StateGeneratedId, "");
    set => StateGeneratedId = value;
  }

  /// <summary>
  /// The value of the START_DATE attribute.
  /// The starting or beginning point in a date range.
  /// </summary>
  [JsonPropertyName("startDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? StartDate
  {
    get => startDate;
    set => startDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The ending or finishing point in a date range.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>Length of the DATA_REQUEST_TYPE attribute.</summary>
  public const int DataRequestType_MaxLength = 20;

  /// <summary>
  /// The value of the DATA_REQUEST_TYPE attribute.
  /// The type of data that is being 
  /// requested in the transaction.
  /// 
  /// Values: financial, caseactivity,
  /// caseparticipants, contactinformation
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Varchar, Length
    = DataRequestType_MaxLength)]
  public string DataRequestType
  {
    get => dataRequestType ?? "";
    set => dataRequestType = Substring(value, 1, DataRequestType_MaxLength);
  }

  /// <summary>
  /// The json value of the DataRequestType attribute.</summary>
  [JsonPropertyName("dataRequestType")]
  [Computed]
  public string DataRequestType_Json
  {
    get => NullIf(DataRequestType, "");
    set => DataRequestType = value;
  }

  /// <summary>Length of the PROVIDER_CASE_STATE attribute.</summary>
  public const int ProviderCaseState_MaxLength = 40;

  /// <summary>
  /// The value of the PROVIDER_CASE_STATE attribute.
  /// The full name of the State Tribe Territory providing case information for 
  /// a specific case request.
  /// 
  /// Example:	 KANSAS, MISSOURI
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Varchar, Length
    = ProviderCaseState_MaxLength)]
  public string ProviderCaseState
  {
    get => providerCaseState ?? "";
    set => providerCaseState = Substring(value, 1, ProviderCaseState_MaxLength);
  }

  /// <summary>
  /// The json value of the ProviderCaseState attribute.</summary>
  [JsonPropertyName("providerCaseState")]
  [Computed]
  public string ProviderCaseState_Json
  {
    get => NullIf(ProviderCaseState, "");
    set => ProviderCaseState = value;
  }

  /// <summary>Length of the PROVIDER_CASE_OTHER_ID attribute.</summary>
  public const int ProviderCaseOtherId_MaxLength = 15;

  /// <summary>
  /// The value of the PROVIDER_CASE_OTHER_ID attribute.
  /// This is what the providing (responding) state shows as the original state 
  /// case number if different than what came through on the original quick
  /// request.              Example: KS queries VA.   VA shows a different KS #
  /// on their case so they return it in this field.  Only completed if the
  /// providing state shows a different case number for the requesting state,
  /// otherwise blank.
  /// </summary>
  [JsonPropertyName("providerCaseOtherId")]
  [Member(Index = 9, Type = MemberType.Varchar, Length
    = ProviderCaseOtherId_MaxLength, Optional = true)]
  public string ProviderCaseOtherId
  {
    get => providerCaseOtherId;
    set => providerCaseOtherId = value != null
      ? Substring(value, 1, ProviderCaseOtherId_MaxLength) : null;
  }

  /// <summary>Length of the REQUESTING_CASE_STATE attribute.</summary>
  public const int RequestingCaseState_MaxLength = 40;

  /// <summary>
  /// The value of the REQUESTING_CASE_STATE attribute.
  /// The full name of the State Tribe Territory issuing a request for case 
  /// information.     Example: KANSAS, MISSOURI
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Varchar, Length
    = RequestingCaseState_MaxLength)]
  public string RequestingCaseState
  {
    get => requestingCaseState ?? "";
    set => requestingCaseState =
      Substring(value, 1, RequestingCaseState_MaxLength);
  }

  /// <summary>
  /// The json value of the RequestingCaseState attribute.</summary>
  [JsonPropertyName("requestingCaseState")]
  [Computed]
  public string RequestingCaseState_Json
  {
    get => NullIf(RequestingCaseState, "");
    set => RequestingCaseState = value;
  }

  /// <summary>Length of the REQUESTING_CASE_ID attribute.</summary>
  public const int RequestingCaseId_MaxLength = 15;

  /// <summary>
  /// The value of the REQUESTING_CASE_ID attribute.
  /// Case ID number
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = RequestingCaseId_MaxLength)]
  public string RequestingCaseId
  {
    get => requestingCaseId ?? "";
    set => requestingCaseId =
      TrimEnd(Substring(value, 1, RequestingCaseId_MaxLength));
  }

  /// <summary>
  /// The json value of the RequestingCaseId attribute.</summary>
  [JsonPropertyName("requestingCaseId")]
  [Computed]
  public string RequestingCaseId_Json
  {
    get => NullIf(RequestingCaseId, "");
    set => RequestingCaseId = value;
  }

  /// <summary>Length of the REQUESTING_CASE_OTHER_ID attribute.</summary>
  public const int RequestingCaseOtherId_MaxLength = 15;

  /// <summary>
  /// The value of the REQUESTING_CASE_OTHER_ID attribute.
  /// Other state case number that the requesting case state believes is the 
  /// correct number for the other state.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = RequestingCaseOtherId_MaxLength)]
  public string RequestingCaseOtherId
  {
    get => requestingCaseOtherId ?? "";
    set => requestingCaseOtherId =
      TrimEnd(Substring(value, 1, RequestingCaseOtherId_MaxLength));
  }

  /// <summary>
  /// The json value of the RequestingCaseOtherId attribute.</summary>
  [JsonPropertyName("requestingCaseOtherId")]
  [Computed]
  public string RequestingCaseOtherId_Json
  {
    get => NullIf(RequestingCaseOtherId, "");
    set => RequestingCaseOtherId = value;
  }

  /// <summary>Length of the SYSTEM_SERVER_ID attribute.</summary>
  public const int SystemServerId_MaxLength = 39;

  /// <summary>
  /// The value of the SYSTEM_SERVER_ID attribute.
  /// Identifier that uniquely defines a SERVER ADDRESS within a network.  IPv6 
  /// is used.
  /// 
  /// Example: 2001:0db8:85a3:0000:0000:8a2e:0370:7334
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = SystemServerId_MaxLength)]
    
  public string SystemServerId
  {
    get => systemServerId ?? "";
    set => systemServerId =
      TrimEnd(Substring(value, 1, SystemServerId_MaxLength));
  }

  /// <summary>
  /// The json value of the SystemServerId attribute.</summary>
  [JsonPropertyName("systemServerId")]
  [Computed]
  public string SystemServerId_Json
  {
    get => NullIf(SystemServerId, "");
    set => SystemServerId = value;
  }

  /// <summary>Length of the SYSTEM_RESPONSE_CODE attribute.</summary>
  public const int SystemResponseCode_MaxLength = 5;

  /// <summary>
  /// The value of the SYSTEM_RESPONSE_CODE attribute.
  /// Code defining a system related success or failure
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = SystemResponseCode_MaxLength)]
  public string SystemResponseCode
  {
    get => systemResponseCode ?? "";
    set => systemResponseCode =
      TrimEnd(Substring(value, 1, SystemResponseCode_MaxLength));
  }

  /// <summary>
  /// The json value of the SystemResponseCode attribute.</summary>
  [JsonPropertyName("systemResponseCode")]
  [Computed]
  public string SystemResponseCode_Json
  {
    get => NullIf(SystemResponseCode, "");
    set => SystemResponseCode = value;
  }

  /// <summary>Length of the DATA_RESPONSE_CODE attribute.</summary>
  public const int DataResponseCode_MaxLength = 5;

  /// <summary>
  /// The value of the DATA_RESPONSE_CODE attribute.
  /// Code  defining a data related success or failure
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = DataResponseCode_MaxLength)]
  public string DataResponseCode
  {
    get => dataResponseCode ?? "";
    set => dataResponseCode =
      TrimEnd(Substring(value, 1, DataResponseCode_MaxLength));
  }

  /// <summary>
  /// The json value of the DataResponseCode attribute.</summary>
  [JsonPropertyName("dataResponseCode")]
  [Computed]
  public string DataResponseCode_Json
  {
    get => NullIf(DataResponseCode, "");
    set => DataResponseCode = value;
  }

  /// <summary>Length of the SYSTEM_RESPONSE_MESSAGE attribute.</summary>
  public const int SystemResponseMessage_MaxLength = 255;

  /// <summary>
  /// The value of the SYSTEM_RESPONSE_MESSAGE attribute.
  ///  System text explanation of success or failure in processing
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Varchar, Length
    = SystemResponseMessage_MaxLength)]
  public string SystemResponseMessage
  {
    get => systemResponseMessage ?? "";
    set => systemResponseMessage =
      Substring(value, 1, SystemResponseMessage_MaxLength);
  }

  /// <summary>
  /// The json value of the SystemResponseMessage attribute.</summary>
  [JsonPropertyName("systemResponseMessage")]
  [Computed]
  public string SystemResponseMessage_Json
  {
    get => NullIf(SystemResponseMessage, "");
    set => SystemResponseMessage = value;
  }

  /// <summary>Length of the DATA_RESPONSE_MESSAGE attribute.</summary>
  public const int DataResponseMessage_MaxLength = 255;

  /// <summary>
  /// The value of the DATA_RESPONSE_MESSAGE attribute.
  /// Data text explanation of failure or success in processing
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Varchar, Length
    = DataResponseMessage_MaxLength)]
  public string DataResponseMessage
  {
    get => dataResponseMessage ?? "";
    set => dataResponseMessage =
      Substring(value, 1, DataResponseMessage_MaxLength);
  }

  /// <summary>
  /// The json value of the DataResponseMessage attribute.</summary>
  [JsonPropertyName("dataResponseMessage")]
  [Computed]
  public string DataResponseMessage_Json
  {
    get => NullIf(DataResponseMessage, "");
    set => DataResponseMessage = value;
  }

  private string systemUserId;
  private DateTime? requestTimestamp;
  private string requestorId;
  private string stateGeneratedId;
  private DateTime? startDate;
  private DateTime? endDate;
  private string dataRequestType;
  private string providerCaseState;
  private string providerCaseOtherId;
  private string requestingCaseState;
  private string requestingCaseId;
  private string requestingCaseOtherId;
  private string systemServerId;
  private string systemResponseCode;
  private string dataResponseCode;
  private string systemResponseMessage;
  private string dataResponseMessage;
}
