// Program: SI_B245_INTERFACE_PERSON_PGM, ID: 371787285, model: 746.
// Short name: SWEI245B
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
/// A program: SI_B245_INTERFACE_PERSON_PGM.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB245InterfacePersonPgm: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B245_INTERFACE_PERSON_PGM program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB245InterfacePersonPgm(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB245InterfacePersonPgm.
  /// </summary>
  public SiB245InterfacePersonPgm(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date    Developer       Request #       Description
    // 06/04/96  G. Lofton         		Initial Development
    // 07/24/97  Siraj Konkader
    // 1. Added logic per original spec to process only the last received record
    // for same Person, Case and Program.
    // 2. Changed restart logic to use Person/Program/CreatedTimestamp
    // 3. Added Program Error Key Info
    // 4. Added logic so that the program will automatically restart after an 
    // error was encountered with the next Person/Program group.
    // 5. Added call to Extract Exit State CAB to display error messages.
    // 11/03/1997	Siraj Konkader		Removed above IDCR 377 related code as it was 
    // incorrect.
    // Implemented IDCR 377
    // 1. Using recently added attr. modified program to restart automatically 
    // after an error with a commit freq > 1
    // 2. Provide for Revert to Open Scenario in which a previously closed ADC 
    // program is reopened due to which an NA program if opened (automatic
    // rollover) needs to be shut down
    // ----------------------------------------------------------
    // ***************************************************************
    // 11/11/1998   C. Ott      Modified procedure for DIR compliance.  Added
    //                          calls of CAB WRITE ERROR and CAB WRITE CONTROL.
    //                          IDCR # 449, added 2 attributes to Interface 
    // Person
    //                          program.
    // **********************************************************
    // ***************************************************************
    // 11/15/1998   C. Ott      Added a REPEAT loop in order to make the
    //                          procedure compliant with DIR standards for Error
    //                          and Control reports.  This REPEAT allows 
    // rollback
    //                          and re-opening of a cursor without exiting the
    //                          program and re-starting.  This permits a single
    //                          error report.
    // ****************************************************************
    // ***************************************************************
    // 12/2/1998    C. Ott      Modified PrAD to eliminate second call of
    //                          SI_BATCH_PROCESS_INTRFC_PRSN_PGM.  Rollover 
    // letters
    //                          will be printed during processing of Interface
    //                          record.
    // **********************************************************
    // ***************************************************************
    // 05/21/1999    C. Ott     Added action block to create CSENet notification
    //                          of Case Type change
    // **********************************************************
    // ***************************************************************
    // 08/20/1999    C. Ott     Modified to correct error in EM and WT
    //                          participation code processing
    // **********************************************************
    // ***************************************************************
    // 08/23/1999    C. Ott     Restructured rollback - restart logic
    // **********************************************************
    // ***************************************************************
    // 11/30/1999    C. Ott     Changes for Work Request # 7.
    // **********************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ****************************************************************
    // Get the run parameters for this program.
    // ****************************************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
    }

    // ****************************************************************
    // Get the DB2 commit frequency counts.
    // ****************************************************************
    // ****************************************************************
    //  Find out if we are in a restart situation.
    // ****************************************************************
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
    }

    // ****************************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // ****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ****************************************************************
    // OPEN OUTPUT CONTROL REPORT 99
    // ****************************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ProgramCheckpointRestart.CheckpointCount = 0;
    local.ProcessDate.ProcMonth = local.ProgramProcessingInfo.ProcessDate;

    // ****************************************************************
    // Set up pointer record to start procesing the interface Person Program 
    // table.
    // ****************************************************************
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.CheckpointRestartKey.CsePersonNumber =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
      local.CheckpointRestartKey.ProgramCode =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 11, 3);
      local.CheckpointRestartKey.CreatedTimestamp =
        Timestamp(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 14, 26));
    }

    // ***************************************************************
    // Initialize Control Report counts.
    // ***************************************************************
    local.CheckpointNumbOfReads.Count = 0;
    local.CheckpointNumbOfUpdates.Count = 0;
    local.CntlTotPersPgmCreated.Count = 0;
    local.AdcRolloverLetterSent.Count = 0;
    local.PaRolloverLetterSent.Count = 0;
    local.CntlTotAdcCaseClosed.Count = 0;
    local.CntlTotAdcCaseOpened.Count = 0;
    local.CntlTotCsePersUpdates.Count = 0;
    local.CntlTotPersPgmClosed.Count = 0;
    local.CntlTotPersPgmCreated.Count = 0;
    local.CntlTotRecordsProcessed.Count = 0;

    // ****************************************************************
    //  Process the selected records in groups based upon the commit 
    // frequencies.  Do a DB2 commit at the end of each group.
    // ***************************************************************
    // ***************************************************************
    // 11/15/1998   C. Ott    Added the REPEAT loop in order to make the 
    // procedure compliant with DIR standards for Error and Control reports.
    // This REPEAT allows rollback and re-opening of a cursor without exiting 
    // the program and re-starting.  This permits a single error report.
    // ****************************************************************
    // ***************************************************************
    // 11/29/1999   C. Ott   Changed the sort on timestamp from descending to 
    // ascending for WR # 7.
    // ****************************************************************
    do
    {
      local.RollbackRestartIndicator.Flag = "N";
      local.ReadSuccessful.Flag = "N";

      foreach(var item in ReadInterfacePersonProgram())
      {
        if (Equal(export.RollbackRestartOnly.CreatedTimestamp,
          entities.InterfacePersonProgram.CreatedTimestamp) && Equal
          (export.RollbackRestartOnly.CsePersonNumber,
          entities.InterfacePersonProgram.CsePersonNumber) && Equal
          (export.RollbackRestartOnly.ProgramCode,
          entities.InterfacePersonProgram.ProgramCode))
        {
          // ***************************************************************
          // This was a record that was previously encountered as an error and 
          // caused a rollback.
          // ****************************************************************
          try
          {
            UpdateInterfacePersonProgram();
            local.PerformCommit.Flag = "Y";
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error updating Interface Person Progam Status Ind for CSE Person " +
                  entities.InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                  entities.InterfacePersonProgram.ProgramCode;
                UseCabErrorReport1();

                if (Equal(local.EabFileHandling.Status, "OK"))
                {
                }
                else
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  goto AfterCycle;
                }

                break;
              case ErrorCode.PermittedValueViolation:
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error updating Interface Person Progam Status Ind for CSE Person " +
                  entities.InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                  entities.InterfacePersonProgram.ProgramCode;
                UseCabErrorReport1();

                if (Equal(local.EabFileHandling.Status, "OK"))
                {
                }
                else
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  goto AfterCycle;
                }

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
          local.ReadSuccessful.Flag = "Y";
          ++local.CntlTotRecordsProcessed.Count;
          ++local.CheckpointNumbOfReads.Count;

          // ***************************************************************
          // Added IF and Read Each stmts to process only the last record from 
          // the set of similar records having the same Person, Case and
          // Program.
          // **************************************************************
          // ***********************************************
          // Include the status_ind in the read each because it
          // was picking the error record and going into a loop.
          // Also ignore the processed records.
          // **********************************************
          // ***************************************************************
          // 11/29/1999   C. Ott   Changed the sort on timestamp from descending
          // to ascending for WR # 7.
          // ****************************************************************
          local.InterfacePersonProgram.Assign(entities.InterfacePersonProgram);

          // ****************************************************************
          // 8/19/99  C. Ott  Check for EM or WT transactions and set 
          // Participation Code
          // ****************************************************************
          if (Equal(local.InterfacePersonProgram.MedType, "EM") || Equal
            (local.InterfacePersonProgram.MedType, "WT"))
          {
            local.InterfacePersonProgram.ParticipationCode =
              local.InterfacePersonProgram.MedType ?? Spaces(2);
          }

          local.DatabaseUpdated.Flag = "";
          ExitState = "ACO_NN0000_ALL_OK";
          UseSiBatchProcessIntrfcPrsnPgm();

          // **************************************************************
          // Here you must interrogate every exit state to determine what should
          // be
          // done. For critical errors that need to abend the program, set an 
          // abort
          // exit state and escape. For non-critical errors you may write an 
          // error
          // record to the program error entity type.  The exit state must be 
          // turned
          // into an error code that is a valid value on the codes entity type.
          // *****************************************************************
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ***************************************************************
            // Save the identifiers for next READ EACH after an error in order 
            // to bypass record.
            // ***************************************************************
            MoveInterfacePersonProgram(entities.InterfacePersonProgram,
              export.RollbackRestartOnly);

            if (IsExitState("SP0000_EVENT_DETAIL_NF"))
            {
              switch(TrimEnd(local.Error.Message))
              {
                case "MOCLOSE":
                  local.EabReportSend.RptDetail =
                    "Event detail for closing Medical Only case was not found for CSE Person " +
                    entities.InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                    entities.InterfacePersonProgram.ProgramCode;

                  break;
                case "FOSTERCLOSE":
                  local.EabReportSend.RptDetail =
                    "Event detail for closing Foster Care case was not found for CSE Person " +
                    entities.InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                    entities.InterfacePersonProgram.ProgramCode;

                  break;
                case "AECCERROR":
                  local.EabReportSend.RptDetail =
                    "Event detail for deleting a KAE or KSC case was not found for CSE Person " +
                    entities.InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                    entities.InterfacePersonProgram.ProgramCode;

                  break;
                case "AEDENIED":
                  local.EabReportSend.RptDetail =
                    "Event detail for KAE denying a case was not found for CSE Person " +
                    entities.InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                    entities.InterfacePersonProgram.ProgramCode;

                  break;
                default:
                  local.EabReportSend.RptDetail =
                    "Event detail was not found for CSE Person " + entities
                    .InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                    entities.InterfacePersonProgram.ProgramCode;

                  break;
              }
            }
            else if (IsExitState("INTERSTATE_CASE_NF"))
            {
              local.EabReportSend.RptDetail =
                "Error encountered attempting to send CSENet notification, Person number = " +
                entities.InterfacePersonProgram.CsePersonNumber;
            }
            else if (IsExitState("CSE_PERSON_NU"))
            {
              local.EabReportSend.RptDetail =
                "Database error encountered attempting to update CSE Person, Person number = " +
                entities.InterfacePersonProgram.CsePersonNumber;
            }
            else if (IsExitState("CSE_PERSON_PV"))
            {
              local.EabReportSend.RptDetail =
                "Database error encountered attempting to update CSE Person, Person number = " +
                entities.InterfacePersonProgram.CsePersonNumber;
            }
            else if (IsExitState("AR_DB_ERROR_NF"))
            {
              local.EabReportSend.RptDetail =
                "Case Role was not found for CSE Person number = " + entities
                .InterfacePersonProgram.CsePersonNumber;
            }
            else if (IsExitState("CSE_PERSON_NF"))
            {
              local.EabReportSend.RptDetail =
                "CSE Person not found, Person number = " + entities
                .InterfacePersonProgram.CsePersonNumber;
            }
            else if (IsExitState("PROGRAM_NF"))
            {
              local.EabReportSend.RptDetail =
                "Program not found, Program Code = " + entities
                .InterfacePersonProgram.ProgramCode;
            }
            else if (IsExitState("PERSON_PROGRAM_AE"))
            {
              local.EabReportSend.RptDetail =
                "Person program already exists for Person Number " + entities
                .InterfacePersonProgram.CsePersonNumber + ", Program Code = " +
                entities.InterfacePersonProgram.ProgramCode;
            }
            else if (IsExitState("PERSON_PROGRAM_NU"))
            {
              local.EabReportSend.RptDetail =
                "Person program already exists for Person Number " + entities
                .InterfacePersonProgram.CsePersonNumber + ", Program Code = " +
                entities.InterfacePersonProgram.ProgramCode;
            }
            else if (IsExitState("PERSON_PROGRAM_NF"))
            {
              local.EabReportSend.RptDetail =
                "Error encountered attempting to Update Person Program, Person Program was not found for CSE Person " +
                entities.InterfacePersonProgram.CsePersonNumber + ", Program Code = " +
                entities.InterfacePersonProgram.ProgramCode;
            }
            else if (IsExitState("PERSON_PROGRAM_PV"))
            {
              local.EabReportSend.RptDetail =
                "Permitted Value error for Person Number " + entities
                .InterfacePersonProgram.CsePersonNumber + ", Program Code = " +
                entities.InterfacePersonProgram.ProgramCode;
            }
            else if (IsExitState("SI0000_DISCONTINUE_DATE_BLANK"))
            {
              local.EabReportSend.RptDetail =
                "The Discontinue Date is blank on Interface Person Program for Person Number " +
                entities.InterfacePersonProgram.CsePersonNumber + ", Program Code = " +
                entities.InterfacePersonProgram.ProgramCode;
            }
            else if (IsExitState("SI0000_INTERFACE_PERSON_PRGM_NF"))
            {
              local.EabReportSend.RptDetail =
                "Error updating Interface Person Progam Status Ind for CSE Person " +
                entities.InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                entities.InterfacePersonProgram.ProgramCode;
            }
            else if (IsExitState("SI0000_INTERFACE_PERSON_PRGM_NU"))
            {
              local.EabReportSend.RptDetail =
                "Error updating Interface Person Progam Status Ind for CSE Person " +
                entities.InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                entities.InterfacePersonProgram.ProgramCode;
            }
            else if (IsExitState("SI0000_INTERFACE_PERSON_PRGM_PV"))
            {
              local.EabReportSend.RptDetail =
                "Error updating Interface Person Progam Status Ind for CSE Person " +
                entities.InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                entities.InterfacePersonProgram.ProgramCode;
            }
            else if (IsExitState("SI0000_PGM_AND_PART_CODE_NOT_DEF"))
            {
              local.EabReportSend.RptDetail =
                "Program and participation code are not defined for Person number " +
                entities.InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                entities.InterfacePersonProgram.ProgramCode + " Participation Code " +
                entities.InterfacePersonProgram.ParticipationCode;
            }
            else if (IsExitState("EFFECTIVE_DATE_REQUIRED"))
            {
              local.EabReportSend.RptDetail =
                "Program effective date is not present for Person Number " + entities
                .InterfacePersonProgram.CsePersonNumber + ", Program Code = " +
                entities.InterfacePersonProgram.ProgramCode;
            }
            else if (IsExitState("SI0000_PARTICIPATION_CDE_NOT_DEF"))
            {
              local.EabReportSend.RptDetail = "Participation Code " + entities
                .InterfacePersonProgram.ParticipationCode + ", is not defined for Person number " +
                entities.InterfacePersonProgram.CsePersonNumber + " Program Code " +
                entities.InterfacePersonProgram.ProgramCode;
            }
            else if (IsExitState("SI0000_REVERT_TO_OPEN_NF"))
            {
              local.EabReportSend.RptDetail =
                "Person Program not found during attempt to revert to Open for Person Number " +
                entities.InterfacePersonProgram.CsePersonNumber + ", Program Code = " +
                entities.InterfacePersonProgram.ProgramCode;
            }
            else if (IsExitState("INVALID_PROGRAM"))
            {
              local.EabReportSend.RptDetail =
                "No previous AF or MA program, unable to create MED TYPE for Person Number " +
                entities.InterfacePersonProgram.CsePersonNumber + ", Medical Type = " +
                entities.InterfacePersonProgram.MedType;
            }
            else
            {
              local.EabReportSend.RptDetail =
                "Error encountered processing Person Program for Person Number " +
                entities.InterfacePersonProgram.CsePersonNumber + ", Program Code = " +
                entities.InterfacePersonProgram.ProgramCode;
            }

            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport1();

            if (Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";

              try
              {
                UpdateInterfacePersonProgram();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error updating Interface Person Progam Status Ind for CSE Person " +
                      entities.InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                      entities.InterfacePersonProgram.ProgramCode;
                    UseCabErrorReport1();

                    if (Equal(local.EabFileHandling.Status, "OK"))
                    {
                    }
                    else
                    {
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      goto AfterCycle;
                    }

                    break;
                  case ErrorCode.PermittedValueViolation:
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error updating Interface Person Progam Status Ind for CSE Person " +
                      entities.InterfacePersonProgram.CsePersonNumber + ", Program Code " +
                      entities.InterfacePersonProgram.ProgramCode;
                    UseCabErrorReport1();

                    if (Equal(local.EabFileHandling.Status, "OK"))
                    {
                    }
                    else
                    {
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      goto AfterCycle;
                    }

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
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto AfterCycle;
            }

            if (AsChar(local.DatabaseUpdated.Flag) == 'Y')
            {
              // *****************************************************************
              // If the database has been changed it must be rolled back to keep
              // data consistent and the cursor must be re-established.
              // ****************************************************************
              local.RollbackRestartIndicator.Flag = "Y";
              UseEabRollbackSql();

              if (local.PassArea.NumericReturnCode != 0)
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error encountered in External Action Block EAB_ROLLBACK_SQL.";
                  
                UseCabErrorReport1();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  goto AfterCycle;
                }
              }

              ExitState = "ACO_NN0000_ALL_OK";
              UseReadPgmCheckpointRestart();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
                {
                  local.CheckpointRestartKey.CsePersonNumber =
                    Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
                    
                  local.CheckpointRestartKey.ProgramCode =
                    Substring(local.ProgramCheckpointRestart.RestartInfo, 11, 3);
                    
                  local.CheckpointRestartKey.CreatedTimestamp =
                    Timestamp(Substring(
                      local.ProgramCheckpointRestart.RestartInfo, 250, 14, 26));
                    
                }
                else
                {
                  // **************************************************************
                  // 11/16/1998  C. Ott     Because the READ EACH statement 
                  // reads sorted by descending timestamp, the following
                  // statement sets the timestamp to high value when not a
                  // restart.
                  // **************************************************************
                  local.CheckpointRestartKey.CreatedTimestamp =
                    new DateTime(2099, 12, 31);
                }
              }
              else
              {
                goto AfterCycle;
              }

              // ****************************************************************
              // Restore Control Report counts to values saved prior to 
              // rollback.
              // ***************************************************************
              local.AdcRolloverLetterSent.Count =
                local.ChkpntAdcRollLettrSent.Count;
              local.PaRolloverLetterSent.Count =
                local.ChkpntPaRollLetterSent.Count;
              local.CntlTotAdcCaseOpened.Count =
                local.ChkpntTotAdcCaseOpened.Count;
              local.CntlTotAdcCaseClosed.Count =
                local.ChkpntTotAdcCaseClosed.Count;
              local.CntlTotPersPgmCreated.Count =
                local.ChkpntTotPersPgmCreate.Count;
              local.CntlTotCsePersUpdates.Count =
                local.ChkpntTotCsePersUpdate.Count;
              local.CntlTotRecordsProcessed.Count =
                local.ChkpntTotalRecsProc.Count;
              local.CntlTotPersPgmClosed.Count =
                local.ChkpntTotPersPgmClosed.Count;

              // ***************************************************************
              // The database has been rolled back so that data remains 
              // consistent after error was encountered.  Update of Interface
              // person Program Status Indicator has been commited.  Exit the
              // READ EACH loop since curser has been lost due to rollback.
              // ****************************************************************
              break;
            }
            else
            {
              local.PerformCommit.Flag = "Y";
            }
          }
        }

        // ******************************************************************
        // This is where we do the CONTROL TOTALs processing and CHECKPOINT, 
        // COMMITs etc.
        // This section is slightly different than the normal batch template 
        // because of the automatic Restart facility. If the ROLLBACK_RESTART
        // flag is set, we do not populate the restart attributes i.e RESTART
        // INFO and RESTART_IND.
        // ******************************************************************
        if (local.CheckpointNumbOfReads.Count >= local
          .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
          .CheckpointNumbOfUpdates.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.PerformCommit.Flag = "Y";
        }

        if (AsChar(local.PerformCommit.Flag) == 'Y')
        {
          // ****************************************************************
          // Record was processed successfully.  Save Control Program counts in 
          // the event of rollback.
          // ***************************************************************
          local.ChkpntAdcRollLettrSent.Count =
            local.AdcRolloverLetterSent.Count;
          local.ChkpntPaRollLetterSent.Count = local.PaRolloverLetterSent.Count;
          local.ChkpntTotAdcCaseOpened.Count = local.CntlTotAdcCaseOpened.Count;
          local.ChkpntTotAdcCaseClosed.Count = local.CntlTotAdcCaseClosed.Count;
          local.ChkpntTotPersPgmCreate.Count =
            local.CntlTotPersPgmCreated.Count;
          local.ChkpntTotCsePersUpdate.Count =
            local.CntlTotCsePersUpdates.Count;
          local.ChkpntTotalRecsProc.Count = local.CntlTotRecordsProcessed.Count;
          local.ChkpntTotPersPgmClosed.Count = local.CntlTotPersPgmClosed.Count;

          // ****************************************************************
          // Record the number of checkpoints and the last checkpoint
          // time.
          // Also return the checkpoint frequency counts in case they
          // been changed since the last read.
          // ***************************************************************
          local.BatchTimestampWorkArea.IefTimestamp =
            local.InterfacePersonProgram.CreatedTimestamp;
          UseLeCabConvertTimestamp();
          local.ProgramCheckpointRestart.RestartInfo =
            local.InterfacePersonProgram.CsePersonNumber + local
            .InterfacePersonProgram.ProgramCode + local
            .BatchTimestampWorkArea.TextTimestamp;
          local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
          local.ProgramCheckpointRestart.RestartInd = "Y";
          export.RollbackRestartOnly.Assign(local.Blank);
          ExitState = "ACO_NN0000_ALL_OK";
          UseUpdatePgmCheckpointRestart2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ok, continue processing.  Reset the checkpoint counters.
            local.CheckpointNumbOfReads.Count = 0;
            local.CheckpointNumbOfUpdates.Count = 0;
            local.PerformCommit.Flag = "";
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error encountered updating Program Checkpoint Restart table.";
            UseCabErrorReport1();

            if (Equal(local.EabFileHandling.Status, "OK"))
            {
            }
            else
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto AfterCycle;
            }
          }

          // **************************************************************
          //  Call an external that does a DB2 commit using a Cobol program.
          // **************************************************************
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error encountered in External Action Block  EXT_TO_DO_A_COMMIT.";
              
            UseCabErrorReport1();

            if (Equal(local.EabFileHandling.Status, "OK"))
            {
            }
            else
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto AfterCycle;
            }
          }
        }
      }

      if (AsChar(local.RollbackRestartIndicator.Flag) != 'Y')
      {
        break;
      }
    }
    while(AsChar(local.ReadSuccessful.Flag) != 'N');

