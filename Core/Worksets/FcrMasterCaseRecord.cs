// The source file: FCR_MASTER_CASE_RECORD, ID: 374577013, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class FcrMasterCaseRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrMasterCaseRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrMasterCaseRecord(FcrMasterCaseRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrMasterCaseRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrMasterCaseRecord that)
  {
    base.Assign(that);
    caseId = that.caseId;
    orderIndicator = that.orderIndicator;
    actionTypeCode = that.actionTypeCode;
    batchNumber = that.batchNumber;
    fipsCountyCode = that.fipsCountyCode;
    previousCaseId = that.previousCaseId;
    caseSentDateToFcr = that.caseSentDateToFcr;
    fcrCaseComments = that.fcrCaseComments;
    fcrCaseResponseDate = that.fcrCaseResponseDate;
    acknowlegementCode = that.acknowlegementCode;
    errorCode1 = that.errorCode1;
    errorCode2 = that.errorCode2;
    errorCode3 = that.errorCode3;
    errorCode4 = that.errorCode4;
    errorCode5 = that.errorCode5;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    recordIdentifier = that.recordIdentifier;
    caseUserField = that.caseUserField;
  }

  /// <summary>Length of the CASE_ID attribute.</summary>
  public const int CaseId_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_ID attribute.
  /// This field must contain a unique identifier assigned to the case by the KS
  /// CSE Application. It must not be all spaces, all zeroes, contain an
  /// asterisk or backslash and the first position must not be a space.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CaseId_MaxLength)]
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

  /// <summary>Length of the ORDER_INDICATOR attribute.</summary>
  public const int OrderIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the ORDER_INDICATOR attribute.
  /// This field must contain one of the following codes:
  /// Y - The State system has a record of the existence of a child support 
  /// order that is applicable to this case.
  /// N - The State system has no record of the existence of a child support 
  /// order that is applicable to this case.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = OrderIndicator_MaxLength)]
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

  /// <summary>Length of the ACTION_TYPE_CODE attribute.</summary>
  public const int ActionTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION_TYPE_CODE attribute.
  /// This field contains the information that was submitted by CSE Application.
  /// This fields will havThis field contains the information that was
  /// submitted by CSE Application.  This fields will have any of the below
  /// mentioned values:
  /// 'A' dd       - Add new records to FCR database
  /// 'C'hanage - Change the information to the existing record with FCR.
  /// 'D'elete     - Remove the existing Case/Person record from FCR databasee 
  /// any of the below mentioned values:
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = ActionTypeCode_MaxLength)]
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

  /// <summary>Length of the BATCH_NUMBER attribute.</summary>
  public const int BatchNumber_MaxLength = 6;

  /// <summary>
  /// The value of the BATCH_NUMBER attribute.
  /// This field contains the KS CSE application assigned number of the batch 
  /// that contained the input record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = BatchNumber_MaxLength)]
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

  /// <summary>Length of the FIPS_COUNTY_CODE attribute.</summary>
  public const int FipsCountyCode_MaxLength = 3;

  /// <summary>
  /// The value of the FIPS_COUNTY_CODE attribute.
  /// County Code submitted by KS CSE:
  /// KS CSE may use this field to specify the county office responsible for the
  /// case.
  /// If present, this field must be positions three through five of the numeric
  /// FIPS State/Territory and County Codes. Refer to the Department of
  /// Commerce FIPS Code Manual, National Institute of Standards and Technology,
  /// FIPS PUB 6-4 (April 1995) for a list of these codes. In addition, FIPS
  /// Codes may be found on the Internet at http://www.itl.nist.gov.
  /// The information included in this field will be stored on the FCR and 
  /// included in FCR Query and Proactive Match Response records.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = FipsCountyCode_MaxLength)]
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

  /// <summary>Length of the PREVIOUS_CASE_ID attribute.</summary>
  public const int PreviousCaseId_MaxLength = 10;

  /// <summary>
  /// The value of the PREVIOUS_CASE_ID attribute.
  /// This field may be used to change the Case ID for a case previously added 
  /// to the FCR.
  /// If present, this field must be different from the Case ID entered in this 
  /// record and it must not be all spaces, all zeroes, contain an asterisk or
  /// backslash and the first position must not be a space.
  /// This field must match to a case on the FCR.
  /// If a Change Transaction is submitted to change the Case ID, this field 
  /// must contain the Case ID used to add the case to the FCR.
  /// If the Change Transaction can be matched to the FCR, the information in 
  /// the Case ID field will be the State's new Case ID on the FCR for the case
  /// and related persons.
  /// All spaces in this field indicate that a change to the Case ID is not 
  /// being made.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = PreviousCaseId_MaxLength)]
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

  /// <summary>
  /// The value of the CASE_SENT_DATE_TO_FCR attribute.
  /// When this CASE Entry sent to FCR, while generating the FCR transaction we 
  /// will be creating a entry in this table, once we get the response we will
  /// be inserting a new row to this table and old row will be moved to Log
  /// table.
  /// </summary>
  [JsonPropertyName("caseSentDateToFcr")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? CaseSentDateToFcr
  {
    get => caseSentDateToFcr;
    set => caseSentDateToFcr = value;
  }

  /// <summary>Length of the FCR_CASE_COMMENTS attribute.</summary>
  public const int FcrCaseComments_MaxLength = 50;

  /// <summary>
  /// The value of the FCR_CASE_COMMENTS attribute.
  /// This will hold specifal information like, when the case is droped and 
  /// added due FCR limitation (i.e. due to business rule somtimes
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = FcrCaseComments_MaxLength)]
    
  public string FcrCaseComments
  {
    get => fcrCaseComments ?? "";
    set => fcrCaseComments =
      TrimEnd(Substring(value, 1, FcrCaseComments_MaxLength));
  }

  /// <summary>
  /// The json value of the FcrCaseComments attribute.</summary>
  [JsonPropertyName("fcrCaseComments")]
  [Computed]
  public string FcrCaseComments_Json
  {
    get => NullIf(FcrCaseComments, "");
    set => FcrCaseComments = value;
  }

  /// <summary>
  /// The value of the FCR_CASE_RESPONSE_DATE attribute.
  /// FCR response received date.
  /// </summary>
  [JsonPropertyName("fcrCaseResponseDate")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? FcrCaseResponseDate
  {
    get => fcrCaseResponseDate;
    set => fcrCaseResponseDate = value;
  }

  /// <summary>Length of the ACKNOWLEGEMENT_CODE attribute.</summary>
  public const int AcknowlegementCode_MaxLength = 5;

  /// <summary>
  /// The value of the ACKNOWLEGEMENT_CODE attribute.
  /// This field contains a code that indicates if the record was accepted, 
  /// rejected, or is pending.
  /// If the record was accepted, this field contains the code AAAAA.
  /// If the record is pending SSN identification on a related person record, 
  /// this field contains the code HOLDS.
  /// If the record was rejected, this field contains the code REJCT.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = AcknowlegementCode_MaxLength)]
  public string AcknowlegementCode
  {
    get => acknowlegementCode ?? "";
    set => acknowlegementCode =
      TrimEnd(Substring(value, 1, AcknowlegementCode_MaxLength));
  }

  /// <summary>
  /// The json value of the AcknowlegementCode attribute.</summary>
  [JsonPropertyName("acknowlegementCode")]
  [Computed]
  public string AcknowlegementCode_Json
  {
    get => NullIf(AcknowlegementCode, "");
    set => AcknowlegementCode = value;
  }

  /// <summary>Length of the ERROR_CODE_1 attribute.</summary>
  public const int ErrorCode1_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_1 attribute.
  /// Error Code Return by FCR Process:
  /// If the record was accepted, but a non-critical error was detected, this 
  /// field contains an alphanumeric warning code beginning with PW or TW.
  /// If the record was rejected, this field contains an alphanumeric error code
  /// beginning with PE or TE.
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

  /// <summary>Length of the ERROR_CODE_2 attribute.</summary>
  public const int ErrorCode2_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_2 attribute.
  /// Error Code Return by FCR Process:
  /// If the record was accepted, but a non-critical error was detected, this 
  /// field contains an alphanumeric warning code beginning with PW or TW.
  /// If the record was rejected, this field contains an alphanumeric error code
  /// beginning with PE or TE.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = ErrorCode2_MaxLength)]
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

  /// <summary>Length of the ERROR_CODE_3 attribute.</summary>
  public const int ErrorCode3_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_3 attribute.
  /// Error Code Return by FCR Process:
  /// If the record was accepted, but a non-critical error was detected, this 
  /// field contains an alphanumeric warning code beginning with PW or TW.
  /// If the record was rejected, this field contains an alphanumeric error code
  /// beginning with PE or TE.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = ErrorCode3_MaxLength)]
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

  /// <summary>Length of the ERROR_CODE_4 attribute.</summary>
  public const int ErrorCode4_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_4 attribute.
  /// Error Code Return by FCR Process:
  /// If the record was accepted, but a non-critical error was detected, this 
  /// field contains an alphanumeric warning code beginning with PW or TW.
  /// If the record was rejected, this field contains an alphanumeric error code
  /// beginning with PE or TE.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = ErrorCode4_MaxLength)]
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

  /// <summary>Length of the ERROR_CODE_5 attribute.</summary>
  public const int ErrorCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE_5 attribute.
  /// Error Code Return by FCR Process:
  /// If the record was accepted, but a non-critical error was detected, this 
  /// field contains an alphanumeric warning code beginning with PW or TW.
  /// If the record was rejected, this field contains an alphanumeric error code
  /// beginning with PE or TE.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = ErrorCode5_MaxLength)]
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

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 17, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the RECORD_IDENTIFIER attribute.</summary>
  public const int RecordIdentifier_MaxLength = 2;

  /// <summary>
  /// The value of the RECORD_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = RecordIdentifier_MaxLength)]
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

  /// <summary>Length of the CASE_USER_FIELD attribute.</summary>
  public const int CaseUserField_MaxLength = 15;

  /// <summary>
  /// The value of the CASE_USER_FIELD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = CaseUserField_MaxLength)]
  public string CaseUserField
  {
    get => caseUserField ?? "";
    set => caseUserField =
      TrimEnd(Substring(value, 1, CaseUserField_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseUserField attribute.</summary>
  [JsonPropertyName("caseUserField")]
  [Computed]
  public string CaseUserField_Json
  {
    get => NullIf(CaseUserField, "");
    set => CaseUserField = value;
  }

  private string caseId;
  private string orderIndicator;
  private string actionTypeCode;
  private string batchNumber;
  private string fipsCountyCode;
  private string previousCaseId;
  private DateTime? caseSentDateToFcr;
  private string fcrCaseComments;
  private DateTime? fcrCaseResponseDate;
  private string acknowlegementCode;
  private string errorCode1;
  private string errorCode2;
  private string errorCode3;
  private string errorCode4;
  private string errorCode5;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string recordIdentifier;
  private string caseUserField;
}
