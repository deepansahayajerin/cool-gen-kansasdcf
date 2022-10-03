// Program: OE_B488_PROCESS_LOCATE_RESPONSE, ID: 374437841, model: 746.
// Short name: SWEE488B
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
/// A program: OE_B488_PROCESS_LOCATE_RESPONSE.
/// </para>
/// <para>
/// This skeleton is an example that uses:
/// A sequential driver file
/// An external to do a DB2 commit
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB488ProcessLocateResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B488_PROCESS_LOCATE_RESPONSE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB488ProcessLocateResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB488ProcessLocateResponse.
  /// </summary>
  public OeB488ProcessLocateResponse(IContext context, Import import,
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
    // *********************************************************
    // Every initial development and change to that development
    // needs to be documented.
    // *********************************************************
    // *********************************************************************
    // Date		Developers Name		Request #	Description
    // --------	----------------------	----------	---------------------
    // 06/27/00	SWSRPDP					Initial Developement
    // 03/2001		SWSRPRM			WR # 291	License Suspension
    // 02/19/2009      Arun Mathias            CQ#7432         Initialized the 
    // read count after the commit.
    // ---------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodeOpen.FileInstruction = "OPEN";
    local.HardcodeRead.FileInstruction = "READ";
    local.HardcodePosition.FileInstruction = "POSITION";
    local.ErrorNumberOfLines.Count = 75;

    // *****************************************
    // Get the run parameters for this program.
    // *****************************************
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
        //     CSE Person Number                  A10
        //     Agency Number                      A5
        //     Sequence Number                    N2                   
        // ************************************************************
        local.Restart.CsePersonNumber =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
        local.Restart.AgencyNumber =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 11, 5);
        local.Restart.SequenceNumber =
          (int)StringToNumber(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250, 14, 2));
      }
    }
    else
    {
      return;
    }

    // ****************************************************************
    // Open the Input File.  If this program run is a restart, open and
    // reposition the file to the next record to be processed.
    // ****************************************************************
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // ********************************************
      // CALL EXTERNAL TO REPOSTITION THE DRIVER FILE
      // ********************************************
      local.PassArea.TextLine80 =
        local.ProgramCheckpointRestart.RestartInfo ?? Spaces(130);
      local.PassArea.FileInstruction = local.HardcodePosition.FileInstruction;
      UseEabProcessLocateResponse1();

      if (!Equal(local.PassArea.TextReturnCode, "00") && !
        Equal(local.PassArea.TextReturnCode, "EF"))
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

        goto Test1;
      }

      local.ProgramCheckpointRestart.RestartInd = "N";
    }
    else
    {
      // *************************************
      // CALL EXTERNAL TO OPEN THE DRIVER FILE
      // *************************************
      local.PassArea.FileInstruction = local.HardcodeOpen.FileInstruction;
      UseEabProcessLocateResponse3();

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

Test1:

    // **************************************************************
    // Process driver records until we have reached the end of file.
    // **************************************************************
    local.RecordsProcessed.Count = 0;
    local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
      SystemGeneratedIdentifier = 0;
    local.Start.Timestamp = Now();

    do
    {
      if (AsChar(local.AbendErrorFound.Flag) == 'Y')
      {
        break;
      }

      // * * * * * * * * * * * * * * * * * * *
      // Open successful - continue processing
      // * * * * * * * * * * * * * * * * * * *
      global.Command = "";

      // ***** Call external to read the driver file.
      ExitState = "ACO_NN0000_ALL_OK";
      local.PassArea.FileInstruction = local.HardcodeRead.FileInstruction;
      UseEabProcessLocateResponse2();

      switch(TrimEnd(local.PassArea.TextReturnCode))
      {
        case "00":
          if (local.LocateRecordCount.Count == 0)
          {
            // --------------------------------------------------------------
            // A HEADER record was put on the outbound file created in
            // SWEEB487 / Run 261 to indicate if the send was
            // a regular locate request or only license suspension so that if
            // the record on MPPI is changed b/t agencies, the proper
            // incoming response records will be processed correctly.
            // If no header record was included in the incoming file, type
            // will default to whatever is on MPPI.
            // ---------------------------------------------------------------
            if (Equal(local.Received.SocialSecurityNumber, "LOCATE"))
            {
              local.ProcessAllLocateRequest.Flag = "Y";
            }
            else if (Equal(local.Received.SocialSecurityNumber, "LICENSE"))
            {
              local.ProcessLicSuspOnly.Flag = "Y";
              local.ProcessAllLocateRequest.Flag = "N";
            }
            else
            {
              local.ParameterTextLength.Count =
                Find(local.ProgramProcessingInfo.ParameterList, ":") + 2;
              local.ParameterStringLength.Count =
                Length(local.ProgramProcessingInfo.ParameterList);
              local.ProcessLicSuspOnly.Flag =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.ParameterTextLength.Count,
                Length(TrimEnd(local.ProgramProcessingInfo.ParameterList)) - local
                .ParameterTextLength.Count + 1);

              if (AsChar(local.ProcessLicSuspOnly.Flag) == 'Y')
              {
              }
              else
              {
                local.ProcessAllLocateRequest.Flag = "Y";
              }

              ++local.LocateRecordCount.Count;

              goto Test2;
            }

            ++local.LocateRecordCount.Count;

            continue;
          }
          else
          {
            ++local.LocateRecordCount.Count;
          }

Test2:

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // * * * * * * * * * * * * * * * * *
            // Validate the LOCATE RESPONSE Data
            // * * * * * * * * * * * * * * * * *
            UseOeValidateLocate();
          }
          else
          {
            break;
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // * * * * * * * * * * * * * * * * * * * *
            // Update the LOCATE RESPONSE Data TABLE
            // * * * * * * * * * * * * * * * * * * * *
            UseOeUpdateLocateResponses();
            local.End.Timestamp = Now();
          }
          else
          {
            break;
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            break;
          }

          // * * * * * * * * * * * * * * * * * * * * * * * * * * *
          //  RESTART Logic - Update Checkpoint - Create ALERTS
          // * * * * * * * * * * * * * * * * * * * * * * * * * * *
          if (local.ProgramCheckpointRestart.UpdateFrequencyCount.
            GetValueOrDefault() <= local.ReadSinceLastCommit.Count)
          {
            UseOeProcessLocateEvents();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              local.AbendErrorFound.Flag = "Y";
              ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

              break;
            }

            // ************************************************************
            // The layout of the Restart Info field should be as follows:
            //     Attribute                         Format
            //     
            // --------------------------------
            // ------
            //     CSE Person Number                  A10
            //     Agency Number                      A5
            //     Sequence Number                    N2                   
            // ************************************************************
            local.ProgramCheckpointRestart.RestartInfo =
              local.Received.CsePersonNumber;
            local.ProgramCheckpointRestart.RestartInfo =
              Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 10) +
              local.Received.AgencyNumber;
            local.ProgramCheckpointRestart.RestartInfo =
              Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 15) +
              NumberToString(local.Received.SequenceNumber, 14, 2);
            local.ProgramCheckpointRestart.RestartInd = "Y";
            local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
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

            UseExtToDoACommit2();
            local.Start.Timestamp = Now();
            local.End.Timestamp = Now();

            // *** CQ#7432 Changes Begin Here ***
            local.ReadSinceLastCommit.Count = 0;

            // *** CQ#7432 Changes End Here ***
          }

          ++local.ReadSinceLastCommit.Count;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.RecordsProcessed.Count;
          }
          else
          {
          }

          break;
        case "DB":
          local.PassArea.TextReturnCode = "00";
          ExitState = "OE_INVALID_DOB";

          break;
        case "DX":
          local.PassArea.TextReturnCode = "00";
          ExitState = "OE_INVALID_EXPERATION_DATE";

          break;
        case "DQ":
          local.PassArea.TextReturnCode = "00";
          ExitState = "OE_INVALID_REQUEST_DATE";

          break;
        case "DI":
          local.PassArea.TextReturnCode = "00";
          ExitState = "OE_INVALID_ISSUE_DATE";

          break;
        case "DS":
          local.PassArea.TextReturnCode = "00";
          ExitState = "OE_INVALID_SUSPENSE_DATE";

          break;
        case "DP":
          local.PassArea.TextReturnCode = "00";
          ExitState = "OE_INVALID_RESPONSE_DATE";

          break;
        case "EF":
          if (local.LocateRecordCount.Count == 0)
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

          // ********************************************************
          // Set the Error Message Text to the exit state description
          // ********************************************************
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

        // Person Number to Report
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = local.Received.CsePersonNumber;

        // ************************
        // Agency Number to Report
        // ************************
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 18) + local
          .Received.AgencyNumber;

        // Sequence Number to Report
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 34) + NumberToString
          (local.Received.SequenceNumber, 14, 2);

        // ************************
        // ERROR MESSAGE to Report
        // ************************
        if (IsExitState("CODE_VALUE_NF"))
        {
          local.ExitStateWorkArea.Message = "Agency Code is Invalid.";
        }
        else
        {
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        }

        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            Substring(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo, 254, 1, 44) + local
          .ExitStateWorkArea.Message;

        // * * * * * * * * * * * * * *
        // Open the ERROR Report
        // * * * * * * * * * * * * * *
        if (AsChar(local.ErrorOpened.Flag) != 'Y')
        {
          local.ReportHandlingEabReportSend.ProcessDate =
            local.ProgramProcessingInfo.ProcessDate;
          local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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
          // * * * * * * * * * * * * * *
          // Write the ERROR Report
          // * * * * * * * * * * * * * *
          local.ReportHandlingEabReportSend.RptDetail =
            "CSE Person    Agency Number    Sequence     Message";
          local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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

          // * * * * * * * * * * * * * * * * * * *
          // Write the ERROR Report -- Spacing
          // * * * * * * * * * * * * * * * * * * *
          local.ReportHandlingEabReportSend.RptDetail = "";
          local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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

        // * * * * * * * * * * * * * * * *
        // Write the ERROR Report
        // * * * * * * * * * * * * * * * *
        local.ReportHandlingEabReportSend.RptDetail =
          local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            KeyInfo ?? Spaces(132);
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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
        ++local.ErrorsWritten.Count;
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

AfterCycle:

    // ****************************************************************
    // The external hit the end of the driver file, closed the file and
    // returned an EF (EOF) indicator.
    // ****************************************************************
    if (AsChar(local.AbendErrorFound.Flag) != 'Y')
    {
      UseOeProcessLocateEvents();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = "";
        local.AbendErrorFound.Flag = "Y";
        UseEabRollbackSql();
        ExitState = "FN0000_ERROR_ON_EVENT_CREATION";
      }
    }

    if (AsChar(local.AbendErrorFound.Flag) == 'Y')
    {
      UseEabRollbackSql();

      // * * * * * * * * * * * * *
      // Open the ERROR Report
      // * * * * * * * * * * * * *
      if (AsChar(local.ErrorOpened.Flag) != 'Y')
      {
        local.ReportHandlingEabReportSend.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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
        // * * * * * * * * * * * * *
        // Write the ERROR Report
        // * * * * * * * * * * * * *
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
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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

      // * * * * * * * * * * * * * * * * * * * * * *
      // Write the ERROR Report -- Spacing
      // * * * * * * * * * * * * * * * * * * * * * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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
      // ************************************************************
      // Set restart indicator to no because we successfully finished
      // this program.
      // ************************************************************
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

      UseExtToDoACommit1();
    }

    if (AsChar(local.ErrorOpened.Flag) == 'Y')
    {
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      // Write the ERROR Report -- Spacing
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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

      // * * * * * * * * * * * * * * *
      // Write the ERROR Report
      // * * * * * * * * * * * * * * *
      local.ReportHandlingEabReportSend.RptDetail =
        "Total Number of Errors Created........" + NumberToString
        (local.ErrorsWritten.Count, 6, 10);
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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

      // * * * * * * * * * * * * * * * * *
      // Write the ERROR Report -- Spacing
      // * * * * * * * * * * * * * * * * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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

      // * * * * * * * * * * * * * * *
      // Close the ERROR Report
      // * * * * * * * * * * * * * * *
      local.ReportHandlingEabReportSend.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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
        // ************************************************************
        // Set restart indicator to no because we successfully finished
        // this program.
        // ************************************************************
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

    // * * * * * * * * * * * * * *
    // Open the Control Report
    // * * * * * * * * * * * * * *
    local.ReportHandlingEabReportSend.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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

    // * * * * * * * * * * * * * * * * *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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

    // * * * * * * * * * * * * * ** * * * * * * * * * * * * *
    // Write the CONTROL Report - Responses Written
    // * * * * * * * * * * * * * ** * * * * * * * * * * * * *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Number of Locate Responses Written........" + NumberToString
      (local.RecordsProcessed.Count, 6, 10);
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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
        // * * * * * * * * * * * * * * * * * * * *
        // Write the Control Report -- ERROR Message
        // * * * * * * * * * * * * * * * * * * * *
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   ERRORS Occurred -- See ERROR Report for Additional Information   * * * * * *";
          
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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
      else if (local.LocateRecordCount.Count == 0)
      {
        // * * * * * * * * * * * * * * * * * * * *
        // Write the Control Report -- ERROR Message
        // * * * * * * * * * * * * * * * * * * * *
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   Input File is EMPTY   * * * * * *";
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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
        // * * * * * * * * * * * * * * * * * * * * * * *
        // Write the Control Report -- SUCCESSFUL Message
        // * * * * * * * * * * * * * * * * * * * * * * *
        local.ReportHandlingEabReportSend.RptDetail =
          "* * * * * *   Successful END of JOB   * * * * * *";
        local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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

      // * * * * * * * * * * * * * * * * * * * *
      // Write the Control Report -- Spacing
      // * * * * * * * * * * * * * * * * * * * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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
      // * * * * * * * * * * * * * * * * * * * * * * *
      // Write the Control Report -- SUCCESSFUL Message
      // * * * * * * * * * * * * * * * * * * * * * * *
      local.ReportHandlingEabReportSend.RptDetail =
        "* * * * * *   Successful END of JOB * * * * * *";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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

      // * * * * * * * * * * * * * * * * * * * *
      // Write the Control Report -- Spacing
      // * * * * * * * * * * * * * * * * * * * *
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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

    // * * * * * * * * * * * *
    // Close the Control Report
    // * * * * * * * * * * * *
    local.ReportHandlingEabReportSend.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.ReportHandlingEabReportSend.ProgramName = "SWEEB488";
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

    // *****************************************************
    // 05/18/99 Added CLOSE per e-mail from Terri Struder:
    //     "batch jobs that call adabas"  05/15/99  10:50 AM
    // *****************************************************
    local.CloseCsePersonsWorkSet.Ssn = "close";
    UseEabReadCsePersonUsingSsn();

    // **************************************************
    //        Do NOT need to verify on Return
    // **************************************************
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

  private static void MoveLocateRequest1(LocateRequest source,
    LocateRequest target)
  {
    target.SocialSecurityNumber = source.SocialSecurityNumber;
    target.DateOfBirth = source.DateOfBirth;
    target.CsePersonNumber = source.CsePersonNumber;
    target.RequestDate = source.RequestDate;
    target.ResponseDate = source.ResponseDate;
    target.LicenseIssuedDate = source.LicenseIssuedDate;
    target.LicenseExpirationDate = source.LicenseExpirationDate;
    target.LicenseSuspendedDate = source.LicenseSuspendedDate;
    target.LicenseNumber = source.LicenseNumber;
    target.AgencyNumber = source.AgencyNumber;
    target.SequenceNumber = source.SequenceNumber;
    target.LicenseSourceName = source.LicenseSourceName;
    target.Street1 = source.Street1;
    target.AddressType = source.AddressType;
    target.Street2 = source.Street2;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.ZipCode3 = source.ZipCode3;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveLocateRequest2(LocateRequest source,
    LocateRequest target)
  {
    target.SocialSecurityNumber = source.SocialSecurityNumber;
    target.DateOfBirth = source.DateOfBirth;
    target.CsePersonNumber = source.CsePersonNumber;
    target.RequestDate = source.RequestDate;
    target.ResponseDate = source.ResponseDate;
    target.LicenseIssuedDate = source.LicenseIssuedDate;
    target.LicenseExpirationDate = source.LicenseExpirationDate;
    target.LicenseSuspendedDate = source.LicenseSuspendedDate;
    target.LicenseNumber = source.LicenseNumber;
    target.AgencyNumber = source.AgencyNumber;
    target.SequenceNumber = source.SequenceNumber;
    target.LicenseSourceName = source.LicenseSourceName;
    target.Street1 = source.Street1;
    target.AddressType = source.AddressType;
    target.Street2 = source.Street2;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.ZipCode3 = source.ZipCode3;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.LicenseSuspensionInd = source.LicenseSuspensionInd;
  }

  private static void MoveLocateRequest3(LocateRequest source,
    LocateRequest target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.AgencyNumber = source.AgencyNumber;
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

  private void UseEabProcessLocateResponse1()
  {
    var useImport = new EabProcessLocateResponse.Import();
    var useExport = new EabProcessLocateResponse.Export();

    useImport.Restart.Assign(local.Restart);
    MoveExternal1(local.PassArea, useImport.External);
    MoveExternal2(local.PassArea, useExport.ReturnedExternal);

    Call(EabProcessLocateResponse.Execute, useImport, useExport);

    MoveExternal2(useExport.ReturnedExternal, local.PassArea);
  }

  private void UseEabProcessLocateResponse2()
  {
    var useImport = new EabProcessLocateResponse.Import();
    var useExport = new EabProcessLocateResponse.Export();

    MoveExternal1(local.PassArea, useImport.External);
    useExport.ReturnedLocateRequest.Assign(local.Received);
    MoveExternal2(local.PassArea, useExport.ReturnedExternal);

    Call(EabProcessLocateResponse.Execute, useImport, useExport);

    local.Received.Assign(useExport.ReturnedLocateRequest);
    MoveExternal2(useExport.ReturnedExternal, local.PassArea);
  }

  private void UseEabProcessLocateResponse3()
  {
    var useImport = new EabProcessLocateResponse.Import();
    var useExport = new EabProcessLocateResponse.Export();

    MoveExternal1(local.PassArea, useImport.External);
    MoveExternal2(local.PassArea, useExport.ReturnedExternal);

    Call(EabProcessLocateResponse.Execute, useImport, useExport);

    MoveExternal2(useExport.ReturnedExternal, local.PassArea);
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CloseCsePersonsWorkSet.Ssn;
    useExport.AbendData.Assign(local.CloseAbendData);
    useExport.CsePersonsWorkSet.Assign(local.CloseReturned);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    local.CloseAbendData.Assign(useExport.AbendData);
    local.CloseReturned.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    Call(EabRollbackSql.Execute, useImport, useExport);
  }

  private void UseExtToDoACommit1()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    Call(ExtToDoACommit.Execute, useImport, useExport);
  }

  private void UseExtToDoACommit2()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeProcessLocateEvents()
  {
    var useImport = new OeProcessLocateEvents.Import();
    var useExport = new OeProcessLocateEvents.Export();

    useImport.ProcessAllLocateRequst.Flag = local.ProcessAllLocateRequest.Flag;
    useImport.Start.Timestamp = local.Start.Timestamp;
    useImport.Ending.Timestamp = local.End.Timestamp;

    Call(OeProcessLocateEvents.Execute, useImport, useExport);
  }

  private void UseOeUpdateLocateResponses()
  {
    var useImport = new OeUpdateLocateResponses.Import();
    var useExport = new OeUpdateLocateResponses.Export();

    useImport.ProcessAllLocateReq.Flag = local.ProcessAllLocateRequest.Flag;
    MoveLocateRequest2(local.Received, useImport.LocateRequest);

    Call(OeUpdateLocateResponses.Execute, useImport, useExport);

    local.LicSuspensionDocument.CsePersonNumber =
      useExport.LicSuspensionDocument.CsePersonNumber;
    MoveLocateRequest1(useExport.LocateRequest, local.Received);
  }

  private void UseOeValidateLocate()
  {
    var useImport = new OeValidateLocate.Import();
    var useExport = new OeValidateLocate.Export();

    MoveLocateRequest3(local.Received, useImport.LocateRequest);

    Call(OeValidateLocate.Execute, useImport, useExport);
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
    /// A value of LicSuspensionDocument.
    /// </summary>
    [JsonPropertyName("licSuspensionDocument")]
    public LocateRequest LicSuspensionDocument
    {
      get => licSuspensionDocument ??= new();
      set => licSuspensionDocument = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public LocateRequest Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Received.
    /// </summary>
    [JsonPropertyName("received")]
    public LocateRequest Received
    {
      get => received ??= new();
      set => received = value;
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
    /// A value of LocateRecordCount.
    /// </summary>
    [JsonPropertyName("locateRecordCount")]
    public Common LocateRecordCount
    {
      get => locateRecordCount ??= new();
      set => locateRecordCount = value;
    }

    /// <summary>
    /// A value of ErrorsWritten.
    /// </summary>
    [JsonPropertyName("errorsWritten")]
    public Common ErrorsWritten
    {
      get => errorsWritten ??= new();
      set => errorsWritten = value;
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
    /// A value of ErrorNumberOfLines.
    /// </summary>
    [JsonPropertyName("errorNumberOfLines")]
    public Common ErrorNumberOfLines
    {
      get => errorNumberOfLines ??= new();
      set => errorNumberOfLines = value;
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
    /// A value of ReportHandlingEabReportSend.
    /// </summary>
    [JsonPropertyName("reportHandlingEabReportSend")]
    public EabReportSend ReportHandlingEabReportSend
    {
      get => reportHandlingEabReportSend ??= new();
      set => reportHandlingEabReportSend = value;
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
    /// A value of CloseAbendData.
    /// </summary>
    [JsonPropertyName("closeAbendData")]
    public AbendData CloseAbendData
    {
      get => closeAbendData ??= new();
      set => closeAbendData = value;
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
    /// A value of CloseReturned.
    /// </summary>
    [JsonPropertyName("closeReturned")]
    public CsePersonsWorkSet CloseReturned
    {
      get => closeReturned ??= new();
      set => closeReturned = value;
    }

    /// <summary>
    /// A value of ParameterTextLength.
    /// </summary>
    [JsonPropertyName("parameterTextLength")]
    public Common ParameterTextLength
    {
      get => parameterTextLength ??= new();
      set => parameterTextLength = value;
    }

    /// <summary>
    /// A value of ParameterStringLength.
    /// </summary>
    [JsonPropertyName("parameterStringLength")]
    public Common ParameterStringLength
    {
      get => parameterStringLength ??= new();
      set => parameterStringLength = value;
    }

    /// <summary>
    /// A value of ProcessLicSuspOnly.
    /// </summary>
    [JsonPropertyName("processLicSuspOnly")]
    public Common ProcessLicSuspOnly
    {
      get => processLicSuspOnly ??= new();
      set => processLicSuspOnly = value;
    }

    /// <summary>
    /// A value of ProcessAllLocateRequest.
    /// </summary>
    [JsonPropertyName("processAllLocateRequest")]
    public Common ProcessAllLocateRequest
    {
      get => processAllLocateRequest ??= new();
      set => processAllLocateRequest = value;
    }

    private LocateRequest licSuspensionDocument;
    private LocateRequest restart;
    private LocateRequest received;
    private DateWorkArea start;
    private DateWorkArea end;
    private Common locateRecordCount;
    private Common errorsWritten;
    private Common errorOpened;
    private Common abendErrorFound;
    private Common errorNumberOfLines;
    private EabFileHandling reportHandlingEabFileHandling;
    private Common readSinceLastCommit;
    private Common recordsProcessed;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private External hardcodeOpen;
    private External hardcodeRead;
    private External hardcodePosition;
    private EabReportSend reportHandlingEabReportSend;
    private Array<AaaGroupLocalErrorsGroup> aaaGroupLocalErrors;
    private AbendData closeAbendData;
    private CsePersonsWorkSet closeCsePersonsWorkSet;
    private CsePersonsWorkSet closeReturned;
    private Common parameterTextLength;
    private Common parameterStringLength;
    private Common processLicSuspOnly;
    private Common processAllLocateRequest;
  }
#endregion
}
