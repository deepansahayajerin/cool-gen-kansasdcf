// The source file: DASHBOARD_STAGING_PRIORITY_35, ID: 945147877, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Dashbord -- final production location from which SSRS reports will pull data
/// for display on the CSS Dashboard
/// </summary>
[Serializable]
public partial class DashboardStagingPriority35: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DashboardStagingPriority35()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DashboardStagingPriority35(DashboardStagingPriority35 that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DashboardStagingPriority35 Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

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
    get => Get<int?>("reportMonth") ?? 0;
    set => Set("reportMonth", value == 0 ? null : value as int?);
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
    get => Get<string>("reportLevel") ?? "";
    set => Set(
      "reportLevel", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ReportLevel_MaxLength)));
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
    get => Get<string>("reportLevelId") ?? "";
    set => Set(
      "reportLevelId", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ReportLevelId_MaxLength)));
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

  /// <summary>
  /// The value of the AS_OF_DATE attribute.
  /// Date through which calculations in this row apply.
  /// </summary>
  [JsonPropertyName("asOfDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? AsOfDate
  {
    get => Get<DateTime?>("asOfDate");
    set => Set("asOfDate", value);
  }

  /// <summary>
  /// The value of the CASES_WITH_EST_REFERRAL attribute.
  /// Number of cases with an active Establishment referral.
  /// </summary>
  [JsonPropertyName("casesWithEstReferral")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesWithEstReferral
  {
    get => Get<int?>("casesWithEstReferral");
    set => Set("casesWithEstReferral", value);
  }

  /// <summary>
  /// The value of the CASES_WITH_ENF_REFERRAL attribute.
  /// Number of cases with an active Enforcement referral.
  /// </summary>
  [JsonPropertyName("casesWithEnfReferral")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesWithEnfReferral
  {
    get => Get<int?>("casesWithEnfReferral");
    set => Set("casesWithEnfReferral", value);
  }

  /// <summary>
  /// The value of the FULL_TIME_EQUIVALENT attribute.
  /// Full Time Equivalent.
  /// </summary>
  [JsonPropertyName("fullTimeEquivalent")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? FullTimeEquivalent
  {
    get => Get<decimal?>("fullTimeEquivalent");
    set => Set("fullTimeEquivalent", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the NEW_ORDERS_ESTABLISHED attribute.
  /// Number of new court orders established.
  /// </summary>
  [JsonPropertyName("newOrdersEstablished")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NewOrdersEstablished
  {
    get => Get<int?>("newOrdersEstablished");
    set => Set("newOrdersEstablished", value);
  }

  /// <summary>
  /// The value of the PATERNITIES_ESTABLISHED attribute.
  /// Number of children with paternity established by court order.
  /// </summary>
  [JsonPropertyName("paternitiesEstablished")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PaternitiesEstablished
  {
    get => Get<int?>("paternitiesEstablished");
    set => Set("paternitiesEstablished", value);
  }

  /// <summary>
  /// The value of the CASES_OPENED_WITH_ORDER attribute.
  /// Number of cases opened that meet the definition of a case under order.
  /// </summary>
  [JsonPropertyName("casesOpenedWithOrder")]
  [Member(Index = 10, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesOpenedWithOrder
  {
    get => Get<int?>("casesOpenedWithOrder");
    set => Set("casesOpenedWithOrder", value);
  }

  /// <summary>
  /// The value of the CASES_OPENED_WITHOUT_ORDERS attribute.
  /// Number of cases opened that do not meet the definition of a case under 
  /// order.
  /// </summary>
  [JsonPropertyName("casesOpenedWithoutOrders")]
  [Member(Index = 11, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesOpenedWithoutOrders
  {
    get => Get<int?>("casesOpenedWithoutOrders");
    set => Set("casesOpenedWithoutOrders", value);
  }

  /// <summary>
  /// The value of the CASES_CLOSED_WITH_ORDERS attribute.
  /// Number of cases closed that were under order.
  /// </summary>
  [JsonPropertyName("casesClosedWithOrders")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesClosedWithOrders
  {
    get => Get<int?>("casesClosedWithOrders");
    set => Set("casesClosedWithOrders", value);
  }

  /// <summary>
  /// The value of the CASES_CLOSED_WITHOUT_ORDERS attribute.
  /// Number of cases closed that were not under order.
  /// </summary>
  [JsonPropertyName("casesClosedWithoutOrders")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesClosedWithoutOrders
  {
    get => Get<int?>("casesClosedWithoutOrders");
    set => Set("casesClosedWithoutOrders", value);
  }

  /// <summary>
  /// The value of the MODIFICATIONS attribute.
  /// Number of modifications reviews completed.
  /// </summary>
  [JsonPropertyName("modifications")]
  [Member(Index = 14, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? Modifications
  {
    get => Get<int?>("modifications");
    set => Set("modifications", value);
  }

  /// <summary>
  /// The value of the INCOME_WITHHOLDINGS_ISSUED attribute.
  /// Number of income withholdings issued.
  /// </summary>
  [JsonPropertyName("incomeWithholdingsIssued")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IncomeWithholdingsIssued
  {
    get => Get<int?>("incomeWithholdingsIssued");
    set => Set("incomeWithholdingsIssued", value);
  }

  /// <summary>
  /// The value of the CONTEMPT_MOTION_FILINGS attribute.
  /// Number of court orders with a Motion for Contempt filed.
  /// </summary>
  [JsonPropertyName("contemptMotionFilings")]
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ContemptMotionFilings
  {
    get => Get<int?>("contemptMotionFilings");
    set => Set("contemptMotionFilings", value);
  }

  /// <summary>
  /// The value of the CONTEMPT_ORDER_FILINGS attribute.
  /// Number of court orders with a Order for Contempt filed.
  /// </summary>
  [JsonPropertyName("contemptOrderFilings")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ContemptOrderFilings
  {
    get => Get<int?>("contemptOrderFilings");
    set => Set("contemptOrderFilings", value);
  }

  /// <summary>
  /// The value of the S_TYPE_COLLECTION_AMOUNT attribute.
  /// Dollar amount of S type collections during the period.
  /// </summary>
  [JsonPropertyName("stypeCollectionAmount")]
  [Member(Index = 18, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? StypeCollectionAmount
  {
    get => Get<decimal?>("stypeCollectionAmount");
    set => Set("stypeCollectionAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the S_TYPE_PERCENT_OF_TOTAL attribute.
  /// Percentage of total collections attributable to S type collections.
  /// </summary>
  [JsonPropertyName("stypePercentOfTotal")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? StypePercentOfTotal
  {
    get => Get<decimal?>("stypePercentOfTotal");
    set => Set("stypePercentOfTotal", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the F_TYPE_COLLECTION_AMOUNT attribute.
  /// Dollar amount of F type collections during the period.
  /// </summary>
  [JsonPropertyName("ftypeCollectionAmount")]
  [Member(Index = 20, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? FtypeCollectionAmount
  {
    get => Get<decimal?>("ftypeCollectionAmount");
    set => Set("ftypeCollectionAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the F_TYPE_PERCENT_OF_TOTAL attribute.
  /// Percentage of total collections attributable to F type collections.
  /// </summary>
  [JsonPropertyName("ftypePercentOfTotal")]
  [Member(Index = 21, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? FtypePercentOfTotal
  {
    get => Get<decimal?>("ftypePercentOfTotal");
    set => Set("ftypePercentOfTotal", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the I_TYPE_COLLECTION_AMOUNT attribute.
  /// Dollar amount of I type collections during the period.
  /// </summary>
  [JsonPropertyName("itypeCollectionAmount")]
  [Member(Index = 22, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? ItypeCollectionAmount
  {
    get => Get<decimal?>("itypeCollectionAmount");
    set => Set("itypeCollectionAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the I_TYPE_PERCENT_OF_TOTAL attribute.
  /// Percentage of total collections attributable to I type collections.
  /// </summary>
  [JsonPropertyName("itypePercentOfTotal")]
  [Member(Index = 23, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? ItypePercentOfTotal
  {
    get => Get<decimal?>("itypePercentOfTotal");
    set => Set("itypePercentOfTotal", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the U_TYPE_COLLECTION_AMOUNT attribute.
  /// Dollar amount of U type collections during the period.
  /// </summary>
  [JsonPropertyName("utypeCollectionAmount")]
  [Member(Index = 24, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? UtypeCollectionAmount
  {
    get => Get<decimal?>("utypeCollectionAmount");
    set => Set("utypeCollectionAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the U_TYPE_PERCENT_OF_TOTAL attribute.
  /// Percentage of total collections attributable to U type collections.
  /// </summary>
  [JsonPropertyName("utypePercentOfTotal")]
  [Member(Index = 25, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? UtypePercentOfTotal
  {
    get => Get<decimal?>("utypePercentOfTotal");
    set => Set("utypePercentOfTotal", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the C_TYPE_COLLECTION_AMOUNT attribute.
  /// Dollar amount of C type collections during the period.
  /// </summary>
  [JsonPropertyName("ctypeCollectionAmount")]
  [Member(Index = 26, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CtypeCollectionAmount
  {
    get => Get<decimal?>("ctypeCollectionAmount");
    set => Set("ctypeCollectionAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the C_TYPE_PERCENT_OF_TOTAL attribute.
  /// Percentage of total collections attributable to C type collections.
  /// </summary>
  [JsonPropertyName("ctypePercentOfTotal")]
  [Member(Index = 27, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CtypePercentOfTotal
  {
    get => Get<decimal?>("ctypePercentOfTotal");
    set => Set("ctypePercentOfTotal", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the TOTAL_COLLECTION_AMOUNT attribute.
  /// Total dollar amount of collections during the period.
  /// </summary>
  [JsonPropertyName("totalCollectionAmount")]
  [Member(Index = 28, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? TotalCollectionAmount
  {
    get => Get<decimal?>("totalCollectionAmount");
    set => Set("totalCollectionAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the DAYS_TO_ORDER_ESTBLSHMNT_NUMER attribute.
  /// Total number of days between referral and order establishment.
  /// </summary>
  [JsonPropertyName("daysToOrderEstblshmntNumer")]
  [Member(Index = 29, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysToOrderEstblshmntNumer
  {
    get => Get<int?>("daysToOrderEstblshmntNumer");
    set => Set("daysToOrderEstblshmntNumer", value);
  }

  /// <summary>
  /// The value of the DAYS_TO_ORDER_ESTBLSHMNT_DENOM attribute.
  /// Total number of referrals.
  /// </summary>
  [JsonPropertyName("daysToOrderEstblshmntDenom")]
  [Member(Index = 30, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysToOrderEstblshmntDenom
  {
    get => Get<int?>("daysToOrderEstblshmntDenom");
    set => Set("daysToOrderEstblshmntDenom", value);
  }

  /// <summary>
  /// The value of the DAYS_TO_ORDER_ESTBLSHMNT_AVG attribute.
  /// Average number of days between referral and order establishment.
  /// </summary>
  [JsonPropertyName("daysToOrderEstblshmntAvg")]
  [Member(Index = 31, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? DaysToOrderEstblshmntAvg
  {
    get => Get<decimal?>("daysToOrderEstblshmntAvg");
    set => Set("daysToOrderEstblshmntAvg", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the DAYS_TO_RETURN_OF_SRVC_NUMER attribute.
  /// Total number of days between referral and return of service.
  /// </summary>
  [JsonPropertyName("daysToReturnOfSrvcNumer")]
  [Member(Index = 32, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysToReturnOfSrvcNumer
  {
    get => Get<int?>("daysToReturnOfSrvcNumer");
    set => Set("daysToReturnOfSrvcNumer", value);
  }

  /// <summary>
  /// The value of the DAYS_TO_RETURN_OF_SERVICE_DENOM attribute.
  /// Total number of referrals.
  /// </summary>
  [JsonPropertyName("daysToReturnOfServiceDenom")]
  [Member(Index = 33, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysToReturnOfServiceDenom
  {
    get => Get<int?>("daysToReturnOfServiceDenom");
    set => Set("daysToReturnOfServiceDenom", value);
  }

  /// <summary>
  /// The value of the DAYS_TO_RETURN_OF_SERVICE_AVG attribute.
  /// Average number of days between referral and return of service.
  /// </summary>
  [JsonPropertyName("daysToReturnOfServiceAvg")]
  [Member(Index = 34, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? DaysToReturnOfServiceAvg
  {
    get => Get<decimal?>("daysToReturnOfServiceAvg");
    set => Set("daysToReturnOfServiceAvg", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the REFERRAL_AGING_60_TO_90_DAYS attribute.
  /// Number of referrals remaining unprocessed 60 to 90 days after creation.
  /// </summary>
  [JsonPropertyName("referralAging60To90Days")]
  [Member(Index = 35, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ReferralAging60To90Days
  {
    get => Get<int?>("referralAging60To90Days");
    set => Set("referralAging60To90Days", value);
  }

  /// <summary>
  /// The value of the REFERRAL_AGING_91_TO_120_DAYS attribute.
  /// Number of referrals remaining unprocessed 91 to 120 days after creation.
  /// </summary>
  [JsonPropertyName("referralAging91To120Days")]
  [Member(Index = 36, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ReferralAging91To120Days
  {
    get => Get<int?>("referralAging91To120Days");
    set => Set("referralAging91To120Days", value);
  }

  /// <summary>
  /// The value of the REFERRAL_AGING_121_TO_150_DAYS attribute.
  /// Number of referrals remaining unprocessed 121 to 150 days after creation.
  /// </summary>
  [JsonPropertyName("referralAging121To150Days")]
  [Member(Index = 37, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ReferralAging121To150Days
  {
    get => Get<int?>("referralAging121To150Days");
    set => Set("referralAging121To150Days", value);
  }

  /// <summary>
  /// The value of the REFERRAL_AGING_151_PLUS_DAYS attribute.
  /// Number of referrals remaining unprocessed 151 or more days after creation.
  /// </summary>
  [JsonPropertyName("referralAging151PlusDays")]
  [Member(Index = 38, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ReferralAging151PlusDays
  {
    get => Get<int?>("referralAging151PlusDays");
    set => Set("referralAging151PlusDays", value);
  }

  /// <summary>
  /// The value of the DAYS_TO_IWO_PAYMENT_NUMERATOR attribute.
  /// Total number of days between IWO issuance and I type payment.
  /// </summary>
  [JsonPropertyName("daysToIwoPaymentNumerator")]
  [Member(Index = 39, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysToIwoPaymentNumerator
  {
    get => Get<int?>("daysToIwoPaymentNumerator");
    set => Set("daysToIwoPaymentNumerator", value);
  }

  /// <summary>
  /// The value of the DAYS_TO_IWO_PAYMENT_DENOMINATOR attribute.
  /// Total number of IWOs issued.
  /// </summary>
  [JsonPropertyName("daysToIwoPaymentDenominator")]
  [Member(Index = 40, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysToIwoPaymentDenominator
  {
    get => Get<int?>("daysToIwoPaymentDenominator");
    set => Set("daysToIwoPaymentDenominator", value);
  }

  /// <summary>
  /// The value of the DAYS_TO_IWO_PAYMENT_AVERAGE attribute.
  /// Average number of days between IWO issuance and I type payment.
  /// </summary>
  [JsonPropertyName("daysToIwoPaymentAverage")]
  [Member(Index = 41, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? DaysToIwoPaymentAverage
  {
    get => Get<decimal?>("daysToIwoPaymentAverage");
    set => Set("daysToIwoPaymentAverage", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the REFERRALS_TO_LEGAL_FOR_EST attribute.
  /// Number of EST referrals sent by a caseworker.
  /// </summary>
  [JsonPropertyName("referralsToLegalForEst")]
  [Member(Index = 42, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ReferralsToLegalForEst
  {
    get => Get<int?>("referralsToLegalForEst");
    set => Set("referralsToLegalForEst", value);
  }

  /// <summary>
  /// The value of the REFERRALS_TO_LEGAL_FOR_ENF attribute.
  /// Number of ENF referrals sent by a caseworker.
  /// </summary>
  [JsonPropertyName("referralsToLegalForEnf")]
  [Member(Index = 43, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ReferralsToLegalForEnf
  {
    get => Get<int?>("referralsToLegalForEnf");
    set => Set("referralsToLegalForEnf", value);
  }

  /// <summary>
  /// The value of the CASELOAD_COUNT attribute.
  /// Number of cases assigned to a caseworker or attorney.
  /// </summary>
  [JsonPropertyName("caseloadCount")]
  [Member(Index = 44, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CaseloadCount
  {
    get => Get<int?>("caseloadCount");
    set => Set("caseloadCount", value);
  }

  /// <summary>
  /// The value of the CASES_OPENED attribute.
  /// Number of cases opened during the reporting period.
  /// </summary>
  [JsonPropertyName("casesOpened")]
  [Member(Index = 45, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesOpened
  {
    get => Get<int?>("casesOpened");
    set => Set("casesOpened", value);
  }

  /// <summary>
  /// The value of the NCP_LOCATES_BY_ADDRESS attribute.
  /// Number of NCPs located by address.
  /// </summary>
  [JsonPropertyName("ncpLocatesByAddress")]
  [Member(Index = 46, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NcpLocatesByAddress
  {
    get => Get<int?>("ncpLocatesByAddress");
    set => Set("ncpLocatesByAddress", value);
  }

  /// <summary>
  /// The value of the NCP_LOCATES_BY_EMPLOYER attribute.
  /// Number of NCPs located by employment.
  /// </summary>
  [JsonPropertyName("ncpLocatesByEmployer")]
  [Member(Index = 47, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NcpLocatesByEmployer
  {
    get => Get<int?>("ncpLocatesByEmployer");
    set => Set("ncpLocatesByEmployer", value);
  }

  /// <summary>
  /// The value of the CASE_CLOSURES attribute.
  /// Number of cases closed during the reporting period.
  /// </summary>
  [JsonPropertyName("caseClosures")]
  [Member(Index = 48, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CaseClosures
  {
    get => Get<int?>("caseClosures");
    set => Set("caseClosures", value);
  }

  /// <summary>
  /// The value of the CASE_REVIEWS attribute.
  /// Number of case reviews completed during the reporting period.
  /// </summary>
  [JsonPropertyName("caseReviews")]
  [Member(Index = 49, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CaseReviews
  {
    get => Get<int?>("caseReviews");
    set => Set("caseReviews", value);
  }

  /// <summary>
  /// The value of the PETITIONS attribute.
  /// The number of petitions a worker has created.
  /// </summary>
  [JsonPropertyName("petitions")]
  [Member(Index = 50, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? Petitions
  {
    get => Get<int?>("petitions");
    set => Set("petitions", value);
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_DENOMINATOR attribute.
  /// Cases open at any point during the report period with arrears due.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsDenominator")]
  [Member(Index = 51, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingArrearsDenominator
  {
    get => Get<int?>("casesPayingArrearsDenominator");
    set => Set("casesPayingArrearsDenominator", value);
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_NUMERATOR attribute.
  /// The number of cases paying towards arrears during the report period.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsNumerator")]
  [Member(Index = 52, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingArrearsNumerator
  {
    get => Get<int?>("casesPayingArrearsNumerator");
    set => Set("casesPayingArrearsNumerator", value);
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_PERCENT attribute.
  /// Percent of cases paying on arrears.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsPercent")]
  [Member(Index = 53, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CasesPayingArrearsPercent
  {
    get => Get<decimal?>("casesPayingArrearsPercent");
    set => Set("casesPayingArrearsPercent", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_RANK attribute.
  /// Ranking of office by cases paying on arrears.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsRank")]
  [Member(Index = 54, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingArrearsRank
  {
    get => Get<int?>("casesPayingArrearsRank");
    set => Set("casesPayingArrearsRank", value);
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_DEN attribute.
  /// Dollar amount of current support due in federal fiscal year to date.	
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdDen")]
  [Member(Index = 55, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidFfytdDen
  {
    get => Get<decimal?>("currentSupportPaidFfytdDen");
    set => Set("currentSupportPaidFfytdDen", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_NUM attribute.
  /// Dollar amount of current support collected in federal fiscal year to date
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdNum")]
  [Member(Index = 56, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidFfytdNum
  {
    get => Get<decimal?>("currentSupportPaidFfytdNum");
    set => Set("currentSupportPaidFfytdNum", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_PER attribute.
  /// Percent of current support paid in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdPer")]
  [Member(Index = 57, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CurrentSupportPaidFfytdPer
  {
    get => Get<decimal?>("currentSupportPaidFfytdPer");
    set => Set("currentSupportPaidFfytdPer", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_RNK attribute.
  /// Ranking of office by percent of current support paid in federal fiscal 
  /// year to date.
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdRnk")]
  [Member(Index = 58, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CurrentSupportPaidFfytdRnk
  {
    get => Get<int?>("currentSupportPaidFfytdRnk");
    set => Set("currentSupportPaidFfytdRnk", value);
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_DEN attribute.
  /// Dollar amount of current support due in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthDen")]
  [Member(Index = 59, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidMthDen
  {
    get => Get<decimal?>("currentSupportPaidMthDen");
    set => Set("currentSupportPaidMthDen", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_NUM attribute.
  /// Dollar amount of current support collected in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthNum")]
  [Member(Index = 60, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidMthNum
  {
    get => Get<decimal?>("currentSupportPaidMthNum");
    set => Set("currentSupportPaidMthNum", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_PER attribute.
  /// Percent of current support paid in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthPer")]
  [Member(Index = 61, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CurrentSupportPaidMthPer
  {
    get => Get<decimal?>("currentSupportPaidMthPer");
    set => Set("currentSupportPaidMthPer", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_RNK attribute.
  /// Ranking of office by percent of current support paid in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthRnk")]
  [Member(Index = 62, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CurrentSupportPaidMthRnk
  {
    get => Get<int?>("currentSupportPaidMthRnk");
    set => Set("currentSupportPaidMthRnk", value);
  }
}
