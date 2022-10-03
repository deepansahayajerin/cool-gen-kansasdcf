// Program: SP_B316_CLOSE_REPORTS, ID: 374563320, model: 746.
// Short name: SWE03129
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B316_CLOSE_REPORTS.
/// </summary>
[Serializable]
public partial class SpB316CloseReports: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B316_CLOSE_REPORTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB316CloseReports(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB316CloseReports.
  /// </summary>
  public SpB316CloseReports(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------
    //                   M A I N T E N A N C E   L O G
    // Date		Developer	Request #	Description
    // 07/14/10	J Huss		CQ# 20646	Initial development.
    // ------------------------------------------------------------------
    if (!IsEmpty(import.Error.RptDetail))
    {
      // If an error message is sent, set the exit state to abort so the job 
      // will return a bad code.
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport1();
    }

    local.EabFileHandling.Action = "CLOSE";
    UseSpB316EabReadInputFile();

    // If the dataset did not close successfully, write a message to the error 
    // report.
    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = local.External.TextLine130;
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport1();
      local.EabFileHandling.Action = "CLOSE";
    }

    UseCabControlReport();
    UseCabErrorReport2();
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = import.Error.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseSpB316EabReadInputFile()
  {
    var useImport = new SpB316EabReadInputFile.Import();
    var useExport = new SpB316EabReadInputFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabFileHandling(local.EabFileHandling, useExport.EabFileHandling);
    useExport.ResultDetail.TextLine130 = local.External.TextLine130;

    Call(SpB316EabReadInputFile.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
    local.External.TextLine130 = useExport.ResultDetail.TextLine130;
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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public EabReportSend Error
    {
      get => error ??= new();
      set => error = value;
    }

    private EabReportSend error;
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

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private External external;
  }
#endregion
}
