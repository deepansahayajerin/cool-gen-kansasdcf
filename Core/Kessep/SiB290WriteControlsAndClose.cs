// Program: SI_B290_WRITE_CONTROLS_AND_CLOSE, ID: 372812167, model: 746.
// Short name: SWE02476
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B290_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SiB290WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B290_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB290WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB290WriteControlsAndClose.
  /// </summary>
  public SiB290WriteControlsAndClose(IContext context, Import import,
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

    // -----------------------------------------------------------------
    // WRITE OUTPUT CONTROL REPORT
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.Subscript.Count = 1;

    for(var limit = local.MaxControlTotal.Count; local.Subscript.Count <= limit
      ; ++local.Subscript.Count)
    {
      local.Label.Text60 = "";

      switch(local.Subscript.Count)
      {
        case 1:
          local.Label.Text60 = "Number of Outgoing CSENet transactions read";
          local.Textnum.Text15 = NumberToString(import.Read.Count, 15);

          break;
        case 2:
          local.Label.Text60 = "Number of Outgoing CSENet transactions sent";
          local.Textnum.Text15 = NumberToString(import.Created.Count, 15);

          break;
        case 3:
          local.Label.Text60 =
            "Number of Outgoing CSENet transactns not sent due to errors";
          local.Textnum.Text15 = NumberToString(import.Erred.Count, 15);

          break;
        default:
          goto AfterCycle;
      }

      local.EabReportSend.RptDetail = local.Label.Text60 + local.Textnum.Text15;
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "Error encountered writing to Control Report";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

AfterCycle:

    // -----------------------------------------------------------------
    // CLOSE CONTROL REPORT
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();

    // -----------------------------------------------------------------
    // CLOSE ERROR REPORT
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport();
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public Common Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of Created.
    /// </summary>
    [JsonPropertyName("created")]
    public Common Created
    {
      get => created ??= new();
      set => created = value;
    }

    /// <summary>
    /// A value of Erred.
    /// </summary>
    [JsonPropertyName("erred")]
    public Common Erred
    {
      get => erred ??= new();
      set => erred = value;
    }

    private Common read;
    private Common created;
    private Common erred;
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
    /// A value of Textnum.
    /// </summary>
    [JsonPropertyName("textnum")]
    public WorkArea Textnum
    {
      get => textnum ??= new();
      set => textnum = value;
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
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

    private WorkArea textnum;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private WorkArea label;
    private Common subscript;
    private Common maxControlTotal;
  }
#endregion
}
