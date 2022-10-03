// Program: SI_B269_CLOSE, ID: 373411374, model: 746.
// Short name: SWE02626
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B269_CLOSE.
/// </summary>
[Serializable]
public partial class SiB269Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B269_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB269Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB269Close.
  /// </summary>
  public SiB269Close(IContext context, Import import, Export export):
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
    // CLOSE INPUT DHR FILE
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseEabReadFederalEmployerFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "ERROR CLOSING EMPLOYER LOAD FILE";
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
          local.EabReportSend.RptDetail = "";

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "EMPLOYERS UPDATED                  " + "   " + NumberToString
            (import.EmployersUpdated.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "SKIPPED - INVALID EMPLOYER ADDRESS " + "   " + NumberToString
            (import.RecordsSkippedInvalid.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "RECORDS ALREADY PROCESSED          " + "   " + NumberToString
            (import.RecordsAlreadyProcessed.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "                                      " + "---------------";

          break;
        case 8:
          local.TotalProcessed.Count = import.EmployersCreated.Count + import
            .EmployersUpdated.Count + import.RecordsSkippedInvalid.Count + import
            .RecordsAlreadyProcessed.Count;
          local.EabReportSend.RptDetail =
            "                    TOTAL:         " + "   " + NumberToString
            (local.TotalProcessed.Count, 15);

          break;
        case 9:
          local.EabReportSend.RptDetail = "";

          break;
        case 10:
          local.EabReportSend.RptDetail =
            "EVENTS CREATED                     " + "   " + NumberToString
            (import.EventsCreated.Count, 15);

          break;
        case 11:
          local.EabReportSend.RptDetail =
            "ALERTS CREATED                     " + "   " + NumberToString
            (import.AlertsCreated.Count, 15);

          break;
        case 12:
          local.EabFileHandling.Action = "CLOSE";

          break;
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 12);

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
    UseCabErrorReport1();

    // **********************************************************
    // CLOSE OUTPUT BUSINESS REPORTS
    // **********************************************************
    UseCabBusinessReport01();
    UseCabBusinessReport04();
    UseCabBusinessReport05();
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport04()
  {
    var useImport = new CabBusinessReport04.Import();
    var useExport = new CabBusinessReport04.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport04.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport05()
  {
    var useImport = new CabBusinessReport05.Import();
    var useExport = new CabBusinessReport05.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport05.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseEabReadFederalEmployerFile()
  {
    var useImport = new EabReadFederalEmployerFile.Import();
    var useExport = new EabReadFederalEmployerFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadFederalEmployerFile.Execute, useImport, useExport);

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
    /// A value of EmployersCreated.
    /// </summary>
    [JsonPropertyName("employersCreated")]
    public Common EmployersCreated
    {
      get => employersCreated ??= new();
      set => employersCreated = value;
    }

    /// <summary>
    /// A value of EmployersUpdated.
    /// </summary>
    [JsonPropertyName("employersUpdated")]
    public Common EmployersUpdated
    {
      get => employersUpdated ??= new();
      set => employersUpdated = value;
    }

    /// <summary>
    /// A value of RecordsSkippedInvalid.
    /// </summary>
    [JsonPropertyName("recordsSkippedInvalid")]
    public Common RecordsSkippedInvalid
    {
      get => recordsSkippedInvalid ??= new();
      set => recordsSkippedInvalid = value;
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
    /// A value of EventsCreated.
    /// </summary>
    [JsonPropertyName("eventsCreated")]
    public Common EventsCreated
    {
      get => eventsCreated ??= new();
      set => eventsCreated = value;
    }

    /// <summary>
    /// A value of AlertsCreated.
    /// </summary>
    [JsonPropertyName("alertsCreated")]
    public Common AlertsCreated
    {
      get => alertsCreated ??= new();
      set => alertsCreated = value;
    }

    private Common recordsRead;
    private Common employersCreated;
    private Common employersUpdated;
    private Common recordsSkippedInvalid;
    private Common recordsAlreadyProcessed;
    private Common eventsCreated;
    private Common alertsCreated;
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

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common subscript;
    private Common totalProcessed;
  }
#endregion
}
