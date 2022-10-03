// Program: OE_B493_CLOSING, ID: 372870988, model: 746.
// Short name: SWE02470
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B493_CLOSING.
/// </summary>
[Serializable]
public partial class OeB493Closing: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B493_CLOSING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB493Closing(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB493Closing.
  /// </summary>
  public OeB493Closing(IContext context, Import import, Export export):
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
    // CLOSE ADABAS
    // **********************************************************
    local.PolicyHolder.Number = "CLOSE";
    UseEabReadCsePersonBatch();

    // **********************************************************************
    // Add 2 to record total to include header and trailer records.
    // **********************************************************************
    local.RecordsWritten.Count = import.RecordsWritten.Count + 2;
    local.EabFileHandling.Action = "CLOSE";
    UseEabWriteHinsCoverageChanges();

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
          local.EabReportSend.RptDetail =
            "RECORDS WRITTEN                    " + "   " + NumberToString
            (import.RecordsWritten.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "RECORDS SKIPPED NO POLICY NUM      " + "   " + NumberToString
            (import.RecordsSkippedNoPolicy.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "RECORDS SKIPPED NO COVERAGE CODES  " + "   " + NumberToString
            (import.RecordsSkippedNoCodes.Count, 15);

          break;
        case 4:
          local.EabFileHandling.Action = "CLOSE";

          break;
        case 5:
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
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 4);

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
    UseCabErrorReport();

    // **********************************************************
    // CLOSE OUTPUT BUSINESS REPORT 01
    // **********************************************************
    local.Subscript.Count = 1;

    do
    {
      switch(local.Subscript.Count)
      {
        case 1:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabFileHandling.Action = "CLOSE";

          break;
        case 3:
          break;
        case 4:
          break;
        case 5:
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

      UseCabBusinessReport01();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 2);
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.PolicyHolder.Number;
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabWriteHinsCoverageChanges()
  {
    var useImport = new EabWriteHinsCoverageChanges.Import();
    var useExport = new EabWriteHinsCoverageChanges.Export();

    useImport.Record.Count = local.RecordsWritten.Count;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWriteHinsCoverageChanges.Execute, useImport, useExport);

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
    /// A value of RecordsSkippedNoPolicy.
    /// </summary>
    [JsonPropertyName("recordsSkippedNoPolicy")]
    public Common RecordsSkippedNoPolicy
    {
      get => recordsSkippedNoPolicy ??= new();
      set => recordsSkippedNoPolicy = value;
    }

    /// <summary>
    /// A value of RecordsSkippedNoCodes.
    /// </summary>
    [JsonPropertyName("recordsSkippedNoCodes")]
    public Common RecordsSkippedNoCodes
    {
      get => recordsSkippedNoCodes ??= new();
      set => recordsSkippedNoCodes = value;
    }

    /// <summary>
    /// A value of RecordsWritten.
    /// </summary>
    [JsonPropertyName("recordsWritten")]
    public Common RecordsWritten
    {
      get => recordsWritten ??= new();
      set => recordsWritten = value;
    }

    private Common recordsSkippedNoPolicy;
    private Common recordsSkippedNoCodes;
    private Common recordsWritten;
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
    /// A value of RecordsWritten.
    /// </summary>
    [JsonPropertyName("recordsWritten")]
    public Common RecordsWritten
    {
      get => recordsWritten ??= new();
      set => recordsWritten = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of PolicyHolder.
    /// </summary>
    [JsonPropertyName("policyHolder")]
    public CsePersonsWorkSet PolicyHolder
    {
      get => policyHolder ??= new();
      set => policyHolder = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private Common recordsWritten;
    private Common subscript;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea current;
    private CsePersonsWorkSet policyHolder;
    private AbendData abendData;
  }
#endregion
}
