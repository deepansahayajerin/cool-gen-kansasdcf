// Program: SP_B730_WRITE_TOTALS_AND_CLOSE, ID: 373509458, model: 746.
// Short name: SWE02813
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B730_WRITE_TOTALS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SpB730WriteTotalsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B730_WRITE_TOTALS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB730WriteTotalsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB730WriteTotalsAndClose.
  /// </summary>
  public SpB730WriteTotalsAndClose(IContext context, Import import,
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
    local.MaxControlTotal.Count = 4;

    // -------------------------------------------------------
    // WRITE OUTPUT CONTROL REPORT
    // -------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabConvertNumeric.SendNonSuppressPos = 1;
    local.Subscript.Count = 1;

    for(var limit = local.MaxControlTotal.Count; local.Subscript.Count <= limit
      ; ++local.Subscript.Count)
    {
      local.LabelColumn.Text40 = "";
      local.WorkArea.Text20 = "";
      local.EabConvertNumeric.SendAmount = "";
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal = "";

      switch(local.Subscript.Count)
      {
        case 1:
          local.LabelColumn.Text40 = "CSE PERSONS READ";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.CsePersonsRead.Count, 15);
          UseEabConvertNumeric1();
          local.WorkArea.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 2:
          local.LabelColumn.Text40 = "CSE PERSONS WITH NO ADDRESS";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.NoAddress.Count, 15);
          UseEabConvertNumeric1();
          local.WorkArea.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 3:
          local.LabelColumn.Text40 = "DOCUMENTS CREATED";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocCreates.Count, 15);
          UseEabConvertNumeric1();
          local.WorkArea.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        case 4:
          local.LabelColumn.Text40 = "DOCUMENTS ERRORS";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocErrors.Count, 15);
          UseEabConvertNumeric1();
          local.WorkArea.Text20 =
            local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

          break;
        default:
          goto AfterCycle;
      }

      local.EabReportSend.RptDetail = local.LabelColumn.Text40 + local
        .WorkArea.Text20;
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "Error writing to Control Report";
        UseCabErrorReport2();

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
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error closing Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // --------------------------------------------------------
    // CLOSE OUTPUT ERROR REPORT
    // --------------------------------------------------------
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error closing Error Report";
      UseCabErrorReport2();

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
    /// A value of CsePersonsRead.
    /// </summary>
    [JsonPropertyName("csePersonsRead")]
    public Common CsePersonsRead
    {
      get => csePersonsRead ??= new();
      set => csePersonsRead = value;
    }

    /// <summary>
    /// A value of NoAddress.
    /// </summary>
    [JsonPropertyName("noAddress")]
    public Common NoAddress
    {
      get => noAddress ??= new();
      set => noAddress = value;
    }

    /// <summary>
    /// A value of DocCreates.
    /// </summary>
    [JsonPropertyName("docCreates")]
    public Common DocCreates
    {
      get => docCreates ??= new();
      set => docCreates = value;
    }

    /// <summary>
    /// A value of DocErrors.
    /// </summary>
    [JsonPropertyName("docErrors")]
    public Common DocErrors
    {
      get => docErrors ??= new();
      set => docErrors = value;
    }

    private Common csePersonsRead;
    private Common noAddress;
    private Common docCreates;
    private Common docErrors;
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
    /// A value of MaxControlTotal.
    /// </summary>
    [JsonPropertyName("maxControlTotal")]
    public Common MaxControlTotal
    {
      get => maxControlTotal ??= new();
      set => maxControlTotal = value;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of LabelColumn.
    /// </summary>
    [JsonPropertyName("labelColumn")]
    public WorkArea LabelColumn
    {
      get => labelColumn ??= new();
      set => labelColumn = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
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

    private Common maxControlTotal;
    private EabFileHandling eabFileHandling;
    private EabConvertNumeric2 eabConvertNumeric;
    private WorkArea labelColumn;
    private WorkArea workArea;
    private Common subscript;
    private EabReportSend eabReportSend;
  }
#endregion
}
