// Program: OE_B412_CLOSE, ID: 373345093, model: 746.
// Short name: SWE01959
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B412_CLOSE.
/// </summary>
[Serializable]
public partial class OeB412Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B412_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB412Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB412Close.
  /// </summary>
  public OeB412Close(IContext context, Import import, Export export):
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
    UseOeEabReadFcrAckRecords();

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
            "Total Number of FCR Records Read from External File   : " + NumberToString
            (import.RecordsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Total Number of FCR Records Skipped (HDR/TRL/OTHERS)  : " + NumberToString
            (import.RecordsSkipped.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Total Number of Non IV-D Acknowledgments Skipped      : " + NumberToString
            (import.KpcRecords.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "Total Number of IV-D Acknowledgments Processed        : " + NumberToString
            (import.AcksReceived.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail = "";

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "Total Number of Events without Alerts Created         : " + NumberToString
            (import.EventsCreated.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "Maximum Number of Alerts Expected to be Generated     : " + NumberToString
            (import.AlertsCreated.Count, 15);

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "(Alerts are Optimized by the EP while sending to Service Providers)";
            

          break;
        case 9:
          local.EabReportSend.RptDetail = "";

          break;
        case 10:
          local.EabReportSend.RptDetail =
            "Total Number of DOD Events Created                    : " + NumberToString
            (import.DodEventsCreated.Count, 15);

          break;
        case 11:
          local.EabReportSend.RptDetail =
            "Total Number of DOD Alerts Created                    : " + NumberToString
            (import.DodAlertsCreated.Count, 15);

          break;
        case 12:
          local.EabReportSend.RptDetail =
            "Total Number of DOD Persons Updated                   : " + NumberToString
            (import.PersonsUpdated.Count, 15);

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
    while(local.Common.Subscript <= 12);

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

  private void UseOeEabReadFcrAckRecords()
  {
    var useImport = new OeEabReadFcrAckRecords.Import();
    var useExport = new OeEabReadFcrAckRecords.Export();

    useImport.External.Assign(local.Eab);
    MoveExternal(local.Eab, useExport.External);

    Call(OeEabReadFcrAckRecords.Execute, useImport, useExport);

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
    /// <summary>
    /// A value of PersonsUpdated.
    /// </summary>
    [JsonPropertyName("personsUpdated")]
    public Common PersonsUpdated
    {
      get => personsUpdated ??= new();
      set => personsUpdated = value;
    }

    /// <summary>
    /// A value of DodEventsCreated.
    /// </summary>
    [JsonPropertyName("dodEventsCreated")]
    public Common DodEventsCreated
    {
      get => dodEventsCreated ??= new();
      set => dodEventsCreated = value;
    }

    /// <summary>
    /// A value of DodAlertsCreated.
    /// </summary>
    [JsonPropertyName("dodAlertsCreated")]
    public Common DodAlertsCreated
    {
      get => dodAlertsCreated ??= new();
      set => dodAlertsCreated = value;
    }

    /// <summary>
    /// A value of KpcRecords.
    /// </summary>
    [JsonPropertyName("kpcRecords")]
    public Common KpcRecords
    {
      get => kpcRecords ??= new();
      set => kpcRecords = value;
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
    /// A value of FcrError.
    /// </summary>
    [JsonPropertyName("fcrError")]
    public Common FcrError
    {
      get => fcrError ??= new();
      set => fcrError = value;
    }

    /// <summary>
    /// A value of ErrorsReceived.
    /// </summary>
    [JsonPropertyName("errorsReceived")]
    public Common ErrorsReceived
    {
      get => errorsReceived ??= new();
      set => errorsReceived = value;
    }

    /// <summary>
    /// A value of AcksReceived.
    /// </summary>
    [JsonPropertyName("acksReceived")]
    public Common AcksReceived
    {
      get => acksReceived ??= new();
      set => acksReceived = value;
    }

    /// <summary>
    /// A value of RecordsSkipped.
    /// </summary>
    [JsonPropertyName("recordsSkipped")]
    public Common RecordsSkipped
    {
      get => recordsSkipped ??= new();
      set => recordsSkipped = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    private Common personsUpdated;
    private Common dodEventsCreated;
    private Common dodAlertsCreated;
    private Common kpcRecords;
    private Common alertsCreated;
    private Common eventsCreated;
    private Common fcrError;
    private Common errorsReceived;
    private Common acksReceived;
    private Common recordsSkipped;
    private Common recordsRead;
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
