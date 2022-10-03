// Program: OE_B427_FIDM_MTHD2_RESPONS_BATCH, ID: 374405114, model: 746.
// Short name: SWEE427B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B427_FIDM_MTHD2_RESPONS_BATCH.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB427FidmMthd2ResponsBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B427_FIDM_MTHD2_RESPONS_BATCH program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB427FidmMthd2ResponsBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB427FidmMthd2ResponsBatch.
  /// </summary>
  public OeB427FidmMthd2ResponsBatch(IContext context, Import import,
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
    // ***************************************************
    // Every initial development and change to that
    // development needs to be documented.
    // ***************************************************
    // *********************************************************************
    // Date      Developers Name         Request #  Description
    // --------  ----------------------  ---------  ------------------------
    // 05/17/00  George Vandy                       Initial Development
    // *********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodeOpen.FileInstruction = "OPEN";
    local.HardcodeRead.FileInstruction = "READ";
    local.HardcodePosition.FileInstruction = "POSITION";
    local.FirstTime.Flag = "Y";
    local.ErrorNumberOfLines.Count = 75;

    // ****************************
    // CHECK IF ADABAS IS AVAILABLE
    // ****************************
    UseCabReadAdabasPersonBatch();

    if (Equal(local.AbendData.AdabasResponseCd, "0148"))
    {
      ExitState = "ADABAS_UNAVAILABLE_RB";

      return;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    // ***************************************
    // GET THE RUN PARAMETERS FOR THIS PROGRAM
    // ***************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *****************************************************************
    // Get the DB2 commit frequency counts and find out if we are
    // in a restart situation.
    // *****************************************************************
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // *****************************************************************
      // Reset the checkpoint restart commit count to zero.
      // *****************************************************************
      local.ProgramCheckpointRestart.CheckpointCount = 0;

      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
      {
        // ************************************************************
        // The layout of the Restart Info field should be as follows:
        //     Attribute                         Format
        //     
        // --------------------------------
        // ------
        //     Institution TIN			N9
        //     Institution Name         		A40
        //     Match Run Date         		N6  (CCYYMM)
        // ************************************************************
        local.Restart.InstitutionTin =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 9);
        local.Restart.InstitutionName =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 10, 40);
        local.Restart.MatchRunDate =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 50, 6);
      }
    }
    else
    {
      return;
    }

    // *****************************************************************
    // Open the Input File.  If this program run is a restart, open
    // and reposition the file to the next record to be processed.
    // *****************************************************************
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // ********************************************
      // CALL EXTERNAL TO REPOSTITION THE DRIVER FILE
      // ********************************************
      local.PassArea.TextLine80 =
        local.ProgramCheckpointRestart.RestartInfo ?? Spaces(130);
      local.PassArea.FileInstruction = local.HardcodePosition.FileInstruction;

      // NOTE:  The external will reposition to the "T" record for the last 
      // institution successfully processed.  Thus, when we enter the repeat
      // loop and read the next record from the file it will be the "A" record
      // for the next institution to be processed.  This way the processing is
      // the same as when not re-starting.
      UseEabFidmMethod2MatchDrvr2();

      if (!Equal(local.PassArea.TextReturnCode, "00") && !
        Equal(local.PassArea.TextReturnCode, "10"))
      {
        ExitState = "FILE_OPEN_ERROR_AB";

        // BAD Open - SET ERROR FLAG
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = local.ExitStateWorkArea.Message;
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          ProgramError1 =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.ProgramError1) + " Abend Code = ";
          
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          ProgramError1 =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.ProgramError1) + " " + local
          .PassArea.TextReturnCode;
        local.AbendErrorFound.Flag = "Y";

        goto Test;
      }

      local.ProgramCheckpointRestart.RestartInd = "N";
    }
    else
    {
      // *************************************
      // CALL EXTERNAL TO OPEN THE DRIVER FILE
      // *************************************
      local.PassArea.FileInstruction = local.HardcodeOpen.FileInstruction;
      UseEabFidmMethod2MatchDrvr3();

      if (!Equal(local.PassArea.TextReturnCode, "00"))
      {
        ExitState = "FILE_OPEN_ERROR_AB";

        // BAD Open - SET ERROR FLAG
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = local.ExitStateWorkArea.Message;
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          ProgramError1 =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.ProgramError1) + " Abend Code = ";
          
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          ProgramError1 =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.ProgramError1) + " " + local
          .PassArea.TextReturnCode;
        local.AbendErrorFound.Flag = "Y";
      }
    }

