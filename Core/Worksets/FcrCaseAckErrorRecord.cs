// The source file: FCR_CASE_ACK_ERROR_RECORD, ID: 373550718, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This record is returned to the state or territory at least once for each 
/// Input Case received by the FCR.
/// This record provides the submitter with the information necessary to 
/// synchronize the FCR data with the information on the State's or territory's
/// system.
/// </summary>
[Serializable]
public partial class FcrCaseAckErrorRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrCaseAckErrorRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrCaseAckErrorRecord(FcrCaseAckErrorRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrCaseAckErrorRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrCaseAckErrorRecord that)
  {
    base.Assign(that);
    recordIdentifier = that.recordIdentifier;
    actionTypeCode = that.actionTypeCode;
    caseId = that.caseId;
    caseType = that.caseType;
    orderIndicator = that.orderIndicator;
    fipsCountyCode = that.fipsCountyCode;
    userField = that.userField;
    previousCaseId = that.previousCaseId;
    batchNumber = that.batchNumber;
    acknowledgementCode = that.acknowledgementCode;
    errorCode1 = that.errorCode1;
    errorMessage1 = that.errorMessage1;
    errorCode2 = that.errorCode2;
    errorMessage2 = that.errorMessage2;
    errorCode3 = that.errorCode3;
    errorMessage3 = that.errorMessage3;
    errorCode4 = that.errorCode4;
    errorMessage4 = that.errorMessage4;
    errorCode5 = that.errorCode5;
    errorMessage5 = that.errorMessage5;
  }

  /// <summary>Length of the RECORD_IDENTIFIER attribute.</summary>
  public const int RecordIdentifier_MaxLength = 2;

  /// <summary>
  /// The value of the RECORD_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordIdentifier_MaxLength)
    ]
  public string RecordIdentifier
  {
    get => recordIdentifier ?? "";
    set => recordIdentifier =
      TrimEnd(Substring(value, 1, RecordIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordIdentifier attribute.</summary>
  [JsonPropertyName("recordIdentifier")]
  [Computed]
  public string RecordIdentifier_Json
  {
    get => NullIf(RecordIdentifier, "");
    set => RecordIdentifier = value;
  }

  /// <summary>Length of the ACTION_TYPE_CODE attribute.</summary>
  public const int ActionTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION_TYPE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ActionTypeCode_MaxLength)]
  public string ActionTypeCode
  {
    get => actionTypeCode ?? "";
    set => actionTypeCode =
      TrimEnd(Substring(value, 1, ActionTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ActionTypeCode attribute.</summary>
  [JsonPropertyName("actionTypeCode")]
  [Computed]
  public string ActionTypeCode_Json
  {
    get => NullIf(ActionTypeCode, "");
    set => ActionTypeCode = value;
  }

  /// <summary>Length of the CASE_ID attribute.</summary>
  public const int CaseId_MaxLength = 15;

  /// <summary>
  /// The value of the CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CaseId_MaxLength)]
  public string CaseId
  {
    get => caseId ?? "";
    set => caseId = TrimEnd(Substring(value, 1, CaseId_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseId attribute.</summary>
  [JsonPropertyName("caseId")]
  [Computed]
  public string CaseId_Json
  {
    get => NullIf(CaseId, "");
    set => CaseId = value;
  }

  /// <summary>Length of the CASE_TYPE attribute.</summary>
  public const int CaseType_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CaseType_MaxLength)]
  public string CaseType
  {
    get => caseType ?? "";
    set => caseType = TrimEnd(Substring(value, 1, CaseType_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseType attribute.</summary>
  [JsonPropertyName("caseType")]
  [Computed]
  public string CaseType_Json
  {
    get => NullIf(CaseType, "");
    set => CaseType = value;
  }

  /// <summary>Length of the ORDER_INDICATOR attribute.</summary>
  public const int OrderIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the ORDER_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = OrderIndicator_MaxLength)]
  public string OrderIndicator
  {
    get => orderIndicator ?? "";
    set => orderIndicator =
      TrimEnd(Substring(value, 1, OrderIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the OrderIndicator attribute.</summary>
  [JsonPropertyName("orderIndicator")]
  [Computed]
  public string OrderIndicator_Json
  {
    get => NullIf(OrderIndicator, "");
    set => OrderIndicator = value;
  }

  /// <summary>Length of the FIPS_COUNTY_CODE attribute.</summary>
  public const int FipsCountyCode_MaxLength = 3;

  /// <summary>
  /// The value of the FIPS_COUNTY_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = FipsCountyCode_MaxLength)]
  public string FipsCountyCode
  {
    get => fipsCountyCode ?? "";
    set => fipsCountyCode =
      TrimEnd(Substring(value, 1, FipsCountyCode_MaxLength));
  }

  /// <summary>
  /// The json value of the FipsCountyCode attribute.</summary>
  [JsonPropertyName("fipsCountyCode")]
  [Computed]
  public string FipsCountyCode_Json
  {
    get => NullIf(FipsCountyCode, "");
    set => FipsCountyCode = value;
  }

  /// <summary>Length of the USER_FIELD attribute.</summary>
  public const int UserField_MaxLength = 15;

  /// <summary>
  /// The value of the USER_FIELD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = UserField_MaxLength)]
  public string UserField
  {
    get => userField ?? "";
    set => userField = TrimEnd(Substring(value, 1, UserField_MaxLength));
  }

  /// <summary>
  /// The json value of the UserField attribute.</summary>
  [JsonPropertyName("userField")]
  [Computed]
  public string UserField_Json
  {
    get => NullIf(UserField, "");
    set => UserField = value;
  }

  /// <summary>Length of the PREVIOUS_CASE_ID attribute.</summary>
  public const int PreviousCaseId_MaxLength = 15;

  /// <summary>
  /// The value of the PREVIOUS_CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = PreviousCaseId_MaxLength)]
  public string PreviousCaseId
  {
    get => previousCaseId ?? "";
    set => previousCaseId =
      TrimEnd(Substring(value, 1, PreviousCaseId_MaxLength));
  }

  /// <summary>
  /// The json value of the PreviousCaseId attribute.</summary>
  [JsonPropertyName("previousCaseId")]
  [Computed]
  public string PreviousCaseId_Json
  {
    get => NullIf(PreviousCaseId, "");
    set => PreviousCaseId = value;
  }

  /// <summary>Length of the BATCH_NUMBER attribute.</summary>
  public const int BatchNumber_MaxLength = 6;

  /// <summary>
  /// The value of the BATCH_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = BatchNumber_MaxLength)]
  public string BatchNumber
  {
    get => batchNumber ?? "";
    set => batchNumber = TrimEnd(Substring(value, 1, BatchNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the BatchNumber attribute.</summary>
  [JsonPropertyName("batchNumber")]
  [Computed]
  public string BatchNumber_Json
  {
    get => NullIf(BatchNumber, "");
    set => BatchNumber = value;
  }

  /// <summary>Length of the ACKNOWLEDGEMENT_CODE attribute.</summary>
  public const int AcknowledgementCode_MaxLength = 5;

  /// <summary>
  /// The value of the ACKNOWLEDGEMENT_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = AcknowledgementCode_MaxLength)]
  public string AcknowledgementCode
  {
    get => acknowledgementCode ?? "";
    set => acknowledgementCode =
      TrimEnd(Substring(value, 1, AcknowledgementCode_MaxLength));
  }

  /// <summary>
  /// The json value of the AcknowledgementCode attribute.</summary>
  [JsonPropertyName("acknowledgementCode")]
  [Computed]
  public string AcknowledgementCode_Json
  {
    get => NullIf(AcknowledgementCode, "");
    set => AcknowledgementCode = value;
  }

  /// <summary>Length of the ERROR_CODE_1 attribute.</summary>
  public const int ErrorCode1_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = ErrorCode1_MaxLength)]
  public string ErrorCode1
  {
    get => errorCode1 ?? "";
    set => errorCode1 = TrimEnd(Substring(value, 1, ErrorCode1_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorCode1 attribute.</summary>
  [JsonPropertyName("errorCode1")]
  [Computed]
  public string ErrorCode1_Json
  {
    get => NullIf(ErrorCode1, "");
    set => ErrorCode1 = value;
  }

  /// <summary>Length of the ERROR_MESSAGE_1 attribute.</summary>
  public const int ErrorMessage1_MaxLength = 50;

  /// <summary>
  /// The value of the ERROR_MESSAGE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = ErrorMessage1_MaxLength)]
  public string ErrorMessage1
  {
    get => errorMessage1 ?? "";
    set => errorMessage1 =
      TrimEnd(Substring(value, 1, ErrorMessage1_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorMessage1 attribute.</summary>
  [JsonPropertyName("errorMessage1")]
  [Computed]
  public string ErrorMessage1_Json
  {
    get => NullIf(ErrorMessage1, "");
    set => ErrorMessage1 = value;
  }

  /// <summary>Length of the ERROR_CODE_2 attribute.</summary>
  public const int ErrorCode2_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = ErrorCode2_MaxLength)]
  public string ErrorCode2
  {
    get => errorCode2 ?? "";
    set => errorCode2 = TrimEnd(Substring(value, 1, ErrorCode2_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorCode2 attribute.</summary>
  [JsonPropertyName("errorCode2")]
  [Computed]
  public string ErrorCode2_Json
  {
    get => NullIf(ErrorCode2, "");
    set => ErrorCode2 = value;
  }

  /// <summary>Length of the ERROR_MESSAGE_2 attribute.</summary>
  public const int ErrorMessage2_MaxLength = 50;

  /// <summary>
  /// The value of the ERROR_MESSAGE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = ErrorMessage2_MaxLength)]
  public string ErrorMessage2
  {
    get => errorMessage2 ?? "";
    set => errorMessage2 =
      TrimEnd(Substring(value, 1, ErrorMessage2_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorMessage2 attribute.</summary>
  [JsonPropertyName("errorMessage2")]
  [Computed]
  public string ErrorMessage2_Json
  {
    get => NullIf(ErrorMessage2, "");
    set => ErrorMessage2 = value;
  }

  /// <summary>Length of the ERROR_CODE_3 attribute.</summary>
  public const int ErrorCode3_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = ErrorCode3_MaxLength)]
  public string ErrorCode3
  {
    get => errorCode3 ?? "";
    set => errorCode3 = TrimEnd(Substring(value, 1, ErrorCode3_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorCode3 attribute.</summary>
  [JsonPropertyName("errorCode3")]
  [Computed]
  public string ErrorCode3_Json
  {
    get => NullIf(ErrorCode3, "");
    set => ErrorCode3 = value;
  }

  /// <summary>Length of the ERROR_MESSAGE_3 attribute.</summary>
  public const int ErrorMessage3_MaxLength = 50;

  /// <summary>
  /// The value of the ERROR_MESSAGE_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = ErrorMessage3_MaxLength)]
  public string ErrorMessage3
  {
    get => errorMessage3 ?? "";
    set => errorMessage3 =
      TrimEnd(Substring(value, 1, ErrorMessage3_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorMessage3 attribute.</summary>
  [JsonPropertyName("errorMessage3")]
  [Computed]
  public string ErrorMessage3_Json
  {
    get => NullIf(ErrorMessage3, "");
    set => ErrorMessage3 = value;
  }

  /// <summary>Length of the ERROR_CODE_4 attribute.</summary>
  public const int ErrorCode4_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = ErrorCode4_MaxLength)]
  public string ErrorCode4
  {
    get => errorCode4 ?? "";
    set => errorCode4 = TrimEnd(Substring(value, 1, ErrorCode4_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorCode4 attribute.</summary>
  [JsonPropertyName("errorCode4")]
  [Computed]
  public string ErrorCode4_Json
  {
    get => NullIf(ErrorCode4, "");
    set => ErrorCode4 = value;
  }

  /// <summary>Length of the ERROR_MESSAGE_4 attribute.</summary>
  public const int ErrorMessage4_MaxLength = 50;

  /// <summary>
  /// The value of the ERROR_MESSAGE_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = ErrorMessage4_MaxLength)]
  public string ErrorMessage4
  {
    get => errorMessage4 ?? "";
    set => errorMessage4 =
      TrimEnd(Substring(value, 1, ErrorMessage4_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorMessage4 attribute.</summary>
  [JsonPropertyName("errorMessage4")]
  [Computed]
  public string ErrorMessage4_Json
  {
    get => NullIf(ErrorMessage4, "");
    set => ErrorMessage4 = value;
  }

  /// <summary>Length of the ERROR_CODE_5 attribute.</summary>
  public const int ErrorCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_5 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = ErrorCode5_MaxLength)]
  public string ErrorCode5
  {
    get => errorCode5 ?? "";
    set => errorCode5 = TrimEnd(Substring(value, 1, ErrorCode5_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorCode5 attribute.</summary>
  [JsonPropertyName("errorCode5")]
  [Computed]
  public string ErrorCode5_Json
  {
    get => NullIf(ErrorCode5, "");
    set => ErrorCode5 = value;
  }

  /// <summary>Length of the ERROR_MESSAGE_5 attribute.</summary>
  public const int ErrorMessage5_MaxLength = 50;

  /// <summary>
  /// The value of the ERROR_MESSAGE_5 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = ErrorMessage5_MaxLength)]
  public string ErrorMessage5
  {
    get => errorMessage5 ?? "";
    set => errorMessage5 =
      TrimEnd(Substring(value, 1, ErrorMessage5_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorMessage5 attribute.</summary>
  [JsonPropertyName("errorMessage5")]
  [Computed]
  public string ErrorMessage5_Json
  {
    get => NullIf(ErrorMessage5, "");
    set => ErrorMessage5 = value;
  }

  private string recordIdentifier;
  private string actionTypeCode;
  private string caseId;
  private string caseType;
  private string orderIndicator;
  private string fipsCountyCode;
  private string userField;
  private string previousCaseId;
  private string batchNumber;
  private string acknowledgementCode;
  private string errorCode1;
  private string errorMessage1;
  private string errorCode2;
  private string errorMessage2;
  private string errorCode3;
  private string errorMessage3;
  private string errorCode4;
  private string errorMessage4;
  private string errorCode5;
  private string errorMessage5;
}
