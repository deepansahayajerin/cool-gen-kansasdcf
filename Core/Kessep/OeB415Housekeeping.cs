// Program: OE_B415_HOUSEKEEPING, ID: 371416711, model: 746.
// Short name: SWE00061
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B415_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class OeB415Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B415_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB415Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB415Housekeeping.
  /// </summary>
  public OeB415Housekeeping(IContext context, Import import, Export export):
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
    export.ProgramProcessingInfo.Name = "SWEEB415";
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

    // ***************************************************
    // * Call external to OPEN the FCR Alert file.       *
    // ***************************************************
    local.Eab.FileInstruction = "OPEN";
    UseOeEabReadOutFcrAlertRecord();

    if (!IsEmpty(local.Eab.TextReturnCode))
    {
      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    // ***************************************************
    // * Call external to OPEN the FCR read master file.       *
    // ***************************************************
    local.Eab.FileInstruction = "OPEN";
    UseSiEabReadOutgoingFcrFile();

    if (!Equal(local.Eab.TextReturnCode, "00"))
    {
      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    // ***************************************************
    // * Call external to OPEN the FCR write master file.       *
    // ***************************************************
    local.Eab.FileInstruction = "OPEN";
    UseSiEabWriteOutgoingFcrFile();

    if (!Equal(local.Eab.TextReturnCode, "OK"))
    {
      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
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
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.RestartInd = source.RestartInd;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
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

  private void UseOeEabReadOutFcrAlertRecord()
  {
    var useImport = new OeEabReadOutFcrAlertRecord.Import();
    var useExport = new OeEabReadOutFcrAlertRecord.Export();

    MoveExternal(local.Eab, useImport.ExternalFileStatus);
    useExport.FcrAlert.Assign(local.FcrAlert);
    MoveExternal(local.Eab, useExport.ExternalFileStatus);

    Call(OeEabReadOutFcrAlertRecord.Execute, useImport, useExport);

    local.FcrAlert.Assign(useExport.FcrAlert);
    local.Eab.Assign(useExport.ExternalFileStatus);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = export.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      export.ProgramProcessingInfo);
  }

  private void UseSiEabReadOutgoingFcrFile()
  {
    var useImport = new SiEabReadOutgoingFcrFile.Import();
    var useExport = new SiEabReadOutgoingFcrFile.Export();

    useImport.External.FileInstruction = local.Eab.FileInstruction;
    useExport.External.Assign(local.Eab);

    Call(SiEabReadOutgoingFcrFile.Execute, useImport, useExport);

    local.Eab.Assign(useExport.External);
  }

  private void UseSiEabWriteOutgoingFcrFile()
  {
    var useImport = new SiEabWriteOutgoingFcrFile.Import();
    var useExport = new SiEabWriteOutgoingFcrFile.Export();

    useImport.External.FileInstruction = local.Eab.FileInstruction;
    useExport.External.Assign(local.Eab);

    Call(SiEabWriteOutgoingFcrFile.Execute, useImport, useExport);

    local.Eab.Assign(useExport.External);
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
    /// A value of FcrAlert.
    /// </summary>
    [JsonPropertyName("fcrAlert")]
    public FcrAlertRecord FcrAlert
    {
      get => fcrAlert ??= new();
      set => fcrAlert = value;
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
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public External Eab
    {
      get => eab ??= new();
      set => eab = value;
    }

    private FcrAlertRecord fcrAlert;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External eab;
  }
#endregion
}
