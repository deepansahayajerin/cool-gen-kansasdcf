// Program: FN_B736_BATCH_INITIALIZATION, ID: 1902573551, model: 746.
// Short name: SWE03761
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B736_BATCH_INITIALIZATION.
/// </summary>
[Serializable]
public partial class FnB736BatchInitialization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B736_BATCH_INITIALIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB736BatchInitialization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB736BatchInitialization.
  /// </summary>
  public FnB736BatchInitialization(IContext context, Import import,
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
    // ---------------------------------------------
    // 09/2016 - DDupree - WR55054
    // Initial code.
    // ---------------------------------------------
    // ---------------------------------------------------
    // Parameter List
    // Test Run - 1
    // Display Ind - 2
    // Run # - 4-5
    // Reporting period (for testing)
    // Start Date - 21-30
    // End date - 31-40
    // -----------------------------------------------------
    // ***** Get the run parameters for this program.
    export.ProgramProcessingInfo.Name = import.ProgramProcessingInfo.Name;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    export.TestRunInd.Flag =
      Substring(export.ProgramProcessingInfo.ParameterList, 1, 1);
    export.DisplayInd.Flag =
      Substring(export.ProgramProcessingInfo.ParameterList, 2, 1);
    export.StatsReport.FirstRunNumber =
      (int?)StringToNumber(Substring(
        export.ProgramProcessingInfo.ParameterList, 4, 2));
    export.From.OfficeId =
      (int?)StringToNumber(Substring(
        export.ProgramProcessingInfo.ParameterList, 12, 4));
    export.To.OfficeId =
      (int?)StringToNumber(Substring(
        export.ProgramProcessingInfo.ParameterList, 16, 4));

    if (AsChar(export.TestRunInd.Flag) == 'Y')
    {
      export.ReportStartDate.Date =
        StringToDate(Substring(
          export.ProgramProcessingInfo.ParameterList, 21, 10));
      export.ReportEndDate.Date =
        StringToDate(Substring(
          export.ProgramProcessingInfo.ParameterList, 31, 10));
      local.FirstDayOfRptMonth.Date = export.ReportStartDate.Date;
    }

    if (Equal(export.ReportStartDate.Date, local.Null1.Date))
    {
      export.ReportStartDate.Date =
        AddMonths(IntToDate(
          Year(export.ProgramProcessingInfo.ProcessDate) * 10000 + Month
        (export.ProgramProcessingInfo.ProcessDate) * 100 + 1), -1);
      export.ReportEndDate.Date =
        AddDays(AddMonths(export.ReportStartDate.Date, 1), -1);

      // -- The first day of the reporting period is the 1st day of the month 
      // prior to the month of the processing date.
      local.FirstDayOfRptMonth.Date =
        AddMonths(AddDays(
          export.ProgramProcessingInfo.ProcessDate,
        -(Day(export.ProgramProcessingInfo.ProcessDate) - 1)), -1);
    }

    // -- The first day of the reporting period is the 1st day of the month 
    // prior to the month of the processing date.
    // -- The report month is the year/month of the reporting period in a YYYYMM
    // format.
    export.StatsReport.YearMonth = Year(export.ReportStartDate.Date) * 100 + Month
      (export.ReportStartDate.Date);

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

    UseFnBuildTimestampFrmDateTime1();

    // ---------------------------------------------------------------------
    // Set FY start and end timestamps to 7am Oct 1.
    // Eg. FY2001, start tmst - 2000-10-01-07.00.00 and
    // end tmst - 2001-10-01-07.00.00
    // --------------------------------------------------------------------
    export.ReportStartDate.Timestamp =
      AddHours(export.ReportStartDate.Timestamp, 7);
    UseFnBuildTimestampFrmDateTime2();
    export.ReportEndDate.Timestamp =
      AddHours(AddDays(export.ReportEndDate.Timestamp, 1), 7);
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport3();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // WRITE DISPLAY STATEMENTS.
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      Substring(
        "RUN PARAMETERS....................................................................",
      1, 50);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "Year Month........................................................", 1,
      30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (DateToInt(export.ReportStartDate.Date), 8, 6);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "Run Number......................................................", 1,
      30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + Substring
      (export.ProgramProcessingInfo.ParameterList, 4, 2);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring("Date Range.................................................",
      1, 30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (DateToInt(export.ReportStartDate.Date), 8, 8) + "-";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (DateToInt(export.ReportEndDate.Date), 8, 8) + " ";
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // Trap Installation errors.
    // **********************************************************
    if (!Equal(global.UserId, export.ProgramProcessingInfo.Name))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Severe Error:  User_ID should be set to " + import
        .ProgramProcessingInfo.Name + " instead of " + global.UserId + ".  This is usually due to an error in the generation/installation.";
        
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // **********************************************************
    // Get DB2 commit frequency counts.
    // **********************************************************
    export.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Fatal error occurred, must abort.  " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
    }

    local.Open.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.Open.BlankLineAfterHeading = "Y";
    local.Open.ProgramName = "SWEFB736";
    local.Open.NumberOfColHeadings = 0;
    local.Open.RptHeading1 = "                         STATE OF KANSAS";
    local.Open.RptHeading2 = "DEPARTMENT FOR CHILDREN AND FAMILY SERVICES";
    local.Open.RunDate = Now().Date;
    local.Open.RunTime = Time(Now());
    local.EabFileHandling.Action = "OPEN";

    // -- Call external to OPEN the statewide report
    local.Open.RptHeading3 =
      "               Monthly Statistical Report By State";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening statewide report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail = "                                   " + TrimEnd
      (local.DateConversion.Text12) + "/" + NumberToString
      (Year(local.FirstDayOfRptMonth.Date), 12, 4);
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing statewide report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail = "";
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing statewide report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail = "";
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing statewide report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail =
      "      TOTALS     OPEN     XA     NA W/ST  NA W/O    OPEN     OPEN     OPEN      NA      OPEN     OPEN    INT-ST    NAI    AFI/FCI";
      
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing statewide report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail =
      "                 TAF     DEBT     DEBT   ST DEBT     FC       NF       NC      ONLY     NAI    AFI/FCI  KS DEBT";
      
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing statewide report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail =
      "------------- -------- -------- -------- -------- -------- -------- -------- -------- -------- -------- -------- -------- --------";
      
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing statewide report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Write.RptDetail = "Open Cases";
    local.EabFileHandling.Action = "WRITE";
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing statewide report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.Open);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.Open);
    useImport.NeededToWrite.RptDetail = local.Write.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
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
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnBuildTimestampFrmDateTime1()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = export.ReportStartDate.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, export.ReportStartDate);
  }

  private void UseFnBuildTimestampFrmDateTime2()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = export.ReportEndDate.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, export.ReportEndDate);
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
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A OpenAttributesGroup group.</summary>
    [Serializable]
    public class OpenAttributesGroup
    {
      /// <summary>
      /// A value of GrExportOpen.
      /// </summary>
      [JsonPropertyName("grExportOpen")]
      public EabReportSend GrExportOpen
      {
        get => grExportOpen ??= new();
        set => grExportOpen = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private EabReportSend grExportOpen;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public StatsReport To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public StatsReport From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of StatsReport.
    /// </summary>
    [JsonPropertyName("statsReport")]
    public StatsReport StatsReport
    {
      get => statsReport ??= new();
      set => statsReport = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of TestRunInd.
    /// </summary>
    [JsonPropertyName("testRunInd")]
    public Common TestRunInd
    {
      get => testRunInd ??= new();
      set => testRunInd = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// Gets a value of OpenAttributes.
    /// </summary>
    [JsonIgnore]
    public Array<OpenAttributesGroup> OpenAttributes => openAttributes ??= new(
      OpenAttributesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of OpenAttributes for json serialization.
    /// </summary>
    [JsonPropertyName("openAttributes")]
    [Computed]
    public IList<OpenAttributesGroup> OpenAttributes_Json
    {
      get => openAttributes;
      set => OpenAttributes.Assign(value);
    }

    /// <summary>
    /// A value of DateHeading.
    /// </summary>
    [JsonPropertyName("dateHeading")]
    public WorkArea DateHeading
    {
      get => dateHeading ??= new();
      set => dateHeading = value;
    }

    private StatsReport to;
    private StatsReport from;
    private StatsReport statsReport;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common testRunInd;
    private Common displayInd;
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
    private Array<OpenAttributesGroup> openAttributes;
    private WorkArea dateHeading;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public Ocse157Verification ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
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
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of ColHeading1.
    /// </summary>
    [JsonPropertyName("colHeading1")]
    public WorkArea ColHeading1
    {
      get => colHeading1 ??= new();
      set => colHeading1 = value;
    }

    /// <summary>
    /// A value of ColHeading2.
    /// </summary>
    [JsonPropertyName("colHeading2")]
    public WorkArea ColHeading2
    {
      get => colHeading2 ??= new();
      set => colHeading2 = value;
    }

    /// <summary>
    /// A value of ColHeading3.
    /// </summary>
    [JsonPropertyName("colHeading3")]
    public WorkArea ColHeading3
    {
      get => colHeading3 ??= new();
      set => colHeading3 = value;
    }

    /// <summary>
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public EabReportSend Write
    {
      get => write ??= new();
      set => write = value;
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
    /// A value of DateConversion.
    /// </summary>
    [JsonPropertyName("dateConversion")]
    public WorkArea DateConversion
    {
      get => dateConversion ??= new();
      set => dateConversion = value;
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

    private DateWorkArea null1;
    private Ocse157Verification forCreate;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private EabFileHandling status;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend open;
    private Common common;
    private WorkArea colHeading1;
    private WorkArea colHeading2;
    private WorkArea colHeading3;
    private EabReportSend write;
    private WorkArea workArea;
    private WorkArea dateConversion;
    private DateWorkArea firstDayOfRptMonth;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
