// Program: SP_B308_WRITE_CONTROLS_AND_CLOSE, ID: 374477699, model: 746.
// Short name: SWE02713
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B308_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SpB308WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B308_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB308WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB308WriteControlsAndClose.
  /// </summary>
  public SpB308WriteControlsAndClose(IContext context, Import import,
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
          local.Label.Text40 = "Total Outgoing Documents Read";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsRead.Count, 15);

          break;
        case 3:
          local.Label.Text40 = "Total Field Values Read";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.FieldValuesRead.Count, 15);

          break;
        case 4:
          local.Label.Text40 = "Total Outgoing Docs Archived and Updated";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsArchvdUpdated.Count, 15);

          break;
        case 5:
          local.Label.Text40 = "Total Outgoing Documents only Updated";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsOnlyUpdated.Count, 15);

          break;
        case 6:
          local.Label.Text40 = "Total Field Values Archived and Deleted";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.FieldValuesArchvdDeltd.Count, 15);

          break;
        case 7:
          local.Label.Text40 = "Total Field Values only Deleted";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.FieldValuesOnlyDeleted.Count, 15);

          break;
        case 8:
          local.Label.Text40 = "Total Records written on Archive File";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.RecsWrittenToArchive.Count, 15);

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
    UseSpEabArchiveFieldValuesRs();

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

  private void UseSpEabArchiveFieldValuesRs()
  {
    var useImport = new SpEabArchiveFieldValuesRs.Import();
    var useExport = new SpEabArchiveFieldValuesRs.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    MoveExternal(local.External, useExport.External);

    Call(SpEabArchiveFieldValuesRs.Execute, useImport, useExport);

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
    /// A value of DocsRead.
    /// </summary>
    [JsonPropertyName("docsRead")]
    public Common DocsRead
    {
      get => docsRead ??= new();
      set => docsRead = value;
    }

    /// <summary>
    /// A value of FieldValuesRead.
    /// </summary>
    [JsonPropertyName("fieldValuesRead")]
    public Common FieldValuesRead
    {
      get => fieldValuesRead ??= new();
      set => fieldValuesRead = value;
    }

    /// <summary>
    /// A value of DocsArchvdUpdated.
    /// </summary>
    [JsonPropertyName("docsArchvdUpdated")]
    public Common DocsArchvdUpdated
    {
      get => docsArchvdUpdated ??= new();
      set => docsArchvdUpdated = value;
    }

    /// <summary>
    /// A value of FieldValuesOnlyDeleted.
    /// </summary>
    [JsonPropertyName("fieldValuesOnlyDeleted")]
    public Common FieldValuesOnlyDeleted
    {
      get => fieldValuesOnlyDeleted ??= new();
      set => fieldValuesOnlyDeleted = value;
    }

    /// <summary>
    /// A value of RecsWrittenToArchive.
    /// </summary>
    [JsonPropertyName("recsWrittenToArchive")]
    public Common RecsWrittenToArchive
    {
      get => recsWrittenToArchive ??= new();
      set => recsWrittenToArchive = value;
    }

    /// <summary>
    /// A value of FieldValuesArchvdDeltd.
    /// </summary>
    [JsonPropertyName("fieldValuesArchvdDeltd")]
    public Common FieldValuesArchvdDeltd
    {
      get => fieldValuesArchvdDeltd ??= new();
      set => fieldValuesArchvdDeltd = value;
    }

    /// <summary>
    /// A value of DocsOnlyUpdated.
    /// </summary>
    [JsonPropertyName("docsOnlyUpdated")]
    public Common DocsOnlyUpdated
    {
      get => docsOnlyUpdated ??= new();
      set => docsOnlyUpdated = value;
    }

    private Common docsRead;
    private Common fieldValuesRead;
    private Common docsArchvdUpdated;
    private Common fieldValuesOnlyDeleted;
    private Common recsWrittenToArchive;
    private Common fieldValuesArchvdDeltd;
    private Common docsOnlyUpdated;
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
