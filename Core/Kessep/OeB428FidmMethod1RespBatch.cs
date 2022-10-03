// Program: OE_B428_FIDM_METHOD1_RESP_BATCH, ID: 374406979, model: 746.
// Short name: SWEE428B
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
/// A program: OE_B428_FIDM_METHOD1_RESP_BATCH.
/// </para>
/// <para>
/// This skeleton is an example that uses:
/// A sequential driver file
/// An external to do a DB2 commit
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB428FidmMethod1RespBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B428_FIDM_METHOD1_RESP_BATCH program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB428FidmMethod1RespBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB428FidmMethod1RespBatch.
  /// </summary>
  public OeB428FidmMethod1RespBatch(IContext context, Import import,
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
    // *****************************************************************
    // Date         Request #  Developer  Description
    // --------     ---------  ---------  --------------------
    // 05/13/00     WR000166   pphinney   NEW Transaction
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

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

    local.HardcodeOpen.FileInstruction = "OPEN";
    local.HardcodeRead.FileInstruction = "READ";
    local.HardcodePosition.FileInstruction = "POSITION";
    local.FirstTime.Flag = "Y";

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
        local.Restart.InstitutionTin =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 9);
        local.Restart.MatchRunDate =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 10, 6);
        local.Restart.InstitutionName =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 16, 40);
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

      // *************************************
      // CALL EXTERNAL TO POSITION THE DRIVER FILE
      // *************************************
      UseEabFidmMethod1MatchDrvr1();

      if (!Equal(local.PassArea.TextReturnCode, "00") && !
        Equal(local.PassArea.TextReturnCode, "10"))
      {
        ExitState = "FILE_POSITION_ERROR_WITH_RB";

        return;
      }
    }
    else
    {
      // *************************************
      // CALL EXTERNAL TO OPEN THE DRIVER FILE
      // *************************************
      local.PassArea.FileInstruction = local.HardcodeOpen.FileInstruction;
      UseEabFidmMethod1MatchDrvr3();

      if (!Equal(local.PassArea.TextReturnCode, "00"))
      {
        ExitState = "ZD_FILE_OPEN_ERROR_WITH_AB";

        return;
      }
    }

    // *************************************************************
    // Initialize the CONTROL Report Table
    // *************************************************************
    // ********************************
    // PROCESS DRIVER RECORDS UNTIL EOF
    // ********************************
    local.SkipToNextHeaderRecord.Flag = "N";
    local.FirstTime.Flag = "Y";
    local.ErrorNumberOfLines.Count = 85;

    do
    {
      global.Command = "";

      // *************************************
      // CALL EXTERNAL TO READ THE DRIVER FILE
      // *************************************
      local.PassArea.FileInstruction = local.HardcodeRead.FileInstruction;
      local.CauseAbend.Flag = "N";

      if (AsChar(local.ProgramCheckpointRestart.RestartInd) != 'Y' || AsChar
        (local.ProgramCheckpointRestart.RestartInd) == 'Y' && AsChar
        (local.FirstTime.Flag) == 'N')
      {
        UseEabFidmMethod1MatchDrvr2();
      }

      local.ProgramCheckpointRestart.RestartInd = "N";

      switch(TrimEnd(local.PassArea.TextReturnCode))
      {
        case "D1":
          ExitState = "ACO_NI0000_INVALID_DATE";
          local.PassArea.TextReturnCode = "00";

          break;
        case "D2":
          ExitState = "ACO_NI0000_INVALID_DATE";
          local.PassArea.TextReturnCode = "00";

          break;
        default:
          break;
      }

      switch(TrimEnd(local.PassArea.TextReturnCode))
      {
        case "00":
          switch(AsChar(local.ReturnedRecType.Text1))
          {
            case 'A':
              // Update the Checkpoint Restart
              if (AsChar(local.FirstTime.Flag) == 'N')
              {
                local.ProgramCheckpointRestart.RestartInfo =
                  local.Returned.InstitutionTin;
                local.ProgramCheckpointRestart.RestartInfo =
                  Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1,
                  9) + local.Returned.MatchRunDate;
                local.ProgramCheckpointRestart.RestartInfo =
                  Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1,
                  15) + (local.Returned.InstitutionName ?? "");
                local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
                local.ProgramCheckpointRestart.ProgramName = global.UserId;
                local.ProgramCheckpointRestart.RestartInd = "Y";
                ExitState = "ACO_NN0000_ALL_OK";
                UseUpdatePgmCheckpointRestart();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                }
                else
                {
                  return;
                }

                local.ReadSinceLastCommit.Count = 0;
                UseExtToDoACommit();

                if (Equal(local.PassArea.TextReturnCode, "00"))
                {
                }
                else
                {
                  ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

                  break;
                }
              }

              local.End.Timestamp = Now();
              local.Start.Timestamp = Now();
              MoveFinancialInstitutionDataMatch3(local.Returned, local.SaveAType);
                
              ++local.Swefb428InstitutionsIn.Count;
              local.FirstTime.Flag = "N";

              break;
            case 'B':
              ++local.Fidm1RecordCount.Count;
              local.Create.Assign(local.Returned);
              MoveFinancialInstitutionDataMatch3(local.SaveAType, local.Create);
              local.Create.PayeeIndicator = "0";
              local.Create.MatchFlag = "0";

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.GetPerson.Ssn = local.Returned.PrimarySsn ?? Spaces(9);
                UseEabReadCsePersonUsingSsn();

                if (!IsEmpty(local.GetPerson.Number))
                {
                  if (ReadCsePerson())
                  {
                    if (!ReadCsePersonAccount())
                    {
                      goto Test1;
                    }
                  }
                  else
                  {
                    goto Test1;
                  }

                  ReadAdministrativeActCertification();

                  // If NOT FOUND -- this will NOT = 'FDSO'
                  if (!Equal(entities.AdministrativeActCertification.Type1,
                    "FDSO"))
                  {
                    goto Test1;
                  }

                  // Only process if AMOUNT > Zero
                  if (!Lt(0,
                    entities.AdministrativeActCertification.CurrentAmount))
                  {
                    goto Test1;
                  }

                  local.Create.CsePersonNumber = local.GetPerson.Number;
                  local.Create.MatchedPayeeSsn = local.Returned.PrimarySsn ?? Spaces
                    (9);

                  if (IsEmpty(local.Returned.SecondPayeeSsn) || IsEmpty
                    (local.Returned.SecondPayeeName))
                  {
                    local.Create.PayeeIndicator = "0";
                  }
                  else
                  {
                    local.Create.PayeeIndicator = "2";
                  }

                  goto Test2;
                }

