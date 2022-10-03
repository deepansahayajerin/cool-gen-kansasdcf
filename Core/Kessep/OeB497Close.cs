// Program: OE_B497_CLOSE, ID: 371177817, model: 746.
// Short name: SWE01974
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B497_CLOSE.
/// </summary>
[Serializable]
public partial class OeB497Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B497_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB497Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB497Close.
  /// </summary>
  public OeB497Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************
    // WRITE OUTPUT CONTROL REPORT AND CLOSE
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";
    local.Subscript.Count = 1;

    do
    {
      switch(local.Subscript.Count)
      {
        case 1:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "";

          break;
        case 2:
          local.NeededToWrite.RptDetail =
            "RECORDS READ (Excl hdr & trailer)  " + "   " + NumberToString
            (import.RecordsRead.Count, 15);

          break;
        case 3:
          local.NeededToWrite.RptDetail =
            "POLICIES TERMINATED                " + "   " + NumberToString
            (import.PoliciesTerminated.Count, 15);

          break;
        case 4:
          local.NeededToWrite.RptDetail =
            "POLICIES ALREADY TERMINATED        " + "   " + NumberToString
            (import.AlreadyProcessed.Count, 15);

          break;
        case 5:
          local.NeededToWrite.RptDetail =
            "POLICIES NOT FOUND                 " + "   " + NumberToString
            (import.PoliciesNotFound.Count, 15);

          break;
        case 6:
          local.NeededToWrite.RptDetail =
            "RECORDS SKIPPED INVALID INFO       " + "   " + NumberToString
            (import.RecordsInvalidInfo.Count, 15);

          break;
        case 7:
          local.EabFileHandling.Action = "CLOSE";

          break;
        case 8:
          break;
        case 9:
          break;
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 7);

    UseEabReadTerminatedPolicyFile();
    UseCabBusinessReport01();
    UseCabErrorReport();
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadTerminatedPolicyFile()
  {
    var useImport = new EabReadTerminatedPolicyFile.Import();
    var useExport = new EabReadTerminatedPolicyFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadTerminatedPolicyFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of PoliciesTerminated.
    /// </summary>
    [JsonPropertyName("policiesTerminated")]
    public Common PoliciesTerminated
    {
      get => policiesTerminated ??= new();
      set => policiesTerminated = value;
    }

    /// <summary>
    /// A value of AlreadyProcessed.
    /// </summary>
    [JsonPropertyName("alreadyProcessed")]
    public Common AlreadyProcessed
    {
      get => alreadyProcessed ??= new();
      set => alreadyProcessed = value;
    }

    /// <summary>
    /// A value of PoliciesNotFound.
    /// </summary>
    [JsonPropertyName("policiesNotFound")]
    public Common PoliciesNotFound
    {
      get => policiesNotFound ??= new();
      set => policiesNotFound = value;
    }

    /// <summary>
    /// A value of RecordsInvalidInfo.
    /// </summary>
    [JsonPropertyName("recordsInvalidInfo")]
    public Common RecordsInvalidInfo
    {
      get => recordsInvalidInfo ??= new();
      set => recordsInvalidInfo = value;
    }

    private Common recordsRead;
    private Common policiesTerminated;
    private Common alreadyProcessed;
    private Common policiesNotFound;
    private Common recordsInvalidInfo;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
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

    private EabFileHandling eabFileHandling;
    private Common subscript;
    private EabReportSend neededToWrite;
  }
#endregion
}
