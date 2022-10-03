// Program: GB_B005_DELETE_OLD_PRGM_RUN_RECS, ID: 374347785, model: 746.
// Short name: SWEG005B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: GB_B005_DELETE_OLD_PRGM_RUN_RECS.
/// </para>
/// <para>
/// Purge 5 weeks (flexible number in days)  Program Run records from production
/// database
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class GbB005DeleteOldPrgmRunRecs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_B005_DELETE_OLD_PRGM_RUN_RECS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbB005DeleteOldPrgmRunRecs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbB005DeleteOldPrgmRunRecs.
  /// </summary>
  public GbB005DeleteOldPrgmRunRecs(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    //   DATE		Developer	Description
    // 04/10/00     	Srini Ganji	Initial Creation
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ************************************************
    // Get the RUN Paramateres for this program
    // ************************************************
    local.ProgramProcessingInfo.Name = "SWEGB005";
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // *****************************************************************
      // Program process date = Null then set it to Current Date
      // *****************************************************************
      if (!Lt(local.NullDate.Date, local.ProgramProcessingInfo.ProcessDate))
      {
        local.ProgramProcessingInfo.ProcessDate = Now().Date;
      }

      // *****************************************************************
      // Read Input Parameters, if any...
      // *****************************************************************
      // *****************************************************************
      // Set default value for Delete Days to 35 Days (==5 weeks)
      // *****************************************************************
      local.NnnNumericDays.Count = 35;

      if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
      {
        // *****************************************************************
        // Extract Delete days from Prarameter list.
        // If DAYS parameter is non numeric or less than 35 then default to 35.
        // *****************************************************************
        local.Position.Count =
          Find(local.ProgramProcessingInfo.ParameterList, "DAYS=");

        if (local.Position.Count > 0)
        {
          local.NnnTextDays.TextLine8 = "00000" + Substring
            (local.ProgramProcessingInfo.ParameterList, local.Position.Count +
            5, 3);

          if (Verify(local.NnnTextDays.TextLine8, "0123456789") == 0)
          {
            local.NnnNumericDays.Count =
              (int)StringToNumber(local.NnnTextDays.TextLine8);

            if (local.NnnNumericDays.Count < 35)
            {
              local.NnnNumericDays.Count = 35;
            }
          }
        }
      }
    }
    else
    {
      return;
    }

    // *****************************************************************
    // Open ERROR Report, DD Name = RPT99
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // *****************************************************************
    // Open CONTROL Report, DD Name = RPT98
    // *****************************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error opening Control Report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // ************************************************
    // *Get the DB2 commit frequency count.           *
    // ************************************************
    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "checkpoint restart not found";
      UseCabErrorReport1();

      return;
    }

    // ************************************************
    // Begin Delete Process
    // ************************************************
    local.Commits.Count = 0;
    local.PgmRunRecsRead.Count = 0;
    local.PgmRecsDelCount.Count = 0;

    // ************************************************
    // Set NNN days old date to ( Process date - input NNN days)
    // ************************************************
    local.NnnDaysOldDate.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate, -
      local.NnnNumericDays.Count);

    // *****************************************************************
    // Write Input parameter Info on CONTROL Report
    // *****************************************************************
    local.Repeat.Count = 1;

    do
    {
      switch(local.Repeat.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "Parameter List : " + (
            local.ProgramProcessingInfo.ParameterList ?? "");

          break;
        case 2:
          local.TempText.TextLine80 =
            NumberToString(Year(local.ProgramProcessingInfo.ProcessDate), 12, 4) +
            "-" + NumberToString
            (Month(local.ProgramProcessingInfo.ProcessDate), 14, 2) + "-";
          local.TempText.TextLine80 =
            Substring(local.TempText.TextLine80, External.TextLine80_MaxLength,
            1, 8) + NumberToString
            (Day(local.ProgramProcessingInfo.ProcessDate), 14, 2);
          local.EabReportSend.RptDetail = "Process Date   : " + Substring
            (local.TempText.TextLine80, External.TextLine80_MaxLength, 1, 10);

          break;
        case 3:
          local.EabReportSend.RptDetail = "Delete Days    : " + NumberToString
            (local.NnnNumericDays.Count, 13, 3);

          break;
        case 4:
          local.TempText.TextLine80 =
            NumberToString(Year(local.NnnDaysOldDate.Date), 12, 4) + "-" + NumberToString
            (Month(local.NnnDaysOldDate.Date), 14, 2) + "-";
          local.TempText.TextLine80 =
            Substring(local.TempText.TextLine80, External.TextLine80_MaxLength,
            1, 8) + NumberToString(Day(local.NnnDaysOldDate.Date), 14, 2);
          local.EabReportSend.RptDetail = "Delete Date    : " + Substring
            (local.TempText.TextLine80, External.TextLine80_MaxLength, 1, 10);

          // ************************************************
          // Format NNN days old timestamp, convert NNN days old date to 
          // Timestamp, format to 'YYYY-MM-DD-00:00:00:000000'
          // ************************************************
          local.NnnDaysOldTs.Timestamp =
            Timestamp(Substring(
              local.TempText.TextLine80, External.TextLine80_MaxLength, 1, 10));
            

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error while writing control report for Input parameter Info";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.Repeat.Count;
    }
    while(local.Repeat.Count != 5);

    // ************************************************
    // Read 5 weeks old Program Run records
    // ************************************************
    foreach(var item in ReadProgramRun())
    {
      ++local.PgmRunRecsRead.Count;

      // ************************************************
      // Don't delete if there is a Program Error record associated with read 
      // Program Run record
      // ************************************************
      if (ReadProgramError())
      {
        continue;
      }

      // ************************************************
      // Don't delete if there is a Program Control Total record associated with
      // read Program Run record
      // ************************************************
      if (ReadProgramControlTotal())
      {
        continue;
      }

      // ************************************************
      // Delete Program Run record
      // ************************************************
      DeleteProgramRun();
      ++local.PgmRecsDelCount.Count;
      ++local.Commits.Count;

      // *****************************************************************
      // Check for Commit count
      // *****************************************************************
      if (local.Commits.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // *****************************************************************
        // Commit database
        // *****************************************************************
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        // *****************************************************************
        // Write commit information on Control report
        // *****************************************************************
        local.EabReportSend.RptDetail = "Program Run Records read so far : " + NumberToString
          (local.PgmRunRecsRead.Count, 15);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + " Time : " + NumberToString
          (TimeToInt(Time(Now())), 10, 6);
        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error while writing control report for Input parameter Info";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.Commits.Count = 0;
      }
    }

    // *****************************************************************
    // Do final Commit
    // *****************************************************************
    UseExtToDoACommit();

    if (local.PassArea.NumericReturnCode != 0)
    {
      ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

      return;
    }

    // *****************************************************************
    // Write Control Report Summary
    // *****************************************************************
    local.Repeat.Count = 1;

    do
    {
      switch(local.Repeat.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Total Program Run records Read    : " + NumberToString
            (local.PgmRunRecsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Total Program Run records Deleted : " + NumberToString
            (local.PgmRecsDelCount.Count, 15);

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error while writing control report for summary details";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.Repeat.Count;
    }
    while(local.Repeat.Count != 3);

    // *****************************************************************
    // Close Control Report File
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while closing control report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // Close Error Report File
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";

    // *****************************************************************
    // End of Praogram run records delete Process
    // *****************************************************************
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

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
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

  private void DeleteProgramRun()
  {
    bool exists;

    exists = Read("DeleteProgramRun#1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "ppiCreatedTstamp",
          entities.ProgramRun.PpiCreatedTstamp.GetValueOrDefault());
        db.SetString(command, "ppiName", entities.ProgramRun.PpiName);
        db.SetDateTime(
          command, "prrStartTstamp",
          entities.ProgramRun.StartTimestamp.GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_PROGRAM_ERROR\".",
        "50001");
    }

    Update("DeleteProgramRun#2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "ppiCreatedTstamp",
          entities.ProgramRun.PpiCreatedTstamp.GetValueOrDefault());
        db.SetString(command, "ppiName", entities.ProgramRun.PpiName);
        db.SetDateTime(
          command, "prrStartTstamp",
          entities.ProgramRun.StartTimestamp.GetValueOrDefault());
      });
  }

  private bool ReadProgramControlTotal()
  {
    System.Diagnostics.Debug.Assert(entities.ProgramRun.Populated);
    entities.ProgramControlTotal.Populated = false;

    return Read("ReadProgramControlTotal",
      (db, command) =>
      {
        db.SetDateTime(
          command, "prrStartTstamp",
          entities.ProgramRun.StartTimestamp.GetValueOrDefault());
        db.SetString(command, "ppiName", entities.ProgramRun.PpiName);
        db.SetDateTime(
          command, "ppiCreatedTstamp",
          entities.ProgramRun.PpiCreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProgramControlTotal.PrrStartTstamp = db.GetDateTime(reader, 0);
        entities.ProgramControlTotal.PpiName = db.GetString(reader, 1);
        entities.ProgramControlTotal.PpiCreatedTstamp =
          db.GetDateTime(reader, 2);
        entities.ProgramControlTotal.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ProgramControlTotal.Populated = true;
      });
  }

  private bool ReadProgramError()
  {
    System.Diagnostics.Debug.Assert(entities.ProgramRun.Populated);
    entities.ProgramError.Populated = false;

    return Read("ReadProgramError",
      (db, command) =>
      {
        db.SetDateTime(
          command, "prrStartTstamp",
          entities.ProgramRun.StartTimestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "ppiCreatedTstamp",
          entities.ProgramRun.PpiCreatedTstamp.GetValueOrDefault());
        db.SetString(command, "ppiName", entities.ProgramRun.PpiName);
      },
      (db, reader) =>
      {
        entities.ProgramError.PrrStartTstamp = db.GetDateTime(reader, 0);
        entities.ProgramError.PpiCreatedTstamp = db.GetDateTime(reader, 1);
        entities.ProgramError.PpiName = db.GetString(reader, 2);
        entities.ProgramError.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ProgramError.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgramRun()
  {
    entities.ProgramRun.Populated = false;

    return ReadEach("ReadProgramRun",
      (db, command) =>
      {
        db.SetDateTime(
          command, "startTimestamp",
          local.NnnDaysOldTs.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProgramRun.PpiCreatedTstamp = db.GetDateTime(reader, 0);
        entities.ProgramRun.PpiName = db.GetString(reader, 1);
        entities.ProgramRun.StartTimestamp = db.GetDateTime(reader, 2);
        entities.ProgramRun.Populated = true;

        return true;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PgmRunRecsRead.
    /// </summary>
    [JsonPropertyName("pgmRunRecsRead")]
    public Common PgmRunRecsRead
    {
      get => pgmRunRecsRead ??= new();
      set => pgmRunRecsRead = value;
    }

    /// <summary>
    /// A value of PgmRecsDelCount.
    /// </summary>
    [JsonPropertyName("pgmRecsDelCount")]
    public Common PgmRecsDelCount
    {
      get => pgmRecsDelCount ??= new();
      set => pgmRecsDelCount = value;
    }

    /// <summary>
    /// A value of NnnTextDays.
    /// </summary>
    [JsonPropertyName("nnnTextDays")]
    public External NnnTextDays
    {
      get => nnnTextDays ??= new();
      set => nnnTextDays = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of NnnNumericDays.
    /// </summary>
    [JsonPropertyName("nnnNumericDays")]
    public Common NnnNumericDays
    {
      get => nnnNumericDays ??= new();
      set => nnnNumericDays = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of Repeat.
    /// </summary>
    [JsonPropertyName("repeat")]
    public Common Repeat
    {
      get => repeat ??= new();
      set => repeat = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of NnnDaysOldDate.
    /// </summary>
    [JsonPropertyName("nnnDaysOldDate")]
    public DateWorkArea NnnDaysOldDate
    {
      get => nnnDaysOldDate ??= new();
      set => nnnDaysOldDate = value;
    }

    /// <summary>
    /// A value of TempText.
    /// </summary>
    [JsonPropertyName("tempText")]
    public External TempText
    {
      get => tempText ??= new();
      set => tempText = value;
    }

    /// <summary>
    /// A value of Commits.
    /// </summary>
    [JsonPropertyName("commits")]
    public Common Commits
    {
      get => commits ??= new();
      set => commits = value;
    }

    /// <summary>
    /// A value of NnnDaysOldTs.
    /// </summary>
    [JsonPropertyName("nnnDaysOldTs")]
    public DateWorkArea NnnDaysOldTs
    {
      get => nnnDaysOldTs ??= new();
      set => nnnDaysOldTs = value;
    }

    private Common pgmRunRecsRead;
    private Common pgmRecsDelCount;
    private External nnnTextDays;
    private Common position;
    private Common nnnNumericDays;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private External passArea;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common repeat;
    private DateWorkArea nullDate;
    private DateWorkArea nnnDaysOldDate;
    private External tempText;
    private Common commits;
    private DateWorkArea nnnDaysOldTs;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
    }

    /// <summary>
    /// A value of ProgramControlTotal.
    /// </summary>
    [JsonPropertyName("programControlTotal")]
    public ProgramControlTotal ProgramControlTotal
    {
      get => programControlTotal ??= new();
      set => programControlTotal = value;
    }

    /// <summary>
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

    private ProgramError programError;
    private ProgramControlTotal programControlTotal;
    private ProgramRun programRun;
  }
#endregion
}
