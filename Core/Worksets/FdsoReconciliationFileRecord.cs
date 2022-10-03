﻿// The source file: FDSO_RECONCILIATION_FILE_RECORD, ID: 371018983, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class FdsoReconciliationFileRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FdsoReconciliationFileRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FdsoReconciliationFileRecord(FdsoReconciliationFileRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FdsoReconciliationFileRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FdsoReconciliationFileRecord that)
  {
    base.Assign(that);
    recordIdentifier = that.recordIdentifier;
    submittingState = that.submittingState;
    localCode = that.localCode;
    ssn = that.ssn;
    caseNumber = that.caseNumber;
    lastName = that.lastName;
    firstName = that.firstName;
    currentArrearageAmount = that.currentArrearageAmount;
    caseType = that.caseType;
    preoffsetNoticeDate = that.preoffsetNoticeDate;
    preoffsetHoldInd = that.preoffsetHoldInd;
    etypeAdministrativeOffset = that.etypeAdministrativeOffset;
    etypeFederalRetirement = that.etypeFederalRetirement;
    etypeVendorPaymentOrMisc = that.etypeVendorPaymentOrMisc;
    etypeFederalSalary = that.etypeFederalSalary;
    etypeTaxRefund = that.etypeTaxRefund;
    etypePassportDenial = that.etypePassportDenial;
    etypeFinancialInstitution = that.etypeFinancialInstitution;
  }

  /// <summary>Length of the RECORD_IDENTIFIER attribute.</summary>
  public const int RecordIdentifier_MaxLength = 3;

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

  /// <summary>Length of the SUBMITTING_STATE attribute.</summary>
  public const int SubmittingState_MaxLength = 2;

  /// <summary>
  /// The value of the SUBMITTING_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = SubmittingState_MaxLength)]
    
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
  [Member(Index = 3, Type = MemberType.Char, Length = LocalCode_MaxLength)]
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
  [Member(Index = 4, Type = MemberType.Number, Length = 9)]
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
  [Member(Index = 5, Type = MemberType.Char, Length = CaseNumber_MaxLength)]
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
  [Member(Index = 6, Type = MemberType.Char, Length = LastName_MaxLength)]
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
  [Member(Index = 7, Type = MemberType.Char, Length = FirstName_MaxLength)]
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
  /// The value of the CURRENT_ARREARAGE_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("currentArrearageAmount")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 8)]
  public int CurrentArrearageAmount
  {
    get => currentArrearageAmount;
    set => currentArrearageAmount = value;
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

  /// <summary>Length of the PREOFFSET_NOTICE_DATE attribute.</summary>
  public const int PreoffsetNoticeDate_MaxLength = 8;

  /// <summary>
  /// The value of the PREOFFSET_NOTICE_DATE attribute.
  /// This field will contain the date the most recent Pre-offset notice was 
  /// sent in CCYYMMDD format.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = PreoffsetNoticeDate_MaxLength)]
  public string PreoffsetNoticeDate
  {
    get => preoffsetNoticeDate ?? "";
    set => preoffsetNoticeDate =
      TrimEnd(Substring(value, 1, PreoffsetNoticeDate_MaxLength));
  }

  /// <summary>
  /// The json value of the PreoffsetNoticeDate attribute.</summary>
  [JsonPropertyName("preoffsetNoticeDate")]
  [Computed]
  public string PreoffsetNoticeDate_Json
  {
    get => NullIf(PreoffsetNoticeDate, "");
    set => PreoffsetNoticeDate = value;
  }

  /// <summary>Length of the PREOFFSET_HOLD_IND attribute.</summary>
  public const int PreoffsetHoldInd_MaxLength = 1;

  /// <summary>
  /// The value of the PREOFFSET_HOLD_IND attribute.
  /// This field will contain one of the following values to indicate the case 
  /// is active at FMS :
  /// H - The case is on hold pending Pre-offset Notice hold period.
  /// Space - The case has either been sent to FMS or is FIDM - only.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = PreoffsetHoldInd_MaxLength)]
  public string PreoffsetHoldInd
  {
    get => preoffsetHoldInd ?? "";
    set => preoffsetHoldInd =
      TrimEnd(Substring(value, 1, PreoffsetHoldInd_MaxLength));
  }

  /// <summary>
  /// The json value of the PreoffsetHoldInd attribute.</summary>
  [JsonPropertyName("preoffsetHoldInd")]
  [Computed]
  public string PreoffsetHoldInd_Json
  {
    get => NullIf(PreoffsetHoldInd, "");
    set => PreoffsetHoldInd = value;
  }

  /// <summary>Length of the ETYPE_ADMINISTRATIVE_OFFSET attribute.</summary>
  public const int EtypeAdministrativeOffset_MaxLength = 3;

  /// <summary>
  /// The value of the ETYPE_ADMINISTRATIVE_OFFSET attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = EtypeAdministrativeOffset_MaxLength)]
  public string EtypeAdministrativeOffset
  {
    get => etypeAdministrativeOffset ?? "";
    set => etypeAdministrativeOffset =
      TrimEnd(Substring(value, 1, EtypeAdministrativeOffset_MaxLength));
  }

  /// <summary>
  /// The json value of the EtypeAdministrativeOffset attribute.</summary>
  [JsonPropertyName("etypeAdministrativeOffset")]
  [Computed]
  public string EtypeAdministrativeOffset_Json
  {
    get => NullIf(EtypeAdministrativeOffset, "");
    set => EtypeAdministrativeOffset = value;
  }

  /// <summary>Length of the ETYPE_FEDERAL_RETIREMENT attribute.</summary>
  public const int EtypeFederalRetirement_MaxLength = 3;

  /// <summary>
  /// The value of the ETYPE_FEDERAL_RETIREMENT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = EtypeFederalRetirement_MaxLength)]
  public string EtypeFederalRetirement
  {
    get => etypeFederalRetirement ?? "";
    set => etypeFederalRetirement =
      TrimEnd(Substring(value, 1, EtypeFederalRetirement_MaxLength));
  }

  /// <summary>
  /// The json value of the EtypeFederalRetirement attribute.</summary>
  [JsonPropertyName("etypeFederalRetirement")]
  [Computed]
  public string EtypeFederalRetirement_Json
  {
    get => NullIf(EtypeFederalRetirement, "");
    set => EtypeFederalRetirement = value;
  }

  /// <summary>Length of the ETYPE_VENDOR_PAYMENT_OR_MISC attribute.</summary>
  public const int EtypeVendorPaymentOrMisc_MaxLength = 3;

  /// <summary>
  /// The value of the ETYPE_VENDOR_PAYMENT_OR_MISC attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = EtypeVendorPaymentOrMisc_MaxLength)]
  public string EtypeVendorPaymentOrMisc
  {
    get => etypeVendorPaymentOrMisc ?? "";
    set => etypeVendorPaymentOrMisc =
      TrimEnd(Substring(value, 1, EtypeVendorPaymentOrMisc_MaxLength));
  }

  /// <summary>
  /// The json value of the EtypeVendorPaymentOrMisc attribute.</summary>
  [JsonPropertyName("etypeVendorPaymentOrMisc")]
  [Computed]
  public string EtypeVendorPaymentOrMisc_Json
  {
    get => NullIf(EtypeVendorPaymentOrMisc, "");
    set => EtypeVendorPaymentOrMisc = value;
  }

  /// <summary>Length of the ETYPE_FEDERAL_SALARY attribute.</summary>
  public const int EtypeFederalSalary_MaxLength = 3;

  /// <summary>
  /// The value of the ETYPE_FEDERAL_SALARY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = EtypeFederalSalary_MaxLength)]
  public string EtypeFederalSalary
  {
    get => etypeFederalSalary ?? "";
    set => etypeFederalSalary =
      TrimEnd(Substring(value, 1, EtypeFederalSalary_MaxLength));
  }

  /// <summary>
  /// The json value of the EtypeFederalSalary attribute.</summary>
  [JsonPropertyName("etypeFederalSalary")]
  [Computed]
  public string EtypeFederalSalary_Json
  {
    get => NullIf(EtypeFederalSalary, "");
    set => EtypeFederalSalary = value;
  }

  /// <summary>Length of the ETYPE_TAX_REFUND attribute.</summary>
  public const int EtypeTaxRefund_MaxLength = 3;

  /// <summary>
  /// The value of the ETYPE_TAX_REFUND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = EtypeTaxRefund_MaxLength)]
    
  public string EtypeTaxRefund
  {
    get => etypeTaxRefund ?? "";
    set => etypeTaxRefund =
      TrimEnd(Substring(value, 1, EtypeTaxRefund_MaxLength));
  }

  /// <summary>
  /// The json value of the EtypeTaxRefund attribute.</summary>
  [JsonPropertyName("etypeTaxRefund")]
  [Computed]
  public string EtypeTaxRefund_Json
  {
    get => NullIf(EtypeTaxRefund, "");
    set => EtypeTaxRefund = value;
  }

  /// <summary>Length of the ETYPE_PASSPORT_DENIAL attribute.</summary>
  public const int EtypePassportDenial_MaxLength = 3;

  /// <summary>
  /// The value of the ETYPE_PASSPORT_DENIAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = EtypePassportDenial_MaxLength)]
  public string EtypePassportDenial
  {
    get => etypePassportDenial ?? "";
    set => etypePassportDenial =
      TrimEnd(Substring(value, 1, EtypePassportDenial_MaxLength));
  }

  /// <summary>
  /// The json value of the EtypePassportDenial attribute.</summary>
  [JsonPropertyName("etypePassportDenial")]
  [Computed]
  public string EtypePassportDenial_Json
  {
    get => NullIf(EtypePassportDenial, "");
    set => EtypePassportDenial = value;
  }

  /// <summary>Length of the ETYPE_FINANCIAL_INSTITUTION attribute.</summary>
  public const int EtypeFinancialInstitution_MaxLength = 3;

  /// <summary>
  /// The value of the ETYPE_FINANCIAL_INSTITUTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = EtypeFinancialInstitution_MaxLength)]
  public string EtypeFinancialInstitution
  {
    get => etypeFinancialInstitution ?? "";
    set => etypeFinancialInstitution =
      TrimEnd(Substring(value, 1, EtypeFinancialInstitution_MaxLength));
  }

  /// <summary>
  /// The json value of the EtypeFinancialInstitution attribute.</summary>
  [JsonPropertyName("etypeFinancialInstitution")]
  [Computed]
  public string EtypeFinancialInstitution_Json
  {
    get => NullIf(EtypeFinancialInstitution, "");
    set => EtypeFinancialInstitution = value;
  }

  private string recordIdentifier;
  private string submittingState;
  private string localCode;
  private int ssn;
  private string caseNumber;
  private string lastName;
  private string firstName;
  private int currentArrearageAmount;
  private string caseType;
  private string preoffsetNoticeDate;
  private string preoffsetHoldInd;
  private string etypeAdministrativeOffset;
  private string etypeFederalRetirement;
  private string etypeVendorPaymentOrMisc;
  private string etypeFederalSalary;
  private string etypeTaxRefund;
  private string etypePassportDenial;
  private string etypeFinancialInstitution;
}
