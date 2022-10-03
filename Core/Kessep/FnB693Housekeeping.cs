// Program: FN_B693_HOUSEKEEPING, ID: 374400745, model: 746.
// Short name: SWE00113
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B693_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB693Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B693_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB693Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB693Housekeeping.
  /// </summary>
  public FnB693Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.CurrentRun.Date = Now().Date;
    local.NeededToOpen.ProcessDate = Now().Date;
    local.NeededToOpen.ProgramName = "SWEFB693";
    local.EabFileHandling.Action = "OPEN";

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.DateWorkArea.TextDate = NumberToString(DateToInt(Now().Date), 8, 8);
    local.NeededToWrite.RptDetail =
      "Run Date. . . . . . . . . . . . . . . . . . : " + Substring
      (local.DateWorkArea.TextDate, DateWorkArea.TextDate_MaxLength, 5, 2) + "-"
      + Substring
      (local.DateWorkArea.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2) + "-"
      + Substring
      (local.DateWorkArea.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT FILE
    // **********************************************************
    local.KpcExternalParms.Parm1 = "OF";
    UseFnExtWriteEmployerFile();

    if (!IsEmpty(local.KpcExternalParms.Parm1))
    {
      ExitState = "FN0000_EMP_FILE_OPEN_ERROR_A";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveKpcExternalParms(KpcExternalParms source,
    KpcExternalParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnExtWriteEmployerFile()
  {
    var useImport = new FnExtWriteEmployerFile.Import();
    var useExport = new FnExtWriteEmployerFile.Export();

    MoveKpcExternalParms(local.KpcExternalParms, useImport.KpcExternalParms);
    MoveKpcExternalParms(local.KpcExternalParms, useExport.KpcExternalParms);

    Call(FnExtWriteEmployerFile.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.KpcExternalParms);
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CurrentRun.
    /// </summary>
    [JsonPropertyName("currentRun")]
    public DateWorkArea CurrentRun
    {
      get => currentRun ??= new();
      set => currentRun = value;
    }

    private DateWorkArea currentRun;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of KpcExternalParms.
    /// </summary>
    [JsonPropertyName("kpcExternalParms")]
    public KpcExternalParms KpcExternalParms
    {
      get => kpcExternalParms ??= new();
      set => kpcExternalParms = value;
    }

    private EabReportSend neededToOpen;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private DateWorkArea dateWorkArea;
    private KpcExternalParms kpcExternalParms;
  }
#endregion
}
