// Program: SI_B462_CLOSE, ID: 371161526, model: 746.
// Short name: SWE03620
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B462_CLOSE.
/// </summary>
[Serializable]
public partial class SiB462Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B462_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB462Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB462Close.
  /// </summary>
  public SiB462Close(IContext context, Import import, Export export):
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
    // 05/30/2009                  DDupree                 CQ 7189
    // Initial programming.
    // **********************************************************************************************
    local.EabFileHandling.Action = "WRITE";

    do
    {
      switch(local.Sub.Count)
      {
        case 2:
          local.EabReportSend.RptDetail =
            "Total number of records processed          :" + NumberToString
            (import.TotalNumProcessed.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Total number of records deleted            :" + NumberToString
            (import.NumRecordsDeleted.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "Total number of error records              :" + NumberToString
            (import.NumberOfErrorRecords.Count, 15);

          break;
        default:
          local.EabReportSend.RptDetail = "";

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
    /// A value of TotalNumProcessed.
    /// </summary>
    [JsonPropertyName("totalNumProcessed")]
    public Common TotalNumProcessed
    {
      get => totalNumProcessed ??= new();
      set => totalNumProcessed = value;
    }

    /// <summary>
    /// A value of NumRecordsDeleted.
    /// </summary>
    [JsonPropertyName("numRecordsDeleted")]
    public Common NumRecordsDeleted
    {
      get => numRecordsDeleted ??= new();
      set => numRecordsDeleted = value;
    }

    /// <summary>
    /// A value of NumberOfErrorRecords.
    /// </summary>
    [JsonPropertyName("numberOfErrorRecords")]
    public Common NumberOfErrorRecords
    {
      get => numberOfErrorRecords ??= new();
      set => numberOfErrorRecords = value;
    }

    private Common totalNumProcessed;
    private Common numRecordsDeleted;
    private Common numberOfErrorRecords;
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
