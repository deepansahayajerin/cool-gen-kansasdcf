// The source file: FCR_SVES_TITLE_II, ID: 945065615, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// This Entity Type stores SVES Title II Claims (E05) response information from
/// FCR.
/// </summary>
[Serializable]
public partial class FcrSvesTitleIi: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrSvesTitleIi()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrSvesTitleIi(FcrSvesTitleIi that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrSvesTitleIi Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrSvesTitleIi that)
  {
    base.Assign(that);
    seqNo = that.seqNo;
    canAndBic = that.canAndBic;
    stateCode = that.stateCode;
    countyCode = that.countyCode;
    directDepositIndicator = that.directDepositIndicator;
    lafCode = that.lafCode;
    deferredPaymentDate = that.deferredPaymentDate;
    initialTitleIiEntitlementDt = that.initialTitleIiEntitlementDt;
    currentTitleIiEntitlementDt = that.currentTitleIiEntitlementDt;
    titleIiSuspendTerminateDt = that.titleIiSuspendTerminateDt;
    netMonthlyTitleIiBenefit = that.netMonthlyTitleIiBenefit;
    hiOptionCode = that.hiOptionCode;
    hiStartDate = that.hiStartDate;
    hiStopDate = that.hiStopDate;
    smiOptionCode = that.smiOptionCode;
    smiStartDate = that.smiStartDate;
    smiStopDate = that.smiStopDate;
    categoryOfAssistance = that.categoryOfAssistance;
    blackLungEntitlementCode = that.blackLungEntitlementCode;
    blackLungPaymentAmount = that.blackLungPaymentAmount;
    railroadIndicator = that.railroadIndicator;
    mbcNumberOfEntries = that.mbcNumberOfEntries;
    mbcDate1 = that.mbcDate1;
    mbcAmount1 = that.mbcAmount1;
    mbcType1 = that.mbcType1;
    mbcDate2 = that.mbcDate2;
    mbcAmount2 = that.mbcAmount2;
    mbcType2 = that.mbcType2;
    mbcDate3 = that.mbcDate3;
    mbcAmount3 = that.mbcAmount3;
    mbcType3 = that.mbcType3;
    mbcDate4 = that.mbcDate4;
    mbcAmount4 = that.mbcAmount4;
    mbcType4 = that.mbcType4;
    mbcDate5 = that.mbcDate5;
    mbcAmount5 = that.mbcAmount5;
    mbcType5 = that.mbcType5;
    mbcDate6 = that.mbcDate6;
    mbcAmount6 = that.mbcAmount6;
    mbcType6 = that.mbcType6;
    mbcDate7 = that.mbcDate7;
    mbcAmount7 = that.mbcAmount7;
    mbcType7 = that.mbcType7;
    mbcDate8 = that.mbcDate8;
    mbcAmount8 = that.mbcAmount8;
    mbcType8 = that.mbcType8;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    fcgLSRspAgy = that.fcgLSRspAgy;
    fcgMemberId = that.fcgMemberId;
  }

  /// <summary>
  /// The value of the SEQ_NO attribute.
  /// This field contains a value to identify unique Title II record.
  /// </summary>
  [JsonPropertyName("seqNo")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 2)]
  public int SeqNo
  {
    get => seqNo;
    set => seqNo = value;
  }

  /// <summary>Length of the CAN_AND_BIC attribute.</summary>
  public const int CanAndBic_MaxLength = 12;

  /// <summary>
  /// The value of the CAN_AND_BIC attribute.
  /// This field contains the Claim Account Number (CAN) and the Beneficiary 
  /// Identification Code (BIC).  The BIC is the last three positions of the
  /// field. Refer to Appendix E, Data Dictionary,  for the  list of valid
  /// values and their descriptions.
  /// </summary>
  [JsonPropertyName("canAndBic")]
  [Member(Index = 2, Type = MemberType.Char, Length = CanAndBic_MaxLength, Optional
    = true)]
  public string CanAndBic
  {
    get => canAndBic;
    set => canAndBic = value != null
      ? TrimEnd(Substring(value, 1, CanAndBic_MaxLength)) : null;
  }

  /// <summary>Length of the STATE_CODE attribute.</summary>
  public const int StateCode_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_CODE attribute.
  /// This field contains the two-character FIPS State Code of the State that is
  /// responsible for any mandatory or optional supplementation payment. The
  /// code represents the State of residence unless another State has
  /// jurisdiction.
  ///  If no State is available, this field contains spaces.	
  /// </summary>
  [JsonPropertyName("stateCode")]
  [Member(Index = 3, Type = MemberType.Char, Length = StateCode_MaxLength, Optional
    = true)]
  public string StateCode
  {
    get => stateCode;
    set => stateCode = value != null
      ? TrimEnd(Substring(value, 1, StateCode_MaxLength)) : null;
  }

  /// <summary>Length of the COUNTY_CODE attribute.</summary>
  public const int CountyCode_MaxLength = 3;

  /// <summary>
  /// The value of the COUNTY_CODE attribute.
  /// This field contains the three-character FIPS County Code of the county 
  /// that is responsible for any mandatory or optional supplementation payment.
  /// The code represents the county of residence unless another county has
  /// jurisdiction.
  ///  If no county is available, this field contains spaces.	
  /// </summary>
  [JsonPropertyName("countyCode")]
  [Member(Index = 4, Type = MemberType.Char, Length = CountyCode_MaxLength, Optional
    = true)]
  public string CountyCode
  {
    get => countyCode;
    set => countyCode = value != null
      ? TrimEnd(Substring(value, 1, CountyCode_MaxLength)) : null;
  }

  /// <summary>Length of the DIRECT_DEPOSIT_INDICATOR attribute.</summary>
  public const int DirectDepositIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the DIRECT_DEPOSIT_INDICATOR attribute.
  /// This field contains one of the following values:
  /// C  Checking
  /// E  Electronic Benefits Transfer
  /// S  Savings
  /// Space  None
  /// </summary>
  [JsonPropertyName("directDepositIndicator")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = DirectDepositIndicator_MaxLength, Optional = true)]
  public string DirectDepositIndicator
  {
    get => directDepositIndicator;
    set => directDepositIndicator = value != null
      ? TrimEnd(Substring(value, 1, DirectDepositIndicator_MaxLength)) : null;
  }

  /// <summary>Length of the LAF_CODE attribute.</summary>
  public const int LafCode_MaxLength = 2;

  /// <summary>
  /// The value of the LAF_CODE attribute.
  /// This field contains a value for the Ledger Account File Code, which 
  /// reflects the MBR payment status for this beneficiary. Refer to Appendix E,
  /// Data Dictionary, for the list of valid values and their descriptions.
  /// </summary>
  [JsonPropertyName("lafCode")]
  [Member(Index = 6, Type = MemberType.Varchar, Length = LafCode_MaxLength, Optional
    = true)]
  public string LafCode
  {
    get => lafCode;
    set => lafCode = value != null ? Substring(value, 1, LafCode_MaxLength) : null
      ;
  }

  /// <summary>
  /// The value of the DEFERRED_PAYMENT_DATE attribute.
  /// This field contains the date in CCYYMM01 format that the first or next 
  /// deferred payment can be made.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("deferredPaymentDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? DeferredPaymentDate
  {
    get => deferredPaymentDate;
    set => deferredPaymentDate = value;
  }

  /// <summary>
  /// The value of the INITIAL_TITLE_II_ENTITLEMENT_DT attribute.
  /// This field contains the date that the beneficiary was originally eligible 
  /// for Title II benefits. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("initialTitleIiEntitlementDt")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? InitialTitleIiEntitlementDt
  {
    get => initialTitleIiEntitlementDt;
    set => initialTitleIiEntitlementDt = value;
  }

  /// <summary>
  /// The value of the CURRENT_TITLE_II_ENTITLEMENT_DT attribute.
  /// This field contains the date that the beneficiary became eligible for 
  /// Title II benefits for the current period of entitlement. The date is in
  /// CCYYMM01 format.
  ///  If this field was returned from SVES with an invalid date, this field 
  /// will contain spaces.
  /// </summary>
  [JsonPropertyName("currentTitleIiEntitlementDt")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? CurrentTitleIiEntitlementDt
  {
    get => currentTitleIiEntitlementDt;
    set => currentTitleIiEntitlementDt = value;
  }

  /// <summary>
  /// The value of the TITLE_II_SUSPEND_TERMINATE_DT attribute.
  /// This field contains the date that the event that caused the suspension or 
  /// termination of Title II benefits occurred. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("titleIiSuspendTerminateDt")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? TitleIiSuspendTerminateDt
  {
    get => titleIiSuspendTerminateDt;
    set => titleIiSuspendTerminateDt = value;
  }

  /// <summary>
  /// The value of the NET_MONTHLY_TITLE_II_BENEFIT attribute.
  /// This field contains the Net Monthly Benefit amount in $$$$¢¢ format. The 
  /// Net Monthly Benefit represents the benefit amount that is payable after
  /// all deductions have been taken.
  /// 
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("netMonthlyTitleIiBenefit")]
  [Member(Index = 11, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? NetMonthlyTitleIiBenefit
  {
    get => netMonthlyTitleIiBenefit;
    set => netMonthlyTitleIiBenefit = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the HI_OPTION_CODE attribute.</summary>
  public const int HiOptionCode_MaxLength = 1;

  /// <summary>
  /// The value of the HI_OPTION_CODE attribute.
  /// This field contains a Health Insurance (HI) Option code:
  /// C  None (cessation of disability)
  /// D  None (coverage denied)
  /// E  Yes (automatic; no premium necessary)
  /// F  None (invalid enrollment terminated)
  /// G  Yes  (good cause)
  /// H  None (not eligible or did not enroll)
  /// N  Obsolete
  /// P  Railroad Board has jurisdiction
  /// R  None (refused coverage)
  /// S  None (no longer under renal disease provision)
  /// T  None (terminated for nonpayment of premiums)
  /// W  None (withdrawal)
  /// X  None (Title II termination)
  /// Y  Supplemental insurance (Part B) premium is payable
  /// If no HI code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("hiOptionCode")]
  [Member(Index = 12, Type = MemberType.Char, Length = HiOptionCode_MaxLength, Optional
    = true)]
  public string HiOptionCode
  {
    get => hiOptionCode;
    set => hiOptionCode = value != null
      ? TrimEnd(Substring(value, 1, HiOptionCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the HI_START_DATE attribute.
  /// This field contains the date that the beneficiary became eligible for 
  /// health insurance. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("hiStartDate")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? HiStartDate
  {
    get => hiStartDate;
    set => hiStartDate = value;
  }

  /// <summary>
  /// The value of the HI_STOP_DATE attribute.
  /// This field contains the date that the beneficiarys health insurance 
  /// benefits ended.
  /// The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("hiStopDate")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? HiStopDate
  {
    get => hiStopDate;
    set => hiStopDate = value;
  }

  /// <summary>Length of the SMI_OPTION_CODE attribute.</summary>
  public const int SmiOptionCode_MaxLength = 1;

  /// <summary>
  /// The value of the SMI_OPTION_CODE attribute.
  /// This field contains a Supplemental Medical Insurance (SMI) Code:
  /// C  No (cessation of disability)
  /// D  No (coverage denied)
  /// F  No (invalid enrollment terminated)
  /// G  Yes (good cause)
  /// N  No (Puerto Rican beneficiary not entitled; also dually/technically 
  /// entitled beneficiary not entitled to SMI)
  /// P  Railroad Board has jurisdiction
  /// R  No (refused coverage)
  /// S  No (no longer under the renal disease provision)
  /// T  No (terminated for non-payment of premiums)
  /// W  No (withdrawal from coverage)
  /// Y  Yes
  /// If no code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("smiOptionCode")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = SmiOptionCode_MaxLength, Optional = true)]
  public string SmiOptionCode
  {
    get => smiOptionCode;
    set => smiOptionCode = value != null
      ? TrimEnd(Substring(value, 1, SmiOptionCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SMI_START_DATE attribute.
  /// This field contains the first month of coverage date that the beneficiary 
  /// became eligible for SMI. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("smiStartDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? SmiStartDate
  {
    get => smiStartDate;
    set => smiStartDate = value;
  }

  /// <summary>
  /// The value of the SMI_STOP_DATE attribute.
  /// This field contains the last month of coverage date of the beneficiarys 
  /// SMI benefits. The date is in CCYYMM01 format.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("smiStopDate")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
  public DateTime? SmiStopDate
  {
    get => smiStopDate;
    set => smiStopDate = value;
  }

  /// <summary>Length of the CATEGORY_OF_ASSISTANCE attribute.</summary>
  public const int CategoryOfAssistance_MaxLength = 1;

  /// <summary>
  /// The value of the CATEGORY_OF_ASSISTANCE attribute.
  /// This is field contains a State exchange categorical assistance code:
  /// A  Aged
  /// B  Blind
  /// C  AFDC
  /// D  Disabled
  /// F  Food Stamps
  /// H  Health maintenance
  /// I  Income Maintenance
  /// J  AFDC and/or Food Stamps
  /// K  Food Stamps and Medicaid
  /// N  Title XIX Medicaid eligibility
  /// P  Child Support Enforcement
  /// S  Statement of Consent
  /// U  Unemployment Compensation
  /// If no category is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("categoryOfAssistance")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = CategoryOfAssistance_MaxLength, Optional = true)]
  public string CategoryOfAssistance
  {
    get => categoryOfAssistance;
    set => categoryOfAssistance = value != null
      ? TrimEnd(Substring(value, 1, CategoryOfAssistance_MaxLength)) : null;
  }

  /// <summary>Length of the BLACK_LUNG_ENTITLEMENT_CODE attribute.</summary>
  public const int BlackLungEntitlementCode_MaxLength = 1;

  /// <summary>
  /// The value of the BLACK_LUNG_ENTITLEMENT_CODE attribute.
  /// This field contains a value to indicates Black Lung entitlement:
  /// D  Death termination
  /// E  Entitled
  /// N  Nonpayment
  /// P  Pending entitlement
  /// T  Terminated (other than death)
  /// If no code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("blackLungEntitlementCode")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = BlackLungEntitlementCode_MaxLength, Optional = true)]
  public string BlackLungEntitlementCode
  {
    get => blackLungEntitlementCode;
    set => blackLungEntitlementCode = value != null
      ? TrimEnd(Substring(value, 1, BlackLungEntitlementCode_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the BLACK_LUNG_PAYMENT_AMOUNT attribute.
  /// This field contains the Black Lung Payment Amount in $$$$¢¢ format.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("blackLungPaymentAmount")]
  [Member(Index = 20, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? BlackLungPaymentAmount
  {
    get => blackLungPaymentAmount;
    set => blackLungPaymentAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the RAILROAD_INDICATOR attribute.</summary>
  public const int RailroadIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the RAILROAD_INDICATOR attribute.
  /// This field contains a Railroad Indicator code:
  /// A  Active claim
  /// T  Terminated claim
  /// S  Currently suspended
  /// If no code is available, this field contains a space.
  /// </summary>
  [JsonPropertyName("railroadIndicator")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = RailroadIndicator_MaxLength, Optional = true)]
  public string RailroadIndicator
  {
    get => railroadIndicator;
    set => railroadIndicator = value != null
      ? TrimEnd(Substring(value, 1, RailroadIndicator_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MBC_NUMBER_OF_ENTRIES attribute.
  /// This field contains the number of occurrences of the historical payment 
  /// fields: MBC Date, MBC Amount, and MBC Type. This field contains a value
  /// from 0 through 8.
  /// </summary>
  [JsonPropertyName("mbcNumberOfEntries")]
  [Member(Index = 22, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? MbcNumberOfEntries
  {
    get => mbcNumberOfEntries;
    set => mbcNumberOfEntries = value;
  }

  /// <summary>
  /// The value of the MBC_DATE_1 attribute.
  /// This field contains the first Monthly Benefit Credited (MBC) Date. The 
  /// date is in CCYYMM01 format. The MBC amount is paid in the month after this
  /// date.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("mbcDate1")]
  [Member(Index = 23, Type = MemberType.Date, Optional = true)]
  public DateTime? MbcDate1
  {
    get => mbcDate1;
    set => mbcDate1 = value;
  }

  /// <summary>
  /// The value of the MBC_AMOUNT_1 attribute.
  /// This field contains the first MBC amount in $$$$¢¢ format. The monthly 
  /// Title II benefit is due after any appropriate dollar rounding (considering
  /// a deductible of the SMI premium) but prior to the actual collection of
  /// any obligation of the beneficiary (including the SMI premium). This amount
  /// may appear after an individual dies. States need to check the LAF Code
  /// and MBC Type to determine if a payment was issued.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("mbcAmount1")]
  [Member(Index = 24, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? MbcAmount1
  {
    get => mbcAmount1;
    set => mbcAmount1 = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the MBC_TYPE_1 attribute.</summary>
  public const int MbcType1_MaxLength = 1;

  /// <summary>
  /// The value of the MBC_TYPE_1 attribute.
  /// This field contains one of the following values:
  /// C  Benefits paid (credited)
  /// E  Benefits not paid (not credited), due to delayed/pending or suspense
  /// N  Benefits not paid (not credited)
  /// Space  Benefits not paid (not credited) or not applicable
  /// </summary>
  [JsonPropertyName("mbcType1")]
  [Member(Index = 25, Type = MemberType.Char, Length = MbcType1_MaxLength, Optional
    = true)]
  public string MbcType1
  {
    get => mbcType1;
    set => mbcType1 = value != null
      ? TrimEnd(Substring(value, 1, MbcType1_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MBC_DATE_2 attribute.
  /// This field contains the second MBC date. The date is in CCYYMM01 format.
  /// The MBC amount is paid in the month after this date.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("mbcDate2")]
  [Member(Index = 26, Type = MemberType.Date, Optional = true)]
  public DateTime? MbcDate2
  {
    get => mbcDate2;
    set => mbcDate2 = value;
  }

  /// <summary>
  /// The value of the MBC_AMOUNT_2 attribute.
  /// This field contains the second MBC amount in $$$$¢¢ format.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("mbcAmount2")]
  [Member(Index = 27, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? MbcAmount2
  {
    get => mbcAmount2;
    set => mbcAmount2 = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the MBC_TYPE_2 attribute.</summary>
  public const int MbcType2_MaxLength = 1;

  /// <summary>
  /// The value of the MBC_TYPE_2 attribute.
  /// This field contains one of the codes listed for the MBC Type 1.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("mbcType2")]
  [Member(Index = 28, Type = MemberType.Char, Length = MbcType2_MaxLength, Optional
    = true)]
  public string MbcType2
  {
    get => mbcType2;
    set => mbcType2 = value != null
      ? TrimEnd(Substring(value, 1, MbcType2_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MBC_DATE_3 attribute.
  /// This field contains the third MBC date. The date is in CCYYMM01 format.
  /// The MBC amount is paid in the month after this date.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("mbcDate3")]
  [Member(Index = 29, Type = MemberType.Date, Optional = true)]
  public DateTime? MbcDate3
  {
    get => mbcDate3;
    set => mbcDate3 = value;
  }

  /// <summary>
  /// The value of the MBC_AMOUNT_3 attribute.
  /// This field contains the third MBC amount in $$$$¢¢ format.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("mbcAmount3")]
  [Member(Index = 30, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? MbcAmount3
  {
    get => mbcAmount3;
    set => mbcAmount3 = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the MBC_TYPE_3 attribute.</summary>
  public const int MbcType3_MaxLength = 1;

  /// <summary>
  /// The value of the MBC_TYPE_3 attribute.
  /// This field contains one of the codes listed for the MBC Type 1.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("mbcType3")]
  [Member(Index = 31, Type = MemberType.Char, Length = MbcType3_MaxLength, Optional
    = true)]
  public string MbcType3
  {
    get => mbcType3;
    set => mbcType3 = value != null
      ? TrimEnd(Substring(value, 1, MbcType3_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MBC_DATE_4 attribute.
  /// This field contains the fourth MBC date. The date is in CCYYMM01 format.
  /// The MBC amount is paid in the month after this date.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("mbcDate4")]
  [Member(Index = 32, Type = MemberType.Date, Optional = true)]
  public DateTime? MbcDate4
  {
    get => mbcDate4;
    set => mbcDate4 = value;
  }

  /// <summary>
  /// The value of the MBC_AMOUNT_4 attribute.
  /// This field contains the fourth MBC amount in $$$$¢¢ format.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("mbcAmount4")]
  [Member(Index = 33, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? MbcAmount4
  {
    get => mbcAmount4;
    set => mbcAmount4 = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the MBC_TYPE_4 attribute.</summary>
  public const int MbcType4_MaxLength = 1;

  /// <summary>
  /// The value of the MBC_TYPE_4 attribute.
  /// This field contains one of the codes listed for the MBC Type 1.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("mbcType4")]
  [Member(Index = 34, Type = MemberType.Char, Length = MbcType4_MaxLength, Optional
    = true)]
  public string MbcType4
  {
    get => mbcType4;
    set => mbcType4 = value != null
      ? TrimEnd(Substring(value, 1, MbcType4_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MBC_DATE_5 attribute.
  /// This field contains the fifth MBC date. The date is in CCYYMM01 format.
  /// The MBC amount is paid in the month after this date.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("mbcDate5")]
  [Member(Index = 35, Type = MemberType.Date, Optional = true)]
  public DateTime? MbcDate5
  {
    get => mbcDate5;
    set => mbcDate5 = value;
  }

  /// <summary>
  /// The value of the MBC_AMOUNT_5 attribute.
  /// This field contains the fifth MBC amount in $$$$¢¢ format.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("mbcAmount5")]
  [Member(Index = 36, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? MbcAmount5
  {
    get => mbcAmount5;
    set => mbcAmount5 = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the MBC_TYPE_5 attribute.</summary>
  public const int MbcType5_MaxLength = 1;

  /// <summary>
  /// The value of the MBC_TYPE_5 attribute.
  /// This field contains one of the codes listed for the MBC Type 1.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("mbcType5")]
  [Member(Index = 37, Type = MemberType.Char, Length = MbcType5_MaxLength, Optional
    = true)]
  public string MbcType5
  {
    get => mbcType5;
    set => mbcType5 = value != null
      ? TrimEnd(Substring(value, 1, MbcType5_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MBC_DATE_6 attribute.
  /// This field contains the sixth MBC date. The date is in CCYYMM01 format.
  /// The MBC amount is paid in the month after this date.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("mbcDate6")]
  [Member(Index = 38, Type = MemberType.Date, Optional = true)]
  public DateTime? MbcDate6
  {
    get => mbcDate6;
    set => mbcDate6 = value;
  }

  /// <summary>
  /// The value of the MBC_AMOUNT_6 attribute.
  /// This field contains the sixth MBC amount in $$$$¢¢ format.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("mbcAmount6")]
  [Member(Index = 39, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? MbcAmount6
  {
    get => mbcAmount6;
    set => mbcAmount6 = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the MBC_TYPE_6 attribute.</summary>
  public const int MbcType6_MaxLength = 1;

  /// <summary>
  /// The value of the MBC_TYPE_6 attribute.
  /// This field contains one of the codes listed for the MBC Type 1.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("mbcType6")]
  [Member(Index = 40, Type = MemberType.Char, Length = MbcType6_MaxLength, Optional
    = true)]
  public string MbcType6
  {
    get => mbcType6;
    set => mbcType6 = value != null
      ? TrimEnd(Substring(value, 1, MbcType6_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MBC_DATE_7 attribute.
  /// This field contains the seventh MBC date. The date is in CCYYMM01 format.
  /// The MBC amount is paid in the month after this date.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("mbcDate7")]
  [Member(Index = 41, Type = MemberType.Date, Optional = true)]
  public DateTime? MbcDate7
  {
    get => mbcDate7;
    set => mbcDate7 = value;
  }

  /// <summary>
  /// The value of the MBC_AMOUNT_7 attribute.
  /// This field contains the seventh MBC amount in $$$$¢¢ format.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("mbcAmount7")]
  [Member(Index = 42, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? MbcAmount7
  {
    get => mbcAmount7;
    set => mbcAmount7 = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the MBC_TYPE_7 attribute.</summary>
  public const int MbcType7_MaxLength = 1;

  /// <summary>
  /// The value of the MBC_TYPE_7 attribute.
  /// This field contains one of the codes listed for the MBC Type 1.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("mbcType7")]
  [Member(Index = 43, Type = MemberType.Char, Length = MbcType7_MaxLength, Optional
    = true)]
  public string MbcType7
  {
    get => mbcType7;
    set => mbcType7 = value != null
      ? TrimEnd(Substring(value, 1, MbcType7_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MBC_DATE_8 attribute.
  /// This field contains the eighth MBC date. The date is in CCYYMM01 format.
  /// The MBC amount is paid in the month after this date.
  /// If this field was returned from SVES with an invalid date, this field will
  /// contain spaces.
  /// </summary>
  [JsonPropertyName("mbcDate8")]
  [Member(Index = 44, Type = MemberType.Date, Optional = true)]
  public DateTime? MbcDate8
  {
    get => mbcDate8;
    set => mbcDate8 = value;
  }

  /// <summary>
  /// The value of the MBC_AMOUNT_8 attribute.
  /// This field contains the eighth MBC amount in $$$$¢¢ format.
  /// If no amount is available, this field contains zeros.
  /// </summary>
  [JsonPropertyName("mbcAmount8")]
  [Member(Index = 45, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? MbcAmount8
  {
    get => mbcAmount8;
    set => mbcAmount8 = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the MBC_TYPE_8 attribute.</summary>
  public const int MbcType8_MaxLength = 1;

  /// <summary>
  /// The value of the MBC_TYPE_8 attribute.
  /// This field contains one of the codes listed for the MBC Type 1.
  /// If no code is available, this field contains spaces.
  /// </summary>
  [JsonPropertyName("mbcType8")]
  [Member(Index = 46, Type = MemberType.Char, Length = MbcType8_MaxLength, Optional
    = true)]
  public string MbcType8
  {
    get => mbcType8;
    set => mbcType8 = value != null
      ? TrimEnd(Substring(value, 1, MbcType8_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 48, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 49, Type = MemberType.Char, Length
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
  [Member(Index = 50, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_RESPONSE_AGENCY_CO attribute.
  /// </summary>
  public const int FcgLSRspAgy_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_RESPONSE_AGENCY_CO attribute.
  /// This field contains the code that identifies the Locate Source.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 51, Type = MemberType.Char, Length = FcgLSRspAgy_MaxLength)]
  public string FcgLSRspAgy
  {
    get => fcgLSRspAgy ?? "";
    set => fcgLSRspAgy = TrimEnd(Substring(value, 1, FcgLSRspAgy_MaxLength));
  }

  /// <summary>
  /// The json value of the FcgLSRspAgy attribute.</summary>
  [JsonPropertyName("fcgLSRspAgy")]
  [Computed]
  public string FcgLSRspAgy_Json
  {
    get => NullIf(FcgLSRspAgy, "");
    set => FcgLSRspAgy = value;
  }

  /// <summary>Length of the MEMBER_ID attribute.</summary>
  public const int FcgMemberId_MaxLength = 15;

  /// <summary>
  /// The value of the MEMBER_ID attribute.
  /// This field contains the Member ID that was provided by the submitter of 
  /// the Locate Request.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 52, Type = MemberType.Char, Length = FcgMemberId_MaxLength)]
  public string FcgMemberId
  {
    get => fcgMemberId ?? "";
    set => fcgMemberId = TrimEnd(Substring(value, 1, FcgMemberId_MaxLength));
  }

  /// <summary>
  /// The json value of the FcgMemberId attribute.</summary>
  [JsonPropertyName("fcgMemberId")]
  [Computed]
  public string FcgMemberId_Json
  {
    get => NullIf(FcgMemberId, "");
    set => FcgMemberId = value;
  }

  private int seqNo;
  private string canAndBic;
  private string stateCode;
  private string countyCode;
  private string directDepositIndicator;
  private string lafCode;
  private DateTime? deferredPaymentDate;
  private DateTime? initialTitleIiEntitlementDt;
  private DateTime? currentTitleIiEntitlementDt;
  private DateTime? titleIiSuspendTerminateDt;
  private decimal? netMonthlyTitleIiBenefit;
  private string hiOptionCode;
  private DateTime? hiStartDate;
  private DateTime? hiStopDate;
  private string smiOptionCode;
  private DateTime? smiStartDate;
  private DateTime? smiStopDate;
  private string categoryOfAssistance;
  private string blackLungEntitlementCode;
  private decimal? blackLungPaymentAmount;
  private string railroadIndicator;
  private int? mbcNumberOfEntries;
  private DateTime? mbcDate1;
  private decimal? mbcAmount1;
  private string mbcType1;
  private DateTime? mbcDate2;
  private decimal? mbcAmount2;
  private string mbcType2;
  private DateTime? mbcDate3;
  private decimal? mbcAmount3;
  private string mbcType3;
  private DateTime? mbcDate4;
  private decimal? mbcAmount4;
  private string mbcType4;
  private DateTime? mbcDate5;
  private decimal? mbcAmount5;
  private string mbcType5;
  private DateTime? mbcDate6;
  private decimal? mbcAmount6;
  private string mbcType6;
  private DateTime? mbcDate7;
  private decimal? mbcAmount7;
  private string mbcType7;
  private DateTime? mbcDate8;
  private decimal? mbcAmount8;
  private string mbcType8;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string fcgLSRspAgy;
  private string fcgMemberId;
}
