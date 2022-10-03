// Program: SP_B704_WRITE_CONTROLS_AND_CLOSE, ID: 370987364, model: 746.
// Short name: SWE02502
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B704_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class SpB704WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B704_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB704WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB704WriteControlsAndClose.
  /// </summary>
  public SpB704WriteControlsAndClose(IContext context, Import import,
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
          local.Label.Text40 = "CASES READ";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.CasesRead.Count, 15);

          break;
        case 2:
          local.Label.Text40 = "     MISSING AR/AR IS ORG";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.MissingAr.Count, 15);

          break;
        case 3:
          local.Label.Text40 = "     NO ACTIVE COMPLIANCE PROGRAMS";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.NoActiveCompliance.Count, 15);

          break;
        case 4:
          local.Label.Text40 = "     AE CASE CLOSED 'DC'";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ClosedDc.Count, 15);

          break;
        case 5:
          local.Label.Text40 = "     CSE CASE CLOSED 'MO'";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ClosedMo.Count, 15);

          break;
        case 6:
          local.Label.Text40 = "     ACTIVE AF PROGRAM OR NO AF PROGRAMS";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.ActiveAfProg.Count, 15);

          break;
        case 7:
          local.Label.Text40 = "DOCUMENT TRIGGERS CREATED";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.DocsCreated.Count, 15);

          break;
        case 8:
          local.Label.Text40 = "     ADC ROLLOVER";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.CreatedAdcrollo.Count, 15);

          break;
        case 9:
          local.Label.Text40 = "     PA ROLLOVER";
          local.EabConvertNumeric.SendAmount =
            NumberToString(import.CreatedParollo.Count, 15);

          break;
        case 10:
          local.Label.Text40 = "DOCUMENT TRIGGER ERRORS";
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
    /// A value of CasesRead.
    /// </summary>
    [JsonPropertyName("casesRead")]
    public Common CasesRead
    {
      get => casesRead ??= new();
      set => casesRead = value;
    }

    /// <summary>
    /// A value of MissingAr.
    /// </summary>
    [JsonPropertyName("missingAr")]
    public Common MissingAr
    {
      get => missingAr ??= new();
      set => missingAr = value;
    }

    /// <summary>
    /// A value of NoActiveCompliance.
    /// </summary>
    [JsonPropertyName("noActiveCompliance")]
    public Common NoActiveCompliance
    {
      get => noActiveCompliance ??= new();
      set => noActiveCompliance = value;
    }

    /// <summary>
    /// A value of ClosedDc.
    /// </summary>
    [JsonPropertyName("closedDc")]
    public Common ClosedDc
    {
      get => closedDc ??= new();
      set => closedDc = value;
    }

    /// <summary>
    /// A value of ClosedMo.
    /// </summary>
    [JsonPropertyName("closedMo")]
    public Common ClosedMo
    {
      get => closedMo ??= new();
      set => closedMo = value;
    }

    /// <summary>
    /// A value of ActiveAfProg.
    /// </summary>
    [JsonPropertyName("activeAfProg")]
    public Common ActiveAfProg
    {
      get => activeAfProg ??= new();
      set => activeAfProg = value;
    }

    /// <summary>
    /// A value of DocsCreated.
    /// </summary>
    [JsonPropertyName("docsCreated")]
    public Common DocsCreated
    {
      get => docsCreated ??= new();
      set => docsCreated = value;
    }

    /// <summary>
    /// A value of CreatedAdcrollo.
    /// </summary>
    [JsonPropertyName("createdAdcrollo")]
    public Common CreatedAdcrollo
    {
      get => createdAdcrollo ??= new();
      set => createdAdcrollo = value;
    }

    /// <summary>
    /// A value of CreatedParollo.
    /// </summary>
    [JsonPropertyName("createdParollo")]
    public Common CreatedParollo
    {
      get => createdParollo ??= new();
      set => createdParollo = value;
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

    private Common casesRead;
    private Common missingAr;
    private Common noActiveCompliance;
    private Common closedDc;
    private Common closedMo;
    private Common activeAfProg;
    private Common docsCreated;
    private Common createdAdcrollo;
    private Common createdParollo;
    private Common docsError;
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
