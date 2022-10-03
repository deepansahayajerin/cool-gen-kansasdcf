// Program: SP_EVENT_PROCESSOR, ID: 372067524, model: 746.
// Short name: SWE01852
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
/// A program: SP_EVENT_PROCESSOR.
/// </para>
/// <para>
/// Processes the infrastructure record :
/// Process Life Cycle Transformation
/// Process Alerts
/// Process Monitored Activities
/// </para>
/// </summary>
[Serializable]
public partial class SpEventProcessor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EVENT_PROCESSOR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEventProcessor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEventProcessor.
  /// </summary>
  public SpEventProcessor(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------------------------------------------------------
    // DATE	  DEVELOPER   REQUEST#	DESCRIPTION
    // 04/15/10  GVandy	CQ 966	1) Initial Development of this cab.
    // 				2) New index on INFRASTRUCTURE process_status eliminates the need
    // 				   to read using created_timestamp > timestamp of the last run of
    // 				   the EP.
    // 				3) All prep work is done prior to any database creates or updates.
    // 				   The rows to be created or updated are returned by the called
    // 				   action blocks in three repeating groups which are then passed to
    // 				   an additional action block which does the creates/updates.
    // 				   This eliminates ROLLBACKs if error are encountered during the
    // 				   prep work.
    // 05/03/11  GVandy	CQ27173	Set an abort exit state if an error is 
    // encountered in the
    // 				update_infrastructure cab.
    // 04/12/13  GVandy	CQ32829	Currently if more than one monitored activity is
    // found to close
    // 				then the infrastructure is set to Error status.  Now we will close
    // 				all the monitored activities found.
    // ------------------------------------------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.EabFileHandling.Action = "WRITE";

    // -------------------------------------------------------------------------------------
    // -- Get the run parameters for this program.
    // -------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEPB301";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Get the DB2 commit frequency counts.
    // -------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Determine if a program run entry has been created today for the event 
    // processor.
    // -- Initialize local program_error system_generated_id to the largest for 
    // this program run entry.
    // -- If creating a new program run then set the local program_error 
    // system_generated_id to to zero.
    // -------------------------------------------------------------------------------------
    if (ReadProgramRun())
    {
      // -- There is a program run entry for today.  Get the max program_error 
      // system_generated_id for that program run entry.
      local.ProgramRun.StartTimestamp = entities.ProgramRun.StartTimestamp;
      ReadProgramError();
    }
    else
    {
      // -- Initialize local program_error system_generated_id to 0.
      local.ProgramError.SystemGeneratedIdentifier = 0;

      // -- There is no program run entry created today.  Create a new program 
      // run entry.
      local.ProgramRun.FromRestartInd =
        local.ProgramCheckpointRestart.RestartInd ?? "";
      UseCreateProgramRun();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // -- Establish the log time threshold.  If processing time for any event 
    // exceeds
    // -- this threshold the event info will be logged to the control report.
    // --
    // -- The PPI parameter is in the format
    // -- WaitSecs=0180,LogTim=020,RunMode=FULL
    // -- LogTim is in Tenths of a second (e.g. 010 = 1 second, 005 = half 
    // second)
    // -------------------------------------------------------------------------------------
    local.LogTimeSeconds.Count =
      (int)StringToNumber(Substring(
        local.ProgramProcessingInfo.ParameterList, 22, 2));
    local.LogTimeMicroSeconds.Count =
      (int)(StringToNumber(
        Substring(local.ProgramProcessingInfo.ParameterList, 24, 1)) * (
        decimal)100000);

    // -- Extract Debug flag from the parameter list.
    local.Debug.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 48, 1);
    local.CheckpointNumbOfUpdates.Count = 0;
    export.EventsProcessed.Count = 0;

    // -------------------------------------------------------------------------------------
    // -- Log the start time to the control report.
    // -------------------------------------------------------------------------------------
    local.BatchTimestampWorkArea.TextTimestamp = "";
    local.BatchTimestampWorkArea.IefTimestamp = Now();
    UseLeCabConvertTimestamp();
    local.EabReportSend.RptDetail =
      local.BatchTimestampWorkArea.TextTimestamp + "  Event Processing Starting";
      
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    do
    {
      local.CursorReopenRequired.Flag = "N";

      // -------------------------------------------------------------------------------------
      // -- Process each infrastructure record in "Q" status.
      // -- For LOC (Locate) events, do not process until the reference_date is 
      // reached.
      // -------------------------------------------------------------------------------------
      foreach(var item in ReadInfrastructure())
      {
        if (AsChar(local.Debug.Flag) == 'Y')
        {
          local.BatchTimestampWorkArea.TextTimestamp = "";
          local.BatchTimestampWorkArea.IefTimestamp = Now();
          UseLeCabConvertTimestamp();
          local.EabReportSend.RptDetail =
            local.BatchTimestampWorkArea.TextTimestamp + "  Processing Inf Id: " +
            NumberToString
            (entities.Infrastructure.SystemGeneratedIdentifier, 7, 9);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "  Event: " + NumberToString
            (entities.Infrastructure.EventId, 11, 5) + "/" + entities
            .Infrastructure.ReasonCode;
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

            return;
          }
        }

        ExitState = "ACO_NN0000_ALL_OK";

        // --  NOTE: Don't initialize the local program_error 
        // system_generated_identifier
        local.BypassErrorReport.Flag = "";
        local.Alerts.Count = 0;
        local.CaseUnits.Count = 0;
        local.Events.Count = 0;

        for(local.StartMona.Index = 0; local.StartMona.Index < local
          .StartMona.Count; ++local.StartMona.Index)
        {
          if (!local.StartMona.CheckSize())
          {
            break;
          }

          local.StartMona.Item.StartMonaAssgnmnt.Count = 0;
        }

        local.StartMona.CheckIndex();
        local.StartMona.Count = 0;
        local.EndMona.Count = 0;
        local.Error.Count = 0;
        MoveInfrastructure2(entities.Infrastructure, local.Infrastructure);
        ++export.EventsProcessed.Count;
        ++local.CheckpointNumbOfUpdates.Count;
        local.Start.Timestamp = Now();

        // -- The following IF statement is added for control purposes.  In 
        // certain instances we will
        //    escape out of the IF statement and bypass the remaining statements
        // inside the IF.
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -------------------------------------------------------------------------------------
          // -- Find the Event and Event Detail for this infrastructure record.
          // -------------------------------------------------------------------------------------
          if (!ReadEventEventDetail())
          {
            // -- Put the error in the error group.  It will be logged to the 
            // program error table later.
            local.Error.Index = local.Error.Count;
            local.Error.CheckSize();

            local.Error.Update.ProgramError.ProgramError1 =
              "SP0000: Event detail was not found.";

            goto Test;
          }

          // -------------------------------------------------------------------------------------
          // -- Process the Event Detail Exception Routine.
          // -------------------------------------------------------------------------------------
          if (!IsEmpty(entities.EventDetail.ExceptionRoutine))
          {
            UseSpEventProcExceptnRoutine();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // -- Put the error in the error group.  It will be logged to the 
              // program error table later.
              local.Error.Index = local.Error.Count;
              local.Error.CheckSize();

              local.Error.Update.ProgramError.ProgramError1 =
                UseEabExtractExitStateMessage();
              local.ProgramError.KeyInfo = "Exception Processing";
              ExitState = "ACO_NN0000_ALL_OK";

              goto Test;
            }
          }

          // -------------------------------------------------------------------------------------
          // -- Determine if any monitored activities are to be started or ended
          // for this event.
          // -------------------------------------------------------------------------------------
          UseSpPrepMonitoredActivities();

          if (AsChar(local.BypassErrorReport.Flag) == 'Y')
          {
            // An error occurred but will not be written to the error report.
            // Some errors are deliberately omitted from the error report.  
            // Typically these have to do
            // with no service provider to which monitored activities can be 
            // assigned.  These records
            // are set to error status.  SRRUN013 resets them to Q status and 
            // the event processor
            // reprocesses them on up to 4 consecutive days in the hopes that a 
            // service provider is
            // assigned during that time.  More details can be found in the 
            // prep_monitored_activities cab.
            // For this situation we'll do two things.  First, set the last of 
            // the local error group back to zero
            // so that nothing is written to the error report.  Second, insure 
            // that infrastructure record
            // is updated with the process status value returned from the 
            // prep_monitored_activities cab.
            local.Error.Count = 0;

            goto Test;
          }
          else if (local.Error.Count > 0)
          {
            // -- One or more errors were returned in the error group.  They 
            // will be logged to the program error table later.
            goto Test;
          }

          if (AsChar(local.Debug.Flag) == 'Y')
          {
            // -- Log info about MONAs to be closed.
            if (local.EndMona.Count > 0)
            {
              local.EabReportSend.RptDetail = "Inf Sys Gen " + NumberToString
                (entities.Infrastructure.SystemGeneratedIdentifier, 15) + "  Closing MONA count = " +
                NumberToString(local.EndMona.Count, 15);
              UseCabControlReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                return;
              }
            }

            for(local.EndMona.Index = 0; local.EndMona.Index < local
              .EndMona.Count; ++local.EndMona.Index)
            {
              if (!local.EndMona.CheckSize())
              {
                break;
              }

              local.EabReportSend.RptDetail = "Inf Sys Gen " + NumberToString
                (entities.Infrastructure.SystemGeneratedIdentifier, 15) + "  Closing MONA " +
                NumberToString
                (local.EndMona.Item.End.SystemGeneratedIdentifier, 15);
              UseCabControlReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                return;
              }
            }

            local.EndMona.CheckIndex();
          }

          // -------------------------------------------------------------------------------------
          // -- Determine if a case unit lifecycle transformation should occur.
          // -------------------------------------------------------------------------------------
          if (AsChar(entities.EventDetail.LifecycleImpactCode) == 'Y')
          {
            UseSpPrepLifecycleTransformation();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // -- Put the error in the error group.  It will be logged to the 
              // program error table later.
              local.Error.Index = local.Error.Count;
              local.Error.CheckSize();

              local.Error.Update.ProgramError.ProgramError1 =
                UseEabExtractExitStateMessage();
              local.ProgramError.KeyInfo =
                "Lifecycle Transformation Processing";
              ExitState = "ACO_NN0000_ALL_OK";

              goto Test;
            }
          }

          // -------------------------------------------------------------------------------------
          // -- Determine if any alerts are to be raised for this event.
          // -------------------------------------------------------------------------------------
          UseSpPrepAlerts();

          if (local.Error.Count > 0)
          {
            // -- One or more errors were returned in the error group.  They 
            // will be logged to the program error table later.
            goto Test;
          }

          // -------------------------------------------------------------------------------------
          // -- Determine the new process status.  If alerts were created the 
          // new status will be
          // -- "O" (Optimize), else if the event_detail is set to log to diary 
          // then the status
          // -- will be "H" (History), otherwise it will be "P" (Processed).
          // -------------------------------------------------------------------------------------
          if (local.Alerts.Count > 0)
          {
            local.Infrastructure.ProcessStatus = "O";
          }
          else if (AsChar(entities.EventDetail.LogToDiaryInd) == 'Y')
          {
            local.Infrastructure.ProcessStatus = "H";
          }
          else
          {
            local.Infrastructure.ProcessStatus = "P";
          }

          // -------------------------------------------------------------------------------------
          // -- Apply all creates and updates (except to the infrastructure 
          // process_status)
          // -------------------------------------------------------------------------------------
          UseSpEventRelatedUpdates();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -------------------------------------------------------------------------------------
            // -- An error occurred during the creates/updates.  Rollback the 
            // changes.
            // -------------------------------------------------------------------------------------
            // -- Put the error in the error group.  It will be logged to the 
            // program error table later.
            local.Error.Index = local.Error.Count;
            local.Error.CheckSize();

            local.Error.Update.ProgramError.ProgramError1 =
              UseEabExtractExitStateMessage();
            local.ProgramError.KeyInfo = "Database Updates";

            // -- Log info about the rollback to control report.
            for(local.Common.Count = 1; local.Common.Count <= 2; ++
              local.Common.Count)
            {
              switch(local.Common.Count)
              {
                case 1:
                  local.BatchTimestampWorkArea.TextTimestamp = "";
                  local.BatchTimestampWorkArea.IefTimestamp = Now();
                  UseLeCabConvertTimestamp();
                  local.EabReportSend.RptDetail =
                    local.BatchTimestampWorkArea.TextTimestamp + "  ROLLBACK Inf Id: " +
                    NumberToString
                    (entities.Infrastructure.SystemGeneratedIdentifier, 7, 9) +
                    "  # Inf rolled back: " + NumberToString
                    (local.CheckpointNumbOfUpdates.Count, 11, 5);

                  break;
                case 2:
                  local.EabReportSend.RptDetail =
                    UseEabExtractExitStateMessage();

                  break;
                default:
                  break;
              }

              UseCabControlReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                return;
              }
            }

            ExitState = "ACO_NN0000_ALL_OK";
            UseEabRollbackSql();

            if (local.External.NumericReturnCode != 0)
            {
              ExitState = "CO0000_INVALID_ROLLBACK_RC_AB";

              return;
            }

            local.CursorReopenRequired.Flag = "Y";

            // -- Continue through the remaining logic which will log the error,
            // set the process status to
            //    "E" (Error), commit, and then re-open the cursor.
          }
        }

