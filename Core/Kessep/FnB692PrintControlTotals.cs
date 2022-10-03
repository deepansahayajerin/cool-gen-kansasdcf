// Program: FN_B692_PRINT_CONTROL_TOTALS, ID: 374398778, model: 746.
// Short name: SWE00079
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B692_PRINT_CONTROL_TOTALS.
/// </summary>
[Serializable]
public partial class FnB692PrintControlTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B692_PRINT_CONTROL_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB692PrintControlTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB692PrintControlTotals.
  /// </summary>
  public FnB692PrintControlTotals(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.CourtOrdersInError.Count = import.CourtOrdersRead.Count - import
      .CourtOrdersProcessed.Count;
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "Court Order records read. . . . . . :";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (import.CourtOrdersRead.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Court Order records processed . . . :";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (import.CourtOrdersProcessed.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Court Order records in error. . . . :";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (local.CourtOrdersInError.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(import.CloseInd.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "CLOSE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
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
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

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
    /// A value of CourtOrdersRead.
    /// </summary>
    [JsonPropertyName("courtOrdersRead")]
    public Common CourtOrdersRead
    {
      get => courtOrdersRead ??= new();
      set => courtOrdersRead = value;
    }

    /// <summary>
    /// A value of CourtOrdersProcessed.
    /// </summary>
    [JsonPropertyName("courtOrdersProcessed")]
    public Common CourtOrdersProcessed
    {
      get => courtOrdersProcessed ??= new();
      set => courtOrdersProcessed = value;
    }

    /// <summary>
    /// A value of CloseInd.
    /// </summary>
    [JsonPropertyName("closeInd")]
    public Common CloseInd
    {
      get => closeInd ??= new();
      set => closeInd = value;
    }

    private Common courtOrdersRead;
    private Common courtOrdersProcessed;
    private Common closeInd;
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
    /// A value of CourtOrdersInError.
    /// </summary>
    [JsonPropertyName("courtOrdersInError")]
    public Common CourtOrdersInError
    {
      get => courtOrdersInError ??= new();
      set => courtOrdersInError = value;
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

    private Common courtOrdersInError;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
