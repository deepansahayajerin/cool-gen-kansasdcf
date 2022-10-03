// The source file: FDSO_RETURN_H, ID: 372668476, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// FDSO 1999 submission edit validation and update edit validation 
/// specification. Used to receive reject cases from OCSE.
/// Populated by external action block. EXHIBIT H
/// </summary>
[Serializable]
public partial class FdsoReturnH: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FdsoReturnH()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FdsoReturnH(FdsoReturnH that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FdsoReturnH Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FdsoReturnH that)
  {
    base.Assign(that);
    submittingState = that.submittingState;
    localCode = that.localCode;
    ssn = that.ssn;
    caseNumber = that.caseNumber;
    lastName = that.lastName;
    firstName = that.firstName;
    amountOwed = that.amountOwed;
    transactionType = that.transactionType;
    caseTypeInd = that.caseTypeInd;
    transferState = that.transferState;
    localForTransfer = that.localForTransfer;
    processYear = that.processYear;
    offsetExclusionType = that.offsetExclusionType;
    errcode1 = that.errcode1;
    errcode2 = that.errcode2;
    errcode3 = that.errcode3;
    errcode4 = that.errcode4;
    errcode5 = that.errcode5;
    errcode6 = that.errcode6;
    fedReturnedLastName = that.fedReturnedLastName;
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
  /// The value of the AMOUNT_OWED attribute.
  /// </summary>
  [JsonPropertyName("amountOwed")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 8)]
  public int AmountOwed
  {
    get => amountOwed;
    set => amountOwed = value;
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

  /// <summary>Length of the CASE_TYPE_IND attribute.</summary>
  public const int CaseTypeInd_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_TYPE_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CaseTypeInd_MaxLength)]
  public string CaseTypeInd
  {
    get => caseTypeInd ?? "";
    set => caseTypeInd = TrimEnd(Substring(value, 1, CaseTypeInd_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseTypeInd attribute.</summary>
  [JsonPropertyName("caseTypeInd")]
  [Computed]
  public string CaseTypeInd_Json
  {
    get => NullIf(CaseTypeInd, "");
    set => CaseTypeInd = value;
  }

  /// <summary>Length of the TRANSFER_STATE attribute.</summary>
  public const int TransferState_MaxLength = 2;

  /// <summary>
  /// The value of the TRANSFER_STATE attribute.
  /// Must be constant thru out file.
  /// Required when state submits transfer.
  /// Required when transfer state updates a case.
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

  /// <summary>
  /// The value of the LOCAL_FOR_TRANSFER attribute.
  /// Local Code for Transfer
  /// When used it must be three digit numeric local code. FIPS code is 
  /// suggested.
  /// </summary>
  [JsonPropertyName("localForTransfer")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int LocalForTransfer
  {
    get => localForTransfer;
    set => localForTransfer = value;
  }

  /// <summary>Length of the PROCESS_YEAR attribute.</summary>
  public const int ProcessYear_MaxLength = 4;

  /// <summary>
  /// The value of the PROCESS_YEAR attribute.
  /// Process Year
  /// Year tax refund / administrative payment was offset. This is only required
  /// for reporting state payment.
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

  /// <summary>Length of the OFFSET_EXCLUSION_TYPE attribute.</summary>
  public const int OffsetExclusionType_MaxLength = 40;

  /// <summary>
  /// The value of the OFFSET_EXCLUSION_TYPE attribute.
  /// Valid only for new cases.
  /// If tax, amount_owed => 25
  /// Separate each exclusion with a comma.
  /// e.g. ADM,RET,TAX
  /// ADM - exclude all administrative offset
  /// RET - exclude federal retirement
  /// SAL - exclude federal salary
  /// TAX - exclude tax refund offset
  /// VEN - exclude vendor payment/misc
  /// PAS - exclude passport denial
  /// FIN - exclude financial institution
  /// blanks - no exclusion
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = OffsetExclusionType_MaxLength)]
  public string OffsetExclusionType
  {
    get => offsetExclusionType ?? "";
    set => offsetExclusionType =
      TrimEnd(Substring(value, 1, OffsetExclusionType_MaxLength));
  }

  /// <summary>
  /// The json value of the OffsetExclusionType attribute.</summary>
  [JsonPropertyName("offsetExclusionType")]
  [Computed]
  public string OffsetExclusionType_Json
  {
    get => NullIf(OffsetExclusionType, "");
    set => OffsetExclusionType = value;
  }

  /// <summary>Length of the ERRCODE1 attribute.</summary>
  public const int Errcode1_MaxLength = 2;

  /// <summary>
  /// The value of the ERRCODE1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = Errcode1_MaxLength)]
  public string Errcode1
  {
    get => errcode1 ?? "";
    set => errcode1 = TrimEnd(Substring(value, 1, Errcode1_MaxLength));
  }

  /// <summary>
  /// The json value of the Errcode1 attribute.</summary>
  [JsonPropertyName("errcode1")]
  [Computed]
  public string Errcode1_Json
  {
    get => NullIf(Errcode1, "");
    set => Errcode1 = value;
  }

  /// <summary>Length of the ERRCODE2 attribute.</summary>
  public const int Errcode2_MaxLength = 2;

  /// <summary>
  /// The value of the ERRCODE2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = Errcode2_MaxLength)]
  public string Errcode2
  {
    get => errcode2 ?? "";
    set => errcode2 = TrimEnd(Substring(value, 1, Errcode2_MaxLength));
  }

  /// <summary>
  /// The json value of the Errcode2 attribute.</summary>
  [JsonPropertyName("errcode2")]
  [Computed]
  public string Errcode2_Json
  {
    get => NullIf(Errcode2, "");
    set => Errcode2 = value;
  }

  /// <summary>Length of the ERRCODE3 attribute.</summary>
  public const int Errcode3_MaxLength = 2;

  /// <summary>
  /// The value of the ERRCODE3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = Errcode3_MaxLength)]
  public string Errcode3
  {
    get => errcode3 ?? "";
    set => errcode3 = TrimEnd(Substring(value, 1, Errcode3_MaxLength));
  }

  /// <summary>
  /// The json value of the Errcode3 attribute.</summary>
  [JsonPropertyName("errcode3")]
  [Computed]
  public string Errcode3_Json
  {
    get => NullIf(Errcode3, "");
    set => Errcode3 = value;
  }

  /// <summary>Length of the ERRCODE4 attribute.</summary>
  public const int Errcode4_MaxLength = 2;

  /// <summary>
  /// The value of the ERRCODE4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = Errcode4_MaxLength)]
  public string Errcode4
  {
    get => errcode4 ?? "";
    set => errcode4 = TrimEnd(Substring(value, 1, Errcode4_MaxLength));
  }

  /// <summary>
  /// The json value of the Errcode4 attribute.</summary>
  [JsonPropertyName("errcode4")]
  [Computed]
  public string Errcode4_Json
  {
    get => NullIf(Errcode4, "");
    set => Errcode4 = value;
  }

  /// <summary>Length of the ERRCODE5 attribute.</summary>
  public const int Errcode5_MaxLength = 2;

  /// <summary>
  /// The value of the ERRCODE5 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = Errcode5_MaxLength)]
  public string Errcode5
  {
    get => errcode5 ?? "";
    set => errcode5 = TrimEnd(Substring(value, 1, Errcode5_MaxLength));
  }

  /// <summary>
  /// The json value of the Errcode5 attribute.</summary>
  [JsonPropertyName("errcode5")]
  [Computed]
  public string Errcode5_Json
  {
    get => NullIf(Errcode5, "");
    set => Errcode5 = value;
  }

  /// <summary>Length of the ERRCODE6 attribute.</summary>
  public const int Errcode6_MaxLength = 2;

  /// <summary>
  /// The value of the ERRCODE6 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = Errcode6_MaxLength)]
  public string Errcode6
  {
    get => errcode6 ?? "";
    set => errcode6 = TrimEnd(Substring(value, 1, Errcode6_MaxLength));
  }

  /// <summary>
  /// The json value of the Errcode6 attribute.</summary>
  [JsonPropertyName("errcode6")]
  [Computed]
  public string Errcode6_Json
  {
    get => NullIf(Errcode6, "");
    set => Errcode6 = value;
  }

  /// <summary>Length of the FED_RETURNED_LAST_NAME attribute.</summary>
  public const int FedReturnedLastName_MaxLength = 4;

  /// <summary>
  /// The value of the FED_RETURNED_LAST_NAME attribute.
  /// This is a correct last Name(first 4 bytes) returned by Fed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = FedReturnedLastName_MaxLength)]
  public string FedReturnedLastName
  {
    get => fedReturnedLastName ?? "";
    set => fedReturnedLastName =
      TrimEnd(Substring(value, 1, FedReturnedLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the FedReturnedLastName attribute.</summary>
  [JsonPropertyName("fedReturnedLastName")]
  [Computed]
  public string FedReturnedLastName_Json
  {
    get => NullIf(FedReturnedLastName, "");
    set => FedReturnedLastName = value;
  }

  private string submittingState;
  private string localCode;
  private int ssn;
  private string caseNumber;
  private string lastName;
  private string firstName;
  private int amountOwed;
  private string transactionType;
  private string caseTypeInd;
  private string transferState;
  private int localForTransfer;
  private string processYear;
  private string offsetExclusionType;
  private string errcode1;
  private string errcode2;
  private string errcode3;
  private string errcode4;
  private string errcode5;
  private string errcode6;
  private string fedReturnedLastName;
}