Test:

    // ********************************
    // PROCESS DRIVER RECORDS UNTIL EOF
    // ********************************
    local.RecordsProcessed.Count = 0;
    local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
      SystemGeneratedIdentifier = 0;

    do
    {
      if (AsChar(local.AbendErrorFound.Flag) == 'Y')
      {
        break;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // *
      // Open successful - continue processing
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // *
      global.Command = "";

      // *************************************
      // CALL EXTERNAL TO READ THE DRIVER FILE
      // *************************************
      ExitState = "ACO_NN0000_ALL_OK";
      local.PassArea.FileInstruction = local.HardcodeRead.FileInstruction;
      UseEabFidmMethod2MatchDrvr1();

      // * Convert Return Code to Exit-State
      if (Equal(local.PassArea.TextReturnCode, "DT"))
      {
        // The file contains an invalid value in the Matched Account Payee Date 
        // of Birth field.
        local.PassArea.TextReturnCode = "00";
        ExitState = "OE_NE0000_INVALID_ACT_OWNER_DOB";

        // ******************************************************************
        // Increment the total number of FIDM records read.
        // ******************************************************************
        ++local.FidmRecordCount.Count;
      }
      else
      {
      }

      switch(TrimEnd(local.PassArea.TextReturnCode))
      {
        case "00":
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          switch(AsChar(local.RecordTypeReturned.Text1))
          {
            case 'A':
              // ******************************************************************
              // A Financial Institution Header Record was returned.
              // ******************************************************************
              // ******************************************************************
              // Save the financial institution info for use when we
              // process the account owner records provided by this
              // institution.
              // ******************************************************************
              local.SaveFinancialInstitution.Assign(
                local.FinancialInstitutionRec);
              local.SaveFirstBRecordTs.Flag = "Y";

              break;
            case 'B':
              // ******************************************************************
              // An Account Owner Record was returned.
              // ******************************************************************
              // ******************************************************************
              // Increment the total number of FIDM records read.
              // ******************************************************************
              ++local.FidmRecordCount.Count;

              // ******************************************************************
              // Validate the FIDM data.   If an error is returned, write the
              // cse_person_number, institution_tin,
              // matched_payee_account_number, and run_date to the
              // error report.
              // ******************************************************************
              MoveFinancialInstitutionDataMatch3(local.SaveFinancialInstitution,
                local.Create);
              MoveFinancialInstitutionDataMatch2(local.AccountOwnerRec,
                local.Create);
              UseOeValidateFidmData();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }

              // ******************************************************************
              // Create the FIDM table entry.  If an error is returned, write
              // the cse_person_number, institution_tin,
              // matched_payee_account_number, and run_date to the
              // error report.
              // ******************************************************************
              UseOeCreateFidmData();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }

              local.End.Timestamp = local.Create.CreatedTimestamp;

              if (AsChar(local.SaveFirstBRecordTs.Flag) == 'Y')
              {
                // ******************************************************************
                // This is the first "B" record processed for this institution.
                // Save the timestamp of the record to be passed to the
                // Raise_Event_For_FIDM cab.
                // ******************************************************************
                local.Start.Timestamp = local.Create.CreatedTimestamp;
                local.SaveFirstBRecordTs.Flag = "N";
              }

              // ******************************************************************
              // Increment the total number of FIDM records matched
              // and the total FIDM dollar amount.
              // ******************************************************************
              ++local.FidmRecordsMatched.Count;
              local.FidmDollarAmount.TotalCurrency += local.Create.
                AccountBalance.GetValueOrDefault();

              break;
            case 'T':
              // ******************************************************************
              // A Total Record was returned.
              // ******************************************************************
              // ******************************************************************
              // The "T" record indicates that we have processed all the "B"
              // records for the institution.  Next, create the infrastructure
              // records used by the event processor to create the alerts to
              // the worker(s).
              // ******************************************************************
              UseOeRaiseEventForFidm();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                local.AbendErrorFound.Flag = "Y";
                ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

                break;
              }

              local.Start.Timestamp = local.Null1.Timestamp;
              local.End.Timestamp = local.Null1.Timestamp;

              // ******************************************************************
              // Perform a checkpoint before processing the next financial
              // institution.  This will save the keys for this financial
              // institution needed in case of a re-start.
              // ******************************************************************
              // ************************************************************
              // The layout of the Restart Info field should be as follows:
              //     Attribute                         Format
              //     
              // --------------------------------
              // ------
              //     Institution TIN			N9
              //     Institution Name         		A40
              //     Match Run Date         		N6  (CCYYMM)
              // ************************************************************
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo =
                local.SaveFinancialInstitution.InstitutionTin + (
                  local.SaveFinancialInstitution.InstitutionName ?? "") + local
                .SaveFinancialInstitution.MatchRunDate;
              local.ProgramCheckpointRestart.CheckpointCount =
                local.ProgramCheckpointRestart.CheckpointCount.
                  GetValueOrDefault() + 1;
              UseUpdatePgmCheckpointRestart2();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                return;
              }

              // ******************************************************************
              // Perform a commit to save all the data for the institution.
              // ******************************************************************
              UseExtToDoACommit();

              if (local.PassArea.NumericReturnCode != 0)
              {
                return;
              }

              break;
            default:
              break;
          }

          break;
        case "EF":
          // *****************************************************************
          // End Of File.  The EAB closed the file.  Complete processing.
          // *****************************************************************
          if (local.FidmRecordCount.Count == 0)
          {
            local.AbendErrorFound.Flag = "Y";
            ExitState = "ACO_RE0000_INPUT_FILE_EMPTY_RB";
            UseEabRollbackSql();

            goto AfterCycle;
          }

          ExitState = "ACO_NN0000_ALL_OK";
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.ProgramError1 = "";

          // *****  End Of File.  The EAB closed the file.  Complete processing.
          goto AfterCycle;
        default:
          UseEabRollbackSql();

          // *****
          // Set the Error Message Text to the exit state description.
          // *****
          ExitState = "FILE_READ_ERROR_WITH_RB";
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              "File Error in External = \"" + local.PassArea.TextReturnCode + "\"   ES = " +
            local.ExitStateWorkArea.Message;
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.SystemGeneratedIdentifier =
              local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.SystemGeneratedIdentifier +
            1;
          local.AbendErrorFound.Flag = "Y";

          goto AfterCycle;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        continue;
      }
      else
      {
        // * * * * * * * * * * * * * * * * * *
        // Report Layout.
        // * * * * * * * * * * * * * * * * * *
        local.AaaGroupLocalErrors.Index = 0;
        local.AaaGroupLocalErrors.CheckSize();

        // Institution Number to Report
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = local.SaveFinancialInstitution.InstitutionTin;

        // Person Number to Report
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 22) + local
          .AccountOwnerRec.CsePersonNumber;

        // Account Number to Report
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 37) + local
          .AccountOwnerRec.MatchedPayeeAccountNumber;

        // YEAR to Report
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 59) + local
          .AccountOwnerRec.MatchRunDate;

        // ERROR MESSAGE to Report
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 71) + local
          .ExitStateWorkArea.Message;

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Open the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        if (AsChar(local.ErrorOpened.Flag) != 'Y')
        {
          local.ReportHandlingEabReportSend.ProcessDate =
            local.ProgramProcessingInfo.ProcessDate;
          local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
          local.ReportHandlingEabFileHandling.Action = "OPEN";
          UseCabErrorReport2();

          if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
          {
          }
          else
          {
            UseEabRollbackSql();
            ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

            return;
          }

          local.ErrorOpened.Flag = "Y";
        }

        if (local.ErrorNumberOfLines.Count > 48)
        {
          // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
          // * * * *
          // Write the ERROR Report
          // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
          // * * * *
          local.ReportHandlingEabReportSend.RptDetail =
            "Institution Number    CSE Person     Account Number        Run Date    Message";
            
          local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
          local.ReportHandlingEabFileHandling.Action = "WRITE";
          UseCabErrorReport1();

          if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
          {
          }
          else
          {
            UseEabRollbackSql();
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }

          // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
          // * * * *
          // Write the ERROR Report -- Spacing
          // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
          // * * * *
          local.ReportHandlingEabReportSend.RptDetail = "";
          local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
          local.ReportHandlingEabFileHandling.Action = "WRITE";
          UseCabErrorReport1();

          if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
          {
          }
          else
          {
            UseEabRollbackSql();
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }

          local.ErrorNumberOfLines.Count = 0;
        }

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            KeyInfo ?? Spaces(132);
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          UseEabRollbackSql();
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        ++local.ErrorNumberOfLines.Count;

        // This is the counter for the number of errors encountered.
        ++local.FidmNoMatchCount.Count;
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

