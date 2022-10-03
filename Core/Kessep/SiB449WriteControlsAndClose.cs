// Program: SI_B449_WRITE_CONTROLS_AND_CLOSE, ID: 1625402243, model: 746.
// Short name: SWE00397
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_B449_WRITE_CONTROLS_AND_CLOSE.
/// </para>
/// <para>
/// This action block called by Pstep OE_B449_FCR_ALERT_PURGE_PROCES to wirte 
/// control totals and close the files.
/// </para>
/// </summary>
[Serializable]
public partial class SiB449WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B449_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB449WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB449WriteControlsAndClose.
  /// </summary>
  public SiB449WriteControlsAndClose(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************************
    // **                           M A I N T E N A N C E   L O G
    // **
    // ***************************************************************************************
    // *   Date     Developer  PR#/WR#     
    // Description
    // 
    // *
    // *-------------------------------------------------------------------------------------*
    // * 12/22/2020 Raj        CQ13044
    // Initial Development
    // 
    // *
    // *
    // 
    // *
    // *
    // 
    // *
    // ***************************************************************************************
    local.EabFileHandling.Action = "WRITE";
    local.ControlReportDetailMax.Count = 99;

    // Write control report details
    local.Subscript.Count = 1;

    for(var limit = local.ControlReportDetailMax.Count; local
      .Subscript.Count <= limit; ++local.Subscript.Count)
    {
      switch(local.Subscript.Count)
      {
        case 1:
          // ***************************************************************************************
          // *** Print blank line
          // 
          // ***
          // ***************************************************************************************
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          // ***************************************************************************************
          // *** Print the number of records found that are eligible for 
          // purging.                ***
          // ***************************************************************************************
          local.EabReportSend.RptDetail =
            "TOTAL PURGABLE RECORDS FOUND:     " + NumberToString
            (import.TotalPurgableRecords.Count, 15);

          break;
        case 3:
          // ***************************************************************************************
          // *** Print the number of records that were actually purged.
          // ***
          // ***************************************************************************************
          local.EabReportSend.RptDetail =
            "TOTAL NUMBER OF RECORDS PURGED:   " + NumberToString
            (import.TotalRecordsPurged.Count, 15);

          break;
        case 4:
          // ***************************************************************************************
          // *** Print the number of Alert records that were actually purged.
          // ***
          // ***************************************************************************************
          local.EabReportSend.RptDetail =
            "TOTAL NUMBER OF ALERTS PURGED:    " + NumberToString
            (import.TotalAlertsPurged.Count, 15);

          break;
        case 5:
          // ***************************************************************************************
          // *** Print the number of Narrative records that were actually 
          // purged.                ***
          // ***************************************************************************************
          local.EabReportSend.RptDetail =
            "TOTAL NUMBER OF NARRTIVES PURGED: " + NumberToString
            (import.TotalNarrPurged.Count, 15);

          break;
        case 6:
          // ***************************************************************************************
          // *** Print the number of records that were skipped due to 
          // dependencies               ***
          // ***************************************************************************************
          local.EabReportSend.RptDetail =
            "TOTAL NUMBER OF RECORDS SKIPPED:  " + NumberToString
            (import.TotalRecordsSkipped.Count, 15);

          break;
        case 7:
          // ***************************************************************************************
          // *** Print **** END OF REPORT ****  Text
          // 
          // ***
          // ***************************************************************************************
          local.EabReportSend.RptDetail =
            "                             **** END OF REPORT ****";

          break;
        default:
          goto AfterCycle;
      }

      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

AfterCycle:

    // ***************************************************************************************
    // *** Write error message to error 
    // report if supplied
    // 
    // ***
    // ***************************************************************************************
    if (!IsEmpty(import.ErrorMessage.RptDetail))
    {
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabFileHandling.Action = "CLOSE";

    // ***************************************************************************************
    // *** Close Control Report File
    // 
    // ***
    // ***************************************************************************************
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";

      return;
    }

    // ***************************************************************************************
    // *** Close Error Report File
    // 
    // ***
    // ***************************************************************************************
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";
    }
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = import.ErrorMessage.RptDetail;
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
    /// A value of TotalPurgableRecords.
    /// </summary>
    [JsonPropertyName("totalPurgableRecords")]
    public Common TotalPurgableRecords
    {
      get => totalPurgableRecords ??= new();
      set => totalPurgableRecords = value;
    }

    /// <summary>
    /// A value of TotalRecordsPurged.
    /// </summary>
    [JsonPropertyName("totalRecordsPurged")]
    public Common TotalRecordsPurged
    {
      get => totalRecordsPurged ??= new();
      set => totalRecordsPurged = value;
    }

    /// <summary>
    /// A value of TotalAlertsPurged.
    /// </summary>
    [JsonPropertyName("totalAlertsPurged")]
    public Common TotalAlertsPurged
    {
      get => totalAlertsPurged ??= new();
      set => totalAlertsPurged = value;
    }

    /// <summary>
    /// A value of TotalNarrPurged.
    /// </summary>
    [JsonPropertyName("totalNarrPurged")]
    public Common TotalNarrPurged
    {
      get => totalNarrPurged ??= new();
      set => totalNarrPurged = value;
    }

    /// <summary>
    /// A value of TotalRecordsSkipped.
    /// </summary>
    [JsonPropertyName("totalRecordsSkipped")]
    public Common TotalRecordsSkipped
    {
      get => totalRecordsSkipped ??= new();
      set => totalRecordsSkipped = value;
    }

    /// <summary>
    /// A value of ErrorMessage.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public EabReportSend ErrorMessage
    {
      get => errorMessage ??= new();
      set => errorMessage = value;
    }

    private Common totalPurgableRecords;
    private Common totalRecordsPurged;
    private Common totalAlertsPurged;
    private Common totalNarrPurged;
    private Common totalRecordsSkipped;
    private EabReportSend errorMessage;
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
    /// A value of ControlReportDetailMax.
    /// </summary>
    [JsonPropertyName("controlReportDetailMax")]
    public Common ControlReportDetailMax
    {
      get => controlReportDetailMax ??= new();
      set => controlReportDetailMax = value;
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

    private EabFileHandling eabFileHandling;
    private Common controlReportDetailMax;
    private EabReportSend eabReportSend;
    private Common subscript;
  }
#endregion
}
