// The source file: FDSO_CERTIFICATION_TAPE_RECORD, ID: 372668443, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: LGLENFAC
/// This work set contains attributes for FDSO certification tape.
/// </summary>
[Serializable]
public partial class FdsoCertificationTapeRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FdsoCertificationTapeRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FdsoCertificationTapeRecord(FdsoCertificationTapeRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FdsoCertificationTapeRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FdsoCertificationTapeRecord that)
  {
    base.Assign(that);
    submittingState = that.submittingState;
    localCode = that.localCode;
    ssn = that.ssn;
    caseNumber = that.caseNumber;
    lastName = that.lastName;
    firstName = that.firstName;
    amountOwed = that.amountOwed;
    adcAmount = that.adcAmount;
    nonAdcAmount = that.nonAdcAmount;
    transactionType = that.transactionType;
    caseTypeInd = that.caseTypeInd;
    transferState = that.transferState;
    localForTransfer = that.localForTransfer;
    processYear = that.processYear;
    addressLine1 = that.addressLine1;
    addressLine2 = that.addressLine2;
    city = that.city;
    stateCode = that.stateCode;
    zipCode = that.zipCode;
    offsetExclusionType = that.offsetExclusionType;
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

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Ssn_MaxLength)]
  public string Ssn
  {
    get => ssn ?? "";
    set => ssn = TrimEnd(Substring(value, 1, Ssn_MaxLength));
  }

  /// <summary>
  /// The json value of the Ssn attribute.</summary>
  [JsonPropertyName("ssn")]
  [Computed]
  public string Ssn_Json
  {
    get => NullIf(Ssn, "");
    set => Ssn = value;
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

  /// <summary>Length of the AMOUNT_OWED attribute.</summary>
  public const int AmountOwed_MaxLength = 8;

  /// <summary>
  /// The value of the AMOUNT_OWED attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = AmountOwed_MaxLength)]
  public string AmountOwed
  {
    get => amountOwed ?? "";
    set => amountOwed = TrimEnd(Substring(value, 1, AmountOwed_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountOwed attribute.</summary>
  [JsonPropertyName("amountOwed")]
  [Computed]
  public string AmountOwed_Json
  {
    get => NullIf(AmountOwed, "");
    set => AmountOwed = value;
  }

  /// <summary>
  /// The value of the ADC_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("adcAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal AdcAmount
  {
    get => adcAmount;
    set => adcAmount = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NON_ADC_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("nonAdcAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal NonAdcAmount
  {
    get => nonAdcAmount;
    set => nonAdcAmount = Truncate(value, 2);
  }

  /// <summary>Length of the TRANSACTION_TYPE attribute.</summary>
  public const int TransactionType_MaxLength = 1;

  /// <summary>
  /// The value of the TRANSACTION_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = TransactionType_MaxLength)
    ]
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
  [Member(Index = 11, Type = MemberType.Char, Length = CaseTypeInd_MaxLength)]
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
  [Member(Index = 12, Type = MemberType.Char, Length = TransferState_MaxLength)]
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
  [Member(Index = 13, Type = MemberType.Number, Length = 3)]
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
  [Member(Index = 14, Type = MemberType.Char, Length = ProcessYear_MaxLength)]
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
  [Member(Index = 15, Type = MemberType.Char, Length = AddressLine1_MaxLength)]
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
  [Member(Index = 16, Type = MemberType.Char, Length = AddressLine2_MaxLength)]
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
  [Member(Index = 17, Type = MemberType.Char, Length = City_MaxLength)]
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
  [Member(Index = 18, Type = MemberType.Char, Length = StateCode_MaxLength)]
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

  /// <summary>Length of the ZIP_CODE attribute.</summary>
  public const int ZipCode_MaxLength = 9;

  /// <summary>
  /// The value of the ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = ZipCode_MaxLength)]
  public string ZipCode
  {
    get => zipCode ?? "";
    set => zipCode = TrimEnd(Substring(value, 1, ZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ZipCode attribute.</summary>
  [JsonPropertyName("zipCode")]
  [Computed]
  public string ZipCode_Json
  {
    get => NullIf(ZipCode, "");
    set => ZipCode = value;
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
  [Member(Index = 20, Type = MemberType.Char, Length
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

  private string submittingState;
  private string localCode;
  private string ssn;
  private string caseNumber;
  private string lastName;
  private string firstName;
  private string amountOwed;
  private decimal adcAmount;
  private decimal nonAdcAmount;
  private string transactionType;
  private string caseTypeInd;
  private string transferState;
  private int localForTransfer;
  private string processYear;
  private string addressLine1;
  private string addressLine2;
  private string city;
  private string stateCode;
  private string zipCode;
  private string offsetExclusionType;
}
