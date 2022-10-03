// Program: OE_B415_CLOSE, ID: 371416956, model: 746.
// Short name: SWE00062
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B415_CLOSE.
/// </summary>
[Serializable]
public partial class OeB415Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B415_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB415Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB415Close.
  /// </summary>
  public OeB415Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Eab.FileInstruction = "CLOSE";
    UseOeEabReadOutFcrAlertRecord();

    if (!IsEmpty(local.Eab.TextReturnCode))
    {
      ExitState = "FILE_CLOSE_ERROR";

      return;
    }

    local.Common.Subscript = 1;
    local.EabFileHandling.Action = "WRITE";

    do
    {
      switch(local.Common.Subscript)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Total Number of SSN Change Records Read               : " + NumberToString
            (import.RecordsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Total Number of SSN Change Records generated Events   : " + NumberToString
            (import.TotalSsnChgInputRecs.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Total Number of SSN Change Records Skipped            : " + NumberToString
            (import.SkipRecordCount.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "   Total Number of CSE Person not found records       : " + NumberToString
            (import.RecordsPersonNotFound.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "Total Number of SSN Change History Records Created    : " + NumberToString
            (import.TotalHistRecordsCreate.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "   Total Primary   SSN Change History Records Created : " + NumberToString
            (import.PriSsnHistRecsCreated.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "   Total Alternate SSN Change History Records Created : " + NumberToString
            (import.AltSsnHistRecsCreated.Count, 15);

          break;
        default:
          break;
      }

      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while writting control report(request update total).";
          
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while writting control report(request update total).";
          
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      ++local.Common.Subscript;
    }
    while(local.Common.Subscript <= 7);

    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while closing control report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseOeEabReadOutFcrAlertRecord()
  {
    var useImport = new OeEabReadOutFcrAlertRecord.Import();
    var useExport = new OeEabReadOutFcrAlertRecord.Export();

    MoveExternal(local.Eab, useImport.ExternalFileStatus);
    MoveExternal(local.Eab, useExport.ExternalFileStatus);

    Call(OeEabReadOutFcrAlertRecord.Execute, useImport, useExport);

    local.Eab.Assign(useExport.ExternalFileStatus);
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
    /// A value of TotalSsnChgInputRecs.
    /// </summary>
    [JsonPropertyName("totalSsnChgInputRecs")]
    public Common TotalSsnChgInputRecs
    {
      get => totalSsnChgInputRecs ??= new();
      set => totalSsnChgInputRecs = value;
    }

    /// <summary>
    /// A value of SkipRecordCount.
    /// </summary>
    [JsonPropertyName("skipRecordCount")]
    public Common SkipRecordCount
    {
      get => skipRecordCount ??= new();
      set => skipRecordCount = value;
    }

    /// <summary>
    /// A value of RecordsPersonNotFound.
    /// </summary>
    [JsonPropertyName("recordsPersonNotFound")]
    public Common RecordsPersonNotFound
    {
      get => recordsPersonNotFound ??= new();
      set => recordsPersonNotFound = value;
    }

    /// <summary>
    /// A value of TotalHistRecordsCreate.
    /// </summary>
    [JsonPropertyName("totalHistRecordsCreate")]
    public Common TotalHistRecordsCreate
    {
      get => totalHistRecordsCreate ??= new();
      set => totalHistRecordsCreate = value;
    }

    /// <summary>
    /// A value of PriSsnHistRecsCreated.
    /// </summary>
    [JsonPropertyName("priSsnHistRecsCreated")]
    public Common PriSsnHistRecsCreated
    {
      get => priSsnHistRecsCreated ??= new();
      set => priSsnHistRecsCreated = value;
    }

    /// <summary>
    /// A value of AltSsnHistRecsCreated.
    /// </summary>
    [JsonPropertyName("altSsnHistRecsCreated")]
    public Common AltSsnHistRecsCreated
    {
      get => altSsnHistRecsCreated ??= new();
      set => altSsnHistRecsCreated = value;
    }

    private Common recordsRead;
    private Common totalSsnChgInputRecs;
    private Common skipRecordCount;
    private Common recordsPersonNotFound;
    private Common totalHistRecordsCreate;
    private Common priSsnHistRecsCreated;
    private Common altSsnHistRecsCreated;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Common common;
    private External eab;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
