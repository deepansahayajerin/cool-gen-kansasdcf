// Program: SI_B750_WRITE_CONTROLS_AND_CLOSE, ID: 374543941, model: 746.
// Short name: SWE03127
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B750_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SiB750WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B750_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB750WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB750WriteControlsAndClose.
  /// </summary>
  public SiB750WriteControlsAndClose(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 08/24/2008	J Huss		CQ# 211		Initial development
    // ----------------------------------------------------------------------------
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
          // Print blank line
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          // Print the number of records found that are eligible for purging.
          local.EabReportSend.RptDetail =
            "TOTAL PURGABLE RECORDS FOUND:    " + NumberToString
            (import.TotalPurgableRecords.Count, 15);

          break;
        case 3:
          // Print the number of records that were actually purged.
          local.EabReportSend.RptDetail =
            "TOTAL NUMBER OF RECORDS PURGED:  " + NumberToString
            (import.TotalRecordsPurged.Count, 15);

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

    // Write error message to error report if supplied
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

    // Close control report
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";

      return;
    }

    // Close error report
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
    /// A value of ErrorMessage.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public EabReportSend ErrorMessage
    {
      get => errorMessage ??= new();
      set => errorMessage = value;
    }

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

    private EabReportSend errorMessage;
    private Common totalPurgableRecords;
    private Common totalRecordsPurged;
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
    /// A value of ControlReportDetailMax.
    /// </summary>
    [JsonPropertyName("controlReportDetailMax")]
    public Common ControlReportDetailMax
    {
      get => controlReportDetailMax ??= new();
      set => controlReportDetailMax = value;
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
    private EabReportSend eabReportSend;
    private Common controlReportDetailMax;
    private Common subscript;
  }
#endregion
}
