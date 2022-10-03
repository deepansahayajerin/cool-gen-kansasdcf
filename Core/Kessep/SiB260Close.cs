// Program: SI_B260_CLOSE, ID: 371045850, model: 746.
// Short name: SWE02558
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B260_CLOSE.
/// </summary>
[Serializable]
public partial class SiB260Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B260_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB260Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB260Close.
  /// </summary>
  public SiB260Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************
    // *Write out Control Report summary totals.
    // ****************************************************
    local.ControlLines.Count = 1;

    do
    {
      local.EabFileHandling.Action = "WRITE";

      switch(local.ControlLines.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "NUMBER OF RECORDS READ FROM INPUT FILE          " + NumberToString
            (import.RecordsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "NUMBER OF RECORDS SKIPPED                       " + NumberToString
            (import.RecordsSkipped.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "NUMBER OF UNEMPLOYMENT INCOME RECORDS READ      " + NumberToString
            (import.UnemployIncomeRead.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail = "";

          break;
        case 5:
          local.EabReportSend.RptDetail = "";

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "NUMBER OF PERSONS PREVIOUSLY ON UI              " + NumberToString
            (import.PersonsPreviouslyOnUi.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "NUMBER OF PERSONS DISCONTINUED RECEIVING UI     " + NumberToString
            (import.PersonsDiscontinued.Count, 15);

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "NUMBER OF PERSONS CONTINUED RECEIVING UI        " + NumberToString
            (import.PersonsContinued.Count, 15);

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "NUMBER OF PERSONS THAT JUST BEGAN RECEIVING UI  " + NumberToString
            (import.PersonsAddedToUi.Count, 15);

          break;
        case 10:
          local.EabReportSend.RptDetail =
            "NUMBER OF PERSONS NOW ON UI                     " + NumberToString
            (import.PersonsNowOnUi.Count, 15);

          break;
        case 11:
          local.PassArea.FileInstruction = "CLOSE";
          UseSiEabReceiveWageIncSource();
          ++local.ControlLines.Count;

          if (local.PassArea.NumericReturnCode != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "ERROR CLOSING WAGE - INCOME SOURCE FILE.";
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto AfterCycle;
          }

          continue;
        case 12:
          local.EabFileHandling.Action = "CLOSE";

          break;
        default:
          break;
      }

      ++local.ControlLines.Count;
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "ERROR WRITING CONTROL REPORT";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }
    }
    while(local.ControlLines.Count <= 12);

AfterCycle:

    // *****************************************************
    // * Close Error Report.  DDNAME=RPT99
    // *****************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private void UseCabControlReport()
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

  private void UseSiEabReceiveWageIncSource()
  {
    var useImport = new SiEabReceiveWageIncSource.Import();
    var useExport = new SiEabReceiveWageIncSource.Export();

    useImport.External.Assign(local.PassArea);
    useExport.External.Assign(local.PassArea);

    Call(SiEabReceiveWageIncSource.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
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
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of RecordsSkipped.
    /// </summary>
    [JsonPropertyName("recordsSkipped")]
    public Common RecordsSkipped
    {
      get => recordsSkipped ??= new();
      set => recordsSkipped = value;
    }

    /// <summary>
    /// A value of UnemployIncomeRead.
    /// </summary>
    [JsonPropertyName("unemployIncomeRead")]
    public Common UnemployIncomeRead
    {
      get => unemployIncomeRead ??= new();
      set => unemployIncomeRead = value;
    }

    /// <summary>
    /// A value of PersonsPreviouslyOnUi.
    /// </summary>
    [JsonPropertyName("personsPreviouslyOnUi")]
    public Common PersonsPreviouslyOnUi
    {
      get => personsPreviouslyOnUi ??= new();
      set => personsPreviouslyOnUi = value;
    }

    /// <summary>
    /// A value of PersonsDiscontinued.
    /// </summary>
    [JsonPropertyName("personsDiscontinued")]
    public Common PersonsDiscontinued
    {
      get => personsDiscontinued ??= new();
      set => personsDiscontinued = value;
    }

    /// <summary>
    /// A value of PersonsContinued.
    /// </summary>
    [JsonPropertyName("personsContinued")]
    public Common PersonsContinued
    {
      get => personsContinued ??= new();
      set => personsContinued = value;
    }

    /// <summary>
    /// A value of PersonsAddedToUi.
    /// </summary>
    [JsonPropertyName("personsAddedToUi")]
    public Common PersonsAddedToUi
    {
      get => personsAddedToUi ??= new();
      set => personsAddedToUi = value;
    }

    /// <summary>
    /// A value of PersonsNowOnUi.
    /// </summary>
    [JsonPropertyName("personsNowOnUi")]
    public Common PersonsNowOnUi
    {
      get => personsNowOnUi ??= new();
      set => personsNowOnUi = value;
    }

    private Common recordsRead;
    private Common recordsSkipped;
    private Common unemployIncomeRead;
    private Common personsPreviouslyOnUi;
    private Common personsDiscontinued;
    private Common personsContinued;
    private Common personsAddedToUi;
    private Common personsNowOnUi;
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
    /// A value of ControlLines.
    /// </summary>
    [JsonPropertyName("controlLines")]
    public Common ControlLines
    {
      get => controlLines ??= new();
      set => controlLines = value;
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

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    private Common controlLines;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External passArea;
  }
#endregion
}