Test1:

                if (IsEmpty(local.Returned.SecondPayeeSsn))
                {
                  continue;
                }

                local.GetPerson.Ssn = local.Returned.SecondPayeeSsn ?? Spaces
                  (9);
                UseEabReadCsePersonUsingSsn();

                if (!IsEmpty(local.GetPerson.Number))
                {
                  if (ReadCsePerson())
                  {
                    if (!ReadCsePersonAccount())
                    {
                      continue;
                    }
                  }
                  else
                  {
                    continue;
                  }

                  ReadAdministrativeActCertification();

                  // If NOT FOUND -- this will NOT = 'FDSO'
                  if (!Equal(entities.AdministrativeActCertification.Type1,
                    "FDSO"))
                  {
                    continue;
                  }

                  // Only process if AMOUNT > Zero
                  if (!Lt(0,
                    entities.AdministrativeActCertification.CurrentAmount))
                  {
                    continue;
                  }

                  local.Create.PayeeIndicator = "1";
                  local.Create.CsePersonNumber = local.GetPerson.Number;
                  local.Create.MatchedPayeeSsn =
                    local.Returned.SecondPayeeSsn ?? Spaces(9);
                }
                else
                {
                  continue;
                }
              }
              else
              {
                break;
              }

Test2:

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseOeValidateFidmData();
              }
              else
              {
                break;
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.Create.MsfidmIndicator = "";
                local.Create.CreatedTimestamp = Now();
                UseOeCreateFidmData();
                local.End.Timestamp = Now();
              }
              else
              {
                break;
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ++local.RecordsProcessed.Count;
                ++local.TotalRecordsProcessed.Count;
                local.Fidm1CollectionAmt.TotalCurrency += local.Create.
                  AccountBalance.GetValueOrDefault();
              }
              else if (IsExitState("FINANCIAL_INSTITUTION_DATA_MA_AE"))
              {
                ++local.Fidm2DuplicateCount.Count;
              }
              else
              {
              }

              break;
            case 'T':
              local.End.Timestamp = Now();
              UseOeRaiseEventForFidm();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.RecordsProcessed.Count = 0;
              }
              else
              {
                local.AbendErrorFound.Flag = "Y";
                ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

                goto Test3;
              }

              break;
            default:
              ExitState = "ACO_RE0000_INPUT_RECORD_TYPE_INV";

              goto Test3;
          }

          ++local.ReadSinceLastCommit.Count;

          break;
        case "EF":
          // *****************************************************************
          // End Of File.  The EAB closed the file.  Complete processing.
          // *****************************************************************
          local.ProgramCheckpointRestart.RestartInfo = "";
          local.ProgramCheckpointRestart.RestartInd = "N";
          ExitState = "ACO_NN0000_ALL_OK";
          UseUpdatePgmCheckpointRestart();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            goto AfterCycle;
          }

          UseExtToDoACommit();

          if (Equal(local.PassArea.TextReturnCode, "00"))
          {
          }
          else
          {
            ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

            goto AfterCycle;
          }

          goto AfterCycle;
        default:
          ExitState = "FILE_READ_ERROR_WITH_RB";
          local.CauseAbend.Flag = "Y";

          // * * * * *
          // Print LINE 1
          // * * * * *
          ++local.AaaGroupLocalErrors.Index;
          local.AaaGroupLocalErrors.CheckSize();

          // *****************************************************************
          // Set the Key Info with the Court number and the creation date
          // of the transaction file.
          // *****************************************************************
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo = "TIN = " + local
            .Returned.InstitutionTin;
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              TrimEnd(local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Interface Date = " +
            local.Returned.MatchRunDate;
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              TrimEnd(local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Input File Return Status = " +
            local.PassArea.TextReturnCode;
          local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
            Command = "ROLLBACK";

          // * * * * *
          // Print LINE 2
          // * * * * *
          // *****************************************************************
          // Set the Error Message Text to the exit state description.
          // *****************************************************************
          ++local.AaaGroupLocalErrors.Index;
          local.AaaGroupLocalErrors.CheckSize();

          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              TrimEnd(local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "Exit State = " +
            local.ExitStateWorkArea.Message;
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.ProgramError1 =
              "Error in header processing";
          local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
            Command = "ROLLBACK";

          // * * * * *
          // Print LINE 3 (spacing)
          // * * * * *
          ++local.AaaGroupLocalErrors.Index;
          local.AaaGroupLocalErrors.CheckSize();

          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo = "";
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.Code = "SPACING";
          ExitState = "ACO_NN0000_ALL_OK";
          local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
            Command = "";

          goto AfterCycle;
      }

Test3:

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
          KeyInfo = local.Create.InstitutionTin;

        // Person Number to Report
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 22) + local
          .Create.CsePersonNumber;

        // Account Number to Report
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 37) + local
          .Create.MatchedPayeeAccountNumber;

        // YEAR to Report
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 59) + local
          .Create.MatchRunDate;

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
          local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
          local.ReportHandlingEabFileHandling.Action = "OPEN";
          UseCabErrorReport3();

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
            
          local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
          local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
        ++local.Fidm1NoMatchCount.Count;
        ++local.TotalErrors.Count;
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      ExitState = "ACO_NN0000_ALL_OK";
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

