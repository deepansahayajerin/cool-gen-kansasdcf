// Program: LE_B589_CLOSE, ID: 1902493401, model: 746.
// Short name: SWE03747
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B589_CLOSE.
/// </summary>
[Serializable]
public partial class LeB589Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B589_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB589Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB589Close.
  /// </summary>
  public LeB589Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************
    // CLOSE INPUT DHR FILE
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseLeB589ReadFile();

    if (!Equal(local.External.TextReturnCode, "00"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "ERROR CLOSING PORTAL EIWO RESULTS INPUT FILE";
      UseCabErrorReport3();
    }

    // **********************************************************
    // WRITE OUTPUT CONTROL REPORT AND CLOSE
    // **********************************************************
    local.Subscript.Count = 1;

    do
    {
      switch(local.Subscript.Count)
      {
        case 1:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "RECORDS READ" + "                          " +
            NumberToString(import.RecordsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "";

          break;
        case 3:
          local.EabReportSend.RptDetail = "NO ERROR RECORDS" + "                       " +
            NumberToString(import.NoErrRecords.Count, 15);

          break;
        case 4:
          local.EabFileHandling.Action = "CLOSE";

          break;
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 4);

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport01();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered closing IV-D error file.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport02();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered closing non IV-D error file.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.NumberOfColHeadings = source.NumberOfColHeadings;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
    target.ColHeading1 = source.ColHeading1;
    target.ColHeading2 = source.ColHeading2;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport02()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
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

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseLeB589ReadFile()
  {
    var useImport = new LeB589ReadFile.Import();
    var useExport = new LeB589ReadFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.External.Assign(local.External);

    Call(LeB589ReadFile.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
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
    /// A value of NoErrRecords.
    /// </summary>
    [JsonPropertyName("noErrRecords")]
    public Common NoErrRecords
    {
      get => noErrRecords ??= new();
      set => noErrRecords = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    private Common noErrRecords;
    private Common recordsRead;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    /// <summary>
    /// A value of TotalProcessed.
    /// </summary>
    [JsonPropertyName("totalProcessed")]
    public Common TotalProcessed
    {
      get => totalProcessed ??= new();
      set => totalProcessed = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
    }

    private External external;
    private CsePersonsWorkSet csePersonsWorkSet;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common subscript;
    private Common totalProcessed;
    private EabReportSend open;
  }
#endregion
}
