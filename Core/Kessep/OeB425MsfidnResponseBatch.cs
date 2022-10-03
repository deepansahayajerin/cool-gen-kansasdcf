// Program: OE_B425_MSFIDN_RESPONSE_BATCH, ID: 374391270, model: 746.
// Short name: SWEE425B
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
/// A program: OE_B425_MSFIDN_RESPONSE_BATCH.
/// </para>
/// <para>
/// This skeleton is an example that uses:
/// A sequential driver file
/// An external to do a DB2 commit
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB425MsfidnResponseBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B425_MSFIDN_RESPONSE_BATCH program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB425MsfidnResponseBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB425MsfidnResponseBatch.
  /// </summary>
  public OeB425MsfidnResponseBatch(IContext context, Import import,
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
    // ******************************
    // MAINTENANCE LOG
    // ******************************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // P.Phinney	05/10/00   Initial Code
    // P.Phinney	03/25/02  I0140394  Add Account_Status_Indicator to FIDM Entity
    // Joyce Harden       06/01/09  CQ7813  Close ADABAS
    // 06/29/2009   DDupree     Added check on matched ssn and cse person number
    // against the invalid ssn table. Part of CQ7189.
    // __________________________________________________________________________________
    // *********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Save.Date = Now().Date;
    local.HardcodeOpen.FileInstruction = "OPEN";
    local.HardcodeRead.FileInstruction = "READ";
    local.HardcodePosition.FileInstruction = "POSITION";
    local.FirstTime.Flag = "Y";
    local.ErrorNumberOfLines.Count = 75;

    // *****
    // Get the run parameters for this program.
    // *****
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *****
    // Get the DB2 commit frequency counts and find out if we are in a restart 
    // situation.
    // *****
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ALWAYS ReStarts from Beginning
      // * * * NO Restart Logic * * *
    }
    else
    {
      return;
    }

    // *****
    // Open the Input File.  If this program run is a restart, open and 
    // reposition the file to the next record to be processed.
    // *****
    // *****
    // Call external to open the driver file.
    // *****
    local.PassArea.FileInstruction = local.HardcodeOpen.FileInstruction;
    UseEabMsfidmDataMatchDrvr2();

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

    // *****
    // Process driver records until we have reached the end of file.
    // *****
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

      // ***** Call external to read the driver file.
      ExitState = "ACO_NN0000_ALL_OK";
      local.PassArea.FileInstruction = local.HardcodeRead.FileInstruction;
      UseEabMsfidmDataMatchDrvr1();

      // * Convert Return Code to Exit-State
      switch(TrimEnd(local.PassArea.TextReturnCode))
      {
        case "MC":
          local.PassArea.TextReturnCode = "00";
          ExitState = "OE_RECORD_TYPE_NOT_MC";

          break;
        case "ST":
          local.PassArea.TextReturnCode = "00";
          ExitState = "OE_INVALID_STATE";

          break;
        case "D1":
          local.PassArea.TextReturnCode = "00";
          ExitState = "OE_INVALID_MATCH_DATE";

          break;
        case "D2":
          local.PassArea.TextReturnCode = "00";
          ExitState = "OE_INVALID_DOB";

          break;
        default:
          break;
      }

      switch(TrimEnd(local.PassArea.TextReturnCode))
      {
        case "00":
          ++local.MsfidmRecordCount.Count;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // * * * * * * * * *
            // ADD OE_VALIDATE_FIDM_DATA -- HERE
            // * * * * * * * * *
            UseOeValidateFidmData();
          }
          else
          {
            break;
          }

          // added this check as part of cq7189.
          local.Convert.SsnNum9 =
            (int)StringToNumber(local.ReturnedFinancialInstitutionDataMatch.
              MatchedPayeeSsn);

          if (ReadInvalidSsn())
          {
            local.Convert.SsnTextPart1 =
              Substring(local.ReturnedFinancialInstitutionDataMatch.
                MatchedPayeeSsn, 1, 3);
            local.Convert.SsnTextPart2 =
              Substring(local.ReturnedFinancialInstitutionDataMatch.
                MatchedPayeeSsn, 4, 2);
            local.Convert.SsnTextPart3 =
              Substring(local.ReturnedFinancialInstitutionDataMatch.
                MatchedPayeeSsn, 6, 4);
            local.Message1.Text11 = local.Convert.SsnTextPart1 + "-" + local
              .Convert.SsnTextPart2 + "-" + local.Convert.SsnTextPart3;
            local.Message1.Text8 = "Bad SSN";
            local.Message1.Text6 = ", Per";
            local.Message1.Text16 = ": Rec not used -";
            local.Message2.Text6 = " and $";
            local.Message2.Text8 = ", Acct #";
            local.Message2.Text15 =
              NumberToString((long)(local.ReturnedFinancialInstitutionDataMatch.
                AccountBalance.GetValueOrDefault() * 100), 15);
            local.Message2.Text7 = Substring(local.Message2.Text15, 8, 6);
            local.Message2.Text2 = Substring(local.Message2.Text15, 14, 2);
            local.Message2.Text9 = TrimEnd(local.Message2.Text7) + "." + local
              .Message2.Text2;
            local.ReportHandlingEabReportSend.RptDetail =
              local.Message1.Text8 + local.Message1.Text11 + local
              .Message1.Text6 + local
              .ReturnedFinancialInstitutionDataMatch.CsePersonNumber + local
              .Message1.Text16 + TrimEnd
              (local.ReturnedFinancialInstitutionDataMatch.InstitutionName) + local
              .Message2.Text8 + TrimEnd
              (local.ReturnedFinancialInstitutionDataMatch.
                MatchedPayeeAccountNumber) + local.Message2.Text6 + local
              .Message2.Text9;
            ExitState = "INVALID_SSN";

            break;
          }
          else
          {
            // this is fine, there is not invalid ssn record for this 
            // combination of cse person number and ssn number
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ReturnedFinancialInstitutionDataMatch.MsfidmIndicator = "Y";
            UseOeCreateFidmData();
            local.End.Timestamp =
              local.ReturnedFinancialInstitutionDataMatch.CreatedTimestamp;

            if (local.RecordsProcessed.Count == 0)
            {
              local.Start.Timestamp =
                local.ReturnedFinancialInstitutionDataMatch.CreatedTimestamp;
            }
          }
          else
          {
            break;
          }

          // RESTART Logic - currently does Nothing
          if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
          {
            local.ProgramCheckpointRestart.RestartInd = "N";
          }

          ++local.ReadSinceLastCommit.Count;

          // * * * * * * * * *
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.RecordsProcessed.Count;
            local.MsfidmCollectionAmt.TotalCurrency += local.
              ReturnedFinancialInstitutionDataMatch.AccountBalance.
                GetValueOrDefault();
          }
          else if (IsExitState("FINANCIAL_INSTITUTION_DATA_MA_AE"))
          {
            ++local.MsfidmDuplicateCount.Count;
          }
          else
          {
          }

          break;
        case "EF":
          if (local.MsfidmRecordCount.Count == 0)
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
          KeyInfo = local.ReturnedFinancialInstitutionDataMatch.InstitutionTin;

        // Person Number to Report
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 22) + local
          .ReturnedFinancialInstitutionDataMatch.CsePersonNumber;

        // Account Number to Report
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 37) + local
          .ReturnedFinancialInstitutionDataMatch.MatchedPayeeAccountNumber;

        // YEAR to Report
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 59) + local
          .ReturnedFinancialInstitutionDataMatch.MatchRunDate;

        // ERROR MESSAGE to Report
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 71) + local
          .ExitStateWorkArea.Message;

        if (IsExitState("INVALID_SSN"))
        {
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              local.ReportHandlingEabReportSend.RptDetail;
        }

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Open the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        if (AsChar(local.ErrorOpened.Flag) != 'Y')
        {
          local.ReportHandlingEabReportSend.ProcessDate =
            local.ProgramProcessingInfo.ProcessDate;
          local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
            
          local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
          local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
        ++local.MsfidmNoMatchCount.Count;
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

AfterCycle:

    // *****
    // The external hit the end of the driver file,
    // closed the file and returned an EF (EOF) indicator.
    // *****
    if (AsChar(local.AbendErrorFound.Flag) != 'Y')
    {
      UseOeRaiseEventForFidm();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        local.AbendErrorFound.Flag = "Y";
        ExitState = "FN0000_ERROR_ON_EVENT_CREATION";
      }
    }

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
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
      UseUpdatePgmCheckpointRestart();

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
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
        "Total MSFIDM Error Created............" + NumberToString
        (local.MsfidmNoMatchCount.Count, 6, 10);
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
        UseUpdatePgmCheckpointRestart();

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
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
      "Total MSFIDM Records Read..................................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      ((long)0 + local.MsfidmRecordCount.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
    // Write the Control Report -- Total of ALL MSFIDM records Matched
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total MFSIDM Records Matched...................................";

    // HERE SWSRPDP
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString((long)0 + local
      .RecordsProcessed.Count + 0, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
      "Total MSFIDM Dollar Amount................................................";
      
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 50) + NumberToString
      ((long)local.MsfidmCollectionAmt.TotalCurrency, 4, 12) + ".00";
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
          
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
      else if (local.MsfidmRecordCount.Count == 0)
      {
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the Control Report -- ERROR Message
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   Input File is EMPTY   * * * * * *";
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
      else if (local.MsfidmNoMatchCount.Count == local.MsfidmRecordCount.Count)
      {
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the Control Report -- ERROR Message
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   NO Records created -- Possible DUPLICATE File --See ERROR Report for Additional Information   * * * * * *";
          
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB425";
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

    // 05/18/99 Added CLOSE per e-mail from Terri Struder
    // "batch jobs that call adabas"  05/15/99  10:50 AM
    local.CloseCsePersonsWorkSet.Ssn = "close";
    UseEabReadCsePersonUsingSsn();

    // Do NOT need to verify on Return
    // Close Adabas per CQ 7813
    UseSiCloseAdabas();
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

    useImport.EabFileHandling.Action =
      local.ReportHandlingEabFileHandling.Action;
    useImport.NeededToWrite.RptDetail =
      local.ReportHandlingEabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportHandlingEabFileHandling.Status =
      useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action =
      local.ReportHandlingEabFileHandling.Action;
    MoveEabReportSend(local.ReportHandlingEabReportSend, useImport.NeededToOpen);
      

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportHandlingEabFileHandling.Status =
      useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action =
      local.ReportHandlingEabFileHandling.Action;
    useImport.NeededToWrite.RptDetail =
      local.ReportHandlingEabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportHandlingEabFileHandling.Status =
      useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action =
      local.ReportHandlingEabFileHandling.Action;
    MoveEabReportSend(local.ReportHandlingEabReportSend, useImport.NeededToOpen);
      

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportHandlingEabFileHandling.Status =
      useExport.EabFileHandling.Status;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseEabMsfidmDataMatchDrvr1()
  {
    var useImport = new EabMsfidmDataMatchDrvr.Import();
    var useExport = new EabMsfidmDataMatchDrvr.Export();

    MoveExternal1(local.PassArea, useImport.External);
    useExport.FinancialInstitutionDataMatch.Assign(
      local.ReturnedFinancialInstitutionDataMatch);
    MoveExternal2(local.PassArea, useExport.External);

    Call(EabMsfidmDataMatchDrvr.Execute, useImport, useExport);

    local.ReturnedFinancialInstitutionDataMatch.Assign(
      useExport.FinancialInstitutionDataMatch);
    MoveExternal2(useExport.External, local.PassArea);
  }

  private void UseEabMsfidmDataMatchDrvr2()
  {
    var useImport = new EabMsfidmDataMatchDrvr.Import();
    var useExport = new EabMsfidmDataMatchDrvr.Export();

    MoveExternal1(local.PassArea, useImport.External);
    MoveExternal2(local.PassArea, useExport.External);

    Call(EabMsfidmDataMatchDrvr.Execute, useImport, useExport);

    MoveExternal2(useExport.External, local.PassArea);
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CloseCsePersonsWorkSet.Ssn;
    useExport.CsePersonsWorkSet.Assign(local.ReturnedCsePersonsWorkSet);
    useExport.AbendData.Assign(local.CloseAbendData);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    local.ReturnedCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.CloseAbendData.Assign(useExport.AbendData);
  }

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    Call(EabRollbackSql.Execute, useImport, useExport);
  }

  private void UseOeCreateFidmData()
  {
    var useImport = new OeCreateFidmData.Import();
    var useExport = new OeCreateFidmData.Export();

    useImport.FinancialInstitutionDataMatch.Assign(
      local.ReturnedFinancialInstitutionDataMatch);

    Call(OeCreateFidmData.Execute, useImport, useExport);

    local.ReturnedFinancialInstitutionDataMatch.CreatedTimestamp =
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

    useImport.FinancialInstitutionDataMatch.Assign(
      local.ReturnedFinancialInstitutionDataMatch);

    Call(OeValidateFidmData.Execute, useImport, useExport);

    local.ReturnedFinancialInstitutionDataMatch.Assign(
      useExport.FinancialInstitutionDataMatch);
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

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private bool ReadInvalidSsn()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.Convert.SsnNum9);
        db.SetString(
          command, "cspNumber",
          local.ReturnedFinancialInstitutionDataMatch.CsePersonNumber);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
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
      public const int Capacity = 1;

      private ProgramError aaaGroupLocalErrorsDetailProgramError;
      private Standard aaaGroupLocalErrorsDetailStandard;
    }

    /// <summary>
    /// A value of ReturnedFinancialInstitutionDataMatch.
    /// </summary>
    [JsonPropertyName("returnedFinancialInstitutionDataMatch")]
    public FinancialInstitutionDataMatch ReturnedFinancialInstitutionDataMatch
    {
      get => returnedFinancialInstitutionDataMatch ??= new();
      set => returnedFinancialInstitutionDataMatch = value;
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
    /// A value of ReturnedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("returnedCsePersonsWorkSet")]
    public CsePersonsWorkSet ReturnedCsePersonsWorkSet
    {
      get => returnedCsePersonsWorkSet ??= new();
      set => returnedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of MsfidmRecordCount.
    /// </summary>
    [JsonPropertyName("msfidmRecordCount")]
    public Common MsfidmRecordCount
    {
      get => msfidmRecordCount ??= new();
      set => msfidmRecordCount = value;
    }

    /// <summary>
    /// A value of MsfidmNoMatchCount.
    /// </summary>
    [JsonPropertyName("msfidmNoMatchCount")]
    public Common MsfidmNoMatchCount
    {
      get => msfidmNoMatchCount ??= new();
      set => msfidmNoMatchCount = value;
    }

    /// <summary>
    /// A value of MsfidmDuplicateCount.
    /// </summary>
    [JsonPropertyName("msfidmDuplicateCount")]
    public Common MsfidmDuplicateCount
    {
      get => msfidmDuplicateCount ??= new();
      set => msfidmDuplicateCount = value;
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
    /// A value of JobRestarted.
    /// </summary>
    [JsonPropertyName("jobRestarted")]
    public Common JobRestarted
    {
      get => jobRestarted ??= new();
      set => jobRestarted = value;
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
    /// A value of AbendErrorFound.
    /// </summary>
    [JsonPropertyName("abendErrorFound")]
    public Common AbendErrorFound
    {
      get => abendErrorFound ??= new();
      set => abendErrorFound = value;
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
    /// A value of ErrorNumberOfLines.
    /// </summary>
    [JsonPropertyName("errorNumberOfLines")]
    public Common ErrorNumberOfLines
    {
      get => errorNumberOfLines ??= new();
      set => errorNumberOfLines = value;
    }

    /// <summary>
    /// A value of FindDuplicateDaysCount.
    /// </summary>
    [JsonPropertyName("findDuplicateDaysCount")]
    public Common FindDuplicateDaysCount
    {
      get => findDuplicateDaysCount ??= new();
      set => findDuplicateDaysCount = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public DateWorkArea Save
    {
      get => save ??= new();
      set => save = value;
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
    /// A value of ReportHandlingEabReportSend.
    /// </summary>
    [JsonPropertyName("reportHandlingEabReportSend")]
    public EabReportSend ReportHandlingEabReportSend
    {
      get => reportHandlingEabReportSend ??= new();
      set => reportHandlingEabReportSend = value;
    }

    /// <summary>
    /// A value of RestartRecordNumber.
    /// </summary>
    [JsonPropertyName("restartRecordNumber")]
    public Common RestartRecordNumber
    {
      get => restartRecordNumber ??= new();
      set => restartRecordNumber = value;
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
    /// A value of RecordsProcessed.
    /// </summary>
    [JsonPropertyName("recordsProcessed")]
    public Common RecordsProcessed
    {
      get => recordsProcessed ??= new();
      set => recordsProcessed = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public ProgramError Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of CloseCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("closeCsePersonsWorkSet")]
    public CsePersonsWorkSet CloseCsePersonsWorkSet
    {
      get => closeCsePersonsWorkSet ??= new();
      set => closeCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public SsnWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of Message1.
    /// </summary>
    [JsonPropertyName("message1")]
    public WorkArea Message1
    {
      get => message1 ??= new();
      set => message1 = value;
    }

    /// <summary>
    /// A value of Message2.
    /// </summary>
    [JsonPropertyName("message2")]
    public WorkArea Message2
    {
      get => message2 ??= new();
      set => message2 = value;
    }

    private FinancialInstitutionDataMatch returnedFinancialInstitutionDataMatch;
    private DateWorkArea start;
    private DateWorkArea end;
    private CsePersonsWorkSet returnedCsePersonsWorkSet;
    private Common msfidmRecordCount;
    private Common msfidmNoMatchCount;
    private Common msfidmDuplicateCount;
    private Common msfidmCollectionAmt;
    private Common jobRestarted;
    private Common errorOpened;
    private Common abendErrorFound;
    private Common firstTime;
    private Common errorNumberOfLines;
    private Common findDuplicateDaysCount;
    private DateWorkArea save;
    private AbendData closeAbendData;
    private EabFileHandling reportHandlingEabFileHandling;
    private EabReportSend reportHandlingEabReportSend;
    private Common restartRecordNumber;
    private Common readSinceLastCommit;
    private Common recordsProcessed;
    private ProgramError initialized;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private External hardcodeOpen;
    private External hardcodeRead;
    private External hardcodePosition;
    private Array<AaaGroupLocalErrorsGroup> aaaGroupLocalErrors;
    private CsePersonsWorkSet closeCsePersonsWorkSet;
    private SsnWorkArea convert;
    private WorkArea message1;
    private WorkArea message2;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
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

    private InvalidSsn invalidSsn;
    private CsePerson csePerson;
  }
#endregion
}
