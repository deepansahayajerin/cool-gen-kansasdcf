// Program: LE_B560_WRITE_CRED_TO_TAPE, ID: 372739485, model: 746.
// Short name: SWEL560B
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
/// A program: LE_B560_WRITE_CRED_TO_TAPE.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB560WriteCredToTape: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B560_WRITE_CRED_TO_TAPE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB560WriteCredToTape(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB560WriteCredToTape.
  /// </summary>
  public LeB560WriteCredToTape(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------
    //                          M A I N T E N A N C E      L O G
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // ??/??/??  H.Hooks (MTW)			Initial Code
    // ??/??/??  govind	102997		Fixed to create program error for CSE Person
    // 					Account NF error. Currently it is not doing
    // 					rollback and not flowing to itself
    // 					recursively. This is okay as it exists now
    // 					since the only update that takes place in
    // 					that action block is UPDATE Credit Reporting
    // 					Action when the whole operation is
    // 					successful. This might have to be revisited
    // 					later when the situation changes.
    // ??/??/??  govind	120497		Removed the usage of persistent views on
    // 					Program Run.
    // 02/08/99  PMcElderry			Removed error logic dealing w/Program_Run,
    // 					Program_Error, Program_Control_Totals and
    // 					replaced with CAB_ERROR_REPORT and
    // 					CAB_CONTROL_REPORT action blocks.
    // 08/16/99  PMcElderry			Logic to close adabas
    // 04/13/04  CMJ 		pr 204813 	Correct cred
    // 08/04/17  GVandy	CQ56369		Changes for Metro2 file/record layouts.
    // 					Restructured code to cleanup formatting and
    // 					simplify logic.
    // ---------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -------------------------------------------------------------------------------------
    // --  Read the PPI Record.
    // -------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Read for checkpoint/restart info.
    // -------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // --  SWELB550 stored the processing date it used in the MPPI parameter for
    // SWELB560.
    // --  We need to use that value as our processing date.  All the records 
    // that we will
    // --  process will have the "date sent to CRA" set to this value.
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // -- Extract parameter values.
    // --
    // --     Positions  Value
    // --     ---------  
    // -------------------------------------------------------------
    // --        1-10    Processing date used by SWELB550
    // --       11-11    blank
    // --       12-12    Continue on Error Flag (used for testing in DEV & ACC)
    // -------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.ProcessDate =
      StringToDate(Substring(local.ProgramProcessingInfo.ParameterList, 1, 10));
      

    if (CharAt(local.ProgramProcessingInfo.ParameterList, 12) == 'Y')
    {
      local.ContinueOnError.Flag = "Y";
    }
    else
    {
      local.ContinueOnError.Flag = "N";
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Error Report.
    // -------------------------------------------------------------------------------------
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
      // -- This could have resulted from not finding the PPI record.
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Initialization Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the control report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Determine if we're restarting.
    // -------------------------------------------------------------------------------------
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -- Extract restart values...
      // -------------------------------------------------------------------------------------
      //  Checkpoint Info...
      // 	Position  Description
      // 	--------  
      // ---------------------------------------------------------
      // 	001-010   Last NCP Number Processed
      // 	011-019   Base Record Count
      // 	020-028   Block Count
      // 	029-037   Status DA Count
      // 	038-046   Status 11 Count
      // 	047-055   Status 13 Count
      // 	056-064   Status 64 Count
      // 	065-073   Status 71 Count
      // 	074-082   Status 93 Count
      // 	083-091   SSN All Segments Count
      // 	092-100   SSN Base Segments Count
      // 	101-109   DOB All Segments Count
      // 	110-118   DOB Base Segment Count
      // -------------------------------------------------------------------------------------
      local.CheckpointRestartKey.Number =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo);
      local.Metro2.BaseRecordCount =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 11, 9));
      local.Metro2.BlockCount =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 20, 9));
      local.Metro2.StatusDaCount =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 29, 9));
      local.Metro2.Status11Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 38, 9));
      local.Metro2.Status13Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 47, 9));
      local.Metro2.Status64Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 56, 9));
      local.Metro2.Status71Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 65, 9));
      local.Metro2.Status93Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 74, 9));
      local.Metro2.SsnAllSegmentsCount =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 83, 9));
      local.Metro2.SsnBaseSegmentCount =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 92, 9));
      local.Metro2.DobAllSegmentsCount =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 101, 9));
      local.Metro2.DobBaseSegmentCount =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 110, 9));

      // -- Log restart information to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 16; ++
        local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = "Program is restarting at:";

            break;
          case 2:
            local.EabReportSend.RptDetail =
              "          CSP Number................" + local
              .CheckpointRestartKey.Number;

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "          Base Record Count........." + NumberToString
              (local.Metro2.BaseRecordCount, 7, 9);

            break;
          case 4:
            local.EabReportSend.RptDetail =
              "          Block Count..............." + NumberToString
              (local.Metro2.BlockCount, 7, 9);

            break;
          case 5:
            local.EabReportSend.RptDetail =
              "         Status DA Count............" + NumberToString
              (local.Metro2.StatusDaCount, 7, 9);

            break;
          case 6:
            local.EabReportSend.RptDetail =
              "         Status 11 Count............" + NumberToString
              (local.Metro2.Status11Count, 7, 9);

            break;
          case 7:
            local.EabReportSend.RptDetail =
              "         Status 13 Count............" + NumberToString
              (local.Metro2.Status13Count, 7, 9);

            break;
          case 8:
            local.EabReportSend.RptDetail =
              "         Status 64 Count............" + NumberToString
              (local.Metro2.Status64Count, 7, 9);

            break;
          case 9:
            local.EabReportSend.RptDetail =
              "         Status 71 Count............" + NumberToString
              (local.Metro2.Status71Count, 7, 9);

            break;
          case 10:
            local.EabReportSend.RptDetail =
              "         Status 93 Count............" + NumberToString
              (local.Metro2.Status93Count, 7, 9);

            break;
          case 11:
            local.EabReportSend.RptDetail =
              "         SSN All Segments Count....." + NumberToString
              (local.Metro2.SsnAllSegmentsCount, 7, 9);

            break;
          case 12:
            local.EabReportSend.RptDetail =
              "         SSN Base Segments Count...." + NumberToString
              (local.Metro2.SsnBaseSegmentCount, 7, 9);

            break;
          case 13:
            local.EabReportSend.RptDetail =
              "         DOB All Segments Count....." + NumberToString
              (local.Metro2.DobAllSegmentsCount, 7, 9);

            break;
          case 14:
            local.EabReportSend.RptDetail =
              "         DOB Base Segments Count...." + NumberToString
              (local.Metro2.DobBaseSegmentCount, 7, 9);

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error Writing Restart Info to Control Report...  Returned Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // -------------------------------------------------------------------------------------
      // EXTEND the output credit reporting file.
      // If this is a restart, then the external output file must be appended 
      // to.  Hence,
      // we 'EXTEND' the file.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "EXTEND";
      UseExtWriteCredToTape1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error on EXTEND of Credit File...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }
    else
    {
      // -- This is a normal run NOT a Restart.
      local.CheckpointRestartKey.Number = "";

      // -- Initialize counters.
      local.Metro2.BaseRecordCount = 0;
      local.Metro2.BlockCount = 0;
      local.Metro2.DobAllSegmentsCount = 0;
      local.Metro2.DobBaseSegmentCount = 0;
      local.Metro2.SsnAllSegmentsCount = 0;
      local.Metro2.SsnBaseSegmentCount = 0;
      local.Metro2.Status11Count = 0;
      local.Metro2.Status13Count = 0;
      local.Metro2.Status64Count = 0;
      local.Metro2.Status71Count = 0;
      local.Metro2.Status93Count = 0;
      local.Metro2.StatusDaCount = 0;

      // -------------------------------------------------------------------------------------
      // Open the output credit reporting file
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseExtWriteCredToTape1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error on OPEN of Credit File...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -------------------------------------------------------------------------------------
      // Write Header record to the output credit reporting file
      // -------------------------------------------------------------------------------------
      local.Metro2CraHeaderRecord.RecordDescriptorWord = "0426";
      local.Metro2CraHeaderRecord.RecordIdentifier = "HEADER";
      local.Metro2CraHeaderRecord.CycleIdentifier = "";
      local.Metro2CraHeaderRecord.InnovisProgramIdentifier = "";
      local.Metro2CraHeaderRecord.EquifaxProgramIdentifier = "3482688";
      local.Metro2CraHeaderRecord.ExperianProgramIdentifier = "D9545";
      local.Metro2CraHeaderRecord.TransunionProgramIdentifier = "00V2269";
      local.Metro2CraHeaderRecord.ActivityDate =
        NumberToString(Month(local.ProgramProcessingInfo.ProcessDate), 14, 2) +
        NumberToString(Day(local.ProgramProcessingInfo.ProcessDate), 14, 2) + NumberToString
        (Year(local.ProgramProcessingInfo.ProcessDate), 12, 4);
      local.Metro2CraHeaderRecord.DateCreated =
        NumberToString(Month(local.ProgramProcessingInfo.ProcessDate), 14, 2) +
        NumberToString(Day(local.ProgramProcessingInfo.ProcessDate), 14, 2) + NumberToString
        (Year(local.ProgramProcessingInfo.ProcessDate), 12, 4);
      local.Metro2CraHeaderRecord.ProgramDate = "";
      local.Metro2CraHeaderRecord.ProgramRevisionDate = "";
      local.Metro2CraHeaderRecord.ReporterName =
        "KANSAS CHILD SUPPORT SERVICES";
      local.Metro2CraHeaderRecord.ReporterAddress =
        "PO BOX 497  TOPEKA, KS 66601";
      local.Metro2CraHeaderRecord.ReporterTelephoneNumber = "8887572445";
      local.Metro2CraHeaderRecord.SoftwareVendorName = "";
      local.Metro2CraHeaderRecord.SoftwareVersionNumber = "";
      local.Metro2CraHeaderRecord.MicrobiltprbcProgramIdentifier = "";
      local.Metro2CraHeaderRecord.Reserved = "";
      local.EabFileHandling.Action = "HEADER";
      UseExtWriteCredToTape2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing HEADER Record to Credit File...  Returned Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // -- Log parameter values to the control report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Using processing date provided by SWELB550 in MPPI parameter:  " +
            Substring(local.ProgramProcessingInfo.ParameterList, 1, 10);

          break;
        case 2:
          local.EabReportSend.RptDetail = "Continue on Error: " + local
            .ContinueOnError.Flag;

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(02) Error Writing Control Report...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Process Credit Reporting records.
    // -------------------------------------------------------------------------------------
    local.ReadsSinceCheckpoint.Count = 0;

    foreach(var item in ReadCsePersonCreditReportingAction())
    {
      ++local.ReadsSinceCommit.Count;
      UseLeCredWriteTapeRecord();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Log the exit state message to the error report.
        UseEabExtractExitStateMessage();

        for(local.Common.Count = 1; local.Common.Count <= 1; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              // -- WRITE message to Error Report
              local.EabReportSend.RptDetail =
                TrimEnd("Le_cred_write_tape_record error msg '" +
                TrimEnd(local.ExitStateWorkArea.Message)) + "' CSE Person " + entities
                .CsePerson.Number;

              break;
            case 2:
              // -- WRITE Error Report spacing
              local.EabReportSend.RptDetail = "";

              break;
            default:
              break;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // -- Set Abort exit state and escape...
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            goto ReadEach;
          }
        }

        // --If flag indicating to continue on after encountering an error is Y 
        // then continue.
        if (AsChar(local.ContinueOnError.Flag) == 'Y')
        {
          ExitState = "ACO_NN0000_ALL_OK";

          goto Test1;
        }

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

Test1:

      // -------------------------------------------------------------------------------------
      // Update checkpoint when checkpoint level reached
      // -------------------------------------------------------------------------------------
      if (local.ReadsSinceCheckpoint.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // -------------------------------------------------------------------------------------
        //  Checkpoint Info...
        // 	Position  Description
        // 	--------  
        // ---------------------------------------------------------
        // 	001-010   Last NCP Number Processed
        // 	011-019   Base Record Count
        // 	020-028   Block Count
        // 	029-037   Status DA Count
        // 	038-046   Status 11 Count
        // 	047-055   Status 13 Count
        // 	056-064   Status 64 Count
        // 	065-073   Status 71 Count
        // 	074-082   Status 93 Count
        // 	083-091   SSN All Segments Count
        // 	092-100   SSN Base Segments Count
        // 	101-109   DOB All Segments Count
        // 	110-118   DOB Base Segment Count
        // -------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInfo = entities.CsePerson.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Metro2.BaseRecordCount, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Metro2.BlockCount, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Metro2.StatusDaCount, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Metro2.Status11Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Metro2.Status13Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Metro2.Status64Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Metro2.Status71Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Metro2.Status93Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Metro2.SsnAllSegmentsCount, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Metro2.SsnBaseSegmentCount, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Metro2.DobAllSegmentsCount, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Metro2.DobBaseSegmentCount, 7, 9);
        local.ProgramCheckpointRestart.RestartInd = "Y";
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -- Extract the exit state message and write to the error report.
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error taking checkpoint..." + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.ReadsSinceCheckpoint.Count = 0;

        // -------------------------------------------------------------------------------------
        // COMMIT records to output credit reporting file.
        // -------------------------------------------------------------------------------------
        local.EabFileHandling.Action = "COMMIT";
        UseExtWriteCredToTape1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error on COMMIT of Credit File...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }
      }
    }

