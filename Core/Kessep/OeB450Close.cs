// Program: OE_B450_CLOSE, ID: 371288742, model: 746.
// Short name: SWE01971
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B450_CLOSE.
/// </summary>
[Serializable]
public partial class OeB450Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B450_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB450Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB450Close.
  /// </summary>
  public OeB450Close(IContext context, Import import, Export export):
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

    do
    {
      switch(local.Sub.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "FPLS Requests Created       :" + NumberToString
            (import.RequestsCreated.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "FPLS Requests Updated       :" + NumberToString
            (import.RequestsUpdated.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail = "FPLS Responses Created      :" + NumberToString
            (import.ResponsesCreated.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail = "FPLS Responses Skipped      :" + NumberToString
            (import.ResponsesSkipped.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail = "FPLS Alerts Created         :" + NumberToString
            (import.AlertsCreated.Count, 15);

          break;
        case 6:
          break;
        case 7:
          break;
        case 8:
          break;
        case 9:
          break;
        default:
          break;
      }

      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while writting control report";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.Sub.Count;
    }
    while(local.Sub.Count <= 5);

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **********************************************************************************************
    // 12/01/2005                    DDupree                 WR258947
    // Now we are going to close the reports for 'A03'.
    // **********************************************************************************************
    UseCabBusinessReport03();
    UseCabBusinessReport04();
    UseCabBusinessReport05();
  }

  private void UseCabBusinessReport03()
  {
    var useImport = new CabBusinessReport03.Import();
    var useExport = new CabBusinessReport03.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport03.Execute, useImport, useExport);
  }

  private void UseCabBusinessReport04()
  {
    var useImport = new CabBusinessReport04.Import();
    var useExport = new CabBusinessReport04.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport04.Execute, useImport, useExport);
  }

  private void UseCabBusinessReport05()
  {
    var useImport = new CabBusinessReport05.Import();
    var useExport = new CabBusinessReport05.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport05.Execute, useImport, useExport);
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
    /// A value of AlertsCreated.
    /// </summary>
    [JsonPropertyName("alertsCreated")]
    public Common AlertsCreated
    {
      get => alertsCreated ??= new();
      set => alertsCreated = value;
    }

    /// <summary>
    /// A value of ResponsesSkipped.
    /// </summary>
    [JsonPropertyName("responsesSkipped")]
    public Common ResponsesSkipped
    {
      get => responsesSkipped ??= new();
      set => responsesSkipped = value;
    }

    /// <summary>
    /// A value of RequestsCreated.
    /// </summary>
    [JsonPropertyName("requestsCreated")]
    public Common RequestsCreated
    {
      get => requestsCreated ??= new();
      set => requestsCreated = value;
    }

    /// <summary>
    /// A value of RequestsUpdated.
    /// </summary>
    [JsonPropertyName("requestsUpdated")]
    public Common RequestsUpdated
    {
      get => requestsUpdated ??= new();
      set => requestsUpdated = value;
    }

    /// <summary>
    /// A value of ResponsesCreated.
    /// </summary>
    [JsonPropertyName("responsesCreated")]
    public Common ResponsesCreated
    {
      get => responsesCreated ??= new();
      set => responsesCreated = value;
    }

    private Common alertsCreated;
    private Common responsesSkipped;
    private Common requestsCreated;
    private Common requestsUpdated;
    private Common responsesCreated;
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
    /// A value of Sub.
    /// </summary>
    [JsonPropertyName("sub")]
    public Common Sub
    {
      get => sub ??= new();
      set => sub = value;
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

    private Common sub;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
