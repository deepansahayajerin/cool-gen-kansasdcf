// Program: OE_B419_HOUSEKEEPING, ID: 374569357, model: 746.
// Short name: SWE00117
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B419_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class OeB419Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B419_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB419Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB419Housekeeping.
  /// </summary>
  public OeB419Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *****************************************************************
    //  Get the run parameters for this program.
    // *****************************************************************
    export.ProgramProcessingInfo.Name = "SWEEB419";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.ProcessingDate.Date = export.ProgramProcessingInfo.ProcessDate;
    export.CurrentDateLessFive.Date =
      AddDays(export.ProgramProcessingInfo.ProcessDate, -5);

    // *****************************************************************
    // Open Error Report DDNAME=RPT99 and Control Report DDNAME = RPT98 .
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWEEB419";
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

    local.Eab.FileInstruction = "OPEN";

    // **************************************************************************************
    // The below mentioned External Action Block (EAB) will read the input 
    // records from the
    // FCR Master Dataset to load the data to FCR DB2 master tables(Case & Case 
    // Member Table).
    // **************************************************************************************
    UseOeEabReadFcrExtractRecord();

    if (!IsEmpty(local.Eab.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in opening the FCR OUTPUT EXTRACT file";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // **************************************************************************************
    // Read the program checkpoint restart records for the execution program and
    // return the
    // values to the Pstep to handle the restart point.
    // **************************************************************************************
    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();
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
    target.TextLine8 = source.TextLine8;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
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

  private void UseOeEabReadFcrExtractRecord()
  {
    var useImport = new OeEabReadFcrExtractRecord.Import();
    var useExport = new OeEabReadFcrExtractRecord.Export();

    MoveExternal(local.Eab, useImport.ExternalFileStatus);
    useExport.Ext.Assign(local.OutputExtFcrOutputCaseRecord);
    useExport.FcrOutputExt.Assign(local.OutputExtFcrOutputMemberRecord);
    MoveExternal(local.Eab, useExport.ExternalFileStatus);

    Call(OeEabReadFcrExtractRecord.Execute, useImport, useExport);

    local.OutputExtFcrOutputCaseRecord.Assign(useExport.Ext);
    local.OutputExtFcrOutputMemberRecord.Assign(useExport.FcrOutputExt);
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
    /// A value of ProcessingDate.
    /// </summary>
    [JsonPropertyName("processingDate")]
    public DateWorkArea ProcessingDate
    {
      get => processingDate ??= new();
      set => processingDate = value;
    }

    /// <summary>
    /// A value of CurrentDateLessFive.
    /// </summary>
    [JsonPropertyName("currentDateLessFive")]
    public DateWorkArea CurrentDateLessFive
    {
      get => currentDateLessFive ??= new();
      set => currentDateLessFive = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private DateWorkArea processingDate;
    private DateWorkArea currentDateLessFive;
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
    /// A value of OutputExtFcrOutputCaseRecord.
    /// </summary>
    [JsonPropertyName("outputExtFcrOutputCaseRecord")]
    public FcrOutputCaseRecord OutputExtFcrOutputCaseRecord
    {
      get => outputExtFcrOutputCaseRecord ??= new();
      set => outputExtFcrOutputCaseRecord = value;
    }

    /// <summary>
    /// A value of OutputExtFcrOutputMemberRecord.
    /// </summary>
    [JsonPropertyName("outputExtFcrOutputMemberRecord")]
    public FcrOutputMemberRecord OutputExtFcrOutputMemberRecord
    {
      get => outputExtFcrOutputMemberRecord ??= new();
      set => outputExtFcrOutputMemberRecord = value;
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

    private FcrOutputCaseRecord outputExtFcrOutputCaseRecord;
    private FcrOutputMemberRecord outputExtFcrOutputMemberRecord;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External eab;
  }
#endregion
}
