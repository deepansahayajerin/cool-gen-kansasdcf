// Program: OE_B492_CLOSE, ID: 371175777, model: 746.
// Short name: SWE02640
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B492_CLOSE.
/// </summary>
[Serializable]
public partial class OeB492Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B492_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB492Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB492Close.
  /// </summary>
  public OeB492Close(IContext context, Import import, Export export):
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
    // WRITE OUTPUT CONTROL REPORT AND CLOSE
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";
    local.Subscript.Count = 1;

    do
    {
      switch(local.Subscript.Count)
      {
        case 1:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "";

          break;
        case 2:
          local.NeededToWrite.RptDetail = "";

          break;
        case 3:
          local.NeededToWrite.RptDetail =
            "RECORDS READ                       " + "   " + NumberToString
            (import.RecordsRead.Count, 15);

          break;
        case 4:
          local.NeededToWrite.RptDetail = "";

          break;
        case 5:
          local.NeededToWrite.RptDetail =
            "POLICIES TERMINATED                " + "   " + NumberToString
            (import.PoliciesTerminated.Count, 15);

          break;
        case 6:
          local.NeededToWrite.RptDetail =
            "CLAIMS PAID                        " + "   " + NumberToString
            (import.ClaimsPaid.Count, 15);

          break;
        case 7:
          local.NeededToWrite.RptDetail =
            "CLAIMS ADJUSTED                    " + "   " + NumberToString
            (import.ClaimsAdjusted.Count, 15);

          break;
        case 8:
          local.NeededToWrite.RptDetail =
            "RECORDS SKIPPED - NOT FOUND        " + "   " + NumberToString
            (import.RecordsSkippedNotFound.Count, 15);

          break;
        case 9:
          local.NeededToWrite.RptDetail =
            "RECORDS SKIPPED - INVALID          " + "   " + NumberToString
            (import.RecordsSkippedInvalid.Count, 15);

          break;
        case 10:
          local.NeededToWrite.RptDetail = "";

          break;
        case 11:
          local.Total.Count = import.PoliciesTerminated.Count + import
            .ClaimsPaid.Count + import.ClaimsAdjusted.Count + import
            .RecordsSkippedInvalid.Count + import.RecordsSkippedNotFound.Count;
          local.NeededToWrite.RptDetail =
            "TOTAL                              " + "   " + NumberToString
            (local.Total.Count, 15);

          break;
        case 12:
          local.NeededToWrite.RptDetail = "";

          break;
        case 13:
          local.NeededToWrite.RptDetail =
            "ALERTS SENT                        " + "   " + NumberToString
            (import.AlertsSent.Count, 15);

          break;
        case 14:
          local.NeededToWrite.RptDetail =
            "ALERTS FAILED                      " + "   " + NumberToString
            (import.AlertsSent.Count, 15);

          break;
        case 15:
          local.EabFileHandling.Action = "CLOSE";

          break;
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 15);

    UseEabReadHinsAlerts();
    UseCabBusinessReport01();
    UseCabErrorReport();
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadHinsAlerts()
  {
    var useImport = new EabReadHinsAlerts.Import();
    var useExport = new EabReadHinsAlerts.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadHinsAlerts.Execute, useImport, useExport);

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
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of RecordsSkippedInvalid.
    /// </summary>
    [JsonPropertyName("recordsSkippedInvalid")]
    public Common RecordsSkippedInvalid
    {
      get => recordsSkippedInvalid ??= new();
      set => recordsSkippedInvalid = value;
    }

    /// <summary>
    /// A value of RecordsSkippedNotFound.
    /// </summary>
    [JsonPropertyName("recordsSkippedNotFound")]
    public Common RecordsSkippedNotFound
    {
      get => recordsSkippedNotFound ??= new();
      set => recordsSkippedNotFound = value;
    }

    /// <summary>
    /// A value of PoliciesTerminated.
    /// </summary>
    [JsonPropertyName("policiesTerminated")]
    public Common PoliciesTerminated
    {
      get => policiesTerminated ??= new();
      set => policiesTerminated = value;
    }

    /// <summary>
    /// A value of ClaimsPaid.
    /// </summary>
    [JsonPropertyName("claimsPaid")]
    public Common ClaimsPaid
    {
      get => claimsPaid ??= new();
      set => claimsPaid = value;
    }

    /// <summary>
    /// A value of ClaimsAdjusted.
    /// </summary>
    [JsonPropertyName("claimsAdjusted")]
    public Common ClaimsAdjusted
    {
      get => claimsAdjusted ??= new();
      set => claimsAdjusted = value;
    }

    /// <summary>
    /// A value of AlertsSent.
    /// </summary>
    [JsonPropertyName("alertsSent")]
    public Common AlertsSent
    {
      get => alertsSent ??= new();
      set => alertsSent = value;
    }

    /// <summary>
    /// A value of AlertsFailed.
    /// </summary>
    [JsonPropertyName("alertsFailed")]
    public Common AlertsFailed
    {
      get => alertsFailed ??= new();
      set => alertsFailed = value;
    }

    private Common recordsRead;
    private Common recordsSkippedInvalid;
    private Common recordsSkippedNotFound;
    private Common policiesTerminated;
    private Common claimsPaid;
    private Common claimsAdjusted;
    private Common alertsSent;
    private Common alertsFailed;
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
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public Common Total
    {
      get => total ??= new();
      set => total = value;
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    private Common total;
    private EabFileHandling eabFileHandling;
    private Common subscript;
    private EabReportSend neededToWrite;
  }
#endregion
}
