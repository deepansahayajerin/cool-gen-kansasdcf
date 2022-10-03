// Program: FN_B647_CLOSING, ID: 372995025, model: 746.
// Short name: SWE02397
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B647_CLOSING.
/// </summary>
[Serializable]
public partial class FnB647Closing: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B647_CLOSING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB647Closing(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB647Closing.
  /// </summary>
  public FnB647Closing(IContext context, Import import, Export export):
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
            "INFRASTRUCTURE DELETED             " + "   " + NumberToString
            (import.InfrastructureDeleted.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "INFRASTRUCTURE CREATED             " + "   " + NumberToString
            (import.InfrastructureCreated.Count, 15);

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
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 3);

    // **********************************************************
    // CLOSE INPUT AP STMT VENDOR FILE
    // **********************************************************
    UseEabReadApStmtsVendorFile();

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
    UseCabErrorReport();
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

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadApStmtsVendorFile()
  {
    var useImport = new EabReadApStmtsVendorFile.Import();
    var useExport = new EabReadApStmtsVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadApStmtsVendorFile.Execute, useImport, useExport);

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
    /// A value of InfrastructureDeleted.
    /// </summary>
    [JsonPropertyName("infrastructureDeleted")]
    public Common InfrastructureDeleted
    {
      get => infrastructureDeleted ??= new();
      set => infrastructureDeleted = value;
    }

    /// <summary>
    /// A value of InfrastructureCreated.
    /// </summary>
    [JsonPropertyName("infrastructureCreated")]
    public Common InfrastructureCreated
    {
      get => infrastructureCreated ??= new();
      set => infrastructureCreated = value;
    }

    private Common infrastructureDeleted;
    private Common infrastructureCreated;
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
