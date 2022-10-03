// Program: FN_B602_RECOV_OB_RPT_BY_WRKR, ID: 372707091, model: 746.
// Short name: SWEF602B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B602_RECOV_OB_RPT_BY_WRKR.
/// </para>
/// <para>
/// This report provides totals for all recovery obligations for a specified 
/// report period - monthly, quarterly, or yearly.  The process date is used to
/// derive the date range to be used for the report.  The program calculates
/// collections, balance owed, and percentage paid for recovery obligations for
/// the report period.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB602RecovObRptByWrkr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B602_RECOV_OB_RPT_BY_WRKR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB602RecovObRptByWrkr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB602RecovObRptByWrkr.
  /// </summary>
  public FnB602RecovObRptByWrkr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    local.NeededToOpen.ProgramName = "SWEFB602";
    local.ProgramProcessingInfo.Name = "SWEFB602";

    // : Open error file.
    local.NeededToOpen.ProcessDate = Now().Date;
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // : Error - abend.
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport2();
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport3();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.High.Date = local.ProgramProcessingInfo.ProcessDate;
    UseCabFirstAndLastDateOfMonth();

    // : High date is set to the last day of whatever month process date is in.
    // If this turns out to be greater than current date, go back to the end
    // of the previous month.
    // IE: if process date is April 1, set the report end date to  March 31, not
    // April 30.
    if (Lt(local.ProgramProcessingInfo.ProcessDate, local.High.Date))
    {
      local.High.Date = AddMonths(local.High.Date, -1);
      UseCabFirstAndLastDateOfMonth();
    }

    // : Call cab to do the calculations and generate the report.
    UseFnB602CalcByWorkerId();
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.High.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.High.Date = useExport.Last.Date;
    local.Low.Date = useExport.First.Date;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnB602CalcByWorkerId()
  {
    var useImport = new FnB602CalcByWorkerId.Import();
    var useExport = new FnB602CalcByWorkerId.Export();

    useImport.Low.Date = local.Low.Date;
    useImport.High.Date = local.High.Date;

    Call(FnB602CalcByWorkerId.Execute, useImport, useExport);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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

    /// <summary>
    /// A value of Canam.
    /// </summary>
    [JsonPropertyName("canam")]
    public ReportParms Canam
    {
      get => canam ??= new();
      set => canam = value;
    }

    /// <summary>
    /// A value of High.
    /// </summary>
    [JsonPropertyName("high")]
    public DateWorkArea High
    {
      get => high ??= new();
      set => high = value;
    }

    /// <summary>
    /// A value of Low.
    /// </summary>
    [JsonPropertyName("low")]
    public DateWorkArea Low
    {
      get => low ??= new();
      set => low = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private ReportParms canam;
    private DateWorkArea high;
    private DateWorkArea low;
    private DateWorkArea zero;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea current;
    private DateWorkArea null1;
    private ExitStateWorkArea exitStateWorkArea;
  }
#endregion
}