Test:

        // -------------------------------------------------------------------------------------
        // -- Create program_error entries for all non critical errors.
        // -- (These entries are then written to the event processor error 
        // report in SRRUN206.)
        // -------------------------------------------------------------------------------------
        if (local.Error.Count > 0)
        {
          // -- Set the process status to "E" (Error) and write all errors in 
          // the error group to the program error table.
          local.Infrastructure.ProcessStatus = "E";

          for(local.Error.Index = 0; local.Error.Index < local.Error.Count; ++
            local.Error.Index)
          {
            if (!local.Error.CheckSize())
            {
              break;
            }

            MoveProgramError2(local.Error.Item.ProgramError, local.ProgramError);
              
            ++local.ProgramError.SystemGeneratedIdentifier;

            // -- The key_info attribute contains info to identify the 
            // infrastructure record that caused the error.
            local.Textnum.Text15 =
              NumberToString(entities.Infrastructure.SystemGeneratedIdentifier,
              15);
            local.ProgramError.KeyInfo = "Inf Id " + Substring
              (local.Textnum.Text15, SpPrintWorkSet.Text15_MaxLength,
              Verify(local.Textnum.Text15, "0"), 16 -
              Verify(local.Textnum.Text15, "0")) + " " + (
                local.ProgramError.KeyInfo ?? "");
            local.ProgramError.KeyInfo = TrimEnd(local.ProgramError.KeyInfo) + " I.BO =" +
              entities.Infrastructure.BusinessObjectCd;
            local.Textnum.Text15 =
              NumberToString(entities.Infrastructure.EventId, 15);
            local.ProgramError.KeyInfo = TrimEnd(local.ProgramError.KeyInfo) + " I.Ev Id =" +
              Substring
              (local.Textnum.Text15, SpPrintWorkSet.Text15_MaxLength,
              Verify(local.Textnum.Text15, "0"), 16 -
              Verify(local.Textnum.Text15, "0"));
            local.ProgramError.KeyInfo = TrimEnd(local.ProgramError.KeyInfo) + " I.Rsn Cd =" +
              entities.Infrastructure.ReasonCode;
            local.ProgramError.KeyInfo = TrimEnd(local.ProgramError.KeyInfo) + " I.Ev Typ =" +
              entities.Infrastructure.EventType;
            local.ProgramError.KeyInfo = TrimEnd(local.ProgramError.KeyInfo) + " I.Case =" +
              entities.Infrastructure.CaseNumber;
            local.Textnum.Text15 =
              NumberToString(entities.Infrastructure.CaseUnitNumber.
                GetValueOrDefault(), 15);
            local.ProgramError.KeyInfo = TrimEnd(local.ProgramError.KeyInfo) + " I.CU =" +
              Substring
              (local.Textnum.Text15, SpPrintWorkSet.Text15_MaxLength,
              Verify(local.Textnum.Text15, "0"), 16 -
              Verify(local.Textnum.Text15, "0"));
            local.ProgramError.KeyInfo = TrimEnd(local.ProgramError.KeyInfo) + " I.Pers =" +
              entities.Infrastructure.CsePersonNumber;
            local.ProgramError.KeyInfo = TrimEnd(local.ProgramError.KeyInfo) + " I.Created By =" +
              entities.Infrastructure.CreatedBy;
            local.ProgramError.KeyInfo = TrimEnd(local.ProgramError.KeyInfo) + " I.Pgm =" +
              entities.Infrastructure.UserId;
            local.ProgramError.KeyInfo = TrimEnd(local.ProgramError.KeyInfo) + " I.E_Dtl =" +
              entities.Infrastructure.EventDetailName;
            local.ProgramError.KeyInfo = TrimEnd(local.ProgramError.KeyInfo) + " I.Dtl =" +
              entities.Infrastructure.Detail;
            UseCreateProgramError();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          local.Error.CheckIndex();
        }

        // -------------------------------------------------------------------------------------
        // -- Update the process_status on the infrastructure record.
        // -------------------------------------------------------------------------------------
        UseUpdateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // 05/03/11  GVandy	CQ27173	Set an abort exit state if an error is 
          // encountered in the update_infrastructure cab.
          // -- Log info about the error to control report and set an abort exit
          // state.
          for(local.Common.Count = 1; local.Common.Count <= 2; ++
            local.Common.Count)
          {
            switch(local.Common.Count)
            {
              case 1:
                local.BatchTimestampWorkArea.TextTimestamp = "";
                local.BatchTimestampWorkArea.IefTimestamp = Now();
                UseLeCabConvertTimestamp();
                local.EabReportSend.RptDetail =
                  local.BatchTimestampWorkArea.TextTimestamp + "  ABORT Inf Id: " +
                  NumberToString
                  (entities.Infrastructure.SystemGeneratedIdentifier, 7, 9) + "  Error in update_infrastructure cab.";
                  

                break;
              case 2:
                local.EabReportSend.RptDetail = UseEabExtractExitStateMessage();

                break;
              default:
                break;
            }

            UseCabControlReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

              return;
            }
          }

          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // -------------------------------------------------------------------------------------
        // -- If event processing time exceeded the LogTim parameter threshold 
        // then
        // -- log the event info to the control report.
        // -------------------------------------------------------------------------------------
        local.EndDateWorkArea.Timestamp = Now();

        if (Lt(AddMicroseconds(
          AddSeconds(local.Start.Timestamp, local.LogTimeSeconds.Count),
          local.LogTimeMicroSeconds.Count), local.EndDateWorkArea.Timestamp))
        {
          // -- Log info about the event and processing time to control report.
          local.BatchTimestampWorkArea.TextTimestamp = "";
          local.BatchTimestampWorkArea.IefTimestamp = Now();
          UseLeCabConvertTimestamp();
          local.EabReportSend.RptDetail =
            local.BatchTimestampWorkArea.TextTimestamp + "  LogTime Exceeded  Event: " +
            NumberToString(entities.Event1.ControlNumber, 11, 5) + "/" + NumberToString
            (entities.EventDetail.SystemGeneratedIdentifier, 13, 3);
          local.BatchTimestampWorkArea.TextTimestamp = "";
          local.BatchTimestampWorkArea.IefTimestamp =
            AddMicroseconds(AddSeconds(
              AddMinutes(AddHours(
              local.EndDateWorkArea.Timestamp, -Hour(local.Start.Timestamp)), -
            Minute(local.Start.Timestamp)), -Second(local.Start.Timestamp)), -
            Microsecond(local.Start.Timestamp));
          UseLeCabConvertTimestamp();
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " Time: " + Substring
            (local.BatchTimestampWorkArea.TextTimestamp,
            BatchTimestampWorkArea.TextTimestamp_MaxLength, 15, 12);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " Inf ID: " + NumberToString
            (entities.Infrastructure.SystemGeneratedIdentifier, 7, 9);
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

            return;
          }
        }

        if (local.CheckpointNumbOfUpdates.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() ||
          AsChar(local.CursorReopenRequired.Flag) == 'Y')
        {
          // -- Log the time of this commit.
          UseUpdatePgmCheckpointRestart();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          // -- External DB2 commit.
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ACO_RE0000_EXT_DO_DB2_COMMIT_AB";

            return;
          }

          // --  Read the PPI and check for END.
          UseReadProgramProcessingInfo();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (Equal(local.ProgramProcessingInfo.ParameterList, 34, 7, "END    "))
            
          {
            return;
          }

          local.CheckpointNumbOfUpdates.Count = 0;
        }

        if (AsChar(local.CursorReopenRequired.Flag) == 'Y')
        {
          // -- A rollback was done due to errors in the event_related_updates 
          // cab.  The process
          //    status of the infrastructure record was then set to "E" and a 
          // commit performed.
          //    Therefore, we need to re-open the cursor so that we pick up all 
          // the events that got
          //    rolled back during that process.  It is very important that the 
          // events are processed in
          //    created timestamp order.
          goto Next;
        }
      }

