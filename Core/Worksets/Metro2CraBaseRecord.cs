// The source file: METRO2_CRA_BASE_RECORD, ID: 1902630921, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class Metro2CraBaseRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Metro2CraBaseRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Metro2CraBaseRecord(Metro2CraBaseRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Metro2CraBaseRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Metro2CraBaseRecord that)
  {
    base.Assign(that);
    recordDescriptorWord = that.recordDescriptorWord;
    processingIndicator = that.processingIndicator;
    timestamp = that.timestamp;
    reserved4 = that.reserved4;
    identificationNumber = that.identificationNumber;
    cycleIdentifier = that.cycleIdentifier;
    consumerAccountNumber = that.consumerAccountNumber;
    portfolioType = that.portfolioType;
    accountType = that.accountType;
    dateOpened = that.dateOpened;
    creditLimit = that.creditLimit;
    highestCreditAmount = that.highestCreditAmount;
    termsDuration = that.termsDuration;
    termsFrequency = that.termsFrequency;
    scheduledMonthlyPaymentAmount = that.scheduledMonthlyPaymentAmount;
    actualPaymentAmount = that.actualPaymentAmount;
    accountStatus = that.accountStatus;
    paymentRating = that.paymentRating;
    paymentHistoryProfile = that.paymentHistoryProfile;
    specialComment = that.specialComment;
    complianceConditionCode = that.complianceConditionCode;
    currentBalance = that.currentBalance;
    amountPastDue = that.amountPastDue;
    originalChargeOffAmount = that.originalChargeOffAmount;
    dateOfAccountInformation = that.dateOfAccountInformation;
    dateOfFirstDelinquency = that.dateOfFirstDelinquency;
    dateClosed = that.dateClosed;
    dateOfLastPayment = that.dateOfLastPayment;
    interestTypeIndicator = that.interestTypeIndicator;
    reserved29 = that.reserved29;
    surname = that.surname;
    firstName = that.firstName;
    middleName = that.middleName;
    generationCode = that.generationCode;
    socialSecurityNumber = that.socialSecurityNumber;
    dateOfBirth = that.dateOfBirth;
    telephoneNumber = that.telephoneNumber;
    ecoaCode = that.ecoaCode;
    consumerInformationIndicator = that.consumerInformationIndicator;
    countryCode = that.countryCode;
    firstLineOfAddress = that.firstLineOfAddress;
    secondLineOfAddress = that.secondLineOfAddress;
    city = that.city;
    state = that.state;
    postalZipCode = that.postalZipCode;
    addressIndicator = that.addressIndicator;
    residenceCode = that.residenceCode;
  }

  /// <summary>Length of the RECORD_DESCRIPTOR_WORD attribute.</summary>
  public const int RecordDescriptorWord_MaxLength = 4;

  /// <summary>
  /// The value of the RECORD_DESCRIPTOR_WORD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = RecordDescriptorWord_MaxLength)]
  public string RecordDescriptorWord
  {
    get => recordDescriptorWord ?? "";
    set => recordDescriptorWord =
      TrimEnd(Substring(value, 1, RecordDescriptorWord_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordDescriptorWord attribute.</summary>
  [JsonPropertyName("recordDescriptorWord")]
  [Computed]
  public string RecordDescriptorWord_Json
  {
    get => NullIf(RecordDescriptorWord, "");
    set => RecordDescriptorWord = value;
  }

  /// <summary>Length of the PROCESSING_INDICATOR attribute.</summary>
  public const int ProcessingIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the PROCESSING_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = ProcessingIndicator_MaxLength)]
  public string ProcessingIndicator
  {
    get => processingIndicator ?? "";
    set => processingIndicator =
      TrimEnd(Substring(value, 1, ProcessingIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the ProcessingIndicator attribute.</summary>
  [JsonPropertyName("processingIndicator")]
  [Computed]
  public string ProcessingIndicator_Json
  {
    get => NullIf(ProcessingIndicator, "");
    set => ProcessingIndicator = value;
  }

  /// <summary>Length of the TIMESTAMP attribute.</summary>
  public const int Timestamp_MaxLength = 14;

  /// <summary>
  /// The value of the TIMESTAMP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Timestamp_MaxLength)]
  public string Timestamp
  {
    get => timestamp ?? "";
    set => timestamp = TrimEnd(Substring(value, 1, Timestamp_MaxLength));
  }

  /// <summary>
  /// The json value of the Timestamp attribute.</summary>
  [JsonPropertyName("timestamp")]
  [Computed]
  public string Timestamp_Json
  {
    get => NullIf(Timestamp, "");
    set => Timestamp = value;
  }

  /// <summary>Length of the RESERVED_4 attribute.</summary>
  public const int Reserved4_MaxLength = 1;

  /// <summary>
  /// The value of the RESERVED_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Reserved4_MaxLength)]
  public string Reserved4
  {
    get => reserved4 ?? "";
    set => reserved4 = TrimEnd(Substring(value, 1, Reserved4_MaxLength));
  }

  /// <summary>
  /// The json value of the Reserved4 attribute.</summary>
  [JsonPropertyName("reserved4")]
  [Computed]
  public string Reserved4_Json
  {
    get => NullIf(Reserved4, "");
    set => Reserved4 = value;
  }

  /// <summary>Length of the IDENTIFICATION_NUMBER attribute.</summary>
  public const int IdentificationNumber_MaxLength = 20;

  /// <summary>
  /// The value of the IDENTIFICATION_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = IdentificationNumber_MaxLength)]
  public string IdentificationNumber
  {
    get => identificationNumber ?? "";
    set => identificationNumber =
      TrimEnd(Substring(value, 1, IdentificationNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the IdentificationNumber attribute.</summary>
  [JsonPropertyName("identificationNumber")]
  [Computed]
  public string IdentificationNumber_Json
  {
    get => NullIf(IdentificationNumber, "");
    set => IdentificationNumber = value;
  }

  /// <summary>Length of the CYCLE_IDENTIFIER attribute.</summary>
  public const int CycleIdentifier_MaxLength = 2;

  /// <summary>
  /// The value of the CYCLE_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CycleIdentifier_MaxLength)]
    
  public string CycleIdentifier
  {
    get => cycleIdentifier ?? "";
    set => cycleIdentifier =
      TrimEnd(Substring(value, 1, CycleIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the CycleIdentifier attribute.</summary>
  [JsonPropertyName("cycleIdentifier")]
  [Computed]
  public string CycleIdentifier_Json
  {
    get => NullIf(CycleIdentifier, "");
    set => CycleIdentifier = value;
  }

  /// <summary>Length of the CONSUMER_ACCOUNT_NUMBER attribute.</summary>
  public const int ConsumerAccountNumber_MaxLength = 30;

  /// <summary>
  /// The value of the CONSUMER_ACCOUNT_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = ConsumerAccountNumber_MaxLength)]
  public string ConsumerAccountNumber
  {
    get => consumerAccountNumber ?? "";
    set => consumerAccountNumber =
      TrimEnd(Substring(value, 1, ConsumerAccountNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the ConsumerAccountNumber attribute.</summary>
  [JsonPropertyName("consumerAccountNumber")]
  [Computed]
  public string ConsumerAccountNumber_Json
  {
    get => NullIf(ConsumerAccountNumber, "");
    set => ConsumerAccountNumber = value;
  }

  /// <summary>Length of the PORTFOLIO_TYPE attribute.</summary>
  public const int PortfolioType_MaxLength = 1;

  /// <summary>
  /// The value of the PORTFOLIO_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = PortfolioType_MaxLength)]
  public string PortfolioType
  {
    get => portfolioType ?? "";
    set => portfolioType =
      TrimEnd(Substring(value, 1, PortfolioType_MaxLength));
  }

  /// <summary>
  /// The json value of the PortfolioType attribute.</summary>
  [JsonPropertyName("portfolioType")]
  [Computed]
  public string PortfolioType_Json
  {
    get => NullIf(PortfolioType, "");
    set => PortfolioType = value;
  }

  /// <summary>Length of the ACCOUNT_TYPE attribute.</summary>
  public const int AccountType_MaxLength = 2;

  /// <summary>
  /// The value of the ACCOUNT_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = AccountType_MaxLength)]
  public string AccountType
  {
    get => accountType ?? "";
    set => accountType = TrimEnd(Substring(value, 1, AccountType_MaxLength));
  }

  /// <summary>
  /// The json value of the AccountType attribute.</summary>
  [JsonPropertyName("accountType")]
  [Computed]
  public string AccountType_Json
  {
    get => NullIf(AccountType, "");
    set => AccountType = value;
  }

  /// <summary>Length of the DATE_OPENED attribute.</summary>
  public const int DateOpened_MaxLength = 8;

  /// <summary>
  /// The value of the DATE_OPENED attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = DateOpened_MaxLength)]
  public string DateOpened
  {
    get => dateOpened ?? "";
    set => dateOpened = TrimEnd(Substring(value, 1, DateOpened_MaxLength));
  }

  /// <summary>
  /// The json value of the DateOpened attribute.</summary>
  [JsonPropertyName("dateOpened")]
  [Computed]
  public string DateOpened_Json
  {
    get => NullIf(DateOpened, "");
    set => DateOpened = value;
  }

  /// <summary>Length of the CREDIT_LIMIT attribute.</summary>
  public const int CreditLimit_MaxLength = 9;

  /// <summary>
  /// The value of the CREDIT_LIMIT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CreditLimit_MaxLength)]
  public string CreditLimit
  {
    get => creditLimit ?? "";
    set => creditLimit = TrimEnd(Substring(value, 1, CreditLimit_MaxLength));
  }

  /// <summary>
  /// The json value of the CreditLimit attribute.</summary>
  [JsonPropertyName("creditLimit")]
  [Computed]
  public string CreditLimit_Json
  {
    get => NullIf(CreditLimit, "");
    set => CreditLimit = value;
  }

  /// <summary>Length of the HIGHEST_CREDIT_AMOUNT attribute.</summary>
  public const int HighestCreditAmount_MaxLength = 9;

  /// <summary>
  /// The value of the HIGHEST_CREDIT_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = HighestCreditAmount_MaxLength)]
  public string HighestCreditAmount
  {
    get => highestCreditAmount ?? "";
    set => highestCreditAmount =
      TrimEnd(Substring(value, 1, HighestCreditAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the HighestCreditAmount attribute.</summary>
  [JsonPropertyName("highestCreditAmount")]
  [Computed]
  public string HighestCreditAmount_Json
  {
    get => NullIf(HighestCreditAmount, "");
    set => HighestCreditAmount = value;
  }

  /// <summary>Length of the TERMS_DURATION attribute.</summary>
  public const int TermsDuration_MaxLength = 3;

  /// <summary>
  /// The value of the TERMS_DURATION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = TermsDuration_MaxLength)]
  public string TermsDuration
  {
    get => termsDuration ?? "";
    set => termsDuration =
      TrimEnd(Substring(value, 1, TermsDuration_MaxLength));
  }

  /// <summary>
  /// The json value of the TermsDuration attribute.</summary>
  [JsonPropertyName("termsDuration")]
  [Computed]
  public string TermsDuration_Json
  {
    get => NullIf(TermsDuration, "");
    set => TermsDuration = value;
  }

  /// <summary>Length of the TERMS_FREQUENCY attribute.</summary>
  public const int TermsFrequency_MaxLength = 1;

  /// <summary>
  /// The value of the TERMS_FREQUENCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = TermsFrequency_MaxLength)]
    
  public string TermsFrequency
  {
    get => termsFrequency ?? "";
    set => termsFrequency =
      TrimEnd(Substring(value, 1, TermsFrequency_MaxLength));
  }

  /// <summary>
  /// The json value of the TermsFrequency attribute.</summary>
  [JsonPropertyName("termsFrequency")]
  [Computed]
  public string TermsFrequency_Json
  {
    get => NullIf(TermsFrequency, "");
    set => TermsFrequency = value;
  }

  /// <summary>Length of the SCHEDULED_MONTHLY_PAYMENT_AMOUNT attribute.
  /// </summary>
  public const int ScheduledMonthlyPaymentAmount_MaxLength = 9;

  /// <summary>
  /// The value of the SCHEDULED_MONTHLY_PAYMENT_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = ScheduledMonthlyPaymentAmount_MaxLength)]
  public string ScheduledMonthlyPaymentAmount
  {
    get => scheduledMonthlyPaymentAmount ?? "";
    set => scheduledMonthlyPaymentAmount =
      TrimEnd(Substring(value, 1, ScheduledMonthlyPaymentAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the ScheduledMonthlyPaymentAmount attribute.</summary>
  [JsonPropertyName("scheduledMonthlyPaymentAmount")]
  [Computed]
  public string ScheduledMonthlyPaymentAmount_Json
  {
    get => NullIf(ScheduledMonthlyPaymentAmount, "");
    set => ScheduledMonthlyPaymentAmount = value;
  }

  /// <summary>Length of the ACTUAL_PAYMENT_AMOUNT attribute.</summary>
  public const int ActualPaymentAmount_MaxLength = 9;

  /// <summary>
  /// The value of the ACTUAL_PAYMENT_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = ActualPaymentAmount_MaxLength)]
  public string ActualPaymentAmount
  {
    get => actualPaymentAmount ?? "";
    set => actualPaymentAmount =
      TrimEnd(Substring(value, 1, ActualPaymentAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the ActualPaymentAmount attribute.</summary>
  [JsonPropertyName("actualPaymentAmount")]
  [Computed]
  public string ActualPaymentAmount_Json
  {
    get => NullIf(ActualPaymentAmount, "");
    set => ActualPaymentAmount = value;
  }

  /// <summary>Length of the ACCOUNT_STATUS attribute.</summary>
  public const int AccountStatus_MaxLength = 2;

  /// <summary>
  /// The value of the ACCOUNT_STATUS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = AccountStatus_MaxLength)]
  public string AccountStatus
  {
    get => accountStatus ?? "";
    set => accountStatus =
      TrimEnd(Substring(value, 1, AccountStatus_MaxLength));
  }

  /// <summary>
  /// The json value of the AccountStatus attribute.</summary>
  [JsonPropertyName("accountStatus")]
  [Computed]
  public string AccountStatus_Json
  {
    get => NullIf(AccountStatus, "");
    set => AccountStatus = value;
  }

  /// <summary>Length of the PAYMENT_RATING attribute.</summary>
  public const int PaymentRating_MaxLength = 1;

  /// <summary>
  /// The value of the PAYMENT_RATING attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = PaymentRating_MaxLength)]
  public string PaymentRating
  {
    get => paymentRating ?? "";
    set => paymentRating =
      TrimEnd(Substring(value, 1, PaymentRating_MaxLength));
  }

  /// <summary>
  /// The json value of the PaymentRating attribute.</summary>
  [JsonPropertyName("paymentRating")]
  [Computed]
  public string PaymentRating_Json
  {
    get => NullIf(PaymentRating, "");
    set => PaymentRating = value;
  }

  /// <summary>Length of the PAYMENT_HISTORY_PROFILE attribute.</summary>
  public const int PaymentHistoryProfile_MaxLength = 24;

  /// <summary>
  /// The value of the PAYMENT_HISTORY_PROFILE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = PaymentHistoryProfile_MaxLength)]
  public string PaymentHistoryProfile
  {
    get => paymentHistoryProfile ?? "";
    set => paymentHistoryProfile =
      TrimEnd(Substring(value, 1, PaymentHistoryProfile_MaxLength));
  }

  /// <summary>
  /// The json value of the PaymentHistoryProfile attribute.</summary>
  [JsonPropertyName("paymentHistoryProfile")]
  [Computed]
  public string PaymentHistoryProfile_Json
  {
    get => NullIf(PaymentHistoryProfile, "");
    set => PaymentHistoryProfile = value;
  }

  /// <summary>Length of the SPECIAL_COMMENT attribute.</summary>
  public const int SpecialComment_MaxLength = 2;

  /// <summary>
  /// The value of the SPECIAL_COMMENT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = SpecialComment_MaxLength)]
    
  public string SpecialComment
  {
    get => specialComment ?? "";
    set => specialComment =
      TrimEnd(Substring(value, 1, SpecialComment_MaxLength));
  }

  /// <summary>
  /// The json value of the SpecialComment attribute.</summary>
  [JsonPropertyName("specialComment")]
  [Computed]
  public string SpecialComment_Json
  {
    get => NullIf(SpecialComment, "");
    set => SpecialComment = value;
  }

  /// <summary>Length of the COMPLIANCE_CONDITION_CODE attribute.</summary>
  public const int ComplianceConditionCode_MaxLength = 2;

  /// <summary>
  /// The value of the COMPLIANCE_CONDITION_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = ComplianceConditionCode_MaxLength)]
  public string ComplianceConditionCode
  {
    get => complianceConditionCode ?? "";
    set => complianceConditionCode =
      TrimEnd(Substring(value, 1, ComplianceConditionCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ComplianceConditionCode attribute.</summary>
  [JsonPropertyName("complianceConditionCode")]
  [Computed]
  public string ComplianceConditionCode_Json
  {
    get => NullIf(ComplianceConditionCode, "");
    set => ComplianceConditionCode = value;
  }

  /// <summary>Length of the CURRENT_BALANCE attribute.</summary>
  public const int CurrentBalance_MaxLength = 9;

  /// <summary>
  /// The value of the CURRENT_BALANCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length = CurrentBalance_MaxLength)]
    
  public string CurrentBalance
  {
    get => currentBalance ?? "";
    set => currentBalance =
      TrimEnd(Substring(value, 1, CurrentBalance_MaxLength));
  }

  /// <summary>
  /// The json value of the CurrentBalance attribute.</summary>
  [JsonPropertyName("currentBalance")]
  [Computed]
  public string CurrentBalance_Json
  {
    get => NullIf(CurrentBalance, "");
    set => CurrentBalance = value;
  }

  /// <summary>Length of the AMOUNT_PAST_DUE attribute.</summary>
  public const int AmountPastDue_MaxLength = 9;

  /// <summary>
  /// The value of the AMOUNT_PAST_DUE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length = AmountPastDue_MaxLength)]
  public string AmountPastDue
  {
    get => amountPastDue ?? "";
    set => amountPastDue =
      TrimEnd(Substring(value, 1, AmountPastDue_MaxLength));
  }

  /// <summary>
  /// The json value of the AmountPastDue attribute.</summary>
  [JsonPropertyName("amountPastDue")]
  [Computed]
  public string AmountPastDue_Json
  {
    get => NullIf(AmountPastDue, "");
    set => AmountPastDue = value;
  }

  /// <summary>Length of the ORIGINAL_CHARGE_OFF_AMOUNT attribute.</summary>
  public const int OriginalChargeOffAmount_MaxLength = 9;

  /// <summary>
  /// The value of the ORIGINAL_CHARGE_OFF_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = OriginalChargeOffAmount_MaxLength)]
  public string OriginalChargeOffAmount
  {
    get => originalChargeOffAmount ?? "";
    set => originalChargeOffAmount =
      TrimEnd(Substring(value, 1, OriginalChargeOffAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the OriginalChargeOffAmount attribute.</summary>
  [JsonPropertyName("originalChargeOffAmount")]
  [Computed]
  public string OriginalChargeOffAmount_Json
  {
    get => NullIf(OriginalChargeOffAmount, "");
    set => OriginalChargeOffAmount = value;
  }

  /// <summary>Length of the DATE_OF_ACCOUNT_INFORMATION attribute.</summary>
  public const int DateOfAccountInformation_MaxLength = 8;

  /// <summary>
  /// The value of the DATE_OF_ACCOUNT_INFORMATION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = DateOfAccountInformation_MaxLength)]
  public string DateOfAccountInformation
  {
    get => dateOfAccountInformation ?? "";
    set => dateOfAccountInformation =
      TrimEnd(Substring(value, 1, DateOfAccountInformation_MaxLength));
  }

  /// <summary>
  /// The json value of the DateOfAccountInformation attribute.</summary>
  [JsonPropertyName("dateOfAccountInformation")]
  [Computed]
  public string DateOfAccountInformation_Json
  {
    get => NullIf(DateOfAccountInformation, "");
    set => DateOfAccountInformation = value;
  }

  /// <summary>Length of the DATE_OF_FIRST_DELINQUENCY attribute.</summary>
  public const int DateOfFirstDelinquency_MaxLength = 8;

  /// <summary>
  /// The value of the DATE_OF_FIRST_DELINQUENCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = DateOfFirstDelinquency_MaxLength)]
  public string DateOfFirstDelinquency
  {
    get => dateOfFirstDelinquency ?? "";
    set => dateOfFirstDelinquency =
      TrimEnd(Substring(value, 1, DateOfFirstDelinquency_MaxLength));
  }

  /// <summary>
  /// The json value of the DateOfFirstDelinquency attribute.</summary>
  [JsonPropertyName("dateOfFirstDelinquency")]
  [Computed]
  public string DateOfFirstDelinquency_Json
  {
    get => NullIf(DateOfFirstDelinquency, "");
    set => DateOfFirstDelinquency = value;
  }

  /// <summary>Length of the DATE_CLOSED attribute.</summary>
  public const int DateClosed_MaxLength = 8;

  /// <summary>
  /// The value of the DATE_CLOSED attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length = DateClosed_MaxLength)]
  public string DateClosed
  {
    get => dateClosed ?? "";
    set => dateClosed = TrimEnd(Substring(value, 1, DateClosed_MaxLength));
  }

  /// <summary>
  /// The json value of the DateClosed attribute.</summary>
  [JsonPropertyName("dateClosed")]
  [Computed]
  public string DateClosed_Json
  {
    get => NullIf(DateClosed, "");
    set => DateClosed = value;
  }

  /// <summary>Length of the DATE_OF_LAST_PAYMENT attribute.</summary>
  public const int DateOfLastPayment_MaxLength = 8;

  /// <summary>
  /// The value of the DATE_OF_LAST_PAYMENT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = DateOfLastPayment_MaxLength)]
  public string DateOfLastPayment
  {
    get => dateOfLastPayment ?? "";
    set => dateOfLastPayment =
      TrimEnd(Substring(value, 1, DateOfLastPayment_MaxLength));
  }

  /// <summary>
  /// The json value of the DateOfLastPayment attribute.</summary>
  [JsonPropertyName("dateOfLastPayment")]
  [Computed]
  public string DateOfLastPayment_Json
  {
    get => NullIf(DateOfLastPayment, "");
    set => DateOfLastPayment = value;
  }

  /// <summary>Length of the INTEREST_TYPE_INDICATOR attribute.</summary>
  public const int InterestTypeIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the INTEREST_TYPE_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = InterestTypeIndicator_MaxLength)]
  public string InterestTypeIndicator
  {
    get => interestTypeIndicator ?? "";
    set => interestTypeIndicator =
      TrimEnd(Substring(value, 1, InterestTypeIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the InterestTypeIndicator attribute.</summary>
  [JsonPropertyName("interestTypeIndicator")]
  [Computed]
  public string InterestTypeIndicator_Json
  {
    get => NullIf(InterestTypeIndicator, "");
    set => InterestTypeIndicator = value;
  }

  /// <summary>Length of the RESERVED_29 attribute.</summary>
  public const int Reserved29_MaxLength = 17;

  /// <summary>
  /// The value of the RESERVED_29 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length = Reserved29_MaxLength)]
  public string Reserved29
  {
    get => reserved29 ?? "";
    set => reserved29 = TrimEnd(Substring(value, 1, Reserved29_MaxLength));
  }

  /// <summary>
  /// The json value of the Reserved29 attribute.</summary>
  [JsonPropertyName("reserved29")]
  [Computed]
  public string Reserved29_Json
  {
    get => NullIf(Reserved29, "");
    set => Reserved29 = value;
  }

  /// <summary>Length of the SURNAME attribute.</summary>
  public const int Surname_MaxLength = 25;

  /// <summary>
  /// The value of the SURNAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length = Surname_MaxLength)]
  public string Surname
  {
    get => surname ?? "";
    set => surname = TrimEnd(Substring(value, 1, Surname_MaxLength));
  }

  /// <summary>
  /// The json value of the Surname attribute.</summary>
  [JsonPropertyName("surname")]
  [Computed]
  public string Surname_Json
  {
    get => NullIf(Surname, "");
    set => Surname = value;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 20;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length = FirstName_MaxLength)]
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

  /// <summary>Length of the MIDDLE_NAME attribute.</summary>
  public const int MiddleName_MaxLength = 20;

  /// <summary>
  /// The value of the MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length = MiddleName_MaxLength)]
  public string MiddleName
  {
    get => middleName ?? "";
    set => middleName = TrimEnd(Substring(value, 1, MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the MiddleName attribute.</summary>
  [JsonPropertyName("middleName")]
  [Computed]
  public string MiddleName_Json
  {
    get => NullIf(MiddleName, "");
    set => MiddleName = value;
  }

  /// <summary>Length of the GENERATION_CODE attribute.</summary>
  public const int GenerationCode_MaxLength = 1;

  /// <summary>
  /// The value of the GENERATION_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length = GenerationCode_MaxLength)]
    
  public string GenerationCode
  {
    get => generationCode ?? "";
    set => generationCode =
      TrimEnd(Substring(value, 1, GenerationCode_MaxLength));
  }

  /// <summary>
  /// The json value of the GenerationCode attribute.</summary>
  [JsonPropertyName("generationCode")]
  [Computed]
  public string GenerationCode_Json
  {
    get => NullIf(GenerationCode, "");
    set => GenerationCode = value;
  }

  /// <summary>Length of the SOCIAL_SECURITY_NUMBER attribute.</summary>
  public const int SocialSecurityNumber_MaxLength = 9;

  /// <summary>
  /// The value of the SOCIAL_SECURITY_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = SocialSecurityNumber_MaxLength)]
  public string SocialSecurityNumber
  {
    get => socialSecurityNumber ?? "";
    set => socialSecurityNumber =
      TrimEnd(Substring(value, 1, SocialSecurityNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the SocialSecurityNumber attribute.</summary>
  [JsonPropertyName("socialSecurityNumber")]
  [Computed]
  public string SocialSecurityNumber_Json
  {
    get => NullIf(SocialSecurityNumber, "");
    set => SocialSecurityNumber = value;
  }

  /// <summary>Length of the DATE_OF_BIRTH attribute.</summary>
  public const int DateOfBirth_MaxLength = 8;

  /// <summary>
  /// The value of the DATE_OF_BIRTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length = DateOfBirth_MaxLength)]
  public string DateOfBirth
  {
    get => dateOfBirth ?? "";
    set => dateOfBirth = TrimEnd(Substring(value, 1, DateOfBirth_MaxLength));
  }

  /// <summary>
  /// The json value of the DateOfBirth attribute.</summary>
  [JsonPropertyName("dateOfBirth")]
  [Computed]
  public string DateOfBirth_Json
  {
    get => NullIf(DateOfBirth, "");
    set => DateOfBirth = value;
  }

  /// <summary>Length of the TELEPHONE_NUMBER attribute.</summary>
  public const int TelephoneNumber_MaxLength = 10;

  /// <summary>
  /// The value of the TELEPHONE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length = TelephoneNumber_MaxLength)
    ]
  public string TelephoneNumber
  {
    get => telephoneNumber ?? "";
    set => telephoneNumber =
      TrimEnd(Substring(value, 1, TelephoneNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the TelephoneNumber attribute.</summary>
  [JsonPropertyName("telephoneNumber")]
  [Computed]
  public string TelephoneNumber_Json
  {
    get => NullIf(TelephoneNumber, "");
    set => TelephoneNumber = value;
  }

  /// <summary>Length of the ECOA_CODE attribute.</summary>
  public const int EcoaCode_MaxLength = 1;

  /// <summary>
  /// The value of the ECOA_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length = EcoaCode_MaxLength)]
  public string EcoaCode
  {
    get => ecoaCode ?? "";
    set => ecoaCode = TrimEnd(Substring(value, 1, EcoaCode_MaxLength));
  }

  /// <summary>
  /// The json value of the EcoaCode attribute.</summary>
  [JsonPropertyName("ecoaCode")]
  [Computed]
  public string EcoaCode_Json
  {
    get => NullIf(EcoaCode, "");
    set => EcoaCode = value;
  }

  /// <summary>Length of the CONSUMER_INFORMATION_INDICATOR attribute.</summary>
  public const int ConsumerInformationIndicator_MaxLength = 2;

  /// <summary>
  /// The value of the CONSUMER_INFORMATION_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = ConsumerInformationIndicator_MaxLength)]
  public string ConsumerInformationIndicator
  {
    get => consumerInformationIndicator ?? "";
    set => consumerInformationIndicator =
      TrimEnd(Substring(value, 1, ConsumerInformationIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the ConsumerInformationIndicator attribute.</summary>
  [JsonPropertyName("consumerInformationIndicator")]
  [Computed]
  public string ConsumerInformationIndicator_Json
  {
    get => NullIf(ConsumerInformationIndicator, "");
    set => ConsumerInformationIndicator = value;
  }

  /// <summary>Length of the COUNTRY_CODE attribute.</summary>
  public const int CountryCode_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTRY_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length = CountryCode_MaxLength)]
  public string CountryCode
  {
    get => countryCode ?? "";
    set => countryCode = TrimEnd(Substring(value, 1, CountryCode_MaxLength));
  }

  /// <summary>
  /// The json value of the CountryCode attribute.</summary>
  [JsonPropertyName("countryCode")]
  [Computed]
  public string CountryCode_Json
  {
    get => NullIf(CountryCode, "");
    set => CountryCode = value;
  }

  /// <summary>Length of the FIRST_LINE_OF_ADDRESS attribute.</summary>
  public const int FirstLineOfAddress_MaxLength = 32;

  /// <summary>
  /// The value of the FIRST_LINE_OF_ADDRESS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = FirstLineOfAddress_MaxLength)]
  public string FirstLineOfAddress
  {
    get => firstLineOfAddress ?? "";
    set => firstLineOfAddress =
      TrimEnd(Substring(value, 1, FirstLineOfAddress_MaxLength));
  }

  /// <summary>
  /// The json value of the FirstLineOfAddress attribute.</summary>
  [JsonPropertyName("firstLineOfAddress")]
  [Computed]
  public string FirstLineOfAddress_Json
  {
    get => NullIf(FirstLineOfAddress, "");
    set => FirstLineOfAddress = value;
  }

  /// <summary>Length of the SECOND_LINE_OF_ADDRESS attribute.</summary>
  public const int SecondLineOfAddress_MaxLength = 32;

  /// <summary>
  /// The value of the SECOND_LINE_OF_ADDRESS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = SecondLineOfAddress_MaxLength)]
  public string SecondLineOfAddress
  {
    get => secondLineOfAddress ?? "";
    set => secondLineOfAddress =
      TrimEnd(Substring(value, 1, SecondLineOfAddress_MaxLength));
  }

  /// <summary>
  /// The json value of the SecondLineOfAddress attribute.</summary>
  [JsonPropertyName("secondLineOfAddress")]
  [Computed]
  public string SecondLineOfAddress_Json
  {
    get => NullIf(SecondLineOfAddress, "");
    set => SecondLineOfAddress = value;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 20;

  /// <summary>
  /// The value of the CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length = City_MaxLength)]
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

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 44, Type = MemberType.Char, Length = State_MaxLength)]
  public string State
  {
    get => state ?? "";
    set => state = TrimEnd(Substring(value, 1, State_MaxLength));
  }

  /// <summary>
  /// The json value of the State attribute.</summary>
  [JsonPropertyName("state")]
  [Computed]
  public string State_Json
  {
    get => NullIf(State, "");
    set => State = value;
  }

  /// <summary>Length of the POSTAL_ZIP_CODE attribute.</summary>
  public const int PostalZipCode_MaxLength = 9;

  /// <summary>
  /// The value of the POSTAL_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length = PostalZipCode_MaxLength)]
  public string PostalZipCode
  {
    get => postalZipCode ?? "";
    set => postalZipCode =
      TrimEnd(Substring(value, 1, PostalZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the PostalZipCode attribute.</summary>
  [JsonPropertyName("postalZipCode")]
  [Computed]
  public string PostalZipCode_Json
  {
    get => NullIf(PostalZipCode, "");
    set => PostalZipCode = value;
  }

  /// <summary>Length of the ADDRESS_INDICATOR attribute.</summary>
  public const int AddressIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the ADDRESS_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = AddressIndicator_MaxLength)]
  public string AddressIndicator
  {
    get => addressIndicator ?? "";
    set => addressIndicator =
      TrimEnd(Substring(value, 1, AddressIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressIndicator attribute.</summary>
  [JsonPropertyName("addressIndicator")]
  [Computed]
  public string AddressIndicator_Json
  {
    get => NullIf(AddressIndicator, "");
    set => AddressIndicator = value;
  }

  /// <summary>Length of the RESIDENCE_CODE attribute.</summary>
  public const int ResidenceCode_MaxLength = 1;

  /// <summary>
  /// The value of the RESIDENCE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length = ResidenceCode_MaxLength)]
  public string ResidenceCode
  {
    get => residenceCode ?? "";
    set => residenceCode =
      TrimEnd(Substring(value, 1, ResidenceCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ResidenceCode attribute.</summary>
  [JsonPropertyName("residenceCode")]
  [Computed]
  public string ResidenceCode_Json
  {
    get => NullIf(ResidenceCode, "");
    set => ResidenceCode = value;
  }

  private string recordDescriptorWord;
  private string processingIndicator;
  private string timestamp;
  private string reserved4;
  private string identificationNumber;
  private string cycleIdentifier;
  private string consumerAccountNumber;
  private string portfolioType;
  private string accountType;
  private string dateOpened;
  private string creditLimit;
  private string highestCreditAmount;
  private string termsDuration;
  private string termsFrequency;
  private string scheduledMonthlyPaymentAmount;
  private string actualPaymentAmount;
  private string accountStatus;
  private string paymentRating;
  private string paymentHistoryProfile;
  private string specialComment;
  private string complianceConditionCode;
  private string currentBalance;
  private string amountPastDue;
  private string originalChargeOffAmount;
  private string dateOfAccountInformation;
  private string dateOfFirstDelinquency;
  private string dateClosed;
  private string dateOfLastPayment;
  private string interestTypeIndicator;
  private string reserved29;
  private string surname;
  private string firstName;
  private string middleName;
  private string generationCode;
  private string socialSecurityNumber;
  private string dateOfBirth;
  private string telephoneNumber;
  private string ecoaCode;
  private string consumerInformationIndicator;
  private string countryCode;
  private string firstLineOfAddress;
  private string secondLineOfAddress;
  private string city;
  private string state;
  private string postalZipCode;
  private string addressIndicator;
  private string residenceCode;
}
