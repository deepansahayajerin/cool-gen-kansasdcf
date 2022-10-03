// Program: OE_B417_CLOSE, ID: 374565566, model: 746.
// Short name: SWE00116
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B417_CLOSE.
/// </summary>
[Serializable]
public partial class OeB417Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B417_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB417Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB417Close.
  /// </summary>
  public OeB417Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.TotalCount.Count = import.TotalUpdCount.Count;

    // *****************************************************************
    // Write the total count of FCR Master records read.
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "Total FCR Master Record Read       :          " + NumberToString
      (import.TotalFcrMasterCount.Count, 15);
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
    // Write the total count of FCR Master records Added to the system
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "Total Records Added to the System  :          " + NumberToString
      (import.TotalCreCount.Count, 15);
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
    // Write the total count of FCR CASE records Added to the system
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "          Total Case Records Added :          " + NumberToString
      (import.CaseCreCount.Count, 15);
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
    // Write the total count of FCR PERSON records Added to the system
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "        Total Person Records Added :          " + NumberToString
      (import.PersonsCreCount.Count, 15);
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
    // Write the total count of FCR records Updated to the system
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "Total Records Updated to the System:          " + NumberToString
      (import.TotalUpdCount.Count, 15);
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
    // Write the total count of FCR CASE records Updated to the system
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "        Total Case Records Updated :          " + NumberToString
      (import.CaseUpdCount.Count, 15);
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
    // Write the total count of FCR PERSON records Updated to the system
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "      Total Person Records Updated :          " + NumberToString
      (import.PersonsUpdCount.Count, 15);
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
    // Write the total count of FCR records Deleted from the system
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "Total Records Deleted from the System:        " + NumberToString
      (import.TotalDelCount.Count, 15);
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
    // Write the total count of FCR CASE records Deleted from the system
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "        Total Case Records Deleted :          " + NumberToString
      (import.CaseDelCount.Count, 15);
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

    // *******************************************************************
    // Write the total count of FCR PERSON records Deleted from the system
    // *******************************************************************
    local.EabReportSend.RptDetail =
      "      Total Person Records Deleted :          " + NumberToString
      (import.PersonsDelCount.Count, 15);
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
    // Write the total count of FCR records Skipped from the system
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "Total Records Skipped from the System:        " + NumberToString
      (import.TotalSkipCount.Count, 15);
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
    // Write the total count of FCR CASE records Skipped from the system
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "        Total Case Records Skipped :          " + NumberToString
      (import.CaseSkipCount.Count, 15);
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
    // Write the total count of FCR PERSON records Skipped from the system
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "      Total Person Records Skipped :          " + NumberToString
      (import.PersonsSkipCount.Count, 15);
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

    // ***************************************************************************************
    // Close  FCR_MASTER  file
    // ***************************************************************************************
    local.ExternalFileStatus.FileInstruction = "CLOSE";
    UseOeEabReadFcrMasterRecord();

    if (!IsEmpty(local.ExternalFileStatus.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while closing external file FCR_MASTER";
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

    local.EabFileHandling.Action = "CLOSE";
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

  private void UseOeEabReadFcrMasterRecord()
  {
    var useImport = new OeEabReadFcrMasterRecord.Import();
    var useExport = new OeEabReadFcrMasterRecord.Export();

    useImport.ExternalFileStatus.Assign(local.ExternalFileStatus);
    useExport.ExternalFileStatus.Assign(local.ExternalFileStatus);

    Call(OeEabReadFcrMasterRecord.Execute, useImport, useExport);

    local.ExternalFileStatus.Assign(useExport.ExternalFileStatus);
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
    /// A value of TotalFcrMasterCount.
    /// </summary>
    [JsonPropertyName("totalFcrMasterCount")]
    public Common TotalFcrMasterCount
    {
      get => totalFcrMasterCount ??= new();
      set => totalFcrMasterCount = value;
    }

    /// <summary>
    /// A value of TotalCreCount.
    /// </summary>
    [JsonPropertyName("totalCreCount")]
    public Common TotalCreCount
    {
      get => totalCreCount ??= new();
      set => totalCreCount = value;
    }

    /// <summary>
    /// A value of TotalUpdCount.
    /// </summary>
    [JsonPropertyName("totalUpdCount")]
    public Common TotalUpdCount
    {
      get => totalUpdCount ??= new();
      set => totalUpdCount = value;
    }

    /// <summary>
    /// A value of TotalDelCount.
    /// </summary>
    [JsonPropertyName("totalDelCount")]
    public Common TotalDelCount
    {
      get => totalDelCount ??= new();
      set => totalDelCount = value;
    }

    /// <summary>
    /// A value of TotalSkipCount.
    /// </summary>
    [JsonPropertyName("totalSkipCount")]
    public Common TotalSkipCount
    {
      get => totalSkipCount ??= new();
      set => totalSkipCount = value;
    }

    /// <summary>
    /// A value of CaseCreCount.
    /// </summary>
    [JsonPropertyName("caseCreCount")]
    public Common CaseCreCount
    {
      get => caseCreCount ??= new();
      set => caseCreCount = value;
    }

    /// <summary>
    /// A value of CaseUpdCount.
    /// </summary>
    [JsonPropertyName("caseUpdCount")]
    public Common CaseUpdCount
    {
      get => caseUpdCount ??= new();
      set => caseUpdCount = value;
    }

    /// <summary>
    /// A value of CaseDelCount.
    /// </summary>
    [JsonPropertyName("caseDelCount")]
    public Common CaseDelCount
    {
      get => caseDelCount ??= new();
      set => caseDelCount = value;
    }

    /// <summary>
    /// A value of CaseSkipCount.
    /// </summary>
    [JsonPropertyName("caseSkipCount")]
    public Common CaseSkipCount
    {
      get => caseSkipCount ??= new();
      set => caseSkipCount = value;
    }

    /// <summary>
    /// A value of PersonsCreCount.
    /// </summary>
    [JsonPropertyName("personsCreCount")]
    public Common PersonsCreCount
    {
      get => personsCreCount ??= new();
      set => personsCreCount = value;
    }

    /// <summary>
    /// A value of PersonsUpdCount.
    /// </summary>
    [JsonPropertyName("personsUpdCount")]
    public Common PersonsUpdCount
    {
      get => personsUpdCount ??= new();
      set => personsUpdCount = value;
    }

    /// <summary>
    /// A value of PersonsDelCount.
    /// </summary>
    [JsonPropertyName("personsDelCount")]
    public Common PersonsDelCount
    {
      get => personsDelCount ??= new();
      set => personsDelCount = value;
    }

    /// <summary>
    /// A value of PersonsSkipCount.
    /// </summary>
    [JsonPropertyName("personsSkipCount")]
    public Common PersonsSkipCount
    {
      get => personsSkipCount ??= new();
      set => personsSkipCount = value;
    }

    private DateWorkArea processingDate;
    private Common totalFcrMasterCount;
    private Common totalCreCount;
    private Common totalUpdCount;
    private Common totalDelCount;
    private Common totalSkipCount;
    private Common caseCreCount;
    private Common caseUpdCount;
    private Common caseDelCount;
    private Common caseSkipCount;
    private Common personsCreCount;
    private Common personsUpdCount;
    private Common personsDelCount;
    private Common personsSkipCount;
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
    /// A value of ExternalFileStatus.
    /// </summary>
    [JsonPropertyName("externalFileStatus")]
    public External ExternalFileStatus
    {
      get => externalFileStatus ??= new();
      set => externalFileStatus = value;
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

    private External externalFileStatus;
    private Common totalCount;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
