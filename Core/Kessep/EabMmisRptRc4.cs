// Program: EAB_MMIS_RPT_RC4, ID: 372814830, model: 746.
// Short name: SWEXF230
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_MMIS_RPT_RC4.
/// </summary>
[Serializable]
public partial class EabMmisRptRc4: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_MMIS_RPT_RC4 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabMmisRptRc4(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabMmisRptRc4.
  /// </summary>
  public EabMmisRptRc4(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEXF230", context, import, export, EabOptions.Hpvp);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Parm1",
      "Parm2",
      "SubreportCode"
    })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of DateRptPeriodStart.
    /// </summary>
    [JsonPropertyName("dateRptPeriodStart")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea DateRptPeriodStart
    {
      get => dateRptPeriodStart ??= new();
      set => dateRptPeriodStart = value;
    }

    /// <summary>
    /// A value of DateRptPeriodEnd.
    /// </summary>
    [JsonPropertyName("dateRptPeriodEnd")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea DateRptPeriodEnd
    {
      get => dateRptPeriodEnd ??= new();
      set => dateRptPeriodEnd = value;
    }

    /// <summary>
    /// A value of DateOfReportRun.
    /// </summary>
    [JsonPropertyName("dateOfReportRun")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea DateOfReportRun
    {
      get => dateOfReportRun ??= new();
      set => dateOfReportRun = value;
    }

    /// <summary>
    /// A value of ControlBreak.
    /// </summary>
    [JsonPropertyName("controlBreak")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text33" })]
    public ReportText ControlBreak
    {
      get => controlBreak ??= new();
      set => controlBreak = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "FormattedName" })
      ]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ReportLineCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("reportLineCashReceiptDetail")]
    [Member(Index = 7, AccessFields = false, Members
      = new[] { "ObligorPersonNumber" })]
    public CashReceiptDetail ReportLineCashReceiptDetail
    {
      get => reportLineCashReceiptDetail ??= new();
      set => reportLineCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ReportLineCollection.
    /// </summary>
    [JsonPropertyName("reportLineCollection")]
    [Member(Index = 8, AccessFields = false, Members = new[]
    {
      "SystemGeneratedIdentifier",
      "Amount",
      "CollectionDt",
      "CollectionAdjustmentDt",
      "ProgramAppliedTo"
    })]
    public Collection ReportLineCollection
    {
      get => reportLineCollection ??= new();
      set => reportLineCollection = value;
    }

    /// <summary>
    /// A value of ReportLineDistributeDt.
    /// </summary>
    [JsonPropertyName("reportLineDistributeDt")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea ReportLineDistributeDt
    {
      get => reportLineDistributeDt ??= new();
      set => reportLineDistributeDt = value;
    }

    /// <summary>
    /// A value of ReportLineSupported.
    /// </summary>
    [JsonPropertyName("reportLineSupported")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson ReportLineSupported
    {
      get => reportLineSupported ??= new();
      set => reportLineSupported = value;
    }

    /// <summary>
    /// A value of ReportLineObligationType.
    /// </summary>
    [JsonPropertyName("reportLineObligationType")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Code" })]
    public ObligationType ReportLineObligationType
    {
      get => reportLineObligationType ??= new();
      set => reportLineObligationType = value;
    }

    /// <summary>
    /// A value of SumMc.
    /// </summary>
    [JsonPropertyName("sumMc")]
    [Member(Index = 12, AccessFields = false, Members
      = new[] { "Number120", "Currency132" })]
    public ReportTotals SumMc
    {
      get => sumMc ??= new();
      set => sumMc = value;
    }

    /// <summary>
    /// A value of SumMj.
    /// </summary>
    [JsonPropertyName("sumMj")]
    [Member(Index = 13, AccessFields = false, Members
      = new[] { "Number120", "Currency132" })]
    public ReportTotals SumMj
    {
      get => sumMj ??= new();
      set => sumMj = value;
    }

    /// <summary>
    /// A value of SumMs.
    /// </summary>
    [JsonPropertyName("sumMs")]
    [Member(Index = 14, AccessFields = false, Members
      = new[] { "Number120", "Currency132" })]
    public ReportTotals SumMs
    {
      get => sumMs ??= new();
      set => sumMs = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    [Member(Index = 15, AccessFields = false, Members
      = new[] { "Number120", "Currency132" })]
    public ReportTotals Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of FtotalMc.
    /// </summary>
    [JsonPropertyName("ftotalMc")]
    [Member(Index = 16, AccessFields = false, Members
      = new[] { "Number120", "Currency132" })]
    public ReportTotals FtotalMc
    {
      get => ftotalMc ??= new();
      set => ftotalMc = value;
    }

    /// <summary>
    /// A value of FtotalMj.
    /// </summary>
    [JsonPropertyName("ftotalMj")]
    [Member(Index = 17, AccessFields = false, Members
      = new[] { "Number120", "Currency132" })]
    public ReportTotals FtotalMj
    {
      get => ftotalMj ??= new();
      set => ftotalMj = value;
    }

    /// <summary>
    /// A value of FtotalMs.
    /// </summary>
    [JsonPropertyName("ftotalMs")]
    [Member(Index = 18, AccessFields = false, Members
      = new[] { "Number120", "Currency132" })]
    public ReportTotals FtotalMs
    {
      get => ftotalMs ??= new();
      set => ftotalMs = value;
    }

    /// <summary>
    /// A value of FoverallTotal.
    /// </summary>
    [JsonPropertyName("foverallTotal")]
    [Member(Index = 19, AccessFields = false, Members
      = new[] { "Number120", "Currency132" })]
    public ReportTotals FoverallTotal
    {
      get => foverallTotal ??= new();
      set => foverallTotal = value;
    }

    /// <summary>
    /// A value of Errors.
    /// </summary>
    [JsonPropertyName("errors")]
    [Member(Index = 20, AccessFields = false, Members
      = new[] { "Number120", "Currency132" })]
    public ReportTotals Errors
    {
      get => errors ??= new();
      set => errors = value;
    }

    private ReportParms reportParms;
    private DateWorkArea dateRptPeriodStart;
    private DateWorkArea dateRptPeriodEnd;
    private DateWorkArea dateOfReportRun;
    private ReportText controlBreak;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CashReceiptDetail reportLineCashReceiptDetail;
    private Collection reportLineCollection;
    private DateWorkArea reportLineDistributeDt;
    private CsePerson reportLineSupported;
    private ObligationType reportLineObligationType;
    private ReportTotals sumMc;
    private ReportTotals sumMj;
    private ReportTotals sumMs;
    private ReportTotals total;
    private ReportTotals ftotalMc;
    private ReportTotals ftotalMj;
    private ReportTotals ftotalMs;
    private ReportTotals foverallTotal;
    private ReportTotals errors;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Parm1",
      "Parm2",
      "SubreportCode"
    })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private ReportParms reportParms;
  }
#endregion
}
