// Program: SI_B740_CLOSE, ID: 373439445, model: 746.
// Short name: SWE01567
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B740_CLOSE.
/// </summary>
[Serializable]
public partial class SiB740Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B740_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB740Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB740Close.
  /// </summary>
  public SiB740Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************************
    // *** CLOSE the CSENET Transaction Error file
    // **********************************************************************
    export.EabFileHandling.Action = "CLOSE";
    UseEabCsenetErrorsFileReader();

    switch(TrimEnd(export.EabFileHandling.Status))
    {
      case "EOF":
        break;
      case "OK":
        break;
      default:
        // **********************************************************************
        // *** WRITE to the Error Report
        // **********************************************************************
        export.EabFileHandling.Action = "WRITE";
        export.NeededToWrite.RptDetail = "Error Closing the CSENET Error File";
        UseCabErrorReport2();

        break;
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
          local.EabReportSend.RptDetail = "ERROR RECORDS READ:  " + "   " + NumberToString
            (import.CountErrorRecordsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "ENVELOPES READ:      " + "   " + NumberToString
            (import.CountEnvelopesRead.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail = "ENVELOPES UPDATED:   " + "   " + NumberToString
            (import.CountEnvelopesUpdated.Count, 15);

          break;
        case 4:
          local.NotProcessed.Count = import.CountErrorRecordsRead.Count - import
            .CountEnvelopesUpdated.Count;
          local.EabReportSend.RptDetail = "Total Not Processed: " + "   " + NumberToString
            (local.NotProcessed.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail = "";

          break;
        case 6:
          local.EabFileHandling.Action = "CLOSE";

          break;
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 6);

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
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

    useImport.EabFileHandling.Action = export.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = export.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCsenetErrorsFileReader()
  {
    var useImport = new EabCsenetErrorsFileReader.Import();
    var useExport = new EabCsenetErrorsFileReader.Export();

    useImport.EabFileHandling.Action = export.EabFileHandling.Action;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCsenetErrorsFileReader.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    /// A value of CountErrorRecordsRead.
    /// </summary>
    [JsonPropertyName("countErrorRecordsRead")]
    public Common CountErrorRecordsRead
    {
      get => countErrorRecordsRead ??= new();
      set => countErrorRecordsRead = value;
    }

    /// <summary>
    /// A value of CountEnvelopesRead.
    /// </summary>
    [JsonPropertyName("countEnvelopesRead")]
    public Common CountEnvelopesRead
    {
      get => countEnvelopesRead ??= new();
      set => countEnvelopesRead = value;
    }

    /// <summary>
    /// A value of CountEnvelopesUpdated.
    /// </summary>
    [JsonPropertyName("countEnvelopesUpdated")]
    public Common CountEnvelopesUpdated
    {
      get => countEnvelopesUpdated ??= new();
      set => countEnvelopesUpdated = value;
    }

    private Common countErrorRecordsRead;
    private Common countEnvelopesRead;
    private Common countEnvelopesUpdated;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    /// <summary>
    /// A value of NotProcessed.
    /// </summary>
    [JsonPropertyName("notProcessed")]
    public Common NotProcessed
    {
      get => notProcessed ??= new();
      set => notProcessed = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common subscript;
    private Common notProcessed;
  }
#endregion
}
