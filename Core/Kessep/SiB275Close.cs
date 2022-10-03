// Program: SI_B275_CLOSE, ID: 372745440, model: 746.
// Short name: SWE02572
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B275_CLOSE.
/// </summary>
[Serializable]
public partial class SiB275Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B275_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB275Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB275Close.
  /// </summary>
  public SiB275Close(IContext context, Import import, Export export):
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
    // CLOSE OUTPUT QUERY FILE
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseEabWriteDhrNewhireQueryFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "ERROR CLOSING DHR OUTPUT FILE";
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
          local.EabReportSend.RptDetail =
            "AP's READ                          " + "   " + NumberToString
            (import.CountOfApRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "AP QUERIES CREATED                 " + "   " + NumberToString
            (import.ApQueriesCreated.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "AR's READ                          " + "   " + NumberToString
            (import.CountOfArRead.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "AR QUERIES CREATED                 " + "   " + NumberToString
            (import.ArQueriesCreated.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "PERSON HAS INVALID SSN             " + "   " + NumberToString
            (import.ErrorInvalidSsn.Count, 15);

          break;
        case 6:
          local.EabFileHandling.Action = "CLOSE";

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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseEabWriteDhrNewhireQueryFile()
  {
    var useImport = new EabWriteDhrNewhireQueryFile.Import();
    var useExport = new EabWriteDhrNewhireQueryFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWriteDhrNewhireQueryFile.Execute, useImport, useExport);

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
    /// A value of CountOfApRead.
    /// </summary>
    [JsonPropertyName("countOfApRead")]
    public Common CountOfApRead
    {
      get => countOfApRead ??= new();
      set => countOfApRead = value;
    }

    /// <summary>
    /// A value of ApQueriesCreated.
    /// </summary>
    [JsonPropertyName("apQueriesCreated")]
    public Common ApQueriesCreated
    {
      get => apQueriesCreated ??= new();
      set => apQueriesCreated = value;
    }

    /// <summary>
    /// A value of CountOfArRead.
    /// </summary>
    [JsonPropertyName("countOfArRead")]
    public Common CountOfArRead
    {
      get => countOfArRead ??= new();
      set => countOfArRead = value;
    }

    /// <summary>
    /// A value of ArQueriesCreated.
    /// </summary>
    [JsonPropertyName("arQueriesCreated")]
    public Common ArQueriesCreated
    {
      get => arQueriesCreated ??= new();
      set => arQueriesCreated = value;
    }

    /// <summary>
    /// A value of ErrorInvalidSsn.
    /// </summary>
    [JsonPropertyName("errorInvalidSsn")]
    public Common ErrorInvalidSsn
    {
      get => errorInvalidSsn ??= new();
      set => errorInvalidSsn = value;
    }

    private Common countOfApRead;
    private Common apQueriesCreated;
    private Common countOfArRead;
    private Common arQueriesCreated;
    private Common errorInvalidSsn;
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

    private Common subscript;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
