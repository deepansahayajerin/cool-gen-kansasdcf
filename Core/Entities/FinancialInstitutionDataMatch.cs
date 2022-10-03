// The source file: FINANCIAL_INSTITUTION_DATA_MATCH, ID: 374391204, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  FINANCIAL
/// 
/// Data match records from Financial Institutions.
/// </summary>
[Serializable]
public partial class FinancialInstitutionDataMatch: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FinancialInstitutionDataMatch()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FinancialInstitutionDataMatch(FinancialInstitutionDataMatch that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FinancialInstitutionDataMatch Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FinancialInstitutionDataMatch that)
  {
    base.Assign(that);
    csePersonNumber = that.csePersonNumber;
    institutionTin = that.institutionTin;
    matchedPayeeAccountNumber = that.matchedPayeeAccountNumber;
    matchRunDate = that.matchRunDate;
    matchedPayeeSsn = that.matchedPayeeSsn;
    matchedPayeeName = that.matchedPayeeName;
    matchedPayeeStreetAddress = that.matchedPayeeStreetAddress;
    matchedPayeeCity = that.matchedPayeeCity;
    matchedPayeeState = that.matchedPayeeState;
    matchedPayeeZipCode = that.matchedPayeeZipCode;
    matchedPayeeZip4 = that.matchedPayeeZip4;
    matchedPayeeZip3 = that.matchedPayeeZip3;
    payeeForeignCountryIndicator = that.payeeForeignCountryIndicator;
    matchFlag = that.matchFlag;
    accountBalance = that.accountBalance;
    accountType = that.accountType;
    trustFundIndicator = that.trustFundIndicator;
    accountBalanceIndicator = that.accountBalanceIndicator;
    dateOfBirth = that.dateOfBirth;
    payeeIndicator = that.payeeIndicator;
    accountFullLegalTitle = that.accountFullLegalTitle;
    primarySsn = that.primarySsn;
    secondPayeeName = that.secondPayeeName;
    secondPayeeSsn = that.secondPayeeSsn;
    msfidmIndicator = that.msfidmIndicator;
    institutionName = that.institutionName;
    institutionStreetAddress = that.institutionStreetAddress;
    institutionCity = that.institutionCity;
    institutionState = that.institutionState;
    institutionZipCode = that.institutionZipCode;
    institutionZip4 = that.institutionZip4;
    institutionZip3 = that.institutionZip3;
    secondInstitutionName = that.secondInstitutionName;
    transmitterTin = that.transmitterTin;
    transmitterName = that.transmitterName;
    transmitterStreetAddress = that.transmitterStreetAddress;
    transmitterCity = that.transmitterCity;
    transmitterState = that.transmitterState;
    transmitterZipCode = that.transmitterZipCode;
    transmitterZip4 = that.transmitterZip4;
    transmitterZip3 = that.transmitterZip3;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    accountStatusIndicator = that.accountStatusIndicator;
    note = that.note;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CsePersonNumber_MaxLength)]
    
  public string CsePersonNumber
  {
    get => csePersonNumber ?? "";
    set => csePersonNumber =
      TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CsePersonNumber attribute.</summary>
  [JsonPropertyName("csePersonNumber")]
  [Computed]
  public string CsePersonNumber_Json
  {
    get => NullIf(CsePersonNumber, "");
    set => CsePersonNumber = value;
  }

  /// <summary>Length of the INSTITUTION_TIN attribute.</summary>
  public const int InstitutionTin_MaxLength = 9;

  /// <summary>
  /// The value of the INSTITUTION_TIN attribute.
  /// Valid nine-digit Taxpayer Identification Number assigned to the Financial 
  /// Institution holding the account.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = InstitutionTin_MaxLength)]
  public string InstitutionTin
  {
    get => institutionTin ?? "";
    set => institutionTin =
      TrimEnd(Substring(value, 1, InstitutionTin_MaxLength));
  }

  /// <summary>
  /// The json value of the InstitutionTin attribute.</summary>
  [JsonPropertyName("institutionTin")]
  [Computed]
  public string InstitutionTin_Json
  {
    get => NullIf(InstitutionTin, "");
    set => InstitutionTin = value;
  }

  /// <summary>Length of the MATCHED_PAYEE_ACCOUNT_NUMBER attribute.</summary>
  public const int MatchedPayeeAccountNumber_MaxLength = 20;

  /// <summary>
  /// The value of the MATCHED_PAYEE_ACCOUNT_NUMBER attribute.
  /// The account number of the payee from the financial institution.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = MatchedPayeeAccountNumber_MaxLength)]
  public string MatchedPayeeAccountNumber
  {
    get => matchedPayeeAccountNumber ?? "";
    set => matchedPayeeAccountNumber =
      TrimEnd(Substring(value, 1, MatchedPayeeAccountNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the MatchedPayeeAccountNumber attribute.</summary>
  [JsonPropertyName("matchedPayeeAccountNumber")]
  [Computed]
  public string MatchedPayeeAccountNumber_Json
  {
    get => NullIf(MatchedPayeeAccountNumber, "");
    set => MatchedPayeeAccountNumber = value;
  }

  /// <summary>Length of the MATCH_RUN_DATE attribute.</summary>
  public const int MatchRunDate_MaxLength = 6;

  /// <summary>
  /// The value of the MATCH_RUN_DATE attribute.
  /// Year and Month from the incoming file.
  /// If Mehod#1-The
  /// year and month the file is generated.
  /// If Mehod#2-The year and month the Inquiry File was
  /// generated from the &quot;D&quot; record that was
  /// supplied by the State for the Financial Institution.
  /// 
  /// If MSFIDM-The year and month the MSFI Inquiry File
  /// was generated.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = MatchRunDate_MaxLength)]
  public string MatchRunDate
  {
    get => matchRunDate ?? "";
    set => matchRunDate = TrimEnd(Substring(value, 1, MatchRunDate_MaxLength));
  }

  /// <summary>
  /// The json value of the MatchRunDate attribute.</summary>
  [JsonPropertyName("matchRunDate")]
  [Computed]
  public string MatchRunDate_Json
  {
    get => NullIf(MatchRunDate, "");
    set => MatchRunDate = value;
  }

  /// <summary>Length of the MATCHED_PAYEE_SSN attribute.</summary>
  public const int MatchedPayeeSsn_MaxLength = 9;

  /// <summary>
  /// The value of the MATCHED_PAYEE_SSN attribute.
  /// The social security number matched from the financial institution.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = MatchedPayeeSsn_MaxLength)]
    
  public string MatchedPayeeSsn
  {
    get => matchedPayeeSsn ?? "";
    set => matchedPayeeSsn =
      TrimEnd(Substring(value, 1, MatchedPayeeSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the MatchedPayeeSsn attribute.</summary>
  [JsonPropertyName("matchedPayeeSsn")]
  [Computed]
  public string MatchedPayeeSsn_Json
  {
    get => NullIf(MatchedPayeeSsn, "");
    set => MatchedPayeeSsn = value;
  }

  /// <summary>Length of the MATCHED_PAYEE_NAME attribute.</summary>
  public const int MatchedPayeeName_MaxLength = 40;

  /// <summary>
  /// The value of the MATCHED_PAYEE_NAME attribute.
  /// This field will contain the name on the account for the SSN from the 
  /// financial institution match.
  /// </summary>
  [JsonPropertyName("matchedPayeeName")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = MatchedPayeeName_MaxLength, Optional = true)]
  public string MatchedPayeeName
  {
    get => matchedPayeeName;
    set => matchedPayeeName = value != null
      ? TrimEnd(Substring(value, 1, MatchedPayeeName_MaxLength)) : null;
  }

  /// <summary>Length of the MATCHED_PAYEE_STREET_ADDRESS attribute.</summary>
  public const int MatchedPayeeStreetAddress_MaxLength = 40;

  /// <summary>
  /// The value of the MATCHED_PAYEE_STREET_ADDRESS attribute.
  /// The address of the person whose SSN has been matched from the financial 
  /// institution. If there is no address for the first payee then the address
  /// of the second account owner will be entered here. Spaces will appear in
  /// this field if a street address was not provided.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = MatchedPayeeStreetAddress_MaxLength)]
  public string MatchedPayeeStreetAddress
  {
    get => matchedPayeeStreetAddress ?? "";
    set => matchedPayeeStreetAddress =
      TrimEnd(Substring(value, 1, MatchedPayeeStreetAddress_MaxLength));
  }

  /// <summary>
  /// The json value of the MatchedPayeeStreetAddress attribute.</summary>
  [JsonPropertyName("matchedPayeeStreetAddress")]
  [Computed]
  public string MatchedPayeeStreetAddress_Json
  {
    get => NullIf(MatchedPayeeStreetAddress, "");
    set => MatchedPayeeStreetAddress = value;
  }

  /// <summary>Length of the MATCHED_PAYEE_CITY attribute.</summary>
  public const int MatchedPayeeCity_MaxLength = 30;

  /// <summary>
  /// The value of the MATCHED_PAYEE_CITY attribute.
  /// The city of the person whose SSN has been matched from the financial 
  /// institution. If there is no address for the first payee then the address
  /// of the second account owner will be entered here. Spaces will appear in
  /// this field if a city was not provided.
  /// </summary>
  [JsonPropertyName("matchedPayeeCity")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = MatchedPayeeCity_MaxLength, Optional = true)]
  public string MatchedPayeeCity
  {
    get => matchedPayeeCity;
    set => matchedPayeeCity = value != null
      ? TrimEnd(Substring(value, 1, MatchedPayeeCity_MaxLength)) : null;
  }

  /// <summary>Length of the MATCHED_PAYEE_STATE attribute.</summary>
  public const int MatchedPayeeState_MaxLength = 2;

  /// <summary>
  /// The value of the MATCHED_PAYEE_STATE attribute.
  /// The state of the person whose SSN has been matched from the financial 
  /// institution. If there is no address for the first payee then the address
  /// of the second account owner will be entered here. Spaces will appear in
  /// this field if a state was not provided.
  /// </summary>
  [JsonPropertyName("matchedPayeeState")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = MatchedPayeeState_MaxLength, Optional = true)]
  public string MatchedPayeeState
  {
    get => matchedPayeeState;
    set => matchedPayeeState = value != null
      ? TrimEnd(Substring(value, 1, MatchedPayeeState_MaxLength)) : null;
  }

  /// <summary>Length of the MATCHED_PAYEE_ZIP_CODE attribute.</summary>
  public const int MatchedPayeeZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the MATCHED_PAYEE_ZIP_CODE attribute.
  /// The first 5 digits of the zip code. The zip code of the person whose SSN 
  /// has been matched from the financial institution. If there is no address
  /// for the first payee then the address of the second account owner will be
  /// entered here. Spaces will appear in this field if a zip code was not
  /// provided.
  /// </summary>
  [JsonPropertyName("matchedPayeeZipCode")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = MatchedPayeeZipCode_MaxLength, Optional = true)]
  public string MatchedPayeeZipCode
  {
    get => matchedPayeeZipCode;
    set => matchedPayeeZipCode = value != null
      ? TrimEnd(Substring(value, 1, MatchedPayeeZipCode_MaxLength)) : null;
  }

  /// <summary>Length of the MATCHED_PAYEE_ZIP4 attribute.</summary>
  public const int MatchedPayeeZip4_MaxLength = 4;

  /// <summary>
  /// The value of the MATCHED_PAYEE_ZIP4 attribute.
  /// The second 4 digits of the zip code. The zip code of the person whose SSN 
  /// has been matched from the financial institution. If there is no address
  /// for the first payee then the address of the second account owner will be
  /// entered here. Spaces will appear in this field if a zip code was not
  /// provided.
  /// </summary>
  [JsonPropertyName("matchedPayeeZip4")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = MatchedPayeeZip4_MaxLength, Optional = true)]
  public string MatchedPayeeZip4
  {
    get => matchedPayeeZip4;
    set => matchedPayeeZip4 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPayeeZip4_MaxLength)) : null;
  }

  /// <summary>Length of the MATCHED_PAYEE_ZIP3 attribute.</summary>
  public const int MatchedPayeeZip3_MaxLength = 3;

  /// <summary>
  /// The value of the MATCHED_PAYEE_ZIP3 attribute.
  /// The last 3 digits of the zip code. The zip code of the person whose SSN 
  /// has been matched from the financial institution. If there is no address
  /// for the first payee then the address of the second account owner will be
  /// entered here. Spaces will appear in this field if a zip code was not
  /// provided.
  /// </summary>
  [JsonPropertyName("matchedPayeeZip3")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = MatchedPayeeZip3_MaxLength, Optional = true)]
  public string MatchedPayeeZip3
  {
    get => matchedPayeeZip3;
    set => matchedPayeeZip3 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPayeeZip3_MaxLength)) : null;
  }

  /// <summary>Length of the PAYEE_FOREIGN_COUNTRY_INDICATOR attribute.
  /// </summary>
  public const int PayeeForeignCountryIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the PAYEE_FOREIGN_COUNTRY_INDICATOR attribute.
  /// If the address of the payee is in a foreign country Values are &quot;Y
  /// &quot; or Blank.
  /// </summary>
  [JsonPropertyName("payeeForeignCountryIndicator")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = PayeeForeignCountryIndicator_MaxLength, Optional = true)]
  public string PayeeForeignCountryIndicator
  {
    get => payeeForeignCountryIndicator;
    set => payeeForeignCountryIndicator = value != null
      ? TrimEnd(Substring(value, 1, PayeeForeignCountryIndicator_MaxLength)) : null
      ;
  }

  /// <summary>Length of the MATCH_FLAG attribute.</summary>
  public const int MatchFlag_MaxLength = 1;

  /// <summary>
  /// The value of the MATCH_FLAG attribute.
  /// Value returned from the financial institution regarding the SSN and name 
  /// match with the Inquiry File supplied by the State or OCSE.
  /// 
  /// Blank if from Method#1.
  /// &quot;O&quot;
  /// if the instituion is unable to match the last name.
  /// &quot;1&quot; if the first four letters of the matched last name, and that
  /// if the Inquiry File are the same.
  /// &quot;2&quot; if the
  /// first four letters of the matched last name, and that of the Inquiry File
  /// last name are not the same.
  /// </summary>
  [JsonPropertyName("matchFlag")]
  [Member(Index = 14, Type = MemberType.Char, Length = MatchFlag_MaxLength, Optional
    = true)]
  public string MatchFlag
  {
    get => matchFlag;
    set => matchFlag = value != null
      ? TrimEnd(Substring(value, 1, MatchFlag_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ACCOUNT_BALANCE attribute.
  /// Account balance for the matched individual for the account identified on 
  /// the record. The account balance or value in whole dollars only. For
  /// brokerage firms reporting margin accounts, the balance or value is the
  /// account holders equity position, or the value of the account less any
  /// borrowed amount.
  /// </summary>
  [JsonPropertyName("accountBalance")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AccountBalance
  {
    get => accountBalance;
    set => accountBalance = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the ACCOUNT_TYPE attribute.</summary>
  public const int AccountType_MaxLength = 2;

  /// <summary>
  /// The value of the ACCOUNT_TYPE attribute.
  /// Two digit code which identifies the type of account. If an IRA or ERISA 
  /// Plan contains any of the others, identify the account only as an IRA or
  /// ERISA Plan. A Compound Account is an investment account where portions of
  /// the balance are in differing funds-stock, money market, bonds, etc.
  /// 
  /// &quot;00&quot;=Not Applicable
  /// &quot;01
  /// &quot;=Savings Account
  /// &quot;04
  /// &quot;=Checking/Demand Deposit Account
  /// &quot;05&quot;=Term Deposit Certificate
  /// &quot;11&quot;=Money
  /// Market Account                                                      &quot;
  /// 12&quot;=IRA/Keogh Account
  /// &quot;14&quot;
  /// =ERISA Plan Accounts
  /// &quot;16&quot;=Cash
  /// Balances
  /// &quot;17&quot;=Compound Account
  /// &quot;18&quot;
  /// =Other
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = AccountType_MaxLength)]
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

  /// <summary>Length of the TRUST_FUND_INDICATOR attribute.</summary>
  public const int TrustFundIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the TRUST_FUND_INDICATOR attribute.
  /// A single digit to indicate whether the account registration 
  /// indicates it is a trust or excrow account. Enter a zero(0) if the
  /// account is not registered as a trust or excrow. For closed
  /// accounts, a zero may be entered but not a blank.
  /// &quot;0&quot;=Not a Trust Account
  /// &quot;1
  /// &quot;=UTMA/UGMA Account
  /// &quot;2
  /// &quot;=IOLTA Account
  /// 
  /// &quot;3&quot;=Mortgage Escrow Account
  /// &quot;4&quot;
  /// =Security Deposits(including Real Estate)
  /// &quot;5&quot;=Other Trust/Escrow
  /// &quot;6
  /// &quot;=Information Not Available
  /// </summary>
  [JsonPropertyName("trustFundIndicator")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = TrustFundIndicator_MaxLength, Optional = true)]
  public string TrustFundIndicator
  {
    get => trustFundIndicator;
    set => trustFundIndicator = value != null
      ? TrimEnd(Substring(value, 1, TrustFundIndicator_MaxLength)) : null;
  }

  /// <summary>Length of the ACCOUNT_BALANCE_INDICATOR attribute.</summary>
  public const int AccountBalanceIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the ACCOUNT_BALANCE_INDICATOR attribute.
  /// Indicator used to identify how the account balance was obtained.
  /// 
  /// &quot;0&quot; if the Account Balance to be entered in the account balance
  /// field has not been provided.
  /// &quot;1&quot; if an average balance is reported.
  /// &quot;2&quot; if a current
  /// balance (as of the day the report is created) is provided.
  /// </summary>
  [JsonPropertyName("accountBalanceIndicator")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = AccountBalanceIndicator_MaxLength, Optional = true)]
  public string AccountBalanceIndicator
  {
    get => accountBalanceIndicator;
    set => accountBalanceIndicator = value != null
      ? TrimEnd(Substring(value, 1, AccountBalanceIndicator_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DATE_OF_BIRTH attribute.
  /// Date of birth of the matched payee account owner.
  /// </summary>
  [JsonPropertyName("dateOfBirth")]
  [Member(Index = 19, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfBirth
  {
    get => dateOfBirth;
    set => dateOfBirth = value;
  }

  /// <summary>Length of the PAYEE_INDICATOR attribute.</summary>
  public const int PayeeIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the PAYEE_INDICATOR attribute.
  /// Indicator used to determine if the payee is the primary or secondary owner
  /// of the account.                                              &quot;0
  /// &quot; if the matched account owner is the sole owner of the account.
  /// 
  /// &quot;1&quot; if a match is generated against a secondary owner's SSN.
  /// 
  /// &quot;2&quot; if the matched account is to the primary owner, and there
  /// are secondary owners to the same account.
  /// </summary>
  [JsonPropertyName("payeeIndicator")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = PayeeIndicator_MaxLength, Optional = true)]
  public string PayeeIndicator
  {
    get => payeeIndicator;
    set => payeeIndicator = value != null
      ? TrimEnd(Substring(value, 1, PayeeIndicator_MaxLength)) : null;
  }

  /// <summary>Length of the ACCOUNT_FULL_LEGAL_TITLE attribute.</summary>
  public const int AccountFullLegalTitle_MaxLength = 100;

  /// <summary>
  /// The value of the ACCOUNT_FULL_LEGAL_TITLE attribute.
  /// The full account title of the account matched. Some institutions may find 
  /// this helpful to report trust accounts, or other titles.
  /// </summary>
  [JsonPropertyName("accountFullLegalTitle")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = AccountFullLegalTitle_MaxLength, Optional = true)]
  public string AccountFullLegalTitle
  {
    get => accountFullLegalTitle;
    set => accountFullLegalTitle = value != null
      ? TrimEnd(Substring(value, 1, AccountFullLegalTitle_MaxLength)) : null;
  }

  /// <summary>Length of the PRIMARY_SSN attribute.</summary>
  public const int PrimarySsn_MaxLength = 9;

  /// <summary>
  /// The value of the PRIMARY_SSN attribute.
  /// The SSN that is the primary owner of the account if the matched person is 
  /// the secondary owner on the account.
  /// </summary>
  [JsonPropertyName("primarySsn")]
  [Member(Index = 22, Type = MemberType.Char, Length = PrimarySsn_MaxLength, Optional
    = true)]
  public string PrimarySsn
  {
    get => primarySsn;
    set => primarySsn = value != null
      ? TrimEnd(Substring(value, 1, PrimarySsn_MaxLength)) : null;
  }

  /// <summary>Length of the SECOND_PAYEE_NAME attribute.</summary>
  public const int SecondPayeeName_MaxLength = 40;

  /// <summary>
  /// The value of the SECOND_PAYEE_NAME attribute.
  /// The name of any other owner of the account. If none exists, leave blank. 
  /// If the secondary owner is the matched person, then this field will contain
  /// the primary owner's name.
  /// </summary>
  [JsonPropertyName("secondPayeeName")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = SecondPayeeName_MaxLength, Optional = true)]
  public string SecondPayeeName
  {
    get => secondPayeeName;
    set => secondPayeeName = value != null
      ? TrimEnd(Substring(value, 1, SecondPayeeName_MaxLength)) : null;
  }

  /// <summary>Length of the SECOND_PAYEE_SSN attribute.</summary>
  public const int SecondPayeeSsn_MaxLength = 9;

  /// <summary>
  /// The value of the SECOND_PAYEE_SSN attribute.
  /// The SSN of the second owner of the account.
  /// </summary>
  [JsonPropertyName("secondPayeeSsn")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = SecondPayeeSsn_MaxLength, Optional = true)]
  public string SecondPayeeSsn
  {
    get => secondPayeeSsn;
    set => secondPayeeSsn = value != null
      ? TrimEnd(Substring(value, 1, SecondPayeeSsn_MaxLength)) : null;
  }

  /// <summary>Length of the MSFIDM_INDICATOR attribute.</summary>
  public const int MsfidmIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the MSFIDM_INDICATOR attribute.
  /// If the record is supplied by OCSE through the Multi-State Financial 
  /// Institution Data Match (MSFIDM) via the Federal Case Registry(FCR) this
  /// field will contain a &quot;Y&quot;, otherwise the field will be blank.
  /// </summary>
  [JsonPropertyName("msfidmIndicator")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = MsfidmIndicator_MaxLength, Optional = true)]
  public string MsfidmIndicator
  {
    get => msfidmIndicator;
    set => msfidmIndicator = value != null
      ? TrimEnd(Substring(value, 1, MsfidmIndicator_MaxLength)) : null;
  }

  /// <summary>Length of the INSTITUTION_NAME attribute.</summary>
  public const int InstitutionName_MaxLength = 40;

  /// <summary>
  /// The value of the INSTITUTION_NAME attribute.
  /// The name of the institution whose TIN appears in the Institution_TIN 
  /// field. This is also the name to be used by the State for proper levy
  /// processing. This is especially important for mutual funds.
  /// </summary>
  [JsonPropertyName("institutionName")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = InstitutionName_MaxLength, Optional = true)]
  public string InstitutionName
  {
    get => institutionName;
    set => institutionName = value != null
      ? TrimEnd(Substring(value, 1, InstitutionName_MaxLength)) : null;
  }

  /// <summary>Length of the INSTITUTION_STREET_ADDRESS attribute.</summary>
  public const int InstitutionStreetAddress_MaxLength = 40;

  /// <summary>
  /// The value of the INSTITUTION_STREET_ADDRESS attribute.
  /// The address of the institution that is authorized to receive a State levy 
  /// served upon the institution.
  /// </summary>
  [JsonPropertyName("institutionStreetAddress")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = InstitutionStreetAddress_MaxLength, Optional = true)]
  public string InstitutionStreetAddress
  {
    get => institutionStreetAddress;
    set => institutionStreetAddress = value != null
      ? TrimEnd(Substring(value, 1, InstitutionStreetAddress_MaxLength)) : null
      ;
  }

  /// <summary>Length of the INSTITUTION_CITY attribute.</summary>
  public const int InstitutionCity_MaxLength = 30;

  /// <summary>
  /// The value of the INSTITUTION_CITY attribute.
  /// The city of the financial institution.
  /// </summary>
  [JsonPropertyName("institutionCity")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = InstitutionCity_MaxLength, Optional = true)]
  public string InstitutionCity
  {
    get => institutionCity;
    set => institutionCity = value != null
      ? TrimEnd(Substring(value, 1, InstitutionCity_MaxLength)) : null;
  }

  /// <summary>Length of the INSTITUTION_STATE attribute.</summary>
  public const int InstitutionState_MaxLength = 2;

  /// <summary>
  /// The value of the INSTITUTION_STATE attribute.
  /// The state of the financial institution.
  /// </summary>
  [JsonPropertyName("institutionState")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = InstitutionState_MaxLength, Optional = true)]
  public string InstitutionState
  {
    get => institutionState;
    set => institutionState = value != null
      ? TrimEnd(Substring(value, 1, InstitutionState_MaxLength)) : null;
  }

  /// <summary>Length of the INSTITUTION_ZIP_CODE attribute.</summary>
  public const int InstitutionZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the INSTITUTION_ZIP_CODE attribute.
  /// The first 5 digits of the financial institution's zip code.
  /// </summary>
  [JsonPropertyName("institutionZipCode")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = InstitutionZipCode_MaxLength, Optional = true)]
  public string InstitutionZipCode
  {
    get => institutionZipCode;
    set => institutionZipCode = value != null
      ? TrimEnd(Substring(value, 1, InstitutionZipCode_MaxLength)) : null;
  }

  /// <summary>Length of the INSTITUTION_ZIP4 attribute.</summary>
  public const int InstitutionZip4_MaxLength = 4;

  /// <summary>
  /// The value of the INSTITUTION_ZIP4 attribute.
  /// The second 4 digits of the financial institution's zip code
  /// </summary>
  [JsonPropertyName("institutionZip4")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = InstitutionZip4_MaxLength, Optional = true)]
  public string InstitutionZip4
  {
    get => institutionZip4;
    set => institutionZip4 = value != null
      ? TrimEnd(Substring(value, 1, InstitutionZip4_MaxLength)) : null;
  }

  /// <summary>Length of the INSTITUTION_ZIP3 attribute.</summary>
  public const int InstitutionZip3_MaxLength = 3;

  /// <summary>
  /// The value of the INSTITUTION_ZIP3 attribute.
  /// The last 3 digits of the financial institution's zip code.
  /// </summary>
  [JsonPropertyName("institutionZip3")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = InstitutionZip3_MaxLength, Optional = true)]
  public string InstitutionZip3
  {
    get => institutionZip3;
    set => institutionZip3 = value != null
      ? TrimEnd(Substring(value, 1, InstitutionZip3_MaxLength)) : null;
  }

  /// <summary>Length of the SECOND_INSTITUTION_NAME attribute.</summary>
  public const int SecondInstitutionName_MaxLength = 40;

  /// <summary>
  /// The value of the SECOND_INSTITUTION_NAME attribute.
  /// The continuation of the Institution Name or the Transfer Agent name. If 
  /// the Transfer Agent Indicator from the file contains a &quot;0&quot;
  /// signifying there is no Transfer Agent, this field may be used to continue
  /// the Institution Name.  If the Transfer Agent Indicator from the file
  /// contains a &quot;1&quot;, this field may contain the name of the Transfer
  /// Agent.
  /// </summary>
  [JsonPropertyName("secondInstitutionName")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = SecondInstitutionName_MaxLength, Optional = true)]
  public string SecondInstitutionName
  {
    get => secondInstitutionName;
    set => secondInstitutionName = value != null
      ? TrimEnd(Substring(value, 1, SecondInstitutionName_MaxLength)) : null;
  }

  /// <summary>Length of the TRANSMITTER_TIN attribute.</summary>
  public const int TransmitterTin_MaxLength = 9;

  /// <summary>
  /// The value of the TRANSMITTER_TIN attribute.
  /// The valid nine-digit Taxpayer Identification Number assigned to the 
  /// Reporting Agent/Transmitter filing the report.
  /// </summary>
  [JsonPropertyName("transmitterTin")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = TransmitterTin_MaxLength, Optional = true)]
  public string TransmitterTin
  {
    get => transmitterTin;
    set => transmitterTin = value != null
      ? TrimEnd(Substring(value, 1, TransmitterTin_MaxLength)) : null;
  }

  /// <summary>Length of the TRANSMITTER_NAME attribute.</summary>
  public const int TransmitterName_MaxLength = 71;

  /// <summary>
  /// The value of the TRANSMITTER_NAME attribute.
  /// The name of the Reporting Agent/Transmitter.
  /// </summary>
  [JsonPropertyName("transmitterName")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = TransmitterName_MaxLength, Optional = true)]
  public string TransmitterName
  {
    get => transmitterName;
    set => transmitterName = value != null
      ? TrimEnd(Substring(value, 1, TransmitterName_MaxLength)) : null;
  }

  /// <summary>Length of the TRANSMITTER_STREET_ADDRESS attribute.</summary>
  public const int TransmitterStreetAddress_MaxLength = 40;

  /// <summary>
  /// The value of the TRANSMITTER_STREET_ADDRESS attribute.
  /// The address of the Reporting Agent/Transmitter.
  /// </summary>
  [JsonPropertyName("transmitterStreetAddress")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = TransmitterStreetAddress_MaxLength, Optional = true)]
  public string TransmitterStreetAddress
  {
    get => transmitterStreetAddress;
    set => transmitterStreetAddress = value != null
      ? TrimEnd(Substring(value, 1, TransmitterStreetAddress_MaxLength)) : null
      ;
  }

  /// <summary>Length of the TRANSMITTER_CITY attribute.</summary>
  public const int TransmitterCity_MaxLength = 30;

  /// <summary>
  /// The value of the TRANSMITTER_CITY attribute.
  /// The city of the Reporting Agent/Transmitter.
  /// </summary>
  [JsonPropertyName("transmitterCity")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = TransmitterCity_MaxLength, Optional = true)]
  public string TransmitterCity
  {
    get => transmitterCity;
    set => transmitterCity = value != null
      ? TrimEnd(Substring(value, 1, TransmitterCity_MaxLength)) : null;
  }

  /// <summary>Length of the TRANSMITTER_STATE attribute.</summary>
  public const int TransmitterState_MaxLength = 2;

  /// <summary>
  /// The value of the TRANSMITTER_STATE attribute.
  /// The state of the Reporting Agent/Transmitter.
  /// </summary>
  [JsonPropertyName("transmitterState")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = TransmitterState_MaxLength, Optional = true)]
  public string TransmitterState
  {
    get => transmitterState;
    set => transmitterState = value != null
      ? TrimEnd(Substring(value, 1, TransmitterState_MaxLength)) : null;
  }

  /// <summary>Length of the TRANSMITTER_ZIP_CODE attribute.</summary>
  public const int TransmitterZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the TRANSMITTER_ZIP_CODE attribute.
  /// The first 5 digits of the Reporting Agent/Transmitter's zid code.
  /// </summary>
  [JsonPropertyName("transmitterZipCode")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = TransmitterZipCode_MaxLength, Optional = true)]
  public string TransmitterZipCode
  {
    get => transmitterZipCode;
    set => transmitterZipCode = value != null
      ? TrimEnd(Substring(value, 1, TransmitterZipCode_MaxLength)) : null;
  }

  /// <summary>Length of the TRANSMITTER_ZIP4 attribute.</summary>
  public const int TransmitterZip4_MaxLength = 4;

  /// <summary>
  /// The value of the TRANSMITTER_ZIP4 attribute.
  /// The second 4 digits of the Reporting Agent/Transmitter's zip code.
  /// </summary>
  [JsonPropertyName("transmitterZip4")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = TransmitterZip4_MaxLength, Optional = true)]
  public string TransmitterZip4
  {
    get => transmitterZip4;
    set => transmitterZip4 = value != null
      ? TrimEnd(Substring(value, 1, TransmitterZip4_MaxLength)) : null;
  }

  /// <summary>Length of the TRANSMITTER_ZIP3 attribute.</summary>
  public const int TransmitterZip3_MaxLength = 3;

  /// <summary>
  /// The value of the TRANSMITTER_ZIP3 attribute.
  /// The last 3 digits of the Reporting Agent/Transmitter's zip code.
  /// </summary>
  [JsonPropertyName("transmitterZip3")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = TransmitterZip3_MaxLength, Optional = true)]
  public string TransmitterZip3
  {
    get => transmitterZip3;
    set => transmitterZip3 = value != null
      ? TrimEnd(Substring(value, 1, TransmitterZip3_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person or program that created the occurrance of the 
  /// entity.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 42, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The date and time that the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 43, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the ACCOUNT_STATUS_INDICATOR attribute.</summary>
  public const int AccountStatusIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the ACCOUNT_STATUS_INDICATOR attribute.
  /// Status of account at the time of the match.
  /// &quot;0&quot; =
  /// Open at time of match
  /// 
  /// &quot;1&quot; = Closed at time of match
  /// 
  /// &quot;2&quot; or space = Status not provided
  /// </summary>
  [JsonPropertyName("accountStatusIndicator")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = AccountStatusIndicator_MaxLength, Optional = true)]
  public string AccountStatusIndicator
  {
    get => accountStatusIndicator;
    set => accountStatusIndicator = value != null
      ? TrimEnd(Substring(value, 1, AccountStatusIndicator_MaxLength)) : null;
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 65;

  /// <summary>
  /// The value of the NOTE attribute.
  /// Allow worker to add a note on the FIDL screen
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 45, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => note;
    set => note = value != null ? Substring(value, 1, Note_MaxLength) : null;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
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
  [Member(Index = 47, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  private string csePersonNumber;
  private string institutionTin;
  private string matchedPayeeAccountNumber;
  private string matchRunDate;
  private string matchedPayeeSsn;
  private string matchedPayeeName;
  private string matchedPayeeStreetAddress;
  private string matchedPayeeCity;
  private string matchedPayeeState;
  private string matchedPayeeZipCode;
  private string matchedPayeeZip4;
  private string matchedPayeeZip3;
  private string payeeForeignCountryIndicator;
  private string matchFlag;
  private decimal? accountBalance;
  private string accountType;
  private string trustFundIndicator;
  private string accountBalanceIndicator;
  private DateTime? dateOfBirth;
  private string payeeIndicator;
  private string accountFullLegalTitle;
  private string primarySsn;
  private string secondPayeeName;
  private string secondPayeeSsn;
  private string msfidmIndicator;
  private string institutionName;
  private string institutionStreetAddress;
  private string institutionCity;
  private string institutionState;
  private string institutionZipCode;
  private string institutionZip4;
  private string institutionZip3;
  private string secondInstitutionName;
  private string transmitterTin;
  private string transmitterName;
  private string transmitterStreetAddress;
  private string transmitterCity;
  private string transmitterState;
  private string transmitterZipCode;
  private string transmitterZip4;
  private string transmitterZip3;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string accountStatusIndicator;
  private string note;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
}
