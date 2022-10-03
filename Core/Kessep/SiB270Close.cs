// Program: SI_B270_CLOSE, ID: 373305519, model: 746.
// Short name: SWE01305
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B270_CLOSE.
/// </summary>
[Serializable]
public partial class SiB270Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B270_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB270Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB270Close.
  /// </summary>
  public SiB270Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "No of external file records written: " + NumberToString
      (import.NumOfRecsWritten.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered writing the Control Report file.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ExtractPersonInfo.FileInstruction = "CLOSE";
    UseSiEabWritePersonInfo();

    if (!IsEmpty(local.ExtractPersonInfo.TextReturnCode))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered closing the Person Information extract file.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***************************************************
    // *Close the CONTROL REPORT.
    // ***************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered closing the Control Report file.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***************************************************
    // *Close the ERROR RPT.
    // ***************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
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
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    /// <summary>
    /// A value of NumOfRecsWritten.
    /// </summary>
    [JsonPropertyName("numOfRecsWritten")]
    public Common NumOfRecsWritten
    {
      get => numOfRecsWritten ??= new();
      set => numOfRecsWritten = value;
    }

    private Common numOfRecsWritten;
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
    /// A value of ExtractPersonInfo.
    /// </summary>
    [JsonPropertyName("extractPersonInfo")]
    public External ExtractPersonInfo
    {
      get => extractPersonInfo ??= new();
      set => extractPersonInfo = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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

    private External extractPersonInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
  }
#endregion
}
