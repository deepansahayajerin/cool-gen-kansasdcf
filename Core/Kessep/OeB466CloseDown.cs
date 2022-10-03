// Program: OE_B466_CLOSE_DOWN, ID: 374469498, model: 746.
// Short name: SWE02707
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B466_CLOSE_DOWN.
/// </summary>
[Serializable]
public partial class OeB466CloseDown: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B466_CLOSE_DOWN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB466CloseDown(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB466CloseDown.
  /// </summary>
  public OeB466CloseDown(IContext context, Import import, Export export):
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

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail =
        "Medical Obligiaton to URA Process has ABENDed due to an unrecoverable error.";
        
      ExitState = "ACO_NN0000_ALL_OK";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      if (IsEmpty(local.ExitStateWorkArea.Message))
      {
        local.EabReportSend.RptDetail =
          "Unknown Error Has Occurred - An ABEND has been issued.";
      }
      else
      {
        local.EabReportSend.RptDetail = "Exit State : " + local
          .ExitStateWorkArea.Message;
      }

      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabReportSend.RptDetail =
        "------------------------------------------------------------------------------------------------------------------------------------";
        
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.EabReportSend.RptDetail = "RUN RESULTS AS FOLLOWS:";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Records Read . . . . . . . . . . . . : " + NumberToString
      (import.RecsRead.Count, 15) + "  -  $ " + NumberToString
      ((long)(import.TotAmtRead.TotalCurrency * 100), 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Records Errored. . . . . . . . . . . : " + NumberToString
      (import.RecsErrored.Count, 15) + "  -  $ " + NumberToString
      ((long)(import.TotAmtErrored.TotalCurrency * 100), 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Records Skipped. . . . . . . . . . . : " + NumberToString
      (import.RecsSkipped.Count, 15) + "  -  $ " + NumberToString
      ((long)(import.TotAmtSkipped.TotalCurrency * 100), 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Records Processed. . . . . . . . . . : " + NumberToString
      (import.RecsProcessed.Count, 15) + "  -  $ " + NumberToString
      ((long)(import.TotAmtProcessed.TotalCurrency * 100), 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

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

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
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
    /// A value of RecsSkipped.
    /// </summary>
    [JsonPropertyName("recsSkipped")]
    public Common RecsSkipped
    {
      get => recsSkipped ??= new();
      set => recsSkipped = value;
    }

    /// <summary>
    /// A value of RecsRead.
    /// </summary>
    [JsonPropertyName("recsRead")]
    public Common RecsRead
    {
      get => recsRead ??= new();
      set => recsRead = value;
    }

    /// <summary>
    /// A value of RecsProcessed.
    /// </summary>
    [JsonPropertyName("recsProcessed")]
    public Common RecsProcessed
    {
      get => recsProcessed ??= new();
      set => recsProcessed = value;
    }

    /// <summary>
    /// A value of RecsErrored.
    /// </summary>
    [JsonPropertyName("recsErrored")]
    public Common RecsErrored
    {
      get => recsErrored ??= new();
      set => recsErrored = value;
    }

    /// <summary>
    /// A value of TotAmtSkipped.
    /// </summary>
    [JsonPropertyName("totAmtSkipped")]
    public Common TotAmtSkipped
    {
      get => totAmtSkipped ??= new();
      set => totAmtSkipped = value;
    }

    /// <summary>
    /// A value of TotAmtRead.
    /// </summary>
    [JsonPropertyName("totAmtRead")]
    public Common TotAmtRead
    {
      get => totAmtRead ??= new();
      set => totAmtRead = value;
    }

    /// <summary>
    /// A value of TotAmtProcessed.
    /// </summary>
    [JsonPropertyName("totAmtProcessed")]
    public Common TotAmtProcessed
    {
      get => totAmtProcessed ??= new();
      set => totAmtProcessed = value;
    }

    /// <summary>
    /// A value of TotAmtErrored.
    /// </summary>
    [JsonPropertyName("totAmtErrored")]
    public Common TotAmtErrored
    {
      get => totAmtErrored ??= new();
      set => totAmtErrored = value;
    }

    private Common recsSkipped;
    private Common recsRead;
    private Common recsProcessed;
    private Common recsErrored;
    private Common totAmtSkipped;
    private Common totAmtRead;
    private Common totAmtProcessed;
    private Common totAmtErrored;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of TotAmtRead.
    /// </summary>
    [JsonPropertyName("totAmtRead")]
    public Common TotAmtRead
    {
      get => totAmtRead ??= new();
      set => totAmtRead = value;
    }

    /// <summary>
    /// A value of ZdelMe12.
    /// </summary>
    [JsonPropertyName("zdelMe12")]
    public Common ZdelMe12
    {
      get => zdelMe12 ??= new();
      set => zdelMe12 = value;
    }

    /// <summary>
    /// A value of TotAmtErrored.
    /// </summary>
    [JsonPropertyName("totAmtErrored")]
    public Common TotAmtErrored
    {
      get => totAmtErrored ??= new();
      set => totAmtErrored = value;
    }

    /// <summary>
    /// A value of ZdelMe10.
    /// </summary>
    [JsonPropertyName("zdelMe10")]
    public Common ZdelMe10
    {
      get => zdelMe10 ??= new();
      set => zdelMe10 = value;
    }

    /// <summary>
    /// A value of TotAmtProcessed.
    /// </summary>
    [JsonPropertyName("totAmtProcessed")]
    public Common TotAmtProcessed
    {
      get => totAmtProcessed ??= new();
      set => totAmtProcessed = value;
    }

    /// <summary>
    /// A value of ZdelMe11.
    /// </summary>
    [JsonPropertyName("zdelMe11")]
    public Common ZdelMe11
    {
      get => zdelMe11 ??= new();
      set => zdelMe11 = value;
    }

    /// <summary>
    /// A value of ZdelMe17.
    /// </summary>
    [JsonPropertyName("zdelMe17")]
    public Common ZdelMe17
    {
      get => zdelMe17 ??= new();
      set => zdelMe17 = value;
    }

    /// <summary>
    /// A value of ZdelMe9.
    /// </summary>
    [JsonPropertyName("zdelMe9")]
    public Common ZdelMe9
    {
      get => zdelMe9 ??= new();
      set => zdelMe9 = value;
    }

    /// <summary>
    /// A value of ZdelMe13.
    /// </summary>
    [JsonPropertyName("zdelMe13")]
    public Common ZdelMe13
    {
      get => zdelMe13 ??= new();
      set => zdelMe13 = value;
    }

    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private Common totAmtRead;
    private Common zdelMe12;
    private Common totAmtErrored;
    private Common zdelMe10;
    private Common totAmtProcessed;
    private Common zdelMe11;
    private Common zdelMe17;
    private Common zdelMe9;
    private Common zdelMe13;
  }
#endregion
}
