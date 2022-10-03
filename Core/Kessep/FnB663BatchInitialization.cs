// Program: FN_B663_BATCH_INITIALIZATION, ID: 371231374, model: 746.
// Short name: SWE02008
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B663_BATCH_INITIALIZATION.
/// </summary>
[Serializable]
public partial class FnB663BatchInitialization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B663_BATCH_INITIALIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB663BatchInitialization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB663BatchInitialization.
  /// </summary>
  public FnB663BatchInitialization(IContext context, Import import,
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
    // --  Read the PPI Record.
    // -------------------------------------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

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
    UseFnB663WriteExtractDataFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "File Open Return Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Extract the AR number and Reporting Period Start and End Dates from 
    // the PPI record.
    // --
    // --  Position  1 - 10  AR cse person number     (format XXXXXXXXXX)
    // --  Position 11 - 20  Report period start date (format yyyy-mm-dd)
    // --  Position 21 - 30  Report period end date   (format yyyy-mm-dd)
    // --  Position 31       Create Events Flag       (values Y or N)
    // -------------------------------------------------------------------------------------------------------------------------
    export.Ar.Number =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 10);

    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 11, 20)))
    {
      // CQ 35556 To change to quarterly
      if (!Lt(local.TextWorkArea.Text4, "0401") && !
        Lt("0630", local.TextWorkArea.Text4))
      {
        export.ReportingPeriodStarting.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-01-01");
          
        export.ReportingPeriodEnding.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-03-31");
          
      }
      else if (!Lt(local.TextWorkArea.Text4, "0701") && !
        Lt("0930", local.TextWorkArea.Text4))
      {
        export.ReportingPeriodStarting.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-04-01");
          
        export.ReportingPeriodEnding.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-06-30");
          
      }
      else if (!Lt(local.TextWorkArea.Text4, "0901") && !
        Lt("1231", local.TextWorkArea.Text4))
      {
        export.ReportingPeriodStarting.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-07-01");
          
        export.ReportingPeriodEnding.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-09-30");
          
      }
      else if (!Lt(local.TextWorkArea.Text4, "0101") && !
        Lt("0331", local.TextWorkArea.Text4))
      {
        export.ReportingPeriodStarting.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-10-01");
          
        export.ReportingPeriodEnding.Date =
          StringToDate(NumberToString(local.DateWorkArea.Year, 12, 4) + "-12-31");
          
        export.ReportingPeriodStarting.Date =
          AddYears(export.ReportingPeriodStarting.Date, -1);
        export.ReportingPeriodEnding.Date =
          AddYears(export.ReportingPeriodEnding.Date, -1);
      }
    }
    else
    {
      // -- Set the reporting period using info on the PPI record.
      export.ReportingPeriodStarting.Date =
        StringToDate(
          Substring(local.ProgramProcessingInfo.ParameterList, 11, 10));
      export.ReportingPeriodEnding.Date =
        StringToDate(
          Substring(local.ProgramProcessingInfo.ParameterList, 21, 10));
    }

    // -- Set the Reporting Period Starting Timestamp.
    export.ReportingPeriodStarting.Timestamp =
      Add(local.Null1.Timestamp, Year(export.ReportingPeriodStarting.Date),
      Month(export.ReportingPeriodStarting.Date),
      Day(export.ReportingPeriodStarting.Date));

    // -- Set the Reporting Period Ending Timestamp.
    export.ReportingPeriodEnding.Timestamp =
      Add(local.Null1.Timestamp, Year(export.ReportingPeriodEnding.Date),
      Month(export.ReportingPeriodEnding.Date),
      Day(export.ReportingPeriodEnding.Date));
    export.ReportingPeriodEnding.Timestamp =
      AddMicroseconds(AddDays(export.ReportingPeriodEnding.Timestamp, 1), -1);

    // -- Set the Create Events Flag.
    local.CreateEvents.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 31, 1);

    if (AsChar(local.CreateEvents.Flag) != 'N')
    {
      local.CreateEvents.Flag = "Y";
    }

    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Write the AR Number to the Control Report.
          // -------------------------------------------------------------------------------------------------------------------------
          if (IsEmpty(export.Ar.Number))
          {
            local.EabReportSend.RptDetail =
              "AR Person Number . . . . . . . . . .Not Specified";
          }
          else
          {
            local.EabReportSend.RptDetail =
              "AR Person Number . . . . . . . . . ." + export.Ar.Number;
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
            (Month(export.ReportingPeriodStarting.Date), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Day(export.ReportingPeriodStarting.Date), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Year(export.ReportingPeriodStarting.Date), 12, 4);

          break;
        case 3:
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Write the Reporting Period End Date to the Control Report.
          // -------------------------------------------------------------------------------------------------------------------------
          local.EabReportSend.RptDetail =
            "Reporting Period End Date. . . . . .";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Month(export.ReportingPeriodEnding.Date), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Day(export.ReportingPeriodEnding.Date), 14, 2);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "/";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (Year(export.ReportingPeriodEnding.Date), 12, 4);

          break;
        case 4:
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Write the Create Events Flag to the Control Report.
          // -------------------------------------------------------------------------------------------------------------------------
          local.EabReportSend.RptDetail =
            "Create AR Statement Event. . . . . ." + local.CreateEvents.Flag;

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
      // --  Characters 1-10 contain the obligor person number.
      // -------------------------------------------------------------------------------------------------------------------------
      export.Restart.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);

      // -------------------------------------------------------------------------------------------------------------------------
      // --  Write the Restart info to the Control Report.
      // -------------------------------------------------------------------------------------------------------------------------
      local.EabReportSend.RptDetail = "Restarting at Obligor # " + export
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

  private void UseFnB663WriteExtractDataFile()
  {
    var useImport = new FnB663WriteExtractDataFile.Import();
    var useExport = new FnB663WriteExtractDataFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB663WriteExtractDataFile.Execute, useImport, useExport);

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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEnding.
    /// </summary>
    [JsonPropertyName("reportingPeriodEnding")]
    public DateWorkArea ReportingPeriodEnding
    {
      get => reportingPeriodEnding ??= new();
      set => reportingPeriodEnding = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStarting.
    /// </summary>
    [JsonPropertyName("reportingPeriodStarting")]
    public DateWorkArea ReportingPeriodStarting
    {
      get => reportingPeriodStarting ??= new();
      set => reportingPeriodStarting = value;
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

    private CsePerson ar;
    private DateWorkArea reportingPeriodEnding;
    private DateWorkArea reportingPeriodStarting;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of CreateEvents.
    /// </summary>
    [JsonPropertyName("createEvents")]
    public Common CreateEvents
    {
      get => createEvents ??= new();
      set => createEvents = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private WorkArea day;
    private WorkArea month;
    private TextWorkArea textWorkArea;
    private DateWorkArea dateWorkArea;
    private Common common;
    private Common createEvents;
    private DateWorkArea null1;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
