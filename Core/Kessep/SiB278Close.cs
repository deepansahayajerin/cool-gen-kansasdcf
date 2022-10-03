// Program: SI_B278_CLOSE, ID: 373313649, model: 746.
// Short name: SWE01302
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B278_CLOSE.
/// </summary>
[Serializable]
public partial class SiB278Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B278_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB278Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB278Close.
  /// </summary>
  public SiB278Close(IContext context, Import import, Export export):
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
    // CLOSE INPUT DHR FILE
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseEabReadFedQtrlyUnemplIncome();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "ERROR CLOSING FEDERAL QTRLY UNEMPLOYMENT INPUT FILE";
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
            "RECORDS READ                       " + "   " + NumberToString
            (import.RecordsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "ALERTS SENT                        " + "   " + NumberToString
            (import.AlertsSent.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "RECORDS SKIPPED SSN MISMATCH       " + "   " + NumberToString
            (import.RecordsSkippedSsn.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "RECORDS PERSON NOT FOUND           " + "   " + NumberToString
            (import.RecordsPersonNotFound.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "RECORDS ALREADY PROCESSED          " + "   " + NumberToString
            (import.RecordsAlreadyProcessed.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "RECORDS MORE THAN 6 MONTHS OLD     " + "   " + NumberToString
            (import.RecordsOlderThan6Mos.Count, 15);

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "                                      " + "---------------";

          break;
        case 9:
          local.TotalProcessed.Count = import.RecordsPersonNotFound.Count + import
            .RecordsSkippedSsn.Count + import.AlertsSent.Count + import
            .RecordsAlreadyProcessed.Count + import.RecordsOlderThan6Mos.Count;
          local.EabReportSend.RptDetail =
            "                    TOTAL:         " + "   " + NumberToString
            (local.TotalProcessed.Count, 15);

          break;
        case 10:
          local.EabFileHandling.Action = "CLOSE";

          break;
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 10);

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
    UseCabErrorReport1();

    // **********************************************************
    // CLOSE OUTPUT BUSINESS REPORTS
    // **********************************************************
    UseCabBusinessReport01();
    UseCabBusinessReport02();
    UseCabBusinessReport03();
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);
  }

  private void UseCabBusinessReport02()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport02.Execute, useImport, useExport);
  }

  private void UseCabBusinessReport03()
  {
    var useImport = new CabBusinessReport03.Import();
    var useExport = new CabBusinessReport03.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport03.Execute, useImport, useExport);
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

  private void UseEabReadFedQtrlyUnemplIncome()
  {
    var useImport = new EabReadFedQtrlyUnemplIncome.Import();
    var useExport = new EabReadFedQtrlyUnemplIncome.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadFedQtrlyUnemplIncome.Execute, useImport, useExport);

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
    /// A value of AlertsSent.
    /// </summary>
    [JsonPropertyName("alertsSent")]
    public Common AlertsSent
    {
      get => alertsSent ??= new();
      set => alertsSent = value;
    }

    /// <summary>
    /// A value of RecordsSkippedSsn.
    /// </summary>
    [JsonPropertyName("recordsSkippedSsn")]
    public Common RecordsSkippedSsn
    {
      get => recordsSkippedSsn ??= new();
      set => recordsSkippedSsn = value;
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
    /// A value of RecordsAlreadyProcessed.
    /// </summary>
    [JsonPropertyName("recordsAlreadyProcessed")]
    public Common RecordsAlreadyProcessed
    {
      get => recordsAlreadyProcessed ??= new();
      set => recordsAlreadyProcessed = value;
    }

    /// <summary>
    /// A value of RecordsOlderThan6Mos.
    /// </summary>
    [JsonPropertyName("recordsOlderThan6Mos")]
    public Common RecordsOlderThan6Mos
    {
      get => recordsOlderThan6Mos ??= new();
      set => recordsOlderThan6Mos = value;
    }

    private Common recordsRead;
    private Common alertsSent;
    private Common recordsSkippedSsn;
    private Common recordsPersonNotFound;
    private Common recordsAlreadyProcessed;
    private Common recordsOlderThan6Mos;
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

    /// <summary>
    /// A value of TotalProcessed.
    /// </summary>
    [JsonPropertyName("totalProcessed")]
    public Common TotalProcessed
    {
      get => totalProcessed ??= new();
      set => totalProcessed = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common subscript;
    private Common totalProcessed;
  }
#endregion
}
