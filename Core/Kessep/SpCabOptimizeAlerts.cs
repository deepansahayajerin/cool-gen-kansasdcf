// Program: SP_CAB_OPTIMIZE_ALERTS, ID: 372067482, model: 746.
// Short name: SWE01762
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
/// A program: SP_CAB_OPTIMIZE_ALERTS.
/// </para>
/// <para>
/// This CAB is intended to optimize the Office Service Provider Alerts (OSP 
/// alerts) that are to be sent as a  result of the imported Infrastructure
/// occurance.
/// There are four rules of Alert Optimization:
/// 1.	Based upon the root Infrastructure record's 'created by', delete all
/// 	occurences of OSP Alert for this situation / case / userid.  Basically,
/// 	the USER who created the alert will not receive it.
/// 2.	For current situation/case/userid, delete all but lowest prioritization
/// 	codes (except 9)
/// 3.	For current situation number, if optimization ind = 'Y', and userid has
/// 	multiple OSP Alerts (&lt;> 9), delete all OSP alerts except for first one
/// 	(&lt;> 9).
/// 4.	Alerts are not received for closed cases
/// </para>
/// </summary>
[Serializable]
public partial class SpCabOptimizeAlerts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_OPTIMIZE_ALERTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabOptimizeAlerts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabOptimizeAlerts.
  /// </summary>
  public SpCabOptimizeAlerts(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date		Developer	Request #
    // 10Jan 97	Alan Samuels	Initial Dev
    // 10Jan99       John C Crook	Assessment
    // Rule # 2 corrected
    // 03/22/01	swsrchf		I00114719
    // Rule #3 corrected
    // 05/30/01	SWSRPRM		PR # 119416
    // Created rule # 4 => New alerts should not be received for
    // closed cases on event 10 and certain event details.
    // 06/04/01	SWSRPRM		PR # 121092
    // Rule # 3b created for AE alerts.
    // 02/2002		SWSRPRM
    // PR # 137502 => Event 47 processing time.
    // 06/2002		SWSRPRM
    // PR # 140563 => Alert for second case for same SVPO was
    // deleted.
    // 04/22/09	GVandy
    // CQ 9788  Complete re-design to improve performance.
    // 04/15/10	GVandy
    // CQ 966	New index on INFRASTRUCTURE process_status eliminates
    // the need to read using created_timestamp > timestamp of the
    // last run of the EP.
    // 04/15/10	GVandy
    // CQ 339	Check for existence of exception routine PREV_CC on the
    // event detail when determining if alerts raised on closed cases should
    // be retained.
    // ------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";

    // -------------------------------------------------------------------------------------
    // -- Get the run parameters for this program.
    // -------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEP301B";
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
    // -- Determine if a program run entry has been created today for the alert 
    // optimizer.
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
      // -- There is no program run entry created today.  Create a new program 
      // run entry.
      local.ProgramRun.FromRestartInd =
        local.ProgramCheckpointRestart.RestartInd ?? "";
      UseCreateProgramRun();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // -- Initialize local program_error system_generated_id to 0.
      local.ProgramError.SystemGeneratedIdentifier = 0;
    }

    // -------------------------------------------------------------------------------------
    // -- Log the start time to the control report.
    // -------------------------------------------------------------------------------------
    local.BatchTimestampWorkArea.TextTimestamp = "";
    local.BatchTimestampWorkArea.IefTimestamp = Now();
    UseLeCabConvertTimestamp();
    local.EabReportSend.RptDetail =
      local.BatchTimestampWorkArea.TextTimestamp + "  Alert Optimization Starting";
      
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    local.CheckpointNumbOfUpdates.Count = 0;
    export.AlertsProcessed.Count = 0;

    // -------------------------------------------------------------------------------------
    // -- Optimize CSE alerts.
    // -------------------------------------------------------------------------------------
    foreach(var item in ReadOfficeServiceProviderAlertInfrastructure2())
    {
      ++export.AlertsProcessed.Count;

      // 4/29/09 Removed "and desired office_service_provider_alert 
      // optimized_flag is less than '3'" from the read each.
      // Otherwise may not pick up the alerts with optimization_ind = 'N'
      if (local.CheckpointNumbOfUpdates.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() && !
        Equal(entities.OfficeServiceProviderAlert.SituationIdentifier,
        local.PreviousOfficeServiceProviderAlert.SituationIdentifier))
      {
        // -- Note: All alerts tied to the situation number MUST be optimized 
        // before commiting since the underlying infrastructure record
        //    status will have been changed from 'O' and would therefore not be 
        // picked up in the event of a restart.
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

          return;
        }

        local.CheckpointNumbOfUpdates.Count = 0;
      }

      ++local.CheckpointNumbOfUpdates.Count;
      local.ProgramError.ProgramError1 = "";

      // -------------------------------------------------------------------------------------
      // -- Determine if the alert should be optimized (i.e. deleted).
      // -------------------------------------------------------------------------------------
      local.DeleteAlert.Flag = "N";

      if (!IsEmpty(entities.Infrastructure.CaseNumber) && entities
        .Infrastructure.EventId != 47 && Equal
        (entities.Infrastructure.CreatedBy,
        entities.OfficeServiceProviderAlert.RecipientUserId))
      {
        // -- If we ever change to not process by O status then may need to 
        // exclude event 20 since the document process creates
        //    history records in H status and it also directly creates alerts 
        // with user_id = created by on the infrastructure record.
        // ----------------------------------------------------------------------
        // Rule #1:
        // Based on the root Infrastructure record's 'created by',
        // remove all occurances of OSP Alert for this situation/
        // case/userid.
        // ----------------------------------------------------------------------
        // -- Delete if alert is for same user that created the infrastructure 
        // record.
        local.DeleteAlert.Flag = "Y";
      }
      else if (Equal(entities.OfficeServiceProviderAlert.SituationIdentifier,
        local.PreviousOfficeServiceProviderAlert.SituationIdentifier) && Equal
        (entities.OfficeServiceProviderAlert.RecipientUserId,
        local.PreviousOfficeServiceProviderAlert.RecipientUserId) && !
        IsEmpty(entities.Infrastructure.CaseNumber) && Equal
        (entities.Infrastructure.CaseNumber,
        local.PreviousInfrastructure.CaseNumber) && AsChar
        (entities.OfficeServiceProviderAlert.OptimizationInd) == 'Y' && Lt
        (entities.OfficeServiceProviderAlert.PrioritizationCode, 9))
      {
        // ----------------------------------------------------------------------
        // Rule #2 & #3 now combined...
        // Rule #2:
        // For current situation/case/userid, delete all but lowest
        // prioritization codes (except 9)
        // Rule 3:
        // For current situation number, if optimization ind = 'Y',
        // and userid has multiple OSP Alerts (<> 9), delete all
        // OSP alerts except for first one (<> 9).
        // ----------------------------------------------------------------------
        local.DeleteAlert.Flag = "Y";
      }
      else if (!IsEmpty(entities.Infrastructure.CaseNumber))
      {
        // -------------------------------------------------------------------------------
        // Rule # 4: No subsequent alerts will be received after a case
        // has been closed with the following exceptions:
        // ====  If the AP is known to an open and a closed case and
        // ====  either case gets a verified locate and the exception routine
        // ====  is PREV_CC on the corresponding event detail.
        // -------------------------------------------------------------------------------
        // 04/15/10  GVandy
        // CQ 339	Check for existence of exception routine PREV_CC on the event 
        // detail when
        // determining if alerts raised on closed cases should be retained.  
        // This replaces a prior
        // hardcoded list of event details.
        local.CreateForClosedCase.Flag = "N";

        if (!ReadEventDetail2())
        {
          ExitState = "SP0000_EVENT_DETAIL_NF_ABORT";

          return;
        }

        if (Equal(entities.EventDetail.ExceptionRoutine, "PREV_CC"))
        {
          foreach(var item1 in ReadCase2())
          {
            if (Equal(entities.Case1.ClosureReason, "NL"))
            {
              local.NoLocate.Flag = "Y";
            }

            if (AsChar(entities.Case1.Status) == 'O')
            {
              local.CreateForClosedCase.SelectChar = "Y";
            }

            if (AsChar(local.CreateForClosedCase.SelectChar) == 'Y' && AsChar
              (local.NoLocate.Flag) == 'Y')
            {
              local.CreateForClosedCase.Flag = "Y";

              goto Test;
            }
          }
        }

Test:

        if (AsChar(local.CreateForClosedCase.Flag) == 'Y')
        {
        }
        else if (ReadCase1())
        {
          if (AsChar(entities.Case1.Status) == 'C')
          {
            local.DeleteAlert.Flag = "Y";
          }
        }
        else
        {
          // Set error message.  Error wil be logged below.
          local.ProgramError.ProgramError1 = "Case " + entities
            .Infrastructure.CaseNumber + " not found.";
        }
      }

      if (!IsEmpty(local.ProgramError.ProgramError1))
      {
        // -- A non-critical error was encountered.  Log the error.
        ++local.ProgramError.SystemGeneratedIdentifier;
        local.ProgramError.KeyInfo = "Infrastructure ID: " + NumberToString
          (entities.Infrastructure.SystemGeneratedIdentifier, 15);
        UseCreateProgramError();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
      else if (AsChar(local.DeleteAlert.Flag) == 'Y')
      {
        // -- The alert is to be optimized (i.e. deleted).
        if (ReadOfficeServiceProviderAlert())
        {
          DeleteOfficeServiceProviderAlert();
        }
        else
        {
          ExitState = "SP0000_ALERT_NF_FOR_OPTIMIZATION";

          return;
        }
      }
      else
      {
        if (Equal(entities.Infrastructure.CreatedBy, "SWEPB305") && (
          Equal(entities.Infrastructure.ReasonCode, "AEADDRCHNG") || Equal
          (entities.Infrastructure.ReasonCode, "KSCADDRCHNG")))
        {
          // ----------------------------------------------------------------
          // Rule # 3b => AE alerts
          // Due to the unique way the records are created by
          // SWEPB305 (Run 95), these records cannot be optimized
          // using Rule # 3 as a stand alone.
          // In the event more than two records exists on the case for the
          // day, the hierarcy of processing the alert is AR - AP - CH.
          // This applies to Event 24 and Event Details 379 & 385 only.
          // ----------------------------------------------------------------
          // -- Set a flag indicating that AE alerts were found.  These alerts 
          // will be processed later.
          local.AeAlertsExist.Flag = "Y";

          continue;
        }

        MoveOfficeServiceProviderAlert2(entities.OfficeServiceProviderAlert,
          local.PreviousOfficeServiceProviderAlert);
        local.PreviousInfrastructure.Assign(entities.Infrastructure);

        // -- The alert has passed all the edits.  Set the optimized flag to "3"
        // which signifies
        //    that the alert has completed the optimization process.
        local.PassTo.Assign(entities.OfficeServiceProviderAlert);
        local.PassTo.OptimizedFlag = "3";
        UseSpCabUpdateOspAlertOptFlag();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // -- Update infrastructure process status if it has not already been 
      // updated.
      //    We may have already changed the status when processing a prior alert
      // record, since many alerts may be
      //    tied to one infrastructure record.
      if (AsChar(entities.Infrastructure.ProcessStatus) == 'O' || !
        IsEmpty(local.ProgramError.ProgramError1) && AsChar
        (entities.Infrastructure.ProcessStatus) != 'E')
      {
        // -- At this point the alert has either been deleted or updated to show
        // that optimization is complete.
        // -- Change the process status on the infrastructure record to signify 
        // that alert optimization has been completed.
        MoveInfrastructure(entities.Infrastructure, local.Infrastructure);

        if (!IsEmpty(local.ProgramError.ProgramError1))
        {
          local.Infrastructure.ProcessStatus = "E";
        }
        else
        {
          if (!ReadEventDetail1())
          {
            ExitState = "SP0000_EVENT_DETAIL_NF_ABORT";

            return;
          }

          if (AsChar(entities.EventDetail.LogToDiaryInd) == 'Y')
          {
            local.Infrastructure.ProcessStatus = "H";
          }
          else
          {
            local.Infrastructure.ProcessStatus = "P";
          }
        }

        UseUpdateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }

    // -------------------------------------------------------------------------------------
    // -- Optimize AE alerts.
    // -------------------------------------------------------------------------------------
    if (AsChar(local.AeAlertsExist.Flag) == 'Y')
    {
      // ----------------------------------------------------------------
      // Rule # 3b => AE alerts
      // Due to the unique way the records are created by
      // SWEPB305 (Run 95), these records cannot be optimized
      // using Rule # 3 as a stand alone.
      // In the event more than two records exists on the case for the
      // day, the hierarcy of processing the alert is AR - AP - CH.
      // This applies to Event 24 and Event Details 379 & 385 only.
      // ----------------------------------------------------------------
      local.PreviousInfrastructure.Assign(local.NullInfrastructure);
      MoveOfficeServiceProviderAlert2(local.NullOfficeServiceProviderAlert,
        local.PreviousOfficeServiceProviderAlert);

      // -- Read each AE alert sorted by case, denorm_text_12 field.  SWEPB305 
      // creates these alerts and sets
      //    the denorm text 12 field to 1 for AR/MO role, 2 for AP/FA role, and 
      // 3 for CH role.  Since AR/MO takes
      //    precedence over AP/FA which takes precedence over CH we just order 
      // by this value and delete all but
      //    the first alert for the case.
      foreach(var item in ReadOfficeServiceProviderAlertInfrastructure1())
      {
        // ------------------------------------------------------------------------------------------------------------------
        // 4/29/09 Removed "and desired office_service_provider_alert 
        // optimized_flag is less than '3'" from the read each.
        // ------------------------------------------------------------------------------------------------------------------
        if (local.CheckpointNumbOfUpdates.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() &&
          !
          Equal(entities.Infrastructure.CaseNumber,
          local.PreviousInfrastructure.CaseNumber))
        {
          // -- Note: All alerts tied to the user id and case number MUST be 
          // optimized before commiting.
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

            return;
          }

          local.CheckpointNumbOfUpdates.Count = 0;
        }

        ++local.CheckpointNumbOfUpdates.Count;

        if (Equal(entities.OfficeServiceProviderAlert.RecipientUserId,
          local.PreviousOfficeServiceProviderAlert.RecipientUserId) && !
          IsEmpty(entities.Infrastructure.CaseNumber) && Equal
          (entities.Infrastructure.CaseNumber,
          local.PreviousInfrastructure.CaseNumber) && Equal
          (entities.Infrastructure.ReasonCode,
          local.PreviousInfrastructure.ReasonCode) && AsChar
          (entities.OfficeServiceProviderAlert.OptimizationInd) == 'Y')
        {
          // -- The alert is to be optimized (i.e. deleted).
          if (ReadOfficeServiceProviderAlert())
          {
            DeleteOfficeServiceProviderAlert();
          }
          else
          {
            ExitState = "SP0000_ALERT_NF_FOR_OPTIMIZATION";

            return;
          }
        }
        else
        {
          // -- The alert will be kept.  Set the optimized flag to 3 to signify 
          // that the alert has completed the optimization process.
          MoveOfficeServiceProviderAlert2(entities.OfficeServiceProviderAlert,
            local.PreviousOfficeServiceProviderAlert);
          local.PreviousInfrastructure.Assign(entities.Infrastructure);
          local.PassTo.Assign(entities.OfficeServiceProviderAlert);
          local.PassTo.OptimizedFlag = "3";
          UseSpCabUpdateOspAlertOptFlag();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        // -- At this point the alert has either been deleted or updated to show
        // that optimization is complete.
        // -- Change the process status on the infrastructure record to signify 
        // that alert optimization has been completed.
        MoveInfrastructure(entities.Infrastructure, local.Infrastructure);

        if (!ReadEventDetail1())
        {
          ExitState = "SP0000_EVENT_DETAIL_NF_ABORT";

          return;
        }

        if (AsChar(entities.EventDetail.LogToDiaryInd) == 'Y')
        {
          local.Infrastructure.ProcessStatus = "H";
        }
        else
        {
          local.Infrastructure.ProcessStatus = "P";
        }

        UseUpdateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }

    // -------------------------------------------------------------------------------------
    // -- Log ending time to control report
    // -------------------------------------------------------------------------------------
    local.BatchTimestampWorkArea.TextTimestamp = "";
    local.BatchTimestampWorkArea.IefTimestamp = Now();
    UseLeCabConvertTimestamp();
    local.EabReportSend.RptDetail =
      local.BatchTimestampWorkArea.TextTimestamp + "  Alert Optimization Ending.  # Processed: " +
      NumberToString(export.AlertsProcessed.Count, 9, 7);
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

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ProcessStatus = source.ProcessStatus;
  }

  private static void MoveOfficeServiceProviderAlert1(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OptimizedFlag = source.OptimizedFlag;
  }

  private static void MoveOfficeServiceProviderAlert2(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.SituationIdentifier = source.SituationIdentifier;
    target.RecipientUserId = source.RecipientUserId;
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

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCreateProgramError()
  {
    var useImport = new CreateProgramError.Import();
    var useExport = new CreateProgramError.Export();

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ProgramRun.StartTimestamp = local.ProgramRun.StartTimestamp;
    useImport.ProgramError.Assign(local.ProgramError);

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

  private void UseSpCabUpdateOspAlertOptFlag()
  {
    var useImport = new SpCabUpdateOspAlertOptFlag.Import();
    var useExport = new SpCabUpdateOspAlertOptFlag.Export();

    MoveOfficeServiceProviderAlert1(local.PassTo,
      useImport.OfficeServiceProviderAlert);

    Call(SpCabUpdateOspAlertOptFlag.Execute, useImport, useExport);
  }

  private void UseUpdateInfrastructure()
  {
    var useImport = new UpdateInfrastructure.Import();
    var useExport = new UpdateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

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

  private void DeleteOfficeServiceProviderAlert()
  {
    Update("DeleteOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.Optimized.SystemGeneratedIdentifier);
      });
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadEventDetail1()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.Infrastructure.EventId);
        db.SetString(command, "reasonCode", entities.Infrastructure.ReasonCode);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.ReasonCode = db.GetString(reader, 1);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 2);
        entities.EventDetail.EveNo = db.GetInt32(reader, 3);
        entities.EventDetail.ExceptionRoutine = db.GetNullableString(reader, 4);
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadEventDetail2()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.Infrastructure.EventId);
        db.SetString(command, "reasonCode", entities.Infrastructure.ReasonCode);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.ReasonCode = db.GetString(reader, 1);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 2);
        entities.EventDetail.EveNo = db.GetInt32(reader, 3);
        entities.EventDetail.ExceptionRoutine = db.GetNullableString(reader, 4);
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderAlert()
  {
    entities.Optimized.Populated = false;

    return Read("ReadOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Optimized.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Optimized.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderAlertInfrastructure1()
  {
    entities.OfficeServiceProviderAlert.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadOfficeServiceProviderAlertInfrastructure1",
      null,
      (db, reader) =>
      {
        entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProviderAlert.TypeCode = db.GetString(reader, 1);
        entities.OfficeServiceProviderAlert.SituationIdentifier =
          db.GetString(reader, 2);
        entities.OfficeServiceProviderAlert.PrioritizationCode =
          db.GetNullableInt32(reader, 3);
        entities.OfficeServiceProviderAlert.OptimizationInd =
          db.GetNullableString(reader, 4);
        entities.OfficeServiceProviderAlert.OptimizedFlag =
          db.GetNullableString(reader, 5);
        entities.OfficeServiceProviderAlert.RecipientUserId =
          db.GetString(reader, 6);
        entities.OfficeServiceProviderAlert.CreatedBy = db.GetString(reader, 7);
        entities.OfficeServiceProviderAlert.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.OfficeServiceProviderAlert.InfId =
          db.GetNullableInt32(reader, 9);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 10);
        entities.Infrastructure.EventId = db.GetInt32(reader, 11);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 12);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 16);
        entities.OfficeServiceProviderAlert.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderAlertInfrastructure2()
  {
    entities.OfficeServiceProviderAlert.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadOfficeServiceProviderAlertInfrastructure2",
      null,
      (db, reader) =>
      {
        entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProviderAlert.TypeCode = db.GetString(reader, 1);
        entities.OfficeServiceProviderAlert.SituationIdentifier =
          db.GetString(reader, 2);
        entities.OfficeServiceProviderAlert.PrioritizationCode =
          db.GetNullableInt32(reader, 3);
        entities.OfficeServiceProviderAlert.OptimizationInd =
          db.GetNullableString(reader, 4);
        entities.OfficeServiceProviderAlert.OptimizedFlag =
          db.GetNullableString(reader, 5);
        entities.OfficeServiceProviderAlert.RecipientUserId =
          db.GetString(reader, 6);
        entities.OfficeServiceProviderAlert.CreatedBy = db.GetString(reader, 7);
        entities.OfficeServiceProviderAlert.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.OfficeServiceProviderAlert.InfId =
          db.GetNullableInt32(reader, 9);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 10);
        entities.Infrastructure.EventId = db.GetInt32(reader, 11);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 12);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 16);
        entities.OfficeServiceProviderAlert.Populated = true;
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
    /// A value of AlertsProcessed.
    /// </summary>
    [JsonPropertyName("alertsProcessed")]
    public Common AlertsProcessed
    {
      get => alertsProcessed ??= new();
      set => alertsProcessed = value;
    }

    private Common alertsProcessed;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of PreviousOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("previousOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert PreviousOfficeServiceProviderAlert
    {
      get => previousOfficeServiceProviderAlert ??= new();
      set => previousOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of DeleteAlert.
    /// </summary>
    [JsonPropertyName("deleteAlert")]
    public Common DeleteAlert
    {
      get => deleteAlert ??= new();
      set => deleteAlert = value;
    }

    /// <summary>
    /// A value of PreviousInfrastructure.
    /// </summary>
    [JsonPropertyName("previousInfrastructure")]
    public Infrastructure PreviousInfrastructure
    {
      get => previousInfrastructure ??= new();
      set => previousInfrastructure = value;
    }

    /// <summary>
    /// A value of CreateForClosedCase.
    /// </summary>
    [JsonPropertyName("createForClosedCase")]
    public Common CreateForClosedCase
    {
      get => createForClosedCase ??= new();
      set => createForClosedCase = value;
    }

    /// <summary>
    /// A value of NoLocate.
    /// </summary>
    [JsonPropertyName("noLocate")]
    public Common NoLocate
    {
      get => noLocate ??= new();
      set => noLocate = value;
    }

    /// <summary>
    /// A value of AeAlertsExist.
    /// </summary>
    [JsonPropertyName("aeAlertsExist")]
    public Common AeAlertsExist
    {
      get => aeAlertsExist ??= new();
      set => aeAlertsExist = value;
    }

    /// <summary>
    /// A value of PassTo.
    /// </summary>
    [JsonPropertyName("passTo")]
    public OfficeServiceProviderAlert PassTo
    {
      get => passTo ??= new();
      set => passTo = value;
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
    /// A value of NullInfrastructure.
    /// </summary>
    [JsonPropertyName("nullInfrastructure")]
    public Infrastructure NullInfrastructure
    {
      get => nullInfrastructure ??= new();
      set => nullInfrastructure = value;
    }

    /// <summary>
    /// A value of NullOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("nullOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert NullOfficeServiceProviderAlert
    {
      get => nullOfficeServiceProviderAlert ??= new();
      set => nullOfficeServiceProviderAlert = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramRun programRun;
    private ProgramError programError;
    private Common checkpointNumbOfUpdates;
    private External passArea;
    private OfficeServiceProviderAlert previousOfficeServiceProviderAlert;
    private Common deleteAlert;
    private Infrastructure previousInfrastructure;
    private Common createForClosedCase;
    private Common noLocate;
    private Common aeAlertsExist;
    private OfficeServiceProviderAlert passTo;
    private Infrastructure infrastructure;
    private Infrastructure nullInfrastructure;
    private OfficeServiceProviderAlert nullOfficeServiceProviderAlert;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Optimized.
    /// </summary>
    [JsonPropertyName("optimized")]
    public OfficeServiceProviderAlert Optimized
    {
      get => optimized ??= new();
      set => optimized = value;
    }

    private ProgramRun programRun;
    private ProgramError programError;
    private ProgramProcessingInfo programProcessingInfo;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private Infrastructure infrastructure;
    private EventDetail eventDetail;
    private Event1 event1;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private OfficeServiceProviderAlert optimized;
  }
#endregion
}
