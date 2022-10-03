// Program: LE_B550_CERTIFY_CRED, ID: 372737128, model: 746.
// Short name: SWEL550B
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
/// A program: LE_B550_CERTIFY_CRED.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB550CertifyCred: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B550_CERTIFY_CRED program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB550CertifyCred(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB550CertifyCred.
  /// </summary>
  public LeB550CertifyCred(IContext context, Import import, Export export):
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
    // ??/??/??  H.Hooks			Initial Code
    // 12/03/97  govind			Made changes to exception processing for unknown
    // 					errors to be inline with other certification
    // 					procedures.  Removed the usage of persistent views
    // 					of program run and related entities.
    // 02/04/99  PMcElderry			Removed all error logic dealing with 
    // Program_Error,
    // 					Program_Control_Totals, and Program_Run entity types.
    // 					Replaced with cabs Cab_Error_Report and
    // 					Cab_Control_Report.
    // 					Added views on recursive flow to prevent reopening of
    // 					ERROR and CONTROL files.
    // 04/15/99  PMcElderry			Logic to ensure valid address exists.
    // 08/16/99  PMcElderry			Logic to close adabas
    // 09/06/99  PMcElderry			Removed SSN validation
    // 03/15/00  PMcElderry	WR# 000162	Family violence logic.  A seperate read 
    // on the
    // 					subtype is done for performance
    // 06/23/00  GVandy	PR # 98995	Removed involuted transfer logic.
    // 03/06/07  Raj S		WR 303198 	Modified to apply the new business rule (
    // Initial
    // 					certification need not have a verified address)
    // 					while selecting the person address.  The address
    // 					type can be 'D'omestic/'F'oreign.
    // 08/04/17  GVandy	CQ56369		Changes for Metro2 file/record layouts.
    // 					Restructured code to cleanup formatting and
    // 					simplify logic.  Changes to certification criteria.
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
    // --  Open the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

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
      UseCabErrorReport2();

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
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Determine if we're restarting.
    // -------------------------------------------------------------------------------------
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -------------------------------------------------------------------------------------
      //  Checkpoint Info...
      // 	Position  Description
      // 	--------  
      // ---------------------------------------------------------
      // 	001-010   Last NCP Number Processed
      //         011-011   blank
      //         012-020   Total Number of Obligors Processed
      //         021-021   blank
      //         022-030   Total Number of CRED Records Created
      //         031-031   blank
      //         032-040   Total Number of Errors Written to Error Report
      //         041-041   blank
      //         042-051   Processing Date (The date sent to CRA value must be 
      // the same for
      //                                    
      // all records even if the job is
      // restarted on a
      //                                    
      // later processing date)
      //         052-052   blank
      //         053-061   Total Number of CRED Records to be Sent to Credit 
      // Agencies
      // -------------------------------------------------------------------------------------
      local.CheckpointRestartKey.Number =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo);
      local.TotalObligorsRead.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 12, 9));
      local.TotalCredRecordsCreated.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 22, 9));
      local.TotalErrorsWritten.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 32, 9));
      local.ProgramProcessingInfo.ProcessDate =
        StringToDate(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 42, 10));
      local.TotalCredToSendToCra.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 53, 9));

      // -- Log restart information to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 9; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = "Program is restarting at:";

            break;
          case 2:
            local.EabReportSend.RptDetail =
              "            CSP Number:                     " + local
              .CheckpointRestartKey.Number;

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "            Obligors Previously Processed:  " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 12, 9);

            break;
          case 4:
            local.EabReportSend.RptDetail =
              "            CRED Recs Previously Created:   " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 22, 9);

            break;
          case 5:
            local.EabReportSend.RptDetail =
              "            Errors Previously Written:      " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 32, 9);

            break;
          case 6:
            local.EabReportSend.RptDetail =
              "            Credit Agency Recs Previously Created:   " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 22, 9);

            break;
          case 7:
            local.EabReportSend.RptDetail =
              "            Processing Date:                " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 42, 10);

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
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }
    else
    {
      local.CheckpointRestartKey.Number = "";
    }

    // -------------------------------------------------------------------------------------
    // -- Extract parameter values.
    // --
    // --     Positions  Value
    // --     ---------  
    // -------------------------------------------------------------
    // --        1-3     Number of days between obligation creation and 
    // certification
    // --        4-4     blank
    // --        5-5     Information Logging Flag
    // --        6-6     blank
    // --        7-7     Continue on Error Flag (used for testing in DEV & ACC)
    // -------------------------------------------------------------------------------------
    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 3)))
    {
      local.NumDaysSinceObCreated.Count = 0;
    }
    else
    {
      local.NumDaysSinceObCreated.Count =
        (int)StringToNumber(TrimEnd(
          Substring(local.ProgramProcessingInfo.ParameterList, 1, 3)));
    }

    local.ObligationCreatedCutoff.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate, -
      local.NumDaysSinceObCreated.Count);
    local.ObligationCreatedCutoff.Timestamp =
      Timestamp(
        NumberToString(Year(local.ObligationCreatedCutoff.Date), 12, 4) + "-" +
      NumberToString(Month(local.ObligationCreatedCutoff.Date), 14, 2) + "-" + NumberToString
      (Day(local.ObligationCreatedCutoff.Date), 14, 2));
    local.InformationLogging.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 5, 1);

    switch(AsChar(local.InformationLogging.Flag))
    {
      case 'Y':
        break;
      case 'N':
        break;
      default:
        local.InformationLogging.Flag = "N";

        break;
    }

    if (CharAt(local.ProgramProcessingInfo.ParameterList, 7) == 'Y')
    {
      local.ContinueOnError.Flag = "Y";
    }
    else
    {
      local.ContinueOnError.Flag = "N";
    }

    // -------------------------------------------------------------------------------------
    // -- Determine if Family Violence Exclusions are enabled.
    // -------------------------------------------------------------------------------------
    if (ReadCodeValue())
    {
      local.ExcludeFvi.Flag = Substring(entities.CodeValue.Description, 1, 1);
    }
    else
    {
      local.ExcludeFvi.Flag = "N";
    }

    // -------------------------------------------------------------------------------------
    // -- Log parameter values to the control report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 6; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Number of Days since Obligation was created: " + NumberToString
            (local.NumDaysSinceObCreated.Count, 13, 3);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Obligation Creation cutoff date: " + NumberToString
            (DateToInt(local.ObligationCreatedCutoff.Date), 8, 8);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Exclude NCPs due to Family Violence: " + local.ExcludeFvi.Flag;

          break;
        case 4:
          local.EabReportSend.RptDetail = "Information logging: " + local
            .InformationLogging.Flag;

          break;
        case 5:
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
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.ProgramCheckpointRestart.CheckpointCount = 0;

    // -------------------------------------------------------------------------------------
    // -- Read each obligor and determine if they should be certified for credit
    // reporting.
    // -------------------------------------------------------------------------------------
    foreach(var item in ReadCsePersonObligor())
    {
      ++local.ReadsSinceCommit.Count;
      ++local.TotalObligorsRead.Count;

      // --Call cab to determine if the NCP meets the credit reporting criteria 
      // and what, if
      //   anything, needs to be sent to the credit agencies.
      UseLeCreateCredCertification();

      if (AsChar(local.InformationLogging.Flag) == 'Y')
      {
        if (IsEmpty(local.EabReportSend.RptDetail))
        {
          local.EabReportSend.RptDetail = "NCP " + entities.CsePerson.Number + "          -  No logging info returned.";
            
        }

        // -- Log info returned from certification cab to the control report.
        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(04) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // --Non Fatal errors are logged inside the certification cabs.
      //   Any exit state returned to the Pstep will result in an Abend.
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Log error to the error report
        UseEabExtractExitStateMessage();

        for(local.Common.Count = 1; local.Common.Count <= 1; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              // -- WRITE message to Error Report
              local.EabReportSend.RptDetail =
                TrimEnd("CSE Person " + entities.CsePerson.Number) + " Exit state is " +
                local.ExitStateWorkArea.Message;

              break;
            case 2:
              // -- WRITE Error Report spacing
              local.EabReportSend.RptDetail = "";

              break;
            default:
              break;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

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
          local.ErrorWritten.Flag = "Y";

          goto Test1;
        }

        // -- Set exit state to Abend.
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

Test1:

      // --Increment Counters.
      if (Equal(local.SendToCra.RptDetail, "Y"))
      {
        ++local.TotalCredToSendToCra.Count;
      }

      if (AsChar(local.CredRecordCreated.Flag) == 'Y')
      {
        ++local.TotalCredRecordsCreated.Count;
      }

      if (AsChar(local.ErrorWritten.Flag) == 'Y')
      {
        ++local.TotalErrorsWritten.Count;
      }

      // -- Commit processing.
      if (local.ReadsSinceCommit.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // -- Checkpoint.
        // -------------------------------------------------------------------------------------
        //  Checkpoint Info...
        // 	Position  Description
        // 	--------  
        // ---------------------------------------------------------
        // 	001-010   Last NCP Number Processed
        //         011-011   blank
        //         012-020   Total Number of Obligors Processed
        //         021-021   blank
        //         022-030   Total Number of CRED Records Created
        //         031-031   blank
        //         032-040   Total Number of Errors Written to Error Report
        //         041-041   blank
        //         042-051   Processing Date (The date sent to CRA value must be
        // the same for
        //                                    
        // all records even if the job is
        // restarted on a
        //                                    
        // later processing date)
        //         052-052   blank
        //         053-061   Total Number of CRED Records to be Sent to Credit 
        // Agencies
        // -------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = entities.CsePerson.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " " + NumberToString
          (local.TotalObligorsRead.Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " " + NumberToString
          (local.TotalCredRecordsCreated.Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " " + NumberToString
          (local.TotalErrorsWritten.Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " " + NumberToString
          (Year(local.ProgramProcessingInfo.ProcessDate), 12, 4) + "-" + NumberToString
          (Month(local.ProgramProcessingInfo.ProcessDate), 14, 2) + "-" + NumberToString
          (Day(local.ProgramProcessingInfo.ProcessDate), 14, 2);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " " + NumberToString
          (local.TotalCredToSendToCra.Count, 7, 9);
        UseUpdatePgmCheckpointRestart2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // -- Commit
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          break;
        }

        local.ReadsSinceCommit.Count = 0;
      }
    }

ReadEach:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -------------------------------------------------------------------------------------
      // --  Store the processing date on the SWELB560 MPPI parameter.
      // --  Records with this date sent to CRA will be processed in SWELB560
      // -------------------------------------------------------------------------------------
      if (ReadProgramProcessingInfo1())
      {
        local.B560.ParameterList =
          NumberToString(Year(local.ProgramProcessingInfo.ProcessDate), 12, 4) +
          "-" + NumberToString
          (Month(local.ProgramProcessingInfo.ProcessDate), 14, 2) + "-" + NumberToString
          (Day(local.ProgramProcessingInfo.ProcessDate), 14, 2) + " " + Substring
          (entities.ProgramProcessingInfo.ParameterList, 12, 229);

        try
        {
          UpdateProgramProcessingInfo();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "PROGRAM_PROCESSING_INFO_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "PROGRAM_PROCESSING_INFO_PV";

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
        ExitState = "PROGRAM_PROCESSING_INFO_NF";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Log error to the error report
        UseEabExtractExitStateMessage();

        for(local.Common.Count = 1; local.Common.Count <= 2; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              // -- WRITE message to Error Report
              local.EabReportSend.RptDetail =
                TrimEnd("When updating MPPI record for SWELB560." + "") + " Exit state is " +
                local.ExitStateWorkArea.Message;

              break;
            case 2:
              // -- WRITE Error Report spacing
              local.EabReportSend.RptDetail = "";

              break;
            default:
              break;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // -- Set Abort exit state and escape...
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            goto Test2;
          }
        }

        // -- Set exit state to Abend.
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        goto Test2;
      }

      // -------------------------------------------------------------------------------------
      // --  Write Totals to the Control Report.
      // -------------------------------------------------------------------------------------
      for(local.Common.Count = -1; local.Common.Count <= 6; ++
        local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            // -- Total obligors processed
            local.EabReportSend.RptDetail =
              "Total Number of Obligors Processed = " + NumberToString
              (local.TotalObligorsRead.Count, 15);

            break;
          case 2:
            // -- Total CRED records created
            local.EabReportSend.RptDetail =
              "Total Number of CRED Records Created = " + NumberToString
              (local.TotalCredRecordsCreated.Count, 15);

            break;
          case 3:
            // -- Total errors written to error report
            local.EabReportSend.RptDetail =
              "Total Number of Errors Written = " + NumberToString
              (local.TotalErrorsWritten.Count, 15);

            break;
          case 4:
            // -- Total CRED records created that will be sent to the credit 
            // agencies
            local.EabReportSend.RptDetail =
              "Total Number of Records to be Sent to Credit Agencies = " + NumberToString
              (local.TotalCredToSendToCra.Count, 15);

            break;
          default:
            // -- Blank line
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
            "(03) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }
      }

      // ------------------------------------------------------------------------------
      // -- Take a final checkpoint.
      // ------------------------------------------------------------------------------
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.RestartInd = "N";
      UseUpdatePgmCheckpointRestart1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error taking final checkpoint..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

Test2:

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
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseLeCreateCredCertification()
  {
    var useImport = new LeCreateCredCertification.Import();
    var useExport = new LeCreateCredCertification.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.ObligationCreatedCutoff.Timestamp =
      local.ObligationCreatedCutoff.Timestamp;
    useImport.InformationLogging.Flag = local.InformationLogging.Flag;
    useImport.ExcludeFvi.Flag = local.ExcludeFvi.Flag;

    Call(LeCreateCredCertification.Execute, useImport, useExport);

    local.SendToCra.RptDetail = useExport.SendToCra.RptDetail;
    local.EabReportSend.RptDetail = useExport.InfoLogging.RptDetail;
    local.CredRecordCreated.Flag = useExport.CredRecordCreated.Flag;
    local.ErrorWritten.Flag = useExport.ErrorWritten.Flag;
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

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      null,
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.Description = db.GetString(reader, 3);
        entities.CodeValue.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonObligor()
  {
    entities.Obligor.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePersonObligor",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CheckpointRestartKey.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.Obligor.Type1 = db.GetString(reader, 2);
        entities.Obligor.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);

        return true;
      });
  }

  private bool ReadProgramProcessingInfo1()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      null,
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 2);
        entities.ProgramProcessingInfo.Populated = true;
      });
  }

  private void UpdateProgramProcessingInfo()
  {
    var parameterList = local.B560.ParameterList ?? "";

    entities.ProgramProcessingInfo.Populated = false;
    Update("UpdateProgramProcessingInfo",
      (db, command) =>
      {
        db.SetNullableString(command, "parameterList", parameterList);
        db.SetString(command, "name", entities.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ProgramProcessingInfo.ParameterList = parameterList;
    entities.ProgramProcessingInfo.Populated = true;
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
    /// A value of TotalCredToSendToCra.
    /// </summary>
    [JsonPropertyName("totalCredToSendToCra")]
    public Common TotalCredToSendToCra
    {
      get => totalCredToSendToCra ??= new();
      set => totalCredToSendToCra = value;
    }

    /// <summary>
    /// A value of SendToCra.
    /// </summary>
    [JsonPropertyName("sendToCra")]
    public EabReportSend SendToCra
    {
      get => sendToCra ??= new();
      set => sendToCra = value;
    }

    /// <summary>
    /// A value of B560.
    /// </summary>
    [JsonPropertyName("b560")]
    public ProgramProcessingInfo B560
    {
      get => b560 ??= new();
      set => b560 = value;
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
    /// A value of TotalObligorsRead.
    /// </summary>
    [JsonPropertyName("totalObligorsRead")]
    public Common TotalObligorsRead
    {
      get => totalObligorsRead ??= new();
      set => totalObligorsRead = value;
    }

    /// <summary>
    /// A value of TotalCredRecordsCreated.
    /// </summary>
    [JsonPropertyName("totalCredRecordsCreated")]
    public Common TotalCredRecordsCreated
    {
      get => totalCredRecordsCreated ??= new();
      set => totalCredRecordsCreated = value;
    }

    /// <summary>
    /// A value of TotalErrorsWritten.
    /// </summary>
    [JsonPropertyName("totalErrorsWritten")]
    public Common TotalErrorsWritten
    {
      get => totalErrorsWritten ??= new();
      set => totalErrorsWritten = value;
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
    /// A value of NumDaysSinceObCreated.
    /// </summary>
    [JsonPropertyName("numDaysSinceObCreated")]
    public Common NumDaysSinceObCreated
    {
      get => numDaysSinceObCreated ??= new();
      set => numDaysSinceObCreated = value;
    }

    /// <summary>
    /// A value of ObligationCreatedCutoff.
    /// </summary>
    [JsonPropertyName("obligationCreatedCutoff")]
    public DateWorkArea ObligationCreatedCutoff
    {
      get => obligationCreatedCutoff ??= new();
      set => obligationCreatedCutoff = value;
    }

    /// <summary>
    /// A value of InformationLogging.
    /// </summary>
    [JsonPropertyName("informationLogging")]
    public Common InformationLogging
    {
      get => informationLogging ??= new();
      set => informationLogging = value;
    }

    /// <summary>
    /// A value of ExcludeFvi.
    /// </summary>
    [JsonPropertyName("excludeFvi")]
    public Common ExcludeFvi
    {
      get => excludeFvi ??= new();
      set => excludeFvi = value;
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
    /// A value of CredRecordCreated.
    /// </summary>
    [JsonPropertyName("credRecordCreated")]
    public Common CredRecordCreated
    {
      get => credRecordCreated ??= new();
      set => credRecordCreated = value;
    }

    /// <summary>
    /// A value of ErrorWritten.
    /// </summary>
    [JsonPropertyName("errorWritten")]
    public Common ErrorWritten
    {
      get => errorWritten ??= new();
      set => errorWritten = value;
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

    private Common totalCredToSendToCra;
    private EabReportSend sendToCra;
    private ProgramProcessingInfo b560;
    private Common continueOnError;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private CsePerson checkpointRestartKey;
    private Common totalObligorsRead;
    private Common totalCredRecordsCreated;
    private Common totalErrorsWritten;
    private Common common;
    private Common numDaysSinceObCreated;
    private DateWorkArea obligationCreatedCutoff;
    private Common informationLogging;
    private Common excludeFvi;
    private Common readsSinceCommit;
    private Common credRecordCreated;
    private Common errorWritten;
    private External passArea;
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

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private CodeValue codeValue;
    private Code code;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
  }
#endregion
}
