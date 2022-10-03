// Program: SP_B316_HOUSEKEEPING, ID: 374563329, model: 746.
// Short name: SWE03130
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B316_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SpB316Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B316_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB316Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB316Housekeeping.
  /// </summary>
  public SpB316Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------
    //                   M A I N T E N A N C E   L O G
    // Date		Developer	Request #	Description
    // 07/14/10	J Huss		CQ# 20646	Initial development.
    // ------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEPB316";
    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadProgramProcessingInfo();

    if (!Lt(local.Null1.Date, export.ProgramProcessingInfo.ProcessDate))
    {
      export.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    UseReadPgmCheckpointRestart();
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;

    // Open the control report
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_WRITING_TO_REPORT_AB";

      return;
    }

    // Open the error report
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "CLOSE";

      // If there was an error opening the error report, close the control 
      // report.
      UseCabControlReport1();
      ExitState = "ERROR_WRITING_TO_REPORT_AB";

      return;
    }

    UseSpB316EabReadInputFile();

    // If the file did not open successfully, close the reports.
    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = local.External.TextLine130;
      UseSpB316CloseReports();
    }
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    export.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      export.ProgramProcessingInfo);
  }

  private void UseSpB316CloseReports()
  {
    var useImport = new SpB316CloseReports.Import();
    var useExport = new SpB316CloseReports.Export();

    useImport.Error.RptDetail = local.EabReportSend.RptDetail;

    Call(SpB316CloseReports.Execute, useImport, useExport);
  }

  private void UseSpB316EabReadInputFile()
  {
    var useImport = new SpB316EabReadInputFile.Import();
    var useExport = new SpB316EabReadInputFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabFileHandling(local.EabFileHandling, useExport.EabFileHandling);
    useExport.ResultDetail.TextLine130 = local.External.TextLine130;

    Call(SpB316EabReadInputFile.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
    local.External.TextLine130 = useExport.ResultDetail.TextLine130;
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

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External external;
    private DateWorkArea null1;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
