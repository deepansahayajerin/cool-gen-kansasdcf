// Program: SP_B309_WRITE_CONTROLS_AND_CLOSE, ID: 370942433, model: 746.
// Short name: SWE02719
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B309_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SpB309WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B309_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB309WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB309WriteControlsAndClose.
  /// </summary>
  public SpB309WriteControlsAndClose(IContext context, Import import,
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
      local.Label.Text40 = "";
      local.EabConvertNumeric.SendAmount = "";
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal = "";

      switch(local.Subscript.Count)
      {
        case 1:
          local.Label.Text40 = "";

          break;
        case 2:
          local.Label.Text40 = "Total Triggers read";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TriggersRead.Count, 15);

          break;
        case 3:
          local.Label.Text40 = "Total Archived records read";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ArchivedRecsRead.Count, 15);

          break;
        case 4:
          local.Label.Text40 = "Total Field Values created";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.FieldValuesCreated.Count, 15);

          break;
        case 5:
          local.Label.Text40 = "Total Outgoing Documents retrieved";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsRetrieved.Count, 15);

          break;
        case 6:
          local.Label.Text40 = "Total Outgoing Documents failed";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsErrored.Count, 15);

          break;
        case 7:
          local.Label.Text40 = "Total Triggers deleted";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.TriggersDeleted.Count, 15);

          break;
        case 8:
          local.Label.Text40 = "Total Alerts created";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.AlertsCreated.Count, 15);

          break;
        case 9:
          local.Label.Text40 = "Total Infrastructure created";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.HistoryCreated.Count, 15);

          break;
        default:
          goto AfterCycle;
      }

      if (!IsEmpty(local.EabConvertNumeric.SendAmount))
      {
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail = local.Label.Text40 + local
          .EabConvertNumeric.ReturnNoCommasInNonDecimal;
      }
      else
      {
        local.EabReportSend.RptDetail = local.Label.Text40;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "Error writing to Control Report";
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
    local.External.FileInstruction = "CLOSE";
    UseSpEabRetrieveFieldValues();

    if (!IsEmpty(local.External.TextReturnCode))
    {
      local.EabReportSend.RptDetail = "Error closing Archive File";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error closing Control Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // --------------------------------------------------------
    // CLOSE OUTPUT ERROR REPORT
    // --------------------------------------------------------
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error closing Error Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextReturnCode = source.TextReturnCode;
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

  private void UseSpEabRetrieveFieldValues()
  {
    var useImport = new SpEabRetrieveFieldValues.Import();
    var useExport = new SpEabRetrieveFieldValues.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    MoveExternal(local.External, useExport.External);

    Call(SpEabRetrieveFieldValues.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
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
    /// A value of ArchivedRecsRead.
    /// </summary>
    [JsonPropertyName("archivedRecsRead")]
    public Common ArchivedRecsRead
    {
      get => archivedRecsRead ??= new();
      set => archivedRecsRead = value;
    }

    /// <summary>
    /// A value of FieldValuesCreated.
    /// </summary>
    [JsonPropertyName("fieldValuesCreated")]
    public Common FieldValuesCreated
    {
      get => fieldValuesCreated ??= new();
      set => fieldValuesCreated = value;
    }

    /// <summary>
    /// A value of DocsRetrieved.
    /// </summary>
    [JsonPropertyName("docsRetrieved")]
    public Common DocsRetrieved
    {
      get => docsRetrieved ??= new();
      set => docsRetrieved = value;
    }

    /// <summary>
    /// A value of DocsErrored.
    /// </summary>
    [JsonPropertyName("docsErrored")]
    public Common DocsErrored
    {
      get => docsErrored ??= new();
      set => docsErrored = value;
    }

    /// <summary>
    /// A value of TriggersDeleted.
    /// </summary>
    [JsonPropertyName("triggersDeleted")]
    public Common TriggersDeleted
    {
      get => triggersDeleted ??= new();
      set => triggersDeleted = value;
    }

    /// <summary>
    /// A value of AlertsCreated.
    /// </summary>
    [JsonPropertyName("alertsCreated")]
    public Common AlertsCreated
    {
      get => alertsCreated ??= new();
      set => alertsCreated = value;
    }

    /// <summary>
    /// A value of HistoryCreated.
    /// </summary>
    [JsonPropertyName("historyCreated")]
    public Common HistoryCreated
    {
      get => historyCreated ??= new();
      set => historyCreated = value;
    }

    private Common triggersRead;
    private Common archivedRecsRead;
    private Common fieldValuesCreated;
    private Common docsRetrieved;
    private Common docsErrored;
    private Common triggersDeleted;
    private Common alertsCreated;
    private Common historyCreated;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

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

    private External external;
    private WorkArea label;
    private EabConvertNumeric2 eabConvertNumeric;
    private Common maxControlTotal;
    private Common subscript;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