ReadEach:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -------------------------------------------------------------------------------------
      // Write Footer record to the output file
      // -------------------------------------------------------------------------------------
      local.Metro2CraTrailerRecord.RecordDesciptorWord = "0426";
      local.Metro2CraTrailerRecord.RecordIdentifier = "TRAILER";
      local.Metro2CraTrailerRecord.TotalBaseRecords =
        NumberToString(local.Metro2.BaseRecordCount, 7, 9);
      local.Metro2CraTrailerRecord.Reserved4 = "";
      local.Metro2CraTrailerRecord.TotalOfStatusCodeDf = "000000000";
      local.Metro2CraTrailerRecord.TotalJ1Segments = "000000000";
      local.Metro2CraTrailerRecord.TotalJ2Segments = "000000000";
      local.Metro2CraTrailerRecord.BlockCount =
        NumberToString(local.Metro2.BlockCount, 7, 9);
      local.Metro2CraTrailerRecord.TotalOfStatusCodeDa =
        NumberToString(local.Metro2.StatusDaCount, 7, 9);
      local.Metro2CraTrailerRecord.TotalOfStatusCode05 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode11 =
        NumberToString(local.Metro2.Status11Count, 7, 9);
      local.Metro2CraTrailerRecord.TotalOfStatusCode13 =
        NumberToString(local.Metro2.Status13Count, 7, 9);
      local.Metro2CraTrailerRecord.TotalOfStatusCode61 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode62 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode63 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode64 =
        NumberToString(local.Metro2.Status64Count, 7, 9);
      local.Metro2CraTrailerRecord.TotalOfStatusCode65 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode71 =
        NumberToString(local.Metro2.Status71Count, 7, 9);
      local.Metro2CraTrailerRecord.TotalOfStatusCode78 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode80 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode82 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode83 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode84 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode88 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode89 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode93 =
        NumberToString(local.Metro2.Status93Count, 7, 9);
      local.Metro2CraTrailerRecord.TotalOfStatusCode94 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode95 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode96 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfStatusCode97 = "000000000";
      local.Metro2CraTrailerRecord.TotalOfEcoaCodeZ = "000000000";
      local.Metro2CraTrailerRecord.TotalEmploymentSegments = "000000000";
      local.Metro2CraTrailerRecord.TotalOriginalCreditorSegments = "000000000";
      local.Metro2CraTrailerRecord.TotalPurchasedFromSoldToSeg = "000000000";
      local.Metro2CraTrailerRecord.TotalMorgageInformationSegmen = "000000000";
      local.Metro2CraTrailerRecord.TotalSpecicalizedPaymentInfo = "000000000";
      local.Metro2CraTrailerRecord.TotalChangeSegements = "000000000";
      local.Metro2CraTrailerRecord.TotalSsnsAllSegments =
        NumberToString(local.Metro2.SsnAllSegmentsCount, 7, 9);
      local.Metro2CraTrailerRecord.TotalSsnsBaseSegments =
        NumberToString(local.Metro2.SsnBaseSegmentCount, 7, 9);
      local.Metro2CraTrailerRecord.TotalSsnsJ1Segements = "000000000";
      local.Metro2CraTrailerRecord.TotalSsnsJ2Segments = "000000000";
      local.Metro2CraTrailerRecord.TotalBirthDatesAllSegments =
        NumberToString(local.Metro2.DobAllSegmentsCount, 7, 9);
      local.Metro2CraTrailerRecord.TotalBirthDatesBaseSegments =
        NumberToString(local.Metro2.DobBaseSegmentCount, 7, 9);
      local.Metro2CraTrailerRecord.TotalBirthDatesJ1Segments = "000000000";
      local.Metro2CraTrailerRecord.TotalBirthDatesJ2Segments = "000000000";
      local.Metro2CraTrailerRecord.TotalPhoneNumberAllSegments = "000000000";
      local.Metro2CraTrailerRecord.Reserved47 = "";
      local.EabFileHandling.Action = "TRAILER";
      UseExtWriteCredToTape3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing TRAILER Record to Credit File...  Returned Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        goto Test2;
      }

      // ------------------------------------------------------------------------------
      // COMMIT records to output credit reporting file.
      // ------------------------------------------------------------------------------
      local.EabFileHandling.Action = "COMMIT";
      UseExtWriteCredToTape1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error on final COMMIT of Credit File...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        goto Test2;
      }

      // ------------------------------------------------------------------------------
      // -- Take a final checkpoint.
      // ------------------------------------------------------------------------------
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.RestartInd = "N";
      UseUpdateCheckpointRstAndCommit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error taking final checkpoint..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        goto Test2;
      }

      // -------------------------------------------------------------------------------------
      // --  Write Totals to the Control Report.
      // -------------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 16; ++
        local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "Credit certification file complete.  Record counts below:";

            break;
          case 2:
            // -- Total record count is base record count + 2.  (one header and 
            // one footer)
            local.EabReportSend.RptDetail =
              "         Total Record Count........." + NumberToString
              ((long)local.Metro2.BaseRecordCount + 2, 7, 9);

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "         Base Record Count.........." + NumberToString
              (local.Metro2.BaseRecordCount, 7, 9);

            break;
          case 4:
            local.EabReportSend.RptDetail =
              "         Block Count................" + NumberToString
              (local.Metro2.BlockCount, 7, 9);

            break;
          case 5:
            local.EabReportSend.RptDetail =
              "         Status DA Count............" + NumberToString
              (local.Metro2.StatusDaCount, 7, 9);

            break;
          case 6:
            local.EabReportSend.RptDetail =
              "         Status 11 Count............" + NumberToString
              (local.Metro2.Status11Count, 7, 9);

            break;
          case 7:
            local.EabReportSend.RptDetail =
              "         Status 13 Count............" + NumberToString
              (local.Metro2.Status13Count, 7, 9);

            break;
          case 8:
            local.EabReportSend.RptDetail =
              "         Status 64 Count............" + NumberToString
              (local.Metro2.Status64Count, 7, 9);

            break;
          case 9:
            local.EabReportSend.RptDetail =
              "         Status 71 Count............" + NumberToString
              (local.Metro2.Status71Count, 7, 9);

            break;
          case 10:
            local.EabReportSend.RptDetail =
              "         Status 93 Count............" + NumberToString
              (local.Metro2.Status93Count, 7, 9);

            break;
          case 11:
            local.EabReportSend.RptDetail =
              "         SSN All Segments Count....." + NumberToString
              (local.Metro2.SsnAllSegmentsCount, 7, 9);

            break;
          case 12:
            local.EabReportSend.RptDetail =
              "         SSN Base Segments Count...." + NumberToString
              (local.Metro2.SsnBaseSegmentCount, 7, 9);

            break;
          case 13:
            local.EabReportSend.RptDetail =
              "         DOB All Segments Count....." + NumberToString
              (local.Metro2.DobAllSegmentsCount, 7, 9);

            break;
          case 14:
            local.EabReportSend.RptDetail =
              "         DOB Base Segments Count...." + NumberToString
              (local.Metro2.DobBaseSegmentCount, 7, 9);

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error Writing Count Totals to Control Report...  Returned Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto Test2;
        }
      }
    }

