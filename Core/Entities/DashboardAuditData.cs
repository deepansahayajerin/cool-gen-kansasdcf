// The source file: DASHBOARD_AUDIT_DATA, ID: 945116244, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

[Serializable]
public partial class DashboardAuditData: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DashboardAuditData()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DashboardAuditData(DashboardAuditData that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DashboardAuditData Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DashboardAuditData that)
  {
    base.Assign(that);
    reportMonth = that.reportMonth;
    dashboardPriority = that.dashboardPriority;
    runNumber = that.runNumber;
    createdTimestamp = that.createdTimestamp;
    office = that.office;
    judicialDistrict = that.judicialDistrict;
    workerId = that.workerId;
    caseNumber = that.caseNumber;
    standardNumber = that.standardNumber;
    payorCspNumber = that.payorCspNumber;
    suppCspNumber = that.suppCspNumber;
    fte = that.fte;
    collectionAmount = that.collectionAmount;
    collAppliedToCd = that.collAppliedToCd;
    collectionCreatedDate = that.collectionCreatedDate;
    collectionType = that.collectionType;
    debtBalanceDue = that.debtBalanceDue;
    debtDueDate = that.debtDueDate;
    debtType = that.debtType;
    legalActionDate = that.legalActionDate;
    legalReferralDate = that.legalReferralDate;
    legalReferralNumber = that.legalReferralNumber;
    daysReported = that.daysReported;
    verifiedDate = that.verifiedDate;
    caseDate = that.caseDate;
    reviewDate = that.reviewDate;
    contractorNumber = that.contractorNumber;
  }

  /// <summary>
  /// The value of the REPORT_MONTH attribute.
  /// Dashboard reprot month.  This will be the reporting year and month in a 
  /// YYYYMM format.
  /// </summary>
  [JsonPropertyName("reportMonth")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 6)]
  public int ReportMonth
  {
    get => reportMonth;
    set => reportMonth = value;
  }

  /// <summary>Length of the DASHBOARD_PRIORITY attribute.</summary>
  public const int DashboardPriority_MaxLength = 8;

  /// <summary>
  /// The value of the DASHBOARD_PRIORITY attribute.
  /// Dashboard priority number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = DashboardPriority_MaxLength)]
  public string DashboardPriority
  {
    get => dashboardPriority ?? "";
    set => dashboardPriority =
      TrimEnd(Substring(value, 1, DashboardPriority_MaxLength));
  }

  /// <summary>
  /// The json value of the DashboardPriority attribute.</summary>
  [JsonPropertyName("dashboardPriority")]
  [Computed]
  public string DashboardPriority_Json
  {
    get => NullIf(DashboardPriority, "");
    set => DashboardPriority = value;
  }

  /// <summary>
  /// The value of the RUN_NUMBER attribute.
  /// Run number during which the audit data was created.
  /// </summary>
  [JsonPropertyName("runNumber")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 3)]
  public int RunNumber
  {
    get => runNumber;
    set => runNumber = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp when the audit data was created.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the OFFICE attribute.
  /// Office ID where data is being reported.
  /// </summary>
  [JsonPropertyName("office")]
  [Member(Index = 5, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? Office
  {
    get => office;
    set => office = value;
  }

  /// <summary>Length of the JUDICIAL_DISTRICT attribute.</summary>
  public const int JudicialDistrict_MaxLength = 2;

  /// <summary>
  /// The value of the JUDICIAL_DISTRICT attribute.
  /// Judicial District where data is being reported.
  /// </summary>
  [JsonPropertyName("judicialDistrict")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = JudicialDistrict_MaxLength, Optional = true)]
  public string JudicialDistrict
  {
    get => judicialDistrict;
    set => judicialDistrict = value != null
      ? TrimEnd(Substring(value, 1, JudicialDistrict_MaxLength)) : null;
  }

  /// <summary>Length of the WORKER_ID attribute.</summary>
  public const int WorkerId_MaxLength = 8;

  /// <summary>
  /// The value of the WORKER_ID attribute.
  /// User ID of the worker who performed the activity.
  /// </summary>
  [JsonPropertyName("workerId")]
  [Member(Index = 7, Type = MemberType.Char, Length = WorkerId_MaxLength, Optional
    = true)]
  public string WorkerId
  {
    get => workerId;
    set => workerId = value != null
      ? TrimEnd(Substring(value, 1, WorkerId_MaxLength)) : null;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// CSS Case number reported.
  /// </summary>
  [JsonPropertyName("caseNumber")]
  [Member(Index = 8, Type = MemberType.Char, Length = CaseNumber_MaxLength, Optional
    = true)]
  public string CaseNumber
  {
    get => caseNumber;
    set => caseNumber = value != null
      ? TrimEnd(Substring(value, 1, CaseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the STANDARD_NUMBER attribute.</summary>
  public const int StandardNumber_MaxLength = 20;

  /// <summary>
  /// The value of the STANDARD_NUMBER attribute.
  /// Legal action standard number reported.
  /// </summary>
  [JsonPropertyName("standardNumber")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = StandardNumber_MaxLength, Optional = true)]
  public string StandardNumber
  {
    get => standardNumber;
    set => standardNumber = value != null
      ? TrimEnd(Substring(value, 1, StandardNumber_MaxLength)) : null;
  }

  /// <summary>Length of the PAYOR_CSP_NUMBER attribute.</summary>
  public const int PayorCspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the PAYOR_CSP_NUMBER attribute.
  /// Obligor/Payor CSP number reported.
  /// </summary>
  [JsonPropertyName("payorCspNumber")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = PayorCspNumber_MaxLength, Optional = true)]
  public string PayorCspNumber
  {
    get => payorCspNumber;
    set => payorCspNumber = value != null
      ? TrimEnd(Substring(value, 1, PayorCspNumber_MaxLength)) : null;
  }

  /// <summary>Length of the SUPP_CSP_NUMBER attribute.</summary>
  public const int SuppCspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the SUPP_CSP_NUMBER attribute.
  /// Supported/Child CSP Number
  /// </summary>
  [JsonPropertyName("suppCspNumber")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = SuppCspNumber_MaxLength, Optional = true)]
  public string SuppCspNumber
  {
    get => suppCspNumber;
    set => suppCspNumber = value != null
      ? TrimEnd(Substring(value, 1, SuppCspNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the FTE attribute.
  /// Full Time Equivalent.
  /// </summary>
  [JsonPropertyName("fte")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? Fte
  {
    get => fte;
    set => fte = value;
  }

  /// <summary>
  /// The value of the COLLECTION_AMOUNT attribute.
  /// Amount of collection reported
  /// </summary>
  [JsonPropertyName("collectionAmount")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CollectionAmount
  {
    get => collectionAmount;
    set => collectionAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the COLL_APPLIED_TO_CD attribute.</summary>
  public const int CollAppliedToCd_MaxLength = 1;

  /// <summary>
  /// The value of the COLL_APPLIED_TO_CD attribute.
  /// Whether the collection applied to Current, Arrears, or Gift.
  /// </summary>
  [JsonPropertyName("collAppliedToCd")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = CollAppliedToCd_MaxLength, Optional = true)]
  public string CollAppliedToCd
  {
    get => collAppliedToCd;
    set => collAppliedToCd = value != null
      ? TrimEnd(Substring(value, 1, CollAppliedToCd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the COLLECTION_CREATED_DATE attribute.
  /// Date collection record was created.
  /// </summary>
  [JsonPropertyName("collectionCreatedDate")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? CollectionCreatedDate
  {
    get => collectionCreatedDate;
    set => collectionCreatedDate = value;
  }

  /// <summary>Length of the COLLECTION_TYPE attribute.</summary>
  public const int CollectionType_MaxLength = 1;

  /// <summary>
  /// The value of the COLLECTION_TYPE attribute.
  /// Collection type code reported.
  /// </summary>
  [JsonPropertyName("collectionType")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = CollectionType_MaxLength, Optional = true)]
  public string CollectionType
  {
    get => collectionType;
    set => collectionType = value != null
      ? TrimEnd(Substring(value, 1, CollectionType_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DEBT_BALANCE_DUE attribute.
  /// Amount of the Debt balance reported.
  /// </summary>
  [JsonPropertyName("debtBalanceDue")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? DebtBalanceDue
  {
    get => debtBalanceDue;
    set => debtBalanceDue = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the DEBT_DUE_DATE attribute.
  /// Date the reported debt was due.
  /// </summary>
  [JsonPropertyName("debtDueDate")]
  [Member(Index = 18, Type = MemberType.Date, Optional = true)]
  public DateTime? DebtDueDate
  {
    get => debtDueDate;
    set => debtDueDate = value;
  }

  /// <summary>Length of the DEBT_TYPE attribute.</summary>
  public const int DebtType_MaxLength = 7;

  /// <summary>
  /// The value of the DEBT_TYPE attribute.
  /// Debt type code of the reported debt.
  /// </summary>
  [JsonPropertyName("debtType")]
  [Member(Index = 19, Type = MemberType.Char, Length = DebtType_MaxLength, Optional
    = true)]
  public string DebtType
  {
    get => debtType;
    set => debtType = value != null
      ? TrimEnd(Substring(value, 1, DebtType_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LEGAL_ACTION_DATE attribute.
  /// Created or Filed date of the reported legal action.
  /// </summary>
  [JsonPropertyName("legalActionDate")]
  [Member(Index = 20, Type = MemberType.Date, Optional = true)]
  public DateTime? LegalActionDate
  {
    get => legalActionDate;
    set => legalActionDate = value;
  }

  /// <summary>
  /// The value of the LEGAL_REFERRAL_DATE attribute.
  /// Date of the reported legal referral.
  /// </summary>
  [JsonPropertyName("legalReferralDate")]
  [Member(Index = 21, Type = MemberType.Date, Optional = true)]
  public DateTime? LegalReferralDate
  {
    get => legalReferralDate;
    set => legalReferralDate = value;
  }

  /// <summary>
  /// The value of the LEGAL_REFERRAL_NUMBER attribute.
  /// Identifier of the legal referral reported.
  /// </summary>
  [JsonPropertyName("legalReferralNumber")]
  [Member(Index = 22, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? LegalReferralNumber
  {
    get => legalReferralNumber;
    set => legalReferralNumber = value;
  }

  /// <summary>
  /// The value of the DAYS_REPORTED attribute.
  /// Number of days calculated between two system events.
  /// </summary>
  [JsonPropertyName("daysReported")]
  [Member(Index = 23, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DaysReported
  {
    get => daysReported;
    set => daysReported = value;
  }

  /// <summary>
  /// The value of the VERIFIED_DATE attribute.
  /// Verified date of record reported.
  /// </summary>
  [JsonPropertyName("verifiedDate")]
  [Member(Index = 24, Type = MemberType.Date, Optional = true)]
  public DateTime? VerifiedDate
  {
    get => verifiedDate;
    set => verifiedDate = value;
  }

  /// <summary>
  /// The value of the CASE_DATE attribute.
  /// Open or close date of the case reported.
  /// </summary>
  [JsonPropertyName("caseDate")]
  [Member(Index = 25, Type = MemberType.Date, Optional = true)]
  public DateTime? CaseDate
  {
    get => caseDate;
    set => caseDate = value;
  }

  /// <summary>
  /// The value of the REVIEW_DATE attribute.
  /// Date of the Case or Modification Review reported.
  /// </summary>
  [JsonPropertyName("reviewDate")]
  [Member(Index = 26, Type = MemberType.Date, Optional = true)]
  public DateTime? ReviewDate
  {
    get => reviewDate;
    set => reviewDate = value;
  }

  /// <summary>Length of the CONTRACTOR_NUMBER attribute.</summary>
  public const int ContractorNumber_MaxLength = 2;

  /// <summary>
  /// The value of the CONTRACTOR_NUMBER attribute.
  /// ID of the firm contracted to provide privatized Child Support services.
  /// </summary>
  [JsonPropertyName("contractorNumber")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = ContractorNumber_MaxLength, Optional = true)]
  public string ContractorNumber
  {
    get => contractorNumber;
    set => contractorNumber = value != null
      ? TrimEnd(Substring(value, 1, ContractorNumber_MaxLength)) : null;
  }

  private int reportMonth;
  private string dashboardPriority;
  private int runNumber;
  private DateTime? createdTimestamp;
  private int? office;
  private string judicialDistrict;
  private string workerId;
  private string caseNumber;
  private string standardNumber;
  private string payorCspNumber;
  private string suppCspNumber;
  private int? fte;
  private decimal? collectionAmount;
  private string collAppliedToCd;
  private DateTime? collectionCreatedDate;
  private string collectionType;
  private decimal? debtBalanceDue;
  private DateTime? debtDueDate;
  private string debtType;
  private DateTime? legalActionDate;
  private DateTime? legalReferralDate;
  private int? legalReferralNumber;
  private int? daysReported;
  private DateTime? verifiedDate;
  private DateTime? caseDate;
  private DateTime? reviewDate;
  private string contractorNumber;
}