Next:
      ;
    }
    while(AsChar(local.CursorReopenRequired.Flag) != 'N');

    // -------------------------------------------------------------------------------------
    // -- Log ending time to control report
    // -------------------------------------------------------------------------------------
    local.BatchTimestampWorkArea.TextTimestamp = "";
    local.BatchTimestampWorkArea.IefTimestamp = Now();
    UseLeCabConvertTimestamp();
    local.EabReportSend.RptDetail =
      local.BatchTimestampWorkArea.TextTimestamp + "  Event Processing Ending.    # Processed: " +
      NumberToString(export.EventsProcessed.Count, 9, 7);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Record Program End Time
    // -------------------------------------------------------------------------------------
    UseUpdateProgramRun();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Do a final commit.
    // -------------------------------------------------------------------------------------
    // --Log time of this commit.
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -- External DB2 commit.
    UseExtToDoACommit();

    if (local.PassArea.NumericReturnCode != 0)
    {
      ExitState = "ACO_RE0000_EXT_DO_DB2_COMMIT_AB";
    }
  }

  private static void MoveAlerts1(Local.AlertsGroup source,
    SpEventProcExceptnRoutine.Export.ImportExportAlertsGroup target)
  {
    target.Goffice.SystemGeneratedId = source.Goffice.SystemGeneratedId;
    MoveOfficeServiceProvider(source.GofficeServiceProvider,
      target.GofficeServiceProvider);
    target.GserviceProvider.SystemGeneratedId =
      source.GserviceProvider.SystemGeneratedId;
    target.GofficeServiceProviderAlert.
      Assign(source.GofficeServiceProviderAlert);
  }

  private static void MoveAlerts2(Local.AlertsGroup source,
    SpPrepAlerts.Export.ImportExportAlertsGroup target)
  {
    target.Goffice.SystemGeneratedId = source.Goffice.SystemGeneratedId;
    MoveOfficeServiceProvider(source.GofficeServiceProvider,
      target.GofficeServiceProvider);
    target.GserviceProvider.SystemGeneratedId =
      source.GserviceProvider.SystemGeneratedId;
    target.GofficeServiceProviderAlert.
      Assign(source.GofficeServiceProviderAlert);
  }

  private static void MoveAlerts3(Local.AlertsGroup source,
    SpEventRelatedUpdates.Import.AlertsGroup target)
  {
    target.Goffice.SystemGeneratedId = source.Goffice.SystemGeneratedId;
    MoveOfficeServiceProvider(source.GofficeServiceProvider,
      target.GofficeServiceProvider);
    target.GserviceProvider.SystemGeneratedId =
      source.GserviceProvider.SystemGeneratedId;
    target.GofficeServiceProviderAlert.
      Assign(source.GofficeServiceProviderAlert);
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCaseUnit(CaseUnit source, CaseUnit target)
  {
    target.CuNumber = source.CuNumber;
    target.State = source.State;
  }

  private static void MoveCaseUnits1(Local.CaseUnitsGroup source,
    SpEventProcExceptnRoutine.Export.ImportExportCaseUnitsGroup target)
  {
    target.Case1.Number = source.Case1.Number;
    MoveCaseUnit(source.CaseUnit, target.CaseUnit);
  }

  private static void MoveCaseUnits2(Local.CaseUnitsGroup source,
    SpPrepLifecycleTransformation.Export.ImportExportCaseUnitsGroup target)
  {
    target.Case1.Number = source.Case1.Number;
    MoveCaseUnit(source.CaseUnit, target.CaseUnit);
  }

  private static void MoveCaseUnits3(Local.CaseUnitsGroup source,
    SpEventRelatedUpdates.Import.CaseUnitsGroup target)
  {
    target.Case1.Number = source.Case1.Number;
    MoveCaseUnit(source.CaseUnit, target.CaseUnit);
  }

  private static void MoveEndMona1(Local.EndMonaGroup source,
    SpEventRelatedUpdates.Import.EndMonaGroup target)
  {
    target.End.Assign(source.End);
  }

  private static void MoveEndMona2(Local.EndMonaGroup source,
    SpPrepMonitoredActivities.Export.ImportExportEndMonaGroup target)
  {
    target.End.Assign(source.End);
  }

  private static void MoveError1(SpPrepAlerts.Export.ErrorGroup source,
    Local.ErrorGroup target)
  {
    MoveProgramError2(source.ProgramError, target.ProgramError);
  }

  private static void MoveError2(SpPrepMonitoredActivities.Export.
    ErrorGroup source, Local.ErrorGroup target)
  {
    MoveProgramError2(source.ProgramError, target.ProgramError);
  }

  private static void MoveEvent1(Event1 source, Event1 target)
  {
    target.ControlNumber = source.ControlNumber;
    target.BusinessObjectCode = source.BusinessObjectCode;
  }

  private static void MoveEventDetail(EventDetail source, EventDetail target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.LogToDiaryInd = source.LogToDiaryInd;
  }

  private static void MoveEvents1(Local.EventsGroup source,
    SpEventProcExceptnRoutine.Export.ImportExportEventsGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveEvents2(Local.EventsGroup source,
    SpPrepLifecycleTransformation.Export.ImportExportEventsGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveEvents3(Local.EventsGroup source,
    SpEventRelatedUpdates.Import.EventsGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveImportExportAlerts1(SpEventProcExceptnRoutine.Export.
    ImportExportAlertsGroup source, Local.AlertsGroup target)
  {
    target.Goffice.SystemGeneratedId = source.Goffice.SystemGeneratedId;
    MoveOfficeServiceProvider(source.GofficeServiceProvider,
      target.GofficeServiceProvider);
    target.GserviceProvider.SystemGeneratedId =
      source.GserviceProvider.SystemGeneratedId;
    target.GofficeServiceProviderAlert.
      Assign(source.GofficeServiceProviderAlert);
  }

  private static void MoveImportExportAlerts2(SpPrepAlerts.Export.
    ImportExportAlertsGroup source, Local.AlertsGroup target)
  {
    target.Goffice.SystemGeneratedId = source.Goffice.SystemGeneratedId;
    MoveOfficeServiceProvider(source.GofficeServiceProvider,
      target.GofficeServiceProvider);
    target.GserviceProvider.SystemGeneratedId =
      source.GserviceProvider.SystemGeneratedId;
    target.GofficeServiceProviderAlert.
      Assign(source.GofficeServiceProviderAlert);
  }

  private static void MoveImportExportCaseUnits1(SpEventProcExceptnRoutine.
    Export.ImportExportCaseUnitsGroup source, Local.CaseUnitsGroup target)
  {
    target.Case1.Number = source.Case1.Number;
    MoveCaseUnit(source.CaseUnit, target.CaseUnit);
  }

  private static void MoveImportExportCaseUnits2(SpPrepLifecycleTransformation.
    Export.ImportExportCaseUnitsGroup source, Local.CaseUnitsGroup target)
  {
    target.Case1.Number = source.Case1.Number;
    MoveCaseUnit(source.CaseUnit, target.CaseUnit);
  }

  private static void MoveImportExportEndMona(SpPrepMonitoredActivities.Export.
    ImportExportEndMonaGroup source, Local.EndMonaGroup target)
  {
    target.End.Assign(source.End);
  }

  private static void MoveImportExportEvents1(SpEventProcExceptnRoutine.Export.
    ImportExportEventsGroup source, Local.EventsGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveImportExportEvents2(SpPrepLifecycleTransformation.
    Export.ImportExportEventsGroup source, Local.EventsGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveImportExportStartMona(SpPrepMonitoredActivities.
    Export.ImportExportStartMonaGroup source, Local.StartMonaGroup target)
  {
    target.StartMona1.Assign(source.StartMona);
    source.StartMonaAssgnmnt.CopyTo(
      target.StartMonaAssgnmnt, MoveStartMonaAssgnmnt3);
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.CreatedBy = source.CreatedBy;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ProcessStatus = source.ProcessStatus;
  }

  private static void MoveInfrastructure3(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveInfrastructure4(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveProgramCheckpointRestart1(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.RestartInd = source.RestartInd;
  }

  private static void MoveProgramCheckpointRestart2(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.RestartInd = source.RestartInd;
  }

  private static void MoveProgramError1(ProgramError source, ProgramError target)
    
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.KeyInfo = source.KeyInfo;
    target.ProgramError1 = source.ProgramError1;
  }

  private static void MoveProgramError2(ProgramError source, ProgramError target)
    
  {
    target.KeyInfo = source.KeyInfo;
    target.ProgramError1 = source.ProgramError1;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveStartMona1(Local.StartMonaGroup source,
    SpEventRelatedUpdates.Import.StartMonaGroup target)
  {
    target.StartMona1.Assign(source.StartMona1);
    source.StartMonaAssgnmnt.CopyTo(
      target.StartMonaAssgnmnt, MoveStartMonaAssgnmnt1);
  }

  private static void MoveStartMona2(Local.StartMonaGroup source,
    SpPrepMonitoredActivities.Export.ImportExportStartMonaGroup target)
  {
    target.StartMona.Assign(source.StartMona1);
    source.StartMonaAssgnmnt.CopyTo(
      target.StartMonaAssgnmnt, MoveStartMonaAssgnmnt2);
  }

  private static void MoveStartMonaAssgnmnt1(Local.
    StartMonaAssgnmntGroup source,
    SpEventRelatedUpdates.Import.StartMonaAssgnmntGroup target)
  {
    target.StartMonaMonitoredActivityAssignment.Assign(
      source.StartMonaMonitoredActivityAssignment);
    MoveOfficeServiceProvider(source.StartMonaOfficeServiceProvider,
      target.StartMonaOfficeServiceProvider);
    target.StartMonaServiceProvider.SystemGeneratedId =
      source.StartMonaServiceProvider.SystemGeneratedId;
    target.StartMonaOffice.SystemGeneratedId =
      source.StartMonaOffice.SystemGeneratedId;
  }

  private static void MoveStartMonaAssgnmnt2(Local.
    StartMonaAssgnmntGroup source,
    SpPrepMonitoredActivities.Export.StartMonaAssgnmntGroup target)
  {
    target.StartMonaMonitoredActivityAssignment.Assign(
      source.StartMonaMonitoredActivityAssignment);
    MoveOfficeServiceProvider(source.StartMonaOfficeServiceProvider,
      target.StartMonaOfficeServiceProvider);
    target.StartMonaServiceProvider.SystemGeneratedId =
      source.StartMonaServiceProvider.SystemGeneratedId;
    target.StartMonaOffice.SystemGeneratedId =
      source.StartMonaOffice.SystemGeneratedId;
  }

  private static void MoveStartMonaAssgnmnt3(SpPrepMonitoredActivities.Export.
    StartMonaAssgnmntGroup source, Local.StartMonaAssgnmntGroup target)
  {
    target.StartMonaMonitoredActivityAssignment.Assign(
      source.StartMonaMonitoredActivityAssignment);
    MoveOfficeServiceProvider(source.StartMonaOfficeServiceProvider,
      target.StartMonaOfficeServiceProvider);
    target.StartMonaServiceProvider.SystemGeneratedId =
      source.StartMonaServiceProvider.SystemGeneratedId;
    target.StartMonaOffice.SystemGeneratedId =
      source.StartMonaOffice.SystemGeneratedId;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCreateProgramError()
  {
    var useImport = new CreateProgramError.Import();
    var useExport = new CreateProgramError.Export();

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    MoveProgramError1(local.ProgramError, useImport.ProgramError);
    useImport.ProgramRun.StartTimestamp = local.ProgramRun.StartTimestamp;

    Call(CreateProgramError.Execute, useImport, useExport);
  }

  private void UseCreateProgramRun()
  {
    var useImport = new CreateProgramRun.Import();
    var useExport = new CreateProgramRun.Export();

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ProgramRun.FromRestartInd = local.ProgramRun.FromRestartInd;

    Call(CreateProgramRun.Execute, useImport, useExport);

    local.ProgramRun.StartTimestamp = useExport.ProgramRun.StartTimestamp;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(EabRollbackSql.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart2(useExport.ProgramCheckpointRestart,
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

  private void UseSpEventProcExceptnRoutine()
  {
    var useImport = new SpEventProcExceptnRoutine.Import();
    var useExport = new SpEventProcExceptnRoutine.Export();

    useImport.Infrastructure.Assign(entities.Infrastructure);
    useImport.EventDetail.ExceptionRoutine =
      entities.EventDetail.ExceptionRoutine;
    local.Alerts.CopyTo(useExport.ImportExportAlerts, MoveAlerts1);
    local.CaseUnits.CopyTo(useExport.ImportExportCaseUnits, MoveCaseUnits1);
    local.Events.CopyTo(useExport.ImportExportEvents, MoveEvents1);

    Call(SpEventProcExceptnRoutine.Execute, useImport, useExport);

    useExport.ImportExportAlerts.CopyTo(local.Alerts, MoveImportExportAlerts1);
    useExport.ImportExportCaseUnits.CopyTo(
      local.CaseUnits, MoveImportExportCaseUnits1);
    useExport.ImportExportEvents.CopyTo(local.Events, MoveImportExportEvents1);
  }

  private void UseSpEventRelatedUpdates()
  {
    var useImport = new SpEventRelatedUpdates.Import();
    var useExport = new SpEventRelatedUpdates.Export();

    local.EndMona.CopyTo(useImport.EndMona, MoveEndMona1);
    local.StartMona.CopyTo(useImport.StartMona, MoveStartMona1);
    local.CaseUnits.CopyTo(useImport.CaseUnits, MoveCaseUnits3);
    local.Events.CopyTo(useImport.Events, MoveEvents3);
    useImport.Infrastructure.SystemGeneratedIdentifier =
      entities.Infrastructure.SystemGeneratedIdentifier;
    local.Alerts.CopyTo(useImport.Alerts, MoveAlerts3);

    Call(SpEventRelatedUpdates.Execute, useImport, useExport);
  }

  private void UseSpPrepAlerts()
  {
    var useImport = new SpPrepAlerts.Import();
    var useExport = new SpPrepAlerts.Export();

    useImport.Event1.ControlNumber = entities.Event1.ControlNumber;
    useImport.EventDetail.SystemGeneratedIdentifier =
      entities.EventDetail.SystemGeneratedIdentifier;
    MoveInfrastructure1(entities.Infrastructure, useImport.Infrastructure);
    useImport.Current.Date = local.Current.Date;
    local.Alerts.CopyTo(useExport.ImportExportAlerts, MoveAlerts2);

    Call(SpPrepAlerts.Execute, useImport, useExport);

    useExport.ImportExportAlerts.CopyTo(local.Alerts, MoveImportExportAlerts2);
    useExport.Error.CopyTo(local.Error, MoveError1);
  }

  private void UseSpPrepLifecycleTransformation()
  {
    var useImport = new SpPrepLifecycleTransformation.Import();
    var useExport = new SpPrepLifecycleTransformation.Export();

    useImport.Event1.ControlNumber = entities.Event1.ControlNumber;
    useImport.EventDetail.SystemGeneratedIdentifier =
      entities.EventDetail.SystemGeneratedIdentifier;
    MoveInfrastructure4(entities.Infrastructure, useImport.Infrastructure);
    local.CaseUnits.CopyTo(useExport.ImportExportCaseUnits, MoveCaseUnits2);
    local.Events.CopyTo(useExport.ImportExportEvents, MoveEvents2);

    Call(SpPrepLifecycleTransformation.Execute, useImport, useExport);

    useExport.ImportExportCaseUnits.CopyTo(
      local.CaseUnits, MoveImportExportCaseUnits2);
    useExport.ImportExportEvents.CopyTo(local.Events, MoveImportExportEvents2);
  }

  private void UseSpPrepMonitoredActivities()
  {
    var useImport = new SpPrepMonitoredActivities.Import();
    var useExport = new SpPrepMonitoredActivities.Export();

    useImport.Current.Date = local.Current.Date;
    MoveInfrastructure3(entities.Infrastructure, useImport.Infrastructure);
    MoveEventDetail(entities.EventDetail, useImport.EventDetail);
    MoveEvent1(entities.Event1, useImport.Event1);
    local.StartMona.CopyTo(useExport.ImportExportStartMona, MoveStartMona2);
    local.EndMona.CopyTo(useExport.ImportExportEndMona, MoveEndMona2);

    Call(SpPrepMonitoredActivities.Execute, useImport, useExport);

    useExport.Error.CopyTo(local.Error, MoveError2);
    useExport.ImportExportStartMona.CopyTo(
      local.StartMona, MoveImportExportStartMona);
    local.BypassErrorReport.Flag = useExport.BypassErrorReport.Flag;
    local.Infrastructure.ProcessStatus = useExport.Infrastructure.ProcessStatus;
    useExport.ImportExportEndMona.
      CopyTo(local.EndMona, MoveImportExportEndMona);
  }

  private void UseUpdateInfrastructure()
  {
    var useImport = new UpdateInfrastructure.Import();
    var useExport = new UpdateInfrastructure.Export();

    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);

    Call(UpdateInfrastructure.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    MoveProgramCheckpointRestart1(local.ProgramCheckpointRestart,
      useImport.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseUpdateProgramRun()
  {
    var useImport = new UpdateProgramRun.Import();
    var useExport = new UpdateProgramRun.Export();

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ProgramRun.StartTimestamp = local.ProgramRun.StartTimestamp;

    Call(UpdateProgramRun.Execute, useImport, useExport);
  }

  private bool ReadEventEventDetail()
  {
    entities.Event1.Populated = false;
    entities.EventDetail.Populated = false;

    return Read("ReadEventEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", entities.Infrastructure.EventId);
        db.SetString(command, "reasonCode", entities.Infrastructure.ReasonCode);
      },
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.Event1.BusinessObjectCode = db.GetString(reader, 1);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.EventDetail.ReasonCode = db.GetString(reader, 3);
        entities.EventDetail.LifecycleImpactCode = db.GetString(reader, 4);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 5);
        entities.EventDetail.ExceptionRoutine = db.GetNullableString(reader, 6);
        entities.Event1.Populated = true;
        entities.EventDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "referenceDate", date);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private bool ReadProgramError()
  {
    System.Diagnostics.Debug.Assert(entities.ProgramRun.Populated);
    local.ProgramError.Populated = false;

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
        local.ProgramError.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        local.ProgramError.Populated = true;
      });
  }

  private bool ReadProgramRun()
  {
    entities.ProgramRun.Populated = false;

    return Read("ReadProgramRun",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "ppiName", local.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "ppiCreatedTstamp",
          local.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
        db.SetDate(command, "currentDate", date);
      },
      (db, reader) =>
      {
        entities.ProgramRun.PpiCreatedTstamp = db.GetDateTime(reader, 0);
        entities.ProgramRun.PpiName = db.GetString(reader, 1);
        entities.ProgramRun.StartTimestamp = db.GetDateTime(reader, 2);
        entities.ProgramRun.Populated = true;
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
    /// A value of EventsProcessed.
    /// </summary>
    [JsonPropertyName("eventsProcessed")]
    public Common EventsProcessed
    {
      get => eventsProcessed ??= new();
      set => eventsProcessed = value;
    }

    private Common eventsProcessed;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A AlertsGroup group.</summary>
    [Serializable]
    public class AlertsGroup
    {
      /// <summary>
      /// A value of Goffice.
      /// </summary>
      [JsonPropertyName("goffice")]
      public Office Goffice
      {
        get => goffice ??= new();
        set => goffice = value;
      }

      /// <summary>
      /// A value of GofficeServiceProvider.
      /// </summary>
      [JsonPropertyName("gofficeServiceProvider")]
      public OfficeServiceProvider GofficeServiceProvider
      {
        get => gofficeServiceProvider ??= new();
        set => gofficeServiceProvider = value;
      }

      /// <summary>
      /// A value of GserviceProvider.
      /// </summary>
      [JsonPropertyName("gserviceProvider")]
      public ServiceProvider GserviceProvider
      {
        get => gserviceProvider ??= new();
        set => gserviceProvider = value;
      }

      /// <summary>
      /// A value of GofficeServiceProviderAlert.
      /// </summary>
      [JsonPropertyName("gofficeServiceProviderAlert")]
      public OfficeServiceProviderAlert GofficeServiceProviderAlert
      {
        get => gofficeServiceProviderAlert ??= new();
        set => gofficeServiceProviderAlert = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Office goffice;
      private OfficeServiceProvider gofficeServiceProvider;
      private ServiceProvider gserviceProvider;
      private OfficeServiceProviderAlert gofficeServiceProviderAlert;
    }

    /// <summary>A CaseUnitsGroup group.</summary>
    [Serializable]
    public class CaseUnitsGroup
    {
      /// <summary>
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of CaseUnit.
      /// </summary>
      [JsonPropertyName("caseUnit")]
      public CaseUnit CaseUnit
      {
        get => caseUnit ??= new();
        set => caseUnit = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 case1;
      private CaseUnit caseUnit;
    }

    /// <summary>A EventsGroup group.</summary>
    [Serializable]
    public class EventsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Infrastructure G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Infrastructure g;
    }

    /// <summary>A StartMonaGroup group.</summary>
    [Serializable]
    public class StartMonaGroup
    {
      /// <summary>
      /// A value of StartMona1.
      /// </summary>
      [JsonPropertyName("startMona1")]
      public MonitoredActivity StartMona1
      {
        get => startMona1 ??= new();
        set => startMona1 = value;
      }

      /// <summary>
      /// Gets a value of StartMonaAssgnmnt.
      /// </summary>
      [JsonIgnore]
      public Array<StartMonaAssgnmntGroup> StartMonaAssgnmnt =>
        startMonaAssgnmnt ??= new(StartMonaAssgnmntGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of StartMonaAssgnmnt for json serialization.
      /// </summary>
      [JsonPropertyName("startMonaAssgnmnt")]
      [Computed]
      public IList<StartMonaAssgnmntGroup> StartMonaAssgnmnt_Json
      {
        get => startMonaAssgnmnt;
        set => StartMonaAssgnmnt.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private MonitoredActivity startMona1;
      private Array<StartMonaAssgnmntGroup> startMonaAssgnmnt;
    }

    /// <summary>A StartMonaAssgnmntGroup group.</summary>
    [Serializable]
    public class StartMonaAssgnmntGroup
    {
      /// <summary>
      /// A value of StartMonaMonitoredActivityAssignment.
      /// </summary>
      [JsonPropertyName("startMonaMonitoredActivityAssignment")]
      public MonitoredActivityAssignment StartMonaMonitoredActivityAssignment
      {
        get => startMonaMonitoredActivityAssignment ??= new();
        set => startMonaMonitoredActivityAssignment = value;
      }

      /// <summary>
      /// A value of StartMonaOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("startMonaOfficeServiceProvider")]
      public OfficeServiceProvider StartMonaOfficeServiceProvider
      {
        get => startMonaOfficeServiceProvider ??= new();
        set => startMonaOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of StartMonaServiceProvider.
      /// </summary>
      [JsonPropertyName("startMonaServiceProvider")]
      public ServiceProvider StartMonaServiceProvider
      {
        get => startMonaServiceProvider ??= new();
        set => startMonaServiceProvider = value;
      }

      /// <summary>
      /// A value of StartMonaOffice.
      /// </summary>
      [JsonPropertyName("startMonaOffice")]
      public Office StartMonaOffice
      {
        get => startMonaOffice ??= new();
        set => startMonaOffice = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private MonitoredActivityAssignment startMonaMonitoredActivityAssignment;
      private OfficeServiceProvider startMonaOfficeServiceProvider;
      private ServiceProvider startMonaServiceProvider;
      private Office startMonaOffice;
    }

    /// <summary>A EndMonaGroup group.</summary>
    [Serializable]
    public class EndMonaGroup
    {
      /// <summary>
      /// A value of End.
      /// </summary>
      [JsonPropertyName("end")]
      public MonitoredActivity End
      {
        get => end ??= new();
        set => end = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 750;

      private MonitoredActivity end;
    }

    /// <summary>A ErrorGroup group.</summary>
    [Serializable]
    public class ErrorGroup
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private ProgramError programError;
    }

    /// <summary>
    /// A value of Debug.
    /// </summary>
    [JsonPropertyName("debug")]
    public Common Debug
    {
      get => debug ??= new();
      set => debug = value;
    }

    /// <summary>
    /// A value of EndBatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("endBatchTimestampWorkArea")]
    public BatchTimestampWorkArea EndBatchTimestampWorkArea
    {
      get => endBatchTimestampWorkArea ??= new();
      set => endBatchTimestampWorkArea = value;
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
    /// A value of LogTimeMicroSeconds.
    /// </summary>
    [JsonPropertyName("logTimeMicroSeconds")]
    public Common LogTimeMicroSeconds
    {
      get => logTimeMicroSeconds ??= new();
      set => logTimeMicroSeconds = value;
    }

    /// <summary>
    /// A value of LogTimeSeconds.
    /// </summary>
    [JsonPropertyName("logTimeSeconds")]
    public Common LogTimeSeconds
    {
      get => logTimeSeconds ??= new();
      set => logTimeSeconds = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
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
    /// A value of CursorReopenRequired.
    /// </summary>
    [JsonPropertyName("cursorReopenRequired")]
    public Common CursorReopenRequired
    {
      get => cursorReopenRequired ??= new();
      set => cursorReopenRequired = value;
    }

    /// <summary>
    /// A value of BypassErrorReport.
    /// </summary>
    [JsonPropertyName("bypassErrorReport")]
    public Common BypassErrorReport
    {
      get => bypassErrorReport ??= new();
      set => bypassErrorReport = value;
    }

    /// <summary>
    /// Gets a value of Alerts.
    /// </summary>
    [JsonIgnore]
    public Array<AlertsGroup> Alerts => alerts ??= new(AlertsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Alerts for json serialization.
    /// </summary>
    [JsonPropertyName("alerts")]
    [Computed]
    public IList<AlertsGroup> Alerts_Json
    {
      get => alerts;
      set => Alerts.Assign(value);
    }

    /// <summary>
    /// Gets a value of CaseUnits.
    /// </summary>
    [JsonIgnore]
    public Array<CaseUnitsGroup> CaseUnits => caseUnits ??= new(
      CaseUnitsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CaseUnits for json serialization.
    /// </summary>
    [JsonPropertyName("caseUnits")]
    [Computed]
    public IList<CaseUnitsGroup> CaseUnits_Json
    {
      get => caseUnits;
      set => CaseUnits.Assign(value);
    }

    /// <summary>
    /// Gets a value of Events.
    /// </summary>
    [JsonIgnore]
    public Array<EventsGroup> Events => events ??= new(EventsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Events for json serialization.
    /// </summary>
    [JsonPropertyName("events")]
    [Computed]
    public IList<EventsGroup> Events_Json
    {
      get => events;
      set => Events.Assign(value);
    }

    /// <summary>
    /// Gets a value of StartMona.
    /// </summary>
    [JsonIgnore]
    public Array<StartMonaGroup> StartMona => startMona ??= new(
      StartMonaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of StartMona for json serialization.
    /// </summary>
    [JsonPropertyName("startMona")]
    [Computed]
    public IList<StartMonaGroup> StartMona_Json
    {
      get => startMona;
      set => StartMona.Assign(value);
    }

    /// <summary>
    /// Gets a value of EndMona.
    /// </summary>
    [JsonIgnore]
    public Array<EndMonaGroup> EndMona => endMona ??= new(
      EndMonaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of EndMona for json serialization.
    /// </summary>
    [JsonPropertyName("endMona")]
    [Computed]
    public IList<EndMonaGroup> EndMona_Json
    {
      get => endMona;
      set => EndMona.Assign(value);
    }

    /// <summary>
    /// Gets a value of Error.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorGroup> Error => error ??= new(ErrorGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Error for json serialization.
    /// </summary>
    [JsonPropertyName("error")]
    [Computed]
    public IList<ErrorGroup> Error_Json
    {
      get => error;
      set => Error.Assign(value);
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

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
    /// A value of Textnum.
    /// </summary>
    [JsonPropertyName("textnum")]
    public SpPrintWorkSet Textnum
    {
      get => textnum ??= new();
      set => textnum = value;
    }

    /// <summary>
    /// A value of EndDateWorkArea.
    /// </summary>
    [JsonPropertyName("endDateWorkArea")]
    public DateWorkArea EndDateWorkArea
    {
      get => endDateWorkArea ??= new();
      set => endDateWorkArea = value;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    private Common debug;
    private BatchTimestampWorkArea endBatchTimestampWorkArea;
    private Common common;
    private Common logTimeMicroSeconds;
    private Common logTimeSeconds;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramError programError;
    private ProgramRun programRun;
    private Common checkpointNumbOfUpdates;
    private Common cursorReopenRequired;
    private Common bypassErrorReport;
    private Array<AlertsGroup> alerts;
    private Array<CaseUnitsGroup> caseUnits;
    private Array<EventsGroup> events;
    private Array<StartMonaGroup> startMona;
    private Array<EndMonaGroup> endMona;
    private Array<ErrorGroup> error;
    private Infrastructure infrastructure;
    private DateWorkArea start;
    private External external;
    private SpPrintWorkSet textnum;
    private DateWorkArea endDateWorkArea;
    private External passArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private BatchTimestampWorkArea batchTimestampWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    private ProgramRun programRun;
    private ProgramError programError;
    private ProgramProcessingInfo programProcessingInfo;
    private Infrastructure infrastructure;
    private Event1 event1;
    private EventDetail eventDetail;
  }
#endregion
}