Test2:

    // ------------------------------------------------------------------------------
    // Close output file.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseExtWriteCredToTape1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Credit File...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_CLOSING_ERROR_RPT";
    }

    // -------------------------------------------------------------------------------------
    // --  Close Adabase.
    // -------------------------------------------------------------------------------------
    UseSiCloseAdabas();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtWriteCredToTape1()
  {
    var useImport = new ExtWriteCredToTape.Import();
    var useExport = new ExtWriteCredToTape.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(ExtWriteCredToTape.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtWriteCredToTape2()
  {
    var useImport = new ExtWriteCredToTape.Import();
    var useExport = new ExtWriteCredToTape.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.Metro2CraHeaderRecord.Assign(local.Metro2CraHeaderRecord);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(ExtWriteCredToTape.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtWriteCredToTape3()
  {
    var useImport = new ExtWriteCredToTape.Import();
    var useExport = new ExtWriteCredToTape.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.Metro2CraTrailerRecord.Assign(local.Metro2CraTrailerRecord);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(ExtWriteCredToTape.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeCredWriteTapeRecord()
  {
    var useImport = new LeCredWriteTapeRecord.Import();
    var useExport = new LeCredWriteTapeRecord.Export();

    useImport.CreditReportingAction.Assign(entities.CreditReportingAction);
    MoveCsePerson(entities.CsePerson, useImport.CsePerson);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Metro2.Assign(local.Metro2);

    Call(LeCredWriteTapeRecord.Execute, useImport, useExport);

    local.Metro2.Assign(useExport.Metro2);
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

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCsePersonCreditReportingAction()
  {
    entities.CreditReportingAction.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonCreditReportingAction",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "dateSentToCra",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CreditReportingAction.Identifier = db.GetInt32(reader, 2);
        entities.CreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 3);
        entities.CreditReportingAction.CraTransCode = db.GetString(reader, 4);
        entities.CreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 5);
        entities.CreditReportingAction.DateSentToCra =
          db.GetNullableDate(reader, 6);
        entities.CreditReportingAction.OriginalAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CreditReportingAction.CurrentAmount =
          db.GetNullableDecimal(reader, 8);
        entities.CreditReportingAction.HighestAmount =
          db.GetNullableDecimal(reader, 9);
        entities.CreditReportingAction.CpaType = db.GetString(reader, 10);
        entities.CreditReportingAction.CspNumber = db.GetString(reader, 11);
        entities.CreditReportingAction.AacType = db.GetString(reader, 12);
        entities.CreditReportingAction.AacTakenDate = db.GetDate(reader, 13);
        entities.CreditReportingAction.AacTanfCode = db.GetString(reader, 14);
        entities.CreditReportingAction.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CreditReportingAction>("CpaType",
          entities.CreditReportingAction.CpaType);
        CheckValid<CreditReportingAction>("AacType",
          entities.CreditReportingAction.AacType);

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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of CheckpointRestartKey.
    /// </summary>
    [JsonPropertyName("checkpointRestartKey")]
    public CsePerson CheckpointRestartKey
    {
      get => checkpointRestartKey ??= new();
      set => checkpointRestartKey = value;
    }

    /// <summary>
    /// A value of Metro2.
    /// </summary>
    [JsonPropertyName("metro2")]
    public Metro2 Metro2
    {
      get => metro2 ??= new();
      set => metro2 = value;
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
    /// A value of Metro2CraHeaderRecord.
    /// </summary>
    [JsonPropertyName("metro2CraHeaderRecord")]
    public Metro2CraHeaderRecord Metro2CraHeaderRecord
    {
      get => metro2CraHeaderRecord ??= new();
      set => metro2CraHeaderRecord = value;
    }

    /// <summary>
    /// A value of ReadsSinceCheckpoint.
    /// </summary>
    [JsonPropertyName("readsSinceCheckpoint")]
    public Common ReadsSinceCheckpoint
    {
      get => readsSinceCheckpoint ??= new();
      set => readsSinceCheckpoint = value;
    }

    /// <summary>
    /// A value of ReadsSinceCommit.
    /// </summary>
    [JsonPropertyName("readsSinceCommit")]
    public Common ReadsSinceCommit
    {
      get => readsSinceCommit ??= new();
      set => readsSinceCommit = value;
    }

    /// <summary>
    /// A value of Metro2CraTrailerRecord.
    /// </summary>
    [JsonPropertyName("metro2CraTrailerRecord")]
    public Metro2CraTrailerRecord Metro2CraTrailerRecord
    {
      get => metro2CraTrailerRecord ??= new();
      set => metro2CraTrailerRecord = value;
    }

    /// <summary>
    /// A value of ContinueOnError.
    /// </summary>
    [JsonPropertyName("continueOnError")]
    public Common ContinueOnError
    {
      get => continueOnError ??= new();
      set => continueOnError = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private CsePerson checkpointRestartKey;
    private Metro2 metro2;
    private Common common;
    private Metro2CraHeaderRecord metro2CraHeaderRecord;
    private Common readsSinceCheckpoint;
    private Common readsSinceCommit;
    private Metro2CraTrailerRecord metro2CraTrailerRecord;
    private Common continueOnError;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of CreditReporting.
    /// </summary>
    [JsonPropertyName("creditReporting")]
    public AdministrativeActCertification CreditReporting
    {
      get => creditReporting ??= new();
      set => creditReporting = value;
    }

    private CreditReportingAction creditReportingAction;
    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private AdministrativeActCertification creditReporting;
  }
#endregion
}
