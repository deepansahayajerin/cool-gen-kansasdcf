// Program: OE_B456_CLOSE, ID: 371367334, model: 746.
// Short name: SWE03602
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B456_CLOSE.
/// </summary>
[Serializable]
public partial class OeB456Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B456_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB456Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB456Close.
  /// </summary>
  public OeB456Close(IContext context, Import import, Export export):
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
    // 8/01/2006                    DDupree                 WR258947
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
            "TOTAL MATCHED RECORDS RECEIVED FROM KDMV       :" + NumberToString
            (import.TotalRecordsProcessed.Count, 15);

          break;
        case 3:
          break;
        case 4:
          local.EabReportSend.RptDetail =
            "KDMV DRIVER LICENSE UPDATED                    :" + NumberToString
            (import.KdmvDriverLicenseUpdat.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "KDMV CSE_PERSON_LICENSE UPDATED                :" + NumberToString
            (import.CsePersonLicenseUpdate.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "TOTAL NUMBER OF ERROR RECORDS                  :" + NumberToString
            (import.TotalErrorRecords.Count, 15);

          break;
        case 7:
          break;
        case 8:
          local.EabReportSend.RptDetail =
            "NON-MATCHED RECORDS FROM KDMV                  :" + NumberToString
            (import.NonMatchRecsFromKdmv.Count, 15);

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
    /// A value of KdmvDriverLicenseUpdat.
    /// </summary>
    [JsonPropertyName("kdmvDriverLicenseUpdat")]
    public Common KdmvDriverLicenseUpdat
    {
      get => kdmvDriverLicenseUpdat ??= new();
      set => kdmvDriverLicenseUpdat = value;
    }

    /// <summary>
    /// A value of TotalErrorRecords.
    /// </summary>
    [JsonPropertyName("totalErrorRecords")]
    public Common TotalErrorRecords
    {
      get => totalErrorRecords ??= new();
      set => totalErrorRecords = value;
    }

    /// <summary>
    /// A value of NonMatchRecsFromKdmv.
    /// </summary>
    [JsonPropertyName("nonMatchRecsFromKdmv")]
    public Common NonMatchRecsFromKdmv
    {
      get => nonMatchRecsFromKdmv ??= new();
      set => nonMatchRecsFromKdmv = value;
    }

    /// <summary>
    /// A value of CsePersonLicenseUpdate.
    /// </summary>
    [JsonPropertyName("csePersonLicenseUpdate")]
    public Common CsePersonLicenseUpdate
    {
      get => csePersonLicenseUpdate ??= new();
      set => csePersonLicenseUpdate = value;
    }

    /// <summary>
    /// A value of TotalRecordsProcessed.
    /// </summary>
    [JsonPropertyName("totalRecordsProcessed")]
    public Common TotalRecordsProcessed
    {
      get => totalRecordsProcessed ??= new();
      set => totalRecordsProcessed = value;
    }

    private Common kdmvDriverLicenseUpdat;
    private Common totalErrorRecords;
    private Common nonMatchRecsFromKdmv;
    private Common csePersonLicenseUpdate;
    private Common totalRecordsProcessed;
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
