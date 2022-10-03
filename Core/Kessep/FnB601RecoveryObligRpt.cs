// Program: FN_B601_RECOVERY_OBLIG_RPT, ID: 372706172, model: 746.
// Short name: SWEF601B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B601_RECOVERY_OBLIG_RPT.
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
public partial class FnB601RecoveryObligRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B601_RECOVERY_OBLIG_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB601RecoveryObligRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB601RecoveryObligRpt.
  /// </summary>
  public FnB601RecoveryObligRpt(IContext context, Import import, Export export):
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
    local.NeededToOpen.ProgramName = "SWEFB601";
    local.ProgramProcessingInfo.Name = "SWEFB601";

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

    // : Initialize the high date of the report period with the process date.
    local.High.Date = local.ProgramProcessingInfo.ProcessDate;
    UseCabDetermineReportingDates();

    // : Get the literal 'MONTHLY', 'QUARTERLY' or 'YEARLY' from the
    //   parameter list.
    local.ReportLiteral.Text10 =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 10);

    switch(TrimEnd(local.ReportLiteral.Text10))
    {
      case "MONTHLY":
        // : This has already been done by the above cab.
        break;
      case "QUARTERLY":
        // : Subtract 2 months, and call the cab to provide the first day of the
        // resulting month.
        local.Low.Date = AddMonths(local.Low.Date, -2);
        local.Low.Timestamp = AddMonths(local.Low.Timestamp, -2);

        break;
      case "YEARLY":
        // : Subtract 11 months, and call the cab to provide the first day of 
        // the resulting month.
        local.Low.Date = AddMonths(local.Low.Date, -11);
        local.Low.Timestamp = AddMonths(local.Low.Timestamp, -11);

        break;
      default:
        // : ERROR - abend.
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Invalid report period: " + local
          .ReportLiteral.Text10;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
    }

    // : Call cab to do the calculations and generate the report.
    UseFnB601CalcReportTotals();
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveTextWorkArea(TextWorkArea source, TextWorkArea target)
  {
    target.Text8 = source.Text8;
    target.Text10 = source.Text10;
  }

  private void UseCabDetermineReportingDates()
  {
    var useImport = new CabDetermineReportingDates.Import();
    var useExport = new CabDetermineReportingDates.Export();

    useImport.DateWorkArea.Date = local.High.Date;

    Call(CabDetermineReportingDates.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.Eom, local.High);
    MoveDateWorkArea(useExport.Bom, local.Low);
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnB601CalcReportTotals()
  {
    var useImport = new FnB601CalcReportTotals.Import();
    var useExport = new FnB601CalcReportTotals.Export();

    MoveTextWorkArea(local.ReportLiteral, useImport.ReportLiteral);
    MoveDateWorkArea(local.Low, useImport.Low);
    MoveDateWorkArea(local.High, useImport.High);

    Call(FnB601CalcReportTotals.Execute, useImport, useExport);
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of ReportLiteral.
    /// </summary>
    [JsonPropertyName("reportLiteral")]
    public TextWorkArea ReportLiteral
    {
      get => reportLiteral ??= new();
      set => reportLiteral = value;
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
    /// A value of MonthDifference.
    /// </summary>
    [JsonPropertyName("monthDifference")]
    public Common MonthDifference
    {
      get => monthDifference ??= new();
      set => monthDifference = value;
    }

    private ExitStateWorkArea exitStateWorkArea;
    private ProgramProcessingInfo programProcessingInfo;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private TextWorkArea reportLiteral;
    private ReportParms canam;
    private DateWorkArea high;
    private DateWorkArea low;
    private DateWorkArea zero;
    private Common monthDifference;
  }
#endregion
}
