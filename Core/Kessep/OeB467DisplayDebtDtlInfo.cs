// Program: OE_B467_DISPLAY_DEBT_DTL_INFO, ID: 374472420, model: 746.
// Short name: SWE02711
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B467_DISPLAY_DEBT_DTL_INFO.
/// </summary>
[Serializable]
public partial class OeB467DisplayDebtDtlInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B467_DISPLAY_DEBT_DTL_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB467DisplayDebtDtlInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB467DisplayDebtDtlInfo.
  /// </summary>
  public OeB467DisplayDebtDtlInfo(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Send.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabErrorReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ForFormattingNumericWorkSet.Number112 =
      import.PerDebtDetail.BalanceDueAmt;
    UseCabFormat112AmtFieldTo1();
    local.ForFormattingDateWorkArea.Date = import.PerDebtDetail.DueDt;
    UseCabFormatDate();
    local.EabReportSend.RptDetail = "AP " + import.PerObligor.Number + " Sup " +
      import.PerSupported.Number + " Dbt Dtl " + local.FormattedAmt1.Text9 + ""
      + "" + " Due " + local.FormattedDebtDetailDueDt.Text10 + " Cov PD St " + local
      .FormattedCovPdStartDt.Text10;
    UseCabErrorReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ForFormattingNumericWorkSet.Number112 = import.PerDebt.Amount;
    UseCabFormat112AmtFieldTo1();
    local.ForFormattingNumericWorkSet.Number112 = import.FcAmt.TotalCurrency;
    UseCabFormat112AmtFieldTo2();
    local.EabReportSend.RptDetail = "Ob ID " + NumberToString
      (import.PerObligation.SystemGeneratedIdentifier, 13, 3) + " Ob Typ " + NumberToString
      (import.PerObligationType.SystemGeneratedIdentifier, 13, 3) + ", " + import
      .PerObligationType.Classification + " Ob Trn ID " + NumberToString
      (import.PerDebt.SystemGeneratedIdentifier, 7, 9) + " Dbt Amt " + local
      .FormattedAmt1.Text9 + " Pgm " + import.Program.Code + "  " + local
      .FormattedAmt2.Text9;
    UseCabErrorReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.Send.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Receive.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFormat112AmtFieldTo1()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.ForFormattingNumericWorkSet.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedAmt1.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo2()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.ForFormattingNumericWorkSet.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedAmt2.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormatDate()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.ForFormattingDateWorkArea.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.FormattedDebtDetailDueDt.Text10 = useExport.FormattedDate.Text10;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of PerDebtDetail.
    /// </summary>
    [JsonPropertyName("perDebtDetail")]
    public DebtDetail PerDebtDetail
    {
      get => perDebtDetail ??= new();
      set => perDebtDetail = value;
    }

    /// <summary>
    /// A value of PerObligor.
    /// </summary>
    [JsonPropertyName("perObligor")]
    public CsePerson PerObligor
    {
      get => perObligor ??= new();
      set => perObligor = value;
    }

    /// <summary>
    /// A value of PerSupported.
    /// </summary>
    [JsonPropertyName("perSupported")]
    public CsePerson PerSupported
    {
      get => perSupported ??= new();
      set => perSupported = value;
    }

    /// <summary>
    /// A value of PerObligation.
    /// </summary>
    [JsonPropertyName("perObligation")]
    public Obligation PerObligation
    {
      get => perObligation ??= new();
      set => perObligation = value;
    }

    /// <summary>
    /// A value of PerObligationType.
    /// </summary>
    [JsonPropertyName("perObligationType")]
    public ObligationType PerObligationType
    {
      get => perObligationType ??= new();
      set => perObligationType = value;
    }

    /// <summary>
    /// A value of PerDebt.
    /// </summary>
    [JsonPropertyName("perDebt")]
    public ObligationTransaction PerDebt
    {
      get => perDebt ??= new();
      set => perDebt = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of FcAmt.
    /// </summary>
    [JsonPropertyName("fcAmt")]
    public Common FcAmt
    {
      get => fcAmt ??= new();
      set => fcAmt = value;
    }

    private DebtDetail perDebtDetail;
    private CsePerson perObligor;
    private CsePerson perSupported;
    private Obligation perObligation;
    private ObligationType perObligationType;
    private ObligationTransaction perDebt;
    private Program program;
    private Common fcAmt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Receive.
    /// </summary>
    [JsonPropertyName("receive")]
    public EabFileHandling Receive
    {
      get => receive ??= new();
      set => receive = value;
    }

    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public EabFileHandling Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of ForFormattingNumericWorkSet.
    /// </summary>
    [JsonPropertyName("forFormattingNumericWorkSet")]
    public NumericWorkSet ForFormattingNumericWorkSet
    {
      get => forFormattingNumericWorkSet ??= new();
      set => forFormattingNumericWorkSet = value;
    }

    /// <summary>
    /// A value of FormattedAmt1.
    /// </summary>
    [JsonPropertyName("formattedAmt1")]
    public WorkArea FormattedAmt1
    {
      get => formattedAmt1 ??= new();
      set => formattedAmt1 = value;
    }

    /// <summary>
    /// A value of FormattedAmt2.
    /// </summary>
    [JsonPropertyName("formattedAmt2")]
    public WorkArea FormattedAmt2
    {
      get => formattedAmt2 ??= new();
      set => formattedAmt2 = value;
    }

    /// <summary>
    /// A value of ForFormattingDateWorkArea.
    /// </summary>
    [JsonPropertyName("forFormattingDateWorkArea")]
    public DateWorkArea ForFormattingDateWorkArea
    {
      get => forFormattingDateWorkArea ??= new();
      set => forFormattingDateWorkArea = value;
    }

    /// <summary>
    /// A value of FormattedDebtDetailDueDt.
    /// </summary>
    [JsonPropertyName("formattedDebtDetailDueDt")]
    public WorkArea FormattedDebtDetailDueDt
    {
      get => formattedDebtDetailDueDt ??= new();
      set => formattedDebtDetailDueDt = value;
    }

    /// <summary>
    /// A value of FormattedCovPdStartDt.
    /// </summary>
    [JsonPropertyName("formattedCovPdStartDt")]
    public WorkArea FormattedCovPdStartDt
    {
      get => formattedCovPdStartDt ??= new();
      set => formattedCovPdStartDt = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling receive;
    private EabFileHandling send;
    private NumericWorkSet forFormattingNumericWorkSet;
    private WorkArea formattedAmt1;
    private WorkArea formattedAmt2;
    private DateWorkArea forFormattingDateWorkArea;
    private WorkArea formattedDebtDetailDueDt;
    private WorkArea formattedCovPdStartDt;
  }
#endregion
}
