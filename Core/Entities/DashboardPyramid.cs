// The source file: DASHBOARD_PYRAMID, ID: 945236973, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Stores aggregate values used for the Priority 4 Dashboard Pyramid Report.
/// </summary>
[Serializable]
public partial class DashboardPyramid: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DashboardPyramid()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DashboardPyramid(DashboardPyramid that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DashboardPyramid Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DashboardPyramid that)
  {
    base.Assign(that);
    reportMonth = that.reportMonth;
    runNumber = that.runNumber;
    asOfDate = that.asOfDate;
    totalCases = that.totalCases;
    csCases = that.csCases;
    payingCases = that.payingCases;
    nopayCases = that.nopayCases;
    nopayNoaddemp = that.nopayNoaddemp;
    nopayAdd = that.nopayAdd;
    nopayAddV = that.nopayAddV;
    nopayAddNv = that.nopayAddNv;
    nopayEmp = that.nopayEmp;
    nopayEmpV = that.nopayEmpV;
    nopayEmpNv = that.nopayEmpNv;
    nopayAddEmp = that.nopayAddEmp;
    nopayAddEmpV = that.nopayAddEmpV;
    nopayAddEmpNv = that.nopayAddEmpNv;
    nonCsObCases = that.nonCsObCases;
    noobCases = that.noobCases;
    noobPat = that.noobPat;
    noobPatNoaddemp = that.noobPatNoaddemp;
    noobPatAdd = that.noobPatAdd;
    noobPatAddV = that.noobPatAddV;
    noobPatAddNv = that.noobPatAddNv;
    noobPatEmp = that.noobPatEmp;
    noobPatEmpV = that.noobPatEmpV;
    noobPatEmpNv = that.noobPatEmpNv;
    noobPatAddemp = that.noobPatAddemp;
    noobPatAddempV = that.noobPatAddempV;
    noobPatAddempNv = that.noobPatAddempNv;
    noobNopat = that.noobNopat;
    noobNopatNoaddem = that.noobNopatNoaddem;
    noobNopatAdd = that.noobNopatAdd;
    noobNopatAddV = that.noobNopatAddV;
    noobNopatAddNv = that.noobNopatAddNv;
    noobNopatEmp = that.noobNopatEmp;
    noobNopatEmpV = that.noobNopatEmpV;
    noobNopatEmpNv = that.noobNopatEmpNv;
    noobNopatAdem = that.noobNopatAdem;
    noobNopatAdemV = that.noobNopatAdemV;
    noobNopatAdemNv = that.noobNopatAdemNv;
  }

  /// <summary>
  /// The value of the REPORT_MONTH attribute.
  /// Dashboard report month.  This will be the reporting year and month in a 
  /// YYYYMM format..
  /// </summary>
  [JsonPropertyName("reportMonth")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 6)]
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

  /// <summary>
  /// The value of the AS_OF_DATE attribute.
  /// Date through which calculations in this row apply.
  /// </summary>
  [JsonPropertyName("asOfDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? AsOfDate
  {
    get => asOfDate;
    set => asOfDate = value;
  }

  /// <summary>
  /// The value of the TOTAL_CASES attribute.
  /// The count of all cases in O status on the run date.
  /// </summary>
  [JsonPropertyName("totalCases")]
  [Member(Index = 4, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TotalCases
  {
    get => totalCases;
    set => totalCases = value;
  }

  /// <summary>
  /// The value of the CS_CASES attribute.
  /// The count of all cases in O status with Current Child Support owed.
  /// </summary>
  [JsonPropertyName("csCases")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CsCases
  {
    get => csCases;
    set => csCases = value;
  }

  /// <summary>
  /// The value of the PAYING_CASES attribute.
  /// The count of all cases in O status with Current Child Support owed and 
  /// paying.
  /// </summary>
  [JsonPropertyName("payingCases")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PayingCases
  {
    get => payingCases;
    set => payingCases = value;
  }

  /// <summary>
  /// The value of the NOPAY_CASES attribute.
  /// The count of all cases in O status with Current Child Support owed and 
  /// not paying.
  /// </summary>
  [JsonPropertyName("nopayCases")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NopayCases
  {
    get => nopayCases;
    set => nopayCases = value;
  }

  /// <summary>
  /// The value of the NOPAY_NOADDEMP attribute.
  /// The count of all cases in O status with Current Child Support owed, not 
  /// paying, and no address or employer.
  /// </summary>
  [JsonPropertyName("nopayNoaddemp")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NopayNoaddemp
  {
    get => nopayNoaddemp;
    set => nopayNoaddemp = value;
  }

  /// <summary>
  /// The value of the NOPAY_ADD attribute.
  /// The count of all cases in O status with Current Child Support owed, not 
  /// paying, and an address only.
  /// </summary>
  [JsonPropertyName("nopayAdd")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NopayAdd
  {
    get => nopayAdd;
    set => nopayAdd = value;
  }

  /// <summary>
  /// The value of the NOPAY_ADD_V attribute.
  /// The count of all cases in O status with Current Child Support owed, not 
  /// paying, and a verified address only.
  /// </summary>
  [JsonPropertyName("nopayAddV")]
  [Member(Index = 10, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NopayAddV
  {
    get => nopayAddV;
    set => nopayAddV = value;
  }

  /// <summary>
  /// The value of the NOPAY_ADD_NV attribute.
  /// The count of all cases in O status with Current Child Support owed, not 
  /// paying, and a non verified address only.
  /// </summary>
  [JsonPropertyName("nopayAddNv")]
  [Member(Index = 11, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NopayAddNv
  {
    get => nopayAddNv;
    set => nopayAddNv = value;
  }

  /// <summary>
  /// The value of the NOPAY_EMP attribute.
  /// The count of all cases in O status with Current Child Support owed, not 
  /// paying, and an employer only.
  /// </summary>
  [JsonPropertyName("nopayEmp")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NopayEmp
  {
    get => nopayEmp;
    set => nopayEmp = value;
  }

  /// <summary>
  /// The value of the NOPAY_EMP_V attribute.
  /// The count of all cases in O status with Current Child Support owed, not 
  /// paying, and a verified employer only.
  /// </summary>
  [JsonPropertyName("nopayEmpV")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NopayEmpV
  {
    get => nopayEmpV;
    set => nopayEmpV = value;
  }

  /// <summary>
  /// The value of the NOPAY_EMP_NV attribute.
  /// The count of all cases in O status with Current Child Support owed, not 
  /// paying, and a non verified employer only.
  /// </summary>
  [JsonPropertyName("nopayEmpNv")]
  [Member(Index = 14, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NopayEmpNv
  {
    get => nopayEmpNv;
    set => nopayEmpNv = value;
  }

  /// <summary>
  /// The value of the NOPAY_ADD_EMP attribute.
  /// The count of all cases in O status with Current Child Support owed, not 
  /// paying, and both an address and employer.
  /// </summary>
  [JsonPropertyName("nopayAddEmp")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NopayAddEmp
  {
    get => nopayAddEmp;
    set => nopayAddEmp = value;
  }

  /// <summary>
  /// The value of the NOPAY_ADD_EMP_V attribute.
  /// The count of all cases in O status with Current Child Support owed, not 
  /// paying, and both a verified address and a verified employer.
  /// </summary>
  [JsonPropertyName("nopayAddEmpV")]
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NopayAddEmpV
  {
    get => nopayAddEmpV;
    set => nopayAddEmpV = value;
  }

  /// <summary>
  /// The value of the NOPAY_ADD_EMP_NV attribute.
  /// The count of all cases in O status with Current Child Support owed, not 
  /// paying, and both a non-verified address and a non-verified employer.
  /// </summary>
  [JsonPropertyName("nopayAddEmpNv")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NopayAddEmpNv
  {
    get => nopayAddEmpNv;
    set => nopayAddEmpNv = value;
  }

  /// <summary>
  /// The value of the NON_CS_OB_CASES attribute.
  /// The count of all cases in O status with any obligation other than 
  /// Current Child Support.
  /// </summary>
  [JsonPropertyName("nonCsObCases")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NonCsObCases
  {
    get => nonCsObCases;
    set => nonCsObCases = value;
  }

  /// <summary>
  /// The value of the NOOB_CASES attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order.
  /// </summary>
  [JsonPropertyName("noobCases")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobCases
  {
    get => noobCases;
    set => noobCases = value;
  }

  /// <summary>
  /// The value of the NOOB_PAT attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order and at least one child on the case does
  /// not have paternity established.
  /// </summary>
  [JsonPropertyName("noobPat")]
  [Member(Index = 20, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobPat
  {
    get => noobPat;
    set => noobPat = value;
  }

  /// <summary>
  /// The value of the NOOB_PAT_NOADDEMP attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, at least one child on the case does
  /// not have paternity established, and the AP has no active address or
  /// employer.
  /// </summary>
  [JsonPropertyName("noobPatNoaddemp")]
  [Member(Index = 21, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobPatNoaddemp
  {
    get => noobPatNoaddemp;
    set => noobPatNoaddemp = value;
  }

  /// <summary>
  /// The value of the NOOB_PAT_ADD attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, at least one child on the case does
  /// not have paternity established, and the AP has an address only.
  /// </summary>
  [JsonPropertyName("noobPatAdd")]
  [Member(Index = 22, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobPatAdd
  {
    get => noobPatAdd;
    set => noobPatAdd = value;
  }

  /// <summary>
  /// The value of the NOOB_PAT_ADD_V attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, at least one child on the case does
  /// not have paternity established, and the AP has a verified address only.
  /// </summary>
  [JsonPropertyName("noobPatAddV")]
  [Member(Index = 23, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobPatAddV
  {
    get => noobPatAddV;
    set => noobPatAddV = value;
  }

  /// <summary>
  /// The value of the NOOB_PAT_ADD_NV attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, at least one child on the case does
  /// not have paternity established, and the AP has a non-verified address
  /// only.
  /// </summary>
  [JsonPropertyName("noobPatAddNv")]
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobPatAddNv
  {
    get => noobPatAddNv;
    set => noobPatAddNv = value;
  }

  /// <summary>
  /// The value of the NOOB_PAT_EMP attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, at least one child on the case does
  /// not have paternity established, and the AP has an employer only.
  /// </summary>
  [JsonPropertyName("noobPatEmp")]
  [Member(Index = 25, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobPatEmp
  {
    get => noobPatEmp;
    set => noobPatEmp = value;
  }

  /// <summary>
  /// The value of the NOOB_PAT_EMP_V attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, at least one child on the case does
  /// not have paternity established, and the AP has a verified employer only.
  /// </summary>
  [JsonPropertyName("noobPatEmpV")]
  [Member(Index = 26, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobPatEmpV
  {
    get => noobPatEmpV;
    set => noobPatEmpV = value;
  }

  /// <summary>
  /// The value of the NOOB_PAT_EMP_NV attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, at least one child on the case does
  /// not have paternity established, and the AP has a non-verified employer
  /// only.
  /// </summary>
  [JsonPropertyName("noobPatEmpNv")]
  [Member(Index = 27, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobPatEmpNv
  {
    get => noobPatEmpNv;
    set => noobPatEmpNv = value;
  }

  /// <summary>
  /// The value of the NOOB_PAT_ADDEMP attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, at least one child on the case does
  /// not have paternity established, and the AP has both an address and
  /// employer.
  /// </summary>
  [JsonPropertyName("noobPatAddemp")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobPatAddemp
  {
    get => noobPatAddemp;
    set => noobPatAddemp = value;
  }

  /// <summary>
  /// The value of the NOOB_PAT_ADDEMP_V attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, at least one child on the case does
  /// not have paternity established, and the AP has both a verified address and
  /// verified employer.
  /// </summary>
  [JsonPropertyName("noobPatAddempV")]
  [Member(Index = 29, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobPatAddempV
  {
    get => noobPatAddempV;
    set => noobPatAddempV = value;
  }

  /// <summary>
  /// The value of the NOOB_PAT_ADDEMP_NV attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, at least one child on the case does
  /// not have paternity established, and the AP has both a non-verified address
  /// and non-verified employer.
  /// </summary>
  [JsonPropertyName("noobPatAddempNv")]
  [Member(Index = 30, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobPatAddempNv
  {
    get => noobPatAddempNv;
    set => noobPatAddempNv = value;
  }

  /// <summary>
  /// The value of the NOOB_NOPAT attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order and all children have paternity
  /// established.
  /// </summary>
  [JsonPropertyName("noobNopat")]
  [Member(Index = 31, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobNopat
  {
    get => noobNopat;
    set => noobNopat = value;
  }

  /// <summary>
  /// The value of the NOOB_NOPAT_NOADDEM attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, all children have paternity
  /// established, and the AP has no active address or employer.
  /// </summary>
  [JsonPropertyName("noobNopatNoaddem")]
  [Member(Index = 32, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobNopatNoaddem
  {
    get => noobNopatNoaddem;
    set => noobNopatNoaddem = value;
  }

  /// <summary>
  /// The value of the NOOB_NOPAT_ADD attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, all children have paternity
  /// established, and the AP has no active address or employer.
  /// </summary>
  [JsonPropertyName("noobNopatAdd")]
  [Member(Index = 33, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobNopatAdd
  {
    get => noobNopatAdd;
    set => noobNopatAdd = value;
  }

  /// <summary>
  /// The value of the NOOB_NOPAT_ADD_V attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, all children have paternity
  /// established, and the AP has a verified address only.
  /// </summary>
  [JsonPropertyName("noobNopatAddV")]
  [Member(Index = 34, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobNopatAddV
  {
    get => noobNopatAddV;
    set => noobNopatAddV = value;
  }

  /// <summary>
  /// The value of the NOOB_NOPAT_ADD_NV attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, all children have paternity
  /// established, and the AP has a non-verified address only.
  /// </summary>
  [JsonPropertyName("noobNopatAddNv")]
  [Member(Index = 35, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobNopatAddNv
  {
    get => noobNopatAddNv;
    set => noobNopatAddNv = value;
  }

  /// <summary>
  /// The value of the NOOB_NOPAT_EMP attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, all children have paternity
  /// established, and the AP has an employer only.
  /// </summary>
  [JsonPropertyName("noobNopatEmp")]
  [Member(Index = 36, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobNopatEmp
  {
    get => noobNopatEmp;
    set => noobNopatEmp = value;
  }

  /// <summary>
  /// The value of the NOOB_NOPAT_EMP_V attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, all children have paternity
  /// established, and the AP has a verified employer only.
  /// </summary>
  [JsonPropertyName("noobNopatEmpV")]
  [Member(Index = 37, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobNopatEmpV
  {
    get => noobNopatEmpV;
    set => noobNopatEmpV = value;
  }

  /// <summary>
  /// The value of the NOOB_NOPAT_EMP_NV attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, all children have paternity
  /// established, and the AP has a non-verified employer only.
  /// </summary>
  [JsonPropertyName("noobNopatEmpNv")]
  [Member(Index = 38, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobNopatEmpNv
  {
    get => noobNopatEmpNv;
    set => noobNopatEmpNv = value;
  }

  /// <summary>
  /// The value of the NOOB_NOPAT_ADEM attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, all children have paternity
  /// established, and the AP has both an address and an employer.
  /// </summary>
  [JsonPropertyName("noobNopatAdem")]
  [Member(Index = 39, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobNopatAdem
  {
    get => noobNopatAdem;
    set => noobNopatAdem = value;
  }

  /// <summary>
  /// The value of the NOOB_NOPAT_ADEM_V attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, all children have paternity
  /// established, and the AP has both a verified address and a verified
  /// employer.
  /// </summary>
  [JsonPropertyName("noobNopatAdemV")]
  [Member(Index = 40, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobNopatAdemV
  {
    get => noobNopatAdemV;
    set => noobNopatAdemV = value;
  }

  /// <summary>
  /// The value of the NOOB_NOPAT_ADEM_NV attribute.
  /// The count of all cases in O status with that do not meet the federal 
  /// definition of a Case Under Order, all children have paternity
  /// established, and the AP has both a non-verified address and a non-verified
  /// employer.
  /// </summary>
  [JsonPropertyName("noobNopatAdemNv")]
  [Member(Index = 41, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NoobNopatAdemNv
  {
    get => noobNopatAdemNv;
    set => noobNopatAdemNv = value;
  }

  private int reportMonth;
  private int runNumber;
  private DateTime? asOfDate;
  private int? totalCases;
  private int? csCases;
  private int? payingCases;
  private int? nopayCases;
  private int? nopayNoaddemp;
  private int? nopayAdd;
  private int? nopayAddV;
  private int? nopayAddNv;
  private int? nopayEmp;
  private int? nopayEmpV;
  private int? nopayEmpNv;
  private int? nopayAddEmp;
  private int? nopayAddEmpV;
  private int? nopayAddEmpNv;
  private int? nonCsObCases;
  private int? noobCases;
  private int? noobPat;
  private int? noobPatNoaddemp;
  private int? noobPatAdd;
  private int? noobPatAddV;
  private int? noobPatAddNv;
  private int? noobPatEmp;
  private int? noobPatEmpV;
  private int? noobPatEmpNv;
  private int? noobPatAddemp;
  private int? noobPatAddempV;
  private int? noobPatAddempNv;
  private int? noobNopat;
  private int? noobNopatNoaddem;
  private int? noobNopatAdd;
  private int? noobNopatAddV;
  private int? noobNopatAddNv;
  private int? noobNopatEmp;
  private int? noobNopatEmpV;
  private int? noobNopatEmpNv;
  private int? noobNopatAdem;
  private int? noobNopatAdemV;
  private int? noobNopatAdemNv;
}
