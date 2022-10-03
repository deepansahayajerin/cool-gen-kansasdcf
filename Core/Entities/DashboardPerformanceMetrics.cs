// The source file: DASHBOARD_PERFORMANCE_METRICS, ID: 945116278, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

[Serializable]
public partial class DashboardPerformanceMetrics: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DashboardPerformanceMetrics()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DashboardPerformanceMetrics(DashboardPerformanceMetrics that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DashboardPerformanceMetrics Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DashboardPerformanceMetrics that)
  {
    base.Assign(that);
    reportMonth = that.reportMonth;
    reportLevel = that.reportLevel;
    reportLevelId = that.reportLevelId;
    type1 = that.type1;
    asOfDate = that.asOfDate;
    casesUnderOrderNumerator = that.casesUnderOrderNumerator;
    casesUnderOrderDenominator = that.casesUnderOrderDenominator;
    casesUnderOrderPercent = that.casesUnderOrderPercent;
    casesUnderOrderRank = that.casesUnderOrderRank;
    pepNumerator = that.pepNumerator;
    pepDenominator = that.pepDenominator;
    pepPercent = that.pepPercent;
    casesPayingArrearsNumerator = that.casesPayingArrearsNumerator;
    casesPayingArrearsDenominator = that.casesPayingArrearsDenominator;
    casesPayingArrearsPercent = that.casesPayingArrearsPercent;
    casesPayingArrearsRank = that.casesPayingArrearsRank;
    currentSupportPaidMthNum = that.currentSupportPaidMthNum;
    currentSupportPaidMthDen = that.currentSupportPaidMthDen;
    currentSupportPaidMthPer = that.currentSupportPaidMthPer;
    currentSupportPaidMthRnk = that.currentSupportPaidMthRnk;
    currentSupportPaidFfytdNum = that.currentSupportPaidFfytdNum;
    currentSupportPaidFfytdDen = that.currentSupportPaidFfytdDen;
    currentSupportPaidFfytdPer = that.currentSupportPaidFfytdPer;
    currentSupportPaidFfytdRnk = that.currentSupportPaidFfytdRnk;
    collectionsFfytdToPriorMonth = that.collectionsFfytdToPriorMonth;
    collectionsFfytdActual = that.collectionsFfytdActual;
    collectionsFfytdPriorYear = that.collectionsFfytdPriorYear;
    collectionsFfytdPercentChange = that.collectionsFfytdPercentChange;
    collectionsFfytdRnk = that.collectionsFfytdRnk;
    collectionsInMonthActual = that.collectionsInMonthActual;
    collectionsInMonthPriorYear = that.collectionsInMonthPriorYear;
    collectionsInMonthPercentChg = that.collectionsInMonthPercentChg;
    collectionsInMonthRnk = that.collectionsInMonthRnk;
    arrearsDistributedMonthActual = that.arrearsDistributedMonthActual;
    arrearsDistributedMonthRnk = that.arrearsDistributedMonthRnk;
    arrearsDistributedFfytdActual = that.arrearsDistributedFfytdActual;
    arrearsDistrubutedFfytdRnk = that.arrearsDistrubutedFfytdRnk;
    arrearsDueActual = that.arrearsDueActual;
    arrearsDueRnk = that.arrearsDueRnk;
    collectionsPerObligCaseNumer = that.collectionsPerObligCaseNumer;
    collectionsPerObligCaseDenom = that.collectionsPerObligCaseDenom;
    collectionsPerObligCaseAvg = that.collectionsPerObligCaseAvg;
    collectionsPerObligCaseRnk = that.collectionsPerObligCaseRnk;
    iwoPerObligCaseNumerator = that.iwoPerObligCaseNumerator;
    iwoPerObligCaseDenominator = that.iwoPerObligCaseDenominator;
    iwoPerObligCaseAverage = that.iwoPerObligCaseAverage;
    iwoPerObligCaseRnk = that.iwoPerObligCaseRnk;
    casesPerFteNumerator = that.casesPerFteNumerator;
    casesPerFteDenominator = that.casesPerFteDenominator;
    casesPerFteAverage = that.casesPerFteAverage;
    casesPerFteRank = that.casesPerFteRank;
    collectionsPerFteNumerator = that.collectionsPerFteNumerator;
    collectionsPerFteDenominator = that.collectionsPerFteDenominator;
    collectionsPerFteAverage = that.collectionsPerFteAverage;
    collectionsPerFteRank = that.collectionsPerFteRank;
    casesPayingNumerator = that.casesPayingNumerator;
    casesPayingDenominator = that.casesPayingDenominator;
    casesPayingPercent = that.casesPayingPercent;
    casesPayingRank = that.casesPayingRank;
    pepRank = that.pepRank;
    contractorNumber = that.contractorNumber;
    prevYrPepNumerator = that.prevYrPepNumerator;
    prevYrPepDenominator = that.prevYrPepDenominator;
    prevYrPepPercent = that.prevYrPepPercent;
    percentChgBetweenYrsPep = that.percentChgBetweenYrsPep;
    prevYrCaseNumerator = that.prevYrCaseNumerator;
    prevYrCaseDenominator = that.prevYrCaseDenominator;
    casesUndrOrdrPrevYrPct = that.casesUndrOrdrPrevYrPct;
    pctChgBtwenYrsCaseUndrOrdr = that.pctChgBtwenYrsCaseUndrOrdr;
    prevYrCurSupprtPaidNumtr = that.prevYrCurSupprtPaidNumtr;
    prevYrCurSupprtPaidDenom = that.prevYrCurSupprtPaidDenom;
    curSupprtPdPrevYrPct = that.curSupprtPdPrevYrPct;
    pctChgBtwenYrsCurSuptPd = that.pctChgBtwenYrsCurSuptPd;
    prvYrCasesPaidArrearsNumtr = that.prvYrCasesPaidArrearsNumtr;
    prvYrCasesPaidArrearsDenom = that.prvYrCasesPaidArrearsDenom;
    casesPayArrearsPrvYrPct = that.casesPayArrearsPrvYrPct;
    pctChgBtwenYrsCasesPayArrs = that.pctChgBtwenYrsCasesPayArrs;
  }

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
    get => reportMonth;
    set => reportMonth = value;
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
  /// Defines whether this is a Judicial District 'GOAL' or 'DATA' record.  The
  /// 'GOAL' record will contain the monthly target amounts as defined by
  /// Central Office for each Judicial District.  The 'DATA' record will contain
  /// the actual values calculated and reported for each Judicial District.
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
  /// The value of the CASES_UNDER_ORDER_NUMERATOR attribute.
  /// Number of cases with an order.
  /// </summary>
  [JsonPropertyName("casesUnderOrderNumerator")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesUnderOrderNumerator
  {
    get => casesUnderOrderNumerator;
    set => casesUnderOrderNumerator = value;
  }

  /// <summary>
  /// The value of the CASES_UNDER_ORDER_DENOMINATOR attribute.
  /// Number of open cases.
  /// </summary>
  [JsonPropertyName("casesUnderOrderDenominator")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesUnderOrderDenominator
  {
    get => casesUnderOrderDenominator;
    set => casesUnderOrderDenominator = value;
  }

  /// <summary>
  /// The value of the CASES_UNDER_ORDER_PERCENT attribute.
  /// Percentage of cases under order (numerator divided by denominator).
  /// </summary>
  [JsonPropertyName("casesUnderOrderPercent")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CasesUnderOrderPercent
  {
    get => casesUnderOrderPercent;
    set => casesUnderOrderPercent = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the CASES_UNDER_ORDER_RANK attribute.
  /// Ranking of office by cases under order.
  /// </summary>
  [JsonPropertyName("casesUnderOrderRank")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesUnderOrderRank
  {
    get => casesUnderOrderRank;
    set => casesUnderOrderRank = value;
  }

  /// <summary>
  /// The value of the PEP_NUMERATOR attribute.
  /// Children in IV-D cases open during or at the end of the federal fiscal 
  /// year with paternity established or acknowledged.  This is reported only at
  /// the statewide level.
  /// </summary>
  [JsonPropertyName("pepNumerator")]
  [Member(Index = 10, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PepNumerator
  {
    get => pepNumerator;
    set => pepNumerator = value;
  }

  /// <summary>
  /// The value of the PEP_DENOMINATOR attribute.
  /// Children in IV-D cases at the end of the prior federal fiscal year who 
  /// were born out of wedlock.  This is the number reported by OCSE157.
  /// </summary>
  [JsonPropertyName("pepDenominator")]
  [Member(Index = 11, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PepDenominator
  {
    get => pepDenominator;
    set => pepDenominator = value;
  }

  /// <summary>
  /// The value of the PEP_PERCENT attribute.
  /// Paterntity Establishment Percentage (PEP).
  /// </summary>
  [JsonPropertyName("pepPercent")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? PepPercent
  {
    get => pepPercent;
    set => pepPercent = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_NUMERATOR attribute.
  /// The number of cases paying towards arrears during the report period.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsNumerator")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingArrearsNumerator
  {
    get => casesPayingArrearsNumerator;
    set => casesPayingArrearsNumerator = value;
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_DENOMINATOR attribute.
  /// Cases open at any point during the report period with arrears due.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsDenominator")]
  [Member(Index = 14, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingArrearsDenominator
  {
    get => casesPayingArrearsDenominator;
    set => casesPayingArrearsDenominator = value;
  }

  /// <summary>
  /// The value of the CASES_PAYING_ARREARS_PERCENT attribute.
  /// Percent of cases paying on arrears.
  /// </summary>
  [JsonPropertyName("casesPayingArrearsPercent")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Precision = 3, Optional
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
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingArrearsRank
  {
    get => casesPayingArrearsRank;
    set => casesPayingArrearsRank = value;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_NUM attribute.
  /// Dollar amount of current support collected in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthNum")]
  [Member(Index = 17, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidMthNum
  {
    get => currentSupportPaidMthNum;
    set => currentSupportPaidMthNum = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_DEN attribute.
  /// Dollar amount of current support due in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthDen")]
  [Member(Index = 18, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidMthDen
  {
    get => currentSupportPaidMthDen;
    set => currentSupportPaidMthDen = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_MTH_PER attribute.
  /// Percent of current support paid in month.
  /// </summary>
  [JsonPropertyName("currentSupportPaidMthPer")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Precision = 3, Optional
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
  [Member(Index = 20, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CurrentSupportPaidMthRnk
  {
    get => currentSupportPaidMthRnk;
    set => currentSupportPaidMthRnk = value;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_NUM attribute.
  /// Dollar amount of current support collected in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdNum")]
  [Member(Index = 21, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidFfytdNum
  {
    get => currentSupportPaidFfytdNum;
    set => currentSupportPaidFfytdNum = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_DEN attribute.
  /// Dollar amount of current support due in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdDen")]
  [Member(Index = 22, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CurrentSupportPaidFfytdDen
  {
    get => currentSupportPaidFfytdDen;
    set => currentSupportPaidFfytdDen = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID_FFYTD_PER attribute.
  /// Percent of current support paid in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("currentSupportPaidFfytdPer")]
  [Member(Index = 23, Type = MemberType.Number, Length = 9, Precision = 3, Optional
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
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CurrentSupportPaidFfytdRnk
  {
    get => currentSupportPaidFfytdRnk;
    set => currentSupportPaidFfytdRnk = value;
  }

  /// <summary>
  /// The value of the COLLECTIONS_FFYTD_TO_PRIOR_MONTH attribute.
  /// Total collections in federal fiscal year through the end of the prior 
  /// month.
  /// </summary>
  [JsonPropertyName("collectionsFfytdToPriorMonth")]
  [Member(Index = 25, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsFfytdToPriorMonth
  {
    get => collectionsFfytdToPriorMonth;
    set => collectionsFfytdToPriorMonth = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the COLLECTIONS_FFYTD_ACTUAL attribute.
  /// Total collections in federal fiscal year to date.  This is the sum of the 
  /// Collections_FFYTD_to_prior_month plus Collections_in_month_actual.
  /// </summary>
  [JsonPropertyName("collectionsFfytdActual")]
  [Member(Index = 26, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsFfytdActual
  {
    get => collectionsFfytdActual;
    set => collectionsFfytdActual = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the COLLECTIONS_FFYTD_PRIOR_YEAR attribute.
  /// Total collections in prior federal fiscal year through the same month.
  /// </summary>
  [JsonPropertyName("collectionsFfytdPriorYear")]
  [Member(Index = 27, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsFfytdPriorYear
  {
    get => collectionsFfytdPriorYear;
    set => collectionsFfytdPriorYear = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the COLLECTIONS_FFYTD_PERCENT_CHANGE attribute.
  /// Percentage change from prior federal fiscal year to month to current 
  /// federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("collectionsFfytdPercentChange")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CollectionsFfytdPercentChange
  {
    get => collectionsFfytdPercentChange;
    set => collectionsFfytdPercentChange = value != null
      ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the COLLECTIONS_FFYTD_RNK attribute.
  /// Ranking of office by percentage change of total collections federal fiscal
  /// year to date to total collections previous federal fiscal year to current
  /// month.
  /// </summary>
  [JsonPropertyName("collectionsFfytdRnk")]
  [Member(Index = 29, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CollectionsFfytdRnk
  {
    get => collectionsFfytdRnk;
    set => collectionsFfytdRnk = value;
  }

  /// <summary>
  /// The value of the COLLECTIONS_IN_MONTH_ACTUAL attribute.
  /// Total collections in month.
  /// </summary>
  [JsonPropertyName("collectionsInMonthActual")]
  [Member(Index = 30, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsInMonthActual
  {
    get => collectionsInMonthActual;
    set => collectionsInMonthActual = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the COLLECTIONS_IN_MONTH_PRIOR_YEAR attribute.
  /// Total collection in same month of previous year.
  /// </summary>
  [JsonPropertyName("collectionsInMonthPriorYear")]
  [Member(Index = 31, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsInMonthPriorYear
  {
    get => collectionsInMonthPriorYear;
    set => collectionsInMonthPriorYear = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the COLLECTIONS_IN_MONTH_PERCENT_CHG attribute.
  /// Percentage change of total collections in month to total collections in 
  /// same month of previous year.
  /// </summary>
  [JsonPropertyName("collectionsInMonthPercentChg")]
  [Member(Index = 32, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CollectionsInMonthPercentChg
  {
    get => collectionsInMonthPercentChg;
    set => collectionsInMonthPercentChg = value != null ? Truncate(value, 3) : null
      ;
  }

  /// <summary>
  /// The value of the COLLECTIONS_IN_MONTH_RNK attribute.
  /// Ranking of office by percentage change of total collections in month to 
  /// total collections in same month of previous year.
  /// </summary>
  [JsonPropertyName("collectionsInMonthRnk")]
  [Member(Index = 33, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CollectionsInMonthRnk
  {
    get => collectionsInMonthRnk;
    set => collectionsInMonthRnk = value;
  }

  /// <summary>
  /// The value of the ARREARS_DISTRIBUTED_MONTH_ACTUAL attribute.
  /// Total arrears amount distributed in month.
  /// </summary>
  [JsonPropertyName("arrearsDistributedMonthActual")]
  [Member(Index = 34, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? ArrearsDistributedMonthActual
  {
    get => arrearsDistributedMonthActual;
    set => arrearsDistributedMonthActual = value != null
      ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the ARREARS_DISTRIBUTED_MONTH_RNK attribute.
  /// Ranking of office by total arrears amount distributed in month.
  /// </summary>
  [JsonPropertyName("arrearsDistributedMonthRnk")]
  [Member(Index = 35, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ArrearsDistributedMonthRnk
  {
    get => arrearsDistributedMonthRnk;
    set => arrearsDistributedMonthRnk = value;
  }

  /// <summary>
  /// The value of the ARREARS_DISTRIBUTED_FFYTD_ACTUAL attribute.
  /// Total arrears amount distributed in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("arrearsDistributedFfytdActual")]
  [Member(Index = 36, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? ArrearsDistributedFfytdActual
  {
    get => arrearsDistributedFfytdActual;
    set => arrearsDistributedFfytdActual = value != null
      ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the ARREARS_DISTRUBUTED_FFYTD_RNK attribute.
  /// Ranking of office by total arrears amount distributed in federal fiscal 
  /// year to date.
  /// </summary>
  [JsonPropertyName("arrearsDistrubutedFfytdRnk")]
  [Member(Index = 37, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ArrearsDistrubutedFfytdRnk
  {
    get => arrearsDistrubutedFfytdRnk;
    set => arrearsDistrubutedFfytdRnk = value;
  }

  /// <summary>
  /// The value of the ARREARS_DUE_ACTUAL attribute.
  /// Total amount of arreas due.
  /// </summary>
  [JsonPropertyName("arrearsDueActual")]
  [Member(Index = 38, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? ArrearsDueActual
  {
    get => arrearsDueActual;
    set => arrearsDueActual = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the ARREARS_DUE_RNK attribute.
  /// Ranking of office by total arrears due.
  /// </summary>
  [JsonPropertyName("arrearsDueRnk")]
  [Member(Index = 39, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ArrearsDueRnk
  {
    get => arrearsDueRnk;
    set => arrearsDueRnk = value;
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_OBLIG_CASE_NUMER attribute.
  /// Total collections in month.
  /// </summary>
  [JsonPropertyName("collectionsPerObligCaseNumer")]
  [Member(Index = 40, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsPerObligCaseNumer
  {
    get => collectionsPerObligCaseNumer;
    set => collectionsPerObligCaseNumer = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_OBLIG_CASE_DENOM attribute.
  /// Number of cases with an order.
  /// </summary>
  [JsonPropertyName("collectionsPerObligCaseDenom")]
  [Member(Index = 41, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsPerObligCaseDenom
  {
    get => collectionsPerObligCaseDenom;
    set => collectionsPerObligCaseDenom = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_OBLIG_CASE_AVG attribute.
  /// Average amount of collections per obligated case.
  /// </summary>
  [JsonPropertyName("collectionsPerObligCaseAvg")]
  [Member(Index = 42, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsPerObligCaseAvg
  {
    get => collectionsPerObligCaseAvg;
    set => collectionsPerObligCaseAvg = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_OBLIG_CASE_RNK attribute.
  /// Ranking of office by average amount of collections per obligated case.
  /// </summary>
  [JsonPropertyName("collectionsPerObligCaseRnk")]
  [Member(Index = 43, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CollectionsPerObligCaseRnk
  {
    get => collectionsPerObligCaseRnk;
    set => collectionsPerObligCaseRnk = value;
  }

  /// <summary>
  /// The value of the IWO_PER_OBLIG_CASE_NUMERATOR attribute.
  /// Income withholding issued.
  /// </summary>
  [JsonPropertyName("iwoPerObligCaseNumerator")]
  [Member(Index = 44, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IwoPerObligCaseNumerator
  {
    get => iwoPerObligCaseNumerator;
    set => iwoPerObligCaseNumerator = value;
  }

  /// <summary>
  /// The value of the IWO_PER_OBLIG_CASE_DENOMINATOR attribute.
  /// Number of cases with an order.
  /// </summary>
  [JsonPropertyName("iwoPerObligCaseDenominator")]
  [Member(Index = 45, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IwoPerObligCaseDenominator
  {
    get => iwoPerObligCaseDenominator;
    set => iwoPerObligCaseDenominator = value;
  }

  /// <summary>
  /// The value of the IWO_PER_OBLIG_CASE_AVERAGE attribute.
  /// Average number of income withholding orders issued.
  /// </summary>
  [JsonPropertyName("iwoPerObligCaseAverage")]
  [Member(Index = 46, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? IwoPerObligCaseAverage
  {
    get => iwoPerObligCaseAverage;
    set => iwoPerObligCaseAverage = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the IWO_PER_OBLIG_CASE_RNK attribute.
  /// Ranking of office by average number of income withholding orders issued.
  /// </summary>
  [JsonPropertyName("iwoPerObligCaseRnk")]
  [Member(Index = 47, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IwoPerObligCaseRnk
  {
    get => iwoPerObligCaseRnk;
    set => iwoPerObligCaseRnk = value;
  }

  /// <summary>
  /// The value of the CASES_PER_FTE_NUMERATOR attribute.
  /// Number of open cases.
  /// </summary>
  [JsonPropertyName("casesPerFteNumerator")]
  [Member(Index = 48, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPerFteNumerator
  {
    get => casesPerFteNumerator;
    set => casesPerFteNumerator = value;
  }

  /// <summary>
  /// The value of the CASES_PER_FTE_DENOMINATOR attribute.
  /// Number of FTE (full time equivalent).
  /// </summary>
  [JsonPropertyName("casesPerFteDenominator")]
  [Member(Index = 49, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CasesPerFteDenominator
  {
    get => casesPerFteDenominator;
    set => casesPerFteDenominator = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the CASES_PER_FTE_AVERAGE attribute.
  /// Average number of cases to FTE (full time equivalent)
  /// </summary>
  [JsonPropertyName("casesPerFteAverage")]
  [Member(Index = 50, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CasesPerFteAverage
  {
    get => casesPerFteAverage;
    set => casesPerFteAverage = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the CASES_PER_FTE_RANK attribute.
  /// Ranking of office by average number of cases to FTE.
  /// </summary>
  [JsonPropertyName("casesPerFteRank")]
  [Member(Index = 51, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPerFteRank
  {
    get => casesPerFteRank;
    set => casesPerFteRank = value;
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_FTE_NUMERATOR attribute.
  /// Total collections in month.
  /// </summary>
  [JsonPropertyName("collectionsPerFteNumerator")]
  [Member(Index = 52, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsPerFteNumerator
  {
    get => collectionsPerFteNumerator;
    set => collectionsPerFteNumerator = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_FTE_DENOMINATOR attribute.
  /// Number of FTE (full time equivalent)
  /// </summary>
  [JsonPropertyName("collectionsPerFteDenominator")]
  [Member(Index = 53, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsPerFteDenominator
  {
    get => collectionsPerFteDenominator;
    set => collectionsPerFteDenominator = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_FTE_AVERAGE attribute.
  /// Number of FTE (full time equivalent)
  /// </summary>
  [JsonPropertyName("collectionsPerFteAverage")]
  [Member(Index = 54, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CollectionsPerFteAverage
  {
    get => collectionsPerFteAverage;
    set => collectionsPerFteAverage = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the COLLECTIONS_PER_FTE_RANK attribute.
  /// Ranking of office by average amount of collections per FTE.
  /// </summary>
  [JsonPropertyName("collectionsPerFteRank")]
  [Member(Index = 55, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CollectionsPerFteRank
  {
    get => collectionsPerFteRank;
    set => collectionsPerFteRank = value;
  }

  /// <summary>
  /// The value of the CASES_PAYING_NUMERATOR attribute.
  /// Number of cases paying in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("casesPayingNumerator")]
  [Member(Index = 56, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingNumerator
  {
    get => casesPayingNumerator;
    set => casesPayingNumerator = value;
  }

  /// <summary>
  /// The value of the CASES_PAYING_DENOMINATOR attribute.
  /// Number of open cases.
  /// </summary>
  [JsonPropertyName("casesPayingDenominator")]
  [Member(Index = 57, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingDenominator
  {
    get => casesPayingDenominator;
    set => casesPayingDenominator = value;
  }

  /// <summary>
  /// The value of the CASES_PAYING_PERCENT attribute.
  /// Percentage of cases paying in federal fiscal year to date.
  /// </summary>
  [JsonPropertyName("casesPayingPercent")]
  [Member(Index = 58, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CasesPayingPercent
  {
    get => casesPayingPercent;
    set => casesPayingPercent = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the CASES_PAYING_RANK attribute.
  /// Ranking of office by percentage of cases paying in federal fiscal year to 
  /// date.
  /// </summary>
  [JsonPropertyName("casesPayingRank")]
  [Member(Index = 59, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CasesPayingRank
  {
    get => casesPayingRank;
    set => casesPayingRank = value;
  }

  /// <summary>
  /// The value of the PEP_RANK attribute.
  /// Ranking of office by Paternity Establishment (PEP) percentage.
  /// </summary>
  [JsonPropertyName("pepRank")]
  [Member(Index = 60, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PepRank
  {
    get => pepRank;
    set => pepRank = value;
  }

  /// <summary>Length of the CONTRACTOR_NUMBER attribute.</summary>
  public const int ContractorNumber_MaxLength = 2;

  /// <summary>
  /// The value of the CONTRACTOR_NUMBER attribute.
  /// ID of the firm contracted to provide privatized Child Support services.
  /// </summary>
  [JsonPropertyName("contractorNumber")]
  [Member(Index = 61, Type = MemberType.Char, Length
    = ContractorNumber_MaxLength, Optional = true)]
  public string ContractorNumber
  {
    get => contractorNumber;
    set => contractorNumber = value != null
      ? TrimEnd(Substring(value, 1, ContractorNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PREV_YR_PEP_NUMERATOR attribute.
  /// The IV-D Paternity Establishment numerator from prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrPepNumerator")]
  [Member(Index = 62, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrevYrPepNumerator
  {
    get => prevYrPepNumerator;
    set => prevYrPepNumerator = value;
  }

  /// <summary>
  /// The value of the PREV_YR_PEP_DENOMINATOR attribute.
  /// The IV-D Paternity Establishment denominator from prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrPepDenominator")]
  [Member(Index = 63, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrevYrPepDenominator
  {
    get => prevYrPepDenominator;
    set => prevYrPepDenominator = value;
  }

  /// <summary>
  /// The value of the PREV_YR_PEP_PERCENT attribute.
  /// The IV-D Paternity Establishment Percentage for prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrPepPercent")]
  [Member(Index = 64, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? PrevYrPepPercent
  {
    get => prevYrPepPercent;
    set => prevYrPepPercent = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the PERCENT_CHG_BETWEEN_YRS_PEP attribute.
  /// The IV-D Paternity Establishment Percentage change between prior year 
  /// month and current same month.
  /// </summary>
  [JsonPropertyName("percentChgBetweenYrsPep")]
  [Member(Index = 65, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? PercentChgBetweenYrsPep
  {
    get => percentChgBetweenYrsPep;
    set => percentChgBetweenYrsPep = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the PREV_YR_CASE_NUMERATOR attribute.
  /// The cases under order numerator from prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrCaseNumerator")]
  [Member(Index = 66, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrevYrCaseNumerator
  {
    get => prevYrCaseNumerator;
    set => prevYrCaseNumerator = value;
  }

  /// <summary>
  /// The value of the PREV_YR_CASE_DENOMINATOR attribute.
  /// The cases under order denominator from prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrCaseDenominator")]
  [Member(Index = 67, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrevYrCaseDenominator
  {
    get => prevYrCaseDenominator;
    set => prevYrCaseDenominator = value;
  }

  /// <summary>
  /// The value of the CASES_UNDR_ORDR_PREV_YR_PCT attribute.
  /// The cases under order percentage for prior year same month.
  /// </summary>
  [JsonPropertyName("casesUndrOrdrPrevYrPct")]
  [Member(Index = 68, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CasesUndrOrdrPrevYrPct
  {
    get => casesUndrOrdrPrevYrPct;
    set => casesUndrOrdrPrevYrPct = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the PCT_CHG_BTWEN_YRS_CASE_UNDR_ORDR attribute.
  /// The cases under order percentage change between prior year month and 
  /// current same month.
  /// </summary>
  [JsonPropertyName("pctChgBtwenYrsCaseUndrOrdr")]
  [Member(Index = 69, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? PctChgBtwenYrsCaseUndrOrdr
  {
    get => pctChgBtwenYrsCaseUndrOrdr;
    set => pctChgBtwenYrsCaseUndrOrdr = value != null ? Truncate(value, 3) : null
      ;
  }

  /// <summary>
  /// The value of the PREV_YR_CUR_SUPPRT_PAID_NUMTR attribute.
  /// The current support paid numerator from prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrCurSupprtPaidNumtr")]
  [Member(Index = 70, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? PrevYrCurSupprtPaidNumtr
  {
    get => prevYrCurSupprtPaidNumtr;
    set => prevYrCurSupprtPaidNumtr = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the PREV_YR_CUR_SUPPRT_PAID_DENOM attribute.
  /// The current support paid denominator from prior year same month.
  /// </summary>
  [JsonPropertyName("prevYrCurSupprtPaidDenom")]
  [Member(Index = 71, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? PrevYrCurSupprtPaidDenom
  {
    get => prevYrCurSupprtPaidDenom;
    set => prevYrCurSupprtPaidDenom = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the CUR_SUPPRT_PD_PREV_YR_PCT attribute.
  /// The percentage of current support paid for prior year same month.
  /// </summary>
  [JsonPropertyName("curSupprtPdPrevYrPct")]
  [Member(Index = 72, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CurSupprtPdPrevYrPct
  {
    get => curSupprtPdPrevYrPct;
    set => curSupprtPdPrevYrPct = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the PCT_CHG_BTWEN_YRS_CUR_SUPT_PD attribute.
  /// The current support paid percentage change between prior year month and 
  /// current same month.
  /// </summary>
  [JsonPropertyName("pctChgBtwenYrsCurSuptPd")]
  [Member(Index = 73, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? PctChgBtwenYrsCurSuptPd
  {
    get => pctChgBtwenYrsCurSuptPd;
    set => pctChgBtwenYrsCurSuptPd = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the PRV_YR_CASES_PAID_ARREARS_NUMTR attribute.
  /// The cases paying on arrears numerator from prior year same month.
  /// </summary>
  [JsonPropertyName("prvYrCasesPaidArrearsNumtr")]
  [Member(Index = 74, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrvYrCasesPaidArrearsNumtr
  {
    get => prvYrCasesPaidArrearsNumtr;
    set => prvYrCasesPaidArrearsNumtr = value;
  }

  /// <summary>
  /// The value of the PRV_YR_CASES_PAID_ARREARS_DENOM attribute.
  /// The cases paying on arrears denominator from prior year same month.
  /// </summary>
  [JsonPropertyName("prvYrCasesPaidArrearsDenom")]
  [Member(Index = 75, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrvYrCasesPaidArrearsDenom
  {
    get => prvYrCasesPaidArrearsDenom;
    set => prvYrCasesPaidArrearsDenom = value;
  }

  /// <summary>
  /// The value of the CASES_PAY_ARREARS_PRV_YR_PCT attribute.
  /// The percentage of cases paying arrears for prior year same month.
  /// </summary>
  [JsonPropertyName("casesPayArrearsPrvYrPct")]
  [Member(Index = 76, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? CasesPayArrearsPrvYrPct
  {
    get => casesPayArrearsPrvYrPct;
    set => casesPayArrearsPrvYrPct = value != null ? Truncate(value, 3) : null;
  }

  /// <summary>
  /// The value of the PCT_CHG_BTWEN_YRS_CASES_PAY_ARRS attribute.
  /// The cases paying arrears percentage change between prior year month and 
  /// current same month.
  /// </summary>
  [JsonPropertyName("pctChgBtwenYrsCasesPayArrs")]
  [Member(Index = 77, Type = MemberType.Number, Length = 9, Precision = 3, Optional
    = true)]
  public decimal? PctChgBtwenYrsCasesPayArrs
  {
    get => pctChgBtwenYrsCasesPayArrs;
    set => pctChgBtwenYrsCasesPayArrs = value != null ? Truncate(value, 3) : null
      ;
  }

  private int reportMonth;
  private string reportLevel;
  private string reportLevelId;
  private string type1;
  private DateTime? asOfDate;
  private int? casesUnderOrderNumerator;
  private int? casesUnderOrderDenominator;
  private decimal? casesUnderOrderPercent;
  private int? casesUnderOrderRank;
  private int? pepNumerator;
  private int? pepDenominator;
  private decimal? pepPercent;
  private int? casesPayingArrearsNumerator;
  private int? casesPayingArrearsDenominator;
  private decimal? casesPayingArrearsPercent;
  private int? casesPayingArrearsRank;
  private decimal? currentSupportPaidMthNum;
  private decimal? currentSupportPaidMthDen;
  private decimal? currentSupportPaidMthPer;
  private int? currentSupportPaidMthRnk;
  private decimal? currentSupportPaidFfytdNum;
  private decimal? currentSupportPaidFfytdDen;
  private decimal? currentSupportPaidFfytdPer;
  private int? currentSupportPaidFfytdRnk;
  private decimal? collectionsFfytdToPriorMonth;
  private decimal? collectionsFfytdActual;
  private decimal? collectionsFfytdPriorYear;
  private decimal? collectionsFfytdPercentChange;
  private int? collectionsFfytdRnk;
  private decimal? collectionsInMonthActual;
  private decimal? collectionsInMonthPriorYear;
  private decimal? collectionsInMonthPercentChg;
  private int? collectionsInMonthRnk;
  private decimal? arrearsDistributedMonthActual;
  private int? arrearsDistributedMonthRnk;
  private decimal? arrearsDistributedFfytdActual;
  private int? arrearsDistrubutedFfytdRnk;
  private decimal? arrearsDueActual;
  private int? arrearsDueRnk;
  private decimal? collectionsPerObligCaseNumer;
  private decimal? collectionsPerObligCaseDenom;
  private decimal? collectionsPerObligCaseAvg;
  private int? collectionsPerObligCaseRnk;
  private int? iwoPerObligCaseNumerator;
  private int? iwoPerObligCaseDenominator;
  private decimal? iwoPerObligCaseAverage;
  private int? iwoPerObligCaseRnk;
  private int? casesPerFteNumerator;
  private decimal? casesPerFteDenominator;
  private decimal? casesPerFteAverage;
  private int? casesPerFteRank;
  private decimal? collectionsPerFteNumerator;
  private decimal? collectionsPerFteDenominator;
  private decimal? collectionsPerFteAverage;
  private int? collectionsPerFteRank;
  private int? casesPayingNumerator;
  private int? casesPayingDenominator;
  private decimal? casesPayingPercent;
  private int? casesPayingRank;
  private int? pepRank;
  private string contractorNumber;
  private int? prevYrPepNumerator;
  private int? prevYrPepDenominator;
  private decimal? prevYrPepPercent;
  private decimal? percentChgBetweenYrsPep;
  private int? prevYrCaseNumerator;
  private int? prevYrCaseDenominator;
  private decimal? casesUndrOrdrPrevYrPct;
  private decimal? pctChgBtwenYrsCaseUndrOrdr;
  private decimal? prevYrCurSupprtPaidNumtr;
  private decimal? prevYrCurSupprtPaidDenom;
  private decimal? curSupprtPdPrevYrPct;
  private decimal? pctChgBtwenYrsCurSuptPd;
  private int? prvYrCasesPaidArrearsNumtr;
  private int? prvYrCasesPaidArrearsDenom;
  private decimal? casesPayArrearsPrvYrPct;
  private decimal? pctChgBtwenYrsCasesPayArrs;
}
