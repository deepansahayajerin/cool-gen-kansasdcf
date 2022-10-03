// Program: SP_B360_HISTORY_OPTIMIZATION, ID: 374372813, model: 746.
// Short name: SWEP360B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B360_HISTORY_OPTIMIZATION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB360HistoryOptimization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B360_HISTORY_OPTIMIZATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB360HistoryOptimization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB360HistoryOptimization.
  /// </summary>
  public SpB360HistoryOptimization(IContext context, Import import,
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
    // ----------------------------------------------------------------------------------------------
    //                   M A I N T E N A N C E    L O G
    // ----------------------------------------------------------------------------------------------
    //   Date   Developer 	PR#/WR#  	Description
    // MAR 2000 SWSRCHF  	H00077472 	Initial development:
    //                             		
    // Optimize(delete) the duplicate
    // Infrastructure(history)
    //                                         
    // records. If a Narrative or Monitored
    // Document exists,
    //                                         
    // the Infrastructure (history) record is
    // not deleted.
    // 05/15/00 SWSRCHF  	H00095120 	Do not delete records where the Last 
    // Updated By is
    //                             		
    // equal to "PARTIAL".
    // 06/06/00 SWSRCHF   	000170   	Replace the check for Narrative(s) with a 
    // check for
    //                             		
    // Narrative Detail(s)
    // 09/10/02 SWSRPRM	PR # 147300	Delete History batch (Job # 124) is not 
    // deleting all
    // 					records it should.
    // 07/07/20 SWCCRXS(Raj)   CQ66150         Modified to add reason code 
    // DMEMANCIP45 exempt list
    //                                         
    // in duplicate check & delete.
    // ----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ***
    // *** Set initial values
    //  ***
    local.ProgramProcessingInfo.Name = "SWEPB360";
    local.ProgramCheckpointRestart.ProgramName = "SWEPB360";
    local.CurrentDateWorkArea.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);
    local.RowsReadCtr.Count = 0;
    local.RowsDeletedCtr.Count = 0;
    local.RepeatLoopCtr.Count = 0;
    local.CommitCtr.Count = 0;

    // ***
    // *** Open the ERROR RPT. DDNAME=RPT99.
    // ***
    local.Report.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = local.CurrentDateWorkArea.Date;
    UseCabErrorReport1();

    if (!Equal(local.Report.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***
    // *** Open the CONTROL RPT. DDNAME=RPT98.
    // ***
    local.Report.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = local.CurrentDateWorkArea.Date;
    UseCabControlReport1();

    if (!Equal(local.Report.Status, "OK"))
    {
      // ***
      // *** Write a line to the ERROR RPT.
      // ***
      local.Report.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered, while opening the Control Report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***
    // *** Get the run parameters for this program.
    // ***
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ***
      // *** Does the Parameter List contain a date range???
      // ***
      if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
      {
        // ***
        // *** Get the range Start Date
        // ***
        local.BeginRange.Date =
          IntToDate((int)StringToNumber(Substring(
            local.ProgramProcessingInfo.ParameterList, 1, 8)));

        // ***
        // *** Get the range End Date
        // ***
        local.EndRange.Date =
          IntToDate((int)StringToNumber(Substring(
            local.ProgramProcessingInfo.ParameterList, 9, 8)));

        // ***
        // *** Is Start date > End date???
        // ***
        if (Lt(local.EndRange.Date, local.BeginRange.Date))
        {
          // ***
          // *** Write a line to the ERROR RPT.
          // ***
          local.Report.Action = "WRITE";
          local.NeededToWrite.RptDetail = "The Start date " + NumberToString
            (DateToInt(local.BeginRange.Date), 15) + " is greater than the End date " +
            NumberToString(DateToInt(local.EndRange.Date), 15);
          UseCabErrorReport2();
          local.BeginRange.Date = local.Init1.Date;
          local.EndRange.Date = local.Init1.Date;
        }
      }
      else
      {
        local.BeginRange.Date = Now().Date.AddDays(-1);
        local.EndRange.Date = local.Max.Date;

        // ***
        // *** Set to Process Date from the PPI table
        // ***
        local.CurrentDateWorkArea.Date =
          local.ProgramProcessingInfo.ProcessDate;
      }

      if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Init1.Date) || Equal
        (local.ProgramProcessingInfo.ProcessDate, local.Max.Date))
      {
        // ***
        // *** Bypass - previously set to CURRENT(system) date
        // ***
      }
    }
    else
    {
      // ***
      // *** Write a line to the ERROR RPT.
      // ***
      local.Report.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered, while reading the Program Processing Info table.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***
    // *** Get the DB2 commit frequency count and restart data.
    // ***
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
      {
        // ***
        // *** Get the following Infrastructure data from the Restart_Info:
        //  ***
        //   ***      Case Number       start position  1, length 10,
        //    ***     Event Id          start position 11, length  15,
        //    ***     Reason Code       start position 26, length 15,
        //    ***     Created Timestamp start position 41, length 26,
        //    ***     Case Unit Number  start position 67, length  15
        //    ***
        local.Previous.CaseNumber =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
        local.Previous.EventId =
          (int)StringToNumber(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250, 11, 15));
        local.Previous.ReasonCode =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 26, 15);
        local.BatchTimestampWorkArea.TextTimestamp =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 41, 26);
        local.Previous.CaseUnitNumber =
          (int?)StringToNumber(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250, 67, 15));

        // ***
        // *** convert TEXT to a Timestamp
        // ***
        UseLeCabConvertTimestamp();
        local.Previous.CreatedTimestamp =
          local.BatchTimestampWorkArea.IefTimestamp;
        local.PreviousInfrastructure.Date =
          Date(local.BatchTimestampWorkArea.IefTimestamp);
      }
    }
    else
    {
      // ***
      // *** Write a line to the ERROR RPT.
      // ***
      local.Report.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered, while reading the Checkpoint Restart table.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***********************************************************
    //    ***        M A I N   P R O C E S S I N G   L O O P        ***
    //   ***         
    // ---------------------------------------
    // ***
    //  ***
    // 
    // ***
    // *** ====> Start <====
    // 
    // ***
    // ***
    // 
    // ***
    // *** Get each INFRASTRUCTURE row (History record) meeting the    ***
    // *** following criteria:
    // 
    // ***
    // ***  1)  Process Status = "H"
    // 
    // ***
    // ***  2)  Case Unit Number > 0 (zero)                            ***
    // ***  3)  Date(Created Timestamp) >= Process Date                ***
    // ***   or Date(Created Timestamp)  >= Start Date range           ***
    // ***                           and <= End Date range             ***
    // ***  4)  matches Event Id/Reason Code combination               ***
    // ***
    // 
    // ***
    // *** Sort Order : ascending
    // 
    // ***
    // *** Sort Fields: CASE NUMBER, EVENT ID, REASON CODE,            ***
    // ***              CREATED TIMESTAMP, CASE UNIT NUMBER            ***
    //  
    // *****************************************************************
    // -----------------
    // PR # 147300 start
    // -----------------
    foreach(var item in ReadInfrastructure())
    {
      // ***
      // *** Increment the READ and COMMIT accumulators
      // ***
      ++local.RowsReadCtr.Count;
      ++local.CommitCtr.Count;

      // *** Problem report H00095120
      // *** start
      if (Equal(entities.ExistingInfrastructure.LastUpdatedBy, "PARTIAL"))
      {
        continue;
      }

      // *** end H00095120
      switch(entities.ExistingInfrastructure.EventId)
      {
        case 5:
          if (Equal(entities.ExistingInfrastructure.ReasonCode,
            "CASREOPENPATEST") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "CASREOPENPATUNK"))
          {
            // ***
            // *** Do NOT delete duplicates for these Event Id/Reason Code 
            // combinations
            // ***
            continue;
          }

          break;
        case 7:
          if (Equal(entities.ExistingInfrastructure.ReasonCode,
            "ISAPFA_OBG_ESTB") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "NOTAP") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "PATESTAB_LOC") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "ISAPMO_OBG_ESTB"))
          {
            // ***
            // *** Do NOT delete duplicates for these Event Id/Reason Code 
            // combinations
            // ***
            continue;
          }

          break;
        case 9:
          if (Equal(entities.ExistingInfrastructure.ReasonCode, "ARWAIVHINS") ||
            Equal
            (entities.ExistingInfrastructure.ReasonCode, "EMANCIPAFUTR") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "LEPRNTLRIGHTS"))
          {
            // ***
            // *** Do NOT delete duplicates for these Event Id/Reason Code 
            // combinations
            // ***
            continue;
          }

          break;
        case 11:
          if (Equal(entities.ExistingInfrastructure.ReasonCode,
            "OBLPAIDOTHRCASE") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "PATNOLONGER") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "APBECOMEAR") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "ARBECOMEAP") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "AP_DISCONTINUED") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "CU_DISCONTINUED") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "CASARCHG") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "PATESTAB") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "OBLGOTHRCAS") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "CH_DISCONTINUED") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "EXPPAT") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "DELETECAU") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "CAUARCHG") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "CHBECOMESAR") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "CHNGMONACTV"))
          {
            // ***
            // *** Do NOT delete duplicates for these Event Id/Reason Code 
            // combinations
            // ***
            continue;
          }

          break;
        case 18:
          // +
          // -----------------------------------------------------------
          // +
          // +CQ66150 Included Reason code DMEMANCIP45 to skip duplicate +
          // +Check & Delete.
          // 
          // +
          // +
          // -----------------------------------------------------------
          // +
          if (Equal(entities.ExistingInfrastructure.ReasonCode, "DMCONTSTGT") ||
            Equal(entities.ExistingInfrastructure.ReasonCode, "DMEMANCIP") || Equal
            (entities.ExistingInfrastructure.ReasonCode, "DMEMANCIP45"))
          {
            // ***
            // *** Do NOT delete duplicates for these Event Id/Reason Code 
            // combinations
            // ***
            continue;
          }

          break;
        case 26:
          // ***
          // *** Do NOT delete duplicates for these Event Id/Reason Code 
          // combinations
          // ***
          continue;
        default:
          break;
      }

      // ***
      // *** Save the CURRENT (Case Number, Event Type, Event Detail Name,
      // *** and Case Unit Number) values.
      // ***
      local.CurrentInfrastructure2.Assign(entities.ExistingInfrastructure);

      // ***
      // *** Get the Date from the Created Timestamp
      // ***
      local.CurrentInfrastructure1.Date =
        Date(entities.ExistingInfrastructure.CreatedTimestamp);

      if (!Equal(local.CurrentInfrastructure2.CaseNumber,
        local.Previous.CaseNumber) || local
        .CurrentInfrastructure2.DenormNumeric12.GetValueOrDefault() != local
        .Previous.DenormNumeric12.GetValueOrDefault() || local
        .CurrentInfrastructure2.EventId != local.Previous.EventId || !
        Equal(local.CurrentInfrastructure2.ReasonCode, local.Previous.ReasonCode)
        || !
        Equal(local.CurrentInfrastructure1.Date,
        local.PreviousInfrastructure.Date) || IsEmpty
        (local.FirstTimeThru.Flag))
      {
        local.Lowest.CaseUnitNumber =
          entities.ExistingInfrastructure.CaseUnitNumber;
      }

      // *** Work Request 000170 start
      // ***
      // *** Does the Infrastructure (History) record have any Narrative Details
      // (s)???
      // ***
      foreach(var item1 in ReadNarrativeDetail())
      {
        // ***
        // *** Increment the COMMIT accumulator
        // ***
        ++local.CommitCtr.Count;
        local.Previous.Assign(local.CurrentInfrastructure2);
        local.PreviousInfrastructure.Date = local.CurrentInfrastructure1.Date;

        // ***
        // *** bypass DELETE processing
        // ***
        goto ReadEach;
      }

      // *** Work Request 000170 end
      // ***
      // *** Does the Infrastructure (History) record have any Monitored 
      // Documents???
      // ***
      foreach(var item1 in ReadOutgoingDocumentMonitoredDocument())
      {
        // ***
        // *** Increment the COMMIT accumulator
        // ***
        local.CommitCtr.Count += 2;
        local.Previous.Assign(local.CurrentInfrastructure2);
        local.PreviousInfrastructure.Date = local.CurrentInfrastructure1.Date;

        // ***
        // *** bypass DELETE processing
        // ***
        goto ReadEach;
      }

      if (IsEmpty(local.FirstTimeThru.Flag))
      {
        // ***
        // *** First Time Thru processing
        // ***
        local.FirstTimeThru.Flag = "N";
        local.Previous.Assign(local.CurrentInfrastructure2);
        local.PreviousInfrastructure.Date = local.CurrentInfrastructure1.Date;

        // ***
        // *** bypass DELETE processing
        // ***
        continue;
      }

      if (Equal(local.CurrentInfrastructure2.CaseNumber,
        local.Previous.CaseNumber) && local.CurrentInfrastructure2.EventId == local
        .Previous.EventId && Equal
        (local.CurrentInfrastructure2.ReasonCode, local.Previous.ReasonCode) &&
        Equal
        (local.CurrentInfrastructure1.Date, local.PreviousInfrastructure.Date) &&
        local.CurrentInfrastructure2.CaseUnitNumber.GetValueOrDefault() > local
        .Lowest.CaseUnitNumber.GetValueOrDefault() && local
        .CurrentInfrastructure2.DenormNumeric12.GetValueOrDefault() == local
        .Previous.DenormNumeric12.GetValueOrDefault())
      {
        // *** Perform DELETE processing
        //  ***
        //   ***     DELETE the Infrastructure (History) record where
        //    ***        current Case Number = previous Case Number,
        //     ***       current Event Id = previous Event Id,
        //    ***        current Reason Code = previous Reason Code,
        //   ***         current Date = previous Date
        //  ***          current Case Unit Number > previous Case Unit Number
        // ***
        DeleteInfrastructure();

        // ***
        // *** Increment the DELETE and COMMIT accumulators
        // ***
        ++local.RowsDeletedCtr.Count;
        ++local.CommitCtr.Count;

        if (local.CommitCtr.Count >= local
          .ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault())
        {
          // ***
          // *** convert a Timestamp to TEXT
          // ***
          local.BatchTimestampWorkArea.IefTimestamp =
            local.CurrentInfrastructure2.CreatedTimestamp;
          UseLeCabConvertTimestamp();

          // ***
          // *** Put the following Infrastructure data into the Restart_Info:
          //  ***
          //   ***      Case Number       start position  1, length 10,
          //    ***     Event Id          start position 11, length  15,
          //    ***     Reason Code       start position 26, length 15,
          //    ***     Created Timestamp start position 41, length 26,
          //    ***     Case Unit Number  start position 67, length  15
          //    ***
          local.ProgramCheckpointRestart.RestartInfo =
            (local.CurrentInfrastructure2.CaseNumber ?? "") + NumberToString
            (local.CurrentInfrastructure2.EventId, 15) + local
            .CurrentInfrastructure2.ReasonCode + local
            .BatchTimestampWorkArea.TextTimestamp + NumberToString
            (local.CurrentInfrastructure2.CaseUnitNumber.GetValueOrDefault(), 15);
            
          local.ProgramCheckpointRestart.RestartInd = "Y";

          // ***
          // *** UPDATE the Program Checkpoint Restart table
          // ***
          UseUpdatePgmCheckpointRestart();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ***
            // *** Write a line to the ERROR RPT.
            // ***
            local.Report.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered, while updating the Checkpoint Restart table.";
              
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // ***
          // *** Call an external that does a DB2 commit using a COBOL program.
          // ***
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            // ***
            // *** Write a line to the ERROR RPT.
            // ***
            local.Report.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered, while performing a commit.";
            UseCabErrorReport2();
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          local.CommitCtr.Count = 0;
        }

        local.Previous.Assign(local.CurrentInfrastructure2);
        local.PreviousInfrastructure.Date = local.CurrentInfrastructure1.Date;
        local.LowestCaseUnitNumber.Flag = "";
      }
      else
      {
        // ***
        // *** bypass DELETE processing
        // ***
        local.Previous.Assign(local.CurrentInfrastructure2);
        local.PreviousInfrastructure.Date = local.CurrentInfrastructure1.Date;

        continue;
      }

      // -----------------
      // PR # 147300 end
      // -----------------
