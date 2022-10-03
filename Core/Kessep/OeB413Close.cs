// Program: OE_B413_CLOSE, ID: 373414329, model: 746.
// Short name: SWE01961
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B413_CLOSE.
/// </summary>
[Serializable]
public partial class OeB413Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B413_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB413Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB413Close.
  /// </summary>
  public OeB413Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************
    // * Call external to CLOSE the driver file.       *
    // ************************************************
    local.Eab.FileInstruction = "CLOSE";
    UseOeEabReadFcrMatchResponses();

    if (!IsEmpty(local.Eab.TextReturnCode))
    {
      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.Loop.Count = 1;

    do
    {
      switch(local.Loop.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Total Number of FCR Records Read from External File   : " + NumberToString
            (import.RecordsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Total Number of FCR Records Skipped (HDR/TRL/OTHERS)  : " + NumberToString
            (import.RecordsSkipped.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "Total Number of FCR Proactive Match Responses Read    : " + NumberToString
            (import.PromatchRead.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "Total Number of FCR DOD Proactive Match Responses Read: " + NumberToString
            (import.PromatchDodRead.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "Total Number of Proactive Matches created             : " + NumberToString
            (import.PromatchCreated.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "Total Number of Manual Requests (non CSENet) needed   : " + NumberToString
            (import.PromatchNonCsenet.Count, 15);

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "Total Number of CSENet CSI Requests Generated         : " + NumberToString
            (import.CsiRequestsCreated.Count, 15);

          break;
        case 9:
          local.EabReportSend.RptDetail = "";

          break;
        case 10:
          local.EabReportSend.RptDetail =
            "Total Number of Proactive Matches already received    : " + NumberToString
            (import.PromatchAlreadyReceived.Count, 15);

          break;
        case 11:
          local.EabReportSend.RptDetail =
            "Total Number of Interstate Cases that already exist   : " + NumberToString
            (import.InterstateCaseExists.Count, 15);

          break;
        case 12:
          local.EabReportSend.RptDetail =
            "Total Number of CSE Persons not found                 : " + NumberToString
            (import.ProactiveMatchErrors.Count, 15);

          break;
        case 13:
          local.EabReportSend.RptDetail = "";

          break;
        case 14:
          local.EabReportSend.RptDetail =
            "Total Number of DOD Events Created                    : " + NumberToString
            (import.DodEventsCreated.Count, 15);

          break;
        case 15:
          local.EabReportSend.RptDetail =
            "Total Number of DOD Alerts Created                    : " + NumberToString
            (import.DodAlertsCreated.Count, 15);

          break;
        case 16:
          local.EabReportSend.RptDetail =
            "Total Number of DOD Persons Updated                   : " + NumberToString
            (import.DodPersonsUpdated.Count, 15);

          break;
        default:
          break;
      }

      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while writing control report.";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.Loop.Count;
    }
    while(local.Loop.Count <= 16);

    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
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

  private void UseOeEabReadFcrMatchResponses()
  {
    var useImport = new OeEabReadFcrMatchResponses.Import();
    var useExport = new OeEabReadFcrMatchResponses.Export();

    useImport.External.Assign(local.Eab);
    useExport.External.Assign(local.Eab);

    Call(OeEabReadFcrMatchResponses.Execute, useImport, useExport);

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
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
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
    /// A value of PromatchRead.
    /// </summary>
    [JsonPropertyName("promatchRead")]
    public Common PromatchRead
    {
      get => promatchRead ??= new();
      set => promatchRead = value;
    }

    /// <summary>
    /// A value of PromatchDodRead.
    /// </summary>
    [JsonPropertyName("promatchDodRead")]
    public Common PromatchDodRead
    {
      get => promatchDodRead ??= new();
      set => promatchDodRead = value;
    }

    /// <summary>
    /// A value of PromatchCreated.
    /// </summary>
    [JsonPropertyName("promatchCreated")]
    public Common PromatchCreated
    {
      get => promatchCreated ??= new();
      set => promatchCreated = value;
    }

    /// <summary>
    /// A value of PromatchNonCsenet.
    /// </summary>
    [JsonPropertyName("promatchNonCsenet")]
    public Common PromatchNonCsenet
    {
      get => promatchNonCsenet ??= new();
      set => promatchNonCsenet = value;
    }

    /// <summary>
    /// A value of CsiRequestsCreated.
    /// </summary>
    [JsonPropertyName("csiRequestsCreated")]
    public Common CsiRequestsCreated
    {
      get => csiRequestsCreated ??= new();
      set => csiRequestsCreated = value;
    }

    /// <summary>
    /// A value of PromatchAlreadyReceived.
    /// </summary>
    [JsonPropertyName("promatchAlreadyReceived")]
    public Common PromatchAlreadyReceived
    {
      get => promatchAlreadyReceived ??= new();
      set => promatchAlreadyReceived = value;
    }

    /// <summary>
    /// A value of InterstateCaseExists.
    /// </summary>
    [JsonPropertyName("interstateCaseExists")]
    public Common InterstateCaseExists
    {
      get => interstateCaseExists ??= new();
      set => interstateCaseExists = value;
    }

    /// <summary>
    /// A value of ProactiveMatchErrors.
    /// </summary>
    [JsonPropertyName("proactiveMatchErrors")]
    public Common ProactiveMatchErrors
    {
      get => proactiveMatchErrors ??= new();
      set => proactiveMatchErrors = value;
    }

    /// <summary>
    /// A value of DodPersonsUpdated.
    /// </summary>
    [JsonPropertyName("dodPersonsUpdated")]
    public Common DodPersonsUpdated
    {
      get => dodPersonsUpdated ??= new();
      set => dodPersonsUpdated = value;
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
    /// A value of DodEventsCreated.
    /// </summary>
    [JsonPropertyName("dodEventsCreated")]
    public Common DodEventsCreated
    {
      get => dodEventsCreated ??= new();
      set => dodEventsCreated = value;
    }

    private Common recordsRead;
    private Common recordsSkipped;
    private Common promatchRead;
    private Common promatchDodRead;
    private Common promatchCreated;
    private Common promatchNonCsenet;
    private Common csiRequestsCreated;
    private Common promatchAlreadyReceived;
    private Common interstateCaseExists;
    private Common proactiveMatchErrors;
    private Common dodPersonsUpdated;
    private Common dodAlertsCreated;
    private Common dodEventsCreated;
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

    /// <summary>
    /// A value of Loop.
    /// </summary>
    [JsonPropertyName("loop")]
    public Common Loop
    {
      get => loop ??= new();
      set => loop = value;
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

    private External eab;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Common loop;
    private ProgramCheckpointRestart programCheckpointRestart;
  }
#endregion
}
