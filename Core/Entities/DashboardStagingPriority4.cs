// The source file: DASHBOARD_STAGING_PRIORITY_4, ID: 945237020, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
///  Staging table for the Dashboard Pyramid Report.
/// </summary>
[Serializable]
public partial class DashboardStagingPriority4: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DashboardStagingPriority4()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DashboardStagingPriority4(DashboardStagingPriority4 that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DashboardStagingPriority4 Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DashboardStagingPriority4 that)
  {
    base.Assign(that);
    reportMonth = that.reportMonth;
    runNumber = that.runNumber;
    caseNumber = that.caseNumber;
    asOfDate = that.asOfDate;
    currentCsInd = that.currentCsInd;
    otherObgInd = that.otherObgInd;
    csDueAmt = that.csDueAmt;
    csCollectedAmt = that.csCollectedAmt;
    payingCaseInd = that.payingCaseInd;
    paternityEstInd = that.paternityEstInd;
    addressVerInd = that.addressVerInd;
    employerVerInd = that.employerVerInd;
    workerId = that.workerId;
    judicialDistrict = that.judicialDistrict;
    contractorNumber = that.contractorNumber;
  }

  /// <summary>
  /// The value of the REPORT_MONTH attribute.
  /// Dashboard report month.  This will be the reporting year and month in a 
  /// YYYYMM format.
  /// </summary>
  [JsonPropertyName("reportMonth")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int ReportMonth
  {
    get => reportMonth;
    set => reportMonth = value;
  }

  /// <summary>
  /// The value of the RUN_NUMBER attribute.
  /// Run number during which the Priority 4 Pyramid Report was created.
  /// </summary>
  [JsonPropertyName("runNumber")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int RunNumber
  {
    get => runNumber;
    set => runNumber = value;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// The CSE case number included in the Dashboard Priority 4 Pyramid Report.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CaseNumber_MaxLength)]
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

  /// <summary>
  /// The value of the AS_OF_DATE attribute.
  /// Date through which calculations in this row apply.
  /// </summary>
  [JsonPropertyName("asOfDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? AsOfDate
  {
    get => asOfDate;
    set => asOfDate = value;
  }

  /// <summary>Length of the CURRENT_CS_IND attribute.</summary>
  public const int CurrentCsInd_MaxLength = 1;

  /// <summary>
  /// The value of the CURRENT_CS_IND attribute.
  /// Flag indicating if the case has a current child support obligation
  /// </summary>
  [JsonPropertyName("currentCsInd")]
  [Member(Index = 5, Type = MemberType.Char, Length = CurrentCsInd_MaxLength, Optional
    = true)]
  public string CurrentCsInd
  {
    get => currentCsInd;
    set => currentCsInd = value != null
      ? TrimEnd(Substring(value, 1, CurrentCsInd_MaxLength)) : null;
  }

  /// <summary>Length of the OTHER_OBG_IND attribute.</summary>
  public const int OtherObgInd_MaxLength = 1;

  /// <summary>
  /// The value of the OTHER_OBG_IND attribute.
  /// Flag indicating if the case has an obligation other than current child 
  /// support.
  /// </summary>
  [JsonPropertyName("otherObgInd")]
  [Member(Index = 6, Type = MemberType.Char, Length = OtherObgInd_MaxLength, Optional
    = true)]
  public string OtherObgInd
  {
    get => otherObgInd;
    set => otherObgInd = value != null
      ? TrimEnd(Substring(value, 1, OtherObgInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CS_DUE_AMT attribute.
  /// The amount of child support due on the case in the preceding three months.
  /// </summary>
  [JsonPropertyName("csDueAmt")]
  [Member(Index = 7, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CsDueAmt
  {
    get => csDueAmt;
    set => csDueAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the CS_COLLECTED_AMT attribute.
  /// The amount of child support collected on the case in the preceding three 
  /// months.
  /// </summary>
  [JsonPropertyName("csCollectedAmt")]
  [Member(Index = 8, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? CsCollectedAmt
  {
    get => csCollectedAmt;
    set => csCollectedAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the PAYING_CASE_IND attribute.</summary>
  public const int PayingCaseInd_MaxLength = 1;

  /// <summary>
  /// The value of the PAYING_CASE_IND attribute.
  /// Flag indicating if the case is paying case.
  /// </summary>
  [JsonPropertyName("payingCaseInd")]
  [Member(Index = 9, Type = MemberType.Char, Length = PayingCaseInd_MaxLength, Optional
    = true)]
  public string PayingCaseInd
  {
    get => payingCaseInd;
    set => payingCaseInd = value != null
      ? TrimEnd(Substring(value, 1, PayingCaseInd_MaxLength)) : null;
  }

  /// <summary>Length of the PATERNITY_EST_IND attribute.</summary>
  public const int PaternityEstInd_MaxLength = 1;

  /// <summary>
  /// The value of the PATERNITY_EST_IND attribute.
  /// Flag indicating if paternity is established for all children on the case.
  /// </summary>
  [JsonPropertyName("paternityEstInd")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = PaternityEstInd_MaxLength, Optional = true)]
  public string PaternityEstInd
  {
    get => paternityEstInd;
    set => paternityEstInd = value != null
      ? TrimEnd(Substring(value, 1, PaternityEstInd_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_VER_IND attribute.</summary>
  public const int AddressVerInd_MaxLength = 1;

  /// <summary>
  /// The value of the ADDRESS_VER_IND attribute.
  /// Flag indicating if the AP has a verified address.
  /// </summary>
  [JsonPropertyName("addressVerInd")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = AddressVerInd_MaxLength, Optional = true)]
  public string AddressVerInd
  {
    get => addressVerInd;
    set => addressVerInd = value != null
      ? TrimEnd(Substring(value, 1, AddressVerInd_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_VER_IND attribute.</summary>
  public const int EmployerVerInd_MaxLength = 1;

  /// <summary>
  /// The value of the EMPLOYER_VER_IND attribute.
  /// Flag indicating if the AP has a verified employer.
  /// </summary>
  [JsonPropertyName("employerVerInd")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = EmployerVerInd_MaxLength, Optional = true)]
  public string EmployerVerInd
  {
    get => employerVerInd;
    set => employerVerInd = value != null
      ? TrimEnd(Substring(value, 1, EmployerVerInd_MaxLength)) : null;
  }

  /// <summary>Length of the WORKER_ID attribute.</summary>
  public const int WorkerId_MaxLength = 8;

  /// <summary>
  /// The value of the WORKER_ID attribute.
  /// User ID of the caseworker assigned to the case.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = WorkerId_MaxLength)]
  public string WorkerId
  {
    get => workerId ?? "";
    set => workerId = TrimEnd(Substring(value, 1, WorkerId_MaxLength));
  }

  /// <summary>
  /// The json value of the WorkerId attribute.</summary>
  [JsonPropertyName("workerId")]
  [Computed]
  public string WorkerId_Json
  {
    get => NullIf(WorkerId, "");
    set => WorkerId = value;
  }

  /// <summary>Length of the JUDICIAL_DISTRICT attribute.</summary>
  public const int JudicialDistrict_MaxLength = 2;

  /// <summary>
  /// The value of the JUDICIAL_DISTRICT attribute.
  /// Judicial district to which the case is assigned.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = JudicialDistrict_MaxLength)]
  public string JudicialDistrict
  {
    get => judicialDistrict ?? "";
    set => judicialDistrict =
      TrimEnd(Substring(value, 1, JudicialDistrict_MaxLength));
  }

  /// <summary>
  /// The json value of the JudicialDistrict attribute.</summary>
  [JsonPropertyName("judicialDistrict")]
  [Computed]
  public string JudicialDistrict_Json
  {
    get => NullIf(JudicialDistrict, "");
    set => JudicialDistrict = value;
  }

  /// <summary>Length of the CONTRACTOR_NUMBER attribute.</summary>
  public const int ContractorNumber_MaxLength = 2;

  /// <summary>
  /// The value of the CONTRACTOR_NUMBER attribute.
  /// ID of the firm contracted to provide privatized Child Support services.
  /// </summary>
  [JsonPropertyName("contractorNumber")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = ContractorNumber_MaxLength, Optional = true)]
  public string ContractorNumber
  {
    get => contractorNumber;
    set => contractorNumber = value != null
      ? TrimEnd(Substring(value, 1, ContractorNumber_MaxLength)) : null;
  }

  private int reportMonth;
  private int runNumber;
  private string caseNumber;
  private DateTime? asOfDate;
  private string currentCsInd;
  private string otherObgInd;
  private decimal? csDueAmt;
  private decimal? csCollectedAmt;
  private string payingCaseInd;
  private string paternityEstInd;
  private string addressVerInd;
  private string employerVerInd;
  private string workerId;
  private string judicialDistrict;
  private string contractorNumber;
}
