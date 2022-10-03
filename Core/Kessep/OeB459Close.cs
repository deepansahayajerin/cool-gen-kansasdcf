// Program: OE_B459_CLOSE, ID: 371387683, model: 746.
// Short name: SWE03612
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B459_CLOSE.
/// </summary>
[Serializable]
public partial class OeB459Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B459_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB459Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB459Close.
  /// </summary>
  public OeB459Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************************************************
    // 01/12/2008                  DDupree                 WR 280420
    // Initial programming.
    // **********************************************************************************************
    local.EabFileHandling.Action = "WRITE";

    do
    {
      switch(local.Sub.Count)
      {
        case 1:
          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Number of restrict records read       :" + NumberToString
            (import.NumRestrictRecordsRead.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Number of restriction requested       :" + NumberToString
            (import.NumOfRestrictRequest.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "Number of failed restriction request  :" + NumberToString
            (import.NumFailedRestrictReqst.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "Number of reinstatement records read  :" + NumberToString
            (import.NumReinstateRecsRead.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "Total numbers of obligors reinstated  :" + NumberToString
            (import.NumOfReinstateRequest.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "Total numbers of error records        :" + NumberToString
            (import.NumErrorRecords.Count, 15);

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
      local.EabReportSend.RptDetail = "";
    }
    while(local.Sub.Count <= 9);

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
    /// A value of NumRestrictRecordsRead.
    /// </summary>
    [JsonPropertyName("numRestrictRecordsRead")]
    public Common NumRestrictRecordsRead
    {
      get => numRestrictRecordsRead ??= new();
      set => numRestrictRecordsRead = value;
    }

    /// <summary>
    /// A value of NumOfRestrictRequest.
    /// </summary>
    [JsonPropertyName("numOfRestrictRequest")]
    public Common NumOfRestrictRequest
    {
      get => numOfRestrictRequest ??= new();
      set => numOfRestrictRequest = value;
    }

    /// <summary>
    /// A value of NumFailedRestrictReqst.
    /// </summary>
    [JsonPropertyName("numFailedRestrictReqst")]
    public Common NumFailedRestrictReqst
    {
      get => numFailedRestrictReqst ??= new();
      set => numFailedRestrictReqst = value;
    }

    /// <summary>
    /// A value of NumReinstateRecsRead.
    /// </summary>
    [JsonPropertyName("numReinstateRecsRead")]
    public Common NumReinstateRecsRead
    {
      get => numReinstateRecsRead ??= new();
      set => numReinstateRecsRead = value;
    }

    /// <summary>
    /// A value of NumOfReinstateRequest.
    /// </summary>
    [JsonPropertyName("numOfReinstateRequest")]
    public Common NumOfReinstateRequest
    {
      get => numOfReinstateRequest ??= new();
      set => numOfReinstateRequest = value;
    }

    /// <summary>
    /// A value of NumErrorRecords.
    /// </summary>
    [JsonPropertyName("numErrorRecords")]
    public Common NumErrorRecords
    {
      get => numErrorRecords ??= new();
      set => numErrorRecords = value;
    }

    private Common numRestrictRecordsRead;
    private Common numOfRestrictRequest;
    private Common numFailedRestrictReqst;
    private Common numReinstateRecsRead;
    private Common numOfReinstateRequest;
    private Common numErrorRecords;
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
