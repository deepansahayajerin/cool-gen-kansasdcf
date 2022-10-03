// Program: FN_B642_CLOSE, ID: 372272023, model: 746.
// Short name: SWE02355
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B642_CLOSE.
/// </summary>
[Serializable]
public partial class FnB642Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B642_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB642Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB642Close.
  /// </summary>
  public FnB642Close(IContext context, Import import, Export export):
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
    local.Subscript.Count = 1;

    do
    {
      switch(local.Subscript.Count)
      {
        case 1:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "CASH RECEIPT DETAILS UPDATED       " + "   " + NumberToString
            (import.NoCashRecptDtlUpdated.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "COLLECTIONS REVERSED               " + "   " + NumberToString
            (import.NoCollectionsReversed.Count, 15);

          break;
        case 3:
          local.EabFileHandling.Action = "CLOSE";

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

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 3);

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

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

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
    /// A value of NoCashRecptDtlUpdated.
    /// </summary>
    [JsonPropertyName("noCashRecptDtlUpdated")]
    public Common NoCashRecptDtlUpdated
    {
      get => noCashRecptDtlUpdated ??= new();
      set => noCashRecptDtlUpdated = value;
    }

    /// <summary>
    /// A value of NoCollectionsReversed.
    /// </summary>
    [JsonPropertyName("noCollectionsReversed")]
    public Common NoCollectionsReversed
    {
      get => noCollectionsReversed ??= new();
      set => noCollectionsReversed = value;
    }

    private Common noCashRecptDtlUpdated;
    private Common noCollectionsReversed;
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

    private EabFileHandling eabFileHandling;
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

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common subscript;
  }
#endregion
}
