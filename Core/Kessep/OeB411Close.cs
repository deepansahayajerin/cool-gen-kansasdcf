// Program: OE_B411_CLOSE, ID: 370940157, model: 746.
// Short name: SWE02716
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B411_CLOSE.
/// </summary>
[Serializable]
public partial class OeB411Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B411_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB411Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB411Close.
  /// </summary>
  public OeB411Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.TotalCount.Count = import.TotalCount.Count;

    // *****************************************************************
    // Write the total count of case records extracted.
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "FCR Number Of CASE records written:       " + NumberToString
      (import.CaseCount.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(number cases).";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // Write the total count of person records extracted.
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "FCR Number Of PERSON records written:     " + NumberToString
      (import.PersonsCount.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(number of Persons records).";
        
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // Write the total count of locate records extracted.
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "FCR Number Of LOCATE records written:     " + NumberToString
      (import.LocateCount.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(number of Locate records).";
        
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // Write the total number of records written.
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "FCR Total Number of records written:      " + NumberToString
      (local.TotalCount.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report (total  records).";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // Close  FCR_REQUEST file
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";

    // *****************************************************************
    // Include header and trail record in the total count
    // *****************************************************************
    local.TotalCount.Count += 2;
    UseOeEabWriteFcrRequests();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while closing external fileFCR_REQUEST";
      UseCabErrorReport1();
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // *****************************************************************
    //  Close Control and Report Error Report  files.
    // *****************************************************************
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

    UseSiCloseAdabas();
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
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

  private void UseOeEabWriteFcrRequests()
  {
    var useImport = new OeEabWriteFcrRequests.Import();
    var useExport = new OeEabWriteFcrRequests.Export();

    useImport.ProcessDate.Date = import.ProcessingDate.Date;
    useImport.RecordCounts.Count = local.TotalCount.Count;
    MoveEabFileHandling(local.EabFileHandling, useImport.EabFileHandling);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(OeEabWriteFcrRequests.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
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
    /// A value of ProcessingDate.
    /// </summary>
    [JsonPropertyName("processingDate")]
    public DateWorkArea ProcessingDate
    {
      get => processingDate ??= new();
      set => processingDate = value;
    }

    /// <summary>
    /// A value of Import1099Count.
    /// </summary>
    [JsonPropertyName("import1099Count")]
    public Common Import1099Count
    {
      get => import1099Count ??= new();
      set => import1099Count = value;
    }

    /// <summary>
    /// A value of TotalCount.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public Common TotalCount
    {
      get => totalCount ??= new();
      set => totalCount = value;
    }

    /// <summary>
    /// A value of PersonsCount.
    /// </summary>
    [JsonPropertyName("personsCount")]
    public Common PersonsCount
    {
      get => personsCount ??= new();
      set => personsCount = value;
    }

    /// <summary>
    /// A value of LocateCount.
    /// </summary>
    [JsonPropertyName("locateCount")]
    public Common LocateCount
    {
      get => locateCount ??= new();
      set => locateCount = value;
    }

    /// <summary>
    /// A value of CaseCount.
    /// </summary>
    [JsonPropertyName("caseCount")]
    public Common CaseCount
    {
      get => caseCount ??= new();
      set => caseCount = value;
    }

    private DateWorkArea processingDate;
    private Common import1099Count;
    private Common totalCount;
    private Common personsCount;
    private Common locateCount;
    private Common caseCount;
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
    /// A value of TotalCount.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public Common TotalCount
    {
      get => totalCount ??= new();
      set => totalCount = value;
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

    private Common totalCount;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
