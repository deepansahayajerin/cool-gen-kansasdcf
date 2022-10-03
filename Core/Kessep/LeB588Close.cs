// Program: LE_B588_CLOSE, ID: 1902488570, model: 746.
// Short name: SWE03744
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B588_CLOSE.
/// </summary>
[Serializable]
public partial class LeB588Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B588_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB588Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB588Close.
  /// </summary>
  public LeB588Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************
    // CLOSE INPUT DHR FILE
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseLeB588ReadFile();

    if (!Equal(local.External.TextReturnCode, "00"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "ERROR CLOSING PORTAL EIWO RESULTS INPUT FILE";
      UseCabErrorReport2();
    }

    // **********************************************************
    // WRITE OUTPUT CONTROL REPORT AND CLOSE
    // **********************************************************
    local.Subscript.Count = 1;

    do
    {
      switch(local.Subscript.Count)
      {
        case 1:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "RECORDS READ" + "                          " +
            NumberToString(import.RecordsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "";

          break;
        case 3:
          local.EabReportSend.RptDetail = "FHS RECORDS" + "                            " +
            NumberToString(import.FhiRecords.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail = "BHS RECORDS" + "                            " +
            NumberToString(import.BhiRecords.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail = "FTS RECORDS" + "                            " +
            NumberToString(import.FtiRecords.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail = "BTS RECORDS" + "                            " +
            NumberToString(import.BtiRecors.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail = "DTL RECORDS" + "                            " +
            NumberToString(import.DtlRecords.Count, 15);

          break;
        case 8:
          local.EabFileHandling.Action = "CLOSE";

          break;
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 8);

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseLeB588ReadFile()
  {
    var useImport = new LeB588ReadFile.Import();
    var useExport = new LeB588ReadFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.External.Assign(local.External);

    Call(LeB588ReadFile.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
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
    /// A value of NoErrRecords.
    /// </summary>
    [JsonPropertyName("noErrRecords")]
    public Common NoErrRecords
    {
      get => noErrRecords ??= new();
      set => noErrRecords = value;
    }

    /// <summary>
    /// A value of DtlRecords.
    /// </summary>
    [JsonPropertyName("dtlRecords")]
    public Common DtlRecords
    {
      get => dtlRecords ??= new();
      set => dtlRecords = value;
    }

    /// <summary>
    /// A value of BtiRecors.
    /// </summary>
    [JsonPropertyName("btiRecors")]
    public Common BtiRecors
    {
      get => btiRecors ??= new();
      set => btiRecors = value;
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

    /// <summary>
    /// A value of FhiRecords.
    /// </summary>
    [JsonPropertyName("fhiRecords")]
    public Common FhiRecords
    {
      get => fhiRecords ??= new();
      set => fhiRecords = value;
    }

    /// <summary>
    /// A value of BhiRecords.
    /// </summary>
    [JsonPropertyName("bhiRecords")]
    public Common BhiRecords
    {
      get => bhiRecords ??= new();
      set => bhiRecords = value;
    }

    /// <summary>
    /// A value of FtiRecords.
    /// </summary>
    [JsonPropertyName("ftiRecords")]
    public Common FtiRecords
    {
      get => ftiRecords ??= new();
      set => ftiRecords = value;
    }

    private Common noErrRecords;
    private Common dtlRecords;
    private Common btiRecors;
    private Common recordsRead;
    private Common fhiRecords;
    private Common bhiRecords;
    private Common ftiRecords;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    /// <summary>
    /// A value of TotalProcessed.
    /// </summary>
    [JsonPropertyName("totalProcessed")]
    public Common TotalProcessed
    {
      get => totalProcessed ??= new();
      set => totalProcessed = value;
    }

    private External external;
    private CsePersonsWorkSet csePersonsWorkSet;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common subscript;
    private Common totalProcessed;
  }
#endregion
}
