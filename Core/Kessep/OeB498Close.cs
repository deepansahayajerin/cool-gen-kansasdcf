// Program: OE_B498_CLOSE, ID: 371179248, model: 746.
// Short name: SWE01978
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B498_CLOSE.
/// </summary>
[Serializable]
public partial class OeB498Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B498_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB498Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB498Close.
  /// </summary>
  public OeB498Close(IContext context, Import import, Export export):
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
          local.NeededToWrite.RptDetail = "";

          break;
        case 2:
          local.NeededToWrite.RptDetail = "";

          break;
        case 3:
          local.NeededToWrite.RptDetail =
            "Total health insurance coverages expired:        " + "   " + NumberToString
            (import.TotHiCoveragesExpired.Count, 15);

          break;
        case 4:
          local.NeededToWrite.RptDetail =
            "Total personal health insurance policies expired:" + "   " + NumberToString
            (import.TotPersonalHiExpired.Count, 15);

          break;
        case 5:
          local.NeededToWrite.RptDetail = "";

          break;
        case 6:
          local.NeededToWrite.RptDetail = "";

          break;
        case 7:
          local.EabFileHandling.Action = "CLOSE";

          break;
        default:
          break;
      }

      UseCabControlReport();
      UseCabBusinessReport01();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 7);

    UseCabErrorReport();
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

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
    /// A value of TotHiCoveragesExpired.
    /// </summary>
    [JsonPropertyName("totHiCoveragesExpired")]
    public Common TotHiCoveragesExpired
    {
      get => totHiCoveragesExpired ??= new();
      set => totHiCoveragesExpired = value;
    }

    /// <summary>
    /// A value of TotPersonalHiExpired.
    /// </summary>
    [JsonPropertyName("totPersonalHiExpired")]
    public Common TotPersonalHiExpired
    {
      get => totPersonalHiExpired ??= new();
      set => totPersonalHiExpired = value;
    }

    private Common totHiCoveragesExpired;
    private Common totPersonalHiExpired;
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

    private EabFileHandling eabFileHandling;
    private Common subscript;
    private EabReportSend neededToWrite;
  }
#endregion
}
