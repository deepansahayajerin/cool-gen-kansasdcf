// Program: FN_BF12_PRINT_MO_OBLIGEE_SUM, ID: 373333835, model: 746.
// Short name: SWE02732
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BF12_PRINT_MO_OBLIGEE_SUM.
/// </summary>
[Serializable]
public partial class FnBf12PrintMoObligeeSum: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF12_PRINT_MO_OBLIGEE_SUM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf12PrintMoObligeeSum(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf12PrintMoObligeeSum.
  /// </summary>
  public FnBf12PrintMoObligeeSum(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // 2001-05-31  WR 000235  Fangman - New AB to print out AR monthly summary 
    // table info.
    // ***************************************************
    local.EabFileHandling.Action = "WRITE";
    local.MoYr.Text5 =
      NumberToString(import.MonthlyObligeeSummary.Month, 14, 2) + "/" + NumberToString
      (import.MonthlyObligeeSummary.Year, 14, 2);
    local.FormattedCollCnt.Text2 =
      NumberToString(import.MonthlyObligeeSummary.NumberOfCollections.
        GetValueOrDefault(), 14, 2);
    local.UnformattedNumber.Number112 =
      import.MonthlyObligeeSummary.CollectionsAmount.GetValueOrDefault();
    UseCabFormat112AmtFieldTo1();
    local.UnformattedNumber.Number112 =
      import.MonthlyObligeeSummary.AdcReimbursedAmount.GetValueOrDefault();
    UseCabFormat112AmtFieldTo2();
    local.UnformattedNumber.Number112 =
      import.MonthlyObligeeSummary.CollectionsDisbursedToAr.GetValueOrDefault();
      
    UseCabFormat112AmtFieldTo3();
    local.UnformattedNumber.Number112 =
      import.MonthlyObligeeSummary.FeeAmount.GetValueOrDefault();
    UseCabFormat112AmtFieldTo4();
    local.UnformattedNumber.Number112 =
      import.MonthlyObligeeSummary.DisbursementsSuppressed.GetValueOrDefault();
    UseCabFormat112AmtFieldTo5();
    local.UnformattedNumber.Number112 =
      import.MonthlyObligeeSummary.RecapturedAmt.GetValueOrDefault();
    UseCabFormat112AmtFieldTo6();
    local.UnformattedNumber.Number112 =
      import.MonthlyObligeeSummary.PassthruAmount;
    UseCabFormat112AmtFieldTo7();
    local.UnformattedNumber.Number112 =
      import.MonthlyObligeeSummary.TotExcessUraAmt.GetValueOrDefault();
    UseCabFormat112AmtFieldTo9();
    local.EabReportSend.RptDetail = local.MoYr.Text5 + " " + local
      .FormattedCollCnt.Text2 + " " + local.FormattedCollAmt.Text9 + " " + local
      .FormattedAfAmt.Text9 + " " + local.FormattedNaAmt.Text9 + " " + local
      .FormattedFees.Text9 + " " + local.FormattedSuppr.Text9 + " " + local
      .FormattedRecap.Text9 + " " + local.FormattedPassthru.Text9 + " " + local
      .FormattedXUra.Text9;
    UseCabControlReport();
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabFormat112AmtFieldTo1()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.UnformattedNumber.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedCollAmt.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo2()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.UnformattedNumber.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedAfAmt.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo3()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.UnformattedNumber.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedNaAmt.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo4()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.UnformattedNumber.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedFees.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo5()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.UnformattedNumber.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedSuppr.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo6()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.UnformattedNumber.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedRecap.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo7()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.UnformattedNumber.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedPassthru.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo9()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.UnformattedNumber.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedXUra.Text9 = useExport.Formatted112AmtField.Text9;
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
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    private MonthlyObligeeSummary monthlyObligeeSummary;
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
    /// A value of MoYr.
    /// </summary>
    [JsonPropertyName("moYr")]
    public WorkArea MoYr
    {
      get => moYr ??= new();
      set => moYr = value;
    }

    /// <summary>
    /// A value of FormattedCollCnt.
    /// </summary>
    [JsonPropertyName("formattedCollCnt")]
    public WorkArea FormattedCollCnt
    {
      get => formattedCollCnt ??= new();
      set => formattedCollCnt = value;
    }

    /// <summary>
    /// A value of UnformattedNumber.
    /// </summary>
    [JsonPropertyName("unformattedNumber")]
    public NumericWorkSet UnformattedNumber
    {
      get => unformattedNumber ??= new();
      set => unformattedNumber = value;
    }

    /// <summary>
    /// A value of FormattedCollAmt.
    /// </summary>
    [JsonPropertyName("formattedCollAmt")]
    public WorkArea FormattedCollAmt
    {
      get => formattedCollAmt ??= new();
      set => formattedCollAmt = value;
    }

    /// <summary>
    /// A value of FormattedAfAmt.
    /// </summary>
    [JsonPropertyName("formattedAfAmt")]
    public WorkArea FormattedAfAmt
    {
      get => formattedAfAmt ??= new();
      set => formattedAfAmt = value;
    }

    /// <summary>
    /// A value of FormattedNaAmt.
    /// </summary>
    [JsonPropertyName("formattedNaAmt")]
    public WorkArea FormattedNaAmt
    {
      get => formattedNaAmt ??= new();
      set => formattedNaAmt = value;
    }

    /// <summary>
    /// A value of FormattedFees.
    /// </summary>
    [JsonPropertyName("formattedFees")]
    public WorkArea FormattedFees
    {
      get => formattedFees ??= new();
      set => formattedFees = value;
    }

    /// <summary>
    /// A value of FormattedSuppr.
    /// </summary>
    [JsonPropertyName("formattedSuppr")]
    public WorkArea FormattedSuppr
    {
      get => formattedSuppr ??= new();
      set => formattedSuppr = value;
    }

    /// <summary>
    /// A value of FormattedRecap.
    /// </summary>
    [JsonPropertyName("formattedRecap")]
    public WorkArea FormattedRecap
    {
      get => formattedRecap ??= new();
      set => formattedRecap = value;
    }

    /// <summary>
    /// A value of FormattedPassthru.
    /// </summary>
    [JsonPropertyName("formattedPassthru")]
    public WorkArea FormattedPassthru
    {
      get => formattedPassthru ??= new();
      set => formattedPassthru = value;
    }

    /// <summary>
    /// A value of FormattedXUra.
    /// </summary>
    [JsonPropertyName("formattedXUra")]
    public WorkArea FormattedXUra
    {
      get => formattedXUra ??= new();
      set => formattedXUra = value;
    }

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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private WorkArea moYr;
    private WorkArea formattedCollCnt;
    private NumericWorkSet unformattedNumber;
    private WorkArea formattedCollAmt;
    private WorkArea formattedAfAmt;
    private WorkArea formattedNaAmt;
    private WorkArea formattedFees;
    private WorkArea formattedSuppr;
    private WorkArea formattedRecap;
    private WorkArea formattedPassthru;
    private WorkArea formattedXUra;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
