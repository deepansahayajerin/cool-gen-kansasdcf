// Program: FN_B792_BATCH_INITIALIZATION, ID: 1902416810, model: 746.
// Short name: SWE03730
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B792_BATCH_INITIALIZATION.
/// </summary>
[Serializable]
public partial class FnB792BatchInitialization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B792_BATCH_INITIALIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB792BatchInitialization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB792BatchInitialization.
  /// </summary>
  public FnB792BatchInitialization(IContext context, Import import,
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
    // 10/01/2013 DDupre	CQ38344	      Initial Development.
    // -------------------------------------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEFB792";
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

    local.EabFileHandling.Action = "OPEN";
    UseFnB792EabWriteIvrFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // --  write to error file...
      local.EabReportSend.RptDetail =
        "(01) Error writing IVR record to the file...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();
      ExitState = "ERROR_WRITING_TO_FILE_AB";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Extract the AR number and Number of days from the PPI record.
    // --
    // --  Position  1 - 4  Number of days     (format XXX,)
    // --  Position  5 - 14  AR cse person number     (format XXXXXXXXXX)
    // -------------------------------------------------------------------------------------------------------------------------
    local.Start.Count = 1;
    local.Current.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;
    local.IncludeArrearsOnly.Flag = "";

    do
    {
      local.Postion.Text1 =
        Substring(local.ProgramProcessingInfo.ParameterList,
        local.CurrentPosition.Count, 1);

      if (AsChar(local.Postion.Text1) == ',')
      {
        ++local.FieldNumber.Count;
        local.WorkArea.Text15 = "";

        if (local.FieldNumber.Count == 1)
        {
          if (local.Current.Count == 1)
          {
            local.NumberOfDaysInPeriod.Count = 90;
          }
          else
          {
            local.WorkArea.Text15 =
              Substring(local.ProgramProcessingInfo.ParameterList,
              local.Start.Count, local.Current.Count - 1);
            local.NumberOfDaysInPeriod.Count =
              (int)StringToNumber(local.WorkArea.Text15);
          }

          local.Start.Count = local.CurrentPosition.Count + 1;
          local.Current.Count = 0;

          break;
        }
        else
        {
        }
      }
      else if (IsEmpty(local.Postion.Text1))
      {
        break;
      }

      ++local.CurrentPosition.Count;
      ++local.Current.Count;
    }
    while(!Equal(global.Command, "COMMAND"));

    if (local.NumberOfDaysInPeriod.Count <= 0)
    {
      local.NumberOfDaysInPeriod.Count = 90;
    }

    export.ReportingPeriodEndingDateWorkArea.Date =
      local.ProgramProcessingInfo.ProcessDate;
    export.ReportingPeriodStartingDateWorkArea.Date =
      AddDays(export.ReportingPeriodEndingDateWorkArea.Date,
      -(local.NumberOfDaysInPeriod.Count - 1));

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
          if (IsEmpty(local.ArCsePerson.Number))
          {
            local.EabReportSend.RptDetail =
              "AR Person Number . . . . . . . . . .Not Specified";
          }
          else
          {
            local.EabReportSend.RptDetail =
              "AR Person Number . . . . . . . . . ." + local
              .ArCsePerson.Number;
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
            "Create AR File For Call Center . . ." + export.CreateEvents.Flag;

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

    local.EabFileHandling.Action = "WRITE";
    local.Month.Text2 =
      NumberToString(Month(local.ProgramProcessingInfo.ProcessDate), 14, 2);
    local.Day.Text2 =
      NumberToString(Day(local.ProgramProcessingInfo.ProcessDate), 14, 2);
    local.Year.Text4 =
      NumberToString(Year(local.ProgramProcessingInfo.ProcessDate), 12, 4);
    local.CreateDate.Text8 = local.Year.Text4 + local.Month.Text2 + local
      .Day.Text2;
    local.RecordType.Text1 = "1";
    UseFnB792EabWriteIvrFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // --  write to error file...
      local.EabReportSend.RptDetail =
        "(01) Error writing IVR record to the file...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();
      ExitState = "ERROR_WRITING_TO_FILE_AB";

      return;
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

  private void UseFnB792EabWriteIvrFile()
  {
    var useImport = new FnB792EabWriteIvrFile.Import();
    var useExport = new FnB792EabWriteIvrFile.Export();

    useImport.CreateDate.Text8 = local.CreateDate.Text8;
    useImport.RecordType.Text1 = local.RecordType.Text1;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB792EabWriteIvrFile.Execute, useImport, useExport);

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
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public WorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of CreateDate.
    /// </summary>
    [JsonPropertyName("createDate")]
    public WorkArea CreateDate
    {
      get => createDate ??= new();
      set => createDate = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public WorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// A value of IncludeArrearsOnly.
    /// </summary>
    [JsonPropertyName("includeArrearsOnly")]
    public Common IncludeArrearsOnly
    {
      get => includeArrearsOnly ??= new();
      set => includeArrearsOnly = value;
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
    /// A value of NumberOfDaysInPeriod.
    /// </summary>
    [JsonPropertyName("numberOfDaysInPeriod")]
    public Common NumberOfDaysInPeriod
    {
      get => numberOfDaysInPeriod ??= new();
      set => numberOfDaysInPeriod = value;
    }

    /// <summary>
    /// A value of TotalForwardedToFamily.
    /// </summary>
    [JsonPropertyName("totalForwardedToFamily")]
    public Collection TotalForwardedToFamily
    {
      get => totalForwardedToFamily ??= new();
      set => totalForwardedToFamily = value;
    }

    /// <summary>
    /// A value of AppliedAsArrears.
    /// </summary>
    [JsonPropertyName("appliedAsArrears")]
    public Collection AppliedAsArrears
    {
      get => appliedAsArrears ??= new();
      set => appliedAsArrears = value;
    }

    /// <summary>
    /// A value of AppliedAsCurrent.
    /// </summary>
    [JsonPropertyName("appliedAsCurrent")]
    public Collection AppliedAsCurrent
    {
      get => appliedAsCurrent ??= new();
      set => appliedAsCurrent = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of DerivedAr.
    /// </summary>
    [JsonPropertyName("derivedAr")]
    public CsePerson DerivedAr
    {
      get => derivedAr ??= new();
      set => derivedAr = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    private WorkArea year;
    private WorkArea createDate;
    private WorkArea recordType;
    private Common common;
    private CsePerson arCsePerson;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea null1;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea dateWorkArea;
    private TextWorkArea textWorkArea;
    private WorkArea day;
    private WorkArea month;
    private Common start;
    private Common current;
    private Common currentPosition;
    private Common fieldNumber;
    private Common includeArrearsOnly;
    private TextWorkArea postion;
    private WorkArea workArea;
    private Common numberOfDaysInPeriod;
    private Collection totalForwardedToFamily;
    private Collection appliedAsArrears;
    private Collection appliedAsCurrent;
    private CsePerson apCsePerson;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private Collection collection;
    private LegalAction legalAction;
    private CsePerson derivedAr;
    private CsePersonsWorkSet arCsePersonsWorkSet;
  }
#endregion
}
