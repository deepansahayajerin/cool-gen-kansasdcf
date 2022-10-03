// The source file: FDSO_NTFY_FED_REF_TAPE_REC_1999, ID: 372538904, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class FdsoNtfyFedRefTapeRec1999: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FdsoNtfyFedRefTapeRec1999()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FdsoNtfyFedRefTapeRec1999(FdsoNtfyFedRefTapeRec1999 that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FdsoNtfyFedRefTapeRec1999 Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FdsoNtfyFedRefTapeRec1999 that)
  {
    base.Assign(that);
    submittingState = that.submittingState;
    localCode = that.localCode;
    ssn = that.ssn;
    caseNumber = that.caseNumber;
    lastName = that.lastName;
    firstName = that.firstName;
    refundAmount = that.refundAmount;
    transactionType = that.transactionType;
    caseType = that.caseType;
    transferState = that.transferState;
    transferLocalCode = that.transferLocalCode;
    processYear = that.processYear;
    addressLine1 = that.addressLine1;
    addressLine2 = that.addressLine2;
    city = that.city;
    stateCode = that.stateCode;
    zipCode9 = that.zipCode9;
    dateIssuedPreOffset = that.dateIssuedPreOffset;
    offsetExclusionIndicatorType = that.offsetExclusionIndicatorType;
    filler = that.filler;
  }

  /// <summary>Length of the SUBMITTING_STATE attribute.</summary>
  public const int SubmittingState_MaxLength = 2;

  /// <summary>
  /// The value of the SUBMITTING_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = SubmittingState_MaxLength)]
    
  public string SubmittingState
  {
    get => submittingState ?? "";
    set => submittingState =
      TrimEnd(Substring(value, 1, SubmittingState_MaxLength));
  }

  /// <summary>
  /// The json value of the SubmittingState attribute.</summary>
  [JsonPropertyName("submittingState")]
  [Computed]
  public string SubmittingState_Json
  {
    get => NullIf(SubmittingState, "");
    set => SubmittingState = value;
  }

  /// <summary>Length of the LOCAL_CODE attribute.</summary>
  public const int LocalCode_MaxLength = 3;

  /// <summary>
  /// The value of the LOCAL_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = LocalCode_MaxLength)]
  public string LocalCode
  {
    get => localCode ?? "";
    set => localCode = TrimEnd(Substring(value, 1, LocalCode_MaxLength));
  }

  /// <summary>
  /// The json value of the LocalCode attribute.</summary>
  [JsonPropertyName("localCode")]
  [Computed]
  public string LocalCode_Json
  {
    get => NullIf(LocalCode, "");
    set => LocalCode = value;
  }

  /// <summary>
  /// The value of the SSN attribute.
  /// </summary>
  [JsonPropertyName("ssn")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 9)]
  public int Ssn
  {
    get => ssn;
    set => ssn = value;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 15;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CaseNumber_MaxLength)]
  public string CaseNumber
  {
    get => caseNumber ?? "";
    set => caseNumber = TrimEnd(Substring(value, 1, CaseNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseNumber attribute.</summary>
  [JsonPropertyName("caseNumber")]
  [Computed]
  public string CaseNumber_Json
  {
    get => NullIf(CaseNumber, "");
    set => CaseNumber = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 20;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastName_MaxLength)]
  public string LastName
  {
    get => lastName ?? "";
    set => lastName = TrimEnd(Substring(value, 1, LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the LastName attribute.</summary>
  [JsonPropertyName("lastName")]
  [Computed]
  public string LastName_Json
  {
    get => NullIf(LastName, "");
    set => LastName = value;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 15;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = FirstName_MaxLength)]
  public string FirstName
  {
    get => firstName ?? "";
    set => firstName = TrimEnd(Substring(value, 1, FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the FirstName attribute.</summary>
  [JsonPropertyName("firstName")]
  [Computed]
  public string FirstName_Json
  {
    get => NullIf(FirstName, "");
    set => FirstName = value;
  }

  /// <summary>
  /// The value of the REFUND_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("refundAmount")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 8)]
  public int RefundAmount
  {
    get => refundAmount;
    set => refundAmount = value;
  }

  /// <summary>Length of the TRANSACTION_TYPE attribute.</summary>
  public const int TransactionType_MaxLength = 1;

  /// <summary>
  /// The value of the TRANSACTION_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = TransactionType_MaxLength)]
    
  public string TransactionType
  {
    get => transactionType ?? "";
    set => transactionType =
      TrimEnd(Substring(value, 1, TransactionType_MaxLength));
  }

  /// <summary>
  /// The json value of the TransactionType attribute.</summary>
  [JsonPropertyName("transactionType")]
  [Computed]
  public string TransactionType_Json
  {
    get => NullIf(TransactionType, "");
    set => TransactionType = value;
  }

  /// <summary>Length of the CASE_TYPE attribute.</summary>
  public const int CaseType_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CaseType_MaxLength)]
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

  /// <summary>Length of the TRANSFER_STATE attribute.</summary>
  public const int TransferState_MaxLength = 2;

  /// <summary>
  /// The value of the TRANSFER_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = TransferState_MaxLength)]
  public string TransferState
  {
    get => transferState ?? "";
    set => transferState =
      TrimEnd(Substring(value, 1, TransferState_MaxLength));
  }

  /// <summary>
  /// The json value of the TransferState attribute.</summary>
  [JsonPropertyName("transferState")]
  [Computed]
  public string TransferState_Json
  {
    get => NullIf(TransferState, "");
    set => TransferState = value;
  }

  /// <summary>Length of the TRANSFER_LOCAL_CODE attribute.</summary>
  public const int TransferLocalCode_MaxLength = 3;

  /// <summary>
  /// The value of the TRANSFER_LOCAL_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = TransferLocalCode_MaxLength)]
  public string TransferLocalCode
  {
    get => transferLocalCode ?? "";
    set => transferLocalCode =
      TrimEnd(Substring(value, 1, TransferLocalCode_MaxLength));
  }

  /// <summary>
  /// The json value of the TransferLocalCode attribute.</summary>
  [JsonPropertyName("transferLocalCode")]
  [Computed]
  public string TransferLocalCode_Json
  {
    get => NullIf(TransferLocalCode, "");
    set => TransferLocalCode = value;
  }

  /// <summary>Length of the PROCESS_YEAR attribute.</summary>
  public const int ProcessYear_MaxLength = 4;

  /// <summary>
  /// The value of the PROCESS_YEAR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = ProcessYear_MaxLength)]
  public string ProcessYear
  {
    get => processYear ?? "";
    set => processYear = TrimEnd(Substring(value, 1, ProcessYear_MaxLength));
  }

  /// <summary>
  /// The json value of the ProcessYear attribute.</summary>
  [JsonPropertyName("processYear")]
  [Computed]
  public string ProcessYear_Json
  {
    get => NullIf(ProcessYear, "");
    set => ProcessYear = value;
  }

  /// <summary>Length of the ADDRESS_LINE1 attribute.</summary>
  public const int AddressLine1_MaxLength = 30;

  /// <summary>
  /// The value of the ADDRESS_LINE1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = AddressLine1_MaxLength)]
  public string AddressLine1
  {
    get => addressLine1 ?? "";
    set => addressLine1 = TrimEnd(Substring(value, 1, AddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressLine1 attribute.</summary>
  [JsonPropertyName("addressLine1")]
  [Computed]
  public string AddressLine1_Json
  {
    get => NullIf(AddressLine1, "");
    set => AddressLine1 = value;
  }

  /// <summary>Length of the ADDRESS_LINE2 attribute.</summary>
  public const int AddressLine2_MaxLength = 30;

  /// <summary>
  /// The value of the ADDRESS_LINE2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = AddressLine2_MaxLength)]
  public string AddressLine2
  {
    get => addressLine2 ?? "";
    set => addressLine2 = TrimEnd(Substring(value, 1, AddressLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressLine2 attribute.</summary>
  [JsonPropertyName("addressLine2")]
  [Computed]
  public string AddressLine2_Json
  {
    get => NullIf(AddressLine2, "");
    set => AddressLine2 = value;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 25;

  /// <summary>
  /// The value of the CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = City_MaxLength)]
  public string City
  {
    get => city ?? "";
    set => city = TrimEnd(Substring(value, 1, City_MaxLength));
  }

  /// <summary>
  /// The json value of the City attribute.</summary>
  [JsonPropertyName("city")]
  [Computed]
  public string City_Json
  {
    get => NullIf(City, "");
    set => City = value;
  }

  /// <summary>Length of the STATE_CODE attribute.</summary>
  public const int StateCode_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = StateCode_MaxLength)]
  public string StateCode
  {
    get => stateCode ?? "";
    set => stateCode = TrimEnd(Substring(value, 1, StateCode_MaxLength));
  }

  /// <summary>
  /// The json value of the StateCode attribute.</summary>
  [JsonPropertyName("stateCode")]
  [Computed]
  public string StateCode_Json
  {
    get => NullIf(StateCode, "");
    set => StateCode = value;
  }

  /// <summary>Length of the ZIP_CODE_9 attribute.</summary>
  public const int ZipCode9_MaxLength = 9;

  /// <summary>
  /// The value of the ZIP_CODE_9 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = ZipCode9_MaxLength)]
  public string ZipCode9
  {
    get => zipCode9 ?? "";
    set => zipCode9 = TrimEnd(Substring(value, 1, ZipCode9_MaxLength));
  }

  /// <summary>
  /// The json value of the ZipCode9 attribute.</summary>
  [JsonPropertyName("zipCode9")]
  [Computed]
  public string ZipCode9_Json
  {
    get => NullIf(ZipCode9, "");
    set => ZipCode9 = value;
  }

  /// <summary>Length of the DATE_ISSUED_PRE_OFFSET attribute.</summary>
  public const int DateIssuedPreOffset_MaxLength = 8;

  /// <summary>
  /// The value of the DATE_ISSUED_PRE_OFFSET attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = DateIssuedPreOffset_MaxLength)]
  public string DateIssuedPreOffset
  {
    get => dateIssuedPreOffset ?? "";
    set => dateIssuedPreOffset =
      TrimEnd(Substring(value, 1, DateIssuedPreOffset_MaxLength));
  }

  /// <summary>
  /// The json value of the DateIssuedPreOffset attribute.</summary>
  [JsonPropertyName("dateIssuedPreOffset")]
  [Computed]
  public string DateIssuedPreOffset_Json
  {
    get => NullIf(DateIssuedPreOffset, "");
    set => DateIssuedPreOffset = value;
  }

  /// <summary>Length of the OFFSET_EXCLUSION_INDICATOR_TYPE attribute.
  /// </summary>
  public const int OffsetExclusionIndicatorType_MaxLength = 40;

  /// <summary>
  /// The value of the OFFSET_EXCLUSION_INDICATOR_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = OffsetExclusionIndicatorType_MaxLength)]
  public string OffsetExclusionIndicatorType
  {
    get => offsetExclusionIndicatorType ?? "";
    set => offsetExclusionIndicatorType =
      TrimEnd(Substring(value, 1, OffsetExclusionIndicatorType_MaxLength));
  }

  /// <summary>
  /// The json value of the OffsetExclusionIndicatorType attribute.</summary>
  [JsonPropertyName("offsetExclusionIndicatorType")]
  [Computed]
  public string OffsetExclusionIndicatorType_Json
  {
    get => NullIf(OffsetExclusionIndicatorType, "");
    set => OffsetExclusionIndicatorType = value;
  }

  /// <summary>Length of the FILLER attribute.</summary>
  public const int Filler_MaxLength = 18;

  /// <summary>
  /// The value of the FILLER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = Filler_MaxLength)]
  public string Filler
  {
    get => filler ?? "";
    set => filler = TrimEnd(Substring(value, 1, Filler_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler attribute.</summary>
  [JsonPropertyName("filler")]
  [Computed]
  public string Filler_Json
  {
    get => NullIf(Filler, "");
    set => Filler = value;
  }

  private string submittingState;
  private string localCode;
  private int ssn;
  private string caseNumber;
  private string lastName;
  private string firstName;
  private int refundAmount;
  private string transactionType;
  private string caseType;
  private string transferState;
  private string transferLocalCode;
  private string processYear;
  private string addressLine1;
  private string addressLine2;
  private string city;
  private string stateCode;
  private string zipCode9;
  private string dateIssuedPreOffset;
  private string offsetExclusionIndicatorType;
  private string filler;
}
