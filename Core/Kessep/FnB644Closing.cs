﻿// Program: FN_B644_CLOSING, ID: 372691360, model: 746.
// Short name: SWE02455
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B644_CLOSING.
/// </summary>
[Serializable]
public partial class FnB644Closing: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B644_CLOSING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB644Closing(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB644Closing.
  /// </summary>
  public FnB644Closing(IContext context, Import import, Export export):
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
            "OBLIGATIONS PROCESSED              " + "   " + NumberToString
            (import.ObligationsProcessed.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "MONTHLY SUMMARIES CREATED          " + "   " + NumberToString
            (import.MonthlySummariesCreated.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "OBLIGATIONS NOT YET ACTIVE         " + "   " + NumberToString
            (import.ObligationsNotActive.Count, 15);

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
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 4);

    // **********************************************************
    // CLOSE OUTPUT BUSINESS REPORT
    // **********************************************************
    UseCabBusinessReport01();

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
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
    /// A value of ObligationsProcessed.
    /// </summary>
    [JsonPropertyName("obligationsProcessed")]
    public Common ObligationsProcessed
    {
      get => obligationsProcessed ??= new();
      set => obligationsProcessed = value;
    }

    /// <summary>
    /// A value of MonthlySummariesCreated.
    /// </summary>
    [JsonPropertyName("monthlySummariesCreated")]
    public Common MonthlySummariesCreated
    {
      get => monthlySummariesCreated ??= new();
      set => monthlySummariesCreated = value;
    }

    /// <summary>
    /// A value of ObligationsNotActive.
    /// </summary>
    [JsonPropertyName("obligationsNotActive")]
    public Common ObligationsNotActive
    {
      get => obligationsNotActive ??= new();
      set => obligationsNotActive = value;
    }

    private Common obligationsProcessed;
    private Common monthlySummariesCreated;
    private Common obligationsNotActive;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private EabFileHandling eabFileHandling;
    private Common subscript;
    private EabReportSend eabReportSend;
  }
#endregion
}
