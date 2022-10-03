// Program: FN_B630_PROCESS_ACCRUAL, ID: 372552269, model: 746.
// Short name: SWEF630B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B630_PROCESS_ACCRUAL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB630ProcessAccrual: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B630_PROCESS_ACCRUAL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB630ProcessAccrual(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB630ProcessAccrual.
  /// </summary>
  public FnB630ProcessAccrual(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";

    // ***************************************************
    // Every initial development and change to that
    // development needs to be documented.
    // ***************************************************
    // *****************************************************************************************************
    // DATE      DEVELOPER NAME          REQUEST #  DESCRIPTION
    // --------  ----------------------  ---------  
    // --------------------------------------------------------
    // 02/14/96  Bryan Fristrup - MTW               Initial Development
    // 09/25/96  Tanmoy Kundu   - MTW                       Rework
    // 09/17/97  R.B.Mohapatra  - MTW           Built Restart logic;Corrected 
    // Control Totals
    // 12/10/97  Evelyn Parker - DIR	Corrected View Matching to Create Program 
    // Control Total, Update Program Control Total, and Create Program Error (
    // Program Process Info match was missing).  Also unmatched Zdel Program Run
    // Persistent View.
    // 12/17/97  Evelyn Parker - DIR	#34482  Added debug logic.  Update Accrual 
    // Instructions was out of place.  Moved it under Create New Debt.
    // 01/15/98  Venkatesh Kamaraj		Changed the way it accesses database to 
    // improve performance.
    // 12/2/1998  E. Parker		Changed logic to derive last day of following month
    // so that PPI Process Date can be set to current date like most other
    // batches; added logic to create new error and control reports;  removed
    // logic using error and control total tables; changed logic to accrue
    // through the accrual instructions discontinue date;  added additional
    // qualifier to read each on accrual instructions to check disc date> last
    // accrual date; changed logic to accrue on the accrual instructions as of
    // date.
    // 2/4/1999  E. Parker 		Added logic to set new created_by and created_tmst 
    // attributes on Debt Detail.  Added those views to
    // fn_assign_debt_dtl_stat_hist_id.  Took out logic to set Debt_Detail
    // ADC_Dt as this attribute will no longer be set, used, or maintained.
    // 4/16/99 - bud adams  -  deleted 24ROUNDED functions; removed
    //   reference to FN_Read_Obligation_Payment_Schedule and
    //   included that entity type in the main Read Each loop;
    // *****************************************************************************************************
    // =================================================
    // 4/16/99 - b adams  -  deleted Read of Obligation_Type and
    //   joined it with the main Read Each.  Removed SETs and
    //   other references to obsolete efforts at checkpoint / restart
    //   logic; deleted use of a cab to retrieve Obligation_Payment_
    //   Schedule (had a Read Each) and included it in Read Each.
    // 4/28/99 - B Adams  -  attempt at checkpoint / restart replaced;
    //   reorganized code; error processing and handling overhauled;
    //   removed a Read Each and included in main R/E; also removed
    //   Read of Legal_Action_Person and put it in Read Each.
    // 6/3/99 - bud adams  -  When an ob_tran is totally suspended
    //   then the accrual_instructions last_accrual_date is now set
    //   to be the accrual_suspension date - 1; removed vestiges of
    //   summary logic.
    // 6/4/99 - bud adams  -  If a supported person is discontinued
    //   on the month currently being accrued, set the last accrual
    //   date to the discontinue date instead of the last day of the
    //   following month, like it is for all other debts.
    // 1/22/00 - b adams  -  PR# 85104: Partially suspended debts
    //   with a suspension date = the day after the last-accrual-date
    //   was not having its last-accrual-date updated - and so was
    //   accruing every night.
    // 01/11/2006 GVandy  PR267430  Set DEBT new_debt_process_date so that retro
    // processing is bypassed if the debt_detail due_date >= processing date + 1
    // month.
    // 07/19/2010 GVandy CQ14223 Use processing date of previous run to 
    // determine if
    // retro processing is necessary for newly created debts.
    // =================================================
    // ***** HARD CODE AREA *****
    local.HardcodedViews.HardcodeMaximumDiscontinue.Date =
      UseCabSetMaximumDiscontinueDate();
    local.RestartDueToError.Flag = "Y";
    UseFnHardcodedDebtDistribution();

    // ***** Get the run parameters for this program.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();
    local.ProcessDate.Date = local.ProgramProcessingInfo.ProcessDate;

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
    }

    // 07/19/2010 GVandy CQ14223 Use processing date of previous run to 
    // determine if
    // retro processing is necessary for newly created debts.
    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 8)))
    {
      local.PreviousProcessDate.Date =
        AddDays(local.ProgramProcessingInfo.ProcessDate, -1);
    }
    else
    {
      local.PreviousProcessDate.Date =
        IntToDate((int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 1, 8)));
    }

    if (!IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 10, 230)))
    {
      local.LeftPad.Text10 =
        TrimEnd(Substring(local.ProgramProcessingInfo.ParameterList, 10, 230));
      UseEabPadLeftWithZeros();
      local.DebugObligor.Number = local.LeftPad.Text10;
    }

    // ***** Get the DB2 commit frequency counts.
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    local.Current.Timestamp = Now();
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // **** get the last cse-person & obligation processed ****
      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
      {
        local.RestartCsePerson.Number =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);

        // =================================================
        // 4/16/99 - b adams  -  removed unnecessary restart info;
        //   changed the start point for "buckets" from 14 to 11
        // =================================================
        local.RestartObligation.SystemGeneratedIdentifier =
          (int)StringToNumber(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250, 11, 3));
      }
      else
      {
        local.RestartCsePerson.Number = "";
        local.RestartObligation.SystemGeneratedIdentifier = 0;
      }
    }
    else
    {
      return;
    }

    // ***** SET-UP AREA *****
    // *** Initialize Control Report ***
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // *** Initialize Error Report ***
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    UseCabFirstAndLastDateOfMonth1();
    local.ProcessThru.Date = AddDays(AddMonths(local.FirstDom.Date, 2), -1);
    local.ProcessThru.YearMonth = UseCabGetYearMonthFromDate1();
    local.ProcessThruBackup.Date = local.ProcessThru.Date;
    local.ProcessThruBackup.YearMonth = local.ProcessThru.YearMonth;

    if (!ReadObligationTransactionRlnRsn())
    {
      ExitState = "FN0000_OBLIG_TRANS_RLN_RSN_NF_RB";

      return;
    }

    local.Timestamp.TextDate =
      NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8);
    local.Control.ProgramName = "SWE630F%";
    local.Control.LastCheckpointTimestamp =
      Timestamp(Substring(
        local.Timestamp.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4) + "-"
      + Substring
      (local.Timestamp.TextDate, DateWorkArea.TextDate_MaxLength, 5, 2) + "-"
      + Substring
      (local.Timestamp.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2) +
      "-11.11.11.111111");

    // =================================================
    // 4/30/99 - bud adams  -  In case this is being restarted, get
    //   the statistics from the previous partial run for the control
    //   report.
    // =================================================
    foreach(var item in ReadProgramCheckpointRestart2())
    {
      switch(entities.ProgramCheckpointRestart.UpdateFrequencyCount)
      {
        case 1:
          local.AccInstrReadInThisRun.Count =
            entities.ProgramCheckpointRestart.ReadFrequencyCount.
              GetValueOrDefault();

          break;
        case 2:
          local.AccInstrUptdInThisRun.Count =
            entities.ProgramCheckpointRestart.ReadFrequencyCount.
              GetValueOrDefault();

          break;
        case 3:
          local.ObligReadInThisRun.Count =
            entities.ProgramCheckpointRestart.ReadFrequencyCount.
              GetValueOrDefault();

          break;
        case 4:
          local.OtrnCreatedInThisRun.Count =
            entities.ProgramCheckpointRestart.ReadFrequencyCount.
              GetValueOrDefault();

          break;
        case 5:
          local.OtrnReadInThisRun.Count =
            entities.ProgramCheckpointRestart.ReadFrequencyCount.
              GetValueOrDefault();

          break;
        default:
          break;
      }
    }

    // ***** MAIN-LINE AREA *****
    // ***** Process the selected records in groups based upon the commit
    // frequencies.  Do a DB2 commit at the end of each group.
    // =================================================
    // 4/27/99 - bud adams  -  Read each processes each qualified
    //   Obligation_Transaction (Debt) ordered by Obligor
    //   CSE_Person, and by Obligation S_G_Identifier.
    //   Checkpoint counts are of Obligations; COMMITs are issued
    //   at the CSE_Person boundary.
    // =================================================
    local.ErrorCsePerson.Number = "";
    local.ErrorObligation.SystemGeneratedIdentifier = 0;

    while(AsChar(local.RestartDueToError.Flag) == 'Y')
    {
      // The purpose of this logic is to make the read qualify only on
      // CSE person # rather than both CSE person # and obligation
      // id. Earlier when the logic was on both, an Index Scan was
      // requested, resulting is more time for cursor materialization.
      // Under direction of Kevin Harrison from IBM.
      // =================================================
      // 4/21/99 - bud adams  -  Obligation_Type is part of the identifier
      //   of Obligation, but the way System_Generated_Identifier is
      //   derived is sequential and unique by CSE_Person
      // =================================================
      // =================================================
      // Obligation is being used as the point of restart instead of
      //   CSE_Person.  If an error occurs, we don't want to skip the
      //   processing of all obligations related to that CSE_Person,
      //   just the one that was in error.
      // =================================================
      foreach(var item in ReadObligationCsePersonAccrualInstructionsObligationType())
        
      {
        // =================================================
        // 4/16/99 - Bud Adams  -  removed Read of Obligation_Type
        //   and included it in Read Each
        //   Removed USE FN_Read_Oblig_Pymt_Schedule from below
        //   and included that entity type in this Read Each.  (It had a
        //   Read Each, and used a hardcode cab, with 120 SETs.
        //   Deleted Read of Legal_Action_Person from below and
        //   included it in this Read Each.
        // 4/31/99 - B Adams  -  deleted Read Each of accrual_instructions
        //   debt, key supported, and key_supported_person cse_person
        //   from below and included Debt in this one.  (Even though it
        //   had been coded as a Read Each, it would only ever return
        //   one row.)  No data was required or retrieved from those
        //   "key" entity types.
        // =================================================
        // =================================================
        // The restart position is based on CSE_Person.  There won't
        // be very many obligations per person, so we'll just flip thru
        // however many Obligations we need to until we find the place
        // we want to restart at.
        // =================================================
        if (AsChar(local.RestartDueToError.Flag) == 'Y')
        {
          if (entities.Obligation.SystemGeneratedIdentifier < local
            .RestartObligation.SystemGeneratedIdentifier && Equal
            (entities.KeyObligor1.Number, local.RestartCsePerson.Number))
          {
            continue;
          }
          else
          {
            // =================================================
            // When we find the "restart at" record, we want to skip it.  It
            // will either be the record where an error was found, the very
            // beginning of the run and the obligation id will be 0, or the
            // place where a COMMIT was issued.  In these cases we want
            // the next record.
            // If the record read has an obligation SGI > the restart value
            // then we want to process it.
            // Since there is no "last-record-processed" in this instance
            // we'll set those values to the current values.
            // =================================================
            local.RestartDueToError.Flag = "N";
            local.LastRecordProcessedCsePerson.Number =
              entities.KeyObligor1.Number;
            local.LastRecordProcessedObligation.SystemGeneratedIdentifier =
              entities.Obligation.SystemGeneratedIdentifier;
          }
        }

        if (Equal(entities.KeyObligor1.Number,
          local.LastRecordProcessedCsePerson.Number) && entities
          .Obligation.SystemGeneratedIdentifier == local
          .LastRecordProcessedObligation.SystemGeneratedIdentifier)
        {
        }
        else
        {
          // =================================================
          // 4/28/99 - bud adams  -  Took this counter out of a CAB inside
          //   the Read Each below and put it here.  That was counting
          //   the supported persons being updated, actually.
          // =================================================
          ++local.UpdatesSinceLastCommit.Count;
          local.LastRecordProcessedCsePerson.Number =
            entities.KeyObligor1.Number;
          local.LastRecordProcessedObligation.SystemGeneratedIdentifier =
            entities.Obligation.SystemGeneratedIdentifier;

          if (local.UpdatesSinceLastCommit.Count >= local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            local.Commit.Flag = "Y";
            local.Commit.SelectChar = "C";
          }
        }

        // =================================================
        // 4/27/99 - bud adams  -  If the current record resulted in an
        //   error earlier, commit all the work done since rolling back,
        //   skip the current record, reset the restart key values, and go
        // =================================================
        if (entities.Obligation.SystemGeneratedIdentifier == local
          .ErrorObligation.SystemGeneratedIdentifier && Equal
          (entities.KeyObligor1.Number, local.ErrorCsePerson.Number))
        {
          local.Commit.Flag = "Y";
          local.Commit.SelectChar = "E";
          UseFnMakeObligationError();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            // ***---  If fn_make_obligation_error fails, it's not critical to 
            // the
            // ***---  success or failure of this process.  Note the problem in
            // ***---  the error report and keep going.
            local.EabReportSend.RptDetail = "Obligation not found.  " + "Obligor Person No  " +
              entities.KeyObligor1.Number + ".  Obligation Id  " + NumberToString
              (entities.Obligation.SystemGeneratedIdentifier, 15) + ".";
            local.EabReportSend.RptDetail =
              ">>> Error updating dormant indicator to '*' for above obligation";
              
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
        }

        if (AsChar(local.Commit.Flag) == 'Y')
        {
          // *****  Create or Update the program control totals.
          local.AccInstrReadInThisRun.Count += local.AccInstrReadSinceChkpt.
            Count;
          local.AccInstrUptdInThisRun.Count += local.AccInstrUptdSinceChkpt.
            Count;
          local.ObligReadInThisRun.Count += local.ObligReadSinceChkpnt.Count;
          local.OtrnCreatedInThisRun.Count += local.OtrnCreatedSinceChkpnt.
            Count;
          local.OtrnReadInThisRun.Count += local.OtrnReadSinceChkpnt.Count;

          // ***** Record the number of checkpoints and the last checkpoint time
          // and
          // set the restart indicator to yes.
          // Also return the checkpoint frequency counts in case they
          // been changed since the last read.
          local.RestartCsePerson.Number = entities.KeyObligor1.Number;
          local.RestartObligation.SystemGeneratedIdentifier =
            entities.Obligation.SystemGeneratedIdentifier;
          local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
          local.Current.Timestamp =
            local.ProgramCheckpointRestart.LastCheckpointTimestamp;
          local.ProgramCheckpointRestart.RestartInd = "Y";

          // **** update restart info with current values of cse_person number &
          // obligation sys gen-id ****
          local.ProgramCheckpointRestart.RestartInfo =
            entities.KeyObligor1.Number;
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            (entities.Obligation.SystemGeneratedIdentifier, 13, 3);
          UseUpdatePgmCheckpointRestart2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ok, continue processing
          }
          else
          {
            // **** no exception processing is reqd as program aborts in the cab
            // update_program_checkpoint_restart on exception ****
            return;
          }

          // =================================================
          // 4/29/99 - bud adams  -  accumulate the counts for the control
          //   report.  In case of having to restart this, the counts from the
          //   previous partial run will be available.
          // =================================================
          for(local.Loop.Count = 1; local.Loop.Count <= 5; ++local.Loop.Count)
          {
            local.Control.UpdateFrequencyCount = local.Loop.Count;
            local.Control.ProgramName =
              Substring(local.Control.ProgramName,
              ProgramCheckpointRestart.ProgramName_MaxLength, 1, 7) + NumberToString
              (local.Loop.Count, 15, 1);

            switch(local.Control.UpdateFrequencyCount.GetValueOrDefault())
            {
              case 1:
                local.Control.ReadFrequencyCount =
                  local.AccInstrReadSinceChkpt.Count;

                break;
              case 2:
                local.Control.ReadFrequencyCount =
                  local.AccInstrUptdSinceChkpt.Count;

                break;
              case 3:
                local.Control.ReadFrequencyCount =
                  local.ObligReadSinceChkpnt.Count;

                break;
              case 4:
                local.Control.ReadFrequencyCount =
                  local.OtrnCreatedSinceChkpnt.Count;

                break;
              case 5:
                local.Control.ReadFrequencyCount =
                  local.OtrnReadSinceChkpnt.Count;

                break;
              default:
                break;
            }

            if (ReadProgramCheckpointRestart1())
            {
              try
              {
                UpdateProgramCheckpointRestart();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    break;
                  case ErrorCode.PermittedValueViolation:
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
              try
              {
                CreateProgramCheckpointRestart();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    break;
                  case ErrorCode.PermittedValueViolation:
                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }

          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          local.AccInstrReadSinceChkpt.Count = 0;
          local.AccInstrUptdSinceChkpt.Count = 0;
          local.ObligReadSinceChkpnt.Count = 0;
          local.OtrnCreatedSinceChkpnt.Count = 0;
          local.OtrnReadSinceChkpnt.Count = 0;
          local.UpdatesSinceLastCommit.Count = 0;
          local.Commit.Flag = "N";

          // ***---  select_char = 'E' means the commit is due to an Error
          // ***
          // ***---  in this case, we do a NEXT because we want to skip
          // ***---  the current, error-causing record.
          if (AsChar(local.Commit.SelectChar) == 'E')
          {
            local.Commit.SelectChar = "";

            continue;
          }

          local.Commit.SelectChar = "";
        }

        ++local.AccInstrReadSinceChkpt.Count;

        // **** update no of obligations read ****
        ++local.ObligReadSinceChkpnt.Count;

        // *** RBM   09/17/97  Set date/Obligor# for recomputing the Buckets ***
        // =================================================
        // 4/31/99 - bud adams  -  IF test was incorrect.  Tested on
        //   Restart Person Number value and this would never be valued
        //   until after a commit.  All we want to know is if this has been
        //   done for that person already.
        // 5/7/99 - bud adams  -  Deleted Use Recomp_Buck_From_Date
        //   and associated logic.  The requirement for this went away
        //   with the decision to do away with Summaries.
        // =================================================
        // =================================================
        // 5/7/99 - bud adams  -  Deleted use FN_Cab_Set_Recomp_Buck_From_Date
        //   and associated SETs.  Requirement for this went away with
        //   the decision to do away with Refresh and Summaries.
        // =================================================
        // =================================================
        // 7/23/99 - bud adams  -  a nested CAB is has its import view
        //   of Accrual_Instructions defined as 'persistent/locked'.  That
        //   means that the EA view being view matched into it must be
        //   read with an Update Intent lock.  And, since Accrual_Instructions
        //   is part of this Read Each, IEF generates a  cursor  'for update
        //   of'  for each entity type.  This requires extra overhead,
        //   keeps the lock manager very busy, and makes the number
        //   of locks held climb very rapidly, forcing the commit count
        //   to be unnecessarily low.
        //   So, a separate read to satisfy the 'persistent' view will
        //   relieve all this.
        // =================================================
        if (ReadAccrualInstructions())
        {
          if (Lt(entities.AccrualInstructions2.AsOfDt, new DateTime(1960, 1, 1)))
            
          {
            local.RestartDueToError.Flag = "Y";
            local.EabReportSend.RptDetail =
              "Accrual Instructions As of Date less than 1960-01-01" + "Obligor Person No:  " +
              entities.KeyObligor1.Number + ".  Obligation Id:  " + NumberToString
              (entities.Obligation.SystemGeneratedIdentifier, 15) + "Ob_Tran ID:  " +
              NumberToString
              (entities.AccrualInstructions1.SystemGeneratedIdentifier, 15) + " - FN_B630_Process_Accrual.";
              
          }
        }
        else
        {
          local.RestartDueToError.Flag = "Y";
          local.EabReportSend.RptDetail =
            "\"Persistent\" Accrual Instructions not found.  " + "Obligor Person No:  " +
            entities.KeyObligor1.Number + ".  Obligation Id:  " + NumberToString
            (entities.Obligation.SystemGeneratedIdentifier, 15) + "Ob_Tran ID:  " +
            NumberToString
            (entities.AccrualInstructions1.SystemGeneratedIdentifier, 15) + " - FN_B630_Process_Accrual.";
            
        }

        if (AsChar(local.RestartDueToError.Flag) == 'Y')
        {
          // ***---  So any error conditions above will skip this
        }
        else
        {
          local.AccrualInstructions.Assign(entities.AccrualInstructions2);
          local.AccrualDiscontinue.Date =
            entities.AccrualInstructions2.DiscontinueDt;
          local.AccrualDiscontinue.YearMonth = UseCabGetYearMonthFromDate2();

          if (Equal(entities.AccrualInstructions2.LastAccrualDt,
            local.Blank.Date))
          {
            local.ProcessFrom.Date = entities.AccrualInstructions2.AsOfDt;
            local.ProcessFrom.YearMonth = UseCabGetYearMonthFromDate3();
          }
          else
          {
            local.ProcessFrom.Date =
              AddDays(entities.AccrualInstructions2.LastAccrualDt, 1);
            local.ProcessFrom.YearMonth = UseCabGetYearMonthFromDate3();
          }

          // =================================================
          // 6/17/99 - b adams  -  If accrual_instructions discontinue_date
          //   is prior to the process-thru-date, then stop this process
          //   when that date is reached.  Later, set the Accrual_Instructions
          //   Last_Accrual_Date to be this Discontinue_Date.
          // =================================================
          if (!Lt(local.ProcessThru.Date,
            entities.AccrualInstructions2.DiscontinueDt))
          {
            local.ProcessThru.YearMonth = local.AccrualDiscontinue.YearMonth;
            local.ProcessThru.Date = local.AccrualDiscontinue.Date;
          }

          MoveDateWorkArea(local.ProcessFrom, local.ProcessCurrent);
          UseCabFirstAndLastDateOfMonth2();
          ++local.OtrnReadSinceChkpnt.Count;
          local.IsDebtCreated.Flag = "N";

          while(local.ProcessCurrent.YearMonth <= local.ProcessThru.YearMonth)
          {
            if (local.ProcessCurrent.YearMonth <= local
              .AccrualDiscontinue.YearMonth)
            {
              UseFnDetermineDueDates();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else if (IsExitState("FN0000_INVALID_FREQ_CODE"))
              {
                local.RestartDueToError.Flag = "Y";
                local.EabReportSend.RptDetail = "Invalid Frequency Code.  " + "Obligor Person No  " +
                  entities.KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                  (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Determine_Due_Dates.";
                  

                goto Test2;
              }
              else if (IsExitState("INVALID_DAY_OF_WEEK"))
              {
                local.RestartDueToError.Flag = "Y";
                local.EabReportSend.RptDetail = "Invalid Day of Week.  " + "Obligor Person No  " +
                  entities.KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                  (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Determine_Due_Dates.";
                  

                goto Test2;
              }
              else
              {
              }

              for(local.Local1.Index = 0; local.Local1.Index < local
                .Local1.Count; ++local.Local1.Index)
              {
                if (Equal(local.Local1.Item.DebtDetail.DueDt, local.Blank.Date) ||
                  local.Local1.Item.Debt.Amount == 0)
                {
                  continue;
                }

                local.DebtDetail.DueDt = local.Local1.Item.DebtDetail.DueDt;
                UseFnCreateNewDebt();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  // **** Increment no of obligation_transaction created ****
                  ++local.OtrnCreatedSinceChkpnt.Count;
                  local.IsDebtCreated.Flag = "Y";
                }
                else if (IsExitState("FN0000_OBLIGATION_NF"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail = "Obligation not found.  " + "Obligor Person No  " +
                    entities.KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else if (IsExitState("FN0000_SUPP_PERSON_ACCT_NF"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail =
                    "Supported Person Account not found.  " + "Obligor Person No  " +
                    entities.KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else if (IsExitState("FN0229_DEBT_NF"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail = "Debt not found.  " + "Obligor Person No  " +
                    entities.KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else if (IsExitState("FN0000_OBLIG_TRANS_RLN_RSN_NF"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail =
                    "Obligation Transaction Relationship Reason not found.  " +
                    "Obligor Person No  " + entities.KeyObligor1.Number + ".  Obligation Id  " +
                    NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else if (IsExitState("FN0000_LEGAL_ACT_PERSON_NF"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail =
                    "Legal Action Person not found.  " + "Obligor Person No  " +
                    entities.KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else if (IsExitState("FN0228_DEBT_AE"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail = "Debt already exists.  " + "Obligor Person No  " +
                    entities.KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else if (IsExitState("FN0231_DEBT_PV"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail =
                    "Debt permitted value violation.  " + "Obligor Person No  " +
                    entities.KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else if (IsExitState("FN0209_DEBT_DETAIL_AE"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail =
                    "Debt Detail already exists.  " + "Obligor Person No  " + entities
                    .KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else if (IsExitState("FN0217_DEBT_DETAIL_PV"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail =
                    "Debt Detail permitted value violation.  " + "Obligor Person No  " +
                    entities.KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else if (IsExitState("FN0220_DEBT_DETL_STAT_HIST_AE"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail =
                    "Debt Detail Status History already exists.  " + "Obligor Person No  " +
                    entities.KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else if (IsExitState("FN0226_DEBT_DETL_STAT_HIST_PV"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail =
                    "Debt Detail Status History permitted value violation.  " +
                    "Obligor Person No  " + entities.KeyObligor1.Number + ".  Obligation Id  " +
                    NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else if (IsExitState("FN0000_OBLIG_TRANS_RLN_AE"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail =
                    "Obligation Transaction Relationship already exists.  " + "Obligor Person No  " +
                    entities.KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else if (IsExitState("FN0000_OBLIG_TRANS_RLN_PV"))
                {
                  local.RestartDueToError.Flag = "Y";
                  local.EabReportSend.RptDetail =
                    "Obligation Transaction Relationship permitted value violation.  " +
                    "Obligor Person No  " + entities.KeyObligor1.Number + ".  Obligation Id  " +
                    NumberToString
                    (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Create_New_Debt.";
                    

                  goto Test2;
                }
                else
                {
                }
              }
            }

            // *****  Increment the processing month.
            local.ProcessCurrent.Date = AddMonths(local.ProcessCurrent.Date, 1);
            local.ProcessCurrent.YearMonth = UseCabGetYearMonthFromDate4();
            UseCabFirstAndLastDateOfMonth2();
          }

          // =================================================
          // 6/2/99 - bud adams  -  This test was only checking for 'flag' =
          //   'Y'.  Also, added the subsequent test on last-accrual-date
          //   so that date won't be updated unnessarily.
          // =================================================
          if (AsChar(local.IsDebtCreated.Flag) == 'Y' || Lt
            (local.Blank.Date, local.AccrualSuspension.SuspendDt))
          {
            // *****  Update the accrual instructions last accrual date.
            // =================================================
            // 1/22/00 - Bud Adams  -  PR# 85104: Test for 'local-valid-date'
            //   was missing.  'S' indicates a total suspension.  Without it,
            //   an obligation that is only partially suspended would never
            //   have its last-accrual-date updated - and so, every night that
            //   accruals are created, another one would be created for that
            //   partially suspended obligation.
            // =================================================
            if (Equal(AddDays(entities.AccrualInstructions2.LastAccrualDt, 1),
              local.AccrualSuspension.SuspendDt) && AsChar
              (local.ValidDateInd.Flag) == 'S')
            {
              goto Test1;
            }

            // =================================================
            // 4/20/99 - bud adams  -  The last accrual date for obligations
            //   that have a current accrual suspension is the date before
            //   that accrual suspension went into effect.
            //   A Debt is created when a suspension does not exist that
            //   reduces the amount to 0.  As long as a debt is created,
            //   even if a suspension record exists, disregard that date.
            // =================================================
            if (AsChar(local.ValidDateInd.Flag) == 'S')
            {
              local.AccrualInstructions.LastAccrualDt =
                AddDays(local.AccrualSuspension.SuspendDt, -1);
            }
            else
            {
              local.AccrualInstructions.LastAccrualDt = local.ProcessThru.Date;
            }

            UseFnUpdateAccrualInstructions();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ****  increment no of accrual_instr updated  ****
              ++local.AccInstrUptdSinceChkpt.Count;
            }
            else if (IsExitState("CO0000_ACCRUAL_INSTRUCTN_NF"))
            {
              local.RestartDueToError.Flag = "Y";
              local.EabReportSend.RptDetail =
                "Accrual Instructions not found.  " + "Obligor Person No  " + entities
                .KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Update_Accrual_Instructions.";
                

              goto Test2;
            }
            else if (IsExitState("FN0000_ACCRUAL_INSTRUCTIONS_NU"))
            {
              local.RestartDueToError.Flag = "Y";
              local.EabReportSend.RptDetail =
                "Accrual Instructions not unique.  " + "Obligor Person No  " + entities
                .KeyObligor1.Number + ".  Obligation Id  " + NumberToString
                (entities.Obligation.SystemGeneratedIdentifier, 15) + " - FN_Update_Accrual_Instructions.";
                

              goto Test2;
            }
            else
            {
            }
          }

Test1:

          local.ProcessThru.YearMonth = local.ProcessThruBackup.YearMonth;
          local.ProcessThru.Date = local.ProcessThruBackup.Date;
        }

Test2:

        if (AsChar(local.RestartDueToError.Flag) == 'Y')
        {
          UseEabRollbackSql();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ZD_ERROR_IN_EAB_ROLLBACK_SQL_AB";

            return;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          // ** Record the point at which the error was detected, get out
          // ** of the Read Each, and then reinitialize the cursor, starting
          // ** at the same point as before.  When this record is retrieved
          // ** next time, a COMMIT will be issued, it will be skipped,
          // ** the restart point will be reset to the error record, and then
          // ** processing will continue.
          local.ErrorCsePerson.Number = entities.KeyObligor1.Number;
          local.ErrorObligation.SystemGeneratedIdentifier =
            entities.Obligation.SystemGeneratedIdentifier;
          local.LastProcessed.Number = entities.KeyObligor1.Number;
          local.UpdatesSinceLastCommit.Count = 0;
          local.AccInstrReadSinceChkpt.Count = 0;
          local.AccInstrUptdSinceChkpt.Count = 0;
          local.ObligReadSinceChkpnt.Count = 0;
          local.OtrnCreatedSinceChkpnt.Count = 0;
          local.OtrnReadSinceChkpnt.Count = 0;
          ExitState = "ACO_NN0000_ALL_OK";

          break;
        }

        // ***---  end of main Read Each
      }

      // =================================================
      // 6/4/99 - b adams  -  when testing, etc., we can get into a loop
      //   if nobody is picked up.
      // =================================================
      if (IsEmpty(entities.KeyObligor1.Number))
      {
        break;
      }

      // ***---  end of WHILE restart_due_to_error = 'Y'
    }

    ExitState = "ACO_NN0000_ALL_OK";

    // *****  Create or Update the program control totals.
    local.AccInstrReadInThisRun.Count += local.AccInstrReadSinceChkpt.Count;
    local.AccInstrUptdInThisRun.Count += local.AccInstrUptdSinceChkpt.Count;
    local.ObligReadInThisRun.Count += local.ObligReadSinceChkpnt.Count;
    local.OtrnCreatedInThisRun.Count += local.OtrnCreatedSinceChkpnt.Count;
    local.OtrnReadInThisRun.Count += local.OtrnReadSinceChkpnt.Count;
    local.Subscript.Count = 1;

    do
    {
      switch(local.Subscript.Count)
      {
        case 1:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "ACCRUAL INSTRUCTIONS READ          " + "    " + NumberToString
            (local.AccInstrReadInThisRun.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "ACCRUAL INSTRUCTIONS UPDATED       " + "    " + NumberToString
            (local.AccInstrUptdInThisRun.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "OBLIGATION TRANSACTIONS READ       " + "    " + NumberToString
            (local.OtrnReadInThisRun.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "OBLIGATION TRANSACTIONS CREATED    " + "    " + NumberToString
            (local.OtrnCreatedInThisRun.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "OBLIGATIONS READ                   " + "    " + NumberToString
            (local.ObligReadInThisRun.Count, 15);

          break;
        case 6:
          local.EabFileHandling.Action = "CLOSE";

          break;
        default:
          break;
      }

      UseCabControlReport1();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 6);

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // 07/19/2010 GVandy CQ14223 Save current processing date for next run.
    local.ProgramProcessingInfo.ParameterList =
      NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8, 8) +
      " " + Substring(local.ProgramProcessingInfo.ParameterList, 10, 231);
    UseUpdateProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
    }

    // ***** Set restart indicator to no because we successfully finished this 
    // program.
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart1();

    // =================================================
    // 4/29/99 - Bud Adams  -  Clean out the control totals after
    //   a successful completion of the job.
    // =================================================
    local.Control.ProgramName = "SWE630F%";

    foreach(var item in ReadProgramCheckpointRestart2())
    {
      DeleteProgramCheckpointRestart();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
    }

    // =================================================
    // 5/7/99 - bud adams  -  Deleted use FN_Cab_Set_Recomp_Buck_From_Date
    //   and associated SETs.  Requirement for this went away with
    //   the decision to do away with Refresh and Summaries.
    // =================================================
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveAccrualSuspension(AccrualSuspension source,
    AccrualSuspension target)
  {
    target.SuspendDt = source.SuspendDt;
    target.ResumeDt = source.ResumeDt;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.YearMonth = source.YearMonth;
  }

  private static void MoveDebtDetailStatusHistory(
    DebtDetailStatusHistory source, DebtDetailStatusHistory target)
  {
    target.Code = source.Code;
    target.ReasonTxt = source.ReasonTxt;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExport1ToLocal1(FnDetermineDueDates.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    target.Debt.Amount = source.Debt.Amount;
    target.DebtDetail.DueDt = source.DebtDetail.DueDt;
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

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFirstAndLastDateOfMonth1()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.ProcessDate.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.FirstDom.Date = useExport.First.Date;
  }

  private void UseCabFirstAndLastDateOfMonth2()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.ProcessCurrent.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.ProcessCurrentEnd.Date = useExport.Last.Date;
  }

  private int UseCabGetYearMonthFromDate1()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.ProcessThru.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private int UseCabGetYearMonthFromDate2()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.AccrualDiscontinue.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private int UseCabGetYearMonthFromDate3()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.ProcessFrom.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private int UseCabGetYearMonthFromDate4()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.ProcessCurrent.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.LeftPad.Text10;
    useExport.TextWorkArea.Text10 = local.LeftPad.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.LeftPad.Text10 = useExport.TextWorkArea.Text10;
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

  private void UseFnCreateNewDebt()
  {
    var useImport = new FnCreateNewDebt.Import();
    var useExport = new FnCreateNewDebt.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
      entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.LegalActionPerson.Identifier =
      entities.LegalActionPerson.Identifier;
    useImport.Obligor.Number = entities.KeyObligor1.Number;
    useImport.AccrualInstructions.SystemGeneratedIdentifier =
      entities.AccrualInstructions1.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.Supported.Number = entities.KeySupportedPerson.Number;
    MoveDebtDetailStatusHistory(local.HardcodedViews.HardcodeActive,
      useImport.DebtDetailStatusHistory);
    useImport.Debt.Amount = local.Local1.Item.Debt.Amount;
    useImport.DebtDetail.DueDt = local.Local1.Item.DebtDetail.DueDt;
    useImport.Current.Timestamp = local.Current.Timestamp;
    useImport.PreviousProcessingDate.Date = local.PreviousProcessDate.Date;

    Call(FnCreateNewDebt.Execute, useImport, useExport);

    local.DebtDetail.Assign(useExport.DebtDetail);
  }

  private void UseFnDetermineDueDates()
  {
    var useImport = new FnDetermineDueDates.Import();
    var useExport = new FnDetermineDueDates.Export();

    useImport.AccrualInstructions.Amount = entities.AccrualInstructions1.Amount;
    useImport.ObligationPaymentSchedule.Assign(
      entities.ObligationPaymentSchedule);
    useImport.ProcessThru.Date = local.ProcessThru.Date;
    useImport.ProcessFrom.Date = local.ProcessFrom.Date;
    MoveDateWorkArea(local.ProcessCurrent, useImport.ProcessCurrent);
    useImport.Persistent.Assign(entities.Persistent);
    useExport.ValidDateInd.Flag = local.ValidDateInd.Flag;

    Call(FnDetermineDueDates.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
    MoveAccrualSuspension(useExport.AccrualSuspension, local.AccrualSuspension);
    local.ValidDateInd.Flag = useExport.ValidDateInd.Flag;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedViews.HardcodeObligor.Type1 =
      useExport.MosCsePersonAccount.Type1;
    local.HardcodedViews.HardcodeObligationMonthlyObligorSummary.Type1 =
      useExport.MosObligation.Type1;
    local.HardcodedViews.HardcodeSupported.Type1 =
      useExport.MspsCsePersonAccount.Type1;
    local.HardcodedViews.HardcodeObligationMonthlySupportedPersonSummary.Type1 =
      useExport.MspsObligation.Type1;
    local.HardcodedViews.HardcodeAccruingClass.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.HardcodedViews.HardcodeAccrualProcess.SystemGeneratedIdentifier =
      useExport.OtrrAccrual.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodeActive.Code = useExport.DdshActiveStatus.Code;
  }

  private void UseFnMakeObligationError()
  {
    var useImport = new FnMakeObligationError.Import();
    var useExport = new FnMakeObligationError.Export();

    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.Obligor.Number = entities.KeyObligor1.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;

    Call(FnMakeObligationError.Execute, useImport, useExport);
  }

  private void UseFnUpdateAccrualInstructions()
  {
    var useImport = new FnUpdateAccrualInstructions.Import();
    var useExport = new FnUpdateAccrualInstructions.Export();

    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.Obligor.Number = entities.KeyObligor1.Number;
    useImport.Debt.SystemGeneratedIdentifier =
      entities.AccrualInstructions1.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.AccrualInstructions.Assign(local.AccrualInstructions);

    Call(FnUpdateAccrualInstructions.Execute, useImport, useExport);
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

  private void UseUpdateProgramProcessingInfo()
  {
    var useImport = new UpdateProgramProcessingInfo.Import();
    var useExport = new UpdateProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);

    Call(UpdateProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void CreateProgramCheckpointRestart()
  {
    var programName = local.Control.ProgramName;
    var updateFrequencyCount =
      local.Control.UpdateFrequencyCount.GetValueOrDefault();
    var readFrequencyCount =
      local.Control.ReadFrequencyCount.GetValueOrDefault();
    var lastCheckpointTimestamp = local.Control.LastCheckpointTimestamp;

    entities.ProgramCheckpointRestart.Populated = false;
    Update("CreateProgramCheckpointRestart",
      (db, command) =>
      {
        db.SetString(command, "programName", programName);
        db.SetNullableInt32(command, "updateFrequencyC", updateFrequencyCount);
        db.SetNullableInt32(command, "readFrequencyCou", readFrequencyCount);
        db.SetNullableInt32(command, "checkpointCount", 0);
        db.
          SetNullableDateTime(command, "lstChkpntTmst", lastCheckpointTimestamp);
          
        db.SetNullableString(command, "restartInd", "");
        db.SetNullableString(command, "restartInfo", "");
      });

    entities.ProgramCheckpointRestart.ProgramName = programName;
    entities.ProgramCheckpointRestart.UpdateFrequencyCount =
      updateFrequencyCount;
    entities.ProgramCheckpointRestart.ReadFrequencyCount = readFrequencyCount;
    entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
      lastCheckpointTimestamp;
    entities.ProgramCheckpointRestart.Populated = true;
  }

  private void DeleteProgramCheckpointRestart()
  {
    Update("DeleteProgramCheckpointRestart",
      (db, command) =>
      {
        db.SetString(
          command, "programName",
          entities.ProgramCheckpointRestart.ProgramName);
      });
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions1.Populated);
    entities.Persistent.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetString(command, "otrType", entities.AccrualInstructions1.Type1);
        db.SetInt32(command, "otyId", entities.AccrualInstructions1.OtyType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.AccrualInstructions1.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.AccrualInstructions1.CpaType);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions1.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.AccrualInstructions1.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Persistent.OtrType = db.GetString(reader, 0);
        entities.Persistent.OtyId = db.GetInt32(reader, 1);
        entities.Persistent.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Persistent.CspNumber = db.GetString(reader, 3);
        entities.Persistent.CpaType = db.GetString(reader, 4);
        entities.Persistent.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.Persistent.AsOfDt = db.GetDate(reader, 6);
        entities.Persistent.DiscontinueDt = db.GetNullableDate(reader, 7);
        entities.Persistent.LastAccrualDt = db.GetNullableDate(reader, 8);
        entities.Persistent.Populated = true;
        CheckValid<AccrualInstructions>("OtrType", entities.Persistent.OtrType);
        CheckValid<AccrualInstructions>("CpaType", entities.Persistent.CpaType);
      });
  }

  private IEnumerable<bool>
    ReadObligationCsePersonAccrualInstructionsObligationType()
  {
    entities.Obligation.Populated = false;
    entities.LegalActionPerson.Populated = false;
    entities.KeyObligor1.Populated = false;
    entities.AccrualInstructions1.Populated = false;
    entities.AccrualInstructions2.Populated = false;
    entities.ObligationType.Populated = false;
    entities.ObligationPaymentSchedule.Populated = false;
    entities.KeySupportedPerson.Populated = false;

    return ReadEach("ReadObligationCsePersonAccrualInstructionsObligationType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.RestartCsePerson.Number);
        db.SetString(command, "number", local.DebugObligor.Number);
        db.
          SetDate(command, "asOfDt", local.ProcessThru.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 0);
        entities.AccrualInstructions1.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.KeyObligor1.Number = db.GetString(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 1);
        entities.AccrualInstructions1.CspNumber = db.GetString(reader, 1);
        entities.AccrualInstructions1.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.AccrualInstructions1.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 3);
        entities.AccrualInstructions1.OtyType = db.GetInt32(reader, 3);
        entities.Obligation.DormantInd = db.GetNullableString(reader, 4);
        entities.AccrualInstructions2.OtrType = db.GetString(reader, 5);
        entities.AccrualInstructions1.Type1 = db.GetString(reader, 5);
        entities.AccrualInstructions2.OtyId = db.GetInt32(reader, 6);
        entities.AccrualInstructions1.OtyType = db.GetInt32(reader, 6);
        entities.AccrualInstructions2.ObgGeneratedId = db.GetInt32(reader, 7);
        entities.AccrualInstructions1.ObgGeneratedId = db.GetInt32(reader, 7);
        entities.AccrualInstructions2.CspNumber = db.GetString(reader, 8);
        entities.AccrualInstructions1.CspNumber = db.GetString(reader, 8);
        entities.AccrualInstructions2.CpaType = db.GetString(reader, 9);
        entities.AccrualInstructions1.CpaType = db.GetString(reader, 9);
        entities.AccrualInstructions2.OtrGeneratedId = db.GetInt32(reader, 10);
        entities.AccrualInstructions1.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.AccrualInstructions2.AsOfDt = db.GetDate(reader, 11);
        entities.AccrualInstructions2.DiscontinueDt =
          db.GetNullableDate(reader, 12);
        entities.AccrualInstructions2.LastAccrualDt =
          db.GetNullableDate(reader, 13);
        entities.ObligationType.Classification = db.GetString(reader, 14);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 15);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 16);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 17);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 18);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 19);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 20);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 21);
        entities.AccrualInstructions1.LapId = db.GetNullableInt32(reader, 21);
        entities.AccrualInstructions1.Amount = db.GetDecimal(reader, 22);
        entities.AccrualInstructions1.CspSupNumber =
          db.GetNullableString(reader, 23);
        entities.KeySupportedPerson.Number = db.GetString(reader, 23);
        entities.AccrualInstructions1.CpaSupType =
          db.GetNullableString(reader, 24);
        entities.Obligation.Populated = true;
        entities.LegalActionPerson.Populated = true;
        entities.KeyObligor1.Populated = true;
        entities.AccrualInstructions1.Populated = true;
        entities.AccrualInstructions2.Populated = true;
        entities.ObligationType.Populated = true;
        entities.ObligationPaymentSchedule.Populated = true;
        entities.KeySupportedPerson.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.AccrualInstructions1.CpaType);
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions2.OtrType);
        CheckValid<ObligationTransaction>("Type1",
          entities.AccrualInstructions1.Type1);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions2.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.AccrualInstructions1.CpaType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.AccrualInstructions1.CpaSupType);

        return true;
      });
  }

  private bool ReadObligationTransactionRlnRsn()
  {
    entities.ObligationTransactionRlnRsn.Populated = false;

    return Read("ReadObligationTransactionRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          local.HardcodedViews.HardcodeAccrualProcess.
            SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.Populated = true;
      });
  }

  private bool ReadProgramCheckpointRestart1()
  {
    entities.ProgramCheckpointRestart.Populated = false;

    return Read("ReadProgramCheckpointRestart1",
      (db, command) =>
      {
        db.SetString(command, "programName", local.Control.ProgramName);
        db.SetInt32(command, "count", local.Loop.Count);
        db.SetNullableDateTime(
          command, "lstChkpntTmst",
          local.Control.LastCheckpointTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProgramCheckpointRestart.ProgramName = db.GetString(reader, 0);
        entities.ProgramCheckpointRestart.UpdateFrequencyCount =
          db.GetNullableInt32(reader, 1);
        entities.ProgramCheckpointRestart.ReadFrequencyCount =
          db.GetNullableInt32(reader, 2);
        entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.ProgramCheckpointRestart.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgramCheckpointRestart2()
  {
    entities.ProgramCheckpointRestart.Populated = false;

    return ReadEach("ReadProgramCheckpointRestart2",
      (db, command) =>
      {
        db.SetString(command, "programName", local.Control.ProgramName);
        db.SetNullableDateTime(
          command, "lstChkpntTmst",
          local.Control.LastCheckpointTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProgramCheckpointRestart.ProgramName = db.GetString(reader, 0);
        entities.ProgramCheckpointRestart.UpdateFrequencyCount =
          db.GetNullableInt32(reader, 1);
        entities.ProgramCheckpointRestart.ReadFrequencyCount =
          db.GetNullableInt32(reader, 2);
        entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.ProgramCheckpointRestart.Populated = true;

        return true;
      });
  }

  private void UpdateProgramCheckpointRestart()
  {
    var readFrequencyCount =
      local.Control.ReadFrequencyCount.GetValueOrDefault() +
      entities.ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault();
    var lastCheckpointTimestamp = local.Control.LastCheckpointTimestamp;

    entities.ProgramCheckpointRestart.Populated = false;
    Update("UpdateProgramCheckpointRestart",
      (db, command) =>
      {
        db.SetNullableInt32(command, "readFrequencyCou", readFrequencyCount);
        db.
          SetNullableDateTime(command, "lstChkpntTmst", lastCheckpointTimestamp);
          
        db.SetString(
          command, "programName",
          entities.ProgramCheckpointRestart.ProgramName);
      });

    entities.ProgramCheckpointRestart.ReadFrequencyCount = readFrequencyCount;
    entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
      lastCheckpointTimestamp;
    entities.ProgramCheckpointRestart.Populated = true;
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
    /// <summary>A HardcodedViewsGroup group.</summary>
    [Serializable]
    public class HardcodedViewsGroup
    {
      /// <summary>
      /// A value of HardcodeMaximumDiscontinue.
      /// </summary>
      [JsonPropertyName("hardcodeMaximumDiscontinue")]
      public DateWorkArea HardcodeMaximumDiscontinue
      {
        get => hardcodeMaximumDiscontinue ??= new();
        set => hardcodeMaximumDiscontinue = value;
      }

      /// <summary>
      /// A value of HardcodeObligor.
      /// </summary>
      [JsonPropertyName("hardcodeObligor")]
      public MonthlyObligorSummary HardcodeObligor
      {
        get => hardcodeObligor ??= new();
        set => hardcodeObligor = value;
      }

      /// <summary>
      /// A value of HardcodeObligationMonthlyObligorSummary.
      /// </summary>
      [JsonPropertyName("hardcodeObligationMonthlyObligorSummary")]
      public MonthlyObligorSummary HardcodeObligationMonthlyObligorSummary
      {
        get => hardcodeObligationMonthlyObligorSummary ??= new();
        set => hardcodeObligationMonthlyObligorSummary = value;
      }

      /// <summary>
      /// A value of HardcodeSupported.
      /// </summary>
      [JsonPropertyName("hardcodeSupported")]
      public MonthlySupportedPersonSummary HardcodeSupported
      {
        get => hardcodeSupported ??= new();
        set => hardcodeSupported = value;
      }

      /// <summary>
      /// A value of HardcodeObligationMonthlySupportedPersonSummary.
      /// </summary>
      [JsonPropertyName("hardcodeObligationMonthlySupportedPersonSummary")]
      public MonthlySupportedPersonSummary HardcodeObligationMonthlySupportedPersonSummary
        
      {
        get => hardcodeObligationMonthlySupportedPersonSummary ??= new();
        set => hardcodeObligationMonthlySupportedPersonSummary = value;
      }

      /// <summary>
      /// A value of HardcodeAccruingClass.
      /// </summary>
      [JsonPropertyName("hardcodeAccruingClass")]
      public ObligationType HardcodeAccruingClass
      {
        get => hardcodeAccruingClass ??= new();
        set => hardcodeAccruingClass = value;
      }

      /// <summary>
      /// A value of HardcodeAccrualProcess.
      /// </summary>
      [JsonPropertyName("hardcodeAccrualProcess")]
      public ObligationTransactionRlnRsn HardcodeAccrualProcess
      {
        get => hardcodeAccrualProcess ??= new();
        set => hardcodeAccrualProcess = value;
      }

      /// <summary>
      /// A value of HardcodeActive.
      /// </summary>
      [JsonPropertyName("hardcodeActive")]
      public DebtDetailStatusHistory HardcodeActive
      {
        get => hardcodeActive ??= new();
        set => hardcodeActive = value;
      }

      private DateWorkArea hardcodeMaximumDiscontinue;
      private MonthlyObligorSummary hardcodeObligor;
      private MonthlyObligorSummary hardcodeObligationMonthlyObligorSummary;
      private MonthlySupportedPersonSummary hardcodeSupported;
      private MonthlySupportedPersonSummary hardcodeObligationMonthlySupportedPersonSummary;
        
      private ObligationType hardcodeAccruingClass;
      private ObligationTransactionRlnRsn hardcodeAccrualProcess;
      private DebtDetailStatusHistory hardcodeActive;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of Debt.
      /// </summary>
      [JsonPropertyName("debt")]
      public ObligationTransaction Debt
      {
        get => debt ??= new();
        set => debt = value;
      }

      /// <summary>
      /// A value of DebtDetail.
      /// </summary>
      [JsonPropertyName("debtDetail")]
      public DebtDetail DebtDetail
      {
        get => debtDetail ??= new();
        set => debtDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private ObligationTransaction debt;
      private DebtDetail debtDetail;
    }

    /// <summary>
    /// A value of PreviousProcessDate.
    /// </summary>
    [JsonPropertyName("previousProcessDate")]
    public DateWorkArea PreviousProcessDate
    {
      get => previousProcessDate ??= new();
      set => previousProcessDate = value;
    }

    /// <summary>
    /// A value of ValidDateInd.
    /// </summary>
    [JsonPropertyName("validDateInd")]
    public Common ValidDateInd
    {
      get => validDateInd ??= new();
      set => validDateInd = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of Timestamp.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateWorkArea Timestamp
    {
      get => timestamp ??= new();
      set => timestamp = value;
    }

    /// <summary>
    /// A value of Loop.
    /// </summary>
    [JsonPropertyName("loop")]
    public Common Loop
    {
      get => loop ??= new();
      set => loop = value;
    }

    /// <summary>
    /// A value of Control.
    /// </summary>
    [JsonPropertyName("control")]
    public ProgramCheckpointRestart Control
    {
      get => control ??= new();
      set => control = value;
    }

    /// <summary>
    /// A value of LastRecordProcessedCsePerson.
    /// </summary>
    [JsonPropertyName("lastRecordProcessedCsePerson")]
    public CsePerson LastRecordProcessedCsePerson
    {
      get => lastRecordProcessedCsePerson ??= new();
      set => lastRecordProcessedCsePerson = value;
    }

    /// <summary>
    /// A value of LastRecordProcessedObligation.
    /// </summary>
    [JsonPropertyName("lastRecordProcessedObligation")]
    public Obligation LastRecordProcessedObligation
    {
      get => lastRecordProcessedObligation ??= new();
      set => lastRecordProcessedObligation = value;
    }

    /// <summary>
    /// Gets a value of HardcodedViews.
    /// </summary>
    [JsonPropertyName("hardcodedViews")]
    public HardcodedViewsGroup HardcodedViews
    {
      get => hardcodedViews ?? (hardcodedViews = new());
      set => hardcodedViews = value;
    }

    /// <summary>
    /// A value of RestartDueToError.
    /// </summary>
    [JsonPropertyName("restartDueToError")]
    public Common RestartDueToError
    {
      get => restartDueToError ??= new();
      set => restartDueToError = value;
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
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of LeftPad.
    /// </summary>
    [JsonPropertyName("leftPad")]
    public TextWorkArea LeftPad
    {
      get => leftPad ??= new();
      set => leftPad = value;
    }

    /// <summary>
    /// A value of DebugObligor.
    /// </summary>
    [JsonPropertyName("debugObligor")]
    public CsePerson DebugObligor
    {
      get => debugObligor ??= new();
      set => debugObligor = value;
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
    /// A value of RestartCsePerson.
    /// </summary>
    [JsonPropertyName("restartCsePerson")]
    public CsePerson RestartCsePerson
    {
      get => restartCsePerson ??= new();
      set => restartCsePerson = value;
    }

    /// <summary>
    /// A value of RestartObligation.
    /// </summary>
    [JsonPropertyName("restartObligation")]
    public Obligation RestartObligation
    {
      get => restartObligation ??= new();
      set => restartObligation = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of FirstDom.
    /// </summary>
    [JsonPropertyName("firstDom")]
    public DateWorkArea FirstDom
    {
      get => firstDom ??= new();
      set => firstDom = value;
    }

    /// <summary>
    /// A value of ProcessThru.
    /// </summary>
    [JsonPropertyName("processThru")]
    public DateWorkArea ProcessThru
    {
      get => processThru ??= new();
      set => processThru = value;
    }

    /// <summary>
    /// A value of ProcessThruBackup.
    /// </summary>
    [JsonPropertyName("processThruBackup")]
    public DateWorkArea ProcessThruBackup
    {
      get => processThruBackup ??= new();
      set => processThruBackup = value;
    }

    /// <summary>
    /// A value of ErrorCsePerson.
    /// </summary>
    [JsonPropertyName("errorCsePerson")]
    public CsePerson ErrorCsePerson
    {
      get => errorCsePerson ??= new();
      set => errorCsePerson = value;
    }

    /// <summary>
    /// A value of ErrorObligation.
    /// </summary>
    [JsonPropertyName("errorObligation")]
    public Obligation ErrorObligation
    {
      get => errorObligation ??= new();
      set => errorObligation = value;
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
    /// A value of AccInstrReadSinceChkpt.
    /// </summary>
    [JsonPropertyName("accInstrReadSinceChkpt")]
    public Common AccInstrReadSinceChkpt
    {
      get => accInstrReadSinceChkpt ??= new();
      set => accInstrReadSinceChkpt = value;
    }

    /// <summary>
    /// A value of ObligReadSinceChkpnt.
    /// </summary>
    [JsonPropertyName("obligReadSinceChkpnt")]
    public Common ObligReadSinceChkpnt
    {
      get => obligReadSinceChkpnt ??= new();
      set => obligReadSinceChkpnt = value;
    }

    /// <summary>
    /// A value of ToComputeBucketsFor.
    /// </summary>
    [JsonPropertyName("toComputeBucketsFor")]
    public CsePerson ToComputeBucketsFor
    {
      get => toComputeBucketsFor ??= new();
      set => toComputeBucketsFor = value;
    }

    /// <summary>
    /// A value of ToComputeBucketsFrom.
    /// </summary>
    [JsonPropertyName("toComputeBucketsFrom")]
    public CsePersonAccount ToComputeBucketsFrom
    {
      get => toComputeBucketsFrom ??= new();
      set => toComputeBucketsFrom = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of AccrualDiscontinue.
    /// </summary>
    [JsonPropertyName("accrualDiscontinue")]
    public DateWorkArea AccrualDiscontinue
    {
      get => accrualDiscontinue ??= new();
      set => accrualDiscontinue = value;
    }

    /// <summary>
    /// A value of ProcessFrom.
    /// </summary>
    [JsonPropertyName("processFrom")]
    public DateWorkArea ProcessFrom
    {
      get => processFrom ??= new();
      set => processFrom = value;
    }

    /// <summary>
    /// A value of ProcessCurrent.
    /// </summary>
    [JsonPropertyName("processCurrent")]
    public DateWorkArea ProcessCurrent
    {
      get => processCurrent ??= new();
      set => processCurrent = value;
    }

    /// <summary>
    /// A value of ProcessCurrentEnd.
    /// </summary>
    [JsonPropertyName("processCurrentEnd")]
    public DateWorkArea ProcessCurrentEnd
    {
      get => processCurrentEnd ??= new();
      set => processCurrentEnd = value;
    }

    /// <summary>
    /// A value of OtrnReadSinceChkpnt.
    /// </summary>
    [JsonPropertyName("otrnReadSinceChkpnt")]
    public Common OtrnReadSinceChkpnt
    {
      get => otrnReadSinceChkpnt ??= new();
      set => otrnReadSinceChkpnt = value;
    }

    /// <summary>
    /// A value of IsDebtCreated.
    /// </summary>
    [JsonPropertyName("isDebtCreated")]
    public Common IsDebtCreated
    {
      get => isDebtCreated ??= new();
      set => isDebtCreated = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of UpdatesSinceLastCommit.
    /// </summary>
    [JsonPropertyName("updatesSinceLastCommit")]
    public Common UpdatesSinceLastCommit
    {
      get => updatesSinceLastCommit ??= new();
      set => updatesSinceLastCommit = value;
    }

    /// <summary>
    /// A value of OtrnCreatedSinceChkpnt.
    /// </summary>
    [JsonPropertyName("otrnCreatedSinceChkpnt")]
    public Common OtrnCreatedSinceChkpnt
    {
      get => otrnCreatedSinceChkpnt ??= new();
      set => otrnCreatedSinceChkpnt = value;
    }

    /// <summary>
    /// A value of AccInstrUptdSinceChkpt.
    /// </summary>
    [JsonPropertyName("accInstrUptdSinceChkpt")]
    public Common AccInstrUptdSinceChkpt
    {
      get => accInstrUptdSinceChkpt ??= new();
      set => accInstrUptdSinceChkpt = value;
    }

    /// <summary>
    /// A value of LastProcessed.
    /// </summary>
    [JsonPropertyName("lastProcessed")]
    public CsePerson LastProcessed
    {
      get => lastProcessed ??= new();
      set => lastProcessed = value;
    }

    /// <summary>
    /// A value of AccInstrReadInThisRun.
    /// </summary>
    [JsonPropertyName("accInstrReadInThisRun")]
    public Common AccInstrReadInThisRun
    {
      get => accInstrReadInThisRun ??= new();
      set => accInstrReadInThisRun = value;
    }

    /// <summary>
    /// A value of AccInstrUptdInThisRun.
    /// </summary>
    [JsonPropertyName("accInstrUptdInThisRun")]
    public Common AccInstrUptdInThisRun
    {
      get => accInstrUptdInThisRun ??= new();
      set => accInstrUptdInThisRun = value;
    }

    /// <summary>
    /// A value of ObligReadInThisRun.
    /// </summary>
    [JsonPropertyName("obligReadInThisRun")]
    public Common ObligReadInThisRun
    {
      get => obligReadInThisRun ??= new();
      set => obligReadInThisRun = value;
    }

    /// <summary>
    /// A value of OtrnCreatedInThisRun.
    /// </summary>
    [JsonPropertyName("otrnCreatedInThisRun")]
    public Common OtrnCreatedInThisRun
    {
      get => otrnCreatedInThisRun ??= new();
      set => otrnCreatedInThisRun = value;
    }

    /// <summary>
    /// A value of OtrnReadInThisRun.
    /// </summary>
    [JsonPropertyName("otrnReadInThisRun")]
    public Common OtrnReadInThisRun
    {
      get => otrnReadInThisRun ??= new();
      set => otrnReadInThisRun = value;
    }

    /// <summary>
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    private DateWorkArea previousProcessDate;
    private Common validDateInd;
    private Common commit;
    private DateWorkArea blank;
    private DateWorkArea timestamp;
    private Common loop;
    private ProgramCheckpointRestart control;
    private CsePerson lastRecordProcessedCsePerson;
    private Obligation lastRecordProcessedObligation;
    private HardcodedViewsGroup hardcodedViews;
    private Common restartDueToError;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea processDate;
    private TextWorkArea leftPad;
    private CsePerson debugObligor;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson restartCsePerson;
    private Obligation restartObligation;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea current;
    private DateWorkArea firstDom;
    private DateWorkArea processThru;
    private DateWorkArea processThruBackup;
    private CsePerson errorCsePerson;
    private Obligation errorObligation;
    private External passArea;
    private Common accInstrReadSinceChkpt;
    private Common obligReadSinceChkpnt;
    private CsePerson toComputeBucketsFor;
    private CsePersonAccount toComputeBucketsFrom;
    private AccrualInstructions accrualInstructions;
    private DateWorkArea accrualDiscontinue;
    private DateWorkArea processFrom;
    private DateWorkArea processCurrent;
    private DateWorkArea processCurrentEnd;
    private Common otrnReadSinceChkpnt;
    private Common isDebtCreated;
    private Array<LocalGroup> local1;
    private DebtDetail debtDetail;
    private Common updatesSinceLastCommit;
    private Common otrnCreatedSinceChkpnt;
    private Common accInstrUptdSinceChkpt;
    private CsePerson lastProcessed;
    private Common accInstrReadInThisRun;
    private Common accInstrUptdInThisRun;
    private Common obligReadInThisRun;
    private Common otrnCreatedInThisRun;
    private Common otrnReadInThisRun;
    private Common subscript;
    private AccrualSuspension accrualSuspension;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public AccrualInstructions Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of KeyObligor1.
    /// </summary>
    [JsonPropertyName("keyObligor1")]
    public CsePerson KeyObligor1
    {
      get => keyObligor1 ??= new();
      set => keyObligor1 = value;
    }

    /// <summary>
    /// A value of AccrualInstructions1.
    /// </summary>
    [JsonPropertyName("accrualInstructions1")]
    public ObligationTransaction AccrualInstructions1
    {
      get => accrualInstructions1 ??= new();
      set => accrualInstructions1 = value;
    }

    /// <summary>
    /// A value of AccrualInstructions2.
    /// </summary>
    [JsonPropertyName("accrualInstructions2")]
    public AccrualInstructions AccrualInstructions2
    {
      get => accrualInstructions2 ??= new();
      set => accrualInstructions2 = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of KeySupportedPerson.
    /// </summary>
    [JsonPropertyName("keySupportedPerson")]
    public CsePerson KeySupportedPerson
    {
      get => keySupportedPerson ??= new();
      set => keySupportedPerson = value;
    }

    /// <summary>
    /// A value of KeySupported.
    /// </summary>
    [JsonPropertyName("keySupported")]
    public CsePersonAccount KeySupported
    {
      get => keySupported ??= new();
      set => keySupported = value;
    }

    /// <summary>
    /// A value of KeyObligor2.
    /// </summary>
    [JsonPropertyName("keyObligor2")]
    public CsePersonAccount KeyObligor2
    {
      get => keyObligor2 ??= new();
      set => keyObligor2 = value;
    }

    private AccrualInstructions persistent;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private Obligation obligation;
    private LegalActionPerson legalActionPerson;
    private CsePerson keyObligor1;
    private ObligationTransaction accrualInstructions1;
    private AccrualInstructions accrualInstructions2;
    private ObligationType obligationType;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePerson keySupportedPerson;
    private CsePersonAccount keySupported;
    private CsePersonAccount keyObligor2;
  }
#endregion
}
