// Program: SP_B305_EXTERNAL_ALERT, ID: 372050646, model: 746.
// Short name: SWEP305B
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
/// A program: SP_B305_EXTERNAL_ALERT.
/// </para>
/// <para>
///    This PrAD handles the processing of INTERFACE_ALERT.
///    If the alert is an outgoing, there is no processing for this PrAD to 
/// handle.
///    If the alert is an incoming, an OSP_ALERT is created which can be 
/// displayed on screens.  This is performed by SP_INCOMING_EXT_ALERT.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB305ExternalAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B305_EXTERNAL_ALERT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB305ExternalAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB305ExternalAlert.
  /// </summary>
  public SpB305ExternalAlert(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *****************************************************************
    //  Date		Developer	Request #      Description
    // *****************************************************************
    //  19 Sep 96     Michael Ramirez                  Initial Dev
    //  17 Jan 97     R. Marchman                      Rework and test.
    //  03 Dec 98     C. Ott    Modified for compliance with DIR standards for
    //                          Error and Control reports.
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *****************************************************************
    //  Get the run parameters for this program
    // *****************************************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(local.ProgramProcessingInfo.ProcessDate,
      local.InitializedDate.Date))
    {
      local.CurrentDate.Date = Now().Date;
      local.CurrentDate.Timestamp = Now();
    }
    else if (Equal(local.ProgramProcessingInfo.ProcessDate, local.MaxDate.Date))
    {
      local.CurrentDate.Date = Now().Date;
      local.CurrentDate.Timestamp = Now();
    }
    else
    {
      local.CurrentDate.Date = local.ProgramProcessingInfo.ProcessDate;
      local.CurrentDate.Timestamp =
        local.ProgramProcessingInfo.CreatedTimestamp;
    }

    // *****************************************************************
    // Open ERROR Report, DD Name = RPT99
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.CurrentDate.Date;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

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
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // *****************************************************************
    //  Get the DB2 commit frequency counts.
    // *****************************************************************
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error reading Program Checkpoint Restart table.";
      UseCabErrorReport2();

      return;
    }

    local.ProgramCheckpointRestart.CheckpointCount = 0;

    // ****************************************************************
    //  Check and setup restart key.
    //  Position 1-16 Identifier which is a text timestamp
    // ****************************************************************
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.CheckpointRestart.Identifier =
        local.ProgramCheckpointRestart.RestartInfo ?? Spaces(16);
    }
    else
    {
      local.CheckpointRestart.Identifier = local.Initialized.Identifier;
    }

    local.CheckpointNumbOfReads.Count = 0;
    local.CheckpointNumbOfUpdates.Count = 0;
    local.CntlTotNumbAlrtsCreatd.Count = 0;
    local.ControlTotNumbErrRecs.Count = 0;
    local.ControlTotNumbRecsRead.Count = 0;

    // ****************************************************************
    // Records are processed in groups based on commit frequency
    // obtained from entity Program Checkpoint Restart.
    // Commits are performed at the end of each group.
    // ****************************************************************
    // ****************************************************************
    // Read all INTERFACE_ALERTs which haven't been processed, (i.e. have a 
    // Process Status of "S").   May be a restart situation, may not.
    // ****************************************************************
    foreach(var item in ReadInterfaceAlert())
    {
      // *****************************************************************
      // Checkpoint is done against number of reads. Do not change.
      // ****************************************************************
      ++local.CheckpointNumbOfReads.Count;
      ++local.ControlTotNumbRecsRead.Count;

      // *****************************************************************
      // Action Block does all the data retrieval.
      // ****************************************************************
      local.CurrentRecordIdInQueue.Identifier =
        entities.InterfaceAlert.Identifier;
      UseSpIncomingExtAlrt();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ****************************************************************
        // When Interface Alert record has been successfully processed, set 
        // Process Status to "P" to assure that the same record is not processed
        // again.
        // ****************************************************************
        try
        {
          UpdateInterfaceAlert2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Not Unique Database error encountered while updating Interface Alert file record # " +
                entities.InterfaceAlert.Identifier;
              UseCabErrorReport1();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              break;
            case ErrorCode.PermittedValueViolation:
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Permitted Value Database error encountered while updating Interface Alert file record # " +
                entities.InterfaceAlert.Identifier;
              UseCabErrorReport1();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        if (IsExitState("SP0000_EVENT_DETAIL_NF"))
        {
          local.EabReportSend.RptDetail = "Interface Alert Identifier: " + TrimEnd
            (local.ReturnFromIncomingExtAlert.Identifier) + ";  CSE Person Number: " +
            entities.InterfaceAlert.CsePersonNumber + ".  Event ID '" + NumberToString
            (local.Infrastructure.EventId, 11, 5) + "' Reason Code '" + TrimEnd
            (local.Infrastructure.ReasonCode) + "' not found.";
        }
        else if (IsExitState("SP0000_ALERT_NF"))
        {
          local.EabReportSend.RptDetail = "Interface Alert Identifier: " + TrimEnd
            (local.ReturnFromIncomingExtAlert.Identifier) + ";  CSE Person Number: " +
            entities.InterfaceAlert.CsePersonNumber + ".  Alert not found for Alert Code '" +
            entities.InterfaceAlert.AlertCode + "'.";
        }
        else if (IsExitState("ACO_NE0000_INVALID_CODE"))
        {
          local.EabReportSend.RptDetail = "Interface Alert Identifier: " + TrimEnd
            (local.ReturnFromIncomingExtAlert.Identifier) + ";  CSE Person Number: " +
            entities.InterfaceAlert.CsePersonNumber + ".  Invalid Interface Alert Code '" +
            entities.InterfaceAlert.AlertCode + "'.";
        }
        else if (IsExitState("CASE_ROLE_NF"))
        {
          local.EabReportSend.RptDetail = "Interface Alert Identifier: " + local
            .ReturnFromIncomingExtAlert.Identifier + TrimEnd
            (";  CSE Person Number: " + entities.InterfaceAlert.CsePersonNumber) +
            ".  Alert Code '" + entities.InterfaceAlert.AlertCode + "'.  " + TrimEnd
            (local.Error.Type1) + " Case Role was not found.";
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          local.EabReportSend.RptDetail = "Interface Alert Identifier: " + TrimEnd
            (local.ReturnFromIncomingExtAlert.Identifier) + ";  CSE Person Number: " +
            entities.InterfaceAlert.CsePersonNumber + " was not found in the CSE Person database.";
            
        }
        else
        {
        }

        // ****************************************************************
        // When Interface Alert record has not been successfully processed, set 
        // Process Status to "E" so that the record may be retrieved for
        // possible correction.
        // ****************************************************************
        try
        {
          UpdateInterfaceAlert1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Not Unique Database error encountered while updating Interface Alert file record # " +
                entities.InterfaceAlert.Identifier;
              UseCabErrorReport1();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              break;
            case ErrorCode.PermittedValueViolation:
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Permitted Value Database error encountered while updating Interface Alert file record # " +
                entities.InterfaceAlert.Identifier;
              UseCabErrorReport1();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // *****************************************************************
        // Write ERROR Report
        // *****************************************************************
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        if (Equal(local.EabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        ++local.ControlTotNumbErrRecs.Count;
        ExitState = "ACO_NN0000_ALL_OK";
      }

      // ****************************************************************
      // Evaluate checkpoint count to determine whether commit is needed.
      // ****************************************************************
      if (local.CheckpointNumbOfReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // *****************************************************************
        // Update 1. number of checkpoints and the last checkpoint
        //        2. last checkpoint time
        //        3. Set restart indicator to YES
        //        4. Restart Information
        // Also, return the checkpoint frequency count in case they
        // have been changed since the last read.
        // CAB increments checkpoint counter.
        // ****************************************************************
        local.ProgramCheckpointRestart.RestartInfo =
          local.CurrentRecordIdInQueue.Identifier;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        UseUpdatePgmCheckpointRestart2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // ****************************************************************
        // External DB2 commit.
        // ****************************************************************
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // *****************************************************************
        // Reset checkpoint counter
        // ****************************************************************
        local.CheckpointNumbOfReads.Count = 0;
      }

      // ****************************************************************
      // End of READ EACH
      // ****************************************************************
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.RestartInd = "N";
    }
    else
    {
      local.ProgramCheckpointRestart.RestartInfo =
        local.CurrentRecordIdInQueue.Identifier;
      local.ProgramCheckpointRestart.RestartInd = "Y";
    }

    UseUpdatePgmCheckpointRestart1();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error updating Program Checkpoint Restart table.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ****************************************************************
    // After all Interface Alert records have been processed, write
    // the program control total report to reflect the total creates
    // and updates.
    // ****************************************************************
    // ****************************************************************
    // Create Control Total 1
    // ****************************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      "TOTAL NUMBER OF EXTERNAL ALERT RECORDS READ          " + NumberToString
      (local.ControlTotNumbRecsRead.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report..";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // *****************************************************************
    // Create Control Total 2
    // *****************************************************************
    local.EabReportSend.RptDetail =
      "TOTAL NUMBER OF EXTERNAL ALERTS CREATED              " + NumberToString
      (local.CntlTotNumbAlrtsCreatd.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report..";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ****************************************************************
    // Create Control Total 3
    // ****************************************************************
    local.EabReportSend.RptDetail =
      "TOTAL NUMBER OF ERROR RECORDS CREATED                " + NumberToString
      (local.ControlTotNumbErrRecs.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report..";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // *****************************************************************
    // Close CONTROL Report
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error writing to Control Report..";
      UseCabErrorReport2();
    }

    // *****************************************************************
    // Close ERROR Report
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
    {
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
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
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
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

  private void UseSpIncomingExtAlrt()
  {
    var useImport = new SpIncomingExtAlrt.Import();
    var useExport = new SpIncomingExtAlrt.Export();

    useImport.ExpCntlTotNumAlrtsCreat.Count =
      local.CntlTotNumbAlrtsCreatd.Count;
    useImport.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);
    useImport.InterfaceAlert.Assign(entities.InterfaceAlert);

    Call(SpIncomingExtAlrt.Execute, useImport, useExport);

    local.CntlTotNumbAlrtsCreatd.Count =
      useImport.ExpCntlTotNumAlrtsCreat.Count;
    entities.InterfaceAlert.Assign(useImport.InterfaceAlert);
    local.Error.Type1 = useExport.Error.Type1;
    MoveInfrastructure(useExport.XtrnlAgencyNotfAlert, local.Infrastructure);
    local.ReturnFromIncomingExtAlert.Identifier =
      useExport.InterfaceAlert.Identifier;
  }

  private void UseUpdatePgmCheckpointRestart1()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart2()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool> ReadInterfaceAlert()
  {
    entities.InterfaceAlert.Populated = false;

    return ReadEach("ReadInterfaceAlert",
      (db, command) =>
      {
        db.SetString(command, "identifier", local.CheckpointRestart.Identifier);
      },
      (db, reader) =>
      {
        entities.InterfaceAlert.Identifier = db.GetString(reader, 0);
        entities.InterfaceAlert.CsePersonNumber =
          db.GetNullableString(reader, 1);
        entities.InterfaceAlert.AlertCode = db.GetNullableString(reader, 2);
        entities.InterfaceAlert.AlertName = db.GetNullableString(reader, 3);
        entities.InterfaceAlert.SendingSystem = db.GetNullableString(reader, 4);
        entities.InterfaceAlert.ReceivingSystem =
          db.GetNullableString(reader, 5);
        entities.InterfaceAlert.ProcessStatus = db.GetNullableString(reader, 6);
        entities.InterfaceAlert.LastUpdatedTmstamp =
          db.GetNullableDateTime(reader, 7);
        entities.InterfaceAlert.NoteText = db.GetNullableString(reader, 8);
        entities.InterfaceAlert.Populated = true;

        return true;
      });
  }

  private void UpdateInterfaceAlert1()
  {
    var processStatus = "E";
    var lastUpdatedTmstamp = local.CurrentDate.Timestamp;

    entities.InterfaceAlert.Populated = false;
    Update("UpdateInterfaceAlert1",
      (db, command) =>
      {
        db.SetNullableString(command, "processStatus", processStatus);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatedTmstamp);
        db.SetString(command, "identifier", entities.InterfaceAlert.Identifier);
      });

    entities.InterfaceAlert.ProcessStatus = processStatus;
    entities.InterfaceAlert.LastUpdatedTmstamp = lastUpdatedTmstamp;
    entities.InterfaceAlert.Populated = true;
  }

  private void UpdateInterfaceAlert2()
  {
    var processStatus = "P";
    var lastUpdatedTmstamp = local.CurrentDate.Timestamp;

    entities.InterfaceAlert.Populated = false;
    Update("UpdateInterfaceAlert2",
      (db, command) =>
      {
        db.SetNullableString(command, "processStatus", processStatus);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatedTmstamp);
        db.SetString(command, "identifier", entities.InterfaceAlert.Identifier);
      });

    entities.InterfaceAlert.ProcessStatus = processStatus;
    entities.InterfaceAlert.LastUpdatedTmstamp = lastUpdatedTmstamp;
    entities.InterfaceAlert.Populated = true;
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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public CaseRole Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of CheckpointNumbOfUpdates.
    /// </summary>
    [JsonPropertyName("checkpointNumbOfUpdates")]
    public Common CheckpointNumbOfUpdates
    {
      get => checkpointNumbOfUpdates ??= new();
      set => checkpointNumbOfUpdates = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of InitializedDate.
    /// </summary>
    [JsonPropertyName("initializedDate")]
    public DateWorkArea InitializedDate
    {
      get => initializedDate ??= new();
      set => initializedDate = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of CurrentRecordIdInQueue.
    /// </summary>
    [JsonPropertyName("currentRecordIdInQueue")]
    public InterfaceAlert CurrentRecordIdInQueue
    {
      get => currentRecordIdInQueue ??= new();
      set => currentRecordIdInQueue = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public InterfaceAlert Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of ReturnFromIncomingExtAlert.
    /// </summary>
    [JsonPropertyName("returnFromIncomingExtAlert")]
    public InterfaceAlert ReturnFromIncomingExtAlert
    {
      get => returnFromIncomingExtAlert ??= new();
      set => returnFromIncomingExtAlert = value;
    }

    /// <summary>
    /// A value of CheckpointRestart.
    /// </summary>
    [JsonPropertyName("checkpointRestart")]
    public InterfaceAlert CheckpointRestart
    {
      get => checkpointRestart ??= new();
      set => checkpointRestart = value;
    }

    /// <summary>
    /// A value of ControlTotNumbErrRecs.
    /// </summary>
    [JsonPropertyName("controlTotNumbErrRecs")]
    public Common ControlTotNumbErrRecs
    {
      get => controlTotNumbErrRecs ??= new();
      set => controlTotNumbErrRecs = value;
    }

    /// <summary>
    /// A value of CntlTotNumbAlrtsCreatd.
    /// </summary>
    [JsonPropertyName("cntlTotNumbAlrtsCreatd")]
    public Common CntlTotNumbAlrtsCreatd
    {
      get => cntlTotNumbAlrtsCreatd ??= new();
      set => cntlTotNumbAlrtsCreatd = value;
    }

    /// <summary>
    /// A value of ControlTotNumbRecsRead.
    /// </summary>
    [JsonPropertyName("controlTotNumbRecsRead")]
    public Common ControlTotNumbRecsRead
    {
      get => controlTotNumbRecsRead ??= new();
      set => controlTotNumbRecsRead = value;
    }

    /// <summary>
    /// A value of ProcessOption.
    /// </summary>
    [JsonPropertyName("processOption")]
    public Common ProcessOption
    {
      get => processOption ??= new();
      set => processOption = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of CheckpointNumbOfReads.
    /// </summary>
    [JsonPropertyName("checkpointNumbOfReads")]
    public Common CheckpointNumbOfReads
    {
      get => checkpointNumbOfReads ??= new();
      set => checkpointNumbOfReads = value;
    }

    private CaseRole error;
    private Infrastructure infrastructure;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Common checkpointNumbOfUpdates;
    private DateWorkArea currentDate;
    private DateWorkArea initializedDate;
    private DateWorkArea maxDate;
    private InterfaceAlert currentRecordIdInQueue;
    private InterfaceAlert initialized;
    private InterfaceAlert returnFromIncomingExtAlert;
    private InterfaceAlert checkpointRestart;
    private Common controlTotNumbErrRecs;
    private Common cntlTotNumbAlrtsCreatd;
    private Common controlTotNumbRecsRead;
    private Common processOption;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private Common checkpointNumbOfReads;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    private InterfaceAlert interfaceAlert;
  }
#endregion
}
