// Program: SP_B701_WRITE_CONTROLS_AND_CLOSE, ID: 372446812, model: 746.
// Short name: SWE02305
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B701_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SpB701WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B701_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB701WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB701WriteControlsAndClose.
  /// </summary>
  public SpB701WriteControlsAndClose(IContext context, Import import,
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
          local.Label.Text40 = "UPDATE DATE OF EMANCIPATION";

          break;
        case 2:
          local.Label.Text40 = "     CHILDREN READ";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ChildRead.Count, 15);

          break;
        case 3:
          local.Label.Text40 = "     CHILDREN UPDATED";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ChildUpdated.Count, 15);

          break;
        case 4:
          local.Label.Text40 = "     CHILDREN ERRED";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ChildError.Count, 15);

          break;
        case 5:
          local.Label.Text40 = "";

          break;
        case 6:
          local.Label.Text40 = "DOCUMENT TRIGGERS";

          break;
        case 7:
          local.Label.Text40 = "     CHILDREN READ";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsRead.Count, 15);

          break;
        case 8:
          local.Label.Text40 = "     KS DOCUMENT TRIGGERS CREATED";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.KsDocsCreated.Count, 15);

          break;
        case 9:
          local.Label.Text40 = "     IC DOCUMENT TRIGGERS CREATED";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.IcDocsCreated.Count, 15);

          break;
        case 10:
          local.Label.Text40 = "     CHILDREN UNPROCESSED (CASE CLOSED)";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsClosedCases.Count, 15);

          break;
        case 11:
          local.Label.Text40 = "     CHILDREN UNPROCESSED (NO AR/ORG)";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsMissingAr.Count, 15);

          break;
        case 12:
          local.Label.Text40 = "     CHILDREN UNPROCESSED (XA CHILD)";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsXaChild.Count, 15);

          break;
        case 13:
          local.Label.Text40 = "     CHILDREN UNPROCESSED (PREV CREATED)";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsPrevPrints.Count, 15);

          break;
        case 14:
          local.Label.Text40 = "     CHILDREN UNPROCESSED (FOREIGN ORDR)";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsForeignOrder.Count, 15);

          break;
        case 15:
          local.Label.Text40 = "     CHILDREN UNPROCESSED (DATA ERROR)";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsError.Count, 15);

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
      }
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
    /// A value of IcDocsCreated.
    /// </summary>
    [JsonPropertyName("icDocsCreated")]
    public Common IcDocsCreated
    {
      get => icDocsCreated ??= new();
      set => icDocsCreated = value;
    }

    /// <summary>
    /// A value of ChildRead.
    /// </summary>
    [JsonPropertyName("childRead")]
    public Common ChildRead
    {
      get => childRead ??= new();
      set => childRead = value;
    }

    /// <summary>
    /// A value of ChildUpdated.
    /// </summary>
    [JsonPropertyName("childUpdated")]
    public Common ChildUpdated
    {
      get => childUpdated ??= new();
      set => childUpdated = value;
    }

    /// <summary>
    /// A value of ChildError.
    /// </summary>
    [JsonPropertyName("childError")]
    public Common ChildError
    {
      get => childError ??= new();
      set => childError = value;
    }

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
    /// A value of KsDocsCreated.
    /// </summary>
    [JsonPropertyName("ksDocsCreated")]
    public Common KsDocsCreated
    {
      get => ksDocsCreated ??= new();
      set => ksDocsCreated = value;
    }

    /// <summary>
    /// A value of DocsError.
    /// </summary>
    [JsonPropertyName("docsError")]
    public Common DocsError
    {
      get => docsError ??= new();
      set => docsError = value;
    }

    /// <summary>
    /// A value of DocsPrevPrints.
    /// </summary>
    [JsonPropertyName("docsPrevPrints")]
    public Common DocsPrevPrints
    {
      get => docsPrevPrints ??= new();
      set => docsPrevPrints = value;
    }

    /// <summary>
    /// A value of DocsClosedCases.
    /// </summary>
    [JsonPropertyName("docsClosedCases")]
    public Common DocsClosedCases
    {
      get => docsClosedCases ??= new();
      set => docsClosedCases = value;
    }

    /// <summary>
    /// A value of DocsMissingAr.
    /// </summary>
    [JsonPropertyName("docsMissingAr")]
    public Common DocsMissingAr
    {
      get => docsMissingAr ??= new();
      set => docsMissingAr = value;
    }

    /// <summary>
    /// A value of DocsXaChild.
    /// </summary>
    [JsonPropertyName("docsXaChild")]
    public Common DocsXaChild
    {
      get => docsXaChild ??= new();
      set => docsXaChild = value;
    }

    /// <summary>
    /// A value of DocsForeignOrder.
    /// </summary>
    [JsonPropertyName("docsForeignOrder")]
    public Common DocsForeignOrder
    {
      get => docsForeignOrder ??= new();
      set => docsForeignOrder = value;
    }

    private Common icDocsCreated;
    private Common childRead;
    private Common childUpdated;
    private Common childError;
    private Common docsRead;
    private Common ksDocsCreated;
    private Common docsError;
    private Common docsPrevPrints;
    private Common docsClosedCases;
    private Common docsMissingAr;
    private Common docsXaChild;
    private Common docsForeignOrder;
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