AfterCycle:

    if (AsChar(local.AbendErrorFound.Flag) == 'Y')
    {
      UseEabRollbackSql();

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Open the ERROR Report
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      if (AsChar(local.ErrorOpened.Flag) != 'Y')
      {
        local.ReportHandlingEabReportSend.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
        local.ReportHandlingEabFileHandling.Action = "OPEN";
        UseCabErrorReport2();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

          return;
        }

        local.ErrorOpened.Flag = "Y";
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        if (IsEmpty(local.AaaGroupLocalErrors.Item.
          AaaGroupLocalErrorsDetailProgramError.KeyInfo))
        {
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              local.ExitStateWorkArea.Message;
        }

        local.ReportHandlingEabReportSend.RptDetail =
          local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            KeyInfo ?? Spaces(132);
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabErrorReport1();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the ERROR Report -- Spacing
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabErrorReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }
    }
    else
    {
      // *****
      // Set restart indicator to no because we successfully finished this 
      // program.
      // *****
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.CheckpointCount = -1;
      UseUpdatePgmCheckpointRestart1();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (AsChar(local.ErrorOpened.Flag) == 'Y')
    {
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the ERROR Report -- Spacing
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabErrorReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the ERROR Report
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail =
        "Total FIDM Error Created............" + NumberToString
        (local.FidmNoMatchCount.Count, 6, 10);
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabErrorReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the ERROR Report -- Spacing
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabErrorReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Close the ERROR Report
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
      local.ReportHandlingEabFileHandling.Action = "CLOSE";
      UseCabErrorReport2();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

        return;
      }

      if (IsExitState("ACO_RE0000_INPUT_FILE_EMPTY_RB"))
      {
        // *****
        // Set restart indicator to no because we successfully finished this 
        // program.
        // *****
        local.ProgramCheckpointRestart.RestartInfo = "";
        local.ProgramCheckpointRestart.RestartInd = "N";
        local.ProgramCheckpointRestart.CheckpointCount = -1;
        ExitState = "ACO_NN0000_ALL_OK";
        UseUpdatePgmCheckpointRestart1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        local.AbendErrorFound.Flag = "N";
        ExitState = "ACO_RE0000_INPUT_FILE_EMPTY_RB";
      }
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Open the Control Report
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
    local.ReportHandlingEabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_OPENING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total Count of ALL Records Read
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total FIDM Records Read....................................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      ((long)0 + local.FidmRecordCount.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Total of ALL FIDM records Matched
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total FIDM Records Matched.....................................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString((long)0 + local
      .FidmRecordsMatched.Count + 0, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- NET Amount
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total FIDM Dollar Amount..................................................";
      
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      ((long)local.FidmDollarAmount.TotalCurrency, 4, 12) + ".00";
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // If an ERROR Occurred -- Display Additional Control Report Message
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    if (AsChar(local.ErrorOpened.Flag) == 'Y')
    {
      if (AsChar(local.AbendErrorFound.Flag) == 'Y')
      {
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the Control Report -- ERROR Message
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   ERRORS Occurred -- See ERROR Report for Additional Information   * * * * * *";
          
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

          return;
        }
      }
      else if (local.FidmRecordCount.Count == 0)
      {
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the Control Report -- ERROR Message
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   Input File is EMPTY   * * * * * *";
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

          return;
        }
      }
      else if (local.FidmNoMatchCount.Count == local.FidmRecordCount.Count)
      {
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the Control Report -- ERROR Message
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   NO Records created -- Possible DUPLICATE File --See ERROR Report for Additional Information   * * * * * *";
          
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

          return;
        }
      }
      else
      {
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the Control Report -- SUCCESSFUL Message
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   Successful END of JOB   * * * * * *";
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

          return;
        }
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the Control Report -- Spacing
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }
    }
    else
    {
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the Control Report -- SUCCESSFUL Message
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail =
        "* * * * * *   Successful END of JOB - NO Report Created  * * * * * *";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the Control Report -- Spacing
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
      local.ReportHandlingEabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Close the Control Report
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB427";
    local.ReportHandlingEabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    if (AsChar(local.AbendErrorFound.Flag) == 'Y')
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    // 05/18/99 Added CLOSE per e-mail from Terri Studer
    // "batch jobs that call adabas"  05/15/99  10:50 AM
    local.CloseCsePersonsWorkSet.Ssn = "close";
    UseEabReadCsePersonUsingSsn();

    // Do NOT need to verify on Return
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal1(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveExternal2(External source, External target)
  {
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveFinancialInstitutionDataMatch1(
    FinancialInstitutionDataMatch source, FinancialInstitutionDataMatch target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.InstitutionTin = source.InstitutionTin;
    target.MatchedPayeeAccountNumber = source.MatchedPayeeAccountNumber;
    target.MatchRunDate = source.MatchRunDate;
    target.MatchedPayeeSsn = source.MatchedPayeeSsn;
    target.MatchedPayeeName = source.MatchedPayeeName;
    target.MatchedPayeeStreetAddress = source.MatchedPayeeStreetAddress;
    target.MatchedPayeeCity = source.MatchedPayeeCity;
    target.MatchedPayeeState = source.MatchedPayeeState;
    target.MatchedPayeeZipCode = source.MatchedPayeeZipCode;
    target.MatchedPayeeZip4 = source.MatchedPayeeZip4;
    target.MatchedPayeeZip3 = source.MatchedPayeeZip3;
    target.PayeeForeignCountryIndicator = source.PayeeForeignCountryIndicator;
    target.MatchFlag = source.MatchFlag;
    target.AccountBalance = source.AccountBalance;
    target.AccountType = source.AccountType;
    target.TrustFundIndicator = source.TrustFundIndicator;
    target.AccountBalanceIndicator = source.AccountBalanceIndicator;
    target.DateOfBirth = source.DateOfBirth;
    target.PayeeIndicator = source.PayeeIndicator;
    target.AccountFullLegalTitle = source.AccountFullLegalTitle;
    target.PrimarySsn = source.PrimarySsn;
    target.SecondPayeeName = source.SecondPayeeName;
    target.SecondPayeeSsn = source.SecondPayeeSsn;
    target.MsfidmIndicator = source.MsfidmIndicator;
    target.InstitutionName = source.InstitutionName;
    target.InstitutionStreetAddress = source.InstitutionStreetAddress;
    target.InstitutionCity = source.InstitutionCity;
    target.InstitutionState = source.InstitutionState;
    target.InstitutionZipCode = source.InstitutionZipCode;
    target.InstitutionZip4 = source.InstitutionZip4;
    target.InstitutionZip3 = source.InstitutionZip3;
    target.SecondInstitutionName = source.SecondInstitutionName;
    target.TransmitterTin = source.TransmitterTin;
    target.TransmitterName = source.TransmitterName;
    target.TransmitterStreetAddress = source.TransmitterStreetAddress;
    target.TransmitterCity = source.TransmitterCity;
    target.TransmitterState = source.TransmitterState;
    target.TransmitterZipCode = source.TransmitterZipCode;
    target.TransmitterZip4 = source.TransmitterZip4;
    target.TransmitterZip3 = source.TransmitterZip3;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveFinancialInstitutionDataMatch2(
    FinancialInstitutionDataMatch source, FinancialInstitutionDataMatch target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.MatchedPayeeAccountNumber = source.MatchedPayeeAccountNumber;
    target.MatchRunDate = source.MatchRunDate;
    target.MatchedPayeeSsn = source.MatchedPayeeSsn;
    target.MatchedPayeeName = source.MatchedPayeeName;
    target.MatchedPayeeStreetAddress = source.MatchedPayeeStreetAddress;
    target.MatchedPayeeCity = source.MatchedPayeeCity;
    target.MatchedPayeeState = source.MatchedPayeeState;
    target.MatchedPayeeZipCode = source.MatchedPayeeZipCode;
    target.MatchedPayeeZip4 = source.MatchedPayeeZip4;
    target.MatchedPayeeZip3 = source.MatchedPayeeZip3;
    target.PayeeForeignCountryIndicator = source.PayeeForeignCountryIndicator;
    target.MatchFlag = source.MatchFlag;
    target.AccountBalance = source.AccountBalance;
    target.AccountType = source.AccountType;
    target.TrustFundIndicator = source.TrustFundIndicator;
    target.AccountBalanceIndicator = source.AccountBalanceIndicator;
    target.DateOfBirth = source.DateOfBirth;
    target.PayeeIndicator = source.PayeeIndicator;
    target.AccountFullLegalTitle = source.AccountFullLegalTitle;
    target.PrimarySsn = source.PrimarySsn;
    target.SecondPayeeName = source.SecondPayeeName;
    target.SecondPayeeSsn = source.SecondPayeeSsn;
  }

  private static void MoveFinancialInstitutionDataMatch3(
    FinancialInstitutionDataMatch source, FinancialInstitutionDataMatch target)
  {
    target.InstitutionTin = source.InstitutionTin;
    target.MatchRunDate = source.MatchRunDate;
    target.InstitutionName = source.InstitutionName;
    target.InstitutionStreetAddress = source.InstitutionStreetAddress;
    target.InstitutionCity = source.InstitutionCity;
    target.InstitutionState = source.InstitutionState;
    target.InstitutionZipCode = source.InstitutionZipCode;
    target.InstitutionZip4 = source.InstitutionZip4;
    target.InstitutionZip3 = source.InstitutionZip3;
    target.SecondInstitutionName = source.SecondInstitutionName;
    target.TransmitterTin = source.TransmitterTin;
    target.TransmitterName = source.TransmitterName;
    target.TransmitterStreetAddress = source.TransmitterStreetAddress;
    target.TransmitterCity = source.TransmitterCity;
    target.TransmitterState = source.TransmitterState;
    target.TransmitterZipCode = source.TransmitterZipCode;
    target.TransmitterZip4 = source.TransmitterZip4;
    target.TransmitterZip3 = source.TransmitterZip3;
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

    useImport.NeededToWrite.RptDetail =
      local.ReportHandlingEabReportSend.RptDetail;
    useImport.EabFileHandling.Action =
      local.ReportHandlingEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportHandlingEabFileHandling.Status =
      useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.ReportHandlingEabReportSend, useImport.NeededToOpen);
      
    useImport.EabFileHandling.Action =
      local.ReportHandlingEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportHandlingEabFileHandling.Status =
      useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail =
      local.ReportHandlingEabReportSend.RptDetail;
    useImport.EabFileHandling.Action =
      local.ReportHandlingEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportHandlingEabFileHandling.Status =
      useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.ReportHandlingEabReportSend, useImport.NeededToOpen);
      
    useImport.EabFileHandling.Action =
      local.ReportHandlingEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportHandlingEabFileHandling.Status =
      useExport.EabFileHandling.Status;
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.AdabasResponseCd = useExport.AbendData.AdabasResponseCd;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseEabFidmMethod2MatchDrvr1()
  {
    var useImport = new EabFidmMethod2MatchDrvr.Import();
    var useExport = new EabFidmMethod2MatchDrvr.Export();

    MoveExternal1(local.PassArea, useImport.External);
    useExport.FinancialInstitutionRec.Assign(local.FinancialInstitutionRec);
    useExport.AccountOwnerRec.Assign(local.AccountOwnerRec);
    useExport.RecordTypeReturned.Text1 = local.RecordTypeReturned.Text1;
    MoveExternal2(local.PassArea, useExport.External);

    Call(EabFidmMethod2MatchDrvr.Execute, useImport, useExport);

    local.FinancialInstitutionRec.Assign(useExport.FinancialInstitutionRec);
    local.AccountOwnerRec.Assign(useExport.AccountOwnerRec);
    local.RecordTypeReturned.Text1 = useExport.RecordTypeReturned.Text1;
    MoveExternal2(useExport.External, local.PassArea);
  }

  private void UseEabFidmMethod2MatchDrvr2()
  {
    var useImport = new EabFidmMethod2MatchDrvr.Import();
    var useExport = new EabFidmMethod2MatchDrvr.Export();

    useImport.Restart.Assign(local.Restart);
    MoveExternal1(local.PassArea, useImport.External);
    MoveExternal2(local.PassArea, useExport.External);

    Call(EabFidmMethod2MatchDrvr.Execute, useImport, useExport);

    MoveExternal2(useExport.External, local.PassArea);
  }

  private void UseEabFidmMethod2MatchDrvr3()
  {
    var useImport = new EabFidmMethod2MatchDrvr.Import();
    var useExport = new EabFidmMethod2MatchDrvr.Export();

    MoveExternal1(local.PassArea, useImport.External);
    MoveExternal2(local.PassArea, useExport.External);

    Call(EabFidmMethod2MatchDrvr.Execute, useImport, useExport);

    MoveExternal2(useExport.External, local.PassArea);
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CloseCsePersonsWorkSet.Ssn;
    useExport.CsePersonsWorkSet.Assign(local.Returned);
    useExport.AbendData.Assign(local.CloseAbendData);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    local.Returned.Assign(useExport.CsePersonsWorkSet);
    local.CloseAbendData.Assign(useExport.AbendData);
  }

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    Call(EabRollbackSql.Execute, useImport, useExport);
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeCreateFidmData()
  {
    var useImport = new OeCreateFidmData.Import();
    var useExport = new OeCreateFidmData.Export();

    MoveFinancialInstitutionDataMatch1(local.Create,
      useImport.FinancialInstitutionDataMatch);

    Call(OeCreateFidmData.Execute, useImport, useExport);

    local.Create.CreatedTimestamp =
      useExport.FinancialInstitutionDataMatch.CreatedTimestamp;
  }

  private void UseOeRaiseEventForFidm()
  {
    var useImport = new OeRaiseEventForFidm.Import();
    var useExport = new OeRaiseEventForFidm.Export();

    useImport.Start.Timestamp = local.Start.Timestamp;
    useImport.Ending.Timestamp = local.End.Timestamp;

    Call(OeRaiseEventForFidm.Execute, useImport, useExport);
  }

  private void UseOeValidateFidmData()
  {
    var useImport = new OeValidateFidmData.Import();
    var useExport = new OeValidateFidmData.Export();

    MoveFinancialInstitutionDataMatch1(local.Create,
      useImport.FinancialInstitutionDataMatch);

    Call(OeValidateFidmData.Execute, useImport, useExport);

    local.Create.Assign(useExport.FinancialInstitutionDataMatch);
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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// <summary>A AaaGroupLocalErrorsGroup group.</summary>
    [Serializable]
    public class AaaGroupLocalErrorsGroup
    {
      /// <summary>
      /// A value of AaaGroupLocalErrorsDetailProgramError.
      /// </summary>
      [JsonPropertyName("aaaGroupLocalErrorsDetailProgramError")]
      public ProgramError AaaGroupLocalErrorsDetailProgramError
      {
        get => aaaGroupLocalErrorsDetailProgramError ??= new();
        set => aaaGroupLocalErrorsDetailProgramError = value;
      }

      /// <summary>
      /// A value of AaaGroupLocalErrorsDetailStandard.
      /// </summary>
      [JsonPropertyName("aaaGroupLocalErrorsDetailStandard")]
      public Standard AaaGroupLocalErrorsDetailStandard
      {
        get => aaaGroupLocalErrorsDetailStandard ??= new();
        set => aaaGroupLocalErrorsDetailStandard = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private ProgramError aaaGroupLocalErrorsDetailProgramError;
      private Standard aaaGroupLocalErrorsDetailStandard;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of FidmDollarAmount.
    /// </summary>
    [JsonPropertyName("fidmDollarAmount")]
    public Common FidmDollarAmount
    {
      get => fidmDollarAmount ??= new();
      set => fidmDollarAmount = value;
    }

    /// <summary>
    /// A value of FidmRecordsMatched.
    /// </summary>
    [JsonPropertyName("fidmRecordsMatched")]
    public Common FidmRecordsMatched
    {
      get => fidmRecordsMatched ??= new();
      set => fidmRecordsMatched = value;
    }

    /// <summary>
    /// A value of SaveFirstBRecordTs.
    /// </summary>
    [JsonPropertyName("saveFirstBRecordTs")]
    public Common SaveFirstBRecordTs
    {
      get => saveFirstBRecordTs ??= new();
      set => saveFirstBRecordTs = value;
    }

    /// <summary>
    /// A value of SaveFinancialInstitution.
    /// </summary>
    [JsonPropertyName("saveFinancialInstitution")]
    public FinancialInstitutionDataMatch SaveFinancialInstitution
    {
      get => saveFinancialInstitution ??= new();
      set => saveFinancialInstitution = value;
    }

    /// <summary>
    /// A value of FinancialInstitutionRec.
    /// </summary>
    [JsonPropertyName("financialInstitutionRec")]
    public FinancialInstitutionDataMatch FinancialInstitutionRec
    {
      get => financialInstitutionRec ??= new();
      set => financialInstitutionRec = value;
    }

    /// <summary>
    /// A value of AccountOwnerRec.
    /// </summary>
    [JsonPropertyName("accountOwnerRec")]
    public FinancialInstitutionDataMatch AccountOwnerRec
    {
      get => accountOwnerRec ??= new();
      set => accountOwnerRec = value;
    }

    /// <summary>
    /// A value of RecordTypeReturned.
    /// </summary>
    [JsonPropertyName("recordTypeReturned")]
    public TextWorkArea RecordTypeReturned
    {
      get => recordTypeReturned ??= new();
      set => recordTypeReturned = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public FinancialInstitutionDataMatch Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of HardcodeRead.
    /// </summary>
    [JsonPropertyName("hardcodeRead")]
    public External HardcodeRead
    {
      get => hardcodeRead ??= new();
      set => hardcodeRead = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// Gets a value of AaaGroupLocalErrors.
    /// </summary>
    [JsonIgnore]
    public Array<AaaGroupLocalErrorsGroup> AaaGroupLocalErrors =>
      aaaGroupLocalErrors ??= new(AaaGroupLocalErrorsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AaaGroupLocalErrors for json serialization.
    /// </summary>
    [JsonPropertyName("aaaGroupLocalErrors")]
    [Computed]
    public IList<AaaGroupLocalErrorsGroup> AaaGroupLocalErrors_Json
    {
      get => aaaGroupLocalErrors;
      set => AaaGroupLocalErrors.Assign(value);
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
    /// A value of ReportHandlingEabReportSend.
    /// </summary>
    [JsonPropertyName("reportHandlingEabReportSend")]
    public EabReportSend ReportHandlingEabReportSend
    {
      get => reportHandlingEabReportSend ??= new();
      set => reportHandlingEabReportSend = value;
    }

    /// <summary>
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
    }

    /// <summary>
    /// A value of AbendErrorFound.
    /// </summary>
    [JsonPropertyName("abendErrorFound")]
    public Common AbendErrorFound
    {
      get => abendErrorFound ??= new();
      set => abendErrorFound = value;
    }

    /// <summary>
    /// A value of RecordsProcessed.
    /// </summary>
    [JsonPropertyName("recordsProcessed")]
    public Common RecordsProcessed
    {
      get => recordsProcessed ??= new();
      set => recordsProcessed = value;
    }

    /// <summary>
    /// A value of FidmRecordCount.
    /// </summary>
    [JsonPropertyName("fidmRecordCount")]
    public Common FidmRecordCount
    {
      get => fidmRecordCount ??= new();
      set => fidmRecordCount = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public FinancialInstitutionDataMatch Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of ReportHandlingEabFileHandling.
    /// </summary>
    [JsonPropertyName("reportHandlingEabFileHandling")]
    public EabFileHandling ReportHandlingEabFileHandling
    {
      get => reportHandlingEabFileHandling ??= new();
      set => reportHandlingEabFileHandling = value;
    }

    /// <summary>
    /// A value of ErrorOpened.
    /// </summary>
    [JsonPropertyName("errorOpened")]
    public Common ErrorOpened
    {
      get => errorOpened ??= new();
      set => errorOpened = value;
    }

    /// <summary>
    /// A value of ErrorNumberOfLines.
    /// </summary>
    [JsonPropertyName("errorNumberOfLines")]
    public Common ErrorNumberOfLines
    {
      get => errorNumberOfLines ??= new();
      set => errorNumberOfLines = value;
    }

    /// <summary>
    /// A value of FidmNoMatchCount.
    /// </summary>
    [JsonPropertyName("fidmNoMatchCount")]
    public Common FidmNoMatchCount
    {
      get => fidmNoMatchCount ??= new();
      set => fidmNoMatchCount = value;
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
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of CloseCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("closeCsePersonsWorkSet")]
    public CsePersonsWorkSet CloseCsePersonsWorkSet
    {
      get => closeCsePersonsWorkSet ??= new();
      set => closeCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public CsePersonsWorkSet Returned
    {
      get => returned ??= new();
      set => returned = value;
    }

    /// <summary>
    /// A value of CloseAbendData.
    /// </summary>
    [JsonPropertyName("closeAbendData")]
    public AbendData CloseAbendData
    {
      get => closeAbendData ??= new();
      set => closeAbendData = value;
    }

    private DateWorkArea null1;
    private Common fidmDollarAmount;
    private Common fidmRecordsMatched;
    private Common saveFirstBRecordTs;
    private FinancialInstitutionDataMatch saveFinancialInstitution;
    private FinancialInstitutionDataMatch financialInstitutionRec;
    private FinancialInstitutionDataMatch accountOwnerRec;
    private TextWorkArea recordTypeReturned;
    private FinancialInstitutionDataMatch restart;
    private AbendData abendData;
    private External hardcodeOpen;
    private External hardcodeRead;
    private External hardcodePosition;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External passArea;
    private Array<AaaGroupLocalErrorsGroup> aaaGroupLocalErrors;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend reportHandlingEabReportSend;
    private Common firstTime;
    private Common abendErrorFound;
    private Common recordsProcessed;
    private Common fidmRecordCount;
    private FinancialInstitutionDataMatch create;
    private EabFileHandling reportHandlingEabFileHandling;
    private Common errorOpened;
    private Common errorNumberOfLines;
    private Common fidmNoMatchCount;
    private DateWorkArea start;
    private DateWorkArea end;
    private CsePersonsWorkSet closeCsePersonsWorkSet;
    private CsePersonsWorkSet returned;
    private AbendData closeAbendData;
  }
#endregion
}
