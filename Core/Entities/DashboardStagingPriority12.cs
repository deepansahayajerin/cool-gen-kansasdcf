// The source file: DASHBOARD_STAGING_PRIORITY_1_2, ID: 945116342, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

[Serializable]
public partial class DashboardStagingPriority12: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DashboardStagingPriority12()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DashboardStagingPriority12(DashboardStagingPriority12 that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DashboardStagingPriority12 Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the REPORT_MONTH attribute.
  /// Dashboard report month.  This will be the reporting year and month in a 
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
  /// Level at which data is being reported: Statewide (ST), Region (RG), Office
  /// (OF), Judicial District (JD).
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
  public const int ReportLevelId_MaxLength = 4;

  /// <summary>
  /// The value of the REPORT_LEVEL_ID attribute.
  /// Identifier of the State, Region, Office, or Judicial District for which 
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
  /// The value of the CASES_UNDER_ORDER_NUMERATOR attribute.
  /// Number of cases with an order.
  /// </summary>
  [JsonPropertyName("casesUnderOrderNumerator")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesUnderOrderNumerator
  {
    get => Get<int?>("casesUnderOrderNumerator");
    set => Set("casesUnderOrderNumerator", value);
  }

  /// <summary>
  /// The value of the CASES_UNDER_ORDER_DENOMINATOR attribute.
  /// Number of open cases.
  /// </summary>
  [JsonPropertyName("casesUnderOrderDenominator")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesUnderOrderDenominator
  {
    get => Get<int?>("casesUnderOrderDenominator");
    set => Set("casesUnderOrderDenominator", value);
  }

  /// <summary>
  /// The value of the CASES_UNDER_ORDER_PERCENT attribute.
  /// Percentage of cases under order (numerator divided by denominator).
  /// </summary>
  [JsonPropertyName("casesUnderOrderPercent")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CasesUnderOrderPercent
  {
    get => Get<decimal?>("casesUnderOrderPercent");
    set => Set("casesUnderOrderPercent", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the CASES_UNDER_ORDER_RANK attribute.
  /// Ranking of office by cases under order.
  /// </summary>
  [JsonPropertyName("casesUnderOrderRank")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesUnderOrderRank
  {
    get => Get<int?>("casesUnderOrderRank");
    set => Set("casesUnderOrderRank", value);
  }

  /// <summary>
  /// The value of the PEP_NUMERATOR attribute.
  /// Children in IV-D cases open during or at the end of the federal fiscal 
  /// year with paternity established or acknowledged.  This is reported only at
  /// the statewide level.
  /// </summary>
  [JsonPropertyName("pepNumerator")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PepNumerator
  {
    get => Get<int?>("pepNumerator");
    set => Set("pepNumerator", value);
  }

  /// <summary>
  /// The value of the PEP_DENOMINATOR attribute.
  /// Children in IV-D cases at the end of the prior federal fiscal year who 
  /// were born out of wedlock.  This is the number reported by OCSE157.
  /// </summary>
  [JsonPropertyName("pepDenominator")]
  [Member(Index = 10, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PepDenominator
  {
    get => Get<int?>("pepDenominator");
    set => Set("pepDenominator", value);
  }

  /// <summary>
  /// The value of the PEP_PERCENT attribute.
  /// Paterntity Establishment Percentage (PEP).
  /// </summary>
  [JsonPropertyName("pepPercent")]
  [Member(Index = 11, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? PepPercent
  {
    get => Get<decimal?>("pepPercent");
    set => Set("pepPercent", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_NUMERATOR attribute.
  /// The number of cases paying towards arrears during the report period.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsNumerator")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingArrearsNumerator
  {
    get => Get<int?>("casesPayingArrearsNumerator");
    set => Set("casesPayingArrearsNumerator", value);
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_DENOMINATOR attribute.
  /// Cases open at any point during the report period with arrears due.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsDenominator")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingArrearsDenominator
  {
    get => Get<int?>("casesPayingArrearsDenominator");
    set => Set("casesPayingArrearsDenominator", value);
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_PERCENT attribute.
  /// Percent of cases paying on arrears.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsPercent")]
  [Member(Index = 14, Type = MemberType.Number, Length = 9, Precision = 3, Optional
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
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingArrearsRank
  {
    get => Get<int?>("casesPayingArrearsRank");
    set => Set("casesPayingArrearsRank", value);
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_NUM attribute.
  /// Dollar amount of current support collected in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthNum")]
  [Member(Index = 16, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidMthNum
  {
    get => Get<decimal?>("currentSupportPaidMthNum");
    set => Set("currentSupportPaidMthNum", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_DEN attribute.
  /// Dollar amount of current support due in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthDen")]
  [Member(Index = 17, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidMthDen
  {
    get => Get<decimal?>("currentSupportPaidMthDen");
    set => Set("currentSupportPaidMthDen", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_PER attribute.
  /// Percent of current support paid in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthPer")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Precision = 3, Optional
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
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CurrentSupportPaidMthRnk
  {
    get => Get<int?>("currentSupportPaidMthRnk");
    set => Set("currentSupportPaidMthRnk", value);
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_NUM attribute.
  /// Dollar amount of current support collected in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdNum")]
  [Member(Index = 20, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidFfytdNum
  {
    get => Get<decimal?>("currentSupportPaidFfytdNum");
    set => Set("currentSupportPaidFfytdNum", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_DEN attribute.
  /// Dollar amount of current support due in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdDen")]
  [Member(Index = 21, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidFfytdDen
  {
    get => Get<decimal?>("currentSupportPaidFfytdDen");
    set => Set("currentSupportPaidFfytdDen", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_PER attribute.
  /// Percent of current support paid in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdPer")]
  [Member(Index = 22, Type = MemberType.Number, Length = 9, Precision = 3, Optional
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
  [Member(Index = 23, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CurrentSupportPaidFfytdRnk
  {
    get => Get<int?>("currentSupportPaidFfytdRnk");
    set => Set("currentSupportPaidFfytdRnk", value);
  }

  /// <summary>
  /// The value of the COLLECTIONS_FFYTD_TO_PRIOR_MONTH attribute.
  /// Total collections in federal fiscal year through the end of the prior 
  /// month.
  /// </summary>
  [JsonPropertyName("collectionsFfytdToPriorMonth")]
  [Member(Index = 24, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsFfytdToPriorMonth
  {
    get => Get<decimal?>("collectionsFfytdToPriorMonth");
    set => Set("collectionsFfytdToPriorMonth", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTIONS_FFYTD_ACTUAL attribute.
  /// Total collections in federal fiscal year to date.  This is the sum of the 
  /// Collections_FFYTD_to_prior_month plus Collections_in_month_actual.
  /// </summary>
  [JsonPropertyName("collectionsFfytdActual")]
  [Member(Index = 25, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsFfytdActual
  {
    get => Get<decimal?>("collectionsFfytdActual");
    set => Set("collectionsFfytdActual", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTIONS_FFYTD_PRIOR_YEAR attribute.
  /// Total collections in prior federal fiscal year through the same month.
  /// </summary>
  [JsonPropertyName("collectionsFfytdPriorYear")]
  [Member(Index = 26, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsFfytdPriorYear
  {
    get => Get<decimal?>("collectionsFfytdPriorYear");
    set => Set("collectionsFfytdPriorYear", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTIONS_FFYTD_PERCENT_CHANGE attribute.
  /// Percentage change from prior federal fiscal year to month to current 
  /// federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("collectionsFfytdPercentChange")]
  [Member(Index = 27, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CollectionsFfytdPercentChange
  {
    get => Get<decimal?>("collectionsFfytdPercentChange");
    set => Set("collectionsFfytdPercentChange", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the COLLECTIONS_FFYTD_RNK attribute.
  /// Ranking of office by percentage change of total collections federal fiscal
  /// year to date to total collections previous federal fiscal year to current
  /// month.
  /// </summary>
  [JsonPropertyName("collectionsFfytdRnk")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CollectionsFfytdRnk
  {
    get => Get<int?>("collectionsFfytdRnk");
    set => Set("collectionsFfytdRnk", value);
  }

  /// <summary>
  /// The value of the COLLECTIONS_IN_MONTH_ACTUAL attribute.
  /// Total collections in month.
  /// </summary>
  [JsonPropertyName("collectionsInMonthActual")]
  [Member(Index = 29, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsInMonthActual
  {
    get => Get<decimal?>("collectionsInMonthActual");
    set => Set("collectionsInMonthActual", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTIONS_IN_MONTH_PRIOR_YEAR attribute.
  /// Total collection in same month of previous year.
  /// </summary>
  [JsonPropertyName("collectionsInMonthPriorYear")]
  [Member(Index = 30, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsInMonthPriorYear
  {
    get => Get<decimal?>("collectionsInMonthPriorYear");
    set => Set("collectionsInMonthPriorYear", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTIONS_IN_MONTH_PERCENT_CHG attribute.
  /// Percentage change of total collections in month to total collections in 
  /// same month of previous year.
  /// </summary>
  [JsonPropertyName("collectionsInMonthPercentChg")]
  [Member(Index = 31, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CollectionsInMonthPercentChg
  {
    get => Get<decimal?>("collectionsInMonthPercentChg");
    set => Set("collectionsInMonthPercentChg", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the COLLECTIONS_IN_MONTH_RNK attribute.
  /// Ranking of office by percentage change of total collections in month to 
  /// total collections in same month of previous year.
  /// </summary>
  [JsonPropertyName("collectionsInMonthRnk")]
  [Member(Index = 32, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CollectionsInMonthRnk
  {
    get => Get<int?>("collectionsInMonthRnk");
    set => Set("collectionsInMonthRnk", value);
  }

  /// <summary>
  /// The value of the ARREARS_DISTRIBUTED_MONTH_ACTUAL attribute.
  /// Total arrears amount distributed in month.
  /// </summary>
  [JsonPropertyName("arrearsDistributedMonthActual")]
  [Member(Index = 33, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? ArrearsDistributedMonthActual
  {
    get => Get<decimal?>("arrearsDistributedMonthActual");
    set => Set("arrearsDistributedMonthActual", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the ARREARS_DISTRIBUTED_MONTH_RNK attribute.
  /// Ranking of office by total arrears amount distributed in month.
  /// </summary>
  [JsonPropertyName("arrearsDistributedMonthRnk")]
  [Member(Index = 34, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ArrearsDistributedMonthRnk
  {
    get => Get<int?>("arrearsDistributedMonthRnk");
    set => Set("arrearsDistributedMonthRnk", value);
  }

  /// <summary>
  /// The value of the ARREARS_DISTRIBUTED_FFYTD_ACTUAL attribute.
  /// Total arrears amount distributed in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("arrearsDistributedFfytdActual")]
  [Member(Index = 35, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? ArrearsDistributedFfytdActual
  {
    get => Get<decimal?>("arrearsDistributedFfytdActual");
    set => Set("arrearsDistributedFfytdActual", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the ARREARS_DISTRUBUTED_FFYTD_RNK attribute.
  /// Ranking of office by total arrears amount distributed in federal fiscal 
  /// year to date.
  /// </summary>
  [JsonPropertyName("arrearsDistrubutedFfytdRnk")]
  [Member(Index = 36, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ArrearsDistrubutedFfytdRnk
  {
    get => Get<int?>("arrearsDistrubutedFfytdRnk");
    set => Set("arrearsDistrubutedFfytdRnk", value);
  }

  /// <summary>
  /// The value of the ARREARS_DUE_ACTUAL attribute.
  /// Total amount of arreas due.
  /// </summary>
  [JsonPropertyName("arrearsDueActual")]
  [Member(Index = 37, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? ArrearsDueActual
  {
    get => Get<decimal?>("arrearsDueActual");
    set => Set("arrearsDueActual", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the ARREARS_DUE_RNK attribute.
  /// Ranking of office by total arrears due.
  /// </summary>
  [JsonPropertyName("arrearsDueRnk")]
  [Member(Index = 38, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ArrearsDueRnk
  {
    get => Get<int?>("arrearsDueRnk");
    set => Set("arrearsDueRnk", value);
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_OBLIG_CASE_NUMER attribute.
  /// Total collections in month.
  /// </summary>
  [JsonPropertyName("collectionsPerObligCaseNumer")]
  [Member(Index = 39, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsPerObligCaseNumer
  {
    get => Get<decimal?>("collectionsPerObligCaseNumer");
    set => Set("collectionsPerObligCaseNumer", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_OBLIG_CASE_DENOM attribute.
  /// Number of cases with an order.
  /// </summary>
  [JsonPropertyName("collectionsPerObligCaseDenom")]
  [Member(Index = 40, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsPerObligCaseDenom
  {
    get => Get<decimal?>("collectionsPerObligCaseDenom");
    set => Set("collectionsPerObligCaseDenom", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_OBLIG_CASE_AVG attribute.
  /// Average amount of collections per obligated case.
  /// </summary>
  [JsonPropertyName("collectionsPerObligCaseAvg")]
  [Member(Index = 41, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsPerObligCaseAvg
  {
    get => Get<decimal?>("collectionsPerObligCaseAvg");
    set => Set("collectionsPerObligCaseAvg", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_OBLIG_CASE_RNK attribute.
  /// Ranking of office by average amount of collections per obligated case.
  /// </summary>
  [JsonPropertyName("collectionsPerObligCaseRnk")]
  [Member(Index = 42, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CollectionsPerObligCaseRnk
  {
    get => Get<int?>("collectionsPerObligCaseRnk");
    set => Set("collectionsPerObligCaseRnk", value);
  }

  /// <summary>
  /// The value of the IWO_PER_OBLIG_CASE_NUMERATOR attribute.
  /// Income withholding issued.
  /// </summary>
  [JsonPropertyName("iwoPerObligCaseNumerator")]
  [Member(Index = 43, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IwoPerObligCaseNumerator
  {
    get => Get<int?>("iwoPerObligCaseNumerator");
    set => Set("iwoPerObligCaseNumerator", value);
  }

  /// <summary>
  /// The value of the IWO_PER_OBLIG_CASE_DENOMINATOR attribute.
  /// Number of cases with an order.
  /// </summary>
  [JsonPropertyName("iwoPerObligCaseDenominator")]
  [Member(Index = 44, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IwoPerObligCaseDenominator
  {
    get => Get<int?>("iwoPerObligCaseDenominator");
    set => Set("iwoPerObligCaseDenominator", value);
  }

  /// <summary>
  /// The value of the IWO_PER_OBLIG_CASE_AVERAGE attribute.
  /// Average number of income withholding orders issued.
  /// </summary>
  [JsonPropertyName("iwoPerObligCaseAverage")]
  [Member(Index = 45, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? IwoPerObligCaseAverage
  {
    get => Get<decimal?>("iwoPerObligCaseAverage");
    set => Set("iwoPerObligCaseAverage", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the IWO_PER_OBLIG_CASE_RNK attribute.
  /// Ranking of office by average number of income withholding orders issued.
  /// </summary>
  [JsonPropertyName("iwoPerObligCaseRnk")]
  [Member(Index = 46, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IwoPerObligCaseRnk
  {
    get => Get<int?>("iwoPerObligCaseRnk");
    set => Set("iwoPerObligCaseRnk", value);
  }

  /// <summary>
  /// The value of the CASES_PER_FTE_NUMERATOR attribute.
  /// Number of open cases.
  /// </summary>
  [JsonPropertyName("casesPerFteNumerator")]
  [Member(Index = 47, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPerFteNumerator
  {
    get => Get<int?>("casesPerFteNumerator");
    set => Set("casesPerFteNumerator", value);
  }

  /// <summary>
  /// The value of the CASES_PER_FTE_DENOMINATOR attribute.
  /// Number of FTE (full time equivalent).
  /// </summary>
  [JsonPropertyName("casesPerFteDenominator")]
  [Member(Index = 48, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CasesPerFteDenominator
  {
    get => Get<decimal?>("casesPerFteDenominator");
    set => Set("casesPerFteDenominator", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CASES_PER_FTE_AVERAGE attribute.
  /// Average number of cases to FTE (full time equivalent)
  /// </summary>
  [JsonPropertyName("casesPerFteAverage")]
  [Member(Index = 49, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CasesPerFteAverage
  {
    get => Get<decimal?>("casesPerFteAverage");
    set => Set("casesPerFteAverage", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CASES_PER_FTE_RANK attribute.
  /// Ranking of office by average number of cases to FTE.
  /// </summary>
  [JsonPropertyName("casesPerFteRank")]
  [Member(Index = 50, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPerFteRank
  {
    get => Get<int?>("casesPerFteRank");
    set => Set("casesPerFteRank", value);
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_FTE_NUMERATOR attribute.
  /// Total collections in month.
  /// </summary>
  [JsonPropertyName("collectionsPerFteNumerator")]
  [Member(Index = 51, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsPerFteNumerator
  {
    get => Get<decimal?>("collectionsPerFteNumerator");
    set => Set("collectionsPerFteNumerator", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_FTE_DENOMINATOR attribute.
  /// Number of FTE (full time equivalent)
  /// </summary>
  [JsonPropertyName("collectionsPerFteDenominator")]
  [Member(Index = 52, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsPerFteDenominator
  {
    get => Get<decimal?>("collectionsPerFteDenominator");
    set => Set("collectionsPerFteDenominator", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_FTE_AVERAGE attribute.
  /// Number of FTE (full time equivalent)
  /// </summary>
  [JsonPropertyName("collectionsPerFteAverage")]
  [Member(Index = 53, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsPerFteAverage
  {
    get => Get<decimal?>("collectionsPerFteAverage");
    set => Set("collectionsPerFteAverage", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_FTE_RANK attribute.
  /// Ranking of office by average amount of collections per FTE.
  /// </summary>
  [JsonPropertyName("collectionsPerFteRank")]
  [Member(Index = 54, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CollectionsPerFteRank
  {
    get => Get<int?>("collectionsPerFteRank");
    set => Set("collectionsPerFteRank", value);
  }

  /// <summary>
  /// The value of the CASES_PAYING_NUMERATOR attribute.
  /// Number of cases paying in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("casesPayingNumerator")]
  [Member(Index = 55, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingNumerator
  {
    get => Get<int?>("casesPayingNumerator");
    set => Set("casesPayingNumerator", value);
  }

  /// <summary>
  /// The value of the CASES_PAYING_DENOMINATOR attribute.
  /// Number of open cases.
  /// </summary>
  [JsonPropertyName("casesPayingDenominator")]
  [Member(Index = 56, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingDenominator
  {
    get => Get<int?>("casesPayingDenominator");
    set => Set("casesPayingDenominator", value);
  }

  /// <summary>
  /// The value of the CASES_PAYING_PERCENT attribute.
  /// Percentage of cases paying in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("casesPayingPercent")]
  [Member(Index = 57, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CasesPayingPercent
  {
    get => Get<decimal?>("casesPayingPercent");
    set => Set("casesPayingPercent", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the CASES_PAYING_RANK attribute.
  /// Ranking of office by percentage of cases paying in federal fiscal year to 
  /// date.
  /// </summary>
  [JsonPropertyName("casesPayingRank")]
  [Member(Index = 58, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingRank
  {
    get => Get<int?>("casesPayingRank");
    set => Set("casesPayingRank", value);
  }

  /// <summary>
  /// The value of the PEP_RANK attribute.
  /// Ranking of office by Paternity Establishment (PEP) percentage.
  /// </summary>
  [JsonPropertyName("pepRank")]
  [Member(Index = 59, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PepRank
  {
    get => Get<int?>("pepRank");
    set => Set("pepRank", value);
  }

  /// <summary>Length of the CONTRACTOR_NUMBER attribute.</summary>
  public const int ContractorNumber_MaxLength = 2;

  /// <summary>
  /// The value of the CONTRACTOR_NUMBER attribute.
  /// ID of the firm contracted to provide privatized Child Support services.
  /// </summary>
  [JsonPropertyName("contractorNumber")]
  [Member(Index = 60, Type = MemberType.Char, Length
    = ContractorNumber_MaxLength, Optional = true)]
  public string ContractorNumber
  {
    get => Get<string>("contractorNumber");
    set => Set(
      "contractorNumber",
      TrimEnd(Substring(value, 1, ContractorNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the PREV_YR_PEP_NUMERATOR attribute.
  /// The IV-D Paternity Establishment numerator from prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrPepNumerator")]
  [Member(Index = 61, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrevYrPepNumerator
  {
    get => Get<int?>("prevYrPepNumerator");
    set => Set("prevYrPepNumerator", value);
  }

  /// <summary>
  /// The value of the PREV_YR_PEP_DENOMINATOR attribute.
  /// The IV-D Paternity Establishment denominator from prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrPepDenominator")]
  [Member(Index = 62, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrevYrPepDenominator
  {
    get => Get<int?>("prevYrPepDenominator");
    set => Set("prevYrPepDenominator", value);
  }

  /// <summary>
  /// The value of the PREV_YR_PEP_PERCENT attribute.
  /// The IV-D Paternity Establishment Percentage for prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrPepPercent")]
  [Member(Index = 63, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? PrevYrPepPercent
  {
    get => Get<decimal?>("prevYrPepPercent");
    set => Set("prevYrPepPercent", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the PERCENT_CHG_BETWEEN_YRS_PEP attribute.
  /// The IV-D Paternity Establishment Percentage change between prior year 
  /// month and current same month.
  /// </summary>
  [JsonPropertyName("percentChgBetweenYrsPep")]
  [Member(Index = 64, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? PercentChgBetweenYrsPep
  {
    get => Get<decimal?>("percentChgBetweenYrsPep");
    set => Set("percentChgBetweenYrsPep", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the PREV_YR_CASE_NUMERATOR attribute.
  /// The cases under order numerator from prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrCaseNumerator")]
  [Member(Index = 65, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrevYrCaseNumerator
  {
    get => Get<int?>("prevYrCaseNumerator");
    set => Set("prevYrCaseNumerator", value);
  }

  /// <summary>
  /// The value of the PREV_YR_CASE_DENOMINATOR attribute.
  /// The cases under order denominator from prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrCaseDenominator")]
  [Member(Index = 66, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrevYrCaseDenominator
  {
    get => Get<int?>("prevYrCaseDenominator");
    set => Set("prevYrCaseDenominator", value);
  }

  /// <summary>
  /// The value of the CASES_UNDR_ORDR_PREV_YR_PCT attribute.
  /// The cases under order percentage for prior year same month.
  /// </summary>
  [JsonPropertyName("casesUndrOrdrPrevYrPct")]
  [Member(Index = 67, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CasesUndrOrdrPrevYrPct
  {
    get => Get<decimal?>("casesUndrOrdrPrevYrPct");
    set => Set("casesUndrOrdrPrevYrPct", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the PCT_CHG_BTWEN_YRS_CASE_UNDR_ORDR attribute.
  /// The cases under order percentage change between prior year month and 
  /// current same month.
  /// </summary>
  [JsonPropertyName("pctChgBtwenYrsCaseUndrOrdr")]
  [Member(Index = 68, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? PctChgBtwenYrsCaseUndrOrdr
  {
    get => Get<decimal?>("pctChgBtwenYrsCaseUndrOrdr");
    set => Set("pctChgBtwenYrsCaseUndrOrdr", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the PREV_YR_CUR_SUPPRT_PAID_NUMTR attribute.
  /// The current support paid numerator from prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrCurSupprtPaidNumtr")]
  [Member(Index = 69, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? PrevYrCurSupprtPaidNumtr
  {
    get => Get<decimal?>("prevYrCurSupprtPaidNumtr");
    set => Set("prevYrCurSupprtPaidNumtr", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the PREV_YR_CUR_SUPPRT_PAID_DENOM attribute.
  /// The current support paid denominator from prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrCurSupprtPaidDenom")]
  [Member(Index = 70, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? PrevYrCurSupprtPaidDenom
  {
    get => Get<decimal?>("prevYrCurSupprtPaidDenom");
    set => Set("prevYrCurSupprtPaidDenom", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CUR_SUPPRT_PD_PREV_YR_PCT attribute.
  /// The percentage of current support paid for prior year same month.
  /// </summary>
  [JsonPropertyName("curSupprtPdPrevYrPct")]
  [Member(Index = 71, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CurSupprtPdPrevYrPct
  {
    get => Get<decimal?>("curSupprtPdPrevYrPct");
    set => Set("curSupprtPdPrevYrPct", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the PCT_CHG_BTWEN_YRS_CUR_SUPT_PD attribute.
  /// The current support paid percentage change between prior year month and 
  /// current same month.
  /// </summary>
  [JsonPropertyName("pctChgBtwenYrsCurSuptPd")]
  [Member(Index = 72, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? PctChgBtwenYrsCurSuptPd
  {
    get => Get<decimal?>("pctChgBtwenYrsCurSuptPd");
    set => Set("pctChgBtwenYrsCurSuptPd", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the PRV_YR_CASES_PAID_ARREARS_NUMTR attribute.
  /// The cases paying on arrears numerator from prior year same month.
  /// </summary>
  [JsonPropertyName("prvYrCasesPaidArrearsNumtr")]
  [Member(Index = 73, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrvYrCasesPaidArrearsNumtr
  {
    get => Get<int?>("prvYrCasesPaidArrearsNumtr");
    set => Set("prvYrCasesPaidArrearsNumtr", value);
  }

  /// <summary>
  /// The value of the PRV_YR_CASES_PAID_ARREARS_DENOM attribute.
  /// The cases paying on arrears denominator from prior year same month.
  /// </summary>
  [JsonPropertyName("prvYrCasesPaidArrearsDenom")]
  [Member(Index = 74, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrvYrCasesPaidArrearsDenom
  {
    get => Get<int?>("prvYrCasesPaidArrearsDenom");
    set => Set("prvYrCasesPaidArrearsDenom", value);
  }

  /// <summary>
  /// The value of the CASES_PAY_ARREARS_PRV_YR_PCT attribute.
  /// The percentage of cases paying arrears for prior year same month.
  /// </summary>
  [JsonPropertyName("casesPayArrearsPrvYrPct")]
  [Member(Index = 75, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CasesPayArrearsPrvYrPct
  {
    get => Get<decimal?>("casesPayArrearsPrvYrPct");
    set => Set("casesPayArrearsPrvYrPct", Truncate(value, 3));
  }

  /// <summary>
  /// The value of the PCT_CHG_BTWEN_YRS_CASES_PAY_ARRS attribute.
  /// The cases paying arrears percentage change between prior year month and 
  /// current same month.
  /// </summary>
  [JsonPropertyName("pctChgBtwenYrsCasesPayArrs")]
  [Member(Index = 76, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? PctChgBtwenYrsCasesPayArrs
  {
    get => Get<decimal?>("pctChgBtwenYrsCasesPayArrs");
    set => Set("pctChgBtwenYrsCasesPayArrs", Truncate(value, 3));
  }
}
