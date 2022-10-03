// The source file: DASHBOARD_OUTPUT_METRICS, ID: 945147820, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Dashboard -- staging table for priority 3 and 5 calculations.
/// </summary>
[Serializable]
public partial class DashboardOutputMetrics: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DashboardOutputMetrics()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DashboardOutputMetrics(DashboardOutputMetrics that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DashboardOutputMetrics Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DashboardOutputMetrics that)
  {
    base.Assign(that);
    reportMonth = that.reportMonth;
    reportLevel = that.reportLevel;
    reportLevelId = that.reportLevelId;
    type1 = that.type1;
    asOfDate = that.asOfDate;
    casesWithEstReferral = that.casesWithEstReferral;
    casesWithEnfReferral = that.casesWithEnfReferral;
    fullTimeEquivalent = that.fullTimeEquivalent;
    newOrdersEstablished = that.newOrdersEstablished;
    paternitiesEstablished = that.paternitiesEstablished;
    casesOpenedWithOrder = that.casesOpenedWithOrder;
    casesOpenedWithoutOrders = that.casesOpenedWithoutOrders;
    casesClosedWithOrders = that.casesClosedWithOrders;
    casesClosedWithoutOrders = that.casesClosedWithoutOrders;
    modifications = that.modifications;
    incomeWithholdingsIssued = that.incomeWithholdingsIssued;
    contemptMotionFilings = that.contemptMotionFilings;
    contemptOrderFilings = that.contemptOrderFilings;
    stypeCollectionAmount = that.stypeCollectionAmount;
    stypePercentOfTotal = that.stypePercentOfTotal;
    ftypeCollectionAmount = that.ftypeCollectionAmount;
    ftypePercentOfTotal = that.ftypePercentOfTotal;
    itypeCollectionAmount = that.itypeCollectionAmount;
    itypePercentOfTotal = that.itypePercentOfTotal;
    utypeCollectionAmount = that.utypeCollectionAmount;
    utypePercentOfTotal = that.utypePercentOfTotal;
    ctypeCollectionAmount = that.ctypeCollectionAmount;
    ctypePercentOfTotal = that.ctypePercentOfTotal;
    totalCollectionAmount = that.totalCollectionAmount;
    daysToOrderEstblshmntNumer = that.daysToOrderEstblshmntNumer;
    daysToOrderEstblshmntDenom = that.daysToOrderEstblshmntDenom;
    daysToOrderEstblshmntAvg = that.daysToOrderEstblshmntAvg;
    daysToReturnOfSrvcNumer = that.daysToReturnOfSrvcNumer;
    daysToReturnOfServiceDenom = that.daysToReturnOfServiceDenom;
    daysToReturnOfServiceAvg = that.daysToReturnOfServiceAvg;
    referralAging60To90Days = that.referralAging60To90Days;
    referralAging91To120Days = that.referralAging91To120Days;
    referralAging121To150Days = that.referralAging121To150Days;
    referralAging151PlusDays = that.referralAging151PlusDays;
    daysToIwoPaymentNumerator = that.daysToIwoPaymentNumerator;
    daysToIwoPaymentDenominator = that.daysToIwoPaymentDenominator;
    daysToIwoPaymentAverage = that.daysToIwoPaymentAverage;
    referralsToLegalForEst = that.referralsToLegalForEst;
    referralsToLegalForEnf = that.referralsToLegalForEnf;
    caseloadCount = that.caseloadCount;
    casesOpened = that.casesOpened;
    ncpLocatesByAddress = that.ncpLocatesByAddress;
    ncpLocatesByEmployer = that.ncpLocatesByEmployer;
    caseClosures = that.caseClosures;
    caseReviews = that.caseReviews;
    petitions = that.petitions;
    casesPayingArrearsDenominator = that.casesPayingArrearsDenominator;
    casesPayingArrearsNumerator = that.casesPayingArrearsNumerator;
    casesPayingArrearsPercent = that.casesPayingArrearsPercent;
    casesPayingArrearsRank = that.casesPayingArrearsRank;
    currentSupportPaidFfytdDen = that.currentSupportPaidFfytdDen;
    currentSupportPaidFfytdNum = that.currentSupportPaidFfytdNum;
    currentSupportPaidFfytdPer = that.currentSupportPaidFfytdPer;
    currentSupportPaidFfytdRnk = that.currentSupportPaidFfytdRnk;
    currentSupportPaidMthDen = that.currentSupportPaidMthDen;
    currentSupportPaidMthNum = that.currentSupportPaidMthNum;
    currentSupportPaidMthPer = that.currentSupportPaidMthPer;
    currentSupportPaidMthRnk = that.currentSupportPaidMthRnk;
  }

  /// <summary>
  /// The value of the REPORT_MONTH attribute.
  /// Dashboard report month. This will be the reporting year and month in a 
  /// YYYYMM numeric format.
  /// </summary>
  [JsonPropertyName("reportMonth")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 6)]
  public int ReportMonth
  {
    get => reportMonth;
    set => reportMonth = value;
  }

  /// <summary>Length of the REPORT_LEVEL attribute.</summary>
  public const int ReportLevel_MaxLength = 2;

  /// <summary>
  /// The value of the REPORT_LEVEL attribute.
  /// Level at which data is being reported: Judicial District (JD), Attorney (
  /// AT), Caseworker (CW)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ReportLevel_MaxLength)]
  public string ReportLevel
  {
    get => reportLevel ?? "";
    set => reportLevel = TrimEnd(Substring(value, 1, ReportLevel_MaxLength));
  }

  /// <summary>
  /// The json value of the ReportLevel attribute.</summary>
  [JsonPropertyName("reportLevel")]
  [Computed]
  public string ReportLevel_Json
  {
    get => NullIf(ReportLevel, "");
    set => ReportLevel = value;
  }

  /// <summary>Length of the REPORT_LEVEL_ID attribute.</summary>
  public const int ReportLevelId_MaxLength = 8;

  /// <summary>
  /// The value of the REPORT_LEVEL_ID attribute.
  /// Identifier of the Judicial District, Attorney, or Caseworker for which 
  /// this data applies.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = ReportLevelId_MaxLength)]
  public string ReportLevelId
  {
    get => reportLevelId ?? "";
    set => reportLevelId =
      TrimEnd(Substring(value, 1, ReportLevelId_MaxLength));
  }

  /// <summary>
  /// The json value of the ReportLevelId attribute.</summary>
  [JsonPropertyName("reportLevelId")]
  [Computed]
  public string ReportLevelId_Json
  {
    get => NullIf(ReportLevelId, "");
    set => ReportLevelId = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 4;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines whether this is a Worker/Judicial District 'GOAL' or 'DATA' 
  /// record.  The 'GOAL' record will contain the monthly target amounts as
  /// defined by Central Office.  The 'DATA' record will contain the actual
  /// values calculated and reported for the month.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Type1_MaxLength)]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
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
  /// The value of the AS_OF_DATE attribute.
  /// Date through which calculations in this row apply.
  /// </summary>
  [JsonPropertyName("asOfDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? AsOfDate
  {
    get => asOfDate;
    set => asOfDate = value;
  }

  /// <summary>
  /// The value of the CASES_WITH_EST_REFERRAL attribute.
  /// Number of cases with an active Establishment referral.
  /// </summary>
  [JsonPropertyName("casesWithEstReferral")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesWithEstReferral
  {
    get => casesWithEstReferral;
    set => casesWithEstReferral = value;
  }

  /// <summary>
  /// The value of the CASES_WITH_ENF_REFERRAL attribute.
  /// Number of cases with an active Enforcement referral.
  /// </summary>
  [JsonPropertyName("casesWithEnfReferral")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesWithEnfReferral
  {
    get => casesWithEnfReferral;
    set => casesWithEnfReferral = value;
  }

  /// <summary>
  /// The value of the FULL_TIME_EQUIVALENT attribute.
  /// Full Time Equivalent.
  /// </summary>
  [JsonPropertyName("fullTimeEquivalent")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? FullTimeEquivalent
  {
    get => fullTimeEquivalent;
    set => fullTimeEquivalent = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the NEW_ORDERS_ESTABLISHED attribute.
  /// Number of new court orders established.
  /// </summary>
  [JsonPropertyName("newOrdersEstablished")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NewOrdersEstablished
  {
    get => newOrdersEstablished;
    set => newOrdersEstablished = value;
  }

  /// <summary>
  /// The value of the PATERNITIES_ESTABLISHED attribute.
  /// Number of children with paternity established by court order.
  /// </summary>
  [JsonPropertyName("paternitiesEstablished")]
  [Member(Index = 10, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PaternitiesEstablished
  {
    get => paternitiesEstablished;
    set => paternitiesEstablished = value;
  }

  /// <summary>
  /// The value of the CASES_OPENED_WITH_ORDER attribute.
  /// Number of cases opened that meet the definition of a case under order.
  /// </summary>
  [JsonPropertyName("casesOpenedWithOrder")]
  [Member(Index = 11, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesOpenedWithOrder
  {
    get => casesOpenedWithOrder;
    set => casesOpenedWithOrder = value;
  }

  /// <summary>
  /// The value of the CASES_OPENED_WITHOUT_ORDERS attribute.
  /// Number of cases opened that do not meet the definition of a case under 
  /// order.
  /// </summary>
  [JsonPropertyName("casesOpenedWithoutOrders")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesOpenedWithoutOrders
  {
    get => casesOpenedWithoutOrders;
    set => casesOpenedWithoutOrders = value;
  }

  /// <summary>
  /// The value of the CASES_CLOSED_WITH_ORDERS attribute.
  /// Number of cases closed that were under order.
  /// </summary>
  [JsonPropertyName("casesClosedWithOrders")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesClosedWithOrders
  {
    get => casesClosedWithOrders;
    set => casesClosedWithOrders = value;
  }

  /// <summary>
  /// The value of the CASES_CLOSED_WITHOUT_ORDERS attribute.
  /// Number of cases closed that were not under order.
  /// </summary>
  [JsonPropertyName("casesClosedWithoutOrders")]
  [Member(Index = 14, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesClosedWithoutOrders
  {
    get => casesClosedWithoutOrders;
    set => casesClosedWithoutOrders = value;
  }

  /// <summary>
  /// The value of the MODIFICATIONS attribute.
  /// Number of modifications reviews completed.
  /// </summary>
  [JsonPropertyName("modifications")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? Modifications
  {
    get => modifications;
    set => modifications = value;
  }

  /// <summary>
  /// The value of the INCOME_WITHHOLDINGS_ISSUED attribute.
  /// Number of income withholdings issued.
  /// </summary>
  [JsonPropertyName("incomeWithholdingsIssued")]
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IncomeWithholdingsIssued
  {
    get => incomeWithholdingsIssued;
    set => incomeWithholdingsIssued = value;
  }

  /// <summary>
  /// The value of the CONTEMPT_MOTION_FILINGS attribute.
  /// Number of court orders with a Motion for Contempt filed.
  /// </summary>
  [JsonPropertyName("contemptMotionFilings")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ContemptMotionFilings
  {
    get => contemptMotionFilings;
    set => contemptMotionFilings = value;
  }

  /// <summary>
  /// The value of the CONTEMPT_ORDER_FILINGS attribute.
  /// Number of court orders with a Order for Contempt filed.
  /// </summary>
  [JsonPropertyName("contemptOrderFilings")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ContemptOrderFilings
  {
    get => contemptOrderFilings;
    set => contemptOrderFilings = value;
  }

  /// <summary>
  /// The value of the S_TYPE_COLLECTION_AMOUNT attribute.
  /// Dollar amount of S type collections during the period.
  /// </summary>
  [JsonPropertyName("stypeCollectionAmount")]
  [Member(Index = 19, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? StypeCollectionAmount
  {
    get => stypeCollectionAmount;
    set => stypeCollectionAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the S_TYPE_PERCENT_OF_TOTAL attribute.
  /// Percentage of total collections attributable to S type collections.
  /// </summary>
  [JsonPropertyName("stypePercentOfTotal")]
  [Member(Index = 20, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? StypePercentOfTotal
  {
    get => stypePercentOfTotal;
    set => stypePercentOfTotal = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the F_TYPE_COLLECTION_AMOUNT attribute.
  /// Dollar amount of F type collections during the period.
  /// </summary>
  [JsonPropertyName("ftypeCollectionAmount")]
  [Member(Index = 21, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? FtypeCollectionAmount
  {
    get => ftypeCollectionAmount;
    set => ftypeCollectionAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the F_TYPE_PERCENT_OF_TOTAL attribute.
  /// Percentage of total collections attributable to F type collections.
  /// </summary>
  [JsonPropertyName("ftypePercentOfTotal")]
  [Member(Index = 22, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? FtypePercentOfTotal
  {
    get => ftypePercentOfTotal;
    set => ftypePercentOfTotal = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the I_TYPE_COLLECTION_AMOUNT attribute.
  /// Dollar amount of I type collections during the period.
  /// </summary>
  [JsonPropertyName("itypeCollectionAmount")]
  [Member(Index = 23, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? ItypeCollectionAmount
  {
    get => itypeCollectionAmount;
    set => itypeCollectionAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the I_TYPE_PERCENT_OF_TOTAL attribute.
  /// Percentage of total collections attributable to I type collections.
  /// </summary>
  [JsonPropertyName("itypePercentOfTotal")]
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? ItypePercentOfTotal
  {
    get => itypePercentOfTotal;
    set => itypePercentOfTotal = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the U_TYPE_COLLECTION_AMOUNT attribute.
  /// Dollar amount of U type collections during the period.
  /// </summary>
  [JsonPropertyName("utypeCollectionAmount")]
  [Member(Index = 25, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? UtypeCollectionAmount
  {
    get => utypeCollectionAmount;
    set => utypeCollectionAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the U_TYPE_PERCENT_OF_TOTAL attribute.
  /// Percentage of total collections attributable to U type collections.
  /// </summary>
  [JsonPropertyName("utypePercentOfTotal")]
  [Member(Index = 26, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? UtypePercentOfTotal
  {
    get => utypePercentOfTotal;
    set => utypePercentOfTotal = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the C_TYPE_COLLECTION_AMOUNT attribute.
  /// Dollar amount of C type collections during the period.
  /// </summary>
  [JsonPropertyName("ctypeCollectionAmount")]
  [Member(Index = 27, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CtypeCollectionAmount
  {
    get => ctypeCollectionAmount;
    set => ctypeCollectionAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the C_TYPE_PERCENT_OF_TOTAL attribute.
  /// Percentage of total collections attributable to C type collections.
  /// </summary>
  [JsonPropertyName("ctypePercentOfTotal")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CtypePercentOfTotal
  {
    get => ctypePercentOfTotal;
    set => ctypePercentOfTotal = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the TOTAL_COLLECTION_AMOUNT attribute.
  /// Total dollar amount of collections during the period.
  /// </summary>
  [JsonPropertyName("totalCollectionAmount")]
  [Member(Index = 29, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? TotalCollectionAmount
  {
    get => totalCollectionAmount;
    set => totalCollectionAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the DAYS_TO_ORDER_ESTBLSHMNT_NUMER attribute.
  /// Total number of days between referral and order establishment.
  /// </summary>
  [JsonPropertyName("daysToOrderEstblshmntNumer")]
  [Member(Index = 30, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysToOrderEstblshmntNumer
  {
    get => daysToOrderEstblshmntNumer;
    set => daysToOrderEstblshmntNumer = value;
  }

  /// <summary>
  /// The value of the DAYS_TO_ORDER_ESTBLSHMNT_DENOM attribute.
  /// Total number of referrals.
  /// </summary>
  [JsonPropertyName("daysToOrderEstblshmntDenom")]
  [Member(Index = 31, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysToOrderEstblshmntDenom
  {
    get => daysToOrderEstblshmntDenom;
    set => daysToOrderEstblshmntDenom = value;
  }

  /// <summary>
  /// The value of the DAYS_TO_ORDER_ESTBLSHMNT_AVG attribute.
  /// Average number of days between referral and order establishment.
  /// </summary>
  [JsonPropertyName("daysToOrderEstblshmntAvg")]
  [Member(Index = 32, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? DaysToOrderEstblshmntAvg
  {
    get => daysToOrderEstblshmntAvg;
    set => daysToOrderEstblshmntAvg = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the DAYS_TO_RETURN_OF_SRVC_NUMER attribute.
  /// Total number of days between referral and return of service.
  /// </summary>
  [JsonPropertyName("daysToReturnOfSrvcNumer")]
  [Member(Index = 33, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysToReturnOfSrvcNumer
  {
    get => daysToReturnOfSrvcNumer;
    set => daysToReturnOfSrvcNumer = value;
  }

  /// <summary>
  /// The value of the DAYS_TO_RETURN_OF_SERVICE_DENOM attribute.
  /// Total number of referrals.
  /// </summary>
  [JsonPropertyName("daysToReturnOfServiceDenom")]
  [Member(Index = 34, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysToReturnOfServiceDenom
  {
    get => daysToReturnOfServiceDenom;
    set => daysToReturnOfServiceDenom = value;
  }

  /// <summary>
  /// The value of the DAYS_TO_RETURN_OF_SERVICE_AVG attribute.
  /// Average number of days between referral and return of service.
  /// </summary>
  [JsonPropertyName("daysToReturnOfServiceAvg")]
  [Member(Index = 35, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? DaysToReturnOfServiceAvg
  {
    get => daysToReturnOfServiceAvg;
    set => daysToReturnOfServiceAvg = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the REFERRAL_AGING_60_TO_90_DAYS attribute.
  /// Number of referrals remaining unprocessed 60 to 90 days after creation.
  /// </summary>
  [JsonPropertyName("referralAging60To90Days")]
  [Member(Index = 36, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ReferralAging60To90Days
  {
    get => referralAging60To90Days;
    set => referralAging60To90Days = value;
  }

  /// <summary>
  /// The value of the REFERRAL_AGING_91_TO_120_DAYS attribute.
  /// Number of referrals remaining unprocessed 91 to 120 days after creation.
  /// </summary>
  [JsonPropertyName("referralAging91To120Days")]
  [Member(Index = 37, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ReferralAging91To120Days
  {
    get => referralAging91To120Days;
    set => referralAging91To120Days = value;
  }

  /// <summary>
  /// The value of the REFERRAL_AGING_121_TO_150_DAYS attribute.
  /// Number of referrals remaining unprocessed 121 to 150 days after creation.
  /// </summary>
  [JsonPropertyName("referralAging121To150Days")]
  [Member(Index = 38, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ReferralAging121To150Days
  {
    get => referralAging121To150Days;
    set => referralAging121To150Days = value;
  }

  /// <summary>
  /// The value of the REFERRAL_AGING_151_PLUS_DAYS attribute.
  /// Number of referrals remaining unprocessed 151 or more days after creation.
  /// </summary>
  [JsonPropertyName("referralAging151PlusDays")]
  [Member(Index = 39, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ReferralAging151PlusDays
  {
    get => referralAging151PlusDays;
    set => referralAging151PlusDays = value;
  }

  /// <summary>
  /// The value of the DAYS_TO_IWO_PAYMENT_NUMERATOR attribute.
  /// Total number of days between IWO issuance and I type payment.
  /// </summary>
  [JsonPropertyName("daysToIwoPaymentNumerator")]
  [Member(Index = 40, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysToIwoPaymentNumerator
  {
    get => daysToIwoPaymentNumerator;
    set => daysToIwoPaymentNumerator = value;
  }

  /// <summary>
  /// The value of the DAYS_TO_IWO_PAYMENT_DENOMINATOR attribute.
  /// Total number of IWOs issued.
  /// </summary>
  [JsonPropertyName("daysToIwoPaymentDenominator")]
  [Member(Index = 41, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysToIwoPaymentDenominator
  {
    get => daysToIwoPaymentDenominator;
    set => daysToIwoPaymentDenominator = value;
  }

  /// <summary>
  /// The value of the DAYS_TO_IWO_PAYMENT_AVERAGE attribute.
  /// Average number of days between IWO issuance and I type payment.
  /// </summary>
  [JsonPropertyName("daysToIwoPaymentAverage")]
  [Member(Index = 42, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? DaysToIwoPaymentAverage
  {
    get => daysToIwoPaymentAverage;
    set => daysToIwoPaymentAverage = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the REFERRALS_TO_LEGAL_FOR_EST attribute.
  /// Number of EST referrals sent by a caseworker.
  /// </summary>
  [JsonPropertyName("referralsToLegalForEst")]
  [Member(Index = 43, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ReferralsToLegalForEst
  {
    get => referralsToLegalForEst;
    set => referralsToLegalForEst = value;
  }

  /// <summary>
  /// The value of the REFERRALS_TO_LEGAL_FOR_ENF attribute.
  /// Number of ENF referrals sent by a caseworker.
  /// </summary>
  [JsonPropertyName("referralsToLegalForEnf")]
  [Member(Index = 44, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ReferralsToLegalForEnf
  {
    get => referralsToLegalForEnf;
    set => referralsToLegalForEnf = value;
  }

  /// <summary>
  /// The value of the CASELOAD_COUNT attribute.
  /// Number of cases assigned to a caseworker or attorney.
  /// </summary>
  [JsonPropertyName("caseloadCount")]
  [Member(Index = 45, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CaseloadCount
  {
    get => caseloadCount;
    set => caseloadCount = value;
  }

  /// <summary>
  /// The value of the CASES_OPENED attribute.
  /// Number of cases opened during the reporting period.
  /// </summary>
  [JsonPropertyName("casesOpened")]
  [Member(Index = 46, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesOpened
  {
    get => casesOpened;
    set => casesOpened = value;
  }

  /// <summary>
  /// The value of the NCP_LOCATES_BY_ADDRESS attribute.
  /// Number of NCPs located by address.
  /// </summary>
  [JsonPropertyName("ncpLocatesByAddress")]
  [Member(Index = 47, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NcpLocatesByAddress
  {
    get => ncpLocatesByAddress;
    set => ncpLocatesByAddress = value;
  }

  /// <summary>
  /// The value of the NCP_LOCATES_BY_EMPLOYER attribute.
  /// Number of NCPs located by employment.
  /// </summary>
  [JsonPropertyName("ncpLocatesByEmployer")]
  [Member(Index = 48, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NcpLocatesByEmployer
  {
    get => ncpLocatesByEmployer;
    set => ncpLocatesByEmployer = value;
  }

  /// <summary>
  /// The value of the CASE_CLOSURES attribute.
  /// Number of cases closed during the reporting period.
  /// </summary>
  [JsonPropertyName("caseClosures")]
  [Member(Index = 49, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CaseClosures
  {
    get => caseClosures;
    set => caseClosures = value;
  }

  /// <summary>
  /// The value of the CASE_REVIEWS attribute.
  /// Number of case reviews completed during the reporting period.
  /// </summary>
  [JsonPropertyName("caseReviews")]
  [Member(Index = 50, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CaseReviews
  {
    get => caseReviews;
    set => caseReviews = value;
  }

  /// <summary>
  /// The value of the PETITIONS attribute.
  /// The number of petitions a worker has created.
  /// </summary>
  [JsonPropertyName("petitions")]
  [Member(Index = 51, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? Petitions
  {
    get => petitions;
    set => petitions = value;
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_DENOMINATOR attribute.
  /// Cases open at any point during the report period with arrears due.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsDenominator")]
  [Member(Index = 52, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingArrearsDenominator
  {
    get => casesPayingArrearsDenominator;
    set => casesPayingArrearsDenominator = value;
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_NUMERATOR attribute.
  /// The number of cases paying towards arrears during the report period.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsNumerator")]
  [Member(Index = 53, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingArrearsNumerator
  {
    get => casesPayingArrearsNumerator;
    set => casesPayingArrearsNumerator = value;
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_PERCENT attribute.
  /// Percent of cases paying on arrears.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsPercent")]
  [Member(Index = 54, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CasesPayingArrearsPercent
  {
    get => casesPayingArrearsPercent;
    set => casesPayingArrearsPercent = value != null ? Truncate(value, 3) : null
      ;
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_RANK attribute.
  /// Ranking of office by cases paying on arrears.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsRank")]
  [Member(Index = 55, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingArrearsRank
  {
    get => casesPayingArrearsRank;
    set => casesPayingArrearsRank = value;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_DEN attribute.
  /// Dollar amount of current support due in federal fiscal year to date.	
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdDen")]
  [Member(Index = 56, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidFfytdDen
  {
    get => currentSupportPaidFfytdDen;
    set => currentSupportPaidFfytdDen = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_NUM attribute.
  /// Dollar amount of current support collected in federal fiscal year to date
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdNum")]
  [Member(Index = 57, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidFfytdNum
  {
    get => currentSupportPaidFfytdNum;
    set => currentSupportPaidFfytdNum = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_PER attribute.
  /// Percent of current support paid in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdPer")]
  [Member(Index = 58, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CurrentSupportPaidFfytdPer
  {
    get => currentSupportPaidFfytdPer;
    set => currentSupportPaidFfytdPer = value != null ? Truncate(value, 3) : null
      ;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_RNK attribute.
  /// Ranking of office by percent of current support paid in federal fiscal 
  /// year to date.
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdRnk")]
  [Member(Index = 59, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CurrentSupportPaidFfytdRnk
  {
    get => currentSupportPaidFfytdRnk;
    set => currentSupportPaidFfytdRnk = value;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_DEN attribute.
  /// Dollar amount of current support due in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthDen")]
  [Member(Index = 60, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidMthDen
  {
    get => currentSupportPaidMthDen;
    set => currentSupportPaidMthDen = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_NUM attribute.
  /// Dollar amount of current support collected in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthNum")]
  [Member(Index = 61, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidMthNum
  {
    get => currentSupportPaidMthNum;
    set => currentSupportPaidMthNum = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_PER attribute.
  /// Percent of current support paid in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthPer")]
  [Member(Index = 62, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CurrentSupportPaidMthPer
  {
    get => currentSupportPaidMthPer;
    set => currentSupportPaidMthPer = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_RNK attribute.
  /// Ranking of office by percent of current support paid in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthRnk")]
  [Member(Index = 63, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CurrentSupportPaidMthRnk
  {
    get => currentSupportPaidMthRnk;
    set => currentSupportPaidMthRnk = value;
  }

  private int reportMonth;
  private string reportLevel;
  private string reportLevelId;
  private string type1;
  private DateTime? asOfDate;
  private int? casesWithEstReferral;
  private int? casesWithEnfReferral;
  private decimal? fullTimeEquivalent;
  private int? newOrdersEstablished;
  private int? paternitiesEstablished;
  private int? casesOpenedWithOrder;
  private int? casesOpenedWithoutOrders;
  private int? casesClosedWithOrders;
  private int? casesClosedWithoutOrders;
  private int? modifications;
  private int? incomeWithholdingsIssued;
  private int? contemptMotionFilings;
  private int? contemptOrderFilings;
  private decimal? stypeCollectionAmount;
  private decimal? stypePercentOfTotal;
  private decimal? ftypeCollectionAmount;
  private decimal? ftypePercentOfTotal;
  private decimal? itypeCollectionAmount;
  private decimal? itypePercentOfTotal;
  private decimal? utypeCollectionAmount;
  private decimal? utypePercentOfTotal;
  private decimal? ctypeCollectionAmount;
  private decimal? ctypePercentOfTotal;
  private decimal? totalCollectionAmount;
  private int? daysToOrderEstblshmntNumer;
  private int? daysToOrderEstblshmntDenom;
  private decimal? daysToOrderEstblshmntAvg;
  private int? daysToReturnOfSrvcNumer;
  private int? daysToReturnOfServiceDenom;
  private decimal? daysToReturnOfServiceAvg;
  private int? referralAging60To90Days;
  private int? referralAging91To120Days;
  private int? referralAging121To150Days;
  private int? referralAging151PlusDays;
  private int? daysToIwoPaymentNumerator;
  private int? daysToIwoPaymentDenominator;
  private decimal? daysToIwoPaymentAverage;
  private int? referralsToLegalForEst;
  private int? referralsToLegalForEnf;
  private int? caseloadCount;
  private int? casesOpened;
  private int? ncpLocatesByAddress;
  private int? ncpLocatesByEmployer;
  private int? caseClosures;
  private int? caseReviews;
  private int? petitions;
  private int? casesPayingArrearsDenominator;
  private int? casesPayingArrearsNumerator;
  private decimal? casesPayingArrearsPercent;
  private int? casesPayingArrearsRank;
  private decimal? currentSupportPaidFfytdDen;
  private decimal? currentSupportPaidFfytdNum;
  private decimal? currentSupportPaidFfytdPer;
  private int? currentSupportPaidFfytdRnk;
  private decimal? currentSupportPaidMthDen;
  private decimal? currentSupportPaidMthNum;
  private decimal? currentSupportPaidMthPer;
  private int? currentSupportPaidMthRnk;
}
