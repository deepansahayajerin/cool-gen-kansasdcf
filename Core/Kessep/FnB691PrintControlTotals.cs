// Program: FN_B691_PRINT_CONTROL_TOTALS, ID: 374396223, model: 746.
// Short name: SWE00078
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B691_PRINT_CONTROL_TOTALS.
/// </summary>
[Serializable]
public partial class FnB691PrintControlTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B691_PRINT_CONTROL_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB691PrintControlTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB691PrintControlTotals.
  /// </summary>
  public FnB691PrintControlTotals(IContext context, Import import, Export export)
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
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "Court Order Changes . . . . . . . . :";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (import.RcoRecsWritten.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "New Court Orders. . . . . . . . . . :";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (import.NcoRecsWritten.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "New Debts processed . . . . . . . . :";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (import.NdbtRecsWritten.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Legal Actions Already Sent. . . . . :";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (import.LegalActionAlreadySent.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Total records written . . . . . . . :";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (import.TotalRecsWritten.Count, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
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
    /// A value of LegalActionAlreadySent.
    /// </summary>
    [JsonPropertyName("legalActionAlreadySent")]
    public Common LegalActionAlreadySent
    {
      get => legalActionAlreadySent ??= new();
      set => legalActionAlreadySent = value;
    }

    /// <summary>
    /// A value of TotalRecsWritten.
    /// </summary>
    [JsonPropertyName("totalRecsWritten")]
    public Common TotalRecsWritten
    {
      get => totalRecsWritten ??= new();
      set => totalRecsWritten = value;
    }

    /// <summary>
    /// A value of NdbtRecsWritten.
    /// </summary>
    [JsonPropertyName("ndbtRecsWritten")]
    public Common NdbtRecsWritten
    {
      get => ndbtRecsWritten ??= new();
      set => ndbtRecsWritten = value;
    }

    /// <summary>
    /// A value of NcoRecsWritten.
    /// </summary>
    [JsonPropertyName("ncoRecsWritten")]
    public Common NcoRecsWritten
    {
      get => ncoRecsWritten ??= new();
      set => ncoRecsWritten = value;
    }

    /// <summary>
    /// A value of RcoRecsWritten.
    /// </summary>
    [JsonPropertyName("rcoRecsWritten")]
    public Common RcoRecsWritten
    {
      get => rcoRecsWritten ??= new();
      set => rcoRecsWritten = value;
    }

    private Common legalActionAlreadySent;
    private Common totalRecsWritten;
    private Common ndbtRecsWritten;
    private Common ncoRecsWritten;
    private Common rcoRecsWritten;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