ReadEach:
      ;
    }

    // *****************************************************************
    // *** ====> End <====
    // 
    // ***
    // ***
    // 
    // ***
    //  ***          M A I N   P R O C E S S I N G   L O O P          ***
    //   ***         
    // ---------------------------------------
    // ***
    //    
    // *************************************************************
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";

    // ***
    // *** UPDATE the Program Checkpoint Restart table, one final time
    // ***
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ***
      // *** Write a line to the ERROR RPT.
      // ***
      local.Report.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered, while performing the final update to the Checkpoint Restart table.";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***
    // *** Call an external that does a DB2 commit using a COBOL program.
    // ***
    UseExtToDoACommit();

    if (local.PassArea.NumericReturnCode != 0)
    {
      // ***
      // *** Write a line to the ERROR RPT.
      // ***
      local.Report.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered, while performing the final commit.";
      UseCabErrorReport2();
      ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

      return;
    }

    do
    {
      // ***
      // *** Increment the LOOP accumulator
      // ***
      ++local.RepeatLoopCtr.Count;

      // ***
      // *** Write a line to the CONTROL RPT.
      // ***
      local.Report.Action = "WRITE";

      switch(local.RepeatLoopCtr.Count)
      {
        case 1:
          local.Repeat.Flag = "Y";
          local.Position.Count = 1;

          do
          {
            if (Equal(NumberToString(
              local.RowsReadCtr.Count, local.Position.Count, 1), "0"))
            {
              if (local.Position.Count == 15)
              {
                // ***
                // *** NO records read
                // ***
                local.NeededToWrite.RptDetail =
                  "Infrastructure records Read   :   " + "0";
                local.Repeat.Flag = "N";
              }
              else
              {
                ++local.Position.Count;
              }
            }
            else
            {
              local.Repeat.Flag = "N";
              local.FirstNonZero.Count = local.Position.Count;
              local.LastZero.Count = local.Position.Count - 1;

              // ***
              // *** Remove leading zeroes
              // ***
              local.NeededToWrite.RptDetail =
                "Infrastructure records Read   :   " + NumberToString
                (local.RowsReadCtr.Count, local.FirstNonZero.Count, 15 -
                local.LastZero.Count);
            }
          }
          while(AsChar(local.Repeat.Flag) != 'N');

          break;
        case 2:
          local.Repeat.Flag = "Y";
          local.Position.Count = 1;

          do
          {
            if (Equal(NumberToString(
              local.RowsDeletedCtr.Count, local.Position.Count, 1), "0"))
            {
              if (local.Position.Count == 15)
              {
                // ***
                // *** NO records deleted
                // ***
                local.NeededToWrite.RptDetail =
                  "Infrastructure records Deleted:   " + "0";
                local.Repeat.Flag = "N";
              }
              else
              {
                ++local.Position.Count;
              }
            }
            else
            {
              local.Repeat.Flag = "N";
              local.FirstNonZero.Count = local.Position.Count;
              local.LastZero.Count = local.Position.Count - 1;

              // ***
              // *** Remove leading zeroes
              // ***
              local.NeededToWrite.RptDetail =
                "Infrastructure records Deleted:   " + NumberToString
                (local.RowsDeletedCtr.Count, local.FirstNonZero.Count, 15 -
                local.LastZero.Count);
            }
          }
          while(AsChar(local.Repeat.Flag) != 'N');

          break;
        case 3:
          local.NeededToWrite.RptDetail = "";

          break;
        case 4:
          local.Work1.TextDate =
            NumberToString(DateToInt(local.CurrentDateWorkArea.Date), 8, 8);
          local.NeededToWrite.RptDetail =
            "History records processed for date " + Substring
            (local.Work1.TextDate, DateWorkArea.TextDate_MaxLength, 5, 2) + "-"
            + Substring
            (local.Work1.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2) + "-"
            + Substring
            (local.Work1.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4);

          break;
        case 5:
          if (Equal(local.BeginRange.Date, local.Init1.Date))
          {
            local.NeededToWrite.RptDetail = "";

            break;
          }

          local.Work1.TextDate =
            NumberToString(DateToInt(local.BeginRange.Date), 8, 8);
          local.Work2.TextDate =
            NumberToString(DateToInt(local.EndRange.Date), 8, 8);
          local.NeededToWrite.RptDetail =
            "History records processed for date range Starting " + Substring
            (local.Work1.TextDate, DateWorkArea.TextDate_MaxLength, 5, 2) + "-"
            + Substring
            (local.Work1.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2) + "-"
            + Substring
            (local.Work1.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4) + " and Ending " +
            Substring
            (local.Work2.TextDate, DateWorkArea.TextDate_MaxLength, 5, 2) + "-"
            + Substring
            (local.Work2.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2) + "-"
            + Substring
            (local.Work2.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4);

          break;
        case 6:
          local.NeededToWrite.RptDetail = "";

          break;
        default:
          break;
      }

      UseCabControlReport2();

      if (!Equal(local.Report.Status, "OK"))
      {
        // ***
        // *** Write a line to the ERROR RPT.
        // ***
        local.Report.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered, while writing to the Control Report.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }
    while(local.RepeatLoopCtr.Count != 6);

    // ***
    // *** Close the CONTROL RPT. DDNAME=RPT98.
    // ***
    local.Report.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.Report.Status, "OK"))
    {
      // ***
      // *** Write a line to the ERROR RPT.
      // ***
      local.Report.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered, while closing the Control Report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***
    // *** Close the ERROR RPT. DDNAME=RPT99.
    // ***
    local.Report.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.Report.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
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

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Report.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.Report.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Report.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Report.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Report.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Report.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Report.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Report.Status = useExport.EabFileHandling.Status;
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

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void DeleteInfrastructure()
  {
    Update("DeleteInfrastructure#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#6",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#7",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#8",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#9",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.ExistingInfrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetDate(
          command, "date1", local.CurrentDateWorkArea.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", local.BeginRange.Date.GetValueOrDefault());
        db.SetDate(command, "date3", local.EndRange.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "caseNumber", local.Previous.CaseNumber ?? "");
        db.SetInt32(command, "eventId", local.Previous.EventId);
        db.SetString(command, "reasonCode", local.Previous.ReasonCode);
        db.SetDateTime(
          command, "createdTimestamp",
          local.Previous.CreatedTimestamp.GetValueOrDefault());
        db.SetNullableInt32(
          command, "caseUnitNum",
          local.Previous.CaseUnitNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.ProcessStatus = db.GetString(reader, 1);
        entities.ExistingInfrastructure.EventId = db.GetInt32(reader, 2);
        entities.ExistingInfrastructure.ReasonCode = db.GetString(reader, 3);
        entities.ExistingInfrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 4);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingInfrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 7);
        entities.ExistingInfrastructure.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ExistingInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.ExistingInfrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail()
  {
    entities.ExistingNarrativeDetail.Populated = false;

    return ReadEach("ReadNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingNarrativeDetail.InfrastructureId =
          db.GetInt32(reader, 0);
        entities.ExistingNarrativeDetail.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ExistingNarrativeDetail.LineNumber = db.GetInt32(reader, 2);
        entities.ExistingNarrativeDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentMonitoredDocument()
  {
    entities.ExistingOutgoingDocument.Populated = false;
    entities.ExistingMonitoredDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentMonitoredDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingOutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.ExistingOutgoingDocument.InfId = db.GetInt32(reader, 1);
        entities.ExistingMonitoredDocument.InfId = db.GetInt32(reader, 1);
        entities.ExistingMonitoredDocument.RequiredResponseDate =
          db.GetDate(reader, 2);
        entities.ExistingOutgoingDocument.Populated = true;
        entities.ExistingMonitoredDocument.Populated = true;

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
    /// A value of FirstNonZero.
    /// </summary>
    [JsonPropertyName("firstNonZero")]
    public Common FirstNonZero
    {
      get => firstNonZero ??= new();
      set => firstNonZero = value;
    }

    /// <summary>
    /// A value of LastZero.
    /// </summary>
    [JsonPropertyName("lastZero")]
    public Common LastZero
    {
      get => lastZero ??= new();
      set => lastZero = value;
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
    /// A value of Work1.
    /// </summary>
    [JsonPropertyName("work1")]
    public DateWorkArea Work1
    {
      get => work1 ??= new();
      set => work1 = value;
    }

    /// <summary>
    /// A value of Work2.
    /// </summary>
    [JsonPropertyName("work2")]
    public DateWorkArea Work2
    {
      get => work2 ??= new();
      set => work2 = value;
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
    /// A value of FirstTimeThru.
    /// </summary>
    [JsonPropertyName("firstTimeThru")]
    public Common FirstTimeThru
    {
      get => firstTimeThru ??= new();
      set => firstTimeThru = value;
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
    /// A value of CommitCtr.
    /// </summary>
    [JsonPropertyName("commitCtr")]
    public Common CommitCtr
    {
      get => commitCtr ??= new();
      set => commitCtr = value;
    }

    /// <summary>
    /// A value of RepeatLoopCtr.
    /// </summary>
    [JsonPropertyName("repeatLoopCtr")]
    public Common RepeatLoopCtr
    {
      get => repeatLoopCtr ??= new();
      set => repeatLoopCtr = value;
    }

    /// <summary>
    /// A value of RowsReadCtr.
    /// </summary>
    [JsonPropertyName("rowsReadCtr")]
    public Common RowsReadCtr
    {
      get => rowsReadCtr ??= new();
      set => rowsReadCtr = value;
    }

    /// <summary>
    /// A value of RowsDeletedCtr.
    /// </summary>
    [JsonPropertyName("rowsDeletedCtr")]
    public Common RowsDeletedCtr
    {
      get => rowsDeletedCtr ??= new();
      set => rowsDeletedCtr = value;
    }

    /// <summary>
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    /// <summary>
    /// A value of CurrentInfrastructure1.
    /// </summary>
    [JsonPropertyName("currentInfrastructure1")]
    public DateWorkArea CurrentInfrastructure1
    {
      get => currentInfrastructure1 ??= new();
      set => currentInfrastructure1 = value;
    }

    /// <summary>
    /// A value of PreviousInfrastructure.
    /// </summary>
    [JsonPropertyName("previousInfrastructure")]
    public DateWorkArea PreviousInfrastructure
    {
      get => previousInfrastructure ??= new();
      set => previousInfrastructure = value;
    }

    /// <summary>
    /// A value of Init1.
    /// </summary>
    [JsonPropertyName("init1")]
    public DateWorkArea Init1
    {
      get => init1 ??= new();
      set => init1 = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of BeginRange.
    /// </summary>
    [JsonPropertyName("beginRange")]
    public DateWorkArea BeginRange
    {
      get => beginRange ??= new();
      set => beginRange = value;
    }

    /// <summary>
    /// A value of EndRange.
    /// </summary>
    [JsonPropertyName("endRange")]
    public DateWorkArea EndRange
    {
      get => endRange ??= new();
      set => endRange = value;
    }

    /// <summary>
    /// A value of Report.
    /// </summary>
    [JsonPropertyName("report")]
    public EabFileHandling Report
    {
      get => report ??= new();
      set => report = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of CurrentInfrastructure2.
    /// </summary>
    [JsonPropertyName("currentInfrastructure2")]
    public Infrastructure CurrentInfrastructure2
    {
      get => currentInfrastructure2 ??= new();
      set => currentInfrastructure2 = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Infrastructure Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of Lowest.
    /// </summary>
    [JsonPropertyName("lowest")]
    public Infrastructure Lowest
    {
      get => lowest ??= new();
      set => lowest = value;
    }

    /// <summary>
    /// A value of LowestCaseUnitNumber.
    /// </summary>
    [JsonPropertyName("lowestCaseUnitNumber")]
    public Common LowestCaseUnitNumber
    {
      get => lowestCaseUnitNumber ??= new();
      set => lowestCaseUnitNumber = value;
    }

    private Common firstNonZero;
    private Common lastZero;
    private Common position;
    private DateWorkArea work1;
    private DateWorkArea work2;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common firstTimeThru;
    private Common repeat;
    private Common commitCtr;
    private Common repeatLoopCtr;
    private Common rowsReadCtr;
    private Common rowsDeletedCtr;
    private DateWorkArea currentDateWorkArea;
    private DateWorkArea currentInfrastructure1;
    private DateWorkArea previousInfrastructure;
    private DateWorkArea init1;
    private DateWorkArea max;
    private DateWorkArea beginRange;
    private DateWorkArea endRange;
    private EabFileHandling report;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private Infrastructure currentInfrastructure2;
    private Infrastructure previous;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private Infrastructure lowest;
    private Common lowestCaseUnitNumber;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingNarrativeDetail.
    /// </summary>
    [JsonPropertyName("existingNarrativeDetail")]
    public NarrativeDetail ExistingNarrativeDetail
    {
      get => existingNarrativeDetail ??= new();
      set => existingNarrativeDetail = value;
    }

    /// <summary>
    /// A value of ExistingOutgoingDocument.
    /// </summary>
    [JsonPropertyName("existingOutgoingDocument")]
    public OutgoingDocument ExistingOutgoingDocument
    {
      get => existingOutgoingDocument ??= new();
      set => existingOutgoingDocument = value;
    }

    /// <summary>
    /// A value of ExistingMonitoredDocument.
    /// </summary>
    [JsonPropertyName("existingMonitoredDocument")]
    public MonitoredDocument ExistingMonitoredDocument
    {
      get => existingMonitoredDocument ??= new();
      set => existingMonitoredDocument = value;
    }

    /// <summary>
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
    }

    private NarrativeDetail existingNarrativeDetail;
    private OutgoingDocument existingOutgoingDocument;
    private MonitoredDocument existingMonitoredDocument;
    private Infrastructure existingInfrastructure;
  }
#endregion
}
