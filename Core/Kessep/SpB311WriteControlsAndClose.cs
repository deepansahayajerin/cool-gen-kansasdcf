// Program: SP_B311_WRITE_CONTROLS_AND_CLOSE, ID: 374351480, model: 746.
// Short name: SWE02523
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B311_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SpB311WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B311_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB311WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB311WriteControlsAndClose.
  /// </summary>
  public SpB311WriteControlsAndClose(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.MaxControlTotal.Count = 99;

    // -------------------------------------------------------
    // WRITE OUTPUT CONTROL REPORT
    // -------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabConvertNumeric.SendNonSuppressPos = 1;
    local.Subscript.Count = 1;

    for(var limit = local.MaxControlTotal.Count; local.Subscript.Count <= limit
      ; ++local.Subscript.Count)
    {
      local.EabReportSend.RptDetail = "";
      local.Label.Text40 = "";
      local.EabConvertNumeric.SendAmount = "";
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal = "";

      switch(local.Subscript.Count)
      {
        case 1:
          local.Label.Text40 = "TRIGGERS READ";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TriggersRead.Count, 15);

          break;
        case 2:
          local.Label.Text40 = "TRIGGERS PROCESSED SUCCESSFULLY";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TriggersProcessed.Count, 15);

          break;
        case 3:
          local.Label.Text40 = "TRIGGERS ERRED";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TriggersErred.Count, 15);

          break;
        default:
          goto AfterCycle;
      }

      if (!IsEmpty(local.Label.Text40))
      {
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail = local.Label.Text40 + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "Error encountered writing to Control Report";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

AfterCycle:

    // -------------------------------------------------------
    // CLOSE OUTPUT CONTROL REPORT
    // -------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";

      return;
    }

    // --------------------------------------------------------
    // CLOSE OUTPUT ERROR REPORT
    // --------------------------------------------------------
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";
    }
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric2(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal;
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
    /// A value of TriggersRead.
    /// </summary>
    [JsonPropertyName("triggersRead")]
    public Common TriggersRead
    {
      get => triggersRead ??= new();
      set => triggersRead = value;
    }

    /// <summary>
    /// A value of TriggersProcessed.
    /// </summary>
    [JsonPropertyName("triggersProcessed")]
    public Common TriggersProcessed
    {
      get => triggersProcessed ??= new();
      set => triggersProcessed = value;
    }

    /// <summary>
    /// A value of TriggersErred.
    /// </summary>
    [JsonPropertyName("triggersErred")]
    public Common TriggersErred
    {
      get => triggersErred ??= new();
      set => triggersErred = value;
    }

    private Common triggersRead;
    private Common triggersProcessed;
    private Common triggersErred;
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
    /// A value of Label.
    /// </summary>
    [JsonPropertyName("label")]
    public WorkArea Label
    {
      get => label ??= new();
      set => label = value;
    }

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of MaxControlTotal.
    /// </summary>
    [JsonPropertyName("maxControlTotal")]
    public Common MaxControlTotal
    {
      get => maxControlTotal ??= new();
      set => maxControlTotal = value;
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

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private WorkArea label;
    private EabConvertNumeric2 eabConvertNumeric;
    private Common maxControlTotal;
    private Common subscript;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
