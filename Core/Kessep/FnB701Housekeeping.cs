// Program: FN_B701_HOUSEKEEPING, ID: 945110156, model: 746.
// Short name: SWE03672
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B701_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB701Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B701_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB701Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB701Housekeeping.
  /// </summary>
  public FnB701Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.ProgramProcessingInfo.Name = global.UserId;

    // -- Retrieve the PPI record
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -- Retrieve the checkpoint/restart info
    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -- Open the error report
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -- Open the control report
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.External.FileInstruction = "OPEN";

    // -- Open the external file
    UseFnEabB701ReadErrorFile();

    if (!Equal(local.External.TextReturnCode, "00"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -- Set default parameter values.  If a value is specified on the PPI 
    // record it will overwrite these values.
    // -- Validate the organization hierarchy type
    // -- The first day of the reporting period is the 1st day of the month 
    // prior to the month of the processing date.
    local.FirstDayOfRptMonth.Date =
      AddMonths(AddDays(
        export.ProgramProcessingInfo.ProcessDate,
      -(Day(export.ProgramProcessingInfo.ProcessDate) - 1)), -1);

    // -- Format report header.  Format <month name>/<year>
    switch(Month(local.FirstDayOfRptMonth.Date))
    {
      case 1:
        local.DateConversion.Text12 = "JANUARY";

        break;
      case 2:
        local.DateConversion.Text12 = "FEBUARY";

        break;
      case 3:
        local.DateConversion.Text12 = "MARCH";

        break;
      case 4:
        local.DateConversion.Text12 = "APRIL";

        break;
      case 5:
        local.DateConversion.Text12 = "MAY";

        break;
      case 6:
        local.DateConversion.Text12 = "JUNE";

        break;
      case 7:
        local.DateConversion.Text12 = "JULY";

        break;
      case 8:
        local.DateConversion.Text12 = "AUGUST";

        break;
      case 9:
        local.DateConversion.Text12 = "SEPTEMBER";

        break;
      case 10:
        local.DateConversion.Text12 = "OCTOBER";

        break;
      case 11:
        local.DateConversion.Text12 = "NOVEMBER";

        break;
      case 12:
        local.DateConversion.Text12 = "DECEMBER";

        break;
      default:
        break;
    }

    export.DateHeading.RptDetail =
      "                                        " + TrimEnd
      (local.DateConversion.Text12) + "/" + NumberToString
      (Year(local.FirstDayOfRptMonth.Date), 12, 4);

    // -- OPEN the four output reports
    local.EabFileHandling.Action = "OPEN";
    local.Open.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.Open.BlankLineAfterHeading = "N";
    local.Open.ProgramName = "SWEFB608";
    local.Open.RptHeading1 = "                         STATE OF KANSAS";
    local.Open.RptHeading2 = "DEPARTMENT FOR CHILDREN AND FAMILIES";
    local.Open.RunDate = Now().Date;
    local.Open.RunTime = Time(Now());

    // -- Call external to OPEN the collection officer report
    local.Open.RptHeading3 =
      "                  Attorney/Contractor Collection Error Report";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail =
        "Error encountered opening collection officer report.";
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    MoveEabReportSend2(local.Open, export.EabReportSend);
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = export.DateHeading.RptDetail;
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail =
        "Error encountered opening collection officer report.";
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.NumberOfColHeadings = source.NumberOfColHeadings;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RunDate = source.RunDate;
    target.RunTime = source.RunTime;
    target.RptHeading1 = source.RptHeading1;
    target.RptHeading2 = source.RptHeading2;
    target.RptHeading3 = source.RptHeading3;
  }

  private static void MoveEabReportSend3(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    MoveEabReportSend1(local.Open, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    MoveEabReportSend1(local.Open, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend3(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend3(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnEabB701ReadErrorFile()
  {
    var useImport = new FnEabB701ReadErrorFile.Import();
    var useExport = new FnEabB701ReadErrorFile.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useExport.External.Assign(local.External);

    Call(FnEabB701ReadErrorFile.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of DateHeading.
    /// </summary>
    [JsonPropertyName("dateHeading")]
    public EabReportSend DateHeading
    {
      get => dateHeading ??= new();
      set => dateHeading = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabReportSend eabReportSend;
    private EabReportSend dateHeading;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of FirstDayCurrentFy.
    /// </summary>
    [JsonPropertyName("firstDayCurrentFy")]
    public DateWorkArea FirstDayCurrentFy
    {
      get => firstDayCurrentFy ??= new();
      set => firstDayCurrentFy = value;
    }

    /// <summary>
    /// A value of FirstDayOfRptMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfRptMonth")]
    public DateWorkArea FirstDayOfRptMonth
    {
      get => firstDayOfRptMonth ??= new();
      set => firstDayOfRptMonth = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of RegionPart1.
    /// </summary>
    [JsonPropertyName("regionPart1")]
    public WorkArea RegionPart1
    {
      get => regionPart1 ??= new();
      set => regionPart1 = value;
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
    /// A value of DateConversion.
    /// </summary>
    [JsonPropertyName("dateConversion")]
    public WorkArea DateConversion
    {
      get => dateConversion ??= new();
      set => dateConversion = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Common Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of FieldNumber.
    /// </summary>
    [JsonPropertyName("fieldNumber")]
    public Common FieldNumber
    {
      get => fieldNumber ??= new();
      set => fieldNumber = value;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    private External external;
    private Common returnCode;
    private DateWorkArea dateWorkArea;
    private DateWorkArea firstDayCurrentFy;
    private DateWorkArea firstDayOfRptMonth;
    private Common common;
    private WorkArea regionPart1;
    private EabReportSend open;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private WorkArea dateConversion;
    private Common start;
    private Common current;
    private Common currentPosition;
    private Common fieldNumber;
    private TextWorkArea postion;
    private WorkArea workArea;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
  }
#endregion
}