AfterCycle:

    // *****************************************************************
    // The external hit the end of the driver file, closed the file and
    // returned an EF (EOF) indicator.
    // *****************************************************************
    // IS ERROR Report open?
    if (AsChar(local.ErrorOpened.Flag) == 'Y')
    {
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the ERROR Report
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the ERROR Report
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail =
        "Total Errors Created = " + NumberToString
        (local.TotalErrors.Count, 15);
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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

      local.ReportProcessing.Action = "CLOSE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB610";
      local.ReportHandlingEabReportSend.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

        return;
      }
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Open the Control Report
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
      "Total FIDM Method 1 Records Read..................................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      ((long)0 + local.Fidm1RecordCount.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
    // Write the Control Report -- Total of ALL FIDM1 records Matched
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total FIDM Method 1 Records Matched...................................";

    // HERE SWSRPDP
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString((long)0 + local
      .TotalRecordsProcessed.Count + 0, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
      "Total FIDM Method 1 Dollar Amount................................................";
      
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      ((long)local.Fidm1CollectionAmt.TotalCurrency, 4, 12) + ".00";
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
          
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
      else if (local.Fidm1RecordCount.Count == 0)
      {
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the Control Report -- ERROR Message
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   Input File is EMPTY   * * * * * *";
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
      else if (local.Fidm1NoMatchCount.Count == local.Fidm1RecordCount.Count)
      {
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the Control Report -- ERROR Message
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   NO Records created -- Possible DUPLICATE File --See ERROR Report for Additional Information   * * * * * *";
          
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
        if (local.Fidm1RecordCount.Count == 0)
        {
          local.ReportHandlingEabReportSend.RptDetail =
            "* * * * * *   Input File is EMPTY   * * * * * *";
        }
        else
        {
          local.ReportHandlingEabReportSend.RptDetail =
            "* * * * * *   Successful END of JOB   * * * * * *";
        }

        local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
      if (local.Fidm1RecordCount.Count == 0)
      {
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   Input File is EMPTY   * * * * * *";
      }
      else
      {
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   Successful END of JOB - NO ERROR Report Created  * * * * * *";
          
      }

      local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB428";
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

    if (AsChar(local.CauseAbend.Flag) == 'Y')
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.NumericReturnCode = source.NumericReturnCode;
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
  }

  private static void MoveFinancialInstitutionDataMatch2(
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

  private static void MoveFinancialInstitutionDataMatch3(
    FinancialInstitutionDataMatch source, FinancialInstitutionDataMatch target)
  {
    target.InstitutionTin = source.InstitutionTin;
    target.MatchFlag = source.MatchFlag;
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
      
    useImport.EabFileHandling.Action = local.ReportProcessing.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
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

  private void UseEabFidmMethod1MatchDrvr1()
  {
    var useImport = new EabFidmMethod1MatchDrvr.Import();
    var useExport = new EabFidmMethod1MatchDrvr.Export();

    useImport.Restart.Assign(local.Restart);
    useImport.PassArea.Assign(local.PassArea);
    useExport.ReturnedRecType.Text1 = local.ReturnedRecType.Text1;
    useExport.ReturnedFinancialInstitutionDataMatch.Assign(local.Returned);
    useExport.ReturnedExternal.Assign(local.PassArea);

    Call(EabFidmMethod1MatchDrvr.Execute, useImport, useExport);

    local.ReturnedRecType.Text1 = useExport.ReturnedRecType.Text1;
    MoveFinancialInstitutionDataMatch1(useExport.
      ReturnedFinancialInstitutionDataMatch, local.Returned);
    MoveExternal(useExport.ReturnedExternal, local.PassArea);
  }

  private void UseEabFidmMethod1MatchDrvr2()
  {
    var useImport = new EabFidmMethod1MatchDrvr.Import();
    var useExport = new EabFidmMethod1MatchDrvr.Export();

    useImport.PassArea.Assign(local.PassArea);
    useExport.ReturnedRecType.Text1 = local.ReturnedRecType.Text1;
    useExport.ReturnedFinancialInstitutionDataMatch.Assign(local.Returned);
    useExport.ReturnedExternal.Assign(local.PassArea);

    Call(EabFidmMethod1MatchDrvr.Execute, useImport, useExport);

    local.ReturnedRecType.Text1 = useExport.ReturnedRecType.Text1;
    MoveFinancialInstitutionDataMatch1(useExport.
      ReturnedFinancialInstitutionDataMatch, local.Returned);
    MoveExternal(useExport.ReturnedExternal, local.PassArea);
  }

  private void UseEabFidmMethod1MatchDrvr3()
  {
    var useImport = new EabFidmMethod1MatchDrvr.Import();
    var useExport = new EabFidmMethod1MatchDrvr.Export();

    useImport.PassArea.Assign(local.PassArea);
    useExport.ReturnedExternal.Assign(local.PassArea);

    Call(EabFidmMethod1MatchDrvr.Execute, useImport, useExport);

    MoveExternal(useExport.ReturnedExternal, local.PassArea);
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.GetPerson.Ssn;
    MoveCsePersonsWorkSet(local.GetPerson, useExport.CsePersonsWorkSet);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.GetPerson);
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

    MoveFinancialInstitutionDataMatch2(local.Create,
      useImport.FinancialInstitutionDataMatch);

    Call(OeCreateFidmData.Execute, useImport, useExport);

    local.Create.CreatedTimestamp =
      useExport.FinancialInstitutionDataMatch.CreatedTimestamp;
  }

  private void UseOeRaiseEventForFidm()
  {
    var useImport = new OeRaiseEventForFidm.Import();
    var useExport = new OeRaiseEventForFidm.Export();

    useImport.Ending.Timestamp = local.End.Timestamp;
    useImport.Start.Timestamp = local.Start.Timestamp;

    Call(OeRaiseEventForFidm.Execute, useImport, useExport);
  }

  private void UseOeValidateFidmData()
  {
    var useImport = new OeValidateFidmData.Import();
    var useExport = new OeValidateFidmData.Export();

    MoveFinancialInstitutionDataMatch2(local.Create,
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

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private bool ReadAdministrativeActCertification()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.CurrentAmount =
          db.GetNullableDecimal(reader, 4);
        entities.AdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 5);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 6);
        entities.AdministrativeActCertification.EtypeFinancialInstitution =
          db.GetNullableString(reader, 7);
        entities.AdministrativeActCertification.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.GetPerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
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
      public const int Capacity = 10;

      private ProgramError aaaGroupLocalErrorsDetailProgramError;
      private Standard aaaGroupLocalErrorsDetailStandard;
    }

    /// <summary>
    /// A value of TotalRecordsProcessed.
    /// </summary>
    [JsonPropertyName("totalRecordsProcessed")]
    public Common TotalRecordsProcessed
    {
      get => totalRecordsProcessed ??= new();
      set => totalRecordsProcessed = value;
    }

    /// <summary>
    /// A value of TotalErrors.
    /// </summary>
    [JsonPropertyName("totalErrors")]
    public Common TotalErrors
    {
      get => totalErrors ??= new();
      set => totalErrors = value;
    }

    /// <summary>
    /// A value of ReturnedRecType.
    /// </summary>
    [JsonPropertyName("returnedRecType")]
    public TextWorkArea ReturnedRecType
    {
      get => returnedRecType ??= new();
      set => returnedRecType = value;
    }

    /// <summary>
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public FinancialInstitutionDataMatch Returned
    {
      get => returned ??= new();
      set => returned = value;
    }

    /// <summary>
    /// A value of SaveAType.
    /// </summary>
    [JsonPropertyName("saveAType")]
    public FinancialInstitutionDataMatch SaveAType
    {
      get => saveAType ??= new();
      set => saveAType = value;
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
    /// A value of ErrorAbend.
    /// </summary>
    [JsonPropertyName("errorAbend")]
    public Common ErrorAbend
    {
      get => errorAbend ??= new();
      set => errorAbend = value;
    }

    /// <summary>
    /// A value of HeaderPrinted.
    /// </summary>
    [JsonPropertyName("headerPrinted")]
    public Common HeaderPrinted
    {
      get => headerPrinted ??= new();
      set => headerPrinted = value;
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
    /// A value of ReportProcessing.
    /// </summary>
    [JsonPropertyName("reportProcessing")]
    public EabFileHandling ReportProcessing
    {
      get => reportProcessing ??= new();
      set => reportProcessing = value;
    }

    /// <summary>
    /// A value of CauseAbend.
    /// </summary>
    [JsonPropertyName("causeAbend")]
    public Common CauseAbend
    {
      get => causeAbend ??= new();
      set => causeAbend = value;
    }

    /// <summary>
    /// A value of ErrorOpen.
    /// </summary>
    [JsonPropertyName("errorOpen")]
    public Common ErrorOpen
    {
      get => errorOpen ??= new();
      set => errorOpen = value;
    }

    /// <summary>
    /// A value of ControlOpen.
    /// </summary>
    [JsonPropertyName("controlOpen")]
    public Common ControlOpen
    {
      get => controlOpen ??= new();
      set => controlOpen = value;
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
    /// A value of Swefb428InstitutionsIn.
    /// </summary>
    [JsonPropertyName("swefb428InstitutionsIn")]
    public Common Swefb428InstitutionsIn
    {
      get => swefb428InstitutionsIn ??= new();
      set => swefb428InstitutionsIn = value;
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
    /// A value of ReadSinceLastCommit.
    /// </summary>
    [JsonPropertyName("readSinceLastCommit")]
    public Common ReadSinceLastCommit
    {
      get => readSinceLastCommit ??= new();
      set => readSinceLastCommit = value;
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
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of SkipToNextHeaderRecord.
    /// </summary>
    [JsonPropertyName("skipToNextHeaderRecord")]
    public Common SkipToNextHeaderRecord
    {
      get => skipToNextHeaderRecord ??= new();
      set => skipToNextHeaderRecord = value;
    }

    /// <summary>
    /// A value of RecordTypeReturned.
    /// </summary>
    [JsonPropertyName("recordTypeReturned")]
    public Common RecordTypeReturned
    {
      get => recordTypeReturned ??= new();
      set => recordTypeReturned = value;
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
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
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
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public DateWorkArea Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
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
    /// A value of Fidm1CollectionAmt.
    /// </summary>
    [JsonPropertyName("fidm1CollectionAmt")]
    public Common Fidm1CollectionAmt
    {
      get => fidm1CollectionAmt ??= new();
      set => fidm1CollectionAmt = value;
    }

    /// <summary>
    /// A value of Fidm2DuplicateCount.
    /// </summary>
    [JsonPropertyName("fidm2DuplicateCount")]
    public Common Fidm2DuplicateCount
    {
      get => fidm2DuplicateCount ??= new();
      set => fidm2DuplicateCount = value;
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
    /// A value of Fidm1NoMatchCount.
    /// </summary>
    [JsonPropertyName("fidm1NoMatchCount")]
    public Common Fidm1NoMatchCount
    {
      get => fidm1NoMatchCount ??= new();
      set => fidm1NoMatchCount = value;
    }

    /// <summary>
    /// A value of Fidm1RecordCount.
    /// </summary>
    [JsonPropertyName("fidm1RecordCount")]
    public Common Fidm1RecordCount
    {
      get => fidm1RecordCount ??= new();
      set => fidm1RecordCount = value;
    }

    /// <summary>
    /// A value of MsfidmCollectionAmt.
    /// </summary>
    [JsonPropertyName("msfidmCollectionAmt")]
    public Common MsfidmCollectionAmt
    {
      get => msfidmCollectionAmt ??= new();
      set => msfidmCollectionAmt = value;
    }

    /// <summary>
    /// A value of GetPerson.
    /// </summary>
    [JsonPropertyName("getPerson")]
    public CsePersonsWorkSet GetPerson
    {
      get => getPerson ??= new();
      set => getPerson = value;
    }

    private Common totalRecordsProcessed;
    private Common totalErrors;
    private TextWorkArea returnedRecType;
    private FinancialInstitutionDataMatch returned;
    private FinancialInstitutionDataMatch saveAType;
    private FinancialInstitutionDataMatch restart;
    private Common errorAbend;
    private Common headerPrinted;
    private EabReportSend reportHandlingEabReportSend;
    private EabFileHandling reportProcessing;
    private Common causeAbend;
    private Common errorOpen;
    private Common controlOpen;
    private AbendData abendData;
    private Common swefb428InstitutionsIn;
    private Common firstTime;
    private Common readSinceLastCommit;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramRun programRun;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private Common skipToNextHeaderRecord;
    private Common recordTypeReturned;
    private External hardcodeOpen;
    private External hardcodeRead;
    private External hardcodePosition;
    private Array<AaaGroupLocalErrorsGroup> aaaGroupLocalErrors;
    private DateWorkArea end;
    private DateWorkArea start;
    private DateWorkArea initialize;
    private Common recordsProcessed;
    private Common fidm1CollectionAmt;
    private Common fidm2DuplicateCount;
    private Common abendErrorFound;
    private FinancialInstitutionDataMatch create;
    private EabFileHandling reportHandlingEabFileHandling;
    private Common errorOpened;
    private Common errorNumberOfLines;
    private Common fidm1NoMatchCount;
    private Common fidm1RecordCount;
    private Common msfidmCollectionAmt;
    private CsePersonsWorkSet getPerson;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
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

    private CsePersonAccount csePersonAccount;
    private AdministrativeActCertification administrativeActCertification;
    private CsePerson csePerson;
  }
#endregion
}
