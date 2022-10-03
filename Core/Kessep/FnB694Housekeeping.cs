// Program: FN_B694_HOUSEKEEPING, ID: 374413124, model: 746.
// Short name: SWE00140
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B694_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB694Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B694_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB694Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB694Housekeeping.
  /// </summary>
  public FnB694Housekeeping(IContext context, Import import, Export export):
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
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // **********************************************************
    export.ProgramProcessingInfo.Name = "SWEFB694";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ZD_PROGRAM_PROCESSING_INFO_NF_AB";

      return;
    }

    local.NeededToOpen.ProgramName = export.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    export.CurrentRun.Date = Now().Date;

    // : Parameter List is as follows:
    // XYYYYMMDD
    // 1-1   Run in Test Mode (Y/N)
    // 2-9   Last Date of Execution
    if (IsEmpty(export.ProgramProcessingInfo.ParameterList))
    {
      export.RunInTestMode.Flag = "N";
      export.LastRun.Date = new DateTime(1900, 1, 1);
    }
    else
    {
      export.RunInTestMode.Flag =
        Substring(export.ProgramProcessingInfo.ParameterList, 1, 1);

      if (IsEmpty(export.RunInTestMode.Flag))
      {
        export.RunInTestMode.Flag = "N";
      }

      if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 2, 8)))
      {
        export.LastRun.Date =
          IntToDate((int)StringToNumber(Substring(
            export.ProgramProcessingInfo.ParameterList, 2, 8)));
      }
      else
      {
        export.LastRun.Date = new DateTime(1900, 1, 1);
      }
    }

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
    local.NeededToWrite.RptDetail = "PARAMETERS:";
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

    local.NeededToWrite.RptDetail = "Run In Test Mode. . . . . . . . . . : " + export
      .RunInTestMode.Flag;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.DateWorkArea.TextDate =
      NumberToString(DateToInt(export.CurrentRun.Date), 8, 8);
    local.NeededToWrite.RptDetail = "Run Date. . . . . . . . . . . . . . : " + Substring
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

    local.DateWorkArea.TextDate =
      NumberToString(DateToInt(export.LastRun.Date), 8, 8);
    local.NeededToWrite.RptDetail = "Last Run Date . . . . . . . . . . . : " + Substring
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
    UseFnExtWriteInterfaceFile();

    switch(TrimEnd(local.KpcExternalParms.Parm1))
    {
      case "E1":
        ExitState = "FN0000_KPC_FILE_OPEN_ERROR_A";

        break;
      case "E2":
        ExitState = "FN0000_KPCFILE_2_OPEN_ERROR_A";

        break;
      case "E3":
        ExitState = "FN0000_KPCFILE_3_OPEN_ERROR_A";

        break;
      case "E4":
        ExitState = "FN0000_KPCFILE_4_OPEN_ERROR_A";

        break;
      case "E5":
        ExitState = "FN0000_KPCFILE_5_OPEN_ERROR_A";

        break;
      case "E6":
        ExitState = "FN0000_KPCFILE_6_OPEN_ERROR_A";

        break;
      case "E7":
        ExitState = "FN0000_KPCFILE_7_OPEN_ERROR_A";

        break;
      case "E8":
        ExitState = "FN0000_KPCFILE_8_OPEN_ERROR_A";

        break;
      case "E9":
        ExitState = "FN0000_KPCFILE_9_OPEN_ERROR_A";

        break;
      case "E0":
        ExitState = "FN0000_KPCFILE_OPEN_ERROR_A";

        break;
      default:
        break;
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

  private void UseFnExtWriteInterfaceFile()
  {
    var useImport = new FnExtWriteInterfaceFile.Import();
    var useExport = new FnExtWriteInterfaceFile.Export();

    MoveKpcExternalParms(local.KpcExternalParms, useImport.KpcExternalParms);
    MoveKpcExternalParms(local.KpcExternalParms, useExport.KpcExternalParms);

    Call(FnExtWriteInterfaceFile.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.KpcExternalParms);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = export.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
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

    /// <summary>
    /// A value of RunInTestMode.
    /// </summary>
    [JsonPropertyName("runInTestMode")]
    public Common RunInTestMode
    {
      get => runInTestMode ??= new();
      set => runInTestMode = value;
    }

    /// <summary>
    /// A value of LastRun.
    /// </summary>
    [JsonPropertyName("lastRun")]
    public DateWorkArea LastRun
    {
      get => lastRun ??= new();
      set => lastRun = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private DateWorkArea currentRun;
    private Common runInTestMode;
    private DateWorkArea lastRun;
    private ProgramProcessingInfo programProcessingInfo;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private DateWorkArea dateWorkArea;
    private KpcExternalParms kpcExternalParms;
  }
#endregion
}
