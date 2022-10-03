// The source file: CASH_RECEIPT, ID: 371431285, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// * Confirmed * 10/26/94
/// This represents the cash, non-cash or interfund portion of cash received by 
/// CSE.  This is any negotiable instrument received directly by CSE and any
/// information about negotiable instruments received on behalf of CSE,
/// including AP to AR direct pays to record the form of cash receipt received
/// by CSE.
/// Examples:
/// Cash: Currency, Check, Electronic Funds Transfer, Money Order, etc.
/// Non-Cash: Non-ADC Direct Pays, Medical Direct Pays, etc.
/// Interfund: Set Off, Fee Fund, etc.
/// </summary>
[Serializable]
public partial class CashReceipt: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CashReceipt()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CashReceipt(CashReceipt that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CashReceipt Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the TOTAL_CASH_FEE_AMOUNT attribute.
  /// The CASH portion of the Total Fee Amount included on an interface.  Used 
  /// only for interfaced cash receipts.
  /// </summary>
  [JsonPropertyName("totalCashFeeAmount")]
  [Member(Index = 1, Type = MemberType.Number, Length = 11, Precision = 2, Optional
    = true)]
  public decimal? TotalCashFeeAmount
  {
    get => Get<decimal?>("totalCashFeeAmount");
    set => Set("totalCashFeeAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOTAL_NON_CASH_FEE_AMOUNT attribute.
  /// The NON-CASH portion of the total Fees included on an interface.  Only 
  /// used for interfaced cash receipts.
  /// </summary>
  [JsonPropertyName("totalNonCashFeeAmount")]
  [Member(Index = 2, Type = MemberType.Number, Length = 11, Precision = 2, Optional
    = true)]
  public decimal? TotalNonCashFeeAmount
  {
    get => Get<decimal?>("totalNonCashFeeAmount");
    set => Set("totalNonCashFeeAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the RECEIPT_AMOUNT attribute.
  /// This is the amount actually being receipted.  It is either the amount of 
  /// the check, money order, or currency.  For non-cash could be the amount of
  /// the direct payments from the court.
  /// This is the amount that gets deposited for cash-types.
  /// </summary>
  [JsonPropertyName("receiptAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal ReceiptAmount
  {
    get => Get<decimal?>("receiptAmount") ?? 0M;
    set => Set(
      "receiptAmount", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the SEQUENTIAL_NUMBER attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("sequentialNumber")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 9)]
  public int SequentialNumber
  {
    get => Get<int?>("sequentialNumber") ?? 0;
    set => Set("sequentialNumber", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the RECEIPT_DATE attribute.
  /// The date that the cash/noncash is receipted.  Usually the same date as the
  /// cash/noncash is received into CSE Receivables.
  /// </summary>
  [JsonPropertyName("receiptDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? ReceiptDate
  {
    get => Get<DateTime?>("receiptDate");
    set => Set("receiptDate", value);
  }

  /// <summary>Length of the CHECK_TYPE attribute.</summary>
  public const int CheckType_MaxLength = 10;

  /// <summary>
  /// The value of the CHECK_TYPE attribute.
  /// If the cash receipt is a check need to know the type of check it is.
  /// Examples are court check, fee fund check, or miscellaneous check.
  /// Miscellaneous checks are personal or out of state.
  /// This is important to the business because different types of checks are 
  /// handled in different manners.
  /// </summary>
  [JsonPropertyName("checkType")]
  [Member(Index = 6, Type = MemberType.Char, Length = CheckType_MaxLength, Optional
    = true)]
  public string CheckType
  {
    get => Get<string>("checkType");
    set => Set("checkType", TrimEnd(Substring(value, 1, CheckType_MaxLength)));
  }

  /// <summary>Length of the CHECK_NUMBER attribute.</summary>
  public const int CheckNumber_MaxLength = 12;

  /// <summary>
  /// The value of the CHECK_NUMBER attribute.
  /// A non-unique number on the payors check.
  /// </summary>
  [JsonPropertyName("checkNumber")]
  [Member(Index = 7, Type = MemberType.Char, Length = CheckNumber_MaxLength, Optional
    = true)]
  public string CheckNumber
  {
    get => Get<string>("checkNumber");
    set => Set(
      "checkNumber", TrimEnd(Substring(value, 1, CheckNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the CHECK_DATE attribute.
  /// The date specified on the check if the receipt is a check type.
  /// </summary>
  [JsonPropertyName("checkDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? CheckDate
  {
    get => Get<DateTime?>("checkDate");
    set => Set("checkDate", value);
  }

  /// <summary>
  /// The value of the RECEIVED_DATE attribute.
  /// The date the cash/noncash was received into CSE Receivable Department.  
  /// This may be different from the receipt date if the receipting process lags
  /// behind the mail.  If receipting occurs the same day the checks arrive
  /// then this date will be the same as the receipt date.
  /// </summary>
  [JsonPropertyName("receivedDate")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? ReceivedDate
  {
    get => Get<DateTime?>("receivedDate");
    set => Set("receivedDate", value);
  }

  /// <summary>
  /// The value of the DEPOSIT_RELEASE_DATE attribute.
  /// The date that the receipt has been included in a clearing fund deposit 
  /// transaction.
  /// </summary>
  [JsonPropertyName("depositReleaseDate")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? DepositReleaseDate
  {
    get => Get<DateTime?>("depositReleaseDate");
    set => Set("depositReleaseDate", value);
  }

  /// <summary>Length of the REFERENCE_NUMBER attribute.</summary>
  public const int ReferenceNumber_MaxLength = 12;

  /// <summary>
  /// The value of the REFERENCE_NUMBER attribute.
  /// The number that is related to or references the item entered (may be the 
  /// check number) from the courts.
  /// </summary>
  [JsonPropertyName("referenceNumber")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = ReferenceNumber_MaxLength, Optional = true)]
  public string ReferenceNumber
  {
    get => Get<string>("referenceNumber");
    set => Set(
      "referenceNumber",
      TrimEnd(Substring(value, 1, ReferenceNumber_MaxLength)));
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 240;

  /// <summary>
  /// The value of the NOTE attribute.
  /// An area for some verbage to contain any extra information related to a 
  /// cash receipt.
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 12, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => Get<string>("note");
    set => Set("note", Substring(value, 1, Note_MaxLength));
  }

  /// <summary>Length of the PAYOR_ORGANIZATION attribute.</summary>
  public const int PayorOrganization_MaxLength = 30;

  /// <summary>
  /// The value of the PAYOR_ORGANIZATION attribute.
  /// If the payor is an organization such as Johnson County or the State of 
  /// Montana, the name of the payor goes here and not in the individual name
  /// fields.
  /// </summary>
  [JsonPropertyName("payorOrganization")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = PayorOrganization_MaxLength, Optional = true)]
  public string PayorOrganization
  {
    get => Get<string>("payorOrganization");
    set => Set(
      "payorOrganization", TrimEnd(Substring(value, 1,
      PayorOrganization_MaxLength)));
  }

  /// <summary>Length of the PAYOR_FIRST_NAME attribute.</summary>
  public const int PayorFirstName_MaxLength = 20;

  /// <summary>
  /// The value of the PAYOR_FIRST_NAME attribute.
  /// The first name of the payor if the payor is an individual.  If the payor 
  /// is an organization the name goes into the payor organization name field.
  /// Attribute dependencies are if this field has a value then the payor last 
  /// name should also have a value.
  /// </summary>
  [JsonPropertyName("payorFirstName")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = PayorFirstName_MaxLength, Optional = true)]
  public string PayorFirstName
  {
    get => Get<string>("payorFirstName");
    set => Set(
      "payorFirstName", TrimEnd(Substring(value, 1, PayorFirstName_MaxLength)));
      
  }

  /// <summary>Length of the PAYOR_MIDDLE_NAME attribute.</summary>
  public const int PayorMiddleName_MaxLength = 20;

  /// <summary>
  /// The value of the PAYOR_MIDDLE_NAME attribute.
  /// The middle name of the payor.
  /// (See PAYOR_FIRST_NAME)
  /// </summary>
  [JsonPropertyName("payorMiddleName")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = PayorMiddleName_MaxLength, Optional = true)]
  public string PayorMiddleName
  {
    get => Get<string>("payorMiddleName");
    set => Set(
      "payorMiddleName",
      TrimEnd(Substring(value, 1, PayorMiddleName_MaxLength)));
  }

  /// <summary>Length of the PAYOR_LAST_NAME attribute.</summary>
  public const int PayorLastName_MaxLength = 30;

  /// <summary>
  /// The value of the PAYOR_LAST_NAME attribute.
  /// The last name of the payor.
  /// (See PAYOR_FIRST_NAME)
  /// </summary>
  [JsonPropertyName("payorLastName")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = PayorLastName_MaxLength, Optional = true)]
  public string PayorLastName
  {
    get => Get<string>("payorLastName");
    set => Set(
      "payorLastName", TrimEnd(Substring(value, 1, PayorLastName_MaxLength)));
  }

  /// <summary>Length of the FORWARDED_TO_NAME attribute.</summary>
  public const int ForwardedToName_MaxLength = 30;

  /// <summary>
  /// The value of the FORWARDED_TO_NAME attribute.
  /// The first name of the person to receive the forwarded cash receipt.
  /// </summary>
  [JsonPropertyName("forwardedToName")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = ForwardedToName_MaxLength, Optional = true)]
  public string ForwardedToName
  {
    get => Get<string>("forwardedToName");
    set => Set(
      "forwardedToName",
      TrimEnd(Substring(value, 1, ForwardedToName_MaxLength)));
  }

  /// <summary>Length of the FORWARDED_STREET_1 attribute.</summary>
  public const int ForwardedStreet1_MaxLength = 25;

  /// <summary>
  /// The value of the FORWARDED_STREET_1 attribute.
  /// The first line of the address.
  /// </summary>
  [JsonPropertyName("forwardedStreet1")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = ForwardedStreet1_MaxLength, Optional = true)]
  public string ForwardedStreet1
  {
    get => Get<string>("forwardedStreet1");
    set => Set(
      "forwardedStreet1",
      TrimEnd(Substring(value, 1, ForwardedStreet1_MaxLength)));
  }

  /// <summary>Length of the FORWARDED_STREET_2 attribute.</summary>
  public const int ForwardedStreet2_MaxLength = 25;

  /// <summary>
  /// The value of the FORWARDED_STREET_2 attribute.
  /// The second line of the address.
  /// </summary>
  [JsonPropertyName("forwardedStreet2")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = ForwardedStreet2_MaxLength, Optional = true)]
  public string ForwardedStreet2
  {
    get => Get<string>("forwardedStreet2");
    set => Set(
      "forwardedStreet2",
      TrimEnd(Substring(value, 1, ForwardedStreet2_MaxLength)));
  }

  /// <summary>Length of the FORWARDED_CITY attribute.</summary>
  public const int ForwardedCity_MaxLength = 30;

  /// <summary>
  /// The value of the FORWARDED_CITY attribute.
  /// The city part of the address.
  /// </summary>
  [JsonPropertyName("forwardedCity")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = ForwardedCity_MaxLength, Optional = true)]
  public string ForwardedCity
  {
    get => Get<string>("forwardedCity");
    set => Set(
      "forwardedCity", TrimEnd(Substring(value, 1, ForwardedCity_MaxLength)));
  }

  /// <summary>Length of the FORWARDED_STATE attribute.</summary>
  public const int ForwardedState_MaxLength = 2;

  /// <summary>
  /// The value of the FORWARDED_STATE attribute.
  /// The state part of the address.
  /// </summary>
  [JsonPropertyName("forwardedState")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = ForwardedState_MaxLength, Optional = true)]
  public string ForwardedState
  {
    get => Get<string>("forwardedState");
    set => Set(
      "forwardedState", TrimEnd(Substring(value, 1, ForwardedState_MaxLength)));
      
  }

  /// <summary>Length of the FORWARDED_ZIP5 attribute.</summary>
  public const int ForwardedZip5_MaxLength = 5;

  /// <summary>
  /// The value of the FORWARDED_ZIP5 attribute.
  /// The first five digits of the zip code.
  /// </summary>
  [JsonPropertyName("forwardedZip5")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = ForwardedZip5_MaxLength, Optional = true)]
  public string ForwardedZip5
  {
    get => Get<string>("forwardedZip5");
    set => Set(
      "forwardedZip5", TrimEnd(Substring(value, 1, ForwardedZip5_MaxLength)));
  }

  /// <summary>Length of the FORWARDED_ZIP4 attribute.</summary>
  public const int ForwardedZip4_MaxLength = 4;

  /// <summary>
  /// The value of the FORWARDED_ZIP4 attribute.
  /// The second part of the zip code.  It consists of four digits.
  /// </summary>
  [JsonPropertyName("forwardedZip4")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = ForwardedZip4_MaxLength, Optional = true)]
  public string ForwardedZip4
  {
    get => Get<string>("forwardedZip4");
    set => Set(
      "forwardedZip4", TrimEnd(Substring(value, 1, ForwardedZip4_MaxLength)));
  }

  /// <summary>Length of the FORWARDED_ZIP3 attribute.</summary>
  public const int ForwardedZip3_MaxLength = 3;

  /// <summary>
  /// The value of the FORWARDED_ZIP3 attribute.
  /// The third part of zip code.  It consists of a two digit house number and a
  /// one digit check code number.
  /// </summary>
  [JsonPropertyName("forwardedZip3")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = ForwardedZip3_MaxLength, Optional = true)]
  public string ForwardedZip3
  {
    get => Get<string>("forwardedZip3");
    set => Set(
      "forwardedZip3", TrimEnd(Substring(value, 1, ForwardedZip3_MaxLength)));
  }

  /// <summary>
  /// The value of the BALANCED_TIMESTAMP attribute.
  /// The date and time that this cash receipt was balanced.
  /// </summary>
  [JsonPropertyName("balancedTimestamp")]
  [Member(Index = 25, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? BalancedTimestamp
  {
    get => Get<DateTime?>("balancedTimestamp");
    set => Set("balancedTimestamp", value);
  }

  /// <summary>
  /// The value of the TOTAL_CASH_TRANSACTION_AMOUNT attribute.
  /// The amount of money for the cash receipt specified in US currency for the 
  /// cash portion of a cash receipt.  This field is only used for interfaced
  /// receipts.
  /// </summary>
  [JsonPropertyName("totalCashTransactionAmount")]
  [Member(Index = 26, Type = MemberType.Number, Length = 11, Precision = 2, Optional
    = true)]
  public decimal? TotalCashTransactionAmount
  {
    get => Get<decimal?>("totalCashTransactionAmount");
    set => Set("totalCashTransactionAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOTAL_NONCASH_TRANSACTION_AMOUNT attribute.
  /// The dollar amount of this cash receipt that represents non-cash 
  /// collections.  This field is only used for interfaced receipts.
  /// </summary>
  [JsonPropertyName("totalNoncashTransactionAmount")]
  [Member(Index = 27, Type = MemberType.Number, Length = 11, Precision = 2, Optional
    = true)]
  public decimal? TotalNoncashTransactionAmount
  {
    get => Get<decimal?>("totalNoncashTransactionAmount");
    set => Set("totalNoncashTransactionAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the TOTAL_CASH_TRANSACTION_COUNT attribute.
  /// The number of transaction records/rows that are part of this cash receipt 
  /// that represent cash collections.  This field is only used for interfaced
  /// receipts.
  /// </summary>
  [JsonPropertyName("totalCashTransactionCount")]
  [Member(Index = 28, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? TotalCashTransactionCount
  {
    get => Get<int?>("totalCashTransactionCount");
    set => Set("totalCashTransactionCount", value);
  }

  /// <summary>
  /// The value of the TOTAL_NONCASH_TRANSACTION_COUNT attribute.
  /// The number of transaction records/rows that are part of this cash receipt 
  /// that represent non-cash collections.  This field is only used for
  /// interfaced receipts.
  /// </summary>
  [JsonPropertyName("totalNoncashTransactionCount")]
  [Member(Index = 29, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? TotalNoncashTransactionCount
  {
    get => Get<int?>("totalNoncashTransactionCount");
    set => Set("totalNoncashTransactionCount", value);
  }

  /// <summary>
  /// The value of the TOTAL_DETAIL_ADJUSTMENT_COUNT attribute.
  /// The total number of detail records/rows for this cash receipt that 
  /// represent adjustments.
  /// </summary>
  [JsonPropertyName("totalDetailAdjustmentCount")]
  [Member(Index = 30, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? TotalDetailAdjustmentCount
  {
    get => Get<int?>("totalDetailAdjustmentCount");
    set => Set("totalDetailAdjustmentCount", value);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The userid or program id responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => Get<string>("createdBy") ?? "";
    set => Set(
      "createdBy", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CreatedBy_MaxLength)));
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
  /// The system time the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 32, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 34, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => Get<DateTime?>("lastUpdatedTimestamp");
    set => Set("lastUpdatedTimestamp", value);
  }

  /// <summary>
  /// The value of the CASH_DUE attribute.
  /// The amount of cash SRS has calculated to be due from the court.  Will try 
  /// to balance with the check amount on the event which is the court
  /// calculated amount.
  /// This amount will never be negative.
  /// </summary>
  [JsonPropertyName("cashDue")]
  [Member(Index = 35, Type = MemberType.Number, Length = 11, Precision = 2, Optional
    = true)]
  public decimal? CashDue
  {
    get => Get<decimal?>("cashDue");
    set => Set("cashDue", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CASH_BALANCE_AMT attribute.
  /// The calculated diffeernce between the cash SRS has calculated for 
  /// remittance and the amount the court has calculated for remittance.
  /// Always keps as a positive number.  If amount is > 0 then the cash balance 
  /// reason must have either and OVER or UNDER in it.
  /// </summary>
  [JsonPropertyName("cashBalanceAmt")]
  [Member(Index = 36, Type = MemberType.Number, Length = 11, Precision = 2, Optional
    = true)]
  public decimal? CashBalanceAmt
  {
    get => Get<decimal?>("cashBalanceAmt");
    set => Set("cashBalanceAmt", Truncate(value, 2));
  }

  /// <summary>Length of the CASH_BALANCE_REASON attribute.</summary>
  public const int CashBalanceReason_MaxLength = 5;

  /// <summary>
  /// The value of the CASH_BALANCE_REASON attribute.
  /// The reason for the out-of-balance cash condition.  This field is required 
  /// to have either and OVER or UNDER in it if the cash balance amount is > 0.
  /// An OVER  indicates that SRS has received an overage of cash from the court
  /// due to a subsequent cash detail adjustment.
  /// An UNDER indicates that the cash total is more than the cash being 
  /// remitted making the cash being sent under what appears to be due.
  /// </summary>
  [JsonPropertyName("cashBalanceReason")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = CashBalanceReason_MaxLength, Optional = true)]
  [Value(null)]
  [Value("OVER")]
  [Value("UNDER")]
  [Value("")]
  public string CashBalanceReason
  {
    get => Get<string>("cashBalanceReason");
    set => Set(
      "cashBalanceReason", TrimEnd(Substring(value, 1,
      CashBalanceReason_MaxLength)));
  }

  /// <summary>Length of the PAYOR_SOCIAL_SECURITY_NUMBER attribute.</summary>
  public const int PayorSocialSecurityNumber_MaxLength = 9;

  /// <summary>
  /// The value of the PAYOR_SOCIAL_SECURITY_NUMBER attribute.
  /// The unique number assigned to the payor by the Social Security 
  /// Administration.
  /// </summary>
  [JsonPropertyName("payorSocialSecurityNumber")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = PayorSocialSecurityNumber_MaxLength, Optional = true)]
  public string PayorSocialSecurityNumber
  {
    get => Get<string>("payorSocialSecurityNumber");
    set => Set(
      "payorSocialSecurityNumber", TrimEnd(Substring(value, 1,
      PayorSocialSecurityNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number that identifies each negotiable instrument in 
  /// any form of money received by CSE.  Or any information of money received
  /// by CSE.
  /// Examples:
  /// Cash, checks, court transmittals.
  /// </summary>
  [JsonPropertyName("crvIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 39, Type = MemberType.Number, Length = 9)]
  public int CrvIdentifier
  {
    get => Get<int?>("crvIdentifier") ?? 0;
    set => Set("crvIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cstIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 40, Type = MemberType.Number, Length = 3)]
  public int CstIdentifier
  {
    get => Get<int?>("cstIdentifier") ?? 0;
    set => Set("cstIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 41, Type = MemberType.Number, Length = 3)]
  public int CrtIdentifier
  {
    get => Get<int?>("crtIdentifier") ?? 0;
    set => Set("crtIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique number that specifies each occurrence of the entity type.
  /// </summary>
  [JsonPropertyName("ftrIdentifier")]
  [Member(Index = 42, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FtrIdentifier
  {
    get => Get<int?>("ftrIdentifier");
    set => Set("ftrIdentifier", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("funIdentifier")]
  [Member(Index = 43, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? FunIdentifier
  {
    get => Get<int?>("funIdentifier");
    set => Set("funIdentifier", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("fttIdentifier")]
  [Member(Index = 44, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? FttIdentifier
  {
    get => Get<int?>("fttIdentifier");
    set => Set("fttIdentifier", value);
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence is activated by the system.  An 
  /// effective date allows the entity to be entered into the system with a
  /// future date.  The occurrence of the entity will &quot;take effect&quot; on
  /// the effective date.
  /// </summary>
  [JsonPropertyName("pcaEffectiveDate")]
  [Member(Index = 45, Type = MemberType.Date, Optional = true)]
  public DateTime? PcaEffectiveDate
  {
    get => Get<DateTime?>("pcaEffectiveDate");
    set => Set("pcaEffectiveDate", value);
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int PcaCode_MaxLength = 5;

  /// <summary>
  /// The value of the CODE attribute.
  /// A short representation  for the purpose of quick identification.
  /// </summary>
  [JsonPropertyName("pcaCode")]
  [Member(Index = 46, Type = MemberType.Char, Length = PcaCode_MaxLength, Optional
    = true)]
  public string PcaCode
  {
    get => Get<string>("pcaCode");
    set => Set("pcaCode", TrimEnd(Substring(value, 1, PcaCode_MaxLength)));
  }
}
