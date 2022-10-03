// Program: OE_B491_CLOSE, ID: 371174485, model: 746.
// Short name: SWE02485
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B491_CLOSE.
/// </summary>
[Serializable]
public partial class OeB491Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B491_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB491Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB491Close.
  /// </summary>
  public OeB491Close(IContext context, Import import, Export export):
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
          local.NeededToWrite.RptDetail =
            "RECORDS READ (Excl hdr & trailer)  " + "   " + NumberToString
            (import.RecordsRead.Count, 15);

          break;
        case 3:
          local.NeededToWrite.RptDetail =
            "COMPANIES ADDED                    " + "   " + NumberToString
            (import.CompaniesAdded.Count, 15);

          break;
        case 4:
          local.NeededToWrite.RptDetail =
            "COMPANIES UPDATED                  " + "   " + NumberToString
            (import.CompaniesUpdated.Count, 15);

          break;
        case 5:
          local.NeededToWrite.RptDetail =
            "COMPANIES SKIPPED INVALID INFO     " + "   " + NumberToString
            (import.CompaniesInvalidInfo.Count, 15);

          break;
        case 6:
          local.NeededToWrite.RptDetail =
            "COMPANIES NO CHANGE                " + "   " + NumberToString
            (import.CompaniesNoChange.Count, 15);

          break;
        case 7:
          local.EabFileHandling.Action = "CLOSE";

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
    while(local.Subscript.Count <= 7);

    UseEabReadHinsCoFile();
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

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

  private void UseEabReadHinsCoFile()
  {
    var useImport = new EabReadHinsCoFile.Import();
    var useExport = new EabReadHinsCoFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadHinsCoFile.Execute, useImport, useExport);

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
    /// A value of CompaniesAdded.
    /// </summary>
    [JsonPropertyName("companiesAdded")]
    public Common CompaniesAdded
    {
      get => companiesAdded ??= new();
      set => companiesAdded = value;
    }

    /// <summary>
    /// A value of CompaniesUpdated.
    /// </summary>
    [JsonPropertyName("companiesUpdated")]
    public Common CompaniesUpdated
    {
      get => companiesUpdated ??= new();
      set => companiesUpdated = value;
    }

    /// <summary>
    /// A value of CompaniesNoChange.
    /// </summary>
    [JsonPropertyName("companiesNoChange")]
    public Common CompaniesNoChange
    {
      get => companiesNoChange ??= new();
      set => companiesNoChange = value;
    }

    /// <summary>
    /// A value of CompaniesInvalidInfo.
    /// </summary>
    [JsonPropertyName("companiesInvalidInfo")]
    public Common CompaniesInvalidInfo
    {
      get => companiesInvalidInfo ??= new();
      set => companiesInvalidInfo = value;
    }

    private Common recordsRead;
    private Common companiesAdded;
    private Common companiesUpdated;
    private Common companiesNoChange;
    private Common companiesInvalidInfo;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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

    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private Common subscript;
  }
#endregion
}
