// Program: OE_B450_HOUSEKEEPING, ID: 371288946, model: 746.
// Short name: SWE01972
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B450_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class OeB450Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B450_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB450Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB450Housekeeping.
  /// </summary>
  public OeB450Housekeeping(IContext context, Import import, Export export):
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
    local.ProgramProcessingInfo.Name = "SWEEB450";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

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

    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **********************************************************************************************
    // 12/01/2005                    DDupree                 WR258947
    // Added the following opens for the reports needed for 'A03' processing.
    // **********************************************************************************************
    // **********************************************************
    // OPEN OUTPUT REPORT 03 - EMPLOYEE NAME MISMATCH
    // **********************************************************
    local.A03eports.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    export.ProgramProcessingInfo.Name = export.ProgramProcessingInfo.Name;
    local.A03eports.RptHeading3 =
      "          NATIONAL SECURITY AGENCY NAME MISMATCH";
    local.A03eports.NumberOfColHeadings = 2;
    local.A03eports.ColHeading1 =
      " Field      NSA Information                       KAECSES Information";
    local.A03eports.ColHeading2 =
      "----------  ---------------------                 -------------------";
    UseCabBusinessReport03();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT REPORT 04 - EMPLOYER NAME OR ADDRESS MISMATCH
    // **********************************************************
    local.A03eports.RptHeading3 =
      "NATIONAL SECURITY AGENCY'S NAME OR ADDRESS MISMATCH";
    local.A03eports.NumberOfColHeadings = 2;
    local.A03eports.ColHeading1 =
      " Field         NSA   Information              KAECSES Information";
    local.A03eports.ColHeading2 =
      "-------- ---------------------------------    -------------------";
    UseCabBusinessReport04();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT REPORT 05 - REVISED EARNINGS
    // **********************************************************
    local.A03eports.RptHeading3 =
      "         NATIONAL SECURITY AGENCY - REVISED EARNINGS";
    local.A03eports.NumberOfColHeadings = 3;
    local.A03eports.ColHeading1 = " PERSON     EMPLOYER";
    local.A03eports.ColHeading2 =
      " NUMBER     NAME                              YEAR QTR";
    local.A03eports.ColHeading3 =
      "---------- ---------------------------------- ---- ---";
    UseCabBusinessReport05();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ************************************************
    // * Call external to OPEN the driver file.       *
    // ************************************************
    local.PassArea.FileInstruction = "OPEN";
    UseOeEabReceiveFplsResponse();

    if (!IsEmpty(local.PassArea.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered opening driver file.";
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

  private void UseCabBusinessReport03()
  {
    var useImport = new CabBusinessReport03.Import();
    var useExport = new CabBusinessReport03.Export();

    useImport.NeededToOpen.Assign(local.A03eports);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport03.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport04()
  {
    var useImport = new CabBusinessReport04.Import();
    var useExport = new CabBusinessReport04.Export();

    useImport.NeededToOpen.Assign(local.A03eports);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport04.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport05()
  {
    var useImport = new CabBusinessReport05.Import();
    var useExport = new CabBusinessReport05.Export();

    useImport.NeededToOpen.Assign(local.A03eports);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport05.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseOeEabReceiveFplsResponse()
  {
    var useImport = new OeEabReceiveFplsResponse.Import();
    var useExport = new OeEabReceiveFplsResponse.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(OeEabReceiveFplsResponse.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

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

    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of A03eports.
    /// </summary>
    [JsonPropertyName("a03eports")]
    public EabReportSend A03eports
    {
      get => a03eports ??= new();
      set => a03eports = value;
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

    private EabReportSend a03eports;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
  }
#endregion
}
