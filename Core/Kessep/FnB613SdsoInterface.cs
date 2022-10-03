// Program: FN_B613_SDSO_INTERFACE, ID: 372427133, model: 746.
// Short name: SWEF613B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B613_SDSO_INTERFACE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB613SdsoInterface: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B613_SDSO_INTERFACE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB613SdsoInterface(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB613SdsoInterface.
  /// </summary>
  public FnB613SdsoInterface(IContext context, Import import, Export export):
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
    // *****************************************************************
    // Date      Developers Name         Request #  Description
    // --------  ----------------------  ---------  --------------------
    // 09/24/96  Holly Kennedy - MTW                Initial-Cloned for Court 
    // Interface.
    // 12/18/98  Paul Phinney                        Replace 
    // CREATE_PROGRAM_ERROR
    // 
    // with CAB_ERROR_REPORT        Replace REATE_PROGRAM_CONTROL with
    // CAB_CONTROL_REPORT
    // And other
    // modifications required to meet NEW DIR standards for reading and writing
    // Control Totals and ERRORS.
    // 10/13/99  Paul Phinney  H00075931        Add A/B to prevent Duplicate 
    // input files from being used.
    // *****************************************************************
    // -------------------------------------------------------------------------------------------
    // 6/18/2009  J Harden  CQ 7810  added si_close_adabas to the end of the 
    // program
    // -------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Save.Date = Now().Date;
    local.HardcodeOpen.FileInstruction = "OPEN";
    local.HardcodeRead.FileInstruction = "READ";
    local.HardcodePosition.FileInstruction = "POSITION";
    local.FirstPass.Flag = "Y";

    // *****
    // Get the run parameters for this program.
    // *****
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.FindDuplicateDaysCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 1, 3));
    }
    else
    {
      // *****
      // Roll back all updates.
      // *****
      UseEabRollbackSql2();

      // *****
      // Write out an error/warning message and EXIT program.
      // *****
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();

      // *****
      // Open the ERROR Report
      // *****
      local.ReportHandlingEabReportSend.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      local.ReportProcessing.Action = "OPEN";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport3();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

        return;
      }

      // *****
      // Write the ERROR Report
      // *****
      local.ReportHandlingEabReportSend.RptDetail = "ES = " + local
        .ExitStateWorkArea.Message;
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // *****
      // Close the ERROR Report
      // *****
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      local.ReportProcessing.Action = "CLOSE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport3();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

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
      // *****  Reset the checkpoint restart commit count to zero.
      local.ProgramCheckpointRestart.CheckpointCount = 0;
    }
    else
    {
      // *****
      // Roll back all updates.
      // *****
      UseEabRollbackSql2();

      // *****
      // Write out an error/warning message and EXIT program.
      // *****
      // *****
      // Open the ERROR Report
      // *****
      local.ReportProcessing.Action = "OPEN";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      local.ReportHandlingEabReportSend.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      UseCabErrorReport3();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

        return;
      }

      // *****
      // Write the ERROR Report
      // *****
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.ReportHandlingEabReportSend.RptDetail = "ES = " + local
        .ExitStateWorkArea.Message;
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // *****
      // Close the ERROR Report
      // *****
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      local.ReportProcessing.Action = "CLOSE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport3();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****
    // Validate the input file.
    // *****
    UseFnAbValidateSdsoFile();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();

      if (IsExitState("ACO_RE0000_INPUT_FILE_EMPTY_RB"))
      {
        // *****
        // The INPUT File is EMPTY.
        // *****
        // *****
        // OPEN the CONTROL Report
        // *****
        local.ReportProcessing.Action = "OPEN";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        local.ReportHandlingEabReportSend.ProcessDate = local.Save.Date;
        UseCabControlReport3();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_OPENING_CTL_RPT_FILE";

          return;
        }

        // *****
        // WRITE the CONTROL Report with EMPTY_INPUT Message.
        // *****
        local.ReportHandlingEabReportSend.RptDetail = "ES = " + local
          .ExitStateWorkArea.Message;
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabControlReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

          return;
        }

        // *****
        // Close the CONTROL Report
        // *****
        local.ReportHandlingEabReportSend.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabControlReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

          return;
        }

        local.ReportProcessing.Action = "CLOSE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabControlReport3();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

          return;
        }

        return;
      }
      else
      {
        // *****
        // For ANY ERROR - Except EMPTY INPUT FILE.
        // *****
        // *****
        // Roll back all updates.
        // *****
        UseEabRollbackSql2();

        // *****
        // Open the ERROR Report
        // *****
        local.ReportProcessing.Action = "OPEN";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        local.ReportHandlingEabReportSend.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        UseCabErrorReport3();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

          return;
        }

        // *****
        // Write the ERROR Report
        // *****
        local.ReportHandlingEabReportSend.RptDetail = "ES = " + local
          .ExitStateWorkArea.Message;
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        // *****
        // Close the ERROR Report
        // *****
        local.ReportHandlingEabReportSend.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        local.ReportProcessing.Action = "CLOSE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabErrorReport3();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // This should never happen - This program should restart from the 
      // begining each time. (ie. complete ROLLBACK on Error).
      // *****
      // Call external to position the driver file.
      // *****
      local.PassArea.FileInstruction = local.HardcodePosition.FileInstruction;
      UseEabSdsoInterfaceDrvr2();

      if (!Equal(local.PassArea.TextReturnCode, "00"))
      {
        ExitState = "FILE_POSITION_ERROR_WITH_RB";

        // *****
        // Write out an error/warning message and EXIT program.
        // *****
        // *****
        // Roll back all updates.
        // *****
        UseEabRollbackSql2();

        // *****
        // Open the ERROR Report
        // *****
        local.ReportProcessing.Action = "OPEN";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        local.ReportHandlingEabReportSend.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        UseCabErrorReport3();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

          return;
        }

        // *****
        // Write the ERROR Report
        // *****
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        local.ReportHandlingEabReportSend.RptDetail = "ES = " + local
          .ExitStateWorkArea.Message;
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        // *****
        // Close the ERROR Report
        // *****
        local.ReportHandlingEabReportSend.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        local.ReportProcessing.Action = "CLOSE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabErrorReport3();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }
    else
    {
      // *****
      // Call external to open the driver file.
      // *****
      local.PassArea.FileInstruction = local.HardcodeOpen.FileInstruction;
      UseEabSdsoInterfaceDrvr2();

      if (!Equal(local.PassArea.TextReturnCode, "00"))
      {
        ExitState = "ZD_FILE_OPEN_ERROR_WITH_AB";

        // *****
        // Write out an error/warning message and EXIT program.
        // *****
        // *****
        // Roll back all updates.
        // *****
        UseEabRollbackSql2();

        // *****
        // Open the ERROR Report
        // *****
        local.ReportProcessing.Action = "OPEN";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        local.ReportHandlingEabReportSend.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        UseCabErrorReport3();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

          return;
        }

        // *****
        // Write the ERROR Report
        // *****
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        local.ReportHandlingEabReportSend.RptDetail = "ES = " + local
          .ExitStateWorkArea.Message;
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        // *****
        // Close the ERROR Report
        // *****
        local.ReportHandlingEabReportSend.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        local.ReportProcessing.Action = "CLOSE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabErrorReport3();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.HardcodedInterfund.SystemGeneratedIdentifier = 10;
    local.Interfund.SystemGeneratedIdentifier =
      local.HardcodedInterfund.SystemGeneratedIdentifier;
    local.Error.Flag = "N";

    // *****
    // Process driver records until we have reached the end of file.
    // *****
    do
    {
      global.Command = "";

      // ***** Call external to read the driver file.
      local.PassArea.FileInstruction = local.HardcodeRead.FileInstruction;
      UseEabSdsoInterfaceDrvr1();

      switch(TrimEnd(local.PassArea.TextReturnCode))
      {
        case "00":
          ++local.Common.Count;

          break;
        case "EF":
          // *****  End Of File.  The EAB closed the file.  Complete processing.
          goto AfterCycle;
        default:
          ExitState = "FILE_READ_ERROR_WITH_RB";

          // *****
          // Write out an error/warning message and EXIT program.
          // *****
          // *****
          // Roll back all updates.
          // *****
          UseEabRollbackSql2();

          // *****
          // Open the ERROR Report
          // *****
          if (AsChar(local.Error.Flag) == 'N')
          {
            local.ReportProcessing.Action = "OPEN";
            local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
            local.ReportHandlingEabReportSend.ProcessDate =
              local.HeaderRecord.HeaderDetail.SourceCreationDate;
            UseCabErrorReport3();

            if (Equal(local.ReportProcessing.Status, "OK"))
            {
            }
            else
            {
              ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

              return;
            }
          }

          // *****
          // Write the ERROR Report
          // *****
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.ReportHandlingEabReportSend.RptDetail = "ES = " + local
            .ExitStateWorkArea.Message;
          local.ReportProcessing.Action = "WRITE";
          local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }

          // *****
          // Close the ERROR Report
          // *****
          local.ReportHandlingEabReportSend.RptDetail = "";
          local.ReportProcessing.Action = "WRITE";
          local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }

          local.ReportProcessing.Action = "CLOSE";
          local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
          UseCabErrorReport3();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
      }

      // *****
      // Re-Initialize the error group view.  Set all of the attributes to 
      // spaces in case the new error does not overwrite the previous data.
      // Execute a "Repeat Targeting" to reset the cardinality to zero.
      // *****
      for(local.AaaGroupLocalErrors.Index = 0; local
        .AaaGroupLocalErrors.Index < local.AaaGroupLocalErrors.Count; ++
        local.AaaGroupLocalErrors.Index)
      {
        if (!local.AaaGroupLocalErrors.CheckSize())
        {
          break;
        }

        MoveProgramError(local.Initialized,
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError);
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
          Command = "";
      }

      local.AaaGroupLocalErrors.CheckIndex();
      local.AaaGroupLocalErrors.Index = -1;

      // *****
      // A Restart situation should never occur.  The program should run 
      // directly through from start to finish.  If an error should occur, a
      // complete rollback will occur.
      // *****
      switch(local.RecordType.Count)
      {
        case 1:
          local.SourceCreation.TextDate =
            NumberToString(DateToInt(
              local.HeaderRecord.HeaderDetail.SourceCreationDate), 8);

          local.AaaGroupLocalErrors.Index = 0;
          local.AaaGroupLocalErrors.CheckSize();

          // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
          // * * * *
          // 01/30/1999    SWSRPDP  Verify that DATE in Record 1
          // is a Valid Date.
          // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
          // * * * *
          // VERIFY MONTH
          if (Lt(Substring(
            local.SourceCreation.TextDate, DateWorkArea.TextDate_MaxLength, 5,
            2), "01") || Lt
            ("12", Substring(local.SourceCreation.TextDate,
            DateWorkArea.TextDate_MaxLength, 5, 2)))
          {
            local.AaaGroupLocalErrors.Index = 0;
            local.AaaGroupLocalErrors.CheckSize();

            local.InvalidDate.Flag = "Y";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                "Invalid Date in Record 1 = " + local.SourceCreation.TextDate;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Month = " +
              Substring
              (local.SourceCreation.TextDate, DateWorkArea.TextDate_MaxLength,
              5, 2);
          }

          // VERIFY DAY
          if (Lt(Substring(
            local.SourceCreation.TextDate, DateWorkArea.TextDate_MaxLength, 7,
            2), "01"))
          {
            if (AsChar(local.InvalidDate.Flag) != 'Y')
            {
              local.AaaGroupLocalErrors.Index = 0;
              local.AaaGroupLocalErrors.CheckSize();

              local.InvalidDate.Flag = "Y";
              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                  "Invalid Date in Record 1 = " + local
                .SourceCreation.TextDate;
            }

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Day = " + Substring
              (local.SourceCreation.TextDate, DateWorkArea.TextDate_MaxLength,
              7, 2);
          }
          else
          {
            switch(TrimEnd(Substring(local.SourceCreation.TextDate, 5, 2)))
            {
              case "09":
                if (Lt("30", Substring(local.SourceCreation.TextDate,
                  DateWorkArea.TextDate_MaxLength, 7, 2)))
                {
                  if (AsChar(local.InvalidDate.Flag) != 'Y')
                  {
                    local.AaaGroupLocalErrors.Index = 0;
                    local.AaaGroupLocalErrors.CheckSize();

                    local.InvalidDate.Flag = "Y";
                    local.AaaGroupLocalErrors.Update.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                        "Invalid Date in Record 1 = " + local
                      .SourceCreation.TextDate;
                  }

                  local.AaaGroupLocalErrors.Update.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                      TrimEnd(local.AaaGroupLocalErrors.Item.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Day = " +
                    Substring
                    (local.SourceCreation.TextDate,
                    DateWorkArea.TextDate_MaxLength, 7, 2);
                }

                break;
              case "04":
                if (Lt("30", Substring(local.SourceCreation.TextDate,
                  DateWorkArea.TextDate_MaxLength, 7, 2)))
                {
                  if (AsChar(local.InvalidDate.Flag) != 'Y')
                  {
                    local.AaaGroupLocalErrors.Index = 0;
                    local.AaaGroupLocalErrors.CheckSize();

                    local.InvalidDate.Flag = "Y";
                    local.AaaGroupLocalErrors.Update.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                        "Invalid Date in Record 1 = " + local
                      .SourceCreation.TextDate;
                  }

                  local.AaaGroupLocalErrors.Update.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                      TrimEnd(local.AaaGroupLocalErrors.Item.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Day = " +
                    Substring
                    (local.SourceCreation.TextDate,
                    DateWorkArea.TextDate_MaxLength, 7, 2);
                }

                break;
              case "06":
                if (Lt("30", Substring(local.SourceCreation.TextDate,
                  DateWorkArea.TextDate_MaxLength, 7, 2)))
                {
                  if (AsChar(local.InvalidDate.Flag) != 'Y')
                  {
                    local.AaaGroupLocalErrors.Index = 0;
                    local.AaaGroupLocalErrors.CheckSize();

                    local.InvalidDate.Flag = "Y";
                    local.AaaGroupLocalErrors.Update.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                        "Invalid Date in Record 1 = " + local
                      .SourceCreation.TextDate;
                  }

                  local.AaaGroupLocalErrors.Update.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                      TrimEnd(local.AaaGroupLocalErrors.Item.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Day = " +
                    Substring
                    (local.SourceCreation.TextDate,
                    DateWorkArea.TextDate_MaxLength, 7, 2);
                }

                break;
              case "11":
                if (Lt("30", Substring(local.SourceCreation.TextDate,
                  DateWorkArea.TextDate_MaxLength, 7, 2)))
                {
                  if (AsChar(local.InvalidDate.Flag) != 'Y')
                  {
                    local.AaaGroupLocalErrors.Index = 0;
                    local.AaaGroupLocalErrors.CheckSize();

                    local.InvalidDate.Flag = "Y";
                    local.AaaGroupLocalErrors.Update.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                        "Invalid Date in Record 1 = " + local
                      .SourceCreation.TextDate;
                  }

                  local.AaaGroupLocalErrors.Update.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                      TrimEnd(local.AaaGroupLocalErrors.Item.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Day = " +
                    Substring
                    (local.SourceCreation.TextDate,
                    DateWorkArea.TextDate_MaxLength, 7, 2);
                }

                break;
              case "02":
                if (Lt("29", Substring(local.SourceCreation.TextDate,
                  DateWorkArea.TextDate_MaxLength, 7, 2)))
                {
                  if (AsChar(local.InvalidDate.Flag) != 'Y')
                  {
                    local.AaaGroupLocalErrors.Index = 0;
                    local.AaaGroupLocalErrors.CheckSize();

                    local.InvalidDate.Flag = "Y";
                    local.AaaGroupLocalErrors.Update.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                        "Invalid Date in Record 1 = " + local
                      .SourceCreation.TextDate;
                  }

                  local.AaaGroupLocalErrors.Update.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                      TrimEnd(local.AaaGroupLocalErrors.Item.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Day = " +
                    Substring
                    (local.SourceCreation.TextDate,
                    DateWorkArea.TextDate_MaxLength, 7, 2);
                }

                break;
              default:
                if (Lt("31", Substring(local.SourceCreation.TextDate,
                  DateWorkArea.TextDate_MaxLength, 7, 2)))
                {
                  if (AsChar(local.InvalidDate.Flag) != 'Y')
                  {
                    local.AaaGroupLocalErrors.Index = 0;
                    local.AaaGroupLocalErrors.CheckSize();

                    local.InvalidDate.Flag = "Y";
                    local.AaaGroupLocalErrors.Update.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                        "Invalid Date in Record 1 = " + local
                      .SourceCreation.TextDate;
                  }

                  local.AaaGroupLocalErrors.Update.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                      TrimEnd(local.AaaGroupLocalErrors.Item.
                      AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Day = " +
                    Substring
                    (local.SourceCreation.TextDate,
                    DateWorkArea.TextDate_MaxLength, 7, 2);
                }

                break;
            }
          }

          // VERIFY CENTURY
          switch(TrimEnd(Substring(local.SourceCreation.TextDate, 1, 2)))
          {
            case "19":
              // VERIFY YEAR
              if (!Equal(local.SourceCreation.TextDate, 3, 2, "99"))
              {
                if (AsChar(local.InvalidDate.Flag) != 'Y')
                {
                  local.AaaGroupLocalErrors.Index = 0;
                  local.AaaGroupLocalErrors.CheckSize();

                  local.InvalidDate.Flag = "Y";
                  local.AaaGroupLocalErrors.Update.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                      "Invalid Date in Record 1 = " + local
                    .SourceCreation.TextDate;
                }

                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    TrimEnd(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Year =  " +
                  Substring
                  (local.SourceCreation.TextDate,
                  DateWorkArea.TextDate_MaxLength, 3, 2);
              }

              break;
            case "20":
              // VERIFY YEAR
              if (Lt(Substring(
                local.SourceCreation.TextDate, DateWorkArea.TextDate_MaxLength,
                3, 2), "00") || Lt
                ("99", Substring(local.SourceCreation.TextDate,
                DateWorkArea.TextDate_MaxLength, 3, 2)))
              {
                if (AsChar(local.InvalidDate.Flag) != 'Y')
                {
                  local.AaaGroupLocalErrors.Index = 0;
                  local.AaaGroupLocalErrors.CheckSize();

                  local.InvalidDate.Flag = "Y";
                  local.AaaGroupLocalErrors.Update.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                      "Invalid Date in Record 1 = " + local
                    .SourceCreation.TextDate;
                }

                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    TrimEnd(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Year =  " +
                  Substring
                  (local.SourceCreation.TextDate,
                  DateWorkArea.TextDate_MaxLength, 3, 2);
              }

              break;
            default:
              if (AsChar(local.InvalidDate.Flag) != 'Y')
              {
                local.AaaGroupLocalErrors.Index = 0;
                local.AaaGroupLocalErrors.CheckSize();

                local.InvalidDate.Flag = "Y";
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    "Invalid Date in Record 1 = " + local
                  .SourceCreation.TextDate;
              }

              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                  TrimEnd(local.AaaGroupLocalErrors.Item.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Century = " +
                Substring
                (local.SourceCreation.TextDate, DateWorkArea.TextDate_MaxLength,
                1, 2);

              if (Lt(Substring(
                local.SourceCreation.TextDate, DateWorkArea.TextDate_MaxLength,
                3, 2), "00") || Lt
                ("99", Substring(local.SourceCreation.TextDate,
                DateWorkArea.TextDate_MaxLength, 3, 2)))
              {
                local.AaaGroupLocalErrors.Update.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                    TrimEnd(local.AaaGroupLocalErrors.Item.
                    AaaGroupLocalErrorsDetailProgramError.KeyInfo) + ", Year =  " +
                  Substring
                  (local.SourceCreation.TextDate,
                  DateWorkArea.TextDate_MaxLength, 3, 2);
              }

              break;
          }

          if (AsChar(local.InvalidDate.Flag) == 'Y')
          {
            local.PassArea.TextReturnCode = "EF";

            break;
          }

          // *****
          // Set the Source Creation text date.  This date will be used to write
          // out any errors found while processing the high level entities.
          // *****
          local.ReportHandlingEabReportSend.ProcessDate =
            local.HeaderRecord.HeaderDetail.SourceCreationDate;
          local.SourceCreation.TextDate =
            NumberToString(DateToInt(
              local.HeaderRecord.HeaderDetail.SourceCreationDate), 8);
          local.SetCashReceiptEvent.SourceCreationDate =
            local.HeaderRecord.HeaderDetail.SourceCreationDate;

          break;
        case 2:
          // *****
          // Process the Cash Receipt Event and Cash Receipt at the first detail
          // record for each Cash Receipt Type.
          // *****
          switch(AsChar(local.DetailRecord.DetailDtlCollType.SelectChar))
          {
            case 'S':
              // 12/18/98  PDP
              local.Type2S.TotalAdjustmentCount =
                local.Type2S.TotalAdjustmentCount.GetValueOrDefault() + 1;
              local.Type2S.TotalNonCashFeeAmt =
                local.Type2S.TotalNonCashFeeAmt.GetValueOrDefault() + local
                .DetailRecord.DetailDetail.CollectionAmount;
              local.TotalAccumulator.TotalAdjustmentCount =
                local.TotalAccumulator.TotalAdjustmentCount.
                  GetValueOrDefault() + 1;
              local.TotalAccumulator.TotalNonCashFeeAmt =
                local.TotalAccumulator.TotalNonCashFeeAmt.GetValueOrDefault() +
                local.DetailRecord.DetailDetail.CollectionAmount;
              local.SetCollectionType.SequentialIdentifier = 4;

              break;
            case 'K':
              // 12/18/98  PDP
              local.Type2K.TotalAdjustmentCount =
                local.Type2K.TotalAdjustmentCount.GetValueOrDefault() + 1;
              local.Type2K.TotalNonCashFeeAmt =
                local.Type2K.TotalNonCashFeeAmt.GetValueOrDefault() + local
                .DetailRecord.DetailDetail.CollectionAmount;
              local.TotalAccumulator.TotalAdjustmentCount =
                local.TotalAccumulator.TotalAdjustmentCount.
                  GetValueOrDefault() + 1;
              local.TotalAccumulator.TotalNonCashFeeAmt =
                local.TotalAccumulator.TotalNonCashFeeAmt.GetValueOrDefault() +
                local.DetailRecord.DetailDetail.CollectionAmount;
              local.SetCollectionType.SequentialIdentifier = 10;

              break;
            case 'U':
              // 12/18/98  PDP
              local.Type2U.TotalAdjustmentCount =
                local.Type2U.TotalAdjustmentCount.GetValueOrDefault() + 1;
              local.Type2U.TotalNonCashFeeAmt =
                local.Type2U.TotalNonCashFeeAmt.GetValueOrDefault() + local
                .DetailRecord.DetailDetail.CollectionAmount;
              local.TotalAccumulator.TotalAdjustmentCount =
                local.TotalAccumulator.TotalAdjustmentCount.
                  GetValueOrDefault() + 1;
              local.TotalAccumulator.TotalNonCashFeeAmt =
                local.TotalAccumulator.TotalNonCashFeeAmt.GetValueOrDefault() +
                local.DetailRecord.DetailDetail.CollectionAmount;
              local.SetCollectionType.SequentialIdentifier = 5;

              break;
            case 'R':
              // 12/18/98  PDP
              local.Type2R.TotalAdjustmentCount =
                local.Type2R.TotalAdjustmentCount.GetValueOrDefault() + 1;
              local.Type2R.TotalNonCashFeeAmt =
                local.Type2R.TotalNonCashFeeAmt.GetValueOrDefault() + local
                .DetailRecord.DetailDetail.CollectionAmount;
              local.TotalAccumulator.TotalAdjustmentCount =
                local.TotalAccumulator.TotalAdjustmentCount.
                  GetValueOrDefault() + 1;
              local.TotalAccumulator.TotalNonCashFeeAmt =
                local.TotalAccumulator.TotalNonCashFeeAmt.GetValueOrDefault() +
                local.DetailRecord.DetailDetail.CollectionAmount;
              local.SetCollectionType.SequentialIdentifier = 11;

              break;
            default:
              // 12/18/98  PDP
              local.Type2BadCollection.TotalAdjustmentCount =
                local.Type2BadCollection.TotalAdjustmentCount.
                  GetValueOrDefault() + 1;
              local.Type2BadCollection.TotalNonCashFeeAmt =
                local.Type2BadCollection.TotalNonCashFeeAmt.
                  GetValueOrDefault() + local
                .DetailRecord.DetailDetail.CollectionAmount;
              local.TotalAccumulator.TotalAdjustmentCount =
                local.TotalAccumulator.TotalAdjustmentCount.
                  GetValueOrDefault() + 1;
              local.TotalAccumulator.TotalNonCashFeeAmt =
                local.TotalAccumulator.TotalNonCashFeeAmt.GetValueOrDefault() +
                local.DetailRecord.DetailDetail.CollectionAmount;
              local.SetCollectionType.SequentialIdentifier = 0;

              break;
          }

          local.SetCollectionType.Code =
            local.DetailRecord.DetailDtlCollType.SelectChar;

          if (local.SdsoCashReceipt.SequentialNumber == 0)
          {
            // *****
            // If the record is the first record add header data
            // *****
            UseFnAddFdsoSdsoHeaderInfo();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }
          }

          // *****
          // Read and establish currency on the Cash Receipts.  This will allow 
          // matching to persistent views in called CABs.
          // *****
          if (ReadCashReceipt())
          {
            // *****
            // Process detail record.
            // *****
            UseFnProcessSdsoCollDtlRecord();
          }
          else
          {
            ExitState = "FN0084_CASH_RCPT_NF";

            goto AfterCycle;
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            // *****
            // If any type of error occurs (critical or non-critical), rollback 
            // all to that record changes and write out an error record.
            // *****
            if (IsExitState("INTERFACE_ALREADY_PROCESSED_RB"))
            {
              ++local.AaaGroupLocalErrors.Index;
              local.AaaGroupLocalErrors.CheckSize();

              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo = "Date = " + local
                .SourceCreation.TextDate;

              // *****
              // Set the Error Message Text to the exit state description.
              // *****
              local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();

              // ????????????????????????????????????????????????????????????
              // If PROGRAM_ERROR MESSAGE_TEXT is added to the data model, 
              // change the following statement to: "SET local program_error
              // message_text TO local exit_state_work_area message"
              // ????????????????????????????????????????????????????????????
              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                  TrimEnd(local.AaaGroupLocalErrors.Item.
                  AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " ES=" + local
                .ExitStateWorkArea.Message;
              ExitState = "ACO_NN0000_ALL_OK";

              // *****
              // Set the termination action.
              // *****
              local.PassArea.TextReturnCode = "EF";
              local.AaaGroupLocalErrors.Update.
                AaaGroupLocalErrorsDetailStandard.Command = "ROLLBACK";
              local.InterfaceErrorSwitch.Flag = "Y";

              goto AfterCycle;
            }
            else
            {
            }

            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            // *****
            // Set the Key Info with the CSE Person Number, Amount, and the 
            // collection type.
            // *****
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                "CSE PERSON #=" + (
                local.DetailRecord.DetailDetail.ObligorPersonNumber ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " AMOUNT=" + NumberToString
              ((long)(local.DetailRecord.DetailDetail.CollectionAmount * 100),
              15);
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " COLL TYPE=" +
              local.DetailRecord.DetailDtlCollType.SelectChar;

            // *****
            // Set the Error Message Text to the exit state description.
            // *****
            local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();

            // ????????????????????????????????????????????????????????????
            // If PROGRAM_ERROR MESSAGE_TEXT is added to the data model, change 
            // the following statement to: "SET local program_error message_text
            // TO local exit_state_work_area message"
            // ????????????????????????????????????????????????????????????
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " ES=" + local
              .ExitStateWorkArea.Message;
            ExitState = "ACO_NN0000_ALL_OK";

            // *****
            // Set the termination action.
            // *****
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "ROLLBACK";
          }

          break;
        case 3:
          // *****
          // Totals to be validated in a pre processing batch program
          // *****
          // *****
          // 
          // 12/18/98  PDP --  Added accumulators to VERIFY Totals
          // Processed in this Program.
          // *****
          local.Type3Total.TotalAdjustmentCount =
            local.TotalRecord.TotalDetail.TotalNonCashTransactionCount.
              GetValueOrDefault();
          local.Type3Total.TotalNonCashFeeAmt =
            local.TotalRecord.TotalDetail.TotalNonCashAmt.GetValueOrDefault();

          break;
        default:
          // 12/18/98  PDP
          // *****
          // If "TYPE" error occurs rollback all changes
          // and write out an error record.
          // *****
          // *****
          // This SHOULD NEVER happen due to File Verification!!!
          // *****
          ExitState = "ACO_RE0000_INPUT_RECORD_TYPE_INV";

          ++local.AaaGroupLocalErrors.Index;
          local.AaaGroupLocalErrors.CheckSize();

          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo = "RECORD TYPE = " + NumberToString
            (local.RecordType.Count, 15);

          // *****
          // Set the Error Message Text to the exit state description.
          // *****
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();

          // ????????????????????????????????????????????????????????????
          // If PROGRAM_ERROR MESSAGE_TEXT is added to the data model, change 
          // the following statement to: "SET local program_error message_text
          // TO local exit_state_work_area message"
          // ????????????????????????????????????????????????????????????
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              TrimEnd(local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " ES=" + local
            .ExitStateWorkArea.Message;
          ExitState = "ACO_NN0000_ALL_OK";

          // *****
          // Set the termination action.
          // *****
          local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
            Command = "ROLLBACK";

          break;
      }

      // *****
      // Process errors.
      // *****
      local.AaaGroupLocalErrors.Index = 0;

      for(var limit = local.AaaGroupLocalErrors.Count; local
        .AaaGroupLocalErrors.Index < limit; ++local.AaaGroupLocalErrors.Index)
      {
        if (!local.AaaGroupLocalErrors.CheckSize())
        {
          break;
        }

        if (IsEmpty(local.AaaGroupLocalErrors.Item.
          AaaGroupLocalErrorsDetailProgramError.KeyInfo) && IsEmpty
          (local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            Code))
        {
          continue;
        }

        // *****
        // Write out an error/warning message.
        // *****
        if (AsChar(local.Error.Flag) == 'N')
        {
          local.ReportProcessing.Action = "OPEN";
          local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
          local.ReportHandlingEabReportSend.ProcessDate =
            local.SetCashReceiptEvent.SourceCreationDate;
          UseCabErrorReport3();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

            return;
          }

          local.Error.Flag = "Y";
        }

        // 12/18/98  PDP
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        local.ReportHandlingEabReportSend.RptDetail =
          local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            KeyInfo ?? Spaces(132);
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }
      }

      local.AaaGroupLocalErrors.CheckIndex();
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

AfterCycle:

    // *****
    // The external hit the end of the driver file,
    // closed the file and returned an EF (EOF) indicator.
    // *****
    // 12/18/98  PDP
    // *****
    // all LOGIC from this point is for processing the Batch_Reports
    // 
    // and closing Files.  Changed to match DIR Standards.
    // *****
    // *****
    // OPEN the CONTROL Report
    // *****
    local.ReportProcessing.Action = "OPEN";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabReportSend.ProcessDate =
      local.SetCashReceiptEvent.SourceCreationDate;
    UseCabControlReport3();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_OPENING_CTL_RPT_FILE";

      return;
    }

    // *****
    // 
    // HEADER Information.
    // *****
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // *****
    // 
    // HEADER Information --  DATE.
    // *****
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabReportSend.RptDetail = "Source Creation Date = " + local
      .SourceCreation.TextDate;
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // *****
    // 
    // HEADER Information  --   Spacing.
    // *****
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // *****
    // 
    // HEADER Information.
    // *****
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabReportSend.RptDetail =
      " Count of SDSO Records Processed:";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // *****
    // 
    // HEADER Information  --  Spacing.
    // *****
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Records processed Header
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Count of SDSO Records Processed:";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Number of SDSO Records read................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 43) + NumberToString
      (local.TotalAccumulator.TotalAdjustmentCount.GetValueOrDefault(), 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // *****
    // Totals for Record Type 2 (Detail) Collection Type "R"ecovery.
    // *****
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Record Type R........................" + NumberToString
      (local.Type2R.TotalAdjustmentCount.GetValueOrDefault(), 15);
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // *****
    // Totals for Record Type 2 (Detail) Collection Type "S"DSO.
    // *****
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Record Type S........................" + NumberToString
      (local.Type2S.TotalAdjustmentCount.GetValueOrDefault(), 15);
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // *****
    // Totals for Record Type 2 (Detail) Collection Type "U"I.
    // *****
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Record Type U........................" + NumberToString
      (local.Type2U.TotalAdjustmentCount.GetValueOrDefault(), 15);
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // *****
    // Totals for Record Type 2 (Detail) Collection Type "K"PERS.
    // *****
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Record Type K........................" + NumberToString
      (local.Type2K.TotalAdjustmentCount.GetValueOrDefault(), 15);
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // *****
    // Totals for Record Type 2 (Detail) Collection Type Invalid.
    // *****
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Record Type Invalid.................." + NumberToString
      (local.Type2BadCollection.TotalAdjustmentCount.GetValueOrDefault(), 15);
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // *****
    // Totals from Type 3 Control Record.
    // *****
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabReportSend.RptDetail =
      "Totals From Record3........................" + NumberToString
      (local.Type3Total.TotalAdjustmentCount.GetValueOrDefault(), 15);
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

      return;
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Records processed Amount Header
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of SDSO Records Processed:";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total of ALL SDSO records Read
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of ALL SDSO Records read............";

    // HERE SWSRPDP
    local.TotalOfAll.CollectionAmount =
      local.TotalAccumulator.TotalNonCashFeeAmt.GetValueOrDefault();
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + NumberToString
      ((long)local.TotalOfAll.CollectionAmount, 15) + "." + NumberToString
      ((long)(local.TotalOfAll.CollectionAmount * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 52, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Amount of Type = 'R'
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of Type \"R\" SDSO Records read............";

    // HERE SWSRPDP
    local.TotalOfAll.CollectionAmount =
      local.Type2R.TotalNonCashFeeAmt.GetValueOrDefault();
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + NumberToString
      ((long)local.TotalOfAll.CollectionAmount, 15) + "." + NumberToString
      ((long)(local.TotalOfAll.CollectionAmount * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 52, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Amount of Type = 'S'
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of Type \"S\" SDSO Records read............";

    // HERE SWSRPDP
    local.TotalOfAll.CollectionAmount =
      local.Type2S.TotalNonCashFeeAmt.GetValueOrDefault();
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + NumberToString
      ((long)local.TotalOfAll.CollectionAmount, 15) + "." + NumberToString
      ((long)(local.TotalOfAll.CollectionAmount * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 52, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Amount of Type = 'U'
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of Type \"U\" SDSO Records read............";

    // HERE SWSRPDP
    local.TotalOfAll.CollectionAmount =
      local.Type2U.TotalNonCashFeeAmt.GetValueOrDefault();
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + NumberToString
      ((long)local.TotalOfAll.CollectionAmount, 15) + "." + NumberToString
      ((long)(local.TotalOfAll.CollectionAmount * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 52, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Amount of Type = 'K'
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of Type \"K\" SDSO Records read............";

    // HERE SWSRPDP
    local.TotalOfAll.CollectionAmount =
      local.Type2K.TotalNonCashFeeAmt.GetValueOrDefault();
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + NumberToString
      ((long)local.TotalOfAll.CollectionAmount, 15) + "." + NumberToString
      ((long)(local.TotalOfAll.CollectionAmount * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 52, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Amount of Type is Invalid
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of Invalid \"Type\" SDSO Records...........";

    // HERE SWSRPDP
    local.TotalOfAll.CollectionAmount =
      local.Type2BadCollection.TotalNonCashFeeAmt.GetValueOrDefault();
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + NumberToString
      ((long)local.TotalOfAll.CollectionAmount, 15) + "." + NumberToString
      ((long)(local.TotalOfAll.CollectionAmount * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 52, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Amount on Record 3
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount on Record 3..............................";

    // HERE SWSRPDP
    local.TotalOfAll.CollectionAmount =
      local.Type3Total.TotalNonCashFeeAmt.GetValueOrDefault();
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + NumberToString
      ((long)local.TotalOfAll.CollectionAmount, 15) + "." + NumberToString
      ((long)(local.TotalOfAll.CollectionAmount * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 52, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // The Following Control Lines for CR_Detail type is copied from FDSO Batch 
    // Interface
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Records processed Header
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Count of SDSO Records Recorded as Collections:";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Count of ALL Records Recorded
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Number of SDSO Records Recorded................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 35) + NumberToString
      (local.RecCollections.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Count of Releassed
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Number of SDSO Records Released...................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 35) + NumberToString
      (local.RelCollections.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Count of Suspended
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Number of SDSO Records Suspended.................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 35) + NumberToString
      (local.SusCollections.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Count of Refunded
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ControlTotals.Index = 0;
    local.ControlTotals.CheckSize();

    local.ReportHandlingEabReportSend.RptDetail =
      "Number of SDSO Records Refunded.................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 35) + NumberToString
      (local.RefCollections.Count, 15);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // Pended is NO LONGER USED
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    // Write the Control Report -- Spacing
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Amount of Recorded
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of SDSO Records Recorded...................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + NumberToString
      ((long)local.RecCollectionsAmt.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.RecCollectionsAmt.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 52, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Amount of Releassed
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of SDSO Records Released...................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + NumberToString
      ((long)local.RelCollectionsAmt.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.RelCollectionsAmt.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 52, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Amount of Suspended
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of SDSO Records Suspended..................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + NumberToString
      ((long)local.SusCollectionsAmt.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.SusCollectionsAmt.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 52, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    // Write the Control Report -- Total Amount of Refunded
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // *
    local.ReportHandlingEabReportSend.RptDetail =
      "Total Amount of SDSO Records Refunded....................";
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + NumberToString
      ((long)local.RefCollectionsAmt.TotalCurrency, 15) + "." + NumberToString
      ((long)(local.RefCollectionsAmt.TotalCurrency * 100), 14, 2);
    local.ReportHandlingEabReportSend.RptDetail =
      Substring(local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 45) + Substring
      (local.ReportHandlingEabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 52, 12);
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

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
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    local.ReportHandlingEabFileHandling.Action = "WRITE";
    UseCabControlReport2();

    if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    if (local.Type3Total.TotalAdjustmentCount.GetValueOrDefault() != local
      .TotalAccumulator.TotalAdjustmentCount.GetValueOrDefault() || local
      .Type3Total.TotalNonCashFeeAmt.GetValueOrDefault() != local
      .TotalAccumulator.TotalNonCashFeeAmt.GetValueOrDefault())
    {
      if (AsChar(local.Error.Flag) == 'N')
      {
        local.ReportProcessing.Action = "OPEN";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        local.ReportHandlingEabReportSend.ProcessDate =
          local.SetCashReceiptEvent.SourceCreationDate;
        UseCabErrorReport3();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

          return;
        }

        local.Error.Flag = "Y";
      }

      ExitState = "FN0000_OUT_OF_BALANCE";

      // *****
      // Roll back all updates.
      // *****
      local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
        Command = "ROLLBACK";
      UseEabRollbackSql2();

      if (local.PassArea.NumericReturnCode != 0)
      {
        ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

        // *****
        // Write out an error/warning message and EXIT program.
        // *****
        // *****
        // Roll back all updates.
        // *****
        UseEabRollbackSql2();

        // *****
        // Open the ERROR Report
        // *****
        if (AsChar(local.Error.Flag) == 'N')
        {
          local.ReportProcessing.Action = "OPEN";
          local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
          local.ReportHandlingEabReportSend.ProcessDate =
            local.SetCashReceiptEvent.SourceCreationDate;
          UseCabErrorReport3();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

            return;
          }

          local.Error.Flag = "Y";
        }

        // *****
        // Write the ERROR Report
        // *****
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        local.ReportHandlingEabReportSend.RptDetail = "ES = " + local
          .ExitStateWorkArea.Message;
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        // *****
        // Close the ERROR Report
        // *****
        local.ReportHandlingEabReportSend.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        local.ReportProcessing.Action = "CLOSE";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        UseCabErrorReport3();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // *****
      // Spacing.
      // *****
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      local.ReportHandlingEabReportSend.RptDetail = "";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // *****
      // Totals which have been accumulated.
      // *****
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      local.ReportHandlingEabReportSend.RptDetail = "Total Record Counts = " + NumberToString
        (local.TotalAccumulator.TotalAdjustmentCount.GetValueOrDefault(), 15);
      local.ReportHandlingEabReportSend.RptDetail =
        TrimEnd(local.ReportHandlingEabReportSend.RptDetail) + "      " + NumberToString
        ((long)(local.TotalAccumulator.TotalNonCashFeeAmt.GetValueOrDefault() *
        100), 15);
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // *****
      // Totals from Type 3 Control Record.
      // *****
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      local.ReportHandlingEabReportSend.RptDetail = "Totals From Record3 = " + NumberToString
        (local.Type3Total.TotalAdjustmentCount.GetValueOrDefault(), 15);
      local.ReportHandlingEabReportSend.RptDetail =
        TrimEnd(local.ReportHandlingEabReportSend.RptDetail) + "      " + NumberToString
        ((long)(local.Type3Total.TotalNonCashFeeAmt.GetValueOrDefault() * 100),
        15);
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // *****
      // Set the Error Message Text to the exit state description.
      // *****
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();

      // ????????????????????????????????????????????????????????????
      // If PROGRAM_ERROR MESSAGE_TEXT is added to the data model, change the 
      // following statement to: "SET local program_error message_text TO local
      // exit_state_work_area message"
      // ????????????????????????????????????????????????????????????
      local.ReportHandlingEabReportSend.RptDetail = "ES = " + local
        .ExitStateWorkArea.Message;
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ALL_OK";
    }

    // *****
    // Set restart indicator to no because we successfully finished this 
    // program.
    // *****
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    UseUpdatePgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      // *****
      // Write out an error/warning message and EXIT program.
      // *****
      // *****
      // Roll back all updates.
      // *****
      UseEabRollbackSql2();

      // *****
      // Open the ERROR Report
      // *****
      if (AsChar(local.Error.Flag) == 'N')
      {
        local.ReportProcessing.Action = "OPEN";
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        local.ReportHandlingEabReportSend.ProcessDate =
          local.SetCashReceiptEvent.SourceCreationDate;
        UseCabErrorReport3();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

          return;
        }

        local.Error.Flag = "Y";
      }

      // *****
      // Write the ERROR Report
      // *****
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.ReportHandlingEabReportSend.RptDetail = "ES = " + local
        .ExitStateWorkArea.Message;
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // *****
      // Close the ERROR Report
      // *****
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      local.ReportProcessing.Action = "CLOSE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport3();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // 10/13/99 H00075931 - PDP - Check for Duplicate File
    if (AsChar(local.Error.Flag) != 'Y')
    {
      if (ReadCashReceipt())
      {
        local.FindDuplicate.Assign(entities.CashReceipt);
      }
      else
      {
        goto Test;
      }

      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      // 10/13/99  Paul Phinney  H00075931        Add A/B to prevent Duplicate 
      // input files from being used.
      UseFnFdsoSdsoCheckForDupBatch();

      // ADD A/B HERE
      // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
      if (local.FindDuplicateCount.Count > 0)
      {
        local.Error.Flag = "Y";
        UseEabRollbackSql1();

        // LINE 1
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = "Current Batch File for " + NumberToString
          (DateToInt(local.SetCashReceiptEvent.SourceCreationDate), 12, 2) + "/"
          ;
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + TrimEnd
          (NumberToString(
            DateToInt(local.SetCashReceiptEvent.SourceCreationDate), 14, 2)) + "/"
          ;
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + TrimEnd
          (NumberToString(
            DateToInt(local.SetCashReceiptEvent.SourceCreationDate), 8, 4)) + " matches the counts and amounts from Already Existing Cash Receipt";
          
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Number " + NumberToString
          (local.FindDuplicate.SequentialNumber, 6, 10);

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Open the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        if (AsChar(local.ErrorOpened.Flag) != 'Y')
        {
          local.ReportHandlingEabReportSend.ProcessDate =
            local.ProgramProcessingInfo.ProcessDate;
          local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
          local.ReportHandlingEabFileHandling.Action = "OPEN";
          UseCabErrorReport4();

          if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
          {
          }
          else
          {
            UseEabRollbackSql1();
            ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

            return;
          }

          local.ErrorOpened.Flag = "Y";
        }

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            KeyInfo ?? Spaces(132);
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          UseEabRollbackSql1();
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        // LINE 2
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = " Received on " + NumberToString
          (DateToInt(local.FindDuplicate.ReceivedDate), 12, 1);
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + TrimEnd
          (NumberToString(DateToInt(local.FindDuplicate.ReceivedDate), 13, 1)) +
          "/";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + TrimEnd
          (NumberToString(DateToInt(local.FindDuplicate.ReceivedDate), 14, 2)) +
          "/";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + TrimEnd
          (NumberToString(DateToInt(local.FindDuplicate.ReceivedDate), 8, 4)) +
          " ";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " with a Cash amount of " +
          NumberToString
          ((long)local.FindDuplicate.TotalCashTransactionAmount.
            GetValueOrDefault(), 6, 10);
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "." + NumberToString
          ((long)(local.FindDuplicate.TotalCashTransactionAmount.
            GetValueOrDefault() * 100), 14, 2);

        if (local.FindDuplicate.TotalCashTransactionAmount.
          GetValueOrDefault() < 0)
        {
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              TrimEnd(local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "-";
        }

        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " and a Cash Due amount of " +
          NumberToString
          ((long)local.FindDuplicate.CashDue.GetValueOrDefault(), 6, 10);
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "." + NumberToString
          ((long)(local.FindDuplicate.CashDue.GetValueOrDefault() * 100), 14, 2);
          

        if (local.FindDuplicate.CashDue.GetValueOrDefault() < 0)
        {
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              TrimEnd(local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "-";
        }

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            KeyInfo ?? Spaces(132);
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          UseEabRollbackSql1();
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        // LINE 3
        // LINE 3
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo = "";
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " with " + NumberToString
          (local.FindDuplicate.TotalCashTransactionCount.GetValueOrDefault(), 6,
          10);
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Collections and " +
          NumberToString
          (local.FindDuplicate.TotalDetailAdjustmentCount.GetValueOrDefault(),
          6, 10);
        local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailProgramError.
          KeyInfo =
            TrimEnd(local.AaaGroupLocalErrors.Item.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " " + "Adjustments * * * *  DUPLICATE INPUT FILE --  Program has ABENDED * * * *";
          

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        // Write the ERROR Report
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // * * *
        local.ReportHandlingEabReportSend.RptDetail =
          local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            KeyInfo ?? Spaces(132);
        local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
        local.ReportHandlingEabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        if (Equal(local.ReportHandlingEabFileHandling.Status, "OK"))
        {
        }
        else
        {
          UseEabRollbackSql1();
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }
      }
    }

Test:

    // *****
    // If an ERROR has occured,
    // 
    // Write a Message on the Control Report
    // *****
    if (AsChar(local.Error.Flag) == 'Y')
    {
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabControlReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }

      local.ReportHandlingEabReportSend.RptDetail =
        "*** Batch Run ABENDED - See ERROR Report ***";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabControlReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }
    }

    // *****
    // Close the CONTROL Report
    // *****
    local.ReportHandlingEabReportSend.RptDetail = "";
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    local.ReportProcessing.Action = "CLOSE";
    local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
    UseCabControlReport3();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

      return;
    }

    // *****
    // If an ERROR has occured,
    // 
    // Close the ERROR Report
    // *****
    if (AsChar(local.Error.Flag) == 'Y')
    {
      // 12/18/98  PDP
      // *****
      // Close the ERROR Report
      // *****
      local.ReportHandlingEabReportSend.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      local.ReportProcessing.Action = "CLOSE";
      local.ReportHandlingEabReportSend.ProgramName = "SWEFB613";
      UseCabErrorReport3();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

        return;
      }

      // *****
      // Roll back all updates.
      // *****
      UseEabRollbackSql2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseSiCloseAdabas();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.ReceivedDate = source.ReceivedDate;
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.TotalCashTransactionCount = source.TotalCashTransactionCount;
    target.TotalDetailAdjustmentCount = source.TotalDetailAdjustmentCount;
    target.CashDue = source.CashDue;
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmount = source.CollectionAmount;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.TotalNonCashTransactionCount = source.TotalNonCashTransactionCount;
    target.TotalNonCashAmt = source.TotalNonCashAmt;
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

  private static void MoveProgramError(ProgramError source, ProgramError target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.KeyInfo = source.KeyInfo;
    target.Code = source.Code;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail =
      local.ReportHandlingEabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.ReportProcessing.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
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

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.ReportHandlingEabReportSend, useImport.NeededToOpen);
      
    useImport.EabFileHandling.Action = local.ReportProcessing.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail =
      local.ReportHandlingEabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.ReportProcessing.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
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

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.ReportHandlingEabReportSend, useImport.NeededToOpen);
      
    useImport.EabFileHandling.Action = local.ReportProcessing.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport4()
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

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseEabRollbackSql1()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    Call(EabRollbackSql.Execute, useImport, useExport);
  }

  private void UseEabRollbackSql2()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(EabRollbackSql.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseEabSdsoInterfaceDrvr1()
  {
    var useImport = new EabSdsoInterfaceDrvr.Import();
    var useExport = new EabSdsoInterfaceDrvr.Export();

    useImport.External.Assign(local.PassArea);
    useExport.External.Assign(local.PassArea);
    useExport.RecordType.Count = local.RecordType.Count;
    useExport.HeaderRecord.Detail.SourceCreationDate =
      local.HeaderRecord.HeaderDetail.SourceCreationDate;
    useExport.DetailRecord.CollectionType.SelectChar =
      local.DetailRecord.DetailDtlCollType.SelectChar;
    MoveCashReceiptDetail(local.DetailRecord.DetailDetail,
      useExport.DetailRecord.Detail);
    MoveCashReceiptEvent(local.TotalRecord.TotalDetail,
      useExport.TotalRecord.DetailTotal);

    Call(EabSdsoInterfaceDrvr.Execute, useImport, useExport);

    local.PassArea.Assign(useImport.External);
    local.PassArea.Assign(useExport.External);
    local.RecordType.Count = useExport.RecordType.Count;
    local.HeaderRecord.HeaderDetail.SourceCreationDate =
      useExport.HeaderRecord.Detail.SourceCreationDate;
    local.DetailRecord.DetailDtlCollType.SelectChar =
      useExport.DetailRecord.CollectionType.SelectChar;
    MoveCashReceiptDetail(useExport.DetailRecord.Detail,
      local.DetailRecord.DetailDetail);
    MoveCashReceiptEvent(useExport.TotalRecord.DetailTotal,
      local.TotalRecord.TotalDetail);
  }

  private void UseEabSdsoInterfaceDrvr2()
  {
    var useImport = new EabSdsoInterfaceDrvr.Import();
    var useExport = new EabSdsoInterfaceDrvr.Export();

    useImport.External.Assign(local.PassArea);
    useExport.External.Assign(local.PassArea);

    Call(EabSdsoInterfaceDrvr.Execute, useImport, useExport);

    local.PassArea.Assign(useImport.External);
    local.PassArea.Assign(useExport.External);
  }

  private void UseFnAbValidateSdsoFile()
  {
    var useImport = new FnAbValidateSdsoFile.Import();
    var useExport = new FnAbValidateSdsoFile.Export();

    Call(FnAbValidateSdsoFile.Execute, useImport, useExport);

    local.SdsoCashReceiptSourceType.Code = useExport.Sdso.Code;
  }

  private void UseFnAddFdsoSdsoHeaderInfo()
  {
    var useImport = new FnAddFdsoSdsoHeaderInfo.Import();
    var useExport = new FnAddFdsoSdsoHeaderInfo.Export();

    useImport.CashReceiptEvent.SourceCreationDate =
      local.SetCashReceiptEvent.SourceCreationDate;
    useImport.CashReceiptSourceType.Code = local.SdsoCashReceiptSourceType.Code;
    useExport.ImportNumberOfReads.Count = local.NumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = local.NumberOfUpdates.Count;

    Call(FnAddFdsoSdsoHeaderInfo.Execute, useImport, useExport);

    local.NumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    local.SdsoCashReceiptSourceType.Code = useExport.CashReceiptSourceType.Code;
    local.SdsoCashReceiptEvent.Assign(useExport.CashReceiptEvent);
    local.NumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
    local.SdsoCashReceipt.SequentialNumber = useExport.NonCash.SequentialNumber;
  }

  private void UseFnFdsoSdsoCheckForDupBatch()
  {
    var useImport = new FnFdsoSdsoCheckForDupBatch.Import();
    var useExport = new FnFdsoSdsoCheckForDupBatch.Export();

    useImport.CashReceipt.Assign(local.FindDuplicate);
    useImport.FindDuplicateDaysCount.Count = local.FindDuplicateDaysCount.Count;

    Call(FnFdsoSdsoCheckForDupBatch.Execute, useImport, useExport);

    MoveCashReceipt(useExport.Found, local.FindDuplicate);
    local.FindDuplicateCount.Count = useExport.DuplicateCount.Count;
  }

  private void UseFnProcessSdsoCollDtlRecord()
  {
    var useImport = new FnProcessSdsoCollDtlRecord.Import();
    var useExport = new FnProcessSdsoCollDtlRecord.Export();

    useImport.SourceCreationCashReceiptEvent.SourceCreationDate =
      local.SetCashReceiptEvent.SourceCreationDate;
    useImport.SourceCreationDateWorkArea.TextDate =
      local.SourceCreation.TextDate;
    useImport.CashReceipt.SequentialNumber =
      local.SdsoCashReceipt.SequentialNumber;
    useImport.CollectionType.SequentialIdentifier =
      local.SetCollectionType.SequentialIdentifier;
    useImport.Header.HeaderDetail.SourceCreationDate =
      local.HeaderRecord.HeaderDetail.SourceCreationDate;
    useImport.Detail.DetailDtlCollType.SelectChar =
      local.DetailRecord.DetailDtlCollType.SelectChar;
    MoveCashReceiptDetail(local.DetailRecord.DetailDetail,
      useImport.Detail.DetailDetail);
    MoveCashReceiptEvent(local.TotalRecord.TotalDetail,
      useImport.Total.TotalDetail);
    useImport.P.Assign(entities.CashReceipt);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      local.SdsoCashReceiptEvent.SystemGeneratedIdentifier;

    useImport.PndCollections.Count = local.PndCollections.Count;
    useImport.RecCollections.Count = local.RecCollections.Count;
    useImport.RefCollections.Count = local.RefCollections.Count;
    useImport.RelCollections.Count = local.RelCollections.Count;
    useImport.SusCollections.Count = local.SusCollections.Count;
    useImport.PndCollectionsAmt.TotalCurrency =
      local.PndCollectionsAmt.TotalCurrency;
    useImport.RecCollectionsAmt.TotalCurrency =
      local.RecCollectionsAmt.TotalCurrency;
    useImport.RefCollectionsAmt.TotalCurrency =
      local.RefCollectionsAmt.TotalCurrency;
    useImport.RelCollectionsAmt.TotalCurrency =
      local.RelCollectionsAmt.TotalCurrency;
    useImport.SusCollectionsAmt.TotalCurrency =
      local.SusCollectionsAmt.TotalCurrency;
    useImport.Save.Date = local.Save.Date;
    useExport.ImportRecordsProcessed.Value = local.RecordsProcessed.Value;
    useExport.ImportRecordsRead.Value = local.RecordsRead.Value;
    useExport.ImportCashReceipt.Assign(local.Total);
    useExport.ImportNextCrdId.SequentialIdentifier =
      local.NextSdsoDetailId.SequentialIdentifier;
    useExport.ImportNumberOfReads.Count = local.NumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = local.NumberOfUpdates.Count;

    Call(FnProcessSdsoCollDtlRecord.Execute, useImport, useExport);

    entities.CashReceipt.SequentialNumber = useImport.P.SequentialNumber;
    local.RecordsProcessed.Value = useExport.ImportRecordsProcessed.Value;
    local.RecordsRead.Value = useExport.ImportRecordsRead.Value;
    local.Total.Assign(useExport.ImportCashReceipt);
    local.NextSdsoDetailId.SequentialIdentifier =
      useExport.ImportNextCrdId.SequentialIdentifier;
    local.NumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    local.NumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
    local.PndCollections.Count = useExport.PndCollections.Count;
    local.RecCollections.Count = useExport.RecCollections.Count;
    local.RefCollections.Count = useExport.RefCollections.Count;
    local.RelCollections.Count = useExport.RelCollections.Count;
    local.SusCollections.Count = useExport.SusCollections.Count;
    local.PndCollectionsAmt.TotalCurrency =
      useExport.PndCollectionsAmt.TotalCurrency;
    local.RecCollectionsAmt.TotalCurrency =
      useExport.RecCollectionsAmt.TotalCurrency;
    local.RefCollectionsAmt.TotalCurrency =
      useExport.RefCollectionsAmt.TotalCurrency;
    local.RelCollectionsAmt.TotalCurrency =
      useExport.RelCollectionsAmt.TotalCurrency;
    local.SusCollectionsAmt.TotalCurrency =
      useExport.SusCollectionsAmt.TotalCurrency;
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

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", local.SdsoCashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 5);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 7);
        entities.CashReceipt.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 8);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 9);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 10);
        entities.CashReceipt.Populated = true;
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
    /// <summary>A TotalRecordGroup group.</summary>
    [Serializable]
    public class TotalRecordGroup
    {
      /// <summary>
      /// A value of TotalDetail.
      /// </summary>
      [JsonPropertyName("totalDetail")]
      public CashReceiptEvent TotalDetail
      {
        get => totalDetail ??= new();
        set => totalDetail = value;
      }

      private CashReceiptEvent totalDetail;
    }

    /// <summary>A DetailRecordGroup group.</summary>
    [Serializable]
    public class DetailRecordGroup
    {
      /// <summary>
      /// A value of DetailDtlCollType.
      /// </summary>
      [JsonPropertyName("detailDtlCollType")]
      public Common DetailDtlCollType
      {
        get => detailDtlCollType ??= new();
        set => detailDtlCollType = value;
      }

      /// <summary>
      /// A value of DetailDetail.
      /// </summary>
      [JsonPropertyName("detailDetail")]
      public CashReceiptDetail DetailDetail
      {
        get => detailDetail ??= new();
        set => detailDetail = value;
      }

      private Common detailDtlCollType;
      private CashReceiptDetail detailDetail;
    }

    /// <summary>A HeaderRecordGroup group.</summary>
    [Serializable]
    public class HeaderRecordGroup
    {
      /// <summary>
      /// A value of HeaderDetail.
      /// </summary>
      [JsonPropertyName("headerDetail")]
      public CashReceiptEvent HeaderDetail
      {
        get => headerDetail ??= new();
        set => headerDetail = value;
      }

      private CashReceiptEvent headerDetail;
    }

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

    /// <summary>A CollectionRecordGroup group.</summary>
    [Serializable]
    public class CollectionRecordGroup
    {
      /// <summary>
      /// A value of DetailLocalCode.
      /// </summary>
      [JsonPropertyName("detailLocalCode")]
      public TextWorkArea DetailLocalCode
      {
        get => detailLocalCode ??= new();
        set => detailLocalCode = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CashReceiptDetail Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailAdjustmentAmt.
      /// </summary>
      [JsonPropertyName("detailAdjustmentAmt")]
      public Common DetailAdjustmentAmt
      {
        get => detailAdjustmentAmt ??= new();
        set => detailAdjustmentAmt = value;
      }

      private TextWorkArea detailLocalCode;
      private CashReceiptDetail detail;
      private Common detailAdjustmentAmt;
    }

    /// <summary>A ControlTotalsGroup group.</summary>
    [Serializable]
    public class ControlTotalsGroup
    {
      /// <summary>
      /// A value of ProgramControlTotal.
      /// </summary>
      [JsonPropertyName("programControlTotal")]
      public ProgramControlTotal ProgramControlTotal
      {
        get => programControlTotal ??= new();
        set => programControlTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private ProgramControlTotal programControlTotal;
    }

    /// <summary>
    /// A value of SaveDaysForDuplicate.
    /// </summary>
    [JsonPropertyName("saveDaysForDuplicate")]
    public TextWorkArea SaveDaysForDuplicate
    {
      get => saveDaysForDuplicate ??= new();
      set => saveDaysForDuplicate = value;
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
    /// A value of InvalidDate.
    /// </summary>
    [JsonPropertyName("invalidDate")]
    public Common InvalidDate
    {
      get => invalidDate ??= new();
      set => invalidDate = value;
    }

    /// <summary>
    /// A value of Type2R.
    /// </summary>
    [JsonPropertyName("type2R")]
    public CashReceiptEvent Type2R
    {
      get => type2R ??= new();
      set => type2R = value;
    }

    /// <summary>
    /// A value of Type2S.
    /// </summary>
    [JsonPropertyName("type2S")]
    public CashReceiptEvent Type2S
    {
      get => type2S ??= new();
      set => type2S = value;
    }

    /// <summary>
    /// A value of Type2U.
    /// </summary>
    [JsonPropertyName("type2U")]
    public CashReceiptEvent Type2U
    {
      get => type2U ??= new();
      set => type2U = value;
    }

    /// <summary>
    /// A value of Type2K.
    /// </summary>
    [JsonPropertyName("type2K")]
    public CashReceiptEvent Type2K
    {
      get => type2K ??= new();
      set => type2K = value;
    }

    /// <summary>
    /// A value of Type2BadCollection.
    /// </summary>
    [JsonPropertyName("type2BadCollection")]
    public CashReceiptEvent Type2BadCollection
    {
      get => type2BadCollection ??= new();
      set => type2BadCollection = value;
    }

    /// <summary>
    /// A value of TotalAccumulator.
    /// </summary>
    [JsonPropertyName("totalAccumulator")]
    public CashReceiptEvent TotalAccumulator
    {
      get => totalAccumulator ??= new();
      set => totalAccumulator = value;
    }

    /// <summary>
    /// A value of Type3Total.
    /// </summary>
    [JsonPropertyName("type3Total")]
    public CashReceiptEvent Type3Total
    {
      get => type3Total ??= new();
      set => type3Total = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of InterfaceErrorSwitch.
    /// </summary>
    [JsonPropertyName("interfaceErrorSwitch")]
    public Common InterfaceErrorSwitch
    {
      get => interfaceErrorSwitch ??= new();
      set => interfaceErrorSwitch = value;
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
    /// A value of ReportHandlingEabReportReturn.
    /// </summary>
    [JsonPropertyName("reportHandlingEabReportReturn")]
    public EabReportReturn ReportHandlingEabReportReturn
    {
      get => reportHandlingEabReportReturn ??= new();
      set => reportHandlingEabReportReturn = value;
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
    /// A value of TbdLocalSdsorCashReceipt.
    /// </summary>
    [JsonPropertyName("tbdLocalSdsorCashReceipt")]
    public CashReceipt TbdLocalSdsorCashReceipt
    {
      get => tbdLocalSdsorCashReceipt ??= new();
      set => tbdLocalSdsorCashReceipt = value;
    }

    /// <summary>
    /// A value of TbdLocalSdsorCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("tbdLocalSdsorCashReceiptEvent")]
    public CashReceiptEvent TbdLocalSdsorCashReceiptEvent
    {
      get => tbdLocalSdsorCashReceiptEvent ??= new();
      set => tbdLocalSdsorCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public ProgramControlTotal RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of RecordsProcessed.
    /// </summary>
    [JsonPropertyName("recordsProcessed")]
    public ProgramControlTotal RecordsProcessed
    {
      get => recordsProcessed ??= new();
      set => recordsProcessed = value;
    }

    /// <summary>
    /// A value of FirstPass.
    /// </summary>
    [JsonPropertyName("firstPass")]
    public Common FirstPass
    {
      get => firstPass ??= new();
      set => firstPass = value;
    }

    /// <summary>
    /// A value of NumberOfUpdates.
    /// </summary>
    [JsonPropertyName("numberOfUpdates")]
    public Common NumberOfUpdates
    {
      get => numberOfUpdates ??= new();
      set => numberOfUpdates = value;
    }

    /// <summary>
    /// A value of NumberOfReads.
    /// </summary>
    [JsonPropertyName("numberOfReads")]
    public Common NumberOfReads
    {
      get => numberOfReads ??= new();
      set => numberOfReads = value;
    }

    /// <summary>
    /// A value of SetCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("setCashReceiptEvent")]
    public CashReceiptEvent SetCashReceiptEvent
    {
      get => setCashReceiptEvent ??= new();
      set => setCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public CashReceipt Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of NextUiCrdId.
    /// </summary>
    [JsonPropertyName("nextUiCrdId")]
    public CashReceiptDetail NextUiCrdId
    {
      get => nextUiCrdId ??= new();
      set => nextUiCrdId = value;
    }

    /// <summary>
    /// A value of NextKpersCrdId.
    /// </summary>
    [JsonPropertyName("nextKpersCrdId")]
    public CashReceiptDetail NextKpersCrdId
    {
      get => nextKpersCrdId ??= new();
      set => nextKpersCrdId = value;
    }

    /// <summary>
    /// A value of SdsoCashReceipt.
    /// </summary>
    [JsonPropertyName("sdsoCashReceipt")]
    public CashReceipt SdsoCashReceipt
    {
      get => sdsoCashReceipt ??= new();
      set => sdsoCashReceipt = value;
    }

    /// <summary>
    /// A value of SdsoCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("sdsoCashReceiptSourceType")]
    public CashReceiptSourceType SdsoCashReceiptSourceType
    {
      get => sdsoCashReceiptSourceType ??= new();
      set => sdsoCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of SdsoCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("sdsoCashReceiptEvent")]
    public CashReceiptEvent SdsoCashReceiptEvent
    {
      get => sdsoCashReceiptEvent ??= new();
      set => sdsoCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of TbdLocalKpersCashReceipt.
    /// </summary>
    [JsonPropertyName("tbdLocalKpersCashReceipt")]
    public CashReceipt TbdLocalKpersCashReceipt
    {
      get => tbdLocalKpersCashReceipt ??= new();
      set => tbdLocalKpersCashReceipt = value;
    }

    /// <summary>
    /// A value of TbdLocalKpersCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("tbdLocalKpersCashReceiptEvent")]
    public CashReceiptEvent TbdLocalKpersCashReceiptEvent
    {
      get => tbdLocalKpersCashReceiptEvent ??= new();
      set => tbdLocalKpersCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of TbdLocalUiCashReceipt.
    /// </summary>
    [JsonPropertyName("tbdLocalUiCashReceipt")]
    public CashReceipt TbdLocalUiCashReceipt
    {
      get => tbdLocalUiCashReceipt ??= new();
      set => tbdLocalUiCashReceipt = value;
    }

    /// <summary>
    /// A value of TbdLocalUiCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("tbdLocalUiCashReceiptEvent")]
    public CashReceiptEvent TbdLocalUiCashReceiptEvent
    {
      get => tbdLocalUiCashReceiptEvent ??= new();
      set => tbdLocalUiCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of SetCollectionType.
    /// </summary>
    [JsonPropertyName("setCollectionType")]
    public CollectionType SetCollectionType
    {
      get => setCollectionType ??= new();
      set => setCollectionType = value;
    }

    /// <summary>
    /// A value of NextSdsoDetailId.
    /// </summary>
    [JsonPropertyName("nextSdsoDetailId")]
    public CashReceiptDetail NextSdsoDetailId
    {
      get => nextSdsoDetailId ??= new();
      set => nextSdsoDetailId = value;
    }

    /// <summary>
    /// Gets a value of TotalRecord.
    /// </summary>
    [JsonPropertyName("totalRecord")]
    public TotalRecordGroup TotalRecord
    {
      get => totalRecord ?? (totalRecord = new());
      set => totalRecord = value;
    }

    /// <summary>
    /// Gets a value of DetailRecord.
    /// </summary>
    [JsonPropertyName("detailRecord")]
    public DetailRecordGroup DetailRecord
    {
      get => detailRecord ?? (detailRecord = new());
      set => detailRecord = value;
    }

    /// <summary>
    /// Gets a value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public HeaderRecordGroup HeaderRecord
    {
      get => headerRecord ?? (headerRecord = new());
      set => headerRecord = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of Interfund.
    /// </summary>
    [JsonPropertyName("interfund")]
    public CashReceiptType Interfund
    {
      get => interfund ??= new();
      set => interfund = value;
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
    /// A value of HardcodedInterfund.
    /// </summary>
    [JsonPropertyName("hardcodedInterfund")]
    public CashReceiptType HardcodedInterfund
    {
      get => hardcodedInterfund ??= new();
      set => hardcodedInterfund = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public ProgramError Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of SourceCreation.
    /// </summary>
    [JsonPropertyName("sourceCreation")]
    public DateWorkArea SourceCreation
    {
      get => sourceCreation ??= new();
      set => sourceCreation = value;
    }

    /// <summary>
    /// A value of SetCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("setCashReceiptSourceType")]
    public CashReceiptSourceType SetCashReceiptSourceType
    {
      get => setCashReceiptSourceType ??= new();
      set => setCashReceiptSourceType = value;
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
    /// Gets a value of CollectionRecord.
    /// </summary>
    [JsonPropertyName("collectionRecord")]
    public CollectionRecordGroup CollectionRecord
    {
      get => collectionRecord ?? (collectionRecord = new());
      set => collectionRecord = value;
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
    /// Gets a value of ControlTotals.
    /// </summary>
    [JsonIgnore]
    public Array<ControlTotalsGroup> ControlTotals => controlTotals ??= new(
      ControlTotalsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ControlTotals for json serialization.
    /// </summary>
    [JsonPropertyName("controlTotals")]
    [Computed]
    public IList<ControlTotalsGroup> ControlTotals_Json
    {
      get => controlTotals;
      set => ControlTotals.Assign(value);
    }

    /// <summary>
    /// A value of RecCollections.
    /// </summary>
    [JsonPropertyName("recCollections")]
    public Common RecCollections
    {
      get => recCollections ??= new();
      set => recCollections = value;
    }

    /// <summary>
    /// A value of RelCollections.
    /// </summary>
    [JsonPropertyName("relCollections")]
    public Common RelCollections
    {
      get => relCollections ??= new();
      set => relCollections = value;
    }

    /// <summary>
    /// A value of SusCollections.
    /// </summary>
    [JsonPropertyName("susCollections")]
    public Common SusCollections
    {
      get => susCollections ??= new();
      set => susCollections = value;
    }

    /// <summary>
    /// A value of RefCollections.
    /// </summary>
    [JsonPropertyName("refCollections")]
    public Common RefCollections
    {
      get => refCollections ??= new();
      set => refCollections = value;
    }

    /// <summary>
    /// A value of PndCollections.
    /// </summary>
    [JsonPropertyName("pndCollections")]
    public Common PndCollections
    {
      get => pndCollections ??= new();
      set => pndCollections = value;
    }

    /// <summary>
    /// A value of RecCollectionsAmt.
    /// </summary>
    [JsonPropertyName("recCollectionsAmt")]
    public Common RecCollectionsAmt
    {
      get => recCollectionsAmt ??= new();
      set => recCollectionsAmt = value;
    }

    /// <summary>
    /// A value of RelCollectionsAmt.
    /// </summary>
    [JsonPropertyName("relCollectionsAmt")]
    public Common RelCollectionsAmt
    {
      get => relCollectionsAmt ??= new();
      set => relCollectionsAmt = value;
    }

    /// <summary>
    /// A value of SusCollectionsAmt.
    /// </summary>
    [JsonPropertyName("susCollectionsAmt")]
    public Common SusCollectionsAmt
    {
      get => susCollectionsAmt ??= new();
      set => susCollectionsAmt = value;
    }

    /// <summary>
    /// A value of RefCollectionsAmt.
    /// </summary>
    [JsonPropertyName("refCollectionsAmt")]
    public Common RefCollectionsAmt
    {
      get => refCollectionsAmt ??= new();
      set => refCollectionsAmt = value;
    }

    /// <summary>
    /// A value of PndCollectionsAmt.
    /// </summary>
    [JsonPropertyName("pndCollectionsAmt")]
    public Common PndCollectionsAmt
    {
      get => pndCollectionsAmt ??= new();
      set => pndCollectionsAmt = value;
    }

    /// <summary>
    /// A value of TbdSwefb612AdjustErrCount.
    /// </summary>
    [JsonPropertyName("tbdSwefb612AdjustErrCount")]
    public Common TbdSwefb612AdjustErrCount
    {
      get => tbdSwefb612AdjustErrCount ??= new();
      set => tbdSwefb612AdjustErrCount = value;
    }

    /// <summary>
    /// A value of TbdSwefb612AdjustmentCount.
    /// </summary>
    [JsonPropertyName("tbdSwefb612AdjustmentCount")]
    public Common TbdSwefb612AdjustmentCount
    {
      get => tbdSwefb612AdjustmentCount ??= new();
      set => tbdSwefb612AdjustmentCount = value;
    }

    /// <summary>
    /// A value of TotalOfAll.
    /// </summary>
    [JsonPropertyName("totalOfAll")]
    public CashReceiptDetail TotalOfAll
    {
      get => totalOfAll ??= new();
      set => totalOfAll = value;
    }

    /// <summary>
    /// A value of TbdSwefb612AdjErrAmount.
    /// </summary>
    [JsonPropertyName("tbdSwefb612AdjErrAmount")]
    public Common TbdSwefb612AdjErrAmount
    {
      get => tbdSwefb612AdjErrAmount ??= new();
      set => tbdSwefb612AdjErrAmount = value;
    }

    /// <summary>
    /// A value of TbdSwefb612AdjustmentAmt.
    /// </summary>
    [JsonPropertyName("tbdSwefb612AdjustmentAmt")]
    public Common TbdSwefb612AdjustmentAmt
    {
      get => tbdSwefb612AdjustmentAmt ??= new();
      set => tbdSwefb612AdjustmentAmt = value;
    }

    /// <summary>
    /// A value of TbdSwefb612CollectionAmt.
    /// </summary>
    [JsonPropertyName("tbdSwefb612CollectionAmt")]
    public Common TbdSwefb612CollectionAmt
    {
      get => tbdSwefb612CollectionAmt ??= new();
      set => tbdSwefb612CollectionAmt = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of FindDuplicate.
    /// </summary>
    [JsonPropertyName("findDuplicate")]
    public CashReceipt FindDuplicate
    {
      get => findDuplicate ??= new();
      set => findDuplicate = value;
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
    /// A value of TbdLocalAbendErrorFound.
    /// </summary>
    [JsonPropertyName("tbdLocalAbendErrorFound")]
    public Common TbdLocalAbendErrorFound
    {
      get => tbdLocalAbendErrorFound ??= new();
      set => tbdLocalAbendErrorFound = value;
    }

    /// <summary>
    /// A value of FindDuplicateCount.
    /// </summary>
    [JsonPropertyName("findDuplicateCount")]
    public Common FindDuplicateCount
    {
      get => findDuplicateCount ??= new();
      set => findDuplicateCount = value;
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

    private TextWorkArea saveDaysForDuplicate;
    private DateWorkArea save;
    private Common invalidDate;
    private CashReceiptEvent type2R;
    private CashReceiptEvent type2S;
    private CashReceiptEvent type2U;
    private CashReceiptEvent type2K;
    private CashReceiptEvent type2BadCollection;
    private CashReceiptEvent totalAccumulator;
    private CashReceiptEvent type3Total;
    private Common error;
    private Common interfaceErrorSwitch;
    private EabReportSend reportHandlingEabReportSend;
    private EabReportReturn reportHandlingEabReportReturn;
    private EabFileHandling reportProcessing;
    private CashReceipt tbdLocalSdsorCashReceipt;
    private CashReceiptEvent tbdLocalSdsorCashReceiptEvent;
    private ProgramControlTotal recordsRead;
    private ProgramControlTotal recordsProcessed;
    private Common firstPass;
    private Common numberOfUpdates;
    private Common numberOfReads;
    private CashReceiptEvent setCashReceiptEvent;
    private CashReceipt total;
    private CashReceiptDetail nextUiCrdId;
    private CashReceiptDetail nextKpersCrdId;
    private CashReceipt sdsoCashReceipt;
    private CashReceiptSourceType sdsoCashReceiptSourceType;
    private CashReceiptEvent sdsoCashReceiptEvent;
    private CashReceipt tbdLocalKpersCashReceipt;
    private CashReceiptEvent tbdLocalKpersCashReceiptEvent;
    private CashReceipt tbdLocalUiCashReceipt;
    private CashReceiptEvent tbdLocalUiCashReceiptEvent;
    private CollectionType setCollectionType;
    private CashReceiptDetail nextSdsoDetailId;
    private TotalRecordGroup totalRecord;
    private DetailRecordGroup detailRecord;
    private HeaderRecordGroup headerRecord;
    private Common recordType;
    private CashReceiptType interfund;
    private External hardcodeOpen;
    private External hardcodeRead;
    private External hardcodePosition;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External passArea;
    private CashReceiptType hardcodedInterfund;
    private Common common;
    private ProgramError initialized;
    private Array<AaaGroupLocalErrorsGroup> aaaGroupLocalErrors;
    private DateWorkArea sourceCreation;
    private CashReceiptSourceType setCashReceiptSourceType;
    private ExitStateWorkArea exitStateWorkArea;
    private CollectionRecordGroup collectionRecord;
    private EabFileHandling reportHandlingEabFileHandling;
    private Array<ControlTotalsGroup> controlTotals;
    private Common recCollections;
    private Common relCollections;
    private Common susCollections;
    private Common refCollections;
    private Common pndCollections;
    private Common recCollectionsAmt;
    private Common relCollectionsAmt;
    private Common susCollectionsAmt;
    private Common refCollectionsAmt;
    private Common pndCollectionsAmt;
    private Common tbdSwefb612AdjustErrCount;
    private Common tbdSwefb612AdjustmentCount;
    private CashReceiptDetail totalOfAll;
    private Common tbdSwefb612AdjErrAmount;
    private Common tbdSwefb612AdjustmentAmt;
    private Common tbdSwefb612CollectionAmt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt findDuplicate;
    private Common errorOpened;
    private Common tbdLocalAbendErrorFound;
    private Common findDuplicateCount;
    private Common findDuplicateDaysCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Tbd.
    /// </summary>
    [JsonPropertyName("tbd")]
    public ProgramRun Tbd
    {
      get => tbd ??= new();
      set => tbd = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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

    private ProgramRun tbd;
    private CashReceipt cashReceipt;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
