// Program: OE_B445_CLOSE, ID: 374518484, model: 746.
// Short name: SWE00149
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B445_CLOSE.
/// </summary>
[Serializable]
public partial class OeB445Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B445_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB445Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB445Close.
  /// </summary>
  public OeB445Close(IContext context, Import import, Export export):
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
    // CLOSE ADABAS
    // **********************************************************
    local.CsePersonsWorkSet.Number = "CLOSE";
    UseEabReadCsePersonBatch();

    // **********************************************************
    // CLOSE INPUT FCR SVES RESPONSE FILE
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseEabReadFcrSvesResponseFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "ERROR CLOSING SVES RESPONSE INPUT FILE";
      UseCabErrorReport2();
    }

    // **********************************************************
    // WRITE OUTPUT CONTROL REPORT AND CLOSE
    // **********************************************************
    local.Subscript.Count = 1;

    do
    {
      switch(local.Subscript.Count)
      {
        case 1:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "FCE SVES RECORDS READ              " + "   " + NumberToString
            (import.RecordsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "NON E04/E05/E06 SVES  RECORDS      " + "   " + NumberToString
            (import.NonT2PendingClaimRecs.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "SVES E04/E05/E06  RECORDS          " + "   " + NumberToString
            (import.T2endingClaimRecs.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "PERSON NOT FOUND RECORD COUNT      " + "   " + NumberToString
            (import.RecordsPersonNotFound.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "SVES BAD SSN RECORD COUNT          " + "   " + NumberToString
            (import.TotalBadSsnRecords.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "SVES E04/E05/E06 SKIPPED COUNT     " + "   " + NumberToString
            (import.TotalT2PendSkippedCr.Count, 15);

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "SVES E04/E05/E06 HIST/ALRT GEN RECS" + "   " + NumberToString
            (import.TotalSvesRecsForAlert.Count, 15);

          break;
        case 9:
          local.EabReportSend.RptDetail = "";

          break;
        case 10:
          local.EabReportSend.RptDetail =
            "TOTAL SVES  HISTORY RECORDS        " + "   " + NumberToString
            (import.TotalHistoryCount.Count, 15);

          break;
        case 11:
          local.EabReportSend.RptDetail =
            "TOTAL SVES WORKER ALERTS RECORDS   " + "   " + NumberToString
            (import.TotalAlertCount.Count, 15);

          break;
        case 12:
          local.EabReportSend.RptDetail =
            "DUPLICATE HISTORY/ALERT  RECORDS   " + "   " + NumberToString
            (import.DuplicateAlertCount.Count, 15);

          break;
        case 13:
          local.EabReportSend.RptDetail = "";

          break;
        case 14:
          local.EabFileHandling.Action = "CLOSE";

          break;
        case 15:
          local.EabReportSend.RptDetail = "";

          break;
        case 16:
          local.EabReportSend.RptDetail = "";

          break;
        case 17:
          break;
        case 18:
          break;
        case 19:
          break;
        case 20:
          break;
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 14);

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
    UseCabErrorReport1();
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseEabReadFcrSvesResponseFile()
  {
    var useImport = new EabReadFcrSvesResponseFile.Import();
    var useExport = new EabReadFcrSvesResponseFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadFcrSvesResponseFile.Execute, useImport, useExport);

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
    /// A value of T2endingClaimRecs.
    /// </summary>
    [JsonPropertyName("t2endingClaimRecs")]
    public Common T2endingClaimRecs
    {
      get => t2endingClaimRecs ??= new();
      set => t2endingClaimRecs = value;
    }

    /// <summary>
    /// A value of TotalHistoryCount.
    /// </summary>
    [JsonPropertyName("totalHistoryCount")]
    public Common TotalHistoryCount
    {
      get => totalHistoryCount ??= new();
      set => totalHistoryCount = value;
    }

    /// <summary>
    /// A value of TotalAlertCount.
    /// </summary>
    [JsonPropertyName("totalAlertCount")]
    public Common TotalAlertCount
    {
      get => totalAlertCount ??= new();
      set => totalAlertCount = value;
    }

    /// <summary>
    /// A value of DuplicateAlertCount.
    /// </summary>
    [JsonPropertyName("duplicateAlertCount")]
    public Common DuplicateAlertCount
    {
      get => duplicateAlertCount ??= new();
      set => duplicateAlertCount = value;
    }

    /// <summary>
    /// A value of NonT2PendingClaimRecs.
    /// </summary>
    [JsonPropertyName("nonT2PendingClaimRecs")]
    public Common NonT2PendingClaimRecs
    {
      get => nonT2PendingClaimRecs ??= new();
      set => nonT2PendingClaimRecs = value;
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
    /// A value of TotalT2PendSkippedCr.
    /// </summary>
    [JsonPropertyName("totalT2PendSkippedCr")]
    public Common TotalT2PendSkippedCr
    {
      get => totalT2PendSkippedCr ??= new();
      set => totalT2PendSkippedCr = value;
    }

    /// <summary>
    /// A value of TotalBadSsnRecords.
    /// </summary>
    [JsonPropertyName("totalBadSsnRecords")]
    public Common TotalBadSsnRecords
    {
      get => totalBadSsnRecords ??= new();
      set => totalBadSsnRecords = value;
    }

    /// <summary>
    /// A value of TotalSvesRecsForAlert.
    /// </summary>
    [JsonPropertyName("totalSvesRecsForAlert")]
    public Common TotalSvesRecsForAlert
    {
      get => totalSvesRecsForAlert ??= new();
      set => totalSvesRecsForAlert = value;
    }

    private Common recordsRead;
    private Common t2endingClaimRecs;
    private Common totalHistoryCount;
    private Common totalAlertCount;
    private Common duplicateAlertCount;
    private Common nonT2PendingClaimRecs;
    private Common recordsPersonNotFound;
    private Common totalT2PendSkippedCr;
    private Common totalBadSsnRecords;
    private Common totalSvesRecsForAlert;
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
    /// A value of TotalProcessed.
    /// </summary>
    [JsonPropertyName("totalProcessed")]
    public Common TotalProcessed
    {
      get => totalProcessed ??= new();
      set => totalProcessed = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    private Common totalProcessed;
    private CsePersonsWorkSet csePersonsWorkSet;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common subscript;
  }
#endregion
}