AfterCycle:

    // ****************************************************************
    //  Set restart indicator to "N" because we successfully finished this 
    // program.
    // ****************************************************************
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    ExitState = "ACO_NN0000_ALL_OK";
    UseUpdatePgmCheckpointRestart1();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered updating Program Checkpoint Restart table.";
      UseCabErrorReport1();

      if (Equal(local.EabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

    // ****************************************************************
    // FORMAT OUTPUT CONTROL REPORT 99
    // ****************************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      "Total Interface Person Program records processed     " + NumberToString
      (local.CntlTotRecordsProcessed.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered closing Control Report, DD=RPT99.";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

    local.EabReportSend.RptDetail =
      "Total CSE Person records updated                     " + NumberToString
      (local.CntlTotCsePersUpdates.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered closing Control Report, DD=RPT99.";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

    local.EabReportSend.RptDetail =
      "Total Person Program records created                 " + NumberToString
      (local.CntlTotPersPgmCreated.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered closing Control Report, DD=RPT99.";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

    local.EabReportSend.RptDetail =
      "Total Person Program records closed                  " + NumberToString
      (local.CntlTotPersPgmClosed.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered closing Control Report, DD=RPT99.";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

    local.EabReportSend.RptDetail =
      "Total ADC Case records opened                        " + NumberToString
      (local.CntlTotAdcCaseOpened.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered closing Control Report, DD=RPT99.";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

    local.EabReportSend.RptDetail =
      "Total ADC Case records closed                        " + NumberToString
      (local.CntlTotAdcCaseClosed.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered closing Control Report, DD=RPT99.";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

    // 09/12/00 M.L Start
    // Adcrollo and Parollo letters are produced. Do not print these two lines.
    // 09/12/00 M.L End
    // ****************************************************************
    // CLOSE OUTPUT CONTROL REPORT 99
    // ****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered closing Control Report, DD=RPT99.";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

    // ****************************************************************
    // CLOSE OUTPUT ERROR REPORT 99
    // ****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveInterfacePersonProgram(InterfacePersonProgram source,
    InterfacePersonProgram target)
  {
    target.ProgramCode = source.ProgramCode;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CreatedTimestamp = source.CreatedTimestamp;
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

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(EabRollbackSql.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
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

  private void UseSiBatchProcessIntrfcPrsnPgm()
  {
    var useImport = new SiBatchProcessIntrfcPrsnPgm.Import();
    var useExport = new SiBatchProcessIntrfcPrsnPgm.Export();

    useImport.ExpPaRolloverLetterSent.Count = local.PaRolloverLetterSent.Count;
    useImport.ExpAdcRolloverLetterSent.Count =
      local.AdcRolloverLetterSent.Count;
    useImport.ExpCheckpointNumbUpdates.Count =
      local.CheckpointNumbOfUpdates.Count;
    useImport.CntlTotCsePersUpdates.Count = local.CntlTotCsePersUpdates.Count;
    useImport.CntlTotPersPgmCloses.Count = local.CntlTotPersPgmClosed.Count;
    useImport.CntlTotAdcCaseCloses.Count = local.CntlTotAdcCaseClosed.Count;
    useImport.CntlTotAdcCaseOpens.Count = local.CntlTotAdcCaseOpened.Count;
    useImport.InterfacePersonProgram.Assign(local.InterfacePersonProgram);
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.CntlTotPersPgmCreates.Count = local.CntlTotPersPgmCreated.Count;

    Call(SiBatchProcessIntrfcPrsnPgm.Execute, useImport, useExport);

    local.PaRolloverLetterSent.Count = useImport.ExpPaRolloverLetterSent.Count;
    local.AdcRolloverLetterSent.Count =
      useImport.ExpAdcRolloverLetterSent.Count;
    local.CheckpointNumbOfUpdates.Count =
      useImport.ExpCheckpointNumbUpdates.Count;
    local.CntlTotCsePersUpdates.Count = useExport.CntlTotCsePersUpdates.Count;
    local.CntlTotPersPgmClosed.Count = useExport.CntlTotPersPgmCloses.Count;
    local.CntlTotAdcCaseClosed.Count = useExport.CntlTotAdcCaseCloses.Count;
    local.CntlTotAdcCaseOpened.Count = useExport.CntlTotAdcCaseOpens.Count;
    local.CntlTotPersPgmCreated.Count = useExport.CntlTotPersPgmCreates.Count;
    local.DatabaseUpdated.Flag = useExport.DatabaseUpdated.Flag;
    local.Error.Message = useExport.Error.Message;
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

  private IEnumerable<bool> ReadInterfacePersonProgram()
  {
    entities.InterfacePersonProgram.Populated = false;

    return ReadEach("ReadInterfacePersonProgram",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.CheckpointRestartKey.CsePersonNumber);
        db.SetString(
          command, "programCode", local.CheckpointRestartKey.ProgramCode);
        db.SetDateTime(
          command, "createdTimestamp",
          local.CheckpointRestartKey.CreatedTimestamp.GetValueOrDefault());
        db.SetNullableDate(
          command, "processDate", local.Blank.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterfacePersonProgram.RecordType = db.GetString(reader, 0);
        entities.InterfacePersonProgram.SourceOfFunds =
          db.GetNullableString(reader, 1);
        entities.InterfacePersonProgram.ProgramCode = db.GetString(reader, 2);
        entities.InterfacePersonProgram.StatusInd =
          db.GetNullableString(reader, 3);
        entities.InterfacePersonProgram.ClosureReason =
          db.GetNullableString(reader, 4);
        entities.InterfacePersonProgram.From = db.GetNullableString(reader, 5);
        entities.InterfacePersonProgram.ProgEffectiveDate =
          db.GetDate(reader, 6);
        entities.InterfacePersonProgram.ProgramEndDate = db.GetDate(reader, 7);
        entities.InterfacePersonProgram.CreatedBy =
          db.GetNullableString(reader, 8);
        entities.InterfacePersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.InterfacePersonProgram.ProcessDate =
          db.GetNullableDate(reader, 10);
        entities.InterfacePersonProgram.AssignedDate =
          db.GetNullableDate(reader, 11);
        entities.InterfacePersonProgram.ParticipationCode =
          db.GetString(reader, 12);
        entities.InterfacePersonProgram.AeProgramSubtype =
          db.GetNullableString(reader, 13);
        entities.InterfacePersonProgram.CsePersonNumber =
          db.GetString(reader, 14);
        entities.InterfacePersonProgram.PaCaseNumber =
          db.GetNullableString(reader, 15);
        entities.InterfacePersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 16);
        entities.InterfacePersonProgram.MedType =
          db.GetNullableString(reader, 17);
        entities.InterfacePersonProgram.Populated = true;

        return true;
      });
  }

  private void UpdateInterfacePersonProgram()
  {
    var statusInd = "Y";
    var processDate = local.ProgramProcessingInfo.ProcessDate;

    entities.InterfacePersonProgram.Populated = false;
    Update("UpdateInterfacePersonProgram",
      (db, command) =>
      {
        db.SetNullableString(command, "statusInd", statusInd);
        db.SetNullableDate(command, "processDate", processDate);
        db.SetString(
          command, "programCode", entities.InterfacePersonProgram.ProgramCode);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.InterfacePersonProgram.CreatedTimestamp.GetValueOrDefault());
          
        db.SetString(
          command, "cspNumber",
          entities.InterfacePersonProgram.CsePersonNumber);
      });

    entities.InterfacePersonProgram.StatusInd = statusInd;
    entities.InterfacePersonProgram.ProcessDate = processDate;
    entities.InterfacePersonProgram.Populated = true;
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
    /// A value of ZdelimportRollbackRestartOnly.
    /// </summary>
    [JsonPropertyName("zdelimportRollbackRestartOnly")]
    public InterfacePersonProgram ZdelimportRollbackRestartOnly
    {
      get => zdelimportRollbackRestartOnly ??= new();
      set => zdelimportRollbackRestartOnly = value;
    }

    private InterfacePersonProgram zdelimportRollbackRestartOnly;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of RollbackRestartOnly.
    /// </summary>
    [JsonPropertyName("rollbackRestartOnly")]
    public InterfacePersonProgram RollbackRestartOnly
    {
      get => rollbackRestartOnly ??= new();
      set => rollbackRestartOnly = value;
    }

    private InterfacePersonProgram rollbackRestartOnly;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DatabaseUpdated.
    /// </summary>
    [JsonPropertyName("databaseUpdated")]
    public Common DatabaseUpdated
    {
      get => databaseUpdated ??= new();
      set => databaseUpdated = value;
    }

    /// <summary>
    /// A value of PerformCommit.
    /// </summary>
    [JsonPropertyName("performCommit")]
    public Common PerformCommit
    {
      get => performCommit ??= new();
      set => performCommit = value;
    }

    /// <summary>
    /// A value of ChkpntPaRollLetterSent.
    /// </summary>
    [JsonPropertyName("chkpntPaRollLetterSent")]
    public Common ChkpntPaRollLetterSent
    {
      get => chkpntPaRollLetterSent ??= new();
      set => chkpntPaRollLetterSent = value;
    }

    /// <summary>
    /// A value of ChkpntAdcRollLettrSent.
    /// </summary>
    [JsonPropertyName("chkpntAdcRollLettrSent")]
    public Common ChkpntAdcRollLettrSent
    {
      get => chkpntAdcRollLettrSent ??= new();
      set => chkpntAdcRollLettrSent = value;
    }

    /// <summary>
    /// A value of ChkpntTotCsePersUpdate.
    /// </summary>
    [JsonPropertyName("chkpntTotCsePersUpdate")]
    public Common ChkpntTotCsePersUpdate
    {
      get => chkpntTotCsePersUpdate ??= new();
      set => chkpntTotCsePersUpdate = value;
    }

    /// <summary>
    /// A value of ChkpntTotalRecsProc.
    /// </summary>
    [JsonPropertyName("chkpntTotalRecsProc")]
    public Common ChkpntTotalRecsProc
    {
      get => chkpntTotalRecsProc ??= new();
      set => chkpntTotalRecsProc = value;
    }

    /// <summary>
    /// A value of ChkpntTotPersPgmClosed.
    /// </summary>
    [JsonPropertyName("chkpntTotPersPgmClosed")]
    public Common ChkpntTotPersPgmClosed
    {
      get => chkpntTotPersPgmClosed ??= new();
      set => chkpntTotPersPgmClosed = value;
    }

    /// <summary>
    /// A value of ChkpntTotAdcCaseClosed.
    /// </summary>
    [JsonPropertyName("chkpntTotAdcCaseClosed")]
    public Common ChkpntTotAdcCaseClosed
    {
      get => chkpntTotAdcCaseClosed ??= new();
      set => chkpntTotAdcCaseClosed = value;
    }

    /// <summary>
    /// A value of ChkpntTotAdcCaseOpened.
    /// </summary>
    [JsonPropertyName("chkpntTotAdcCaseOpened")]
    public Common ChkpntTotAdcCaseOpened
    {
      get => chkpntTotAdcCaseOpened ??= new();
      set => chkpntTotAdcCaseOpened = value;
    }

    /// <summary>
    /// A value of ChkpntTotPersPgmCreate.
    /// </summary>
    [JsonPropertyName("chkpntTotPersPgmCreate")]
    public Common ChkpntTotPersPgmCreate
    {
      get => chkpntTotPersPgmCreate ??= new();
      set => chkpntTotPersPgmCreate = value;
    }

    /// <summary>
    /// A value of PaRolloverLetterSent.
    /// </summary>
    [JsonPropertyName("paRolloverLetterSent")]
    public Common PaRolloverLetterSent
    {
      get => paRolloverLetterSent ??= new();
      set => paRolloverLetterSent = value;
    }

    /// <summary>
    /// A value of AdcRolloverLetterSent.
    /// </summary>
    [JsonPropertyName("adcRolloverLetterSent")]
    public Common AdcRolloverLetterSent
    {
      get => adcRolloverLetterSent ??= new();
      set => adcRolloverLetterSent = value;
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
    /// A value of MaxTimestamp.
    /// </summary>
    [JsonPropertyName("maxTimestamp")]
    public DateWorkArea MaxTimestamp
    {
      get => maxTimestamp ??= new();
      set => maxTimestamp = value;
    }

    /// <summary>
    /// A value of ReadSuccessful.
    /// </summary>
    [JsonPropertyName("readSuccessful")]
    public Common ReadSuccessful
    {
      get => readSuccessful ??= new();
      set => readSuccessful = value;
    }

    /// <summary>
    /// A value of CntlTotCsePersUpdates.
    /// </summary>
    [JsonPropertyName("cntlTotCsePersUpdates")]
    public Common CntlTotCsePersUpdates
    {
      get => cntlTotCsePersUpdates ??= new();
      set => cntlTotCsePersUpdates = value;
    }

    /// <summary>
    /// A value of CntlTotRecordsProcessed.
    /// </summary>
    [JsonPropertyName("cntlTotRecordsProcessed")]
    public Common CntlTotRecordsProcessed
    {
      get => cntlTotRecordsProcessed ??= new();
      set => cntlTotRecordsProcessed = value;
    }

    /// <summary>
    /// A value of CntlTotPersPgmClosed.
    /// </summary>
    [JsonPropertyName("cntlTotPersPgmClosed")]
    public Common CntlTotPersPgmClosed
    {
      get => cntlTotPersPgmClosed ??= new();
      set => cntlTotPersPgmClosed = value;
    }

    /// <summary>
    /// A value of CntlTotAdcCaseClosed.
    /// </summary>
    [JsonPropertyName("cntlTotAdcCaseClosed")]
    public Common CntlTotAdcCaseClosed
    {
      get => cntlTotAdcCaseClosed ??= new();
      set => cntlTotAdcCaseClosed = value;
    }

    /// <summary>
    /// A value of CntlTotAdcCaseOpened.
    /// </summary>
    [JsonPropertyName("cntlTotAdcCaseOpened")]
    public Common CntlTotAdcCaseOpened
    {
      get => cntlTotAdcCaseOpened ??= new();
      set => cntlTotAdcCaseOpened = value;
    }

    /// <summary>
    /// A value of PrintLetterOnly.
    /// </summary>
    [JsonPropertyName("printLetterOnly")]
    public Common PrintLetterOnly
    {
      get => printLetterOnly ??= new();
      set => printLetterOnly = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public ExitStateWorkArea Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of RollbackRestartIndicator.
    /// </summary>
    [JsonPropertyName("rollbackRestartIndicator")]
    public Common RollbackRestartIndicator
    {
      get => rollbackRestartIndicator ??= new();
      set => rollbackRestartIndicator = value;
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
    /// A value of InterfacePersonProgram.
    /// </summary>
    [JsonPropertyName("interfacePersonProgram")]
    public InterfacePersonProgram InterfacePersonProgram
    {
      get => interfacePersonProgram ??= new();
      set => interfacePersonProgram = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public InterfacePersonProgram Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of CheckpointRestartKey.
    /// </summary>
    [JsonPropertyName("checkpointRestartKey")]
    public InterfacePersonProgram CheckpointRestartKey
    {
      get => checkpointRestartKey ??= new();
      set => checkpointRestartKey = value;
    }

    /// <summary>
    /// A value of ZdelocalAbortProgramIndicator.
    /// </summary>
    [JsonPropertyName("zdelocalAbortProgramIndicator")]
    public Common ZdelocalAbortProgramIndicator
    {
      get => zdelocalAbortProgramIndicator ??= new();
      set => zdelocalAbortProgramIndicator = value;
    }

    /// <summary>
    /// A value of ZdelLocalProcessOption.
    /// </summary>
    [JsonPropertyName("zdelLocalProcessOption")]
    public Common ZdelLocalProcessOption
    {
      get => zdelLocalProcessOption ??= new();
      set => zdelLocalProcessOption = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public OblgWork ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
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

    /// <summary>
    /// A value of CntlTotPersPgmCreated.
    /// </summary>
    [JsonPropertyName("cntlTotPersPgmCreated")]
    public Common CntlTotPersPgmCreated
    {
      get => cntlTotPersPgmCreated ??= new();
      set => cntlTotPersPgmCreated = value;
    }

    /// <summary>
    /// A value of HardcodeOpen.
    /// </summary>
    [JsonPropertyName("hardcodeOpen")]
    public External HardcodeOpen
    {
      get => hardcodeOpen ??= new();
      set => hardcodeOpen = value;
    }

    /// <summary>
    /// A value of HardcodeWrite.
    /// </summary>
    [JsonPropertyName("hardcodeWrite")]
    public External HardcodeWrite
    {
      get => hardcodeWrite ??= new();
      set => hardcodeWrite = value;
    }

    /// <summary>
    /// A value of HardcodePosition.
    /// </summary>
    [JsonPropertyName("hardcodePosition")]
    public External HardcodePosition
    {
      get => hardcodePosition ??= new();
      set => hardcodePosition = value;
    }

    /// <summary>
    /// A value of SendAdcRolloverLetter.
    /// </summary>
    [JsonPropertyName("sendAdcRolloverLetter")]
    public Common SendAdcRolloverLetter
    {
      get => sendAdcRolloverLetter ??= new();
      set => sendAdcRolloverLetter = value;
    }

    /// <summary>
    /// A value of SendPaRolloverLetter.
    /// </summary>
    [JsonPropertyName("sendPaRolloverLetter")]
    public Common SendPaRolloverLetter
    {
      get => sendPaRolloverLetter ??= new();
      set => sendPaRolloverLetter = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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

    private Common databaseUpdated;
    private Common performCommit;
    private Common chkpntPaRollLetterSent;
    private Common chkpntAdcRollLettrSent;
    private Common chkpntTotCsePersUpdate;
    private Common chkpntTotalRecsProc;
    private Common chkpntTotPersPgmClosed;
    private Common chkpntTotAdcCaseClosed;
    private Common chkpntTotAdcCaseOpened;
    private Common chkpntTotPersPgmCreate;
    private Common paRolloverLetterSent;
    private Common adcRolloverLetterSent;
    private Common checkpointNumbOfUpdates;
    private DateWorkArea maxTimestamp;
    private Common readSuccessful;
    private Common cntlTotCsePersUpdates;
    private Common cntlTotRecordsProcessed;
    private Common cntlTotPersPgmClosed;
    private Common cntlTotAdcCaseClosed;
    private Common cntlTotAdcCaseOpened;
    private Common printLetterOnly;
    private ExitStateWorkArea error;
    private Common rollbackRestartIndicator;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private InterfacePersonProgram interfacePersonProgram;
    private InterfacePersonProgram blank;
    private InterfacePersonProgram checkpointRestartKey;
    private Common zdelocalAbortProgramIndicator;
    private Common zdelLocalProcessOption;
    private OblgWork processDate;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private Common checkpointNumbOfReads;
    private Common cntlTotPersPgmCreated;
    private External hardcodeOpen;
    private External hardcodeWrite;
    private External hardcodePosition;
    private Common sendAdcRolloverLetter;
    private Common sendPaRolloverLetter;
    private Document document;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ToBeProcessed.
    /// </summary>
    [JsonPropertyName("toBeProcessed")]
    public InterfacePersonProgram ToBeProcessed
    {
      get => toBeProcessed ??= new();
      set => toBeProcessed = value;
    }

    /// <summary>
    /// A value of InterfacePersonProgram.
    /// </summary>
    [JsonPropertyName("interfacePersonProgram")]
    public InterfacePersonProgram InterfacePersonProgram
    {
      get => interfacePersonProgram ??= new();
      set => interfacePersonProgram = value;
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

    private InterfacePersonProgram toBeProcessed;
    private InterfacePersonProgram interfacePersonProgram;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
