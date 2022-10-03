// Program: SP_B350_CLOSE_ASSIGNMENTS, ID: 374342800, model: 746.
// Short name: SWEP350B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B350_CLOSE_ASSIGNMENTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB350CloseAssignments: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B350_CLOSE_ASSIGNMENTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB350CloseAssignments(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB350CloseAssignments.
  /// </summary>
  public SpB350CloseAssignments(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    //            M A I N T E N A N C E       L O G
    // -------------------------------------------------------------------
    //   Date     Developer   PR#    Description
    // --------   ---------   ---    -----------
    // 12/22/99    SWDPARM H00083260 New Program
    // 12/04/02    SWSRPDP PR 162736 Add Checkpoint Processing
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.ClosureDate = new DateTime(2099, 12, 31);
    local.ProgramProcessingInfo.Name = global.UserId;

    // *** get the run parameters for this program
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // * *      To CONTROL Commit Frequency
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // **************************************************
    // * Open the ERROR RPT DDNAME=RPT99
    // **************************************************
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = Now().Date;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **************************************************
    // * Open the ERROR RPT DDNAME=RPT98
    // **************************************************
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = Now().Date;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // **************************************************
      // * Write a line to the error report
      // **************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Control Report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    foreach(var item in ReadMonitoredActivityMonitoredActivityAssignment())
    {
      try
      {
        UpdateMonitoredActivityAssignment();
        ++local.MonaAssignUpdates.Count;
        local.Processed.UpdateFrequencyCount =
          local.Processed.UpdateFrequencyCount.GetValueOrDefault() + 1;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_MONITORED_ACT_ASSGN_NU";

            // **************************************************
            // * Write to ERROR RPT DDNAME=RPT99
            // **************************************************
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "MONITORED ACTIVITY ASSIGNMENT NOT UNIQUE";
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            continue;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_MONITORED_ACT_ASSGN_PV";

            // **************************************************
            // * Write to ERROR RPT DDNAME=RPT99
            // **************************************************
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "MONITORED ACTIVITY ASSIGNMENT PERMITTED VALUE VIOLATION";
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            continue;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // * *   PR 162736  12/04/02    Add Checkpoint Processing
      // * *      To CONTROL Commit Frequency
      if (local.Processed.UpdateFrequencyCount.GetValueOrDefault() >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();

        if (local.ForCommit.NumericReturnCode != 0)
        {
          ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

          return;
        }

        local.Processed.UpdateFrequencyCount = 0;
      }
    }

    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail =
      "Updated Monitored Activity Assigments: " + NumberToString
      (local.MonaAssignUpdates.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // **************************************************
      // * Write a line to the error report
      // **************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Control Report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **************************************************
    // * CLOSE the ERROR RPT DDNAME=RPT99
    // **************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **************************************************
    // * CLOSE the ERROR RPT DDNAME=RPT98
    // **************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // **************************************************
      // * Write a line to the error report
      // **************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Control Report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
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

    useExport.External.NumericReturnCode = local.ForCommit.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ForCommit.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private IEnumerable<bool> ReadMonitoredActivityMonitoredActivityAssignment()
  {
    entities.MonitoredActivityAssignment.Populated = false;
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivityMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "closureDate", local.Max.ClosureDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 0);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 1);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 6);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 7);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 8);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 9);
        entities.MonitoredActivityAssignment.Populated = true;
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private void UpdateMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.MonitoredActivityAssignment.Populated);

    var discontinueDate = entities.MonitoredActivity.ClosureDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.MonitoredActivityAssignment.Populated = false;
    Update("UpdateMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.MonitoredActivityAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.
          SetInt32(command, "spdId", entities.MonitoredActivityAssignment.SpdId);
          
        db.
          SetInt32(command, "offId", entities.MonitoredActivityAssignment.OffId);
          
        db.SetString(
          command, "ospCode", entities.MonitoredActivityAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.MonitoredActivityAssignment.OspDate.GetValueOrDefault());
        db.
          SetInt32(command, "macId", entities.MonitoredActivityAssignment.MacId);
          
      });

    entities.MonitoredActivityAssignment.DiscontinueDate = discontinueDate;
    entities.MonitoredActivityAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.MonitoredActivityAssignment.Populated = true;
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
    /// A value of MonaAssignUpdates.
    /// </summary>
    [JsonPropertyName("monaAssignUpdates")]
    public Common MonaAssignUpdates
    {
      get => monaAssignUpdates ??= new();
      set => monaAssignUpdates = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public MonitoredActivity Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public ProgramCheckpointRestart Processed
    {
      get => processed ??= new();
      set => processed = value;
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
    /// A value of ForCommit.
    /// </summary>
    [JsonPropertyName("forCommit")]
    public External ForCommit
    {
      get => forCommit ??= new();
      set => forCommit = value;
    }

    private Common monaAssignUpdates;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private MonitoredActivity max;
    private ProgramCheckpointRestart processed;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External forCommit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    private MonitoredActivityAssignment monitoredActivityAssignment;
    private MonitoredActivity monitoredActivity;
  }
#endregion
}
