// Program: FN_B692_HOUSEKEEPING, ID: 374398437, model: 746.
// Short name: SWE00082
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B692_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB692Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B692_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB692Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB692Housekeeping.
  /// </summary>
  public FnB692Housekeeping(IContext context, Import import, Export export):
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
    export.ProgramProcessingInfo.Name = "SWEFB692";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF_AB";

      return;
    }

    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;

    // : Parameter List is as follows:
    // XYYYYMMDD
    // 1-1   Run in Test Mode (Y/N)  no longer used
    // 2-9   Last Date of Execution
    if (!IsEmpty(export.ProgramProcessingInfo.ParameterList))
    {
      export.LastRun.Date =
        IntToDate((int)StringToNumber(Substring(
          export.ProgramProcessingInfo.ParameterList, 2, 8)));
    }
    else
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
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

    local.DateWorkArea.TextDate =
      NumberToString(DateToInt(export.LastRun.Date), 8, 8);
    local.NeededToWrite.RptDetail =
      "Last Run Date . . . . . . . . . . . . . . : " + Substring
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

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
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

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
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
    /// A value of ZdelDateWorkArea.
    /// </summary>
    [JsonPropertyName("zdelDateWorkArea")]
    public DateWorkArea ZdelDateWorkArea
    {
      get => zdelDateWorkArea ??= new();
      set => zdelDateWorkArea = value;
    }

    /// <summary>
    /// A value of ZdelCommon.
    /// </summary>
    [JsonPropertyName("zdelCommon")]
    public Common ZdelCommon
    {
      get => zdelCommon ??= new();
      set => zdelCommon = value;
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

    private DateWorkArea zdelDateWorkArea;
    private Common zdelCommon;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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

    private EabReportSend eabReportSend;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private DateWorkArea dateWorkArea;
    private KpcExternalParms kpcExternalParms;
  }
#endregion
}
