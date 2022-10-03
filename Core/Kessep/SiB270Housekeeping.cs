// Program: SI_B270_HOUSEKEEPING, ID: 373305684, model: 746.
// Short name: SWE01304
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B270_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SiB270Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B270_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB270Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB270Housekeeping.
  /// </summary>
  public SiB270Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF";

      // ***************************************************************
      // 11/04/98  C. OTT  -   All 'Hard' errors will cause an Abort exit state 
      // to be set.  'Hard' errors are database errors and file handling errors.
      // ****************************************************************
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***************************************************
    // *Open the ERROR RPT.  DDNAME=RPT99.
    // ***************************************************
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = export.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

      // ***************************************************************
      // 11/04/98  C. OTT  -   All 'Hard' errors will cause an Abort exit state 
      // to be set.  'Hard' errors are database errors and file handling errors.
      // ****************************************************************
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***************************************************
    // *Open the CONTROL RPT. DDNAME=RPT98.
    // ***************************************************
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Control Report.";
      UseCabErrorReport();

      if (Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "SI0000_ERR_OPENING_CTL_RPT_FILE";
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";
      }

      // ***************************************************************
      // 11/04/98  C. OTT  -   All 'Hard' errors will cause an Abort exit state 
      // to be set.  'Hard' errors are database errors and file handling errors.
      // ****************************************************************
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.ExtractPersonInfo.FileInstruction = "OPEN";
    UseSiEabWritePersonInfo();

    if (!IsEmpty(local.ExtractPersonInfo.TextReturnCode))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Person Information extract file.";
      UseCabErrorReport();

      if (Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FILE_OPEN_ERROR";
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";
      }

      // ***************************************************************
      // 11/04/98  C. OTT  -   All 'Hard' errors will cause an Abort exit state 
      // to be set.  'Hard' errors are database errors and file handling errors.
      // ****************************************************************
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = export.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiEabWritePersonInfo()
  {
    var useImport = new SiEabWritePersonInfo.Import();
    var useExport = new SiEabWritePersonInfo.Export();

    useImport.External.Assign(local.ExtractPersonInfo);
    useExport.External.Assign(local.ExtractPersonInfo);

    Call(SiEabWritePersonInfo.Execute, useImport, useExport);

    local.ExtractPersonInfo.Assign(useExport.External);
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of ExtractPersonInfo.
    /// </summary>
    [JsonPropertyName("extractPersonInfo")]
    public External ExtractPersonInfo
    {
      get => extractPersonInfo ??= new();
      set => extractPersonInfo = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private External extractPersonInfo;
  }
#endregion
}
