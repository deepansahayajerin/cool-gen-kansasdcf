// Program: OE_B496_CLOSE, ID: 371173814, model: 746.
// Short name: SWE02636
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B496_CLOSE.
/// </summary>
[Serializable]
public partial class OeB496Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B496_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB496Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB496Close.
  /// </summary>
  public OeB496Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // : Write number of records written to control report.
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "Number of companies updated: " + NumberToString
      (import.CompaniesUpdated.Count, 15);
    UseCabControlReport2();
    local.NeededToWrite.RptDetail = "Number of coverages updated: " + NumberToString
      (import.CoveragesUpdated.Count, 15);
    UseCabControlReport2();
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport();
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
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
    /// A value of CompaniesUpdated.
    /// </summary>
    [JsonPropertyName("companiesUpdated")]
    public Common CompaniesUpdated
    {
      get => companiesUpdated ??= new();
      set => companiesUpdated = value;
    }

    /// <summary>
    /// A value of CoveragesUpdated.
    /// </summary>
    [JsonPropertyName("coveragesUpdated")]
    public Common CoveragesUpdated
    {
      get => coveragesUpdated ??= new();
      set => coveragesUpdated = value;
    }

    private Common companiesUpdated;
    private Common coveragesUpdated;
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
#endregion
}
