// Program: FN_B646_HOUSEKEEPING, ID: 372754572, model: 746.
// Short name: SWE02462
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B646_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB646Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B646_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB646Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB646Housekeeping.
  /// </summary>
  public FnB646Housekeeping(IContext context, Import import, Export export):
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
    local.ProgramProcessingInfo.Name = "SWEFB646";

    if (ReadProgramProcessingInfo())
    {
      export.Process.Date = entities.ProgramProcessingInfo.ProcessDate;
      local.PpiFound.Flag = "Y";
      export.ProgramProcessingInfo.Assign(entities.ProgramProcessingInfo);
    }

    if (AsChar(local.PpiFound.Flag) != 'Y')
    {
      ExitState = "ZD_PROGRAM_PROCESSING_INFO_NF_AB";

      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.Process.Date;
    local.EabReportSend.ProgramName = entities.ProgramProcessingInfo.Name;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "ZD_FILE_OPEN_ERROR_WITH_AB";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "ZD_FILE_OPEN_ERROR_WITH_AB";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT BUSINESS REPORT 01
    // **********************************************************
    local.EabReportSend.RptHeading3 =
      "                 DELETE MONTHLY OBLIGATION SUMMARIES";
    local.EabReportSend.NumberOfColHeadings = 2;
    local.EabReportSend.ColHeading1 =
      "  DATE      DESCRIPTION  AMOUNT DUE  AMOUNT PAID   BALANCE";
    local.EabReportSend.ColHeading2 =
      "----------  -----------  ----------  -----------  -----------";
    local.EabReportSend.BlankLineAfterHeading = "Y";
    local.EabReportSend.BlankLineAfterColHead = "N";
    UseCabBusinessReport01();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "ZD_FILE_OPEN_ERROR_WITH_AB";

      return;
    }

    // **********************************************************
    // CALCULATE STATEMENT BEGIN DATE
    // If PPI Process_Date = 11/15/1997, then statement date = 10/31/1997
    // This program will ensure that a monthly obligation summary  exists for 9/
    // 30/97, creating one if necessary.  A summary for 10/31/97 will be
    // created.
    // **********************************************************
    local.ProcessMonth.Count =
      Month(entities.ProgramProcessingInfo.ProcessDate);
    local.ProcessYear.Count = Year(entities.ProgramProcessingInfo.ProcessDate);
    export.BeginSummaryPeriodDateWorkArea.Date =
      IntToDate((int)((long)local.ProcessYear.Count * 10000 + (
        long)local.ProcessMonth.Count * 100 + 1));
    export.BeginSummaryPeriodDateWorkArea.Date =
      AddMonths(export.BeginSummaryPeriodDateWorkArea.Date, -1);
    export.BeginSummaryPeriodDateWorkArea.Timestamp =
      Timestamp(NumberToString(
        Year(export.BeginSummaryPeriodDateWorkArea.Date), 12, 4) + "-" + NumberToString
      (Month(export.BeginSummaryPeriodDateWorkArea.Date), 14, 2) + "-" + NumberToString
      (Day(export.BeginSummaryPeriodDateWorkArea.Date), 14, 2) +
      "-00.00.00.000001");
    export.BeginSummaryPeriodTextWorkArea.Text10 =
      NumberToString(Month(export.BeginSummaryPeriodDateWorkArea.Date), 14, 2) +
      "/" + NumberToString
      (Day(export.BeginSummaryPeriodDateWorkArea.Date), 14, 2) + "/" + NumberToString
      (Year(export.BeginSummaryPeriodDateWorkArea.Date), 12, 4);

    // **********************************************************
    // CALCULATE STATEMENT END DATE
    // **********************************************************
    export.EndSummaryPeriodDateWorkArea.Date =
      AddMonths(export.BeginSummaryPeriodDateWorkArea.Date, 1);
    export.EndSummaryPeriodDateWorkArea.Date =
      AddDays(export.EndSummaryPeriodDateWorkArea.Date, -1);
    export.EndSummaryPeriodDateWorkArea.Timestamp =
      Timestamp(NumberToString(
        Year(export.EndSummaryPeriodDateWorkArea.Date), 12, 4) + "-" + NumberToString
      (Month(export.EndSummaryPeriodDateWorkArea.Date), 14, 2) + "-" + NumberToString
      (Day(export.EndSummaryPeriodDateWorkArea.Date), 14, 2) +
      "-23.59.59.999999");
    export.EndSummaryPeriodTextWorkArea.Text10 =
      NumberToString(Month(export.EndSummaryPeriodDateWorkArea.Date), 14, 2) + "/"
      + NumberToString(Day(export.EndSummaryPeriodDateWorkArea.Date), 14, 2) + "/"
      + NumberToString(Year(export.EndSummaryPeriodDateWorkArea.Date), 12, 4);

    // **********************************************************
    // CALCULATE LAST MONTH OBLIGATION SUMMARY DATE
    // **********************************************************
    local.PreviousMonth.Date =
      AddDays(export.BeginSummaryPeriodDateWorkArea.Date, -1);
    export.PreviousSummaryPeriod.YearMonth = Year(local.PreviousMonth.Date) * 100
      + Month(local.PreviousMonth.Date);

    // **********************************************************
    // CALCULATE THIS MONTH OBLIGATION SUMMARY DATE
    // **********************************************************
    export.CurrentSummaryPeriod.YearMonth =
      Year(export.EndSummaryPeriodDateWorkArea.Date) * 100 + Month
      (export.EndSummaryPeriodDateWorkArea.Date);

    // **********************************************************
    // LOOK AT PPI PARMLIST TO SEE IF AN OBLIGOR WAS SUPPLIED
    // If obligor supplied, only that obligor will be processed.
    // Otherwise all obligors will be processed.
    // **********************************************************
    UseFnB644ValidateParameters();

    if (AsChar(local.ValidDate.Flag) == 'N')
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "The PPI Parmlist did not include a valid conversion date in column 1 (MMDDYYYY)." +
        " " + "   ";
      UseCabErrorReport1();

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        ExitState = "FILE_WRITE_ERROR_RB";
      }
      else
      {
        ExitState = "ACO_NE0000_ENTER_REQUIRD_DATA_RB";
      }

      return;
    }
    else
    {
      export.PpiParameter.ObligorPersonNumber =
        Substring(export.ProgramProcessingInfo.ParameterList, 10, 10);

      if (!IsEmpty(export.PpiParameter.ObligorPersonNumber))
      {
        // **********************************************************
        // WRITE TO CONTROL REPORT 98
        // **********************************************************
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "The PPI Parmlist requested processing for only obligor number: " + (
            export.PpiParameter.ObligorPersonNumber ?? "") + "   ";
        UseCabControlReport1();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabControlReport1();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

    // **********************************************************
    // WRITE STATEMENT DATE RANGE TO CONTROL REPORT 98
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "Statement Date Range:" + NumberToString
      (Month(export.BeginSummaryPeriodDateWorkArea.Date), 14, 2) + "/" + NumberToString
      (Day(export.BeginSummaryPeriodDateWorkArea.Date), 14, 2) + "/" + NumberToString
      (Year(export.BeginSummaryPeriodDateWorkArea.Date), 12, 4) + " thru " + NumberToString
      (Month(export.EndSummaryPeriodDateWorkArea.Date), 14, 2) + "/" + NumberToString
      (Day(export.EndSummaryPeriodDateWorkArea.Date), 14, 2) + "/" + NumberToString
      (Year(export.EndSummaryPeriodDateWorkArea.Date), 12, 4);
    UseCabControlReport1();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // GET COMMIT FREQUENCY AND SEE IF THIS IS A RESTART
    // **********************************************************
    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // **** ok, continue processing. Pick up the last Cash Receipt Detail 
      // processed, so that during restart we can skip it ****
      if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
      {
        export.Restart.ObligorPersonNumber =
          export.ProgramCheckpointRestart.RestartInfo ?? "";

        // **********************************************************
        // WRITE TO CONTROL REPORT 98
        // **********************************************************
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "This program is being restarted after obligor number: " + (
            export.Restart.ObligorPersonNumber ?? "") + "   ";
        UseCabControlReport1();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabControlReport1();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        }
      }
    }
    else
    {
      ExitState = "PROGRAM_CHECKPOINT_NF_RB";
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
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.EabReportSend);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB644ValidateParameters()
  {
    var useImport = new FnB644ValidateParameters.Import();
    var useExport = new FnB644ValidateParameters.Export();

    useImport.ProgramProcessingInfo.ParameterList =
      export.ProgramProcessingInfo.ParameterList;

    Call(FnB644ValidateParameters.Execute, useImport, useExport);

    local.ValidDate.Flag = useExport.ValidDate.Flag;
    export.Conversion.Date = useExport.Conversion.Date;
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

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", local.ProgramProcessingInfo.Name);
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 3);
        entities.ProgramProcessingInfo.Populated = true;
      });
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of BeginSummaryPeriodDateWorkArea.
    /// </summary>
    [JsonPropertyName("beginSummaryPeriodDateWorkArea")]
    public DateWorkArea BeginSummaryPeriodDateWorkArea
    {
      get => beginSummaryPeriodDateWorkArea ??= new();
      set => beginSummaryPeriodDateWorkArea = value;
    }

    /// <summary>
    /// A value of BeginSummaryPeriodTextWorkArea.
    /// </summary>
    [JsonPropertyName("beginSummaryPeriodTextWorkArea")]
    public TextWorkArea BeginSummaryPeriodTextWorkArea
    {
      get => beginSummaryPeriodTextWorkArea ??= new();
      set => beginSummaryPeriodTextWorkArea = value;
    }

    /// <summary>
    /// A value of EndSummaryPeriodDateWorkArea.
    /// </summary>
    [JsonPropertyName("endSummaryPeriodDateWorkArea")]
    public DateWorkArea EndSummaryPeriodDateWorkArea
    {
      get => endSummaryPeriodDateWorkArea ??= new();
      set => endSummaryPeriodDateWorkArea = value;
    }

    /// <summary>
    /// A value of EndSummaryPeriodTextWorkArea.
    /// </summary>
    [JsonPropertyName("endSummaryPeriodTextWorkArea")]
    public TextWorkArea EndSummaryPeriodTextWorkArea
    {
      get => endSummaryPeriodTextWorkArea ??= new();
      set => endSummaryPeriodTextWorkArea = value;
    }

    /// <summary>
    /// A value of PreviousSummaryPeriod.
    /// </summary>
    [JsonPropertyName("previousSummaryPeriod")]
    public MonthlyObligorSummary PreviousSummaryPeriod
    {
      get => previousSummaryPeriod ??= new();
      set => previousSummaryPeriod = value;
    }

    /// <summary>
    /// A value of CurrentSummaryPeriod.
    /// </summary>
    [JsonPropertyName("currentSummaryPeriod")]
    public MonthlyObligorSummary CurrentSummaryPeriod
    {
      get => currentSummaryPeriod ??= new();
      set => currentSummaryPeriod = value;
    }

    /// <summary>
    /// A value of Conversion.
    /// </summary>
    [JsonPropertyName("conversion")]
    public DateWorkArea Conversion
    {
      get => conversion ??= new();
      set => conversion = value;
    }

    /// <summary>
    /// A value of PpiParameter.
    /// </summary>
    [JsonPropertyName("ppiParameter")]
    public CashReceiptDetail PpiParameter
    {
      get => ppiParameter ??= new();
      set => ppiParameter = value;
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
    public CashReceiptDetail Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    private DateWorkArea process;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private DateWorkArea beginSummaryPeriodDateWorkArea;
    private TextWorkArea beginSummaryPeriodTextWorkArea;
    private DateWorkArea endSummaryPeriodDateWorkArea;
    private TextWorkArea endSummaryPeriodTextWorkArea;
    private MonthlyObligorSummary previousSummaryPeriod;
    private MonthlyObligorSummary currentSummaryPeriod;
    private DateWorkArea conversion;
    private CashReceiptDetail ppiParameter;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CashReceiptDetail restart;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    /// <summary>
    /// A value of PpiFound.
    /// </summary>
    [JsonPropertyName("ppiFound")]
    public Common PpiFound
    {
      get => ppiFound ??= new();
      set => ppiFound = value;
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
    /// A value of ProcessMonth.
    /// </summary>
    [JsonPropertyName("processMonth")]
    public Common ProcessMonth
    {
      get => processMonth ??= new();
      set => processMonth = value;
    }

    /// <summary>
    /// A value of ProcessYear.
    /// </summary>
    [JsonPropertyName("processYear")]
    public Common ProcessYear
    {
      get => processYear ??= new();
      set => processYear = value;
    }

    /// <summary>
    /// A value of PreviousMonth.
    /// </summary>
    [JsonPropertyName("previousMonth")]
    public DateWorkArea PreviousMonth
    {
      get => previousMonth ??= new();
      set => previousMonth = value;
    }

    /// <summary>
    /// A value of ValidDate.
    /// </summary>
    [JsonPropertyName("validDate")]
    public Common ValidDate
    {
      get => validDate ??= new();
      set => validDate = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private Common ppiFound;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common processMonth;
    private Common processYear;
    private DateWorkArea previousMonth;
    private Common validDate;
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
