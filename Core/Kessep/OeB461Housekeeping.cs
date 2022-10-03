// Program: OE_B461_HOUSEKEEPING, ID: 371393490, model: 746.
// Short name: SWE03616
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B461_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class OeB461Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B461_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB461Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB461Housekeeping.
  /// </summary>
  public OeB461Housekeeping(IContext context, Import import, Export export):
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
    local.ProgramProcessingInfo.Name = "SWEEB461";
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

    // ************************************************
    // * Call external to OPEN the input file.       *
    // ************************************************
    local.PassArea.FileInstruction = "OPEN";
    UseOeEabRestrictConfirmDenied();

    if (!Equal(local.PassArea.TextReturnCode, "00"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening KDMV intput file.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ************************************************
    // * WRITE TO CONTROL REPORT THE PROGRAM HAS BEEN RESTARTED    *
    // ************************************************
    if (export.ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault() > 0)
    {
      local.RecordStartAt.Text15 =
        NumberToString(export.ProgramCheckpointRestart.CheckpointCount.
          GetValueOrDefault(), 15);
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "PROGRAM RESTARTED AT RECORD NUMBER: " + local
        .RecordStartAt.Text15;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ************************************************
    // * Call external to OPEN the detail report cab for mis-match records from 
    // kdmv.       *
    // ************************************************
    local.Open.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.Open.BlankLineAfterHeading = "Y";
    local.Open.ProgramName = "SWEEB461";
    local.Open.NumberOfColHeadings = 1;
    local.Open.RptHeading3 =
      "           KS DRIVERS LICENSE MANUAL REINSTATEMENTS";
    local.Name.Text33 = "LAST NAME           FIRST NAME";
    local.SsnDob.Text23 = "   SSN         DOB";
    local.PersonNumDrvLicense.Text25 = "   PERSON #     KS DL #";
    local.ProblemText.Text40 = "   STAT  EXCEPTION REASON";
    local.Open.ColHeading1 = local.Name.Text33 + local.SsnDob.Text23 + local
      .PersonNumDrvLicense.Text25 + local.ProblemText.Text40;
    local.EabFileHandling.Action = "OPEN";
    UseCabBusinessReport01();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered opening driver file.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.NumberOfColHeadings = source.NumberOfColHeadings;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
    target.ColHeading1 = source.ColHeading1;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.Open, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

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
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeEabRestrictConfirmDenied()
  {
    var useImport = new OeEabRestrictConfirmDenied.Import();
    var useExport = new OeEabRestrictConfirmDenied.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.External.Assign(local.PassArea);

    Call(OeEabRestrictConfirmDenied.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
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
    /// A value of RecordStartAt.
    /// </summary>
    [JsonPropertyName("recordStartAt")]
    public WorkArea RecordStartAt
    {
      get => recordStartAt ??= new();
      set => recordStartAt = value;
    }

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
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public WorkArea Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of SsnDob.
    /// </summary>
    [JsonPropertyName("ssnDob")]
    public WorkArea SsnDob
    {
      get => ssnDob ??= new();
      set => ssnDob = value;
    }

    /// <summary>
    /// A value of PersonNumDrvLicense.
    /// </summary>
    [JsonPropertyName("personNumDrvLicense")]
    public WorkArea PersonNumDrvLicense
    {
      get => personNumDrvLicense ??= new();
      set => personNumDrvLicense = value;
    }

    /// <summary>
    /// A value of ProblemText.
    /// </summary>
    [JsonPropertyName("problemText")]
    public WorkArea ProblemText
    {
      get => problemText ??= new();
      set => problemText = value;
    }

    private WorkArea recordStartAt;
    private EabReportSend a03eports;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private EabReportSend open;
    private WorkArea name;
    private WorkArea ssnDob;
    private WorkArea personNumDrvLicense;
    private WorkArea problemText;
  }
#endregion
}
