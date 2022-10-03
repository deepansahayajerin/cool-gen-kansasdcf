// Program: FN_B738_HOUSEKEEPING, ID: 1902605625, model: 746.
// Short name: SWE01226
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B738_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB738Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B738_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB738Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB738Housekeeping.
  /// </summary>
  public FnB738Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************
    // *Get the run parameters for this program.     *
    // ***********************************************
    local.ProgramProcessingInfo.Name = "SWEFB738";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    switch(Month(export.ProgramProcessingInfo.ProcessDate))
    {
      case 1:
        local.Month.Text12 = "JANUARY";

        break;
      case 2:
        local.Month.Text12 = "FEBUARY";

        break;
      case 3:
        local.Month.Text12 = "MARCH";

        break;
      case 4:
        local.Month.Text12 = "APRIL";

        break;
      case 5:
        local.Month.Text12 = "MAY";

        break;
      case 6:
        local.Month.Text12 = "JUNE";

        break;
      case 7:
        local.Month.Text12 = "JULY";

        break;
      case 8:
        local.Month.Text12 = "AUGUST";

        break;
      case 9:
        local.Month.Text12 = "SEPTEMBER";

        break;
      case 10:
        local.Month.Text12 = "OCTOBER";

        break;
      case 11:
        local.Month.Text12 = "NOVEMBER";

        break;
      case 12:
        local.Month.Text12 = "DECEMBER";

        break;
      default:
        break;
    }

    local.Year.Text4 =
      NumberToString(Year(export.ProgramProcessingInfo.ProcessDate), 12, 4);
    local.Date.Text17 = TrimEnd(" FY") + " " + local.Year.Text4;

    // ************************************************
    // * Call external to OPEN the detail report cab.       *
    // ************************************************
    local.Open.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.Open.BlankLineAfterHeading = "Y";
    local.Open.ProgramName = "SWEFB738";
    local.Open.NumberOfColHeadings = 0;
    local.EabFileHandling.Action = "OPEN";
    UseEabExternalReportWriter();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered opening report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabReportSend.RptDetail =
      " CSE                                             KANSAS DEPARTMENT FOR CHILDREN AND FAMILIES";
      
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "                                                           CHILD SUPPORT SERVICES";
      
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Name.Text80 =
      "                                                   SRRUN293 - ANNUAL DA32 REPORT";
      
    local.EabReportSend.RptDetail = local.Name.Text80 + local.Date.Text17;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.EabReportSend.RptDetail =
      "                                   Past Due Recoveries         Account for Collections       Account for coll adj      Total FY";
      
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.EabReportSend.RptDetail =
      "                                   prior to post FY adjust     rec'd after FY end (+)        made after FY end (-)     Recovery Due";
      
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExternalReportWriter()
  {
    var useImport = new EabExternalReportWriter.Import();
    var useExport = new EabExternalReportWriter.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.RptDetail = local.EabReportSend.RptDetail;
    MoveExternal(local.PassArea, useExport.External);

    Call(EabExternalReportWriter.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
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
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Report.
    /// </summary>
    [JsonPropertyName("report")]
    public External Report
    {
      get => report ??= new();
      set => report = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public WorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public WorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public WorkArea Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public WorkArea Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    private External report;
    private WorkArea date;
    private WorkArea year;
    private WorkArea month;
    private WorkArea name;
    private EabReportSend open;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
  }
#endregion
}
