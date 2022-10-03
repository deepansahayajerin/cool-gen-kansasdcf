// Program: FN_B664_BATCH_INITIALIZATION, ID: 371232780, model: 746.
// Short name: SWE02010
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B664_BATCH_INITIALIZATION.
/// </summary>
[Serializable]
public partial class FnB664BatchInitialization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B664_BATCH_INITIALIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB664BatchInitialization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB664BatchInitialization.
  /// </summary>
  public FnB664BatchInitialization(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 02/17/05  GVandy	PR233867	Initial Development.  New business rules for AR
    // statements.
    // -------------------------------------------------------------------------------------------------------------------------
    // 05/28/13  JHarden       CQ 35556        Change default to quarterly.
    // -------------------------------------------------------------------------------------------------------------------------
    // --  Read the PPI Record for SWEFB663.
    // --
    // --  NOTE THAT WE'RE USING THE SWEFB663 PPI RECORD NOT THE SWEFB664 PPI 
    // RECORD.
    // -------------------------------------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEFB663";
    UseReadProgramProcessingInfo();
    export.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;

    // CQ 35556 to change to quarterly
    local.DateWorkArea.Day = Day(local.ProgramProcessingInfo.ProcessDate);
    local.DateWorkArea.Month = Month(local.ProgramProcessingInfo.ProcessDate);
    local.DateWorkArea.Year = Year(local.ProgramProcessingInfo.ProcessDate);
    local.Day.Text2 = NumberToString(local.DateWorkArea.Day, 14, 2);
    local.Month.Text2 = NumberToString(local.DateWorkArea.Month, 14, 2);
    local.TextWorkArea.Text4 = local.Month.Text2 + local.Day.Text2;

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open the Error Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.  Had to 
      // open the error report before escaping to the PrAD.
      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_CONTROL_RPT";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open the Extract File.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseFnB664ReadExtractDataFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening extract file.  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open Report Files for the AR Statements.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseSpEabWriteDocument();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening report files.  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Extract the AR number and Reporting Period Start and End Dates from 
    // the SWEFB663 PPI record.
    // --
    // --  Position  1 - 10  AR cse person number     (format XXXXXXXXXX)
    // --  Position 11 - 20  Report period start date (format yyyy-mm-dd)
    // --  Position 21 - 30  Report period end date   (format yyyy-mm-dd)
    // --  Position 31       Create Events Flag       (values Y or N)
    // -------------------------------------------------------------------------------------------------------------------------
    local.Ar.Number =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 10);

    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 11, 20)))
    {
      // CQ 35556 To change to quarterly
      if (!Lt(local.TextWorkArea.Text4, "0401") && !
        Lt("0630", local.TextWorkArea.Text4))
      {
        export.ReportingPeriodStartingDateWorkArea.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-01-01");
          
        export.ReportingPeriodEndingDateWorkArea.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-03-31");
          
      }
      else if (!Lt(local.TextWorkArea.Text4, "0701") && !
        Lt("0930", local.TextWorkArea.Text4))
      {
        export.ReportingPeriodStartingDateWorkArea.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-04-01");
          
        export.ReportingPeriodEndingDateWorkArea.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-06-30");
          
      }
      else if (!Lt(local.TextWorkArea.Text4, "0901") && !
        Lt("1231", local.TextWorkArea.Text4))
      {
        export.ReportingPeriodStartingDateWorkArea.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-07-01");
          
        export.ReportingPeriodEndingDateWorkArea.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-09-30");
          
      }
      else if (!Lt(local.TextWorkArea.Text4, "0101") && !
        Lt("0331", local.TextWorkArea.Text4))
      {
        export.ReportingPeriodStartingDateWorkArea.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-10-01");
          
        export.ReportingPeriodEndingDateWorkArea.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-12-31");
          
        export.ReportingPeriodStartingDateWorkArea.Date =
          AddYears(export.ReportingPeriodStartingDateWorkArea.Date, -1);
        export.ReportingPeriodEndingDateWorkArea.Date =
          AddYears(export.ReportingPeriodEndingDateWorkArea.Date, -1);
      }
    }
    else
    {
      // -- Set the reporting period using info on the PPI record.
      export.ReportingPeriodStartingDateWorkArea.Date =
        StringToDate(
          Substring(local.ProgramProcessingInfo.ParameterList, 11, 10));
      export.ReportingPeriodEndingDateWorkArea.Date =
        StringToDate(
          Substring(local.ProgramProcessingInfo.ParameterList, 21, 10));
    }

    // -- Set the Reporting Period Starting Timestamp.
    export.ReportingPeriodStartingDateWorkArea.Timestamp =
      Add(local.Null1.Timestamp,
      Year(export.ReportingPeriodStartingDateWorkArea.Date),
      Month(export.ReportingPeriodStartingDateWorkArea.Date),
      Day(export.ReportingPeriodStartingDateWorkArea.Date));

    // -- Set the Reporting Period Ending Timestamp.
    export.ReportingPeriodEndingDateWorkArea.Timestamp =
      Add(local.Null1.Timestamp,
      Year(export.ReportingPeriodEndingDateWorkArea.Date),
      Month(export.ReportingPeriodEndingDateWorkArea.Date),
      Day(export.ReportingPeriodEndingDateWorkArea.Date));
    export.ReportingPeriodEndingDateWorkArea.Timestamp =
      AddMicroseconds(AddDays(
        export.ReportingPeriodEndingDateWorkArea.Timestamp, 1), -1);

    // -- Set the Create Events Flag.
    export.CreateEvents.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 31, 1);

    if (AsChar(export.CreateEvents.Flag) != 'N')
    {
      export.CreateEvents.Flag = "Y";
    }

    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Write the AR Number to the Control Report.
          // -------------------------------------------------------------------------------------------------------------------------
          if (IsEmpty(local.Ar.Number))
          {
            local.EabReportSend.RptDetail =
              "AR Person Number . . . . . . . . . .Not Specified";
          }
          else
          {
            local.EabReportSend.RptDetail =
              "AR Person Number . . . . . . . . . ." + local.Ar.Number;
          }

          break;
        case 2:
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Write the Reporting Period Start Date to the Control Report.
          // -------------------------------------------------------------------------------------------------------------------------
          local.EabReportSend.RptDetail =
            "Reporting Period Start Date. . . . .";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Month(export.ReportingPeriodStartingDateWorkArea.Date), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Day(export.ReportingPeriodStartingDateWorkArea.Date), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Year(export.ReportingPeriodStartingDateWorkArea.Date), 12, 4);

          break;
        case 3:
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Write the Reporting Period End Date to the Control Report.
          // -------------------------------------------------------------------------------------------------------------------------
          local.EabReportSend.RptDetail =
            "Reporting Period End Date. . . . . .";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Month(export.ReportingPeriodEndingDateWorkArea.Date), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Day(export.ReportingPeriodEndingDateWorkArea.Date), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Year(export.ReportingPeriodEndingDateWorkArea.Date), 12, 4);

          break;
        case 4:
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Write the Create Events Flag to the Control Report.
          // -------------------------------------------------------------------------------------------------------------------------
          local.EabReportSend.RptDetail =
            "Create AR Statement Event. . . . . ." + export.CreateEvents.Flag;

          break;
        case 5:
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Write the Blank Line to the Control Report.
          // -------------------------------------------------------------------------------------------------------------------------
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Determine if we are Restarting.
    // -------------------------------------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -------------------------------------------------------------------------------------------------------------------------
      // --  We are restarting.  Extract the last checkpoint info.
      // --  Characters 1-10 contain the last AR person number processed.
      // --
      // --  Note that we will duplicate AR statements for any ARs processed in 
      // the previous run after the last checkpoint was done.
      // -------------------------------------------------------------------------------------------------------------------------
      export.Restart.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);

      // -------------------------------------------------------------------------------------------------------------------------
      // --  Write the Restart info to the Control Report.
      // -------------------------------------------------------------------------------------------------------------------------
      local.EabReportSend.RptDetail = "Restarting at AR # " + export
        .Restart.Number;
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";
      }
    }
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
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

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
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB664ReadExtractDataFile()
  {
    var useImport = new FnB664ReadExtractDataFile.Import();
    var useExport = new FnB664ReadExtractDataFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB664ReadExtractDataFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSpEabWriteDocument()
  {
    var useImport = new SpEabWriteDocument.Import();
    var useExport = new SpEabWriteDocument.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SpEabWriteDocument.Execute, useImport, useExport);

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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CreateEvents.
    /// </summary>
    [JsonPropertyName("createEvents")]
    public Common CreateEvents
    {
      get => createEvents ??= new();
      set => createEvents = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEndingTextWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndingTextWorkArea")]
    public TextWorkArea ReportingPeriodEndingTextWorkArea
    {
      get => reportingPeriodEndingTextWorkArea ??= new();
      set => reportingPeriodEndingTextWorkArea = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStartingTextWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodStartingTextWorkArea")]
    public TextWorkArea ReportingPeriodStartingTextWorkArea
    {
      get => reportingPeriodStartingTextWorkArea ??= new();
      set => reportingPeriodStartingTextWorkArea = value;
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
    /// A value of ReportingPeriodEndingDateWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndingDateWorkArea")]
    public DateWorkArea ReportingPeriodEndingDateWorkArea
    {
      get => reportingPeriodEndingDateWorkArea ??= new();
      set => reportingPeriodEndingDateWorkArea = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStartingDateWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodStartingDateWorkArea")]
    public DateWorkArea ReportingPeriodStartingDateWorkArea
    {
      get => reportingPeriodStartingDateWorkArea ??= new();
      set => reportingPeriodStartingDateWorkArea = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    private Common createEvents;
    private TextWorkArea reportingPeriodEndingTextWorkArea;
    private TextWorkArea reportingPeriodStartingTextWorkArea;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea reportingPeriodEndingDateWorkArea;
    private DateWorkArea reportingPeriodStartingDateWorkArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson restart;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of Day.
    /// </summary>
    [JsonPropertyName("day")]
    public WorkArea Day
    {
      get => day ??= new();
      set => day = value;
    }

    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public WorkArea Month
    {
      get => month ??= new();
      set => month = value;
    }

    private Common common;
    private CsePerson ar;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea null1;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea dateWorkArea;
    private TextWorkArea textWorkArea;
    private WorkArea day;
    private WorkArea month;
  }
#endregion
}
