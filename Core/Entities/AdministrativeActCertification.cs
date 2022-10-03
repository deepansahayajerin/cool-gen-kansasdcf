// The source file: ADMINISTRATIVE_ACT_CERTIFICATION, ID: 371430209, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// An administrative action taken to enforce an obligation.
/// Examples:  FDSO, SDSO, Credit Reporting, Referral to a Collection Agency, 
/// IRS Full Collection Service, Kansas Most Wanted.
/// </summary>
[Serializable]
public partial class AdministrativeActCertification: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AdministrativeActCertification()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AdministrativeActCertification(AdministrativeActCertification that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AdministrativeActCertification Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the TANF_CODE attribute.</summary>
  public const int TanfCode_MaxLength = 1;

  /// <summary>
  /// The value of the TANF_CODE attribute.
  /// Code used to identify TANF or non-TANF.                                 T 
  /// - TANF
  /// 
  /// N - Non-TANF
  /// 
  /// Space - Not Seperated by TANF (Default)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = TanfCode_MaxLength)]
  public string TanfCode
  {
    get => Get<string>("tanfCode") ?? "";
    set => Set(
      "tanfCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TanfCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the TanfCode attribute.</summary>
  [JsonPropertyName("tanfCode")]
  [Computed]
  public string TanfCode_Json
  {
    get => NullIf(TanfCode, "");
    set => TanfCode = value;
  }

  /// <summary>
  /// The value of the DATE_SENT attribute.
  /// This attribute specifies the date on which the certification referral was 
  /// sent (written to tape.)
  /// For instance, the system may create a Collection Agency Referral 
  /// automatically through the batch procedure. But the referral is not sent
  /// immediately to the collection agency. The referral are reviewed by the CSE
  /// worker before they are sent on tape to collection agency. This attribute
  /// helps in identifying whether or not the referral has been sent to the
  /// agency.
  /// </summary>
  [JsonPropertyName("dateSent")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? DateSent
  {
    get => Get<DateTime?>("dateSent");
    set => Set("dateSent", value);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 4;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of certified enforcement action taken.
  /// They can be: FDSO
  ///              SDSO
  ///              CRED
  ///              RECA
  ///              IRS
  ///              KSMW
  /// 
  /// KDWP
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("KDWP")]
  [Value("KDMV")]
  [Value("FDSO")]
  [Value("IRSC")]
  [Value("COAG")]
  [Value("CRED")]
  [Value("SDSO")]
  [Value("KSMW")]
  public string Type1
  {
    get => Get<string>("type1") ?? "";
    set => Set(
      "type1", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Type1_MaxLength)));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>
  /// The value of the TAKEN_DATE attribute.
  /// This is the date the enforcement action is taken for a particular 
  /// Obligation.
  /// </summary>
  [JsonPropertyName("takenDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? TakenDate
  {
    get => Get<DateTime?>("takenDate");
    set => Set("takenDate", value);
  }

  /// <summary>
  /// The value of the ORIGINAL_AMOUNT attribute.
  /// The original amount submitted for certification.
  /// </summary>
  [JsonPropertyName("originalAmount")]
  [Member(Index = 5, Type = MemberType.Number, Length = 10, Precision = 2, Optional
    = true)]
  public decimal? OriginalAmount
  {
    get => Get<decimal?>("originalAmount");
    set => Set("originalAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CURRENT_AMOUNT attribute.
  /// The current amount that is certified for collection.
  /// </summary>
  [JsonPropertyName("currentAmount")]
  [Member(Index = 6, Type = MemberType.Number, Length = 10, Precision = 2, Optional
    = true)]
  public decimal? CurrentAmount
  {
    get => Get<decimal?>("currentAmount");
    set => Set("currentAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CURRENT_AMOUNT_DATE attribute.
  /// The date the current amount for collection is certified.
  /// </summary>
  [JsonPropertyName("currentAmountDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? CurrentAmountDate
  {
    get => Get<DateTime?>("currentAmountDate");
    set => Set("currentAmountDate", value);
  }

  /// <summary>
  /// The value of the DECERTIFIED_DATE attribute.
  /// The date the certification is cancelled or removed.
  /// </summary>
  [JsonPropertyName("decertifiedDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? DecertifiedDate
  {
    get => Get<DateTime?>("decertifiedDate");
    set => Set("decertifiedDate", value);
  }

  /// <summary>
  /// The value of the NOTIFICATION_DATE attribute.
  /// The date the obligor is notified of the action.
  /// </summary>
  [JsonPropertyName("notificationDate")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? NotificationDate
  {
    get => Get<DateTime?>("notificationDate");
    set => Set("notificationDate", value);
  }

  /// <summary>Length of the NOTIFIED_BY attribute.</summary>
  public const int NotifiedBy_MaxLength = 33;

  /// <summary>
  /// The value of the NOTIFIED_BY attribute.
  /// The name of the agency sending the notice.
  /// </summary>
  [JsonPropertyName("notifiedBy")]
  [Member(Index = 10, Type = MemberType.Char, Length = NotifiedBy_MaxLength, Optional
    = true)]
  public string NotifiedBy
  {
    get => Get<string>("notifiedBy");
    set =>
      Set("notifiedBy", TrimEnd(Substring(value, 1, NotifiedBy_MaxLength)));
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 12, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => Get<DateTime?>("createdTstamp");
    set => Set("createdTstamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person that last updated the occurrance of the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 14, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => Get<DateTime?>("lastUpdatedTstamp");
    set => Set("lastUpdatedTstamp", value);
  }

  /// <summary>Length of the CHANGE_SSN_IND attribute.</summary>
  public const int ChangeSsnInd_MaxLength = 1;

  /// <summary>
  /// The value of the CHANGE_SSN_IND attribute.
  /// Attribute will be used to signal when an obligors SSN has changed from the
  /// previous FDSO cetification.
  /// </summary>
  [JsonPropertyName("changeSsnInd")]
  [Member(Index = 15, Type = MemberType.Char, Length = ChangeSsnInd_MaxLength, Optional
    = true)]
  public string ChangeSsnInd
  {
    get => Get<string>("changeSsnInd");
    set => Set(
      "changeSsnInd", TrimEnd(Substring(value, 1, ChangeSsnInd_MaxLength)));
  }

  /// <summary>Length of the ETYPE_ADM_BANKRUPT attribute.</summary>
  public const int EtypeAdmBankrupt_MaxLength = 1;

  /// <summary>
  /// The value of the ETYPE_ADM_BANKRUPT attribute.
  /// Indicates if there is an active bankruptcy filed after October 17, 2005.
  /// </summary>
  [JsonPropertyName("etypeAdmBankrupt")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = EtypeAdmBankrupt_MaxLength, Optional = true)]
  public string EtypeAdmBankrupt
  {
    get => Get<string>("etypeAdmBankrupt");
    set => Set(
      "etypeAdmBankrupt",
      TrimEnd(Substring(value, 1, EtypeAdmBankrupt_MaxLength)));
  }

  /// <summary>Length of the DECERTIFICATION_REASON attribute.</summary>
  public const int DecertificationReason_MaxLength = 240;

  /// <summary>
  /// The value of the DECERTIFICATION_REASON attribute.
  /// The reason the certification is removed or cancelled.
  /// </summary>
  [JsonPropertyName("decertificationReason")]
  [Member(Index = 17, Type = MemberType.Varchar, Length
    = DecertificationReason_MaxLength, Optional = true)]
  public string DecertificationReason
  {
    get => Get<string>("decertificationReason");
    set => Set(
      "decertificationReason", Substring(value, 1,
      DecertificationReason_MaxLength));
  }

  /// <summary>
  /// The value of the PREVIOUS_AMOUNT attribute.
  /// Previous amount of debt owed.
  /// </summary>
  [JsonPropertyName("previousAmount")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? PreviousAmount
  {
    get => Get<decimal?>("previousAmount");
    set => Set("previousAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the MINIMUM_AMOUNT attribute.
  /// The minimum amount used to determine if this record is to be sent the 
  /// Kansas Dept. of Wildlife and Parks.
  /// </summary>
  [JsonPropertyName("minimumAmount")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? MinimumAmount
  {
    get => Get<decimal?>("minimumAmount");
    set => Set("minimumAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the NUMBER_OF_COURT_ORDERS attribute.
  /// The total number of court orders that qualified to have an AP's drivers 
  /// license restricted.
  /// </summary>
  [JsonPropertyName("numberOfCourtOrders")]
  [Member(Index = 20, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? NumberOfCourtOrders
  {
    get => Get<int?>("numberOfCourtOrders");
    set => Set("numberOfCourtOrders", value);
  }

  /// <summary>
  /// The value of the LOWEST_COURT_ORDER_AMOUNT attribute.
  /// The lowest amount that an AP owed for a court order when the driver's 
  /// license was restricted.
  /// </summary>
  [JsonPropertyName("lowestCourtOrderAmount")]
  [Member(Index = 21, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? LowestCourtOrderAmount
  {
    get => Get<decimal?>("lowestCourtOrderAmount");
    set => Set("lowestCourtOrderAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the HIGHEST_COURT_ORDER_AMOUNT attribute.
  /// Highest amount that an AP owed for a court order when the driver's license
  /// was restricted.
  /// </summary>
  [JsonPropertyName("highestCourtOrderAmount")]
  [Member(Index = 22, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? HighestCourtOrderAmount
  {
    get => Get<decimal?>("highestCourtOrderAmount");
    set => Set("highestCourtOrderAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the ADC_AMOUNT attribute.
  /// The amount of debt for ADC.
  /// </summary>
  [JsonPropertyName("adcAmount")]
  [Member(Index = 23, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AdcAmount
  {
    get => Get<decimal?>("adcAmount");
    set => Set("adcAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the NON_ADC_AMOUNT attribute.
  /// The amount of debt for Non-ADC.
  /// </summary>
  [JsonPropertyName("nonAdcAmount")]
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? NonAdcAmount
  {
    get => Get<decimal?>("nonAdcAmount");
    set => Set("nonAdcAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the INJURED_SPOUSE_DATE attribute.
  /// This attribute specifies the Date on which the Injured Spouse Claim was 
  /// filed. Currently this attribute is not being used either by Legal or
  /// Finance.
  /// </summary>
  [JsonPropertyName("injuredSpouseDate")]
  [Member(Index = 25, Type = MemberType.Date, Optional = true)]
  public DateTime? InjuredSpouseDate
  {
    get => Get<DateTime?>("injuredSpouseDate");
    set => Set("injuredSpouseDate", value);
  }

  /// <summary>Length of the LOCAL_CODE attribute.</summary>
  public const int LocalCode_MaxLength = 3;

  /// <summary>
  /// The value of the LOCAL_CODE attribute.
  /// Must be 3 digit FIPS (federal Identification Processing Standards) local 
  /// code which will be used in the obligor submission.
  /// </summary>
  [JsonPropertyName("localCode")]
  [Member(Index = 26, Type = MemberType.Char, Length = LocalCode_MaxLength, Optional
    = true)]
  public string LocalCode
  {
    get => Get<string>("localCode");
    set => Set("localCode", TrimEnd(Substring(value, 1, LocalCode_MaxLength)));
  }

  /// <summary>
  /// The value of the SSN attribute.
  /// Must be valid obligor social security number.
  /// </summary>
  [JsonPropertyName("ssn")]
  [DefaultValue(0)]
  [Member(Index = 27, Type = MemberType.Number, Length = 9)]
  public int Ssn
  {
    get => Get<int?>("ssn") ?? 0;
    set => Set("ssn", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 15;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// State CSE person case number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length = CaseNumber_MaxLength)]
  public string CaseNumber
  {
    get => Get<string>("caseNumber") ?? "";
    set => Set(
      "caseNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CaseNumber_MaxLength)));
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
  /// Last name associated with the obligor's SSN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length = LastName_MaxLength)]
  public string LastName
  {
    get => Get<string>("lastName") ?? "";
    set => Set(
      "lastName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LastName_MaxLength)));
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
  /// The first name associated with the obligor's SSN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length = FirstName_MaxLength)]
  public string FirstName
  {
    get => Get<string>("firstName") ?? "";
    set => Set(
      "firstName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, FirstName_MaxLength)));
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
  /// Amount owed by the obligor. It is a whole number, with no deciamal, no 
  /// comma, no sign.
  /// </summary>
  [JsonPropertyName("amountOwed")]
  [DefaultValue(0)]
  [Member(Index = 31, Type = MemberType.Number, Length = 8)]
  public int AmountOwed
  {
    get => Get<int?>("amountOwed") ?? 0;
    set => Set("amountOwed", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the CASE_TYPE attribute.</summary>
  public const int CaseType_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_TYPE attribute.
  /// The type of case for the obligor.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length = CaseType_MaxLength)]
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

  /// <summary>Length of the TRANSFER_STATE attribute.</summary>
  public const int TransferState_MaxLength = 2;

  /// <summary>
  /// The value of the TRANSFER_STATE attribute.
  /// Must be constant throughout the file. Required when the state submits a 
  /// transfer. Required when transfer state updates a case. The FIPS (Federal
  /// Identification Processing Standards) state code.
  /// </summary>
  [JsonPropertyName("transferState")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = TransferState_MaxLength, Optional = true)]
  public string TransferState
  {
    get => Get<string>("transferState");
    set => Set(
      "transferState", TrimEnd(Substring(value, 1, TransferState_MaxLength)));
  }

  /// <summary>
  /// The value of the LOCAL_FOR_TRANSFER attribute.
  /// The FIPS (Federal Identification Processing Standards) local code used 
  /// when state submits a transfer. When used it must be 3 digit numeric.
  /// </summary>
  [JsonPropertyName("localForTransfer")]
  [Member(Index = 34, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? LocalForTransfer
  {
    get => Get<int?>("localForTransfer");
    set => Set("localForTransfer", value);
  }

  /// <summary>
  /// The value of the PROCESS_YEAR attribute.
  /// The year tax refund/administrative payment was offset. This is required 
  /// only for reporting state payment, yyyy.
  /// </summary>
  [JsonPropertyName("processYear")]
  [Member(Index = 35, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? ProcessYear
  {
    get => Get<int?>("processYear");
    set => Set("processYear", value);
  }

  /// <summary>Length of the TTYPE_A_ADD_NEW_CASE attribute.</summary>
  public const int TtypeAAddNewCase_MaxLength = 1;

  /// <summary>
  /// The value of the TTYPE_A_ADD_NEW_CASE attribute.
  /// Transaction type for Federal Debt Setoff
  /// A - Add new case
  /// 
  /// Space - N/A
  /// </summary>
  [JsonPropertyName("ttypeAAddNewCase")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = TtypeAAddNewCase_MaxLength, Optional = true)]
  public string TtypeAAddNewCase
  {
    get => Get<string>("ttypeAAddNewCase");
    set => Set(
      "ttypeAAddNewCase",
      TrimEnd(Substring(value, 1, TtypeAAddNewCase_MaxLength)));
  }

  /// <summary>Length of the TTYPE_D_DELETE_CERTIFICATION attribute.</summary>
  public const int TtypeDDeleteCertification_MaxLength = 1;

  /// <summary>
  /// The value of the TTYPE_D_DELETE_CERTIFICATION attribute.
  /// Transaction type for Federal Debt 
  /// Setoff
  /// 
  /// D - Delete federal debt setoff
  /// certification
  /// Space
  /// - N/A
  /// </summary>
  [JsonPropertyName("ttypeDDeleteCertification")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = TtypeDDeleteCertification_MaxLength, Optional = true)]
  public string TtypeDDeleteCertification
  {
    get => Get<string>("ttypeDDeleteCertification");
    set => Set(
      "ttypeDDeleteCertification", TrimEnd(Substring(value, 1,
      TtypeDDeleteCertification_MaxLength)));
  }

  /// <summary>Length of the TTYPE_L_CHANGE_SUBMITTING_STATE attribute.
  /// </summary>
  public const int TtypeLChangeSubmittingState_MaxLength = 1;

  /// <summary>
  /// The value of the TTYPE_L_CHANGE_SUBMITTING_STATE attribute.
  /// Transaction type for Federal Debt Setoff
  /// L - Change
  /// submitting state local
  /// 
  /// Space - N/A
  /// </summary>
  [JsonPropertyName("ttypeLChangeSubmittingState")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = TtypeLChangeSubmittingState_MaxLength, Optional = true)]
  public string TtypeLChangeSubmittingState
  {
    get => Get<string>("ttypeLChangeSubmittingState");
    set => Set(
      "ttypeLChangeSubmittingState", TrimEnd(Substring(value, 1,
      TtypeLChangeSubmittingState_MaxLength)));
  }

  /// <summary>Length of the TTYPE_M_MODIFY_AMOUNT attribute.</summary>
  public const int TtypeMModifyAmount_MaxLength = 1;

  /// <summary>
  /// The value of the TTYPE_M_MODIFY_AMOUNT attribute.
  /// Transaction type for Federal Debt Setoff
  /// 
  /// M - Modify (decrease or increase) amount
  /// of certification       Space - N/A
  /// </summary>
  [JsonPropertyName("ttypeMModifyAmount")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = TtypeMModifyAmount_MaxLength, Optional = true)]
  public string TtypeMModifyAmount
  {
    get => Get<string>("ttypeMModifyAmount");
    set => Set(
      "ttypeMModifyAmount", TrimEnd(Substring(value, 1,
      TtypeMModifyAmount_MaxLength)));
  }

  /// <summary>Length of the TTYPE_R_MODIFY_EXCLUSION attribute.</summary>
  public const int TtypeRModifyExclusion_MaxLength = 1;

  /// <summary>
  /// The value of the TTYPE_R_MODIFY_EXCLUSION attribute.
  /// Transaction type for Federal Debt Setoff
  /// 
  /// R - Replace federal debt offset exclusion
  /// indicator type             Space - N/A
  /// </summary>
  [JsonPropertyName("ttypeRModifyExclusion")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = TtypeRModifyExclusion_MaxLength, Optional = true)]
  public string TtypeRModifyExclusion
  {
    get => Get<string>("ttypeRModifyExclusion");
    set => Set(
      "ttypeRModifyExclusion", TrimEnd(Substring(value, 1,
      TtypeRModifyExclusion_MaxLength)));
  }

  /// <summary>Length of the TTYPE_S_STATE_PAYMENT attribute.</summary>
  public const int TtypeSStatePayment_MaxLength = 1;

  /// <summary>
  /// The value of the TTYPE_S_STATE_PAYMENT attribute.
  /// Transaction type for Federal Debt Setoff
  /// S - State payment
  /// 
  /// Space - N/A
  /// </summary>
  [JsonPropertyName("ttypeSStatePayment")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = TtypeSStatePayment_MaxLength, Optional = true)]
  public string TtypeSStatePayment
  {
    get => Get<string>("ttypeSStatePayment");
    set => Set(
      "ttypeSStatePayment", TrimEnd(Substring(value, 1,
      TtypeSStatePayment_MaxLength)));
  }

  /// <summary>Length of the TTYPE_T_TRANSFER_ADMIN_REVIEW attribute.</summary>
  public const int TtypeTTransferAdminReview_MaxLength = 1;

  /// <summary>
  /// The value of the TTYPE_T_TRANSFER_ADMIN_REVIEW attribute.
  /// Transaction type for Federal Debt 
  /// Setoff
  /// 
  /// T - Transfer for administrative review
  /// to state with the order         Space -
  /// N/A
  /// </summary>
  [JsonPropertyName("ttypeTTransferAdminReview")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = TtypeTTransferAdminReview_MaxLength, Optional = true)]
  public string TtypeTTransferAdminReview
  {
    get => Get<string>("ttypeTTransferAdminReview");
    set => Set(
      "ttypeTTransferAdminReview", TrimEnd(Substring(value, 1,
      TtypeTTransferAdminReview_MaxLength)));
  }

  /// <summary>Length of the ETYPE_ADMINISTRATIVE_OFFSET attribute.</summary>
  public const int EtypeAdministrativeOffset_MaxLength = 1;

  /// <summary>
  /// The value of the ETYPE_ADMINISTRATIVE_OFFSET attribute.
  /// Exclusion type for Federal Debt Setoff.
  /// Exclude all administrative
  /// offset.                                                    Space - N/A
  /// 
  /// Non Space - Applicable
  /// </summary>
  [JsonPropertyName("etypeAdministrativeOffset")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = EtypeAdministrativeOffset_MaxLength, Optional = true)]
  public string EtypeAdministrativeOffset
  {
    get => Get<string>("etypeAdministrativeOffset");
    set => Set(
      "etypeAdministrativeOffset", TrimEnd(Substring(value, 1,
      EtypeAdministrativeOffset_MaxLength)));
  }

  /// <summary>Length of the ETYPE_FEDERAL_RETIREMENT attribute.</summary>
  public const int EtypeFederalRetirement_MaxLength = 1;

  /// <summary>
  /// The value of the ETYPE_FEDERAL_RETIREMENT attribute.
  /// Exclusion type for Federal Debt Setoff.
  /// Exclude federal retirement.
  /// Space - N/A
  /// 
  /// Non Space - Applicable
  /// </summary>
  [JsonPropertyName("etypeFederalRetirement")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = EtypeFederalRetirement_MaxLength, Optional = true)]
  public string EtypeFederalRetirement
  {
    get => Get<string>("etypeFederalRetirement");
    set => Set(
      "etypeFederalRetirement", TrimEnd(Substring(value, 1,
      EtypeFederalRetirement_MaxLength)));
  }

  /// <summary>Length of the ETYPE_FEDERAL_SALARY attribute.</summary>
  public const int EtypeFederalSalary_MaxLength = 1;

  /// <summary>
  /// The value of the ETYPE_FEDERAL_SALARY attribute.
  /// Exclusion type for Federal Debt Setoff.
  /// Exclude federal salary.
  /// Space - N/
  /// A
  /// 
  /// Non Space - Applicable
  /// </summary>
  [JsonPropertyName("etypeFederalSalary")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = EtypeFederalSalary_MaxLength, Optional = true)]
  public string EtypeFederalSalary
  {
    get => Get<string>("etypeFederalSalary");
    set => Set(
      "etypeFederalSalary", TrimEnd(Substring(value, 1,
      EtypeFederalSalary_MaxLength)));
  }

  /// <summary>Length of the ETYPE_TAX_REFUND attribute.</summary>
  public const int EtypeTaxRefund_MaxLength = 1;

  /// <summary>
  /// The value of the ETYPE_TAX_REFUND attribute.
  /// Exclusion type for Federal Debt Setoff.
  /// Exclude tax refund offset.
  /// Space - N
  /// /A
  /// 
  /// Non Space - Applicable
  /// </summary>
  [JsonPropertyName("etypeTaxRefund")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = EtypeTaxRefund_MaxLength, Optional = true)]
  public string EtypeTaxRefund
  {
    get => Get<string>("etypeTaxRefund");
    set => Set(
      "etypeTaxRefund", TrimEnd(Substring(value, 1, EtypeTaxRefund_MaxLength)));
      
  }

  /// <summary>Length of the ETYPE_VENDOR_PAYMENT_OR_MISC attribute.</summary>
  public const int EtypeVendorPaymentOrMisc_MaxLength = 1;

  /// <summary>
  /// The value of the ETYPE_VENDOR_PAYMENT_OR_MISC attribute.
  /// Exclusion type for Federal Debt Setoff.
  /// Exclude vendor payment/misc.
  /// Space - N/A
  /// 
  /// Non Space - Applicable
  /// </summary>
  [JsonPropertyName("etypeVendorPaymentOrMisc")]
  [Member(Index = 47, Type = MemberType.Char, Length
    = EtypeVendorPaymentOrMisc_MaxLength, Optional = true)]
  public string EtypeVendorPaymentOrMisc
  {
    get => Get<string>("etypeVendorPaymentOrMisc");
    set => Set(
      "etypeVendorPaymentOrMisc", TrimEnd(Substring(value, 1,
      EtypeVendorPaymentOrMisc_MaxLength)));
  }

  /// <summary>Length of the ETYPE_PASSPORT_DENIAL attribute.</summary>
  public const int EtypePassportDenial_MaxLength = 1;

  /// <summary>
  /// The value of the ETYPE_PASSPORT_DENIAL attribute.
  /// Exclusion type for Federal Debt Setoff.
  /// Exclude passport denial.
  /// Space - N
  /// /A
  /// 
  /// Non Space - Applicable
  /// </summary>
  [JsonPropertyName("etypePassportDenial")]
  [Member(Index = 48, Type = MemberType.Char, Length
    = EtypePassportDenial_MaxLength, Optional = true)]
  public string EtypePassportDenial
  {
    get => Get<string>("etypePassportDenial");
    set => Set(
      "etypePassportDenial", TrimEnd(Substring(value, 1,
      EtypePassportDenial_MaxLength)));
  }

  /// <summary>Length of the ETYPE_FINANCIAL_INSTITUTION attribute.</summary>
  public const int EtypeFinancialInstitution_MaxLength = 1;

  /// <summary>
  /// The value of the ETYPE_FINANCIAL_INSTITUTION attribute.
  /// Exclusion type for Federal Debt Setoff.
  /// Exclude financial institution.
  /// Space - N/A
  /// 
  /// Non Space - Applicable
  /// </summary>
  [JsonPropertyName("etypeFinancialInstitution")]
  [Member(Index = 49, Type = MemberType.Char, Length
    = EtypeFinancialInstitution_MaxLength, Optional = true)]
  public string EtypeFinancialInstitution
  {
    get => Get<string>("etypeFinancialInstitution");
    set => Set(
      "etypeFinancialInstitution", TrimEnd(Substring(value, 1,
      EtypeFinancialInstitution_MaxLength)));
  }

  /// <summary>Length of the RETURN_STATUS attribute.</summary>
  public const int ReturnStatus_MaxLength = 1;

  /// <summary>
  /// The value of the RETURN_STATUS attribute.
  /// The return status is set to indicate the position in the life cycle of the
  /// Administrative Act Certification.
  /// Space - N/A
  /// 
  /// E - Error
  /// 
  /// R - Return Okay
  /// </summary>
  [JsonPropertyName("returnStatus")]
  [Member(Index = 50, Type = MemberType.Char, Length = ReturnStatus_MaxLength, Optional
    = true)]
  public string ReturnStatus
  {
    get => Get<string>("returnStatus");
    set => Set(
      "returnStatus", TrimEnd(Substring(value, 1, ReturnStatus_MaxLength)));
  }

  /// <summary>
  /// The value of the RETURN_STATUS_DATE attribute.
  /// The return status date is set when the return status attribute 
  /// is set, and indicates the date that the status of the
  /// Administrative Act Certification was changed.
  /// Space / 00001-01-01     - N/A
  /// 
  /// 1999-01-01
  /// </summary>
  [JsonPropertyName("returnStatusDate")]
  [Member(Index = 51, Type = MemberType.Date, Optional = true)]
  public DateTime? ReturnStatusDate
  {
    get => Get<DateTime?>("returnStatusDate");
    set => Set("returnStatusDate", value);
  }

  /// <summary>Length of the TTYPE_B_NAME_CHANGE attribute.</summary>
  public const int TtypeBNameChange_MaxLength = 1;

  /// <summary>
  /// The value of the TTYPE_B_NAME_CHANGE attribute.
  /// Transaction type for FDSO:
  /// 
  /// B - Change Obligor Name
  /// 
  /// Space - N/A
  /// </summary>
  [JsonPropertyName("ttypeBNameChange")]
  [Member(Index = 52, Type = MemberType.Char, Length
    = TtypeBNameChange_MaxLength, Optional = true)]
  public string TtypeBNameChange
  {
    get => Get<string>("ttypeBNameChange");
    set => Set(
      "ttypeBNameChange",
      TrimEnd(Substring(value, 1, TtypeBNameChange_MaxLength)));
  }

  /// <summary>Length of the TTYPE_Z_ADDRESS_CHANGE attribute.</summary>
  public const int TtypeZAddressChange_MaxLength = 1;

  /// <summary>
  /// The value of the TTYPE_Z_ADDRESS_CHANGE attribute.
  /// Transaction type for FDSO:
  /// 
  /// Z - Change Obligor Address
  /// 
  /// Space - N/A
  /// </summary>
  [JsonPropertyName("ttypeZAddressChange")]
  [Member(Index = 53, Type = MemberType.Char, Length
    = TtypeZAddressChange_MaxLength, Optional = true)]
  public string TtypeZAddressChange
  {
    get => Get<string>("ttypeZAddressChange");
    set => Set(
      "ttypeZAddressChange", TrimEnd(Substring(value, 1,
      TtypeZAddressChange_MaxLength)));
  }

  /// <summary>Length of the ADDRESS_STREET_1 attribute.</summary>
  public const int AddressStreet1_MaxLength = 30;

  /// <summary>
  /// The value of the ADDRESS_STREET_1 attribute.
  /// First line of postal address
  /// </summary>
  [JsonPropertyName("addressStreet1")]
  [Member(Index = 54, Type = MemberType.Char, Length
    = AddressStreet1_MaxLength, Optional = true)]
  public string AddressStreet1
  {
    get => Get<string>("addressStreet1");
    set => Set(
      "addressStreet1", TrimEnd(Substring(value, 1, AddressStreet1_MaxLength)));
      
  }

  /// <summary>Length of the ADDRESS_STREET_2 attribute.</summary>
  public const int AddressStreet2_MaxLength = 30;

  /// <summary>
  /// The value of the ADDRESS_STREET_2 attribute.
  /// Second line of postal address
  /// </summary>
  [JsonPropertyName("addressStreet2")]
  [Member(Index = 55, Type = MemberType.Char, Length
    = AddressStreet2_MaxLength, Optional = true)]
  public string AddressStreet2
  {
    get => Get<string>("addressStreet2");
    set => Set(
      "addressStreet2", TrimEnd(Substring(value, 1, AddressStreet2_MaxLength)));
      
  }

  /// <summary>Length of the ADDRESS_CITY attribute.</summary>
  public const int AddressCity_MaxLength = 25;

  /// <summary>
  /// The value of the ADDRESS_CITY attribute.
  /// Community where address is located
  /// </summary>
  [JsonPropertyName("addressCity")]
  [Member(Index = 56, Type = MemberType.Char, Length = AddressCity_MaxLength, Optional
    = true)]
  public string AddressCity
  {
    get => Get<string>("addressCity");
    set => Set(
      "addressCity", TrimEnd(Substring(value, 1, AddressCity_MaxLength)));
  }

  /// <summary>Length of the ADDRESS_STATE attribute.</summary>
  public const int AddressState_MaxLength = 2;

  /// <summary>
  /// The value of the ADDRESS_STATE attribute.
  /// Region in which the address is located
  /// </summary>
  [JsonPropertyName("addressState")]
  [Member(Index = 57, Type = MemberType.Char, Length = AddressState_MaxLength, Optional
    = true)]
  public string AddressState
  {
    get => Get<string>("addressState");
    set => Set(
      "addressState", TrimEnd(Substring(value, 1, AddressState_MaxLength)));
  }

  /// <summary>Length of the ADDRESS_ZIP attribute.</summary>
  public const int AddressZip_MaxLength = 9;

  /// <summary>
  /// The value of the ADDRESS_ZIP attribute.
  /// Zip code + 4 where address is located
  /// </summary>
  [JsonPropertyName("addressZip")]
  [Member(Index = 58, Type = MemberType.Char, Length = AddressZip_MaxLength, Optional
    = true)]
  public string AddressZip
  {
    get => Get<string>("addressZip");
    set =>
      Set("addressZip", TrimEnd(Substring(value, 1, AddressZip_MaxLength)));
  }

  /// <summary>Length of the WITNESS attribute.</summary>
  public const int Witness_MaxLength = 37;

  /// <summary>
  /// The value of the WITNESS attribute.
  /// The name of the witness signing the Kansas Most Wanted Consent Form.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 59, Type = MemberType.Char, Length = Witness_MaxLength)]
  public string Witness
  {
    get => Get<string>("witness") ?? "";
    set => Set(
      "witness", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Witness_MaxLength)));
  }

  /// <summary>
  /// The json value of the Witness attribute.</summary>
  [JsonPropertyName("witness")]
  [Computed]
  public string Witness_Json
  {
    get => NullIf(Witness, "");
    set => Witness = value;
  }

  /// <summary>
  /// The value of the CSE_CENT_OFF_APPROVAL_DATE attribute.
  /// This attributes specifies the date on which the central office approved 
  /// the certification for Kansas Most Wanted action.
  /// </summary>
  [JsonPropertyName("cseCentOffApprovalDate")]
  [Member(Index = 60, Type = MemberType.Date, Optional = true)]
  public DateTime? CseCentOffApprovalDate
  {
    get => Get<DateTime?>("cseCentOffApprovalDate");
    set => Set("cseCentOffApprovalDate", value);
  }

  /// <summary>
  /// The value of the FEE_COLLECTED_DATE attribute.
  /// The date that the fee for IRS Full Collection is received by CSE Central 
  /// Office.
  /// </summary>
  [JsonPropertyName("feeCollectedDate")]
  [Member(Index = 61, Type = MemberType.Date, Optional = true)]
  public DateTime? FeeCollectedDate
  {
    get => Get<DateTime?>("feeCollectedDate");
    set => Set("feeCollectedDate", value);
  }

  /// <summary>
  /// The value of the CSE_CENTRAL_OFFICE_APPROVAL_DATE attribute.
  /// The date CSE Central Office approved the IRS Full Certification to be sent
  /// to the OCSE Regional office.
  /// </summary>
  [JsonPropertyName("cseCentralOfficeApprovalDate")]
  [Member(Index = 62, Type = MemberType.Date, Optional = true)]
  public DateTime? CseCentralOfficeApprovalDate
  {
    get => Get<DateTime?>("cseCentralOfficeApprovalDate");
    set => Set("cseCentralOfficeApprovalDate", value);
  }

  /// <summary>
  /// The value of the OCSE_REGIONAL_APPROVAL_DATE attribute.
  /// The date OCSE Regional Office approved the obligation for IRS Full 
  /// Collection and refer to Washington Central Office OCSE.
  /// </summary>
  [JsonPropertyName("ocseRegionalApprovalDate")]
  [Member(Index = 63, Type = MemberType.Date, Optional = true)]
  public DateTime? OcseRegionalApprovalDate
  {
    get => Get<DateTime?>("ocseRegionalApprovalDate");
    set => Set("ocseRegionalApprovalDate", value);
  }

  /// <summary>
  /// The value of the OCSE_FEDERAL_APPROVAL_DATE attribute.
  /// The date Washington Centroal Office OCSE approved the IRS Full Collection 
  /// to be sent to the IRS.
  /// </summary>
  [JsonPropertyName("ocseFederalApprovalDate")]
  [Member(Index = 64, Type = MemberType.Date, Optional = true)]
  public DateTime? OcseFederalApprovalDate
  {
    get => Get<DateTime?>("ocseFederalApprovalDate");
    set => Set("ocseFederalApprovalDate", value);
  }

  /// <summary>
  /// The value of the DENIAL_DATE attribute.
  /// The date the IRS Full Collection is denied.  The denial can come from any 
  /// of the offices that must approve the IRS Full Collection.
  /// </summary>
  [JsonPropertyName("denialDate")]
  [Member(Index = 65, Type = MemberType.Date, Optional = true)]
  public DateTime? DenialDate
  {
    get => Get<DateTime?>("denialDate");
    set => Set("denialDate", value);
  }

  /// <summary>Length of the DENIAL_REASON attribute.</summary>
  public const int DenialReason_MaxLength = 240;

  /// <summary>
  /// The value of the DENIAL_REASON attribute.
  /// The reason the IRS Full Collection is denied.
  /// </summary>
  [JsonPropertyName("denialReason")]
  [Member(Index = 66, Type = MemberType.Char, Length = DenialReason_MaxLength, Optional
    = true)]
  public string DenialReason
  {
    get => Get<string>("denialReason");
    set => Set(
      "denialReason", TrimEnd(Substring(value, 1, DenialReason_MaxLength)));
  }

  /// <summary>Length of the DENIED_BY attribute.</summary>
  public const int DeniedBy_MaxLength = 37;

  /// <summary>
  /// The value of the DENIED_BY attribute.
  /// The agency that turned down the IRS Full Collection.
  /// </summary>
  [JsonPropertyName("deniedBy")]
  [Member(Index = 67, Type = MemberType.Char, Length = DeniedBy_MaxLength, Optional
    = true)]
  public string DeniedBy
  {
    get => Get<string>("deniedBy");
    set => Set("deniedBy", TrimEnd(Substring(value, 1, DeniedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the AP_RESP_RECEIVED_DATA attribute.
  /// This attribute specifies the date a response was received from the AP for 
  /// the notification we had sent out for Credit Reporting. A response must be
  /// received within 15 days from the notificatino date.
  /// </summary>
  [JsonPropertyName("apRespReceivedData")]
  [Member(Index = 68, Type = MemberType.Date, Optional = true)]
  public DateTime? ApRespReceivedData
  {
    get => Get<DateTime?>("apRespReceivedData");
    set => Set("apRespReceivedData", value);
  }

  /// <summary>
  /// The value of the DATE_STAYED attribute.
  /// This attribute specifies the date the reporting to Credit Reporting Agency
  /// was stayed.
  /// </summary>
  [JsonPropertyName("dateStayed")]
  [Member(Index = 69, Type = MemberType.Date, Optional = true)]
  public DateTime? DateStayed
  {
    get => Get<DateTime?>("dateStayed");
    set => Set("dateStayed", value);
  }

  /// <summary>
  /// The value of the DATE STAY RELEASED attribute.
  /// This attribute specifies the date the stay was released for reporting to 
  /// Credit Reporting Agency.
  /// </summary>
  [JsonPropertyName("dateStayReleased")]
  [Member(Index = 70, Type = MemberType.Date, Optional = true)]
  public DateTime? DateStayReleased
  {
    get => Get<DateTime?>("dateStayReleased");
    set => Set("dateStayReleased", value);
  }

  /// <summary>
  /// The value of the HIGHEST_AMOUNT attribute.
  /// This attribute specifies the highest amount reported to the Credit 
  /// Reporting Agency. This amount will not go down. It can only go up. However
  /// if a D04 record was sent, a new highest amount will be subsequently sent.
  /// </summary>
  [JsonPropertyName("highestAmount")]
  [Member(Index = 71, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? HighestAmount
  {
    get => Get<decimal?>("highestAmount");
    set => Set("highestAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the RECOVERY_AMOUNT attribute.
  /// The amount of debt related to recoveries.
  /// Examples include bad checks, etc.
  /// </summary>
  [JsonPropertyName("recoveryAmount")]
  [Member(Index = 72, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? RecoveryAmount
  {
    get => Get<decimal?>("recoveryAmount");
    set => Set("recoveryAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CHILD_SUPPORT_RELATED_AMOUNT attribute.
  /// The amount of debt owed to a state agency.
  /// </summary>
  [JsonPropertyName("childSupportRelatedAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 73, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal ChildSupportRelatedAmount
  {
    get => Get<decimal?>("childSupportRelatedAmount") ?? 0M;
    set => Set(
      "childSupportRelatedAmount", value == 0M ? null : Truncate
      (value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the REFERRAL_DATE attribute.
  /// The date the obligation is actually given to a collection agency for 
  /// collection.
  /// </summary>
  [JsonPropertyName("referralDate")]
  [Member(Index = 74, Type = MemberType.Date, Optional = true)]
  public DateTime? ReferralDate
  {
    get => Get<DateTime?>("referralDate");
    set => Set("referralDate", value);
  }

  /// <summary>
  /// The value of the WITHDRAW_DATE attribute.
  /// Date the referral was withdrawn from the collection agency.
  /// </summary>
  [JsonPropertyName("withdrawDate")]
  [Member(Index = 75, Type = MemberType.Date, Optional = true)]
  public DateTime? WithdrawDate
  {
    get => Get<DateTime?>("withdrawDate");
    set => Set("withdrawDate", value);
  }

  /// <summary>Length of the REASON_FOR_WITHDRAW attribute.</summary>
  public const int ReasonForWithdraw_MaxLength = 72;

  /// <summary>
  /// The value of the REASON_FOR_WITHDRAW attribute.
  /// The reason given to Central Office by the Collection Officer on why the 
  /// obligation is withdrawn from the collection agency.
  /// </summary>
  [JsonPropertyName("reasonForWithdraw")]
  [Member(Index = 76, Type = MemberType.Char, Length
    = ReasonForWithdraw_MaxLength, Optional = true)]
  public string ReasonForWithdraw
  {
    get => Get<string>("reasonForWithdraw");
    set => Set(
      "reasonForWithdraw", TrimEnd(Substring(value, 1,
      ReasonForWithdraw_MaxLength)));
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 77, Type = MemberType.Char, Length = CpaType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => Get<string>("cpaType") ?? "";
    set => Set(
      "cpaType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CpaType_MaxLength)));
  }

  /// <summary>
  /// The json value of the CpaType attribute.</summary>
  [JsonPropertyName("cpaType")]
  [Computed]
  public string CpaType_Json
  {
    get => NullIf(CpaType, "");
    set => CpaType = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 78, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => Get<string>("cspNumber") ?? "";
    set => Set(
      "cspNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CspNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdGeneratedId")]
  [Member(Index = 79, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? SpdGeneratedId
  {
    get => Get<int?>("spdGeneratedId");
    set => Set("spdGeneratedId", value);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int AatType_MaxLength = 4;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This identifies the type of enforcement that can be taken.
  /// Examples are:
  ///      SDSO
  ///      FDSO
  /// </summary>
  [JsonPropertyName("aatType")]
  [Member(Index = 80, Type = MemberType.Char, Length = AatType_MaxLength, Optional
    = true)]
  public string AatType
  {
    get => Get<string>("aatType");
    set => Set("aatType", TrimEnd(Substring(value, 1, AatType_MaxLength)));
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumberAssign_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumberAssign")]
  [Member(Index = 81, Type = MemberType.Char, Length
    = CspNumberAssign_MaxLength, Optional = true)]
  public string CspNumberAssign
  {
    get => Get<string>("cspNumberAssign");
    set => Set(
      "cspNumberAssign",
      TrimEnd(Substring(value, 1, CspNumberAssign_MaxLength)));
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Unique system generated number that descripts the details of a particular 
  /// vendor.
  /// </summary>
  [JsonPropertyName("venIdentifier")]
  [Member(Index = 82, Type = MemberType.Number, Length = 8, Optional = true)]
  public int? VenIdentifier
  {
    get => Get<int?>("venIdentifier");
    set => Set("venIdentifier", value);
  }
}
