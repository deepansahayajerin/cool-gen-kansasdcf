// The source file: INTERSTATE_CASE, ID: 371436111, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Case-related attributes and CSENet Header attributes of a referral.  This 
/// entity type is the core record for all CSENet transmissions.
/// </summary>
[Serializable]
public partial class InterstateCase: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateCase()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateCase(InterstateCase that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateCase Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the LOCAL_FIPS_STATE attribute.
  /// The KS State FIPS
  /// </summary>
  [JsonPropertyName("localFipsState")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 2)]
  public int LocalFipsState
  {
    get => Get<int?>("localFipsState") ?? 0;
    set => Set("localFipsState", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the LOCAL_FIPS_COUNTY attribute.
  /// The KS County FIPS
  /// </summary>
  [JsonPropertyName("localFipsCounty")]
  [Member(Index = 2, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? LocalFipsCounty
  {
    get => Get<int?>("localFipsCounty");
    set => Set("localFipsCounty", value);
  }

  /// <summary>
  /// The value of the LOCAL_FIPS_LOCATION attribute.
  /// The KS SUB (office) FIPS
  /// </summary>
  [JsonPropertyName("localFipsLocation")]
  [Member(Index = 3, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? LocalFipsLocation
  {
    get => Get<int?>("localFipsLocation");
    set => Set("localFipsLocation", value);
  }

  /// <summary>
  /// The value of the OTHER_FIPS_STATE attribute.
  /// The other state's State FIPS
  /// </summary>
  [JsonPropertyName("otherFipsState")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 2)]
  public int OtherFipsState
  {
    get => Get<int?>("otherFipsState") ?? 0;
    set => Set("otherFipsState", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the OTHER_FIPS_COUNTY attribute.
  /// The other state's County FIPS
  /// </summary>
  [JsonPropertyName("otherFipsCounty")]
  [Member(Index = 5, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OtherFipsCounty
  {
    get => Get<int?>("otherFipsCounty");
    set => Set("otherFipsCounty", value);
  }

  /// <summary>
  /// The value of the OTHER_FIPS_LOCATION attribute.
  /// The other state's SUB (office) FIPS
  /// </summary>
  [JsonPropertyName("otherFipsLocation")]
  [Member(Index = 6, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? OtherFipsLocation
  {
    get => Get<int?>("otherFipsLocation");
    set => Set("otherFipsLocation", value);
  }

  /// <summary>
  /// The value of the TRANS_SERIAL_NUMBER attribute.
  /// This is a unique number assigned to each CSENet transaction.  It has no 
  /// place in the KESSEP system but is required to provide a key used to
  /// process CSENet Referrals.
  /// </summary>
  [JsonPropertyName("transSerialNumber")]
  [DefaultValue(0L)]
  [Member(Index = 7, Type = MemberType.Number, Length = 12)]
  public long TransSerialNumber
  {
    get => Get<long?>("transSerialNumber") ?? 0L;
    set => Set("transSerialNumber", value == 0L ? null : value as long?);
  }

  /// <summary>Length of the ACTION_CODE attribute.</summary>
  public const int ActionCode_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION_CODE attribute.
  /// Describes the action of this CSENet referral.
  /// Values:
  /// R = Request
  /// A = Acknowledgement of receipt
  /// P = Provision of Data
  /// M = Reminder
  /// U = Update of previously transmitted request
  /// C = Cancel
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ActionCode_MaxLength)]
  public string ActionCode
  {
    get => Get<string>("actionCode") ?? "";
    set => Set(
      "actionCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ActionCode_MaxLength)));
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

  /// <summary>Length of the FUNCTIONAL_TYPE_CODE attribute.</summary>
  public const int FunctionalTypeCode_MaxLength = 3;

  /// <summary>
  /// The value of the FUNCTIONAL_TYPE_CODE attribute.
  /// Describes the CSE activity requested by the CSENet referral.
  /// Values:
  /// LO1 = Quick Locate
  /// LO2 = Full Locate
  /// PAT = Paternity Establishment
  /// EST = Order Establishment
  /// ENF = Enforcement
  /// COL = Collection
  /// MSC = Miscellaneous
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = FunctionalTypeCode_MaxLength)]
  public string FunctionalTypeCode
  {
    get => Get<string>("functionalTypeCode") ?? "";
    set => Set(
      "functionalTypeCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, FunctionalTypeCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the FunctionalTypeCode attribute.</summary>
  [JsonPropertyName("functionalTypeCode")]
  [Computed]
  public string FunctionalTypeCode_Json
  {
    get => NullIf(FunctionalTypeCode, "");
    set => FunctionalTypeCode = value;
  }

  /// <summary>
  /// The value of the TRANSACTION_DATE attribute.
  /// This is the date on which CSENet transmitted the Referral.
  /// </summary>
  [JsonPropertyName("transactionDate")]
  [Member(Index = 10, Type = MemberType.Date)]
  public DateTime? TransactionDate
  {
    get => Get<DateTime?>("transactionDate");
    set => Set("transactionDate", value);
  }

  /// <summary>Length of the KS_CASE_ID attribute.</summary>
  public const int KsCaseId_MaxLength = 15;

  /// <summary>
  /// The value of the KS_CASE_ID attribute.
  /// This is the KESSEP Case Id.  It will not be present when a new CSENet 
  /// Referral is received.  It should be present when a CSENet Referral update
  /// comes in.
  /// </summary>
  [JsonPropertyName("ksCaseId")]
  [Member(Index = 11, Type = MemberType.Char, Length = KsCaseId_MaxLength, Optional
    = true)]
  public string KsCaseId
  {
    get => Get<string>("ksCaseId");
    set => Set("ksCaseId", TrimEnd(Substring(value, 1, KsCaseId_MaxLength)));
  }

  /// <summary>Length of the INTERSTATE_CASE_ID attribute.</summary>
  public const int InterstateCaseId_MaxLength = 15;

  /// <summary>
  /// The value of the INTERSTATE_CASE_ID attribute.
  /// The Case Id in the state that originated this CSENet Referral.
  /// </summary>
  [JsonPropertyName("interstateCaseId")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = InterstateCaseId_MaxLength, Optional = true)]
  public string InterstateCaseId
  {
    get => Get<string>("interstateCaseId");
    set => Set(
      "interstateCaseId",
      TrimEnd(Substring(value, 1, InterstateCaseId_MaxLength)));
  }

  /// <summary>Length of the ACTION_REASON_CODE attribute.</summary>
  public const int ActionReasonCode_MaxLength = 5;

  /// <summary>
  /// The value of the ACTION_REASON_CODE attribute.
  /// CSENet field that indicates the reason code associated with this 
  /// transaction.  Sample values are as follows:
  /// GSADD Add a participant
  /// GSDEL Delete a participant
  /// GSCAS Change local case ID
  /// GSFIP Change local FIPS code
  /// </summary>
  [JsonPropertyName("actionReasonCode")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = ActionReasonCode_MaxLength, Optional = true)]
  public string ActionReasonCode
  {
    get => Get<string>("actionReasonCode");
    set => Set(
      "actionReasonCode",
      TrimEnd(Substring(value, 1, ActionReasonCode_MaxLength)));
  }

  /// <summary>
  /// The value of the ACTION_RESOLUTION_DATE attribute.
  /// The date that the 'Action' event occurred.
  /// </summary>
  [JsonPropertyName("actionResolutionDate")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? ActionResolutionDate
  {
    get => Get<DateTime?>("actionResolutionDate");
    set => Set("actionResolutionDate", value);
  }

  /// <summary>Length of the ATTACHMENTS_IND attribute.</summary>
  public const int AttachmentsInd_MaxLength = 1;

  /// <summary>
  /// The value of the ATTACHMENTS_IND attribute.
  /// Indicates whether attachments accompany this transaction.  Options are 
  /// &quot;Y&quot; for yes and &quot;N&quot; for no.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = AttachmentsInd_MaxLength)]
    
  public string AttachmentsInd
  {
    get => Get<string>("attachmentsInd") ?? "";
    set => Set(
      "attachmentsInd", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AttachmentsInd_MaxLength)));
  }

  /// <summary>
  /// The json value of the AttachmentsInd attribute.</summary>
  [JsonPropertyName("attachmentsInd")]
  [Computed]
  public string AttachmentsInd_Json
  {
    get => NullIf(AttachmentsInd, "");
    set => AttachmentsInd = value;
  }

  /// <summary>
  /// The value of the CASE_DATA_IND attribute.
  /// Indicates whether a Case data block is included in this CSENet 
  /// transaction.
  /// 0 - No
  /// 1 - Yes
  /// </summary>
  [JsonPropertyName("caseDataInd")]
  [Member(Index = 16, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? CaseDataInd
  {
    get => Get<int?>("caseDataInd");
    set => Set("caseDataInd", value);
  }

  /// <summary>
  /// The value of the AP_IDENTIFICATION_IND attribute.
  /// Indicates whether an AP Identification Data block is included in this 
  /// CSENet transaction.
  /// 0 - No
  /// 1 - Yes
  /// </summary>
  [JsonPropertyName("apIdentificationInd")]
  [Member(Index = 17, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? ApIdentificationInd
  {
    get => Get<int?>("apIdentificationInd");
    set => Set("apIdentificationInd", value);
  }

  /// <summary>
  /// The value of the AP_LOCATE_DATA_IND attribute.
  /// Indicates whether an AP Locate data block is included in this CSENet 
  /// transaction.
  /// 0   - No
  /// 1   - Yes
  /// </summary>
  [JsonPropertyName("apLocateDataInd")]
  [Member(Index = 18, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? ApLocateDataInd
  {
    get => Get<int?>("apLocateDataInd");
    set => Set("apLocateDataInd", value);
  }

  /// <summary>
  /// The value of the PARTICIPANT_DATA_IND attribute.
  /// Indicates that Participant data block(s) included in this CSENet 
  /// transaction.
  /// 0      - No
  /// 1-9    - Yes
  /// </summary>
  [JsonPropertyName("participantDataInd")]
  [Member(Index = 19, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? ParticipantDataInd
  {
    get => Get<int?>("participantDataInd");
    set => Set("participantDataInd", value);
  }

  /// <summary>
  /// The value of the ORDER_DATA_IND attribute.
  /// Indicates that Support Order data block(s) included
  /// 0       No
  /// 1-9     Yes
  /// </summary>
  [JsonPropertyName("orderDataInd")]
  [Member(Index = 20, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? OrderDataInd
  {
    get => Get<int?>("orderDataInd");
    set => Set("orderDataInd", value);
  }

  /// <summary>
  /// The value of the COLLECTION_DATA_IND attribute.
  /// Indicates that Collection data block(s) included.
  /// 0        - No	
  /// 1-9      - Yes
  /// </summary>
  [JsonPropertyName("collectionDataInd")]
  [Member(Index = 21, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? CollectionDataInd
  {
    get => Get<int?>("collectionDataInd");
    set => Set("collectionDataInd", value);
  }

  /// <summary>
  /// The value of the INFORMATION_IND attribute.
  /// Indicates whether an information data block is included in this CSENet 
  /// transaction.
  /// 0 - No
  /// 1 - Yes
  /// </summary>
  [JsonPropertyName("informationInd")]
  [Member(Index = 22, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? InformationInd
  {
    get => Get<int?>("informationInd");
    set => Set("informationInd", value);
  }

  /// <summary>
  /// The value of the SENT_DATE attribute.
  /// This attribute is used by CSENet to stamp each transaction with the date 
  /// that transaction was placed in the outbound mailbox.  This field should be
  /// blank.
  /// </summary>
  [JsonPropertyName("sentDate")]
  [Member(Index = 23, Type = MemberType.Date, Optional = true)]
  public DateTime? SentDate
  {
    get => Get<DateTime?>("sentDate");
    set => Set("sentDate", value);
  }

  /// <summary>
  /// The value of the SENT_TIME attribute.
  /// This attribute is used by CSENet to stamp each transaction with the time 
  /// that transaction was placed in the outbound mailbox.  This field should be
  /// blank.
  /// </summary>
  [JsonPropertyName("sentTime")]
  [Member(Index = 24, Type = MemberType.Time, Optional = true)]
  public TimeSpan? SentTime
  {
    get => Get<TimeSpan?>("sentTime");
    set => Set("sentTime", value);
  }

  /// <summary>
  /// The value of the DUE_DATE attribute.
  /// CSENet assigns a due date to each transaction based on the entries in the 
  /// Due Date table.  Once the due date has passed and the &quot;Issue Overdue
  /// Reminders Automatically&quot; flag is set to Yes in the System
  /// Configuration table, CSENet will create a reminder and recalculate the due
  /// date for that reminder transaction.  If the statewide system is
  /// automatically generating reminders to CSENet, the &quot;Ussue Overdue
  /// Reminders Automatically&quot; flag should be set to &quot;N&quot; and this
  /// field should be blank.
  /// </summary>
  [JsonPropertyName("dueDate")]
  [Member(Index = 25, Type = MemberType.Date, Optional = true)]
  public DateTime? DueDate
  {
    get => Get<DateTime?>("dueDate");
    set => Set("dueDate", value);
  }

  /// <summary>
  /// The value of the OVERDUE_IND attribute.
  /// This indicates that a response is overdue.  When the due date has passed, 
  /// the overdue indicator is incremented.  Leave this field blank.
  /// </summary>
  [JsonPropertyName("overdueInd")]
  [Member(Index = 26, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? OverdueInd
  {
    get => Get<int?>("overdueInd");
    set => Set("overdueInd", value);
  }

  /// <summary>
  /// The value of the DATE_RECEIVED attribute.
  /// This attribute is used by CSENet to stamp each transaction with the date 
  /// that the specific transaction was received into the inbound mailbox.
  /// </summary>
  [JsonPropertyName("dateReceived")]
  [Member(Index = 27, Type = MemberType.Date, Optional = true)]
  public DateTime? DateReceived
  {
    get => Get<DateTime?>("dateReceived");
    set => Set("dateReceived", value);
  }

  /// <summary>
  /// The value of the TIME_RECEIVED attribute.
  /// This attribute is used by CSENEt to stamp each transaction with the time 
  /// that the specific transaction was received into the inbound mailbox.
  /// </summary>
  [JsonPropertyName("timeReceived")]
  [Member(Index = 28, Type = MemberType.Time, Optional = true)]
  public TimeSpan? TimeReceived
  {
    get => Get<TimeSpan?>("timeReceived");
    set => Set("timeReceived", value);
  }

  /// <summary>
  /// The value of the ATTACHMENTS_DUE_DATE attribute.
  /// Similar to the due date field, this field contains the due date for 
  /// attachments.
  /// </summary>
  [JsonPropertyName("attachmentsDueDate")]
  [Member(Index = 29, Type = MemberType.Date, Optional = true)]
  public DateTime? AttachmentsDueDate
  {
    get => Get<DateTime?>("attachmentsDueDate");
    set => Set("attachmentsDueDate", value);
  }

  /// <summary>Length of the INTERSTATE_FORMS_PRINTED attribute.</summary>
  public const int InterstateFormsPrinted_MaxLength = 1;

  /// <summary>
  /// The value of the INTERSTATE_FORMS_PRINTED attribute.
  /// Indicates that the workstation printed an interstate form for this 
  /// transaction.  Used internally by the application.
  /// </summary>
  [JsonPropertyName("interstateFormsPrinted")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = InterstateFormsPrinted_MaxLength, Optional = true)]
  public string InterstateFormsPrinted
  {
    get => Get<string>("interstateFormsPrinted");
    set => Set(
      "interstateFormsPrinted", TrimEnd(Substring(value, 1,
      InterstateFormsPrinted_MaxLength)));
  }

  /// <summary>Length of the CASE_TYPE attribute.</summary>
  public const int CaseType_MaxLength = 3;

  /// <summary>
  /// The value of the CASE_TYPE attribute.
  /// CSENet Values:
  /// AD-AFDC
  /// NA-Non-AFDC
  /// AF-Foster Care
  /// MA-Medical Need Only
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length = CaseType_MaxLength)]
  public string CaseType
  {
    get => Get<string>("caseType") ?? "";
    set => Set(
      "caseType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CaseType_MaxLength)));
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

  /// <summary>Length of the CASE_STATUS attribute.</summary>
  public const int CaseStatus_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_STATUS attribute.
  /// CSENet Values:
  /// O - Open
  /// C - Closed
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length = CaseStatus_MaxLength)]
  public string CaseStatus
  {
    get => Get<string>("caseStatus") ?? "";
    set => Set(
      "caseStatus", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CaseStatus_MaxLength)));
  }

  /// <summary>
  /// The json value of the CaseStatus attribute.</summary>
  [JsonPropertyName("caseStatus")]
  [Computed]
  public string CaseStatus_Json
  {
    get => NullIf(CaseStatus, "");
    set => CaseStatus = value;
  }

  /// <summary>Length of the PAYMENT_MAILING_ADDRESS_LINE_1 attribute.</summary>
  public const int PaymentMailingAddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the PAYMENT_MAILING_ADDRESS_LINE_1 attribute.
  /// *** DRAFT ***
  /// The first line of the address.
  /// </summary>
  [JsonPropertyName("paymentMailingAddressLine1")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = PaymentMailingAddressLine1_MaxLength, Optional = true)]
  public string PaymentMailingAddressLine1
  {
    get => Get<string>("paymentMailingAddressLine1");
    set => Set(
      "paymentMailingAddressLine1", TrimEnd(Substring(value, 1,
      PaymentMailingAddressLine1_MaxLength)));
  }

  /// <summary>Length of the PAYMENT_ADDRESS_LINE_2 attribute.</summary>
  public const int PaymentAddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the PAYMENT_ADDRESS_LINE_2 attribute.
  /// *** DRAFT ***
  /// The second line of the address.
  /// </summary>
  [JsonPropertyName("paymentAddressLine2")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = PaymentAddressLine2_MaxLength, Optional = true)]
  public string PaymentAddressLine2
  {
    get => Get<string>("paymentAddressLine2");
    set => Set(
      "paymentAddressLine2", TrimEnd(Substring(value, 1,
      PaymentAddressLine2_MaxLength)));
  }

  /// <summary>Length of the PAYMENT_CITY attribute.</summary>
  public const int PaymentCity_MaxLength = 15;

  /// <summary>
  /// The value of the PAYMENT_CITY attribute.
  /// *** DRAFT ***
  /// The city part of the address.
  /// </summary>
  [JsonPropertyName("paymentCity")]
  [Member(Index = 35, Type = MemberType.Char, Length = PaymentCity_MaxLength, Optional
    = true)]
  public string PaymentCity
  {
    get => Get<string>("paymentCity");
    set => Set(
      "paymentCity", TrimEnd(Substring(value, 1, PaymentCity_MaxLength)));
  }

  /// <summary>Length of the PAYMENT_STATE attribute.</summary>
  public const int PaymentState_MaxLength = 2;

  /// <summary>
  /// The value of the PAYMENT_STATE attribute.
  /// *** DRAFT ***
  /// The state part of the address.
  /// </summary>
  [JsonPropertyName("paymentState")]
  [Member(Index = 36, Type = MemberType.Char, Length = PaymentState_MaxLength, Optional
    = true)]
  public string PaymentState
  {
    get => Get<string>("paymentState");
    set => Set(
      "paymentState", TrimEnd(Substring(value, 1, PaymentState_MaxLength)));
  }

  /// <summary>Length of the PAYMENT_ZIP_CODE_5 attribute.</summary>
  public const int PaymentZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the PAYMENT_ZIP_CODE_5 attribute.
  /// *** DRAFT ***
  /// The first five digits of the zip code.
  /// </summary>
  [JsonPropertyName("paymentZipCode5")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = PaymentZipCode5_MaxLength, Optional = true)]
  public string PaymentZipCode5
  {
    get => Get<string>("paymentZipCode5");
    set => Set(
      "paymentZipCode5",
      TrimEnd(Substring(value, 1, PaymentZipCode5_MaxLength)));
  }

  /// <summary>Length of the PAYMENT_ZIP_CODE_4 attribute.</summary>
  public const int PaymentZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the PAYMENT_ZIP_CODE_4 attribute.
  /// *** DRAFT ***
  /// The second part of the zip code.  It consists of four digits.
  /// </summary>
  [JsonPropertyName("paymentZipCode4")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = PaymentZipCode4_MaxLength, Optional = true)]
  public string PaymentZipCode4
  {
    get => Get<string>("paymentZipCode4");
    set => Set(
      "paymentZipCode4",
      TrimEnd(Substring(value, 1, PaymentZipCode4_MaxLength)));
  }

  /// <summary>Length of the ZDEL_CP_ADDR_LINE1 attribute.</summary>
  public const int ZdelCpAddrLine1_MaxLength = 25;

  /// <summary>
  /// The value of the ZDEL_CP_ADDR_LINE1 attribute.
  /// *** DRAFT ***
  /// The first line of the address.
  /// </summary>
  [JsonPropertyName("zdelCpAddrLine1")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = ZdelCpAddrLine1_MaxLength, Optional = true)]
  public string ZdelCpAddrLine1
  {
    get => Get<string>("zdelCpAddrLine1");
    set => Set(
      "zdelCpAddrLine1",
      TrimEnd(Substring(value, 1, ZdelCpAddrLine1_MaxLength)));
  }

  /// <summary>Length of the ZDEL_CP_ADDR_LINE2 attribute.</summary>
  public const int ZdelCpAddrLine2_MaxLength = 25;

  /// <summary>
  /// The value of the ZDEL_CP_ADDR_LINE2 attribute.
  /// *** DRAFT ***
  /// The second line of the address.
  /// </summary>
  [JsonPropertyName("zdelCpAddrLine2")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = ZdelCpAddrLine2_MaxLength, Optional = true)]
  public string ZdelCpAddrLine2
  {
    get => Get<string>("zdelCpAddrLine2");
    set => Set(
      "zdelCpAddrLine2",
      TrimEnd(Substring(value, 1, ZdelCpAddrLine2_MaxLength)));
  }

  /// <summary>Length of the ZDEL_CP_CITY attribute.</summary>
  public const int ZdelCpCity_MaxLength = 15;

  /// <summary>
  /// The value of the ZDEL_CP_CITY attribute.
  /// *** DRAFT ***
  /// The city part of the address.
  /// </summary>
  [JsonPropertyName("zdelCpCity")]
  [Member(Index = 41, Type = MemberType.Char, Length = ZdelCpCity_MaxLength, Optional
    = true)]
  public string ZdelCpCity
  {
    get => Get<string>("zdelCpCity");
    set =>
      Set("zdelCpCity", TrimEnd(Substring(value, 1, ZdelCpCity_MaxLength)));
  }

  /// <summary>Length of the ZDEL_CP_STATE attribute.</summary>
  public const int ZdelCpState_MaxLength = 2;

  /// <summary>
  /// The value of the ZDEL_CP_STATE attribute.
  /// *** DRAFT ***
  /// The state part of the address.
  /// </summary>
  [JsonPropertyName("zdelCpState")]
  [Member(Index = 42, Type = MemberType.Char, Length = ZdelCpState_MaxLength, Optional
    = true)]
  public string ZdelCpState
  {
    get => Get<string>("zdelCpState");
    set => Set(
      "zdelCpState", TrimEnd(Substring(value, 1, ZdelCpState_MaxLength)));
  }

  /// <summary>Length of the ZDEL_CP_ZIP_CODE_5 attribute.</summary>
  public const int ZdelCpZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ZDEL_CP_ZIP_CODE_5 attribute.
  /// *** DRAFT ***
  /// The first five digits of the zip code.
  /// </summary>
  [JsonPropertyName("zdelCpZipCode5")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = ZdelCpZipCode5_MaxLength, Optional = true)]
  public string ZdelCpZipCode5
  {
    get => Get<string>("zdelCpZipCode5");
    set => Set(
      "zdelCpZipCode5", TrimEnd(Substring(value, 1, ZdelCpZipCode5_MaxLength)));
      
  }

  /// <summary>Length of the ZDEL_CP_ZIP_CODE_4 attribute.</summary>
  public const int ZdelCpZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the ZDEL_CP_ZIP_CODE_4 attribute.
  /// *** DRAFT ***
  /// The second part of the zip code.  It consists of four digits.
  /// </summary>
  [JsonPropertyName("zdelCpZipCode4")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = ZdelCpZipCode4_MaxLength, Optional = true)]
  public string ZdelCpZipCode4
  {
    get => Get<string>("zdelCpZipCode4");
    set => Set(
      "zdelCpZipCode4", TrimEnd(Substring(value, 1, ZdelCpZipCode4_MaxLength)));
      
  }

  /// <summary>Length of the CONTACT_NAME_LAST attribute.</summary>
  public const int ContactNameLast_MaxLength = 17;

  /// <summary>
  /// The value of the CONTACT_NAME_LAST attribute.
  /// Contact's last name
  /// </summary>
  [JsonPropertyName("contactNameLast")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = ContactNameLast_MaxLength, Optional = true)]
  public string ContactNameLast
  {
    get => Get<string>("contactNameLast");
    set => Set(
      "contactNameLast",
      TrimEnd(Substring(value, 1, ContactNameLast_MaxLength)));
  }

  /// <summary>Length of the CONTACT_NAME_FIRST attribute.</summary>
  public const int ContactNameFirst_MaxLength = 12;

  /// <summary>
  /// The value of the CONTACT_NAME_FIRST attribute.
  /// Contact's first name
  /// </summary>
  [JsonPropertyName("contactNameFirst")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = ContactNameFirst_MaxLength, Optional = true)]
  public string ContactNameFirst
  {
    get => Get<string>("contactNameFirst");
    set => Set(
      "contactNameFirst",
      TrimEnd(Substring(value, 1, ContactNameFirst_MaxLength)));
  }

  /// <summary>Length of the CONTACT_NAME_MIDDLE attribute.</summary>
  public const int ContactNameMiddle_MaxLength = 1;

  /// <summary>
  /// The value of the CONTACT_NAME_MIDDLE attribute.
  /// Contact's middle initial
  /// </summary>
  [JsonPropertyName("contactNameMiddle")]
  [Member(Index = 47, Type = MemberType.Char, Length
    = ContactNameMiddle_MaxLength, Optional = true)]
  public string ContactNameMiddle
  {
    get => Get<string>("contactNameMiddle");
    set => Set(
      "contactNameMiddle", TrimEnd(Substring(value, 1,
      ContactNameMiddle_MaxLength)));
  }

  /// <summary>Length of the CONTACT_NAME_SUFFIX attribute.</summary>
  public const int ContactNameSuffix_MaxLength = 3;

  /// <summary>
  /// The value of the CONTACT_NAME_SUFFIX attribute.
  /// Contact's name suffix (Jr, Sr, III, etc.)
  /// </summary>
  [JsonPropertyName("contactNameSuffix")]
  [Member(Index = 48, Type = MemberType.Char, Length
    = ContactNameSuffix_MaxLength, Optional = true)]
  public string ContactNameSuffix
  {
    get => Get<string>("contactNameSuffix");
    set => Set(
      "contactNameSuffix", TrimEnd(Substring(value, 1,
      ContactNameSuffix_MaxLength)));
  }

  /// <summary>Length of the CONTACT_ADDRESS_LINE_1 attribute.</summary>
  public const int ContactAddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the CONTACT_ADDRESS_LINE_1 attribute.
  /// *** DRAFT ***
  /// The first line of the address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 49, Type = MemberType.Char, Length
    = ContactAddressLine1_MaxLength)]
  public string ContactAddressLine1
  {
    get => Get<string>("contactAddressLine1") ?? "";
    set => Set(
      "contactAddressLine1", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ContactAddressLine1_MaxLength)));
  }

  /// <summary>
  /// The json value of the ContactAddressLine1 attribute.</summary>
  [JsonPropertyName("contactAddressLine1")]
  [Computed]
  public string ContactAddressLine1_Json
  {
    get => NullIf(ContactAddressLine1, "");
    set => ContactAddressLine1 = value;
  }

  /// <summary>Length of the CONTACT_ADDRESS_LINE_2 attribute.</summary>
  public const int ContactAddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the CONTACT_ADDRESS_LINE_2 attribute.
  /// *** DRAFT ***
  /// The second line of the address.
  /// </summary>
  [JsonPropertyName("contactAddressLine2")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = ContactAddressLine2_MaxLength, Optional = true)]
  public string ContactAddressLine2
  {
    get => Get<string>("contactAddressLine2");
    set => Set(
      "contactAddressLine2", TrimEnd(Substring(value, 1,
      ContactAddressLine2_MaxLength)));
  }

  /// <summary>Length of the CONTACT_CITY attribute.</summary>
  public const int ContactCity_MaxLength = 15;

  /// <summary>
  /// The value of the CONTACT_CITY attribute.
  /// *** DRAFT ***
  /// The city part of the address.
  /// </summary>
  [JsonPropertyName("contactCity")]
  [Member(Index = 51, Type = MemberType.Char, Length = ContactCity_MaxLength, Optional
    = true)]
  public string ContactCity
  {
    get => Get<string>("contactCity");
    set => Set(
      "contactCity", TrimEnd(Substring(value, 1, ContactCity_MaxLength)));
  }

  /// <summary>Length of the CONTACT_STATE attribute.</summary>
  public const int ContactState_MaxLength = 2;

  /// <summary>
  /// The value of the CONTACT_STATE attribute.
  /// *** DRAFT ***
  /// The state part of the address.
  /// </summary>
  [JsonPropertyName("contactState")]
  [Member(Index = 52, Type = MemberType.Char, Length = ContactState_MaxLength, Optional
    = true)]
  public string ContactState
  {
    get => Get<string>("contactState");
    set => Set(
      "contactState", TrimEnd(Substring(value, 1, ContactState_MaxLength)));
  }

  /// <summary>Length of the CONTACT_ZIP_CODE_5 attribute.</summary>
  public const int ContactZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the CONTACT_ZIP_CODE_5 attribute.
  /// *** DRAFT ***
  /// The first five digits of the zip code.
  /// </summary>
  [JsonPropertyName("contactZipCode5")]
  [Member(Index = 53, Type = MemberType.Char, Length
    = ContactZipCode5_MaxLength, Optional = true)]
  public string ContactZipCode5
  {
    get => Get<string>("contactZipCode5");
    set => Set(
      "contactZipCode5",
      TrimEnd(Substring(value, 1, ContactZipCode5_MaxLength)));
  }

  /// <summary>Length of the CONTACT_ZIP_CODE_4 attribute.</summary>
  public const int ContactZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the CONTACT_ZIP_CODE_4 attribute.
  /// *** DRAFT ***
  /// The second part of the zip code.  It consists of four digits.
  /// </summary>
  [JsonPropertyName("contactZipCode4")]
  [Member(Index = 54, Type = MemberType.Char, Length
    = ContactZipCode4_MaxLength, Optional = true)]
  public string ContactZipCode4
  {
    get => Get<string>("contactZipCode4");
    set => Set(
      "contactZipCode4",
      TrimEnd(Substring(value, 1, ContactZipCode4_MaxLength)));
  }

  /// <summary>
  /// The value of the CONTACT_PHONE_NUM attribute.
  /// Work phone for Contact
  /// </summary>
  [JsonPropertyName("contactPhoneNum")]
  [Member(Index = 55, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? ContactPhoneNum
  {
    get => Get<int?>("contactPhoneNum");
    set => Set("contactPhoneNum", value);
  }

  /// <summary>
  /// The value of the ASSN_DEACT_DT attribute.
  /// This is the date on which a KS Interstate worker made the decision to 
  /// accept or decline the Referral.
  /// </summary>
  [JsonPropertyName("assnDeactDt")]
  [Member(Index = 56, Type = MemberType.Date, Optional = true)]
  public DateTime? AssnDeactDt
  {
    get => Get<DateTime?>("assnDeactDt");
    set => Set("assnDeactDt", value);
  }

  /// <summary>Length of the ASSN_DEACT_IND attribute.</summary>
  public const int AssnDeactInd_MaxLength = 1;

  /// <summary>
  /// The value of the ASSN_DEACT_IND attribute.
  /// The Interstate worker indicates whether this Referral is:
  /// A - Accepted
  /// D - Declined
  /// </summary>
  [JsonPropertyName("assnDeactInd")]
  [Member(Index = 57, Type = MemberType.Char, Length = AssnDeactInd_MaxLength, Optional
    = true)]
  public string AssnDeactInd
  {
    get => Get<string>("assnDeactInd");
    set => Set(
      "assnDeactInd", TrimEnd(Substring(value, 1, AssnDeactInd_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_DEFER_DT attribute.
  /// This is the last date on which an Interstate worker viewed this block and 
  /// decided to defer action on it.
  /// </summary>
  [JsonPropertyName("lastDeferDt")]
  [Member(Index = 58, Type = MemberType.Date, Optional = true)]
  public DateTime? LastDeferDt
  {
    get => Get<DateTime?>("lastDeferDt");
    set => Set("lastDeferDt", value);
  }

  /// <summary>Length of the MEMO attribute.</summary>
  public const int Memo_MaxLength = 80;

  /// <summary>
  /// The value of the MEMO attribute.
  /// Free-form field to accept Referral information.
  /// </summary>
  [JsonPropertyName("memo")]
  [Member(Index = 59, Type = MemberType.Varchar, Length = Memo_MaxLength, Optional
    = true)]
  public string Memo
  {
    get => Get<string>("memo");
    set => Set("memo", Substring(value, 1, Memo_MaxLength));
  }

  /// <summary>Length of the CONTACT_PHONE_EXTENSION attribute.</summary>
  public const int ContactPhoneExtension_MaxLength = 6;

  /// <summary>
  /// The value of the CONTACT_PHONE_EXTENSION attribute.
  /// The extension number associated to a person's telephone number.
  /// </summary>
  [JsonPropertyName("contactPhoneExtension")]
  [Member(Index = 60, Type = MemberType.Char, Length
    = ContactPhoneExtension_MaxLength, Optional = true)]
  public string ContactPhoneExtension
  {
    get => Get<string>("contactPhoneExtension");
    set => Set(
      "contactPhoneExtension", TrimEnd(Substring(value, 1,
      ContactPhoneExtension_MaxLength)));
  }

  /// <summary>
  /// The value of the CONTACT_FAX_NUMBER attribute.
  /// The telephone number where faxes can be sent to the contact person.
  /// </summary>
  [JsonPropertyName("contactFaxNumber")]
  [Member(Index = 61, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? ContactFaxNumber
  {
    get => Get<int?>("contactFaxNumber");
    set => Set("contactFaxNumber", value);
  }

  /// <summary>
  /// The value of the CONTACT_FAX_AREA_CODE attribute.
  /// The area code for the telephone number where faxes can be sent.
  /// </summary>
  [JsonPropertyName("contactFaxAreaCode")]
  [Member(Index = 62, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ContactFaxAreaCode
  {
    get => Get<int?>("contactFaxAreaCode");
    set => Set("contactFaxAreaCode", value);
  }

  /// <summary>Length of the CONTACT_INTERNET_ADDRESS attribute.</summary>
  public const int ContactInternetAddress_MaxLength = 35;

  /// <summary>
  /// The value of the CONTACT_INTERNET_ADDRESS attribute.
  /// The internet adress of the contact person.
  /// </summary>
  [JsonPropertyName("contactInternetAddress")]
  [Member(Index = 63, Type = MemberType.Char, Length
    = ContactInternetAddress_MaxLength, Optional = true)]
  public string ContactInternetAddress
  {
    get => Get<string>("contactInternetAddress");
    set => Set(
      "contactInternetAddress", TrimEnd(Substring(value, 1,
      ContactInternetAddress_MaxLength)));
  }

  /// <summary>Length of the INITIATING_DOCKET_NUMBER attribute.</summary>
  public const int InitiatingDocketNumber_MaxLength = 17;

  /// <summary>
  /// The value of the INITIATING_DOCKET_NUMBER attribute.
  /// The docket number from the initiating state.
  /// </summary>
  [JsonPropertyName("initiatingDocketNumber")]
  [Member(Index = 64, Type = MemberType.Char, Length
    = InitiatingDocketNumber_MaxLength, Optional = true)]
  public string InitiatingDocketNumber
  {
    get => Get<string>("initiatingDocketNumber");
    set => Set(
      "initiatingDocketNumber", TrimEnd(Substring(value, 1,
      InitiatingDocketNumber_MaxLength)));
  }

  /// <summary>Length of the SEND_PAYMENTS_BANK_ACCOUNT attribute.</summary>
  public const int SendPaymentsBankAccount_MaxLength = 20;

  /// <summary>
  /// The value of the SEND_PAYMENTS_BANK_ACCOUNT attribute.
  /// The bank account number where payments are to be sent.
  /// </summary>
  [JsonPropertyName("sendPaymentsBankAccount")]
  [Member(Index = 65, Type = MemberType.Char, Length
    = SendPaymentsBankAccount_MaxLength, Optional = true)]
  public string SendPaymentsBankAccount
  {
    get => Get<string>("sendPaymentsBankAccount");
    set => Set(
      "sendPaymentsBankAccount", TrimEnd(Substring(value, 1,
      SendPaymentsBankAccount_MaxLength)));
  }

  /// <summary>
  /// The value of the SEND_PAYMENTS_ROUTING_CODE attribute.
  /// The routing code where payments are to be sent.
  /// </summary>
  [JsonPropertyName("sendPaymentsRoutingCode")]
  [Member(Index = 66, Type = MemberType.Number, Length = 10, Optional = true)]
  public long? SendPaymentsRoutingCode
  {
    get => Get<long?>("sendPaymentsRoutingCode");
    set => Set("sendPaymentsRoutingCode", value);
  }

  /// <summary>Length of the NONDISCLOSURE_FINDING attribute.</summary>
  public const int NondisclosureFinding_MaxLength = 1;

  /// <summary>
  /// The value of the NONDISCLOSURE_FINDING attribute.
  /// An order from a tribunal indicating that the AR and children's  address 
  /// will not be provided on an interstate case.
  /// </summary>
  [JsonPropertyName("nondisclosureFinding")]
  [Member(Index = 67, Type = MemberType.Char, Length
    = NondisclosureFinding_MaxLength, Optional = true)]
  public string NondisclosureFinding
  {
    get => Get<string>("nondisclosureFinding");
    set => Set(
      "nondisclosureFinding", TrimEnd(Substring(value, 1,
      NondisclosureFinding_MaxLength)));
  }

  /// <summary>Length of the RESPONDING_DOCKET_NUMBER attribute.</summary>
  public const int RespondingDocketNumber_MaxLength = 17;

  /// <summary>
  /// The value of the RESPONDING_DOCKET_NUMBER attribute.
  /// The docket number from the responding state.
  /// </summary>
  [JsonPropertyName("respondingDocketNumber")]
  [Member(Index = 68, Type = MemberType.Char, Length
    = RespondingDocketNumber_MaxLength, Optional = true)]
  public string RespondingDocketNumber
  {
    get => Get<string>("respondingDocketNumber");
    set => Set(
      "respondingDocketNumber", TrimEnd(Substring(value, 1,
      RespondingDocketNumber_MaxLength)));
  }

  /// <summary>Length of the STATE_WITH_CEJ attribute.</summary>
  public const int StateWithCej_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_WITH_CEJ attribute.
  /// The state with Continuing Exclusive Jurisdiction (CEJ).
  /// </summary>
  [JsonPropertyName("stateWithCej")]
  [Member(Index = 69, Type = MemberType.Char, Length = StateWithCej_MaxLength, Optional
    = true)]
  public string StateWithCej
  {
    get => Get<string>("stateWithCej");
    set => Set(
      "stateWithCej", TrimEnd(Substring(value, 1, StateWithCej_MaxLength)));
  }

  /// <summary>Length of the PAYMENT_FIPS_COUNTY attribute.</summary>
  public const int PaymentFipsCounty_MaxLength = 3;

  /// <summary>
  /// The value of the PAYMENT_FIPS_COUNTY attribute.
  /// FIPS county used for the payment mailing address.
  /// </summary>
  [JsonPropertyName("paymentFipsCounty")]
  [Member(Index = 70, Type = MemberType.Char, Length
    = PaymentFipsCounty_MaxLength, Optional = true)]
  public string PaymentFipsCounty
  {
    get => Get<string>("paymentFipsCounty");
    set => Set(
      "paymentFipsCounty", TrimEnd(Substring(value, 1,
      PaymentFipsCounty_MaxLength)));
  }

  /// <summary>Length of the PAYMENT_FIPS_STATE attribute.</summary>
  public const int PaymentFipsState_MaxLength = 2;

  /// <summary>
  /// The value of the PAYMENT_FIPS_STATE attribute.
  /// FIPS state used for the payment mailing address.
  /// </summary>
  [JsonPropertyName("paymentFipsState")]
  [Member(Index = 71, Type = MemberType.Char, Length
    = PaymentFipsState_MaxLength, Optional = true)]
  public string PaymentFipsState
  {
    get => Get<string>("paymentFipsState");
    set => Set(
      "paymentFipsState",
      TrimEnd(Substring(value, 1, PaymentFipsState_MaxLength)));
  }

  /// <summary>Length of the PAYMENT_FIPS_LOCATION attribute.</summary>
  public const int PaymentFipsLocation_MaxLength = 2;

  /// <summary>
  /// The value of the PAYMENT_FIPS_LOCATION attribute.
  /// FIPS office used for the payment mailing address.
  /// </summary>
  [JsonPropertyName("paymentFipsLocation")]
  [Member(Index = 72, Type = MemberType.Char, Length
    = PaymentFipsLocation_MaxLength, Optional = true)]
  public string PaymentFipsLocation
  {
    get => Get<string>("paymentFipsLocation");
    set => Set(
      "paymentFipsLocation", TrimEnd(Substring(value, 1,
      PaymentFipsLocation_MaxLength)));
  }

  /// <summary>
  /// The value of the CONTACT_AREA_CODE attribute.
  /// The area code of the work number for this person.
  /// </summary>
  [JsonPropertyName("contactAreaCode")]
  [Member(Index = 73, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ContactAreaCode
  {
    get => Get<int?>("contactAreaCode");
    set => Set("contactAreaCode", value);
  }
}
